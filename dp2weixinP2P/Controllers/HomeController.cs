using DigitalPlatform.LibraryRestClient;
using dp2weixin;
using dp2weixinP2P.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace dp2weixinP2P.Controllers
{
    public class HomeController : Controller
    {
        string url = "http://localhost:15794/weixin/index";
        /// <summary>
        /// 通道所使用的 HTTP Cookies
        /// </summary>
        //public CookieContainer Cookies = new System.Net.CookieContainer();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LibraryM()
        {
            return View();
        }

        //WeixinUser
        public ActionResult WeixinUser()
        {
            return View();
        }

        //WeixinMessage
        public ActionResult WeixinMessage()
        {
            MessageModel model = new MessageModel();
            return View(model);
        }

        [HttpPost]
        //WeixinMessage
        public ActionResult WeixinMessage(MessageModel model)
        {
            string msgSend = model.RequestMsg;//Request["txtMessage"];
            string resultStr = "";
            try
            {
                CookieContainer cookies = new System.Net.CookieContainer();
                CookieAwareWebClient client = new CookieAwareWebClient(cookies);
                client.Headers["Content-type"] = "application/xml; charset=utf-8";
                string xml = WeiXinClientUtil.GetPostXmlToWeiXinGZH(msgSend);
                byte[] baData = Encoding.UTF8.GetBytes(xml);
                byte[] result = client.UploadData(this.url,
                    "POST",
                    baData);
                string strResult = Encoding.UTF8.GetString(result);
                resultStr = strResult;
            }
            catch (Exception ex)
            {
                resultStr = "Exception :" + ex.Message;
            }

            // 继续跟上登录成功返回的url
            model.ResponseMsg = resultStr;
            return View(model);
        }

        //WeixinMenu
        public ActionResult WeixinMenu()
        {
            return View();
        }

	}
}