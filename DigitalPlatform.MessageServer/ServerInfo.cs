using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Threading;
using System.Net;

using MongoDB.Driver;

using DigitalPlatform.IO;

namespace DigitalPlatform.MessageServer
{
    public static class ServerInfo
    {
        // 检索请求管理集合
        public static SearchTable SearchTable = new SearchTable(true);

        // 通道集合
        public static ConnectionTable ConnectionTable = new ConnectionTable();

        // 配置文件 XmlDocument
        public static XmlDocument ConfigDom = new XmlDocument();

        // 用户数据库
        public static UserDatabase UserDatabase = new UserDatabase();

        // 消息数据库
        public static MessageDatabase MessageDatabase = new MessageDatabase();

        // 组数据库
        public static GroupDatabase GroupDatabase = new GroupDatabase();

        static MongoClient _mongoClient = null;

        static BackThread BackThread = new BackThread();

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

        static string AutoTriggerUrl
        {
            get;
            set;
        }

        public static void CreateCfgFile(string strCfgFileName,
            string strMongoDbConnStr = "",
            string strMongoDbInstancePrefix = "")
        {
            // 默认值
            if (string.IsNullOrEmpty(strMongoDbConnStr))
                strMongoDbConnStr = "mongodb://localhost";
            if (string.IsNullOrEmpty(strMongoDbInstancePrefix))
                strMongoDbInstancePrefix = "mserver";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            XmlElement mongoDB = dom.CreateElement("mongoDB");
            dom.DocumentElement.AppendChild(mongoDB);

            mongoDB.SetAttribute("connectionString", strMongoDbConnStr);
            mongoDB.SetAttribute("instancePrefix", strMongoDbInstancePrefix);

            dom.Save(strCfgFileName);
        }

        public static void Initial(InitialParam param)
        {
            AutoTriggerUrl = param.AutoTriggerUrl;

            if (Directory.Exists(param.DataDir) == false)
            {
                // throw new Exception("数据目录 '" + strDataDir + "' 尚未创建");
                WriteWindowsLog("数据目录 '" + param.DataDir + "' 尚未创建");
            }

            DataDir = param.DataDir;
            LogDir = Path.Combine(param.DataDir, "log");   // 日志目录
            PathUtil.CreateDirIfNeed(LogDir);

            // 验证一下日志文件是否允许写入。这样就可以设置一个标志，决定后面的日志信息写入文件还是 Windows 日志
            DetectWriteErrorLog("*** dp2MServer 开始启动");

            string strCfgFileName = Path.Combine(param.DataDir, "config.xml");
            try
            {
                ConfigDom.Load(strCfgFileName);

                // 元素 <mongoDB>
                // 属性 connectionString / instancePrefix
                XmlElement node = ConfigDom.DocumentElement.SelectSingleNode("mongoDB") as XmlElement;
                if (node != null)
                {
                    strMongoDbConnStr = node.GetAttribute("connectionString");
                    strMongoDbInstancePrefix = node.GetAttribute("instancePrefix");
                }
                else
                    throw new Exception("尚未配置 mongoDB 元素");
            }
            catch (Exception ex)
            {
                WriteErrorLog("装载配置文件 '" + strCfgFileName + "' 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
            }

            BackThread.BeginThread();
        }

        static string strMongoDbConnStr = "";
        static string strMongoDbInstancePrefix = "";

        static CancellationTokenSource _cancel = new CancellationTokenSource();

        public static void InitialMongoDb(bool bFirst)
        {
            string strError = "";
            if (_mongoClient == null)
            {
                try
                {
                    Console.WriteLine("正在初始化 MongoDB 数据库 ...");

                    _mongoClient = new MongoClient(strMongoDbConnStr);

                    UserDatabase.Open(_mongoClient, strMongoDbInstancePrefix, "user", _cancel.Token);
                    MessageDatabase.Open(_mongoClient, strMongoDbInstancePrefix, "message", _cancel.Token);
                    GroupDatabase.Open(_mongoClient, strMongoDbInstancePrefix, "group", _cancel.Token);

                    if (bFirst == false)
                    {
                        strError = "重试初始化 MongoDB 数据库成功";
                        WriteErrorLog(strError);
                        Console.WriteLine(strError);
                    }
                    else
                        Console.WriteLine("初始化 MongoDB 数据库成功。");
                }
                catch (Exception ex)
                {
                    _mongoClient = null;

                    if (bFirst)
                    {
                        strError = "首次初始化 MongoDB 数据库时出现异常(稍后会自动重试初始化): " + ExceptionUtil.GetExceptionText(ex);
                        WriteErrorLog(strError);
                        Console.WriteLine(strError);
                    }
                }
            }
        }

        public static void CleanExpiredMessage()
        {
            try
            {
                // 删除 1 天以前失效的消息
                MessageDatabase.DeleteExpired(DateTime.Now - new TimeSpan(1, 0, 0, 0)).Wait();
                // 删除一年前发布的消息
                MessageDatabase.DeleteByPublishTime(DateTime.Now - new TimeSpan(365,0,0,0)).Wait();
            }
            catch(Exception ex)
            {
                WriteErrorLog("清理失效消息时出现异常: " + ExceptionUtil.GetDebugText(ex));
            }
        }

        public static void TriggerUrl()
        {
            if (string.IsNullOrEmpty(AutoTriggerUrl))
                return;

            try
            {
                WebRequest request = WebRequest.Create(
                    AutoTriggerUrl
                    );
                request.Timeout = 1000;
                using (var stream = request.GetResponse().GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    reader.ReadToEnd();
                }
            }
            catch
            {

            }
        }

        // 准备退出
        public static void Exit()
        {
            _cancel.Cancel();

            BackThread.StopThread(false);
            BackThread.Dispose();

            SearchTable.Exit();
            SearchTable.Dispose();

            WriteErrorLog("*** dp2MServer 降落成功");
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

            WriteWindowsLog("日志文件检测正常。从此开始关于 dp2MServer 的日志会写入目录 " + LogDir + " 中当天的日志文件",
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
            Log.Source = "dp2MessageService";
            Log.WriteEntry(strText, type);
        }

        #endregion

    }

    // 负责重试创建一些对象
    class BackThread : ThreadBase
    {
        public BackThread()
        {
            this.PerTime = 60 * 1000;
        }

        bool _first = true;

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            if (this.Stopped == true)
                return;

            ServerInfo.InitialMongoDb(_first);
            _first = false;

            ServerInfo.TriggerUrl();

            ServerInfo.CleanExpiredMessage();
        }
    }

    public class InitialParam
    {
        public string DataDir { get; set; }
        public string AutoTriggerUrl { get; set; }
    }
}
