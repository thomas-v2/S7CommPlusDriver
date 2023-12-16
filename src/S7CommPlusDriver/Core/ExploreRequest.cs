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
    public class ExploreRequest : IS7pRequest
    {
        byte TransportFlags = 0x34; // or 0x36???
        public UInt32 ExploreId;
        public UInt32 ExploreRequestId;
        public byte ExploreChildsRecursive;
        public byte ExploreParents;
        public ValueStruct FilterData;
        public List<UInt32> AddressList = new List<UInt32>();

        public uint SessionId { get; set; }
        public byte ProtocolVersion { get; set; }
        public ushort FunctionCode { get => Functioncode.Explore; }
        public ushort SequenceNumber { get; set; }
        public uint IntegrityId { get; set; }
        public bool WithIntegrityId { get; set; }

        public ExploreRequest(byte protocolVersion)
        {
            ProtocolVersion = protocolVersion;
            WithIntegrityId = true;
        }

        public byte GetProtocolVersion()
        {
            return ProtocolVersion;
        }

        public int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, Opcode.Request);
            ret += S7p.EncodeUInt16(buffer, 0);                               // Reserved
            ret += S7p.EncodeUInt16(buffer, FunctionCode);
            ret += S7p.EncodeUInt16(buffer, 0);                               // Reserved
            ret += S7p.EncodeUInt16(buffer, SequenceNumber);
            ret += S7p.EncodeUInt32(buffer, SessionId);
            ret += S7p.EncodeByte(buffer, TransportFlags);

            // Request set
            ret += S7p.EncodeUInt32(buffer, ExploreId);
            ret += S7p.EncodeUInt32Vlq(buffer, ExploreRequestId);
            ret += S7p.EncodeByte(buffer, ExploreChildsRecursive);
            ret += S7p.EncodeByte(buffer, 1);                                   // unknown 0 or 1?
            ret += S7p.EncodeByte(buffer, ExploreParents);

            if (FilterData != null)
            {
                ret += S7p.EncodeByte(buffer, 1); // 1 object / value
                
                // TODO / Experimental:
                // Not 100% sure about how this has to be used:
                // On a Struct, we don't write the datatypeflags into the stream.
                // Maybe the byte before are the flags (which is the way I have it in the Wireshark dissector so far, which may be wrong).
                // To get this working, the byte which gas given the number of addresses isn't written to the stream anymore.
                // ret += S7p.EncodeByte(buffer, 0); // 0 address
                ret += FilterData.Serialize(buffer);
            }

            ret += S7p.EncodeByte(buffer, 0);                                   // Number of following Objects / unknown

            ret += S7p.EncodeUInt32Vlq(buffer, (UInt32)AddressList.Count);      // in Wireshark Dissector only 1 Byte, but maybe a VLQ
            foreach (UInt32 id in AddressList)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, id);
            }
            if (WithIntegrityId)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, IntegrityId);
            }
            // Fill?
            ret += S7p.EncodeUInt32(buffer, 0);
            // Plcsim V13 with Integrity Id needs here 5 Bytes, with 4 doesn't work (not responding).
            // But with my old 1200er FW2.2 it's still working with 4.
            ret += S7p.EncodeByte(buffer, 0);

            return ret;
        }
        
        public override string ToString()
        {
            string s = "";
            s += "<ExploreRequest>" + Environment.NewLine;
            s += "<ProtocolVersion>" + ProtocolVersion.ToString() + "</ProtocolVersion>" + Environment.NewLine;
            s += "<SequenceNumber>" + SequenceNumber.ToString() + "</SequenceNumber>" + Environment.NewLine;
            s += "<SessionId>" + SessionId.ToString() + "</SessionId>" + Environment.NewLine;
            s += "<TransportFlags>" + TransportFlags.ToString() + "</TransportFlags>" + Environment.NewLine;
            s += "<RequestSet>" + Environment.NewLine;
            s += "<ExploreId>" + ExploreId.ToString() + "</ExploreId>" + Environment.NewLine;
            s += "<ExploreRequestId>" + ExploreRequestId.ToString() + "</ExploreRequestId>" + Environment.NewLine;
            s += "<ExploreChildsRecursive>" + ExploreChildsRecursive.ToString() + "</ExploreChildsRecursive>" + Environment.NewLine;
            s += "<ExploreParents>" + ExploreParents.ToString() + "</ExploreParents>" + Environment.NewLine;
            s += "<AddressList>" + Environment.NewLine;
            foreach (UInt32 id in AddressList)
            {
                s += "<Id>" + id.ToString() + "</Id>";
            }
            s += "</AddressList>" + Environment.NewLine;
            s += "</RequestSet>" + Environment.NewLine;
            s += "</ExploreRequest>" + Environment.NewLine;
            return s;
        }
    }
}
