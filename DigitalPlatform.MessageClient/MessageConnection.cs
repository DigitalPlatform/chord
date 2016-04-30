using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Net;

using Microsoft.AspNet.SignalR.Client;

using DigitalPlatform.Message;

namespace DigitalPlatform.MessageClient
{
    /// <summary>
    /// 实现热点功能的一个连接，基础类
    /// 负责处理收发消息
    /// </summary>
    public class MessageConnection
    {
        public event ConnectionEventHandler ConnectionStateChange = null;

        /// <summary>
        /// 连接的名字。用于分辨(针对同一 dp2mserver的)不同用途的连接
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 附加的信息
        /// </summary>
        public object Tag
        {
            get;
            set;
        }

        public MessageConnectionCollection Container
        {
            get;
            set;
        }

        private IHubProxy HubProxy
        {
            get;
            set;
        }

        private HubConnection Connection
        {
            get;
            set;
        }

        System.Timers.Timer _timer = new System.Timers.Timer();

        bool _exiting = false;  // 是否处在正在退出过程

        public virtual string ServerUrl
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public string Parameters
        {
            get;
            set;
        }

        public MessageConnection()
        {
            _timer.Interval = 1000 * 30;
            _timer.Elapsed += _timer_Elapsed;
        }

        // 调用前要求先设置好 this.ServerUrl this.UserName this.Password this.Parameters
        public virtual void InitialAsync()
        {
            if (string.IsNullOrEmpty(this.ServerUrl) == false)
            {
                // this.MainForm.BeginInvoke(new Action<string>(ConnectAsync), this.dp2MServerUrl);
                ConnectAsync();
            }
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            AddInfoLine("tick connection state = " + this.Connection.State.ToString());

            if (this.Connection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected)
            {
                AddInfoLine("自动重新连接 ...");
                this.EnsureConnect();
            }
        }

        public bool IsConnected
        {
            get
            {
                if (this.Connection == null)
                    return false;
                return this.Connection.State == ConnectionState.Connected;
            }
        }

        // 确保连接和登录
        public void EnsureConnect()
        {
            if (string.IsNullOrEmpty(this.ServerUrl) == true)
                throw new Exception("MessageConnection.dp2MServerUrl 尚未设置");

            if (this.Connection == null || this.Connection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected)
            {
                ConnectAsync().Wait();
            }
        }

        public virtual void Destroy()
        {
            _timer.Stop();
            _exiting = true;
            CloseConnection();
        }

        #region 显示信息

        public virtual void AddErrorLine(string strContent)
        {
            OutputText(strContent, 2);
        }

        public virtual void AddInfoLine(string strContent)
        {
            OutputText(strContent, 0);
        }

        // parameters:
        //      nWarningLevel   0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)
        /// <summary>
        /// 向控制台输出纯文本
        /// </summary>
        /// <param name="strText">要输出的纯文本字符串</param>
        /// <param name="nWarningLevel">警告级别。0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)</param>
        public virtual void OutputText(string strText, int nWarningLevel = 0)
        {
        }

        #endregion

        // 连接 server
        // 要求调用前设置好 this.ServerUrl this.UserName this.Password this.Parameters
        public Task<MessageResult> ConnectAsync(
            // string strServerUrl
            )
        {
            // 一直到真正连接前才触发登录事件
            if (this.Container != null)
                this.Container.TriggerLogin(this);

            AddInfoLine("正在连接服务器 " + this.ServerUrl + " ...");
            Connection = new HubConnection(this.ServerUrl);

            Connection.Closed += new Action(Connection_Closed);
            Connection.Reconnecting += Connection_Reconnecting;
            Connection.Reconnected += Connection_Reconnected;
            // Connection.Error += Connection_Error;

            // Connection.Credentials = new NetworkCredential("testusername", "testpassword");
            Connection.Headers.Add("username", this.UserName);
            Connection.Headers.Add("password", this.Password);
            Connection.Headers.Add("parameters", this.Parameters);

            HubProxy = Connection.CreateHubProxy("MyHub");

            HubProxy.On<string, IList<MessageRecord>>("addMessage",
                (name, messages) =>
                OnAddMessageRecieved(name, messages)
                );

            // *** search
            HubProxy.On<SearchRequest>("search",
                (param) => OnSearchRecieved(param)
                );

#if NO
            HubProxy.On<string,
    long,
    long,
    IList<Record>,
    string,
            string>("responseSearch",
    (taskID,
resultCount,
start,
records,
errorInfo,
errorCode) =>
OnResponseSearchRecieved(taskID,
resultCount,
start,
records,
errorInfo,
errorCode)
);
#endif

            // *** bindPatron
            HubProxy.On<BindPatronRequest>("bindPatron",
                (param) => OnBindPatronRecieved(param)
                );

#if NO
            HubProxy.On<string, long, IList<string>, string>("responseBindPatron",
    (taskID,
resultValue,
results,
errorInfo) =>
OnResponseBindPatronRecieved(taskID,
resultValue,
results,
errorInfo)

);
#endif

            // *** setInfo
            HubProxy.On<SetInfoRequest>("setInfo",
                (param) => OnSetInfoRecieved(param)
                );

            // *** circulation
            HubProxy.On<CirculationRequest>("circulation",
                (param) => OnCirculationRecieved(param)
                );


            try
            {
                return Connection.Start()
                    .ContinueWith<MessageResult>((antecendent) =>
                    {
                        MessageResult result = new MessageResult();
                        if (antecendent.IsFaulted == true)
                        {
#if NO
                            AddErrorLine(GetExceptionText(antecendent.Exception));
                            return;
#endif
                            result.Value = -1;
                            result.ErrorInfo = GetExceptionText(antecendent.Exception);
                            return result;
                        }
                        AddInfoLine("停止 Timer");
                        _timer.Stop();
                        AddInfoLine("成功连接到 " + this.ServerUrl);
                        // TriggerLogin();
                        TriggerConnectionStateChange("Connected");

                        return result;
                    });
            }
            catch (Exception ex)
            {
                AddErrorLine(ex.Message);
                throw ex;   // return null ?
            }
        }

#if NO
        // 连接成功后被调用，执行登录功能。重载时要调用 Login(...) 向 server 发送 login 消息
        public virtual void TriggerLogin()
        {
            this.Container.TriggerLogin(this);
        }
#endif

