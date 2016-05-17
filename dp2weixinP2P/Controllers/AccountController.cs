using dp2Command.Service;
using dp2weixin.service;
using dp2weixinP2P.Models;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dp2weixinP2P.Controllers
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


            // 如果没有绑定一个账号，进入到账号绑定界面
            string weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            // 检查微信id是否已经绑定的读者
            List<WxUserItem> userList = WxUserDatabase.Current.GetByWeixinId(weiXinId);
            if (userList == null || userList.Count == 0)
                return RedirectToAction("Bind");


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

    }
}