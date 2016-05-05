using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Principal;

using Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Microsoft.AspNet.SignalR;

using dp2MServer.Properties;

using DigitalPlatform.MessageServer;
using DigitalPlatform.ServiceProcess;
using System.Configuration;
using System.IO;
using DigitalPlatform.Xml;

namespace dp2MServer
{
    class Program : MyServiceBase
    {
        static IDisposable SignalR { get; set; }
        // const string ServerURI = "http://localhost:8083";

        public Program()
        {
            //this.ServiceName = "dp2 Message Service";
            //ServiceShortName = "dp2mserver";
            ServiceShortName = "dp2MessageService";
        }

        static void Main(string[] args)
        {
            ServiceShortName = "dp2MessageService";
#if NO
            // Task.Run(() => StartServer());
            StartServer();

            Console.WriteLine("Server running on {0}", ServerURI);

            Console.WriteLine("press 'x' to exit");
            // Keep going until somebody hits 'x'
            while (true)
            {
                ConsoleKeyInfo ki = Console.ReadKey(true);
                if (ki.Key == ConsoleKey.X)
                {
                    Console.WriteLine("exiting ...");
                    ServerInfo.Exit();
                    break;
                }
            }

            if (SignalR != null)
            {
                SignalR.Dispose();
                SignalR = null;
            }
#endif
            // 修改配置
            if (args.Length == 1 && args[0].Equals("setting"))
            {
                InitialConfig();
                // config.AppSettings.Settings.Add(newKey, newValue);

                Console.WriteLine("(直接回车表示不修改当前值)");
                Console.WriteLine("请输入服务器 URI: (当前值为 " + ServerURI + ")");
                string strValue = Console.ReadLine();
                if (string.IsNullOrEmpty(strValue) == false)
                    ServerURI = strValue;

                Console.WriteLine("请输入服务器路径: (当前值为 " + ServerPath + ")");
                strValue = Console.ReadLine();
                if (string.IsNullOrEmpty(strValue) == false)
                    ServerPath = strValue;

                Console.WriteLine("请输入数据目录: (当前值为 " + DataDir + ")");
                strValue = Console.ReadLine();
                if (string.IsNullOrEmpty(strValue) == false)
                    DataDir = strValue;

                SaveConfig();
#if NO
                Console.WriteLine("(直接回车表示不修改当前值)");
                Console.WriteLine("请输入服务器 URI: (当前值为 " + Settings.Default.ServerURI + ")");
                string strValue = Console.ReadLine();
                if (string.IsNullOrEmpty(strValue) == false)
                    Settings.Default.ServerURI = strValue;

                Console.WriteLine("请输入服务器路径: (当前值为 " + Settings.Default.ServerPath + ")");
                strValue = Console.ReadLine();
                if (string.IsNullOrEmpty(strValue) == false)
                    Settings.Default.ServerPath = strValue;

                Console.WriteLine("请输入数据目录: (当前值为 " + Settings.Default.DataDir + ")");
                strValue = Console.ReadLine();
                if (string.IsNullOrEmpty(strValue) == false)
                    Settings.Default.DataDir = strValue;

                Settings.Default.Save();
#endif

                Console.WriteLine();
                Console.WriteLine("注：修改将在服务重启以后生效");
                Console.WriteLine("(按回车键返回)");
                Console.ReadLine();
                return;
            }

            // 创建超级用户账户
            if (args.Length == 1 && args[0].Equals("createuser"))
            {
                if (Initial() == false)
                    return;

                string strUserName = "supervisor";
                string strPassword = "";

                Console.WriteLine("(直接回车表示不修改当前值)");
                Console.WriteLine("请输入超级用户名: (当前值为 " + strUserName + ")");
                string strValue = Console.ReadLine();
                if (string.IsNullOrEmpty(strValue) == false)
                    strUserName = strValue;

                Console.WriteLine("请输入超级用户密码: ");
                strValue = Console.ReadLine();
                strPassword = strValue;

                CreateSupervisor(strUserName, strPassword);

                Console.WriteLine();
                Console.WriteLine("注：修改已经立即生效");
                Console.WriteLine("(按回车键返回)");
                Console.ReadLine();
                ServerInfo.Exit();  // Initial() 执行后，需要 Exit() 才能退出
                return;
            }

            if (args.Length == 1
                    && (args[0].Equals("install") || args[0].Equals("uninstall"))
                    )
            {
                bool bInstall = true;

                if (args[0].Equals("uninstall"))
                    bInstall = false;

                // 注册为 Windows Service
                // string strExePath = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
                string strExePath = Assembly.GetExecutingAssembly().Location;
                Console.WriteLine((bInstall ? "注册" : "注销") + " Windows Service ...");

                string strError = "";
                int nRet = ServiceUtil.InstallService(strExePath,
        bInstall,
        out strError);
                if (nRet == -1)
                    Console.WriteLine("error: " + strError);

                if (bInstall == true)
                {
                    // 创建事件日志目录
                    if (!EventLog.SourceExists("dp2mserver"))
                    {
                        EventLog.CreateEventSource("dp2mserver", "DigitalPlatform");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("(按回车键返回)");
                Console.ReadLine();
                return;
            }

            if (args.Length == 1 && args[0].Equals("console"))
            {
                if (Initial() == false)
                    return;
                new Program().ConsoleRun();
            }
            else
            {
                // 这是从命令行启动的情况
                if (Environment.UserInteractive == true)
                {
                    Console.WriteLine("dp2MServer 用法:");
                    Console.WriteLine("注册 Windows Service: dp2mserver install");
                    Console.WriteLine("注销 Windows Service: dp2mserver uninstall");
                    Console.WriteLine("以控制台方式运行: dp2mserver console");
                    Console.WriteLine("修改配置参数: dp2mserver setting");
                    Console.WriteLine("创建超级用户: dp2mserver createuser");

                    Console.WriteLine("(按回车键返回)");
                    Console.ReadLine();
                    return;
                }

                if (Initial() == false)
                    return;

                // 这是被当作 service 启动的情况
                ServiceBase.Run(new Program());
            }
        }

        static void CreateSupervisor(string strUserName, string strPassword)
        {
            UserItem item = new UserItem();
            item.userName = strUserName;

            ServerInfo.UserDatabase.Delete(item).Wait();

            item.password = strPassword;
            item.rights = "supervisor";
            ServerInfo.UserDatabase.Add(item).Wait();
        }

        // return:
        //      true    初始化成功
        //      false   初始化失败
        static bool Initial()
        {
            try
            {
                InitialConfig();
                Program.WriteWindowsLog("dump config: " + Config.Dump(), EventLogEntryType.Information);

                ServerInfo.Initial(DataDir);

                return true;
            }
            catch (Exception ex)
            {
                WriteWindowsLog(ex.Message, EventLogEntryType.Error);
                Console.WriteLine("初始化失败: " + ex.Message);
                return false;
            }
        }

#if NO
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
            Log.Source = "dp2mserver";
            Log.WriteEntry(strText, type);
        }

        #region 控制台方式运行

        static CtrlEventHandler _handler;

        private bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    {
                        Debug.WriteLine("close ...");
                        Console.WriteLine("closing...");
                        ServerInfo.Exit();
                    }
                    return true;
                default:
                    break;
            }

