using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class WxUserController : ApiController
    {
        private WxUserDatabase wxUserDb = WxUserDatabase.Current;

        // 获取全部绑定账户，包括读者与工作人员
        [HttpGet]
        public WxUserResult Get()
        {
            WxUserResult result = new WxUserResult();
            List<WxUserItem> list = wxUserDb.Get(null,null,-1,null,null,false);//.GetUsers();
            result.users = list;
            return result;
        }

        public WxUserResult Get(string weixinId)
        {
            WxUserResult result = new WxUserResult();
            List<WxUserItem> list = wxUserDb.Get(weixinId, null, -1);
            result.users = list;
            return result;
        }

        [HttpPost]
        public WxUserResult DoThing(string actionType)
        {
            // 恢复用户
            if (actionType == "recover")
            {
                return dp2WeiXinService.Instance.RecoverUsers();
            }

            WxUserResult result = new WxUserResult();
            return result;
        }

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libId"></param>
        /// <param name="name"></param>
        /// <param name="tel"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult ResetPassword(string weixinId,
            string libId,
            string name, 
            string tel)
        {
            ApiResult result = new ApiResult();

            string strError = "";
            string patronBarcode = "";
            int nRet = dp2WeiXinService.Instance.ResetPassword(weixinId,
                libId,
                name,
                tel,
                out patronBarcode,
                out strError);
            result.errorCode = nRet;
            result.errorInfo = strError;
            result.info = patronBarcode;

            return result;
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult Setting(string weixinId, UserSettingItem item)
        {
            ApiResult result = new ApiResult();

            //string setting_lib = libId;

            try
            {
                UserSettingDb.Current.SetLib(item);

                // 2016-8-13 jane 检查微信用户对于该馆是否设置了活动账户
                dp2WeiXinService.Instance.CheckUserActivePatron(item.weixinId, item.libId);

            }
            catch (Exception ex)
            {
                result.errorCode = -1;
                result.errorInfo = ex.Message;
                return result;
            }

            return result;        
        }


        // 绑定
        [HttpPost]
        public WxUserResult Bind(BindItem item)
        {
            // 返回对象
            WxUserResult result = new WxUserResult();

            // 前端有时传上来是这个值
            if (item.prefix == "null")
                item.prefix = "";
            WxUserItem userItem = null;
            string strError="";
            int nRet= dp2WeiXinService.Instance.Bind(item.libId,
                item.prefix,
                item.word,
                item.password,
                item.appId,
                item.weixinId,
                out userItem,
                out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }
            result.users = new List<WxUserItem>();
            result.users.Add(userItem);

            return result;// repo.Add(item);
        }



        // 修改密码
        [HttpPost]
        public ApiResult ChangePassword(string libId,
            string patron,
            string oldPassword,
            string newPassword)
        {
            ApiResult result = new ApiResult();

            string strError = "";
            int nRet = dp2WeiXinService.Instance.ChangePassword(libId,
                patron,
                oldPassword,
                newPassword,
                out strError);
            result.errorCode = nRet;
            result.errorInfo = strError;

            return result;
        }


        // 设为活动账户
        [HttpPost]
        public void ActivePatron(string weixinId,string id)
        {
            if (weixinId == "null")
                weixinId = "";

            if (id == "null")
                id = "";

            WxUserItem user = wxUserDb.GetById(id);
            if (user != null)
            {
                //设为活动账户
                WxUserDatabase.Current.SetActivePatron(user.weixinId, user.id);

                // 自动更新设置的当前图书馆
                dp2WeiXinService.Instance.UpdateUserSetting(user.weixinId, user.libId, "",false,user.refID);
            }
        }

        // 删除
        [HttpDelete]
        public ApiResult Delete(string id)
        {
            ApiResult result = new ApiResult();
            string strError = "";
            int nRet = dp2WeiXinService.Instance.Unbind(id, out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }

            return result;
        }



    }
}
