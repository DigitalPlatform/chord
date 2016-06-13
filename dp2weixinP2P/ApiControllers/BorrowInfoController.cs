using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class BorrowInfoController : ApiController
    {

        // GET api/<controller>
        public BorrowInfoResult GetBorrowInfos(string libUserName,
            string patronBarcode)
        {

            return  dp2WeiXinService.Instance.GetPatronBorrowInfos(libUserName,
                 patronBarcode);
        }

        // POST api/<controller>
        [HttpPost]
        public ApiResult Post(string libUserName,
            string action,
            string patron,
            string item)
        {
            ApiResult result = new ApiResult();

            if (action == "renew")
            { 
            string strError = "";
            int nRet = dp2WeiXinService.Instance.Renew(libUserName,
                item,
                out strError);
            result.errorCode = nRet;
            result.errorInfo = strError;
        }

            return result;
        }



    }
}
