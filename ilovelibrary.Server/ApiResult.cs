using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ilovelibrary.Server
{
    // API返回结果
    public class ApiResult
    {
        public string errorInfo = "";

        /// <summary>
        /// -1:表示出错
        /// </summary>
        public int errorCode = 0;
    }
}
