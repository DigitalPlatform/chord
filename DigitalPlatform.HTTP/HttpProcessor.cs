using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalPlatform.HTTP
{
    public static class HttpProcessor
    {
        // this formats the HTTP response...
        public static void WriteResponse(Stream stream, HttpResponse response)
        {
            if (response.Content == null)
            {
                response.Content = new byte[] { };
            }

            // default to text/html content type
            if (!response.Headers.ContainsKey("Content-Type"))
            {
                response.Headers["Content-Type"] = "text/html";
            }

            response.Headers["Content-Length"] = response.Content.Length.ToString();

            Write(stream, string.Format("HTTP/1.0 {0} {1}\r\n", response.StatusCode, response.ReasonPhrase));
            Write(stream, string.Join("\r\n", response.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            Write(stream, "\r\n\r\n");

            stream.Write(response.Content, 0, response.Content.Length);
        }

        public static async Task WriteResponseAsync(Stream stream,
            HttpResponse response,
            CancellationToken token)
        {
            if (response.Content == null)
            {
                response.Content = new byte[] { };
            }

            // default to text/html content type
            if (!response.Headers.ContainsKey("Content-Type"))
            {
                response.Headers["Content-Type"] = "text/html";
            }

            response.Headers["Content-Length"] = response.Content.Length.ToString();

            Write(stream, string.Format("HTTP/1.0 {0} {1}\r\n", response.StatusCode, response.ReasonPhrase));
            Write(stream, string.Join("\r\n", response.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            Write(stream, "\r\n\r\n");

            await stream.WriteAsync(response.Content, 0, response.Content.Length, token);
        }


        static int MAX_HEADER_LINES = 100;
        static int MAX_ENTITY_BYTES = 1024 * 1024;   // 1M bytes

        public delegate bool Delegate_verifyHeaders(Dictionary<string, string> headers);

        public static async Task<HttpRequest> GetIncomingRequest(Stream inputStream,
            List<byte> cache,
            Delegate_verifyHeaders proc_verify,
            CancellationToken token)
        {
            // Read Request Line
            string request = await ReadLineAsync(inputStream, cache, token);
            if (string.IsNullOrEmpty(request))
                return null;    // 表示前端已经切断通讯
#if NO
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
#endif
            List<string> tokens = MessageUtility.SplitFirstLine(request);
            if (tokens == null)
                throw new Exception("1 invalid http request line '" + request + "'");

            string method = tokens[0].ToUpper();
            string url = tokens[1];
            string protocolVersion = tokens[2];

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string line;
            while ((line = await ReadLineAsync(inputStream, cache, token)) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("1 invalid http header line: " + line);
                }
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                headers.Add(name, value);
                if (headers.Count > MAX_HEADER_LINES)
                    throw new Exception("headers 行数超过配额");
            }

            if (proc_verify != null)
            {
                if (proc_verify(headers) == false)
                    return null;
            }

            byte[] raw_content = null;
            if (headers.ContainsKey("Content-Length"))
            {
                int totalBytes = Convert.ToInt32(headers["Content-Length"]);

                if (totalBytes >= MAX_ENTITY_BYTES)
                    throw new Exception("Content-Length " + totalBytes + " 超过配额");

                int bytesLeft = totalBytes;
                byte[] bytes = new byte[totalBytes];

                while (bytesLeft > 0)
                {
                    byte[] buffer = new byte[bytesLeft > 1024 ? 1024 : bytesLeft];

                    int nRet = 0;
                    if (cache.Count > 0)
                    {
                        nRet = Math.Min(buffer.Length, cache.Count);
                        Array.Copy(cache.ToArray(), buffer, nRet);
                        cache.RemoveRange(0, nRet);
                    }

                    int n = 0;
                    if (nRet < buffer.Length)
                    {
                        // int n = inputStream.Read(buffer, 0, buffer.Length);
                        n = await inputStream.ReadAsync(buffer, nRet, buffer.Length - nRet, token);
                    }

                    buffer.CopyTo(bytes, totalBytes - bytesLeft);

                    bytesLeft -= nRet + n;
                }

                raw_content = bytes;
            }

            return new HttpRequest()
            {
                Method = method,
                Url = url,
                Version = protocolVersion,
                Headers = headers,
                Content = raw_content
            };
        }

        // 将一个请求发送给指定的地址，并获得响应
        public static HttpResponse WebCall(HttpRequest request,
            string target_url)
        {
            // http://localhost/dp2library/xe/basic
            UriBuilder builder = new UriBuilder(target_url);

            string strHostName = builder.Host;
            int nPort = builder.Port;
            if (nPort == -1)
                nPort = 80;

            TcpClient client = new TcpClient(AddressFamily.InterNetwork);
            client.Connect(strHostName, nPort);

            request.Url = builder.Path;
            WriteRequest(client.GetStream(), request);

            return GetResponse(client.GetStream());
        }

        // TODO: 可以根据 http header 中的 _timeout 实现超时机制
        // 将一个请求发送给指定的地址，并获得响应
        public static async Task<HttpResponse> WebCallAsync(HttpRequest request,
            string target_url,
            CancellationToken token)
        {
            // http://localhost/dp2library/xe/basic
            UriBuilder builder = new UriBuilder(target_url);

            string strHostName = builder.Host;
            int nPort = builder.Port;
            if (nPort == -1)
                nPort = 80;

            TcpClient client = new TcpClient(AddressFamily.InterNetwork);
            await client.ConnectAsync(strHostName, nPort);

            request.Url = builder.Path;
            await WriteRequestAsync(client.GetStream(), request, token);

            return await GetResponseAsync(client.GetStream(), token);
        }

        // 向 dp2library 发出请求
        private static void WriteRequest(Stream stream,
            HttpRequest request)
        {
            if (request.Content == null)
            {
                request.Content = new byte[] { };
            }

#if NO
            // default to text/html content type
            if (!response.Headers.ContainsKey("Content-Type"))
            {
                response.Headers["Content-Type"] = "text/html";
            }
#endif

            request.Headers["Content-Length"] = request.Content.Length.ToString();

            Write(stream, string.Format("{0} {1} HTTP/1.0\r\n", request.Method, request.Url));
            Write(stream, string.Join("\r\n", request.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            Write(stream, "\r\n\r\n");

            stream.Write(request.Content, 0, request.Content.Length);
        }

        // 向 dp2library 发出请求
        private static async Task WriteRequestAsync(Stream stream,
            HttpRequest request,
            CancellationToken token)
        {
            if (request.Content == null)
            {
                request.Content = new byte[] { };
            }

            request.Headers["Content-Length"] = request.Content.Length.ToString();

            await WriteAsync(stream,
                string.Format("{0} {1} HTTP/1.0\r\n", request.Method, request.Url),
                token);
            await WriteAsync(stream,
                string.Join("\r\n", request.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))),
                token);
            await WriteAsync(stream,
                "\r\n\r\n",
                token);

            await stream.WriteAsync(request.Content, 0, request.Content.Length, token);
        }


        // 从 dp2library 获得响应
        private static HttpResponse GetResponse(Stream inputStream)
        {
            // Read Response Line
            string first_line = Readline(inputStream);

#if NO
            // HTTP/1.1 404 Not Found
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("1 invalid http response line '"+request+"'");
            }
#endif
            List<string> tokens = MessageUtility.SplitFirstLine(first_line);
            if (tokens == null)
                throw new Exception("1 invalid http response line '" + first_line + "'");

            string version = tokens[0].ToUpper();
            string status_code = tokens[1];
            string reason_phrase = tokens[2];

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string line;
            while ((line = Readline(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("2 invalid http header line: " + line);
                }
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                headers.Add(name, value);
            }

            byte[] raw_content = null;
            if (headers.ContainsKey("Content-Length"))
            {
                int totalBytes = Convert.ToInt32(headers["Content-Length"]);
                int bytesLeft = totalBytes;
                byte[] bytes = new byte[totalBytes];

                while (bytesLeft > 0)
                {
                    byte[] buffer = new byte[bytesLeft > 1024 ? 1024 : bytesLeft];
                    int n = inputStream.Read(buffer, 0, buffer.Length);
                    buffer.CopyTo(bytes, totalBytes - bytesLeft);

                    bytesLeft -= n;
                }

                raw_content = bytes;
            }

            return new HttpResponse()
            {
                StatusCode = status_code,
                ReasonPhrase = reason_phrase,
                Headers = headers,
                Content = raw_content
            };
        }

        // 从 dp2library 获得响应
        private static async Task<HttpResponse> GetResponseAsync(Stream inputStream,
            CancellationToken token)
        {
            // Read Response Line
            string first_line = await ReadLineAsync0(inputStream, token);

#if NO
            // HTTP/1.1 404 Not Found
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("1 invalid http response line '"+request+"'");
            }
#endif
            List<string> tokens = MessageUtility.SplitFirstLine(first_line);
            if (tokens == null)
                throw new Exception("1 invalid http response line '" + first_line + "'");

            string version = tokens[0].ToUpper();
            string status_code = tokens[1];
            string reason_phrase = tokens[2];

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string line;
            while ((line = await ReadLineAsync0(inputStream, token)) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("2 invalid http header line: " + line);
                }
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                headers.Add(name, value);
            }

            byte[] raw_content = null;
            if (headers.ContainsKey("Content-Length"))
            {
                int totalBytes = Convert.ToInt32(headers["Content-Length"]);
                // TODO 观察配额
                int bytesLeft = totalBytes;
                byte[] bytes = new byte[totalBytes];

                while (bytesLeft > 0)
                {
                    byte[] buffer = new byte[bytesLeft > 1024 ? 1024 : bytesLeft];
                    int n = await inputStream.ReadAsync(buffer, 0, buffer.Length, token);
                    buffer.CopyTo(bytes, totalBytes - bytesLeft);

                    bytesLeft -= n;
                }

                raw_content = bytes;
            }

            return new HttpResponse()
            {
                StatusCode = status_code,
                ReasonPhrase = reason_phrase,
                Headers = headers,
                Content = raw_content
            };
        }


        static int MAX_LINE_CHARS = 500;

        private static string Readline(Stream stream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = stream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);

                if (data.Length >= MAX_LINE_CHARS)
                    throw new Exception("头字段行长度超过配额");
            }
            return data;
        }

        static async Task<string> ReadLineAsync0(Stream stream,
            CancellationToken token)
        {
            byte[] buffer = new byte[1];

            int next_char;
            string data = "";
            while (true)
            {
                // next_char = stream.ReadByte();

                int nRet = await stream.ReadAsync(buffer, 0, 1, token);
                if (nRet < 1)
                    break;
                next_char = buffer[0];

                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }

                data += Convert.ToChar(next_char);

                if (data.Length >= MAX_LINE_CHARS)
                    throw new Exception("头字段行长度超过配额");
            }
            return data;
        }

        /*
2016/11/20 10:41:15 ip:127.0.0.1 TestHandleClient() 异常: System.ObjectDisposedException:无法访问已释放的对象。
对象名:“System.Net.Sockets.NetworkStream”。
   在 System.Net.Sockets.NetworkStream.EndRead(IAsyncResult asyncResult)
   在 System.Threading.Tasks.TaskFactory`1.FromAsyncTrimPromise`1.Complete(TInstance thisRef, Func`3 endMethod, IAsyncResult asyncResult, Boolean requiresSynchronization)
--- 引发异常的上一位置中堆栈跟踪的末尾 ---
   在 System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   在 System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   在 System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   在 DigitalPlatform.HTTP.HttpProcessor.<ReadLineAsync>d__43.MoveNext() 位置 c:\chord\chord\DigitalPlatform.HTTP\HttpProcessor.cs:行号 471
--- 引发异常的上一位置中堆栈跟踪的末尾 ---
   在 System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   在 System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   在 System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   在 DigitalPlatform.HTTP.HttpProcessor.<GetIncomingRequest>d__8.MoveNext() 位置 c:\chord\chord\DigitalPlatform.HTTP\HttpProcessor.cs:行号 70
--- 引发异常的上一位置中堆栈跟踪的末尾 ---
   在 System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   在 System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   在 System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   在 dp2Router.HttpServer.<TestHandleClient>d__7.MoveNext() 位置 c:\chord\chord\dp2Router\HttpServer.cs:行号 117
         * */
        static async Task<string> ReadLineAsync(Stream stream,
    List<byte> cache,
    CancellationToken token)
        {
            byte[] buffer = new byte[4096];

            StringBuilder result = new StringBuilder();
            while (true)
            {
                if (cache.Count == 0)
                {
                    int nRet = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (nRet <= 0)
                        break;

                    // 追加到 cache 中
                    {
                        int i = 0;
                        foreach (byte c in buffer)
                        {
                            if (i >= nRet)
                                break;
                            cache.Add(c);
                            i++;
                        }

                    }
                }

                int remove_count = 0;
                try
                {
                    int i = 0;
                    foreach (byte c in cache)
                    {
                        if (c == '\n')
                        {
                            remove_count++;
                            goto END1;
                        }
                        if (c == '\r')
                        {
                            remove_count++;
                            continue;
                        }

                        result.Append(Convert.ToChar(c));
                        remove_count++;
                    }
                }
                finally
                {
                    cache.RemoveRange(0, remove_count);
                }

                if (result.Length >= MAX_LINE_CHARS)
                    throw new Exception("头字段行长度超过配额");
            }
        END1:
            return result.ToString();
        }

        private static void Write(Stream stream, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static async Task WriteAsync(Stream stream, string text, CancellationToken token)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            await stream.WriteAsync(bytes, 0, bytes.Length, token);
        }
    }
}
