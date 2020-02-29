using DigitalPlatform.IO;
using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web;

namespace dp2weixinWeb.ApiControllers
{
    public class LibMessageController : ApiController
    {
        public ApiResult GetTemplate(string group,
            string libId,
            string subject)
        {
            ApiResult result = new ApiResult();
            //result.info = "test";


            LibEntity lib = dp2WeiXinService.Instance.GetLibById(libId);
            if (lib == null)
            {
                result.errorCode = -1;
                result.errorInfo = "未找到id为'"+libId+"'的图书馆";
                return result;
            }


            string file = dp2WeiXinService.Instance._weiXinDataDir 
                + "/lib/" + "template"//lib.capoUserName 
                + "/home/" 
                + subject+".txt";

                // 文件存在，取出文件 的内容
            string text = "";
            string strError = "";
            if (System.IO.File.Exists(file) == true)
            {
                Encoding encoding;
                // 能自动识别文件内容的编码方式的读入文本文件内容模块
                // parameters:
                //      lMaxLength  装入的最大长度。如果超过，则超过的部分不装入。如果为-1，表示不限制装入长度
                // return:
                //      -1  出错 strError中有返回值
                //      0   文件不存在 strError中有返回值
                //      1   文件存在
                //      2   读入的内容不是全部
                int nRet = FileUtil.ReadTextFileContent(file,
                    -1,
                    out text,
                    out encoding,
                    out strError);
                if (nRet == -1 || nRet == 0)
                {
                    goto ERROR1;
                }
                if (nRet == 2)
                {
                    strError="FileUtil.ReadTextFileContent() error";
                    goto ERROR1;
                }

                result.info = text;
            }


            return result;

        ERROR1:
            result.errorInfo = strError;
        result.errorCode = -1;
        return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libId"></param>
        /// <param name="msgId"></param>
        /// <param name="style">browse/full</param>
        /// <returns></returns>
        public SubjectResult GetSubject(string weixinId, 
            string group,
            string libId,
            string selSubject,
            string param)
        {
            SubjectResult result = new SubjectResult();
            string strError = "";

            // 获取栏目
            List<SubjectItem> list = null;
            int nRet = dp2WeiXinService.Instance.GetSubject(libId, group,
                out list, out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }
            if (param.Contains("list")==true)
                result.list = list;

            if (param.Contains("html") == true)
            {
                string html = dp2WeiXinService.Instance.GetSubjectHtml(libId,
                    group,
                   selSubject,
                   true,
                   list);
                result.html = html;
            }

            result.errorCode = nRet;
            result.errorInfo = strError;

            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libId"></param>
        /// <param name="msgId"></param>
        /// <param name="style">browse/full</param>
        /// <returns></returns>
        public WxMessageResult GetMessage(string weixinId, 
            string group,
            string libId, 
            string msgId,
            string subject,
            string style)
        {
            WxMessageResult result = new WxMessageResult();
            string strError = "";

            strError = dp2WeiXinService.checkGroup(group);
            if (strError !="")
            {
                result.errorInfo = strError;
                result.errorCode = -1;
                return result;
            }

            if (HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo] == null)
            {
                result.errorInfo = "session失效。";
                result.errorCode = -1;
                return result;
            }
            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo];
            if (sessionInfo == null)
            {
                result.errorInfo = "session失效2。";
                result.errorCode = -1;
                return result;
            }

            // 检查下有无绑定工作人员账号
            result.userName = "";
            string userName = "";
            string libraryCode= "";
            if (string.IsNullOrEmpty(weixinId) == false)
            {
                // 查找当前微信用户绑定的工作人员账号

                WxUserItem user = sessionInfo.ActiveUser; //WxUserDatabase.Current.GetWorker(weixinId, libId);
                if (user != null)
                {
                    libraryCode = user.bindLibraryCode;
                    // 检索是否有权限 _wx_setbbj
                    string needRight =dp2WeiXinService.GetNeedRight(group);

                    LibEntity lib = dp2WeiXinService.Instance.GetLibById(libId);
                    if (lib == null)
                    {
                        result.errorInfo = "未找到id为[" + libId + "]的图书馆定义。";
                        result.errorCode = -1;
                        return result;
                    }

                    int nHasRights = dp2WeiXinService.Instance.CheckRights(user,
                        lib,
                        needRight,
                        out strError);
                    if (nHasRights == -1)
                    {
                        result.errorInfo = strError;
                        result.errorCode = -1;
                        return result;
                    }
                    if (nHasRights == 1)
                    {
                        userName = user.userName;
                    }
                    else
                    {
                        userName = "";
                    }
                }
            }
            result.userName = userName;

            List<MessageItem> list = null;
            int nRet = dp2WeiXinService.Instance.GetMessage(group,
                libId+"/"+libraryCode,
                msgId, 
                subject,
                style,
                out list,
                out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }
            result.items = list;
            
            result.errorCode = nRet;
            result.errorInfo = strError;

            return result;
        }

        // 新增消息
        public WxMessageResult Post(string weixinId, 
            string group, 
            string libId, 
            string parameters, 
            MessageItem item)
        {

            //// 更新setting
            //if (string.IsNullOrEmpty(weixinId) == false && group == "gn:_lib_book")
            //{
            //    dp2WeiXinService.Instance.UpdateUserSetting(weixinId, 
            //        libId,
            //        null, 
            //        item.subject,
            //        false,
            //        null);
            //}


            // 服务器会自动产生id
            return this.CoverMessage(weixinId,
                group,
                libId,
                item,
                "create",
                parameters );
        }

        public WxMessageResult CoverMessage(string weixinId,
    string group,
    string libId,
    MessageItem item,
    string style,
    string parameters)
        {

            WxMessageResult result = new WxMessageResult();
            string strError = "";

            if (HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo] == null)
            {
                result.errorInfo = "session失效。";
                result.errorCode = -1;
                return result;
            }
            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo];
            if (sessionInfo == null)
            {
                result.errorInfo = "session失效2。";
                result.errorCode = -1;
                return result;
            }

            MessageItem returnItem = null;
            int nRet = dp2WeiXinService.Instance.CoverMessage(sessionInfo.ActiveUser,
                weixinId,
                group,
                libId,
                item,
                style,
                parameters,
                out returnItem,
                out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
                return result;
            }

            List<MessageItem> list = new List<MessageItem>();
            list.Add(returnItem);
            result.items = list;
            return result;
        }

        // 修改消息
        public WxMessageResult Put(string weixinId,
            string group,
            string libId,
            MessageItem item)
        {
            //// 更新setting
            //if (string.IsNullOrEmpty(weixinId) == false && group == "gn:_lib_book")
            //{
            //    dp2WeiXinService.Instance.UpdateUserSetting(weixinId,
            //        libId,
            //        null, 
            //        item.subject,
            //        false,
            //        null);
            //}
            return this.CoverMessage(weixinId,
                group, libId, item, "change", "");
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        public WxMessageResult Delete(string weixinId, 
            string group, string libId, string msgId, string userName)
        {
            WxMessageResult result = null;
            string[] ids = msgId.Split(new char[] { ','});
            foreach (string id in ids)
            {
                MessageItem item = new MessageItem();
                item.id = id;
                item.creator = userName;
                //style == delete
                result = this.CoverMessage(weixinId, group, libId, item, "delete", "");
                if (result.errorCode == -1)
                    return result;
            }
            return result;
        }
    }
}
