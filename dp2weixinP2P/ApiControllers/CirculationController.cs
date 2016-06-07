using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class CirculationController : ApiController
    {
        // POST api/<controller>
        [HttpPost]
        public ApiResult Reservation(string libUserName,
            string patron,
            string items,
            string style)
        {
            ApiResult result = new ApiResult();

            string strError = "";
            int nRet = dp2WeiXinService.Instance.Reservation(libUserName,
                patron,
                items,
                style,
                out strError);
            result.errorCode = nRet;
            result.errorInfo = strError;

            return result;
        }
    }
}
