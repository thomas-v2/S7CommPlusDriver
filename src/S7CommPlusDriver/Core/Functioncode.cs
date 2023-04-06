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

namespace S7CommPlusDriver
{
    public static class Functioncode
    {
        public const int Error = 0x04b1;
        public const int Explore = 0x04bb;
        public const int CreateObject = 0x04ca;
        public const int DeleteObject = 0x04d4;
        public const int SetVariable = 0x04f2;
        public const int GetVariable = 0x04fc; /* only in old 1200 FW? */
        public const int AddLink = 0x0506;        
        public const int RemoveLink = 0x051a;
        public const int GetLink = 0x0524;
        public const int SetMultiVariables = 0x0542;
        public const int GetMultiVariables = 0x054c;
        public const int BeginSequence = 0x0556;
        public const int EndSequence = 0x0560;
        public const int Invoke = 0x056b;
        public const int SetVarSubStreamed = 0x057c;
        public const int GetVarSubStreamed = 0x0586;
        public const int GetVariablesAddress = 0x0590;
        public const int Abort = 0x059a;
        public const int Error2 = 0x05a9;
        public const int InitSsl = 0x05b3;
    }
}
