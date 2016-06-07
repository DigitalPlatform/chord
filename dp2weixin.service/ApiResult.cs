using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    // API返回结果
    public class ApiResult
    {
        public string errorInfo = "";

        /// <summary>
        /// -1:表示出错
        /// </summary>
        public long errorCode = 0;
    }

    public class WxUserResult
    {
        public WxUserItem userItem { get; set; }
        public ApiResult apiResult { get; set; }
    }

    public class ReservationResult:ApiResult
    {
        // 在借册
        public List<ReservationInfo> reservations { get; set; }

    }
}
