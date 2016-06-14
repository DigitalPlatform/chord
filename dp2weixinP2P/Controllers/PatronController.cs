using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using dp2Command.Service;
using dp2weixin.service;
using dp2weixinWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace dp2weixinWeb.Controllers
{
    public class PatronController : BaseController
    {

        #region del
        // GET: Patron
        public ActionResult Index(string code, string state)
        {
            string strError = "";
            string strXml = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(code, state, "", out activeUserItem, out strXml);
            if (nRet == -1 || nRet == 0)
                return Content(strError);

            if (nRet == -2)// 未绑定的情况，转到绑定界面
            {
                return RedirectToAction("Bind", "Account");
            }
            // 没有设置默认账户，转到帐户管理界面
            if (nRet == -3)
            {
                return RedirectToAction("Index", "Account");
            }
            
            // 获取当前账户的信息
            PatronInfo patronInfo =null;
            nRet = dp2WeiXinService.Instance.GetPatron(activeUserItem.libUserName,
                activeUserItem.readerBarcode,
                out patronInfo,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                return Content(strError);
            }
            ViewBag.LibUserName = activeUserItem.libUserName;
            return View(patronInfo);
        }

        #endregion

        public ActionResult PersonalInfo(string code, string state)
        {
            string strError = "";
            string strXml = "";
            WxUserItem activeUserItem = null;
            int nRet = this.GetReaderXml(code, state, "advancexml", out activeUserItem, out strXml);
            if (nRet == -1 || nRet == 0)
                return Content(strError);

            if (nRet == -2)// 未绑定的情况，转到绑定界面
            {
                return RedirectToAction("Bind", "Account");
            }
            // 没有设置默认账户，转到帐户管理界面
            if (nRet == -3)
            {
                return RedirectToAction("Index", "Account");
            }

            PersonalInfoModel model = this.ParseXml(strXml);
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

            if (nRet == -2)// 未绑定的情况，转到绑定界面
            {
                return RedirectToAction("Bind", "Account");
            }
            // 没有设置默认账户，转到帐户管理界面
            if (nRet == -3)
            {
                return RedirectToAction("Index", "Account");
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

            if (nRet == -2)// 未绑定的情况，转到绑定界面
            {
                return RedirectToAction("Bind", "Account");
            }
            // 没有设置默认账户，转到帐户管理界面
            if (nRet == -3)
            {
                return RedirectToAction("Index", "Account");
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

            if (nRet == -2)// 未绑定的情况，转到绑定界面
            {
                return RedirectToAction("Bind", "Account");
            }
            // 没有设置默认账户，转到帐户管理界面
            if (nRet == -3)
            {
                return RedirectToAction("Index", "Account");
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
        public int GetReaderXml(string code, string state,string strFormat,out WxUserItem activeUserItem,out string strXml)
        {
            strXml = "";
            activeUserItem = null;

            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return -1;

            string weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];

            // 检查微信用户是否已经绑定的读者
            List<WxUserItem> userList = WxUserDatabase.Current.GetByWeixinId(weiXinId);
            if (userList.Count == 0)// 未绑定的情况，转到绑定界面
                return -2;


            // 检查是否设置了默认账户
            activeUserItem = null;
            foreach (WxUserItem item in userList)
            {
                if (item.isActive == 1)
                {
                    activeUserItem = item;
                    break;
                }
            }
            // 没有设置默认账户，转到帐户管理界面
            if (activeUserItem == null)
                return -2;

            // 有的调用处不需要获取读者xml，例如预约
            if (String.IsNullOrEmpty(strFormat) == false)
            {
                // 获取当前账户的信息
                nRet = dp2WeiXinService.Instance.GetPatronXml(activeUserItem.libUserName,
                    activeUserItem.readerBarcode,
                    strFormat,
                    out strXml,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    return nRet;
            }
            return 1;
        }



        private PersonalInfoModel ParseXml(string strXml)
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
            string opacUrl =dp2WeiXinService.Instance.opacUrl;
            string qrcodeUrl = "";
            if (String.IsNullOrEmpty(opacUrl) == false)
                qrcodeUrl = opacUrl + "/getphoto.aspx?action=pqri&barcode=" + HttpUtility.UrlEncode(strBarcode);
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
            string imageUrl = "";
            if (String.IsNullOrEmpty(opacUrl) == false)
                imageUrl=opacUrl+ "/getphoto.aspx?barcode=" + strBarcode;
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
            else
            {
                text = num.ToString();
            }
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