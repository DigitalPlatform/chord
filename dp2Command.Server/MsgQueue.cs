using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Command.Service
{
    public class MsgQueue:Queue
    {
        /*
        /// <summary>
        /// 处理过的消息队列，用于消息查重
        /// </summary>
        private Queue msgQueue = new Queue();

        /// <summary>
        /// 查重，检查消息是否已经处理过
        /// </summary>
        /// <returns></returns>
        public bool Contains(string msgId)
        {
            return msgQueue.Contains(msgId);
        }
        */
        /// <summary>
        /// 把处理过的消息加到队列里
        /// </summary>
        /// <param name="msgId"></param>
        public void AddMsgToQueue(string msgId)
        {
            // todo 如果队列超过100，删除100前面的
            if (this.Count > 5)
            {
                while (this.Count == 5)
                {
                    this.Dequeue();
                }
            }

            //加到队列
            this.Enqueue(msgId);
        }
    }
}
