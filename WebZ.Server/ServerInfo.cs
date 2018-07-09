﻿//2018-06-06  测试
using DigitalPlatform;
using DigitalPlatform.Interfaces;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using WebZ.Server.database;

namespace WebZ.Server
{
    public class ServerInfo
    {
        #region 单一实例

        static ServerInfo _instance;
        private ServerInfo()
        {
        }
        private static object _lock = new object();
        static public ServerInfo Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (_lock)  //线程安全的
                    {
                        _instance = new ServerInfo();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region 成员变量
        public string DataDir { get; set; }
        public  string LogDir { get; set; }
         
        // mongodb
        MongoClient _mongoClient = null;
        CancellationTokenSource _cancel = new CancellationTokenSource();

        // 站点配置数据库
        public ZServerDatabase ZServerDb = new ZServerDatabase();

        #endregion

        #region 初始化

        // 初始化
        public void Initial(string dataDir)
        {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

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
                this.CreateCfgFile(strCfgFileName);
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
                {
                    throw new Exception("尚未配置 mongoDB 元素");
                }

                // 初始化短信接口类
                string strError = "";
                int nRet = this.InitialExternalMessageInterfaces(cfgDom, out strError);
                if (nRet == -1)
                    throw new Exception("初始化短信接口配置信息出错：" + strError);
            }
            catch (Exception ex)
            {
                throw new Exception("装载配置文件 '" + strCfgFileName + "' 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
            }

            // 初始化数据库
            _mongoClient = new MongoClient(strMongoDbConnStr);
            ZServerDb.Open(_mongoClient, strMongoDbInstancePrefix, "zserver", _cancel.Token);
        }

        // 创建配置文件
        private  void CreateCfgFile(string strCfgFileName,
            string strMongoDbConnStr = "",
            string strMongoDbInstancePrefix = "")
        {
            // 默认值
            if (string.IsNullOrEmpty(strMongoDbConnStr))
                strMongoDbConnStr = "mongodb://localhost";
            if (string.IsNullOrEmpty(strMongoDbInstancePrefix))
                strMongoDbInstancePrefix = "webz";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            XmlNode root = dom.DocumentElement;

            XmlElement mongoDB = dom.CreateElement("mongoDB");
            root.AppendChild(mongoDB);

            mongoDB.SetAttribute("connectionString", strMongoDbConnStr);
            mongoDB.SetAttribute("instancePrefix", strMongoDbInstancePrefix);


            /*
   <externalMessageInterface>
    <interface type="sms" assemblyName="DongshifangMessageInterface" />
  </externalMessageInterface>            
             */
            XmlElement externalMessageInterface = dom.CreateElement("externalMessageInterface");
            root.AppendChild(externalMessageInterface);

            XmlElement interfaceNode = dom.CreateElement("interface");
            externalMessageInterface.AppendChild(interfaceNode);


            interfaceNode.SetAttribute("type", "sms");
            interfaceNode.SetAttribute("assemblyName", "DongshifangMessageInterface");


            dom.Save(strCfgFileName);
        }

        #endregion

        #region 短信接口

        public  List<MessageInterface> m_externalMessageInterfaces = null;

        // 初始化扩展的消息接口
        /*
    <externalMessageInterface>
        <interface type="sms" assemblyName="chchdxmessageinterface"/>
    </externalMessageInterface>
         */
        // parameters:
        // return:
        //      -1  出错
        //      0   当前没有配置任何扩展消息接口
        //      1   成功初始化
        public  int InitialExternalMessageInterfaces(XmlDocument dom, out string strError)
        {
            strError = "";

            m_externalMessageInterfaces = null;
            XmlNode root = dom.DocumentElement.SelectSingleNode("externalMessageInterface");
            if (root == null)
            {
                strError = "在weixin.xml中没有找到<externalMessageInterface>元素";
                return 0;
            }

            m_externalMessageInterfaces = new List<MessageInterface>();

            XmlNodeList nodes = root.SelectNodes("interface");
            foreach (XmlNode node in nodes)
            {
                string strType = DomUtil.GetAttr(node, "type");
                if (String.IsNullOrEmpty(strType) == true)
                {
                    strError = "<interface>元素未配置type属性值";
                    return -1;
                }

                string strAssemblyName = DomUtil.GetAttr(node, "assemblyName");
                if (String.IsNullOrEmpty(strAssemblyName) == true)
                {
                    strError = "<interface>元素未配置assemblyName属性值";
                    return -1;
                }

                MessageInterface message_interface = new MessageInterface();
                message_interface.Type = strType;
                message_interface.Assembly = Assembly.Load(strAssemblyName);
                if (message_interface.Assembly == null)
                {
                    strError = "名字为 '" + strAssemblyName + "' 的Assembly加载失败...";
                    return -1;
                }

                Type hostEntryClassType = ScriptManager.GetDerivedClassType(
        message_interface.Assembly,
        "DigitalPlatform.Interfaces.ExternalMessageHost");
                if (hostEntryClassType == null)
                {
                    strError = "名字为 '" + strAssemblyName + "' 的Assembly中未找到 DigitalPlatform.Interfaces.ExternalMessageHost类的派生类，初始化扩展消息接口失败...";
                    return -1;
                }

                message_interface.HostObj = (ExternalMessageHost)hostEntryClassType.InvokeMember(null,
        BindingFlags.DeclaredOnly |
        BindingFlags.Public | BindingFlags.NonPublic |
        BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
        null);
                if (message_interface.HostObj == null)
                {
                    strError = "创建 type 为 '" + strType + "' 的 DigitalPlatform.Interfaces.ExternalMessageHost 类的派生类的对象（构造函数）失败，初始化扩展消息接口失败...";
                    return -1;
                }

                message_interface.HostObj.App =  this;   //如果全部成功是static,这里好像有问题

                this.m_externalMessageInterfaces.Add(message_interface);
            }

            return 1;
        }

        public MessageInterface GetMessageInterface(string strType)
        {
            // 2012/3/29
            if (this.m_externalMessageInterfaces == null)
                return null;

            foreach (MessageInterface message_interface in this.m_externalMessageInterfaces)
            {
                if (message_interface.Type == strType)
                    return message_interface;
            }

            return null;
        }


        public int SendVerifyCodeSMS(string phone,
            string code,
            out string error)
        {
            error = "";


            // 短信接口            
            MessageInterface external_interface = this.GetMessageInterface("sms");

            // 短信模板
            string strMessageTemplate =  "验证码为 %verifycode%。一小时内有效。";
            string strBody = strMessageTemplate.Replace("%verifycode%", code);
            int nRet = 0;
            // 向手机号码发送短信
            {
                // 得到高级xml
                string strXml = "<root><tel>" + phone + "</tel></root>";
                // 发送消息
                try
                {

                    // 发送一条消息
                    // parameters:
                    //      strPatronBarcode    读者证条码号
                    //      strPatronXml    读者记录XML字符串。如果需要除证条码号以外的某些字段来确定消息发送地址，可以从XML记录中取
                    //      strMessageText  消息文字
                    //      strError    [out]返回错误字符串
                    // return:
                    //      -1  发送失败
                    //      0   没有必要发送
                    //      >=1   发送成功，返回实际发送的消息条数
                    nRet = external_interface.HostObj.SendMessage(
                        "", //strBarcode
                        strXml,
                        strBody,
                        "",//lib.libName, //todo,注意这里原来传的code 还是读者的libraryCode
                        out error);
                    if (nRet == -1 || nRet == 0)
                        return nRet;
                }
                catch (Exception ex)
                {
                    error = external_interface.Type + " 类型的外部消息接口Assembly中SendMessage()函数抛出异常: " + ex.Message;
                    nRet = -1;
                }
                if (nRet == -1)
                {
                    error = "向"+phone+"发送" + external_interface.Type + " message时出错：" + error;

                    this.WriteErrorLog(error);
                    return -1;
                }
            }

            return 0;
        }


        #endregion

        #region 写日志

        private  readonly Object _syncRoot = new Object();

         void WriteErrorLog(string strText)
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

        #region 站点配置管理

        // 获取站点列表
        // 
        public List<ZServerItem>   Search(string word,
            string from,
            int start,
            int count)
        {


                 return this.ZServerDb.Get(word,
                    from,
                    start,
                    count).Result;

        }

        HashSet<List<ZServerItem>> result = new HashSet<List<ZServerItem>>();

        // 获取站点列表
        public ZServerItem GetOneZServer(string id)
        {
            return this.ZServerDb.GetById(id).Result;
        }

        // 新增
        public ZServerItem AddZServerItem(ZServerItem item)
        {
            return this.ZServerDb.Add(item).Result;
        }

        // 修改
        public ZServerItem UpdateZServerItem(ZServerItem item)
        {
            return this.ZServerDb.Update(item).Result;
        }

        // 根据id删除站点配置，id可以是逗号分隔的多个id。
        public void DeleteZSererItem(string ids)
        {
            string[] idArray = ids.Split(new char[] { ',' });
            foreach (string one in idArray)
            {
                this.ZServerDb.Delete(one).Wait();
            }
        }

        public void ClearZServerItem()
        {
            this.ZServerDb.Clear().Wait();

        }


        #endregion

    }

    // 一个验证码事项
    public class TempCode
    {
        // 键。一般由用户名 + 电话号码 + 前端 IP 地址组成
        public string Key { get; set; }
        // 验证码
        public string Code { get; set; }
        // 失效时间
        public DateTime ExpireTime { get; set; }
    }
}
