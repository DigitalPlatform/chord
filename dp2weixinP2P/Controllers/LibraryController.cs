using dp2weixin.service;
using dp2weixinWeb.Models;
using Senparc.Weixin.MP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace dp2weixinWeb.Controllers
{
    public class LibraryController : BaseController
    {
        // 操作类型常量字符串
        const string C_ope_borrow = "borrow";
        const string C_ope_return = "return";
        const string C_ope_searchItem = "searchItem";

        // 专业借还流程
        // operationType 操作类型
        public ActionResult Circulate(string code, string state)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/Circulate");
                return View();
            }

            // public帐号不支持专业借还
            if (sessionInfo.ActiveUser.userName == WxUserDatabase.C_Public)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("专业借还", "/Library/Circulate", true);
                return View();
            }

            // 是否校验条码
            ViewBag.verifyBarcode = sessionInfo.ActiveUser.verifyBarcode;
            ViewBag.audioType = sessionInfo.ActiveUser.audioType;

            // 关注馆藏地，转成显示格式
            ViewBag.Location = SubLib.ParseToView(sessionInfo.ActiveUser.selLocation);

            // 需要有借还权限
            bool canBorrow = true;
            bool canReturn = true;
            if (sessionInfo.ActiveUser.rights.Contains("borrow") == false)
                canBorrow = false;
            if (sessionInfo.ActiveUser.rights.Contains("return") == false)
                canReturn = false;
            ViewBag.canBorrow = canBorrow;
            ViewBag.canReturn = canReturn;
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            ViewBag.version = version.ToString();
            return View(sessionInfo.ActiveUser);
        }




        public ActionResult Message(string code, string state)
        {

            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/Message");
                return View();
            }

            //ViewBag.userId = sessionInfo.ActiveUser.id; 在base页面中已有

            /*
<root>
<first>▉▊▋▍▎▉▊▋▍▎▉▊▋▍▎</first>
<keyword1>文心雕龙义证 [专著]  </keyword1>
<keyword2>B001(本地图书馆/流通库)</keyword2>
<keyword3>2023/05/24</keyword3>
<keyword4>31天</keyword4>
<keyword5>2023/05/24 test P001(本地图书馆)</keyword5>
<remark>test P001(本地图书馆)，感谢还书。</remark>
</root>
             */
            // 获取消息
            List<UserMessageItem> list =  UserMessageDb.Current.GetByUserId(sessionInfo.ActiveUser.id);


            List<UserMessageMode> modelist = new List<UserMessageMode>();
            foreach (UserMessageItem item in list)
            {
                modelist.Add(new UserMessageMode(item));
            }

            return View(modelist);
        }




        // 借还窗
        public ActionResult Charge2(string code, string state)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/Charge2");
                return View();
            }

            bool canBorrow = true;
            bool canReturn = true;
            if (sessionInfo.ActiveUser.userName== WxUserDatabase.C_Public)
            {
                canBorrow = false;
                canReturn = false;
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("借还窗", "/Library/Charge2", true);
                return View();
            }
            if (sessionInfo.ActiveUser.type == WxUserDatabase.C_Type_Worker)
            {
                if (sessionInfo.ActiveUser.rights.Contains("borrow") == false)
                    canBorrow = false;
                if (sessionInfo.ActiveUser.rights.Contains("return") == false)
                    canReturn = false;
            }
            else 
            {
                //读者如果有权限可以借还，但都不可以还书。
                if (sessionInfo.ActiveUser.rights.Contains("return") == false)
                    canReturn = false;

                ViewBag.patronBarcode = sessionInfo.ActiveUser.readerBarcode;
            }

            if (canBorrow == false)
                ViewBag.canBorrow = "disabled";
            if (canReturn == false)
                ViewBag.canReturn = "disabled";

            //设到ViewBag里
            string userName = "";
            if (sessionInfo.ActiveUser.type == WxUserDatabase.C_Type_Worker)
            {
                userName = sessionInfo.ActiveUser.userName;
                ViewBag.isPatron = 0;
            }
            else
            {
                userName = sessionInfo.ActiveUser.readerBarcode;
                ViewBag.isPatron = 1;
            }

            ViewBag.userName = userName;
            //ViewBag.userId = sessionInfo.ActiveUser.id;  //在基类中已存在
            ViewBag.Location = SubLib.ParseToView(sessionInfo.ActiveUser.selLocation);
            ViewBag.verifyBarcode = sessionInfo.ActiveUser.verifyBarcode;
            ViewBag.audioType = sessionInfo.ActiveUser.audioType;
            return View(sessionInfo.ActiveUser);
        }
        
        // 公告
        public ActionResult BB(string code, string state)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/BB");
                return View();
            }

            //绑定的工作人员账号 需要有权限
            string userName = "";
            string weixinId = ViewBag.weixinId; 
            if (weixinId == dp2WeiXinService.C_Supervisor)
            {
                userName = weixinId;
            }
            else
            {
                userName = this.GetHasRightUserName(sessionInfo.ActiveUser, sessionInfo.CurrentLib);
            }

           // 2020/1/31 add,如果设置了不允许外部访问，则公告也不允许外部人员访问
            if (sessionInfo.CurrentLib.Entity.noShareBiblio == 1)
            {
                List<WxUserItem> users = WxUserDatabase.Current.Get(weixinId, sessionInfo.CurrentLib.Entity.id, -1);
                if (users.Count == 0
                    || (users.Count == 1 && users[0].userName == WxUserDatabase.C_Public))
                {
                    ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("公告", "/Library/BB", sessionInfo.CurrentLib.Entity.libName);
                    return View();
                }
            }

            //设到ViewBag里
            ViewBag.userName = userName;

            // 获取消息
            List<MessageItem> list = null;
            nRet =dp2WeiXinService.Instance.GetMessage(dp2WeiXinService.C_Group_Bb,
                sessionInfo.ActiveUser.libId+"/"+sessionInfo.ActiveUser.bindLibraryCode,
                "",
                "",
                "browse",
                out list,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            //// 2021/8/2 这里是dp2mserver的帐号
            //ViewBag.loginUserName = sessionInfo.CurrentLib.Entity.wxUserName;
            //ViewBag.loginUserType = "";

            return View(list);
        }


        private string GetHasRightUserName(WxUserItem activeUser,Library lib)
        {
            string userName = "";
            string strError = "";

            // 查找当前微信用户绑定的工作人员账号
            //WxUserItem user = WxUserDatabase.Current.GetWorker(weixinId, libId);
            // todo 后面可以放开对读者的权限
            if (activeUser != null 
                && activeUser.type==WxUserDatabase.C_Type_Worker
                && activeUser.userName != WxUserDatabase.C_Public)
            {
                // 检索是否有权限 _wx_setHomePage
                string needRight = dp2WeiXinService.C_Right_SetBb;
                int nHasRights = dp2WeiXinService.Instance.CheckRights(activeUser,
                    lib.Entity,
                    needRight,
                    out strError);
                if (nHasRights == -1)
                {
                    dp2WeiXinService.Instance.WriteErrorLog("CheckRights()出错：" + strError);
                    userName = "";
                }
                if (nHasRights == 1)
                {
                    userName = activeUser.userName;
                }
                else
                {
                    userName = "";
                }
            }

            return userName;
        }

        // 图书馆介绍
        public ActionResult Home(string code, string state, string weixinId)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError,
                weixinId);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/Home");
                return View();
            }

            if (sessionInfo.CurrentLib == null)
            {
                ViewBag.Error = "您之前选择的图书馆已经不存在。";
                return View();
            }

            //绑定的工作人员账号 需要有权限
            string userName = "";
            // 2016-8-24 超级管理员可修改任何图书馆的介绍与公告
            if (weixinId == dp2WeiXinService.C_Supervisor)
            {
                userName = weixinId;
            }
            else
            {
                userName = this.GetHasRightUserName(sessionInfo.ActiveUser, sessionInfo.CurrentLib);
            }

            // 设到ViewBag
            ViewBag.userName = userName;

            // 获取栏目
            List<SubjectItem> list1 = null;
            nRet = dp2WeiXinService.Instance.GetSubject(sessionInfo.ActiveUser.libId,
                dp2WeiXinService.C_Group_HomePage,
                out list1, out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }


            List<SubjectItem> list = new List<SubjectItem>();
            foreach (SubjectItem item in list1)
            {
                if (item.count == 0)
                    continue;
                list.Add(item);
            }

            //// 2021/8/2 这里是dp2mserver的帐号
            //ViewBag.loginUserName = sessionInfo.CurrentLib.Entity.wxUserName;
            //ViewBag.loginUserType = "";

            return View(list);

        }

        // 图书馆介绍
        public ActionResult dpHome(string code, string state, string weixinId)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError,
                weixinId);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            //绑定的工作人员账号 需要有权限
            string userName = "";
            // 2016-8-24 超级管理员可修改任何图书馆的介绍与公告
            if (sessionInfo.ActiveUser!=null 
                && sessionInfo.ActiveUser.weixinId == dp2WeiXinService.C_Supervisor)
            {
                userName = weixinId;
            }
            // 设到ViewBag
            ViewBag.userName = userName;

            string libId = "";
            if (sessionInfo.ActiveUser != null)
                libId = sessionInfo.ActiveUser.libId;

            // 获取栏目
            List<SubjectItem> list1 = null;
            // 当是数字平台group时，libId可为空
            nRet = dp2WeiXinService.Instance.GetSubject(libId,
                dp2WeiXinService.C_Group_dp_home,
                out list1, 
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            List<SubjectItem> list = new List<SubjectItem>();
            foreach (SubjectItem item in list1)
            {
                if (item.count == 0)
                    continue;
                list.Add(item);
            }

            return View(list);
        }


        // 好书推荐
        public ActionResult BookSubject(string code, string state)
        {

            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/BookSubject");
                return View();
            }

            // 不支持外部访问
            if (sessionInfo.CurrentLib.Entity.noShareBiblio == 1)
            {
                List<WxUserItem> users = WxUserDatabase.Current.Get(sessionInfo.WeixinId, sessionInfo.CurrentLib.Entity.id, -1);
                if (users.Count == 0
                    ||( users.Count==1 && users[0].userName== WxUserDatabase.C_Public))
                {
                    ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("好书推荐", "/Library/BookSubject", sessionInfo.CurrentLib.Entity.libName);
                    return View();
                }
            }

            //绑定的工作人员账号 需要有权限
            // 查找当前微信用户绑定的工作人员账号
            string userName = this.GetHasRightUserName(sessionInfo.ActiveUser, sessionInfo.CurrentLib);
            ViewBag.userName = userName;

            // 获取栏目
            List<SubjectItem> list = null;
            nRet = dp2WeiXinService.Instance.GetSubject(sessionInfo.ActiveUser.libId,
                dp2WeiXinService.C_Group_Book,
                out list, out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            return View(list);
        }



        public ActionResult Book(string code, string state,
            string libId,
            string userName,
            string subject,
            string biblioPath,
            string isNew)
        {

            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/Book");
                return View();
            }


            if (String.IsNullOrEmpty(subject) == true)
            {
                subject = ViewBag.remeberBookSubject;
            }
            if (subject == null)
                subject = "";

            
            if (sessionInfo.CurrentLib.Entity.noShareBiblio == 1)
            {
                List<WxUserItem> users = WxUserDatabase.Current.Get(sessionInfo.ActiveUser.weixinId, sessionInfo.CurrentLib.Entity.id, -1);
                if (users.Count == 0
                 || (users.Count == 1 && users[0].userName == WxUserDatabase.C_Public))
                {
                    ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("好书推荐", "/Library/BookSubject", sessionInfo.CurrentLib.Entity.libName);
                    return View();
                }
            }

            // 是否新建
            if (isNew == "1")
            {
                isNew = "1";
                // 栏目html
                ViewBag.SubjectHtml = dp2WeiXinService.Instance.GetSubjectHtml(libId,
                    dp2WeiXinService.C_Group_Book,
                   subject,
                   true,
                   null);

                if (String.IsNullOrEmpty(biblioPath) == false)
                    ViewBag.content = biblioPath;
            }
            else
            {
                isNew = "0";
            }
            ViewBag.isNew = isNew;
            ViewBag.LibId = libId;
            ViewBag.userName = userName;
            ViewBag.subject = subject;

            int no=0;
            string right ="";
            dp2WeiXinService.Instance.SplitSubject(subject, out  no, out  right);
            ViewBag.pureSubject = right;
            List<MessageItem> list = new List<MessageItem>();

            if (String.IsNullOrEmpty(subject) == false)
            {
                nRet = dp2WeiXinService.Instance.GetMessage(dp2WeiXinService.C_Group_Book,
                    sessionInfo.ActiveUser.libId + "/" + sessionInfo.ActiveUser.bindLibraryCode,
                    "",
                    subject,
                    "browse",
                    out list,
                    out strError);
                if (nRet == -1)
                {
                    ViewBag.Error = strError;
                    return View();
                }
            }


            return View(list);

        }



        public ActionResult BookMsg(string code, string state,
            string libId,
            string userName,
            string subject)
        {

            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/BookMsg");
                return View();
            }

            if (String.IsNullOrEmpty(subject) == true)
            {
                strError = "subject参数不能为空";
                ViewBag.Error = strError;
                return View();
            }

            // 如果当前图书馆是不公开书目，则出现提示
            if (sessionInfo.CurrentLib.Entity.noShareBiblio == 1)
            {
                List<WxUserItem> users = WxUserDatabase.Current.Get(sessionInfo.WeixinId, sessionInfo.CurrentLib.Entity.id, -1);
                if (users.Count == 0
                 || (users.Count == 1 && users[0].userName == WxUserDatabase.C_Public))
                {
                    ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("好书推荐", "/Library/BookSubject", sessionInfo.CurrentLib.Entity.libName);
                    return View();
                }
            }

            ViewBag.LibId = libId;
            ViewBag.userName = userName;
            ViewBag.subject = subject;
            List<MessageItem> list = new List<MessageItem>();
            nRet = dp2WeiXinService.Instance.GetMessage(dp2WeiXinService.C_Group_Book,
                sessionInfo.ActiveUser.libId + "/" + sessionInfo.ActiveUser.bindLibraryCode,
                "",
                subject,
                "browse",
                out list,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

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

            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            // 当前帐号不存在，尚未选择图书馆
            if (sessionInfo.ActiveUser == null)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/BookEdit");
                return View();
            }


            if (String.IsNullOrEmpty(userName) == true)
            {
                strError = "userName参数不能为空。";
                ViewBag.Error = strError;
                return View();
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
                    sessionInfo.ActiveUser.libId + "/" + sessionInfo.ActiveUser.bindLibraryCode,
                    msgId,
                    "",
                    "original",
                    out list,
                    out strError);
                if (nRet == -1)
                {
                    ViewBag.Error = strError;
                    return View();
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
                    ViewBag.Error = strError;
                    return View();
                }
            }

            if (String.IsNullOrEmpty(biblioPath) == false)
                model.content = biblioPath;

            return View(model);
        }


    }
}