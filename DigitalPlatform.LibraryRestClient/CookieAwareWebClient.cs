using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace DigitalPlatform.LibraryRestClient
{
    /// <summary>
    /// 从WebClient继承
    /// </summary>
    public class CookieAwareWebClient : WebClient
    {
        /// 保持通道的恒定身份，是靠 HTTP 通讯的 Cookies 机制
        public CookieContainer CookieContainer { get; set; }

        public CookieAwareWebClient()
            : this(new CookieContainer())
        { }
        public CookieAwareWebClient(CookieContainer c)
        {
            this.CookieContainer = c;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = this.CookieContainer;
            }
            return request;
        }
    }
}