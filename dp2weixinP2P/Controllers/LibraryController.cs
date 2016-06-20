using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace dp2weixinWeb.Controllers
{
    public class LibraryController : BaseController
    {
        // GET: Library
        public ActionResult Bb(string code, string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            string weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            WxUserItem userItem = WxUserDatabase.Current.GetActivePatron(weiXinId);
            if (userItem == null)
            {
                // 找工作人员帐户
                userItem = WxUserDatabase.Current.GetOneWorker(weiXinId);
            }
            string selLibId = "";
            if (userItem != null)
                selLibId=userItem.libId;

            List<LibItem> list = LibDatabase.Current.GetLibs();//"*", 0, -1).Result;
            var opt = "<option value=''>请选择 图书馆</option>";
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                string selectedString = "";
                if (selLibId != "" && selLibId == item.id)
                {
                    selectedString = " selected='selected' ";
                }
                opt += "<option value='"+item.id+"' " + selectedString + ">" + item.libName + "</option>";
            }
            ViewBag.LibHtml = "<select id='selLib' style='padding-left: 0px;width: 65%;'   data-bind=\"optionsCaption:'请选择 图书馆'\">" + opt + "</select>";
            //

  

            List<BbItem> annlist = new List<BbItem>();
            if (userItem != null)
            {
                nRet = dp2WeiXinService.Instance.GetBbs(selLibId, out annlist, out strError);
                if (nRet == -1)
                {
                    return Content(strError);
                }
            }
            return View(annlist);
        }



        // GET: Library
        public ActionResult BbManage(string code, string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            // 找工作人员帐户
            string weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            WxUserItem worker = WxUserDatabase.Current.GetOneWorker(weiXinId);
            if (worker == null)
            {
                string returnUrl = "/Library/BbManage";
                string bindUrl = "/Account/Bind?returnUrl=" + HttpUtility.UrlEncode(returnUrl);
                string bindLink = "请先点击<a href='javascript:void(0)' onclick='gotoUrl(\"" + bindUrl + "\")'>这里</a>进行绑定。";
                string strRedirectInfo = "您尚未绑定工作人员帐号，不能管理公告，" + bindLink;
                strRedirectInfo = "<div class='mui-content-padded' style='color:#666666'>"
                    + strRedirectInfo
                    + "</div>";

                ViewBag.RedirectInfo = strRedirectInfo;
            }
            else
            {
                ViewBag.LibName = worker.libName;
                ViewBag.LibId = worker.libId;
                ViewBag.Creator = worker.userName;
            }




            return View();
        }
    }
}