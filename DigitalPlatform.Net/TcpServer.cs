using DigitalPlatform.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalPlatform.Net
{
    /// <summary>
    /// 通用的 TCP 服务器类。
    /// 为它加上具体的消息分界机制以后，可以派生出 ZServer 类用于处理 Z39.50 协议；也可以派生出 SIPServer 用于处理 SIP2 协议
    /// </summary>
    public class TcpServer : IDisposable
    {
        #region Fields

        public TcpChannelCollection _tcpChannels = new TcpChannelCollection();

        public CancellationToken _cancelToken = new CancellationToken();

        private int _port;
        private TcpListener _listener;
        private bool _isActive = true;

        private IpTable _ipTable = new IpTable();
        public IpTable IpTable
        {
            get
            {
                return _ipTable;
            }
        }

        CompactLog _compactLog = new CompactLog();

        #endregion

        public TcpServer(int port)
        {
            this._port = port;
            // _log = log;
        }

        /// <summary>
        /// 用于写入 Log 的服务器名称
        /// </summary>
        /// <returns>服务器名称</returns>
        public virtual string GetServerName()
        {
            return "TcpServer";
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                _tcpChannels.Dispose();

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~TcpServer() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion

        public static string GetClientIP(TcpClient s)
        {
            return ((IPEndPoint)s?.Client?.RemoteEndPoint)?.Address?.ToString();
        }

        public void Listen(int backlog)
        {
            try
            {
                this._ipTable.Clear();
                this._listener = new TcpListener(IPAddress.Any, this._port);
                this._listener.Start(backlog);  // TODO: 要捕获异常
            }
            catch (Exception ex)
            {
                string strError = "Listen() Start() 出现异常: " + ExceptionUtil.GetExceptionMessage(ex);
                LibraryManager.Log?.Error(strError);
                throw ex;
            }

            Console.WriteLine(this.GetServerName() + "成功监听于 " + this._port.ToString());

            var task = Task.Run(() => Run());
        }

        async Task Run()
        {
            while (this._isActive)
            {
                TcpClient tcpClient = null;
                string ip = "";
                try
                {
                    tcpClient = await this._listener.AcceptTcpClientAsync().ConfigureAwait(false);

                    // string ip = ((IPEndPoint)s.Client.RemoteEndPoint).Address.ToString();
                    ip = GetClientIP(tcpClient);
                    // ZManager.Log?.Info("*** ip [" + ip + "] request");

                    if (this._ipTable != null)
                    {
                        string error = this._ipTable.CheckIp(ip);
                        if (error != null)
                        {
                            tcpClient.Close();
                            // TODO: 可以在首次出现这种情况的时候记入错误日志
                            _compactLog?.Add("*** ip {0} 被禁止 Connect。原因: {1}", new object[] { ip, error });
                            // LibraryManager.Log?.Info("*** ip [" + ip + "] 被禁止 Connect。原因: " + error);
                            continue;
                        }
                    }

                    // throw new Exception("test");

                    Task task = // 用来消除警告 // https://stackoverflow.com/questions/18577054/alternative-to-task-run-that-doesnt-throw-warning
                    Task.Run(() =>
                            HandleClient(tcpClient,
                                () =>
                                {
                                    if (this._ipTable != null && string.IsNullOrEmpty(ip) == false)
                                    {
                                        this._ipTable.FinishIp(ip);
                                        ip = "";
                                    }
                                    tcpClient = null;
                                },
                                _cancelToken));

                }
                catch (Exception ex)
                {
                    if (tcpClient != null)
                    {
                        tcpClient.Close();
                        if (this._ipTable != null && string.IsNullOrEmpty(ip) == false)
                        {
                            this._ipTable.FinishIp(ip);
                            ip = "";
                        }
                        tcpClient = null;
                    }

                    if (this._isActive == false)
                        break;
                    LibraryManager.Log?.Error(this.GetServerName() + " AcceptTcpClientAsync() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                }
                Thread.Sleep(1);
            }
        }

        public void Close()
        {
            this._isActive = false;
            this._listener.Stop();
            _tcpChannels.Clear();
            TryFlushCompactLog();
        }

        public void TryClearBlackList()
        {
            if (this._ipTable == null)
                return;

            // 清理一次黑名单
            this._ipTable.ClearBlackList(TimeSpan.FromMinutes(10));
        }

        // 把紧凑日志写入日志文件
        public void TryFlushCompactLog()
        {
            _compactLog?.WriteToLog((text) =>
            {
                LibraryManager.Log?.Error(text);
            });
        }

        // 处理一个通道的通讯活动
        public virtual void HandleClient(TcpClient tcpClient,
            Action close_action,
            CancellationToken token)
        {
#if NO
            ZServerChannel channel = _zChannels.Add(tcpClient);
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
                        Result result = await SendResponse(response, tcpClient);
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
                    // tcpClient.Close();
                    // 清除全局结果集
                }
            }
            finally
            {
                _zChannels.Remove(channel);
                channel.Close();
                close_action.Invoke();
            }
#endif
        }
    }
}

