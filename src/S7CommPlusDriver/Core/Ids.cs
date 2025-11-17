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
        public const int NativeObjects_theASRoot_Rid = 1;
        public const int NativeObjects_thePLCProgram_Rid = 3;
        public const int NativeObjects_theCPUProxy_Rid = 49;
        public const int NativeObjects_theCPUCommon_Rid = 50;
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
        public const int ServerSessionRequest = 303;
        public const int ServerSessionResponse = 304;
        public const int ServerSessionVersion = 306;
        public const int LID_SessionVersionSystemPAOMString = 319;
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
        public const int Legitimate = 1846;
        public const int SubscriptionReferenceList = 1048;
        public const int SubscriptionCycleTime = 1049;
        public const int SubscriptionDelayTime = 1050;
        public const int SubscriptionDisabled = 1051;
        public const int SubscriptionCount = 1052;
        public const int SubscriptionCreditLimit = 1053;
        public const int SubscriptionTicks = 1054;
        public const int FreeItems = 1081;
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
        public const int TI_TComSize = 1502;
        public const int EffectiveProtectionLevel = 1842;
        public const int ActiveProtectionLevel = 1843;
        public const int CPUexecUnit_operatingStateReq = 2167;
        public const int ASRoot_ItsFolders = 2468;
        public const int PLCProgram_Class_Rid = 2520;
        public const int Block_BlockNumber = 2521;
        public const int Block_AutoNumbering = 2522;
        public const int Block_BlockLanguage = 2523;
        public const int Block_KnowhowProtected = 2524;
        public const int Block_Unlinked = 2527;
        public const int Block_LowLatency = 2528;
        public const int Block_RuntimeModified = 2529;
        public const int Block_Dirty = 2531;
        public const int Block_CRC = 2532;
        public const int Block_Binding = 3151;
        public const int Block_OptimizeInfo = 2537;
        public const int Block_TOblockSetNumber = 3619;
        public const int Block_TypeInfo = 4578;
        public const int Block_FunctionalSignature = 7589;
        public const int Block_AdditionalMAC = 7831;
        public const int Block_FailsafeBlockInfo = 7843;
        public const int Block_FailsafeIFRHash = 7955;
        public const int ASObjectES_IdentES = 2449;
        public const int Block_BodyDescription = 2533;
        public const int DataInterface_InterfaceDescription = 2544;
        public const int DataInterface_LineComments = 2546;
        public const int DB_ValueInitial = 2548;
        public const int DB_ValueActual = 2550;
        public const int DB_InitialChanged = 2551;
        public const int DB_Class_Rid = 2574;
        public const int FB_Class_Rid = 2578;
        public const int FC_Class_Rid = 2579;
        public const int FunctionalObject_Code = 2580;
        public const int FunctionalObject_ParameterModified = 2581;
        public const int FunctionalObject_extRefData = 2582;
        public const int FunctionalObject_intRefData = 2583;
        public const int FunctionalObject_NetworkComments = 2584;
        public const int FunctionalObject_NetworkTitles = 2585;
        public const int FunctionalObject_CalleeList = 2586;
        public const int FunctionalObject_InterfaceSignature = 2587;
        public const int FunctionalObject_DisplayInfo = 2588;
        public const int FunctionalObject_DebugInfo = 2589;
        public const int FunctionalObject_LocalErrorhandling = 2590;
        public const int FunctionalObject_LongConstants = 2591;
        public const int FunctionalObject_Class_Rid = 2592;
        public const int OB_StartInfoType = 2607;
        public const int OB_Class_Rid = 2610;
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
        public const int UDT_Class_Rid = 3932;
        public const int DAI_MessageType = 4079;
        public const int ConstantsGlobal_Symbolics = 4275;
        public const int ASObjectES_Comment = 4288;
        public const int AlarmSubscriptionRef_AlarmDomain2 = 7731;
        public const int DAI_HmiInfo = 7813;
        public const int MultipleSTAI_Class_Rid = 7854;
        public const int MultipleSTAI_STAIs = 7859;
        public const int DAI_SequenceCounter = 7917;
        public const int AlarmSubscriptionRef_AlarmTextLanguages_Rid = 8181;
        public const int AlarmSubscriptionRef_SendAlarmTexts_Rid = 8173;
        public const int ReturnValue = 40305;
        public const int LID_LegitimationPayloadStruct = 40400;
        public const int LID_LegitimationPayloadType = 40401;
        public const int LID_LegitimationPayloadUsername = 40402;
        public const int LID_LegitimationPayloadPassword = 40403;

        public const uint ReleaseMngmtRoot_Rid = 2303328256;

        public const uint Constants = 0x8a110000;

        public const int TI_BOOL = 0x02000000 + 1;
        public const int TI_BYTE = 0x02000000 + 2;
        public const int TI_CHAR = 0x02000000 + 3;
        public const int TI_WORD = 0x02000000 + 4;
        public const int TI_INT = 0x02000000 + 5;
        public const int TI_DWORD = 0x02000000 + 6;
        public const int TI_DINT = 0x02000000 + 7;
        public const int TI_REAL = 0x02000000 + 8;
        public const int TI_STRING = 0x02000000 + 19;
        public const int TI_LREAL = 0x02000000 + 48;
        public const int TI_USINT = 0x02000000 + 52;
        public const int TI_UINT = 0x02000000 + 53;
        public const int TI_UDINT = 0x02000000 + 54;
        public const int TI_SINT = 0x02000000 + 55;
        public const int TI_WCHAR = 0x02000000 + 61;
        public const int TI_WSTRING = 0x02000000 + 62;
        public const int TI_STRING_START = 0x020a0000;  // Start for String[0]
        public const int TI_STRING_END = 0x020affff;    // End (String[65535])
        public const int TI_WSTRING_START = 0x020b0000; // Start for WString[0]
        public const int TI_WSTRING_END = 0x020bffff;   // End (WString[65535])
    }
}
