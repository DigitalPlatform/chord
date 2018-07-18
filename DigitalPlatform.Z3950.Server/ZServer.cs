using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DigitalPlatform.Net;
using log4net;

namespace DigitalPlatform.Z3950.Server
{
    public class ZServer : IDisposable
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

        public ZServerChannelCollection _zChannels = new ZServerChannelCollection();

        public CancellationToken _cancelToken = new CancellationToken();

        #region Fields

        private int Port;
        private TcpListener Listener;
        private bool IsActive = true;

        private IpTable IpTable;

        #endregion

        // public static ILog _log = null;

        #region Public Methods
        public ZServer(int port)
        {
            this.Port = port;
            // _log = log;
        }

        public static string GetClientIP(TcpClient s)
        {
            return ((IPEndPoint)s.Client.RemoteEndPoint).Address.ToString();
        }

        public async void Listen(int backlog)
        {
            try
            {
                this.IpTable = new IpTable();
                this.Listener = new TcpListener(IPAddress.Any, this.Port);
                this.Listener.Start(backlog);  // TODO: 要捕获异常
            }
            catch (Exception ex)
            {
                string strError = "Listen() Start() 出现异常: " + ExceptionUtil.GetExceptionMessage(ex);
                ZManager.Log?.Error(strError);
                throw ex;
            }

            Console.WriteLine("Z39.50 服务器成功监听于 " + this.Port.ToString());

            while (this.IsActive)
            {
                TcpClient tcpClient = null;
                try
                {
                    tcpClient = await this.Listener.AcceptTcpClientAsync();

                    // string ip = ((IPEndPoint)s.Client.RemoteEndPoint).Address.ToString();
                    string ip = GetClientIP(tcpClient);
                    // ZManager.Log?.Info("*** ip [" + ip + "] request");

                    if (this.IpTable != null)
                    {
                        string error = this.IpTable.CheckIp(ip);
                        if (error != null)
                        {
                            tcpClient.Close();
                            // TODO: 可以在首次出现这种情况的时候记入错误日志
                            // ZManager.Log?.Info("*** ip [" + ip + "] 被禁止 Connect。原因: " + error);
                            continue;
                        }
                    }

                    Task task = // 用来消除警告 // https://stackoverflow.com/questions/18577054/alternative-to-task-run-that-doesnt-throw-warning
                    Task.Run(() =>
                            HandleClient(tcpClient,
                                () =>
                                {
                                    if (this.IpTable != null)
                                        this.IpTable.FinishIp(ip);
                                    tcpClient = null;
                                },
                                _cancelToken));


                }
                catch (Exception ex)
                {
                    if (tcpClient != null)
                        tcpClient.Close();

                    if (this.IsActive == false)
                        break;
                    ZManager.Log?.Error("Listen() 出现异常: " + ExceptionUtil.GetExceptionMessage(ex));
                }
                Thread.Sleep(1);
            }
        }

        public void Close()
        {
            this.IsActive = false;
            this.Listener.Stop();
            _zChannels.Clear();
        }

        public void TryClearBlackList()
        {
            if (this.IpTable == null)
                return;

            // 清理一次黑名单
            this.IpTable.ClearBlackList(TimeSpan.FromMinutes(10));
        }

#if NO
        public virtual async void TestHandleClient(TcpClient tcpClient,
    CancellationToken token)
        {

        }
#endif

        public void Dispose()
        {
            _zChannels.Dispose();
        }

        // 处理一个通道的通讯活动
        public async void HandleClient(TcpClient tcpClient,
            Action close_action,
            CancellationToken token)
        {
            ZServerChannel channel = _zChannels.Add(tcpClient);
            // 允许对 channel 做额外的初始化
            if (this.ChannelOpened != null)
                this.ChannelOpened(channel, new EventArgs());
            try
            {
                //List<byte> cache = new List<byte>();
                string ip = "";
                //Stream inputStream = tcpClient.GetStream();
                //Stream outputStream = tcpClient.GetStream();

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
                        BerTree request = await ZProcessor.GetIncomingRequest(tcpClient);
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
                            response = await DefaultProcessRequest(channel, request);
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
                        Result result = await ZProcessor.SendResponse(response, tcpClient);
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
                    string strError = "ip:" + ip + " HandleClient() 异常: " + ExceptionUtil.GetExceptionText(ex);
                    ZManager.Log?.Error(strError);
                    // Console.WriteLine(strError);
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
                _zChannels.Remove(channel);
#if NO
                if (this.ChannelClosed != null)
                {
                    ChannelClosedEventArgs e = new ChannelClosedEventArgs();
                    e.Channel = channel;
                    this.ChannelClosed(channel, e);
                }
#endif
                channel.Close();
                close_action.Invoke();
            }
        }

