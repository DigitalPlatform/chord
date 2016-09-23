// #define FIX_HANDLER
// #define VERIFY_CHUNK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Net.Http;

using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;

using Nito.AsyncEx;

using DigitalPlatform.Message;
using DigitalPlatform.Text;
using DigitalPlatform.Common;

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

#if NO
        // 调用前要求先设置好 this.ServerUrl this.UserName this.Password this.Parameters
        public virtual async void InitialAsync()
        {
            if (string.IsNullOrEmpty(this.ServerUrl) == false)
            {
                // this.MainForm.BeginInvoke(new Action<string>(ConnectAsync), this.dp2MServerUrl);
                await ConnectAsync();
            }
        }
#endif


        int _inTimer = 0;   // 防止 _timer_Elapsed() 重叠运行

        async void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_inTimer == 0)
            {
                _inTimer++;
                try
                {
                    if (this.Connection != null)
                        AddInfoLine("tick connection state = " + this.Connection.State.ToString());

                    if (this.Connection == null ||
                        this.Connection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected)
                    {
                        AddInfoLine("自动重新连接 ...");
                        await this.EnsureConnect();
                    }
                }
                finally
                {
                    _inTimer--;
                }
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
        public async Task<bool> EnsureConnect()
        {
            if (string.IsNullOrEmpty(this.ServerUrl) == true)
                throw new Exception("MessageConnection.dp2MServerUrl 尚未设置");

            if (this.Connection == null || this.Connection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected)
            {

                // ConnectAsync().Wait();
                await ConnectAsync();
                return true;
            }
            return false;
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

#if NO
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

            if (this.Container != null && this.Container.TraceWriter != null)
                Connection.TraceWriter = this.Container.TraceWriter;

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

#if FIX_HANDLER
            HubProxy.On<SearchResponse>("responseSearch",
    (param) => OnResponseSearchRecieved(param)
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

            // *** getRes
            HubProxy.On<GetResRequest>("getRes",
                (param) => OnGetResRecieved(param)
                );

            try
            {
                return Connection.Start()    // new ServerSentEventsTransport() new LongPollingTransport()
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

#endif

        // 连接 server
        // 要求调用前设置好 this.ServerUrl this.UserName this.Password this.Parameters
        public async Task<MessageResult> ConnectAsync(
            // string strServerUrl
            )
        {
            // 2016/9/15
            // 防范本函数被重叠调用时形成多个连接
            if (this.Connection != null)
            {
                this.Connection.Dispose();
                this.Connection = null;
            }

            _exiting = true;

            // 一直到真正连接前才触发登录事件
            if (this.Container != null)
                this.Container.TriggerLogin(this);

            AddInfoLine("正在连接服务器 " + this.ServerUrl + " ...");
            Connection = new HubConnection(this.ServerUrl);

            Connection.Closed += new Action(Connection_Closed);
            Connection.Reconnecting += Connection_Reconnecting;
            Connection.Reconnected += Connection_Reconnected;
            // Connection.Error += Connection_Error;

            if (this.Container != null && this.Container.TraceWriter != null)
                Connection.TraceWriter = this.Container.TraceWriter;

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

#if FIX_HANDLER
            HubProxy.On<SearchResponse>("responseSearch",
    (param) => OnResponseSearchRecieved(param)
);
#endif

            // *** bindPatron
            HubProxy.On<BindPatronRequest>("bindPatron",
                (param) => OnBindPatronRecieved(param)
                );

            // *** setInfo
            HubProxy.On<SetInfoRequest>("setInfo",
                (param) => OnSetInfoRecieved(param)
                );

            // *** circulation
            HubProxy.On<CirculationRequest>("circulation",
                (param) => OnCirculationRecieved(param)
                );

            // *** getRes
            HubProxy.On<GetResRequest>("getRes",
                (param) => OnGetResRecieved(param)
                );

            // *** webCall
            HubProxy.On<WebCallRequest>("webCall",
                (param) => OnWebCallRecieved(param)
                );

            // *** close
            HubProxy.On<CloseRequest>("close",
                (param) => OnCloseRecieved(param)
                );
            try
            {
                await Connection.Start();
#if NO
                if (Connection.Start().Wait(TimeSpan.FromSeconds(60)) == false)
                {
                    AddInfoLine("连接超时");
                    MessageResult result = new MessageResult();
                    result.Value = -1;
                    result.ErrorInfo = "Connection Start() Timeout";
                    result.String = "ConnectionStartTimeout";
                    return result;
                }
#endif

                {
                    MessageResult result = new MessageResult();
                    AddInfoLine("停止 Timer");
                    _timer.Stop();
                    _exiting = false;
                    AddInfoLine("成功连接到 " + this.ServerUrl);
                    TriggerConnectionStateChange("Connected");
                    return result;
                }
            }
            catch (HttpRequestException ex)
            {
                MessageResult result = new MessageResult();
                result.Value = -1;
                result.ErrorInfo = ex.Message;
                result.String = "HttpRequestException";
                return result;
            }
            catch (Microsoft.AspNet.SignalR.Client.HttpClientException ex)
            {
                Microsoft.AspNet.SignalR.Client.HttpClientException ex0 = ex as Microsoft.AspNet.SignalR.Client.HttpClientException;
                MessageResult result = new MessageResult();
                result.Value = -1;
                result.ErrorInfo = ex.Message;
                result.String = ex0.Response.StatusCode.ToString();
                return result;
            }
            catch (AggregateException ex)
            {
                MessageResult result = new MessageResult();
                result.Value = -1;
                result.ErrorInfo = GetExceptionText(ex);
                return result;
            }
            catch (Exception ex)
            {
                MessageResult result = new MessageResult();
                result.Value = -1;
                result.ErrorInfo = ExceptionUtil.GetExceptionText(ex);
                return result;
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

        // await 版本
        public async Task<GetMessageResult> GetMessageAsyncLite(GetMessageRequest request,
    TimeSpan timeout,
    CancellationToken cancel_token)
        {
            List<MessageRecord> results = new List<MessageRecord>();
            MessageResult result = await this.GetMessageAsyncLite(
    request,
    (totalCount,
start,
records,
errorInfo,
errorCode) =>
    {
        if (records != null)
        {
            foreach (MessageRecord record in records)
            {
                results.Add(record);
            }
        }
    },
    timeout,
    cancel_token);
            return new GetMessageResult(result, results);
        }

        // 包装后的同步函数。注意 request.Count 的使用，要避免一次调用获得的记录太多而导致内存放不下
        public GetMessageResult GetMessage(GetMessageRequest request,
            TimeSpan timeout,
            CancellationToken cancel_token)
        {
            List<MessageRecord> results = new List<MessageRecord>();
            MessageResult result = this.GetMessageTaskAsync(
    request,
    (totalCount,
start,
records,
errorInfo,
errorCode) =>
    {
        if (records != null)
        {
            foreach (MessageRecord record in records)
            {
                results.Add(record);
            }
        }
    },
    timeout,
    cancel_token).Result;
            return new GetMessageResult(result, results);
        }

        public delegate void Delegate_outputMessage(long totalCount,
            long start,
            IList<MessageRecord> records,
            string errorInfo,
            string errorCode);

        public Task<MessageResult> GetMessageTaskAsync(
    GetMessageRequest request,
    Delegate_outputMessage proc,
    TimeSpan timeout,
    CancellationToken token)
        {
            return TaskRun<MessageResult>(
                () =>
                {
                    return GetMessageAsyncLite(request, proc, timeout, token).Result;
                },
            token);
        }

        public async Task<MessageResult> GetMessageAsyncLite(
            GetMessageRequest request,
            Delegate_outputMessage proc,
            TimeSpan timeout,
            CancellationToken token)
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

                        if (errorCode == "_complete")
                        {
                            result.Value = resultCount;
                            wait_events.finish_event.Set();
                            return;
                        }

                        if (resultCount >= 0 &&
                            IsComplete(request.Start, request.Count, resultCount, recieved) == true)
                            wait_events.finish_event.Set();
                        else
                            wait_events.active_event.Set();
                    }))
                {
                    MessageResult temp = await HubProxy.Invoke<MessageResult>(
"RequestGetMessage",
request);
                    if (temp.Value == -1 || temp.Value == 0 || temp.Value == 2)
                        return temp;

                    // result.String 里面是返回的 taskID

                    await WaitAsync(
    request.TaskID,
    wait_events,
    timeout,
    token);
                    return result;
                }
            }
        }

        #endregion

        #region SetMessage() API

#if NO
        public Task<SetMessageResult> SetMessageAsync(SetMessageRequest param)
        {
            return HubProxy.Invoke<SetMessageResult>(
 "SetMessage",
 param);
        }
#endif
        public async Task<SetMessageResult> SetMessageAsyncLite(
SetMessageRequest request)
        {
            return await HubProxy.Invoke<SetMessageResult>(
"SetMessage",
request);
        }

        public Task<SetMessageResult> SetMessageTaskAsync(
SetMessageRequest request,
CancellationToken token)
        {
#if NO
            // 验证用
            return Task.Factory.StartNew(
                async () =>
                {
                    return await SetMessageAsyncLite(request);
                },
                token,
            TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
#endif
#if NO
            return Task.Run<SetMessageResult>(
                async () =>
                {
                    return await SetMessageAsyncLite(request);
                },
            token);
#endif
            return TaskRun<SetMessageResult>(
                () =>
                {
                    return SetMessageAsyncLite(request).Result;
                },
            token);
        }

        public async Task<SetMessageResult> SetMessageAsyncLite(
    SetMessageRequest request,
    TimeSpan timeout,
    CancellationToken token)
        {
            Task<SetMessageResult> task = HubProxy.Invoke<SetMessageResult>(
"SetMessage",
request);
            if (task == await Task.WhenAny(task, Task.Delay(timeout)))
                return task.Result;

            throw new TimeoutException("已超时 " + timeout.ToString());
        }

        public Task<SetMessageResult> SetMessageTaskAsync(
SetMessageRequest request,
TimeSpan timeout,
CancellationToken token)
        {
            return TaskRun<SetMessageResult>(
                () =>
                {
                    return SetMessageAsyncLite(request, timeout, token).Result;
                },
            token);
        }

        #endregion

        #region GetConnectionInfo() API

        public Task<GetConnectionInfoResult> GetConnectionInfoTaskAsync(
GetConnectionInfoRequest request,
TimeSpan timeout,
CancellationToken token)
        {
            return TaskRun<GetConnectionInfoResult>(
                () =>
                {
                    return GetConnectionInfoAsyncLite(request, timeout, token).Result;
                },
            token);
        }

        public async Task<GetConnectionInfoResult> GetConnectionInfoAsyncLite(
    GetConnectionInfoRequest request,
    TimeSpan timeout,
    CancellationToken token)
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
                    MessageResult message = await HubProxy.Invoke<MessageResult>(
        "RequestGetConnectionInfo",
        request);
                    if (message.Value == -1 || message.Value == 0)
                    {
                        result.ErrorInfo = message.ErrorInfo;
                        result.ResultCount = -1;
                        result.ErrorCode = message.String;
                        return result;
                    }

                    // result.String 里面是返回的 taskID

                    // start_time = DateTime.Now;

                    await WaitAsync(
    request.TaskID,
    wait_events,
    timeout,
    token);
                    return result;
                }
            }
        }

        #endregion

        #region Close() API

        // 当 server 发来 Close 请求的时候被调用。
        public virtual void OnCloseRecieved(CloseRequest param)
        {
            this.CloseConnection();
            if (param.Action == "reconnect")
            {
                ConnectAsync(); // 不用等待完成
            }
        }

        #endregion

        #region WebCall() API

        // 当 server 发来 WebCall 请求的时候被调用。重载的时候要对目标发送 HTTP 请求，获得 HTTP 响应，并调用 responseWebCall 把响应结果发送给 server
        public virtual void OnWebCallRecieved(WebCallRequest param)
        {

        }

