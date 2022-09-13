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
    public class WxUserApiController : ApiController
    {
        // 绑定帐户mongodb数据库
        private WxUserDatabase wxUserDb = WxUserDatabase.Current;

        // 获取一个唯一的guid
        [HttpGet]
        public string GetGuid()
        {
            return Guid.NewGuid().ToString();
        }

        // 绑定帐号
        // 该接口用来为三种来源的用户（微信用户、浏览器用户、小程序用户）绑定对应的图书馆系统的帐户，包括馆户帐户和读者读者。
        [HttpPost]
        public WxUserResult Bind(BindItem item)
        {
            string error = "";

            dp2WeiXinService.Instance.WriteDebug("走进bind API");

            if (item.bindLibraryCode == null)
                item.bindLibraryCode = "";

            // 返回对象
            WxUserResult result = new WxUserResult();

            // 前端有时传上来是这个值
            if (item.prefix == "null")
                item.prefix = "";





            WxUserItem userItem = null;

            int nRet = dp2WeiXinService.Instance.Bind(item.libId,
                item.bindLibraryCode,
                item.prefix,
                item.word,
                item.password,
                item.weixinId,
                out userItem,
                out error);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = error;
                return result;
            }
            result.users = new List<WxUserItem>();
            result.users.Add(userItem);

            // 将绑定的帐户设为当前帐户
            nRet = ApiHelper.ActiveUser(userItem, out error);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = error;
                return result;
            }

            return result;
        }

        // 根据前端用户的id获取绑定的图书馆帐号，可能一个前端用户绑定了多个图书馆帐号
        // weixinId：前端用户的id，用户来源唯一号，格式如下：
        // web浏览器来源的，~~开头
        // 微信公众号来源的，weixinId@公众号appid
        // 小程序来源的：!!用户id
        public WxUserResult GetBindUsers(string weixinId)
        {
            WxUserResult result = new WxUserResult();

            // 2022/07/25 必须给weixinId传参数，否则导致获取全部帐号。
            if (string.IsNullOrEmpty(weixinId) == true)
            { 
                result.errorCode= -1;
                result.errorInfo = "参数weixinId不能为空";
                return result;
            }


            List<WxUserItem> list = wxUserDb.Get(weixinId, null, -1);
            foreach (WxUserItem user in list)
            {
                // 把读者xml删除，否则多个帐户时传输数据量大
                user.xml = "";
            }

            result.users = list;
            return result;
        }

        // 解绑帐户
        // bindUserId：该参数传绑定接口返回结果中绑定对象的id，
        // 这是我爱图书馆服务器端mongodb存储的绑定对象的唯一id，即mongodb记录中的id，
        // 不是从前端角度来看的用户的id，因为一个前端用户可以绑定多个帐户，
        // 该接口是删除其中一个绑定帐户。
        [HttpDelete]
        public ApiResult Unbind(string bindUserId)
        {
            ApiResult result = new ApiResult();
            string error = "";
            bool isPublic = false;
            WxUserItem newActiveUser = null;
            int nRet = dp2WeiXinService.Instance.Unbind(bindUserId,
                out newActiveUser,
                out error,
                out isPublic);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = error;
                return result;
            }

            // 由于有错误信息的话，把错误信息输出
            if (String.IsNullOrEmpty(error) == false)
                result.errorInfo = error;


            // 设置当前活动帐户，更新session信息
            if (newActiveUser != null)  //如果当前删除不是活动帐户，则返回的newActiveUser为null
            {
                nRet = ApiHelper.ActiveUser(newActiveUser, out error);
                if (nRet == -1)
                {
                    result.errorCode = -1;
                    result.errorInfo = error;
                    return result;
                }
            }
            // 2022/7/20 支持解绑public帐户，同时当删除public帐户时，则session中存的当前帐户置为null
            else if (newActiveUser == null && isPublic == true)
            {
                SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo];
                if (sessionInfo != null && sessionInfo.ActiveUser!=null
                    && sessionInfo.ActiveUser.userName=="public")
                {
                    sessionInfo.ActiveUser = null;
                }
            }


            return result;

        }

        // 设为当前活动账户
        // weixinId:前端用户的唯一id
        // bindUserId:绑定帐户的记录id
        [HttpPost]
        public ApiResult ActiveUser(string weixinId, string bindUserId)
        {
            ApiResult result = new ApiResult();
            string error = "";

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

            if (weixinId == "null")
                weixinId = "";

            if (bindUserId == "null")
                bindUserId = "";

            WxUserItem user = wxUserDb.GetById(bindUserId);
            if (user == null)
            {
                error = "未找到" + bindUserId + "对应的绑定用户";
                goto ERROR1;
            }

            //设为活动账户
            WxUserDatabase.Current.SetActivePatron1(user.weixinId, user.id);

            //更新session
            int nRet = sessionInfo.GetActiveUser(weixinId, out error);
            if (nRet == -1)
                goto ERROR1;


            return result;// repo.Add(item);


        ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;

        }

        // 设置前端用户的当前图书馆，
        // 如果从来没有绑定过该馆帐户，则以public身份;
        // 如果绑定过该馆帐户，则以绑定的第1个帐户为活动帐号
        // 参数：
        // weixinId:前端用户的唯一id
        // libId:图书馆id，如果是分馆，格式为:图书馆id~分馆代码
        [HttpPost]
        public ApiResult SetLibId(string weixinId, string libId)
        {
            ApiResult result = new ApiResult();
            string error = "";

            string temp = libId;
            string bindLibraryCode = "";
            int nIndex = libId.IndexOf("~");
            if (nIndex > 0)
            {
                libId = temp.Substring(0, nIndex);
                bindLibraryCode = temp.Substring(nIndex + 1);
            }

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

            //2022 / 7 / 20 把一段注释掉，不论session中是否有当前帐户，都设置一下
            //如果选择的图书馆就是是当前活动帐户对应的图书馆，则不用处理
            if (sessionInfo.ActiveUser != null
                && sessionInfo.ActiveUser.libId == libId
                && sessionInfo.ActiveUser.bindLibraryCode == bindLibraryCode)
            {
                return result;
            }


            // 先看看有没有public的,有的话，先删除
            //注意这里不过滤图书馆，就是说临时选择的图书馆，如果未绑定正式帐户，则会在选择下一个图书馆时被清除
            List<WxUserItem> publicList = WxUserDatabase.Current.GetWorkers(weixinId, "", WxUserDatabase.C_Public);
            if (publicList.Count > 0)
            {
                //dp2WeiXinService.Instance.WriteDebug("删除了" + publicList.Count + "个临时public帐户");
                for (int i = 0; i < publicList.Count; i++)
                {
                    WxUserDatabase.Current.SimpleDelete(publicList[i].id);
                }
            }


            // 如果微信用户已经绑定了该图书馆的帐户，则设这个馆第一个帐户为活动帐户
            WxUserItem user = null;
            List<WxUserItem> list = WxUserDatabase.Current.Get(weixinId, libId, -1); //注意这里不区分分馆,在下面还是要看分馆
            if (list.Count > 0)
            {
                List<WxUserItem> foundList = new List<WxUserItem>();
                foreach (WxUserItem u in list)
                {
                    if (u.bindLibraryCode == bindLibraryCode)
                    {
                        user = u;
                        break;
                    }
                }
            }

            // 如果微信用户针对这个选择的图书馆没有绑定过帐号，则自动创建一个临时public帐户
            if (user == null)
            {
                // 创建一个public帐号
                user = WxUserDatabase.Current.CreatePublic(weixinId, libId, bindLibraryCode);
            }

            // 设为当前帐户
            WxUserDatabase.Current.SetActivePatron1(user.weixinId, user.id);

            // 初始化sesson
            int nRet = sessionInfo.GetActiveUser(user.weixinId, out error);
            if (nRet == -1)
                goto ERROR1;

            //===================

            return result;

        ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
        }


        // 找回密码
        // weixinId:前端用户唯一id,目前没有实际使用到
        // libId:图书馆id
        // name:姓名
        // tel:手机号
        [HttpPost]
        public ApiResult ResetPassword(string weixinId,
            string libId,
            //string libraryCode,
            string name, 
            string tel)
        {
            ApiResult result = new ApiResult();

            string strError = "";
            string patronBarcode = "";
            int nRet = dp2WeiXinService.Instance.ResetPassword(//weixinId,
                libId,
                //libraryCode,
                name,
                tel,
                out patronBarcode,
                out strError);
            result.errorCode = nRet;
            result.errorInfo = strError;
            result.info = patronBarcode;

            return result;
        }

        // 修改密码
        // libId:图书馆id
        // patron:读者证条码号
        // oldPassword:旧密码
        // newPassword:新密码
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

        // todo 需增强安全性
        // 获取指定图书馆绑定的帐户，后台管理使用
        // libId:图书馆id
        // type:类型筛选
        // -1 表示不限，包括馆员和读者
        // 0 仅读者
        // 1 仅馆员
        // public  仅public帐户
        [HttpGet]
        public WxUserResult GetBindUsersByLibId(string libId, string type)
        {
            WxUserResult result = new WxUserResult();
            // 获取绑定的读者数量
            List<WxUserItem> users = new List<WxUserItem>();

            if (type == "-1")
                users = WxUserDatabase.Current.Get("", libId, -1);
            else if (type == "0")
                users = WxUserDatabase.Current.Get("", libId, WxUserDatabase.C_Type_Patron);
            else if (type == "1")
            {
                List<WxUserItem> tempList = WxUserDatabase.Current.Get("", libId, WxUserDatabase.C_Type_Worker);
                foreach (WxUserItem item in tempList)
                {
                    if (item.userName == WxUserDatabase.C_Public)
                        continue;
                    users.Add(item);
                }
            }
            else if (type == WxUserDatabase.C_Public)
            {
                List<WxUserItem> tempList = WxUserDatabase.Current.Get("", libId, WxUserDatabase.C_Type_Worker);
                foreach (WxUserItem item in tempList)
                {
                    if (item.userName == WxUserDatabase.C_Public)
                    {
                        users.Add(item);
                    }
                }
            }
            result.users = users;
            return result;
        }


        public const string C_UserInfoType_showCover = "showCover";  // 图书封面
        public const string C_UserInfoType_showPhoto = "showPhoto";  // 头像
        public const string C_UserInfoType_audioType = "audioType";  // 语音方案类型
        public const string C_UserInfoType_verifyBarcode = "verifyBarcode"; //借书时是否校验条码
        public const string C_UserInfoType_selLocation = "selLocation"; //当ISBN借书时，关注的馆藏地，些功能不常用
        public const string C_UserInfoType_tracing = "tracing"; //设置监控图书馆消息


        // 更新用户信息，只根据上次某名称图书馆名称变化，做了更新图书馆名称
        [HttpPost]
        public ApiResult UpdateUserInfo(string bindUserId, 
            string userInfoType, 
            WxUserItem input)
        {

            ApiResult result = new ApiResult();
            string error = "";
            int nRet = 0;

            // 修改用户的libName是用于维护中，图书馆的名称改为，但已绑定的帐户还是旧名称的情况。
            // 所以不需要校验传入的bindUserId是否与当前帐户一致，传哪个帐户改哪个帐户。
            if (userInfoType == "libName")
            {
                // 根据绑定id获取到绑定用户对象
                WxUserItem user = WxUserDatabase.Current.GetById(bindUserId);

                // 获取到对应的图书馆
                LibEntity lib = LibDatabase.Current.GetLibById1(user.libId);

                // 更新用户的图书馆名称
                user.libName = lib.libName;
                user.libraryCode = "";
                user.bindLibraryCode = "";
                WxUserDatabase.Current.Update(user);

                return result;  //直接返回
            }

            // 检查传入的帐户id是否与当前帐户一致。
            // 检查session中的当前帐户是否存在
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

            if (sessionInfo.ActiveUser == null)
            {
                error = "session中没有活动帐号，不可能的情况";
                goto ERROR1;
            }

            if (sessionInfo.ActiveUser.id != bindUserId)
            {
                error = "传来的user对象id与当前活动对象的id不一致。";
                goto ERROR1;
            }


            // 修改各种信息
            if (userInfoType == C_UserInfoType_showCover)  //设置是否显示封面
            {
                sessionInfo.ActiveUser.showCover = input.showCover;
                nRet = (int)WxUserDatabase.Current.Update(sessionInfo.ActiveUser);
            }
            else if (userInfoType == C_UserInfoType_showPhoto)  // 是否显示头像
            {
                sessionInfo.ActiveUser.showPhoto = input.showPhoto;
                nRet = (int)WxUserDatabase.Current.Update(sessionInfo.ActiveUser);
            }
            else if (userInfoType == C_UserInfoType_audioType)  // 语音方案类型
            {
                sessionInfo.ActiveUser.audioType = input.audioType;
                nRet = (int)WxUserDatabase.Current.Update(sessionInfo.ActiveUser);
            }
            else if (userInfoType == C_UserInfoType_verifyBarcode)  //借书时是否校验条码
            {
                sessionInfo.ActiveUser.verifyBarcode = input.verifyBarcode;
                nRet = (int)WxUserDatabase.Current.Update(sessionInfo.ActiveUser);
            }
            else if (userInfoType == C_UserInfoType_selLocation)  //当ISBN借书时，关注的馆藏地，些功能不常用
            {
                sessionInfo.ActiveUser.selLocation = input.selLocation;
                nRet = (int)WxUserDatabase.Current.Update(sessionInfo.ActiveUser);
            }
            else if (userInfoType == C_UserInfoType_tracing)
            {
                sessionInfo.ActiveUser.tracing = input.tracing;
                nRet = (int)WxUserDatabase.Current.Update(sessionInfo.ActiveUser);

                // 更新内存
                dp2WeiXinService.Instance.UpdateTracingUser(sessionInfo.ActiveUser);
            }
            else
            {
                error = "不能识别的用户信息类型[" + userInfoType + "]";
                goto ERROR1;
            }

            // 返回
            result.errorCode = nRet;
            return result;



        ERROR1:  // 出错返回
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
        }

        #region 被替代的代码 2022/9/8
        /*
        
        //设置监控图书馆消息
        [HttpPost]        
        public ApiResult UpdateTracing(string bindUserId,
                    string tracing)
        {
            ApiResult result = new ApiResult();

            string error = "";

            // 获取session信息
            if (HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo] == null)
            {
                error = "session失效。";
                goto ERROR1;
            }

            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo];
            if (sessionInfo.ActiveUser == null)
            {
                error = "session中没有活动帐号，不可能的情况";
                goto ERROR1;
            }

            if (sessionInfo.ActiveUser.id != bindUserId)
            {
                error = "传来的user对象id与当前活动对象的id不一致。";
                goto ERROR1;
            }

            // 更新数据库设置
            sessionInfo.ActiveUser.tracing = tracing;
            int nRet = (int)WxUserDatabase.Current.Update(sessionInfo.ActiveUser);
            // 更新内存
            dp2WeiXinService.Instance.UpdateTracingUser(sessionInfo.ActiveUser);

            result.errorCode = nRet;
            result.errorInfo = error;
            return result;

            ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
        }
        
        // 保存选择的馆藏地
        [HttpPost]
        public ApiResult SaveLocation(string bindUserId, string locations)
        {
            ApiResult result = new ApiResult();

            string error = "";

            // 获取session信息
            if (HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo] == null)
            {
                error = "session失效。";
                goto ERROR1;
            }

            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo];
            if (sessionInfo.ActiveUser == null)
            {
                error = "session中没有活动帐号，不可能的情况";
                goto ERROR1;
            }

            if (sessionInfo.ActiveUser.id != bindUserId)
            {
                error = "传来的user对象id与当前活动对象的id不一致。";
                goto ERROR1;
            }

            sessionInfo.ActiveUser.selLocation = locations;
            WxUserDatabase.Current.Update(sessionInfo.ActiveUser);

            return result;

            ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
        }



        // 保存是否校验条码
        [HttpPost]
        public ApiResult SaveVerifyBarcode(string bindUserId, int verifyBarcode)
        {
            ApiResult result = new ApiResult();
            string error = "";

            // 获取session信息
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

            if (sessionInfo.ActiveUser == null)
            {
                error = "session中没有活动帐号，不可能的情况";
                goto ERROR1;
            }

            if (sessionInfo.ActiveUser.id != bindUserId)
            {
                error = "传来的user对象id与当前活动对象的id不一致。";
                goto ERROR1;
            }

            sessionInfo.ActiveUser.verifyBarcode = verifyBarcode;

            WxUserDatabase.Current.Update(sessionInfo.ActiveUser);

            return result;

        ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
        }

        // 保存语音方案
        [HttpPost]
        public ApiResult SaveAudioType(string bindUserId, int audioType)
        {
            
            ApiResult result = new ApiResult();
            string error = "";

            // 获取session信息
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

            if (sessionInfo.ActiveUser == null)
            {
                error = "session中没有活动帐号，不可能的情况";
                goto ERROR1;
            }

            if (sessionInfo.ActiveUser.id != bindUserId)
            {
                error = "传来的user对象id与当前活动对象的id不一致。";
                goto ERROR1;
            }

            sessionInfo.ActiveUser.audioType = audioType;
            WxUserDatabase.Current.Update(sessionInfo.ActiveUser);

            return result;

            ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
        }



        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult Setting(string weixinId, WxUserItem input)
        {
            
            ApiResult result = new ApiResult();
            string error = "";

            // 检查session中的当前帐户是否存在
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

            if (sessionInfo.ActiveUser == null)
            {
                error = "session中没有活动帐号，不可能的情况";
                goto ERROR1;
            }


            // 是否显示图书封面
            sessionInfo.ActiveUser.showCover = input.showCover;
            // 是否显示头像
            sessionInfo.ActiveUser.showPhoto = input.showPhoto;
            // 更新到数据库
            WxUserDatabase.Current.Update(sessionInfo.ActiveUser);



            return result;

            ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
        }
        */
        #endregion

        #region 暂时关闭的一些接口


        // 根据dp2library数据恢复本地绑定库
        // 20220720注：未严格测试，暂时关闭此接口
        /*
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
        */


        // todo 必须先登录，或者传了管理员帐号和密码
        // 获取全部绑定账户，包括读者与工作人员
        [HttpGet]
        public WxUserResult GetAll()
        {
            //dp2WeiXinService.Instance.WriteLog1("WxUserController.Get()开始");

            WxUserResult result = new WxUserResult();
            List<WxUserItem> list = wxUserDb.Get(null,null,null,-1,null,null,false);//.GetUsers();

            // 在绑定的时候已经设置好了，
            //foreach (WxUserItem user in list)
            //{
            //    if (String.IsNullOrEmpty(user.libraryCode) == false)
            //        user.libName = user.libraryCode;
            //}
            //dp2WeiXinService.Instance.WriteLog1("WxUserController.Get()返回");
            result.users = list;
            return result;
        }


        #endregion
    }
}
