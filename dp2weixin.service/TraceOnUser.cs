using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    public class TracingOnUser
    {
        public string WeixinId { get; set; }
        public bool IsAdmin = false; // 是否是数字平台管理员
    }
}
