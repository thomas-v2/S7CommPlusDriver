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
    //  7813 = DAI.HmiInfo
    public class AlarmsHmiInfo
    {
        public ushort SyntaxId;
        public ushort Version;
        public uint ClientAlarmId;
        public byte Priority;
        public byte Reserved1;
        public byte Reserved2;
        public byte Reserved3;
        public ushort AlarmClass;
        public byte Producer;
        public byte GroupId;
        public byte Flags;

        public override string ToString()
        {
            string s = "<AlarmsHmiInfo>" + Environment.NewLine;
            s += "<SyntaxId>" + SyntaxId.ToString() + "</SyntaxId>" + Environment.NewLine;
            s += "<Version>" + Version.ToString() + "</Version>" + Environment.NewLine;
            s += "<ClientAlarmId>" + ClientAlarmId.ToString() + "</ClientAlarmId>" + Environment.NewLine;
            s += "<Priority>" + Priority.ToString() + "</Priority>" + Environment.NewLine;
            if (SyntaxId >= 257)
            {
                s += "<Reserved1>" + Reserved1.ToString() + "</Reserved1>" + Environment.NewLine;
                s += "<Reserved2>" + Reserved2.ToString() + "</Reserved2>" + Environment.NewLine;
                s += "<Reserved3>" + Reserved3.ToString() + "</Reserved3>" + Environment.NewLine;
                if (SyntaxId >= 258)
                {
                    s += "<AlarmClass>" + AlarmClass.ToString() + "</AlarmClass>" + Environment.NewLine;
                    s += "<Producer>" + Producer.ToString() + "</Producer>" + Environment.NewLine;
                    s += "<GroupId>" + GroupId.ToString() + "</GroupId>" + Environment.NewLine;
                    s += "<Flags>" + Flags.ToString() + "</Flags>" + Environment.NewLine;
                }
            }
            s += "</AlarmsHmiInfo>" + Environment.NewLine;
            return s;
        }

        public int Deserialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.DecodeUInt16(buffer, out SyntaxId);
            ret += S7p.DecodeUInt16(buffer, out Version);
            ret += S7p.DecodeUInt32(buffer, out ClientAlarmId);
            ret += S7p.DecodeByte(buffer, out Priority);
            if (SyntaxId >= 257)
            {
                ret += S7p.DecodeByte(buffer, out Reserved1);
                ret += S7p.DecodeByte(buffer, out Reserved2);
                ret += S7p.DecodeByte(buffer, out Reserved3);
                if (SyntaxId >= 258)
                {
                    ret += S7p.DecodeUInt16(buffer, out AlarmClass);
                    ret += S7p.DecodeByte(buffer, out Producer);
                    ret += S7p.DecodeByte(buffer, out GroupId);
                    ret += S7p.DecodeByte(buffer, out Flags);
                }
            }
            return ret;
        }

        public static AlarmsHmiInfo FromValueBlob(ValueBlob blob)
        {
            var hmiinfo = new AlarmsHmiInfo();
            var barr = blob.GetValue();
            uint pos = 0;
            hmiinfo.SyntaxId = Utils.GetUInt16(barr, pos);
            pos += 2;
            hmiinfo.Version = Utils.GetUInt16(barr, pos);
            pos += 2;
            hmiinfo.ClientAlarmId = Utils.GetUInt32(barr, pos);
            pos += 4;
            hmiinfo.Priority = Utils.GetUInt8(barr, pos);
            pos += 1;
            if (hmiinfo.SyntaxId >= 257)
            {
                hmiinfo.Reserved1 = Utils.GetUInt8(barr, pos);
                pos += 1;
                hmiinfo.Reserved2 = Utils.GetUInt8(barr, pos);
                pos += 1;
                hmiinfo.Reserved3 = Utils.GetUInt8(barr, pos);
                pos += 1;
                if (hmiinfo.SyntaxId >= 258)
                {
                    hmiinfo.AlarmClass = Utils.GetUInt16(barr, pos);
                    pos += 2;
                    hmiinfo.Producer = Utils.GetUInt8(barr, pos);
                    pos += 1;
                    hmiinfo.GroupId = Utils.GetUInt8(barr, pos);
                    pos += 1;
                    hmiinfo.Flags = Utils.GetUInt8(barr, pos);
                    pos += 1;
                }
            }
            return hmiinfo;
        }
    }
}
