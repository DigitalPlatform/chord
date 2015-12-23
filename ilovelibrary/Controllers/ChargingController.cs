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
        //
        // GET: /Charging/
        public ActionResult Main()
        {            
            // 如果未登录，先去登录界面
            if (Session[SessionInfo.C_Session_sessioninfo] == null)
            {
                return this.RedirectToAction("Login", "Account", new { ReturnUrl = "~/Charging/Main"});
            }

            //为了书写简单，开关参数值用0和1表示
            //string showbtn = Request["showbtn"];
            //string showlbl = Request["showlbl"];

            return View();
        }
    }
}
