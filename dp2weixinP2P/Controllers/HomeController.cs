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
        public ActionResult Test()
        {
            ViewBag.LibHtml = this.GetLibSelectHtml("");
            return View();
        }
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
            WxUserItem userItem = WxUserDatabase.Current.GetActivePatron(weiXinId);

            LibInfoModel libInfo = null;
            if (userItem!=null)
            {
                string libName = userItem.libName;
                libInfo = new LibInfoModel();
                libInfo.Title = libName+" 主页";

                string htmlFile = dp2WeiXinService.Instance.weiXinDataDir + "/lib/" + userItem.libId+"/index.html";
                if (System.IO.File.Exists(htmlFile) == false)
                {
                    // 先缺省html文件
                    htmlFile = dp2WeiXinService.Instance.weiXinDataDir + "/lib/index.html";
                }

                string strHtml = "";
                // 文件存在，取出文件 的内容
                if (System.IO.File.Exists(htmlFile) == true)
                {
                    Encoding encoding;
                    // 能自动识别文件内容的编码方式的读入文本文件内容模块
                    // parameters:
                    //      lMaxLength  装入的最大长度。如果超过，则超过的部分不装入。如果为-1，表示不限制装入长度
                    // return:
                    //      -1  出错 strError中有返回值
                    //      0   文件不存在 strError中有返回值
                    //      1   文件存在
                    //      2   读入的内容不是全部
                    nRet = FileUtil.ReadTextFileContent(htmlFile,
                        -1,
                        out strHtml,
                        out encoding,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                        throw new Exception(strError);
                    if (nRet == 2)
                        throw new Exception("FileUtil.ReadTextFileContent() error");

                    // 替换关键词
                    strHtml = strHtml.Replace("%libName%", userItem.libName);
                }
                else
                {
                    strHtml=@"<div class='mui-content-padded'>"
                        +"欢迎访问 "+libName+" 图书馆"
                        +"</div>";
                }

                libInfo.Content = strHtml;
            }

            return View(libInfo);
        }

        // 系统设置
        public ActionResult Setting()
        {
            ViewBag.success = false;

            /*
            // 从web config中取出mserver服务器地址，微信自己的账号
            string dp2MServerUrl = WebConfigurationManager.AppSettings["dp2MServerUrl"];
            string userName = WebConfigurationManager.AppSettings["userName"];            
            string password = WebConfigurationManager.AppSettings["password"];
            if (string.IsNullOrEmpty(password)==false)// 解密
                password = Cryptography.Decrypt(password, dp2WeiXinService.EncryptKey);
             */
            SettingModel model = new SettingModel();
            model.dp2MserverUrl = dp2WeiXinService.Instance.dp2MServerUrl;// "";// dp2MServerUrl;
            model.userName = dp2WeiXinService.Instance.userName;// "";//userName;
            model.password = dp2WeiXinService.Instance.password;// "";//password;

            return View(model);
        }
        [HttpPost]
        public ActionResult Setting(SettingModel model)
        {
            ViewBag.success = false;  

            dp2WeiXinService.Instance.SetDp2mserverInfo(model.dp2MserverUrl,
                model.userName,
                model.password); //函数里面会将密码加密

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


        //UiTest
        public ActionResult UiTest()
        {
            return View();
        }

	}
}