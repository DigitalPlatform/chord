using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Collections;
using System.Messaging;
using System.Diagnostics;
using System.Threading;
using System.Net;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.Message;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.SIP.Server;

namespace dp2Capo
{
    /// <summary>
    /// 一个服务器实例
    /// </summary>
    public class Instance
    {
        // 最近一次检查的时间
        public DateTime LastCheckTime { get; set; }

        public LibraryHostInfo dp2library { get; set; }
        public HostInfo dp2mserver { get; set; }

        // 2016/6/16
        public ZHostInfo zhost { get; set; }

        public SipHostInfo sip_host { get; set; }

        // 没有用 MessageConnectionCollectoin 管理
        public ServerConnection MessageConnection = new ServerConnection();

        NotifyThread _notifyThread = null;
        MessageQueue _queue = null;

        public string Name
        {
            get;
            set;
        }

        public string LogDir
        {
            get;
            set;
        }

        public string DataDir
        {
            get;
            set;
        }

        private bool _running = false;

        public bool Running
        {
            get
            {
                return _running;
            }
        }


        #region 全局结果集管理

        private readonly Object _syncResultSets = new Object();

        // 需要(管理线程去)删除的全局结果集名
        List<string> _globalResultSets = new List<string>();

        public void AddGlobalResultSets(List<string> names)
        {
            lock (_syncResultSets)
            {
                _globalResultSets.AddRange(names);
            }
        }

        public void FreeGlobalResultSets()
        {
            List<string> names = new List<string>();
            lock (_syncResultSets)
            {
                names.AddRange(_globalResultSets);
                _globalResultSets.Clear();
            }

            if (names.Count > 0)
            {
                LibraryChannel library_channel = this.MessageConnection.GetChannel(null);
                try
                {
                    foreach (string name in names)
                    {
                        // TODO: 要是能用通配符来删除大量结果集就好了
                        long lRet = library_channel.GetSearchResult("",
                            0,
                            0,
                            "@remove:" + name,
                            "zh",
                            out DigitalPlatform.LibraryClient.localhost.Record[] searchresults,
                            out string strError);
                        if (lRet == -1)
                        {
                            // 写入错误日志
                            WriteErrorLog("清除全局结果集 '" + name + "' 时发生错误: " + strError);
                            return;
                        }
                    }
                }
                finally
                {
                    this.MessageConnection.ReturnChannel(library_channel);
                }
            }
        }

        #endregion

        public Instance()
        {
            LastCheckTime = DateTime.Now;
        }

        public static string GetInstanceName(string strXmlFileName)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strXmlFileName);
            }
            catch (FileNotFoundException)
            {
                return Path.GetFileName(Path.GetDirectoryName(strXmlFileName));
            }
            string strInstanceName = dom.DocumentElement.GetAttribute("instanceName");
            if (string.IsNullOrEmpty(strInstanceName) == true)
                return Path.GetFileName(Path.GetDirectoryName(strXmlFileName));
            return strInstanceName;
        }

        public void Start()
        {
            string strXmlFileName = Path.Combine(this.DataDir, "capo.xml");
            if (File.Exists(strXmlFileName) == false)
                return; // TODO: 似乎应该抛出异常才好

            _cancel = new CancellationTokenSource();
            // SetShortDelay();

            this.Name = GetInstanceName(strXmlFileName);  // Path.GetFileName(Path.GetDirectoryName(strXmlFileName));

            Console.WriteLine();
            Console.WriteLine("*** 启动实例: " + this.Name + " -- " + strXmlFileName);

            this.LogDir = Path.Combine(Path.GetDirectoryName(strXmlFileName), "log");
            PathUtil.CreateDirIfNeed(this.LogDir);

            // string strVersion = Assembly.GetAssembly(typeof(Instance)).GetName().Version.ToString();

            // 验证一下日志文件是否允许写入。这样就可以设置一个标志，决定后面的日志信息写入文件还是 Windows 日志
            this.DetectWriteErrorLog("*** 实例 " + this.Name + " 开始启动 (dp2Capo 版本: " + ServerInfo.Version + ")");

            this.WriteErrorLog("old ServicePointManager.DefaultConnectionLimit=" + ServicePointManager.DefaultConnectionLimit);
            // http://stackoverflow.com/questions/5760403/what-to-set-servicepointmanager-defaultconnectionlimit-to
            ServicePointManager.DefaultConnectionLimit = 12;
            this.WriteErrorLog("new ServicePointManager.DefaultConnectionLimit=" + ServicePointManager.DefaultConnectionLimit);

            XmlDocument dom = new XmlDocument();
            dom.Load(strXmlFileName);

            {
                //this.Name = dom.DocumentElement.GetAttribute("instanceName");
                //if (string.IsNullOrEmpty(this.Name) == true)
                //    this.Name = Path.GetFileName(Path.GetDirectoryName(strXmlFileName));
                try
                {
                    XmlElement element = dom.DocumentElement.SelectSingleNode("dp2library") as XmlElement;
                    if (element == null)
                    {
                        // throw new Exception("配置文件 " + strXmlFileName + " 中根元素下尚未定义 dp2library 元素");
                        this.WriteErrorLog("配置文件 " + strXmlFileName + " 中根元素下尚未定义 dp2library 元素");
                    }

                    this.dp2library = new LibraryHostInfo();
                    this.dp2library.Initial(element);
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("配置文件 " + strXmlFileName + " (dp2library 元素内) 格式错误: " + ex.Message);
                }

                try
                {
                    if (!(dom.DocumentElement.SelectSingleNode("dp2mserver") is XmlElement element))
                    {
                        // dp2Capo 应可以不配置 dp2mserver 相关参数，这种状态也是有意义的，因为可以配置 Z39.50 服务器参数仅作为 Z39.50 服务器使用
                        // this.WriteErrorLog("配置文件 " + strXmlFileName + " 中根元素下尚未定义 dp2mserver 元素");
                    }
                    else
                    {
                        this.dp2mserver = new HostInfo();
                        this.dp2mserver.Initial(element);
                    }

                    this.MessageConnection.Instance = this;
                    this.MessageConnection.dp2library = this.dp2library;
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("配置文件 " + strXmlFileName + " (dp2mserver 元素内) 格式错误: " + ex.Message);
                }

                try
                {
                    if (dom.DocumentElement.SelectSingleNode("zServer") is XmlElement element)
                    {
                        this.zhost = new ZHostInfo();
                        this.zhost.Initial(element);

                        // this.zhost.SlowConfigInitialized = false;
                        // InitialZHostSlowConfig();
                    }
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("配置文件 " + strXmlFileName + " (zServer 元素内) 格式错误: " + ex.Message);
                }

                try
                {
                    if (dom.DocumentElement.SelectSingleNode("sipServer") is XmlElement element)
                    {
                        this.sip_host = new SipHostInfo();
                        this.sip_host.Initial(element);
                    }
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("配置文件 " + strXmlFileName + " (sipServer 元素内) 格式错误: " + ex.Message);
                }
            }


            // 只要定义了队列就启动这个线程
            if (string.IsNullOrEmpty(this.dp2library.DefaultQueue) == false)
            {
                this._notifyThread = new NotifyThread();
                this._notifyThread.Container = this;
                this._notifyThread.BeginThread();    // TODO: 应该在 MessageConnection 第一次连接成功以后，再启动这个线程比较好
            }

            InitialQueue(true);

            this._running = true;
        }

