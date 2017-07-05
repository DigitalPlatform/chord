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
    public class ChargeCommandController : ApiController
    {
        [HttpGet]
        public ChargeCommandResult GetCommands(string libId)
        {
            ChargeCommandResult result = new ChargeCommandResult();

            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo];
            ChargeCommandContainer cmdContainer = sessionInfo.cmdContainer;
            result.cmds = cmdContainer;

            return result;
        }

        [HttpPost]
        public ChargeCommand CreateCmd(string weixinId, 
            string libId,
            string libraryCode,
            int isTransfromed,
            ChargeCommand cmd)
        {

            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo];
            ChargeCommandContainer cmdContainer = sessionInfo.cmdContainer;
            // 执行命令
            return cmdContainer.AddCmd(weixinId,
                libId,
                libraryCode,
                isTransfromed,
                cmd);
        }

        public ApiResult VerifyBarcode(string libId,
            string libraryCode,
            string userId,
            string barcode)
        {
            ApiResult result = new ApiResult();

            string error = "";
            string resultBarcode="";
            int nRet = dp2WeiXinService.Instance.VerifyBarcode(libId,
                libraryCode,
                userId,
                barcode,
                out resultBarcode,
                out error);
            result.errorCode = nRet;
            result.errorInfo = error;
            result.info = resultBarcode;

            return result;
        }
    }
}
