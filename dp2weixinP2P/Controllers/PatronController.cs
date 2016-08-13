using DigitalPlatform.IO;
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
        public ActionResult Setting(string code, string state,string returnUrl)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            string weixinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            ViewBag.returnUrl = returnUrl;

            // 图书馆html
            ViewBag.LibHtml = this.GetLibSelectHtml(ViewBag.LibId, weixinId);

            string photoChecked = "";
            if (ViewBag.showPhoto == 1)
                photoChecked = " checked='checked' ";
            ViewBag.photoChecked = photoChecked;

            string coverChecked = "";
            if (ViewBag.showCover == 1)
                coverChecked = " checked='checked' ";
            ViewBag.coverChecked = coverChecked;

            return View();
        }


        #region 二维码 图片

        // 二维码
        public ActionResult QRcode(string code, string state)
        {
            string strError = "";
            string strXml = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(code, state, "", out activeUserItem, out strXml);
            if (nRet == -1 || nRet == 0)
                return Content(strError);
            if (nRet==-2)
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
        }

        // 图片
        public ActionResult GetPhoto(string libId, string type, string barcode,string objectPath)
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
                if (nRet == -1 || nRet==0)
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
                string mimetype = DomUtil.GetAttr(dom.DocumentElement, "mimetype");
                Response.ContentType = mimetype;

                //ms = dp2WeiXinService.Instance.GetErrorImg(mimetype);
                //return File(ms.ToArray(), "image/jpeg");  

                Response.OutputStream.Flush();

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



        #endregion

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

        public ActionResult PersonalInfo(string code, string state)
        {
            string strError = "";
            string strXml = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(code, state, "advancexml", out activeUserItem, out strXml);
            if (nRet == -1 || nRet == 0)
                return Content(strError);
            if (nRet==-2)
            {
                ViewBag.RedirectInfo = this.getLinkHtml("我的信息", "/Patron/PersonalInfo");
                return View();
            }

            PersonalInfoModel model = null;
            if (activeUserItem != null)
                model= this.ParseXml(activeUserItem.libId, strXml,activeUserItem.recPath);

            return View(model);
        }

        //违约交费信息
        public ActionResult OverdueInfo(string code, string state)
        {
            string strError = "";
            string strXml = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(code, state, "xml", out activeUserItem, out strXml);
            if (nRet == -1 || nRet == 0)
                return Content(strError);
            if (nRet == -2)// 未绑定当前图书馆的读者，转到绑定界面
            {
                return RedirectToAction("Bind", "Account");
            }

            string strWarningText = "";
            List<OverdueInfo> overdueList= dp2WeiXinService.Instance.GetOverdueInfo(strXml, out strWarningText);

            return View(overdueList);
        }

        /// <summary>
        /// 预约入口
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult Reservation(string code, string state)
        {
            string strError = "";
            string strXml = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(code, state, "",out activeUserItem, out strXml);
            if (nRet == -1 || nRet == 0)
                return Content(strError);
            if (nRet == -2)// 未绑定当前图书馆的读者，转到绑定界面
            {
                return RedirectToAction("Bind", "Account");
            }

            return View(activeUserItem);
        }



        //BorrowInfo
        public ActionResult BorrowInfo(string code, string state)
        {
            string strError = "";
            string strXml = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(code, state, "", out activeUserItem, out strXml);
            if (nRet == -1 || nRet == 0)
                return Content(strError);

            if (nRet == -2)// 未绑定当前图书馆的读者，转到绑定界面
            {
                return RedirectToAction("Bind", "Account");
            }

            return View(activeUserItem);
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
        public int GetReaderXml(string code, string state,
            string strFormat,
            out WxUserItem activeUserItem,
            out string strXml)
        {
            strXml = "";
            activeUserItem = null;

            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return -1;

            string weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            activeUserItem = WxUserDatabase.Current.GetActivePatron(weiXinId, ViewBag.LibId);
            // 未绑定读者账户，应该不会出现未激活的情况，todo，当设置图书馆，如果发现微信用户绑定了该图书馆的读者账户，则自动找到第一个激活
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



        private PersonalInfoModel ParseXml(string libId,string strXml,string recPath)
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
                XmlNodeList fileNodes = dom.DocumentElement.SelectNodes("//dprms:file[@usage='photo']", nsmgr);
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

        public string ConvertToString(int num)
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
                text = "("+text+")";
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


    }
}