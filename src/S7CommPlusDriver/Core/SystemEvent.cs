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
    /// <summary>
    /// These extended keep-alive telegrams came up with TIA V14, and are sent from the PLC or HMI.
    /// There is a version of 16 bytes length and another with 22 bytes length.
    /// The 22 byte version may contain a string like "LOGOUT", but this only after a DeleteObject.
    /// In contrast to all other protocol functions, the values don't use the VLQ encoding!
    /// </summary>
    public class SystemEvent
    {
        public byte TransportFlags;
        public UInt64 ReturnValue;
        public UInt32 Reserved1;
        public UInt32 ConfirmedBytes;
        public UInt32 Reserved2;
        public UInt32 Reserved3;
        public bool IsData;
        public PValue Data;
        public bool IsMessage;
        public String Message;

        public byte ProtocolVersion { get; set; }

        public SystemEvent(byte protocolVersion)
        {
            ProtocolVersion = protocolVersion;
        }

        public int Deserialize(Stream buffer)
        {
            int ret = 0;
            UInt32 peekType;

            S7p.DecodeUInt32(buffer, out Reserved1);
            S7p.DecodeUInt32(buffer, out ConfirmedBytes);
            S7p.DecodeUInt32(buffer, out Reserved2);
            S7p.DecodeUInt32(buffer, out Reserved3);
            
            // If's possible that this is the end of the dataset, without data value or message string.
            var remaining_length = buffer.Length - buffer.Position;
            if (remaining_length >= 4)
            {
                // Heuristic check if next is a string or a struct
                S7p.DecodeUInt32(buffer, out peekType);
                buffer.Position -= 4; // set position back
                if (remaining_length >= 4 && peekType == Datatype.Struct)
                {
                    // binary coded
                    // This seems to work like ordinary values, but without that VLQ encoding.
                    // So a UDINT is always 4 bytes long.
                    IsData = true;
                    IsMessage = false;
                    Data = PValue.Deserialize(buffer, disableVlq: true);
                }
            }
            if (!IsData && remaining_length > 0)
            {
                IsMessage = true;
                // raw string without header or end termination.
                S7p.DecodeWString(buffer, (int)remaining_length, out Message);
            }
            return ret;
        }

        public bool IsFatalError()
        {
            // If we don't have a Data struct at all, then this is possibly just a kind of notification
            if (Data != null)
            {
                try
                {
                    // We excpect Data is of type ValueStruct, and has a structmember "ReturnValue" 40305 of Type LInt
                    var str = (ValueStruct)Data;
                    var retval = (ValueLInt)str.GetStructElement(Ids.ReturnValue);
                    // It's just guess that if the value is negative, then it's a fatal error and we need to disconnect
                    if (retval.GetValue() < 0 )
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                catch
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            string s = "";
            s += "<SystemEvent>" + Environment.NewLine;
            s += "<ProtocolVersion>" + ProtocolVersion.ToString() + "</ProtocolVersion>" + Environment.NewLine;
            s += "<Reserved1>" + Reserved1.ToString() + "</Reserved1>" + Environment.NewLine;
            s += "<ConfirmedBytes>" + ConfirmedBytes.ToString() + "</ConfirmedBytes>" + Environment.NewLine;
            s += "<Reserved2>" + Reserved2.ToString() + "</ReReserved2served1>" + Environment.NewLine;
            s += "<Reserved3>" + Reserved3.ToString() + "</Reserved3>" + Environment.NewLine;
            if (IsData)
            {
                s += "<Data>" + Data.ToString() + "</Data>" + Environment.NewLine;
                s += "<Message></Message>" + Environment.NewLine;
            } 
            else if (IsMessage)
            {
                s += "<Data></Data>" + Environment.NewLine;
                s += "<Message>" + Message.ToString() + "</Message>" + Environment.NewLine;
            }
            else
            {
                s += "<Data></Data>" + Environment.NewLine;
                s += "<Message></Message>" + Environment.NewLine;
            }
            s += "</SystemEvent>" + Environment.NewLine;
            return s;
        }

        public static SystemEvent DeserializeFromPdu(Stream pdu)
        {
            byte protocolVersion;
            // Special handling of ProtocolVersion, which is written to the stream before
            S7p.DecodeByte(pdu, out protocolVersion);
            if (protocolVersion != S7CommPlusDriver.ProtocolVersion.SystemEvent)
            {
                return null;
            }
            var sysevt = new SystemEvent(protocolVersion);
            sysevt.Deserialize(pdu);
            return sysevt;
        }
    }
}
