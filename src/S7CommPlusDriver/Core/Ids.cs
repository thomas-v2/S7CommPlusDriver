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
    public static class Ids
    {
        public const int None = 0;
        public const int NativeObjects_thePLCProgram_Rid = 3;
        public const int NativeObjects_theIArea_Rid = 80;
        public const int NativeObjects_theQArea_Rid = 81;
        public const int NativeObjects_theMArea_Rid = 82;
        public const int NativeObjects_theS7Counters_Rid = 83;
        public const int NativeObjects_theS7Timers_Rid = 84;
        public const int ObjectRoot = 201;
        public const int GetNewRIDOnServer = 211;
        public const int ObjectVariableTypeName = 233;
        public const int ClassSubscriptions = 255;
        public const int ClassServerSessionContainer = 284;
        public const int ObjectServerSessionContainer = 285;
        public const int ClassServerSession = 287;
        public const int ObjectNullServerSession = 288;
        public const int ServerSessionClientRID = 300;
        public const int ServerSessionVersion = 306;
        public const int ClassTypeInfo = 511;
        public const int ObjectOMSTypeInfoContainer = 537;
        public const int SystemLimits = 1037;
        public const int ObjectQualifier = 1256;
        public const int ParentRID = 1257;
        public const int CompositionAID = 1258;
        public const int KeyQualifier = 1259;
        public const int Block_BlockNumber = 2521;
        public const int DB_ValueInitial = 2548;
        public const int DB_ValueActual = 2550;
        public const int DB_InitialChanged = 2551;
        public const int DB_Class_Rid = 2574;
        public const int ControllerArea_ValueInitial = 3735;
        public const int ControllerArea_ValueActual = 3736;
        public const int ControllerArea_RuntimeModified = 3737;
        public const int ASObjectES_Comment = 4288;
    }
}
