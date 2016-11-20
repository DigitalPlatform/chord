using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.HTTP
{
    public class HttpRequest : HttpMessage
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public string Path { get; set; } // either the Url, or the first regex group

        // 2016/11/20
        public string Version { get; set; }
#if NO
        public Dictionary<string, string> Headers { get; set; }

        public byte[] Content { get; set; }  // 2016/7/4
#endif

        public HttpRequest()
        {
            this.Headers = new Dictionary<string, string>();
        }

#if NO
        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(this.Content))
                if (!this.Headers.ContainsKey("Content-Length"))
                    this.Headers.Add("Content-Length", this.Content.Length.ToString());

            return string.Format("{0} {1} HTTP/1.0\r\n{2}\r\n\r\n{3}", this.Method, this.Url, string.Join("\r\n", this.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))), this.Content);
        }
#endif

        public string Dump(bool displayContent = false)
        {
            StringBuilder text = new StringBuilder();
            text.Append(string.Format("{0} {1} HTTP/1.0\r\n", this.Method, this.Url));
            text.Append(string.Join("\r\n", this.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            text.Append("\r\n\r\n");

            if (displayContent)
                text.Append(this.GetContentString());
            else
                text.Append("Content.Length=" + (this.Content == null ? 0 : this.Content.Length) + "\r\n");
            return text.ToString();
        }
    }
}
