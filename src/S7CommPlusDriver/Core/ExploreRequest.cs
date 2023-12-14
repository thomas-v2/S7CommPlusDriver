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
    public class ExploreRequest : IS7pSendableObject
    {
        public readonly ushort FunctionCode = Functioncode.Explore;
        public byte ProtocolVersion;
        public UInt16 SequenceNumber;
        public UInt32 SessionId;
        byte TransportFlags = 0x34; // oder 0x36???
        public UInt32 ExploreId;
        public UInt32 ExploreRequestId;
        public byte ExploreChildsRecursive;
        public byte ExploreParents;

        public ValueStruct FilterData;

        public bool WithIntegrityId = true; // Bei neueren FW (ab 3 oder 4?) immer vorhanden
        public UInt32 IntegrityId;

        public List<UInt32> AddressList = new List<UInt32>();

        public ExploreRequest(byte protocolVersion)
        {
            ProtocolVersion = protocolVersion;
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
            ret += S7p.EncodeByte(buffer, 1);                                   // Unbekannt 0 oder 1?
            ret += S7p.EncodeByte(buffer, ExploreParents);

            if (FilterData != null)
            {
                ret += S7p.EncodeByte(buffer, 1); // 1 object / value
                
                // TODO / Experimentell:
                // Besonderheit hier, oder noch nicht ganz verstanden wie es funktioniert:
                // Hier werden bei einer Struct die DatatypeFlags nicht mit in den Stream geschrieben.
                // Oder das Byte davor gibt die Flags an? (so wird es aktuell in Wireshark angezeigt)
                // Provisorisch wird das eine Byte was (vermutet) die Anzahl der Adressen angab, nicht
                // in den Stream geschrieben.
                // ret += S7p.EncodeByte(buffer, 0); // 0 address
                ret += FilterData.Serialize(buffer);
            }

            ret += S7p.EncodeByte(buffer, 0);                                   // Number of following Objects / unbekannt

            ret += S7p.EncodeUInt32Vlq(buffer, (UInt32)AddressList.Count);      // im Wireshark Dissector nur 1 Byte, vermutl. aber ein VLQ
            foreach (UInt32 id in AddressList)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, id);
            }
            if (WithIntegrityId)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, IntegrityId);
            }
            // Füllbytes?
            ret += S7p.EncodeUInt32(buffer, 0);
            // Plcsim V13 mit Integrity Id benötigt hier 5 Bytes, mit nur 4 funktioniert es hier nicht (keine Antwort).
            // Mit meiner 1200er FW2.2 funktioniert es hingegen auch mit 4.
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