        public virtual void TriggerConnectionStateChange(string strAction)
        {
            // 先触发通道的事件
            ConnectionEventHandler handler = this.ConnectionStateChange;
            if (handler != null)
            {
                ConnectionEventArgs e = new ConnectionEventArgs();
                e.Action = strAction;
                handler(this, e);
            }

            // 然后触发集合的事件
            if (this.Container != null)
                this.Container.TriggerConnectionStateChange(this, strAction);
        }

        void Connection_Reconnecting()
        {
            // tryingToReconnect = true;

            // Connection.Headers 里面还保留着已经设置好的内容

            TriggerConnectionStateChange("Reconnecting");
        }

        void Connection_Reconnected()
        {
            // tryingToReconnect = false;

            AddInfoLine("Connection_Reconnected");

            // Task.Factory.StartNew(() => { Thread.Sleep(1000); this.TriggerLogin(); });

            TriggerConnectionStateChange("Reconnected");
        }

        void Connection_Closed()
        {
            if (_exiting == false)
            {
                AddInfoLine("开启 Timer");
                _timer.Start();
            }

            TriggerConnectionStateChange("Closed");
#if NO
            this.Invoke((Action)(() => panelChat.Visible = false));
            this.Invoke((Action)(() => buttonSend.Enabled = false));
            this.Invoke((Action)(() => this.labelStatusText.Text = "You have been disconnected."));
            this.Invoke((Action)(() => this.panelSignIn.Visible = true));
#endif
        }

        void Connection_Error(Exception obj)
        {
            AddErrorLine(obj.ToString());
        }

        public virtual void OnAddMessageRecieved(string action,
            IList<MessageRecord> messages)
        {
            if (this.Container != null)
            {
                AddMessageEventArgs e = new AddMessageEventArgs();
                e.Action = action;
                e.Records = new List<MessageRecord>();
                e.Records.AddRange(messages);
                this.Container.TriggerAddMessage(this, e);
            }
        }

        public delegate void Delegate_addMessage(string action, List<MessageRecord> records);

        // 加入一个消息接收处理函数
        public IDisposable AddMessageHandler(Delegate_addMessage handler)
        {
            return HubProxy.On<string, List<MessageRecord>>("addMessage",
    (action, messages) =>
    handler(action, messages)
    );
        }

        #region GetMessage() API

        public delegate void Delegate_outputMessage(long totalCount,
            long start,
            IList<MessageRecord> records,
            string errorInfo,
            string errorCode);

        public Task<MessageResult> GetMessageAsync(
            GetMessageRequest request,
            Delegate_outputMessage proc,
            TimeSpan timeout,
            CancellationToken token)
        {
            return Task.Factory.StartNew<MessageResult>(
                () =>
                {
                    MessageResult result = new MessageResult();

                    if (string.IsNullOrEmpty(request.TaskID) == true)
                        request.TaskID = Guid.NewGuid().ToString();

                    long recieved = 0;

                    using (WaitEvents wait_events = new WaitEvents())    // 表示中途数据到来
                    {
                        using (var handler = HubProxy.On<
                            string, long, long, IList<MessageRecord>, string, string>(
                            "responseGetMessage",
                            (taskID, resultCount, start, records, errorInfo, errorCode) =>
                            {
                                if (taskID != request.TaskID)
                                    return;

                                if (resultCount == -1 || start == -1)
                                {
                                    if (start == -1)
                                    {
                                        // 表示发送响应过程已经结束。只是起到通知的作用，不携带任何信息
                                        // result.Finished = true;
                                    }
                                    else
                                    {
                                        result.Value = resultCount;
                                        result.ErrorInfo = errorInfo;
                                        result.String = errorCode;
                                    }
                                    wait_events.finish_event.Set();
                                    return;
                                }

                                proc(resultCount,
                                    start,
                                    records,
                                    errorInfo,
                                    errorCode);

                                if (records != null)
                                    recieved += records.Count;

                                if (IsComplete(request.Start, request.Count, resultCount, recieved) == true)
                                    wait_events.finish_event.Set();
                                else
                                    wait_events.active_event.Set();
                            }))
                        {
                            MessageResult temp = HubProxy.Invoke<MessageResult>(
"RequestGetMessage",
request).Result;
                            if (temp.Value == -1 || temp.Value == 0)
                                return temp;

                            // result.String 里面是返回的 taskID

                            Wait(
            request.TaskID,
            wait_events,
            timeout,
            token);
                            return result;
                        }
                    }
                },
            token);
        }

        #endregion

        #region SetMessage() API

        public Task<SetMessageResult> SetMessageAsync(SetMessageRequest param)
        {
            return HubProxy.Invoke<SetMessageResult>(
 "SetMessage",
 param);
        }

        #endregion

        #region GetConnectionInfo() API