#if NO
        // 将 data 内容追加到 exist
        static void AddData(WebData exist, 
            StringBuilder text,
            WebData data)
        {
            if (data.Headers != null)
            {
                if (exist.Headers == null)
                    exist.Headers = data.Headers;
                else
                    exist.Headers += data.Headers;
            }

            if (data.Content != null)
            {
                if (exist.Content == null)
                {
#if VERIFY_CHUNK
                    if (data.Offset != 0)
                        throw new Exception("第一个 chunk 其 Offset 应该为 0");
#endif
                    exist.Content = data.Content;
                }
                else
                {
#if VERIFY_CHUNK
                    if (exist.Content.Length != data.Offset)
                        throw new Exception("累积 Content 的长度 " + exist.Content.Length + " 和当前 chunk 的 offset " + data.Offset.ToString() + " 不一致");
#endif
                    exist.Content = ByteArray.Add(exist.Content, data.Content);
                }

#if VERIFY_CHUNK
                string md5 = StringUtil.GetMd5(exist.Content);
                if (md5 != exist.MD5)
                    throw new Exception("请求者收取的 Content MD5 校验不正确");
#endif
            }

            if (data.Text != null)
            {
#if NO
                if (exist.Text == null)
                {
                    exist.Text = data.Text;
                }
                else
                {
                    exist.Text += data.Text;
                }
#endif
                text.Append(data.Text);
            }

        }
