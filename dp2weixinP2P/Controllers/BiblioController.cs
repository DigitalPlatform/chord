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

        public ActionResult ViewPDF(string libId,string uri, string page,string totalPage)
        {
            // 登录检查
            string strError = "";
            int nRet = 0;

            string strImgUri = uri+ "/page:" + page + ",format:jpeg,dpi:75";

            ViewBag.imgUrl = "../patron/getphoto?libId=" + HttpUtility.UrlEncode(libId)
                            + "&objectPath=" + HttpUtility.UrlEncode(strImgUri);

            int nPage = 0;
            Int32.TryParse(page, out nPage);

            if (nPage < 1)
                nPage = 1;

            ViewBag.page = page;
            ViewBag.totalPage = totalPage;

            ViewBag.fristUrl= "gotoUrl('/Biblio/ViewPDF?libid=" + libId + "&uri=" + uri + "&page=1&totalPage=" + totalPage + "')";
            ViewBag.prevUrl = "gotoUrl('/Biblio/ViewPDF?libid=" + libId + "&uri=" + uri + "&page="+(nPage-1)+ "&totalPage=" + totalPage + "') ";
            ViewBag.nextUrl = "gotoUrl('/Biblio/ViewPDF?libid=" + libId + "&uri=" + uri + "&page=" + (nPage + 1) + "&totalPage=" + totalPage + "') ";
            ViewBag.tailUrl = "gotoUrl('/Biblio/ViewPDF?libid=" + libId + "&uri=" + uri + "&page=" + (nPage + 1) + "&totalPage=" + totalPage + "')";

            return View();

            ERROR1:
            ViewBag.Error = strError;
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

            //    nRet = this.CheckLogin(code, state, out strError);
            //if (nRet == -1)
            //{
            //    goto ERROR1;
            //}
            //if (nRet == 0)
            //{
            //    return Redirect("~/Account/Bind?from=web");
            //}

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