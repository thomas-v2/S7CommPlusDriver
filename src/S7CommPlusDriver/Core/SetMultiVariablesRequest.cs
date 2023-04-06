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
    public class SetMultiVariablesRequest : IS7pSendableObject
    {
        public byte ProtocolVersion;
        public UInt16 SequenceNumber;
        public UInt32 SessionId;
        byte TransportFlags = 0x34;

        public UInt32 InObjectId;
        /* Hier ist eine Besonderheit:
         * "Normale" Variablenwerte werden geschrieben, in dem InObjectId = 0 ist. Dann werden
         * über die Adressliste die Adressen vorgegeben, und in der Valuelist die Werte.
         * Das Feld nach Itemcount enthält dann nicht die Anzahl der Adressen, sondern die Anzahl der
         * Felder die sich darin befinden. Das ist normalerweise:
         * 1. SymbolCRC
         * 2. Access base-area
         * 3. Hier steht schon die Anzahl der Felder die jetzt noch folgen
         * Es ist also je Adresse die Anzahl von 3 plus 3.
         * 
         * Wenn Werte in Objekten geschrieben werden, enthält die Adressliste nur jeweils einen einzigen Wert.
         * Die Zählung ist im Prinzip identisch.
         * 
         * Da jetzt zwei AddressList vorhanden sind, ist das evtl. verwirrend.
         * TODO
         */
        public List<UInt32> AddressList = new List<UInt32>();
        public List<ItemAddress> AddressListVar = new List<ItemAddress>();
        public List<PValue> ValueList = new List<PValue>();

        public bool WithIntegrityId = true;
        public UInt32 IntegrityId;

        public SetMultiVariablesRequest(byte protocolVersion)
        {
            ProtocolVersion = protocolVersion;
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
            ret += S7p.EncodeUInt16(buffer, Functioncode.SetMultiVariables);
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
                // Item Number + Wert
                ret += S7p.EncodeUInt32Vlq(buffer, i);
                ret += value.Serialize(buffer);
                i++;
            }
            // 1 Füllbyte
            ret += S7p.EncodeByte(buffer, 0x00);
            ret += S7p.EncodeObjectQualifier(buffer);

            if (WithIntegrityId)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, IntegrityId);
            }

            // Füllbytes?
            ret += S7p.EncodeUInt32(buffer, 0);

            return ret;
        }

        public void SetSessionSetupData(UInt32 sessionId, ValueStruct SessionVersion)
        {
            // Initialisiert die Daten für ein erstes SetMultiVariablesRequest nach Verbindungsaufbau
            // Ggf. müssen einige Werte anhand der ersten Antwort je nach CPU eingetragen werden.
            SessionId = sessionId;
            InObjectId = sessionId;
            AddressList.Clear();
            AddressList.Add(Ids.ServerSessionVersion);
            ValueList.Clear();
            ValueList.Add(SessionVersion);
            // Da noch Protocol Version 2, ohne
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
