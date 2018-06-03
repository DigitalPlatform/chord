using System;
using System.Xml;
using System.Diagnostics;
using System.Text;

using DigitalPlatform.Xml;
using System.Web;
using System.Collections.Generic;

namespace DigitalPlatform.Marc
{
    /// <summary>
    /// 一些有关MARC的实用函数
    /// </summary>
    public class MarcUtil
    {
        public const char FLDEND = (char)30;    // 字段结束符
        public const char RECEND = (char)29;    // 记录结束符
        public const char SUBFLD = (char)31;    // 子字段指示符






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

        // 获得 MARC 记录的 HTML 格式字符串
        public static string GetHtmlOfMarc(string strMARC,
            string strFragmentXml,
            string strCoverImageFragment,
            bool bSubfieldReturn)
        {
            StringBuilder strResult = new StringBuilder("\r\n<table class='marc'>", 4096);

            MarcRecord record = new MarcRecord(strMARC);
            // 
            strResult.Append("\r\n<tr class='header'><td class='fieldname'></td>"
                + "<td class='indicator'></td>"
                + "<td class='content'>" + record.Header.ToString() + "</td></tr>");

            MarcNodeList fields = record.select("field");
            int i = 0;
            foreach (MarcField field in fields)
            {
                string strField = field.Content;

                string strLineClass = "datafield";
                string strFieldName = field.Name;
                string strIndicatior = field.Indicator;
                string strContent = "";

                if (field.IsControlField)
                {
                    strLineClass = "controlfield";
                    strField = strField.Replace(' ', '_');
                }

                strIndicatior = strIndicatior.Replace(' ', '_');

#if NO
                if (i != 0)
                {
                    // 取字段名
                    if (strField.Length < 3)
                    {
                        strFieldName = strField;
                        strField = "";
                    }
                    else
                    {
                        strFieldName = strField.Substring(0, 3);
                        strField = strField.Substring(3);
                    }

                    // 取指示符
                    if (IsControlFieldName(strFieldName) == true)
                    {
                        strLineClass = "controlfield";
                        strField = strField.Replace(' ', '_');
                    }
                    else
                    {
                        if (strField.Length < 2)
                        {
                            strIndicatior = strField;
                            strField = "";
                        }
                        else
                        {
                            strIndicatior = strField.Substring(0, 2);
                            strField = strField.Substring(2);
                        }
                        strIndicatior = strIndicatior.Replace(' ', '_');

                        strLineClass = "datafield";

                        // 1XX字段有定长内容
                        if (strFieldName.Length >= 1 && strFieldName[0] == '1')
                        {
                            strField = strField.Replace(' ', '_');
                            strLineClass += " fixedlengthsubfield";
                        }
                    }
                }
                else
                {
                    strLineClass = "header";
                    strField = strField.Replace(' ', '_');
                }

#endif

                strContent = GetHtmlFieldContent(strField,
                    bSubfieldReturn);

                // 
                strResult.Append("\r\n<tr class='" + strLineClass + "'><td class='fieldname'>" + strFieldName + "</td>"
                    + "<td class='indicator'>" + strIndicatior + "</td>"
                    + "<td class='content'>" + strContent + "</td></tr>");

                if (i == 0)
                    strResult.Append(GetImageHtml(strCoverImageFragment));

                i++;
            }

            if (string.IsNullOrEmpty(strFragmentXml) == false)
            {
                strResult.Append(GetFragmentHtml(strFragmentXml));
            }

            strResult.Append("\r\n</table>");
            return strResult.ToString();
        }

        public static string GetHtmlFieldContent(string strContent,
    bool bSubfieldReturn)
        {
            const string SubFieldChar = "‡";
            const string FieldEndChar = "¶";

            StringBuilder result = new StringBuilder(4096);
            for (int i = 0; i < strContent.Length; i++)
            {
                char ch = strContent[i];
                if (ch == (char)31)
                {
                    if (result.Length > 0)
                    {
                        if (bSubfieldReturn == true)
                            result.Append("<br/>");
                        else
                            result.Append(" "); // 为了显示时候可以折行
                    }

                    result.Append("<span class='subfield'>");
                    result.Append(SubFieldChar);
                    if (i < strContent.Length - 1)
                    {
                        result.Append(strContent[i + 1]);
                        i++;
                    }
                    else
                        result.Append(SubFieldChar);

                    result.Append("</span>");
                    continue;
                }
                result.Append(ch);
            }

            result.Append("<span class='fieldend'>" + FieldEndChar + "</span>");

            return result.ToString();
        }

