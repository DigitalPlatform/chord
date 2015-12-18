using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Message
{
    public static class MessageUtil
    {

    }

    public class BiblioRecord
    {
        // 记录路径。这是本地路径，例如 “图书总库/1”
        public string RecPath { get; set; }
        // 图书馆 UID
        public string LibraryUID { get; set; }
        // 图书馆名
        public string LibraryName { get; set; }

        public string Format { get; set; }
        public string Data { get; set; }
        public string Timestamp { get; set; }
    }

    public class MessageResult
    {
        public string String { get; set; }  // 字符串类型的返回值
        public long Value { get; set; }      // 整数类型的返回值
        public string ErrorInfo { get; set; }   // 出错信息
    }

    public class GetUserResult : MessageResult
    {
        public List<User> Users { get; set; }
    }

    public class GetRecordResult : MessageResult
    {
        public List<string> Formats { get; set; }
        public List<string> Results { get; set; }

        public string RecPath { get; set; }
        public string Timestamp { get; set; }
    }

    public class SearchRequest
    {
        public string SearchID { get; set; }
        public string Operation { get; set; }
        public string DbNameList { get; set; }
        public string QueryWord { get; set; }
        public string UseList { get; set; }
        public string MatchStyle { get; set; }
        public string FormatList { get; set; }
        public long MaxResults { get; set; }

        public SearchRequest(string searchID,
            string operation,
            string dbNameList,
            string queryWord,
            string useList,
            string matchStyle,
            string formatList,
            long maxResults)
        {
            this.SearchID = searchID;
            this.Operation = operation;
            this.DbNameList = dbNameList;
            this.QueryWord = queryWord;
            this.UseList = useList;
            this.MatchStyle = matchStyle;
            this.FormatList = formatList;
            this.MaxResults = maxResults;
        }
    }

    public class User
    {
        public string id { get; set; }

        public string userName { get; set; } // 用户名
        public string password { get; set; }  // 密码
        public string rights { get; set; } // 权限
        public string department { get; set; } // 部门名称
        public string tel { get; set; }  // 电话号码
        public string comment { get; set; }  // 注释
    }

}
