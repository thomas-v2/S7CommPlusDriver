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
using System.Linq;
using S7CommPlusDriver;
using S7CommPlusDriver.ClientApi;

namespace DriverTest
{
    public class TestPlcTag
    {
        // If the DB is created from source, the .x IDs should
        // be remain the same, if there's no change in the DB at the variables after compiling.
        // 8A0E000F is in this case 0x05 = DB15.
        // The pointer variable types can only be located in an instance DB of a FB.
        public string PlcTagBoolAddress = "8A0E000F.9";
        public string PlcTagByteAddress = "8A0E000F.A";
        public string PlcTagWordAddress = "8A0E000F.B";
        public string PlcTagDWordAddress = "8A0E000F.C";
        public string PlcTagLWordAddress = "8A0E000F.D";
        public string PlcTagUSIntAddress = "8A0E000F.E";
        public string PlcTagSIntAddress = "8A0E000F.F";
        public string PlcTagUIntAddress = "8A0E000F.10";
        public string PlcTagIntAddress = "8A0E000F.11";
        public string PlcTagUDIntAddress = "8A0E000F.12";
        public string PlcTagDIntAddress = "8A0E000F.13";
        public string PlcTagULIntAddress = "8A0E000F.14";
        public string PlcTagLIntAddress = "8A0E000F.15";
        public string PlcTagRealAddress = "8A0E000F.16";
        public string PlcTagLRealAddress = "8A0E000F.17";
        public string PlcTagTimeAddress = "8A0E000F.18";
        public string PlcTagLTimeAddress = "8A0E000F.19";
        public string PlcTagS5TimeAddress = "8A0E000F.1A";
        public string PlcTagDateAddress = "8A0E000F.1B";
        public string PlcTagDateAndTimeAddress = "8A0E000F.1C";
        public string PlcTagDTLAddress = "8A0E000F.1D";
        public string PlcTagLDTAddress = "8A0E000F.1E";
        public string PlcTagTimeOfDayAddress = "8A0E000F.1F";
        public string PlcTagLTODAddress = "8A0E000F.20";
        public string PlcTagCharAddress = "8A0E000F.21";
        public string PlcTagWCharAddress = "8A0E000F.22";
        public string PlcTagStringAddress = "8A0E000F.23";
        public string PlcTagWStringAddress = "8A0E000F.24";
        // Pointer types are located in a different instance DB
        public string PlcTagPointerAddress = "8A0E0001.A";
        public string PlcTagAnyAddress = "8A0E0001.9";

        private Random m_Rand;

        /// <summary>
        /// Start the testrun for the PlcTag types
        /// </summary>
        /// <param name="conn">Connection to use for the test</param>
        /// <param name="nrandom">Number of random values to generate for each Tag type</param>
        /// <param name="testPointers">Option to do the testruns on Pointer types (Any, Pointer)</param>
        /// <returns></returns>
        public int DoTests(S7CommPlusConnection conn, int nrandom, bool testPointers)
        {
            int result = 0;
            m_Rand = new Random();

            // Boolean
            result += Test_PlcTag_Bool(conn);
            // Bitstrings
            result += Test_PlcTag_Byte(conn, nrandom);
            result += Test_PlcTag_Word(conn, nrandom);
            result += Test_PlcTag_DWord(conn, nrandom);
            result += Test_PlcTag_LWord(conn, nrandom);
            // Integers
            result += Test_PlcTag_USInt(conn, nrandom);
            result += Test_PlcTag_SInt(conn, nrandom);
            result += Test_PlcTag_UInt(conn, nrandom);
            result += Test_PlcTag_Int(conn, nrandom);
            result += Test_PlcTag_UDInt(conn, nrandom);
            result += Test_PlcTag_DInt(conn, nrandom);
            result += Test_PlcTag_ULInt(conn, nrandom);
            result += Test_PlcTag_LInt(conn, nrandom);
            // Floating point
            result += Test_PlcTag_Real(conn, nrandom);
            result += Test_PlcTag_LReal(conn, nrandom);
            // Time values
            result += Test_PlcTag_Time(conn, nrandom);
            result += Test_PlcTag_LTime(conn, nrandom);
            result += Test_PlcTag_S5Time(conn, nrandom);
            // Date and Time values
            result += Test_PlcTag_Date(conn, nrandom);
            result += Test_PlcTag_DateAndTime(conn, nrandom);
            result += Test_PlcTag_LDT(conn, nrandom);
            result += Test_PlcTag_TimeOfDay(conn, nrandom);
            result += Test_PlcTag_LTOD(conn, nrandom);
            result += Test_PlcTag_DTL(conn, nrandom);
            // Strings
            result += Test_PlcTag_Char(conn, nrandom);
            result += Test_PlcTag_WChar(conn, nrandom);
            result += Test_PlcTag_String(conn, nrandom);
            result += Test_PlcTag_WString(conn, nrandom);
            // Pointers
            if (testPointers) // only optional, because depending on the testprogram, you can't write them
            {
                result += Test_PlcTag_Pointer(conn, nrandom);
                result += Test_PlcTag_Any(conn, nrandom);
            }

            Console.WriteLine();
            Console.WriteLine("=======================================");
            Console.WriteLine("DoTests: Number of errors = " + result);
            Console.WriteLine("=======================================");
            return result;
        }