#if NO
        public void InitialZHostSlowConfig()
        {
            if (this.zhost == null)
                return;

            if (this.zhost.SlowConfigInitialized == true)
                return;

            Debug.Assert(this.MessageConnection != null, "");

            // 获得一些比较耗时的配置参数。
            // return:
            //      -2  出错。但后面可以重试
            //      -1  出错，后面不再重试
            //      0   成功
            int nRet = this.zhost.GetSlowCfgInfo(this.MessageConnection,
                out string strError);
            if (nRet == -2)
            {
                this.WriteErrorLog("实例 " + this.Name + " 首次初始化慢速参数时出错：" + strError + "。后面将自动重试初始化直到初始化成功");
            }
            else
            {
                if (nRet == -1)
                {
                    this.zhost = null;
                    this.WriteErrorLog("实例 " + this.Name + " 首次初始化 ZHost 慢速参数时出错：" + strError + "。后面不再进行重试");
                }
                this.zhost.SlowConfigInitialized = true;
            }
        }
#endif

        // parameters:
        //      bFirst  是否首次启动。首次启动和重试启动，若发生错误写入日志的方式不同。
        void InitialQueue(bool bFirst)
        {
            // 若使用 dp2library GetMessage() API 来获得消息
            if (this.dp2library.DefaultQueue == "!api")
                return;
            try
            {
                if (_queue == null
                    && string.IsNullOrEmpty(this.dp2library.DefaultQueue) == false)
                {
                    _queue = new MessageQueue(this.dp2library.DefaultQueue);    // TODO: 不知道当 Queue 尚未创建的时候，这个语句是否可能抛出异常?
                    _queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

                    // _queue.BeginPeek(new TimeSpan(0, 1, 0), null, OnMessageAdded);
                    this.BeginPeek();

                    if (bFirst == false)
                    {
                        //Program.WriteWindowsLog("重试启动实例 " + this.Name + " 的 Queue '" + this.dp2library.DefaultQueue + "' 成功",
                        //    EventLogEntryType.Information);
                        this.WriteErrorLog("重试启动队列 '" + this.dp2library.DefaultQueue + "' 成功");
                    }
                }
            }
            catch (Exception ex)
            {
                {
                    MessageQueue temp = this._queue;
                    if (temp != null)
                        temp.Dispose();
                    _queue = null;  // 2016/9/6 这样可以迫使后面一轮调用重新进入 if (_queue == null ...
                }

                if (bFirst)
                {
                    // Program.WriteWindowsLog("启动实例 " + this.Name + " 的 Queue '" + this.dp2library.DefaultQueue + "' 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
                    this.WriteErrorLog("首次启动队列 '" + this.dp2library.DefaultQueue + "' 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
                }
            }
        }

        public async Task BeginConnectTask()
        {
            // return Task.Run(() => BeginConnnect());

            this.WriteErrorLog("BeginConnect() 开始");    // 这样写入日志，是观察 BeginConnectTask() 的 task 到底多久完成

            try
            {
                this.MessageConnection.ServerUrl = this.dp2mserver.Url;

                this.MessageConnection.UserName = this.dp2mserver.UserName;
                this.MessageConnection.Password = this.dp2mserver.Password;
                this.MessageConnection.Parameters = GetParameters();

                // this.MessageConnection.InitialAsync();
                MessageResult result = await this.MessageConnection.ConnectAsync();
                if (result.Value == -1)
                {
                    string strError = "BeginConnect() 连接 " + this.MessageConnection.ServerUrl + " 时出错: " + result.ErrorInfo;
                    this.WriteErrorLog(strError);
                    Console.WriteLine(DateTime.Now.ToString() + " " + strError);
                }
                else
                {
                    // 2016/9/14
                    string strText = "连接 " + this.MessageConnection.ServerUrl + " 成功";
                    this.WriteErrorLog(strText);
                    Console.WriteLine(DateTime.Now.ToString() + " " + strText);
                }
            }
            catch (Exception ex)
            {
                string strError = "BeginConnect() 出现异常: " + ExceptionUtil.GetExceptionText(ex);
                this.WriteErrorLog(strError);
                Console.WriteLine(strError);
            }
            finally
            {
                this.WriteErrorLog("BeginConnect() 结束");
            }
        }

        public async void BeginConnect()
        {
            try
            {
                this.MessageConnection.ServerUrl = this.dp2mserver.Url;

                this.MessageConnection.UserName = this.dp2mserver.UserName;
                this.MessageConnection.Password = this.dp2mserver.Password;
                this.MessageConnection.Parameters = GetParameters();

                // this.MessageConnection.InitialAsync();
                MessageResult result = await this.MessageConnection.ConnectAsync();
                if (result.Value == -1)
                {
                    string strError = "BeginConnect() 连接 " + this.MessageConnection.ServerUrl + " 时出错: " + result.ErrorInfo;
                    this.WriteErrorLog(strError);
                    Console.WriteLine(DateTime.Now.ToString() + " " + strError);
                }
                else
                {
                    // 2016/9/14
                    string strText = "连接 " + this.MessageConnection.ServerUrl + " 成功";
                    this.WriteErrorLog(strText);
                    Console.WriteLine(DateTime.Now.ToString() + " " + strText);
                }
            }
            catch (Exception ex)
            {
                string strError = "BeginConnect() 出现异常: " + ExceptionUtil.GetExceptionText(ex);
                this.WriteErrorLog(strError);
                Console.WriteLine(strError);
            }
        }

        // 获得调试状态
        public string GetDebugState()
        {
            StringBuilder text = new StringBuilder();

            text.Append("instance name: " + this.Name + "\r\n");

            if (this.MessageConnection != null)
            {
                text.Append("connection state: " + this.MessageConnection.ConnectState.ToString() + "\r\n");
            }

            if (this.dp2library != null)
            {
                text.Append("libraryUID: " + this.dp2library.LibraryUID + "\r\n");
            }

            return text.ToString();
        }

        string GetParameters()
        {
            Hashtable table = new Hashtable();
            table["libraryUID"] = this.dp2library.LibraryUID;
            table["libraryName"] = this.dp2library.LibraryName;
            // table["propertyList"] = (this.ShareBiblio ? "biblio_search" : "");
            table["libraryUserName"] = "dp2Capo";
            return StringUtil.BuildParameterString(table, ',', '=', "url");
        }

        public void Close()
        {
            _cancel.Cancel();

            if (this._notifyThread != null)
                _notifyThread.StopThread(true);

            // this.MessageConnection.CloseConnection();
            this.MessageConnection.Close();

            this._running = false;
            this.WriteErrorLog("*** 实例 " + this.Name + " 成功降落。");
        }

        // 运用控制台显示方式，设置一个实例的基本参数
        public static void ChangeSettings(string strXmlFileName)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strXmlFileName);
            }
            catch (FileNotFoundException)
            {
                dom.LoadXml("<root />");
            }

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2library") as XmlElement;
            if (element == null)
            {
                element = dom.CreateElement("dp2library");
                dom.DocumentElement.AppendChild(element);
            }

            Console.WriteLine("请输入 dp2library 服务器 URL: (当前值为 '" + element.GetAttribute("url") + "' )");
            string strNewValue = Console.ReadLine();
            if (string.IsNullOrEmpty(strNewValue) == false)
                element.SetAttribute("url", strNewValue);

            Console.WriteLine("请输入 dp2library 服务器 用户名: (当前值为 '" + element.GetAttribute("userName") + "' )");
            strNewValue = Console.ReadLine();
            if (string.IsNullOrEmpty(strNewValue) == false)
                element.SetAttribute("userName", strNewValue);

            string strPassword = Cryptography.Decrypt(element.GetAttribute("password"), HostInfo.EncryptKey);
            Console.WriteLine("请输入 dp2library 服务器 密码: (当前值为 '" + new string('*', strPassword.Length) + "' )");

            Console.BackgroundColor = Console.ForegroundColor;
            strNewValue = Console.ReadLine();
            Console.ResetColor();

            if (string.IsNullOrEmpty(strNewValue) == false)
                element.SetAttribute("password", Cryptography.Encrypt(strNewValue, HostInfo.EncryptKey));

            // 2016/4/10
            Console.WriteLine("请输入 dp2library 的 MSMQ 消息队列名: (当前值为 '" + element.GetAttribute("defaultQueue") + "' )");
            strNewValue = Console.ReadLine();
            if (string.IsNullOrEmpty(strNewValue) == false)
                element.SetAttribute("defaultQueue", strNewValue);

            // 2016/7/4
            Console.WriteLine("请输入 dp2library 的 basic.http 或 rest.http 协议绑定地址[若有多个地址请用分号间隔]: (当前值为 '" + element.GetAttribute("webURL") + "' )");
            strNewValue = Console.ReadLine();
            if (string.IsNullOrEmpty(strNewValue) == false)
                element.SetAttribute("webURL", strNewValue);

            element = dom.DocumentElement.SelectSingleNode("dp2mserver") as XmlElement;
            if (element == null)
            {
                element = dom.CreateElement("dp2mserver");
                dom.DocumentElement.AppendChild(element);
            }

            Console.WriteLine("请输入 dp2mserver 服务器 URL: (当前值为 '" + element.GetAttribute("url") + "' )");
            strNewValue = Console.ReadLine();
            if (string.IsNullOrEmpty(strNewValue) == false)
                element.SetAttribute("url", strNewValue);

            Console.WriteLine("请输入 dp2mserver 服务器 用户名: (当前值为 '" + element.GetAttribute("userName") + "' )");
            strNewValue = Console.ReadLine();
            if (string.IsNullOrEmpty(strNewValue) == false)
                element.SetAttribute("userName", strNewValue);

            strPassword = Cryptography.Decrypt(element.GetAttribute("password"), HostInfo.EncryptKey);
            Console.WriteLine("请输入 dp2mserver 服务器 密码: (当前值为 '" + new string('*', strPassword.Length) + "' )");

            Console.BackgroundColor = Console.ForegroundColor;
            strNewValue = Console.ReadLine();
            Console.ResetColor();

            if (string.IsNullOrEmpty(strNewValue) == false)
                element.SetAttribute("password", Cryptography.Encrypt(strNewValue, HostInfo.EncryptKey));

            dom.Save(strXmlFileName);
        }

