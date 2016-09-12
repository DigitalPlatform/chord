using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using System.Web.Configuration;
using dp2Command.Service;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using dp2weixin;
using System.IO;
using System.Web.Caching;
using dp2weixin.service;
using DigitalPlatform;

namespace dp2weixinWeb
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // 在应用程序启动时运行的代码
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // 从web config中取出数据目录
            string dataDir = WebConfigurationManager.AppSettings["DataDir"];
            if (String.IsNullOrEmpty(dataDir) == true)
            {
                throw new Exception("尚未在Web.config文件中设置DataDir参数");
            }
            if (dataDir.Substring(0, 1) == "~")
                dataDir = Server.MapPath(string.Format(dataDir));//"~/App_Data"
            if (Directory.Exists(dataDir) == false)
            {
                throw new Exception("微信数据目录" + dataDir + "不存在。");
            }

            // 初始化命令服务类
            dp2WeiXinService.Instance.Init(dataDir);

            dp2WeiXinService.Instance.WriteLog1("走进Application_Start");

            throw new Exception("test");

            //Application["app"] = "test";
        }


        public override void Init()
        {
            // 2016.8.7 web api不需要session，检索到第几页是通过页面传过去的。
            // 对web api启用session，主要用于检索下一页
            //this.PostAuthenticateRequest += (sender, e) => HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            
            base.Init();
        }

        void Application_End(object sender, EventArgs e)
        {
            dp2WeiXinService.Instance.Close();

            dp2WeiXinService.Instance.WriteLog1("走进Application_End");
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            Exception ex = HttpContext.Current.Server.GetLastError();

            string strText = ExceptionUtil.GetDebugText(ex);

            try
            {
                strText += "\r\n"
                + "\r\nUserHostAddres=" + HttpContext.Current.Request.UserHostAddress
                + "\r\nRequest.RawUrl=" + HttpContext.Current.Request.RawUrl
                + "\r\nForm Data=" + HttpContext.Current.Request.Form.ToString()
                + "\r\nForm Data(Decoded)=" + HttpUtility.UrlDecode(HttpContext.Current.Request.Form.ToString())
                + "\r\n\r\n版本: " + System.Reflection.Assembly.GetAssembly(typeof(dp2WeiXinService)).GetName().ToString();
            }
            catch (Exception ex1)
            {
                strText += "获取Request信息出错：" + ex1.Message;
            }


            string strError = "";
            try
            {
                string strSender = HttpContext.Current.Server.MachineName;
                //// 崩溃报告
                //int nRet = LibraryChannel.CrashReport(
                //    strSender,
                //    "dp2OPAC",
                //    strText,
                //    out strError);

                dp2WeiXinService.Instance.WriteErrorLog1(strText);
            }
            catch (Exception ex0)
            {
                strError = "CrashReport() 过程出现异常: " + ExceptionUtil.GetDebugText(ex0);
                // nRet = -1;
            }
        }

    }
}