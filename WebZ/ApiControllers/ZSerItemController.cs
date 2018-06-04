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
    public class ZSerItemController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public ApiResult Get(int start,
                int count)
        {
            ApiResult result = new ApiResult();
            try
            {
                List<ZServerItem> list = ServerInfo.ZServerDb.Get(start, count).Result;

                ZServerItem item = new ZServerItem();
                item.itemId = "111";//Guid.NewGuid().ToString();
                item.port = "210";
                item.hostName = "测试";
                item.creatorPhone = "123";
                item.createTime = DateTime.Now.ToString();
                list.Add(item);


                item = new ZServerItem();
                item.itemId = "222";//Guid.NewGuid().ToString();
                item.port = "210";
                item.hostName = "测试2";
                item.creatorPhone = "123";
                item.createTime = DateTime.Now.ToString();
                list.Add(item);


                result.data = list;
            }
            catch (Exception ex)
            {
                result.errorInfo = ex.Message;
                result.errorCode = -1;
            }

            return result;
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
