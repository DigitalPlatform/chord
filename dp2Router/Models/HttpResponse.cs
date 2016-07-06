using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Router.Models
{
#if NO
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

    public class HttpResponse
    {
        public string StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public byte[] Content { get; set; }

        public Dictionary<string, string> Headers { get; set; }

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

        public HttpResponse()
        {
            this.Headers = new Dictionary<string, string>();
        }

        // informational only tostring...
        public override string ToString()
        {
            return string.Format("HTTP status {0} {1}", this.StatusCode, this.ReasonPhrase);
        }
    }


#endif
}
