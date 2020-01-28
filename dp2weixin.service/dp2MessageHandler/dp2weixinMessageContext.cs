/*----------------------------------------------------------------
    Copyright (C) 2015 Senparc
    
    文件名：CustomMessageContext.cs
    文件功能描述：微信消息上下文
    
    
    创建标识：Senparc - 20150312
----------------------------------------------------------------*/

//using Senparc.Weixin.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Senparc.Weixin.MP.Entities;
using dp2Command.Service;
using Senparc.NeuChar.Context;
using Senparc.NeuChar.Entities;
using Senparc.NeuChar;
using System.Xml.Linq;
using Senparc.Weixin.MP.MessageContexts;

namespace dp2weixin
{
    /// <summary>
    /// 用户上下文
    /// </summary>
    public class dp2weixinMessageContext : DefaultMpMessageContext // 2020-1-29改为继承DefaultMpMessageContext //MessageContext<IRequestMessageBase,IResponseMessageBase>
    {
        //// 命令集合
        //public CommandContainer CmdContiner = null;

        //// 当前命令
        //public string CurrentCmdName = null;

        //// 当前命令路径,该变量主要用于输出
        //public string CurrentCmdPath = "";


        /// <summary>
        /// 构造函数
        /// </summary>
        public dp2weixinMessageContext():base()
        {
            base.MessageContextRemoved += Dp2weixinMessageContext_MessageContextRemoved; //+= CustomMessageContext_MessageContextRemoved;

            // 初始命令集合，目前只存放三个有状态的命令：search,binding,renew
           // this.CmdContiner = new CommandContainer();
        }

        public override IRequestMessageBase GetRequestEntityMappingResult(RequestMsgType requestMsgType, XDocument doc)
        {
            return base.GetRequestEntityMappingResult(requestMsgType, doc);
        }

        public override IResponseMessageBase GetResponseEntityMappingResult(ResponseMsgType responseMsgType, XDocument doc)
        {
            return base.GetResponseEntityMappingResult(responseMsgType, doc);
        }

        private void Dp2weixinMessageContext_MessageContextRemoved(object sender, WeixinContextRemovedEventArgs<IRequestMessageBase, IResponseMessageBase> e)
        {
            var messageContext = e.MessageContext as dp2weixinMessageContext;
            if (messageContext == null)
            {
                return;//如果是正常的调用，messageContext不会为null
            }

        }

    }
}