#if NO
        // 监控 MSMQ 新消息的循环中的停顿时间。防止 CPU 过分耗用
        TimeSpan _delay = TimeSpan.FromMilliseconds(500);

        // 设为较长的等待时间
        void SetLongDelay()
        {
            _delay = TimeSpan.FromSeconds(3);
        }

        // 设为较短的等待时间
        void SetShortDelay()
        {
            _delay = TimeSpan.FromMilliseconds(500);
        }
#endif

#if NO
        private void OnMessageAdded(IAsyncResult ar)
        {
            if (this._queue != null)
            {
                try
                {
                    if (_queue.EndPeek(ar) != null)
                        this._notifyThread.Activate();

                    Wait(_delay);

                    _queue.BeginPeek(new TimeSpan(0, 1, 0), null, OnMessageAdded);
                }
                catch (MessageQueueException ex)
                {
                    if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                        _queue.BeginPeek(new TimeSpan(0, 1, 0), null, OnMessageAdded);
                    else
                    {
                        // Program.WriteWindowsLog("针对 '" + this.dp2library.DefaultQueue + "' OnMessageAdded() 出现异常: " + ex.Message);
                        this.WriteErrorLog("针对 '" + this.dp2library.DefaultQueue + "' 的 OnMessageAdded() 出现异常: " + ExceptionUtil.GetExceptionText(ex));
                    }
                }
                catch (Exception ex)
                {
                    // Program.WriteWindowsLog("针对 '" + this.dp2library.DefaultQueue + "' OnMessageAdded() 出现异常: " + ex.Message);
                    this.WriteErrorLog("针对 '" + this.dp2library.DefaultQueue + "' 的 OnMessageAdded() 出现异常: " + ExceptionUtil.GetExceptionText(ex));
                }
            }
        }
