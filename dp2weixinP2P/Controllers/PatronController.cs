using dp2Command.Service;
using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dp2weixinWeb.Controllers
{
    public class PatronController : BaseController
    {
        // GET: Patron
        public ActionResult Index(string code, string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            string test = "test";


           string weiXinId =(string)Session[WeiXinConst.C_Session_WeiXinId];


            PatronInfo patronInfo = null;
            // 检查微信id是否已经绑定的读者，是否设置了默认值
            List<WxUserItem> userList = WxUserDatabase.Current.GetByWeixinId(weiXinId);
            if (userList.Count == 0)
            {
                ViewBag.bindFlag = 0; //未绑定
                return RedirectToAction("Bind", "Account");
            }
            else
            {
                WxUserItem activeUserItem = null;
                foreach (WxUserItem item in userList)
                {
                    if (item.isActive == 1)
                    {
                        activeUserItem = item;
                        break;
                    }
                }

                if (activeUserItem == null)
                {
                    ViewBag.bindFlag = 1;//未设默认值
                    return RedirectToAction("Index", "Account");
                }
                else
                {
                    ViewBag.bindFlag = 2;//有默认值
                    Session[WeiXinConst.C_Session_CurPatronInfo] = activeUserItem;

                    
                    string xml = "";
                    nRet = dp2WeiXinService.Instance.GetPatronInfo(activeUserItem.libUserName,
                        activeUserItem.readerBarcode,
                        "advancexml,advancexml_borrow_bibliosummary",
                        out xml,
                        out strError);
                    if (nRet == -1)
                    {
                        return Content(strError);
                    }
                    if (nRet == 0)
                    {
                        return Content( "从dp2library未找到证条码号为'" + activeUserItem.readerBarcode + "'的记录"); //todo refID
                    }

                    patronInfo = WeiXinService.ParseReaderXml(xml);
                   

                }
            }

            return View(patronInfo);
        }
    }
}