using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.SIP2;

namespace DigitalPlatform.SIP2
{
    public class TcpClientWrapper
    {
        private TcpClient _client = null;
        private NetworkStream _networkStream = null;


        #region 一些配置参数
        // 字符集
        public Encoding Encoding { get; set; }
        // 命令结束符
        // public char MessageTerminator { get; set; }

        // SIP Server Url 与 Port
        public string SIPServerUrl { get; set; }
        public int SIPServerPort { get; set; }

        #endregion

        public TcpClientWrapper()
        {
            this.SetDefaultParameter();
        }

        public TcpClientWrapper(TcpClient client)
        {
            this._client = client;
            this._networkStream = client.GetStream();
            this.SetDefaultParameter();
        }

        public TcpClientWrapper(Encoding encoding)
        {
            this.Encoding = encoding;
        }

        // 设置缺省参数;
        public void SetDefaultParameter()
        {
            this.Encoding = Encoding.UTF8;
            // this.MessageTerminator = (char)13;
        }

        public bool Connection(string serverUrl, int port, out string error)
        {
            error = "";

            this.SIPServerUrl = serverUrl;
            this.SIPServerPort = port;

            // 先进行关闭
            this.Close();

            try
            {
                // 这段代码当ip地址对应的服务器没有对应域名时，会抛异常。例如腾讯云的几台服务器
                // IPAddress ipAddress = IPAddress.Parse(this.SIPServerUrl);
                // string hostName = Dns.GetHostEntry(ipAddress).HostName;
                // TcpClient client = new TcpClient(hostName, this.SIPServerPort); 

                TcpClient client = new TcpClient();
                client.Connect(IPAddress.Parse(this.SIPServerUrl), this.SIPServerPort);
                this._client = client;
                this._networkStream = client.GetStream();
            }
            catch (Exception ex)
            {
                error = "连接服务器失败:" + ex.Message;
                return false;
            }

            return true;
        }

        // 关闭通道
        public void Close()
        {
            if (_networkStream != null)
            {
                //您必须先关闭 NetworkStream 您何时通过发送和接收数据。 关闭 TcpClient 不会释放 NetworkStream。
                _networkStream.Close();
                _networkStream = null;
                LibraryManager.Log?.Info("关闭NetworkStream完成");
            }

            if (_client != null)
            {
                /*
                TCPClient.Close 释放此 TcpClient 实例本身，并请求关闭基础 TCP 连接（被封装的Socket）。
                TCPClient.Client.Close 关闭 被封装的Socket 连接并释放所有关联此Socket的资源。
                TCPClient.Client.Shutdown 禁用被封装的 Socket 上的发送和接收。
                 */

                //this._client.Client.Shutdown(SocketShutdown.Both);
                //this._client.Client.Close(); //
                //LogManager.Logger.Info("关闭TcpClient封装的Socket连接完成");



                this._client.Close();
                LibraryManager.Log?.Info("关闭TcpClient实例连接完成");

                this._client = null;
            }
        }

        // 发送消息
        public int SendMessage(string sendMsg,
            out string error)
        {
            error = "";

            if (this._client == null)
            {
                error = "_client对象不能为null。";
                return -1;
            }

            if (this._networkStream == null)
            {
                error = "_networkStream对象不能为null。";
                return -1;
            }

            try
            {
                if (this._networkStream.DataAvailable == true)
                {
                    error = "异常：发送前发现流中有未读的数据!";
                    return -1;
                }

                // 2018/7/28
                {
                    char tail_char = sendMsg[sendMsg.Length - 1];
                    if (tail_char != '\r' && tail_char != '\n')
                        sendMsg += '\r';
                }

                byte[] baPackage = this.Encoding.GetBytes(sendMsg);

                this._networkStream.Write(baPackage, 0, baPackage.Length);
                this._networkStream.Flush();//刷新当前数据流中的数据
                return 0;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return -1;
            }
        }

        static Tuple<int, byte> FindTerminator(byte[] buffer, int start, int length)
        {
            for (int i = start; i < start + length; i++)
            {
                byte b = buffer[i];
                if (b == '\r' || b == '\n')
                    return new Tuple<int, byte>(i + 1, b);
            }

            return new Tuple<int, byte>(0, (byte)0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recvMsg"></param>
        /// <param name="error"></param>
        /// <returns>
        /// <para> 0 正确 </para>
        /// <para> -1 错误 </para>
        /// <para> -2 空消息 </para>
        /// </returns>
        // 接收消息
        public int RecvMessage(out string recvMsg,
            out string error)
        {
            error = "";
            recvMsg = "";

            if (this._client == null)
            {
                error = "_client对象不能为null。";
                return -1;
            }

            if (this._networkStream == null)
            {
                error = "_networkStream对象不能为null。";
                return -1;
            }

            int offset = 0; //偏移量
            int nRet = 0;

            int nPackageLength = SIPConst.COMM_BUFF_LEN; //1024
            byte[] baPackage = new byte[nPackageLength];

            while (offset < nPackageLength)
            {
                if (this._client == null)
                {
                    error = "通讯中断";
                    goto ERROR1;
                }

                try
                {
                    nRet = this._networkStream.Read(baPackage,
                        offset,
                        baPackage.Length - offset);
                }
                catch (SocketException ex)
                {
                    // ??这个什么错误码
                    if (ex.ErrorCode == 10035)
                    {
                        System.Threading.Thread.Sleep(100);
                        continue;
                    }

                    error = ExceptionUtil.GetDebugText(ex);
                    goto ERROR1;
                }
                catch (System.IO.IOException ex1)
                {
                    error = ex1.Message;
                    goto ERROR1;
                }
                catch (Exception ex)
                {
                    error = ExceptionUtil.GetDebugText(ex);
                    goto ERROR1;
                }

                if (nRet == 0) //返回值为0
                {
                    error = "Closed by remote peer";
                    goto ERROR1;
                }

                // 得到包的长度
                if (nRet >= 1 || offset >= 1)
                {
#if NO
                    //没有找到结束符，继续读
                    int nIndex = Array.IndexOf(baPackage, (byte)this.MessageTerminator);
                    if (nIndex != -1)
                    {
                        nPackageLength = nIndex;
                        break;
                    }
#endif

                    Tuple<int, byte> ret = FindTerminator(baPackage, offset, nRet);
                    if (ret.Item1 != 0)
                    {
                        nPackageLength = ret.Item1;
                        break;
                    }
#if NO
                    //流中没有数据了
                    if (this._networkStream.DataAvailable == false)
                    {
                        nPackageLength = offset + nRet;
                        break;
                    }
#endif
                }

                offset += nRet;
                if (offset >= baPackage.Length)
                {
                    // 扩大缓冲区
                    byte[] temp = new byte[baPackage.Length + SIPConst.COMM_BUFF_LEN];//1024
                    Array.Copy(baPackage, 0, temp, 0, offset);
                    baPackage = temp;
                    nPackageLength = baPackage.Length;
                }
            }

            // 最后规整缓冲区尺寸，如果必要的话
            if (baPackage.Length > nPackageLength)
            {
                byte[] temp = new byte[nPackageLength];
                Array.Copy(baPackage, 0, temp, 0, nPackageLength);
                baPackage = temp;
            }

            recvMsg = this.Encoding.GetString(baPackage);
            return 0;

            ERROR1:
            LibraryManager.Log?.Error(error);
            baPackage = null;
            return -1;
        }
    }
}
