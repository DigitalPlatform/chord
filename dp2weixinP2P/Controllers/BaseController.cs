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
        public SessionInfo GetSessionInfo1()
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
        /// 获取当前帐号
        /// </summary>
        /// <param name="state"></param>
        /// <param name="code"></param>
        /// <param name="browseId"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int GetActive(string code,
            string state,
            out WxUserItem activeUser,
            out string strError,
            string myWeixinId = "")
        {
            strError = "";
            int nRet = 0;
            activeUser = null;

            // 日志
            dp2WeiXinService.Instance.WriteLog1("code=["+code+"] state=["+state+"]");


            // 获取session对象，如果不存在新new一个
            SessionInfo sessionInfo = this.GetSessionInfo1();
            try
            {
                if (sessionInfo != null)
                {
                    //sessionInfo.ClearDebugInfo();

                    // 日志
                    dp2WeiXinService.Instance.WriteLog1("已存在session");
                }
                else
                {
                    // 当发现session为空时，new一个sessioninfo
                    sessionInfo = new SessionInfo();

                    sessionInfo.AddDebugInfo("session不存在，新建一个session对象");
                    Session[WeiXinConst.C_Session_sessioninfo] = sessionInfo;

                    // 得到weixinid
                    GzhCfg gzh1 = sessionInfo.gzh;
                    List<string> libList1 = new List<string>();
                    if (string.IsNullOrEmpty(state) == true)
                    {
                        if (string.IsNullOrEmpty(code) == false)
                        {
                            strError = "从微信入口进来，code参数不能为空";
                            return -1;
                        }
                        state = "ilovelibrary";
                    }
                    sessionInfo.AddDebugInfo("给sesion存入state[" + state + "]");
                    nRet = dp2WeiXinService.Instance.GetGzhAndLibs(state,
                       out gzh1,
                       out libList1,
                       out strError);
                    if (nRet == -1)
                        return -1;
                    if (gzh1 == null)
                    {
                        strError = "异常，未得到公众号配置信息";
                        goto ERROR1;
                    }
                    sessionInfo.SetInfo(state, gzh1, libList1);
                }

                sessionInfo.AddDebugInfo("~~~~~~" + this.Request.Path + "~~~~~~");

                // 如果客户端特别传来了weixinid，使用传来的weixinid
                if (string.IsNullOrEmpty(myWeixinId) == false
                    && sessionInfo.WeixinId != myWeixinId)
                {
                    sessionInfo.AddDebugInfo("原来的weixinid=" + sessionInfo.WeixinId);

                    sessionInfo.WeixinId = myWeixinId;
                    sessionInfo.Active = null;
                    sessionInfo.AddDebugInfo("使用传进来的weixinId=" + myWeixinId);

                }


                if (code == null)
                    code = "";
                if (state == null)
                    state = "";
                if (sessionInfo.WeixinId == null)
                    sessionInfo.WeixinId = "";
                sessionInfo.AddDebugInfo("走进GetActive(),code=[" + code + "] state=[" + state + "],session.weixinId=[" + sessionInfo.WeixinId + "]");

                activeUser = sessionInfo.Active;
                if (activeUser != null)
                {
                    sessionInfo.AddDebugInfo("session中有活动帐号");

                    if (activeUser.weixinId.Substring(0, 2) == "~~"
                        && string.IsNullOrEmpty(code) == false)
                    {
                        strError = "异常：微信入口怎么是临时ID";
                        goto ERROR1;
                    }

                    if (string.IsNullOrEmpty(state) == false  //传进来的state不为空
                       && sessionInfo.gzhState != state)
                    {
                        sessionInfo.AddDebugInfo("发现之前state为[" + sessionInfo.gzhState + "]，目前传入的state参数为[" + state + "],两者不一样，重新初始化session信息");
                        sessionInfo.AddDebugInfo("传入的code为[" + code + "]，有值，重新获取weixinId");
                        sessionInfo.WeixinId = "";
                        sessionInfo.Active = null;
                    }
                    else
                    {
                        sessionInfo.AddDebugInfo("使用当前已有session信息");
                        // 初始化 viewbag 
                        nRet = this.InitViewBag(sessionInfo, out strError);
                        if (nRet == -1)
                        {
                            goto ERROR1;
                        }
                        sessionInfo.AddDebugInfo("GetActive()返回1,当前有活动帐号");
                        return 1;
                    }
                }

                // 得到weixinid
                GzhCfg gzh = sessionInfo.gzh;
                List<string> libList = new List<string>();
                if (string.IsNullOrEmpty(state) == true)
                {
                    if (string.IsNullOrEmpty(code) == false)
                    {
                        strError = "从微信入口进来，code参数不能为空";
                        return -1;
                    }
                    state = "ilovelibrary:";
                }
                sessionInfo.AddDebugInfo("给sesion存入state[" + state + "]");
                nRet = dp2WeiXinService.Instance.GetGzhAndLibs(state,
                   out gzh,
                   out libList,
                   out strError);
                if (nRet == -1)
                    return -1;
                if (gzh == null)
                {
                    strError = "异常，未得到公众号配置信息";
                    goto ERROR1;
                }
                sessionInfo.SetInfo(state, gzh, libList);


                sessionInfo.AddDebugInfo("配置信息存到session");


                string weixinId = sessionInfo.WeixinId;
                if (string.IsNullOrEmpty(weixinId) == false
                    && weixinId.Length > 2
                    && weixinId.Substring(0, 2) == "~~"
                    && string.IsNullOrEmpty(code) == false)
                {
                    sessionInfo.AddDebugInfo("异常：发现微信入口是临时ID，根据code重新获取id");
                    weixinId = "";
                }

                // 正式id
                if (string.IsNullOrEmpty(weixinId) == false)
                {
                    sessionInfo.AddDebugInfo("session中已有id=[" + weixinId + "],直接使用该id初始化对象");
                }
                else
                {
                    //微信入口
                    if (string.IsNullOrEmpty(code) == false)
                    {
                        // 正式微信id的情况
                        sessionInfo.AddDebugInfo("微信入口，根据微信code=[" + code + "]获取weixinId");

                        // 根据微信接口得到weixinid
                        nRet = dp2WeiXinService.Instance.GetWeiXinId1(code,
                           gzh,
                          out weixinId,
                          out strError);
                        if (nRet == -1)
                        {
                            goto ERROR1;
                        }
                        if (String.IsNullOrEmpty(weixinId) == true)
                        {
                            strError = "异常，未得到微信id";
                            return -1;
                        }
                        sessionInfo.oauth2_return_code = code;

                        sessionInfo.AddDebugInfo("根据微信code获到的weixinId=[" + weixinId + "]");

                        // 写到微信浏览器的cookies里，以解决微信不退出，但web页面失效的问题
                        HttpCookie aCookie = new HttpCookie("browseId");
                        aCookie.Value = weixinId;
                        aCookie.Expires = DateTime.MaxValue;//设为永久不失效， DateTime.Now.AddDays(1); 
                        Response.Cookies.Add(aCookie);
                        sessionInfo.AddDebugInfo("将微信入口的weixinid写入微信浏览器的cookies=" + weixinId);
                    }
                    else
                    {
                        // 浏览器id
                        sessionInfo.AddDebugInfo("session中weixinid为空，且没有微信传来的code");

                        sessionInfo.AddDebugInfo("检查浏览器是否存在cookies");
                        string browseId = "";
                        if (Request.Cookies["browseId"] != null)
                        {
                            HttpCookie aCookie = Request.Cookies["browseId"];
                            browseId = aCookie.Value;

                            sessionInfo.AddDebugInfo("存在cookies=" + browseId);
                        }

                        if (string.IsNullOrEmpty(browseId) == true)
                        {
                            sessionInfo.AddDebugInfo("不存在cookies或值为空");

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


                        weixinId = browseId;

                    }
                }


                // 初始化session
                nRet = sessionInfo.Init1(weixinId, out strError);// this.InitSession(state, weixinId, out sessionInfo, out strError);
                if (nRet == -1)
                    return -1;

                // 有当前帐号
                activeUser = sessionInfo.Active;
                if (activeUser != null)
                {
                    // 初始化 viewbag
                    nRet = this.InitViewBag(sessionInfo, out strError);
                    if (nRet == -1)
                    {
                        goto ERROR1;
                    }

                    sessionInfo.AddDebugInfo("GetActive()返回1,存在活动帐户");
                    return 1;
                }




                ViewBag.AppName = sessionInfo.gzh.appNameCN;
                ViewBag.weixinId = sessionInfo.WeixinId;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

            strError = "尚未选择图书馆";
            sessionInfo.AddDebugInfo("GetActive()返回0,error=[" + strError + "]");
            return 0;

            ERROR1:

            sessionInfo.AddDebugInfo("GetActive()返回-1,error=[" + strError + "]");

            return -1;
        }
        
        public int InitViewBag(SessionInfo sessionInfo, out string strError)
        {
            strError = "";

            ViewBag.AppName = sessionInfo.gzh.appNameCN;
            ViewBag.weixinId = sessionInfo.WeixinId;

            if (sessionInfo.Active != null)
            {
                //  取出上次记住的图书推荐栏目
                if (Request.Path == "/Library/Book")
                {
                    ViewBag.remeberBookSubject = sessionInfo.Active.bookSubject;
                }

                //设到ViewBag里
                string userName = "";
                string userNameInfo = "";
                if (sessionInfo.Active.type == WxUserDatabase.C_Type_Worker)
                {
                    userName = sessionInfo.Active.userName;
                    userNameInfo = userName;
                    ViewBag.isPatron = 0;
                }
                else
                {
                    userName = sessionInfo.Active.readerBarcode;
                    userNameInfo = sessionInfo.Active.readerName;// +"["+sessionInfo.Active.readerBarcode+"]";
                    ViewBag.isPatron = 1;
                }
                ViewBag.userName = userName;
                ViewBag.userNameInfo = userNameInfo;
                ViewBag.userId = sessionInfo.Active.id;

                string libName = sessionInfo.Active.libName;//sessionInfo.CurrentLib.Entity.libName;
                if (string.IsNullOrEmpty(sessionInfo.Active.bindLibraryCode) == false)
                {
                    //libName = sessionInfo.Active.bindLibraryCode;
                    // 2019/05/06 显示的名称依据libcfg.xml的配置
                    libName = dp2WeiXinService.Instance.areaMgr.GetLibCfgName(sessionInfo.Active.libId, sessionInfo.Active.bindLibraryCode);
                }

                    

                string libId = sessionInfo.Active.libId;

                ViewBag.LibName = "[" + libName + "]";
                ViewBag.PureLibName = libName;
                ViewBag.LibId = libId;
                ViewBag.LibraryCode = sessionInfo.Active.bindLibraryCode;  //这里用绑定的图书馆 20180313

                LibEntity libEntity = sessionInfo.CurrentLib.Entity;//dp2WeiXinService.Instance.GetLibById(libId);
                if (libEntity != null && libEntity.state == "到期"
                    && Request.Path.Contains("/Patron/SelectLib") == false) //选择图书馆界面除外
                {
                    strError = "服务已到期，请联系图书馆工作人员。";
                    return -1;
                }

                ViewBag.showPhoto = sessionInfo.Active.showPhoto;
                ViewBag.showCover = sessionInfo.Active.showCover;

                ViewBag.LibState = sessionInfo.CurrentLib.State;
                if (sessionInfo.CurrentLib.State == LibraryManager.C_State_Hangup)  //checkLibState == true && 
                {
                    string warn = LibraryManager.GetLibHungWarn(sessionInfo.CurrentLib);
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