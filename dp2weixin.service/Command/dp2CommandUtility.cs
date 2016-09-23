using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dp2Command.Service
{
    public class dp2CommandUtility
    {
        public const string C_Command_Search = "search";
        public const string C_Command_Set = "set";

        // 检索每页显示记录数
        public const int C_ViewCount_OnePage = 20;


        //public const String C_WeiXinIdPrefix = "weixinid:";

        /// <summary>
        /// 校验字符串是否是命令
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static bool CheckIsCommand(string strText)
        {
            strText = strText.ToLower();
            if (strText == C_Command_Search
                || strText == C_Command_Set
                )
            {
                return true;
            }

            return false;
        }
    }
}