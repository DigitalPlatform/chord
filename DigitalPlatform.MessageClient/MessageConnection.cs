using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;

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

        public MessageConnection()
        {
            _timer.Interval = 1000 * 30;
            _timer.Elapsed += _timer_Elapsed;
        }

        public virtual void InitialAsync()
        {
            if (string.IsNullOrEmpty(this.ServerUrl) == false)
            {
                // this.MainForm.BeginInvoke(new Action<string>(ConnectAsync), this.dp2MServerUrl);
                ConnectAsync(this.ServerUrl);
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
                // this.MainForm.BeginInvoke(new Action<string>(ConnectAsync), this.dp2MServerUrl);
                ConnectAsync(this.ServerUrl).Wait();
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
        public Task ConnectAsync(string strServerUrl)
        {
            AddInfoLine("正在连接服务器 " + strServerUrl + " ...");

            Connection = new HubConnection(strServerUrl);
            Connection.Closed += new Action(Connection_Closed);
            Connection.Reconnecting += Connection_Reconnecting;
            Connection.Reconnected += Connection_Reconnected;
            // Connection.Error += Connection_Error;

            HubProxy = Connection.CreateHubProxy("MyHub");

            HubProxy.On<string, string>("AddMessage",
                (name, message) =>
                OnAddMessageRecieved(name, message)
                );

            HubProxy.On<SearchRequest>("searchBiblio",
                (searchParam) => OnSearchBiblioRecieved(searchParam)
                );

            HubProxy.On<string,
    long,
    long,
    IList<BiblioRecord>,
        string>("responseSearchBiblio", (searchID,
    resultCount,
    start,
    records,
    errorInfo) =>
 OnSearchResponseRecieved(searchID,
    resultCount,
    start,
    records,
    errorInfo)

);

#if NO
            Task task = Connection.Start();
#if NO
            CancellationTokenSource token = new CancellationTokenSource();
            if (!task.Wait(60 * 1000, token.Token))
            {
                token.Cancel();
                // labelStatusText.Text = "time out";
                AddMessageLine("error", "time out");
                return;
            }
#endif
            while (task.IsCompleted == false)
            {
                Application.DoEvents();
                Thread.Sleep(200);
            }

            if (task.IsFaulted == true)
            {
#if NO
                if (task.Exception is HttpRequestException)
                    labelStatusText.Text = "Unable to connect to server: start server bofore connection client.";
#endif
                AddErrorLine(GetExceptionText(task.Exception));
                return;
            }


            AddInfoLine("停止 Timer");
            _timer.Stop();

            //EnableControls(true);
            //textBox_input.Focus();
            AddInfoLine("成功连接到 " + strServerUrl);

            this.MainForm.BeginInvoke(new Action(Login));
#endif
            try
            {
                return Connection.Start()
                    .ContinueWith((antecendent) =>
                    {
                        if (antecendent.IsFaulted == true)
                        {
                            AddErrorLine(GetExceptionText(antecendent.Exception));
                            return;
                        }
                        AddInfoLine("停止 Timer");
                        _timer.Stop();
                        AddInfoLine("成功连接到 " + strServerUrl);
                        TriggerLogin();
                    });
            }
            catch (Exception ex)
            {
                AddErrorLine(ex.Message);
                throw ex;   // return null ?
            }
        }

        // 连接成功后被调用，执行登录功能。重载时要调用 Login(...) 向 server 发送 login 消息
        public virtual void TriggerLogin()
        {
            this.Container.TriggerLogin(this);
        }

        void Connection_Reconnecting()
        {
            // tryingToReconnect = true;
        }

        void Connection_Reconnected()
        {
            // tryingToReconnect = false;

            AddInfoLine("Connection_Reconnected");

            this.TriggerLogin();
        }

        void Connection_Closed()
        {
            if (_exiting == false)
            {
                AddInfoLine("开启 Timer");
                _timer.Start();
            }
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

        public virtual void OnAddMessageRecieved(string strName, string strContent)
        {

        }

        // 
        // 当 server 发来检索请求的时候被调用。重载的时候要进行检索，并调用 Response 把检索结果发送给 server
        public virtual void OnSearchBiblioRecieved(
#if NO
            string searchID,
            string operation,
            string dbNameList,
             string queryWord,
             string fromList,
             string matchStyle,
             string formatList,
             long maxResults
#endif
SearchRequest param
            )
        {
        }

        public Task<SearchBiblioResult> SearchBiblioAsync(
            string strRemoteUserName,
            SearchRequest request,
            TimeSpan timeout,
            CancellationToken token)
        {
            return Task.Factory.StartNew<SearchBiblioResult>(() =>
            {
                SearchBiblioResult result = new SearchBiblioResult();

                if (string.IsNullOrEmpty(request.SearchID) == true)
                {
                    request.SearchID = Guid.NewGuid().ToString();
                }

                MessageResult message = HubProxy.Invoke<MessageResult>(
    "RequestSearchBiblio",
    strRemoteUserName,
    request).Result;
                if (message.Value == -1 || message.Value == 0)
                {
                    result.ErrorInfo = message.ErrorInfo;
                    result.ResultCount = -1;
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

                    SearchBiblioResult result0 = (SearchBiblioResult)_resultTable[request.SearchID];
                    if (result0 != null && result0.ResultCount == -1)
                    {
                        ClearResultFromTable(request.SearchID);
                        return result0;
                    }

                    if (result0 != null && result0.Records != null && result0.Records.Count >= result0.ResultCount)
                    {
                        ClearResultFromTable(request.SearchID);
                        return result0;
                    }

                    if (result0 != null && result0.Finished == true)
                    {
                        ClearResultFromTable(request.SearchID);
                        return result0;
                    }

                    Thread.Sleep(200);
                }
            }, token);
        }

        Hashtable _resultTable = new Hashtable();   // searchID --> SearchBiblioResult 

        // 从结果集表中移走结果
        void ClearResultFromTable(string searchID)
        {
            _resultTable.Remove(searchID);
        }

        // TODO: 按照 searchID 把检索结果一一存储起来。用信号通知消费线程。消费线程每次可以取走一部分，以后每一次就取走余下的。
        // 当 server 发来检索响应的时候被调用。重载时可以显示收到的记录
        public virtual void OnSearchResponseRecieved(string searchID,
    long resultCount,
    long start,
    IList<BiblioRecord> records,
    string errorInfo)
        {
            // TODO: 监视 Hashtable 中的元素数量，超过一个极限值要抛出异常

            SearchBiblioResult result = (SearchBiblioResult)_resultTable[searchID];
            if (result == null)
            {
                result = new SearchBiblioResult();
                // result.SearchID = searchID;
                _resultTable[searchID] = result;
            }

            if (result.Records == null)
                result.Records = new List<BiblioRecord>();

            if (resultCount == -1 && start == -1)
            {
                // 表示发送响应过程已经结束
                result.Finished = true;
                return;
            }

            result.ResultCount = resultCount;
            result.Records.AddRange(records);
            result.ErrorInfo = errorInfo;
            return;
        }

        // 关闭连接，并且不会引起自动重连接
        public void CloseConnection()
        {
            if (this.Connection != null)
            {
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

        #region 调用 Server 端函数

        // 发起一次书目检索
        // result.Value:
        //      -1  出错
        //      0   没有检索目标
        //      1   成功发起检索。此时 Result.String 里面返回了 searchID
        public Task<MessageResult> SearchBiblioAsync(
            string userNameList,
            SearchRequest searchParam)
        {
            return HubProxy.Invoke<MessageResult>(
                "RequestSearchBiblio",
                userNameList,
                searchParam);
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

        // 调用 server 端 ResponseSearchBiblio
        public async void Response(
            string searchID,
            long resultCount,
            long start,
            IList<BiblioRecord> records,
            string errorInfo)
        {
            try
            {
                MessageResult result = await HubProxy.Invoke<MessageResult>("ResponseSearchBiblio",
    searchID,
    resultCount,
    start,
    records,
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


#if NO
            Task<MessageResult> task = HubProxy.Invoke<MessageResult>("ResponseSearchBiblio",
searchID,
resultCount,
start,
records,
errorInfo);
            task.Wait();
            if (task.IsFaulted == true)
            {
                AddErrorLine(GetExceptionText(task.Exception));
                return;
            }
            if (task.Result.Value == -1)
            {
                AddErrorLine(task.Result.ErrorInfo);
            }
#endif
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
            string userName,
            string password,
            string libraryUID,
            string libraryName,
            string propertyList)
        {
            return HubProxy.Invoke<MessageResult>("Login",
                userName,
                password,
                libraryUID,
                libraryName,
                propertyList);
        }

        #endregion
    }

    public class SearchBiblioResult
    {
        public long ResultCount = 0;
        public List<BiblioRecord> Records = null;
        public string ErrorInfo = "";
        public bool Finished = false;
    }
}
