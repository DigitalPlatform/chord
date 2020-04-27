using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;


using System.Net.Http.Headers;

namespace dp2weixinWeb.ApiControllers
{
    public class UserMessageApiController : ApiController
    {
        private UserMessageDb userMessageDb = UserMessageDb.Current;

        // 获取全部绑定账户，包括读者与工作人员
        [HttpGet]
        public UserMessageResult Get(string userId)
        {
            //dp2WeiXinService.Instance.WriteLog1("WxUserController.Get()开始");

            UserMessageResult result = new UserMessageResult();

            List<UserMessageItem> list = new List<UserMessageItem>();
            if (string.IsNullOrEmpty(userId) == true)
            {
                list = this.userMessageDb.GetAll();// (null, null, null, -1, null, null, false);//.GetUsers();
            }
            else
            {
                list = this.userMessageDb.GetByUserId(userId);
            }

            result.messages = list;
            return result;
        }


        // 删除消息
        [HttpGet]
        public ApiResult Delete(string userId,string id)
        {
            ApiResult result = new ApiResult();

            this.userMessageDb.Delete(userId, id);

            return result;
        }

    }
}
