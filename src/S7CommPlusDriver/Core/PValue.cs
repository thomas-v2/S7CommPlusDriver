﻿#region License
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
    // TODO:
    // Sinnvoller wäre es die Array-Verarbeitung außerhalb der Klassen zu platzieren,
    // da jetzt in jeder Array-Klasse viel duplizierter Code vorhanden ist.
    //
    // Ein S7String beim Variablen Browsen wird in der Antwort als USInt-Array übertragen!
    public abstract class PValue : IS7pSerialize
    {
        protected static byte FLAGS_ARRAY = 0x10;
        protected static byte FLAGS_ADDRESSARRAY = 0x20;
        protected static byte FLAGS_SPARSEARRAY = 0x40;

        protected byte DatatypeFlags;
        public abstract int Serialize(Stream buffer);

        public bool IsArray()
        {
            return ((DatatypeFlags & FLAGS_ARRAY) != 0);
        }

        public bool IsAddressArray()
        {
            return ((DatatypeFlags & FLAGS_ADDRESSARRAY) != 0);
        }

        public bool IsSparseArray()
        {
            return ((DatatypeFlags & FLAGS_SPARSEARRAY) != 0);
        }

        // TODO: Sollte wie Serialize die Länge zurückgeben??
        // Wobei die Länge nie weiter benötigt wird, ggf. den Rückgabewert dann sinnvoller verwenden.
        public static PValue Deserialize(Stream buffer)
        {
            byte flags;
            byte datatype;
            S7p.DecodeByte(buffer, out flags);
            S7p.DecodeByte(buffer, out datatype);

            // Sparsearray ist komplett anders, wie auch Adressarray bei Struct
            if (flags == FLAGS_ARRAY || flags == FLAGS_ADDRESSARRAY)
            {
                switch (datatype)
                {
                    case Datatype.USInt:
                        return ValueUSIntArray.Deserialize(buffer, flags);
                    case Datatype.UInt:
                        return ValueUIntArray.Deserialize(buffer, flags);
                    case Datatype.UDInt:
                        return ValueUDIntArray.Deserialize(buffer, flags);
                    case Datatype.ULInt:
                        return ValueUDIntArray.Deserialize(buffer, flags);
                    case Datatype.SInt:
                        return ValueSIntArray.Deserialize(buffer, flags);
                    case Datatype.Int:
                        return ValueIntArray.Deserialize(buffer, flags);
                    case Datatype.DInt:
                        return ValueDIntArray.Deserialize(buffer, flags);
                    case Datatype.LInt:
                        return ValueLIntArray.Deserialize(buffer, flags);
                    case Datatype.Byte:
                        return ValueByteArray.Deserialize(buffer, flags);
                    case Datatype.Word:
                        return ValueWordArray.Deserialize(buffer, flags);
                    case Datatype.DWord:
                        return ValueDWordArray.Deserialize(buffer, flags);
                    case Datatype.LWord:
                        return ValueLWordArray.Deserialize(buffer, flags);
                    case Datatype.Real:
                        return ValueRealArray.Deserialize(buffer, flags);
                    case Datatype.LReal:
                        return ValueLRealArray.Deserialize(buffer, flags);
                    case Datatype.RID:
                        return ValueRIDArray.Deserialize(buffer, flags);
                    case Datatype.AID:
                        return ValueAIDArray.Deserialize(buffer, flags);
                    case Datatype.Blob:
                        return ValueBlobArray.Deserialize(buffer, flags);
                    case Datatype.Timestamp:
                        throw new NotImplementedException();
                    case Datatype.Timespan:
                        throw new NotImplementedException();

                    case Datatype.WString:
                        return ValueWStringArray.Deserialize(buffer, flags);
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (flags == FLAGS_SPARSEARRAY)
            {
                switch (datatype)
                {
                    case Datatype.DInt:
                        return ValueDIntSparseArray.Deserialize(buffer, flags);
                    case Datatype.UDInt:
                        return ValueUDIntSparseArray.Deserialize(buffer, flags);
                    case Datatype.Blob:
                        return ValueBlobSparseArray.Deserialize(buffer, flags);
                    case Datatype.WString:
                        return ValueWStringSparseArray.Deserialize(buffer, flags);
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                switch (datatype)
                {
                    case Datatype.Null:
                        return ValueNull.Deserialize(buffer, flags);
                    case Datatype.Bool:
                        return ValueBool.Deserialize(buffer, flags);
                    case Datatype.USInt:
                        return ValueUSInt.Deserialize(buffer, flags);
                    case Datatype.UInt:
                        return ValueUInt.Deserialize(buffer, flags);
                    case Datatype.UDInt:
                        return ValueUDInt.Deserialize(buffer, flags);
                    case Datatype.ULInt:
                        return ValueULInt.Deserialize(buffer, flags);
                    case Datatype.SInt:
                        return ValueSInt.Deserialize(buffer, flags);
                    case Datatype.Int:
                        return ValueInt.Deserialize(buffer, flags);
                    case Datatype.DInt:
                        return ValueDInt.Deserialize(buffer, flags);
                    case Datatype.LInt:
                        return ValueLInt.Deserialize(buffer, flags);
                    case Datatype.Byte:
                        return ValueByte.Deserialize(buffer, flags);
                    case Datatype.Word:
                        return ValueWord.Deserialize(buffer, flags);
                    case Datatype.DWord:
                        return ValueDWord.Deserialize(buffer, flags);
                    case Datatype.LWord:
                        return ValueLWord.Deserialize(buffer, flags);
                    case Datatype.Real:
                        return ValueReal.Deserialize(buffer, flags);
                    case Datatype.LReal:
                        return ValueLReal.Deserialize(buffer, flags);
                    case Datatype.Timestamp:
                        return ValueTimestamp.Deserialize(buffer, flags);
                    case Datatype.Timespan:
                        return ValueTimespan.Deserialize(buffer, flags);
                    case Datatype.RID:
                        return ValueRID.Deserialize(buffer, flags);
                    case Datatype.AID:
                        return ValueAID.Deserialize(buffer, flags);
                    case Datatype.Blob:
                        return ValueBlob.Deserialize(buffer, flags);
                    case Datatype.WString:
                        return ValueWString.Deserialize(buffer, flags);
                    case Datatype.Variant:
                        throw new NotImplementedException();
                    case Datatype.Struct:
                        return ValueStruct.Deserialize(buffer, flags);
                    case Datatype.S7String:
                        throw new NotImplementedException();
                }
            }
            return null;
        }
    }

    public class ValueNull : PValue
    {
        public ValueNull() : this(0)
        {
        }

        public ValueNull(byte flags)
        {
            DatatypeFlags = flags;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Null);
            return ret;
        }

        public override string ToString()
        {
            return "<Value type=\"Null\"></Value>";
        }

        public static ValueNull Deserialize(Stream buffer, byte flags)
        {
            return new ValueNull(flags);
        }
    }

    public class ValueBool : PValue
    {
        bool Value;

        public ValueBool(bool value) : this(value, 0)
        {
        }

        public ValueBool(bool value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public bool GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Bool);
            ret += S7p.EncodeByte(buffer, Convert.ToByte(Value));
            return ret;
        }

        public override string ToString()
        {
            return "<Value type=\"Bool\">" + Value.ToString() + "</Value>";
        }

        public static ValueBool Deserialize(Stream buffer, byte flags)
        {
            byte value;
            S7p.DecodeByte(buffer, out value);
            return new ValueBool(Convert.ToBoolean(value), flags);
        }
    }

    public class ValueUSInt : PValue
    {
        byte Value;

        public ValueUSInt(byte value) : this(value, 0)
        {
        }

        public ValueUSInt(byte value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public byte GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.USInt);
            ret += S7p.EncodeByte(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return "<Value type=\"USInt\">" + Value.ToString() + "</Value>";
        }

        public static ValueUSInt Deserialize(Stream buffer, byte flags)
        {
            byte value;
            S7p.DecodeByte(buffer, out value);
            return new ValueUSInt(value, flags);
        }
    }

    public class ValueUSIntArray : PValue
    {
        byte[] Value;

        public ValueUSIntArray(byte[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueUSIntArray(byte[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new byte[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public byte[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.USInt);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeByte(buffer, Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"USIntArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueUSIntArray Deserialize(Stream buffer, byte flags)
        {
            byte[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new byte[size];
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeByte(buffer, out value[i]);
            }
            return new ValueUSIntArray(value, flags);
        }
    }

    public class ValueUInt : PValue
    {
        UInt16 Value;

        public ValueUInt(UInt16 value) : this(value, 0)
        {
        }

        public ValueUInt(UInt16 value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public UInt16 GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.UInt);
            ret += S7p.EncodeUInt16(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return "<Value type=\"UInt\">" + Value.ToString() + "</Value>";
        }

        public static ValueUInt Deserialize(Stream buffer, byte flags)
        {
            UInt16 value;
            S7p.DecodeUInt16(buffer, out value);
            return new ValueUInt(value, flags);
        }
    }

    public class ValueUIntArray : PValue
    {
        UInt16[] Value;

        public ValueUIntArray(UInt16[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueUIntArray(UInt16[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new UInt16[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public UInt16[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.UInt);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeUInt16(buffer, Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"UIntArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueUIntArray Deserialize(Stream buffer, byte flags)
        {
            UInt16[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new UInt16[size];
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeUInt16(buffer, out value[i]);
            }
            return new ValueUIntArray(value, flags);
        }
    }

    public class ValueUDInt : PValue
    {
        UInt32 Value;

        public ValueUDInt(UInt32 value) : this(value, 0)
        {
        }

        public ValueUDInt(UInt32 value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public UInt32 GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.UDInt);
            ret += S7p.EncodeUInt32Vlq(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return "<Value type=\"UDInt\">" + Value.ToString() + "</Value>";
        }

        public static ValueUDInt Deserialize(Stream buffer, byte flags)
        {
            UInt32 value;
            S7p.DecodeUInt32Vlq(buffer, out value);
            return new ValueUDInt(value, flags);
        }
    }

    public class ValueUDIntArray : PValue
    {
        UInt32[] Value;

        public ValueUDIntArray(UInt32[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueUDIntArray(UInt32[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new UInt32[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public UInt32[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.UDInt);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"UDIntArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueUDIntArray Deserialize(Stream buffer, byte flags)
        {
            UInt32[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new UInt32[size];
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeUInt32Vlq(buffer, out value[i]);
            }
            return new ValueUDIntArray(value, flags);
        }
    }
    
    // Sparsearray ist vom Aufbau her ähnlich wie das Lesen einer Struct.
    // Alle Elemente in der Liste sind vom Eintrag key,value. Value ist immer von diesem Typ.
    // Die Liste ist dabei Null-Terminiert.
    // Beispiel: Lesen von 1037 (SystemLimits) via GetVarSubStreamed in der Response.
    public class ValueUDIntSparseArray : PValue
    {
        Dictionary<UInt32, UInt32> Value;

        public ValueUDIntSparseArray(Dictionary<UInt32, UInt32> value) : this(value, FLAGS_SPARSEARRAY)
        {
        }

        public ValueUDIntSparseArray(Dictionary<UInt32, UInt32> value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new Dictionary<UInt32, UInt32>(value);
            }
        }

        public Dictionary<UInt32, UInt32> GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.UDInt);
            foreach (var v in Value)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, v.Key);
                ret += S7p.EncodeUInt32Vlq(buffer, v.Value);
            }
            ret += S7p.EncodeByte(buffer, 0);
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"UDIntSparseArray\">";
            foreach (var v in Value)
            {
                s += String.Format("<Value key=\"{0}\">{1}</Value>", v.Key, v.Value);
            }
            s += "</Value>";
            return s;
        }

        public static ValueUDIntSparseArray Deserialize(Stream buffer, byte flags)
        {
            Dictionary<UInt32, UInt32> value = new Dictionary<uint, uint>();
            UInt32 k = 0;
            UInt32 v = 0;
            S7p.DecodeUInt32Vlq(buffer, out k);
            while (k > 0)
            {
                S7p.DecodeUInt32Vlq(buffer, out v);
                value.Add(k, v);
                S7p.DecodeUInt32Vlq(buffer, out k);
            }
            return new ValueUDIntSparseArray(value, flags);
        }
    }

    public class ValueULInt : PValue
    {
        UInt64 Value;

        public ValueULInt(UInt64 value) : this(value, 0)
        {
        }

        public ValueULInt(UInt64 value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public UInt64 GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.ULInt);
            ret += S7p.EncodeUInt64Vlq(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return "<Value type=\"ULInt\">" + Value.ToString() + "</Value>";
        }

        public static ValueULInt Deserialize(Stream buffer, byte flags)
        {
            UInt64 value;
            S7p.DecodeUInt64Vlq(buffer, out value);
            return new ValueULInt(value, flags);
        }
    }

    public class ValueULIntArray : PValue
    {
        UInt64[] Value;

        public ValueULIntArray(UInt64[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueULIntArray(UInt64[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new UInt64[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public UInt64[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.ULInt);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeUInt64Vlq(buffer, Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"ULIntArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueULIntArray Deserialize(Stream buffer, byte flags)
        {
            UInt64[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new UInt64[size];
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeUInt64Vlq(buffer, out value[i]);
            }
            return new ValueULIntArray(value, flags);
        }
    }

    public class ValueSInt : PValue
    {
        sbyte Value;

        public ValueSInt(sbyte value) : this(value, 0)
        {
        }

        public ValueSInt(sbyte value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public sbyte GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.SInt);
            ret += S7p.EncodeByte(buffer, (byte)Value);
            return ret;
        }

        public override string ToString()
        {
            return "<Value type=\"SInt\">" + Value.ToString() + "</Value>";
        }

        public static ValueSInt Deserialize(Stream buffer, byte flags)
        {
            byte value;
            S7p.DecodeByte(buffer, out value);
            return new ValueSInt((sbyte)value, flags);
        }
    }

    public class ValueSIntArray : PValue
    {
        sbyte[] Value;

        public ValueSIntArray(sbyte[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueSIntArray(sbyte[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new sbyte[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public sbyte[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.USInt);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeByte(buffer, (byte)Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"SIntArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueSIntArray Deserialize(Stream buffer, byte flags)
        {
            sbyte[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new sbyte[size];
            byte b;
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeByte(buffer, out b);
                value[i] = (sbyte)b;
            }
            return new ValueSIntArray(value, flags);
        }
    }

    public class ValueInt : PValue
    {
        Int16 Value;

        public ValueInt(Int16 value) : this(value, 0)
        {
        }

        public ValueInt(Int16 value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public Int16 GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Int);
            ret += S7p.EncodeInt16(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return "<Value type=\"Int\">" + Value.ToString() + "</Value>";
        }

        public static ValueInt Deserialize(Stream buffer, byte flags)
        {
            Int16 value;
            S7p.DecodeInt16(buffer, out value);
            return new ValueInt(value, flags);
        }
    }

    public class ValueIntArray : PValue
    {
        Int16[] Value;

        public ValueIntArray(Int16[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueIntArray(Int16[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new Int16[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public Int16[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Int);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeInt16(buffer, Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"IntArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueIntArray Deserialize(Stream buffer, byte flags)
        {
            Int16[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new Int16[size];
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeInt16(buffer, out value[i]);
            }
            return new ValueIntArray(value, flags);
        }
    }

    public class ValueDInt : PValue
    {
        Int32 Value;

        public ValueDInt(Int32 value) : this(value, 0)
        {
        }

        public ValueDInt(Int32 value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public Int32 GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.DInt);
            ret += S7p.EncodeInt32Vlq(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return "<Value type=\"DInt\">" + Value.ToString() + "</Value>";
        }

        public static ValueDInt Deserialize(Stream buffer, byte flags)
        {
            Int32 value;
            S7p.DecodeInt32Vlq(buffer, out value);
            return new ValueDInt(value, flags);
        }
    }

    public class ValueDIntArray : PValue
    {
        Int32[] Value;

        public ValueDIntArray(Int32[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueDIntArray(Int32[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new Int32[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public Int32[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.DInt);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeInt32Vlq(buffer, Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"DIntArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueDIntArray Deserialize(Stream buffer, byte flags)
        {
            Int32[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new Int32[size];
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeInt32Vlq(buffer, out value[i]);
            }
            return new ValueDIntArray(value, flags);
        }
    }

    // Sparsearray ist vom Aufbau her ähnlich wie das Lesen einer Struct.
    // Alle Elemente in der Liste sind vom Eintrag key,value. Value ist immer von diesem Typ.
    // Die Liste ist dabei Null-Terminiert.
    // Beispiel: Lesen von 1037 (SystemLimits) via GetVarSubStreamed in der Response.
    public class ValueDIntSparseArray : PValue
    {
        Dictionary<UInt32, Int32> Value;

        public ValueDIntSparseArray(Dictionary<UInt32, Int32> value) : this(value, FLAGS_SPARSEARRAY)
        {
        }

        public ValueDIntSparseArray(Dictionary<UInt32, Int32> value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new Dictionary<UInt32, Int32>(value);
            }
        }

        public Dictionary<UInt32, Int32> GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.DInt);
            foreach (var v in Value)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, v.Key);
                ret += S7p.EncodeInt32Vlq(buffer, v.Value);
            }
            ret += S7p.EncodeByte(buffer, 0);
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"DIntSparseArray\">";
            foreach (var v in Value)
            {
                s += String.Format("<Value key=\"{0}\">{1}</Value>", v.Key, v.Value);
            }
            s += "</Value>";
            return s;
        }

        public static ValueDIntSparseArray Deserialize(Stream buffer, byte flags)
        {
            Dictionary<UInt32, Int32> value = new Dictionary<UInt32, Int32>();
            UInt32 k = 0;
            Int32 v = 0;
            S7p.DecodeUInt32Vlq(buffer, out k);
            while (k > 0)
            {
                S7p.DecodeInt32Vlq(buffer, out v);
                value.Add(k, v);
                S7p.DecodeUInt32Vlq(buffer, out k);
            }
            return new ValueDIntSparseArray(value, flags);
        }
    }

    public class ValueLInt : PValue
    {
        Int64 Value;

        public ValueLInt(Int64 value) : this(value, 0)
        {
        }

        public ValueLInt(Int64 value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public Int64 GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.LInt);
            ret += S7p.EncodeInt64Vlq(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return "<Value type=\"LInt\">" + Value.ToString() + "</Value>";
        }

        public static ValueLInt Deserialize(Stream buffer, byte flags)
        {
            Int64 value;
            S7p.DecodeInt64Vlq(buffer, out value);
            return new ValueLInt(value, flags);
        }
    }

    public class ValueLIntArray : PValue
    {
        Int64[] Value;

        public ValueLIntArray(Int64[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueLIntArray(Int64[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new Int64[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public Int64[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.LInt);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeInt64Vlq(buffer, Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"LIntArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueLIntArray Deserialize(Stream buffer, byte flags)
        {
            Int64[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new Int64[size];
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeInt64Vlq(buffer, out value[i]);
            }
            return new ValueLIntArray(value, flags);
        }
    }

    public class ValueByte : PValue
    {
        byte Value;

        public ValueByte(byte value) : this(value, 0)
        {
        }

        public ValueByte(byte value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public byte GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Byte);
            ret += S7p.EncodeByte(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return String.Format("<Value type=\"Byte\">{0}</Value>", Value);
        }

        public static ValueByte Deserialize(Stream buffer, byte flags)
        {
            byte value;
            S7p.DecodeByte(buffer, out value);
            return new ValueByte(value, flags);
        }
    }

    public class ValueByteArray : PValue
    {
        byte[] Value;

        public ValueByteArray(byte[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueByteArray(byte[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new byte[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public byte[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Byte);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeByte(buffer, Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"ByteArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueByteArray Deserialize(Stream buffer, byte flags)
        {
            byte[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new byte[size];
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeByte(buffer, out value[i]);
            }
            return new ValueByteArray(value, flags);
        }
    }

    public class ValueWord : PValue
    {
        UInt16 Value;

        public ValueWord(UInt16 value) : this(value, 0)
        {
        }

        public ValueWord(UInt16 value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public UInt16 GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Word);
            ret += S7p.EncodeUInt16(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return String.Format("<Value type=\"Word\">{0}</Value>", Value);
        }

        public static ValueWord Deserialize(Stream buffer, byte flags)
        {
            UInt16 value;
            S7p.DecodeUInt16(buffer, out value);
            return new ValueWord(value, flags);
        }
    }

    public class ValueWordArray : PValue
    {
        UInt16[] Value;

        public ValueWordArray(UInt16[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueWordArray(UInt16[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new UInt16[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public UInt16[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Word);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeUInt16(buffer, Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"WordArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueWordArray Deserialize(Stream buffer, byte flags)
        {
            UInt16[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new UInt16[size];
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeUInt16(buffer, out value[i]);
            }
            return new ValueWordArray(value, flags);
        }
    }

    public class ValueDWord : PValue
    {
        UInt32 Value;

        public ValueDWord(UInt32 value) : this(value, 0)
        {
        }

        public ValueDWord(UInt32 value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public UInt32 GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.DWord);
            ret += S7p.EncodeUInt32(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return String.Format("<Value type=\"DWord\">{0}</Value>", Value);
        }

        public static ValueDWord Deserialize(Stream buffer, byte flags)
        {
            UInt32 value;
            S7p.DecodeUInt32(buffer, out value);
            return new ValueDWord(value, flags);
        }
    }

    public class ValueDWordArray : PValue
    {
        UInt32[] Value;

        public ValueDWordArray(UInt32[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueDWordArray(UInt32[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new UInt32[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public UInt32[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.DWord);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeUInt32(buffer, Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"DWordArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueDWordArray Deserialize(Stream buffer, byte flags)
        {
            UInt32[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new UInt32[size];
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeUInt32(buffer, out value[i]);
            }
            return new ValueDWordArray(value, flags);
        }
    }

    public class ValueLWord : PValue
    {
        UInt64 Value;

        public ValueLWord(UInt64 value) : this(value, 0)
        {
        }

        public ValueLWord(UInt64 value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public UInt64 GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.LWord);
            ret += S7p.EncodeUInt64(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return String.Format("<Value type=\"LWord\">{0}</Value>", Value);
        }

        public static ValueLWord Deserialize(Stream buffer, byte flags)
        {
            UInt64 value;
            S7p.DecodeUInt64(buffer, out value);
            return new ValueLWord(value, flags);
        }
    }

    public class ValueLWordArray : PValue
    {
        UInt64[] Value;

        public ValueLWordArray(UInt64[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueLWordArray(UInt64[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new UInt64[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public UInt64[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.LWord);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeUInt64(buffer, Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"LWordArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueLWordArray Deserialize(Stream buffer, byte flags)
        {
            UInt64[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new UInt64[size];
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeUInt64(buffer, out value[i]);
            }
            return new ValueLWordArray(value, flags);
        }
    }

    public class ValueReal : PValue
    {
        float Value;

        public ValueReal(float value) : this(value, 0)
        {
        }

        public ValueReal(float value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public float GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Real);
            ret += S7p.EncodeFloat(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return "<Value type=\"Real\">" + Value.ToString() + "</Value>";
        }

        public static ValueReal Deserialize(Stream buffer, byte flags)
        {
            float value;
            S7p.DecodeFloat(buffer, out value);
            return new ValueReal(value, flags);
        }
    }

    public class ValueRealArray : PValue
    {
        float[] Value;

        public ValueRealArray(float[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueRealArray(float[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new float[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public float[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Real);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeFloat(buffer, Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"RealArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueRealArray Deserialize(Stream buffer, byte flags)
        {
            float[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new float[size];
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeFloat(buffer, out value[i]);
            }
            return new ValueRealArray(value, flags);
        }
    }

    public class ValueLReal : PValue
    {
        double Value;

        public ValueLReal(double value) : this(value, 0)
        {
        }

        public ValueLReal(double value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public double GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.LReal);
            ret += S7p.EncodeDouble(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return "<Value type=\"LReal\">" + Value.ToString() + "</Value>";
        }

        public static ValueLReal Deserialize(Stream buffer, byte flags)
        {
            double value;
            S7p.DecodeDouble(buffer, out value);
            return new ValueLReal(value, flags);
        }
    }

    public class ValueLRealArray : PValue
    {
        double[] Value;

        public ValueLRealArray(double[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueLRealArray(double[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new double[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public double[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.LReal);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeDouble(buffer, Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"LRealArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueLRealArray Deserialize(Stream buffer, byte flags)
        {
            double[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new double[size];
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeDouble(buffer, out value[i]);
            }
            return new ValueLRealArray(value, flags);
        }
    }

    public class ValueTimestamp : PValue
    {
        UInt64 Value;

        public ValueTimestamp(UInt64 value) : this(value, 0)
        {
        }

        public ValueTimestamp(UInt64 value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public UInt64 GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Timestamp);
            ret += S7p.EncodeUInt64(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            DateTime dt = new DateTime(1970, 1, 1);
            ulong v, ns;
            string fmt;
            v = Value;
            ns = v % 1000000000;
            v /= 1000000000;

            dt = dt.AddSeconds(v);

            if ((ns % 1000) > 0)
            {
                fmt = "{0}.{1:D09}";
            }
            else if ((ns % 1000000) > 0)
            {
                fmt = "{0}.{1:D06}";
                ns /= 1000;
            }
            else if ((ns % 1000000000) > 0)
            {
                fmt = "{0}.{1:D03}";
                ns /= 1000000;
            }
            else
            {
                return "<Value type=\"Timestamp\">" + dt.ToString() + "</Value>";
            }
            return "<Value type=\"Timestamp\">" + String.Format(fmt, dt.ToString(), ns) + "</Value>";
        }

        public static ValueTimestamp Deserialize(Stream buffer, byte flags)
        {
            UInt64 value;
            S7p.DecodeUInt64(buffer, out value);
            return new ValueTimestamp(value, flags);
        }
    }

    public class ValueTimespan : PValue
    {
        Int64 Value;

        public ValueTimespan(Int64 value) : this(value, 0)
        {
        }

        public ValueTimespan(Int64 value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public Int64 GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Timespan);
            ret += S7p.EncodeInt64Vlq(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            string str;
            long[] divs = { 86400000000000, 3600000000000, 60000000000, 1000000000, 1000000, 1000, 1 };
            string[] vfmt = { "{0}d", "{0:00}h", "{0:00}m", "{0:00}s", "{0:000}ms", "{0:000}us", "{0:000}ns" };
            long val;
            long timespan = Value;
            bool time_negative = false;
            if (timespan == 0)
            {
                str = "LT#000ns";
            }
            else
            {
                if (timespan < 0)
                {
                    str = "LT#-";
                    time_negative = true;
                    for (int i = 0; i < 7; i++)
                    {
                        divs[i] = -divs[i];
                    }
                }
                else
                {
                    str = "LT#";
                }

                for (int i = 0; i < 7; i++)
                {
                    val = timespan / divs[i];
                    timespan -= val * divs[i];
                    if (val > 0)
                    {
                        str += String.Format(vfmt[i], (Int32)val);
                        if ((!time_negative && timespan > 0) || (time_negative && timespan < 0))
                        {
                            str += "_";
                        }
                    }
                }
            }
            return ("<Value type=\"Timespan\">" + str + "</Value>");
        }

        public static ValueTimespan Deserialize(Stream buffer, byte flags)
        {
            Int64 value;
            S7p.DecodeInt64Vlq(buffer, out value);
            return new ValueTimespan(value, flags);
        }
    }

    public class ValueRID : PValue
    {
        UInt32 Value;

        public ValueRID(UInt32 rid) : this (rid, 0)
        {
        }

        public ValueRID(UInt32 rid, byte flags)
        {
            DatatypeFlags = flags;
            Value = rid;
        }

        public UInt32 GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.RID);
            ret += S7p.EncodeUInt32(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return "<Value type=\"RID\">" + Value.ToString() + "</Value>";
        }

        public static ValueRID Deserialize(Stream buffer, byte flags)
        {
            UInt32 value;
            S7p.DecodeUInt32(buffer, out value);
            return new ValueRID(value, flags);
        }
    }

    public class ValueRIDArray : PValue
    {
        UInt32[] Value;

        public ValueRIDArray(UInt32[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueRIDArray(UInt32[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new UInt32[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public UInt32[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.RID);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeUInt32(buffer, Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"RIDArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueRIDArray Deserialize(Stream buffer, byte flags)
        {
            UInt32[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new UInt32[size];
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeUInt32(buffer, out value[i]);
            }
            return new ValueRIDArray(value, flags);
        }
    }

    public class ValueAID : PValue
    {
        UInt32 Value;

        public ValueAID(UInt32 value) : this(value, 0)
        {
        }

        public ValueAID(UInt32 value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public UInt32 GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.AID);
            ret += S7p.EncodeUInt32Vlq(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return "<Value type=\"AID\">" + Value.ToString() + "</Value>";
        }

        public static ValueAID Deserialize(Stream buffer, byte flags)
        {
            UInt32 value;
            S7p.DecodeUInt32Vlq(buffer, out value);
            return new ValueAID(value, flags);
        }
    }

    public class ValueAIDArray : PValue
    {
        UInt32[] Value;

        public ValueAIDArray(UInt32[] value) : this(value, FLAGS_ARRAY)
        {
        }

        public ValueAIDArray(UInt32[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new UInt32[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public UInt32[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.AID);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, Value[i]);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"AIDArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueAIDArray Deserialize(Stream buffer, byte flags)
        {
            UInt32[] value;
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            value = new UInt32[size];
            for (int i = 0; i < size; i++)
            {
                S7p.DecodeUInt32Vlq(buffer, out value[i]);
            }
            return new ValueAIDArray(value, flags);
        }
    }

    public class ValueBlob : PValue
    {
        UInt32 BlobRootId;
        byte[] Value;

        public ValueBlob(UInt32 blobRootId, byte[] value) : this(blobRootId, value, 0)
        {
        }

        public ValueBlob(UInt32 blobRootId, byte[] value, byte flags)
        {
            BlobRootId = blobRootId;
            DatatypeFlags = flags;
            // Im Protokoll ist ein Blob mit Größe Null erlaubt
            if (value != null)
            {
                Value = new byte[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public byte[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Blob);
            ret += S7p.EncodeUInt32Vlq(buffer, BlobRootId);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            ret += S7p.EncodeOctets(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type=\"Blob\" BlobRootId=\"" + BlobRootId.ToString() + "\">";
            if (Value != null)
            {
                s += BitConverter.ToString(Value);
            }
            s += "</Value>";
            return s;
        }

        public static ValueBlob Deserialize(Stream buffer, byte flags)
        {
            UInt32 blobRootId;
            UInt32 blobSize;
            byte[] value;
            S7p.DecodeUInt32Vlq(buffer, out blobRootId);
            S7p.DecodeUInt32Vlq(buffer, out blobSize);
            value = new byte[blobSize];
            S7p.DecodeOctets(buffer, (int)blobSize, out value);
            return new ValueBlob(blobRootId, value, flags);
        }
    }

    public class ValueBlobArray : PValue
    {
        ValueBlob[] Value;

        public ValueBlobArray(ValueBlob[] value) : this(value, FLAGS_ADDRESSARRAY)
        {
        }

        public ValueBlobArray(ValueBlob[] value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new ValueBlob[value.Length];
                Array.Copy(value, Value, value.Length);
            }
        }

        public ValueBlob[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Blob);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += Value[i].Serialize(buffer);
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"ValueBlobArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueBlobArray Deserialize(Stream buffer, byte flags)
        {
            UInt32 size = 0;
            S7p.DecodeUInt32Vlq(buffer, out size);
            ValueBlob[] value = new ValueBlob[size];
            for (int i = 0; i < size; i++)
            {
                value[i] = ValueBlob.Deserialize(buffer, flags);
            }
            return new ValueBlobArray(value, flags);
        }
    }

    public class ValueBlobSparseArray : PValue
    {
        public struct BlobEntry
        {
            public UInt32 blobRootId;
            public byte[] value;
        }

        Dictionary<UInt32, BlobEntry> Value;

        public ValueBlobSparseArray(Dictionary<UInt32, BlobEntry> value) : this(value, FLAGS_SPARSEARRAY)
        {
        }

        public ValueBlobSparseArray(Dictionary<UInt32, BlobEntry> value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new Dictionary<UInt32, BlobEntry>(value);
            }
        }

        public Dictionary<UInt32, BlobEntry> GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Blob);
            foreach (var v in Value)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, v.Key);
                ret += S7p.EncodeUInt32Vlq(buffer, v.Value.blobRootId);
                ret += S7p.EncodeUInt32Vlq(buffer, (uint)v.Value.value.Length);
                ret += S7p.EncodeOctets(buffer, v.Value.value);
            }
            ret += S7p.EncodeByte(buffer, 0);
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type=\"BlobSparseArray\">";
            foreach (var v in Value)
            {
                s += String.Format("<Value key=\"{0}\" BlobRootId=\"{1}\"", v.Key, v.Value.blobRootId);
                if (Value != null)
                {
                    s += BitConverter.ToString(v.Value.value);
                }
                s += "</Value>";
            }
            s += "</Value>";
            return s;
        }

        public static ValueBlobSparseArray Deserialize(Stream buffer, byte flags)
        {
            Dictionary<UInt32, BlobEntry> value = new Dictionary<UInt32, BlobEntry>();
            UInt32 k = 0;
            BlobEntry v = new BlobEntry();
            UInt32 blobSize = 0;
            S7p.DecodeUInt32Vlq(buffer, out k);
            while (k > 0)
            {
                S7p.DecodeUInt32Vlq(buffer, out v.blobRootId);
                S7p.DecodeUInt32Vlq(buffer, out blobSize);
                v.value = new byte[blobSize];
                S7p.DecodeOctets(buffer, (int)blobSize, out v.value);
                value.Add(k, v);

                S7p.DecodeUInt32Vlq(buffer, out k);
            }
            return new ValueBlobSparseArray(value, flags);
        }
    }

    public class ValueWString : PValue
    {
        string Value;

        public ValueWString(string value) : this(value, 0)
        {
        }

        public ValueWString(string value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public string GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.WString);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            ret += S7p.EncodeWString(buffer, Value);
            return ret;
        }

        public override string ToString()
        {
            return "<Value type=\"WString\">" + Value + "</Value>";
        }

        public static ValueWString Deserialize(Stream buffer, byte flags)
        {
            string value;
            UInt32 stringlen;
            S7p.DecodeUInt32Vlq(buffer, out stringlen);
            S7p.DecodeWString(buffer, (int)stringlen, out value);
            return new ValueWString(value, flags);
        }
    }

    public class ValueWStringArray : PValue
    {
        string[] Value;

        public ValueWStringArray(string[] value) : this(value, FLAGS_ADDRESSARRAY)
        {
        }

        public ValueWStringArray(string[] value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
        }

        public string[] GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.WString);
            ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, (uint)Value[i].Length);
                ret += S7p.EncodeWString(buffer, Value[i]);
            }               
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"WStringArray\" size=\"" + Value.Length.ToString() + "\">";
            for (int i = 0; i < Value.Length; i++)
            {
                s += String.Format("<Value>{0}</Value>", Value[i]);
            }
            s += "</Value>";
            return s;
        }

        public static ValueWStringArray Deserialize(Stream buffer, byte flags)
        {
            string[] value;
            UInt32 stringlen;
            UInt32 arraySize;
            S7p.DecodeUInt32Vlq(buffer, out arraySize);
            value = new string[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                S7p.DecodeUInt32Vlq(buffer, out stringlen);
                S7p.DecodeWString(buffer, (int)stringlen, out value[i]);
            }                
            return new ValueWStringArray(value, flags);
        }
    }

    public class ValueWStringSparseArray : PValue
    {
        Dictionary<UInt32, string> Value;

        public ValueWStringSparseArray(Dictionary<UInt32, string> value) : this(value, FLAGS_SPARSEARRAY)
        {
        }

        public ValueWStringSparseArray(Dictionary<UInt32, string> value, byte flags)
        {
            DatatypeFlags = flags;
            if (value != null)
            {
                Value = new Dictionary<UInt32, string>(value);
            }
        }

        public Dictionary<UInt32, string> GetValue()
        {
            return Value;
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.WString);
            foreach (var v in Value)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, v.Key);
                ret += S7p.EncodeUInt32Vlq(buffer, (uint)v.Value.Length);
                ret += S7p.EncodeWString(buffer, v.Value);
            }
            ret += S7p.EncodeByte(buffer, 0);
            return ret;
        }

        public override string ToString()
        {
            string s = "<Value type =\"WStringSparseArray\">";
            foreach (var v in Value)
            {
                s += String.Format("<Value key=\"{0}\">{1}</Value>", v.Key, v.Value);
            }
            s += "</Value>";
            return s;
        }

        public static ValueWStringSparseArray Deserialize(Stream buffer, byte flags)
        {
            Dictionary<UInt32, string> value = new Dictionary<UInt32, string>();
            UInt32 k = 0;
            UInt32 stringlen;
            string v = String.Empty;

            S7p.DecodeUInt32Vlq(buffer, out k);
            while (k > 0)
            {
                S7p.DecodeUInt32Vlq(buffer, out stringlen);
                S7p.DecodeWString(buffer, (int)stringlen, out v);
                value.Add(k, v);
                S7p.DecodeUInt32Vlq(buffer, out k);
            }
            return new ValueWStringSparseArray(value, flags);
        }
    }


    // TODO: Variant

    public class ValueStruct : PValue
    {
        UInt32 Value;
        private Dictionary<uint, PValue> Elements = new Dictionary<uint, PValue>();

        public ValueStruct(UInt32 value) : this (value, 0)
        {
        }

        public ValueStruct(UInt32 value, byte flags)
        {
            DatatypeFlags = flags;
            Value = value;
            Elements = new Dictionary<uint, PValue>();
        }

        public UInt32 GetValue()
        {
            return Value;
        }

        public void AddStructElement(uint id,  PValue elem)
        {
            Elements.Add(id, elem);
        }

        public PValue GetStructElement(uint id)
        {
            return Elements[id];
        }

        public override int Serialize(Stream buffer)
        {
            int ret = 0;

            ret += S7p.EncodeByte(buffer, DatatypeFlags);
            ret += S7p.EncodeByte(buffer, Datatype.Struct);
            ret += S7p.EncodeUInt32(buffer, Value);
            foreach (var elem in Elements)
            {
                ret += S7p.EncodeUInt32Vlq(buffer, elem.Key);
                ret += elem.Value.Serialize(buffer);
            }
            ret += S7p.EncodeByte(buffer, 0); // List Terminator
            return ret;
        }

        public override string ToString()
        {
            string s = "";
            s += "<Value type =\"Struct\">" + Environment.NewLine;
            s += "<ID>" + Value.ToString() + "</ID>" + Environment.NewLine;
            foreach (var elem in Elements)
            {
                s += "<Element>" + Environment.NewLine;
                s += "<ID>" + elem.Key.ToString() + "</ID>" + Environment.NewLine;
                s += elem.Value.ToString() + Environment.NewLine;
                s += "</Element>" + Environment.NewLine;
            }
            s += "</Value>" + Environment.NewLine;
            return s;
        }

        public static ValueStruct Deserialize(System.IO.Stream buffer, byte flags)
        {
            UInt32 value;
            ValueStruct stru;

            S7p.DecodeUInt32(buffer, out value);
            // Special handling of datatype struct and some specific ID ranges:
            // Some struct elements aren't transmitted as single elements. Instead they are packed (e.g. DTL-Struct).
            // The ID number range where this is used is only guessed (Type Info).
            if ((value > 0x90000000 && value < 0x9fffffff) || (value > 0x02000000 && value < 0x02ffffff))
            {
                // Packed Struct
                // TODO: 
                // Es handelt sich hierbei um System-Datentypen. Entweder es muss der Aufbau online aus der CPU angefragt werden,
                // oder vorher bekannt sein. Erkennung ist anhand value des ValueStruct möglich.
                // Daten vorerst hier als Bytearray abglegen, so werden sie auch übertragen. Interpretation bei Bedarf später.
                stru = new ValueStruct(value);

                UInt64 interfaceTimestamp;
                S7p.DecodeUInt64(buffer, out interfaceTimestamp);
                UInt32 transp_flags;
                S7p.DecodeUInt32Vlq(buffer, out transp_flags);
                UInt32 elementcount;
                S7p.DecodeUInt32Vlq(buffer, out elementcount);
                if ((transp_flags & 0x400) != 0)
                {
                    // Hier gibt es eine zusätzliche Zählung, warum auch immer...
                    S7p.DecodeUInt32Vlq(buffer, out elementcount);
                }
                
                byte[] barr = new byte[elementcount];
                for (int i = 0; i < elementcount; i++)
                {
                    S7p.DecodeByte(buffer, out barr[i]);
                }
                ValueByteArray elem = new ValueByteArray(barr);
                stru.AddStructElement(value, elem);
            }
            else
            {
                PValue elem;
                stru = new ValueStruct(value);
                S7p.DecodeUInt32Vlq(buffer, out value);
                while (value > 0)
                {
                    elem = PValue.Deserialize(buffer);
                    stru.AddStructElement(value, elem);
                    S7p.DecodeUInt32Vlq(buffer, out value);
                }
            }
            return stru;
        }
    }
}
