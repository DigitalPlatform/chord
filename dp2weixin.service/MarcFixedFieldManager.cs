using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dp2weixin.service
{
    /*
<?xml version="1.0" encoding="utf-8"?>
<root>
    <field name="100$a" lable="通用处理数据" length="34">
            <char name="0/8" lable="记录生成时间" defaultValue="%year%%m2%%d2%" />
            <char name="8/1" lable="出版时间类型" valueList="#unimarc_100_a_8/1" />
            <char name="9/4" lable="出版年1" defaultValue="%year%" />
            <char name="13/4" lable="出版年2" defaultValue="%year%" />
            <char name="17/3" lable="阅读对象代码" valueList="#unimarc_100_a_17/3" />
            <char name="20/1" lable="政府出版物代码" valueList="#unimarc_100_a_20/1" />
            <char name="21/1" lable="变更记录代码(必备)" valueList="#unimarc_100_a_21/1" />
            <char name="22/3" lable="编目语种代码(必备)" valueList="#languagecode" />
            <char name="25/1" lable="音译代码" valueList="#unimarc_100_a_25/1" />
            <char name="26/4" lable="字符集(必备)" valueList="#unimarc_100_a_26/4" />
            <char name="30/4" lable="补充字符集" />
            <char name="34/2" lable="题名文种代码" valueList="#unimarc_100_a_34/2" />
    </field>
    <field name="105$a" lable="专著编码数据" length="13">
            <char name="0/4" lable="图表代码" valueList="#unimarc_105_a_0/4"/>
            <char name="4/4" lable="内容类型代码" valueList="#unimarc_105_a_4/4"/>
            <char name="8/1" lable="会议代码" valueList="#unimarc_105_a_8/1"/>
            <char name="9/1" lable="纪念文集指示符" valueList="#unimarc_105_a_9/1"/>
            <char name="10/1" lable="索引指示符" valueList="#unimarc_105_a_10/1"/>
            <char name="11/1" lable="文学体裁代码" valueList="#unimarc_105_a_11/1"/>
            <char name="12/1" lable="传记代码" valueList="#unimarc_105_a_12/1"/>
    </field>
</root>
     */
    // Marc定长字段管理器
    public class MarcFixedFieldManager
    {
        // 定长字段集合
        public List<MarcFixedField> _fixedFields = new List<MarcFixedField>();

        // 配置文件
        string _marcFixedFieldDef = "";

        // todo 还缺一个valuelist配置文件和相关处理。

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="strMarcFixedFieldDef">MarcFixedFieldDef.xml配置文件</param>
        /// <param name="error"></param>
        /// <returns></returns>
        public int init(string strMarcFixedFieldDef, out string error)
        {
            error = "";

            this._marcFixedFieldDef = strMarcFixedFieldDef;

            try
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(this._marcFixedFieldDef);
                XmlNode root = dom.DocumentElement;

                XmlNodeList fieldNodes = root.SelectNodes("field");
                foreach (XmlNode one in fieldNodes)
                {
                    MarcFixedField field = new MarcFixedField();
                    field.Init(one);
                    this._fixedFields.Add(field);


                }
            }
            catch (Exception ex)
            {
                error = "初始化定长字段配置文件'" + strMarcFixedFieldDef + "'出错:" + ex.Message;
                return -1;

            }

            return 0;
        }


        // 根据名称找到对应的配置定长字段配置，名称可能是字段名，例如001; 也可能是子字段名，例如105$a。
        // 使用的地方，如果调此函数没有找到对应的定长字段配置会返回null，则以默认的textbox输入框来显示。
        public MarcFixedField GetFixedField(string strName)
        {
            foreach (MarcFixedField field in _fixedFields)
            {
                if (field.name== strName)
                    return field;
            }

            return null;
        }


        // 获取html，构造好的界面
        public static string GetHtml(MarcFixedField field,out string inputIds)
        {
            // 按顺序各char输入框的id，以逗号分隔
            inputIds = "";

            //把$替换成空格，例如100$a变成100a。
            //这个$符合不能作为html元素的id，要不在jquery里找不到这个元素
            string tempFieldName = field.name.Replace("$", "");
            tempFieldName = tempFieldName.Replace("###", "head");  //替换头标区,要不js无法取名称

            // 展开收缩
            string html = "<div id='"+ tempFieldName + "' style='padding:2px'>"  
                             + "<button id='btn-expand' onclick=\"expandHeader('"+ tempFieldName + "')\">展开</button>"
                             + "<div class='mui-collapse-content marcheader' style='display:none'>"
                            + " <table>";

            //List<MarcFixedFieldChar> fixedChars = MarcHeaderHelper.Parse(strHeader);
            foreach (MarcFixedFieldChar item in field.fixedChars)
            {
                int width = 20 * item.length; //输入框的宽度

                // 每一行字符
                string id = "f"+ tempFieldName + "_" + item.start + "_" + item.length;

                // 以逗号分隔
                if (string.IsNullOrEmpty(inputIds) == false)
                    inputIds += ",";
                inputIds += id;

                // 拼html
                html += "<tr>"
                    + "<td class='label'>" + item.lable + "&nbsp;" + item.name + "</td>"
                    + "<td><input id='" + id + "' type='text' class='myinput' style='width:" + width + "px;' value='" + item.value + "' /></td>"
                    + "</tr>";
            }

            // 加一行说明
            html += "<tr><td colspan='2' style='color:#cccccc; font-size: 8px;text-align:left'>栏位说明后面的'X/X'表示'超始位置/字符长度'，如果内容的长度不足规定的长度，系统自动在末尾补空格或?号；如果内容的长度超过规定的长度，则从前方截取。</td></tr>";

            html += "</table>"
                + "</div></div>";

            return html;
        }

        public static string GetMarcHeaderText()
        {
            string header = "?????nam0 22?????   45__";

            return header;
        }
    }



    // Marc定长字段
    public class MarcFixedField
    {
        public string lable { get; set; } //标题，例如：专著编码数据

        //可以为字段名（例如001），也可以为子字段名（100$a)
        public string name { get; set; }

        public string fieldName { get; set; } //字段名，从name中拆出来的
        public string subFieldName { get; set; }//子字段名，从name中拆出来的

        // 定长字段总长度
        public int length { get; set; }      //长度，例如1个字符

        // 各个位
        public List<MarcFixedFieldChar> fixedChars = new List<MarcFixedFieldChar>();

        /*
    <field name="105$a" lable="专著编码数据" length="13">
            <char name="0/4" lable="图表代码" valueList="#unimarc_105_a_0/4"/>
            <char name="4/4" lable="内容类型代码" valueList="#unimarc_105_a_4/4"/>
            <char name="8/1" lable="会议代码" valueList="#unimarc_105_a_8/1"/>
            <char name="9/1" lable="纪念文集指示符" valueList="#unimarc_105_a_9/1"/>
            <char name="10/1" lable="索引指示符" valueList="#unimarc_105_a_10/1"/>
            <char name="11/1" lable="文学体裁代码" valueList="#unimarc_105_a_11/1"/>
            <char name="12/1" lable="传记代码" valueList="#unimarc_105_a_12/1"/>
    </field> 
         */
        // 根据xmlnode节点配置初始化定长字段
        internal void Init(XmlNode fieldNode)
        {
            this.name = DomUtil.GetAttr(fieldNode, "name");

            // 将name拆成字段名 与 子字段名，使用的时候判断如果子字段为空，则为字段名
            this.fieldName = this.name;
            int nIndex = this.name.IndexOf('$');
            if (nIndex > 0)
            {
                this.fieldName = this.name.Substring(0, nIndex);
                this.subFieldName = this.name.Substring(nIndex + 1);
            }

            // 标签名
            this.lable = DomUtil.GetAttr(fieldNode, "lable");

            // 总长度
            string strLengh = DomUtil.GetAttr(fieldNode, "length");
            try
            {
                // 如果strLenght忘记配置或者配置的不是数字，会抛信息
                this.length = Convert.ToInt32(strLengh);
            }
            catch (Exception ex)
            {
                throw new Exception("定长字段'" + name + "'的length配置不合法：" + ex.Message);
            }

            // 处理下级的char
            XmlNodeList charNodes = fieldNode.SelectNodes("char");
            foreach (XmlNode one in charNodes)
            {
                MarcFixedFieldChar fieldChar = new MarcFixedFieldChar(one);
                this.fixedChars.Add(fieldChar);
            }

        }

        // 是否是子字段
        public bool IsSubField
        {
            get 
            {
                if (string.IsNullOrEmpty(this.subFieldName) == false)
                    return true;
                else
                    return false;
            }
        }

        // 把值设到各个栏位上。
        public void SetValue(string value)
        {
            // 有可能进来的长度不够
            value = value.PadRight(this.length,' ');

            //throw new NotImplementedException();
            foreach (MarcFixedFieldChar c in this.fixedChars)
            {
                c.value = value.Substring(c.start, c.length);
            }
        }


    }

    public class MarcFixedFieldChar
    {
        public string lable { get; set; } //标题，例如：记录状态
        public string name { get; set; }    //例如0/4，解析为开始位置和长度
        public int start { get; set; }     //位置，例如5表示从第5位起
        public int length { get; set; }      //长度，例如1个字符
        public string value { get; set; }     //值
        public string valueList { get; set; }  //值列表
        public string defaultValue { get; set; }  // 默认值

        //  <char name="12/1" lable="传记代码" valueList="#unimarc_105_a_12/1"/>
        public MarcFixedFieldChar(XmlNode node)
        {
            this.lable = DomUtil.GetAttr(node, "lable");

            // name="12/1"
            this.name = DomUtil.GetAttr(node, "name");
            if (string.IsNullOrEmpty(this.name) == false)
            {
                int nIndex = this.name.IndexOf("/");
                if (nIndex > 0)
                {
                    start = Convert.ToInt32(this.name.Substring(0, nIndex));
                    length = Convert.ToInt32(this.name.Substring(nIndex + 1));
                }
            }

            // value一般是界面上输入的
            //value = DomUtil.GetAttr(node,"value");

            // 缺省值，todo未实现
            this.defaultValue = DomUtil.GetAttr(node, "defaultValue");
            // 下拉列表，todo未实现
            this.valueList = DomUtil.GetAttr(node, "valueList");
        }

        public MarcFixedFieldChar(string strLable,
            string strName,
            int nStart,
            int nLength,
            string strValue)
        {
            this.lable = strLable;
            this.name = strName;
            this.start = nStart;
            this.length = nLength;
            this.value = strValue;
        }


    }

}
