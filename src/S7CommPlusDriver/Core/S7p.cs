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
using System.Text;
using System.Collections.Generic;

namespace S7CommPlusDriver
{
    public class S7p
    {
        public static int EncodeByte(System.IO.Stream buffer, byte value)
        {
            buffer.WriteByte((byte)value);
            return 1;
        }

        public static int EncodeUInt16(System.IO.Stream buffer, UInt16 value)
        {
            buffer.WriteByte((byte)((value & 0xFF00) >> 08));
            buffer.WriteByte((byte)((value & 0x00FF) >> 00));
            return 2;
        }

        public static int EncodeInt16(System.IO.Stream buffer, Int16 value)
        {
            buffer.WriteByte((byte)((value & 0xFF00) >> 08));
            buffer.WriteByte((byte)((value & 0x00FF) >> 00));
            return 2;
        }

        public static int EncodeUInt32(System.IO.Stream buffer, UInt32 value)
        {
            buffer.WriteByte((byte)((value & 0xFF000000) >> 24));
            buffer.WriteByte((byte)((value & 0x00FF0000) >> 16));
            buffer.WriteByte((byte)((value & 0x0000FF00) >> 08));
            buffer.WriteByte((byte)((value & 0x000000FF) >> 00));
            return 4;
        }

        public static int EncodeInt32(System.IO.Stream buffer, Int32 value)
        {
            buffer.WriteByte((byte)((value & 0xFF000000) >> 24));
            buffer.WriteByte((byte)((value & 0x00FF0000) >> 16));
            buffer.WriteByte((byte)((value & 0x0000FF00) >> 08));
            buffer.WriteByte((byte)((value & 0x000000FF) >> 00));
            return 4;
        }

        public static int EncodeUInt64(System.IO.Stream buffer, UInt64 value)
        {
            buffer.WriteByte((byte)((value & 0xFF00000000000000) >> 56));
            buffer.WriteByte((byte)((value & 0x00FF000000000000) >> 48));
            buffer.WriteByte((byte)((value & 0x0000FF0000000000) >> 40));
            buffer.WriteByte((byte)((value & 0x000000FF00000000) >> 32));
            buffer.WriteByte((byte)((value & 0x00000000FF000000) >> 24));
            buffer.WriteByte((byte)((value & 0x0000000000FF0000) >> 16));
            buffer.WriteByte((byte)((value & 0x000000000000FF00) >> 08));
            buffer.WriteByte((byte)((value & 0x00000000000000FF) >> 00));
            return 8;
        }

        public static int EncodeInt64(System.IO.Stream buffer, Int64 value)
        {
            buffer.WriteByte((byte)(((UInt64)value & 0xFF00000000000000) >> 56));
            buffer.WriteByte((byte)((value & 0x00FF000000000000) >> 48));
            buffer.WriteByte((byte)((value & 0x0000FF0000000000) >> 40));
            buffer.WriteByte((byte)((value & 0x000000FF00000000) >> 32));
            buffer.WriteByte((byte)((value & 0x00000000FF000000) >> 24));
            buffer.WriteByte((byte)((value & 0x0000000000FF0000) >> 16));
            buffer.WriteByte((byte)((value & 0x000000000000FF00) >> 08));
            buffer.WriteByte((byte)((value & 0x00000000000000FF) >> 00));
            return 8;
        }

        public static int DecodeByte(System.IO.Stream buffer, out byte value)
        {
            if (buffer.Length - buffer.Position < 1)
            {
                value = 0;
                return 0;
            }
            value = (byte)buffer.ReadByte();
            return 1;
        }

        public static int DecodeUInt16(System.IO.Stream buffer, out UInt16 value)
        {
            if (buffer.Length - buffer.Position < 2)
            {
                value = 0;
                return 0;
            }
            value = (UInt16)((buffer.ReadByte() << 8) | buffer.ReadByte());
            return 2;
        }

        // Little Endian
        public static int DecodeUInt16LE(System.IO.Stream buffer, out UInt16 value)
        {
            if (buffer.Length - buffer.Position < 2)
            {
                value = 0;
                return 0;
            }
            value = (UInt16)(buffer.ReadByte() | (buffer.ReadByte() << 8));
            return 2;
        }

        public static int DecodeInt16(System.IO.Stream buffer, out Int16 value)
        {
            if (buffer.Length - buffer.Position < 2)
            {
                value = 0;
                return 0;
            }
            value = (Int16)((buffer.ReadByte() << 8) | buffer.ReadByte());
            return 2;
        }