        #region Tests for Boolean tags
        private int Test_PlcTag_Bool(S7CommPlusConnection conn)
        {
            int result = 0;
            Console.WriteLine("*** TestPlcTag_Bool ***");
            bool writevalue;
            var tags = new List<PlcTag>();
            var tag = new PlcTagBool("Bool_Var", new ItemAddress(PlcTagBoolAddress), Softdatatype.S7COMMP_SOFTDATATYPE_BBOOL);
            tags.Add(tag);
            for (int i = 0; i < 5; i++)
            {
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                writevalue = !(tag.Value);
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }
        #endregion

        #region Tests for Bitstring tags
        private int Test_PlcTag_Byte(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_Byte ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagByte("Byte_Var", new ItemAddress(PlcTagByteAddress), Softdatatype.S7COMMP_SOFTDATATYPE_BYTE);
            var testvalues = new List<byte>() { 0x00, 0x01, 0x55, 0xaa, 0xff };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add((byte)m_Rand.Next(0, 255));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = 0x{1:X2}", tag.Name, tag.Value));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = 0x{1:X2}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = 0x{1:X2}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_Word(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_Word ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagWord("Word_Var", new ItemAddress(PlcTagWordAddress), Softdatatype.S7COMMP_SOFTDATATYPE_WORD);
            var testvalues = new List<ushort>() { 0x0000, 0x0001, 0xa5a5, 0x5a5a, 0xffff };

            var rand = new Random();
            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add((ushort)rand.Next(ushort.MinValue, ushort.MaxValue));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = 0x{1:X4}", tag.Name, tag.Value));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = 0x{1:X4}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = 0x{1:X4}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_DWord(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_DWord ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagDWord("DWord_Var", new ItemAddress(PlcTagDWordAddress), Softdatatype.S7COMMP_SOFTDATATYPE_DWORD);
            var testvalues = new List<uint>() { 0x00000000, 0x00000001, 0xa5a5a5a5, 0x5a5a5a5a, 0xffffffff };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add((uint)m_Rand.NextLong(uint.MinValue, uint.MaxValue));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = 0x{1:X8}", tag.Name, tag.Value));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = 0x{1:X8}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = 0x{1:X8}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_LWord(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_LWord ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagLWord("LWord_Var", new ItemAddress(PlcTagLWordAddress), Softdatatype.S7COMMP_SOFTDATATYPE_LWORD);
            var testvalues = new List<ulong>() { 0x0000000000000000, 0x0000000000000001, 0xa5a5a5a5a5a5a5a5, 0x5a5a5a5a5a5a5a5a, 0xffffffffffffffff };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add((ulong)m_Rand.NextLong(0, long.MaxValue) + (ulong)m_Rand.NextLong(0, long.MaxValue));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = 0x{1:X8}", tag.Name, tag.Value));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = 0x{1:X8}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = 0x{1:X8}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }
        #endregion

        #region Tests for Integer tags
        private int Test_PlcTag_USInt(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_USInt ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagUSInt("USInt_Var", new ItemAddress(PlcTagUSIntAddress), Softdatatype.S7COMMP_SOFTDATATYPE_USINT);
            var testvalues = new List<byte>() { byte.MinValue, 1, 123, 210, byte.MaxValue };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add((byte)m_Rand.Next(0, 255));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_SInt(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_SInt ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagSInt("SInt_Var", new ItemAddress(PlcTagSIntAddress), Softdatatype.S7COMMP_SOFTDATATYPE_SINT);
            var testvalues = new List<sbyte>() { -128, 0, 1, 123, 127 };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add((sbyte)m_Rand.Next(sbyte.MinValue, sbyte.MaxValue));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_UInt(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_UInt ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagUInt("UInt_Var", new ItemAddress(PlcTagUIntAddress), Softdatatype.S7COMMP_SOFTDATATYPE_UINT);
            var testvalues = new List<ushort>() { ushort.MinValue, 1, 12345, 4321, ushort.MaxValue };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add((ushort)m_Rand.Next(ushort.MinValue, ushort.MaxValue));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_Int(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_Int ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagInt("Int_Var", new ItemAddress(PlcTagIntAddress), Softdatatype.S7COMMP_SOFTDATATYPE_INT);
            var testvalues = new List<short>() { short.MinValue, -12345, 0, 12345, short.MaxValue };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add((short)m_Rand.Next(short.MinValue, short.MaxValue));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_UDInt(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_UDInt ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagUDInt("UDInt_Var", new ItemAddress(PlcTagUDIntAddress), Softdatatype.S7COMMP_SOFTDATATYPE_UDINT);
            var testvalues = new List<uint>() { uint.MinValue, 1, 127, 128, 255, 256, 1234567, uint.MaxValue };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add((uint)m_Rand.NextLong(uint.MinValue, uint.MaxValue));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_DInt(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_DInt ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagDInt("DInt_Var", new ItemAddress(PlcTagDIntAddress), Softdatatype.S7COMMP_SOFTDATATYPE_DINT);
            var testvalues = new List<int>() { int.MinValue, -256, -255, -128, -127, 0, 127, 128, 255, 256, int.MaxValue };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add((int)m_Rand.Next(int.MinValue, int.MaxValue));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_ULInt(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_ULInt ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagULInt("ULInt_Var", new ItemAddress(PlcTagULIntAddress), Softdatatype.S7COMMP_SOFTDATATYPE_ULINT);
            var testvalues = new List<ulong>() { ulong.MinValue, 127, 128, 255, 256, 0x00FFFFFFFFFFFFFF, 0x00FFFFFFFFFFFFFF + 1, 0x00FFFFFFFFFFFFFF + 2, ulong.MaxValue };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add((ulong)m_Rand.NextLong(0, long.MaxValue) + (ulong)m_Rand.NextLong(0, long.MaxValue));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_LInt(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_LInt ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagLInt("LInt_Var", new ItemAddress(PlcTagLIntAddress), Softdatatype.S7COMMP_SOFTDATATYPE_LINT);
            var testvalues = new List<long>() { long.MinValue, -256, -255, -128, -127, 0, 127, 128, 255, 256, 0x007FFFFFFFFFFFFF, 0x007FFFFFFFFFFFFF + 1, long.MaxValue };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add(m_Rand.NextLong(long.MinValue, long.MaxValue));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }
        #endregion

        #region Tests for Floating point tags
        private int Test_PlcTag_Real(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_Real ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagReal("Real_Var", new ItemAddress(PlcTagRealAddress), Softdatatype.S7COMMP_SOFTDATATYPE_REAL);
            // float.NaN fails by design
            var testvalues = new List<float>() { float.MinValue, float.NegativeInfinity, float.PositiveInfinity, -1234.5f, 0.0f, 0.125f, float.MaxValue };

            for (int i = 0; i < nrandom; i++)
            {
                double r = m_Rand.NextDouble();
                double r1 = (r * ((double)float.MaxValue - (double)float.MinValue)) + float.MinValue;
                testvalues.Add((float)r1);
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_LReal(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_LReal ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagLReal("LReal_Var", new ItemAddress(PlcTagLRealAddress), Softdatatype.S7COMMP_SOFTDATATYPE_LREAL);
            // double.NaN fails by design
            var testvalues = new List<double>() { double.MinValue, double.NegativeInfinity, double.PositiveInfinity, -1234.5, 0.0, 0.125, double.MaxValue };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add(m_Rand.NextDouble() * 2000.0 - 1000.0); // NextDouble gives values between 0..1, scale up that we get at least some negative values
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }
        #endregion

        #region Tests for Time value tags
        private int Test_PlcTag_Time(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_Time ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagTime("Time_Var", new ItemAddress(PlcTagTimeAddress), Softdatatype.S7COMMP_SOFTDATATYPE_TIME);
            var testvalues = new List<int>() { int.MinValue, -256, -255, -128, -127, 0, 127, 128, 255, 256, int.MaxValue };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add((int)m_Rand.Next(int.MinValue, int.MaxValue));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1,-11} - as String {2}", tag.Name, tag.Value, tag.ToString()));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1,-11}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1,-11} - as String {2}", tag.Name, tag.Value, tag.ToString()));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_LTime(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_LTime ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagLTime("LTime_Var", new ItemAddress(PlcTagLTimeAddress), Softdatatype.S7COMMP_SOFTDATATYPE_LTIME);
            var testvalues = new List<long>() { long.MinValue, -256, -255, -128, -127, 0, 127, 128, 255, 256, 0x007FFFFFFFFFFFFF, 0x007FFFFFFFFFFFFF + 1, long.MaxValue };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add(m_Rand.NextLong(long.MinValue, long.MaxValue));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1,-22} - as String {2}", tag.Name, tag.Value, tag.ToString()));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1,-22}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1,-22} - as String {2}", tag.Name, tag.Value, tag.ToString()));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_S5Time(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_S5Time ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagS5Time("S5Time_Var", new ItemAddress(PlcTagS5TimeAddress), Softdatatype.S7COMMP_SOFTDATATYPE_S5TIME);
            // TimeValue (0..999), TimeBase (0=10ms, 1=100ms, 2=1s, 3=10s)
            var testvalues = new List<(ushort TimeValue, ushort TimeBase)>() { (0, 0), (1, 1), (999, 0), (10, 1), (123, 2), (999, 3) };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add(((ushort)m_Rand.Next(0, 999), (ushort)m_Rand.Next(0, 3)));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  (TimeValue, TimeBase) = ({1,-3}, {2,-3}) -> {3,-5}", tag.Name, tag.TimeValue, tag.TimeBase, tag.ToString()));
            foreach (var writevalue in testvalues)
            {
                tag.TimeValue = writevalue.TimeValue;
                tag.TimeBase = writevalue.TimeBase;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value (TimeValue, TimeBase) = ({1,-3}, {2,-3})", tag.Name, writevalue.TimeValue, writevalue.TimeBase));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  (TimeValue, TimeBase) = ({1,-3}, {2,-3}) -> {3,-5}", tag.Name, tag.TimeValue, tag.TimeBase, tag.ToString()));
                if (tag.TimeValue == writevalue.TimeValue && tag.TimeBase == writevalue.TimeBase)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }
        #endregion

        #region Tests for Date and Time value tags
        private int Test_PlcTag_Date(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_Date ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagDate("Date_Var", new ItemAddress(PlcTagDateAddress), Softdatatype.S7COMMP_SOFTDATATYPE_DATE);
            var testvalues = new List<DateTime>() { new DateTime(1990, 1, 1), new DateTime(2023, 12, 23), new DateTime(2169, 6, 6) };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add(new DateTime(m_Rand.Next(1990, 2169), m_Rand.Next(1, 12), m_Rand.Next(1, 27)));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value.ToShortDateString()));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue.ToShortDateString()));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value.ToShortDateString()));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_DateAndTime(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_DateAndTime ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagDateAndTime("DateAndTime_Var", new ItemAddress(PlcTagDateAndTimeAddress), Softdatatype.S7COMMP_SOFTDATATYPE_DATEANDTIME);
            var testvalues = new List<DateTime>() { new DateTime(1990, 1, 1), new DateTime(2023, 12, 23, 1, 2, 3, 123), new DateTime(2089, 12, 31, 23, 59, 59, 999) };
            for (int i = 0; i < nrandom; i++)
            {
                var dt = new DateTime(m_Rand.Next(1990, 2089), m_Rand.Next(1, 12), m_Rand.Next(1, 27));
                dt = dt.AddSeconds((double)m_Rand.Next(0, 864000));
                dt = dt.AddMilliseconds((double)m_Rand.Next(0, 999));
                testvalues.Add(dt);
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}.{2:D03}", tag.Name, tag.Value, tag.Value.Millisecond));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}.{2:D03}", tag.Name, writevalue, writevalue.Millisecond));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}.{2:D03}", tag.Name, tag.Value, tag.Value.Millisecond));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_LDT(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_LDT ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagLDT("LDT_Var", new ItemAddress(PlcTagLDTAddress), Softdatatype.S7COMMP_SOFTDATATYPE_LDT);
            var testvalues = new List<ulong>() { ulong.MinValue, 127, 128, 255, 256, 0x00FFFFFFFFFFFFFF, 0x00FFFFFFFFFFFFFF + 1, 0x00FFFFFFFFFFFFFF + 2, ulong.MaxValue };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add((ulong)m_Rand.NextLong(0, long.MaxValue) + (ulong)m_Rand.NextLong(0, long.MaxValue));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1,-20} - as String {2}", tag.Name, tag.Value, tag.ToString()));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1,-20}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1,-20} - as String {2}", tag.Name, tag.Value, tag.ToString()));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_TimeOfDay(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_TimeOfDay ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagTimeOfDay("TimeOfDay_Var", new ItemAddress(PlcTagTimeOfDayAddress), Softdatatype.S7COMMP_SOFTDATATYPE_TIMEOFDAY);
            var testvalues = new List<uint>() { 0, 1, 123456, 86400000 - 1 };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add((uint)m_Rand.NextLong(0, 86400000 - 1));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1,-20} - as String {2}", tag.Name, tag.Value, tag.ToString()));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1,-20}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1,-20} - as String {2}", tag.Name, tag.Value, tag.ToString()));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_LTOD(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_LTOD ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagLTOD("LTOD_Var", new ItemAddress(PlcTagLTODAddress), Softdatatype.S7COMMP_SOFTDATATYPE_LTOD);
            var testvalues = new List<ulong>() { 0, 1234, 1234567890, 86400000000000 - 1 };

            for (int i = 0; i < nrandom; i++)
            {
                testvalues.Add((ulong)m_Rand.NextLong(0, 86400000000000 - 1));
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1,-20} - as String {2}", tag.Name, tag.Value, tag.ToString()));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1,-20}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1,-20} - as String {2}", tag.Name, tag.Value, tag.ToString()));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_DTL(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_DTL ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagDTL("DTL_Var", new ItemAddress(PlcTagDTLAddress), Softdatatype.S7COMMP_SOFTDATATYPE_DTL);
            // We can't use the milliseconds in DateTime here, because all is done with an additional Nanosecond field.
            var testvalues = new List<(DateTime dt, uint ns)>();
            testvalues.Add((new DateTime(1970, 1, 1), 0));
            testvalues.Add((new DateTime(2023, 12, 23, 1, 2, 3, 0), 123456789));
            testvalues.Add((new DateTime(2089, 12, 31, 23, 59, 59, 0), 999999999));
            testvalues.Add((new DateTime(2262, 4, 11, 23, 47, 16, 0), 854775807));

            uint ns;
            for (int i = 0; i < nrandom; i++)
            {
                var dt = new DateTime(m_Rand.Next(1970, 2261), m_Rand.Next(1, 12), m_Rand.Next(1, 27));
                dt = dt.AddSeconds((double)m_Rand.Next(0, 864000));
                ns = (uint)m_Rand.Next(0, 999999999);
                testvalues.Add((dt, ns));
            }
            // interface timestamp, important!
            Console.WriteLine("DTLInterfaceTimestamp=" + tag.DTLInterfaceTimestamp);

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}.{2:D09}", tag.Name, tag.Value, tag.ValueNanosecond));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue.dt;
                tag.ValueNanosecond = writevalue.ns;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}.{2:D09}", tag.Name, writevalue.dt, writevalue.ns));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}.{2:D09}", tag.Name, tag.Value, tag.ValueNanosecond));
                if (tag.Value == writevalue.dt && tag.ValueNanosecond == writevalue.ns)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }
        #endregion

        #region Tests for Character / String tags
        private int Test_PlcTag_Char(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_Char ***");
            var tags = new List<PlcTag>();
            var tag = new PlcTagChar("Char_Var", new ItemAddress(PlcTagCharAddress), Softdatatype.S7COMMP_SOFTDATATYPE_CHAR);
            var testvalues = new List<char>() { 'A', 'B', ' ', 'ä', '1' };

            int cval;
            for (int i = 0; i < nrandom; i++)
            {
                cval = m_Rand.Next(32, 126);
                testvalues.Add((char)cval);
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));

            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_WChar(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_WChar ***");
            var tags = new List<PlcTag>();
            var tag = new PlcTagWChar("WChar_Var", new ItemAddress(PlcTagWCharAddress), Softdatatype.S7COMMP_SOFTDATATYPE_WCHAR);
            var testvalues = new List<char>() { 'A', 'B', ' ', 'ä', '1', 'Ʃ' };

            int cval;
            for (int i = 0; i < nrandom; i++)
            {
                cval = m_Rand.Next(32, 126); // TODO: Write some Unicode values?
                testvalues.Add((char)cval);
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));

            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_String(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_String ***");
            byte string_maxlength = 254;
            var tags = new List<PlcTag>();
            var tag = new PlcTagString("String_Var", new ItemAddress(PlcTagStringAddress), Softdatatype.S7COMMP_SOFTDATATYPE_STRING, string_maxlength);
            var testvalues = new List<string>() { "Hello", "World", "This is a test!", "Motörhead", "abcdefghijklmnopqrstuvwxyz1234567890" };

            // Generate a string of maxlength
            string smax = String.Empty;
            int cval = 32; // Space
            for (int i = 0; i < string_maxlength; i++)
            {
                smax += (char)cval;
                cval++;
                if (cval > 126)
                    cval = 32;
            }
            testvalues.Add(smax);

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));

            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_WString(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_WString ***");
            byte string_maxlength = 254;
            var tags = new List<PlcTag>();
            var tag = new PlcTagWString("WString_Var", new ItemAddress(PlcTagWStringAddress), Softdatatype.S7COMMP_SOFTDATATYPE_WSTRING, string_maxlength);
            var testvalues = new List<string>() { "Hello", "World", "This is a test!", "Motörhead", "Test Greek ΣΛΔ end." };

            // Generate a string of maxlength
            string smax = String.Empty;
            int cval = 32; // Space
            for (int i = 0; i < string_maxlength; i++)
            {
                smax += (char)cval;
                cval++;
                if (cval > 126)
                    cval = 32;
            }
            testvalues.Add(smax);

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));

            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, writevalue));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1}", tag.Name, tag.Value));
                if (tag.Value == writevalue)
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }
        #endregion

        #region Tests for Pointers type tags
        private int Test_PlcTag_Pointer(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_Pointer ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagPointer("Pointer_Var", new ItemAddress(PlcTagPointerAddress), Softdatatype.S7COMMP_SOFTDATATYPE_POINTER);
            var testvalues = new List<byte[]>();
            testvalues.Add(new byte[] { 0x04, 0xd2, 0x84, 0x06, 0xee, 0xaa }); // P#DB1234.DBX56789.2
            testvalues.Add(new byte[] { 0xff, 0xff, 0x84, 0x01, 0x81, 0xcc }); // P#DB65535.DBX12345.4

            int dbnr;
            int bytenr;
            int bitnr;
            for (int i = 0; i < nrandom; i++)
            {
                var b = new byte[6];
                dbnr = (int)m_Rand.Next(1, 65535);
                b[0] = (byte)(dbnr >> 8);
                b[1] = (byte)dbnr;
                b[2] = 0x84; // only in DBX area full range of dbnr and bytenr is allowed
                bytenr = (int)m_Rand.Next(0, 65535);
                bitnr = (byte)m_Rand.Next(0, 7);
                b[3] = (byte)(bytenr >> 13);
                b[4] = (byte)(bytenr >> 5);
                b[5] = (byte)(bitnr | (bytenr << 3));
                testvalues.Add(b);
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1} {2}", tag.Name, GetHexstring(tag.Value), tag.ToString()));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, GetHexstring(writevalue)));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1} {2}", tag.Name, GetHexstring(tag.Value), tag.ToString()));
                if (tag.Value.SequenceEqual(writevalue))
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }

        private int Test_PlcTag_Any(S7CommPlusConnection conn, int nrandom)
        {
            int result = 0;
            Console.WriteLine("*** Test_PlcTag_Any ***");

            var tags = new List<PlcTag>();
            var tag = new PlcTagAny("Any_Var", new ItemAddress(PlcTagAnyAddress), Softdatatype.S7COMMP_SOFTDATATYPE_ANY);
            var testvalues = new List<byte[]>();
            testvalues.Add(new byte[] { 0x10, 0x02, 0x00, 0x7b, 0x04, 0xd2, 0x84, 0x06, 0xee, 0xaa }); // P#DB1234.DBX56789.2 BYTE 123
            testvalues.Add(new byte[] { 0x10, 0x02, 0x02, 0xa6, 0xff, 0xff, 0x84, 0x01, 0x81, 0xcc }); // P#DB65535.DBX12345.4 BYTE 678

            int dbnr;
            int bytenr;
            int bitnr;
            int factor;
            for (int i = 0; i < nrandom; i++)
            {
                var b = new byte[10];
                b[0] = 0x10; // header
                b[1] = 0x02; // datatype 2=byte
                factor = (int)m_Rand.Next(1, 65535);
                b[2] = (byte)(factor >> 8);
                b[3] = (byte)factor;
                dbnr = (int)m_Rand.Next(1, 65535);
                b[4] = (byte)(dbnr >> 8);
                b[5] = (byte)dbnr;
                b[6] = 0x84; // only in DBX area full range of dbnr and bytenr is allowed
                bytenr = (int)m_Rand.Next(0, 65535);
                bitnr = (byte)m_Rand.Next(0, 7);
                b[7] = (byte)(bytenr >> 13);
                b[8] = (byte)(bytenr >> 5);
                b[9] = (byte)(bitnr | (bytenr << 3));
                testvalues.Add(b);
            }

            tags.Add(tag);
            conn.ReadTags(tags);
            Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1} {2}", tag.Name, GetHexstring(tag.Value), tag.ToString()));
            foreach (var writevalue in testvalues)
            {
                tag.Value = writevalue;
                Console.WriteLine(String.Format("Name= {0,-20}: Write value = {1}", tag.Name, GetHexstring(writevalue)));
                conn.WriteTags(tags);
                conn.ReadTags(tags);
                Console.WriteLine(String.Format("Name= {0,-20}: Read value  = {1} {2}", tag.Name, GetHexstring(tag.Value), tag.ToString()));
                if (tag.Value.SequenceEqual(writevalue))
                {
                    Console.WriteLine(String.Format("Name= {0,-20}: Success.", tag.Name));
                }
                else
                {
                    Console.Beep();
                    Console.WriteLine(String.Format("Name= {0,-20}: FAILED!!!", tag.Name));
                    result++;
                }
            }
            return result;
        }
        #endregion type tags

        private string GetHexstring(byte[] b)
        {
            string s = "";
            for (int i = 0; i < b.Length; i++)
            {
                s += String.Format("{0:X2} ", b[i]);
            }
            return s;
        }
    }

    // A non-cryptographic usable extension to get some long random numbers (64 bit random only available in .Net Core).
    // For our case it's good enough (for testing VLQ values a logarithmic random would be useful).
    public static class RandomExt
    {
        public static long NextLong(this Random random, long min, long max)
        {
            ulong range = (ulong)(max - min);
            ulong urnd;
            do
            {
                byte[] buf = new byte[8];
                random.NextBytes(buf);
                urnd = (ulong)BitConverter.ToInt64(buf, 0);
            } while (urnd > ulong.MaxValue - ((ulong.MaxValue % range) + 1) % range);

            return (long)(urnd % range) + min;
        }
    }
}
