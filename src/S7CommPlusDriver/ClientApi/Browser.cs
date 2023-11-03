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
    // Diese Klasse verwaltet übergeordnete Funktionen.
    // Sie baut aus der Antwort zu einem ExploreRequest mit dem root node den kompletten Variablenhaushalt zusammen.
    // ----DB100
    //        +--Int
    //        +--Real
    //        +--Struct
    //              +---Int
    // ----MArea
    //        +--Int
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
            // Liste aufbauen
            foreach (var node in m_Root.Nodes)
            {
                // Ggf. leere Listen in einem Bereich wie Merker oder Timer nicht verarbeiten. Würde
                // sonst zu einem Eintrag mit nur dem root-node führen.
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
                    // TODO: Hier Besonderheit: Zwischen Array-Index und Zugriffs-ID steht noch eine 1.
                    // Unklar ob die 1 fix ist, oder sich aus einem weiteren Wert ergibt.
                    accessIds += "." + String.Format("{0:X}", node.AccessId + ".1");
                    break;
                default:
                    names += "." + node.Name;
                    accessIds += "." + String.Format("{0:X}", node.AccessId);
                    break;
            }
            
            if (node.Childs.Count == 0)
            {
                // Am Blatt des Baums angekommen
                var info = new VarInfo
                {
                    Name = names,
                    AccessSequence = accessIds,
                    Softdatatype = node.Softdatatype
                };
                m_varInfoList.Add(info);
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
            // Sind in einem Bereich überhaupt keine Variablen vorhanden, dann ist diese Liste auch nicht vorhanden.
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
                    // Arrays verarbeiten
                    // TODO: Besonderheiten der Übersichtlichkeit wegen in eigene Methoden auslagern.

                    if (vte.OffsetInfoType.Is1Dim())
                    {
                        #region Struct/UDT oder flaches Array mit einer Dimension

                        var ioit = (IOffsetInfoType_1Dim)vte.OffsetInfoType;
                        ArrayElementCount = ioit.GetArrayElementCount();
                        ArrayLowerBounds = ioit.GetArrayLowerBounds();

                        // Die Zugriffs-ID beginnt hier immer bei 0, unabhängig von Lowerbounds
                        for (uint i = 0; i < ArrayElementCount; i++)
                        {
                            // Struct Array gesondert behandeln/kennzeichnen. Hier ist noch eine zusätzliche ID hinter Array Index
                            // und Zugriffs-LID vorhanden.

                            if (vte.Softdatatype == Softdatatype.S7COMMP_SOFTDATATYPE_STRUCT)
                            {
                                var arraynode = new Node
                                {
                                    NodeType = eNodeType.StructArray,
                                    Name = "[" + (i + ArrayLowerBounds) + "]",
                                    Softdatatype = vte.Softdatatype,
                                    AccessId = i
                                };
                                subnode.Childs.Add(arraynode);

                                // Alle OffsetInfoTypes die hier kommen, sollten eine Relation Id besitzen.
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
                        #region Struct/UDT oder flaches Array mehr als einer Dimension

                        var ioit = (IOffsetInfoType_MDim)vte.OffsetInfoType;
                        ArrayElementCount = ioit.GetArrayElementCount();
                        ArrayLowerBounds = ioit.GetArrayLowerBounds();
                        MdimArrayElementCount = ioit.GetMdimArrayElementCount();
                        MdimArrayLowerBounds = ioit.GetMdimArrayLowerBounds();

                        // Feststellen wie viele Dimensionen das Array besitzt
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

                            if (vte.Softdatatype == Softdatatype.S7COMMP_SOFTDATATYPE_STRUCT)
                            {
                                var arraynode = new Node
                                {
                                    NodeType = eNodeType.StructArray,
                                    Name = aname,
                                    Softdatatype = vte.Softdatatype,
                                    AccessId = id
                                };
                                subnode.Childs.Add(arraynode);

                                // Alle OffsetInfoTypes die hier kommen, sollten eine Relation Id besitzen.
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
                            // Bei BBOOL-Arrays wird die ID bei Überlauf des kleinsten Array-Index immer auf ein Vielfaches von 8 gerundet.
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
                    if (vte.OffsetInfoType.HasRelation())
                    {
                        #region Struct / UDT / System-Biliotheks-Strukturen (DTL, IEC_TIMER, ...) aber keine Art von Array
                        var ioit = (IOffsetInfoType_Relation)vte.OffsetInfoType;

                        foreach (var ob in m_objs)
                        {
                            if (ob.RelationId == ioit.GetRelationId())
                            {
                                AddSubNodes(ref subnode, ob);
                                break;
                            }
                        }
                        // Es kann durchaus vorkommen, dass kein Eintrag gefunden wird, z.B. wenn ein Bereich wie Merker oder
                        // Ausgänge leer ist. Darum nicht weiter als Fehler auswerten.
                        #endregion
                    }
                    element_index++;
                }
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
