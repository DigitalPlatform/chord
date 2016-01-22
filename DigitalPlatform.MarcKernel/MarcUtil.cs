using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

using DigitalPlatform.Xml;
using DigitalPlatform;
using DigitalPlatform.Text;
using System.Web;

namespace DigitalPlatform.Marc
{
    public enum ItemType
    {
        Filter = 0,
        Record = 1,
        Field = 2,
        Subfield = 3,
        Group = 4,
        Char = 5,
    }

    /// <summary>
    /// 一些有关MARC的实用函数
    /// </summary>
    public class MarcUtil
    {

        public const char FLDEND = (char)30;	// 字段结束符
        public const char RECEND = (char)29;	// 记录结束符
        public const char SUBFLD = (char)31;	// 子字段指示符

        public const int FLDNAME_LEN = 3;       // 字段名长度
        public const int MAX_MARCREC_LEN = 100000;   // MARC记录的最大长度




        // 包装以后的版本
        public static int Xml2Marc(string strXml,
            bool bWarning,
            string strMarcSyntax,
            out string strOutMarcSyntax,
            out string strMARC,
            out string strError)
        {
            // Debug.Assert(string.IsNullOrEmpty(strXml) == false, "");
            string strFragmentXml = "";
            return Xml2Marc(strXml,
                bWarning ? Xml2MarcStyle.Warning : Xml2MarcStyle.None,
                strMarcSyntax,
                out strOutMarcSyntax,
                out strMARC,
                out strFragmentXml,
                out strError);
        }

        [Flags]
        public enum Xml2MarcStyle
        {
            None = 0,
            Warning = 0x1,
            OutputFragmentXml = 0x02,
        }

        // 将MARCXML格式的xml记录转换为marc机内格式字符串
        // 注意，如果strXml内容为空，本函数会报错。最好在进入函数前进行判断。
        // parameters:
        //		bWarning	        ==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
        //		strMarcSyntax	    指示marc语法,如果==""，则自动识别
        //		strOutMarcSyntax	[out] 返回记录的 MARC 格式。如果 strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
        //      strFragmentXml      [out] 返回删除 <leader> <controlfield> <datafield> 以后的 XML 代码。注意，包含 <record> 元素
        public static int Xml2Marc(string strXml,
            Xml2MarcStyle style,
            string strMarcSyntax,
            out string strOutMarcSyntax,
            out string strMARC,
            out string strFragmentXml,
            out string strError)
        {
            strMARC = "";
            strError = "";
            strOutMarcSyntax = "";
            strFragmentXml = "";

            // 2013/9/25
            if (string.IsNullOrEmpty(strXml) == true)
                return 0;

            Debug.Assert(string.IsNullOrEmpty(strXml) == false, "");

            bool bWarning = (style & Xml2MarcStyle.Warning) != 0;
            bool bOutputFragmentXml = (style & Xml2MarcStyle.OutputFragmentXml) != 0;

            XmlDocument dom = new XmlDocument();
            dom.PreserveWhitespace = true;  // 在意空白符号
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "Xml2Marc() strXml 加载 XML 到 DOM 时出错: " + ex.Message;
                return -1;
            }

            // 取MARC根
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("unimarc", Ns.unimarcxml);
            nsmgr.AddNamespace("usmarc", Ns.usmarcxml);

            XmlNode root = null;
            if (string.IsNullOrEmpty(strMarcSyntax) == true)
            {
                // '//'保证了无论MARC的根在何处，都可以正常取出。
                root = dom.DocumentElement.SelectSingleNode("//unimarc:record", nsmgr);
                if (root == null)
                {
                    root = dom.DocumentElement.SelectSingleNode("//usmarc:record", nsmgr);

                    if (root == null)
                    {
                        // TODO: 是否要去除所有 MARC 相关元素
                        if (bOutputFragmentXml)
                            strFragmentXml = dom.DocumentElement.OuterXml;
                        return 0;
                    }

                    strMarcSyntax = "usmarc";
                }
                else
                {
                    strMarcSyntax = "unimarc";
                }
            }
            else
            {
                // 2012/1/8
                if (strMarcSyntax != null)
                    strMarcSyntax = strMarcSyntax.ToLower();

                if (strMarcSyntax != "unimarc"
                    && strMarcSyntax != "usmarc")
                {
                    strError = "无法识别 MARC格式 '" + strMarcSyntax + "' 。目前仅支持 unimarc 和 usmarc 两种格式";
                    return -1;
                }

                root = dom.DocumentElement.SelectSingleNode("//" + strMarcSyntax + ":record", nsmgr);
                if (root == null)
                {
                    // TODO: 是否要去除所有 MARC 相关元素
                    if (bOutputFragmentXml)
                        strFragmentXml = dom.DocumentElement.OuterXml;
                    return 0;
                }
            }

