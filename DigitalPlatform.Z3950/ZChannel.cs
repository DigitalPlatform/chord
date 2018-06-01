using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalPlatform.Z3950
{
    // TODO: 注意提供 IDisposeable 接口。因为内含的 TcpClient 是 IDisposeable 的
    public class ZChannel
    {
        public TcpClient client = null;

        public const int DefaultPort = 210;

        public string m_strHostName = "";
        public int m_nPort = DefaultPort;

        bool m_bInitialized = false;

        // public event CommIdleEventHandle CommIdle = null;

        // 异步发送和接收
        //public byte[] baSend = null;
        //public byte[] baRecv = null;
        //public string strErrorString = "";
        //public int nErrorCode = 0;

        // TODO: 要实现 IDisposeable 接口，释放 ...
        internal AutoResetEvent eventClose = new AutoResetEvent(false);	// true : initial state is signaled 
        internal AutoResetEvent eventFinished = new AutoResetEvent(false);	// true : initial state is signaled 

        public int Timeout = 60 * 1000;   // 60秒

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
                this.m_bInitialized = false;
            }
        }


        public bool Connected
        {
            get
            {
                if (this.client == null)
                    return false;

                return this.client.Connected;
                /*
                if (this.client != null)
                    return true;

                return false;
                 * */
            }
        }

        public string HostName
        {
            get
            {
                return this.m_strHostName;
            }
        }

        public int Port
        {
            get
            {
                return this.m_nPort;
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

        async Task<RecvResult> SendAndRecvThread(byte[] baSend)
        {
            {
                Result result = await this.SimpleSendTcpPackage(
                    baSend,
                    baSend.Length);
                if (result.Value == -1 || result.Value == 1)
                    return new RecvResult { Value = -1, ErrorInfo = result.ErrorInfo };
            }

            {
                //byte[] baPackage = null;
                //int nRecvLen = 0;
                RecvResult result = await this.SimpleRecvTcpPackage();
                if (result.Value == -1)
                    return new RecvResult { Value = -1, ErrorInfo = result.ErrorInfo };

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
                this.eventFinished.Set();
                return result;
            }
        }

        public class Result
        {
            public int Value { get; set; }
            public string ErrorInfo { get; set; }
        }

        public class RecvResult : Result
        {
            public int Length { get; set; }
            public byte[] Package { get; set; }
        }

        // 发出请求包
        // return:
        //      -1  出错
        //      0   正确发出
        //      1   发出前，发现流中有未读入的数据
        public async Task<Result> SimpleSendTcpPackage(byte[] baPackage,
            int nLen)
        {
            Result result = new Result();

            if (client == null)
            {
                result.Value = -1;
                result.ErrorInfo = "client尚未初始化。请重新连接和检索。";
                return result;
            }

            if (this.client == null)
            {
                result.Value = -1;
                result.ErrorInfo = "用户中断";
                return result;
            }

            // TODO: 是否要关闭 NetworkStream !!!
            NetworkStream stream = client.GetStream();

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
                result.Value = -1;
                result.ErrorInfo = "send出错: " + ex.Message;
                // this.CloseSocket();
                return result;
            }

            return result;
        }

        // 接收响应包
        public async Task<RecvResult> SimpleRecvTcpPackage()
        {
            string strError = "";
            RecvResult result = new RecvResult();

            int nInLen;
            int wRet = 0;
            bool bInitialLen = false;

            Debug.Assert(client != null, "client为空");

            result.Package = new byte[4096];
            nInLen = 0;
            result.Length = 4096; //COMM_BUFF_LEN;

            while (nInLen < result.Length)
            {
                if (client == null)
                {
                    strError = "通讯中断";
                    goto ERROR1;
                }

                try
                {
                    wRet = await client.GetStream().ReadAsync(result.Package,
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
                    strError = "recv出错: " + ex.Message;
                    goto ERROR1;
                }
                catch (Exception ex)
                {
                    strError = "recv出错: " + ex.Message;
                    goto ERROR1;
                }

                if (wRet == 0)
                {
                    strError = "Closed by remote peer";
                    goto ERROR1;
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
            ERROR1:
            // this.CloseSocket();
            // baPackage = null;
            return new RecvResult { Value = -1, ErrorInfo = strError };
        }

        public void CloseSocket()
        {
            if (client != null)
            {
                TcpClient temp_client = client;
                this.client = null;

                try
                {
                    temp_client.Close();
                    goto END1;
                }
                catch
                {
                }

#if NO
                try
                {
                    NetworkStream stream = temp_client.GetStream();
                    stream.Close();
                }
                catch { }
                try
                {
                    temp_client.Close();
                }
                catch { }
#endif
            }

            END1:
            this.m_bInitialized = false;

            this.eventClose.Set();
        }
    }
}
