using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Net
{
    // 通用的返回结果
    public class Result
    {
        public int Value { get; set; }
        public string ErrorInfo { get; set; }
        public string ErrorCode { get; set; }
        public Exception Exception { get; set; }

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

    public class RecvResult : Result
    {
        public int Length { get; set; }
        public byte[] Package { get; set; }

        // 通讯包的结束符
        public object Terminator { get; set; }

        public RecvResult()
        {

        }

        public RecvResult(Result source)
        {
            Result.CopyTo(source, this);
        }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder(base.ToString());
            text.Append("Package=" + this.Package + "\r\n");
            text.Append("Length=" + this.Length + "\r\n");
            return text.ToString();
        }
    }


}