            StringBuilder strMarc = new StringBuilder(4096);

            strOutMarcSyntax = strMarcSyntax;

            XmlNode leader = root.SelectSingleNode(strMarcSyntax + ":leader", nsmgr);
            if (leader == null)
            {
                strError += "缺<" + strMarcSyntax + ":leader>元素\r\n";
                if (bWarning == false)
                    return -1;
                else
                    strMarc.Append("012345678901234567890123");
            }
            else // 正常情况
            {
                // string strLeader = DomUtil.GetNodeText(leader);
                // GetNodeText()会自动Trim()，会导致头标区内容末尾丢失字符
                string strLeader = leader.InnerText;
                if (strLeader.Length != 24)
                {
                    strError += "<" + strMarcSyntax + ":leader>元素内容应为24字符\r\n";
                    if (bWarning == false)
                        return -1;
                    else
                    {
                        if (strLeader.Length < 24)
                            strLeader = strLeader.PadRight(24, ' ');
                        else
                            strLeader = strLeader.Substring(0, 24);
                    }

                }

                strMarc.Append(strLeader);

                // 从 DOM 中删除 leader 元素
                if (bOutputFragmentXml)
                    leader.ParentNode.RemoveChild(leader);
            }

            int i = 0;

            // 固定长字段
            XmlNodeList controlfields = root.SelectNodes(strMarcSyntax + ":controlfield", nsmgr);
            for (i = 0; i < controlfields.Count; i++)
            {
                XmlNode field = controlfields[i];
                string strTag = DomUtil.GetAttr(field, "tag");
                if (strTag.Length != 3)
                {
                    strError += "<" + strMarcSyntax + ":controlfield>元素的tag属性值'" + strTag + "'应当为3字符\r\n";
                    if (bWarning == false)
                        return -1;
                    else
                    {
                        if (strTag.Length < 3)
                            strTag = strTag.PadRight(3, '*');
                        else
                            strTag = strTag.Substring(0, 3);
                    }
                }

                string strContent = DomUtil.GetNodeText(field);

                strMarc.Append(strTag + strContent + new string(MarcUtil.FLDEND, 1));

                // 从 DOM 中删除
                if (bOutputFragmentXml)
                    field.ParentNode.RemoveChild(field);
            }

