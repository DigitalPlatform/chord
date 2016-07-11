using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.HTTP
{
    public enum HttpStatusCode
    {
        // for a full list of status codes, see..
        // https://en.wikipedia.org/wiki/List_of_HTTP_status_codes

        Continue = 100,

        Ok = 200,
        Created = 201,
        Accepted = 202,
        MovedPermanently = 301,
        Found = 302,
        NotModified = 304,
        BadRequest = 400,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        InternalServerError = 500
    }

    public class HttpResponse : HttpMessage
    {
        public string StatusCode { get; set; }
        public string ReasonPhrase { get; set; }

#if NO
        public string ContentAsUTF8
        {
            set
            {
                this.setContent(value, encoding: Encoding.UTF8);
            }
        }

        public void setContent(string content, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            Content = encoding.GetBytes(content);
        }
#endif

        public HttpResponse()
        {
            this.Headers = new Dictionary<string, string>();
        }

        public HttpResponse(string status_code, 
            string reason_phrase,
            string content)
        {
            this.StatusCode = status_code;
            this.ReasonPhrase = reason_phrase;
            this.Headers = new Dictionary<string, string>() {
            { "Content-Type","text/plain; charset=utf-8"}
            };
            this.SetContentString(content);
        }

        // informational only tostring...
        public override string ToString()
        {
            return string.Format("HTTP status {0} {1}", this.StatusCode, this.ReasonPhrase);
        }

        public string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.Append(string.Format("HTTP/1.0 {0} {1}\r\n", this.StatusCode, this.ReasonPhrase));
            text.Append(string.Join("\r\n", this.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            text.Append("\r\n\r\n");

            text.Append(this.GetContentString());
            return text.ToString();
        }
    }

}
