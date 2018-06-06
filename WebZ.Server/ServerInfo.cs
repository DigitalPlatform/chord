//2018-06-06  测试
using DigitalPlatform;
using DigitalPlatform.IO;
using MongoDB.Driver;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using WebZ.Server.database;

namespace WebZ.Server
{
    public class ServerInfo
    {
        public static string DataDir { get; set; }
        public static string LogDir { get; set; }

        static MongoClient _mongoClient = null;
        // 站点配置数据库
        public static ZServerDatabase ZServerDb = new ZServerDatabase();
        static CancellationTokenSource _cancel = new CancellationTokenSource();
      

        // 初始化
        public static void Initial(string dataDir)
        {
            // 如果数据目录不存，则创建
            PathUtil.CreateDirIfNeed(dataDir);

            // 日志目录
            DataDir = dataDir;
            LogDir = Path.Combine(dataDir, "log");   
            PathUtil.CreateDirIfNeed(LogDir);

            string strVersion = Assembly.GetAssembly(typeof(ServerInfo)).GetName().Version.ToString();
            WriteErrorLog("*** WebZ 开始启动 (WebZ 版本: " + strVersion + ")");

            string strCfgFileName = Path.Combine(dataDir, "config.xml");
            // 如果配置文件不存在，自动创建数据目录中的 config.xml 配置文件
            if (File.Exists(strCfgFileName) == false)
            {
                PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strCfgFileName));
                ServerInfo.CreateCfgFile(strCfgFileName);
                WriteErrorLog("首次创建配置文件 " + strCfgFileName);
            }

            string strMongoDbConnStr = "";
            string strMongoDbInstancePrefix = "";
            try
            {
                XmlDocument cfgDom = new XmlDocument();
                cfgDom.Load(strCfgFileName);

                XmlNode root = cfgDom.DocumentElement;

                // 元素 <mongoDB>
                // 属性 connectionString / instancePrefix
                XmlElement node =root.SelectSingleNode("mongoDB") as XmlElement;
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

            // 初始化数据库
            _mongoClient = new MongoClient(strMongoDbConnStr);
            ZServerDb.Open(_mongoClient, strMongoDbInstancePrefix, "zserver", _cancel.Token);
        }


        // 创建配置文件
        public static void CreateCfgFile(string strCfgFileName,
            string strMongoDbConnStr = "",
            string strMongoDbInstancePrefix = "")
        {
            // 默认值
            if (string.IsNullOrEmpty(strMongoDbConnStr))
                strMongoDbConnStr = "mongodb://localhost";
            if (string.IsNullOrEmpty(strMongoDbInstancePrefix))
                strMongoDbInstancePrefix = "WebZ";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            XmlElement mongoDB = dom.CreateElement("mongoDB");
            dom.DocumentElement.AppendChild(mongoDB);

            mongoDB.SetAttribute("connectionString", strMongoDbConnStr);
            mongoDB.SetAttribute("instancePrefix", strMongoDbInstancePrefix);

            dom.Save(strCfgFileName);
        }

        #region 写日志

        private static readonly Object _syncRoot = new Object();

        static void WriteErrorLog(string strText)
        {
            // 注: 当 LogDir 为空的时候会抛出异常
            lock (_syncRoot)
            {
                DateTime now = DateTime.Now;
                // 每天一个日志文件
                string strFilename = Path.Combine(LogDir, "log_" + DateTimeUtil.DateTimeToString8(now) + ".txt");
                string strTime = now.ToString();
                FileUtil.WriteText(strFilename,
                    strTime + " " + strText + "\r\n");
            }
        }

        #endregion
    }
}
