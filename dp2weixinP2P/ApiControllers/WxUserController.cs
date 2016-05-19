using dp2Command.Service;
using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dp2weixinP2P.ApiControllers
{
    public class WxUserController : ApiController
    {
        private WxUserDatabase repo = WxUserDatabase.Current;

        // GET api/<controller>
        public IEnumerable<WxUserItem> Get()
        {
            List<WxUserItem> list = repo.GetUsers();//"*", 0, -1).Result;
            return list;
        }

        // GET api/<controller>
        public IEnumerable<WxUserItem> Get(string weixinId)
        {
            List<WxUserItem> list = repo.GetByWeixinId(weixinId);//.GetUsers();//"*", 0, -1).Result;
            return list;
        }



        // POST api/<controller>
        [HttpPost]
        public WxUserResult Bind(WxUserItem item)
        {
            // 返回对象
            WxUserResult result = new WxUserResult();
            result.userItem = null;
            result.apiResult = new ApiResult();

            WxUserItem userItem = null;
            string readerBarcode="";
            string strError="";
            string fullWord = item.word;
            if (string.IsNullOrEmpty(item.prefix) == false && item.prefix != "null")
                fullWord = item.prefix + ":" + item.word;
            int nRet= dp2CmdService2.Instance.Bind(item.libUserName,
                item.libCode,
                fullWord,
                item.password,
                item.weixinId,
                out userItem,
                out readerBarcode,
                out strError);
            if (nRet == -1)
            {
                result.apiResult.errorCode = -1;
                result.apiResult.errorInfo = strError;
            }
            result.userItem = userItem;

            return result;// repo.Add(item);
        }


        // POST api/<controller>
        [HttpPost]
        public ApiResult ResetPassword(string libUserName,
            string libCode,
            string name,string tel)
        {
            ApiResult result = new ApiResult();
            
            string strError="";
            int nRet = dp2CmdService2.Instance.ResetPassword(libUserName,
                libCode,
                name,
                tel,
                out strError);
            result.errorCode = nRet;
            result.errorInfo = strError;

            return result;// repo.Add(item);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public void ActivePatron(string weixinId,string id)
        {
             repo.SetActive(weixinId,id);// repo.Update(item);
        }



        // DELETE api/<controller>/5
        [HttpDelete]
        public ApiResult Delete(string id)
        {
            ApiResult result = new ApiResult();
            string strError = "";
            int nRet = dp2CmdService2.Instance.Unbind(id, out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }

            return result;
        }



    }
}
