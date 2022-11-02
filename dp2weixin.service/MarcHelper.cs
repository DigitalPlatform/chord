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
        public static string GetFields(MarcRecord record, string strFieldMap)
        {
            List<FieldItem> fieldList = MarcHelper.ParseFieldMap(strFieldMap);

            // 抽取字段规则
            MarcHelper.GetFields(record, ref fieldList);

            string info = "";
            foreach (FieldItem field in fieldList)
            {
                if (string.IsNullOrEmpty(info) == false)
                {
                    info += "\r\n";
                }
                info += field.dump();
            }
            return info;
        }

        // 根据字段规则，从marc抽取字段
        public static void GetFields(MarcRecord record, ref List<FieldItem> fieldList)
        {
            //MarcRecord record = MarcRecord.FromWorksheet(strMarcWorksheet); //new MarcRecord(strMarc);
            foreach (FieldItem field in fieldList)
            {
                if (field.name == "###")
                {
                    if (record.Header!= null)  // 2022/11/2 record.Header有可能为null，就是没有头标区的情况。
                        field.value = record.Header.ToString();

                    continue;
                }

                if (string.IsNullOrEmpty(field.subfield) == false)
                {
                    // 设置字段的值
                    string value = record.select("field[@name='" + field.field + "']/subfield[@name='" + field.subfield + "']").FirstContent;
                    field.value = value;
                }
                else
                {
                    // 设置字段的值
                    string value = record.select("field[@name='" + field.field + "']").FirstContent;
                    field.value = value;
                }
            }
        }

        // 把字段设到Marc上，并返回更新后的marc
        //public static MarcRecord SetFields(string strMarcWorksheet, List<FieldItem> list)

        public static MarcRecord SetFields(MarcRecord record, List<FieldItem> list)
        {

            //MarcRecord record = MarcRecord.FromWorksheet(strMarcWorksheet); //new MarcRecord(strMarc);
            foreach (FieldItem field in list)
            {
                try
                {
                    if (field.name == "###")
                    {
                        //头标区
                        record.Header[0, 24] = field.value;//.Header;
                                                           //record.Header = field.value;
                        continue;
                    }

                    // 子字段的情况
                    if (string.IsNullOrEmpty(field.subfield) == false)
                    {
                        MarcNodeList fields = record.select("field[@name='" + field.field + "']");
                        if (fields.count > 0)
                        {
                            MarcNodeList subfields = fields[0].select("subfield[@name='" + field.subfield + "']");
                            if (subfields.count > 0)
                                subfields[0].Content = field.value;  //只更改第一个子字段
                            else
                                fields[0].ChildNodes.insertSequence(new MarcSubfield(field.subfield, field.value)); //不存在时，新增一个子字段

                        }
                        else
                            record.ChildNodes.insertSequence(new MarcField('$', field.field + "  $" + field.subfield + field.value));
                    }
                    else  //字段的情况
                    {
                        MarcNodeList fields = record.select("field[@name='" + field.field + "']");
                        if (fields.count > 0)
                        {
                            fields[0].Content = field.value;
                        }
                        else
                        {
                            record.ChildNodes.insertSequence(new MarcField(field.field + field.value));  //中间没有2位"  " 

                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("设置'"+field.lable+ field.name+"'字段的出错："+ex.Message);
                }

            }

            // 对字段排序
            record.ChildNodes.sort((a, b) => {
                return string.Compare(a.Name, b.Name);
            });

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
                strError = "MarcUtil.Xml2Marc()返回错误:" + strError;

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
                string name = "";
                //string field = "";
                //string subfield = "";
                string value = "";



                int index = one.IndexOf('|');
                if (index != -1)
                {
                    caption = one.Substring(0, index);
                    string right = one.Substring(index + 1);
                    name = right;

                    index = right.IndexOf('|');
                    if (index != -1)
                    {
                        name = right.Substring(0, index);
                        value = right.Substring(index + 1);
                    }

                    // 放在字段里面处理
                    //index = name.IndexOf("$");
                    //if (index != -1)
                    //{
                    //    field = name.Substring(0, index);
                    //    subfield = name.Substring(index + 1);
                    //}
                }

                if (string.IsNullOrEmpty(caption) == true
                    || string.IsNullOrEmpty(name) == true)
                    //|| string.IsNullOrEmpty(subfield) == true)
                {
                    throw new Exception("字段规则[" + one + "]配置的不合法，应为[caption|name]格式。name可以是字段名，例如001；也可以是子字段名称，例如105$a；头标区时name为###。");
                }


                FieldItem item = new FieldItem(caption,name, value);
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


        // 为某个字段设置值。
        public static void SetFieldValue(List<FieldItem> list, string fieldName, string fieldValue)
        {
            foreach (FieldItem field in list)
            {
                if (field.name == fieldName)
                {
                    field.value = fieldValue;
                    return;
                }
            }
        }

    }

    public class FieldItem
    {



        // name的值可能是字段，例如001；也可能是子字段，例如105$a
        public FieldItem(string strCaption, string strName,string strValue)
        {
            this.lable = strCaption;
            this.name  = strName;
            this.value = strValue;

            // 字段与子字段之间用$号分隔。
            this.field = this.name;
            // 如果有$表示有子字段，需要拆分一下
            int index = this.name.IndexOf("$");
            if (index != -1)
            {
                this.field = this.name.Substring(0, index);
                this.subfield = this.name.Substring(index + 1);
            }

        }



        public string lable { get; set; }

        public string name { get; set; }

        public string field { get; set; }

        public string subfield { get; set; }


        public string value { get; set; }


        public string dump()
        {

            string result = lable + "|" + field;
            
            if (string.IsNullOrEmpty(this.subfield)==false)
                result+= "$" + subfield;


            if (string.IsNullOrEmpty(value) == false)
                result += "|" + value;

            return result;

        }
    }
}
