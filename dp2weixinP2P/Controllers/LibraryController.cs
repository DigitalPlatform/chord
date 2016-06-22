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
        // 公告
        public ActionResult MsgManage(string code, string state,string msgType)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            if (msgType != dp2WeiXinService.C_MsgType_Bb
                && msgType != dp2WeiXinService.C_MsgType_Book)
            {
                return Content("不支持的消息类型" + msgType);
            }

            // 图书馆html
            ViewBag.LibHtml = this.GetLibHtml();

            ViewBag.MsgType = msgType;
            if (msgType == dp2WeiXinService.C_MsgType_Bb)
                ViewBag.MsgTypeTitle = "公告";
            else
                ViewBag.MsgTypeTitle = "新书推荐";

            return View();
        }


        // 新书推荐
        public ActionResult Book(string code, string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            // 图书馆html
            ViewBag.LibHtml = this.GetLibHtml();

            return View();
        }


        private string GetLibHtml()
        {
            // 找工作人员帐户
            string weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            WxUserItem userItem = WxUserDatabase.Current.GetOneWorker(weiXinId);
            if (userItem == null)
            {
                // 找读者帐户
                userItem = WxUserDatabase.Current.GetActivePatron(weiXinId);
            }
            string selLibId = "";
            if (userItem != null)
                selLibId = userItem.libId;

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
                opt += "<option value='" + item.id + "' " + selectedString + ">" + item.libName + "</option>";
            }
            string libHtml= "<select id='selLib' style='padding-left: 0px;width: 65%;'   data-bind=\"optionsCaption:'请选择 图书馆'\">" + opt + "</select>";
            return libHtml;
        }
    }
}