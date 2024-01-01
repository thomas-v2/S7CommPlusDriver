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
    public class DeleteObjectResponse : IS7pResponse
    {
        public byte TransportFlags;
        public UInt64 ReturnValue;
        public UInt32 DeleteObjectId;

        public byte ProtocolVersion { get; set; }
        public ushort FunctionCode { get => Functioncode.DeleteObject; }
        public ushort SequenceNumber { get; set; }
        public uint IntegrityId { get; set; }
        public bool WithIntegrityId { get; set; }

        public DeleteObjectResponse(byte protocolVersion, bool withIntegrityId)
        {
            ProtocolVersion = protocolVersion;
            WithIntegrityId = withIntegrityId; // When deleting the Sesssion Object-Id, there's no Integrity-Id!
        }

        public int Deserialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.DecodeUInt16(buffer, out ushort seqnr);
            SequenceNumber = seqnr;
            ret += S7p.DecodeByte(buffer, out TransportFlags);

            // Response Set
            ret += S7p.DecodeUInt64Vlq(buffer, out ReturnValue);
            ret += S7p.DecodeUInt32(buffer, out DeleteObjectId);
            if ((ReturnValue & 0x4000000000000000) > 0) // Error Extension
            {
                // Decode the error object, but don't use any informations from it. Must be processed on a higher level.
                PObject errorObject = new PObject();
                ret += S7p.DecodeObject(buffer, ref errorObject);
            }
            if (WithIntegrityId)
            {
                ret += S7p.DecodeUInt32Vlq(buffer, out uint iid);
                IntegrityId = iid;
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
            s += "<WithIntegrityId>" + WithIntegrityId.ToString() + "</WithIntegrityId>" + Environment.NewLine;
            s += "<IntegrityId>" + IntegrityId.ToString() + "</IntegrityId>" + Environment.NewLine;
            s += "</DeleteObjectResponse>" + Environment.NewLine;
            return s;
        }

        public static DeleteObjectResponse DeserializeFromPdu(Stream pdu, bool withIntegrityId)
        {
            byte protocolVersion;
            byte opcode;
            UInt16 function;
            UInt16 reserved;
            // Special handling of ProtocolVersion, which is written to the stream before
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
            DeleteObjectResponse resp = new DeleteObjectResponse(protocolVersion, withIntegrityId);
            resp.Deserialize(pdu);

            return resp;
        }
    }
}
