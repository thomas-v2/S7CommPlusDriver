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
    public interface IOffsetInfoType_Relation
    {
        UInt32 GetRelationId();
    }

    public interface IOffsetInfoType_MDim
    {
        Int32 GetArrayLowerBounds();
        UInt32 GetArrayElementCount();
        Int32[] GetMdimArrayLowerBounds();
        UInt32[] GetMdimArrayElementCount();
    }

    public interface IOffsetInfoType_1Dim
    {
        Int32 GetArrayLowerBounds();
        UInt32 GetArrayElementCount();
    }

    public abstract class POffsetInfoType
    {
        // Offsetinfo type for tag description.
        // Values 1..7 are used in old firmware version.
        // TLS supporting firmware version are using 8..15.
        public enum OffsetInfoType
        {
            FbArray = 0,
            StructElemStd = 1,
            StructElemString = 2,
            StructElemArray1Dim = 3,
            StructElemArrayMDim = 4,
            StructElemStruct = 5,
            StructElemStruct1Dim = 6,
            StructElemStructMDim = 7,
            Std = 8,
            String = 9,
            Array1Dim = 10,
            ArrayMDim = 11,
            Struct = 12,
            Struct1Dim = 13,
            StructMDim = 14,
            FbSfb = 15
        }

        public UInt32 OptimizedAddress;
        public UInt32 NonoptimizedAddress;

        public abstract bool HasRelation();
        public abstract bool Is1Dim();
        public abstract bool IsMDim();

        public static POffsetInfoType Deserialize(Stream buffer, int offsetinfotype, out int length)
        {
            switch ((OffsetInfoType)offsetinfotype)
            {
                case OffsetInfoType.FbArray:
                    return POffsetInfoType_FbArray.Deserialize(buffer, out length);
                case OffsetInfoType.StructElemStd:
                case OffsetInfoType.Std:
                    return POffsetInfoType_Std.Deserialize(buffer, out length, offsetinfotype);
                case OffsetInfoType.StructElemString:
                case OffsetInfoType.String:
                    return POffsetInfoType_String.Deserialize(buffer, out length);
                case OffsetInfoType.StructElemArray1Dim:
                case OffsetInfoType.Array1Dim:
                    return POffsetInfoType_Array1Dim.Deserialize(buffer, out length);
                case OffsetInfoType.StructElemArrayMDim:
                case OffsetInfoType.ArrayMDim:
                    return POffsetInfoType_ArrayMDim.Deserialize(buffer, out length);
                case OffsetInfoType.StructElemStruct:
                case OffsetInfoType.Struct:
                    return POffsetInfoType_Struct.Deserialize(buffer, out length);
                case OffsetInfoType.StructElemStruct1Dim:
                case OffsetInfoType.Struct1Dim:
                    return POffsetInfoType_Struct1Dim.Deserialize(buffer, out length);
                case OffsetInfoType.StructElemStructMDim:
                case OffsetInfoType.StructMDim:
                    return POffsetInfoType_StructMDim.Deserialize(buffer, out length);
                case OffsetInfoType.FbSfb:
                    return POffsetInfoType_FbSfb.Deserialize(buffer, out length);
            }
            length = 0;
            return null;
        }
    }

    public class POffsetInfoType_FbSfb : POffsetInfoType, IOffsetInfoType_Relation
    {
        public UInt16 UnspecifiedOffsetinfo1;
        public UInt16 UnspecifiedOffsetinfo2;
        public UInt32 RelationId;
        public UInt32 Info4;
        public UInt32 Info5;
        public UInt32 Info6;
        public UInt32 Info7;
        public UInt32 RetainSectionOffset;
        public UInt32 VolatileSectionOffset;

        public override bool HasRelation() { return true; }
        public override bool Is1Dim() { return false; }
        public override bool IsMDim() { return false; }

        public static POffsetInfoType_FbSfb Deserialize(Stream buffer, out int length)
        {
            int ret = 0;
            POffsetInfoType_FbSfb oi = new POffsetInfoType_FbSfb();

            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo1);
            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo2);

            ret += S7p.DecodeUInt32LE(buffer, out oi.OptimizedAddress);
            ret += S7p.DecodeUInt32LE(buffer, out oi.NonoptimizedAddress);

            ret += S7p.DecodeUInt32LE(buffer, out oi.RelationId);
            ret += S7p.DecodeUInt32LE(buffer, out oi.Info4);
            ret += S7p.DecodeUInt32LE(buffer, out oi.Info5);
            ret += S7p.DecodeUInt32LE(buffer, out oi.Info6);
            ret += S7p.DecodeUInt32LE(buffer, out oi.Info7);
            ret += S7p.DecodeUInt32LE(buffer, out oi.RetainSectionOffset);
            ret += S7p.DecodeUInt32LE(buffer, out oi.VolatileSectionOffset);

            length = ret;
            return oi;
        }

        public override string ToString()
        {
            string s = "";
            s += "<POffsetInfoType_FbSfb>" + Environment.NewLine;
            s += "<UnspecifiedOffsetinfo1>" + UnspecifiedOffsetinfo1.ToString() + "</UnspecifiedOffsetinfo1>" + Environment.NewLine;
            s += "<UnspecifiedOffsetinfo2>" + UnspecifiedOffsetinfo2.ToString() + "</UnspecifiedOffsetinfo2>" + Environment.NewLine;

            s += "<OptimizedAddress>" + OptimizedAddress.ToString() + "</OptimizedAddress>" + Environment.NewLine;
            s += "<NonoptimizedAddress>" + NonoptimizedAddress.ToString() + "</NonoptimizedAddress>" + Environment.NewLine;

            s += "<RelationId>" + RelationId.ToString() + "</RelationId>" + Environment.NewLine;
            s += "<Info4>" + Info4.ToString() + "</Info4>" + Environment.NewLine;
            s += "<Info5>" + Info5.ToString() + "</Info5>" + Environment.NewLine;
            s += "<Info6>" + Info6.ToString() + "</Info6>" + Environment.NewLine;
            s += "<Info7>" + Info7.ToString() + "</Info7>" + Environment.NewLine;

            s += "<RetainSectionOffset>" + RetainSectionOffset.ToString() + "</RetainSectionOffset>" + Environment.NewLine;
            s += "<VolatileSectionOffset>" + VolatileSectionOffset.ToString() + "</VolatileSectionOffset>" + Environment.NewLine;

            s += "</POffsetInfoType_FbSfb>" + Environment.NewLine;

            return s;
        }

        public uint GetRelationId()
        {
            return RelationId;
        }
    }

    public class POffsetInfoType_StructMDim : POffsetInfoType, IOffsetInfoType_Relation, IOffsetInfoType_MDim
    {
        public UInt16 UnspecifiedOffsetinfo1;
        public UInt16 UnspecifiedOffsetinfo2;
        public Int32 ArrayLowerBounds;
        public UInt32 ArrayElementCount;
        public Int32[] MdimArrayLowerBounds = new Int32[6];
        public UInt32[] MdimArrayElementCount = new UInt32[6];
        public UInt32 NonoptimizedStructSize;
        public UInt32 OptimizedStructSize;
        public UInt32 RelationId;
        public UInt32 StructInfo4;
        public UInt32 StructInfo5;
        public UInt32 StructInfo6;
        public UInt32 StructInfo7;

        public override bool HasRelation() { return true; }
        public override bool Is1Dim() { return false; }
        public override bool IsMDim() { return true; }

        public static POffsetInfoType_StructMDim Deserialize(Stream buffer, out int length)
        {
            int ret = 0;
            POffsetInfoType_StructMDim oi = new POffsetInfoType_StructMDim();

            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo1);
            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo2);

            ret += S7p.DecodeUInt32LE(buffer, out oi.OptimizedAddress);
            ret += S7p.DecodeUInt32LE(buffer, out oi.NonoptimizedAddress);

            ret += S7p.DecodeInt32LE(buffer, out oi.ArrayLowerBounds);
            ret += S7p.DecodeUInt32LE(buffer, out oi.ArrayElementCount);

            for (int d = 0; d < 6; d++)
            {
                ret += S7p.DecodeInt32LE(buffer, out oi.MdimArrayLowerBounds[d]);
            }
            for (int d = 0; d < 6; d++)
            {
                ret += S7p.DecodeUInt32LE(buffer, out oi.MdimArrayElementCount[d]);
            }

            ret += S7p.DecodeUInt32LE(buffer, out oi.NonoptimizedStructSize);
            ret += S7p.DecodeUInt32LE(buffer, out oi.OptimizedStructSize);

            ret += S7p.DecodeUInt32LE(buffer, out oi.RelationId);
            ret += S7p.DecodeUInt32LE(buffer, out oi.StructInfo4);
            ret += S7p.DecodeUInt32LE(buffer, out oi.StructInfo5);
            ret += S7p.DecodeUInt32LE(buffer, out oi.StructInfo6);
            ret += S7p.DecodeUInt32LE(buffer, out oi.StructInfo7);

            length = ret;
            return oi;
        }

        public override string ToString()
        {
            string s = "";
            s += "<POffsetInfoType_StructMDim>" + Environment.NewLine;
            s += "<UnspecifiedOffsetinfo1>" + UnspecifiedOffsetinfo1.ToString() + "</UnspecifiedOffsetinfo1>" + Environment.NewLine;
            s += "<UnspecifiedOffsetinfo2>" + UnspecifiedOffsetinfo2.ToString() + "</UnspecifiedOffsetinfo2>" + Environment.NewLine;

            s += "<OptimizedAddress>" + OptimizedAddress.ToString() + "</OptimizedAddress>" + Environment.NewLine;
            s += "<NonoptimizedAddress>" + NonoptimizedAddress.ToString() + "</NonoptimizedAddress>" + Environment.NewLine;

            s += "<ArrayLowerBounds>" + ArrayLowerBounds.ToString() + "</ArrayLowerBounds>" + Environment.NewLine;
            s += "<ArrayElementCount>" + ArrayElementCount.ToString() + "</ArrayElementCount>" + Environment.NewLine;

            for (int d = 0; d < 6; d++)
            {
                s += "<MdimArrayLowerBounds[" + d + "]>" + MdimArrayLowerBounds[d].ToString() + "</MdimArrayLowerBounds[" + d + "]>"+ Environment.NewLine;
            }
            for (int d = 0; d < 6; d++)
            {
                s += "<MdimArrayElementCount[" + d + "]>" + MdimArrayElementCount[d].ToString() + "</MdimArrayElementCount[" + d + "]>"+ Environment.NewLine;
            }

            s += "<OptimizedStructSize>" + OptimizedStructSize.ToString() + "</OptimizedStructSize>" + Environment.NewLine;
            s += "<NonoptimizedStructSize>" + NonoptimizedStructSize.ToString() + "</NonoptimizedStructSize>" + Environment.NewLine;

            s += "<RelationId>" + RelationId.ToString() + "</RelationId>" + Environment.NewLine;
            s += "<StructInfo4>" + StructInfo4.ToString() + "</StructInfo4>" + Environment.NewLine;
            s += "<StructInfo5>" + StructInfo5.ToString() + "</StructInfo5>" + Environment.NewLine;
            s += "<StructInfo6>" + StructInfo6.ToString() + "</StructInfo6>" + Environment.NewLine;
            s += "<StructInfo7>" + StructInfo7.ToString() + "</StructInfo7>" + Environment.NewLine;

            s += "</POffsetInfoType_StructMDim>" + Environment.NewLine;

            return s;
        }

        public uint GetRelationId()
        {
            return RelationId;
        }

        public Int32 GetArrayLowerBounds()
        {
            return ArrayLowerBounds;
        }

        public UInt32 GetArrayElementCount()
        {
                return ArrayElementCount;
        }

        public Int32[] GetMdimArrayLowerBounds()
        {
            return MdimArrayLowerBounds;
        }

        public UInt32[] GetMdimArrayElementCount()
        {
            return MdimArrayElementCount;
        }
    }

    public class POffsetInfoType_Struct1Dim : POffsetInfoType, IOffsetInfoType_Relation, IOffsetInfoType_1Dim
    {
        public UInt16 UnspecifiedOffsetinfo1;
        public UInt16 UnspecifiedOffsetinfo2;
        public Int32 ArrayLowerBounds;
        public UInt32 ArrayElementCount;
        public UInt32 NonoptimizedStructSize;
        public UInt32 OptimizedStructSize;
        public UInt32 RelationId;
        public UInt32 StructInfo4;
        public UInt32 StructInfo5;
        public UInt32 StructInfo6;
        public UInt32 StructInfo7;

        public override bool HasRelation() { return true; }
        public override bool Is1Dim() { return true; }
        public override bool IsMDim() { return false; }

        public static POffsetInfoType_Struct1Dim Deserialize(Stream buffer, out int length)
        {
            int ret = 0;
            POffsetInfoType_Struct1Dim oi = new POffsetInfoType_Struct1Dim();

            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo1);
            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo2);

            ret += S7p.DecodeUInt32LE(buffer, out oi.OptimizedAddress);
            ret += S7p.DecodeUInt32LE(buffer, out oi.NonoptimizedAddress);

            ret += S7p.DecodeInt32LE(buffer, out oi.ArrayLowerBounds);
            ret += S7p.DecodeUInt32LE(buffer, out oi.ArrayElementCount);

            ret += S7p.DecodeUInt32LE(buffer, out oi.NonoptimizedStructSize);
            ret += S7p.DecodeUInt32LE(buffer, out oi.OptimizedStructSize);

            ret += S7p.DecodeUInt32LE(buffer, out oi.RelationId);
            ret += S7p.DecodeUInt32LE(buffer, out oi.StructInfo4);
            ret += S7p.DecodeUInt32LE(buffer, out oi.StructInfo5);
            ret += S7p.DecodeUInt32LE(buffer, out oi.StructInfo6);
            ret += S7p.DecodeUInt32LE(buffer, out oi.StructInfo7);

            length = ret;
            return oi;
        }

        public override string ToString()
        {
            string s = "";
            s += "<POffsetInfoType_Struct1Dim>" + Environment.NewLine;

            s += "<UnspecifiedOffsetinfo1>" + UnspecifiedOffsetinfo1.ToString() + "</UnspecifiedOffsetinfo1>" + Environment.NewLine;
            s += "<UnspecifiedOffsetinfo2>" + UnspecifiedOffsetinfo2.ToString() + "</UnspecifiedOffsetinfo2>" + Environment.NewLine;

            s += "<OptimizedAddress>" + OptimizedAddress.ToString() + "</OptimizedAddress>" + Environment.NewLine;
            s += "<NonoptimizedAddress>" + NonoptimizedAddress.ToString() + "</NonoptimizedAddress>" + Environment.NewLine;

            s += "<ArrayLowerBounds>" + ArrayLowerBounds.ToString() + "</ArrayLowerBounds>" + Environment.NewLine;
            s += "<ArrayElementCount>" + ArrayElementCount.ToString() + "</ArrayElementCount>" + Environment.NewLine;

            s += "<OptimizedStructSize>" + OptimizedStructSize.ToString() + "</OptimizedStructSize>" + Environment.NewLine;
            s += "<NonoptimizedStructSize>" + NonoptimizedStructSize.ToString() + "</NonoptimizedStructSize>" + Environment.NewLine;

            s += "<RelationId>" + RelationId.ToString() + "</RelationId>" + Environment.NewLine;
            s += "<StructInfo4>" + StructInfo4.ToString() + "</StructInfo4>" + Environment.NewLine;
            s += "<StructInfo5>" + StructInfo5.ToString() + "</StructInfo5>" + Environment.NewLine;
            s += "<StructInfo6>" + StructInfo6.ToString() + "</StructInfo6>" + Environment.NewLine;
            s += "<StructInfo7>" + StructInfo7.ToString() + "</StructInfo7>" + Environment.NewLine;

            s += "</POffsetInfoType_Struct1Dim>" + Environment.NewLine;

            return s;
        }

        public uint GetRelationId()
        {
            return RelationId;
        }

        public Int32 GetArrayLowerBounds()
        {
            return ArrayLowerBounds;
        }

        public UInt32 GetArrayElementCount()
        {
            return ArrayElementCount;
        }
    }

    public class POffsetInfoType_Struct : POffsetInfoType, IOffsetInfoType_Relation
    {
        public UInt16 UnspecifiedOffsetinfo1;
        public UInt16 UnspecifiedOffsetinfo2;
        public UInt32 RelationId;
        public UInt32 StructInfo4;
        public UInt32 StructInfo5;
        public UInt32 StructInfo6;
        public UInt32 StructInfo7;

        public override bool HasRelation() { return true; }
        public override bool Is1Dim() { return false; }
        public override bool IsMDim() { return false; }

        public static POffsetInfoType_Struct Deserialize(Stream buffer, out int length)
        {
            int ret = 0;
            POffsetInfoType_Struct oi = new POffsetInfoType_Struct();

            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo1);
            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo2);

            ret += S7p.DecodeUInt32LE(buffer, out oi.OptimizedAddress);
            ret += S7p.DecodeUInt32LE(buffer, out oi.NonoptimizedAddress);

            ret += S7p.DecodeUInt32LE(buffer, out oi.RelationId);
            ret += S7p.DecodeUInt32LE(buffer, out oi.StructInfo4);
            ret += S7p.DecodeUInt32LE(buffer, out oi.StructInfo5);
            ret += S7p.DecodeUInt32LE(buffer, out oi.StructInfo6);
            ret += S7p.DecodeUInt32LE(buffer, out oi.StructInfo7);

            length = ret;
            return oi;
        }

        public override string ToString()
        {
            string s = "";
            s += "<POffsetInfoType_Struct>" + Environment.NewLine;

            s += "<UnspecifiedOffsetinfo1>" + UnspecifiedOffsetinfo1.ToString() + "</UnspecifiedOffsetinfo1>" + Environment.NewLine;
            s += "<UnspecifiedOffsetinfo2>" + UnspecifiedOffsetinfo2.ToString() + "</UnspecifiedOffsetinfo2>" + Environment.NewLine;

            s += "<OptimizedAddress>" + OptimizedAddress.ToString() + "</OptimizedAddress>" + Environment.NewLine;
            s += "<NonoptimizedAddress>" + NonoptimizedAddress.ToString() + "</NonoptimizedAddress>" + Environment.NewLine;

            s += "<RelationId>" + RelationId.ToString() + "</RelationId>" + Environment.NewLine;
            s += "<StructInfo4>" + StructInfo4.ToString() + "</StructInfo4>" + Environment.NewLine;
            s += "<StructInfo5>" + StructInfo5.ToString() + "</StructInfo5>" + Environment.NewLine;
            s += "<StructInfo6>" + StructInfo6.ToString() + "</StructInfo6>" + Environment.NewLine;
            s += "<StructInfo7>" + StructInfo7.ToString() + "</StructInfo7>" + Environment.NewLine;

            s += "</POffsetInfoType_Struct>" + Environment.NewLine;

            return s;
        }

        public uint GetRelationId()
        {
            return RelationId;
        }
    }

    public class POffsetInfoType_ArrayMDim : POffsetInfoType, IOffsetInfoType_MDim
    {
        public UInt16 UnspecifiedOffsetinfo1;
        public UInt16 UnspecifiedOffsetinfo2;
        public Int32 ArrayLowerBounds;
        public UInt32 ArrayElementCount;
        public Int32[] MdimArrayLowerBounds = new Int32[6];
        public UInt32[] MdimArrayElementCount = new UInt32[6];

        public override bool HasRelation() { return false; }
        public override bool Is1Dim() { return false; }
        public override bool IsMDim() { return true; }

        public static POffsetInfoType_ArrayMDim Deserialize(Stream buffer, out int length)
        {
            int ret = 0;
            POffsetInfoType_ArrayMDim oi = new POffsetInfoType_ArrayMDim();

            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo1);
            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo2);

            ret += S7p.DecodeUInt32LE(buffer, out oi.OptimizedAddress);
            ret += S7p.DecodeUInt32LE(buffer, out oi.NonoptimizedAddress);

            ret += S7p.DecodeInt32LE(buffer, out oi.ArrayLowerBounds);
            ret += S7p.DecodeUInt32LE(buffer, out oi.ArrayElementCount);

            for (int d = 0; d < 6; d++)
            {
                ret += S7p.DecodeInt32LE(buffer, out oi.MdimArrayLowerBounds[d]);
            }
            for (int d = 0; d < 6; d++)
            {
                ret += S7p.DecodeUInt32LE(buffer, out oi.MdimArrayElementCount[d]);
            }

            length = ret;
            return oi;
        }

        public override string ToString()
        {
            string s = "";
            s += "<POffsetInfoType_ArrayMDim>" + Environment.NewLine;

            s += "<UnspecifiedOffsetinfo1>" + UnspecifiedOffsetinfo1.ToString() + "</UnspecifiedOffsetinfo1>" + Environment.NewLine;
            s += "<UnspecifiedOffsetinfo2>" + UnspecifiedOffsetinfo2.ToString() + "</UnspecifiedOffsetinfo2>" + Environment.NewLine;

            s += "<OptimizedAddress>" + OptimizedAddress.ToString() + "</OptimizedAddress>" + Environment.NewLine;
            s += "<NonoptimizedAddress>" + NonoptimizedAddress.ToString() + "</NonoptimizedAddress>" + Environment.NewLine;

            s += "<ArrayLowerBounds>" + ArrayLowerBounds.ToString() + "</ArrayLowerBounds>" + Environment.NewLine;
            s += "<ArrayElementCount>" + ArrayElementCount.ToString() + "</ArrayElementCount>" + Environment.NewLine;

            for (int d = 0; d < 6; d++)
            {
                s += "<MdimArrayLowerBounds[" + d + "]>" + MdimArrayLowerBounds[d].ToString() + "</MdimArrayLowerBounds[" + d + "]>"+ Environment.NewLine;
            }
            for (int d = 0; d < 6; d++)
            {
                s += "<MdimArrayElementCount[" + d + "]>" + MdimArrayElementCount[d].ToString() + "</MdimArrayElementCount[" + d + "]>"+ Environment.NewLine;
            }

            s += "</POffsetInfoType_ArrayMDim>" + Environment.NewLine;

            return s;
        }

        public Int32 GetArrayLowerBounds()
        {
            return ArrayLowerBounds;
        }

        public UInt32 GetArrayElementCount()
        {
            return ArrayElementCount;
        }

        public Int32[] GetMdimArrayLowerBounds()
        {
            return MdimArrayLowerBounds;
        }

        public UInt32[] GetMdimArrayElementCount()
        {
            return MdimArrayElementCount;
        }
    }

    public class POffsetInfoType_Array1Dim : POffsetInfoType, IOffsetInfoType_1Dim
    {
        public UInt16 UnspecifiedOffsetinfo1;
        public UInt16 UnspecifiedOffsetinfo2;
        public Int32 ArrayLowerBounds;
        public UInt32 ArrayElementCount;

        public override bool HasRelation() { return false; }
        public override bool Is1Dim() { return true; }
        public override bool IsMDim() { return false; }

        public static POffsetInfoType_Array1Dim Deserialize(Stream buffer, out int length)
        {
            int ret = 0;
            POffsetInfoType_Array1Dim oi = new POffsetInfoType_Array1Dim();

            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo1);
            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo2);

            ret += S7p.DecodeUInt32LE(buffer, out oi.OptimizedAddress);
            ret += S7p.DecodeUInt32LE(buffer, out oi.NonoptimizedAddress);

            ret += S7p.DecodeInt32LE(buffer, out oi.ArrayLowerBounds);
            ret += S7p.DecodeUInt32LE(buffer, out oi.ArrayElementCount);

            length = ret;
            return oi;
        }

        public override string ToString()
        {
            string s = "";
            s += "<POffsetInfoType_Array1Dim>" + Environment.NewLine;

            s += "<UnspecifiedOffsetinfo1>" + UnspecifiedOffsetinfo1.ToString() + "</UnspecifiedOffsetinfo1>" + Environment.NewLine;
            s += "<UnspecifiedOffsetinfo2>" + UnspecifiedOffsetinfo2.ToString() + "</UnspecifiedOffsetinfo2>" + Environment.NewLine;

            s += "<OptimizedAddress>" + OptimizedAddress.ToString() + "</OptimizedAddress>" + Environment.NewLine;
            s += "<NonoptimizedAddress>" + NonoptimizedAddress.ToString() + "</NonoptimizedAddress>" + Environment.NewLine;

            s += "<ArrayLowerBounds>" + ArrayLowerBounds.ToString() + "</ArrayLowerBounds>" + Environment.NewLine;
            s += "<ArrayElementCount>" + ArrayElementCount.ToString() + "</ArrayElementCount>" + Environment.NewLine;

            s += "</POffsetInfoType_Array1Dim>" + Environment.NewLine;

            return s;
        }

        public Int32 GetArrayLowerBounds()
        {
            return ArrayLowerBounds;
        }

        public UInt32 GetArrayElementCount()
        {
            return ArrayElementCount;
        }
    }

    public class POffsetInfoType_String: POffsetInfoType
    {
        public UInt16 UnspecifiedOffsetinfo1;   // This is the max. length of the string
        public UInt16 UnspecifiedOffsetinfo2;   // max. lengh plus 2 bytes stringheader

        public override bool HasRelation() { return false; }
        public override bool Is1Dim() { return false; }
        public override bool IsMDim() { return false; }

        public static POffsetInfoType_String Deserialize(Stream buffer, out int length)
        {
            int ret = 0;
            POffsetInfoType_String oi = new POffsetInfoType_String();

            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo1);
            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo2);

            ret += S7p.DecodeUInt32LE(buffer, out oi.OptimizedAddress);
            ret += S7p.DecodeUInt32LE(buffer, out oi.NonoptimizedAddress);

            length = ret;
            return oi;
        }

        public override string ToString()
        {
            string s = "";
            s += "<POffsetInfoType_String>" + Environment.NewLine;

            s += "<UnspecifiedOffsetinfo1>" + UnspecifiedOffsetinfo1.ToString() + "</UnspecifiedOffsetinfo1>" + Environment.NewLine;
            s += "<UnspecifiedOffsetinfo2>" + UnspecifiedOffsetinfo2.ToString() + "</UnspecifiedOffsetinfo2>" + Environment.NewLine;

            s += "<OptimizedAddress>" + OptimizedAddress.ToString() + "</OptimizedAddress>" + Environment.NewLine;
            s += "<NonoptimizedAddress>" + NonoptimizedAddress.ToString() + "</NonoptimizedAddress>" + Environment.NewLine;

            s += "</POffsetInfoType_String>" + Environment.NewLine;

            return s;
        }
    }

    public class POffsetInfoType_Std: POffsetInfoType
    {
        public override bool HasRelation() { return false; }
        public override bool Is1Dim() { return false; }
        public override bool IsMDim() { return false; }

        public static POffsetInfoType_Std Deserialize(Stream buffer, out int length, int offsetinfotype)
        {
            int ret = 0;
            POffsetInfoType_Std oi = new POffsetInfoType_Std();
            // The order of addresses is swapped between old Std (8) and new (1) offsetinfotype.
            ushort v;
            if ((OffsetInfoType)offsetinfotype == OffsetInfoType.Std)
            {
                ret += S7p.DecodeUInt16LE(buffer, out v);
                oi.OptimizedAddress = v;
                ret += S7p.DecodeUInt16LE(buffer, out v);
                oi.NonoptimizedAddress = v;
            }
            else
            {
                ret += S7p.DecodeUInt16LE(buffer, out v);
                oi.NonoptimizedAddress = v;
                ret += S7p.DecodeUInt16LE(buffer, out v);
                oi.OptimizedAddress = v;
            }
            length = ret;
            return oi;
        }

        public override string ToString()
        {
            string s = "";
            s += "<POffsetInfoType_Std>" + Environment.NewLine;

            s += "<OptimizedAddress>" + OptimizedAddress.ToString() + "</OptimizedAddress>" + Environment.NewLine;
            s += "<NonoptimizedAddress>" + NonoptimizedAddress.ToString() + "</NonoptimizedAddress>" + Environment.NewLine;

            s += "</POffsetInfoType_Std>" + Environment.NewLine;

            return s;
        }
    }

    public class POffsetInfoType_FbArray : POffsetInfoType, IOffsetInfoType_Relation
    {
        public UInt16 UnspecifiedOffsetinfo1;
        public UInt16 UnspecifiedOffsetinfo2;
        public UInt32 RelationId;
        public UInt32 Info4;
        public UInt32 Info5;
        public UInt32 Info6;
        public UInt32 Info7;
        public UInt32 RetainSectionOffset;
        public UInt32 VolatileSectionOffset;
        public UInt32 ArrayElementCount;
        public UInt32 ClassicSectionSize;
        public UInt32 RetainSectionSize;
        public UInt32 VolatileSectionSize;
        public Int32[] MdimArrayLowerBounds = new Int32[6];
        public UInt32[] MdimArrayElementCount = new UInt32[6];

        public override bool HasRelation() { return true; }
        public override bool Is1Dim() { return false; } //!!! TODO
        public override bool IsMDim() { return false; } //!!! TODO

        public static POffsetInfoType_FbArray Deserialize(Stream buffer, out int length)
        {
            int ret = 0;
            POffsetInfoType_FbArray oi = new POffsetInfoType_FbArray();

            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo1);
            ret += S7p.DecodeUInt16LE(buffer, out oi.UnspecifiedOffsetinfo2);

            ret += S7p.DecodeUInt32LE(buffer, out oi.OptimizedAddress);
            ret += S7p.DecodeUInt32LE(buffer, out oi.NonoptimizedAddress);

            ret += S7p.DecodeUInt32LE(buffer, out oi.RelationId);
            ret += S7p.DecodeUInt32LE(buffer, out oi.Info4);
            ret += S7p.DecodeUInt32LE(buffer, out oi.Info5);
            ret += S7p.DecodeUInt32LE(buffer, out oi.Info6);
            ret += S7p.DecodeUInt32LE(buffer, out oi.Info7);
            ret += S7p.DecodeUInt32LE(buffer, out oi.RetainSectionOffset);
            ret += S7p.DecodeUInt32LE(buffer, out oi.VolatileSectionOffset);
            ret += S7p.DecodeUInt32LE(buffer, out oi.ArrayElementCount);
            ret += S7p.DecodeUInt32LE(buffer, out oi.ClassicSectionSize);
            ret += S7p.DecodeUInt32LE(buffer, out oi.RetainSectionSize);
            ret += S7p.DecodeUInt32LE(buffer, out oi.VolatileSectionSize);

            for (int d = 0; d < 6; d++)
            {
                ret += S7p.DecodeInt32LE(buffer, out oi.MdimArrayLowerBounds[d]);
            }
            for (int d = 0; d < 6; d++)
            {
                ret += S7p.DecodeUInt32LE(buffer, out oi.MdimArrayElementCount[d]);
            }

            length = ret;
            return oi;
        }

        public override string ToString()
        {
            string s = "";
            s += "<POffsetInfoType_FbArray>" + Environment.NewLine;

            s += "<UnspecifiedOffsetinfo1>" + UnspecifiedOffsetinfo1.ToString() + "</UnspecifiedOffsetinfo1>" + Environment.NewLine;
            s += "<UnspecifiedOffsetinfo2>" + UnspecifiedOffsetinfo2.ToString() + "</UnspecifiedOffsetinfo2>" + Environment.NewLine;

            s += "<OptimizedAddress>" + OptimizedAddress.ToString() + "</OptimizedAddress>" + Environment.NewLine;
            s += "<NonoptimizedAddress>" + NonoptimizedAddress.ToString() + "</NonoptimizedAddress>" + Environment.NewLine;

            s += "<RelationId>" + RelationId.ToString() + "</RelationId>" + Environment.NewLine;
            s += "<Info4>" + Info4.ToString() + "</Info4>" + Environment.NewLine;
            s += "<Info5>" + Info5.ToString() + "</Info5>" + Environment.NewLine;
            s += "<Info6>" + Info6.ToString() + "</Info6>" + Environment.NewLine;
            s += "<Info7>" + Info7.ToString() + "</Info7>" + Environment.NewLine;

            s += "<RetainSectionOffset>" + RetainSectionOffset.ToString() + "</RetainSectionOffset>" + Environment.NewLine;
            s += "<VolatileSectionOffset>" + VolatileSectionOffset.ToString() + "</VolatileSectionOffset>" + Environment.NewLine;
            s += "<ClassicSectionSize>" + ClassicSectionSize.ToString() + "</ClassicSectionSize>" + Environment.NewLine;
            s += "<RetainSectionSize>" + RetainSectionSize.ToString() + "</RetainSectionSize>" + Environment.NewLine;
            s += "<VolatileSectionSize>" + VolatileSectionSize.ToString() + "</VolatileSectionSize>" + Environment.NewLine;

            for (int d = 0; d < 6; d++)
            {
                s += "<MdimArrayLowerBounds[" + d + "]>" + MdimArrayLowerBounds[d].ToString() + "</MdimArrayLowerBounds[" + d + "]>"+ Environment.NewLine;
            }
            for (int d = 0; d < 6; d++)
            {
                s += "<MdimArrayElementCount[" + d + "]>" + MdimArrayElementCount[d].ToString() + "</MdimArrayElementCount[" + d + "]>"+ Environment.NewLine;
            }

            s += "</POffsetInfoType_FbArray>" + Environment.NewLine;

            return s;
        }

        public uint GetRelationId()
        {
            return RelationId;
        }
    }
}
