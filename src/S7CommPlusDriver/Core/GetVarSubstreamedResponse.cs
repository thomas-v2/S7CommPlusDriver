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
    public class GetVarSubstreamedResponse : IS7pResponse
    {
        public byte TransportFlags;
        public UInt64 ReturnValue;
        public PValue Value;

        public byte ProtocolVersion { get; set; }
        public ushort FunctionCode { get => Functioncode.SetVariable; }
        public ushort SequenceNumber { get; set; }
        public uint IntegrityId { get; set; }
        public bool WithIntegrityId { get; set; }

        public GetVarSubstreamedResponse(byte protocolVersion)
        {
            ProtocolVersion = protocolVersion;
            WithIntegrityId = true;
        }

        public int Deserialize(Stream buffer)
        {
            int ret = 0;

            ret += S7p.DecodeUInt16(buffer, out ushort seqnr);
            SequenceNumber = seqnr;
            ret += S7p.DecodeByte(buffer, out TransportFlags);

            // Response Set
            ret += S7p.DecodeUInt64Vlq(buffer, out ReturnValue);

            byte unknown;
            ret += S7p.DecodeByte(buffer, out unknown);

            Value = PValue.Deserialize(buffer);

            ret += S7p.DecodeUInt32Vlq(buffer, out uint iid);
            IntegrityId = iid;
            return ret;
        }

        public override string ToString()
        {
            string s = "";
            s += "<GetVarSubstreamedResponse>" + Environment.NewLine;
            s += "<ProtocolVersion>" + ProtocolVersion.ToString() + "</ProtocolVersion>" + Environment.NewLine;
            s += "<SequenceNumber>" + SequenceNumber.ToString() + "</SequenceNumber>" + Environment.NewLine;
            s += "<TransportFlags>" + TransportFlags.ToString() + "</TransportFlags>" + Environment.NewLine;
            s += "<ResponseSet>" + Environment.NewLine;
            s += "<ReturnValue>" + ReturnValue.ToString() + "</ReturnValue>" + Environment.NewLine;
            s += "</ResponseSet>" + Environment.NewLine;
            s += "<IntegrityId>" + IntegrityId.ToString() + "</IntegrityId>" + Environment.NewLine;
            s += "</GetVarSubstreamedResponse>" + Environment.NewLine;
            return s;
        }

        public static GetVarSubstreamedResponse DeserializeFromPdu(Stream pdu)
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
            if (function != Functioncode.GetVarSubStreamed)
            {
                return null;
            }
            GetVarSubstreamedResponse resp = new GetVarSubstreamedResponse(protocolVersion);
            resp.Deserialize(pdu);

            return resp;
        }
    }
}
