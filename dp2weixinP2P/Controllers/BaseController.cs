using dp2Command.Service;
using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dp2weixinP2P.Controllers
{
    public class BaseController : Controller
    {



        public int CheckIsFromWeiXin(string code, string state,out string strError)
        {
            strError = "";

            // 从微信进入的
            string weiXinId = "";
            if (string.IsNullOrEmpty(code) == false)
            {
                int nRet = dp2CmdService2.Instance.GetWeiXinId(code, state, out weiXinId, out strError);
                if (nRet == -1)
                { return -1; }
            }

            if (String.IsNullOrEmpty(weiXinId) == false)
            {
                // 记下微信id
                Session[WeiXinConst.C_Session_WeiXinId] = weiXinId;
            }

            if (Session[WeiXinConst.C_Session_WeiXinId] == null
                || (String)Session[WeiXinConst.C_Session_WeiXinId] == "")
            {
                strError = "非正规途径进入(未传入微信用户ID) 或者Session已失效请重新从微信中\"我爱图书馆\"公众号进入。";
                return -1;
            }


            weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            // 检查微信id是否已经绑定的读者
            List<WxUserItem> userList = WxUserDatabase.Current.GetByWeixinId(weiXinId);
            if (userList !=null && userList.Count >0)
                Session[WeiXinConst.C_Session_IsBind] = 1;
            else
                Session[WeiXinConst.C_Session_IsBind] = 0;

            return 0;
        }
    }
}