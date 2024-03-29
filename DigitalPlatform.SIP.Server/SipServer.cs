﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using DigitalPlatform.Core;
using DigitalPlatform.Net;

namespace DigitalPlatform.SIP.Server
{
    /// <summary>
    /// SIP2 服务器
    /// </summary>
    public class SipServer : TcpServer
    {
        internal Hashtable _accountTable = new Hashtable(); // 限制每个账户的 LibraryChannel 最大通道数

        // 常量
        public const string DEFAULT_DATE_FORMAT = "yyyy-MM-dd";
        public const string DEFAULT_ENCODING_NAME = "UTF-8";
        public const bool DEFAULT_BOOKUIISTRICT = false;
        public const int DEFAULT_MAXCHANNELS = 2;
        public const bool DEFAULT_ISMANAGER = false;
        public const int DEFAULT_CHARGEDLIMIT = -1; // -1 表示不限制

        int _maxPackageLength = 4096;
        public int MaxPackageLength
        {
            get
            {
                return _maxPackageLength;
            }
            set
            {
                _maxPackageLength = value;
            }
        }
        public event ProcessSipRequestEventHandler ProcessRequest = null;

        public SipServer(int port) : base(port)
        {
            this.IpTable.MaxClientsPerIp = 100; // 每个前端 IP 最多允许 100 个通道
        }

        public override string GetServerName()
        {
            return "SIP2 服务器";
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

        static int DEFAULT_CLEAR_SECONDS = 10;  // TCP 连接后，登录以前，能存活的秒数。超过这个秒数就可能被管理线程 Close 和清除

        // 处理一个通道的通讯活动
        public async override void HandleClient(TcpClient tcpClient,
            Action close_action,
            CancellationToken token)
        {
            SipChannel channel = _tcpChannels.Add(tcpClient, () => { return new SipChannel(); }) as SipChannel;
            channel.CreateTime = channel.LastTime;  // 2022/3/29
            channel.Timeout = TimeSpan.FromSeconds(DEFAULT_CLEAR_SECONDS);  // 在登录以前，TCP 连接可以存活 DEFAULT_CLEAR_SECONDS 秒。DEFAULT_CLEAR_SECONDS 秒以后还不接到登录请求，TCP 连接会被自动清除。登录后，Timeout 则会被设置为实例参数 AutoClearSeconds 值

            List<byte> cache = new List<byte>();

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

                        byte[] response = null;

                        {
                            // TODO: 也可以在这里等待多少秒，然后超时以后，条件符合时，自动清除 TCP 通道。可以弥补管理线程轮次不及时的缺点
                            // 注意调用返回后如果发现返回 null 或者抛出了异常，调主要主动 Close 和重新分配 TcpClient
                            RecvResult result = await TcpChannel.SimpleRecvTcpPackage(tcpClient,
                                cache,
                                (package, start, length) =>
                                {
                                    return FindTerminator(package, start, length);
                                },
                                this.MaxPackageLength).ConfigureAwait(false); // 2018/10/10 add configure
                            if (result.Value == -1)
                            {
                                if (result.ErrorCode == "ConnectionAborted")
                                    Console.WriteLine("client close on request " + i);
                                else
                                    Console.WriteLine("recv error on request " + i + ": " + result.ErrorInfo);
                                break;
                            }
                            channel.Terminator = (byte)result.Terminator;
                            Console.WriteLine("request " + i);

                            // byte [] 转换为 string

                            channel.Touch();
                            channel.RequestCount++;
                            if (token != null && token.IsCancellationRequested)
                                return;

                            ProcessSipRequestEventArgs e = new ProcessSipRequestEventArgs { AccountTable = _accountTable };
                            e.Request = result.Package;
                            this.ProcessRequest(channel, e);
                            response = e.Response;
                        }

                        channel.Touch();
                        if (token != null && token.IsCancellationRequested)
                            return;

                        {
                            // 注意调用返回 result.Value == -1 情况下，要及时 Close TcpClient
                            Result result = await TcpChannel.SimpleSendTcpPackage(tcpClient,
        response,
        response.Length).ConfigureAwait(false); // 2018/10/10 add configure
                            channel.Touch();
                            if (result.Value == -1 || result.Value == 1)
                            {
                                Console.WriteLine("error on response " + i + ": " + result.ErrorInfo);
                                break;
                            }
                        }

                        i++;
                    }
                }
                catch (Exception ex)
                {
                    string strError = "ip:" + ip + " HandleClient() 异常: " + ExceptionUtil.GetExceptionText(ex);
                    LibraryManager.Log?.Error(strError);
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
                _tcpChannels.Remove(channel);
                // channel.InstanceName;channel.UserName;

                // 2022/3/22
                channel.SetUserName("", "", 0, _accountTable);

                channel.Close();
                if (close_action != null)
                    close_action.Invoke();
            }
        }

    }

    /// <summary>
    /// 处理 请求 事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ProcessSipRequestEventHandler(object sender,
        ProcessSipRequestEventArgs e);

    /// <summary>
    /// 处理请求事件的参数
    /// </summary>
    public class ProcessSipRequestEventArgs : EventArgs
    {
        public byte[] Request { get; set; }

        // 返回空表示需要立即 Close 通道
        public byte[] Response { get; set; }    // [out]
        // result.Value:
        // public Result Result = new Result();    // [out]

        // 2022/3/22
        public Hashtable AccountTable { get; set; }
    }
}
