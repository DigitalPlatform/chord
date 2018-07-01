using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Xml;
using System.Text;

using Microsoft.AspNet.SignalR.Client;
using log4net;

using DigitalPlatform;
using DigitalPlatform.Common;
using DigitalPlatform.Net;
using DigitalPlatform.MessageClient;
using DigitalPlatform.IO;
using DigitalPlatform.Z3950.Server;
using DigitalPlatform.Text;
using DigitalPlatform.Z3950;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Marc;
using DigitalPlatform.Message;

namespace dp2Capo
{
    public static class ServerInfo
    {
        public static ZServer ZServer { get; set; }


        // 实例集合
        public static List<Instance> _instances = new List<Instance>();

        // 管理线程
        public static DefaultThread _defaultThread = new DefaultThread();

        public static LifeThread _lifeThread = new LifeThread();

        public static RecordLockCollection _recordLocks = new RecordLockCollection();


        public static string DataDir
        {
            get;
            set;
        }

        const int Z3950_DEFAULT_PORT = 210;

        static int _z3950_port = -1;   // -1 表示不启用 Z39.50 Server
        public static int Z3950ServerPort
        {
            get
            {
                return _z3950_port;
            }
            set
            {
                _z3950_port = value;
            }
        }

        // 配置文件 XmlDocument
        public static XmlDocument ConfigDom = null; // new XmlDocument();

        internal static CancellationTokenSource _cancel = new CancellationTokenSource();



        public static void LoadCfg(string strDataDir, bool bAutoCreate = true)
        {
            string strCfgFileName = Path.Combine(strDataDir, "config.xml");
            try
            {
                ConfigDom = new XmlDocument();
                ConfigDom.Load(strCfgFileName);

                // 元素 <zServer>
                // 属性 port
                XmlElement node = ConfigDom.DocumentElement.SelectSingleNode("zServer") as XmlElement;
                if (node != null && node.HasAttribute("port"))
                {
                    string v = node.GetAttribute("port");
                    if (string.IsNullOrEmpty(v))
                        _z3950_port = -1;
                    else
                    {
                        if (Int32.TryParse(v, out int port) == false)
                            throw new Exception("port 属性值必须是整数");
                        _z3950_port = port;
                    }
                }
                else
                {
                    _z3950_port = -1;  // -1 表示不启用 Z39.50 Server
                }
            }
            catch (FileNotFoundException ex)
            {
                if (bAutoCreate == true)
                {
                    ConfigDom.LoadXml("<root />");
                }
                else
                    throw new Exception("配置文件 '" + strCfgFileName + "' 不存在", ex);
            }
            catch (Exception ex)
            {
                // WriteErrorLog("装载配置文件 '" + strCfgFileName + "' 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
                throw new Exception("装载配置文件 '" + strCfgFileName + "' 时出现异常: " + ex.Message, ex);
            }
        }

        public static void SaveCfg(string strDataDir)
        {
            if (ConfigDom == null)
                throw new Exception("尚未执行 Initial()");

            XmlElement node = ConfigDom.DocumentElement.SelectSingleNode("zServer") as XmlElement;
            if (node == null)
            {
                node = ConfigDom.CreateElement("zServer");
                ConfigDom.DocumentElement.AppendChild(node);
            }

            node.SetAttribute("port", _z3950_port.ToString());

            string strCfgFileName = Path.Combine(strDataDir, "config.xml");
            PathUtil.CreateDirIfNeed(strDataDir);
            ConfigDom.Save(strCfgFileName);
        }


        // 从数据目录装载全部实例定义，并连接服务器
        public static void Initial(string strDataDir)
        {
            DataDir = strDataDir;
            LogDir = Path.Combine(strDataDir, "log");   // 日志目录
            PathUtil.CreateDirIfNeed(LogDir);

            var repository = log4net.LogManager.CreateRepository("main");
            log4net.GlobalContext.Properties["LogFileName"] = Path.Combine(LogDir, "log_");
            log4net.Config.XmlConfigurator.Configure(repository);

            ZManager.Log = LogManager.GetLogger("main", "zlib");
            _log = LogManager.GetLogger("main", "capo");

            string strVersion = Assembly.GetAssembly(typeof(ServerInfo)).GetName().Version.ToString();

            // 验证一下日志文件是否允许写入。这样就可以设置一个标志，决定后面的日志信息写入文件还是 Windows 日志
            // DetectWriteErrorLog("*** dp2Capo 开始启动 (dp2Capo 版本: " + strVersion + ")");
            WriteLog("info", "*** dp2Capo 开始启动 (dp2Capo 版本: " + strVersion + ")");

            try
            {
                ServerInfo.LoadCfg(DataDir);
            }
            catch (Exception ex)
            {
                WriteLog("fatal", "dp2Capo 装载全局配置文件阶段出错: " + ex.Message);
                throw ex;
            }

            _exited = false;

            DirectoryInfo root = new DirectoryInfo(strDataDir);
            var dis = root.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                if (di.Name.ToLower() == "log")
                    continue;
                string strXmlFileName = Path.Combine(di.FullName, "capo.xml");
                if (File.Exists(strXmlFileName) == false)
                    continue;
                Instance instance = new Instance();
                instance.Initial(strXmlFileName);
                _instances.Add(instance);
            }

#if NO
            // 连接服务器，如果暂时连接失败，后面会不断自动重试连接
            foreach (Instance instance in _instances)
            {
                instance.BeginConnnect();
            }
#endif

            _defaultThread.BeginThread();

            _lifeThread.BeginThread();
        }

