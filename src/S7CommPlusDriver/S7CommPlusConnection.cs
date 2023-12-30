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

namespace S7CommPlusDriver
{
    public partial class S7CommPlusConnection
    {
        #region Private Members
        private S7Client m_client;
        private MemoryStream m_ReceivedStream;
        private bool m_ReceivedNeedMorePdus;
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
        // Initialize the max values to 20, 50 is possibly the lowest value.
        // The actual supported value is read after connection setup via a ReadRequest, which returns a DInt.
        private Int32 m_MaxTagsPerReadRequestLimit = 20;
        private Int32 m_MaxTagsPerWriteRequestLimit = 20;

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

            while (!m_NewS7CommPlusReceived && !Expired)
            {
                Thread.Sleep(2);
                Expired = Environment.TickCount - Elapsed > Timeout;
            }

            if (Expired)
            {
                Console.WriteLine("S7CommPlusConnection - WaitForNewS7plusReceived: ERROR: Timeout!");
                m_LastError = S7Consts.errTCPDataReceive;
            }
            m_NewS7CommPlusReceived = false;
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
            byte[] packet = new byte[NegotiatedIsoPduSize];

            // 4 Byte TPKT Header
            // 3 Byte ISO-Header
            // 5 Byte TLS Header + 17 Bytes addition from TLS
            // 4 Byte S7CommPlus Header
            // 4 Byte S7CommPlus Trailer (must fit into last PDU)
            int MaxSize = NegotiatedIsoPduSize - 4 - 3 - 5 - 17 - 4 - 4;

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
                    packet[sendLen] = 0x72;
                    sendLen++;
                    packet[sendLen] = protoVersion;
                    sendLen++;
                    packet[sendLen] = 0;
                    sendLen++;
                    packet[sendLen] = 0;
                    sendLen++;
                }
                Array.Resize(ref packet, sendLen);
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

            if (!m_ReceivedNeedMorePdus)
            {
                m_ReceivedStream = new MemoryStream();
            }
            // S7comm-plus
            byte protoVersion;
            int pos = 0;
            int s7HeaderDataLen = 0;
            // Check header
            if (PDU[pos] != 0x72)
            {
                m_LastError = S7Consts.errIsoInvalidPDU;
            }
            pos++;
            protoVersion = PDU[pos];
            if (protoVersion != ProtocolVersion.V1 && protoVersion != ProtocolVersion.V2 && protoVersion != ProtocolVersion.V3 && protoVersion != ProtocolVersion.SystemEvent)
            {
                // Need to disconnect
                m_LastError = S7Consts.errIsoInvalidPDU;
            }
            // For the first fragment, write the ProtocolVersion into the stream in advance
            if (!m_ReceivedNeedMorePdus)
            {
                m_ReceivedStream.Write(PDU, pos, 1);
            }
            pos++;

