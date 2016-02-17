using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin
{
    public static class WeiXinClientUtil
    {
        /// <summary>
        /// 微信的CreateTime是当前与1970-01-01 00:00:00之间的秒数
        /// </summary>
        /// <param name=“dt”></param>
        /// <returns></returns>
        public static string DateTimeToLongString(this DateTime dt)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            //intResult = (time- startTime).TotalMilliseconds;
            long t = (dt.Ticks - startTime.Ticks) / 10000000;            //现在是10位，除10000调整为13位
            return t.ToString();
        }


        public static string GetPostXmlToWeiXinGZH(string message)
        {
            string xml = @"<xml>
                    <ToUserName>123456789</ToUserName>
                    <FromUserName>00002</FromUserName>
                    <CreateTime>" + WeiXinClientUtil.DateTimeToLongString(DateTime.Now) + "</CreateTime>"
                    + "<MsgType>text</MsgType>"
                    + "<Content>" + message + "</Content>"
                    + @"<MsgId>" + DateTime.Now.Ticks + "</MsgId>"
                    + "</xml>";
            return xml;
        }
    }
}
