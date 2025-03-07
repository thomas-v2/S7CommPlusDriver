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
using System.Linq;
using System.Text;

namespace S7CommPlusDriver
{
    public partial class S7CommPlusConnection
    {
        public enum BlockType
        {
            unkown,
            DB,
            FB,
            FC,
            OB,
        }

        public enum ProgrammingLanguage : int
        {
            Undef = 0,
            STL = 1,
            LAD_CLASSIC = 2,
            FBD_CLASSIC = 3,
            SCL = 4,
            DB = 5,
            GRAPH = 6,
            SDB = 7,
            CPU_DB = 8,
            CPU_SDB = 17,
            CforS7 = 21,
            HIGRAPH = 22,
            CFC = 23,
            SFC = 24,
            S7_PDIAG = 26,
            RSE = 29,
            F_STL = 31,
            F_LAD = 32,
            F_FBD = 33,
            F_DB = 34,
            F_CALL = 35,
            TechnoDB = 37,
            F_LAD_LIB = 38,
            F_FBD_LIB = 39,
            ClassicEncryption = 41,
            FCP = 50,
            LAD_IEC = 100,
            FBD_IEC = 101,
            FLD = 102,
            UDT = 150,
            SDT = 151,
            FBT = 152,
            CB = 160,
            ST = 161,
            AX_CODE = 190,
            AX_DATA = 191,
            BMC_200 = 200,
            Motion_DB = 201,
            BMC_202 = 202,
            BMC_203 = 203,
            BMC_204 = 204,
            BMC_205 = 205,
            BMC_206 = 206,
            BMC_207 = 207,
            BMC_208 = 208,
            BMC_209 = 209,
            GRAPH_ACTIONS = 300,
            GRAPH_SEQUENCE = 301,
            GRAPH_ADDINFOS = 303,
            GRAPH_PLUS = 310,
            MC7plus = 400,
            ProDiag = 500,
            ProDiag_OB = 501,
            CEM = 600,
        }

        public enum BinaryArtifacts : uint
        {
            Undefined = 0u,
            PlcFamily = 2147483649u,
            PlcMc7plusData = 2147483650u,
            PlcOptimizationInfoData = 2147483651u,
            PlcClosedImmediateData = 2147483652u,
            SimulatorFamily = 2147483665u,
            SimulatorMc7plusData = 2147483666u,
            SimulatorOptimizationInfoData = 2147483667u,
            SimulatorClosedImmediateData = 2147483668u,
            VirtualPlcFamilyKey = 2147483681u,
            VirtualPlcMc7plusDataKey = 2147483682u,
            VirtualPlcOptimizationInfoDataKey = 2147483683u,
            VirtualPlcClosedImmediateDataKey = 2147483684u
        }

        public class BlockInfo
        {
            public string name;                                          // Name of the datablock
            public UInt32 number;                                        // Number of the datablock
            public BlockType type;
            public ProgrammingLanguage lang;
            public UInt32 db_block_relid;                                   // RID of the datablock
            public UInt32 db_block_ti_relid;                                // Type-Info RID of the datablock
        };

