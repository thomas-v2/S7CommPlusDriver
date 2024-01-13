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

namespace S7CommPlusDriver
{
    public static class Ids
    {
        public const int None = 0;
        public const int NativeObjects_thePLCProgram_Rid = 3;
        public const int NativeObjects_theCPUexecUnit_Rid = 52;
        public const int NativeObjects_theIArea_Rid = 80;
        public const int NativeObjects_theQArea_Rid = 81;
        public const int NativeObjects_theMArea_Rid = 82;
        public const int NativeObjects_theS7Counters_Rid = 83;
        public const int NativeObjects_theS7Timers_Rid = 84;
        public const int ObjectRoot = 201;
        public const int GetNewRIDOnServer = 211;
        public const int ObjectVariableTypeParentObject = 229;
        public const int ObjectVariableTypeName = 233;
        public const int ClassSubscriptions = 255;
        public const int ClassServerSessionContainer = 284;
        public const int ObjectServerSessionContainer = 285;
        public const int ClassServerSession = 287;
        public const int ObjectNullServerSession = 288;
        public const int ServerSessionClientRID = 300;
        public const int ServerSessionVersion = 306;
        public const int ClassTypeInfo = 511;
        public const int ClassOMSTypeInfoContainer = 534;
        public const int ObjectOMSTypeInfoContainer = 537;
        public const int TextLibraryClassRID = 606;
        public const int TextLibraryOffsetArea = 608;
        public const int TextLibraryStringArea = 609;
        public const int ClassSubscription = 1001;
        public const int SubscriptionMissedSendings = 1002;
        public const int SubscriptionSubsystemError = 1003;
        public const int SubscriptionReferenceTriggerAndTransmitMode = 1005;
        public const int SystemLimits = 1037;
        public const int SubscriptionRouteMode = 1040;
        public const int SubscriptionActive = 1041;
        public const int SubscriptionReferenceList = 1048;
        public const int SubscriptionCycleTime = 1049;
        public const int SubscriptionDelayTime = 1050;
        public const int SubscriptionDisabled = 1051;
        public const int SubscriptionCount = 1052;
        public const int SubscriptionCreditLimit = 1053;
        public const int SubscriptionTicks = 1054;
        public const int SubscriptionFunctionClassId = 1082;
        public const int Filter = 1246;
        public const int FilterOperation = 1247;
        public const int AddressCount = 1249;
        public const int Address = 1250;
        public const int FilterValue = 1251;
        public const int ObjectQualifier = 1256;
        public const int ParentRID = 1257;
        public const int CompositionAID = 1258;
        public const int KeyQualifier = 1259;
        public const int CPUexecUnit_operatingStateReq = 2167;
        public const int PLCProgram_Class_Rid = 2520;
        public const int Block_BlockNumber = 2521;
        public const int DB_ValueInitial = 2548;
        public const int DB_ValueActual = 2550;
        public const int DB_InitialChanged = 2551;
        public const int DB_Class_Rid = 2574;
        public const int AlarmSubscriptionRef_AlarmDomain = 2659;
        public const int AlarmSubscriptionRef_itsAlarmSubsystem = 2660;
        public const int AlarmSubscriptionRef_Class_Rid = 2662;
        public const int DAI_CPUAlarmID = 2670;
        public const int DAI_AllStatesInfo = 2671;
        public const int DAI_AlarmDomain = 2672;
        public const int DAI_Coming = 2673;
        public const int DAI_Going = 2677;
        public const int DAI_Class_Rid = 2681;
        public const int DAI_AlarmTexts_Rid = 2715;
        public const int AS_CGS_AllStatesInfo = 3474;
        public const int AS_CGS_Timestamp = 3475;
        public const int AS_CGS_AssociatedValues = 3476;
        public const int AS_CGS_AckTimestamp = 3646;
        public const int ControllerArea_ValueInitial = 3735;
        public const int ControllerArea_ValueActual = 3736;
        public const int ControllerArea_RuntimeModified = 3737;
        public const int DAI_MessageType = 4079;
        public const int ASObjectES_Comment = 4288;
        public const int AlarmSubscriptionRef_AlarmDomain2 = 7731;
        public const int DAI_HmiInfo = 7813;
        public const int MultipleSTAI_Class_Rid = 7854;
        public const int MultipleSTAI_STAIs = 7859;
        public const int DAI_SequenceCounter = 7917;
        public const int AlarmSubscriptionRef_AlarmTextLanguages_Rid = 8181;
        public const int AlarmSubscriptionRef_SendAlarmTexts_Rid = 8173;
        public const int ReturnValue = 40305;
    }
}