        public Task<GetConnectionInfoResult> GetConnectionInfoAsync(
    GetConnectionInfoRequest request,
    TimeSpan timeout,
    CancellationToken token)
        {
            return Task.Factory.StartNew<GetConnectionInfoResult>(
                () =>
                {
                    GetConnectionInfoResult result = new GetConnectionInfoResult();
                    if (result.Records == null)
                        result.Records = new List<ConnectionRecord>();

                    if (string.IsNullOrEmpty(request.TaskID) == true)
                        request.TaskID = Guid.NewGuid().ToString();

                    using (WaitEvents wait_events = new WaitEvents())    // 表示中途数据到来
                    {
                        using (var handler = HubProxy.On<
                            string, long, long, IList<ConnectionRecord>, string, string>(
                            "responseGetConnectionInfo",
                            (taskID, resultCount, start, records, errorInfo, errorCode) =>
                            {
                                if (taskID != request.TaskID)
                                    return;

                                // 装载命中结果
                                if (resultCount == -1 || start == -1)
                                {
                                    if (start == -1)
                                    {
                                        // 表示发送响应过程已经结束。只是起到通知的作用，不携带任何信息
                                        // result.Finished = true;
                                    }
                                    else
                                    {
                                        result.ResultCount = resultCount;
                                        result.ErrorInfo = errorInfo;
                                        result.ErrorCode = errorCode;
                                    }
                                    wait_events.finish_event.Set();
                                    return;
                                }

                                result.ResultCount = resultCount;
                                // TODO: 似乎应该关注 start 位置
                                result.Records.AddRange(records);
                                result.ErrorInfo = errorInfo;
                                result.ErrorCode = errorCode;

                                if (IsComplete(request.Start, request.Count, resultCount, result.Records.Count) == true)
                                    wait_events.finish_event.Set();
                                else
                                    wait_events.active_event.Set();
                            }))
                        {
                            MessageResult message = HubProxy.Invoke<MessageResult>(
                "RequestGetConnectionInfo",
                request).Result;
                            if (message.Value == -1 || message.Value == 0)
                            {
                                result.ErrorInfo = message.ErrorInfo;
                                result.ResultCount = -1;
                                return result;
                            }

                            // result.String 里面是返回的 taskID

                            // start_time = DateTime.Now;

                            Wait(
            request.TaskID,
            wait_events,
            timeout,
            token);
                            return result;
                        }
                    }
                },
            token);
        }

        #endregion

        #region Search() API
        // 
        // 当 server 发来检索请求的时候被调用。重载的时候要进行检索，并调用 Response 把检索结果发送给 server
        public virtual void OnSearchRecieved(SearchRequest param)
        {
        }

        // 新版 API，测试中
        public Task<SearchResult> SearchAsync(
    string strRemoteUserName,
    SearchRequest request,
    TimeSpan timeout,
    CancellationToken token)
        {
            return Task.Factory.StartNew<SearchResult>(
                () =>
                {
                    // DateTime start_time = DateTime.Now;

                    SearchResult result = new SearchResult();
                    if (result.Records == null)
                        result.Records = new List<Record>();

                    if (string.IsNullOrEmpty(request.TaskID) == true)
                        request.TaskID = Guid.NewGuid().ToString();

                    using (WaitEvents wait_events = new WaitEvents())    // 表示中途数据到来
                    {
                        using (var handler = HubProxy.On<
                            string, long, long, IList<Record>, string, string>(
                            "responseSearch",
                            (taskID, resultCount, start, records, errorInfo, errorCode) =>
                            {
                                if (taskID != request.TaskID)
                                    return;

                                // start_time = DateTime.Now;  // 重新计算超时

                                // 装载命中结果
                                if (resultCount == -1 || start == -1)
                                {
                                    if (start == -1)
                                    {
                                        // 表示发送响应过程已经结束。只是起到通知的作用，不携带任何信息
                                        // result.Finished = true;
                                    }
                                    else
                                    {
                                        result.ResultCount = resultCount;
                                        result.ErrorInfo = errorInfo;
                                        result.ErrorCode = errorCode;
                                    }
                                    wait_events.finish_event.Set();
                                    return;
                                }

                                result.ResultCount = resultCount;
                                // TODO: 似乎应该关注 start 位置
                                result.Records.AddRange(records);
                                result.ErrorInfo = errorInfo;
                                result.ErrorCode = errorCode;

                                if (IsComplete(request.Start, request.Count, resultCount, result.Records.Count) == true)
                                    wait_events.finish_event.Set();
                                else
                                    wait_events.active_event.Set();
                            }))
                        {
                            MessageResult message = HubProxy.Invoke<MessageResult>(
                "RequestSearch",
                strRemoteUserName,
                request).Result;
                            if (message.Value == -1 || message.Value == 0)
                            {
                                result.ErrorInfo = message.ErrorInfo;
                                result.ResultCount = -1;
                                return result;
                            }

                            // start_time = DateTime.Now;

                            Wait(
            request.TaskID,
            wait_events,
            timeout,
            token);
                            return result;
                        }
                    }
                },
            token);
        }

        class WaitEvents : IDisposable
        {
            public ManualResetEvent finish_event = new ManualResetEvent(false);    // 表示数据全部到来
            public AutoResetEvent active_event = new AutoResetEvent(false);    // 表示中途数据到来

            public virtual void Dispose()
            {
                finish_event.Dispose();
                active_event.Dispose();
            }
        }

