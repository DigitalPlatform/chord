using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Xml;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters;

using Microsoft.AspNet.SignalR.Client;
using log4net;

using DigitalPlatform;
using DigitalPlatform.Common;
using DigitalPlatform.Net;
using DigitalPlatform.MessageClient;
using DigitalPlatform.IO;
using DigitalPlatform.Z3950.Server;
using DigitalPlatform.Interfaces;
using DigitalPlatform.SIP.Server;
using DigitalPlatform.Text;
using System.Text;

namespace dp2Capo
{
    public static class ServerInfo
    {
        // 指示全局 Service 是否处在运行状态
        public static bool GlobalServiceRunning { get; set; }

        public static ZServer ZServer { get; set; }

        public static SipServer SipServer { get; set; }


        // 实例集合
        public static SafeList<Instance> _instances = new SafeList<Instance>();

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

        static int _sip_port = -1;   // -1 表示不启用 SIP Server
        public static int SipServerPort
        {
            get
            {
                return _sip_port;
            }
            set
            {
                _sip_port = value;
            }
        }

        // 配置文件 XmlDocument
        public static XmlDocument ConfigDom = null; // new XmlDocument();

        internal static CancellationTokenSource _cancel = new CancellationTokenSource();

        public static void LoadGlobalCfg(string strDataDir, bool bAutoCreate = true)
        {
            string strCfgFileName = Path.Combine(strDataDir, "config.xml");
            try
            {
                ConfigDom = new XmlDocument();
                ConfigDom.Load(strCfgFileName);

                {
                    // 元素 <zServer>
                    // 属性 port
                    if (ConfigDom.DocumentElement.SelectSingleNode("zServer") is XmlElement node && node.HasAttribute("port"))
                    {
                        string v = node.GetAttribute("port");
                        if (string.IsNullOrEmpty(v))
                            _z3950_port = -1;
                        else
                        {
                            if (Int32.TryParse(v, out int port) == false)
                                throw new Exception("zServer/@port 属性值必须是整数");
                            _z3950_port = port;
                        }
                    }
                    else
                    {
                        _z3950_port = -1;  // -1 表示不启用 Z39.50 Server
                    }
                }

                {
                    // 元素 <zServer>
                    // 属性 port
                    if (ConfigDom.DocumentElement.SelectSingleNode("sipServer") is XmlElement node && node.HasAttribute("port"))
                    {
                        string v = node.GetAttribute("port");
                        if (string.IsNullOrEmpty(v))
                            _sip_port = -1;
                        else
                        {
                            if (Int32.TryParse(v, out int port) == false)
                                throw new Exception("sipServer/@port 属性值必须是整数");
                            _sip_port = port;
                        }
                    }
                    else
                    {
                        _sip_port = -1;  // -1 表示不启用 Z39.50 Server
                    }
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

        // 暂时没有地方调用本函数。注意应该是有个 changed 表示为 true 时候才覆盖原有配置文件。避免和外面主动修改配置文件的时候互相覆盖
        public static void SaveGlobalCfg(string strDataDir)
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

        // static Mutex _mutex = null;

        // 首次从数据目录装载全部实例定义，并连接服务器
        public static void Initial(string strDataDir)
        {
            DataDir = strDataDir;
            LogDir = Path.Combine(strDataDir, "log");   // 日志目录
            PathUtil.CreateDirIfNeed(LogDir);


            // http://stackoverflow.com/questions/184084/how-to-force-c-sharp-net-app-to-run-only-one-instance-in-windows
            // mutex name need contains windows account name. or us programes file path, hashed
            // _mutex = new Mutex(true, "dp2Capo V1", out bool createdNew);

            // 要避免初始化发生两次
            // https://stackoverflow.com/questions/579688/why-is-the-date-appended-twice-on-filenames-when-using-log4net
            var repository = log4net.LogManager.CreateRepository("main");
            // if (createdNew)
            log4net.GlobalContext.Properties["LogFileName"] = Path.Combine(LogDir, "log_");
            log4net.Config.XmlConfigurator.Configure(repository);

            LibraryManager.Log = LogManager.GetLogger("main", "zlib");
            _log = LogManager.GetLogger("main", "capo");

            string strVersion = Assembly.GetAssembly(typeof(ServerInfo)).GetName().Version.ToString();

            // 验证一下日志文件是否允许写入。这样就可以设置一个标志，决定后面的日志信息写入文件还是 Windows 日志
            // DetectWriteErrorLog("*** dp2Capo 开始启动 (dp2Capo 版本: " + strVersion + ")");
            WriteLog("info", "*** dp2Capo 开始启动 (dp2Capo 版本: " + strVersion + ")");

#if NO
            try
            {
                ServerInfo.LoadGlobalCfg(DataDir);
            }
            catch (Exception ex)
            {
                WriteLog("fatal", "dp2Capo 装载全局配置文件阶段出错: " + ex.Message);
                throw ex;
            }
#endif

            _exited = false;

            DirectoryInfo root = new DirectoryInfo(strDataDir);
            var dis = root.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                if (di.Name.ToLower() == "log")
                    continue;
                //string strXmlFileName = Path.Combine(di.FullName, "capo.xml");
                //if (File.Exists(strXmlFileName) == false)
                //    continue;
                Instance instance = new Instance();
                instance.DataDir = di.FullName;
                instance.Start();
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

#if NO
        // 根据实例名，找到实例数据目录
        public static string FindInstanceDataDir(string strInstanceName)
        {
            DirectoryInfo root = new DirectoryInfo(DataDir);
            var dis = root.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                if (di.Name.ToLower() == "log")
                    continue;
                string strXmlFileName = Path.Combine(di.FullName, "capo.xml");
                string strCurrentInstanceName = Instance.GetInstanceName(strXmlFileName);
                //if (File.Exists(strXmlFileName) == false)
                //    continue;
                if (strInstanceName == strCurrentInstanceName)
                    return di.FullName;
            }

            return null;
        }
#endif
        // 从 _instances 集合中查找一个实例
        public static Instance FindInstance(string strInstanceName)
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
            //if (instance == null)
            //    return instance;
            return instance;
        }

        // 尝试从数据目录装载一个尚未存在于 _instances 集合中的实例
        // exception:
        //      可能会抛出异常。XML 文件装载失败引起
        public static Instance LoadInstance(string strInstanceName)
        {
            var exist = _instances.Find((o) =>
            {
                if (o.Name == strInstanceName) return true;
                return false;
            });
            if (exist != null)
                return exist;

            DirectoryInfo root = new DirectoryInfo(DataDir);
            var dis = root.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                if (di.Name.ToLower() == "log")
                    continue;
                string strXmlFileName = Path.Combine(di.FullName, "capo.xml");
                if (File.Exists(strXmlFileName) == false)
                    continue;

                string strCurrentInstanceName = Instance.GetInstanceName(strXmlFileName);
                if (strCurrentInstanceName == strInstanceName)
                {
                    Instance instance = new Instance();
                    instance.Name = strCurrentInstanceName;
                    instance.DataDir = di.FullName;
                    _instances.Add(instance);
                    return instance;
                }
            }

            return null;
        }

        // 启动一个实例
        // parameters:
        //      strInstanceName 实例名。如果为 ".global" 表示全局服务
        public static ServiceControlResult StartInstance(string strInstanceName)
        {
            if (strInstanceName == ".global")
            {
                StartGlobalService(true);
                return new ServiceControlResult();
            }

            Instance instance = FindInstance(strInstanceName);
            if (instance == null)
            {
                // 尝试从数据目录装载一个尚未存在于 _instances 集合中的实例
                // exception:
                //      可能会抛出异常。XML 文件装载失败引起
                try
                {
                    instance = LoadInstance(strInstanceName);
                }
                catch (Exception ex)
                {
                    return new ServiceControlResult
                    {
                        Value = -1,
                        ErrorCode = ex.GetType().ToString(),
                        ErrorInfo = "实例 '" + strInstanceName + "' LoadInstance() 时出现异常: " + ExceptionUtil.GetAutoText(ex)
                    };
                }
            }

            if (instance == null)
                return new ServiceControlResult
                {
                    Value = -1,
                    ErrorCode = "NotFound",
                    ErrorInfo = "实例 '" + strInstanceName + "' 没有找到"
                };
            try
            {
                instance.Start();
                return new ServiceControlResult();
            }
            catch (Exception ex)
            {
                return new ServiceControlResult
                {
                    Value = -1,
                    ErrorCode = ex.GetType().ToString(),
                    ErrorInfo = "启动实例 '" + strInstanceName + "' 时出现异常: " + ex.Message
                };
            }
        }

        public static void StartGlobalService(bool bLoadGlobalCfg)
        {
            if (bLoadGlobalCfg)
            {
                try
                {
                    ServerInfo.LoadGlobalCfg(DataDir);
                }
                catch (Exception ex)
                {
                    WriteLog("fatal", "dp2Capo 启动全局服务阶段，装载全局配置文件出错: " + ex.Message);
                    // throw ex;
                    return;
                }
            }

            if (ServerInfo.Z3950ServerPort != -1)
            {
                ServerInfo.ZServer = new ZServer(ServerInfo.Z3950ServerPort);
                Z3950Processor.AddEvents(ServerInfo.ZServer, true);
                ServerInfo.ZServer.Listen(1000);
            }

            if (ServerInfo.SipServerPort != -1)
            {
                ServerInfo.SipServer = new SipServer(ServerInfo.SipServerPort);
                SipProcessor.AddEvents(ServerInfo.SipServer, true);
                ServerInfo.SipServer.Listen(1000);
            }

            ServerInfo.GlobalServiceRunning = true;
        }

        public static void StopGlobalService()
        {
            if (ServerInfo.ZServer != null)
            {
                ServerInfo.ZServer.Close();
                ServerInfo.ZServer.Dispose();
                Z3950Processor.AddEvents(ServerInfo.ZServer, false);
                ServerInfo.ZServer = null;
            }

            if (ServerInfo.SipServer != null)
            {
                ServerInfo.SipServer.Close();
                ServerInfo.SipServer.Dispose();
                SipProcessor.AddEvents(ServerInfo.SipServer, false);
                ServerInfo.SipServer = null;
            }

            ServerInfo.GlobalServiceRunning = false;
        }

#if NO
        // TODO: 这里有个问题：什么能代表全局服务？
        public static bool IsGlobalServiceRunning()
        {
            if (ServerInfo.ZServer != null)
                return true;
            return false;
        }
#endif

        // 停止一个实例
        //      strInstanceName 实例名。如果为 ".global" 表示全局服务
        public static ServiceControlResult StopInstance(string strInstanceName)
        {
            if (strInstanceName == ".global")
            {
                StopGlobalService();
                return new ServiceControlResult();
            }

            Instance instance = FindInstance(strInstanceName);
            if (instance == null)
                return new ServiceControlResult
                {
                    Value = -1,
                    ErrorCode = "NotFound",
                    ErrorInfo = "实例 '" + strInstanceName + "' 没有找到"
                };
            try
            {
                instance.Close();
                return new ServiceControlResult();
            }
            catch (Exception ex)
            {
                return new ServiceControlResult
                {
                    Value = -1,
                    ErrorCode = ex.GetType().ToString(),
                    ErrorInfo = "停止实例 '" + strInstanceName + "' 时出现异常: " + ex.Message
                };
            }
        }

        #region Windows Service 控制命令设施

        static IpcServerChannel m_serverChannel = null;

        public static void StartRemotingServer()
        {
            // http://www.cnblogs.com/gisser/archive/2011/12/31/2308989.html
            // https://stackoverflow.com/questions/7126733/ipcserverchannel-properties-problem
            // https://stackoverflow.com/questions/2400320/dealing-with-security-on-ipc-remoting-channel
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            Hashtable ht = new Hashtable();
            ht["portName"] = "dp2capo_ServiceControlChannel";
            ht["name"] = "ipc";
            ht["authorizedGroup"] = "Administrators"; // "Everyone";
            m_serverChannel = new IpcServerChannel(ht, provider);

            //Register the server channel.
            ChannelServices.RegisterChannel(m_serverChannel, false);

            RemotingConfiguration.ApplicationName = "dp2library_ServiceControlServer";

            //Register this service type.
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServiceControlServer),
                "dp2library_ServiceControlServer",
                WellKnownObjectMode.Singleton);
        }

