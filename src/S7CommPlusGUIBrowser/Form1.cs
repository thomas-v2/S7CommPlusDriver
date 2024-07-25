using S7CommPlusDriver;
using S7CommPlusDriver.ClientApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace S7CommPlusGUIBrowser
{
    public partial class Form1 : Form
    {

        private S7CommPlusConnection conn = null;

        public Form1()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();
            // Als Parameter lässt sich die IP-Adresse übergeben, sonst Default-Wert von oben
            if (args.Length >= 1)
            {
                tbIpAddress.Text = args[1];
            }
            // Als Parameter lässt sich das Passwort übergeben, sonst Default-Wert von oben (kein Passwort)
            if (args.Length >= 2)
            {
                tbPassword.Text = args[2];
            }
        }

        private void setStatus(string status)
        {
            lbStatus.Text = status;
            lbStatus.Refresh();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            setStatus("connecting...");

            if (conn != null) conn.Disconnect();
            conn = new S7CommPlusConnection();
            int res = conn.Connect(tbIpAddress.Text, tbPassword.Text);
            if (res != 0)
            {
                setStatus("error");
                return;
            }
            setStatus("connected");

            treeView1.Nodes.Clear();
            setStatus("loading...");
            List<S7CommPlusConnection.DatablockInfo> dbInfoList;
            res = conn.GetListOfDatablocks(out dbInfoList);
            if (res != 0)
            {
                setStatus("error");
                return;
            }
            TreeNode tn;
            foreach (S7CommPlusConnection.DatablockInfo dbInfo in dbInfoList)
            {
                tn = treeView1.Nodes.Add(dbInfo.db_name);
                tn.Nodes.Add("Loading...");
                tn.Tag = dbInfo.db_block_ti_relid;
            }
            //Inputs
            tn = treeView1.Nodes.Add("Inputs");
            tn.Nodes.Add("Loading...");
            tn.Tag = 0x90010000;
            //Outputs
            tn = treeView1.Nodes.Add("Outputs");
            tn.Nodes.Add("Loading...");
            tn.Tag = 0x90020000;
            //Merker
            tn = treeView1.Nodes.Add("Merker");
            tn.Nodes.Add("Loading...");
            tn.Tag = 0x90030000;

            setStatus("connected");
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            setStatus("disconnecting...");

            if (conn != null) conn.Disconnect();
            conn = null;
            treeView1.Nodes.Clear();

            setStatus("disconnected");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (conn != null) conn.Disconnect();
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Nodes.Count < 0 || e.Node.Nodes[0].Text != "Loading...") return;

            setStatus("loading...");
            e.Node.Nodes.Clear();
            uint relTiId = (uint)e.Node.Tag;
            PObject pObj = conn.getTypeInfoByRelId(relTiId);
            setStatus("connected");

            if (pObj == null || pObj.VarnameList == null) return;
            TreeNode tn;
            TreeNode tnarr;
            for (int i = 0; i < pObj.VarnameList.Names.Count; ++i)
            {
                tn = e.Node.Nodes.Add(pObj.VarnameList.Names[i]);
                if (pObj.VartypeList.Elements[i].OffsetInfoType.Is1Dim())
                {
                    var ioitarr = (IOffsetInfoType_1Dim)pObj.VartypeList.Elements[i].OffsetInfoType;
                    uint arrayElementCount = ioitarr.GetArrayElementCount();
                    int arrayLowerBounds = ioitarr.GetArrayLowerBounds();
                    for (int j = 0; j < arrayElementCount; ++j)
                    {
                        tnarr = tn.Nodes.Add(pObj.VarnameList.Names[i] + "[" + (j + arrayLowerBounds) + "]");
                        if (pObj.VartypeList.Elements[i].OffsetInfoType.HasRelation())
                        {
                            var ioit = (IOffsetInfoType_Relation)pObj.VartypeList.Elements[i].OffsetInfoType;
                            tnarr.Nodes.Add("Loading...");
                            tnarr.Tag = ioit.GetRelationId();
                        }
                    }
                    tn.Tag = (uint)0; //is array
                }
                else if (pObj.VartypeList.Elements[i].OffsetInfoType.IsMDim())
                {
                    var ioitarrm = (IOffsetInfoType_MDim)pObj.VartypeList.Elements[i].OffsetInfoType;
                    uint[] MdimArrayElementCount = ioitarrm.GetMdimArrayElementCount();
                    int[] MdimArrayLowerBounds = ioitarrm.GetMdimArrayLowerBounds();
                    int dimCount = MdimArrayElementCount.Aggregate(0, (acc, act) => acc += (act > 0) ? 1 : 0);
                    int[] indexes = new int[dimCount];
                    bool stop = false;
                    while (!stop)
                    {
                        string arrIdxStr = "";
                        for (int j = dimCount - 1; j >= 0; --j)
                        {
                            arrIdxStr += (arrIdxStr != "" ? "," : "") + (indexes[j] + MdimArrayLowerBounds[j]);
                        }
                        tnarr = tn.Nodes.Add(pObj.VarnameList.Names[i] + "[" + arrIdxStr + "]");
                        if (pObj.VartypeList.Elements[i].OffsetInfoType.HasRelation())
                        {
                            var ioit = (IOffsetInfoType_Relation)pObj.VartypeList.Elements[i].OffsetInfoType;
                            tnarr.Nodes.Add("Loading...");
                            tnarr.Tag = ioit.GetRelationId();
                        }
                        ++indexes[0];
                        for (int j = 0; j < dimCount; ++j)
                        {
                            if (indexes[j] >= MdimArrayElementCount[j])
                            {
                                if (j + 1 < dimCount)
                                {
                                    indexes[j] = 0;
                                    ++indexes[j + 1];
                                }
                                else
                                {
                                    stop = true;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    tn.Tag = (uint)0; //is array
                }
                else
                {
                    if (pObj.VartypeList.Elements[i].OffsetInfoType.HasRelation())
                    {
                        var ioit = (IOffsetInfoType_Relation)pObj.VartypeList.Elements[i].OffsetInfoType;
                        tn.Nodes.Add("Loading...");
                        tn.Tag = ioit.GetRelationId();
                    }
                }

            }

        }

        private string escapeTiaString(string str, bool isRootNode, bool isArray)
        {
            if (isRootNode) return '"' + str + '"';
            Regex re = new Regex("(^[0-9]|[^0-9A-Za-z_])");
            if (isArray)
            {
                Regex reArr = new Regex("^([^\"]*)(\\[[0-9, ]+\\])$");
                Match m = reArr.Match(str);
                if (!m.Success) return str;
                if (re.Match(m.Groups[1].Value).Success) return '"' + m.Groups[1].Value + '"' + m.Groups[2].Value;
                return str;
            }
            if (re.IsMatch(str)) return '"' + str + '"';
            return str;
        }

        private void readTagBySymbol()
        {
            tbValue.Text = "";
            tbSymbolicAddress.Text = "";

            setStatus("loading...");
            PlcTag tag = conn.getPlcTagBySymbol(tbSymbol.Text);
            setStatus("connected");
            if (tag == null) return;

            tbSymbolicAddress.Text = tag.Address.GetAccessString();

            PlcTags tags = new PlcTags();
            tags.AddTag(tag);
            if (tags.ReadTags(conn) != 0) return;
            tbValue.Text = tag.ToString();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null) return; //has relId

            string name = "";
            TreeNode tn = e.Node;
            while (tn != null)
            {
                bool isArray = false;
                string nodeText = tn.Text;
                tn = tn.Parent;
                if (tn != null && tn.Tag != null)
                { //is array
                    if ((uint)tn.Tag == 0)
                    {
                        isArray = true;
                        tn = tn.Parent; //skip array parent
                    }
                }
                if (tn != null && tn.Tag != null)
                { //dont add in/out/merker area as tag
                    uint relId = (uint)tn.Tag;
                    if (relId == 0x90010000 || relId == 0x90020000 || relId == 0x90030000) tn = null;
                }
                name = escapeTiaString(nodeText, tn == null, isArray) + (name != "" ? "." : "") + name;
            }
            tbSymbol.Text = name;


            readTagBySymbol();
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            if (tbSymbol.Text == "") return;

            try
            {
                readTagBySymbol();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }
    }
}
