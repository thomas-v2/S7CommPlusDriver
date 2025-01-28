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

namespace S7CommPlusDriver.Alarming
{
    public class AlarmsAssociatedValues
    {
        public AssociatedValue SD_1;
        public AssociatedValue SD_2;
        public AssociatedValue SD_3;
        public AssociatedValue SD_4;
        public AssociatedValue SD_5;
        public AssociatedValue SD_6;
        public AssociatedValue SD_7;
        public AssociatedValue SD_8;
        public AssociatedValue SD_9;
        public AssociatedValue SD_10;

        public override string ToString()
        {
            string s = "<AlarmsAssociatedValues>" + Environment.NewLine;
            s += "<SD_1>" + (SD_1 is null ? String.Empty : SD_1.ToString()) + "</SD_1>" + Environment.NewLine;
            s += "<SD_2>" + (SD_2 is null ? String.Empty : SD_2.ToString()) + "</SD_2>" + Environment.NewLine;
            s += "<SD_3>" + (SD_3 is null ? String.Empty : SD_3.ToString()) + "</SD_3>" + Environment.NewLine;
            s += "<SD_4>" + (SD_4 is null ? String.Empty : SD_4.ToString()) + "</SD_4>" + Environment.NewLine;
            s += "<SD_5>" + (SD_5 is null ? String.Empty : SD_5.ToString()) + "</SD_5>" + Environment.NewLine;
            s += "<SD_6>" + (SD_6 is null ? String.Empty : SD_6.ToString()) + "</SD_6>" + Environment.NewLine;
            s += "<SD_7>" + (SD_7 is null ? String.Empty : SD_7.ToString()) + "</SD_7>" + Environment.NewLine;
            s += "<SD_8>" + (SD_8 is null ? String.Empty : SD_8.ToString()) + "</SD_8>" + Environment.NewLine;
            s += "<SD_9>" + (SD_9 is null ? String.Empty : SD_9.ToString()) + "</SD_9>" + Environment.NewLine;
            s += "<SD_10>" + (SD_10 is null ? String.Empty : SD_10.ToString()) + "</SD_10>" + Environment.NewLine;
            s += "</AlarmsAssociatedValues>" + Environment.NewLine;
            return s;
        }

        public static AlarmsAssociatedValues FromValueBlob(ValueBlobArray blob)
        {
            var av = new AlarmsAssociatedValues();
            var blobs = blob.GetValue();
            // Comes as Array[17], with indices:
            // 0 = Unknown Typeinformation, 4 Bytes
            // 1..10 = SD_1..SD_10
            //
            // The typeinformation at index 0 has a BlobRootId of 3476 = AS_CGS.AssociatedValues
            // When browsing 0x2000113 we get the result:
            // Type   Name
            // ---------------
            // UInt   Syntax
            // Byte   Aap
            int i = 0;
            AssociatedValue pv;
            foreach(var b in blobs)
            {
                var bytes = b.GetValue();
                switch (b.BlobRootId)
                {
                    case (Ids.TI_BOOL):
                        pv = new AssociatedValue(b.BlobRootId);
                        pv.SetBool(bytes[0] != 0);
                        av.SetSDValue(pv, i);
                        break;
                    case (Ids.TI_BYTE):
                        pv = new AssociatedValue(b.BlobRootId);
                        pv.SetInt(bytes[0]);
                        av.SetSDValue(pv, i);
                        break;
                    case (Ids.TI_CHAR):
                        pv = new AssociatedValue(b.BlobRootId);
                        pv.SetString(Encoding.GetEncoding("ISO-8859-1").GetString(bytes, 0, 1));
                        av.SetSDValue(pv, i);
                        break;
                    case (Ids.TI_WORD):
                        pv = new AssociatedValue(b.BlobRootId);
                        pv.SetInt(Utils.GetUInt16(bytes, 0));
                        av.SetSDValue(pv, i);
                        break;
                    case (Ids.TI_INT):
                        pv = new AssociatedValue(b.BlobRootId);
                        pv.SetInt(Utils.GetInt16(bytes, 0));
                        av.SetSDValue(pv, i);
                        break;
                    case (Ids.TI_DWORD):
                        pv = new AssociatedValue(b.BlobRootId);
                        pv.SetInt(Utils.GetUInt32(bytes, 0));
                        av.SetSDValue(pv, i);
                        break;
                    case (Ids.TI_DINT):
                        pv = new AssociatedValue(b.BlobRootId);
                        pv.SetInt(Utils.GetInt32(bytes, 0));
                        av.SetSDValue(pv, i);
                        break;
                    case (Ids.TI_REAL):
                        pv = new AssociatedValue(b.BlobRootId);
                        pv.SetReal(Utils.GetFloat(bytes, 0));
                        av.SetSDValue(pv, i);
                        break;
                    case (Ids.TI_LREAL):
                        pv = new AssociatedValue(b.BlobRootId);
                        pv.SetReal(Utils.GetDouble(bytes, 0));
                        av.SetSDValue(pv, i);
                        break;
                    case (Ids.TI_USINT):
                        pv = new AssociatedValue(b.BlobRootId);
                        pv.SetInt(bytes[0]);
                        av.SetSDValue(pv, i);
                        break;
                    case (Ids.TI_UINT):
                        pv = new AssociatedValue(b.BlobRootId);
                        pv.SetInt(Utils.GetUInt16(bytes, 0));
                        av.SetSDValue(pv, i);
                        break;
                    case (Ids.TI_UDINT):
                        pv = new AssociatedValue(b.BlobRootId);
                        pv.SetInt(Utils.GetUInt32(bytes, 0));
                        av.SetSDValue(pv, i);
                        break;
                    case (Ids.TI_SINT):
                        pv = new AssociatedValue(b.BlobRootId);
                        pv.SetInt((sbyte)bytes[0]);
                        av.SetSDValue(pv, i);
                        break;
                    case (Ids.TI_WCHAR):
                        pv = new AssociatedValue(b.BlobRootId);
                        pv.SetString(((char)Utils.GetUInt16(bytes, 0)).ToString());
                        av.SetSDValue(pv, i);
                        break;
                    default:
                        if (b.BlobRootId > Ids.TI_STRING_START && b.BlobRootId <= Ids.TI_STRING_END)
                        {
                            //byte s_maxlen = bytes[0]; // Don't need this value
                            byte s_actlen = bytes[1];
                            pv = new AssociatedValue(Ids.TI_STRING);
                            pv.SetString(Encoding.GetEncoding("ISO-8859-1").GetString(bytes, 2, s_actlen));
                            av.SetSDValue(pv, i);
                        }
                        else if (b.BlobRootId > Ids.TI_WSTRING_START && b.BlobRootId <= Ids.TI_WSTRING_END)
                        {
                            //int ws_maxlen = Utils.GetUInt16(bytes, 0); // Don't need this value
                            int ws_actlen = Utils.GetUInt16(bytes, 2);
                            pv = new AssociatedValue(Ids.TI_WSTRING);
                            pv.SetString(Encoding.BigEndianUnicode.GetString(bytes, 4, ws_actlen * 2));
                            av.SetSDValue(pv, i);
                        }
                        break;
                }
                i++;
                // All other elements have no value
                if (i > 10)
                {
                    break;
                }
            }
            return av;
        }

