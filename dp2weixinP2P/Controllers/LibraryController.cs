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
        public ActionResult MsgManage(string code, string state,string group)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            if (String.IsNullOrEmpty(group) == true)
                group = dp2WeiXinService.C_GroupName_Bb;

            if (group != dp2WeiXinService.C_GroupName_Bb
                && group != dp2WeiXinService.C_GroupName_Book)
            {
                return Content("不支持的群" + group);
            }




            // 图书馆html
            ViewBag.LibHtml = this.GetLibHtml("");

            ViewBag.group = group;
            if (group == dp2WeiXinService.C_GroupName_Bb)
                ViewBag.groupTitle = "公告";
            else
                ViewBag.groupTitle = "新书推荐";

            return View();
        }

        // 图书馆主页
        public ActionResult HomePage(string code, string state, string admin, string weiXinId)
        {
            if (String.IsNullOrEmpty(admin) == false && admin == "1")
            {
                Session["userType"] = "admin";
            }
            // 用于测试，如果传了一个weixin id参数，则存到session里
            if (String.IsNullOrEmpty(weiXinId) == false)
            {
                // 记下微信id
                Session[WeiXinConst.C_Session_WeiXinId] = weiXinId;
            }

            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            // 图书馆html
            ViewBag.LibHtml = this.GetLibHtml("");

            return View();
        }


        // 好书推荐
        public ActionResult BookSubject(string code, string state,string libId)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            // 图书馆html
             ViewBag.LibHtml = this.GetLibHtml(libId);

            return View();
        }


        public ActionResult BookMsg(string code, string state,
            string libId,
            string userName,
            string subject)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            if (String.IsNullOrEmpty(libId) == true)
            {
                return Content("libId参数不能为空");
            }
            //if (String.IsNullOrEmpty(userName) == true)
            //{
            //    return Content("userName参数不能为空");
            //}
            if (String.IsNullOrEmpty(subject) == true)
            {
                return Content("subject参数不能为空");
            }

            ViewBag.LibId = libId;
            ViewBag.userName = userName;
            ViewBag.subject = subject;

            List<MessageItem> list = new List<MessageItem>();
            nRet = dp2WeiXinService.Instance.GetMessage(dp2WeiXinService.C_GroupName_Book,
        libId,
        "",
        subject,
        "browse",
        out list,
        out strError);
            //nRet = dp2WeiXinService.Instance.GetBookMsg(libId, 
            //    subject, 
            //    out list,
            //    out strError);
            if (nRet == -1)
                return Content(strError);

            return View(list);
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

            // 栏目html
            ViewBag.SubjectHtml = this.GetSubjectHtml(dp2WeiXinService.C_GroupName_Book,
                libId, subject, true);

            // 将这些参数值设到model上，这里可以回传返回
            BookEditModel model = new BookEditModel();
            model._libId = libId;
            model._userName = userName;
            model._subject = subject;
            model._returnUrl = returnUrl;


            if (string.IsNullOrEmpty(msgId) == false)
            {
                List<MessageItem> list = null;
                nRet = dp2WeiXinService.Instance.GetMessage(dp2WeiXinService.C_GroupName_Book,
                    libId,
                    msgId,
                    "",
                    "original",
                    out list,
                    out strError);
                if (nRet == -1)
                    return Content(strError);

                if (list != null && list.Count == 1)
                {
                    MessageItem item = list[0];
                    model.id = item.id;
                    model.title = item.title;
                    model.remark = item.remark;
                    model.content = item.content;
                    model.creator = item.creator;
                    model.publishTime = item.publishTime;
                }
                else
                {
                    return Content("根据id获取消息异常，未找到或者条数不对");
                }
            }
            


            // todo 根据id获取消息  //msgId
            if (String.IsNullOrEmpty(biblioPath) == false)
                model.content = biblioPath;

            //model.msgItem = item;
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult BookEdit(BookEditModel model)
        {
            string strError = "";
            // 实际保存
            string libId = model._libId;
            string userName = model._userName;
            string subject =model._subject;
            string returnUrl1 = model._returnUrl;

            MessageItem returnItem = null;

            MessageItem item = new MessageItem();
            item.id = model.id;
            item.title = model.title;
            item.content = model.content;
            item.remark = model.remark;
            item.creator = model._userName;
            item.subject = model.subject;
            if (String.IsNullOrEmpty(item.id) == true)
            {
                int nRet =dp2WeiXinService.Instance.CoverMessage(dp2WeiXinService.C_GroupName_Book,
                    libId,
                    item,
                    "create",
                    out returnItem,
                    out strError);
                if (nRet == -1)
                    return Content(strError);
            }
            else 
            {
                int nRet = dp2WeiXinService.Instance.CoverMessage(dp2WeiXinService.C_GroupName_Book,
                    libId,
                    item,
                    "change",
                    out returnItem,
                    out strError);
                if (nRet == -1)
                    return Content(strError);
            }


            // 如果没有传入返回路径，保存完转到BookSubject
            if (String.IsNullOrEmpty(model._returnUrl) == true)
            {
                return this.RedirectToAction("BookSubject", "Library");
            }
            else
            {
                if (model._returnUrl == "/Biblio/Index")  // 直接跳转没有数据,改为javascript返回，注意是-2
                    return Content("<script>window.history.go(-2);</script>");
                else
                {
                    string url =Url.Content("~" + model._returnUrl);
                    return Redirect(url);
                }
            }

            //// 如果我们进行到这一步时某个地方出错，则重新显示表单
            //ModelState.AddModelError("", strError);//"提供的用户名或密码不正确。");
            //return View(model);

        }



        private string GetLibHtml(string libId)
        {
            if (String.IsNullOrEmpty(libId) == true)
            {
                // 找工作人员帐户
                string weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
                WxUserItem userItem = WxUserDatabase.Current.GetOneWorker(weiXinId);
                if (userItem == null)
                {
                    // 找读者帐户
                    userItem = WxUserDatabase.Current.GetActivePatron(weiXinId);
                }
                if (userItem != null)
                    libId = userItem.libId;
            }

            return this.GetLibSelectHtml(libId);
        }


        private string GetSubjectHtml(string group, string libId, string selSubject, bool bNew)
        {

            string strError="";
            List<SubjectItem> list = null;
            int nRet = dp2WeiXinService.Instance.GetSubject(libId, group, out list, out strError);
            if (nRet == -1)
            {
                return "获取好书推荐的栏目出错";
            }

            var opt = "<option value=''>请选择 栏目</option>";
            for (var i = 0; i < list.Count; i++)
            {
                SubjectItem item = list[i];
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