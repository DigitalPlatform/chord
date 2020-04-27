﻿// #define LOG
// #define LOG_REQUEST_SEARCH

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using System.Threading;

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

        public string echo(string text)
        {
            if (text == "!verify")
            {
                // 验证 ConnectionId 是否在 ConnectionTable 中存在，如果不存在则自动触发前端重连接
                MessageResult result = new MessageResult();
                ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
    result,
    "echo()",
    true);
                if (result.String == "_connectionNotFound")
                    return result.String;
            }

            return text;
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

        void CallClientClose(string connection_id,
            string strAction,
            TimeSpan delay)
        {
            try
            {
                Thread.Sleep(delay);
                CloseRequest request = new CloseRequest(strAction);
                Clients.Client(connection_id).close(request);
            }
            catch (Exception ex)
            {
                Console.Write("切断前端通讯通道时出现异常: " + ExceptionUtil.GetExceptionText(ex));
                ServerInfo.WriteErrorLog("切断前端通讯通道时出现异常: " + ExceptionUtil.GetExceptionText(ex));   // 2016/10/21
            }
        }

        // 获得通道信息，并顺便检查状态
        ConnectionInfo GetConnection(string id,
            MessageResult result,
            string strFunctionName,
            bool checkLogin = true,
            bool reconnect = true)
        {
            ConnectionInfo info = ServerInfo.ConnectionTable.GetConnection(id);   // Context.ConnectionId
            if (info == null)
            {
                result.Value = -1;
                result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。" + strFunctionName + " 操作失败";
                result.String = "_connectionNotFound";
                // 延迟两秒以后让前端重连这个通道
                if (reconnect)
                    Task.Run(() => CallClientClose(id, "reconnect", TimeSpan.FromSeconds(2)));
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

        // 获得 GroupDatabase 中的群记录
        public MessageResult RequestGetGroup(GetGroupRequest param)
        {
            // param.Count 为 0 和 -1 意义不同。前者可用于只探索条数，不获取数据。比如在界面上显示有多少条新的信息

            MessageResult result = new MessageResult();

            try
            {

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
                // Task.Run(() => SearchGroupAndResponse(param));
                SearchGroupAndResponse(param);  // TODO: 是否等待?

                result.Value = 1;   // 成功
            }
            catch (Exception ex)
            {
                result.SetError("RequestGetGroup() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }

            return result;
        }

        async Task SearchGroupAndResponse(
            GetGroupRequest param)
        {
            SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(param.TaskID, false);
            if (search_info == null)
                return;

            try
            {
                int batch_size = 10;    // 100? 或者根据 data 尺寸动态计算每批的个数
                int send_count = 0;

                List<GroupRecord> records = new List<GroupRecord>();

                await ServerInfo.GroupDatabase.GetGroups(param.GroupCondition,
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
                    }).ConfigureAwait(false); // .Wait();
            }
            finally
            {
                // 主动清除已经完成的任务对象
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

        // 正规化 MessageItem 里面的 subjects 成员内容
        // 如果 subjects 仅有一个元素而且为 ""，则规范为 subjects 为空
        MessageItem CanonicalizeMessageItemSubjects(MessageItem item)
        {
#if ARRAY
            if (item.subjects != null && item.subjects.Length == 1
                && string.IsNullOrEmpty(item.subjects[0]))
            {
                item.subjects = null;
            }
#else
            if (item.subjects != null && item.subjects.Count == 1
                && string.IsNullOrEmpty(item.subjects[0]))
            {
                item.subjects = null;
            }
#endif

            return item;
        }

        // 拼接多次发送的 MessageRecord
        // return:
        //      true    完成
        //      false   尚未完成
        bool ConcatRecords(SetMessageRequest param)
        {
            // 拼接记录
            if (string.IsNullOrEmpty(param.TaskID))
                throw new ArgumentException("param.TaskID 不应为空", "param");

            SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(param.TaskID);
            if (search_info == null)
            {
                search_info = ServerInfo.SearchTable.AddSearch(Context.ConnectionId,
                    param.TaskID);
            }

            foreach (MessageRecord record in param.Records)
            {
                if (record.data == null)
                    goto FINISH;

                if (search_info.Data == null)
                    search_info.Data = record;
                else
                {
                    // 第二次以及以后。拼接 .data 字符串
                    MessageRecord old = search_info.Data as MessageRecord;
                    old.data += record.data;  // 请求中，第一个后面的每个 MessageRecord 只需要承载 data 即可，其他成员可以省略
                }
            }

            return false;   // 表示拼接尚未结束。需要继续等待后面的请求来进行拼接
        FINISH:
            param.Records = new List<MessageRecord>();
            param.Records.Add(search_info.Data as MessageRecord);
#if NO
            // 这一段是测试验证一下合并起来的内容是否正确。但只能适合判断 XML 内容，普通字符串不适合用这一段判断
            {
                MessageRecord temp = param.Records[0];
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(temp.data);

            }
#endif
            ServerInfo.SearchTable.RemoveSearch(param.TaskID);
            return true;
        }

#if ARRAY
        // 正规化 MessageItem 里面的 groups 成员内容
        void CanonicalizeMessageItemGroups(MessageItem item, ConnectionInfo connection_info)
        {
            // 正规化组名
            item.groups = GroupSegment.Canonicalize(item.groups, (name) =>
            {
                if (name.Type == "un" && name.Text == connection_info.UserName)
                    return new GroupName("ui", connection_info.UserID);
                // 需要进行检索
                if (name.Type == "un")
                {
                    List<UserItem> users = ServerInfo.UserDatabase.GetUsersByName(name.Text, 0, 1).Result;
                    if (users == null || users.Count == 0)
                        throw new Exception("未知的用户名 '" + name.Text + "'");
                    return new GroupName("ui", users[0].id);
                }
                if (name.Type == "gn")
                {
                    List<GroupItem> groups = ServerInfo.GroupDatabase.GetGroupsByName(name.Text, 0, 1).Result;
                    if (groups == null || groups.Count == 0)
                        throw new Exception("未知的组名 '" + name.Text + "'");
                    return new GroupName("gi", groups[0].id);
                }
                return name;
            });
        }
#else
        // 正规化 MessageItem 里面的 groups 成员内容
        void CanonicalizeMessageItemGroups(MessageItem item, ConnectionInfo connection_info)
        {
            // 正规化组名
            item.groups = GroupSegment.Canonicalize(item.groups, (name) =>
            {
                if (name.Type == "un" && name.Text == connection_info.UserName)
                    return new GroupName("ui", connection_info.UserID);
                // 需要进行检索
                if (name.Type == "un")
                {
                    List<UserItem> users = ServerInfo.UserDatabase.GetUsersByName(name.Text, 0, 1).Result;
                    if (users == null || users.Count == 0)
                        throw new Exception("未知的用户名 '" + name.Text + "'");
                    return new GroupName("ui", users[0].id);
                }
                if (name.Type == "gn")
                {
                    List<GroupItem> groups = ServerInfo.GroupDatabase.GetGroupsByName(name.Text, 0, 1).Result;
                    if (groups == null || groups.Count == 0)
                        throw new Exception("未知的组名 '" + name.Text + "'");
                    return new GroupName("gi", groups[0].id);
                }
                return name;
            });
        }
#endif
        // 返回 MessageRecord 数组，因为有些字段是服务器给设定的，例如 PublishTime, ID
        // 但如果让前端知道哪条是哪条呢？方法是返回的数组和请求的数组大小顺序一样
        // 前端得到这些内容的好处是，可以自己在本地刷新显示，不必等服务器发过来通知
        // 在中途出错的情况下，数组中的元素可能少于请求的数目，但顺序是和请求是一致的
        public async Task<SetMessageResult> SetMessage(
#if NO
            string action,
            List<MessageRecord> messages
#endif
SetMessageRequest param
            )
        {
            SetMessageResult result = new SetMessageResult();
            List<MessageItem> saved_items = new List<MessageItem>();

            try
            {
                List<MessageItem> items = new List<MessageItem>();

                // 拼接记录
                if (string.IsNullOrEmpty(param.TaskID) == false)
                {
                    // 拼接多次发送的 MessageRecord
                    // return:
                    //      true    完成
                    //      false   尚未完成
                    if (ConcatRecords(param) == false)
                    {
                        result.Value = 0;
                        result.String = "_continue";
                        return result;
                    }

                }

                // 转换类型
                foreach (MessageRecord record in param.Records)
                {

                    if (string.IsNullOrEmpty(string.Join(",", record.groups)))
                    {
                        result.String = "InvalidData";
                        result.Value = -1;
                        result.ErrorInfo = "messages 中包含不合法的 MessageRecord 记录，groups 成员不允许为空(可以使用 '<default>' 作为默认群组的名字)";
                        return result;
                    }
                    items.Add(CanonicalizeMessageItemSubjects(BuildMessageItem(record)));
                }


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
                        // 正规化组名
                        CanonicalizeMessageItemGroups(item, connection_info);

                        if (bSupervisor == false)
                        {
                            if (connection_info.UserItem == null)
                            {
                                if (GroupDefinition.IsDefaultGroupName(item.groups) == false)
                                {
                                    result.String = "Denied";
                                    result.Value = -1;
                                    result.ErrorInfo = "当前用户(未注册用户)只能在 <default> 组创建消息";
                                    return result;
                                }
                            }
                            else
                            {
#if ARRAY
                                if (item.groups.Length == 1 &&  // 多个 id 联合的情况暂时不检查
                                    GroupDefinition.IncludeGroup(connection_info.Groups, item.groups) == false)
#else
                                if (item.groups.Count == 1 &&  // 多个 id 联合的情况暂时不检查
                                    GroupDefinition.IncludeGroup(connection_info.Groups, item.groups?.ToArray()) == false)

#endif
                                {
                                    result.String = "Denied";
                                    result.Value = -1;
                                    result.ErrorInfo = "当前用户 '" + connection_info.UserName + "' 没有加入群组 '" + string.Join(",", item.groups) + "'，因此无法在其中创建消息";
                                    return result;
                                }
                            }
                        }

                        item.publishTime = DateTime.Now;
                        // item.expireTime = new DateTime(0);  // 表示永远不失效
                        item.creator = BuildMessageUserID(connection_info);
                        item.userName = connection_info.UserName;
                        item.SetID(Guid.NewGuid().ToString());  // 确保 id 字段有值。是否可以允许前端指定这个 ID 呢？如果要进行查重就麻烦了
                        await ServerInfo.MessageDatabase.Add(item).ConfigureAwait(false); // .Wait();
                        saved_items.Add(item);

#if NO
                        {
                            Console.WriteLine("*** 开始验证刚写入的消息");
                            Console.WriteLine("请求的消息:\r\n" + item.Dump());

                            // 检索出原有的消息，以便核对条件
                            List<MessageItem> results = ServerInfo.MessageDatabase.GetMessageByID(item.id).Result;
                            if (results.Count == 0)
                            {
                                result.String = "VerifyFailed";
                                result.Value = -1;
                                result.ErrorInfo = "刚刚创建的 id 为 '" + item.id + "' 的消息在消息库中没有找到";
                                return result;
                            }
                            if (results.Count > 1)
                            {
                                result.String = "VerifyFailed";
                                result.Value = -1;
                                result.ErrorInfo = "刚刚创建的 id 为 '" + item.id + "' 的消息在消息库中居然发现有 "+results.Count+" 条";
                                return result;
                            }
                            if (results.Count == 1)
                            {
                                MessageItem new_item = results[0];
                                string error = MessageItem.IsEqual(new_item, item);
                                if (string.IsNullOrEmpty(error) == false)
                                {
                                    Console.WriteLine("从消息库中取出的消息:\r\n" + new_item.Dump());

                                    result.String = "VerifyFailed";
                                    result.Value = -1;
                                    result.ErrorInfo = "刚刚创建的 id 为 '" + item.id + "' 的消息从消息库中取出以后，发现和请求的记录不一致:" + error;
                                    return result;
                                }
                            }
                            Console.WriteLine("上述消息成功写入数据库");
                            Console.WriteLine("*** 结束验证刚写入的消息");
                        }
#endif
                    }
                }
                else if (param.Action == "change")
                {
                    // 注: 超级用户可以修改任何消息
                    // 其他人只能修改自己原先创建的消息

                    foreach (MessageItem item in items)
                    {
                        // 正规化组名
                        // CanonicalizeMessageItemGroups(item, connection_info);

                        // 注: item 里面只有 id 可信，用于判断权限的其他字段应该从数据库中获得

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

                        if (bSupervisor == false)
                        {
#if ARRAY
                            string strParameters = GroupDefinition.FindGroupDefinition(connection_info.Groups, exist.groups);
#else
                            string strParameters = GroupDefinition.FindGroupDefinition(connection_info.Groups, exist.groups?.ToArray());
#endif
                            if (string.IsNullOrEmpty(strParameters) == false
    && StringUtil.ContainsRight(strParameters, "ca") == 1)
                            {
                                // 特殊许可了修改全部消息
                            }
                            else
                            {
                                if (connection_info.UserItem == null)
                                {
                                    if (GroupDefinition.IsDefaultGroupName(exist.groups) == false)
                                    {
                                        result.String = "Denied";
                                        result.Value = -1;
                                        result.ErrorInfo = "当前用户(未注册用户)只能修改 <default> 组内自己发布的消息";
                                        return result;
                                    }
                                }
                                else
                                {
#if ARRAY
                                    if (exist.groups.Length == 1 &&  // 多个 id 联合的情况暂时不检查
                                        GroupDefinition.IncludeGroup(connection_info.Groups, exist.groups) == false)
#else
                                    if (exist.groups.Count == 1 &&  // 多个 id 联合的情况暂时不检查
    GroupDefinition.IncludeGroup(connection_info.Groups, exist.groups?.ToArray()) == false)
#endif
                                    {
                                        result.String = "Denied";
                                        result.Value = -1;
                                        result.ErrorInfo = "当前用户 '" + connection_info.UserName + "' 没有加入群组 '" + string.Join(",", exist.groups) + "'，因此无法修改其中的消息";
                                        return result;
                                    }
                                }
                            }

                            string creator_string = BuildMessageUserID(connection_info);

                            if (exist.creator != creator_string)
                            {
                                result.String = "Denied";
                                result.Value = -1;
                                result.ErrorInfo = "因 id 为 '" + item.id + "' 的消息记录原先不是当前用户创建的，修改操作被拒绝";
                                return result;
                            }
                        }

                        // groups 字段不允许修改
                        item.groups = exist.groups;
                        // creator 字段不允许修改
                        item.creator = exist.creator;

                        // publishTime 字段不允许修改 2016/8/12
                        item.publishTime = exist.publishTime;

                        // exipreTime 字段不允许修改
                        item.expireTime = exist.expireTime;
                        await ServerInfo.MessageDatabase.Update(item).ConfigureAwait(false);  // .Wait();
                        saved_items.Add(item);  // TODO: 应该返回修改后的记录内容
                    }
                }
                else if (param.Action == "delete" || param.Action == "expire")
                {
                    // TODO: 可否在遇到不能操作的 item 的时候，先跳过这些 item 把能操作的全部操作了? 不过报错信息就会很长了

                    string strActionName = "删除";
                    if (param.Action == "expire")
                        strActionName = "失效";

                    // 注: 超级用户可以删除任何消息
                    // 其他人只能删除自己原先创建的消息

                    foreach (MessageItem item in items)
                    {
                        // 正规化组名
                        // CanonicalizeMessageItemGroups(item, connection_info);

                        // 注: item 里面只有 id 有用，其他字段内容都会被忽略

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

                        bool bDeleteAllRights = false;
                        if (bSupervisor == false)
                        {
#if ARRAY
                            string strParameters = GroupDefinition.FindGroupDefinition(connection_info.Groups, exist.groups);
#else
                            string strParameters = GroupDefinition.FindGroupDefinition(connection_info.Groups, exist.groups?.ToArray());
#endif
                            if (string.IsNullOrEmpty(strParameters) == false
    && StringUtil.ContainsRight(strParameters, "da") == 1)
                            {
                                // 特殊许可了删除全部消息
                                bDeleteAllRights = true;
                            }
                            else
                            {
                                if (connection_info.UserItem == null)
                                {
                                    if (GroupDefinition.IsDefaultGroupName(exist.groups) == false)
                                    {
                                        result.String = "Denied";
                                        result.Value = -1;
                                        result.ErrorInfo = "当前用户(未注册用户)只能" + strActionName + " <default> 组内自己创建的消息";
                                        return result;
                                    }
                                }
                                else
                                {
#if ARRAY
                                    if (exist.groups.Length == 1 &&  // 多个 id 联合的情况暂时不检查
                                        GroupDefinition.IncludeGroup(connection_info.Groups, exist.groups) == false)
#else
                                    if (exist.groups.Count == 1 &&  // 多个 id 联合的情况暂时不检查
    GroupDefinition.IncludeGroup(connection_info.Groups, exist.groups?.ToArray()) == false)
#endif
                                    {
                                        result.String = "Denied";
                                        result.Value = -1;
                                        result.ErrorInfo = "当前用户 '" + connection_info.UserName + "' 没有加入群组 '" + string.Join(",", exist.groups) + "'，因此无法" + strActionName + "其中的消息";
                                        return result;
                                    }
                                }
                            }

                            string creator_string = BuildMessageUserID(connection_info);

                            if (bDeleteAllRights == false
                                && exist.creator != creator_string)
                            {
                                result.String = "Denied";
                                result.Value = -1;
                                result.ErrorInfo = "因 id 为 '" + item.id + "' 的消息不是当前用户创建的，" + strActionName + "操作被拒绝";
                                return result;
                            }
                        }

                        if (param.Action == "delete")
                            await ServerInfo.MessageDatabase.DeleteByID(item.id).ConfigureAwait(false);   // .Wait();
                        else
                        {
                            DateTime now = DateTime.Now;
                            await ServerInfo.MessageDatabase.ExpireByID(item.id, now).ConfigureAwait(false);  // .Wait();
                            item.expireTime = now;
                        }
                        saved_items.Add(exist);
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
                ServerInfo.WriteErrorLog(result.ErrorInfo);
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
                if (StringUtil.IsInList("dontNotifyMe", param.Style) == true)
                {
                    excludeConnectionIds = new string[1];
                    excludeConnectionIds[0] = Context.ConnectionId;
                }
                // 即便遇到抛出异常的情况，先前保存成功的部分消息也可以发送出去
                // 启动一个任务，向相关的前端推送消息，以便它们在界面上显示消息或者变化(比如删除或者修改以后的效果)
                /*
                await Task.Run(() => PushMessageToClient(param.Action,
                    excludeConnectionIds,
                    saved_items));
                    */

                await PushMessageToClient(param.Action,
                excludeConnectionIds,
                saved_items);
            }

            return result;
        }

        // 构造用于消息内 create 字段的 user id
        static string BuildMessageUserID(ConnectionInfo info)
        {
            if (string.IsNullOrEmpty(info.UserID) == false)
                return info.UserID;
            return "~" + info.LibraryUserName + "@" + info.LibraryName + "|" + info.LibraryUID;
        }

        async Task PushMessageToClient(string action,
            string[] excludeConnectionIds,
            List<MessageItem> messages)
        {
            try
            {
                // 按照 Group 名字，分为若干个 List
                Hashtable group_table = new Hashtable();  // groupName --> List<MessageItem>

                foreach (MessageItem item in messages)
                {
                    Debug.Assert(string.IsNullOrEmpty(item.id) == false, "");

                    string groupName = string.Join(",", item.groups).ToLower(); // 注意标准化以后的群名都是小写的
                    if (string.IsNullOrEmpty(groupName) == true)
                    {
                        throw new ArgumentException("MessageItem 对象的 groups 成员不应为空");
                    }
#if NO
                if (string.IsNullOrEmpty(groupName) == true)
                    groupName = "<default>";    // 需要在各个环节正规化组的名字
#endif
                    // 二人或者三人小组，没有必要采用 SignalR 的 group 机制，而用 connection id 一个一个发送
                    if (item.groups.
#if ARRAY
                        Length
#else
                        Count
#endif
                        > 1)
                    {
#if ARRAY
                        List<string> ids = ServerInfo.ConnectionTable.GetConnectionIds(item.groups);
#else
                        List<string> ids = ServerInfo.ConnectionTable.GetConnectionIds(item.groups?.ToArray());
#endif
                        if (ids.Count > 0)
                        {
                            List<MessageRecord> records = BuildMessageRecords(item);
                            // Clients.Clients(ids).addMessage(action, batch);

                            await ProcessMessageRecords(
    records,
    async (batch) =>
    {
        await Clients.Clients(ids).addMessage(action, batch);
        return true;
    },
    4096);
                        }
                    }
                    else
                    {
                        List<MessageRecord> records = (List<MessageRecord>)group_table[groupName];
                        if (records == null)
                        {
                            records = new List<MessageRecord>();
                            group_table[groupName] = records;
                        }
                        records.AddRange(BuildMessageRecords(item));
                    }
                }

                // int batch_size = 10;

                foreach (string groupName in group_table.Keys)
                {
                    List<MessageRecord> records = (List<MessageRecord>)group_table[groupName];
                    Debug.Assert(records != null, "");
                    Debug.Assert(records.Count != 0, "");
                    Console.WriteLine("Push to group '" + groupName + "'");

                    await ProcessMessageRecords(
                        records,
                        async (batch) =>
                        {
                            if (excludeConnectionIds == null)
                                await Clients.Group(groupName).addMessage(action, batch);
                            else
                                await Clients.Group(groupName, excludeConnectionIds).addMessage(action, batch);
                            return true;
                        },
                        4096);
#if NO
                    // 如果 records 包含数量太多，需要分批发送
                    List<MessageRecord> batch = new List<MessageRecord>();
                    int i = 0;
                    foreach (MessageRecord record in records)
                    {
                        batch.Add(record);
                        if (batch.Count >= batch_size
                            || (i == records.Count - 1 && batch.Count > 0))
                        {
                            if (excludeConnectionIds == null)
                                Clients.Group(groupName).addMessage(action, batch);
                            else
                                Clients.Group(groupName, excludeConnectionIds).addMessage(action, batch);
                            batch.Clear();
                        }
                        i++;
                    }
#endif
                }
            }
            catch (Exception ex)
            {
                ServerInfo.WriteErrorLog("PushMessageToClient() 出现异常: " + ExceptionUtil.GetExceptionText(ex));
            }
        }

        // return:
        //      true    继续
        //      false   中断
        public delegate Task<bool> Func_ProcessRecords(List<MessageRecord> records);

        async Task ProcessMessageRecords(
            List<MessageRecord> records,
            Func_ProcessRecords proc,
            int chunk_size)
        {
            Debug.Assert(records != null, "");
            Debug.Assert(records.Count != 0, "");

            while (records.Count > 0)
            {
                List<MessageRecord> temp = CopyPrevElements(records, chunk_size);
                var result = await proc(temp);
                if (result  == false)
                    return;
                records.RemoveRange(0, temp.Count);
            }
        }

        static MessageItem BuildMessageItem(MessageRecord record)
        {
            MessageItem item = new MessageItem();
            item.SetID(record.id);
#if ARRAY
            item.groups = record.groups;
#else
            item.groups = NewListString(record.groups);
#endif
            item.creator = record.creator;
            item.userName = record.userName;
            item.data = record.data;
            item.format = record.format;
            item.type = record.type;
            item.thread = record.thread;
#if ARRAY
            item.subjects = record.subjects;
#else
            item.subjects = NewListString(record.subjects);
#endif
            item.publishTime = record.publishTime;
            item.expireTime = record.expireTime;
            return item;
        }

        static List<string> NewListString(string[] contents)
        {
            if (contents == null)
                return new List<string>();
            return new List<string>(contents);
        }

        static MessageRecord BuildMessageRecord(MessageItem item)
        {
            MessageRecord record = new MessageRecord();
            record.id = item.id;
#if ARRAY
            record.groups = item.groups;
#else
            record.groups = item.groups?.ToArray();
#endif
            record.creator = item.creator;
            record.userName = item.userName;
            record.data = item.data;
            record.format = item.format;
            record.type = item.type;
            record.thread = item.thread;
#if ARRAY
            record.subjects = item.subjects;
#else
            record.subjects = item.subjects?.ToArray();
#endif
            record.publishTime = item.publishTime;
            record.expireTime = item.expireTime;
            return record;
        }

        // 根据一个 MessageItem 构造出一个或多个 MessageRecord
        static List<MessageRecord> BuildMessageRecords(MessageItem item)
        {
            int chunk_size = 4096;
            List<MessageRecord> results = new List<MessageRecord>();
            string data = item.data;
            int send = 0;
            for (; ; )
            {
                string current_data = data.Substring(send, Math.Min(chunk_size, data.Length - send));

                MessageRecord record = new MessageRecord();
                record.data = current_data;
                if (send + current_data.Length >= data.Length)
                {
                    // 表示最后一个 chunk
                    record.id = item.id;
#if ARRAY
                    record.groups = item.groups;
#else
                    record.groups = item.groups?.ToArray();
#endif
                    record.creator = item.creator;
                    record.userName = item.userName;
                    record.format = item.format;
                    record.type = item.type;
                    record.thread = item.thread;
#if ARRAY
                    record.subjects = item.subjects;
#else
                    record.subjects = item.subjects?.ToArray();
#endif
                    record.publishTime = item.publishTime;
                    record.expireTime = item.expireTime;
                }
                else
                {
                    record.id = ""; // 表示非最后一个 chunk

                    // 2020/4/27
                    // 中间的 chunk message 也要有 groups，否则前端收到这样的消息的时候无法判断它是属于什么群的
                    record.groups = item.groups?.ToArray();
                }

                results.Add(record);

                send += current_data.Length;
                if (send >= data.Length)
                    break;
            }

            return results;
        }

        static long GetDataLength(List<MessageRecord> records)
        {
            long length = 0;
            foreach (MessageRecord record in records)
            {
                if (record != null && record.data != null)
                    length += record.data.Length;
            }

            return length;
        }

        // 复制 chunk_size 以内的元素
        // 至少会返回一个元素
        static List<MessageRecord> CopyPrevElements(List<MessageRecord> records,
            int chunk_size)
        {
            List<MessageRecord> results = new List<MessageRecord>();

            int length = 0;
            foreach (MessageRecord record in records)
            {
                if (record != null && record.data != null)
                    length += record.data.Length;

                if (length == chunk_size)
                {
                    results.Add(record);
                    return results;
                }

                if (length > chunk_size)
                    return results;

                results.Add(record);
            }
            return results;
        }

        #endregion

        #region GetMessage() API

        // 把用户名列表字符串转换为用户ID列表字符串
        string CanonicalizeUserName(string userNameList)
        {
            if (string.IsNullOrEmpty(userNameList))
                return userNameList;

            GroupSegment segment = new GroupSegment(userNameList, "un");
            segment.Canonicalize((name) =>
            {
                if (name.Type == "un")
                {
                    List<UserItem> results = ServerInfo.UserDatabase.GetUsersByName(name.Text, 0, 1).Result;
                    if (results == null || results.Count == 0)
                        throw new ArgumentException("未知的用户名 '" + name.Text + "'");
                    return new GroupName("ui", results[0].id);
                }
                if (name.Type == "gn")
                {
                    throw new ArgumentException("不允许使用 gn 前缀");
                }
                return name;
            });

            return segment.ToStringUnQuote();
        }

#if NO
        public class NotFoundException : Exception
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="error"></param>
            /// <param name="strText"></param>
            public NotFoundException(string strText)
                : base(strText)
            {
            }
        }
#endif

        bool DisplaylizeGroupQuery(GroupQuery query, ConnectionInfo connection_info)
        {
            // 正规化组名
            return query.Displaylize((name) =>
            {
                if (name.Type == "ui" && name.Text == connection_info.UserID)
                    return new GroupName("un", connection_info.UserName);
                // 需要进行检索
                if (name.Type == "ui")
                {
                    List<UserItem> users = ServerInfo.UserDatabase.GetUsersByID(name.Text, 0, 1).Result;
                    if (users == null || users.Count == 0)
                    {
                        // throw new Exception("未知的用户 ID '" + name.Text + "'");
                        return new GroupName("ui", "!未知的用户 ID '" + name.Text + "'");
                    }
                    return new GroupName("un", users[0].userName);
                }
                if (name.Type == "gi" && (name.Text[0] != '<' && name.Text[0] != '_'))
                {
                    List<GroupItem> groups = ServerInfo.GroupDatabase.GetGroupsByID(name.Text, 0, 1).Result;
                    if (groups == null || groups.Count == 0)
                    {
                        // throw new Exception("未知的群 ID '" + name.Text + "'");
                        return new GroupName("gi", "!未知的群 ID '" + name.Text + "'");
                    }
                    return new GroupName("gn", groups[0].name);
                }
                return name;
            });
        }

        bool CanonicalizeGroupQuery(GroupQuery query, ConnectionInfo connection_info)
        {
            // 正规化组名
            return query.Canonicalize((name) =>
            {
                if (name.Type == "un" && name.Text == connection_info.UserName)
                    return new GroupName("ui", connection_info.UserID);
                // 需要进行检索
                if (name.Type == "un")
                {
                    List<UserItem> users = ServerInfo.UserDatabase.GetUsersByName(name.Text, 0, 1).Result;
                    if (users == null || users.Count == 0)
                        throw new Exception("未知的用户名 '" + name.Text + "'");
                    return new GroupName("ui", users[0].id);
                }
                if (name.Type == "gn")
                {
                    List<GroupItem> groups = ServerInfo.GroupDatabase.GetGroupsByName(name.Text, 0, 1).Result;
                    if (groups == null || groups.Count == 0)
                        throw new Exception("未知的群名 '" + name.Text + "'");
                    return new GroupName("gi", groups[0].id);
                }
                return name;
            });
        }

        public MessageResult RequestGetMessage(GetMessageRequest param)
        {
            // param.Count 为 0 和 -1 意义不同。前者可用于只探索条数，不获取数据。比如在界面上显示有多少条新的信息

            MessageResult result = new MessageResult();

            try
            {
                // 检查参数
                if (string.IsNullOrEmpty(param.GroupCondition) == true)
                {
                    result.Value = -1;
                    result.String = "InvalidParam";
                    result.ErrorInfo = "param 成员 GroupCondition 不应为空";
                    return result;
                }

                try
                {
                    param.UserCondition = CanonicalizeUserName(param.UserCondition);
                }
                catch (ArgumentException ex)
                {
                    result.Value = -1;
                    result.String = "ConditionError";
                    result.ErrorInfo = ex.Message;
                    return result;
                }

                ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
    result,
    "RequestGetMessage()",
    false);
                if (connection_info == null)
                    return result;


                // TODO: 检查 GroupCondition，禁止当前用户获取不该获取的消息
                GroupQuery group_query = new GroupQuery(param.GroupCondition);
                if (param.Action == "transGroupNameQuick")
                {
                    // 将组名翻译为可读的形态
                    DisplaylizeGroupQuery(group_query, connection_info);
                    // 立即返回
                    result.String = group_query.ToStringUnQuote();
                    result.Value = 2;
                    return result;
                }
                if (param.Action == "transGroupName")
                {
                    DisplaylizeGroupQuery(group_query, connection_info);
                }
                else
                {
                    CanonicalizeGroupQuery(group_query, connection_info);

                    bool bSupervisor = (StringUtil.Contains(connection_info.Rights, "supervisor"));
#if NO
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
#endif

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

                if (param.Action == "transGroupName")
                    Task.Run(() => ResponseGroupName(param, group_query));
                else if (param.Action == "enumGroupName")
                {
                    // Task.Run(() => EnumFieldAndResponse("groups", param, group_query));
                    EnumFieldAndResponse("groups", param, group_query);
                }
                else if (param.Action == "enumSubject")
                {
                    // Task.Run(() => EnumFieldAndResponse("subjects", param, group_query));
                    EnumFieldAndResponse("subjects", param, group_query);
                }
                else if (param.Action == "enumCreator")
                {
                    // Task.Run(() => EnumFieldAndResponse("creator", param, group_query));
                    EnumFieldAndResponse("creator", param, group_query);
                }
                else
                {
                    // 启动一个独立的 Task，该 Task 负责搜集和发送结果信息
                    // 这是典型的 dp2MServer 能完成任务的情况，不需要再和另外一个前端通讯
                    // 不过，请求本 API 的前端，要做好在 Request 返回以前就先得到数据响应的准备
                    // Task.Run(() => SearchMessageAndResponse(param, group_query));
                    SearchMessageAndResponse(param, group_query);
                }

                result.Value = 1;   // 成功
            }
            catch (Exception ex)
            {
                result.SetError("RequestGetMessage() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }

            return result;
        }

        void ResponseGroupName(
    GetMessageRequest param,
    GroupQuery group_query)
        {
            SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(param.TaskID, false);
            if (search_info == null)
                return;

            try
            {
                int batch_size = 50;    // 100? 或者根据 data 尺寸动态计算每批的个数
                int send_count = 0;

                int totalCount = group_query.Segments.Count;

                // 原来的 Query 用于对照
                GroupQuery origin_query = new GroupQuery(param.GroupCondition);

                List<MessageRecord> records = new List<MessageRecord>();
                int i = 0;
                foreach (GroupSegment segment in group_query.Segments)
                {
                    MessageRecord record = new MessageRecord();
                    record.id = origin_query.Segments[i].ToStringUnQuote();
                    record.data = segment.ToStringUnQuote();
                    record.groups = segment.ToStringArray();
                    records.Add(record);

                    // 集中发送一次
                    if (records.Count >= batch_size || (i >= totalCount - 1 && records.Count > 0))
                    {
                        ChangeBlankID(records);

                        Clients.Client(search_info.RequestConnectionID).responseGetMessage(
                            param.TaskID,
                            (long)totalCount, // resultCount,
                            (long)send_count,
                            records,
                            "", // errorInfo,
                            "" // errorCode
                            );
                        send_count += records.Count;

                        records.Clear();
                    }

                    i++;
                }

            }
            catch (Exception ex)
            {
                Clients.Client(search_info.RequestConnectionID).responseGetMessage(
    param.TaskID,
    -1, // resultCount,
    0,
    null,
    ExceptionUtil.GetExceptionText(ex), // errorInfo,
    "_sendExeption" // errorCode
    );
            }
            finally
            {
                // 主动清除已经完成的任务对象
                ServerInfo.SearchTable.RemoveSearch(param.TaskID);
            }
        }

        // 给 MessageRecord .id 成员为空的设置一个值，从而避免被前端当成片段
        static void ChangeBlankID(List<MessageRecord> records)
        {
            if (records == null)
                return;

            foreach (MessageRecord record in records)
            {
                if (string.IsNullOrEmpty(record.id))
                    record.id = "~";
            }
        }

        async Task EnumFieldAndResponse(
            string field,
            GetMessageRequest param,
            GroupQuery group_query)
        {
            SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(param.TaskID, false);
            if (search_info == null)
                return;

            try
            {
                int batch_size = 10;    // 100? 或者根据 data 尺寸动态计算每批的个数
                int send_count = 0;

                List<MessageRecord> records = new List<MessageRecord>();

                await ServerInfo.MessageDatabase.GetFieldAggregate(
                    field,
                    group_query,
                    param.UserCondition,
                    param.TimeCondition,
                    param.IdCondition,
                    param.SubjectCondition,
                    (int)param.Start,
                    (int)param.Count,
                    (totalCount, item) =>
                    {
                        if (item != null)
                            records.Add(BuildMessageRecord(item));
                        // 集中发送一次
                        if (records.Count >= batch_size || item == null)
                        {
                            ChangeBlankID(records);
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
                    }).ConfigureAwait(false); // .Wait();
            }
            catch (Exception ex)
            {
                Clients.Client(search_info.RequestConnectionID).responseGetMessage(
    param.TaskID,
    -1, // resultCount,
    0,
    null,
    ExceptionUtil.GetExceptionText(ex), // errorInfo,
    "_sendExeption" // errorCode
    );
            }
            finally
            {
                // 主动清除已经完成的任务对象
                ServerInfo.SearchTable.RemoveSearch(param.TaskID);
            }
        }

#if NO
        void EnumGroupNameAndResponse(
    GetMessageRequest param,
    GroupQuery group_query)
        {
            SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(param.TaskID, false);
            if (search_info == null)
                return;

            try
            {
                int batch_size = 10;    // 100? 或者根据 data 尺寸动态计算每批的个数
                int send_count = 0;

                List<MessageRecord> records = new List<MessageRecord>();

                ServerInfo.MessageDatabase.GetGroupsFieldAggragate(//param.GroupCondition,
                    group_query,
                    param.TimeCondition,
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
            catch (Exception ex)
            {
                Clients.Client(search_info.RequestConnectionID).responseGetMessage(
    param.TaskID,
    -1, // resultCount,
    0,
    null,
    ExceptionUtil.GetExceptionText(ex), // errorInfo,
    "_sendExeption" // errorCode
    );
            }
            finally
            {
                // 主动清除已经完成的任务对象
                ServerInfo.SearchTable.RemoveSearch(param.TaskID);
            }
        }
#endif
        // TODO: 发送消息过程部分建议用 ProcessMessageRecords() 改写
        async Task SearchMessageAndResponse(
            GetMessageRequest param,
            GroupQuery group_query)
        {
            SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(param.TaskID, false);
            if (search_info == null)
                return;

            try
            {
                // int batch_size = 10;    // 100? 或者根据 data 尺寸动态计算每批的个数。主要是 data 成员的长度差异很大
                int send_count = 0;
                int chunk_size = 4096;

                List<MessageRecord> records = new List<MessageRecord>();

                await ServerInfo.MessageDatabase.GetMessages(//param.GroupCondition,
                    group_query,
                    param.UserCondition,    // TODO: 需要转换为 ID
                    param.TimeCondition,
                    param.SortCondition,
                    param.IdCondition,
                    param.SubjectCondition,
                    (int)param.Start,
                    (int)param.Count,
                    (totalCount, item) =>
                    {
                        if (item != null)
                            records.AddRange(BuildMessageRecords(item));

                        string errorCode = "";

                        while (records.Count > 0)
                        {
                            long length = GetDataLength(records);
                            if (length < chunk_size && item != null)
                                break;

                            {
                                Console.WriteLine("send message totalCount=" + totalCount + " send_count=" + send_count + " records.Count=" + records.Count);

                                // 把残余的结果推送出去
                                if (records.Count > 0)
                                {
                                    List<MessageRecord> temp = null;

                                    if (length <= chunk_size)
                                        temp = records;
                                    else
                                        temp = CopyPrevElements(records, chunk_size);

                                    if (item == null && records.Count - temp.Count == 0)
                                        errorCode = "_complete";

                                    // 让前端获得检索结果
                                    try
                                    {
                                        Clients.Client(search_info.RequestConnectionID).responseGetMessage(
                                            param.TaskID,
                                            (long)totalCount, // resultCount,
                                            (long)send_count,
                                            temp,
                                            "", // errorInfo,
                                            errorCode
                                            );
                                        send_count += temp.Count;
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("中心向前端分发 responseGetMessage() 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
                                        return false;
                                    }
                                    records.RemoveRange(0, temp.Count);
                                }
                            }
                        }

                        // 发送结束信号
                        if (item == null
                            && string.IsNullOrEmpty(errorCode) == true)
                        {
                            Debug.WriteLine("专门发出一次结束信号");
                            try
                            {
                                Clients.Client(search_info.RequestConnectionID).responseGetMessage(
                                    param.TaskID,
                                    (long)totalCount, // resultCount,
                                    (long)send_count,
                                    records,
                                    "", // errorInfo,
                                    "_complete" // errorCode
                                    );
                                send_count += records.Count;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("中心向前端分发 responseGetMessage() 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
                                return false;
                            }
                        }

                        return true;
                    }).ConfigureAwait(false); // .Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("send message exception: " + ExceptionUtil.GetExceptionText(ex));
                Clients.Client(search_info.RequestConnectionID).responseGetMessage(
    param.TaskID,
    -1, // resultCount,
    0,
    null,
    ExceptionUtil.GetExceptionText(ex), // errorInfo,
    "_sendExeption" // errorCode
    );
            }
            finally
            {
                // 主动清除已经完成的任务对象
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
                    result.Users = BuildUsers(ServerInfo.UserDatabase.GetUsersByName(userName, start, count).Result);
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

                    result.Users = BuildUsers(ServerInfo.UserDatabase.GetUsersByName(userName, start, count).Result);
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

        // 正规化 UserItem 里面的 groups 定义
        // groups 的每一个元素，都是一个完整的群名定义
        void CanonicalizeUserItemGroups(UserItem item)
        {
            if (item.groups == null)
                return;

            if (item.groups != null && item.groups.Length == 1
                && string.IsNullOrEmpty(item.groups[0]))
            {
                item.groups = null;
                return;
            }

            List<string> correct_groups = new List<string>();   // 修正后的 groups 字符串集合。2017/12/31

            for (int i = 0; i < item.groups.Length; i++)
            {
                string strGroupString = item.groups[i];
                if (string.IsNullOrEmpty(strGroupString))
                    continue;

                GroupSegment segment = new GroupSegment(strGroupString);

                segment.Canonicalize((name) =>
                {
                    if (name.Type == "un")
                    {
                        List<UserItem> results = ServerInfo.UserDatabase.GetUsersByName(name.Text, 0, 1).Result;
                        if (results == null || results.Count == 0)
                            throw new Exception("未知的用户名 '" + name.Text + "'");
                        return new GroupName("ui", results[0].id);
                    }
                    if (name.Type == "gn")
                    {
                        List<GroupItem> groups = ServerInfo.GroupDatabase.GetGroupsByName(name.Text, 0, 1).Result;
                        if (groups == null || groups.Count == 0)
                            throw new Exception("未知的组名 '" + name.Text + "'");
                        return new GroupName("gi", groups[0].id);
                    }
                    return name;
                });

                // item.groups[i] = segment.ToStringUnQuote();
                correct_groups.Add(segment.ToStringUnQuote());
            }

            item.groups = correct_groups.ToArray();
        }

        static bool AutoBindingIP(UserItem info, string strClientAddress)
        {
            string strBinding = info.binding;
            if (string.IsNullOrEmpty(strBinding))
                return false;

            bool bChanged = false;
            List<string> temp = StringUtil.ParseTwoPart(strClientAddress, "@");
            string ip = temp[0];

            if (ip == "::1" || ip == "127.0.0.1")
                ip = "localhost";

            List<string> results = new List<string>();
            string[] parts = strBinding.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in parts)
            {
                string strLine = s.Trim();
                if (string.IsNullOrEmpty(strLine))
                    continue;
                string strLeft = "";
                string strRight = "";
                StringUtil.ParseTwoPart(strLine, ":", out strLeft, out strRight);
                if (strLeft == "ip")
                {
                    if (strRight == "[current]")
                    {
                        // 替换为当前前端的 ip 地址
                        results.Add("ip:" + ip);
                        bChanged = true;
                        continue;
                    }
                }
                results.Add(strLine);
            }

            info.binding = StringUtil.MakePathList(results, ",");
            return bChanged;
        }

        // 设置用户。包括增删改功能
        // 可能返回的错误码:
        //      Denied
        public async Task<MessageResult> SetUsers(string action, List<UserItem> users)
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
                ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
result,
"SetUsers()",
true);
                if (connection_info == null)
                    return result;

                // 正规化组名
                if (action == "create" || action == "change")
                {
                    foreach (UserItem item in users)
                    {
                        CanonicalizeUserItemGroups(item);
                    }
                }

                if (action == "create")
                {
                    // 验证请求者是否有 supervisor 权限
                    if (StringUtil.Contains(connection_info.Rights, "supervisor") == false)
                    {
                        result.String = "Denied";
                        result.Value = -1;
                        result.ErrorInfo = "当前用户不具备 supervisor 权限，create 命令被拒绝";
                        return result;
                    }

                    foreach (UserItem item in users)
                    {
                        List<UserItem> exists = ServerInfo.UserDatabase.GetUsersByName(item.userName, 0, 1).Result;

                        if (exists.Count > 0)
                        {
                            result.String = "AlreadyExist";
                            result.Value = -1;
                            result.ErrorInfo = "create 命令失败: 用户名为 '" + item.userName + "' 的账户已经存在";
                            return result;
                        }

                        // 2016/9/16
                        AutoBindingIP(item, connection_info.ClientIP);

                        await ServerInfo.UserDatabase.Add(item).ConfigureAwait(false);  // .Wait();
                    }
                }
                else if (action == "change")
                {
                    // TODO: 如果 groups 成员发生了修改，要重新加入 SignalR 的 group
                    // 或者为了省事，全部重新加入一次

                    // 验证请求者是否有 supervisor 权限
                    if (StringUtil.Contains(connection_info.Rights, "supervisor") == false)
                    {
                        result.String = "Denied";
                        result.Value = -1;
                        result.ErrorInfo = "当前用户不具备 supervisor 权限，change 命令被拒绝";
                        return result;
                    }

                    foreach (UserItem item in users)
                    {
                        await ServerInfo.UserDatabase.Update(item).ConfigureAwait(false); // .Wait();
                    }
                }
                else if (action == "changePassword")
                {
                    // 超级用户可以修改所有用户的密码。而普通用户只能修改自己的密码
                    foreach (UserItem item in users)
                    {
                        if (StringUtil.Contains(connection_info.Rights, "supervisor") == false
                            && item.userName != connection_info.UserName)
                        {
                            result.String = "Denied";
                            result.Value = -1;
                            result.ErrorInfo = "当前用户不具备 supervisor 权限，试图修改其他用户 '" + item.userName + "' 密码的 changePassword 命令被拒绝";
                            return result;
                        }

                        await ServerInfo.UserDatabase.UpdatePassword(item).ConfigureAwait(false); // .Wait();
                    }
                }
                else if (action == "delete")
                {
                    // TODO: 注意从 SignalR 的 group 中 remove

                    // 验证请求者是否有 supervisor 权限
                    if (StringUtil.Contains(connection_info.Rights, "supervisor") == false)
                    {
                        result.String = "Denied";
                        result.Value = -1;
                        result.ErrorInfo = "当前用户不具备 supervisor 权限，delete 命令被拒绝";
                        return result;
                    }

                    foreach (UserItem item in users)
                    {
                        await ServerInfo.UserDatabase.Delete(item).ConfigureAwait(false); // .Wait();
                    }
                }
                else
                {
                    result.String = "ActionError";
                    result.Value = -1;
                    result.ErrorInfo = "无法识别的 action 参数值 '" + action + "'";
                }

                // 启动一个任务，刷新已经登录的连接中保持在内存的账户信息
                Task.Run(() => RefreshUserInfo(action, users));
            }
            catch (Exception ex)
            {
                result.SetError("SetUsers() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);

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
            user.binding = item.binding;
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
                // 要让 SignalR 层的连接删除
                CallClientClose(info.ConnectionID, "", TimeSpan.FromTicks(0));
#if NO
                try
                {
                    CloseRequest request = new CloseRequest("");
                    Clients.Client(info.ConnectionID).close(request);
                }
                catch (Exception ex)
                {
                    Console.Write("切断前端通讯通道时出现异常: " + ExceptionUtil.GetExceptionText(ex));
                }
#endif
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
                ServerInfo.WriteErrorLog(result.ErrorInfo);
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
                ServerInfo.WriteErrorLog(result.ErrorInfo);
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

        MessageResult ClearConnection(string connection_id)
        {
            MessageResult result = new MessageResult();

            if (connection_id != "*")
            {
                ConnectionInfo connection_info = ServerInfo.ConnectionTable.GetConnection(connection_id);
                if (connection_info == null)
                {
                    connection_info = ServerInfo.ConnectionTable.GetConnectionByUserName(connection_id);
                    if (connection_info == null)
                    {
                        result.ErrorInfo = "拟清除的 id 或 用户名 为 '" + connection_id + "' 的 connection 没有找到";
                        result.Value = -1;
                        result.String = "";
                        return result;
                    }
                }
            }

            if (connection_id == "*")
                ServerInfo.ConnectionTable.Clear();
            else
            {
                ConnectionInfo info = ServerInfo.ConnectionTable.GetConnectionByUserName(connection_id);
                if (info == null)
                    ServerInfo.ConnectionTable.RemoveConnection(connection_id);
                else
                    ServerInfo.ConnectionTable.RemoveConnection(info.ConnectionID);
            }
            result.Value = 0;
            return result;
        }

        public MessageResult RequestGetConnectionInfo(GetConnectionInfoRequest param)
        {
            MessageResult result = new MessageResult();

            try
            {
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

                if (param.Operation == "clear")
                {
                    // 检查请求者是否具备操作的权限
                    if (StringUtil.Contains(connection_info.Rights, "clearConnection") == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "当前用户 '" + connection_info.UserName + "' 不具备进行 '" + "clearConnection" + "' 操作的权限";
                        return result;
                    }
                    return ClearConnection(param.QueryWord);
                }
                else if (param.Operation == "close" || param.Operation == "reconnect")
                {
                    // 检查请求者是否具备操作的权限
                    if (StringUtil.Contains(connection_info.Rights, "clearConnection") == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "当前用户 '" + connection_info.UserName + "' 不具备进行 '" + "clearConnection" + "' 操作的权限";
                        return result;
                    }

                    string id = "";
                    // 试探一下看看是不是用户名
                    ConnectionInfo info = ServerInfo.ConnectionTable.GetConnectionByUserName(param.QueryWord);
                    if (info != null)
                        id = info.ConnectionID;
                    else
                        id = param.QueryWord;

                    try
                    {
                        string strAction = "";
                        if (param.Operation == "reconnect")
                            strAction = "reconnect";
                        CloseRequest request = new CloseRequest(strAction);
                        Clients.Client(id).close(request);
                    }
                    catch (Exception ex)
                    {
                        string strError = "dp2mserver 切断前端通讯通道 '" + param.QueryWord + "' 时出现异常: " + ExceptionUtil.GetExceptionText(ex);
                        Console.Write(strError);
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        return result;
                    }

                    result.Value = 0;
                    return result;
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
                Task.Run(() => SearchConnectionInfoAndResponse(param));

                result.Value = 1;   // 成功
            }
            catch (Exception ex)
            {
                result.SetError("RequestGetConnectionInfo() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }

            return result;
        }

        void SearchConnectionInfoAndResponse(
            GetConnectionInfoRequest param)
        {
            SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(param.TaskID, false);
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
                    ConnectionRecord record = new ConnectionRecord(
                        info.ConnectionID,
                        info.UserName,
            info.Rights,
            info.Duty,
            info.Department, // department,
            info.Tel, // tel,
            info.Comment, // comment,
            info.LibraryUID,
            info.LibraryName,
            info.LibraryUserName,
            info.PropertyList,
            info.ClientIP,
            info.Notes);
                    records.Add(record);
                    if (records.Count >= batch_size
                        || i == count - 1)
                    {
                        // 每次发送前，重新试探获得一次，可以有效探知前端已经 CancelSearch() 的情况
                        search_info = ServerInfo.SearchTable.GetSearchInfo(param.TaskID, false);
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
                // 主动清除已经完成的任务对象
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
                    result.ErrorInfo = "ID 为 '" + taskID + "' 的任务对象无法找到";
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
                        // 主动清除已经完成的任务对象
                        ServerInfo.SearchTable.RemoveSearch(taskID);
                        Console.WriteLine("complete");
                    }
                }
            }
            catch (Exception ex)
            {
                result.SetError("ResponseGetConnectionInfo() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);
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
                SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(taskID, false);
                if (search_info == null)
                {
                    result.ErrorInfo = "ID 为 '" + taskID + "' 的任务对象无法找到";
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
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }
            return result;
        }

        #endregion

        #region WebCall() API

        public MessageResult RequestWebCall(
    string userNameList,
    WebCallRequest param
    )
        {

#if LOG
            writeDebug("RequestWebCall.1 userNameList=" + userNameList
                + ", param=" + param.Dump());
#endif
            MessageResult result = new MessageResult();

            try
            {
                ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
    result,
    "RequestWebCall()",
    false);
                if (connection_info == null)
                    return result;

#if LOG
                writeDebug("RequestWebCall.2");
#endif

                // 检查请求者是否具备操作的权限
                if (StringUtil.Contains(connection_info.Rights, "webCall") == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "当前用户 '" + connection_info.UserName + "' 不具备进行 '" + "webCall" + "' 操作的权限";
                    return result;
                }

#if LOG
                writeDebug("RequestWebCall.3");
#endif

                List<string> connectionIds = null;
                string strError = "";

                int nRet = ServerInfo.ConnectionTable.GetOperTargetsByUserName(
                    userNameList,
                    connection_info.UserName,
                    "webCall",
                    "strict_one", // "all",
                    out connectionIds,
                    out strError);
                if (nRet == -1)
                {
                    result.Value = -1;
                    result.ErrorInfo = strError;
                    return result;
                }

#if LOG
                writeDebug("RequestWebCall.4");
#endif

                if (connectionIds == null || connectionIds.Count == 0)
                {
                    result.Value = 0;
                    result.ErrorInfo = "当前没有发现可操作的目标 (详情 '" + strError + "')";
                    result.String = "TargetNotFound";
                    return result;
                }
#if LOG
                writeDebug("RequestWebCall.5");
#endif

                SearchInfo search_info = null;
                try
                {
                    if (param.First)
                        search_info = ServerInfo.SearchTable.AddSearch(Context.ConnectionId,
                            param.TaskID,
                            0,  // searchParam.Start,
                            -1, // searchParam.Count,
                            ""  // searchParam.ServerPushEncoding
                        );
                    else
                    {
                        search_info = ServerInfo.SearchTable.GetSearchInfo(param.TaskID);
                        if (search_info == null)
                        {
                            result.ErrorInfo = "ID 为 '" + param.TaskID + "' 的任务对象无法找到";
                            result.Value = -1;
                            result.String = "_notFound";
                            return result;
                        }
                    }
                }
                catch (ArgumentException)
                {
                    result.Value = -1;
                    result.ErrorInfo = "TaskID '" + param.TaskID + "' 已经存在了，不允许重复使用";
                    return result;
                }

                result.String = search_info.UID;   // 返回检索请求的 UID
                search_info.SetTargetIDs(connectionIds);

#if LOG
                writeDebug("RequestWebCall.6 sendSearch connectionIds=" + StringUtil.MakePathList(connectionIds.ToList<string>()));
#endif
                if (param.Complete == false)
                    SendWebCall(connectionIds, param);
                else
                    Task.Run(() => SendWebCall(connectionIds, param));

                result.Value = connectionIds.Count;   // 表示已经成功发起了操作
            }
            catch (Exception ex)
            {
                result.SetError("RequestWebCall() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }
            return result;
        }

        void SendWebCall(List<string> connectionIds, WebCallRequest param)
        {
            Clients.Clients(connectionIds).webCall(
    param);
        }

        // parameters:
        //      resultCount    命中的总的结果数。如果为 -1，表示检索出错，errorInfo 会给出出错信息
        //                      这个值实际上是表示全部命中结果的数目，可能比 records 中的元素要多
        //      start  records 参数中的第一个元素，在总的命中结果集中的偏移
        //      errorInfo   错误信息
        public MessageResult ResponseWebCall(WebCallResponse responseParam)
        {
#if LOG
            writeDebug("ResponseWebCall.1 responseParam=" + responseParam.Dump());
#endif
            MessageResult result = new MessageResult();
            try
            {
#if NO
                Console.WriteLine("ResponseSearch start=" + responseParam.Start
                    + ", records.Count=" + (responseParam.Records == null ? "null" : responseParam.Records.Count.ToString())
                    + ", errorInfo=" + responseParam.ErrorInfo
                    + ", errorCode=" + responseParam.ErrorCode);
#endif
                SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(responseParam.TaskID);
                if (search_info == null)
                {
                    result.ErrorInfo = "ID 为 '" + responseParam.TaskID + "' 的任务对象无法找到";
                    result.Value = -1;
                    result.String = "_notFound";
                    return result;
                }

#if LOG
                writeDebug("ResponseSearch.2");
#endif

#if NO
                Task.Run(() =>
                SendWebCallResponse(
    search_info,
    responseParam));
#endif
                // 此处不应另开线程进行处理。否则会在请求方引起数据错误
                SendWebCallResponse(
search_info,
responseParam);

            }
            catch (Exception ex)
            {
                result.SetError("ResponseWebCall() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }
            return result;
        }

        void SendWebCallResponse(// string taskID,
    SearchInfo search_info,
    WebCallResponse responseParam)
        {
#if LOG
            writeDebug("SendWebCallResponse.1");
#endif

            // 让前端获得响应结果
#if NO
            try
            {
#endif
            Clients.Client(search_info.RequestConnectionID).responseWebCall(
                responseParam);

#if LOG
                writeDebug("SendWebCallResponse.2");
#endif

            // 主动清除已经完成的任务对象
            if (responseParam.Complete == true)
                ServerInfo.SearchTable.RemoveSearch(search_info.UID);
#if NO
            }
            catch (Exception ex)
            {
                ServerInfo.WriteErrorLog("中心向前端分发 responseWebCall() 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
            }
#endif

#if LOG
            writeDebug("SendWebCallResponse.3");
#endif
        }

        #endregion

        #region Search() API

        void writeDebug(string strText)
        {
            ServerInfo.WriteErrorLog("debug: " + strText);
        }

        // return:
        //      result.Value    -1 出错; 0 没有任何检索目标; >0 成功发起检索，数字代表给多少个目标发出了请求
        public MessageResult RequestSearch(
            string userNameList,
            SearchRequest searchParam
            )
        {
            // throw new Exception("test exception");

            if (searchParam.Count == 0)
                searchParam.Count = -1;
#if LOG || LOG_REQUEST_SEARCH
            writeDebug("RequestSearch.1 userNameList=" + userNameList
                + ", searchParam=" + searchParam.Dump());
#endif
            MessageResult result = new MessageResult();

            try
            {
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

#if LOG || LOG_REQUEST_SEARCH
                writeDebug("RequestSearch.2");
#endif

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

#if LOG || LOG_REQUEST_SEARCH
                writeDebug("RequestSearch.3");
#endif

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
                        "strict_one", // "all",
                        out connectionIds,
                        out strError);
                    if (nRet == -1)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        return result;
                    }
                }

#if LOG || LOG_REQUEST_SEARCH
                writeDebug("RequestSearch.4");
#endif

                if (connectionIds == null || connectionIds.Count == 0)
                {
                    result.Value = 0;
                    // result.ErrorInfo = "当前没有任何可检索的目标 (目标用户名 '"+userNameList+"'; 操作 '"+searchParam.Operation+"')";
                    result.ErrorInfo = "当前没有发现可检索的目标 (详情 '" + strError + "')";
                    result.String = "TargetNotFound";
                    return result;
                }
#if LOG || LOG_REQUEST_SEARCH
                writeDebug("RequestSearch.5");
#endif


                SearchInfo search_info = null;
                try
                {
                    search_info = ServerInfo.SearchTable.AddSearch(Context.ConnectionId,
                        searchParam.TaskID,
                        searchParam.Start,
                        searchParam.Count,
                        searchParam.ServerPushEncoding);
                }
                catch (ArgumentException)
                {
                    result.Value = -1;
                    result.ErrorInfo = "TaskID '" + searchParam.TaskID + "' 已经存在了，不允许重复使用";
                    return result;
                }

                result.String = search_info.UID;   // 返回检索请求的 UID
                search_info.SetTargetIDs(connectionIds);

#if LOG || LOG_REQUEST_SEARCH
                writeDebug("RequestSearch.6 sendSearch connectionIds=" + StringUtil.MakePathList(connectionIds.ToList<string>()));
#endif
                Task.Run(() => SendSearch(connectionIds, searchParam));

                result.Value = connectionIds.Count;   // 表示已经成功发起了检索
            }
            catch (Exception ex)
            {
                result.SetError("RequestSearch() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }
            return result;
        }

        void SendSearch(List<string> connectionIds, SearchRequest searchParam)
        {
            Clients.Clients(connectionIds).search(
    searchParam);
        }

        // object _lock = new object();

        // parameters:
        //      resultCount    命中的总的结果数。如果为 -1，表示检索出错，errorInfo 会给出出错信息
        //                      这个值实际上是表示全部命中结果的数目，可能比 records 中的元素要多
        //      start  records 参数中的第一个元素，在总的命中结果集中的偏移
        //      errorInfo   错误信息
        public MessageResult ResponseSearch(SearchResponse responseParam)
        {
#if LOG
            writeDebug("ResponseSearch.1 responseParam=" + responseParam.Dump());
#endif
            MessageResult result = new MessageResult();
            try
            {
                Console.WriteLine("ResponseSearch start=" + responseParam.Start
                    + ", records.Count=" + (responseParam.Records == null ? "null" : responseParam.Records.Count.ToString())
                    + ", errorInfo=" + responseParam.ErrorInfo
                    + ", errorCode=" + responseParam.ErrorCode);

                SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(responseParam.TaskID);
                if (search_info == null)
                {
                    result.ErrorInfo = "ID 为 '" + responseParam.TaskID + "' 的任务对象无法找到";
                    result.Value = -1;
                    result.String = "_notFound";
                    return result;
                }

#if LOG
                writeDebug("ResponseSearch.2");
#endif

                // 给 RecPath 加上 @ 部分
                if (responseParam.Records != null)
                {
                    ConnectionInfo connection_info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
                    if (connection_info == null)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。回传检索结果失败";
                        return result;
                    }

                    // 强行覆盖
                    // responseParam.LibraryUID = connection_info.LibraryName + "|" + connection_info.LibraryUID;
                    if (string.IsNullOrEmpty(connection_info.LibraryName) == false
                        || string.IsNullOrEmpty(connection_info.LibraryUID) == false)
                        responseParam.LibraryUID = connection_info.LibraryName + "|" + connection_info.LibraryUID;

#if NO
                    string strLongPostfix = connection_info.LibraryName + "|" + connection_info.LibraryUID;
                    string strShortPostfix = "|" + connection_info.LibraryUID;
#endif

                    Encoding encoding = null;
                    if (string.IsNullOrEmpty(search_info.ServerPushEncoding) == false)
                        encoding = Encoding.GetEncoding(search_info.ServerPushEncoding);

                    // 第一条记录是长的后缀，其他记录是短的后缀。这样可以节约不少空间
                    int i = 0;
                    foreach (Record record in responseParam.Records)
                    {
#if NO
                        record.RecPath = record.RecPath + "@"
                            + (i == 0 ? strLongPostfix : strShortPostfix);
#endif

                        // 校验一下 MD5
                        if (string.IsNullOrEmpty(record.MD5) == false)
                        {
                            string strMD5 = StringUtil.GetMd5(record.Data);
                            if (record.MD5 != strMD5)
                                throw new Exception("dp2MServer : 记录 '" + record.RecPath + "' Data 的 MD5 校验出现异常");
                        }

                        if (encoding != null)
                        {
                            record.Data = EncodeString(record.Data, encoding);
                            record.RecPath = EncodeString(record.RecPath, encoding);
                        }

                        i++;
                    }
                }

#if LOG
                writeDebug("ResponseSearch.3 SendResponse");
#endif
                Task.Run(() =>
                SendSearchResponse(// string taskID,
    search_info,
    responseParam));
            }
            catch (Exception ex)
            {
                result.SetError("ResponseSearch() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }
            return result;
        }

        static string EncodeString(string strText, Encoding encoding)
        {
            if (string.IsNullOrEmpty(strText))
                return strText;
            return GetString(encoding.GetBytes(strText));
        }

        // 将每个 byte 翻译为 char，构成字符串
        static string GetString(byte[] bytes)
        {
            StringBuilder text = new StringBuilder();
            foreach (byte b in bytes)
            {
                text.Append((char)b);
            }
            return text.ToString();
        }

        void SendSearchResponse(// string taskID,
            SearchInfo search_info,
            SearchResponse responseParam)
        {
#if LOG
            writeDebug("SendResponse.1");
#endif

            // Thread.Sleep(500);
            // 让前端获得检索结果
            try
            {
                Clients.Client(search_info.RequestConnectionID).responseSearch(
                    responseParam);

#if LOG
                writeDebug("SendResponse.2");
#endif

                // 标记结束一个检索目标
                // return:
                //      0   尚未结束
                //      1   结束
                //      2   全部结束
                int nRet = search_info.CompleteTarget(Context.ConnectionId,
                    responseParam.ResultCount,
                    responseParam.Records == null ? 0 : responseParam.Records.Count);
                if (nRet == 2)
                {
                    // 追加一个消息，表示检索响应已经全部完成
                    Clients.Client(search_info.RequestConnectionID).responseSearch(
    new SearchResponse(
        search_info.UID,
    -1,
    -1,
    "", // libraryUID,
    null,
    "",
    ""));
                    // 主动清除已经完成的任务对象
                    ServerInfo.SearchTable.RemoveSearch(search_info.UID);  // taskID
                    Console.WriteLine("complete");
                }
            }
            catch (Exception ex)
            {
                ServerInfo.WriteErrorLog("中心向前端分发 responseSearch() 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
            }

#if LOG
            writeDebug("SendResponse.3");
#endif
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

            try
            {
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
                    result.String = "TargetNotFound";
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
            }
            catch (Exception ex)
            {
                result.SetError("RequestSetInfo() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }

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
                ServerInfo.WriteErrorLog(result.ErrorInfo);
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

            try
            {
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
                    result.String = "TargetNotFound";
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
            }
            catch (Exception ex)
            {
                result.SetError("RequestBindPatron() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }

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
                ServerInfo.WriteErrorLog(result.ErrorInfo);
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

            try
            {
                // 检查参数
                if (userNameList == "*")
                {
                    result.Value = -1;
                    result.ErrorInfo = "RequestCirculation() 不允许 userNameList 参数值为 '" + userNameList + "'";
                    return result;
                }

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
                    result.String = "TargetNotFound";
                    return result;
                }

                // 流通操作不允许广播式进行
                if (connectionIds.Count > 1)
                {
                    result.Value = -1;
                    result.ErrorInfo = "当前符合条件的操作目标多于 1 个。操作被拒绝";
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
            }
            catch (Exception ex)
            {
                result.SetError("RequestCirculation() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }

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
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }
            finally
            {
                // 主动清除已经完成的任务对象
                ServerInfo.SearchTable.RemoveSearch(taskID);
            }
            return result;
        }

        #endregion

        #region GetRes() API

        public MessageResult RequestGetRes(
            string userNameList,
            GetResRequest param)
        {
#if LOG
            writeDebug("RequestGetRes.1 userNameList=" + userNameList
                + ", param=" + param.Dump());
#endif
            MessageResult result = new MessageResult();

            try
            {
                ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
    result,
    "RequestGetRes()",
    false);
                if (connection_info == null)
                    return result;

                if (StringUtil.Contains(connection_info.Rights, param.Operation) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "当前用户 '" + connection_info.UserName + "' 不具备进行 '" + param.Operation + "' 操作的权限";
                    return result;
                }

                List<string> connectionIds = null;
                string strError = "";

                int nRet = ServerInfo.ConnectionTable.GetOperTargetsByUserName(
    userNameList,
    connection_info.UserName,
    param.Operation,
    "strict_one", // "all",
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
                    // result.ErrorInfo = "当前没有任何可检索的目标 (目标用户名 '"+userNameList+"'; 操作 '"+searchParam.Operation+"')";
                    result.ErrorInfo = "当前没有发现可操作的目标 (详情 '" + strError + "')";
                    result.String = "TargetNotFound";
                    return result;
                }

                SearchInfo search_info = null;
                try
                {
                    search_info = ServerInfo.SearchTable.AddSearch(Context.ConnectionId,
                        param.TaskID,
                        param.Start,
                        param.Length,
                        "" //param.ServerPushEncoding
                        );
                }
                catch (ArgumentException)
                {
                    result.Value = -1;
                    result.ErrorInfo = "TaskID '" + param.TaskID + "' 已经存在了，不允许重复使用";
                    return result;
                }

                result.String = search_info.UID;   // 返回检索请求的 UID
                search_info.SetTargetIDs(connectionIds);

                Task.Run(() => SendGetRes(connectionIds, param));

                result.Value = connectionIds.Count;   // 表示已经成功发起了检索
            }
            catch (Exception ex)
            {
                result.SetError("RequestGetRes() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }
            return result;
        }

        void SendGetRes(List<string> connectionIds, GetResRequest param)
        {
            Clients.Clients(connectionIds).getRes(param);
        }

        public MessageResult ResponseGetRes(GetResResponse responseParam)
        {
#if LOG
            writeDebug("ResponseGetRes.1 responseParam=" + responseParam.Dump());
#endif
            MessageResult result = new MessageResult();
            try
            {
                Console.WriteLine("ResponseGetRes start=" + responseParam.Start
                    + ", data.Count=" + (responseParam.Data == null ? "null" : responseParam.Data.Length.ToString())
                    + ", errorInfo=" + responseParam.ErrorInfo
                    + ", errorCode=" + responseParam.ErrorCode);

                SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(responseParam.TaskID);
                if (search_info == null)
                {
                    result.ErrorInfo = "ID 为 '" + responseParam.TaskID + "' 的任务对象无法找到";
                    result.Value = -1;
                    result.String = "_notFound";
                    return result;
                }

                search_info.Activate();

#if LOG
                writeDebug("ResponseGetRes.3 SendResponse");
#endif
                Task.Run(() =>
                SendGetResResponse(// string taskID,
    search_info,
    responseParam));
            }
            catch (Exception ex)
            {
                result.SetError("ResponseGetRes() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }
            return result;
        }

        void SendGetResResponse(// string taskID,
    SearchInfo search_info,
    GetResResponse responseParam)
        {
#if LOG
            writeDebug("SendGetResResponse.1");
#endif
            try
            {
                Clients.Client(search_info.RequestConnectionID).responseGetRes(
                    responseParam);

#if LOG
                writeDebug("SendGetResResponse.2");
#endif

                // 标记结束一个检索目标
                // return:
                //      0   尚未结束
                //      1   结束
                //      2   全部结束
                int nRet = search_info.CompleteTarget(Context.ConnectionId,
                    responseParam.TotalLength,
                    responseParam.Data == null ? 0 : responseParam.Data.Length);
                if (nRet == 2 || responseParam.Data == null || responseParam.Data.Length == 0)
                {
                    // 追加一个消息，表示检索响应已经全部完成
                    Clients.Client(search_info.RequestConnectionID).responseGetRes(
    new GetResResponse(
        search_info.UID,
    -1,
    -1,
    "",
    null,
    "",
    "",
    "",
    ""));
                    // 主动清除已经完成的任务对象
                    ServerInfo.SearchTable.RemoveSearch(search_info.UID);  // taskID
                    Console.WriteLine("complete");
                }
            }
            catch (Exception ex)
            {
                ServerInfo.WriteErrorLog("中心向前端分发 responseGetRes() 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
            }

#if LOG
            writeDebug("SendGetResResponse.3");
#endif
        }


        #endregion

        #region ListRes() API

        public MessageResult RequestListRes(
            string userNameList,
            ListResRequest param)
        {
#if LOG
            writeDebug("RequestListRes.1 userNameList=" + userNameList
                + ", param=" + param.Dump());
#endif
            MessageResult result = new MessageResult();

            try
            {
                ConnectionInfo connection_info = GetConnection(Context.ConnectionId,
    result,
    "RequestListRes()",
    false);
                if (connection_info == null)
                    return result;

                if (StringUtil.Contains(connection_info.Rights, param.Operation) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "当前用户 '" + connection_info.UserName + "' 不具备进行 '" + param.Operation + "' 操作的权限";
                    return result;
                }

                List<string> connectionIds = null;
                string strError = "";

                int nRet = ServerInfo.ConnectionTable.GetOperTargetsByUserName(
    userNameList,
    connection_info.UserName,
    param.Operation,
    "strict_one", // "all",
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
                    // result.ErrorInfo = "当前没有任何可检索的目标 (目标用户名 '"+userNameList+"'; 操作 '"+searchParam.Operation+"')";
                    result.ErrorInfo = "当前没有发现可操作的目标 (详情 '" + strError + "')";
                    result.String = "TargetNotFound";
                    return result;
                }

                SearchInfo search_info = null;
                try
                {
                    search_info = ServerInfo.SearchTable.AddSearch(Context.ConnectionId,
                        param.TaskID,
                        param.Start,
                        param.Length,
                        "" //param.ServerPushEncoding
                        );
                }
                catch (ArgumentException)
                {
                    result.Value = -1;
                    result.ErrorInfo = "TaskID '" + param.TaskID + "' 已经存在了，不允许重复使用";
                    return result;
                }

                result.String = search_info.UID;   // 返回检索请求的 UID
                search_info.SetTargetIDs(connectionIds);

                Task.Run(() => SendListRes(connectionIds, param));

                result.Value = connectionIds.Count;   // 表示已经成功发起了检索
            }
            catch (Exception ex)
            {
                result.SetError("RequestListRes() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }
            return result;
        }

        void SendListRes(List<string> connectionIds, ListResRequest param)
        {
            Clients.Clients(connectionIds).listRes(param);
        }

        public MessageResult ResponseListRes(ListResResponse responseParam)
        {
#if LOG
            writeDebug("ResponseListRes.1 responseParam=" + responseParam.Dump());
#endif
            MessageResult result = new MessageResult();
            try
            {
                Console.WriteLine("ResponseListRes start=" + responseParam.Start
                    + ", results.Count=" + (responseParam.Results == null ? "null" : responseParam.Results.Count.ToString())
                    + ", errorInfo=" + responseParam.ErrorInfo
                    + ", errorCode=" + responseParam.ErrorCode);

                SearchInfo search_info = ServerInfo.SearchTable.GetSearchInfo(responseParam.TaskID);
                if (search_info == null)
                {
                    result.ErrorInfo = "ID 为 '" + responseParam.TaskID + "' 的任务对象无法找到";
                    result.Value = -1;
                    result.String = "_notFound";
                    return result;
                }

                search_info.Activate();

#if LOG
                writeDebug("ResponseListRes.3 SendListResResponse");
#endif
                Task.Run(() =>
                SendListResResponse(// string taskID,
    search_info,
    responseParam));
            }
            catch (Exception ex)
            {
                result.SetError("ResponseListRes() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                ServerInfo.WriteErrorLog(result.ErrorInfo);
            }
            return result;
        }

        void SendListResResponse(
    SearchInfo search_info,
    ListResResponse responseParam)
        {
#if LOG
            writeDebug("SendListResResponse.1");
#endif
            try
            {
                Clients.Client(search_info.RequestConnectionID).responseListRes(
                    responseParam);

#if LOG
                writeDebug("SendListResResponse.2");
#endif

                // 标记结束一个检索目标
                // return:
                //      0   尚未结束
                //      1   结束
                //      2   全部结束
                int nRet = search_info.CompleteTarget(Context.ConnectionId,
                    responseParam.TotalLength,
                    responseParam.Results == null ? 0 : responseParam.Results.Count);
                if (nRet == 2 || responseParam.Results == null || responseParam.Results.Count == 0)
                {
                    // 追加一个消息，表示检索响应已经全部完成
                    Clients.Client(search_info.RequestConnectionID).responseListRes(
    new ListResResponse(
        search_info.UID,
    -1,
    -1,
    null,
    "",
    ""));
                    // 主动清除已经完成的任务对象
                    ServerInfo.SearchTable.RemoveSearch(search_info.UID);  // taskID
                    Console.WriteLine("complete");
                }
            }
            catch (Exception ex)
            {
                ServerInfo.WriteErrorLog("中心向前端分发 responseListRes() 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
            }

#if LOG
            writeDebug("SendListResResponse.3");
#endif
        }


        #endregion

        #region 几个事件

        // TODO: 当前系统一共用到了哪些名为 <...> 的组，需要从所有用户的 groups 定义中归纳。
        // 值得注意的是，当用户信息发生修改的时候，这个列表需要及时更新。
        public static string[] default_groups = new string[] {
            "gn:<default>",
            "gn:<dp2circulation>",
            "gn:<dp2catalog>",
            "gn:<dp2opac>",
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
            try
            {
                ConnectionInfo connection_info = ServerInfo.ConnectionTable.AddConnection(Context.ConnectionId);

                if (Context.Request.Environment.ContainsKey(USERITEM_KEY))
                {
                    UserItem useritem = (UserItem)Context.Request.Environment[USERITEM_KEY];
                    connection_info.UserItem = useritem;
                }

                string strParameters = Context.Request.Headers["parameters"];

                Hashtable table = StringUtil.ParseParameters(strParameters, ',', '=', "url");  // "url"
                connection_info.PropertyList = (string)table["propertyList"];
                connection_info.LibraryUID = (string)table["libraryUID"];
                connection_info.LibraryName = (string)table["libraryName"];
                connection_info.LibraryUserName = (string)table["libraryUserName"];
                connection_info.Notes = (string)table["notes"];   // 前端给出的识别用的注释 2016/10/15
                // 2016/8/11
                // TODO: 以下这一段是否会抛出异常? 需要测试验证一下
                try
                {
                    object ipAddress;
                    Context.Request.Environment.TryGetValue("server.RemoteIpAddress", out ipAddress);
                    connection_info.ClientIP = (string)ipAddress;
                }
                catch (Exception ex)
                {
                    ServerInfo.WriteErrorLog("AddConnection() 中获取 ClientIP 时出现异常: " + ExceptionUtil.GetDebugText(ex));
                }

                AddToSignalRGroup(connection_info, true);
            }
            catch (Exception ex)
            {
                ServerInfo.WriteErrorLog("AddConnection() 出现异常: " + ExceptionUtil.GetExceptionText(ex));
            }
        }

        void AddToSignalRGroup(ConnectionInfo connection_info, bool add = true)
        {
            if (connection_info == null)
                throw new ArgumentException("connection_info 参数值不应为空", "connection_info");

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
                        && StringUtil.ContainsRight(def.Definition, "n") == -1)
                    {
                        Console.WriteLine("'" + s + "' Skip join or un-join SignalR group '" + def.GroupNameString + "'");
                        goto CONTINUE;
                    }

                    if (add)
                    {
                        Console.WriteLine("'" + s + "' Join SignalR group '" + def.GroupNameString + "'");
                        Groups.Add(connection_info.ConnectionID, def.GroupNameString);
                    }
                    else
                    {
                        Console.WriteLine("'" + s + "' Un-Join SignalR group '" + def.GroupNameString + "'");
                        Groups.Remove(connection_info.ConnectionID, def.GroupNameString);
                    }

                CONTINUE:
                    defaults.Remove(def.GroupNameString);
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

            if (connection_info != null)
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

        static string[] CanonicalizeGroups(string[] groups_param)
        {
            if (groups_param == null || groups_param.Length == 0)
                return groups_param;

            if (groups_param.Length == 1 && string.IsNullOrEmpty(groups_param[0]))
                return null;

            // 正规化组名
            return GroupSegment.Canonicalize(groups_param, (name) =>
            {
                // 需要进行检索
                if (name.Type == "un")
                {
                    List<UserItem> users = ServerInfo.UserDatabase.GetUsersByName(name.Text, 0, 1).Result;
                    if (users == null || users.Count == 0)
                        throw new Exception("未知的用户名 '" + name.Text + "'");
                    return new GroupName("ui", users[0].id);
                }
                if (name.Type == "gn")
                {
                    List<GroupItem> groups = ServerInfo.GroupDatabase.GetGroupsByName(name.Text, 0, 1).Result;
                    if (groups == null || groups.Count == 0)
                        throw new Exception("未知的组名 '" + name.Text + "'");
                    return new GroupName("gi", groups[0].id);
                }
                return name;
            }, false);
        }

        static string GetIpAddress(IRequest request)
        {
            // TODO: 以下这一段是否会抛出异常?
            object ipAddress;
            request.Environment.TryGetValue("server.RemoteIpAddress", out ipAddress);
            return (string)ipAddress;
        }

        private static bool AuthenticateUser(
            IRequest request
            // string credentials
            )
        {
            try
            {
                if (request.Environment.ContainsKey(MyHub.USERITEM_KEY) == true)
                    return true;

                string userName = request.Headers["username"];

                if (string.IsNullOrEmpty(userName) == true)
                {
                    return true;    // 也算授权成功，但 request.Environment 里面没有用户对象
                    // return false;   // 不允许没有用户名登录了
                }

                string password = request.Headers["password"];

                // 获得用户信息
                var results = ServerInfo.UserDatabase.GetUsersByName(userName, 0, 1).Result;
                if (results.Count != 1)
                {
                    ServerInfo.WriteErrorLog("权限认证时，用户名 '" + userName + "' 不存在或多于一个 (" + results.Count.ToString() + ")");
                    return false;
                }

                var user = results[0];

                if (string.IsNullOrEmpty(user.binding) == false)
                {
                    string list = StringUtil.GetOneBinding(user.binding, "ip");
                    if (string.IsNullOrEmpty(list) == false)
                    {
                        // 检查 IP 地址
                        string ip = GetIpAddress(request);
                        if (StringUtil.MatchIpAddressList(list, ip) == false)
                        {
                            ServerInfo.WriteErrorLog("权限认证时，用户名 '" + user.userName + "' 其前端 IP '" + ip + "' 无法匹配 '" + list + "'");
                            return false;
                        }
                    }
                }

                string strHashed = Cryptography.GetSHA1(password);
                if (user.password != strHashed)
                {
                    ServerInfo.WriteErrorLog("权限认证时，用户名 '" + user.userName + "' 其密码不正确");
                    return false;
                }

                // 需要把 UserItem.groups 正规化
                // 可以规定，在保存账户信息阶段正规化。这样每次使用的时候就省心了

                // user.Groups 定义正规化
                user.groups = CanonicalizeGroups(user.groups);

                // 注： user 对象放在 Environment 里面所占空间何时释放?
                request.Environment[MyHub.USERITEM_KEY] = user;
                return true;
            }
            catch (Exception ex)
            {
                ServerInfo.WriteErrorLog("AuthenticateUser() 出现异常: " + ExceptionUtil.GetDebugText(ex));
                return false;
            }
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
