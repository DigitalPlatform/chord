using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.ServiceProcess;

using dp2Capo.Properties;

using DigitalPlatform.ServiceProcess;

namespace dp2Capo
{
    class Program : MyServiceBase
    {
        public Program()
        {
            this.ServiceName = "dp2 Capo Service";
            ServiceShortName = "dp2capo";
        }

        static void Main(string[] args)
        {
            // 修改配置
            if (args.Length == 1 && args[0].Equals("setting"))
            {
                ChangeSettings();
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
                    Console.WriteLine("dp2capo 用法:");
                    Console.WriteLine("注册 Windows Service: dp2capo install");
                    Console.WriteLine("注销 Windows Service: dp2capo uninstall");
                    Console.WriteLine("以控制台方式运行: dp2capo console");
                    Console.WriteLine("修改配置参数: dp2capo setting");

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
                ServerInfo.Initial(Settings.Default.DataDir);
                return true;
            }
            catch (Exception ex)
            {
                WriteWindowsLog(ex.Message, EventLogEntryType.Error);
                Console.WriteLine("初始化失败: " + ex.Message);
                return false;
            }
        }

        static void SetOneParameter(string strPromptName, Settings obj, string strFieldName)
        {
            PropertyInfo info = obj.GetType().GetProperty(strFieldName);
            string value = (string)info.GetValue(obj, null);
            Console.WriteLine("请输入 "+strPromptName+": (当前值为 '" + value + "' )");
            string strNewValue = Console.ReadLine();
            if (string.IsNullOrEmpty(strNewValue) == false)
                info.SetValue(obj, strNewValue, null);
        }

        // 修改配置
        static void ChangeSettings()
        {
            Console.WriteLine("(直接回车表示不修改当前值)");

            SetOneParameter("数据目录", Settings.Default, "DataDir");

            Settings.Default.Save();

            Console.WriteLine();
            Console.WriteLine("注：修改将在服务重启以后生效");
            Console.WriteLine("(按回车键返回)");
            Console.ReadLine();
            return;
        }

        protected override void OnStart(string[] args)
        {
            StartServer();
        }

        protected override void OnStop()
        {
            this.Close();
        }

        public override void Close()
        {
            base.Close();

            ServerInfo.Exit();
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

        }
    }
}
