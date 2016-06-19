using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    public class AnnouncementResult : ApiResult
    {
        public List<AnnouncementItem> items { get; set; }

    }

    public class AnnouncementItem
    {
        public string id { get; set; }
        public string title { get; set; }
        public string content { get; set; }
    }
}
