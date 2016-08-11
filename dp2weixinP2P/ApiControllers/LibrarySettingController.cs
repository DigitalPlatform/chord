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
        public LibSetResult Post(LibItem item)
        {
            LibSetResult result = new LibSetResult();
            string strError = "";
            LibItem outputItem = null;
            int nRet= dp2WeiXinService.Instance.AddLib(item,out outputItem, out strError);// libDb.Add(item);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }
            result.libItem = outputItem;
            return result;

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
            return dp2WeiXinService.Instance.deleteLib(id);
        }
    }
}