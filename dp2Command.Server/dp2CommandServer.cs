using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryRestClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace dp2Command.Server
{
    public class dp2CommandServer
    {
        //=================
        // 设为单一实例
        static dp2CommandServer _instance;
        private dp2CommandServer()
        {
            //Thread.Sleep(100); //假设多线程的时候因某种原因阻塞100毫秒
        }
        static object myObject = new object();
        static public dp2CommandServer Instance
        {
            get
            {
                lock (myObject)
                {
                    if (null == _instance)
                    {
                        _instance = new dp2CommandServer();
                    }
                    return _instance;
                }
            }
        }
        //===========


        // dp2服务器地址与代理账号
        public string dp2Url = "";//"http://dp2003.com/dp2library/rest/"; //"http://localhost:8001/dp2library/rest/";//
        public string dp2UserName = "";//"weixin";
        public string dp2Password = "";//"111111";

        // dp2weixin 
        public string dp2WeiXinUrl = "http://dp2003.com/dp2weixin";
        public string dp2WeiXinLogDir = "";
        

        // dp2通道池
        public LibraryChannelPool ChannelPool = null;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strDp2Url"></param>
        /// <param name="strDp2UserName"></param>
        /// <param name="strDp2Password"></param>
        /// <param name="strDp2WeiXinUrl"></param>
        /// <param name="strDp2WeiXinLogDir"></param>
        public void  Init(string strDp2Url,
            string strDp2UserName,
            string strDp2Password,
            string strDp2WeiXinUrl,
            string strDp2WeiXinLogDir)
        {
            this.dp2Url = strDp2Url;
            this.dp2UserName = strDp2UserName;
            this.dp2Password = strDp2Password;
            this.dp2WeiXinUrl = strDp2WeiXinUrl;
            this.dp2WeiXinLogDir = strDp2WeiXinLogDir;

            // 通道池对象
            ChannelPool = new LibraryChannelPool();
            ChannelPool.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            ChannelPool.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

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

            // 我这里赋上通道自己的账号，而不是使用全局变量。
            // 因为从池中征用通道后，都给通道设了密码。账号密码是通道的属性。
            LibraryChannel channel = sender as LibraryChannel;
            e.LibraryServerUrl = channel.Url;
            e.UserName = channel.UserName;
            e.Password = channel.Password;
        }

        #region 检索相关

        /// <summary>
        /// 检索书目
        /// </summary>
        /// <param name="strWord"></param>
        /// <returns></returns>
        public long SearchBiblio(string strWord,
            SearchCommand searchCmd,
            out string strFirstPage,
            out string strError)
        {
            strFirstPage = "";
            strError = "";

            // 判断检索词
            strWord = strWord.Trim();
            if (String.IsNullOrEmpty(strWord))
            {
                strError = "检索词不能为空。";
                return -1;
            }

            long lTotoalCount = 0;
            // 从池中征用通道
            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2Url, this.dp2UserName);
            channel.Password = this.dp2Password;
            try
            {
                // -1失败
                // 0 未命令
                long lRet = channel.SearchBiblio(strWord,
                    "",//全部途径
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    return lRet;
                }

                // 取出命中记录列表
                lTotoalCount = lRet;

                List<string> totalResultList = new List<string>();
                long lStart = 0;
                // 当前总共取的多少记录
                long lCurTotalCount = 0;

            REDO:
                List<string> resultPathList = null;
                long lCount = -1;
                lRet = channel.GetBiblioSearchResult(lStart,
                    lCount,
                     out resultPathList,
                     out strError);
                if (lRet == -1)
                    return -1;

                // 加到结果集中
                totalResultList.AddRange(resultPathList);

                // 检查记录是否获取完成，没取完继续取
                lCurTotalCount += lRet;
                if (lCurTotalCount < lTotoalCount)
                {
                    lStart = lCurTotalCount;
                    goto REDO;
                }


                // 检查一下，取出来的记录数，是否与返回的命中数量一致
                if (lTotoalCount != totalResultList.Count)
                {
                    strError = "内部错误，不可能结果集数量不一致";
                    return -1;
                }

                // 将检索结果信息保存到检索命令中
                searchCmd.BiblioResultPathList = totalResultList;
                searchCmd.ResultNextStart = 0;
                searchCmd.IsCanNextPage = true;

                // 获得第一页检索结果
                bool bRet = searchCmd.GetNextPage(out strFirstPage, out strError);
                if (bRet == false)
                {
                    return -1;
                }
            }
            finally
            {
                // 归还通道到池
                this.ChannelPool.ReturnChannel(channel);
            }

            return lTotoalCount;
        }


        /// <summary>
        /// 根据书目序号得到详细的参考信息
        /// </summary>
        /// <param name="nIndex">书目序号，从1排序</param>
        /// <param name="strInfo"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int GetDetailBiblioInfo(SearchCommand searchCmd,
            int  nIndex,
            out string strBiblioInfo,
            out string strError)
        {
            strBiblioInfo = "";
            strError = "";
            Debug.Assert(searchCmd != null);

            //检查有无超过数组界面
            if (nIndex <= 0 || searchCmd.BiblioResultPathList.Count < nIndex)
            {
                strError = "您输入的书目序号[" + nIndex.ToString() + "]越出范围。";
                return -1;
            }

            // 获取路径，注意要截取
            string strPath = searchCmd.BiblioResultPathList[nIndex - 1];
            int index = strPath.IndexOf("*");
            if (index > 0)
                strPath = strPath.Substring(0, index);
            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2Url, this.dp2UserName);
            channel.Password = this.dp2Password;
            try
            {
                long lRet1 = channel.GetBiblioDetail(strPath,
                    out strBiblioInfo,
                    out strError);
                if (lRet1 == -1)
                {
                    strError = "获取详细信息失败：" + strError;
                    return -1;
                }               
            }
            finally
            {
                this.ChannelPool.ReturnChannel(channel);
            }
            return 0;
        }

        /// <summary>
        /// 根据操作时间检索
        /// </summary>
        /// <param name="strWord"></param>
        /// <param name="searchCmd"></param>
        /// <param name="strFirstPage"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public long SearchBiblioByPublishtime(string strWord,
            SearchCommand searchCmd,
            out string strFirstPage,
            out string strError)
        {
            strFirstPage = "";
            strError = "";

            // 判断检索词
            strWord = strWord.Trim();
            if (String.IsNullOrEmpty(strWord))
            {
                strError = "检索词不能为空。";
                return -1;
            }

            long lTotoalCount = 0;
            // 从池中征用通道
            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2Url, this.dp2UserName);
            channel.Password = this.dp2Password;
            try
            {
                // -1失败
                // 0 未命令
                long lRet = channel.SearchBiblio(strWord,
                    "publishtime,_time,_freetime",
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    return lRet;
                }

                // 取出命中记录列表
                lTotoalCount = lRet;

                List<string> totalResultList = new List<string>();
                long lStart = 0;
                // 当前总共取的多少记录
                long lCurTotalCount = 0;

            REDO:
                List<string> resultPathList = null;
                long lCount = -1;
                lRet = channel.GetBiblioSearchResult(lStart,
                    lCount,
                     out resultPathList,
                     out strError);
                if (lRet == -1)
                    return -1;

                // 加到结果集中
                totalResultList.AddRange(resultPathList);

                // 检查记录是否获取完成，没取完继续取
                lCurTotalCount += lRet;
                if (lCurTotalCount < lTotoalCount)
                {
                    lStart = lCurTotalCount;
                    goto REDO;
                }


                // 检查一下，取出来的记录数，是否与返回的命中数量一致
                if (lTotoalCount != totalResultList.Count)
                {
                    strError = "内部错误，不可能结果集数量不一致";
                    return -1;
                }

                // 将检索结果信息保存到检索命令中
                searchCmd.BiblioResultPathList = totalResultList;
                searchCmd.ResultNextStart = 0;
                searchCmd.IsCanNextPage = true;

                // 获得第一页检索结果
                bool bRet = searchCmd.GetNextPage(out strFirstPage, out strError);
                if (bRet == false)
                {
                    return -1;
                }
            }
            finally
            {
                // 归还通道到池
                this.ChannelPool.ReturnChannel(channel);
            }

            return lTotoalCount;
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
        /// 0 读者证条码号或密码不正确
        /// 1 成功
        /// </returns>
        public int Binding(string strBarcode, 
            string strPassword, 
            string strWeiXinId,
            out string strReaderBarcode,
            out string strError)
        {
            strError = "";
            strReaderBarcode = "";

            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2Url, this.dp2UserName);
            channel.Password = this.dp2Password;
            try
            {
                // 检验用户名与密码                
                long lRet = channel.VerifyReaderPassword(strBarcode,
                   strPassword,
                    out strError);
                if (lRet == -1)
                {
                    strError = "读者证条码号或密码不正确。\n请重新输入'读者证条码号'（注:您也可以同时输入'读者证条码号'和'密码'，中间以/分隔，例如:R0000001/123）";
                    return 0;
                }

                if (lRet == 0)
                {
                    strError = "读者证条码号或密码不正确。\n请重新输入'读者证条码号'（注:您也可以同时输入'读者证条码号'和'密码'，中间以/分隔，例如:R0000001/123）";
                    return 0;
                }

                if (lRet == 1)
                {
                    // 进行绑定
                    // 先根据barcode检索出来,得到原记录与时间戳
                    GetReaderInfoResponse response = channel.GetReaderInfo(strBarcode,
                        "xml");
                    if (response.GetReaderInfoResult.Value != 1)
                    {
                        strError = "根据读者证条码号得到读者记录异常：" + response.GetReaderInfoResult.ErrorInfo;
                        return -1;
                    }
                    string strRecPath = response.strRecPath;
                    string strTimestamp = StringUtil.GetHexTimeStampString(response.baTimestamp);
                    string strXml = response.results[0];

                    // 改读者的email字段
                    XmlDocument readerDom = new XmlDocument();
                    readerDom.LoadXml(strXml);
                    XmlNode emailNode = readerDom.SelectSingleNode("//email");
                    if (emailNode == null)
                    {
                        emailNode = readerDom.CreateElement("email");
                        readerDom.DocumentElement.AppendChild(emailNode);
                    }
                    emailNode.InnerText = JoinEmail(emailNode.InnerText, strWeiXinId);
                    string strNewXml = ConvertXmlToString(readerDom);

                    // 更新到读者库
                    lRet = channel.SetReaderInfoForWeiXin(strRecPath,
                        strNewXml,
                        strTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "绑定出错：" + strError;
                        return -1;
                    }

                    // 绑定成功，把读者证条码记下来，用于续借 2015/11/7，不要用strbarcode变量，因为可能做的大小写转换
                    strReaderBarcode = DomUtil.GetNodeText(readerDom.DocumentElement.SelectSingleNode("barcode"));                    
                    return 1;

                }

                strError = "校验读者账号返回未知情况，返回值：" + lRet.ToString() + "-" + strError;
                return -1;
            }
            finally
            {
                this.ChannelPool.ReturnChannel(channel);
            }
        }

        /// <summary>
        /// 根据微信id检索读者
        /// </summary>
        /// <param name="strWeiXinId"></param>
        /// <param name="strRecPath"></param>
        /// <param name="strXml"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public long SearchReaderByWeiXinId(string strWeiXinId, out string strRecPath, out string strXml,
            out string strError)
        {
            strError = "";
            strRecPath = "";
            strXml = "";

            long lRet = 0;

            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2Url, this.dp2UserName);
            channel.Password = this.dp2Password;
            try
            {
                strWeiXinId = dp2CommandUtility.C_WeiXinIdPrefix + strWeiXinId;//weixinid:
                lRet = channel.SearchReaderByWeiXinId(strWeiXinId, out strError);
                if (lRet == -1)
                {
                    strError = "检索微信用户对应的读者出错:" + strError;
                    return -1;
                }
                else if (lRet > 1)
                {
                    strError = "检索微信用户对应的读者异常，得到" + lRet.ToString() + "条读者记录";
                    return -1;
                }
                else if (lRet == 0)
                {
                    strError = "根据微信id未找到对应读者。";
                    return 0;
                }
                else if (lRet == 1)
                {
                    lRet = channel.GetSearchResultForWeiXinUser(out strRecPath,
                         out strXml,
                         out strError);
                    if (lRet != 1)
                    {
                        strError = "获取结果集异常:" + strError;
                        return -1;
                    }
                }
            }
            finally
            {
                this.ChannelPool.ReturnChannel(channel);
            }

            return 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weiXinId"></param>
        /// <param name="strError"></param>
        /// <returns>
        /// -1 出错
        /// 0   本来就未绑定，不需解绑
        /// 1   解除绑定成功
        /// </returns>
        public int Unbinding(string strReaderBarcode, out string strError)
        {
            strError = "";

            /*
            // 检查微信用户是否绑定读者账号
            string strBarcode = "";
            long lRet = this.CheckIsBinding(strWeiXinId,out strBarcode, out strError);
            if (lRet == -1)
                return -1;
            // 未绑定
            if (lRet == 0)
            {
                strError = "您尚未绑定读者账号，不需要解除绑定。";
                return 0;
            }
            */

            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2Url, this.dp2UserName);
            channel.Password = this.dp2Password;
            try
            {
                // 得到原读者记录与时间戳
                GetReaderInfoResponse response = channel.GetReaderInfo(strReaderBarcode, "xml");
                if (response.GetReaderInfoResult.Value != 1)
                {
                    strError = "根据路径得到读者记录异常：" + response.GetReaderInfoResult.ErrorInfo;
                    return -1;
                }
                string strRecPath = response.strRecPath;
                string strTimestamp = StringUtil.GetHexTimeStampString(response.baTimestamp);
                string strXml = response.results[0];

                // 修改xml中的email字段，去掉weixin:***
                // 改为读者的email字段
                XmlDocument readerDom = new XmlDocument();
                readerDom.LoadXml(strXml);
                XmlNode emailNode = readerDom.SelectSingleNode("//email");
                string email = emailNode.InnerText.Trim();
                string strEmailLeft = email;
                string strEmailLRight = "";
                int nIndex = email.IndexOf(dp2CommandUtility.C_WeiXinIdPrefix);//"weixinid:");
                if (nIndex >= 0)
                {
                    strEmailLeft = email.Substring(0, nIndex);
                    string strOldWeixinId = email.Substring(nIndex);
                    nIndex = strOldWeixinId.IndexOf(',');
                    if (nIndex > 0)
                    {
                        strEmailLRight = strOldWeixinId.Substring(nIndex);
                        strOldWeixinId = strOldWeixinId.Substring(0, nIndex);
                    }
                    strEmailLeft = TrimComma(strEmailLeft);
                    strEmailLRight = TrimComma(strEmailLRight);
                }
                email = strEmailLeft;
                if (strEmailLRight != "")
                {
                    if (email != "")
                        email += ",";
                    email += strEmailLRight;
                }
                emailNode.InnerText = email;
                string strNewXml = ConvertXmlToString(readerDom);

                // 更新到读者库
                long lRet = channel.SetReaderInfoForWeiXin(strRecPath,
                    strNewXml,
                    strTimestamp,
                    out strError);
                if (lRet == -1)
                {
                    strError = "解除绑定出错：" + strError;
                    return -1;
                }

                //清掉内存的ReaderBarcode???
                //this.ReaderBarcode = "";

                return 1;
            }
            finally
            {
                this.ChannelPool.ReturnChannel(channel);
            }


        }

        #endregion


        #region 我的空间

        /// <summary>
        /// 续借
        /// </summary>
        /// <param name="strItemBarcode">册条码号</param>
        /// <returns></returns>
        public int Renew(string strReaderBarcode, string strItemBarcode, out BorrowInfo borrowInfo, out string strError)
        {
            borrowInfo = null;
            strError = "";

            if (strItemBarcode == null)
                strItemBarcode = "";
            strItemBarcode = strItemBarcode.Trim();
            if (strItemBarcode == "")
            {
                strError = "续借失败：您输入的续借图书编号或者册条码号为空。";
                return -1;
            }

            if (String.IsNullOrEmpty(strReaderBarcode )==true)
            {
                strError = "续借失败：内部错误，读者证条码号为空。";
                return -1;
            }

            /*
            // 优先从序号字典中找下
            if (this.CurrentMessageContext.BorrowDict.ContainsKey(strItemBarcode))
            {
                string temp = this.CurrentMessageContext.BorrowDict[strItemBarcode];
                if (temp != null && temp != "")
                    strItemBarcode = temp;
            }
            */

            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2Url, this.dp2UserName);
            channel.Password = this.dp2Password;
            try
            {
                long lRet = channel.Borrow(true,
                    strReaderBarcode,
                    strItemBarcode,
                    out borrowInfo,
                    out strError);
                if (lRet == -1)
                    return -1;

                return 1;

            }
            finally
            {
                this.ChannelPool.ReturnChannel(channel);
            }

        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="myinfo"></param>
        /// <param name="strError"></param>
        /// <returns>
        /// -1  出错
        /// 0   未绑定
        /// 1   成功
        /// </returns>
        public int GetMyInfo1(string strReaderBarcode, out string strMyInfo, out string strError)
        {
            strError = "";
            strMyInfo = "";

            Debug.Assert(String.IsNullOrEmpty(strReaderBarcode) == false);
            /*
            // 检查是否已经绑定读者账号
            long lRet = this.CheckIsBinding(strWeiXinId,out strError);
            if (lRet == -1)
                return -1;
            // 未绑定
            if (lRet == 0)
            {
                strError = "尚未绑定读者账号";
                return 0;
            }
            */
            // 得到高级xml
            string strXml = "";
            long lRet = this.GetReaderAdvanceXml(strReaderBarcode, out strXml,
                out strError);
            if (lRet == -1)
                return -1;

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
                + "姓名：" + strName + "\n"
                + "证条码号：" + strReaderBarcode + "\n"
                + "部门：" + strDepartment + "\n"
                + "联系方式：\n" + GetContactString(dom) + "\n"
                + "状态：" + strState + "\n"
                + "有效期：" + strCreateDate + "~" + strExpireDate + "\n"
                + "读者类别：" + strReaderType + "\n"
                + "注释：" + strComment;
            return 1;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="strMyInfo"></param>
        /// <param name="strError"></param>
        /// <returns>
        /// -1  出错
        /// 0   未绑定
        /// 1   成功
        /// </returns>
        public int GetBorrowInfo1(string strReaderBarcode, out string strBorrowInfo, out string strError)
        {
            strError = "";
            strBorrowInfo = "";

            /*
            // 检查是否已经绑定读者账号
            long lRet = this.CheckIsBinding(strWeiXinId, out strError);
            if (lRet == -1)
                return -1;
            // 未绑定
            if (lRet == 0)
            {
                strError = "尚未绑定读者账号";
                return 0;
            }
             */

            // 得到高级xml
            string strXml = "";
            long lRet = this.GetReaderAdvanceXml(strReaderBarcode, out strXml,
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
        /// 详细借阅信息
        /// </summary>
        /// <param name="strXml"></param>
        /// <param name="strBorrowInfo"></param>
        /// <returns></returns>
       private int GetBorrowsInfoInternal(string strXml, out string strBorrowInfo)
        {
            strBorrowInfo = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
            if (nodes.Count == 0)
            {
                strBorrowInfo= "无借阅记录";
                return 0;
            }            

            Dictionary<string, string> borrowLit = new Dictionary<string, string>();

            int index = 1;
            foreach (XmlElement borrow in nodes)
            {
                if (strBorrowInfo != "")
                    strBorrowInfo += "===============\n";

                string overdueText = "";
                string strIsOverdue = DomUtil.GetAttr(borrow, "isOverdue");
                if (strIsOverdue == "yes")
                    overdueText = DomUtil.GetAttr(borrow, "overdueInfo1");
                else
                    overdueText = "未超期";


                string itemBarcode = DomUtil.GetAttr(borrow, "barcode");
                borrowLit[index.ToString()] = itemBarcode; // 设到字典点，已变续借


                string bookName = DomUtil.GetAttr(borrow, "summary");//borrow.GetAttribute("summary")
                int tempIndex = bookName.IndexOf('/');
                if (tempIndex > 0)
                {
                    bookName = bookName.Substring(0, tempIndex);
                }

                strBorrowInfo += "编号：" + index.ToString() + "\n"
                    + "册条码号：" + itemBarcode + "\n"
                    + "书       名：" + bookName + "\n"
                    + "借阅时间：" + DateTimeUtil.ToLocalTime(borrow.GetAttribute("borrowDate"), "yyyy-MM-dd HH:mm") + "\n"
                    + "借       期：" + DateTimeUtil.GetDisplayTimePeriodString(borrow.GetAttribute("borrowPeriod")) + "\n"
                    + "应还时间：" + DateTimeUtil.ToLocalTime(borrow.GetAttribute("returningDate"), "yyyy-MM-dd") + "\n"
                    + "是否超期：" + overdueText+"\n";


                index++; //编号+1
            }

            // 设到用户上下文
            //this.CurrentMessageContext.BorrowDict = borrowLit;

            return nodes.Count;

        }


        /// <summary>
        /// 获取读者Advance Xml
        /// </summary>
        /// <param name="strRecPath"></param>
        /// <param name="strXml"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        private int GetReaderAdvanceXml(string strReaderBarcode, out string strXml, out string strError) 
        {
            strXml = "";
            strError = "";

            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2Url, this.dp2UserName);
            channel.Password = this.dp2Password;
            try
            {
                // 先根据barcode检索出来,得到原记录与时间戳
                GetReaderInfoResponse response = channel.GetReaderInfo(strReaderBarcode,//"@path:" + strRecPath,
                    "advancexml,advancexml_borrow_bibliosummary,advancexml_overdue_bibliosummary");
                if (response.GetReaderInfoResult.Value != 1)
                {
                    strError = "根据读者证条码号得到读者记录异常：" + response.GetReaderInfoResult.ErrorInfo;
                    return -1;
                }
                string strTimestamp = StringUtil.GetHexTimeStampString(response.baTimestamp);
                strXml = response.results[0];
                return 1;
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
                strEmail = JoinEmail(strEmail, "");
                list.Add(strEmail);
            }
            if (string.IsNullOrEmpty(strAddress) == false)
                list.Add(strAddress);
            return StringUtil.MakePathList(list, "; ");
        }

        #endregion

        #region 静态函数

        /// <summary>
        /// 拼email
        /// </summary>
        /// <param name="oldEmail"></param>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static string JoinEmail(string oldEmail, string openid)
        {
            string email = oldEmail.Trim();
            string strEmailLeft = email;
            string strEmailLRight = "";
            int nIndex = email.IndexOf(dp2CommandUtility.C_WeiXinIdPrefix);//"weixinid:");
            if (nIndex > 0)
            {
                strEmailLeft = email.Substring(0, nIndex);
                string strOldWeixinId = email.Substring(nIndex);
                nIndex = strOldWeixinId.IndexOf(',');
                if (nIndex > 0)
                {
                    strEmailLRight = strOldWeixinId.Substring(nIndex);
                    strOldWeixinId = strOldWeixinId.Substring(0, nIndex);
                }
                strEmailLeft = TrimComma(strEmailLeft);
                strEmailLRight = TrimComma(strEmailLRight);
            }
            email = strEmailLeft;
            if (strEmailLRight != "")
            {
                if (email != "")
                    email += ",";
                email += strEmailLRight;
            }

            if (openid != null && openid != "")
            {
                if (email != "")
                    email += ",";
                email += dp2CommandUtility.C_WeiXinIdPrefix + openid;// "weixinid:"
            }

            return email;
        }

        /// <summary>
        /// 去掉前后逗号
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static string TrimComma(string strText)
        {
            if (strText == null || strText.Length == 0)
                return strText;

            int nIndex = strText.LastIndexOf(',');
            if (nIndex > 0)
                strText = strText.Substring(0, nIndex);

            nIndex = strText.IndexOf(',');
            if (nIndex > 0)
                strText = strText.Substring(nIndex + 1);

            return strText;
        }

        /// <summary>
        /// 将XmlDocument转化为string
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public static string ConvertXmlToString(XmlDocument xmlDoc)
        {
            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, null);
            writer.Formatting = Formatting.Indented;
            xmlDoc.Save(writer);

            StreamReader sr = new StreamReader(stream, System.Text.Encoding.UTF8);
            stream.Position = 0;
            string xmlString = sr.ReadToEnd();
            sr.Close();
            stream.Close();

            return xmlString;
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
                    DateTime now = DateTime.Now;
                    // 每天一个日志文件
                    string strFilename = Path.Combine(this.dp2WeiXinLogDir, "log_" + DateTimeUtil.DateTimeToString8(now) + ".txt");
                    string strTime = now.ToString();
                    FileUtil.WriteText(strFilename,
                        strTime + " " + strText + "\r\n");
                }
            }
            catch (Exception ex)
            {
                EventLog Log = new EventLog();
                Log.Source = "dp2opac";
                Log.WriteEntry("因为原本要写入日志文件的操作发生异常， 所以不得不改为写入Windows系统日志(见后一条)。异常信息如下：'" + ExceptionUtil.GetDebugText(ex) + "'", EventLogEntryType.Error);
                Log.WriteEntry(strText, EventLogEntryType.Error);
            }
        }

        #endregion
    }
}
