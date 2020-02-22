using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Mini
{
    public class SettingInfo
    {
        public string Url { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsSavePassword { get; set; }

        public string NotFoundReasons { get; set; }

        // 把原因拆成数组，方便后面使用
        public string[] ReasonArray
        {
            get
            {
                if(string.IsNullOrEmpty(this.NotFoundReasons) ==true)
                    return new string[] { };

                string temp = this.NotFoundReasons.Replace("\r\n", "\n");
                return temp.Split(new char[] {'\n'});
            }
        }
    }
}