        public static int DecodeUInt32(System.IO.Stream buffer, out UInt32 value)
        {
            if (buffer.Length - buffer.Position < 4)
            {
                value = 0;
                return 0;
            }
            value = (UInt32)((buffer.ReadByte() << 24) | (buffer.ReadByte() << 16) | (buffer.ReadByte() << 8) | buffer.ReadByte());
            return 4;
        }

        // Little Endian
        public static int DecodeUInt32LE(System.IO.Stream buffer, out UInt32 value)
        {
            if (buffer.Length - buffer.Position < 4)
            {
                value = 0;
                return 0;
            }
            value = (UInt32)(buffer.ReadByte() | (buffer.ReadByte() << 8) | (buffer.ReadByte() << 16) | (buffer.ReadByte() << 24));
            return 4;
        }

        // Little Endian
        public static int DecodeInt32LE(System.IO.Stream buffer, out Int32 value)
        {
            if (buffer.Length - buffer.Position < 4)
            {
                value = 0;
                return 0;
            }
            value = (Int32)((buffer.ReadByte() | (buffer.ReadByte() << 8) | (buffer.ReadByte() << 16) | buffer.ReadByte() << 24));
            return 4;
        }

        public static int DecodeInt32(System.IO.Stream buffer, out Int32 value)
        {
            if (buffer.Length - buffer.Position < 4)
            {
                value = 0;
                return 0;
            }
            value = (Int32)((buffer.ReadByte() << 24) | (buffer.ReadByte() << 16) | (buffer.ReadByte() << 8) | buffer.ReadByte());
            return 4;
        }

        public static int DecodeUInt64(System.IO.Stream buffer, out UInt64 value)
        {
            if (buffer.Length - buffer.Position < 8)
            {
                value = 0;
                return 0;
            }
            byte[] b = new byte[8];
            buffer.Read(b, 0, 8);
            Array.Reverse(b, 0, 8);
            value = BitConverter.ToUInt64(b, 0);
            return 8;
        }

        public static int DecodeInt64(System.IO.Stream buffer, out Int64 value)
        {
            if (buffer.Length - buffer.Position < 8)
            {
                value = 0;
                return 0;
            }
            byte[] b = new byte[8];
            buffer.Read(b, 0, 8);
            Array.Reverse(b, 0, 8);
            value = BitConverter.ToInt64(b, 0);
            return 8;
        }

        public static int EncodeUInt32Vlq(System.IO.Stream buffer, UInt32 value)
        {
            byte[] bytes = new byte[5];
            int i, j;
            for (i = 4; i > 0; i--)
            {
                if ((value & (0x7f << (i * 7))) > 0)
                {
                    break;
                }
            }
            for (j = 0; j <= i; j++)
            {
                bytes[j] = (byte)(((value >> ((i - j) * 7)) & 0x7f) | 0x80);
            }
            bytes[i] ^= 0x80;
            buffer.Write(bytes, 0, i + 1);
            return i + 1;
        }

        public static int DecodeUInt32Vlq(System.IO.Stream buffer, out UInt32 value)
        {
            int counter;
            UInt32 val = 0;
            byte octet;
            byte cont = 0;
            int length = 0;
            for (counter = 1; counter <= 5; counter++)
            {
                octet = (byte)buffer.ReadByte();
                length++;
                val <<= 7;
                cont = (byte)(octet & 0x80);
                octet &= 0x7f;
                val += octet;
                if (cont == 0)
                {
                    break;
                }
            }
            value = val;
            return length;
        }
        
        public static int DecodeInt32Vlq(System.IO.Stream buffer, out Int32 value)
        {
            int counter;
            Int32 val = 0;
            byte octet;
            byte cont = 0;
            int length = 0;
            for (counter = 1; counter <= 5; counter++)
            {
                octet = (byte)buffer.ReadByte();
                length++;
                if ((counter == 1) && ((octet & 0x40) != 0))
                {     // check sign 
                    octet &= 0xbf;               
                    val = -64; // pre-load with one complement, excluding first 6 bits
                }
                else
                {
                    val <<= 7;
                }
                cont = (byte)(octet & 0x80);
                octet &= 0x7f;
                val += octet;
                if (cont == 0)
                {
                    break;
                }
            }
            value = val;
            return length;
        }

