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
        public MessageResult Get(string weixinId, string msgType,
            string libId, 
            string msgId,
            string style)
        {
            MessageResult result = new MessageResult();

            string strError = "";
            List<MessageItem> list = null;
            string worker = "";
            int nRet = dp2WeiXinService.Instance.GetMessage(weixinId,
                msgType,
                libId,
                msgId, 
                style,
                out list,
                out worker,
                out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }

            result.items = list;
            result.worker = worker;
            result.errorCode = nRet;
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
