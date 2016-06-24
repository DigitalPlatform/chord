using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dp2weixinWeb.Controllers
{
    public class BiblioController : BaseController
    {
        // 书目查询主界面
        public ActionResult Index(string code,string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            ViewBag.workerUserName = "";

            string weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            WxUserItem userItem1= WxUserDatabase.Current.GetActivePatron(weiXinId);
            string libId = "";
            if (userItem1 != null)
            {
                ViewBag.MyLibId = userItem1.libId;
                ViewBag.PatronBarcode = userItem1.readerBarcode;
                ViewBag.PatronName = userItem1.readerName;
                libId = userItem1.libId;
            }
            else
            {
                userItem1 = WxUserDatabase.Current.GetOneWorker(weiXinId);
                if (userItem1 != null)
                {
                    libId = userItem1.libId;
                    ViewBag.workerUserName = userItem1.userName;
                } 
            }

            ViewBag.LibHtml = this.GetLibSelectHtml(libId);

            return View();
        }

        // 书目查询主界面
        public ActionResult Detail(string code, string state,string biblioPath)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            string weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];

            ViewBag.BiblioPath = biblioPath;

            return View();
        }


    }
}