        public static int EncodeInt32Vlq(System.IO.Stream buffer, Int32 value)
        {
            // You can write negative values like
            // -1234567
            // with full one complement bytes as:
            // 8f ff b4 d2 79
            // The read back value (from plc) is encoded as:
            // ff b4 d2 79
            //   or:
            // -255
            // Write with full one complement bytes as:
            // 8f ff ff fe 01
            // The read back value (from plc) is encoded as:
            // fe 01
            //
            // The actual method writes the values in the 2nd compact variant.
            // The decode algorithms can handle both variants.

            byte[] b = new byte[5];
            UInt32 abs_v;
            if (value == Int32.MinValue) // Handle this, otherwise Math.Abs() will fail
            {
                abs_v = 2147483648;
            }
            else
            {
                abs_v = (UInt32)Math.Abs(value);
            }
            
            b[0] = (byte)(value & 0x7f);
            int length = 1;
            for (int i = 1; i < 5; i++)
            {
                if (abs_v >= 0x40)
                {
                    length++;
                    abs_v >>= 7;
                    value >>= 7;
                    b[i] = (byte)((value & 0x7f) + 0x80);
                }
                else
                {
                    break;
                }
            }

            // Reverse order of bytes
            for (int i = length - 1; i >= 0; i--)
            {
                buffer.Write(b, i, 1);
            }
            return length;
        }

        public static int EncodeUInt64Vlq(System.IO.Stream buffer, UInt64 value)
        {
            // Special handling in the 64 bit VLQ variants:
            // The special handling on the 64 bit variants is neccessary, because without this we would need
            // max. 10 bytes than the max. 9 now.
            // Every byte looses 1 bit for the "continue" flag. For 8 bytes loosing 1 bit, we need 1 more byte.
            // Which in the normal variant would have only 7 bit of space, because 1 bit is for the "continue" flag.
            // The special handling allows to use all 8 bits in the additional 9th byte.
            byte[] b = new byte[9];

            bool special = value > 0x00ffffffffffffff;
            if (special)
            {
                b[0] = (byte)(value & 0xff);
            }
            else
            {
                b[0] = (byte)(value & 0x7f);
            }

            int length = 1;
            for (int i = 1; i < 9; i++)
            {
                if (value >= 0x80)
                {
                    length++;
                    if (i == 1 && special)
                    {
                        value >>= 8;
                    }
                    else
                    {
                        value >>= 7;
                    }
                    b[i] = (byte)((value & 0x7f) + 0x80);
                }
                else
                {
                    break;
                }
            }

            if (special && length == 8)
            {
                // If the guess from above is, that we need 9 bytes but the value encoding would still fit into 8 bytes, then we need
                // to write an empty 0x80 value for continue flag.
                // Example: 123456789012345678 would be "ed d3 b4 dd 98 e1 f3 4e" and fit into 8 bytes.
                // But writing would fail.
                // Testcases where the special handling is needed are values:
                // - 0x00FFFFFFFFFFFFFF      -> standard
                // - 0x00FFFFFFFFFFFFFF + 1  -> additional 0x80 needed
                // The decode algorithm can handle both variants, but the plc accepts it only with the additional bytes (seems Siemens
                // uses a different algorithm than we are using)
                length++;
                b[8] = 0x80;
            }

            // Reverse order of bytes
            for (int i = length - 1; i >= 0; i--)
            {
                buffer.Write(b, i, 1);
            }
            return length;
        }

        public static int DecodeUInt64Vlq(System.IO.Stream buffer, out UInt64 value)
        {
            int counter;
            UInt64 val = 0;
            byte octet;
            byte cont = 0;
            int length = 0;
            for (counter = 1; counter <= 8; counter++)
            {
                octet = (byte)buffer.ReadByte();
                length++;
                val <<= 7;
                cont = (byte)(octet & 0x80);
                octet &= 0x7f;
                val += octet;
                if (cont == 0)
                {
                    break;
                }
            }
            if (cont > 0)         /* 8*7 bit + 8 bit = 64 bit -> Special case in last octet! */
            {
                octet = (byte)buffer.ReadByte();
                length++;
                val <<= 8;
                val += octet;
            }
            value = val;
            return length;
        }

        public static int DecodeInt64Vlq(System.IO.Stream buffer, out Int64 value)
        {
            int counter;
            Int64 val = 0;
            byte octet;
            byte cont = 0;
            int length = 0;
            for (counter = 1; counter <= 8; counter++)
            {
                octet = (byte)buffer.ReadByte();
                length++;
                if ((counter == 1) && ((octet & 0x40) != 0))
                {     // check sign 
                    octet &= 0xbf;
                    val = -64; // pre-load with one complement, excluding first 6 bits
                }
                else
                {
                    val <<= 7;
                }
                cont = (byte)(octet & 0x80);
                octet &= 0x7f;
                val += octet;
                if (cont == 0)
                {
                    break;
                }
            }
            if (cont > 0)
            {
                // 8*7 bit + 8 bit = 64 bit -> Special case in last octet!
                octet = (byte)buffer.ReadByte();
                length++;
                val <<= 8;
                val += octet;
            }
            value = val;
            return length;
        }

