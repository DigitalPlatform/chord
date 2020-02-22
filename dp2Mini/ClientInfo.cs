using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;

using log4net;

using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Core;
using System.Drawing;
using System.Xml;
using DigitalPlatform.Xml;
using DigitalPlatform.Forms;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 存储各种程序信息的全局类
    /// </summary>
    public static class ClientInfo
    {
        /// <summary>
        /// 程序名
        /// </summary>
        public static string ProgramName { get; set; }

        /// <summary>
        /// 程序主窗口
        /// </summary>
        public static Form MainForm { get; set; }


        /// <summary>
        /// 前端的版本号
        /// </summary>
        public static string ClientVersion { get; set; }

        /// <summary>
        /// 程序类型，是否是开发模板？todo
        /// </summary>
        public static Type TypeOfProgram { get; set; }

        /// <summary>
        /// 数据目录
        /// </summary>
        public static string DataDir = "";

        /// <summary>
        /// 用户目录
        /// </summary>
        public static string UserDir = "";

        /// <summary>
        /// 错误日志目录
        /// </summary>
        public static string UserLogDir = "";

        /// <summary>
        /// 临时文件目录
        /// </summary>
        public static string UserTempDir = "";

        // 附加的一些文件名非法字符。比如 XP 下 Path.GetInvalidPathChars() 不知何故会遗漏 '*'
        static string spec_invalid_chars = "*?:";

        public static string GetValidPathString(string strText, string strReplaceChar = "_")
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            char[] invalid_chars = Path.GetInvalidPathChars();
            StringBuilder result = new StringBuilder();
            foreach (char c in strText)
            {
                if (c == ' ')
                    continue;
                if (IndexOf(invalid_chars, c) != -1
                    || spec_invalid_chars.IndexOf(c) != -1)
                    result.Append(strReplaceChar);
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        static int IndexOf(char[] chars, char c)
        {
            int i = 0;
            foreach (char c1 in chars)
            {
                if (c1 == c)
                    return i;
                i++;
            }

            return -1;
        }




        public static string Lang = "zh";

        public static string ProductName = "";

        // return:
        //      true    不检查序列号
        public delegate bool Delegate_skipSerialNumberCheck();

        // parameters:
        //      product_name    例如 "fingerprintcenter"
        public static void Initial(string product_name, Delegate_skipSerialNumberCheck skipCheck = null)
        {
            ProductName = product_name;
            ClientVersion = Assembly.GetAssembly(TypeOfProgram).GetName().Version.ToString();

            if (ApplicationDeployment.IsNetworkDeployed == true)
            {
                // MessageBox.Show(this, "network");
                DataDir = Application.LocalUserAppDataPath;
            }
            else
            {
                DataDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }

            UserDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    product_name);
            PathUtil.TryCreateDir(UserDir);

            UserTempDir = Path.Combine(UserDir, "temp");
            PathUtil.TryCreateDir(UserTempDir);

            UserLogDir = Path.Combine(UserDir, "log");
            PathUtil.TryCreateDir(UserLogDir);

            // 初始化setting配置文件
            InitialConfig();

            var repository = log4net.LogManager.CreateRepository("main");
            log4net.GlobalContext.Properties["LogFileName"] = Path.Combine(UserLogDir, "log_");
            log4net.Config.XmlConfigurator.Configure(repository);

            //LibraryChannelManager.Log = LogManager.GetLogger("main", "channellib");
            //_log = LogManager.GetLogger("main",
            //    product_name
            //    // "fingerprintcenter"
            //    );

            // 启动时在日志中记载当前 .exe 版本号
            // 此举也能尽早发现日志目录无法写入的问题，会抛出异常
            WriteInfoLog(Assembly.GetAssembly(typeof(ClientInfo)).FullName);

            /*
            {
                // 检查序列号
                // if (DateTime.Now >= start_day || this.MainForm.IsExistsSerialNumberStatusFile() == true)
                if (SerialNumberMode == "must"
                    && (skipCheck == null || skipCheck() == false))
                {
                    // 在用户目录中写入一个隐藏文件，表示序列号功能已经启用
                    // this.WriteSerialNumberStatusFile();

                    int nRet = VerifySerialCode($"{product_name}需要先设置序列号才能使用",
                        "",
                        "reinput",
                        out string strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(MainForm, $"{product_name}需要先设置序列号才能使用");
                        API.PostMessage(MainForm.Handle, API.WM_CLOSE, 0, 0);
                        return;
                    }
                }
            }
            */
        }

        public static void Finish()
        {
            SaveConfig();
        }

        #region Log

        static ILog _log = null;

        public static ILog Log
        {
            get
            {
                return _log;
            }
        }

        // 写入错误日志文件
        public static void WriteErrorLog(string strText)
        {
            WriteLog("error", strText);
        }

        public static void WriteInfoLog(string strText)
        {
            WriteLog("info", strText);
        }

        // 写入错误日志文件
        // parameters:
        //      level   info/error
        // Exception:
        //      可能会抛出异常
        public static void WriteLog(string level, string strText)
        {
            // Console.WriteLine(strText);

            if (_log == null) // 先前写入实例的日志文件发生过错误，所以改为写入 Windows 日志。会加上实例名前缀字符串
                WriteWindowsLog(strText, EventLogEntryType.Error);
            else
            {
                // 注意，这里不捕获异常
                if (level == "info")
                    _log.Info(strText);
                else
                    _log.Error(strText);
            }
        }

        // 写入 Windows 日志
        public static void WriteWindowsLog(string strText)
        {
            WriteWindowsLog(strText, EventLogEntryType.Error);
        }

        #endregion

        #region 未捕获的异常处理 

        // 准备接管未捕获的异常
        public static void PrepareCatchException()
        {
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        static bool _bExiting = false;   // 是否处在 正在退出 的状态

        static void CurrentDomain_UnhandledException(object sender,
    UnhandledExceptionEventArgs e)
        {
            if (_bExiting == true)
                return;

            Exception ex = (Exception)e.ExceptionObject;
            string strError = GetExceptionText(ex, "");

            // TODO: 把信息提供给数字平台的开发人员，以便纠错
            // TODO: 显示为红色窗口，表示警告的意思
            bool bSendReport = true;
            DialogResult result = MessageDlg.Show(MainForm,
    $"{ProgramName} 发生未知的异常:\r\n\r\n" + strError + "\r\n---\r\n\r\n点“关闭”即关闭程序",
    $"{ProgramName} 发生未知的异常",
    MessageBoxButtons.OK,
    MessageBoxDefaultButton.Button1,
    ref bSendReport,
    new string[] { "关闭" },
    "将信息发送给开发者");
            // 发送异常报告
            if (bSendReport)
                CrashReport(strError);
        }

        static string GetExceptionText(Exception ex, string strType)
        {
            // Exception ex = (Exception)e.Exception;
            string strError = "发生未捕获的" + strType + "异常: \r\n" + ExceptionUtil.GetDebugText(ex);
            Assembly myAssembly = Assembly.GetAssembly(TypeOfProgram);
            strError += $"\r\n{ProgramName} 版本: " + myAssembly.FullName;
            strError += "\r\n操作系统：" + Environment.OSVersion.ToString();
            strError += "\r\n本机 MAC 地址: " + StringUtil.MakePathList(SerialCodeForm.GetMacAddress());

            // TODO: 给出操作系统的一般信息

            // MainForm.WriteErrorLog(strError);
            return strError;
        }

        static void Application_ThreadException(object sender,
    ThreadExceptionEventArgs e)
        {
            if (_bExiting == true)
                return;

            Exception ex = (Exception)e.Exception;
            string strError = GetExceptionText(ex, "界面线程");

            bool bSendReport = true;
            DialogResult result = MessageDlg.Show(MainForm,
    $"{ProgramName} 发生未知的异常:\r\n\r\n" + strError + "\r\n---\r\n\r\n是否关闭程序?",
    $"{ProgramName} 发生未知的异常",
    MessageBoxButtons.YesNo,
    MessageBoxDefaultButton.Button2,
    ref bSendReport,
    new string[] { "关闭", "继续" },
    "将信息发送给开发者");
            {
                if (bSendReport)
                    CrashReport(strError);
            }
            if (result == DialogResult.Yes)
            {
                _bExiting = true;
                Application.Exit();
            }
        }

        static void CrashReport(string strText)
        {
            /*
            MessageBar _messageBar = null;

            _messageBar = new MessageBar
            {
                TopMost = false,
                Text = $"{ProgramName} 出现异常",
                MessageText = "正在向 dp2003.com 发送异常报告 ...",
                StartPosition = FormStartPosition.CenterScreen
            };
            _messageBar.Show(MainForm);
            _messageBar.Update();

            int nRet = 0;
            string strError = "";
            try
            {
                string strSender = "";
                //if (MainForm != null)
                //    strSender = MainForm.GetCurrentUserName() + "@" + MainForm.ServerUID;
                // 崩溃报告

                nRet = LibraryChannel.CrashReport(
                    strSender,
                    $"{ProgramName}",
                    strText,
                    out strError);
            }
            catch (Exception ex)
            {
                strError = "CrashReport() 过程出现异常: " + ExceptionUtil.GetDebugText(ex);
                nRet = -1;
            }
            finally
            {
                _messageBar.Close();
                _messageBar = null;
            }

            if (nRet == -1)
            {
                strError = "向 dp2003.com 发送异常报告时出错，未能发送成功。详细情况: " + strError;
                MessageBox.Show(MainForm, strError);
                // 写入错误日志
                WriteErrorLog(strError);
            }
            */
        }


        // 写入 Windows 系统日志
        public static void WriteWindowsLog(string strText,
            EventLogEntryType type)
        {
            try
            {
                EventLog Log = new EventLog("Application");
                Log.Source = $"{ProgramName}";
                Log.WriteEntry(strText, type);
            }
            catch
            {

            }
        }

        #endregion

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
            if (string.IsNullOrEmpty(UserDir))
                throw new ArgumentException("UserDir 尚未初始化");

            string filename = Path.Combine(UserDir, "settings.xml");
            _config = ConfigSetting.Open(filename, true);
        }

        // 可反复调用
        public static void SaveConfig()
        {
            // Save the configuration file.
            if (_config != null && _config.Changed == true)
                _config.Save();
        }




        #region Form 实用函数

        public delegate void delegate_action(object o);

        public static void ProcessControl(Control control,
            delegate_action action)
        {
            action(control);
            ProcessChildren(control, action);
        }

        static void ProcessChildren(Control parent,
            delegate_action action)
        {
            // 修改所有下级控件的字体，如果字体名不一样的话
            foreach (Control sub in parent.Controls)
            {
                action(sub);

                if (sub is ToolStrip)
                {
                    ProcessToolStrip((ToolStrip)sub, action);
                }

                if (sub is SplitContainer)
                {
                    ProcessSplitContainer(sub as SplitContainer, action);
                }

                // 递归
                ProcessChildren(sub, action);
            }
        }

        static void ProcessToolStrip(ToolStrip tool,
delegate_action action)
        {
            List<ToolStripItem> items = new List<ToolStripItem>();
            foreach (ToolStripItem item in tool.Items)
            {
                items.Add(item);
            }

            foreach (ToolStripItem item in items)
            {
                action(item);

                if (item is ToolStripMenuItem)
                {
                    ProcessDropDownItemsFont(item as ToolStripMenuItem, action);
                }
            }
        }

        static void ProcessDropDownItemsFont(ToolStripMenuItem menu,
            delegate_action action)
        {
            // 修改所有事项的字体，如果字体名不一样的话
            foreach (ToolStripItem item in menu.DropDownItems)
            {

                action(item);

                if (item is ToolStripMenuItem)
                {
                    ProcessDropDownItemsFont(item as ToolStripMenuItem, action);
                }
            }
        }

        static void ProcessSplitContainer(SplitContainer container,
            delegate_action action)
        {
            action(container.Panel1);

            foreach (Control control in container.Panel1.Controls)
            {
                ProcessChildren(control, action);
            }

            action(container.Panel2);

            foreach (Control control in container.Panel2.Controls)
            {
                ProcessChildren(control, action);
            }
        }

        #endregion



        public static bool IsMinimizeMode()
        {
            try
            {
                // https://stackoverflow.com/questions/558344/clickonce-appref-ms-argument
                var args = AppDomain.CurrentDomain?.SetupInformation?.ActivationArguments?.ActivationData[0];
                // List<string> args = StringUtil.GetCommandLineArgs();
                return args.IndexOf("minimize") != -1;
            }
            catch
            {
                return false;
            }
        }

        /*
         * <config amerce_interface="<无>" im_server_url="http://dp2003.com:8083/dp2MServer" green_package_server_url="" pinyin_server_url="http://dp2003.com/dp2library" gcat_server_url="http://dp2003.com/dp2library" circulation_server_url="net.pipe://localhost/dp2library/XE"/>
         * <default_account tempCode="" phoneNumber="" occur_per_start="true" location="" isreader="false" savepassword_long="true" savepassword_short="true" username="supervisor" password="Z7RAQEBWFmBcKM8mFvOjwg=="/>
         * */
        public static int GetDp2circulationUserName(
            out string url,
            out string userName,
            out string password,
            out bool savePassword,
            out string strError)
        {
            strError = "";
            url = "";
            userName = "";
            password = "";
            savePassword = false;

            string strXmlFilename = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "dp2circulation_v2\\dp2circulation.xml");
            if (File.Exists(strXmlFilename) == false)
                return 0;

            XmlDocument dom = new XmlDocument();
            dom.PreserveWhitespace = true;
            try
            {
                dom.Load(strXmlFilename);
            }
            catch(Exception ex)
            {
                strError = $"打开 XML 文件失败: {ex.Message}";
                return -1;
            }

            {
                XmlElement default_account = dom.DocumentElement.SelectSingleNode("default_account") as XmlElement;
                if (default_account != null)
                {
                    userName = default_account.GetAttribute("username");
                    savePassword = DomUtil.GetBooleanParam(default_account, "savepassword_long", false);
                    if (savePassword == true)
                    {
                        string password_text = default_account.GetAttribute("password");
                        password = DecryptDp2circulationPasssword(password_text);
                    }
                }
            }

            {
                XmlElement config = dom.DocumentElement.SelectSingleNode("config") as XmlElement;
                if (config != null)
                {
                    url = config.GetAttribute("circulation_server_url");
                }
            }

            return 1;
        }

        internal static string DecryptDp2circulationPasssword(string strEncryptedText)
        {
            if (String.IsNullOrEmpty(strEncryptedText) == false)
            {
                try
                {
                    string strPassword = Cryptography.Decrypt(
        strEncryptedText,
        "dp2circulation_client_password_key");
                    return strPassword;
                }
                catch
                {
                    return "errorpassword";
                }
            }

            return "";
        }
    }
}
