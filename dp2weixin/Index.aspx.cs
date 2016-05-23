using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities.Request;
using DigitalPlatform.LibraryRestClient;
using dp2Command.Service;
namespace dp2weixin
{
    public partial class Index : System.Web.UI.Page
    {
        //与微信公众账号后台的Token设置保持一致，区分大小写。123444
        private readonly string Token = "dp2weixin";
        
        protected void Page_Load(object sender, EventArgs e)
        {
            string signature = Request["signature"];
            string timestamp = Request["timestamp"];
            string nonce = Request["nonce"];
            string echostr = Request["echostr"];

            if (Request.HttpMethod == "GET")
            {
                //get method - 仅在微信后台填写URL验证时触发
                if (CheckSignature.Check(signature, timestamp, nonce, Token))
                {
                    WriteContent(echostr); //返回随机字符串则表示验证通过
                }
                else
                {
                    WriteContent("failed:" + signature + "," + CheckSignature.GetSignature(timestamp, nonce, Token) + "。" +
                                "如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。");
                }
                Response.End();
            }
            else
            {      
                // 本地调试时，要把校验关掉
                /*
                //post method - 当有用户向公众账号发送消息时触发
                if (!CheckSignature.Check(signature, timestamp, nonce, Token))
                {
                    WriteContent("参数错误！");
                    return;
                }
                 */
                var postModel = new PostModel()
                {
                    Signature = Request.QueryString["signature"],
                    Msg_Signature = Request.QueryString["msg_signature"],
                    Timestamp = Request.QueryString["timestamp"],
                    Nonce = Request.QueryString["nonce"],

                    //以下保密信息不会（不应该）在网络上传播，请注意
                    Token = Token,
                    //根据自己后台的设置保持一致??? todo 在微信测试公众号没看到这个值EncodingAESKey？
                    EncodingAESKey = "85777abcddde69d7c44f421c49dfa331",
                    AppId = "wx0f2b65b37835f531"//根据自己后台的设置保持一致
                };
                

                //v4.2.2之后的版本，可以设置每个人上下文消息储存的最大数量，防止内存占用过多，如果该参数小于等于0，则不限制
                var maxRecordCount = 10;

                //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
                var messageHandler = new dp2weixinMessageHandler(dp2CommandService.Instance,
                    Request.InputStream, postModel, maxRecordCount);
                messageHandler.Init(Server.MapPath("~"), true,false);

                try
                {
                    //测试时可开启此记录，帮助跟踪数据
                    //Global.GlobalWeiXinServer.WriteErrorLog(messageHandler.RequestDocument.ToString());
                    
                    //执行微信处理过程
                    messageHandler.Execute();                    

                    //测试时可开启，帮助跟踪数据
                    //Global.GlobalWeiXinServer.WriteErrorLog(messageHandler.ResponseDocument.ToString());

                    // 返回给微信服务器
                    WriteContent(messageHandler.ResponseDocument.ToString());

                    return;
                }
                catch (Exception ex)
                {                    
                    //将程序运行中发生的错误记录到日志
                    dp2CommandService.Instance.WriteErrorLog(LibraryChannel.GetExceptionMessage(ex));
                    if (messageHandler.ResponseDocument != null)
                    {
                        dp2CommandService.Instance.WriteErrorLog(messageHandler.ResponseDocument.ToString());
                    }

                    // 返回给微信服务器为空内容
                    WriteContent("");
                }
                finally
                {
                    Response.End();
                }
            }
        }

        /// <summary>
        /// 返回内容
        /// </summary>
        /// <param name="str"></param>
        private void WriteContent(string str)
        {
            Response.Output.Write(str);
        }
    }
}