        public static int EncodeInt64Vlq(System.IO.Stream buffer, Int64 value)
        {
            byte[] b = new byte[9];
            UInt64 abs_v;
            if (value == Int64.MinValue) // Handle this, otherwise Math.Abs() will fail
            {
                abs_v = 9223372036854775808;
            }
            else
            {
                abs_v = (UInt64)Math.Abs(value);
            }

            bool special = abs_v > 0x007fffffffffffff;
            if (special)
            {
                b[0] = (byte)(value & 0xff);
            }
            else
            {
                b[0] = (byte)(value & 0x7f);
            }

            int length = 1;
            for (int i = 1; i < 9; i++)
            {
                if (abs_v >= 0x40)
                {
                    length++;
                    if (i == 1 && special)
                    {
                        abs_v >>= 8;
                        value >>= 8;
                    }
                    else
                    {
                        abs_v >>= 7;
                        value >>= 7;
                    }
                    b[i] = (byte)((value & 0x7f) + 0x80);
                }
                else
                {
                    break;
                }
            }

            if (special && length == 8)
            {
                // See comment at EncodeUInt64Vlq.
                // Because of the sign bit, the special handling starts here at > 0x007fffffffffffff
                // And we need a different value for negative numbers.
                length++;
                if (value >= 0)
                {
                    b[8] = 0x80;
                }
                else
                {
                    b[8] = 0xff;
                }
            }

            // Reverse order of bytes
            for (int i = length - 1; i >= 0; i--)
            {
                buffer.Write(b, i, 1);
            }
            return length;
        }

        public static int EncodeFloat(System.IO.Stream buffer, float value)
        {
            byte[] v = BitConverter.GetBytes(value);
            buffer.WriteByte(v[3]);
            buffer.WriteByte(v[2]);
            buffer.WriteByte(v[1]);
            buffer.WriteByte(v[0]);
            return 4;
        }

        public static int DecodeFloat(System.IO.Stream buffer, out float value)
        {
            byte[] v = new byte[4];
            v[3] = (byte)buffer.ReadByte();
            v[2] = (byte)buffer.ReadByte();
            v[1] = (byte)buffer.ReadByte();
            v[0] = (byte)buffer.ReadByte();
            value = BitConverter.ToSingle(v, 0);
            return 4;
        }

        public static int EncodeDouble(System.IO.Stream buffer, double value)
        {
            byte[] v = BitConverter.GetBytes(value);
            buffer.WriteByte(v[7]);
            buffer.WriteByte(v[6]);
            buffer.WriteByte(v[5]);
            buffer.WriteByte(v[4]);
            buffer.WriteByte(v[3]);
            buffer.WriteByte(v[2]);
            buffer.WriteByte(v[1]);
            buffer.WriteByte(v[0]);
            return 8;
        }

        public static int DecodeDouble(System.IO.Stream buffer, out double value)
        {
            byte[] v = new byte[8];
            v[7] = (byte)buffer.ReadByte();
            v[6] = (byte)buffer.ReadByte();
            v[5] = (byte)buffer.ReadByte();
            v[4] = (byte)buffer.ReadByte();
            v[3] = (byte)buffer.ReadByte();
            v[2] = (byte)buffer.ReadByte();
            v[1] = (byte)buffer.ReadByte();
            v[0] = (byte)buffer.ReadByte();
            value = BitConverter.ToDouble(v, 0);
            return 8;
        }

        public static int EncodeWString(System.IO.Stream buffer, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            buffer.Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }

        public static int DecodeWString(System.IO.Stream buffer, int length, out string value)
        {
            if (buffer.Length - buffer.Position < length)
            {
                value = string.Empty;
                return 0;
            }
            byte[] tmp = new byte[length];
            buffer.Read(tmp, 0, length);
            value = Encoding.UTF8.GetString(tmp);
            return tmp.Length;
        }

        public static int EncodeOctets(System.IO.Stream buffer, byte[] value)
        {
            if (value == null || value.Length == 0) return 0;
            buffer.Write(value, 0, value.Length);
            return value.Length;
        }

        public static int DecodeOctets(System.IO.Stream buffer, int length, out byte[] value)
        {
            if (length <= 0 || buffer.Length - buffer.Position < length)
            {
                value = null;
                return 0;
            }
            value = new byte[length];
            buffer.Read(value, 0, length);
            return value.Length;
        }

