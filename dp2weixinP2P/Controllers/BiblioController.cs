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

        public ActionResult ViewPDF(string libId, string uri)
        {
            ViewBag.libId = libId;
            ViewBag.objectUri = uri;


            string strError = "";
            string filename = "";
            int totalPage = dp2WeiXinService.Instance.GetPDFCount(libId, uri, out filename, out strError);
            ViewBag.pageCount = totalPage;

            string strImgUri = uri + "/page:1,format:jpeg,dpi:75";
            ViewBag.firstUrl = "../patron/getphoto?libId=" + HttpUtility.UrlEncode(libId)
                            + "&objectPath=" + HttpUtility.UrlEncode(strImgUri);
            return View();
        }



            // 书目查询主界面
            public ActionResult Index(string code, string state)
        {
            // 登录检查
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Biblio/Index?a=1");// ("书目查询", "/Biblio/Index?a=1", lib.libName);
                return View();
            }

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


            WxUserItem userItem1 = WxUserDatabase.Current.GetActive(weixinId);
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state,"/Biblio/Detail");
                return View();
            }

            if (activeUser != null)
            {
                ViewBag.PatronBarcode = activeUser.readerBarcode;
            }

            ViewBag.BiblioPath = biblioPath;

            return View();

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }


    }
}