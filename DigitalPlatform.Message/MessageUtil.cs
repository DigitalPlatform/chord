using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Message
{
    public static class MessageUtil
    {
        public const int BINARY_CHUNK_SIZE = 4 * 1024;  // 4K 时会出现数据传输错误

    }

    // 2016/10/23
    public class LoginInfo
    {
        public string UserName { get; set; }    // 用户名。指 dp2library 的用户名。如果 Type 为 "Patron"，表示这是一个读者。2016/10/21
        public string UserType { get; set; }    // 用户类型。patron 表示读者，其他表示工作人员
        public string Password { get; set; }    // 密码。如果为 null，表示用代理方式登录
        public string Style { get; set; }       // 登录方式

        public LoginInfo()
        {

        }

        public LoginInfo(string userName, bool isPatron)
        {
            this.UserName = userName;
            if (isPatron)
                this.UserType = "patron";
        }

        public LoginInfo(string userName,
            bool isPatron,
            string password,
            string style)
        {
            this.UserName = userName;
            if (isPatron)
                this.UserType = "patron";
            this.Password = password;
            this.Style = style;
        }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            if (string.IsNullOrEmpty(this.UserName) == false)
                text.Append("UserName=" + this.UserName + ";");
            if (string.IsNullOrEmpty(this.UserType) == false)
                text.Append("UserType=" + this.UserType + ";");
            if (string.IsNullOrEmpty(this.Password) == false)
                text.Append("Password=" + this.Password + ";");
            if (string.IsNullOrEmpty(this.Style) == false)
                text.Append("Style=" + this.Style + ";");
            return text.ToString();
        }
    }

    // 通用的 API 返回值结构
    public class MessageResult
    {
        public string String { get; set; }  // 字符串类型的返回值
        public long Value { get; set; }      // 整数类型的返回值
        public string ErrorInfo { get; set; }   // 出错信息

        public void SetError(string errorInfo, string errorCode)
        {
            this.ErrorInfo = errorInfo;
            this.String = errorCode;
            this.Value = -1;
        }

        public string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.Append("String=" + this.String + "\r\n");
            text.Append("Value=" + this.Value + "\r\n");
            text.Append("ErrorInfo=" + this.ErrorInfo + "\r\n");
            return text.ToString();
        }
    }

    #region Group 有关

    public class GetGroupRequest
    {
        public string TaskID { get; set; }    // 本次检索的任务 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作

        public string GroupCondition { get; set; }
        public string UserCondition { get; set; }
        public string TimeCondition { get; set; }

        public long Start { get; set; }
        public long Count { get; set; }

        public GetGroupRequest()
        {

        }

        public GetGroupRequest(string taskID,
            string groupCondition,
            string userCondition,
            string timeCondition,
            long start,
            long count)
        {
            this.TaskID = taskID;
            this.GroupCondition = groupCondition;
            this.UserCondition = userCondition;
            this.TimeCondition = timeCondition;
            this.Start = start;
            this.Count = count;
        }
    }

    public class GetGroupResult : MessageResult
    {
        public List<GroupRecord> Results { get; set; }
    }

    public class GroupRecord
    {
        public string id { get; set; }  // 组的 id

        public string name { get; set; }   // 组名。表意的名称
        public string creator { get; set; } // 创建组的人。用户名或 id
        public string[] manager { get; set; }   // 管理员
        public string comment { get; set; }  // 注释
        public string type { get; set; }    // 组类型。类型是从用途角度来说的

        public DateTime createTime { get; set; } // 创建时间
        public DateTime expireTime { get; set; } // 组失效时间
    }

    #endregion

    #region Message 有关

    public class MessageRecord
    {
        public string id { get; set; }  // 消息的 id

        public string[] groups { get; set; }   // 组名 或 组id。消息所从属的组
        public string creator { get; set; } // 创建消息的人。也就是发送消息的用户名或 id
        public string userName { get; set; } // 创建消息的人的用户名
        public string data { get; set; }  // 消息数据体
        public string format { get; set; } // 消息格式。格式是从存储格式角度来说的
        public string type { get; set; }    // 消息类型。类型是从用途角度来说的
        public string thread { get; set; }    // 消息所从属的话题线索
        public string[] subjects { get; set; }   // 主题词

        public DateTime publishTime { get; set; } // 消息发布时间
        public DateTime expireTime { get; set; } // 消息失效时间

        public void CopyFrom(MessageRecord record)
        {
            this.id = record.id;
            this.groups = record.groups;
            this.creator = record.creator;
            this.userName = record.userName;
            this.data = record.data;
            this.format = record.format;
            this.type = record.type;
            this.thread = record.thread;
            this.subjects = record.subjects;
            this.publishTime = record.publishTime;
            this.expireTime = record.expireTime;
        }
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

    public class SetMessageRequest
    {
        public string TaskID { get; set; }  // 2016/11/30 新增，如果为空，表示不使用多次分批发送功能

        public string Action { get; set; }
        public string Style { get; set; }
        public List<MessageRecord> Records { get; set; } 

        public SetMessageRequest()
        {

        }

        public SetMessageRequest(
            string action,
            string style,
            List<MessageRecord> records)
        {
            this.TaskID = "";
            this.Action = action;
            this.Style = style;
            this.Records = records;
        }

        public SetMessageRequest(
            string taskID,
            string action,
            string style,
            List<MessageRecord> records)
        {
            this.TaskID = taskID;
            this.Action = action;
            this.Style = style;
            this.Records = records;
        }
    }

    public class SetMessageResult : MessageResult
    {
        public List<MessageRecord> Results { get; set; }    // 返回的实际被创建或者修改的消息
    }

    public class GetMessageRequest
    {
        public string TaskID { get; set; }    // 本次检索的任务 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作

        public string Action { get; set; }  // 动作。空/transGroupName/transGroupNameQuick/enumGroupName/enumSubject

        public string GroupCondition { get; set; }  // 群组名或者 ID
        public string UserCondition { get; set; }   // 用户名条件
        public string TimeCondition { get; set; }   // 时间范围。xxxx~xxxx。DateTime.ToString("G") 格式。
        public string SortCondition { get; set; }   // 排序方式。publishTime|ascending。默认按照 publishTime 升序
        public string IdCondition { get; set; } // 消息 ID 的列表
        public string SubjectCondition { get; set; }    // 主题词的列表

        public long Start { get; set; }
        public long Count { get; set; }

        public GetMessageRequest()
        {

        }

        public GetMessageRequest(string taskID,
            string action,
            string groupCondition,
            string userCondition,
            string timeCondition,
            string sortCondition,
            string idCondition,
            string subjectCondition,
            long start,
            long count)
        {
            this.TaskID = taskID;
            this.Action = action;
            this.GroupCondition = groupCondition;
            this.UserCondition = userCondition;
            this.TimeCondition = timeCondition;
            this.SortCondition = sortCondition;
            this.IdCondition = idCondition;
            this.SubjectCondition = subjectCondition;
            this.Start = start;
            this.Count = count;
        }
    }

    public class GetMessageResult : MessageResult
    {
        public List<MessageRecord> Results { get; set; }

        public GetMessageResult(MessageResult result, List<MessageRecord> results)
        {
            this.String = result.String;
            this.Value = result.Value;
            this.ErrorInfo = result.ErrorInfo;

            this.Results = results;
        }
    }

    #endregion

    #region Login() 有关

    public class LoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public string LibraryUserName { get; set; }
        public string LibraryUID { get; set; }
        public string LibraryName { get; set; }
        public string PropertyList { get; set; }
    }

    #endregion

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


    #region Search() 有关

    public class Record
    {
        // 记录路径。可能是本地路径，例如 “图书总库/1”；也可能是全局路径，例如“图书总库@xxxxxxx”
        public string RecPath { get; set; }
        public string Format { get; set; }
        public string Data { get; set; }
        public string Timestamp { get; set; }

        public string MD5 { get; set; } // Data 的 MD5 hash
    }

    public class SearchRequest
    {
        public string TaskID { get; set; }    // 本次检索的任务 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作
        public LoginInfo LoginInfo { get; set; }    // 登录信息 2016/10/22
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
        public string ServerPushEncoding { get; set; }
        public TimeSpan Timeout { get; set; }   // 超时参数。默认为一分钟 2016/9/17

        public SearchRequest()
        {

        }

        public SearchRequest(string taskID,
            LoginInfo loginInfo,
            string operation,
            string dbNameList,
            string queryWord,
            string useList,
            string matchStyle,
            string resultSetName,
            string formatList,
            long maxResults,
            long start,
            long count,
            string serverPushEncoding = "")
        {
            this.TaskID = taskID;
            this.LoginInfo = loginInfo;
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
            this.ServerPushEncoding = serverPushEncoding;
        }

        public SearchRequest(string taskID,
            LoginInfo loginInfo,
    string operation,
    string dbNameList,
    string queryWord,
    string useList,
    string matchStyle,
    string resultSetName,
    string formatList,
    long maxResults,
    long start,
    long count,
            TimeSpan timeout,
    string serverPushEncoding = "")
        {
            this.TaskID = taskID;
            this.LoginInfo = loginInfo;
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
            this.ServerPushEncoding = serverPushEncoding;
            this.Timeout = timeout;
        }

        public string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.Append("TaskID=" + this.TaskID + "\r\n");
            if (this.LoginInfo != null)
                text.Append("LoginInfo=" + this.LoginInfo.ToString() + "\r\n");
            text.Append("Operation=" + this.Operation + "\r\n");
            text.Append("DbNameList=" + this.DbNameList + "\r\n");
            text.Append("QueryWord=" + this.QueryWord + "\r\n");
            text.Append("UseList=" + this.UseList + "\r\n");
            text.Append("MatchStyle=" + this.MatchStyle + "\r\n");
            text.Append("ResultSetName=" + this.ResultSetName + "\r\n");
            text.Append("FormatList=" + this.FormatList + "\r\n");
            text.Append("MaxResults=" + this.MaxResults + "\r\n");
            text.Append("Start=" + this.Start + "\r\n");
            text.Append("Count=" + this.Count + "\r\n");
            text.Append("Timeout=" + this.Timeout.ToString() + "\r\n");
            text.Append("ServerPushEncoding=" + this.ServerPushEncoding + "\r\n");
            return text.ToString();
        }
    }

    public class SearchResponse
    {
        public string TaskID { get; set; }    // 本次检索的任务 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作
        public long ResultCount { get; set; }
        public long Start { get; set; }    // 本次响应的偏移
        public string LibraryUID { get; set; }  // 响应者的 UID。这样 Record.RecPath 中就记载短路径即可
        public IList<Record> Records { get; set; }
        public string ErrorInfo { get; set; }
        public string ErrorCode { get; set; }

        public SearchResponse(string taskID,
            long resultCount,
            long start,
            string libraryUID,
            IList<Record> records,
            string errorInfo,
            string errorCode)
        {
            this.TaskID = taskID;
            this.ResultCount = resultCount;
            this.Start = start;
            this.LibraryUID = libraryUID;
            this.Records = records;
            this.ErrorInfo = errorInfo;
            this.ErrorCode = errorCode;
        }

        public string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.Append("TaskID=" + this.TaskID + "\r\n");
            text.Append("ResultCount=" + this.ResultCount + "\r\n");
            text.Append("Start=" + this.Start + "\r\n");
            text.Append("LibraryUID=" + this.LibraryUID + "\r\n");
            text.Append("Records:\r\n" + this.DumpRecords() + "\r\n");
            text.Append("ErrorInfo=" + this.ErrorInfo + "\r\n");
            text.Append("ErrorCode=" + this.ErrorCode + "\r\n");
            return text.ToString();
        }

        public string DumpRecords()
        {
            if (this.Records == null)
                return "{null}";
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (Record record in this.Records)
            {
                text.Append((i + 1).ToString() + ")\r\n");

                text.Append("RecPath=" + record.RecPath + "\r\n");
                text.Append("Format=" + record.Format + "\r\n");
                text.Append("Data=" + record.Data + "\r\n");
                text.Append("Timestamp=" + record.Timestamp + "\r\n");
                text.Append("MD5=" + record.MD5 + "\r\n");

                i++;
            }
            return text.ToString();
        }
    }

    #endregion

    #region Connection 有关

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

        public string[] groups { get; set; }  // 所加入的群组
        public string binding { get; set; }
    }

    public class GetConnectionInfoRequest
    {
        public string TaskID { get; set; }    // 本次检索的任务 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作
        public string Operation { get; set; }   // 操作名。
        public string QueryWord { get; set; }   // 检索词。
        public string FormatList { get; set; }  // 返回的数据格式列表
        public long MaxResults { get; set; }    // 本次检索最多命中的记录数。-1 表示不限制
        public long Start { get; set; } // 本次获得结果的开始位置
        public long Count { get; set; } // 本次获得结果的个数。 -1表示尽可能多

        public GetConnectionInfoRequest()
        {

        }

        public GetConnectionInfoRequest(string taskID,
            string operation,
            string queryWord,
            string formatList,
            long maxResults,
            long start,
            long count)
        {
            this.TaskID = taskID;
            this.Operation = operation;
            this.QueryWord = queryWord;
            this.FormatList = formatList;
            this.MaxResults = maxResults;
            this.Start = start;
            this.Count = count;
        }
    }

    public class ConnectionRecord
    {
        public User User { get; set; }
        public string LibraryUID { get; set; }
        public string LibraryName { get; set; }
        public string LibraryUserName { get; set; }
        public string Notes { get; set; }

        public string PropertyList { get; set; }

        public string ClientIP { get; set; }

        public string ConnectionID { get; set; }

        public ConnectionRecord(
            string connectionID,
            string userName,
            string rights,
            string duty,
            string department,
            string tel,
            string comment,
            string libraryUID,
            string libraryName,
            string libraryUserName,
            string propertyList,
            string clientIP,
            string notes)
        {
            User user = new User();
            user.userName = userName;
            user.rights = rights;
            user.duty = duty;
            user.department = department;
            user.tel = tel;
            user.comment = comment;

            this.User = user;
            this.LibraryUID = libraryUID;
            this.LibraryName = libraryName;
            this.LibraryUserName = libraryUserName;
            this.PropertyList = propertyList;
            this.ClientIP = clientIP;
            this.Notes = notes;
            this.ConnectionID = connectionID;
        }
    }

    #endregion

    #region SetInfo() 有关

    public class SetInfoRequest
    {
        public string TaskID { get; set; }    // 任务 ID。由于一个 Connection 可以用于同时执行多个任务，本参数用于区分不同的任务
        public LoginInfo LoginInfo { get; set; }    // 登录信息 2016/10/22
        public string Operation { get; set; }   // 操作名。

        public string BiblioRecPath { get; set; }
        public List<Entity> Entities { get; set; }

        public SetInfoRequest()
        {

        }

        public SetInfoRequest(string taskID,
            LoginInfo loginInfo,
            string operation,
            string biblioRecPath,
            List<Entity> entities)
        {
            this.TaskID = taskID;
            this.LoginInfo = loginInfo;
            this.Operation = operation;
            this.BiblioRecPath = biblioRecPath;
            this.Entities = entities;
        }
    }

    public class SetInfoResult : MessageResult
    {
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

    #endregion

    #region BindPatron() 有关

    public class BindPatronRequest
    {
        public string TaskID { get; set; }    // 任务 ID。由于一个 Connection 可以用于同时执行多个任务，本参数用于区分不同的任务
        public LoginInfo LoginInfo { get; set; }    // 登录信息 2016/10/22
        public string Action { get; set; }   // 动作名。

        public string QueryWord { get; set; }

        public string Password { get; set; }
        public string BindingID { get; set; }
        public string Style { get; set; }
        public string ResultTypeList { get; set; }

        public BindPatronRequest()
        {

        }

        public BindPatronRequest(string taskID,
            LoginInfo loginInfo,
            string action,
            string queryWord,
            string password,
            string bindID,
            string style,
            string resultTypeList)
        {
            this.TaskID = taskID;
            this.LoginInfo = loginInfo;
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

    #endregion

    #region Circulation() 有关

    public class CirculationRequest
    {
        public string TaskID { get; set; }    // 本次检索的任务 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作
        public LoginInfo LoginInfo { get; set; }    // 登录信息 2016/10/22
        public string Operation { get; set; }   // 操作名。
        public string Patron { get; set; }  // strReaderBarcode
        public string Item { get; set; }   // strItemBarcode 和 strConfirmItemRecPath 都包含
        public string Style { get; set; }     // 
        public string PatronFormatList { get; set; }
        public string ItemFormatList { get; set; }  // 
        public string BiblioFormatList { get; set; }

        public CirculationRequest()
        {

        }

        public CirculationRequest(string taskID,
            LoginInfo loginInfo,
            string operation,
            string patron,
            string item,
            string style,
            string outputPatronFormatList,
            string outputItemFormatList,
            string outputBiblioFormatList)
        {
            this.TaskID = taskID;
            this.LoginInfo = loginInfo;
            this.Operation = operation;
            this.Patron = patron;
            this.Item = item;
            this.Style = style;
            this.PatronFormatList = outputPatronFormatList;
            this.ItemFormatList = outputItemFormatList;
            this.BiblioFormatList = outputBiblioFormatList;
        }
    }

    public class CirculationResult : MessageResult
    {
        public List<string> PatronResults { get; set; }
        public List<string> ItemResults { get; set; }
        public List<string> BiblioResults { get; set; }
        public List<string> DupPaths { get; set; }

        public string PatronBarcode { get; set; }
        public BorrowInfo BorrowInfo { get; set; }
        public ReturnInfo ReturnInfo { get; set; }
    }

    public class BorrowInfo
    {
        // 应还日期/时间
        public string LatestReturnTime { get; set; }   // RFC1123格式，GMT时间

        // 借书期限。例如“20day”
        public string Period { get; set; }

        // 当前为续借的第几次？0表示初次借阅
        public long BorrowCount { get; set; }

        // 借书操作者
        public string BorrowOperator { get; set; }
    }

    // 还书成功后的信息
    public class ReturnInfo
    {
        // 借阅日期/时间
        public string BorrowTime { get; set; }  // RFC1123格式，GMT时间

        // 应还日期/时间
        public string LatestReturnTime { get; set; }   // RFC1123格式，GMT时间

        // 原借书期限。例如“20day”
        public string Period { get; set; }

        // 当前为续借的第几次？0表示初次借阅
        public long BorrowCount { get; set; }

        // 违约金描述字符串。XML格式
        public string OverdueString { get; set; }

        // 借书操作者
        public string BorrowOperator { get; set; }

        // 还书操作者
        public string ReturnOperator { get; set; }

        /// <summary>
        /// 所还的册的图书类型
        /// </summary>
        public string BookType { get; set; }

        /// <summary>
        /// 所还的册的馆藏地点
        /// </summary>
        public string Location { get; set; }
    }

    #endregion

    #region GetRes() 有关

    public class GetResRequest
    {
        public string TaskID { get; set; }    // 本次任务 ID。
        public LoginInfo LoginInfo { get; set; }    // 登录信息 2016/10/22
        public string Operation { get; set; }   // 操作名。
        public string Path { get; set; }
        public long Start { get; set; }   // strItemBarcode 和 strConfirmItemRecPath 都包含
        public long Length { get; set; }     // 
        public string Style { get; set; }

        public GetResRequest()
        {

        }

        public GetResRequest(string taskID,
            LoginInfo loginInfo,
            string operation,
            string path,
            long start,
            long length,
            string style)
        {
            this.TaskID = taskID;
            this.LoginInfo = loginInfo;
            this.Operation = operation;
            this.Path = path;
            this.Start = start;
            this.Length = length;
            this.Style = style;
        }
    }

    public class GetResResponse
    {
        public string TaskID { get; set; }    // 本次检索的任务 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作
        public long TotalLength { get; set; }
        public long Start { get; set; }    // 本次响应 Data 的偏移
        public string Path { get; set; }
        public byte[] Data { get; set; }
        public string Metadata { get; set; }
        public string Timestamp { get; set; }
        public string ErrorInfo { get; set; }
        public string ErrorCode { get; set; }

        public GetResResponse()
        {

        }

        public GetResResponse(string taskID,
            long totalLength,
            long start,
            string path,
            byte[] data,
            string metadata,
            string timestamp,
            string errorInfo,
            string errorCode)
        {
            this.TaskID = taskID;
            this.TotalLength = totalLength;
            this.Start = start;
            this.Path = path;
            this.Data = data;
            this.Metadata = metadata;
            this.Timestamp = timestamp;
            this.ErrorInfo = errorInfo;
            this.ErrorCode = errorCode;
        }

        public string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.Append("TaskID=" + this.TaskID + "\r\n");
            text.Append("TotalLength=" + this.TotalLength + "\r\n");
            text.Append("Start=" + this.Start + "\r\n");
            text.Append("Data.Length=" + (this.Data != null ? this.Data.Length : 0) + "\r\n");
            text.Append("Metadata" + this.Metadata + "\r\n");
            text.Append("Timestamp" + this.Timestamp + "\r\n");
            text.Append("ErrorInfo=" + this.ErrorInfo + "\r\n");
            text.Append("ErrorCode=" + this.ErrorCode + "\r\n");
            return text.ToString();
        }
    }

    #endregion


    #region WebCall() 有关

    public class WebCallResult : MessageResult
    {
        public WebData WebData { get; set; }

        public string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.Append("Value=" + this.Value + "\r\n");
            text.Append("ErrorInfo=" + this.ErrorInfo + "\r\n");
            text.Append("String=" + this.String + "\r\n");

            if (this.WebData != null)
                text.Append("WebData=" + this.WebData.Dump() + "\r\n");
            return text.ToString();
        }
    }

    public class WebCallRequest
    {
        public string TaskID { get; set; }    // 本次任务 ID

        public string TransferEncoding { get; set; }
        public WebData WebData { get; set; }    // 传送的数据

        public bool First { get; set; }         // 是否为本次任务的第一个请求
        public bool Complete { get; set; }      // 传送是否结束

        // [JsonConstructor]
        public WebCallRequest()
        {

        }

        public WebCallRequest(string taskID,
            string transferEncoding,
            WebData webData,
            bool first,
            bool complete)
        {
            this.TaskID = taskID;
            this.TransferEncoding = transferEncoding;
            this.WebData = webData;
            this.First = first;
            this.Complete = complete;
        }
    }

    public class WebData
    {
        public string Headers { get; set; }   // 头字段

        public byte[] Content { get; set; }     // 数据体
        public string Text { get; set; }        // 文本形态的数据体

#if NO
        public int Offset { get; set; } // 本次传输的 Content 在总的 Content 里面的偏移
        public string MD5 { get; set; }
#endif

        public WebData()
        {

        }

        public WebData(string headers, byte[] content)
        {
            this.Headers = headers;
            this.Content = content;
            this.Text = null;
        }

        public WebData(string headers, string text)
        {
            this.Headers = headers;
            this.Text = text;
            this.Content = null;
        }

        public string Dump()
        {
            StringBuilder text = new StringBuilder();
            if (string.IsNullOrEmpty(this.Headers) == false)
                text.Append("Headers=" + this.Headers + "\r\n");
            if (this.Content != null)
                text.Append("Content.Length=" + this.Content.Length + "\r\n");
            if (this.Text != null)
                text.Append("Text=" + this.Text + "\r\n");
            return text.ToString();
        }
    }

    public class WebCallResponse
    {
        public string TaskID { get; set; }    // 本次任务 ID

        public string TransferEncoding { get; set; }
        public WebData WebData { get; set; }    // 传送的数据

        public bool Complete { get; set; }      // 传送是否结束
        public MessageResult Result { get; set; }   // 是否出错

        public string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.Append("TaskID=" + this.TaskID + "\r\n");
            text.Append("TransferEncoding=" + this.TransferEncoding + "\r\n");
            if (this.WebData != null)
                text.Append("WebData=" + this.WebData.Dump().Replace("\r\n", ";") + "\r\n");
            text.Append("Complete=" + this.Complete + "\r\n");
            if (this.Result != null)
                text.Append("Result=" + this.Result.Dump().Replace("\r\n", ";") + "\r\n");
            return text.ToString();
        }
    }

    #endregion

    #region Close() 相关

    public class CloseRequest
    {
        public string Action { get; set; }

        public CloseRequest(string action)
        {
            this.Action = action;
        }
    }

    #endregion

    #region ListRes() 相关

    public class ListResRequest
    {
        public string TaskID { get; set; }    // 本次任务 ID。
        public LoginInfo LoginInfo { get; set; }    // 登录信息
        public string Operation { get; set; }   // 操作名。
        public string Category { get; set; }
        public string Path { get; set; }
        public long Start { get; set; }
        public long Length { get; set; }     // 
        public string Style { get; set; }

        public ListResRequest()
        {

        }

        public ListResRequest(string taskID,
            LoginInfo loginInfo,
            string operation,
            string category,
            string path,
            long start,
            long length,
            string style)
        {
            this.TaskID = taskID;
            this.LoginInfo = loginInfo;
            this.Operation = operation;
            this.Category = category;
            this.Path = path;
            this.Start = start;
            this.Length = length;
            this.Style = style;
        }
    }

    public class ResInfo
    {

    }

    public class ListResResponse
    {
        public string TaskID { get; set; }    // 本次检索的任务 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作
        public long TotalLength { get; set; }
        public long Start { get; set; }    // 本次响应 Data 的偏移
        public List<ResInfo> Results { get; set; }

        public string ErrorInfo { get; set; }
        public string ErrorCode { get; set; }

        public ListResResponse()
        {

        }

        public ListResResponse(string taskID,
            long totalLength,
            long start,
            List<ResInfo> results,
            string errorInfo,
            string errorCode)
        {
            this.TaskID = taskID;
            this.TotalLength = totalLength;
            this.Start = start;
            this.Results = results;
            this.ErrorInfo = errorInfo;
            this.ErrorCode = errorCode;
        }

        public string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.Append("TaskID=" + this.TaskID + "\r\n");
            text.Append("TotalLength=" + this.TotalLength + "\r\n");
            text.Append("Start=" + this.Start + "\r\n");
            text.Append("Results.Count=" + (this.Results != null ? this.Results.Count : 0) + "\r\n");
            text.Append("ErrorInfo=" + this.ErrorInfo + "\r\n");
            text.Append("ErrorCode=" + this.ErrorCode + "\r\n");
            return text.ToString();
        }
    }

    public class ListResResult : MessageResult
    {
        public List<ResInfo> Results { get; set; }

        public ListResResult(MessageResult result, List<ResInfo> results)
        {
            this.String = result.String;
            this.Value = result.Value;
            this.ErrorInfo = result.ErrorInfo;

            this.Results = results;
        }
    }

    #endregion
}
