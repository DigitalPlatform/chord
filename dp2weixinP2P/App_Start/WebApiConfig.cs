using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace dp2weixinWeb
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //2.自定义路由一：匹配到action
            config.Routes.MapHttpRoute(
                name: "ActionApi",
                routeTemplate: "api2/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

        }
    }
}
