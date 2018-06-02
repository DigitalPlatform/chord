using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalPlatform.Z3950
{
    // 检索目标信息结构
    public class TargetInfo
    {
        //public TreeNode ServerNode = null;  // 相关的server类型节点
        //public TreeNode StartNode = null;   // 发起检索的节点。可以不是server类型节点

        public string HostName = "";
        public int Port = 210;

        public string[] DbNames = null;

        public string UserName = "";
        public string Password = "";
        public string GroupID = "";
        public int AuthenticationMethod = 0;

        public string PreferredRecordSyntax = BerTree.MARC_SYNTAX;  // 可以有--部分。使用时候小心，用GetLeftValue()获得干净的值
        public string DefaultResultSetName = "default";

        public string DefaultElementSetName = "F -- Full"; // 可以有--部分。使用时候小心，用GetLeftValue()获得干净的值

        public int PresentPerBatchCount = 10;   // 每批数量

        public bool ConvertEACC = true;
        public bool FirstFull = true;
        public bool DetectMarcSyntax = true;
        public bool IgnoreReferenceID = false;
        public bool IsbnForce13 = false;
        public bool IsbnForce10 = false;
        public bool IsbnAddHyphen = false;
        public bool IsbnRemoveHyphen = false;
        public bool IsbnWild = false;

        public Encoding DefaultRecordsEncoding = Encoding.GetEncoding(936);
        public Encoding DefaultQueryTermEncoding = Encoding.GetEncoding(936);

        public RecordSyntaxAndEncodingBindingCollection Bindings = null;

        public bool CharNegoUTF8 = true;
        public bool CharNegoRecordsUTF8 = true;

        public string UnionCatalogBindingDp2ServerName = "";
        public string UnionCatalogBindingUcServerUrl = "";

        bool m_bChanged = false;

#if NO
        // 树上显示的名字
        public string Name
        {
            get
            {
                // 2007/8/3
                if (this.StartNode != null)
                {
                    // Debug.Assert(this.ServerNode == this.StartNode.Parent, ""); // 2007/11/2 BUG

                    if (ZTargetControl.IsDatabaseType(this.StartNode) == true)
                        return this.StartNode.Text + "." + this.StartNode.Parent.Text;
                }

                if (this.ServerNode == null)
                    return "";

                return this.ServerNode.Text;
            }
        }
#endif

#if NO
        // 服务器名。树节点上显示的名字，不包含括号部分
        public string ServerName
        {
            get
            {
                if (this.ServerNode == null)
                    return "";
                TreeNodeInfo info = (TreeNodeInfo)this.ServerNode.Tag;
                if (info == null)
                    return "";
                return info.Name;
            }
        }
#endif

        public string HostNameAndPort
        {
            get
            {
                return this.HostName + ":" + this.Port.ToString();
            }
        }

#if NO
        public void OnlineServerIcon(bool bOnline)
        {
            int nImageIndex = ZTargetControl.TYPE_SERVER_ONLINE;

            if (bOnline == false)
                nImageIndex = ZTargetControl.TYPE_SERVER_OFFLINE;

            if (this.ServerNode != null)
            {
                if (this.ServerNode.TreeView.InvokeRequired == true)
                {
                    ZTargetControl.Delegate_SetNodeImageIndex d = new ZTargetControl.Delegate_SetNodeImageIndex(ZTargetControl.SetNodeImageIndex);
                    this.ServerNode.TreeView.Invoke(d, new object[] { this.ServerNode, nImageIndex });
                }
                else
                {
                    this.ServerNode.ImageIndex = nImageIndex;
                    this.ServerNode.SelectedImageIndex = nImageIndex;
                }
            }
        }
#endif
        // 内容是否发生过修改
        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                this.m_bChanged = value;
            }
        }

