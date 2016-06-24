using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class LibMessageController : ApiController
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libId"></param>
        /// <param name="msgId"></param>
        /// <param name="style">browse/full</param>
        /// <returns></returns>
        public MessageResult Get(string weixinId, 
            string group,
            string libId, 
            string msgId,
            string style)
        {
            MessageResult result = new MessageResult();

            if (group != dp2WeiXinService.C_GroupName_Bb
                && group != dp2WeiXinService.C_GroupName_Book)
            {
                result.errorInfo = "不支持的群" + group;
                result.errorCode = -1;
                return result;
            }

            string strError="";

            // 检查下有无绑定工作人员账号
            string worker = "";
            if (string.IsNullOrEmpty(weixinId) == false)
            {
                // 查找当前微信用户绑定的工作人员账号
                WxUserItem user = WxUserDatabase.Current.GetWorker(weixinId, libId);
                if (user != null)
                {
                    // 检索是否有权限 _wx_setbbj
                    string needRight = "";
                    if (group == dp2WeiXinService.C_GroupName_Bb)
                        needRight = dp2WeiXinService.C_Right_SetBb;
                    else
                        needRight = dp2WeiXinService.C_Right_SetBook;

                    LibItem lib = LibDatabase.Current.GetLibById(libId);
                    if (lib == null)
                    {
                        result.errorInfo = "未找到id为[" + libId + "]的图书馆定义。";
                        result.errorCode = -1;
                        return result;
                    }

                    int nHasRights = dp2WeiXinService.Instance.CheckRights(lib.capoUserName,
                        user.userName,
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
                        worker = user.userName;
                    }
                    else
                    {
                        worker = "";
                    }
                }
            }

            List<MessageItem> list = null;
            int nRet = dp2WeiXinService.Instance.GetMessage(group,
                libId,
                msgId, 
                "",//subject
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

        // POST api/<controller>
        public MessageResult Post(string group, string libId, MessageItem item)
        {
            // 服务器会自动产生id
            //item.id = Guid.NewGuid().ToString();
            return dp2WeiXinService.Instance.CoverMessage(group, libId, item, "create");
        }

        // PUT api/<controller>/5
        public MessageResult Put(string group, string libId, MessageItem item)
        {
            //return libDb.Update(id, item);

            string test = "";

            return dp2WeiXinService.Instance.CoverMessage(group, libId, item, "change");
     }

        // DELETE api/<controller>/5
        [HttpDelete]
        public MessageResult Delete(string group, string libId, string msgId,string userName)
        {
            MessageItem item = new MessageItem();
            item.id = msgId;
            item.creator = userName;
            //style == delete
            return dp2WeiXinService.Instance.CoverMessage(group, libId, item, "delete");
        }
    }
}
