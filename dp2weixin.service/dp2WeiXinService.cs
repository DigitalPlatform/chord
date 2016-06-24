using com.google.zxing;
using com.google.zxing.common;
using DigitalPlatform.Interfaces;
using DigitalPlatform.IO;
using DigitalPlatform.Message;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Senparc.Weixin.MP.CommonAPIs;
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
using System.Xml;

namespace dp2weixin.service
{
    public class dp2WeiXinService
    {
        // 消息类型
        public const string C_MsgType_Bb = "bb";
        public const string C_MsgType_Book = "book";

        // 通道常量
        public const string C_ConnName_TraceMessage = "_traceMessage";
        public const string C_ConnPrefix_Myself = "<myself>:";

        // 群组常量
        public const string C_GroupName_Bb = "gn:_lib_bb";
        public const string C_GroupName_PatronNotity = "gn:_patronNotify";
        public const string C_GroupName_Book = "gn:_lib_book";

        // 消息权限
        public const string C_Right_SetBb = "_wx_setbb";
        public const string C_Right_SetBook = "_wx_setbook";

        #region 成员变量

        // 微信数据目录
        public string weiXinDataDir = "";
        public string weiXinLogDir = "";
        public string _cfgFile = "";      // 配置文件

        // dp2服务器地址与代理账号
        public string dp2MServerUrl = "";
        public string userName = "";
        public string password = "";

        // 微信信息
        public string weiXinAppId { get; set; }
        public string weiXinSecret { get; set; }
        public bool bTrace = false;

        // 微信web程序url
        //public string opacUrl = "";

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
            this.userName = DomUtil.GetAttr(nodeDp2mserver, "username");//WebConfigurationManager.AppSettings["userName"];
            this.password = DomUtil.GetAttr(nodeDp2mserver, "password");//WebConfigurationManager.AppSettings["password"];
            if (string.IsNullOrEmpty(this.password) == false)// 解密
                this.password = Cryptography.Decrypt(this.password, WeiXinConst.EncryptKey);

            // 取出微信配置信息
            XmlNode nodeDp2weixin = root.SelectSingleNode("dp2weixin");
            //this.opacUrl = DomUtil.GetAttr(nodeDp2weixin, "opacUrl"); //WebConfigurationManager.AppSettings["weiXinUrl"];
            this.weiXinAppId = DomUtil.GetAttr(nodeDp2weixin, "AppId"); //WebConfigurationManager.AppSettings["weiXinAppId"];
            this.weiXinSecret = DomUtil.GetAttr(nodeDp2weixin, "Secret"); //WebConfigurationManager.AppSettings["weiXinSecret"];
            string trace = DomUtil.GetAttr(nodeDp2weixin, "trace");
            if (trace.ToLower() == "true")
                this.bTrace = true;


            // mongo配置
            XmlNode nodeMongoDB = root.SelectSingleNode("mongoDB");
            string connectionString = DomUtil.GetAttr(nodeMongoDB, "connectionString");
            if (String.IsNullOrEmpty(connectionString) == true)
            {
                throw new Exception("尚未配置mongoDB节点的connectionString属性");
            }
            string instancePrefix = DomUtil.GetAttr(nodeMongoDB, "instancePrefix");
            // 打开图书馆账号库与用户库
            WxUserDatabase.Current.Open(connectionString, instancePrefix);
            LibDatabase.Current.Open(connectionString, instancePrefix);

            // 初始化接口类
            string strError = "";
            int nRet = this.InitialExternalMessageInterfaces(dom, out strError);
            if (nRet == -1)
                throw new Exception("初始化接口配置信息出错：" + strError);


