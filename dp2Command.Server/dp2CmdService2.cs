using DigitalPlatform.IO;
using DigitalPlatform.Message;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Xml;
using dp2Command.Service;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Senparc.Weixin.MP.CommonAPIs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace dp2Command.Service
{
    public class dp2CmdService2:dp2BaseCommandService
    {
        MessageConnectionCollection _channels = new MessageConnectionCollection();

        // dp2服务器地址与代理账号
        public string dp2MServerUrl = "";
        public string userName = "";
        public string password = "";

        public string weiXinAppId { get; set; }
        public string weiXinSecret { get; set; }

        // 背景图管理器
        public string TodayUrl = "";

        WxMsgThread _wxMsgThread = null;

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

        public void Init(string weiXinAppId,
            string weiXinSecret,
            string dp2MServerUrl,
            string userName,
            string password,
            string weiXinUrl,
            string weiXinDataDir,
            string mongoDbConnStr,
            string instancePrefix)
        {
            this.weiXinAppId = weiXinAppId;
            this.weiXinSecret = weiXinSecret;
            this.dp2MServerUrl = dp2MServerUrl;
            this.userName = userName;
            this.password = password;
            this.weiXinUrl = weiXinUrl;
            this.weiXinDataDir = weiXinDataDir;

            // 使用mongodb存储微信用户与读者绑定关系
            WxUserDatabase.Current.Open(mongoDbConnStr, instancePrefix);

            //全局只需注册一次
            AccessTokenContainer.Register(this.weiXinAppId, this.weiXinSecret);

            _channels.Login += _channels_Login;
            _channels.AddMessage += _channels_AddMessage;

            // 启一个线程取消息
            this._wxMsgThread = new WxMsgThread();
            this._wxMsgThread.Container = this;
            this._wxMsgThread.BeginThread();    // TODO: 应该在 MessageConnection 第一次连接成功以后，再启动这个线程比较好

            //Task.Factory.StartNew(() => DoLoadMessage("<default>"));


            // 初始img manager
            string imgFile = this.weiXinDataDir + "\\" + "image.xml";
            ImgManager imgManager = new ImgManager(imgFile);
            string todayNo = DateTime.Now.Day.ToString();
            TodayUrl = imgManager.GetImgUrl(todayNo);
        }

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

        #region 消息处理
        public async void DoLoadMessage()
        {
            string strGroupName = "_patronNotify";//"<default>";

            string strError = "";
            var accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            GetMessageRequest request = new GetMessageRequest(id,
                strGroupName, // "" 表示默认群组
                "",
                "",
                0,
                -1);
            try
            {
                MessageConnection connection = await this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    this.userName);  //todo这里用本方还是远方的账号，好像都可以，主要是确定在patronNotify
                MessageResult result = await connection.GetMessageAsync(
                        request,
                        FillMessage,
                        new TimeSpan(0, 1, 0),
                        cancel_token);
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
            return;

        ERROR1:

            this.WriteErrorLog(strError);
            //CustomApi.SendText(accessToken, openId, "error");

        }

        public void SendCustomerMsg(string openId, string text)
        {
            var accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);
            CustomApi.SendText(accessToken, openId, "error");
        }

        static string ToString(MessageResult result)
        {
            StringBuilder text = new StringBuilder();
            text.Append("ResultValue=" + result.Value + "\r\n");
            text.Append("ErrorInfo=" + result.ErrorInfo + "\r\n");
            text.Append("String=" + result.String + "\r\n");
            return text.ToString();
        }

        void FillMessage(long totalCount,
    long start,
    IList<MessageRecord> records,
    string errorInfo,
    string errorCode)
        {


            if (records != null)
            {
                SendMessageToWxUser(records);
            }
        }

        public void SendMessageToWxUser(IList<MessageRecord> records)
        {
            int i = 0;
            foreach (MessageRecord record in records)
            {
                i++;

                StringBuilder text = new StringBuilder();
                text.Append("***\r\n");
                text.Append("id=" + record.id + "\r\n");
                text.Append("data=" + record.data + "\r\n");
                text.Append("group=" + record.groups + "\r\n");
                text.Append("creator=" + record.creator + "\r\n");

                text.Append("format=" + record.format + "\r\n");
                text.Append("type=" + record.type + "\r\n");
                text.Append("thread=" + record.thread + "\r\n");

                text.Append("publishTime=" + record.publishTime + "\r\n");
                text.Append("expireTime=" + record.expireTime + "\r\n");

                //todo 将来data为xml，这里解析出微信用户id,解析出模板需要的字段
                string openId = "o4xvUviTxj2HbRqbQb9W2nMl4fGg";
                var templateId = "QcS3LoLHk37Jh0rgKJId2o93IZjulr5XxgshzlW5VkY";//换成已经在微信后台添加的模板Id
                var accessToken = AccessTokenContainer.GetAccessToken(this.weiXinAppId);
                var testData = new CaoQiTemplateData()
                {
                    first = new TemplateDataItem("您借阅的图书已超期，请尽快归还！\n", "#000000"),
                    keyword1 = new TemplateDataItem(record.data, "#000000"),//text.ToString()),// "请让我慢慢长大"),
                    keyword2 = new TemplateDataItem("2016-05-01", "#000000"),
                    keyword3 = new TemplateDataItem("1天", "#000000"),
                    remark = new TemplateDataItem("\n点击[详情]查看个人详细信息", "#CCCCCC")
                };
                var result = TemplateApi.SendTemplateMessage(accessToken, openId, templateId, "#FF0000", "dp2003.com", testData);
                // 测试
                if (i == 3)
                    return;

                // todo 发送完成，要删除一条消息

            }
        }




        void _channels_AddMessage(object sender, AddMessageEventArgs e)
        {
            // todo只处理_patronNotify郡里的消息

            if (e.Records != null)
            {
                SendMessageToWxUser(e.Records);
            }
        }

        #endregion

        #region 绑定解绑

        public List<WxUserItem> GetBindInfo(string weixinId)
        {
            List<WxUserItem> list = new List<WxUserItem>();

            // 目前只支持从数据库中查找
            list= WxUserDatabase.Current.GetByWeixinId(weixinId);


            return list;
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
        public override int Binding(string remoteUserName,
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
                    userItem = new WxUserItem();
                    userItem.weixinId = strWeiXinId;
                    userItem.libCode = libCode;
                    userItem.libUserName = remoteUserName;
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
        public override int Unbinding(string remoteUserName,
            string libCode,
            string strBarcode,
            string strWeiXinId,
             out string strError)
        {
            strError = "";

            // 从mongodb删除
            long nCount = WxUserDatabase.Current.Delete(strWeiXinId, strBarcode,libCode);


            // 调点对点解绑接口
            string fullWeixinId = dp2CommandUtility.C_WeiXinIdPrefix + strWeiXinId;
            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            BindPatronRequest request = new BindPatronRequest(id,
                "unbind",
                strBarcode,
                "",//password  todo
                fullWeixinId,
               "multiple,null_password",
                "xml");
            try
            {
                // 得到连接
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    remoteUserName).Result;

                // 调绑定函数，todo为啥await没反应
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
            if ( String.IsNullOrEmpty(userItem.readerBarcode)==false)
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
                    resultPathList.Add( path+ "*" + name);
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
                    string strBorrowInfo ="借阅情况:在架";
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
                    if (string.IsNullOrEmpty(strBorrower)==false)
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
            if (nRet ==-1)
                return -1;
            if (nRet == 0)
            {
                strError = "从dp2library未找到证条码号为'" + strReaderBarcode+ "'的记录"; //todo refID
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


                if (result.ResultCount == 0)
                    return 0;

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


        public int GetWeiXinId(string code, out string weiXinId,
            out string strError)
        {
            strError = "";
            weiXinId = "";

            //用code换取access_token
            var result = OAuthApi.GetAccessToken(this.weiXinAppId,this.weiXinSecret, code);
            if (result.errcode != ReturnCode.请求成功)
            {
                strError="获取微信id出错：" + result.errmsg;
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
    }

    public class CaoQiTemplateData
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem remark { get; set; }

    }
}