            // Read the length of the data-part from header
            s7HeaderDataLen = GetWordAt(PDU, pos);
            pos += 2;
            if (s7HeaderDataLen > 0)
            {
                // Special handling for SystemEvent 0xfe PDUs:
                // This only confirms a few data, but also reports major protocol errors (e.g.incorrect sequence numbers).
                // The confirms can be discarded (for now), but the errors are relevant, because a connection termination is neccessary.
                // As we don't have a trailer on this types, it's not possible that they are transmitted as fragments.
                if (protoVersion == ProtocolVersion.SystemEvent)
                {
                    Console.WriteLine("S7CommPlusConnection - OnDataReceived: ProtocolVersion 0xfe SystemEvent received");
                    m_ReceivedStream.Write(PDU, pos, s7HeaderDataLen);
                    pos += s7HeaderDataLen;
                    // Create SystemEventObject
                    m_ReceivedNeedMorePdus = false;
                    m_ReceivedStream.Position = 0;
                    m_NewS7CommPlusReceived = false;

                    var sysevt = SystemEvent.DeserializeFromPdu(m_ReceivedStream);
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
                    m_ReceivedStream.Write(PDU, pos, s7HeaderDataLen);
                    pos += s7HeaderDataLen;
                    // If this is a fragmented PDU, then at this point no trailer
                    if ((len - 4 - 4) == s7HeaderDataLen)
                    {
                        m_ReceivedNeedMorePdus = false;
                        m_ReceivedStream.Position = 0;    // Set position back to zero, ready for readout
                        m_NewS7CommPlusReceived = true;
                    }
                    else
                    {
                        m_ReceivedNeedMorePdus = true;
                    }
                }
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

        private int ReadSystemLimits()
        {
            // Read SystemLimits
            // Assumption (so far, because for all CPUs which have be seen both values were the same):
            // 1000 = Number for Reading
            // 1001 = Number for Writing
            int res;
            var readlist = new List<ItemAddress>();
            var values = new List<object>();
            var errors = new List<UInt64>();

            var adrMaxReadTags = new ItemAddress
            {
                AccessArea = Ids.ObjectRoot,
                AccessSubArea = Ids.SystemLimits
            };
            adrMaxReadTags.LID.Add(1000);
            var adrMaxWriteTags = new ItemAddress
            {
                AccessArea = Ids.ObjectRoot,
                AccessSubArea = Ids.SystemLimits
            };
            adrMaxWriteTags.LID.Add(1001);
            readlist.Add(adrMaxReadTags);
            readlist.Add(adrMaxWriteTags);

            res = ReadValues(readlist, out values, out errors);
            if (values[0] != null && errors[0] == 0)
            {
                var v = (ValueDInt)values[0];
                m_MaxTagsPerReadRequestLimit = v.GetValue();
            }
            if (values[1] != null && errors[1] == 0)
            {
                var v = (ValueDInt)values[1];
                m_MaxTagsPerWriteRequestLimit = v.GetValue();
            }
            return res;
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
        public int Connect(string address)
        {
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
            sslRes = InitSslResponse.DeserializeFromPdu(m_ReceivedStream);
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

            var createObjReq = new CreateObjectRequest(ProtocolVersion.V1, 0);
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

            var createObjRes = CreateObjectResponse.DeserializeFromPdu(m_ReceivedStream);
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

            var setMultiVarRes = SetMultiVariablesResponse.DeserializeFromPdu(m_ReceivedStream);
            if (setMultiVarRes == null)
            {
                Console.WriteLine("S7CommPlusConnection - Connect: SetMultiVariablesResponse with Error!");
                m_client.Disconnect();
                return S7Consts.errIsoInvalidPDU;
            }

            #endregion

            #region Step 5: Read SystemLimits
            res = ReadSystemLimits();
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
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
                var delObjRes = DeleteObjectResponse.DeserializeFromPdu(m_ReceivedStream, false);
                Trace.WriteLine("S7CommPlusConnection - DeleteSession: Deleted our own Session Id object, not checking the response.");
                m_SessionId = 0; // not valid anymore
                m_SessionId2 = 0;
            }
            else
            {
                var delObjRes = DeleteObjectResponse.DeserializeFromPdu(m_ReceivedStream, true);
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
                while (count_perChunk < m_MaxTagsPerReadRequestLimit && (chunk_startIndex + count_perChunk) < addresslist.Count)
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

                var getMultiVarRes = GetMultiVariablesResponse.DeserializeFromPdu(m_ReceivedStream);
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
                while (count_perChunk < m_MaxTagsPerWriteRequestLimit && (chunk_startIndex + count_perChunk) < addresslist.Count)
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

                var setMultiVarRes = SetMultiVariablesResponse.DeserializeFromPdu(m_ReceivedStream);
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

            var setVarRes = SetVariableResponse.DeserializeFromPdu(m_ReceivedStream);
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

            exploreRes = ExploreResponse.DeserializeFromPdu(m_ReceivedStream, true);
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
            // This is necessary because, for example, with instance DBs (e.g. TON), the type information must
            // not be accessed via the RID of the DB but of the TON.
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
            // Remove elements with db_block_ti_relid == 0. This occurs e.g. on datablocks only present in load memors.
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
            exploreRes = ExploreResponse.DeserializeFromPdu(m_ReceivedStream, true);
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

            var exploreRes = ExploreResponse.DeserializeFromPdu(m_ReceivedStream, true);
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
                    // On error, set relid=0, which is then removed in the next step
                    // Should we report this for the user?
                    var data = dbInfoList[i];
                    data.db_block_ti_relid = 0;
                    dbInfoList[i] = data;
                }
            }

            // Remove elements with db_block_ti_relid == 0.
            // This can occur on datablocks which are only in load memory, and can't be explored.
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

            var exploreRes = ExploreResponse.DeserializeFromPdu(m_ReceivedStream, true);
            res = checkResponseWithIntegrity(exploreReq, exploreRes);
            if (res != 0)
            {
                return res;
            }
            objList = exploreRes.Objects;

            return 0;
        }
    }
    #endregion
}
