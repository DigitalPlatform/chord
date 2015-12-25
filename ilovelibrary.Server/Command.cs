using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ilovelibrary.Server
{
    public class Command
    {
        // 命令常量
        public const string C_Command_Borrow = "borrow";
        public const string C_Command_Return = "return";
        public const string C_Command_Renew = "renew";

        public int id { get; set; }
        public string type { get; set; }
        public string typeString { get; set; }
        public string readerBarcode { get; set; }
        public string itemBarcode { get; set; }
        public string description { get; set; }
        public string operTime { get; set; }

        // 处理状态
        public int state { get; set; }
        public string resultInfo { get; set; }

        public static string getTypeString(string type)
        {
            if (type == C_Command_Borrow)
                return "借";
            if (type == C_Command_Return)
                return "还";
            if (type == C_Command_Renew)
                return "续借";
            return "未知命令";
        }
    }
}
