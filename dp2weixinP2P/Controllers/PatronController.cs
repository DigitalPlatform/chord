using dp2Command.Service;
using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dp2weixinP2P.Controllers
{
    public class PatronController : Controller
    {
        // GET: Patron
        public ActionResult Index(string code, string state, string weiXinId)
        {
            // 从微信进入的
            if (string.IsNullOrEmpty(code) == false)
            {
                //可以传一个state用于校验
                if (state != "dp2weixin")
                {
                    return Content("验证失败！请从正规途径进入！");
                }

                string strError = "";
                int nRet = dp2CmdService2.Instance.GetWeiXinId(code, out weiXinId, out strError);
                if (nRet == -1)
                    return Content(strError);
            }

            if (String.IsNullOrEmpty(weiXinId) == false)
            {
                // 记下微信id
                Session[WeiXinConst.C_Session_WeiXinId] = weiXinId;
            }

            if (Session[WeiXinConst.C_Session_WeiXinId] == null
                || (String)Session[WeiXinConst.C_Session_WeiXinId] == "")
            {
                return Content("非正规途径，未传入微信id。");
            }
            else
            {
                weiXinId =(string)Session[WeiXinConst.C_Session_WeiXinId];
            }

            PatronInfo patronInfo = null;
            // 检查微信id是否已经绑定的读者，是否设置了默认值
            List<WxUserItem> userList = WxUserDatabase.Current.GetByWeixinId(weiXinId);
            if (userList.Count == 0)
            {
                ViewBag.bindFlag = 0; //未绑定
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
                }
                else
                {
                    ViewBag.bindFlag = 2;//有默认值
                    Session[WeiXinConst.C_Session_CurPatronInfo] = activeUserItem;

                    
                    string xml = "";
                    string strError = "";
                    int nRet = dp2CmdService2.Instance.GetPatronInfo(activeUserItem.libUserName,
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

                    patronInfo = WeixinService.ParseReaderXml(xml);
                   

                }
            }

            return View(patronInfo);
        }
    }
}