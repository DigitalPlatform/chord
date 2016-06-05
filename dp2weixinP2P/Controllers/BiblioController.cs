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
        // GET: Biblio
        public ActionResult Index(string code,string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            string weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            WxUserItem userItem= WxUserDatabase.Current.GetActive(weiXinId);
            if (userItem != null)
            {
                ViewBag.LibCode = userItem.libCode+"*"+userItem.libUserName;// "lib_local*mycapo";
            }

            return View();
        }
    }
}