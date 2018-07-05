using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using log4net;

using DigitalPlatform.HTTP;
using DigitalPlatform;
using DigitalPlatform.Text;

namespace dp2Router
{
    public class HttpServer
    {
        #region Fields

        private int Port;
        private TcpListener Listener;
        // private HttpProcessor Processor;
        private bool IsActive = true;

        #endregion

        private static readonly ILog log = LogManager.GetLogger(typeof(HttpServer));

        #region Public Methods
        public HttpServer(int port
            // , List<Route> routes
            )
        {
            this.Port = port;
            // this.Processor = new HttpProcessor();

#if NO
            foreach (var route in routes)
            {
                this.Processor.AddRoute(route);
            }
#endif
        }

        static string GetClientIP(TcpClient s)
        {
            return ((IPEndPoint)s.Client.RemoteEndPoint).Address.ToString();
        }

        public async void Listen()
        {
            this.Listener = new TcpListener(IPAddress.Any, this.Port);
            this.Listener.Start();  // TODO: 要捕获异常

            Console.WriteLine("成功监听于 " + this.Port.ToString());

            while (this.IsActive)
            {
                try
                {
                    TcpClient tcpClient = await this.Listener.AcceptTcpClientAsync();

                    // string ip = ((IPEndPoint)s.Client.RemoteEndPoint).Address.ToString();
                    string ip = GetClientIP(tcpClient);
                    ServerInfo.WriteErrorLog("*** ip [" + ip + "] request");

#if NO
                    Thread thread = new Thread(() =>
                    {
                        // this.Processor.TestHandleClient(s);
                        TestHandleClient(tcpClient, ServerInfo._cancel.Token);
                    });
                    thread.Start();
#endif
                    Task.Run(() => TestHandleClient(tcpClient, ServerInfo._cancel.Token));

                    // TestHandleClient(tcpClient, ServerInfo._cancel.Token);
                }
                catch (Exception ex)
                {
                    if (this.IsActive == false)
                        break;
                    ServerInfo.WriteErrorLog("Listen() 出现异常: " + ExceptionUtil.GetExceptionMessage(ex));
                }
                Thread.Sleep(1);
            }
        }

        public void Close()
        {
            this.IsActive = false;
            this.Listener.Stop();
        }

        public async void TestHandleClient(TcpClient tcpClient,
            CancellationToken token)
        {
            HttpChannel channel = ServerInfo._httpChannels.Add(tcpClient);
            try
            {
                List<byte> cache = new List<byte>();
                string ip = "";
                Stream inputStream = tcpClient.GetStream();
                Stream outputStream = tcpClient.GetStream();

                try
                {
                    ip = GetClientIP(tcpClient);
                    channel.Touch();

                    int i = 0;
                    bool running = true;
                    while (running)
                    {
                        HttpRequest request = await HttpProcessor.GetIncomingRequest(inputStream,
                            cache,
                            (headers) =>
                            {
#if NO
                                StringBuilder text = new StringBuilder();
                                text.Append(string.Join("\r\n", headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
                                Console.WriteLine("=== headers ===\r\n" + text.ToString());
#endif

                                if (headers.ContainsKey("User-Agent") == false)
                                    return false;
                                if (headers["User-Agent"] != "dp2LibraryClient")
                                    throw new InvalidRequestException("请求不是来自 dp2LibraryClient");
                                return true;
                            },
                            token);
                        if (request == null)
                        {
                            Console.WriteLine("client close on request " + i);
                            break;
                        }
                        Console.WriteLine("request " + i);

                        TimeSpan timeout = GetTimeout(request);
                        if (timeout != TimeSpan.MinValue)
                            channel.Touch(timeout);
                        else
                            channel.Touch();

                        // 添加头字段 _dp2router_clientip
                        request.Headers.Add("_dp2router_clientip", ip);

                        // Console.WriteLine("=== request ===\r\n" + request.Dump());
                        // ServerInfo.WriteErrorLog("=== request ===\r\n" + request.Dump());

                        HttpResponse response = await ServerInfo.WebCall(request, "content", timeout);   // content text.utf-7
                        channel.Touch();
                        // string content = response.GetContentString();

                        // Console.WriteLine("=== response ===\r\n" + response.Dump());

                        await HttpProcessor.WriteResponseAsync(outputStream, response, token);
                        channel.Touch();

                        // Console.WriteLine("http version = '"+request.Version+"'");

                        if (request.Headers.ContainsKey("Connection"))
                        {
                            string strValue = request.Headers["Connection"];
                            if (strValue == "Keep-Alive")
                                running = true;
                            else
                                running = false;
                        }
                        else
                        {
                            if (StringUtil.CompareVersion(request.Version, "1.1") >= 0)
                                running = true; // HTTP 1.1 默认就是 Connection Keep-Alive。即便没有 Connection 头字段，也是 Keep-Alive 状态
                            else
                                running = false;
                        }

                        i++;
                    }
                }
                catch (InvalidRequestException ex)
                {
                    // 2016/11/20
                    ServerInfo.WriteErrorLog("ip:" + ip + " : " + ex.Message);
                }
                catch (Exception ex)
                {
                    // 2016/11/14
                    ServerInfo.WriteErrorLog("ip:" + ip + " TestHandleClient() 异常: " + ExceptionUtil.GetExceptionText(ex));
                }
                finally
                {
                    outputStream.Flush();
                    outputStream.Close();
                    outputStream = null;

                    inputStream.Close();
                    inputStream = null;
                }
            }
            finally
            {
                ServerInfo._httpChannels.Remove(channel);
            }
        }

        static string TIMEOUT_HEADER = "_timeout";

        // 从 HTTP header 里面剖析 _timeout field
        // return:
        //      TimeSpan.MinValue   当 Headers 里面没有 _timeout field 的时候返回这个
        //      其它  超时时间值
        static TimeSpan GetTimeout(HttpRequest request)
        {
            TimeSpan result = TimeSpan.MinValue;
            if (request.Headers.ContainsKey(TIMEOUT_HEADER))
            {
                string strTimeout = request.Headers[TIMEOUT_HEADER];
                if (string.IsNullOrEmpty(strTimeout) == false)
                {
                    // Console.WriteLine(TIMEOUT_HEADER + "=" + strTimeout);
                    if (TimeSpan.TryParse(strTimeout, out result) == true)
                        return result + TimeSpan.FromSeconds(5);    // 额外加上 5 秒
                }
            }

            return result;
        }

        #endregion

    }

    public class InvalidRequestException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="error"></param>
        /// <param name="strText"></param>
        public InvalidRequestException(string strText)
            : base(strText)
        {
        }
    }
}
