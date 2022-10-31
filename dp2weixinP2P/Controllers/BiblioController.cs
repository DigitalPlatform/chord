using common;
using DigitalPlatform.Marc;
using DigitalPlatform.Message;
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
        // 册查询界面
        public ActionResult SearchItem(string code, string state)
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Biblio/SearchItem");// ("书目查询", "/Biblio/Index?a=1", lib.libName);
                return View();
            }

            // 不允许外部访问，转到绑定帐号界面
            if (sessionInfo.CurrentLib.Entity.noShareBiblio == 1)
            {
                List<WxUserItem> users = WxUserDatabase.Current.Get(sessionInfo.WeixinId, sessionInfo.CurrentLib.Entity.id, -1);
                if (users.Count == 0)
                {
                    ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("册查询", "/Biblio/SearchItem", sessionInfo.CurrentLib.Entity.libName);
                    return View();
                }
            }

            //// 为啥要给ViewBag设置证条码号？ 2020-2-7
            // 主要是一些预约和续借的功能，需要用到证条码号
            ViewBag.PatronBarcode = sessionInfo.ActiveUser.readerBarcode;

            //// 检索匹配方式
            //string match = sessionInfo.CurrentLib.Entity.match;
            //if (String.IsNullOrEmpty(match) == true)
            //    match = "left";
            //ViewBag.Match = match;

            if (sessionInfo.ActiveUser != null
                && sessionInfo.ActiveUser.type == WxUserDatabase.C_Type_Worker
                && sessionInfo.ActiveUser.userName != WxUserDatabase.C_Public)
            {
                ViewBag.Worker = sessionInfo.ActiveUser.userName;
            }

            return View();
        }

        // 简编界面
        public ActionResult BiblioEdit(string code, string state, string biblioPath)
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Biblio/BiblioEdit");
                return View();
            }

            if (sessionInfo.ActiveUser != null
                && sessionInfo.ActiveUser.type == WxUserDatabase.C_Type_Worker
                && sessionInfo.ActiveUser.userName != WxUserDatabase.C_Public)
            {
                ViewBag.Worker = sessionInfo.ActiveUser.userName;
            }

            // 可编辑字段配置
            string biblioDbName = "";
            string fieldMap = dp2WeiXinService.Instance.GetFieldsMap(sessionInfo.ActiveUser.libId,
                sessionInfo.ActiveUser.bindLibraryCode,
                out biblioDbName);
            if (string.IsNullOrEmpty(fieldMap) == true)
            {
                if (string.IsNullOrEmpty(biblioDbName) == true)
                {
                    ViewBag.Error = "尚未配置简单编目的字段，请联系管理员。";
                    return View();
                }
            }

            // 得到一个缺少值的头标区
            string strHeader = MarcFixedFieldManager.GetMarcHeaderText();

            string btnName = "保存";
            string timestamp = "";

            // 新增时，必须要在配置文件中配置好目标数据库
            if (string.IsNullOrEmpty(biblioPath) == true)
            {
                if (string.IsNullOrEmpty(biblioDbName) == true)
                {
                    ViewBag.Error = "尚未配置目标书目库，无法新增书目，请联系管理员。";
                    return View();
                }

                btnName = "新增";
                biblioPath = biblioDbName + "/?";
                ViewBag.biblioAction = "new";
            }
            else
            {
                // 馆员身份
                LoginInfo loginInfo = new LoginInfo(sessionInfo.ActiveUser.userName, false);
                // 根据id找到图书馆对象
                LibEntity lib = dp2WeiXinService.Instance.GetLibById(sessionInfo.ActiveUser.libId);
                if (lib == null)
                {
                    ViewBag.Error = "未找到id为[" + sessionInfo.ActiveUser.libId + "]的图书馆定义。";
                    return View();
                }

                List<string> dataList = null;
                // 把书目记录获取出来（包括时间戳），把相关字段内容取出来

                nRet = dp2WeiXinService.Instance.GetBiblioInfo(lib,
                    loginInfo,
                    biblioPath,
                   "xml,timestamp",
                    out dataList,
                    out strError);
                if (nRet == -1 || nRet == 0)
                {
                    ViewBag.Error = "获取书目["+biblioPath+"]出错："+strError;
                    return View();
                }
                // 书目原始xml
                string oldbiblioXml = dataList[0];

                // 从marc中取出字段的值
                MarcRecord marcRecord = MarcHelper.MarcXml2MarcRecord(oldbiblioXml, out string outMarcSyntax, out strError);

                // 头标区
                strHeader= marcRecord.Header.ToString();
                
                // 从marc中抽取字段,再变成一组有值的fieldMap字段串
                fieldMap=MarcHelper.GetFields(marcRecord, fieldMap);

                // 时间戳
                timestamp = dataList[1];
                ViewBag.biblioTimestamp=timestamp;
                ViewBag.biblioAction = "change";
            }


            string html = "";

            // 书目路径
            html += @"<div class='mui-input-row '>"
                + "<label  style='color:#cccccc'>书目路径</label>"
                + "<input id='biblioPath' disabled type='text' class=' mui-input mui-input-clear' style='color:#bbbbbb'  value='" + biblioPath + "'/>"
                + "</div>";

            // 解析marc字段字符串
            List<FieldItem> fieldList = new List<FieldItem>();
            try
            {
                fieldList = MarcHelper.ParseFieldMap(fieldMap);
            }
            catch (Exception ex)
            {
                @ViewData["marcField"] = "解析marc字段配置规则出错：" + ex.Message;
                return View();
            }


        //    // 加头标区
        //    string headerHtml = MarcHeaderHelper.GetHeaderHtml(strHeader);
        //    html += @"<table style='width:100%;border-bottom:1px solid #eeeeee;'>
        //    <tr>
        //        <td style='width:150px;padding-left:15px;padding-top:10px;vertical-align:top'>
        //                    <label style='color:#cccccc;'>头标区</label>
        //        </td>
        //        <td>" + headerHtml+@"</td>
        //    </tr>
        //</table>";

            //加一个集合里，方便界面的js使用
            List<string> fixedFileds = new List<string>();

            MarcFixedFieldManager fixedFieldMgr = dp2WeiXinService.Instance._marcFixedFieldMgr;
            // 字段
            foreach (FieldItem field in fieldList)
            {
                // 这个id很长，但很有意义，后面组合编辑后的字符串时，直接使用。
                string id = field.lable + "|" + field.name; //有可能没有子字段//field.field + "$" + field.subfield;

                MarcFixedField fixedField = fixedFieldMgr.GetFixedField(field.name);
                if (fixedField != null)  // 找到对应的定长字段配置，按定长样式拼html
                {

                    // 先得把原有的值设进去
                    if (string.IsNullOrEmpty(field.value) == false)
                    {
                        fixedField.SetValue(field.value);
                    }


                    // 获取这个定长字段的html
                    string inputIds = "";
                    string fieldhtml = MarcFixedFieldManager.GetHtml(fixedField,out inputIds);

                    // 加到定长字段名称集合中
                    fixedFileds.Add(id + "-" + inputIds);
                    
                    // 外面用一个table括起来，与其它行整合
                    html += "<table style='width:100%;border-bottom:1px solid #eeeeee;'>"
                                +"<tr>"
                                    + "<td style='min-width:150px;padding-left:15px;padding-top:10px;vertical-align:top'>"
                                        + "<label style='color:#cccccc;vertical-align:bottom'>" + fixedField.lable+ "<span style='font-size:1px'>" + fixedField.name + "</span>" +"</label>"
                                    + "</td> "
                                    + "<td style='width:100%'>" + fieldhtml + @"</td>"
                                + " </tr>"
                            +"</table>";
                }
                else
                {
                    html += @"<div class='mui-input-row ' >"
                    + "<label  style='color:#cccccc;vertical-align:bottom'>" + field.lable+ "<span style='font-size:1px;'>" + field.name+ "</label>"
                    + "<input id='" + id + "' type='text' class='_field mui-input mui-input-clear' value='" + field.value + "' />"
                + "</div>";
                }
            }

            //// 操作按钮
            //html += @"<div class='mui-content-padded'><table style='width:100%'><tr>"
            //    + "<td><button id='btnOpeType' class='mui-btn mui-btn-block mui-btn-default' onclick='saveBiblio()'>" + btnName + "</button></td>"
            //     + "<td width='10px'>&nbsp;</td>"
            //      + "<td><button class='mui-btn mui-btn-block mui-btn-default' onclick='cancelEdit()'>取消</button></td>"
            //      + "</tr></table></div>";

            // 一个按钮，三个锚点
            string biblioEditUrl = "/Biblio/BiblioEdit";
            string detailUrl = "/Biblio/Detail?biblioPath=" + HttpUtility.UrlEncode(biblioPath);
            string biblioSearchUrl = "/Biblio/Index";

            // 界面是新增的状态，不显示再次新增和详情
            string style = "";
            if (ViewBag.biblioAction=="new")
                style = "style='display: none'";

            html += @"<div class='mui-content-padded'>"
                + "<button id='btnOpeType' class='mui-btn mui-btn-block mui-btn-primary' onclick='saveBiblio()'>" + btnName + "</button>"
                + "<div class='link-area'><center>"
                + "<a id='again' "+style+" href='JavaScript:void(0)' onclick='gotoUrl(\"" + biblioEditUrl+"\")'>新增书目</a>"
                + "&nbsp;&nbsp;<a id='detail' "+style+" href='JavaScript:void(0)'  onclick='gotoUrl(\"" + detailUrl + "\")'>查看书目详情</a>"
                                + "&nbsp;&nbsp;<a href='JavaScript:void(0)' onclick='gotoUrl(\"" + biblioSearchUrl + "\")'>返回书目查询</a>"

                + "</center></div></div>";

            // 加外壳
            html = "<div class='mui-input-group' id='_marcEditor'>"
                + html
                + "</div>";

            // 把拼出来的字段放在viewdata里，到时显示在前端界面
            @ViewData["marcField"] = html;


            // 定长字段
            ViewData["fixedFileds"] = fixedFileds;

            return View();
        }

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
            int totalPage = dp2WeiXinService.Instance.GetPDFCount(libId, uri,
                out filename, out strError);
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
        public ActionResult Detail(string code, string state, string biblioPath, string biblioName)
        {
            string strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(biblioPath) == true)
            {
                ViewBag.Error = "尚未传入biblioPath,请从书目查询窗进入。";
                return View();
            }


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
                    if (currentLocation == temp || list.Length == 1) //2020/10/10 如果只有一项，则默认选中 
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
                    html += "<option value='" + temp + "' " + sel + ">" + one + "</option>";  //2022/10/27 改：value里放的是不带括号的
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