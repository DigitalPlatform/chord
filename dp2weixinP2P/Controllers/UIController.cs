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
    /*
    public class MarcHeaderHelper
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

//记录长度 0/5
//记录状态 5/1
//执行代码:记录类型 6/1
//执行代码:书目级别 7/1
//执行代码:层次等级 8/1
//执行代码:未定义 9/1
//指示符长度: 10/1
//子字段标识符长度: 11/1
//数据基地址: 12/5
//记录附加定义:编目等级 17/1
//记录附加定义:著录格式 18/1
//记录附加定义:未定义 19/1
//地址目次项结构 20/4 
 

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
                                                + "<button id='btn-header' onclick=\"expand('marcheader')\">展开</button>"
                                                + "<div class='mui-collapse-content marcheader' style='display:none'>"
                                                 + "<div style='color:#cccccc; font-size: 9px'>栏位说明后面的'X/X'表示'超始位置/字符长度'，如果内容的长度不足规定的长度，系统自动在末尾补空格或?号；如果内容的长度超过规定的长度，则从前方截取。</div>"
            + " <table>";

            List<MarcHeaderItem> marcHeader = MarcHeaderHelper.Parse(strHeader);
            foreach (MarcHeaderItem item in marcHeader)
            {
                int width = 20 * item.Length;
                string id = "h" + item.Start + "L" + item.Length;
                headerHtml += "<tr>"
                    + "<td class='label'>" + item.Name + "&nbsp;" + item.Location + "</td>"
                    + "<td><input id='" + id + "' type='text' class='myinput' style='width:" + width + "px;' value='" + item.Value + "' /></td>"
                    + "</tr>";
            }

            headerHtml += "</table></div></div>";

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
    */

    public class UIController : Controller
    {
        public ActionResult header()
        {

            string strheader = MarcHeaderHelper.GetMarcHeaderText();

            string headerHtml = MarcHeaderHelper.GetHeaderHtml(strheader);
            ViewData["marcheader"] = headerHtml;

            return View();
        }



        public ActionResult SearchItemUI()
        {
            return View();
        }


        public ActionResult AudioTest()
        {
            return View();
        }
        public Task<FileStreamResult> Speak(string text)
        {
            return Task.Factory.StartNew(() =>
            {
                using (SpeechSynthesizer synthesizer = new SpeechSynthesizer())
                {
                    var ms = new MemoryStream();
                    try
                    {
                        synthesizer.SetOutputToWaveStream(ms);
                        synthesizer.Speak(text);
                    }
                    catch (Exception ex)
                    {
                        dp2WeiXinService.Instance.WriteErrorLog("!!!生成声音["+text+"]异常：" + ex.Message);
                        
                    }
                    
                    ms.Position = 0;
                    return new FileStreamResult(ms, "audio/vnd.wav");//"audio/x-wav");//"audio/wav");
                }
            });
        }

        public Task<FilePathResult> CreateMp3()
        {
            return Task.Factory.StartNew(() =>
            {
                using (var ss = new SpeechSynthesizer())
                {
                    //string dataDir = Server.MapPath(string.Format("~/audio"));
                    //string fileName = dataDir + "/test.wav";
                    //ss.SetOutputToWaveFile(fileName);   //(@"C:\MyAudioFile.wav");
                    //ss.Speak("Hello World");
                    //return new FilePathResult("~/audio/test.wav", "audio/wav");

                    return new FilePathResult("~/audio/patron.mp3", "audio/mp3");
                }
            });
        }

        public Task<FilePathResult> CreateWav(string text)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var ss = new SpeechSynthesizer())
                {
                    string dataDir = Server.MapPath(string.Format("~/audio"));
                    string fileName = dataDir + "/test.wav";
                    ss.SetOutputToWaveFile(fileName);   //(@"C:\MyAudioFile.wav");
                    ss.Speak(text);
                    return new FilePathResult("~/audio/test.wav", "audio/wav");
                }
            });
        }


        public ActionResult CtrlDemo()
        {

            //List<string> libList = new List<string>();
            //libList.Add("分馆一");
            //libList.Add("分馆二");
            //libList.Add("分馆三");
            //ViewBag.libList = libList;

            string xml = @"<item canborrow='no' itemBarcodeNullable='yes'>保存本库</item><item canborrow='no' itemBarcodeNullable='yes'>阅览室</item><item canborrow='yes' itemBarcodeNullable='yes'>流通库</item>
<library code='方洲小学'><item canborrow='yes' itemBarcodeNullable='yes'>图书总库</item></library>
<library code='星洲小学'><item canborrow='yes' itemBarcodeNullable='yes'>阅览室</item>
</library>";

            List<SubLib> subLibs = SubLib.ParseSubLib(xml,true);
            ViewBag.libList = subLibs;
            ViewBag.bindLink = "<a href='www.dp2003.com'>尚未绑定帐户</a>";
            return View();
        }

        public ActionResult Charge()
        {
            return View();
        }

        public ActionResult Scan()
        {


            //JsSdkUiPackage package = JSSDKHelper.GetJsSdkUiPackage(dp2WeiXinService.Instance.weiXinAppId,
            //    dp2WeiXinService.Instance.weiXinSecret,
            //    Request.Url.AbsoluteUri);

            //ViewData["AppId"] = dp2WeiXinService.Instance.weiXinAppId;
            //ViewData["Timestamp"] = package.Timestamp;
            //ViewData["NonceStr"] = package.NonceStr;
            //ViewData["Signature"] = package.Signature;
            return View();
        }

        public ActionResult MsgEdit()
        {

            //string resUri = HttpUtility.UrlEncode("http://localhost/dp2weixin/img/guide.pdf");
            //string resLink = "<a href='../Patron/getobject?strURI=" + resUri + "'>test res</a>";

            string resUri = "中文图书/3" + "/object/1";
            string libId = "57b91e7083cbdc2394ea17dc";

            string resLink = "<a href='../Patron/getobject?libId=" + HttpUtility.UrlEncode(libId)
            + "&mime=" + HttpUtility.UrlEncode("application/pdf")
            + "&uri=" + HttpUtility.UrlEncode(resUri)+"'>"
            + "test res</a>  ";


            ViewBag.ResLink = resLink;
            return View();
        }


        // GET: UI
        public ActionResult Loading()
        {
            return View();
        }

        // GET: UI
        public ActionResult BiblioIndex()
        {
            return View();
        }

        public ActionResult PersonalInfo()
        {
            return View();
        }

        public ActionResult Message()
        {
            return View();
        }
    }
}