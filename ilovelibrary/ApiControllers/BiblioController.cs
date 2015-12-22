using ilovelibrary.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ilovelibrary.ApiControllers
{
    public class BiblioController : ApiController
    {
        /// <summary>
        /// 获得读者的借阅信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public string GetBiblio(string id, [FromUri] string format)
        {
            if (HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo] == null)
            {
                throw new Exception("尚未登录");
            }         
            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo];

            if (format == "summary") //todo 将summary字符串改为常量
            {
                string strSummary = ilovelibraryServer.Instance.GetBiblioSummary(sessionInfo, id);
                return strSummary;
            }

            return "未实现的风格:" + format;
        }
    }
}
