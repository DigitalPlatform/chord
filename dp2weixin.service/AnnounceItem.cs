using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    public class BbResult : ApiResult
    {
        public List<BbItem> items { get; set; }

    }

    public class BbItem
    {
        public string id { get; set; }
        public string title { get; set; }
        public string content { get; set; }

        public string publishTime { get; set; } // 2016-6-20 jane 发布时间，服务器消息的时间
    }
}
