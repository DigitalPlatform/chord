using dp2weixin.service;
using dp2weixinWeb.Models;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dp2weixinWeb.Controllers
{
    public class AccountController : BaseController
    {
        /// <summary>
        /// 账户管理
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult Index(string code, string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                goto ERROR1;

            // 检查微信id是否已经绑定的读者
            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            List<WxUserItem> userList = WxUserDatabase.Current.Get(weixinId,null,-1);
            if (userList ==null || userList.Count==0)
            {
                return RedirectToAction("Bind");
            }             

            return View();

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }

        /// <summary>
        /// 绑定账户
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public ActionResult Bind(string code, string state, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                goto ERROR1;

            // 图书馆html,选中项为设置的图书馆
            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            ViewBag.LibHtml = this.GetLibSelectHtml("", weixinId,true); //2016-9-4 绑定时不支持选中默认图书馆 ViewBag.LibId

            return View();

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }

        //
        public ActionResult ScanQRCodeBind(string code, string state,
            string libId)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                goto ERROR1;

            if (libId == null)
                libId = "";

            // 图书馆html
            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            ViewBag.LibHtml = this.GetLibSelectHtml(libId, weixinId, true);

            ViewBag.LibVersions = dp2WeiXinService.Instance.LibManager.GetLibVersiongString();

            return View();

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="libId"></param>
        /// <param name="readerName"></param>
        /// <returns></returns>
        public ActionResult ResetPassword(string code, string state,
            string libId,
            string readerName)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                goto ERROR1;
            
            if (libId == null)
                libId = "";

            // 图书馆html
            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            ViewBag.LibHtml = this.GetLibSelectHtml(libId,weixinId,true);
            

            if (string.IsNullOrEmpty(readerName) == false && readerName != "undefined")
                ViewBag.ReaderName = readerName;// "test";

            return View();

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }


        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="patronBarcode"></param>
        /// <returns></returns>
        public ActionResult ChangePassword(string code, string state,string patronBarcode)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                goto ERROR1;

            if (String.IsNullOrEmpty(patronBarcode) == true)
            {
                string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
                WxUserItem userItem = WxUserDatabase.Current.GetActivePatron(weixinId,ViewBag.LibId);
                if (userItem != null)
                    patronBarcode = userItem.readerBarcode;
            }

            ViewBag.patronBarcode = patronBarcode;

            return View();

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }


    }
}