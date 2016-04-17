using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dp2Command.Service
{
    public class dp2CommandUtility
    {
        // 2016/2/20 选择图书馆
        public const string C_Command_SelectLib = "selectlib";
        // 切换读者，用于微信用户绑定多个读者的情况
        public const string C_Command_ChangePatron = "changePatron";
             

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
                || strText == C_Command_Notice
                || strText == C_Command_SelectLib
                )
            {
                return true;
            }

            return false;
        }
    }
}