using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dp2Router.Models;
using dp2Router.RouteHandlers;

namespace dp2Router
{
#if NO
    static class Routes
    {

        public static List<Route> GET
        {
            get
            {
                return new List<Route>()
                {
                    new Route()
                    {
                        Callable = HomeIndex,
                        UrlRegex = "^\\/$",
                        Method = "GET"
                    },
                    new Route()
                    {
                        Callable = new FileSystemRouteHandler() { BasePath = @"C:\Users\Barend.Erasmus\Desktop\Test"}.Handle,
                        UrlRegex = "^\\/Static\\/(.*)$",
                        Method = "GET"
                    }
                };

            }
        }

        private static HttpResponse HomeIndex(HttpRequest request)
        {
            return new HttpResponse()
            {
                ContentAsUTF8 = "Hello",
                ReasonPhrase = "OK",
                StatusCode = "200"
            };

        }
    }
#endif
}
