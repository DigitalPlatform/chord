using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryRestClient;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dp2Command.Service
{
    public class dp2BaseCommandService
    {
        // 检索限制最大命中数常量
        public const int C_Search_MaxCount = 100;

        // 微信web程序url
        public string weiXinUrl = "";
        // 微信目录
        public string weiXinDataDir = "";

        // 访问的目标图书馆
        public string libCode = "";
        public string remoteUserName = "";

        #region 绑定解绑

        public virtual int Binding(string strBarcode,
            string strPassword,
            string strWeiXinId,
            out string strReaderBarcode,
            out string strError)
        {
            strReaderBarcode = "";
            strError = "未实现";
            return -1;

        }

        /// <returns>
        /// -1 出错
        /// 0   成功
        /// </returns>
        public virtual int Unbinding1(string strrBarcode,
            string strWeiXinId,
             out string strError)
        {
            strError = "未实现";

            return -1;
        }

        #endregion

        #region 根据微信id从远程库中查找对应读者

        public virtual long SearchOnePatronByWeiXinId(string strWeiXinId,
            out string strBarcode,
            out string strError)
        {
            strError = "未实现";
            strBarcode = "";


            return -1;
        }

        #endregion


        #region 检索书目

        /// <summary>
        /// 检索书目
        /// </summary>
        /// <param name="strWord"></param>
        /// <returns></returns>
        public virtual long SearchBiblio(string strWord,
            SearchCommand searchCmd,
            out string strFirstPage,
            out string strError)
        {
            strFirstPage = "";
            strError = "未实现";

            return -1;
        }

        public virtual int GetDetailBiblioInfo(SearchCommand searchCmd,
            int nIndex,
            out string strBiblioInfo,
            out string strError)
        {
            strBiblioInfo = "";
            strError = "未实现";
            Debug.Assert(searchCmd != null);

            return -1;
        }

        #endregion


        /// <returns>
        /// -1  出错
        /// 0   未找到读者记录
        /// 1   成功
        /// </returns>
        public virtual int GetBorrowInfo(string strReaderBarcode, 
            out string strBorrowInfo, 
            out string strError)
        {
            strBorrowInfo = "";
            strError = "未实现";
            return -1;
        }



        /// <returns>
        /// -1  出错
        /// 0   未绑定
        /// 1   成功
        /// </returns>
        public virtual int GetMyInfo(string strReaderBarcode,
            out string strMyInfo,
            out string strError)
        {
            strError = "未实现";
            strMyInfo = "";
            Debug.Assert(String.IsNullOrEmpty(strReaderBarcode) == false);
            return -1;
        }

        /// <summary>
        /// 得到的读者的联系方式
        /// </summary>
        /// <param name="dom"></param>
        /// <returns></returns>
        public string GetContactString(XmlDocument dom)
        {
            string strTel = DomUtil.GetElementText(dom.DocumentElement, "tel");
            string strEmail = DomUtil.GetElementText(dom.DocumentElement, "email");
            string strAddress = DomUtil.GetElementText(dom.DocumentElement, "address");
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(strTel) == false)
                list.Add(strTel);
            if (string.IsNullOrEmpty(strEmail) == false)
            {
                //strEmail = JoinEmail(strEmail, "");
                list.Add(strEmail);
            }
            if (string.IsNullOrEmpty(strAddress) == false)
                list.Add(strAddress);
            return StringUtil.MakePathList(list, "; ");
        }



        /// <summary>
        /// 续借
        /// </summary>
        /// <param name="strItemBarcode">册条码号</param>
        /// <returns></returns>
        public virtual int Renew(string strReaderBarcode,
            string strItemBarcode,
            out BorrowInfo borrowInfo,
            out string strError)
        {
            borrowInfo = null;
            strError = "未实现";

            return -1;
        }

        /// <summary>
        /// 详细借阅信息
        /// </summary>
        /// <param name="strXml"></param>
        /// <param name="strBorrowInfo"></param>
        /// <returns></returns>
        public int GetBorrowsInfoInternal(string strXml, out string strBorrowInfo)
        {
            strBorrowInfo = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

            /*
                <info>
        <item name="可借总册数" value="10" />
        <item name="日历名">
            <value>基本日历</value>
        </item>
        <item name="当前还可借" value="9" />
    </info>
''             
             */
            string maxBorrowCount = "";
            string curBorrowCount = "";
            XmlNode nodeMax = dom.DocumentElement.SelectSingleNode("info/item[@name='可借总册数']");
            if (nodeMax == null)
            {
                maxBorrowCount = "获取当前读者可借总册数出错：未找到对应节点。";
            }
            else
            {
                string maxCount = DomUtil.GetAttr(nodeMax, "value");
                if (maxCount == "")
                {
                    maxBorrowCount = "获取当前读者可借总册数出错：未设置对应值。";
                }
                else
                {
                    maxBorrowCount = "最多可借:" + maxCount; ;
                    XmlNode nodeCurrent = dom.DocumentElement.SelectSingleNode("info/item[@name='当前还可借']");
                    if (nodeCurrent == null)
                    {
                        curBorrowCount = "获取当前还可借出错：未找到对应节点。";
                    }
                    else
                    {
                        curBorrowCount = "当前可借:" + DomUtil.GetAttr(nodeCurrent, "value");
                    }
                }
            }

            strBorrowInfo = maxBorrowCount + " " + curBorrowCount + "\n";

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
            if (nodes.Count == 0)
            {
                strBorrowInfo += "无借阅记录";
                return 0;
            }

            Dictionary<string, string> borrowLit = new Dictionary<string, string>();
            int index = 1;
            string books = "";
            foreach (XmlElement borrow in nodes)
            {
                if (books != "")
                    books += "===============\n";

                string overdueText = "";
                string strIsOverdue = DomUtil.GetAttr(borrow, "isOverdue");
                if (strIsOverdue == "yes")
                    overdueText = DomUtil.GetAttr(borrow, "overdueInfo1");
                else
                    overdueText = "未超期";


                string itemBarcode = DomUtil.GetAttr(borrow, "barcode");
                borrowLit[index.ToString()] = itemBarcode; // 设到字典点，已变续借

                /*
                string bookName = DomUtil.GetAttr(borrow, "summary");//borrow.GetAttribute("summary")
                int tempIndex = bookName.IndexOf('/');
                if (tempIndex > 0)
                {
                    bookName = bookName.Substring(0, tempIndex);
                }
                 */
                string summary = DomUtil.GetAttr(borrow, "summary");
                books += "编号：" + index.ToString() + "\n"
                    + "册条码号：" + itemBarcode + "\n"
                    + "摘       要：" + summary + "\n"
                    + "借阅时间：" + DateTimeUtil.ToLocalTime(borrow.GetAttribute("borrowDate"), "yyyy-MM-dd HH:mm") + "\n"
                    + "借       期：" + DateTimeUtil.GetDisplayTimePeriodString(borrow.GetAttribute("borrowPeriod")) + "\n"
                    + "应还时间：" + DateTimeUtil.ToLocalTime(borrow.GetAttribute("returningDate"), "yyyy-MM-dd") + "\n"
                    + "是否超期：" + overdueText + "\n";


                index++; //编号+1
            }

            strBorrowInfo += books;

            // 设到用户上下文
            //this.CurrentMessageContext.BorrowDict = borrowLit;

            return nodes.Count;

        }

        #region 微信用户选择图书馆

        /// <summary>
        /// 检查微信用户是否已经选择了图书馆
        /// </summary>
        /// <param name="strWeiXinId"></param>
        /// <returns></returns>
        public WxUserItem CheckIsSelectLib(string strWeiXinId)
        {
            WxUserItem userItem = WxUserDatabase.Current.GetActive(strWeiXinId);
            if (userItem == null)
                return null;

            //记下来，以便点对点方便访问该图书馆
            this.libCode = userItem.libCode;
            this.remoteUserName = userItem.libUserName;

            return userItem;
        }

        /// <summary>
        /// 选择图书馆
        /// </summary>
        /// <param name="strWeiXinId"></param>
        /// <param name="libCode"></param>
        public WxUserItem SelectLib(string strWeiXinId, string libCode, string libUserName)
        {
            WxUserItem userItem = WxUserDatabase.Current.GetActiveOrFirst(strWeiXinId,libCode);
            if (userItem == null)
            {
                userItem = new WxUserItem();
                userItem.weixinId = strWeiXinId;
                userItem.libCode = libCode;
                userItem.libUserName = libUserName;
                userItem.readerBarcode = "";
                userItem.readerName = "";
                userItem.xml = "";
                userItem.refID = "";
                userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                userItem.updateTime = userItem.createTime;
                WxUserDatabase.Current.Add(userItem);
            }

            //设为当前活动状态
            WxUserDatabase.Current.SetActive(userItem);

            //记下来，以便点对点方便访问该图书馆
            this.libCode = libCode;
            this.remoteUserName = libUserName;

            return userItem;
        }

        #endregion

        #region 错误日志

        /// <summary>
        /// 日志锁
        /// </summary>
        static object logSyncRoot = new object();

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="strText"></param>
        public void WriteErrorLog(string strText)
        {
            try
            {
                lock (logSyncRoot)
                {
                    var logDir = this.weiXinDataDir + "/log";//Server.MapPath(string.Format("~/App_Data/log"));
                    if (!Directory.Exists(logDir))
                    {
                        Directory.CreateDirectory(logDir);
                    }

                    DateTime now = DateTime.Now;
                    // 每天一个日志文件
                    string strFilename = Path.Combine(logDir, "error.txt");
                    string strTime = now.ToString();
                    FileUtil.WriteText(strFilename, strTime + " " + strText + "\r\n");
                }
            }
            catch (Exception ex)
            {
                EventLog Log = new EventLog();
                Log.Source = "dp2weixin";
                Log.WriteEntry("因为原本要写入日志文件的操作发生异常， 所以不得不改为写入 Windows 日志(见后一条)。异常信息如下：'" + ExceptionUtil.GetDebugText(ex) + "'", EventLogEntryType.Error);
                Log.WriteEntry(strText, EventLogEntryType.Error);
            }
        }

        public void WriteInfoLog(string strText)
        {
            try
            {
                lock (logSyncRoot)
                {
                    var logDir = this.weiXinDataDir + "/log";//Server.MapPath(string.Format("~/App_Data/log"));
                    if (!Directory.Exists(logDir))
                    {
                        Directory.CreateDirectory(logDir);
                    }

                    DateTime now = DateTime.Now;
                    // 每天一个日志文件
                    string strFilename = Path.Combine(logDir, "info.txt");
                    string strTime = now.ToString();
                    FileUtil.WriteText(strFilename, strTime + " " + strText + "\r\n");
                }
            }
            catch (Exception ex)
            {
                EventLog Log = new EventLog();
                Log.Source = "dp2weixin";
                Log.WriteEntry("因为原本要写入日志文件的操作发生异常， 所以不得不改为写入 Windows 日志(见后一条)。异常信息如下：'" + ExceptionUtil.GetDebugText(ex) + "'", EventLogEntryType.Error);
                Log.WriteEntry(strText, EventLogEntryType.Error);
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
    }


}