        #endregion

        public Result DefaultSetChannelProperty(ZServerChannel channel,
    InitRequestInfo info)
        {
            return new Result();
        }

        public ZConfig DefaultGetZConfig(ZServerChannel channel,
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
                        return await DefaultProcessInitialize(channel, request);
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
                        return await DefaultProcessSearch(channel, request);
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
                        return await DefaultProcessPresent(channel, request);
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
        }

        // 根据 @xxx 找到相关的 capo 实例，然后找到配置参数
        Result AutoSetChannelProperty(ZServerChannel channel,
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

        public async Task<byte[]> DefaultProcessInitialize(ZServerChannel channel,
            BerTree request)
        {
            BerNode root = request.GetAPDuRoot();

            int nRet = ZProcessor.Decode_InitRequest(
                root,
                out InitRequestInfo info,
                out string strDebugInfo,
                out string strError);
            if (nRet == -1)
                goto ERROR1;

            // 可以用groupid来表示字符集信息

            InitResponseInfo response_info = new InitResponseInfo();

            // 判断info中的信息，决定是否接受Init请求。

            // ZServerChannel 初始化设置一些信息。这样它一直携带着伴随生命周期全程
            Result result = AutoSetChannelProperty(channel, info);
            if (result.Value == -1)
            {
                response_info.m_nResult = 0;
                channel.SetProperty()._bInitialized = false;

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
                    channel.SetProperty()._bInitialized = false;

                    ZProcessor.SetInitResponseUserInfo(response_info,
                        "1.2.840.10003.4.1", // string strOID,
                        string.IsNullOrEmpty(e.Result.ErrorCode) ? 101 : Convert.ToInt32(e.Result.ErrorCode),  // Access-control failure
                        e.Result.ErrorInfo);
                }
                else
                {
                    response_info.m_nResult = 1;
                    channel.SetProperty()._bInitialized = true;
                }
            }
            else
            {
                response_info.m_nResult = 1;
                channel.SetProperty()._bInitialized = true;
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
            // 填充response_info的其它结构
            response_info.m_strReferenceId = info.m_strReferenceId;

            //if (channel._property == null)
            //    channel._property = new ChannelPropterty();

            if (info.m_lPreferredMessageSize != 0)
                channel.SetProperty().PreferredMessageSize = info.m_lPreferredMessageSize;
            // 极限
            if (channel.SetProperty().PreferredMessageSize > ChannelProperty.MaxPreferredMessageSize)
                channel.SetProperty().PreferredMessageSize = ChannelProperty.MaxPreferredMessageSize;
            response_info.m_lPreferredMessageSize = channel.SetProperty().PreferredMessageSize;

            if (info.m_lExceptionalRecordSize != 0)
                channel.SetProperty().ExceptionalRecordSize = info.m_lExceptionalRecordSize;
            // 极限
            if (channel.SetProperty().ExceptionalRecordSize > ChannelProperty.MaxExceptionalRecordSize)
                channel.SetProperty().ExceptionalRecordSize = ChannelProperty.MaxExceptionalRecordSize;
            response_info.m_lExceptionalRecordSize = channel.SetProperty().ExceptionalRecordSize;

            response_info.m_strImplementationId = "Digital Platform";
            response_info.m_strImplementationName = "dp2Capo";
            response_info.m_strImplementationVersion = "1.0";

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
                    channel.SetProperty()._searchTermEncoding = Encoding.UTF8;
                    if (info.m_charNego.RecordsInSelectedCharsets != -1)
                    {
                        response_info.m_charNego.RecordsInSelectedCharsets = info.m_charNego.RecordsInSelectedCharsets; // 依从前端的请求
                        if (response_info.m_charNego.RecordsInSelectedCharsets == 1)
                            channel.SetProperty()._marcRecordEncoding = Encoding.UTF8;
                    }
                }
            }
            else
            {
                response_info.m_strOptions = "yynnnnnnnnnnnnn";
            }

            // BerTree tree = new BerTree();
            ZProcessor.Encode_InitialResponse(response_info,
                out byte[] baResponsePackage);

            return baResponsePackage;
            ERROR1:
            // TODO: 将错误原因写入日志
            ZManager.Log?.Error(strError);
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

            if (channel.SetProperty()._bInitialized == false)
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

            if (channel.SetProperty()._bInitialized == false)
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
    }

}
