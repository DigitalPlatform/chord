using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform.Common;
using DigitalPlatform.Net;
using Newtonsoft.Json;

namespace DigitalPlatform.Z3950.Server
{
    /// <summary>
    /// Z39.50 服务器
    /// </summary>
    public class ZServer : TcpServer
    {
        // ZServerChannel 对象打开事件
        // 如果希望为 ZServerChannel 挂接 Closed 事件，可以在此事件内挂接
        public event EventHandler ChannelOpened = null;

        // public event ChannelClosedEventHandler ChannelClosed = null;

        public event ProcessRequestEventHandler ProcessRequest = null;

        public event ProcessInitializeEventHandler ProcessInitialize = null;

        // 初始化阶段登录 事件
        public event InitializeLoginEventHandler InitializeLogin = null;

        public event SetChannelPropertyEventHandler SetChannelProperty = null;

        // public event GetZConfigEventHandler GetZConfig = null;

        public event ProcessSearchEventHandler ProcessSearch = null;

        public event SearchSearchEventHandler SearchSearch = null;

        public event ProcessPresentEventHandler ProcessPresent = null;

        public event PresentGetRecordsEventHandler PresentGetRecords = null;


        // public static ILog _log = null;

        #region Public Methods

        public ZServer(int port) : base(port)
        {
            // this.Port = port;
        }

