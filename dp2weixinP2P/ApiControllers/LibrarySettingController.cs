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
        public void Delete(string id)
        {
            libDb.Delete(id);
        }
    }
}