            return false;
        }

        // bool m_bConsoleRun = false;

        public delegate bool CtrlEventHandler(CtrlType sig);

        public enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(CtrlEventHandler handler, bool add);

        private void ConsoleRun()
        {
            // this.m_bConsoleRun = true;

            // Some biolerplate to react to close window event
            _handler += new CtrlEventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            Console.WriteLine("{0}::starting...", GetType().FullName);

            OnStart(null);

            Console.WriteLine("{0}::ready (ENTER to exit)", GetType().FullName);

            Console.ReadLine();

            OnStop();
            Console.WriteLine("{0}::stopped", GetType().FullName);
        }

        #endregion
#endif

        protected override void OnStart(string[] args)
        {
            StartServer();
        }

        protected override void OnStop()
        {
            this.Close();

            // 因为这里 ServerInfo 并没有调用 Exit() 所以后来的 OnStart() 不用重新初始化 ServerInfo
        }

        public override void Close()
        {
            base.Close();

            ServerInfo.Exit();

            if (SignalR != null)
            {
                SignalR.Dispose();
                SignalR = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }

            base.Dispose(disposing);
        }

        static string DataDir
        {
            get
            {
#if NO
                string strServerURI = Settings.Default.ServerURI;
                if (string.IsNullOrEmpty(strServerURI) == true)
                    strServerURI = "http://*:8083";

                return strServerURI;
#endif
                return Config.Get("default", "data_dir", "c:\\mserver_data");
            }
            set
            {
                Config.Set("default", "data_dir", value);
            }
        }

        static string ServerURI
        {
            get
            {
#if NO
                string strServerURI = Settings.Default.ServerURI;
                if (string.IsNullOrEmpty(strServerURI) == true)
                    strServerURI = "http://*:8083";

                return strServerURI;
#endif
                return Config.Get("default", "server_uri", "http://*:8083");
            }
            set
            {
                Config.Set("default", "server_uri", value);
            }
        }

        internal static string ServerPath
        {
            get
            {
#if NO
                string strServerPath = Settings.Default.ServerPath;
                if (string.IsNullOrEmpty(strServerPath) == true)
                    strServerPath = "/dp2MServer";
                return strServerPath;
#endif
                return Config.Get("default", "server_path", "/dp2MServer");
            }
            set
            {
                Config.Get("default", "server_path", value);
            }
        }

        static void StartServer()
        {
            try
            {
                SignalR = WebApp.Start(ServerURI);
            }
            catch (TargetInvocationException)
            {
                WriteToConsole("Server failed to start. A server is already running on " + ServerURI);
                return;
            }
            WriteToConsole("Server started at " + ServerURI + ServerPath);
        }

        /// <summary>
        /// This method adds a line to the RichTextBoxConsole control, using Invoke if used
        /// from a SignalR hub thread rather than the UI thread.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteToConsole(String message)
        {
            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// Used by OWIN's startup process. 
    /// </summary>
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            // app.MapSignalR();

            {
                var hubConfiguration = new HubConfiguration();
                hubConfiguration.EnableDetailedErrors = true;
                app.MapSignalR(Program.ServerPath, hubConfiguration);
            }

            /*
https://github.com/SignalR/SignalR/issues/1205
maximum message size #1205

https://github.com/SignalR/SignalR/issues/2631
InvalidOperationException : "Connection started reconnecting before invocation result was received" #2631 
@fyip We added a MaxIncomingWebSocketMessageSize property to IConfigurationManager which allows you to increase or disable this limit.
             * */
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = 128 * 1024;  // 默认为 64K
        }
    }


}
