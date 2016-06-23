using dp2weixin.service;
using dp2weixinWeb.Models;
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

        // 好书推荐
        public ActionResult BookSubject(string code, string state)
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


        public ActionResult BookMsg(string code, string state,string libId,string userName,string subject)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);




            return View();
        }


        /// <summary>
        /// 新增推荐 编辑界面
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult BookEdit(string code, string state,
            string libId,
            string userName,
            string subject,
            string msgId,
            string biblioPath,
            string returnUrl)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            if (String.IsNullOrEmpty(libId) == true)
            {
                return Content("libId参数不能为空。");
            }

            if (String.IsNullOrEmpty(userName) == true)
            {
                return Content("userName参数不能为空。");
            }

            // 将这些值设到ViewTag
            ViewBag.libId = libId;
            ViewBag.userName = userName;
            ViewBag.ReturnUrl = returnUrl;

            BookEditModel model = new BookEditModel();
            model.libId = libId;
            model.userName = userName;
            model.subject = subject;
            model.returnUrl = "";

            // 栏目html
            ViewBag.SubjectHtml = this.GetSubjectHtml(libId,subject,true);


            // todo 根据id获取消息  //msgId
            MessageItem item = new MessageItem();
            if (String.IsNullOrEmpty(biblioPath) == false)
                item.content = biblioPath;

            model.msgItem = item;
            return View(model);
        }

        [HttpPost]
        public ActionResult BookEdit(MessageItem item, string returnUrl)
        {
            // 实际保存

            // 如果没有传入返回路径，保存完转到BookSubject
            if (String.IsNullOrEmpty(returnUrl) == true)
            {
                return this.RedirectToAction("BookSubject", "Library");
            }
            else
            {
                if (returnUrl == "/Biblio/Index")  // 直接跳转没有数据,改为javascript返回，注意是-2
                    return Content("<script>window.history.go(-2);</script>");
                else
                    return Redirect(returnUrl);
            }
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
            string libHtml= "<select id='selLib' style='padding-left: 0px;width: 65%;'  >" + opt + "</select>";
            return libHtml;
        }


        private string GetSubjectHtml(string libId, string selSubject, bool bNew)
        {

            string strError="";
            List<BookSubjectItem> list = null;
            int nRet = dp2WeiXinService.Instance.GetBookSubject(libId, out list, out strError);
            if (nRet == -1)
            {
                return "获取好书推荐的栏目出错";
            }

            var opt = "<option value=''>请选择 栏目</option>";
            for (var i = 0; i < list.Count; i++)
            {
                BookSubjectItem item = list[i];
                string selectedString = "";
                if (selSubject != "" && selSubject == item.name)
                {
                    selectedString = " selected='selected' ";
                }
                opt += "<option value='" + item.name+ "' " + selectedString + ">" + item.name + "</option>";
            }

            string onchange = "";
            if (bNew == true)
            {
                opt += "<option value='new'>自定义栏目</option>";
                onchange = " onchange='subjectChanged()' ";
            }

            string subjectHtml = "<select id='selSubject' style='padding-left: 0px;width: 65%;'  "+onchange+" >" + opt + "</select>";


            return subjectHtml;
        }
    }
}