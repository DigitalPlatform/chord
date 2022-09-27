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
        // weixinId:前端id
        // libId:图书馆id
        // patronBarcode:读者证条码号
        // itemBarcode:册条码号
        [HttpPost]
        public ApiResult Renew(string weixinId,   //前端id,目前没用到
            string libId,
            string patronBarcode,
            string itemBarcode)
        {
            ApiResult result = new ApiResult();

            string strError = "";
            int nRet = dp2WeiXinService.Instance.Renew1(libId,
                patronBarcode,
                itemBarcode,
                out strError);
            result.errorCode = nRet;
            result.errorInfo = strError;


            return result;
        }



    }
}
