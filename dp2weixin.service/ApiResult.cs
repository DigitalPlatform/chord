﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    // API返回结果
    public class ApiResult
    {
        // 错误码
        // 0:表示成功
        // -1:表示出错
        public long errorCode = 0;

        // 错误信息
        public string errorInfo = "";

        // 普通提示信息
        public string info = "";

        // 返回的对象
        public object obj = "";
    }

    public class SetBiblioResult : ApiResult
    {
        public string biblioPath = "";
        public string biblioTimestamp = "";
    }

    public class SetReaderInfoResult:ApiResult
    {
        public string recPath = "";
        public string timestamp = "";
    }

    public class GetVerifyCodeResult : ApiResult
    {
        public string verifyCode = "";
    }

    public class ChargeCommandResult:ApiResult
    {
        public List<ChargeCommand> cmds = null;
    }


    public class LibSetResult : ApiResult
    {
        public LibEntity lib { get; set; }
    }

    // 绑定帐户返回结果结构
    public class WxUserResult:ApiResult
    {
        // 对应的绑定帐户集合
        public List<WxUserItem> users { get; set; }
    }

    public class UserMessageResult : ApiResult
    {
        public List<UserMessageItem> messages { get; set; }
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
