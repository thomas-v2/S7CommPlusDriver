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
using System.IO;

namespace S7CommPlusDriver.Alarming
{
    public class AlarmsMultipleStai
    {
        public ushort Alid;
        public ushort AlarmDomain; // 1=Systemdiagnose, 2=Security, 256..272 = UserClass_0..UserClass_16
        public ushort MessageType; // 1=Alarm AP, 2=Notify AP, 3=Info Report AP, 4=Event Ack AP
        public byte AlarmEnabled; //0=No, 1=Yes

        public ushort HmiInfoLength;
        public AlarmsHmiInfo HmiInfo;
        public ushort LidCount;
        public uint[] Lids;

        public override string ToString()
        {
            string s = "";
            s += "<AlarmsMultipleStai>" + Environment.NewLine;
            s += "<Alid>" + Alid.ToString() + "</Alid>" + Environment.NewLine;
            s += "<AlarmDomain>" + AlarmDomain.ToString() + "</AlarmDomain>" + Environment.NewLine;
            s += "<MessageType>" + MessageType.ToString() + "</MessageType>" + Environment.NewLine;
            s += "<AlarmEnabled>" + AlarmEnabled.ToString() + "</AlarmEnabled>" + Environment.NewLine;
            s += "<HmiInfoLength>" + HmiInfoLength.ToString() + "</HmiInfoLength>" + Environment.NewLine;
            s += "<HmiInfo>" + Environment.NewLine + HmiInfo.ToString() + "</HmiInfo>" + Environment.NewLine;
            s += "<LidCount>" + LidCount.ToString() + "</LidCount>" + Environment.NewLine;
            foreach (var li in Lids)
            {
                s += "<Lid>" + li.ToString() + "</Lid>" + Environment.NewLine;
            }
            s += "</AlarmsMultipleStai>" + Environment.NewLine;
            return s;
        }

        public int Deserialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.DecodeUInt16(buffer, out Alid);
            ret += S7p.DecodeUInt16(buffer, out AlarmDomain);
            ret += S7p.DecodeUInt16(buffer, out MessageType);
            ret += S7p.DecodeByte(buffer, out AlarmEnabled);
            ret += S7p.DecodeUInt16(buffer, out HmiInfoLength);
            HmiInfo = new AlarmsHmiInfo();
            ret += HmiInfo.Deserialize(buffer);
            ret += S7p.DecodeUInt16(buffer, out LidCount);
            Lids = new uint[LidCount];
            for (int i = 0; i < LidCount; i++)
            {
                ret += S7p.DecodeUInt32(buffer, out Lids[i]);
            }
            return ret;
        }
    }
}
