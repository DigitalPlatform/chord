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
    public class PatronApiController : ApiController
    {
        // 参数值常量
        public const string C_format_summary = "summary";
        public const string C_format_verifyBarcode = "verifyBarcode";


        /// <summary>
        /// 获取读者信息
        /// </summary>
        /// <param name="libId">图书馆id</param>
        /// <param name="userName">用户名</param>
        /// <param name="patronBarcode">读者证条码</param>
        /// <returns></returns>
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

        /// <summary>
        /// 设置读者信息
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="userName"></param>
        /// <param name="action"></param>
        /// <param name="recPath"></param>
        /// <param name="timestamp"></param>
        /// <param name="weixinId"></param>
        /// <param name="patron"></param>
        /// <param name="bMergeInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public SetReaderInfoResult SetPatron(string libId,
            string userName,
            string action,
            string recPath,
            string timestamp,
            string weixinId,
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
                weixinId,
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

            // 初始化sesson
            string error = "";
            if (string.IsNullOrEmpty(weixinId) ==false)
            {
                if (HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo] == null)
                {
                    error = "session失效。";
                    goto ERROR1;
                }
                SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo];
                if (sessionInfo == null)
                {
                    error = "session失效2。";
                    goto ERROR1;
                }
                nRet = sessionInfo.GetActiveUser(weixinId, out error);
                if (nRet == -1)
                    goto ERROR1;
            }
            



            return result;


        ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="tel"></param>
        /// <returns></returns>
        public GetVerifyCodeResult GetVerifyCode(string libId, 
            string tel)
        {
            GetVerifyCodeResult result = new GetVerifyCodeResult();
            result.verifyCode = "7";

            string error = "";
            int nRet = dp2WeiXinService.Instance.GetVerifyCode(libId,
                tel,
                out string verifyCode,
                out error);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = error;
            }

            result.verifyCode = verifyCode;

            return result;
        }



    }


}
