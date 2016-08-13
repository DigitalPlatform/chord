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
        public ActionResult BB(string code, string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            //绑定的工作人员账号 需要有权限
            string userName = "";
            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            string libId = ViewBag.LibId;
            // 查找当前微信用户绑定的工作人员账号
            WxUserItem user = WxUserDatabase.Current.GetWorker(weixinId, libId);
            // todo 后面可以放开对读者的权限
            if (user != null)
            {
                // 检索是否有权限 _wx_setHomePage
                string needRight = dp2WeiXinService.C_Right_SetBb;
                LibItem lib = LibDatabase.Current.GetLibById(libId);
                if (lib == null)
                {
                    strError = "未找到id为[" + libId + "]的图书馆定义。";
                    goto ERROR1;
                }

                int nHasRights = dp2WeiXinService.Instance.CheckRights(lib.capoUserName,
                    user.userName,
                    needRight,
                    out strError);
                if (nHasRights == -1)
                {
                    goto ERROR1;
                }
                if (nHasRights == 1)
                {
                    userName = user.userName;
                }
                else
                {
                    userName = "";
                }
            }
            ViewBag.userName = userName;


            // 获取消息
            List<MessageItem> list = null;
            nRet =dp2WeiXinService.Instance.GetMessage(dp2WeiXinService.C_Group_Bb,
                libId,
                "",
                "",
                "browse",
                out list,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return View(list);

        ERROR1:
            return Content(strError);
        }

        // 公告
        public ActionResult MsgManage(string code, string state,string group)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            if (String.IsNullOrEmpty(group) == true)
                group = dp2WeiXinService.C_Group_Bb;

            if (group != dp2WeiXinService.C_Group_Bb
                && group != dp2WeiXinService.C_Group_Book)
            {
                return Content("不支持的群" + group);
            }




            // 图书馆html
            //ViewBag.LibHtml = this.GetLibHtml("");

            ViewBag.group = group;
            if (group == dp2WeiXinService.C_Group_Bb)
                ViewBag.groupTitle = "公告";
            else
                ViewBag.groupTitle = "新书推荐";

            return View();
        }

        // 图书馆主页
        public ActionResult Home(string code, string state, string weixinId)
        {
            // 用于测试，如果传了一个weixin id参数，则存到session里
            if (String.IsNullOrEmpty(weixinId) == false)
            {
                // 记下微信id
                Session[WeiXinConst.C_Session_WeiXinId] = weixinId;
            }

            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            //绑定的工作人员账号 需要有权限
            string userName = "";
            weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            string libId = ViewBag.LibId;
            // 查找当前微信用户绑定的工作人员账号
            WxUserItem user = WxUserDatabase.Current.GetWorker(weixinId, libId);
            // todo 后面可以放开对读者的权限
            if (user != null)
            {
                // 检索是否有权限 _wx_setHomePage
                string needRight = dp2WeiXinService.C_Right_SetHomePage;
                LibItem lib = LibDatabase.Current.GetLibById(libId);
                if (lib == null)
                {
                    strError = "未找到id为[" + libId + "]的图书馆定义。";
                    goto ERROR1;
                }

                int nHasRights = dp2WeiXinService.Instance.CheckRights(lib.capoUserName,
                    user.userName,
                    needRight,
                    out strError);
                if (nHasRights == -1)
                {
                    goto ERROR1;
                }
                if (nHasRights == 1)
                {
                    userName = user.userName;
                }
                else
                {
                    userName = "";
                }
            }
            ViewBag.userName = userName;

            // 获取栏目
            List<SubjectItem> list = null;
            nRet = dp2WeiXinService.Instance.GetSubject(libId, 
                dp2WeiXinService.C_Group_HomePage,
                out list, out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            return View(list);

        ERROR1:

            ViewBag.Error = strError;
            return View();//Content(strError);
        }


        // 好书推荐
        public ActionResult BookSubject(string code, string state,string libId)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            //绑定的工作人员账号 需要有权限
            string userName = "";
            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            if (String.IsNullOrEmpty(libId)==true)
                libId = ViewBag.LibId;
            // 查找当前微信用户绑定的工作人员账号
            WxUserItem user = WxUserDatabase.Current.GetWorker(weixinId, libId);
            // 2016-8-13 加了当前工作所在图书馆与设置图书馆的判断
            if (user != null && user.libId== libId)
            {
                // 检索是否有权限 _wx_setHomePage
                string needRight = dp2WeiXinService.C_Right_SetBook;
                LibItem lib = LibDatabase.Current.GetLibById(libId);
                if (lib == null)
                {
                    strError = "未找到id为[" + libId + "]的图书馆定义。";
                    goto ERROR1;
                }

                int nHasRights = dp2WeiXinService.Instance.CheckRights(lib.capoUserName,
                    user.userName,
                    needRight,
                    out strError);
                if (nHasRights == -1)
                {
                    goto ERROR1;
                }
                if (nHasRights == 1)
                {
                    userName = user.userName;
                }
                else
                {
                    userName = "";
                }
            }
            ViewBag.userName = userName;

            // 获取栏目
            List<SubjectItem> list = null;
            nRet = dp2WeiXinService.Instance.GetSubject(libId,
                dp2WeiXinService.C_Group_Book,
                out list, out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            return View(list);

        ERROR1:
            return Content(strError);
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
            nRet = dp2WeiXinService.Instance.GetMessage(dp2WeiXinService.C_Group_Book,
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

            if (String.IsNullOrEmpty(subject) == true)
            {
                subject = ViewBag.remeberBookSubject;
            }

            // 栏目html
            ViewBag.SubjectHtml = dp2WeiXinService.Instance.GetSubjectHtml( libId,
                dp2WeiXinService.C_Group_Book,
               subject,
               true,
               null);

            // 将这些参数值设到model上，这里可以回传返回
            BookEditModel model = new BookEditModel();
            model._libId = libId;
            model._userName = userName;
            model._subject = subject;
            model._returnUrl = returnUrl;


            if (string.IsNullOrEmpty(msgId) == false)
            {
                List<MessageItem> list = null;
                nRet = dp2WeiXinService.Instance.GetMessage(dp2WeiXinService.C_Group_Book,
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
                int nRet =dp2WeiXinService.Instance.CoverMessage(dp2WeiXinService.C_Group_Book,
                    libId,
                    item,
                    "create",
                    "",//parameters
                    out returnItem,
                    out strError);
                if (nRet == -1)
                    return Content(strError);
            }
            else 
            {
                int nRet = dp2WeiXinService.Instance.CoverMessage(dp2WeiXinService.C_Group_Book,
                    libId,
                    item,
                    "change",
                    "",//parameters
                    out returnItem,
                    out strError);
                if (nRet == -1)
                    return Content(strError);
            }

            // 2016-8-13 jane 记住选择的subject
            if (model.subject != ViewBag.remeberBookSubject)
            {
                ViewBag.remeberBookSubject = model.subject;

                // todo 保存到mongo库里
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




    }
}