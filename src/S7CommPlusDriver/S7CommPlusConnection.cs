#region License
/******************************************************************************
 * S7CommPlusDriver
 * 
 * Copyright (C) 2023 Thomas Wiens, th.wiens@gmx.de
 *
 * This file is part of S7CommPlusDriver.
 *
 * S7CommPlusDriver is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 /****************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Linq;
using System.Diagnostics;
using S7CommPlusDriver.ClientApi;
using System.Text.RegularExpressions;
using S7CommPlusDriver.Core;
using System.Security.Cryptography;

namespace S7CommPlusDriver
{
    public partial class S7CommPlusConnection
    {
        #region Private Members
        private S7Client m_client;
        private MemoryStream m_ReceivedPDU;
        private MemoryStream m_ReceivedTempPDU;
        private Queue<MemoryStream> m_ReceivedPDUs = new Queue<MemoryStream>();
        private Mutex m_Mutex = new Mutex();

        private bool m_ReceivedNeedMoreDataForCompletePDU;
        private bool m_NewS7CommPlusReceived;
        private UInt32 m_SessionId;
        private UInt32 m_SessionId2;
        public UInt32 SessionId2
        {
            get { return m_SessionId2; }
            private set { m_SessionId2 = value; }
        }

        private int m_ReadTimeout = 5000;
        private UInt16 m_SequenceNumber = 0;
        private UInt32 m_IntegrityId = 0;
        private UInt32 m_IntegrityId_Set = 0;
        private CommRessources m_CommRessources = new CommRessources();

        private List<DatablockInfo> dbInfoList;
        private List<PObject> typeInfoList = new List<PObject>();
        #endregion

        #region Public Members
        public int m_LastError = 0;

        #endregion

        #region Private Methods

        private UInt16 GetNextSequenceNumber()
        {
            if (m_SequenceNumber == UInt16.MaxValue)
            {
                m_SequenceNumber = 1;
            }
            else
            {
                m_SequenceNumber++;
            }
            return m_SequenceNumber;
        }

        // We must count the IntegrityId for different functions of the protocol.
        // As a first guess functions for setting variables need separate counters.
        // Use the functioncode to differ between the which sequence/integrity counter values.
        private UInt32 GetNextIntegrityId(ushort functioncode)
        {
            UInt32 ret;
            switch (functioncode)
            {
                case Functioncode.SetMultiVariables:
                case Functioncode.SetVariable:
                case Functioncode.SetVarSubStreamed:
                case Functioncode.DeleteObject:
                case Functioncode.CreateObject:
                    if (m_IntegrityId_Set == UInt32.MaxValue)
                    {
                        m_IntegrityId_Set = 0;
                    }
                    else
                    {
                        m_IntegrityId_Set++;
                    }
                    ret = m_IntegrityId_Set;
                    break;
                default:
                    if (m_IntegrityId == UInt32.MaxValue)
                    {
                        m_IntegrityId = 0;
                    }
                    else
                    {
                        m_IntegrityId++;
                    }
                    ret = m_IntegrityId;
                    break;
            }
            return ret;
        }

        private void WaitForNewS7plusReceived(int Timeout)
        {
            bool Expired = false;
            int Elapsed = Environment.TickCount;
            bool done = false;

            m_Mutex.WaitOne();
            if (m_ReceivedPDUs.Count > 0)
            {
                m_ReceivedPDU = m_ReceivedPDUs.Dequeue();
                done = true;
            }
            m_Mutex.ReleaseMutex();

            while (!done && !Expired)
            {
                Thread.Sleep(2);
                Expired = Environment.TickCount - Elapsed > Timeout;
                m_Mutex.WaitOne();
                if (m_ReceivedPDUs.Count > 0)
                {
                    m_ReceivedPDU = m_ReceivedPDUs.Dequeue();
                    done = true;
                }
                m_Mutex.ReleaseMutex();
            }

            if (Expired)
            {
                Console.WriteLine("S7CommPlusConnection - WaitForNewS7plusReceived: ERROR: Timeout!");
                m_LastError = S7Consts.errTCPDataReceive;
            }
        }

        private int SendS7plusFunctionObject(IS7pRequest funcObj)
        {
            // If we don't have a SessionId, this must be the first CreateObjectRequest, where we use the Id for NullServerSession
            if (m_SessionId == 0)
            {
                funcObj.SessionId = Ids.ObjectNullServerSession;
            }
            else
            {
                funcObj.SessionId = m_SessionId;
            }

            // Insert SequenceNumber and IntegrityId, if neccessary for object type and state of communication
            funcObj.SequenceNumber = GetNextSequenceNumber();
            if (funcObj.WithIntegrityId)
            {
                funcObj.IntegrityId = GetNextIntegrityId(funcObj.FunctionCode);
            }

            MemoryStream stream = new MemoryStream();
            funcObj.Serialize(stream);
            return SendS7plusPDUdata(stream.ToArray(), (int)stream.Length, funcObj.ProtocolVersion);
        }

        private int SendS7plusPDUdata(byte[] sendPduData, int bytesToSend, byte protoVersion)
        {
            m_LastError = 0;

            int curSize;
            int sourcePos = 0;
            int sendLen;
            int NegotiatedIsoPduSize = 1024;// TODO: Respect the negotiated TPDU size

            // 4 Byte TPKT Header
            // 3 Byte ISO-Header
            // 5 Byte TLS Header + 17 Bytes addition from TLS
            // 4 Byte S7CommPlus Header
            // 4 Byte S7CommPlus Trailer (must fit into last PDU)
            int MaxSize = NegotiatedIsoPduSize - 4 - 3 - 5 - 17 - 4 - 4;
            byte[] packet = new byte[MaxSize + 4]; //max packet size is always MaxSize + PDU Header

            while (bytesToSend > 0)
            {
                if (bytesToSend > MaxSize)
                {
                    curSize = MaxSize;
                    bytesToSend -= MaxSize;
                }
                else
                {
                    curSize = bytesToSend;
                    bytesToSend -= curSize;
                }
                // Header
                packet[0] = 0x72;
                packet[1] = protoVersion;
                packet[2] = (byte)(curSize >> 8);
                packet[3] = (byte)(curSize & 0x00FF);
                // Data part
                Array.Copy(sendPduData, sourcePos, packet, 4, curSize);
                sourcePos += curSize;
                sendLen = 4 + curSize;

                // Trailer only in last packet
                if (bytesToSend == 0)
                {
                    Array.Resize(ref packet, sendLen + 4); //resize only the last package to sendLen + TrailerLen
                    packet[sendLen] = 0x72;
                    sendLen++;
                    packet[sendLen] = protoVersion;
                    sendLen++;
                    packet[sendLen] = 0;
                    sendLen++;
                    packet[sendLen] = 0;
                    sendLen++;
                }
                m_client.Send(packet);
            }
            return m_LastError;
        }

        private void OnDataReceived(byte[] PDU, int len)
        {
            // In this method, we've got always a complete TPDU (from protocol layer above) without fragmentation
            // At this point, we can detect if we receive a fragmented S7CommPlus PDU.
            // If not fragmented, then TPKT.Length - 15 is equal of the length in S7CommPlus.Header.
            // 15 bytes because: 4 Bytes TPKT.Header.len + 3 Bytes ISO.Header.Len + 4 Bytes S7CommPlus.Header.len + 4 Bytes S7CommPlus.trailer.Len.
            // Since the pure userdata of the TPDU comes in here, that is only minus 4 bytes header + 4 bytes trailer.
            // 
            // Special handling for SystemEvents with ProtocolVersion = 0xfe:
            // Here's only a header.
            // Because of this, the first byte for the ProtocolVersion must be written in then stream at first.
            // The datalength must not be written into the stream, because it's not valid on fragmented PDUs
            // for the complete length, only for the single fragment.

            // This method is called from a different thread.
            // If we use subscriptions or alarming, we may get new data before the last PDU was processed completely.
            // First step we push the complete PDU to a queue.
            // TODO: m_LastError handling would also not work as expected. This needs some more redesign.

            if (!m_ReceivedNeedMoreDataForCompletePDU)
            {
                m_ReceivedTempPDU = new MemoryStream();
            }
            // S7comm-plus
            byte protoVersion;
            int pos = 0;
            int s7HeaderDataLen = 0;
            // Check header
            if (PDU[pos] != 0x72)
            {
                m_ReceivedNeedMoreDataForCompletePDU = false;
                m_LastError = S7Consts.errIsoInvalidPDU;
                return;
            }
            pos++;
            protoVersion = PDU[pos];
            if (protoVersion != ProtocolVersion.V1 && protoVersion != ProtocolVersion.V2 && protoVersion != ProtocolVersion.V3 && protoVersion != ProtocolVersion.SystemEvent)
            {
                m_ReceivedNeedMoreDataForCompletePDU = false;
                m_LastError = S7Consts.errIsoInvalidPDU;
                return;
            }
            // For the first fragment, write the ProtocolVersion into the stream in advance
            if (!m_ReceivedNeedMoreDataForCompletePDU)
            {
                m_ReceivedTempPDU.Write(PDU, pos, 1);
            }
            pos++;

            // Read the length of the data-part from header
            s7HeaderDataLen = GetWordAt(PDU, pos);
            pos += 2;
            if (s7HeaderDataLen > 0)
            {
                // Special handling for SystemEvent 0xfe PDUs:
                // This only confirms a few data, but also reports major protocol errors (e.g. incorrect sequence numbers).
                // The confirms can be discarded (for now), but the errors are relevant, because a connection termination is neccessary.
                // As we don't have a trailer on this types, it's not possible that they are transmitted as fragments.
                if (protoVersion == ProtocolVersion.SystemEvent)
                {
                    Console.WriteLine("S7CommPlusConnection - OnDataReceived: ProtocolVersion 0xfe SystemEvent received");
                    m_ReceivedTempPDU.Write(PDU, pos, s7HeaderDataLen);
                    pos += s7HeaderDataLen;
                    // Create SystemEventObject
                    m_ReceivedNeedMoreDataForCompletePDU = false;
                    m_ReceivedTempPDU.Position = 0;
                    m_NewS7CommPlusReceived = false;

                    var sysevt = SystemEvent.DeserializeFromPdu(m_ReceivedTempPDU);
                    if (sysevt.IsFatalError())
                    {
                        Console.WriteLine("S7CommPlusConnection - OnDataReceived: SystemEvent has fatal error");
                        // Termination neccessary
                        m_LastError = S7Consts.errIsoInvalidPDU;
                    }
                    else
                    {
                        Console.WriteLine("S7CommPlusConnection - OnDataReceived: SystemEvent with non fatal error, do nothing");
                    }
                }
                else
                {
                    // Copy data part to destination stream
                    m_ReceivedTempPDU.Write(PDU, pos, s7HeaderDataLen);
                    pos += s7HeaderDataLen;
                    // If this is a fragmented PDU, then at this point no trailer
                    if ((len - 4 - 4) == s7HeaderDataLen)
                    {
                        m_ReceivedNeedMoreDataForCompletePDU = false;
                        m_ReceivedTempPDU.Position = 0;    // Set position back to zero, ready for readout
                        m_NewS7CommPlusReceived = true;
                    }
                    else
                    {
                        m_ReceivedNeedMoreDataForCompletePDU = true;
                    }
                }
            }

            // If a complete (usable) PDU is received, add to the queue (threadsafe) for readout
            if (m_NewS7CommPlusReceived)
            {
                // Push complete PDU to the queue
                m_Mutex.WaitOne();
                m_ReceivedPDUs.Enqueue(m_ReceivedTempPDU);
                m_Mutex.ReleaseMutex();
                m_NewS7CommPlusReceived = false;
            }
        }

        private UInt16 GetWordAt(byte[] Buffer, int Pos)
        {
            return (UInt16)((Buffer[Pos] << 8) | Buffer[Pos + 1]);
        }

        private void SetWordAt(byte[] Buffer, int Pos, UInt16 Value)
        {
            Buffer[Pos] = (byte)(Value >> 8);
            Buffer[Pos + 1] = (byte)(Value & 0x00FF);
        }

        private void printBuf(byte[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                Console.Write("0x" + String.Format("{0:X02} ", b[i]));
            }
            Console.Write(Environment.NewLine);
        }

        private int checkResponseWithIntegrity(IS7pRequest request, IS7pResponse response)
        {
            if (response == null)
            {
                Console.WriteLine("checkResponseWithIntegrity: ERROR! response == null");
                return S7Consts.errIsoInvalidPDU;
            }
            if (request.SequenceNumber != response.SequenceNumber)
            {
                Console.WriteLine(String.Format("checkResponseWithIntegrity: ERROR! SeqenceNumber of Response ({0}) doesn't match Request ({1})", response.SequenceNumber, request.SequenceNumber));
                return S7Consts.errIsoInvalidPDU;
            }
            // Overflow is possible and allowed
            UInt32 reqIntegCheck = (UInt32)request.SequenceNumber + request.IntegrityId;
            if (response.IntegrityId != reqIntegCheck)
            {
                Console.WriteLine(String.Format("checkResponseWithIntegrity: ERROR! IntegrityId of the Response ({0}) doesn't match Request ({1})", response.IntegrityId, reqIntegCheck));
                // Don't return this as error so far
            }
            return 0;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Establishes a connection to the PLC.
        /// </summary>
        /// <param name="address">PLC IP address</param>
        /// <param name="password">PLC password (if set)</param>
        /// <param name="timeoutMs">read timeout in milliseconds (default: 5000 ms)</param>
        /// <returns></returns>
        public int Connect(string address, string password = "", int timeoutMs = 5000)
        {
            if (timeoutMs > 0) {
                m_ReadTimeout = timeoutMs;
            }

            m_LastError = 0;
            int res;
            int Elapsed = Environment.TickCount;
            m_client = new S7Client();
            m_client.OnDataReceived = this.OnDataReceived;

            m_client.SetConnectionParams(address, 0x0600, Encoding.ASCII.GetBytes("SIMATIC-ROOT-HMI"));
            res = m_client.Connect();
            if (res != 0)
                return res;

            #region Step 1: Unencrypted InitSSL Request / Response

            InitSslRequest sslReq = new InitSslRequest(ProtocolVersion.V1, 0 , 0);
            res = SendS7plusFunctionObject(sslReq);
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                m_client.Disconnect();
                return m_LastError;
            }
            InitSslResponse sslRes;
            sslRes = InitSslResponse.DeserializeFromPdu(m_ReceivedPDU);
            if (sslRes == null)
            {
                Console.WriteLine("S7CommPlusConnection - Connect: InitSslResponse with Error!");
                m_client.Disconnect();
                return m_LastError;
            }

            #endregion

            #region Step 2: Activate TLS. Everything from here onwards is TLS encrypted.

            res = m_client.SslActivate();
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
            }

            #endregion

            #region Step 3: CreateObjectRequest / Response (with TLS)

            var createObjReq = new CreateObjectRequest(ProtocolVersion.V1, 0, false);
            createObjReq.SetNullServerSessionData();
            res = SendS7plusFunctionObject(createObjReq);
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                m_client.Disconnect();
                return m_LastError;
            }

            var createObjRes = CreateObjectResponse.DeserializeFromPdu(m_ReceivedPDU);
            if (createObjRes == null)
            {
                Console.WriteLine("S7CommPlusConnection - Connect: CreateObjectResponse with Error!");
                m_client.Disconnect();
                return S7Consts.errIsoInvalidPDU;
            }
            // There are (always?) at least two IDs in the response.
            // Usually the first is used for polling data, and the 2nd for jobs which use notifications, e.g. alarming, subscriptions.
            m_SessionId = createObjRes.ObjectIds[0];
            m_SessionId2 = createObjRes.ObjectIds[1];
            Console.WriteLine("S7CommPlusConnection - Connect: Using SessionId=0x" + String.Format("{0:X04}", m_SessionId));

            // Evaluate Struct 314
            PValue sval = createObjRes.ResponseObject.GetAttribute(Ids.ServerSessionVersion);
            ValueStruct serverSession = (ValueStruct)sval;

            #endregion

            #region Step 4: SetMultiVariablesRequest / Response

            var setMultiVarReq = new SetMultiVariablesRequest(ProtocolVersion.V2);
            setMultiVarReq.SetSessionSetupData(m_SessionId, serverSession);
            res = SendS7plusFunctionObject(setMultiVarReq);
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                m_client.Disconnect();
                return m_LastError;
            }

            var setMultiVarRes = SetMultiVariablesResponse.DeserializeFromPdu(m_ReceivedPDU);
            if (setMultiVarRes == null)
            {
                Console.WriteLine("S7CommPlusConnection - Connect: SetMultiVariablesResponse with Error!");
                m_client.Disconnect();
                return S7Consts.errIsoInvalidPDU;
            }

            #endregion

            #region Step 5: Read SystemLimits
            res = m_CommRessources.ReadMax(this);
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
            }
            #endregion

            #region Step 6: Password
            // Get current protection level
            var getVarSubstreamedReq = new GetVarSubstreamedRequest(ProtocolVersion.V2);
            getVarSubstreamedReq.InObjectId = m_SessionId;
            getVarSubstreamedReq.SessionId = m_SessionId;
            getVarSubstreamedReq.Address = Ids.EffectiveProtectionLevel;
            res = SendS7plusFunctionObject(getVarSubstreamedReq);
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                m_client.Disconnect();
                return m_LastError;
            }

            var getVarSubstreamedRes = GetVarSubstreamedResponse.DeserializeFromPdu(m_ReceivedPDU);
            if (getVarSubstreamedRes == null)
            {
                Console.WriteLine("S7CommPlusConnection - Connect.Password: GetVarSubstreamedResponse with Error!");
                m_client.Disconnect();
                return S7Consts.errIsoInvalidPDU;
            }

            // Check access level
            UInt32 accessLevel = (getVarSubstreamedRes.Value as ValueUDInt).GetValue();
            if (accessLevel > AccessLevel.FullAccess && password != "")
            {
                // Get challenge
                var getVarSubstreamedReq_challange = new GetVarSubstreamedRequest(ProtocolVersion.V2);
                getVarSubstreamedReq_challange.InObjectId = m_SessionId;
                getVarSubstreamedReq_challange.SessionId = m_SessionId;
                getVarSubstreamedReq_challange.Address = Ids.ServerSessionRequest;
                res = SendS7plusFunctionObject(getVarSubstreamedReq_challange);
                if (res != 0)
                {
                    m_client.Disconnect();
                    return res;
                }
                m_LastError = 0;
                WaitForNewS7plusReceived(m_ReadTimeout);
                if (m_LastError != 0)
                {
                    m_client.Disconnect();
                    return m_LastError;
                }

                var getVarSubstreamedRes_challenge = GetVarSubstreamedResponse.DeserializeFromPdu(m_ReceivedPDU);
                if (getVarSubstreamedRes_challenge == null)
                {
                    Console.WriteLine("S7CommPlusConnection - Connect.Password: getVarSubstreamedRes_challenge with Error!");
                    m_client.Disconnect();
                    return S7Consts.errIsoInvalidPDU;
                }

                byte[] challenge = (getVarSubstreamedRes_challenge.Value as ValueUSIntArray).GetValue();

                // Calculate challengeResponse [sha1(password) xor challenge]
                byte[] challengeResponse;
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    challengeResponse = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                }
                if (challengeResponse.Length != challenge.Length)
                {
                    Console.WriteLine("S7CommPlusConnection - Connect.Password: challengeResponse.Length != challenge.Length");
                    m_client.Disconnect();
                    return S7Consts.errIsoInvalidPDU;
                }
                for (int i = 0; i < challengeResponse.Length; ++i)
                {
                    challengeResponse[i] = (byte)(challengeResponse[i] ^ challenge[i]);
                }

                // Send challengeResponse
                var setVariableReq = new SetVariableRequest(ProtocolVersion.V2);
                setVariableReq.InObjectId = m_SessionId;
                setVariableReq.SessionId = m_SessionId;
                setVariableReq.Address = Ids.ServerSessionResponse;
                setVariableReq.Value = new ValueUSIntArray(challengeResponse);
                res = SendS7plusFunctionObject(setVariableReq);
                if (res != 0)
                {
                    m_client.Disconnect();
                    return res;
                }
                m_LastError = 0;
                WaitForNewS7plusReceived(m_ReadTimeout);
                if (m_LastError != 0)
                {
                    m_client.Disconnect();
                    return m_LastError;
                }

                var setVariableResponse = SetVariableResponse.DeserializeFromPdu(m_ReceivedPDU);
                if (setVariableResponse == null)
                {
                    Console.WriteLine("S7CommPlusConnection - Connect.Password: setVariableResponse with Error!");
                    m_client.Disconnect();
                    return S7Consts.errIsoInvalidPDU;
                }

            }
            else if (accessLevel > AccessLevel.FullAccess)
            {
                Console.WriteLine("S7CommPlusConnection - Connect.Password: Warning: Access level is not fullaccess but no password set!");
            }

            #endregion

            // If everything has been error-free up to this point, then the connection has been established successfully.
            Console.WriteLine("S7CommPlusConnection - Connect: Time for connection establishment: " + (Environment.TickCount - Elapsed) + " ms.");
            return 0;
        }

        public void Disconnect()
        {
            DeleteObject(m_SessionId);
            m_client.Disconnect();
        }

        /// <summary>
        /// Deletes the object with the given Id.
        /// </summary>
        /// <param name="deleteObjectId">The object Id to delete</param>
        /// <returns>0 on success</returns>
        private int DeleteObject(uint deleteObjectId)
        {
            int res;
            var delObjReq = new DeleteObjectRequest(ProtocolVersion.V2);
            delObjReq.DeleteObjectId = deleteObjectId;
            res = SendS7plusFunctionObject(delObjReq);
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                return m_LastError;
            }
            // If we delete our own session id, then there's no IntegrityId in the response.
            // And the error code gives an error, but not a fatal one.
            // If we delete another object, there should be an IntegrityId in the response, and
            // the response gives no error.
            if (deleteObjectId == m_SessionId)
            {
                var delObjRes = DeleteObjectResponse.DeserializeFromPdu(m_ReceivedPDU, false);
                Trace.WriteLine("S7CommPlusConnection - DeleteSession: Deleted our own Session Id object, not checking the response.");
                m_SessionId = 0; // not valid anymore
                m_SessionId2 = 0;
            }
            else
            {
                var delObjRes = DeleteObjectResponse.DeserializeFromPdu(m_ReceivedPDU, true);
                res = checkResponseWithIntegrity(delObjReq, delObjRes);
                if (res != 0)
                {
                    return res;
                }
                if (delObjRes.ReturnValue != 0)
                {
                    Console.WriteLine("S7CommPlusConnection - DeleteSession: Executed with Error! ReturnValue=" + delObjRes.ReturnValue);
                    res = -1;
                }
            }
            return res;
        }

        public int ReadValues(List<ItemAddress> addresslist, out List<object> values, out List<UInt64> errors)
        {
            // The requester must pass the internal type with the request, otherwise not all return values can be converted automatically.
            // For example, strings are transmitted as UInt-Array.
            values = new List<object>();
            errors = new List<UInt64>();
            // Initialize error fields to error value
            for (int i = 0; i < addresslist.Count; i++)
            {
                values.Add(null);
                errors.Add(0xffffffffffffffff);
            }

            // Split request into chunks, taking the MaxTags per request into account
            int chunk_startIndex = 0;
            int count_perChunk = 0;
            do
            {
                int res;
                var getMultiVarReq = new GetMultiVariablesRequest(ProtocolVersion.V2);

                getMultiVarReq.AddressList.Clear();
                count_perChunk = 0;
                while (count_perChunk < m_CommRessources.TagsPerReadRequestMax  && (chunk_startIndex + count_perChunk) < addresslist.Count)
                {
                    getMultiVarReq.AddressList.Add(addresslist[chunk_startIndex + count_perChunk]);
                    count_perChunk++;
                }

                res = SendS7plusFunctionObject(getMultiVarReq);
                m_LastError = 0;
                WaitForNewS7plusReceived(m_ReadTimeout);
                if (m_LastError != 0)
                {
                    return m_LastError;
                }

                var getMultiVarRes = GetMultiVariablesResponse.DeserializeFromPdu(m_ReceivedPDU);
                res = checkResponseWithIntegrity(getMultiVarReq, getMultiVarRes);
                if (res != 0)
                {
                    return res;
                }
                // ReturnValue shows also an error, if only one single variable could not be read
                if (getMultiVarRes.ReturnValue != 0)
                {
                    Console.WriteLine("S7CommPlusConnection - ReadValues: Executed with Error! ReturnValue=" + getMultiVarRes.ReturnValue);
                }

                // TODO: If a variable could not be read, there is no value, but there is an ErrorValue.
                // The user must therefore check whether Value != null. Maybe there's a more elegant solution.
                foreach (var v in getMultiVarRes.Values)
                {
                    values[chunk_startIndex + (int)v.Key - 1] = v.Value;
                    // Initialize error to 0, will be overwritten below if there was an error on an item.
                    errors[chunk_startIndex + (int)v.Key - 1] = 0;
                }

                foreach (var ev in getMultiVarRes.ErrorValues)
                {
                    errors[chunk_startIndex + (int)ev.Key - 1] = ev.Value;
                }
                chunk_startIndex += count_perChunk;

            } while (chunk_startIndex < addresslist.Count);

            return m_LastError;
        }

        public int WriteValues(List<ItemAddress> addresslist, List<PValue> values, out List<UInt64> errors)
        {
            int res;
            errors = new List<UInt64>();
            for (int i = 0; i < addresslist.Count; i++)
            {
                // Initialize to no error value, as there's no explicit value for write success.
                errors.Add(0);
            }

            // Split request into chunks, taking the MaxTags per request into account
            int chunk_startIndex = 0;
            int count_perChunk = 0;
            do
            {
                var setMultiVarReq = new SetMultiVariablesRequest(ProtocolVersion.V2);
                setMultiVarReq.AddressListVar.Clear();
                setMultiVarReq.ValueList.Clear();
                count_perChunk = 0;
                while (count_perChunk < m_CommRessources.TagsPerWriteRequestMax && (chunk_startIndex + count_perChunk) < addresslist.Count)
                {
                    setMultiVarReq.AddressListVar.Add(addresslist[chunk_startIndex + count_perChunk]);
                    setMultiVarReq.ValueList.Add(values[chunk_startIndex + count_perChunk]);
                    count_perChunk++;
                }

                res = SendS7plusFunctionObject(setMultiVarReq);
                if (res != 0)
                {
                    return res;
                }
                m_LastError = 0;
                WaitForNewS7plusReceived(m_ReadTimeout);
                if (m_LastError != 0)
                {
                    return m_LastError;
                }

                var setMultiVarRes = SetMultiVariablesResponse.DeserializeFromPdu(m_ReceivedPDU);
                res = checkResponseWithIntegrity(setMultiVarReq, setMultiVarRes);
                if (res != 0)
                {
                    return res;
                }
                // ReturnValue shows also an error, if only one single variable could not be written
                if (setMultiVarRes.ReturnValue != 0)
                {
                    Console.WriteLine("S7CommPlusConnection - WriteValues: Write with errors. ReturnValue=" + setMultiVarRes.ReturnValue);
                }

                foreach (var ev in setMultiVarRes.ErrorValues)
                {
                    errors[chunk_startIndex + (int)ev.Key - 1] = ev.Value;
                }
                chunk_startIndex += count_perChunk;

            } while (chunk_startIndex < addresslist.Count);

            return m_LastError;
        }

        public int SetPlcOperatingState(Int32 state)
        {
            int res;
            var setVarReq = new SetVariableRequest(ProtocolVersion.V2);
            setVarReq.InObjectId = Ids.NativeObjects_theCPUexecUnit_Rid;
            setVarReq.Address = Ids.CPUexecUnit_operatingStateReq;
            setVarReq.Value = new ValueDInt(state);

            res = SendS7plusFunctionObject(setVarReq);
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                m_client.Disconnect();
                return m_LastError;
            }

            var setVarRes = SetVariableResponse.DeserializeFromPdu(m_ReceivedPDU);
            if (setVarRes == null)
            {
                Console.WriteLine("S7CommPlusConnection - Connect: SetVariableResponse with Error!");
                m_client.Disconnect();
                return S7Consts.errIsoInvalidPDU;
            }

            return 0;
        }

        public int Browse(out List<VarInfo> varInfoList)
        {
            int res;
            varInfoList = new List<VarInfo>();
            Browser vars = new Browser();
            ExploreRequest exploreReq;
            ExploreResponse exploreRes;

            #region Read all objects

            var exploreData = new List<BrowseData>();

            exploreReq = new ExploreRequest(ProtocolVersion.V2);
            exploreReq.ExploreId = Ids.NativeObjects_thePLCProgram_Rid;
            exploreReq.ExploreRequestId = Ids.None;
            exploreReq.ExploreChildsRecursive = 1;
            exploreReq.ExploreParents = 0;

            // We want to know the following attributes
            exploreReq.AddressList.Add(Ids.ObjectVariableTypeName);
            exploreReq.AddressList.Add(Ids.Block_BlockNumber);
            exploreReq.AddressList.Add(Ids.ASObjectES_Comment);

            res = SendS7plusFunctionObject(exploreReq);
            if (res != 0)
            {
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                return m_LastError;
            }

            exploreRes = ExploreResponse.DeserializeFromPdu(m_ReceivedPDU, true);
            res = checkResponseWithIntegrity(exploreReq, exploreRes);
            if (res != 0)
            {
                return res;
            }

            #endregion

            #region Evaluate all data blocks that then need to be browsed

            var obj = exploreRes.Objects.First(o => o.ClassId == Ids.PLCProgram_Class_Rid);

            foreach (var ob in obj.GetObjects())
            {
                switch (ob.ClassId)
                {
                    case Ids.DB_Class_Rid:
                        UInt32 relid = ob.RelationId;
                        UInt32 area = (relid >> 16);
                        UInt32 num = relid & 0xffff;
                        if (area == 0x8a0e)
                        {
                            var name = (ValueWString)(ob.GetAttribute(Ids.ObjectVariableTypeName));
                            BrowseData data = new BrowseData();
                            data.db_block_relid = relid;
                            data.db_name = name.GetValue();
                            data.db_number = num;
                            exploreData.Add(data);
                        }
                        break;
                }
            }

            #endregion

            #region Determine the TypeInfo RID to the RelId from the first response
            // By querying LID = 1 from all DBs you get the RID back with which the type information can be queried.
            // This is neccessary because, for example, with instance DBs (e.g. TON), the type information must
            // not be accessed via the RID of the DB, but of the RID of the TON.
            var readlist = new List<ItemAddress>();
            var values = new List<object>();
            var errors = new List<UInt64>();

            foreach (var data in exploreData)
            {
                if (data.db_number > 0) // only process datablocks here, no marker, timer etc.
                {
                    // Insert the variable address
                    var adr1 = new ItemAddress();
                    adr1.AccessArea = data.db_block_relid;
                    adr1.AccessSubArea = Ids.DB_ValueActual;
                    adr1.LID.Add(1);
                    readlist.Add(adr1);
                }
            }
            res = ReadValues(readlist, out values, out errors);
            if (res != 0)
            {
                return res;
            }
            #endregion

            #region Pass the preliminary information for recombination to ExploreSymbols

            // Add the response information to the list
            for (int i = 0; i < values.Count; i++)
            {
                if (errors[i] == 0)
                {
                    ValueRID rid = (ValueRID)values[i];
                    var data = exploreData[i];
                    data.db_block_ti_relid = rid.GetValue();
                    exploreData[i] = data;
                }
                else
                {
                    // On error, set the relid to zero, will be removed from the list in the next step.
                    // TODO: Report this as an error?
                    var data = exploreData[i];
                    data.db_block_ti_relid = 0;
                    exploreData[i] = data;
                }
            }
            // Remove elements with db_block_ti_relid == 0. This occurs e.g. on datablocks only present in load memory.
            // The informations can't be used any further (at least not for variable access).
            exploreData.RemoveAll(item => item.db_block_ti_relid == 0);

            foreach (var ed in exploreData)
            {
                vars.AddBlockNode(eNodeType.Root, ed.db_name, ed.db_block_relid, ed.db_block_ti_relid);
            }

            // Add IQMCT areas manually
            vars.AddBlockNode(eNodeType.Root, "IArea", Ids.NativeObjects_theIArea_Rid, 0x90010000);
            vars.AddBlockNode(eNodeType.Root, "QArea", Ids.NativeObjects_theQArea_Rid, 0x90020000);
            vars.AddBlockNode(eNodeType.Root, "MArea", Ids.NativeObjects_theMArea_Rid, 0x90030000);
            vars.AddBlockNode(eNodeType.Root, "S7Timers", Ids.NativeObjects_theS7Timers_Rid, 0x90050000);
            vars.AddBlockNode(eNodeType.Root, "S7Counters", Ids.NativeObjects_theS7Counters_Rid, 0x90060000);

            #endregion

            #region Read the Type Info Container (as a single big PDU, must be proven to be the way to go in big programs)
            exploreReq = new ExploreRequest(ProtocolVersion.V2);
            // With ObjectOMSTypeInfoContainer we get all in a big PDU (with maybe hundreds of fragments)
            exploreReq.ExploreId = Ids.ObjectOMSTypeInfoContainer;
            exploreReq.ExploreRequestId = Ids.None;
            exploreReq.ExploreChildsRecursive = 1;
            exploreReq.ExploreParents = 0;

            res = SendS7plusFunctionObject(exploreReq);
            if (res != 0)
            {
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                return m_LastError;
            }
            #endregion

            #region Process the response, and build the complete variables list
            exploreRes = ExploreResponse.DeserializeFromPdu(m_ReceivedPDU, true);
            res = checkResponseWithIntegrity(exploreReq, exploreRes);
            if (res != 0)
            {
                return res;
            }
            var objs = exploreRes.Objects.First(o => o.ClassId == Ids.ClassOMSTypeInfoContainer);

            vars.SetTypeInfoContainerObjects(objs.GetObjects());
            vars.BuildTree();
            vars.BuildFlatList();
            varInfoList = vars.GetVarInfoList();
            #endregion

            return 0;
        }

        /// <summary>
        /// Gets the first level of a tag symbol string. Removes the " used to escape special chars.
        /// </summary>
        /// <param name="symbol">plc tag symbol</param>
        /// <returns>The first level of the symbol string</returns>
        /// <exception cref="Exception">Symbol syntax error</exception>
        private string parseSymbolLevel(ref string symbol)
        {
            if (symbol.StartsWith("\""))
            {
                int idx = symbol.IndexOf('"', 1);
                if (idx < 0) throw new Exception("Symbol syntax error");
                string lvl = symbol.Substring(1, idx - 1);
                symbol = symbol.Remove(0, idx + 1);
                if (symbol.StartsWith(".")) symbol = symbol.Remove(0, 1);
                return lvl;
            }
            else
            {
                int idx = symbol.IndexOf('.');
                int idx2 = symbol.IndexOf('[', 1);
                if (idx2 >= 0 && (idx2 < idx || idx < 0)) idx = idx2;
                if (idx >= 0)
                {
                    string lvl = symbol.Substring(0, idx);
                    symbol = symbol.Remove(0, idx);
                    if (symbol.StartsWith(".")) symbol = symbol.Remove(0, 1);
                    return lvl;
                }
                else
                {
                    string lvl = symbol;
                    symbol = "";
                    return lvl;
                }
            }
        }

        /// <summary>
        /// Gets the typeinfo by given ti relid from the internal buffer. If it's not found in the buffer
        /// it's fetched from the PLC and stored in the buffer.
        /// </summary>
        /// <param name="ti_relid">type info relid</param>
        /// <returns>type info</returns>
        /// <exception cref="Exception">Could not get type info</exception>
        public PObject getTypeInfoByRelId(uint ti_relid)
        {
            PObject pObj = typeInfoList.Find(ti => ti.RelationId == ti_relid);
            if (pObj == null)
            {
                // Type info not found in list, request it from plc
                List<PObject> newPObj = new List<PObject>();
                if (GetTypeInformation(ti_relid, out newPObj) != 0) throw new Exception("Could not get type info");
                typeInfoList.AddRange(newPObj);
                // Try again
                pObj = typeInfoList.Find(ti => ti.RelationId == ti_relid);
            }
            return pObj;
        }

        /// <summary>
        /// Calculates the access sequence for 1 dimensional arrays.
        /// </summary>
        /// <param name="symbol">plc tag symbol</param>
        /// <param name="varType">Var type that holds the dim info</param>
        /// <param name="varInfo">used to build access sequence</param>
        /// <exception cref="Exception">Symbol syntax error</exception>
        private void calcAccessSeqFor1DimArray(ref string symbol, PVartypeListElement varType, VarInfo varInfo)
        {
            Regex re = new Regex(@"^\[(-?\d+)\]");
            Match m = re.Match(symbol);
            if (!m.Success) throw new Exception("Symbol syntax error");
            parseSymbolLevel(ref symbol); // remove index from symbol string
            int arrayIndex = int.Parse(m.Groups[1].Value);

            var ioit = (IOffsetInfoType_1Dim)varType.OffsetInfoType;
            uint arrayElementCount = ioit.GetArrayElementCount();
            int arrayLowerBounds = ioit.GetArrayLowerBounds();

            if (arrayIndex - arrayLowerBounds > arrayElementCount) throw new Exception("Out of bounds");
            if (arrayIndex < arrayLowerBounds) throw new Exception("Out of bounds");
            varInfo.AccessSequence += "." + String.Format("{0:X}", arrayIndex - arrayLowerBounds);
            if (varType.OffsetInfoType.HasRelation()) varInfo.AccessSequence += ".1"; // additional ".1" for array of struct
        }

        /// <summary>
        /// Calculates the access sequence for multi-dimensional arrays.
        /// </summary>
        /// <param name="symbol">plc tag symbol</param>
        /// <param name="varType">Var type that holds the dim info</param>
        /// <param name="varInfo">used to build access sequence</param>
        /// <exception cref="Exception">Symbol syntax error</exception>
        private void calcAccessSeqForMDimArray(ref string symbol, PVartypeListElement varType, VarInfo varInfo)
        {
            Regex re = new Regex(@"^\[( ?-?\d+ ?(, ?-?\d+ ?)+)\]");
            Match m = re.Match(symbol);
            if (!m.Success) throw new Exception("Symbol syntax error");
            parseSymbolLevel(ref symbol); // remove index from symbol string
            string idxs = m.Groups[1].Value.Replace(" ", "");

            int[] indexes = Array.ConvertAll(idxs.Split(','), e => int.Parse(e));
            var ioit = (IOffsetInfoType_MDim)varType.OffsetInfoType;
            uint[] MdimArrayElementCount = (uint[])ioit.GetMdimArrayElementCount().Clone();
            int[] MdimArrayLowerBounds = ioit.GetMdimArrayLowerBounds();

            // check dim count
            int dimCount = MdimArrayElementCount.Aggregate(0, (acc, act) => acc += (act > 0) ? 1 : 0);
            if (dimCount != indexes.Count()) throw new Exception("Out of bounds");
            // check bounds
            for (int i = 0; i < dimCount; ++i)
            {
                indexes[i] = (indexes[i] - MdimArrayLowerBounds[dimCount - i - 1]);
                if (indexes[i] > MdimArrayElementCount[dimCount - i - 1]) throw new Exception("Out of bounds");
                if (indexes[i] < 0) throw new Exception("Out of bounds");
            }

            // calc dim size
            if (varType.Softdatatype == Softdatatype.S7COMMP_SOFTDATATYPE_BBOOL)
            {
                MdimArrayElementCount[0] += 8 - MdimArrayElementCount[0] % 8; // for bool must be a mutiple of 8!
            }
            uint[] dimSize = new uint[dimCount];
            uint g = 1;
            for (int i = 0; i < dimCount - 1; ++i)
            {
                dimSize[i] = g;
                g *= MdimArrayElementCount[i];
            }
            dimSize[dimCount - 1] = g;

            // calc id
            int arrayIndex = 0;
            for (int i = 0; i < dimCount; ++i)
            {
                arrayIndex += indexes[i] * (int)dimSize[dimCount - i - 1];
            }

            varInfo.AccessSequence += "." + String.Format("{0:X}", arrayIndex);
            if (varType.OffsetInfoType.HasRelation()) varInfo.AccessSequence += ".1"; // additional ".1" for array of struct
        }

        /// <summary>
        /// Browses the symbol level by level recursively. Fetches missing type info automatically from the plc.
        /// </summary>
        /// <param name="ti_relid">type info relid</param>
        /// <param name="symbol">plc tag symbol</param>
        /// <param name="varInfo">used to build access sequence</param>
        /// <returns>plc tag or null if not found</returns>
        /// <exception cref="Exception">Symbol syntax error, Out of bounds</exception>
        private PlcTag browsePlcTagBySymbol(uint ti_relid, ref string symbol, VarInfo varInfo)
        {
            PObject pObj = getTypeInfoByRelId(ti_relid);
            if (pObj == null) throw new Exception("Could not get type info");
            string levelName = parseSymbolLevel(ref symbol);
            // find level name of symbol in var list
            int idx = pObj.VarnameList?.Names?.IndexOf(levelName) ?? -1;
            if (idx < 0) return null;
            PVartypeListElement varType = pObj.VartypeList.Elements[idx];
            varInfo.AccessSequence += "." + String.Format("{0:X}", varType.LID);
            if (varType.OffsetInfoType.Is1Dim())
            {
                calcAccessSeqFor1DimArray(ref symbol, varType, varInfo);
            }
            if (varType.OffsetInfoType.IsMDim())
            {
                calcAccessSeqForMDimArray(ref symbol, varType, varInfo);
            }
            if (varType.OffsetInfoType.HasRelation())
            {
                if (symbol.Length <= 0)
                {
                    return null;
                }
                else
                {
                    var ioit = (IOffsetInfoType_Relation)varType.OffsetInfoType;
                    return browsePlcTagBySymbol(ioit.GetRelationId(), ref symbol, varInfo);
                }
            }
            else
            {
                return PlcTags.TagFactory(varInfo.Name, new ItemAddress(varInfo.AccessSequence), varType.Softdatatype);
            }
        }

        /// <summary>
        /// Get the plc tag for the given plc tag symbol. 
        /// </summary>
        /// <param name="symbol">plc tag symbol</param>
        /// <returns>plc tag, returns null if plc tag could not be found</returns>
        public PlcTag getPlcTagBySymbol(string symbol)
        {
            VarInfo varInfo = new VarInfo();
            varInfo.Name = symbol;
            // make sure we have the db list
            if (dbInfoList == null)
            {
                if (GetListOfDatablocks(out dbInfoList) != 0) { return null; }
            }
            string levelName = parseSymbolLevel(ref symbol);
            // find db by first level name of symbol
            DatablockInfo dbInfo = dbInfoList.Find(dbi => dbi.db_name == levelName);
            if (dbInfo != null)
            {
                varInfo.AccessSequence = String.Format("{0:X}", dbInfo.db_block_relid);
                return browsePlcTagBySymbol(dbInfo.db_block_ti_relid, ref symbol, varInfo);
            }
            else
            {
                symbol = varInfo.Name;
                // Merker
                varInfo.AccessSequence = String.Format("{0:X}", Ids.NativeObjects_theMArea_Rid);
                PlcTag tag = browsePlcTagBySymbol(0x90030000, ref symbol, varInfo);
                if (tag != null) return tag;
                symbol = varInfo.Name;
                // Outputs
                varInfo.AccessSequence = String.Format("{0:X}", Ids.NativeObjects_theQArea_Rid);
                tag = browsePlcTagBySymbol(0x90020000, ref symbol, varInfo);
                if (tag != null) return tag;
                symbol = varInfo.Name;
                // Inputs
                varInfo.AccessSequence = String.Format("{0:X}", Ids.NativeObjects_theIArea_Rid);
                tag = browsePlcTagBySymbol(0x90010000, ref symbol, varInfo);
                if (tag != null) return tag;
                // TODO: implement s5timers and counters... no one uses them anymore anyway
            }
            return null;
        }

        public class BrowseEntry
        {
            public string Name;
            public uint Softdatatype;
            public UInt32 LID;
            public UInt32 SymbolCrc;
            public string AccessSequence;
        };

        public class BrowseData
        {
            public string db_name;                                          // Name of the datablock
            public UInt32 db_number;                                        // Number of the datablock
            public UInt32 db_block_relid;                                   // RID of the datablock
            public UInt32 db_block_ti_relid;                                // Type-Info RID of the datablock
            public List<BrowseEntry> variables = new List<BrowseEntry>();   // Variables inside the datablock
        };

        public class DatablockInfo
        {
            public string db_name;                                          // Name of the datablock
            public UInt32 db_number;                                        // Number of the datablock
            public UInt32 db_block_relid;                                   // RID of the datablock
            public UInt32 db_block_ti_relid;                                // Type-Info RID of the datablock
        };

        public int GetListOfDatablocks(out List<DatablockInfo> dbInfoList)
        {
            int res;

            dbInfoList = new List<DatablockInfo>();

            var exploreReq = new ExploreRequest(ProtocolVersion.V2);
            exploreReq.ExploreId = Ids.NativeObjects_thePLCProgram_Rid;
            exploreReq.ExploreRequestId = Ids.None;
            exploreReq.ExploreChildsRecursive = 1;
            exploreReq.ExploreParents = 0;

            // Add the attributes we need in the response
            exploreReq.AddressList.Add(Ids.ObjectVariableTypeName);

            // Set filter on Id for Datablock Class RID. With this filter, we only
            // get informations from datablocks, and not other blocks we don't need here.
            var filter = new ValueStruct(Ids.Filter);
            filter.AddStructElement(Ids.FilterOperation, new ValueDInt(8)); // 8 = InstanceIOf
            filter.AddStructElement(Ids.AddressCount, new ValueUDInt(0));
            uint[] faddress = new uint[32]; // Unknown, possible dependant on FilterOperation
            filter.AddStructElement(Ids.Address, new ValueUDIntArray(faddress));
            filter.AddStructElement(Ids.FilterValue, new ValueRID(Ids.DB_Class_Rid));

            exploreReq.FilterData = filter;

            res = SendS7plusFunctionObject(exploreReq);
            if (res != 0)
            {
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                return m_LastError;
            }

            var exploreRes = ExploreResponse.DeserializeFromPdu(m_ReceivedPDU, true);
            res = checkResponseWithIntegrity(exploreReq, exploreRes);
            if (res != 0)
            {
                return res;
            }

            // Get the datablock information we want further informations from.
            var objList = exploreRes.Objects;

            foreach (var ob in objList)
            {
                // May be this check can be removed, if setting the filter to the DB_Class_Rid is working 100%.
                switch (ob.ClassId)
                {
                    case Ids.DB_Class_Rid:
                        UInt32 relid = ob.RelationId;
                        UInt32 area = (relid >> 16);
                        UInt32 num = relid & 0xffff;
                        if (area == 0x8a0e)
                        {
                            var name = (ValueWString)(ob.GetAttribute(Ids.ObjectVariableTypeName));
                            DatablockInfo data = new DatablockInfo();
                            data.db_block_relid = relid;
                            data.db_name = name.GetValue();
                            data.db_number = num;
                            dbInfoList.Add(data);
                        }
                        break;
                }
            }

            // Get the TypeInfo RID to RelId from the first response

            // With LID=1 we get the RID back. With this number we can explore further 
            // informations of this datablock.
            // This is neccessary, because informations about instance DBs (e.g. TON) you
            // don't get by the RID of the DB, instead of exploring the TON Type RID.
            var readlist = new List<ItemAddress>();
            var values = new List<object>();
            var errors = new List<UInt64>();

            foreach (var data in dbInfoList)
            {
                if (data.db_number > 0)
                {
                    // Insert the address
                    var adr1 = new ItemAddress();
                    adr1.AccessArea = data.db_block_relid;
                    adr1.AccessSubArea = Ids.DB_ValueActual;
                    adr1.LID.Add(1);
                    readlist.Add(adr1);
                }
            }
            res = ReadValues(readlist, out values, out errors);
            if (res != 0)
            {
                return res;
            }

            // Insert response data into the list
            for (int i = 0; i < values.Count; i++)
            {
                if (errors[i] == 0)
                {
                    var rid = (ValueRID)values[i];
                    var data = dbInfoList[i];
                    data.db_block_ti_relid = rid.GetValue();
                    dbInfoList[i] = data;
                }
                else
                {
                    // On error, set relid=0, which is then removed in the next step.
                    // Should we report this for the user?
                    var data = dbInfoList[i];
                    data.db_block_ti_relid = 0;
                    dbInfoList[i] = data;
                }
            }

            // Remove elements with db_block_ti_relid == 0.
            // This can occur on datablocks which are only in load memory and can't be explored.
            dbInfoList.RemoveAll(item => item.db_block_ti_relid == 0);

            return 0;
        }

        public int GetTypeInformation(uint exploreId, out List<PObject> objList)
        {
            int res;
            objList = new List<PObject>();

            var exploreReq = new ExploreRequest(ProtocolVersion.V2);
            exploreReq.ExploreId = exploreId;
            exploreReq.ExploreRequestId = Ids.None;
            exploreReq.ExploreChildsRecursive = 1;
            exploreReq.ExploreParents = 0;

            res = SendS7plusFunctionObject(exploreReq);
            if (res != 0)
            {
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                return m_LastError;
            }

            var exploreRes = ExploreResponse.DeserializeFromPdu(m_ReceivedPDU, true);
            res = checkResponseWithIntegrity(exploreReq, exploreRes);
            if (res != 0)
            {
                return res;
            }
            objList = exploreRes.Objects;

            return 0;
        }

        /// <summary>
        /// Requests the tag and block comments from the Plc, returned as XML strings.
        /// xml_linecomment:
        /// The returned XML format differs between between request of I/Q/M/C/T areas and datablocks:
        /// I/Q/M/C/T: <CommentDictionary>     <TagLineComments>      <Comment RefID="ID"> <DictEntry Lanuage="de-DE"> ....
        /// Datablock: <InterfaceLineComments> <Part Kind="Comments"> <Comment Path="ID">  <DictEntry Lanuage="de-DE"> ....
        /// As "ID" the number for the variable identification is used.
        /// 
        /// xml_dbcomment:
        /// The xml-value description generated from our own value xml-serialization for WStringSparseArray. The value key is the language id.
        /// Example:
        /// <Value type ="WStringSparseArray"><Value key="1032">DB Kommentar in german de-DE</Value><Value key="1034">DB comment in english en-US</Value></Value>
        /// </summary>
        /// <param name="relid">The relation ID for the area you want the comments for, e.g. 0x8a0e0000+db_number, or 0x52 for M-area</param>
        /// <param name="xml_linecomment"></param>
        /// <param name="xml_dbcomment"></param>
        /// <returns>0 if no error</returns>
        public int GetCommentsXml(uint relid, out string xml_linecomment, out string xml_dbcomment)
        {
            int res;
            // With requesting DataInterface_InterfaceDescription, whe would be able to get all informations like the access ids and
            // datatype informations, that we get from the other browsing method. Needs to be tested which one is more efficient on network traffic or plc load.
            // If we keep use browsing for the comments, at least we would be able to read all information in one request.
            xml_linecomment = String.Empty;
            xml_dbcomment = String.Empty;

            var exploreReq = new ExploreRequest(ProtocolVersion.V2);
            exploreReq.ExploreId = relid;
            exploreReq.ExploreRequestId = Ids.None;
            exploreReq.ExploreChildsRecursive = 1;
            exploreReq.ExploreParents = 0;

            // We want to know the following attributes
            exploreReq.AddressList.Add(Ids.ASObjectES_Comment);
            exploreReq.AddressList.Add(Ids.DataInterface_LineComments);

            res = SendS7plusFunctionObject(exploreReq);
            if (res != 0)
            {
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                return m_LastError;
            }

            var exploreRes = ExploreResponse.DeserializeFromPdu(m_ReceivedPDU, true);
            res = checkResponseWithIntegrity(exploreReq, exploreRes);
            if (res != 0)
            {
                return res;
            }

            foreach(var obj in exploreRes.Objects)
            {
                foreach(var att in obj.Attributes)
                {
                    switch (att.Key)
                    {
                        case Ids.ASObjectES_Comment:
                            var att_comment = (ValueWStringSparseArray)att.Value;
                            xml_dbcomment = att_comment.ToString();
                            break;
                        case Ids.DataInterface_LineComments:
                            var att_linecomment = (ValueBlobSparseArray)att.Value;
                            BlobDecompressor bd = new BlobDecompressor();
                            var blob_sp = att_linecomment.GetValue();
                            // In DBs we get the data with Sparsearray key = 1, in M-Area with key = 2.
                            // For now, just take the first, don't know where the key ids are for.
                            foreach (var key in blob_sp.Keys)
                            {
                                xml_linecomment = bd.decompress(blob_sp[key].value, 4); // Offset of 4, as we have a header for the zlib dictionary version
                                break;
                            }
                            break;
                    }
                }
            }
            return 0;
        }
    }
    #endregion
}