        static string GetFragmentHtml(string strFragmentXml)
        {
            if (string.IsNullOrEmpty(strFragmentXml) == true)
                return "";

            strFragmentXml = DomUtil.GetIndentInnerXml(strFragmentXml);    // 不包含根节点
            return GetPlanTextHtml(strFragmentXml);
        }

        static string GetImageHtml(string strImageFragment)
        {
            if (string.IsNullOrEmpty(strImageFragment) == true)
                return "";

            string strLineClass = "datafield";
            StringBuilder strResult = new StringBuilder(4096);

            strResult.Append("\r\n<tr class='" + strLineClass + "'>");

            strResult.Append("\r\n<td class='content' colspan='3'>"    //  
                + strImageFragment
                + "</td>");

            strResult.Append("\r\n</tr>");

            return strResult.ToString();
        }


        static string GetPlanTextHtml(string strOldFragmentXml)
        {
            string strLineClass = "datafield";
            StringBuilder strResult = new StringBuilder(4096);

            strResult.Append("\r\n<tr class='" + strLineClass + "'>");

            // 
            string[] lines = HttpUtility.HtmlEncode(strOldFragmentXml).Replace("\r\n", "\n").Split(new char[] { '\n' });
            StringBuilder result = new StringBuilder(4096);
            foreach (string line in lines)
            {
                if (result.Length > 0)
                    result.Append("<br/>");
                result.Append(ReplaceLeadingTab(line));
            }

            strResult.Append("\r\n<td class='content' colspan='3'>" + result + "</td>");

            strResult.Append("\r\n</tr>");

            return strResult.ToString();
        }

        // 将前方连续的若干空格字符替换为 &nbsp;
        public static string ReplaceLeadingTab(string strText)
        {
            StringBuilder result = new StringBuilder(4096);
            int i = 0;
            foreach (char c in strText)
            {
                if (c == ' ')
                    result.Append("&nbsp;");
                else
                {
                    result.Append(strText.Substring(i));
                    break;
                }

                i++;
            }

            return result.ToString();
        }

        #region ISO2709 --> 机内格式

        // 把byte[]类型的MARC记录转换为机内格式
        // return:
        //		-2	MARC格式错
        //		-1	一般错误
        //		0	正常
        public static int ConvertByteArrayToMarcRecord(byte[] baRecord,
            Encoding encoding,
            bool bForce,
            out string strMarc,
            out string strError)
        {
            strError = "";
            strMarc = "";
            int nRet = 0;

            bool bUcs2 = false;

            if (encoding.Equals(Encoding.Unicode) == true)
                bUcs2 = true;

            List<byte[]> aField = new List<byte[]>();
            if (bForce == true
                || bUcs2 == true)
            {
                nRet = MarcUtil.ForceCvt2709ToFieldArray(
                    ref encoding,
                    baRecord,
                    out aField,
                    out strError);

                Debug.Assert(nRet != -2, "");

                /*
                if (bUcs2 == true)
                {
                    // 转换后，编码方式已经变为UTF8
                    Debug.Assert(encoding.Equals(Encoding.UTF8), "");
                }
                 * */
            }
            else
            {
                //???
                nRet = MarcUtil.Cvt2709ToFieldArray(
                    encoding,
                    baRecord,
                    out aField,
                    out strError);
            }

            if (nRet == -1)
                return -1;

            if (nRet == -2)  //marc出错
                return -2;

            string[] saField = null;
            GetMarcRecordString(aField,
                encoding,
                out saField);

            if (saField.Length > 0)
            {
                string strHeader = saField[0];

                if (strHeader.Length > 24)
                    strHeader = strHeader.Substring(0, 24);
                else
                    strHeader = saField[0].PadRight(24, '*');

                StringBuilder text = new StringBuilder(1024);
                text.Append(strHeader);
                for (int i = 1; i < saField.Length; i++)
                {
                    text.Append(saField[i] + new string(FLDEND, 1));
                }

                strMarc = text.ToString().Replace("\r", "*").Replace("\n", "*");    // 2012/3/16
                return 0;
            }

            return 0;
        }


