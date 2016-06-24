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
        public BorrowInfoResult GetBorrowInfos(string libId,
            string patronBarcode)
        {

            return  dp2WeiXinService.Instance.GetPatronBorrowInfos1(libId,
                 patronBarcode);
        }

        // POST api/<controller>
        [HttpPost]
        public ApiResult Post(string libId,
            string action,
            string patron,
            string item)
        {
            ApiResult result = new ApiResult();

            if (action == "renew")
            { 
            string strError = "";
            int nRet = dp2WeiXinService.Instance.Renew1(libId,
                item,
                out strError);
            result.errorCode = nRet;
            result.errorInfo = strError;
        }

            return result;
        }



    }
}
