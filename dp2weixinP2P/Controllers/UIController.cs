using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dp2weixinWeb.Controllers
{
    public class UIController : Controller
    {
        public ActionResult MsgEdit()
        {
            return View();
        }

        // GET: UI
        public ActionResult BiblioIndex()
        {
            return View();
        }

        public ActionResult PersonalInfo()
        {
            return View();
        }

        public ActionResult Message()
        {
            return View();
        }
    }
}