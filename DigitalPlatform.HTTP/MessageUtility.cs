using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Message;

namespace DigitalPlatform.HTTP
{
    // Message 和 HTTP 转换的实用函数
    public static class MessageUtility
    {
        // 根据 HttpRequest 构造 WebData
        public static WebData BuildWebData(HttpRequest request)
        {
            StringBuilder text = new StringBuilder();

            text.Append(string.Format("{0} {1} HTTP/1.0\r\n", request.Method, request.Url));
            text.Append(string.Join("\r\n", request.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            text.Append("\r\n\r\n");

            WebData data = new WebData();
            data.Headers = text.ToString();
            data.Content = request.Content;

            return data;
        }

        // 根据 HttpResponse 构造 WebData
        public static WebData BuildWebData(HttpResponse response)
        {
            StringBuilder text = new StringBuilder();
            text.Append(string.Format("HTTP/1.0 {0} {1}\r\n", response.StatusCode, response.ReasonPhrase));
            text.Append(string.Join("\r\n", response.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            text.Append("\r\n\r\n");

            WebData data = new WebData();
            data.Headers = text.ToString();
            data.Content = response.Content;

            return data;
        }

        // 将第一行根据空格切割为三个部分
        public static List<string> SplitFirstLine(string request)
        {
            string[] tokens = request.Split(' ');
            if (tokens.Length < 3)
                return null;    // 格式不正确
            List<string> results = new List<string>();
            results.Add(tokens[0]);
            results.Add(tokens[1]);
            string[] rest = new string[tokens.Length - 2];
            Array.Copy(tokens, 2, rest, 0, tokens.Length - 2);
            results.Add(string.Join(" ", rest));

            return results;
        }

        // 根据 WebData 构造 HttpResponse
        public static HttpResponse BuildHttpResponse(WebData data)
        {
            string[] lines = data.Headers.Replace("\r\n", "\n").Split(new char[] { '\n' });

            // 第一行
            string first_line = lines[0];

#if NO
            string[] tokens = request.Split(' ');
            if (tokens.Length < 3)
            {
                throw new Exception("2 invalid http response line '" + request + "'");
            }

            string protocolVersion = tokens[0];
            string status_code = tokens[1];
            string[] rest = new string[tokens.Length - 2];
            Array.Copy(tokens, 2, rest, 0, tokens.Length - 2);
            string reason_phrase = string.Join(" ", rest);
#endif
            List<string> tokens = SplitFirstLine(first_line);
            if (tokens == null)
                throw new Exception("2 invalid http response line '" + first_line + "'");

            string protocolVersion = tokens[0];
            string status_code = tokens[1];
            string reason_phrase = tokens[2];

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            int i = 0;
            foreach (string line in lines)
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }

                if (line.Equals(""))
                {
                    break;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("3 invalid http header line: " + line);
                }
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                headers.Add(name, value);

                i++;
            }

            return new HttpResponse()
            {
                StatusCode = status_code,
                ReasonPhrase = reason_phrase,
                Headers = headers,
                Content = data.Content
            };
        }

        // 根据 WebData 构造 HttpRequest
        public static HttpRequest BuildHttpRequest(WebData data)
        {
            string[] lines = data.Headers.Replace("\r\n", "\n").Split(new char[] { '\n' });

            // 第一行
            string request = lines[0];

#if NO
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
#endif
            List<string> tokens = SplitFirstLine(request);
            if (tokens == null)
                throw new Exception("2 invalid http request line '" + request + "'");

            string method = tokens[0].ToUpper();
            string url = tokens[1];
            string protocolVersion = tokens[2];

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            int i = 0;
            foreach (string line in lines)
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }

                if (line.Equals(""))
                {
                    break;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("4 invalid http header line: " + line);
                }
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                headers.Add(name, value);

                i++;
            }

            return new HttpRequest()
            {
                Method = method,
                Url = url,
                Headers = headers,
                Content = data.Content
            };
        }
    }
}
