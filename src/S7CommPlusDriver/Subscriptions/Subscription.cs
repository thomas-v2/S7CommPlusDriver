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
using S7CommPlusDriver.ClientApi;

namespace S7CommPlusDriver
{
    partial class S7CommPlusConnection
    {
        // *************************************************
        // *                IMPORTANT!                     *
        // * This is basically a test for subscriptions,   *
        // * how to use them, and how to later integrate   *
        // * this into the complete library!               *
        // *************************************************

        Dictionary<UInt32, PlcTag> m_SubscribedTags; // ItemRefId
        byte m_SubcriptionChangeCounter = 1;
        uint m_SubscriptionRelationId = 0x7fffc001; // TODO! Unknown value!0x7fffc001. Seems to be a startvalue, increases on next CreateObject. Guess: It's stored in the plc under this id.
        short m_NextCreditLimit;
        uint m_SubscriptionObjectId;

        /// <summary>
        /// Creates a subscription
        /// </summary>
        /// <param name="plcTags">The list of tags to add to the subscription</param>
        /// <param name="cycleTime">Cycle time for update in milliseconds. Lowest value seems to be 100 ms (if it's not dependant on the CPU).</param>
        /// <returns></returns>
        public int SubscriptionCreate(List<PlcTag> plcTags, ushort cycleTime)
        {
            int res;
            m_SubscribedTags = new Dictionary<uint, PlcTag>();
            PObject subsobj = new PObject();
            subsobj.ClassId = Ids.ClassSubscription;
            subsobj.RelationId = m_SubscriptionRelationId;
            subsobj.AddAttribute(Ids.ObjectVariableTypeName, new ValueWString("Subscription_" + m_SubscriptionRelationId.ToString()));
            subsobj.AddAttribute(Ids.SubscriptionFunctionClassId, new ValueUSInt(0));
            subsobj.AddAttribute(Ids.SubscriptionMissedSendings, new ValueUInt(0));
            subsobj.AddAttribute(Ids.SubscriptionSubsystemError, new ValueLInt(0));
            subsobj.AddAttribute(Ids.SubscriptionRouteMode, new ValueUSInt(0x14)); // TODO Unknown, mostly seen 0x04, 0x14 or 0x15. Needs to be tested

            // Testresults of some RouteModes (0x04, 0x14, 0x20) some applications are using, together with credit limits:
            // For Alarm Subscription RouteMode 0x02 is used.
            //-----------+-------------+-----------------------------------------------------------------------------------------------------------------------------------------------------------------
            // RouteMode | CreditLimit | Behaviour
            //-----------+-------------+-----------------------------------------------------------------------------------------------------------------------------------------------------------------
            // 0x00      |  0          | No notification at all
            // 0x00      | -1          | All values on create; then values that have changed, empty Notification each cycle; unlimited without retriggering; CreditTick always 0
            // 0x00      | n>0         | All values on create; then values that have changed, empty Notification each cycle; stops after CreditTick reaches difference of n when not set to new value
            // 0x04      | 0           | Identical to 0x00 / 0
            // 0x04      | -1          | Identical to 0x00 / -1
            // 0x04      | n>0         | Identical to 0x00 / n>0
            // 0x14      | 0           | Identical to 0x00 / 0
            // 0x14      | -1          | Identical to 0x00 / -1
            // 0x14      | n>0         | Identical to 0x00 / n>0
            // 0x20      | 0           | Identical to 0x00 / 0
            // 0x20      | -1          | All values on create; then values that have changed, on cycle without change no notification; unlimited without retriggering; CreditTick always 0
            // 0x20      | n>0         | All values on create; then values that have changed, on cycle without change no notification; stops after CreditTick reaches difference of n when not set to new value

            subsobj.AddAttribute(Ids.SubscriptionActive, new ValueBool(true));
            subsobj.AddAttribute(Ids.SubscriptionReferenceList, GetSubscriptionListArray(plcTags));
            subsobj.AddAttribute(Ids.SubscriptionCycleTime, new ValueUDInt(cycleTime));
            subsobj.AddAttribute(Ids.SubscriptionDisabled, new ValueUSInt(0));
            subsobj.AddAttribute(Ids.SubscriptionCount, new ValueUSInt(0));
            m_NextCreditLimit = 10;
            subsobj.AddAttribute(Ids.SubscriptionCreditLimit, new ValueInt(m_NextCreditLimit)); // -1=unlimited, 255 = max
            subsobj.AddAttribute(Ids.SubscriptionTicks, new ValueUInt(65535));
            // 1055 = Unknown -> is working without setting this.
            subsobj.AddAttribute(1055, new ValueUSInt(0));

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
                Console.WriteLine("Subscription - Create: CreateObjectResponse with Error!");
                return S7Consts.errIsoInvalidPDU;
            }