        public int BrowseAllBlocks(out List<BlockInfo> exploreData)
        {
            int res;
            Browser vars = new Browser();
            ExploreRequest exploreReq;
            ExploreResponse exploreRes;

            #region Read all objects

            exploreData = new List<BlockInfo>();

            exploreReq = new ExploreRequest(ProtocolVersion.V2);
            exploreReq.ExploreId = Ids.NativeObjects_thePLCProgram_Rid;
            exploreReq.ExploreRequestId = Ids.None;
            exploreReq.ExploreChildsRecursive = 1;
            exploreReq.ExploreParents = 0;

            // We want to know the following attributes
            exploreReq.AddressList.Add(Ids.ObjectVariableTypeName);
            exploreReq.AddressList.Add(Ids.Block_BlockNumber);
            exploreReq.AddressList.Add(Ids.Block_BlockLanguage);

            res = SendS7plusFunctionObject(exploreReq);
            if (res != 0)
            {
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                return m_LastError;
            }

            exploreRes = ExploreResponse.DeserializeFromPdu(m_ReceivedPDU, true);
            res = checkResponseWithIntegrity(exploreReq, exploreRes);
            if (res != 0)
            {
                return res;
            }

            #endregion

            #region Evaluate all data blocks that then need to be browsed

            var obj = exploreRes.Objects.First(o => o.ClassId == Ids.PLCProgram_Class_Rid);

            foreach (var ob in obj.GetObjects())
            {
                switch (ob.ClassId)
                {
                    case Ids.DB_Class_Rid:
                    case Ids.FB_Class_Rid:
                    case Ids.FC_Class_Rid:
                    case Ids.OB_Class_Rid:
                        UInt32 relid = ob.RelationId;
                        UInt32 area = (relid >> 16);
                        UInt32 num = relid & 0xffff;

                        var name = (ValueWString)(ob.GetAttribute(Ids.ObjectVariableTypeName));
                        var data = new BlockInfo();
                        data.db_block_relid = relid;
                        data.name = name.GetValue();
                        data.number = num;
                        data.type = ob.ClassId switch
                        {
                            Ids.DB_Class_Rid => BlockType.DB,
                            Ids.FB_Class_Rid => BlockType.FB,
                            Ids.FC_Class_Rid => BlockType.FC,
                            Ids.OB_Class_Rid => BlockType.OB,
                        };

                        var lang = ((ValueUInt)ob.Attributes[Ids.Block_BlockLanguage]).GetValue();
                        data.lang = (ProgrammingLanguage) lang;
                        exploreData.Add(data);
                        break;
                }
            }

            #endregion

            return 0;
        }

