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
        public ActionResult SelectLib(string code, string state, string returnUrl)
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


            ViewBag.returnUrl = returnUrl;

            // 2017-10-1 只列出可访问的图书馆
            List<Library> avaiblelibList = dp2WeiXinService.Instance.LibManager.GetLibraryByIds(sessionInfo.libIds);


            // 绑定的帐户
            List<WxUserItem> list = WxUserDatabase.Current.Get(sessionInfo.WeixinId, null, -1);

            // 可显示的域区
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
                        continue;
                    }

                    //如果不在可访问范围，不显示
                    if (thisLib != null && avaiblelibList.IndexOf(thisLib) == -1)
                    {
                        continue;
                    }

                    // 2019/08/05加，如果不加这一句，会把一些到期的图书馆列出来
                    if (thisLib == null)
                        continue;

                    libList.Add(lib);
                    //
                    if (this.CheckIsBind(list, lib) == true)  //libs.Contains(lib.libId)
                        lib.bindFlag = " * ";

                    if (sessionInfo.ActiveUser != null)
                    {
                        if (lib.libId == sessionInfo.ActiveUser.libId
                            && lib.libraryCode == sessionInfo.ActiveUser.bindLibraryCode)
                        {
                            lib.Checked = " checked ";
                        }
                    }
                }


                if (libList.Count > 0)
                {
                    Area newArea = new Area();
                    newArea.name = area.name;
                    newArea.libs = libList;
                    areaList.Add(newArea);
                }

            }

            ViewBag.areaList = areaList;

            return View();

        }

        public bool CheckIsBind(List<WxUserItem> list, libModel lib)
        {
            foreach (WxUserItem user in list)
            {
                if (user.libId == lib.libId)
                {
                    if (user.bindLibraryCode == lib.libraryCode)  //这里按bind帐户来
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        public ActionResult Setting(string code, string state, string returnUrl)
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state,"/Patron/Setting");
                return View();
            }

            // 返回url
            ViewBag.returnUrl = returnUrl;

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

            if (sessionInfo.ActiveUser.type == WxUserDatabase.C_Type_Worker 
                && sessionInfo.ActiveUser.userName!="public")
            {
                ViewBag.workerId = sessionInfo.ActiveUser.id;
                if (sessionInfo.ActiveUser.tracing == "on" || sessionInfo.ActiveUser.tracing == "on -mask")
                {
                    tracingChecked = " checked='checked' ";
                    maskChecked = " checked='checked' ";
                    if (sessionInfo.ActiveUser.tracing == "on -mask")
                        maskChecked = " ";
                }
            }
            ViewBag.tracingChecked = tracingChecked;
            ViewBag.maskChecked = maskChecked;
            if (ViewBag.LibName == "[" + WeiXinConst.C_Dp2003LibName + "]")
            {
                ViewBag.info = "监控所有图书馆的消息";
            }

            ViewBag.subLibGray = "";
            // 未绑定帐户 ，todo 普通读者一样可选择关注馆藏地
            if (sessionInfo.ActiveUser == null 
                || sessionInfo.ActiveUser.userName =="public")
            {
                ViewBag.subLibGray = "color:#cccccc";
                return View();
            }

            string accountInfo = "";
            if (sessionInfo.ActiveUser.type == WxUserDatabase.C_Type_Worker)
            {
                accountInfo = "帐号:"+ sessionInfo.ActiveUser.userName;
            }
            else
            {
                accountInfo = "读者:" + sessionInfo.ActiveUser.readerBarcode;
            }
            if (accountInfo != "")
            {
                accountInfo = "(" + accountInfo + ")";
            }
            ViewBag.accountInfo = accountInfo;
            ViewBag.userId = sessionInfo.ActiveUser.id;

            string locationXml = sessionInfo.ActiveUser.location;
            if(String.IsNullOrEmpty(sessionInfo.ActiveUser.location)==true 
                && sessionInfo.ActiveUser.userName!="public")
            {
                // 从dp2服务器获取
                nRet =dp2WeiXinService.Instance.GetLocation(ViewBag.LibId,
                    sessionInfo.ActiveUser,
                   out locationXml,
                   out strError);
                if (nRet == -1)
                {
                    ViewBag.Error = strError;
                    return View();
                }


                //保存到微信用户库
                sessionInfo.ActiveUser.location = locationXml;
                WxUserDatabase.Current.Update(sessionInfo.ActiveUser);
            }


            // 解析本帐户拥有的全部馆藏地
            List<SubLib> subLibs = SubLib.ParseSubLib(locationXml,true);

            //上次选中的打上勾
            if (String.IsNullOrEmpty(sessionInfo.ActiveUser.selLocation)==false)
            {
                string selLocation = SubLib.ParseToSplitByComma(sessionInfo.ActiveUser.selLocation);
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
            if (sessionInfo.ActiveUser != null && sessionInfo.ActiveUser.verifyBarcode == 1)
            {
                ViewBag.verifyBarcode = "checked";
            }

            ViewBag.audioType = 1;
            if (sessionInfo.ActiveUser != null && sessionInfo.ActiveUser.audioType >0)
            {
                ViewBag.audioType = sessionInfo.ActiveUser.audioType;
            }

            return View();

        }


        #region 二维码 图片

        // 二维码
        public ActionResult QRcode(string code, string state)
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/QRcode");
                return View();
            }

            if (sessionInfo.ActiveUser != null 
                && sessionInfo.ActiveUser.type != WxUserDatabase.C_Type_Patron)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("二维码", "/Patron/QRcode");
                return View();
            }


            string strXml = "";
            string patronBarcode = "";
            string recPath = "";
            nRet = this.GetReaderXml(sessionInfo.ActiveUser,
               out strXml,
               out recPath,
               out strError);
            if (nRet == -1)
            {
                ViewBag.Error = strError;
                return View();
            }

            string warn = "";
            string qrcodeUrl = "";
            if (String.IsNullOrEmpty(warn) == true)
            {
                qrcodeUrl = "./getphoto?libId=" + HttpUtility.UrlEncode(sessionInfo.ActiveUser.libId)
                     + "&type=pqri"
                     + "&barcode=" + HttpUtility.UrlEncode(sessionInfo.ActiveUser.readerBarcode);
                //+ "&width=400&height=400";
            }
            ViewBag.qrcodeUrl = qrcodeUrl;


            return View(sessionInfo.ActiveUser);
        }

        // 图片
        public ActionResult GetPhoto(string code, string state,string libId, string type, string barcode, string objectPath)
        {
            MemoryStream ms = new MemoryStream(); ;
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1)//???为什么要做这一句 2020-2-8 && ViewBag.LibState != LibraryManager.C_State_Hangup1)
            {
                goto ERROR1;
            }


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
            string weixinId = ViewBag.weixinId;
            nRet = dp2WeiXinService.GetObject0(this, libId,weixinId,objectPath, out strError);
            if (nRet == -1)
                goto ERROR1;

            return null;


        ERROR1:

            ms = dp2WeiXinService.Instance.GetErrorImg(strError);
            return File(ms.ToArray(), "image/jpeg");
        }

        // 资源
        public ActionResult GetObject(string code, string state,string libId,string uri)
        {
            string strError = "";
            int nRet = 0;

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code, state,
                out SessionInfo sessionInfo,
                out strError);
            if (nRet == -1) //??? 为什么加后面部分 2020-2-8 && ViewBag.LibState != LibraryManager.C_State_Hangup1)
            {
                goto ERROR1;
            }

            //处理 dp2 系统外部的 URL
            Uri tempUri = dp2WeiXinService.GetUri(uri);
            if (tempUri != null
                && (tempUri.Scheme == "http" || tempUri.Scheme == "https"))
            {
                return Redirect(uri);
            }

            string weixinId = ViewBag.weixinId;
            nRet = dp2WeiXinService.GetObject0(this, libId, weixinId, uri, out strError);
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
        public ActionResult PersonalInfo(string code, string state)
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state,"/Patron/PersonalInfo");
                return View();
            }

            if (sessionInfo.ActiveUser.type != WxUserDatabase.C_Type_Patron)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("我的信息", "/Patron/PersonalInfo");
                return View();
            }


            string patronXml = "";
            string recPath = "";
   
             nRet = this.GetReaderXml(sessionInfo.ActiveUser,
                out patronXml,
                out recPath,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                ViewBag.Error = strError;
                return View();
            }



            ViewBag.overdueUrl = "../Patron/OverdueInfo";
            ViewBag.borrowUrl = "../Patron/BorrowInfo";
            ViewBag.reservationUrl = "../Patron/Reservation";

            string libId = ViewBag.LibId;
            Patron patron = null;
            patron = dp2WeiXinService.Instance.ParsePatronXml(libId,
                    patronXml,
                    recPath,
                    ViewBag.showPhoto);
            return View(patron);

        }

        //违约交费信息
        public ActionResult OverdueInfo(string code, string state)
        {
            string strError = "";
            int nRet = 0;

            string patronXml = "";
            string recPath = "";

            /*
            SessionInfo sessionInfo = this.GetSessionInfo1();
            if (sessionInfo == null)
            {
                strError = "session失效";
                goto ERROR1;
            }
            nRet = this.InitViewBag(sessionInfo, out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            WxUserItem activeUserItem = sessionInfo.ActiveUser;
            */

            // 获取当前sessionInfo，里面有选择的图书馆和帐号等信息
            // -1 出错
            // 0 成功
            nRet = this.GetSessionInfo(code,state,
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/PersonalInfo");
                return View();
            }

            if (sessionInfo.ActiveUser.type != WxUserDatabase.C_Type_Patron)
            {
                strError = "当前帐户不是读者帐户";
                goto ERROR1;
            }
            
            ViewBag.patronBarcode = sessionInfo.ActiveUser.readerBarcode;

             nRet = this.GetReaderXml(sessionInfo.ActiveUser,
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
        public ActionResult Reservation(string code, string state)
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/PersonalInfo");
                return View();
            }

            if (sessionInfo.ActiveUser.type != WxUserDatabase.C_Type_Patron)
            {
                strError = "当前帐户不是读者帐户";
                goto ERROR1;
            }
            // 放到界面的变量
            ViewBag.patronBarcode = sessionInfo.ActiveUser.readerBarcode;

            string patronXml = "";
            string recPath = "";
             nRet = this.GetReaderXml(sessionInfo.ActiveUser,
                out patronXml,
                out recPath,
                out strError);
            if (nRet == -1 || nRet == 0 || nRet == -2)
                goto ERROR1;
            

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
        public ActionResult BorrowInfo(string code, string state)
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
                ViewBag.RedirectInfo = dp2WeiXinService.GetSelLibLink(state, "/Patron/PersonalInfo");
                return View();
            }

            if (sessionInfo.ActiveUser.type != WxUserDatabase.C_Type_Patron)
            {
                strError = "当前帐户不是读者帐户";
                ViewBag.Error = strError;
                return View();
            }
            ViewBag.patronBarcode = sessionInfo.ActiveUser.readerBarcode;

            string patronXml = "";
            string recPath = "";

            nRet = this.GetReaderXml(sessionInfo.ActiveUser,
                out patronXml,
                out recPath,
                out strError);
            if (nRet == -1 || nRet == 0 || nRet == -2)
            {
                ViewBag.Error = strError;
                return View();
            }



            string strWarningText = "";
            string maxBorrowCountString = "";
            string curBorrowCountString = "";
            List<BorrowInfo2> overdueList = dp2WeiXinService.Instance.GetBorrowInfo(patronXml,
                out strWarningText,
                out maxBorrowCountString,
                out curBorrowCountString);
            ViewBag.maxBorrowCount = maxBorrowCountString;
            ViewBag.curBorrowCount = curBorrowCountString;


            return View(overdueList);


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
        private int GetReaderXml(WxUserItem activeUser,
            out string patronXml,
            out string recPath,
            out string strError)
        {
            patronXml = "";
            strError = "";
            recPath = "";
            int nRet = 0;
            if (activeUser == null)
            {
                strError = "activeUser参数不能为空";
                return -1;
            }

            if (activeUser.type != WxUserDatabase.C_Type_Patron)
            {
                strError = "当前活动需为读者帐号";
                return -1;
            }


            string libId = activeUser.libId;
            string patronBarcode = activeUser.readerBarcode;

            // 登录人是读者自己
            string loginUserName = activeUser.readerBarcode;
            bool isPatron = true;

            string searchWord = patronBarcode;
            if (patronBarcode.Length > 7 && patronBarcode.Substring(0, 7) == "@refid:")
            {
                searchWord ="@path:"+ activeUser.recPath;
            }

            // 获取读者记录
            LoginInfo loginInfo = new LoginInfo("", false);//new LoginInfo(loginUserName, isPatron);
            string timestamp = "";
            nRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                loginInfo,
                searchWord,
                "advancexml",  // 格式
                out recPath,
                out timestamp,
                out patronXml,
                out strError);
            if (nRet == -1 || nRet == 0)
                return -1;

            activeUser.recPath = recPath; // todo 应该在绑定的时候赋值，但绑定时没有返回路径
            //todo1 要保存到数据库里


            return 1;
        }


        #endregion
    }
}