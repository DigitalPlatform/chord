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
        public ApiResult Get(string word,
                string from,
                int start,
                int count)
        {
            ApiResult result = new ApiResult();
            try
            {

                //if (String.IsNullOrEmpty(resultSet) == true)
                //    resultSet = "webz-" + Guid.NewGuid();

                result.data = ServerInfo.Instance.Search(word,
                    from,
                    start, 
                    count);
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
                result.data = ServerInfo.Instance.GetOneZServer(id);
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
                // 创建者ip地址
                item.creatorIP = Request.HttpContext.Connection.RemoteIpAddress.ToString();

                result.data = ServerInfo.Instance.AddZServerItem(item);
            }
            catch (Exception ex)
            {
                result.errorCode = -1;
                result.errorInfo = ex.Message;
            }

            return result;
        }

        // 一般有管理员审核修改
        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public ApiResult Put(string id,[FromBody]ZServerItem item)
        {
            ApiResult result = new ApiResult();
            try
            {

                result.data = ServerInfo.Instance.UpdateZServerItem(item);
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
                ServerInfo.Instance.DeleteZSererItem(id);
            }
            catch (Exception ex)
            {
                result.errorCode = -1;
                result.errorInfo = ex.Message;
            }
            return result;
        }


        //// GET: api/<controller>
        //[HttpGet]
        //public ApiResult GetVerifyCodeSMS(string phone)
        //{
        //    ApiResult result = new ApiResult();
        //    try
        //    {
        //        string error = "";
        //        int nRet= ServerInfo.Instance.SendVerifyCodeSMS(phone,out error);
        //        if (nRet == -1)
        //        {
        //            result.errorInfo = error;
        //            result.errorCode = -1;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.errorInfo = ex.Message;
        //        result.errorCode = -1;
        //    }

        //    return result;
        //}
    }
}