        public static void EndRemotingServer()
        {
            if (m_serverChannel != null)
            {
                ChannelServices.UnregisterChannel(m_serverChannel);
                m_serverChannel = null;
            }
        }

        #endregion


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

        static bool NeedCheckMessageServer(Instance instance)
        {
            // 2018/8/28
            if (instance.dp2mserver == null)
                return false;

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

        static void EchoMessageServer(Instance instance, bool writeLog)
        {
            // 2018/8/28
            if (instance.dp2mserver == null)
                return;

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
        static void CheckMessageServer(Instance instance, List<Task> tasks)
        {
            if (instance.dp2mserver == null || string.IsNullOrEmpty(instance.dp2mserver.Url))
                return;

            if (instance.MessageConnection.ConnectState == ConnectionState.Disconnected)    // 注：如果在 Connecting 和 Reconnecting 状态则不尝试 ConnectAsync
            {
                // instance.BeginConnnect();
                tasks.Add(instance.BeginConnectTask());

                Uri uri = new Uri(instance.dp2mserver.Url);
                try
                {
                    if (NetUtil.Ping(uri.DnsSafeHost, out string strInformation) == true)
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
                EchoMessageServer(instance, true);
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
                // 2018/8/28
                if (instance.dp2mserver == null)
                    continue;
                if (now - instance.LastCheckTime > delta)
                    return true;
            }

            return false;
        }

#if NO
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
#endif

        static int _statisCount = 0;

        static DateTime _lastCleanTime = DateTime.Now;

        // 执行一些后台管理任务
        public static void BackgroundWork()
        {
            try
            {
                List<Task> tasks = new List<Task>();


#if NO
                Instance first_instance = null;
                if (_instances.Count > 0)
                    first_instance = _instances[0];
#endif

                foreach (Instance instance in _instances)
                {
                    // 向 dp2mserver 发送心跳消息
                    // instance.SendHeartBeat();

                    bool bNeedCheckMessageServer = NeedCheckMessageServer(instance);
                    bool bOutputBegin = false;

                    if (bNeedCheckMessageServer)
                    {
                        instance.WriteErrorLog("<<< BackgroundWork 开始一轮处理\r\n状态:\r\n" + instance.GetDebugState());
                        bOutputBegin = true;
                    }

                    string strError = "";
                    // 利用 dp2library API 获取一些配置信息
                    if (instance.dp2library != null
                        && string.IsNullOrEmpty(instance.dp2library.LibraryUID) == true)
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
                            if (bNeedCheckMessageServer)
                                CheckMessageServer(instance, tasks);
                            else
                                EchoMessageServer(instance, false);
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

                        if (bNeedCheckMessageServer)
                            CheckMessageServer(instance, tasks);
                        else
                            EchoMessageServer(instance, false);

                        // 每隔二十分钟清理一次闲置的 dp2library 通道
                        if (DateTime.Now - _lastCleanTime >= TimeSpan.FromMinutes(20))
                        {
                            try
                            {
                                WriteLog("info", $"实例 {instance.Name} 开始释放空闲通道");
                                int count = instance.MessageConnection.CleanLibraryChannel();
                                WriteLog("info", $"实例 {instance.Name} 释放空闲通道 {count} 个。正在占用的通道为 {instance.MessageConnection._libraryChannelPool.GetUsingCount()} 个");
                            }
                            catch (Exception ex)
                            {
                                string error = $"实例 {instance.Name} 释放空闲通道时出现异常: {ExceptionUtil.GetExceptionText(ex)}";
                                instance.WriteErrorLog(error);
                                WriteLog("error", error);
                            }
                            _lastCleanTime = DateTime.Now;
                        }

                        // 重试初始化 ZHost 慢速参数
                        // instance.InitialZHostSlowConfig();

                        // 清除废弃的全局结果集
                        Task.Run(() => instance.FreeGlobalResultSets());
                    }

                    if (bOutputBegin == true)
                        instance.WriteErrorLog(">>> BackgroundWork 结束一轮处理\r\n");
                }

                {
                    // 写入统计信息
                    _statisCount++;
                    if ((_statisCount % 10) == 1)
                    {
                        LogCpuUsage("dp2capo");
                        WriteLog("info", "ZServer 统计信息: " + ZServer?.GetStatisInfo() + "LibraryChannel 占用: " + GetLibraryChannelCountText());
                    }

                    // 清理闲置超期的 Channels
                    ZServer?._tcpChannels?.CleanIdleChannels(TimeSpan.FromMinutes(2));
                    SipServer?._tcpChannels?.CleanIdleChannels(TimeSpan.MaxValue);   // TimeSpan.FromMinutes(2)

                    ZServer?.TryClearBlackList();
                    SipServer?.TryClearBlackList();

                    // 把紧凑日志写入日志文件
                    ZServer?.TryFlushCompactLog();

                    // 顺便清理一下 hangup 状态缓存
                    SipProcessor.ClearHangupStatusTable();
                }

                // 阻塞，直到全部任务完成。避免 BeginConnect() 函数被重叠调用
                if (tasks.Count > 0)
                {
                    // test 这一句可以用来制造死锁测试场景
                    // Thread.Sleep(60 * 60 * 1000);
                    WriteLog("info", "-- BackgroundWork - 等待 " + tasks.Count + " 个 Connect 任务完成");

                    Task.WaitAll(tasks.ToArray(), ServerInfo._cancel.Token);

                    WriteLog("info", "-- BackgroundWork - " + tasks.Count + " 个 Connect 任务已经完成");
                }

            }
            catch (Exception ex)
            {
                WriteLog("error", "BackgroundWork() 出现异常: " + ExceptionUtil.GetDebugText(ex));
            }
        }

        // 获得描述每个通道正在使用的 LibraryChannel 数量的文字
        static string GetLibraryChannelCountText()
        {
            try
            {
                StringBuilder text = new StringBuilder();
                foreach (Instance instance in _instances)
                {
                    text.AppendFormat("{0}={1}; ",
                        instance.Name,
                        instance.MessageConnection._libraryChannelPool.GetUsingCount());
                }
                return text.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // 检测高内存耗用
        public static bool DetectHighMemory(string appName, long threshold)
        {
            try
            {
                PerformanceCounter memory = new PerformanceCounter("Process", "Private Bytes", appName);

                for (int i = 0; i < 2; i++)
                {
                    if (i != 0)
                        System.Threading.Thread.Sleep(1000);

                    float m = memory.NextValue();
                    if (i != 0)
                    {
                        long mem_bytes = Convert.ToInt64(m);
                        if (mem_bytes >= threshold)
                        {
                            WriteLog("info", "*** 检测到高内存耗用 " + StringUtil.GetLengthText(mem_bytes));
                            return true;
                        }
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                WriteLog("error", "DetectHighMemory() 内出现异常: " + ExceptionUtil.GetExceptionText(ex));
                return false;
            }
        }

        private static void LogCpuUsage(string appName)
        {
            try
            {
                PerformanceCounter total_cpu = new PerformanceCounter("Process", "% Processor Time", "_Total");
                PerformanceCounter process_cpu = new PerformanceCounter("Process", "% Processor Time", appName);
                PerformanceCounter memory = new PerformanceCounter("Process", "Private Bytes", appName);


                for (int i = 0; i < 2; i++)
                {
                    if (i != 0)
                        System.Threading.Thread.Sleep(1000);

                    float t = total_cpu.NextValue();
                    float p = process_cpu.NextValue();
                    float m = memory.NextValue();
                    if (i != 0)
                    {
                        // Console.WriteLine(String.Format("_Total = {0}  App = {1} {2}%\n", t, p, p / t * 100));
                        WriteLog("info", "CPU " + Convert.ToInt64(p / t * 100).ToString() + "%, Memory " + StringUtil.GetLengthText(Convert.ToInt64(m)));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog("error", "LogCpuUsage() 内出现异常: " + ExceptionUtil.GetExceptionText(ex));
            }
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

    }
}
