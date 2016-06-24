using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class LibBookController : ApiController
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libId"></param>
        /// <param name="msgId"></param>
        /// <param name="style">browse/full</param>
        /// <returns></returns>
        public BookSubjectResult GetSubject(string weixinId, string libId)
        {
            BookSubjectResult result = new BookSubjectResult();

            string strError = "";


            // 检查是否绑定工作人员以及权限，前端根据返回的工作人员账号判断是否可以进入编辑界面
            string userName = "";
            if (string.IsNullOrEmpty(weixinId) == false)
            {
                // 查找当前微信用户绑定的工作人员账号
                WxUserItem user = WxUserDatabase.Current.GetWorker(weixinId, libId);
                // todo 后面可以放开对读者的权限
                if (user != null)
                {
                    // 检索是否有权限 _wx_setbbj
                    string needRight = dp2WeiXinService.C_Right_SetBook;

                    LibItem lib = LibDatabase.Current.GetLibById(libId);
                    if (lib == null)
                    {
                        strError = "未找到id为["+libId+"]的图书馆定义。";
                        goto ERROR1;
                    }

                    int nHasRights = dp2WeiXinService.Instance.CheckRights(lib.capoUserName,
                        user.userName,
                        needRight,
                        out strError);
                    if (nHasRights == -1)
                    {
                        goto ERROR1;
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

            // 获取指定图书的栏目
            List<BookSubjectItem> list = null;
            int nRet = dp2WeiXinService.Instance.GetBookSubject(libId, out list, out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }

            result.list = list;
            result.errorCode = nRet;
            result.errorInfo = strError;

            return result;

        ERROR1:
            result.errorCode = -1;
            result.errorInfo = strError;
            return result;
        }

        public BookMsgResult GetSubjectMsg(string weixinId, string libId, string subject)
        {
            BookMsgResult result = new BookMsgResult();
            string strError = "";


            // 检查是否绑定工作人员以及权限，前端根据返回的工作人员账号判断是否可以进入编辑界面
            string userName = "";
            if (string.IsNullOrEmpty(weixinId) == false)
            {
                // 查找当前微信用户绑定的工作人员账号
                WxUserItem user = WxUserDatabase.Current.GetWorker(weixinId, libId);
                // todo 后面可以放开对读者的权限
                if (user != null)
                {
                    // 检索是否有权限 _wx_setbbj
                    string needRight = dp2WeiXinService.C_Right_SetBook;

                    LibItem lib = LibDatabase.Current.GetLibById(libId);
                    if (lib == null)
                    {
                        strError = "未找到id为[" + libId + "]的图书馆定义。";
                        goto ERROR1;
                    }

                    int nHasRights = dp2WeiXinService.Instance.CheckRights(lib.capoUserName,
                        user.userName,
                        needRight,
                        out strError);
                    if (nHasRights == -1)
                    {
                        goto ERROR1;
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

            // 获取指定栏目的消息
            List<MessageItem> list = null;
            int nRet = dp2WeiXinService.Instance.GetBookMsg(libId,subject, out list, out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }

            result.list = list;
            result.userName = userName;
            result.errorCode = nRet;
            result.errorInfo = strError;

            return result;

        ERROR1:
            result.errorCode = -1;
            result.errorInfo = strError;
            return result;
        }

        // POST api/<controller>
        public MessageResult Post(string msgType, string libId, MessageItem item)
        {
            // 服务器会自动产生id
            //item.id = Guid.NewGuid().ToString();
            return dp2WeiXinService.Instance.CoverMessage(msgType, libId, item, "create");
        }

        // PUT api/<controller>/5
        public MessageResult Put(string msgType, string libId, MessageItem item)
        {
            //return libDb.Update(id, item);

            string test = "";

            return dp2WeiXinService.Instance.CoverMessage(msgType, libId, item, "change");

            //return null;
     }

        // DELETE api/<controller>/5
        [HttpDelete]
        public void Delete(string msgType, string id, string libId)
        {
            MessageItem item = new MessageItem();
            item.id = id;
            //style == delete
            dp2WeiXinService.Instance.CoverMessage(msgType,libId, item, "delete");

            return;
        }
    }
}
