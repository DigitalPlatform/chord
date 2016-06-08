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

            string weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];

            // 检查微信用户是否已经绑定的读者
            List<WxUserItem> userList = WxUserDatabase.Current.GetByWeixinId(weiXinId);
            if (userList.Count == 0)// 未绑定的情况，转到绑定界面
            {
                return RedirectToAction("Bind", "Account");
            }

            // 检查是否设置了默认账户
            WxUserItem activeUserItem = null;
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


        public ActionResult Reservation(string code, string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            string weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];

            // 检查微信用户是否已经绑定的读者
            List<WxUserItem> userList = WxUserDatabase.Current.GetByWeixinId(weiXinId);
            if (userList.Count == 0)// 未绑定的情况，转到绑定界面
            {
                return RedirectToAction("Bind", "Account");
            }

            // 检查是否设置了默认账户
            WxUserItem activeUserItem = null;
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
            {
                return RedirectToAction("Index", "Account");
            }

            return View(activeUserItem);
        }
    }
}