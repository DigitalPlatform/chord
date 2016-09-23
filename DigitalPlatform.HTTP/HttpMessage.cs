using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.HTTP
{
    public class HttpMessage
    {
        public byte[] Content { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public string GetContentString(Encoding encoding = null)
        {
            if (this.Content == null || this.Content.Length == 0)
                return null;

            if (encoding == null)
                encoding = Encoding.UTF8;

            return encoding.GetString(this.Content);
        }

        public void SetContentString(string content, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            this.Content = encoding.GetBytes(content);
        }

    }
}
