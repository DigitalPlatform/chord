using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    public class WeiXinConst
    {
        public const string C_Session_WeiXinId = "weixinId";
        public const string C_Session_Code = "code";
        public const string C_Session_CmdContainer = "cmdcontainer";


        public static string EncryptKey = "dp2weixinPassword";

        public const String C_WeiXinIdPrefix = "weixinid:";

        // 检索限制最大命中数常量
        public const int C_Search_MaxCount = 200;
        public const int C_OnePage_Count = 10;

        public const string C_Dp2003LibName = "数字平台";


    }
}
