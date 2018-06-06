using System;
using System.Collections.Generic;
using System.Text;

namespace WebZ.Server
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
        public string message = "";

        // 返回的数据对象
        public object data = "";
    }
}
