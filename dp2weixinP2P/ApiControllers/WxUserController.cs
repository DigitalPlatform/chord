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

            // 前端有时传上来是这个值
            if (item.prefix == "null")
                item.prefix = "";

            WxUserItem userItem = null;
            string strError="";
            //string fullWord = item.word;
            //if (string.IsNullOrEmpty(item.prefix) == false && item.prefix != "null")
            //    fullWord = item.prefix + ":" + item.word;
            int nRet= dp2WeiXinService.Instance.Bind(item.libUserName,
                item.libCode,
                item.prefix,
                item.word,
                item.password,
                item.weixinId,
                out userItem,
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
            int nRet = dp2WeiXinService.Instance.ResetPassword(libUserName,
                libCode,
                name,
                tel,
                out strError);
            result.errorCode = nRet;
            result.errorInfo = strError;

            return result;
        }

        // 修改密码
        [HttpPost]
        public ApiResult ChangePassword(string libUserName,
            string patron,
            string oldPassword,
            string newPassword)
        {
            ApiResult result = new ApiResult();

            string strError = "";
            int nRet = dp2WeiXinService.Instance.ChangePassword(libUserName,
                patron,
                oldPassword,
                newPassword,
                out strError);
            result.errorCode = nRet;
            result.errorInfo = strError;

            return result;
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
