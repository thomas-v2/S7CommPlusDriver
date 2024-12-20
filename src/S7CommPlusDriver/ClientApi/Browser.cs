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
    public class Browser
    {
        VarRoot m_Root;
        List<PObject> m_objs;
        List<VarInfo> m_varInfoList;

        public Browser()
        {
            m_Root = new VarRoot();
            m_Root.Nodes = new List<Node>();
        }

        public List<VarInfo> GetVarInfoList()
        {
            return m_varInfoList;
        }

        public void SetTypeInfoContainerObjects(List<PObject> objs)
        {
            m_objs = objs;
        }

        public void AddBlockNode(eNodeType nodetype, string name, uint accessid, uint ti_rel_id)
        {
            var db = new Node
            {
                NodeType = nodetype,
                Name = name,
                AccessId = accessid,
                RelationId = ti_rel_id
            };
            m_Root.Nodes.Add(db);
        }

        public void BuildFlatList()
        {
            m_varInfoList = new List<VarInfo>();
            foreach (var node in m_Root.Nodes)
            {
                // Skip empty lists in any area like marker or timers.
                if (node.Childs.Count > 0)
                {
                    AddFlatSubnodes(node, String.Empty, String.Empty);
                }
            }
        }

        private void AddFlatSubnodes(Node node, string names, string accessIds)
        {
            switch (node.NodeType)
            {
                case eNodeType.Root:
                    names += node.Name;
                    accessIds += String.Format("{0:X}", node.AccessId);
                    break;
                case eNodeType.Array:
                    names += node.Name;
                    accessIds += "." + String.Format("{0:X}", node.AccessId);
                    break;
                case eNodeType.StructArray:
                    names += node.Name;
                    // TODO: Special: Between an array-index and the access-id is an additional 1. It's not known if it's a fixed or variable value.
                    accessIds += "." + String.Format("{0:X}", node.AccessId) + ".1";
                    break;
                default:
                    names += "." + node.Name;
                    accessIds += "." + String.Format("{0:X}", node.AccessId);
                    break;
            }
            
            if (node.Childs.Count == 0)
            {
                // We are at the leaf of our tree
                if (IsSoftdatatypeSupported(node.Softdatatype))
                {
                    var info = new VarInfo
                    {
                        Name = names,
                        AccessSequence = accessIds,
                        Softdatatype = node.Softdatatype
                    };
                    m_varInfoList.Add(info);
                }
            }
            else
            {
                foreach (var sub in node.Childs)
                {
                    AddFlatSubnodes(sub, names, accessIds);
                }
            }
        }

        public void BuildTree()
        {
            for (int i = 0; i < m_Root.Nodes.Count; i++)
            {
                foreach (var ob in m_objs)
                {
                    if (ob.RelationId == m_Root.Nodes[i].RelationId)
                    {
                        var node = m_Root.Nodes[i];
                        AddSubNodes(ref node, ob);
                        break;
                    }
                }
            }
        }

        private void AddSubNodes(ref Node node, PObject o)
        {
            uint ArrayElementCount;
            int ArrayLowerBounds;
            uint[] MdimArrayElementCount;
            int[] MdimArrayLowerBounds;

            int element_index = 0;
            // If there are no variables at all in an area, then this list does not exist (no error).
            if (o.VartypeList != null)
            {
                foreach (var vte in o.VartypeList.Elements)
                {
                    var subnode = new Node
                    {
                        Name = o.VarnameList.Names[element_index],
                        Softdatatype = vte.Softdatatype,
                        AccessId = vte.LID
                    };
                    node.Childs.Add(subnode);
                    // Process arrays. TODO: Put the processing to separate methods, to shorten this method.
                    if (vte.OffsetInfoType.Is1Dim())
                    {
                        #region Struct/UDT or flat arrays with one dimension
                        var ioit = (IOffsetInfoType_1Dim)vte.OffsetInfoType;
                        ArrayElementCount = ioit.GetArrayElementCount();
                        ArrayLowerBounds = ioit.GetArrayLowerBounds();

                        // The access-id always starts with 0, independent of lowerbounds
                        for (uint i = 0; i < ArrayElementCount; i++)
                        {
                            // Handle Struct/FB Array separate: Has an additional ID between array index and access-LID.
                            if (vte.OffsetInfoType.HasRelation())
                            {
                                var arraynode = new Node
                                {
                                    NodeType = eNodeType.StructArray,
                                    Name = "[" + (i + ArrayLowerBounds) + "]",
                                    Softdatatype = vte.Softdatatype,
                                    AccessId = i
                                };
                                subnode.Childs.Add(arraynode);

                                // All OffsetInfoTypes which occur at this point should have a Relation Id
                                var ioit2 = (IOffsetInfoType_Relation)vte.OffsetInfoType;

                                foreach (var ob in m_objs)
                                {
                                    if (ob.RelationId == ioit2.GetRelationId())
                                    {
                                        AddSubNodes(ref arraynode, ob);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                var arraynode = new Node
                                {
                                    NodeType = eNodeType.Array,
                                    Name = "[" + (i + ArrayLowerBounds) + "]",
                                    Softdatatype = vte.Softdatatype,
                                    AccessId = i
                                };
                                subnode.Childs.Add(arraynode);
                            }
                        }
                        #endregion
                    }
                    else if (vte.OffsetInfoType.IsMDim())
                    {
                        #region Struct/UDT or flat array with more than one dimension
                        var ioit = (IOffsetInfoType_MDim)vte.OffsetInfoType;
                        ArrayElementCount = ioit.GetArrayElementCount();
                        ArrayLowerBounds = ioit.GetArrayLowerBounds();
                        MdimArrayElementCount = ioit.GetMdimArrayElementCount();
                        MdimArrayLowerBounds = ioit.GetMdimArrayLowerBounds();

                        // Determine the actual number of dimensions
                        int actdimensions = 0;
                        for (int d = 0; d < 6; d++)
                        {
                            if (MdimArrayElementCount[d] > 0)
                            {
                                actdimensions++;
                            }
                        }

                        string aname = "";
                        int n = 1;
                        uint id = 0;
                        uint[] xx = new uint[6] { 0, 0, 0, 0, 0, 0 };
                        do
                        {
                            aname = "[";
                            for (int j = actdimensions - 1; j >= 0; j--)
                            {
                                aname += (xx[j] + MdimArrayLowerBounds[j]).ToString();
                                if (j > 0)
                                {
                                    aname += ",";
                                }
                                else
                                {
                                    aname += "]";
                                }
                            }

                            if (vte.OffsetInfoType.HasRelation())
                            {
                                var arraynode = new Node
                                {
                                    NodeType = eNodeType.StructArray,
                                    Name = aname,
                                    Softdatatype = vte.Softdatatype,
                                    AccessId = id
                                };
                                subnode.Childs.Add(arraynode);

                                // All OffsetInfoTypes which occur at this point should have a Relation Id
                                var ioit2 = (IOffsetInfoType_Relation)vte.OffsetInfoType;

                                foreach (var ob in m_objs)
                                {
                                    if (ob.RelationId == ioit2.GetRelationId())
                                    {
                                        AddSubNodes(ref arraynode, ob);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                var arraynode = new Node
                                {
                                    NodeType = eNodeType.Array,
                                    Name = aname,
                                    Softdatatype = vte.Softdatatype,
                                    AccessId = id
                                };
                                subnode.Childs.Add(arraynode);
                            }
                            xx[0]++;
                            // BBOOL-Arrays on overflow the ID of the lowest array index goes only up to 8.
                            if (subnode.Softdatatype == Softdatatype.S7COMMP_SOFTDATATYPE_BBOOL && xx[0] >= MdimArrayElementCount[0])
                            {
                                if (MdimArrayElementCount[0] % 8 != 0)
                                {
                                    id += 8 - (xx[0] % 8);
                                }
                            }
                            for (int dim = 0; dim < 5; dim++)
                            {
                                if (xx[dim] >= MdimArrayElementCount[dim])
                                {
                                    xx[dim] = 0;
                                    xx[dim + 1]++;
                                }
                            }
                            id++;
                            n++;
                        } while (n <= ArrayElementCount);
                        #endregion
                    }
                    else if (vte.OffsetInfoType.HasRelation())
                    {
                        #region Struct / UDT / system library types (DTL, IEC_TIMER, ...) but not an array ...
                        var ioit = (IOffsetInfoType_Relation)vte.OffsetInfoType;

                        foreach (var ob in m_objs)
                        {
                            if (ob.RelationId == ioit.GetRelationId())
                            {
                                AddSubNodes(ref subnode, ob);
                                break;
                            }
                        }
                        // Empty areas are allowed, so don't return this as an error.
                        #endregion
                    }
                    element_index++;
                }
            }
        }

        private bool IsSoftdatatypeSupported(uint softdatatype)
        {
            switch (softdatatype)
            {
                case Softdatatype.S7COMMP_SOFTDATATYPE_BOOL:
                case Softdatatype.S7COMMP_SOFTDATATYPE_BYTE:
                case Softdatatype.S7COMMP_SOFTDATATYPE_CHAR:
                case Softdatatype.S7COMMP_SOFTDATATYPE_WORD:
                case Softdatatype.S7COMMP_SOFTDATATYPE_INT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_DWORD:
                case Softdatatype.S7COMMP_SOFTDATATYPE_DINT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_REAL:
                case Softdatatype.S7COMMP_SOFTDATATYPE_DATE:
                case Softdatatype.S7COMMP_SOFTDATATYPE_TIMEOFDAY:
                case Softdatatype.S7COMMP_SOFTDATATYPE_TIME:
                case Softdatatype.S7COMMP_SOFTDATATYPE_S5TIME:
                case Softdatatype.S7COMMP_SOFTDATATYPE_DATEANDTIME:
                case Softdatatype.S7COMMP_SOFTDATATYPE_STRING:
                case Softdatatype.S7COMMP_SOFTDATATYPE_POINTER:
                case Softdatatype.S7COMMP_SOFTDATATYPE_ANY:
                case Softdatatype.S7COMMP_SOFTDATATYPE_BLOCKFB:
                case Softdatatype.S7COMMP_SOFTDATATYPE_BLOCKFC:
                case Softdatatype.S7COMMP_SOFTDATATYPE_COUNTER:
                case Softdatatype.S7COMMP_SOFTDATATYPE_TIMER:
                case Softdatatype.S7COMMP_SOFTDATATYPE_BBOOL:
                case Softdatatype.S7COMMP_SOFTDATATYPE_LREAL:
                case Softdatatype.S7COMMP_SOFTDATATYPE_ULINT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_LINT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_LWORD:
                case Softdatatype.S7COMMP_SOFTDATATYPE_USINT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_UINT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_UDINT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_SINT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_WCHAR:
                case Softdatatype.S7COMMP_SOFTDATATYPE_WSTRING:
                case Softdatatype.S7COMMP_SOFTDATATYPE_LTIME:
                case Softdatatype.S7COMMP_SOFTDATATYPE_LTOD:
                case Softdatatype.S7COMMP_SOFTDATATYPE_LDT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_DTL:
                case Softdatatype.S7COMMP_SOFTDATATYPE_REMOTE:
                case Softdatatype.S7COMMP_SOFTDATATYPE_AOMIDENT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_EVENTANY:
                case Softdatatype.S7COMMP_SOFTDATATYPE_EVENTATT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_AOMAID:
                case Softdatatype.S7COMMP_SOFTDATATYPE_AOMLINK:
                case Softdatatype.S7COMMP_SOFTDATATYPE_EVENTHWINT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_HWANY:
                case Softdatatype.S7COMMP_SOFTDATATYPE_HWIOSYSTEM:
                case Softdatatype.S7COMMP_SOFTDATATYPE_HWDPMASTER:
                case Softdatatype.S7COMMP_SOFTDATATYPE_HWDEVICE:
                case Softdatatype.S7COMMP_SOFTDATATYPE_HWDPSLAVE:
                case Softdatatype.S7COMMP_SOFTDATATYPE_HWIO:
                case Softdatatype.S7COMMP_SOFTDATATYPE_HWMODULE:
                case Softdatatype.S7COMMP_SOFTDATATYPE_HWSUBMODULE:
                case Softdatatype.S7COMMP_SOFTDATATYPE_HWHSC:
                case Softdatatype.S7COMMP_SOFTDATATYPE_HWPWM:
                case Softdatatype.S7COMMP_SOFTDATATYPE_HWPTO:
                case Softdatatype.S7COMMP_SOFTDATATYPE_HWINTERFACE:
                case Softdatatype.S7COMMP_SOFTDATATYPE_HWIEPORT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_OBANY:
                case Softdatatype.S7COMMP_SOFTDATATYPE_OBDELAY:
                case Softdatatype.S7COMMP_SOFTDATATYPE_OBTOD:
                case Softdatatype.S7COMMP_SOFTDATATYPE_OBCYCLIC:
                case Softdatatype.S7COMMP_SOFTDATATYPE_OBATT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_CONNANY:
                case Softdatatype.S7COMMP_SOFTDATATYPE_CONNPRG:
                case Softdatatype.S7COMMP_SOFTDATATYPE_CONNOUC:
                case Softdatatype.S7COMMP_SOFTDATATYPE_CONNRID:
                case Softdatatype.S7COMMP_SOFTDATATYPE_PORT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_RTM:
                case Softdatatype.S7COMMP_SOFTDATATYPE_PIP:
                case Softdatatype.S7COMMP_SOFTDATATYPE_OBPCYCLE:
                case Softdatatype.S7COMMP_SOFTDATATYPE_OBHWINT:
                case Softdatatype.S7COMMP_SOFTDATATYPE_OBDIAG:
                case Softdatatype.S7COMMP_SOFTDATATYPE_OBTIMEERROR:
                case Softdatatype.S7COMMP_SOFTDATATYPE_OBSTARTUP:
                case Softdatatype.S7COMMP_SOFTDATATYPE_DBANY:
                case Softdatatype.S7COMMP_SOFTDATATYPE_DBWWW:
                case Softdatatype.S7COMMP_SOFTDATATYPE_DBDYN:
                    return true;
                default:
                    return false;
            }
        }

        protected class Node
        {
            public eNodeType NodeType;
            public string Name;
            public UInt32 AccessId;
            public UInt32 Softdatatype;
            public UInt32 RelationId;
            public List<Node> Childs = new List<Node>();

            public Node()
            {
                NodeType = eNodeType.Undefined;
            }
        }

        protected class VarRoot
        {
            public List<Node> Nodes;
        }
    }

    public class VarInfo
    {
        public string Name;
        public string AccessSequence;
        public UInt32 Softdatatype;
    }

    public enum eNodeType
    {
        Undefined = 0,
        Root,
        Var,
        Array,
        StructArray
    }
}
