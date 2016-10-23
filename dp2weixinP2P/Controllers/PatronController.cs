using DigitalPlatform.IO;
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

            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            ViewBag.returnUrl = returnUrl;

            // 图书馆html
            ViewBag.LibHtml = this.GetLibSelectHtml(ViewBag.LibId, weixinId, false);

            string photoChecked = "";
            if (ViewBag.showPhoto == 1)
                photoChecked = " checked='checked' ";
            ViewBag.photoChecked = photoChecked;

            string coverChecked = "";
            if (ViewBag.showCover == 1)
                coverChecked = " checked='checked' ";
            ViewBag.coverChecked = coverChecked;

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
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(code, state, "", out activeUserItem, out strXml, out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            if (nRet == -2)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("二维码", "/Patron/QRcode");
                return View();
            }

            string qrcodeUrl = "./getphoto?libId=" + HttpUtility.UrlEncode(activeUserItem.libId)
                + "&type=pqri"
                + "&barcode=" + HttpUtility.UrlEncode(activeUserItem.readerBarcode);
            //+ "&width=400&height=400";
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
        public ActionResult PersonalInfo(string code, string state)
        {
            string strError = "";

            string strXml = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(code, state, "advancexml", out activeUserItem, out strXml, out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            if (nRet == -2)
            {
                ViewBag.RedirectInfo = dp2WeiXinService.GetLinkHtml("我的信息", "/Patron/PersonalInfo");
                return View();
            }

            Patron model = null;
            if (activeUserItem != null)
            {
                model = dp2WeiXinService.Instance.ParsePatronXml(activeUserItem.libId,
                    strXml,
                    activeUserItem.recPath,
                    ViewBag.showPhoto);
            }

            if (model == null)
            {
                strError = "patron为null,返回值为" + nRet + "，error为" + strError;
                goto ERROR1;
            }

            return View(model);

        ERROR1:

            if (strError == "")
            {
                strError = "error怎么没赋值呢？ret=" + nRet;
            }
            ViewBag.Error = strError;
            return View();
        }

        //违约交费信息
        public ActionResult OverdueInfo(string code, string state)
        {
            string strError = "";
            string strXml = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(code, state, "xml", out activeUserItem, out strXml, out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            if (nRet == -2)// 未绑定当前图书馆的读者，转到绑定界面
            {
                return RedirectToAction("Bind", "Account");
            }

            string strWarningText = "";
            List<OverdueInfo> overdueList = dp2WeiXinService.Instance.GetOverdueInfo(strXml, out strWarningText);

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
            string strXml = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(code, state, "", out activeUserItem, out strXml, out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;
            if (nRet == -2)// 未绑定当前图书馆的读者，转到绑定界面
            {
                return RedirectToAction("Bind", "Account");
            }

            return View(activeUserItem);

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
            string strXml = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(code, state, "advancexml", out activeUserItem, out strXml, out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            if (nRet == -2)// 未绑定当前图书馆的读者，转到绑定界面
            {
                return RedirectToAction("Bind", "Account");
            }
            ViewBag.readerBarcode = activeUserItem.readerBarcode;

            string strWarningText = "";
            string maxBorrowCountString="";
            string curBorrowCountString="";
            List<BorrowInfo2> overdueList = dp2WeiXinService.Instance.GetBorrowInfo(strXml, out strWarningText,
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
        private int GetReaderXml(string code, string state,
            string strFormat,
            out WxUserItem activeUserItem,
            out string strXml,
            out string strError)
        {
            strXml = "";
            activeUserItem = null;
            strError = "";

            // 检查是否从微信入口进来
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return -1;

            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            activeUserItem = WxUserDatabase.Current.GetActivePatron(weixinId, ViewBag.LibId);
            // 未绑定读者账户,不会出现未激活的情况
            if (activeUserItem == null)
                return -2;


            // 有的调用处不需要获取读者xml，例如预约
            if (String.IsNullOrEmpty(strFormat) == false)
            {
                // 获取当前账户的信息
                string recPath = "";
                nRet = dp2WeiXinService.Instance.GetPatronXml(activeUserItem.libId,
                    activeUserItem.readerBarcode,
                    strFormat,
                    out recPath,
                    out strXml,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    return nRet;

                activeUserItem.recPath = recPath;
            }
            return 1;
        }


        #endregion
    }
}