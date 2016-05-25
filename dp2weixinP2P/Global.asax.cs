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
using dp2Command.Service;
using DigitalPlatform.Text;
using dp2weixin;
using System.IO;
using System.Web.Caching;

namespace dp2weixinP2P
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
            if (String.IsNullOrEmpty(dataDir)== true)
            {
                throw new Exception("尚未在Web.config文件中设置DataDir参数");
            }
            if (dataDir.Substring(0, 1) == "~")
                dataDir = Server.MapPath(string.Format(dataDir));//"~/App_Data"
            if (Directory.Exists(dataDir) == false)
            {
                throw new Exception("微信数据目录"+dataDir+"不存在。");
            }
            // 初始化命令服务类
            dp2CmdService2.Instance.Init(dataDir);

            // 注册一缓存条目在5分钟内到期,到期后模拟点击网站网页  
            this.RegisterCacheEntry();
        }

        void Application_End(object sender, EventArgs e)
        {
            dp2CmdService2.Instance.Close();
        }


        //防止程序无访问时，停掉 http://blog.csdn.net/a497785609/article/details/5941283
        //http://www.codeproject.com/Articles/12117/Simulate-a-Windows-Service-using-ASP-NET-to-run-sc
        private const string DummyPageUrl = "http:/dp2003.com/dp2weixin/home/index";
        private const string DummyCacheItemKey = "dp2weixin-index";
        // 注册一缓存条目在5分钟内到期，到期后触发的调事件  
        private void RegisterCacheEntry()
        {
            if (null != HttpContext.Current.Cache[DummyCacheItemKey]) 
                return;

            HttpContext.Current.Cache.Add(DummyCacheItemKey,
                "Test",
                null,
                DateTime.MaxValue,
                TimeSpan.FromMinutes(5),
                CacheItemPriority.NotRemovable,
                new CacheItemRemovedCallback(CacheItemRemovedCallback));

            dp2CmdService2.Instance.WriteInfoLog("注册一缓存条目在5分钟内到期");
        }

        // 缓存项过期时程序模拟点击页面，阻止应用程序结束  
        public void CacheItemRemovedCallback(string key, object value, CacheItemRemovedReason reason)
        {
            HitPage();
        }

        // 模拟点击网站网页  
        private void HitPage()
        {
            System.Net.WebClient client = new System.Net.WebClient();
            client.DownloadData(DummyPageUrl);
        }
        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            if (HttpContext.Current.Request.Url.ToString() == DummyPageUrl)
            {
                RegisterCacheEntry();
            }
        }  


    }
}