using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ilovelibrary.Server
{
    // 
    /// <summary>
    /// 书目库属性
    /// </summary>
    public class BiblioDbProperty
    {
        /// <summary>
        /// 书目库名
        /// </summary>
        public string DbName = "";  // 书目库名
        /// <summary>
        /// 格式语法
        /// </summary>
        public string Syntax = "";  // 格式语法

        /// <summary>
        /// 实体库名
        /// </summary>
        public string ItemDbName = "";  // 对应的实体库名

        /// <summary>
        /// 期库名
        /// </summary>
        public string IssueDbName = ""; // 对应的期库名 2007/10/19 

        /// <summary>
        /// 订购库名
        /// </summary>
        public string OrderDbName = ""; // 对应的订购库名 2007/11/30 

        /// <summary>
        /// 评注库名
        /// </summary>
        public string CommentDbName = "";   // 对应的评注库名 2009/10/23 

        /// <summary>
        /// 角色
        /// </summary>
        public string Role = "";    // 角色 2009/10/23 

        /// <summary>
        /// 是否参与流通
        /// </summary>
        public bool InCirculation = true;  // 是否参与流通 2009/10/23 
    }
}
