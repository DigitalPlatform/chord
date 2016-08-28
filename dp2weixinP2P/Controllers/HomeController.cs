using DigitalPlatform.IO;
using DigitalPlatform.LibraryRestClient;
using DigitalPlatform.Text;
using dp2Command.Service;
using dp2weixin;
using dp2weixin.service;
using dp2weixinWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace dp2weixinWeb.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index(string code, string state, string weixinId)
        {
            return Redirect("~/Library/Home?code=" + code
                + "&state=" + state
                + "&weixinId=" + weixinId);
        }

        // 超级管理员登录
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel model,string returnUrl)
        {
            string userName = "";
            string password = "";
            dp2WeiXinService.Instance.GetSupervisorAccount(out userName, out password);

            string error = "";
            if (model.UserName != userName || model.Password != password)
            {
                error = "账户或密码不正确。";
            }

            if (error == "")
            {
                // 设为超级管理员已登录状态
                Session[dp2WeiXinService.C_Session_Supervisor] = true;

                if (string.IsNullOrEmpty(returnUrl) == false)
                    return Redirect(returnUrl);
                else
                    return Redirect("~/Home/Manager");
            }

            ViewBag.Error = error;
            return View();
        }



        // Manager
        public ActionResult Manager()
        {
            if (CheckSupervisorLogin() == false)
            {
                return Redirect("~/Home/Login?returnUrl=" + HttpUtility.UrlEncode("~/Home/Manager"));
            }

            return View();
        }

        // 参数配置
        public ActionResult Setting()
        {
            if (CheckSupervisorLogin() == false)
            {
                return Redirect("~/Home/Login?returnUrl=" + HttpUtility.UrlEncode("~/Home/Setting"));
            }

            ViewBag.success = false;

            SettingModel model = new SettingModel();
            model.dp2MserverUrl = dp2WeiXinService.Instance.dp2MServerUrl;// "";// dp2MServerUrl;
            model.userName = dp2WeiXinService.Instance.userNameWeixin;// "";//userName;
            model.password = dp2WeiXinService.Instance.password;// "";//password;
            model.mongoDbConnection = dp2WeiXinService.Instance.monodbConnectionString;
            model.mongoDbPrefix = dp2WeiXinService.Instance.monodbPrefixString;

            return View(model);
        }
        [HttpPost]
        public ActionResult Setting(SettingModel model)
        {
            ViewBag.success = false;

            string strError = "";
            int nRet = dp2WeiXinService.Instance.SetDp2mserverInfo(model.dp2MserverUrl,
                model.userName,
                model.password,
                "",
                "",
                out strError); //函数里面会将密码加密
            if (nRet == -1)
            {
                ViewBag.success = false;
                ViewBag.Error = strError;
            }
            else
                ViewBag.success = true;  
            return View(model);
        }

        public ActionResult LibraryM()
        {
            if (CheckSupervisorLogin() == false)
            {
                return Redirect("~/Home/Login?returnUrl=" + HttpUtility.UrlEncode("~/Home/LibraryM"));
            }


            return View();
        }

        //WeixinUser
        public ActionResult WeixinUser(string code, string state)
        {
            if (CheckSupervisorLogin() == false)
            {
                return Redirect("~/Home/Login?returnUrl=" + HttpUtility.UrlEncode("~/Home/WeixinUser"));
            }

            return View();
        }

        //WeixinMessage
        public ActionResult WeixinMessage()
        {
            if (CheckSupervisorLogin() == false)
            {
                return Redirect("~/Home/Login?returnUrl=" + HttpUtility.UrlEncode("~/Home/WeixinMessage"));
            }

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
                string url = "http://localhost:15794/weixin/index";
                byte[] result = client.UploadData(url,
                    "POST",
                    baData);
                string strResult = Encoding.UTF8.GetString(result);
                resultStr = strResult;
            }
            catch (Exception ex)
            {
                resultStr = "Exception :" + ex.Message;
            }

            model.ResponseMsg = resultStr;
            return View(model);
        }

        /// <summary>
        /// 联系开发者
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult Contact(string code, string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                goto ERROR1;

            return View();

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }


	}
}