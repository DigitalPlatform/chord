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
        /// 获得summary
        /// </summary>
        /// <param name="id"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public string GetBiblio(string id, [FromUri] string format)
        {
            string strSummary = "";
            if (HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo] == null)
            {
                throw new Exception("尚未登录");
            }         
            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo];
            if (id == "more") 
            {
                strSummary = ilovelibraryServer.Instance.GetBarcodesSummary(sessionInfo, format);
                return strSummary;
            }


            if (format == "summary") //todo 将summary字符串改为常量
            {
                strSummary = ilovelibraryServer.Instance.GetBiblioSummary(sessionInfo, id);
                return strSummary;
            }


            return "未实现的风格:" + format;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action">search</param>
        /// <param name="functionType"></param>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public SearchItemResult GetItem(string action,string searchText, string functionType )
        {
            // 检查是否登录
            if (HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo] == null)
            {
                SearchItemResult result = new SearchItemResult();
                result.apiResult = new ApiResult();
                result.apiResult.errorCode = -1;
                result.apiResult.errorInfo = "尚未登录";
                return result;
            }
            
            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo];
            if (action == "search")
            {
                return ilovelibraryServer.Instance.SearchItem(sessionInfo, functionType, searchText);
            }

            return null;
        }
    }
}
