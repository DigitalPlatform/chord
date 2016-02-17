/*----------------------------------------------------------------
    Copyright (C) 2015 Senparc
    
    文件名：CustomMessageContext.cs
    文件功能描述：微信消息上下文
    
    
    创建标识：Senparc - 20150312
----------------------------------------------------------------*/

using Senparc.Weixin.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Senparc.Weixin.MP.Entities;
using dp2Command.Service;

namespace dp2weixin
{
    /// <summary>
    /// 用户上下文
    /// </summary>
    public class dp2MessageContext : MessageContext<IRequestMessageBase,IResponseMessageBase>
    {
        /// <summary>
        /// 读者证条码号，如果未绑定则为空
        /// </summary>
        public string ReaderBarcode = "";

        // 命令集合
        public CommandContainer CmdContiner = null;
        // 当前命令
        public string CurrentCmdName = null;

        // 当前命令路径,该变量主要用于输入
        public string CurrentCmdPath = "";

        /// <summary>
        /// 构造函数
        /// </summary>
        public dp2MessageContext():base()
        {
            base.MessageContextRemoved += CustomMessageContext_MessageContextRemoved;

            // 初始命令集合，目前只存放三个有状态的命令：search,binding,renew
            this.CmdContiner = new CommandContainer();
        }

        /// <summary>
        /// 当上下文过期，被移除时触发的时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CustomMessageContext_MessageContextRemoved(object sender, Senparc.Weixin.Context.WeixinContextRemovedEventArgs<IRequestMessageBase,IResponseMessageBase> e)
        {
            /* 注意，这个事件不是实时触发的（当然你也可以专门写一个线程监控）
             * 为了提高效率，根据WeixinContext中的算法，这里的过期消息会在过期后下一条请求执行之前被清除
             */

            var messageContext = e.MessageContext as dp2MessageContext;
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