        // 运用控制台显示方式，设置一个实例的基本参数
        // parameters:
        //      index   实例子目录下标。从 0 开始计数
        public static void ChangeInstanceSettings(string strDataDir, int index)
        {
            DirectoryInfo root = new DirectoryInfo(strDataDir);
            var dis = root.GetDirectories();
            int i = 0;
            foreach (DirectoryInfo di in dis)
            {
                string strXmlFileName = Path.Combine(di.FullName, "capo.xml");

                if (i == index)
                {
                    Instance.ChangeSettings(strXmlFileName);
                    return;
                }
                i++;
            }

            // throw new Exception("下标 "+index.ToString()+" 超过了当前实际存在的实例数");

            // 创建足够多的新实例子目录
            for (; i <= index; i++)
            {
                string strName = Guid.NewGuid().ToString();
                string strInstanceDir = Path.Combine(strDataDir, strName);
                Directory.CreateDirectory(strInstanceDir);
                if (i == index)
                {
                    string strXmlFileName = Path.Combine(strInstanceDir, "capo.xml");
                    Instance.ChangeSettings(strXmlFileName);
                    return;
                }
            }
        }

        static bool _exited = false;
        // 准备退出
        public static void Exit()
        {
            if (_exited == false)
            {
                _lifeThread.StopThread(true);
                _lifeThread.Dispose();

                _defaultThread.StopThread(true);    // 强制退出
                _defaultThread.Dispose();

                // 保存配置

                // 切断连接
                foreach (Instance instance in _instances)
                {
                    instance.Close();
                }
                _exited = true;

                WriteLog("info", "*** dp2Capo 成功降落");
                _logger.Dispose();
            }
        }

        static TimeSpan LongTime = TimeSpan.FromMinutes(5);    // 10

        static bool NeedCheck(Instance instance)
        {
            DateTime now = DateTime.Now;

            ConnectionState state = instance.MessageConnection.ConnectState;

            if (state == ConnectionState.Disconnected)
            {
                instance.LastCheckTime = now;
                return true;
            }
            else
            {
                if (now - instance.LastCheckTime >= LongTime)
                {
                    instance.LastCheckTime = now;
                    return true;
                }
            }

            return false;
        }

        static void Echo(Instance instance, bool writeLog)
        {
            if (instance.MessageConnection.ConnectState != ConnectionState.Connected)
                return;

            // 验证一次请求
            // string text = Guid.NewGuid().ToString();
            string text = "!verify";

            if (writeLog)
                instance.WriteErrorLog("Begin echo: " + text);

            try
            {
                string result = instance.MessageConnection.EchoTaskAsync(text, TimeSpan.FromSeconds(5), instance._cancel.Token).Result;

                // 此用法在 dp2mserver 不存在 echo() API 的时候会挂起当前线程
                // string result = instance.MessageConnection.echo(text).Result;

                if (result == null)
                    result = "(timeout)";

                if (writeLog)
                    instance.WriteErrorLog("End   echo: " + result);
            }
            catch (Exception ex)
            {
                instance.WriteErrorLog("echo 出现异常: " + ExceptionUtil.GetExceptionText(ex));
                {
                    string strErrorCode = "";
                    if (MessageConnection.IsHttpClientException(ex, out strErrorCode))
                    {
                        // echo 的时候有小概率可能会返回用户认证异常？重置连接
                        Task.Run(() => instance.TryResetConnection());
                    }
                }
            }

        }

        /*
*
.NET Runtime 	Error 	2016/11/4 13:19:54
Application: dp2capo.exe
Framework Version: v4.0.30319
Description: The process was terminated due to an unhandled exception.
Exception Info: System.Net.Sockets.SocketException
   at System.Net.Dns.GetAddrInfo(System.String)
   at System.Net.Dns.InternalGetHostByName(System.String, Boolean)
   at System.Net.Dns.GetHostAddresses(System.String)
   at System.Net.NetworkInformation.Ping.Send(System.String, Int32, Byte[], System.Net.NetworkInformation.PingOptions)

Exception Info: System.Net.NetworkInformation.PingException
   at System.Net.NetworkInformation.Ping.Send(System.String, Int32, Byte[], System.Net.NetworkInformation.PingOptions)
   at DigitalPlatform.Net.NetUtil.Ping(System.String, System.String ByRef)
   at dp2Capo.ServerInfo.Check(dp2Capo.Instance, System.Collections.Generic.List`1<System.Threading.Tasks.Task>)
   at dp2Capo.ServerInfo.BackgroundWork()
   at dp2Capo.DefaultThread.Worker()
   at DigitalPlatform.ThreadBase.ThreadMain()
   at System.Threading.ExecutionContext.RunInternal(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object, Boolean)
   at System.Threading.ExecutionContext.Run(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object, Boolean)
   at System.Threading.ExecutionContext.Run(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object)
   at System.Threading.ThreadHelper.ThreadStart()
         * */
        static void Check(Instance instance, List<Task> tasks)
        {
            if (instance.dp2mserver == null)
                return;

            if (instance.MessageConnection.ConnectState == ConnectionState.Disconnected)    // 注：如果在 Connecting 和 Reconnecting 状态则不尝试 ConnectAsync
            {
                // instance.BeginConnnect();
                tasks.Add(instance.BeginConnectTask());

                Uri uri = new Uri(instance.dp2mserver.Url);
                try
                {
                    string strInformation = "";
                    if (NetUtil.Ping(uri.DnsSafeHost, out strInformation) == true)
                    {
                        instance.WriteErrorLog("ping '" + uri.DnsSafeHost + "' success");
                    }
                    else
                    {
                        instance.WriteErrorLog("ping '" + uri.DnsSafeHost + "' fail: " + strInformation);
                    }
                }
                catch (Exception ex)
                {
                    instance.WriteErrorLog("ping '" + uri.DnsSafeHost + "' 出现异常: " + ExceptionUtil.GetExceptionText(ex));
                }

            }
            else
            {
                Echo(instance, true);
            }
        }

