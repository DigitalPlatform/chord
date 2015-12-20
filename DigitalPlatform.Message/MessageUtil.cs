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

    public class Record
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
        public string SearchID { get; set; }    // 本次检索的 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作
        public string Operation { get; set; }   // 操作名。若为 getResult 表示本次不需要进行检索，而是从已有的结果集中获取数据。结果集名在 ResultSetName 中
        public string DbNameList { get; set; }  // 数据库名列表。一般为 "<全部>"
        public string QueryWord { get; set; }   // 检索词。
        public string UseList { get; set; }     // 检索途径列表
        public string MatchStyle { get; set; }  // 匹配方式。为 exact/left/right/middle 之一
        public string ResultSetName { get; set; }   // 检索创建的结果集名。空表示为默认结果集
        public string FormatList { get; set; }  // 返回的数据格式列表
        public long MaxResults { get; set; }    // 本次检索最多命中的记录数。-1 表示不限制
        public long Start { get; set; } // 本次获得结果的开始位置
        public long Count { get; set; } // 本次获得结果的个数。 -1表示尽可能多

        public SearchRequest(string searchID,
            string operation,
            string dbNameList,
            string queryWord,
            string useList,
            string matchStyle,
            string resultSetName,
            string formatList,
            long maxResults,
            long start,
            long count)
        {
            this.SearchID = searchID;
            this.Operation = operation;
            this.DbNameList = dbNameList;
            this.QueryWord = queryWord;
            this.UseList = useList;
            this.MatchStyle = matchStyle;
            this.ResultSetName = resultSetName;
            this.FormatList = formatList;
            this.MaxResults = maxResults;
            this.Start = start;
            this.Count = count;
        }
    }

    public class User
    {
        public string id { get; set; }

        public string userName { get; set; } // 用户名
        public string password { get; set; }  // 密码
        public string rights { get; set; } // 权限
        public string duty { get; set; }    // 义务
        public string department { get; set; } // 部门名称
        public string tel { get; set; }  // 电话号码
        public string comment { get; set; }  // 注释
    }
}