#if NO
        // 用于判断对象唯一性的名字
        public string QualifiedName
        {
            get
            {
                string strDbNameList = "";
                if (this.DbNames != null)
                {
                    strDbNameList = string.Join(",", this.DbNames);
                }

                string strTreePath = "";

                if (this.StartNode != null)
                    strTreePath = this.StartNode.FullPath;
                else if (this.ServerNode != null)
                    strTreePath = this.ServerNode.FullPath;

                return this.HostName + ":" + this.Port.ToString() + ";treepath=" + strTreePath + ";dbnames=" + strDbNameList;
            }
        }

#endif
    }

    // 绑定信息元素
    public class RecordSyntaxAndEncodingBindingItem
    {
        public string RecordSyntaxOID = "";
        public string RecordSyntaxComment = "";
        public string EncodingName = "";
        public string EncodingNameComment = "";

        // 将 "value -- comment" 形态的字符串拆分为"value"和"comment"两个部分
        public static void ParseValueAndComment(string strText,
            out string strValue,
            out string strComment)
        {
            int nRet = strText.IndexOf("--");
            if (nRet == -1)
            {
                strValue = strText.Trim();
                strComment = "";
                return;
            }

            strValue = strText.Substring(0, nRet).Trim();
            strComment = strText.Substring(nRet + 2).Trim();
        }

        public string RecordSyntax
        {
            get
            {
                if (String.IsNullOrEmpty(this.RecordSyntaxComment) == true)
                    return this.RecordSyntaxOID;

                return this.RecordSyntaxOID + " -- " + this.RecordSyntaxComment;
            }
            set
            {
                string strValue = "";
                string strComment = "";

                ParseValueAndComment(value, out strValue, out strComment);
                this.RecordSyntaxOID = strValue;
                this.RecordSyntaxComment = strComment;
            }
        }

        public string Encoding
        {
            get
            {
                if (String.IsNullOrEmpty(this.EncodingNameComment) == true)
                    return this.EncodingName;
                return this.EncodingName + " -- " + this.EncodingNameComment;
            }
            set
            {
                string strValue = "";
                string strComment = "";

                ParseValueAndComment(value, out strValue, out strComment);
                this.EncodingName = strValue;
                this.EncodingNameComment = strComment;
            }
        }
    }

    // 绑定信息数组
    public class RecordSyntaxAndEncodingBindingCollection : List<RecordSyntaxAndEncodingBindingItem>
    {
        // parameters:
        //      strBindingString    格式为"syntaxoid1 -- syntaxcomment1|encodingname1 -- encodingcomment1||syntaxoid2 -- syntaxcomment2|encodingname2 -- encodingcomment2"，末尾可能有多余的“||”
        public void Load(string strBindingString)
        {
            this.Clear();

            string[] lines = strBindingString.Split(new string[] { "||" },
                StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string strSyntax = "";
                string strEncoding = "";
                string strLine = lines[i].Trim();
                if (String.IsNullOrEmpty(strLine) == true)
                    continue;
                int nRet = strLine.IndexOf('|');
                if (nRet != -1)
                {
                    strSyntax = strLine.Substring(0, nRet).Trim();
                    strEncoding = strLine.Substring(nRet + 1).Trim();
                }
                else
                {
                    strSyntax = strLine;
                    strEncoding = "";
                }

                RecordSyntaxAndEncodingBindingItem item = new RecordSyntaxAndEncodingBindingItem();
                item.RecordSyntax = strSyntax;
                item.Encoding = strEncoding;

                this.Add(item);
            }
        }

        // 返还为字符串形态
        public string GetString()
        {
            string strResult = "";
            for (int i = 0; i < this.Count; i++)
            {
                RecordSyntaxAndEncodingBindingItem item = this[i];
                strResult += item.RecordSyntax + "|" + item.Encoding + "||";
            }

            return strResult;
        }

        public string GetEncodingName(string strRecordSyntaxOID)
        {
            for (int i = 0; i < this.Count; i++)
            {
                RecordSyntaxAndEncodingBindingItem item = this[i];

                if (item.RecordSyntaxOID == strRecordSyntaxOID)
                    return item.EncodingName;
            }

            return null;    // not found
        }

    }

}
