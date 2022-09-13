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
    // 用户通知
    public class UserNoticeApiController : ApiController
    {
        private UserMessageDb userMessageDb = UserMessageDb.Current;

        // 获取全部消息
        [HttpGet]
        public UserMessageResult GetNotices(string userId)
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
        [HttpPost]
        public ApiResult DeleteNotice(string bindUserId, string id)
        {
            ApiResult result = new ApiResult();

            this.userMessageDb.Delete(bindUserId, id);

            return result;
        }

    }
}