        ///////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////
        // High Level Decode/Encode methods

        public static int DecodeObjectList(System.IO.Stream buffer, ref List<PObject> objList)
        {
            int ret = 0;
            byte tagId;
            objList = new List<PObject>();
            // Peek one byte and set buffer back
            S7p.DecodeByte(buffer, out tagId);
            buffer.Position -= 1;
            while (tagId == ElementID.StartOfObject)
            {
                PObject obj = null;
                ret += S7p.DecodeObject(buffer, ref obj, AsList: true);
                objList.Add(obj);
                S7p.DecodeByte(buffer, out tagId);
                buffer.Position -= 1;
            }
            return ret;
        }

        public static int DecodeObject(System.IO.Stream buffer, ref PObject obj, bool AsList = false)
        {
            byte tagId;
            UInt32 id;
            bool terminate = false;
            int ret = 0;
            do
            {
                ret += DecodeByte(buffer, out tagId);
                switch (tagId)
                {
                    case ElementID.StartOfObject:
                        if (obj == null)
                        {
                            obj = new PObject();
                            ret += DecodeUInt32(buffer, out obj.RelationId);
                            ret += DecodeUInt32Vlq(buffer, out obj.ClassId);
                            ret += DecodeUInt32Vlq(buffer, out obj.ClassFlags);
                            ret += DecodeUInt32Vlq(buffer, out obj.AttributeId);
                            // If a List is expected, don't add the objects coming next to the parent object
                            // TODO: May be it's better to always expect a list? Adding the following objects to the first as children must always be wrong.
                            if (AsList == false)
                            {
                                ret += DecodeObject(buffer, ref obj);
                            }
                        }
                        else
                        {
                            PObject newobj = new PObject();
                            ret += DecodeUInt32(buffer, out newobj.RelationId);
                            ret += DecodeUInt32Vlq(buffer, out newobj.ClassId);
                            ret += DecodeUInt32Vlq(buffer, out newobj.ClassFlags);
                            ret += DecodeUInt32Vlq(buffer, out newobj.AttributeId);
                            ret += DecodeObject(buffer, ref newobj);
                            obj.AddObject(newobj);
                        }
                        break;
                    case ElementID.TerminatingObject:
                        terminate = true;
                        break;
                    case ElementID.Attribute:
                        ret += DecodeUInt32Vlq(buffer, out id);
                        obj.AddAttribute(id, PValue.Deserialize(buffer));
                        break;
                    case ElementID.StartOfTagDescription:
                        // Skip, only 1200 FW2 and maybe older, which definitively don't support TLS
                        break;
                    case ElementID.VartypeList:
                        PVartypeList typelist = new PVartypeList();
                        ret += typelist.Deserialize(buffer);
                        obj.SetVartypeList(typelist);
                        break;
                    case ElementID.VarnameList:
                        PVarnameList namelist = new PVarnameList();
                        ret += namelist.Deserialize(buffer);
                        obj.SetVarnameList(namelist);
                        break;
                    default:
                        terminate = true;
                        break;
                }
            } while (terminate == false);
            return ret;
        }

        public static int DecodeHeader(System.IO.Stream buffer, out byte version, out UInt16 length)
        {
            if (buffer.Length - buffer.Position < 4)
            {
                version = 0;
                length = 0;
                return 0;
            }

            buffer.ReadByte(); // Skip one byte (purpose unclear)
            version = (byte)buffer.ReadByte();
            DecodeUInt16(buffer, out length);
            return 4;
        }

        public static int EncodeHeader(System.IO.Stream buffer, byte version, UInt16 length)
        {
            buffer.WriteByte(0x72);
            buffer.WriteByte(version);
            EncodeUInt16(buffer, length);
            return 4;
        }

        public static int EncodeObjectQualifier(System.IO.Stream buffer)
        {
            int ret = 0;

            ret += EncodeUInt32(buffer, Ids.ObjectQualifier);

            var parentRID = new ValueRID(0);
            var compositionAID = new ValueAID(0);
            var keyQualifier = new ValueUDInt(0);

            ret += EncodeUInt32Vlq(buffer, Ids.ParentRID);
            ret += parentRID.Serialize(buffer);

            ret += EncodeUInt32Vlq(buffer, Ids.CompositionAID);
            ret += compositionAID.Serialize(buffer);

            ret += EncodeUInt32Vlq(buffer, Ids.KeyQualifier);
            ret += keyQualifier.Serialize(buffer);

            ret += EncodeByte(buffer, 0);

            return ret;
        }
    }
}
