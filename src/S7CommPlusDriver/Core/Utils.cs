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

namespace S7CommPlusDriver
{
    class Utils
    {
        public static string HexDump(byte[] bytes, int bytesPerLine = 16)
        {
            if (bytes == null) return "<null>";
            int bytesLength = bytes.Length;

            char[] HexChars = "0123456789ABCDEF".ToCharArray();

            int firstHexColumn =
                  8                             // 8 characters for the address
                + 3;                            // 3 spaces

            int firstCharColumn = firstHexColumn
                + bytesPerLine * 3              // - 2 digit for the hexadecimal value and 1 space
                + (bytesPerLine - 1) / 8        // - 1 extra space every 8 characters from the 9th
                + 2;                            // 2 spaces 

            int lineLength = firstCharColumn
                + bytesPerLine                  // - characters to show the ascii value
                + Environment.NewLine.Length;   // Carriage return and line feed (should normally be 2)

            char[] line = (new String(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new StringBuilder(expectedLines * lineLength);

            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = (b < 32 ? '·' : (char)b);
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                result.Append(line);
            }
            return result.ToString();
        }

        public static DateTime DtFromValueTimestamp(UInt64 value)
        {
            // Protocol ValueTimestamp is number of nanoseconds from 1. Jan 1970 (Unit Time).
            // .Net DateTime tick is 100 ns based
            ulong epochTicks = 621355968000000000; // Unix Time (UTC) on 1st January 1970.
            return new DateTime((long)((value / 100) + epochTicks), DateTimeKind.Utc);
        }

        public static byte GetUInt8(byte[] array, uint pos)
        {
            return array[pos];
        }

        public static ushort GetUInt16LE(byte[] array, uint pos)
        {
            return (ushort)(array[pos + 1] * 256 + array[pos]);
        }

        public static ushort GetUInt16(byte[] array, uint pos)
        {
            return (ushort)(array[pos] * 256 + array[pos + 1]);
        }

        public static short GetInt16(byte[] array, uint pos)
        {
            return (short)((array[pos] << 8) | array[pos + 1]);
        }

        public static uint GetUInt32LE(byte[] array, uint pos)
        {
            return (uint)array[pos + 3] * 16777216 + (uint)array[pos + 2] * 65536 + (uint)array[pos + 1] * 256 + (uint)array[pos];
        }

        public static uint GetUInt32(byte[] array, uint pos)
        {
            return (uint)array[pos] * 16777216 + (uint)array[pos + 1] * 65536 + (uint)array[pos + 2] * 256 + (uint)array[pos + 3];
        }

        public static int GetInt32(byte[] array, uint pos)
        {
            return (int)((array[pos] << 24) | (array[pos + 1] << 16) | (array[pos + 2] << 8) | array[pos + 3]);
        }

        public static float GetFloat(byte[] array, uint pos)
        {
            byte[] v = new byte[4];
            v[3] = array[pos];
            v[2] = array[pos + 1];
            v[1] = array[pos + 2];
            v[0] = array[pos + 3];
            return BitConverter.ToSingle(v, 0);
        }

        public static double GetDouble(byte[] array, uint pos)
        {
            byte[] v = new byte[8];
            v[7] = array[pos];
            v[6] = array[pos + 1];
            v[5] = array[pos + 2];
            v[4] = array[pos + 3];
            v[3] = array[pos + 4];
            v[2] = array[pos + 5];
            v[1] = array[pos + 6];
            v[0] = array[pos + 7];
            return BitConverter.ToDouble(v, 0);
        }

        public static String GetUtfString(byte[] array, uint pos, uint len)
        {
            return System.Text.Encoding.UTF8.GetString(array, (int)pos, (int)len);
        }
    }
}
