using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Web;
using System.Net;
using System.Reflection;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Message;
using DigitalPlatform.HTTP;

namespace dp2Router
{
    public static class ServerInfo
    {
        static MessageConnectionCollection _channels = new MessageConnectionCollection();

        // 配置文件 XmlDocument
        public static XmlDocument ConfigDom = null; // new XmlDocument();

        static CancellationTokenSource _cancel = new CancellationTokenSource();

        public static string DataDir
        {
            get;
            set;
        }

        static string LogDir
        {
            get;
            set;
        }

        static string _url = "";
        public static string Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
            }
        }

        static string _userName = "";
        public static string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
            }
        }

        static string _password = "";
        public static string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
            }
        }

        const string DEFAULT_PORT = "8888";

        static string _port = DEFAULT_PORT;
        public static string ServerPort
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
            }
        }

        public static void SaveCfg(string strDataDir)
        {
            if (ConfigDom == null)
                throw new Exception("尚未执行 Initial()");

            XmlElement node = ConfigDom.DocumentElement.SelectSingleNode("messageServer") as XmlElement;
            if (node == null)
            {
                node = ConfigDom.CreateElement("messageServer");
                ConfigDom.DocumentElement.AppendChild(node);
            }

            node.SetAttribute("url", _url);
            node.SetAttribute("userName", _userName);
            node.SetAttribute("password", Cryptography.Encrypt(_password, EncryptKey));

            node = ConfigDom.DocumentElement.SelectSingleNode("httpServer") as XmlElement;
            if (node == null)
            {
                node = ConfigDom.CreateElement("httpServer");
                ConfigDom.DocumentElement.AppendChild(node);
            }

            node.SetAttribute("port", _port);

            string strCfgFileName = Path.Combine(strDataDir, "config.xml");
            PathUtil.CreateDirIfNeed(strDataDir);
            ConfigDom.Save(strCfgFileName);
        }

        public static string EncryptKey = "_dp2routerpassword";

        // parameters:
        //      bAutoCreate 当 config.xml 不存在时自动初始化，不抛出异常
        public static void Initial(string strDataDir,
            bool bAutoCreate = false)
        {
            if (Directory.Exists(strDataDir) == false)
            {
                // WriteWindowsLog("数据目录 '" + param.DataDir + "' 尚未创建");
                if (bAutoCreate == true)
                    PathUtil.CreateDirIfNeed(strDataDir);
                else
                    throw new Exception("数据目录 '" + strDataDir + "' 尚未创建");
            }

            DataDir = strDataDir;
            LogDir = Path.Combine(strDataDir, "log");   // 日志目录
            PathUtil.CreateDirIfNeed(LogDir);

            string strVersion = Assembly.GetAssembly(typeof(ServerInfo)).GetName().Version.ToString();

            // 验证一下日志文件是否允许写入。这样就可以设置一个标志，决定后面的日志信息写入文件还是 Windows 日志
            DetectWriteErrorLog("*** dp2Router 开始启动 (dp2Router 版本: " + strVersion + ")");

            string strCfgFileName = Path.Combine(strDataDir, "config.xml");
            try
            {
                ConfigDom = new XmlDocument();
                ConfigDom.Load(strCfgFileName);

                // 元素 <messageServer>
                // 属性 url / userName / password
                XmlElement node = ConfigDom.DocumentElement.SelectSingleNode("messageServer") as XmlElement;
                if (node != null)
                {
                    _url = node.GetAttribute("url");
                    _userName = node.GetAttribute("userName");
                    _password = Cryptography.Decrypt(node.GetAttribute("password"), EncryptKey);
                }
                else
                    throw new Exception("尚未配置 messageServer 元素");

                // 元素 <httpServer>
                // 属性 port
                node = ConfigDom.DocumentElement.SelectSingleNode("httpServer") as XmlElement;
                if (node != null)
                {
                    _port = node.GetAttribute("port");
                }
                else
                {
                    _port = DEFAULT_PORT;
                    // throw new Exception("尚未配置 httpServer 元素");
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

            _channels.Login += _channels_Login;
            _channels.AddMessage += _channels_AddMessage;

            // BackThread.BeginThread();
        }

        static void _channels_Login(object sender, LoginEventArgs e)
        {
            MessageConnection connection = sender as MessageConnection;

            e.UserName = UserName;
            if (string.IsNullOrEmpty(e.UserName) == true)
                throw new Exception("尚未指定用户名，无法进行登录");

            e.Password = Password;
            e.Parameters = "";  // "propertyList=biblio_search,libraryUID=xxx";
        }

        static void _channels_AddMessage(object sender, AddMessageEventArgs e)
        {
        }

        // 准备退出
        public static void Exit()
        {
            _channels.Login -= _channels_Login;
            _channels.AddMessage -= _channels_AddMessage;
        }

        public static DigitalPlatform.HTTP.HttpResponse WebCall(DigitalPlatform.HTTP.HttpRequest request,
            string transferEncoding)
        {
            // 从 request.Url 中解析出 remoteUserName
            string remoteUserName = request.Url;
            if (string.IsNullOrEmpty(remoteUserName) == false
                && remoteUserName[0] == '/')
                remoteUserName = remoteUserName.Substring(1);

            {
                // 只取第一级作为对方用户名
                List<string> parts = StringUtil.ParseTwoPart(remoteUserName, "/");
                remoteUserName = parts[0];
            }

            WebData data = MessageUtility.BuildWebData(request, transferEncoding);

            string id = Guid.NewGuid().ToString();
            WebCallRequest param = new WebCallRequest(id, 
                transferEncoding,
                data, 
                true,
                true);
            CancellationToken cancel_token = new CancellationToken();

            try
            {
                // Console.WriteLine("Begin WebCall");

                MessageConnection connection = _channels.GetConnectionTaskAsync(
                    Url,
                    request.Url).Result;
                WebCallResult result = connection.WebCallTaskAsync(
                    remoteUserName,
                    param,
                    new TimeSpan(0, 1, 10), // 10 秒
                    cancel_token).Result;

                // Console.WriteLine("End WebCall result=" + result.Dump());

                if (result.Value == -1)
                {
                    // 构造一个 500 错误响应包
                    return new DigitalPlatform.HTTP.HttpResponse("500",
                        "Router Error: " + WebUtility.UrlEncode(result.ErrorInfo),
                        result.ErrorInfo);
                }

                return MessageUtility.BuildHttpResponse(result.WebData, transferEncoding);
            }
            catch (AggregateException ex)
            {
                string code = "500";
                string error = ExceptionUtil.GetExceptionText(ex);

                Console.WriteLine("Exception: " + error);

                MessageException ex1 = ExceptionUtil.FindInnerException(ex, typeof(MessageException)) as MessageException;
                if (ex1 != null && ex1.ErrorCode.ToLower() == "unauthorized")
                {
                    error = "dp2Router 针对 dp2MServer 的账户 '"+ex1.UserName+"' 登录失败";
                    code = "401";
                }

                WriteErrorLog("1: " + error);

                // 构造一个错误代码响应包
                return new DigitalPlatform.HTTP.HttpResponse(
                    code,
                    "Router Error: " + WebUtility.UrlEncode(error),
                    error);
            }
            catch (Exception ex)
            {
                WriteErrorLog("2: " + ExceptionUtil.GetExceptionText(ex));

                // 构造一个 500 错误响应包
                return new DigitalPlatform.HTTP.HttpResponse(
                    "500",
                    "Router Error: " + WebUtility.UrlEncode(ex.Message),
                    ExceptionUtil.GetExceptionText(ex));
            }
        }

        #region 日志

        static bool _errorLogError = false;    // 写入实例的日志文件是否发生过错误

        static void _writeErrorLog(string strText)
        {
            lock (LogDir)
            {
                DateTime now = DateTime.Now;
                // 每天一个日志文件
                string strFilename = Path.Combine(LogDir, "log_" + DateTimeUtil.DateTimeToString8(now) + ".txt");
                string strTime = now.ToString();
                FileUtil.WriteText(strFilename,
                    strTime + " " + strText + "\r\n");
            }
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

            WriteWindowsLog("日志文件检测正常。从此开始关于 dp2Router 的日志会写入目录 " + LogDir + " 中当天的日志文件",
                EventLogEntryType.Information);
            return true;
        }

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
            Log.Source = "dp2RouterService";
            Log.WriteEntry(strText, type);
        }

        #endregion

    }

#if NO
    public class InitialParam
    {
        public string DataDir { get; set; }
    }
#endif
}