        // 将ISO2709格式记录转换为字段数组
        // aResult的每个元素为byte[]型，内容是一个字段。第一个元素是头标区，一定是24bytes
        // return:
        //	-1	一般性错误
        //	-2	MARC格式错误
        public static int Cvt2709ToFieldArray(
            Encoding encoding,  // 2007/7/11
            byte[] s,
            out List<byte[]> aResult,   // out
            out string strErrorInfo)
        {
            strErrorInfo = "";
            aResult = new List<byte[]>();

            // const char *sopp;
            int maxbytes = 2000000;	// 约2000K，防止攻击

            // const byte RECEND = 29;
            // const byte FLDEND = 30;
            // const byte SUBFLD = 31;

            if (encoding.Equals(Encoding.Unicode) == true)
                throw new Exception("UCS2编码方式应当使用 ForceCvt2709ToFieldArray()，而不是 Cvt2709ToFieldArray()");

            MarcHeaderStruct header = new MarcHeaderStruct(encoding, s);

            {
                // 输出头标区
                byte[] tarray = null;
                tarray = new byte[24];
                Array.Copy(s, 0, tarray, 0, 24);

                // 2014/5/9
                // 防范头标区出现 0 字符
                for (int j = 0; j < tarray.Length; j++)
                {
                    if (tarray[j] == 0)
                        tarray[j] = (byte)'*';
                }

                aResult.Add(tarray);
            }

            int somaxlen;
            int reclen, baseaddr, lenoffld, startposoffld;
            int len, startpos;
            // char *dirp;
            int offs = 0;
            int t = 0;
            int i;
            // char temp[30];

            somaxlen = s.Length;
            try
            {
                reclen = header.RecLength;
            }
            catch (FormatException ex)
            {
                strErrorInfo = "头标区开始5字符 '" + header.RecLengthString + "' 不是纯数字 :" + ex.Message;
                // throw(new MarcException(strErrorInfo));
                goto ERROR2;
            }
            if (reclen > somaxlen)
            {
                strErrorInfo = "头标区头5字符表示的记录长度"
                    + Convert.ToString(reclen)
                    + "大于源缓冲区整个内容的长度"
                    + Convert.ToString(somaxlen);
                goto ERROR2;
            }
            if (reclen < 24)
            {
                strErrorInfo = "头标区头5字符表示的记录长度"
                    + Convert.ToString(reclen)
                    + "小于24";
                goto ERROR2;
            }

            if (s[reclen - 1] != RECEND)
            {
                strErrorInfo = "头标区声称的结束位置不是MARC记录结束符";
                goto ERROR2;  // 结束符不正确
            }

            for (i = 0; i < reclen - 1; i++)
            {
                if (s[i] == RECEND)
                {
                    strErrorInfo = "记录内容中不能有记录结束符";
                    goto ERROR2;
                }
            }

            try
            {
                baseaddr = header.BaseAddress;
            }
            catch (FormatException ex)
            {
                strErrorInfo = "头标区数据基地址5字符 '" + header.BaseAddressString + " '不是纯数字 :" + ex.Message;
                //throw(new MarcException(strErrorInfo));
                goto ERROR2;
            }

            if (baseaddr > somaxlen)
            {
                strErrorInfo = "数据基地址值 "
                    + Convert.ToString(baseaddr)
                    + " 已经超出源缓冲区整个内容的长度 "
                    + Convert.ToString(somaxlen);
                goto ERROR2;
            }
            if (baseaddr <= 24)
            {
                strErrorInfo = "数据基地址值 "
                    + Convert.ToString(baseaddr)
                    + " 小于24";
                goto ERROR2;  // 数据基地址太小
            }
            if (s[baseaddr - 1] != FLDEND)
            {
                strErrorInfo = "没有在目次区尾部位置" + Convert.ToString(baseaddr) + "找到FLDEND符号";
                goto ERROR2;  // 
            }

            try
            {
                lenoffld = header.WidthOfFieldLength;
            }
            catch (FormatException ex)
            {
                strErrorInfo = "头标区目次区字段长度1字符 '" + header.WidthOfFieldLengthString + " '不是纯数字 :" + ex.Message;
                //throw(new MarcException(strErrorInfo));
                goto ERROR2;
            }

            try
            {
                startposoffld = header.WidthOfStartPositionOfField;
            }
            catch (FormatException ex)
            {
                strErrorInfo = "头标区目次区字段起始位置1字符 '" + header.WidthOfStartPositionOfFieldString + " '不是纯数字 :" + ex.Message;
                // throw(new MarcException(strErrorInfo));
                goto ERROR2;
            }


            if (lenoffld <= 0 || lenoffld > 30)
            {
                strErrorInfo = "目次区中字段长度值占用字符数 "
                    + Convert.ToString(lenoffld)
                    + " 不正确，应在1和29之间...";
                goto ERROR2;
            }

            if (lenoffld != 4)
            {	// 2001/5/15
                strErrorInfo = "目次区中字段长度值占用字符数 "
                    + Convert.ToString(lenoffld)
                    + " 不正确，应为4...";
                goto ERROR2;
            }

            lenoffld = 4;
            if (startposoffld <= 0 || startposoffld > 30)
            {
                strErrorInfo = "目次区中字段起始位置值占用字符数 "
                    + Convert.ToString(startposoffld)
                    + " 不正确，应在1到29之间...";
                goto ERROR2;
            }

            startposoffld = 5;

            // 开始处理目次区
            // dirp = (char *)sopp;
            t = 24;
            offs = 24;
            MyByteList baField = null;
            for (i = 0; ; i++)
            {
                if (s[offs] == FLDEND)
                    break;  // 目次区结束

                // 将字段名装入目标
                if (offs + 3 >= baseaddr)
                    break;
                if (t + 3 >= maxbytes)
                    break;
                /*
                baTarget.SetSize(t+3, CHUNK_SIZE);
                memcpy((char *)baTarget.GetData()+t,
                    dirp+offs,
                    3);
                t+=3;
                */
                baField = new MyByteList();
                baField.AddRange(s, offs, 3);
                t += 3;


                // 得到字段长度
                offs += 3;
                if (offs + lenoffld >= baseaddr)
                    break;
                len = MarcHeaderStruct.IntValue(s, offs, lenoffld);

                // 得到字段内容开始地址
                offs += lenoffld;
                if (offs + startposoffld >= baseaddr)
                    break;
                startpos = MarcHeaderStruct.IntValue(s, offs, startposoffld);

                offs += startposoffld;
                if (offs >= baseaddr)
                    break;

                // 将字段内容装入目标
                if (t + len >= maxbytes)
                    break;
                if (s[baseaddr + startpos - 1] != FLDEND)
                {
                    // errnoiso2709 = ERROR_BADFLDCONTENT;
                    strErrorInfo = "缺乏字段结束符";
                    goto ERROR2;
                }

                if (s[baseaddr + startpos + len - 1] != FLDEND)
                {
                    //errnoiso2709 = ERROR_BADFLDCONTENT;
                    strErrorInfo = "缺乏字段结束符";
                    goto ERROR2;
                }

                /*
                baTarget.SetSize(t+len, CHUNK_SIZE);
                memcpy((char *)baTarget.GetData()+t,
                    sopp+baseaddr+startpos,
                    len);
                t += len;
                */
                baField.AddRange(s, baseaddr + startpos, len == 0 ? len : len - 1);
                t += len;

                aResult.Add(baField.GetByteArray());
                baField = null;
            }

            if (t + 1 >= maxbytes)
            {
                // errnoiso2709 = ERROR_TARGETBUFFEROVERFLOW;
                strErrorInfo = "记录太大";
                goto ERROR2;  // 目标空间不够
            }

            /*
            baField.Add((char)RECEND);
            t ++;
            */

            /*
            baTarget.SetSize(t+1, CHUNK_SIZE);
            *((char *)baTarget.GetData() + t++) = RECEND;
            if (t+1>=maxbytes) 
            {
                errnoiso2709 = ERROR_TARGETBUFFEROVERFLOW;
                goto ERROR1;  // 目标空间不够
            }
            */

            Debug.Assert(t != -2, "");
            return t;
            //ERROR1:
            //	return -1;	// 一般性错误
            ERROR2:
            // 调试用
            Debug.Assert(false, "");
            return -2;	// MARC格式错误
        }

