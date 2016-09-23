using ilovelibrary.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace ilovelibrary.Controllers
{
    public class ChargingController : Controller
    {
        public ActionResult Main()
        {            
            // 如果未登录，先去登录界面
            if (Session[SessionInfo.C_Session_sessioninfo] == null)
            {
                return this.RedirectToAction("Login", "Account", new { ReturnUrl = "~/Charging/Main"});
            }

            return View();
        }
    }
}
