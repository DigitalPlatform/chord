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
    public class PatronApiController : ApiController  //BaseApiController//
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

            WxUserItem userItem = null;
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
                out userItem,
                out  strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
                return result;
            }
            
            // 返回读者路径和时间戳
            result.recPath = outputRecPath;
            result.timestamp = outputTimestamp;

            // 如果是读者自助注册过来的，需要把注册的这个帐户设置为当前帐户
            if (string.IsNullOrEmpty(weixinId) ==false && userItem !=null )
            {
                result.info = userItem.readerBarcode;

                nRet = ApiHelper.ActiveUser(userItem, out strError);
                if (nRet == -1)
                {
                    result.errorInfo = strError;
                    return result;
                }
            }
            

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
