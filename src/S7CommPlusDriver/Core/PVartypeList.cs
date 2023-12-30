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
    public class PVartypeList
    {
        public List<PVartypeListElement> Elements;
        public UInt32 FirstId;

        public int Deserialize(Stream buffer)
        {
            int ret = 0;
            int maxret;
            UInt16 blocklen = 0;

            Elements = new List<PVartypeListElement>();

            ret += S7p.DecodeUInt16(buffer, out blocklen);
            maxret = ret + blocklen;
            // This ID occurs only on the first block.
            ret += S7p.DecodeUInt32LE(buffer, out FirstId);

            while (blocklen > 0)
            {
                do
                {
                    var elem = new PVartypeListElement();
                    ret += elem.Deserialize(buffer);
                    Elements.Add(elem);
                } while (ret < maxret);
                ret += S7p.DecodeUInt16(buffer, out blocklen);
                maxret = ret + blocklen;
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "";
            s += "<VartypeList>" + Environment.NewLine;
            s += "<FirstId>" + FirstId.ToString() + "</FirstId>" + Environment.NewLine;
            int i = 1;
            foreach (var elem in Elements)
            {
                s += "<Element index=\"" + i.ToString() + "\">" + Environment.NewLine;
                s += elem.ToString();
                s += "</Element>" + Environment.NewLine;
                i++;
            }
            s += "</VartypeList>" + Environment.NewLine;
            return s;
        }
    }

    public class PVartypeListElement
    {
        /* flags in tag description for 1500 */
        const int S7COMMP_TAGDESCR_ATTRIBUTE2_OFFSETINFOTYPE = 0xf000;      /* Bits 13..16 */
        const int S7COMMP_TAGDESCR_ATTRIBUTE2_HMIVISIBLE = 0x0800;          /* Bit 12 */
        const int S7COMMP_TAGDESCR_ATTRIBUTE2_HMIREADONLY = 0x0400;         /* Bit 11 */
        const int S7COMMP_TAGDESCR_ATTRIBUTE2_HMIACCESSIBLE = 0x0200;       /* Bit 10 */
        const int S7COMMP_TAGDESCR_ATTRIBUTE2_BIT09 = 0x0100;               /* Bit 09 */
        const int S7COMMP_TAGDESCR_ATTRIBUTE2_OPTIMIZEDACCESS = 0x0080;     /* Bit 08 */
        const int S7COMMP_TAGDESCR_ATTRIBUTE2_SECTION = 0x0070;             /* Bits 05..07 */
        const int S7COMMP_TAGDESCR_ATTRIBUTE2_BIT04 = 0x0008;               /* Bit 04 */
        const int S7COMMP_TAGDESCR_ATTRIBUTE2_BITOFFSET = 0x0007;           /* Bits 01..03 */

        /* Offsetinfo type for tag description (S7-1500) */
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_FB_ARRAY = 0;
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_STRUCTELEM_STD = 1;
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_STRUCTELEM_STRING = 2;
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_STRUCTELEM_ARRAY1DIM = 3;
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_STRUCTELEM_ARRAYMDIM = 4;
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_STRUCTELEM_STRUCT = 5;
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_STRUCTELEM_STRUCT1DIM = 6;
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_STRUCTELEM_STRUCTMDIM = 7;
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_STD = 8;
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_STRING = 9;
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_ARRAY1DIM = 10;
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_ARRAYMDIM = 11;
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_STRUCT = 12;
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_STRUCT1DIM = 13;
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_STRUCTMDIM = 14;
        const int S7COMMP_TAGDESCR_OFFSETINFOTYPE2_PROGRAMALARM = 15;

        const int S7COMMP_TAGDESCR_BITOFFSETINFO_RETAIN = 0x80;
        const int S7COMMP_TAGDESCR_BITOFFSETINFO_NONOPTBITOFFSET = 0x70;
        const int S7COMMP_TAGDESCR_BITOFFSETINFO_CLASSIC = 0x08;
        const int S7COMMP_TAGDESCR_BITOFFSETINFO_OPTBITOFFSET = 0x07;

        public UInt32 LID;
        public UInt32 SymbolCrc;
        public UInt32 Softdatatype;
        public UInt16 AttributeFlags;

        public int GetAttributeSection()
        {
            return ((AttributeFlags & S7COMMP_TAGDESCR_ATTRIBUTE2_SECTION) >> 4);
        }

        public int GetAttributeBitoffset()
        {
            return (AttributeFlags & S7COMMP_TAGDESCR_ATTRIBUTE2_BITOFFSET);
        }

        public bool GetAttributeFlagHmiVisible()
        {
            return ((AttributeFlags & S7COMMP_TAGDESCR_ATTRIBUTE2_HMIVISIBLE) != 0);
        }

        public bool GetAttributeFlagHmiAccessible()
        {
            return ((AttributeFlags & S7COMMP_TAGDESCR_ATTRIBUTE2_HMIACCESSIBLE) != 0);
        }

        public bool GetAttributeFlagOptimizedAccess()
        {
            return ((AttributeFlags & S7COMMP_TAGDESCR_ATTRIBUTE2_OPTIMIZEDACCESS) != 0);
        }

        public bool GetAttributeFlagHmiReadonly()
        {
            return ((AttributeFlags & S7COMMP_TAGDESCR_ATTRIBUTE2_HMIREADONLY) != 0);
        }

        public byte BitoffsetinfoFlags;

        public bool GetBitoffsetinfoFlagRetain()
        {
            return ((BitoffsetinfoFlags & S7COMMP_TAGDESCR_BITOFFSETINFO_RETAIN) != 0);
        }

        public bool GetBitoffsetinfoFlagClassic()
        {
            return ((BitoffsetinfoFlags & S7COMMP_TAGDESCR_BITOFFSETINFO_CLASSIC) != 0);
        }

        public int GetBitoffsetinfoNonoptimizedBitoffset()
        {
            return ((BitoffsetinfoFlags & S7COMMP_TAGDESCR_BITOFFSETINFO_NONOPTBITOFFSET) >> 4);
        }

        public int GetBitoffsetinfoOptimizedBitoffset()
        {
            return (BitoffsetinfoFlags & S7COMMP_TAGDESCR_BITOFFSETINFO_OPTBITOFFSET);
        }

        public POffsetInfoType OffsetInfoType;

        public int Deserialize(Stream buffer)
        {
            int ret = 0;
            byte bval = 0;
            int offsetinfotype;

            ret += S7p.DecodeUInt32LE(buffer, out LID);
            ret += S7p.DecodeUInt32LE(buffer, out SymbolCrc);
            ret += S7p.DecodeByte(buffer, out bval);
            Softdatatype = bval;    // For keepint the type similar
            ret += S7p.DecodeUInt16(buffer, out AttributeFlags);

            offsetinfotype = ((AttributeFlags & S7COMMP_TAGDESCR_ATTRIBUTE2_OFFSETINFOTYPE) >> 12);

            ret += S7p.DecodeByte(buffer, out BitoffsetinfoFlags);

            int length = 0;
            OffsetInfoType = POffsetInfoType.Deserialize(buffer, offsetinfotype, out length);
            ret += length;

            return ret;
        }

        public override string ToString()
        {
            string s = "";
            s += "<VartypeListElement>" + Environment.NewLine;
            s += "<LID>" + LID.ToString() + "</LID>" + Environment.NewLine;
            s += "<SymbolCRC>" + SymbolCrc.ToString() + "</SymbolCRC>" + Environment.NewLine;
            s += "<AttributeFlags>" + AttributeFlags.ToString() + "</AttributeFlags>" + Environment.NewLine;
            s += "<BitoffsetinfoFlags>" + BitoffsetinfoFlags.ToString() + "</BitoffsetinfoFlags>" + Environment.NewLine;
            s += "<OffsetInfoType>" + OffsetInfoType.ToString() + "</OffsetInfoType>" + Environment.NewLine;
            s += "</VartypeListElement>" + Environment.NewLine;
            return s;
        }
    }
}    