#endif

        // 第二个版本
        private void OnMessageAdded(IAsyncResult ar)
        {
            if (this._queue != null)
            {
                try
                {
                    if (EndPeek(ar) != null)
                        this._notifyThread.Activate();  // 被激活的线程中会再次 BeginPeek()
#if NO
                    if (_queue.EndPeek(ar) != null)
                        this._notifyThread.Activate();  // 被激活的线程中会再次 BeginPeek()
#endif
                }
                catch (MessageQueueException ex)
                {
                    if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                    {
                        // _queue.BeginPeek(new TimeSpan(0, 1, 0), null, OnMessageAdded);
                        BeginPeek();
                    }
                    else
                    {
                        // Program.WriteWindowsLog("针对 '" + this.dp2library.DefaultQueue + "' OnMessageAdded() 出现异常: " + ex.Message);
                        this.WriteErrorLog("针对 '" + this.dp2library.DefaultQueue + "' 的 OnMessageAdded() 出现异常: " + ExceptionUtil.GetExceptionText(ex));
                    }
                }
                catch (Exception ex)
                {
                    // Program.WriteWindowsLog("针对 '" + this.dp2library.DefaultQueue + "' OnMessageAdded() 出现异常: " + ex.Message);
                    this.WriteErrorLog("针对 '" + this.dp2library.DefaultQueue + "' 的 OnMessageAdded() 出现异常: " + ExceptionUtil.GetExceptionText(ex));
                }
            }
        }

        public void TryResetConnection(/*string strErrorCode*/)
        {
            // if (strErrorCode == "_connectionNotFound")
            {
                this.WriteErrorLog("Connection 开始重置。方法是 CloseConnection()。");
                this.MessageConnection.CloseConnection();
                this.WriteErrorLog("Connection 已经被重置");

#if NO
                this.WriteErrorLog("Connection 开始重置。最长等待 6 秒");
                Task.Run(() => this.MessageConnection.CloseConnection(), _cancel.Token).Wait(new TimeSpan(0, 0, 6));
                this.WriteErrorLog("Connection 已经被重置");
#endif
#if NO
                // 用单独线程的方法
                this.WriteErrorLog("Connection 开始重置。最长等待 6 秒");
                this.MessageConnection.CloseConnection(TimeSpan.FromSeconds(6));
                this.WriteErrorLog("Connection 已经被重置");
#endif

#if NO
                // 缺点是可能会在 dp2mserver 一端遗留原有通道。需要测试验证一下
                this.WriteErrorLog("Connection 开始重置。方法是重新连接。最长等待 6 秒");
                this.MessageConnection.ConnectAsync().Wait(TimeSpan.FromSeconds(6));
                this.WriteErrorLog("Connection 已经被重置");
#endif

#if NO
                // 缺点是可能会在 dp2mserver 一端遗留原有通道。需要测试验证一下
                this.WriteErrorLog("Connection 开始重置。方法是重新连接。");
                this.MessageConnection.ConnectAsync().Wait();
                this.WriteErrorLog("Connection 已经被重置");
#endif
            }
        }

        // 中断信号
        internal CancellationTokenSource _cancel = new CancellationTokenSource();
#if NO

        // 等待一段时间，或者提前遇到中断信号返回
        void Wait(TimeSpan delta)
        {
            WaitHandle.WaitAny(new[] { _cancel.Token.WaitHandle }, delta);
        }
