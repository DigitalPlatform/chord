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

        // 信息
        public string info = "";

        // 返回的对象
        public object obj = "";
    }

    public class ChargeCommandResult:ApiResult
    {
        public List<ChargeCommand> cmds = null;
    }


    public class LibSetResult : ApiResult
    {
        public LibEntity lib { get; set; }
    }

    public class WxUserResult:ApiResult
    {
        public List<WxUserItem> users { get; set; }
    }

    public class ReservationResult:ApiResult
    {
        // 在借册
        public List<ReservationInfo> reservations { get; set; }

    }

    public class ItemReservationResult : ApiResult
    {
        public string reserRowHtml { get; set; }
    }

    public class BorrowInfoResult : ApiResult
    {
        // 在借册
        public List<BorrowInfo2> borrowInfos { get; set; }

        public string maxBorrowCount { get; set; }

        public string curBorrowCount { get; set; }

    }
}
