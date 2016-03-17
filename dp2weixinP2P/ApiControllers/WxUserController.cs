using dp2Command.Service;
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
        //private LibraryRespository repo = LibraryRespository.Current;
        private WxUserDatabase repo = WxUserDatabase.Current;

        // GET api/<controller>
        public IEnumerable<WxUserItem> Get()
        {
            List<WxUserItem> list = repo.GetUsers();//"*", 0, -1).Result;
            return list;
        }

        // GET api/<controller>/5
        public WxUserItem Get(string id)
        {
            return repo.GetOneByWeixinId(id);
        }

        // POST api/<controller>
        public WxUserItem Post(WxUserItem item)
        {
            return repo.Add(item);
        }

        // PUT api/<controller>/5
        public long Put(WxUserItem item)
        {
            return repo.Update(item);
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        public void Delete(string id)
        {
            repo.Delete(id);
        }
    }
}
