﻿#region License
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

namespace S7CommPlusDriver
{
    class InitSslResponse
    {
        public byte ProtocolVersion;
        public UInt16 SequenceNumber;
        public byte TransportFlags;
        public UInt64 ReturnValue;
        public InitSslResponse(byte protocolVersion)
        {
            ProtocolVersion = protocolVersion;
        }

        public int Deserialize(Stream buffer)
        {
            int ret = 0;

            ret += S7p.DecodeUInt16(buffer, out SequenceNumber);
            ret += S7p.DecodeByte(buffer, out TransportFlags);

            // Response Set
            ret += S7p.DecodeUInt64Vlq(buffer, out ReturnValue);
            if ((ReturnValue & 0x4000000000000000) > 0) // Error Extension
            {
                // Objekt nur dekodieren, aber z.Zt. nicht weiter nutzen weil Funktion unbekannt
                // evtl. gehört das Objekt zur Fehlermeldung an sich um mehr Details zum Fehler zu übermitteln.
                PObject errorObject = new PObject();
                ret += S7p.DecodeObject(buffer, ref errorObject);
            }

            return ret;
        }

        public override string ToString()
        {
            string s = "";
            s += "<InitSslResponse>" + Environment.NewLine;
            s += "<ProtocolVersion>" + ProtocolVersion.ToString() + "</ProtocolVersion>" + Environment.NewLine;
            s += "<SequenceNumber>" + SequenceNumber.ToString() + "</SequenceNumber>" + Environment.NewLine;
            s += "<TransportFlags>" + TransportFlags.ToString() + "</TransportFlags>" + Environment.NewLine;
            s += "<ResponseSet>" + Environment.NewLine;
            s += "<ReturnValue>" + ReturnValue.ToString() + "</ReturnValue>" + Environment.NewLine;
            s += "</ResponseSet>" + Environment.NewLine;
            s += "</InitSslResponse>" + Environment.NewLine;
            return s;
        }

        public static InitSslResponse DeserializeFromPdu(Stream pdu)
        {
            byte protocolVersion;
            byte opcode;
            UInt16 function;
            UInt16 reserved;
            // ProtocolVersion wird vorab als ein Byte in den Stream geschrieben, Sonderbehandlung
            S7p.DecodeByte(pdu, out protocolVersion);
            S7p.DecodeByte(pdu, out opcode);
            if (opcode != Opcode.Response)
            {
                return null;
            }
            S7p.DecodeUInt16(pdu, out reserved);
            S7p.DecodeUInt16(pdu, out function);
            S7p.DecodeUInt16(pdu, out reserved);
            if (function != Functioncode.InitSsl)
            {
                return null;
            }
            InitSslResponse resp = new InitSslResponse(protocolVersion);
            resp.Deserialize(pdu);

            return resp;
        }
    }
}
