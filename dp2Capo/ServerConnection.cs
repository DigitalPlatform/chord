// #define LOG
// #define VERIFY_CHUNK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;
using System.Collections;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.Message;
using DigitalPlatform.MessageClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.HTTP;

namespace dp2Capo
{
    /// <summary>
    /// 连接到 dp2mserver 的通讯通道
    /// </summary>
    public class ServerConnection : MessageConnection
    {
#if NO
        public string UserName { get; set; }
        public string Password { get; set; }
#endif
        public Instance Instance { get; set; }  // 方便访问 Instance

        public LibraryHostInfo dp2library { get; set; }
        internal LibraryChannelPool _channelPool = new LibraryChannelPool();

        public ServerConnection()
        {
            this._channelPool.BeforeLogin += new BeforeLoginEventHandle(_channelPool_BeforeLogin);
        }

        void _channelPool_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == true)
            {
                e.LibraryServerUrl = this.dp2library.Url;
                bool bIsReader = false;

                e.UserName = this.dp2library.UserName;

                e.Password = this.dp2library.Password;

                bIsReader = false;

                // e.Parameters = "location=" + strLocation;
                if (bIsReader == true)
                    e.Parameters += ",type=reader";

                // e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

                e.Parameters += ",client=dp2capo|" + "0.01";    // +Program.ClientVersion;

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            // TODO: 可以出现对话框，但要注意跨线程的问题
            // TODO: 当首次登录对话框没有输入密码的时候，这里就必须出现对话框询问密码了
            e.Cancel = true;
        }

        /// <summary>
        /// 向控制台输出纯文本
        /// </summary>
        /// <param name="strText">要输出的纯文本字符串</param>
        /// <param name="nWarningLevel">警告级别。0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)</param>
        public override void OutputText(string strText, int nWarningLevel = 0)
        {
            // Console.WriteLine(strText);  // 用于观察重新连接情况
        }

        public LibraryChannel GetChannel()
        {
            string strServerUrl = this.dp2library.Url;
            string strUserName = this.dp2library.UserName;

            return this._channelPool.GetChannel(strServerUrl, strUserName);
        }

        #region 针对 dp2library 服务器的一些功能

