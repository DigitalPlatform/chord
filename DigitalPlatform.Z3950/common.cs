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

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            text.Append("Value=" + this.Value + "\r\n");
            text.Append("ErrorInfo=" + this.ErrorInfo + "\r\n");
            return text.ToString();
        }
    }

}