#endif
        public Task<WebCallResult> WebCallTaskAsync(
string strRemoteUserName,
WebCallRequest request,
TimeSpan timeout,
CancellationToken token)
        {
            return TaskRun<WebCallResult>(
                () =>
                {
                    return WebCallAsyncLite(strRemoteUserName, request, timeout, token).Result;
                },
            token);
        }

        public async Task<WebCallResult> WebCallAsyncLite(
    string strRemoteUserName,
    WebCallRequest request,
    TimeSpan timeout,
    CancellationToken token)
        {
            List<string> errors = new List<string>();
            List<string> codes = new List<string>();

            WebCallResult result = new WebCallResult();
#if NO
            if (result.WebData == null)
                result.WebData = new WebData();
#endif
            WebDataWrapper wrapper = new WebDataWrapper();

            if (string.IsNullOrEmpty(request.TaskID) == true)
                request.TaskID = Guid.NewGuid().ToString();

            using (WaitEvents wait_events = new WaitEvents())    // 表示中途数据到来
            {
                using (var handler = HubProxy.On<WebCallResponse>(
                    "responseWebCall",
                    (responseParam) =>
                    {
                        try
                        {
                            if (responseParam.TaskID != request.TaskID)
                                return;

                            if (responseParam.Result != null
    && string.IsNullOrEmpty(responseParam.Result.ErrorInfo) == false
    && errors.IndexOf(responseParam.Result.ErrorInfo) == -1)
                            {
                                errors.Add(responseParam.Result.ErrorInfo);
                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                            }
                            if (responseParam.Result != null
                                && string.IsNullOrEmpty(responseParam.Result.String) == false
                                && codes.IndexOf(responseParam.Result.String) == -1)
                            {
                                codes.Add(responseParam.Result.String);
                                result.String = StringUtil.MakePathList(codes, ",");
                            }

                            // 报错了
                            if (responseParam.Result != null
                                && responseParam.Result.Value == -1)
                            {
                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                                result.String = StringUtil.MakePathList(codes, ",");
                                result.Value = -1;
                                wait_events.finish_event.Set();
                                return;
                            }

                            // 拼接命中结果
                            if (responseParam.WebData != null)
                                wrapper.Append(responseParam.WebData);

                            if (responseParam.Complete)
                                wait_events.finish_event.Set();
                            else
                                wait_events.active_event.Set();
                        }
                        catch (Exception ex)
                        {
                            errors.Add("WebCallAsync handler 内出现异常: " + ExceptionUtil.GetDebugText(ex));
                            result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                            if (!(ex is ObjectDisposedException))
                                wait_events.finish_event.Set();
                        }
                    }))
                {
                    // 分批发出请求
                    WebDataSplitter splitter = new WebDataSplitter();
                    splitter.ChunkSize = MessageUtil.BINARY_CHUNK_SIZE;
                    splitter.TransferEncoding = request.TransferEncoding;
                    splitter.WebData = request.WebData;

                    foreach (WebData current in splitter)
                    {
                        WebCallRequest param = new WebCallRequest(
                        request.TaskID,
                        request.TransferEncoding,
                        current,
                        splitter.FirstOne,
                        splitter.LastOne);

                        MessageResult message = await HubProxy.Invoke<MessageResult>(
"RequestWebCall",
strRemoteUserName,
param);
                        if (message.Value == -1 || message.Value == 0)
                        {
                            result.ErrorInfo = message.ErrorInfo;
                            result.Value = -1;
                            result.String = message.String;
                            return result;
                        }
                    }

                    // 等待回应消息
                    try
                    {
                        await WaitAsync(
        request.TaskID,
        wait_events,
        timeout,
        token);
                    }
                    catch (TimeoutException)
                    {
                        throw;
                    }

                    wrapper.Flush();
                    result.WebData = wrapper.WebData;
                    return result;
                }
            }
        }

        #endregion

        #region Search() API

        static void AddLibraryUID(IList<Record> records, string libraryUID)
        {
            if (records == null)
                return;
            foreach (Record record in records)
            {
                record.RecPath += "@" + libraryUID;
            }
        }
        // 
        // 当 server 发来检索请求的时候被调用。重载的时候要进行检索，并调用 Response 把检索结果发送给 server
        public virtual void OnSearchRecieved(SearchRequest param)
        {

        }

#if NO
        public class SearchTask
        {
            public ResultManager manager = new ResultManager();
            public List<string> errors = new List<string>();
            public List<string> codes = new List<string>();

            public string id = "";
            internal WaitEvents wait_events = new WaitEvents();
            public IDisposable handler = null;
            public SearchResult result = new SearchResult();
        }

        public SearchTask BeginSearch(string taskID)
        {
            SearchTask task = new SearchTask();
            task.id = taskID;
            if (task.result.Records == null)
                task.result.Records = new List<Record>();

            task.handler = HubProxy.On<SearchResponse>(
                    "responseSearch",
                    (responseParam
                        // taskID, resultCount, start, records, errorInfo, errorCode
                        ) =>
                    {
                        if (responseParam.TaskID != task.id)
                            return;

                        // 装载命中结果
                        if (responseParam.ResultCount == -1 && responseParam.Start == -1)
                        {
                            if (task.result.ResultCount != -1)
                                task.result.ResultCount = task.manager.GetTotalCount();
                            task.result.ErrorInfo = StringUtil.MakePathList(task.errors, "; ");
                            task.result.ErrorCode = StringUtil.MakePathList(task.codes, ",");

                            task.wait_events.finish_event.Set();
                            return;
                        }

                        // TODO: 似乎应该关注 start 位置
                        if (responseParam.Records != null)
                            AddLibraryUID(responseParam.Records, responseParam.LibraryUID);

                        task.result.Records.AddRange(responseParam.Records);
                        if (string.IsNullOrEmpty(responseParam.ErrorInfo) == false
                            && task.errors.IndexOf(responseParam.ErrorInfo) == -1)
                        {
                            task.errors.Add(responseParam.ErrorInfo);
                            task.result.ErrorInfo = StringUtil.MakePathList(task.errors, "; ");
                        }
                        if (string.IsNullOrEmpty(responseParam.ErrorCode) == false
                            && task.codes.IndexOf(responseParam.ErrorCode) == -1)
                        {
                            task.codes.Add(responseParam.ErrorCode);
                            task.result.ErrorCode = StringUtil.MakePathList(task.codes, ",");
                        }

                        // 标记结束一个检索目标
                        // return:
                        //      0   尚未结束
                        //      1   结束
                        //      2   全部结束
                        int nRet = task.manager.CompleteTarget(responseParam.LibraryUID,
                            responseParam.ResultCount,
                            responseParam.Records == null ? 0 : responseParam.Records.Count);

                        if (responseParam.ResultCount == -1)
                            task.result.ResultCount = -1;
                        else
                            task.result.ResultCount = task.manager.GetTotalCount();

                        if (nRet == 2)
                            task.wait_events.finish_event.Set();
                        else
                            task.wait_events.active_event.Set();
                    });

            return task;
        }

        public void EndSearch(SearchTask task)
        {
            task.wait_events.Dispose();
            task.handler.Dispose();
        }

        // await 版本
        public async Task<SearchResult> Search(
            SearchTask task,
    string strRemoteUserName,
    SearchRequest request,
    TimeSpan timeout,
    CancellationToken token)
        {
            if (string.IsNullOrEmpty(request.TaskID) == true)
                throw new ArgumentException("request.TaskID 不应为空");

            try
            {
                MessageResult message = HubProxy.Invoke<MessageResult>(
    "RequestSearch",
    strRemoteUserName,
    request).Result;
                if (message.Value == -1 || message.Value == 0)
                {
                    task.result.ErrorInfo = message.ErrorInfo;
                    task.result.ResultCount = -1;
                    task.result.ErrorCode = message.String;
                    return task.result;
                }

                if (task.manager.SetTargetCount(message.Value) == true)
                    return task.result;
            }
            catch(Exception ex)
            {
                task.result.ErrorInfo = ExceptionUtil.GetDebugText(ex);
                task.result.ResultCount = -1;
                return task.result;
            }

            // start_time = DateTime.Now;

            try
            {
                Wait(
request.TaskID,
task.wait_events,
timeout,
token);
            }
            catch (TimeoutException)
            {
                // 超时的时候实际上有结果了
                if (task.result.Records != null
                    && task.result.Records.Count > 0)
                {
                    task.result.ErrorCode += ",_timeout";    // 附加一个错误码，表示虽然返回了结果，但是已经超时
                    return task.result;
                }
                throw;
            }

            return task.result;
        }
#endif

