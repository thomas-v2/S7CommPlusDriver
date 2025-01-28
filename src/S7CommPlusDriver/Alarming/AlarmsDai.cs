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

namespace S7CommPlusDriver.Alarming
{
    public class AlarmsDai
    {
        public string ObjectVariableTypeName;

        public ulong CpuAlarmId;
        public byte AllStatesInfo;
        public ushort AlarmDomain;
        public int MessageType;
        public uint SequenceCounter;
        public AlarmsAlarmTexts AlarmTexts;
        public AlarmsHmiInfo HmiInfo;
        public AlarmsAsCgs AsCgs;

        public override string ToString()
        {
            string s = "<AlarmsDai>" + Environment.NewLine;
            s += "<ObjectVariableTypeName>" + ObjectVariableTypeName.ToString() + "</ObjectVariableTypeName>" + Environment.NewLine;
            s += "<CpuAlarmId>" + CpuAlarmId.ToString() + "</CpuAlarmId>" + Environment.NewLine;
            s += "<AllStatesInfo>" + AllStatesInfo.ToString() + "</AllStatesInfo>" + Environment.NewLine;
            s += "<AlarmDomain>" + AlarmDomain.ToString() + "</AlarmDomain>" + Environment.NewLine;
            s += "<MessageType>" + MessageType.ToString() + "</MessageType>" + Environment.NewLine;
            s += "<HmiInfo>" + Environment.NewLine + HmiInfo.ToString() + "</HmiInfo>" + Environment.NewLine;
            s += "<AsCgs>" + Environment.NewLine + AsCgs.ToString() + "</AsCgs>" + Environment.NewLine;
            s += "<SequenceCounter>" + SequenceCounter.ToString() + "</SequenceCounter>" + Environment.NewLine;
            if (AlarmTexts != null)
            {
                s += "<AlarmTexts>" + Environment.NewLine + AlarmTexts.ToString() + "</AlarmTexts>" + Environment.NewLine;
            }
            else
            {
                s += "<AlarmTexts></AlarmTexts>" + Environment.NewLine;
            }
            s += "</AlarmsDai>" + Environment.NewLine;
            return s;
        }

        public static AlarmsDai FromNotificationObject(PObject pobj, int alarmtextsLanguageId)
        {
            var dai = new AlarmsDai();
            dai.ObjectVariableTypeName = ((ValueWString)pobj.GetAttribute(Ids.ObjectVariableTypeName)).GetValue();
            dai.CpuAlarmId = ((ValueLWord)pobj.GetAttribute(Ids.DAI_CPUAlarmID)).GetValue();
            dai.AllStatesInfo = ((ValueUSInt)pobj.GetAttribute(Ids.DAI_AllStatesInfo)).GetValue();
            dai.AlarmDomain = ((ValueUInt)pobj.GetAttribute(Ids.DAI_AlarmDomain)).GetValue();
            dai.MessageType = ((ValueDInt)pobj.GetAttribute(Ids.DAI_MessageType)).GetValue();
            dai.HmiInfo = AlarmsHmiInfo.FromValueBlob(((ValueBlob)pobj.GetAttribute(Ids.DAI_HmiInfo)));
            // TODO: Blob for additional values
            dai.SequenceCounter = ((ValueUDInt)pobj.GetAttribute(Ids.DAI_SequenceCounter)).GetValue();
            ValueStruct str = null;
            uint dai_id = 0;
            if (pobj.Attributes.ContainsKey(Ids.DAI_Coming))
            {
                str = (ValueStruct)pobj.GetAttribute(Ids.DAI_Coming);
                dai_id = Ids.DAI_Coming;
            }
            else if (pobj.Attributes.ContainsKey(Ids.DAI_Going))
            {
                str = (ValueStruct)pobj.GetAttribute(Ids.DAI_Going);
                dai_id = Ids.DAI_Going;
            }
            if (dai_id == 0)
            {
                return null;
            }
            dai.AsCgs = AlarmsAsCgs.FromValueStruct(str);
            dai.AsCgs.SubtypeId = dai_id;
            dai.AlarmTexts = AlarmsAlarmTexts.FromNotificationBlob(((ValueBlobSparseArray)pobj.GetAttribute(Ids.DAI_AlarmTexts_Rid)), alarmtextsLanguageId);
            return dai;
        }
    }
}