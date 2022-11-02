using DigitalPlatform.Xml;
using dp2weixin.service;
using Senparc.Weixin.MP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Speech;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.IO;

namespace dp2weixinWeb.Controllers
{

    public class MarcHeaderHelper1
    {
        // 得到默认的header结构
        public static List<MarcHeaderItem> GetMarcHeader()
        {
            string header = "?????nam0 22?????   45__";

            return Parse(header);
        }

        // 得到默认的header字符串
        public static string GetMarcHeaderText()
        {
            string header = "?????nam0 22?????   45__";

            return header;
        }


        public static List<MarcHeaderItem> Parse(string strHeader)
        {
            List<MarcHeaderItem> list = new List<MarcHeaderItem>();

            /*
记录长度 0/5
记录状态 5/1
执行代码:记录类型 6/1
执行代码:书目级别 7/1
执行代码:层次等级 8/1
执行代码:未定义 9/1
指示符长度: 10/1
子字段标识符长度: 11/1
数据基地址: 12/5
记录附加定义:编目等级 17/1
记录附加定义:著录格式 18/1
记录附加定义:未定义 19/1
地址目次项结构 20/4 
 */

            //?????nam0 22?????   45__
            if (string.IsNullOrEmpty(strHeader) == true)
                throw new Exception("strHeader参数不能为空");

            if (strHeader.Length != 24)
                throw new Exception("Marc头标区不合法，长度须为24个字符。");

            string value0 = strHeader.Substring(0, 5);
            list.Add(new MarcHeaderItem("记录长度", 0, 5, value0));

            string value5 = strHeader.Substring(5, 1);
            list.Add(new MarcHeaderItem("记录状态", 5, 1, value5));

            string value6 = strHeader.Substring(6, 1);
            list.Add(new MarcHeaderItem("执行代码:记录类型", 6, 1, value6));

            string value7 = strHeader.Substring(7, 1);
            list.Add(new MarcHeaderItem("执行代码:书目级别", 7, 1, value7));

            string value8 = strHeader.Substring(8, 1);
            list.Add(new MarcHeaderItem("执行代码:层次等级", 8, 1, value8));

            string value9 = strHeader.Substring(9, 1);
            list.Add(new MarcHeaderItem("执行代码:未定义", 9, 1, value9));

            string value10 = strHeader.Substring(10, 1);
            list.Add(new MarcHeaderItem("指示符长度", 10, 1, value10));

            string value11 = strHeader.Substring(11, 1);
            list.Add(new MarcHeaderItem("子字段标识符长度", 11, 1, value11));

            string value12 = strHeader.Substring(12, 5);
            list.Add(new MarcHeaderItem("数据基地址", 12, 5, value12));

            string value17 = strHeader.Substring(17, 1);
            list.Add(new MarcHeaderItem("记录附加定义:编目等级", 17, 1, value17));

            string value18 = strHeader.Substring(8, 1);
            list.Add(new MarcHeaderItem("记录附加定义:著录格式", 18, 1, value18));

            string value19 = strHeader.Substring(9, 1);
            list.Add(new MarcHeaderItem("记录附加定义:未定义", 19, 1, value19));

            string value20 = strHeader.Substring(20, 4);
            list.Add(new MarcHeaderItem("地址目次项结构", 20, 4, value20));

            return list;
        }

        // 获取html，构造好的界面
        public static string GetHeaderHtml(string strHeader)
        {
            string headerHtml = "<div id='marcheader' style='padding:2px'>"
                                                + "<button id='btn-expand' onclick=\"expandHeader('marcheader')\">展开</button>"
                                                + "<div class='mui-collapse-content marcheader' style='display:none'>"
            + " <table>";

            List<MarcHeaderItem> marcHeader = MarcHeaderHelper1.Parse(strHeader);
            foreach (MarcHeaderItem item in marcHeader)
            {
                int width = 20 * item.Length;
                string id = "h" + item.Start + "L" + item.Length;
                headerHtml += "<tr>"
                    + "<td class='label'>" + item.Name + "&nbsp;" + item.Location + "</td>"
                    + "<td><input id='" + id + "' type='text' class='myinput' style='width:" + width + "px;' value='" + item.Value + "' /></td>"
                    + "</tr>";
            }

            headerHtml += "<tr><td colspan='2' style='color:#cccccc; font-size: 9px;text-align:left'>栏位说明后面的'X/X'表示'超始位置/字符长度'，如果内容的长度不足规定的长度，系统自动在末尾补空格或?号；如果内容的长度超过规定的长度，则从前方截取。</td></tr>";

            headerHtml += "</table>"
                 //+"<div style='color:#cccccc; font-size: 9px'>栏位说明后面的'X/X'表示'超始位置/字符长度'，如果内容的长度不足规定的长度，系统自动在末尾补空格或?号；如果内容的长度超过规定的长度，则从前方截取。</div>"
                + "</div></div>";

            return headerHtml;


        }
    }


    public class MarcHeaderItem
    {
        //记录状态 5/1
        public string Name;   //标题，例如：记录状态
        public int Start;    //位置，例如5表示从第5位起
        public int Length;      //长度，例如1个字符
        public string Value;    //值

        public MarcHeaderItem(string name, int start, int length, string value)
        {
            this.Name = name;
            this.Start = start;
            this.Length = length;
            this.Value = value;
        }


        // 用于显示
        public string Location
        {
            get
            {
                return Start + "/" + Length;
            }
        }
    }

}