        // 强制将ISO2709格式记录转换为字段数组
        // 本函数采用的算法是将目次区的地址和长度忽略，只取3字符的字段名
        // aResult的每个元素为byte[]型，内容是一个字段。第一个元素是头标区，一定是24bytes
        // return:
        //	-1	一般性错误
        //	-2	MARC格式错误
        public static int ForceCvt2709ToFieldArray(
            ref Encoding encoding,  // 2007/7/11 函数内可能发生变化
            byte[] s,
            out List<byte[]> aResult,
            out string strErrorInfo)
        {
            strErrorInfo = "";
            aResult = new List<byte[]>();

            List<MyByteList> results = new List<MyByteList>();

            bool bUcs2 = false;
            if (encoding.Equals(Encoding.Unicode) == true)
                bUcs2 = true;

            if (bUcs2 == true)
            {
                string strRecord = encoding.GetString(s);

                // 变换成UTF-8编码方式处理
                s = Encoding.UTF8.GetBytes(strRecord);
                encoding = Encoding.UTF8;
            }

            MarcHeaderStruct header = null;
            try
            {
                header = new MarcHeaderStruct(encoding, s);
            }
            catch (ArgumentException)
            {
                // 不足 24 字符的，给与宽容
                header = new MarcHeaderStruct(Encoding.ASCII, Encoding.ASCII.GetBytes("012345678901234567890123"));
            }
            header.ForceUNIMARCHeader();	// 强制将某些位置设置为缺省值

            results.Add(header.GetByteList());

            int somaxlen;
            int offs;
            int i, j;

            somaxlen = s.Length;

            // 开始处理目次区
            offs = 24;
            MyByteList baField = null;
            bool bFound = false;
            for (i = 0; ; i++)
            {
                bFound = false;
                for (j = offs; j < offs + 3 + 4 + 5; j++)
                {
                    if (j >= somaxlen)
                        break;
                    if (s[j] == FLDEND)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (j >= somaxlen)
                {
                    offs = j;
                    break;
                }

                if (bFound == true)
                {
                    if (j <= offs + 3)
                    {
                        offs = j + 1;
                        break;
                    }
                }


                // 将字段名装入目标
                baField = new MyByteList();
                baField.AddRange(s, offs, 3);

                results.Add(baField);
                baField = null;
                // 得到字段内容开始地址
                offs += 3;
                offs += 4;
                offs += 5;

                if (bFound == true)
                {
                    offs = j + 1;
                    break;
                }

            }

            if (offs >= somaxlen)
                return 0;

            int nFieldNumber = 1;
            baField = null;
            // 加入对应的字段内容
            for (; offs < somaxlen; offs++)
            {
                byte c = s[offs];
                if (c == RECEND)
                    break;
                if (c == FLDEND)
                {
                    nFieldNumber++;
                    baField = null;
                }
                else
                {
                    if (baField == null)
                    {
                        // 确保下标不越界
                        while (nFieldNumber >= results.Count)
                        {
                            MyByteList temp = new MyByteList();
                            temp.Add((byte)'?');
                            temp.Add((byte)'?');
                            temp.Add((byte)'?');
                            results.Add(temp);
                        }
                        baField = results[nFieldNumber];
                    }

                    baField.Add(c);
                }
            }

            aResult = new List<byte[]>();
            foreach (MyByteList list in results)
            {
                aResult.Add(list.GetByteArray());
            }

            return 0;
            //		ERROR1:
            //			return -1;	// 一般性错误
            //		ERROR2:
            //			return -2;	// MARC格式错误
        }


        // 把 [byte []] 变换为 string []
        // aSourceField:	MARC字段数组。注意ArrayList每个元素要求为byte[]类型
        static int GetMarcRecordString(List<byte[]> aSourceField,
            Encoding encoding,
            out string[] saTarget)
        {
            saTarget = new string[aSourceField.Count];
            for (int j = 0; j < aSourceField.Count; j++)
            {
                saTarget[j] = encoding.GetString((byte[])aSourceField[j]);
            }

            return 0;
        }

#endregion
    }




}

