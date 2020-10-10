using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dp2weixinWeb.Controllers
{
    public class BiblioController : BaseController
    {
        /// <summary>
        /// 查看PDF
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public ActionResult ViewPDF(string libId, string uri)
        {
            ViewBag.libId = libId;
            ViewBag.objectUri = uri;

            string strError = "";
            string filename = "";
            int totalPage = dp2WeiXinService.Instance.GetPDFCount(libId, uri, out filename, out strError);
            ViewBag.pageCount = totalPage;

            string strImgUri = uri + "/page:1,format:jpeg,dpi:75";
            ViewBag.firstUrl = "../patron/getphoto?libId=" + HttpUtility.UrlEncode(libId)
                            + "&objectPath=" + HttpUtility.UrlEncode(strImgUri);
            return View();
        }

        // 书目查询主界面
        public ActionResult Index(string code, string state)
        {
            // 登录检查
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Biblio/Index?a=1");// ("书目查询", "/Biblio/Index?a=1", lib.libName);
                return View();
            }

            // 不允许外部访问，转到绑定帐号界面
            if (sessionInfo.CurrentLib.Entity.noShareBiblio == 1)
            {
                List<WxUserItem> users = WxUserDatabase.Current.Get(sessionInfo.WeixinId, sessionInfo.CurrentLib.Entity.id, -1);
                if (users.Count == 0)
                {
                    ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("书目查询", "/Biblio/Index?a=1", sessionInfo.CurrentLib.Entity.libName);
                    return View();
                }
            }

            // 为啥要给ViewBag设置证条码号？ 2020-2-7
            // 主要是一些预约和续借的功能，需要用到证条码号
            ViewBag.PatronBarcode = sessionInfo.ActiveUser.readerBarcode;

            // 检索匹配方式
            string match = sessionInfo.CurrentLib.Entity.match;
            if (String.IsNullOrEmpty(match) == true)
                match = "left";
            ViewBag.Match = match;

            if (sessionInfo.ActiveUser != null
                && sessionInfo.ActiveUser.type == WxUserDatabase.C_Type_Worker
                && sessionInfo.ActiveUser.userName != WxUserDatabase.C_Public)
            {
                ViewBag.Worker = sessionInfo.ActiveUser.userName;
            }

            return View();
        }

        // 书目查询详细界面
        public ActionResult Detail(string code, string state, string biblioPath,string biblioName)
        {
            string strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(biblioPath) == true)
            {
                ViewBag.Error = "尚未传入biblioPath,请从书目查询窗进入。";
                return View();
            }

            //if (string.IsNullOrEmpty(biblioName) == true)
            //{
            //    ViewBag.Error = "尚未传入biblioName,请从书目查询窗进入。";
            //    return View();
            //}

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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Biblio/Detail");
                return View();
            }

            if (sessionInfo.ActiveUser != null
                && sessionInfo.ActiveUser.type == WxUserDatabase.C_Type_Worker
                && sessionInfo.ActiveUser.userName != WxUserDatabase.C_Public)
            {
                ViewBag.Worker = sessionInfo.ActiveUser.userName;
            }

            // 馆藏地
            string location = sessionInfo.GetLocation(out strError);
            if (string.IsNullOrEmpty(strError) == false)
            {
                ViewBag.Error = strError;
                return View();
            }
            string defaultLocation = "办公室"; //默认馆藏地todo，后面要修改为馆员在设置界面可以配置
            ViewBag.LocationHtml = this.GetLocationHtml(location, defaultLocation);

            // 图书类型
            string bookType = sessionInfo.GetBookType(out strError);
            if (string.IsNullOrEmpty(strError) == false)
            {
                ViewBag.Error = strError;
                return View();
            }
            ViewBag.BookTypeHtml = this.GetBookTypeHtml(bookType, "");
            ViewBag.BiblioName = biblioName;
            
            ViewBag.PatronBarcode = sessionInfo.ActiveUser.readerBarcode;
            ViewBag.BiblioPath = biblioPath;
            return View();
        }


        public string GetLocationHtml(string locationXml, string currentLocation)
        {
            string html = "";
            if (String.IsNullOrEmpty(locationXml) == false)
            {

                string location = "";
                // 解析本帐户拥有的全部馆藏地
                List<SubLib> subLibs = SubLib.ParseSubLib(locationXml, true);
                foreach (SubLib subLib in subLibs)
                {
                    foreach (Location loc in subLib.Locations)
                    {
                        string locPath = "";
                        if (string.IsNullOrEmpty(subLib.libCode) == true)
                            locPath = loc.Name;
                        else
                            locPath = subLib.libCode + "/" + loc.Name;

                        if (location != "")
                            location += ",";

                        location += locPath;

                    }
                }

                string[] list = location.Split(new char[] { ',' });
                foreach (string one in list)
                {
                    string temp = one;
                    int nIndex = one.IndexOf("}");
                    if (nIndex != -1)
                    {
                        temp = one.Substring(nIndex + 1).Trim();
                    }

                    string sel = "";
                    if (currentLocation == temp || list.Length==1) //2020/10/10 如果只有一项，则默认选中 
                        sel = " selected ";
                    html += "<option value='" + one + "' " + sel + ">" + one + "</option>";
                }
            }
            html = "<select id='selLocation' name='selLocation' class='selArrowRight'>"
                    + "<option value=''>请选择</option>"
                    + html
                    + "</select>";

            return html;
        }

        public string GetBookTypeHtml(string bookType, string currentBookType)
        {
            string html = "";
            if (String.IsNullOrEmpty(bookType) == false)
            {
                string[] list = bookType.Split(new char[] { ',' });
                foreach (string one in list)
                {
                    string temp = one;
                    int nIndex = one.IndexOf("}");
                    if (nIndex != -1)
                    {
                        temp = one.Substring(nIndex + 1).Trim();
                    }

                    string sel = "";
                    if (currentBookType == temp || list.Length == 1) //2020/10/10 如果只有一项，则默认选中 
                        sel = " selected ";
                    html += "<option value='" + one + "' " + sel + ">" + one + "</option>";
                }
            }
            html = "<select id='selBookType' name='selBookType' class='selArrowRight'>"
                    + "<option value=''>请选择</option>"
                    + html
                    + "</select>";

            return html;
        }
    }
}