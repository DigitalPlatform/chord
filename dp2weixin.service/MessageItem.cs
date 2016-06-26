using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    public class MessageResult : ApiResult
    {
        public List<MessageItem> items { get; set; }

        public string worker { get; set; }
    }

    public class MessageItem
    {
        public string id { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string contentFormat { get; set; }  // 2016-6-20 text/markdown
        public string contentHtml { get; set; }  // 2016-6-20 html形态
        public string publishTime { get; set; } // 2016-6-20 jane 发布时间，服务器消息的时间

        public string creator { get; set; }  //创建消息的工作人员帐户

        public string subject { get; set; } // 栏目
        public string remark { get; set; } //注释
    }
}
