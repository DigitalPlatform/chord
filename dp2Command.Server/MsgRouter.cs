﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

using DigitalPlatform;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Message;

namespace dp2Command.Service
{
    public class MsgRouter : ThreadBase
    {
        public event SendMessageEventHandler SendMessageEvent = null;

        // 这里是引用外部的对象，不负责创建和销毁
        public MessageConnectionCollection Channels = null;

        public string Url { get; set; }
        public string GroupName { get; set; }

        // 存储从 AddMessage() 得到的消息
        List<MessageRecord> _messageList = new List<MessageRecord>();
        private static readonly Object _syncRoot_messageList = new Object();

        // 记忆已经发送过的消息，避免重复发送
        Hashtable _sendedTable = new Hashtable();

        public MsgRouter()
        {
            this.PerTime = 60 * 1000;   // 60 * 1000
        }

        public void Start(MessageConnectionCollection channels,
            string url,
            string groupName)
        {
            this.Url = url;
            this.GroupName = groupName;
            this.Channels = channels;

            //Channels.Login += _channels_Login;
            Channels.AddMessage -= _channels_AddMessage;
            Channels.AddMessage += _channels_AddMessage;

            Channels.ConnectionStateChange -= _channels_ConnectionStateChange;
            Channels.ConnectionStateChange += _channels_ConnectionStateChange;

            this.BeginThread();
        }

        public void Stop()
        {
            Channels.AddMessage -= _channels_AddMessage;
            Channels.ConnectionStateChange -= _channels_ConnectionStateChange;

            this.StopThread(false);
        }

        void _channels_ConnectionStateChange(object sender, ConnectionEventArgs e)
        {
            if (e.Action == "Reconnected"
                || e.Action == "Connected")
            {
                this.Activate();//激活线程
            }
        }

        void _channels_AddMessage(object sender, AddMessageEventArgs e)
        {
            if (e.Action != "create")
                return;

            lock (_syncRoot_messageList)
            {
                // 累积太多了就不送入 list 了，只是激活线程等 GetMessage() 慢慢一百条地处理
                if (this._messageList.Count < 10000)
                    this._messageList.AddRange(e.Records);
            }
            this.Activate();
        }

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            //this.WriteLog("走到worker1");
            List<MessageRecord> records = GetMessage();
            if (records.Count > 0)
            {
                lock (_syncRoot_messageList)
                {
                    this._messageList.AddRange(records);
                }
            }
            //this.WriteErrorLog("走到worker2:" +records.Count);

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

                //this.WriteErrorLog("走到worker3:" + temp_records.Count);

                // 发送消息给下游模块
                SendMessage(temp_records);

                //this.WriteErrorLog("走到worker4:");

                // 从 dp2mserver 中删除这些消息
                DeleteMessage(temp_records, this.GroupName);

                //this.WriteErrorLog("走到worker5:");
            }

            // 如果本轮主动获得过消息，就要连续激活线程，让线程下次继续处理。只有本轮发现没有新消息了，才会进入休眠期
            if (records.Count > 0)
                this.Activate();

            CleanSendedTable(); // TODO: 可以改进为判断间隔至少 5 分钟才做一次
        }

        // 将消息发送给下游模块
        void SendMessage(List<MessageRecord> records)
        {
            SendMessageEventHandler handler = this.SendMessageEvent;

            foreach (MessageRecord record in records)
            {
                if (this._sendedTable.ContainsKey(record.id))
                    continue;

                this.WriteLog("开始处理:" + record.id);

                // 发送
                if (handler != null)
                {
                    SendMessageEventArgs e = new SendMessageEventArgs();
                    e.Message = record;
                    handler(this, e);
                }

                this.WriteLog("处理结束:" + record.id);

                this._sendedTable[record.id] = DateTime.Now;
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

        void WriteLog(string strText)
        {
            dp2CmdService2.Instance.WriteLog(strText);
            //MessageRecord record = new MessageRecord();
            //record.data = "*** error *** " + strText;
            //SendMessageEventArgs e = new SendMessageEventArgs();
            //e.Message = record;
            //this.SendMessageEvent(this, e);
        }

        // 从 dp2mserver 获得消息
        // 每次最多获得 100 条
        List<MessageRecord> GetMessage()
        {
            string strError = "";
            CancellationToken cancel_token = new CancellationToken();

            string id = Guid.NewGuid().ToString();
            GetMessageRequest request = new GetMessageRequest(id,
                "",
                this.GroupName, // "" 表示默认群组
                "",
                "", // strTimeRange,
                0,
                100);
            try
            {
                MessageConnection connection = this.Channels.GetConnectionAsync(
                    this.Url,
                    "").Result;
                GetMessageResult result = connection.GetMessageAsync(
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
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
            this.WriteLog("GetMessage() error: " + strError);
            return new List<MessageRecord>();
        }

        void DeleteMessage(List<MessageRecord> records,
            string strGroupName)
        {
            List<MessageRecord> delete_records = new List<MessageRecord>();

            foreach (MessageRecord source in records)
            {
                MessageRecord record = new MessageRecord();
                record.groups = strGroupName.Split(new char[] { ',' });
                record.id = source.id;
                delete_records.Add(record);
            }

            string strError = "";

            // CancellationToken cancel_token = new CancellationToken();

            try
            {
                MessageConnection connection = this.Channels.GetConnectionAsync(
                    this.Url,
                    "").Result;
                SetMessageRequest param = new SetMessageRequest("expire",
                    "dontNotifyMe",
                    records);

                SetMessageResult result = connection.SetMessageAsync(param).Result;
                if (result.Value == -1)
                    goto ERROR1;
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
            return;
        ERROR1:
            this.WriteLog("DeleteMessage() error : " + strError);
        }
    }

    public delegate void SendMessageEventHandler(object sender,
    SendMessageEventArgs e);

    public class SendMessageEventArgs : EventArgs
    {
        public MessageRecord Message = null;
    }
}
