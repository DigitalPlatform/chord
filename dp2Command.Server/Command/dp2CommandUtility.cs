using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dp2Command.Service
{
    public class dp2CommandUtility
    {
        public const string C_Command_Binding = "binding";
        public const string C_Command_Unbinding = "unbinding";
        public const string C_Command_MyInfo = "myinfo";
        public const string C_Command_BorrowInfo = "borrowinfo";
        public const string C_Command_Renew = "renew";
        public const string C_Command_Search = "search";
        public const string C_Command_SearchDetail = "search-detail";
        // 公共信息
        public const string C_Command_BookRecommend = "bookrecommend";
        public const string C_Command_Notice = "notice";//Notice
        // 检索每页显示记录数
        public const int C_ViewCount_OnePage = 20;


        public const String C_WeiXinIdPrefix = "weixinid:";

        /// <summary>
        /// 校验字符串是否是命令
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static bool CheckIsCommand(string strText)
        {
            strText = strText.ToLower();
            if (strText == C_Command_Search
                || strText == C_Command_Binding
                || strText == C_Command_Unbinding
                || strText == C_Command_MyInfo
                || strText == C_Command_BorrowInfo
                || strText == C_Command_Renew
                || strText == C_Command_BookRecommend
                || strText == C_Command_Notice)
            {
                return true;
            }

            return false;
        }
    }
}