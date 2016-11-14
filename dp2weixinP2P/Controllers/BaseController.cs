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
        public string GetLibSelectHtml(string selLibId, string weixinId, bool bContainEmptyLine,bool bFull=false)
        {
            List<LibEntity> list1 = LibDatabase.Current.GetLibs();

            string curLib = "";
            UserSettingItem settingItem = UserSettingDb.Current.GetByWeixinId(weixinId);
            if (settingItem != null)
            {
                curLib = settingItem.libId;
            }

            if (String.IsNullOrEmpty(selLibId) == true)
                selLibId = curLib;

            // 得到该微信用户绑定过的图书馆列表
            List<string> libs = WxUserDatabase.Current.GetLibsByWeixinId(weixinId);

            // 将所有图书馆分为2组：绑定过账户与未绑定过
            List<LibEntity> bindList = new List<LibEntity>();
            List<LibEntity> unbindList = new List<LibEntity>();
            foreach (LibEntity lib in list1)
            {
                if (libs.Contains(lib.id) == true)
                {
                    bindList.Add(lib);
                }
                else
                {
                    unbindList.Add(lib);
                }
            }


            var opt = "";

            if (bContainEmptyLine == true)
                opt = "<option style='color:#aaaaaa' value=''>请选择图书馆</option>";

            // 先加绑定的
            if (bindList.Count > 0)
            {
                opt += "<optgroup label='已绑定图书馆' class='option-group'  >已绑定图书馆</optgroup>";
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
                opt += "<optgroup label='其它图书馆' class='option-group' >其它图书馆</optgroup>";
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
            if (bFull == true)
            {
                width = "width: 100%;margin-bottom:0px;padding:0px";
                clickEvent = " onchange='save()' ";
            }



            string libHtml = "<select id='selLib' "+clickEvent+" style='background-color:transparent;display:inline;padding-left: 0px;" + width + "border:1px solid #eeeeee'  >" + opt + "</select>";
            return libHtml;
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


        public int CheckIsFromWeiXin(string code, string state, out string strError,bool checkLibState=true)
        {
            strError = "";

            SessionInfo sessionInfo = this.GetSessionInfo();
            string weixinId = "";
            GzhCfg gzh = null;

            // 从微信进入的            
            if (string.IsNullOrEmpty(code) == false)
            {
                // 如果session中的code与传进入的code相同，则不再获取weixinid
                if ( String.IsNullOrEmpty(sessionInfo.oauth2_return_code)==false
                    && sessionInfo.oauth2_return_code == code)
                {
                    dp2WeiXinService.Instance.WriteLog1("传进来的code[" + code + "]与session中保存的code相同，不再获取weixinid了。");
                }
                else
                {
                    dp2WeiXinService.Instance.WriteLog1("传进来的code[" + code + "]与session中保存的code[" + sessionInfo.oauth2_return_code + "]不同，重新获取weixinid了，ip=" + Request.UserHostAddress + "。");

                    int nRet = dp2WeiXinService.Instance.GetWeiXinId(code, state, out gzh,out weixinId, out strError);
                    if (nRet == -1)
                    {
                        return -1;
                    }

                    if (String.IsNullOrEmpty(weixinId) == true || gzh == null)
                    {
                        strError = "异常，未得到微信id 或者 公众号配置信息";
                        return -1;
                    }

                    sessionInfo.weixinId =weixinId;
                    sessionInfo.gzh=gzh;
                }
            }

            // 检查session是否超时
            if (sessionInfo == null || String.IsNullOrEmpty(sessionInfo.weixinId)==true)
            {
                string libHomeUrl = dp2WeiXinService.Instance.GetOAuth2Url(sessionInfo.gzh, "Library/Home");
                strError = "页面超时，请点击<a href='" + libHomeUrl + "'>这里</a>或者从微信窗口重新进入。";//请重新从微信\"我爱图书馆\"公众号进入。"; //Sessin
                return -1;
            }

            gzh = sessionInfo.gzh;//重新赋值一下
            ViewBag.AppName = sessionInfo.gzh.appNameCN;
            weixinId = sessionInfo.weixinId;
            ViewBag.weixinId = weixinId; // 存在ViewBag里，省得使用的页面每次从session中取

            // 微信用户设置的图书馆
            string libName = "";
            string libId = "";
            int showPhoto = 0; //显示头像
            int showCover = 0;//显示封面
            Library lib = null;
            UserSettingItem settingItem = UserSettingDb.Current.GetByWeixinId(weixinId);
            if (settingItem != null)
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

            if (lib==null) // 找第一个
            {
                List<Library> libs = dp2WeiXinService.Instance.LibManager.Librarys;//  LibDatabase.Current.GetLibs();
                if (libs == null || libs.Count == 0)
                {
                    strError = "当前系统未配置图书馆";
                    return -1;
                }

                lib = libs[0]; // 第一个是数字平台
            }

            if (lib != null)
            {
                libName = lib.Entity.libName;
                libId = lib.Entity.id;
            }

            ViewBag.LibName = "[" + libName + "]";
            ViewBag.LibId = libId;
            ViewBag.showPhoto = showPhoto;
            ViewBag.showCover = showCover;
            ViewBag.LibState = lib.State;
            if (checkLibState==true && lib.State == LibraryManager.C_State_Hangup)
            {
                // 立即重新检查一下
                dp2WeiXinService.Instance.LibManager.RedoGetVersion(lib);


                if (lib.Version == "-1")
                {
                    //的桥接服务器dp2capo已失去连接，请尽快修复。
                    strError = libName + " 的桥接服务器dp2capo失去连接，公众号功能已被挂起，请尽快修复。";
                }
                else
                {
                    strError = libName + " 的桥接服务器dp2capo版本不够新，公众号功能已被挂起，请尽快升级。";
                }
                return -1;
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