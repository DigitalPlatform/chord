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
    public class WxUserController : ApiController
    {
        private WxUserDatabase wxUserDb = WxUserDatabase.Current;

        // 获取全部绑定账户，包括读者与工作人员
        [HttpGet]
        public WxUserResult Get()
        {
            WxUserResult result = new WxUserResult();
            List<WxUserItem> list = wxUserDb.Get(null,null,-1,null,null,false);//.GetUsers();

            // 在绑定的时候已经设置好了，
            //foreach (WxUserItem user in list)
            //{
            //    if (String.IsNullOrEmpty(user.libraryCode) == false)
            //        user.libName = user.libraryCode;
            //}
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
        public WxUserResult DoThing_HF(string actionType)
        {
            // 恢复用户
            if (actionType == "recover")
            {
                return dp2WeiXinService.Instance.RecoverUsers_HF();
            }

            if (actionType == "addAppId")
            {
                return dp2WeiXinService.Instance.AddAppId_HF();
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
            string libraryCode,
            string name, 
            string tel)
        {
            ApiResult result = new ApiResult();

            string strError = "";
            string patronBarcode = "";
            int nRet = dp2WeiXinService.Instance.ResetPassword(weixinId,
                libId,
                libraryCode,
                name,
                tel,
                out patronBarcode,
                out strError);
            result.errorCode = nRet;
            result.errorInfo = strError;
            result.info = patronBarcode;

            return result;
        }

        //监控开关
        [HttpPost]        
        public ApiResult UpdateTracing(string workerId,
                    string tracing)
        {
            ApiResult result = new ApiResult();

            string strError = "";
            int nRet = dp2WeiXinService.Instance.UpdateTracingUser(workerId,
                tracing, 
                out strError);

            result.errorCode = nRet;
            result.errorInfo = strError;
            return result;
        }

        // 保存选择的馆藏地
        [HttpPost]
        public ApiResult SaveLocation(string userId, string locations)
        {
            ApiResult result = new ApiResult();

            WxUserItem user = WxUserDatabase.Current.GetById(userId);
            user.selLocation = locations;
            WxUserDatabase.Current.Update(user);


            return result;
        }

        // 保存是否校验条码
        [HttpPost]
        public ApiResult SaveVerifyBarcode(string userId, int verifyBarcode)
        {
            ApiResult result = new ApiResult();

            WxUserItem user = WxUserDatabase.Current.GetById(userId);
            user.verifyBarcode = verifyBarcode;
            WxUserDatabase.Current.Update(user);


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
            string error = "";


            try
            {
                // 保存设置
                UserSettingDb.Current.SetLib(item);

                // 2016-8-13 jane 检查微信用户对于该馆是否设置了活动账户
                dp2WeiXinService.Instance.CheckUserActivePatron(item.weixinId, item.libId);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                goto ERROR1;
            }

            //======================
            // 更新session信息
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
            int nRet = sessionInfo.SetCurInfo(out error);
            if (nRet == -1)
                goto ERROR1;

            //===================

            return result;

        ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
        }


        [HttpPost]
        public ApiResult SetLibId(string weixinId, string libId)
        {
            ApiResult result = new ApiResult();
            string error = "";
            try
            {
                string temp = libId;
                string libraryCode = "";
                int nIndex = libId.IndexOf("~");
                if (nIndex > 0)
                {
                    libId = temp.Substring(0, nIndex);
                    libraryCode = temp.Substring(nIndex + 1);
                }

                UserSettingItem item = UserSettingDb.Current.GetByWeixinId(weixinId);
                if (item == null)
                {
                    item = new UserSettingItem();
                    item.weixinId = weixinId;
                    item.libId = libId;
                    item.libraryCode = libraryCode;
                    item.showCover = 1;
                    item.showPhoto = 1;
                    item.xml = "";
                    item.patronRefID = "";
                    UserSettingDb.Current.Add (item);
                }
                else
                {
                    // 保存设置
                    UserSettingDb.Current.UpdateLibId(weixinId, libId, libraryCode);// (item);
                }




                // 2016-8-13 jane 检查微信用户对于该馆是否设置了活动账户
                dp2WeiXinService.Instance.CheckUserActivePatron(weixinId, libId);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                goto ERROR1;
            }

            //======================
            // 更新session信息
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
            int nRet = sessionInfo.SetCurInfo(out error);
            if (nRet == -1)
                goto ERROR1;

            //===================

            return result;

        ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
        }

        // 绑定
        [HttpPost]
        public WxUserResult Bind(BindItem item)
        {
            if (item.bindLibraryCode == null)
                item.bindLibraryCode = "";

            // 返回对象
            WxUserResult result = new WxUserResult();

            // 前端有时传上来是这个值
            if (item.prefix == "null")
                item.prefix = "";
            WxUserItem userItem = null;
            string error="";
            int nRet= dp2WeiXinService.Instance.Bind(item.libId,
                item.bindLibraryCode,
                item.prefix,
                item.word,
                item.password,
                item.weixinId,
                out userItem,
                out error);
            if (nRet == -1)
            {
                goto ERROR1;
            }
            result.users = new List<WxUserItem>();
            result.users.Add(userItem);

            //======================
            // 更新session信息
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
             nRet = sessionInfo.SetCurInfo(out error);
            if (nRet == -1)
                goto ERROR1;

            //===================

            return result;// repo.Add(item);

        ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
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
        public ApiResult ActivePatron(string weixinId, string id)
        {
            ApiResult result = new ApiResult();
            string error = "";

            if (weixinId == "null")
                weixinId = "";

            if (id == "null")
                id = "";

            WxUserItem user = wxUserDb.GetById(id);
            if (user == null)
            {
                error = "未找到" + id + "对应的绑定用户";
                goto ERROR1;
            }

            //设为活动账户
            WxUserDatabase.Current.SetActivePatron(user.weixinId, user.id);

            // 自动更新设置的当前图书馆
            dp2WeiXinService.Instance.UpdateUserSetting(user.weixinId,
                user.libId,
                user.bindLibraryCode,
                "",
                false,
                user.refID);




            //======================
            // 更新session信息
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
            int nRet = sessionInfo.SetCurInfo(out error);
            if (nRet == -1)
                goto ERROR1;

            //===================


            return result;// repo.Add(item);


            ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;

        }

        // 解绑
        [HttpDelete]
        public ApiResult Delete(string id)
        {

            ApiResult result = new ApiResult();
            string error = "";
            int nRet = dp2WeiXinService.Instance.Unbind(id, out error);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            // 由于有错误信息的话，把错误信息输出
            if (String.IsNullOrEmpty(error) == false)
                result.errorInfo = error;

            //======================
            // 更新session信息
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

            if (string.IsNullOrEmpty(sessionInfo.WeixinId) == false) //微信入口进来的
            {
                nRet = sessionInfo.SetCurInfo(out error);
                if (nRet == -1)
                    goto ERROR1;
            }

            //===================

            return result;


        ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
        }



    }
}
