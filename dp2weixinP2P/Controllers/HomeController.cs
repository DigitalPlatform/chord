using DigitalPlatform.LibraryRestClient;
using dp2weixin;
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
        string url = "~/weixin/index";
        /// <summary>
        /// 通道所使用的 HTTP Cookies
        /// </summary>
        public CookieContainer Cookies = new System.Net.CookieContainer();

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
            return View();
        }

        [HttpPost]
        //WeixinMessage
        public ActionResult WeixinMessagePost()
        {
            string msgSend = Request["txtMessage"];
            string resultStr = "";
            try
            {
                CookieAwareWebClient client = new CookieAwareWebClient(this.Cookies);
                client.Headers["Content-type"] = "application/xml; charset=utf-8";
                string xml = WeiXinClientUtil.GetPostXmlToWeiXinGZH(msgSend);
                byte[] baData = Encoding.UTF8.GetBytes(xml);
                byte[] result = client.UploadData(this.url,
                    "POST",
                    baData);
                string strResult = Encoding.UTF8.GetString(result);
                resultStr = strResult;

                // 将焦点设回输入框
                //this.lblMessage.Text = "您刚才发的消息是[" + this.txtMessage.Text + "]";

                //this.txtMessage.Text = "";
                //this.txtMessage.Focus();
            }
            catch (Exception ex)
            {
                resultStr = "Exception :" + ex.Message;
            }

            // 继续跟上登录成功返回的url
            ViewBag.msgResult = resultStr;
            return View();
        }

        //WeixinMenu
        public ActionResult WeixinMenu()
        {
            return View();
        }

	}
}