        public int GetBlockXml(uint relid, out string xml_linecomment, out Dictionary<uint, string> xml_comment, out string interfaceDescription, out string[] blockBody, out string fuctionalObjectCode, out string[] intRef, out string[] extRef)
        {
            int res;
            // With requesting DataInterface_InterfaceDescription, whe would be able to get all informations like the access ids and
            // datatype informations, that we get from the other browsing method. Needs to be tested which one is more efficient on network traffic or plc load.
            // If we keep use browsing for the comments, at least we would be able to read all information in one request.
            xml_linecomment = String.Empty;
            xml_comment = new();
            interfaceDescription = String.Empty;
            blockBody = new string[0];
            fuctionalObjectCode = String.Empty;
            intRef = new string[0];
            extRef = new string[0];

            var exploreReq = new ExploreRequest(ProtocolVersion.V2);
            exploreReq.ExploreId = relid;
            exploreReq.ExploreRequestId = Ids.None;
            exploreReq.ExploreChildsRecursive = 1;
            exploreReq.ExploreParents = 0;

            // We want to know the following attributes
            exploreReq.AddressList.Add(Ids.ASObjectES_Comment);
            exploreReq.AddressList.Add(Ids.DataInterface_LineComments);
            exploreReq.AddressList.Add(Ids.DataInterface_InterfaceDescription);
            exploreReq.AddressList.Add(Ids.Block_BodyDescription);
            exploreReq.AddressList.Add(Ids.FunctionalObject_extRefData);
            exploreReq.AddressList.Add(Ids.FunctionalObject_intRefData);

            res = SendS7plusFunctionObject(exploreReq);
            if (res != 0)
            {
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                return m_LastError;
            }

            var exploreRes = ExploreResponse.DeserializeFromPdu(m_ReceivedPDU, true);
            res = checkResponseWithIntegrity(exploreReq, exploreRes);
            if (res != 0)
            {
                return res;
            }

            foreach (var obj in exploreRes.Objects)
            {
                foreach (var att in obj.Attributes)
                {
                    switch (att.Key)
                    {
                        case Ids.FunctionalObject_extRefData:
                            {
                                var xx = (ValueBlobSparseArray)att.Value;
                                BlobDecompressor bd3 = new BlobDecompressor();
                                var blob_sp3 = xx.GetValue();
                                extRef = new string[blob_sp3.Count];
                                var i = 0;
                                foreach (var key in blob_sp3.Keys)
                                {
                                    if (blob_sp3[key].value != null)
                                        extRef[i++] = bd3.decompress(blob_sp3[key].value, 4);
                                }
                                break;
                            }
                        case Ids.FunctionalObject_intRefData:
                            {
                                var xx = (ValueBlobSparseArray)att.Value;
                                BlobDecompressor bd3 = new BlobDecompressor();
                                var blob_sp3 = xx.GetValue();
                                intRef = new string[blob_sp3.Count];
                                var i = 0;
                                foreach (var key in blob_sp3.Keys)
                                {
                                    if (blob_sp3[key].value != null)
                                        intRef[i++] = bd3.decompress(blob_sp3[key].value, 4);
                                }
                                break;
                            }
                        case Ids.ASObjectES_Comment:
                            {
                                var att_comment = (ValueWStringSparseArray)att.Value;
                                xml_comment = att_comment.GetValue();
                                break;
                            }
                        case Ids.DataInterface_LineComments:
                            {
                                var att_linecomment = (ValueBlobSparseArray)att.Value;
                                BlobDecompressor bd = new BlobDecompressor();
                                var blob_sp = att_linecomment.GetValue();
                                // In DBs we get the data with Sparsearray key = 1, in M-Area with key = 2.
                                // For now, just take the first, don't know where the key ids are for.
                                foreach (var key in blob_sp.Keys)
                                {
                                    xml_linecomment = bd.decompress(blob_sp[key].value, 4); // Offset of 4, as we have a header for the zlib dictionary version
                                    break;
                                }
                                break;
                            }
                        case Ids.DataInterface_InterfaceDescription:
                            {
                                var att_ifsescr = (ValueBlob)att.Value;
                                BlobDecompressor bd2 = new BlobDecompressor();
                                var blob_sp2 = att_ifsescr.GetValue();
                                interfaceDescription = bd2.decompress(blob_sp2, 4); // Offset of 4, as we have a header for the zlib dictionary version
                                break;
                            }
                        case Ids.Block_BodyDescription:
                            {
                                var xx = (ValueBlobSparseArray)att.Value;
                                BlobDecompressor bd3 = new BlobDecompressor();
                                var blob_sp3 = xx.GetValue();
                                blockBody = new string[blob_sp3.Where(x => x.Key < (uint)BinaryArtifacts.PlcFamily).Count()];
                                var i = 0;
                                foreach (var key in blob_sp3.Keys.Order())
                                {
                                    if (!(key < (uint)BinaryArtifacts.PlcFamily))
                                    {
                                        //TODO: what to do with binary artifacts?
                                        var binaryArtifactType = (BinaryArtifacts)key;
                                        continue;
                                    }

                                    if (blob_sp3[key].value != null)
                                    {
                                        var code = bd3.decompress(blob_sp3[key].value, 4);
                                        blockBody[i++] = code;
                                    }
                                }
                                break;
                            }
                    }
                }
            }
            return 0;
        }

        public int RunExploreRequest(uint relid, uint[] attributes, out List<PObject> objects, byte exploreChildsRecursive = 1, byte exploreParents = 0)
        {
            int res;

            objects = null;

            var exploreReq = new ExploreRequest(ProtocolVersion.V2);
            exploreReq.ExploreId = relid;
            exploreReq.ExploreRequestId = Ids.None;
            exploreReq.ExploreChildsRecursive = exploreChildsRecursive;
            exploreReq.ExploreParents = exploreParents;

            if (attributes != null && attributes.Length > 0)
            {
                // We want to know the following attributes
                exploreReq.AddressList.AddRange(attributes);
            }

            res = SendS7plusFunctionObject(exploreReq);
            if (res != 0)
            {
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                return m_LastError;
            }

            var exploreRes = ExploreResponse.DeserializeFromPdu(m_ReceivedPDU, true);
            res = checkResponseWithIntegrity(exploreReq, exploreRes);
            if (res != 0)
            {
                return res;
            }

            objects = exploreRes.Objects;

            return 0;
        }
    }
}