        public override string GetServerName()
        {
            return "Z39.50 服务器";
        }

#if NO
        public virtual async void TestHandleClient(TcpClient tcpClient,
    CancellationToken token)
        {

        }
#endif

#if NO
        // 处理一个通道的通讯活动
        public async override void HandleClient(TcpClient tcpClient,
            Action close_action,
            CancellationToken token)
        {
            List<byte> cache = new List<byte>();

            ZServerChannel channel = _tcpChannels.Add(tcpClient, () => { return new ZServerChannel(); }) as ZServerChannel;
            // 允许对 channel 做额外的初始化
            if (this.ChannelOpened != null)
                this.ChannelOpened(channel, new EventArgs());
            try
            {
                string ip = "";

                try
                {
                    ip = GetClientIP(tcpClient);
                    channel.Touch();

                    int i = 0;
                    bool running = true;
                    while (running)
                    {
                        if (token != null && token.IsCancellationRequested)
                            return;
                        // 注意调用返回后如果发现返回 null 或者抛出了异常，调主要主动 Close 和重新分配 TcpClient
                        BerTree request = await ZProcessor.GetIncomingRequest(
                            cache,
                            tcpClient,
                            ()=>channel.Touch()).ConfigureAwait(false);  // 2018/10/10 add configure
                        if (request == null)
                        {
                            Console.WriteLine("client close on request " + i);
                            break;
                        }
                        Console.WriteLine("request " + i);

                        channel.Touch();
                        if (token != null && token.IsCancellationRequested)
                            return;

                        byte[] response = null;
                        if (this.ProcessRequest == null)
                            response = await DefaultProcessRequest(channel, request).ConfigureAwait(false);  // 2018/10/10 add configure
                        else
                        {
                            ProcessRequestEventArgs e = new ProcessRequestEventArgs();
                            e.Request = request;
                            this.ProcessRequest(channel, e);
                            response = e.Response;
                        }

                        channel.Touch();
                        if (token != null && token.IsCancellationRequested)
                            return;

                        // 注意调用返回 result.Value == -1 情况下，要及时 Close TcpClient
                        Result result = await ZProcessor.SendResponse(response, tcpClient).ConfigureAwait(false); // 2018/10/10 add configure
                        channel.Touch();
                        if (result.Value == -1)
                        {
                            Console.WriteLine("error on response " + i + ": " + result.ErrorInfo);
                            break;
                        }

                        i++;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null && ex.InnerException is ObjectDisposedException)
                    {
                        // 这种情况一般是 server 主动清理闲置通道导致的，不记入日志
                    }
                    else if (ex is ObjectDisposedException)
                    {
                        // 这种情况一般是 client close 通道导致，不记入日志
                    }
                    else
                    {
                        string strName = "ip:" + ip 
                            + channel == null ? "(null)" : " channel:" + channel.GetHashCode();
                        string strError = strName + " HandleClient() 异常: " + ExceptionUtil.GetExceptionText(ex);
                        LibraryManager.Log?.Error(strError);
                        // Console.WriteLine(strError);
                    }
                }
                finally
                {
#if NO
                    outputStream.Flush();
                    outputStream.Close();
                    outputStream = null;

                    inputStream.Close();
                    inputStream = null;
#endif

                    // tcpClient.Close();

                    // 清除全局结果集
                }
            }
            finally
            {
                _tcpChannels.Remove(channel);
#if NO
                if (this.ChannelClosed != null)
                {
                    ChannelClosedEventArgs e = new ChannelClosedEventArgs();
                    e.Channel = channel;
                    this.ChannelClosed(channel, e);
                }
#endif
                channel.Close();
                if (close_action != null)
                    close_action.Invoke();
            }
        }
#endif
        // Pipeline 版本
        // 处理一个通道的通讯活动
        public async override void HandleClient(TcpClient tcpClient,
            Action close_action,
            CancellationToken token)
        {
            List<byte> cache = new List<byte>();

            ZServerChannel channel = _tcpChannels.Add(tcpClient, () => { return new ZServerChannel(); }) as ZServerChannel;
            // 允许对 channel 做额外的初始化
            if (this.ChannelOpened != null)
                this.ChannelOpened(channel, new EventArgs());
            try
            {
                string name = "";
                string ip = "";

                Task<Result> task = null;
                try
                {
                    name = channel.GetDebugName(tcpClient);
                    ip = TcpServer.GetClientIP(tcpClient);

                    channel.Touch();

                    int i = 0;
                    bool running = true;
                    while (running)
                    {
                        if (token != null && token.IsCancellationRequested)
                            return;

                        // 注意调用返回后如果发现返回 null 或者抛出了异常，调主要主动 Close 和重新分配 TcpClient
                        BerTree request = await ZProcessor.GetIncomingRequest(
                            cache,
                            tcpClient,
                            () => channel.Touch()).ConfigureAwait(false);  // 2018/10/10 add configure
                        if (request == null)
                        {
                            Console.WriteLine("client close on request " + i);
                            break;
                        }
                        Console.WriteLine("request " + i);

                        channel.Touch();
                        if (token != null && token.IsCancellationRequested)
                            return;

                        // 如果前一轮的任务还没有完成，这里等待
                        if (task != null)
                        {
                            Result result = task.Result;
                            if (result.Value == -1)
                                return;
                            task = null;
                        }

                        task = Task.Run(() =>
                         ProcessAndResponse(
    tcpClient,
    close_action,
    channel,
    request,
    token));

                        i++;
                    }
                }
                catch (Exception ex)
                {
#if NO
                                        if (ex.InnerException != null && ex.InnerException is ObjectDisposedException)
                    {
                        // 这种情况一般是 server 主动清理闲置通道导致的，不记入日志
                    }
                    else if (ex is ObjectDisposedException)
                    {
                        // 这种情况一般是 client close 通道导致，不记入日志
                    }
#endif

#if NO
                    if (GetException(ex, typeof(ObjectDisposedException)) != null)
                        return;

                    SocketException socket_ex = (SocketException)GetException(ex, typeof(SocketException));
                    if (socket_ex != null && socket_ex.SocketErrorCode == SocketError.ConnectionReset)
                        return;

                    {
                        string strName = "ip:" + ip
                        + channel == null ? "(null)" : " channel:" + channel.GetHashCode();
                        string strError = strName + " HandleClient() 异常: " + ExceptionUtil.GetExceptionText(ex);
                        LibraryManager.Log?.Error(strError);
                        // Console.WriteLine(strError);
                    }
#endif
                    if (ex is UnknownApduException
                        || ex is BadApduException)
                    {
                        IpTable.SetInBlackList(ip, TimeSpan.FromHours(1));
                        LibraryManager.Log?.Info(string.Format("IP 地址 {0} 已被加入黑名单，时限一个小时", ip));
                    }

                    LogException(ex, channel, name);
                }
                finally
                {
#if NO
                    outputStream.Flush();
                    outputStream.Close();
                    outputStream = null;

                    inputStream.Close();
                    inputStream = null;
#endif

                    // tcpClient.Close();

                    // 清除全局结果集
                    if (task != null)
                    {
                        // TODO: 要想办法立即终止检索和响应过程
                    }
                }
            }
            finally
            {
                _tcpChannels.Remove(channel);
#if NO
                if (this.ChannelClosed != null)
                {
                    ChannelClosedEventArgs e = new ChannelClosedEventArgs();
                    e.Channel = channel;
                    this.ChannelClosed(channel, e);
                }
#endif
                channel.Close();
                if (close_action != null)
                    close_action.Invoke();
            }
        }

