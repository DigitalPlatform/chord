using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalPlatform.Z3950
{
    // 通用的返回结果
    public class Result
    {
        public int Value { get; set; }
        public string ErrorInfo { get; set; }
        public string ErrorCode { get; set; }

        public static void CopyTo(Result source, Result target)
        {
            target.Value = source.Value;
            target.ErrorInfo = source.ErrorInfo;
            target.ErrorCode = source.ErrorCode;
        }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            text.Append("Value=" + this.Value + "\r\n");
            text.Append("ErrorInfo=" + this.ErrorInfo + "\r\n");
            if (string.IsNullOrEmpty(this.ErrorCode) == false)
                text.Append("ErrorCode=" + this.ErrorCode + "\r\n");
            return text.ToString();
        }
    }

}