        // 检测是否发生了死锁
        // return:
        //      true    发生了死锁
        //      false   没有发生死锁
        public static bool DetectDeadLock()
        {
            if (_instances == null || _instances.Count == 0)
                return false;
            DateTime now = DateTime.Now;
            TimeSpan delta = TimeSpan.FromMinutes(10);  // 10 分钟没有动作就表明死锁了
            foreach (Instance instance in _instances)
            {
                if (now - instance.LastCheckTime > delta)
                    return true;
            }

            return false;
        }

        // 向第一个实例的日志文件中写入
        public static void WriteFirstInstanceErrorLog(string strText)
        {
            if (_instances == null || _instances.Count == 0)
                return;
            else
            {
                try
                {
                    _instances[0].WriteErrorLog(strText);
                }
                catch
                {
                    Program.WriteWindowsLog(strText);
                }
            }
        }

        static DateTime _lastCleanTime = DateTime.Now;

        // 执行一些后台管理任务
        public static void BackgroundWork()
        {
            List<Task> tasks = new List<Task>();

            bool bOutputBegin = false;

            Instance first_instance = null;
            if (_instances.Count > 0)
                first_instance = _instances[0];
            foreach (Instance instance in _instances)
            {
                // 向 dp2mserver 发送心跳消息
                // instance.SendHeartBeat();

                bool bNeedCheck = NeedCheck(instance);

                if (bNeedCheck)
                {
                    instance.WriteErrorLog("<<< BackgroundWork 开始一轮处理\r\n状态:\r\n" + instance.GetDebugState());
                    bOutputBegin = true;
                }

                string strError = "";
                // 利用 dp2library API 获取一些配置信息
                if (string.IsNullOrEmpty(instance.dp2library.LibraryUID) == true)
                {
                    int nRet = instance.MessageConnection.GetConfigInfo(out strError);
                    if (nRet == -1)
                    {
                        // Program.WriteWindowsLog(strError);
                        instance.WriteErrorLog("获得 dp2library 配置时出错: " + strError);
                    }
                    else
                    {
#if NO
                        if (instance.MessageConnection.IsConnected == false)
                        {
                            tasks.Add(instance.BeginConnectTask()); // 2016/10/13 以前没有 if 语句，那样就容易导致重复 BeginConnect()
                        }
#endif
                        if (bNeedCheck)
                            Check(instance, tasks);
                        else
                            Echo(instance, false);
                    }
                }
                else
                {
#if NO
                    if (instance.MessageConnection.IsConnected == false)
                    {
                        // instance.BeginConnnect();
                        tasks.Add(instance.BeginConnectTask());
                    }
                    else
                    {
                        // TODO: 验证一次请求
                    }
#endif

                    if (bNeedCheck)
                        Check(instance, tasks);
                    else
                        Echo(instance, false);

                    // 每隔二十分钟清理一次闲置的 dp2library 通道
                    if (DateTime.Now - _lastCleanTime >= TimeSpan.FromMinutes(20))
                    {
                        try
                        {
                            instance.MessageConnection.CleanLibraryChannel();
                        }
                        catch (Exception ex)
                        {
                            instance.WriteErrorLog("CleanLibraryChannel() 异常: " + ExceptionUtil.GetExceptionText(ex));
                        }
                        _lastCleanTime = DateTime.Now;
                    }

                    // 重试初始化 ZHost 慢速参数
                    // instance.InitialZHostSlowConfig();

                    // 清理闲置超期的 zChannels
                    ZServer._zChannels.CleanIdleChannels(TimeSpan.FromMinutes(2));

                    // 清除废弃的全局结果集
                    Task.Run(() => instance.FreeGlobalResultSets());
                }
            }

            // 阻塞，直到全部任务完成。避免 BeginConnect() 函数被重叠调用
            if (tasks.Count > 0)
            {
                // test 这一句可以用来制造死锁测试场景
                // Thread.Sleep(60 * 60 * 1000);

                if (first_instance != null)
                    first_instance.WriteErrorLog("-- BackgroundWork - 等待 " + tasks.Count + " 个 Connect 任务完成");

                Task.WaitAll(tasks.ToArray());

                if (first_instance != null)
                    first_instance.WriteErrorLog("-- BackgroundWork - " + tasks.Count + " 个 Connect 任务已经完成");
            }

            if (bOutputBegin == true && first_instance != null)
                first_instance.WriteErrorLog(">>> BackgroundWork 结束一轮处理\r\n");
        }

        public static string Version
        {
            get
            {
                return Assembly.GetAssembly(typeof(Instance)).GetName().Version.ToString();
            }
        }

        #region 日志

        public static Logger _logger = new Logger();

        static string LogDir
        {
            get;
            set;
        }

        // private static readonly Object _syncRoot = new Object();

        static bool _errorLogError = false;    // 写入实例的日志文件是否发生过错误

#if NO
        static void _writeErrorLog(string strText)
        {
            _logger.Write(LogDir, strText);
        }

