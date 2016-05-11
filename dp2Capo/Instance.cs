using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Collections;
using System.Messaging;
using System.Diagnostics;

using DigitalPlatform.Text;
using DigitalPlatform.Message;
using DigitalPlatform;
using DigitalPlatform.IO;

namespace dp2Capo
{
    /// <summary>
    /// 一个服务器实例
    /// </summary>
    public class Instance
    {
        public LibraryHostInfo dp2library { get; set; }
        public HostInfo dp2mserver { get; set; }

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

        public void Initial(string strXmlFileName)
        {
            Console.WriteLine();
            Console.WriteLine("*** 初始化实例: " + strXmlFileName);

            this.Name = Path.GetDirectoryName(strXmlFileName);

            this.LogDir = Path.Combine(Path.GetDirectoryName(strXmlFileName), "log");
            PathUtil.CreateDirIfNeed(this.LogDir);

            // 验证一下日志文件是否允许写入。这样就可以设置一个标志，决定后面的日志信息写入文件还是 Windows 日志
            this.DetectWriteErrorLog("*** 实例 " + this.Name + " 开始启动");

            XmlDocument dom = new XmlDocument();
            dom.Load(strXmlFileName);

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2library") as XmlElement;
            if (element == null)
            {
                // throw new Exception("配置文件 " + strXmlFileName + " 中根元素下尚未定义 dp2library 元素");
                this.WriteErrorLog("配置文件 " + strXmlFileName + " 中根元素下尚未定义 dp2library 元素");
            }

            try
            {
                this.dp2library = new LibraryHostInfo();
                this.dp2library.Initial(element);

                element = dom.DocumentElement.SelectSingleNode("dp2mserver") as XmlElement;
                if (element == null)
                {
                    // throw new Exception("配置文件 " + strXmlFileName + " 中根元素下尚未定义 dp2mserver 元素");
                    this.WriteErrorLog("配置文件 " + strXmlFileName + " 中根元素下尚未定义 dp2mserver 元素");
                }

                this.dp2mserver = new HostInfo();
                this.dp2mserver.Initial(element);

                this.MessageConnection.dp2library = this.dp2library;
            }
            catch (Exception ex)
            {
                // throw new Exception("配置文件 " + strXmlFileName + " 格式错误: " + ex.Message);
                this.WriteErrorLog("配置文件 " + strXmlFileName + " 格式错误: " + ex.Message);
            }

            // 只要定义了队列就启动这个线程
            if (string.IsNullOrEmpty(this.dp2library.DefaultQueue) == false)
            {
                this._notifyThread = new NotifyThread();
                this._notifyThread.Container = this;
                this._notifyThread.BeginThread();    // TODO: 应该在 MessageConnection 第一次连接成功以后，再启动这个线程比较好
            }

            InitialQueue(true);
        }

        // parameters:
        //      bFirst  是否首次启动。首次启动和重试启动，写入日志的方式不同。
        void InitialQueue(bool bFirst)
        {
            try
            {
                if (_queue == null
                    && string.IsNullOrEmpty(this.dp2library.DefaultQueue) == false)
                {
                    _queue = new MessageQueue(this.dp2library.DefaultQueue);    // TODO: 不知道当 Queue 尚未创建的时候，这个语句是否可能抛出异常?
                    _queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                    _queue.BeginPeek(new TimeSpan(0, 1, 0), null, OnMessageAdded);

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
                if (bFirst)
                {
                    // Program.WriteWindowsLog("启动实例 " + this.Name + " 的 Queue '" + this.dp2library.DefaultQueue + "' 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
                    this.WriteErrorLog("首次启动队列 '" + this.dp2library.DefaultQueue + "' 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
                }
            }
        }

        public void BeginConnnect()
        {
            try
            {
                this.MessageConnection.ServerUrl = this.dp2mserver.Url;

                this.MessageConnection.UserName = this.dp2mserver.UserName;
                this.MessageConnection.Password = this.dp2mserver.Password;
                this.MessageConnection.Parameters = GetParameters();

                this.MessageConnection.InitialAsync();
            }
            catch (Exception ex)
            {
                this.WriteErrorLog("BeginConnect() 出现异常: " + ExceptionUtil.GetExceptionText(ex));
            }
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
            if (this._notifyThread != null)
                _notifyThread.StopThread(true);

            this.MessageConnection.CloseConnection();

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

        private void OnMessageAdded(IAsyncResult ar)
        {
            if (this._queue != null)
            {
                try
                {
                    if (_queue.EndPeek(ar) != null)
                        this._notifyThread.Activate();

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

        public void Notify()
        {
            // 如果第一次初始化 Queue 没有成功，这里再试探初始化
            InitialQueue(false);

            // 进行通知处理
            if (_queue != null
                && this.MessageConnection.IsConnected)
            {
                try
                {
                    MessageEnumerator iterator = _queue.GetMessageEnumerator2();
                    while (iterator.MoveNext())
                    {
                        Message message = iterator.Current;

                        MessageRecord record = new MessageRecord();
                        record.groups = new string[1]{"gn:_patronNotify"};  // gn 表示 group name
                        record.data = (string)message.Body;
                        record.format = "xml";
                        List<MessageRecord> records = new List<MessageRecord> { record };

                        SetMessageRequest param = new SetMessageRequest("create",
                            "dontNotifyMe",
                            records);
                        SetMessageResult result = this.MessageConnection.SetMessageAsync(param).Result;
                        if (result.Value == -1)
                        {
                            this.WriteErrorLog("Instance.Notify() 中 SetMessageAsync() 出错: " + result.ErrorInfo);
                            return;
                        }

                        iterator.RemoveCurrent();
                    }
                }
                catch (MessageQueueException ex)
                {
                    // 记入错误日志
                    // Program.WriteWindowsLog("Instance.Notify() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                    this.WriteErrorLog("Instance.Notify() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                }
                catch (InvalidCastException ex)
                {
                    // 记入错误日志
                    // Program.WriteWindowsLog("Instance.Notify() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                    this.WriteErrorLog("Instance.Notify() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                }
                catch (Exception ex)
                {
                    // 记入错误日志
                    // Program.WriteWindowsLog("Instance.Notify() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                    this.WriteErrorLog("Instance.Notify() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                }
            }
        }

        #region 日志

        bool _errorLogError = false;    // 写入实例的日志文件是否发生过错误

        void _writeErrorLog(string strText)
        {
            lock (this.LogDir)
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
        }
    }

    public class HostInfo
    {
        public static string EncryptKey = "dp2capopassword";

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
}
