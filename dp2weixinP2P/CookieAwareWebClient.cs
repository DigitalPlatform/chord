using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace dp2weixin
{
    // 2019/5/29 注: 原来这个类是在 DigitalPlatform.LibraryRestClient 中。但为了这一个类引用 DigitalPlatform.LibraryRestClient.dll 不划算，所以这里把代码单独复制过来了
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