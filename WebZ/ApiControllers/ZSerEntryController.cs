using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebZ.Server;
using WebZ.Server.database;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebZ.ApiControllers
{
    [Route("api/[controller]")]
    public class ZSerEntryController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<ZServerItem> Get(int start,
                int count)
        {
            return  ServerInfo.ZServerDb.Get(start, count).Result;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public ZServerItem Get(string id)
        {
            return ServerInfo.ZServerDb.GetById(id);
        }

        // POST api/<controller>
        [HttpPost]
        public async void Post([FromBody]ZServerItem item)
        {
            await ServerInfo.ZServerDb.Add(item);
        }

        //// PUT api/<controller>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public async void Delete(string id)
        {
            await ServerInfo.ZServerDb.Delete(id);
        }
    }
}
