/*----------------------------------------------------------------
    Copyright (C) 2016 Senparc
    
    文件名：WeixinController.cs
    文件功能描述：用于处理微信回调的信息
    
    
    创建标识：Senparc - 20150312
----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Xml.Linq;
using Senparc.Weixin.MP.Entities.Request;

using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Helpers;
//using Senparc.Weixin.MP.MvcExtension;
//using Senparc.Weixin.MP.Sample.Service;
//using Senparc.Weixin.MP.Sample.CustomerMessageHandler;
using Senparc.Weixin.MP.Sample.CommonService;
using Senparc.Weixin.MP.Sample.CommonService.CustomMessageHandler;
using Senparc.Weixin.MP.MvcExtension;
using Senparc.Weixin.MP;
using dp2weixin;
using dp2Command.Service;
using dp2weixin.service;

namespace dp2weixinWeb.Controllers
{


    public partial class WeixinController : Controller
    {

        readonly Func<string> _getRandomFileName = () => DateTime.Now.Ticks + Guid.NewGuid().ToString("n").Substring(0, 6);

        public WeixinController()
        {

        }

        /// <summary>
        /// 微信后台验证地址（使用Get），微信后台的“接口配置信息”的Url填写如：http://weixin.senparc.com/weixin
        /// </summary>
        [HttpGet]
        [ActionName("Index")]
        public ActionResult Get(PostModel postModel, string echostr)
        {
            GzhCfg gzh = dp2WeiXinService.Instance.gzhContainer.GetDefault();
            if (string.IsNullOrEmpty(postModel.AppId) == false)
            {
                gzh = dp2WeiXinService.Instance.gzhContainer.GetByAppId(postModel.AppId);
                dp2WeiXinService.Instance.WriteErrorLog1("惊喜Get中有appId=" + postModel.AppId);

            }
            //GzhCfg gzh = dp2WeiXinService.Instance.gzhContainer.GetByAppId(postModel.AppId);
            if (gzh == null)
            {
                string tempError = "Get未找到公众号[" + postModel.AppId + "]的配置信息。";
                dp2WeiXinService.Instance.WriteErrorLog1(tempError);
                return new WeixinResult(tempError);
            }

            if (CheckSignature.Check(postModel.Signature, 
                postModel.Timestamp, 
                postModel.Nonce, 
                gzh.token))
            {
                return Content(echostr); //返回随机字符串则表示验证通过
            }
            else
            {
                return Content("failed:" + postModel.Signature + "," + CheckSignature.GetSignature(postModel.Timestamp, postModel.Nonce, gzh.token) + "。" +
                    "如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。");
            }
        }

        /// <summary>
        /// 用户发送消息后，微信平台自动Post一个请求到这里，并等待响应XML。
        /// PS：此方法为简化方法，效果与OldPost一致。
        /// v0.8之后的版本可以结合Senparc.Weixin.MP.MvcExtension扩展包，使用WeixinResult，见MiniPost方法。
        /// </summary>
        [HttpPost]
        [ActionName("Index")]
        public ActionResult Post(PostModel postModel)
        {
            GzhCfg gzh=dp2WeiXinService.Instance.gzhContainer.GetDefault();
            if (string.IsNullOrEmpty(postModel.AppId) == false)
            { 
                gzh = dp2WeiXinService.Instance.gzhContainer.GetByAppId(postModel.AppId);
                dp2WeiXinService.Instance.WriteErrorLog1("惊喜post中有appId=" + postModel.AppId);
            }

            if (gzh == null)
            {
                string tempError = "Post未找到公众号[" + postModel.AppId + "]的配置信息。";
                dp2WeiXinService.Instance.WriteErrorLog1(tempError);
                return new WeixinResult(tempError);
            }

            // 本机调试注掉
            if (!CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, gzh.token))
            {
                return Content("参数错误！");
            }

            //string appId = dp2WeiXinService.Instance.gzhContainer.GetDefault().appId;
            //postModel.AppId = appId;



            // 开始时间
            DateTime start_time = DateTime.Now;

            //与公众后台的设置保持一致
            postModel.AppId = gzh.appId;
            postModel.Token = gzh.token;
            postModel.EncodingAESKey = gzh.encodingAESKey;


            //v4.2.2之后的版本，可以设置每个人上下文消息储存的最大数量，防止内存占用过多，如果该参数小于等于0，则不限制
            var maxRecordCount = 10;

            // 日志总目录,使用前请确保App_Data文件夹存在，且有读写权限。
            var logDir = dp2WeiXinService.Instance.weiXinLogDir;

            //// 当日日志目录，用于详细输出消息
            //var logToday =string.Format(logDir + "/{0}/", DateTime.Now.ToString("yyyy-MM-dd"));
            //if (!Directory.Exists(logToday))
            //{
            //    Directory.CreateDirectory(logToday);
            //}

            //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
            var messageHandler = new dp2weixinMessageHandler(Request.InputStream, postModel, maxRecordCount);
            try
            {
                ////测试时可开启此记录，帮助跟踪数据
                //string id=_getRandomFileName();
                //string tempPath = Path.Combine(logToday, string.Format("{0}_Request_{1}.txt", id, messageHandler.RequestMessage.FromUserName));
                //messageHandler.RequestDocument.Save(tempPath);
                //if (messageHandler.UsingEcryptMessage)
                //{
                //    tempPath = Path.Combine(logToday, string.Format("{0}_Request_Ecrypt_{1}.txt", id, messageHandler.RequestMessage.FromUserName));
                //    messageHandler.EcryptRequestDocument.Save(tempPath);
                //}

                /* 如果需要添加消息去重功能，只需打开OmitRepeatedMessage功能，SDK会自动处理。
                 * 收到重复消息通常是因为微信服务器没有及时收到响应，会持续发送2-5条不等的相同内容的RequestMessage*/
                //messageHandler.OmitRepeatedMessage = true;

                //执行微信处理过程
                messageHandler.Execute();

                //////测试时可开启，帮助跟踪数据
                //if (messageHandler.ResponseDocument != null)
                //{
                //    //tempPath = Path.Combine(logToday, string.Format("{0}_Response_{1}.txt", id, messageHandler.RequestMessage.FromUserName));
                //    string path = dp2WeiXinService.Instance.weiXinDataDir+"/test.txt";
                //    messageHandler.ResponseDocument.Save(path);
                //}
                //if (messageHandler.UsingEcryptMessage)
                //{
                //    //记录加密后的响应信息
                //    tempPath = Path.Combine(logToday, string.Format("{0}_Response_Final_{1}.txt", id, messageHandler.RequestMessage.FromUserName));
                //    messageHandler.FinalResponseDocument.Save(tempPath);
                //}

                //return Content(messageHandler.ResponseDocument.ToString());//v0.7-
                //return new FixWeixinBugWeixinResult(messageHandler);//为了解决官方微信5.0软件换行bug暂时添加的方法，平时用下面一个方法即可
                
                // 测试异常
                //throw new Exception("test");



                // 计算处理消息用了多少时间
                TimeSpan time_length = DateTime.Now - start_time;
                string strMsgContext = "";
                if (messageHandler.RequestMessage is RequestMessageText)
                    strMsgContext = ((RequestMessageText)messageHandler.RequestMessage).Content;
                string info = "处理[" + messageHandler.RequestMessage.CreateTime + "-" + messageHandler.RequestMessage.MsgType.ToString() + "-" + strMsgContext + "]消息，time span: " + time_length.TotalSeconds.ToString() + " secs";
                if (time_length.TotalSeconds > 5)
                {
                    info = "请求超时:" + info;
                    dp2WeiXinService.Instance.WriteLog1(info);
                }
                else
                {
                    dp2WeiXinService.Instance.WriteLog3(info);
                }

                // 发送客服消息
                //messageHandler.SendCustomeMessage(info);

                // 如果消息是空内容，直接返回空，这样微信就不是重试了，用于 用户请求书目详细消息，公众号以客户消息返回
                if (messageHandler.ResponseMessage is ResponseMessageText)
                {
                    var mess = (ResponseMessageText)messageHandler.ResponseMessage;
                    if (String.IsNullOrEmpty(mess.Content) == true)
                    {
                        return Content("");
                    }
                }
                return new WeixinResult(messageHandler);//v0.8+
            }
            catch (Exception ex)
            {
                // 发送客服消息
                //messageHandler.SendCustomeMessage("异常：" + ex.Message);

                string error = "ExecptionMessage:" + ex.Message + "\n";
                error += ex.Source + "\n";
                error += ex.StackTrace + "\n";
                if (ex.InnerException != null)
                {
                    error += "========= InnerException =========" + "\n"; ;
                    error += ex.InnerException.Message + "\n"; ;
                    error += ex.InnerException.Source + "\n"; ;
                    error += ex.InnerException.StackTrace + "\n"; ;
                }

                //将程序运行中发生的错误记录到日志
                dp2WeiXinService.Instance.WriteErrorLog1(error);

                // 返回error信息
                return new WeixinResult(error);
            }
        }

    }
}
