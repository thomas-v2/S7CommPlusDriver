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
using System.IO;

namespace S7CommPlusDriver
{
    public class PObject : IS7pSerialize
    {
        public UInt32 RelationId;
        public UInt32 ClassId;
        public UInt32 ClassFlags;
        public UInt32 AttributeId;
        public Dictionary<UInt32, PValue> Attributes = new Dictionary<UInt32, PValue>();
        public Dictionary<Tuple<UInt32, UInt32>, PObject> Objects = new Dictionary<Tuple<UInt32, UInt32>, PObject>();
        public Dictionary<UInt32, UInt32> Relations = new Dictionary<UInt32, UInt32>();
        public PVartypeList VartypeList;
        public PVarnameList VarnameList;

        public PObject() : this(0, 0, 0)
        {
        }

        public PObject(UInt32 RID, UInt32 CLSID, UInt32 AID)
        {
            RelationId = RID;
            ClassId = CLSID;
            ClassFlags = 0;
            AttributeId = AID;
        }

        public void AddAttribute(UInt32 attributeid, PValue value)
        {
            Attributes.Add(attributeid, value);
        }

        public PValue GetAttribute(UInt32 attributeid)
        {
            return Attributes[attributeid];
        }

        public void AddRelation(UInt32 relationid, UInt32 value)
        {
            Relations.Add(relationid, value);
        }

        public void SetVartypeList(PVartypeList typelist)
        {
            VartypeList = typelist;
        }

        public void SetVarnameList(PVarnameList namelist)
        {
            VarnameList = namelist;
        }

        public void AddObject(PObject obj)
        {
            // Whether using the ClassId as Key makes sense, remains to be seen
            // TODO: The ClassId is not unique and may be occur more than once
            // (e.g. DB.Class_Rid and in RelId is the DB number as DB.1)
            var tuple = new Tuple<UInt32, UInt32>(obj.ClassId, obj.RelationId);
            Objects.Add(tuple, obj);
        }

        public PObject GetObjectByClassId(UInt32 classId, UInt32 relId)
        {
            var tuple = new Tuple<UInt32, UInt32>(classId, relId);
            return Objects[tuple];
        }

        public List<PObject> GetObjectsByClassId(UInt32 classId)
        {
            var objList = new List<PObject>();
            foreach(var obj in Objects)
            {
                if (obj.Key.Item1 == classId)
                {
                    objList.Add(obj.Value);
                }
            }
            return objList;
        }

        public List<PObject> GetObjects()
        {
            var objList = new List<PObject>();
            foreach (var obj in Objects)
            {
                objList.Add(obj.Value);
            }
            return objList;
        }

        public int Serialize(Stream buffer)
        {
            int ret = 0;
            ret += S7p.EncodeByte(buffer, ElementID.StartOfObject);
            ret += S7p.EncodeUInt32(buffer, RelationId);
            ret += S7p.EncodeUInt32Vlq(buffer, ClassId);
            ret += S7p.EncodeUInt32Vlq(buffer, ClassFlags);
            ret += S7p.EncodeUInt32Vlq(buffer, AttributeId);
            foreach (var elem in Attributes)
            {
                ret += S7p.EncodeByte(buffer, ElementID.Attribute);
                ret += S7p.EncodeUInt32Vlq(buffer, elem.Key);
                ret += elem.Value.Serialize(buffer);
            }
            foreach (var o in Objects)
            {
                ret += o.Value.Serialize(buffer);
            }
            foreach (var rel in Relations)
            {
                ret += S7p.EncodeByte(buffer, ElementID.Relation);
                ret += S7p.EncodeUInt32Vlq(buffer, rel.Key);
                ret += S7p.EncodeUInt32(buffer, rel.Value);
            }
            ret += S7p.EncodeByte(buffer, ElementID.TerminatingObject);
            return ret;
        }

        public override string ToString()
        {
            string s = "";
            s += "<Object>" + Environment.NewLine;
            s += "<RelationId>" + RelationId.ToString() + "</RelationId>" + Environment.NewLine;
            s += "<ClassId>" + ClassId.ToString() + "</ClassId>" + Environment.NewLine;
            s += "<AttributeId>" + AttributeId.ToString() + "</AttributeId>" + Environment.NewLine;
            foreach (var a in Attributes)
            {
                s += "<Attribute>" + Environment.NewLine;
                s += "<ID>" + a.Key.ToString() + "</ID>" + Environment.NewLine;
                s += a.Value.ToString();
                s += "</Attribute>" + Environment.NewLine;
            }
            if (VartypeList != null)
            {
                s += VartypeList.ToString();
            }
            if (VarnameList != null)
            {
                s += VarnameList.ToString();
            }
            foreach (var o in Objects)
            {
                s += o.Value.ToString();
            }
            foreach (var rel in Relations)
            {
                s += "<Relation>" + Environment.NewLine;
                s += "<ID>" + rel.Key.ToString() + "</ID>" + Environment.NewLine;
                s += rel.Value.ToString();
                s += "</Relation>" + Environment.NewLine;
            }
            s += "</Object>" + Environment.NewLine;
            return s;
        }
    }  
}