        static Exception GetException(Exception exception, Type type)
        {
            while (exception != null)
            {
                if (exception.GetType() == type)
                    return exception;
                exception = exception.InnerException;
            }

            return null;
        }

        void LogException(Exception ex,
            ZServerChannel channel,
            string name)
        {
            if (GetException(ex, typeof(ObjectDisposedException)) != null)
                return;

            SocketException socket_ex = (SocketException)GetException(ex, typeof(SocketException));
            if (socket_ex != null && socket_ex.SocketErrorCode == SocketError.ConnectionReset)
                return;

            {
                //string strName = "ip:" + ip
                //+ channel == null ? "(null)" : " channel:" + channel.GetHashCode();
                string strError = string.Format("{0} HandleClient() 异常: {1}",
                    name, ExceptionUtil.GetExceptionText(ex));
                LibraryManager.Log?.Error(strError);
                // Console.WriteLine(strError);
            }
        }

        async Task<Result> ProcessAndResponse(
            TcpClient tcpClient,
            Action close_action,
            ZServerChannel channel,
            BerTree request,
            CancellationToken token)
        {
            string name = "";
            string ip = "";
            bool error = false;
            try
            {
                name = channel.GetDebugName(tcpClient);
                ip = TcpServer.GetClientIP(tcpClient);

                byte[] response = null;
                if (this.ProcessRequest == null)
                    response = await DefaultProcessRequest(channel, request).ConfigureAwait(false);  // 2018/10/10 add configure
                else
                {
                    ProcessRequestEventArgs e = new ProcessRequestEventArgs();
                    e.Request = request;
                    this.ProcessRequest(channel, e);
                    response = e.Response;
                }

                channel.Touch();
                if (token != null && token.IsCancellationRequested)
                {
                    error = true;
                    return new Result { Value = -1, ErrorInfo = "Cancelled" };
                }

                // 注意调用返回 result.Value == -1 情况下，要及时 Close TcpClient
                Result result = await ZProcessor.SendResponse(response, tcpClient).ConfigureAwait(false); // 2018/10/10 add configure
                channel.Touch();
                if (result.Value == -1)
                {
                    Console.WriteLine("error on response " + name + ": " + result.ErrorInfo);
                    error = true;
                    return result;
                }

                return new Result();
            }
            catch (Exception ex)
            {
                if (ex is UnknownApduException
                    || ex is BadApduException)
                {
                    IpTable.SetInBlackList(ip, TimeSpan.FromHours(1));
                    LibraryManager.Log?.Info(string.Format("IP 地址 {0} 已被加入黑名单，时限一个小时", ip));
                }

                if (channel != null)
                {
                    channel.Close();
                    if (close_action != null)
                        close_action.Invoke();
                    LogException(ex, channel, name);
                    channel = null;
                }
                else
                    LogException(ex, channel, name);

                return new Result { Value = -1, ErrorInfo = "Cancelled" };
            }
            finally
            {
                if (error == true
                    && channel != null)
                {
                    channel.Close();
                    if (close_action != null)
                        close_action.Invoke();
                }
            }
        }

        #endregion

        public Result DefaultSetChannelProperty(TcpChannel channel,
    InitRequestInfo info)
        {
            return new Result();
        }

        public ZConfig DefaultGetZConfig(TcpChannel channel,
            InitRequestInfo info,
            out string strError)
        {
            strError = "";
            return new ZConfig();
        }

