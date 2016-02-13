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


            // 从web config中取出url,weixin代理账号
            string mongoDbConnStr = WebConfigurationManager.AppSettings["mongoDbConnStr"];
            if (String.IsNullOrEmpty(mongoDbConnStr) == true)
            {
                throw new Exception("尚未配置mongodb连接字符串");
            }
            string instancePrefix = WebConfigurationManager.AppSettings["instancePrefix"];
            LibDatabase.Current.Open(mongoDbConnStr, instancePrefix);
        }
    }
}