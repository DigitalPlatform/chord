using ilovelibrary.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;

namespace ilovelibrary
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {

        static void ConfigureApi(HttpConfiguration config)
        {
            config.Filters.Add(new NotImplExceptionFilter());
        } 

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            //注册异常过滤
            //ConfigureApi(GlobalConfiguration.Configuration);

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // 初始化全局服务器
            //"http://localhost/dp2library/xe/rest";//"http://dp2003.com/dp2library/rest/";
            string dp2LibraryUrl = WebConfigurationManager.AppSettings["dp2LibraryUrl"];
            string dataDir = WebConfigurationManager.AppSettings["ilovelibraryDataDir"]; 
            string dp2OpacUrl = WebConfigurationManager.AppSettings["dp2OpacUrl"]; 
            ilovelibraryServer.Instance.Init(dp2LibraryUrl, dataDir,dp2OpacUrl);
        }


        public override void Init()
        {
            this.PostAuthenticateRequest += (sender, e) => HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            base.Init();
        }
    }
}