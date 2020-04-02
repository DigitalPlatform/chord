using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

using DigitalPlatform;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Message;
using DigitalPlatform.Core;

namespace dp2weixin.service
{
    // 消息路由器，记得这个类还是15年时谢老师帮忙写的，当初我自己写了很长时间也没搞清楚。
    public class MsgRouter : ThreadBase
    {
        // 发消息事件
        public event SendMessageEventHandler SendMessageEvent = null;

        // 消息通道集合，这里是引用外部的对象，不负责创建和销毁
        public MessageConnectionCollection Channels = null;

        // 服务器地址和群名称
        public string Url { get; set; }
        public string GroupName { get; set; }

        // 存储从 AddMessage() 得到的消息
        List<MessageRecord> _messageList = new List<MessageRecord>();
        private static readonly Object _syncRoot_messageList = new Object();   //做锁

        // 记忆已经发送过的消息，避免重复发送
        Hashtable _sendedTable = new Hashtable();

        // 构造函数，里面设置运行时间
        public MsgRouter()
        {
            this.PerTime = 3*60 * 1000;  //改为3分钟了 2016-6-13 // 60 * 1000
        }


        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="channels">通道集合</param>
        /// <param name="url"></param>
        /// <param name="groupName"></param>
        public void Start(MessageConnectionCollection channels,
            string url,
            string groupName)
        {
            this.Url = url;
            this.GroupName = groupName;
            this.Channels = channels;

            // 通道消息事件，一有消息就立即收到，与getmessage公众号这边主动获取是两个类别。
            // 注意设置消息接管事件时先减再加
            Channels.AddMessage -= _channels_AddMessage;
            Channels.AddMessage += _channels_AddMessage;

            // 通道连接状态变化消息
            Channels.ConnectionStateChange -= _channels_ConnectionStateChange;
            Channels.ConnectionStateChange += _channels_ConnectionStateChange;

            // 开始
            this.BeginThread();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            Channels.AddMessage -= _channels_AddMessage;
            Channels.ConnectionStateChange -= _channels_ConnectionStateChange;

            this.StopThread(true);//2016-9-22 StopThread() 的参数最好修改为 true，以便在重启之类情况下尽快关闭线程。
        }

        /// <summary>
        /// 通道状态重连时，把线程激活
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _channels_ConnectionStateChange(object sender, ConnectionEventArgs e)
        {
            if (e.Action == "Reconnected"
                || e.Action == "Connected")
            {
                this.Activate();//激活线程
            }
        }

        // 片断消息缓冲
        StringBuilder _msgBuffer = new StringBuilder();
        public const long C_StringBuffer_MaxLength = 1024 * 1024;
        public const long C_SendTable_MaxCount = 5000;

        // 接管新增消息事件
        void _channels_AddMessage(object sender, AddMessageEventArgs e)
        {
            if (e.Action != "create")
                return;

            MessageConnection connection = (MessageConnection)sender;
            // 这一断没太看懂，只有通道名为_traceMessage才处理吗
            if (connection.Name != dp2WeiXinService.C_ConnName_TraceMessage)
                return;

            // 合并消息
            List<MessageRecord> fullRecords = new List<MessageRecord>();
            if (e.Records.Count > 0)
            {
                foreach (MessageRecord record in e.Records)
                {
                    // 检查尺寸有没有超过限制
                    if (_msgBuffer.Length + record.data.Length > C_StringBuffer_MaxLength)
                    {
                        dp2WeiXinService.Instance.WriteErrorLog("消息尺寸超过1M，不处理该消息。");
                        _msgBuffer.Clear();//消空消息缓存
                        return;
                    }

                    // 中间的片断消息
                    if (string.IsNullOrEmpty(record.id) == true)
                    {
                        _msgBuffer.Append(record.data);
                        continue;
                    }

                    // 收尾消息
                    _msgBuffer.Append(record.data); //串上自己的data
                    record.data = _msgBuffer.ToString();  //设上完整的data
                    fullRecords.Add(record); //设为正式的消息
                    _msgBuffer.Clear();//消空消息缓存
                }
            }

            // 有完整的消息才加到数组里
            if (fullRecords.Count > 0)
            {
                lock (_syncRoot_messageList)
                {
                    // 累积太多了就不送入 list 了，只是激活线程等 GetMessage() 慢慢一百条地处理
                    if (this._messageList.Count < 10000)
                        this._messageList.AddRange(fullRecords);//e.Records);
                }

                dp2WeiXinService.Instance.WriteDebug2("AddMessage得到" + fullRecords.Count.ToString() + "条消息。");
                
                // 收到消息后，触发线程处理，即会走到worker里
                this.Activate();
            }
        }

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            List<MessageRecord> records = new List<MessageRecord>();
 
            // 2016-12-2 不是从addMessage过来的才调GetMessage，否则总是重复取消息
            if (this._messageList.Count == 0) 
            {
                records = GetMessage();
                if (records.Count > 0)
                {
                    lock (_syncRoot_messageList)
                    {
                        this._messageList.AddRange(records);
                    }
                }
            }

            bool bDeleteOk = false;
            if (this._messageList.Count > 0)
            {
                // 取出前面 100 个加以处理
                // 这样锁定的时间很短
                List<MessageRecord> temp_records = new List<MessageRecord>();
                lock (_syncRoot_messageList)
                {
                    int i = 0;
                    foreach(MessageRecord record in this._messageList)
                    {
                        if (i >= 100)
                            break;
                        temp_records.Add(record);
                        i++;
                    }
                    this._messageList.RemoveRange(0, temp_records.Count);
                }

                // 发送消息给下游模块
                SendMessage(temp_records);


                // 从 dp2mserver 中删除这些消息
                bDeleteOk = DeleteMessage(temp_records);//jane 2016-6-20 不需要传group参数了, this.GroupName);

            }

