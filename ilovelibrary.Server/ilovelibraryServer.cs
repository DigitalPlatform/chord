using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryRestClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
using System.Collections;
using DigitalPlatform.Marc;

namespace ilovelibrary.Server
{
    public class ilovelibraryServer
    {
        //=================
        // 设为单一实例
        static ilovelibraryServer _instance;
        private ilovelibraryServer()
        {
            //Thread.Sleep(100); //假设多线程的时候因某种原因阻塞100毫秒
        }
        static object myObject = new object();
        static public ilovelibraryServer Instance
        {
            get
            {
                lock (myObject)
                {
                    if (null == _instance)
                    {
                        _instance = new ilovelibraryServer();
                    }
                    return _instance;
                }
            }
        }
        //===========

        // dp2服务器地址
        public string dp2LibraryUrl = "";//"http://dp2003.com/dp2library/rest/"; //"http://localhost:8001/dp2library/rest/";//
        public string dataDir = "";
        public string dp2OpacUrl = "";

        // dp2通道池
        public LibraryChannelPool ChannelPool = null;

        // 背景图管理器
        public string TodayUrl="";
        public bool isVerifyBarcode = true; //是否校验证条码号

        // 检索用的有实体库的书目库
        string strBiblioDbNames = "";

        public void Init(string strDp2LibraryUrl, string strDataDir,string strDp2OpacUrl)
        {
            this.dp2LibraryUrl = strDp2LibraryUrl;
            this.dp2OpacUrl = strDp2OpacUrl;
            this.dataDir = strDataDir;
            PathUtil.CreateDirIfNeed(this.dataDir);	// 确保目录创建

            // 通道池对象
            ChannelPool = new LibraryChannelPool();
            ChannelPool.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            ChannelPool.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);            

            // 初始img manager
            string imgFile = this.dataDir+ "\\" +"image.xml";
            ImgManager imgManager = new ImgManager(imgFile);
            string todayNo = DateTime.Now.Day.ToString();
            TodayUrl = imgManager.GetImgUrl(todayNo);
        }

        /// <summary>
        /// 自动登录，提供密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == false)
            {
                e.Cancel = true;
                return;
            }

