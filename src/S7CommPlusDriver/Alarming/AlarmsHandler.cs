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
using S7CommPlusDriver.Alarming;

namespace S7CommPlusDriver
{
    public partial class S7CommPlusConnection
    {
        // *************************************************
        // *                IMPORTANT!                     *
        // * This is basically a test for Alarming,        *
        // * how to use it, and how to later integrate     *
        // * this into the complete library!               *
        // *************************************************
        //
        // Example code for testing:
        // CultureInfo ci = new CultureInfo("en-US");
        // conn.AlarmSubscriptionCreate();
        // conn.TestWaitForAlarmNotifications(20000, 3, ci.LCID);
        // conn.AlarmSubscriptionDelete();

        uint m_AlarmSubscriptionRelationId = 0x7fffc001; // TODO! Unknown value! See also Subscription.cs
        uint m_AlarmSubscriptionRefRelationId = 0x51010001; // TODO! Unknown value!
        short m_AlarmNextCreditLimit;
        uint m_AlarmSubscriptionObjectId;

        public int AlarmSubscriptionCreate()
        {
            int res;
            PObject subsobj = new PObject();
            subsobj.ClassId = Ids.ClassSubscription;
            subsobj.RelationId = m_AlarmSubscriptionRelationId;
            subsobj.AddAttribute(Ids.ObjectVariableTypeName, new ValueWString("Subscription_" + m_AlarmSubscriptionRelationId.ToString()));
            subsobj.AddAttribute(Ids.SubscriptionFunctionClassId, new ValueUSInt(2));
            subsobj.AddAttribute(Ids.SubscriptionMissedSendings, new ValueUInt(0));
            subsobj.AddAttribute(Ids.SubscriptionSubsystemError, new ValueLInt(0));
            subsobj.AddAttribute(Ids.SubscriptionRouteMode, new ValueUSInt(2)); // TODO Unknown
            subsobj.AddAttribute(Ids.SubscriptionActive, new ValueBool(true));
            subsobj.AddAttribute(Ids.SubscriptionReferenceList, new ValueUDIntArray(new uint[3] { 0x80010000, 0, 0 }, 0x20)); // 0x20 = Adressarray
            subsobj.AddAttribute(Ids.SubscriptionCycleTime, new ValueUDInt(0));
            subsobj.AddAttribute(Ids.SubscriptionDelayTime, new ValueUDInt(0));
            subsobj.AddAttribute(Ids.SubscriptionDisabled, new ValueUSInt(0));
            subsobj.AddAttribute(Ids.SubscriptionCount, new ValueUSInt(0));
            m_AlarmNextCreditLimit = 10;
            subsobj.AddAttribute(Ids.SubscriptionCreditLimit, new ValueInt(m_AlarmNextCreditLimit)); // -1=unlimited, 255 = max
            subsobj.AddAttribute(Ids.SubscriptionTicks, new ValueUInt(65535)); // 65535
            // 1055 = Unknown -> is working without setting this. Maybe default attribute is zero.
            //subsobj.AddAttribute(1055, new ValueUSInt(0));
            PObject asrefsobj = new PObject();
            asrefsobj.ClassId = Ids.AlarmSubscriptionRef_Class_Rid;
            asrefsobj.RelationId = m_AlarmSubscriptionRefRelationId;
            asrefsobj.AddAttribute(Ids.ObjectVariableTypeName, new ValueWString("S7pDriver_Alarming"));
            asrefsobj.AddAttribute(Ids.SubscriptionReferenceTriggerAndTransmitMode, new ValueUSInt(3));
            asrefsobj.AddAttribute(Ids.AlarmSubscriptionRef_AlarmDomain, new ValueUIntArray(new ushort[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0x10));
            // Also variant to set explicit the alarm domain as filter, for example:
            // {1, 256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 267, 268, 269, 270, 271, 272}
            // Possibly 65535 is "all".
            asrefsobj.AddAttribute(Ids.AlarmSubscriptionRef_AlarmDomain2, new ValueUIntArray(new ushort[1] { 65535 }, 0x20)); // 0x20 = Adressarray
            // OPTION: 
            // Send text informations with the message, we don't need to browse them in advance.
            asrefsobj.AddAttribute(Ids.AlarmSubscriptionRef_AlarmTextLanguages_Rid, new ValueUDIntArray(new uint[0], 0x20)); // Empty for all languanges? Otherwise e.g. 1031 for de-de or what you need.
            asrefsobj.AddAttribute(Ids.AlarmSubscriptionRef_SendAlarmTexts_Rid, new ValueBool(true));

            asrefsobj.AddRelation(Ids.AlarmSubscriptionRef_itsAlarmSubsystem, 0x00000008);
            subsobj.AddObject(asrefsobj);
            // Build the request object
            var createObjReq = new CreateObjectRequest(ProtocolVersion.V2, 0, true);
            createObjReq.TransportFlags = 0x34;
            createObjReq.RequestId = SessionId2;
            createObjReq.RequestValue = new ValueUDInt(0);
            createObjReq.SetRequestObject(subsobj);

            // Send it
            res = SendS7plusFunctionObject(createObjReq);
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                m_client.Disconnect();
                return m_LastError;
            }

            var createObjRes = CreateObjectResponse.DeserializeFromPdu(m_ReceivedPDU);
            if (createObjRes == null)
            {
                Console.WriteLine("AlarmSubscription - Create: CreateObjectResponse with Error!");
                return S7Consts.errIsoInvalidPDU;
            }

            if (createObjRes.ReturnValue == 0)
            {
                // Save the ObjectId, to modify the existing subscription
                m_AlarmSubscriptionObjectId = createObjRes.ObjectIds[0];
            }
            else
            {
                // If creating a subscription fails, the object is still created and should be deleted.
                // At least deleting it, gives no error.
                Console.WriteLine(String.Format("AlarmSubscription - Create: Failed with Returnvalue = 0x{0:X8}", createObjRes.ReturnValue));
                res = S7Consts.errCliInvalidParams;
            }

            return res;
        }

