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
        /// myWeixinId：参数里传一个weixinId，用于在浏览器模拟微信功能
        /// <returns></returns>
        public ActionResult Index(string code, string state,string myWeixinId)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code,
                state,
                out SessionInfo sessionInfo,
                out strError,
                myWeixinId);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 如果尚未选择图书馆，不存在当前帐号，出现绑定帐号链接
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Account/Index?myWeixinId="+myWeixinId);
                return View();
            }

            // 检查微信id是否已经绑定的读者
            if (sessionInfo.ActiveUser.type == WxUserDatabase.C_Type_Worker 
                && sessionInfo.ActiveUser.userName == "public")
            {
                List<WxUserItem> userList = WxUserDatabase.Current.Get(sessionInfo.WeixinId, null, -1);
                if (userList.Count > 1)
                {
                    ViewBag.Warn = "您尚未绑定当前图书馆[" + ViewBag.LibName + "]的帐户，请点击'新增绑定账号'按钮绑定帐户。";
                }
                else
                {
                    return RedirectToAction("Bind");
                }
            }

            return View();
        }

        /// <summary>
        /// 绑定账户
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="returnUrl">绑定完返回的url</param>
        /// <returns></returns>
        public ActionResult Bind(string code, string state, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code,
                state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 如果尚未选择图书馆，不存在当前帐号，出现绑定帐号链接
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Accout/Bind");
                return View();
            }

            ViewBag.fromUrl = "/Account/Bind";
            return View();
        }


        /// <summary>
        /// 柜台绑定
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="libId">???没用吧</param>
        /// <returns></returns>
        public ActionResult ScanQRCodeBind(string code, string state,
            string libId)
        {

            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code,
                state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Accout/ScanQRCodeBind");
                return View();
            }

            ViewBag.LibVersions = dp2WeiXinService.Instance.LibManager.GetLibVersiongString();
            return View();
        }

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="readerName"></param>
        /// <returns></returns>
        public ActionResult ResetPassword(string code, 
            string state,
            string readerName)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code,state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state,"/Account/ResetPassword");
                return View();
            }

            if (string.IsNullOrEmpty(readerName) == false
                && readerName != "undefined")
            {
                ViewBag.ReaderName = readerName;// "test";
            }

            return View();
        }


        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="patronBarcode"></param>
        /// <returns></returns>
        public ActionResult ChangePassword(string code, 
            string state,
            string patronBarcode)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Account/ChangePassword");
                return View();
            }

            if (String.IsNullOrEmpty(patronBarcode) == true)
            {
                if (sessionInfo.ActiveUser != null)
                    patronBarcode = sessionInfo.ActiveUser.readerBarcode;
            }

            ViewBag.patronBarcode = patronBarcode;
            return View();
        }


    }
}