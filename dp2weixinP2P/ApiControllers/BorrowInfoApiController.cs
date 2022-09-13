using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class BorrowInfoApiController : ApiController
    {

        // 续借
        [HttpPost]
        public ApiResult Renew(string weixinId,   //前端id，目前没用到
            string libId,
            string patron,
            string item)
        {
            ApiResult result = new ApiResult();

            string strError = "";
            int nRet = dp2WeiXinService.Instance.Renew1(libId,
                patron,
                item,
                out strError);
            result.errorCode = nRet;
            result.errorInfo = strError;


            return result;
        }



    }
}
