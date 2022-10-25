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


    

    public class UIController : Controller
    {
        public ActionResult header()
        {
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