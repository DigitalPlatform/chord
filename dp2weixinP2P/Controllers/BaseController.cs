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
        // 获取session
        private SessionInfo GetSessionInfoInternal()
        {
            SessionInfo sessionInfo = null;
            if (Session[WeiXinConst.C_Session_sessioninfo] != null)
            {
                sessionInfo = (SessionInfo)Session[WeiXinConst.C_Session_sessioninfo];
                return sessionInfo;
            }
            return null;
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
        public int GetSessionInfo3(string code,
            string state,
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

            // 调试信息
            dp2WeiXinService.Instance.WriteDebug("GetActive-1--code=[" + code + "],state=[" + state + "]"
                + ",Request.Path=[" + this.Request.Path + "]");


            // 检查传入的myWeixinId参数是否合理
            if (string.IsNullOrEmpty(code) == false
                && string.IsNullOrEmpty(myWeixinId) == false)
            {
                dp2WeiXinService.Instance.WriteErrorLog("参数异常：如果是微信入口，不可能传入myWeixinId参数");
            }

            // 获取session对象
            sessionInfo = this.GetSessionInfoInternal();
            try
            {
                // 当已有session时
                if (sessionInfo != null)
                {
                    nRet = WhenHasSession(code,
                        state,
                        myWeixinId,
                        sessionInfo,
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


        // 当已有session的时候
        private int WhenHasSession(string code,
            string state,
            string myWeixinId,
            SessionInfo sessionInfo,
            out string strError)
        {
            int nRet = 0;
            strError = "";

            // 是否是从微信入口进来的
            bool bFromWeixin = false;
            if (string.IsNullOrEmpty(code) == false)
                bFromWeixin = true;

            // 调试信息
            dp2WeiXinService.Instance.WriteDebug("GetActive/WhenHasSession-0--已存在session。信息如下："
                + sessionInfo.Dump());

            //如果参数传进来的state与session中原有的state不一致时，要整个重新初始化session
            if (string.IsNullOrEmpty(state) == false
               && sessionInfo.gzhState != state)
            {
                dp2WeiXinService.Instance.WriteDebug("GetActive/WhenHasSession-1--session原来的state为[" + sessionInfo.gzhState + "]，此次传入的state参数为[" + state + "],两者不一样，重新初始化session信息");

                // 完全重新初始化session
                return this.WhenNoSession(code,
                    state,
                    myWeixinId,
                    out strError);
            }

            // 检查session中的公众号配置与数据库配置是否正常
            if (sessionInfo.gzh == null)
            {
                dp2WeiXinService.Instance.WriteDebug("异常：已存在session中gzh为null");
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

                dp2WeiXinService.Instance.WriteDebug("GetActive/WhenHasSession-2--参数中传入的weixinId=[" + myWeixinId + "],"
                    + "与session中原来的weixinid=[" + sessionInfo.WeixinId + "]不同，"
                    + "使用传入的weixinId,将active置为null。");
            }

            // 微信入口时,如果session里的是临时id，则需重新根据code获取weixinId
            string oldWeixinId = sessionInfo.WeixinId;
            if (bFromWeixin == true
                && string.IsNullOrEmpty(oldWeixinId) == false  //weixinid不为空
                && oldWeixinId.Length > 2
                && oldWeixinId.Substring(0, 2) == "~~")
            {
                dp2WeiXinService.Instance.WriteDebug("GetActive/WhenHasSession-3--异常：发现微信入口是临时ID，根据code重新获取id");
                sessionInfo.WeixinId = "";
                sessionInfo.ActiveUser = null;
            }

            // 如果经过前面的判断和设置，session中活动帐号依然存在，则直接使用该activeUser
            if (sessionInfo.ActiveUser != null)
            {
                if (string.IsNullOrEmpty(sessionInfo.WeixinId) == true)
                {
                    strError = "sessionInfo.Active存在，weixinId不可能为空。";
                    return -1;
                }

                dp2WeiXinService.Instance.WriteDebug("GetActive/WhenHasSession-4--如果经过前面的判断和设置，session中活动帐号依然存在，根据session初始化ViewBag");

                // 初始化 viewbag 
                nRet = this.InitViewBag(sessionInfo, out strError);
                if (nRet == -1)
                    return -1;

                dp2WeiXinService.Instance.WriteDebug("GetActive/WhenHasSession-5--InitViewBag完成，直接返回");

                // 直接返回1
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
            }

            // 根据weiXin初始化session
            nRet = sessionInfo.GetActiveUser(sessionInfo.WeixinId,
                out strError);
            if (nRet == -1)
                return -1;

            // 初始化 viewbag
            nRet = this.InitViewBag(sessionInfo, out strError);
            if (nRet == -1)
                return -1;


            return 0;
        }

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
                dp2WeiXinService.Instance.WriteDebug("GetActive/WhenHasSession-7--微信入口，根据微信code=[" + code + "]获取weixinId");

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

                dp2WeiXinService.Instance.WriteDebug("GetActive/WhenHasSession-8--根据微信code获到的weixinId=[" + weixinId + "]");

                // 写到微信浏览器的cookies里，以解决微信不退出，但web页面失效的问题
                HttpCookie aCookie = new HttpCookie("browseId");
                aCookie.Value = weixinId;
                aCookie.Expires = DateTime.MaxValue;//设为永久不失效， DateTime.Now.AddDays(1); 
                Response.Cookies.Add(aCookie);
                dp2WeiXinService.Instance.WriteDebug("GetActive/WhenHasSession-9--将微信入口的weixinid写入微信浏览器的cookies=" + weixinId);
            }
            else
            {
                // 浏览器id
                dp2WeiXinService.Instance.WriteDebug("GetActive/WhenHasSession-10--浏览器入口，准备获取browseId");


                string browseId = "";
                if (Request.Cookies["browseId"] != null)
                {
                    HttpCookie aCookie = Request.Cookies["browseId"];
                    browseId = aCookie.Value;
                    sessionInfo.AddDebugInfo("浏览器存在cookies=" + browseId);
                }

                if (string.IsNullOrEmpty(browseId) == true)
                {
                    sessionInfo.AddDebugInfo("浏览器不存在cookies或值为空");

                    string guid = Guid.NewGuid().ToString();
                    browseId = "~~" + guid;

                    sessionInfo.AddDebugInfo("创建一个临时id=" + browseId);

                    // 写到cookies
                    HttpCookie aCookie = new HttpCookie("browseId");
                    aCookie.Value = browseId;
                    aCookie.Expires = DateTime.MaxValue;//设为永久不失效， DateTime.Now.AddDays(1); 
                    Response.Cookies.Add(aCookie);

                    sessionInfo.AddDebugInfo("将临时id写入cookies=" + browseId);
                }

                // 设到weixinId变量
                weixinId = browseId;
            }

            return 0;
        }

        // 当session不存在的时候 
        private int WhenNoSession(string code,
            string state,
            string myWeixinId,
            out string strError)
        {
            int nRet = 0;
            strError = "";

            // 当发现session为空时，new一个sessioninfo
            SessionInfo sessionInfo = new SessionInfo();
            Session[WeiXinConst.C_Session_sessioninfo] = sessionInfo;

            // 调试日志
            dp2WeiXinService.Instance.WriteDebug("GetActive/WhenNoSession()-1--不已存在session，新new一个session。");

            
            // 给session设置公众号和图书馆配置
             nRet = sessionInfo.SetGzhInfo(state,out strError);
            if (nRet == -1)
                return -1;


            // 如果传入了myWeixinId参数，则以该参数作为weixinId
            if (string.IsNullOrEmpty(myWeixinId) == false)
            {
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


            // 根据weiXin初始化session
            nRet = sessionInfo.GetActiveUser(sessionInfo.WeixinId,
                out strError);
            if (nRet == -1)
                return -1;

            // 初始化 viewbag
            nRet = this.InitViewBag(sessionInfo, out strError);
            if (nRet == -1)
                return -1;


            return 0;
        }


        // 根据session信息初始化界面信息
        public int InitViewBag(SessionInfo sessionInfo, out string strError)
        {
            strError = "";

            ViewBag.AppName = sessionInfo.gzh.appNameCN;
            ViewBag.weixinId = sessionInfo.WeixinId;

            if (sessionInfo.ActiveUser != null)
            {
                //  取出上次记住的图书推荐栏目
                if (Request.Path == "/Library/Book")
                {
                    ViewBag.remeberBookSubject = sessionInfo.ActiveUser.bookSubject;
                }

                //设到ViewBag里
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
                    userNameInfo = sessionInfo.ActiveUser.readerName;// +"["+sessionInfo.Active.readerBarcode+"]";
                    ViewBag.isPatron = 1;
                }
                ViewBag.userName = userName;
                ViewBag.userNameInfo = userNameInfo;
                ViewBag.userId = sessionInfo.ActiveUser.id;

                string libName = sessionInfo.ActiveUser.libName;//sessionInfo.CurrentLib.Entity.libName;
                if (string.IsNullOrEmpty(sessionInfo.ActiveUser.bindLibraryCode) == false)
                {
                    //libName = sessionInfo.Active.bindLibraryCode;
                    // 2019/05/06 显示的名称依据libcfg.xml的配置
                    libName = dp2WeiXinService.Instance.areaMgr.GetLibCfgName(sessionInfo.ActiveUser.libId, sessionInfo.ActiveUser.bindLibraryCode);
                }

                    

                string libId = sessionInfo.ActiveUser.libId;

                ViewBag.LibName = "[" + libName + "]";
                ViewBag.PureLibName = libName;
                ViewBag.LibId = libId;
                ViewBag.LibraryCode = sessionInfo.ActiveUser.bindLibraryCode;  //这里用绑定的图书馆 20180313

                LibEntity libEntity = sessionInfo.CurrentLib.Entity;//dp2WeiXinService.Instance.GetLibById(libId);
                if (libEntity != null && libEntity.state == "到期"
                    && Request.Path.Contains("/Patron/SelectLib") == false) //选择图书馆界面除外
                {
                    strError = "服务已到期，请联系图书馆工作人员。";
                    return -1;
                }

                ViewBag.showPhoto = sessionInfo.ActiveUser.showPhoto;
                ViewBag.showCover = sessionInfo.ActiveUser.showCover;

                ViewBag.LibState = sessionInfo.CurrentLib.IsHangup.ToString();
                if (sessionInfo.CurrentLib.IsHangup == true)  //checkLibState == true && 
                {
                    string warn = LibraryManager.GetLibHungWarn(sessionInfo.CurrentLib,true);
                    if (string.IsNullOrEmpty(warn) == false)
                    {
                        strError = warn;
                        return -1;
                    }
                }
            }

            // 书目查询 与 借还 使用 JSSDK
            try
            {
                if (Request.Path.Contains("/Biblio/Index") == true
                    || Request.Path.Contains("/Library/Charge2") == true
                    || Request.Path.Contains("/Account/ScanQRCodeBind") == true)
                {
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
                }
            }
            catch (Exception ex)
            { }

            return 0;

        }
        

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