        // 尝试写入实例的错误日志文件
        // return:
        //      false   失败
        //      true    成功
        public static bool DetectWriteErrorLog(string strText)
        {
            _errorLogError = false;
            try
            {
                _writeErrorLog(strText);
            }
            catch (Exception ex)
            {
                WriteWindowsLog("尝试写入目录 " + LogDir + " 的日志文件发生异常， 后面将改为写入 Windows 日志。异常信息如下：'" + ExceptionUtil.GetDebugText(ex) + "'", EventLogEntryType.Error);
                _errorLogError = true;
                return false;
            }

            WriteWindowsLog("日志文件检测正常。从此开始关于 dp2Capo 的日志会写入目录 " + LogDir + " 中当天的日志文件",
                EventLogEntryType.Information);
            return true;
        }
#endif


#if NO
        // 写入实例的错误日志文件
        public static void WriteErrorLog(string strText)
        {
            Console.WriteLine(strText);

            if (_errorLogError == true) // 先前写入实例的日志文件发生过错误，所以改为写入 Windows 日志。会加上实例名前缀字符串
                WriteWindowsLog(strText, EventLogEntryType.Error);
            else
            {
                try
                {
                    _writeErrorLog(strText);
                }
                catch (Exception ex)
                {
                    WriteWindowsLog("因为原本要写入日志文件的操作发生异常， 所以不得不改为写入 Windows 日志(见后一条)。异常信息如下：'" + ExceptionUtil.GetDebugText(ex) + "'", EventLogEntryType.Error);
                    WriteWindowsLog(strText, EventLogEntryType.Error);
                }
            }
        }
#endif
        public static ILog Log
        {
            get
            {
                return _log;
            }
        }

        static ILog _log = null;

        // 写入全局日志文件
        public static void WriteErrorLog(string strText)
        {
            WriteLog("error", strText);
        }

        // 写入全局日志文件
        // parameters:
        //      level   info/error
        public static void WriteLog(string level, string strText)
        {
            Console.WriteLine(strText);

            if (_errorLogError == true || _log == null) // 先前写入实例的日志文件发生过错误，所以改为写入 Windows 日志。会加上实例名前缀字符串
                WriteWindowsLog(strText, EventLogEntryType.Error);
            else
            {
                try
                {
                    if (level == "info")
                        _log.Info(strText);
                    else if (level == "fatal")
                        _log.Fatal(strText);
                    else
                        _log.Error(strText);
                }
                catch (Exception ex)
                {
                    WriteWindowsLog("因为原本要写入日志文件的操作发生异常， 所以不得不改为写入 Windows 日志(见后一条)。异常信息如下：'" + ExceptionUtil.GetDebugText(ex) + "'", EventLogEntryType.Error);
                    WriteWindowsLog(strText, EventLogEntryType.Error);
                }
            }
        }

        // 写入 Windows 日志
        public static void WriteWindowsLog(string strText)
        {
            WriteWindowsLog(strText, EventLogEntryType.Error);
        }

        // 写入 Windows 日志
        public static void WriteWindowsLog(string strText,
            EventLogEntryType type)
        {
            EventLog Log = new EventLog();
            Log.Source = "dp2CapoService";
            Log.WriteEntry(strText, type);
        }

        #endregion

        public static void AddEvents(ZServer zserver, bool bAdd)
        {
            if (bAdd)
            {
                zserver.SetChannelProperty += Zserver_SetChannelProperty;
                //zserver.GetZConfig += Zserver_GetZConfig;
                zserver.InitializeLogin += Zserver_InitializeLogin;
                zserver.SearchSearch += Zserver_SearchSearch;
                zserver.PresentGetRecords += Zserver_PresentGetRecords;
                zserver.ChannelOpened += Zserver_ChannelOpened;
                //zserver.ChannelClosed += Zserver_ChannelClosed;
            }
            else
            {
                zserver.SetChannelProperty -= Zserver_SetChannelProperty;
                //zserver.GetZConfig -= Zserver_GetZConfig;
                zserver.InitializeLogin -= Zserver_InitializeLogin;
                zserver.SearchSearch -= Zserver_SearchSearch;
                zserver.PresentGetRecords -= Zserver_PresentGetRecords;
                zserver.ChannelOpened += Zserver_ChannelOpened;
                //zserver.ChannelClosed -= Zserver_ChannelClosed;
            }
        }

        private static void Zserver_ChannelOpened(object sender, EventArgs e)
        {
            ZServerChannel channel = (ZServerChannel)sender;
            channel.Closed += Channel_Closed;
        }

        private static void Channel_Closed(object sender, EventArgs e)
        {
            ZServerChannel channel = (ZServerChannel)sender;
            channel.Closed -= Channel_Closed;   // 避免重入

            List<string> names = GetResultSetNameList(channel, true);
            if (names.Count > 0)
            {
                FreeGlobalResultSets(channel, names);
            }
        }

#if NO
        private static void Zserver_ChannelClosed(object sender, ChannelClosedEventArgs e)
        {
            List<string> names = GetResultSetNameList(e.Channel);
            if (names.Count > 0)
            {
                FreeGlobalResultSets(e.Channel, names);
            }
        }
#endif

