using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
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
using DigitalPlatform.Xml;

namespace dp2Command.Service
{
    public class dp2CommandService : dp2BaseCommandService
    {


        //=================
        // 设为单一实例
        static dp2CommandService _instance;
        private dp2CommandService()
        {
            //Thread.Sleep(100); //假设多线程的时候因某种原因阻塞100毫秒
        }
        static object myObject = new object();
        static public dp2CommandService Instance
        {
            get
            {
                lock (myObject)
                {
                    if (null == _instance)
                    {
                        _instance = new dp2CommandService();
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


        // dp2通道池
        public LibraryChannelPool ChannelPool = null;

        // 是否使用mongodb存储微信用户与读者关系
        private bool IsUseMongoDb = false;



        /// <summary>
        /// 
        /// </summary>
        /// <param name="strDp2Url"></param>
        /// <param name="strDp2UserName"></param>
        /// <param name="strDp2Password"></param>
        /// <param name="weiXinUrl"></param>
        /// <param name="weiXinLogDir"></param>
        /// <param name="isUseMongoDb"></param>
        /// <param name="mongoDbConnStr"></param>
        /// <param name="instancePrefix"></param>
        public void Init(string strDp2Url,
            string strDp2UserName,
            string strDp2Password,
            string weiXinUrl,
            string weiXinDataDir,
            bool isUseMongoDb,
            string mongoDbConnStr,
            string instancePrefix 
            )
        {
            this.dp2Url = strDp2Url;
            this.dp2UserName = strDp2UserName;
            this.dp2Password = strDp2Password;
            this.weiXinUrl = weiXinUrl;
            this.weiXinDataDir = weiXinDataDir;

            // 通道池对象
            ChannelPool = new LibraryChannelPool();
            ChannelPool.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            ChannelPool.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            // 使用mongodb存储微信用户与读者绑定关系
            this.IsUseMongoDb = isUseMongoDb;
            if (this.IsUseMongoDb == true)
            {
                WxUserDatabase.Current.Open(mongoDbConnStr, instancePrefix);
            }
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
            e.Parameters = "client=ilovelibrary|1.0"; //todo 写到这里合适吗
        }

        #region 检索相关

        /// <summary>
        /// 检索书目
        /// </summary>
        /// <param name="strWord"></param>
        /// <returns></returns>
        public override long SearchBiblio(string remoteUserName, 
            string strWord,
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
                /*
                         public long SearchBiblio(
            string strBiblioDbNames,
            string strQueryWord,
            int nPerMax,
            string strFromStyle,
            string strMatchStyle,
            string strResultSetName,
             string strOutputStyle,
            out string strError)
                 */
                // -1失败
                // 0 未命令
                string strQueryXml = "";
                long lRet = channel.SearchBiblio("",//全部途径
                    strWord,
                    C_Search_MaxCount,
                    "",
                    "middle",
                    "", // "weixin-biblio";  
                    "id,cols",
                    out strQueryXml,
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
        public override int GetDetailBiblioInfo(string remoteUserName, 
            SearchCommand searchCmd,
            int nIndex,
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
                /*
         public long SearchBiblio(
string strBiblioDbNames,
string strQueryWord,
int nPerMax,
string strFromStyle,
string strMatchStyle,
string strResultSetName,
string strOutputStyle,
out string strError)
 */
                // -1失败
                // 0 未命令
                string strQueryXml = "";
                long lRet = channel.SearchBiblio("",//全部途径
                    strWord,
                    C_Search_MaxCount,
                    "publishtime,_time,_freetime",
                    "left",
                    "",
                    "id,cols",
                    out strQueryXml,
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

            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2Url, this.dp2UserName);
            channel.Password = this.dp2Password;
            try
            {
                // 检验用户名与密码                
                long lRet = channel.VerifyReaderPassword(strFullWord,
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
                    GetReaderInfoResponse response = channel.GetReaderInfo(strFullWord,
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

                    // 将关系存到mongodb库
                    if (this.IsUseMongoDb == true)
                    {
                        //name
                        string name = "";
                        XmlNode node = readerDom.DocumentElement.SelectSingleNode("name");
                        if (node != null)
                            name = DomUtil.GetNodeText(node);

                        userItem = WxUserDatabase.Current.GetActiveOrFirst(strWeiXinId,libCode);
                        if (userItem == null)
                        {
                            userItem = new WxUserItem();
                            userItem.weixinId = strWeiXinId;
                            userItem.libCode = "";
                            userItem.libUserName = "";
                            userItem.readerBarcode = strReaderBarcode;
                            userItem.readerName = name;
                            userItem.xml = strXml;
                            userItem.refID = strRecPath;
                            userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                            userItem.updateTime = userItem.createTime;
                            WxUserDatabase.Current.Add(userItem);
                        }
                        else
                        {
                            userItem.readerBarcode = strReaderBarcode;
                            userItem.readerName = name;
                            userItem.xml = strXml;
                            userItem.refID = strRecPath;
                            userItem.updateTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                            lRet = WxUserDatabase.Current.Update(userItem);
                        }
                    }

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
        public override long SearchOnePatronByWeiXinId(string remoteUserName,
            string libCode, 
            string strWeiXinId,
            out string strBarcode,
            out string strError)
        {
            strError = "";
            strBarcode = "";

            long lRet = 0;

            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2Url, this.dp2UserName);
            channel.Password = this.dp2Password;
            try
            {
                strWeiXinId = dp2CommandUtility.C_WeiXinIdPrefix + strWeiXinId;//weixinid:

                // 先从mongodb查
                if (this.IsUseMongoDb == true)
                {

                    return 0;
                }


                lRet = channel.SearchReader("",
                strWeiXinId,
                -1,
                "email",
                "exact",
                "zh",
                "weixin",
                "keyid",
                out strError);
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
                    Record[] searchresults = null;
                    lRet = channel.GetSearchResult("weixin",
                        0,
                        -1,
                        "id,xml",
                        "zh",
                        out searchresults,
                        out strError);
                    if (searchresults.Length != 1)
                    {
                        throw new Exception("获得的记录数不是1");
                    }
                    if (lRet != 1)
                    {
                        strError = "获取结果集异常:" + strError;
                        return -1;
                    }

                    string strXml = searchresults[0].RecordBody.Xml;
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(strXml);
                    strBarcode = DomUtil.GetNodeText(dom.DocumentElement.SelectSingleNode("barcode"));
                    // 将关系存到mongodb库
                    if (this.IsUseMongoDb == true)
                    {
                        //name
                        string name = "";
                        XmlNode node = dom.DocumentElement.SelectSingleNode("name");
                        if (node != null)
                            name = DomUtil.GetNodeText(node);

                        WxUserItem userItem = new WxUserItem();
                        userItem.weixinId = strWeiXinId;
                        userItem.readerBarcode = strBarcode;
                        userItem.readerName = name;
                        userItem.libCode = "";
                        userItem.libUserName = "";
                        userItem.xml = strXml;
                        userItem.refID = searchresults[0].Path ;
                        userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                        userItem.updateTime = userItem.createTime;
                        WxUserDatabase.Current.Add(userItem);
                    }

                }
            }
            finally
            {
                this.ChannelPool.ReturnChannel(channel);
            }

            return 0;
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
        public override int Unbind(string remoteUserName,
            string libCode, 
            string strBarcode,
            string strWeiXinId,
            out string strError)
        {
            strError = "";

            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2Url, this.dp2UserName);
            channel.Password = this.dp2Password;
            try
            {
                // 得到原读者记录与时间戳
                GetReaderInfoResponse response = channel.GetReaderInfo(strBarcode, "xml");
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

                // 从mongodb删除
                if (this.IsUseMongoDb == true)
                {
                    //todo
                    //long nCount = WxUserDatabase.Current.Delete(strWeiXinId, strBarcode,libCode);
                }

                return 0;
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
        public override int Renew(string remoteUserName, 
            string strReaderBarcode, 
            string strItemBarcode, 
            out BorrowInfo borrowInfo, 
            out string strError)
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

            if (String.IsNullOrEmpty(strReaderBarcode) == true)
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

                string strOutputReaderBarcode = "";
                string strReaderXml = "";
                long lRet = channel.Borrow(true,
                    strReaderBarcode,
                    strItemBarcode,
                    out strOutputReaderBarcode,
                    out strReaderXml,
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
        /// 0   未找到读者记录
        /// 1   成功
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
        /// 0   未找到读者记录
        /// 1   成功
        /// </returns>
        public override int GetBorrowInfo(string remoteUserName, 
            string strReaderBarcode, out string strBorrowInfo, out string strError)
        {
            strError = "";
            strBorrowInfo = "";

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





    }
}
