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
            WxUserItem userItem1= WxUserDatabase.Current.GetActivePatron(weiXinId);
            string libUserName = "";
            if (userItem1 != null)
            {
                ViewBag.LibCode = userItem1.libCode + "*" + userItem1.libUserName;// "lib_local*mycapo";
                ViewBag.PatronBarcode = userItem1.readerBarcode;
                ViewBag.PatronName = userItem1.readerName;
                ViewBag.LibUserName = userItem1.libUserName;
                libUserName = userItem1.libUserName;
            }
            else
            {
                userItem1 = WxUserDatabase.Current.GetOneWorker(weiXinId);
                if (userItem1 != null)
                {
                    libUserName = userItem1.libUserName;
                }
 
            }
            //ViewBag.IsFirst = "1";
            //ViewBag.ResultSetName = "weixin-" + Guid.NewGuid().ToString();

            List<LibItem> list = LibDatabase.Current.GetLibs();//"*", 0, -1).Result;
            var opt = "<option value=''>请选择 图书馆</option>";
            for (var i = 0; i < list.Count; i++)
            {var item = list[i];
                string selectedString = "";
                if (libUserName != "" && libUserName == item.libUserName)
                {
                    selectedString = " selected='selected' ";
                }                
                opt += "<option value='" + item.libCode + '*' + item.libUserName + "' "+selectedString+">" + item.libName + "</option>";
            }
            ViewBag.LibHtml = "<select id='selLib' style='padding-left: 0px;width: 65%' data-bind=\"optionsCaption:'请选择 图书馆',value:selectedLib\">" + opt + "</select>";

            return View();
        }
    }
}