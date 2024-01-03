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
    public class SetMultiVariablesRequest : IS7pRequest
    {
        byte TransportFlags = 0x34;

        public UInt32 InObjectId;
        /* Special
         * Plain variables are accessed with InObjectId = 0. Then the user needs to add
         * the addresses via the addresslist and the valuelist the values which should be written.
         * The field after Itemcount then doesn't contains the number of addresses, but the number
         * of field inside it.
         * Which is in the normal use case:
         * 1. SymbolCRC (maybe zero of not CRC check is needed)
         * 2. Access base-area
         * 3. Number of fields which are now following
         * Depending on the address this if 
         *
         * If values inside objects are to be written, then the addresslist contains only a single value.
         * But counting them is identically.
         *
         * The only misleading thing is, we have two addresslists as members for both use-cases.
         * TODO
         */
        public List<UInt32> AddressList = new List<UInt32>();
        public List<ItemAddress> AddressListVar = new List<ItemAddress>();
        public List<PValue> ValueList = new List<PValue>();

        public uint SessionId { get; set; }
        public byte ProtocolVersion { get; set; }
        public ushort FunctionCode { get => Functioncode.SetMultiVariables; }
        public ushort SequenceNumber { get; set; }
        public uint IntegrityId { get; set; }
        public bool WithIntegrityId { get; set; }

        public SetMultiVariablesRequest(byte protocolVersion)
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
            UInt32 i;
            int ret = 0;
            ret += S7p.EncodeByte(buffer, Opcode.Request);
            ret += S7p.EncodeUInt16(buffer, 0);                               // Reserved
            ret += S7p.EncodeUInt16(buffer, FunctionCode);
            ret += S7p.EncodeUInt16(buffer, 0);                               // Reserved
            ret += S7p.EncodeUInt16(buffer, SequenceNumber);
            ret += S7p.EncodeUInt32(buffer, SessionId);
            ret += S7p.EncodeByte(buffer, TransportFlags);

            // Request set
            ret += S7p.EncodeUInt32(buffer, InObjectId);
            ret += S7p.EncodeUInt32Vlq(buffer, (UInt32)ValueList.Count);
            if (InObjectId > 0)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, (UInt32)AddressList.Count);
                foreach (UInt32 id in AddressList)
                {
                    ret += S7p.EncodeUInt32Vlq(buffer, id);
                }
            } 
            else
            {
                UInt32 fieldCount = 0;
                foreach (var adr in AddressListVar)
                {
                    fieldCount += adr.GetNumberOfFields();
                }
                ret += S7p.EncodeUInt32Vlq(buffer, fieldCount);

                foreach (ItemAddress adr in AddressListVar)
                {
                    ret += adr.Serialize(buffer);
                }
            }
            
            i = 1;
            foreach (PValue value in ValueList)
            {
                // Item Number + Value
                ret += S7p.EncodeUInt32Vlq(buffer, i);
                ret += value.Serialize(buffer);
                i++;
            }
            // 1 Fill byte
            ret += S7p.EncodeByte(buffer, 0x00);
            ret += S7p.EncodeObjectQualifier(buffer);

            if (WithIntegrityId)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, IntegrityId);
            }

            // Fill?
            ret += S7p.EncodeUInt32(buffer, 0);

            return ret;
        }

        public void SetSessionSetupData(UInt32 sessionId, ValueStruct SessionVersion)
        {
            // Initializes some values for session setup. Depending on the CPU, some more values needs to be set.
            SessionId = sessionId;
            InObjectId = sessionId;
            AddressList.Clear();
            AddressList.Add(Ids.ServerSessionVersion);
            ValueList.Clear();
            ValueList.Add(SessionVersion);
            // As we use her ProtocolVersion 2, without
            WithIntegrityId = false;
        }

        public override string ToString()
        {
            string s = "";
            s += "<SetMultiVariablesRequest>" + Environment.NewLine;
            s += "<ProtocolVersion>" + ProtocolVersion.ToString() + "</ProtocolVersion>" + Environment.NewLine;
            s += "<SequenceNumber>" + SequenceNumber.ToString() + "</SequenceNumber>" + Environment.NewLine;
            s += "<SessionId>" + SessionId.ToString() + "</SessionId>" + Environment.NewLine;
            s += "<TransportFlags>" + TransportFlags.ToString() + "</TransportFlags>" + Environment.NewLine;
            s += "<RequestSet>" + Environment.NewLine;
            s += "<InObjectId>" + InObjectId.ToString() + "</InObjectId>" + Environment.NewLine;
            s += "<ItemCount>" + ValueList.Count + "</ItemCount>" + Environment.NewLine;
            if (InObjectId > 0)
            {
                s += "<ItemAddressCount>" + AddressList.Count + "</ItemAddressCount>" + Environment.NewLine;
                s += "<AddressList>" + Environment.NewLine;
                foreach (UInt32 id in AddressList)
                {
                    s += "<Id>" + id.ToString() + "</Id>" + Environment.NewLine;
                }
                s += "</AddressList>" + Environment.NewLine;
            }
            else
            {
                UInt32 fieldCount = 0;
                foreach (ItemAddress adr in AddressListVar)
                {
                    fieldCount += adr.GetNumberOfFields();
                }
                s += "<NumberOfFields>" + fieldCount.ToString() + "</NumberOfFields>" + Environment.NewLine;
                s += "<AddressList>" + Environment.NewLine;
                foreach (ItemAddress adr in AddressListVar)
                {
                    s += adr.ToString();
                }
                s += "</AddressList>" + Environment.NewLine;
            }
            s += "<ValueList>" + Environment.NewLine;
            foreach (PValue val in ValueList)
            {
                s += "<Value>" + val.ToString() + "</Value>" + Environment.NewLine;
            }
            s += "</ValueList>" + Environment.NewLine;
            s += "</RequestSet>" + Environment.NewLine;
            s += "</SetMultiVariablesRequest>" + Environment.NewLine;
            return s;
        }
    }
}
