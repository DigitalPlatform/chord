using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DigitalPlatform.Z3950
{
    //
    /*
<item name = "ISSN" value=8 uni_name = "Identifier-ISSN" />
     * */
    public class Bib1Use
    {
        public string Name = "";
        public string Value = "";
        public string UniName = "";
        public string Comment = "";
    }

    public class FromCollection : List<Bib1Use>
    {
        public string GetValue(string strName)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Name.Trim() == strName.Trim())
                    return this[i].Value;
            }

            return null;    // not found
        }

        // 装载检索途径信息
        public Result Load(string strFileName)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                return new Result { Value = -1, ErrorInfo = "装载文件 " + strFileName + " 到XMLDOM时出错: " + ex.Message };
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("item");
            foreach (XmlElement node in nodes)
            {
                Bib1Use from = new Bib1Use
                {
                    Name = node.GetAttribute("name"),
                    UniName = node.GetAttribute("uni_name"),
                    Value = node.GetAttribute("value"),
                    Comment = node.GetAttribute("comment")
                };

                this.Add(from);
            }

            return new Result();
        }

        // 获得检索途径列表
        public string[] GetDropDownList()
        {
            string[] result = new string[this.Count];

            for (int i = 0; i < this.Count; i++)
            {
                Bib1Use from = this[i];
                result[i] = from.Name + " - " + from.Comment;
            }

            return result;
        }
    }
}
