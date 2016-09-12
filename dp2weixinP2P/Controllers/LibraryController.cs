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
        public ActionResult BB(string code, string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                goto ERROR1;

            //绑定的工作人员账号 需要有权限
            string userName = "";
            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            string libId = ViewBag.LibId;

            if (weixinId == dp2WeiXinService.C_Supervisor)
            {
                userName = weixinId;
            }
            else
            {
                // 查找当前微信用户绑定的工作人员账号
                WxUserItem user = WxUserDatabase.Current.GetWorker(weixinId, libId);
                // todo 后面可以放开对读者的权限
                if (user != null)
                {
                    // 检索是否有权限 _wx_setHomePage
                    string needRight = dp2WeiXinService.C_Right_SetBb;
                    LibEntity lib = LibDatabase.Current.GetLibById(libId);
                    if (lib == null)
                    {
                        strError = "未找到id为[" + libId + "]的图书馆定义。";
                        goto ERROR1;
                    }

                    int nHasRights = dp2WeiXinService.Instance.CheckRights(lib,
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
            }

            //设到ViewBag里
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
            ViewBag.Error = strError;
            return View();
        }


        // 图书馆介绍
        public ActionResult Home(string code, string state, string weixinId)
        {
            // 如果是超级管理员，支持传一个weixin id参数
            if (String.IsNullOrEmpty(weixinId) == false)
            {
                if (this.CheckSupervisorLogin() == true)
                {
                    // 记下微信id
                    Session[WeiXinConst.C_Session_WeiXinId] = weixinId;
                }
                else
                {
                    // 转到登录界面
                    return Redirect("~/Home/Login?returnUrl=" + HttpUtility.UrlEncode("~/Library/Home?weixinId="+weixinId));
                }
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

            // 微信id
            weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];

            // 图书馆id
            string libId = ViewBag.LibId;


            // 2016-8-24 超级管理员可修改任何图书馆的介绍与公告
            if (weixinId ==dp2WeiXinService.C_Supervisor)
            {
                userName = weixinId;
            }
            else
            {
                // 查找当前微信用户绑定的工作人员账号
                WxUserItem user = WxUserDatabase.Current.GetWorker(weixinId, libId);
                // todo 后面可以放开对读者的权限
                if (user != null)
                {
                    // 检索是否有权限 _wx_setHomePage
                    string needRight = dp2WeiXinService.C_Right_SetHomePage;
                    LibEntity lib = LibDatabase.Current.GetLibById(libId);
                    if (lib == null)
                    {
                        strError = "未找到id为[" + libId + "]的图书馆定义。";
                        goto ERROR1;
                    }

                    int nHasRights = dp2WeiXinService.Instance.CheckRights(lib,
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
            }

            // 设到ViewBag
            ViewBag.userName = userName;

            // 获取栏目
            List<SubjectItem> list1 = null;
            nRet = dp2WeiXinService.Instance.GetSubject(libId, 
                dp2WeiXinService.C_Group_HomePage,
                out list1, out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }
            List<SubjectItem> list = new List<SubjectItem>();
            foreach (SubjectItem item in list1)
            {
                if (item.count == 0)
                    continue;
                list.Add(item);
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
                goto ERROR1;

            // weixin id 与图书馆id
            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            if (String.IsNullOrEmpty(libId)==true)
                libId = ViewBag.LibId;

            // 如果当前图书馆是不公开书目，则出现提示
            LibEntity lib = LibDatabase.Current.GetLibById(ViewBag.LibId);
            if (lib == null)
            {
                strError = "未设置当前图书馆。";
                goto ERROR1;
            }
            if (lib.noShareBiblio == 1)
            {
                List<WxUserItem> users = WxUserDatabase.Current.Get(weixinId, lib.id, -1);
                if (users.Count == 0)
                {
                    ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("好书推荐", "/Library/BookSubject", lib.libName);
                    return View();
                }
            }


            //绑定的工作人员账号 需要有权限
            // 查找当前微信用户绑定的工作人员账号
            string userName = "";
            WxUserItem user = WxUserDatabase.Current.GetWorker(weixinId, libId);
            // 2016-8-13 加了当前工作所在图书馆与设置图书馆的判断
            if (user != null && user.libId== libId)
            {
                // 检索是否有权限 _wx_setHomePage
                string needRight = dp2WeiXinService.C_Right_SetBook;
                int nHasRights = dp2WeiXinService.Instance.CheckRights(lib,
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
            ViewBag.Error = strError;
            return View();//Content(strError);
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
            {
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(libId) == true)
            {
                strError = "libId参数不能为空";
                goto ERROR1;
            }
            if (String.IsNullOrEmpty(subject) == true)
            {
                strError = "subject参数不能为空";
                goto ERROR1;
            }

            // 如果当前图书馆是不公开书目，则出现提示
            LibEntity lib = LibDatabase.Current.GetLibById(libId);
            if (lib == null)
            {
                strError = "未设置当前图书馆。";
                goto ERROR1;
            }
            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            if (lib.noShareBiblio == 1)
            {
                List<WxUserItem> users = WxUserDatabase.Current.Get(weixinId, lib.id, -1);
                if (users.Count == 0)
                {
                    ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("好书推荐", "/Library/BookSubject", lib.libName);
                    return View();
                }
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
            if (nRet == -1)
                goto ERROR1;

            return View(list);

        ERROR1:
            ViewBag.Error = strError;
            return View();//Content(strError);
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
            {
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(libId) == true)
            {
                strError = "libId参数不能为空。";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(userName) == true)
            {
                strError = "userName参数不能为空。";
                goto ERROR1;
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
                {
                    goto ERROR1;
                }

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
                    strError = "根据id获取消息异常，未找到或者条数不对";
                    goto ERROR1;
                }
            }
            


            // todo 根据id获取消息  //msgId
            if (String.IsNullOrEmpty(biblioPath) == false)
                model.content = biblioPath;

            //model.msgItem = item;
            return View(model);

        ERROR1:
            ViewBag.Error = strError;
            return View();//Content(strError);
        }




    }
}