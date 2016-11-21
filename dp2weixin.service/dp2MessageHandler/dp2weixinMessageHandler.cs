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
using Senparc.Weixin.Context;
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
            //这里设置仅用于测试，实际开发可以在外部更全局的地方设置，
            //比如MessageHandler<MessageContext>.GlobalWeixinContext.ExpireMinutes = 3。
            WeixinContext.ExpireMinutes = 3;

           this.AppId=postModel.AppId;
           this.EncodingAESKey = postModel.EncodingAESKey;
           this.Token = postModel.Token;
        }



        /// <summary>
        /// 执行时，用于过滤黑名单
        /// </summary>
        public override void OnExecuting()
        {
            base.OnExecuting();
        }

        /// <summary>
        /// 执行后
        /// </summary>
        public override void OnExecuted()
        {
            base.OnExecuted();
        }

        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            string strText = requestMessage.Content;

            //设当前命令路径，用于在回复时输出
            this.CurrentMessageContext.CurrentCmdPath = strText;

            // 退出命令环境
            if (strText == "exit" || strText == "quit")
            {
                // 置空当前命令
                this.CurrentMessageContext.CurrentCmdName = "";
                return this.CreateTextResponseMessage("成功退出命令环境。");
            }

            // 用空隔号分隔命令与参数，例如：
            // search 空 重新发起检索
            // search n             显示上次命中结果集中下一页
            // search 序号         显示详细
            // binding r0000001/111111
            // renew view
            string strCommand = strText;
            string strParam = "";
            int nIndex = strText.IndexOf(' ');
            if (nIndex > 0)
            {
                strCommand = strText.Substring(0, nIndex);
                strParam = strText.Substring(nIndex + 1);
            }

            // 检查是否是命令，如果不是，则将输入认为是当前命令的参数（二级命令）
            bool bRet = dp2CommandUtility.CheckIsCommand(strCommand);
            if (bRet == false)
            {
                strCommand = "";
                if (String.IsNullOrEmpty(this.CurrentMessageContext.CurrentCmdName) == false)
                {
                    strCommand = this.CurrentMessageContext.CurrentCmdName;
                    strParam = strText;
                }
                else
                {
                    // 没有当前命令
                    return DoUnknownCmd(strText);
                }
            }

            //设当前命令路径，用于在回复时输出
            string strPath = strCommand;
            if (String.IsNullOrEmpty(strParam) == false)
                strPath = strCommand + ">" + strParam;
            this.CurrentMessageContext.CurrentCmdPath = strPath;

            // command可以转成小写
            strCommand = strCommand.ToLower();

            //=========================
            
            // 检索命令
            if (strCommand == dp2CommandUtility.C_Command_Search)
            {
                return this.DoSearch(strParam);
            }

            //=========================
            // 设置命令
            if (strCommand == dp2CommandUtility.C_Command_Set)
            {
                return this.DoSet(strParam);
            }


            // 不认识的命令
            return DoUnknownCmd(strText);
        }

        private IResponseMessageBase DoSet(string strParam)
        {
            // 设置当前命令
            this.CurrentMessageContext.CurrentCmdName = dp2CommandUtility.C_Command_Set;


            if (strParam == "")
            {
                return this.CreateTextResponseMessage("请输入要设置的功能名称");
            }

            // 继续解析strParam
            string function = strParam;
            string parameter = "";
            int nIndex = strParam.IndexOf(' ');
            if (nIndex > 0)
            {
                function = strParam.Substring(0, nIndex);
                parameter = strParam.Substring(nIndex + 1);
            }

            string weixinId = this.WeixinOpenId + "@" + this.AppId;


            if (function == "tracing")
            {
                // 先检查该微信用户是否绑定了图书馆的工作人员账号

                List<WxUserItem> workerList = WxUserDatabase.Current.Get(weixinId,
                    null,
                    WxUserDatabase.C_Type_Worker);
                if (workerList == null || workerList.Count == 0)
                {
                    GzhCfg gzh = dp2WeiXinService.Instance.gzhContainer.GetByAppId(this.AppId);
                    if (gzh == null)
                    {
                        return this.CreateTextResponseMessage("未找到" + this.AppId + "对应的公众号配置", false);
                    }
                    string accountIndex = dp2WeiXinService.Instance.GetOAuth2Url(gzh, "Account/Index");

                    return this.CreateTextResponseMessage("您尚未绑定图书馆工作人员账户，不能使用tracing功能。"
                       + "\n点击 <a href='" + accountIndex + "'>绑定账户</a>。");
                }                

                string paramLeft = parameter;
                string paramRight = "";
                nIndex = parameter.IndexOf(' ');
                if (nIndex > 0)
                {
                    paramLeft = parameter.Substring(0, nIndex);
                    paramRight = parameter.Substring(nIndex + 1);
                }

                if (paramLeft == "off")
                {
                    // 更新到数据库中 todo绑了多个图馆的情况
                    foreach(WxUserItem user in workerList)
                    {
                        WxUserDatabase.Current.UpdateTracing(user.id, "off");
                    }

                    dp2WeiXinService.Instance.TracingOnUsers.Remove(weixinId);
                    string text = "set tracing off 成功，您将不再收到非本人的微信通知。";
                    return this.CreateTextResponseMessage(text);
                }
                else if (paramLeft == "on")
                {
                    string tempTracing = "on";
                    if (paramRight == "-mask")
                        tempTracing += " " + paramRight;
                    // 更新到数据库中 todo绑了多个图馆的情况
                    foreach (WxUserItem user in workerList)
                    {
                        WxUserDatabase.Current.UpdateTracing(user.id, tempTracing);
                    }

                    TracingOnUser tracingOnUser = new TracingOnUser();
                    tracingOnUser.WeixinId = weixinId;// this.WeixinOpenId + "@" + this.AppId; //让这个微信id带上@appId
                    //tracingOnUser.AppId = this.AppId;

                    if (paramRight == "-mask")
                        tracingOnUser.IsMask = false;

                    // 检查有没有绑 数字平台,绑了的话，设为公司管理员
                    foreach (WxUserItem user in workerList)
                    {
                        LibEntity lib = dp2WeiXinService.Instance.GetLibById(user.libId);
                        if (lib != null)
                        {
                            if (lib.libName == WeiXinConst.C_Dp2003LibName)
                            {
                                tracingOnUser.IsAdmin = true;
                                break;
                            }
                        }
                    }

                    // 设到hashtable里
                    dp2WeiXinService.Instance.TracingOnUsers[weixinId] = tracingOnUser;

                    string text = "set " + strParam + " 成功，您将会收到本馆的全部微信通知";
                    if (tracingOnUser.IsAdmin == true)
                    {
                        text = "set "+strParam+" 成功，您是数字平台工作人员，您将会收到全部图书馆的微信通知";
                    }
                    if (tracingOnUser.IsMask == false)
                        text += "，且指定对读者敏感信息不做马赛克处理。";
                    else
                        text += "，系统默认对读者敏感信息做马赛克处理。";

                    return this.CreateTextResponseMessage(text);
                }
                else
                {
                    if (dp2WeiXinService.Instance.TracingOnUsers[weixinId] == null)
                    {
                        return this.CreateTextResponseMessage("您当前是 tracing off 状态。");
                    }
                    else
                    {
                        string text = "您当前是 tracing on 状态";
                        TracingOnUser user = (TracingOnUser)dp2WeiXinService.Instance.TracingOnUsers[weixinId];
                        if (user.IsAdmin)
                            text += "，且是数据平台管理员";

                        if (user.IsMask == true)
                            text += "，系统默认对读者敏感信息做马赛克处理。";
                        else
                            text += "，且指定了对读者敏感信息不做马赛克处理。";

                        return this.CreateTextResponseMessage(text);
                    }
                }
            }

            return this.CreateTextResponseMessage("set不支持"+strParam+"功能");

        }

        /// <summary>
        /// 检索
        /// </summary>
        /// <param name="strParam"></param>
        /// <returns></returns>
        private IResponseMessageBase DoSearch(string strParam)
        {
            //return this.CreateTextResponseMessage("未实现");

            // 设置当前命令
            this.CurrentMessageContext.CurrentCmdName = dp2CommandUtility.C_Command_Search;


            if (strParam == "")
            {
                return this.CreateTextResponseMessage("请输入检索词");
            }

            long lRet = 0;
            string strError = "";
            SearchCommand searchCmd = (SearchCommand)this.CurrentMessageContext.CmdContiner.GetCommand(dp2CommandUtility.C_Command_Search);


            // 如果没有结果集，优先认查询
            if (string.IsNullOrEmpty(searchCmd.ResultSetName) == false)
            {
                // 查看，从第一页开始
                if (strParam.ToLower() == "v")
                {
                    // 从头显示
                    //searchCmd.ResultNextStart = 0;
                    //searchCmd.IsCanNextPage = true;
                    //string strNextPage = "";
                    //bool bRet = searchCmd.GetNextPage(out strNextPage, out strError);
                    //if (bRet == true)
                    //    return this.CreateTextResponseMessage(strNextPage);
                    //else
                    //    return this.CreateTextResponseMessage(strError);
                }

                // 下一页
                if (strParam.ToLower() == "n")
                {
                    //string strNextPage = "";
                    //bool bRet = searchCmd.GetNextPage(out strNextPage, out strError);
                    //if (bRet == true)
                    //    return this.CreateTextResponseMessage(strNextPage);
                    //else
                    //    return this.CreateTextResponseMessage(strError);
                }

                // 试着转换为书目序号
                int nBiblioIndex = 0;
                try
                {
                    nBiblioIndex = int.Parse(strParam);
                }
                catch
                { }
                // 获取详细信息
                if (nBiblioIndex >= 1)
                {

                    ////异步操作 使用客服消息接口回复用户
                    //AsyncManager m = new AsyncManager();
                    //m.OutstandingOperations.Increment();//AsyncManager.OutstandingOperations.Increment();                    
                    //var task = Task.Run(() => this.SendBiblioDetail(searchCmd,nBiblioIndex));
                    //task.ContinueWith(t =>
                    //{
                    //      m.OutstandingOperations.Decrement(); //AsyncManager.OutstandingOperations.Decrement();
                    //});

                    //// 返回空
                    //var responseMessage = CreateResponseMessage<ResponseMessageText>();
                    //responseMessage.Content = "";
                    //return responseMessage;                   

                }

            }

            // 检索
            string strFirstPage = "test";
            //lRet = this.CmdService.SearchBiblio(this.CurrentMessageContext.LibUserName,
            //    strParam,
            //    searchCmd,
            //    out strFirstPage,
            //    out strError);
            //if (lRet == -1)
            //{
            //    return this.CreateTextResponseMessage("检索出错：" + strError);
            //}
            //else if (lRet == 0)
            //{
            //    return this.CreateTextResponseMessage("未命中");
            //}

            return this.CreateTextResponseMessage(strFirstPage);

        }

          
        // 消息处理
        public void SendBiblioDetail(SearchCommand searchCmd, int nBiblioIndex)
        {
            //string strResult = "";
            //string strError = "";
            //string strBiblioInfo = "";
            //int lRet = this.CmdService.GetDetailBiblioInfo(this.CurrentMessageContext.LibUserName, 
            //    searchCmd, 
            //    nBiblioIndex,
            //    out strBiblioInfo,
            //    out strError);
            //if (lRet == -1 || lRet == 0)
            //{
            //    strResult = strError;
            //}
            //else
            //{
            //    strResult = strBiblioInfo;
            //}
            //// 发送客服消息
            //((dp2CmdService2)this.CmdService).SendCustomerMsg(this.WeixinOpenId, strResult);

        }
        
                


        /// <summary>
        /// 处理未知的命令
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        private IResponseMessageBase DoUnknownCmd(string strText)
        {
            //string strMessage = "您好，不认识的命令，您可以回复：\n"
            //        + "selectlib:选择图书馆" + "\n"
            //       + "search:检索" + "\n"
            //       + "binding:绑定读者账号" + "\n"
            //       + "unbinding:解除绑定" + "\n"
            //       + "myinfo:个人信息" + "\n"
            //       + "borrowinfo:借阅信息" + "\n"
            //       + "renew:续借" + "\n"
            //       + "bookrecommend:新书推荐" + "\n"
            //       + "notice:公告" + "\n"
            //       + "changePatron:切换读者" + "\n";
            string strMessage = "您好，欢迎使用。";
            return this.CreateTextResponseMessage(strMessage);
        }

        private IResponseMessageBase CreateTextResponseMessage(string strText, bool bShowPath)
        {
            //if (bShowPath == true)
            //{
            //    strText = "命令路径:[" + this.CurrentMessageContext.CurrentCmdPath + "]\n"
            //        + "============\n"
            //        + strText;
            //}

            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = strText;
            return responseMessage;
        }
        /// <summary>
        /// 创建文本回复消息
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        private IResponseMessageBase CreateTextResponseMessage(string strText)
        {
            return this.CreateTextResponseMessage(strText, true);
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
