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

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="strUserName"></param>
        /// <param name="strPassword"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public SessionInfo Login(string strUserName, string strPassword,
            bool bReader,
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
                if (bReader == true)
                    strParam = "type=reader";
                LoginResponse ret = channel.Login(strUserName, strPassword, strParam);
                if (ret.LoginResult.Value != 1)
                {
                    strError = ret.LoginResult.ErrorInfo;
                    return null;
                }

                SessionInfo sessionInfo = new SessionInfo();
                sessionInfo.UserName = strUserName;
                sessionInfo.Password = strPassword;
                sessionInfo.Parameters = strParam;
                sessionInfo.Rights = ret.strRights;
                sessionInfo.LibraryCode = ret.strLibraryCode;
                return sessionInfo;
            }
            catch (WebException wex)
            {
                strError = "访问dp2library服务器出错："+wex.Message+"\n请联系系统管理员修改dp2library服务器地址配置信息。";
                return null;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
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
                    result.apiResult.errorCode = -1;
                    result.apiResult.errorInfo = "异常：根据证条码号[" + strReaderBarcode + "]找到多条读者记录，请联系管理员。";
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
                // 先根据barcode检索出来,得到原记录与时间戳
                GetReaderInfoResponse response = channel.GetReaderInfo(strReaderBarcode,//"@path:" + strRecPath,
                   "advancexml");// "advancexml,advancexml_borrow_bibliosummary,advancexml_overdue_bibliosummary");
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
                    result.apiResult.errorCode = -1;
                    result.apiResult.errorInfo = "异常：根据证条码号[" + strReaderBarcode + "]找到多条读者记录，请联系管理员。";
                    return result;
                }
                string strXml = response.results[0];

                // 取出个人信息
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(strXml);
                string name = DomUtil.GetElementText(dom.DocumentElement, "name");
                string department = DomUtil.GetElementText(dom.DocumentElement, "department");

                if (name != "")
                {
                    result.summary = "<span style=' font-size: 14.8px;font-weight:bold'>" + name + "</span>";

                    if (department != "")
                        result.summary += "（" + department + "）";
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
#if NO
        /// <summary>
        /// 获得读者借阅信息
        /// </summary>
        /// <param name="sessionInfo"></param>
        /// <param name="strReaderBarcode"></param>
        /// <returns></returns>
        public BorrowInfoResult GetBorrowInfo(SessionInfo sessionInfo,
            string strReaderBarcode)
        {
            BorrowInfoResult result = new BorrowInfoResult();
            List<BorrowInfo> borrowList = new List<BorrowInfo>();
            result.borrowList = borrowList;
            result.apiResult = new ApiResult();
            if (sessionInfo == null)
            {
                result.apiResult.errorCode = -1;
                result.apiResult.errorInfo = "尚未登录";
                return result;
            }

            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2LibraryUrl, sessionInfo.UserName);
            channel.Password = sessionInfo.Password;
            try
            {
                // 先根据barcode检索出来,得到原记录与时间戳
                GetReaderInfoResponse response = channel.GetReaderInfo(strReaderBarcode,//"@path:" + strRecPath,
                   "advancexml,advancexml_borrow_bibliosummary,advancexml_overdue_bibliosummary");
                /// <para>-1:   出错</para>
                /// <para>0:    没有找到读者记录</para>
                /// <para>1:    找到读者记录</para>
                /// <para>&gt;>1:   找到多于一条读者记录，返回值是找到的记录数，这是一种不正常的情况</para>
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
                    result.apiResult.errorCode = -1;
                    result.apiResult.errorInfo = "异常：根据证条码号[" + strReaderBarcode + "]找到多条读者记录，请联系管理员。";
                    return result;
                }
                string strXml = response.results[0];

                // 取出个人信息
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(strXml);

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
                int borrowLineCount = nodes.Count;
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];

                    string strBarcode = DomUtil.GetAttr(node, "barcode");
                    string strRenewNo = DomUtil.GetAttr(node, "no");
                    string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                    string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                    string strOperator = DomUtil.GetAttr(node, "operator");
                    string strRenewComment = DomUtil.GetAttr(node, "renewComment");

                    string strOverDue = "";
                    bool bOverdue = false;  // 是否超期                   
                    strOverDue = DomUtil.GetAttr(node, "overdueInfo");
                    string strOverdue1 = DomUtil.GetAttr(node, "overdueInfo1");
                    string strIsOverdue = DomUtil.GetAttr(node, "isOverdue");
                    if (strIsOverdue == "yes")
                        bOverdue = true;

                    DateTime timeReturning = DateTime.MinValue;
                    string strTimeReturning = DomUtil.GetAttr(node, "timeReturning");
                    if (String.IsNullOrEmpty(strTimeReturning) == false)
                        timeReturning = DateTimeUtil.FromRfc1123DateTimeString(strTimeReturning).ToLocalTime();
                    string strReturnDate = LocalDateOrTime(timeReturning, strPeriod);

                    // 创建 borrowinfo对象，加到集合里
                    BorrowInfo borrowInfo = new BorrowInfo();
                    borrowInfo.barcode = strBarcode;
                    borrowInfo.renewNo = strRenewNo;
                    borrowInfo.borrowDate = LocalDateOrTime(strBorrowDate, strPeriod);// strBorrowDate;
                    borrowInfo.period = strPeriod;
                    borrowInfo.borrowOperator = strOperator;
                    borrowInfo.renewComment = strRenewComment;
                    borrowInfo.overdue = strOverDue;
                    borrowInfo.returnDate = strReturnDate;
                    borrowInfo.barcodeUrl = this.dp2OpacUrl + "/book.aspx?barcode="+strBarcode+"&borrower="+strReaderBarcode;
                    borrowList.Add(borrowInfo);
                }
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

            return result;
        }
#endif
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