        private void SetSDValue(AssociatedValue v, int index)
        {
            switch(index)
            {
                case 1: SD_1 = v; break;
                case 2: SD_2 = v; break;
                case 3: SD_3 = v; break;
                case 4: SD_4 = v; break;
                case 5: SD_5 = v; break;
                case 6: SD_6 = v; break;
                case 7: SD_7 = v; break;
                case 8: SD_8 = v; break;
                case 9: SD_9 = v; break;
                case 10: SD_10 = v; break;
            }
        }
    }

    public class AssociatedValue
    {
        bool ValueBool;
        Int64 ValueInt;
        double ValueReal;
        string ValueString;

        public uint TypeInfo;
        // Allowed types in plc program: Bool, Byte, Char, DInt, DWord, Int, LReal, Real, SInt, String, UDInt, UInt, WChar, Word, WString
        // Break down to .Net types which can handle all these values: Bool, Int64, double, string

        public AssociatedValue(uint typeinfo)
        {
            TypeInfo = typeinfo;
        }

        public void SetBool(bool value)
        {
            ValueBool = value;
        }

        public void SetInt(Int64 value)
        {
            ValueInt = value;
        }

        public void SetReal(double value)
        {
            ValueReal = value;
        }

        public void SetString(string value)
        {
            ValueString = value;
        }

        public override string ToString()
        {
            string s = String.Empty;
            switch (TypeInfo)
            {
                case (Ids.TI_BOOL):
                    s = ValueBool.ToString();
                    break;
                case (Ids.TI_BYTE):
                    s = ValueInt.ToString();
                    break;
                case (Ids.TI_CHAR):
                    s = ValueString;
                    break;
                case (Ids.TI_WORD):
                    s = ValueInt.ToString();
                    break;
                case (Ids.TI_INT):
                    s = ValueInt.ToString();
                    break;
                case (Ids.TI_DWORD):
                    s = ValueInt.ToString();
                    break;
                case (Ids.TI_DINT):
                    s = ValueInt.ToString();
                    break;
                case (Ids.TI_REAL):
                    s = ValueReal.ToString();
                    break;
                case (Ids.TI_LREAL):
                    s = ValueReal.ToString();
                    break;
                case (Ids.TI_USINT):
                    s = ValueInt.ToString();
                    break;
                case (Ids.TI_UINT):
                    s = ValueInt.ToString();
                    break;
                case (Ids.TI_UDINT):
                    s = ValueInt.ToString();
                    break;
                case (Ids.TI_SINT):
                    s = ValueInt.ToString();
                    break;
                case (Ids.TI_WCHAR):
                    s = ValueString;
                    break;
                case (Ids.TI_STRING):
                    s = ValueString;
                    break;
                case (Ids.TI_WSTRING):
                    s = ValueString;
                    break;
            }
            return s;
        }
    }
}
