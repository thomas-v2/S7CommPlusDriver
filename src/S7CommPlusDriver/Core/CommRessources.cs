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

namespace S7CommPlusDriver
{
    public class CommRessources
    {
        public int TagsPerReadRequestMax { get; private set; } = 20;
        public int TagsPerWriteRequestMax { get; private set; } = 20;
        public int PlcAttributesMax { get; private set; }
        public int PlcAttributesFree { get; private set; }
        public int PlcSubscriptionsMax { get; private set; }
        public int PlcSubscriptionsFree { get; private set; }
        public int SubscriptionMemoryMax { get; private set; }
        public int SubscriptionMemoryFree { get; private set; }

        public int ReadMax(S7CommPlusConnection conn)
        {
            // Read SystemLimits
            // Assumption (so far, because for all CPUs which have be seen both values were the same):
            // 1000 = Number for Reading
            // 1001 = Number for Writing
            int res;
            var readlist = new List<ItemAddress>();
            var values = new List<object>();
            var errors = new List<UInt64>();

            var adrTagsPerReadRequestMax = new ItemAddress
            {
                AccessArea = Ids.ObjectRoot,
                AccessSubArea = Ids.SystemLimits
            };
            adrTagsPerReadRequestMax.LID.Add(1000);

            var adrTagsPerWriteRequestMax = new ItemAddress
            {
                AccessArea = Ids.ObjectRoot,
                AccessSubArea = Ids.SystemLimits
            };
            adrTagsPerWriteRequestMax.LID.Add(1001);

            var adrPlcSubscriptionsMax = new ItemAddress
            {
                AccessArea = Ids.ObjectRoot,
                AccessSubArea = Ids.SystemLimits
            };
            adrPlcSubscriptionsMax.LID.Add(0);

            var adrPlcAttributesMax = new ItemAddress
            {
                AccessArea = Ids.ObjectRoot,
                AccessSubArea = Ids.SystemLimits
            };
            adrPlcAttributesMax.LID.Add(1);

            var adrSubscriptionMemoryMax = new ItemAddress
            {
                AccessArea = Ids.ObjectRoot,
                AccessSubArea = Ids.SystemLimits
            };
            adrSubscriptionMemoryMax.LID.Add(2);

            readlist.Add(adrTagsPerReadRequestMax);
            readlist.Add(adrTagsPerWriteRequestMax);
            readlist.Add(adrPlcSubscriptionsMax);
            readlist.Add(adrPlcAttributesMax);
            readlist.Add(adrSubscriptionMemoryMax);

            res = conn.ReadValues(readlist, out values, out errors);
            int i = 0;
            for (i = 0; i < values.Count; i++)
            {
                if (values[i] != null && errors[i] == 0)
                {
                    int v = ((ValueDInt)values[i]).GetValue();
                    switch (i)
                    {
                        case 0:
                            TagsPerReadRequestMax = v;
                            break;
                        case 1:
                            TagsPerWriteRequestMax = v;
                            break;
                        case 2:
                            PlcSubscriptionsMax = v;
                            break;
                        case 3:
                            PlcAttributesMax = v;
                            break;
                        case 4:
                            SubscriptionMemoryMax = v;
                            break;
                    }
                }
            }
            return res;
        }

        public int ReadFree(S7CommPlusConnection conn)
        {
            int res;
            var readlist = new List<ItemAddress>();
            var values = new List<object>();
            var errors = new List<UInt64>();

            var adrPlcSubscriptionsFree = new ItemAddress
            {
                AccessArea = Ids.ObjectRoot,
                AccessSubArea = Ids.FreeItems
            };
            adrPlcSubscriptionsFree.LID.Add(0);

            var adrPlcAttributesFree = new ItemAddress
            {
                AccessArea = Ids.ObjectRoot,
                AccessSubArea = Ids.FreeItems
            };
            adrPlcAttributesFree.LID.Add(1);

            var adrSubscriptionMemoryFree = new ItemAddress
            {
                AccessArea = Ids.ObjectRoot,
                AccessSubArea = Ids.FreeItems
            };
            adrSubscriptionMemoryFree.LID.Add(2);

            readlist.Add(adrPlcSubscriptionsFree);
            readlist.Add(adrPlcAttributesFree);
            readlist.Add(adrSubscriptionMemoryFree);

            res = conn.ReadValues(readlist, out values, out errors);
            int i = 0;
            for (i = 0; i < values.Count; i++)
            {
                if (values[i] != null && errors[i] == 0)
                {
                    int v = ((ValueDInt)values[i]).GetValue();
                    switch (i)
                    {
                        case 0:
                            PlcSubscriptionsFree = v;
                            break;
                        case 1:
                            PlcAttributesFree = v;
                            break;
                        case 2:
                            SubscriptionMemoryFree = v;
                            break;
                    }
                }
            }
            return res;
        }

        public override string ToString()
        {
            string s = "<CommRessources>" + Environment.NewLine;
            s += "<TagsPerReadRequestMax>" + TagsPerReadRequestMax.ToString() + "</TagsPerReadRequestMax>" + Environment.NewLine;
            s += "<TagsPerWriteRequestMax>" + TagsPerWriteRequestMax.ToString() + "</TagsPerWriteRequestMax>" + Environment.NewLine;
            s += "<PlcAttributesMax>" + PlcAttributesMax.ToString() + "</PlcAttributesMax>" + Environment.NewLine;
            s += "<PlcAttributesFree>" + PlcAttributesFree.ToString() + "</PlcAttributesFree>" + Environment.NewLine;
            s += "<PlcSubscriptionsMax>" + PlcSubscriptionsMax.ToString() + "</PlcSubscriptionsMax>" + Environment.NewLine;
            s += "<PlcSubscriptionsFree>" + PlcSubscriptionsFree.ToString() + "</PlcSubscriptionsFree>" + Environment.NewLine;
            s += "<SubscriptionMemoryMax>" + SubscriptionMemoryMax.ToString() + "</SubscriptionMemoryMax>" + Environment.NewLine;
            s += "<SubscriptionMemoryFree>" + SubscriptionMemoryFree.ToString() + "</SubscriptionMemoryFree>" + Environment.NewLine;
            s += "</CommRessources>" + Environment.NewLine;
            return s;
        }
    }
}