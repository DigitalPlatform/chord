using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Weixin.MP.Helpers;
using System.Xml;
using DigitalPlatform.Xml;
using System.Globalization;
//using Senparc.Weixin.Context;
using Senparc.Weixin.MP.Entities.Request;
using DigitalPlatform.IO;
using System.Diagnostics;
using DigitalPlatform;
using DigitalPlatform.Text;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.CommonAPIs;
using System.Web.Mvc.Async;
using System.Threading.Tasks;
using dp2weixin.service;
using dp2Command.Service;
using Senparc.NeuChar.Entities;

namespace dp2weixin
{
    /// <summary>
    /// 自定义MessageHandler
    /// 把MessageHandler作为基类，重写对应请求的处理方法
    /// </summary>
    public partial class dp2weixinMessageHandler : MessageHandler<dp2weixinMessageContext>
    {
        public string AppId = "";
        private string EncodingAESKey = "";
        private string Token = "";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="maxRecordCount"></param>
        public dp2weixinMessageHandler(Stream inputStream, 
            PostModel postModel, 
            int maxRecordCount = 0): base(inputStream, postModel, maxRecordCount)
        {

           this.AppId=postModel.AppId;
           this.EncodingAESKey = postModel.EncodingAESKey;
           this.Token = postModel.Token;
        }



        /// <summary>
        /// 执行时，用于过滤黑名单
        /// </summary>
        public  override Task OnExecutingAsync(System.Threading.CancellationToken token)
        {
           return  base.OnExecutingAsync(token);
        }

        /// <summary>
        /// 执行后
        /// </summary>
        public override Task OnExecutedAsync(System.Threading.CancellationToken token)
        {
            return base.OnExecutedAsync(token);
        }
      
        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            string strText = requestMessage.Content;

            return this.CreateTextResponseMessage("您刚才发送["+strText+"]");

            //string strMessage = "您好，欢迎使用。";
            //return this.CreateTextResponseMessage(strMessage);

        }
 
        /// <summary>
        /// 创建文本回复消息
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        private IResponseMessageBase CreateTextResponseMessage(string strText)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = strText;
            return responseMessage;
        }


 



        #region 其它类型消息

        /// <summary>
        /// 处理位置请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnLocationRequest(RequestMessageLocation requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = string.Format("您刚才发送了地理位置信息。Location_X：{0}，Location_Y：{1}，Scale：{2}，标签：{3}",
                              requestMessage.Location_X, requestMessage.Location_Y,
                              requestMessage.Scale, requestMessage.Label);
            return responseMessage;
        }
        /// <summary>
        /// 处理图片请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnImageRequest(RequestMessageImage requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageNews>();
            responseMessage.Articles.Add(new Article()
            {
                Title = "您刚才发送了图片信息",
                Description = "您发送的图片将会显示在边上",
                PicUrl = requestMessage.PicUrl,
                Url = "http://www.qxuninfo.com"
            });
            //responseMessage.Articles.Add(new Article()
            //{
            //    Title = "第二条",
            //    Description = "第二条带连接的内容",
            //    PicUrl = requestMessage.PicUrl,
            //    Url = "http://www.qxuninfo.com"
            //});
            return responseMessage;
        }
        /// <summary>
        /// 处理语音请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnVoiceRequest(RequestMessageVoice requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageMusic>();
            responseMessage.Music.MusicUrl = "http://www.qxuninfo.com/music.mp3";
            responseMessage.Music.Title = "这里是一条音乐消息";
            responseMessage.Music.Description = "时间都去哪儿了";
            return responseMessage;
        }
        /// <summary>
        /// 处理视频请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnVideoRequest(RequestMessageVideo requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您发送了一条视频信息，ID：" + requestMessage.MediaId;
            return responseMessage;
        }
        /// <summary>
        /// 处理链接消息请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnLinkRequest(RequestMessageLink requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = string.Format(@"您发送了一条连接信息：
Title：{0}
Description:{1}
Url:{2}", requestMessage.Title, requestMessage.Description, requestMessage.Url);
            return responseMessage;
        }
        /// <summary>
        /// 处理事件请求（这个方法一般不用重写，这里仅作为示例出现。除非需要在判断具体Event类型以外对Event信息进行统一操作
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEventRequest(IRequestMessageEventBase requestMessage)
        {
            var eventResponseMessage = base.OnEventRequest(requestMessage);//对于Event下属分类的重写方法，见：CustomerMessageHandler_Events.cs
            return eventResponseMessage;
        }

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            //所有没有被处理的消息会默认返回这里的结果
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "这条消息来自DefaultResponseMessage。";
            return responseMessage;
        }
        #endregion
    }
}
