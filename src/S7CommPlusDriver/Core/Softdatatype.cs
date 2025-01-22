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

using System.Collections.Generic;

namespace S7CommPlusDriver
{
    public static class Softdatatype
    {
        public const uint S7COMMP_SOFTDATATYPE_VOID = 0;
        public const uint S7COMMP_SOFTDATATYPE_BOOL = 1;
        public const uint S7COMMP_SOFTDATATYPE_BYTE = 2;
        public const uint S7COMMP_SOFTDATATYPE_CHAR = 3;
        public const uint S7COMMP_SOFTDATATYPE_WORD = 4;
        public const uint S7COMMP_SOFTDATATYPE_INT = 5;
        public const uint S7COMMP_SOFTDATATYPE_DWORD = 6;
        public const uint S7COMMP_SOFTDATATYPE_DINT = 7;
        public const uint S7COMMP_SOFTDATATYPE_REAL = 8;
        public const uint S7COMMP_SOFTDATATYPE_DATE = 9;
        public const uint S7COMMP_SOFTDATATYPE_TIMEOFDAY = 10;
        public const uint S7COMMP_SOFTDATATYPE_TIME = 11;
        public const uint S7COMMP_SOFTDATATYPE_S5TIME = 12;
        public const uint S7COMMP_SOFTDATATYPE_S5COUNT = 13;
        public const uint S7COMMP_SOFTDATATYPE_DATEANDTIME = 14;
        public const uint S7COMMP_SOFTDATATYPE_INTERNETTIME = 15;
        public const uint S7COMMP_SOFTDATATYPE_ARRAY = 16;
        public const uint S7COMMP_SOFTDATATYPE_STRUCT = 17;
        public const uint S7COMMP_SOFTDATATYPE_ENDSTRUCT = 18;
        public const uint S7COMMP_SOFTDATATYPE_STRING = 19;
        public const uint S7COMMP_SOFTDATATYPE_POINTER = 20;
        public const uint S7COMMP_SOFTDATATYPE_MULTIFB = 21;
        public const uint S7COMMP_SOFTDATATYPE_ANY = 22;
        public const uint S7COMMP_SOFTDATATYPE_BLOCKFB = 23;
        public const uint S7COMMP_SOFTDATATYPE_BLOCKFC = 24;
        public const uint S7COMMP_SOFTDATATYPE_BLOCKDB = 25;
        public const uint S7COMMP_SOFTDATATYPE_BLOCKSDB = 26;
        public const uint S7COMMP_SOFTDATATYPE_MULTISFB = 27;
        public const uint S7COMMP_SOFTDATATYPE_COUNTER = 28;
        public const uint S7COMMP_SOFTDATATYPE_TIMER = 29;
        public const uint S7COMMP_SOFTDATATYPE_IECCOUNTER = 30;
        public const uint S7COMMP_SOFTDATATYPE_IECTIMER = 31;
        public const uint S7COMMP_SOFTDATATYPE_BLOCKSFB = 32;
        public const uint S7COMMP_SOFTDATATYPE_BLOCKSFC = 33;
        public const uint S7COMMP_SOFTDATATYPE_BLOCKCB = 34;
        public const uint S7COMMP_SOFTDATATYPE_BLOCKSCB = 35;
        public const uint S7COMMP_SOFTDATATYPE_BLOCKOB = 36;
        public const uint S7COMMP_SOFTDATATYPE_BLOCKUDT = 37;
        public const uint S7COMMP_SOFTDATATYPE_OFFSET = 38;
        public const uint S7COMMP_SOFTDATATYPE_BLOCKSDT = 39;
        public const uint S7COMMP_SOFTDATATYPE_BBOOL = 40;
        public const uint S7COMMP_SOFTDATATYPE_BLOCKEXT = 41;
        public const uint S7COMMP_SOFTDATATYPE_LREAL = 48;
        public const uint S7COMMP_SOFTDATATYPE_ULINT = 49;
        public const uint S7COMMP_SOFTDATATYPE_LINT = 50;
        public const uint S7COMMP_SOFTDATATYPE_LWORD = 51;
        public const uint S7COMMP_SOFTDATATYPE_USINT = 52;
        public const uint S7COMMP_SOFTDATATYPE_UINT = 53;
        public const uint S7COMMP_SOFTDATATYPE_UDINT = 54;
        public const uint S7COMMP_SOFTDATATYPE_SINT = 55;
        public const uint S7COMMP_SOFTDATATYPE_BCD8 = 56;
        public const uint S7COMMP_SOFTDATATYPE_BCD16 = 57;
        public const uint S7COMMP_SOFTDATATYPE_BCD32 = 58;
        public const uint S7COMMP_SOFTDATATYPE_BCD64 = 59;
        public const uint S7COMMP_SOFTDATATYPE_AREF = 60;
        public const uint S7COMMP_SOFTDATATYPE_WCHAR = 61;
        public const uint S7COMMP_SOFTDATATYPE_WSTRING = 62;
        public const uint S7COMMP_SOFTDATATYPE_VARIANT = 63;
        public const uint S7COMMP_SOFTDATATYPE_LTIME = 64;
        public const uint S7COMMP_SOFTDATATYPE_LTOD = 65;
        public const uint S7COMMP_SOFTDATATYPE_LDT = 66;
        public const uint S7COMMP_SOFTDATATYPE_DTL = 67;
        public const uint S7COMMP_SOFTDATATYPE_IECLTIMER = 68;
        public const uint S7COMMP_SOFTDATATYPE_IECSCOUNTER = 69;
        public const uint S7COMMP_SOFTDATATYPE_IECDCOUNTER = 70;
        public const uint S7COMMP_SOFTDATATYPE_IECLCOUNTER = 71;
        public const uint S7COMMP_SOFTDATATYPE_IECUCOUNTER = 72;
        public const uint S7COMMP_SOFTDATATYPE_IECUSCOUNTER = 73;
        public const uint S7COMMP_SOFTDATATYPE_IECUDCOUNTER = 74;
        public const uint S7COMMP_SOFTDATATYPE_IECULCOUNTER = 75;
        public const uint S7COMMP_SOFTDATATYPE_REMOTE = 96;
        public const uint S7COMMP_SOFTDATATYPE_ERRORSTRUCT = 97;
        public const uint S7COMMP_SOFTDATATYPE_NREF = 98;
        public const uint S7COMMP_SOFTDATATYPE_VREF = 99;
        public const uint S7COMMP_SOFTDATATYPE_FBTREF = 100;
        public const uint S7COMMP_SOFTDATATYPE_CREF = 101;
        public const uint S7COMMP_SOFTDATATYPE_VAREF = 102;
        public const uint S7COMMP_SOFTDATATYPE_AOMIDENT = 128;
        public const uint S7COMMP_SOFTDATATYPE_EVENTANY = 129;
        public const uint S7COMMP_SOFTDATATYPE_EVENTATT = 130;
        public const uint S7COMMP_SOFTDATATYPE_FOLDER = 131;
        public const uint S7COMMP_SOFTDATATYPE_AOMAID = 132;
        public const uint S7COMMP_SOFTDATATYPE_AOMLINK = 133;
        public const uint S7COMMP_SOFTDATATYPE_EVENTHWINT = 134;
        public const uint S7COMMP_SOFTDATATYPE_HWANY = 144;
        public const uint S7COMMP_SOFTDATATYPE_HWIOSYSTEM = 145;
        public const uint S7COMMP_SOFTDATATYPE_HWDPMASTER = 146;
        public const uint S7COMMP_SOFTDATATYPE_HWDEVICE = 147;
        public const uint S7COMMP_SOFTDATATYPE_HWDPSLAVE = 148;
        public const uint S7COMMP_SOFTDATATYPE_HWIO = 149;
        public const uint S7COMMP_SOFTDATATYPE_HWMODULE = 150;
        public const uint S7COMMP_SOFTDATATYPE_HWSUBMODULE = 151;
        public const uint S7COMMP_SOFTDATATYPE_HWHSC = 152;
        public const uint S7COMMP_SOFTDATATYPE_HWPWM = 153;
        public const uint S7COMMP_SOFTDATATYPE_HWPTO = 154;
        public const uint S7COMMP_SOFTDATATYPE_HWINTERFACE = 155;
        public const uint S7COMMP_SOFTDATATYPE_HWIEPORT = 156;
        public const uint S7COMMP_SOFTDATATYPE_OBANY = 160;
        public const uint S7COMMP_SOFTDATATYPE_OBDELAY = 161;
        public const uint S7COMMP_SOFTDATATYPE_OBTOD = 162;
        public const uint S7COMMP_SOFTDATATYPE_OBCYCLIC = 163;
        public const uint S7COMMP_SOFTDATATYPE_OBATT = 164;
        public const uint S7COMMP_SOFTDATATYPE_CONNANY = 168;
        public const uint S7COMMP_SOFTDATATYPE_CONNPRG = 169;
        public const uint S7COMMP_SOFTDATATYPE_CONNOUC = 170;
        public const uint S7COMMP_SOFTDATATYPE_CONNRID = 171;
        public const uint S7COMMP_SOFTDATATYPE_HWNR = 172;
        public const uint S7COMMP_SOFTDATATYPE_PORT = 173;
        public const uint S7COMMP_SOFTDATATYPE_RTM = 174;
        public const uint S7COMMP_SOFTDATATYPE_PIP = 175;
        public const uint S7COMMP_SOFTDATATYPE_CALARM = 176;
        public const uint S7COMMP_SOFTDATATYPE_CALARMS = 177;
        public const uint S7COMMP_SOFTDATATYPE_CALARM8 = 178;
        public const uint S7COMMP_SOFTDATATYPE_CALARM8P = 179;
        public const uint S7COMMP_SOFTDATATYPE_CALARMT = 180;
        public const uint S7COMMP_SOFTDATATYPE_CARSEND = 181;
        public const uint S7COMMP_SOFTDATATYPE_CNOTIFY = 182;
        public const uint S7COMMP_SOFTDATATYPE_CNOTIFY8P = 183;
        public const uint S7COMMP_SOFTDATATYPE_OBPCYCLE = 192;
        public const uint S7COMMP_SOFTDATATYPE_OBHWINT = 193;
        public const uint S7COMMP_SOFTDATATYPE_OBCOMM = 194;
        public const uint S7COMMP_SOFTDATATYPE_OBDIAG = 195;
        public const uint S7COMMP_SOFTDATATYPE_OBTIMEERROR = 196;
        public const uint S7COMMP_SOFTDATATYPE_OBSTARTUP = 197;
        public const uint S7COMMP_SOFTDATATYPE_OPCUALOCTXTENCM = 200;
        public const uint S7COMMP_SOFTDATATYPE_OPCUASTRACTLEN = 201;
        public const uint S7COMMP_SOFTDATATYPE_DBANY = 208;
        public const uint S7COMMP_SOFTDATATYPE_DBWWW = 209;
        public const uint S7COMMP_SOFTDATATYPE_DBDYN = 210;
        public const uint S7COMMP_SOFTDATATYPE_PARA = 253;
        public const uint S7COMMP_SOFTDATATYPE_LABEL = 254;
        public const uint S7COMMP_SOFTDATATYPE_UDEFINED = 255;
        public const uint S7COMMP_SOFTDATATYPE_NOTCHOSEN = 256;

