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
            WxUserItem activeUser = null;
            try
            {
                // 检查当前是否已经选择了图书馆绑定了帐号
                
                nRet = this.GetActive(code, state,
                    out activeUser,
                    out strError);
                if (nRet == -1)
                {
                    goto ERROR1;
                }
                if (nRet == 0)
                {
                    ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/Circulate");
                    return View();
                }

                // 得到该微信用户绑定的账号
                if (activeUser == null || activeUser.userName == "public")
                {
                    ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("专业借还", "/Library/Circulate", true);
                    return View();
                }


                //===

                // 是否校验条码
                ViewBag.verifyBarcode = activeUser.verifyBarcode;
                ViewBag.audioType = activeUser.audioType;

                // 关注馆藏地，转成显示格式
                ViewBag.Location = SubLib.ParseToView(activeUser.selLocation);


                //===
                // 需要有权限
                bool canBorrow = true;
                bool canReturn = true;
                // 如果没有借还权限，不能操作
                if (activeUser != null)
                {
                    if (activeUser.rights.Contains("borrow") == false)
                    {
                        canBorrow = false;
                    }
                    if (activeUser.rights.Contains("return") == false)
                    {
                        canReturn = false;
                    }
                }

                // 放到ViewBag里，传到页面
                ViewBag.canBorrow = canBorrow;
                ViewBag.canReturn = canReturn;

                //// 没有权限时出现提示
                //if (canBorrow == false && operationType == C_ope_borrow)
                //{
                //    strError = "当前帐户"+userName+"没有借书权限";
                //    goto ERROR1;
                //}
                //if (canReturn == false && operationType == C_ope_return)
                //{
                //    strError = "当前帐户" + userName + "没有还书权限";
                //    goto ERROR1;
                //}

                //// 操作类型与输入框类型
                //ViewBag.operation = operationType;
                //if (operationType== C_ope_borrow)
                //    ViewBag.inputType = "1"; //1表示读者证条码，2表示册条码

                string a = "test";
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                ViewBag.version = version.ToString();
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

            return View(activeUser);

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }



        // 读者登记
        public ActionResult PatronEdit(string code, string state)
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state,"/Library/PatronEdit");
                return View();
            }



            //绑定的工作人员账号 需要有权限
            string weixinId = ViewBag.weixinId;//(string)Session[WeiXinConst.C_Session_WeiXinId];
            string libId = ViewBag.LibId;


            if (activeUser == null || activeUser.type!= WxUserDatabase.C_Type_Worker
                || activeUser.userName=="public")
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("读者登记", "/Library/PatronEdit", true);
                return View();
            }
            ViewBag.userName = activeUser.userName;

            // 读者类别
            string[] libraryList = activeUser.libraryCode.Split(new []{','});

            SessionInfo sessionInfo = this.GetSessionInfo1();
            string types = sessionInfo.readerTypes;
            string typesHtml = "";
            if (String.IsNullOrEmpty(types) == false)
            {
                string[] typeList = types.Split(new char[] { ',' });
                foreach (string type in typeList)
                {
                    // 如果这个类型的分馆 是当前帐户可用的分馆，才列出来
                    if (activeUser.libraryCode != "")
                    {
                        int nIndex = type.LastIndexOf("}");
                        if (nIndex > 0)
                        {
                            string left = type.Substring(0, nIndex);
                            nIndex = left.IndexOf("{");
                            if (nIndex != -1)
                            {
                                left = left.Substring(nIndex + 1);

                                if (libraryList.Contains(left) == true)
                                {
                                    typesHtml += "<option value='" + type + "'>" + type + "</option>";
                                }
                            }
                        }
                    }
                    else
                    {
                        typesHtml += "<option value='" + type + "'>" + type + "</option>";
                    }
                }
            }

            typesHtml = "<select id='selReaderType' name='selReaderType' class='selArrowRight'>"
                    +"<option value=''>请选择</option>"
                    + typesHtml
                    + "</select>";

            ViewBag.readerTypeHtml = typesHtml;
            
            // 目标数据库
            string dbs=sessionInfo.readerDbnames;
            string dbsHtml = "";
            if (String.IsNullOrEmpty(dbs) == false)
            {
                string[] dbList = dbs.Split(new char[] { ',' });
                foreach (string db in dbList)
                {
                    dbsHtml += "<option value='" + db + "'>" + db + "</option>";
                }
            }
            if (dbsHtml != "")
            {
                dbsHtml = "<select id='selDbName' name='selDbName' class='selArrowRight'>"
                    + "<option value=''>请选择</option>"
                    + dbsHtml
                    + "</select>";
            }
            ViewBag.readerDbnamesHtml = dbsHtml;




            return View();


        ERROR1:
            ViewBag.Error = strError;
            return View();
        }


        public ActionResult Message(string code, string state)
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/Message");
                return View();
            }
            


            // 获取消息
            List<UserMessageItem> list =  UserMessageDb.Current.GetByUserId(activeUser.weixinId);



            return View(list);


            ERROR1:
            ViewBag.Error = strError;
            return View();
        }

        // 内务
        public ActionResult SearchItem(string from,string word)
        {
            string strError = "";

            return View();
        }

        // 内务
        public ActionResult Charge2(string code, string state)
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/Charge2");
                return View();
            }

            //绑定的工作人员账号 需要有权限
            string weixinId = ViewBag.weixinId;//(string)Session[WeiXinConst.C_Session_WeiXinId];
            string libId = ViewBag.LibId;

            bool canBorrow = true;
            bool canReturn = true;


            if (activeUser == null)
            {
                strError = "当前活动帐户不存在";
                goto ERROR1;
            }

            //WxUserItem user = sessionInfo.Active;// WxUserDatabase.Current.GetWorker(weixinId, ViewBag.LibId);
            // 未绑定
            if (activeUser == null || activeUser.userName=="public")
            {
                canBorrow = false;
                canReturn = false;
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("借还窗", "/Library/Charge2", true);
                return View();
            }
            if (activeUser.rights.Contains("borrow") == false)
            {
                canBorrow = false;
            }
            if (activeUser.rights.Contains("return") == false)
            {
                canReturn = false;
            }



            if (canBorrow == false)
                ViewBag.canBorrow = "disabled";
            if (canReturn == false)
                ViewBag.canReturn = "disabled";

            LibEntity lib = dp2WeiXinService.Instance.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为" + libId + "的图书馆";
                goto ERROR1;
            }
            // 是否校验条码
            //ViewBag.verifyBarcode = lib.verifyBarcode;

            //设到ViewBag里
            string userName = "";
            if (activeUser.type == WxUserDatabase.C_Type_Worker)
            {
                userName = activeUser.userName;
                ViewBag.isPatron = 0;
            }
            else
            {
                userName = activeUser.readerBarcode;
                ViewBag.isPatron = 1;
            }

            ViewBag.userName = userName;
            ViewBag.userId = activeUser.id;

            // 关注馆藏去掉前面
            //string clearLocs = "";
            //if (string.IsNullOrEmpty(user.selLocation) == false)
            //{
            //    string[] selLoc = user.selLocation.Split(new char[] { ',' });
            //    foreach (string loc in selLoc)
            //    {
            //        string tempLoc = "";
            //        int nIndex = loc.IndexOf('/');
            //        if (nIndex > 0)
            //            tempLoc = loc.Substring(nIndex+1);

            //        if (clearLocs != "")
            //            clearLocs += ",";

            //        clearLocs += tempLoc;
            //    }
            //}
            ViewBag.Location = SubLib.ParseToView(activeUser.selLocation);

            ViewBag.verifyBarcode = activeUser.verifyBarcode;
            ViewBag.audioType = activeUser.audioType;
            return View(activeUser);


            ERROR1:
            ViewBag.Error = strError;
            return View();
        }
        
        // 公告
        public ActionResult BB(string code, string state)
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/BB");
                return View();
            }

            //绑定的工作人员账号 需要有权限
            string userName = "";
            string weixinId = ViewBag.weixinId; //(string)Session[WeiXinConst.C_Session_WeiXinId];
            string libId = ViewBag.LibId;
            Library lib = dp2WeiXinService.Instance.LibManager.GetLibrary(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                goto ERROR1;
            }

            if (weixinId == dp2WeiXinService.C_Supervisor)
            {
                userName = weixinId;
            }
            else
            {
                userName = this.GetHasRightUserName(activeUser, lib);
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


            // 如果图书馆是挂起状态，作为警告
            ViewBag.Warn = LibraryManager.GetLibHungWarn(lib);


            return View(list);


        ERROR1:
            ViewBag.Error = strError;
            return View();
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
                && activeUser.userName != "public")
            {
                // 检索是否有权限 _wx_setHomePage
                string needRight = dp2WeiXinService.C_Right_SetBb;
                int nHasRights = dp2WeiXinService.Instance.CheckRights(activeUser,
                    lib.Entity,
                    needRight,
                    out strError);
                if (nHasRights == -1)
                {
                    dp2WeiXinService.Instance.WriteErrorLog1("CheckRights()出错：" + strError);
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

            // 检查当前是否已经选择了图书馆绑定了帐号
            WxUserItem activeUser = null;

            /*
            // 如果是超级管理员，支持传一个weixin id参数
            if (String.IsNullOrEmpty(weixinId) == false)
            {
                if (this.CheckSupervisorLogin() == true)
                {
                    // 记下微信id
                    nRet = this.GetActive(code, state,
                        out activeUser,
                        out strError,
                        weixinId);
                    if (nRet == -1)
                    {
                        goto ERROR1;
                    }
                }
                else
                {
                    // 转到登录界面
                    return Redirect("~/Home/Login?returnUrl=" + HttpUtility.UrlEncode("~/Library/Home?weixinId=" + weixinId));
                }
            }
            */

                nRet = this.GetActive(code, state,
                    out activeUser,
                    out strError);
                if (nRet == -1)
                {
                    goto ERROR1;
                }
                if (nRet == 0)
                {
                    ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/Home");
                    return View();
                }




            //绑定的工作人员账号 需要有权限
            string userName = "";

            // 微信id
            weixinId = ViewBag.weixinId; //(string)Session[WeiXinConst.C_Session_WeiXinId];

            // 图书馆id
            string libId = ViewBag.LibId;
            Library lib = dp2WeiXinService.Instance.LibManager.GetLibrary(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                goto ERROR1;
            }
            //LibEntity lib = dp2WeiXinService.Instance.GetLibById(libId);
            //if (lib == null)
            //{
            //    strError = "未找到id为[" + libId + "]的图书馆定义。";
            //    goto ERROR1;
            //}

            // 2016-8-24 超级管理员可修改任何图书馆的介绍与公告
            if (weixinId == dp2WeiXinService.C_Supervisor)
            {
                userName = weixinId;
            }
            else
            {
                userName = this.GetHasRightUserName(activeUser, lib);
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

            // 如果图书馆是挂起状态，作为警告
            ViewBag.Warn = LibraryManager.GetLibHungWarn(lib);

            return View(list);

        ERROR1:

            ViewBag.Error = strError;
            return View();//Content(strError);
        }

        // 图书馆介绍
        public ActionResult dpHome(string code, string state, string weixinId)
        {

            string strError = "";
            int nRet = 0;

            /*
            // 如果是超级管理员，支持传一个weixin id参数
            if (String.IsNullOrEmpty(weixinId) == false)
            {
                if (this.CheckSupervisorLogin() == true)
                {
                    // 记下微信id
                    SessionInfo sessionInfo = this.GetSessionInfo();

                    GzhCfg gzh = null;
                    List<string> libIds = null;
                    nRet = dp2WeiXinService.Instance.GetGzhAndLibs(state, out gzh,
                        out libIds,
                        out strError);
                    if (nRet == -1)
                    {
                        goto ERROR1;
                    }

                    sessionInfo.gzh = gzh;
                    sessionInfo.libIds = libIds;
                    nRet= sessionInfo.Init1(weixinId, out strError);
                    if (nRet == -1)
                    {
                        goto ERROR1;
                    }

                }
                else
                {
                    // 转到登录界面
                    return Redirect("~/Home/Login?returnUrl=" + HttpUtility.UrlEncode("~/Library/Home?weixinId=" + weixinId));
                }
            }
            */
            // 检查当前是否已经选择了图书馆绑定了帐号
            WxUserItem activeUser = null;
            nRet = this.GetActive(code, state, 
                out activeUser,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }
            //if (nRet == 0)
            //{
            //    // todo1
            //    goto ERROR1;
            //}

            //绑定的工作人员账号 需要有权限
            string userName = "";

            // 微信id
            weixinId = ViewBag.weixinId; //(string)Session[WeiXinConst.C_Session_WeiXinId];

            // 图书馆id
            string libId = ViewBag.LibId;


            // 2016-8-24 超级管理员可修改任何图书馆的介绍与公告
            if (weixinId == dp2WeiXinService.C_Supervisor)
            {
                userName = weixinId;
            }
            

            // 设到ViewBag
            ViewBag.userName = userName;

            // 获取栏目
            List<SubjectItem> list1 = null;
            nRet = dp2WeiXinService.Instance.GetSubject(libId,
                dp2WeiXinService.C_Group_dp_home,
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/BookSubject");
                return View();
            }



            // weixin id 与图书馆id
            string weixinId = ViewBag.weixinId; //(string)Session[WeiXinConst.C_Session_WeiXinId];
            if (String.IsNullOrEmpty(libId)==true)
                libId = ViewBag.LibId;

            // 如果当前图书馆是不公开书目，则出现提示
            //LibEntity lib =  dp2WeiXinService.Instance.GetLibById(ViewBag.LibId);
            Library lib = dp2WeiXinService.Instance.LibManager.GetLibrary(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                goto ERROR1;
            }


            if (lib.Entity.noShareBiblio == 1)
            {
                List<WxUserItem> users = WxUserDatabase.Current.Get(weixinId, lib.Entity.id, -1);
                if (users.Count == 0)
                {
                    ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("好书推荐", "/Library/BookSubject", lib.Entity.libName);
                    return View();
                }
            }


            //绑定的工作人员账号 需要有权限
            // 查找当前微信用户绑定的工作人员账号
            string userName = this.GetHasRightUserName(activeUser, lib);
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

            // 如果图书馆是挂起状态，作为警告
            ViewBag.Warn = LibraryManager.GetLibHungWarn(lib);


            return View(list);

        ERROR1:
            ViewBag.Error = strError;
            return View();//Content(strError);
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/Book");
                return View();
            }

            if (String.IsNullOrEmpty(libId) == true)
            {
                strError = "libId参数不能为空";
                goto ERROR1;
            }
            //if (String.IsNullOrEmpty(subject) == true)
            //{
            //    strError = "subject参数不能为空";
            //    goto ERROR1;
            //}

            if (String.IsNullOrEmpty(subject) == true)
            {
                subject = ViewBag.remeberBookSubject;
            }
            if (subject == null)
                subject = "";

            // 如果当前图书馆是不公开书目，则出现提示
            Library lib = dp2WeiXinService.Instance.LibManager.GetLibrary(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                goto ERROR1;
            }
            //LibEntity lib = dp2WeiXinService.Instance.GetLibById(libId);
            //if (lib == null)
            //{
            //    strError = "未设置当前图书馆。";
            //    goto ERROR1;
            //}
            string weixinId = ViewBag.weixinId; //(string)Session[WeiXinConst.C_Session_WeiXinId];
            if (lib.Entity.noShareBiblio == 1)
            {
                List<WxUserItem> users = WxUserDatabase.Current.Get(weixinId, lib.Entity.id, -1);
                if (users.Count == 0)
                {
                    ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("好书推荐", "/Library/BookSubject", lib.Entity.libName);
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
                    libId,
                    "",
                    subject,
                    "browse",
                    out list,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 如果图书馆是挂起状态，作为警告
            ViewBag.Warn = LibraryManager.GetLibHungWarn(lib);


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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/BookMsg");
                return View();
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
            LibEntity lib = dp2WeiXinService.Instance.GetLibById(libId);
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

                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Library/BookEdit");
                return View();
            }

            //// 登录检查
            //nRet = this.CheckLogin(code, state, out strError);
            //if (nRet == -1)
            //{
            //    goto ERROR1;
            //}
            //if (nRet == 0)
            //{
            //    return Redirect("~/Account/Bind?from=web");
            //}

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