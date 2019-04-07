using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Reflection;
using System.IO;

using DigitalPlatform.Xml;
using DigitalPlatform.Core;

namespace DigitalPlatform.ServiceProcess
{
    public class MyServiceBase : ServiceBase
    {


        /// <summary>
        /// 服务的短名称。例如 "dp2mserver"。用作 Windows Event Log 名称
        /// </summary>
        public static string ServiceShortName
        {
            get;
            set;
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
            Debug.Assert(string.IsNullOrEmpty(ServiceShortName) == false, "");
            Log.Source = ServiceShortName;
            Log.WriteEntry(strText, type);
        }

        static ConfigSetting _config = null;

        public static ConfigSetting Config
        {
            get
            {
                return _config;
            }
        }

        public static void InitialConfig()
        {
            string strExePath = Assembly.GetExecutingAssembly().Location;

            string filename = Path.Combine(Path.GetDirectoryName(strExePath), "settings.xml");
            Console.WriteLine(filename);

            _config = ConfigSetting.Open(filename, true);
        }

        public static void SaveConfig()
        {
            // Save the configuration file.
            if (_config != null)
            {
                _config.Save();
                _config = null;
            }
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
                        // ServerInfo.Exit();
                        this.Close();
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

        public static bool ConsoleMode = false;

        public void ConsoleRun()
        {
            // this.m_bConsoleRun = true;
            ConsoleMode = true;

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

        public virtual void Close()
        {

        }

    }
}
