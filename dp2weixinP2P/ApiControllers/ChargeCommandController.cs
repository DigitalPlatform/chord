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
        [HttpPost]
        public ChargeCommand CreateCmd(string libId,
            ChargeCommand cmd)
        {

            ChargeCommandContainer cmdContainer = (ChargeCommandContainer)HttpContext.Current.Session[WeiXinConst.C_Session_CmdContainer];

            // 执行命令
            return cmdContainer.AddCmd(libId,cmd);
        }
    }
}
