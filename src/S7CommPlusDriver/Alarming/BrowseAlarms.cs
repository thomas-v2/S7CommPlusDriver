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
using System.Diagnostics;
using System.IO;

namespace S7CommPlusDriver
{
    public partial class S7CommPlusConnection
    {
        /// <summary>
        /// Explores the AS and the User program alarms, gets the corresponding texts usind the language id (e.g.1031 = de-DE)
        /// 
        /// Call example:
        /// CultureInfo ci = new CultureInfo("de-DE");
        /// Dictionary<ulong, AlarmData> Alarms = new Dictionary<ulong, AlarmData>();
        /// conn.ExploreASAlarms(ref Alarms, ci.LCID);
        /// foreach (var al in Alarms)
        /// {
        ///     Console.WriteLine(al.Value.ToString());
        /// }
        /// </summary>
        /// <param name="Alarms">Dictionary <ulong, AlarmData> where the results are written to. Key is used as address.</param>
        /// <param name="languageId">Language id for retrieving the text entries, use language code e.g. 1031 for german</param>
        /// <returns></returns>
        public int ExploreASAlarms(ref Dictionary<ulong, AlarmData> Alarms, int languageId)
        {
            int res;

            #region Explore all other than Alarm AP (AnwenderProgramAlarme)
            var exploreReq = new ExploreRequest(ProtocolVersion.V2);
            exploreReq.ExploreId = 0x8a7e0000; // ASAlarms.0
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
            List<PObject> staiclass = exploreRes.ResponseObject.GetObjectsByClassId(7854); // 7854 = MultipleSTAI.Class_Rid
            if (staiclass != null && staiclass.Count > 0)
            {
                PValue stais = staiclass[0].GetAttribute(7859); // 7859 = MultipleSTAI.STAIs
                if (stais != null)
                {
                    if (stais.GetType() == typeof(ValueBlobSparseArray))
                    {
                        var dict = ((ValueBlobSparseArray)stais).GetValue();
                        foreach (var entry in dict)
                        {
                            var alarm = new AlarmData(0x8a7e0000); // TODO: Get this from parent object?
                            Stream buffer = new MemoryStream(entry.Value.value);
                            alarm.Deserialize(buffer);
                            Alarms.Add(alarm.GetCpuAlarmId(), alarm);
                        }
                    }
                    else
                    {
                        Console.WriteLine("ExploreASAlarms(): stais is not ValueBlobSparseArray");
                    }
                }
                else
                {
                    Console.WriteLine("ExploreASAlarms(): stais = null");

                }
            }
            else
            {
                Console.WriteLine("ExploreASAlarms(): staiclass = null");

            }
            #endregion

            #region Explore Alarm AP
            exploreReq = new ExploreRequest(ProtocolVersion.V2);
            exploreReq.ExploreId = Ids.NativeObjects_thePLCProgram_Rid;
            exploreReq.ExploreRequestId = Ids.None;
            exploreReq.ExploreChildsRecursive = 1;
            exploreReq.ExploreParents = 0;

            // Add the requestes attributes
            exploreReq.AddressList.Add(229); // 229 = ObjectVariableTypeParentObject
            exploreReq.AddressList.Add(7859); // 7859 = MultipleSTAI.STAIs

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
            if ((exploreRes == null) ||
                (exploreRes.SequenceNumber != exploreReq.SequenceNumber) ||
                (exploreRes.ReturnValue != 0))
            {
                return S7Consts.errIsoInvalidPDU;
            }

            // All objects which have Alarm AP inside, have a sub-Object with ID 7854 = MultipleSTAI.Class_Rid
            List<PObject> objList = exploreRes.ResponseObject.GetObjects();

            foreach (var ob in objList)
            {
                staiclass = ob.GetObjectsByClassId(7854); // 7854 = MultipleSTAI.Class_Rid
                if (staiclass != null && staiclass.Count > 0)
                {
                    PValue stais = staiclass[0].GetAttribute(7859); // 7859 = MultipleSTAI.STAIs
                    if (stais != null)
                    {
                        if (stais.GetType() == typeof(ValueBlobSparseArray))
                        {
                            var dict = ((ValueBlobSparseArray)stais).GetValue();
                            foreach (var entry in dict)
                            {
                                var alarm = new AlarmData(ob.RelationId);
                                Stream buffer = new MemoryStream(entry.Value.value);

                                alarm.Deserialize(buffer);
                                Alarms.Add(alarm.GetCpuAlarmId(), alarm);
                            }
                        }
                        else
                        {
                            Console.WriteLine("ExploreASAlarms(): stais is not ValueBlobSparseArray");
                        }
                    }
                    else
                    {
                        Console.WriteLine("ExploreASAlarms(): stais = null");
                    }
                }
            }
            #endregion

            #region Explore AlarmTextLists

            exploreReq = new ExploreRequest(ProtocolVersion.V2);
            exploreReq.ExploreId = 0x8a360000 + (ushort)languageId; // There may be several languages, add language ID (e.g. 1031 = german / de-DE)
            exploreReq.ExploreRequestId = Ids.None;
            exploreReq.ExploreChildsRecursive = 0;
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

            exploreRes = ExploreResponse.DeserializeFromPdu(m_ReceivedStream, true);
            if ((exploreRes == null) ||
                (exploreRes.SequenceNumber != exploreReq.SequenceNumber) ||
                (exploreRes.ReturnValue != 0))
            {
                return S7Consts.errIsoInvalidPDU;
            }

            // TextLibraryOffsetArea is an array[3] of Blob, which contains offset information.
            // [0] contains informations of how to get infos from [1], which has infos about offsets in [2].
            // [2] contains the resulting offsets for the strings in TextLibraryStringArea.
            // TODO: Check if all fields are present, or use try/catch?

            var tloa = ((ValueBlobArray)exploreRes.ResponseObject.GetAttribute(608)).GetValue(); // 608 = TextLibraryOffsetArea
            var tlsa = ((ValueBlob)exploreRes.ResponseObject.GetAttribute(609)).GetValue(); // 609 = TextLibraryStringArea

            var tloa_1 = tloa[0].GetValue();
            var tloa_2 = tloa[1].GetValue();
            var tloa_3 = tloa[2].GetValue();

            GetTexts(tloa_1, tloa_2, tloa_3, tlsa, ref Alarms);

            #endregion

            return 0;
        }

