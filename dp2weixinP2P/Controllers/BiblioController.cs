using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dp2weixinP2P.Controllers
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

            return View();
        }
    }
}