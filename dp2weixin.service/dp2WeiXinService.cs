//#define LOG_REQUEST_SEARCH

using com.google.zxing;
using com.google.zxing.common;
using DigitalPlatform.Interfaces;
using DigitalPlatform.IO;
using DigitalPlatform.Marc;
using DigitalPlatform.Message;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Containers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace dp2weixin.service
{
    public class dp2WeiXinService
    {
        #region 常量

        // 通道常量
        public const string C_ConnName_TraceMessage = "_traceMessage";
        public const string C_ConnPrefix_Myself = "<myself>:";
        public const string C_ConnName_dp = "_dp_";
        public const string C_ConnName_CheckOfflineLibs = "_checkOfflineLibs";

        // 群组常量
        public const string C_Group_Bb = "gn:_lib_bb";
        public const string C_Group_Book = "gn:_lib_book";
        public const string C_Group_HomePage = "gn:_lib_homePage"; //图书馆介绍
        public const string C_Group_PatronNotity = "gn:_patronNotify";
        // 公司公告
        public const string C_Group_dp_home = "gn:_dp_home";

        // 消息权限
        public const string C_Right_SetBb = "_wx_setbb";
        public const string C_Right_SetBook = "_wx_setbook";
        public const string C_Right_SetHomePage = "_wx_setHomePage";
        // 接收警告通知
        public const string C_Right_GetWarning = "_wx_getWarning";

        // 超级管理员标志
        public const string C_Supervisor = "_supervisor_";

        public const string C_Session_Supervisor = "supervisor";

        //// 日志分级
        public const int C_LogLevel_1 = 1;
        public const int C_LogLevel_2 = 2;
        public const int C_LogLevel_3 = 3;

        // 公众号名称
        public const string C_gzh_ilovelibrary = "ilovelibrary";
        public const string C_gzh_dp = "dp";
        public const string C_gzh_dpcomm = "dpcomm";

        #endregion

        #region 成员变量

        //public string OAuth2_Url_PersonalInfo = "";//https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx57aa3682c59d16c2&redirect_uri=http%3a%2f%2fdp2003.com%2fdp2weixin%2fPatron%2fPersonalInfo&response_type=code&scope=snsapi_base&state=dp2weixin#wechat_redirect";
        //public string OAuth2_Url_AccountIndex = "";
        //public string OAuth2_Url_AccountResetPassword = "";
        //public string Auth2Url_LibHome = "";


        // 日志级别
        public int LogLevel = 1;

        // 微信数据目录
        public string weiXinDataDir = "";
        public string weiXinLogDir = "";
        public string _cfgFile = "";      // 配置文件

        // dp2服务器地址与代理账号
        public string dp2MServerUrl = "";
        public string userNameWeixin = "";
        public string password = "";

        public string monodbConnectionString = "";
        public string monodbPrefixString = "";
        public bool bTrace = false;

        // 微信信息
        public GzhContainer gzhContainer = null;
        //public string weiXinAppId { get; set; }
        //public string weiXinSecret { get; set; }
        //public string weixin_Token { get; set; }
        //public string weixin_EncodingAESKey { get; set; }
        //public string ApplName = "";


        // dp2消息处理类
        MsgRouter _msgRouter = new MsgRouter();

        /// <summary>
        /// 通道
        /// </summary>
        MessageConnectionCollection _channels = new MessageConnectionCollection();
        public MessageConnectionCollection Channels
        {
            get
            {
                return this._channels;
            }
        }

        // 打开 tracing的微信用户
        public Hashtable TracingOnUsers = new Hashtable(); //List<TraceOnUser>();


        // 公众号后台管理线程
        ManagerThread _managerThread = new ManagerThread();

        // 图书馆集合类
        public LibraryManager LibManager = new LibraryManager();




        #endregion

        #region 图书馆参于检索的数据库

        public Hashtable LibDbs = new Hashtable();

        public int GetDbNames(LibEntity lib, out string dbnames, out string strError)
        {
            strError = "";
            dbnames = "";
            // 优先认配置的
            if (String.IsNullOrEmpty(lib.searchDbs) == false)
            {
                dbnames = lib.searchDbs;
                return 1;
            }

            string libId = lib.id;
            // 如果已在缓冲中，直接使用
            if (this.LibDbs.ContainsKey(libId) == true)
            {
                dbnames = (string)this.LibDbs[libId];
                return 1;
            }

            // 调服务器api，获得opac检索库
            //string dbnames="";
            int nRet = this.GetDbNamesFromLib(lib, out dbnames, out strError);
            if (nRet == -1)
                return -1;

            // 如果没有配置设全部
            if (dbnames == "")
                dbnames = "<全部>";


            // 设到缓冲里
            this.LibDbs[libId] = dbnames;
            return 1;
        }

        public int GetDbNamesFromLib(LibEntity lib, out string dbnames, out string strError)
        {
            dbnames = "";
            strError = "";
            List<string> dataList = new List<string>();
            int nRet = this.GetSystemParameter(lib, "virtual", "def", out dataList, out strError);
            if (nRet == -1 || nRet == 0)
            {
                return nRet;
            }

            string xml = dataList[0];
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            XmlNode root = dom.DocumentElement;
            XmlNodeList dbList = root.SelectNodes("database");
            foreach (XmlNode node in dbList)
            {
                if (dbnames != "")
                    dbnames += ",";

                string name = DomUtil.GetAttr(node, "name");

                XmlNode captionNode = node.SelectSingleNode("caption[@lang='zh']");
                if (captionNode != null)
                    name = DomUtil.GetNodeText(captionNode);

                dbnames += name;
            }


            return 1;
        }

        public int GetSystemParameter(LibEntity lib,
            string catgory,
            string name,
            out List<string> dataList,
            out string strError)
        {
            // 20170116 使用代理账户
            string userName = "";
            bool isPatron = false;

            return this.GetInfo(lib,
                userName,
                isPatron,
                "getSystemParameter",
                catgory,
                name,
                out dataList,
                out strError);
        }

        public int GetLibName(string capoUserName,
            out string libName,
            out string strError)
        {
            libName = "";
            strError = "";
            List<string> dataList = new List<string>();

            LibEntity lib = new LibEntity();
            lib.capoUserName = capoUserName;
            int nRet = this.GetSystemParameter(lib, "library", "name", out dataList, out strError);
            if (nRet == -1 || nRet == 0)
            {
                if (strError == "TargetNotFound")
                    strError = "账户 " + capoUserName + " 在dp2mserver服务器中不存在 或者对应的桥接服务器失去联系，无法获得图书馆名称。";
                return -1;
            }

            libName = dataList[0];
            return 1;
        }

        #endregion

        #region 单一实例

        static dp2WeiXinService _instance;
        private dp2WeiXinService()
        {
        }
        private static object _lock = new object();
        static public dp2WeiXinService Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (_lock)  //线程安全的
                    {
                        _instance = new dp2WeiXinService();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region 初始化

        public void Init(string dataDir)
        {
            string strError = "";
            int nRet = 0;

            this.weiXinDataDir = dataDir;

            this._cfgFile = this.weiXinDataDir + "\\" + "weixin.xml";
            if (File.Exists(this._cfgFile) == false)
            {
                throw new Exception("配置文件" + this._cfgFile + "不存在。");
            }

            // 日志目录
            this.weiXinLogDir = this.weiXinDataDir + "/log";
            if (!Directory.Exists(weiXinLogDir))
            {
                Directory.CreateDirectory(weiXinLogDir);
            }


            XmlDocument dom = new XmlDocument();
            dom.Load(this._cfgFile);
            XmlNode root = dom.DocumentElement;

            // 取出mserver服务器配置信息
            XmlNode nodeDp2mserver = root.SelectSingleNode("dp2mserver");
            this.dp2MServerUrl = DomUtil.GetAttr(nodeDp2mserver, "url");// WebConfigurationManager.AppSettings["dp2MServerUrl"];
            this.userNameWeixin = DomUtil.GetAttr(nodeDp2mserver, "username");//WebConfigurationManager.AppSettings["userName"];
            this.password = DomUtil.GetAttr(nodeDp2mserver, "password");//WebConfigurationManager.AppSettings["password"];
            if (string.IsNullOrEmpty(this.password) == false)// 解密
                this.password = Cryptography.Decrypt(this.password, WeiXinConst.EncryptKey);

            string trace = DomUtil.GetAttr(nodeDp2mserver, "trace");
            if (trace.ToLower() == "true")
                this.bTrace = true;

            // 取出微信配置信息
            this.gzhContainer = new GzhContainer(dom);
            //XmlNode nodeDp2weixin = root.SelectSingleNode("dp2weixin");
            //this.weiXinAppId = DomUtil.GetAttr(nodeDp2weixin, "AppId"); //WebConfigurationManager.AppSettings["weiXinAppId"];
            //this.weiXinSecret = DomUtil.GetAttr(nodeDp2weixin, "Secret"); //WebConfigurationManager.AppSettings["weiXinSecret"];
            //this.weixin_Token = DomUtil.GetAttr(nodeDp2weixin, "Token");
            //this.weixin_EncodingAESKey = DomUtil.GetAttr(nodeDp2weixin, "EncodingAESKey");
            //this.ApplName = DomUtil.GetAttr(nodeDp2weixin, "AppName");
            //if (this.ApplName == "")
            //    throw new Exception("尚未定义后台应用名称");

            //// 模板id
            //this.Template_Bind = this.GetTemplateId(root, "Bind");
            //this.Template_UnBind = this.GetTemplateId(root, "UnBind");
            //this.Template_Borrow = this.GetTemplateId(root, "Borrow");
            //this.Template_Return = this.GetTemplateId(root, "Return");
            //this.Template_Pay = this.GetTemplateId(root, "Pay");
            //this.Template_CancelPay = this.GetTemplateId(root, "CancelPay");
            //this.Template_Message = this.GetTemplateId(root, "Message");
            //this.Template_Arrived = this.GetTemplateId(root, "Arrived");
            //this.Template_CaoQi = this.GetTemplateId(root, "CaoQi");

            // mongo配置
            XmlNode nodeMongoDB = root.SelectSingleNode("mongoDB");
            this.monodbConnectionString = DomUtil.GetAttr(nodeMongoDB, "connectionString");
            if (String.IsNullOrEmpty(this.monodbConnectionString) == true)
            {
                throw new Exception("尚未配置mongoDB节点的connectionString属性");
            }
            this.monodbPrefixString = DomUtil.GetAttr(nodeMongoDB, "instancePrefix");

            // 打开图书馆账号库与用户库 todo是放在这里打开 还是 在LibraryManager类中打开？
            WxUserDatabase.Current.Open(this.monodbConnectionString, this.monodbPrefixString);
            LibDatabase.Current.Open(this.monodbConnectionString, this.monodbPrefixString);
            UserSettingDb.Current.Open(this.monodbConnectionString, this.monodbPrefixString);

            // 挂上登录事件
            _channels.Login -= _channels_Login;
            _channels.Login += _channels_Login;

            // 将mongodb库中的图书馆加载到内存中
            nRet = this.LibManager.Init(out strError);
            if (nRet == -1)
                throw new Exception("加载图书馆到内存出错：" + strError);

            // 初始化接口类
            nRet = this.InitialExternalMessageInterfaces(dom, out strError);
            if (nRet == -1)
                throw new Exception("初始化接口配置信息出错：" + strError);

            // 加载打开监控功能微信用户
            LoadTracingUser();


            ////全局只需注册一次
            //AccessTokenContainer.Register(this.weiXinAppId, this.weiXinSecret);

            if (bTrace == true)
            {
                if (this.Channels.TraceWriter != null)
                    this.Channels.TraceWriter.Close();
                StreamWriter sw = new StreamWriter(Path.Combine(this.weiXinDataDir, "trace.txt"));
                sw.AutoFlush = true;
                _channels.TraceWriter = sw;
            }

            // 消息处理类
            this._msgRouter.SendMessageEvent -= _msgRouter_SendMessageEvent;
            this._msgRouter.SendMessageEvent += _msgRouter_SendMessageEvent;
            this._msgRouter.Start(this._channels,
                this.dp2MServerUrl,
                C_Group_PatronNotity);

            // 启动管理线程
            this._managerThread.WeixinService = this;
            this._managerThread.BeginThread();

        }

        public void LoadTracingUser()
        {
            List<WxUserItem> userList = WxUserDatabase.Current.GetTracingUsers();
            if (userList != null && userList.Count >0)
            {
                foreach (WxUserItem user in userList)
                {
                    string tracing = user.tracing;
                    TracingOnUser tracingOnUser = new TracingOnUser();
                    tracingOnUser.WeixinId = user.weixinId;
                    if (tracing.IndexOf("-mask") !=-1)
                        tracingOnUser.IsMask = false;

                    LibEntity lib = this.GetLibById(user.libId);
                    if (lib !=null && lib.libName == WeiXinConst.C_Dp2003LibName)
                    {
                        tracingOnUser.IsAdmin = true;
                    }
                    // 设到hashtable里
                    dp2WeiXinService.Instance.TracingOnUsers[user.weixinId] = tracingOnUser;
                }
            }
        }

        public int UpdateTracingUser(string workerId, string tracing, out string error)
        {
            error = "";
            WxUserItem user = WxUserDatabase.Current.GetById(workerId);
            if (user == null)
            {
                error = "在数据库中未找到id=" + workerId + "的绑定信息";
                return -1;
            }

            // 更新数据库设置
            int nRet = (int)WxUserDatabase.Current.UpdateTracing(workerId, tracing);

            // 更新内存
            this.UpdateMemoryTracingUser(user.weixinId, user.libId, tracing);

            return 0;
        }

        public void UpdateMemoryTracingUser(string weixinId,string libId, string tracing)
        {
            if (tracing !="on" && tracing!="on -mask")//== "off")
            {
                if (tracing != "off")
                {
                    this.WriteErrorLog1("异常：tracing值["+tracing+"]不为off。");
                }

                dp2WeiXinService.Instance.TracingOnUsers.Remove(weixinId);
                return;
            }

            // 打开监控功能
            TracingOnUser tracingOnUser = new TracingOnUser();
            tracingOnUser.WeixinId = weixinId;
            if (tracing.IndexOf("-mask") != -1)
                tracingOnUser.IsMask = false;

            LibEntity lib = this.GetLibById(libId);
            if (lib != null && lib.libName == WeiXinConst.C_Dp2003LibName)
            {
                tracingOnUser.IsAdmin = true;
            }
            // 设到hashtable里
            dp2WeiXinService.Instance.TracingOnUsers[weixinId] = tracingOnUser;

        }

        //Patron/PersonalInfo,Account/Index,Account/ResetPassword,Library/Home
        public string GetOAuth2Url(GzhCfg gzh, string func)
        {
            func = func.Replace("/", "%2f");

            // auth2地址
            string url = "https://open.weixin.qq.com/connect/oauth2/authorize"
                + "?appid=" + gzh.appId
                + "&redirect_uri=http%3a%2f%2fdp2003.com%2f" + func
                + "&response_type=code"
                + "&scope=snsapi_base"
                + "&state=" + gzh.appName
                + "#wechat_redirect";
            return url;
        }

        // 得到模板id
        //private string GetTemplateId(XmlNode root, string type)
        //{
        //    XmlNode templateNode = root.SelectSingleNode("templates/template[@name='" + type + "']");
        //    if (templateNode == null)
        //        throw new Exception("尚未配置" + type + "模板");
        //    return DomUtil.GetAttr(templateNode, "id");
        //}

        public void Close()
        {
            if (this._msgRouter != null)
            {
                this._msgRouter.SendMessageEvent -= _msgRouter_SendMessageEvent;
                this._msgRouter.Stop();
            }
            if (this.Channels != null)
            {
                this.Channels.Login -= _channels_Login;
                if (this.Channels.TraceWriter != null)
                    this.Channels.TraceWriter.Close();
            }

            this.WriteLog1("走到close()");
        }

        #endregion

        #region 本方账号登录


        void _channels_Login(object sender, LoginEventArgs e)
        {
            MessageConnection connection = sender as MessageConnection;

            string connName = connection.Name;
            string prefix = C_ConnPrefix_Myself;
            if (connName.Length > prefix.Length && connName.Substring(0, prefix.Length) == prefix)
            {
                // 需要使用当前图书馆的微信端本方帐户
                string libId = connName.Substring(prefix.Length);

                // 全局的联系我们
                if (libId == C_ConnName_dp)
                {

                    e.UserName = GetUserName();
                    if (string.IsNullOrEmpty(e.UserName) == true)
                        throw new Exception("尚未指定用户名，无法进行登录");

                    e.Password = GetPassword();
                }
                else
                {

                    LibEntity lib = this.GetLibById(libId);
                    if (lib == null)
                    {
                        throw new Exception("异常的情况:根据id[" + libId + "]未找到图书馆对象。");
                    }

                    e.UserName = lib.wxUserName;
                    if (string.IsNullOrEmpty(e.UserName) == true)
                        throw new Exception("尚未指定微信本方用户名，无法进行登录");
                    e.Password = lib.wxPassword;
                }

            }
            else
            {
                e.UserName = GetUserName();
                if (string.IsNullOrEmpty(e.UserName) == true)
                    throw new Exception("尚未指定用户名，无法进行登录");


                e.Password = GetPassword();

            }

            e.Parameters = "notes=" + HttpUtility.UrlEncode(connection.Name); //"propertyList=biblio_search,libraryUID=xxx";

        }


        string GetUserName()
        {
            return this.userNameWeixin;
        }
        string GetPassword()
        {
            return this.password;
        }

        #endregion

        #region 设置dp2mserver信息

        public int SetDp2mserverInfo(string dp2mserverUrl,
            string userName,
            string password,
            string mongodbConnection,
            string mongodbPrefix,
            out string strError)
        {
            strError = "";

            string oldUserName = this.GetUserName();
            string oldPassword = this.password;

            // 先检查下地址与密码是否可用，如不可用，不保存
            try
            {
                this.userNameWeixin = userName;
                this.password = password;
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                  dp2mserverUrl,
                    Guid.NewGuid().ToString()).Result;
            }
            catch (AggregateException ex)
            {
                strError = "测试服务器连接不成功：" + MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "测试服务器连接不成功：" + ex.Message;
                goto ERROR1;
            }

            XmlDocument dom = new XmlDocument();
            dom.Load(this._cfgFile);
            XmlNode root = dom.DocumentElement;

            // 设置mserver服务器配置信息
            XmlNode nodeDp2mserver = root.SelectSingleNode("dp2mserver");
            if (nodeDp2mserver == null)
            {
                nodeDp2mserver = dom.CreateElement("dp2mserver");
                root.AppendChild(nodeDp2mserver);
            }
            DomUtil.SetAttr(nodeDp2mserver, "url", dp2mserverUrl);
            DomUtil.SetAttr(nodeDp2mserver, "username", userName);
            string encryptPassword = Cryptography.Encrypt(password, WeiXinConst.EncryptKey);
            DomUtil.SetAttr(nodeDp2mserver, "password", encryptPassword);
            dom.Save(this._cfgFile);

            // 更新内存的信息
            this.dp2MServerUrl = dp2mserverUrl;
            this.userNameWeixin = userName;
            this.password = password;

            return 0;

        ERROR1:
            // 还原原来的值
            this.userNameWeixin = oldUserName;
            this.password = oldPassword;
            return -1;
        }

        public void GetDp2mserverInfo(out string dp2mserverUrl,
            out string userName,
            out string password,
            out string mongodbConnection,
            out string mongodbPrefix)
        {
            dp2mserverUrl = "";
            userName = "";
            password = "";
            mongodbConnection = "";
            mongodbPrefix = "";

            XmlDocument dom = new XmlDocument();
            dom.Load(this._cfgFile);
            XmlNode root = dom.DocumentElement;

            // 设置mserver服务器配置信息
            XmlNode nodeDp2mserver = root.SelectSingleNode("dp2mserver");
            if (nodeDp2mserver != null)
            {
                dp2mserverUrl = DomUtil.GetAttr(nodeDp2mserver, "url");
                userName = DomUtil.GetAttr(nodeDp2mserver, "username");
                password = DomUtil.GetAttr(nodeDp2mserver, "password");
                if (string.IsNullOrEmpty(password) == false)// 解密
                    password = Cryptography.Decrypt(this.password, WeiXinConst.EncryptKey);
            }

            // 设置mongoDB
            XmlNode nodeMongoDB = root.SelectSingleNode("mongoDB");
            if (nodeMongoDB != null)
            {
                mongodbConnection = DomUtil.GetAttr(nodeMongoDB, "connectionString");
                mongodbPrefix = DomUtil.GetAttr(nodeMongoDB, "instancePrefix");
            }
        }

        public void GetSupervisorAccount(out string username,
            out string password)
        {
            username = "";
            password = "";

            XmlDocument dom = new XmlDocument();
            dom.Load(this._cfgFile);
            XmlNode root = dom.DocumentElement;

            // 设置mserver服务器配置信息
            XmlNode nodeSupervisor = root.SelectSingleNode("Supervisor");
            if (nodeSupervisor != null)
            {
                username = DomUtil.GetAttr(nodeSupervisor, "username");
                password = DomUtil.GetAttr(nodeSupervisor, "password");
                if (string.IsNullOrEmpty(password) == false)// 解密
                    password = Cryptography.Decrypt(password, WeiXinConst.EncryptKey);
            }
        }

        #endregion

        #region 消息处理

        // 处理收到的消息
        void _msgRouter_SendMessageEvent(object sender, SendMessageEventArgs e)
        {
            MessageRecord record = e.Message;
            if (record == null)
            {
                this.WriteErrorLog1("传过来的e.Message为null");
                return;
            }

            this.WriteLog3("收到消息[" + record.id + "]准备处理，publishTime=" + record.publishTime);

            //this.WriteErrorLog("走进_msgRouter_SendMessageEvent");

            try
            {
                LibEntity lib = null;
                if (string.IsNullOrEmpty(record.userName) == false)
                {
                    lib = LibDatabase.Current.GetLibByCapoUserName(record.userName);
                    if (lib == null)
                    {
                        this.WriteErrorLog1("未找到[" + record.userName + "]对应的图书馆。");
                    }
                }
                else
                {
                    this.WriteErrorLog1("异常：消息[" + record.id + "]传过来的userName为空。");
                }


                string strError = "";
                /// <returns>
                /// -1 不符合条件，不处理
                /// 1 成功
                /// </returns>
                int nRet = this.InternalDoMessage(record, lib, out strError);
                if (nRet == -1)
                {
                    this.WriteErrorLog1("[" + record.id + "]未发送成功:" + strError);
                }
                else
                {
                    this.WriteLog3("消息[" + record.id + "]处理完成。");
                }
            }
            catch (Exception ex)
            {
                this.WriteErrorLog1("[" + record.id + "]异常：" + ex.Message);

            }
        }

        /// <summary>
        /// 内部处理消息
        /// </summary>
        /// <param name="record"></param>
        /// <param name="strError"></param>
        /// <returns>
        /// -1 不符合条件，不处理
        /// 0 未绑定微信id，未处理
        /// 1 成功
        /// </returns>
        public int InternalDoMessage(MessageRecord record, LibEntity lib, out string strError)
        {
            strError = "";

            string id = record.id;
            string data = record.data;
            string[] group = record.groups;
            string create = record.creator;

            //===检查是不是patronNotify通知===
            //<root>
            //    <type>patronNotify</type>
            //    <recipient>R0000001@LUID:62637a12-1965-4876-af3a-fc1d3009af8a</recipient>
            //    <mime>xml</mime>
            //    <body>...</body>
            //</root>
            XmlDocument dataDom = new XmlDocument();
            try
            {
                dataDom.LoadXml(data);
            }
            catch (Exception ex)
            {
                strError = "加载消息data到xml出错：" + ex.Message;
                return -1;
            }

            XmlNode nodeType = dataDom.DocumentElement.SelectSingleNode("type");
            if (nodeType == null)
            {
                strError = "尚未定义<type>节点";
                return -1;
            }
            string topType = DomUtil.GetNodeText(nodeType);
            if (topType != "patronNotify") //只处理通知消息
            {
                strError = "<type>节点值不是patronNotify。";
                return -1;
            }

            XmlNode nodeBody = dataDom.DocumentElement.SelectSingleNode("body");
            if (nodeBody == null)
            {
                strError = "data中不存在body节点";
                return -1;
            }

            //===处理各类通知===
            /*
            body元素里面是预约到书通知记录(注意这是一个字符串，需要另行装入一个XmlDocument解析)，其格式如下：
            <?xml version="1.0" encoding="utf-8"?>
            <root>
                <type>预约到书通知</type>
                ...
            </root
            */
            XmlDocument bodyDom = new XmlDocument();
            try
            {
                bodyDom.LoadXml(nodeBody.InnerText);//.InnerXml); 
            }
            catch (Exception ex)
            {
                strError = "加载消息data中的body到xml出错：" + ex.Message;
                return -1;
            }

            //===根据type处理各类消息===
            int nRet = 0;

            XmlNode root = bodyDom.DocumentElement;
            XmlNode typeNode = root.SelectSingleNode("type");
            if (typeNode == null)
            {
                strError = "消息data的body中未定义type节点。";
                return -1;
            }
            string strType = DomUtil.GetNodeText(typeNode);

            //===数据处理消息================
            if (strType == "工作人员账户变动")
            {
                nRet = this.UpdateWorker(lib, bodyDom, out strError);
                return nRet;
            }
            if (strType == "读者记录变动")
            {
                nRet = this.UpdatePatron(lib, bodyDom, out strError);
                return nRet;
            }
            if (strType == "登录验证码")
            {
                /*
                 <root>
                    <type>登录验证码</type>
                    <phoneNumber>123</phoneNumber>
                    <text>登录验证码为9999。十分钟以后失效</text>
                </root>
                 */
                nRet = this.SendLoginVerifyCode(lib,bodyDom, out strError);
                return nRet;
            }



            //===微信通知====
            string libName = "";
            string libId = "";
            if (lib != null)
            {
                libName = lib.libName;
                libId = lib.id;
            }

            // 得到此读者对应的绑定的微信id
            string patronBarcode = "";
            string patronName = "";
            string libraryCode = "";
            List<string> bindWeixinIds = this.GetBindWeiXinIds(bodyDom,
                out patronBarcode,
                out patronName,
                out libraryCode);

            // patronInfo = "";  //姓名 证条码号（图书馆/分馆）
            string fullPatronName = this.GetFullPatronName(patronName, patronBarcode, libName, libraryCode, false);
            string markFullPatronName = this.GetFullPatronName(patronName, patronBarcode, libName, libraryCode, true);


            // 得到找开tracing功能的工作人员微信id
            List<TracingOnUser> workerWeixinIds = this.getWorkerWeixinIds(libId, libraryCode);

            // 没有绑定微信id,也没有打开tracing的工作人员，不处理消息
            if (bindWeixinIds.Count == 0 && workerWeixinIds.Count == 0)
            {
                strError = "未绑定微信id,也没有打开tracing的工作人员";
                return 0;
            }

            // 根据类型发送不同的模板消息
            if (strType == "预约到书通知")
            {
                nRet = this.SendArrived(bodyDom,
                    libName,
                    bindWeixinIds,
                    fullPatronName,
                    markFullPatronName,
                    workerWeixinIds,
                    out strError);
            }
            else if (strType == "超期通知")
            {
                nRet = this.SendCaoQi(bodyDom,
                    libName,
                    bindWeixinIds,
                    fullPatronName,
                    markFullPatronName,
                    workerWeixinIds,
                    out strError);
            }
            else if (strType == "借书成功")
            {
                nRet = this.SendBorrowMsg(bodyDom,
                    libName,
                    bindWeixinIds,
                    patronBarcode,
                    patronName,
                    libraryCode,
                    workerWeixinIds,
                    out strError);
            }
            else if (strType == "还书成功")
            {
                nRet = this.SendReturnMsg(bodyDom,
                    libName,
                    bindWeixinIds,
                    fullPatronName,
                    markFullPatronName,
                    workerWeixinIds,
                    out strError);
            }
            else if (strType == "交费")
            {
                nRet = this.SendPayMsg(bodyDom,
                    libName,
                    bindWeixinIds,
                    fullPatronName,
                    markFullPatronName,
                    workerWeixinIds,
                    out strError);
            }
            else if (strType == "撤销交费")
            {
                nRet = this.SendCancelPayMsg(bodyDom,
                    libName,
                    bindWeixinIds,
                    fullPatronName,
                    markFullPatronName,
                    workerWeixinIds,
                    out strError);
            }
            else if (strType == "以停代金到期")
            {
                nRet = this.SendYtdjMsg(bodyDom,
                    libName,
                    bindWeixinIds,
                    fullPatronName,
                    markFullPatronName,
                    workerWeixinIds,
                    out strError);
            }
            else
            {
                strError = "不支持的消息类型[" + strType + "]";
                return -1;
            }

            return nRet;
        }

        /// <summary>
        /// 发送登录验证码
        /// </summary>
        /// <param name="bodyDom"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        private int SendLoginVerifyCode(LibEntity lib, XmlDocument bodyDom, out string strError)
        {
            strError = "";
            /*
             <root>
                <type>登录验证码</type>
                <phoneNumber>123</phoneNumber>
                <text>登录验证码为9999。十分钟以后失效</text>
            </root>
             */
            XmlNode root = bodyDom.DocumentElement;
            //手机号
            XmlNode phoneNumberNode = root.SelectSingleNode("phoneNumber");
            string phone = DomUtil.GetNodeText(phoneNumberNode);
            //内容
            XmlNode textNode = root.SelectSingleNode("text");
            string strBody = DomUtil.GetNodeText(textNode);

            // 得到xml
            string strXml = "<root><tel>" + phone + "</tel></root>";
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
                string strBarcode = "";
                string libraryCode = "";
                if (lib != null)
                    libraryCode = lib.libName;
                MessageInterface external_interface = this.GetMessageInterface("sms");
                int nRet = external_interface.HostObj.SendMessage(
                    strBarcode,
                    strXml,
                    strBody,
                    libraryCode, //todo,注意这里原来传的code 还是读者的libraryCode
                    out strError);
                if (nRet == -1)
                {
                    strError = "发送登录验证码出错：" + strError;
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strError = "发送登录验证码 异常: " + ex.Message;
                return -1;
            }

            return 0;
        }

        #region 内容函数

        private string _msgFirstLeft = "尊敬的读者：您好，";
        private string _msgRemark = "如有疑问，请联系系统管理员。";


        private string markString(string text)
        {
            if (String.IsNullOrEmpty(text) == true)
                return "";

            return text.Substring(text.Length - 1).PadLeft(text.Length, '*');
        }

        /*
        private int SendWeixinMsg3(List<string> weixinIds,
          string templateId,
          object msgData,
          string linkUrl,
          out string strError)
        {
            return this.SendWeixinMsg1(weixinIds,
                templateId,
                msgData,
                linkUrl,
                "",
                out strError);
        }
         */


        /// <summary>
        /// 发送微信通知
        /// </summary>
        /// <param name="weixinIds">微信id数组</param>
        /// <param name="templateName">模板名</param>
        /// <param name="msgData">发送内容</param>
        /// <param name="linkUrl">链接地址</param>
        /// <param name="theOperator">
        /// 操作人,这里为什么没有把操作人放在内容里，是因为要与日期拼起来
        /// 而日期是在这里统一加上的
        /// </param>
        /// <param name="strError"></param>
        /// <returns></returns>
        private int SendTemplateMsgInternal(List<string> weixinIds,
          string templateName,
          object msgData,
          string linkUrl,
          string theOperator,
          out string strError)
        {
            strError = "";

            BaseTemplateData templateData = (BaseTemplateData)msgData;
            string oldRemark = templateData.remark.value;

            // 加日期与操作人
            string nowTime = DateTimeUtil.DateTimeToStringNoSec(DateTime.Now);
            templateData.remark.value = oldRemark + "\n" + nowTime;
            if (theOperator != "")
                templateData.remark.value = templateData.remark.value + " " + theOperator;

            // 一个人给微信用户发送通知
            foreach (string oneWeixinId in weixinIds)
            {
                try
                {
                    string weixinId = oneWeixinId;
                    GzhCfg gzh = null;

                    // 检查weixinid是否包括@
                    int nIndex = oneWeixinId.IndexOf("@");
                    if (nIndex > 0)
                    {
                        weixinId = oneWeixinId.Substring(0, nIndex);
                        string gzhAppId = oneWeixinId.Substring(nIndex + 1);
                        if (gzhAppId != "")
                        {
                            gzh = this.gzhContainer.GetByAppId(gzhAppId);
                        }
                    }
                    // 如果微信id中没带appid，取默认公众号设置
                    if (gzh == null)
                    {
                        gzh = this.gzhContainer.GetDefault();
                    }
                    if (gzh == null)
                    {
                        this.WriteErrorLog1("未找到对应的公众号");
                        continue;
                    }

                    // 调微信接口发送消息
                    string templateId = gzh.GetTemplateId(templateName);
                    var accessToken = AccessTokenContainer.GetAccessToken(gzh.appId);
                    var result1 = TemplateApi.SendTemplateMessage(accessToken,
                        weixinId,
                        templateId,
                        linkUrl,
                        templateData);
                    if (result1.errcode != 0)
                    {
                        strError = result1.errmsg;
                        //return -1;
                        this.WriteErrorLog1(strError);
                        continue;
                    }
                }
                catch (Exception ex0)
                {
                    strError = "给[" + oneWeixinId + "]发送" + templateName + "微信通知异常:" + ex0.Message;
                    //return -1;
                    this.WriteErrorLog1(strError);
                    continue;
                }
            }

            // 还回原来的值，因为是引用型，外面还在用这个data
            templateData.remark.value = oldRemark;

            return 0;
        }


        /// <summary>
        /// 发给微信模板消息统一入口
        /// </summary>
        /// <param name="templateName">模板名称</param>
        /// <param name="weixinIds">微信用户id数组</param>
        /// <param name="workers">工作人员数组</param>
        /// <param name="msgData">显文消息内容</param>
        /// <param name="markMsgData">mask消息内容</param>
        /// <param name="linkUrl">链接</param>
        /// <param name="theOperator">操作人</param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int SendTemplateMsg(string templateName,
            List<string> weixinIds,
            List<TracingOnUser> workers,
            object msgData,
            object maskMsgData,
            string linkUrl,
            string theOperator,
            out string strError)
        {
            int nRet = 0;
            strError = "";

            // 发给本人
            if (weixinIds.Count > 0)
            {
                nRet = this.SendTemplateMsgInternal(weixinIds,
                    templateName,
                    msgData,
                    linkUrl,
                    "",// theOperator,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            // 发给工作人员
            if (workers.Count > 0)
            {
                // 将工作人员分成两组，一组是发全文内容，一组是发mask内容
                List<string> workerIds = null;
                List<string> maskWorkerIds = null;
                SplitWorkers(workers, out workerIds, out maskWorkerIds);
                if (workerIds.Count > 0)
                {
                    nRet = this.SendTemplateMsgInternal(workerIds,
                        templateName,
                        msgData,
                        linkUrl,
                        theOperator,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                if (maskWorkerIds.Count > 0)
                {
                    nRet = this.SendTemplateMsgInternal(maskWorkerIds,
                        templateName,
                        maskMsgData,
                        linkUrl,
                        theOperator,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
            }
            return 0;
        }

        private void SplitWorkers(List<TracingOnUser> users,
             out List<string> workerIds,
            out List<string> maskWorkerIds)
        {

            workerIds = new List<string>();
            maskWorkerIds = new List<string>();

            // 将这边tracing on的工作人员分为2组，一组是mask的，一组是不mask的
            foreach (TracingOnUser user in users)
            {
                string fullWeixinId = user.WeixinId;//2016-11-16 weixinId带了@appId // + "@" + user.AppId;

                if (user.IsMask == false)
                    workerIds.Add(user.WeixinId);
                else
                    maskWorkerIds.Add(user.WeixinId);
            }
        }

        /*
        private int SendWeixinMsg2(List<TracingOnUser> users,
          string templateName,
          object msgData,
          object markMsgData,
          string linkUrl,
            string theOperator,
          out string strError)
        {
            strError = "";
            int nRet = 0;
            List<string> weixinIds = new List<string>();
            List<string> maskWeixinIds = new List<string>();

            // 将这边tracing on的工作人员分为2组，一组是mask的，一组是不mask的
            foreach (TracingOnUser user in users)
            {
                if (user.IsMask == false)
                    weixinIds.Add(user.WeixinId);
                else
                    maskWeixinIds.Add(user.WeixinId);
            }

            if (weixinIds.Count > 0)
            {
                nRet = this.SendWeixinMsg1(weixinIds,
                    templateName,
                    msgData,
                    linkUrl,
                    theOperator,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            if (maskWeixinIds.Count > 0)
            {
                nRet = this.SendWeixinMsg1(maskWeixinIds,
                    templateName,
                    markMsgData,
                    linkUrl,
                    theOperator,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }
        */
        /// <summary>
        /// 获取读者记录中绑定的微信id,返回数组
        /// </summary>
        /// <param name="bodyDom"></param>
        /// <param name="patronName"></param>
        /// <returns></returns>
        private List<string> GetBindWeiXinIds(XmlDocument bodyDom,
            out string patronBarcode,
            out string patronName,
            out string libraryCode)
        {
            patronBarcode = "";
            patronName = "";
            libraryCode = "";

            XmlNode root = bodyDom.DocumentElement;
            XmlNode patronRecordNode = root.SelectSingleNode("patronRecord");
            if (patronRecordNode == null)
                throw new Exception("尚未定义<patronRecordNode>节点");

            // barcode
            XmlNode node = patronRecordNode.SelectSingleNode("barcode");
            if (node != null)
                patronBarcode = DomUtil.GetNodeText(node);

            // name
            node = patronRecordNode.SelectSingleNode("name");
            if (node != null)
                patronName = DomUtil.GetNodeText(node);

            // libraryCode
            node = patronRecordNode.SelectSingleNode("libraryCode");
            if (node != null)
                libraryCode = DomUtil.GetNodeText(node);

            // 取微信id
            List<string> weixinIdList = new List<string>();
            XmlNode emailNode = patronRecordNode.SelectSingleNode("email");
            if (emailNode != null)
            {
                string email = DomUtil.GetNodeText(emailNode);
                //<email>test@163.com,123,weixinid:o4xvUviTxj2HbRqbQb9W2nMl4fGg,weixinid:o4xvUvnLTg6NnflbYdcS-sxJCGFo,weixinid:testid</email>
                weixinIdList = this.GetWeixinIds(email);
            }
            return weixinIdList;
        }

        //private List<WxUserItem> getWorkerWeixinIds(string libId)
        //{
        //    List<string> weixinIds = new List<string>();

        //    // 再查找绑定本馆工作人员账户;
        //    if (String.IsNullOrEmpty(libId) == false)
        //    {
        //        List<WxUserItem> userList = WxUserDatabase.Current.Get(null, libId, WxUserDatabase.C_Type_Worker);
        //        if (userList != null && userList.Count > 0)
        //        {
        //            foreach (WxUserItem user in userList)
        //            {
        //                weixinIds.Add(user.weixinId);                        
        //            }
        //        }
        //    }

        //    return weixinIds;
        //}

        private List<TracingOnUser> getWorkerWeixinIds(string libId, string libraryCode)
        {
            // ====给工作人员发通知===

            // 先找到公司管理员
            List<TracingOnUser> workerWeixinIds = this.GetAdminWeixinIds();

            // 再查找绑定本馆工作人员账户;
            if (String.IsNullOrEmpty(libId) == false)
            {
                List<WxUserItem> userList = WxUserDatabase.Current.Get(null, libId, WxUserDatabase.C_Type_Worker);
                if (userList != null && userList.Count > 0)
                {
                    foreach (WxUserItem user in userList)
                    {
                        // 不属于同一分馆则不处理
                        if (user.libraryCode != "" && user.libraryCode != libraryCode)
                            continue;


                        // 检查是否设为tracing on
                        if (this.TracingOnUsers.ContainsKey(user.weixinId) == true)
                        {
                            TracingOnUser traceUser = (TracingOnUser)this.TracingOnUsers[user.weixinId];
                            if (workerWeixinIds.Contains(traceUser) == false)
                            {
                                workerWeixinIds.Add(traceUser);
                            }
                        }
                    }
                }
            }

            return workerWeixinIds;
        }

        private List<TracingOnUser> GetAdminWeixinIds()
        {
            List<TracingOnUser> weixinIds = new List<TracingOnUser>();

            foreach (string key in this.TracingOnUsers.Keys)
            {
                var user = (TracingOnUser)this.TracingOnUsers[key];
                if (user.IsAdmin == true)
                {
                    weixinIds.Add(user);
                }
            }

            return weixinIds;
        }

        private string GetFullPatronName(string patronName,
            string patronCode,
            string libName,
            string libraryCode,
            bool bMark)
        {
            string info = "";
            if (string.IsNullOrEmpty(patronName) == false)
            {
                if (bMark == false)
                    info += patronName;
                else
                    info += this.markString(patronName);
            }

            if (string.IsNullOrEmpty(patronCode) == false)
            {
                if (info != "")
                    info += " ";

                if (bMark == false)
                    info += patronCode;
                else
                    info += this.markString(patronCode);
            }

            string fullLibName = this.GetFullLibName(libName, libraryCode, "");
            if (String.IsNullOrEmpty(fullLibName) == false)
            {
                info += "(" + fullLibName + ")";
            }

            return info;
        }

        private string GetFullLibName(string libName, string libraryCode, string location)
        {
            string fullLibName = libName;

            if (String.IsNullOrEmpty(libraryCode) == false)
            {
                if (fullLibName != "")
                    fullLibName += "/";

                fullLibName += libraryCode;
            }

            if (String.IsNullOrEmpty(location) == false)
            {
                if (fullLibName != "")
                    fullLibName += "/";

                fullLibName += location;
            }

            return fullLibName;
        }

        private string GetFullItemBarcode(string itemBarcode, string libName, string location)
        {
            string info = itemBarcode;

            string fullLibName = this.GetFullLibName(libName, "", location);
            if (fullLibName != "")
                info += "(" + fullLibName + ")";

            return info;
        }

        public string GetShortSummary(string summary)
        {
            int nIndex = summary.IndexOf("/");
            if (nIndex > 0)
            {
                summary = summary.Substring(0, nIndex);
            }
            else
            {
                //西文标点，点、空、横、横、空
                nIndex = summary.IndexOf(". -- ");
                if (nIndex > 0)
                {
                    summary = summary.Substring(0, nIndex);
                }
            }
            return summary;
        }

        #endregion



        /// <returns>
        /// 处理以停代金消息
        /// </returns>
        private int SendYtdjMsg(XmlDocument bodyDom,
            string libName,
            List<string> bindWeixinIds,
            string fullPatronName,
            string maskFullPatronName,
            List<TracingOnUser> workers,
            out string strError)
        {
            strError = "";
            /*
<root>
    <type>以停代金到期</type>
…

根元素下的items元素下，有一个或者多个overdue元素，记载了刚到期的以停代金事项信息。
在patronRecord的下级元素overdues下，可以看到若干overdue元素，这是当前还未到期或者交费的事项。只要还有这样的事项，读者就不被允许借书，只能还书。所以通知消息文字组织的时候，可以考虑提醒尚余多少违约事项，这样可以避免读者空高兴一场以为马上可以借书了
     
           */

            XmlNode root = bodyDom.DocumentElement;

            // 操作时间
            XmlNode nodeOperTime = root.SelectSingleNode("operTime");
            if (nodeOperTime == null)
            {
                strError = "尚未定义<borrowDate>节点";
                return -1;
            }
            string operTime = DomUtil.GetNodeText(nodeOperTime);
            operTime = DateTimeUtil.ToLocalTime(operTime, "yyyy/MM/dd");

            // 多笔以停代金事项
            XmlNodeList listOverdue = root.SelectNodes("items/overdue");
            string barcodes = "";
            double totalPrice = 0;

            string lastLocation = "";
            foreach (XmlNode node in listOverdue)
            {
                string oneBarcode = DomUtil.GetAttr(node, "barcode");

                string tempLibName = libName;

                string location = DomUtil.GetAttr(node, "location");
                if (location != lastLocation)
                {
                    // 更新lastLocation
                    lastLocation = location;
                }
                else
                {
                    // location与上一item的location相同，则不见显示
                    tempLibName = "";
                    location = "";
                }

                string fullItemBarcode = this.GetFullItemBarcode(oneBarcode, tempLibName, location);

                if (barcodes != "")
                    barcodes += ",";
                barcodes += fullItemBarcode;

                string price = DomUtil.GetAttr(node, "price");
                if (String.IsNullOrEmpty(price) == false && price.Length > 3)
                {
                    double dPrice = Convert.ToDouble(price.Substring(3));
                    totalPrice += dPrice;
                }
            }
            string strText = fullPatronName + "，您有册条码为 " + barcodes + " 项违约以停代金到期了，";

            // 还存在违约项未到期的情况
            XmlNodeList listOverdue1 = root.SelectNodes("patronRecord/overdues/overdue");
            if (listOverdue1.Count > 0)
            {
                strText += "您还有" + listOverdue1.Count.ToString() + "项违约未到期，还不能借书。";
            }
            else
            {
                strText += "您可以继续借书了。";
            }

            // 备注
            string remark = "\n" + this._msgRemark;

            string first_text = "☀☀☀☀☀☀☀☀☀☀";
            string first_color = "#9400D3";
            string title = "以停代金到期";
            //{{first.DATA}}
            //标题：{{keyword1.DATA}}
            //时间：{{keyword2.DATA}}
            //内容：{{keyword3.DATA}}
            //{{remark.DATA}}
            MessageTemplateData msgData = new MessageTemplateData(first_text,
                first_color,
                title,
                operTime,
                strText,
                remark);

            // mask
            strText = strText.Replace(fullPatronName, maskFullPatronName);
            MessageTemplateData maskMsgData = new MessageTemplateData(first_text,
                first_color,
                title,
                operTime,
                strText,
                remark);

            int nRet = this.SendTemplateMsg(GzhCfg.C_Template_Message,
                bindWeixinIds,
                workers,
                msgData,
                maskMsgData,
                "",//linkurl
                "",//theOperator,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }


        /// 借书成功
        /// <returns>
        /// -1 出错，格式出错或者发送模板消息出错
        /// 0 未绑定微信id
        /// 1 成功
        /// </returns>
        private int SendBorrowMsg(XmlDocument bodyDom,
            string libName,
            List<string> bindWeixinIds,
            string patronBarcode,
            string patronName,
            string patronLibraryCode,
            List<TracingOnUser> workers,
            out string strError)
        {
            strError = "";
            /*
 <root>
  <type>借书成功</type>
  <libraryCode></libraryCode>
  <operation>borrow</operation>
  <action>borrow</action>
  <readerBarcode>R0000001</readerBarcode>
  <itemBarcode>0000001</itemBarcode>
  <borrowDate>Sun, 22 May 2016 19:48:01 +0800</borrowDate>
  <borrowPeriod>31day</borrowPeriod>
  <returningDate>Wed, 22 Jun 2016 12:00:00 +0800</returningDate>
  <price>$4.65</price>
  <no>0</no>
  <bookType>普通</bookType>
  <operator>supervisor</operator>
  <operTime>Sun, 22 May 2016 19:48:01 +0800</operTime>
  <clientAddress>::1</clientAddress>
  <version>1.02</version>
  <uid>062724d8-80c1-4752-979a-b1cd548466be</uid>
  <patronRecord>
    <barcode>R0000001</barcode>
    <readerType>本科生</readerType>
    <name>张三</name>
    <borrows>
      <borrow barcode="0000001" recPath="中文编目实体/1" biblioRecPath="中文编目/602" borrowDate="Sun, 22 May 2016 19:48:01 +0800" borrowPeriod="31day" returningDate="Wed, 22 Jun 2016 12:00:00 +0800" operator="supervisor" type="普通" price="$4.65" />
    </borrows>
    <refID>be13ecc5-6a9c-4400-9453-a072c50cede1</refID>
    <department>/</department>
    <address>address</address>
    <cardNumber>C12345</cardNumber>
    <refid>8aa41a9a-fb42-48c0-b9b9-9d6656dbeb76</refid>
    <email>email:xietao@dp2003.com,weixinid:testwx2,testid:123456</email>
    <idCardNumber>1234567890123</idCardNumber>
    <tel>13641016400</tel>
    <libraryCode></libraryCode>
  </patronRecord>
  <itemRecord>
    <parent>602</parent>
    <refID>59b613c6-fe09-4280-8884-43f2b045c41c</refID>
    <barcode>0000001</barcode>
    <location>流通库</location>
    <price>$4.65</price>
    <bookType>普通</bookType>
    <accessNo>U664.121/N590</accessNo>
    <borrower>R0000001</borrower>
    <borrowerReaderType>本科生</borrowerReaderType>
    <borrowerRecPath>读者/1</borrowerRecPath>
    <borrowDate>Sun, 22 May 2016 19:48:01 +0800</borrowDate>
    <borrowPeriod>31day</borrowPeriod>
    <returningDate>Wed, 22 Jun 2016 12:00:00 +0800</returningDate>
    <operator>supervisor</operator>
    <summary>船舶柴油机 / 聂云超主编. -- ISBN 7-81007-115-7 : $4.65</summary>
  </itemRecord>
</root>            
           */
            XmlNode root = bodyDom.DocumentElement;
            //<itemBarcode>0000001</itemBarcode>
            //<borrowDate>Sun, 22 May 2016 19:48:01 +0800</borrowDate>
            //<borrowPeriod>31day</borrowPeriod>
            //<returningDate>Wed, 22 Jun 2016 12:00:00 +0800</returningDate>

            // 册条码
            XmlNode nodeItemBarcode = root.SelectSingleNode("itemBarcode");
            if (nodeItemBarcode == null)
            {
                strError = "尚未定义<itemBarcode>节点";
                return -1;
            }
            string itemBarcode = DomUtil.GetNodeText(nodeItemBarcode);

            //期限
            XmlNode nodeBorrowPeriod = root.SelectSingleNode("borrowPeriod");
            if (nodeBorrowPeriod == null)
            {
                strError = "尚未定义<borrowPeriod>节点";
                return -1;
            }
            string borrowPeriod = DomUtil.GetNodeText(nodeBorrowPeriod);
            borrowPeriod = GetDisplayTimePeriodStringEx(borrowPeriod);

            // 借书日期
            XmlNode nodeBorrowDate = root.SelectSingleNode("borrowDate");
            if (nodeBorrowDate == null)
            {
                strError = "尚未定义<borrowDate>节点";
                return -1;
            }
            string borrowDate = DomUtil.GetNodeText(nodeBorrowDate);
            borrowDate = DateTimeUtil.ToLocalTime(borrowDate, "yyyy/MM/dd");
            // 借书日期的值为 日期+期限 2016-9-2
            borrowDate = borrowDate + " 期限为" + borrowPeriod;

            //应还日期
            XmlNode nodeReturningDate = root.SelectSingleNode("returningDate");
            if (nodeReturningDate == null)
            {
                strError = "尚未定义<returningDate>节点";
                return -1;
            }
            string returningDate = DomUtil.GetNodeText(nodeReturningDate);
            returningDate = DateTimeUtil.ToLocalTime(returningDate, "yyyy/MM/dd");

            // 摘要
            XmlNode nodeSummary = root.SelectSingleNode("itemRecord/summary");
            if (nodeSummary == null)
            {
                strError = "尚未定义itemRecord/summary节点";
                return -1;
            }
            string summary = DomUtil.GetNodeText(nodeSummary);

            // 馆藏地
            string location = "";
            XmlNode nodeLocation = root.SelectSingleNode("itemRecord/location");
            if (nodeLocation != null)
                location = DomUtil.GetNodeText(nodeLocation);

            // 册条码完整表示 C001 图书馆/馆藏地
            string fullItemBarcode = this.GetFullItemBarcode(itemBarcode, libName, location);

            // 操作人 operator
            string theOperator = "";
            XmlNode nodeOperator = root.SelectSingleNode("operator");
            if (nodeOperator != null)
            {
                theOperator = DomUtil.GetNodeText(nodeOperator);
                //if (String.IsNullOrEmpty(theOperator) == false)
                //    theOperator = " 操作者：" + theOperator;
            }

            // 备注
            string remark = patronName + "，祝您阅读愉快。";//，欢迎再借。";


            // 完整证条码 
            string fullPatronBarcode = this.GetFullPatronName("", patronBarcode, libName, patronLibraryCode,false);
            summary = this.GetShortSummary(summary);

            //您好，您已借书成功。 腾讯工作人员您好，虽然模板库中已存在类似模板，但与我司的字段定义不同，我司为几千家图书馆提供专业服务，需要采用专业术语（例如书刊摘要，册条码号，证条码号等）,以免被行内人士吐槽，请批准，谢谢！
            //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00
            //册条码号：C0000001
            //借书日期：2016-07-01 期限为31天
            //应还日期：2016-07-31
            //证条码号：R0000001
            //xxx，祝您阅读愉快，欢迎再借。
            string first="▉▊▋▍▎▉▊▋▍▎▉▊▋▍▎";
            string first_color="#006400";
            BorrowTemplateData msgData = new BorrowTemplateData(first,
                first_color,
                summary,
                fullItemBarcode,
                borrowDate,
                returningDate,
                fullPatronBarcode,
                remark);

            //mask
            //证条码号处
            string tempFullPatronBarcode =this.GetFullPatronName("", patronBarcode, libName, patronLibraryCode, true);
            //备注姓名
            string markPatronName = this.markString(patronName);
            string tempRemark = remark.Replace(patronName, markPatronName);// +theOperator; ;
            BorrowTemplateData maskMsgData = new BorrowTemplateData(first,
                first_color,
                summary,
                fullItemBarcode,
                borrowDate,
                returningDate,
                tempFullPatronBarcode,
                tempRemark);

            int nRet = this.SendTemplateMsg(GzhCfg.C_Template_Borrow,
                bindWeixinIds,
                workers,
                msgData,
                maskMsgData,
                "",//linkurl
                theOperator,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }


        /// 还书成功
        /// <returns>
        /// -1 出错，格式出错或者发送模板消息出错
        /// 0 未绑定微信id
        /// 1 成功
        /// </returns>
        private int SendReturnMsg(XmlDocument bodyDom,
            string libName,
            List<string> bindWeixinIds,
            string fullPatronName,
            string markFullPatronName,
            List<TracingOnUser> workers,
            out string strError)
        {
            strError = "";
            /*
<type>还书成功</type>
<libraryCode>分馆1</libraryCode>
<operation>return</operation>
<action>return</action>
<borrowDate>Sun, 04 Sep 2016 15:22:02 +0800</borrowDate>
<borrowPeriod>31day</borrowPeriod>
<returningDate>Wed, 05 Oct 2016 12:00:00 +0800</returningDate>
<borrowOperator>jane</borrowOperator>
<itemBarcode>C1000</itemBarcode>
<readerBarcode>L1-001</readerBarcode>
<operator>jane</operator>
<operTime>Mon, 05 Sep 2016 01:58:21 +0800</operTime>
<clientAddress>::1</clientAddress>
<version>1.02</version>
<time></time>
<uid>7ae41585-cc60-47cf-b3f0-708ea77b5a8e</uid>
<patronRecord>
  <barcode>L1-001</barcode>
  <refID>84dbbde6-7aac-4912-9a16-18cfffef9697</refID>
  <readerType>本科</readerType>
  <createDate>Sun, 04 Sep 2016 00:00:00 +0800</createDate>
  <expireDate>Sun, 31 Dec 2017 00:00:00 +0800</expireDate>
  <name>延1</name>
  <hire expireDate="" period="" />
  <borrows></borrows>
  <email>weixinid:o4xvUviTxj2HbRqbQb9W2nMl4fGg</email>
  <libraryCode>分馆1</libraryCode>
</patronRecord>
<itemRecord>
  <barcode>C1000</barcode>
  <parent>3</parent>
  <refID>e4e4d608-0fd8-4fe9-ae66-52b0e9c6a796</refID>
  <state></state>
  <publishTime></publishTime>
  <location>分馆1/阅览室</location>
  <seller></seller>
  <source></source>
  <price></price>
  <bindingCost></bindingCost>
  <bookType>普通</bookType>
  <registerNo></registerNo>
  <comment></comment>
  <mergeComment></mergeComment>
  <batchNo></batchNo>
  <volume></volume>
  <accessNo></accessNo>
  <intact></intact>
  <binding />
  <operations>
    <operation name="create" time="Sun, 04 Sep 2016 14:05:45 +0800" operator="jane" />
  </operations>
  <summary>当我想睡的时候 [专著]  / (美)简·R. 霍华德文 ; (美)琳内·彻丽图 ; 林芳萍翻译. -- ISBN 978-7-5434-7754-4 (精装 ) : CNY27.80</summary>
</itemRecord>      
           */

            XmlNode root = bodyDom.DocumentElement;
            //<itemBarcode>0000001</itemBarcode>
            //<borrowDate>Sun, 22 May 2016 19:48:01 +0800</borrowDate>
            //<borrowPeriod>31day</borrowPeriod>
            //<returningDate>Wed, 22 Jun 2016 12:00:00 +0800</returningDate>

            // 册条码
            XmlNode nodeItemBarcode = root.SelectSingleNode("itemBarcode");
            if (nodeItemBarcode == null)
            {
                strError = "尚未定义<itemBarcode>节点";
                return -1;
            }
            string itemBarcode = DomUtil.GetNodeText(nodeItemBarcode);

            // 借书日期
            string borrowDate = "";
            XmlNode nodeBorrowDate = root.SelectSingleNode("borrowDate");
            if (nodeBorrowDate != null)
            {
                borrowDate = DomUtil.GetNodeText(nodeBorrowDate);
                borrowDate = DateTimeUtil.ToLocalTime(borrowDate, "yyyy/MM/dd");
            }

            // 期限
            string borrowPeriod = "";
            XmlNode nodeBorrowPeriod = root.SelectSingleNode("borrowPeriod");
            if (nodeBorrowPeriod != null)
            {
                borrowPeriod = DomUtil.GetNodeText(nodeBorrowPeriod);
                borrowPeriod = GetDisplayTimePeriodStringEx(borrowPeriod);
            }

            // 实际还书日期
            XmlNode nodeOperTime = root.SelectSingleNode("operTime");
            if (nodeOperTime == null)
            {
                strError = "尚未定义<operTime>节点";
                return -1;
            }
            string operTime = DomUtil.GetNodeText(nodeOperTime);
            operTime = DateTimeUtil.ToLocalTime(operTime, "yyyy/MM/dd");

            // 摘要
            XmlNode nodeSummary = root.SelectSingleNode("itemRecord/summary");
            if (nodeSummary == null)
            {
                strError = "尚未定义itemRecord/summary节点";
                return -1;
            }
            string summary = DomUtil.GetNodeText(nodeSummary);

            // 馆藏地
            string location = "";
            XmlNode nodeLocation = root.SelectSingleNode("itemRecord/location");
            if (nodeLocation != null)
                location = DomUtil.GetNodeText(nodeLocation);

            // 操作人 operator
            string theOperator = "";
            XmlNode nodeOperator = root.SelectSingleNode("operator");
            if (nodeOperator != null)
            {
                theOperator = DomUtil.GetNodeText(nodeOperator);
                //if (String.IsNullOrEmpty(theOperator) == false)
                //    theOperator = " 操作者：" + theOperator;
            }

            // 册条码完整表示 C001 图书馆/馆藏地
            string fullItemBarcode = this.GetFullItemBarcode(itemBarcode, libName, location);


            // 备注，检查是否有超期信息
            string remark = fullPatronName + "，感谢还书。";//"，感谢及时归还，欢迎继续借书。";
            XmlNodeList listOverdue = root.SelectNodes("patronRecord/overdues/overdue");
            if (listOverdue.Count > 0)
            {
                remark = fullPatronName + "，您有" + listOverdue.Count + "笔超期违约记录，请履行超期手续。";
            }

            //尊敬的读者，您已成功还书。
            //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00 
            //册条码号：C0000001
            //借书日期：2016-5-27
            //借阅期限：31天
            //还书日期：2016-6-27
            //谢谢您及时归还，欢迎再借。
            summary = this.GetShortSummary(summary);

            // 
            string first="▉▊▋▍▎▉▊▋▍▎▉▊▋▍▎";
            string first_color="#00008B";
            ReturnTemplateData msgData = new ReturnTemplateData(first,
                first_color,
                summary,
                fullItemBarcode,
                borrowDate,
                borrowPeriod,
                operTime,
                remark);
            remark = remark.Replace(fullPatronName, markFullPatronName);
            ReturnTemplateData maskMsgData = new ReturnTemplateData(first,
                first_color,
                summary,
                fullItemBarcode,
                borrowDate,
                borrowPeriod,
                operTime,
                remark);

            // 发送消息
            int nRet = this.SendTemplateMsg(GzhCfg.C_Template_Return,
                bindWeixinIds,
                workers,
                msgData,
                maskMsgData,
                "",//linkurl
                theOperator,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        /// 交费成功
        /// <returns>
        /// -1 出错，格式出错或者发送模板消息出错
        /// 0 未绑定微信id
        /// 1 成功
        /// </returns>
        private int SendPayMsg(XmlDocument bodyDom,
            string libName,
            List<string> bindWeixinIds,
            string fullPatronName,
            string markFullPatronName,
            List<TracingOnUser> workers,
            out string strError)
        {
            strError = "";
            /*
 <?xml version="1.0" encoding="utf-8"?>
<root>
    <type>交费</type>
    <libraryCode></libraryCode>
    <operation>amerce</operation>
    <action>amerce</action>
    <readerBarcode>R0000001</readerBarcode>
    <operator>supervisor</operator>
    <operTime>Sun, 22 May 2016 19:28:52 +0800</operTime>
    <clientAddress>::1</clientAddress>
    <version>1.02</version>
    <patronRecord>
        <barcode>R0000001</barcode>
        <readerType>本科生</readerType>
        <name>张三</name>
        <refID>be13ecc5-6a9c-4400-9453-a072c50cede1</refID>
        <department>/</department>
        <address>address</address>
        <cardNumber>C12345</cardNumber>
        <refid>8aa41a9a-fb42-48c0-b9b9-9d6656dbeb76</refid>
        <email>email:xietao@dp2003.com,weixinid:testwx2,testid:123456</email>
        <idCardNumber>1234567890123</idCardNumber>
        <tel>13641016400</tel>
        <libraryCode></libraryCode>
    </patronRecord>
    <items>
        <overdue barcode="0000001" summary=”…” reason="超期。超 335天; 违约金因子: CNY1.0/day" overduePeriod="335day" price="CNY335" borrowDate="Tue, 01 Dec 2015 14:09:33 +0800" borrowPeriod="31day" returnDate="Thu, 01 Dec 2016 14:09:52 +0800" borrowOperator="supervisor" operator="supervisor" id="635845758236835562-1" />
    </items>
</root>       
           */
            XmlNode root = bodyDom.DocumentElement;

            //交费时间
            XmlNode nodeOperTime = root.SelectSingleNode("operTime");
            if (nodeOperTime == null)
            {
                strError = "尚未定义<borrowDate>节点";
                return -1;
            }
            string operTime = DomUtil.GetNodeText(nodeOperTime);
            operTime = DateTimeUtil.ToLocalTime(operTime, "yyyy/MM/dd");

            // 操作人 operator
            string theOperator = "";
            XmlNode nodeOperator = root.SelectSingleNode("operator");
            if (nodeOperator != null)
            {
                theOperator = DomUtil.GetNodeText(nodeOperator);
                //if (String.IsNullOrEmpty(theOperator) == false)
                //    theOperator = " 操作者：" + theOperator;
            }

            // 备注
            string remark = "\n" + fullPatronName + "，您已成功交费。";// +this._msgRemark;

            XmlNodeList listOverdue = root.SelectNodes("items/overdue");
            foreach (XmlNode node in listOverdue)
            {
                string oneBarcode = DomUtil.GetAttr(node, "barcode");
                string price = DomUtil.GetAttr(node, "price");
                string summary = DomUtil.GetAttr(node, "summary");
                string reason = DomUtil.GetAttr(node, "reason");

                // todo,检查结构中是否有location
                string location = DomUtil.GetAttr(node, "location");
                string fullItemBarcode = this.GetFullItemBarcode(oneBarcode, libName, location);


                summary = this.GetShortSummary(summary);
                //尊敬的读者，您已成功交费。
                //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00
                //册条码号：C0000001
                //交费金额：CNY 10元
                //交费原因：超期。超1天，违约金因子：CNY1.0/Day
                //交费时间：2015-12-27 13:15
                //如有疑问，请联系系统管理员。
                string first = "💰💰💰💰💰💰💰💰💰💰";
                string first_color = "#556B2F";
                PayTemplateData msgData = new PayTemplateData(first,
                    first_color,
                    summary,
                    fullItemBarcode,
                    price,
                    reason,
                    operTime,
                    remark);
                
                //mask
                remark = remark.Replace(fullPatronName, markFullPatronName);
                PayTemplateData maskMsgData = new PayTemplateData(first,
                    first_color,
                    summary,
                    fullItemBarcode,
                    price,
                    reason,
                    operTime,
                    remark);

                // 发送消息
                int nRet = this.SendTemplateMsg(GzhCfg.C_Template_Pay,
                    bindWeixinIds,
                    workers,
                    msgData,
                    maskMsgData,
                    "",//linkurl
                    theOperator,
                    out strError);
                if (nRet == -1)
                    return -1;

            }

            return 0;
        }

        /// 撤消交费成功
        /// <returns>
        /// -1 出错，格式出错或者发送模板消息出错
        /// 0 未绑定微信id
        /// 1 成功
        /// </returns>
        private int SendCancelPayMsg(XmlDocument bodyDom,
            string libName,
            List<string> bindWeixinIds,
            string fullPatronName,
            string markFullPatronName,
            List<TracingOnUser> workers,
            out string strError)
        {
            strError = "";
            /*
 <?xml version="1.0" encoding="utf-8"?>
<root>
    <type>撤销交费</type>
    <libraryCode></libraryCode>
    <operation>amerce</operation>
    <action>undo</action>
    <readerBarcode>R0000001</readerBarcode>
    <operator>supervisor</operator>
    <operTime>Sun, 22 May 2016 19:15:54 +0800</operTime>
    <clientAddress>::1</clientAddress>
    <version>1.02</version>
    <patronRecord>
        <barcode>R0000001</barcode>
        <readerType>本科生</readerType>
        <name>张三</name>
        <refID>be13ecc5-6a9c-4400-9453-a072c50cede1</refID>
        <department>/</department>
        <address>address</address>
        <cardNumber>C12345</cardNumber>
        <overdues>
            <overdue barcode="0000001" reason="超期。超 335天; 违约金因子: CNY1.0/day" overduePeriod="335day" price="CNY335" borrowDate="Tue, 01 Dec 2015 14:09:33 +0800" borrowPeriod="31day" returnDate="Thu, 01 Dec 2016 14:09:52 +0800" borrowOperator="supervisor" operator="supervisor" id="635845758236835562-1" />
        </overdues>
        <refid>8aa41a9a-fb42-48c0-b9b9-9d6656dbeb76</refid>
        <email>email:xietao@dp2003.com,weixinid:testwx2,testid:123456</email>
        <idCardNumber>1234567890123</idCardNumber>
        <tel>13641016400</tel>
        <libraryCode></libraryCode>
    </patronRecord>
    <items>
        <overdue barcode="0000001" summary=”…” reason="超期。超 335天; 违约金因子: CNY1.0/day" overduePeriod="335day" price="CNY335" borrowDate="Tue, 01 Dec 2015 14:09:33 +0800" borrowPeriod="31day" returnDate="Thu, 01 Dec 2016 14:09:52 +0800" borrowOperator="supervisor" operator="supervisor" id="635845758236835562-1" />
    </items>
</root>     
           */
            XmlNode root = bodyDom.DocumentElement;

            //撤消时间
            XmlNode nodeOperTime = root.SelectSingleNode("operTime");
            if (nodeOperTime == null)
            {
                strError = "尚未定义<borrowDate>节点";
                return -1;
            }
            string operTime = DomUtil.GetNodeText(nodeOperTime);
            operTime = DateTimeUtil.ToLocalTime(operTime, "yyyy/MM/dd");

            // 操作人 operator
            string theOperator = "";
            XmlNode nodeOperator = root.SelectSingleNode("operator");
            if (nodeOperator != null)
            {
                theOperator = DomUtil.GetNodeText(nodeOperator);
                //if (String.IsNullOrEmpty(theOperator) == false)
                //    theOperator = " 操作者：" + theOperator;
            }

            // 备注
            string remark = "\n" + fullPatronName + "，您已成功撤消交费。";// +this._msgRemark;

            XmlNodeList listOverdue = root.SelectNodes("items/overdue");
            foreach (XmlNode node in listOverdue)
            {
                string oneBarcode = DomUtil.GetAttr(node, "barcode");
                string price = DomUtil.GetAttr(node, "price");
                string summary = DomUtil.GetAttr(node, "summary");
                string reason = DomUtil.GetAttr(node, "reason");

                // todo,检查结构中是否有location
                string location = DomUtil.GetAttr(node, "location");
                string fullItemBarcode = this.GetFullItemBarcode(oneBarcode, libName, location);

                summary = this.GetShortSummary(summary);
                //{{first.DATA}}
                //书刊摘要：{{keyword1.DATA}}
                //册条码号：{{keyword2.DATA}}
                //交费原因：{{keyword3.DATA}}
                //撤消金额：{{keyword4.DATA}}
                //撤消时间：{{keyword5.DATA}}
                //{{remark.DATA}}
                string first = "✈ ☁ ☁ ☁ ☁ ☁ ☁";
                string first_color = "#B8860B";
                CancelPayTemplateData msgData  = new CancelPayTemplateData(first,
                    first_color,
                    summary,
                    fullItemBarcode,
                    reason,
                    price,
                    operTime,
                    remark);
                //mask
                remark = remark.Replace(fullPatronName, markFullPatronName);
                CancelPayTemplateData maskMsgData  = new CancelPayTemplateData(first,
                    first_color,
                    summary,
                    fullItemBarcode,
                    reason,
                    price,
                    operTime,
                    remark);

                // 发送消息
                int nRet = this.SendTemplateMsg(GzhCfg.C_Template_CancelPay,
                    bindWeixinIds,
                    workers,
                    msgData,
                    maskMsgData,
                    "",//linkurl
                    theOperator,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            return 0;
        }


        /// <summary>
        /// 超期通知
        /// </summary>
        /// <param name="bodyDom"></param>
        private int SendCaoQi(XmlDocument bodyDom,
            string libName,
            List<string> bindWeixinIds,
            string fullPatronName,
            string markFullPatronName,
            List<TracingOnUser> workers,
            out string strError)
        {
            strError = "";
            /*
<root>
    <type>超期通知</type>
    <items overdueCount="1" normalCount="0">
        <item barcode="" refID="" 
             * summary="船舶柴油机 / 聂云超主编. -- ISBN 7-..." 
             * timeReturning="2016/5/18" 
             * overdue="已超期 31 天" 
             * overdueType="overdue" />
    </items>
    <text>您借阅的下列书刊：
船舶柴油机 / 聂云超主编. -- ISBN 7-... 应还日期: 2016/5/18 已超期 31 天
</text>
    <patronRecord>...
    </patronRecord>
</root>
            
//overdueType是超期类型，overdue表示超期，warning表示即将超期。             
           */
            XmlNode root = bodyDom.DocumentElement;

            // 取出册列表
            XmlNode nodeItems = root.SelectSingleNode("items");
            string overdueCount = DomUtil.GetAttr(nodeItems, "overdueCount");

            XmlNodeList nodeList = nodeItems.SelectNodes("item");
            // 一册一个通知
            foreach (XmlNode item in nodeList)
            {
                string summary = DomUtil.GetAttr(item, "summary");
                string timeReturning = DomUtil.GetAttr(item, "timeReturning");
                string overdue = DomUtil.GetAttr(item, "overdue");

                string barcode = DomUtil.GetAttr(item, "barcode");

                // 馆藏地 todo
                string location = DomUtil.GetAttr(item, "location");
                string fullItemBarcode = this.GetFullItemBarcode(barcode, libName, location);

                // 借书日期
                string borrowDate = DomUtil.GetAttr(item, "borrowDate");
                borrowDate = DateTimeUtil.ToLocalTime(borrowDate, "yyyy/MM/dd");

                //overdueType是超期类型，overdue表示超期，warning表示即将超期。
                string templateName = GzhCfg.C_Template_CaoQi;//this.Template_CaoQi;
                string overdueType = DomUtil.GetAttr(item, "overdueType");
                string remark = "";
                if (overdueType == "overdue")
                {
                    remark = "\n" + fullPatronName + "，您借出的图书已超期，请尽快归还。";
                }
                else if (overdueType == "warning")
                {
                    remark = "\n" + fullPatronName + "，您借出的图书即将到期，请注意不要超期，留意归还。";
                }
                else
                {
                    strError = "overdueType属性值[" + overdueType + "]不合法。";
                    return -1;//整个不处理 //continue;                    
                }

                summary = this.GetShortSummary(summary);
                //您好，您借出的图书已超期。
                //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00
                //册条码号：C0000001
                //借书日期：2016-07-01
                //应还日期：2016-07-31
                //超期情况：已超期30天
                //任延华，您借出的图书已超期，请尽快归还。
                string first = "📙📙📙📙📙📙📙📙📙📙";
                string first_color = "#FFFF00";
                CaoQiTemplateData msgData = new CaoQiTemplateData(first,
                    first_color,
                    summary,
                    fullItemBarcode,
                    borrowDate,
                    timeReturning,
                    overdue,
                    remark);

                //mask
                remark = remark.Replace(fullPatronName, markFullPatronName);
                CaoQiTemplateData maskMsgData = new CaoQiTemplateData(first,
                    first_color,
                    summary,
                    fullItemBarcode,
                    borrowDate,
                    timeReturning,
                    overdue,
                    remark);

                // 发送消息
                int nRet = this.SendTemplateMsg(GzhCfg.C_Template_CaoQi,
                    bindWeixinIds,
                    workers,
                    msgData,
                    maskMsgData,
                    "",//linkurl
                    "",//theOperator,
                    out strError);
                if (nRet == -1)
                    return -1;

            }
            return 0;
        }

        /// <summary>
        /// 预约到书通知
        /// </summary>
        /// <param name="bodyDom"></param>
        /// <param name="strError"></param>
        /// <returns>
        /// -1 出错，格式出错或者发送模板消息出错
        /// 0 未绑定微信id
        /// 1 成功
        /// </returns>
        private int SendArrived(XmlDocument bodyDom,
            string libName,
            List<string> bindWeixinIds,
            string fullPatronName,
            string markFullPatronName,
            List<TracingOnUser> workers,
            out string strError)
        {
            strError = "";
            /*
           body元素里面是预约到书通知记录(注意这是一个字符串，需要另行装入一个XmlDocument解析)，其格式如下：
           <?xml version="1.0" encoding="utf-8"?>
 <root>
    <type>预约到书通知</type>
    <itemBarcode>0000001</itemBarcode>
    <refID> </refID>
    <onShelf>false</onShelf>
    <opacURL>/book.aspx?barcode=0000001</opacURL>
    <reserveTime>2天</reserveTime>
    <today>2016/5/17 10:10:59</today>
    <summary>船舶柴油机 / 聂云超主编. -- ISBN 7-...</summary>
    <patronName>张三</patronName>
    <patronRecord>
        <barcode>R0000001</barcode>
        <readerType>本科生</readerType>
        <name>张三</name>
        <refID>be13ecc5-6a9c-4400-9453-a072c50cede1</refID>
        <department>数学系</department>
        <address>address</address>
        <cardNumber>C12345</cardNumber>
        <refid>8aa41a9a-fb42-48c0-b9b9-9d6656dbeb76</refid>
        <email>email:xietao@dp2003.com,weixinid:testwx2</email>
        <tel>13641016400</tel>
        <idCardNumber>1234567890123</idCardNumber>
    </patronRecord>
</root>
           */
            XmlNode root = bodyDom.DocumentElement;
            //<onShelf>false</onShelf>
            // <reserveTime>2天</reserveTime>
            // <today>2016/5/17 10:10:59</today>

            // 摘要
            XmlNode nodeSummary = root.SelectSingleNode("summary");
            if (nodeSummary == null)
            {
                strError = "尚未定义<summary>节点";
                return -1;
            }
            string summary = DomUtil.GetNodeText(nodeSummary);

            //<accessNo>
            XmlNode accessNoToday = root.SelectSingleNode("accessNo");
            if (accessNoToday == null)
            {
                strError = "尚未定义<accessNo>节点";
                return -1;
            }
            string accessNo = DomUtil.GetNodeText(accessNoToday);

            // 到书日期
            XmlNode nodeToday = root.SelectSingleNode("today");
            if (nodeToday == null)
            {
                strError = "尚未定义<today>节点";
                return -1;
            }
            string today = DomUtil.GetNodeText(nodeToday);

            //保留期限
            XmlNode nodeReserveTime = root.SelectSingleNode("reserveTime");
            if (nodeReserveTime == null)
            {
                strError = "尚未定义<reserveTime>节点";
                return -1;
            }
            string reserveTime = DomUtil.GetNodeText(nodeReserveTime);//2天

            // 计算保留至日期
            string toDate = "";
            try
            {
                DateTime tempDate = DateTime.Parse(today);
                if (reserveTime.Contains("天") == true)
                {
                    int day = Convert.ToInt32(reserveTime.Replace("天", ""));
                    if (day > 0)
                    {
                        tempDate = tempDate.AddDays(day);
                        toDate = tempDate.ToShortDateString();
                    }
                }
                else if (reserveTime.Contains("小时") == true)
                {
                    int hour = Convert.ToInt32(reserveTime.Replace("小时", ""));
                    if (hour > 0)
                    {
                        tempDate = tempDate.AddHours(hour);
                        toDate = tempDate.ToShortDateString();
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteErrorLog1("计算保留至的日期出错：" + ex.Message);
            }

            reserveTime = "保留" + reserveTime;

            if (toDate != "")
            {
                reserveTime += "(至" + toDate + ")";
            }

            // 是否在架
            XmlNode nodeOnShelf = root.SelectSingleNode("onShelf");
            if (nodeOnShelf == null)
            {
                strError = "尚未定义<onShelf>节点";
                return -1;
            }
            string onShelf = DomUtil.GetNodeText(nodeOnShelf);
            bool bOnShelf = false;
            if (onShelf == "true")
                bOnShelf = true;

            // 册条码
            string itemBarcode = "";
            XmlNode nodeItemBarcode = root.SelectSingleNode("itemBarcode");
            if (nodeItemBarcode != null)
            {
                itemBarcode = DomUtil.GetNodeText(nodeItemBarcode);
            }
            if (itemBarcode == "")
            {
                XmlNode nodeRefId = root.SelectSingleNode("refID");
                if (nodeRefId != null)
                {
                    itemBarcode = DomUtil.GetNodeText(nodeRefId);
                }
            }

            //reserve
            string requestDate = "";
            XmlNode nodeRequestDate = root.SelectSingleNode("requestDate");
            if (nodeRequestDate != null)
            {
                requestDate = DomUtil.GetNodeText(nodeRequestDate);
                requestDate = DateTimeUtil.ToLocalTime(requestDate, "yyyy/MM/dd");
            }


            // 馆藏地
            string location = "";
            XmlNode nodeLocation = root.SelectSingleNode("location");
            if (nodeLocation != null)
            {
                location = DomUtil.GetNodeText(nodeLocation);
            }
            string fullItemBarcode = this.GetFullItemBarcode(itemBarcode, libName, location);

            // 备注
            string remark = fullPatronName + "，您预约的图书 " + fullItemBarcode + " 到了，请尽快来图书馆办理借书手续。";//如果您未能在保留期限内来馆办理借阅手续，图书馆将把优先借阅权转给后面排队等待的预约者，或做归架处理。";
            if (bOnShelf == true)
            {
                remark = fullPatronName + "，您预约的图书 " + fullItemBarcode + " 已经在架上，请尽快来图书馆办理借书手续。";//如果您未能在保留期限内来馆办理借阅手续，图书馆将把优先借阅权转给后面排队等待的预约者，或允许其他读者借阅。";
            }
            if (string.IsNullOrEmpty(accessNo) == false)
            {
                remark += "索取号" + accessNo;
            }

            summary = this.GetShortSummary(summary);
            //您好，您预约的图书已经到书。
            //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00
            //册条码号：C00001
            //预约日期：2016-08-15
            //到书日期：2016-09-05
            //保留期限：2016-09-07（保留2天）
            //XXX，您预约的图书到了，请尽快来图书馆办理借书手续，请尽快来图书馆办理借书手续。
            //如果您未能在保留期限内来馆办理借阅手续，图书馆将把优先借阅权转给后面排队等待的预约者，或做归架处理。
            string first = "📗📗📗📗📗📗📗📗📗📗";
            string first_color = "#FF8C00";
            ArrivedTemplateData msgData = new ArrivedTemplateData(first,
                first_color,
                summary,
                fullItemBarcode,
                requestDate,
                today,
                reserveTime,
                remark);
            //mask
            remark = remark.Replace(fullPatronName, markFullPatronName);
            ArrivedTemplateData maskMsgData = new ArrivedTemplateData(first,
                first_color,
                summary,
                fullItemBarcode,
                requestDate,
                today,
                reserveTime,
                remark);

            // 发送消息
            int nRet = this.SendTemplateMsg(GzhCfg.C_Template_Arrived,
                bindWeixinIds,
                workers,
                msgData,
                maskMsgData,
                "",//linkurl
                "",//theOperator,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }



        #endregion

        #region 短信接口

        public List<MessageInterface> m_externalMessageInterfaces = null;

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
        public int InitialExternalMessageInterfaces(XmlDocument dom, out string strError)
        {
            strError = "";

            this.m_externalMessageInterfaces = null;
            XmlNode root = dom.DocumentElement.SelectSingleNode("externalMessageInterface");
            if (root == null)
            {
                strError = "在weixin.xml中没有找到<externalMessageInterface>元素";
                return 0;
            }

            this.m_externalMessageInterfaces = new List<MessageInterface>();

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

                message_interface.HostObj.App = this;

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

        #endregion

        #region 检查不在线图书馆

        // 不在线图书馆
        public Hashtable _offlineLibs = new Hashtable();

        /// <summary>
        /// 获取不在线的图书馆,包含2类
        /// 1是由于版本低挂起状态的图书馆
        /// 2是版本正常，真不在线的图书馆
        /// </summary>
        /// <returns></returns>
        public List<Library> GetOfflineLibs()
        {
            List<Library> offlineLibs = new List<Library>();
            string strError = "";

            List<Library> libs = dp2WeiXinService.Instance.LibManager.Librarys; //LibDatabase.Current.GetLibs();  // 20161016 jane 这里应该用内存的数据库集合是好了 todo
            foreach (Library lib in libs)
            {
                // 可以图书馆的备注信息设为
                if (lib.Entity.comment == "notwarn") // 任延华测试用的图书馆
                    continue;

                // 如果是挂起状态，直接加入不在线列表
                if (lib.State == LibraryManager.C_State_Hangup)
                {
                    offlineLibs.Add(lib);
                    continue;
                }

                LibEntity libEntity = lib.Entity;
                try
                {
                    // 使用代理账号capo 20161024 jane
                    LoginInfo loginInfo = new LoginInfo("", false);

                    CancellationToken cancel_token = new CancellationToken();
                    string id = Guid.NewGuid().ToString();
                    SearchRequest request = new SearchRequest(id,
                        loginInfo,
                        "getSystemParameter",
                        "",
                        "_clock",//"cfgs",//queryWord,
                        "",
                        "",
                        "",
                        "",//"getDataDir",//formatList,//
                        1,
                        0,
                        -1);
                    /*2016/10/16
                    光光(2820725526) 20:59:53
                    我看你的代码中 GetOfflineLibs() 函数中，有这么一段：
                                        MessageConnection connection = this._channels.GetConnectionTaskAsync(
                                            this.dp2MServerUrl,
                                            lib.capoUserName).Result;
                    你在获得 Connection 的时候，用的名字字符串是“对方账户名”。这是出于什么考虑呢？
                    光光(2820725526) 21:05:59
                    提醒一下，你检测若干对方 dp2library 连通状态的过程，是个循环。每一次做完再做下一个。而且是十分钟一次循环。使用频率不是很高，对对方服务器发起请求不存在互相重叠。这样，用一根通道就可以了。也就是说，甚至可以在循环外面 GetConnection() ，然后在循环里面集中使用这一根通道。
                    光光(2820725526) 21:06:53
                    那么 GetConnection() 时候要获得的通道的名字，就不必和对方或者本方账号名字挂钩了，用个死名字即可。
                    光光(2820725526) 21:08:19
                    打个比方，你和对方一个屋子里面的人循序讲话，你接通电话以后，他们一个一个顺序过来和你对话即可，不必用多个电话机。
                    光光(2820725526) 21:10:41
                    这里屋子的比方，就是 dp2mserver。你把请求到达 dp2mserver 即可，它会通过你给 SearchTaskAsync() 的 strRemoteUser 参数分发。你这一侧的通道不必用多条。
                    光光(2820725526) 21:13:28
                    记住，无论你和远端多少 dp2Capo 联系，都是中间通过 dp2mserver 分发的。有时候我们在微信公众号这一侧启用多根通道(通过名字参数区分)，是为了并发，或者其他需要。并不是意味着微信公众号和 dp2Capo “直连”了。
                    */
                    MessageConnection connection = this._channels.GetConnectionTaskAsync(
                        this.dp2MServerUrl,
                        C_ConnName_CheckOfflineLibs).Result;//lib.capoUserName 2016/10/16 jane 改为一根通道了

                    SearchResult result = connection.SearchTaskAsync(
                        libEntity.capoUserName,
                        request,
                        new TimeSpan(0, 1, 0),
                        cancel_token).Result;
                    if (result.ResultCount == -1)
                    {
                        if (result.ErrorCode == "TargetNotFound")
                        {
                            // 总是漏掉江西警察学校，这里输出日志看看。
                            this.WriteLog2("检查 " + libEntity.libName + " 为离线状态。");

                            offlineLibs.Add(lib);
                            continue;
                        }

                        // 注意这里不要用GetFriendlyErrorInfo
                        strError = result.ErrorInfo;
                        goto ERROR1;
                    }

                    // 总是漏掉江西警察学校，这里输出日志看看。
                    this.WriteLog2("检查 " + libEntity.libName + " 为在线状态。");
                }
                catch (AggregateException ex)
                {
                    strError = MessageConnection.GetExceptionText(ex);
                    goto ERROR1;

                }
                catch (Exception ex)
                {
                    strError = ex.Message;
                    goto ERROR1;
                }

                Thread.Sleep(1000);
                continue;


            ERROR1:
                //将错误写到日志里，继续检索其它图书馆
                strError = "检查图书馆 " + libEntity.libName + " 是否在线出错: " + strError;
                WriteErrorLog1(strError);

            }


            return offlineLibs;
        }


        /// <summary>
        /// 对在线图书馆，给该图书馆的管理员 和数字平台管理员发通知
        /// </summary>
        public void WarnOfflineLib()
        {
            string strError = "";
            try
            {
                // 本次不在线的图书馆
                List<Library> thisOfflineLibs = this.GetOfflineLibs();
                this.WriteLog2("本轮检查有" + thisOfflineLibs.Count + "个不在线图书馆");

                // 整理缓存中不在线的图书馆
                if (thisOfflineLibs.Count == 0)
                {
                    this.WriteLog2("检查没有发现不在线的图书馆");

                    // 清除内存中保存的不在线图书馆
                    this._offlineLibs.Clear();

                    this.WriteLog2("清除内存中不在线图书馆");
                }
                else
                {
                    // 已经恢复在线的图书馆，要从内存不在线图书馆中删除 
                    List<string> removeIds = new List<string>();
                    foreach (string libid in this._offlineLibs.Keys)
                    {
                        bool bFound = false;
                        foreach (Library lib in thisOfflineLibs)
                        {
                            if (libid == lib.Entity.id)
                            {
                                bFound = true;
                                break;
                            }
                        }
                        // 在本次的未在线图书馆中不出现，则删除内存中的
                        if (bFound == false)
                        {
                            this.WriteLog2("图书馆 " + libid + " 恢复在线");
                            removeIds.Add(libid);
                        }
                    }

                    // 移除已经连线的图书馆
                    foreach (string oneId in removeIds)
                    {
                        this.WriteLog2("图书馆 " + oneId + " 从内存不在线图书馆移除。");
                        this._offlineLibs.Remove(oneId);
                    }
                }

                // 处理本次的未在线图书馆，得到需要发通知的图书馆
                List<Library> warningLibs = new List<Library>();
                if (thisOfflineLibs.Count > 0)
                {
                    DateTime now = DateTime.Now;
                    TimeSpan delta = new TimeSpan(1, 0, 0);
                    foreach (Library lib in thisOfflineLibs)
                    {
                        if (this._offlineLibs.ContainsKey(lib.Entity.id) == false)
                        {
                            this.WriteLog2("不在线图书馆 " + lib.Entity.libName + " 本来不在内存中，加到发通知列表");
                            warningLibs.Add(lib);
                        }
                        else
                        {
                            // 检查上次发通知时间,超过一小时，继续通知
                            DateTime lastTime = (DateTime)this._offlineLibs[lib.Entity.id];
                            if (now - lastTime > delta) //2016/9/25 jane 改bug 比较的2个时间写反了 lastTime-now
                            {
                                warningLibs.Add(lib);
                                this.WriteLog2("不在线图书馆 " + lib.Entity.libName + " 本来在内存中，上次发送时间超过1小时，需再次发通知，加到发通知列表中。");
                            }
                            else
                            {
                                this.WriteLog2("不在线图书馆 " + lib.Entity.libName + " 本来在内存中，上次发送时间小于1小时，此轮不发通知。");

                            }
                        }
                    }
                }

                //===给图书馆发通知=====

                // 数据平台工作人员 weixinid
                List<WxUserItem> dp2003Workers = new List<WxUserItem>();
                if (warningLibs.Count > 0)
                {
                    dp2003Workers = this.GetDp2003WorkerWeixinIds();
                    WriteLog2("找到 " + dp2003Workers.Count.ToString() + " 位数字平台工作人员");
                }

                foreach (Library lib in warningLibs)
                {
                    this.WriteLog2("准备发送 " + lib.Entity.libName + " 不在线通知");

                    // 图书馆工作人员 weixinid
                    List<WxUserItem> libWorkers = this.getWarningWorkerWeixinIds(lib.Entity);
                    WriteLog2("找到 " + libWorkers.Count.ToString() + " 位图书馆 " + lib.Entity.libName + " 工作人员");

                    // 当工作人员与数字平台都没有 可收警告的工作人员，则不再发送通知
                    if (libWorkers.Count == 0 && dp2003Workers.Count == 0)
                    {
                        this.WriteLog2("没有可接收到工作人员");
                        continue;
                    }

                    string libName = lib.Entity.libName;

                    //{{first.DATA}}
                    //标题：{{keyword1.DATA}}
                    //时间：{{keyword2.DATA}}
                    //内容：{{keyword3.DATA}}
                    //{{remark.DATA}}
                    //图书馆桥接服务器失去连接
                    //2016-9-8 15:04
                    //***，贵图书馆 XXX 桥接服务器失去连接，请及时修复，以免影响读者访问。
                    string title = libName + " 桥接服务器dp2capo失去连接";

                    string operTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                    string text = libName + " 的桥接服务器dp2capo已失去连接，请尽快修复。";

                    if (lib.State == LibraryManager.C_State_Hangup)
                    {
                        if (lib.Version == "-1")
                        {
                            title = libName + " 桥接服务器被挂起";
                            text = libName + " 的桥接服务器dp2capo已失去连接，公众号功能已被挂起，请尽快修复。";
                        }
                        else
                        {
                            title = libName + " 桥接服务器被挂起";
                            text = libName + " 的桥接服务器dp2capo版本不够新，公众号功能已被挂起，请尽快升级。";
                        }
                    }

                    string first = "☀☀☀☀☀☀☀☀☀☀";
                    string first_color = "#9400D3";
                    MessageTemplateData msgData = new MessageTemplateData(first,
                        first_color,
                        title,
                        operTime,
                        text,
                        "");

                    foreach (WxUserItem worker in libWorkers)
                    {
                        if (worker.weixinId == C_Supervisor)
                            continue;

                        string tempText = worker.userName + "，贵馆 " + text;
                        msgData.keyword3 = new TemplateDataItem(tempText, "#000000");

                        List<string> ids = new List<string>();
                        string fullWeixinId = worker.weixinId;//2016-11-16 weixinId带了@appId //+ "@" + worker.appId;
                        ids.Add(fullWeixinId);

                        int nRet = this.SendTemplateMsgInternal(ids,
                            GzhCfg.C_Template_Message,//this.Template_Message,
                             msgData,
                             "",
                             "",// theOperator,
                             out strError);
                        if (nRet == -1)
                        {
                            WriteErrorLog1("给工作人员 " + worker.userName + " 发送图书馆不在线通知出错：" + strError);
                            continue;  //goto ERROR1;                            
                        }

                        WriteLog2("给图书馆工作人员 " + worker.userName + " 发送图书馆 " + libName + " 不在线通知完成");

                    }

                    // 给数字平台工作人员发通知
                    foreach (WxUserItem worker in dp2003Workers)
                    {
                        msgData.keyword3 = new TemplateDataItem(text, "#000000");

                        List<string> ids = new List<string>();
                        string fullWeixinId = worker.weixinId;//2016-11-16 weixinId带了@appId // + "@" + worker.appId;

                        ids.Add(fullWeixinId);
                        int nRet = this.SendTemplateMsgInternal(ids,
                            GzhCfg.C_Template_Message,//this.Template_Message,
                             msgData,
                             "",
                             "",// theOperator,
                             out strError);
                        if (nRet == -1)
                        {
                            WriteErrorLog1(strError);
                            continue;  //goto ERROR1;                            
                        }

                        WriteLog2("给数字平台工作人员 " + worker.userName + " 发送图书馆 " + libName + " 不在线通知完成");
                    }

                    // 加到内存中
                    this._offlineLibs[lib.Entity.id] = DateTime.Now;
                    this.WriteLog2("记下图书馆 " + libName + " 最后发送通知时间" + DateTimeUtil.DateTimeToString(((DateTime)this._offlineLibs[lib.Entity.id])));
                }

                // 返回
                return;

            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

        ERROR1:
            this.WriteErrorLog1(strError);
        }




        /// <summary>
        /// 得到数字平台工作人员的weixin id
        /// </summary>
        /// <returns></returns>
        public List<WxUserItem> GetDp2003WorkerWeixinIds()
        {
            LibEntity lib = LibDatabase.Current.GetLibByName(WeiXinConst.C_Dp2003LibName);
            return this.getWarningWorkerWeixinIds(lib);
        }

        public List<WxUserItem> getWarningWorkerWeixinIds(LibEntity lib)
        {
            List<WxUserItem> adminWeixinIds = new List<WxUserItem>();
            if (lib != null)
            {
                List<WxUserItem> workers = WxUserDatabase.Current.Get("", lib.id, WxUserDatabase.C_Type_Worker);

                // 如果有工作人员配了 接收警告 权限，将只返回有权限的工作人员
                foreach (WxUserItem user in workers)
                {
                    bool hasRight = CheckContainRights(user.rights, dp2WeiXinService.C_Right_GetWarning);
                    if (hasRight == true)
                    {
                        adminWeixinIds.Add(user);
                    }
                }

                /*
                2016-10-20 任延华
                修改起因：南开徐老师被不在线通知烦到了，提出改进这个功能，少给图书馆工作人员发通知。
                修改为：如果图书馆工作人员都没有设_wx_getWarning，就不通知了图书馆人员了，只通知数字平台人员。

                // 如果没有一个人配置 接收警告信息 的权限，则发给所有的工作人员
                if (adminWeixinIds.Count == 0)
                {
                    foreach (WxUserItem user in workers)
                    {
                        adminWeixinIds.Add(user);
                    }
                }
                 */
            }

            return adminWeixinIds;
        }

        #endregion

        #region 错误友好提示

        /// <summary>
        /// 提示信息
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public static string GetLinkHtml(string menu, string returnUrl, string libName = "")
        {
            return GetLinkHtml(menu, returnUrl, false, libName);
        }

        public static string GetLinkHtml(string menu, string returnUrl, bool bNeedWorker, string libName = "")
        {
            string name = "读者";
            string action = "查看";
            if (bNeedWorker == true)
            {
                name = "工作人员";
                action = "使用";
            }

            string bindUrl = "/Account/Bind?returnUrl=" + HttpUtility.UrlEncode(returnUrl);
            string bindLink = "请先点击<a href='javascript:void(0)' onclick='gotoUrl(\"" + bindUrl + "\")'>这里</a>进行绑定。";
            string strRedirectInfo = "您尚未绑定当前图书馆的" + name + "账户，不能" + action + menu + "，" + bindLink;

            if (menu == "书目查询" || menu == "好书推荐")
                strRedirectInfo = libName + " 不对外公开书目，您需要先<a href='javascript:void(0)' onclick='gotoUrl(\"" + bindUrl + "\")'>绑定图书馆账户</a>，才能进入" + menu;

            strRedirectInfo = "<div class='mui-content-padded' style='color:#666666'>"
                + strRedirectInfo
                + "</div>";
            return strRedirectInfo;
        }

        /// <summary>
        /// 得到友好的提示
        /// </summary>
        /// <returns></returns>
        private string GetFriendlyErrorInfo(MessageResult result, string libName)
        {
            if (result.String == "TargetNotFound")
            {

                // 2016/9/30 在需要的地方再激活吧，放在这里都受影响了。// 激活后面处理线程，可以给工作人员发通知
                //this._managerThread.Activate();

                if (string.IsNullOrEmpty(libName) == true)
                    return "图书馆 " + libName + " 的桥接服务器失去连接，无法访问。";

                return "图书馆 " + libName + " 的桥接服务器失去连接，无法访问。";
            }

            return result.ErrorInfo;
        }

        //SearchRequest
        private string GetFriendlyErrorInfo(SearchResult result, string libName, out bool bOffline)
        {
            bOffline = false;
            if (result.ErrorCode == "TargetNotFound")
            {
                // 2016/9/30 在需要的地方再激活吧，放在这里都受影响了。//激活后面处理线程，可以给工作人员发通知
                //this._managerThread.Activate();
                bOffline = true;

                if (String.IsNullOrEmpty(libName) == true)
                {
                    return "TargetNotFound";
                }

                return "图书馆 " + libName + " 的桥接服务器失去连接，无法访问。";
            }

            return result.ErrorInfo;
        }

        #endregion

        #region 找回密码，修改密码，二维码

        /// <summary>
        /// 更新用户设置
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libId"></param>
        /// <param name="bookSubject"></param>
        public void UpdateUserSetting(string weixinId, string libId, string bookSubject, bool bCheckActiveUser, string patronRefID)
        {
            if (bookSubject == null)
                bookSubject = "";

            UserSettingItem settingItem = UserSettingDb.Current.GetByWeixinId(weixinId);
            if (settingItem == null)
            {
                settingItem = new UserSettingItem();
                settingItem.weixinId = weixinId;
                settingItem.libId = libId;
                settingItem.showCover = 1;
                settingItem.showPhoto = 1;

                if (patronRefID == null)
                    patronRefID = "";
                settingItem.patronRefID = patronRefID;

                settingItem.xml = "<root><subject book='" + bookSubject + "'/></root>";
                UserSettingDb.Current.Add(settingItem);
            }
            else
            {
                if (string.IsNullOrEmpty(libId) == false)
                {
                    if (settingItem.libId != libId)
                    {
                        settingItem.libId = libId;
                    }
                }

                if (string.IsNullOrEmpty(bookSubject) == false)
                {
                    // todo 要先取出xml，根据group更新subject
                    settingItem.xml = "<root><subject book='" + bookSubject + "'/></root>";
                }

                if (String.IsNullOrEmpty(patronRefID) == false)
                {
                    settingItem.patronRefID = patronRefID;
                }

                UserSettingDb.Current.Update(settingItem);

            }

            if (bCheckActiveUser == true)
            {
                //检查微信用户对于该馆是否设置了活动账户
                this.CheckUserActivePatron(weixinId, libId);
            }
        }

        /// <summary>
        /// 检查微信用户对于该馆是否设置了活动账户
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libId"></param>
        public void CheckUserActivePatron(string weixinId, string libId)
        {
            // 检查该图是否绑定了读者账户
            List<WxUserItem> users = WxUserDatabase.Current.GetPatron(weixinId, libId, null);
            if (users != null && users.Count > 0)
            {
                // 检查当前有没有激活账户
                bool isActive = false;
                foreach (WxUserItem user in users)
                {
                    if (user.isActive == 1)
                    {
                        isActive = true;
                        break;
                    }
                }

                // 没有激活的，激活第一人
                if (isActive == false)
                {
                    WxUserItem userItem = users[0];
                    WxUserDatabase.Current.SetActivePatron(userItem.weixinId, userItem.id);
                }
            }
            else
            {
                // 没有绑定该图书馆读者账户，全部将该微信用户对应的他馆账户置为未激活状态
                WxUserDatabase.Current.SetNoActivePatron(weixinId);
            }
        }

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="strLibraryCode"></param>
        /// <param name="name"></param>
        /// <param name="tel"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int ResetPassword(string weixinId,
            string libId,
            string name,
            string tel,
            out string patronBarcode,
            out string strError)
        {
            strError = "";
            patronBarcode = "";

            LibEntity lib = this.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                return -1;
            }

            string resultXml = "";
            string patronParam = "style=returnMessage,"
                + "queryword=NB:" + name + "|,"
                + "tel=" + tel + ","
                + "name=" + name;

            // 使用代理账号capo 20161024 jane
            LoginInfo loginInfo = new LoginInfo("", false);

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            CirculationRequest request = new CirculationRequest(id,
                loginInfo,
                "resetPassword",
                patronParam,
                "",//this.textBox_circulation_item.Text,
                "",//this.textBox_circulation_style.Text,
                "",//this.textBox_circulation_patronFormatList.Text,
                "",//this.textBox_circulation_itemFormatList.Text,
                "");//this.textBox_circulation_biblioFormatList.Text);
            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;
                CirculationResult result = connection.CirculationTaskAsync(
                    lib.capoUserName,
                    request,
                    new TimeSpan(0, 1, 10), // 10 秒
                    cancel_token).Result;
                if (result.Value == -1)
                {
                    //strError = "操作失败：" + result.ErrorInfo;
                    //return -1;

                    strError = this.GetFriendlyErrorInfo(result, lib.libName); //result.ErrorInfo;
                    return -1;
                }

                if (result.Value == 0)
                {
                    if (result.String == "NotFound")
                    {
                        strError = "操作失败：读者 " + name + " 在图书馆系统中未设置手机号码，请先到图书馆出纳台由工作人员帮助登记手机号码。";
                    }
                    else
                    {
                        if (result.ErrorInfo.IndexOf("不存在") != -1)
                            strError = "操作失败：读者姓名和手机号 与 图书馆系统中存储的姓名和手机号不一致。";
                        else
                            strError = "操作失败：" + result.ErrorInfo;//读者姓名和手机号 与 图书馆系统中存储的姓名和手机号不一致。";//
                    }

                    return 0;
                }

                resultXml = result.PatronBarcode;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

            // 2016-8-13 jane 自动修改设置的图书馆
            this.UpdateUserSetting(weixinId, libId, "", true, null);


            // 发送短信
            string strMessageTemplate = "";
            MessageInterface external_interface = this.GetMessageInterface("sms");
            if (string.IsNullOrEmpty(strMessageTemplate) == true)
            {
                //strMessageTemplate = "%name% 您好！\n您的读者帐户(证条码号为 %barcode%)已设临时密码 %temppassword%，在 %period% 内登录会成为正式密码";
                strMessageTemplate = "%name% 您好！密码为 %temppassword%。一小时内有效。";
            }
            /*
                                    DomUtil.SetElementText(node, "tel", strTelParam);
                                    DomUtil.SetElementText(node, "barcode", strBarcode);
                                    DomUtil.SetElementText(node, "name", strName);
                                    DomUtil.SetElementText(node, "tempPassword", strReaderTempPassword);
                                    DomUtil.SetElementText(node, "expireTime", expire.ToLongTimeString());
                                    DomUtil.SetElementText(node, "period", "一小时");
                                    DomUtil.SetElementText(node, "refID", strRefID); 
             */
            /*
<root>
  <patron>
    <tel>13862157150</tel>
    <barcode>R00001</barcode>
    <name>任延华</name>
    <tempPassword>586284</tempPassword>
    <expireTime>13:24:57</expireTime>
    <period>一小时</period>
    <refID>63aeb890-8936-4471-bfc5-8e72d5c7fe94</refID>
  </patron>
</root>             
             */
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(resultXml);
            XmlNode nodePatron = dom.DocumentElement.SelectSingleNode("patron");
            string strRefID = DomUtil.GetNodeText(nodePatron.SelectSingleNode("refID"));
            string strTel = DomUtil.GetNodeText(nodePatron.SelectSingleNode("tel"));

            string strBarcode = DomUtil.GetNodeText(nodePatron.SelectSingleNode("barcode"));
            patronBarcode = strBarcode;//2016-8-13 jane加，返回给前端，用于指定修改密码的账户
            string strName = DomUtil.GetNodeText(nodePatron.SelectSingleNode("name"));
            string strReaderTempPassword = DomUtil.GetNodeText(nodePatron.SelectSingleNode("tempPassword"));
            string expireTime = DomUtil.GetNodeText(nodePatron.SelectSingleNode("expireTime"));
            string period = DomUtil.GetNodeText(nodePatron.SelectSingleNode("period"));

            string strBody = strMessageTemplate.Replace("%barcode%", strBarcode)
                .Replace("%name%", strName)
                .Replace("%temppassword%", strReaderTempPassword)
                .Replace("%expiretime%", expireTime)
                .Replace("%period%", period);
            // string strBody = "读者(证条码号) " + strBarcode + " 的帐户密码已经被重设为 " + strReaderNewPassword + "";
            int nRet = 0;
            // 向手机号码发送短信
            {
                // 得到高级xml
                string strXml = "<root><tel>" + strTel + "</tel></root>";
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
                        strBarcode,
                        strXml,
                        strBody,
                        lib.libName, //todo,注意这里原来传的code 还是读者的libraryCode
                        out strError);
                    if (nRet == -1 || nRet == 0)
                        return nRet;
                }
                catch (Exception ex)
                {
                    strError = external_interface.Type + " 类型的外部消息接口Assembly中SendMessage()函数抛出异常: " + ex.Message;
                    nRet = -1;
                }
                if (nRet == -1)
                {
                    strError = "向读者 '" + strBarcode + "' 发送" + external_interface.Type + " message时出错：" + strError;

                    this.WriteErrorLog1(strError);
                    return -1;
                }
            }

            return 1;

        ERROR1:
            return -1;
        }




        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="patron"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <param name="strError"></param>
        /// <returns>
        /// -1  出错
        /// 0   未成功
        /// 1   成功
        /// </returns>
        public int ChangePassword(string libId,
            string patron,
            string oldPassword,
            string newPassword,
            out string strError)
        {
            strError = "";
            LibEntity lib = this.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                goto ERROR1;
            }

            // 注意新旧两个密码参数即便为空，也应该是 "" 而不应该是 null。
            // 所以在使用 Item 参数的时候，old 和 new 两个子参数的任何一个都不该被省略。
            // 省略子参数的用法是有意义的，但不该被用在这个修改读者密码的场合。
            string item = "old=" + oldPassword + ",new=" + newPassword;

            // 使用读者账号 20161024 jane
            LoginInfo loginInfo = new LoginInfo(patron, true);

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            CirculationRequest request = new CirculationRequest(id,
                loginInfo,
                "changePassword",
                patron,
                item,
                "",//this.textBox_circulation_style.Text,
                "",//this.textBox_circulation_patronFormatList.Text,
                "",//this.textBox_circulation_itemFormatList.Text,
                "");//this.textBox_circulation_biblioFormatList.Text);
            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;
                CirculationResult result = connection.CirculationTaskAsync(
                    lib.capoUserName,
                    request,
                    new TimeSpan(0, 1, 10), // 10 秒
                    cancel_token).Result;
                if (result.Value == -1)
                {
                    strError = "出错：" + result.ErrorInfo;
                    return -1;
                }

                if (result.Value == 0)
                {
                    strError = result.ErrorInfo;
                    return 0;
                }

                return (int)result.Value;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }


        ERROR1:
            return -1;
        }



        #endregion

        #region 绑定解绑
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strBarcode"></param>
        /// <param name="strPassword"></param>
        /// <param name="weixinId"></param>
        /// <returns>
        /// -1 出错
        /// 0 成功
        /// </returns>
        public int Bind(string libId,
            string strPrefix,
            string strWord,
            string strPassword,
            string weixinId,
            out WxUserItem userItem,
            out string strError)
        {
            userItem = null;
            strError = "";
            long lRet = -1;

            LibEntity lib = this.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                return -1;
            }


            string strFullWord = strWord;
            if (string.IsNullOrEmpty(strPrefix) == false)
                strFullWord = strPrefix + ":" + strWord ;


            if (strPrefix == "PQR")
                strPassword = Cryptography.GetSHA1(strFullWord);

            // 使用代理账号capo 20161024 jane
            LoginInfo loginInfo = new LoginInfo("", false);

            CancellationToken cancel_token = new CancellationToken();

            string fullWeixinId = WeiXinConst.C_WeiXinIdPrefix + weixinId;//前端传来的weixinId带了@appId //+"@"+appId;
            string id = Guid.NewGuid().ToString();
            BindPatronRequest request = new BindPatronRequest(id,
                loginInfo,
                "bind",
                strFullWord,
                strPassword,
                fullWeixinId,
                "multiple",   // 2016-8-24 改为多重绑定，这是复杂的情况，要不没法与mongodb保持一致，比较一个微信用户绑了一位读者，另一个微信用户又绑了这名相同的读者，如果不用多重绑定，就用第一名读者冲掉了，但微信mongodb并不知道。 //single,multiple
                "xml");
            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;
                BindPatronResult result = connection.BindPatronTaskAsync(
                     lib.capoUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                if (result.Value == -1)
                {
                    strError = this.GetFriendlyErrorInfo(result, lib.libName); //result.ErrorInfo;
                    return -1;
                }

                // 获取需要缓存的信息
                string xml = result.Results[0];

                // 读者信息
                string readerBarcode = "";
                string readerName = "";
                string refID = "";
                string department = "";

                // 工作人员信息
                string userName = "";

                // 分馆代码，读者与工作人员共有
                string libraryCode = "";

                // 权限
                string rights = "";

                // 账户类型
                int type = 0;//账户类型：0表示读者 1表示工作人员

                // 工作人员账户
                if (strPrefix == "UN")
                {
                    type = 1;// 工作人员账户

                    List<string> weixinIds = null;
                    this.GetWorkerInfoByXml(xml, out weixinIds, out userName, out libraryCode, out rights);
                }
                else
                {
                    type = 0;//读者
                    List<string> weixinIds = null;
                    WxUserItem patronInfo = this.GetPatronInfoByXml(xml, out weixinIds);
                    readerBarcode = patronInfo.readerBarcode;
                    readerName = patronInfo.readerName;
                    refID = patronInfo.refID;
                    department = patronInfo.department;
                    libraryCode = patronInfo.libraryCode;
                }

                // 找到库中对应的记录
                if (type == 0)
                {
                    List<WxUserItem> tempList = WxUserDatabase.Current.GetPatron(weixinId, libId, readerBarcode);
                    if (tempList != null && tempList.Count > 0)
                        userItem = tempList[0];
                }
                else
                    userItem = WxUserDatabase.Current.GetWorker(weixinId, libId);

                // 是否新增，对于工作人员账户，一个图书馆只绑一个工作人员，所以有update的情况
                bool bNew = false;
                if (userItem == null)
                {
                    bNew = true;
                    userItem = new WxUserItem();
                }

                userItem.weixinId = weixinId;
                userItem.libName = lib.libName;
                userItem.libId = lib.id;

                userItem.readerBarcode = readerBarcode;
                userItem.readerName = readerName;
                userItem.department = department;
                userItem.xml = xml;

                userItem.refID = refID;
                userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                userItem.updateTime = userItem.createTime;
                userItem.isActive = 0; // isActive只针对读者，后面会激活读者，工作人员时均为0

                userItem.libraryCode = libraryCode;
                userItem.type = type;
                userItem.userName = userName;
                userItem.isActiveWorker = 0;//是否是激活的工作人员账户，读者时均为0
                userItem.tracing = "off";//默认是关闭监控

                // 2016-8-26 新增
                userItem.state = 1;
                userItem.remark = strFullWord;
                userItem.rights = rights;

                //从微信id中拆出来
                string appId = "";
                int tempIndex =weixinId.IndexOf('@');
                if (tempIndex>0)
                {
                    appId = weixinId.Substring(tempIndex + 1); 
                }
                userItem.appId = appId;

                if (bNew == true)
                {
                    WxUserDatabase.Current.Add(userItem);
                    // 更新内存图书馆的绑定数量 2016/9/13
                    this.LibManager.UpdateBindCount(userItem.libId);
                }
                else
                {
                    lRet = WxUserDatabase.Current.Update(userItem);
                }

                if (type == 0)
                {
                    // 置为活动状态
                    WxUserDatabase.Current.SetActivePatron(userItem.weixinId, userItem.id);
                }

                // 2016-8-13 jane 自动修改当前的图书馆
                this.UpdateUserSetting(weixinId, libId, "", true, refID);//,因为有工作人员的情况，这里要传true


                // 发送绑定成功的客服消息    
                string strFirst = "☀恭喜您！您已成功绑定图书馆读者账号。";
                string strAccount = this.GetFullPatronName(userItem.readerName, userItem.readerBarcode, "", "", false);
                string strRemark = "您可以直接通过微信公众号访问图书馆，进行信息查询，预约续借等功能。如需解绑，请通过“绑定账号”菜单操作。";
                if (type == 1)
                {
                    strFirst = "☀恭喜您！您已成功绑定图书馆工作人员账号。";
                    strAccount = userItem.userName;
                    strRemark = "欢迎您使用微信公众号管理图书馆业务，如需解绑，请通过“绑定账号”菜单操作。";
                }

                string fullLibName = this.GetFullLibName(userItem.libName, userItem.libraryCode, "");
                string linkUrl="";//dp2WeiXinService.Instance.OAuth2_Url_AccountIndex,//详情转到账户管理界面
                // 本人
                List<string> bindWeixinIds = new List<string>();
                string tempfullWeixinId = weixinId;//2016-11-16 传进来的weixinId带了@appId // +"@" + appId;
                bindWeixinIds.Add(tempfullWeixinId);

                // 工作人员
                List<TracingOnUser> workers = this.getWorkerWeixinIds(libId, userItem.libraryCode);
                string first_color = "#000000";
                BindTemplateData msgData = new BindTemplateData(strFirst,
                    first_color,
                    strAccount,
                    fullLibName,
                    strRemark);

                //mask
                strAccount = this.GetFullPatronName(userItem.readerName, userItem.readerBarcode, "", "", true);//this.markString(userItem.readerName) + " " + this.markString(userItem.readerBarcode) + "";
                if (type == 1)
                {
                    strAccount = this.markString(userItem.userName);
                }
                BindTemplateData maskMsgData = new BindTemplateData(strFirst,
                    first_color,
                    strAccount,
                    fullLibName,
                    strRemark);

                // 发送消息
                int nRet= this.SendTemplateMsg(GzhCfg.C_Template_Bind,
                    bindWeixinIds,
                    workers,
                    msgData,
                    maskMsgData,
                    linkUrl,
                    "",
                    out strError);
                if (nRet == -1)
                {
                    return -1;
                }


                return 0;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

        ERROR1:
            return -1;
        }





        /// <returns>
        /// -1 出错
        /// 0   成功
        /// </returns>
        public int Unbind(string userId,
             out string strError)
        {
            strError = "";

            WxUserItem userItem = WxUserDatabase.Current.GetById(userId);
            if (userItem == null)
            {
                strError = "绑定账号未找到";
                return -1;
            }
            string weixinId = userItem.weixinId;

            LibEntity lib = this.GetLibById(userItem.libId);
            if (lib == null)
            {
                strError = "未找到id为[" + userItem.libId + "]的图书馆定义。";
                return -1;
            }

            string queryWord = "";
            if (userItem.type == WxUserDatabase.C_Type_Patron)
                queryWord = userItem.readerBarcode;
            else
                queryWord = "UN:" + userItem.userName;

            // 使用代理账号capo 20161024 jane
            LoginInfo loginInfo = new LoginInfo("", false);

            string weixinIdTemp = userItem.weixinId;
            int nTemp= weixinIdTemp.IndexOf("@");
            if ( nTemp>0)
            {
                weixinIdTemp =weixinIdTemp.Substring(0,nTemp);
            }

            weixinIdTemp += "*";
 


            // 调点对点解绑接口
            string fullWeixinId = WeiXinConst.C_WeiXinIdPrefix + weixinIdTemp;//userItem.weixinId + "*";// +userItem.appId;
            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            BindPatronRequest request = new BindPatronRequest(id,
                loginInfo,
                "unbind",
                queryWord,//userItem.readerBarcode,
                "",//password  todo
                fullWeixinId,
               "multiple,null_password",
                "xml");
            try
            {
                // 得到连接
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;

                BindPatronResult result = connection.BindPatronTaskAsync(
                     lib.capoUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                if (result.Value == -1)
                {
                    strError = this.GetFriendlyErrorInfo(result, lib.libName);//result.ErrorInfo;
                    return -1;
                }
                // 2016-11-15
                if (result.Value == 0)
                {
                    strError = "未匹配上weixinid。";
                    return -1;
                }

                // 删除mongodb库的记录
                WxUserItem newActivePatron = null;
                WxUserDatabase.Current.Delete(userId, out newActivePatron);

                // 更新内存图书馆的绑定数量 2016/9/13
                this.LibManager.UpdateBindCount(lib.id);


                string refID = "";
                if (newActivePatron != null)
                    refID = newActivePatron.refID;

                // 更新图书馆设置
                this.UpdateUserSetting(weixinId, lib.id, null, false, refID);

                // 发送解绑消息    
                string strFirst = "☀您已成功对图书馆读者账号解除绑定。";
                string strAccount = this.GetFullPatronName(userItem.readerName, userItem.readerBarcode, "", "", false);
                string strRemark = "\n您现在不能查看您在该图书馆的个人信息了，如需访问，请重新绑定。";
                if (userItem.type == WxUserDatabase.C_Type_Worker)
                {
                    strFirst = "☀您已成功对图书馆工作人员账号解除绑定。";
                    strAccount = userItem.userName;
                    strRemark = "\n您现在不能对该图书馆进行管理工作了，如需访问，请重新绑定。";
                }

                string fullLibName = this.GetFullLibName(userItem.libName, userItem.libraryCode, "");

                string linkUrl="";//dp2WeiXinService.Instance.OAuth2_Url_AccountIndex,//详情转到账户管理界面
                                string first_color = "#000000";

                // 本人
                List<string> bindWeixinIds = new List<string>();
                string temp = userItem.weixinId;//2016-11-16 传进来的weixinId带了@appId //+ "@"+userItem.appId;
                bindWeixinIds.Add(temp);//weixinId);
                List<TracingOnUser> workers = this.getWorkerWeixinIds(lib.id, userItem.libraryCode);

                //显文 
                UnBindTemplateData msgData = new UnBindTemplateData(strFirst,
                    first_color,
                    strAccount,
                    fullLibName,
                    strRemark);

                //mask
                strAccount = this.GetFullPatronName(userItem.readerName, userItem.readerBarcode, "", "", true);
                if (userItem.type == WxUserDatabase.C_Type_Worker)
                {
                    strAccount = this.markString(userItem.userName);
                }
                UnBindTemplateData maskMsgData = new UnBindTemplateData(strFirst,
                    first_color,
                    strAccount,
                    fullLibName,
                    strRemark);

                int nRet = this.SendTemplateMsg(GzhCfg.C_Template_UnBind,
                    bindWeixinIds,
                    workers,
                    msgData,
                    maskMsgData,
                    linkUrl,
                    "",
                    out strError);
                if (nRet == -1)
                {
                    return -1;
                }


                // 移除该工作人员的tracing on
                if (userItem.type == WxUserDatabase.C_Type_Worker)
                {
                    if (this.TracingOnUsers.ContainsKey(weixinId) == true)
                    {
                        // 检查一下该微信用户是否还绑定其它工作人员，如果没有再绑工作人员，从tracing列表中删除
                        List<WxUserItem> workerList = WxUserDatabase.Current.Get(weixinId, null, WxUserDatabase.C_Type_Worker);
                        if (workerList.Count == 0)
                        {
                            this.TracingOnUsers.Remove(weixinId);
                        }
                    }
                }

                return 0;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

        ERROR1:
            return -1;
        }



        #endregion

        #region 检索书目

        /// <summary>
        /// 检索书目
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="strFrom"></param>
        /// <param name="strWord"></param>
        /// <returns></returns>
        public SearchBiblioResult SearchBiblio(string weixinId,
            string libId,
            string strFrom,
            string strWord,
            string match,
            string resultSet)
        {
            SearchBiblioResult searchRet = new SearchBiblioResult();
            searchRet.apiResult = new ApiResult();
            searchRet.apiResult.errorCode = 0;
            searchRet.apiResult.errorInfo = "";
            searchRet.records = new List<BiblioRecord>();
            searchRet.isCanNext = false;


            // 未传入word
            if (string.IsNullOrEmpty(strWord) == true)
            {
                searchRet.apiResult.errorCode = -1;
                searchRet.apiResult.errorInfo = "尚未传入检索词";
                return searchRet;
            }

            // 未传入检索途径
            if (string.IsNullOrEmpty(strFrom) == true)
            {
                searchRet.apiResult.errorCode = -1;
                searchRet.apiResult.errorInfo = "尚未传入检索途径";
                return searchRet;
            }

            string strError = "";
            // 这里的records是第一页的记录
            List<BiblioRecord> records = null;
            bool bNext = false;
            long lRet = this.SearchBiblioInternal(weixinId,
                libId,
                strFrom,
                strWord,
                match,
                resultSet,
                0,
                WeiXinConst.C_OnePage_Count,
                out records,
                out bNext,
                out strError);
            if (lRet == -1 || lRet == 0)
            {
                searchRet.apiResult.errorCode = (int)lRet;
                searchRet.apiResult.errorInfo = strError;
                return searchRet;
            }

            searchRet.records = records;
            searchRet.resultCount = records.Count;
            searchRet.isCanNext = bNext;
            searchRet.apiResult.errorCode = lRet;

            return searchRet;
        }

        // 从结果集中取记录
        public SearchBiblioResult getFromResultSet(string weixinId,
            string libId,
            string resultSet,
            long start,
            long count)
        {
            SearchBiblioResult searchRet = new SearchBiblioResult();
            searchRet.apiResult = new ApiResult();
            searchRet.apiResult.errorCode = 0;
            searchRet.apiResult.errorInfo = "";
            searchRet.records = new List<BiblioRecord>();
            searchRet.isCanNext = false;


            string strError = "";
            List<BiblioRecord> records = null;
            bool bNext = false;
            long lRet = this.SearchBiblioInternal(weixinId,
                libId,
                 "",
                 "!getResult",
                 "",//match
                 resultSet,
                 start,
                 count,
                 out records,
                 out bNext,
                 out strError);
            if (lRet == -1 || lRet == 0)
            {
                searchRet.apiResult.errorCode = (int)lRet;
                searchRet.apiResult.errorInfo = strError;
                return searchRet;
            }
            searchRet.resultCount = records.Count;
            searchRet.records = records;
            searchRet.isCanNext = bNext;
            searchRet.apiResult.errorCode = lRet;
            return searchRet;
        }

        /// <summary>
        /// 检索书目
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="strFrom"></param>
        /// <param name="strWord"></param>
        /// <param name="records">第一批的10条</param>
        /// <param name="strError"></param>
        /// <returns></returns>
        private long SearchBiblioInternal(string weixinId,
            string libId,
            string strFrom,
            string strWord,
            string match,
            string resultSet,
            long start,
            long count,
            out List<BiblioRecord> records,
            out bool bNext,
            out string strError)
        {
            strError = "";
            records = new List<BiblioRecord>();
            bNext = false;

            LibEntity lib = this.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                return -1;
            }

            // 检查该图书馆的配置是否支持检索
            if (lib.noShareBiblio == 1)
            {
                // 检查微信用户是否绑定了图书馆账户，如果未绑定，则不能检索
                List<WxUserItem> userList = WxUserDatabase.Current.Get(weixinId, libId, -1);
                if (userList == null || userList.Count == 0)
                {
                    strError = "图书馆\"" + lib.libName + "\"不对外公开书目信息。";
                    return -1;
                }
            }

            string dbnames = "";
            int nRet = this.GetDbNames(lib, out dbnames, out strError);
            if (nRet == -1)
                return -1;
            //if (String.IsNullOrEmpty(lib.searchDbs) == false)
            //    dbnames = lib.searchDbs;


            //long start = 0;
            //long count = 10;
            try
            {

                // 使用代理账号capo 20161024 jane
                LoginInfo loginInfo = new LoginInfo("", false);

                CancellationToken cancel_token = new CancellationToken();
                string id = Guid.NewGuid().ToString();
                SearchRequest request = new SearchRequest(id,
                    loginInfo,
                    "searchBiblio",
                    dbnames,
                    strWord,
                    strFrom,
                    match,//"middle",
                    resultSet,//"weixin",
                    "id,cols",
                    WeiXinConst.C_Search_MaxCount,  //最大数量
                    start,  //每次获取范围
                    count);



                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;

                SearchResult result = connection.SearchTaskAsync(
                    lib.capoUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                // 输出检索语句
#if LOG_REQUEST_SEARCH
                writeDebug("search searchParam=" + request.Dump());
#endif

                if (result.ResultCount == -1)
                {
                    bool bOffline = false;
                    strError = this.GetFriendlyErrorInfo(result, lib.libName, out bOffline);
                    if (bOffline == true)
                    {
                        // 激活工作线程，为了给工作人员发通知。
                        // todo 但发通知是超过一小时才会发通知，工作线程是10分钟一次，好像这里激活一下意义也不太大。
                        this._managerThread.Activate();
                    }
                    return -1;
                }
                if (result.ResultCount == 0)
                {
                    strError = "未命中";
                    return 0;
                }


                List<string> resultPathList = new List<string>();
                for (int i = 0; i < result.Records.Count; i++)
                {
                    string xml = result.Records[i].Data;
                    /*<root><col>请让我慢慢长大</col>
                     * <col>吴蓓著</col>
                     * <col>天津教育出版社</col>
                     * <col>2009</col>
                     * <col>G61-53</col>
                     * <col>儿童教育儿童教育</col>
                     * <col></col>
                     * <col>978-7-5309-5335-8</col></root>*/
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    string name = DomUtil.GetNodeText(dom.DocumentElement.SelectSingleNode("col"));
                    string path = result.Records[i].RecPath;
                    int nIndex = path.IndexOf("@");
                    path = path.Substring(0, nIndex);

                    BiblioRecord record = new BiblioRecord();
                    record.recPath = path;
                    record.name = name;
                    record.no = (i + start + 1).ToString();//todo 注意下一页的时候
                    //record.libId = libId;
                    records.Add(record);
                }

                // 检查是否有下页
                if (start + records.Count < result.ResultCount)
                    bNext = true;

                //测试用2分钟。
                //Thread.Sleep(1000 * 60*2);

                return result.ResultCount;// records.Count;
            }
            catch (AggregateException ex)
            {
                strError = ex.Message;
                return -1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }

        #region 封面图像 静态函数

        public static string GetObjectHtmlFragment(string libId,
            string strBiblioRecPath,
            string objectUrl,
            string mime,
            string label)
        {
            if (string.IsNullOrEmpty(objectUrl) == true)
                return "";

            if (StringUtil.HasHead(objectUrl, "http:") == false)
            {
                string strUri = MakeObjectUrl(strBiblioRecPath,
                      objectUrl);

                objectUrl = "../patron/GetObject?libId=" + HttpUtility.UrlEncode(libId)
                    + "&uri=" + HttpUtility.UrlEncode(strUri);
            }


            string html = "<a href='" + objectUrl + "'>" + label + "</a>";
            return html;
        }

        public static string GetImageHtmlFragment(string libId,
    string strBiblioRecPath,
    string strImageUrl,
            bool addOnloadEvent)
        {
            //

            if (string.IsNullOrEmpty(strImageUrl) == true)
                return "";

            if (StringUtil.HasHead(strImageUrl, "http:") == false)
            {
                string strUri = MakeObjectUrl(strBiblioRecPath,
                      strImageUrl);

                strImageUrl = "../patron/getphoto?libId=" + HttpUtility.UrlEncode(libId)
                + "&objectPath=" + HttpUtility.UrlEncode(strUri);
            }

            string onloadStr = "";
            if (addOnloadEvent == true)
                onloadStr = " onload='setImgSize(this)' ";

            string html = "<img src='" + strImageUrl + "'  style='max-width:200px' " + onloadStr + "></img>"; // 2016/8/19 不要人为把宽高固定了  width='100px' height='100px'
            return html;
        }

        /// <summary>
        /// 获得封面图像 URL
        /// 优先选择中等大小的图片
        /// </summary>
        /// <param name="strMARC">MARC机内格式字符串</param>
        /// <param name="strPreferredType">优先使用何种大小类型</param>
        /// <returns>返回封面图像 URL。空表示没有找到</returns>
        public static string GetCoverImageUrl(string strMARC,
            string strPreferredType = "MediumImage")
        {
            string strLargeUrl = "";
            string strMediumUrl = "";   // type:FrontCover.MediumImage
            string strUrl = ""; // type:FronCover
            string strSmallUrl = "";

            MarcRecord record = new MarcRecord(strMARC);
            MarcNodeList fields = record.select("field[@name='856']");
            foreach (MarcField field in fields)
            {
                string x = field.select("subfield[@name='x']").FirstContent;
                if (string.IsNullOrEmpty(x) == true)
                    continue;
                Hashtable table = StringUtil.ParseParameters(x, ';', ':');
                string strType = (string)table["type"];
                if (string.IsNullOrEmpty(strType) == true)
                    continue;

                string u = field.select("subfield[@name='u']").FirstContent;
                // if (string.IsNullOrEmpty(u) == true)
                //     u = field.select("subfield[@name='8']").FirstContent;

                // . 分隔 FrontCover.MediumImage
                if (StringUtil.HasHead(strType, "FrontCover." + strPreferredType) == true)
                    return u;

                if (StringUtil.HasHead(strType, "FrontCover.SmallImage") == true)
                    strSmallUrl = u;
                else if (StringUtil.HasHead(strType, "FrontCover.MediumImage") == true)
                    strMediumUrl = u;
                else if (StringUtil.HasHead(strType, "FrontCover.LargeImage") == true)
                    strLargeUrl = u;
                else if (StringUtil.HasHead(strType, "FrontCover") == true)
                    strUrl = u;

            }

            if (string.IsNullOrEmpty(strLargeUrl) == false)
                return strLargeUrl;
            if (string.IsNullOrEmpty(strMediumUrl) == false)
                return strMediumUrl;
            if (string.IsNullOrEmpty(strUrl) == false)
                return strUrl;
            return strSmallUrl;
        }

        // 为了二次开发脚本使用
        public static string MakeObjectUrl(string strRecPath,
            string strUri)
        {
            if (string.IsNullOrEmpty(strUri) == true)
                return strUri;

            if (StringUtil.HasHead(strUri, "http:") == true)
                return strUri;

            if (StringUtil.HasHead(strUri, "uri:") == true)
                strUri = strUri.Substring(4).Trim();

            string strDbName = GetDbName(strRecPath);
            string strRecID = GetRecordID(strRecPath);

            string strOutputUri = "";
            ReplaceUri(strUri,
                strDbName,
                strRecID,
                out strOutputUri);

            return strOutputUri;
        }

        /// <summary>
        /// 从路径中取出库名部分
        /// </summary>
        /// <param name="strPath">路径。例如"中文图书/3"</param>
        /// <returns>返回库名部分</returns>
        public static string GetDbName(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return strPath;

            return strPath.Substring(0, nRet).Trim();
        }

        // 
        // parammeters:
        //      strPath 路径。例如"中文图书/3"
        /// <summary>
        /// 从路径中取出记录号部分
        /// </summary>
        /// <param name="strPath">路径。例如"中文图书/3"</param>
        /// <returns>返回记录号部分</returns>
        public static string GetRecordID(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return "";

            return strPath.Substring(nRet + 1).Trim();
        }

        static bool ReplaceUri(string strUri,
    string strCurDbName,
    string strCurRecID,
    out string strOutputUri)
        {
            strOutputUri = strUri;
            string strTemp = strUri;
            // 看看第一部分是不是object
            string strPart = GetFirstPartPath(ref strTemp);
            if (strPart == "")
                return false;

            if (strTemp == "")
            {
                strOutputUri = strCurDbName + "/" + strCurRecID + "/object/" + strPart;
                return true;
            }

            if (strPart == "object")
            {
                strOutputUri = strCurDbName + "/" + strCurRecID + "/object/" + strTemp;
                return true;
            }

            string strPart2 = GetFirstPartPath(ref strTemp);
            if (strPart2 == "")
                return false;

            if (strPart2 == "object")
            {
                strOutputUri = strCurDbName + "/" + strPart + "/object/" + strTemp;
                return false;
            }

            string strPart3 = GetFirstPartPath(ref strTemp);
            if (strPart3 == "")
                return false;

            if (strPart3 == "object")
            {
                strOutputUri = strPart + "/" + strPart2 + "/object/" + strTemp;
                return true;
            }

            return false;
        }

        // 得到strPath的第一部分,以'/'作为间隔符,同时 strPath 缩短
        public static string GetFirstPartPath(ref string strPath)
        {
            if (string.IsNullOrEmpty(strPath) == true)
                return "";

            string strResult = "";

            int nIndex = strPath.IndexOf('/');
            if (nIndex == -1)
            {
                strResult = strPath;
                strPath = "";
                return strResult;
            }

            strResult = strPath.Substring(0, nIndex);
            strPath = strPath.Substring(nIndex + 1);

            return strResult;
        }

        #endregion

        public long SearchBiblio(string weixinId,
            string libId,
            string from,
            string word,
            string match,
            out List<BiblioRecord> biblioList,
            out string strError)
        {
            strError = "";

            // 先检索书目
            string resultSet = "_searchitem";
            bool bNext = false;
            int start = 0;
            int count = WeiXinConst.C_OnePage_Count;
            biblioList = new List<BiblioRecord>();
            for (; ; )
            {
                List<BiblioRecord> records = null;
                long lRet = this.SearchBiblioInternal(weixinId,
                    libId,
                    from,
                    word,
                    match,
                    resultSet,
                    start,
                    count,
                    out records,
                    out bNext,
                    out strError);
                if (lRet == -1)
                    return -1;
                if (lRet == 0) //本有没有数据
                {
                    break;
                }

                if (records != null && records.Count > 0)
                {
                    biblioList.AddRange(records);
                }

                if (bNext == true)
                {
                    word = "!getResult";
                    from = "";
                    match = "";
                    start += records.Count;
                }
                else
                {
                    break;
                }
            }

            return biblioList.Count();
        }

        public long SearchItem(string weixinId,
            string libId,
            string userName,
            bool isPatron,
            string from,
            string word,
            string match,
            string cmdType,
            out List<BiblioItem> items,
            out string strError)
        {
            strError = "";
            items = new List<BiblioItem>();

            LibEntity lib = this.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                return -1;
            }

            List<BiblioRecord> biblioList = new List<BiblioRecord>();
            long lRet = this.SearchBiblio(weixinId,
                libId,
                from,
                word,
                match,
                out biblioList,
                out strError);
            if (lRet == -1 || lRet == 0)
                return lRet;

            foreach (BiblioRecord biblio in biblioList)
            {
                string biblioPath = biblio.recPath;

                // 取item
                List<BiblioItem> itemList = null;
                int nRet = (int)this.GetItemInfo(lib,
                    userName,
                    isPatron,
                    biblioPath,
                    cmdType,
                    out itemList,
                    out strError);
                if (nRet == -1) //0的情况表示没有册，不是错误
                {
                    return -1;
                }

                // 加到集合里
                items.AddRange(itemList);
            }

            return items.Count;
        }


        // 20170116 修改使用绑定的账户，如未绑定用public
        public BiblioDetailResult GetBiblioDetail(string weixinId,
            string libId,
            string biblioPath,
            string format,
            string from)
        {
            BiblioDetailResult result = new BiblioDetailResult();
            result.errorCode = 0;
            result.biblioPath = biblioPath;

            string strError = "";

            LibEntity lib = this.GetLibById(libId);
            if (lib == null)
            {
                result.errorInfo = "未找到id为[" + libId + "]的图书馆定义。";
                result.errorCode = -1;
                return result;
            }

            DateTime start_time = DateTime.Now;

            try
            {
                int nRet = 0;
                TimeSpan time_length = DateTime.Now - start_time;
                string logInfo = "";

                bool showCover = false;
                UserSettingItem item = UserSettingDb.Current.GetByWeixinId(weixinId);
                if (item != null && item.showCover == 1)
                {
                    showCover = true;
                }


                // 得到绑定工作人员账号，并检查是否有权限，进行好书推荐
                string workerUserName = "";
                string userName = "";
                bool isPatron = false;
                if (string.IsNullOrEmpty(weixinId) == false)
                {
                    WxUserItem patron = WxUserDatabase.Current.GetActivePatron(weixinId, libId);
                    if (patron != null)
                    {
                        userName = patron.readerBarcode;
                        isPatron = true;
                    }

                    // 查找当前微信用户绑定的工作人员账号，条件满足出来推荐图书按钮
                    WxUserItem worker = WxUserDatabase.Current.GetWorker(weixinId, libId);
                    // todo 后面可以放开对读者的权限
                    if (worker != null)
                    {
                        // 检索是否有权限 _wx_setHomePage
                        string needRight = dp2WeiXinService.C_Right_SetHomePage;
                        int nHasRights = dp2WeiXinService.Instance.CheckRights(worker,
                            lib,
                            needRight,
                            out strError);
                        if (nHasRights == -1)
                        {
                            result.errorCode = -1;
                            result.errorInfo = strError;
                            return result;
                        }
                        if (nHasRights == 1)
                        {
                            workerUserName = worker.userName;
                        }
                        else
                        {
                            workerUserName = "";
                        }

                        // 检查优先使用工作人员账户
                        userName = worker.userName;
                        isPatron = false;
                    }
                }

                if (String.IsNullOrEmpty(userName) == true)
                {
                    userName = "public";
                    isPatron = false;
                }


                // 取出summary
                this.WriteLog3("开始获取biblio info");

                string strBiblioInfo = "";
                string imgHtml = "";// 封面图像
                string biblioInfo = "";
                if (format == "summary")
                {
                    nRet = this.GetSummaryAndImgHtml(lib,
                        userName,
                        isPatron,
                       biblioPath,
                       showCover,
                        //lib,
                       out strBiblioInfo,
                       out imgHtml,
                       out strError);
                    if (nRet == -1 || nRet == 0)
                    {
                        result.errorCode = -1;
                        result.errorInfo = strError;
                        return result;
                    }

                    biblioInfo = "<table class='info'>"
                        + "<tr>"
                            + "<td class='cover'>" + imgHtml + "</td>"
                            + "<td class='biblio_info'>" + strBiblioInfo + "</td>"
                        + "</tr>"
                    + "</table>";
                }
                else if (format == "table")
                {
                    nRet = this.GetTableAndImgHtml(lib,
                        userName,
                        isPatron,
                        biblioPath,
                        showCover,
                        out strBiblioInfo,
                        out imgHtml,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                    {
                        result.errorCode = -1;
                        result.errorInfo = strError;
                        return result;
                    }

                    biblioInfo = "<table class='info'>"
                                        + "<tr>"
                                            + "<td class='biblio_info'>" + strBiblioInfo + "</td>" //image放在里面了 2016.8.8
                                        + "</tr>"
                                    + "</table>";
                }
                else
                {
                    strBiblioInfo = format + "风格";
                }


                string recommendBtn = "";
                if (workerUserName != "")
                {
                    string returnUrl = "/Biblio/Index";
                    if (from == "detail")
                        returnUrl = "/Biblio/Detail?biblioPath=" + HttpUtility.UrlEncode(biblioPath);

                    string recommPath = "/Library/Book?libId=" + libId //BookEdit
                        + "&userName=" + workerUserName
                        + "&biblioPath=" + HttpUtility.UrlEncode(biblioPath)
                        + "&isNew=1";
                    // + "&returnUrl=" + HttpUtility.UrlEncode(returnUrl);
                    recommendBtn = "<div class='btnRow'><button class='mui-btn  mui-btn-default' "
                        + " onclick=\"gotoUrl('" + recommPath + "')\">好书推荐</button></div>";

                }

                // 书目信息上方为 table+好书推荐按钮
                result.info = biblioInfo + recommendBtn; ;



                time_length = DateTime.Now - start_time;
                string info = "获取[" + biblioPath + "]的table信息完毕 time span: " + time_length.TotalSeconds.ToString() + " secs";
                this.WriteLog3(info);

                //Thread.Sleep(1000);

                // 取item
                this.WriteLog3("开始获取items");
                List<BiblioItem> itemList = null;
                nRet = (int)this.GetItemInfo(lib,
                    userName,
                    isPatron,
                    biblioPath,
                    "",
                    out itemList,
                    out strError);
                if (nRet == -1) //0的情况表示没有册，不是错误
                {
                    result.errorCode = -1;
                    result.errorInfo = strError;
                    return result;
                }

                // 计算用了多少时间
                time_length = DateTime.Now - start_time;
                logInfo = "获取[" + biblioPath + "]的item信息完毕 time span: " + time_length.TotalSeconds.ToString() + " secs";
                this.WriteLog3(logInfo);

                result.itemList = itemList;
                result.errorCode = 1;
                return result;
            }
            catch (Exception ex)
            {
                result.errorCode = -1;
                result.errorInfo = ex.Message;
                return result;
            }

        ERROR1:
            result.errorCode = -1;
            result.errorInfo = strError;
            return result;

        }

        //得到table风格的书目信息
        private int GetTableAndImgHtml(LibEntity lib,
            string userName,
            bool isPatron,
            string biblioPath,
            bool showCover,
            out string table,
            out string coverImgHtml,
            out string strError)
        {
            strError = "";
            table = "";
            coverImgHtml = "";

            List<string> dataList = null;
            int nRet = this.GetBiblioInfo(lib,
                userName,
                isPatron,
                biblioPath,
               "table",
                out dataList,
                out strError);
            if (nRet == -1 || nRet == 0)
                return nRet;

            string xml = dataList[0];
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
            XmlNode root = dom.DocumentElement;
            //<root>
            //  <line name="_coverImage" value="http://www.hongniba.com.cn/bookclub/images/books/book_20005451_b.jpg" />
            //  <line name="题名与责任说明拼音" value="dang wo xiang shui de shi hou" type="titlepinyin" />
            //  <line name="题名与责任说明" value="当我想睡的时候 [专著]  / (美)简·R. 霍华德文 ; (美)琳内·彻丽图 ; 林芳萍翻译" type="title" />
            //  <line name="责任者" value="霍华德; 林芳萍; 彻丽" />
            //  <line name="出版发行" value="石家庄 : 河北教育出版社, 2010" />
            //  <line name="载体形态" value="1册 ; 26cm" />
            //  <line name="主题分析" value="图画故事-美国-现代" />
            //  <line name="分类号" value="中图法分类号: I712.85" />
            //  <line name="附注" value="启发精选世界优秀畅销绘本版权页英文题名：When I'm sleepy" />
            //  <line name="获得方式" value="ISBN 978-7-5434-7754-4 (精装 ) : CNY27.80" />
            //  <line name="提要文摘" value="临睡前，带着孩子一起环游世界，看看他可不可以像长颈鹿一样站着睡，和蝙蝠一起倒挂着睡，或者像企鹅一样睡在好冷好冷的地方。" />
            //<line name="数字资源" type="object">
            //        <table>
            //            <line type="" urlLabel="this is link txt" uri="1" mime="image/pjpeg" bytes="20121" />
            //        </table>
            //    </line>
            //</root>
            string imgUrl = "";
            XmlNodeList lineList = root.SelectNodes("line");
            string pinyin = "";
            foreach (XmlNode node in lineList)
            {
                string name = DomUtil.GetAttr(node, "name");
                string value = DomUtil.GetAttr(node, "value");
                string type = DomUtil.GetAttr(node, "type");

                // 处理换行
                value = this.ConvertHtmlLine(value);

                if (name == "_coverImage")
                {
                    imgUrl = value;
                    if (showCover == true && String.IsNullOrEmpty(imgUrl) == false)
                    {
                        coverImgHtml = dp2WeiXinService.GetImageHtmlFragment(lib.id, biblioPath, imgUrl, true);
                    }

                    table += "<tr>"
                        + "<td class='name'></td>"
                        + "<td class='value'>" + coverImgHtml + "</td>"  // style='background-color:red'
                        + "</tr>";

                    //table += "<tr>"
                    //    + "<td colspan='2'>" + coverImgHtml + "</td>"
                    //    + "</tr>";

                    continue;
                }

                // 处理数字资源
                if (this.CheckContainWord(type, "object") == true)
                {

                    string resHtml = "<table>";

                    XmlNodeList resLineList = node.SelectNodes("table/line");
                    foreach (XmlNode resLineNode in resLineList)
                    {
                        //if (resHtml != "")
                        //    resHtml += "<br/>";

                        //<line type="" urlLabel="this is link txt" uri="1" mime="image/pjpeg" bytes="20121" />
                        string urlLabel = DomUtil.GetAttr(resLineNode, "urlLabel");
                        string uri = DomUtil.GetAttr(resLineNode, "uri");
                        string mime = DomUtil.GetAttr(resLineNode, "mime");
                        string bytes = DomUtil.GetAttr(resLineNode, "bytes");

                        string objectHtml = GetObjectHtmlFragment(lib.id, biblioPath, uri, mime, urlLabel);

                        resHtml += "<tr><td style='padding-bottom:5px'>" + objectHtml;

                        if (mime != "")
                            resHtml += "<br/><span style='color:gray'>媒体类型：</span>" + mime;

                        if (bytes != "")
                            resHtml += "<br/><span style='color:gray'>字节数：</span>" + bytes;

                        resHtml += "</td></tr>";

                    }
                    resHtml += "</table>";

                    table += "<tr>"
                       + "<td class='name'>" + name + "</td>"
                       + "<td class='value' style='word-wrap:break-word;word-break:break-all;white-space:pre-wrap'>" + resHtml + "</td>"
                       + "</tr>";

                    continue;
                }

                //这个版本，为书目信息的 table 格式，增加了一个 type 属性。注意这是一个逗号间隔的字符串，
                //虽然现在还没有用到逗号。无论是中文还是英文的书目数据，题名行都有 type=title，需要用这个来识别，以把这行加粗。
                //特殊地，中文的书目数据，还可能具有题名拼音行，那么它会有 type=titlepinyin。
                //再次强调一下，type 属性的值是一个逗号间隔的字符串，因此判断 title 和 titlepinyin 的时候要用特定的解析函数，
                //否则将来数据中一旦出现逗号的时候就会出现故障。
                //  <line name="题名与责任说明拼音" value="dang wo xiang shui de shi hou" type="titlepinyin" />
                //  <line name="题名与责任说明" value="当我想睡的时候 [专著]  林芳萍翻译" type="title" />

                // 检查是不是拼音
                if (this.CheckContainWord(type, "titlepinyin") == true) // name == "题名与责任说明拼音")
                {
                    pinyin = value;
                    continue;
                }


                // 是否为标题行
                if (this.CheckContainWord(type, "title") == true) //(name == "题名与责任说明")
                {
                    // 拼音与书名合为一行
                    if (String.IsNullOrEmpty(pinyin) == false)
                    {
                        table += "<tr>"
                            + "<td class='name'>" + name + "</td>"
                            + "<td class='titlevalue'>"
                                + "<span style='color:gray'>" + pinyin + "</span><br/>"
                                + value
                            + "</td>"
                            + "</tr>";
                    }
                    else
                    {
                        table += "<tr>"
                           + "<td class='name'>" + name + "</td>"
                           + "<td class='titlevalue'>" + value + "</td>"
                           + "</tr>";
                    }
                    continue;
                }

                table += "<tr>"
                    + "<td class='name'>" + name + "</td>"
                    + "<td class='value'>" + value + "</td>"
                    + "</tr>";
            }

            if (table != "")
            {
                table += "<tr>"
                    + "<td class='name'>路径</td>"
                    + "<td class='value'>" + biblioPath + "</td>"
                    + "</tr>";

                table = "<table class='biblio_table'>" + table + "</table>";
            }

            return 1;
        }

        public bool CheckContainWord(string text, string word)
        {
            // 先将text按逗号拆分
            string[] list = text.Split(',');
            foreach (string str in list)
            {
                if (str.Trim() == word) //注意这里去掉前后空白了
                    return true;
            }

            return false;
        }


        public string ConvertHtmlLine(string text1)
        {
            // 处理换行
            string retText = text1;

            retText = retText.Replace("\r\n", "\n");
            retText = retText.Replace("\r", "\n");
            retText = retText.Replace("\n", "<br/>");

            return retText;
        }


        private int GetSummaryAndImgHtml(LibEntity lib,
            string userName,
            bool isPatron,
            string biblioPath,
            bool showCover,
            out string summary,
            out string coverImgHtml,
            out string strError)
        {
            strError = "";
            summary = "";
            coverImgHtml = "";

            List<string> dataList = null;
            int nRet = this.GetBiblioInfo(lib,
                userName,
                isPatron,
                biblioPath,
               "summary,xml",
                out dataList,
                out strError);
            if (nRet == -1 || nRet == 0)
                return nRet;

            summary = dataList[0];
            summary = "<span class='summary'>" + summary + "</span>";
            string xml = dataList[1];
            if (showCover == true)
            {
                string strOutMarcSyntax = "";
                string strMARC = "";
                string strFragmentXml = "";
                // 将XML格式转换为MARC格式
                // 自动从数据记录中获得MARC语法
                nRet = MarcUtil.Xml2Marc(xml,
                    MarcUtil.Xml2MarcStyle.Warning | MarcUtil.Xml2MarcStyle.OutputFragmentXml,
                    "",
                    out strOutMarcSyntax,
                    out strMARC,
                    out strFragmentXml,
                    out strError);
                if (nRet == -1)
                {
                    strError = "XML转换到MARC记录时出错：" + strError;
                    return -1;
                }

                string strImageUrl = GetCoverImageUrl(strMARC, "MediumImage");
                coverImgHtml = dp2WeiXinService.GetImageHtmlFragment(lib.id, biblioPath, strImageUrl, false);

            }


            return 1;
        }

        public int GetBiblioInfo(LibEntity lib,
            string userName,
            bool isPatron,
            string biblioPath,
            string formatList,
            out List<string> dataList,
            out string strError)
        {
            // 20170116 如果即没有绑工作人员，也没有绑读者账户，采用publish帐号，
            // 因为数字资源有访问权限，不能再用capo， capo账户有wirteobject权限 导致 856和对象上设置的访问权限失效了
            if (String.IsNullOrEmpty(userName) == true)
            {
                userName = "public";
                isPatron = false;
            }
            //LoginInfo loginInfo = new LoginInfo(userName, isPatron); 


            return this.GetInfo(lib,
                userName,
                isPatron,
                "getBiblioInfo", 
                biblioPath, 
                formatList, 
                out dataList, 
                out strError);
        }

        /// <summary>
        /// get info 底层api
        /// </summary>
        /// <param name="lib"></param>
        /// <param name="method"></param>
        /// <param name="queryWord"></param>
        /// <param name="formatList"></param>
        /// <param name="dataList"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int GetInfo(LibEntity lib,
            string userName,
            bool isPatron,
            string method,
            string queryWord,
            string formatList,
            out List<string> dataList,
            out string strError)
        {
            strError = "";
            dataList = new List<string>();

            // 使用代理账号capo 20161024 jane
            // 20170116 改为实际传进来的值
            LoginInfo loginInfo = new LoginInfo(userName, isPatron);

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
                loginInfo,
                method,
                "",
                queryWord,
                "",
                "",
                "",
                formatList,//
                1,
                0,
                -1);
            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;
                SearchResult result = connection.SearchTaskAsync(
                    lib.capoUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                if (result.ResultCount == -1)
                {
                    bool bOffline = false;
                    strError = this.GetFriendlyErrorInfo(result, lib.libName, out bOffline);// result.ErrorInfo;
                    return -1;
                }
                if (result.ResultCount == 0)
                {
                    strError = "未命中";
                    return 0;
                }
                if (result.Records != null && result.Records.Count > 0)
                {
                    for (int i = 0; i < result.Records.Count; i++)
                    {
                        dataList.Add(result.Records[i].Data);
                    }
                }

                return 1;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
        ERROR1:
            return -1;
        }


        /// <summary>
        /// 获取summary
        /// </summary>
        /// <param name="capoUserName"></param>
        /// <param name="word"></param>
        /// <param name="strBiblioRecPathExclude">排除的书目路径</param>
        /// <param name="summary"></param>
        /// <param name="strRecPath"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int GetBiblioSummary(LibEntity lib,//string capoUserName,
            string word,
            string strBiblioRecPathExclude,
            out string summary,
            out string strRecPath,
            out string strError)
        {
            summary = "";
            strError = "";
            strRecPath = "";

            // 使用代理账号capo 20161024 jane
            LoginInfo loginInfo = new LoginInfo("", false);

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
                loginInfo,
                "getBiblioSummary",
                "<全部>",
                word,
                "",
                strBiblioRecPathExclude,
                "",
                "",
                1,
                0,
                -1);
            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;  //+ "-1"

                SearchResult result = connection.SearchTaskAsync(
                    lib.capoUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                if (result.ResultCount == -1)
                {
                    bool bOffline = false;
                    strError = "GetBiblioSummary()出错：" + this.GetFriendlyErrorInfo(result, lib.libName, out bOffline); //result.ErrorInfo ;
                    return -1;
                }
                if (result.ResultCount == 0)
                {
                    strError = "未命中";
                    return 0;
                }

                summary = result.Records[0].Data;
                strRecPath = result.Records[0].RecPath;


                return 1;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
        ERROR1:
            return -1;
        }

        public long GetItemInfoApi(LibEntity lib,
            string userName,
            bool isPatron,
            string biblioPath,
            string dbNameList,
            string formatList,
            long maxResults,
            out List<Record> dataList,
            out string strError)
        {
            strError = "";
            dataList = new List<Record>();

            // 使用代理账号capo 20161024 jane
            LoginInfo loginInfo = new LoginInfo(userName, isPatron);

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
                loginInfo,
                "getItemInfo",
                dbNameList,
                biblioPath,
                "",
                "",
                "",
                formatList,
                maxResults,
                0,
                -1);
            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;

                SearchResult result = connection.SearchTaskAsync(
                       lib.capoUserName,
                       request,
                       new TimeSpan(0, 1, 0),
                       cancel_token).Result;

                if (result.ResultCount == -1 && result.ErrorCode != "ItemDbNotDef") // 2016-8-19 过滤到未定义实体库的情况
                {
                    bool bOffline = false;
                    strError = "GetItemInfo()出错：" + this.GetFriendlyErrorInfo(result, lib.libName, out bOffline);//result.ErrorInfo;
                    return -1;
                }
                if (result.ResultCount == 0)
                {
                    strError = "未命中";
                    return 0;
                }

                for (int i = 0; i < result.Records.Count; i++)
                {
                    //string data = result.Records[i].Data;
                    dataList.Add(result.Records[i]);
                }

                return result.Records.Count;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }


        ERROR1:
            return -1;
        }

        //
        public long GetIssue(LibEntity lib,
            string userName,
            bool isPatron,
            string biblioPath,
            string style,
            out string issuePath,
            out string issueXml,
            out string strError)
        {
            strError = "";
            issueXml = "";
            issuePath = "";

           
            List<Record> dataList = null;
            long lRet = this.GetItemInfoApi(lib,
                userName,
                isPatron,
                biblioPath,
                "issue",
                style,
                10,
                out dataList,
                out strError);
            if (lRet == -1)
                return -1;

            if (lRet == 0)
            {
                strError = "通过biblioPath=" + biblioPath + ",style=" + style + " 查期未找到。";
                return -1;
            }

            issueXml = dataList[0].Data;
            issuePath = dataList[0].RecPath;
            issuePath = GetPurePath(issuePath);


            return 1;
        }

        public long GetItemInfo(LibEntity lib,//string libId,            
            string userName,
            bool isPatron,
            string biblioPath,
            string cmdType,
            out List<BiblioItem> itemList,
            out string strError)
        {
            itemList = new List<BiblioItem>();
            strError = "";

            if (lib == null)
            {
                strError = "GetItemInfo() lib参数不能为null。";
                goto ERROR1;
            }

            // 是否是读者账号
            string patronBarcode = "";
            if (isPatron == true)
                patronBarcode = userName;

            string patronName = "";//todo

            List<ReservationInfo> reserList = null;
            if (string.IsNullOrEmpty(patronBarcode) == false)
            {
                int nRet = this.GetPatronReservation(lib.id,
                    userName,
                    isPatron,
                    patronBarcode,
                    out reserList,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;
            }

            bool bCanReservation = false;
            string returnUrl = "/Biblio/Index";
            string reservationInfo = "<span class='remark'>您尚未绑定当前选择图书馆的读者账号，所以看不到预约信息，"
                + "点击<a href='javascript:void(0)' onclick='gotoUrl(\"/Account/Bind?returnUrl="
                + HttpUtility.UrlEncode(returnUrl) + "\")'>这里</a>绑定读者帐号。</span>";
            if (patronBarcode != null && patronBarcode != "")
            {
                bCanReservation = true;
                reservationInfo = "";
            }

            List<Record> recordList = null;
            long lRet = this.GetItemInfoApi(lib,
                userName,
                isPatron,
                biblioPath,
                "entity",//dbNameList,
                "opac",//formatList,
                10,//maxResults,
                out recordList,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (lRet == 0)
                return 0;

            for (int i = 0; i < recordList.Count; i++)
            {
                BiblioItem item = new BiblioItem();
                item.recPath = this.GetPurePath(recordList[i].RecPath);

                string xml = recordList[i].Data;//result.Records[i].Data;
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(xml);

                string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");

                item.refID = strRefID;
                item.pureBarcode = strBarcode;

                // 册条码号
                string strViewBarcode = "";
                if (string.IsNullOrEmpty(strBarcode) == false)
                    strViewBarcode = strBarcode;
                else
                    strViewBarcode = "@refID:" + strRefID;  //"@refID:"
                item.barcode = strViewBarcode;

                //状态
                string strState = DomUtil.GetElementText(dom.DocumentElement, "state");
                if (cmdType == "borrow")
                {
                }

                //// strState = strMessageText + strState;
                //string strStateMessage = DomUtil.GetElementText(dom.DocumentElement,
                //    "stateMessage");
                //if (String.IsNullOrEmpty(strStateMessage) == false)
                //    item.state = strStateMessage;
                bool bMember = false;
                XmlNode nodeBindingParent = dom.DocumentElement.SelectSingleNode("binding/bindingParent");
                if (nodeBindingParent != null)
                {
                    bMember = true;
                    StringUtil.SetInList(ref strState, "已装入合订册", true);

                    string parentRefID = DomUtil.GetAttr(nodeBindingParent, "refID");
                    item.parentInfo = "@refID:" + parentRefID;
                }
                item.state = strState;


                bool disable = false;
                if (StringUtil.IsInList("加工中", item.state) == true
                    || bMember == true)
                {
                    disable = true;
                }
                item.disable = disable;


                //卷册
                item.volume = DomUtil.GetElementText(dom.DocumentElement, "volume");
                // 馆藏地
                item.location = DomUtil.GetElementText(dom.DocumentElement, "location");
                // 索引号
                item.accessNo = DomUtil.GetElementText(dom.DocumentElement, "accessNo");
                // 出版日期
                item.publishTime = DomUtil.GetElementText(dom.DocumentElement, "publishTime");
                // 价格
                item.price = DomUtil.GetElementText(dom.DocumentElement, "price");
                // 注释
                item.comment = DomUtil.GetElementText(dom.DocumentElement, "comment");

                // 借阅情况
                /*
                 <borrower>R00001</borrower>
<borrowerReaderType>教职工</borrowerReaderType>
<borrowerRecPath>读者/1</borrowerRecPath>
<borrowDate>Sun, 17 Apr 2016 23:57:40 +0800</borrowDate>
<borrowPeriod>31day</borrowPeriod>
<returningDate>Wed, 18 May 2016 12:00:00 +0800</returningDate>
                 */
                string strBorrower = DomUtil.GetElementText(dom.DocumentElement, "borrower");
                string borrowDate = DateTimeUtil.ToLocalTime(DomUtil.GetElementText(dom.DocumentElement,
"borrowDate"), "yyyy/MM/dd");
                string borrowPeriod = DomUtil.GetElementText(dom.DocumentElement, "borrowPeriod");
                borrowPeriod = GetDisplayTimePeriodStringEx(borrowPeriod);
                item.borrower = strBorrower;
                item.borrowDate = borrowDate;
                item.borrowPeriod = borrowPeriod;

                item.isGray = false;
                if (isPatron == false)
                {
                    if (cmdType == "borrow")
                    {
                        if (string.IsNullOrEmpty(strBorrower) == false
                            || String.IsNullOrEmpty(strState) == false)
                        {
                            item.isGray = true;
                        }
                    }
                    else if (cmdType == "return")
                    {
                        // 没有在借的册需要显示为灰色
                        if (string.IsNullOrEmpty(strBorrower) == true)
                            item.isGray = true;
                    }
                }

                // 成员册 不显示“在借情况”和“预约信息”
                if (bMember == false)
                {
                    string strBorrowInfo = "在架";
                    bool bOwnBorrow = false;
                    // 检查是不是当前读者借的
                    if (string.IsNullOrEmpty(strBorrower) == false)
                    {
                        if (patronBarcode != item.borrower)
                        {
                            if (isPatron == true)
                            {
                                strBorrowInfo = "借阅者：***<br/>"
                                + "借阅时间：" + item.borrowDate + "<br/>"
                                + "借期：" + item.borrowPeriod;
                            }
                            else
                            {
                                strBorrowInfo = "借阅者：" + strBorrower + "<br/>"
                               + "借阅时间：" + item.borrowDate + "<br/>"
                               + "借期：" + item.borrowPeriod;
                            }
                        }
                        else
                        {
                            // 2016-8-16 修改isbn不能预约的情况
                            string tempBarcode = item.barcode;
                            if (tempBarcode.Contains("@refID:") == true)
                                tempBarcode = tempBarcode.Replace("@refID:", "refID-");

                            strBorrowInfo =
                                "<table style='width:100%;border:0px'>"
                                + "<tr>"
                                    + "<td class='info' style='border:0px'>借阅者：" + patronName + "<br/>"
                                                                + "借阅时间：" + item.borrowDate + "<br/>"
                                                                + "借期：" + item.borrowPeriod
                                        + "</td>"
                                    + "<td class='btn' style='border:0px'>";

                            if (isPatron == true) // 只有读者身份，才有预约按钮
                            {
                                strBorrowInfo += "<button class='mui-btn  mui-btn-default'  onclick=\"renew('" + tempBarcode + "')\">续借</button>";
                            }
                            strBorrowInfo += "</td>"
                                + "</tr>"
                                + "<tr><td colspan='2'><div id='renewInfo-" + tempBarcode + "'/></td></tr>"
                                + "</table>";

                            // 此时不能预约
                            bOwnBorrow = true;
                            reservationInfo = "<div class='remark'>该册目前是您在借中，不能预约。</div>";
                        }
                    }
                    item.borrowInfo = strBorrowInfo;

                    // 预约信息
                    if (bCanReservation == true && bOwnBorrow == false)
                    {
                        bool showReser = true;
                        if (disable == true || isPatron == false)
                            showReser = false;


                        string state = this.getReservationState(reserList, item.barcode);
                        reservationInfo = getReservationHtml(state, item.barcode, false, showReser);
                    }
                    item.reservationInfo = reservationInfo;
                }


                // 检查数据库是否为期刊库
                string biblioDbName = "";
                string biblioId = "";
                int index = biblioPath.LastIndexOf("/");
                biblioDbName = biblioPath.Substring(0, index);
                biblioId = biblioPath.Substring(index + 1);

                Library thisLib = this.LibManager.GetLibrary(lib.id);
                DbCfg db = thisLib.GetDb(biblioDbName);
                if (db != null && String.IsNullOrEmpty(db.IssueDbName) == false)
                {
                    string totalImgs = "";

                    // 封面图片
                    // 得到检索期的字符串
                    List<IssueString> issueList = dp2StringUtil.GetIssueQueryStringFromItemXml(dom);
                    if (issueList != null && issueList.Count > 0)// todo 为啥会有多项？
                    {
                        foreach (IssueString issueStr in issueList)
                        {

                            // 获取期记录
                            string style = "query:父记录+期号|" + biblioId + "|" + issueStr.Query;
                            string issueXml = "";
                            string issuePath = "";
                            long ret = this.GetIssue(lib,
                                userName,
                                isPatron,
                                biblioPath,
                                style,
                                out issuePath,
                                out issueXml,
                                out strError);
                            if (ret == -1)
                            {
                                goto ERROR1;
                            }

                            // 从期中取出图片url
                            XmlDocument issueDom = new XmlDocument();
                            issueDom.LoadXml(issueXml);
                            string imgId = dp2StringUtil.GetCoverImageIDFromIssueRecord(issueDom, "LargeImage");
                            //string issueImgUrl = issuePath + "/object/"+imgId;
                            string imgHtml = dp2WeiXinService.GetImageHtmlFragment(lib.id, issuePath, imgId, true);

                            if (totalImgs != "")
                                totalImgs += "&nbsp;";
                            totalImgs += imgHtml;
                        }
                    }

                    item.imgHtml = totalImgs;
                }

                // 加到数组里
                itemList.Add(item);
            }

            return lRet;

        ERROR1:
            return -1;
        }


        // 得到预约状态
        private string getReservationState(List<ReservationInfo> list, string barcode)
        {
            if (list == null || list.Count == 0)
                return "未预约";
            for (int i = 0; i < list.Count; i++)
            {
                ReservationInfo reservation = list[i];
                string barcodeList = reservation.pureBarcodes;
                string arrivedBarcode = reservation.arrivedBarcode;

                //int nIndex = barcodeList.IndexOf(barcode);
                //if (nIndex >= 0)
                if (StringUtil.IsInList(barcode, barcodeList) == true)
                {
                    if (barcode == arrivedBarcode)
                        return "已到书";
                    else
                        return "已预约";
                }
            }

            return "未预约";
        }

        // 得到预约状态和操作按钮
        private string getReservationHtml(string reservationState, string barcode, bool bOnlyReserRow, bool showBtn)//List<ReservationInfo> list, string barcode)
        {

            // 2016-8-16 修改isbn不能预约的情况
            if (barcode.Contains("@refID:") == true)
                barcode = barcode.Replace("@refID:", "refID-");

            string html = "";
            //string reservationState = this.getReservationState(list, barcode);
            string btn = "";

            if (showBtn == true)
            {
                if (reservationState == "未预约")
                {
                    btn = "<button class='mui-btn  mui-btn-default' onclick=\"reservation(this,'" + barcode + "','new')\">预约</button>";
                }
                else if (reservationState == "已预约")
                {
                    btn = "<button class='mui-btn  mui-btn-default'  onclick=\"reservation(this,'" + barcode + "','delete')\">取消预约</button>";
                }
                else if (reservationState == "已到书")
                {
                    btn = "<button class='mui-btn  mui-btn-default'  onclick=\"reservation(this,'" + barcode + "','delete')\">放弃取书</button>";
                }
            }

            if (reservationState != "")
            {
                if (bOnlyReserRow == false)
                {
                    html += "<table style='width:100%;border:0px'>";
                }

                html += "<tr class='reserRow'>"
                    + "<td class='info'  style='border:0px'>" + reservationState + "</td><td class='btn'>" + btn + "</td>"
                    + "</tr>";

                if (bOnlyReserRow == false)
                {
                    html += "<tr>"
                        + "<td colspan='2' style='border:0px'><div class='resultInfo'></div></td>"
                    + "</tr>"
                    + "</table>";
                }
            }
            return html;
        }

        // 获取多个item的summary
        public string GetBarcodesSummary(LibEntity lib,//string capoUserName,
            string strBarcodes)
        {
            string strSummary = "";
            string strArrivedItemBarcode = "";
            int nIndex = strBarcodes.IndexOf("*");
            if (nIndex > 0)
            {
                string tempBarcodes = strBarcodes.Substring(0, nIndex);
                strArrivedItemBarcode = strBarcodes.Substring(nIndex + 1);
                strBarcodes = tempBarcodes;
            }

            string strDisableClass = "";
            if (string.IsNullOrEmpty(strArrivedItemBarcode) == false)
                strDisableClass = "deleted";

            string strPrevBiblioRecPath = "";
            string[] barcodes = strBarcodes.Split(new char[] { ',' });
            for (int j = 0; j < barcodes.Length; j++)
            {
                string strBarcode = barcodes[j];
                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

                // 获得摘要
                string strOneSummary = "";
                string strBiblioRecPath = "";


                //    GetBiblioSummaryResponse result = channel.GetBiblioSummary(strBarcode, strPrevBiblioRecPath);
                string strError = "";
                int nRet = this.GetBiblioSummary(lib,//capoUserName,
                    strBarcode,
                    strPrevBiblioRecPath,
                    out strOneSummary,
                    out strBiblioRecPath,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    strOneSummary = strError;

                int tempIndex = strBiblioRecPath.IndexOf("@");
                if (tempIndex > 0)
                    strBiblioRecPath = strBiblioRecPath.Substring(0, tempIndex);

                if (strOneSummary == "" && strPrevBiblioRecPath == strBiblioRecPath)
                {
                    strOneSummary = "(同上)";
                }

                string strClass = "";
                if (string.IsNullOrEmpty(strDisableClass) == false && strBarcode != strArrivedItemBarcode)
                    strClass = " class='" + strDisableClass + "' ";

                //string strBarcodeLink = "<a " + strClass
                //    + " href='" + this.opacUrl + "/book.aspx?barcode=" + strBarcode + "'   target='_blank' " + ">" + strBarcode + "</a>";

                string strBarcodeLink = strBarcode;
                strSummary += "<div>" + strBarcodeLink + strOneSummary + "</div>";

                //var strOneSummaryOverflow = "<div style='width:100%;white-space:nowrap;overflow:hidden; text-overflow:ellipsis;'  title='" + strOneSummary + "'>"
                //   + strOneSummary
                //   + "</div>";

                //strSummary += "<table style='width:100%;table-layout:fixed;'>"
                //    + "<tr>"
                //    + "<td width='10%;vertical-align:middle'>" + strBarcodeLink + "</td>"
                //    + "<td>" + strOneSummaryOverflow + "</td>"
                //    + "</tr></table>";

                strPrevBiblioRecPath = strBiblioRecPath;
            }
            return strSummary;
        }


        #endregion

        #region 个人信息


        public int GetPatronInfo(string libId,
            string userName,
            bool isPatron,
            string patronBarcode,
            string style,
            out Patron patron,
            out string info,
            out string strError)
        {
            patron = null;
            strError = "";
            info = "";

            // 获取当前账户的信息
            string strXml = "";
            string recPath = "";
            int nRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                userName,
                isPatron,
                patronBarcode,
                "advancexml",
                out recPath,
                out strXml,
                out strError);
            if (nRet == -1 || nRet == 0)
                return nRet;

            int showPhoto = 0;
            if (StringUtil.IsInList("img", style) == true)
                showPhoto = 1;
            patron = this.ParsePatronXml(libId,
                strXml,
                recPath,
                showPhoto);

            if (StringUtil.IsInList("html", style) == true)
            {
                info = this.GetPatronHtml(patron);
            }
            else if (StringUtil.IsInList("summary", style) == true)
            {
                info = this.GetPatronSummary(patron, userName);
            }

            return 1;
        }

        // 获取读者的html
        public string GetPatronHtml(Patron patron, bool showQrcode = true)
        {
            string html = "";

            html = "<table class='personalinfo'>"
            + "<tr class='person_name'> <td class='name'>姓名</td> <td class='value'>" + patron.name + "</td> </tr>"
            + "<tr> <td class='name'>性别</td> <td class='value'>" + patron.gender + "</td> </tr>"
            + "<tr> <td class='name'>出生日期</td> <td class='value'>" + patron.dateOfBirth + "</td> </tr>"
            + "<tr> <td class='name'>证号</td> <td class='value'>" + patron.cardNumber + "</td> </tr>"
            + "<tr> <td class='name'>身份证号</td> <td class='value'>" + patron.idCardNumber + "</td> </tr>"
            + "<tr> <td class='name'>单位</td> <td class='value'>" + patron.department + "</td> </tr>"
            + "<tr> <td class='name'>职务</td> <td class='value'>" + patron.post + "</td> </tr>"
            + "<tr> <td class='name'>地址</td> <td class='value'>" + patron.address + "</td> </tr>"
            + "<tr> <td class='name'>电话</td> <td class='value'>" + patron.tel + "</td> </tr>"
            + "<tr> <td class='name'>Email</td> <td class='value'>" + patron.email + "</td> </tr>"
            + "<tr class='barcode'> <td class='name'>证条码号</td> <td class='value'>" + patron.barcode + "</td> </tr>"
            + "<tr> <td class='name'>读者类型</td> <td class='value'>" + patron.readerType + "</td> </tr>"
            + "<tr class='state'> <td class='name'> 证状态</td> <td class='value'>" + patron.state + "</td> </tr>"
            + "<tr class='createdate'> <td class='name'>发证日期</td> <td class='value'>" + patron.createDate + "</td> </tr>"
            + "<tr class='expiredate'> <td class='name'>证失效期</td> <td class='value'>" + patron.expireDate + "</td> </tr>"
            + "<tr class='hire'> <td class='name'>租金</td> <td class='value'>" + patron.hire + "</td> </tr>";

            if (string.IsNullOrEmpty(patron.imageUrl) == false)
            {
                html += "<tr class='photo'>"
                   + "<td class='name'>头像</td>"
                   + "<td class='value'>"
                   + "<img src='" + patron.imageUrl + "' style='max-width:200px' />"
                   + "</td>"
                   + "</tr>";
            }
            html += "</table>";

            if (showQrcode == true)
            {
                // 二维码
                html += "<div class='mui-content-padded'>"
                                + "<center>"
                                    + "<img id='qrcode' src='" + patron.qrcodeUrl + "' alt='QRCode image' />"
                                    + "<div style='font-size:9pt'>(不要把二维码展示和提供给无关人员，以免账号被窃。)</div>"
                                + "</center>"
                           + "</div>";
            }

            return html;
        }

        // 获取读者的summary
        public string GetPatronSummary(Patron patron, string userName)
        {
            string summary = "";

            //summary += "<div onclick='gotoUrl(\"/patron/PersonalInfo?loginUserName=" + HttpUtility.UrlEncode(userName) + "&patronBarcode=" + HttpUtility.UrlEncode(patron.barcode) + "\")'>";
            //summary += "<span style='font-size: 14.8px;font-weight:bold'>" + patron.barcode + "</span>"
            //    + "&nbsp;"
            //    + "<span style='font-size: 14.8px;font-weight:bold'>" + patron.name + "</span>";
            //if (String.IsNullOrEmpty(patron.department)==false)
            //    summary += "<span style='font-size: 14.8px;color:gray'>(" + patron.department + ")</span>";
            //summary += "&nbsp;&nbsp;>";
            //summary += "<div>";



            summary += "<ul class='mui-table-view'>"
    + "<li class='mui-table-view-cell'>"
            + "<a class='mui-navigate-right' href='../Patron/PersonalInfo?loginUserName=" + HttpUtility.UrlEncode(userName) + "&patronBarcode=" + HttpUtility.UrlEncode(patron.barcode) + "'>"
            + "<table class='simplepatron'>"
                + "<tbody>"
                    + "<tr>"
                        + "<td>"
                            + "<span class='barcode'>" + patron.barcode + "</span>" + "\n"
                            + "<span class='name'>" + patron.name + "</span>" + "\n";
            if (String.IsNullOrEmpty(patron.department) == false)
            {
                summary += "<span class='department'>(" + patron.department + ")</span>";
            }
            summary += "</td>"
                    + "</tr>"
                    + "<tr>"
                        + "<td>";

            if (patron.BorrowCountHtml != "")
            {
                summary += "<span>在借" + patron.BorrowCountHtml + "</span>" + "\n";
            }

            if (patron.CaoQiCount > 0)
            {
                summary += "<span class='mui-badge overdueNum'>" + patron.CaoQiCount + "</span>" + "\n"
                    + "<span class='rightText'>超期</span>" + "\n";

            }
            if (patron.OverdueCount > 0)
            {
                summary += "<span class='mui-badge amerceNum'>" + patron.OverdueCount + "</span>" + "\n"
                    + "<span class='rightText'>待交费</span>" + "\n";
            }

            //if (patron.ReservationCountHtml != "")
            //{
            //    summary += "<span>预约" + patron.ReservationCountHtml + "</span>" + "\n";
            //}

            if (patron.ArrivedCount > 0)
            {
                summary += "<span class='mui-badge arriveNum'>" + patron.ArrivedCount + "</span>" + "\n"
                + "<span class='rightText'>预约到书</span>" + "\n";
            }

            summary += "</td>"
                    + "</tr>"
                + "</tbody>"
            + "</table>"
        + "</a>"
    + "</li>"
+ "</ul>";


            return summary;
        }


        /// <summary>
        /// 获取读者的预约信息
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="patronBarcode"></param>
        /// <param name="reservations"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int GetPatronReservation(string libId,
            string userName,
            bool isPatron,
            string patronBarcode,
            out List<ReservationInfo> reservations,
            out string strError)
        {
            reservations = new List<ReservationInfo>();
            strError = "";

            string xml = "";
            string recPath = "";
            int nRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                userName,
                isPatron,
                patronBarcode,
                "xml",
                out recPath,
                out xml,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 0)
            {
                strError = "从dp2library未找到证条码号为'" + patronBarcode + "'的记录";
                return 0;
            }

            // 预约请求
            string strReservationWarningText = "";
            reservations = GetReservations(xml, out strReservationWarningText);

            return 1;
        }

        /*
        public BorrowInfoResult GetPatronBorrowInfos1(string libId,
            string readerBarcode)
        {
            BorrowInfoResult result = new BorrowInfoResult();
            result.errorCode = 0;
            result.errorInfo = "";

            LibEntity lib = this.GetLibById(libId);
            if (lib == null)
            {
                result.errorCode = -1;
                result.errorInfo = "未找到id为[" + libId + "]的图书馆定义。";
                return result;
            }


            string strError = "";

            string xml = "";
            string recPath = "";
            int nRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                readerBarcode,
                "advancexml",
                out recPath,
                out xml,
                out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
                return result;
            }

            if (nRet == 0)
            {
                result.errorCode = 0;
                result.errorInfo = "从dp2library未找到证条码号为'" + readerBarcode + "'的记录";
                return result;
            }


            //在借册
            string strBorrowWarningText = "";
            string maxBorrowCountString = "";
            string curBorrowCountString = "";
            List<BorrowInfo2> borrowList = GetBorrowInfo(xml, out strBorrowWarningText, out maxBorrowCountString, out curBorrowCountString);
            result.borrowInfos = borrowList;
            result.maxBorrowCount = maxBorrowCountString;
            result.curBorrowCount = curBorrowCountString;
            result.errorCode = 1;

            return result;
        }
        */
        public Patron ParsePatronXml(string libId, string strPatronXml, string recPath, int showPhoto)
        {
            // 取出个人信息
            Patron patron = new Patron();
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strPatronXml);

            // 证条码号
            string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            patron.barcode = strBarcode;

            // 显示名
            string strDisplayName = DomUtil.GetElementText(dom.DocumentElement, "displayName");
            patron.displayName = strDisplayName;

            // 姓名
            string strName = DomUtil.GetElementText(dom.DocumentElement, "name");
            patron.name = strName;

            // 性别
            string strGender = DomUtil.GetElementText(dom.DocumentElement, "gender");
            patron.gender = strGender;

            // 出生日期
            string strDateOfBirth = DomUtil.GetElementText(dom.DocumentElement, "dateOfBirth");
            if (string.IsNullOrEmpty(strDateOfBirth) == true)
                strDateOfBirth = DomUtil.GetElementText(dom.DocumentElement, "birthday");
            strDateOfBirth = DateTimeUtil.LocalDate(strDateOfBirth);
            patron.dateOfBirth = strDateOfBirth;

            // 证号 2008/11/11
            string strCardNumber = DomUtil.GetElementText(dom.DocumentElement, "cardNumber");
            patron.cardNumber = strCardNumber;

            // 身份证号
            string strIdCardNumber = DomUtil.GetElementText(dom.DocumentElement, "idCardNumber");
            patron.idCardNumber = strIdCardNumber;

            // 单位
            string strDepartment = DomUtil.GetElementText(dom.DocumentElement, "department");
            patron.department = strDepartment;

            // 职务
            string strPost = DomUtil.GetElementText(dom.DocumentElement, "post");
            patron.post = strPost;

            // 地址
            string strAddress = DomUtil.GetElementText(dom.DocumentElement, "address");
            patron.address = strAddress;

            // 电话
            string strTel = DomUtil.GetElementText(dom.DocumentElement, "tel");
            patron.tel = strTel;

            // email
            string strEmail = DomUtil.GetElementText(dom.DocumentElement, "email");
            patron.email = this.RemoveWeiXinId(strEmail);//过滤掉微信id

            // 读者类型
            string strReaderType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
            patron.readerType = strReaderType;

            // 证状态
            string strState = DomUtil.GetElementText(dom.DocumentElement, "state");
            patron.state = strState;

            // 发证日期
            string strCreateDate = DomUtil.GetElementText(dom.DocumentElement, "createDate");
            strCreateDate = DateTimeUtil.LocalDate(strCreateDate);
            patron.createDate = strCreateDate;

            // 证失效期
            string strExpireDate = DomUtil.GetElementText(dom.DocumentElement, "expireDate");
            strExpireDate = DateTimeUtil.LocalDate(strExpireDate);
            patron.expireDate = strExpireDate;

            // 租金 
            string strHireExpireDate = "";
            string strHirePeriod = "";
            XmlNode nodeHire = dom.DocumentElement.SelectSingleNode("hire");
            string strHire = "";
            if (nodeHire != null)
            {
                strHireExpireDate = DomUtil.GetAttr(nodeHire, "expireDate");
                strHirePeriod = DomUtil.GetAttr(nodeHire, "period");

                strHireExpireDate = DateTimeUtil.LocalDate(strHireExpireDate);
                strHirePeriod = dp2WeiXinService.GetDisplayTimePeriodStringEx(strHirePeriod);

                strHire = "周期" + ": " + strHirePeriod + "; "
                + "失效期" + ": " + strHireExpireDate;
            }
            patron.hire = strHire;

            // 押金 2008/11/11
            string strForegift = DomUtil.GetElementText(dom.DocumentElement,
                "foregift");
            patron.foregift = strForegift;

            // 二维码
            string qrcodeUrl = "../patron/getphoto?libId=" + HttpUtility.UrlEncode(libId)
                + "&type=pqri"
                + "&barcode=" + HttpUtility.UrlEncode(strBarcode);
            patron.qrcodeUrl = qrcodeUrl;

            //头像
            //recPath
            string imageUrl = "";
            if (showPhoto == 1)
            {
                //dprms:file
                // 看看是不是已经有图像对象
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("dprms", DpNs.dprms);
                // 全部<dprms:file>元素
                XmlNodeList fileNodes = dom.DocumentElement.SelectNodes("//dprms:file[@usage='cardphoto']", nsmgr);
                if (fileNodes.Count > 0)
                {
                    string strPhotoPath = recPath + "/object/" + DomUtil.GetAttr(fileNodes[0], "id");

                    imageUrl = "../patron/getphoto?libId=" + HttpUtility.UrlEncode(libId)
                    + "&objectPath=" + HttpUtility.UrlEncode(strPhotoPath);
                }
            }
            patron.imageUrl = imageUrl;


            // 违约
            List<OverdueInfo> overdueLit = new List<OverdueInfo>();
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("overdues/overdue");
            patron.OverdueCount = nodes.Count;
            patron.OverdueCountHtml = ConvertToString(patron.OverdueCount);

            // 在借
            nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
            patron.BorrowCount = nodes.Count;
            patron.BorrowCountHtml = ConvertToString(patron.BorrowCount);
            int caoQiCount = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strIsOverdue = DomUtil.GetAttr(node, "isOverdue");
                if (strIsOverdue == "yes")
                {
                    caoQiCount++;
                }
            }
            patron.CaoQiCount = caoQiCount;

            // 预约
            nodes = dom.DocumentElement.SelectNodes("reservations/request");
            patron.ReservationCount = nodes.Count;
            patron.ReservationCountHtml = ConvertToString(patron.ReservationCount);
            int arrivedCount = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string state = DomUtil.GetAttr(node, "state");
                if (state == "arrived")
                {
                    arrivedCount++;
                }
            }
            patron.ArrivedCount = arrivedCount;

            return patron;
        }

        private string ConvertToString(int num)
        {
            string text = "";
            if (num > 0 && num <= 5)
            {
                text = "<span class='leftNum'>" + "▪".PadRight(num, '▪') + "</span>";
            }
            else if (num == 0)
            {
                text = "";
            }
            else
            {
                text = num.ToString();
            }

            if (text != "")
                text = "(" + text + ")";
            return text;
        }

        private string RemoveWeiXinId(string email)
        {
            //<email>test@163.com,123,weixinid:o4xvUviTxj2HbRqbQb9W2nMl4fGg,weixinid:o4xvUvnLTg6NnflbYdcS-sxJCGFo,weixinid:testid</email>
            string[] emailList = email.Split(new char[] { ',' });
            string clearEmail = "";
            for (int i = 0; i < emailList.Length; i++)
            {
                string oneEmail = emailList[i].Trim();
                if (oneEmail.Length > 9 && oneEmail.Substring(0, 9) == WeiXinConst.C_WeiXinIdPrefix)
                {
                    continue;
                }

                if (clearEmail != "")
                    clearEmail += ",";

                clearEmail += oneEmail;
            }

            return clearEmail;
        }

        public List<BorrowInfo2> GetBorrowInfo(string strXml, out string strWarningText,
            out string maxBorrowCountString,
            out string curBorrowCountString)
        {
            strWarningText = "";
            maxBorrowCountString = "";
            curBorrowCountString = "";
            List<BorrowInfo2> borrowList = new List<BorrowInfo2>();

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);
            // ***
            // 在借册
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
            int nBorrowCount = nodes.Count;
            /*
<info>
<item name="可借总册数" value="10" />
<item name="日历名">
  <value>ILL1/新建日历</value>
</item>
<item name="当前还可借" value="10" />
</info>                  
             */
            XmlNode nodeMax = dom.DocumentElement.SelectSingleNode("info/item[@name='可借总册数']");
            if (nodeMax == null)
            {
                maxBorrowCountString = "获取当前读者可借总册数出错：未找到对应节点。";
            }
            else
            {
                string maxCount = DomUtil.GetAttr(nodeMax, "value");
                if (maxCount == "")
                {
                    maxBorrowCountString = "获取当前读者可借总册数出错：未设置对应值。";
                }
                else
                {
                    maxBorrowCountString = "最多可借:" + maxCount; ;
                    XmlNode nodeCurrent = dom.DocumentElement.SelectSingleNode("info/item[@name='当前还可借']");
                    if (nodeCurrent == null)
                    {
                        curBorrowCountString = "获取当前还可借出错：未找到对应节点。";
                    }
                    else
                    {
                        curBorrowCountString = "当前可借:" + DomUtil.GetAttr(nodeCurrent, "value");
                    }
                }
            }
            /*
              <borrows>
                <borrow barcode="C20" recPath="中文图书实体/10" biblioRecPath="中文图书/3" borrowDate="Mon, 28 Dec 2015 10:14:18 +0800" borrowPeriod="5day" returningDate="Sat, 02 Jan 2016 12:00:00 +0800" operator="ILL1-R002" type="童话" price="" isOverdue="yes" overdueInfo="已超过借阅期限 (2016年1月2日) 3 天。" overdueInfo1=" (已超期 3 天)" timeReturning="Sat, 02 Jan 2016 12:00:00 +0800" />
                <borrow barcode="C10" recPath="中文图书实体/3" biblioRecPath="中文图书/1" borrowDate="Mon, 28 Dec 2015 09:49:47 +0800" borrowPeriod="5day" returningDate="Sat, 02 Jan 2016 12:00:00 +0800" operator="ILL1-R002" type="童话" price="15" isOverdue="yes" overdueInfo="已超过借阅期限 (2016年1月2日) 3 天。" overdueInfo1=" (已超期 3 天)" timeReturning="Sat, 02 Jan 2016 12:00:00 +0800" />
                <borrow barcode="C32" recPath="中文图书实体/13" biblioRecPath="中文图书/2" borrowDate="Mon, 28 Dec 2015 04:15:39 +0800" borrowPeriod="5day" returningDate="Sat, 02 Jan 2016 12:00:00 +0800" operator="supervisor" type="童话" price="" isOverdue="yes" overdueInfo="已超过借阅期限 (2016年1月2日) 3 天。" overdueInfo1=" (已超期 3 天)" timeReturning="Sat, 02 Jan 2016 12:00:00 +0800" />
                <borrow barcode="C33" recPath="中文图书实体/14" biblioRecPath="中文图书/2" borrowDate="Mon, 28 Dec 2015 04:14:43 +0800" borrowPeriod="5day" returningDate="Sat, 02 Jan 2016 12:00:00 +0800" operator="supervisor" type="音乐" price="" isOverdue="yes" overdueInfo="已超过借阅期限 (2016年1月2日) 3 天。" overdueInfo1=" (已超期 3 天)" timeReturning="Sat, 02 Jan 2016 12:00:00 +0800" />
                <borrow barcode="C31" recPath="中文图书实体/12" biblioRecPath="中文图书/2" borrowDate="Mon, 28 Dec 2015 04:14:39 +0800" borrowPeriod="5day" returningDate="Sat, 02 Jan 2016 12:00:00 +0800" operator="supervisor" type="童话" price="" isOverdue="yes" overdueInfo="已超过借阅期限 (2016年1月2日) 3 天。" overdueInfo1=" (已超期 3 天)" timeReturning="Sat, 02 Jan 2016 12:00:00 +0800" />
                <borrow barcode="C23" recPath="中文图书实体/9" biblioRecPath="中文图书/3" borrowDate="Mon, 28 Dec 2015 04:14:36 +0800" borrowPeriod="5day" returningDate="Sat, 02 Jan 2016 12:00:00 +0800" operator="supervisor" type="童话" price="" isOverdue="yes" overdueInfo="已超过借阅期限 (2016年1月2日) 3 天。" overdueInfo1=" (已超期 3 天)" timeReturning="Sat, 02 Jan 2016 12:00:00 +0800" />
              </borrows>
            */
            int nOverdueCount = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                //借阅基本信息
                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strBorrowDate = DateTimeUtil.LocalDate(DomUtil.GetAttr(node, "borrowDate"));
                string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                string strOperator = DomUtil.GetAttr(node, "operator");
                string strTimeReturning = DateTimeUtil.LocalDate(DomUtil.GetAttr(node, "timeReturning"));

                // 续借信息
                string strRenewNo = DomUtil.GetAttr(node, "no"); // 续借次数
                string strRenewComment = DomUtil.GetAttr(node, "renewComment");

                string rowCss = "";
                string strOverdueInfo = DomUtil.GetAttr(node, "overdueInfo");
                string strOverdue1 = DomUtil.GetAttr(node, "overdueInfo1");
                bool bOverdue = false;
                string strIsOverdue = DomUtil.GetAttr(node, "isOverdue");
                if (strIsOverdue == "yes")
                {
                    bOverdue = true;
                    nOverdueCount++;

                    strTimeReturning += strOverdue1;

                    rowCss = "borrowinfo-overdue";
                }

                // 创建 borrowinfo对象，加到集合里
                BorrowInfo2 borrowInfo = new BorrowInfo2();
                borrowInfo.barcode = strBarcode;
                borrowInfo.renewNo = strRenewNo;
                borrowInfo.borrowDate = strBorrowDate;
                borrowInfo.period = strPeriod;
                borrowInfo.borrowOperator = strOperator;
                borrowInfo.renewComment = strRenewComment;
                borrowInfo.overdue = strOverdueInfo;
                borrowInfo.returnDate = strTimeReturning;
                //if (string.IsNullOrEmpty(this.opacUrl) == false)
                //    borrowInfo.barcodeUrl = this.opacUrl + "/book.aspx?barcode=" + strBarcode;
                //else
                //    borrowInfo.barcodeUrl = "";
                borrowInfo.barcodeUrl = "";
                borrowInfo.rowCss = rowCss;
                borrowList.Add(borrowInfo);

            }
            if (nOverdueCount > 0)
                strWarningText += "<div class='warning overdue'><div class='number'>" + nOverdueCount.ToString() + "</div><div class='text'>已超期</div></div>";

            return borrowList;
        }


        public List<OverdueInfo> GetOverdueInfo(string strXml, out string strWarningText)
        {
            strWarningText = "";
            List<OverdueInfo> overdueList = new List<OverdueInfo>();

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

            // ***
            // 违约/交费信息
            List<OverdueInfo> overdueLit = new List<OverdueInfo>();
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("overdues/overdue");
            if (nodes.Count > 0)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];
                    string strBarcode = DomUtil.GetAttr(node, "barcode");
                    string strOver = DomUtil.GetAttr(node, "reason");
                    string strBorrowPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                    strBorrowPeriod = dp2WeiXinService.GetDisplayTimePeriodStringEx(strBorrowPeriod);
                    string strBorrowDate = LocalDateOrTime(DomUtil.GetAttr(node, "borrowDate"), strBorrowPeriod);
                    string strReturnDate = LocalDateOrTime(DomUtil.GetAttr(node, "returnDate"), strBorrowPeriod);
                    string strID = DomUtil.GetAttr(node, "id");
                    string strPrice = DomUtil.GetAttr(node, "price");
                    string strOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");
                    string strComment = DomUtil.GetAttr(node, "comment");
                    if (String.IsNullOrEmpty(strComment) == true)
                        strComment = "&nbsp;";
                    string strPauseInfo = "";
                    // 把一行文字变为两行显示
                    //strBorrowDate = strBorrowDate.Replace(" ", "<br/>");
                    //strReturnDate = strReturnDate.Replace(" ", "<br/>");

                    OverdueInfo overdueInfo = new OverdueInfo();
                    overdueInfo.barcode = strBarcode;
                    //if (string.IsNullOrEmpty(this.opacUrl) == false)
                    //    overdueInfo.barcodeUrl = this.opacUrl + "/book.aspx?barcode=" + strBarcode;
                    //else
                    //    overdueInfo.barcodeUrl = "";
                    overdueInfo.barcodeUrl = "";
                    overdueInfo.reason = strOver;
                    overdueInfo.price = strPrice;
                    overdueInfo.pauseInfo = strPauseInfo;
                    overdueInfo.borrowDate = strBorrowDate;
                    overdueInfo.borrowPeriod = strBorrowPeriod;
                    overdueInfo.returnDate = strReturnDate;
                    overdueInfo.comment = strComment;
                    overdueLit.Add(overdueInfo);
                }

                strWarningText += "<div class='warning amerce'><div class='number'>" + nodes.Count.ToString() + "</div><div class='text'>待交费</div></div>";
            }

            return overdueLit;
        }

        public List<ReservationInfo> GetReservations(string strXml, out string strWarningText)
        {
            strWarningText = "";
            List<ReservationInfo> reservationList = new List<ReservationInfo>();

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

            // ***
            // 预约请求
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("reservations/request");
            if (nodes.Count > 0)
            {
                int nArriveCount = 0;
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];

                    string strBarcodes = DomUtil.GetAttr(node, "items");
                    string strRequestDate = DateTimeUtil.LocalTime(DomUtil.GetAttr(node, "requestDate"));

                    string strOperator = DomUtil.GetAttr(node, "operator");
                    string strArrivedItemBarcode = DomUtil.GetAttr(node, "arrivedItemBarcode");

                    int nBarcodesCount = GetBarcodesCount(strBarcodes);
                    // 状态
                    string strArrivedDate = DateTimeUtil.LocalTime(DomUtil.GetAttr(node, "arrivedDate"));
                    string strState = DomUtil.GetAttr(node, "state");
                    string strStateText = "";
                    if (strState == "arrived")
                    {
                        strStateText = "册 " + strArrivedItemBarcode + " 已于 " + strArrivedDate + " 到书";

                        if (nBarcodesCount > 1)
                        {
                            strStateText += string.Format("; 同一预约请求中的其余 {0} 册旋即失效",  // "；同一预约请求中的其余 {0} 册旋即失效"
                                (nBarcodesCount - 1).ToString());
                        }

                        nArriveCount++;
                    }

                    string strBarcodesHtml = MakeBarcodeListHyperLink(strBarcodes, strArrivedItemBarcode, ", ")
                     + (nBarcodesCount > 1 ? " 之一" : "");

                    ReservationInfo reservationInfo = new ReservationInfo();
                    reservationInfo.pureBarcodes = strBarcodes;
                    reservationInfo.barcodes = strBarcodesHtml;
                    reservationInfo.state = strState;
                    reservationInfo.stateText = strStateText;
                    reservationInfo.requestdate = strRequestDate;
                    reservationInfo.operatorAccount = strOperator;
                    reservationInfo.arrivedBarcode = strArrivedItemBarcode;
                    reservationInfo.fullBarcodes = strBarcodes + "*" + strArrivedItemBarcode;
                    reservationList.Add(reservationInfo);
                }


                if (nArriveCount > 0)
                    strWarningText = "<div class='warning arrive'><div class='number'>" + nArriveCount.ToString() + "</div><div class='text'>预约到书</div></div>";
            }

            return reservationList;
        }
        int GetBarcodesCount(string strBarcodes)
        {
            string[] barcodes = strBarcodes.Split(new char[] { ',' });

            return barcodes.Length;
        }

        string MakeBarcodeListHyperLink(string strBarcodes,
            string strArrivedItemBarcode,
            string strSep)
        {
            string strResult = "";
            string strDisableClass = "";
            if (string.IsNullOrEmpty(strArrivedItemBarcode) == false)
                strDisableClass = "deleted";
            string[] barcodes = strBarcodes.Split(new char[] { ',' });
            for (int i = 0; i < barcodes.Length; i++)
            {
                string strBarcode = barcodes[i];
                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

                if (strResult != "")
                    strResult += strSep;

                //strResult += "<a "
                //    + (string.IsNullOrEmpty(strDisableClass) == false && strBarcode != strArrivedItemBarcode ? "class='" + strDisableClass + "'" : "")
                //    + " href='"+this.opacUrl+ "/book.aspx?barcode=" + strBarcode+"'  target='_blank' " + ">" + strBarcode + "</a>";  

                strResult += "<a "
    + (string.IsNullOrEmpty(strDisableClass) == false && strBarcode != strArrivedItemBarcode ? "class='" + strDisableClass + "'" : "")
    + " href='#'" + ">" + strBarcode + "</a>";

            }

            return strResult;
        }

        // return:
        //      -1  检测过程发生了错误。应当作不能借阅来处理
        //      0   可以借阅
        //      1   证已经过了失效期，不能借阅
        //      2   证有不让借阅的状态
        public static int CheckReaderExpireAndState(XmlDocument readerdom,
            out string strError)
        {
            strError = "";

            string strExpireDate = DomUtil.GetElementText(readerdom.DocumentElement, "expireDate");
            if (String.IsNullOrEmpty(strExpireDate) == false)
            {
                DateTime expireDate;
                try
                {
                    expireDate = DateTimeUtil.FromRfc1123DateTimeString(strExpireDate);
                }
                catch
                {
                    strError = string.Format("借阅证失效期值s格式错误", // "借阅证失效期<expireDate>值 '{0}' 格式错误"
                        strExpireDate);

                    // "借阅证失效期<expireDate>值 '" + strExpireDate + "' 格式错误";
                    return -1;
                }

                DateTime now = DateTime.Now;//todo//this.Clock.UtcNow;
                if (expireDate <= now)
                {
                    // text-level: 用户提示
                    strError = string.Format("今天s已经超过借阅证失效期s",  // "今天({0})已经超过借阅证失效期({1})。"
                        now.ToLocalTime().ToLongDateString(),
                        expireDate.ToLocalTime().ToLongDateString());
                    return 1;
                }

            }

            string strState = DomUtil.GetElementText(readerdom.DocumentElement, "state");
            if (String.IsNullOrEmpty(strState) == false)
            {
                strError = string.Format("借阅证的状态为s", // "借阅证的状态为 '{0}'。"
                    strState);
                return 2;
            }

            return 0;
        }
        /// <summary>
        /// 得到的读者的联系方式
        /// </summary>
        /// <param name="dom"></param>
        /// <returns></returns>
        private static string GetContactString(XmlDocument dom)
        {
            string strTel = DomUtil.GetElementText(dom.DocumentElement, "tel");
            string strEmail = DomUtil.GetElementText(dom.DocumentElement, "email");
            string strAddress = DomUtil.GetElementText(dom.DocumentElement, "address");
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(strTel) == false)
                list.Add(strTel);
            if (string.IsNullOrEmpty(strEmail) == false)
            {
                strEmail = "";// JoinEmail(strEmail, "");
                list.Add(strEmail);
            }
            if (string.IsNullOrEmpty(strAddress) == false)
                list.Add(strAddress);
            return StringUtil.MakePathList(list, "; ");
        }

        /// <summary>
        /// 获取读者信息
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="patronBarocde"></param>
        /// <param name="format"></param>
        /// <param name="xml"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int GetPatronXml(string libId,
            string userName,
            bool isPatron,
            string patronBarocde,  //todo refID
            string format,
            out string recPath,
            out string xml,
            out string strError)
        {
            xml = "";
            strError = "";
            recPath = "";

            LibEntity lib = this.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                goto ERROR1;
            }

            // 使用传进来的账户，有可以是工作人员，也有可以是读者自己
            LoginInfo loginInfo = new LoginInfo(userName, isPatron);

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
                loginInfo,
                "getPatronInfo",
                "",
                patronBarocde,
                "",
                "",
                "",
                format,
                1,
                0,
                -1);

            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;

                SearchResult result = connection.SearchTaskAsync(
                    lib.capoUserName,
                    request,
                    new TimeSpan(0,0,15),  //改为15秒
                    cancel_token).Result;

                if (result.ResultCount == -1)
                {
                    bool bOffline = false;
                    strError = this.GetFriendlyErrorInfo(result, lib.libName, out bOffline);//result.ErrorInfo;
                    return -1;
                }

                if (result.ResultCount == 0)
                {
                    strError = result.ErrorInfo;
                    return 0;
                }
                string path = result.Records[0].RecPath;
                int nIndex = path.IndexOf("@");
                path = path.Substring(0, nIndex);
                recPath = path;
                xml = result.Records[0].Data;
                return 1;

            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

        ERROR1:
            //WriteErrorLog("4返回-1，返回errorinfo:" + strError);

            return -1;
        }

        public string GetPurePath(string strBiblioPath)
        {
            int tempIndex = strBiblioPath.IndexOf("@");
            if (tempIndex > 0)
                return strBiblioPath.Substring(0, tempIndex);

            return strBiblioPath;
        }


        /// <summary>
        /// 根据code获取微信id
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="weixinId"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int GetWeiXinId(string code, string state,
            out GzhCfg gzh,
            out string weixinId,
            out List<string> libList,
            out string strError)
        {
            strError = "";
            weixinId = "";
            gzh = null;
            libList = new List<string>();

            int nRet = this.GetGzhAndLibs(state,
                out gzh,
                out libList,
                out strError);
            if (nRet == -1)
                return -1;

            try
            {
                //用code换取access_token
                var result = OAuthApi.GetAccessToken(gzh.appId, gzh.secret, code);//this.weiXinAppId, this.weiXinSecret, code);
                if (result == null)
                {
                    strError = "GetAccessToken()返回的result为null。";
                    return -1;
                }
                if (result.errcode != ReturnCode.请求成功)
                {
                    strError = "获取微信id出错：" + result.errmsg;
                    return -1;
                }

                // 取出微信id
                weixinId = result.openid + "@" + gzh.appId; //2016-11-16，系统中使用的微信id都带上@appId
                return 0;
            }
            catch (Exception ex)
            {
                strError = "获取微信id异常：" + ex.Message;
                return -1;
            }
        }

        public int GetGzhAndLibs(string state, 
            out GzhCfg gzh, 
            out List<string> libList,
            out string strError)
        {
            strError = "";
            gzh = null;
            libList = new List<string>();

            if (String.IsNullOrEmpty(state) == true)
            {
                strError = "state参数为空";
                return -1;
            }

            // 2016-11-22
            //state格式：公众号名称:图书馆capo账户1+图书馆capo账户2
            int nIndex = state.IndexOf(":");
            string gzhName = state;
            string libCapoNames = "";
            string[] libs = null;
            if (nIndex != -1)
            {
                gzhName = state.Substring(0, nIndex);
                libCapoNames = state.Substring(nIndex + 1);
                if (libCapoNames != "")
                {
                    libs = libCapoNames.Split(new char[] { ',' });
                }
            }

            // 根据传进来的参数，得到公众号配置信息
            gzh = this.gzhContainer.GetByAppName(gzhName); //函数内会处理空的情况
            if (gzh == null)
            {
                strError = "验证失败：非正规途径[" + state + "]进入！";
                return -1;
            }

            ////根据state确定是哪个公众号进来的
            //if (state != C_gzh_ilovelibrary && state != C_gzh_dp && state != C_gzh_dpcomm)
            //{
            //    strError = "验证失败！非正规途径["+state+"]进入！";
            //    return -1;
            //}
            //gzh = this.gzhContainer.GetByAppName(state);

            //得到可以访问的图书馆列表
            if (libs == null || libs.Length == 0)
            {
                //未配时，全部图书馆
                foreach (Library lib in this.LibManager.Librarys)
                {
                    libList.Add(lib.Entity.id);
                }
            }
            else
            {

                foreach (Library lib in this.LibManager.Librarys)
                {
                    if (libs.Contains(lib.Entity.capoUserName) ==true)
                        libList.Add(lib.Entity.id);
                }

                /*
                foreach (string capoName in libs)
                {
                    Library lib = this.LibManager.GetLibraryByCapoName(capoName);
                    if (lib == null)
                    {
                        strError = "验证失败：存在不合理的图书馆名称["+capoName+"]！";
                        return -1;
                    }
                    libList.Add(lib.Entity.id);
                }
                 */
            }

            // 未设置要访问的图书馆
            if (libList.Count == 0)
            {
                strError = "验证失败：未设置要访问的图书馆";
                return -1;
            }
            return 0;
        }

        ///// <summary>
        ///// 发送客服消息
        ///// </summary>
        ///// <param name="openId"></param>
        ///// <param name="text"></param>
        //public void SendCustomerMsg(string openId, string text)
        //{
        //    var accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);
        //    CustomApi.SendText(accessToken, openId, "error");
        //}



        // 分析期限参数
        public static int ParsePeriodUnit(string strPeriod,
            out long lValue,
            out string strUnit,
            out string strError)
        {
            lValue = 0;
            strUnit = "";
            strError = "";

            strPeriod = strPeriod.Trim();

            if (String.IsNullOrEmpty(strPeriod) == true)
            {
                strError = "期限字符串为空";
                return -1;
            }

            string strValue = "";


            for (int i = 0; i < strPeriod.Length; i++)
            {
                if (strPeriod[i] >= '0' && strPeriod[i] <= '9')
                {
                    strValue += strPeriod[i];
                }
                else
                {
                    strUnit = strPeriod.Substring(i).Trim();
                    break;
                }
            }

            // 将strValue转换为数字
            try
            {
                lValue = Convert.ToInt64(strValue);
            }
            catch (Exception)
            {
                strError = "期限参数数字部分'" + strValue + "'格式不合法";
                return -1;
            }

            if (String.IsNullOrEmpty(strUnit) == true)
                strUnit = "day";   // 缺省单位为"天"

            strUnit = strUnit.ToLower();    // 统一转换为小写

            return 0;
        }

        #endregion



        #region 续借

        /*
        public int CirculationByWorker(string libId,
            string userName,
            bool isPatron,
            string operation,
            string patron,
            string item,
            out string patronBarcode,
            out string strError)
        {
            strError = "";
            patronBarcode = "";

            ReturnInfo resultInfo = null;



            int nRet= Circulation(libId,
                userName,
                isPatron,
                operation,
                patron,
                item,
                out patronBarcode,
                out resultInfo,
                
                out strError);

            return nRet;

        }
        */
        public int Circulation(string libId,
            string userName,
            bool isPatron,
            string operation,
            string patron,
            string item,
            out string patronBarcode,
            out string patronXml,
            out ReturnInfo resultInfo,
            out string strError)
        {
            strError = "";
            resultInfo = null;
            patronBarcode = "";
            patronXml = "";

            LibEntity lib = this.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                return -1;
            }

            // 使用的登录账号 20161025 jane
            LoginInfo loginInfo = new LoginInfo(userName, isPatron);

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            CirculationRequest request = new CirculationRequest(id,
                loginInfo,
                operation,
                patron,
                item,
                "reader",  //style
                "xml",
                "",
                "");
            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;

                CirculationResult result = connection.CirculationTaskAsync(
                    lib.capoUserName,
                    request,
                    new TimeSpan(0, 1, 10), // 10 秒
                    cancel_token).Result;

                resultInfo = result.ReturnInfo;
                patronBarcode = result.PatronBarcode;
                if (result.PatronResults != null && result.PatronResults.Count >0)
                    patronXml = result.PatronResults[0];
                strError = result.ErrorInfo;

                if (result.Value == -1)
                {
                    strError = this.GetFriendlyErrorInfo(result, lib.libName);
                    return -1;
                }


                return (int)result.Value;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                return -1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }

        public int Renew1(string libId,
            string patron,
            string item,
            out string strError)
        {
            strError = "";

            LibEntity lib = this.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                return -1;
            }

            // 使用读者账号 20161024 jane
            LoginInfo loginInfo = new LoginInfo(patron, true);

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            CirculationRequest request = new CirculationRequest(id,
                loginInfo,
                "renew",
                patron,
                item,
                "",
                "",
                "",
                "");
            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;

                CirculationResult result = connection.CirculationTaskAsync(
                    lib.capoUserName,
                    request,
                    new TimeSpan(0, 1, 10), // 10 秒
                    cancel_token).Result;
                if (result.Value == -1)
                {
                    strError = this.GetFriendlyErrorInfo(result, lib.libName);
                    return -1;
                }

                strError = result.ErrorInfo;
                return (int)result.Value;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                return -1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }


        #endregion

        #region 预约

        public int Reservation(string weixinId,
            string libId,
            string patron,
            string items,
            string style,
            out string reserRowHtml,
            out string strError)
        {
            strError = "";
            reserRowHtml = "";

            LibEntity lib = this.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                return -1;
            }

            // 使用读者账号 20161024 jane
            LoginInfo loginInfo = new LoginInfo(patron, true);

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            CirculationRequest request = new CirculationRequest(id,
                loginInfo,
                "reservation",
                patron,
                items,
                style,
                "",
                "",
                "");
            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;

                CirculationResult result = connection.CirculationTaskAsync(
                    lib.capoUserName,
                    request,
                    new TimeSpan(0, 1, 10), // 10 秒
                    cancel_token).Result;
                if (result.Value == -1)
                {
                    strError = this.GetFriendlyErrorInfo(result, lib.libName);
                    return -1;
                }
                else
                {
                    strError = result.ErrorInfo;
                }



                if (style == "delete")
                {
                    reserRowHtml = this.getReservationHtml("未预约", items, true, true);
                }
                else if (style == "new")
                {
                    // 根据result.ErrorInfo区分是否到书 todo这个区分可靠吗？
                    if (strError != "")
                        reserRowHtml = this.getReservationHtml("已到书", items, true, true);
                    else
                        reserRowHtml = this.getReservationHtml("已预约", items, true, true);
                }


                // 取消预约，发送微信通知
                if (style == "delete")
                {
                    try
                    {
                        WxUserItem user = WxUserDatabase.Current.GetActivePatron(weixinId, libId);
                        if (user == null)
                        {
                            strError = "取消预约时，不可能找不到当前绑定的读者账户";
                            return -1;
                        }

                        string operTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                        string fullPatronName = this.GetFullPatronName(user.readerName, user.readerBarcode, lib.libName, user.libraryCode, false);
                        string strText = fullPatronName + "，您已对图书 " + items + " 取消预约,该书将不再为您保留。";
                        string remark = "\n" + this._msgRemark;

                        List<string> bindWeixinIds = new List<string>();
                        string fullWeixinId = weixinId;//2016-11-16 传进来的weixinId带了@appId // + "@" + user.appId;
                        bindWeixinIds.Add(fullWeixinId);

                        // 得到找开tracing功能的工作人员微信id
                        List<TracingOnUser> workers = this.getWorkerWeixinIds(lib.id, user.libraryCode);


                        //{{first.DATA}}
                        //标题：{{keyword1.DATA}}
                        //时间：{{keyword2.DATA}}
                        //内容：{{keyword3.DATA}}
                        //{{remark.DATA}}
                        string first = "☀☀☀☀☀☀☀☀☀☀";
                        string first_color = "#9400D3";
                        string title="取消预约成功";
                        MessageTemplateData msgData = new MessageTemplateData(first,
                            first_color,
                            title,
                            operTime,
                            strText,
                            remark);
                        //mask
                        string markFullPatronName = this.GetFullPatronName(user.readerName, user.readerBarcode, lib.libName, user.libraryCode, true);
                        strText = strText.Replace(fullPatronName, markFullPatronName);
                        MessageTemplateData maskMsgData = new MessageTemplateData(first,
                            first_color,
                            title,
                            operTime,
                            strText,
                            remark);

                        int nRet = this.SendTemplateMsg(GzhCfg.C_Template_Message,
                            bindWeixinIds,
                            workers,
                            msgData,
                            maskMsgData,
                            "",
                            "",
                            out strError);
                        if (nRet == -1)
                            return -1;

                    }
                    catch (Exception ex)
                    {
                        this.WriteErrorLog1("给读者" + patron + "发送'取消预约成功'通知异常：" + ex.Message);
                    }
                }

                strError = result.ErrorInfo;
                return (int)result.Value;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                return -1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }


        #endregion


        #region 静态函数

        // 把整个字符串中的时间单位变换为可读的形态
        public static string GetDisplayTimePeriodStringEx(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";
            strText = strText.Replace("day", "天");
            return strText.Replace("hour", "小时");
        }

        // 根据strPeriod中的时间单位(day/hour)，返回本地日期或者时间字符串
        // parameters:
        //      strPeriod   原始格式的时间长度字符串。也就是说，时间单位不和语言相关，是"day"或"hour"
        public static string LocalDateOrTime(string strTimeString,
            string strPeriod)
        {
            string strError = "";
            long lValue = 0;
            string strUnit = "";
            int nRet = ParsePeriodUnit(strPeriod,
                        out lValue,
                        out strUnit,
                        out strError);
            if (nRet == -1)
                strUnit = "day";
            if (strUnit == "day")
                return DateTimeUtil.LocalDate(strTimeString);

            return DateTimeUtil.LocalTime(strTimeString);
        }





        #endregion

        #region 错误日志

        void writeDebug(string strText)
        {
            this.WriteLog("debug:" + strText, 1);
        }

        public void WriteErrorLog1(string strText)
        {
            this.WriteLog("ERROR:" + strText, 1);
        }

        public void WriteLog1(string strText)
        {
            this.WriteLog(strText, 1);
        }

        public void WriteLog3(string strText)
        {
            this.WriteLog(strText, 3);
        }

        public void WriteLog2(string strText)
        {
            this.WriteLog(strText, 2);
        }

        public void WriteLog(string strText, int logLevel)
        {
            // todo 有空比对下谢老师写日志的代码
            //DateTime now = DateTime.Now;
            //// 每天一个日志文件
            //string strFilename = Path.Combine(this.LogDir, "log_" + DateTimeUtil.DateTimeToString8(now) + ".txt");
            //string strTime = now.ToString();
            //FileUtil.WriteText(strFilename,
            //    strTime + " " + strText + "\r\n");

            if (logLevel <= this.LogLevel)
            {
                var logDir = this.weiXinLogDir;
                string strFilename = string.Format(logDir + "/log_{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
                FileUtil.WriteLog(strFilename, strText, "dp2weixin");
            }
        }


        /// <summary>
        /// 获得异常信息
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <returns></returns>
        public static string GetExceptionMessage(Exception ex)
        {
            string strResult = ex.GetType().ToString() + ":" + ex.Message;
            while (ex != null)
            {
                if (ex.InnerException != null)
                    strResult += "\r\n" + ex.InnerException.GetType().ToString() + ": " + ex.InnerException.Message;

                ex = ex.InnerException;
            }

            return strResult;
        }

        #endregion

        #region 二维码
        public int GetQRcode(string libId,
            string patronBarcode,
            out string code,
            out string strError)
        {
            strError = "";
            code = "";

            LibEntity lib = this.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                goto ERROR1;
            }

            //Operation:verifyPassword
            //Patron: !getpatrontempid:R0000001
            //Item:
            //ResultValue返回1表示成功，-1或0表示不成功。
            //成功的情况下，ErrorInfo成员里面返回了二维码字符串，形如” PQR:R0000001@00JDURE5FT1JEOOWGJV0R1JXMYI”

            string patron = "!getpatrontempid:" + patronBarcode;


            // 使用读者账号 20161024 jane
            LoginInfo loginInfo = new LoginInfo(patronBarcode, true);

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            CirculationRequest request = new CirculationRequest(id,
                loginInfo,
                "verifyPassword",
                patron,
                "",
                "",//this.textBox_circulation_style.Text,
                "",//this.textBox_circulation_patronFormatList.Text,
                "",//this.textBox_circulation_itemFormatList.Text,
                "");//this.textBox_circulation_biblioFormatList.Text);
            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;
                CirculationResult result = connection.CirculationTaskAsync(
                    lib.capoUserName,
                    request,
                    new TimeSpan(0, 1, 10), // 10 秒
                    cancel_token).Result;
                if (result.Value == -1 || result.Value == 0)
                {
                    strError = this.GetFriendlyErrorInfo(result, lib.libName); //"出错：" + result.ErrorInfo;
                    return (int)result.Value;
                }

                // 成功
                code = result.ErrorInfo;
                return (int)result.Value;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }


        ERROR1:
            return -1;
        }

        public void GetQrImage(
    string strCode,
    int nWidth,
    int nHeight,
            Stream outputStream,
    out string strError)
        {
            strError = "";
            try
            {
                if (nWidth <= 0)
                    nWidth = 300;
                if (nHeight <= 0)
                    nHeight = 300;

                MultiFormatWriter writer = new MultiFormatWriter(); //BarcodeWriter
                ByteMatrix bm = writer.encode(strCode, BarcodeFormat.QR_CODE, nWidth, nHeight);// 300, 300);
                using (Bitmap img = bm.ToBitmap())
                {
                    //MemoryStream ms = new MemoryStream();
                    img.Save(outputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    //return ms;
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                //return null;
            }
        }

        /// <summary>
        /// 根据文字生成图片
        /// </summary>
        /// <param name="strError"></param>
        /// <returns></returns>
        public MemoryStream GetErrorImg(string strError)
        {
            Bitmap bmp = new Bitmap(210, 110);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);


            Font font = SystemFonts.DefaultFont;
            Brush fontBrush = Brushes.Red;// SystemBrushes.ControlText;
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            Rectangle rect = new Rectangle(2, 2, 200, 100);
            g.DrawString(strError, font, fontBrush, rect, sf);

            //g.FillRectangle(Brushes.Red, 2, 2, 100, 100);
            //g.DrawString(strError, new Font("微软雅黑", 10f), Brushes.Black, new PointF(5f, 5f));

            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            g.Dispose();
            bmp.Dispose();

            return ms;
        }


        public int GetObjectMetadata(string libId,
            string objectPath,
            string style,
            Stream outputStream,
            out string metadata,
            out string timestamp,
            out string outputpath,
            out string strError)
        {
            strError = "";
            metadata = "";
            timestamp = "";
            outputpath = "";

            LibEntity lib = this.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                goto ERROR1;
            }

            // 使用代理账号capo 20161024 jane
            LoginInfo loginInfo = new LoginInfo("", false);

            CancellationToken cancel_token = new CancellationToken();

            string id = Guid.NewGuid().ToString();
            GetResRequest request = new GetResRequest(id,
                loginInfo,
                "getRes",
                objectPath,
                0,
                -1,
                style);
            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;
                GetResResponse result = connection.GetResTaskAsync(
                   lib.capoUserName,
                    request,
                    outputStream,
                    null,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;

                if (String.IsNullOrEmpty(result.ErrorCode) == false)
                {
                    strError = "调GetRes出错：" + result.ErrorInfo;
                    return -1;
                }

                if (result.TotalLength == -1)
                {
                    strError = "调GetRes出错：" + result.ErrorInfo;
                    return -1;
                }

                // 成功
                metadata = result.Metadata;
                timestamp = result.Timestamp;
                outputpath = result.Path;
                return 0;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }


        ERROR1:
            return -1;
        }




        #endregion

        #region 消息：公告，好书，图书馆介绍

        public int GetMessage(string group,
            string libId,
             string msgId,
            string subjectCondition,
            string style,
            out List<MessageItem> list,
            out string strError)
        {
            list = new List<MessageItem>();
            strError = "";



            List<MessageRecord> records = new List<MessageRecord>();
            int nRet = this.GetMessageInternal("", // action
                group,
                libId,
                msgId,
                subjectCondition,
                out records,
                out strError);
            if (nRet == -1)
                return -1;

            foreach (MessageRecord record in records)
            {
                MessageItem item = ConvertMsgRecord(group, record, style, libId);
                list.Add(item);
            }

            return nRet;
        }

        public MessageItem ConvertMsgRecord(string group,
            MessageRecord record,
            string style,
            string libId)
        {

            MessageItem item = new MessageItem();
            item.id = record.id;
            item.publishTime = DateTimeUtil.DateTimeToString(record.publishTime);
            item.subject = "";
            if (record.subjects != null && record.subjects.Length > 0)
            {
                item.subject = record.subjects[0];
                item.subject = StringUtil.UnescapeString(item.subject);
            }
            string title = "";
            string content = "";
            string format = "text"; //默认是text样式
            string creator = "";
            string remark = "";

            string xml = record.data;
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
                XmlNode root = dom.DocumentElement;
                XmlNode nodeTitle = root.SelectSingleNode("title");
                if (nodeTitle != null)
                    title = nodeTitle.InnerText;

                XmlNode nodeContent = root.SelectSingleNode("content");
                if (nodeTitle != null)
                    content = nodeContent.InnerText;

                XmlNode nodeRemark = root.SelectSingleNode("remark");
                if (nodeRemark != null)
                    remark = nodeRemark.InnerText;

                format = DomUtil.GetAttr(nodeContent, "format");
                if (format == "")
                    format = "text";

                XmlNode nodeCreator = root.SelectSingleNode("creator");
                if (nodeCreator != null)
                    creator = DomUtil.GetNodeText(nodeCreator);
            }
            catch
            {
                title = "不符合格式的消息";
                content = "不符合格式的消息-" + xml;
            }

            item.title = title;
            item.contentFormat = format;
            item.creator = creator;
            item.content = "";
            item.remark = remark;

            if (style == "original")
            {
                item.content = content;
            }
            else if (style == "browse")
            {
                item.content = "";

                string contentHtml = "";
                if (group == C_Group_Bb || group == C_Group_HomePage || group == C_Group_dp_home)
                {
                    contentHtml = GetMsgHtml(format, content, libId);
                }
                else if (group == C_Group_Book)
                {
                    contentHtml = GetBookHtml(content, libId);

                }
                item.contentHtml = contentHtml;

                if (String.IsNullOrEmpty(item.remark) == false)
                {
                    item.remarkHtml = GetMsgHtml("text", item.remark, libId);
                }
            }
            return item;
        }

        public string GetMsgHtml(string format, string content, string libId)
        {
            string contentHtml = "";
            if (format == "markdown")
            {
                contentHtml = CommonMark.CommonMarkConverter.Convert(content);
            }
            else
            {
                //普通text 处理换行
                //contentHtml = HttpUtility.HtmlEncode(content);
                //contentHtml = contentHtml.Replace("\r\n", "\n");
                //contentHtml = contentHtml.Replace("\n", "<br/>");

                content = content.Replace("\r\n", "\n");
                content = content.Replace("\r", "\n");
                string[] list = content.Split(new char[] { '\n' });
                foreach (string str in list)
                {
                    if (contentHtml != "")
                        contentHtml += "<br/>";

                    contentHtml += HttpUtility.HtmlEncode(str);
                }
            }

            // 替换宏
            if (contentHtml.Contains(LibraryManager.M_Lib_PatronCount) == true
                || contentHtml.Contains(LibraryManager.M_Lib_WorkerCount) == true
                || contentHtml.Contains(LibraryManager.M_Lib_BindTotalCount) == true)
            {
                Library library = this.LibManager.GetLibrary(libId);
                contentHtml = contentHtml.Replace(LibraryManager.M_Lib_PatronCount, library.PatronCount.ToString());
                contentHtml = contentHtml.Replace(LibraryManager.M_Lib_WorkerCount, library.WorkerCount.ToString());
                contentHtml = contentHtml.Replace(LibraryManager.M_Lib_BindTotalCount, library.BindTotalCount.ToString());
            }



            return contentHtml;
        }

        public static bool CheckIsBiblioPath(string text)
        {
            bool bPath = false;
            int index = text.LastIndexOf('/');
            if (index > 0) //因为路径中的/不可能是第一个字符，所以没有-1
            {
                string right = text.Substring(index + 1);
                try
                {
                    int no = Convert.ToInt32(right);
                    if (no >= 0)
                        bPath = true;
                }
                catch
                { }
            }

            return bPath;
        }


        public string GetBookHtml(string content, string libId)
        {
            string contentHtml = "";

            content = content.Replace("\r\n", "\n");
            content = content.Replace("\r", "\n");
            string[] list = content.Split(new char[] { '\n' });
            foreach (string str in list)
            {
                // 检查是不是书目路径
                bool bPath = dp2WeiXinService.CheckIsBiblioPath(str);
                if (bPath == false)
                {
                    contentHtml += "<div style='color:gray'>" + HttpUtility.HtmlEncode(str) + "</div>";
                }
                else
                {
                    var word = "@bibliorecpath:" + str;
                    string detalUrl = "/Biblio/Detail?biblioPath=" + HttpUtility.UrlEncode(str);
                    contentHtml += "<div style='padding-top:4px;'><a href='javascript:void(0)' onclick='gotoBiblioDetail(\"" + detalUrl + "\")'>" + HttpUtility.HtmlEncode(str) + "</a></div>";
                    contentHtml += "<div  class='pending' style='padding-bottom:4px'>"
                                           + "<label>bs-" + word + "</label>"
                                           + "<img src='../img/wait2.gif' />"
                                           + "<span>" + libId + "</span>"
                                       + "</div>";
                }

            }

            return contentHtml;
        }

        // 从 dp2mserver 获得消息
        // 每次最多获得 100 条
        private int GetMessageInternal(string action,
            string group,
            string libId,
            string msgId,
            string subjectCondition,
            out List<MessageRecord> records,
            out string strError)
        {
            strError = "";
            records = new List<MessageRecord>();
            //return 0;

            // string connName
            string connName = C_ConnPrefix_Myself + libId;
            if (group == C_Group_dp_home)
                connName = C_ConnPrefix_Myself + C_ConnName_dp;


            // 取出用户名
            LibEntity lib = this.GetLibById(libId);
            string wxUserName = lib.wxUserName;

            if (group == C_Group_dp_home)
                wxUserName = this.GetUserName();

            // 这里要转换一下，接口传进来的是转义后的
            //subjectCondition = HttpUtility.HtmlDecode(subjectCondition);

            string sortCondition = "publishTime|desc";
            if (group == dp2WeiXinService.C_Group_HomePage)
                sortCondition = "publishTime|asc";


            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            GetMessageRequest request = new GetMessageRequest(id,
                action,
                group,
                wxUserName,
                "", // strTimeRange,
                sortCondition,//sortCondition 按发布时间倒序排
                msgId, //IdCondition 
                subjectCondition,
                0,
                100);  // todo 如果超过100条，要做成递归来调
            try
            {
                MessageConnection connection = this.Channels.GetConnectionTaskAsync(this.dp2MServerUrl,
                    connName).Result;
                GetMessageResult result = connection.GetMessage(request,
                    new TimeSpan(0, 0, 10),
                    cancel_token);
                if (result.Value == -1)
                {
                    strError = this.GetFriendlyErrorInfo(result, lib.libName);
                    goto ERROR1;
                }

                records = result.Results;
                return result.Results.Count;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
        ERROR1:
            strError = "访问服务器异常: " + strError;
            return -1;
        }


        public WxMessageResult CoverMessage(string weixinId,
            string group,
            string libId,
            MessageItem item,
            string style,
            string parameters)
        {
            WxMessageResult result = new WxMessageResult();
            string strError = "";
            MessageItem returnItem = null;
            int nRet = this.CoverMessage(weixinId,
                group,
                libId,
                item,
                style,
                parameters,
                out returnItem,
                out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
                return result;
            }

            List<MessageItem> list = new List<MessageItem>();
            list.Add(returnItem);
            result.items = list;
            return result;
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="libUserName"></param>
        /// <param name="style"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public int CoverMessage(string weixinId,
            string group,
            string libId,
            MessageItem item,
            string style,
            string parameters,
            out MessageItem returnItem,
            out string strError)
        {
            strError = "";

            returnItem = null;

            strError = checkGroup(group);
            if (strError != "")
            {
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(item.creator) == true)
            {
                strError = "未传入creator";
                goto ERROR1;
            }

            // 2016-8-24 超级管理员可以编辑任何图书馆介绍与公告,注意这个条件判断是取反
            if (!((group == dp2WeiXinService.C_Group_Bb || group == dp2WeiXinService.C_Group_HomePage || group == dp2WeiXinService.C_Group_dp_home)
                && item.creator == dp2WeiXinService.C_Supervisor))
            {
                // 检索工作人员是否有权限 _wx_setbb
                string needRight = dp2WeiXinService.GetNeedRight(group);

                LibEntity lib = this.GetLibById(libId);
                if (lib == null)
                {
                    strError = "根据id[" + libId + "]未找到对应的图书馆配置";
                    goto ERROR1;
                }
                WxUserItem worker = WxUserDatabase.Current.GetWorker(weixinId, libId);
                if (worker == null)
                {
                    strError = "内部错误：未绑定工作人员账户，不应走到编辑消息这里。";
                    goto ERROR1;

                }

                int nHasRights = this.CheckRights(worker, lib, needRight, out strError);
                if (nHasRights == -1)
                {
                    strError = "用账户名'" + item.creator + "'获取工作人员账户出错：" + strError;
                    goto ERROR1;
                }
                if (nHasRights == 0)
                {
                    strError = "帐户[" + item.creator + "]没有" + needRight + "权限";
                    goto ERROR1;
                }
            }


            string connName = C_ConnPrefix_Myself + libId;
            if (group == dp2WeiXinService.C_Group_dp_home)
                connName = C_ConnPrefix_Myself + C_ConnName_dp;// "_dp_";

            string strText = "";
            if (style != "delete")
            {
                strText = "<body>"
                + "<title>" + HttpUtility.HtmlEncode(item.title) + "</title>"  //前端传过来时，已经转义过了 HttpUtility.HtmlEncode(item.title)
                + "<content format='" + item.contentFormat + "'>" + HttpUtility.HtmlEncode(item.content) + "</content>"
                + "<remark>" + HttpUtility.HtmlEncode(item.remark) + "</remark>"
                + "<creator>" + item.creator + "</creator>"
                + "</body>";
            }



            List<MessageRecord> records = new List<MessageRecord>();
            MessageRecord record = new MessageRecord();
            record.id = item.id;
            record.groups = group.Split(new char[] { ',' });
            record.creator = "";    // 服务器会自己填写
            record.data = strText;
            record.format = "text";
            record.type = "message";
            record.thread = "";
            record.expireTime = new DateTime(0);    // 表示永远不失效
            if (item.subject != null)
                item.subject = item.subject.Trim();// 2016-8-20 jane 对栏目首尾去掉空白
            if (item.subject != null && item.subject != "")
            {
                string tempSubject = StringUtil.EscapeString(item.subject, "[](),|");
                record.subjects = new string[] { tempSubject };//2016-8-20,不管有没有逗号，只当作一条subject处理。item.subject.Split(new char[] { ',' },StringSplitOptions.RemoveEmptyEntries); // 2016-8-20，jane,对首尾去掉空白，与服务器保存一致。
            }
            else
                record.subjects = new string[] { };
            records.Add(record);

            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    connName).Result;
                SetMessageRequest param = new SetMessageRequest(style,
                    "",
                    records);
                CancellationToken cancel_token = new CancellationToken();

                SetMessageResult result = connection.SetMessageTaskAsync(param,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }

                MessageRecord returnRecord = result.Results[0];
                returnRecord.data = strText;
                returnItem = this.ConvertMsgRecord(group, returnRecord, "browse", libId);
                //returnItem

                // 新创建，且check栏目的序号
                if (style == "create" && parameters != null && parameters.Contains("checkSubjectIndex") == true
                    && returnItem.subject != "")
                {
                    List<SubjectItem> list = null;
                    int nRet = this.GetSubject(libId,
                        group,
                        out list,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    int nIndex = 0;
                    foreach (SubjectItem sub in list)
                    {
                        if (sub.count == 0) //因为前端不显示为0的栏目，所以这里要忽略掉
                            continue;

                        if (sub.name == returnItem.subject)
                        {
                            returnItem.subjectIndex = nIndex;
                            break;
                        }
                        nIndex++;
                    }

                    // 得到去掉序号的subject
                    int no = 0;
                    string right = returnItem.subject;
                    this.SplitSubject(returnItem.subject, out no, out right);
                    returnItem.subjectPureName = right;
                }

                return 0;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
        ERROR1:
            return -1;
        }


        /// <returns>
        /// -1：出错
        /// 0   无权限
        /// 1   有权限
        /// </returns>
        public int CheckRights(WxUserItem user,
            LibEntity lib,
            string needRight,
            out string strError)
        {
            strError = "";
            string rights = user.rights;

            if (String.IsNullOrEmpty(rights) == true)
            {
                // 当发现mongodb中没有存权限信息时，从dp2library中查找
                int nRet = this.GetUserRights(lib,
                    user.userName,
                    out rights,
                    out strError);
                if (nRet == -1 || nRet == 0)
                {
                    return -1;
                }
                // 更新到mongodb中
                user.rights = rights;
                WxUserDatabase.Current.Update(user);
            }


            bool bRight = this.CheckContainRights(rights, needRight);
            if (bRight == true)
                return 1;

            return 0;
        }

        /// <summary>
        /// 检查权限字符串中是否包括指定的权限
        /// </summary>
        /// <param name="rights"></param>
        /// <param name="needRight"></param>
        /// <returns></returns>
        public bool CheckContainRights(string rights, string needRight)
        {
            if (string.IsNullOrEmpty(rights) == true || string.IsNullOrEmpty(needRight) == true)
                return false;

            // 先将rights以逗号分成数组
            string[] rightList = rights.Split(',');
            foreach (string right in rightList)
            {
                if (right == needRight)
                    return true;
            }

            return false;
        }


        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <param name="opacUserName"></param>
        /// <param name="strWord"></param>
        /// <param name="right"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int GetUserRights(LibEntity lib,
            string strWord,
            out string rights,
            out string strError)
        {
            strError = "";
            rights = "";

            List<Record> records = null;
            int nRet = this.GetUserInfo1(lib, strWord, out records, out strError);
            if (nRet == -1 || nRet == 0)
                return nRet;

            if (records == null || records.Count == 0)
                return 0;

            string strXml = records[0].Data;
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);
            rights = DomUtil.GetAttr(dom.DocumentElement, "rights");

            return 1;

        }


        public int GetUserInfo1(LibEntity lib, string strWord,
            out List<Record> records,
            out string strError)
        {
            strError = "";
            records = null;

            //long start = 0;
            //long count = 10;
            try
            {
                // 使用代理账号capo 20161024 jane
                LoginInfo loginInfo = new LoginInfo("", false);

                CancellationToken cancel_token = new CancellationToken();
                string id = Guid.NewGuid().ToString();
                SearchRequest request = new SearchRequest(id,
                    loginInfo,
                    "getUserInfo",
                    "",
                    strWord,
                    "",//strFrom,
                    "",//"middle",
                    "",//resultSet,//"weixin",
                    "",//"id,cols",
                    1,//WeiXinConst.C_Search_MaxCount,  //最大数量
                    0,  //每次获取范围
                    -1);

                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;

                SearchResult result = connection.SearchTaskAsync(
                    lib.capoUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                if (result.ResultCount == -1)
                {
                    bool bOffline = false;
                    strError = "GetUserInfo()出错：" + this.GetFriendlyErrorInfo(result, lib.libName, out bOffline);// result.ErrorInfo;// +" \n dp2mserver账户:" + connection.UserName;
                    return -1;
                }
                if (result.ResultCount == 0)
                {
                    strError = "未命中";
                    return 0;
                }

                records = result.Records;


                return (int)result.ResultCount;// records.Count;
            }
            catch (AggregateException ex)
            {
                strError = ex.Message;
                return -1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

        }


        public const string C_Active_EnumSubject = "enumSubject";

        public static string checkGroup(string group)
        {
            if (group != dp2WeiXinService.C_Group_Bb
               && group != dp2WeiXinService.C_Group_Book
               && group != dp2WeiXinService.C_Group_HomePage
                && group != dp2WeiXinService.C_Group_dp_home)
            {
                return "不支持的群" + group;
            }

            return "";
        }

        /// <summary>
        /// 获取栏目
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libId"></param>
        /// <param name="userName">
        /// 返回的当前绑定工作人员账户号，如果为空，不没有编辑权限
        /// </param>
        /// <returns></returns>
        public int GetSubject(string libId,
            string group,
            out List<SubjectItem> list,
            out string strError)
        {
            strError = "";
            list = new List<SubjectItem>();

            strError = checkGroup(group);
            if (strError != "")
            {
                return -1;
            }

            // 获取栏目
            List<MessageRecord> records = null;
            int nRet = this.GetMessageInternal(C_Active_EnumSubject,
                group,
                libId,
                "",
                "",
                out records,
                out strError);
            if (nRet == -1)
                return -1;

            foreach (MessageRecord record in records)
            {
                string[] subjects = record.subjects;
                SubjectItem subItem = new SubjectItem();

                // 发现确实空的情况。
                if (subjects == null || subjects.Length == 0)
                    continue;

                string subject = subjects[0];//2016-8-20 jane 这里的栏目是从服务器上得到了，不用管首尾空白的问题，如果管了反而暴露不出来问题
                subject = StringUtil.UnescapeString(subject);

                int no = 0;
                string right = subject;
                this.SplitSubject(subject, out no, out right);

                subItem.no = no;
                subItem.pureName = right;
                subItem.name = subject;
                subItem.count = 0;
                try
                {
                    subItem.count = Convert.ToInt32(record.data);
                }
                catch (Exception ex)
                {
                    {
                        strError = "将data转变数字出错：" + ex.Message;
                        return -1;
                    }
                }
                list.Add(subItem);
            }

            // 如果是图书馆介绍，需要加一些默认模板
            if (group == dp2WeiXinService.C_Group_HomePage)
            {
                LibEntity lib = this.GetLibById(libId);
                string dir = dp2WeiXinService.Instance.weiXinDataDir + "/lib/" + "template/home";// +lib.capoUserName + "/home";
                if (Directory.Exists(dir) == true)
                {
                    string[] files = Directory.GetFiles(dir, "*.txt");
                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        bool bExist = this.checkContaint(list, fileName);
                        if (bExist == false)
                        {
                            string subject = fileName.Trim();// 2016-8-20 对首尾去空白，因为服务器对subject的空白支持不太好
                            int no = 0;
                            string right = subject;
                            this.SplitSubject(subject, out no, out right);

                            SubjectItem subItem = new SubjectItem();
                            subItem.no = no;
                            subItem.pureName = right;
                            subItem.name = subject;
                            subItem.count = 0;
                            list.Add(subItem);
                        }
                    }
                }
            }


            // 2016-8-19 jane 修改栏目排序算法

            // 先检查列表项中有没有带{}的
            bool hasParenthesis = false;
            foreach (SubjectItem sub in list)
            {
                if (sub.no != -1)
                {
                    hasParenthesis = true;
                    break;
                }
            }

            // 带括号的情况，括号里的值会自动扩充长度
            if (hasParenthesis == true)
            {
                list.Sort((x, y) =>
                {
                    //// 一个有括号，一个没括号，有{}的排前面
                    //if (x.no == -1 && y.no !=-1) // 左没有，右有{}
                    //    return 0;
                    //if (x.no != -1 && y.no == -1) // 左有{}，右没有
                    //    return 1;
                    if ((x.no == -1 && y.no != -1) || (x.no != -1 && y.no == -1))
                    {
                        // 右对齐 2016-8-20 jane 发现上面的算法，当一个有括号一个没括号时，排序出来的结果不出。
                        string tempName1 = x.name;
                        string tempName2 = y.name;
                        int length = tempName1.Length > tempName2.Length ? tempName1.Length : tempName2.Length;
                        if (tempName1.Length < length)
                            tempName1 = tempName1.PadLeft(length, '0');
                        if (tempName2.Length < length)
                            tempName2 = tempName2.PadLeft(length, '0');

                        return tempName1.CompareTo(tempName2); //左对齐排序


                    }


                    // 都没有括号的时候左对齐
                    if (x.no == -1 && x.no == 1)
                        return x.name.CompareTo(y.name);

                    //都有括号的时候，把括号里的内容扩充为等长，前补0
                    string tempNo1 = x.no.ToString();
                    string tempNo2 = y.no.ToString();
                    int maxLength = tempNo1.Length > tempNo2.Length ? tempNo1.Length : tempNo2.Length;
                    if (tempNo1.Length < maxLength)
                        tempNo1 = tempNo1.PadLeft(maxLength, '0');
                    if (tempNo2.Length < maxLength)
                        tempNo2 = tempNo2.PadLeft(maxLength, '0');

                    string name1 = "{" + tempNo1 + "}" + x.pureName;
                    string name2 = "{" + tempNo2 + "}" + y.pureName;

                    return name1.CompareTo(name2); //左对齐排序
                });
            }
            else
            {
                // 不带括号的情况，全部左对齐
                list.Sort((x, y) =>
                {
                    int value = x.name.CompareTo(y.name);
                    return value;
                });
            }


            return records.Count;
        }

        public void SplitSubject(string subject, out int no, out string right)
        {
            no = -1;
            int nIndex = subject.IndexOf('}');
            string left = "";
            right = subject;
            if (nIndex > 0)
            {
                right = subject.Substring(nIndex + 1);
                left = subject.Substring(0, nIndex + 1);//{3}
                left = left.Substring(0, left.Length - 1);//{3
                if (left.Length > 0 && left.Substring(0, 1) == "{")
                    left = left.Substring(1).Trim(); //去掉序号两边的空格
                try
                {
                    no = Convert.ToInt32(left);
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog1("栏目'" + subject + "'的序号格式不合法，无法参于排序。");
                }
            }
            // 2016-8-19 排序只处理{}的情况，注掉下方内容
            //else
            //{
            //    //检查是否有空格，如果空格前面是数字，也参考排序
            //    nIndex = subject.IndexOf(' ');
            //    if (nIndex > 0)
            //    {
            //        left = subject.Substring(0, nIndex);
            //        try
            //        {
            //            no = Convert.ToInt32(left);
            //        }
            //        catch
            //        {
            //            this.WriteErrorLog("栏目'" + subject + "'虽有空格，但空格前是非数字，无法参考排序。");
            //        }
            //    }

            //}
        }

        /// <summary>
        /// 检查一个栏目是否已存在
        /// </summary>
        /// <param name="list"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        private bool checkContaint(List<SubjectItem> list, string subject)
        {
            foreach (SubjectItem item in list)
            {
                if (item.name == subject)
                    return true;
            }
            return false;
        }
        public string GetSubjectHtml(string libId, string group, string selSubject, bool bNew, List<SubjectItem> list)
        {
            int nRet = 0;
            string strError = "";
            if (list == null) //外面可以传进来
            {
                nRet = this.GetSubject(libId, group, out list, out strError);
                if (nRet == -1)
                {
                    return "获取好书推荐的栏目出错";
                }
            }
            //if (String.IsNullOrEmpty(selSubject)== false)
            //{
            //    if (selSubject.Length > 6 && selSubject.Substring(0, 6) == "msgid-")
            //    {
            //        string msgId=selSubject.Substring(6);
            //        List<MessageItem> msgList = null;
            //        nRet = this.GetMessage(group,
            //            libId,
            //            msgId,
            //            "",
            //            "original",
            //            out msgList,
            //            out strError);
            //        if (nRet ==-1)
            //        {
            //            return "获得id为'"+msgId+"'的message出错。";
            //        }

            //        if(msgList != null && msgList.Count>0)
            //        {
            //            selSubject = msgList[0].subject;
            //        }
            //    }
            //}

            var opt = "<option value=''>请选择 栏目</option>";
            for (var i = 0; i < list.Count; i++)
            {
                SubjectItem item = list[i];
                string selectedString = "";
                if (selSubject != "" && selSubject == item.name)
                {
                    selectedString = " selected='selected' ";
                }
                opt += "<option value='" + item.name + "' " + selectedString + ">" + item.name + "</option>";
            }

            string onchange = "";
            if (bNew == true)
            {
                opt += "<option value='new'>自定义栏目</option>";

                onchange = " onchange='subjectChanged(false,this)' ";

                if (group == dp2WeiXinService.C_Group_HomePage)
                    onchange = " onchange='subjectChanged(true,this)' ";
            }

            string subjectHtml = "<select id='selSubject'  " + onchange + " >" + opt + "</select>";


            return subjectHtml;
        }

        #endregion

        #region 恢复绑定账户

        public WxUserResult AddAppId()
        {
            WxUserResult result = new WxUserResult();

            GzhCfg gzh=this.gzhContainer.GetDefault();

            List<UserSettingItem> settingList = UserSettingDb.Current.GetAll();
            foreach (UserSettingItem one in settingList)
            {
                string weixinId = one.weixinId;

                if (weixinId == C_Supervisor)
                    continue;

                if (weixinId.IndexOf("@") == -1)
                    weixinId += "@" + gzh.appId;

                one.weixinId = weixinId;
                UserSettingDb.Current.UpdateById(one);
            }

            List<WxUserItem> userList = WxUserDatabase.Current.GetAll();
            foreach (WxUserItem one in userList)
            {
                string weixinId = one.weixinId;
                if (weixinId == C_Supervisor)
                    continue;

                if (weixinId.IndexOf("@") == -1)
                {
                    if (string.IsNullOrEmpty(one.appId) == false)
                    {
                        weixinId += "@" + one.appId;
                    }
                    else
                    {
                        weixinId += "@" + gzh.appId;
                    }
                }

                one.weixinId = weixinId;
                WxUserDatabase.Current.Update(one);
            }


            return result;
        }

        public WxUserResult RecoverUsers()
        {
            string strError = "";
            WxUserResult result = new WxUserResult();

            // 统一设置一下setting表中当前用户patronRefId，用于恢复过程的最后一更，更新当前活动账户
            List<WxUserItem> activeUserList = WxUserDatabase.Current.GetActivePatrons();
            foreach (WxUserItem activeUser in activeUserList)
            {
                UpdateUserSetting(activeUser.weixinId, activeUser.libId, null, false, activeUser.refID); // todo 这里的libid有没有错
            }


            // 循环处理每个图书馆
            //List<LibEntity> libs = LibDatabase.Current.GetLibs();
            List<Library> libs = this.LibManager.Librarys;
            foreach (Library library in libs)
            {
                LibEntity lib = library.Entity;
                // 从远方图书馆查到绑定了微信的工作人员，以临时状态保存的微信用户库
                int nRet = this.SetWorkersFromLib(lib,
                    out strError);
                if (nRet == -1)
                {
                    //goto ERROR1;
                    this.WriteErrorLog1("恢复用户-获得工作人员出错：" + strError);
                    continue;
                }

                // 从远方图书馆查到绑定了微信的读者，以临时状态保存的微信用户库
                long lRet = this.SetPatronsFromLib(lib,
                    out strError);
                if (lRet == -1)
                {
                    //goto ERROR1;
                    this.WriteErrorLog1("恢复用户-获得读者出错：" + strError);
                    continue;
                }

                // 将原来有效的删除
                WxUserDatabase.Current.Delete(lib.id, WxUserDatabase.C_State_Available);

                // 将临时状态变为有效状态
                WxUserDatabase.Current.SetState(lib.id, WxUserDatabase.C_State_Temp, WxUserDatabase.C_State_Available);


                // 根据用户setting表，重新设上当前活动账户
                List<UserSettingItem> settingList = UserSettingDb.Current.GetByLibId(lib.id);
                foreach (UserSettingItem setting in settingList)
                {
                    if (String.IsNullOrEmpty(setting.patronRefID) == false)
                    {
                        WxUserItem tempUser = WxUserDatabase.Current.GetPatronByPatronRefID(setting.weixinId,
                            setting.libId,
                            setting.patronRefID);

                        if (tempUser != null)
                        {
                            // 把对应的绑定账户激活
                            WxUserDatabase.Current.SetActivePatron(setting.weixinId, tempUser.id);
                        }
                    }
                }
            }


            List<WxUserItem> list = WxUserDatabase.Current.Get(null, null, -1, null, null, false);//.GetUsers();
            result.users = list;


            return result;

        ERROR1:
            result.errorCode = -1;
            result.errorInfo = strError;

            return result;

        }

        /// <summary>
        /// 从图书馆查询到绑定了微信的读者，同时转为mongodb的格式
        /// </summary>
        /// <param name="lib"></param>
        /// <param name="users"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public long SetPatronsFromLib(LibEntity lib,
            out string strError)
        {
            strError = "";

            // 使用代理账号capo 20161024 jane
            LoginInfo loginInfo = new LoginInfo("", false);

            // 从远程dp2library中查
            string strWord = WeiXinConst.C_WeiXinIdPrefix;// +weixinId;
            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
                loginInfo,
                "searchPatron",
                "<全部>",
                strWord,
                "email",
                "left",
                "wx-patron",
                "id,xml",
                1000,
                0,
                WeiXinConst.C_Search_MaxCount);
            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    lib.capoUserName).Result;

                SearchResult result = connection.SearchTaskAsync(
                    lib.capoUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                if (result.ResultCount == -1)
                {
                    bool bOffline = false;
                    strError = this.GetFriendlyErrorInfo(result, lib.libName, out bOffline);// result.ErrorInfo;
                    return -1;
                }
                if (result.ResultCount == 0)
                    return 0;


                foreach (Record record in result.Records)// int i = 0; i < result.ResultCount; i++)
                {
                    string xml = record.Data;

                    List<string> weixinIds = null;
                    WxUserItem patronInfo = this.GetPatronInfoByXml(xml, out weixinIds);
                    foreach (string oneWeixinId in weixinIds)
                    {
                        WxUserItem userItem = this.newPatronUserItem(lib, oneWeixinId, patronInfo);
                        userItem.state = WxUserDatabase.C_State_Temp;

                        WxUserDatabase.Current.Add(userItem);
                    }

                    return 1;
                }

            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
        ERROR1:
            return -1;
        }

        public WxUserItem newPatronUserItem(LibEntity lib, string weixinId, WxUserItem patronInfo)
        {
            WxUserItem userItem = new WxUserItem();
            userItem.weixinId = weixinId;
            userItem.libName = lib.libName;
            userItem.libId = lib.id;

            userItem.readerBarcode = patronInfo.readerBarcode;
            userItem.readerName = patronInfo.readerName;
            userItem.department = patronInfo.department;
            userItem.xml = patronInfo.xml;

            userItem.refID = patronInfo.refID;
            userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
            userItem.updateTime = userItem.createTime;
            userItem.isActive = 0; // isActive只针对读者，后面会激活读者，工作人员时均为0

            userItem.libraryCode = patronInfo.libraryCode;
            userItem.type = WxUserDatabase.C_Type_Patron;
            userItem.userName = "";
            userItem.isActiveWorker = 0;//是否是激活的工作人员账户，读者时均为0
            userItem.tracing = "off";//无意义，设为关闭状态


            userItem.state = WxUserDatabase.C_State_Available;
            userItem.remark = "";
            userItem.rights = patronInfo.rights;
            userItem.appId = patronInfo.appId;

            return userItem;
        }

        public int SetWorkersFromLib(LibEntity lib,
            out string strError)
        {
            strError = "";

            List<Record> records = null;
            int nRet = this.GetUserInfo1(lib, "", out records, out strError);
            if (nRet == -1 || nRet == 0)
            {
                return nRet;
            }

            foreach (Record record in records)// int i = 0; i < result.ResultCount; i++)
            {
                string xml = record.Data;

                List<string> weixinIds = null;
                string name = "";
                string libraryCode = "";
                string rights = "";
                this.GetWorkerInfoByXml(xml, out weixinIds, out name, out libraryCode, out rights);
                foreach (string oneWeixinId in weixinIds)
                {
                    WxUserItem userItem = this.NewWorkerUserItem(lib, oneWeixinId, name, libraryCode, rights);
                    userItem.state = WxUserDatabase.C_State_Temp;// 设为临时状态
                    WxUserDatabase.Current.Add(userItem);
                }

            }




            return 1;
        }

        public WxUserItem NewWorkerUserItem(LibEntity lib,
            string weixinId,
            string name,
            string libraryCode,
            string rights)
        {
            WxUserItem userItem = new WxUserItem();
            userItem.weixinId = weixinId;
            userItem.libName = lib.libName;
            userItem.libId = lib.id;

            userItem.readerBarcode = "";
            userItem.readerName = "";
            userItem.department = "";
            userItem.xml = "";

            userItem.refID = "";
            userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
            userItem.updateTime = userItem.createTime;
            userItem.isActive = 0; // isActive只针对读者，后面会激活读者，工作人员时均为0

            userItem.libraryCode = libraryCode;
            userItem.type = WxUserDatabase.C_Type_Worker;
            userItem.userName = name;
            userItem.isActiveWorker = 0;//是否是激活的工作人员账户，读者时均为0
            userItem.tracing = "off";//默认是关闭状态

            userItem.state = WxUserDatabase.C_State_Available;
            userItem.remark = "";
            userItem.rights = rights;

            return userItem;
        }


        private WxUserItem GetPatronInfoByXml(string xml, out List<string> weixinIds)
        {
            weixinIds = new List<string>();

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            XmlNode root = dom.DocumentElement;
            XmlNode node = null;

            // 证条码号
            string readerBarcode = "";
            node = root.SelectSingleNode("barcode");
            if (node != null)
                readerBarcode = DomUtil.GetNodeText(node);

            // 姓名
            string readerName = "";
            node = root.SelectSingleNode("name");
            if (node != null)
                readerName = DomUtil.GetNodeText(node);


            //参考id
            string refID = "";
            node = root.SelectSingleNode("refID");
            if (node != null)
                refID = DomUtil.GetNodeText(node);

            // 部门
            string department = "";
            node = root.SelectSingleNode("department");
            if (node != null)
                department = DomUtil.GetNodeText(node);

            // 分馆代码
            string libraryCode = "";
            node = root.SelectSingleNode("libraryCode");
            if (node != null)
                libraryCode = DomUtil.GetNodeText(node);

            // 权限
            string rights = "";
            node = root.SelectSingleNode("rights");
            if (node != null)
                rights = DomUtil.GetNodeText(node);


            WxUserItem userItem = new WxUserItem();
            userItem.readerBarcode = readerBarcode;
            userItem.readerName = readerName;
            userItem.department = department;
            userItem.refID = refID;
            userItem.libraryCode = libraryCode;
            userItem.xml = xml;
            userItem.rights = rights;

            // 取email
            string email = "";
            node = root.SelectSingleNode("email");
            if (node != null)
                email = DomUtil.GetNodeText(node);
            weixinIds = this.GetWeixinIds(email);

            return userItem;
        }

        private void GetWorkerInfoByXml(string xml, out List<string> weixinIds,
            out string name,
            out string libraryCode,
            out string rights)
        {
            weixinIds = new List<string>();

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            XmlNode root = dom.DocumentElement;

            name = DomUtil.GetAttr(root, "name");
            libraryCode = DomUtil.GetAttr(root, "libraryCode");
            rights = DomUtil.GetAttr(root, "rights");

            // 取出binding="weixinid:o4xvUviTxj2HbRqbQb9W2nMl4fGg" 
            string binding = DomUtil.GetAttr(root, "binding");
            weixinIds = this.GetWeixinIds(binding);
        }

        private List<string> GetWeixinIds(string text)
        {
            List<string> weixinIds = new List<string>();

            GzhCfg gzh=this.gzhContainer.GetDefault();

            if (String.IsNullOrEmpty(text) == false)
            {
                string[] emailList = text.Split(new char[] { ',' });
                for (int i = 0; i < emailList.Length; i++)
                {
                    string oneEmail = emailList[i].Trim();
                    if (oneEmail.Length > 9 && oneEmail.Substring(0, 9) == WeiXinConst.C_WeiXinIdPrefix)
                    {
                        string weixinId = oneEmail.Substring(9).Trim();
                        if (weixinId != "")
                        {
                            //int nTemp = weixinId.IndexOf("@");
                            //if (nTemp == -1)
                            //    weixinId += "@" + gzh.appId;

                            weixinIds.Add(weixinId);
                        }
                    }
                }
            }
            return weixinIds;

        }

        private List<string> AddAppIdForWeixinId(List<string> weixinIds)
        {
            GzhCfg gzh=dp2WeiXinService.Instance.gzhContainer.GetDefault();

            for (int i = 0; i < weixinIds.Count; i++)
            {
                string weixinId = weixinIds[i];
                if (weixinId.IndexOf('@') == -1)
                {
                    weixinId += "@" + gzh.appId;
                }
                weixinIds[i] = weixinId;
            }
            return weixinIds;
        }

        private int UpdateWorker(LibEntity lib, XmlDocument bodyDom, out string strError)
        {
            strError = "";
            //<root>
            //  <type>工作人员账户变动</type>
            //  <operation>setUser</operation>
            //  <action>change</action>
            //  <operator>jane</operator>
            //  <operTime>Fri, 23 Sep 2016 03:37:05 +0800</operTime>
            //  <oldAccount name="jane" rights="borrow,return,renew,lost,read,reservation,order,setclock,changereaderpassword,verifyreaderpassword,getbibliosummary,searchcharging,searchreader,getreaderinfo,setreaderinfo,movereaderinfo,changereaderstate,changereaderbarcode,listbibliodbfroms,listdbfroms,searchbiblio,getbiblioinfo,searchitem,getiteminfo,setiteminfo,getoperlog,amerce,amercemodifyprice,amercemodifycomment,amerceundo,inventory,inventorydelete,search,getrecord,getcalendar,changecalendar,newcalendar,deletecalendar,batchtask,clearalldbs,devolvereaderinfo,getuser,changeuser,newuser,deleteuser,changeuserpassword,simulatereader,simulateworker,getsystemparameter,setsystemparameter,urgentrecover,repairborrowinfo,passgate,getres,writeres,setbiblioinfo,hire,foregift,returnforegift,settlement,undosettlement,deletesettlement,searchissue,getissueinfo,setissueinfo,searchorder,getorderinfo,setorderinfo,getcommentinfo,setcommentinfo,searchcomment,denychangemypassword,writeobject,writerecord,writetemplate,managedatabase,restore,managecache,managecomment,settailnumber,setutilinfo,getpatrontempid,getchannelinfo,managechannel,viewreport,upload,download,checkclientversion,client_uimodifyorderrecord,client_forceverifydata,client_deletebibliosubrecords,client_simulateborrow,bindpatron,resetpasswordreturnmessage,_wx_setbb,_wx_setbook,_wx_setHomePage,_wx_getWarning" libraryCode="" access="" comment="" type="" binding="weixinid:o4xvUviTxj2HbRqbQb9W2nMl4fGg" />
            //  <account name="jane" rights="borrow,return,renew,lost,read,reservation,order,setclock,changereaderpassword,verifyreaderpassword,getbibliosummary,searchcharging,searchreader,getreaderinfo,setreaderinfo,movereaderinfo,changereaderstate,changereaderbarcode,listbibliodbfroms,listdbfroms,searchbiblio,getbiblioinfo,searchitem,getiteminfo,setiteminfo,getoperlog,amerce,amercemodifyprice,amercemodifycomment,amerceundo,inventory,inventorydelete,search,getrecord,getcalendar,changecalendar,newcalendar,deletecalendar,batchtask,clearalldbs,devolvereaderinfo,getuser,changeuser,newuser,deleteuser,changeuserpassword,simulatereader,simulateworker,getsystemparameter,setsystemparameter,urgentrecover,repairborrowinfo,passgate,getres,writeres,setbiblioinfo,hire,foregift,returnforegift,settlement,undosettlement,deletesettlement,searchissue,getissueinfo,setissueinfo,searchorder,getorderinfo,setorderinfo,getcommentinfo,setcommentinfo,searchcomment,denychangemypassword,writeobject,writerecord,writetemplate,managedatabase,restore,managecache,managecomment,settailnumber,setutilinfo,getpatrontempid,getchannelinfo,managechannel,viewreport,upload,download,checkclientversion,client_uimodifyorderrecord,client_forceverifydata,client_deletebibliosubrecords,client_simulateborrow,bindpatron,resetpasswordreturnmessage,_wx_setbb,_wx_setbook,_wx_setHomePage,_wx_getWarning," libraryCode="" access="" comment="" type="" binding="weixinid:o4xvUviTxj2HbRqbQb9W2nMl4fGg" />
            //  <clientAddress via="net.pipe://localhost/dp2library/XE">::1</clientAddress>
            //  <version>1.02</version>
            //</root>

            // 取出传过来的账户信息
            XmlNode root = bodyDom.DocumentElement;

            XmlNode actionNode = root.SelectSingleNode("action");
            string action = DomUtil.GetNodeText(actionNode);
            if (action != "change")
                return 0;

            XmlNode nodeAccount = root.SelectSingleNode("account");

            List<string> newWeixinIds = null;
            string name = "";
            string libraryCode = "";
            string rights = "";
            this.GetWorkerInfoByXml(nodeAccount.OuterXml,
                out newWeixinIds,
                out name,
                out libraryCode,
                out rights);
            if (name == "")
            {
                this.WriteErrorLog1("服务器传来的 工作人员账户变动 消息的账号名为空");
                return 0;
            }

            //2016-11-16
            newWeixinIds = AddAppIdForWeixinId(newWeixinIds);



            this.WriteLog2("收到通知，更新图书馆 " + lib.libName + " 工作人员 " + name + " 的信息。");

            // 查一下数据库中有没有绑定该账户的微信
            List<WxUserItem> userList = WxUserDatabase.Current.GetWorkers(null, lib.id, name);
            List<string> oldWeixinIds = new List<string>();
            foreach (WxUserItem user in userList)
            {
                string temp = user.weixinId;

                // mongodb中存的weixinId带了@appId 2016-11-16
                //if (String.IsNullOrEmpty(user.appId) == false)
                //    temp += "@" + user.appId;

                oldWeixinIds.Add(temp);
            }

            // 没有绑定的微信用户
            if (newWeixinIds.Count == 0 && oldWeixinIds.Count == 0)
                return 0;

            List<string> addIds = null;
            List<string> modifyIds = null;
            List<string> deleteIds = null;
            this.CompareOldNew(oldWeixinIds, newWeixinIds, out addIds, out modifyIds, out deleteIds);

            // 新增的绑定用户
            if (addIds.Count > 0)
            {
                foreach (string weixinId in addIds)
                {
                    WxUserItem userItem = this.NewWorkerUserItem(lib, weixinId, name, libraryCode, rights);
                    WxUserDatabase.Current.Add(userItem);
                }
            }

            // 修改的用户
            if (modifyIds.Count > 0)
            {
                foreach (string weixinId in modifyIds)
                {
                    WxUserItem userItem = this.getUser(userList, weixinId);
                    userItem.libraryCode = libraryCode;
                    userItem.rights = rights;
                    userItem.updateTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                    WxUserDatabase.Current.Update(userItem);
                }
            }

            // 删除的用户
            if (deleteIds.Count > 0)
            {
                foreach (string weixinId in deleteIds)
                {
                    WxUserItem userItem = this.getUser(userList, weixinId);
                    WxUserItem newActivePatron = null;
                    WxUserDatabase.Current.Delete(userItem.id, out newActivePatron);
                }
            }

            return 1;
        }

        private WxUserItem getUser(List<WxUserItem> users, string weixinId)
        {
            foreach (WxUserItem user in users)
            {
                if (user.weixinId == weixinId)
                    return user;
            }
            return null;
        }

        public void CompareOldNew(List<string> oldWeixinIds,
            List<string> newWeixinIds,
            out List<string> addIds,
            out List<string> modifyIds,
            out List<string> deleteIds)
        {
            addIds = new List<string>();
            modifyIds = new List<string>();
            deleteIds = new List<string>();

            // 检查出删除和修改的
            foreach (string id in oldWeixinIds)
            {
                if (newWeixinIds.Contains(id) == false)
                {
                    deleteIds.Add(id);
                }
                else
                {
                    modifyIds.Add(id);
                }
            }

            // 检查出新增的
            foreach (string id in newWeixinIds)
            {
                if (oldWeixinIds.Contains(id) == false)
                    addIds.Add(id);
            }
        }

        private int UpdatePatron(LibEntity lib, XmlDocument bodyDom, out string strError)
        {
            strError = "";
            //<root>
            //    <type>读者记录变动</type>
            //    <operation>setReaderInfo</operation>
            //    <libraryCode></libraryCode>
            //    <action>change</action>
            //    <operator>jane</operator>
            //    <operTime>Fri, 23 Sep 2016 05:20:28 +0800</operTime>
            //    <clientAddress>::1</clientAddress>
            //    <version>1.02</version>
            //    <patronRecord>
            //      <barcode>R00003</barcode>
            //      <readerType>教职工</readerType>
            //      <name>任3-1</name>
            //      <refID>63aeb890-8936-4471-bfc5-8e72d5c7fe94</refID>
            //      <tel>13862157150</tel>
            //      <hire expireDate="" period=""></hire>
            //      <email>renyh@163.com,weixinid:o4xvUviTxj2HbRqbQb9W2nMl4fGg</email>
            //      <reservations>
            //        <request items="C102" requestDate="Wed, 07 Sep 2016 16:41:57 +0800" operator="supervisor" />
            //      </reservations>
            //      <outofReservations count="4">
            //        <request itemBarcode="C104" notifyDate="Tue, 05 Jul 2016 14:29:52 +0800"></request>
            //        <request itemBarcode="C103" notifyDate="Tue, 05 Jul 2016 14:40:50 +0800"></request>
            //        <request itemBarcode="C102" notifyDate="Tue, 05 Jul 2016 16:27:28 +0800"></request>
            //        <request itemBarcode="C101" notifyDate="Sat, 17 Sep 2016 22:02:13 +0800" />
            //      </outofReservations>
            //      <borrows>
            //        <borrow barcode="C100" recPath="中文图书实体/4" biblioRecPath="中文图书/3" borrowDate="Sun, 04 Sep 2016 09:31:23 +0800" borrowPeriod="31day" returningDate="Wed, 05 Oct 2016 12:00:00 +0800" operator="jane" type="普通" price="CNY27.80" notifyHistory="nnyn" />
            //      </borrows>
            //      <overdues></overdues>
            //      <libraryCode></libraryCode>
            //      <dprms:file id="0" usage="cardphoto" xmlns:dprms="http://dp2003.com/dprms" />
            //    </patronRecord>
            //  </root>



            // 取出传过来的账户信息
            XmlNode root = bodyDom.DocumentElement;
            XmlNode actionNode = root.SelectSingleNode("action");
            string action = DomUtil.GetNodeText(actionNode);
            if (action != "change")
                return 0;

            XmlNode patronRecord = root.SelectSingleNode("patronRecord");

            List<string> newWeixinIds = null;
            WxUserItem patronInfo = this.GetPatronInfoByXml(patronRecord.OuterXml, out newWeixinIds);

            // 2016-11-16
            newWeixinIds = this.AddAppIdForWeixinId(newWeixinIds);

            // 查一下数据库中有没有绑定该账户的微信
            List<WxUserItem> userList = WxUserDatabase.Current.GetPatron(null, lib.id, patronInfo.readerBarcode);
            List<string> oldWeixinIds = new List<string>();
            foreach (WxUserItem user in userList)
            {
                string temp = user.weixinId;

                // mongodb中存的weixinId带了@appId 2016-11-16
                //if (String.IsNullOrEmpty(user.appId) == false)
                //    temp += "@" + user.appId;

                oldWeixinIds.Add(temp);
            }

            // 没有绑定的微信用户
            if (newWeixinIds.Count == 0 && oldWeixinIds.Count == 0)
                return 0;

            List<string> addIds = null;
            List<string> modifyIds = null;
            List<string> deleteIds = null;
            this.CompareOldNew(oldWeixinIds, newWeixinIds, out addIds, out modifyIds, out deleteIds);

            // 新增的绑定用户
            if (addIds.Count > 0)
            {
                foreach (string weixinId in addIds)
                {
                    WxUserItem userItem = this.newPatronUserItem(lib, weixinId, patronInfo);
                    WxUserDatabase.Current.Add(userItem);
                }
            }

            // 修改的用户
            if (modifyIds.Count > 0)
            {
                foreach (string weixinId in modifyIds)
                {
                    WxUserItem userItem = this.getUser(userList, weixinId);
                    userItem.readerBarcode = patronInfo.readerBarcode;
                    userItem.readerName = patronInfo.readerName;
                    userItem.department = patronInfo.department;
                    userItem.xml = patronInfo.xml;
                    userItem.refID = patronInfo.refID;
                    userItem.libraryCode = patronInfo.libraryCode;
                    userItem.updateTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                    WxUserDatabase.Current.Update(userItem);
                }
            }

            // 删除的用户
            if (deleteIds.Count > 0)
            {
                foreach (string weixinId in deleteIds)
                {
                    WxUserItem userItem = this.getUser(userList, weixinId);
                    WxUserItem newActivePatron = null;
                    WxUserDatabase.Current.Delete(userItem.id, out newActivePatron);
                }
            }

            return 1;
        }


        #endregion

        #region 图书馆管理

        public int AddLib(LibEntity item, out LibEntity outputItem, out string strError)
        {
            strError = "";
            outputItem = null;
            try
            {
                outputItem = LibDatabase.Current.Add(item);

                ////创建对应的图书馆介绍配置目录
                //string libDir = dp2WeiXinService.Instance.weiXinDataDir + "/lib/" + item.capoUserName + "/home";
                //if (Directory.Exists(libDir) == false)
                //    Directory.CreateDirectory(libDir);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            return 0;

        }


        public ApiResult deleteLib(string id)
        {
            string strError = "";

            ApiResult result = new ApiResult();
            // 先检查一下，是否有微信用户绑定了该图书馆
            List<WxUserItem> list = WxUserDatabase.Current.Get(null, id, -1);
            if (list != null && list.Count > 0)
            {
                strError = "目前存在微信用户绑定了该图书馆的账户，不能删除图书馆。";
                goto ERROR1;
            }

            // 检查是否有微信用户设置了该图书馆
            List<UserSettingItem> settingList = UserSettingDb.Current.GetByLibId(id);
            if (settingList != null && settingList.Count > 0)
            {
                strError = "目前已经存在微信用户设置了该图书馆，不能删除图书馆。";
                goto ERROR1;
            }

            // 删除配置目录
            LibEntity lib = this.GetLibById(id);
            if (lib != null)
            {
                try
                {
                    //创建对应的图书馆介绍配置目录
                    string libDir = dp2WeiXinService.Instance.weiXinDataDir + "/lib/" + lib.capoUserName;// +"/home";
                    if (Directory.Exists(libDir) == true)
                        Directory.Delete(libDir, true);
                }
                catch (Exception ex)
                {
                    strError = ex.Message;

                    // 2016-8-19权限不够，删除不了目录的话，改为记到日志里。
                    this.WriteErrorLog1(strError);
                    //goto ERROR1;
                }
            }

            // 从mongodb中删除
            LibDatabase.Current.Delete(id);
            this.LibManager.DeleteLib(id);

            return result;


        ERROR1:
            result.errorCode = -1;
            result.errorInfo = strError;
            return result;
        }

        #endregion


        #region 资源下载

        public static Uri GetUri(string strURI)
        {
            try
            {
                return new Uri(strURI);
            }
            catch
            {
                return null;
            }
        }

        public static bool MatchMIME(string strMime, string strLeftParam)
        {
            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(strMime, "/", out strLeft, out strRight);
            if (string.Compare(strLeft, strLeftParam, true) == 0)
                return true;
            return false;
        }

        public static string PureName(string strPath)
        {
            // 2012/11/30
            if (string.IsNullOrEmpty(strPath) == true)
                return strPath;

            string sResult = "";
            sResult = strPath;
            sResult = sResult.Replace("/", "\\");
            if (sResult.Length > 0)
            {
                if (sResult[sResult.Length - 1] == '\\')
                    sResult = sResult.Substring(0, sResult.Length - 1);
            }
            int nRet = sResult.LastIndexOf("\\");
            if (nRet != -1)
                sResult = sResult.Substring(nRet + 1);

            return sResult;
        }

        public static int GetObject0(Controller mvcControl, string libId, string uri, out string strError)
        {
            strError = "";
            int nRet = 0;

            // 先取出metadata
            string metadata = "";
            string timestamp = "";
            string outputpath = "";
            nRet = dp2WeiXinService.Instance.GetObjectMetadata(libId,
                uri,
                "metadata",
                null,
                out metadata,
                out timestamp,
                out outputpath,
                out strError);
            if (nRet == -1)
                return -1;

            // <file mimetype="application/pdf" localpath="D:\工作清单.pdf" size="188437"
            // lastmodified="2016/9/6 12:45:14" readCount="17" />
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(metadata);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            // 检查是否有更新，没更新直接用浏览器缓存数据
            string strLastModifyTime = DomUtil.GetAttr(dom.DocumentElement, "lastmodified");//lastmodifytime");
            if (String.IsNullOrEmpty(strLastModifyTime) == false)
            {
                DateTime lastmodified = DateTime.Parse(strLastModifyTime).ToUniversalTime();

                string strIfHeader = mvcControl.Request.Headers["If-Modified-Since"];
                if (String.IsNullOrEmpty(strIfHeader) == false)
                {
                    DateTime isModifiedSince = DateTimeUtil.FromRfc1123DateTimeString(strIfHeader); // .ToLocalTime();

                    if (DateTimeUtil.CompareHeaderTime(isModifiedSince, lastmodified) != 0)
                    {
                        // 修改过
                    }
                    else
                    {
                        // 没有修改过
                        mvcControl.Response.StatusCode = 304;
                        mvcControl.Response.SuppressContent = true;
                        return 0;
                    }
                }

                mvcControl.Response.AddHeader("Last-Modified", DateTimeUtil.Rfc1123DateTimeString(lastmodified)); // .ToUniversalTime()
            }

            // 设置媒体类型
            string strMime = DomUtil.GetAttr(dom.DocumentElement, "mimetype");
            bool bSaveAs = false;
            if (string.IsNullOrEmpty(strMime) == true
                || MatchMIME(strMime, "text") == true
                || MatchMIME(strMime, "image") == true)
            {

            }
            else
            {
                bSaveAs = true;
            }
            if (strMime == "text/plain")
                strMime = "text";
            mvcControl.Response.ContentType = strMime;

            // 是否出现另存为对话框
            if (bSaveAs == true)
            {
                string strClientPath = DomUtil.GetAttr(dom.DocumentElement, "localpath");
                if (strClientPath != "")
                    strClientPath = PureName(strClientPath);

                string strEncodedFileName = HttpUtility.UrlEncode(strClientPath, Encoding.UTF8);
                mvcControl.Response.AddHeader("content-disposition", "attachment; filename=" + strEncodedFileName);
            }

            //设置尺寸
            string strSize = DomUtil.GetAttr(dom.DocumentElement, "size");
            if (String.IsNullOrEmpty(strSize) == false)
            {
                mvcControl.Response.AddHeader("Content-Length", strSize);
            }

            // 关闭buffer
            mvcControl.Response.BufferOutput = false; // 2016/8/31


            // 输出数据流
            nRet = dp2WeiXinService.Instance.GetObjectMetadata(libId,
                uri,
                "metadata,timestamp,data,outputpath",
                mvcControl.Response.OutputStream, //ms,//
                out metadata,
                out timestamp,
                out outputpath,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }



        #endregion


        #region 创建weixin_xxx账户

        private string ManagerUserName = "";
        private string ManagerPassword = "";


        // 用 supervisor 用户名进行登录
        void channels_LoginSupervisor(object sender, LoginEventArgs e)
        {
            MessageConnection connection = sender as MessageConnection;

            if (string.IsNullOrEmpty(this.ManagerUserName) == true)
                throw new Exception("尚未指定超级用户名，无法进行登录");

            e.UserName = this.ManagerUserName;
            e.Password = this.ManagerPassword;
            e.Parameters = "propertyList=biblio_search,libraryUID=weixin" + ",notes=" + HttpUtility.UrlEncode(connection.Name); ;
            return;
        }

        // 创建mserver账号
        public bool CreateMserverUser(string username,
            string password,
            string strDepartment,
            string mUsername,
            string mPassword,
            out string strError)
        {
            strError = "";

            this.ManagerUserName = mUsername;
            this.ManagerPassword = mPassword;

            MessageConnectionCollection channels = null;
            try
            {
                channels = new MessageConnectionCollection();
                channels.Login += channels_LoginSupervisor;

                MessageConnection connection = channels.GetConnectionTaskAsync(
                    dp2MServerUrl,
                    "supervisor-weixin").Result;

                CancellationToken cancel_token = new CancellationToken();
                string id = Guid.NewGuid().ToString();

                List<User> users = new List<User>();

                User user = new User();
                user.userName = username;  // 这个名称必须是weixin_***
                user.password = password;
                user.rights = "getPatronInfo,searchBiblio,searchPatron,bindPatron,getBiblioInfo,getBiblioSummary,getItemInfo,circulation,getUserInfo,getRes,getSystemParameter";
                user.duty = "";
                user.groups = new string[] { "gn:_lib_bb", "gn:_lib_book", "gn:_lib_homePage" };
                user.department = strDepartment;
                user.binding = "ip:[current]";   //20161024 jane 绑定本机ip
                users.Add(user);

                // todo
                MessageResult result = connection.SetUsersTaskAsync("create",
                    users,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;

                if (result.Value == -1)
                {
                    strError = "创建用户 '" + username + "' 时出错: " + result.ErrorInfo;
                    goto ERROR1;
                }

                return true;
            }
            catch (MessageException ex)
            {
                strError = this.GetFriendlyException(ex);
                goto ERROR1;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count > 0 && ex.InnerExceptions[0] is MessageException)
                {
                    MessageException ex1 = (MessageException)ex.InnerExceptions[0];
                    if (ex1.ErrorCode == "Unauthorized")
                    {
                        strError = "dp2mserver超级管理员账户或密码不正确。";
                        goto ERROR1;
                    }
                }

                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                if (channels != null)
                    channels.Login -= channels_LoginSupervisor;

                this.ManagerUserName = "";
                this.ManagerPassword = "";
            }
        ERROR1:

            return false;
        }

        // 测试账号与密码
        private string DetectUserName = "";
        private string DetectPassword = "";

        void channels_LoginUser(object sender, LoginEventArgs e)
        {
            MessageConnection connection = sender as MessageConnection;

            if (string.IsNullOrEmpty(this.DetectUserName) == true)
                throw new Exception("尚未指定用户名，无法进行登录"); ;

            e.UserName = this.DetectUserName;
            e.Password = this.DetectPassword;
            e.Parameters = "propertyList=biblio_search,libraryUID=weixin" + ",notes=" + HttpUtility.UrlEncode(connection.Name);
            return;
        }

        public bool DetectMserverUser(string username, string password, out string strError)
        {
            strError = "";

            this.DetectUserName = username;
            this.DetectPassword = password;

            MessageConnectionCollection channels = null;
            try
            {
                channels = new MessageConnectionCollection();
                channels.Login += channels_LoginUser;

                MessageConnection connection = channels.GetConnectionTaskAsync(
        this.dp2MServerUrl,
        "").Result;
                CancellationToken cancel_token = new CancellationToken();

                string id = Guid.NewGuid().ToString();
                GetMessageRequest request = new GetMessageRequest(id,
                    "",
                    "gn:<default>", // "" 表示默认群组
                    "",
                    "",
                    "",
                    "",
                    "",
                    0,
                    1);

                GetMessageResult result = connection.GetMessage(request,
                   new TimeSpan(0, 1, 0),
                   cancel_token);
                if (result.Value == -1)
                {
                    strError = "检测用户时出错: " + result.ErrorInfo;
                    goto ERROR1;
                }

                return true;
            }
            catch (MessageException ex)
            {
                strError = this.GetFriendlyException(ex);
                goto ERROR1;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count > 0 && ex.InnerExceptions[0] is MessageException)
                {
                    MessageException ex1 = (MessageException)ex.InnerExceptions[0];
                    if (ex1.ErrorCode == "Unauthorized")
                    {
                        strError = "用户名或密码不存在";
                        goto ERROR1;
                    }
                }

                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                if (channels != null)
                    channels.Login -= channels_LoginUser;

                // 把测试账户置空
                this.DetectUserName = "";
                this.DetectPassword = "";
            }

        ERROR1:
            return false;
        }

        private string GetFriendlyException(MessageException ex)
        {
            string strError = "";
            if (ex.ErrorCode == "Unauthorized")
            {
                strError = "以用户名 '" + ex.UserName + "' 登录时, 用户名或密码不正确";
            }
            else if (ex.ErrorCode == "HttpRequestException")
            {
                strError = "dp2MServer URL 不正确，或 dp2MServer 尚未启动";
            }
            else
            {
                strError = ex.Message;
            }
            return strError;
        }

        #endregion

        public static string GetNeedRight(string group)
        {
            string needRight = "";
            if (group == dp2WeiXinService.C_Group_Bb)
                needRight = dp2WeiXinService.C_Right_SetBb;
            else if (group == dp2WeiXinService.C_Group_Book)
                needRight = dp2WeiXinService.C_Right_SetBook;
            else if (group == dp2WeiXinService.C_Group_HomePage)
                needRight = dp2WeiXinService.C_Right_SetHomePage;

            return needRight;
        }

        public LibEntity GetLibById(string libId)
        {
            Library lib = this.LibManager.GetLibrary(libId);
            if (lib != null)
                return lib.Entity;

            return null;
        }

    }
}
