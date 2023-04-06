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

namespace S7CommPlusDriver
{
    public class DeleteObjectResponse
    {
        public byte ProtocolVersion;

        public UInt16 SequenceNumber;
        public byte TransportFlags;

        public UInt64 ReturnValue;
        public UInt32 DeleteObjectId;

        public DeleteObjectResponse(byte protocolVersion)
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
            ret += S7p.DecodeUInt32Vlq(buffer, out DeleteObjectId);
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
            s += "<DeleteObjectResponse>" + Environment.NewLine;
            s += "<ProtocolVersion>" + ProtocolVersion.ToString() + "</ProtocolVersion>" + Environment.NewLine;
            s += "<SequenceNumber>" + SequenceNumber.ToString() + "</SequenceNumber>" + Environment.NewLine;
            s += "<TransportFlags>" + TransportFlags.ToString() + "</TransportFlags>" + Environment.NewLine;
            s += "<ResponseSet>" + Environment.NewLine;
            s += "<ReturnValue>" + ReturnValue.ToString() + "</ReturnValue>" + Environment.NewLine;
            s += "<DeleteObjectId>" + DeleteObjectId.ToString() + "</DeleteObjectId>" + Environment.NewLine;
            s += "</ResponseSet>" + Environment.NewLine;
            s += "</DeleteObjectResponse>" + Environment.NewLine;
            return s;
        }

        public static DeleteObjectResponse DeserializeFromPdu(Stream pdu)
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
            if (function != Functioncode.DeleteObject)
            {
                return null;
            }
            DeleteObjectResponse resp = new DeleteObjectResponse(protocolVersion);
            resp.Deserialize(pdu);

            return resp;
        }
    }
}
