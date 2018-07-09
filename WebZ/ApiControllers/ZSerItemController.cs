using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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


        public int SendVerifyCode(string phone,
            out string code,
            out string error)
        {
            code = "";
            error = "";
            if (string.IsNullOrEmpty(phone) == true)
            {
                error = "手机号不能为空";
                return -1;
            }

            bool bRet = this.CheckPhoneIsAble(phone);
            if (bRet == false)
            {
                error = "手机号格式不正确";
                return -1;
            }



            // 生成验证码
            // 重新设定一个密码
            Random rnd = new Random();
            code= rnd.Next(1, 999999).ToString(); 
            //TempCode code = new TempCode();
            //code.Key = "";
            //code.Code = rnd.Next(1, 999999).ToString();
            //code.ExpireTime = DateTime.Now + TempCodeExpireLength;

            //  记在session里
            HttpContext.Session.SetString(SessionKey_VerifyCode, code);

            // 发短信 todo
            try
            {
                int nRet = ServerInfo.Instance.SendVerifyCodeSMS(phone, code, out error);
                if (nRet == -1)
                    return -1;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return -1;
            }

            return 0;
        }

        // 验证码多长时间过期
        public static TimeSpan TempCodeExpireLength = TimeSpan.FromHours(48);   // TimeSpan.FromMinutes(10);   // 10 分钟


        public bool IsTelephone(string phone)
        {
            return Regex.IsMatch(phone, @"^(\d{3,4}-)?\d{6,8}$");
        }

        private bool CheckPhoneIsAble(string input)
        {
            if (input.Length < 11)
            {
                return false;
            }
            //电信手机号码正则
            string dianxin = @"^1[3578][01379]\d{8}$";
            Regex regexDX = new Regex(dianxin);
            //联通手机号码正则
            string liantong = @"^1[34578][01256]\d{8}";
            Regex regexLT = new Regex(liantong);
            //移动手机号码正则
            string yidong = @"^(1[012345678]\d{8}|1[345678][012356789]\d{8})$";
            Regex regexYD = new Regex(yidong);
            if (regexDX.IsMatch(input) || regexLT.IsMatch(input) || regexYD.IsMatch(input))
            {
                return true;
            }
            else
            {
                return false;
            }
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
            int nRet = 0;
            string error = "";
            string code = "";
            code = HttpContext.Session.GetString(SessionKey_VerifyCode);

            if (string.IsNullOrEmpty(verifyCode) == true)
            {
                string info = "";

                // 检查session中是否已经存在验证码
                // 如果不存在，发短信，且把验证码保存在session里
                if (string.IsNullOrEmpty(code)==true)
                {
                    nRet = this.SendVerifyCode(item.creatorPhone,
                        out code,
                        out error);
                    if (nRet == -1)
                    {
                        result.errorCode = -1;
                        result.errorInfo = error;
                        return result;
                    }
                    info = "验证码已发到手机"+item.creatorPhone+ "，请输入短信验证码，重新提交。"
                        +"code="+code;
                }
                else
                {
                    info = "请输入已经收到的手机短信验证，重新提交。"
                        + "code=" + code; 
                }

                result.errorCode = -2;
                result.errorInfo = "参数中缺少验证码。"+info;
                return result;
            }

            if (String.IsNullOrEmpty(code) == true)
            {
                // 重发验证码
                nRet = this.SendVerifyCode(item.creatorPhone,
                    out code,
                    out error);
                if (nRet == -1)
                {
                    result.errorCode = -1;
                    result.errorInfo = error;
                    return result;
                }

                result.errorCode = -1;
                result.errorInfo = "系统已给您手机号"+item.creatorPhone+"发送短信验证码，请输入验证码，重新提交。"
                    + "code=" + code;
                return result;
            }

            // 比如传入的验证码与session中的验证码是否一致
            if (verifyCode != code)
            {
                result.errorCode = -1;
                result.errorInfo = "验证码不匹配，请重新输入手机短信中验证码。"
                    + "code=" + code;
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
