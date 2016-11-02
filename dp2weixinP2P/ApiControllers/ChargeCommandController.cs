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

            ChargeCommandContainer cmdContainer = (ChargeCommandContainer)HttpContext.Current.Session[WeiXinConst.C_Session_CmdContainer];
            result.cmds = cmdContainer;

            return result;
        }

        [HttpPost]
        public ChargeCommand CreateCmd(string weixinId, 
            string libId,
            ChargeCommand cmd)
        {

            ChargeCommandContainer cmdContainer = (ChargeCommandContainer)HttpContext.Current.Session[WeiXinConst.C_Session_CmdContainer];

            // 执行命令
            return cmdContainer.AddCmd(weixinId,
                libId,
                cmd);
        }
    }
}