        void Wait(string taskID,
            WaitEvents wait_events,
            TimeSpan timeout,
            CancellationToken cancellation_token)
        {
            DateTime start_time = DateTime.Now; // 其实可以不用

            WaitHandle[] events = null;

            if (cancellation_token != null)
            {
                events = new WaitHandle[3];
                events[0] = wait_events.finish_event;
                events[1] = wait_events.active_event;
                events[2] = cancellation_token.WaitHandle;
            }
            else
            {
                events = new WaitHandle[2];
                events[0] = wait_events.finish_event;
                events[1] = wait_events.active_event;
            }

            while (true)
            {
                int index = WaitHandle.WaitAny(events,
                    timeout,
                    false);

                if (index == 0) // 正常完成
                    return; //  result;
                else if (index == 1)
                {
                    start_time = DateTime.Now;  // 重新计算超时开始时刻
                }
                else if (index == 2)
                {
                    if (cancellation_token != null)
                    {
                        // 向服务器发送 CancelSearch 请求
                        CancelSearchAsync(taskID);
                        cancellation_token.ThrowIfCancellationRequested();
                    }
                }
                else if (index == WaitHandle.WaitTimeout)
                {
                    // if (DateTime.Now - start_time >= timeout)
                    {
                        // 向服务器发送 CancelSearch 请求
                        CancelSearchAsync(taskID);
                        throw new TimeoutException("已超时 " + timeout.ToString());
                    }
                }
            }
        }

#if NO
        // 进行检索并得到结果
        // 这是将发出和接受消息结合起来的功能比较完整的 API
        public Task<SearchResult> SearchAsync(
            string strRemoteUserName,
            SearchRequest request,
            TimeSpan timeout,
            CancellationToken token)
        {
            return Task.Factory.StartNew<SearchResult>(() =>
            {
                SearchResult result = new SearchResult();

                if (string.IsNullOrEmpty(request.TaskID) == true)
                    request.TaskID = Guid.NewGuid().ToString();

                MessageResult message = HubProxy.Invoke<MessageResult>(
    "RequestSearch",
    strRemoteUserName,
    request).Result;
                if (message.Value == -1 || message.Value == 0)
                {
                    result.ErrorInfo = message.ErrorInfo;
                    result.ResultCount = -1;
                    return result;
                }

                DateTime start_time = DateTime.Now;

                // TODO: start_time 要被每次到来数据时候重新设置为当时时间。这样就重新开始计算超时。等于是从最近一次获取到数据开始计算超时

                // 循环，取出得到的检索结果
                for (; ; )
                {
                    if (token != null)
                        token.ThrowIfCancellationRequested();

                    if (DateTime.Now - start_time >= timeout)
                        throw new TimeoutException("已超时 " + timeout.ToString());

                    SearchResult result0 = (SearchResult)_resultTable[request.TaskID];
                    if (result0 != null && result0.ResultCount == -1)
                    {
                        ClearResultFromTable(request.TaskID);
                        return result0;
                    }

                    if (result0 != null && result0.Records != null
                        && IsComplete(request.Start,
            request.Count,
            result0.ResultCount,
            result0.Records.Count)
                        // && result0.Records.Count >= result0.ResultCount
                        )    // request.Start 不一定是从头开始，也不一定要获取到最末尾
                    {
                        ClearResultFromTable(request.TaskID);
                        return result0;
                    }

                    if (result0 != null && result0.Finished == true)
                    {
                        ClearResultFromTable(request.TaskID);
                        return result0;
                    }

                    Thread.Sleep(200);
                }
            }, token);
        }

#endif

        static bool IsComplete(long requestStart,
            long requestCount,
            long totalCount,
            long recordsCount)
        {
            long tail = 0;
            if (requestCount != -1)
                tail = Math.Min(requestStart + requestCount, totalCount);
            else
                tail = totalCount;

            if (requestStart + recordsCount >= totalCount)
                return true;
            return false;
        }

#if NO
        // TODO: 按照 searchID 把检索结果一一存储起来。用信号通知消费线程。消费线程每次可以取走一部分，以后每一次就取走余下的。
        // 当 server 发来检索响应的时候被调用。重载时可以显示收到的记录
        public virtual void OnResponseSearchRecieved(string taskID,
    long resultCount,
    long start,
    IList<Record> records,
    string errorInfo,
            string errorCode)   // 2016/4/15 增加
        {
            Debug.WriteLine("OnResponseSearchRecieved taskID=" + taskID + ", start=" + start + ", records.Count="
                + (records == null ? "null" : records.Count.ToString())
                + ", errorInfo=" + errorInfo
                + ", errorCode=" + errorCode);
            lock (_resultTable)
            {

                // TODO: 监视 Hashtable 中的元素数量，超过一个极限值要抛出异常

                SearchResult result = (SearchResult)_resultTable[taskID];
                if (result == null)
                {
                    result = new SearchResult();
                    _resultTable[taskID] = result;
                }

                if (result.Records == null)
                    result.Records = new List<Record>();

                if (resultCount == -1 && start == -1)
                {
                    // 表示发送响应过程已经结束
                    result.Finished = true;
                    return;
                }

                result.ResultCount = resultCount;
                // TODO: 似乎应该关注 start 位置
                result.Records.AddRange(records);
                result.ErrorInfo = errorInfo;
                result.ErrorCode = errorCode;   // 2016/4/15 增加
            }
        }

#endif

        #endregion

        #region SetInfo() API

        public virtual void OnSetInfoRecieved(SetInfoRequest param)
        {
        }

        public Task<SetInfoResult> SetInfoAsync(
    string strRemoteUserName,
    SetInfoRequest request,
    TimeSpan timeout,
    CancellationToken token)
        {
            return Task.Factory.StartNew<SetInfoResult>(() =>
            {
                SetInfoResult result = new SetInfoResult();
                if (result.Entities == null)
                    result.Entities = new List<Entity>();

                if (string.IsNullOrEmpty(request.TaskID) == true)
                    request.TaskID = Guid.NewGuid().ToString();

                using (WaitEvents wait_events = new WaitEvents())
                {
                    using (var handler = HubProxy.On<
                        string, long, IList<Entity>, string>(
                        "responseSetInfo",
                        (taskID, resultValue, entities, errorInfo) =>
                        {
                            if (taskID != request.TaskID)
                                return;

                            // 装载命中结果
                            if (entities != null)
                                result.Entities.AddRange(entities);
                            result.Value = resultValue;
                            result.ErrorInfo = errorInfo;
                            wait_events.finish_event.Set();
                        }))
                    {
                        MessageResult message = HubProxy.Invoke<MessageResult>(
            "RequestSetInfo",
            strRemoteUserName,
            request).Result;
                        if (message.Value == -1
                            || message.Value == 0)
                        {
                            result.ErrorInfo = message.ErrorInfo;
                            result.Value = -1;
                            return result;
                        }

#if NO
                        WaitHandle[] events = null;
                        if (token != null)
                        {
                            events = new WaitHandle[2];
                            events[0] = finish_event;
                            events[1] = token.WaitHandle;
                        }
                        else
                        {
                            events = new WaitHandle[1];
                            events[0] = finish_event;
                        }

                        int index = WaitHandle.WaitAny(events, timeout, false);
                        if (index == WaitHandle.WaitTimeout)
                        {
                            // 向服务器发送 CancelSearch 请求
                            CancelSearchAsync(request.TaskID);
                            throw new TimeoutException("已超时 " + timeout.ToString());
                        }

                        if (index == 0) // 正常完成
                            return result;
                        if (token != null)
                        {
                            // 向服务器发送 CancelSearch 请求
                            CancelSearchAsync(request.TaskID);
                            token.ThrowIfCancellationRequested();
                        }
                        result.ErrorInfo += "_error";
                        return result;
#endif
                        Wait(
        request.TaskID,
        wait_events,
        timeout,
        token);
                        return result;
                    }
                }
            }, token);
        }