#if FIX_HANDLER
        public SearchReponseRecievedEventHandler SearchResponseRevievedEvent = null;

        public virtual void OnResponseSearchRecieved(SearchResponse param)
        {
            SearchReponseRecievedEventHandler handler = this.SearchResponseRevievedEvent;
            if (handler != null)
            {
                SearchResponseRevievedEventArgs e = new SearchResponseRevievedEventArgs();
                e.Param = param;
                handler(this, e);
            }
        }

        public Task<SearchResult> SearchAsync(
string strRemoteUserName,
SearchRequest request,
TimeSpan timeout,
CancellationToken token)
        {
            return TaskRun<SearchResult>(
                () =>
                {
                    // DateTime start_time = DateTime.Now;
                    ResultManager manager = new ResultManager();
                    List<string> errors = new List<string>();
                    List<string> codes = new List<string>();

                    SearchResult result = new SearchResult();
                    if (result.Records == null)
                        result.Records = new List<Record>();

                    if (string.IsNullOrEmpty(request.TaskID) == true)
                        request.TaskID = Guid.NewGuid().ToString();

                    Debug.WriteLine("using wait_events");
                    using (WaitEvents wait_events = new WaitEvents())    // 表示中途数据到来
                    {
                        Debug.WriteLine("using handle");

                        SearchReponseRecievedEventHandler handler = (sender, e) =>
                        {
                            SearchResponse responseParam = e.Param;
                            try
                            {
                                if (responseParam.TaskID != request.TaskID)
                                    return;

                                Debug.WriteLine("handler called. responseParam\r\n***\r\n" + responseParam.Dump() + "***\r\n");

                                // 装载命中结果
                                if (responseParam.ResultCount == -1 && responseParam.Start == -1)
                                {
                                    if (result.ResultCount != -1)
                                        result.ResultCount = manager.GetTotalCount();
                                    //result.ErrorInfo = responseParam.ErrorInfo;
                                    //result.ErrorCode = responseParam.ErrorCode;
                                    result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                                    result.ErrorCode = StringUtil.MakePathList(codes, ",");

                                    Debug.WriteLine("finish_event.Set() 1");
                                    wait_events.finish_event.Set();
                                    return;
                                }

                                // TODO: 似乎应该关注 start 位置
                                if (responseParam.Records != null)
                                    AddLibraryUID(responseParam.Records, responseParam.LibraryUID);

                                result.Records.AddRange(responseParam.Records);
                                if (string.IsNullOrEmpty(responseParam.ErrorInfo) == false
                                    && errors.IndexOf(responseParam.ErrorInfo) == -1)
                                {
                                    errors.Add(responseParam.ErrorInfo);
                                    result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                                }
                                if (string.IsNullOrEmpty(responseParam.ErrorCode) == false
                                    && codes.IndexOf(responseParam.ErrorCode) == -1)
                                {
                                    codes.Add(responseParam.ErrorCode);
                                    result.ErrorCode = StringUtil.MakePathList(codes, ",");
                                }

                                // 标记结束一个检索目标
                                // return:
                                //      0   尚未结束
                                //      1   结束
                                //      2   全部结束
                                int nRet = manager.CompleteTarget(responseParam.LibraryUID,
                                    responseParam.ResultCount,
                                    responseParam.Records == null ? 0 : responseParam.Records.Count);

                                if (responseParam.ResultCount == -1)
                                    result.ResultCount = -1;
                                else
                                    result.ResultCount = manager.GetTotalCount();

#if NO
                                            if (nRet == 2)
                                            {
                                                Debug.WriteLine("finish_event.Set() 2");
                                                wait_events.finish_event.Set();
                                            }
                                            else
                                                wait_events.active_event.Set();
#endif
                                wait_events.active_event.Set();

                            }
                            catch (Exception ex)
                            {
                                errors.Add("SearchAsync handler 内出现异常: " + ExceptionUtil.GetDebugText(ex));
                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                                if (!(ex is ObjectDisposedException))
                                    wait_events.finish_event.Set();
                            }

                        };

                        // http://stackoverflow.com/questions/6460281/removing-anonymous-event-handlers
                        this.SearchResponseRevievedEvent += handler;

                        try
                        {
#if NO
                            // https://github.com/SignalR/SignalR/issues/2153
                            ManualResetEventSlim ok = new ManualResetEventSlim();
                            MessageResult message = null;
                            new Thread(() =>
                            {
                                // do work here
                                message = HubProxy.Invoke<MessageResult>(
                 "RequestSearch",
                 strRemoteUserName,
                 request).Result;
                                ok.Set();
                            }).Start();

                            ok.Wait();
#endif

                            MessageResult message = HubProxy.Invoke<MessageResult>(
                "RequestSearch",
                strRemoteUserName,
                request).Result;
                            if (message.Value == -1 || message.Value == 0)
                            {
                                result.ErrorInfo = message.ErrorInfo;
                                result.ResultCount = -1;
                                result.ErrorCode = message.String;
                                Debug.WriteLine("return pos 1");
                                return result;
                            }

                            if (manager.SetTargetCount(message.Value) == true)
                            {
                                Debug.WriteLine("return pos 2");
                                return result;
                            }

                            // start_time = DateTime.Now;

                            try
                            {
                                Wait(
                request.TaskID,
                wait_events,
                timeout,
                token);
                            }
                            catch (TimeoutException)
                            {
                                // 超时的时候实际上有结果了
                                if (result.Records != null
                                    && result.Records.Count > 0)
                                {
                                    result.ErrorCode += ",_timeout";    // 附加一个错误码，表示虽然返回了结果，但是已经超时
                                    Debug.WriteLine("return pos 3");
                                    return result;
                                }
                                throw;
                            }

                            Debug.WriteLine("return pos 4");
                            return result;
                        }
                        catch (Exception ex)
                        {
                            result.ResultCount = -1;
                            result.ErrorInfo = "exception: " + ExceptionUtil.GetExceptionText(ex);
                            return result;
                        }
                        finally
                        {
                            this.SearchResponseRevievedEvent -= handler;
                        }
                    }
                },
            token);
        }

#else