            if (createObjRes.ReturnValue == 0)
            {
                // Save the ObjectId, to modify the existing subscription if needed
                m_SubscriptionObjectId = createObjRes.ObjectIds[0];
            }
            else
            {
                // If creating a subscription fails, the object is still created and should be deleted.
                // At least deleting it, gives no error.
                Console.WriteLine(String.Format("Subscription - Create: Failed with Returnvalue = 0x{0:X8}", createObjRes.ReturnValue));
                res = S7Consts.errCliInvalidParams;
            }
            return res;
        }

        private int SubscriptionSetCreditLimit(short limit)
        {
            int res;
            var setVarReq = new SetVariableRequest(ProtocolVersion.V2);
            setVarReq.TransportFlags = 0x74; // Set flag, that we need no response
            setVarReq.InObjectId = m_SubscriptionObjectId;
            setVarReq.Address = Ids.SubscriptionCreditLimit;
            setVarReq.Value = new ValueInt(limit);
            res = SendS7plusFunctionObject(setVarReq);
            return res;
        }

        private ValueUDIntArray GetSubscriptionListArray(List<PlcTag> plcTags)
        {
            var la = new List<uint>();
            // 0x8?ssxxxx = 8 = flag CreateNew, ss = 1 byte subscription Change counter, xxxx = unknown
            la.Add(0x80000000 | ((uint)(m_SubcriptionChangeCounter) << 16));
            la.Add(0);                     // Number of items to unsubscribe
            la.Add((uint)plcTags.Count);   // Number of items to subscribe

            uint tagReferenceId = 1;
            uint head;
            foreach (var tag in plcTags)
            {
                // Save the reference Id in the dictionary. In the notification we get this reference Id back
                // and know to which tag the value belongs to.
                m_SubscribedTags.Add(tagReferenceId, tag);
                // Write the Item address
                head = 0x80040000;
                // It's not known where 0x8004 stands for -> 4 was a guess it's for the number of fields
                // before the LIDs, but that's wrong (coincidentally fits here in this special case).
                // Get the number of IDs in advance, Sub-Area counts as one, and then count each LID.
                // 0x8aaabbbb = aaa = unknown value, bbbb = number of fields in the 2nd part.
                head |= (uint)(1 + tag.Address.LID.Count);
                la.Add(head);
                la.Add(tagReferenceId);
                la.Add(0); // Unknown 1
                la.Add(tag.Address.AccessArea);
                la.Add(tag.Address.SymbolCrc);
                // Count value in head starts from here
                la.Add(tag.Address.AccessSubArea);
                foreach(var li in tag.Address.LID)
                {
                    la.Add(li);
                }
                tagReferenceId++;
            }
            // Convert all data to protocol UDInt Array (VLQ encoded)
            return new ValueUDIntArray(la.ToArray(), 0x20); // 0x20 -> Adressarray
        }

        public int TestWaitForVariableChangeNotifications(int untilNumberOfNotifications)
        {
            int res = 0;
            short creditLimitStep = 5;

            for (int i = 1; i <= untilNumberOfNotifications; i++)
            {
                Console.WriteLine(Environment.NewLine + "WaitForNotifications(): *** Loop #" + i.ToString() + " ***");
                m_LastError = 0;
                WaitForNewS7plusReceived(5000);
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
                    Console.WriteLine(String.Format(" PLC-Timestamp={0}.{1:D03} ValuesCount={2}", noti.Add1Timestamp.ToString(), noti.Add1Timestamp.Millisecond, noti.Values.Count));
                    foreach(var v in noti.Values)
                    {
                        Console.WriteLine("---> key=" + v.Key + " value=" + v.Value.ToString());
                        // Error value in tags expects a 64 bit value, in subscriptions it's only 1 byte (for it's not known what all values are for -> TODO)
                        m_SubscribedTags[v.Key].ProcessReadResult(v.Value, 0);
                    }

                    if (noti.NotificationCreditTick >= m_NextCreditLimit - 1) // Set new limit one tick before it expires, to get a constant flow of data
                    {
                        // CreditTick in Notification is only one byte
                        m_NextCreditLimit = (short)((m_NextCreditLimit + creditLimitStep) % 255);
                        Console.WriteLine("--> Credit limit of " + noti.NotificationCreditTick + " reached. SetCreditLimit to " + m_NextCreditLimit.ToString());
                        SubscriptionSetCreditLimit(m_NextCreditLimit);
                    }
                }
            }
            return res;
        }

        public int SubscriptionDelete()
        {
            int res;
            m_SubscribedTags.Clear();
            m_SubscriptionObjectId = 0;
            Console.WriteLine(String.Format("SubscriptionDelete: Calling DeleteObject for SessionId2={0:X8}", SessionId2));
            res = DeleteObject(SessionId2);
            return res;
        }
    }
}
