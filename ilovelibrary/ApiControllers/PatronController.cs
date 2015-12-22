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
    public class PatronController : ApiController
    {
        /// <summary>
        /// 获得读者基本信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [NotImplExceptionFilter]
        public PatronResult GetPatron(string id)
        {
            if (HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo] == null)
            {
                throw new Exception("尚未登录!");
            }
            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo];
            
            // 获取读者基本信息
            PatronResult patronResult = ilovelibraryServer.Instance.GetPatronInfo(sessionInfo, id);
            return patronResult;
        }

        /// <summary>
        /// 获得读者的借阅信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public BorrowInfoResult GetBorrowInfo(string id, [FromUri] string format)
        {            
            if (HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo] == null)
            {
                throw new Exception("尚未登录");
            }
            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo] ;
            
            //获取读者借阅信息
            BorrowInfoResult result = ilovelibraryServer.Instance.GetBorrowInfo(sessionInfo, id);

            return result;              
        }


    }


}