        #endregion

        #region BindPatron() API

        public virtual void OnBindPatronRecieved(BindPatronRequest param)
        {
        }

        public Task<BindPatronResult> BindPatronAsync(
    string strRemoteUserName,
    BindPatronRequest request,
    TimeSpan timeout,
    CancellationToken token)
        {
            return Task.Factory.StartNew<BindPatronResult>(() =>
            {
                BindPatronResult result = new BindPatronResult();
                if (result.Results == null)
                    result.Results = new List<string>();

                if (string.IsNullOrEmpty(request.TaskID) == true)
                    request.TaskID = Guid.NewGuid().ToString();

                using (WaitEvents wait_events = new WaitEvents())
                {
                    using (var handler = HubProxy.On<
                        string, long, IList<string>, string>(
                        "responseBindPatron",
                        (taskID, resultValue, results, errorInfo) =>
                        {
                            if (taskID != request.TaskID)
                                return;

                            // 装载命中结果
                            if (results != null)
                                result.Results.AddRange(results);
                            result.Value = resultValue;
                            result.ErrorInfo = errorInfo;
                            wait_events.finish_event.Set();
                        }))
                    {
                        MessageResult message = HubProxy.Invoke<MessageResult>(
            "RequestBindPatron",
            strRemoteUserName,
            request).Result;
                        if (message.Value == -1
                            || message.Value == 0)
                        {
                            result.ErrorInfo = message.ErrorInfo;
                            result.Value = -1;
                            return result;
                        }

#if NO
                        WaitHandle[] events = null;
                        if (token != null)
                        {
                            events = new WaitHandle[2];
                            events[0] = finish_event;
                            events[1] = token.WaitHandle;
                        }
                        else
                        {
                            events = new WaitHandle[1];
                            events[0] = finish_event;
                        }

                        int index = WaitHandle.WaitAny(events, timeout, false);
                        if (index == WaitHandle.WaitTimeout)
                        {
                            // 向服务器发送 CancelSearch 请求
                            CancelSearchAsync(request.TaskID);
                            throw new TimeoutException("已超时 " + timeout.ToString());
                        }

                        if (index == 0) // 正常完成
                            return result;
                        if (token != null)
                        {
                            // 向服务器发送 CancelSearch 请求
                            CancelSearchAsync(request.TaskID);
                            token.ThrowIfCancellationRequested();
                        }
                        result.ErrorInfo += "_error";
                        return result;
#endif
                        Wait(
request.TaskID,
wait_events,
timeout,
token);
                        return result;
                    }
                }
            }, token);
        }

#if NO
        public Task<BindPatronResult> BindPatronAsync(
            string strRemoteUserName,
            BindPatronRequest request,
            TimeSpan timeout,
            CancellationToken token)
        {
            return Task.Factory.StartNew<BindPatronResult>(() =>
            {
                BindPatronResult result = new BindPatronResult();

                if (string.IsNullOrEmpty(request.TaskID) == true)
                    request.TaskID = Guid.NewGuid().ToString();

                MessageResult message = HubProxy.Invoke<MessageResult>(
    "RequestBindPatron",
    strRemoteUserName,
    request).Result;
                if (message.Value == -1
                    || message.Value == 0)
                {
                    result.ErrorInfo = message.ErrorInfo;
                    result.Value = -1;
                    return result;
                }

                DateTime start_time = DateTime.Now;

                // 循环，取出得到的检索结果
                for (; ; )
                {
                    if (token != null)
                        token.ThrowIfCancellationRequested();

                    if (DateTime.Now - start_time >= timeout)
                        throw new TimeoutException("已超时 " + timeout.ToString());

                    BindPatronResult result0 = (BindPatronResult)_resultTable[request.TaskID];
                    if (result0 != null)
                    {
                        ClearResultFromTable(request.TaskID);
                        return result0;
                    }

                    Thread.Sleep(200);
                }
            }, token);

            // TODO: 超时以后到来的结果，放入 hashtable 以后，时间长了谁来清理？可能还是需要一个专门的线程来做清理
            // 或者超时的时候，在 Hashtable 中放入一个占位事项，后面响应到来的时候看到这个占位事项就知道已经超时了，需要把事项清除。但，如果响应始终不来呢？
        }
#endif

#if NO
        // 当 server 发来检索响应的时候被调用。重载时可以显示收到的记录
        // 按照 searchID 把返回的唯一结果存储起来。消费线程一旦发现有了这个事项，就表明请求得到了响应，可取走结果，注意要从 Hashtable 里面删除结果，避免长期运行后堆积占据空间
        public virtual void OnResponseBindPatronRecieved(string taskID,
    long resultValue,
    IList<string> results,
    string errorInfo)
        {
            lock (_resultTable)
            {
                BindPatronResult result = (BindPatronResult)_resultTable[taskID];
                if (result == null)
                {
                    result = new BindPatronResult();
                    _resultTable[taskID] = result;
                }

                if (result.Results == null)
                    result.Results = new List<string>();

                result.Results.AddRange(results);
                result.Value = resultValue;
                result.ErrorInfo = errorInfo;
            }
        }
#endif

        #endregion

        #region Circulation() API

        public virtual void OnCirculationRecieved(CirculationRequest param)
        {
        }

