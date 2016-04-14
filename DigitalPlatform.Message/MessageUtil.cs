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
        // 记录路径。可能是本地路径，例如 “图书总库/1”；也可能是全局路径，例如“图书总库@xxxxxxx”
        public string RecPath { get; set; }
        public string Format { get; set; }
        public string Data { get; set; }
        public string Timestamp { get; set; }
    }

#if NO
    // 全局记录。包含了图书馆名称信息
    public class GlobalRecord
    {
        // 图书馆 UID
        public string LibraryUID { get; set; }
        // 图书馆名
        public string LibraryName { get; set; }
        // 基本记录
        public Record Record { get; set; }

        public GlobalRecord(Record record)
        {
            this.Record = new Record();
            this.Record.RecPath = record.RecPath;
            this.Record.Format = record.Format;
            this.Record.Data = record.Data;
            this.Record.Timestamp = record.Timestamp;
        }
    }
#endif

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
        public string TaskID { get; set; }    // 本次检索的任务 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作
        public string Operation { get; set; }   // 操作名。
        public string DbNameList { get; set; }  // 数据库名列表。一般为 "<全部>"
        public string QueryWord { get; set; }   // 检索词。若为 !getResult 表示不检索、从已有结果集中获取记录
        public string UseList { get; set; }     // 检索途径列表
        public string MatchStyle { get; set; }  // 匹配方式。为 exact/left/right/middle 之一
        public string ResultSetName { get; set; }   // 检索创建的结果集名。空表示为默认结果集
        public string FormatList { get; set; }  // 返回的数据格式列表
        public long MaxResults { get; set; }    // 本次检索最多命中的记录数。-1 表示不限制
        public long Start { get; set; } // 本次获得结果的开始位置
        public long Count { get; set; } // 本次获得结果的个数。 -1表示尽可能多

        public SearchRequest(string taskID,
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
            this.TaskID = taskID;
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

    public class SetInfoRequest
    {
        public string TaskID { get; set; }    // 任务 ID。由于一个 Connection 可以用于同时执行多个任务，本参数用于区分不同的任务
        public string Operation { get; set; }   // 操作名。

        public string BiblioRecPath { get; set; }
        public List<Entity> Entities { get; set; }
    }

    public class Entity
    {
        public string Action { get; set; }   // 要执行的操作(get时此项无用)

        public string RefID { get; set; }   // 参考 ID

        public Record OldRecord { get; set; }

        public Record NewRecord { get; set; }

        public string Style { get; set; }   // 风格。常用作附加的特性参数。例如: nocheckdup,noeventlog,force

        public string ErrorInfo { get; set; }  // 出错信息

        public string ErrorCode { get; set; }   // 出错码（表示属于何种类型的错误）
    }

    public class BindPatronRequest
    {
        public string TaskID { get; set; }    // 任务 ID。由于一个 Connection 可以用于同时执行多个任务，本参数用于区分不同的任务
        public string Action { get; set; }   // 动作名。

        public string QueryWord { get; set; }

        public string Password { get; set; }
        public string BindingID { get; set; }
        public string Style { get; set; }
        public string ResultTypeList { get; set; }

        public BindPatronRequest(string taskID,
            string action,
            string queryWord,
            string password,
            string bindID,
            string style,
            string resultTypeList)
        {
            this.TaskID = taskID;
            this.Action = action;
            this.QueryWord = queryWord;
            this.Password = password;
            this.BindingID = bindID;
            this.Style = style;
            this.ResultTypeList = resultTypeList;
        }
    }

    public class BindPatronResult : MessageResult
    {
        public List<string> Formats { get; set; }
        public List<string> Results { get; set; }

        public string RecPath { get; set; }
        public string Timestamp { get; set; }
    }
}