        private static void Zserver_PresentGetRecords(object sender, PresentGetRecordsEventArgs e)
        {
            string strError = "";

            ZServerChannel zserver_channel = (ZServerChannel)sender;

            string strInstanceName = zserver_channel.SetProperty().GetKeyValue("i_n");
            if (strInstanceName == null)
            {
                strError = "通道中 实例名 '" + strInstanceName + "' 尚未初始化";
                ZManager.Log.Error(strError);
                goto ERROR1;
            }
            Instance instance = FindInstance(strInstanceName);
            if (instance == null)
            {
                strError = "实例名 '" + strInstanceName + "' 不存在(或实例没有启用 Z39.50 服务)";
                goto ERROR1;
            }

            string strResultSetName = e.Request.m_strResultSetID;
            if (String.IsNullOrEmpty(strResultSetName) == true)
                strResultSetName = "default";
            long lStart = e.Request.m_lResultSetStartPoint - 1;
            long lNumber = e.Request.m_lNumberOfRecordsRequested;

            int MAX_PRESENT_RECORD = 100;

            // 限制每次 present 的记录数量
            if (lNumber > MAX_PRESENT_RECORD)
                lNumber = MAX_PRESENT_RECORD;

            DiagFormat diag = null;
            List<RetrivalRecord> records = new List<RetrivalRecord>();

            string strUserName = zserver_channel.SetProperty().GetKeyValue("i_u");
            string strPassword = zserver_channel.SetProperty().GetKeyValue("i_p");

            LoginInfo login_info = new LoginInfo { UserName = strUserName, Password = strPassword };
            LibraryChannel library_channel = instance.MessageConnection.GetChannel(login_info);
            try
            {
                // 全局结果集名
                string resultset_name = MakeGlobalResultSetName(zserver_channel, strResultSetName);

                ResultSetLoader loader = new ResultSetLoader(library_channel,
                    resultset_name,
                    "id,xml,timestamp")
                {
                    Start = lStart,
                    BatchSize = Math.Min(10, lNumber)
                };
                int i = 0;
                int nSize = 0;
                foreach (DigitalPlatform.LibraryClient.localhost.Record dp2library_record in loader)
                {
                    if (i >= lNumber)
                        break;

                    if (i == 0)
                    {
                        e.TotalCount = loader.TotalCount;
                        if (lStart >= loader.TotalCount)
                        {
                            DiagFormat diag1 = null;
                            ZProcessor.SetPresentDiagRecord(ref diag1,
    13,  // Present request out-of-range
    "Present 所请求的起始偏移位置 " + lStart + " 超过结果集中记录总数 " + loader.TotalCount);
                            e.Diag = diag1;
                            return;
                        }
                    }

                    {
                        // 解析出数据库名和ID
                        string strDbName = dp2StringUtil.GetDbName(dp2library_record.Path);
                        string strRecID = dp2StringUtil.GetRecordID(dp2library_record.Path);

                        // 如果取得的是xml记录，则根元素可以看出记录的marc syntax，进一步可以获得oid；
                        // 如果取得的是MARC格式记录，则需要根据数据库预定义的marc syntax来看出oid了
                        // string strMarcSyntaxOID = GetMarcSyntaxOID(instance, strDbName);
                        string strMarcSyntaxOID = GetMarcSyntaxOID(dp2library_record);

                        RetrivalRecord record = new RetrivalRecord
                        {
                            m_strDatabaseName = strDbName
                        };

                        // 根据书目库名获得书目库属性对象
                        BiblioDbProperty prop = instance.zhost.GetDbProperty(
                            strDbName,
                            false);

                        int nRet = GetIso2709Record(dp2library_record,
                            e.Request.m_elementSetNames,
                            prop != null ? prop.AddField901 : false,
                            prop != null ? prop.RemoveFields : "997",
                            zserver_channel.SetProperty().MarcRecordEncoding,
                            out byte[] baIso2709,
                            out strError);

                        /*
                                                // 测试记录群中包含诊断记录
                                                if (i == 1)
                                                {
                                                    nRet = -1;
                                                    strError = "测试获取记录错误";
                                                }
                        */
                        if (nRet == -1)
                        {
                            record.m_surrogateDiagnostic = new DiagFormat
                            {
                                m_strDiagSetID = "1.2.840.10003.4.1",
                                m_nDiagCondition = 14,  // system error in presenting records
                                m_strAddInfo = strError
                            };
                        }
                        else if (nRet == 0)
                        {
                            record.m_surrogateDiagnostic = new DiagFormat
                            {
                                m_strDiagSetID = "1.2.840.10003.4.1",
                                m_nDiagCondition = 1028,  // record deleted
                                m_strAddInfo = strError
                            };
                        }
                        else if (String.IsNullOrEmpty(strMarcSyntaxOID) == true)
                        {
                            // 根据数据库名无法获得marc syntax oid。可能是虚拟库检索命中记录所在的物理库没有在 capo.xml 中配置。
                            record.m_surrogateDiagnostic = new DiagFormat
                            {
                                m_strDiagSetID = "1.2.840.10003.4.1",
                                m_nDiagCondition = 109,  // database unavailable // 似乎235:database dos not exist也可以
                                m_strAddInfo = "根据数据库名 '" + strDbName + "' 无法获得 marc syntax oid"
                            };
                        }
                        else
                        {
                            record.m_external = new External
                            {
                                m_strDirectRefenerce = strMarcSyntaxOID,
                                m_octectAligned = baIso2709
                            };
                        }

                        nSize += record.GetPackageSize();

                        if (i == 0)
                        {
                            // 连一条记录也放不下
                            if (nSize > zserver_channel.SetProperty().ExceptionalRecordSize)
                            {
                                Debug.Assert(diag == null, "");
                                ZProcessor.SetPresentDiagRecord(ref diag,
                                    17, // record exceeds Exceptional_record_size
                                    "记录尺寸 " + nSize.ToString() + " 超过 Exceptional_record_size " + zserver_channel.SetProperty().ExceptionalRecordSize.ToString());
                                lNumber = 0;
                                break;
                            }
                        }
                        else
                        {
                            if (nSize >= zserver_channel.SetProperty().PreferredMessageSize)
                            {
                                // 调整返回的记录数
                                lNumber = i;
                                break;
                            }
                        }

                        records.Add(record);
                    }

                    i++;
                }
            }
            catch(ChannelException ex)
            {
                // 指定的结果集没有找到
                if (ex.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                {
                    DiagFormat diag1 = null;
                    ZProcessor.SetPresentDiagRecord(ref diag1,
30,  // Specified result set does not exist
ex.Message);
                    e.Diag = diag1;
                    return;
                }
                strError = "获取结果集时出现异常(ChannelException): " + ex.Message;
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "获取结果集时出现异常: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                instance.MessageConnection.ReturnChannel(library_channel);
            }

            e.Records = records;
            e.Diag = diag;
            return;
            ERROR1:
            {
                DiagFormat diag1 = null;
                ZProcessor.SetPresentDiagRecord(ref diag1,
                    100,  // (unspecified) error
                    strError);
                e.Diag = diag1;
                // e.Result = new ZClient.SearchResult { Value = -1, ErrorInfo = strError };
                return;
            }
        }

#if NO
        static string GetMarcSyntaxOID(Instance instance, string strBiblioDbName)
        {
            string strSyntax = instance.zhost.GetMarcSyntax(strBiblioDbName);
            if (strSyntax == null)
                return null;
            if (strSyntax == "unimarc")
                return "1.2.840.10003.5.1";
            if (strSyntax == "usmarc")
                return "1.2.840.10003.5.10";

            return null;
        }
#endif

        // 获得一条记录的 MARC 格式 OID
        static string GetMarcSyntaxOID(DigitalPlatform.LibraryClient.localhost.Record dp2library_record)
        {
            if (dp2library_record.RecordBody == null)
                return null;

            if (string.IsNullOrEmpty(dp2library_record.RecordBody.Xml))
                return null;

            // return:
            //      -1  出错
            //      0   正常
            int nRet = MarcUtil.GetMarcSyntax(dp2library_record.RecordBody.Xml,
        out string strOutMarcSyntax,
        out string strError);
            if (nRet == -1)
                throw new Exception("获得 MARCXML 记录的 MARC Syntax 时出错: " + strError);

            if (strOutMarcSyntax == "unimarc")
                return "1.2.840.10003.5.1";
            if (strOutMarcSyntax == "usmarc")
                return "1.2.840.10003.5.10";

            return null;
        }

        // 获得MARC记录
        // parameters:
        //      bAddField901    是否加入901字段？
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        static int GetIso2709Record(
            DigitalPlatform.LibraryClient.localhost.Record dp2library_record,
            List<string> elementSetNames,
            bool bAddField901,
            string strRemoveFields,
            Encoding marcRecordEncoding,
            out byte[] baIso2709,
            out string strError)
        {
            baIso2709 = null;
            strError = "";

            string strMarcSyntax = "";

            // 转换为机内格式
            int nRet = MarcUtil.Xml2Marc(dp2library_record.RecordBody.Xml,
                true,
                strMarcSyntax,
                out string strOutMarcSyntax,
                out string strMarc,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            // 去掉记录中的 997/998
            MarcRecord record = new MarcRecord(strMarc);
            if (string.IsNullOrEmpty(strRemoveFields) == false)
            {
                List<string> field_names = StringUtil.SplitList(strRemoveFields);
                foreach (string field_name in field_names)
                {
                    if (field_name.Length != 3)
                    {
                        strError = "removeFields 定义里面出现了不是 3 字符的字段名('" + strRemoveFields + "')";
                        return -1;
                    }
                    record.select("field[@name='" + field_name + "']").detach();
                }
            }

            if (bAddField901 == true)
            {
                // 901  $p记录路径$t时间戳
                string strContent = "$p" + dp2library_record.Path
                    + "$t" + ByteArray.GetHexTimeStampString(dp2library_record.RecordBody.Timestamp);
                record.setFirstField("901", "  ", strContent.Replace("$", MarcQuery.SUBFLD), "  ");
            }
            strMarc = record.Text;

            // 转换为ISO2709
            nRet = MarcUtil.CvtJineiToISO2709(
                strMarc,
                strOutMarcSyntax,
                marcRecordEncoding,
                out baIso2709,
                out strError);
            if (nRet == -1)
                return -1;

            return 1;
        }

        private static void Zserver_SearchSearch(object sender, SearchSearchEventArgs e)
        {
            ZServerChannel zserver_channel = (ZServerChannel)sender;

            string strInstanceName = zserver_channel.SetProperty().GetKeyValue("i_n");
            if (strInstanceName == null)
            {
                string strErrorText = "通道中 实例名 '" + strInstanceName + "' 尚未初始化";
                ZManager.Log?.Error(strErrorText);
                e.Result = new DigitalPlatform.Z3950.ZClient.SearchResult { Value = -1, ErrorInfo = strErrorText };
                return;
            }
            Instance instance = FindInstance(strInstanceName);
            if (instance == null)
            {
                e.Result = new DigitalPlatform.Z3950.ZClient.SearchResult { Value = -1, ErrorInfo = "实例名 '" + strInstanceName + "' 不存在(或实例没有启用 Z39.50 服务)" };
                return;
            }

            // 检查实例是否有至少一个可用数据库
            if (instance.zhost.GetDbCount() == 0)
            {
                string strErrorText = "实例 '" + strInstanceName + "' 没有提供可检索的数据库";
                DiagFormat diag = null;
                ZProcessor.SetPresentDiagRecord(ref diag,
                    1017,  // Init/AC: No databases available for specified userId
                    strErrorText);
                e.Diag = diag;
                e.Result = new ZClient.SearchResult { Value = -1, ErrorInfo = strErrorText };
                return;
            }

            // 根据逆波兰表构造出 dp2 系统检索式
            // return:
            //      -1  出错
            //      0   数据库没有找到
            //      1   成功
            int nRet = Z3950Utility.BuildQueryXml(
                instance.zhost,
                e.Request.m_dbnames,
                e.Request.m_rpnRoot,
                zserver_channel.SetProperty().SearchTermEncoding,
                out string strQueryXml,
                out string strError);
            if (nRet == -1 || nRet == 0)
            {
                DiagFormat diag = null;
                ZProcessor.SetPresentDiagRecord(ref diag,
                    nRet == -1 ? 2 : 235,  // 2:temporary system error; 235:Database does not exist (database name)
                    strError);
                e.Diag = diag;
                e.Result = new ZClient.SearchResult { Value = -1, ErrorInfo = strError };
                return;
            }

            string strUserName = zserver_channel.SetProperty().GetKeyValue("i_u");
            string strPassword = zserver_channel.SetProperty().GetKeyValue("i_p");

            LoginInfo login_info = new LoginInfo { UserName = strUserName, Password = strPassword };

            LibraryChannel library_channel = instance.MessageConnection.GetChannel(login_info);
            try
            {
                // 全局结果集名
                string resultset_name = MakeGlobalResultSetName(zserver_channel, e.Request.m_strResultSetName);
                // 进行检索
                long lRet = library_channel.Search(
        strQueryXml,
        resultset_name,
        "", // strOutputStyle
        out strError);

                /*
                // 测试检索失败
                lRet = -1;
                strError = "测试检索失败";
                 * */

                if (lRet == -1)
                {
                    DiagFormat diag = null;
                    ZProcessor.SetPresentDiagRecord(ref diag,
                        2,  // temporary system error
                        strError);
                    e.Diag = diag;
                    e.Result = new ZClient.SearchResult { Value = -1, ErrorInfo = strError };
                    return;
                }
                else
                {
                    // 记忆结果集名
                    // return:
                    //      false   正常
                    //      true    结果集数量超过 MAX_RESULTSET_COUNT，返回前已经开始释放所有结果集
                    if (MemoryResultSetName(zserver_channel, resultset_name) == true)
                    {
                        DiagFormat diag = null;
                        ZProcessor.SetPresentDiagRecord(ref diag,
                            112,  // Too many result sets created (maximum)
                            strError);  // TODO: 应为 MAX_RESULTSET_COUNT
                        e.Diag = diag;
                        e.Result = new ZClient.SearchResult { Value = -1, ErrorInfo = strError };
                        return;
                    }

                    e.Result = new ZClient.SearchResult { ResultCount = lRet };
                }
            }
            finally
            {
                instance.MessageConnection.ReturnChannel(library_channel);
            }
        }

        // 构造全局结果集名
        static string MakeGlobalResultSetName(ZServerChannel zserver_channel, string strResultSetName)
        {
            return "#" + zserver_channel.GetHashCode() + "_" + strResultSetName;
        }

        // 一个 ZServerChannel 中能用到的最多全局结果集数。超过这个数目就会自动开始删除。删除可能会造成前端访问某些结果集时报错
        static readonly int MAX_RESULTSET_COUNT = 100;

        // 记忆全局结果集名
        // return:
        //      false   正常
        //      true    结果集数量超过 MAX_RESULTSET_COUNT，返回前已经开始释放所有结果集
        static bool MemoryResultSetName(ZServerChannel zserver_channel,
            string resultset_name)
        {
            if (!(zserver_channel.SetProperty().GetKeyObject("r_n") is List<string> names))
            {
                names = new List<string>();
                zserver_channel.SetProperty().SetKeyObject("r_n", names);
            }

            if (names.IndexOf(resultset_name) == -1)
                names.Add(resultset_name);

            // 如果结果集名数量太多，就要开始删除
            if (names.Count > MAX_RESULTSET_COUNT)
            {
                FreeGlobalResultSets(zserver_channel, names);

                return true;
            }

            return false;
        }

        // 取出先前记忆的全局结果集名列表
        // parameters:
        //      bRemove 是否在返回前自动删除 key_object 集合中的值
        static List<string> GetResultSetNameList(ZServerChannel zserver_channel,
            bool bRemove = false)
        {
            lock (zserver_channel)
            {
                if (!(zserver_channel.SetProperty().GetKeyObject("r_n") is List<string> names))
                    return new List<string>();
                else
                {
                    if (bRemove)
                        zserver_channel.SetProperty().SetKeyObject("r_n", null);
                }
                return names;
            }
        }

        static void FreeGlobalResultSets(ZServerChannel zserver_channel,
            List<string> names)
        {
            string strInstanceName = zserver_channel.SetProperty().GetKeyValue("i_n");
            if (strInstanceName == null)
            {
                string strError = "通道中 实例名 '" + strInstanceName + "' 尚未初始化";
                ZManager.Log?.Error(strError);
            }
            Instance instance = FindInstance(strInstanceName);
            if (instance == null)
            {
                string strError = "实例名 '" + strInstanceName + "' 不存在(或实例没有启用 Z39.50 服务)";
                // 写入错误日志
                ZManager.Log?.Error(strError);
                return;
            }

            // TODO: 交给 Instance 释放
            instance.AddGlobalResultSets(names);

#if NO
            LibraryChannel library_channel = instance.MessageConnection.GetChannel(null);
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
                        return;
                    }
                }
            }
            finally
            {
                instance.MessageConnection.ReturnChannel(library_channel);
            }
#endif
        }

        static Instance FindInstance(string strInstanceName)
        {
            Instance instance = null;
            if (string.IsNullOrEmpty(strInstanceName))
                instance = _instances[0];
            else
                instance = _instances.Find(
                    (o) =>
                    {
                        return (o.Name == strInstanceName);
                    });
            if (instance == null)
                return instance;
            if (instance.zhost == null)
                return null;    // 实例虽然存在，但没有启用 Z39.50 服务
            return instance;
        }