        public int TestWaitForAlarmNotifications(int waitTimeout, int untilNumberOfAlarms, int alarmTextsLanguageId)
        {
            int res = 0;
            short creditLimitStep = 5;

            for (int i = 1; i <= untilNumberOfAlarms; i++)
            {
                Console.WriteLine(Environment.NewLine + "WaitForAlarmNotifications(): *** Loop #" + i.ToString() + " ***");
                m_LastError = 0;
                WaitForNewS7plusReceived(waitTimeout);
                if (m_LastError != 0)
                {
                    return m_LastError;
                }

                var noti = Notification.DeserializeFromPdu(m_ReceivedPDU);
                if (noti == null)
                {
                    Console.WriteLine("Notification == null!");
                    return S7Consts.errIsoInvalidPDU;
                }
                else
                {
                    Console.Write("Notification: CreditTick=" + noti.NotificationCreditTick + " SequenceNumber=" + noti.NotificationSequenceNumber);
                    Console.WriteLine(String.Format(" PLC-Timestamp={0}.{1:D03}", noti.Add1Timestamp.ToString(), noti.Add1Timestamp.Millisecond));

                    var dai = AlarmsDai.FromNotificationObject(noti.P2Objects[0], alarmTextsLanguageId);
                    Console.WriteLine(dai.ToString());
                    if (noti.NotificationCreditTick >= m_AlarmNextCreditLimit - 1) // Set new limit one tick before it expires, to get a constant flow of data
                    {
                        // CreditTick in Notification is only one byte
                        m_AlarmNextCreditLimit = (short)((m_AlarmNextCreditLimit + creditLimitStep) % 255);
                        Console.WriteLine("--> Credit limit of " + noti.NotificationCreditTick + " reached. SetCreditLimit to " + m_AlarmNextCreditLimit.ToString());
                        SubscriptionSetCreditLimit(m_AlarmNextCreditLimit);
                    }
                }
            }
            return res;
        }

        public int AlarmSubscriptionDelete()
        {
            int res;
            m_AlarmSubscriptionObjectId = 0;
            Console.WriteLine(String.Format("SubscriptionDelete: Calling DeleteObject for SessionId2={0:X8}", SessionId2));
            res = DeleteObject(SessionId2);
            return res;
        }
    }
}
