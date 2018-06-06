using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalPlatform.IO;
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

                //ZServerItem item = new ZServerItem();
                //item.id= Guid.NewGuid().ToString();
                //item.port = "210";
                //item.hostName = "测试";
                //item.creatorPhone = "123";
                //item.createTime = DateTime.Now.ToString();
                //list.Add(item);

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
        public ApiResult Get(string id)
        {
            ApiResult result = new ApiResult();
            try
            {
                result.data = ServerInfo.ZServerDb.GetById(id).Result;
            }
            catch (Exception ex)
            {
                result.errorCode = -1;
                result.errorInfo = ex.Message;
            }

            return result;
        }

        // POST api/<controller>
        [HttpPost]
        public ApiResult Post([FromBody]ZServerItem item)
        {
            ApiResult result = new ApiResult();
            if (item == null)
            {
                result.errorCode = -1;
                result.errorInfo = "item对象为null";
            }
            try
            {
                item.creatorIP = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                result.data = ServerInfo.ZServerDb.Add(item).Result;
            }
            catch (Exception ex)
            {
                result.errorCode = -1;
                result.errorInfo = ex.Message;
            }

            return result;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public ApiResult Put(string id,[FromBody]ZServerItem item)
        {
            ApiResult result = new ApiResult();
            try
            {
                item.creatorIP = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                item.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);


                result.data = ServerInfo.ZServerDb.Update(item).Result;
            }
            catch (Exception ex)
            {
                result.errorCode = -1;
                result.errorInfo = ex.Message;
            }
            return result;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public ApiResult Delete(string id)
        {
            ApiResult result = new ApiResult();
            try
            {
                string[] ids = id.Split(new char[] { ',' });
                foreach (string one in ids)
                {
                    ServerInfo.ZServerDb.Delete(one).Wait();
                }
            }
            catch (Exception ex)
            {
                result.errorCode = -1;
                result.errorInfo = ex.Message;
            }
            return result;
        }
    }
}
