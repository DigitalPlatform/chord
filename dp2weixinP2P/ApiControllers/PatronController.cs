using DigitalPlatform.Message;
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

#if no
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
                "advancexml",
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
#endif

        public SetReaderInfoResult GetPatron(string libId,
           string userName,
           string patronBarcode)
        {
            SetReaderInfoResult result = new SetReaderInfoResult();

            string strError = "";
            string recPath = "";
            string timestamp = "";

            LoginInfo loginInfo = new LoginInfo(userName,false);

            string strXml = "";
            int nRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                loginInfo,
                patronBarcode,
                "xml,timestamp",
                out recPath,
                out timestamp,
                out strXml,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }

            result.recPath = recPath;
            result.timestamp = timestamp;

            int showPhoto = 0;
            Patron patron = dp2WeiXinService.Instance.ParsePatronXml(libId,
                strXml,
                recPath,
                showPhoto);

            result.obj = patron;

            return result;
        }

        [HttpPost]
        public SetReaderInfoResult SetPatron(string libId,
            string userName,
            string action,
            string recPath,
            string timestamp,
            SimplePatron patron,
            bool bMergeInfo)
        {
            SetReaderInfoResult result = new SetReaderInfoResult();

            string strError="";

            string outputRecPath = "";
            string outputTimestamp = "";

            int nRet = dp2WeiXinService.Instance.SetReaderInfo(libId,
                userName,
                action,
                recPath,
                timestamp,
                patron,
                bMergeInfo,
                out outputRecPath,
                out outputTimestamp,
                out  strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }
            
            

            result.recPath = outputRecPath;
            result.timestamp = outputTimestamp;


            return result;
        }

    }


}
