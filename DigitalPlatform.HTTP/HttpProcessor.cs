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

        public static HttpRequest GetIncomingRequest(Stream inputStream
            // , Stream outputStream
            )
        {
            // Read Request Line
            string request = Readline(inputStream);

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
            while ((line = Readline(inputStream)) != null)
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

            return new HttpRequest()
            {
                Method = method,
                Url = url,
                Headers = headers,
                Content = raw_content
            };
        }

        // 将一个请求发送给指定的地址，并获得响应
        public static HttpResponse WebCall(HttpRequest request, string target_url)
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
            }
            return data;
        }

        private static void Write(Stream stream, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
