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

       /// <summary>
        /// 构造函数
        /// </summary>
        public dp2weixinMessageContext()
        {
            //base.MessageContextRemoved += Dp2weixinMessageContext_MessageContextRemoved; //+= CustomMessageContext_MessageContextRemoved;

            base.MessageContextRemoved += CustomMessageContext_MessageContextRemoved;


            // 初始命令集合，目前只存放三个有状态的命令：search,binding,renew
            // this.CmdContiner = new CommandContainer();
        }

        /*
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
        */

        /// <summary>
        /// 当上下文过期，被移除时触发的时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CustomMessageContext_MessageContextRemoved(object sender, Senparc.NeuChar.Context.WeixinContextRemovedEventArgs<IRequestMessageBase, IResponseMessageBase> e)
        {
            /* 注意，这个事件不是实时触发的（当然你也可以专门写一个线程监控）
             * 为了提高效率，根据WeixinContext中的算法，这里的过期消息会在过期后下一条请求执行之前被清除
             */

            var messageContext = e.MessageContext as dp2weixinMessageContext;
            if (messageContext == null)
            {
                return;//如果是正常的调用，messageContext不会为null
            }

            //TODO:这里根据需要执行消息过期时候的逻辑，下面的代码仅供参考

            //Log.InfoFormat("{0}的消息上下文已过期",e.OpenId);
            //api.SendMessage(e.OpenId, "由于长时间未搭理客服，您的客服状态已退出！");
        }


    }
}
