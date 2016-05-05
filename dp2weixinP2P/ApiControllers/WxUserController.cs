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

        /*
        // GET api/<controller>/5
        public WxUserItem Get(string id)
        {
            return repo.GetOneByWeixinId(id);
        }
         */

        // POST api/<controller>
        public WxUserItem Post(WxUserItem item)
        {
            string test = "";

            return item;// repo.Add(item);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public void ActivePatron(string weixinId,string id)
        {
             repo.SetActive(weixinId,id);// repo.Update(item);
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        public void Delete(string id)
        {
            repo.Delete(id);
        }
    }
}