        // 默认的请求处理过程。应可以被重新指定
        // 下级函数，例如处理 Initialize Search Present 的函数，还可以被重新指定
        public async Task<byte[]> DefaultProcessRequest(ZServerChannel channel,
            BerTree request)
        {
            BerNode root = request.GetAPDuRoot();

            switch (root.m_uTag)
            {
                case BerTree.z3950_initRequest:
                    if (this.ProcessInitialize == null)
                        return await DefaultProcessInitialize(channel, request).ConfigureAwait(false);   // 2018/10/10 add configure
                    else
                    {
                        ProcessInitializeEventArgs e = new ProcessInitializeEventArgs();
                        e.Request = request;
                        this.ProcessInitialize(channel, e);
                        return e.Response;
                    }
                    break;
                case BerTree.z3950_searchRequest:
                    if (this.ProcessSearch == null)
                        return await DefaultProcessSearch(channel, request).ConfigureAwait(false); // 2018/10/10 add configure
                    else
                    {
                        ProcessSearchEventArgs e = new ProcessSearchEventArgs();
                        e.Request = request;
                        this.ProcessSearch(channel, e);
                        return e.Response;
                    }
                    break;
                case BerTree.z3950_presentRequest:
                    if (this.ProcessPresent == null)
                        return await DefaultProcessPresent(channel, request).ConfigureAwait(false); // 2018/10/10 add configure
                    else
                    {
                        ProcessPresentEventArgs e = new ProcessPresentEventArgs();
                        e.Request = request;
                        this.ProcessPresent(channel, e);
                        return e.Response;
                    }
                    break;
            }

            return new byte[0];
            // TODO 如果将来嫌日志中记载 BerNode Dump 结果体积太大，Dump 结果可以考虑不进入日志
            //string text = JsonConvert.SerializeObject(root, Formatting.Indented);
            //throw new UnknownApduException(string.Format("无法识别的 APDU tag[{0}]\r\nBerNode={1}", root.m_uTag, text));
        }

        // 根据 @xxx 找到相关的 capo 实例，然后找到配置参数
        Result AutoSetChannelProperty(TcpChannel channel,
            InitRequestInfo info)
        {
            if (this.SetChannelProperty == null)
                return this.DefaultSetChannelProperty(channel, info);
            else
            {
                SetChannelPropertyEventArgs e = new SetChannelPropertyEventArgs();
                e.Info = info;
                this.SetChannelProperty(channel, e);
                return e.Result;
            }
        }

#if NO
        // 根据 @xxx 找到相关的 capo 实例，然后找到配置参数
        ZConfig AutoGetZConfig(ZServerChannel channel,
            InitRequestInfo info,
            out string strError)
        {
            strError = "";
            if (this.GetZConfig == null)
                return this.DefaultGetZConfig(channel, info, out strError);

            GetZConfigEventArgs e = new GetZConfigEventArgs();
            e.Info = info;
            this.GetZConfig(channel, e);
            strError = e.Result.ErrorInfo;
            return e.ZConfig;
        }
#endif

