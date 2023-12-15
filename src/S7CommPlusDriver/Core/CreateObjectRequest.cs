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
    public class CreateObjectRequest : IS7pSendableObject
    {
        public byte TransportFlags = 0x36;
        public UInt32 RequestId;
        public PValue RequestValue;
        public PObject RequestObject;

        public uint SessionId { get; set; }
        public byte ProtocolVersion { get; set; }
        public ushort FunctionCode { get => Functioncode.CreateObject; }
        public ushort SequenceNumber { get; set; }
        public uint IntegrityId { get; set; }
        public bool WithIntegrityId { get; set; }

        public CreateObjectRequest(byte protocolVersion, UInt16 seqNum)
        {
            ProtocolVersion = protocolVersion;
            SequenceNumber = seqNum;
            WithIntegrityId = false;
        }

        public void SetRequestIdValue(UInt32 requestId, PValue requestValue)
        {
            RequestId = requestId;
            RequestValue = requestValue;
        }

        public void SetRequestObject(PObject requestObject)
        {
            RequestObject = requestObject;
        }

        public void SetNullServerSessionData()
        {
            // Initializes the data for a Nullserver Session on connection setup.
            // SessionId is set automatically to Ids.ObjectNullServerSession when this this object is sent, if there's no session Id.
            TransportFlags = 0x36;
            RequestId = Ids.ObjectServerSessionContainer;
            RequestValue = new ValueUDInt(0);

            RequestObject = new PObject(RID: Ids.GetNewRIDOnServer, CLSID: Ids.ClassServerSession, AID: Ids.None);
            RequestObject.AddAttribute(Ids.ServerSessionClientRID, new ValueRID(0x80c3c901));
            RequestObject.AddObject(new PObject(RID: Ids.GetNewRIDOnServer, CLSID: Ids.ClassSubscriptions, AID: Ids.None));
        }

        public byte GetProtocolVersion()
        {
            return ProtocolVersion;
        }

        public int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, Opcode.Request);
            ret += S7p.EncodeUInt16(buffer, 0);
            ret += S7p.EncodeUInt16(buffer, FunctionCode);
            ret += S7p.EncodeUInt16(buffer, 0);
            ret += S7p.EncodeUInt16(buffer, SequenceNumber);
            ret += S7p.EncodeUInt32(buffer, SessionId);
            ret += S7p.EncodeByte(buffer, TransportFlags);

            // Request set
            ret += S7p.EncodeUInt32(buffer, Ids.ObjectServerSessionContainer);
            ret += S7p.EncodeByte(buffer, 0x00);
            ret += S7p.EncodeByte(buffer, Datatype.UDInt);
            ret += S7p.EncodeUInt32Vlq(buffer, 0);

            ret += S7p.EncodeUInt32(buffer, 0); // Unknown value 1

            // Object 
            ret += RequestObject.Serialize(buffer);

            // Füllbytes?
            ret += S7p.EncodeUInt32(buffer, 0);
            return ret;
        }

        public override string ToString()
        {
            string s = "";
            s += "<CreateObjectRequest>" + Environment.NewLine;
            s += "<ProtocolVersion>" + ProtocolVersion.ToString() + "</ProtocolVersion>" + Environment.NewLine;
            s += "<SequenceNumber>" + SequenceNumber.ToString() + "</SequenceNumber>" + Environment.NewLine;
            s += "<SessionId>" + SessionId.ToString() + "</SessionId>" + Environment.NewLine;
            s += "<TransportFlags>" + TransportFlags.ToString() + "</TransportFlags>" + Environment.NewLine;
            s += "<RequestSet>" + Environment.NewLine;
            s += "<RequestId>" + RequestId.ToString() + "</RequestId>" + Environment.NewLine;
            s += "<RequestValue>" + RequestValue.ToString() + "</RequestValue>" + Environment.NewLine;
            s += "<RequestObject>" + RequestObject.ToString() + "</RequestObject>" + Environment.NewLine;
            s += "</RequestSet>" + Environment.NewLine;
            s += "</CreateObjectRequest>" + Environment.NewLine;
            return s;
        }
    }
}