        public static readonly Dictionary<uint, string> Types = new Dictionary<uint, string>
        {
            { S7COMMP_SOFTDATATYPE_VOID,                "Void" },
            { S7COMMP_SOFTDATATYPE_BOOL,                "Bool" },
            { S7COMMP_SOFTDATATYPE_BYTE,                "Byte" },
            { S7COMMP_SOFTDATATYPE_CHAR,                "Char" },
            { S7COMMP_SOFTDATATYPE_WORD,                "Word" },
            { S7COMMP_SOFTDATATYPE_INT,                 "Int" },
            { S7COMMP_SOFTDATATYPE_DWORD,               "DWord" },
            { S7COMMP_SOFTDATATYPE_DINT,                "DInt" },
            { S7COMMP_SOFTDATATYPE_REAL,                "Real" },
            { S7COMMP_SOFTDATATYPE_DATE,                "Date" },
            { S7COMMP_SOFTDATATYPE_TIMEOFDAY,           "Time_Of_Day" },
            { S7COMMP_SOFTDATATYPE_TIME,                "Time" },
            { S7COMMP_SOFTDATATYPE_S5TIME,              "S5Time" },
            { S7COMMP_SOFTDATATYPE_S5COUNT,             "S5Count" },
            { S7COMMP_SOFTDATATYPE_DATEANDTIME,         "Date_And_Time" },
            { S7COMMP_SOFTDATATYPE_INTERNETTIME,        "Internet_Time" },
            { S7COMMP_SOFTDATATYPE_ARRAY,               "Array" },
            { S7COMMP_SOFTDATATYPE_STRUCT,              "Struct" },
            { S7COMMP_SOFTDATATYPE_ENDSTRUCT,           "Endstruct" },
            { S7COMMP_SOFTDATATYPE_STRING,              "String" },
            { S7COMMP_SOFTDATATYPE_POINTER,             "Pointer" },
            { S7COMMP_SOFTDATATYPE_MULTIFB,             "Multi_FB" },
            { S7COMMP_SOFTDATATYPE_ANY,                 "Any" },
            { S7COMMP_SOFTDATATYPE_BLOCKFB,             "Block_FB" },
            { S7COMMP_SOFTDATATYPE_BLOCKFC,             "Block_FC" },
            { S7COMMP_SOFTDATATYPE_BLOCKDB,             "Block_DB" },
            { S7COMMP_SOFTDATATYPE_BLOCKSDB,            "Block_SDB" },
            { S7COMMP_SOFTDATATYPE_MULTISFB,            "Multi_SFB" },
            { S7COMMP_SOFTDATATYPE_COUNTER,             "Counter" },
            { S7COMMP_SOFTDATATYPE_TIMER,               "Timer" },
            { S7COMMP_SOFTDATATYPE_IECCOUNTER,          "IEC_COUNTER" },
            { S7COMMP_SOFTDATATYPE_IECTIMER,            "IEC_TIMER" },
            { S7COMMP_SOFTDATATYPE_BLOCKSFB,            "Block_SFB" },
            { S7COMMP_SOFTDATATYPE_BLOCKSFC,            "Block_SFC" },
            { S7COMMP_SOFTDATATYPE_BLOCKCB,             "Block_CB" },
            { S7COMMP_SOFTDATATYPE_BLOCKSCB,            "Block_SCB" },
            { S7COMMP_SOFTDATATYPE_BLOCKOB,             "Block_OB" },
            { S7COMMP_SOFTDATATYPE_BLOCKUDT,            "Block_UDT" },
            { S7COMMP_SOFTDATATYPE_OFFSET,              "Offset" },
            { S7COMMP_SOFTDATATYPE_BLOCKSDT,            "Block_SDT" },
            { S7COMMP_SOFTDATATYPE_BBOOL,               "BBOOL" },
            { S7COMMP_SOFTDATATYPE_BLOCKEXT,            "BLOCK_EXT" },
            { S7COMMP_SOFTDATATYPE_LREAL,               "LReal" },
            { S7COMMP_SOFTDATATYPE_ULINT,               "ULInt" },
            { S7COMMP_SOFTDATATYPE_LINT,                "LInt" },
            { S7COMMP_SOFTDATATYPE_LWORD,               "LWord" },
            { S7COMMP_SOFTDATATYPE_USINT,               "USInt" },
            { S7COMMP_SOFTDATATYPE_UINT,                "UInt" },
            { S7COMMP_SOFTDATATYPE_UDINT,               "UDInt" },
            { S7COMMP_SOFTDATATYPE_SINT,                "SInt" },
            { S7COMMP_SOFTDATATYPE_BCD8,                "Bcd8" },
            { S7COMMP_SOFTDATATYPE_BCD16,               "Bcd16" },
            { S7COMMP_SOFTDATATYPE_BCD32,               "Bcd32" },
            { S7COMMP_SOFTDATATYPE_BCD64,               "Bcd64" },
            { S7COMMP_SOFTDATATYPE_AREF,                "ARef" },
            { S7COMMP_SOFTDATATYPE_WCHAR,               "WChar" },
            { S7COMMP_SOFTDATATYPE_WSTRING,             "WString" },
            { S7COMMP_SOFTDATATYPE_VARIANT,             "Variant" },
            { S7COMMP_SOFTDATATYPE_LTIME,               "LTime" },
            { S7COMMP_SOFTDATATYPE_LTOD,                "LTOD" },
            { S7COMMP_SOFTDATATYPE_LDT,                 "LDT" },
            { S7COMMP_SOFTDATATYPE_DTL,                 "DTL" },
            { S7COMMP_SOFTDATATYPE_IECLTIMER,           "IEC_LTIMER" },
            { S7COMMP_SOFTDATATYPE_IECSCOUNTER,         "IEC_SCOUNTER" },
            { S7COMMP_SOFTDATATYPE_IECDCOUNTER,         "IEC_DCOUNTER" },
            { S7COMMP_SOFTDATATYPE_IECLCOUNTER,         "IEC_LCOUNTER" },
            { S7COMMP_SOFTDATATYPE_IECUCOUNTER,         "IEC_UCOUNTER" },
            { S7COMMP_SOFTDATATYPE_IECUSCOUNTER,        "IEC_USCOUNTER" },
            { S7COMMP_SOFTDATATYPE_IECUDCOUNTER,        "IEC_UDCOUNTER" },
            { S7COMMP_SOFTDATATYPE_IECULCOUNTER,        "IEC_ULCOUNTER" },
            { S7COMMP_SOFTDATATYPE_REMOTE,              "REMOTE" },
            { S7COMMP_SOFTDATATYPE_ERRORSTRUCT,         "Error_Struct" },
            { S7COMMP_SOFTDATATYPE_NREF,                "NREF" },
            { S7COMMP_SOFTDATATYPE_VREF,                "VREF" },
            { S7COMMP_SOFTDATATYPE_FBTREF,              "FBTREF" },
            { S7COMMP_SOFTDATATYPE_CREF,                "CREF" },
            { S7COMMP_SOFTDATATYPE_VAREF,               "VAREF" },
            { S7COMMP_SOFTDATATYPE_AOMIDENT,            "AOM_IDENT" },
            { S7COMMP_SOFTDATATYPE_EVENTANY,            "EVENT_ANY" },
            { S7COMMP_SOFTDATATYPE_EVENTATT,            "EVENT_ATT" },
            { S7COMMP_SOFTDATATYPE_FOLDER,              "FOLDER" },
            { S7COMMP_SOFTDATATYPE_AOMAID,              "AOM_AID" },
            { S7COMMP_SOFTDATATYPE_AOMLINK,             "AOM_LINK" },
            { S7COMMP_SOFTDATATYPE_EVENTHWINT,          "EVENT_HWINT" },
            { S7COMMP_SOFTDATATYPE_HWANY,               "HW_ANY" },
            { S7COMMP_SOFTDATATYPE_HWIOSYSTEM,          "HW_IOSYSTEM" },
            { S7COMMP_SOFTDATATYPE_HWDPMASTER,          "HW_DPMASTER" },
            { S7COMMP_SOFTDATATYPE_HWDEVICE,            "HW_DEVICE" },
            { S7COMMP_SOFTDATATYPE_HWDPSLAVE,           "HW_DPSLAVE" },
            { S7COMMP_SOFTDATATYPE_HWIO,                "HW_IO" },
            { S7COMMP_SOFTDATATYPE_HWMODULE,            "HW_MODULE" },
            { S7COMMP_SOFTDATATYPE_HWSUBMODULE,         "HW_SUBMODULE" },
            { S7COMMP_SOFTDATATYPE_HWHSC,               "HW_HSC" },
            { S7COMMP_SOFTDATATYPE_HWPWM,               "HW_PWM" },
            { S7COMMP_SOFTDATATYPE_HWPTO,               "HW_PTO" },
            { S7COMMP_SOFTDATATYPE_HWINTERFACE,         "HW_INTERFACE" },
            { S7COMMP_SOFTDATATYPE_HWIEPORT,            "HW_IEPORT" },
            { S7COMMP_SOFTDATATYPE_OBANY,               "OB_ANY" },
            { S7COMMP_SOFTDATATYPE_OBDELAY,             "OB_DELAY" },
            { S7COMMP_SOFTDATATYPE_OBTOD,               "OB_TOD" },
            { S7COMMP_SOFTDATATYPE_OBCYCLIC,            "OB_CYCLIC" },
            { S7COMMP_SOFTDATATYPE_OBATT,               "OB_ATT" },
            { S7COMMP_SOFTDATATYPE_CONNANY,             "CONN_ANY" },
            { S7COMMP_SOFTDATATYPE_CONNPRG,             "CONN_PRG" },
            { S7COMMP_SOFTDATATYPE_CONNOUC,             "CONN_OUC" },
            { S7COMMP_SOFTDATATYPE_CONNRID,             "CONN_R_ID" },
            { S7COMMP_SOFTDATATYPE_HWNR,                "HW_NR" },
            { S7COMMP_SOFTDATATYPE_PORT,                "PORT" },
            { S7COMMP_SOFTDATATYPE_RTM,                 "RTM" },
            { S7COMMP_SOFTDATATYPE_PIP,                 "PIP" },
            { S7COMMP_SOFTDATATYPE_CALARM,              "C_ALARM" },
            { S7COMMP_SOFTDATATYPE_CALARMS,             "C_ALARM_S" },
            { S7COMMP_SOFTDATATYPE_CALARM8,             "C_ALARM_8" },
            { S7COMMP_SOFTDATATYPE_CALARM8P,            "C_ALARM_8P" },
            { S7COMMP_SOFTDATATYPE_CALARMT,             "C_ALARM_T" },
            { S7COMMP_SOFTDATATYPE_CARSEND,             "C_AR_SEND" },
            { S7COMMP_SOFTDATATYPE_CNOTIFY,             "C_NOTIFY" },
            { S7COMMP_SOFTDATATYPE_CNOTIFY8P,           "C_NOTIFY_8P" },
            { S7COMMP_SOFTDATATYPE_OBPCYCLE,            "OB_PCYCLE" },
            { S7COMMP_SOFTDATATYPE_OBHWINT,             "OB_HWINT" },
            { S7COMMP_SOFTDATATYPE_OBCOMM,              "OB_COMM" },
            { S7COMMP_SOFTDATATYPE_OBDIAG,              "OB_DIAG" },
            { S7COMMP_SOFTDATATYPE_OBTIMEERROR,         "OB_TIMEERROR" },
            { S7COMMP_SOFTDATATYPE_OBSTARTUP,           "OB_STARTUP" },
            { S7COMMP_SOFTDATATYPE_OPCUALOCTXTENCM,     "OPC_UA_LocalizedTextEncodingMask" },
            { S7COMMP_SOFTDATATYPE_OPCUASTRACTLEN,      "OPC_UA_ByteStringActualLength" },
            { S7COMMP_SOFTDATATYPE_DBANY,               "DB_ANY" },
            { S7COMMP_SOFTDATATYPE_DBWWW,               "DB_WWW" },
            { S7COMMP_SOFTDATATYPE_DBDYN,               "DB_DYN" },
            { S7COMMP_SOFTDATATYPE_PARA,                "Para" },
            { S7COMMP_SOFTDATATYPE_LABEL,               "Label" },
            { S7COMMP_SOFTDATATYPE_UDEFINED,            "Undefined" },
            { S7COMMP_SOFTDATATYPE_NOTCHOSEN,           "NotChosen" }
        };
    }
}
