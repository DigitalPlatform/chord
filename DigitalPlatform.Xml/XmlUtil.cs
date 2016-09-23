using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace DigitalPlatform.Xml
{
    /// <summary>
    /// 扩展 XML 功能
    /// </summary>
    public static class XmlUtil
    {
        // 不会抛出异常。如果 XML 格式有误，则返回原始字符串
        public static string TryGetIndentXml(string strXml,
            bool bHasProlog = false)
        {
            try
            {
                return GetIndentXml(strXml, bHasProlog);
            }
            catch
            {
                return strXml;
            }
        }

        // parameters:
        //      bHasProlog  是否要产生 prolog 行
        // exception:
        //      XmlDocument.LoadXml() 要抛出的那些异常
        public static string GetIndentXml(string strXml,
            bool bHasProlog = false)
        {
            if (string.IsNullOrEmpty(strXml) == true)
                return strXml;

            XmlDocument dom = new XmlDocument();
            // 可能抛出异常
            dom.LoadXml(strXml);

            if (bHasProlog == true)
                return GetIndentXml(dom);
            else
                return GetIndentXml(dom.DocumentElement);
        }

        // 获得缩进格式的 XML 源代码
        public static string GetIndentXml(XmlNode node, int indentation = 4)
        {
            using (MemoryStream m = new MemoryStream())
            using (XmlTextWriter w = new XmlTextWriter(m, Encoding.UTF8))
            {
                w.Formatting = Formatting.Indented;
                w.Indentation = indentation;
                node.WriteTo(w);
                w.Flush();

                m.Seek(0, SeekOrigin.Begin);

                using (StreamReader sr = new StreamReader(m, Encoding.UTF8))
                {
                    string strText = sr.ReadToEnd();
                    return strText;
                }
                // 注意，此后 m 已经关闭
            }
        }
    }
}
