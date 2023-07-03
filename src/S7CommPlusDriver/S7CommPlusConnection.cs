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

namespace S7CommPlusDriver
{
    // Diese Klasse handelt das aufteilen der S7commPlus Pakete auf ein oder mehrere Iso PDUs
    public class S7CommPlusConnection
    {
        #region Private Members
        private S7Client m_client;
        private MemoryStream m_ReceivedStream;
        private bool m_ReceivedNeedMorePdus;
        private bool m_NewS7CommPlusReceived;
        
        private UInt32 m_SessionId;
        private int m_ReadTimeout = 5000;

        private UInt16 m_SequenceNumber = 0;
        private UInt32 m_IntegrityId = 0;
        private UInt32 m_LastIntegrityId;

        // Maximale Anzahl mit 20 initialisieren, da 50 vermutlich der Mindestwert ist. 
        // Denn die konkrete Anzahl wird über einen ReadRequest ausgelesen.
        // Rückgabetyp ist DInt.
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

        private UInt32 GetNextIntegrityId()
        {
            // Wenn Überlauf nicht bei 0, dann Fehler von SPS
            if (m_IntegrityId == UInt32.MaxValue)
            {
                m_IntegrityId = 0;
            }
            else
            {
                m_IntegrityId++;
            }

            return m_IntegrityId;
        }

        private void WaitForNewS7plusReceived(int Timeout)
        {
            bool Expired = false;
            int Elapsed = Environment.TickCount;
            //Console.WriteLine("S7CommPlusConnection - WaitForNewS7plusReceived: Warte max " + Timeout + " ms auf neue PDU..");
            while (!m_NewS7CommPlusReceived && !Expired)
            {
                Thread.Sleep(2);
                Expired = Environment.TickCount - Elapsed > Timeout;
            }
            
            if (Expired)
            {
                Console.WriteLine("S7CommPlusConnection - WaitForNewS7plusReceived: FEHLER: Timeout!");
                m_LastError = S7Consts.errTCPDataReceive;
            } else
            {
                // Console.WriteLine("S7CommPlusConnection - WaitForNewS7plusReceived: ...neue S7CommPlusPDU vollständig empfangen. Zeit: " + (Environment.TickCount - Elapsed) + " ms.");
            }
            m_NewS7CommPlusReceived = false;
        }


        private int SendS7plusFunctionObject(IS7pSendableObject funcObj)
        {
            MemoryStream stream = new MemoryStream();
            funcObj.Serialize(stream);
            return SendS7plusPDUdata(stream.ToArray(), (int)stream.Length, funcObj.GetProtocolVersion());
        }

