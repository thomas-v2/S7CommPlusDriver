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
using System.IO;

namespace S7CommPlusDriver
{
    public class CreateObjectResponse : IS7pResponse
    {
        public byte TransportFlags;
        public UInt64 ReturnValue;
        public byte ObjectIdCount;
        public List<UInt32> ObjectIds;
        public PObject ResponseObject;

        public byte ProtocolVersion { get; set; }
        public ushort FunctionCode { get => Functioncode.CreateObject; }
        public ushort SequenceNumber { get; set; }
        public uint IntegrityId { get; set; }
        public bool WithIntegrityId { get; set; }

        public CreateObjectResponse(byte protocolVersion)
        {
            ProtocolVersion = protocolVersion;
            WithIntegrityId = false;
        }

        public int Deserialize(Stream buffer)
        {
            int ret = 0;
            UInt32 object_id = 0;
            ret += S7p.DecodeUInt16(buffer, out ushort seqnr);
            SequenceNumber = seqnr;
            ret += S7p.DecodeByte(buffer, out TransportFlags);

            // Response Set
            ret += S7p.DecodeUInt64Vlq(buffer, out ReturnValue);
            ret += S7p.DecodeByte(buffer, out ObjectIdCount);

            ObjectIds = new List<uint>(ObjectIdCount);
            for (int i = 0; i < ObjectIdCount; i++)
            {
                ret += S7p.DecodeUInt32Vlq(buffer, out object_id);
                ObjectIds.Add(object_id);
            }
            ret += S7p.DecodeObject(buffer, ref ResponseObject);
            return ret;
        }

        public override string ToString()
        {
            string s = "";
            s += "<CreateObjectResponse>" + Environment.NewLine;
            s += "<ProtocolVersion>" + ProtocolVersion.ToString() + "</ProtocolVersion>" + Environment.NewLine;
            s += "<SequenceNumber>" + SequenceNumber.ToString() + "</SequenceNumber>" + Environment.NewLine;
            s += "<TransportFlags>" + TransportFlags.ToString() + "</TransportFlags>" + Environment.NewLine;
            s += "<ResponseSet>" + Environment.NewLine;
            s += "<ReturnValue>" + ReturnValue.ToString() + "</ReturnValue>" + Environment.NewLine;
            s += "<ObjectIdCount>" + ObjectIdCount.ToString() + "</ObjectIdCount>" + Environment.NewLine;
            foreach (UInt32 id in ObjectIds)
            {
                s += "<ObjectId>" + id.ToString() + "</ObjectId>" + Environment.NewLine;
            }
            s += "<ResponseObject>" + ResponseObject.ToString() + "</ResponseObject>" + Environment.NewLine;
            s += "</ResponseSet>" + Environment.NewLine;
            s += "</CreateObjectResponse>" + Environment.NewLine;
            return s;
        }

        public static CreateObjectResponse DeserializeFromPdu(Stream pdu)
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
            if (function != Functioncode.CreateObject)
            {
                return null;
            }
            CreateObjectResponse resp = new CreateObjectResponse(protocolVersion);
            resp.Deserialize(pdu);
            return resp;
        }
    }
}
