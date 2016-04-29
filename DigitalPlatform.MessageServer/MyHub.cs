using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using System.Security.Claims;

using Microsoft.AspNet.SignalR;

using DigitalPlatform.Message;
using DigitalPlatform.Text;

namespace DigitalPlatform.MessageServer
{
    // TODO: 检查所有前端需要使用的 API，对 notlogin 返回值做规范化处理。以便前端能统一处理这种状态
    /// <summary>
    /// 
    /// </summary>
    [MyAuthorize]
    public class MyHub : Hub
    {
        public const string USERITEM_KEY = "MyHub.UserItem";

        public void Send(string name, string message)
        {
            Clients.All.addMessage(name, message);
        }

        #region 辅助函数

        // 判断响应是否为(顺次发回的)最后一个响应
        // parameters:
        //      resultCount 结果集中命中的结果总数
        //      returnStart 本次要返回的，结果集中的开始位置
        //      returnCount 本次要返回的，结果集中的从 returnStart 开始的元素个数
        //      start   集合 records 开始的偏移位置。数值是从结果集的最开头算起
        static bool IsComplete(long resultCount,
            long returnStart,
            long returnCount,
            long start,
            IList<Record> records)
        {
#if NO
            Console.WriteLine("IsComplete() resultCount="+resultCount
                +",returnStart="+returnStart
                +",returnCount="+returnCount
                +",start="+start
                +",records.Count="
                +records == null ? "null" : records.Count.ToString());
#endif
            if (returnCount == 0)
                returnCount = -1;   // 暂时矫正

            if (resultCount == -1)
                return true;    // 出错，也意味着响应结束

            if (resultCount < 0)
                return false;   // -1 表示结果尺寸不确定

            long tail = resultCount;
            if (returnCount != -1)
                tail = Math.Min(resultCount, returnStart + returnCount);

            if (records == null)
            {
                if (start >= tail)
                    return true;
                return false;
            }

            if (start + records.Count >= tail)
                return true;
            return false;
        }

        // 获得通道信息，并顺便检查状态
        ConnectionInfo GetConnection(string id,
            MessageResult result,
            string strFunctionName,
            bool checkLogin = true)
        {
            ConnectionInfo info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
            if (info == null)
            {
                result.Value = -1;
                result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。" + strFunctionName + " 操作失败";
                result.String = "_connectionNotFound";
                return null;
            }

            if (checkLogin && info.UserItem == null)
            {
                result.Value = -1;
                result.String = "_notLogin";
                result.ErrorInfo = "未注册用户无法使用 " + strFunctionName + " 功能";
                return null;
            }

            return info;
        }

        #endregion

        #region GetGroup() API

        public MessageResult RequestGetGroup(GetGroupRequest param)
        {
            // param.Count 为 0 和 -1 意义不同。前者可用于只探索条数，不获取数据。比如在界面上显示有多少条新的信息

            MessageResult result = new MessageResult();

            // 检查参数
            if (string.IsNullOrEmpty(param.GroupCondition) == true)
            {
                result.Value = -1;
                result.String = "InvalidParam";
                result.ErrorInfo = "param 成员 GroupCondition 不应为空";
                return result;
            }

            ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
result,
"RequestGetGroup()",
false);
            if (connection_info == null)
                return result;

            bool bSupervisor = (StringUtil.Contains(connection_info.Rights, "supervisor"));
            if (bSupervisor == false)
            {
                if (string.IsNullOrEmpty(connection_info.UserName))
                {
                    if (GroupDefinition.IsDefaultGroupName(param.GroupCondition) == false)
                    {
                        result.Value = -1;
                        result.String = "Denied";
                        result.ErrorInfo = "未注册的用户只允许查看 <default> 群组的消息，不允许查看群组 '" + param.GroupCondition + "' 的消息";
                        return result;
                    }
                }

                if (GroupDefinition.IncludeGroup(connection_info.Groups, param.GroupCondition) == false)
                {
                    result.Value = -1;
                    result.String = "Denied";
                    result.ErrorInfo = "当前用户不能查看群组 '" + param.GroupCondition + "' 的消息";
                    return result;
                }
            }

            SearchInfo search_info = null;
            try
            {
                search_info = ServerInfo.SearchTable.AddSearch(Context.ConnectionId,
                    param.TaskID,
                    param.Start,
                    param.Count);
            }
            catch (ArgumentException)
            {
                result.Value = -1;
                result.ErrorInfo = "TaskID '" + param.TaskID + "' 已经存在了，不允许重复使用";
                return result;
            }

            result.String = search_info.UID;   // 返回检索请求的 UID

            // 启动一个独立的 Task，该 Task 负责搜集和发送结果信息
            // 这是典型的 dp2MServer 能完成任务的情况，不需要再和另外一个前端通讯
            // 不过，请求本 API 的前端，要做好在 Request 返回以前就先得到数据响应的准备
            Task.Factory.StartNew(() => SearchGroupAndResponse(param));

            result.Value = 1;   // 成功
            return result;
        }

