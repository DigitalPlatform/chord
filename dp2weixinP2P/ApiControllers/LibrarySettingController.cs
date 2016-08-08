using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class LibrarySettingController : ApiController
    {
        private LibDatabase libDb = LibDatabase.Current;    

        // GET api/<controller>
        public IEnumerable<LibItem> Get()
        {
            List<LibItem> list = libDb.GetLibs();//"*", 0, -1).Result;
            return list;
        }

        // GET api/<controller>/5
        public LibItem Get(string id)
        {
            return libDb.GetLibById(id);
        }

        // POST api/<controller>
        public LibItem Post(LibItem item)
        {
            return libDb.Add(item);
        }

        // PUT api/<controller>/5
        public long Put(string id,LibItem item)
        {
            return libDb.Update(id,item);
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        public ApiResult Delete(string id)
        {
            ApiResult result = new ApiResult();
            // 先检查一下，是否有微信用户绑定了该图书馆
            List<WxUserItem> list = WxUserDatabase.Current.GetByLibId(id);
            if (list != null && list.Count > 0)
            {
                result.errorCode = -1;
                result.errorInfo = "目前存在微信用户绑定了该图书馆的账户，不能删除图书馆。";
                return result;
            }

            //
            libDb.Delete(id);

            return result;
        }
    }
}