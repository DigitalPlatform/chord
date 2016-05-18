using DigitalPlatform.LibraryRestClient;
using DigitalPlatform.Text;
using dp2Command.Service;
using dp2weixin;
using dp2weixin.service;
using dp2weixinP2P.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace dp2weixinP2P.Controllers
{
    public class HomeController : BaseController
    {

        public ActionResult Index(string code, string state, string admin, string weiXinId)
        {
            if (String.IsNullOrEmpty(admin) == false && admin == "1")
            {
                Session["userType"] = "admin";
            }

            if (String.IsNullOrEmpty(admin) == false && admin == "0")
            {
                Session["userType"] = null;
            }

            // 用于测试，如果传了一个weixin id参数，则存到session里
            if (String.IsNullOrEmpty(weiXinId) == false)
            {
                // 记下微信id
                Session[WeiXinConst.C_Session_WeiXinId] = weiXinId;
            }

            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            WxUserItem userItem= WxUserDatabase.Current.GetActive(weiXinId);

            LibInfoModel libInfo = null;
            if (userItem!=null)
            {
                string libName = userItem.libName;
                libInfo = new LibInfoModel();
                libInfo.Title = libName+" 主页";
                libInfo.Content = @"<div class='mui-content-padded'>"
    +"欢迎访问”"+libName+"“图书馆"
    +"</div>";
            }

            return View(libInfo);
        }

        // 系统设置
        public ActionResult Setting()
        {
            ViewBag.success = false;

            // 从web config中取出mserver服务器地址，微信自己的账号
            string dp2MServerUrl = WebConfigurationManager.AppSettings["dp2MServerUrl"];
            string userName = WebConfigurationManager.AppSettings["userName"];            
            string password = WebConfigurationManager.AppSettings["password"];
            if (string.IsNullOrEmpty(password)==false)// 解密
                password = Cryptography.Decrypt(password, dp2CmdService2.EncryptKey);

            SettingModel model = new SettingModel();
            model.dp2MserverUrl = dp2MServerUrl;
            model.userName = userName;
            model.password = password;
            return View(model);
        }
        [HttpPost]
        public ActionResult Setting(SettingModel model)
        {
            ViewBag.success = false;  

            Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
            //获取appSettings节点
            AppSettingsSection appSection = (AppSettingsSection)config.GetSection("appSettings");


            //在appSettings节点中添加元素Add方法，多次添加的值会以逗号分隔，所以要先remove，再add
            //appSection.Settings["dp2mServerUrl"].Value = model.dp2MserverUrl;
            appSection.Settings.Remove("dp2mServerUrl");
            appSection.Settings.Add("dp2mServerUrl", model.dp2MserverUrl);

            appSection.Settings.Remove("userName");
            appSection.Settings.Add("userName", model.userName);

            appSection.Settings.Remove("password");
            string password = Cryptography.Encrypt(model.password, dp2CmdService2.EncryptKey);
            appSection.Settings.Add("password", password);

            config.Save();
            //Response.Write("保存成功");

            //Response.Write("<script>alert('配置信息保存成功');</script>");
            ViewBag.success = true;  
            return View(model);
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

        //WeixinMenu
        public ActionResult WeixinMenu()
        {
            return View();
        }


        public ActionResult About(string code, string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            return View();
        }


        public ActionResult Contact(string code, string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            return View();
        }
	}
}