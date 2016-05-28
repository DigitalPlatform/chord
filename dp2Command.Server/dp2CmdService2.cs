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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace dp2Command.Service
{
    public class dp2CmdService2 : dp2BaseCommandService
    {
        public static string EncryptKey = "dp2weixinPassword";

        #region 模板消息

        //微信绑定通知
        public const string C_Template_Bind = "hFmNH7on2FqSOAiYPZVJN-FcXBv4xpVLBvHsfpLLQKU";
        // 微信解绑通知 overdues
        public const string C_Template_UnBind = "1riAKkt2W0AOtkx5rx-Lwa0RKRydDTHaMjSoUBGuHog";
        //预约到书通知 
        public const string C_Template_Arrived = "Wm-7-0HJay4yloWEgGG9HXq9eOF5cL8Qm2aAUy-isoM";
        //图书超期提醒 
        public const string C_Template_CaoQi = "QcS3LoLHk37Jh0rgKJId2o93IZjulr5XxgshzlW5VkY";
        //图书到期提醒
        public const string C_Template_DaoQi = "Q6O3UFPxPnq0rSz82r9P9be41tqEPaJVPD3U0PU8XOU";

        //借阅成功通知
        public const string C_Template_Borrow = "_F9kVyDWhunqM5ijvcwm6HwzVCnwbkeZl6GV6awB_fc";
        //图书归还通知 
        public const string C_Template_Return = "86Ee0NevuLIVGZE4Xu0uzDdmg0T3xnRMOJ5tREIEG_w";
        //缴费成功通知
        public const string C_Template_Pay = "4HNhEfLcroEMdX0Pr6aFo_n7_aHuvAzD8_6lzABHkiM";
        //退款通知
        public const string C_Template_ReturnPay = "sIzSJJ-VRbFUFrDHszxCqwiIYjr9IyyqEqLr95iJVTs";
        //个人消息通知 
        public const string C_Template_Message = "rtAx0BoUAwZ3npbNIO8Y9eIbdWO-weLGE2iOacGqN_s";

        #endregion

        MessageConnectionCollection _channels = new MessageConnectionCollection();
         public MessageConnectionCollection Channels
         {
             get
             {
                 return this._channels;
             }
         }

        // 配置文件
        public string _cfgFile = "";

        // dp2服务器地址与代理账号
        public string dp2MServerUrl = "";
        public string userName = "";
        public string password = "";

        // 微信信息
        public string weiXinAppId { get; set; }
        public string weiXinSecret { get; set; }

        // 背景图管理器
        public string TodayUrl = "";

        // dp2消息处理类
        MsgRouter _msgRouter = new MsgRouter();

        //=================
        // 设为单一实例
        static dp2CmdService2 _instance;
        private dp2CmdService2()
        {
        }
        private static object _lock = new object();
        static public dp2CmdService2 Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (_lock)  //线程安全的
                    {
                        _instance = new dp2CmdService2();
                    }
                }
                return _instance;
            }
        }
        //===========

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
                this.password = Cryptography.Decrypt(this.password, dp2CmdService2.EncryptKey);

            // 取出微信配置信息
            XmlNode nodeDp2weixin = root.SelectSingleNode("dp2weixin");
            this.weiXinUrl = DomUtil.GetAttr(nodeDp2weixin, "url"); //WebConfigurationManager.AppSettings["weiXinUrl"];
            this.weiXinAppId = DomUtil.GetAttr(nodeDp2weixin, "AppId"); //WebConfigurationManager.AppSettings["weiXinAppId"];
            this.weiXinSecret = DomUtil.GetAttr(nodeDp2weixin, "Secret"); //WebConfigurationManager.AppSettings["weiXinSecret"];

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

            // 消息处理类
            this._msgRouter.SendMessageEvent -= _msgRouter_SendMessageEvent;
            this._msgRouter.SendMessageEvent += _msgRouter_SendMessageEvent;
            this._msgRouter.Start(this._channels,
                this.dp2MServerUrl,
                "_patronNotify");

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
            }

            this.WriteInfoLog("走到close()");
        }

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
                        dp2CmdService2.C_Template_Message,
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
                    first = new TemplateDataItem("☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆", "#B8860B"),  // 	dark golden rod//this._msgFirstLeft + "撤消交费成功！"
                    reason = new TemplateDataItem("撤消[" + barcodes + "]交费。", "#000000"),//text.ToString()),// "请让我慢慢长大"),
                    refund = new TemplateDataItem("CNY" + totalPrice, "#000000"),
                    remark = new TemplateDataItem(this._msgRemark, "#CCCCCC")
                };

                // 发送模板消息
                var result1 = TemplateApi.SendTemplateMessage(accessToken,
                    weiXinId,
                    dp2CmdService2.C_Template_ReturnPay,
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
                        first = new TemplateDataItem("★★★★★★★★★★★★★★★", "#556B2F"),//dark olive green//this._msgFirstLeft+"您已交费成功！"
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
                        dp2CmdService2.C_Template_Pay,
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
            string remark = "\n欢迎继续借书。";
            XmlNodeList listOverdue = root.SelectNodes("patronRecord/overdues/overdue");
            if (listOverdue.Count > 0)
            {
                remark = "\n您有" + listOverdue.Count + "笔超期违约记录，请履行超期手续。";
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
                        first = new TemplateDataItem("┅┅┅┅┅┅┅┅┅┅┅┅", "#00008B"),  // 	dark blue//this._msgFirstLeft + "您借出的图书已确认归还。"
                        keyword1 = new TemplateDataItem(summary, "#000000"),//text.ToString()),// "请让我慢慢长大"),
                        keyword2 = new TemplateDataItem(operTime, "#000000"),
                        keyword3 = new TemplateDataItem(patronName, "#000000"),
                        remark = new TemplateDataItem(remark, "#CCCCCC")
                    };

                    // 发送模板消息
                    var result1 = TemplateApi.SendTemplateMessage(accessToken,
                        weiXinId,
                        dp2CmdService2.C_Template_Return,
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
                        remark = new TemplateDataItem("\n祝您阅读愉快，欢迎再借。", "#CCCCCC")
                    };

                    // 发送模板消息
                    var result1 = TemplateApi.SendTemplateMessage(accessToken,
                        weiXinId,
                        dp2CmdService2.C_Template_Borrow,
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

            string first = this._msgFirstLeft+"我们很高兴地通知您，您预约的下列图书到了，请尽快来图书馆办理借书手续。";
            string end = "\n如果您未能在保留期限内来馆办理借阅手续，图书馆将把优先借阅权转给后面排队等待的预约者，或做归架处理。";
            if (bOnShelf == true)
            {
                first = this._msgFirstLeft + "我们很高兴地通知您，您预约的图书已经在架上，请尽快来图书馆办理借书手续。";
                end = "\n如果您未能在保留期限内来馆办理借阅手续，图书馆将把优先借阅权转给后面排队等待的预约者，或允许其他读者借阅。";
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
                        first = new TemplateDataItem("▇▆▅▇▆▅▇▆▅▇▆▅▇▆▅", "#FF8C00"),//  dark orange   	yellow 	#FFFF00
                        keyword1 = new TemplateDataItem(summary, "#000000"),//text.ToString()),// "请让我慢慢长大"),
                        keyword2 = new TemplateDataItem(today, "#000000"),
                        keyword3 = new TemplateDataItem("保留" + reserveTime, "#000000"),
                        remark = new TemplateDataItem(end, "#CCCCCC")
                    };

                    // 发送预约模板消息
                    //string detailUrl = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx57aa3682c59d16c2&redirect_uri=http%3a%2f%2fdp2003.com%2fdp2weixin%2fPatron%2fIndex&response_type=code&scope=snsapi_base&state=dp2weixin#wechat_redirect";
                    var result1 = TemplateApi.SendTemplateMessage(accessToken,
                        weiXinId,
                        dp2CmdService2.C_Template_Arrived,
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
                string first = "";
                string end = "";
                if (overdueType == "overdue")
                {
                    templateId = dp2CmdService2.C_Template_CaoQi;
                    first = this._msgFirstLeft+"您借出的图书已超期，请尽快归还。";
                    end = "\n您借出的图书已超期，请尽快归还。";
                }
                else if (overdueType == "warning")
                {
                    templateId = dp2CmdService2.C_Template_DaoQi;
                    first = this._msgFirstLeft+"您借出的图书即将到期，请注意不要超期，留意归还。";
                    end = "\n您借出的图书即将到期，请注意不要超期，留意归还。";
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
                if (oneEmail.Length > 9 && oneEmail.Substring(0, 9) == dp2CommandUtility.C_WeiXinIdPrefix)
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

        #region 本方账号登录
        void _channels_Login(object sender, LoginEventArgs e)
        {
            MessageConnection connection = sender as MessageConnection;

            e.UserName = GetUserName();
            if (string.IsNullOrEmpty(e.UserName) == true)
                throw new Exception("尚未指定用户名，无法进行登录");

            e.Password = GetPassword();
            e.Parameters = "";

            // TODO: 登录如果失败，界面会有提示么?
        }

        string GetUserName()
        {
            return this.userName;
        }
        string GetPassword()
        {
            return this.password;
        }

        #endregion



        #region 绑定解绑

        public List<WxUserItem> GetBindInfo(string weixinId)
        {
            List<WxUserItem> list = new List<WxUserItem>();

            // 目前只支持从数据库中查找
            list = WxUserDatabase.Current.GetByWeixinId(weixinId);


            return list;
        }

        public int ResetPassword(string remoteUserName,
            string strLibraryCode,
            string name,
            string tel,
            out string strError)
        {
            strError = "";
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
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    "").Result;
                CirculationResult result = connection.CirculationAsync(
                    remoteUserName,
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
                        strLibraryCode,
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
        /// 
        /// </summary>
        /// <param name="strBarcode"></param>
        /// <param name="strPassword"></param>
        /// <param name="weiXinId"></param>
        /// <returns>
        /// -1 出错
        /// 0 成功
        /// </returns>
        public override int Bind(string remoteUserName,
            string libCode,
            string strFullWord,
            string strPassword,
            string strWeiXinId,
            out WxUserItem userItem,
            out string strReaderBarcode,
            out string strError)
        {
            userItem = null;
            strError = "";
            strReaderBarcode = "";
            long lRet = -1;


            CancellationToken cancel_token = new CancellationToken();

            string fullWeixinId = dp2CommandUtility.C_WeiXinIdPrefix + strWeiXinId;
            string id = Guid.NewGuid().ToString();
            BindPatronRequest request = new BindPatronRequest(id,
                "bind",
                strFullWord,
                strPassword,
                fullWeixinId,
               "multiple",//single
                "xml");

            try
            {
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    remoteUserName).Result;


                BindPatronResult result = connection.BindPatronAsync(
                     remoteUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;

                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }

                string xml = result.Results[0];
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(xml);

                // 绑定成功，把读者证条码记下来，用于续借 2015/11/7，不要用strbarcode变量，因为可能做的大小写转换
                strReaderBarcode = DomUtil.GetNodeText(dom.DocumentElement.SelectSingleNode("barcode"));

                // 将关系存到mongodb库
                string name = "";
                XmlNode node = dom.DocumentElement.SelectSingleNode("name");
                if (node != null)
                    name = DomUtil.GetNodeText(node);
                string refID = "";
                node = dom.DocumentElement.SelectSingleNode("refID");
                if (node != null)
                    refID = DomUtil.GetNodeText(node);

                // 找到库中对应的记录
                userItem = WxUserDatabase.Current.GetOneOrEmptyPatron(strWeiXinId, libCode, strReaderBarcode);
                if (userItem == null)
                {
                    LibItem lib = LibDatabase.Current.GetLibByLibCode(libCode);


                    userItem = new WxUserItem();
                    userItem.weixinId = strWeiXinId;
                    userItem.libCode = libCode;
                    userItem.libUserName = remoteUserName;
                    userItem.libName = lib.libName;
                    userItem.readerBarcode = strReaderBarcode;
                    userItem.readerName = name;
                    userItem.xml = xml;
                    userItem.refID = refID;
                    userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                    userItem.updateTime = userItem.createTime;
                    userItem.isActive = 1;
                    userItem.fullWord = strFullWord;
                    userItem.password = strPassword;
                    WxUserDatabase.Current.Add(userItem);
                }
                else
                {
                    userItem.readerBarcode = strReaderBarcode;
                    userItem.readerName = name;
                    userItem.xml = xml;
                    userItem.refID = refID;
                    userItem.updateTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                    userItem.isActive = 1;
                    userItem.fullWord = strFullWord;
                    userItem.password = strPassword;
                    lRet = WxUserDatabase.Current.Update(userItem);
                }
                // 置为活动状态
                WxUserDatabase.Current.SetActive(userItem);

                // 发送绑定成功的客服消息                
                string accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);
                var testData = new BindTemplateData()
                {
                    first = new TemplateDataItem("恭喜您！您已成功绑定图书馆账号。", "#000000"),
                    keyword1 = new TemplateDataItem(userItem.readerName + "(" + userItem.readerBarcode + ")", "#000000"),
                    keyword2 = new TemplateDataItem("图书馆[" + userItem.libName + "]", "#000000"),
                    remark = new TemplateDataItem("您可以直接通过微信公众号访问图书馆，进行信息查询，预约续借等功能。如需解绑，请在“绑定账号”菜单操作。", "#CCCCCC")
                };

                // 详细转到账户管理界面
                string detailUrl = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx57aa3682c59d16c2&redirect_uri=http%3a%2f%2fdp2003.com%2fdp2weixin%2fAccount%2fIndex&response_type=code&scope=snsapi_base&state=dp2weixin#wechat_redirect";
                var result1 = TemplateApi.SendTemplateMessage(accessToken,
                    strWeiXinId,
                    dp2CmdService2.C_Template_Bind,
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
        public override int Unbind(string userId,
             out string strError)
        {
            strError = "";

            //string remoteUserName,
            //    string libCode,
            //    string strBarcode,
            //    string strWeiXinId,

            WxUserItem userItem = WxUserDatabase.Current.GetById(userId);
            if (userItem == null)
            {
                strError = "绑定账号未找到";
                return -1;
            }


            // 调点对点解绑接口
            string fullWeixinId = dp2CommandUtility.C_WeiXinIdPrefix + userItem.weixinId;
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
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    userItem.libUserName).Result;

                BindPatronResult result = connection.BindPatronAsync(
                     userItem.libUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                if (result.Value == -1)
                {
                    //strError = result.ErrorInfo;
                    //return -1;
                }

                // 删除mongodb库的记录
                WxUserDatabase.Current.Delete(userId);

                // 发送解绑消息            
                string accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);
                var data = new UnBindTemplateData()
                {
                    first = new TemplateDataItem("您已成功对图书馆账号解除绑定。", "#000000"),
                    keyword1 = new TemplateDataItem(userItem.readerName + "(" + userItem.readerBarcode + ")", "#000000"),
                    keyword2 = new TemplateDataItem("图书馆[" + userItem.libName + "]", "#000000"),
                    remark = new TemplateDataItem("\n您现在不能访问该图书馆信息了，如需访问，请重新绑定。", "#CCCCCC")
                };
                SendTemplateMessageResult result1 = TemplateApi.SendTemplateMessage(accessToken,
                    userItem.weixinId,
                    dp2CmdService2.C_Template_UnBind,
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

        public override long SearchOnePatronByWeiXinId(string remoteUserName,
            string libCode,
            string strWeiXinId,
            out string strBarcode,
            out string strError)
        {
            strError = "";
            strBarcode = "";

            // 从mongodb中检查是否绑定了用户
            WxUserItem userItem = WxUserDatabase.Current.GetActiveOrFirst(strWeiXinId, libCode);
            if (userItem == null)
            {
                strError = "异常的情况，未怎么图书馆时不应走到SearchPatronByWeiXinId函数。";
                return -1;
            }

            // mongodb存在
            if (String.IsNullOrEmpty(userItem.readerBarcode) == false)
            {
                strBarcode = userItem.readerBarcode;
                return 1;
            }

            // 从远程dp2library中查
            string strWord = dp2CommandUtility.C_WeiXinIdPrefix + strWeiXinId;
            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
                "searchPatron",
                "<全部>",
                strWord,
                "email",
                "left",
                "wx-patron",
                "id,cols",
                1000,
                0,
                C_Search_MaxCount);
            try
            {
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    remoteUserName).Result;

                SearchResult result = connection.SearchAsync(
                    remoteUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                if (result.ResultCount == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }
                if (result.ResultCount == 0)
                    return 0;

                // 找到对应的读者记录
                if (result.ResultCount > 0)
                {
                    LibItem libItem = LibDatabase.Current.GetLibByLibCode(libCode);
                    string libName = libItem.libName;
                    for (int i = 0; i < result.ResultCount; i++)
                    {
                        // 可能会检索出多笔记录，先取第一笔 todo
                        string strXml = result.Records[i].Data;
                        XmlDocument dom = new XmlDocument();
                        dom.LoadXml(strXml);
                        string strTempBarcode = DomUtil.GetNodeText(dom.DocumentElement.SelectSingleNode("barcode"));

                        // 更新到mongodb库
                        string name = "";
                        XmlNode node = dom.DocumentElement.SelectSingleNode("name");
                        if (node != null)
                            name = DomUtil.GetNodeText(node);
                        string refID = "";
                        node = dom.DocumentElement.SelectSingleNode("refID");
                        if (node != null)
                            refID = DomUtil.GetNodeText(node);

                        if (i == 0)
                        {
                            userItem.readerBarcode = strTempBarcode;
                            userItem.readerName = name;
                            userItem.xml = strXml;
                            userItem.refID = refID;
                            userItem.updateTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                            WxUserDatabase.Current.Update(userItem);
                            //将第一笔设为活动状态
                            WxUserDatabase.Current.SetActive(userItem);
                            //返回的strBarcode //todo refID
                            strBarcode = strTempBarcode;
                        }
                        else
                        {
                            userItem = new WxUserItem();
                            userItem.weixinId = strWeiXinId;
                            userItem.libCode = libCode;
                            userItem.libUserName = remoteUserName;
                            userItem.libName = libName;
                            userItem.readerBarcode = strTempBarcode;
                            userItem.readerName = name;
                            userItem.xml = strXml;
                            userItem.refID = refID;
                            userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                            userItem.updateTime = userItem.createTime;
                            WxUserDatabase.Current.Add(userItem);
                        }
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


        #endregion

        #region 检索书目

        /// <summary>
        /// 检索书目，word如果传_N表示取下一页
        /// </summary>
        /// <param name="remoteUserName"></param>
        /// <param name="strFrom"></param>
        /// <param name="strWord"></param>
        /// <returns></returns>
        public long SearchBiblio(string remoteUserName,
            string strFrom,
            string strWord,
            out List<BiblioRecord> records,
            out string strError)
        {
            strError = "";
            records = new List<BiblioRecord>();


            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
                "searchBiblio",
                "",
                strWord,
                strFrom,
                "middle",
                "weixin",
                "id,cols",
                C_Search_MaxCount,  //todo这个值一般多少
                0,
                -1); //todo 这个值设多少
            try
            {
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    remoteUserName).Result;

                SearchResult result = connection.SearchAsync(
                    remoteUserName,
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
                for (int i = 0; i < result.ResultCount; i++)
                {
                    string xml = result.Records[i].Data;
                    /*<root><col>请让我慢慢长大</col><col>吴蓓著</col><col>天津教育出版社</col><col>2009</col><col>G61-53</col><col>儿童教育儿童教育</col><col></col><col>978-7-5309-5335-8</col></root>*/
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    string name = DomUtil.GetNodeText(dom.DocumentElement.SelectSingleNode("col"));
                    string path = result.Records[i].RecPath;

                    // todo，改为分批返回
                    BiblioRecord record = new BiblioRecord();
                    record.recPath = path;
                    record.name = name;
                    record.no = (i + 1).ToString();//todo 注意下一页的时候
                    records.Add(record);
                }

                return result.ResultCount;
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

        public override long SearchBiblio(string remoteUserName,
            string strWord,
            SearchCommand searchCmd,
            out string strFirstPage,
            out string strError)
        {
            strFirstPage = "";
            strError = "";

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
                "searchBiblio",
                "<全部>",
                strWord,
                "title",
                "middle",
                "test",
                "id,cols",
                C_Search_MaxCount,
                0,
                -1);
            try
            {
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    remoteUserName).Result;

                SearchResult result = connection.SearchAsync(
                    remoteUserName,
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
                for (int i = 0; i < result.ResultCount; i++)
                {
                    string xml = result.Records[i].Data;
                    /*<root><col>请让我慢慢长大</col><col>吴蓓著</col><col>天津教育出版社</col><col>2009</col><col>G61-53</col><col>儿童教育儿童教育</col><col></col><col>978-7-5309-5335-8</col></root>*/
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    string path = result.Records[i].RecPath;
                    int index = path.IndexOf("@");
                    if (index >= 0)
                        path = path.Substring(0, index);

                    string name = DomUtil.GetNodeText(dom.DocumentElement.SelectSingleNode("col"));
                    resultPathList.Add(path + "*" + name);
                }

                // 将检索结果信息保存到检索命令中
                searchCmd.BiblioResultPathList = resultPathList;
                searchCmd.ResultNextStart = 0;
                searchCmd.IsCanNextPage = true;

                // 获得第一页检索结果
                bool bRet = searchCmd.GetNextPage(out strFirstPage, out strError);
                if (bRet == false)
                {
                    return -1;
                }

                return result.ResultCount;
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



        public override int GetDetailBiblioInfo(string remoteUserName,
            SearchCommand searchCmd,
            int nIndex,
            out string strBiblioInfo,
            out string strError)
        {
            strBiblioInfo = "";
            strError = "未实现";
            Debug.Assert(searchCmd != null);

            // 开始时间
            DateTime start_time = DateTime.Now;

            //检查有无超过数组界面
            if (nIndex <= 0 || searchCmd.BiblioResultPathList.Count < nIndex)
            {
                strError = "您输入的书目序号[" + nIndex.ToString() + "]越出范围。";
                return -1;
            }

            // 获取路径，注意要截取
            string strPath = searchCmd.BiblioResultPathList[nIndex - 1];
            string strName = "";
            int index = strPath.IndexOf("*");
            if (index > 0)
            {
                strName = strPath.Substring(index + 1);
                strPath = strPath.Substring(0, index);

            }
            //strBiblioInfo += strName + "\n";

            int nRet = 0;


            //2个任务并行
            string strInfo = "";
            nRet = this.GetBiblioAndSub(remoteUserName,
                strPath,  //GetBiblioAndSub
                out strInfo,
                out strError);
            if (nRet == -1 || nRet == 0)
                return nRet;
            strBiblioInfo += strInfo + "\n";


            /*
            try
            {
                // 取出summary
                string strSummary = "";
                nRet = this.GetBiblioSummary(strPath, out strSummary, out strError);
                if (nRet == -1 || nRet == 0)
                    return nRet;
                strBiblioInfo += strSummary + "\n";


                // 取item
                string strItemInfo = "";
                nRet = (int)this.GetItemInfo(strPath, out strItemInfo, out strError);
                if (nRet == -1 || nRet == 0)
                    return nRet;
                if (strItemInfo != "")
                {
                    strBiblioInfo += "===========\n";
                    strBiblioInfo += strItemInfo;
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
            */
            // 计算用了多少时间
            TimeSpan time_length = DateTime.Now - start_time;
            strBiblioInfo = "time span: " + time_length.TotalSeconds.ToString() + " secs" + "\n"
                + strBiblioInfo;



            return 1;
        }

        private int GetBiblioSummary(string remoteUserName,
            string biblioPath,
            out string summary,
            out string strError)
        {
            summary = "";
            strError = "";

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
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
            try
            {
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    remoteUserName).Result;

                SearchResult result = connection.SearchAsync(
                    remoteUserName,
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


        private long GetItemInfo(string remoteUserName,
            string biblioPath,
            out string itemInfo,
            out string strError)
        {
            itemInfo = "";
            strError = "";

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
                "getItemInfo",
                "entity",
                biblioPath,
                "",
                "",
                "",
                "opac",
                1,
                0,
                -1);
            try
            {
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    remoteUserName).Result;

                SearchResult result = connection.SearchAsync(
                    remoteUserName,
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


                for (int i = 0; i < result.ResultCount; i++)
                {
                    if (itemInfo != "")
                        itemInfo += "===========\n";

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
                        strViewBarcode = "refID:" + strRefID;  //"@refID:"
                    //状态
                    string strState = DomUtil.GetElementText(dom.DocumentElement, "state");
                    // 馆藏地
                    string strLocation = DomUtil.GetElementText(dom.DocumentElement, "location");
                    // 索引号
                    string strAccessNo = DomUtil.GetElementText(dom.DocumentElement, "accessNo");

                    // 出版日期
                    string strPublishTime = DomUtil.GetElementText(dom.DocumentElement, "publishTime");
                    // 价格
                    string strPrice = DomUtil.GetElementText(dom.DocumentElement, "price");
                    // 注释
                    string strComment = DomUtil.GetElementText(dom.DocumentElement, "comment");

                    // 借阅情况
                    string strBorrowInfo = "借阅情况:在架";
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
                    if (string.IsNullOrEmpty(strBorrower) == false)
                        strBorrowInfo = "借阅者:*** 借阅时间:" + borrowDate + " 借期:" + borrowPeriod;

                    itemInfo += "序号:" + (i + 1).ToString() + "\n"
                        + "册条码号:" + strViewBarcode + "\n"
                        + "状态:" + strState + "\n"
                        + "馆藏地:" + strLocation + "\n"
                        + "索引号:" + strAccessNo + "\n"
                        + "出版日期:" + strPublishTime + "\n"
                        + "价格:" + strPrice + "\n"
                        + "注释:" + strComment + "\n"
                        + strBorrowInfo + "\n";
                }


                return result.ResultCount;
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


        private int GetBiblioAndSub(string remoteUserName,
            string biblioPath,
            out string strInfo,
            out string strError)
        {
            strError = "";
            strInfo = "";


            DateTime start_time = DateTime.Now;

            CancellationToken cancel_token = new CancellationToken();

            // 获取书目记录
            string id1 = Guid.NewGuid().ToString();
            SearchRequest request1 = new SearchRequest(id1,
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
            // 获取下属记录
            string id2 = Guid.NewGuid().ToString();
            SearchRequest request2 = new SearchRequest(id2,
                "getItemInfo",
                "entity",
                biblioPath,
                "",
                "",
                "",
                "opac",
                1000,
                0,
                10);

            try
            {
                MessageConnection connection = this._channels.GetConnectionAsync(
this.dp2MServerUrl,
remoteUserName).Result;


                Task<SearchResult> task1 = connection.SearchAsync(
remoteUserName,
request1,
new TimeSpan(0, 1, 0),
cancel_token);
                Task<SearchResult> task2 = connection.SearchAsync(
                     remoteUserName,
                    request2,
                    new TimeSpan(0, 1, 0),
                    cancel_token);

                Task<SearchResult>[] tasks = new Task<SearchResult>[2];
                tasks[0] = task1;
                tasks[1] = task2;
                Task.WaitAll(tasks);


                if (task1.Result.ResultCount == -1)
                {
                    strError = "获取摘要出错：" + task1.Result.ErrorInfo;
                    return -1;
                }
                if (task1.Result.ResultCount == 0)
                {
                    strError = "未命中";
                    return 0;
                }

                strInfo = task1.Result.Records[0].Data;

                if (task2.Result.ResultCount == -1)
                {
                    strError = "获取册出错：" + task2.Result.ErrorInfo;
                    return -1;
                }
                if (task2.Result.ResultCount == 0)
                {
                    strError = "未命中";
                    return 0;
                }

                string itemInfo = "";
                for (int i = 0; i < task2.Result.Records.Count; i++)
                {
                    if (itemInfo != "")
                        itemInfo += "===========\n";

                    string xml = task2.Result.Records[i].Data;
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);

                    string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");
                    // 册条码号
                    string strViewBarcode = "";
                    if (string.IsNullOrEmpty(strBarcode) == false)
                        strViewBarcode = strBarcode;
                    else
                        strViewBarcode = "refID:" + strRefID;  //"@refID:"
                    //状态
                    string strState = DomUtil.GetElementText(dom.DocumentElement, "state");
                    // 馆藏地
                    string strLocation = DomUtil.GetElementText(dom.DocumentElement, "location");
                    // 索引号
                    string strAccessNo = DomUtil.GetElementText(dom.DocumentElement, "accessNo");

                    // 出版日期
                    string strPublishTime = DomUtil.GetElementText(dom.DocumentElement, "publishTime");
                    // 价格
                    string strPrice = DomUtil.GetElementText(dom.DocumentElement, "price");
                    // 注释
                    string strComment = DomUtil.GetElementText(dom.DocumentElement, "comment");

                    // 借阅情况
                    string strBorrowInfo = "借阅情况:在架";
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
                    if (string.IsNullOrEmpty(strBorrower) == false)
                        strBorrowInfo = "借阅者:*** 借阅时间:" + borrowDate + " 借期:" + borrowPeriod;

                    itemInfo += "序号:" + (i + 1).ToString() + "\n"
                        + "册条码号:" + strViewBarcode + "\n"
                        + "状态:" + strState + "\n"
                        + "馆藏地:" + strLocation + "\n"
                        + "索引号:" + strAccessNo + "\n"
                        + "出版日期:" + strPublishTime + "\n"
                        + "价格:" + strPrice + "\n"
                        + "注释:" + strComment + "\n"
                        + strBorrowInfo + "\n";
                }

                if (itemInfo != "")
                {
                    strInfo += "===========\n";
                    strInfo += itemInfo;
                }

                return (int)task2.Result.ResultCount;

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

        /// <returns>
        /// -1  出错
        /// 0   未查到对应记录
        /// 1  成功
        /// </returns>
        public override int GetMyInfo(string remoteUserName,
            string strReaderBarcode,
            out string strMyInfo,
            out string strError)
        {
            strError = "";
            strMyInfo = "";
            Debug.Assert(String.IsNullOrEmpty(strReaderBarcode) == false);


            // 得到高级xml
            string strXml = "";
            int nRet = this.GetPatronInfo(remoteUserName,
                strReaderBarcode,
                "xml",
                out strXml,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "从dp2library未找到证条码号为'" + strReaderBarcode + "'的记录"; //todo refID
                return 0;
            }

            // 取出个人信息
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);
            //string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            string strName = DomUtil.GetElementText(dom.DocumentElement, "name");
            string strDepartment = DomUtil.GetElementText(dom.DocumentElement, "department");
            string strState = DomUtil.GetElementText(dom.DocumentElement, "state");
            string strCreateDate = DateTimeUtil.ToLocalTime(DomUtil.GetElementText(dom.DocumentElement,
                "createDate"), "yyyy/MM/dd");
            string strExpireDate = DateTimeUtil.ToLocalTime(DomUtil.GetElementText(dom.DocumentElement,
                "expireDate"), "yyyy/MM/dd");
            string strReaderType = DomUtil.GetElementText(dom.DocumentElement,
                "readerType");
            string strComment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");

            strMyInfo = "个人信息" + "\n"
                + "姓名:" + strName + "\n"
                + "证条码号:" + strReaderBarcode + "\n"
                + "部门:" + strDepartment + "\n"
                + "联系方式:\n" + GetContactString(dom) + "\n"
                + "状态:" + strState + "\n"
                + "有效期:" + strCreateDate + "~" + strExpireDate + "\n"
                + "读者类别:" + strReaderType + "\n"
                + "注释:" + strComment;
            return 1;
        }

        /// <returns>
        /// -1  出错
        /// 0   未找到读者记录
        /// 1   成功
        /// </returns>
        public override int GetBorrowInfo(string remoteUserName,
            string strReaderBarcode,
            out string strBorrowInfo,
            out string strError)
        {
            strError = "";
            strBorrowInfo = "";


            // 得到高级xml
            string strXml = "";
            long lRet = this.GetPatronInfo(remoteUserName,
                strReaderBarcode,
                "advancexml,advancexml_borrow_bibliosummary",
                out strXml,
                out strError);
            if (lRet == -1)
                return -1;

            // 提取借书信息
            lRet = this.GetBorrowsInfoInternal(strXml, out strBorrowInfo);
            if (lRet == -1)
                return -1;


            return 1;

        }

        /// <summary>
        /// 获取读者信息
        /// </summary>
        /// <param name="remoteUserName"></param>
        /// <param name="strReaderBarocde"></param>
        /// <param name="strFormat"></param>
        /// <param name="xml"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int GetPatronInfo(string remoteUserName,
            string strReaderBarocde,  //todo refID
            string strFormat,
            out string xml,
            out string strError)
        {
            xml = "";
            strError = "";

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
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    remoteUserName).Result;

                SearchResult result = connection.SearchAsync(
                    remoteUserName,
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
                    return -1;
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
    }



}