        // TODO: 可以返回 -2 表示为致命错误，永远不再重试
        // 利用 dp2library API 获取一些配置信息
        public int GetConfigInfo(out string strError)
        {
            strError = "";
            long lRet = 0;

            LibraryChannel channel = GetChannel();
            try
            {
                {
                    string strVersion = "";
                    string strUID = "";
                    lRet = channel.GetVersion(
        out strVersion,
        out strUID,
        out strError);
                    if (lRet == -1)
                        return -1;

                    if (string.IsNullOrEmpty(strUID) == true)
                    {
                        strError = "dp2Capo 所所连接的 dp2library 服务器 '" + channel.Url + "' 没有配置 图书馆 UID，无法被 dp2Capo 正常使用。请为 dp2library 增配 图书馆 UID";
                        return -1;
                    }

                    this.dp2library.LibraryUID = strUID;

                    // 检查 dp2library 版本
                    if (string.IsNullOrEmpty(strVersion) == true)
                        strVersion = "2.0";

                    string base_version = "2.80";   // 2.80 ChangeReaderPassword() API 做了修改 // 2.75 允许用 GetMessage() API 获取 MSMQ 消息
                    if (StringUtil.CompareVersion(strVersion, base_version) < 0)   // 2.12
                    {
                        strError = "dp2Capo 所所连接的 dp2library 服务器 '" + channel.Url + "' 其版本必须升级为 " + base_version + " 以上时才能使用 (当前 dp2library 版本为 " + strVersion + ")\r\n\r\n请立即升级 dp2Library 到最新版本";
                        return -1;
                    }
                }

                {
                    string strValue = "";
                    lRet = channel.GetSystemParameter(
                        "library",
                        "name",
                        out strValue,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    this.dp2library.LibraryName = strValue;
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = "GetConfigInfo() 出现异常: " + ExceptionUtil.GetExceptionText(ex);
                return -1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }
        }

        public int GetMsmqMessage(
            out MessageData[] messages,
            out string strError)
        {
            messages = null;
            strError = "";
            long lRet = 0;

            LibraryChannel channel = GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 2, 0);    // dp2library 服务器这个 API 是最多等待一分钟
            try
            {
                Hashtable table = new Hashtable();
                table["action"] = "get";
                table["count"] = "1";
                string strParameters = StringUtil.BuildParameterString(table);
                string[] message_ids = new string[] { "!msmq", strParameters };
                lRet = channel.GetMessage(message_ids,
                    MessageLevel.Full,
                    out messages,
                    out strError);
                if (lRet == -1)
                    return -1;

                return 0;
            }
            catch (Exception ex)
            {
                strError = "GetMsmqMessage() 出现异常: " + ExceptionUtil.GetExceptionText(ex);
                return -1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                this._channelPool.ReturnChannel(channel);
            }
        }

        public int RemoveMsmqMessage(int nCount,
    out string strError)
        {
            strError = "";
            long lRet = 0;

            if (nCount == 0)
                return 0;

            LibraryChannel channel = GetChannel();
            try
            {
                Hashtable table = new Hashtable();
                table["action"] = "remove";
                table["count"] = nCount.ToString();
                string strParameters = StringUtil.BuildParameterString(table);
                string[] message_ids = new string[] { "!msmq", strParameters };

                MessageData[] messages = null;
                lRet = channel.GetMessage(message_ids,
                    MessageLevel.Full,
                    out messages,
                    out strError);
                if (lRet == -1)
                    return -1;

                return 0;
            }
            catch (Exception ex)
            {
                strError = "RemoveMsmqMessage() 出现异常: " + ExceptionUtil.GetExceptionText(ex);
                return -1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }
        }

        #endregion

        #region GetRes() API

        public override void OnGetResRecieved(DigitalPlatform.Message.GetResRequest param)
        {
            Task.Run(() => GetResAndResponse(param));
        }

        // TODO: 比对每次获得的时间戳，如果不一致则要报错。
        void GetResAndResponse(DigitalPlatform.Message.GetResRequest param)
        {
            string strError = "";
            IList<string> results = new List<string>();
            long batch_size = 4 * 1024;    // 4K

            string strStyle = param.Style;
            if (StringUtil.IsInList("timestamp", strStyle) == false)
                StringUtil.SetInList(ref strStyle, "timestamp", true);  // 为了每次 GetRes() 以后比对

            LibraryChannel channel = GetChannel();
            try
            {
                long lTotalLength = 0;  // 对象的总长度
                long send = 0;  // 累计发送了多少 byte
                long chunk_size = -1;   // 每次从 dp2library 获取的分片大小
                long length = -1;    // 本次点对点 API 希望获得的长度

                if (param.Length != -1)
                {
                    chunk_size = Math.Min(100 * 1024, (int)param.Length);
                    length = param.Length;
                }

                byte[] timestamp = null;
                for (; ; )
                {
                    long lRet = 0;
                    byte[] baContent = null;
                    string strMetadata = "";
                    string strOutputResPath = "";
                    byte[] baOutputTimestamp = null;

                    if (param.Operation == "getRes")
                    {
                        Console.WriteLine("getRes() start=" + (param.Start + send)
                            + " length=" + chunk_size);
                        // Thread.Sleep(500);

                        lRet = channel.GetRes(param.Path,
                            param.Start + send,
                            (int)chunk_size, // (int)param.Length,
                            strStyle,   // param.Style,
                            out baContent,
                            out strMetadata,
                            out strOutputResPath,
                            out baOutputTimestamp,
                            out strError);
                    }
                    else
                    {
                        strError = "无法识别的 Operation 值 '" + param.Operation + "'";
                        goto ERROR1;
                    }

                    if (timestamp != null)
                    {
                        if (ByteArray.Compare(timestamp, baOutputTimestamp) != 0)
                        {
                            strError = "获取对象过程中发现时间戳发生了变化，本次获取操作无效";
                            goto ERROR1;
                        }
                    }

                    // 记忆下来供下一轮比对之用
                    timestamp = baOutputTimestamp;

                    lTotalLength = lRet;
                    if (length == -1)
                        length = lRet;

                    DigitalPlatform.Message.GetResResponse result = new DigitalPlatform.Message.GetResResponse();
                    result.TaskID = param.TaskID;
                    result.TotalLength = lRet;
                    result.Start = param.Start + send;
                    result.Path = strOutputResPath;
                    result.Data = baContent;
                    if (send == 0)
                    {
                        result.Metadata = strMetadata;
                        if (StringUtil.IsInList("timestamp", param.Style) == true)
                            result.Timestamp = ByteArray.GetHexTimeStampString(baOutputTimestamp);
                    }
                    result.ErrorInfo = strError;
                    result.ErrorCode = channel.ErrorCode == ErrorCode.NoError ? "" : channel.ErrorCode.ToString();

                    bool bRet = TryResponseGetRes(result,
        ref batch_size);
                    if (bRet == false
                        || result.Data == null || result.Data.Length == 0
                        || length == -1)
                        return;

                    if (param.Start + send >= length)
                        return;

                    send += result.Data.Length;

                    {
                        chunk_size = length - param.Start - send;
                        if (chunk_size <= 0)
                            return;
                        if (chunk_size >= Int32.MaxValue)
                            chunk_size = 100 * 1024;
                    }
                }
            }
            catch (Exception ex)
            {
                AddErrorLine("GetResAndResponse() 出现异常: " + ex.Message);
                strError = ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

        ERROR1:
            {
                // 报错
                DigitalPlatform.Message.GetResResponse result = new DigitalPlatform.Message.GetResResponse();
                result.TaskID = param.TaskID;
                result.TotalLength = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = channel.ErrorCode.ToString();

                ResponseGetRes(result);
            }
        }

        #endregion

        #region Circulation() API

        public override void OnCirculationRecieved(CirculationRequest param)
        {
            // 单独给一个线程来执行
            Task.Run(() => CirculationAndResponse(param));
        }

        static string GetItemBarcode(string strText)
        {
            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(strText, "|", out strLeft, out strRight);
            return strLeft;
        }

        static string GetItemConfirmPath(string strText)
        {
            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(strText, "|", out strLeft, out strRight);
            return strRight;
        }

        static DigitalPlatform.Message.BorrowInfo BuildBorrowInfo(DigitalPlatform.LibraryClient.localhost.BorrowInfo borrow_info)
        {
            if (borrow_info == null)
                return null;
            DigitalPlatform.Message.BorrowInfo info = new DigitalPlatform.Message.BorrowInfo();
            info.LatestReturnTime = borrow_info.LatestReturnTime;
            info.Period = borrow_info.Period;
            info.BorrowCount = borrow_info.BorrowCount;
            info.BorrowOperator = borrow_info.BorrowOperator;
            return info;
        }

        static DigitalPlatform.Message.ReturnInfo BuildReturnInfo(DigitalPlatform.LibraryClient.localhost.ReturnInfo return_info)
        {
            if (return_info == null)
                return null;
            DigitalPlatform.Message.ReturnInfo info = new DigitalPlatform.Message.ReturnInfo();
            info.BorrowTime = return_info.BorrowTime;
            info.LatestReturnTime = return_info.LatestReturnTime;
            info.Period = return_info.Period;
            info.BorrowCount = return_info.BorrowCount;
            info.OverdueString = return_info.OverdueString;
            info.BorrowOperator = return_info.BorrowOperator;
            info.ReturnOperator = return_info.ReturnOperator;
            info.BookType = return_info.BookType;
            info.Location = return_info.Location;
            return info;
        }

        void CirculationAndResponse(CirculationRequest param)
        {
            string strError = "";
            IList<string> results = new List<string>();

            LibraryChannel channel = GetChannel();
            try
            {
                long lRet = 0;
                string[] item_records = null;
                string[] reader_records = null;
                string[] biblio_records = null;
                string[] dup_paths = null;
                string strOutputReaderBarcode = "";
                DigitalPlatform.LibraryClient.localhost.BorrowInfo borrow_info = null;
                DigitalPlatform.LibraryClient.localhost.ReturnInfo return_info = null;

                string strStyle = param.Style;
                if (string.IsNullOrEmpty(param.PatronFormatList) == false)
                    StringUtil.SetInList(ref strStyle, "reader", true);
                if (string.IsNullOrEmpty(param.ItemFormatList) == false)
                    StringUtil.SetInList(ref strStyle, "item", true);
                if (string.IsNullOrEmpty(param.BiblioFormatList) == false)
                    StringUtil.SetInList(ref strStyle, "biblio", true);

                if (param.Operation == "borrow"
                    || param.Operation == "renew")
                {
                    lRet = channel.Borrow(param.Operation == "renew",
                        param.Patron,
                        GetItemBarcode(param.Item),
                        GetItemConfirmPath(param.Item),
                        false,
                        null,
                        strStyle,   // param.Style,
                        param.ItemFormatList,
                        out item_records,
                        param.PatronFormatList,
                        out reader_records,
                        param.BiblioFormatList,
                        out biblio_records,
                        out dup_paths,
                        out strOutputReaderBarcode,
                        out borrow_info,
                        out strError);
                }
                else if (param.Operation == "return"
                    || param.Operation == "lost"
                    || param.Operation == "inventory"
                    || param.Operation == "read")
                {
                    lRet = channel.Return(param.Operation,
                        param.Patron,
                        GetItemBarcode(param.Item),
                        GetItemConfirmPath(param.Item),
                        false,
                        strStyle,   // param.Style,
                        param.ItemFormatList,
                        out item_records,
                        param.PatronFormatList,
                        out reader_records,
                        param.BiblioFormatList,
                        out biblio_records,
                        out dup_paths,
                        out strOutputReaderBarcode,
                        out return_info,
                        out strError);
                }
                else if (param.Operation == "reservation")
                {
                    lRet = channel.Reservation(param.Style,
                        param.Patron,
                        param.Item,
                        out strError);
                }
                else if (param.Operation == "resetPassword")
                {
                    lRet = channel.ResetPassword(param.Patron,  // strPatameters
                        param.Item, // strMessageTemplate
                        out strOutputReaderBarcode,
                        out strError);
                }
                else if (param.Operation == "changePassword")
                {
                    Hashtable table = StringUtil.ParseParameters(param.Item, ',', '=', "url");
                    lRet = channel.ChangeReaderPassword(param.Patron,  // strPatameters
                        (string)table["old"],
                        (string)table["new"],
                        out strError);
                }
                else if (param.Operation == "verifyPassword")
                {
                    lRet = channel.VerifyReaderPassword(param.Patron,
                        param.Item,
                        out strError);
                }
                else
                {
                    strError = "无法识别的 Operation 值 '" + param.Operation + "'";
                    goto ERROR1;
                }

                CirculationResult circulation_result = new CirculationResult();
                circulation_result.Value = lRet;
                circulation_result.ErrorInfo = strError;
                circulation_result.String = channel.ErrorCode.ToString();
                circulation_result.DupPaths = dup_paths == null ? null : dup_paths.ToList();
                circulation_result.PatronResults = reader_records == null ? null : reader_records.ToList();
                circulation_result.ItemResults = item_records == null ? null : item_records.ToList();
                circulation_result.BiblioResults = biblio_records == null ? null : biblio_records.ToList();
                circulation_result.PatronBarcode = strOutputReaderBarcode;
                circulation_result.ReturnInfo = BuildReturnInfo(return_info);
                circulation_result.BorrowInfo = BuildBorrowInfo(borrow_info);

                TryResponseCirculation(param.TaskID,
    circulation_result);
                return;
            }
            catch (Exception ex)
            {
                AddErrorLine("CirculationAndResponse() 出现异常: " + ex.Message);
                strError = ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

        ERROR1:
            {
                // 报错
                CirculationResult circulation_result = new CirculationResult();
                circulation_result.Value = -1;
                circulation_result.ErrorInfo = strError;
                circulation_result.String = channel.ErrorCode.ToString();

                TryResponseCirculation(
    param.TaskID,
    circulation_result);
            }
        }

        #endregion

        #region SetInfo() API

        public override void OnSetInfoRecieved(SetInfoRequest param)
        {
            // 单独给一个线程来执行
            Task.Run(() => SetInfoAndResponse(param));
        }

        DigitalPlatform.LibraryClient.localhost.EntityInfo BuildEntityInfo(Entity entity)
        {
            DigitalPlatform.LibraryClient.localhost.EntityInfo info = new DigitalPlatform.LibraryClient.localhost.EntityInfo();
            info.Action = entity.Action;
            info.RefID = entity.RefID;

            if (entity.OldRecord != null)
            {
                info.OldRecord = entity.OldRecord.Data;
                info.OldRecPath = entity.OldRecord.RecPath;
                info.OldTimestamp = ByteArray.GetTimeStampByteArray(entity.OldRecord.Timestamp);
            }

            if (entity.NewRecord != null)
            {
                info.NewRecord = entity.NewRecord.Data;
                info.OldRecPath = entity.NewRecord.RecPath;
                info.OldTimestamp = ByteArray.GetTimeStampByteArray(entity.NewRecord.Timestamp);
            }

            info.Style = entity.Style;

            info.ErrorInfo = entity.ErrorInfo;

            info.ErrorCode = DigitalPlatform.LibraryClient.localhost.ErrorCodeValue.NoError;

            return info;
        }

        Entity BuildEntity(DigitalPlatform.LibraryClient.localhost.EntityInfo info)
        {
            Entity entity = new Entity();
            entity.Action = info.Action;
            entity.RefID = info.RefID;

            entity.OldRecord = new DigitalPlatform.Message.Record();
            entity.OldRecord.Data = info.OldRecord;
            entity.OldRecord.RecPath = info.OldRecPath;
            entity.OldRecord.Timestamp = ByteArray.GetHexTimeStampString(info.OldTimestamp);

            entity.NewRecord = new DigitalPlatform.Message.Record();
            entity.NewRecord.Data = info.NewRecord;
            entity.NewRecord.RecPath = info.NewRecPath;
            entity.NewRecord.Timestamp = ByteArray.GetHexTimeStampString(info.NewTimestamp);

            entity.Style = info.Style;
            entity.ErrorInfo = info.ErrorInfo;
            entity.ErrorCode = info.ErrorCode.ToString();
            return entity;
        }

        void SetInfoAndResponse(SetInfoRequest param)
        {
            string strError = "";
            IList<Entity> results = new List<Entity>();

            List<DigitalPlatform.LibraryClient.localhost.EntityInfo> entities = new List<DigitalPlatform.LibraryClient.localhost.EntityInfo>();
            foreach (Entity entity in param.Entities)
            {
                entities.Add(BuildEntityInfo(entity));
            }

            LibraryChannel channel = GetChannel();
            try
            {
                DigitalPlatform.LibraryClient.localhost.EntityInfo[] errorinfos = null;
                long lRet = channel.SetEntities(param.BiblioRecPath,
                    entities.ToArray(),
                    out errorinfos,
                    out strError);
                if (errorinfos != null)
                {
                    foreach (DigitalPlatform.LibraryClient.localhost.EntityInfo error in errorinfos)
                    {
                        results.Add(BuildEntity(error));
                    }
                }
                ResponseSetInfo(param.TaskID,
    lRet,
    results,
    strError);
                return;
            }
            catch (Exception ex)
            {
                AddErrorLine("SetInfoAndResponse() 出现异常: " + ex.Message);
                strError = ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

        ERROR1:
            // 报错
            TryResponseSetInfo(
param.TaskID,
-1,
results,
strError);
        }

        #endregion

        #region BindPatron() API

        public override void OnBindPatronRecieved(DigitalPlatform.Message.BindPatronRequest param)
        {
            // 单独给一个线程来执行
            Task.Run(() => BindPatronAndResponse(param));
        }

        void BindPatronAndResponse(DigitalPlatform.Message.BindPatronRequest param)
        {
            string strError = "";
            IList<string> results = new List<string>();

            LibraryChannel channel = GetChannel();
            try
            {
                string[] temp_results = null;
                long lRet = channel.BindPatron(param.Action,
                    param.QueryWord,
                    param.Password,
                    param.BindingID,
                    param.Style,
                    param.ResultTypeList,
                    out temp_results,
                    out strError);
                if (temp_results != null)
                {
                    foreach (string s in temp_results)
                    {
                        results.Add(s);
                    }
                }
                ResponseBindPatron(param.TaskID,
    lRet,
    results,
    strError);
                return;
            }
            catch (Exception ex)
            {
                AddErrorLine("BindPatronAndResponse() 出现异常: " + ex.Message);
                strError = ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

        ERROR1:
            // 报错
            TryResponseBindPatron(
param.TaskID,
-1,
results,
strError);
        }

        #endregion

        #region WebCall() API

        WebDataTable _webDataTable = new WebDataTable();

        public void CleanWebDataTable()
        {
            _webDataTable.Clean(TimeSpan.FromMinutes(10));
        }

        // 当 server 发来 WebCall 请求的时候被调用。重载的时候要对目标发送 HTTP 请求，获得 HTTP 响应，并调用 responseWebCall 把响应结果发送给 server
        public override void OnWebCallRecieved(WebCallRequest param)
        {
            // 累积数据，当数据完整后，执行请求和获得响应
            _webDataTable.AddData(param.TaskID, param.WebData);
            if (param.Complete == true)
            {
                WebData data = _webDataTable.GetData(param.TaskID);

                Task.Run(() => WebCallAndResponse(param.TaskID, param.TransferEncoding, data));
            }
        }

        // 判断特征是否匹配 url。算法是取 url 右边部分和 filter 进行比较
        static bool MatchUrl(string url, string filter)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(filter))
                return false;

            if (url.Length < filter.Length)
                return false;

            // 右边加上一个 '/'
            if (url[url.Length - 1] != '/')
                url += "/";

            if (filter[filter.Length - 1] != '/')
                filter += "/";

            if (url.EndsWith(filter) && url[url.Length - filter.Length - 1] == '/')
                return true;

            return false;
        }

        // 将 rest/verb 剖析为两个部分 rest 和 /verb 。注意，是从右边开始找第一个 '/'
        static List<string> SplitVerb(string text)
        {
            List<string> results = new List<string>();
            int nRet = text.LastIndexOf("/");
            if (nRet == -1)
            {
                results.Add(text);
                results.Add("");
                return results;
            }

            results.Add(text.Substring(0, nRet));
            results.Add(text.Substring(nRet));
            return results;
        }

        // 从若干 URL 中根据 filter 提供的特征选择一个 URL
        static string GetTargetUrl(string urls, string filter)
        {
            if (string.IsNullOrEmpty(filter) == false
                && filter[0] == '/')
                filter = filter.Substring(1);

            {
                // 只取第二级(以及以下全部内容)作为筛选特征
                List<string> parts = StringUtil.ParseTwoPart(filter, "/");
                filter = parts[1];
            }

            string verb = "";
            // rest 要特殊处理
            if (filter.StartsWith("rest/"))
            {
                List<string> parts = SplitVerb(filter);
                filter = parts[0];
                verb = parts[1];
            }

            string[] list = urls.Split(new char[] { ';' });
            if (string.IsNullOrEmpty(filter))
                return list[0] + verb;

            foreach (string url in list)
            {
                if (MatchUrl(url, filter))
                    return url + verb;
            }

            return "";
        }

        void WebCallAndResponse(string taskID,
            string transferEncoding,
            WebData request_data)
        {
            string strError = "";

            try
            {
                if (string.IsNullOrEmpty(this.dp2library.WebUrl) == true)
                {
                    // 报错
                    strError = "dp2Capo 中尚未为 dp2library 配置 webURL 参数";
                    goto ERROR1;
                }

                HttpRequest request = MessageUtility.BuildHttpRequest(request_data, transferEncoding);

                // Console.WriteLine("request=" + request.Dump());

                // this.dp2library.WebUrl 可以是若干个 URL，需要用 request.Url 的第二级来进行匹配
                string url = GetTargetUrl(this.dp2library.WebUrl, request.Url);

                if (string.IsNullOrEmpty(url))
                {
                    strError = "dp2Capo 的 webURL 定义 '" + this.dp2library.WebUrl + "' 无法匹配上原始请求的 '" + request.Url + "'";
                    goto ERROR1;
                }

                request_data = null;    // 防止后面无意用到

                HttpResponse response = HttpProcessor.WebCall(request, url);

                /*
                // 调试用
                if (response.StatusCode != "200")
                {
                    Console.WriteLine("request=" + request.Dump(true));
                    Console.WriteLine("response=" + response.Dump());
                }
                 * */

                WebData response_data = MessageUtility.BuildWebData(response, transferEncoding);

                // 分批发出
                WebDataSplitter splitter = new WebDataSplitter();
                splitter.ChunkSize = MessageUtil.BINARY_CHUNK_SIZE;
                splitter.TransferEncoding = transferEncoding;
                splitter.WebData = response_data;

                foreach (WebData current in splitter)
                {
                    WebCallResponse param = new WebCallResponse();
                    param.TaskID = taskID;
                    param.WebData = current;
                    param.Complete = splitter.LastOne;

                    MessageResult result = ResponseWebCallAsync(param).Result;
                    if (result.Value == -1)
                        break;
                }
            }
            catch (Exception ex)
            {
                strError = "WebCallAndResponse() 出现异常: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                _webDataTable.RemoveData(taskID);
            }

        ERROR1:
            {
                WebCallResponse param = new WebCallResponse();
                param.TaskID = taskID;
                param.Complete = true;
                param.Result = new MessageResult();
                param.Result.Value = -1;
                param.Result.ErrorInfo = strError;

                TryResponseWebCall(param);
            }
        }

        #endregion

        #region Search() API

        // 
        // 当 server 发来检索请求的时候被调用。重载的时候要进行检索，并调用 Response 把检索结果发送给 server
        public override void OnSearchRecieved(SearchRequest param)
        {
            // 单独给一个线程来执行
            // Task.Factory.StartNew(() => SearchAndResponse(param));
            Task.Run(() => SearchAndResponse(param));
        }

        void writeDebug(string strText)
        {
            // this.Instance.WriteErrorLog("debug: " + strText);
        }

        // TODO: 本函数最好放在一个工作线程内执行
        // Form Close 的时候要及时中断工作线程
        // getUserInfo getPatronInfo getBiblioInfo getBiblioSummary searchBiblio searchPatron
        void SearchAndResponse(SearchRequest searchParam)
        {
#if LOG
            writeDebug("searchParam=" + searchParam.Dump());
#endif

            if (searchParam.Operation == "getPatronInfo")
            {
                GetPatronInfo(searchParam);
                return;
            }

            if (searchParam.Operation == "getSystemParameter")
            {
                GetSystemParameter(searchParam);
                return;
            }

            if (searchParam.Operation == "getUserInfo")
            {
                GetUserInfo(searchParam);
                return;
            }

            if (searchParam.Operation == "getBiblioInfo")
            {
                GetBiblioInfo(searchParam);
                return;
            }

            if (searchParam.Operation == "getBiblioSummary")
            {
                GetBiblioSummary(searchParam);
                return;
            }

            if (searchParam.Operation == "getItemInfo")
            {
                GetItemInfo(searchParam);
                return;
            }

            if (searchParam.Operation == "getBrowseRecords")
            {
                GetBrowseRecords(searchParam);
                return;
            }

            string strError = "";
            string strErrorCode = "";
            IList<DigitalPlatform.Message.Record> records = new List<DigitalPlatform.Message.Record>();

            string strResultSetName = searchParam.ResultSetName;
            if (string.IsNullOrEmpty(strResultSetName) == true)
                strResultSetName = "default";  // "#" + searchParam.TaskID;    // "default";
            else
                strResultSetName = "#" + strResultSetName;  // 如果请求方指定了结果集名，则在 dp2library 中处理为全局结果集名

            LibraryChannel channel = GetChannel();
            try
            {
                string strQueryXml = "";
                long lRet = 0;

                if (searchParam.QueryWord == "!getResult")
                {
                    lRet = -1;
                }
                else
                {
                    if (searchParam.Operation == "searchBiblio")
                    {
                        lRet = channel.SearchBiblio(// null,
                             searchParam.DbNameList,
                             searchParam.QueryWord,
                             (int)searchParam.MaxResults,
                             searchParam.UseList,
                             searchParam.MatchStyle,
                             "zh",
                             strResultSetName,
                             "", // strSearchStyle
                             "", // strOutputStyle
                             "",
                             out strQueryXml,
                             out strError);
                        writeDebug("searchBiblio() lRet=" + lRet
                            + ", strQueryXml=" + strQueryXml
                            + ", strError=" + strError
                            + ",errorcode=" + channel.ErrorCode.ToString());
                    }
                    else if (searchParam.Operation == "searchPatron")
                    {
                        lRet = channel.SearchReader(// null,
                            searchParam.DbNameList,
                            searchParam.QueryWord,
                            (int)searchParam.MaxResults,
                            searchParam.UseList,
                            searchParam.MatchStyle,
                            "zh",
                            strResultSetName,
                            "",
                            out strError);
                    }
                    else
                    {
                        lRet = -1;
                        strError = "无法识别的 Operation 值 '" + searchParam.Operation + "'";
                    }

                    strErrorCode = channel.ErrorCode.ToString();

                    if (lRet == -1 || lRet == 0)
                    {
                        if (lRet == 0
                            || (lRet == -1 && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound))
                        {
                            // 没有命中
                            TryResponseSearch(
                                new SearchResponse(
    searchParam.TaskID,
    0,
    0,
    this.dp2library.LibraryUID,
    records,
    strError,  // 出错信息大概为 not found。
    strErrorCode));
                            return;
                        }
                        goto ERROR1;
                    }
                }


                {
                    long lHitCount = lRet;

                    if (searchParam.Count == 0)
                    {
                        // 返回命中数
                        TryResponseSearch(
                            new SearchResponse(
                            searchParam.TaskID,
                            lHitCount,
0,
this.dp2library.LibraryUID,
records,
"本次没有返回任何记录",
strErrorCode));
                        return;
                    }

                    LibraryChannel temp_channel = channel;
                    channel = null;
                    Task.Run(() => SendResults(
                        temp_channel,
                        searchParam,
                    strResultSetName,
                    lHitCount));
                }
            }
            catch (Exception ex)
            {
                AddErrorLine("SearchAndResponse() 出现异常: " + ex.Message);
                strError = ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                if (channel != null)
                    this._channelPool.ReturnChannel(channel);
            }

            this.AddInfoLine("search and response end");
            return;
        ERROR1:
            // 报错
            TryResponseSearch(
                new SearchResponse(
searchParam.TaskID,
-1,
0,
this.dp2library.LibraryUID,
records,
strError,
strErrorCode));
        }

        void SendResults(
            LibraryChannel channel,
            SearchRequest searchParam,
            string strResultSetName,
            long lHitCount)
        {
            string strError = "";
            string strErrorCode = "";
            IList<DigitalPlatform.Message.Record> records = new List<DigitalPlatform.Message.Record>();

            // LibraryChannel channel = GetChannel();
            try
            {
                long batch_size = 100;

                long lStart = searchParam.Start;
                long lPerCount = searchParam.Count; // 本次拟返回的个数

                if (lHitCount != -1)
                {
                    if (lPerCount == -1)
                        lPerCount = lHitCount - lStart;
                    else
                        lPerCount = Math.Min(lPerCount, lHitCount - lStart);

                    if (lPerCount <= 0)
                    {
                        strError = "命中结果总数为 " + lHitCount + "，取结果开始位置为 " + lStart + "，它已超出结果集范围";
                        goto ERROR1;
                    }
                }


                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    string strBrowseStyle = searchParam.FormatList; // "id,xml";

                    long lRet = channel.GetSearchResult(
                        // null,
        strResultSetName,
        lStart,
        lPerCount,
        strBrowseStyle,
        "zh", // this.Lang,
        out searchresults,
        out strError);
                    strErrorCode = channel.ErrorCode.ToString();
                    writeDebug("GetSearchResult lRet=" + lRet + ", strResultSetName=" + strResultSetName
                        + ",lStart=" + lStart
                        + ",lPerCount=" + lPerCount
                        + ",strBrowseStyle=" + strBrowseStyle
                        + ", strError=" + strError
                        + ",errorcode=" + strErrorCode);

                    if (lRet == -1)
                        goto ERROR1;

                    if (searchresults.Length == 0)
                    {
                        strError = "GetSearchResult() searchResult empty";
                        goto ERROR1;
                    }

                    if (lHitCount == -1)
                        lHitCount = lRet;   // 延迟得到命中总数

                    records.Clear();
                    foreach (DigitalPlatform.LibraryClient.localhost.Record record in searchresults)
                    {
#if NO
                            DigitalPlatform.Message.Record biblio = new DigitalPlatform.Message.Record();
                            biblio.RecPath = record.Path;
                            biblio.Data = record.RecordBody.Xml;
                            records.Add(biblio);
#endif
                        DigitalPlatform.Message.Record biblio = FillBiblio(record);
                        records.Add(biblio);
                    }

#if NO
                        ResponseSearch(
                            searchParam.TaskID,
                            lHitCount,
                            lStart,
                            records,
                            "",
                            strErrorCode);
#endif
                    bool bRet = TryResponseSearch(
    searchParam.TaskID,
    lHitCount,
    lStart,
    this.dp2library.LibraryUID, // libraryUID,
    records,
    "",
    strErrorCode,
    ref batch_size);
                    Console.WriteLine("ResponseSearch called " + records.Count.ToString() + ", bRet=" + bRet);
                    if (bRet == false)
                        return;

                    Debug.Assert(searchresults.Length == records.Count, "");

                    lStart += searchresults.Length;

                    if (lPerCount != -1)
                        lPerCount -= searchresults.Length;

                    if (lStart >= lHitCount || (lPerCount <= 0 && lPerCount != -1))
                        break;
                }
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

            return;
        ERROR1:
            // 报错
            TryResponseSearch(
new SearchResponse(
searchParam.TaskID,
-1,
0,
this.dp2library.LibraryUID,
records,
strError,
strErrorCode));
        }

        void GetBrowseRecords(SearchRequest searchParam)
        {
            string strError = "";
            string strErrorCode = "";
            IList<DigitalPlatform.Message.Record> records = new List<DigitalPlatform.Message.Record>();

            LibraryChannel channel = GetChannel();
            try
            {
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;
                // return:
                //      result.Value    -1 出错；0 成功
                long lRet = channel.GetBrowseRecords(
                    searchParam.QueryWord.Split(new char[] { ',' }),
                    searchParam.FormatList,
                    out searchresults,
                    out strError);
                strErrorCode = channel.ErrorCode.ToString();
                if (lRet == -1)
                {
                    if (lRet == 0
                        || (lRet == -1 && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound))
                    {
                        // 没有命中
                        TryResponseSearch(
                                                            new SearchResponse(
searchParam.TaskID,
0,
0,
this.dp2library.LibraryUID,
records,
strError,   // 出错信息大概为 not found。
strErrorCode));
                        return;
                    }
                    goto ERROR1;
                }

                records.Clear();

                // TODO: 根据 format list 选择返回哪些信息

                foreach (DigitalPlatform.LibraryClient.localhost.Record record in searchresults)
                {
                    DigitalPlatform.Message.Record biblio = FillBiblio(record);
                    records.Add(biblio);
                }

#if NO
                ResponseSearch(
                    searchParam.TaskID,
                    records.Count,  // lHitCount,
                    0, // lStart,
                    records,
                    "",
                    strErrorCode);
#endif
                long batch_size = -1;
                bool bRet = TryResponseSearch(
searchParam.TaskID,
records.Count,
0,
this.dp2library.LibraryUID,
records,
"",
strErrorCode,
ref batch_size);
                Console.WriteLine("ResponseSearch called " + records.Count.ToString() + ", bRet=" + bRet);
                if (bRet == false)
                    return;
            }
            catch (Exception ex)
            {
                AddErrorLine("GetBrowseRecords() 出现异常: " + ex.Message);
                strError = "GetBrowseRecords() 异常：" + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

            this.AddInfoLine("search and response end");
            return;
        ERROR1:
            // 报错
            TryResponseSearch(
                                                new SearchResponse(
searchParam.TaskID,
-1,
0,
this.dp2library.LibraryUID,
records,
strError,
strErrorCode));
        }

        static DigitalPlatform.Message.Record FillBiblio(DigitalPlatform.LibraryClient.localhost.Record record)
        {
            DigitalPlatform.Message.Record biblio = new DigitalPlatform.Message.Record();
            biblio.RecPath = record.Path;

            if (record.RecordBody != null
                && record.RecordBody.Result != null
                && record.RecordBody.Result.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCodeValue.NotFound)
                return biblio;  // 记录不存在

            // biblio 中里面应该有表示错误码的成员就好了。Result.ErrorInfo 提供了错误信息

            XmlDocument dom = new XmlDocument();
            if (record.RecordBody != null
                && string.IsNullOrEmpty(record.RecordBody.Xml) == false)
            {
                // xml
                dom.LoadXml(record.RecordBody.Xml);
            }
            else
            {
                dom.LoadXml("<root />");
            }

            if (record.Cols != null)
            {
                // cols
                foreach (string s in record.Cols)
                {
                    XmlElement col = dom.CreateElement("col");
                    dom.DocumentElement.AppendChild(col);
                    col.InnerText = s;
                }
            }

            biblio.Format = "";
            if (record.RecordBody != null)
            {
                // metadata
                if (string.IsNullOrEmpty(record.RecordBody.Metadata) == false)
                {
                    // 在根元素下放一个 metadata 元素
                    XmlElement metadata = dom.CreateElement("metadata");
                    dom.DocumentElement.AppendChild(metadata);

                    try
                    {
                        XmlDocument metadata_dom = new XmlDocument();
                        metadata_dom.LoadXml(record.RecordBody.Metadata);

                        foreach (XmlAttribute attr in metadata_dom.DocumentElement.Attributes)
                        {
                            metadata.SetAttribute(attr.Name, attr.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        metadata.SetAttribute("error", "metadata XML '" + record.RecordBody.Metadata + "' 装入 DOM 时出错: " + ex.Message);
                    }
                }
                // timestamp
                biblio.Timestamp = ByteArray.GetHexTimeStampString(record.RecordBody.Timestamp);
            }

            biblio.Data = dom.DocumentElement.OuterXml;

            biblio.MD5 = StringUtil.GetMd5(biblio.Data);

            return biblio;
        }

        // TODO: 如果 count 要求 -1，则要循环获取直到 hitcount。
        void GetItemInfo(SearchRequest searchParam)
        {
            string strError = "";
            string strErrorCode = "";
            IList<DigitalPlatform.Message.Record> records = new List<DigitalPlatform.Message.Record>();
            // long batch_size = -1;   // 50 比较合适
            long batch_size = 10;   // 50 比较合适

            LibraryChannel channel = GetChannel();
            try
            {
            // TODO: 若一次调用不足以满足 searchParam.Count 所要求的数量，要能反复多次发出响应数据，直到满足要求的数量未为止。这样的好处是让调用者比较简单，可以假定请求的数量一定会被满足

                BEGIN:
                DigitalPlatform.LibraryClient.localhost.EntityInfo[] entities = null;

                long lRet = 0;

                if (searchParam.DbNameList == "entity")
                    lRet = channel.GetEntities(
                         searchParam.QueryWord,  // strBiblioRecPath
                         searchParam.Start,
                        // searchParam.Count == -1 ? 20 : searchParam.Count,  // 为何这里直接用 -1 导致检索命中 100 个记录后，dp2mserver 收不到？
                         searchParam.Count,  // 为何这里直接用 -1 导致检索命中 100 个记录后，dp2mserver 收不到？
                         searchParam.FormatList,
                         "zh",
                         out entities,
                         out strError);
                else if (searchParam.DbNameList == "order")
                    lRet = channel.GetOrders(
                         searchParam.QueryWord,  // strBiblioRecPath
                         searchParam.Start,
                         searchParam.Count,
                         searchParam.FormatList,
                         "zh",
                         out entities,
                         out strError);
                else if (searchParam.DbNameList == "issue")
                    lRet = channel.GetIssues(
                         searchParam.QueryWord,  // strBiblioRecPath
                         searchParam.Start,
                         searchParam.Count,
                         searchParam.FormatList,
                         "zh",
                         out entities,
                         out strError);
                else if (searchParam.DbNameList == "comment")
                    lRet = channel.GetComments(
                         searchParam.QueryWord,  // strBiblioRecPath
                         searchParam.Start,
                         searchParam.Count,
                         searchParam.FormatList,
                         "zh",
                         out entities,
                         out strError);
                else
                {
                    strError = "无法识别的 DbNameList 参数值 '" + searchParam.DbNameList + "'";
                    goto ERROR1;
                }

                strErrorCode = channel.ErrorCode.ToString();

                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0
                        || (lRet == -1 && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound))
                    {
                        // 没有命中
                        TryResponseSearch(
                                                            new SearchResponse(
searchParam.TaskID,
0,
0,
this.dp2library.LibraryUID,
records,
strError,  // 出错信息大概为 not found。
strErrorCode));
                        return;
                    }
                    // TODO: 如何返回 channel.ErrorCode ?
                    // 或者把 ErrorCode.ItemDbNotDef 当作没有命中来返回
                    goto ERROR1;
                }

                long lHitCount = lRet;

                if (entities == null)
                    entities = new DigitalPlatform.LibraryClient.localhost.EntityInfo[0];

                records.Clear();
                int i = 0;
                foreach (DigitalPlatform.LibraryClient.localhost.EntityInfo entity in entities)
                {
                    DigitalPlatform.Message.Record biblio = new DigitalPlatform.Message.Record();

                    biblio.RecPath = entity.OldRecPath;
                    biblio.Data = entity.OldRecord;
                    biblio.Timestamp = ByteArray.GetHexTimeStampString(entity.OldTimestamp);
                    biblio.Format = "xml";

                    records.Add(biblio);
                    i++;
                }

                // TODO: 如何限定 records 的总尺寸在 64K 以内？一种办法是每次减少一半的数量重新发送
                bool bRet = TryResponseSearch(
                        searchParam.TaskID,
                        lHitCount,
                        searchParam.Start, // lStart,
                        this.dp2library.LibraryUID,
                        records,
                        "",
                        strErrorCode,
                        ref batch_size);
                Console.WriteLine("ResponseSearch called " + records.Count.ToString() + ", bRet=" + bRet);
                if (bRet == false)
                    return;

                if (searchParam.Start + records.Count < lHitCount)
                {
                    searchParam.Start += records.Count;
#if NO
                    if (searchParam.Start >= lHitCount)
                        goto END1;
#endif
                    if (searchParam.Count != -1)
                    {
                        searchParam.Count -= records.Count;
                        if (searchParam.Count <= 0)
                            goto END1;
                    }

                    goto BEGIN;
                }
            }
            catch (Exception ex)
            {
                AddErrorLine("GetItemInfo() 出现异常: " + ex.Message);
                strError = "GetItemInfo() 异常：" + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

        END1:
            this.AddInfoLine("search and response end");
            return;
        ERROR1:
            // 报错
            TryResponseSearch(
                                                new SearchResponse(
searchParam.TaskID,
-1,
0,
this.dp2library.LibraryUID,
records,
strError,
strErrorCode));
        }

        /*
        public string TaskID { get; set; }    // 本次检索的任务 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作
        public string Operation { get; set; }   // 操作名。
        public string DbNameList { get; set; }  // 数据库名列表。一般为 "<全部>"
        public string QueryWord { get; set; }   // 检索词。若为 !getResult 表示不检索、从已有结果集中获取记录
        public string UseList { get; set; }     // 检索途径列表
        public string MatchStyle { get; set; }  // 匹配方式。为 exact/left/right/middle 之一
        public string ResultSetName { get; set; }   // 检索创建的结果集名。空表示为默认结果集
        public string FormatList { get; set; }  // 返回的数据格式列表
        public long MaxResults { get; set; }    // 本次检索最多命中的记录数。-1 表示不限制
        public long Start { get; set; } // 本次获得结果的开始位置
        public long Count { get; set; } // 本次获得结果的个数。 -1表示尽可能多

         * QueryWord --> strItemBarcode
         * UseList --> strConfirmItemRecPath
         * MatchStyle --> strBiblioRecPathExclude
         * () --> nMaxLength 暂时没有使用

        public int GetBiblioSummary(string strItemBarcode,
            string strConfirmItemRecPath,
            string strBiblioRecPathExclude,
            int nMaxLength,
            out string strBiblioRecPath,
            out string strSummary,
            out string strError) 
         * 
         * */
        void GetBiblioSummary(SearchRequest searchParam)
        {
            string strError = "";
            string strErrorCode = "";
            IList<DigitalPlatform.Message.Record> records = new List<DigitalPlatform.Message.Record>();

            LibraryChannel channel = GetChannel();
            try
            {
                string strBiblioRecPath = "";
                string strSummary = "";

                long lRet = channel.GetBiblioSummary(
                    searchParam.QueryWord,
                    searchParam.UseList,
                    searchParam.MatchStyle,
                    out strBiblioRecPath,
                    out strSummary,
                    out strError);
                strErrorCode = channel.ErrorCode.ToString();
                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0
                        || (lRet == -1 && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound))
                    {
                        // 没有命中
                        TryResponseSearch(
                                                            new SearchResponse(
searchParam.TaskID,
0,
0,
this.dp2library.LibraryUID,
records,
strError,   // 出错信息大概为 not found。
strErrorCode));
                        return;
                    }
                    goto ERROR1;
                }

                records.Clear();
                {
                    DigitalPlatform.Message.Record biblio = new DigitalPlatform.Message.Record();
                    biblio.RecPath = strBiblioRecPath;
                    biblio.Data = strSummary;
                    biblio.Format = "";
                    records.Add(biblio);
                }

#if NO
                ResponseSearch(
                    searchParam.TaskID,
                    records.Count,  // lHitCount,
                    0, // lStart,
                    records,
                    "",
                    strErrorCode);
#endif
                // TODO: 是否按照 searchParam.Count 来返回？似乎没有必要，因为调用者可以控制请求参数中的路径个数
                long batch_size = -1;
                bool bRet = TryResponseSearch(
searchParam.TaskID,
records.Count,
0,
this.dp2library.LibraryUID,
records,
"",
strErrorCode,
ref batch_size);
                Console.WriteLine("ResponseSearch called " + records.Count.ToString() + ", bRet=" + bRet);
                if (bRet == false)
                    return;
            }
            catch (Exception ex)
            {
                AddErrorLine("GetBiblioSummary() 出现异常: " + ex.Message);
                strError = "GetBiblioSummary() 异常：" + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

            this.AddInfoLine("search and response end");
            return;
        ERROR1:
            // 报错
            TryResponseSearch(
                                                new SearchResponse(

searchParam.TaskID,
-1,
0,
this.dp2library.LibraryUID,
records,
strError,
strErrorCode));
        }

        static void ConnonicalizeFormats(string[] formats)
        {
            // 规整一下，outputpath/path/recpath 都可用
            int index = Array.IndexOf(formats, "path");
            if (index != -1)
                formats[index] = "outputpath";

            index = Array.IndexOf(formats, "recpath");
            if (index != -1)
                formats[index] = "outputpath";
        }

        // searchParam.UseList 里面提供 strBiblioXml 参数，即，前端提供给服务器，希望服务器加工处理的书目XML内容
        void GetBiblioInfo(SearchRequest searchParam)
        {
            string strError = "";
            string strErrorCode = "";
            IList<DigitalPlatform.Message.Record> records = new List<DigitalPlatform.Message.Record>();

            if (string.IsNullOrEmpty(searchParam.FormatList) == true)
            {
                strError = "FormatList 不应为空";
                goto ERROR1;
            }

            LibraryChannel channel = GetChannel();
            try
            {
                string[] formats = searchParam.FormatList.Split(new char[] { ',' });
                int nPathFormatIndex = -1;  // formats 中 “outputpath” 元素的下标
                if (formats.Length > 0)
                {
                    ConnonicalizeFormats(formats);

                    nPathFormatIndex = Array.IndexOf(formats, "outputpath");
                }

                string[] results = null;
                // string strRecPath = "";
                byte[] baTimestamp = null;

                long lRet = channel.GetBiblioInfos(
                    searchParam.QueryWord,
                    searchParam.UseList, // strBiblioXml
                    formats,
                    out results,
                    out baTimestamp,
                    out strError);
                strErrorCode = channel.ErrorCode.ToString();
                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0
                        || (lRet == -1 && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound))
                    {
                        // 没有命中
                        TryResponseSearch(
                                                            new SearchResponse(

searchParam.TaskID,
0,
0,
this.dp2library.LibraryUID,
records,
strError,  // 出错信息大概为 not found。
strErrorCode));
                        return;
                    }
                    goto ERROR1;
                }

                if (results == null)
                    results = new string[0];

                records.Clear();
                int i = 0;
                foreach (string result in results)
                {
                    DigitalPlatform.Message.Record biblio = new DigitalPlatform.Message.Record();

                    // 注：可以在 formatlist 中包含 outputpath(recpath) 要求获得记录路径，这时记录路径会返回在对应元素的 Data 成员中
                    biblio.RecPath = "";
                    biblio.Data = result;

                    // 当 strBiblioRecPath 用 @path-list: 方式调用时，formats 格式个数 X 路径个数 = results 中元素数
                    // 要将 formats 均匀分配到 records 元素中
                    if (formats != null && formats.Length > 0)
                        biblio.Format = formats[i % formats.Length];

                    records.Add(biblio);
                    i++;
                }


                // 为每个元素的 RecPath 填充值，如果 formats 中出现过 outputpath 的话
                if (nPathFormatIndex != -1)
                {
                    for (int j = 0; j < records.Count / formats.Length; j++)
                    {
                        string path = records[j * formats.Length + nPathFormatIndex].Data;
                        for (int k = 0; k < formats.Length; k++)
                        {
                            records[j * formats.Length + k].RecPath = path;
                        }
                    }
                }

#if NO
                ResponseSearch(
                    searchParam.TaskID,
                    records.Count,  // lHitCount,
                    0, // lStart,
                    records,
                    "",
                    strErrorCode);
#endif
                long batch_size = -1;
                bool bRet = TryResponseSearch(
searchParam.TaskID,
records.Count,
0,
this.dp2library.LibraryUID,
records,
strError,
strErrorCode,
ref batch_size);
                Console.WriteLine("ResponseSearch called " + records.Count.ToString() + ", bRet=" + bRet);
                if (bRet == false)
                    return;
            }
            catch (Exception ex)
            {
                AddErrorLine("GetBiblioInfo() 出现异常: " + ex.Message);
                strError = "GetBiblioInfo() 异常：" + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

            this.AddInfoLine("search and response end");
            return;
        ERROR1:
            // 报错
            TryResponseSearch(
                                                new SearchResponse(
searchParam.TaskID,
-1,
0,
this.dp2library.LibraryUID,
records,
strError,
strErrorCode));
        }

        void GetSystemParameter(SearchRequest searchParam)
        {
            string strError = "";
            string strErrorCode = "";
            IList<DigitalPlatform.Message.Record> records = new List<DigitalPlatform.Message.Record>();

            if (string.IsNullOrEmpty(searchParam.QueryWord) == true)
            {
                strError = "QueryWord 不应为空";
                goto ERROR1;
            }

            LibraryChannel channel = GetChannel();
            try
            {
                string strValue = "";

                long lRet = channel.GetSystemParameter(// null,
                    searchParam.QueryWord,
                    searchParam.FormatList,
                    out strValue,
                    out strError);
                strErrorCode = channel.ErrorCode.ToString();
                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0
                        || (lRet == -1 && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound))
                    {
                        // 没有命中
                        TryResponseSearch(
                            new SearchResponse(
searchParam.TaskID,
0,
0,
this.dp2library.LibraryUID,
records,
strError,  // 出错信息大概为 not found。
strErrorCode));
                        return;
                    }
                    goto ERROR1;
                }

                records.Clear();
                {
                    DigitalPlatform.Message.Record biblio = new DigitalPlatform.Message.Record();
                    biblio.RecPath = "";
                    biblio.Data = strValue;
                    biblio.Format = "";
                    records.Add(biblio);
                }

                long batch_size = -1;
                bool bRet = TryResponseSearch(
searchParam.TaskID,
records.Count,
0,
this.dp2library.LibraryUID,
records,
"",
strErrorCode,
ref batch_size);
                Console.WriteLine("ResponseSearch called " + records.Count.ToString() + ", bRet=" + bRet);
                if (bRet == false)
                    return;
            }
            catch (Exception ex)
            {
                strError = "GetSystemParameter() 异常：" + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }
            return;
        ERROR1:
            // 报错
            TryResponseSearch(
                new SearchResponse(
searchParam.TaskID,
-1,
0,
this.dp2library.LibraryUID,
records,
strError,
strErrorCode));
        }


        void GetPatronInfo(SearchRequest searchParam)
        {
            string strError = "";
            string strErrorCode = "";
            IList<DigitalPlatform.Message.Record> records = new List<DigitalPlatform.Message.Record>();

            if (string.IsNullOrEmpty(searchParam.FormatList) == true)
            {
                strError = "FormatList 不应为空";
                goto ERROR1;
            }

            LibraryChannel channel = GetChannel();
            try
            {
                string[] results = null;
                string strRecPath = "";
                byte[] baTimestamp = null;

                long lRet = channel.GetReaderInfo(// null,
                    searchParam.QueryWord,
                    searchParam.FormatList,
                    out results,
                    out strRecPath,
                    out baTimestamp,
                    out strError);
                strErrorCode = channel.ErrorCode.ToString();
                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0
                        || (lRet == -1 && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound))
                    {
                        // 没有命中
                        TryResponseSearch(
                                                            new SearchResponse(
searchParam.TaskID,
0,
0,
this.dp2library.LibraryUID,
records,
strError,  // 出错信息大概为 not found。
strErrorCode));
                        return;
                    }
                    goto ERROR1;
                }

                if (results == null)
                    results = new string[0];

                records.Clear();
                string[] formats = searchParam.FormatList.Split(new char[] { ',' });
                int i = 0;
                foreach (string result in results)
                {
                    DigitalPlatform.Message.Record biblio = new DigitalPlatform.Message.Record();
                    biblio.RecPath = strRecPath;
                    biblio.Data = result;
                    biblio.Format = formats[i];
                    records.Add(biblio);
                    i++;
                }

#if NO
                ResponseSearch(
                    searchParam.TaskID,
                    records.Count,  // lHitCount,
                    0, // lStart,
                    records,
                    "",
                    strErrorCode);
#endif
                long batch_size = -1;
                bool bRet = TryResponseSearch(
searchParam.TaskID,
records.Count,
0,
this.dp2library.LibraryUID,
records,
"",
strErrorCode,
ref batch_size);
                Console.WriteLine("ResponseSearch called " + records.Count.ToString() + ", bRet=" + bRet);
                if (bRet == false)
                    return;
            }
            catch (Exception ex)
            {
                AddErrorLine("GetPatronInfo() 出现异常: " + ex.Message);
                strError = "GetPatronInfo() 异常：" + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

            this.AddInfoLine("search and response end");
            return;
        ERROR1:
            // 报错
            TryResponseSearch(
                                                new SearchResponse(
searchParam.TaskID,
-1,
0,
this.dp2library.LibraryUID,
records,
strError,
strErrorCode));
        }

