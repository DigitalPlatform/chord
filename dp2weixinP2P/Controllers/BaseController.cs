using DigitalPlatform.Message;
using DigitalPlatform.Xml;
using dp2weixin.service;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.MP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace dp2weixinWeb.Controllers
{
    public class BaseController : Controller
    {
        public int GetSessionInfo(string code,
    string state,
    out SessionInfo sessionInfo,
    out string strError,
    string myWeixinId = "")
        {
            return this.GetSessionInfo(code,
                state,
                true,
                false,
                out sessionInfo,
                out strError,
                myWeixinId);
        
        }

        /// <summary>
        /// 获取当前sessionInfo
        /// </summary>
        /// <param name="code">公众号入口code值，web入口无此参数</param>
        /// <param name="state">公众号名称和图书馆参数，需要在weixin.xml里配置公众号的appID等参数和对应的模板消息
        /// 公众号菜单入口：http://www.dp2003.com/dp2weixin/weixin/index?state=ilovelibrary
        /// web入口默认是ilovelibrary公众号，如果要指定特定用户单位，state里可以写capo_XXX帐户，例如http://dp2003.com/i?state=capo_cabr
        /// </param>
        /// <param name="sessionInfo">当前sessionInfo</param>
        /// <param name="strError"></param>
        /// <param name="myWeixinId">特殊用法，用于内部调试直接输入微信id，以便通过浏览器使用公众号的功能</param>
        /// <returns>
        /// -1 出错
        /// 0 成功
        /// </returns>
        public int GetSessionInfo(string code,
            string state,
            bool isCheckLibState,
            bool redoGetActiveUser,
            out SessionInfo sessionInfo,
            out string strError,
            string myWeixinId = "")
        {
            strError = "";
            int nRet = 0;
            if (code == null)
                code = "";
            if (state == null)
                state = "";
            sessionInfo = null;

            // 调试信息
            //dp2WeiXinService.Instance.WriteDebug("Url=[" + this.Request.Url + "]");

            // 检查传入的myWeixinId参数是否合理
            if (string.IsNullOrEmpty(code) == false
                && string.IsNullOrEmpty(myWeixinId) == false)
            {
                //异常信息
                dp2WeiXinService.Instance.WriteErrorLog("参数异常：如果是微信入口，不可能传入myWeixinId参数");
            }

            try
            {
                // 检查session对象是否存在
                if (Session[WeiXinConst.C_Session_sessioninfo] != null)
                {
                    sessionInfo = (SessionInfo)Session[WeiXinConst.C_Session_sessioninfo];
                }

                // 当已有session时
                if (sessionInfo != null)
                {
                    nRet = WhenHasSession(code,
                        state,
                        myWeixinId,
                        sessionInfo,
                        isCheckLibState,
                        redoGetActiveUser,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                // session不存在时
                else
                {
                    nRet = WhenNoSession(code,
                        state,
                        myWeixinId,
                        isCheckLibState,
                        out sessionInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                // 成功返回0
                return 0;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }


        /// <summary>
        /// sessionInfo已存在，但注意里面的内容可能没有，如果没有的话还是需要进行初始化
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="myWeixinId"></param>
        /// <param name="sessionInfo"></param>
        /// <param name="strError"></param>
        /// <returns>
        /// -1 出错
        /// 0 成功
        /// </returns>
        private int WhenHasSession(string code,
            string state,
            string myWeixinId,
            SessionInfo sessionInfo,
            bool isCheckLibState,
            bool redoGetActiveUser,
            out string strError)
        {
            int nRet = 0;
            strError = "";

            // 是否是从微信入口进来的
            bool bFromWeixin = false;
            if (string.IsNullOrEmpty(code) == false)
                bFromWeixin = true;

            //// 调试信息
           // dp2WeiXinService.Instance.WriteDebug("WhenHasSession-1--已存在session。信息如下："+ sessionInfo.Dump());

            //如果参数传进来的state与session中原有的state不一致时，要整个重新初始化session
            if (string.IsNullOrEmpty(state) == false
               && sessionInfo.gzhState != state)
            {
                //dp2WeiXinService.Instance.WriteDebug("WhenHasSession-2--session原来的gzhState为[" + sessionInfo.gzhState + "]，"
                //    +"此次传入的state参数为[" + state + "],两者不一样，重新设置公众号信息。");

                // 给session设置公众号和图书馆配置
                nRet = sessionInfo.SetGzhInfo(state, out strError);
                if (nRet == -1)
                    return -1;
            }

            // 检查session中的公众号配置与数据库配置是否正常
            if (sessionInfo.gzh == null)
            {
                //dp2WeiXinService.Instance.WriteDebug("WhenHasSession-3--已存在session中gzh为null");

                // 给session设置公众号和图书馆配置
                nRet = sessionInfo.SetGzhInfo(state, out strError);
                if (nRet == -1)
                    return -1;
            }

            // 如果参数中传来了weixinid，则使用传来的weixinid，重新获取当前帐户
            if (string.IsNullOrEmpty(myWeixinId) == false
                && sessionInfo.WeixinId != myWeixinId)
            {
                sessionInfo.WeixinId = myWeixinId;
                sessionInfo.ActiveUser = null;

                //dp2WeiXinService.Instance.WriteDebug("WhenHasSession-4--参数中传入的weixinId=[" + myWeixinId + "],"
                //    + "与session中原来的weixinid=[" + sessionInfo.WeixinId + "]不同，"
                //    + "使用传入的weixinId,将active置为null。");
            }

            // 微信入口时,如果session里的是临时id，则需重新根据code获取weixinId
            string oldWeixinId = sessionInfo.WeixinId;
            if (bFromWeixin == true
                && string.IsNullOrEmpty(oldWeixinId) == false  //weixinid不为空
                && oldWeixinId.Length > 2
                && oldWeixinId.Substring(0, 2) == "~~")
            {
                //dp2WeiXinService.Instance.WriteDebug("WhenHasSession-5--发现微信入口是临时ID，根据code重新获取id");
                
                sessionInfo.WeixinId = "";
                sessionInfo.ActiveUser = null;
            }

            // 如果经过前面的判断和设置，session中活动帐号依然存在，则直接使用该activeUser
            if (sessionInfo.ActiveUser != null && redoGetActiveUser == false)  // 2020-3-8 增加redoGetActiveUser是否为null
            {
                if (string.IsNullOrEmpty(sessionInfo.WeixinId) == true)
                {
                    strError = "sessionInfo.Active存在，weixinId不可能为空。";
                    return -1;
                }

                //dp2WeiXinService.Instance.WriteDebug("WhenHasSession-6--如果经过前面的判断和设置，session中活动帐号依然存在，根据session初始化ViewBag");

                // 初始化 viewbag 
                nRet = this.InitViewBag(sessionInfo, isCheckLibState, out strError);
                if (nRet == -1)
                    return -1;

                //dp2WeiXinService.Instance.WriteDebug("WhenHasSession-7--InitViewBag完成，直接返回");

                // 直接返回0
                return 0;
            }

            // 获取weixinId
            if (string.IsNullOrEmpty(sessionInfo.WeixinId) == true)
            {
                nRet = this.GetLogicWeiXinId(code,
                    sessionInfo,
                    out string weixinid,
                    out strError);
                if (nRet == -1)
                    return -1;
                sessionInfo.WeixinId = weixinid;

                //dp2WeiXinService.Instance.WriteDebug("WhenHasSession-8--得到weixinId为["+weixinid+"]");
            }

            // 根据weiXin初始化session
            nRet = sessionInfo.GetActiveUser(sessionInfo.WeixinId,
                out strError);
            if (nRet == -1)
                return -1;

           //if (sessionInfo.ActiveUser !=null)
           //     dp2WeiXinService.Instance.WriteDebug("WhenHasSession-9--获取当前帐户为"  + (String.IsNullOrEmpty(sessionInfo.ActiveUser.readerName) == false ? sessionInfo.ActiveUser.readerName : sessionInfo.ActiveUser.userName)); // this.ActiveUser.Dump() + "\r\n";
                                                                                                                                                                                                                         //sessionInfo.ActiveUser.Dump());
            // 初始化 viewbag
            nRet = this.InitViewBag(sessionInfo, isCheckLibState,out strError);
            if (nRet == -1)
                return -1;

            //dp2WeiXinService.Instance.WriteDebug("WhenHasSession-10-InitViewBag()完成");


            return 0;
        }



        /// <summary>
        /// 当session不存在的时候 ,新new一个session并做一系列初始化
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="myWeixinId"></param>
        /// <param name="strError"></param>
        /// <returns>
        /// -1 出错
        /// 0 成功
        /// </returns>
        private int WhenNoSession(string code,
            string state,
            string myWeixinId,
            bool isCheckLibState,
            out SessionInfo sessionInfo,
            out string strError)
        {
            int nRet = 0;
            strError = "";

            // 当发现session为空时，new一个sessioninfo
            sessionInfo = new SessionInfo();
            Session[WeiXinConst.C_Session_sessioninfo] = sessionInfo;

            // 调试日志
            //dp2WeiXinService.Instance.WriteDebug("WhenNoSession()-1--不已存在session，新new一个session。");

            // 给session设置公众号和图书馆配置
             nRet = sessionInfo.SetGzhInfo(state,out strError);
            if (nRet == -1)
                return -1;

            // 调试日志
            //dp2WeiXinService.Instance.WriteDebug("WhenNoSession()-2--SetGzhInfo()完成");


            // 如果传入了myWeixinId参数，则以该参数作为weixinId
            if (string.IsNullOrEmpty(myWeixinId) == false)
            {
                myWeixinId = dp2WeiXinService.Instance.AddAppIdForWeixinId(myWeixinId,null);
                sessionInfo.WeixinId = myWeixinId;
            }
            else
            {
                nRet = this.GetLogicWeiXinId(code,
                    sessionInfo,
                    out string weixinid,
                    out strError);
                if (nRet == -1)
                    return -1;
                sessionInfo.WeixinId = weixinid;
            }

            // 调试日志
            //dp2WeiXinService.Instance.WriteDebug("WhenNoSession()-3--获得weixinId为["+sessionInfo.WeixinId+"]");


            // 根据weiXin初始化session
            nRet = sessionInfo.GetActiveUser(sessionInfo.WeixinId,
                out strError);
            if (nRet == -1)
                return -1;

            if (sessionInfo.ActiveUser != null)
            {
                //dp2WeiXinService.Instance.WriteDebug("WhenNoSession-4--获取当前帐户为" + (String.IsNullOrEmpty(sessionInfo.ActiveUser.readerName) == false ? sessionInfo.ActiveUser.readerName : sessionInfo.ActiveUser.userName)); // sessionInfo.ActiveUser.Dump() + "\r\n";
            }

            // 初始化 viewbag
            nRet = this.InitViewBag(sessionInfo, isCheckLibState,out strError);
            if (nRet == -1)
                return -1;

            //dp2WeiXinService.Instance.WriteDebug("WhenNoSession-5-InitViewBag()完成");

            return 0;
        }

        /// <summary>
        /// 得到微信id，有两种入口
        /// 一种是微信入口，这时是根据微信传过来的code得到weixinId
        /// 一种是浏览器入口，这时是从cookies里找browseId，如果没有设置一个guid到cookies
        /// </summary>
        /// <param name="code">微信传来的code</param>
        /// <param name="sessionInfo">sessionInfo对象</param>
        /// <param name="weixinId">返回weixinId</param>
        /// <param name="strError"></param>
        /// <returns>
        /// -1 出错
        /// 0 成功
        /// </returns>
        private int GetLogicWeiXinId(string code,
            SessionInfo sessionInfo,
            out string weixinId,
            out string strError)
        {
            int nRet = 0;
            strError = "";
            weixinId = "";

            //微信入口
            if (string.IsNullOrEmpty(code) == false)
            {
                // 获取微信id
                dp2WeiXinService.Instance.WriteDebug2("微信入口,根据code=[" + code + "]获取weixinId");

                // 根据微信接口得到weixinid

                nRet = dp2WeiXinService.Instance.GetWeiXinId(code,
                   sessionInfo.gzh,
                  out weixinId,
                  out strError);
                if (nRet == -1)
                {
                    return -1;
                }

                if (String.IsNullOrEmpty(weixinId) == true)
                {
                    strError = "异常，未得到微信id";
                    return -1;
                }
                // 把传过来的code设在session信息里
                sessionInfo.oauth2_return_code = code;

                dp2WeiXinService.Instance.WriteDebug2("根据微信code获到的weixinId=[" + weixinId + "]");

                // 下面这一步很重要，在微信进入我爱图书web后，内部其实就没有weixinId，所以把weixinId直接写入cookies
                // 写到微信浏览器的cookies里，以解决微信不退出，但web页面失效的问题
                HttpCookie aCookie = new HttpCookie("browseId");
                aCookie.Value = weixinId;
                aCookie.Expires = DateTime.MaxValue;//设为永久不失效， DateTime.Now.AddDays(1); 
                Response.Cookies.Add(aCookie);
                dp2WeiXinService.Instance.WriteDebug2("将微信入口的weixinid写入微信浏览器的cookies=" + weixinId);
            }
            else
            {
                // 浏览器入口
                // 注意在微信进入的web页面后，则看作是浏览器入口了，只有在微信界面点菜单时，才是微信入口。
                dp2WeiXinService.Instance.WriteDebug2("浏览器入口,准备获取browseId");  


                string browseId = "";
                if (Request.Cookies["browseId"] != null)
                {
                    HttpCookie aCookie = Request.Cookies["browseId"];
                    browseId = aCookie.Value;

                    dp2WeiXinService.Instance.WriteDebug2("浏览器存在cookies=" + browseId); //sessionInfo.AddDebugInfo("浏览器存在cookies=" + browseId);
                }

                if (string.IsNullOrEmpty(browseId) == true)
                {
                    string guid = Guid.NewGuid().ToString();
                    browseId = "~~" + guid;

                    dp2WeiXinService.Instance.WriteDebug2("浏览器不存在cookies或值为空,创建一个临时id=" + browseId);

                    // 写到cookies
                    HttpCookie aCookie = new HttpCookie("browseId");
                    aCookie.Value = browseId;
                    aCookie.Expires = DateTime.MaxValue;//设为永久不失效， DateTime.Now.AddDays(1); 
                    Response.Cookies.Add(aCookie);

                    dp2WeiXinService.Instance.WriteDebug2("将临时id写入cookies=" + browseId);
                }

                // 设到weixinId变量
                weixinId = browseId;
            }

            return 0;
        }

        /// <summary>
        /// 根据session信息初始化界面信息
        /// </summary>
        /// <param name="sessionInfo"></param>
        /// <param name="strError"></param>
        /// <returns>
        /// -1 出错
        /// 0 成功
        /// </returns>
        private int InitViewBag(SessionInfo sessionInfo,
            bool isCheckLibState,
            out string strError)
        {
            strError = "";

            // 书目查询 与 借还 使用 JSSDK
            try
            {
                //if (Request.Path.Contains("/Biblio/Index") == true
                //    || Request.Path.Contains("/Library/Charge2") == true
                //    || Request.Path.Contains("/Account/ScanQRCodeBind") == true)
                //{
                    GzhCfg gzh = sessionInfo.gzh;
                    bool bJsReg = JsApiTicketContainer.CheckRegistered(gzh.appId);
                    // 注意这里有时异常
                    JsSdkUiPackage package = JSSDKHelper.GetJsSdkUiPackage(gzh.appId,
                        gzh.secret,
                        Request.Url.AbsoluteUri);//http://localhost:15794/Library/Charge  //http://www.dp2003.com/dp2weixin/Library/Charge
                    ViewData["AppId"] = gzh.appId;
                    ViewData["Timestamp"] = package.Timestamp;
                    ViewData["NonceStr"] = package.NonceStr;
                    ViewData["Signature"] = package.Signature;
                //}
            }
            catch (Exception ex)
            { }


            ViewBag.AppName = sessionInfo.gzh.appNameCN;
            ViewBag.weixinId = sessionInfo.WeixinId;

            // 没有当前帐户时，直接返回
            if (sessionInfo.ActiveUser == null)
                return 0;


            //=====
            //  取出上次记住的图书推荐栏目
            if (Request.Path == "/Library/Book")
            {
                ViewBag.remeberBookSubject = sessionInfo.ActiveUser.bookSubject;
            }

            //设到ViewBag里，当前帐户信息
            string userName = "";
            string userNameInfo = "";
            if (sessionInfo.ActiveUser.type == WxUserDatabase.C_Type_Worker)
            {
                userName = sessionInfo.ActiveUser.userName;
                userNameInfo = userName;
                ViewBag.isPatron = 0;
            }
            else
            {
                userName = sessionInfo.ActiveUser.readerBarcode;
                userNameInfo = sessionInfo.ActiveUser.displayReaderName;//2021/8/20 改为脱敏显示 readerName;// +"["+sessionInfo.Active.readerBarcode+"]";
                ViewBag.isPatron = 1;
            }
            ViewBag.userName = userName;
            ViewBag.userNameInfo = userNameInfo;
            ViewBag.bindUserId = sessionInfo.ActiveUser.id;

            

            // 2020-2-29 在配置文件中增加读者库配置
            string patronDbName = "";
            string libName = sessionInfo.ActiveUser.libName;//sessionInfo.CurrentLib.Entity.libName;
            // 2019/05/06 显示的名称依据libcfg.xml的配置
            LibModel libCfg = dp2WeiXinService.Instance._areaMgr.GetLibCfg(sessionInfo.ActiveUser.libId,
                sessionInfo.ActiveUser.bindLibraryCode);
            if (libCfg != null)
            {
                libName = libCfg.name;
                patronDbName = libCfg.patronDbName;
            }
            ViewBag.PatronDbName = patronDbName;

            ViewBag.LibName = "[" + libName + "]";
            ViewBag.PureLibName = libName;
            ViewBag.LibId = sessionInfo.ActiveUser.libId;
            ViewBag.LibraryCode = sessionInfo.ActiveUser.bindLibraryCode;  //这里用绑定的图书馆 20180313

            // 到期的图书馆应该不会显示出来，所以这一段后面可以删除 2020-2-29
            {
                if (sessionInfo.CurrentLib == null)
                {
                    strError = "图书馆公众号服务停止，请联系图书馆工作人员。";
                    return 0;
                }
                else
                {
                    LibEntity libEntity = sessionInfo.CurrentLib.Entity;//dp2WeiXinService.Instance.GetLibById(libId);
                    if (libEntity != null && libEntity.state == "到期"
                        && Request.Path.Contains("/Patron/SelectOwnerLib") == false) //选择图书馆界面除外
                    {
                        strError = "服务已到期，请联系图书馆工作人员。";
                        return -1;
                    }
                }
            }

            ViewBag.LibState = sessionInfo.CurrentLib.IsHangup.ToString();
            if (isCheckLibState == true && sessionInfo.CurrentLib.IsHangup == true)  
            {
                // 获取服务器不通文字描述
                string warn = LibraryManager.GetLibHungWarn(sessionInfo.CurrentLib, true);
                if (string.IsNullOrEmpty(warn) == false)
                {
                    strError = warn;
                    return -1;
                }
            }

            //2021/7/30 设置debug态 因为了用于currentlib，所以放在最下面。
            ViewBag.isDebug = "0"; //默认不是调试态
            // 找到当前活动帐户图书馆，绑定的工作人员，如果其中某个工作人员有wx_debug权限，则可以以调试态看界面
            List<WxUserItem> workers = WxUserDatabase.Current.GetWorkers(sessionInfo.ActiveUser.weixinId,
                sessionInfo.ActiveUser.libId, null);
            if (workers.Count > 0 && sessionInfo.CurrentLib != null)
            {
                foreach (WxUserItem worker in workers)
                {
                    if (worker.userName != "public")
                    {
                        int nHasRights = dp2WeiXinService.Instance.CheckRights(worker, sessionInfo.CurrentLib.Entity,
                            dp2WeiXinService.C_Right_Debug, out strError);
                        if (nHasRights == -1)
                        {
                            // 2021/8/4 出错的时候，不返回错误，防止没法再选择图书馆
                            //strError = "检查工作人员(" + worker.userName + ")是否有[" + dp2WeiXinService.C_Right_Debug + "]权限时出错：" + strError;
                            //return -1;
                        }
                        if (nHasRights == 1)
                        {
                            ViewBag.isDebug = "1";
                            break;
                        }
                    }
                }
            }

            // 获取当前登录身份
            LoginInfo loginInfo = dp2WeiXinService.Instance.Getdp2AccoutActive(sessionInfo.ActiveUser);
            ViewBag.loginUserName = loginInfo.UserName;
            ViewBag.loginUserType = loginInfo.UserType;

            return 0;
        }
        
        /// <summary>
        /// 是否是supervisor登录
        /// </summary>
        /// <returns></returns>
        public bool CheckSupervisorLogin()
        {
            if (Session[dp2WeiXinService.C_Session_Supervisor] != null
                && (bool)Session[dp2WeiXinService.C_Session_Supervisor] == true)
            {
                return true;
            }
            return false;
        }

    }
}