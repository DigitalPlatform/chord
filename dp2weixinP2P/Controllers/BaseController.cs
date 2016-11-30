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
        public int GetLibSelectHtml(string selLibId, 
            string weixinId, 
            bool bContainEmptyLine,
            string onchangeEvent,
            out string selLibHtml,
            out string error)
        {
            error = "";
            selLibHtml = "";

            SessionInfo sessionInfo = this.GetSessionInfo();
            if (sessionInfo == null)
            {
                error= "session已失效。";
                return -1;
            }
            if (sessionInfo.libIds == null || sessionInfo.libIds.Count == 0)
            {
                error ="未得到可访问的图书馆信息";
                return -1;
            }

            // 2016-11-22 只列出可访问的图书馆
            //List<LibEntity> list1 = LibDatabase.Current.GetLibs();
            List<Library> libList = dp2WeiXinService.Instance.LibManager.GetLibraryByIds(sessionInfo.libIds);


            // 当前图书馆
            string curLib = "";
            UserSettingItem settingItem = UserSettingDb.Current.GetByWeixinId(weixinId);
            if (settingItem != null)
            {
                curLib = settingItem.libId;
            }
            // 优先认传进来的图书馆
            if (String.IsNullOrEmpty(selLibId) == true)
                selLibId = curLib;


            // 得到该微信用户绑定过的图书馆列表
            List<string> libs = WxUserDatabase.Current.GetLibsByWeixinId(weixinId);

            // 将所有图书馆分为2组：绑定过账户与未绑定过
            List<LibEntity> bindList = new List<LibEntity>();
            List<LibEntity> unbindList = new List<LibEntity>();
            foreach (Library lib in libList)
            {
                if (libs.Contains(lib.Entity.id) == true)
                {
                    bindList.Add(lib.Entity);
                }
                else
                {
                    unbindList.Add(lib.Entity);
                }
            }


            var opt = "";

            if (bContainEmptyLine == true)
                opt = "<option style='color:#aaaaaa' value=''>请选择图书馆</option>";

            // 先加绑定的
            if (bindList.Count > 0)
            {
                //2016-11-22 如果存在未绑定的图书馆，才这样区分
                if (unbindList.Count > 0)
                {
                    opt += "<optgroup label='已绑定图书馆' class='option-group'  >已绑定图书馆</optgroup>";
                }

                for (var i = 0; i < bindList.Count; i++)
                {
                    var item = bindList[i];
                    string selectedString = "";
                    if (selLibId != "" && selLibId == item.id)
                    {
                        selectedString = " selected='selected' ";
                    }

                    string name = item.libName;
                    //if (curLib != "" && curLib == item.id)
                    //    name = "*" + name;
                    string className = "option-bind";
                    if (curLib != "" && curLib == item.id)
                        className = "option-current";

                    opt += "<option class='" + className + "' value='" + item.id + "' " + selectedString + ">" + name + "</option>";
                }
            }

            // 再加未绑定的
            if (unbindList.Count > 0)
            {
                //2016-11-22 如果存在已绑定的图书馆，才这样区分
                if (bindList.Count > 0)
                {
                    opt += "<optgroup label='其它图书馆' class='option-group' >其它图书馆</optgroup>";
                }

                for (var i = 0; i < unbindList.Count; i++)
                {
                    var item = unbindList[i];
                    string selectedString = "";
                    if (selLibId != "" && selLibId == item.id)
                    {
                        selectedString = " selected='selected' ";
                    }

                    string name = item.libName;
                    //if (curLib != "" && curLib == item.id)
                    //    name = "*" + name;

                    string className = "option-unbind";
                    if (curLib != "" && curLib == item.id)
                        className = "option-current";
                    opt += "<option class='" + className + "' value='" + item.id + "' " + selectedString + ">" + name + "</option>";
                }
            }

            string width = "width: 65%;";
            string clickEvent = "";
            if (string.IsNullOrEmpty(onchangeEvent) == false)
            {
                //width = " width:75%;font-size:14px;"; //"width: 90%;margin-bottom:0px;padding:0px";
                clickEvent = " onchange='" + onchangeEvent + "' ";
            }

            selLibHtml = "<select id='selLib' "+clickEvent+" style='background-color:transparent;display:inline;padding-left: 0px;" + width + "border:1px solid #eeeeee'  >" + opt + "</select>";
            return 0;
        }

        public SessionInfo GetSessionInfo()
        {
            if (Session[WeiXinConst.C_Session_sessioninfo] != null)
            {
                SessionInfo myinfo = (SessionInfo)Session[WeiXinConst.C_Session_sessioninfo];
                return myinfo;
            }

            return null;
        }


        public int CheckIsFromWeiXin(string code, string state, out string strError, bool checkLibState = true)
        {
            strError = "";

            SessionInfo sessionInfo = this.GetSessionInfo();
            if (sessionInfo == null)
            {
                //string libHomeUrl = dp2WeiXinService.Instance.GetOAuth2Url(sessionInfo.gzh, "Library/Home");
                strError = "页面超时，请从微信窗口重新进入。";//请重新从微信\"我爱图书馆\"公众号进入。"; //Sessin
                return -1;
            }
            string weixinId = "";
            GzhCfg gzh = sessionInfo.gzh;

            // 从微信进入的            
            if (string.IsNullOrEmpty(code) == false)
            {
                // 如果session中的code与传进入的code相同，则不再获取weixinid
                if (String.IsNullOrEmpty(sessionInfo.oauth2_return_code) == false
                    && sessionInfo.oauth2_return_code == code)
                {
                    dp2WeiXinService.Instance.WriteLog1("传进来的code[" + code + "]与session中保存的code相同，不再获取weixinid了。");
                }
                else
                {
                    dp2WeiXinService.Instance.WriteLog1("传进来的code[" + code + "]与session中保存的code[" + sessionInfo.oauth2_return_code + "]不同，重新获取weixinid了，ip=" + Request.UserHostAddress + "。");
                    List<string> libList = null;
                    int nRet = dp2WeiXinService.Instance.GetWeiXinId(code,
                        state,
                        out gzh,
                        out weixinId,
                        out libList,
                        out strError);
                    if (nRet == -1)
                    {
                        return -1;
                    }
                    if (String.IsNullOrEmpty(weixinId) == true || gzh == null)
                    {
                        strError = "异常，未得到微信id 或者 公众号配置信息";
                        return -1;
                    }

                    sessionInfo.weixinId = weixinId;
                    sessionInfo.gzh = gzh;
                    sessionInfo.libIds = libList;
                }
            }

            //// 检查session是否超时
            //if (String.IsNullOrEmpty(sessionInfo.weixinId)==true)
            //{
            //    string libHomeUrl = dp2WeiXinService.Instance.GetOAuth2Url(sessionInfo.gzh, "Library/Home");
            //    strError = "页面超时，请点击<a href='" + libHomeUrl + "'>这里</a>或者从微信窗口重新进入。";//请重新从微信\"我爱图书馆\"公众号进入。"; //Sessin
            //    return -1;
            //}

            if (sessionInfo.libIds == null || sessionInfo.libIds.Count == 0)
            {
                strError = "未找到可访问的图书馆";
                return -1;
            }

            gzh = sessionInfo.gzh;//重新赋值一下
            if (gzh == null)
            {
                strError = "未找到公众号配置信息";
                return -1;
            }
            ViewBag.AppName = sessionInfo.gzh.appNameCN;
            weixinId = sessionInfo.weixinId;
            ViewBag.weixinId = weixinId; // 存在ViewBag里，省得使用的页面每次从session中取

            // 微信用户设置信息
            int showPhoto = 0; //显示头像
            int showCover = 0;//显示封面
            Library lib = null;
            UserSettingItem settingItem = UserSettingDb.Current.GetByWeixinId(weixinId);
            if (settingItem != null)
            {
                if (sessionInfo.libIds.IndexOf(settingItem.libId) != -1) // 2016-11-22 先要在自己的可访问图书馆
                {
                    lib = dp2WeiXinService.Instance.LibManager.GetLibrary(settingItem.libId);//.GetLibById(settingItem.libId);
                    if (lib == null)
                    {
                        strError = "未找到id为'" + settingItem.libId + "'对应的图书馆"; //这里lib为null竟然用了lib.id，一个bug 2016-8-11
                        return -1;
                    }

                    showPhoto = settingItem.showPhoto;
                    showCover = settingItem.showCover;
                    if (Request.Path == "/Library/Book")//) == true)///Library/BookEdit
                    {
                        string xml = settingItem.xml;
                        ViewBag.remeberBookSubject = UserSettingDb.getBookSubject(xml);
                    }
                }
                else
                {
                    dp2WeiXinService.Instance.WriteErrorLog1("发现weixinid=" + weixinId + "设置的图书馆" + settingItem.libId + "不在访问列表中");
                }
            }

            if (lib == null) // 找第一个
            {
                //List<Library> libs = dp2WeiXinService.Instance.LibManager.Librarys;//  LibDatabase.Current.GetLibs();
                //if (libs == null || libs.Count == 0)
                //{
                //    strError = "当前系统未配置图书馆";
                //    return -1;
                //}
                //lib = libs[0]; // 第一个是数字平台

                // 找可访问的第一个图书馆 2016-11-22
                string firstLibId = sessionInfo.libIds[0];
                lib = dp2WeiXinService.Instance.LibManager.GetLibrary(firstLibId);
            }
            if (lib == null)
            {
                strError = "未匹配上图书馆";
                return -1;
            }


            string libName = lib.Entity.libName;
            string libId = lib.Entity.id;

            ViewBag.LibName = "[" + libName + "]";
            ViewBag.LibId = libId;
            ViewBag.showPhoto = showPhoto;
            ViewBag.showCover = showCover;
            ViewBag.LibState = lib.State;
            if (checkLibState == true && lib.State == LibraryManager.C_State_Hangup)
            {
                // 2016-11-22 注释，留页面做，不要写的这样，否则页面空白等待时间过多，造成白页，用户体验不好
                // 立即重新检查一下 
                //dp2WeiXinService.Instance.LibManager.RedoGetVersion(lib);

                //if (lib.Version == "-1")
                //{
                //    //的桥接服务器dp2capo已失去连接，请尽快修复。
                //    strError = libName + " 的桥接服务器dp2capo失去连接，公众号功能已被挂起，请尽快修复。";
                //}
                //else
                //{
                //    strError = libName + " 的桥接服务器dp2capo版本不够新，公众号功能已被挂起，请尽快升级。";
                //}
                //return -1;

                string test = "123";
            }

            bool bJsReg = JsApiTicketContainer.CheckRegistered(gzh.appId);

            ////获取时间戳
            //var timestamp = JSSDKHelper.GetTimestamp();
            ////获取随机码
            //string nonceStr = JSSDKHelper.GetNoncestr();
            //string ticket = JsApiTicketContainer.GetJsApiTicket(dp2WeiXinService.Instance.weiXinAppId);
            ////.TryGetJsApiTicket(appId,appSecret);
            ////获取签名
            //string signature = JSSDKHelper.GetSignature(ticket, nonceStr, timestamp, Request.Url.AbsoluteUri);


            // 注意这里有时异常
            JsSdkUiPackage package = JSSDKHelper.GetJsSdkUiPackage(gzh.appId,
                gzh.secret,
                Request.Url.AbsoluteUri);//http://localhost:15794/Library/Charge  //http://www.dp2003.com/dp2weixin/Library/Charge
            ViewData["AppId"] = gzh.appId;
            ViewData["Timestamp"] = package.Timestamp;
            ViewData["NonceStr"] = package.NonceStr;
            ViewData["Signature"] = package.Signature;

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

        //protected override void OnException(ExceptionContext filterContext)
        //{
        //    // 标记异常已处理
        //    filterContext.ExceptionHandled = true;
        //    // 跳转到错误页
        //    filterContext.Result = this.RedirectToAction("Error", "Shared");// new RedirectResult("/Shared/Error");//Url.Action("Error", "Shared"));
        //}
    }
}