        public Task<CirculationResult> CirculationAsync(
    string strRemoteUserName,
    CirculationRequest request,
    TimeSpan timeout,
    CancellationToken token)
        {
            return Task.Factory.StartNew<CirculationResult>(() =>
            {
                CirculationResult result = new CirculationResult();

                if (string.IsNullOrEmpty(request.TaskID) == true)
                    request.TaskID = Guid.NewGuid().ToString();

                using (WaitEvents wait_events = new WaitEvents())
                {
                    using (var handler = HubProxy.On<
                        string, CirculationResult>(
                        "responseCirculation",
                        (taskID, circulation_result) =>
                        {
                            if (taskID != request.TaskID)
                                return;

                            // 装载命中结果
                            result = circulation_result;
                            wait_events.finish_event.Set();
                        }))
                    {
                        MessageResult message = HubProxy.Invoke<MessageResult>(
            "RequestCirculation",
            strRemoteUserName,
            request).Result;
                        if (message.Value == -1
                            || message.Value == 0)
                        {
                            result.ErrorInfo = message.ErrorInfo;
                            result.Value = -1;
                            return result;
                        }

#if NO
                        WaitHandle[] events = null;
                        if (token != null)
                        {
                            events = new WaitHandle[2];
                            events[0] = finish_event;
                            events[1] = token.WaitHandle;
                        }
                        else
                        {
                            events = new WaitHandle[1];
                            events[0] = finish_event;
                        }

                        int index = WaitHandle.WaitAny(events, timeout, false);
                        if (index == WaitHandle.WaitTimeout)
                        {
                            // 向服务器发送 CancelSearch 请求
                            CancelSearchAsync(request.TaskID);
                            throw new TimeoutException("已超时 " + timeout.ToString());
                        }

                        if (index == 0) // 正常完成
                            return result;
                        if (token != null)
                        {
                            // 向服务器发送 CancelSearch 请求
                            CancelSearchAsync(request.TaskID);
                            token.ThrowIfCancellationRequested();
                        }
                        result.ErrorInfo += "_error";
                        return result;
#endif
                        Wait(
request.TaskID,
wait_events,
timeout,
token);
                        return result;
                    }
                }
            }, token);
        }

        #endregion

#if NO
        Hashtable _resultTable = new Hashtable();   // taskID --> SearchResult 

        // 从结果集表中移走结果
        void ClearResultFromTable(string taskID)
        {
            _resultTable.Remove(taskID);
        }
#endif

