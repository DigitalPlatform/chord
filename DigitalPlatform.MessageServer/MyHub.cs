using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Microsoft.AspNet.SignalR;

using DigitalPlatform.Message;
using DigitalPlatform.Text;

namespace DigitalPlatform.MessageServer
{
    /// <summary>
    /// 
    /// </summary>
    public class MyHub : Hub
    {
        public void Send(string name, string message)
        {
            Clients.All.addMessage(name, message);
        }

        public GetUserResult GetUsers(string userName, int start, int count)
        {
            GetUserResult result = new GetUserResult();

            try
            {
                ConnectionInfo info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
                if (info == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。操作失败";
                    return result;
                }

                if (info.UserItem == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "尚未登录，无法使用 GetUsers() 功能";
                }

                // supervisor 权限用户，可以获得所有用户的信息
                if (StringUtil.Contains(info.UserItem == null ? "" : info.UserItem.rights,
                    "supervisor") == true)
                {
                    var task = ServerInfo.UserDatabase.GetUsers(userName, start, count);
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

                    var task = ServerInfo.UserDatabase.GetUsers(userName, start, count);
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
            return user;
        }

        public MessageResult SetUsers(string action, List<UserItem> users)
        {
            MessageResult result = new MessageResult();

            try
            {
                ConnectionInfo info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
                if (info == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。操作失败";
                    return result;
                }

                // 验证请求者是否登录，是否有 supervisor 权限

                if (action == "create")
                {
                    if (StringUtil.Contains(info.UserItem.rights, "supervisor") == false)
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
                    if (StringUtil.Contains(info.UserItem.rights, "supervisor") == false)
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
                        if (StringUtil.Contains(info.UserItem.rights, "supervisor") == false
                            && item.userName != info.UserItem.userName)
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
                    if (StringUtil.Contains(info.UserItem.rights, "supervisor") == false)
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

        // 登录，并告知 server 关于自己的一些属性。如果不登录，则 server 会按照缺省的方式设置这些属性，例如无法实现检索响应功能
        // parameters:
        //      propertyList    属性列表。
        public MessageResult Login(string userName,
            string password,
            string libraryUID,
            string libraryName,
            string propertyList)
        {
            MessageResult result = new MessageResult();
            try
            {
                ConnectionInfo info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
                if (info == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。登录失败";
                    return result;
                }

                if (string.IsNullOrEmpty(userName) == false)
                {
                    // 获得用户信息
                    var results = ServerInfo.UserDatabase.GetUsers(userName, 0, 1).Result;
                    if (results.Count != 1)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "用户名 '" + userName + "' 不存在。登录失败";
                        return result;
                    }
                    var user = results[0];
                    string strHashed = Cryptography.GetSHA1(password);

                    if (user.password != strHashed)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "密码不正确。登录失败";
                        return result;
                    }
                    info.UserItem = user;
                }

                info.PropertyList = propertyList;
                info.LibraryUID = libraryUID;
                info.LibraryName = libraryName;
            }
            catch (Exception ex)
            {
                result.SetError("Login() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                Console.WriteLine(result.ErrorInfo);
            }
            return result;
        }

        public MessageResult Logout()
        {
            MessageResult result = new MessageResult();
            try
            {
                ConnectionInfo info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
                if (info == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。登出失败";
                    return result;
                }

                // 登出。但连接依然存在
                info.UserItem = null;
                info.PropertyList = "";
                info.LibraryUID = "";
                info.LibraryName = "";
                info.SearchCount = 0;
            }
            catch (Exception ex)
            {
                result.SetError("Login() 时出现异常: " + ExceptionUtil.GetExceptionText(ex),
ex.GetType().ToString());
                Console.WriteLine(result.ErrorInfo);
            }
            return result;
        }

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
            "", // department,
            "", // tel,
            "", // comment,
            info.LibraryUID,
            info.LibraryName,
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
                            Console.WriteLine("中心向前端分发 responseSearch() 时出现异常: " + ExceptionUtil.GetExceptionText(ex));
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

        #region Search() API

        public MessageResult CancelSearch(string taskID)
        {
            MessageResult result = new MessageResult();
            try
            {
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

        // return:
        //      result.Value    -1 出错; 0 没有任何检索目标; 1 成功发起检索
        public MessageResult RequestSearch(
            string userNameList,
#if NO
            string operation,
            string searchID,
            string dbNameList,
            string queryWord,
            string fromList,
            string matchStyle,
            string formatList,
            long maxResults
#endif
 SearchRequest searchParam
            )
        {
            if (searchParam.Count == 0)
                searchParam.Count = -1;

            MessageResult result = new MessageResult();

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

            search_info.TargetIDs = connectionIds;
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

        #region SetInfo() API

        // return:
        //      result.Value    -1 出错; 0 没有任何检索目标; 1 成功发起检索
        public MessageResult RequestSetInfo(
            string userNameList,
            SetInfoRequest setInfoParam
            )
        {
            MessageResult result = new MessageResult();

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

            Clients.Clients(connectionIds).setInfo(
                setInfoParam);

            search_info.TargetIDs = connectionIds;
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

            Clients.Clients(connectionIds).bindPatron(
                bindPatronParam);

            search_info.TargetIDs = connectionIds;
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
            CirculationRequest circulationParam
            )
        {
            MessageResult result = new MessageResult();

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
            search_info.TargetIDs = connectionIds;
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

        public override Task OnConnected()
        {
            ServerInfo.ConnectionTable.AddConnection(Context.ConnectionId);

            //Program.WriteToConsole("Client connected: " + Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            ServerInfo.ConnectionTable.AddConnection(Context.ConnectionId);

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
            ServerInfo.ConnectionTable.RemoveConnection(Context.ConnectionId);

            //Program.WriteToConsole("Client disconnected: " + Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }
    }

}