#if NO // 稳定版本
        // 新版 API，测试中
        public Task<SearchResult> SearchTaskAsync(
    string strRemoteUserName,
    SearchRequest request,
    TimeSpan timeout,
    CancellationToken token)
        {
            return TaskRun<SearchResult>(
                () =>
                {
                    // DateTime start_time = DateTime.Now;
                    ResultManager manager = new ResultManager();
                    List<string> errors = new List<string>();
                    List<string> codes = new List<string>();

                    SearchResult result = new SearchResult();
                    if (result.Records == null)
                        result.Records = new List<Record>();

                    if (string.IsNullOrEmpty(request.TaskID) == true)
                        request.TaskID = Guid.NewGuid().ToString();

                    using (WaitEvents wait_events = new WaitEvents())    // 表示中途数据到来
                    {
                        using (var handler = HubProxy.On<SearchResponse>(
                            "responseSearch",
                            (responseParam) =>
                            {
                                try
                                {
                                    if (responseParam.TaskID != request.TaskID)
                                        return;

                                    Debug.WriteLine("handler called. responseParam\r\n***\r\n" + responseParam.Dump() + "***\r\n");

                                    // 装载命中结果
                                    if (responseParam.ResultCount == -1 && responseParam.Start == -1)
                                    {
                                        if (result.ResultCount != -1)
                                            result.ResultCount = manager.GetTotalCount();
                                        //result.ErrorInfo = responseParam.ErrorInfo;
                                        //result.ErrorCode = responseParam.ErrorCode;
                                        result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                                        result.ErrorCode = StringUtil.MakePathList(codes, ",");

                                        Debug.WriteLine("finish_event.Set() 1");
                                        wait_events.finish_event.Set();
                                        return;
                                    }

                                    // TODO: 似乎应该关注 start 位置
                                    if (responseParam.Records != null)
                                        AddLibraryUID(responseParam.Records, responseParam.LibraryUID);

                                    result.Records.AddRange(responseParam.Records);
                                    if (string.IsNullOrEmpty(responseParam.ErrorInfo) == false
                                        && errors.IndexOf(responseParam.ErrorInfo) == -1)
                                    {
                                        errors.Add(responseParam.ErrorInfo);
                                        result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                                    }
                                    if (string.IsNullOrEmpty(responseParam.ErrorCode) == false
                                        && codes.IndexOf(responseParam.ErrorCode) == -1)
                                    {
                                        codes.Add(responseParam.ErrorCode);
                                        result.ErrorCode = StringUtil.MakePathList(codes, ",");
                                    }

                                    // 标记结束一个检索目标
                                    // return:
                                    //      0   尚未结束
                                    //      1   结束
                                    //      2   全部结束
                                    int nRet = manager.CompleteTarget(responseParam.LibraryUID,
                                        responseParam.ResultCount,
                                        responseParam.Records == null ? 0 : responseParam.Records.Count);

                                    if (responseParam.ResultCount == -1)
                                        result.ResultCount = -1;
                                    else
                                        result.ResultCount = manager.GetTotalCount();

#if NO
                                            if (nRet == 2)
                                            {
                                                Debug.WriteLine("finish_event.Set() 2");
                                                wait_events.finish_event.Set();
                                            }
                                            else
                                                wait_events.active_event.Set();
#endif
                                    wait_events.active_event.Set();

                                }
                                catch (Exception ex)
                                {
                                    errors.Add("SearchAsync handler 内出现异常: " + ExceptionUtil.GetDebugText(ex));
                                    result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                                    if (!(ex is ObjectDisposedException))
                                        wait_events.finish_event.Set();
                                }
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
                                result.ErrorCode = message.String;
                                Debug.WriteLine("return pos 1");
                                return result;
                            }

                            if (manager.SetTargetCount(message.Value) == true)
                            {
                                Debug.WriteLine("return pos 2");
                                return result;
                            }

                            try
                            {
                                Wait(
                request.TaskID,
                wait_events,
                timeout,
                token);
                            }
                            catch (TimeoutException)
                            {
                                // 超时的时候实际上有结果了
                                if (result.Records != null
                                    && result.Records.Count > 0)
                                {
                                    result.ErrorCode += ",_timeout";    // 附加一个错误码，表示虽然返回了结果，但是已经超时
                                    Debug.WriteLine("return pos 3");
                                    return result;
                                }
                                throw;
                            }

                            Debug.WriteLine("return pos 4");
                            return result;
                        }
                    }
                },
            token);
        }

#endif

        // 新版 API，测试中
        public Task<SearchResult> SearchTaskAsync(
    string strRemoteUserName,
    SearchRequest request,
    TimeSpan timeout,
    CancellationToken token)
        {
            return TaskRun<SearchResult>(
                () =>
                {
                    return SearchAsyncLite(strRemoteUserName, request, timeout, token).Result;
                },
            token);
        }

#if TESTING
        // 用于测试的包装函数
        public Task<SearchResult> SearchAsync(
string strRemoteUserName,
SearchRequest request,
TimeSpan timeout,
CancellationToken token)
        {
            return SearchTaskAsync(
strRemoteUserName,
request,
timeout,
token);
        }
#endif

        // 新版 API，测试中
        public async Task<SearchResult> SearchAsyncLite(
    string strRemoteUserName,
    SearchRequest request,
    TimeSpan timeout,
    CancellationToken token)
        {
            ResultManager manager = new ResultManager();
            List<string> errors = new List<string>();
            List<string> codes = new List<string>();

            SearchResult result = new SearchResult();
            if (result.Records == null)
                result.Records = new List<Record>();

            if (string.IsNullOrEmpty(request.TaskID) == true)
                request.TaskID = Guid.NewGuid().ToString();

            using (WaitEvents wait_events = new WaitEvents())    // 表示中途数据到来
            {
                using (var handler = HubProxy.On<SearchResponse>(
                    "responseSearch",
                    (responseParam) =>
                    {
                        try
                        {
                            if (responseParam.TaskID != request.TaskID)
                                return;

                            Debug.WriteLine("handler called. responseParam\r\n***\r\n" + responseParam.Dump() + "***\r\n");

                            // 装载命中结果
                            if (responseParam.ResultCount == -1 && responseParam.Start == -1)
                            {
                                if (result.ResultCount != -1)
                                    result.ResultCount = manager.GetTotalCount();
                                //result.ErrorInfo = responseParam.ErrorInfo;
                                //result.ErrorCode = responseParam.ErrorCode;
                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                                result.ErrorCode = StringUtil.MakePathList(codes, ",");

                                Debug.WriteLine("finish_event.Set() 1");
                                wait_events.finish_event.Set();
                                return;
                            }

                            // TODO: 似乎应该关注 start 位置
                            if (responseParam.Records != null)
                                AddLibraryUID(responseParam.Records, responseParam.LibraryUID);

                            result.Records.AddRange(responseParam.Records);
                            if (string.IsNullOrEmpty(responseParam.ErrorInfo) == false
                                && errors.IndexOf(responseParam.ErrorInfo) == -1)
                            {
                                errors.Add(responseParam.ErrorInfo);
                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                            }
                            if (string.IsNullOrEmpty(responseParam.ErrorCode) == false
                                && codes.IndexOf(responseParam.ErrorCode) == -1)
                            {
                                codes.Add(responseParam.ErrorCode);
                                result.ErrorCode = StringUtil.MakePathList(codes, ",");
                            }

                            // 标记结束一个检索目标
                            // return:
                            //      0   尚未结束
                            //      1   结束
                            //      2   全部结束
                            int nRet = manager.CompleteTarget(responseParam.LibraryUID,
                                responseParam.ResultCount,
                                responseParam.Records == null ? 0 : responseParam.Records.Count);

                            if (responseParam.ResultCount == -1)
                                result.ResultCount = -1;
                            else
                                result.ResultCount = manager.GetTotalCount();

#if NO
                                            if (nRet == 2)
                                            {
                                                Debug.WriteLine("finish_event.Set() 2");
                                                wait_events.finish_event.Set();
                                            }
                                            else
                                                wait_events.active_event.Set();
#endif
                            wait_events.active_event.Set();

                        }
                        catch (Exception ex)
                        {
                            errors.Add("SearchAsync handler 内出现异常: " + ExceptionUtil.GetDebugText(ex));
                            result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                            if (!(ex is ObjectDisposedException))
                                wait_events.finish_event.Set();
                        }
                    }))
                {
                    MessageResult message = await HubProxy.Invoke<MessageResult>(
        "RequestSearch",
        strRemoteUserName,
        request);
                    if (message.Value == -1 || message.Value == 0)
                    {
                        result.ErrorInfo = message.ErrorInfo;
                        result.ResultCount = -1;
                        result.ErrorCode = message.String;
                        Debug.WriteLine("return pos 1");
                        return result;
                    }

                    if (manager.SetTargetCount(message.Value) == true)
                    {
                        Debug.WriteLine("return pos 2");
                        return result;
                    }

                    try
                    {
                        await WaitAsync(
        request.TaskID,
        wait_events,
        timeout,
        token);
                    }
                    catch (TimeoutException)
                    {
                        // 超时的时候实际上有结果了
                        if (result.Records != null
                            && result.Records.Count > 0)
                        {
                            result.ErrorCode += ",_timeout";    // 附加一个错误码，表示虽然返回了结果，但是已经超时
                            Debug.WriteLine("return pos 3");
                            return result;
                        }
                        throw;
                    }

                    Debug.WriteLine("return pos 4");
                    return result;
                }
            }
        }

#endif

        internal class WaitEvents : IDisposable
        {
            public ManualResetEvent finish_event = new ManualResetEvent(false);    // 表示数据全部到来
            public AutoResetEvent active_event = new AutoResetEvent(false);    // 表示中途数据到来

            public virtual void Dispose()
            {
                finish_event.Dispose();
                active_event.Dispose();
            }
        }

        Task WaitAsync(string taskID,
            WaitEvents wait_events,
            TimeSpan timeout,
            CancellationToken cancellation_token)
        {
            return TaskRunAction(
    () =>
    {
        Wait(taskID, wait_events, timeout, cancellation_token);
    },
cancellation_token);
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
                    true); // false

                if (index == 0) // 正常完成
                    return; //  result;
                else if (index == 1)
                {
                    start_time = DateTime.Now;  // 重新计算超时开始时刻
                    Debug.WriteLine("重新计算超时开始时间 " + start_time.ToString());
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
                        Debug.WriteLine("超时。delta=" + (DateTime.Now - start_time).TotalSeconds.ToString());
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
                    result.ErrorCode = message.String;
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

        public Task<SetInfoResult> SetInfoTaskAsync(
string strRemoteUserName,
SetInfoRequest request,
TimeSpan timeout,
CancellationToken token)
        {
            return TaskRun<SetInfoResult>(() =>
            {
                return SetInfoAsyncLite(strRemoteUserName, request, timeout, token).Result;
            }, token);
        }

        public async Task<SetInfoResult> SetInfoAsyncLite(
    string strRemoteUserName,
    SetInfoRequest request,
    TimeSpan timeout,
    CancellationToken token)
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
                    MessageResult message = await HubProxy.Invoke<MessageResult>(
        "RequestSetInfo",
        strRemoteUserName,
        request);
                    if (message.Value == -1
                        || message.Value == 0)
                    {
                        result.ErrorInfo = message.ErrorInfo;
                        result.Value = -1;
                        result.String = message.String;
                        return result;
                    }

                    await WaitAsync(
    request.TaskID,
    wait_events,
    timeout,
    token);
                    return result;
                }
            }
        }

        #endregion

        #region BindPatron() API

        public virtual void OnBindPatronRecieved(BindPatronRequest param)
        {
        }

        public Task<BindPatronResult> BindPatronTaskAsync(
string strRemoteUserName,
BindPatronRequest request,
TimeSpan timeout,
CancellationToken token)
        {
            return TaskRun<BindPatronResult>(() =>
            {
                return BindPatronAsyncLite(strRemoteUserName, request, timeout, token).Result;
            }, token);
        }

        public async Task<BindPatronResult> BindPatronAsyncLite(
    string strRemoteUserName,
    BindPatronRequest request,
    TimeSpan timeout,
    CancellationToken token)
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
                    MessageResult message = await HubProxy.Invoke<MessageResult>(
        "RequestBindPatron",
        strRemoteUserName,
        request);
                    if (message.Value == -1
                        || message.Value == 0)
                    {
                        result.ErrorInfo = message.ErrorInfo;
                        result.Value = -1;
                        result.String = message.String;
                        return result;
                    }

                    await WaitAsync(
request.TaskID,
wait_events,
timeout,
token);
                    return result;
                }
            }
        }

        #endregion

        #region Circulation() API

        public virtual void OnCirculationRecieved(CirculationRequest param)
        {
        }

        public Task<CirculationResult> CirculationTaskAsync(
string strRemoteUserName,
CirculationRequest request,
TimeSpan timeout,
CancellationToken token)
        {
            return TaskRun<CirculationResult>(() =>
            {
                return CirculationAsyncLite(strRemoteUserName, request, timeout, token).Result;
            }, token);
        }

        public async Task<CirculationResult> CirculationAsyncLite(
    string strRemoteUserName,
    CirculationRequest request,
    TimeSpan timeout,
    CancellationToken token)
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
                    MessageResult message = await HubProxy.Invoke<MessageResult>(
        "RequestCirculation",
        strRemoteUserName,
        request);
                    if (message.Value == -1
                        || message.Value == 0)
                    {
                        // -1 表示请求失败；0 表示没有找到调用目标。1 才是成功发起了操作
                        result.ErrorInfo = message.ErrorInfo;
                        result.Value = -1;
                        result.String = message.String;
                        return result;
                    }

                    await WaitAsync(
request.TaskID,
wait_events,
timeout,
token);
                    return result;
                }
            }
        }

        #endregion


        #region GetRes() API

        public virtual void OnGetResRecieved(GetResRequest param)
        {
        }

        public delegate void Delegate_setProgress(long totalLength, long current);

        // 写入流版本
        // 返回结果中 result.Data 不会使用，为 null
        public async Task<GetResResponse> GetResAsyncLite(
            string strRemoteUserName,
            GetResRequest request,
            Stream stream,
            Delegate_setProgress func_setProgress,
            TimeSpan timeout,
            CancellationToken token)
        {
            long lTail = -1;    // -1 表示尚未使用
            long count = 0;
            List<string> errors = new List<string>();
            List<string> codes = new List<string>();

            GetResResponse result = new GetResResponse();

            if (string.IsNullOrEmpty(request.TaskID) == true)
                request.TaskID = Guid.NewGuid().ToString();

            using (WaitEvents wait_events = new WaitEvents())    // 表示中途数据到来
            {
                using (var handler = HubProxy.On<GetResResponse>(
                    "responseGetRes",
                    (responseParam) =>
                    {
                        try
                        {
                            if (responseParam.TaskID != request.TaskID)
                                return;

                            Debug.WriteLine("handler called. responseParam\r\n***\r\n" + responseParam.Dump() + "***\r\n");

                            // 装载命中结果
                            if (responseParam.TotalLength == -1 && responseParam.Start == -1)
                            {
                                if (func_setProgress != null && result.TotalLength >= 0)
                                    func_setProgress(result.TotalLength, lTail);

                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                                result.ErrorCode = StringUtil.MakePathList(codes, ",");

                                Debug.WriteLine("finish_event.Set() 1");
                                wait_events.finish_event.Set();
                                return;
                            }

                            result.TotalLength = responseParam.TotalLength;
                            if (string.IsNullOrEmpty(responseParam.Metadata) == false)
                                result.Metadata = responseParam.Metadata;
                            if (string.IsNullOrEmpty(responseParam.Timestamp) == false)
                                result.Timestamp = responseParam.Timestamp;
                            if (string.IsNullOrEmpty(responseParam.Path) == false)
                                result.Path = responseParam.Path;

                            // TODO: 检查一下和上次的最后位置是否连续
                            if (lTail != -1 && responseParam.Start != lTail)
                            {
                                errors.Add("GetResAsync 接收数据过程出现不连续的批次 lTail=" + lTail + " param.Start=" + responseParam.Start);
                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                                result.TotalLength = -1;
                                // 向服务器发送 CancelSearch 请求
                                CancelSearchAsync(responseParam.TaskID);
                                wait_events.finish_event.Set();
                                return;
                            }

                            if (responseParam.Data != null)
                            {
                                stream.Write(responseParam.Data,
                                    0,
                                    responseParam.Data.Length);
                                lTail = responseParam.Start + responseParam.Data.Length;
                            }

                            if (func_setProgress != null && result.TotalLength >= 0 && (count++ % 10) == 0)
                                func_setProgress(result.TotalLength, lTail);

                            if (string.IsNullOrEmpty(responseParam.ErrorInfo) == false
                                && errors.IndexOf(responseParam.ErrorInfo) == -1)
                            {
                                errors.Add(responseParam.ErrorInfo);
                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                            }
                            if (string.IsNullOrEmpty(responseParam.ErrorCode) == false
                                && codes.IndexOf(responseParam.ErrorCode) == -1)
                            {
                                codes.Add(responseParam.ErrorCode);
                                result.ErrorCode = StringUtil.MakePathList(codes, ",");
                            }

                            Debug.WriteLine("active_event activate");
                            wait_events.active_event.Set();
                        }
                        catch (Exception ex)
                        {
                            errors.Add("GetResAsync handler 内出现异常: " + ExceptionUtil.GetDebugText(ex));
                            result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                            if (!(ex is ObjectDisposedException))
                                wait_events.finish_event.Set();
                            // 向服务器发送 CancelSearch 请求
                            CancelSearchAsync(responseParam.TaskID);
                        }

                    }))
                {
                    MessageResult message = await HubProxy.Invoke<MessageResult>(
        "RequestGetRes",
        strRemoteUserName,
        request);
                    if (message.Value == -1 || message.Value == 0)
                    {
                        result.ErrorInfo = message.ErrorInfo;
                        result.TotalLength = -1;
                        result.ErrorCode = message.String;
                        Debug.WriteLine("return pos 1");
                        return result;
                    }

                    try
                    {
                        await WaitAsync(
        request.TaskID,
        wait_events,
        timeout,
        token);
                    }
                    catch (TimeoutException)
                    {
                        // 超时的时候实际上有结果了
                        if (lTail != -1)
                        {
                            result.ErrorCode += ",_timeout";    // 附加一个错误码，表示虽然返回了结果，但是已经超时
                            return result;
                        }
                        throw;
                    }

                    return result;
                }
            }
        }

        // 写入流版本
        // 返回结果中 result.Data 不会使用，为 null
        public Task<GetResResponse> GetResTaskAsync(
            string strRemoteUserName,
            GetResRequest request,
            Stream stream,
            Delegate_setProgress func_setProgress,
            TimeSpan timeout,
            CancellationToken token)
        {
            return TaskRun<GetResResponse>(
                () =>
                {
                    return GetResAsyncLite(strRemoteUserName, request, stream, func_setProgress, timeout, token).Result;
                },
            token);
        }

        // byte [] 返回版本。注意要小批调用本函数，以避免内存溢出
        public async Task<GetResResponse> GetResAsyncLite(
string strRemoteUserName,
GetResRequest request,
TimeSpan timeout,
CancellationToken token)
        {
            // ResultManager manager = new ResultManager();
            long lTail = -1;    // -1 表示尚未使用
            List<string> errors = new List<string>();
            List<string> codes = new List<string>();

            GetResResponse result = new GetResResponse();
            if (result.Data == null)
                result.Data = new byte[0];

            if (string.IsNullOrEmpty(request.TaskID) == true)
                request.TaskID = Guid.NewGuid().ToString();

            Debug.WriteLine("using wait_events");
            using (WaitEvents wait_events = new WaitEvents())    // 表示中途数据到来
            {
                Debug.WriteLine("using handle");
                using (var handler = HubProxy.On<GetResResponse>(
                    "responseGetRes",
                    (responseParam) =>
                    {
                        try
                        {
                            if (responseParam.TaskID != request.TaskID)
                                return;

                            Debug.WriteLine("handler called. responseParam\r\n***\r\n" + responseParam.Dump() + "***\r\n");

                            // 装载命中结果
                            if (responseParam.TotalLength == -1 && responseParam.Start == -1)
                            {
                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                                result.ErrorCode = StringUtil.MakePathList(codes, ",");

                                Debug.WriteLine("finish_event.Set() 1");
                                wait_events.finish_event.Set();
                                return;
                            }

                            result.TotalLength = responseParam.TotalLength;
                            if (string.IsNullOrEmpty(responseParam.Metadata) == false)
                                result.Metadata = responseParam.Metadata;
                            if (string.IsNullOrEmpty(responseParam.Timestamp) == false)
                                result.Timestamp = responseParam.Timestamp;
                            if (string.IsNullOrEmpty(responseParam.Path) == false)
                                result.Path = responseParam.Path;

                            // TODO: 检查一下和上次的最后位置是否连续
                            if (lTail != -1 && responseParam.Start != lTail)
                            {
                                errors.Add("GetResAsync 接收数据过程出现不连续的批次 lTail=" + lTail + " param.Start=" + responseParam.Start);
                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                                result.TotalLength = -1;
                                // 向服务器发送 CancelSearch 请求
                                CancelSearchAsync(responseParam.TaskID);
                                wait_events.finish_event.Set();
                                return;
                            }

                            if (responseParam.Data != null)
                            {
                                result.Data = ByteArray.Add(result.Data, responseParam.Data);
                                lTail = responseParam.Start + responseParam.Data.Length;
                            }

                            if (string.IsNullOrEmpty(responseParam.ErrorInfo) == false
                                && errors.IndexOf(responseParam.ErrorInfo) == -1)
                            {
                                errors.Add(responseParam.ErrorInfo);
                                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                            }
                            if (string.IsNullOrEmpty(responseParam.ErrorCode) == false
                                && codes.IndexOf(responseParam.ErrorCode) == -1)
                            {
                                codes.Add(responseParam.ErrorCode);
                                result.ErrorCode = StringUtil.MakePathList(codes, ",");
                            }

                            wait_events.active_event.Set();
                        }
                        catch (Exception ex)
                        {
                            errors.Add("GetResAsync handler 内出现异常: " + ExceptionUtil.GetDebugText(ex));
                            result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                            if (!(ex is ObjectDisposedException))
                                wait_events.finish_event.Set();
                            // 向服务器发送 CancelSearch 请求
                            CancelSearchAsync(responseParam.TaskID);
                        }

                    }))
                {
                    MessageResult message = await HubProxy.Invoke<MessageResult>(
        "RequestGetRes",
        strRemoteUserName,
        request);
                    if (message.Value == -1 || message.Value == 0)
                    {
                        result.ErrorInfo = message.ErrorInfo;
                        result.TotalLength = -1;
                        result.ErrorCode = message.String;
                        Debug.WriteLine("return pos 1");
                        return result;
                    }

                    try
                    {
                        Wait(
        request.TaskID,
        wait_events,
        timeout,
        token);
                    }
                    catch (TimeoutException)
                    {
                        // 超时的时候实际上有结果了
                        if (result.Data != null
                            && result.Data.Length > 0)
                        {
                            result.ErrorCode += ",_timeout";    // 附加一个错误码，表示虽然返回了结果，但是已经超时
                            Debug.WriteLine("return pos 3");
                            return result;
                        }
                        throw;
                    }

                    Debug.WriteLine("return pos 4");
                    return result;
                }
            }
        }

        #endregion

        #region SetUsers() API

        public Task<MessageResult> SetUsersTaskAsync(
            string action,
            List<User> users,
            TimeSpan timeout,
            CancellationToken token)
        {
            return TaskRun<MessageResult>(() =>
            {
                return SetUsersAsyncLite(action, users, timeout, token).Result;
            }, token);
        }

        public async Task<MessageResult> SetUsersAsyncLite(
    string action,
    List<User> users,
    TimeSpan timeout,
    CancellationToken token)
        {
            Task<MessageResult> task = HubProxy.Invoke<MessageResult>("SetUsers",
                action,
                users);

            List<Task> tasks = new List<Task>() { };
            if (task == await Task.WhenAny(task, Task.Delay(timeout), token.AsTask()))
                return task.Result;

            throw new TimeoutException("已超时 " + timeout.ToString());
        }


        #endregion

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

