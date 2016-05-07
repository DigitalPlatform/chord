using dp2Command.Service;
using dp2weixin.service;
using dp2weixinP2P.Models;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dp2weixinP2P.Controllers
{
    public class AccountController : Controller
    {
        // GET: Bind 绑定主界面‘
        public ActionResult Index(string weiXinId)
        {
            // todo 如果未传入微信id，该怎么处理，这里先报错吧
            if (String.IsNullOrEmpty(weiXinId) == true)
            {
                return Content("未传入weinxinId参数。");
            }

            // 记下微信id
            Session[WeiXinConst.C_Session_WeiXinId] = weiXinId;

            //先检查一下，微信用户是否已经绑定的读者，
            //如未绑定，到新增绑定界面
            //如果已经绑定，则显示绑定列表页面
            List<WxUserItem> userList = dp2CmdService2.Instance.GetBindInfo(weiXinId);
            if (userList == null || userList.Count == 0)
            {

            }

            return View(userList);
        }

        // GET: Bind 绑定主界面‘
        public ActionResult Index2(string weiXinId)
        {
            // todo 如果未传入微信id，该怎么处理，这里先报错吧
            if (String.IsNullOrEmpty(weiXinId) == true)
            {
                return Content("未传入weinxinId参数。");
            }

            // 记下微信id
            Session[WeiXinConst.C_Session_WeiXinId] = weiXinId;

            //先检查一下，微信用户是否已经绑定的读者，
            //如未绑定，到新增绑定界面
            //如果已经绑定，则显示绑定列表页面
            List<WxUserItem> userList = dp2CmdService2.Instance.GetBindInfo(weiXinId);
            if (userList == null || userList.Count == 0)
            {

            }

            return View(userList);
        }

        /// <summary>
        /// 通过OAuth2.0方式重定向过来
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ActionResult Index3(string code, string state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Content("您拒绝了授权！");
            }

            if (state != "dp2weixin")
            {
                return Content("验证失败！请从正规途径进入！");
            }

            //用code换取access_token
            var result = OAuthApi.GetAccessToken(dp2CmdService2.Instance.weiXinAppId, dp2CmdService2.Instance.weiXinSecret, code);
            if (result.errcode != ReturnCode.请求成功)
            {
                return Content("错误：" + result.errmsg);
            }

            //下面2个数据也可以自己封装成一个类，储存在数据库中（建议结合缓存）
            //如果可以确保安全，可以将access_token存入用户的cookie中，每一个人的access_token是不一样的
            //Session["OAuthAccessTokenStartTime"] = DateTime.Now;
            //Session["OAuthAccessToken"] = result;            

            // 取出微信id
            string weixinId = result.openid;
            return this.Index(weixinId);
        }



        public ActionResult Bind(string weixinId)
        {
            return View();
        }




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

            if (bReader==true)
            {
                string prefix = Request.Form["selPrefix"];
                if (String.IsNullOrEmpty(prefix) == false)
                {
                    userName = prefix + ":" + userName;
                }
            }

            // 这里调绑定的接口

            string strError = "";
            string strRight = "";
            /*
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
            */

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            ModelState.AddModelError("", strError);//"提供的用户名或密码不正确。");

            // 继续跟上登录成功返回的url
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }


        public ActionResult Logout()
        {
            // 将session置空
            //Session[SessionInfo.C_Session_sessioninfo] = null;

            return RedirectToAction("Main", "Charging");
        }

    }
}