        void SearchGroupAndResponse(
            GetGroupRequest param)
        {
            SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(param.TaskID);
            if (search_info == null)
                return;

            try
            {
                int batch_size = 10;    // 100? 或者根据 data 尺寸动态计算每批的个数
                int send_count = 0;

                List<GroupRecord> records = new List<GroupRecord>();

                ServerInfo.GroupDatabase.GetGroups(param.GroupCondition,
                    (int)param.Start,
                    (int)param.Count,
                    (totalCount, item) =>
                    {
                        if (item != null)
                            records.Add(BuildGroupRecord(item));
                        // 集中发送一次
                        if (records.Count >= batch_size || item == null)
                        {
                            // 让前端获得检索结果
                            try
                            {
                                Clients.Client(search_info.RequestConnectionID).responseGetGroup(
                                    param.TaskID,
                                    (long)totalCount, // resultCount,
                                    (long)send_count,
                                    records,
                                    "", // errorInfo,
                                    "" // errorCode
                                    );
                                send_count += records.Count;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("中心向前端分发 responseGetGroup() 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
                                return false;
                            }
                            records.Clear();
                        }

                        return true;
                    }).Wait();
            }
            finally
            {
                // 主动清除已经完成的检索对象
                ServerInfo.SearchTable.RemoveSearch(param.TaskID);
            }
        }

        static GroupItem BuildGroupItem(GroupRecord record)
        {
            GroupItem item = new GroupItem();
            item.SetID(record.id);
            item.name = record.name;
            item.creator = record.creator;
            item.manager = record.manager;
            item.comment = record.comment;
            item.type = record.type;

            item.createTime = record.createTime;
            item.expireTime = record.expireTime;
            return item;
        }

        static GroupRecord BuildGroupRecord(GroupItem item)
        {
            GroupRecord record = new GroupRecord();
            record.id = item.id;
            record.name = item.name;
            record.creator = item.creator;
            record.manager = item.manager;
            record.comment = item.comment;
            record.type = item.type;

            record.createTime = item.createTime;
            record.expireTime = item.expireTime;
            return record;
        }


        #endregion


        #region SetMessage() API



        // 返回 MessageRecord 数组，因为有些字段是服务器给设定的，例如 PublishTime, ID
        // 但如果让前端知道哪条是哪条呢？方法是返回的数组和请求的数组大小顺序一样
        // 前端得到这些内容的好处是，可以自己在本地刷新显示，不必等服务器发过来通知
        // 在中途出错的情况下，数组中的元素可能少于请求的数目，但顺序是和请求是一致的
        public SetMessageResult SetMessage(
#if NO
            string action,
            List<MessageRecord> messages
#endif
SetMessageRequest param
            )
        {
            SetMessageResult result = new SetMessageResult();
            List<MessageItem> saved_items = new List<MessageItem>();
            List<MessageItem> items = new List<MessageItem>();

            // 转换类型
            foreach (MessageRecord record in param.Records)
            {
                if (string.IsNullOrEmpty(record.group))
                {
                    result.String = "InvalidData";
                    result.Value = -1;
                    result.ErrorInfo = "messages 中包含不合法的 MessageRecord 记录，group 成员不允许为空(可以使用 '<defallt>' 作为默认群组的名字)";
                    return result;
                }
                items.Add(BuildMessageItem(record));
            }

            try
            {
                ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
result,
"SetMessage()",
false); // 没有以用户名登录的 connection 也可以在默认群发出消息
                if (connection_info == null)
                    return result;

                bool bSupervisor = (StringUtil.Contains(connection_info.Rights, "supervisor"));

                if (param.Action == "create")
                {
                    // 注: 超级用户可以在任何群组发布消息

                    foreach (MessageItem item in items)
                    {
                        if (bSupervisor == false)
                        {
                            if (connection_info.UserItem == null)
                            {
                                if (GroupDefinition.IsDefaultGroupName(item.group) == false)
                                {
                                    result.String = "Denied";
                                    result.Value = -1;
                                    result.ErrorInfo = "当前用户(未注册用户)只能在 <default> 组创建消息";
                                    return result;
                                }
                            }
                            else
                            {
                                if (GroupDefinition.IncludeGroup(connection_info.Groups, item.group) == false)
                                {
                                    result.String = "Denied";
                                    result.Value = -1;
                                    result.ErrorInfo = "当前用户没有加入群组 '" + item.group + "'，因此无法在其中创建消息";
                                    return result;
                                }
                            }
                        }

                        item.publishTime = DateTime.Now;
                        // item.expireTime = new DateTime(0);  // 表示永远不失效
                        item.creator = BuildMessageUserID(connection_info);
                        item.userName = connection_info.UserName;
                        item.SetID(Guid.NewGuid().ToString());  // 确保 id 字段有值。是否可以允许前端指定这个 ID 呢？如果要进行查重就麻烦了
                        ServerInfo.MessageDatabase.Add(item).Wait();
                        saved_items.Add(item);
                    }
                }
                else if (param.Action == "change")
                {
                    // 注: 超级用户可以修改任何消息
                    // 其他人只能修改自己原先创建的消息

                    foreach (MessageItem item in items)
                    {
                        if (bSupervisor == false)
                        {
                            if (connection_info.UserItem == null)
                            {
                                if (GroupDefinition.IsDefaultGroupName(item.group) == false)
                                {
                                    result.String = "Denied";
                                    result.Value = -1;
                                    result.ErrorInfo = "当前用户(未注册用户)只能修改 <default> 组内自己发布的消息";
                                    return result;
                                }
                            }
                            else
                            {
                                if (GroupDefinition.IncludeGroup(connection_info.Groups, item.group) == false)
                                {
                                    result.String = "Denied";
                                    result.Value = -1;
                                    result.ErrorInfo = "当前用户没有加入群组 '" + item.group + "'，因此无法修改其中的消息";
                                    return result;
                                }
                            }

                            // 检索出原有的消息，以便核对条件
                            List<MessageItem> results = ServerInfo.MessageDatabase.GetMessageByID(item.id).Result;

                            if (results.Count == 0)
                            {
                                result.String = "NotFound";
                                result.Value = -1;
                                result.ErrorInfo = "id 为 '" + item.id + "' 的消息记录不存在";
                                return result;
                            }

                            MessageItem exist = results[0];
                            string creator_string = BuildMessageUserID(connection_info);

                            if (exist.creator != creator_string)
                            {
                                result.String = "Denied";
                                result.Value = -1;
                                result.ErrorInfo = "因 id 为 '" + item.id + "' 的消息记录原先不是当前用户创建的，修改操作被拒绝";
                                return result;
                            }
                        }

                        ServerInfo.MessageDatabase.Update(item).Wait();
                        saved_items.Add(item);
                    }
                }
                else if (param.Action == "delete")
                {
                    // 注: 超级用户可以删除任何消息
                    // 其他人只能删除自己原先创建的消息

                    foreach (MessageItem item in items)
                    {
                        if (bSupervisor == false)
                        {
                            if (connection_info.UserItem == null)
                            {
                                if (GroupDefinition.IsDefaultGroupName(item.group) == false)
                                {
                                    result.String = "Denied";
                                    result.Value = -1;
                                    result.ErrorInfo = "当前用户(未注册用户)只能删除 <default> 组内自己创建的消息";
                                    return result;
                                }
                            }
                            else
                            {
                                if (GroupDefinition.IncludeGroup(connection_info.Groups, item.group) == false)
                                {
                                    result.String = "Denied";
                                    result.Value = -1;
                                    result.ErrorInfo = "当前用户没有加入群组 '" + item.group + "'，因此无法删除其中的消息";
                                    return result;
                                }
                            }

                            // 检索出原有的消息，以便核对条件
                            List<MessageItem> results = ServerInfo.MessageDatabase.GetMessageByID(item.id).Result;

                            if (results.Count == 0)
                            {
                                result.String = "NotFound";
                                result.Value = -1;
                                result.ErrorInfo = "id 为 '" + item.id + "' 的消息不存在";
                                return result;
                            }

                            MessageItem exist = results[0];
                            string creator_string = BuildMessageUserID(connection_info);

                            if (exist.creator != creator_string)
                            {
                                result.String = "Denied";
                                result.Value = -1;
                                result.ErrorInfo = "因 id 为 '" + item.id + "' 的消息不是当前用户创建的，删除操作被拒绝";
                                return result;
                            }
                        }

                        ServerInfo.MessageDatabase.DeleteByID(item.id).Wait();
                        saved_items.Add(item);
                    }
                }
                else
                {
                    result.String = "ActionError";
                    result.Value = -1;
                    result.ErrorInfo = "无法识别的 action 参数值 '" + param.Action + "'";
                }
            }
            catch (Exception ex)
            {
                result.SetError("SetMessage() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                Console.WriteLine(result.ErrorInfo);
            }
            finally
            {
                // 准备返回的数组
                result.Results = new List<MessageRecord>();
                foreach (MessageItem item in saved_items)
                {
                    MessageRecord record = BuildMessageRecord(item);
                    // 为了节省空间，把 data 等字段清除
                    record.data = null;
                    result.Results.Add(record);
                }

                string[] excludeConnectionIds = null;
                // 通知消息的时候，排除掉请求者自己
                if (StringUtil.IsInList("excludeMe", param.Style) == true)
                {
                    excludeConnectionIds = new string[1];
                    excludeConnectionIds[0] = Context.ConnectionId;
                }
                // 即便遇到抛出异常的情况，先前保存成功的部分消息也可以发送出去
                // 启动一个任务，向相关的前端推送消息，以便它们在界面上显示消息或者变化(比如删除或者修改以后的效果)
                Task.Factory.StartNew(() => PushMessageToClient(param.Action,
                    excludeConnectionIds,
                    saved_items));
            }

            return result;
        }

        // 构造用于消息内 create 字段的 user id
        static string BuildMessageUserID(ConnectionInfo info)
        {
            if (string.IsNullOrEmpty(info.UserID) == false)
                return info.UserID;
            if (string.IsNullOrEmpty(info.LibraryName) == false)
                return "~" + info.LibraryUserName + "@" + info.LibraryName;

            return "~" + info.LibraryUserName + "@" + info.LibraryUID;
        }

        void PushMessageToClient(string action,
            string[] excludeConnectionIds,
            List<MessageItem> messages)
        {
            // 按照 Group 名字，分为若干个 List
            Hashtable table = new Hashtable();  // groupName --> List<MessageItem>
            foreach (MessageItem item in messages)
            {
                Debug.Assert(string.IsNullOrEmpty(item.id) == false, "");

                string groupName = item.group;
                if (string.IsNullOrEmpty(groupName) == true)
                {
                    throw new ArgumentException("MessageItem 对象的 gourp 成员不应为空");
                }
#if NO
                if (string.IsNullOrEmpty(groupName) == true)
                    groupName = "<default>";    // 需要在各个环节正规化组的名字
#endif

                List<MessageRecord> records = (List<MessageRecord>)table[groupName];
                if (records == null)
                {
                    records = new List<MessageRecord>();
                    table[groupName] = records;
                }
                records.Add(BuildMessageRecord(item));
            }

            foreach (string groupName in table.Keys)
            {
                List<MessageRecord> records = (List<MessageRecord>)table[groupName];
                Debug.Assert(records != null, "");
                Debug.Assert(records.Count != 0, "");
                if (excludeConnectionIds == null)
                    Clients.Group(groupName).addMessage(action, records);
                else
                    Clients.Group(groupName, excludeConnectionIds).addMessage(action, records);
            }
        }

        static MessageItem BuildMessageItem(MessageRecord record)
        {
            MessageItem item = new MessageItem();
            item.SetID(record.id);
            item.group = record.group;
            item.creator = record.creator;
            item.userName = record.userName;
            item.data = record.data;
            item.format = record.format;
            item.type = record.type;
            item.thread = record.thread;

            item.publishTime = record.publishTime;
            item.expireTime = record.expireTime;
            return item;
        }

        static MessageRecord BuildMessageRecord(MessageItem item)
        {
            MessageRecord record = new MessageRecord();
            record.id = item.id;
            record.group = item.group;
            record.creator = item.creator;
            record.userName = item.userName;
            record.data = item.data;
            record.format = item.format;
            record.type = item.type;
            record.thread = item.thread;

            record.publishTime = item.publishTime;
            record.expireTime = item.expireTime;
            return record;
        }

        #endregion

        #region GetMessage() API


        public MessageResult RequestGetMessage(GetMessageRequest param)
        {
            // param.Count 为 0 和 -1 意义不同。前者可用于只探索条数，不获取数据。比如在界面上显示有多少条新的信息

            MessageResult result = new MessageResult();

            // 检查参数
            if (string.IsNullOrEmpty(param.GroupCondition) == true)
            {
                result.Value = -1;
                result.String = "InvalidParam";
                result.ErrorInfo = "param 成员 GroupCondition 不应为空";
                return result;
            }

            ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
result,
"RequestGetMessage()",
false);
            if (connection_info == null)
                return result;

            bool bSupervisor = (StringUtil.Contains(connection_info.Rights, "supervisor"));
            if (bSupervisor == false)
            {
                if (string.IsNullOrEmpty(connection_info.UserName))
                {
                    if (GroupDefinition.IsDefaultGroupName(param.GroupCondition) == false)
                    {
                        result.Value = -1;
                        result.String = "Denied";
                        result.ErrorInfo = "未注册的用户只允许查看 <default> 群组的消息，不允许查看群组 '" + param.GroupCondition + "' 的消息";
                        return result;
                    }
                }

                if (GroupDefinition.IncludeGroup(connection_info.Groups, param.GroupCondition) == false)
                {
                    result.Value = -1;
                    result.String = "Denied";
                    result.ErrorInfo = "当前用户不能查看群组 '" + param.GroupCondition + "' 的消息";
                    return result;
                }
            }

            SearchInfo search_info = null;
            try
            {
                search_info = ServerInfo.SearchTable.AddSearch(Context.ConnectionId,
                    param.TaskID,
                    param.Start,
                    param.Count);
            }
            catch (ArgumentException)
            {
                result.Value = -1;
                result.ErrorInfo = "TaskID '" + param.TaskID + "' 已经存在了，不允许重复使用";
                return result;
            }

            result.String = search_info.UID;   // 返回检索请求的 UID

            // 启动一个独立的 Task，该 Task 负责搜集和发送结果信息
            // 这是典型的 dp2MServer 能完成任务的情况，不需要再和另外一个前端通讯
            // 不过，请求本 API 的前端，要做好在 Request 返回以前就先得到数据响应的准备
            Task.Factory.StartNew(() => SearchMessageAndResponse(param));

            result.Value = 1;   // 成功
            return result;
        }

