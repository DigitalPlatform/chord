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

        // 模板消息ID
        public const string C_Template_Bind = "hFmNH7on2FqSOAiYPZVJN-FcXBv4xpVLBvHsfpLLQKU";
        public const string C_Template_UnBind = "1riAKkt2W0AOtkx5rx-Lwa0RKRydDTHaMjSoUBGuHog";
        public const string C_Template_Arrived = "Wm-7-0HJay4yloWEgGG9HXq9eOF5cL8Qm2aAUy-isoM";
        public const string C_Template_CaoQi = "QcS3LoLHk37Jh0rgKJId2o93IZjulr5XxgshzlW5VkY";



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
        private dp2MsgHandler _dp2MsgHandler = null;

        //=================
        // 设为单一实例
        static dp2CmdService2 _instance;
        private dp2CmdService2()
        {
            //Thread.Sleep(100); //假设多线程的时候因某种原因阻塞100毫秒
        }
        static object myObject = new object();
        static public dp2CmdService2 Instance
        {
            get
            {
                lock (myObject)
                {
                    if (null == _instance)
                    {
                        _instance = new dp2CmdService2();
                    }
                    return _instance;
                }
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
            _channels.Login += _channels_Login;

            // dp2消息处理类
            this._dp2MsgHandler = new dp2MsgHandler();
            this._dp2MsgHandler.Init(this._channels, 
                this.dp2MServerUrl,
                this.weiXinLogDir,
                this.weiXinAppId);

        }

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
                    strError = result.ErrorInfo;
                    return -1;
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
        public SearchBiblioResult SearchBiblio(string remoteUserName,
            string strFrom,
            string strWord)
        {
            SearchBiblioResult searchRet = new SearchBiblioResult();
            searchRet.apiResult = new ApiResult();
            searchRet.apiResult.errorCode = 0;
            searchRet.apiResult.errorInfo = "";
            searchRet.records = new List<BiblioRecord>();
            searchRet.isCanNext = false;

            // 取下一页的情况
            if (strWord == "_N")
            {
                searchRet.apiResult.errorCode = -1;
                searchRet.apiResult.errorInfo = "尚未完成";
                return searchRet;
            }

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
                    searchRet.apiResult.errorCode = -1;
                    searchRet.apiResult.errorInfo = "检索出错：" + result.ErrorInfo;
                    return searchRet;
                }
                if (result.ResultCount == 0)
                {
                    searchRet.apiResult.errorCode = 0;
                    searchRet.apiResult.errorInfo = "未命中";
                    return searchRet;
                }

                // 记下命令的结果数量
                searchRet.resultCount = result.ResultCount;

                List<string> resultPathList = new List<string>();
                for (int i = 0; i < result.ResultCount; i++)
                {
                    if (i == 10)
                    {
                        searchRet.isCanNext = true;
                        break;
                    }

                    string xml = result.Records[i].Data;
                    /*<root><col>请让我慢慢长大</col><col>吴蓓著</col><col>天津教育出版社</col><col>2009</col><col>G61-53</col><col>儿童教育儿童教育</col><col></col><col>978-7-5309-5335-8</col></root>*/
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    string name = DomUtil.GetNodeText(dom.DocumentElement.SelectSingleNode("col"));

                    string path = result.Records[i].RecPath;
                    //int index = path.IndexOf("@");
                    //if (index >= 0)
                    //    path = path.Substring(0, index);


                    // todo，改为分批返回
                    BiblioRecord record = new BiblioRecord();
                    record.recPath = path;
                    record.name = name;
                    record.no = (i + 1).ToString();//todo 注意下一页的时候
                    searchRet.records.Add(record);
                }



                //// 将检索结果信息保存到检索命令中
                //searchCmd.BiblioResultPathList = resultPathList;
                //searchCmd.ResultNextStart = 0;
                //searchCmd.IsCanNextPage = true;

                //// 获得第一页检索结果
                //bool bRet = searchCmd.GetNextPage(out strFirstPage, out strError);
                //if (bRet == false)
                //{
                //    return -1;
                //}
                searchRet.apiResult.errorCode = 1;
                return searchRet;
            }
            catch (AggregateException ex)
            {
                searchRet.apiResult.errorCode = -1;
                searchRet.apiResult.errorInfo = MessageConnection.GetExceptionText(ex); ;
                return searchRet;
            }
            catch (Exception ex)
            {
                searchRet.apiResult.errorCode = -1;
                searchRet.apiResult.errorInfo = ex.Message; ;
                return searchRet;
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