#if NO
        // 等待 Task 结束。重载时可以在其中加入出让界面控制权，或者显示进度的功能
        public virtual void WaitTaskComplete(Task task)
        {
            task.Wait();
        }
#endif

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
        public async Task<MessageResult> ResponseBindPatron(
            string taskID,
            long resultValue,
            IList<string> results,
            string errorInfo)
        {
            return await HubProxy.Invoke<MessageResult>("ResponseBindPatron",
taskID,
resultValue,
results,
errorInfo);
        }

        public async void TryResponseBindPatron(
    string taskID,
    long resultValue,
    IList<string> results,
    string errorInfo)
        {
            try
            {
                await HubProxy.Invoke<MessageResult>("ResponseBindPatron",
    taskID,
    resultValue,
    results,
    errorInfo);
            }
            catch
            {

            }
        }

        // 调用 server 端 ResponseSetInfo
        // TODO: 要考虑发送失败的问题
        public async Task<MessageResult> ResponseSetInfo(
            string taskID,
            long resultValue,
            IList<Entity> results,
            string errorInfo)
        {
            return await HubProxy.Invoke<MessageResult>("ResponseSetInfo",
taskID,
resultValue,
results,
errorInfo);
        }

        public async void TryResponseSetInfo(
    string taskID,
    long resultValue,
    IList<Entity> results,
    string errorInfo)
        {
            try
            {
                await HubProxy.Invoke<MessageResult>("ResponseSetInfo",
    taskID,
    resultValue,
    results,
    errorInfo);
            }
            catch
            {

            }
        }

        DateTime _lastTime = DateTime.Now;

        // 和上次操作的时刻之间，等待至少这么多时间。
        public void Wait(TimeSpan length)
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
        public bool TryResponseSearch(
            string taskID,
            long resultCount,
            long start,
            string libraryUID,
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
                    Wait(new TimeSpan(0, 0, 0, 0, 50)); // 50

                    MessageResult result = ResponseSearchAsync(
                        new SearchResponse(
                        taskID,
                        resultCount,
                        start + send,
                        libraryUID,
                        current,
                        errorInfo,
                        errorCode)).Result;
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

                Console.WriteLine("成功发送 offset=" + (start + send) + " " + current.Count.ToString());

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
            TryResponseSearch(
                new SearchResponse(
taskID,
-1,
0,
libraryUID,
new List<Record>(),
strError,
"_sendResponseSearchError"));    // 消息层面发生的错误(表示不是 dp2library 层面的错误)，错误码为 _ 开头
            return false;
        }

        // TODO: 如果第一次 metadata 和 timestamp 发送成功了，后面的几次就不要发送了，这样可以节省流量
        // TODO: 如果 dp2mserver 返回值表示需要中断，就不要继续处理了
        // parameters:
        //      batch_size  建议的最佳一次发送数目。-1 表示不限制
        // return:
        //      true    成功
        //      false   失败
        public bool TryResponseGetRes(
            GetResResponse param,
            ref long batch_size)
        {
            string strError = "";

            List<byte> rest = new List<byte>(); // 等待发送的
            List<byte> current = new List<byte>();  // 当前正在发送的
            if (param.Data != null)
            {
                if (batch_size == -1)
                    current.AddRange(param.Data);
                else
                {
                    rest.AddRange(param.Data);

                    // 将最多 batch_size 个元素从 rest 中移动到 current 中
                    for (int i = 0; i < batch_size && rest.Count > 0; i++)
                    {
                        current.Add(rest[0]);
                        rest.RemoveAt(0);
                    }
                }
            }

            long send = 0;  // 已经发送过的元素数
            while (current.Count > 0 || param.Data == null)
            {
                try
                {
                    Wait(new TimeSpan(0, 0, 0, 0, 50)); // 50

                    MessageResult result = HubProxy.Invoke<MessageResult>("ResponseGetRes",
                        new DigitalPlatform.Message.GetResResponse(
                        param.TaskID,
                        param.TotalLength,
                        param.Start + send,
                        param.Path,
                        current.ToArray(),
                        send == 0 ? param.Metadata : "",
                        send == 0 ? param.Timestamp : "",
                        param.ErrorInfo,
                        param.ErrorCode)).Result;
                    _lastTime = DateTime.Now;
                    if (result.Value == -1)
                        return false;   // 可能因为服务器端已经中断此 taskID，或者执行 ReponseSearch() 时出错
                }
                catch (Exception ex)
                {
                    Console.WriteLine("(retry)ResponseGetRes() exception=" + ex.Message);

                    if (ex.InnerException is InvalidOperationException)
                    {
                        if (current.Count == 1)
                        {
                            strError = "向中心发送 ResponseGetRes 消息时出现异常(连一个元素也发送不出去): " + ex.InnerException.Message;
                            goto ERROR1;
                        }
                        // 减少一半元素发送
                        int half = Math.Max(1, current.Count / 2);
                        int offs = current.Count - half;
                        for (int i = 0; current.Count > offs; i++)
                        {
                            byte record = current[offs];
                            rest.Insert(i, record);
                            current.RemoveAt(offs);
                        }
                        batch_size = half;
                        continue;
                    }

                    strError = "向中心发送 ResponseGetRes 消息时出现异常: " + ExceptionUtil.GetExceptionText(ex);
                    goto ERROR1;
                }

                // Console.WriteLine("成功发送 offset=" + (param.Start + send) + " " + current.Count.ToString());

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

                if (param.Data == null)
                    break;
            }

            Debug.Assert(rest.Count == 0, "");
            Debug.Assert(current.Count == 0, "");
            return true;
        ERROR1:
            // 报错
            {
                MessageResult result = HubProxy.Invoke<MessageResult>("ResponseGetRes",
        new DigitalPlatform.Message.GetResResponse(
        param.TaskID,
        -1, // param.TotalLength,
        param.Start + send,
        param.Path,
        current.ToArray(),
        param.Metadata,
        param.Timestamp,
        strError,
        "_sendResponseGetResError")).Result;
                // 消息层面发生的错误(表示不是 dp2library 层面的错误)，错误码为 _ 开头
            }
            return false;
        }

        public void ResponseGetRes(GetResResponse param)
        {
            try
            {
                MessageResult result = HubProxy.Invoke<MessageResult>("ResponseGetRes",
 param).Result;
            }
            catch
            {
            }
        }

        // 调用 server 端 ResponseSearchBiblio
        public Task<MessageResult> ResponseSearchAsync(
SearchResponse responseParam)
        {
            return HubProxy.Invoke<MessageResult>("ResponseSearch",
 responseParam);
        }

        // 调用 server 端 ResponseSearchBiblio
        public async void TryResponseSearch(
SearchResponse responseParam)
        {
            // TODO: 等待执行完成。如果有异常要当时处理。比如减小尺寸重发。
            int nRedoCount = 0;
        REDO:
            try
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("ResponseSearch",
 responseParam);
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
#if NO
            var task = HubProxy.Invoke<MessageResult>("SetUsers",
                action,
                users);
            task.Wait();
            return task.Result;
#endif
            return HubProxy.Invoke<MessageResult>("SetUsers",
                action,
                users).Result;
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

        // 调用 server 端 ResponseWebCall
        public Task<MessageResult> ResponseWebCallAsync(
WebCallResponse responseParam)
        {
            return HubProxy.Invoke<MessageResult>("ResponseWebCall",
 responseParam);
        }

        public void TryResponseWebCall(
WebCallResponse responseParam)
        {
            try
            {
                HubProxy.Invoke<MessageResult>("ResponseWebCall",
     responseParam).Wait();
            }
            catch
            {

            }
        }

        #endregion

        public static Task<TResult> TaskRun<TResult>(
            Func<TResult> function,
            CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew<TResult>(
                function,
                cancellationToken,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public static Task TaskRunAction(Action function, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(
                function,
                cancellationToken,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
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

#if FIX_HANDLER
    public delegate void SearchReponseRecievedEventHandler(object sender,
SearchResponseRevievedEventArgs e);

    /// <summary>
    /// 通道创建成功事件的参数
    /// </summary>
    public class SearchResponseRevievedEventArgs : EventArgs
    {
        public SearchResponse Param = null;
    }

#endif
}