        void SearchMessageAndResponse(
            GetMessageRequest param)
        {
            SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(param.TaskID);
            if (search_info == null)
                return;

            try
            {
                int batch_size = 10;    // 100? 或者根据 data 尺寸动态计算每批的个数
                int send_count = 0;

                List<MessageRecord> records = new List<MessageRecord>();

                ServerInfo.MessageDatabase.GetMessages(param.GroupCondition,
                    (int)param.Start,
                    (int)param.Count,
                    (totalCount, item) =>
                    {
                        if (item != null)
                            records.Add(BuildMessageRecord(item));
                        // 集中发送一次
                        if (records.Count >= batch_size || item == null)
                        {
                            // 让前端获得检索结果
                            try
                            {
                                Clients.Client(search_info.RequestConnectionID).responseGetMessage(
                                    param.TaskID,
                                    (long)totalCount, // resultCount,
                                    (long)send_count,
                                    records,
                                    "", // errorInfo,
                                    "" // errorCode
                                    );
                                send_count += records.Count;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("中心向前端分发 responseGetMessage() 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
                                return false;
                            }
                            records.Clear();
                        }

                        return true;
                    }).Wait();
            }
            finally
            {
                // 主动清除已经完成的检索对象
                ServerInfo.SearchTable.RemoveSearch(param.TaskID);
            }
        }
#if NO
        public GetMessageResult GetMessage(GetMessageRequest param)
        {
            GetMessageResult result = new GetMessageResult();

            try
            {
                ConnectionInfo info = GetConnection(Context.ConnectionId,
            result,
            "GetMessage()",
            true);
                if (info == null)
                    return result;

                // 按照群组名检索；按照用户名检索；按照群组名和用户名检索；加上时间范围限制
                // 或者用 XML 检索式表示

                var items = ServerInfo.MessageDatabase.GetMessages(param.GroupCondition, (int)param.Start, (int)param.Count).Result;
                result.Results = new List<MessageRecord>();
                foreach (MessageItem item in items)
                {
                    result.Results.Add(BuildMessageRecord(item));
                }
                result.Value = 0;
                return result;
            }
            catch (Exception ex)
            {
                result.Value = -1;
                result.ErrorInfo = "GetMessage() 出错：" + ExceptionUtil.GetExceptionText(ex);
                return result;
            }
        }
#endif

        #endregion


        #region GetUsers() API

        public GetUserResult GetUsers(string userName, int start, int count)
        {
            GetUserResult result = new GetUserResult();

            try
            {
#if NO
                ConnectionInfo info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
                if (info == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。操作失败";
                    result.String = "_connectionNotFound";
                    return result;
                }

                if (info.UserItem == null)
                {
                    result.Value = -1;
                    result.String = "_notLogin";
                    result.ErrorInfo = "尚未登录，无法使用 GetUsers() 功能";
                    return result;
                }
#endif
                ConnectionInfo info = GetConnection(Context.ConnectionId,
            result,
            "GetUsers()",
            true);
                if (info == null)
                    return result;

                // supervisor 权限用户，可以获得所有用户的信息
                if (StringUtil.Contains(info.UserItem == null ? "" : info.UserItem.rights,
                    "supervisor") == true)
                {
                    var task = ServerInfo.UserDatabase.GetUsersByName(userName, start, count);
                    task.Wait();
                    result.Users = BuildUsers(task.Result);
                }
                else
                {
                    // 否则只能获得自己的用户信息
                    string strCurrentUserName = info.UserItem == null ? "" : info.UserItem.userName;

                    if (userName == "*" || string.IsNullOrEmpty(userName) == true)
                        userName = strCurrentUserName;
                    else
                    {
                        // userName 参数为 * 或者 空 以外的值
                        result.Value = -1;
                        result.ErrorInfo = "当前用户身份 '" + strCurrentUserName + "' 无法获得用户名为 '" + userName + "' 的用户信息";
                        return result;
                    }

                    var task = ServerInfo.UserDatabase.GetUsersByName(userName, start, count);
                    task.Wait();
                    result.Users = BuildUsers(task.Result);
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Value = -1;
                result.ErrorInfo = "GetUsers() 出错：" + ExceptionUtil.GetExceptionText(ex);
                return result;
            }
        }

        #endregion

        #region SetUsers() API

        // 设置用户。包括增删改功能
        // 可能返回的错误码:
        //      Denied
        public MessageResult SetUsers(string action, List<UserItem> users)
        {
            MessageResult result = new MessageResult();

            try
            {
#if NO
                ConnectionInfo info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
                if (info == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。操作失败";
                    return result;
                }
#endif
                ConnectionInfo info = GetConnection(Context.ConnectionId,
result,
"SetUsers()",
true);
                if (info == null)
                    return result;


                if (action == "create")
                {
                    // 验证请求者是否有 supervisor 权限
                    if (StringUtil.Contains(info.Rights, "supervisor") == false)
                    {
                        result.String = "Denied";
                        result.Value = -1;
                        result.ErrorInfo = "当前用户不具备 supervisor 权限，create 命令被拒绝";
                        return result;
                    }

                    foreach (UserItem item in users)
                    {
                        ServerInfo.UserDatabase.Add(item).Wait();
                    }
                }
                else if (action == "change")
                {
                    // TODO: 如果 groups 成员发生了修改，要重新加入 SignalR 的 group
                    // 或者为了省事，全部重新加入一次

                    // 验证请求者是否有 supervisor 权限
                    if (StringUtil.Contains(info.Rights, "supervisor") == false)
                    {
                        result.String = "Denied";
                        result.Value = -1;
                        result.ErrorInfo = "当前用户不具备 supervisor 权限，change 命令被拒绝";
                        return result;
                    }

                    foreach (UserItem item in users)
                    {
                        ServerInfo.UserDatabase.Update(item).Wait();
                    }
                }
                else if (action == "changePassword")
                {
                    // 超级用户可以修改所有用户的密码。而普通用户只能修改自己的密码
                    foreach (UserItem item in users)
                    {
                        if (StringUtil.Contains(info.Rights, "supervisor") == false
                            && item.userName != info.UserName)
                        {
                            result.String = "Denied";
                            result.Value = -1;
                            result.ErrorInfo = "当前用户不具备 supervisor 权限，试图修改其他用户 '" + item.userName + "' 密码的 changePassword 命令被拒绝";
                            return result;
                        }

                        ServerInfo.UserDatabase.UpdatePassword(item).Wait();
                    }
                }
                else if (action == "delete")
                {
                    // TODO: 注意从 SignalR 的 group 中 remove

                    // 验证请求者是否有 supervisor 权限
                    if (StringUtil.Contains(info.Rights, "supervisor") == false)
                    {
                        result.String = "Denied";
                        result.Value = -1;
                        result.ErrorInfo = "当前用户不具备 supervisor 权限，delete 命令被拒绝";
                        return result;
                    }

                    foreach (UserItem item in users)
                    {
                        ServerInfo.UserDatabase.Delete(item).Wait();
                    }
                }
                else
                {
                    result.String = "ActionError";
                    result.Value = -1;
                    result.ErrorInfo = "无法识别的 action 参数值 '" + action + "'";
                }

                // 启动一个任务，刷新已经登录的连接中保持在内存的账户信息
                Task.Factory.StartNew(() => RefreshUserInfo(action, users));
            }
            catch (Exception ex)
            {
                result.SetError("SetUsers() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                Console.WriteLine(result.ErrorInfo);

#if NO
                result.String = "Exception";
                result.Value = -1;
                result.ErrorInfo = "SetUsers() 出错：" + ExceptionUtil.GetExceptionText(ex);
                return result;
#endif
            }
            return result;
        }


        // 构造适于返回给前端的 User 对象列表
        static List<User> BuildUsers(List<UserItem> items)
        {
            List<User> results = new List<User>();
            foreach (UserItem item in items)
            {
                results.Add(BuildUser(item));
            }
            return results;
        }

        // 构造适于返回给前端的 User 对象
        static User BuildUser(UserItem item)
        {
            User user = new User();
            user.id = item.id;
            user.userName = item.userName;
            user.password = ""; // 密码不必返回给前端，因为这是 hash 以后的字符串了。 item.password;
            user.rights = item.rights;
            user.duty = item.duty;
            user.department = item.department;
            user.tel = item.tel;
            user.comment = item.comment;
            user.groups = item.groups;
            return user;
        }


        void RefreshUserInfo(string action, List<UserItem> users)
        {
            List<ConnectionInfo> delete_connections = new List<ConnectionInfo>();
            foreach (ConnectionInfo info in ServerInfo.ConnectionTable)
            {
                foreach (UserItem user in users)
                {
                    if (user.userName == info.UserName)
                    {
                        if (action == "change")
                        {
                            RefreshConnectionInfo(info, user);
                        }
                        else if (action == "changePassword"
                            || action == "delete")
                        {
                            delete_connections.Add(info);   // 希望强制登出
                        }
                        break;
                    }
                }
            }

            foreach (ConnectionInfo info in delete_connections)
            {
                ServerInfo.ConnectionTable.RemoveConnection(info.ConnectionID);
            }

            // TODO: dp2Capo 遇到中途报错没有登录的情况，会自动重试登录么?
            // 要测试一下密码被修改的情况，并且要把重新登录失败的情况记入事件日志，让管理员能看到
            // 还有一个办法就是，遇到 dp2mserver 主动 logout 一个 connection 的情况，可以给前端发起一个调用，通知它 login 状态变化了，需要及时做出处理。这个机制可以和其他机制结合使用。因为这种通知也是不保险的，需要结合其他机制才能完整
        }

        // 用 item 的信息更新 info 里面的对应字段
        static void RefreshConnectionInfo(ConnectionInfo info, UserItem item)
        {
            if (info.UserItem == null)
                return;

            // id 不要更新
            info.UserItem.userName = item.userName;
            info.UserItem.rights = item.rights;

            info.UserItem.duty = item.duty;
            info.UserItem.department = item.department;
            info.UserItem.tel = item.tel;
            info.UserItem.comment = item.comment;
            info.UserItem.groups = item.groups;
        }

        #endregion

        #region Login() API

        // 登录，并告知 server 关于自己的一些属性。如果不登录，则 server 会按照缺省的方式设置这些属性，例如无法实现检索响应功能
        // parameters:
        //      propertyList    属性列表。
        // 错误码
        //      异常
        MessageResult Login(
LoginRequest param
            )
        {
            MessageResult result = new MessageResult();
            try
            {
#if NO
                ConnectionInfo info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
                if (info == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。登录失败";
                    return result;
                }
#endif
                ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
result,
"Login()",
false);
                if (connection_info == null)
                    return result;

                if (string.IsNullOrEmpty(param.UserName) == false)
                {
                    // 获得用户信息
                    var results = ServerInfo.UserDatabase.GetUsersByName(param.UserName, 0, 1).Result;
                    if (results.Count != 1)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "用户名 '" + param.UserName + "' 不存在。登录失败";
                        return result;
                    }
                    var user = results[0];
                    string strHashed = Cryptography.GetSHA1(param.Password);

                    if (user.password != strHashed)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "密码不正确。登录失败";
                        return result;
                    }
                    connection_info.UserItem = user;
                }

                // 加入 SignalR 的 group
                if (connection_info.UserItem != null
                    && connection_info.UserItem.groups != null)
                {
                    foreach (string group in connection_info.UserItem.groups)
                    {
                        Groups.Add(Context.ConnectionId, group);
                    }
                }
                foreach (string group in default_groups)
                {
                    Groups.Add(Context.ConnectionId, group);
                }

                connection_info.PropertyList = param.PropertyList;
                connection_info.LibraryUID = param.LibraryUID;
                connection_info.LibraryName = param.LibraryName;
                connection_info.LibraryUserName = param.LibraryUserName;
            }
            catch (Exception ex)
            {
                result.SetError("Login() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                Console.WriteLine(result.ErrorInfo);
            }
            return result;
        }

        #endregion

        #region Logout() API

        // 错误码:
        //      异常
        MessageResult Logout()
        {
            MessageResult result = new MessageResult();
            try
            {
#if NO
                ConnectionInfo info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
                if (info == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。登出失败";
                    return result;
                }
#endif
                ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
result,
"Logout()",
false);
                if (connection_info == null)
                    return result;

                // 登出。但连接依然存在
                connection_info.UserItem = null;
                connection_info.PropertyList = "";
                connection_info.LibraryUID = "";
                connection_info.LibraryName = "";
                connection_info.SearchCount = 0;

                // 从 SignalR 的 group 中移走
                if (connection_info.UserItem != null
                    && connection_info.UserItem.groups != null)
                {
                    foreach (string group in connection_info.UserItem.groups)
                    {
                        Groups.Remove(Context.ConnectionId, group);
                    }
                }

                foreach (string group in default_groups)
                {
                    Groups.Remove(Context.ConnectionId, group);
                }

            }
            catch (Exception ex)
            {
                result.SetError("Login() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                Console.WriteLine(result.ErrorInfo);
            }
            return result;
        }

        #endregion

#if NO
        // parameters:
        //      userNames   被请求的用户名列表
        //      recordType  什么类型的记录。书目库记录？读者库记录？实体记录?
        //      recPath 记录路径
        //      input   前端提供给处理者的记录。这个参数一般比较罕用
        //      style   处理风格。也就是对处理的附加要求
        //      formats 要返回的记录格式列表
        // return:
        //      result.Value    -1 出错; 0 没有任何检索目标; 1 成功发起检索
        public GetRecordResult RequestGetRecord(
            List<string> userNames,
            string searchID,
            string recordType,
            string recPath,
            string input,
            string style,
            List<string> formats)
        {
            GetRecordResult result = new GetRecordResult();

            ConnectionInfo connection_info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
            if (connection_info == null)
            {
                result.Value = -1;
                result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。请求检索书目失败";
                return result;
            }

#if NO
            if (Global.Contains(connection_info.PropertyList, "biblio_search") == false)
            {
                result.Value = -1;
                result.ErrorInfo = "当前连接未开通书目检索功能";
                return result;
            }
#endif

            // TODO: 改造为，根据用户名获得 connectionId 列表
            List<string> connectionIds = null;
            string strError = "";
            // 获得书目检索的目标 connection 的 id 集合
            // parameters:
            //      strRequestLibraryUID    发起检索的人所在的图书馆的 UID。本函数要在返回结果中排除这个 UID 的图书馆的连接
            // return:
            //      -1  出错
            //      0   成功
            int nRet = ServerInfo.ConnectionTable.GetBiblioSearchTargets(
                connection_info.LibraryUID,
                out connectionIds,
                out strError);
            if (nRet == -1)
            {
                result.Value = -1;
                result.ErrorInfo = strError;
                return result;
            }

            if (connectionIds == null || connectionIds.Count == 0)
            {
                result.Value = 0;
                result.ErrorInfo = "当前没有任何可操作的目标";
                return result;
            }

            SearchInfo search_info = null;

            try
            {
                search_info = ServerInfo.SearchTable.AddSearch(Context.ConnectionId, searchID);
            }
            catch (ArgumentException)
            {
                result.Value = -1;
                result.ErrorInfo = "SearchID '" + searchID + "' 已经存在了，不允许重复使用";
                return result;
            }

            result.String = search_info.UID;   // 返回检索请求的 UID

            Clients.Clients(connectionIds).getRecord(// "searchBiblio",
                search_info.UID,   // 检索请求的 UID
                recordType,
                recPath,
                input,
                style);

            search_info.TargetIDs = connectionIds;
            result.Value = 1;   // 表示已经成功发起了检索
            return result;
        }
#endif

        #region GetConnectionInfo() API

        public MessageResult RequestGetConnectionInfo(GetConnectionInfoRequest param)
        {
            MessageResult result = new MessageResult();

#if NO
            ConnectionInfo connection_info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
            if (connection_info == null)
            {
                result.Value = -1;
                result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。请求检索书目失败";
                return result;
            }

            if (connection_info.UserItem == null)
            {
                result.Value = -1;
                result.ErrorInfo = "尚未登录，无法使用 RequestGetConnectionInfo() 功能";
            }
#endif
            ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
result,
"RequestGetConnectionInfo()",
true);
            if (connection_info == null)
                return result;

            SearchInfo search_info = null;
            try
            {
                search_info = ServerInfo.SearchTable.AddSearch(Context.ConnectionId,
                    param.TaskID,
                    param.Start,
                    param.Count);
            }
            catch (ArgumentException)
            {
                result.Value = -1;
                result.ErrorInfo = "TaskID '" + param.TaskID + "' 已经存在了，不允许重复使用";
                return result;
            }

            result.String = search_info.UID;   // 返回检索请求的 UID

            // 启动一个独立的 Task，该 Task 负责搜集和发送结果信息
            // 这是典型的 dp2MServer 能完成任务的情况，不需要再和另外一个前端通讯
            // 不过，请求本 API 的前端，要做好在 Request 返回以前就先得到数据响应的准备
            Task.Factory.StartNew(() => SearchConnectionInfoAndResponse(param));

            result.Value = 1;   // 成功
            return result;
        }

