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
        public ActionResult Index(string code, string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                goto ERROR1;

            // 如果当前图书馆是不公开书目，则出现提示
            LibEntity lib = dp2WeiXinService.Instance.GetLibById(ViewBag.LibId);
            if (lib == null)
            {
                strError = "未设置当前图书馆。";
                goto ERROR1;
            }

            string weixinId = ViewBag.weixinId; // (string)Session[WeiXinConst.C_Session_WeiXinId];
            if (lib.noShareBiblio == 1)
            {
                List<WxUserItem> users = WxUserDatabase.Current.Get(weixinId, lib.id, -1);
                if (users.Count == 0)
                {
                    ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("书目查询", "/Biblio/Index?a=1",lib.libName);
                    return View();
                }
            }


            WxUserItem userItem1 = WxUserDatabase.Current.GetActivePatron(weixinId, ViewBag.LibId);
            if (userItem1 != null)
            {
                ViewBag.PatronBarcode = userItem1.readerBarcode;
            }

            string match = lib.match;
            if (String.IsNullOrEmpty(match) == true)
                match = "left";
            ViewBag.Match = match;
            return View();

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }

        // 书目查询主界面
        public ActionResult Detail(string code, string state,string biblioPath)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                goto ERROR1;

            string weixinId = ViewBag.weixinId; //(string)Session[WeiXinConst.C_Session_WeiXinId];
            WxUserItem userItem1 = WxUserDatabase.Current.GetActivePatron(weixinId,ViewBag.LibId);
            // patron.libId==libId  2016-8-13 jane todo 关于当前账户与设置图书馆这块内容要统一修改
            if (userItem1 != null)// && userItem1.libId== ViewBag.LibId)
            {
                ViewBag.PatronBarcode = userItem1.readerBarcode;
            }

            ViewBag.BiblioPath = biblioPath;

            return View();

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }


    }
}