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

namespace S7CommPlusDriver.Alarming
{
    public class AlarmsAlarmTexts
    {
        public string Infotext = String.Empty;
        public string AlarmText = String.Empty;
        public string AdditionalText1 = String.Empty;
        public string AdditionalText2 = String.Empty;
        public string AdditionalText3 = String.Empty;
        public string AdditionalText4 = String.Empty;
        public string AdditionalText5 = String.Empty;
        public string AdditionalText6 = String.Empty;
        public string AdditionalText7 = String.Empty;
        public string AdditionalText8 = String.Empty;
        public string AdditionalText9 = String.Empty;

        // These two values we get in addition when browsing for the alarmtexts
        // Don't know if they are useful for something.
        public ushort UnknownValue1;
        public ushort UnknownValue2;

        public static AlarmsAlarmTexts FromNotificationBlob(ValueBlobSparseArray blob)
        {
            var at = new AlarmsAlarmTexts();
            string s;
            foreach (var v in blob.Value)
            {
                s = Utils.GetUtfString(v.Value.value, 0, (uint)v.Value.value.Length);
                // Values in older CPUs, from: 0xa09c8001..0xa09c800b (2694610945..2694610955)
                // Current CPUs use:           0x04070001..0x0407000b (  67567617..  67567627)
                // Did they change the key-values? Or depending on CPU?
                switch (v.Key)
                {
                    case 67567617:
                        at.Infotext = s;
                        break;
                    case 67567618:
                        at.AlarmText = s;
                        break;
                    case 67567619:
                        at.AdditionalText1 = s;
                        break;
                    case 67567620:
                        at.AdditionalText2 = s;
                        break;
                    case 67567621:
                        at.AdditionalText3 = s;
                        break;
                    case 67567622:
                        at.AdditionalText4 = s;
                        break;
                    case 67567623:
                        at.AdditionalText5 = s;
                        break;
                    case 67567624:
                        at.AdditionalText6 = s;
                        break;
                    case 67567625:
                        at.AdditionalText7 = s;
                        break;
                    case 67567626:
                        at.AdditionalText8 = s;
                        break;
                    case 67567627:
                        at.AdditionalText9 = s;
                        break;
                }
            }
            return at;
        }

        public override string ToString()
        {
            string s = "<AlarmsAlarmTexts>" + Environment.NewLine;
            s += "<Infotext>" + Infotext.ToString() + "</Infotext>" + Environment.NewLine;
            s += "<AlarmText>" + AlarmText.ToString() + "</AlarmText>" + Environment.NewLine;
            s += "<AdditionalText1>" + AdditionalText1.ToString() + "</AdditionalText1>" + Environment.NewLine;
            s += "<AdditionalText2>" + AdditionalText2.ToString() + "</AdditionalText2>" + Environment.NewLine;
            s += "<AdditionalText3>" + AdditionalText3.ToString() + "</AdditionalText3>" + Environment.NewLine;
            s += "<AdditionalText4>" + AdditionalText4.ToString() + "</AdditionalText4>" + Environment.NewLine;
            s += "<AdditionalText5>" + AdditionalText5.ToString() + "</AdditionalText5>" + Environment.NewLine;
            s += "<AdditionalText6>" + AdditionalText6.ToString() + "</AdditionalText6>" + Environment.NewLine;
            s += "<AdditionalText7>" + AdditionalText7.ToString() + "</AdditionalText7>" + Environment.NewLine;
            s += "<AdditionalText8>" + AdditionalText8.ToString() + "</AdditionalText8>" + Environment.NewLine;
            s += "<AdditionalText9>" + AdditionalText9.ToString() + "</AdditionalText9>" + Environment.NewLine;
            s += "</AlarmsAlarmTexts>" + Environment.NewLine;
            return s;
        }
    }
}