            // 如果本轮主动获得过消息，就要连续激活线程，让线程下次继续处理。
            // 只有本轮发现没有新消息了，才会进入休眠期
            // 如果发现删除出错，不再进行 Activate()。这样工作线程等几分钟才会过来重试。
            if (records.Count > 0 && bDeleteOk==true)
            {
                /* 光 2016/6/9 1:25:16
在你的router类中，当
DeleteMessage(temp_records, this.GroupName);
里面出错后，(原因是 当前用户账户没有配置 groups 为 _patronNotify，所以 expire 失效总会失败，请你模拟一下这种情况）
但后面：
            if (records.Count > 0)
                this.Activate();
马上又激活了线程(因为records里面总有内容)。导致线程快速反复循环，始终失效不了任何消息，耗费大量资源。也许错误日志也会被迅速填满呢。
应当考虑在出错后，增加一些判断，此时不要进行 Activate()。这样工作线程等几分钟才会过来重试。
                */
                this.Activate();
            }

            CleanSendedTable(); // TODO: 可以改进为判断间隔至少 5 分钟才做一次
        }

        // 将消息发送给下游模块
        void SendMessage(List<MessageRecord> records)
        {
            SendMessageEventHandler handler = this.SendMessageEvent;

            // 下级的代码不影响
            foreach (MessageRecord record in records)
            {
                if (this._sendedTable.ContainsKey(record.id))
                    continue;

                dp2WeiXinService.Instance.WriteDebug2("开始处理:" + record.id);
                
                // 内容太多了，不适合加
                //dp2WeiXinService.Instance.WriteDebug2("消息内容：" + record.data);

                // 发送
                if (handler != null)
                {
                    SendMessageEventArgs e = new SendMessageEventArgs();
                    e.Message = record;
                    handler(this, e);
                }
                dp2WeiXinService.Instance.WriteDebug2("处理结束:" + record.id);

                if (this._sendedTable.Count < C_SendTable_MaxCount)  //大于了5K则不再给里面增加了。
                {
                    this._sendedTable[record.id] = DateTime.Now;
                }
            }
        }


        // 清理超过一定时间的“已发送”记忆事项
        void CleanSendedTable()
        {
            DateTime now = DateTime.Now;
            TimeSpan delta = new TimeSpan(0, 30, 0);
            List<string> delete_keys = new List<string>();
            foreach (string key in this._sendedTable.Keys)
            {
                var time = (DateTime)this._sendedTable[key];
                if (time - now > delta)
                    delete_keys.Add(key);
            }

            foreach (string key in delete_keys)
            {
                this._sendedTable.Remove(key);
            }
        }

        // 从 dp2mserver 获得消息
        // 每次最多获得 100 条
        List<MessageRecord> GetMessage()
        {
            string strError = "";
            CancellationToken cancel_token = new CancellationToken();

            string subjectCondition = "";
            string id = Guid.NewGuid().ToString();
            GetMessageRequest request = new GetMessageRequest(id,
                "",
                this.GroupName, // "" 表示默认群组
                "",
                "", // strTimeRange,
                "publishTime|asc",//sortCondition 按发布时间正序排
                "",
                subjectCondition,
                0,
                100);
            try
            {
                MessageConnection connection = this.Channels.GetConnectionTaskAsync(
                    this.Url,
                    dp2WeiXinService.C_ConnName_TraceMessage).Result;
                GetMessageResult result = connection.GetMessage(request,
                    new TimeSpan(0, 1, 0),
                    cancel_token);
                if (result.Value == -1)
                    goto ERROR1;
                return result.Results;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

        ERROR1:
            dp2WeiXinService.Instance.WriteErrorLog("GetMessage() error: " + strError);
            //this.WriteLog("GetMessage() error: " + strError,dp2WeiXinService.C_LogLevel_1);
            return new List<MessageRecord>();
        }

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <param name="records"></param>
        /// <param name="strGroupName"></param>
        /// <returns>
        /// true    成功
        /// false   失败
        /// </returns>
        bool DeleteMessage(List<MessageRecord> records)
        {
             List<MessageRecord> delete_records = new List<MessageRecord>();

            foreach (MessageRecord source in records)
            {
                MessageRecord record = new MessageRecord();
                // 2016-6-20 jane 不需要传group参数
                record.groups = dp2WeiXinService.C_Group_PatronNotity .Split(new char[] { ',' });
                record.id = source.id;
                delete_records.Add(record);
            }

            string strError = "";

            // CancellationToken cancel_token = new CancellationToken();

            try
            {

                MessageConnection connection = this.Channels.GetConnectionTaskAsync(
                    this.Url,
                    dp2WeiXinService.C_ConnName_TraceMessage).Result;

                SetMessageRequest param = new SetMessageRequest("expire",
                    "dontNotifyMe",
                    delete_records);//records);这里应该用delete_records吧，用records好像也没错
                CancellationToken cancel_token = new CancellationToken();
                SetMessageResult result = connection.SetMessageTaskAsync(param, 
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            return true;


        ERROR1:
            dp2WeiXinService.Instance.WriteErrorLog("DeleteMessage() error : " + strError);

            //this.WriteLog("DeleteMessage() error : " + strError, dp2WeiXinService.C_LogLevel_1);
            return false;
        }
    }

    public delegate void SendMessageEventHandler(object sender,
    SendMessageEventArgs e);

    public class SendMessageEventArgs : EventArgs
    {
        public MessageRecord Message = null;
    }
}