            // 可变长字段
            XmlNodeList datafields = root.SelectNodes(strMarcSyntax + ":datafield", nsmgr);
            for (i = 0; i < datafields.Count; i++)
            {
                XmlNode field = datafields[i];
                string strTag = DomUtil.GetAttr(field, "tag");
                if (strTag.Length != 3)
                {
                    strError += "<" + strMarcSyntax + ":datafield>元素的tag属性值'" + strTag + "'应当为3字符\r\n";
                    if (bWarning == false)
                        return -1;
                    else
                    {
                        if (strTag.Length < 3)
                            strTag = strTag.PadRight(3, '*');
                        else
                            strTag = strTag.Substring(0, 3);
                    }
                }

                string strInd1 = DomUtil.GetAttr(field, "ind1");
                if (strInd1.Length != 1)
                {
                    strError += "<" + strMarcSyntax + ":datalfield>元素的ind1属性值'" + strInd1 + "'应当为1字符\r\n";
                    if (bWarning == false)
                        return -1;
                    else
                    {
                        if (strInd1.Length < 1)
                            strInd1 = '*'.ToString();
                        else
                            strInd1 = strInd1[0].ToString();
                    }
                }

                string strInd2 = DomUtil.GetAttr(field, "ind2");
                if (strInd2.Length != 1)
                {
                    strError += "<" + strMarcSyntax + ":datalfield>元素的indi2属性值'" + strInd2 + "'应当为1字符\r\n";
                    if (bWarning == false)
                        return -1;
                    else
                    {
                        if (strInd2.Length < 1)
                            strInd2 = '*'.ToString();
                        else
                            strInd2 = strInd2[0].ToString();
                    }
                }

                // string strContent = DomUtil.GetNodeText(field);
                XmlNodeList subfields = field.SelectNodes(strMarcSyntax + ":subfield", nsmgr);
                StringBuilder strContent = new StringBuilder(4096);
                for (int j = 0; j < subfields.Count; j++)
                {
                    XmlNode subfield = subfields[j];

                    XmlAttribute attr = subfield.Attributes["code"];
#if NO
					string strCode = DomUtil.GetAttr(subfield, "code");
					if (strCode.Length != 1)
					{
						strError += "<"+strMarcSyntax+":subfield>元素的code属性值'"+strCode+"'应当为1字符\r\n";
						if (bWarning == false)
							return -1;
						else 
						{
							if (strCode.Length < 1)
								strCode = '*'.ToString();
							else
								strCode = strCode[0].ToString();
						}
					}

                    string strSubfieldContent = DomUtil.GetNodeText(subfield);

					strContent += new string(MarcUtil.SUBFLD,1) + strCode + strSubfieldContent;

#endif
                    if (attr == null)
                    {
                        // 前导纯文本
                        strContent.Append(DomUtil.GetNodeText(subfield));
                        continue;   //  goto CONTINUE; BUG!!!
                    }

                    string strCode = attr.Value;
                    if (strCode.Length != 1)
                    {
                        strError += "<" + strMarcSyntax + ":subfield>元素的 code 属性值 '" + strCode + "' 应当为1字符\r\n";
                        if (bWarning == false)
                            return -1;
                        else
                        {
                            if (strCode.Length < 1)
                                strCode = "";   // '*'.ToString();
                            else
                                strCode = strCode[0].ToString();
                        }
                    }

                    string strSubfieldContent = DomUtil.GetNodeText(subfield);
                    strContent.Append(new string(MarcUtil.SUBFLD, 1) + strCode + strSubfieldContent);
                }

                strMarc.Append(strTag + strInd1 + strInd2 + strContent + new string(MarcUtil.FLDEND, 1));

            CONTINUE:
                // 从 DOM 中删除
                if (bOutputFragmentXml)
                    field.ParentNode.RemoveChild(field);
            }

            strMARC = strMarc.ToString();
            if (bOutputFragmentXml)
                strFragmentXml = dom.DocumentElement.OuterXml;
            return 0;
        }

    }


#if NO
	/// <summary>
	/// byte数组，可以动态扩展
	/// </summary>
	public class TempByteArray : ArrayList
	{
		public void AddRange(byte[] baSource)
		{
			for(int i=0;i<baSource.Length;i++) 
			{
				this.Add(baSource[i]);
			}
		}

		public int AddRange(byte[] baSource,
			int nStart,
			int nLength)
		{
			int nCount = 0;
			for(int i=nStart;i<baSource.Length && i<nStart+nLength;i++) 
			{
				this.Add(baSource[i]);
				nCount ++;
			}

			return nCount;
		}
			
		public byte[] GetByteArray()
		{
			byte[] result = new byte[this.Count];

			for(int i=0;i<this.Count;i++) 
			{
				result[i] = (byte)this[i];
			}

			return result;
		}
	}

#endif
    /// <summary>
    /// byte数组，可以动态扩展
    /// </summary>
    public class MyByteList : List<byte>
    {
        public MyByteList()
            : base()
        {
        }

        public MyByteList(int capacity)
            : base(capacity)
        {
        }

        public void AddRange(byte[] baSource)
        {
            base.AddRange(baSource);
        }

        public int AddRange(byte[] baSource,
            int nStart,
            int nLength)
        {
            int nCount = 0;
            for (int i = nStart; i < baSource.Length && i < nStart + nLength; i++)
            {
                this.Add(baSource[i]);
                nCount++;
            }

            return nCount;
        }

        public byte[] GetByteArray()
        {
            byte[] result = new byte[this.Count];
            base.CopyTo(result);
            return result;
        }
    }

}

