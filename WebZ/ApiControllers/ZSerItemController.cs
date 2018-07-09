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

        // 验证码多长时间过期
        public static TimeSpan TempCodeExpireLength = TimeSpan.FromHours(48);   // TimeSpan.FromMinutes(10);   // 10 分钟

        public int SendVerifyCode(string phone,
            int testMode,
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

            bool bRet = ServerInfo.CheckPhone(phone);
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
            if (HttpContext.Session == null)
            {
                error = "HttpContext.Session";
                return -1;
            }
            HttpContext.Session.SetString(SessionKey_VerifyCode, code);

            // 非测试模式，才发短信
            if (testMode ==0)
            {
                // 发短信 todo
                try
                {
                    int nRet = ServerInfo.Instance.SendVerifyCodeSMS(phone, code, out error);
                    if (nRet == -1)
                        return -1;
                }
                catch (Exception ex)
                {
                    error = "发送短信验证码出错：" + ex.Message;
                    return -1;
                }
            }

            return 0;
        }



        // POST api/<controller>
        [HttpPost]
        public ApiResult Post(string verifyCode,
            string testMode,
            [FromBody]ZServerItem item)
        {
            int nRet = 0;
            string error = "";

            ApiResult result = new ApiResult();
            if (item == null)
            {
                error = "item对象为null";
                goto ERROR1;
            }

            int nTestMode = 0;
            try
            {
                nTestMode = Convert.ToInt32(testMode);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                goto ERROR1;
            }



            //============
            // 验证码相关代码
            if (verifyCode != "201807" && verifyCode != "201808")
            {
                string code = "";

                // 获取session存储的验证码
                if (HttpContext.Session == null)
                {
                    error = "HttpContext.Session为null";
                    goto ERROR1;
                }
                code = HttpContext.Session.GetString(SessionKey_VerifyCode);
                bool bSend = false;
                if (String.IsNullOrEmpty(code) == true)
                {
                    // 重发验证码
                    nRet = this.SendVerifyCode(item.creatorPhone,
                      nTestMode,
                        out code,
                        out error);
                    if (nRet == -1)
                    {
                        goto ERROR1;
                    }
                    bSend = true;
                }

                result.info = code;

                // 前端没有发来验证码的情况
                if (string.IsNullOrEmpty(verifyCode) == true)
                {
                    if (bSend == true)
                    {
                        error = "验证码已发到手机" + item.creatorPhone + "，请输入短信验证码。";
                            //+ "code=" + code;  放在result了
                    }
                    else
                    {
                        error = "请输入已经收到的手机短信验证码。";
                            //+ "code=" + code;  放在result了
                    }

                    // errorCode返回-2
                    result.errorCode = -2;
                    result.errorInfo = error;
                    return result;
                }
                else
                {
                    if (bSend == true)
                    {
                        error = "验证码无效，系统已重新给您手机号" + item.creatorPhone + "发送短信验证码，请输入新的验证码。";
                            //+ "code=" + code;  放在result了
                        goto ERROR1;
                    }
                    else
                    {
                        //传入的验证码与session中的验证码不一致
                        if (verifyCode != code)
                        {
                            error = "验证码不匹配，请重新输入手机短信中的验证码。";
                            //+ "code=" + code;  放在result了
                            goto ERROR1;
                        }
                    }
                }
            }

            // 保存站点信息
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


            ERROR1:

            result.errorCode = -1;
            result.errorInfo = error;
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
