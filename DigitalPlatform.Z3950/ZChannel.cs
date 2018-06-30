﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalPlatform.Z3950
{
    /// <summary>
    /// 用于 Z39.50 通讯的通道。处理包的发送和接收。
    /// </summary>
    public class ZChannel : IDisposable
    {
        public TcpClient _client = new TcpClient();

        public const int DefaultPort = 210;

        public string _hostName = "";
        public int _port = DefaultPort;

        bool m_bInitialized = false;

        // public int Timeout = 60 * 1000;   // 60秒

        public void Dispose()
        {
            if (_client != null)
                _client.Dispose();
        }

        /// <summary>
        /// 是否被Z39.50初始化
        /// </summary>
        public bool Initialized
        {
            get
            {
                return this.m_bInitialized;
            }
            set
            {
                this.m_bInitialized = value;
            }
        }


        public bool Connected
        {
            get
            {
                if (this._client == null)
                    return false;

                try
                {
                    return this._client.Connected;
                }
                catch
                {
                    return false;
                }
            }
        }

        public string HostName
        {
            get
            {
                return this._hostName;
            }
        }

        public int Port
        {
            get
            {
                return this._port;
            }
        }

#if NO
        // 线程connect()到主机
        public int NewConnectSocket(string strHostName,
            int nPort,
            out string strError)
        {
            strError = "";

            this.m_strHostName = strHostName;
            this.m_nPort = nPort;


            // 在线程之前试探Close();
            this.CloseSocket();

            this.eventClose.Reset();
            this.eventFinished.Reset();

            this.strErrorString = "";
            this.nErrorCode = 0;


            Thread clientThread = new Thread(new ThreadStart(ConnectThread));
            clientThread.Start();

            // 等待线程结束
            WaitHandle[] events = new WaitHandle[2];
            events[0] = this.eventClose;
            events[1] = this.eventFinished;

            int nIdleTimeCount = 0;
            int nIdleTicks = 100;

            REDO:
            DoIdle();

            int index = 0;
            try
            {
                index = WaitHandle.WaitAny(events, nIdleTicks, false);
            }
            catch (System.Threading.ThreadAbortException ex)
            {
                strError = "线程被杀死";
                return -1;
            }

            if (index == WaitHandle.WaitTimeout)
            {
                nIdleTimeCount += nIdleTicks;

                if (nIdleTicks >= this.Timeout)
                {
                    // 超时
                    strError = "超时 (" + this.Timeout + "毫秒)";
                    return -1;
                }

                goto REDO;
            }
            else if (index == 0)
            {
                // 得到Close信号
                strError = "通道被切断";
                return -1;
            }
            else
            {
                // 得到finish信号
                if (nErrorCode != 0)
                {
                    strError = this.strErrorString;
                    return nErrorCode;
                }
                return 0;
            }

            return -1;
        }
#endif


        public async Task<Result> Connect(string host_name, int port = 210)
        {
            try
            {
                this._hostName = host_name;
                this._port = port;
                await _client.ConnectAsync(host_name, port);
                // client.NoDelay = true;
                return new Result();
            }
            catch (Exception ex)  // SocketException
            {
                return new Result { Value = -1, ErrorInfo = "Connect出错: " + ex.Message };
            }
        }

#if NO
        // 新启动一个线程的SendAndRecv
        public int SendAndRecv(byte[] baSend,
            out byte[] baRecv,
            out int nRecvLength,
            out string strError)
        {
            baRecv = null;
            nRecvLength = 0;
            strError = "";

            this.eventClose.Reset();
            this.eventFinished.Reset();

            this.baSend = baSend;
            this.strErrorString = "";
            this.nErrorCode = 0;

            Thread clientThread = new Thread(new ThreadStart(SendAndRecvThread));
            clientThread.Start();

            // 等待线程结束
            WaitHandle[] events = new WaitHandle[2];

            events[0] = this.eventClose;
            events[1] = this.eventFinished;

            int nIdleTimeCount = 0;
            int nIdleTicks = 100;

            REDO:
            DoIdle();


            int index = 0;
            try
            {
                index = WaitHandle.WaitAny(events, nIdleTicks, false);
            }
            catch (System.Threading.ThreadAbortException ex)
            {
                strError = "线程被杀死";
                goto ERROR1;
            }

            if (index == WaitHandle.WaitTimeout)
            {
                nIdleTimeCount += nIdleTicks;

                if (nIdleTicks >= this.Timeout)
                {
                    // 超时
                    strError = "超时 (" + this.Timeout + "毫秒)";
                    return -1;
                }

                goto REDO;
            }
            else if (index == 0)
            {
                // 得到Close信号
                strError = "通道被切断";
                goto ERROR1;
            }
            else
            {
                // 得到finish信号
                if (nErrorCode != 0)
                {
                    if (nErrorCode == -1)
                        this.CloseSocket();

                    strError = this.strErrorString;
                    return nErrorCode;
                }

                baRecv = this.baRecv;
                nRecvLength = baRecv.Length;
                return 0;
            }

            ERROR1:
            this.CloseSocket();
            return -1;
        }
#endif

        public async Task<RecvResult> SendAndRecv(byte[] baSend)
        {
            {
                Result result = await SimpleSendTcpPackage(this._client,
                    baSend,
                    baSend.Length);
                if (result.Value == -1 || result.Value == 1)
                {
                    this.CloseSocket();
                    return new RecvResult(result)
                    {
                        Value = -1,
                        //ErrorInfo = result.ErrorInfo,
                        //ErrorCode = result.ErrorCode
                    };
                }
            }

            {
                //byte[] baPackage = null;
                //int nRecvLen = 0;
                // 注意调用返回后如果发现出错，调主要主动 Close 和重新分配 TcpClient
                RecvResult result = await SimpleRecvTcpPackage(this._client);
                if (result.Value == -1)
                {
                    this.CloseSocket();
                    return new RecvResult(result);
                }

#if DEBUG
                if (result.Package != null)
                {
                    Debug.Assert(result.Package.Length == result.Length, "");
                }
                else
                {
                    Debug.Assert(result.Length == 0, "");
                }
#endif

                // this.baRecv = result.Package;
                // this.eventFinished.Set();
                return result;
            }
        }

        public class RecvResult : Result
        {
            public int Length { get; set; }
            public byte[] Package { get; set; }

            public RecvResult()
            {

            }

            public RecvResult(Result source)
            {
                Result.CopyTo(source, this);
            }

            public override string ToString()
            {
                StringBuilder text = new StringBuilder(base.ToString());
                text.Append("Package=" + this.Package + "\r\n");
                text.Append("Length=" + this.Length + "\r\n");
                return text.ToString();
            }
        }

        // 发出请求包
        // return:
        //      -1  出错
        //      0   正确发出
        //      1   发出前，发现流中有未读入的数据
        public static async Task<Result> SimpleSendTcpPackage(
            TcpClient _client,
            byte[] baPackage,
            int nLen)
        {
            Result result = new Result();

            if (_client == null)
            {
                result.Value = -1;
                result.ErrorInfo = "client尚未初始化。请重新连接和检索。";
                return result;
            }

            if (_client == null)
            {
                result.Value = -1;
                result.ErrorInfo = "用户中断";
                return result;
            }

            // TODO: 是否要关闭 NetworkStream !!!
            NetworkStream stream = _client.GetStream();

            if (stream.DataAvailable == true)
            {
                result.Value = 1;
                result.ErrorInfo = "发送前发现流中有未读的数据";
                return result;
            }

            try
            {
                // stream.Write(baPackage, 0, nLen);
                await stream.WriteAsync(baPackage, 0, nLen);
            }
            catch (Exception ex)
            {
                if (ex is IOException && ex.InnerException is SocketException)
                {
                    // "ConnectionAborted"
                    result.ErrorCode = ((SocketException)ex.InnerException).SocketErrorCode.ToString();
                }

                result.Value = -1;
                result.ErrorInfo = "send出错: " + ex.Message;
                // this.CloseSocket();
                return result;
            }

            return result;
        }

        // 接收响应包
        // 注意调用返回后如果发现出错，调主要主动 Close 和重新分配 TcpClient
        public static async Task<RecvResult> SimpleRecvTcpPackage(TcpClient _client)
        {
            // string strError = "";
            RecvResult result = new RecvResult();

            int nInLen;
            int wRet = 0;
            bool bInitialLen = false;

            Debug.Assert(_client != null, "client为空");

            result.Package = new byte[4096];
            nInLen = 0;
            result.Length = 4096; //COMM_BUFF_LEN;

            while (nInLen < result.Length)
            {
                if (_client == null)
                {
                    return new RecvResult
                    {
                        Value = -1,
                        ErrorInfo = "通讯中断"
                    };
                }

                try
                {
                    wRet = await _client.GetStream().ReadAsync(result.Package,
                        nInLen,
                        result.Package.Length - nInLen);
                }
                catch (SocketException ex)
                {

                    if (ex.ErrorCode == 10035)
                    {
                        System.Threading.Thread.Sleep(100);
                        continue;
                    }
                    return new RecvResult
                    {
                        Value = -1,
                        ErrorInfo = "recv出错1: " + ex.Message,
                        // "ConnectionAborted"
                        ErrorCode = ((SocketException)ex).SocketErrorCode.ToString()
                    };
                }
                catch (Exception ex)
                {
                    if (ex is IOException && ex.InnerException is SocketException)
                    {
                        // "ConnectionAborted"
                        result.ErrorCode = ((SocketException)ex.InnerException).SocketErrorCode.ToString();
                    }
                    result.ErrorInfo = "recv出错2: " + ex.Message;
                    result.Value = -1;
                    return result;
                }

                if (wRet == 0)
                {
                    return new RecvResult
                    {
                        Value = -1,
                        ErrorCode = "Closed",
                        ErrorInfo = "Closed by remote peer"
                    };
                }

                // 得到包的长度

                if ((wRet >= 1 || nInLen >= 1)
                    && bInitialLen == false)
                {
                    bool bRet = BerNode.IsCompleteBER(result.Package,
                        0,
                        nInLen + wRet,
                        out long remainder);
                    if (bRet == true)
                    {
                        result.Length = nInLen + wRet;
                        break;
                    }
                }

                nInLen += wRet;
                if (nInLen >= result.Package.Length
                    && bInitialLen == false)
                {
                    // 扩大缓冲区
                    byte[] temp = new byte[result.Package.Length + 4096];
                    Array.Copy(result.Package, 0, temp, 0, nInLen);
                    result.Package = temp;
                    result.Length = result.Package.Length;
                }
            }

            // 最后规整缓冲区尺寸，如果必要的话
            if (result.Package.Length > result.Length)
            {
                byte[] temp = new byte[result.Length];
                Array.Copy(result.Package, 0, temp, 0, result.Length);
                result.Package = temp;
            }

            return result;
#if NO
            ERROR1:
            // this.CloseSocket();
            // baPackage = null;
            return new RecvResult
            {
                Value = -1,
                ErrorInfo = strError,
                ErrorCode = result.ErrorCode
            };
#endif
        }

        // 流中是否还有未读入的数据?
        public bool DataAvailable
        {
            get
            {
                if (_client == null)
                    return false;

                // TODO: 是否要关闭 NetworkStream !!!
                NetworkStream stream = _client.GetStream();

                if (stream == null)
                    return false;

                bool bOldBlocking = this._client.Client.Blocking;
                this._client.Client.Blocking = true;
                try
                {

                    return stream.DataAvailable;
                }
                finally
                {
                    this._client.Client.Blocking = bOldBlocking;
                }
            }
        }

        // TODO: 增加一个事件，让外面知晓这里发生了 Close()。这样便于外面自动跟随清除 TargetInfo
        public void CloseSocket()
        {
#if NO
            if (_client != null)
            {
                TcpClient temp_client = _client;
                this._client = null;

                try
                {
                    temp_client.Close();
                    goto END1;
                }
                catch
                {
                }
            }
#endif
            if (_client != null)
            {
                try
                {
                    _client.Close();
                }
                catch
                {
                }
                finally
                {
                    this._client = new TcpClient(); // 如果 Close 之后不重新 new，则会遇到 NullException
                    this.Initialized = false;
                }
            }
            // this.eventClose.Set();
        }

    }
}
