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

            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            WxUserItem userItem1 = WxUserDatabase.Current.GetActivePatron(weixinId, ViewBag.LibId);
            // 增加userItem1.libId == ViewBag.LibId  2016-8-13 jane todo
            if (userItem1 != null) //&& userItem1.libId == ViewBag.LibId)
            {
                ViewBag.PatronBarcode = userItem1.readerBarcode;
            }

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

            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            WxUserItem userItem1 = WxUserDatabase.Current.GetActivePatron(weixinId,ViewBag.LibId);
            // patron.libId==libId  2016-8-13 jane todo 关于当前账户与设置图书馆这块内容要统一修改
            if (userItem1 != null)// && userItem1.libId== ViewBag.LibId)
            {
                ViewBag.PatronBarcode = userItem1.readerBarcode;
            }

            ViewBag.BiblioPath = biblioPath;

            return View();
        }


    }
}