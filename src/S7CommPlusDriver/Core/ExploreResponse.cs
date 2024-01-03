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
using System.Collections.Generic;

namespace S7CommPlusDriver
{
    public class ExploreResponse : IS7pResponse
    {
        public byte TransportFlags;
        public UInt64 ReturnValue;
        public UInt32 ExploreId;
        public List<PObject> Objects;

        public byte ProtocolVersion { get; set; }
        public ushort FunctionCode { get => Functioncode.Explore; }
        public ushort SequenceNumber { get; set; }
        public uint IntegrityId { get; set; }
        public bool WithIntegrityId { get; set; }

        public ExploreResponse(byte protocolVersion)
        {
            ProtocolVersion = protocolVersion;
        }

        public int Deserialize(Stream buffer)
        {
            int ret = 0;

            ret += S7p.DecodeUInt16(buffer, out ushort seqnr);
            SequenceNumber = seqnr;
            ret += S7p.DecodeByte(buffer, out TransportFlags);

            // Response Set
            ret += S7p.DecodeUInt64Vlq(buffer, out ReturnValue);
            ret += S7p.DecodeUInt32(buffer, out ExploreId);

            if (WithIntegrityId)
            {
                ret += S7p.DecodeUInt32Vlq(buffer, out uint iid);
                IntegrityId = iid;
            }

            // This is a List of objects
            Objects = new List<PObject>();
            ret += S7p.DecodeObjectList(buffer, ref Objects);
            return ret;
        }

        public override string ToString()
        {
            string s = "";
            s += "<ExploreResponse>" + Environment.NewLine;
            s += "<ProtocolVersion>" + ProtocolVersion.ToString() + "</ProtocolVersion>" + Environment.NewLine;
            s += "<SequenceNumber>" + SequenceNumber.ToString() + "</SequenceNumber>" + Environment.NewLine;
            s += "<TransportFlags>" + TransportFlags.ToString() + "</TransportFlags>" + Environment.NewLine;
            s += "<ResponseSet>" + Environment.NewLine;
            s += "<ReturnValue>" + ReturnValue.ToString() + "</ReturnValue>" + Environment.NewLine;
            s += "<ExploreId>" + ExploreId.ToString() + "</ExploreId>" + Environment.NewLine;
            s += "<Objects>";
            foreach (var obj in Objects)
            {
                s += obj.ToString();
            }
            s += "</Objects>" + Environment.NewLine;
            s += "</ResponseSet>" + Environment.NewLine;
            s += "</ExploreResponse>" + Environment.NewLine;
            return s;
        }

        public static ExploreResponse DeserializeFromPdu(Stream pdu, bool withIntegrityId)
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
            if (function != Functioncode.Explore)
            {
                return null;
            }
            ExploreResponse resp = new ExploreResponse(protocolVersion);
            resp.WithIntegrityId = withIntegrityId;
            resp.Deserialize(pdu);

            return resp;
        }
    }
}
