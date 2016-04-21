using DigitalPlatform.IO;
using DigitalPlatform.Message;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Xml;
using dp2Command.Service;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.CommonAPIs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace dp2Command.Server
{
    public class dp2CmdService2:dp2BaseCommandService
    {
        MessageConnectionCollection _channels = new MessageConnectionCollection();


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


        // dp2服务器地址与代理账号
        public string dp2MServerUrl = "";
        public string userName = "";
        public string password = "";






        public void Init(string dp2MServerUrl,
            string userName,
            string password,
            string weiXinUrl,
            string weiXinLogDir,
            string mongoDbConnStr,
            string instancePrefix)
        {
            this.dp2MServerUrl = dp2MServerUrl;
            this.userName = userName;
            this.password = password;
            this.weiXinUrl = weiXinUrl;
            this.weiXinLogDir = weiXinLogDir;

            _channels.Login += _channels_Login;

            // 使用mongodb存储微信用户与读者绑定关系
            WxUserDatabase.Current.Open(mongoDbConnStr, instancePrefix);
        }

        void _channels_Login(object sender, LoginEventArgs e)
        {
            MessageConnection connection = sender as MessageConnection;

            if (string.IsNullOrEmpty(this.userName) == true)
                throw new Exception("尚未指定用户名，无法进行登录");

            MessageResult result = connection.LoginAsync(
                this.userName,
                this.password,
                "",
                "",
                "property").Result;
            if (result.Value == -1)
            {
                throw new Exception(result.ErrorInfo);
            }
        }



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
        public override int Binding(string strBarcode,
            string strPassword,
            string strWeiXinId,
            out string strReaderBarcode,
            out string strError)
        {
            strError = "";
            strReaderBarcode = "";
            long lRet = -1;


            CancellationToken cancel_token = new CancellationToken();

            string fullWeixinId = dp2CommandUtility.C_WeiXinIdPrefix + strWeiXinId;
            string id = Guid.NewGuid().ToString();
            BindPatronRequest request = new BindPatronRequest(id,
                "bind",
                strBarcode,
                strPassword,
                fullWeixinId,
               "multiple",//single
                "xml");

            try
            {
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    this.remoteUserName).Result;


                BindPatronResult result = connection.BindPatronAsync(
                     this.remoteUserName,
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
                WxUserItem userItem = WxUserDatabase.Current.GetOneOrEmptyPatron(strWeiXinId, this.libCode, strBarcode);
                if (userItem == null)
                {
                    userItem = new WxUserItem();
                    userItem.weixinId = strWeiXinId;
                    userItem.libCode = this.libCode;
                    userItem.libUserName = this.remoteUserName;
                    userItem.readerBarcode = strBarcode;
                    userItem.readerName = name;
                    userItem.xml = xml;
                    userItem.refID = refID;
                    userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                    userItem.updateTime = userItem.createTime;
                    userItem.isActive = 1;
                    WxUserDatabase.Current.Add(userItem);
                }
                else
                {
                    userItem.readerBarcode = strBarcode;
                    userItem.readerName = name;
                    userItem.xml = xml;
                    userItem.refID = refID;
                    userItem.updateTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                    userItem.isActive = 1;
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
        public override int Unbinding1(string strBarcode,
            string strWeiXinId,
             out string strError)
        {
            strError = "";

            // 从mongodb删除
            long nCount = WxUserDatabase.Current.Delete(strWeiXinId, strBarcode,this.libCode);


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
                    this.remoteUserName).Result;

                // 调绑定函数，todo为啥await没反应
                BindPatronResult result = connection.BindPatronAsync(
                     this.remoteUserName,
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

        public override long SearchPatronByWeiXinId(string strWeiXinId,
            out string strBarcode,
            out string strError)
        {
            strError = "";
            strBarcode = "";

            // 从mongodb中检查是否绑定了用户
            WxUserItem userItem = WxUserDatabase.Current.GetActiveOrFirst(strWeiXinId, this.libCode);
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
                    this.remoteUserName).Result;

                SearchResult result = connection.SearchAsync(
                    this.remoteUserName,
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
                string fristBarcode = "";
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
                            userItem.libCode = this.libCode;
                            userItem.libUserName = this.remoteUserName;
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

        public override long SearchBiblio(string strWord,
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
                    this.remoteUserName).Result;

                SearchResult result = connection.SearchAsync(
                    this.remoteUserName,
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



        public override int GetDetailBiblioInfo(SearchCommand searchCmd,
            int nIndex,
            out string strBiblioInfo,
            out string strError)
        {
            strBiblioInfo = "";
            strError = "未实现";
            Debug.Assert(searchCmd != null);

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

            // 开始时间
            DateTime start_time = DateTime.Now;


            int nRet = 0;
            string strInfo = "";
            nRet = this.GetBiblioAndSub(strPath,  //GetBiblioAndSub
                out strInfo,
                out strError);
            if (nRet == -1 || nRet == 0)
                return nRet;
            strBiblioInfo += strInfo + "\n";

            /*
            //微信时间不够，先不取summary
            // 取出summary
            string strSummary ="";
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
            */

            // 计算用了多少时间
            TimeSpan time_length = DateTime.Now - start_time;
            strBiblioInfo = "time span: " + time_length.TotalSeconds.ToString() + " secs" + "\n"
                + strBiblioInfo;
             
            return 1;
        }

        private int GetBiblioSummary(string biblioPath,
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
                    this.remoteUserName).Result;

                SearchResult result = connection.SearchAsync(
                    this.remoteUserName,
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


        private long GetItemInfo(string biblioPath,
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
                    this.remoteUserName).Result;

                SearchResult result = connection.SearchAsync(
                    this.remoteUserName,
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


        private int GetBiblioAndSub(string biblioPath,
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
                    3,
                    0,
                    -1);

                try
                {
                    MessageConnection connection = this._channels.GetConnectionAsync(
    this.dp2MServerUrl,
    this.remoteUserName).Result;


                    Task<SearchResult> task1 = connection.SearchAsync(
    this.remoteUserName,
    request1,
    new TimeSpan(0, 1, 0),
    cancel_token);
                    Task<SearchResult> task2 = connection.SearchAsync(
                         this.remoteUserName,
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
                    long nMax = 10;
                    if (task2.Result.ResultCount < nMax)
                        nMax = task2.Result.ResultCount;
                    for (int i = 0; i < nMax; i++)
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

        private int Test2Api(string biblioPath,
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
this.remoteUserName).Result;


                Task<SearchResult> task1 = connection.SearchAsync(
this.remoteUserName,
request1,
new TimeSpan(0, 1, 0),
cancel_token);
                Task<SearchResult> task2 = connection.SearchAsync(
                     this.remoteUserName,
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
                strInfo = task1.Result.Records[0].Data+"\n";

                // api 2
                strInfo += task2.Result.Records[0].Data;


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
        public override int GetMyInfo(string strReaderBarcode,
            out string strMyInfo,
            out string strError)
        {
            strError = "";
            strMyInfo = "";
            Debug.Assert(String.IsNullOrEmpty(strReaderBarcode) == false);


            // 得到高级xml
            string strXml = "";
            int nRet = this.GetPatronInfo(strReaderBarcode,
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
        public override int GetBorrowInfo(string strReaderBarcode, 
            out string strBorrowInfo, 
            out string strError)
        {
            strError = "";
            strBorrowInfo = "";


            // 得到高级xml
            string strXml = "";
            long lRet = this.GetPatronInfo(strReaderBarcode,
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


        public int GetPatronInfo(string strReaderBarocde,  //todo refID
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
                    this.remoteUserName).Result;

                SearchResult result = connection.SearchAsync(
                    this.remoteUserName,
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

    }
}