#if NO
        private static void Zserver_GetZConfig(object sender, GetZConfigEventArgs e)
        {
            ZServerChannel zserver_channel = (ZServerChannel)sender;

#if NO
            List<string> parts = StringUtil.ParseTwoPart(e.Info.m_strID, "@");
            string strInstanceName = parts[1];
#endif
            string strInstanceName = zserver_channel.SetProperty().GetKeyValue("i_n");
            if (strInstanceName == null)
            {
                string strError = "通道中 实例名 '" + strInstanceName + "' 尚未初始化";
                ZManager.Log.Error(strError);
                e.ZConfig = null;
                e.Result.ErrorInfo = strError;
                return;
            }

            Instance instance = FindInstance(strInstanceName);
            if (instance == null)
            {
                e.ZConfig = null;
                e.Result.ErrorInfo = "以用户名 '" + e.Info.m_strID + "' 中包含的实例名 '" + strInstanceName + "' 没有找到任何实例";
                return;
            }

            // 让 channel 携带 Instance Name
            // zserver_channel.SetProperty().SetKeyValue("i_n", strInstanceName);

            e.ZConfig = new ZConfig
            {
                AnonymousUserName = instance.zhost.AnonymousUserName,
                AnonymousPassword = instance.zhost.AnonymousPassword,
            };
        }