            // 这里赋上通道自己的账号，而不是使用全局变量。
            // 因为从池中征用通道后，都给通道设了密码。账号密码是通道的属性。
            LibraryChannel channel = sender as LibraryChannel;
            e.LibraryServerUrl = channel.Url;
            e.UserName = channel.UserName;
            e.Password = channel.Password;
            e.Parameters = channel.Parameters;
        }

        //      result.Value 0: 不是合法的条码号 1:合法的读者证条码号 2:合法的册条码号
        // -2 服务器端未配置该函数
        public ApiResult VerifyBarcode(SessionInfo sessionInfo,
            string strBarcode)
        {
            string strError = "";
            ApiResult result = new ApiResult();

            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2LibraryUrl,
                sessionInfo.UserName);
            channel.Password = sessionInfo.Password;
            channel.Parameters = sessionInfo.Parameters;
            try
            {
                // todo 这里传的工作人员的libraryCode对吗？
                long ret = channel.VerifyBarcode(sessionInfo.LibraryCode,strBarcode,out strError);
                if (ret < 0)  //-1未设置校验函数
                {
                    this.isVerifyBarcode = false;
                }
                result.errorCode = (int)ret;
                result.errorInfo = strError;


                return result;
            }
            catch (WebException wex)
            {
                result.errorCode = -1;
                result.errorInfo = "访问dp2library服务器出错：" + wex.Message + "\n请联系系统管理员修改dp2library服务器地址配置信息。";
                return result;
            }
            catch (Exception ex)
            {
                result.errorCode = -1;
                result.errorInfo= ex.Message;
                return result;
            }
            finally
            {
                this.ChannelPool.ReturnChannel(channel);
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="strUserName"></param>
        /// <param name="strPassword"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public SessionInfo Login(string strUserName, string strPassword,
            bool isReader,
            out string rights,
            out string strError)
        {
            strError = "";
            rights = "";

            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2LibraryUrl, strUserName);
            channel.Password = strPassword;
            try
            {
                string strParam = "";
                if (isReader == true)
                    strParam = "type=reader";

                //光光 0:05:23
                //最近我为 dp2library 增加了一种强制检查前端版本号的机制。ilovelibrary 也是一个“前端”，其 Login() API 需要新的参数：
                //光光 0:05:26
                //e.Parameters += ",client=dp2circulation|" + Program.ClientVersion;
                //光光 0:05:54
                //其中 dp2circulation 可以换成 ilovelibrary。竖线后面是个版本号，例如 1.1 之类
                //光光 0:06:21
                //整个加起来就是 ,client=ilovelibrary|1.1
                //光光
                //为了测试最新的强制 dp2library 验证前端版本号功能，需要在 library.xml 的根元素下配置这么一个片段：
                //光光 2016/1/7 21:58:00
                //<login checkClientVersion="true" />
                //光光 2016/1/7 21:58:36
                //今晚红泥巴服务器的 dp2library 我已经这样配置了。ilovelibrary 登录的时候就会失败，说版本不够新
                //光光 2016/1/7 21:59:01
                //需要在 Login() API 的 parameters 参数中，添加一点内容 
                //光光 2016/1/7 21:59:24
                //,client=ilovelibrary|1.0
                strParam += ",client=ilovelibrary|1.0";

                LoginResponse ret = channel.Login(strUserName, strPassword, strParam);
                if (ret.LoginResult.Value != 1)
                {
                    strError = ret.LoginResult.ErrorInfo;
                    return null;
                }
                strUserName = ret.strOutputUserName;
                SessionInfo sessionInfo = new SessionInfo();
                sessionInfo.UserName = strUserName;
                sessionInfo.Password = strPassword;
                sessionInfo.Parameters = strParam;
                sessionInfo.Rights = ret.strRights;
                sessionInfo.LibraryCode = ret.strLibraryCode;
                sessionInfo.isReader = isReader;
                sessionInfo.PersonalLibrary = "";

                // 初始一下可用的书目数据库
                int nRet = this.GetBiblioDbNames(channel, out this.strBiblioDbNames, out strError);
                if (nRet == -1)
                {
                    strError = "获得书目库出错："+strError;
                    return null;
                }

                // 取一下书斋名称
                if (strParam.Contains("type=reader") == true)
                {
                    GetReaderInfoResponse res = channel.GetReaderInfo(strUserName, "xml");
                    if (res.GetReaderInfoResult.Value == -1 || res.GetReaderInfoResult.Value == 0)
                    {
                        strError = "获得读者记录出错：" + strError;
                        return null;
                    }
                    Debug.Assert(res.results != null, "res.results不应该为null");
                    Debug.Assert(res.results.Length==1, "res.results应该包括一个值");

                    string xml = res.results[0];
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    XmlNode node = dom.DocumentElement.SelectSingleNode("personalLibrary");
                    if (node !=null)
                        sessionInfo.PersonalLibrary = DomUtil.GetNodeText(node);
                }
                return sessionInfo;
            }
            catch (WebException wex)
            {
                strError = "访问dp2library服务器出错："+wex.Message+"\n请联系系统管理员修改dp2library服务器地址配置信息。";
                return null;
            }
            catch (Exception ex)
            {
                strError = "Login() 函数出现异常:" + ex.Message;
                return null;
            }
            finally
            {
                this.ChannelPool.ReturnChannel(channel);
            }
        }

        public void ParseReaderXml(string strXml, PatronResult patronResult)    
        {
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
            int nRet = this.CheckReaderExpireAndState(dom, out strError);
            if (nRet !=0)
                patron.isWarning = 1;
                

            // 赋给返回对象
            patronResult.patron = patron;

            // 警告信息，显示在头像旁边
            string strWarningText = "";

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
                    overdueInfo.barcodeUrl = this.dp2OpacUrl + "/book.aspx?barcode=" + strBarcode;
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
            // 赋到返回对象上
            patronResult.overdueList = overdueLit;

            // ***
            // 在借册
            nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
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
                patron.maxBorrowCount = "获取当前读者可借总册数出错：未找到对应节点。";
            }
            else
            {
                string maxCount = DomUtil.GetAttr(nodeMax, "value");
                if (maxCount == "")
                {
                    patron.maxBorrowCount = "获取当前读者可借总册数出错：未设置对应值。";
                }
                else
                {
                    patron.maxBorrowCount = "最多可借:" + maxCount; ;
                    XmlNode nodeCurrent = dom.DocumentElement.SelectSingleNode("info/item[@name='当前还可借']");
                    if (nodeCurrent == null)
                    {
                        patron.curBorrowCount = "获取当前还可借出错：未找到对应节点。";
                    }
                    else
                    {
                        patron.curBorrowCount = "当前可借:" + DomUtil.GetAttr(nodeCurrent, "value");
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
            List<BorrowInfo> borrowList = new List<BorrowInfo>();
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
                BorrowInfo borrowInfo = new BorrowInfo();
                borrowInfo.barcode = strBarcode;
                borrowInfo.renewNo = strRenewNo;
                borrowInfo.borrowDate = strBorrowDate;
                borrowInfo.period = strPeriod;
                borrowInfo.borrowOperator = strOperator;
                borrowInfo.renewComment = strRenewComment;
                borrowInfo.overdue = strOverdueInfo;
                borrowInfo.returnDate = strTimeReturning;
                borrowInfo.barcodeUrl = this.dp2OpacUrl + "/book.aspx?barcode=" + strBarcode;
                borrowInfo.rowCss = rowCss;
                borrowList.Add(borrowInfo);

            }
            if (nOverdueCount > 0)
                strWarningText += "<div class='warning overdue'><div class='number'>" + nOverdueCount.ToString() + "</div><div class='text'>已超期</div></div>";
            // 赋给返回对象
            patronResult.borrowList = borrowList;




            // ***
            // 预约请求
            nodes = dom.DocumentElement.SelectNodes("reservations/request");
            if (nodes.Count > 0)
            {
                List<ReservationInfo> reservationList = new List<ReservationInfo>();
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
                    reservationInfo.barcodes = strBarcodesHtml;
                    reservationInfo.state = strState;
                    reservationInfo.stateText = strStateText;
                    reservationInfo.requestdate = strRequestDate;
                    reservationInfo.operatorAccount = strOperator;
                    reservationInfo.arrivedBarcode = strArrivedItemBarcode;
                    reservationInfo.fullBarcodes = strBarcodes + "*" + strArrivedItemBarcode;
                    reservationList.Add(reservationInfo);
                }

                patronResult.reservationList = reservationList;

                if (nArriveCount > 0)
                    strWarningText += "<div class='warning arrive'><div class='number'>" + nArriveCount.ToString() + "</div><div class='text'>预约到书</div></div>";
            }


            patronResult.patron.warningText = strWarningText;
        }

        /// <summary>
        /// 获得读者基本信息
        /// </summary>
        /// <param name="sessionInfo"></param>
        /// <param name="strReaderBarcode"></param>
        /// <returns></returns>
        public PatronResult GetPatronInfo(SessionInfo sessionInfo,
            string strReaderBarcode)
        {
            // 返回对象
            PatronResult result = new PatronResult();
            result.patron = null;
            result.apiResult = new ApiResult();
            if (sessionInfo == null)
            {
                result.apiResult.errorCode = -1;
                result.apiResult.errorInfo = "尚未登录";
                return result;
            }


            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2LibraryUrl, sessionInfo.UserName);
            channel.Password = sessionInfo.Password;
            channel.Parameters = sessionInfo.Parameters;
            try
            {
                // 先根据barcode检索出来,得到原记录与时间戳
                GetReaderInfoResponse response = channel.GetReaderInfo(strReaderBarcode,
                    "advancexml");//
                if (response.GetReaderInfoResult.Value == -1)
                {
                    result.apiResult.errorCode = -1;
                    result.apiResult.errorInfo = "获取读者记录出错：" + response.GetReaderInfoResult.ErrorInfo;
                    return result;
                }
                else if (response.GetReaderInfoResult.Value == 0)
                {
                    result.apiResult.errorCode = -1;
                    result.apiResult.errorInfo = "未找到证条码号为[" + strReaderBarcode + "]的读者记录";
                    return result;
                }
                else if (response.GetReaderInfoResult.Value > 1)
                {
                    result.apiResult.errorCode = (int)response.GetReaderInfoResult.Value;
                    result.apiResult.errorInfo = "查到" + response.GetReaderInfoResult.Value + "条读者记录";

                    string strHtml = "";
                    string strError = "";
                    int nRet = ReaderMulPath2Html(channel,
                        response.strRecPath,
                        out strHtml,
                        out strError);
                    if (nRet == -1)
                    {
                        result.apiResult.errorCode = -1;
                        result.apiResult.errorInfo = strError;
                        return result;
                    }

                    // 直接返回
                    result.multipleReaderHtml = strHtml;
                    return result;
                }

                string strXml = response.results[0];

                //解析读者xml到对象
                this.ParseReaderXml(strXml, result);

                return result;
            }
            catch (Exception ex)
            {
                result.apiResult.errorCode = -1;
                result.apiResult.errorInfo = ex.Message;
                return result;
            }
            finally
            {
                this.ChannelPool.ReturnChannel(channel);
            }
        }

        public int ReaderMulPath2Html(LibraryChannel channel, 
            string strRecPath, 
            out string strHtml,
            out string strError)
        {
            strHtml = "";
            strError = "";

            StringBuilder sr = new StringBuilder(1024);
            sr.Append("<table  class='table readerTable' align='center' border='0' cellspacing='0' cellpadding='0' id='tab' >");


            // 拆分rec path
            List<string> pathList = StringUtil.SplitList(strRecPath);
            int i = 0;
            foreach (string onePath in pathList)
            {
                GetReaderInfoResponse response = channel.GetReaderInfo("@path:" + onePath,
                    "xml");
                if (response.GetReaderInfoResult.Value == -1)
                {
                    strError = "获取读者记录出错：" + response.GetReaderInfoResult.ErrorInfo;
                    return -1;
                }
                else if (response.GetReaderInfoResult.Value == 0)
                {
                    strError = "未找到路径为[" + onePath + "]的读者记录";
                    return -1;
                }
                if (response.results == null || response.results.Length < 1)
                {
                    strError = "results error";
                    return -1;
                }

                string strXml = response.results[0];
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "XML装入DOM失败: " + ex.Message;
                    return -1;
                }

                string strBarcode = DomUtil.GetElementText(dom.DocumentElement,
                    "barcode");
                string strState = DomUtil.GetElementText(dom.DocumentElement,
                    "state");
                string strName = DomUtil.GetElementText(dom.DocumentElement,
                    "name");
                string strGender = DomUtil.GetElementText(dom.DocumentElement,
                    "gender");
                string strDepartment = DomUtil.GetElementText(dom.DocumentElement,
                    "department");
                string strIdCardNumber = DomUtil.GetElementText(dom.DocumentElement,
                    "idCardNumber");

                string strComment = DomUtil.GetElementText(dom.DocumentElement,
                    "comment");

                string strSelected = "";
                if (i == 0)
                    strSelected = " reader-selected-bg";


                sr.Append("<tr class='reader-tr " + strSelected + "' onclick='readerClick(this)' ondblclick='okSelectReaderLayer(this)'>");
                sr.Append(@"<td>        <table>
            <tr>
                <td class='itemLeftTd'>证条码号</td>
                <td class='readerBarcode-td itemRightTd'>" + strBarcode + @"</td>
            </tr>
            <tr>
                <td class='itemLeftTd'>状态</td>
                <td class='itemRightTd'>" + strState + @"</td>
            </tr>
            <tr>
                <td class='itemLeftTd'>姓名</td>
                <td class='itemRightTd'>" + strName + @"</td>
            </tr>
            <tr>
                <td class='itemLeftTd'>性别</td>
                <td class='itemRightTd'>" + strGender + @"</td>
            </tr>
            <tr>
                <td class='itemLeftTd'>部门</td>
                <td class='itemRightTd'>" + strDepartment + @"</td>
            </tr>                                                
            <tr>
                <td class='itemLeftTd'>身份证号</td>
                <td class='itemRightTd'>" + strIdCardNumber + @"</td>
            </tr> 
        </table></td></tr>");


                i++;
            }

            sr.Append("</table>");


            // todo StringUtil.SplitList(strRecPath).Count < lRet

            //todo 检查是否有重新的证条码号，严重错误

            strHtml = sr.ToString();
            return 0;
        }

        static int GetBarcodesCount(string strBarcodes)
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
                strResult += "<a "
                    + (string.IsNullOrEmpty(strDisableClass) == false && strBarcode != strArrivedItemBarcode ? "class='" + strDisableClass + "'" : "")
                    + " href=' "+ this.dp2OpacUrl + "/book.aspx?barcode=" + strBarcode + "'  target='_blank' " + ">" + strBarcode + "</a>";
            }

            return strResult;
        }

        // return:
        //      -1  检测过程发生了错误。应当作不能借阅来处理
        //      0   可以借阅
        //      1   证已经过了失效期，不能借阅
        //      2   证有不让借阅的状态
        public int CheckReaderExpireAndState(XmlDocument readerdom,
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
        private string GetContactString(XmlDocument dom)
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
        /// 获取读者摘要
        /// </summary>
        /// <param name="sessionInfo"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public PatronSummaryResult GetPatronSummary(SessionInfo sessionInfo, string strReaderBarcode)
        {
            // 返回对象
            PatronSummaryResult result = new PatronSummaryResult();
            result.summary = "";
            result.apiResult = new ApiResult();
            if (sessionInfo == null)
            {
                result.apiResult.errorCode = -1;
                result.apiResult.errorInfo = "尚未登录";
                return result;
            }

            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2LibraryUrl, sessionInfo.UserName);
            channel.Password = sessionInfo.Password;
            channel.Parameters = sessionInfo.Parameters;
            try
            {
                string strSummary = "";
                string strError = "";
                int nRet =this.GetPatronSummary(channel, strReaderBarcode,
                    true,
                    out strSummary,
                    out strError);
                if (nRet == -1)
                {
                    result.apiResult.errorCode = -1;
                    result.apiResult.errorInfo = strError;
                    return result;
                }
                if (strSummary!="")
                {
                    result.summary = "<span style=' font-size: 14.8px;font-weight:bold'>" + strSummary + "</span>";
                }                    

                return result;
            }
            catch (Exception ex)
            {
                result.apiResult.errorCode = -1;
                result.apiResult.errorInfo = ex.Message;
                return result;
            }
            finally
            {
                this.ChannelPool.ReturnChannel(channel);
            }
        }

        private int GetPatronSummary(LibraryChannel channel, string strReaderBarcode,
            bool isContainDept,
            out string strSummary,
            out string strError)
        {
            strSummary = "";
            strError = "";

            // 先根据barcode检索出来,得到原记录与时间戳
            GetReaderInfoResponse response = channel.GetReaderInfo(strReaderBarcode,//"@path:" + strRecPath,
               "advancexml");// "advancexml,advancexml_borrow_bibliosummary,advancexml_overdue_bibliosummary");
            if (response.GetReaderInfoResult.Value == -1)
            {
                strError = "获取读者记录出错：" + response.GetReaderInfoResult.ErrorInfo;
                return -1;
            }
            else if (response.GetReaderInfoResult.Value == 0)
            {
                strError = "未找到证条码号为[" + strReaderBarcode + "]的读者记录";
                return -1;
            }
            else if (response.GetReaderInfoResult.Value > 1)
            {
                strError = "异常：根据证条码号[" + strReaderBarcode + "]找到多条读者记录，请联系管理员。";
                return -1;
            }
            string strXml = response.results[0];

            // 取出个人信息
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);
            string name = DomUtil.GetElementText(dom.DocumentElement, "name");
            string department = DomUtil.GetElementText(dom.DocumentElement, "department");

            if (name != "")
            {
                strSummary = name;//"<span style=' font-size: 14.8px;font-weight:bold'>" 

                if (department != "" && isContainDept==true)
                    strSummary += "（" + department + "）";
            }
            return 0;

        }


        /// <summary>
        /// 获取书目摘要
        /// </summary>
        /// <param name="strItemBarcode"></param>
        /// <returns></returns>
        public string GetBiblioSummary(SessionInfo sessionInfo, string strItemBarcode)
        {
            if (sessionInfo == null)
                throw new Exception("尚未登录");

            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2LibraryUrl, sessionInfo.UserName);
            channel.Password = sessionInfo.Password;
            channel.Parameters = sessionInfo.Parameters;
            try
            {
                GetBiblioSummaryResponse result = channel.GetBiblioSummary(strItemBarcode,"");
                if (result.GetBiblioSummaryResult.Value == -1 || result.GetBiblioSummaryResult.Value==0)
                    return result.GetBiblioSummaryResult.ErrorInfo;

                return result.strSummary;
            }
            finally
            {
                this.ChannelPool.ReturnChannel(channel);
            }
        }

        // 获取多个item的summary
        public string GetBarcodesSummary(SessionInfo sessionInfo,
            string strBarcodes)
        {
            string strSummary = "";
             string strArrivedItemBarcode="";
            int nIndex = strBarcodes.IndexOf("*");
            if (nIndex > 0)
            {
                string tempBarcodes = strBarcodes.Substring(0, nIndex);
                strArrivedItemBarcode = strBarcodes.Substring(nIndex+1);
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

                // 2012/3/28
                LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2LibraryUrl, sessionInfo.UserName);
                channel.Password = sessionInfo.Password;
                channel.Parameters = sessionInfo.Parameters;
                try
                {
                    GetBiblioSummaryResponse result = channel.GetBiblioSummary(strBarcode,strPrevBiblioRecPath);
                    strOneSummary = result.strSummary;
                    strBiblioRecPath = result.strBiblioRecPath;

                    if (result.GetBiblioSummaryResult.Value == -1 || result.GetBiblioSummaryResult.Value == 0)
                        strOneSummary = result.GetBiblioSummaryResult.ErrorInfo;

                    if (strOneSummary=="" && strPrevBiblioRecPath == strBiblioRecPath)
                        strOneSummary = "(同上)";

                    string strClass = "";
                    if (string.IsNullOrEmpty(strDisableClass) == false && strBarcode != strArrivedItemBarcode)
                        strClass=" class='"+strDisableClass+"' ";

                    string strBarcodeLink = "<a " + strClass
                        + " href='" + this.dp2OpacUrl + "/book.aspx?barcode=" + strBarcode + "'   target='_blank' " + ">" + strBarcode + "</a>";

                    /*
                    string shortText = strOneSummary;
                    if (shortText.Length > 30)
                        shortText = shortText.Substring(0, 30) + "···";
                    var strOneSummaryTip = "<a href='#' data-toggle='tooltip' data-placement='right' "
                           + " title='" + strOneSummary + "'>"
                           + shortText
                           + "</a>";
                     */

                    var strOneSummaryOverflow = "<div style='width:100%;white-space:nowrap;overflow:hidden; text-overflow:ellipsis;'  title='" + strOneSummary + "'>"
                       + strOneSummary 
                       + "</div>";

                    strSummary += "<table style='width:100%;table-layout:fixed;'>"
                        +"<tr>"
                        + "<td width='10%;vertical-align:middle'>" + strBarcodeLink + "</td>"
                        +"<td>" + strOneSummaryOverflow + "</td>"
                        +"</tr></table>"; 


                    strPrevBiblioRecPath = strBiblioRecPath;

                }
                finally
                {
                    this.ChannelPool.ReturnChannel(channel);
                }
            }


            return strSummary;
        }



        // 
        // return:
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        //      1   出错，但希望继续后面的操作
        /// <summary>
        /// 获得编目库属性列表
        /// </summary>
        /// <param name="bPrepareSearch">是否要准备通道</param>
        /// <returns>-1: 出错，不希望继续以后的操作; 0: 成功; 1: 出错，但希望继续后面的操作</returns>
        public int GetBiblioDbNames(LibraryChannel channel,
            out string strDbNames,
            out string strError)
        {
            strDbNames = "";
            strError = "";
            int nRet = 0;

            string strDbXml = "";
            nRet = channel.GetAllDatabase(out strDbXml, out strError);
            if (nRet == -1)
                return -1;
            if (String.IsNullOrEmpty(strDbXml) == true)
                return 0;

            List<string> results = new List<string>();
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strDbXml);
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database[@type='biblio']");
            foreach (XmlNode node in nodes)
            {

                string strName = DomUtil.GetAttr(node, "name");
                string strItemDbName = DomUtil.GetAttr(node, "entityDbName");
                if (string.IsNullOrEmpty(strName) == false &&
    string.IsNullOrEmpty(strItemDbName) == false)
                {
                    results.Add(strName);
                }
            }

            strDbNames= StringUtil.MakePathList(results);
            return 0;
        }


        /// <summary>
        /// 获取书目摘要
        /// </summary>
        /// <param name="strItemBarcode"></param>
        /// <returns></returns>
        public SearchItemResult SearchItem(SessionInfo sessionInfo,
            string functionType,
            string searchText)
        {
            string strError = "";

            // 返回对象
            SearchItemResult result = new SearchItemResult();
            result.itemList = new List<BiblioItem>();
            result.apiResult = new ApiResult();
            if (sessionInfo == null)
            {
                result.apiResult.errorCode = -1;
                result.apiResult.errorInfo = "尚未登录";
                return result;
            }

            // 置空
            this._biblioXmlTable.Clear();

            // searchText格式   检索途径::关键词
            string strFromStyle="";
            string strQueryWord = "";
            int nIndex = searchText.IndexOf("::");
            if (nIndex > 0)
            {
                strFromStyle = searchText.Substring(0, nIndex);
                strQueryWord = searchText.Substring(nIndex + 2);
            }
            if (String.IsNullOrEmpty(strFromStyle) == true)
            {
                result.apiResult.errorCode = -1;
                result.apiResult.errorInfo = "尚未选定检索途径";
                return result;
            }
            if (String.IsNullOrEmpty(strQueryWord) == true)
            {
                result.apiResult.errorCode = -1;
                result.apiResult.errorInfo = "尚未设置检索词";
                return result;
            }


            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2LibraryUrl, sessionInfo.UserName);
            channel.Password = sessionInfo.Password;
            channel.Parameters = sessionInfo.Parameters;
            try
            {
                string strMatchStyle = "left"; 
                /*
        public long SearchBiblio(
            string strBiblioDbNames,
            string strQueryWord,
            int nPerMax,
            string strFromStyle,
            string strMatchStyle,
            string strResultSetName,
             string strOutputStyle,
            out string strQueryXml,
            out string strError)
                 */
                string strQueryXml = "";
                long lRet = channel.SearchBiblio( strBiblioDbNames,  //this.GetBiblioDbNames(),   todo
                    strQueryWord,   // this.textBox_queryWord.Text,
                    1000, // TODO: 最多检索1000条的限制，可以作为参数配置？
                    strFromStyle,
                    strMatchStyle,                    
                    null,   // strResultSetName
                    "", // strOutputStyle
                    out strQueryXml,
                    out strError);
                if (lRet == -1)
                {
                    result.apiResult.errorCode = -1;
                    result.apiResult.errorInfo = "检索失败：" + strError;
                    return result;
                }                

                // 未命中
                long lHitCount = lRet;
                if (lHitCount == 0)
                {
                    strError = "从途径 '" + strFromStyle + "' 检索 '" + strQueryWord + "' 没有命中";
                    result.apiResult.errorCode =0;
                    result.apiResult.errorInfo = strError;
                    return result;
                }

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                Record[] searchresults = null;
                List<string> biblioRecPaths = new List<string>();
                // 装入浏览格式
                for (; ; )
                {
                    /*
                            public long GetSearchResult(
            string strResultSetName,
            long lStart,
            long lCount,
            string strBrowseInfoStyle,
            string strLang,
            out Record[] searchresults,
            out string strError)
                     */
                    lRet = channel.GetSearchResult(
                        null,   // strResultSetName
                        lStart,
                        lPerCount,
                        "id", // "id,cols",
                        "zh",
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        result.apiResult.errorCode = -1;
                        result.apiResult.errorInfo = "检索失败：" + strError;
                        return result;
                    }

                    if (lRet == 0)
                    {
                        //result.apiResult.errorCode = 0;
                        //result.apiResult.errorInfo = "未命中";
                        //return result;
                        break;
                    }

                    // 处理浏览结果
                    foreach (Record record in searchresults)
                    {
                        biblioRecPaths.Add(record.Path);

                        // 存储书目记录 XML
                        if (functionType == "read" && record.RecordBody != null)
                            this._biblioXmlTable[record.Path] = record.RecordBody.Xml;
                    }

                    lStart += searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;
                }

                // 将每条书目记录下属的册记录装入
                List<BiblioItem> allItemList = new List<BiblioItem>();
                foreach (string strBiblioRecPath in biblioRecPaths)
                {
                    List<BiblioItem> itemList = null;
                    int nRet = LoadBiblioSubItems(channel,
                        sessionInfo,
                        functionType,
                        strBiblioRecPath,
                        out itemList,
                        out strError);
                    if (nRet == -1)
                    {
                        result.apiResult.errorCode = -1;
                        result.apiResult.errorInfo = strError;
                        return result;
                    }

                    // 加到全局记录里
                    allItemList.AddRange(itemList);
                }

                // 将册记录设到返回对象上
                result.itemList = allItemList;
                result.apiResult.errorCode = result.itemList.Count;
                return result;
            }
            finally
            {
                this.ChannelPool.ReturnChannel(channel);
            }
        }

        Hashtable _biblioXmlTable = new Hashtable(); // biblioRecPath --> xml

        // 将一条书目记录下属的若干册记录装入列表
        // return:
        //      -2  用户中断
        //      -1  出错
        //      >=0 装入的册记录条数
        int LoadBiblioSubItems(LibraryChannel channel,
            SessionInfo sessionInfo,
            string functionType,
            string strBiblioRecPath,
            out List<BiblioItem> itemList,
            out string strError)
        {
            strError = "";
            itemList = new List<BiblioItem>();

            // 如果是读过，加书目行
            if (functionType == "read")
            {
                BiblioItem biblio=GetBiblioLine(strBiblioRecPath);
                itemList.Add(biblio);
            }

            int nCount = 0;

            long lPerCount = 100; // 每批获得多少个
            long lStart = 0;
            long lResultCount = 0;
            long lCount = -1;
            for (; ; )
            {
                EntityInfo[] entities = null;

                long lRet = channel.GetEntities(
                     strBiblioRecPath,
                     lStart,
                     lCount,
                     "",  // bDisplayOtherLibraryItem == true ? "getotherlibraryitem" : "",
                     "zh",
                     out entities,
                     out strError);
                if (lRet == -1)
                    return -1;

                lResultCount = lRet;

                if (lRet == 0)
                {
                    //return nCount;
                    break;
                }
                Debug.Assert(entities != null, "");

                foreach (EntityInfo entity in entities)
                {
                    string strXml = entity.OldRecord;

                    XmlDocument dom = new XmlDocument();
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(strXml) == false)
                                dom.LoadXml(strXml);
                            else
                                dom.LoadXml("<root />");
                        }
                        catch (Exception ex)
                        {
                            strError = "XML 装入 DOM 出错: " + ex.Message;
                            return -1;
                        }
                    }
                    BiblioItem item = new BiblioItem();
                    item.backColor = "";

                    string strState = DomUtil.GetElementText(dom.DocumentElement, "state");
                    string strBorrower = DomUtil.GetElementText(dom.DocumentElement, "borrower");
                    if (functionType == "borrow")
                    {
                        // 在借的册、或者状态有值的需要显示为灰色
                        if (string.IsNullOrEmpty(strBorrower) == false
                            || string.IsNullOrEmpty(strState) == false)
                        {
                            item.isGray = true;
                        }
                    }
                    else if (functionType == "return")
                    {
                        // 没有在借的册需要显示为灰色
                        if (string.IsNullOrEmpty(strBorrower) == true)
                            item.isGray = true;

                        /*
                        if (string.IsNullOrEmpty(this.VerifyBorrower) == false)
                        {
                            // 验证还书时，不是要求的读者所借阅的册，显示为灰色
                            if (strBorrower != this.VerifyBorrower)
                                SetGrayText(row);
                        }
                         */
                    }
                    else if (functionType == "renew")
                    {
                        // 没有在借的册需要显示为灰色
                        if (string.IsNullOrEmpty(strBorrower) == true)
                            item.isGray = true;
                    }

                    string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");

                    // 状态
                    item.state = strState;

                    // 册条码号
                    if (string.IsNullOrEmpty(strBarcode) == false)
                        item.barcode = strBarcode;
                    else
                        item.barcode = "@refID:" + strRefID;

                    // 在借情况
                    string strReaderSummary = "";
                    item.readerSummaryStyle = " style=''";
                    if (string.IsNullOrEmpty(strBorrower) == false)
                    {
                        strReaderSummary = "";//todo this.MainForm.GetReaderSummary(strBorrower, false);
                        int nRet = this.GetPatronSummary(channel, strBorrower,
                            false,
                            out strReaderSummary,
                            out strError);
                        if (nRet == -1)
                            strReaderSummary = strError;

                        bool bError = (string.IsNullOrEmpty(strReaderSummary) == false && strReaderSummary[0] == '!');

                        string backColor = "";                        
                        if (bError == true)
                            backColor = "#B40000";//Color.FromArgb(180, 0, 0);
                        else
                        {
                            if (item.isGray == true)
                                backColor = "#DCDC00"; //Color.FromArgb(220, 220, 0);
                            else
                                backColor = "#B4B400"; //Color.FromArgb(180, 180, 0);
                        }

                        string strFont="";
                        if (bError == false)
                            strFont = "font-size: 20px;font-weight: bold;";//new System.Drawing.Font(this.dpTable_items.Font.FontFamily.Name, this.dpTable_items.Font.Size * 2, FontStyle.Bold);

                        string foreColor = "#FFFFFF";//Color.FromArgb(255, 255, 255);
                        // TODO: 后面还可加上借阅时间，应还时间

                        item.readerSummaryStyle = " style='text-align: center;white-space: nowrap;background-color:" + backColor + ";color:" + foreColor + ";" + strFont + "' ";
                    }
                    item.readerSummary=strReaderSummary;


                    // 书目摘要
                    string strSummary = "";
                    if (entity.ErrorCode != ErrorCodeValue.NoError)
                    {
                        strSummary = entity.ErrorInfo;
                    }
                    else
                    {
                        /*
                        int nRet = this.MainForm.GetBiblioSummary("@bibliorecpath:" + strBiblioRecPath,
                            "",
                            false,
                            out strSummary,
                            out strError);
                        if (nRet == -1)
                            strSummary = strError;
                         */
                        strSummary = "<label style='display:inline' >bs-@bibliorecpath:" + strBiblioRecPath+"</label>"
                                    +"<img src='~/img/wait2.gif' height='10' width='10' />"; 
                    }
                    item.summary = strSummary;

                    // 卷册
                    string strVolumn = DomUtil.GetElementText(dom.DocumentElement, "volumn");
                    item.volumn = strVolumn;

                    // 价格
                    string strPrice = DomUtil.GetElementText(dom.DocumentElement, "price");
                    item.price = strPrice;

                    // 册记录路径
                    item.oldRecPath = entity.OldRecPath;

                    // 地点
                    string strLocation = DomUtil.GetElementText(dom.DocumentElement, "location");
                    item.location = strLocation;
                     string itemLib = "";
                        string itemLibLevel2 = strLocation;
                        int nTempIndex = strLocation.IndexOf("/");
                        if (nTempIndex >= 0)
                        {
                            itemLib = strLocation.Substring(0, nTempIndex);
                            itemLibLevel2 = strLocation.Substring(nTempIndex + 1);
                        }

                    item.isManagetLoc = true;
                    // 检查一下当前操作用户是否有管理本馆书的权限
                    if (String.IsNullOrEmpty(sessionInfo.PersonalLibrary) ==false 
                        && sessionInfo.PersonalLibrary != "*")
                    {
                        // 分馆相同，且该书馆藏在读者管理的馆藏中
                        if (itemLib == sessionInfo.LibraryCode
                            && sessionInfo.PersonalLibrary.Contains(itemLibLevel2))
                        {
                            //这里可用的情况
                            item.isManagetLoc = true;
                        }
                        else
                        {
                            item.isManagetLoc =false ;
                        } 
                    }




                    // 在登录时，选定了馆藏
                   if (String.IsNullOrEmpty(sessionInfo.SelPerLib) == false)
                   {
                       if (itemLib == sessionInfo.LibraryCode
                            && sessionInfo.SelPerLib.Contains(itemLibLevel2))
                       {
                           itemList.Add(item);
                       }
                   }
                   else
                   {
                       itemList.Add(item);
                   }
                    nCount++;
                }

                lStart += entities.Length;
                if (lStart >= lResultCount)
                    break;

                if (lCount == -1)
                    lCount = lPerCount;

                if (lStart + lCount > lResultCount)
                    lCount = lResultCount - lStart;
            }

            /* 加一条横线
            if (lStart > 0)
            {
                DpRow row = new DpRow();
                row.Style = DpRowStyle.Seperator;
                this.dpTable_items.Rows.Add(row);
            }
             */

            // 本书目最后一行加画线标志
            if (itemList.Count > 0)
            {
                itemList[itemList.Count - 1].isAddLine = true;
            }

            return nCount;
        }



        // 生成书目行
        BiblioItem GetBiblioLine(string strBiblioRecPath)
        {
            string strError = "";

            string strVolume = "";
            string strPrice = "";

            GetVolume(strBiblioRecPath,
            out strVolume,
            out strPrice);

            // 背景LightGreen
            BiblioItem item = new BiblioItem();
            item.backColor = "LightGreen";

            // 状态
            item.state="";

            // 册条码号
            item.barcode = "@biblioRecPath:" + strBiblioRecPath;

            // 在借情况
            item.readerSummary="";

            // 书目摘要
            string strSummary = "<label style='display:inline' >bs-@bibliorecpath:" + strBiblioRecPath+"</label>"
                                    +"<img src='~/img/wait2.gif' height='10' width='10' />"; 
            item.summary = strSummary;

            // 卷册
            item.volumn = strVolume;

            // 地点
            item.location="";

            // 价格
            item.price=strPrice;

            // 册记录路径
            item.oldRecPath= strBiblioRecPath;

            return item;
        }

        void GetVolume(string strBiblioRecPath,
            out string strVolume,
            out string strPrice)
        {
            strVolume = "";
            strPrice = "";

            string strXml = (string)this._biblioXmlTable[strBiblioRecPath];
            if (string.IsNullOrEmpty(strXml) == true)
                return;

            string strOutMarcSyntax = "";
            string strMARC = "";
            string strError = "";
            int nRet = MarcUtil.Xml2Marc(strXml,
                false,
                "",
                out strOutMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == -1)
                return;
            if (string.IsNullOrEmpty(strMARC) == true)
                return;
            MarcRecord record = new MarcRecord(strMARC);
            if (strOutMarcSyntax == "unimarc")
            {
                string h = record.select("field[@name='200']/subfield[@name='h']").FirstContent;
                string i = record.select("field[@name='200']/subfield[@name='h']").FirstContent;
                if (string.IsNullOrEmpty(h) == false && string.IsNullOrEmpty(i) == false)
                    strVolume = h + " . " + i;
                else
                {
                    if (h == null)
                        h = "";
                    strVolume = h + i;
                }

                strPrice = record.select("field[@name='010']/subfield[@name='d']").FirstContent;
            }
            else if (strOutMarcSyntax == "usmarc")
            {
                string n = record.select("field[@name='200']/subfield[@name='n']").FirstContent;
                string p = record.select("field[@name='200']/subfield[@name='p']").FirstContent;
                if (string.IsNullOrEmpty(n) == false && string.IsNullOrEmpty(p) == false)
                    strVolume = n + " . " + p;
                else
                {
                    if (n == null)
                        n = "";
                    strVolume = n + p;
                }

                strPrice = record.select("field[@name='020']/subfield[@name='c']").FirstContent;
            }
            else
            {

            }

        }



        #region 静态函数

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





    }
}
