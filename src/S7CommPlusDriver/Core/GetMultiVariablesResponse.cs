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
    public class GetMultiVariablesResponse : IS7pResponse
    {
        public byte TransportFlags;
        public UInt64 ReturnValue;
        public Dictionary<UInt32, PValue> Values;           // ItemNumber, Value
        public Dictionary<UInt32, UInt64> ErrorValues;      // ItemNumber, ReturnValue

        public byte ProtocolVersion { get; set; }
        public ushort FunctionCode { get => Functioncode.GetMultiVariables; }
        public ushort SequenceNumber { get; set; }
        public uint IntegrityId { get; set; }
        public bool WithIntegrityId { get; set; }

        public GetMultiVariablesResponse(byte protocolVersion)
        {
            ProtocolVersion = protocolVersion;
            ErrorValues = new Dictionary<UInt32, UInt64>();
            Values = new Dictionary<UInt32, PValue>();
            WithIntegrityId = true;
        }

        public int Deserialize(Stream buffer)
        {
            int ret = 0;
            UInt32 itemnr = 0;
            UInt64 retval = 0;

            ret += S7p.DecodeUInt16(buffer, out ushort seqnr);
            SequenceNumber = seqnr;
            ret += S7p.DecodeByte(buffer, out TransportFlags);

            // Response Set
            ret += S7p.DecodeUInt64Vlq(buffer, out ReturnValue);
            ErrorValues.Clear();

            // ValueList
            ret += S7p.DecodeUInt32Vlq(buffer, out itemnr);
            while (itemnr > 0)
            {
                PValue v = PValue.Deserialize(buffer);
                Values.Add(itemnr, v);
                ret += S7p.DecodeUInt32Vlq(buffer, out itemnr);
            }

            // ErrorvalueList
            ret += S7p.DecodeUInt32Vlq(buffer, out itemnr);
            while (itemnr > 0)
            {
                ret += S7p.DecodeUInt64Vlq(buffer, out retval);
                ErrorValues.Add(itemnr, retval);
                ret += S7p.DecodeUInt32Vlq(buffer, out itemnr);
            }
            ret += S7p.DecodeUInt32Vlq(buffer, out uint iid);
            IntegrityId = iid;
            return ret;
        }

        public override string ToString()
        {
            string s = "";
            s += "<GetMultiVariablesResponse>" + Environment.NewLine;
            s += "<ProtocolVersion>" + ProtocolVersion.ToString() + "</ProtocolVersion>" + Environment.NewLine;
            s += "<SequenceNumber>" + SequenceNumber.ToString() + "</SequenceNumber>" + Environment.NewLine;
            s += "<TransportFlags>" + TransportFlags.ToString() + "</TransportFlags>" + Environment.NewLine;
            s += "<ResponseSet>" + Environment.NewLine;
            s += "<ReturnValue>" + ReturnValue.ToString() + "</ReturnValue>" + Environment.NewLine;

            s += "<ValueList>" + Environment.NewLine;
            foreach (var value in Values)
            {
                s += "<Value>" + Environment.NewLine;
                s += "<ItemNr>" + value.Key.ToString() + "</ItemNr>" + Environment.NewLine;
                s += value.Value.ToString();
                s += "</Value>" + Environment.NewLine;
            }
            s += "</ValueList>" + Environment.NewLine;

            s += "<ErrorValueList>" + Environment.NewLine;
            foreach (var errval in ErrorValues)
            {
                s += "<ErrorValue>" + Environment.NewLine;
                s += "<ItemNr>" + errval.Key.ToString() + "</ItemNr>" + Environment.NewLine;
                s += "<ReturnValue>" + errval.Value.ToString() + "</ReturnValue>" + Environment.NewLine;
                s += "</ErrorValue>" + Environment.NewLine;
            }
            s += "</ErrorValueList>" + Environment.NewLine;
            s += "</ResponseSet>" + Environment.NewLine;
            s += "<IntegrityId>" + IntegrityId.ToString() + "</IntegrityId>" + Environment.NewLine;
            s += "</GetMultiVariablesResponse>" + Environment.NewLine;
            return s;
        }

        public static GetMultiVariablesResponse DeserializeFromPdu(Stream pdu)
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
            if (function != Functioncode.GetMultiVariables)
            {
                return null;
            }
            GetMultiVariablesResponse resp = new GetMultiVariablesResponse(protocolVersion);
            resp.Deserialize(pdu);

            return resp;
        }
    }
}
