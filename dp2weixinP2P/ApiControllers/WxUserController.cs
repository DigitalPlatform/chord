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
    public class WxUserController : ApiController
    {
        private WxUserDatabase wxUserDb = WxUserDatabase.Current;

        // 获取全部绑定账户，包括读者与工作人员
        [HttpGet]
        public WxUserResult Get()
        {
            //dp2WeiXinService.Instance.WriteLog1("WxUserController.Get()开始");
            WxUserResult result = new WxUserResult();
            List<WxUserItem> list = wxUserDb.Get(null,null,-1,null,null,false);//.GetUsers();

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

        [HttpGet]
        public WxUserResult GetByLibId(string libId,string type)
        {
            WxUserResult result = new WxUserResult();
            // 获取绑定的读者数量
            List<WxUserItem> users = new List<WxUserItem>();

            if (type=="-1")
                users= WxUserDatabase.Current.Get("", libId,-1);
            else if (type =="0")
                users= WxUserDatabase.Current.Get("", libId,WxUserDatabase.C_Type_Patron);
            else if (type=="1")
                users = WxUserDatabase.Current.Get("", libId, WxUserDatabase.C_Type_Worker);


            result.users = users;

            return result;
        }

        public WxUserResult Get(string weixinId)
        {
            //dp2WeiXinService.Instance.WriteLog1("WxUserController.Get(string weixinId)开始");

            WxUserResult result = new WxUserResult();
            List<WxUserItem> list = wxUserDb.Get(weixinId, null, -1);
            foreach (WxUserItem user in list)
            {
                user.xml = "";
            }

            result.users = list;


            //// 测试，只返回第一个
            //if (list.Count > 0)
            //{
            //    result.users = new List<WxUserItem>();
            //    result.users.Add(list[0]);
            //    dp2WeiXinService.Instance.WriteLog1("第一个对象 "+list[0].Dump());
            //}



            //dp2WeiXinService.Instance.WriteLog1("WxUserController.Get(string weixinId)结束");

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

            string error = "";

            // 获取session信息
            if (HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo] == null)
            {
                error = "session失效。";
                goto ERROR1;
            }

            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo];
            if (sessionInfo.Active == null)
            {
                error = "session中没有活动帐号，不可能的情况";
                goto ERROR1;
            }

            if (sessionInfo.Active.id != workerId)
            {
                error = "传来的user对象id与当前活动对象的id不一致。";
                goto ERROR1;
            }

            // 更新数据库设置
            sessionInfo.Active.tracing = tracing;
            int nRet = (int)WxUserDatabase.Current.Update(sessionInfo.Active);//.UpdateTracing(workerId, tracing);


            // 更新内存
            dp2WeiXinService.Instance.UpdateTracingUser(sessionInfo.Active);//.UpdateMemoryTracingUser(sessionInfo.Active.weixinId, 
                //sessionInfo.Active.libId,
                //tracing);

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
        public ApiResult SaveLocation(string userId, string locations)
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
            if (sessionInfo.Active == null)
            {
                error = "session中没有活动帐号，不可能的情况";
                goto ERROR1;
            }

            sessionInfo.Active.selLocation = locations;
            WxUserDatabase.Current.Update(sessionInfo.Active);

            return result;

            ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
        }

        // 保存是否校验条码
        [HttpPost]
        public ApiResult SaveVerifyBarcode(string userId, int verifyBarcode)
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

            if (sessionInfo.Active == null)
            {
                error = "session中没有活动帐号，不可能的情况";
                goto ERROR1;
            }

            sessionInfo.Active.verifyBarcode = verifyBarcode;

            WxUserDatabase.Current.Update(sessionInfo.Active);

            return result;

            ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
        }

        // 保存语音方案
        [HttpPost]
        public ApiResult SaveAudioType(string userId, int audioType)
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

            if (sessionInfo.Active == null)
            {
                error = "session中没有活动帐号，不可能的情况";
                goto ERROR1;
            }

            sessionInfo.Active.audioType = audioType;

            WxUserDatabase.Current.Update(sessionInfo.Active);

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

            // 更新session信息???
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

            if (sessionInfo.Active == null)
            {
                error = "session中没有活动帐号，不可能的情况";
                goto ERROR1;
            }


            //sessionInfo.Active.libraryCode = input.libraryCode;
            sessionInfo.Active.showCover = input.showCover;
            sessionInfo.Active.showPhoto = input.showPhoto;

            WxUserDatabase.Current.Update(sessionInfo.Active);



            return result;

            ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
        }
        

        
        // 设置微信用户当前图书馆
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

            //如果选择的图书馆就是是当前活动帐户对应的图书馆，则不用处理
            if (sessionInfo.Active != null
                && sessionInfo.Active.libId == libId
                && sessionInfo.Active.bindLibraryCode == bindLibraryCode)
            {
                return result;
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
            if (user == null) // 绑public身份创建一个帐号
            {
                // 先看看有没有public的,有的话，用绑定的实际帐号替换
                //注意这里不过滤图书馆，就是说临时选择的图书馆，如果未绑定正式帐户，则会被新选择的图书馆public帐户代替
                List<WxUserItem> publicList = WxUserDatabase.Current.GetWorkers(weixinId, "", "public");
                if (publicList.Count > 0)
                {
                    user = publicList[0];
                    if (publicList.Count > 1)
                    {
                        dp2WeiXinService.Instance.WriteLog1("!!!异常：出现" + publicList.Count + "个public帐户?应该只有一个，把多余的帐户删除");
                        for (int i = 1; i < publicList.Count; i++)
                        {
                            WxUserDatabase.Current.SimpleDelete(publicList[i].id);
                        }
                    }

                    user.libId = libId;
                    Library lib = dp2WeiXinService.Instance.LibManager.GetLibrary(libId);
                    if (lib == null)
                    {
                        error = "未找到id="+libId+"对应的图书馆";
                        goto ERROR1;
                    }
                    user.libName = lib.Entity.libName;
                    user.bindLibraryCode = bindLibraryCode;
                    if (string.IsNullOrEmpty(user.bindLibraryCode) == false)
                        user.libName = user.bindLibraryCode;

                    user.libraryCode = "";
                    WxUserDatabase.Current.Update(user);
                }
                else
                {
                    user = WxUserDatabase.Current.CreatePublic(weixinId, libId,bindLibraryCode);
                }
            }

            // 设为当前帐户
            WxUserDatabase.Current.SetActivePatron1(user.weixinId, user.id);

            // 初始化sesson
            int nRet = sessionInfo.Init1(user.weixinId, out error);
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
            string error = "";

            dp2WeiXinService.Instance.WriteLog1("!!!走进bind API");

            if (item.bindLibraryCode == null)
                item.bindLibraryCode = "";

            // 返回对象
            WxUserResult result = new WxUserResult();

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
                goto ERROR1;
            }
            result.users = new List<WxUserItem>();
            result.users.Add(userItem);

            if (sessionInfo.Active != null)
            {
                dp2WeiXinService.Instance.WriteLog1("原来session中的user对象id=["+sessionInfo.Active.id+"],weixinid=["+sessionInfo.WeixinId+"]");
            }
            else
            {
                dp2WeiXinService.Instance.WriteLog1("原来session中无user对象");
            }

            dp2WeiXinService.Instance.WriteUserInfo(item.weixinId, "bind返回后");


            //更新session信息
            nRet = sessionInfo.Init1(item.weixinId, out error);
            if (nRet == -1)
                goto ERROR1;

            dp2WeiXinService.Instance.WriteUserInfo(item.weixinId, "session.init后");

            return result;

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

            if (id == "null")
                id = "";

            WxUserItem user = wxUserDb.GetById(id);
            if (user == null)
            {
                error = "未找到" + id + "对应的绑定用户";
                goto ERROR1;
            }

            //设为活动账户
            WxUserDatabase.Current.SetActivePatron1(user.weixinId, user.id);




            //更新session

            int nRet = sessionInfo.Init1(weixinId,out error);
            if (nRet == -1)
                goto ERROR1;


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

            nRet = sessionInfo.Init1(sessionInfo.WeixinId, out error);
            if (nRet == -1)
                goto ERROR1;


            //===================

            return result;


            ERROR1:
            result.errorCode = -1;
            result.errorInfo = error;
            return result;
        }



    }
}
