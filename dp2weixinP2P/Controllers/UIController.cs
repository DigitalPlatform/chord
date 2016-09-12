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

            //string resUri = HttpUtility.UrlEncode("http://localhost/dp2weixin/img/guide.pdf");
            //string resLink = "<a href='../Patron/getobject?strURI=" + resUri + "'>test res</a>";

            string resUri = "中文图书/3" + "/object/1";
            string libId = "57b91e7083cbdc2394ea17dc";

            string resLink = "<a href='../Patron/getobject?libId=" + HttpUtility.UrlEncode(libId)
            + "&mime=" + HttpUtility.UrlEncode("application/pdf")
            + "&uri=" + HttpUtility.UrlEncode(resUri)+"'>"
            + "test res</a>  ";


            ViewBag.ResLink = resLink;
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