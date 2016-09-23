using ilovelibrary.Models;
using ilovelibrary.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ilovelibrary.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            string  isReader = model.IsReader;
            bool bReader = false;
            if (isReader == "1")
                bReader = true;

            //FormCollection c=this.for
            string userName = model.UserName;

            if (bReader == true)
            {
                string prefix = Request.Form["selPrefix"];
                if (String.IsNullOrEmpty(prefix) == false)
                {
                    userName = prefix + ":" + userName;
                }
            }


            string strError = "";
            string strRight = "";
            //登录dp2library服务器
            SessionInfo sessionInfo = ilovelibraryServer.Instance.Login(userName,
                model.Password,
                bReader,
                out strRight,
                out strError);
            if (sessionInfo != null)
            {
                // 存到Session中
                Session[SessionInfo.C_Session_sessioninfo] = sessionInfo;
                if (String.IsNullOrEmpty(returnUrl) == true)
                    returnUrl = "~/Charging/Main";

                // 是读者身份登录，且书斋名称不等空和*，转到选择馆藏地界面
                if (sessionInfo.isReader == true
                    && String.IsNullOrEmpty(sessionInfo.PersonalLibrary) == false
                    && sessionInfo.PersonalLibrary != "*")
                {
                    return this.RedirectToAction("Login2", "Account", new { ReturnUrl = returnUrl });
                }
                else
                {
                    return Redirect(returnUrl);
                }                
            }


            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            ModelState.AddModelError("", strError);//"提供的用户名或密码不正确。");

            // 继续跟上登录成功返回的url
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }


        public ActionResult Logout()
        {
            // 将session置空
            Session[SessionInfo.C_Session_sessioninfo] = null;

            return RedirectToAction("Main", "Charging");
        }

        public ActionResult Login2(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Login2(FormCollection c, string returnUrl)
        {
            // 设定选定的馆藏
            string temp = c["perLib"];
            if (String.IsNullOrEmpty(temp) == false)
            {
                SessionInfo sinfo = (SessionInfo)Session[SessionInfo.C_Session_sessioninfo];
                sinfo.SelPerLib = temp;
            }           

            return Redirect(returnUrl);
        }
    }
}