            //全局只需注册一次
            AccessTokenContainer.Register(this.weiXinAppId, this.weiXinSecret);
           
            
            _channels.Login -= _channels_Login;
            _channels.Login += _channels_Login;

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
                C_GroupName_PatronNotity);

        }

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

            this.WriteLog("走到close()");
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
                LibItem lib = LibDatabase.Current.GetLibById(libId);
                if (lib == null)
                {
                    throw new Exception("异常的情况:根据id[" + libId + "]未找到图书馆对象。");
                }

                e.UserName = lib.wxUserName;
                if (string.IsNullOrEmpty(e.UserName) == true)
                    throw new Exception("尚未指定微信本方用户名，无法进行登录");
                e.Password = lib.wxPassword;

                //e.UserName = GetUserName();
                //if (string.IsNullOrEmpty(e.UserName) == true)
                //    throw new Exception("尚未指定用户名，无法进行登录");

                //e.Password = GetPassword();
            }
            else
            {
                e.UserName = GetUserName();
                if (string.IsNullOrEmpty(e.UserName) == true)
                    throw new Exception("尚未指定用户名，无法进行登录");

                e.Password = GetPassword();
                //e.Parameters = "propertyList=biblio_search,libraryUID=xxx";

            }
        }


        string GetUserName()
        {
            return this.userName;
        }
        string GetPassword()
        {
            return this.password;
        }


        //Hashtable _wxUserTable = new Hashtable();

        public void SetActivePatron(WxUserItem userItem)
        {
            // 更新数据库
            WxUserDatabase.Current.SetActivePatron(userItem.weixinId, userItem.id);

            //更新内存
            //this._wxUserTable[userItem.weixinId] = userItem;
            //this.CurWxUser = userItem;

            return;
        }

        public void DeletePatron(string userId)
        {
            // 删除mongodb库的记录
            WxUserItem newActivePatron = null;
            WxUserDatabase.Current.Delete(userId, out newActivePatron);

            //if (newActivePatron != null)
            //{
            //    //更新内存
            //    //this.CurWxUser = newActivePatron;
            //    //this._wxUserTable[newActivePatron.weixinId] = newActivePatron;
            //}

            return;
        }


        // 当然微信用户 与当前图书馆
        //private WxUserItem _curWxUser = null;
        //private LibItem _curLib = null;
        //public WxUserItem CurWxUser
        //{
        //    get
        //    {
        //        return this._curWxUser;
        //    }
        //    set
        //    {
        //        this._curWxUser = value;

        //        string libId = this._curWxUser.libId;
        //        if (String.IsNullOrEmpty(libId))
        //        {
        //            throw new Exception("异常的情况:微信用户的libId参数为空。");
        //        }

        //        this. _curLib = LibDatabase.Current.GetLibById(libId);
        //        if (this._curLib == null)
        //        {
        //            throw new Exception("异常的情况:根据id["+libId+"]未找到图书馆对象。");
        //        }
        //    }
        //}

        #endregion

        #region 设置dp2mserver信息

        public void SetDp2mserverInfo(string dp2mserverUrl,
            string userName,
            string password)
        {
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
            this.userName = userName;
            this.password = password;
        }

        public void GetDp2mserverInfo(out string dp2mserverUrl,
            out string userName,
            out string password)
        {
            dp2mserverUrl = "";
            userName = "";
            password = "";

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
        }

        #endregion

        #region 消息处理

        // 处理收到的消息
        void _msgRouter_SendMessageEvent(object sender, SendMessageEventArgs e)
        {
            MessageRecord record = e.Message;
            if (record == null)
            {
                this.WriteErrorLog("传过来的e.Message为null");
                return;
            }

            //this.WriteErrorLog("走进_msgRouter_SendMessageEvent");

            try
            {
                string strError = "";
                /// <returns>
                /// -1 不符合条件，不处理
                /// 0 未绑定微信id，未处理
                /// 1 成功
                /// </returns>
                int nRet = this.InternalDoMessage(record, out strError);
                if (nRet == -1)
                {
                    this.WriteErrorLog("[" + record.id + "]未发送成功:" + strError);
                }
                else if (nRet == 0)
                {
                    this.WriteErrorLog("[" + record.id + "]未发送成功：未绑定微信id。");
                }
                else
                {
                    this.WriteErrorLog("[" + record.id + "]发送成功。");
                }
            }
            catch (Exception ex)
            {
                this.WriteErrorLog("[" + record.id + "]异常："+ex.Message);

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
        public int InternalDoMessage(MessageRecord record, out string strError)
        {
            strError = "";

            string id = record.id;
            string data = record.data;
            string[] group = record.groups;
            string create = record.creator;


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
                strError = "加载消息返回的data到xml出错:" + ex.Message;
                return -1;
            }

            XmlNode nodeType = dataDom.DocumentElement.SelectSingleNode("type");
            if (nodeType == null)
            {
                strError = "尚未定义<type>节点";
                return -1;
            }
            string type = DomUtil.GetNodeText(nodeType);
            if (type != "patronNotify") //只处理通知消息
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

            /*
            body元素里面是预约到书通知记录(注意这是一个字符串，需要另行装入一个XmlDocument解析)，其格式如下：
            <?xml version="1.0" encoding="utf-8"?>
            <root>
                <type>预约到书通知</type>
                <itemBarcode>0000001</itemBarcode>
                <refID> </refID>
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
            </root
            */
            XmlDocument bodyDom = new XmlDocument();
            try
            {
                bodyDom.LoadXml(nodeBody.InnerText);//.InnerXml); 
            }
            catch (Exception ex)
            {
                strError = "加载消息data中的body到xml出错:" + ex.Message;
                return -1;
            }
            XmlNode root = bodyDom.DocumentElement;
            XmlNode typeNode = root.SelectSingleNode("type");
            if (typeNode == null)
            {
                strError = "消息data的body中未定义type节点。";
                return -1;
            }
            string strType = DomUtil.GetNodeText(typeNode);
            int nRet = 0;
            // 根据类型发送不同的模板消息
            if (strType == "预约到书通知")
            {
                nRet = this.SendArrived(bodyDom, out strError);
            }
            else if (strType == "超期通知")
            {
                nRet = this.SendCaoQi(bodyDom, out strError);
            }
            else if (strType == "借书成功")
            {
                nRet = this.SendBorrowMsg(bodyDom, out strError);
            }
            else if (strType == "还书成功")
            {
                nRet = this.SendReturnMsg(bodyDom, out strError);
            }
            else if (strType == "交费")
            {
                nRet = this.SendPayMsg(bodyDom, out strError);
            }
            else if (strType == "撤销交费")
            {
                nRet = this.SendReturnPayMsg(bodyDom, out strError);
            }
            else if (strType == "以停代金到期")
            {
                nRet = this.SendMessageMsg(bodyDom, out strError);
            }
            else
            {
                strError = "不支持的消息类型["+strType+"]";
                return -1;            
            }


            return nRet;
        }

        private string _msgFirstLeft = "尊敬的读者：您好，";
        private string _msgRemark = "\n如有疑问，请联系系统管理员。";

        /// <returns>
        /// -1 出错，格式出错或者发送模板消息出错
        /// 0 未绑定微信id
        /// 1 成功
        /// </returns>
        private int SendMessageMsg(XmlDocument bodyDom, out string strError)
        {
            strError = "";
            /*
<root>
    <type>以停代金到期</type>
…

根元素下的items元素下，有一个或者多个overdue元素，记载了刚到期的以停代金事项信息。
在patronRecord的下级元素overdues下，可以看到若干overdue元素，这是当前还未到期或者交费的事项。只要还有这样的事项，读者就不被允许借书，只能还书。所以通知消息文字组织的时候，可以考虑提醒尚余多少违约事项，这样可以避免读者空高兴一场以为马上可以借书了
     
           */

            string patronName = "";
            List<string> weiXinIdList = this.GetWeiXinIds(bodyDom, out patronName);
            if (weiXinIdList.Count == 0)
            {
                strError = "未绑定微信id";
                return 0;
            }

            XmlNode root = bodyDom.DocumentElement;
            XmlNode nodeOperTime = root.SelectSingleNode("operTime");
            if (nodeOperTime == null)
            {
                strError = "尚未定义<borrowDate>节点";
                return -1;
            }
            string operTime = DomUtil.GetNodeText(nodeOperTime);
            operTime = DateTimeUtil.ToLocalTime(operTime, "yyyy/MM/dd");


            XmlNodeList listOverdue = root.SelectNodes("items/overdue");
            string barcodes = "";
            double totalPrice = 0;
            foreach (XmlNode node in listOverdue)
            {
                string oneBarcode = DomUtil.GetAttr(node, "barcode");
                if (barcodes != "")
                    barcodes += ",";
                barcodes += oneBarcode;

                string price = DomUtil.GetAttr(node, "price");
                if (String.IsNullOrEmpty(price) == false && price.Length > 3)
                {
                    double dPrice = Convert.ToDouble(price.Substring(3));
                    totalPrice += dPrice;
                }
            }

            string strText = "您有["+barcodes + "]项违约以停代金到期了，";
            XmlNodeList listOverdue1 = root.SelectNodes("patronRecord/overdues/overdue");
            if (listOverdue1.Count > 0)
            {
                strText += "您还有" + listOverdue1.Count.ToString() + "项违约未到期，还不能借书。";
            }
            else
            {
                strText += "您可以继续借书了。";
            }


            foreach (string weiXinId in weiXinIdList)
            {
                try
                {
                    var accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);

                    //{{first.DATA}}
                    //标题：{{keyword1.DATA}}
                    //时间：{{keyword2.DATA}}
                    //内容：{{keyword3.DATA}}
                    //{{remark.DATA}}
                    var msgData = new BorrowTemplateData()
                    {
                        first = new TemplateDataItem("〓〓〓〓〓〓〓〓〓〓〓〓〓〓〓", "#9400D3"),// 	dark violet //this._msgFirstLeft + "您的停借期限到期了。" //$$$$$$$$$$$$$$$$
                        keyword1 = new TemplateDataItem("以停代金到期", "#000000"),//text.ToString()),// "请让我慢慢长大"),
                        keyword2 = new TemplateDataItem(operTime, "#000000"),
                        keyword3 = new TemplateDataItem(strText, "#000000"),
                        remark = new TemplateDataItem(this._msgRemark, "#CCCCCC")
                    };

                    // 发送模板消息
                    var result1 = TemplateApi.SendTemplateMessage(accessToken,
                        weiXinId,
                        WeiXinConst.C_Template_Message,
                        "#FF0000",
                        "",//不出现详细了
                        msgData);
                    if (result1.errcode != 0)
                    {
                        strError = result1.errmsg;
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("给读者" + patronName + "发送'以停代金到期'通知异常：" + ex.Message);
                }
            }


            return 1;
        }

        /// <returns>
        /// -1 出错，格式出错或者发送模板消息出错
        /// 0 未绑定微信id
        /// 1 成功
        /// </returns>
        private int SendReturnPayMsg(XmlDocument bodyDom, out string strError)
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

            string patronName = "";
            List<string> weiXinIdList = this.GetWeiXinIds(bodyDom, out patronName);
            if (weiXinIdList.Count == 0)
            {
                strError = "未绑定微信id";
                return 0;
            }

            XmlNode root = bodyDom.DocumentElement;
            XmlNode nodeOperTime = root.SelectSingleNode("operTime");
            if (nodeOperTime == null)
            {
                strError = "尚未定义<borrowDate>节点";
                return -1;
            }
            string operTime = DomUtil.GetNodeText(nodeOperTime);
            operTime = DateTimeUtil.ToLocalTime(operTime, "yyyy/MM/dd");


            XmlNodeList listOverdue = root.SelectNodes("items/overdue");
            string barcodes = "";
            double totalPrice = 0;
            foreach (XmlNode node in listOverdue)
            {
                string oneBarcode = DomUtil.GetAttr(node, "barcode");
                if (barcodes != "")
                    barcodes += ",";
                barcodes += oneBarcode;

                string price = DomUtil.GetAttr(node, "price");
                if (String.IsNullOrEmpty(price) == false && price.Length > 3)
                {
                    double dPrice = Convert.ToDouble(price.Substring(3));
                    totalPrice += dPrice;
                }
            }

            foreach (string weiXinId in weiXinIdList)
            {
                try
                { 
                var accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);

                //{{first.DATA}}
                //退款原因：{{reason.DATA}}
                //退款金额：{{refund.DATA}}
                //{{remark.DATA}}
                var msgData = new ReturnPayTemplateData()
                {
                    first = new TemplateDataItem("━━━━━━$━━━━━━", "#B8860B"),  // ☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆ 	dark golden rod//this._msgFirstLeft + "撤消交费成功！"
                    reason = new TemplateDataItem("撤消[" + barcodes + "]交费。", "#000000"),//text.ToString()),// "请让我慢慢长大"),
                    refund = new TemplateDataItem("CNY" + totalPrice, "#000000"),
                    remark = new TemplateDataItem(this._msgRemark, "#CCCCCC")
                };

                // 发送模板消息
                var result1 = TemplateApi.SendTemplateMessage(accessToken,
                    weiXinId,
                    WeiXinConst.C_Template_ReturnPay,
                    "#FF0000",
                    "",//不出现详细了
                    msgData);
                if (result1.errcode != 0)
                {
                    strError = result1.errmsg;
                    return -1;
                }
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("给读者" + patronName + "发送'撤消交费成功'通知异常：" + ex.Message);
                }
            }


            return 1;
        }

        /// <returns>
        /// -1 出错，格式出错或者发送模板消息出错
        /// 0 未绑定微信id
        /// 1 成功
        /// </returns>
        private int SendPayMsg(XmlDocument bodyDom, out string strError)
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

            string patronName = "";
            List<string> weiXinIdList = this.GetWeiXinIds(bodyDom, out patronName);
            if (weiXinIdList.Count == 0)
            {
                strError = "未绑定微信id";
                return 0;
            }

            XmlNode root = bodyDom.DocumentElement;
            XmlNode nodeOperTime = root.SelectSingleNode("operTime");
            if (nodeOperTime == null)
            {
                strError = "尚未定义<borrowDate>节点";
                return -1;
            }
            string operTime = DomUtil.GetNodeText(nodeOperTime);
            operTime = DateTimeUtil.ToLocalTime(operTime, "yyyy/MM/dd");


            XmlNodeList listOverdue = root.SelectNodes("items/overdue");
            string barcodes = "";
            double totalPrice = 0;
            string reasons = "";
            foreach (XmlNode node in listOverdue)
            {
                string oneBarcode = DomUtil.GetAttr(node, "barcode");
                if (barcodes != "")
                    barcodes += ",";
                barcodes += oneBarcode;

                string price = DomUtil.GetAttr(node, "price");
                if (String.IsNullOrEmpty(price) == false && price.Length > 3)
                {
                    double dPrice = Convert.ToDouble(price.Substring(3));
                    totalPrice += dPrice;
                }

                string oneReason = DomUtil.GetAttr(node, "reason");
                if (reasons != "")
                    reasons += ",";
                reasons += oneReason;
            }

            foreach (string weiXinId in weiXinIdList)
            {

                try
                {
                    var accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);

                    //{{first.DATA}}
                    //订单号：{{keyword1.DATA}}
                    //缴费人：{{keyword2.DATA}}
                    //缴费金额：{{keyword3.DATA}}
                    //费用类型：{{keyword4.DATA}}
                    //缴费时间：{{keyword5.DATA}}
                    //{{remark.DATA}}
                    //您好，您已缴费成功！
                    //订单号：书名（册条码号）
                    //缴费人：张三
                    //缴费金额：￥100.00
                    //费用类型：违约
                    //缴费时间：2015-12-27 13:15
                    //如有疑问，请联系学校管理员，感谢您的使用！、
                    var msgData = new PayTemplateData()
                    {
                        first = new TemplateDataItem("++++++$++++++", "#556B2F"),//★★★★★★★★★★★★★★★ dark olive green//this._msgFirstLeft+"您已交费成功！"
                        keyword1 = new TemplateDataItem(barcodes, "#000000"),//text.ToString()),// "请让我慢慢长大"),
                        keyword2 = new TemplateDataItem(patronName, "#000000"),
                        keyword3 = new TemplateDataItem("CNY" + totalPrice, "#000000"),
                        keyword4 = new TemplateDataItem(reasons, "#000000"),
                        keyword5 = new TemplateDataItem(operTime, "#000000"),
                        remark = new TemplateDataItem(this._msgRemark, "#CCCCCC")
                    };

                    // 发送模板消息
                    var result1 = TemplateApi.SendTemplateMessage(accessToken,
                        weiXinId,
                        WeiXinConst.C_Template_Pay,
                        "#FF0000",
                        "",//不出现详细了
                        msgData);
                    if (result1.errcode != 0)
                    {
                        strError = result1.errmsg;
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("给读者" + patronName + "发送交费成功通知异常：" + ex.Message);
                }
            }


            return 1;
        }

        /// <returns>
        /// -1 出错，格式出错或者发送模板消息出错
        /// 0 未绑定微信id
        /// 1 成功
        /// </returns>
        private int SendReturnMsg(XmlDocument bodyDom, out string strError)
        {
            strError = "";
            /*
 <?xml version="1.0" encoding="utf-8"?>
<root>
    <type>还书成功</type>
    <libraryCode></libraryCode>
    <operation>return</operation>
    <action>return</action>
    <itemBarcode>0000001</itemBarcode>
    <readerBarcode>R0000001</readerBarcode>
    <operator>supervisor</operator>
    <operTime>Sun, 22 May 2016 13:11:33 +0800</operTime>
    <clientAddress>::1</clientAddress>
    <version>1.02</version>
    <uid>4a9730b1-a6d7-4fd5-9e6f-57c074f73661</uid>
    <patronRecord>
        <barcode>R0000001</barcode>
        <readerType>本科生</readerType>
        <name>张三</name>
        <borrows>
        </borrows>
        <refID>be13ecc5-6a9c-4400-9453-a072c50cede1</refID>
        <reservations>
        </reservations>
        <department>/</department>
        <address>address</address>
        <cardNumber>C12345</cardNumber>
        <overdues>
        </overdues>
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
        <summary>船舶柴油机 / 聂云超主编. -- ISBN 7-81007-115-7 : $4.65</summary>
    </itemRecord>
</root>
        
           */

            string patronName = "";
            List<string> weiXinIdList = this.GetWeiXinIds(bodyDom, out patronName);
            if (weiXinIdList.Count == 0)
            {
                strError = "未绑定微信id";
                return 0;
            }

            XmlNode root = bodyDom.DocumentElement;
            //<itemBarcode>0000001</itemBarcode>
            //<borrowDate>Sun, 22 May 2016 19:48:01 +0800</borrowDate>
            //<borrowPeriod>31day</borrowPeriod>
            //<returningDate>Wed, 22 Jun 2016 12:00:00 +0800</returningDate>
            XmlNode nodeItemBarcode = root.SelectSingleNode("itemBarcode");
            if (nodeItemBarcode == null)
            {
                strError = "尚未定义<itemBarcode>节点";
                return -1;
            }
            string itemBarcode = DomUtil.GetNodeText(nodeItemBarcode);

            XmlNode nodeOperTime = root.SelectSingleNode("operTime");
            if (nodeOperTime == null)
            {
                strError = "尚未定义<borrowDate>节点";
                return -1;
            }
            string operTime = DomUtil.GetNodeText(nodeOperTime);
            operTime = DateTimeUtil.ToLocalTime(operTime, "yyyy/MM/dd");


            XmlNode nodeSummary = root.SelectSingleNode("itemRecord/summary");
            if (nodeSummary == null)
            {
                strError = "尚未定义itemRecord/summary节点";
                return -1;
            }
            string summary = DomUtil.GetNodeText(nodeSummary);

            // 检查是否有超期信息
            string remark = "\n" + patronName + "，感谢及时归还，欢迎继续借书。";
            XmlNodeList listOverdue = root.SelectNodes("patronRecord/overdues/overdue");
            if (listOverdue.Count > 0)
            {
                remark = "\n"+patronName+"，您有" + listOverdue.Count + "笔超期违约记录，请履行超期手续。";
            }


            foreach (string weiXinId in weiXinIdList)
            {
                try
                {
                    var accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);

                    //{{first.DATA}}
                    //书名：{{keyword1.DATA}}
                    //归还时间：{{keyword2.DATA}}
                    //借阅人：{{keyword3.DATA}}
                    //{{remark.DATA}}    
                    //您好,你借阅的图书已确认归还.
                    //书名：算法导论
                    //归还时间：2015-10-10 12:14
                    //借阅人：李明
                    //欢迎继续借书!
                    var msgData = new ReturnTemplateData()
                    {
                        first = new TemplateDataItem("▉▊▋▍▎▉▊▋▍▎▉▊▋▍▎", "#00008B"),  // 	dark blue//this._msgFirstLeft + "您借出的图书已确认归还。"
                        keyword1 = new TemplateDataItem(summary, "#000000"),//text.ToString()),// "请让我慢慢长大"),
                        keyword2 = new TemplateDataItem(operTime, "#000000"),
                        keyword3 = new TemplateDataItem(patronName, "#000000"),
                        remark = new TemplateDataItem(remark, "#CCCCCC")
                    };

                    // 发送模板消息
                    var result1 = TemplateApi.SendTemplateMessage(accessToken,
                        weiXinId,
                        WeiXinConst.C_Template_Return,
                        "#00008B",
                        "",//不出现详细了
                        msgData);
                    if (result1.errcode != 0)
                    {
                        strError = result1.errmsg;
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("给读者" + patronName + "发送还书成功通知异常：" + ex.Message);
                }
            }


            return 1;
        }

        /// <returns>
        /// -1 出错，格式出错或者发送模板消息出错
        /// 0 未绑定微信id
        /// 1 成功
        /// </returns>
        private int SendBorrowMsg(XmlDocument bodyDom, out string strError)
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

            string patronName = "";
            List<string> weiXinIdList = this.GetWeiXinIds(bodyDom, out patronName);
            if (weiXinIdList.Count == 0)
            {
                strError = "未绑定微信id";
                return 0;
            }

            XmlNode root = bodyDom.DocumentElement;
  //<itemBarcode>0000001</itemBarcode>
  //<borrowDate>Sun, 22 May 2016 19:48:01 +0800</borrowDate>
  //<borrowPeriod>31day</borrowPeriod>
  //<returningDate>Wed, 22 Jun 2016 12:00:00 +0800</returningDate>
            XmlNode nodeItemBarcode = root.SelectSingleNode("itemBarcode");
            if (nodeItemBarcode == null)
            {
                strError = "尚未定义<itemBarcode>节点";
                return -1;
            }
            string itemBarcode = DomUtil.GetNodeText(nodeItemBarcode);

            XmlNode nodeBorrowDate = root.SelectSingleNode("borrowDate");
            if (nodeBorrowDate == null)
            {
                strError = "尚未定义<borrowDate>节点";
                return -1;
            }
            string borrowDate = DomUtil.GetNodeText(nodeBorrowDate);
            borrowDate=DateTimeUtil.ToLocalTime(borrowDate, "yyyy/MM/dd");

            XmlNode nodeBorrowPeriod = root.SelectSingleNode("borrowPeriod");
            if (nodeBorrowPeriod == null)
            {
                strError = "尚未定义<borrowPeriod>节点";
                return -1;
            }
            string borrowPeriod = DomUtil.GetNodeText(nodeBorrowPeriod);

            XmlNode nodeReturningDate = root.SelectSingleNode("returningDate");
            if (nodeReturningDate == null)
            {
                strError = "尚未定义<returningDate>节点";
                return -1;
            }
            string returningDate = DomUtil.GetNodeText(nodeReturningDate);
            returningDate = DateTimeUtil.ToLocalTime(returningDate, "yyyy/MM/dd");


            XmlNode nodeSummary = root.SelectSingleNode("itemRecord/summary");
            if (nodeSummary == null)
            {
                strError = "尚未定义itemRecord/summary节点";
                return -1;
            }
            string summary = DomUtil.GetNodeText(nodeSummary);

            foreach (string weiXinId in weiXinIdList)
            {
                try
                {
                    var accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);

                    //尊敬的XXX，恭喜您借书成功。
                    //图书书名：C#开发教程
                    //册条码号：C0000001
                    //借阅日期：2016-5-27
                    //借阅期限：31
                    //应还日期：2016-6-27
                    //祝您阅读愉快，欢迎再借。
                    var msgData = new BorrowTemplateData()
                    {
                        first = new TemplateDataItem("▉▊▋▍▎▉▊▋▍▎▉▊▋▍▎", "#006400"), // 	dark green //this._msgFirstLeft + "恭喜您借书成功。"
                        keyword1 = new TemplateDataItem(summary, "#000000"),//text.ToString()),// "请让我慢慢长大"),
                        keyword2 = new TemplateDataItem(itemBarcode, "#000000"),
                        keyword3 = new TemplateDataItem(borrowDate, "#000000"),
                        keyword4 = new TemplateDataItem(borrowPeriod, "#000000"),
                        keyword5 = new TemplateDataItem(returningDate, "#000000"),
                        remark = new TemplateDataItem("\n"+patronName+"，祝您阅读愉快，欢迎再借。", "#CCCCCC")
                    };

                    // 发送模板消息
                    var result1 = TemplateApi.SendTemplateMessage(accessToken,
                        weiXinId,
                        WeiXinConst.C_Template_Borrow,
                        "#006400",  //FF0000
                        "",//不出现详细了
                        msgData);
                    if (result1.errcode != 0)
                    {
                        strError = result1.errmsg;
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("给读者" + patronName + "发送借书成功通知异常：" + ex.Message);
                }
            }
            return 1;
        }



        /// <summary>
        /// 发送预约通知
        /// </summary>
        /// <param name="bodyDom"></param>
        /// <param name="strError"></param>
        /// <returns>
        /// -1 出错，格式出错或者发送模板消息出错
        /// 0 未绑定微信id
        /// 1 成功
        /// </returns>
        private int SendArrived(XmlDocument bodyDom, out string strError)
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

            string patronName = "";
            List<string> weiXinIdList = this.GetWeiXinIds(bodyDom, out patronName);
            if (weiXinIdList.Count == 0)
            {
                strError = "未绑定微信id";
                return 0;
            }

            XmlNode root = bodyDom.DocumentElement;
            //<onShelf>false</onShelf>
            // <reserveTime>2天</reserveTime>
            // <today>2016/5/17 10:10:59</today>
            // 取出预约消息
            XmlNode nodeSummary = root.SelectSingleNode("summary");
            if (nodeSummary == null)
            {
                strError = "尚未定义<summary>节点";
                return -1;
            }
            string summary = DomUtil.GetNodeText(nodeSummary);

            XmlNode nodeReserveTime = root.SelectSingleNode("reserveTime");
            if (nodeReserveTime == null)
            {
                strError = "尚未定义<reserveTime>节点";
                return -1;
            }
            string reserveTime = DomUtil.GetNodeText(nodeReserveTime);

            XmlNode nodeToday = root.SelectSingleNode("today");
            if (nodeToday == null)
            {
                strError = "尚未定义<today>节点";
                return -1;
            }
            string today = DomUtil.GetNodeText(nodeToday);

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
            XmlNode nodeItemBarcode = root.SelectSingleNode("itemBarcode");
            if (nodeItemBarcode == null)
            {
                strError = "尚未定义<itemBarcode>节点";
                return -1;
            }
            string itemBarcode = DomUtil.GetNodeText(nodeItemBarcode);

            //string first = this._msgFirstLeft+"我们很高兴地通知您，您预约的图书到了，请尽快来图书馆办理借书手续。";
            string end = "\n" + patronName + "，您预约的图书["+itemBarcode+"]到了，请尽快来图书馆办理借书手续，请尽快来图书馆办理借书手续。如果您未能在保留期限内来馆办理借阅手续，图书馆将把优先借阅权转给后面排队等待的预约者，或做归架处理。";
            if (bOnShelf == true)
            {
                //first = this._msgFirstLeft + "我们很高兴地通知您，您预约的图书已经在架上，请尽快来图书馆办理借书手续。";
                end = "\n" + patronName + "，您预约的图书[" + itemBarcode + "]已经在架上，请尽快来图书馆办理借书手续。如果您未能在保留期限内来馆办理借阅手续，图书馆将把优先借阅权转给后面排队等待的预约者，或允许其他读者借阅。";
            }

            foreach (string weiXinId in weiXinIdList)
            {
                try
                {
                    var accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);

                    //{{first.DATA}}
                    //图书书名：{{keyword1.DATA}}
                    //到书日期：{{keyword2.DATA}}
                    //保留期限：{{keyword3.DATA}}
                    //{{remark.DATA}}
                    var msgData = new ArrivedTemplateData()
                    {
                        first = new TemplateDataItem("▉▉▉▉▉▉▉▉▉▉▉▉▉▉▉", "#FF8C00"),//  dark orange   	yellow 	#FFFF00
                        keyword1 = new TemplateDataItem(summary, "#000000"),//text.ToString()),// "请让我慢慢长大"),
                        keyword2 = new TemplateDataItem(today, "#000000"),
                        keyword3 = new TemplateDataItem("保留" + reserveTime, "#000000"),
                        remark = new TemplateDataItem(end, "#CCCCCC")
                    };

                    // 发送预约模板消息
                    //string detailUrl = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx57aa3682c59d16c2&redirect_uri=http%3a%2f%2fdp2003.com%2fdp2weixin%2fPatron%2fIndex&response_type=code&scope=snsapi_base&state=dp2weixin#wechat_redirect";
                    var result1 = TemplateApi.SendTemplateMessage(accessToken,
                        weiXinId,
                        WeiXinConst.C_Template_Arrived,
                        "#FF0000",
                        "",//不出现详细了
                        msgData);
                    if (result1.errcode != 0)
                    {
                        strError = result1.errmsg;
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    this.WriteErrorLog("给读者" + patronName + "发送预约到书通知异常：" + ex.Message);
                }
            }


            return 1;
        }

        /// <summary>
        /// 发送超期通知
        /// </summary>
        /// <param name="bodyDom"></param>
        private int SendCaoQi(XmlDocument bodyDom, out string strError)
        {
            strError = "";

            /*
<root>
    <type>超期通知</type>
    <items overdueCount="1" normalCount="0">
        <item summary="船舶柴油机 / 聂云超主编. -- ISBN 7-..." timeReturning="2016/5/18" overdue="已超期 31 天" overdueType="overdue" />
    </items>
    <text>您借阅的下列书刊：
船舶柴油机 / 聂云超主编. -- ISBN 7-... 应还日期: 2016/5/18 已超期 31 天
</text>
    <patronRecord>...
    </patronRecord>
</root>
            
//overdueType是超期类型，overdue表示超期，warning表示即将超期。             
           */

            // 得到绑定的微信id
            string patronName = "";
            List<string> weiXinIdList = this.GetWeiXinIds(bodyDom, out patronName);
            if (weiXinIdList.Count == 0)
            {
                strError = "未绑定微信id";
                return 0;
            }

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

                //overdueType是超期类型，overdue表示超期，warning表示即将超期。
                string templateId = "";
                string overdueType = DomUtil.GetAttr(item, "overdueType");
                //string first = "";
                string end = "";
                if (overdueType == "overdue")
                {
                    templateId = WeiXinConst.C_Template_CaoQi;
                    //first = this._msgFirstLeft+"您借出的图书已超期，请尽快归还。";
                    end = "\n"+patronName+"，您借出的图书已超期，请尽快归还。";
                }
                else if (overdueType == "warning")
                {
                    templateId = WeiXinConst.C_Template_DaoQi;
                    // first = this._msgFirstLeft+"您借出的图书即将到期，请注意不要超期，留意归还。";
                    end = "\n" + patronName + "，您借出的图书即将到期，请注意不要超期，留意归还。";
                }
                else 
                {
                    strError ="overdueType属性值[]不合法。";
                    return -1;//整个不处理 //continue;                    
                }           

                foreach (string weiXinId in weiXinIdList)
                {
                    try
                    {
                        var accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);

                        //{{first.DATA}}
                        //图书书名：{{keyword1.DATA}}
                        //应还日期：{{keyword2.DATA}}
                        //超期天数：{{keyword3.DATA}}
                        //{{remark.DATA}}

                        //{{first.DATA}}
                        //图书书名：{{keyword1.DATA}}
                        //归还日期：{{keyword2.DATA}}
                        //剩余天数：{{keyword3.DATA}}
                        //{{remark.DATA}}
                        //超期和到期格式一样，就不用再建一个TemplateData类了
                        var msgData = new ArrivedTemplateData()
                        {
                            first = new TemplateDataItem("▉▉▉▉▉▉▉▉▉▉▉▉▉▉▉", "#FFFF00"), //yellow 	#

                            keyword1 = new TemplateDataItem(summary, "#000000"),//text.ToString()),// "请让我慢慢长大"),
                            keyword2 = new TemplateDataItem(timeReturning, "#000000"),
                            keyword3 = new TemplateDataItem(overdue, "#000000"),
                            remark = new TemplateDataItem(end, "#CCCCCC")//"\n点击下方”详情“查看个人详细信息。"
                        };

                        string detailUrl = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx57aa3682c59d16c2&redirect_uri=http%3a%2f%2fdp2003.com%2fdp2weixin%2fPatron%2fIndex&response_type=code&scope=snsapi_base&state=dp2weixin#wechat_redirect";
                        var result1 = TemplateApi.SendTemplateMessage(accessToken,
                            weiXinId,
                            templateId,
                            "#FF0000",
                            detailUrl,//不出现详细了
                            msgData);
                        if (result1.errcode != 0)
                        {
                            strError = result1.errmsg;
                            return -1;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.WriteErrorLog("给读者"+patronName+"发送超期通知异常："+ex.Message);
                    }
                }
            }

            // 发送成功
            return 1;
        }

        /// <summary>
        /// 获取读者记录中绑定的微信id,返回数组
        /// </summary>
        /// <param name="bodyDom"></param>
        /// <param name="patronName"></param>
        /// <returns></returns>
        private List<string> GetWeiXinIds(XmlDocument bodyDom, out string patronName)
        {
            patronName = "";

            XmlNode root = bodyDom.DocumentElement;
            XmlNode patronRecordNode = root.SelectSingleNode("patronRecord");
            if (patronRecordNode == null)
                throw new Exception("尚未定义<patronRecordNode>节点");
            patronName = DomUtil.GetNodeText(patronRecordNode.SelectSingleNode("name"));
            XmlNode emailNode = patronRecordNode.SelectSingleNode("email");
            if (emailNode == null)
                throw new Exception("尚未定义<email>节点");
            string email = DomUtil.GetNodeText(emailNode);
            //<email>test@163.com,123,weixinid:o4xvUviTxj2HbRqbQb9W2nMl4fGg,weixinid:o4xvUvnLTg6NnflbYdcS-sxJCGFo,weixinid:testid</email>
            string[] emailList = email.Split(new char[] { ',' });
            List<string> weiXinIdList = new List<string>();
            for (int i = 0; i < emailList.Length; i++)
            {
                string oneEmail = emailList[i].Trim();
                if (oneEmail.Length > 9 && oneEmail.Substring(0, 9) == WeiXinConst.C_WeiXinIdPrefix)
                {
                    string weiwinId = oneEmail.Substring(9).Trim();
                    if (weiwinId != "")
                        weiXinIdList.Add(weiwinId);
                }
            }
            return weiXinIdList;
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




        #region 找回密码，修改密码，二维码

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="strLibraryCode"></param>
        /// <param name="name"></param>
        /// <param name="tel"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int ResetPassword(string libId,
            string name,
            string tel,
            out string strError)
        {
            strError = "";

            LibItem lib = LibDatabase.Current.GetLibById(libId);
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
            CancellationToken cancel_token = new CancellationToken();

            string id = Guid.NewGuid().ToString();
            CirculationRequest request = new CirculationRequest(id,
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
                    strError = "出错：" + result.ErrorInfo;
                    return -1;
                }

                if (result.Value == 0)
                {
                    if (result.String == "NotFound")
                    {
                        strError = "操作未成功：读者 " + name + " 尚未在图书馆账户中注册过手机号码，因此无法找回密码。请先去图书馆出纳台请工作人员帮助注册一下手机号码。";
                    }
                    else
                    {
                        strError = "操作未成功：" + result.ErrorInfo;
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
                    strError = "向读者 '" + strBarcode + "' 发送" + external_interface.Type + " message时出错: " + strError;

                    this.WriteErrorLog(strError);
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
            LibItem lib = LibDatabase.Current.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                goto ERROR1;
            }

            // 注意新旧两个密码参数即便为空，也应该是 "" 而不应该是 null。
            // 所以在使用 Item 参数的时候，old 和 new 两个子参数的任何一个都不该被省略。
            // 省略子参数的用法是有意义的，但不该被用在这个修改读者密码的场合。
            string item = "old=" + oldPassword + ",new=" + newPassword;

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            CirculationRequest request = new CirculationRequest(id,
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

        public int GetQRcode(string libId,
            string patronBarcode,
            out string code,
            out string strError)
        {
            strError = "";
            code = "";

            LibItem lib = LibDatabase.Current.GetLibById(libId);
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

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            CirculationRequest request = new CirculationRequest(id,
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
                    strError = "出错：" + result.ErrorInfo;
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

        #endregion

        #region 绑定解绑
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strBarcode"></param>
        /// <param name="strPassword"></param>
        /// <param name="weiXinId"></param>
        /// <returns>
        /// -1 出错
        /// 0 成功
        /// </returns>
        public int Bind(string libId,
            string strPrefix,
            string strWord,
            string strPassword,
            string strWeiXinId,
            out WxUserItem userItem,
            out string strError)
        {
            userItem = null;
            strError = "";
            long lRet = -1;

            LibItem lib = LibDatabase.Current.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为["+libId+"]的图书馆定义。";
                return -1;
            }


            string strFullWord = strWord;
            if (string.IsNullOrEmpty(strPrefix) == false)
                strFullWord = strPrefix + ":" + strWord;

            CancellationToken cancel_token = new CancellationToken();

            string fullWeixinId = WeiXinConst.C_WeiXinIdPrefix + strWeiXinId;
            string id = Guid.NewGuid().ToString();
            BindPatronRequest request = new BindPatronRequest(id,
                "bind",
                strFullWord,
                strPassword,
                fullWeixinId,
                "single",   // "multiple",由于工作人员是single的用户，先统一设为single,multiple用法不常见
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
                    strError = result.ErrorInfo;
                    return -1;
                }

                // 获取需要缓存的信息
                string xml = result.Results[0];
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(xml);
                XmlNode rootNode = dom.DocumentElement;

                // 读者信息
                string readerBarcode = "";
                string readerName = "";
                string refID = "";
                string department = "";

                // 工作人员信息
                string userName = "";

                // 分馆代码，读者与工作人员共有
                string libraryCode = "";

                // 账户类型
                int type = 0;//账户类型：0表示读者 1表示工作人员

                // 工作人员账户
                if (strPrefix == "UN")
                {
                    type = 1;// 工作人员账户
                    userName = DomUtil.GetAttr(rootNode, "name");
                    libraryCode = DomUtil.GetAttr(rootNode, "libraryCode");
                }
                else
                {
                    type = 0;//读者
                    // 证条码号
                    readerBarcode = DomUtil.GetNodeText(rootNode.SelectSingleNode("barcode"));
                    // 姓名
                    XmlNode nodeName = rootNode.SelectSingleNode("name");
                    if (nodeName != null)
                        readerName = DomUtil.GetNodeText(nodeName);
                    //参考id
                    XmlNode nodeRefID = rootNode.SelectSingleNode("refID");
                    if (nodeRefID != null)
                        refID = DomUtil.GetNodeText(nodeRefID);
                    // 部门
                    XmlNode nodeDept = rootNode.SelectSingleNode("department");
                    if (nodeDept != null)
                        department = DomUtil.GetNodeText(nodeDept);
                    // 分馆代码
                    XmlNode nodelibraryCode = rootNode.SelectSingleNode("libraryCode");
                    if (nodelibraryCode != null)
                        libraryCode = DomUtil.GetNodeText(nodelibraryCode);
                }

                // 找到库中对应的记录
                if (type == 0)
                    userItem = WxUserDatabase.Current.GetPatronAccount(strWeiXinId, libId, readerBarcode);
                else
                    userItem = WxUserDatabase.Current.GetWorker(strWeiXinId, libId);

                // 是否新增，对于工作人员账户，一个图书馆只绑一个工作人员，所以有update的情况
                bool bNew = false;
                if (userItem == null)
                {
                    bNew = true;
                    userItem = new WxUserItem();
                }

                userItem.weixinId = strWeiXinId;                
                //userItem.libCode = libCode;
                //userItem.libUserName = remoteUserName;
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

                userItem.prefix = strPrefix;
                userItem.word = strWord;
                userItem.fullWord = strFullWord;
                userItem.password = strPassword;

                userItem.libraryCode = libraryCode;
                userItem.type = type;
                userItem.userName = userName;
                userItem.isActiveWorker = 0;//是否是激活的工作人员账户，读者时均为0

                if (bNew == true)
                    WxUserDatabase.Current.Add(userItem);
                else
                    lRet = WxUserDatabase.Current.Update(userItem);

                if (type == 0)
                {
                    // 置为活动状态
                    this.SetActivePatron(userItem);
                }



                // 发送绑定成功的客服消息    
                string strFirst = "恭喜您！您已成功绑定图书馆读者账号。";
                string strAccount = userItem.readerName + "(" + userItem.readerBarcode + ")";
                string strRemark = "您可以直接通过微信公众号访问图书馆，进行信息查询，预约续借等功能。如需解绑，请通过“绑定账号”菜单操作。";
                if (type == 1)
                {
                    strFirst = "恭喜您！您已成功绑定图书馆工作人员账号。";
                    strAccount=userItem.userName;
                    strRemark = "欢迎您使用微信公众号管理图书馆业务，如需解绑，请通过“绑定账号”菜单操作。";
                }

                string accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);
                var testData = new BindTemplateData()
                {
                    first = new TemplateDataItem(strFirst, "#000000"),
                    keyword1 = new TemplateDataItem(strAccount, "#000000"),
                    keyword2 = new TemplateDataItem("图书馆[" + userItem.libName + "]", "#000000"),
                    remark = new TemplateDataItem(strRemark, "#CCCCCC")
                };
                // 详细转到账户管理界面
                string detailUrl = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx57aa3682c59d16c2&redirect_uri=http%3a%2f%2fdp2003.com%2fdp2weixin%2fAccount%2fIndex&response_type=code&scope=snsapi_base&state=dp2weixin#wechat_redirect";
                var result1 = TemplateApi.SendTemplateMessage(accessToken,
                    strWeiXinId,
                    WeiXinConst.C_Template_Bind,
                    "#FF0000",
                    detailUrl,//k"dp2003.com/dp2weixin/patron/index", // todo注意这里是否需要oauth接口，想着消息既然是从web发过来了，立即点进去还有session信息存在，但时间长了session失效就没有信息了
                    testData);
                if (result1.errcode != 0)
                {
                    strError = result1.errmsg;
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





        /// <summary>
        /// 
        /// </summary>
        /// <param name="weiXinId"></param>
        /// <param name="strError"></param>
        /// <returns>
        /// -1 出错
        /// 0   成功
        /// </returns>
        public  int Unbind(string userId,
             out string strError)
        {
            strError = "";

            WxUserItem userItem = WxUserDatabase.Current.GetById(userId);
            if (userItem == null)
            {
                strError = "绑定账号未找到";
                return -1;
            }

            LibItem lib = LibDatabase.Current.GetLibById(userItem.libId);
            if (lib == null)
            {
                strError = "未找到id为[" + userItem.libId + "]的图书馆定义。";
                return -1;
            }


            // 调点对点解绑接口
            string fullWeixinId = WeiXinConst.C_WeiXinIdPrefix + userItem.weixinId;
            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            BindPatronRequest request = new BindPatronRequest(id,
                "unbind",
                userItem.readerBarcode,
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
                    //strError = result.ErrorInfo;
                    //return -1;
                }

                // 删除mongodb库的记录
                this.DeletePatron(userId);

                // 发送解绑消息    
                string strFirst = "您已成功对图书馆读者账号解除绑定。";
                string strAccount = userItem.readerName + "(" + userItem.readerBarcode + ")";
                string strRemark="\n您现在不能管理该图书馆的个人信息了，如需访问，请重新绑定。";
                if (userItem.type == WxUserDatabase.C_Type_Worker)
                {
                    strFirst="您已成功对图书馆工作人员账号解除绑定。";
                    strAccount=userItem.userName;
                    strRemark="\n您现在不能对该图书馆进行管理工作了，如需访问，请重新绑定。";
                }

                        
                string accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);
                var data = new UnBindTemplateData()
                {
                    first = new TemplateDataItem(strFirst, "#000000"),
                    keyword1 = new TemplateDataItem(strAccount, "#000000"),
                    keyword2 = new TemplateDataItem("图书馆[" + userItem.libName + "]", "#000000"),
                    remark = new TemplateDataItem(strRemark, "#CCCCCC")
                };
                SendTemplateMessageResult result1 = TemplateApi.SendTemplateMessage(accessToken,
                    userItem.weixinId,
                    WeiXinConst.C_Template_UnBind,
                    "#FF0000",
                    "",//k"dp2003.com/dp2weixin/patron/index", // todo注意这里是否需要oauth接口，想着消息既然是从web发过来了，立即点进去还有session信息存在，但时间长了session失效就没有信息了
                    data);
                if (result1.errcode != 0)
                {
                    strError = result1.errmsg;
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



        #endregion

        #region 检索书目

        /// <summary>
        /// 检索书目
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="strFrom"></param>
        /// <param name="strWord"></param>
        /// <returns></returns>
        public SearchBiblioResult SearchBiblio(string libId,
            string strFrom,
            string strWord,
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
            long lRet = this.SearchBiblio1(libId,
                strFrom,
                strWord,
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

        public SearchBiblioResult getFromResultSet(string libId, string resultSet, long start, long count)
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
            long lRet = this.SearchBiblio1(libId,
                 "",
                 "!getResult",
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
        public long SearchBiblio1(string libId,
            string strFrom,
            string strWord,
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

            LibItem lib = LibDatabase.Current.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                return -1;
            }

            //long start = 0;
            //long count = 10;
            try
            {

                CancellationToken cancel_token = new CancellationToken();
                string id = Guid.NewGuid().ToString();
                SearchRequest request = new SearchRequest(id,
                    "searchBiblio",
                    "",
                    strWord,
                    strFrom,
                    "middle",
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
                if (result.ResultCount == -1)
                {
                    strError = "检索出错：" + result.ErrorInfo;
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
                    record.no = (i+start + 1).ToString();//todo 注意下一页的时候
                    record.libId = libId;
                    records.Add(record);
                }

                // 检查是否有下页
                if (start + records.Count < result.ResultCount)
                    bNext = true;

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

        public BiblioRecordResult GetBiblioDetail(string libId,
            string biblioPath)
        {
            BiblioRecordResult result = new BiblioRecordResult();
            result.errorCode = 0;
            result.biblioPath = biblioPath;

            LibItem lib = LibDatabase.Current.GetLibById(libId);
            if (lib == null)
            {
                result.errorInfo= "未找到id为[" + libId + "]的图书馆定义。";
                result.errorCode = -1;
                return result;
            }


            DateTime start_time = DateTime.Now;

            try
            {
                string strError="";
                int nRet = 0;
                TimeSpan time_length = DateTime.Now - start_time;
                string logInfo = "";

                // 取出summary
                this.WriteLog("开始获取summary");
                string strSummary = "";
                string tempPath = "@bibliorecpath:" + biblioPath;
                string strOutputRecPath = "";
                nRet = this.GetBiblioSummary(lib.capoUserName,
                    tempPath,
                    "",out strSummary, out strOutputRecPath,out strError);
                if (nRet == -1 || nRet == 0)
                {
                    result.errorCode = -1;
                    result.errorInfo = strError;
                    return result;
                }
                result.summary = strSummary;
                time_length = DateTime.Now - start_time;
                string info = "获取[" + biblioPath + "]的summary信息完毕 time span: " + time_length.TotalSeconds.ToString() + " secs";
                this.WriteLog(info);

                //Thread.Sleep(1000);
                
                // 取item
                this.WriteLog("开始获取items");
                List<BiblioItem> itemList = null;
                nRet = (int)this.GetItemInfo(lib.capoUserName, biblioPath, out itemList, out strError);
                if (nRet == -1) //0的情况表示没有册，不是错误
                {
                    result.errorCode = -1;
                    result.errorInfo = strError;
                    return result;
                }

                // 计算用了多少时间
                time_length = DateTime.Now - start_time;
                logInfo = "获取[" + biblioPath + "]的item信息完毕 time span: " + time_length.TotalSeconds.ToString() + " secs";
                this.WriteLog(logInfo);

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
        public int GetBiblioSummary(string capoUserName,
            string word,
            string strBiblioRecPathExclude,
            out string summary,
            out string strRecPath,
            out string strError)
        {
            summary = "";
            strError = "";
            strRecPath = "";

            /*
            SearchRequest request = new SearchRequest(id,
                "getBiblioInfo",
                "<全部>",
                biblioPath,
                "",
                "",
                "",
                "summary",
                1,
                0,
                -1);             
             */

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
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
                    capoUserName).Result;  //+ "-1"

                SearchResult result = connection.SearchTaskAsync(
                    capoUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                if (result.ResultCount == -1)
                {
                    strError = "检索出错：" + result.ErrorInfo;
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


        private long GetItemInfo(string capoUserName,
            string biblioPath,
            out List<BiblioItem> itemList,
            out string strError)
        {
            itemList = new List<BiblioItem>();
            strError = "";

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString(); // "2-item";//
            SearchRequest request = new SearchRequest(id,
                "getItemInfo",
                "entity",
                biblioPath,
                "",
                "",
                "",
                "opac",
                10,
                0,
                -1);
            try
            {
                this.WriteLog("GetItemInfo1");

                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    this.dp2MServerUrl,
                    capoUserName).Result;  //+"-2"
                this.WriteLog("GetItemInfo2");

                //string strFilename = string.Format(this.weiXinLogDir + "/log_{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
                //connection.logFileName = strFilename;
                SearchResult result = null;
                try
                {
                    result = connection.SearchTaskAsync(
                       capoUserName,
                       request,
                       new TimeSpan(0, 1, 0),
                       cancel_token).Result;
                }
                catch (Exception ex)
                {
                    strError = "检索出错：[SearchAsync异常]" + ex.Message;
                    return -1;
                }

                this.WriteLog("GetItemInfo3");
                if (result.ResultCount == -1)
                {
                    strError = "检索出错：" + result.ErrorInfo;
                    return -1;
                }
                if (result.ResultCount == 0)
                {
                    strError = "未命中";
                    return 0;
                }

                this.WriteLog("GetItemInfo4");
                for (int i = 0; i < result.Records.Count; i++)
                {
                    BiblioItem item = new BiblioItem();

                    string xml = result.Records[i].Data;
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);

                    string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");
                    // 册条码号
                    string strViewBarcode = "";
                    if (string.IsNullOrEmpty(strBarcode) == false)
                        strViewBarcode = strBarcode;
                    else
                        strViewBarcode = "@refID:" + strRefID;  //"@refID:"
                    item.barcode = strViewBarcode;

                    //状态
                    item.state = DomUtil.GetElementText(dom.DocumentElement, "state");

                    //卷册
                    item.volumn = DomUtil.GetElementText(dom.DocumentElement, "volumn");


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
                    string strBorrowInfo = "在架";
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
                    if (string.IsNullOrEmpty(strBorrower) == false)
                        strBorrowInfo = "借阅者:*** 借阅时间:" + borrowDate + " 借期:" + borrowPeriod;

                    item.borrowInfo = strBorrowInfo;
                    item.borrower = strBorrower;
                    item.borrowDate = borrowDate;
                    item.borrowPeriod = borrowPeriod;

                    itemList.Add(item);
                }

                this.WriteLog("GetItemInfo5");
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


        // 获取多个item的summary
        public string GetBarcodesSummary(string capoUserName,
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
                int nRet = this.GetBiblioSummary(capoUserName,
                    strBarcode,
                    strPrevBiblioRecPath,
                    out strOneSummary,
                    out strBiblioRecPath,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    strOneSummary = strError;

                int tempIndex = strBiblioRecPath.IndexOf("@");
                if (tempIndex>0)
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


        /// <summary>
        /// 获取读者的预约信息
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="readerBarcode"></param>
        /// <param name="reservations"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int GetPatronReservation(string libId,
            string readerBarcode,
            out List<ReservationInfo> reservations,
            out string strError)
        {
            reservations = new List<ReservationInfo>();
            strError = "";

            string xml = "";
            int nRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                readerBarcode,
                "xml",
                out xml,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 0)
            {
                strError = "从dp2library未找到证条码号为'" + readerBarcode + "'的记录";
                return 0;
            }

            // 预约请求
            string strReservationWarningText = "";
            reservations = GetReservations(xml, out strReservationWarningText);

            return 1;
        }

        public BorrowInfoResult GetPatronBorrowInfos1(string libId,
            string readerBarcode)
        {
            BorrowInfoResult result = new BorrowInfoResult();
            result.errorCode = 0;
            result.errorInfo = "";

            LibItem lib = LibDatabase.Current.GetLibById(libId);
            if (lib == null)
            {
                result.errorCode =-1;
                result.errorInfo = "未找到id为[" + libId + "]的图书馆定义。";
                return result;
            }


            string strError = "";

            string xml = "";
            int nRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                readerBarcode,
                "advancexml",
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

        public PatronInfo ParseReaderXml(string strXml)
        {
            PatronInfo patronResult = new PatronInfo();

            // 取出个人信息
            Patron patron = new Patron();
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);
            patron.barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            patron.name = DomUtil.GetElementText(dom.DocumentElement, "name");
            patron.department = DomUtil.GetElementText(dom.DocumentElement, "department");
            patron.readerType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
            patron.state = DomUtil.GetElementText(dom.DocumentElement, "state");
            patron.createDate = DateTimeUtil.ToLocalTime(DomUtil.GetElementText(dom.DocumentElement, "createDate"), "yyyy/MM/dd");
            patron.expireDate = DateTimeUtil.ToLocalTime(DomUtil.GetElementText(dom.DocumentElement, "expireDate"), "yyyy/MM/dd");
            patron.comment = DomUtil.GetElementText(dom.DocumentElement, "comment");// +"测试革skdslfjsalfjsda;dfsajf;k;lllllllaslkjdfasssssfffffffffffffffffffffffffffffffsal;sdjflsafjsla;fdjadsl;fjsal;fjaslfjdaslfjaslfjlsafjsadlj我们枯叶sksdlfjasfljsaf;lasjf;aslfjsda;lfjsadlf";

            string strError = "";
            int nRet = CheckReaderExpireAndState(dom, out strError);
            if (nRet != 0)
                patron.isWarning = 1;


            // 赋给返回对象
            patronResult.patron = patron;

            // 警告信息，显示在头像旁边
            string strWarningText = "";

            // ***
            // 违约/交费信息
            string strOverdueWarningText = "";
            List<OverdueInfo> overdueList = GetOverdueInfo(strXml, out strOverdueWarningText);
            if (strOverdueWarningText != "")
                strWarningText += strOverdueWarningText;
            patronResult.overdueList = overdueList;


            //在借册
            string strBorrowWarningText = "";
            string maxBorrowCountString = "";
            string curBorrowCountString = "";
            List<BorrowInfo2> borrowList = GetBorrowInfo(strXml, out strBorrowWarningText, out maxBorrowCountString, out curBorrowCountString);
            if (strBorrowWarningText != "")
                strWarningText += strBorrowWarningText;
            patronResult.borrowList = borrowList;
            patron.maxBorrowCount = maxBorrowCountString;
            patron.curBorrowCount = curBorrowCountString;


            // 预约请求
            string strReservationWarningText = "";
            List<ReservationInfo> reservationList = GetReservations(strXml, out strReservationWarningText);
            if (strReservationWarningText!="")
                strWarningText += strReservationWarningText;
            patronResult.reservationList = reservationList;


            //提醒 信息
            patronResult.patron.warningText = strWarningText;

            return patronResult;
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
        /// <param name="strReaderBarocde"></param>
        /// <param name="strFormat"></param>
        /// <param name="xml"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int GetPatronXml(string libId,
            string strReaderBarocde,  //todo refID
            string strFormat,
            out string xml,
            out string strError)
        {
            xml = "";
            strError = "";

            LibItem lib = LibDatabase.Current.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                goto ERROR1;
            }

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
                "getPatronInfo",
                "",
                strReaderBarocde,
                "",
                "",
                "",
                strFormat,
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
                    strError = result.ErrorInfo;
                    return -1;
                }

                if (result.ResultCount == 0)
                {
                    strError = result.ErrorInfo;
                    return 0;
                }

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
            return -1;
        }


        /// <summary>
        /// 根据code获取微信id
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="weiXinId"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int GetWeiXinId(string code, string state, out string weiXinId,
            out string strError)
        {
            strError = "";
            weiXinId = "";

            try
            {
                //可以传一个state用于校验
                if (state != "dp2weixin")
                {
                    strError = "验证失败！请从正规途径进入！";
                    return -1;
                }

                //用code换取access_token
                var result = OAuthApi.GetAccessToken(this.weiXinAppId, this.weiXinSecret, code);
                if (result.errcode != ReturnCode.请求成功)
                {
                    strError = "获取微信id出错：" + result.errmsg;
                    return -1;
                }

                //下面2个数据也可以自己封装成一个类，储存在数据库中（建议结合缓存）
                //如果可以确保安全，可以将access_token存入用户的cookie中，每一个人的access_token是不一样的
                //Session["OAuthAccessTokenStartTime"] = DateTime.Now;
                //Session["OAuthAccessToken"] = result;            

                // 取出微信id
                weiXinId = result.openid;
                return 0;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;

            }
        }

        /// <summary>
        /// 发送客服消息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="text"></param>
        public void SendCustomerMsg(string openId, string text)
        {
            var accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);
            CustomApi.SendText(accessToken, openId, "error");
        }



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

        public int Renew1(string libId,
            //string patron,
            string item,
            out string strError)
        {
            strError = "";

            LibItem lib = LibDatabase.Current.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                return -1;
            }

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            CirculationRequest request = new CirculationRequest(id,
                "renew",
                "",
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

        public int Reservation(string libId,
            string patron,
            string items,
            string style,
            out string strError)
        {
            strError = "";

            LibItem lib = LibDatabase.Current.GetLibById(libId);
            if (lib == null)
            {
                strError = "未找到id为[" + libId + "]的图书馆定义。";
                return -1;
            }

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            CirculationRequest request = new CirculationRequest(id,
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

        public void WriteErrorLog(string strText)
        {
            this.WriteLog("ERROR:" + strText);
        }

        public void WriteLog(string strText)
        {
            // todo 有空比对下谢老师写日志的代码
            //DateTime now = DateTime.Now;
            //// 每天一个日志文件
            //string strFilename = Path.Combine(this.LogDir, "log_" + DateTimeUtil.DateTimeToString8(now) + ".txt");
            //string strTime = now.ToString();
            //FileUtil.WriteText(strFilename,
            //    strTime + " " + strText + "\r\n");

            var logDir = this.weiXinLogDir;
            string strFilename = string.Format(logDir + "/log_{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
            FileUtil.WriteLog(strFilename, strText, "dp2weixin");
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

        public MemoryStream GetQrImage(
    string strCode,
    int nWidth,
    int nHeight,
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
                    MemoryStream ms = new MemoryStream();
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return ms;
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return null;
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



        #endregion




        #region 公告





        public int GetMessage(string weixinId,
            string msgType,
            string libId,            
             string msgId,
            string style,
            out List<MessageItem> list,
            out string worker,
            out string strError)
        {
            list = new List<MessageItem>();
            strError = "";
            worker = "";

            if (msgType != dp2WeiXinService.C_MsgType_Bb
                && msgType != dp2WeiXinService.C_MsgType_Book)
            {
                strError = "不支持的消息类型" + msgType;
                return -1;
            }

            if (string.IsNullOrEmpty(weixinId) == false)
            {
                // 查找当前微信用户绑定的工作人员账号
                WxUserItem user = WxUserDatabase.Current.GetWorker(weixinId, libId);
                if (user != null)
                {
                    // 检索是否有权限 _wx_setbbj
                    string needRight = "";
                    if (msgType == C_MsgType_Bb)
                        needRight = C_Right_SetBb;
                    else if (msgType == C_MsgType_Book)
                        needRight = C_Right_SetBook;

                    LibItem lib = LibDatabase.Current.GetLibById(libId);
                    if (lib == null)
                    {
                        strError = "未找到id为[" + libId + "]的图书馆定义。";
                        return -1;
                    }

                    int nHasRights = this.CheckRights(lib.capoUserName, user.userName, needRight, out strError);
                    if (nHasRights == -1)
                        return -1;

                    if (nHasRights == 1)
                    {
                        worker = user.userName;
                    }
                    else
                    {
                        worker = "";
                    }
                }
            }

            string group="";
            if (msgType == C_MsgType_Bb)
                group = C_GroupName_Bb;
            else if (msgType == C_MsgType_Book)
                group = C_GroupName_Book;

            List<MessageRecord>  records = new List<MessageRecord>();
            int nRet = this.GetMessage(group, libId,msgId, out records, out strError);
            if (nRet == -1)
                return -1;
            
            foreach (MessageRecord record in records)
            {
                MessageItem item = new MessageItem();
                item.id = record.id;
                item.publishTime = DateTimeUtil.DateTimeToString( record.publishTime);

                string title = "";
                string content = "";
                string format = "text"; //默认是text样式
                string creator = "";

                string xml = record.data;
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(xml);
                    XmlNode root = dom.DocumentElement;
                    XmlNode nodeTitle = root.SelectSingleNode("title");
                    title = nodeTitle.InnerText;//DomUtil.GetNodeText(root.SelectSingleNode("title"));
                    XmlNode nodeContent = root.SelectSingleNode("content");
                    content = nodeContent.InnerText;//DomUtil.GetNodeText();

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

                if (style == "full")
                {
                    item.content = content;
                }
                else if (style == "browse")
                {
                    item.content = "";

                    string contentHtml = "";
                    if (msgType == C_MsgType_Bb)
                    {
                        contentHtml = GetBbHtml(format, content);
                    }
                    else
                    {
                        contentHtml = GetBookHtml(content);

                    }
                    item.contentHtml = contentHtml;
                }

                list.Add(item);
            }

            return nRet;
        }

        public string GetBbHtml(string format,string content)
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
                string[] list=content.Split(new char[] { '\n' });
                foreach (string str in list)
                {
                    if (contentHtml != "")
                        contentHtml += "<br/>";

                    contentHtml += HttpUtility.HtmlEncode(str);
                }
            }
            return contentHtml;
        }


        public string GetBookHtml(string content)
        {
            string contentHtml = "";

            content = content.Replace("\r\n", "\n");
            content = content.Replace("\r", "\n");
            string[] list = content.Split(new char[] { '\n' });
            foreach (string str in list)
            {
                if (contentHtml != "")
                    contentHtml += "<br/>";

                string detalUrl = "/Biblio/Detail?biblioPath=" + HttpUtility.UrlEncode(str);
                contentHtml += "<a href='javascript:void(0)' onclick='gotoUrl(\"" + detalUrl + "\")'>" + HttpUtility.HtmlEncode(str) + "</a>";

            }

            return contentHtml;
        }

        // 从 dp2mserver 获得消息
        // 每次最多获得 100 条
        private int GetMessage(string groupName,
            string libId,
            string msgId,
            out List<MessageRecord> records,
            out string strError)
        {
            strError = "";
            records = new List<MessageRecord>();

            // string connName
            string connName = C_ConnPrefix_Myself + libId;

            // 取出用户名
            LibItem lib = LibDatabase.Current.GetLibById(libId);
            string wxUserName = lib.wxUserName;


            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            GetMessageRequest request = new GetMessageRequest(id,
                "",
                groupName,
                wxUserName,
                "", // strTimeRange,
                "publishTime|desc",//sortCondition 按发布时间倒序排
                msgId, //IdCondition 
                0,
                100);  // todo 如果超过100条，要做成递归来调
            try
            {
                MessageConnection connection = this.Channels.GetConnectionTaskAsync(this.dp2MServerUrl,
                    connName).Result;
                GetMessageResult result = connection.GetMessage(request,
                    new TimeSpan(0, 1, 0),
                    cancel_token);
                if (result.Value == -1)
                    goto ERROR1;
                
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
            strError = "服务器异常: " + strError;
            return -1;
        }

        /// <summary>
        /// 处理公告
        /// </summary>
        /// <param name="libUserName"></param>
        /// <param name="style"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public MessageResult CoverMessage(string msgType, 
            string libId, 
            MessageItem item, 
            string style)
        {
            string strError = "";
            MessageResult apiResult = new MessageResult();

            if (msgType != dp2WeiXinService.C_MsgType_Bb
                && msgType != dp2WeiXinService.C_MsgType_Book)
            {
                strError = "不支持的消息类型" + msgType;
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(item.creator) == true)
            {
                strError = "未传入creator";
                goto ERROR1;
            }

            // 检索工作人员是否有权限 _wx_setbb
            string needRight = "";
            if (msgType == C_MsgType_Bb)
                needRight = C_Right_SetBb;
            else if (msgType == C_MsgType_Book)
                needRight = C_Right_SetBook;
            LibItem libItem = LibDatabase.Current.GetLibById(libId);
            if (libItem == null)
            {
                strError = "根据id["+libId+"]未找到对应的图书馆配置";
                goto ERROR1;                
            }
            int nHasRights = this.CheckRights(libItem.capoUserName, item.creator, needRight, out strError);
            if (nHasRights == -1)
            {
                goto ERROR1;
            }
            if (nHasRights == 0)
            {
                strError = "帐户["+userName+"]没有"+needRight+"权限";
                goto ERROR1;
            }


            string connName = C_ConnPrefix_Myself + libId;
            string strText = "";
            if (style != "delete")
            {
                strText = "<body>"
                + "<title>" + HttpUtility.HtmlEncode(item.title) + "</title>"  //前端传过来时，已经转义过了 HttpUtility.HtmlEncode(item.title)
                + "<content format='"+item.contentFormat+"'>" + HttpUtility.HtmlEncode(item.content) + "</content>"
                + "<creator>"+item.creator+"</creator>"
                + "</body>";
            }

            string group = "";
            if (msgType == C_MsgType_Bb)
                group = C_GroupName_Bb;
            else if (msgType == C_MsgType_Book)
                group = C_GroupName_Book;

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

            // todo subject

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

                //result.
                apiResult.errorCode = result.Value;
                return apiResult;
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
            apiResult.errorCode = -1;
            apiResult.errorInfo= strError;
            return apiResult;
        }


        /// <returns>
        /// -1：出错
        /// 0   无权限
        /// 1   有权限
        /// </returns>
        public int CheckRights(string opacUserName,string worker,string needRight, out string strError)
        {
            strError ="";
            string rights = "";
            int nRet = this.GetUserInfo(opacUserName,
                worker,
                out rights,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                return -1;
            }

            if (rights.Contains(needRight) == true)
                return 1;

            return 0;

        }

        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <param name="opacUserName"></param>
        /// <param name="strWord"></param>
        /// <param name="right"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int GetUserInfo(string opacUserName, string strWord,
            out string rights,
            out string strError)
        {
            strError = "";
            rights = "";            

            //long start = 0;
            //long count = 10;
            try
            {

                CancellationToken cancel_token = new CancellationToken();
                string id = Guid.NewGuid().ToString();
                SearchRequest request = new SearchRequest(id,
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
                    opacUserName).Result;

                SearchResult result = connection.SearchTaskAsync(
                    opacUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                if (result.ResultCount == -1)
                {
                    strError = "检索出错：" + result.ErrorInfo;
                    return -1;
                }
                if (result.ResultCount == 0)
                {
                    strError = "未命中";
                    return 0;
                }

                string strXml = result.Records[0].Data;
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(strXml);
                rights = DomUtil.GetAttr(dom.DocumentElement, "rights");


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

        #endregion

        #region  好书


        /// <summary>
        /// 获取好多推荐栏目
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libId"></param>
        /// <param name="userName">
        /// 返回的当前绑定工作人员账户号，如果为空，不没有编辑权限
        /// </param>
        /// <returns></returns>
        public int GetBookSubject(string libId,
            out List<BookSubjectItem> list,
            out string strError)
        {
            //userName = "";
            strError = "";
            list = new List<BookSubjectItem>();

            


            BookSubjectItem item = new BookSubjectItem();
            item.name = "我喜欢的书";
            item.count = 1;
            list.Add(item);

            item = new BookSubjectItem();
            item.name = "6月新书";
            item.count = 5;
            list.Add(item);

            // todo
            // 用点对点api获取好书推荐栏目

            return 0;
        }

        #endregion

        public int GetBookMsg(string libId, string subject, out List<MessageItem> list, out string strError)
        {
            list = new List<MessageItem>();
            strError = "";

            //todo 根据subject取消息

            return 0;
        }
    }
}
