using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;

using DigitalPlatform.ServiceProcess;
using DigitalPlatform.IO;
using DigitalPlatform;

namespace dp2Router
{
    class Program : MyServiceBase
    {
        public Program()
        {
            ServiceShortName = "dp2RouterService";
        }

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            ServerInfo.WriteErrorLog("全局异常: e.IsTerminating(" + e.IsTerminating + ")\r\n" + ExceptionUtil.GetExceptionText(e.ExceptionObject as Exception));
        }

        static void Main(string[] args)
        {
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            ServiceShortName = "dp2RouterService";

            // 修改配置
            if (args.Length == 1 && args[0].Equals("setting"))
            {
                InitialConfig();

                Console.WriteLine("(直接回车表示不修改当前值)");

                Console.WriteLine("请输入数据目录: (当前值为 " + DataDir + ")");
                string strValue = Console.ReadLine();
                if (string.IsNullOrEmpty(strValue) == false)
                    DataDir = strValue;

                SaveConfig();
                InitialConfig();

                // 设置需要保存到数据目录 config.xml 中的其他参数
                ServerInfo.Initial(DataDir, true);

                Console.WriteLine("请输入服务器端口号: (当前值为 " + ServerInfo.ServerPort + ")");
                strValue = Console.ReadLine();
                if (string.IsNullOrEmpty(strValue) == false)
                    ServerInfo.ServerPort = strValue;

                Console.WriteLine("请输入 dp2MServer URL: (当前值为 " + ServerInfo.Url + ")");
                strValue = Console.ReadLine();
                if (string.IsNullOrEmpty(strValue) == false)
                    ServerInfo.Url = strValue;

                Console.WriteLine("请输入 dp2MServer 用户名: (当前值为 " + ServerInfo.UserName + ")");
                strValue = Console.ReadLine();
                if (string.IsNullOrEmpty(strValue) == false)
                    ServerInfo.UserName = strValue;

                Console.WriteLine("请输入 dp2MServer 密码:");
                Console.BackgroundColor = Console.ForegroundColor;
                strValue = Console.ReadLine();
                Console.ResetColor();
                if (string.IsNullOrEmpty(strValue) == false)
                    ServerInfo.Password = strValue;

                ServerInfo.SaveCfg(DataDir);

                ServerInfo.Exit();

                Console.WriteLine();
                Console.WriteLine("注：修改将在服务重启以后生效");
                Console.WriteLine("(按回车键返回)");
                Console.ReadLine();
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
                    Console.WriteLine("dp2Router 用法:");
                    Console.WriteLine("注册 Windows Service: dp2router install");
                    Console.WriteLine("注销 Windows Service: dp2router uninstall");
                    Console.WriteLine("以控制台方式运行: dp2router console");
                    Console.WriteLine("修改配置参数: dp2router setting");

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

        // return:
        //      true    初始化成功
        //      false   初始化失败
        static bool Initial()
        {
            try
            {
                InitialConfig();
                Program.WriteWindowsLog("dump config: " + Config.Dump(), EventLogEntryType.Information);

                //InitialParam param = new InitialParam();
                //param.DataDir = DataDir;
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
            StopServer();

            // Thread.Sleep(2000);
#if NO
            if (SignalR != null)
            {
                SignalR.Dispose();
                SignalR = null;
            }
#endif
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
                return Config.Get("default", "data_dir", "c:\\router_data");
            }
            set
            {
                Config.Set("default", "data_dir", value);
            }
        }

#if NO
        static string ServerPort
        {
            get
            {
                return Config.Get("default", "server_port", "8888");
            }
            set
            {
                Config.Set("default", "server_port", value);
            }
        }
#endif
        static HttpServer _httpServer = null;

        static void StartServer()
        {
            int nPort = Convert.ToInt32(ServerInfo.ServerPort);
            _httpServer = new HttpServer(nPort);

            //Thread thread = new Thread(new ThreadStart(_httpServer.Listen));
            //thread.Start();

            _httpServer.Listen();
#if NO
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
#endif
        }

        static void StopServer()
        {
            _httpServer.Close();
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
}