        private int SendS7plusPDUdata(byte[] sendPduData, int bytesToSend, byte protoVersion)
        {
            m_LastError = 0;

            int curSize;
            int sourcePos = 0;
            int sendLen;
            int NegotiatedIsoPduSize = 1024;// TODO: Ausgehandelte TPDU auswerten
            byte[] packet = new byte[NegotiatedIsoPduSize];

            // 4 Byte TPKT Header
            // 3 Byte ISO-Header
            // 5 Byte TLS Header + 17 Bytes Zusatz durch TLS
            // 4 Byte S7CommPlus Header
            // 4 Byte S7CommPlus Trailer (muss bei letzter mit hineinpassen)
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
                // Datenteil
                Array.Copy(sendPduData, sourcePos, packet, 4, curSize);
                sourcePos += curSize;
                sendLen = 4 + curSize;

                // Trailer nur beim letzten Paket
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
            // Console.WriteLine("S7CommPlusConnection - OnDataReceived: len=" + len);

            // Hier kommt immer eine komplette TPDU herein.
            // An dieser Stelle können schon fragmentierte S7CommPlus PDU festgestellt werden.
            // Wenn unfragmentiert, dann ist die TPKT.Length minus 15 gleich der Länge im S7CommPlus.Header.
            // 15 Bytes wegen: 4 Bytes TPKT.Header.len + 3 Bytes ISO.Header.Len + 4 Bytes S7CommPlus.Header.len + 4 Bytes S7CommPlus.trailer.Len.
            // Da hier schon die reinen Nutzdaten der TPDU hereinkommen, sind das nur minus 4 Bytes Header + 4 Bytes Trailer.
            // 
            // Besonderheit bei SystemEvents mit ProtocolVersion = 0xfe: Hier gibt es nur den Header
            // Es müsste als erstes Byte auf jeden Fall immer das eine Byte für die Protocol-Version in den Stream geschrieben werden.
            //
            // Die Datenlänge darf nicht, weil die bei fragmentierten PDUs nicht gültig ist
            //
            if (!m_ReceivedNeedMorePdus)
            {
                m_ReceivedStream = new MemoryStream();
            }

            // S7comm-plus
            byte protoVersion;
            int pos = 0;
            int s7HeaderDataLen = 0;
            // Header prüfen
            // Console.WriteLine("S7CommPlusConnection - OnDataReceived: Prüfe auf S7 Protokoll-ID PDU[0]=0x" + String.Format("{0:X}", PDU[pos]));
            if (PDU[pos] != 0x72)
            {
                m_LastError = S7Consts.errIsoInvalidPDU;
            }
            pos++;
            protoVersion = PDU[pos];
            if (protoVersion != ProtocolVersion.V1 && protoVersion != ProtocolVersion.V2 && protoVersion != ProtocolVersion.V3 && protoVersion != ProtocolVersion.SystemEvent)
            {
                // Abbau der Verbindung notwendig
                m_LastError = S7Consts.errIsoInvalidPDU;
            }
            // Beim ersten Fragment die ProtocolVersion vorab in den Stream schreiben
            if (!m_ReceivedNeedMorePdus)
            {
                m_ReceivedStream.Write(PDU, pos, 1);
            }
            pos++;

            // Länge des Datenteils aus dem Header auslesen
            s7HeaderDataLen = GetWordAt(PDU, pos);
            pos += 2;
            // Console.WriteLine("S7CommPlusConnection - OnDataReceived: Längenangabe aus S7 Header, len=" + s7HeaderDataLen.ToString());
            if (s7HeaderDataLen > 0)
            {
                // SystemEvent 0xfe PDUs gesondert behandeln.
                // Es werden dadurch nur ein paar Daten bestätigt, aber auch grobe Protokollfehler gemeldet (z.B. falsche Sequenznummern)
                // Bei ersterem können die Daten vorerst verworfen werden, bei letzterem ist eigentlich ein Verbindungsabbruch notwendig.
                // Bei ersterem ist Datalength immer 16 Bytes, wenn mehr, dann Fehler
                // Da hier kein Trailer vorhanden ist, können die PDUs eigentlich nicht fragmentiert übertragen werden
                if (protoVersion == ProtocolVersion.SystemEvent)
                {
                    Console.WriteLine("S7CommPlusConnection - OnDataReceived: ProtocolVersion 0xfe SystemEvent received");
                    m_ReceivedStream.Write(PDU, pos, s7HeaderDataLen);
                    pos += s7HeaderDataLen;
                    if (s7HeaderDataLen > 16)
                    {
                        Console.WriteLine("S7CommPlusConnection - OnDataReceived: SystemEvent mit s7HeaderDataLen > 16, vermutlich Fehler.");
                        // Abbau der Verbindung notwendig
                        m_LastError = S7Consts.errIsoInvalidPDU;
                    }
                    // Alles verwerden
                    m_ReceivedNeedMorePdus = false;
                    m_ReceivedStream.Position = 0;    // Position wieder auf Null setzen, damit später ausgelesen werden kann
                    m_NewS7CommPlusReceived = false;
                }
                else
                {
                    // Datenteil in Ziel kopieren
                    // Console.WriteLine("S7CommPlusConnection - OnDataReceived: Schreibe " + s7HeaderDataLen.ToString() + " Bytes in den Stream, PDU pos=" + pos.ToString());
                    m_ReceivedStream.Write(PDU, pos, s7HeaderDataLen);
                    pos += s7HeaderDataLen;

                    // Wenn dieses eine fragmentierte PDU ist, dann folgt jetzt kein Trailer mehr.
                    if ((len - 4 - 4) == s7HeaderDataLen)
                    {
                        // Console.WriteLine("S7CommPlusConnection - OnDataReceived: Fertig eingelesen.");
                        m_ReceivedNeedMorePdus = false;
                        m_ReceivedStream.Position = 0;    // Position wieder auf Null setzen, damit später ausgelesen werden kann
                        m_NewS7CommPlusReceived = true;
                    }
                    else
                    {
                        //Console.WriteLine("S7CommPlusConnection - OnDataReceived: Fragmentiert, brauche noch mehr PDUs.");
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
            // SystemLimits auslesen
            // Der eine Wertist max. tags to read, das andere tags to write.
            // Welche von beiden was ist, ist noch nicht klar. Bisher waren immer beide Werte identisch
            // Annahme bis zur Klärung: 1000 = Read, 1001 = Write
            int res;
            List<ItemAddress> readlist = new List<ItemAddress>();
            List<object> values = new List<object>();
            List<UInt64> errors = new List<UInt64>();

            ItemAddress adrMaxReadTags = new ItemAddress
            {
                AccessArea = Ids.ObjectRoot,
                AccessSubArea = Ids.SystemLimits
            };
            adrMaxReadTags.LID.Add(1000);
            ItemAddress adrMaxWriteTags = new ItemAddress
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
            Console.WriteLine("S7CommPlusConnection - ReadSystemLimits: m_MaxTagsPerReadRequestLimit=" + m_MaxTagsPerReadRequestLimit);
            Console.WriteLine("S7CommPlusConnection - ReadSystemLimits: m_MaxTagsPerWriteRequestLimit=" + m_MaxTagsPerWriteRequestLimit);
            return res;
        }

        private int checkResponseWithIntegrity(object responseObject, UInt16 requestSequenceNumber, UInt16 responseSequenceNumber, UInt32 requestIntegrity, UInt32 responseIntegrity)
        {
            if (responseObject == null)
            {
                Console.WriteLine("checkResponseWithIntegrity: FEHLER! responseObject == null");
                return S7Consts.errIsoInvalidPDU;
            }
            if (requestSequenceNumber != responseSequenceNumber)
            {
                Console.WriteLine(String.Format("checkResponseWithIntegrity: FEHLER! SeqenceNumber von Response ({0}) passt nicht zum Request ({1})", responseSequenceNumber, requestSequenceNumber));
                return S7Consts.errIsoInvalidPDU;
            }
            // Hier kann ein Overflow vorkommen, ist aber erlaubt und Ergebnis wird akzeptiert.
            UInt32 reqIntegCheck = (UInt32)requestSequenceNumber + requestIntegrity;
            if (responseIntegrity != reqIntegCheck)
            {  
                Console.WriteLine(String.Format("checkResponseWithIntegrity: FEHLER! Integrity der Response ({0}) passt nicht zum Request ({1})", responseIntegrity, reqIntegCheck));
                // Vorerst nicht als Fehler zurückgeben
            }
            return 0;
        }

        private int checkResponse(object responseObject, ushort requestSequenceNumber, ushort responseSequenceNumber)
        {
            if (responseObject == null)
            {
                Console.WriteLine("checkResponse: FEHLER! responseObject == null");
                return S7Consts.errIsoInvalidPDU;
            }
            if (requestSequenceNumber != responseSequenceNumber)
            {
                Console.WriteLine(String.Format("checkResponse: FEHLER! SeqenceNumber von Response ({0}) passt nicht zum Request ({1})", responseSequenceNumber, requestSequenceNumber));
                return S7Consts.errIsoInvalidPDU;
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

            #region Schritt 1: Unverschlüsselt InitSSL Request / Response
            
            // Ab jetzt den Thread starten
            InitSslRequest sslrequest = new InitSslRequest(ProtocolVersion.V1, GetNextSequenceNumber(), 0);
            res = SendS7plusFunctionObject(sslrequest);
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
            InitSslResponse sslresponse;
            sslresponse = InitSslResponse.DeserializeFromPdu(m_ReceivedStream);
            if (sslresponse == null)
            {
                Console.WriteLine("S7CommPlusConnection - Connect: InitSslResponse fehlerhaft");
                m_client.Disconnect();
                return m_LastError;
            }
            // Console.WriteLine(sslresponse.ToString());

            #endregion

            #region Schritt 2: SSL aktivieren, alles ab hier erfolgt die Übertragung TLS verschlüsselt

            res = m_client.SslActivate();
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
            }

            #endregion

            #region Schritt 3: CreateObjectRequest / Response (mit TLS)

            CreateObjectRequest createObjectRequest = new CreateObjectRequest(ProtocolVersion.V1, GetNextSequenceNumber(), Ids.ObjectNullServerSession);
            createObjectRequest.SetNullServerSessionData();
            res = SendS7plusFunctionObject(createObjectRequest);
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

            CreateObjectResponse createObjectResponse;
            createObjectResponse = CreateObjectResponse.DeserializeFromPdu(m_ReceivedStream);
            if (createObjectResponse == null)
            {
                Console.WriteLine("S7CommPlusConnection - Connect: CreateObjectResponse fehlerhaft");
                m_client.Disconnect();
                return S7Consts.errIsoInvalidPDU;
            }
            m_SessionId = createObjectResponse.ObjectIds[0];
            Console.WriteLine("S7CommPlusConnection - Connect: Verwende SessionId=0x" + String.Format("{0:X04}", m_SessionId));


            ////////////////////////////////////////////////
            // Struct 314 auswerten
            PValue sval = createObjectResponse.ResponseObject.GetAttribute(Ids.ServerSessionVersion);
            ValueStruct serverSession = (ValueStruct)sval;

            #endregion

            #region Schritt 4: SetMultiVariablesRequest / Response
            
            SetMultiVariablesRequest setMultiVariablesRequest = new SetMultiVariablesRequest(ProtocolVersion.V2);
            setMultiVariablesRequest.SetSessionSetupData(m_SessionId, serverSession);
            setMultiVariablesRequest.SequenceNumber = GetNextSequenceNumber();
            res = SendS7plusFunctionObject(setMultiVariablesRequest);
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

            SetMultiVariablesResponse setMultiVariablesResponse;
            setMultiVariablesResponse = SetMultiVariablesResponse.DeserializeFromPdu(m_ReceivedStream);
            if (setMultiVariablesResponse == null)
            {
                Console.WriteLine("S7CommPlusConnection - Connect: SetMultiVariablesResponse fehlerhaft");
                m_client.Disconnect();
                return S7Consts.errIsoInvalidPDU;
            }
            m_LastIntegrityId = setMultiVariablesResponse.IntegrityId;

            #endregion

            #region Schritt 5: SystemLimits auslesen
            res = ReadSystemLimits();
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
            }
            #endregion

            // Wenn bis hier her alles ohne Fehler, dann ist Verbindung erfolgreich aufgebaut
            Console.WriteLine("S7CommPlusConnection - Connect: Benötige Zeit für gesamten Verbindungsaufbau: " + (Environment.TickCount - Elapsed) + " ms.");
            return 0;
        }

        public void Disconnect()
        {
            m_client.Disconnect();
        }

        /*
         * public void ReadValues(
            IList<NodeId>           variableIds,
            IList<Type>             expectedTypes,
            out List<object>        values, 
            out List<ServiceResult> errors)
        */
        public int ReadValues(List<ItemAddress> addresslist, out List<object> values, out List<UInt64> errors)
        {
            // Der Anfragesteller muss den internen Typ schon mit der Anfrage übergeben,
            // sonst können nicht alle Rückgabewerte automatisch konvertiert werden.
            // Beispielsweise werden Strings als UInt-Array zurückgegeben.
            values = new List<object>();
            errors = new List<UInt64>();
            // values und errors initialisieren
            for (int i = 0; i < addresslist.Count; i++)
            {
                values.Add(null);
                errors.Add(0xffffffffffffffff); // Auf Fehler initialisieren
            }

            // Request in Teile aufsplitten
            int chunk_startIndex = 0;
            int count_perChunk = 0;
            do
            {
                int res;
                GetMultiVariablesRequest getMultiVariablesRequest = new GetMultiVariablesRequest(ProtocolVersion.V2);
                getMultiVariablesRequest.SessionId = m_SessionId;
                getMultiVariablesRequest.SequenceNumber = GetNextSequenceNumber();
                getMultiVariablesRequest.IntegrityId = GetNextIntegrityId();

                //getMultiVariablesRequest.AddressList = addresslist;
                getMultiVariablesRequest.AddressList.Clear();
                count_perChunk = 0;
                while (count_perChunk < m_MaxTagsPerReadRequestLimit && (chunk_startIndex + count_perChunk) < addresslist.Count)
                { 
                    getMultiVariablesRequest.AddressList.Add(addresslist[chunk_startIndex + count_perChunk]);
                    count_perChunk++;
                }

                res = SendS7plusFunctionObject(getMultiVariablesRequest);
                m_LastError = 0;
                WaitForNewS7plusReceived(m_ReadTimeout);
                if (m_LastError != 0)
                {
                    return m_LastError;
                }
                // Antwort auswerten
                GetMultiVariablesResponse getMultiVariablesResponse = GetMultiVariablesResponse.DeserializeFromPdu(m_ReceivedStream);
                res = checkResponseWithIntegrity(getMultiVariablesResponse,
                    getMultiVariablesRequest.SequenceNumber,
                    getMultiVariablesResponse.SequenceNumber,
                    getMultiVariablesRequest.IntegrityId,
                    getMultiVariablesResponse.IntegrityId);
                if (res != 0)
                {
                    return res;
                }
                // ReturnValue zeigt auch an wenn nur eine Variable von mehreren nicht gelesen werden konnte.
                if (getMultiVariablesResponse.ReturnValue != 0)
                {
                    Console.WriteLine("S7CommPlusConnection - ReadValues: Mit Fehler ausgeführt. ReturnValue=" + getMultiVariablesResponse.ReturnValue);
                }
                m_LastIntegrityId = getMultiVariablesResponse.IntegrityId;

                // TODO: Falls eine Variable nicht gelesen werden konnte, ist kein Value vorhanden, dafür dann aber ein ErrorValue.
                // Der Anwender muss darum prüfen, ob Value != null ist. Eventuell eleganter zu lösen.
                foreach (var v in getMultiVariablesResponse.Values)
                {
                    values[chunk_startIndex + (int)v.Key - 1] = v.Value;
                    // Error hier schon mal auf 0 zurücksetzen, wird ggf. unten überschrieben falls doch ein Fehler vorhanden war.
                    errors[chunk_startIndex + (int)v.Key - 1] = 0;
                }

                foreach (var ev in getMultiVariablesResponse.ErrorValues)
                {
                    errors[chunk_startIndex+ (int)ev.Key - 1] = ev.Value;
                }

                chunk_startIndex += count_perChunk;

            } while (chunk_startIndex < addresslist.Count);

            return m_LastError;
        }

        public int WriteValues(List<ItemAddress> addresslist, List<PValue> values, out List<UInt64> errors)
        {
            int res;

            errors = new List<UInt64>();
            // errors initialisieren
            for (int i = 0; i < addresslist.Count; i++)
            {
                errors.Add(0xffffffffffffffff); // Auf Fehler initialisieren
            }

            SetMultiVariablesRequest setMultiVariablesRequest = new SetMultiVariablesRequest(ProtocolVersion.V2);
            setMultiVariablesRequest.SessionId = m_SessionId;
            setMultiVariablesRequest.SequenceNumber = GetNextSequenceNumber();
            setMultiVariablesRequest.IntegrityId = GetNextIntegrityId();

            setMultiVariablesRequest.AddressListVar = addresslist;
            setMultiVariablesRequest.ValueList = values;

            res = SendS7plusFunctionObject(setMultiVariablesRequest);
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

            SetMultiVariablesResponse setMultiVariablesResponse;
            setMultiVariablesResponse = SetMultiVariablesResponse.DeserializeFromPdu(m_ReceivedStream);
            res = checkResponseWithIntegrity(setMultiVariablesResponse,
                    setMultiVariablesRequest.SequenceNumber,
                    setMultiVariablesResponse.SequenceNumber,
                    setMultiVariablesRequest.IntegrityId,
                    setMultiVariablesResponse.IntegrityId);
            if (res != 0)
            {
                return res;
            }
            // ReturnValue zeigt auch an wenn nur eine Variable von mehreren nicht geschrieben werden konnte.
            if (setMultiVariablesResponse.ReturnValue != 0)
            {
                Console.WriteLine("S7CommPlusConnection - WriteValues: Mit Fehler ausgeführt. ReturnValue=" + setMultiVariablesResponse.ReturnValue);
            }
            m_LastIntegrityId = setMultiVariablesResponse.IntegrityId;

            return m_LastError;
        }

        public int SetPlcOperatingState(Int32 state)
        {
            int res;

            SetVariableRequest setVariableRequest = new SetVariableRequest(ProtocolVersion.V2);
            setVariableRequest.SessionId = m_SessionId;
            setVariableRequest.SequenceNumber = GetNextSequenceNumber();
            setVariableRequest.IntegrityId = GetNextIntegrityId();

            setVariableRequest.InObjectId = 52; // NativeObjects.theCPUexecUnit_Rid
            setVariableRequest.Address = 2167; // CPUexecUnit.operatingStateREQ
            setVariableRequest.Value = new ValueDInt(state);

            res = SendS7plusFunctionObject(setVariableRequest);
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

            SetVariableResponse setVariableResponse;
            setVariableResponse = SetVariableResponse.DeserializeFromPdu(m_ReceivedStream);
            if (setVariableResponse == null)
            {
                Console.WriteLine("S7CommPlusConnection - Connect: SetVariableResponse fehlerhaft");
                m_client.Disconnect();
                return S7Consts.errIsoInvalidPDU;
            }
            m_LastIntegrityId = setVariableResponse.IntegrityId;
            return 0;
        }

        public int Browse(out List<VarInfo> varInfoList)
        {
            //Console.WriteLine("S7CommPlusConnection - Browse: Start");
            varInfoList = new List<VarInfo>();
            int res;

            Browser vars = new Browser();

            ExploreRequest exploreReq;
            ExploreResponse exploreRes;

            #region Alle Objekte auslesen
            //Console.WriteLine("S7CommPlusConnection - Browse: Alle Objekte auslesen");
            List<BrowseData> exploreData = new List<BrowseData>();

            exploreReq = new ExploreRequest(ProtocolVersion.V2);
            exploreReq.SessionId = m_SessionId;
            exploreReq.SequenceNumber = GetNextSequenceNumber();
            exploreReq.ExploreId = Ids.NativeObjects_thePLCProgram_Rid;
            exploreReq.ExploreRequestId = Ids.None;
            exploreReq.ExploreChildsRecursive = 1;
            exploreReq.ExploreParents = 0;
            exploreReq.IntegrityId = GetNextIntegrityId();

            // Diese Objektattribute werden mit abgefragt
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
            // Antwort auswerten
            exploreRes = ExploreResponse.DeserializeFromPdu(m_ReceivedStream, true);
            if ((exploreRes == null) ||
                (exploreRes.SequenceNumber != exploreReq.SequenceNumber) ||
                (exploreRes.ReturnValue != 0))
            {
                return S7Consts.errIsoInvalidPDU;
            }

            #endregion

            #region Alle Datenbausteine auswerten die anschließend gebrowst werden müssen
            
            List<PObject> objList = exploreRes.ResponseObject.GetObjects();

            foreach (var ob in objList)
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

            #region TypeInfo RID zur RelId aus erster Anfrage bestimmen

            // Mit Abfrage von LID=1 aus allen DBs bekommt man die RID zurück mit dem
            // die Typinformationen abgefragt werden können.
            // Das ist notwendig, weil z.B. bei Instanz-DBs (z.B. TON) an die Typinformationen
            // nicht über die RID des DBs sondern des TONs gelangt werden muss
            GetMultiVariablesRequest getMultiVariablesReq = new GetMultiVariablesRequest(ProtocolVersion.V2);
            getMultiVariablesReq.SessionId = m_SessionId;
            getMultiVariablesReq.SequenceNumber = GetNextSequenceNumber();
            getMultiVariablesReq.IntegrityId = GetNextIntegrityId();


            // TODO: WICHTIG!
            // Hier kann auch nur die maximale Anzahl an Variablen pro Read Request gelesen werden!
            // Es kann eigentlich direkt ReadValues verwendet werden.
            foreach (var data in exploreData)
            {
                if (data.db_number > 0)        // Merker usw. nicht abfragen
                {
                    // Variablenadresse einfügen
                    ItemAddress adr1 = new ItemAddress();
                    adr1.AccessArea = data.db_block_relid;
                    adr1.AccessSubArea = Ids.DB_ValueActual;
                    adr1.LID.Add(1);
                    // Variablenadresse dem Request hinzufügen
                    getMultiVariablesReq.AddressList.Add(adr1);
                }
            }
            res = SendS7plusFunctionObject(getMultiVariablesReq);
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
            // Antwort auswerten
            GetMultiVariablesResponse getMultiVariablesRes = GetMultiVariablesResponse.DeserializeFromPdu(m_ReceivedStream);
            if ((getMultiVariablesRes == null) ||
                (getMultiVariablesRes.SequenceNumber != getMultiVariablesReq.SequenceNumber) ||
                (getMultiVariablesRes.ReturnValue != 0))
            {
                return S7Consts.errIsoInvalidPDU;
            }
            #endregion

            #region Vorabinformationen zur Kombination an ExploreSymbols übergeben

            // Antworten in Liste eintragen
            foreach (var val in getMultiVariablesRes.Values)
            {
                int key = (int)(val.Key) - 1;
                ValueRID rid = (ValueRID)val.Value;
                var data = exploreData[key];
                data.db_block_ti_relid = rid.GetValue();
                exploreData[key] = data;
            }

            // Elemente mit db_block_ti_relid == 0 aus Liste löschen
            // Wenn die RID == 0 ist, dann kann diese nicht weiter verwendet werden!
            // Das kommt vor bei Datenbausteinen die nur im Ladespeicher abgelegt sind.
            exploreData.RemoveAll(item => item.db_block_ti_relid == 0);

            foreach (var ed in exploreData)
            {
                vars.AddBlockNode(eNodeType.Root, ed.db_name, ed.db_block_relid, ed.db_block_ti_relid);
            }

            // Merker usw. manuell hinzufügen.
            vars.AddBlockNode(eNodeType.Root, "IArea", Ids.NativeObjects_theIArea_Rid, 0x90010000);
            vars.AddBlockNode(eNodeType.Root, "QArea", Ids.NativeObjects_theQArea_Rid, 0x90020000);
            vars.AddBlockNode(eNodeType.Root, "MArea", Ids.NativeObjects_theMArea_Rid, 0x90030000);
            vars.AddBlockNode(eNodeType.Root, "S7Timers", Ids.NativeObjects_theS7Timers_Rid, 0x90050000);
            vars.AddBlockNode(eNodeType.Root, "S7Counters", Ids.NativeObjects_theS7Counters_Rid, 0x90060000);

            #endregion

            #region Type Info Container auslesen (große PDU, muss geprüft werden, ob das bei sehr großen Programmen praktikabel ist)
            ExploreRequest exploreRequest = new ExploreRequest(ProtocolVersion.V2);
            exploreRequest.SessionId = m_SessionId;
            exploreRequest.SequenceNumber = GetNextSequenceNumber();
            exploreRequest.IntegrityId = GetNextIntegrityId();

            exploreRequest.ExploreId = Ids.NativeObjects_thePLCProgram_Rid;

            // Mit AID.ObjectOMSTypeInfoContainer erhält man den kompletten Variablenhaushalt in einer großen PDU!
            exploreRequest.ExploreId = Ids.ObjectOMSTypeInfoContainer;

            exploreRequest.ExploreRequestId = Ids.None;
            exploreRequest.ExploreChildsRecursive = 1;
            exploreRequest.ExploreParents = 0;

            res = SendS7plusFunctionObject(exploreRequest);
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

            #region Antwort auswerten
            ExploreResponse exploreResponse = ExploreResponse.DeserializeFromPdu(m_ReceivedStream, true);
            if (exploreResponse != null)
            {
                if (exploreResponse.SequenceNumber != exploreRequest.SequenceNumber)
                {
                    Console.WriteLine("S7CommPlusConnection - Browse: ExploreResponse SequenceNumber stimmt nicht mit Request überein.");
                }
                else
                {
                    List<PObject> objs = exploreResponse.ResponseObject.GetObjectsByClassId(Ids.ClassTypeInfo);
                    
                    vars.SetTypeInfoContainerObjects(objs);
                    vars.BuildTree();
                    vars.BuildFlatList();
                    varInfoList = vars.GetVarInfoList();
                }
            }
            #endregion
            return 0;
        }

        public class BrowseEntry
        {
            public string Name;
            public uint Softdatatype;
            public UInt32 LID;
            public UInt32 SymbolCrc;
            public string AccessSequence;                                   // Zugriffsname wie er in WinCC angezeigt wird z.B. (8A0E0003.11)
        };

        public class BrowseData
        {
            public string db_name;                                          // Name des Datenbausteins
            public UInt32 db_number;                                        // Nummer des Datenbausteins
            public UInt32 db_block_relid;                                   // RID des Datenbausteins
            public UInt32 db_block_ti_relid;                                // Type-Info RID des Datenbausteins
            public List<BrowseEntry> variables = new List<BrowseEntry>(); // Variablen des Datenbausteins
        };
        #endregion
    }
}