        // 关闭连接，并且不会引起自动重连接
        public void CloseConnection()
        {
            if (this.Connection != null)
            {
                // HubProxy.Invoke<MessageResult>("Logout").Wait(500);

                Connection.Closed -= new Action(Connection_Closed);
                /*
操作类型 crashReport -- 异常报告 
主题 dp2circulation 
发送者 xxxxxxx
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.NullReferenceException
Message: 未将对象引用设置到对象的实例。
Stack:
在 Microsoft.AspNet.SignalR.Client.Connection.Stop(TimeSpan timeout)
在 dp2Circulation.MessageHub.CloseConnection()
在 dp2Circulation.MessageHub.Close()
在 dp2Circulation.MainForm.MainForm_FormClosed(Object sender, FormClosedEventArgs e)
在 System.Windows.Forms.Form.OnFormClosed(FormClosedEventArgs e)
在 System.Windows.Forms.Form.WmClose(Message& m)
在 System.Windows.Forms.Form.WndProc(Message& m)
在 dp2Circulation.MainForm.WndProc(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Circulation 版本: dp2Circulation, Version=2.4.5697.17821, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 5.1.2600 Service Pack 3 
操作时间 2015/8/7 10:51:56 (Fri, 07 Aug 2015 10:51:56 +0800) 
前端地址 xxxxx 经由 http://dp2003.com/dp2library 

                 * 
                 * */
                try
                {
                    this.Connection.Stop(new TimeSpan(0, 0, 5));
                }
                catch (System.NullReferenceException)
                {
                }
                this.Connection = null;
            }
        }

#if NO
        // 发起一次书目检索
        // 发起检索成功后，调主应该用 SearchResponseEvent 事件接收检索结果
        // return:
        //      -1  出错
        //      0   没有检索目标
        //      1   成功发起检索
        public int BeginSearchBiblio(
            string inputSearchID,
            string dbNameList,
            string queryWord,
            string fromList,
            string matchStyle,
            string formatList,
            long maxResults,
            out string outputSearchID,
            out string strError)
        {
            strError = "";
            outputSearchID = "";

            try
            {

                Task<MessageResult> task = HubProxy.Invoke<MessageResult>("RequestSearchBiblio",
                    inputSearchID,
                    dbNameList,
                    queryWord,
                    fromList,
                    matchStyle,
                    formatList,
                    maxResults);

                while (task.IsCompleted == false)
                {
                    Application.DoEvents();
                    Thread.Sleep(200);
                }

                if (task.IsFaulted == true)
                {
                    // AddErrorLine(GetExceptionText(task.Exception));
                    strError = GetExceptionText(task.Exception);
                    return -1;
                }

                MessageResult result = task.Result;
                if (result.Value == -1)
                {
                    // AddErrorLine(result.ErrorInfo);
                    strError = result.ErrorInfo;
                    return -1;
                }
                if (result.Value == 0)
                {
                    // AddErrorLine(result.ErrorInfo);
                    strError = result.ErrorInfo;
                    return 0;
                }
                // AddMessageLine("search ID:", result.String);
                outputSearchID = result.String;
                return 1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }
#endif
        public static string GetExceptionText(AggregateException exception)
        {
            StringBuilder text = new StringBuilder();
            foreach (Exception ex in exception.InnerExceptions)
            {
                if (ex is AggregateException)
                    text.Append(GetExceptionText(ex as AggregateException));
                else
                    text.Append(ex.Message + "\r\n");
                // text.Append(ex.ToString() + "\r\n");
            }

            return text.ToString();
        }

        // 等待 Task 结束。重载时可以在其中加入出让界面控制权，或者显示进度的功能
        public virtual void WaitTaskComplete(Task task)
        {
#if NO
            while (task.IsCompleted == false)
            {
                Application.DoEvents();
                Thread.Sleep(200);
            }
#endif
            task.Wait();
        }

        #region 调用 Server 端函数 (直接调用的浅包装)

        // 发起一次书目检索
        // 这是比较原始的 API，并不负责接收对方传来的消息
        // result.Value:
        //      -1  出错
        //      0   没有检索目标
        //      1   成功发起检索。此时 Result.String 里面返回了 taskID
        public Task<MessageResult> SearchAsync(
            string userNameList,
            SearchRequest searchParam)
        {
            return HubProxy.Invoke<MessageResult>(
                "RequestSearch",
                userNameList,
                searchParam);
        }

        // 请求服务器中断一个 task
        public Task<MessageResult> CancelSearchAsync(string taskID)
        {
            return HubProxy.Invoke<MessageResult>(
                "CancelSearch",
                taskID);
        }

#if NO
        // 发起一次书目检索
        // 发起检索成功后，调主应该用 SearchResponseEvent 事件接收检索结果
        // return:
        //      -1  出错
        //      0   没有检索目标
        //      1   成功发起检索
        public int BeginSearchBiblio(
            string userNameList,
#if NO
            string operation,
            string inputSearchID,
            string dbNameList,
            string queryWord,
            string fromList,
            string matchStyle,
            string formatList,
            long maxResults,
#endif
 SearchRequest searchParam,
            out string outputSearchID,
            out string strError)
        {
            strError = "";
            outputSearchID = "";

            AddInfoLine("BeginSearchBiblio "
                + "; userNameList=" + userNameList
#if NO
                + "; operation=" + operation
                + "; searchID=" + inputSearchID
                + "; dbNameList=" + dbNameList
                + "; queryWord=" + queryWord
                + "; fromList=" + fromList
                + "; matchStyle=" + matchStyle
                + "; formatList=" + formatList
                + "; maxResults=" + maxResults
#endif
);
            try
            {
                Task<MessageResult> task = HubProxy.Invoke<MessageResult>("RequestSearchBiblio",
                    userNameList,
#if NO
                    inputSearchID,
                    dbNameList,
                    queryWord,
                    fromList,
                    matchStyle,
                    formatList,
                    maxResults
#endif
 searchParam
                    );

#if NO
                while (task.IsCompleted == false)
                {
                    Application.DoEvents();
                    Thread.Sleep(200);
                }
#endif
                WaitTaskComplete(task);

                if (task.IsFaulted == true)
                {
                    // AddErrorLine(GetExceptionText(task.Exception));
                    strError = GetExceptionText(task.Exception);
                    AddInfoLine("BeginSearchBiblio inputSearchID=" + searchParam.SearchID
    + "; return error=" + strError + " value="
    + -1);
                    return -1;
                }

                MessageResult result = task.Result;
                if (result.Value == -1)
                {
                    // AddErrorLine(result.ErrorInfo);
                    strError = result.ErrorInfo;
                    AddInfoLine("BeginSearchBiblio inputSearchID=" + searchParam.SearchID
    + "; return error=" + strError + " value="
    + -1);
                    return -1;
                }
                if (result.Value == 0)
                {
                    // AddErrorLine(result.ErrorInfo);
                    strError = result.ErrorInfo;
                    AddInfoLine("BeginSearchBiblio inputSearchID=" + searchParam.SearchID
   + "; return error=" + strError + " value="
   + 0);
                    return 0;
                }
                // AddMessageLine("search ID:", result.String);
                outputSearchID = result.String;
                AddInfoLine("BeginSearchBiblio inputSearchID=" + searchParam.SearchID
+ "; return value="
+ 1);
                return 1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;  //  ExceptionUtil.GetAutoText(ex);
                AddInfoLine("BeginSearchBiblio inputSearchID=" + searchParam.SearchID
+ "; return error=" + strError + " value="
+ -1);
                return -1;
            }
        }
#endif

        // 调用 server 端 ResponseCirculation
        public Task<MessageResult> ResponseCirculationAsync(
            string taskID,
            CirculationResult circulation_result)
        {
            return HubProxy.Invoke<MessageResult>("ResponseCirculation",
taskID,
circulation_result);
        }


        // 调用 server 端 ResponseCirculation
        public bool TryResponseCirculation(
            string taskID,
            CirculationResult circulation_result)
        {
            try
            {
                Wait(new TimeSpan(0, 0, 0, 0, 50));

                MessageResult result = ResponseCirculationAsync(
                    taskID,
                    circulation_result).Result;
                _lastTime = DateTime.Now;
                if (result.Value == -1)
                    return false;   // 可能因为服务器端已经中断此 taskID，或者执行 ReponseSearch() 时出错
                return true;
            }
            catch (Exception ex)
            {
                // 报错
                CirculationResult error = new CirculationResult();
                error.Value = -1;
                error.ErrorInfo = ex.Message;
                error.String = "_sendResponseCirculationError";
                ResponseCirculationAsync(
    taskID,
    error);    // 消息层面发生的错误(表示不是 dp2library 层面的错误)，错误码为 _ 开头
                return false;
            }
        }

        // 调用 server 端 ResponseBindPatron
        public async void ResponseBindPatron(
            string taskID,
            long resultValue,
            IList<string> results,
            string errorInfo)
        {
            try
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("ResponseBindPatron",
    taskID,
    resultValue,
    results,
    errorInfo);
                if (result.Value == -1)
                {
                    AddErrorLine(result.ErrorInfo);
                    return;
                }
            }
            catch (Exception ex)
            {
                AddErrorLine(ex.Message);
            }
        }

        // 调用 server 端 ResponseSetInfo
        // TODO: 要考虑发送失败的问题
        public async void ResponseSetInfo(
            string taskID,
            long resultValue,
            IList<Entity> results,
            string errorInfo)
        {
            try
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("ResponseSetInfo",
    taskID,
    resultValue,
    results,
    errorInfo);
                if (result.Value == -1)
                {
                    AddErrorLine(result.ErrorInfo);
                    return;
                }
            }
            catch (Exception ex)
            {
                AddErrorLine(ex.Message);
            }
        }

        DateTime _lastTime = DateTime.Now;

        // 和上次操作的时刻之间，等待至少这么多时间。
        void Wait(TimeSpan length)
        {
            DateTime now = DateTime.Now;
            TimeSpan delta = now - _lastTime;
            if (delta < length)
            {
                // Console.WriteLine("Sleep " + (length - delta).ToString());
                Thread.Sleep(length - delta);
            }
            _lastTime = DateTime.Now;
        }

