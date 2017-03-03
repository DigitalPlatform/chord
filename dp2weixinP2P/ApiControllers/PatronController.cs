using dp2Command.Service;
using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class PatronController : ApiController
    {
        // 参数值常量
        public const string C_format_summary = "summary";
        public const string C_format_verifyBarcode = "verifyBarcode";

        
        /// <summary>
        /// 获得读者基本信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ApiResult GetPatron(string libId,
            string userName,
            bool isPatron,
            string patronBarcode,
            string style)
        {
            ApiResult result = new ApiResult();

            string strError="";
            string info="";
            Patron patron= null;
            int nRet = dp2WeiXinService.Instance.GetPatronInfo(libId,
                userName,
                isPatron,
                patronBarcode,
                style,
                out patron,
                out info,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }

            result.obj = patron;
            result.info = info;

            return result;
        }


        [HttpPost]
        public ApiResult SetPatron(string libId,
            string userName,
            string action,
            string recPath,
            SimplePatron patron)
        {
            ApiResult result = new ApiResult();


            return result;
        }

    }


}
