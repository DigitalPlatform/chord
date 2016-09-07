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
                goto ERROR1;

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
                ViewBag.RedirectInfo = this.getLinkHtml("二维码", "/Patron/QRcode");
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

            if (type == "pqri")
            {
                // 读者证号二维码
                string strCode = "";
                // 获得读者证号二维码字符串
                int nRet = dp2WeiXinService.Instance.GetQRcode(libId,
                    barcode,
                    out strCode,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;    // 把出错信息作为图像返回

                Response.ContentType = "image/jpeg";

                // 获得二维码图片
                dp2WeiXinService.Instance.GetQrImage(strCode,
                   nWidth,
                   nHeight,
                   Response.OutputStream,
                   out strError);
                if (strError != "")
                    goto ERROR1;
                return null;
                // return File(Response.OutputStream, "image/jpeg");
                //return File(ms.ToArray(), "image/jpeg");  
            }

            // 取头像
            if (type == "photo")
            {
                // 先取出metadata
                string metadata = "";
                string timestamp = "";
                string outputpath = "";
                int nRet = dp2WeiXinService.Instance.GetObjectMetadata(libId,
                    objectPath,
                    "metadata",
                    null,
                    out metadata,
                    out timestamp,
                    out outputpath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;    // 把出错信息作为图像返回


                // 找出mimetype
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(metadata);
                }
                catch (Exception ex)
                {
                    strError = ex.Message;
                    goto ERROR1;
                }

                //Response.OutputStream.Flush();

                string mimetype = DomUtil.GetAttr(dom.DocumentElement, "mimetype");
                Response.ContentType = mimetype;
                Response.Clear();

                //ms = dp2WeiXinService.Instance.GetErrorImg(mimetype);
                //return File(ms.ToArray(), "image/jpeg");  



                // 输出数据流
                nRet = dp2WeiXinService.Instance.GetObjectMetadata(libId,
                    objectPath,
                    "metadata,timestamp,data,outputpath",
                    //"metadata,data",
                    Response.OutputStream, //ms,//
                    out metadata,
                    out timestamp,
                    out outputpath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;    // 把出错信息作为图像返回

                Response.OutputStream.Flush();

                return null;

                //ms.Seek(0, SeekOrigin.Begin);
                //return File(ms, mimetype);

            }

            ms = dp2WeiXinService.Instance.GetErrorImg("不支持");
            return File(ms.ToArray(), "image/jpeg");

        ERROR1:

            ms = dp2WeiXinService.Instance.GetErrorImg(strError);
            return File(ms.ToArray(), "image/jpeg");
        }


        static Uri GetUri(string strURI)
        {
            try
            {
                return new Uri(strURI);
            }
            catch
            {
                return null;
            }
        }

        public static bool MatchMIME(string strMime, string strLeftParam)
        {
            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(strMime, "/", out strLeft, out strRight);
            if (string.Compare(strLeft, strLeftParam, true) == 0)
                return true;
            return false;
        }

        // 图片
        public ActionResult GetObject(string uri,string libId)
        {
            MemoryStream ms = new MemoryStream(); ;
            string strError = "";
            int nRet = 0;

            Uri tempUri = GetUri(uri);

            //处理 dp2 系统外部的 URL
            if (tempUri != null
                && (tempUri.Scheme == "http" || tempUri.Scheme == "https"))
            {
                return Redirect(uri);
            }

            // 先取出metadata
            string metadata = "";
            string timestamp = "";
            string outputpath = "";
            nRet = dp2WeiXinService.Instance.GetObjectMetadata(libId,
                uri,
                "metadata",
                null,
                out metadata,
                out timestamp,
                out outputpath,
                out strError);
            if (nRet == -1)
                goto ERROR1;    // 把出错信息作为图像返回

            // <file mimetype="application/pdf" localpath="D:\工作清单.pdf" size="188437"
            // lastmodified="2016/9/6 12:45:14" readCount="17" />
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(metadata);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

            // 检查是否有更新，没更新直接用浏览器缓存数据
            string strLastModifyTime = DomUtil.GetAttr(dom.DocumentElement, "lastmodified");//lastmodifytime");
            if (String.IsNullOrEmpty(strLastModifyTime) == false)
            {
                DateTime lastmodified = DateTime.Parse(strLastModifyTime).ToUniversalTime();
                string strIfHeader = Request.Headers["If-Modified-Since"];

                if (String.IsNullOrEmpty(strIfHeader) == false)
                {
                    DateTime isModifiedSince = DateTimeUtil.FromRfc1123DateTimeString(strIfHeader); // .ToLocalTime();

                    if (DateTimeUtil.CompareHeaderTime(isModifiedSince, lastmodified) != 0)
                    {
                        // 修改过
                    }
                    else
                    {
                        // 没有修改过
                        Response.StatusCode = 304;
                        Response.SuppressContent = true;
                        return null;
                    }
                }

                Response.AddHeader("Last-Modified", DateTimeUtil.Rfc1123DateTimeString(lastmodified)); // .ToUniversalTime()
            }

            string strMime = DomUtil.GetAttr(dom.DocumentElement, "mimetype");
            bool bSaveAs = false;
            if (string.IsNullOrEmpty(strMime) == true
                || MatchMIME(strMime, "text") == true
                || MatchMIME(strMime, "image") == true)
            {

            }
            else
            {
                bSaveAs = true;
            }

            // 设置媒体类型
            if (strMime == "text/plain")
                strMime = "text";
            Response.ContentType = strMime;

            // 是否出现另存为对话框
            if (bSaveAs == true)
            {
                string strClientPath = DomUtil.GetAttr(dom.DocumentElement, "localpath");
                if (strClientPath != "")
                    strClientPath = PureName(strClientPath);

                string strEncodedFileName = HttpUtility.UrlEncode(strClientPath, Encoding.UTF8);
                Response.AddHeader("content-disposition", "attachment; filename=" + strEncodedFileName);
            }

            //设置尺寸
            string strSize = DomUtil.GetAttr(dom.DocumentElement, "size");
            if (String.IsNullOrEmpty(strSize) == false)
            {
                Response.AddHeader("Content-Length", strSize);
            }



            // 关闭buffer
            Response.BufferOutput = false; // 2016/8/31


            // 输出数据流
            nRet = dp2WeiXinService.Instance.GetObjectMetadata(libId,
                uri,
                "metadata,timestamp,data,outputpath",
                Response.OutputStream, //ms,//
                out metadata,
                out timestamp,
                out outputpath,
                out strError);
            if (nRet == -1)
                goto ERROR1;    // 把出错信息作为图像返回

            

            Response.OutputStream.Flush();
            Response.End();  
            return null;


        ERROR1:

            ms = dp2WeiXinService.Instance.GetErrorImg(strError);
            return File(ms.ToArray(), "image/jpeg");
        }

        public static string PureName(string strPath)
        {
            // 2012/11/30
            if (string.IsNullOrEmpty(strPath) == true)
                return strPath;

            string sResult = "";
            sResult = strPath;
            sResult = sResult.Replace("/", "\\");
            if (sResult.Length > 0)
            {
                if (sResult[sResult.Length - 1] == '\\')
                    sResult = sResult.Substring(0, sResult.Length - 1);
            }
            int nRet = sResult.LastIndexOf("\\");
            if (nRet != -1)
                sResult = sResult.Substring(nRet + 1);

            return sResult;
        }
        /*

        public ActionResult GetObject(string code, string state, string weixinId,
            string strURI,
            string style,
            string biblioRecPath,
            string saveas)
        {
            // 登录账户?



            string strError = "";
            int nRet = 0;

            //string strURI = uri;//Request.QueryString["uri"];
            string strStyle = style;// Request.QueryString["style"];
            string strBiblioRecPath = biblioRecPath;// Request.QueryString["biblioRecPath"];

            Uri uri = GetUri(strURI);

            //处理 dp2 系统外部的 URL
            if (uri != null
                && (uri.Scheme == "http" || uri.Scheme == "https"))
            {
                return Redirect(strURI);
            }


            // *** 以下是处理 dp2 系统内部对象
            // TODO: dp2 系统内部对象总是有访问计数功能的，

            string strSaveAs = saveas;
            bool bSaveAs = false;
            if (strSaveAs == "true")
                bSaveAs = true;

            // FlushOutput flushdelegate = new FlushOutput(MyFlushOutput);

            // this.Response.BufferOutput = false;
            this.Server.ScriptTimeout = 10 * 60 * 60;    // 10 个小时

            nRet = app.DownloadObject(
                this,
                // flushdelegate,
                // sessioninfo.Channels,
                channel,
                strURI,
                bSaveAs,
                strStyle,
                out strError);
            if (nRet == -1)
            {
                // Response.Write(strError);
               MemoryStream  ms = dp2WeiXinService.Instance.GetErrorImg(strError);
                return File(ms.ToArray(), "image/jpeg");    
            }

            Response.End();
            return null;

        }

        // 下载对象资源
        // parameters:
        //      strStyle    如果包含 hitcount，表示希望获取访问计数的数字，返回图像格式。否则是希望返回对象本身
        // return:
        //      -1  出错
        //      0   304返回
        //      1   200返回
        public int DownloadObject0(System.Web.UI.Page Page,
            string strPath,
            bool bSaveAs,
            string strStyle,
            out string strError)
        {
            strError = "";


            // 先取出metadata
            string metadata = "";
            string timestamp = "";
            string outputpath = "";
            int nRet = dp2WeiXinService.Instance.GetObjectMetadata(libId,
                objectPath,
                "metadata",
                null,
                out metadata,
                out timestamp,
                out outputpath,
                out strError);
            if (nRet == -1)
                goto ERROR1;    // 把出错信息作为图像返回


            // 找出mimetype
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(metadata);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

            //Response.OutputStream.Flush();

            string mimetype = DomUtil.GetAttr(dom.DocumentElement, "mimetype");
            Response.ContentType = mimetype;
            Response.Clear();



            WebPageStop stop = new WebPageStop(Page);

            // strPath = boards.GetCanonicalUri(strPath);

            // 获得资源。写入文件的版本。特别适用于获得资源，也可用于获得主记录体。
            // parameters:
            //		fileTarget	文件。注意在调用函数前适当设置文件指针位置。函数只会在当前位置开始向后写，写入前不会主动改变文件指针。
            //		strStyleParam	一般设置为"content,data,metadata,timestamp,outputpath";
            //		input_timestamp	若!=null，则本函数会把第一个返回的timestamp和本参数内容比较，如果不相等，则报错
            // return:
            //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
            //		0	成功
            string strMetaData = "";
            string strOutputPath;
            byte[] baOutputTimeStamp = null;
            byte[] baContent = null;
            // 只获得媒体类型
            long lRet = channel.GetRes(
                stop,
                strPath,
                0,
                0,
                "metadata",
                out baContent,
                out strMetaData,
                out strOutputPath,
                out baOutputTimeStamp,
                out strError);
            if (lRet == -1)
            {
                if (StringUtil.IsInList("hitcount", strStyle))
                {
                    OutputImage(Page,
                        Color.FromArgb(100, Color.Red),
                        "?");
                    return 1;
                }

                if (channel.ErrorCode == ErrorCode.AccessDenied)
                {
                    // 权限不够
                    return -1;
                }

                strError = "GetRes() (for metadata) Error : " + strError;
                return -1;
            }

            if (Page.Response.IsClientConnected == false)
                return -1;

            // 取 metadata 中的 mime 类型信息
            Hashtable values = StringUtil.ParseMedaDataXml(strMetaData,
                out strError);
            if (values == null)
            {
                strError = "ParseMedaDataXml() Error :" + strError;
                return -1;
            }

            if (StringUtil.IsInList("hitcount", strStyle))
            {
                string strReadCount = (string)values["readCount"];
                if (string.IsNullOrEmpty(strReadCount) == true)
                    strReadCount = "?";
                OutputImage(Page,
                    Color.FromArgb(200, Color.DarkGreen),
                    strReadCount);
                return 1;
            }


            string strLastModifyTime = (string)values["lastmodifytime"];
            if (String.IsNullOrEmpty(strLastModifyTime) == false)
            {
                DateTime lastmodified = DateTime.Parse(strLastModifyTime).ToUniversalTime();
                string strIfHeader = Page.Request.Headers["If-Modified-Since"];

                if (String.IsNullOrEmpty(strIfHeader) == false)
                {
                    DateTime isModifiedSince = DateTimeUtil.FromRfc1123DateTimeString(strIfHeader); // .ToLocalTime();

                    if (DateTimeUtil.CompareHeaderTime(isModifiedSince, lastmodified) != 0)
                    {
                        // 修改过
                    }
                    else
                    {
                        // 没有修改过
                        Page.Response.StatusCode = 304;
                        Page.Response.SuppressContent = true;
                        return 0;
                    }
                }

                Page.Response.AddHeader("Last-Modified", DateTimeUtil.Rfc1123DateTimeString(lastmodified)); // .ToUniversalTime()

            }

            string strMime = (string)values["mimetype"];
            string strClientPath = (string)values["localpath"];
            if (strClientPath != "")
                strClientPath = PathUtil.PureName(strClientPath);

            // TODO: 如果是非image/????类型，都要加入content-disposition
            // 是否出现另存为对话框
            if (bSaveAs == true)
            {
                string strEncodedFileName = HttpUtility.UrlEncode(strClientPath, Encoding.UTF8);
                Page.Response.AddHeader("content-disposition", "attachment; filename=" + strEncodedFileName);
            }



            // 用 text/plain IE XML 搜索google
            // http://support.microsoft.com/kb/329661
            // http://support.microsoft.com/kb/239750/EN-US/



            // 设置媒体类型
            if (strMime == "text/plain")
                strMime = "text";
            Page.Response.ContentType = strMime;

            string strSize = (string)values["size"];
            if (String.IsNullOrEmpty(strSize) == false)
            {
                Page.Response.AddHeader("Content-Length", strSize);
            }

            if (Page.Response.IsClientConnected == false)
                return -1;

            string strGetStyle = "content,data,incReadCount";
            if (StringUtil.IsInList("log", this.SearchLogEnable) == false)
                strGetStyle += ",skipLog";
            else
                strGetStyle += ",clientAddress:" + Page.Request.UserHostAddress;

            // 传输数据
            lRet = channel.GetRes(
                stop,
                strPath,
                Page.Response.OutputStream,
                strGetStyle,
                null,	// byte [] input_timestamp,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                // Page.Response.ContentType = "text/plain";    // 可能因为 Page.Response.OutputStream 已经写入了部分内容，这时候设置 ContentType 会抛出异常
                strError = "GetRes() (for res) Error : " + strError;
                return -1;
            }
            return 1;
        }
        */


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
            dp2WeiXinService.Instance.WriteErrorLog("test0");
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            dp2WeiXinService.Instance.WriteErrorLog("test1");

            if (nRet == -2)
            {
                dp2WeiXinService.Instance.WriteErrorLog("test2");
                ViewBag.RedirectInfo = this.getLinkHtml("我的信息", "/Patron/PersonalInfo");
                return View();
            }

            dp2WeiXinService.Instance.WriteErrorLog("test3");
            PersonalInfoModel model = null;
            if (activeUserItem != null)
            {
                model = this.ParseXml(activeUserItem.libId, strXml, activeUserItem.recPath);
                dp2WeiXinService.Instance.WriteErrorLog("test4");
            }

            if (model == null)
            {
                dp2WeiXinService.Instance.WriteErrorLog("test5");
                strError = "model为null,返回值为" + nRet + "，error为" + strError;
                goto ERROR1;
            }

            dp2WeiXinService.Instance.WriteErrorLog("test6");
            return View(model);

        ERROR1:

            dp2WeiXinService.Instance.WriteErrorLog("test7");
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

        #region 内部函数

        private string getLinkHtml(string menu, string returnUrl)
        {
            //string returnUrl = "/Patron/PersonalInfo";
            string bindUrl = "/Account/Bind?returnUrl=" + HttpUtility.UrlEncode(returnUrl);
            string bindLink = "请先点击<a href='javascript:void(0)' onclick='gotoUrl(\"" + bindUrl + "\")'>这里</a>进行绑定。";
            string strRedirectInfo = "您尚未绑定当前图书馆的读者账户，不能查看" + menu + "，" + bindLink;

            strRedirectInfo = "<div class='mui-content-padded' style='color:#666666'>"
                //+ "<center>"
                + strRedirectInfo
                //+ "</center"
                + "</div>";


            return strRedirectInfo;
        }

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

        private PersonalInfoModel ParseXml(string libId, string strXml, string recPath)
        {
            PersonalInfoModel model = new PersonalInfoModel();
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);


            // 证条码号
            string strBarcode = DomUtil.GetElementText(dom.DocumentElement,
    "barcode");
            model.barcode = strBarcode;

            // 显示名
            string strDisplayName = DomUtil.GetElementText(dom.DocumentElement,
    "displayName");
            model.displayName = strDisplayName;

            // 二维码
            string qrcodeUrl = "./getphoto?libId=" + HttpUtility.UrlEncode(libId)
                + "&type=pqri"
                + "&barcode=" + HttpUtility.UrlEncode(strBarcode);
            model.qrcodeUrl = qrcodeUrl;

            // 姓名
            string strName = DomUtil.GetElementText(dom.DocumentElement,
                "name");
            model.name = strName;

            // 性别
            string strGender = DomUtil.GetElementText(dom.DocumentElement,
                "gender");
            model.gender = strGender;

            // 出生日期
            string strDateOfBirth = DomUtil.GetElementText(dom.DocumentElement,
                "dateOfBirth");
            if (string.IsNullOrEmpty(strDateOfBirth) == true)
                strDateOfBirth = DomUtil.GetElementText(dom.DocumentElement,
   "birthday");
            strDateOfBirth = DateTimeUtil.LocalDate(strDateOfBirth);
            model.dateOfBirth = strDateOfBirth;

            // 证号 2008/11/11
            string strCardNumber = DomUtil.GetElementText(dom.DocumentElement,
    "cardNumber");
            model.cardNumber = strCardNumber;

            // 身份证号
            string strIdCardNumber = DomUtil.GetElementText(dom.DocumentElement,
    "idCardNumber");
            model.idCardNumber = strIdCardNumber;

            // 单位
            string strDepartment = DomUtil.GetElementText(dom.DocumentElement,
"department");
            model.department = strDepartment;

            // 职务
            string strPost = DomUtil.GetElementText(dom.DocumentElement,
"post");
            model.post = strPost;

            // 地址
            string strAddress = DomUtil.GetElementText(dom.DocumentElement,
"address");
            model.address = strAddress;

            // 电话
            string strTel = DomUtil.GetElementText(dom.DocumentElement,
"tel");
            model.tel = strTel;

            // email
            string strEmail = DomUtil.GetElementText(dom.DocumentElement,
"email");
            model.email = this.RemoveWeiXinId(strEmail);//过滤掉微信id



            // 读者类型
            string strReaderType = DomUtil.GetElementText(dom.DocumentElement,
"readerType");
            model.readerType = strReaderType;

            // 证状态
            string strState = DomUtil.GetElementText(dom.DocumentElement,
"state");
            model.state = strState;

            // 发证日期
            string strCreateDate = DomUtil.GetElementText(dom.DocumentElement,
                "createDate");
            strCreateDate = DateTimeUtil.LocalDate(strCreateDate);
            model.createDate = strCreateDate;

            // 证失效期
            string strExpireDate = DomUtil.GetElementText(dom.DocumentElement,
                "expireDate");
            strExpireDate = DateTimeUtil.LocalDate(strExpireDate);
            model.expireDate = strExpireDate;

            // 租金 2008/11/11
            string strHireExpireDate = "";
            string strHirePeriod = "";
            XmlNode nodeHire = dom.DocumentElement.SelectSingleNode("hire");
            string strHire = "";
            if (nodeHire != null)
            {
                strHireExpireDate = DomUtil.GetAttr(nodeHire, "expireDate");
                strHirePeriod = DomUtil.GetAttr(nodeHire, "period");

                strHireExpireDate = DateTimeUtil.LocalDate(strHireExpireDate);
                strHirePeriod = dp2WeiXinService.GetDisplayTimePeriodStringEx(strHirePeriod);

                strHire = "周期" + ": " + strHirePeriod + "; "
                + "失效期" + ": " + strHireExpireDate;
            }
            model.hire = strHire;

            // 押金 2008/11/11
            string strForegift = DomUtil.GetElementText(dom.DocumentElement,
                "foregift");
            model.foregift = strForegift;

            //头像
            //recPath
            string imageUrl = "";
            if (ViewBag.showPhoto == 1)
            {
                //dprms:file
                // 看看是不是已经有图像对象
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("dprms", DpNs.dprms);
                // 全部<dprms:file>元素
                XmlNodeList fileNodes = dom.DocumentElement.SelectNodes("//dprms:file[@usage='cardphoto']", nsmgr);
                if (fileNodes.Count > 0)
                {
                    string strPhotoPath = recPath + "/object/" + DomUtil.GetAttr(fileNodes[0], "id");

                    dp2WeiXinService.Instance.WriteLog("photoPath:" + strPhotoPath);

                    imageUrl = "./getphoto?libId=" + HttpUtility.UrlEncode(libId)
                    + "&type=photo"
                    + "&objectPath=" + HttpUtility.UrlEncode(strPhotoPath);
                }
            }

            model.imageUrl = imageUrl;


            // 违约
            List<OverdueInfo> overdueLit = new List<OverdueInfo>();
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("overdues/overdue");
            model.OverdueCount = nodes.Count;
            model.OverdueCountHtml = ConvertToString(model.OverdueCount);

            // 在借
            nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
            model.BorrowCount = nodes.Count;
            model.BorrowCountHtml = ConvertToString(model.BorrowCount);
            int caoQiCount = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strIsOverdue = DomUtil.GetAttr(node, "isOverdue");
                if (strIsOverdue == "yes")
                {
                    caoQiCount++;
                }
            }
            model.CaoQiCount = caoQiCount;

            // 预约
            nodes = dom.DocumentElement.SelectNodes("reservations/request");
            model.ReservationCount = nodes.Count;
            model.ReservationCountHtml = ConvertToString(model.ReservationCount);
            int daoQiCount = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string state = DomUtil.GetAttr(node, "state");
                if (state == "arrived")
                {
                    daoQiCount++;
                }
            }
            model.DaoQiCount = daoQiCount;

            // 返回读者信息对象
            return model;
        }

        private string ConvertToString(int num)
        {
            string text = "";
            if (num > 0 && num <= 5)
            {
                text = "<span class='leftNum'>" + "▪".PadRight(num, '▪') + "</span>";
            }
            else if (num == 0)
            {
                text = "";
            }
            else
            {
                text = num.ToString();
            }

            if (text != "")
                text = "(" + text + ")";
            return text;
        }

        private string RemoveWeiXinId(string email)
        {
            //<email>test@163.com,123,weixinid:o4xvUviTxj2HbRqbQb9W2nMl4fGg,weixinid:o4xvUvnLTg6NnflbYdcS-sxJCGFo,weixinid:testid</email>
            string[] emailList = email.Split(new char[] { ',' });
            string clearEmail = "";
            for (int i = 0; i < emailList.Length; i++)
            {
                string oneEmail = emailList[i].Trim();
                if (oneEmail.Length > 9 && oneEmail.Substring(0, 9) == WeiXinConst.C_WeiXinIdPrefix)
                {
                    continue;
                }

                if (clearEmail != "")
                    clearEmail += ",";

                clearEmail += oneEmail;
            }

            return clearEmail;
        }

        #endregion
    }
}