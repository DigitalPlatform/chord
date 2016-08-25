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

            // 未账户任何账户时，自动转到绑定界面            
            if (Session[WeiXinConst.C_Session_IsBind] == null 
                || (int)Session[WeiXinConst.C_Session_IsBind] == 0)
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
            ViewBag.LibHtml = this.GetLibSelectHtml(ViewBag.LibId,weixinId);

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
            
            // 如果是从绑定界面过来的，会传来绑定界面使用的图书馆
            // 如果未传进图书馆，使用设置的图书馆
            if (string.IsNullOrEmpty(libId) == true)
            {
                libId=ViewBag.LibId;
            }            

            // 图书馆html
            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            ViewBag.LibHtml = this.GetLibSelectHtml(libId,weixinId);
            

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