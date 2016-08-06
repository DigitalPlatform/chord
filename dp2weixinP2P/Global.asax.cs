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

            // 注册一缓存条目在5分钟内到期,到期后模拟点击网站网页  
            //this.RegisterCacheEntry();
            //HttpContext.Current.Cache.Remove(DummyCacheItemKey);

            dp2WeiXinService.Instance.WriteLog("走进Application_Start");
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

            dp2WeiXinService.Instance.WriteLog("走进Application_End");
        }       



    }
}