        public /*async*/ Task<byte[]> DefaultProcessInitialize(ZServerChannel channel,
            BerTree request)
        {
            BerNode root = request.GetAPDuRoot();

            Encoding encoding = Encoding.GetEncoding(936);

            REDO:
            int nRet = ZProcessor.Decode_InitRequest(
                root,
                encoding,
                out InitRequestInfo info,
                out string strDebugInfo,
                out string strError);
            if (nRet == -1)
                goto ERROR1;

            // 可以用groupid来表示字符集信息

            InitResponseInfo response_info = new InitResponseInfo();

            if (info.m_charNego != null)
            {
                /* option
        * 
        search                 (0), 
        present                (1), 
        delSet                 (2),
        resourceReport         (3),
        triggerResourceCtrl    (4),
        resourceCtrl           (5), 
        accessCtrl             (6),
        scan                   (7),
        sort                   (8), 
        --                     (9) (reserved)
        extendedServices       (10),
        level-1Segmentation    (11),
        level-2Segmentation    (12),
        concurrentOperations   (13),
        namedResultSets        (14)
        15 Encapsulation  Z39.50-1995 Amendment 3: Z39.50 Encapsulation 
        16 resultCount parameter in Sort Response  See Note 8 Z39.50-1995 Amendment 1: Add resultCount parameter to Sort Response  
        17 Negotiation Model  See Note 9 Model for Z39.50 Negotiation During Initialization  
        18 Duplicate Detection See Note 1  Z39.50 Duplicate Detection Service  
        19 Query type 104 
        * }
        */
                response_info.m_strOptions = "yynnnnnnnnnnnnn";

                if (info.m_charNego.EncodingLevelOID == CharsetNeogatiation.Utf8OID)
                {
                    BerTree.SetBit(ref response_info.m_strOptions,
                        17,
                        true);
                    response_info.m_charNego = info.m_charNego;

                    // 2018/9/19
                    // 如果最初解码用的编码方式不是 UTF8，则需要重新解码
                    if (encoding != Encoding.UTF8)
                    {
                        encoding = Encoding.UTF8;
                        goto REDO;
                    }
                    channel.EnsureProperty()._searchTermEncoding = Encoding.UTF8;
                    if (info.m_charNego.RecordsInSelectedCharsets != -1)
                    {
                        response_info.m_charNego.RecordsInSelectedCharsets = info.m_charNego.RecordsInSelectedCharsets; // 依从前端的请求
                        if (response_info.m_charNego.RecordsInSelectedCharsets == 1)
                            channel.EnsureProperty()._marcRecordEncoding = Encoding.UTF8;
                    }
                }
            }
            else
            {
                response_info.m_strOptions = "yynnnnnnnnnnnnn";
            }




            // 判断info中的信息，决定是否接受Init请求。

            // ZServerChannel 初始化设置一些信息。这样它一直携带着伴随生命周期全程
            Result result = AutoSetChannelProperty(channel, info);
            if (result.Value == -1)
            {
                response_info.m_nResult = 0;
                channel.EnsureProperty()._bInitialized = false;

                ZProcessor.SetInitResponseUserInfo(response_info,
                    "1.2.840.10003.4.1", // string strOID,
                    string.IsNullOrEmpty(result.ErrorCode) ? 100 : Convert.ToInt32(result.ErrorCode),  // (unspecified) error
                    result.ErrorInfo);
                goto DO_RESPONSE;
            }

#if NO
            if (String.IsNullOrEmpty(info.m_strID) == true)
            {
                ZConfig config = AutoGetZConfig(channel, info, out strError);
                if (config == null)
                {
                    ZProcessor.SetInitResponseUserInfo(response_info,
    "", // string strOID,
    0,  // long lErrorCode,
    strError);
                    goto DO_RESPONSE;
                }
                // 如果定义了允许匿名登录
                if (String.IsNullOrEmpty(config.AnonymousUserName) == false)
                {
                    info.m_strID = config.AnonymousUserName;
                    info.m_strPassword = config.AnonymousPassword;
                }
                else
                {
                    response_info.m_nResult = 0;
                    channel.SetProperty()._bInitialized = false;

                    ZProcessor.SetInitResponseUserInfo(response_info,
                        "", // string strOID,
                        0,  // long lErrorCode,
                        "不允许匿名登录");
                    goto DO_RESPONSE;
                }
            }
#endif

            if (this.InitializeLogin != null)
            {
                InitializeLoginEventArgs e = new InitializeLoginEventArgs();

                this.InitializeLogin(channel, e);
                if (e.Result.Value == -1 || e.Result.Value == 0)
                {
                    response_info.m_nResult = 0;
                    channel.EnsureProperty()._bInitialized = false;

                    ZProcessor.SetInitResponseUserInfo(response_info,
                        "1.2.840.10003.4.1", // string strOID,
                        string.IsNullOrEmpty(e.Result.ErrorCode) ? 101 : Convert.ToInt32(e.Result.ErrorCode),  // Access-control failure
                        e.Result.ErrorInfo);
                }
                else
                {
                    response_info.m_nResult = 1;
                    channel.EnsureProperty()._bInitialized = true;
                }
            }
            else
            {
                response_info.m_nResult = 1;
                channel.EnsureProperty()._bInitialized = true;
            }

#if NO
            // 进行登录
            // return:
            //      -1  error
            //      0   登录未成功
            //      1   登录成功
            nRet = DoLogin(info.m_strGroupID,
                info.m_strID,
                info.m_strPassword,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                response_info.m_nResult = 0;
                this._bInitialized = false;

                ZProcessor.SetInitResponseUserInfo(response_info,
                    "", // string strOID,
                    0,  // long lErrorCode,
                    strError);
            }
            else
            {
                response_info.m_nResult = 1;
                channel._bInitialized = true;
            }
#endif

            DO_RESPONSE:
            // 填充 response_info 的其它结构
            response_info.m_strReferenceId = info.m_strReferenceId;

            //if (channel._property == null)
            //    channel._property = new ChannelPropterty();

            if (info.m_lPreferredMessageSize != 0)
                channel.EnsureProperty().PreferredMessageSize = info.m_lPreferredMessageSize;
            // 极限
            if (channel.EnsureProperty().PreferredMessageSize > ZServerChannelProperty.MaxPreferredMessageSize)
                channel.EnsureProperty().PreferredMessageSize = ZServerChannelProperty.MaxPreferredMessageSize;
            response_info.m_lPreferredMessageSize = channel.EnsureProperty().PreferredMessageSize;

            if (info.m_lExceptionalRecordSize != 0)
                channel.EnsureProperty().ExceptionalRecordSize = info.m_lExceptionalRecordSize;
            // 极限
            if (channel.EnsureProperty().ExceptionalRecordSize > ZServerChannelProperty.MaxExceptionalRecordSize)
                channel.EnsureProperty().ExceptionalRecordSize = ZServerChannelProperty.MaxExceptionalRecordSize;
            response_info.m_lExceptionalRecordSize = channel.EnsureProperty().ExceptionalRecordSize;

            response_info.m_strImplementationId = "Digital Platform";
            response_info.m_strImplementationName = "dp2Capo";
            response_info.m_strImplementationVersion = "1.0";

            // BerTree tree = new BerTree();
            ZProcessor.Encode_InitialResponse(response_info,
                out byte[] baResponsePackage);

            return Task.FromResult(baResponsePackage);
            ERROR1:
            // TODO: 将错误原因写入日志
            LibraryManager.Log?.Error(strError);
            return null;
        }

