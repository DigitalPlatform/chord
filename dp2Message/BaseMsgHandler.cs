using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Message;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Xml;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace dp2Message
{
    public class BaseMsgHandler
    {
        MessageConnectionCollection _channels = null;
        private string _dp2mserverUrl = "";
        private string _logDir = "";

        // 轮循线程
        WxMsgThread _wxMsgThread = null;

        public void Init(MessageConnectionCollection channels,
            string dp2mserverUrl,
            string logDir)
        {
            this._dp2mserverUrl = dp2mserverUrl;
            this._logDir = logDir;

            //todo 现在MessageConnectionCollection是传进来的。
            this._channels = channels;

            // 接管消息事件
            this._channels.AddMessage += _channels_AddMessage;
            this._channels.ConnectionStateChange += _channels_ConnectionStateChange;
            try
            {
                // web 项目不支持这个事件
                SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            }
            catch
            { }

            // 启一个轮循线程获取消息
            this._wxMsgThread = new WxMsgThread();
            this._wxMsgThread.Container = this;
            this._wxMsgThread.BeginThread();
        }


        int _inGetMessage = 0;  // 防止因为 ConnectionStateChange 事件导致重入
        void _channels_ConnectionStateChange(object sender, ConnectionEventArgs e)
        {
            if (_inGetMessage > 0)
                return;

            if (e.Action == "Reconnected"
                || e.Action == "Connected")
            {
                FillDeltaMessage();
            }
        }

        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
                FillDeltaMessage();
        }

        void FillDeltaMessage()
        {
            Task.Factory.StartNew(() => DoLoadMessage());
        }

        /// <summary>
        /// 消息事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _channels_AddMessage(object sender, AddMessageEventArgs e)
        {
            if (e.Action != "create")
            {
                return;
            }

            if (e.Records != null)
            {
                DoMessage(e.Records, "addMessage");
            }
        }

        // 被1分钟轮循一次的工作线程调用
        public async void DoLoadMessage()
        {
            if (_inGetMessage > 0)
                return;
            _inGetMessage++;
            try
            {
                string strGroupName = "_patronNotify";//"<default>";
                string strError = "";
                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsync(
                        this._dp2mserverUrl,
                        "");
                    /*
我提前说一下：小批循环获取，如果按照一个条件，不在中途删除服务器端的消息，应该是这样循环：start=0,count=100; start=101,count=100,...
如果你每次中途都主动删除(或者失效)刚处理的这小批消息，循环就是这样了：start=0,count=100; start=0,count=100;... 明白么？分号位置是要做删除调用的
当然如果请求的条件发生变化了，就不算同一批了
多次调用只是为了避免突破内存空间问题，脑子里要清楚这个原则。
                     */
                    int batchNo = 1;//用于输出，看结果对不对
                    long totalCount = -1;//用于输出，看结果对不对 -1表示未赋值

                    int start = 0;
                    int count = 10;
                REDO:
                    CancellationToken cancel_token = new CancellationToken();
                    string id = Guid.NewGuid().ToString();
                    GetMessageRequest request = new GetMessageRequest(id,
                        "",
                        strGroupName, // "" 表示默认群组
                        "",
                        "",
                        start,
                        count);

                    // 改为同步小批循环获取。原来回调函数方案删除或失效有问题，也占响应时间
                    GetMessageResult result = connection.GetMessage(request,
                        new TimeSpan(0, 1, 0),
                        cancel_token);
                    if (result.Value == -1)
                    {
                        goto ERROR1;
                    }
                    if (result.Value == 0)
                    {
                        goto END1;
                    }

                    // 用于测试，第一次返回的记为总记录数
                    if (totalCount == -1)
                        totalCount = result.Value;

                    // 做事，发送消息给微信，里面用了expire,所以下次的start位置不变
                    if (result.Results != null && result.Results.Count > 0)
                    {
                        this.DoMessage(result.Results, "getMessage");
                    }

                    // 继续获取消息
                    if (result.Results != null && result.Results.Count > 0
                        && result.Value > result.Results.Count)
                    {
                        //start += result.Results.Count;

                        batchNo++; //用于输出，测试
                        goto REDO;
                    }

                END1:
                    // 输出一次分批获取的情况
                    this.WriteErrorLog("总记录数[" + totalCount + "],分为[" + batchNo + "]批获取完:)");
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
                this.WriteErrorLog(strError);
            }
            finally
            {
                _inGetMessage--;
            }
        }

        void OutputMessage(long totalCount,
            long start,
            IList<MessageRecord> records,
            string errorInfo,
            string errorCode)
        {
            if (totalCount == -1) // todo 什么情况下-1ko
            {
                StringBuilder text = new StringBuilder();
                text.Append("***\r\n");
                text.Append("totalCount=" + totalCount + "\r\n");
                text.Append("errorInfo=" + errorInfo + "\r\n");
                text.Append("errorCode=" + errorCode + "\r\n");
                this.WriteErrorLog(text.ToString());
                //return;
            }

            if (records != null && records.Count > 0)
            {
                DoMessage(records, "getMessage");
            }
        }

        //已处理过的消息队列
        Hashtable msgHashTable = new Hashtable();
        private bool checkMsgIsDone(string msgId)
        {
            if (msgHashTable.ContainsKey(msgId))
                return true;

            return false;
        }

        private bool AddMsgToHashTable(string msgId)
        {
            if (this.msgHashTable.ContainsKey(msgId))
                return false;

            this.msgHashTable[msgId] = DateTime.Now;
            return true;
        }

        // 处理消息锁
        private object msgLocker = new object();

        /// <summary>
        /// 实际处理通知消息
        /// </summary>
        /// <param name="records"></param>
        private void DoMessage(IList<MessageRecord> records, string from)
        {
            lock (msgLocker)
            {
                try
                {
                    if (records == null || records.Count == 0)
                        return;

                    List<string> delIds = new List<string>();
                    foreach (MessageRecord record in records)
                    {
                        // 先检查一下是不是_patronNotify组消息，因为addMessage会得到账户配置的所有组的消息，getMessage没关系只获取_patronNotify群消息
                        bool bPatronNotifyGroup = this.CheckIsNotifyGroup(record.groups);
                        if (bPatronNotifyGroup == false)
                        {
                            continue;
                        }

                        // getMessage与addMessage处理消息都会走到这里，对这段代码加锁，以保证不会重发消息。

                        this.WriteErrorLog("这次是[" + from + "]传过来的消息。");

                        // 从已处理消息队列里查重，如果是前面处理过的，则不再处理
                        bool bSended = this.checkMsgIsDone(record.id);
                        if (bSended == true)
                            continue;

                        string strError = "";
                        /// <returns>
                        /// -1 不符合条件，不处理
                        /// 0 未绑定微信id，未处理
                        /// 1 成功
                        /// </returns>
                        int nRet = this.InternalDoMessage(record, out strError);
                        if (nRet == -1)
                        {
                            this.WriteErrorLog("[" + record.id + "]出错:" + strError);
                            // todo,对于这些消息是否删除？，现在是统统删除
                        }

                        // 加到已处理消息队列里
                        this.AddMsgToHashTable(record.id);


                        // 加到删除列表
                        delIds.Add(record.id);
                    }

                    //删除处理过的消息
                    if (delIds.Count > 0)
                    {
                        string strError = "";
                        int nRet = this.DeleteMessage(delIds, out strError);
                        if (nRet == -1)
                            throw new Exception(strError);
                    }
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog(ex.Message);
                }
            }
        }


        /// <summary>
        /// 内部处理消息
        /// </summary>
        /// <param name="record"></param>
        /// <param name="strError"></param>
        /// <returns>
        /// -1 不符合条件，不处理
        /// 0 未绑定微信id，未处理
        /// 1 成功
        /// </returns>
        public virtual int InternalDoMessage(MessageRecord record, out string strError)
        {
            strError = "";
            return 1;
        }
        /// <summary>
        /// 检查是否属于通知组的消息
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private bool CheckIsNotifyGroup(string[] arrayGroup)
        {
            foreach (string s in arrayGroup)
            {
                if (s == "_patronNotify" || s == "gn:_patronNotify")
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <param name="idList"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        int DeleteMessage(List<string> idList, out string strError)
        {
            strError = "";
            if (idList.Count == 0)
                return 0;

            string strGroupName = "gn:_patronNotify";

            List<MessageRecord> records = new List<MessageRecord>();
            for (int i = 0; i < idList.Count; i++)
            {
                if (idList[i].Trim() == "")
                    continue;

                MessageRecord record = new MessageRecord();
                record.groups = strGroupName.Split(new char[] { ',' });
                record.id = idList[i].Trim();
                records.Add(record);
            }

            SetMessageRequest param = new SetMessageRequest("expire",  //delete
                "",
                records);

            try
            {
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this._dp2mserverUrl,
                    "").Result;

                SetMessageResult result = connection.SetMessageAsync(param).Result;
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }

                return 0;
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
            this.WriteErrorLog(strError);
            return -1;
        }


        public void WriteErrorLog(string strText)
        {
            string strFilename = Path.Combine(this._logDir, "error.txt");
            FileUtil.WriteLog(strFilename, strText, "dp2weixin");
        }

    }

    /// <summary>
    /// 用于搜集 dp2library 通知消息并发送给 dp2mserver 的线程
    /// </summary>
    public class WxMsgThread : ThreadBase
    {
        public BaseMsgHandler Container = null;

        public WxMsgThread()
        {
            this.PerTime = 60 * 1000;
        }

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            if (this.Stopped == true)
                return;

            Container.DoLoadMessage();
        }
    }

}
