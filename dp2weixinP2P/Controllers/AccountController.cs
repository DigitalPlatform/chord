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

        public ActionResult NewParton(string code, string state,
         string libId)
        {
            string strError = "";
            int nRet = 0;

            // 检查当前是否已经选择了图书馆绑定了帐号
            WxUserItem activeUser = null;
            nRet = this.GetActive(code, state,
                out activeUser,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }
            //if (nRet == 0)
            //{
            //    ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Account/NewParton");
            //    return View();
            //}

            if (libId == null)
                libId = "";

            string weixinId = ViewBag.weixinId; 


            return View();

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }

        /*
        // web登录
        public ActionResult WebLogin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            string strError = "";

            //// 检查是否从微信入口进来
            //string strError = "";
            //string state = "ilovelibrary";
            //int nRet = this.CheckIsFromWeiXin("", state, out strError);
            //if (nRet == -1 && strError!="未登录1")
            //    goto ERROR1;

            

            // 临时id
            //string guid = Guid.NewGuid().ToString();
            string weixinId = "temp" ;//这是时间还得用temp因为还没有登录成功，只是做一些初始化设置，后面 "~~" + guid; //2018/3/8


            // 初始化session
            string state = "ilovelibrary";
            SessionInfo sessionInfo = null;
           int nRet = this.InitSession(state, weixinId, out sessionInfo, out strError);
            if (nRet == -1)
                goto ERROR1;

            // 初始化 viewbag
            nRet = this.InitViewBag(sessionInfo, out strError);
            if (nRet == -1)
                goto ERROR1;

            return View();


            ERROR1:
            ViewBag.Error = strError;
            return View();
        }
        */

        /// <summary>
        /// 账户管理
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult Index(string code, string state,string myWeixinId)
        {
            string strError = "";
            int nRet = 0;

            // 检查当前是否已经选择了图书馆绑定了帐号
            WxUserItem activeUser = null;
            nRet = this.GetActive(code, state, 
                out  activeUser,
                out strError,
                myWeixinId);
            if (nRet == -1)
            {
                goto ERROR1;
            }
            if (nRet == 0)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state,"/Account/Index");
                return View();
            }

            //dp2WeiXinService.Instance.WriteLog1("Index页面，获取完当前对象。");

            // 检查微信id是否已经绑定的读者
            string weixinId = ViewBag.weixinId; //(string)Session[WeiXinConst.C_Session_WeiXinId];
            

            //dp2WeiXinService.Instance.WriteLog1("Index页面，检查绑了" + userList.Count + "对象。");

            if (activeUser.type == WxUserDatabase.C_Type_Worker && activeUser.userName == "public")
            {
                List<WxUserItem> userList = WxUserDatabase.Current.Get(weixinId, null, -1);
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
        public ActionResult Bind(string code, string state, string returnUrl,string from)
        {
            ViewBag.ReturnUrl = returnUrl;

            string strError = "";
            int nRet = 0;

            // 检查当前是否已经选择了图书馆绑定了帐号
            WxUserItem activeUser = null;
            nRet = this.GetActive(code, state,
                out activeUser,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }
            if (nRet == 0)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Accout/Bind");
                return View();
            }

            ViewBag.fromUrl = "/Account/Bind";

            return View();

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }

        // 柜台绑定
        public ActionResult ScanQRCodeBind(string code, string state,
            string libId)
        {

            string strError = "";
            int nRet = 0;

            // 检查当前是否已经选择了图书馆绑定了帐号
            WxUserItem activeUser = null;
            nRet = this.GetActive(code, state,
                out activeUser,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }
            if (nRet == 0)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Accout/ScanQRCodeBind");
                return View();
            }

            if (libId == null)
                libId = "";

            // 图书馆html
            string weixinId = ViewBag.weixinId; //(string)Session[WeiXinConst.C_Session_WeiXinId];


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
            string strError = "";
            int nRet = 0;

            // 检查当前是否已经选择了图书馆绑定了帐号
            WxUserItem activeUser = null;
            nRet = this.GetActive(code, state, 
                out activeUser,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }
            if (nRet == 0)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state,"/Account/ResetPassword");
                return View();
            }

            if (libId == null)
                libId = "";

            // 图书馆html
            string weixinId = ViewBag.weixinId; //(string)Session[WeiXinConst.C_Session_WeiXinId];
      
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
            string strError = "";
            int nRet = 0;

            // 检查当前是否已经选择了图书馆绑定了帐号
            WxUserItem activeUser = null;
            nRet = this.GetActive(code, state, 
                out activeUser,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }
            if (nRet == 0)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Account/ChangePassword");
                return View();
            }

            if (String.IsNullOrEmpty(patronBarcode) == true)
            {
                string weixinId = ViewBag.weixinId; //(string)Session[WeiXinConst.C_Session_WeiXinId];
                WxUserItem userItem = WxUserDatabase.Current.GetActive(weixinId);
                if (userItem.type == WxUserDatabase.C_Type_Worker)
                {
                    strError = "当前用户不可能是工作人员";
                    goto ERROR1;
                }
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