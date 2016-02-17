using dp2Command.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dp2weixinP2P.ApiControllers
{
    public class LibraryController : ApiController
    {
        //private LibraryRespository repo = LibraryRespository.Current;
        private LibDatabase repo = LibDatabase.Current;
            




        // GET api/<controller>
        public IEnumerable<LibItem> Get()
        {
            List<LibItem> list = repo.GetLibs();//"*", 0, -1).Result;
            return list;
        }

        // GET api/<controller>/5
        public LibItem Get(string id)
        {
            return repo.GetLibById(id);
        }

        // POST api/<controller>
        public LibItem Post(LibItem item)
        {
            return repo.Add(item);
        }

        // PUT api/<controller>/5
        public long Put(LibItem item)
        {
            return repo.Update(item).Result;
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        public void Delete(string id)
        {
            repo.Delete(id);
        }
    }
}