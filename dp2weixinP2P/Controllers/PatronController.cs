using DigitalPlatform.IO;
using DigitalPlatform.Message;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using dp2Command.Service;
using dp2weixin.service;
using dp2weixinWeb.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace dp2weixinWeb.Controllers
{
    public class PatronController : BaseController
    {
        public ActionResult SelectLib(string returnUrl)
        {
            string strError = "";
            // 检查是否从微信入口进来
            int nRet = this.CheckIsFromWeiXin("", "", out strError, false);
            if (nRet == -1)
            {
                goto ERROR1;
            }
            ViewBag.returnUrl = returnUrl;

            SessionInfo sessionInfo = this.GetSessionInfo();

            // 2017-2-28不可能出现这种情况了
            if (sessionInfo == null)
            {
                //string libHomeUrl = dp2WeiXinService.Instance.GetOAuth2Url(sessionInfo.gzh, "Library/Home");
                strError = "页面超时，请从微信窗口重新进入。";//请重新从微信\"我爱图书馆\"公众号进入。"; //Sessin
                goto ERROR1;
            }
            // 检查session是否超时
            if (String.IsNullOrEmpty(sessionInfo.WeixinId) == true)
            {
                strError = "异常：没有weixinId。";
                goto ERROR1;
            }


            // 2017-10-1 只列出可访问的图书馆
            List<Library> avaiblelibList = dp2WeiXinService.Instance.LibManager.GetLibraryByIds(sessionInfo.libIds);


            // 得到该微信用户绑定过的图书馆列表
            //List<string> libs = WxUserDatabase.Current.GetLibsByWeixinId(sessionInfo.WeixinId);




            List<WxUserItem> list = WxUserDatabase.Current.Get(sessionInfo.WeixinId, null, -1);

            List<Area> areaList = new List<Area>();

            foreach(Area area in dp2WeiXinService.Instance.areaMgr.areas)
            {
                //area.visible = true;
                //int disVisibleCout = 0;

                List<libModel> libList = new List<libModel>();
                foreach (libModel lib in area.libs)
                {
                   // lib.visible = true;
                    lib.Checked = "";
                    lib.bindFlag = "";

                    // 如果是到期的图书馆，不显示出来
                    Library thisLib = dp2WeiXinService.Instance.LibManager.GetLibrary(lib.libId);//.GetLibById(lib.libId);
                    if (thisLib != null && thisLib.Entity.state == "到期")
                    {
                        //lib.visible = false;
                        //disVisibleCout++;
                        continue;
                    }

                    //如果不在可访问范围，不显示
                    if (thisLib != null && avaiblelibList.IndexOf(thisLib) == -1)
                    {
                        //lib.visible = false;
                        //disVisibleCout++;
                        continue;
                    }

                    libList.Add(lib);
                    //
                    if (this.CheckIsBind(list, lib) == true)  //libs.Contains(lib.libId)
                        lib.bindFlag = " * ";

                    if (lib.libId == sessionInfo.CurrentLib.Entity.id && lib.name == sessionInfo.CurrentLibName)
                        lib.Checked = " checked ";
                }

                // 如果下级图书馆都是到期状态，则地址不显示
                //if (disVisibleCout == area.libs.Count)
                //{
                //    area.visible = false;
                //}

                if (libList.Count > 0)
                {
                    Area newArea = new Area();
                    newArea.name = area.name;
                    newArea.libs = libList;
                    areaList.Add(newArea);
                }

            }

            ViewBag.areaList = areaList;// dp2WeiXinService.Instance.areaMgr.areas;

            return View();

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }

        public bool CheckIsBind(List<WxUserItem> list, libModel lib)
        {
            foreach (WxUserItem user in list)
            {
                if (user.libId == lib.libId)
                {
                    if (string.IsNullOrEmpty(lib.libraryCode)==false)
                    {
                        if (user.libraryCode == lib.libraryCode)
                            return true;
                        else
                            return false;
                    }

                    return true;
                }
            }
            return false;
        }

        public ActionResult Setting(string code, string state, string returnUrl)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
            {
                if (ViewBag.LibState != LibraryManager.C_State_Hangup)//图书馆挂起，数字平台界面可用
                    goto ERROR1;
            }

            string weixinId = ViewBag.weixinId; //(string)Session[WeiXinConst.C_Session_WeiXinId];
            ViewBag.returnUrl = returnUrl;

            //// 图书馆html
            //string selLibHtml = "";
            //nRet = this.GetLibSelectHtml(ViewBag.LibId, 
            //    weixinId, 
            //    true,
            //    "save()",
            //    out selLibHtml,
            //    out strError);
            //if (nRet==-1)
            //{
            //    goto ERROR1;
            //}
            //ViewBag.LibHtml = selLibHtml;

            string photoChecked = "";
            if (ViewBag.showPhoto == 1)
                photoChecked = " checked='checked' ";
            ViewBag.photoChecked = photoChecked;

            string coverChecked = "";
            if (ViewBag.showCover == 1)
                coverChecked = " checked='checked' ";
            ViewBag.coverChecked = coverChecked;

            // 检查是否绑定工作人员，决定界面上是否出现 打开监控功能
            ViewBag.info = "监控本馆消息";
            string tracingChecked = "";
            string maskChecked = "";
            WxUserItem worker = WxUserDatabase.Current.GetWorker(weixinId, ViewBag.LibId);
            if (worker != null)
            {
                ViewBag.workerId = worker.id;
                if (worker.tracing == "on" || worker.tracing == "on -mask")
                {
                    tracingChecked = " checked='checked' ";
                    maskChecked = " checked='checked' ";
                    if (worker.tracing == "on -mask")
                        maskChecked = " ";
                }
            }
            ViewBag.tracingChecked = tracingChecked;
            ViewBag.maskChecked = maskChecked;
            if (ViewBag.LibName == "[" + WeiXinConst.C_Dp2003LibName + "]")
            {
                ViewBag.info = "监控所有图书馆的消息";
            }

            //选择分馆与馆藏地
            WxUserItem user = worker;
            if (user==null)
            {
                // 取读者帐户
                user = WxUserDatabase.Current.GetActivePatron(weixinId, ViewBag.LibId);
            }

            ViewBag.subLibGray = "";
            // 未绑定帐户 ，todo 普通读者一样可选择关注馆藏地
            if (user == null || 
                (user.type==WxUserDatabase.C_Type_Patron && user.rights.Contains("borrow") == false && user.rights.Contains("return") == false))
            {
                //string bindUrl = "/Account/Bind?returnUrl=" + HttpUtility.UrlEncode(returnUrl);
                //string bindLink = "<a href='javascript:void(0)' onclick='gotoUrl(\"" + bindUrl + "\")'>尚未绑定帐户</a>。";
                //ViewBag.bindLink = bindLink;

                ViewBag.subLibGray = "color:#cccccc";
                return View();
            }

            string accountInfo = "";
            if (user.type == WxUserDatabase.C_Type_Worker)
            {
                accountInfo = "帐号:"+user.userName;
            }
            else
            {
                accountInfo = "读者:" + user.readerBarcode;
            }
            if (accountInfo != "")
            {
                accountInfo = "(" + accountInfo + ")";
            }
            ViewBag.accountInfo = accountInfo;
            ViewBag.userId = user.id;

            string locationXml = user.location;
            if(String.IsNullOrEmpty(user.location)==true)
            {
                // 从dp2服务器获取
                nRet =dp2WeiXinService.Instance.GetLocation(ViewBag.LibId,
                    user,
                   out locationXml,
                   out strError);
                if (nRet == -1)
                    goto ERROR1;


                //保存到微信用户库
                user.location = locationXml;
                WxUserDatabase.Current.Update(user);
            }


            // 解析本帐户拥有的全部馆藏地
            List<SubLib> subLibs = SubLib.ParseSubLib(locationXml,true);

            //上次选中的打上勾
            if (String.IsNullOrEmpty(user.selLocation)==false)
            {
                string selLocation = SubLib.ParseToSplitByComma(user.selLocation);
                if (selLocation != "")
                {
                    string[] selLocList = selLocation.Split(new char[] { ',' });
                    foreach (SubLib subLib in subLibs)
                    {
                        foreach (Location loc in subLib.Locations)
                        {
                            string locPath = subLib.libCode + "/" + loc.Name;
                            if (selLocList.Contains(locPath) == true)
                            {
                                subLib.Checked = "checked";
                                loc.Checked = "checked";
                            }
                        }
                    }
                }
                // end
            }

            // todo 其实，可以用一个字段来表示馆藏地和选中的项，就量在xml的字段中加checked属性，
            // 但如果服务器更新了，刷的时候就全部覆盖了。
            // 现在还没做到服务器更新后，自动刷过来


            ViewBag.libList = subLibs;


            ViewBag.verifyBarcode = "";
            if (user != null && user.verifyBarcode == 1)
            {
                ViewBag.verifyBarcode = "checked";
            }

            return View();

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }


        #region 二维码 图片

        // 二维码
        public ActionResult QRcode(string code, string state)
        {
            string strError = "";

            string strXml = "";
            string patronBarcode = "";
            string recPath = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(code, 
                state,
                null,
                ref patronBarcode,
                "", 
                out activeUserItem, 
                out strXml,
                out recPath,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            if (nRet == -2)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("二维码", "/Patron/QRcode");
                return View();
            }

            // 如果图书馆是挂起状态，作为警告
            string libId = activeUserItem.libId;
            Library lib = dp2WeiXinService.Instance.LibManager.GetLibrary(libId);
            string warn = LibraryManager.GetLibHungWarn(lib);
            ViewBag.Warn = warn;

            string qrcodeUrl = "";
            if (String.IsNullOrEmpty(warn) == true)
            {
                qrcodeUrl = "./getphoto?libId=" + HttpUtility.UrlEncode(libId)
                     + "&type=pqri"
                     + "&barcode=" + HttpUtility.UrlEncode(activeUserItem.readerBarcode);
                //+ "&width=400&height=400";
            }
            ViewBag.qrcodeUrl = qrcodeUrl;


            return View(activeUserItem);

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }

        // 图片
        public ActionResult GetPhoto(string libId, string type, string barcode, string objectPath)
        {
            MemoryStream ms = new MemoryStream(); ;
            string strError = "";
            int nRet = 0;


            // 读者二维码
            if (type == "pqri")
            {                
                // 设置媒体类型
                Response.ContentType = "image/jpeg";

                // 获得读者证号二维码字符串
                string strCode = "";
                nRet = dp2WeiXinService.Instance.GetQRcode(libId,
                    barcode,
                    out strCode,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;

                // 获得二维码图片
                string strWidth = Request.QueryString["width"];
                string strHeight = Request.QueryString["height"];
                int nWidth = 0;
                if (string.IsNullOrEmpty(strWidth) == false)
                {
                    if (Int32.TryParse(strWidth, out nWidth) == false)
                    {
                        strError = "width 参数 '" + strWidth + "' 格式不合法";
                        goto ERROR1;
                    }
                }
                int nHeight = 0;
                if (string.IsNullOrEmpty(strHeight) == false)
                {
                    if (Int32.TryParse(strHeight, out nHeight) == false)
                    {
                        strError = "height 参数 '" + strHeight + "' 格式不合法";
                        goto ERROR1;
                    }
                }
                dp2WeiXinService.Instance.GetQrImage(strCode,
                   nWidth,
                   nHeight,
                   Response.OutputStream,
                   out strError);
                if (strError != "")
                    goto ERROR1;
                return null;
            }

            // 取头像 或 封面
            nRet = dp2WeiXinService.GetObject0(this, libId, objectPath, out strError);
            if (nRet == -1)
                goto ERROR1;

            return null;


        ERROR1:

            ms = dp2WeiXinService.Instance.GetErrorImg(strError);
            return File(ms.ToArray(), "image/jpeg");
        }

        // 资源
        public ActionResult GetObject(string libId,string uri)
        {
            string strError = "";
            int nRet = 0;

            //处理 dp2 系统外部的 URL
            Uri tempUri = dp2WeiXinService.GetUri(uri);
            if (tempUri != null
                && (tempUri.Scheme == "http" || tempUri.Scheme == "https"))
            {
                return Redirect(uri);
            }

            nRet = dp2WeiXinService.GetObject0(this, libId, uri, out strError);
            if (nRet == -1)
                goto ERROR1;
 
            return null;


        ERROR1:
            MemoryStream ms =  dp2WeiXinService.Instance.GetErrorImg(strError);
            return File(ms.ToArray(), "image/jpeg");
        }


        #endregion



        /// <summary>
        /// 我的信息主界面
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult PersonalInfo(string code, string state,
            string loginUserName,
            string patronBarcode)
        {
            string strError = "";

            string patronXml = "";
            string recPath = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(code, 
                state,
                loginUserName,
                ref patronBarcode,
                "advancexml",
                out activeUserItem,
                out patronXml,
                out recPath,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            if (nRet == -2)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("我的信息", "/Patron/PersonalInfo");
                return View();
            }

            ViewBag.userName = loginUserName;
            ViewBag.patronBarcode = patronBarcode;

            ViewBag.overdueUrl = "../Patron/OverdueInfo?loginUserName="+loginUserName+"&patronBarcode="+patronBarcode;
            ViewBag.borrowUrl = "../Patron/BorrowInfo?loginUserName=" + loginUserName + "&patronBarcode=" + patronBarcode;
            ViewBag.reservationUrl = "../Patron/Reservation?loginUserName=" + loginUserName + "&patronBarcode=" + patronBarcode;

            string libId = ViewBag.LibId;
            Patron patron = null;
            patron = dp2WeiXinService.Instance.ParsePatronXml(libId,
                    patronXml,
                    recPath,
                    ViewBag.showPhoto);
            return View(patron);

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }

        //违约交费信息
        public ActionResult OverdueInfo(string loginUserName, string patronBarcode)
        {
            string strError = "";
            string patronXml = "";
            string recPath = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(null, null,
                loginUserName,
                ref patronBarcode,
                "advancexml",
                out activeUserItem,
                out patronXml,
                out recPath,
                out strError);
            if (nRet == -1 || nRet == 0 || nRet == -2)
                goto ERROR1;

            string strWarningText = "";
            List<OverdueInfo> overdueList = dp2WeiXinService.Instance.GetOverdueInfo(patronXml, 
                out strWarningText);

            return View(overdueList);

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }

        /// <summary>
        /// 预约请求界面
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult Reservation(string loginUserName, string patronBarcode)
        {
            string strError = "";
            string patronXml = "";
            string recPath = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(null, null,
                loginUserName,
                ref patronBarcode,
                "advancexml",
                out activeUserItem,
                out patronXml,
                out recPath,
                out strError);
            if (nRet == -1 || nRet == 0 || nRet == -2)
                goto ERROR1;

            // 放到界面的变量
            ViewBag.patronBarcode = patronBarcode;

            // 预约请求
            string strReservationWarningText = "";
            List<ReservationInfo> reservations = dp2WeiXinService.Instance.GetReservations(patronXml,
                out strReservationWarningText);


            return View(reservations);

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }

        /// <summary>
        /// 在借续借界面
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult BorrowInfo(string loginUserName,string patronBarcode)
        {
            string strError = "";
            string patronXml = "";
            string recPath = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(null, null,
                loginUserName,
                ref patronBarcode,
                "advancexml", 
                out activeUserItem,
                out patronXml,
                out recPath,
                out strError);
            if (nRet == -1 || nRet == 0 || nRet==-2)
                goto ERROR1;

            // 放到界面的变量
            ViewBag.patronBarcode = patronBarcode;

            string strWarningText = "";
            string maxBorrowCountString="";
            string curBorrowCountString="";
            List<BorrowInfo2> overdueList = dp2WeiXinService.Instance.GetBorrowInfo(patronXml,
                out strWarningText,
                out maxBorrowCountString,
                out curBorrowCountString);
            ViewBag.maxBorrowCount = maxBorrowCountString;
            ViewBag.curBorrowCount = curBorrowCountString;


            return View(overdueList);

        ERROR1:
            ViewBag.Error = strError;
            return View();
        }

        #region 内部函数



        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns>
        /// -1 出错，或者非正常途径登录
        /// -2 未绑定 
        /// -3 未设置默认账户
        /// 0 未找到读者记录
        /// 1 成功
        /// </returns>
        private int GetReaderXml(string code, 
            string state,
            string loginUserName,
            ref string patronBarcode,
            string strFormat,
            out WxUserItem activeUserItem,
            out string patronXml,
            out string recPath,
            out string strError)
        {
            patronXml = "";
            activeUserItem = null;
            strError = "";
            recPath = "";

            // 检查是否从微信入口进来
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return -1;

            string libId=ViewBag.LibId;
            bool isPatron = false;
            if (String.IsNullOrEmpty(loginUserName) == false)
            {
                isPatron = false;
                if (string.IsNullOrEmpty(patronBarcode) == true)
                {
                    strError = "异常：当传了loginUserName参数时，必须传patronBarcode叁数";
                    return -1;
                }
            }
            else
            {
                //if (string.IsNullOrEmpty(patronBarcode) == false)
                //{
                //    strError = "异常：当未传loginUserName参数时，则不需传patronBarcode参数";
                //    return -1;
                //}
                string weixinId = ViewBag.weixinId; //(string)Session[WeiXinConst.C_Session_WeiXinId];
                activeUserItem = WxUserDatabase.Current.GetActivePatron(weixinId, libId);
                // 未绑定读者账户,不会出现未激活的情况
                if (activeUserItem == null)
                {
                    strError = "当前没有活动读者账户";
                    return -2;
                }
                patronBarcode = activeUserItem.readerBarcode;

                // 登录人是读者自己
                loginUserName = activeUserItem.readerBarcode;
                isPatron = true;
            }

            if (string.IsNullOrEmpty(strFormat) == false)
            {
                // 获取读者记录
                LoginInfo loginInfo = new LoginInfo(loginUserName, isPatron);
                string timestamp = "";
                nRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                    loginInfo,
                    patronBarcode,
                    "advancexml",
                    out recPath,
                    out timestamp,
                    out patronXml,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    return -1;

                if (activeUserItem != null)
                    activeUserItem.recPath = recPath; // todo 应该在绑定的时候赋值，但绑定时没有返回路径

            }

            return 1;
        }


        #endregion
    }
}