        private void GetTexts(byte[] tloa_1, byte[] tloa_2, byte[] tloa_3, byte[] tlsa, ref Dictionary<ulong, AlarmData> Alarms)
        {
            uint pos1, pos2, pos3;
            uint t1_count, t1_relid, t1_relid_off;
            uint t2_count, t2_off;
            ushort t2_alid;
            uint t3_count, t3_off;
            byte t3_typeindex;

            ushort ts_len;
            string ts_s;

            ulong cpualarmid;

            // Step 1: Get RelationId from table 1
            pos1 = 16; // What is in the bytes before 16 is not known.

            t1_count = GetUInt32(tloa_1, pos1);
            pos1 += 4;

            for (int i = 0; i < t1_count; i++)
            {
                t1_relid = GetUInt32(tloa_1, pos1);
                pos1 += 4;
                t1_relid_off = GetUInt32(tloa_1, pos1);
                pos1 += 4;

                // Step 2: Start offset in table 3
                pos2 = t1_relid_off;
                t2_count = GetUInt32(tloa_2, pos2);
                pos2 += 4;
                for (int j = 0; j < t2_count; j++)
                {
                    t2_alid = GetUInt16(tloa_2, pos2);
                    pos2 += 2;
                    t2_off = GetUInt32(tloa_2, pos2);
                    pos2 += 4;
                    // This ID is used as unique identifier (address), which is also transferred later on alarm notification
                    cpualarmid = (ulong)t1_relid << 32 | (ulong)t2_alid << 16;

                    // Check if we have the key stored from response data before, to have storage where we can put the Text information in
                    if (Alarms.ContainsKey(cpualarmid) == false)
                    {
                        Trace.WriteLine(String.Format("BrowseAlarms GetTexts(): CPU Alarm Id {0:X} is not in dictionary!", cpualarmid));
                        continue;
                    }

                    // Step 3: Get offsets to text array from table 3
                    pos3 = t2_off;
                    t3_count = GetUInt32(tloa_3, pos3);
                    pos3 += 4;
                    for (int k = 0; k < t3_count; k++)
                    {
                        // t3_typeindex:
                        // 0 = Infotext, 1 = AlarmText, 2..10 = AdditionalText1..AdditionalText9, 255 = Unknown 1 or 2 values
                        t3_typeindex = GetUInt8(tloa_3, pos3);
                        pos3 += 1;
                        t3_off = GetUInt32(tloa_3, pos3);
                        pos3 += 4;

                        // Step 4: Finally get the text and store the data
                        if (t3_typeindex == 255)
                        {
                            Alarms[cpualarmid].AlText.UnknownValue1 = GetUInt16(tlsa, t3_off);
                            Alarms[cpualarmid].AlText.UnknownValue2 = GetUInt16(tlsa, t3_off + 2);
                        }
                        else
                        {
                            ts_len = GetUInt16(tlsa, t3_off);
                            ts_s = GetString(tlsa, t3_off + 2, ts_len);
                            switch (t3_typeindex)
                            {
                                case 0:
                                    Alarms[cpualarmid].AlText.Infotext = ts_s;
                                    break;
                                case 1:
                                    Alarms[cpualarmid].AlText.AlarmText = ts_s;
                                    break;
                                case 2:
                                    Alarms[cpualarmid].AlText.AdditionalText1 = ts_s;
                                    break;
                                case 3:
                                    Alarms[cpualarmid].AlText.AdditionalText2 = ts_s;
                                    break;
                                case 4:
                                    Alarms[cpualarmid].AlText.AdditionalText3 = ts_s;
                                    break;
                                case 5:
                                    Alarms[cpualarmid].AlText.AdditionalText4 = ts_s;
                                    break;
                                case 6:
                                    Alarms[cpualarmid].AlText.AdditionalText5 = ts_s;
                                    break;
                                case 7:
                                    Alarms[cpualarmid].AlText.AdditionalText6 = ts_s;
                                    break;
                                case 8:
                                    Alarms[cpualarmid].AlText.AdditionalText7 = ts_s;
                                    break;
                                case 9:
                                    Alarms[cpualarmid].AlText.AdditionalText8 = ts_s;
                                    break;
                                case 10:
                                    Alarms[cpualarmid].AlText.AdditionalText9 = ts_s;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private byte GetUInt8(byte[] array, uint pos)
        {
            return array[pos];
        }

        private ushort GetUInt16(byte[] array, uint pos)
        {
            return (ushort)(array[pos + 1] * 256 + array[pos]);
        }

        private uint GetUInt32(byte[] array, uint pos)
        {
            return (uint)array[pos + 3] * 16777216 + (uint)array[pos + 2] * 65536 + (uint)array[pos + 1] * 256 + (uint)array[pos];
        }

        private String GetString(byte[] array, uint pos, uint len)
        {
            return System.Text.Encoding.UTF8.GetString(array, (int)pos, (int)len);
        }
    }

    public class AlarmTexts
    {
        public string Infotext;
        public string AlarmText;
        public string AdditionalText1;
        public string AdditionalText2;
        public string AdditionalText3;
        public string AdditionalText4;
        public string AdditionalText5;
        public string AdditionalText6;
        public string AdditionalText7;
        public string AdditionalText8;
        public string AdditionalText9;

        public ushort UnknownValue1;
        public ushort UnknownValue2;
    }

    public class AlarmData
    {
        public AlarmData(uint relationid)
        {
            RelationId = relationid;
        }

        public ulong GetCpuAlarmId()
        {
            return ((ulong)RelationId << 32) | ((ulong)Alid << 16);
        }
        
        public uint RelationId;

        public ushort Alid;
        public ushort AlarmDomain; // 1=Systemdiagnose, 2=Security, 256..272 = UserClass_0..UserClass_16
        public ushort MessageType; // 1=Alarm AP, 2=Notify AP, 3=Info Report AP, 4=Event Ack AP
        public byte AlarmEnabled; //0=No, 1=Yes
        
        public ushort HmiInfoLength;
        public ushort HmiInfo_SyntaxId;
        public ushort HmiInfo_Version;
        public uint HmiInfo_ClientAlarmId;
        public byte HmiInfo_Priority;
        // HmiInfo_SyntaxId >= 258 with HmiInfoLength >= 17
        public ushort HmiInfo_AlarmClass;
        public byte HmiInfo_Producer;
        public byte HmiInfo_GroupId;
        public byte HmiInfo_Flags;

        public ushort LidCount;
        public uint[] Lids;
  
        public AlarmTexts AlText = new AlarmTexts();

        public int Deserialize(Stream buffer)
        {
            int ret = 0;

            ret += S7p.DecodeUInt16(buffer, out Alid);
            ret += S7p.DecodeUInt16(buffer, out AlarmDomain);
            ret += S7p.DecodeUInt16(buffer, out MessageType);
            ret += S7p.DecodeByte(buffer, out AlarmEnabled);

            ret += S7p.DecodeUInt16(buffer, out HmiInfoLength);

            ret += S7p.DecodeUInt16(buffer, out HmiInfo_SyntaxId);
            ret += S7p.DecodeUInt16(buffer, out HmiInfo_Version);
            ret += S7p.DecodeUInt32(buffer, out HmiInfo_ClientAlarmId);
            ret += S7p.DecodeByte(buffer, out HmiInfo_Priority);
            if (HmiInfo_SyntaxId >= 257) {
                // Skip 3 bytes with no useful data
                ret += S7p.DecodeByte(buffer, out _);
                ret += S7p.DecodeByte(buffer, out _);
                ret += S7p.DecodeByte(buffer, out _);
                if (HmiInfo_SyntaxId >= 258)
                {
                    ret += S7p.DecodeUInt16(buffer, out HmiInfo_AlarmClass);
                    ret += S7p.DecodeByte(buffer, out HmiInfo_Producer);
                    ret += S7p.DecodeByte(buffer, out HmiInfo_GroupId);
                    ret += S7p.DecodeByte(buffer, out HmiInfo_Flags);
                }
            }
            ret += S7p.DecodeUInt16(buffer, out LidCount);
            Lids = new uint[LidCount];
            for (int i = 0; i < LidCount; i++)
            {
                ret += S7p.DecodeUInt32(buffer, out Lids[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "";
            s += "<AlarmData>" + Environment.NewLine;
            s += "<CpuAlarmId>" + GetCpuAlarmId().ToString() + "</CpuAlarmId>" + Environment.NewLine;
            s += "<RelationId>" + RelationId.ToString() + "</RelationId>" + Environment.NewLine;
            s += "<Alid>" + Alid.ToString() + "</Alid>" + Environment.NewLine;
            s += "<AlarmDomain>" + AlarmDomain.ToString() + "</AlarmDomain>" + Environment.NewLine;
            s += "<MessageType>" + MessageType.ToString() + "</MessageType>" + Environment.NewLine;
            s += "<AlarmEnabled>" + AlarmEnabled.ToString() + "</AlarmEnabled>" + Environment.NewLine;
            s += "<HmiInfoLength>" + HmiInfoLength.ToString() + "</HmiInfoLength>" + Environment.NewLine;
            s += "<HmiInfo_SyntaxId>" + HmiInfo_SyntaxId.ToString() + "</HmiInfo_SyntaxId>" + Environment.NewLine;
            s += "<HmiInfo_Version>" + HmiInfo_Version.ToString() + "</HmiInfo_Version>" + Environment.NewLine;
            s += "<HmiInfo_ClientAlarmId>" + HmiInfo_ClientAlarmId.ToString() + "</HmiInfo_ClientAlarmId>" + Environment.NewLine;
            s += "<HmiInfo_Priority>" + HmiInfo_Priority.ToString() + "</HmiInfo_Priority>" + Environment.NewLine;
            if (HmiInfo_SyntaxId >= 258)
            {
                s += "<HmiInfo_AlarmClass>" + HmiInfo_AlarmClass.ToString() + "</HmiInfo_AlarmClass>" + Environment.NewLine;
                s += "<HmiInfo_Producer>" + HmiInfo_Producer.ToString() + "</HmiInfo_Producer>" + Environment.NewLine;
                s += "<HmiInfo_GroupId>" + HmiInfo_GroupId.ToString() + "</HmiInfo_GroupId>" + Environment.NewLine;
                s += "<HmiInfo_Flags>" + HmiInfo_Flags.ToString() + "</HmiInfo_Flags>" + Environment.NewLine;
            }
            s += "<LidCount>" + LidCount.ToString() + "</LidCount>" + Environment.NewLine;
            foreach (var li in Lids)
            {
                s += "<Lid>" + li.ToString() + "</Lid>" + Environment.NewLine;
            }
            s += "<AlText>" + Environment.NewLine;
            s += "<Infotext>" + AlText.Infotext + "</Infotext>" + Environment.NewLine;
            s += "<AlarmText>" + AlText.AlarmText + "</AlarmText>" + Environment.NewLine;
            s += "<AdditionalText1>" + AlText.AdditionalText1 + "</AdditionalText1>" + Environment.NewLine;
            s += "<AdditionalText2>" + AlText.AdditionalText2 + "</AdditionalText2>" + Environment.NewLine;
            s += "<AdditionalText3>" + AlText.AdditionalText3 + "</AdditionalText3>" + Environment.NewLine;
            s += "<AdditionalText4>" + AlText.AdditionalText4 + "</AdditionalText4>" + Environment.NewLine;
            s += "<AdditionalText5>" + AlText.AdditionalText5 + "</AdditionalText5>" + Environment.NewLine;
            s += "<AdditionalText6>" + AlText.AdditionalText6 + "</AdditionalText6>" + Environment.NewLine;
            s += "<AdditionalText7>" + AlText.AdditionalText7 + "</AdditionalText7>" + Environment.NewLine;
            s += "<AdditionalText8>" + AlText.AdditionalText8 + "</AdditionalText8>" + Environment.NewLine;
            s += "<AdditionalText9>" + AlText.AdditionalText9 + "</AdditionalText9>" + Environment.NewLine;
            s += "<UnknownValue1>" + AlText.UnknownValue1.ToString() + "</UnknownValue1>" + Environment.NewLine;
            s += "<UnknownValue2>" + AlText.UnknownValue2.ToString() + "</UnknownValue2>" + Environment.NewLine;
            s += "</AlText>" + Environment.NewLine;
            s += "</AlarmData>" + Environment.NewLine;
            return s;
        }
    }
}