        // TODO: 注意测试，一次只能发送一个元素，或者连一个元素都发送不成功的情况
        // 具有重试机制的 ReponseSearch
        // 运行策略是，当遇到 InvalidOperationException 异常时，减少一半数量重试发送，用多次小批发送解决问题
        // 如果最终无法完成发送，则尝试发送一条报错信息，然后返回 false
        // parameters:
        //      batch_size  建议的最佳一次发送数目。-1 表示不限制
        // return:
        //      true    成功
        //      false   失败
        public bool TryResponseSearch(string taskID,
            long resultCount,
            long start,
            IList<Record> records,
            string errorInfo,
            string errorCode,
            ref long batch_size)
        {
            string strError = "";

            List<Record> rest = new List<Record>(); // 等待发送的
            List<Record> current = new List<Record>();  // 当前正在发送的
            if (batch_size == -1)
                current.AddRange(records);
            else
            {
                rest.AddRange(records);

                // 将最多 batch_size 个元素从 rest 中移动到 current 中
                for (int i = 0; i < batch_size && rest.Count > 0; i++)
                {
                    current.Add(rest[0]);
                    rest.RemoveAt(0);
                }
            }

            long send = 0;  // 已经发送过的元素数
            while (current.Count > 0)
            {
                try
                {
                    Wait(new TimeSpan(0, 0, 0, 0, 50));

                    MessageResult result = ResponseSearchAsync(
                        taskID,
                        resultCount,
                        start + send,
                        current,
                        errorInfo,
                        errorCode).Result;
                    _lastTime = DateTime.Now;
                    if (result.Value == -1)
                        return false;   // 可能因为服务器端已经中断此 taskID，或者执行 ReponseSearch() 时出错
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is InvalidOperationException)
                    {
                        if (current.Count == 1)
                        {
                            strError = "向中心发送 ResponseSearch 消息时出现异常(连一个元素也发送不出去): " + ex.InnerException.Message;
                            goto ERROR1;
                        }
                        // 减少一半元素发送
                        int half = Math.Max(1, current.Count / 2);
                        int offs = current.Count - half;
                        for (int i = 0; current.Count > offs; i++)
                        {
                            Record record = current[offs];
                            rest.Insert(i, record);
                            current.RemoveAt(offs);
                        }
                        batch_size = half;
                        continue;
                    }

                    strError = "向中心发送 ResponseSearch 消息时出现异常: " + ExceptionUtil.GetExceptionText(ex);
                    goto ERROR1;
                }

                Console.WriteLine("成功发送 " + current.Count.ToString());

                send += current.Count;
                current.Clear();
                if (batch_size == -1)
                    current.AddRange(rest);
                else
                {
                    // 将最多 batch_size 个元素从 rest 中移动到 current 中
                    for (int i = 0; i < batch_size && rest.Count > 0; i++)
                    {
                        current.Add(rest[0]);
                        rest.RemoveAt(0);
                    }
                }
            }

            Debug.Assert(rest.Count == 0, "");
            Debug.Assert(current.Count == 0, "");
            return true;
        ERROR1:
            // 报错
            ResponseSearch(
taskID,
-1,
0,
new List<Record>(),
strError,
"_sendResponseSearchError");    // 消息层面发生的错误(表示不是 dp2library 层面的错误)，错误码为 _ 开头
            return false;
        }

        // 调用 server 端 ResponseSearchBiblio
        public Task<MessageResult> ResponseSearchAsync(
            string taskID,
            long resultCount,
            long start,
            IList<Record> records,
            string errorInfo,
            string errorCode)
        {
            return HubProxy.Invoke<MessageResult>("ResponseSearch",
taskID,
resultCount,
start,
records,
errorInfo,
errorCode);
        }

        // 调用 server 端 ResponseSearchBiblio
        public async void ResponseSearch(
            string taskID,
            long resultCount,
            long start,
            IList<Record> records,
            string errorInfo,
            string errorCode)
        {
            // TODO: 等待执行完成。如果有异常要当时处理。比如减小尺寸重发。
            int nRedoCount = 0;
        REDO:
            try
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("ResponseSearch",
    taskID,
    resultCount,
    start,
    records,
    errorInfo,
    errorCode);
                if (result.Value == -1)
                {
                    AddErrorLine(result.ErrorInfo);
                    return;
                }
            }
            catch (Exception ex)
            {
                AddErrorLine(ex.Message);
                if (ex.InnerException is InvalidOperationException
                    && nRedoCount < 2)
                {
                    nRedoCount++;
                    Thread.Sleep(1000);
                    goto REDO;
                }
            }
        }

        public GetUserResult GetUsers(string userName, int start, int count)
        {
            var task = HubProxy.Invoke<GetUserResult>("GetUsers",
                userName,
                start,
                count);
            task.Wait();
            return task.Result;
        }

        public MessageResult SetUsers(string action, List<User> users)
        {
            var task = HubProxy.Invoke<MessageResult>("SetUsers",
                action,
                users);
            task.Wait();
            return task.Result;
        }

        // 调用 server 端 Login
        public Task<MessageResult> LoginAsync(
#if NO
            string userName,
            string password,
            string libraryUID,
            string libraryName,
            string propertyList
#endif
LoginRequest param)
        {
            return HubProxy.Invoke<MessageResult>("Login",
#if NO
                userName,
                password,
                libraryUID,
                libraryName,
                propertyList
#endif
 param);
        }

        #endregion
    }

    public class SearchResult
    {
        public long ResultCount = 0;
        public List<Record> Records = null;
        public string ErrorInfo = "";
        public string ErrorCode = "";   // 2016/4/15 增加
        // public bool Finished = false;
    }

    public class GetConnectionInfoResult
    {
        public long ResultCount = 0;
        public List<ConnectionRecord> Records = null;
        public string ErrorInfo = "";
        public string ErrorCode = "";
    }
}