#endif
        private static readonly Object _syncRoot = new Object();
        bool _peekOn = false;

        void BeginPeek()
        {
            lock (_syncRoot)
            {
                if (_peekOn == true || _queue == null)
                    return;
                _queue.BeginPeek(new TimeSpan(0, 1, 0), null, OnMessageAdded);
                _peekOn = true;
            }
        }

        Message EndPeek(IAsyncResult ar)
        {
            lock (_syncRoot)
            {
                if (_peekOn == false || _queue == null)
                    return null;
                _peekOn = false;
                return _queue.EndPeek(ar);
            }
        }

        public void Notify()
        {
            // TODO: 要增加判断，防止过分频繁地被调用
            this.MessageConnection.CleanWebDataTable();

            if (this.dp2library.DefaultQueue == "!api")
            {
                try
                {
                    if (this.MessageConnection.ConnectState != Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
                        return;

                    MessageData[] messages = null;
                    string strError = "";
                    int nRet = this.MessageConnection.GetMsmqMessage(
                out messages,
                out strError);
                    if (nRet == -1)
                    {
                        this.WriteErrorLog("Instance.Notify() 中 GetMsmqMessage() 出错: " + strError);
                        return;
                    }
                    if (messages != null)
                    {
                        foreach (MessageData data in messages)
                        {
                            MessageRecord record = new MessageRecord();
                            record.groups = new string[1] { "gn:_patronNotify" };  // gn 表示 group name
                            record.data = (string)data.strBody;
                            record.format = "xml";
                            List<MessageRecord> records = new List<MessageRecord> { record };

                            DigitalPlatform.Message.SetMessageRequest param =
                                new DigitalPlatform.Message.SetMessageRequest("create",
                                "dontNotifyMe",
                                records);
                            SetMessageResult result = this.MessageConnection.SetMessageTaskAsync(param,
                                _cancel.Token).Result;
                            if (result.Value == -1)
                            {
                                this.WriteErrorLog("Instance.Notify() 中 SetMessageAsync() 出错: " + result.ErrorInfo);
                                return;
                            }
                        }

                        nRet = this.MessageConnection.RemoveMsmqMessage(
                            messages.Length,
                            out strError);
                        if (nRet == -1)
                        {
                            this.WriteErrorLog("Instance.Notify() 中 RemoveMsmqMessage() 出错: " + strError);
                            return;
                        }
                    }

                    this._notifyThread.Activate();
                    return;
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("Instance.Notify() 出现异常1: " + ExceptionUtil.GetDebugText(ex));
                }
            }

            // 如果第一次初始化 Queue 没有成功，这里再试探初始化
            InitialQueue(false);

            // 进行通知处理
            if (_queue != null
                && this.MessageConnection.ConnectState == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
            {
                try
                {
                    ServerInfo._recordLocks.LockForWrite(this._queue.Path);
                }
                catch (ApplicationException)
                {
                    // 超时了
                    return;
                }

                bool bSucceed = false;
                try
                {
                    MessageEnumerator iterator = _queue.GetMessageEnumerator2();
                    while (iterator.MoveNext())
                    {
                        Message message = iterator.Current;

                        MessageRecord record = new MessageRecord();
                        record.groups = new string[1] { "gn:_patronNotify" };  // gn 表示 group name
                        record.data = (string)message.Body;
                        record.format = "xml";
                        List<MessageRecord> records = new List<MessageRecord> { record };

                        int length = record.data.Length;

                        DigitalPlatform.Message.SetMessageRequest param =
                            new DigitalPlatform.Message.SetMessageRequest("create",
                            "dontNotifyMe",
                            records);
                        SetMessageResult result = this.MessageConnection.SetMessageTaskAsync(param,
                            _cancel.Token).Result;
                        if (result.Value == -1)
                        {
                            this.WriteErrorLog("Instance.Notify() 中 SetMessageAsync() 出错: " + result.ErrorInfo);
                            if (result.String == "_connectionNotFound")
                                Task.Run(() => TryResetConnection(/*result.String*/));
                            return;
                        }

                        // http://stackoverflow.com/questions/21864043/with-messageenumerator-removecurrent-how-do-i-know-if-i-am-at-end-of-queue
                        try
                        {
                            iterator.RemoveCurrent();
                        }
                        finally
                        {
                            iterator.Reset();
                        }
                    }

                    bSucceed = true;
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (MessageQueueException ex)
                {
                    // 记入错误日志
                    // Program.WriteWindowsLog("Instance.Notify() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                    this.WriteErrorLog("Instance.Notify() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                    // Thread.Sleep(5 * 1000);   // 拖延 5 秒
                }
                catch (InvalidCastException ex)
                {
                    // 记入错误日志
                    // Program.WriteWindowsLog("Instance.Notify() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                    this.WriteErrorLog("Instance.Notify() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                    // Thread.Sleep(5 * 1000);   // 拖延 5 秒
                }
                catch (Exception ex)
                {
                    // 记入错误日志
                    // Program.WriteWindowsLog("Instance.Notify() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                    this.WriteErrorLog("Instance.Notify() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                    // Thread.Sleep(5 * 1000);   // 拖延 5 秒
                }
                finally
                {
                    ServerInfo._recordLocks.UnlockForWrite(this._queue.Path);

                    // 只有当发送到 dp2mserver 成功的情况下才立即重新监控最新 MQ
                    // 否则就等下一轮 Worker() 来处理
                    if (bSucceed == true)
                    {
                        // _queue.BeginPeek(new TimeSpan(0, 1, 0), null, OnMessageAdded);
                        BeginPeek();
                    }
                }
            }
        }

        // 发送心跳消息给 dp2mserver
        /*
    <root>
    <type>patronNotify</type>
    <recipient>R0000001@LUID:62637a12-1965-4876-af3a-fc1d3009af8a</recipient>
    <mime>xml</mime>
    <body>...</body>
    </root>

         * */
        public bool SendHeartBeat()
        {
            if (this.MessageConnection == null || this.MessageConnection.ConnectState != Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
                return false;

            try
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root><type>heartbeat</type></root>");
                MessageRecord record = new MessageRecord();
                record.groups = new string[1] { "gn:_patronNotify" };  // gn 表示 group name
                record.data = dom.DocumentElement.OuterXml;
                record.format = "xml";
                List<MessageRecord> records = new List<MessageRecord> { record };

                DigitalPlatform.Message.SetMessageRequest param =
        new DigitalPlatform.Message.SetMessageRequest("create",
        "dontNotifyMe",
        records);
                SetMessageResult result = this.MessageConnection.SetMessageTaskAsync(param,
                    _cancel.Token).Result;
                if (result.Value == -1)
                {
                    this.WriteErrorLog("Instance.SendHeartBeat() 中 SetMessageAsync() [heartbeat] 出错: " + result.ErrorInfo);
                    if (result.String == "_connectionNotFound")
                        Task.Run(() => TryResetConnection(/*result.String*/));
                    return false;
                }

                return true;
            }
            catch (ThreadAbortException)
            {
                return false;
            }
            catch (Exception ex)
            {
                this.WriteErrorLog("Instance.SendHeartBeat() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                return false;
            }
        }

        #region 日志

        private readonly Object _syncLog = new Object();

        bool _errorLogError = false;    // 写入实例的日志文件是否发生过错误

        void _writeErrorLog(string strText)
        {
            lock (_syncLog)
            {
                DateTime now = DateTime.Now;
                // 每天一个日志文件
                string strFilename = Path.Combine(this.LogDir, "log_" + DateTimeUtil.DateTimeToString8(now) + ".txt");
                string strTime = now.ToString();
                FileUtil.WriteText(strFilename,
                    strTime + " " + strText + "\r\n");
            }
        }

        // 尝试写入实例的错误日志文件
        // return:
        //      false   失败
        //      true    成功
        public bool DetectWriteErrorLog(string strText)
        {
            _errorLogError = false;
            try
            {
                _writeErrorLog(strText);
            }
            catch (Exception ex)
            {
                Program.WriteWindowsLog("尝试写入实例 " + this.Name + " 的日志文件发生异常， 后面将改为写入 Windows 日志。异常信息如下：'" + ExceptionUtil.GetDebugText(ex) + "'", EventLogEntryType.Error);
                _errorLogError = true;
                return false;
            }

            Program.WriteWindowsLog("实例日志文件检测正常。从此开始关于实例 " + this.Name + " 的日志会写入目录 " + this.LogDir + " 中当天的日志文件",
                EventLogEntryType.Information);
            return true;
        }

        // 写入实例的错误日志文件
        public void WriteErrorLog(string strText)
        {
            if (_errorLogError == true) // 先前写入实例的日志文件发生过错误，所以改为写入 Windows 日志。会加上实例名前缀字符串
                Program.WriteWindowsLog("实例 " + this.Name + ": " + strText, EventLogEntryType.Error);
            else
            {
                try
                {
                    _writeErrorLog(strText);
                }
                catch (Exception ex)
                {
                    Program.WriteWindowsLog("因为原本要写入日志文件的操作发生异常， 所以不得不改为写入 Windows 日志(见后一条)。异常信息如下：'" + ExceptionUtil.GetDebugText(ex) + "'", EventLogEntryType.Error);
                    Program.WriteWindowsLog(strText, EventLogEntryType.Error);
                }
            }
        }

        #endregion

    }

    public class LibraryHostInfo : HostInfo
    {
        // basic.http 协议绑定 URL。便于进行 Web 访问
        public string WebUrl
        {
            get;
            set;
        }

        // 默认的 MSMQ 队列路径
        public string DefaultQueue
        {
            get;
            set;
        }

        // 图书馆 UID。从 dp2library 用 API 获取
        public string LibraryUID
        {
            get;
            set;
        }

        // 图书馆名。从 dp2library 用 API 获取
        public string LibraryName
        {
            get;
            set;
        }

        public override void Initial(XmlElement element)
        {
            base.Initial(element);

            this.DefaultQueue = element.GetAttribute("defaultQueue");

            Console.WriteLine(element.Name + " defaultQueue=" + this.DefaultQueue);
#if NO
            if (string.IsNullOrEmpty(this.DefaultQueue) == true)
                throw new Exception("元素 " + element.Name + " 尚未定义 defaultQueue 属性");
#endif

            this.WebUrl = element.GetAttribute("webURL");

            Console.WriteLine(element.Name + " webURL=" + this.WebUrl);
        }
    }

    public class HostInfo
    {
        public static readonly string EncryptKey = "dp2capopassword";

        public string Url { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public virtual void Initial(XmlElement element)
        {
            this.Url = element.GetAttribute("url");
            if (string.IsNullOrEmpty(this.Url) == true)
                throw new Exception("元素 " + element.Name + " 尚未定义 url 属性");

            Console.WriteLine(element.Name + " url=" + this.Url);

            this.UserName = element.GetAttribute("userName");
            if (string.IsNullOrEmpty(this.UserName) == true)
                throw new Exception("元素 " + element.Name + " 尚未定义 userName 属性");

            Console.WriteLine(element.Name + " userName=" + this.UserName);

            this.Password = Cryptography.Decrypt(element.GetAttribute("password"), EncryptKey);
        }
    }

    // Z39.50 主机信息
    public class ZHostInfo : HostInfo
    {
        // 慢速参数是否初始化成功?
        // public bool SlowConfigInitialized { get; set; }

#if NO
        // 图书馆 UID。从 dp2library 用 API 获取
        public string LibraryUID
        {
            get;
            set;
        }

        // 图书馆名。从 dp2library 用 API 获取
        public string LibraryName
        {
            get;
            set;
        }

        // dp2library 服务器 URL
        public string LibraryServerUrl { get; set; }

        // 管理者的用户名和密码
        public string ManagerUserName { get; set; }
        public string ManagerPassword { get; set; }
#endif

        // 匿名访问时使用的用户名和密码
        public string AnonymousUserName { get; set; }
        public string AnonymousPassword { get; set; }

        private int _maxResultCount = -1;
        public int MaxResultCount
        {
            get => _maxResultCount;
            set => _maxResultCount = value;
        }

        new static readonly string EncryptKey = "dp2zserver_password_key";

        public string DecryptPasssword(string strEncryptedText)
        {
            if (String.IsNullOrEmpty(strEncryptedText) == false)
            {
                try
                {
                    string strPassword = Cryptography.Decrypt(
        strEncryptedText,
        EncryptKey);
                    return strPassword;
                }
                catch
                {
                    return "errorpassword";
                }

            }

            return "";
        }

        public string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, ZHostInfo.EncryptKey);
        }

        private XmlElement _root = null;

        public override void Initial(XmlElement element)
        {
            _root = element;

            // base.Initial(element);

            // 取出一些常用的指标

            // 1) 图书馆应用服务器URL
            // 2) 管理员用的帐户和密码
            XmlElement node = element.SelectSingleNode("dp2library") as XmlElement;
            if (node != null)
            {
#if NO
                this.LibraryServerUrl = node.GetAttribute("url");

                this.ManagerUserName = node.GetAttribute("username");
                string strPassword = node.GetAttribute("password");
                this.ManagerPassword = DecryptPasssword(strPassword);
#endif
                this.AnonymousUserName = node.GetAttribute("anonymousUserName");
                string strPassword = node.GetAttribute("anonymousPassword");
                this.AnonymousPassword = DecryptPasssword(strPassword);
            }
            else
            {
#if NO
                this.LibraryServerUrl = "";

                this.ManagerUserName = "";
                this.ManagerUserName = "";
#endif
                this.AnonymousUserName = "";
                this.AnonymousPassword = "";
            }

#if NO
            this.DefaultQueue = element.GetAttribute("defaultQueue");

            Console.WriteLine(element.Name + " defaultQueue=" + this.DefaultQueue);


            Console.WriteLine(element.Name + " webURL=" + this.WebUrl);
#endif
            XmlNode nodeDatabases = element.SelectSingleNode("databases");
            if (nodeDatabases != null)
            {
                // maxResultCount

                // 获得整数型的属性参数值
                // return:
                //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
                //      0   正常获得明确定义的参数值
                //      1   参数没有定义，因此代替以缺省参数值返回
                int nRet = DomUtil.GetIntegerParam(nodeDatabases,
                    "maxResultCount",
                    -1,
                    out int nMaxResultCount,
                    out string strError);
                if (nRet == -1)
                {
                    strError = "<databases>元素" + strError;
                    throw new Exception(strError);
                }

                this.MaxResultCount = nMaxResultCount;
            }

            {
                int nRet = this.AppendDbProperties(out string strError);
                if (nRet == -1)
                    throw new Exception(strError);
            }

        }

#if NO
        // 获得一些比较耗时的配置参数。
        // return:
        //      -2  出错。但后面可以重试
        //      -1  出错，后面不再重试
        //      0   成功
        public int GetSlowCfgInfo(ServerConnection connection,
            out string strError)
        {
            lock (this)
            {
                strError = "";
                int nRet = 0;

                // 预先获得编目库属性列表
                nRet = GetBiblioDbProperties(connection, out strError);
                if (nRet == -1)
                    return -2;

                // 为数据库属性集合中增补需要从xml文件中获得的其他属性
                nRet = AppendDbProperties(out strError);
                if (nRet == -1)
                    return -1;

                return 0;
            }
        }
#endif

        public List<BiblioDbProperty> BiblioDbProperties = null;

#if NO
        // 获得编目库属性列表
        int GetBiblioDbProperties(ServerConnection connection,
            out string strError)
        {
            strError = "";

            LibraryChannel channel = connection.GetChannel(null);
            try
            {
                this.BiblioDbProperties = new List<BiblioDbProperty>();

                string strValue = "";
                long lRet = channel.GetSystemParameter(
                    "biblio",
                    "dbnames",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得编目库名列表过程发生错误：" + strError;
                    goto ERROR1;
                }

                string[] biblioDbNames = strValue.Split(new char[] { ',' });

                for (int i = 0; i < biblioDbNames.Length; i++)
                {
                    BiblioDbProperty property = new BiblioDbProperty();
                    property.DbName = biblioDbNames[i];
                    this.BiblioDbProperties.Add(property);
                }

                // 获得语法格式
                lRet = channel.GetSystemParameter(
                    "biblio",
                    "syntaxs",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得编目库数据格式列表过程发生错误：" + strError;
                    goto ERROR1;
                }

                string[] syntaxs = strValue.Split(new char[] { ',' });

                if (syntaxs.Length != this.BiblioDbProperties.Count)
                {
                    strError = "针对服务器 " + channel.Url + " 获得编目库名为 " + this.BiblioDbProperties.Count.ToString() + " 个，而数据格式为 " + syntaxs.Length.ToString() + " 个，数量不一致";
                    goto ERROR1;
                }

                // 增补数据格式
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    this.BiblioDbProperties[i].Syntax = syntaxs[i];
                }

                // 获得虚拟数据库名
                lRet = channel.GetSystemParameter(
                    "virtual",
                    "dbnames",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得虚拟库名列表过程发生错误：" + strError;
                    goto ERROR1;
                }
                string[] virtualDbNames = strValue.Split(new char[] { ',' });

                for (int i = 0; i < virtualDbNames.Length; i++)
                {
                    BiblioDbProperty property = new BiblioDbProperty();
                    property.DbName = virtualDbNames[i];
                    property.IsVirtual = true;
                    this.BiblioDbProperties.Add(property);
                }
            }
            finally
            {
                connection.ReturnChannel(channel);
            }

            return 0;
            ERROR1:
            this.BiblioDbProperties = null;
            return -1;
        }
#endif

        // 为数据库属性集合中增补需要从xml文件中获得的其他属性
        int AppendDbProperties(out string strError)
        {
            strError = "";

            // 增补MaxResultCount
            if (this._root == null)
            {
                strError = "调用 AppendDbProperties()以前，需要先初始化 _root";
                return -1;
            }

            Debug.Assert(this._root != null, "");

            this.BiblioDbProperties = new List<BiblioDbProperty>();

            XmlNodeList databases = _root.SelectNodes("databases/database");
            foreach (XmlElement nodeDatabase in databases)

            // for (int i = 0; i < this.BiblioDbProperties.Count; i++)
            {
#if NO
                BiblioDbProperty prop = this.BiblioDbProperties[i];

                string strDbName = prop.DbName;

                XmlElement nodeDatabase = _root.SelectSingleNode("databases/database[@name='" + strDbName + "']") as XmlElement;
                if (nodeDatabase == null)
                    continue;
#endif
                BiblioDbProperty prop = new BiblioDbProperty();
                this.BiblioDbProperties.Add(prop);
                prop.DbName = nodeDatabase.GetAttribute("name");

                // maxResultCount

                // 获得整数型的属性参数值
                // return:
                //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
                //      0   正常获得明确定义的参数值
                //      1   参数没有定义，因此代替以缺省参数值返回
                int nRet = DomUtil.GetIntegerParam(nodeDatabase,
                    "maxResultCount",
                    -1,
                    out prop.MaxResultCount,
                    out strError);
                if (nRet == -1)
                {
                    strError = "为数据库 '" + prop.DbName + "' 配置的<databases/database>元素的" + strError;
                    return -1;
                }

                // alias
                prop.DbNameAlias = DomUtil.GetAttr(nodeDatabase, "alias");

                // addField901
                // 2007/12/16
                nRet = DomUtil.GetBooleanParam(nodeDatabase,
                    "addField901",
                    false,
                    out prop.AddField901,
                    out strError);
                if (nRet == -1)
                {
                    strError = "为数据库 '" + prop.DbName + "' 配置的<databases/database>元素的" + strError;
                    return -1;
                }

                if (nodeDatabase.GetAttributeNode("removeFields") == null)
                    prop.RemoveFields = "997";
                else
                    prop.RemoveFields = nodeDatabase.GetAttribute("removeFields");
            }

            return 0;
        }

        #region

        // 获得可用的数据库数
        public int GetDbCount()
        {
            if (this.BiblioDbProperties == null)
                return 0;
            return this.BiblioDbProperties.Count;
        }

        // 根据书目库名获得书目库属性对象
        public BiblioDbProperty GetDbProperty(string strBiblioDbName,
            bool bSearchAlias)
        {
            if (this.BiblioDbProperties == null)
                throw new Exception("书目库参数尚未初始化");

            for (int i = 0; i < this.BiblioDbProperties.Count; i++)
            {
                if (this.BiblioDbProperties[i].DbName == strBiblioDbName)
                {
                    return this.BiblioDbProperties[i];
                }

                if (bSearchAlias == true)
                {
                    if (this.BiblioDbProperties[i].DbNameAlias.ToLower() == strBiblioDbName.ToLower())
                    {
                        return this.BiblioDbProperties[i];
                    }
                }

            }

            return null;
        }

#if NO
        // 根据书目库名获得MARC格式语法名
        public string GetMarcSyntax(string strBiblioDbName)
        {
            if (this.BiblioDbProperties == null)
                throw new Exception("书目库参数尚未初始化");

            for (int i = 0; i < this.BiblioDbProperties.Count; i++)
            {
                if (this.BiblioDbProperties[i].DbName == strBiblioDbName)
                {
                    string strResult = this.BiblioDbProperties[i].Syntax;
                    if (String.IsNullOrEmpty(strResult) == true)
                        strResult = "unimarc";  // 缺省为unimarc
                    return strResult;
                }
            }

            // 2007/8/9
            // 如果在this.BiblioDbProperties里面找不到，可以直接在xml配置的<database>元素中找
            XmlNode nodeDatabase = _root.SelectSingleNode("databases/database[@name='" + strBiblioDbName + "']");
            if (nodeDatabase == null)
                return null;

            return DomUtil.GetAttr(nodeDatabase, "marcSyntax");
        }
#endif

        // 根据书目库名(或者别名)获得检索途径名
        // parameters:
        //      strOutputDbName 输出的数据库名。不是Z39.50服务器定义的别名，而是正规数据库名。
        public string GetFromName(string strDbNameOrAlias,
            long lAttributeValue,
            out string strOutputDbName,
            out string strError)
        {
            strError = "";
            strOutputDbName = "";

            // 因为XMLDOM中无法进行大小写不敏感的搜索，所以把搜索别名的这个任务交给properties
            Debug.Assert(_root != null, "");
            BiblioDbProperty prop = this.GetDbProperty(strDbNameOrAlias, true);
            if (prop == null)
            {
                strError = "名字或者别名为 '" + strDbNameOrAlias + "' 的数据库不存在";
                return null;
            }

            strOutputDbName = prop.DbName;

            XmlNode nodeDatabase = _root.SelectSingleNode("databases/database[@name='" + strOutputDbName + "']");

            if (nodeDatabase == null)
            {
                strError = "名字为 '" + strOutputDbName + "' 的数据库不存在";
            }

            XmlNode nodeUse = nodeDatabase.SelectSingleNode("use[@value='" + lAttributeValue.ToString() + "']");
            if (nodeUse == null)
            {
                strError = "数据库 '" + strDbNameOrAlias + "' 中没有找到关于 '" + lAttributeValue.ToString() + "' 的检索途径定义";
                return null;
            }

            string strFrom = DomUtil.GetAttr(nodeUse, "from");
            if (String.IsNullOrEmpty(strFrom) == true)
            {
                strError = "数据库 '" + strDbNameOrAlias + "' <database>元素中关于 '" + lAttributeValue.ToString() + "' 的<use>配置缺乏from属性值";
                return null;
            }

            return strFrom;
        }

        #endregion
    }

    // 书目库属性
    // 注：除了 Syntax 以外，其他信息都应该可以从 capo.xml 中 zServer/databases/database 元素中获得
    // 可以在安装配置阶段，一次性从 dp2library 获取全部信息写入 database 元素，这样每次 zhost 启动时候就不用从 dp2library 获得数据库信息了
    public class BiblioDbProperty
    {
        // dp2library定义的特性
        public string DbName = "";  // 书目库名
                                    // public string Syntax = "";  // 格式语法

        public bool IsVirtual = false;  // 是否为虚拟库

        // 在dp2zserver.xml中定义的特性
        public int MaxResultCount = -1; // 检索命中的最多条数
        public string DbNameAlias = ""; // 数据库别名

        public bool AddField901 = false;    // 是否在MARC字段中加入表示记录路径和时间戳的的901字段

        // 2017/4/15
        public string RemoveFields = "997"; // 返回前要删除的字段名列表，逗号分隔。缺省为 "997"
    }

    // SIP 主机信息
    public class SipHostInfo : HostInfo
    {
        // 匿名访问时使用的用户名和密码
        public string AnonymousUserName { get; set; }
        public string AnonymousPassword { get; set; }

#if NO
        // 自动清理的秒数。0 表示不清理
        public int AutoClearSeconds { get; set; }

        // 日期格式
        public string DateFormat { get; set; }

        // 编码方式
        public Encoding Encoding { get; set; }

        // 前端 IP 地址白名单。空表示所有 IP 地址都许可，和 * 作用一致
        public string IpList { get; set; }
#endif

        new static readonly string EncryptKey = "dp2sipserver_password_key";

        public string DecryptPasssword(string strEncryptedText)
        {
            if (String.IsNullOrEmpty(strEncryptedText) == false)
            {
                try
                {
                    string strPassword = Cryptography.Decrypt(
        strEncryptedText,
        EncryptKey);
                    return strPassword;
                }
                catch
                {
                    return "errorpassword";
                }

            }

            return "";
        }

        public string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, SipHostInfo.EncryptKey);
        }

        private XmlElement _root = null;

        // parameters:
        //      element sipServer 元素
        public override void Initial(XmlElement element)
        {
            _root = element;

            // base.Initial(element);

            Debug.Assert(element != null, "");

            {
                // 取出一些常用的指标

                // 1) 图书馆应用服务器URL
                // 2) 管理员用的帐户和密码
                if (element.SelectSingleNode("dp2library") is XmlElement node)
                {
                    this.AnonymousUserName = node.GetAttribute("anonymousUserName");
                    string strPassword = node.GetAttribute("anonymousPassword");
                    this.AnonymousPassword = DecryptPasssword(strPassword);
                }
                else
                {
                    this.AnonymousUserName = "";
                    this.AnonymousPassword = "";
                }
            }

#if NO
            {
                // SIP 服务参数

                this.DateFormat = element.GetAttribute("dateFormat");
                if (string.IsNullOrEmpty(this.DateFormat))
                    this.DateFormat = SipServer.DEFAULT_DATE_FORMAT;

                string strEncoding = element.GetAttribute("encoding");
                if (string.IsNullOrEmpty(strEncoding) == false)
                    this.Encoding = Encoding.GetEncoding(strEncoding);
                else
                    this.Encoding = Encoding.GetEncoding(SipServer.DEFAULT_ENCODING_NAME);

                this.IpList = element.GetAttribute("ipList");

                // 2018/8/10
                string strAutoClearSeconds = element.GetAttribute("autoClearSeconds");
                int seconds = 0;
                if (string.IsNullOrEmpty(strAutoClearSeconds) == false
                    && Int32.TryParse(strAutoClearSeconds, out seconds) == false)
                {
                    throw new Exception("sipServer@autoClearSeconds 属性值 '"+strAutoClearSeconds+"' 不合法。应为纯数字");
                }
                this.AutoClearSeconds = seconds;
            }
#endif
        }

        // 获得一个用户相关的 SIP 服务参数
        public SipParam GetSipParam(string userName)
        {
            return SipParam.GetSipParam(this._root, userName);
        }
    }

    public class SipParam
    {
        // 自动清理的秒数。0 表示不清理
        public int AutoClearSeconds { get; set; }

        // 日期格式
        public string DateFormat { get; set; }

        // 编码方式
        public Encoding Encoding { get; set; }

        // 前端 IP 地址白名单。空表示所有 IP 地址都许可，和 * 作用一致
        public string IpList { get; set; }

        public static SipParam GetSipParam(XmlElement element1,
            string userName)
        {
            if (element1 == null)
                return null;

            if (!(element1.SelectSingleNode("user[@userName='" + userName + "']") is XmlElement user))
            {
                user = element1.SelectSingleNode("user[@userName='*']") as XmlElement;
                if (user == null)
                    throw new Exception("用户名 '" + userName + "' 在 capo.xml 中没有找到 SIP 配置信息");
            }

            // SIP 服务参数
            SipParam param = new SipParam();

            param.DateFormat = user.GetAttribute("dateFormat");
            if (string.IsNullOrEmpty(param.DateFormat))
                param.DateFormat = SipServer.DEFAULT_DATE_FORMAT;

            string strEncoding = user.GetAttribute("encoding");
            if (string.IsNullOrEmpty(strEncoding) == false)
                param.Encoding = Encoding.GetEncoding(strEncoding);
            else
                param.Encoding = Encoding.GetEncoding(SipServer.DEFAULT_ENCODING_NAME);

            param.IpList = user.GetAttribute("ipList");

            // 2018/8/10
            string strAutoClearSeconds = user.GetAttribute("autoClearSeconds");
            int seconds = 0;
            if (string.IsNullOrEmpty(strAutoClearSeconds) == false
                && Int32.TryParse(strAutoClearSeconds, out seconds) == false)
            {
                throw new Exception("sipServer@autoClearSeconds 属性值 '" + strAutoClearSeconds + "' 不合法。应为纯数字");
            }
            param.AutoClearSeconds = seconds;

            return param;
        }

    }
}
