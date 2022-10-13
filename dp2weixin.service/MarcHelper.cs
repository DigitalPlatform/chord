using DigitalPlatform.Marc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common
{
    public class MarcHelper
    {

        // 根据字段规则，从marc抽取字段
        public static string GetFields(string strMarcWorksheet, string strFieldMap)
        {
            List<FieldItem> fieldList = MarcHelper.ParseFieldMap(strFieldMap);

            // 抽取字段规则
            MarcHelper.GetFields(strMarcWorksheet, ref fieldList);

            string info = "";
            foreach (FieldItem field in fieldList)
            {
                if (string.IsNullOrEmpty(info) == false)
                {
                    info += "\r\n";
                }
                info += field.Dump();
            }
            return info;
        }

        // 根据字段规则，从marc抽取字段
        public static void GetFields(string strMarcWorksheet, ref List<FieldItem> fieldList)
        {
            MarcRecord record = MarcRecord.FromWorksheet(strMarcWorksheet); //new MarcRecord(strMarc);
            foreach (FieldItem field in fieldList)
            {
                // 设置字段的值
                string value = record.select("field[@name='" + field.Field + "']/subfield[@name='" + field.Subfield + "']").FirstContent;
                field.Value = value;
            }
        }

        // 把字段设到Marc上，并返回更新后的marc
        //public static MarcRecord SetFields(string strMarcWorksheet, List<FieldItem> list)

        public static MarcRecord SetFields(MarcRecord record, List<FieldItem> list)
        {

            //MarcRecord record = MarcRecord.FromWorksheet(strMarcWorksheet); //new MarcRecord(strMarc);
            foreach (FieldItem field in list)
            {
                MarcNodeList fields = record.select("field[@name='" + field.Field + "']");
                if (fields.count > 0)
                {
                    MarcNodeList subfields = fields[0].select("subfield[@name='" + field.Subfield + "']");
                    if (subfields.count > 0)
                        subfields[0].Content = field.Value;  //只更改第一个子字段
                    else
                        fields[0].ChildNodes.insertSequence(new MarcSubfield(field.Subfield, field.Value)); //不存在时，新增一个子字段

                }
                else
                    record.ChildNodes.insertSequence(new MarcField('$', field.Field + "  $" + field.Subfield + field.Value));
            }
            return record;

            //return record.ToWorksheet(); //转换成工作单格式  //.Text;  //完整内容

        }

        public static MarcRecord SetFields(MarcRecord record, string strFieldMap)
        {
            List<FieldItem> fieldList = MarcHelper.ParseFieldMap(strFieldMap);
            return SetFields(record, fieldList);
        }

        public static MarcRecord MarcXml2MarcRecord(string strMarcXml,
out string strOutMarcSyntax,
out string strError)
        {
            MarcRecord record = null;

            strError = "";
            strOutMarcSyntax = "";

            string strMARC = "";
            int nRet = MarcUtil.Xml2Marc(strMarcXml,
                false,
                "",
                out strOutMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == 0)
                record = new MarcRecord(strMARC);
            else
                strError = "MarcXml转换错误:" + strError;

            return record;
        }


        // 解析字段规则字符串，变成一个字段配置数组。
        public static List<FieldItem> ParseFieldMap(string fieldMap)
        {
            List<FieldItem> list = new List<FieldItem>();

            fieldMap = fieldMap.Replace("\r\n", "\n");
            string[] fields = fieldMap.Split(new char[] { '\n' });
            foreach (string one in fields)
            {
                if (one.Trim() == "")
                    continue;

                string caption = "";
                string field = "";
                string subfield = "";
                string value = "";



                int index = one.IndexOf('|');
                if (index != -1)
                {
                    caption = one.Substring(0, index);
                    string right = one.Substring(index + 1);
                    string tempField = right;

                    index = right.IndexOf('|');
                    if (index != -1)
                    {
                        tempField = right.Substring(0, index);
                        value = right.Substring(index + 1);
                    }

                    index = tempField.IndexOf("$");
                    if (index != -1)
                    {
                        field = tempField.Substring(0, index);
                        subfield = tempField.Substring(index + 1);
                    }
                }

                if (string.IsNullOrEmpty(caption) == true
                    || string.IsNullOrEmpty(field) == true
                    || string.IsNullOrEmpty(subfield) == true)
                {
                    throw new Exception("此行字段抽取规则[" + one + "]配置的不合法，应为[caption|field$subfield]格式。");
                }


                FieldItem item = new FieldItem(caption, field, subfield, value);
                list.Add(item);
            }

            return list;
        }



        public string getmarc()
        {

            return "";

            /*
            string strTitle = this.textBox__SetBiblioInfo_title.Text;
            string strAuthor = this.textBox__SetBiblioInfo_author.Text;

            MarcRecord record = new MarcRecord();
            record.add(new MarcField('$', "200  $a" + strTitle));
            record.add(new MarcField('$', "690  $aI247.5"));
            record.add(new MarcField('$', "701  $a" + strAuthor));
            string strMARC = record.Text;

            string strMarcSyntax = "unimarc";
            string strXml = "";
            int nRet = MarcUtil.Marc2Xml(strMARC,
                strMarcSyntax,
                out strXml,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
                //return -1;
            }
            */
        }


    }

    public class FieldItem
    {

        public FieldItem(string caption, string field, string subfield)
        {
            this.Caption = caption;
            this.Field = field;
            this.Subfield = subfield;
        }

        public FieldItem(string caption, string field, string subfield, string value)
        {
            this.Caption = caption;
            this.Field = field;
            this.Subfield = subfield;
            this.Value = value;
        }

        public string Caption { get; set; }

        public string Field { get; set; }

        public string Subfield { get; set; }


        public string Value { get; set; }


        public string Dump()
        {
            string result = Caption + "|" + Field + "$" + Subfield;
            if (string.IsNullOrEmpty(Value) == false)
                result += "|" + Value;

            return result;

        }
    }
}
