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

        // dp2通道池
        public LibraryChannelPool ChannelPool = null;

        // 背景图管理器
        public string TodayUrl="";

        public void Init(string strDp2LibraryUrl, string strDataDir)
        {
            this.dp2LibraryUrl = strDp2LibraryUrl;
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
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="strUserName"></param>
        /// <param name="strPassword"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public SessionInfo Login(string strUserName, string strPassword,
            out string rights,
            out string strError)
        {
            strError = "";
            rights = "";

            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2LibraryUrl, strUserName);
            channel.Password = strPassword;
            try
            {
                LoginResponse ret = channel.Login(strUserName, strPassword, "");
                if (ret.LoginResult.Value != 1)
                {
                    strError = ret.LoginResult.ErrorInfo;
                    return null;
                }

                SessionInfo sessionInfo = new SessionInfo();
                sessionInfo.UserName = strUserName;
                sessionInfo.Password = strPassword;
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
                patron.comment = DomUtil.GetElementText(dom.DocumentElement, "comment");

                // 赋给返回对象
                result.patron = patron;
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
            try
            {
                return channel.GetBiblioSummary(strItemBarcode);
            }
            finally
            {
                this.ChannelPool.ReturnChannel(channel);
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

        // 根据strPeriod中的时间单位(day/hour)，返回本地日期或者时间字符串
        // parameters:
        //      strPeriod   原始格式的时间长度字符串。也就是说，时间单位不和语言相关，是"day"或"hour"
        public static string LocalDateOrTime(DateTime time,
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
                return time.ToString("d");  // 精确到日

            return time.ToString("g");  // 精确到分钟。G精确到秒
            // http://www.java2s.com/Tutorial/CSharp/0260__Date-Time/UsetheToStringmethodtoconvertaDateTimetoastringdDfFgGmrstTuUy.htm
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
