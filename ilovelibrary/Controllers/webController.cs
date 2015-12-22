using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ilovelibrary.Controllers
{
    public class webController : ApiController
    {
        // GET: api/web
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/web/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/web
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/web/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/web/5
        public void Delete(int id)
        {
        }
    }
}
