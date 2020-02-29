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
    public class ApiHelper
    {

        /// <summary>
        /// 将帐户置为当前活动帐户，会被bind,读者注册功能调用
        /// </summary>
        /// <param name="userItem"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static int ActiveUser(WxUserItem userItem,
            out string error)
        {
            error = "";
            int nRet = 0;

            if (HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo] == null)
            {
                error = "session失效。";
                return -1;
            }
            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo];
            if (sessionInfo == null)
            {
                error = "session失效2。";
                return -1;
            }

            // 读者注册过来的，不可能为null
            if (userItem == null)
            {
                error = "异常：userItem不能为null。";
                return -1;
            }

            if (sessionInfo.ActiveUser != null)
            {
                dp2WeiXinService.Instance.WriteDebug("原来session中的user对象id=[" + sessionInfo.ActiveUser.id + "],weixinid=[" + sessionInfo.WeixinId + "]");
            }
            else
            {
                dp2WeiXinService.Instance.WriteDebug("原来session中无user对象");
            }

            // 置为活动状态 2020-3-1,改在外面函数设，不再SaveUserToLocal函数里
            WxUserDatabase.Current.SetActivePatron1(userItem.weixinId, userItem.id);

            // 更新session的activeUser
            nRet = sessionInfo.GetActiveUser(userItem.weixinId, out error);
            if (nRet == -1)
                return -1;

            return 0;
        }



    }
}