        static void SetUserXml(UserInfo userinfo, XmlElement nodeAccount)
        {
            DomUtil.SetAttr(nodeAccount, "name", userinfo.UserName);

            if (String.IsNullOrEmpty(userinfo.Type) == false)
                DomUtil.SetAttr(nodeAccount, "type", userinfo.Type);

            DomUtil.SetAttr(nodeAccount, "rights", userinfo.Rights);

            DomUtil.SetAttr(nodeAccount, "libraryCode", userinfo.LibraryCode);

            DomUtil.SetAttr(nodeAccount, "access", userinfo.Access);

            DomUtil.SetAttr(nodeAccount, "comment", userinfo.Comment);

            DomUtil.SetAttr(nodeAccount, "binding", userinfo.Binding);
        }

        void GetUserInfo(SearchRequest searchParam)
        {
            string strError = "";
            string strErrorCode = "";
            IList<DigitalPlatform.Message.Record> records = new List<DigitalPlatform.Message.Record>();

#if NO
            if (string.IsNullOrEmpty(searchParam.FormatList) == true)
            {
                strError = "FormatList 不应为空";
                goto ERROR1;
            }
#endif

            LibraryChannel channel = GetChannel();
            try
            {
                UserInfo[] results = null;

                long lRet = channel.GetUser(// null,
                    "",
                    searchParam.QueryWord,
                    (int)searchParam.Start,
                    (int)searchParam.Count,
                    out results,
                    out strError);
                strErrorCode = channel.ErrorCode.ToString();
                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0
                        || (lRet == -1 && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound))
                    {
                        // 没有命中
                        TryResponseSearch(
                                                            new SearchResponse(
searchParam.TaskID,
0,
0,
this.dp2library.LibraryUID,
records,
strError,  // 出错信息大概为 not found。
strErrorCode));
                        return;
                    }
                    goto ERROR1;
                }

