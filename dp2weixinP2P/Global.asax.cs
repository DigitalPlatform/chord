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

            // 从web config中取出url,weixin代理账号
            string strDp2Url = WebConfigurationManager.AppSettings["dp2Url"];
            string strDp2UserName = WebConfigurationManager.AppSettings["dp2UserName"];
            // todo 密码改为加密格式
            string strDp2Password = WebConfigurationManager.AppSettings["dp2Password"];

            // 错误日志目录
            string strDp2WeiXinLogDir = WebConfigurationManager.AppSettings["dp2WeiXinLogDir"];
            PathUtil.CreateDirIfNeed(strDp2WeiXinLogDir);	// 确保目录创建

            string strDp2WeiXinUrl = "http://dp2003.com/dp2weixin";

            // 初始化命令服务类
            dp2CommandService.Instance.Init(strDp2Url,
                strDp2UserName,
                strDp2Password,
                strDp2WeiXinUrl,
                strDp2WeiXinLogDir,
                true,
                mongoDbConnStr,
                instancePrefix);
        }
    }
}