        public async Task<byte[]> DefaultProcessSearch(ZServerChannel channel,
            BerTree request)
        {
            BerNode root = request.GetAPDuRoot();

            // 解码Search请求包
            int nRet = ZProcessor.Decode_SearchRequest(
                    root,
                    out SearchRequestInfo info,
                    out string strError);
            if (nRet == -1)
                goto ERROR1;

            if (channel.EnsureProperty()._bInitialized == false)
                return null;

            SearchSearchEventArgs e = new SearchSearchEventArgs();
            e.Request = info;
            if (this.SearchSearch == null)
            {
                // 返回模拟的结果，假装命中了一条记录
                e.Result = new ZClient.SearchResult { Value = 1 };
            }
            else
            {
                this.SearchSearch(channel, e);
            }

            // 编码Search响应包
            nRet = ZProcessor.Encode_SearchResponse(info,
                e.Result,
                e.Diag,
                out byte[] baResponsePackage,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return baResponsePackage;
            ERROR1:
            // TODO: 将错误原因写入日志
            return null;
        }

        public async Task<byte[]> DefaultProcessPresent(ZServerChannel channel,
    BerTree request)
        {
            BerNode root = request.GetAPDuRoot();

            // 解码Search请求包
            int nRet = ZProcessor.Decode_PresentRequest(
                root,
                out PresentRequestInfo info,
                out string strError);
            if (nRet == -1)
                goto ERROR1;

            if (channel.EnsureProperty()._bInitialized == false)
                return null;

            PresentGetRecordsEventArgs e = new PresentGetRecordsEventArgs();
            if (this.PresentGetRecords == null)
            {
                // 模拟返回一条记录
                e.Records = new List<RetrivalRecord>();
                RetrivalRecord record = new RetrivalRecord();
                // TODO: 准备数据
                e.Records.Add(record);
            }
            else
            {
                e.Request = info;
                this.PresentGetRecords(channel, e);
            }

            // 编码Present响应包
            nRet = ZProcessor.Encode_PresentResponse(info,
                e.Records,
                e.Diag,
                e.TotalCount,
                out byte[] baResponsePackage);
            if (nRet == -1)
                goto ERROR1;

            return baResponsePackage;
            ERROR1:
            // TODO: 将错误原因写入日志
            return null;
        }

        // 获得统计信息
        public string GetStatisInfo()
        {
            StringBuilder text = new StringBuilder();

            text.AppendFormat("通道对象数: {0}\r\n", this._tcpChannels.Count);
            text.AppendFormat("IpTable: {0}", this.IpTable.GetStatisInfo());

            return text.ToString();
        }
    }

    /// <summary>
    /// 无法识别的 APDU。注：APDU 本身是完整的
    /// </summary>
    public class UnknownApduException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="error"></param>
        /// <param name="strText"></param>
        public UnknownApduException(string strText)
            : base(strText)
        {
        }
    }

    /// <summary>
    /// 不合法的 APDU
    /// </summary>
    public class BadApduException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="error"></param>
        /// <param name="strText"></param>
        public BadApduException(string strText)
            : base(strText)
        {
        }
    }
}
