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
        /// 可通过OAuth2.0方式重定向过来
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
                return Content(strError);

            // 未账户任何账户时，自动转到绑定界面            
            if (Session[WeiXinConst.C_Session_IsBind] == null || (int)Session[WeiXinConst.C_Session_IsBind] == 0)
            {
                return RedirectToAction("Bind");
            }              


            return View();
        }

        public ActionResult Bind(string code, string state, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);
            

            return View();
        }

        public ActionResult ResetPassword(string code, string state,
            string libCode,
            string readerName)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            if (string.IsNullOrEmpty(libCode) == false && libCode != "undefined")
                ViewBag.LibCode = libCode;// "lib_local*mycapo";

            if (string.IsNullOrEmpty(readerName) == false && readerName != "undefined")
                ViewBag.ReaderName = readerName;// "test";

            return View();
        }


        public ActionResult ChangePassword(string code, string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            string weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            WxUserItem userItem = WxUserDatabase.Current.GetActivePatron(weiXinId);

            return View(userItem);
        }


    }
}