                if (results == null)
                    results = new UserInfo[0];

                records.Clear();
                int i = 0;
                foreach (UserInfo userinfo in results)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml("<account />");
                    SetUserXml(userinfo, dom.DocumentElement);

                    DigitalPlatform.Message.Record biblio = new DigitalPlatform.Message.Record();
                    biblio.RecPath = "";
                    biblio.Data = dom.DocumentElement.OuterXml;
                    biblio.Format = "xml";
                    records.Add(biblio);
                    i++;
                }

                long batch_size = 10;
                bool bRet = TryResponseSearch(
searchParam.TaskID,
records.Count,
0,
this.dp2library.LibraryUID,
records,
"",
strErrorCode,
ref batch_size);
                Console.WriteLine("ResponseSearch called " + records.Count.ToString() + ", bRet=" + bRet);
                if (bRet == false)
                    return;
            }
            catch (Exception ex)
            {
                AddErrorLine("GetUserInfo() 出现异常: " + ex.Message);
                strError = "GetUserInfo() 异常：" + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

            this.AddInfoLine("search and response end");
            return;
        ERROR1:
            // 报错
            TryResponseSearch(
                                                new SearchResponse(
searchParam.TaskID,
-1,
0,
this.dp2library.LibraryUID,
records,
strError,
strErrorCode));
        }

        #endregion

#if NO
        // 连接成功后被调用，执行登录功能。重载时要调用 Login(...) 向 server 发送 login 消息
        public override void TriggerLogin()
        {
            LoginRequest param = new LoginRequest();
            param.UserName = this.UserName;
            param.Password = this.Password;

            LoginAsync(param)
            .ContinueWith((antecendent) =>
                    {
                        if (antecendent.IsFaulted == true)
                        {
                            // AddErrorLine(GetExceptionText(antecendent.Exception));
                            // 在日志中写入一条错误信息
                            // Program.WriteWindowsLog();
                            return;
                        }
                    });
        }
#endif
    }
}
