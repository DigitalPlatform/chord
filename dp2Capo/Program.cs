using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.ServiceProcess;

// using dp2Capo.Properties;

using DigitalPlatform;
using DigitalPlatform.ServiceProcess;
using DigitalPlatform.Z3950.Server;

namespace dp2Capo
{
    class Program : MyServiceBase
    {
        static Program()
        {
            // this.ServiceName = "dp2 Capo Service";
            // ServiceShortName = "dp2Capo";
            ServiceShortName = "dp2CapoService";
        }

        static void PrintLogo()
        {
            // http://www.network-science.de/ascii/
            Console.WriteLine(@"
      dP          d8888b.  a88888b.                            
      88              `88 d8'   `88                            
.d888b88 88d888b. .aaadP' 88        .d8888b. 88d888b. .d8888b. 
88'  `88 88'  `88 88'     88        88'  `88 88'  `88 88'  `88 
88.  .88 88.  .88 88.     Y8.   .88 88.  .88 88.  .88 88.  .88 
`88888P8 88Y888P' Y88888P  Y88888P' `88888P8 88Y888P' `88888P' 
         88                                  88                
         dP  -dp2 V2 和 Chord 之间的桥接器-  dP                 
");
        }

#if NO
        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Program.WriteWindowsLog("全局异常: " + ExceptionUtil.GetExceptionText(e.ExceptionObject as Exception), EventLogEntryType.Error);
        }
#endif

        static void Main(string[] args)
        {
            // System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            ServiceShortName = "dp2CapoService";

            PrintLogo();

            // 修改配置
            if (args.Length >= 1 && args[0].Equals("setting"))
            {
                ChangeSettings(args.Length > 1 ? args[1] : "");
                return;
            }

            // 注册或注销 Windows Service
            if (args.Length == 1
        && (args[0].Equals("install") || args[0].Equals("uninstall"))
        )
            {
                bool bInstall = true;

                if (args[0].Equals("uninstall"))
                    bInstall = false;

                // 注册为 Windows Service
                string strExePath = Assembly.GetExecutingAssembly().Location;
                Console.WriteLine((bInstall ? "注册" : "注销") + " Windows Service ...");

                try
                {
                    Console.WriteLine("停止服务 ...");
                    ServiceUtil.StopService("dp2CapoService", TimeSpan.FromMinutes(2));
                    Console.WriteLine("服务已停止。");
                }
                catch//(Exception ex)
                {
                    // Console.WriteLine("停止服务时发生异常: " + ExceptionUtil.GetExceptionText(ex));
                }
#if NO

                if (bInstall == true)
                {
                    // 创建事件日志目录
                    // 注: 创建事件日志目录应该在 InstallService 以前。因为 InstallService 过程中涉及到启动服务，可能要写入日志
                    if (!EventLog.SourceExists(ServiceShortName))   // "dp2Capo"
                    {
                        EventLog.CreateEventSource(ServiceShortName, "DigitalPlatform");
                    }
                }
#endif

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

            // 以控制台方式运行
            if (args.Length == 1 && args[0].Equals("console"))
            {
                if (Initial() == false)
                    return;
                new Program().ConsoleRun();
                return;
            }
            else
            {
                // 这是从命令行启动的情况
                if (Environment.UserInteractive == true)
                {
                    Console.WriteLine("dp2capo 用法:");
                    Console.WriteLine("注册 Windows Service: dp2capo install");
                    Console.WriteLine("注销 Windows Service: dp2capo uninstall");
                    Console.WriteLine("以控制台方式运行: dp2capo console");
                    Console.WriteLine("修改配置参数: dp2capo setting");
                    Console.WriteLine("修改实例配置参数: dp2capo setting 1");

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

                ServerInfo.Initial(DataDir);
                return true;
            }
            catch (Exception ex)
            {
                WriteWindowsLog(ExceptionUtil.GetDebugText(ex), EventLogEntryType.Error);
                Console.WriteLine("初始化失败: " + ExceptionUtil.GetDebugText(ex));
                return false;
            }
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
                return Config.Get("default", "data_dir", "c:\\capo_data");
            }
            set
            {
                Config.Set("default", "data_dir", value);
            }
        }

#if NO
        static void SetOneParameter(string strPromptName, object obj, string strFieldName)
        {
            PropertyInfo info = obj.GetType().GetProperty(strFieldName);
            string value = (string)info.GetValue(obj, null);
            Console.WriteLine("请输入 "+strPromptName+": (当前值为 '" + value + "' )");
            string strNewValue = Console.ReadLine();
            if (string.IsNullOrEmpty(strNewValue) == false)
                info.SetValue(obj, strNewValue, null);
        }
#endif

        // 修改配置
        // parameters:
        //      strInstanceIndex    实例下标。从 1 开始计数。如果为空，表示仅仅设置 DataDir 参数。如果不为空，表示设置一个具体的实例的参数
        static void ChangeSettings(string strInstanceIndex)
        {
            Console.WriteLine("(直接回车表示不修改当前值)");

            InitialConfig();

            if (string.IsNullOrEmpty(strInstanceIndex) == true)
            {
                // SetOneParameter("数据目录", (object)Program, "DataDir");

                // Settings.Default.Save();
                Console.WriteLine("请输入数据目录: (当前值为 " + DataDir + ")");
                string strValue = Console.ReadLine();
                if (string.IsNullOrEmpty(strValue) == false)
                    DataDir = strValue;
            }
            else
            {
                int index = Int32.Parse(strInstanceIndex) - 1;
                ChangeInstanceSettings(index);
            }

            SaveConfig();

            Console.WriteLine();
            Console.WriteLine("注：修改将在服务重启以后生效");
            Console.WriteLine("(按回车键返回)");
            Console.ReadLine();
            return;
        }

        // parameters:
        //      index   实例子目录下标。从 0 开始计数
        static void ChangeInstanceSettings(int index)
        {
            ServerInfo.ChangeInstanceSettings(DataDir, index);
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
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }

            base.Dispose(disposing);
        }

        static void StartServer()
        {
            if (ServerInfo.ServerPort != 0)
            {
                ServerInfo.ZServer = new ZServer(ServerInfo.ServerPort);
                ServerInfo.AddEvents(ServerInfo.ZServer, true);
                ServerInfo.ZServer.Listen();
            }
        }

        static void StopServer()
        {
            if (ServerInfo.ZServer != null)
            {
                ServerInfo.ZServer.Close();
                ServerInfo.ZServer.Dispose();
                ServerInfo.AddEvents(ServerInfo.ZServer, false);
                ServerInfo.ZServer = null;
            }
        }
    }
}
