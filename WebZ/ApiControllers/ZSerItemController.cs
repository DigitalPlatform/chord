using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalPlatform.IO;
using Microsoft.AspNetCore.Http;
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

        // 验证码存在session中的key
        public const string SessionKey_VerifyCode = "_verifycode";


        public void SendVerifyCode()
        {
            // 生成验证码

            //  记在session里
            HttpContext.Session.SetString(SessionKey_VerifyCode, "test");

            // 发短信
        }
        // POST api/<controller>
        [HttpPost]
        public ApiResult Post(string verifyCode,[FromBody]ZServerItem item)
        {
            ApiResult result = new ApiResult();
            if (item == null)
            {
                result.errorCode = -1;
                result.errorInfo = "item对象为null";
                return result;
            }

            //============
            // 验证码相关代码
            if (string.IsNullOrEmpty(verifyCode) == true)
            {
                // 检查session中是否已经存在验证码
                // 如果不存在，发短信，且把验证码保存在session里
                if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKey_VerifyCode)))
                {

                    this.SendVerifyCode();
                }

                result.errorCode = -1;
                result.errorInfo = "尚未输入短信验证码。";
                return result;
            }

            // 
            string myverifycode = HttpContext.Session.GetString(SessionKey_VerifyCode);
            if (String.IsNullOrEmpty(myverifycode) == true)
            {
                // 重发验证码
                this.SendVerifyCode();

                result.errorCode = -1;
                result.errorInfo = "验证码失效，已重发验证码，请使用手机短信中新的验证码提交。";
                return result;
            }

            // 比如传入的验证码与session中的验证码是否一致
            if (verifyCode != myverifycode)
            {
                result.errorCode = -1;
                result.errorInfo = "验证码不匹配，请重新输入手机短信中验证码。";
                return result;
            }
            //===================

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
        public ApiResult Put(string id, string verifyCode, [FromBody]ZServerItem item)
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