        void SearchConnectionInfoAndResponse(
            GetConnectionInfoRequest param)
        {
            SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(param.TaskID);
            if (search_info == null)
                return;

            try
            {
                int batch_size = 10;
                int send_count = 0;
                List<ConnectionRecord> records = new List<ConnectionRecord>();
                int i = 0;
                int count = ServerInfo.ConnectionTable.Count;
                foreach (ConnectionInfo info in ServerInfo.ConnectionTable)
                {
                    // TODO: 根据请求者的级别不同，所获得的信息详尽程度不同
                    ConnectionRecord record = new ConnectionRecord(info.UserName,
            info.Rights,
            info.Duty,
            info.Department, // department,
            info.Tel, // tel,
            info.Comment, // comment,
            info.LibraryUID,
            info.LibraryName,
            info.LibraryUserName,
            info.PropertyList);
                    records.Add(record);
                    if (records.Count >= batch_size
                        || i == count - 1)
                    {
                        // 每次发送前，重新试探获得一次，可以有效探知前端已经 CancelSearch() 的情况
                        search_info = ServerInfo.SearchTable.GetSearchInfo(param.TaskID);
                        if (search_info == null)
                            return;

                        // 让前端获得检索结果
                        try
                        {
                            Clients.Client(search_info.RequestConnectionID).responseGetConnectionInfo(
                                param.TaskID,
                                (long)count, // resultCount,
                                (long)send_count,
                                records,
                                "", // errorInfo,
                                "" // errorCode
                                );
                            send_count += records.Count;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("中心向前端分发 responseGetConnectionInfo() 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
                        }
                        records.Clear();
                    }

                    i++;
                }
            }
            finally
            {
                // 主动清除已经完成的检索对象
                ServerInfo.SearchTable.RemoveSearch(param.TaskID);
            }
        }

#if NO
        public MessageResult ResponseGetConnectionInfo(string taskID,
    long resultCount,
    long start,
    IList<ConnectionRecord> records,
    string errorInfo,
    string errorCode)
        {
            // Thread.Sleep(1000 * 60 * 2);
            MessageResult result = new MessageResult();
            try
            {
                Console.WriteLine("ResponseSearch start=" + start
                    + ", records.Count=" + (records == null ? "null" : records.Count.ToString())
                    + ", errorInfo=" + errorInfo
                    + ", errorCode=" + errorCode);

                SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(taskID);
                if (search_info == null)
                {
                    result.ErrorInfo = "ID 为 '" + taskID + "' 的检索对象无法找到";
                    result.Value = -1;
                    result.String = "_notFound";
                    return result;
                }

                // 给 RecPath 加上 @ 部分
                if (records != null)
                {
                    ConnectionInfo connection_info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
                    if (connection_info == null)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。回传检索结果失败";
                        return result;
                    }
                    string strPostfix = connection_info.LibraryUID;
                    if (string.IsNullOrEmpty(strPostfix) == true)
                        strPostfix = connection_info.LibraryName;

                    foreach (Record record in records)
                    {
                        record.RecPath = record.RecPath + "@" + strPostfix;
                    }
                }

                // 让前端获得检索结果
                try
                {
                    Clients.Client(search_info.RequestConnectionID).responseSearch(
                        taskID,
                        resultCount,
                        start,
                        records,
                        errorInfo,
                        errorCode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("中心向前端分发 responseSearch() 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
                }

                // 判断响应是否为最后一个响应
                bool bRet = IsComplete(resultCount,
                    search_info.ReturnStart,
                    search_info.ReturnCount,
                    start,
                    records);
                if (bRet == true)
                {
                    bool bAllComplete = search_info.CompleteTarget(Context.ConnectionId);
                    if (bAllComplete)
                    {
                        // 追加一个消息，表示检索响应已经全部完成
                        Clients.Client(search_info.RequestConnectionID).responseSearch(
        taskID,
        -1,
        -1,
        null,
        "",
        "");
                        // 主动清除已经完成的检索对象
                        ServerInfo.SearchTable.RemoveSearch(taskID);
                        Console.WriteLine("complete");
                    }
                }
            }
            catch (Exception ex)
            {
                result.SetError("ResponseGetConnectionInfo() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                Console.WriteLine(result.ErrorInfo);
            }
            return result;
        }
#endif
        #endregion

        #region CancelSearch() API
        // 错误码:
        //      _notFound/异常
        public MessageResult CancelSearch(string taskID)
        {
            MessageResult result = new MessageResult();
            try
            {
                ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
result,
"CancelSearch()",
true);
                if (connection_info == null)
                    return result;

                // TODO: 要确信当前 connection 是启动 Search 的
                SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(taskID);
                if (search_info == null)
                {
                    result.ErrorInfo = "ID 为 '" + taskID + "' 的检索对象无法找到";
                    result.Value = -1;
                    result.String = "_notFound";
                    return result;
                }

                ServerInfo.SearchTable.RemoveSearch(taskID);
            }
            catch (Exception ex)
            {
                result.SetError("CancelSearch() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                Console.WriteLine(result.ErrorInfo);
            }
            return result;
        }

        #endregion

        #region Search() API


        // return:
        //      result.Value    -1 出错; 0 没有任何检索目标; 1 成功发起检索
        public MessageResult RequestSearch(
            string userNameList,
            SearchRequest searchParam
            )
        {
            if (searchParam.Count == 0)
                searchParam.Count = -1;

            MessageResult result = new MessageResult();

#if NO
            ConnectionInfo connection_info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
            if (connection_info == null)
            {
                result.Value = -1;
                result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。请求检索书目失败";
                return result;
            }

            if (connection_info.UserItem == null)
            {
                result.Value = -1;
                result.ErrorInfo = "尚未登录，无法使用 RequestSearch() 功能";
            }
#endif
            ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
result,
"RequestSearch()",
false);
            if (connection_info == null)
                return result;

            if (searchParam.Operation == "searchBiblio"
                && userNameList == "*"
                && StringUtil.Contains(connection_info.PropertyList, "biblio_search") == false)
            {
                result.Value = -1;
                result.ErrorInfo = "当前连接未开通书目检索功能";
                return result;
            }

            // 检查请求者是否具备操作的权限
            if (searchParam.Operation == "searchBiblio"
    && userNameList == "*"
    && StringUtil.Contains(connection_info.PropertyList, "biblio_search") == true)
            {
                // 请求者具有共享检索资格
            }
            else
            {
                if (StringUtil.Contains(connection_info.Rights, searchParam.Operation) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "当前用户 '" + connection_info.UserName + "' 不具备进行 '" + searchParam.Operation + "' 操作的权限";
                    return result;
                }
            }

            List<string> connectionIds = null;
            string strError = "";

            if (searchParam.Operation == "searchBiblio"
    && userNameList == "*")
            {
                // 获得书目检索的目标 connection 的 id 集合
                // parameters:
                //      strRequestLibraryUID    发起检索的人所在的图书馆的 UID。本函数要在返回结果中排除这个 UID 的图书馆的连接
                // return:
                //      -1  出错
                //      0   成功
                int nRet = ServerInfo.ConnectionTable.GetBiblioSearchTargets(
                    connection_info.LibraryUID,
                    false,
                    out connectionIds,
                    out strError);
                if (nRet == -1)
                {
                    result.Value = -1;
                    result.ErrorInfo = strError;
                    return result;
                }
            }
            else
            {
                int nRet = ServerInfo.ConnectionTable.GetOperTargetsByUserName(
                    userNameList,
                    connection_info.UserName,
                    searchParam.Operation,
                    "all",
                    out connectionIds,
                    out strError);
                if (nRet == -1)
                {
                    result.Value = -1;
                    result.ErrorInfo = strError;
                    return result;
                }
            }

            if (connectionIds == null || connectionIds.Count == 0)
            {
                result.Value = 0;
                // result.ErrorInfo = "当前没有任何可检索的目标 (目标用户名 '"+userNameList+"'; 操作 '"+searchParam.Operation+"')";
                result.ErrorInfo = "当前没有发现可检索的目标 (详情 '" + strError + "')";
                return result;
            }

            SearchInfo search_info = null;
            try
            {
                search_info = ServerInfo.SearchTable.AddSearch(Context.ConnectionId,
                    searchParam.TaskID,
                    searchParam.Start,
                    searchParam.Count);
            }
            catch (ArgumentException)
            {
                result.Value = -1;
                result.ErrorInfo = "TaskID '" + searchParam.TaskID + "' 已经存在了，不允许重复使用";
                return result;
            }

            result.String = search_info.UID;   // 返回检索请求的 UID
            search_info.SetTargetIDs(connectionIds);

            Clients.Clients(connectionIds).search(// "searchBiblio",
#if NO
                search_info.UID,   // 检索请求的 UID
                operation,
                dbNameList,
                queryWord,
                fromList,
                matchStyle,
                formatList,
                maxResults
#endif
                searchParam);

            result.Value = 1;   // 表示已经成功发起了检索
            return result;
#if NO
            SearchInfo info = ServerInfo.AddSearch(Context.ConnectionId);

            // TODO: 筛选目标，只发给那些具有可检索属性的目标
            // 或者用 group 机制
            Clients.All.searchBiblio("searchBiblio",
                info.UID,   // 检索请求的 UID
                dbNameList,
                queryWord,
                fromList,
                macthStyle,
                formatList,
                maxResults);
            return info.UID;
#endif
        }

        // object _lock = new object();

        // parameters:
        //      resultCount    命中的总的结果数。如果为 -1，表示检索出错，errorInfo 会给出出错信息
        //                      这个值实际上是表示全部命中结果的数目，可能比 records 中的元素要多
        //      start  records 参数中的第一个元素，在总的命中结果集中的偏移
        //      errorInfo   错误信息
        public MessageResult ResponseSearch(string taskID,
            long resultCount,
            long start,
            IList<Record> records,
            string errorInfo,
            string errorCode)
        {
            // Thread.Sleep(1000 * 60 * 2);
            MessageResult result = new MessageResult();
            try
            {
                Console.WriteLine("ResponseSearch start=" + start
                    + ", records.Count=" + (records == null ? "null" : records.Count.ToString())
                    + ", errorInfo=" + errorInfo
                    + ", errorCode=" + errorCode);

                SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(taskID);
                if (search_info == null)
                {
                    result.ErrorInfo = "ID 为 '" + taskID + "' 的检索对象无法找到";
                    result.Value = -1;
                    result.String = "_notFound";
                    return result;
                }

                // 给 RecPath 加上 @ 部分
                if (records != null)
                {
                    ConnectionInfo connection_info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
                    if (connection_info == null)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。回传检索结果失败";
                        return result;
                    }
                    string strPostfix = connection_info.LibraryUID;
                    if (string.IsNullOrEmpty(strPostfix) == true)
                        strPostfix = connection_info.LibraryName;

                    foreach (Record record in records)
                    {
                        record.RecPath = record.RecPath + "@" + strPostfix;
                    }
                }

                // 让前端获得检索结果
                try
                {
                    Clients.Client(search_info.RequestConnectionID).responseSearch(
                        taskID,
                        resultCount,
                        start,
                        records,
                        errorInfo,
                        errorCode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("中心向前端分发 responseSearch() 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
                }

                // 判断响应是否为最后一个响应
                bool bRet = IsComplete(resultCount,
                    search_info.ReturnStart,
                    search_info.ReturnCount,
                    start,
                    records);
                if (bRet == true)
                {
                    bool bAllComplete = search_info.CompleteTarget(Context.ConnectionId);
                    if (bAllComplete)
                    {
                        // 追加一个消息，表示检索响应已经全部完成
                        Clients.Client(search_info.RequestConnectionID).responseSearch(
        taskID,
        -1,
        -1,
        null,
        "",
        "");
                        // 主动清除已经完成的检索对象
                        ServerInfo.SearchTable.RemoveSearch(taskID);
                        Console.WriteLine("complete");
                    }
                }

            }
            catch (Exception ex)
            {
                result.SetError("ResponseSearch() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                Console.WriteLine(result.ErrorInfo);
            }
            return result;
        }

        #endregion

        #region SetInfo() API

        // return:
        //      result.Value    -1 出错; 0 没有任何检索目标; 1 成功发起检索
        public MessageResult RequestSetInfo(
            string userNameList,
            SetInfoRequest setInfoParam
            )
        {
            MessageResult result = new MessageResult();

#if NO
            ConnectionInfo connection_info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
            if (connection_info == null)
            {
                result.Value = -1;
                result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。请求检索书目失败";
                return result;
            }

            if (connection_info.UserItem == null)
            {
                result.Value = -1;
                result.ErrorInfo = "尚未登录，无法使用 RequestSetInfo() 功能";
            }
#endif
            ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
result,
"RequestSetInfo()",
true);
            if (connection_info == null)
                return result;

            // 检查请求者是否具备操作的权限
            if (StringUtil.Contains(connection_info.Rights, setInfoParam.Operation) == false)
            {
                result.Value = -1;
                result.ErrorInfo = "当前用户 '" + connection_info.UserName + "' 不具备进行 '" + setInfoParam.Operation + "' 操作的权限";
                return result;
            }

            List<string> connectionIds = null;
            string strError = "";
            int nRet = ServerInfo.ConnectionTable.GetOperTargetsByUserName(
                userNameList,
                connection_info.UserName,
                setInfoParam.Operation,
                "all",
                out connectionIds,
                out strError);
            if (nRet == -1)
            {
                result.Value = -1;
                result.ErrorInfo = strError;
                return result;
            }

            if (connectionIds == null || connectionIds.Count == 0)
            {
                result.Value = 0;
                result.ErrorInfo = "当前没有任何可操作的目标";
                return result;
            }

            SearchInfo search_info = null;

            try
            {
                search_info = ServerInfo.SearchTable.AddSearch(Context.ConnectionId,
                    setInfoParam.TaskID);
            }
            catch (ArgumentException)
            {
                result.Value = -1;
                result.ErrorInfo = "TaskID '" + setInfoParam.TaskID + "' 已经存在了，不允许重复使用";
                return result;
            }

            result.String = search_info.UID;   // 返回检索请求的 UID
            search_info.SetTargetIDs(connectionIds);

            Clients.Clients(connectionIds).setInfo(
                setInfoParam);

            result.Value = 1;   // 表示已经成功发起了操作
            return result;
        }

        // TODO: 是否会出现一次发送不完所有元素的情况?
        // parameters:
        //      resultCount    命中的总的结果数。如果为 -1，表示检索出错，errorInfo 会给出出错信息
        //      start  records 参数中的第一个元素，在总的命中结果集中的偏移
        //      errorInfo   错误信息
        public MessageResult ResponseSetInfo(string taskID,
            long resultValue,
            IList<Entity> entities,
            string errorInfo)
        {
            // Thread.Sleep(1000 * 60 * 2);
            MessageResult result = new MessageResult();
            try
            {
                // TODO: 要附加对方的 id 进行检查，看看是不是这个 task 请求过的对方。以免出现被冒用的情况
                SearchInfo info = ServerInfo.SearchTable.GetSearchInfo(taskID);
                if (info == null)
                {
                    result.ErrorInfo = "找不到 ID 为 '" + taskID + "' 的任务对象";
                    result.Value = -1;
                    return result;
                }

#if NO
            // 给 RecPath 加上 @ 部分
            if (records != null)
            {
                ConnectionInfo connection_info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
                if (connection_info == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。回传检索结果失败";
                    return result;
                }
                foreach (Record record in records)
                {
                    // record.RecPath += "@UID:" + connection_info.LibraryUID;
                    record.LibraryName = connection_info.LibraryName;
                    record.LibraryUID = connection_info.LibraryUID;
                }
            }
#endif

                // 让前端获得操作结果
                Clients.Client(info.RequestConnectionID).responseSetInfo(
                    taskID,
                    resultValue,
                    entities,
                    errorInfo);
            }
            catch (Exception ex)
            {
                result.SetError("ResponseSetInfo() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                Console.WriteLine(result.ErrorInfo);
            }
            return result;
        }

        #endregion

        #region BindPatron() API

        // return:
        //      result.Value    -1 出错; 0 没有任何检索目标; 1 成功发起检索
        public MessageResult RequestBindPatron(
            string userNameList,
            BindPatronRequest bindPatronParam
            )
        {
            MessageResult result = new MessageResult();

#if NO
            ConnectionInfo connection_info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
            if (connection_info == null)
            {
                result.Value = -1;
                result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。请求检索书目失败";
                return result;
            }

            if (connection_info.UserItem == null)
            {
                result.Value = -1;
                result.ErrorInfo = "尚未登录，无法使用 RequestBindPatron() 功能";
            }
#endif
            ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
result,
"RequestBindPatron()",
true);
            if (connection_info == null)
                return result;

            // 检查请求者是否具备操作的权限
            if (StringUtil.Contains(connection_info.Rights, "bindPatron") == false)
            {
                result.Value = -1;
                result.ErrorInfo = "当前用户 '" + connection_info.UserName + "' 不具备进行 'bindPatron' 操作的权限";
                return result;
            }

            List<string> connectionIds = null;
            string strError = "";
            int nRet = ServerInfo.ConnectionTable.GetOperTargetsByUserName(
                userNameList,
                connection_info.UserName,
                "bindPatron",
                "all",
                out connectionIds,
                out strError);
            if (nRet == -1)
            {
                result.Value = -1;
                result.ErrorInfo = strError;
                return result;
            }

            if (connectionIds == null || connectionIds.Count == 0)
            {
                result.Value = 0;
                result.ErrorInfo = "当前没有任何可操作的目标: " + strError;
                return result;
            }

            SearchInfo search_info = null;

            try
            {
                search_info = ServerInfo.SearchTable.AddSearch(Context.ConnectionId,
                    bindPatronParam.TaskID);
            }
            catch (ArgumentException)
            {
                result.Value = -1;
                result.ErrorInfo = "TaskID '" + bindPatronParam.TaskID + "' 已经存在了，不允许重复使用";
                return result;
            }

            result.String = search_info.UID;   // 返回操作请求的 UID
            search_info.SetTargetIDs(connectionIds);

            Clients.Clients(connectionIds).bindPatron(
                bindPatronParam);

            result.Value = 1;   // 表示已经成功发起了操作
            return result;
        }

        // parameters:
        public MessageResult ResponseBindPatron(string taskID,
            long resultValue,
            List<string> results,
            string errorInfo)
        {
            // Thread.Sleep(1000 * 60 * 2);
            MessageResult result = new MessageResult();
            try
            {
                SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(taskID);
                if (search_info == null)
                {
                    result.ErrorInfo = "找不到 ID 为 '" + taskID + "' 的任务对象";
                    result.Value = -1;
                    return result;
                }

                // 让前端获得检索结果
                Clients.Client(search_info.RequestConnectionID).responseBindPatron(
                    taskID,
                    resultValue,
                    results,
                    errorInfo);
            }
            catch (Exception ex)
            {
                result.SetError("ResponseBindPatron() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
                    ex.GetType().ToString());
                Console.WriteLine(result.ErrorInfo);
            }
            return result;
        }

        #endregion

        #region Circulation() API

        // return:
        //      result.Value    -1 出错; 0 没有任何检索目标; 1 成功发起检索
        public MessageResult RequestCirculation(
            string userNameList,
            CirculationRequest circulationParam)
        {
            MessageResult result = new MessageResult();

#if NO
            ConnectionInfo connection_info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
            if (connection_info == null)
            {
                result.Value = -1;
                result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。请求检索书目失败";
                return result;
            }

            if (connection_info.UserItem == null)
            {
                result.Value = -1;
                result.ErrorInfo = "尚未登录，无法使用 RequestCirculation() 功能";
            }
#endif
            ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
result,
"RequestCirculation()",
true);
            if (connection_info == null)
                return result;

            // 检查请求者是否具备操作的权限
            if (StringUtil.Contains(connection_info.Rights, "circulation") == false)
            {
                result.Value = -1;
                result.ErrorInfo = "当前用户 '" + connection_info.UserName + "' 不具备进行 'circulation' 操作的权限";
                return result;
            }

            List<string> connectionIds = null;
            string strError = "";
            int nRet = ServerInfo.ConnectionTable.GetOperTargetsByUserName(
                userNameList,
                connection_info.UserName,
                "circulation",
                "all",
                out connectionIds,
                out strError);
            if (nRet == -1)
            {
                result.Value = -1;
                result.ErrorInfo = strError;
                return result;
            }

            if (connectionIds == null || connectionIds.Count == 0)
            {
                result.Value = 0;
                result.ErrorInfo = "当前没有任何可操作的目标: " + strError;
                return result;
            }

            SearchInfo search_info = null;
            try
            {
                search_info = ServerInfo.SearchTable.AddSearch(Context.ConnectionId,
                    circulationParam.TaskID);
            }
            catch (ArgumentException)
            {
                result.Value = -1;
                result.ErrorInfo = "TaskID '" + circulationParam.TaskID + "' 已经存在了，不允许重复使用";
                return result;
            }

            result.String = search_info.UID;   // 返回操作请求的 UID
            search_info.SetTargetIDs(connectionIds);

            try
            {
                Clients.Clients(connectionIds).circulation(
                    circulationParam);
            }
            catch (Exception ex)
            {
                result.Value = -1;
                result.ErrorInfo = "分发 circulation 请求时出现异常: " + ExceptionUtil.GetExceptionText(ex);
                return result;
            }
            result.Value = 1;   // 表示已经成功发起了操作
            return result;
        }

        // parameters:
        public MessageResult ResponseCirculation(string taskID,
            CirculationResult circulation_result)
        {
            // Thread.Sleep(1000 * 60 * 2);
            MessageResult result = new MessageResult();
            try
            {
                SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(taskID);
                if (search_info == null)
                {
                    result.ErrorInfo = "找不到 ID 为 '" + taskID + "' 的任务对象";
                    result.Value = -1;
                    return result;
                }

                // 让前端获得检索结果
                Clients.Client(search_info.RequestConnectionID).responseCirculation(
                    taskID,
                    circulation_result);
            }
            catch (Exception ex)
            {
                result.SetError("ResponseCirculation() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
    ex.GetType().ToString());
                Console.WriteLine(result.ErrorInfo);
            }
            return result;
        }

        #endregion

        #region 几个事件

        public static string[] default_groups = new string[] {
            "<default>",
            "<dp2circulation>",
            "<dp2catalog>",
            "<dp2opac>",
        };

        public override Task OnConnected()
        {
#if NO
            UserItem useritem = (UserItem)Context.Request.Environment[USERITEM_KEY];

            string userName = Context.Headers["username"];
            string password = Context.Headers["password"];
            string parameters = Context.Headers["parameters"];

            Hashtable table = StringUtil.ParseParameters(parameters);
            LoginRequest param = new LoginRequest();
            param.UserName = userName;
            param.Password = password;
            param.LibraryName = (string)table["libraryName"];
            param.LibraryUID = (string)table["libraryUID"];
            param.LibraryUserName = (string)table["libraryUserName"];
            param.PropertyList = (string)table["propertyList"];

            ServerInfo.ConnectionTable.AddConnection(Context.ConnectionId);

            MessageResult result = Login(param);
            if (result.Value == -1)
            {
                ServerInfo.ConnectionTable.RemoveConnection(Context.ConnectionId);
                // return Task.Run(() => { });
                throw new Exception(result.ErrorInfo);
            }
#endif
            AddConnection();

            //Program.WriteToConsole("Client connected: " + Context.ConnectionId);
            return base.OnConnected();
        }

        void AddConnection()
        {
            ConnectionInfo connection_info = ServerInfo.ConnectionTable.AddConnection(Context.ConnectionId);

            if (Context.Request.Environment.ContainsKey(USERITEM_KEY))
            {
                UserItem useritem = (UserItem)Context.Request.Environment[USERITEM_KEY];
                connection_info.UserItem = useritem;
            }

            string strParameters = Context.Request.Headers["parameters"];

            Hashtable table = StringUtil.ParseParameters(strParameters, ',', '=', "url");
            connection_info.PropertyList = (string)table["propertyList"];
            connection_info.LibraryUID = (string)table["libraryUID"];
            connection_info.LibraryName = (string)table["libraryName"];
            connection_info.LibraryUserName = (string)table["libraryUserName"];

            AddToSignalRGroup(connection_info, true);
        }

        void AddToSignalRGroup(ConnectionInfo connection_info, bool add = true)
        {
            // 默认的几个群组
            List<string> defaults = new List<string>();
            defaults.AddRange(default_groups);

            // 加入 SignalR 的 group
            if (connection_info.Groups != null)
            {
                foreach (string s in connection_info.Groups)
                {
                    GroupDefinition def = GroupDefinition.Build(s);

                    // 如果定义了不希望获得通知
                    if (string.IsNullOrEmpty(def.Definition) == false
                        && StringUtil.ContainsRight(def.Definition, "n") == false)
                        goto CONTINUE;

                    if (add)
                        Groups.Add(connection_info.ConnectionID, def.GroupName);
                    else
                        Groups.Remove(connection_info.ConnectionID, def.GroupName);

                CONTINUE:
                    defaults.Remove(def.GroupName);
                }
            }

            foreach (string group in defaults)
            {
                if (add)
                    Groups.Add(connection_info.ConnectionID, group);
                else
                    Groups.Remove(connection_info.ConnectionID, group);
            }
        }

        public override Task OnReconnected()
        {
#if NO
            UserItem useritem = (UserItem)Context.Request.Environment[USERITEM_KEY];

            string userName = Context.Headers["username"];
            string password = Context.Headers["password"];
            string parameters = Context.Headers["parameters"];

            Hashtable table = StringUtil.ParseParameters(parameters);
            LoginRequest param = new LoginRequest();
            param.UserName = userName;
            param.Password = password;
            param.LibraryName = (string)table["libraryName"];
            param.LibraryUID = (string)table["libraryUID"];
            param.LibraryUserName = (string)table["libraryUserName"];
            param.PropertyList = (string)table["propertyList"];

            ServerInfo.ConnectionTable.AddConnection(Context.ConnectionId);

            MessageResult result = Login(param);
            if (result.Value == -1)
            {
                ServerInfo.ConnectionTable.RemoveConnection(Context.ConnectionId);
                throw new Exception(result.ErrorInfo);
            }
#endif
            AddConnection();

            //Program.WriteToConsole("Client Re-connected: " + Context.ConnectionId);

            return base.OnReconnected();
        }

        //
        // 摘要: 
        //     Called when a connection disconnects from this hub gracefully or due to a
        //     timeout.
        //
        // 参数: 
        //   stopCalled:
        //     true, if stop was called on the client closing the connection gracefully;
        //     false, if the connection has been lost for longer than the Microsoft.AspNet.SignalR.Configuration.IConfigurationManager.DisconnectTimeout.
        //      Timeouts can be caused by clients reconnecting to another SignalR server
        //     in scaleout.
        //
        // 返回结果: 
        //     A System.Threading.Tasks.Task
        public override Task OnDisconnected(bool stopCalled)
        {
            ConnectionInfo connection_info = ServerInfo.ConnectionTable.RemoveConnection(Context.ConnectionId);

            AddToSignalRGroup(connection_info, false);

            //Program.WriteToConsole("Client disconnected: " + Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        #endregion

    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class MyAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool UserAuthorized(System.Security.Principal.IPrincipal user)
        {
            return true;

#if NO
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var principal = user as ClaimsPrincipal;

            if (principal != null)
            {
                Claim authenticated = principal.FindFirst(ClaimTypes.Authentication);
                if (authenticated != null && authenticated.Value == "true")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
#endif
        }

        public override bool AuthorizeHubConnection(Microsoft.AspNet.SignalR.Hubs.HubDescriptor hubDescriptor, IRequest request)
        {
            // return base.AuthorizeHubConnection(hubDescriptor, request);

            return AuthenticateUser(request);
        }

        private static bool AuthenticateUser(
            IRequest request
            // string credentials
            )
        {
            if (request.Environment.ContainsKey(MyHub.USERITEM_KEY) == true)
                return true;

            string userName = request.Headers["username"];

            if (string.IsNullOrEmpty(userName) == true)
                return true;    // 也算授权成功，但 request.Environment 里面没有用户对象

            string password = request.Headers["password"];

            // 获得用户信息
            var results = ServerInfo.UserDatabase.GetUsersByName(userName, 0, 1).Result;
            if (results.Count != 1)
                return false;
            var user = results[0];
            string strHashed = Cryptography.GetSHA1(password);

            if (user.password != strHashed)
                return false;
            request.Environment[MyHub.USERITEM_KEY] = user;
            return true;
        }

#if NO
        private static bool CheckAuthorization()
        {
            var cache = AppHostBase.Resolve<ICacheClient>();
            var sess = cache.SessionAs<AuthUserSession>();
            return sess.IsAuthenticated;
        }
#endif

        public override bool AuthorizeHubMethodInvocation(Microsoft.AspNet.SignalR.Hubs.IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            return base.AuthorizeHubMethodInvocation(hubIncomingInvokerContext, appliesToMethod);
        }
    }

}
