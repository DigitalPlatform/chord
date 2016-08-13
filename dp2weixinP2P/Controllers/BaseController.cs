using DigitalPlatform.Xml;
using dp2weixin.service;
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
        public string GetLibSelectHtml(string selLibId, string weixinId)
        {
            List<LibItem> list1 = LibDatabase.Current.GetLibs();

            string curLib = "";
            UserSettingItem settingItem = UserSettingDb.Current.GetByWeixinId(weixinId);
            if (settingItem != null)
            {
                curLib = settingItem.libId;
            }

            // 得到该微信用户绑定过的图书馆列表
            List<string> libs = WxUserDatabase.Current.GetLibsByWeixinId(weixinId);

            // 将所有图书馆分为2组：绑定过账户与未绑定过
            List<LibItem> bindList = new List<LibItem>();
            List<LibItem> unbindList = new List<LibItem>();
            foreach (LibItem libItem in list1)
            {
                if (libs.Contains(libItem.id) == true)
                {
                    bindList.Add(libItem);
                }
                else
                {
                    unbindList.Add(libItem);
                }
            }


            var opt = "";// "<option style='color:#aaaaaa' value=''>请选择图书馆</option>";

            // 先加绑定的
            if (bindList.Count > 0)
            {
                //opt += "<optgroup label='已绑定图书馆' class='select-group' >已绑定图书馆</optgroup>";
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
                //opt += "<optgroup label='其它图书馆' class='select-group' >其它图书馆</optgroup>";
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

            string libHtml = "<select id='selLib' style='padding-left: 0px;width: 65%;border:1px solid #eeeeee'  >" + opt + "</select>";
            return libHtml;
        }
        public int CheckIsFromWeiXin(string code, string state,out string strError)
        {
            strError = "";

            // 从微信进入的            
            if (string.IsNullOrEmpty(code) == false)
            {
                // 取出session中保存的code
                string sessionCode = "";
                if (Session[WeiXinConst.C_Session_Code] != null)
                {
                    sessionCode = (String)Session[WeiXinConst.C_Session_Code];
                }
                // 如果session中的code与传进入的code相同，则不再获取weixinid
                if (sessionCode == code)
                {
                    dp2WeiXinService.Instance.WriteLog("传进来的code["+code+"]与session中保存的code相同，不再获取weixinid了。");
                }
                else
                {
                    dp2WeiXinService.Instance.WriteLog("传进来的code[" + code + "]与session中保存的code["+sessionCode+"]不同，重新获取weixinid了。");

                    string weiXinIdTemp = "";
                    int nRet = dp2WeiXinService.Instance.GetWeiXinId(code, state, out weiXinIdTemp, out strError);
                    if (nRet == -1)
                    { return -1; }

                    if (String.IsNullOrEmpty(weiXinIdTemp) == false)
                    {
                        // 记下微信id
                        Session[WeiXinConst.C_Session_WeiXinId] = weiXinIdTemp;
                        // 记下code，因为在iphone点返回按钮，要重新传过来同样的code,再用这code取weixinid就会报40029
                        Session[WeiXinConst.C_Session_Code] = code;
                    }
                }
            }

            // 检查session中是否存在weixinid
            if (Session[WeiXinConst.C_Session_WeiXinId] == null
                || (String)Session[WeiXinConst.C_Session_WeiXinId] == "")
            {
                strError = "非正规途径进入或者Session已失效，请重新从微信中\"我爱图书馆\"公众号进入。";
                return -1;
            }


            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            // 检查微信id是否已经绑定的读者
            List<WxUserItem> userList = WxUserDatabase.Current.GetAllByWeixinId(weixinId);
            if (userList !=null && userList.Count >0)
                Session[WeiXinConst.C_Session_IsBind] = 1;
            else
                Session[WeiXinConst.C_Session_IsBind] = 0;

            // 微信用户设置的图书馆
            string libName = "";
            string libId = "";
            int showPhoto = 0; //显示头像
            int showCover = 0;//显示封面
            
            UserSettingItem settingItem = UserSettingDb.Current.GetByWeixinId(weixinId);
            if (settingItem != null)
            {
                LibItem lib = LibDatabase.Current.GetLibById(settingItem.libId);
                if (lib == null)
                {
                    strError = "未找到id为'" + settingItem.libId + "'对应的图书馆"; //这里lib为null竟然用了lib.id，一个bug 2016-8-11
                    return -1;
                }
                if (lib != null)
                {
                    libName = lib.libName;
                    libId = lib.id;
                    showPhoto = settingItem.showPhoto;
                    showCover = settingItem.showCover;

                    if (Request.Path.Contains("/Library/BookEdit") == true)
                    {
                        string xml = settingItem.xml;
                        ViewBag.remeberBookSubject = UserSettingDb.getBookSubject(xml);
                    }
                }
            }
            if (libName == "" || libId=="")
            {
                LibItem lib = LibDatabase.Current.GetOneLib();
                if (lib == null)
                {
                    strError = "当前系统未配置图书馆";
                    return -1;
                }
                libName = lib.libName;
                libId = lib.id;
            }
            ViewBag.LibName = "["+libName+"]";
            ViewBag.LibId = libId;
            ViewBag.showPhoto = showPhoto;
            ViewBag.showCover = showCover;


            ////当前读者
            //WxUserItem curPatron = WxUserDatabase.Current.GetActivePatron(weixinId);

            // 当前工作人员
            //WxUserItem curWorker = WxUserDatabase.Current.GetOneWorker(weixinId);
            
            

            return 0;
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