#endif
        private static void Zserver_InitializeLogin(object sender, InitializeLoginEventArgs e)
        {
            ZServerChannel zserver_channel = (ZServerChannel)sender;

            string strInstanceName = zserver_channel.SetProperty().GetKeyValue("i_n");
            if (strInstanceName == null)
            {
                string strErrorText = "通道中 实例名 '" + strInstanceName + "' 尚未初始化";
                ZManager.Log?.Error(strErrorText);
                e.Result = new Result
                {
                    Value = -1,
                    ErrorCode = "2",
                    ErrorInfo = strErrorText
                };
                return;
            }
            Instance instance = FindInstance(strInstanceName);
            if (instance == null)
            {
                e.Result = new Result
                {
                    Value = -1,
                    ErrorCode = "",
                    ErrorInfo = "实例名 '" + strInstanceName + "' 不存在(或实例没有启用 Z39.50 服务)"
                };
                return;
            }


            string strUserName = zserver_channel.SetProperty().GetKeyValue("i_u");
            string strPassword = zserver_channel.SetProperty().GetKeyValue("i_p");

            LoginInfo login_info = new LoginInfo { UserName = strUserName, Password = strPassword };

            LibraryChannel library_channel = instance.MessageConnection.GetChannel(login_info);
            try
            {
                string strParameters = "";
                if (login_info.UserType == "patron")
                    strParameters += ",type=reader";
                strParameters += ",client=dp2capo|" + "0.01";

                // result.Value:
                //      -1  登录出错
                //      0   登录未成功
                //      1   登录成功
                long lRet = library_channel.Login(strUserName,
                    strPassword,
                    strParameters,
                    out string strError);
                e.Result.Value = (int)lRet;
                if (lRet != 1)
                    e.Result.ErrorCode = "101";
                e.Result.ErrorInfo = strError;
            }
            finally
            {
                instance.MessageConnection.ReturnChannel(library_channel);
            }
        }

        private static void Zserver_SetChannelProperty(object sender, SetChannelPropertyEventArgs e)
        {
            ZServerChannel zserver_channel = (ZServerChannel)sender;

            List<string> parts = StringUtil.ParseTwoPart(e.Info.m_strID, "@");
            string strUserName = parts[0];
            string strInstanceName = parts[1];

            string strPassword = e.Info.m_strPassword;

            // 匿名登录情形
            if (string.IsNullOrEmpty(strUserName))
            {
                Instance instance = FindInstance(strInstanceName);
                if (instance == null)
                {
                    e.Result = new Result
                    {
                        Value = -1,
                        ErrorCode = "",
                        ErrorInfo = "以用户名 '" + e.Info.m_strID + "' 中包含的实例名 '" + strInstanceName + "' 没有找到任何实例(或实例没有启用 Z39.50 服务)"
                    };
                    return;
                }

                // 如果定义了允许匿名登录
                if (String.IsNullOrEmpty(instance.zhost.AnonymousUserName) == false)
                {
                    strUserName = instance.zhost.AnonymousUserName;
                    strPassword = instance.zhost.AnonymousPassword;
                }
                else
                {
                    e.Result = new Result
                    {
                        Value = -1,
                        ErrorCode = "101",
                        ErrorInfo = "不允许匿名登录"
                    };
                    return;
                }
            }

            // 让 channel 从此携带 Instance Name
            zserver_channel.SetProperty().SetKeyValue("i_n", strInstanceName);
            zserver_channel.SetProperty().SetKeyValue("i_u", strUserName);
            zserver_channel.SetProperty().SetKeyValue("i_p", strPassword);

            Debug.Assert(e.Result != null, "");
        }
    }
}
