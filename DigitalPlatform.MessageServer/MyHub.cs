using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR;

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

            ConnectionInfo info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
            if (info == null)
            {
                result.Value = -1;
                result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。操作失败";
                return result;
            }

            // 验证请求者是否登录，是否有 supervisor 权限

            // 获得用户信息
            var task = ServerInfo.UserDatabase.GetUsers(userName, start, count);
            task.Wait();
            result.Users = task.Result;
            return result;
        }

        public MessageResult SetUsers(string action, List<UserItem> users)
        {
            MessageResult result = new MessageResult();

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
                foreach (UserItem item in users)
                {
                    ServerInfo.UserDatabase.Add(item);
                }
            }
            else if (action == "change")
            {
                foreach (UserItem item in users)
                {
                    ServerInfo.UserDatabase.Update(item);
                }
            }
            else if (action == "changePassword")
            {
                foreach (UserItem item in users)
                {
                    ServerInfo.UserDatabase.UpdatePassword(item);
                }
            }
            else if (action == "delete")
            {
                foreach (UserItem item in users)
                {
                    ServerInfo.UserDatabase.Delete(item);
                }
            }
            else
            {
                result.Value = -1;
                result.ErrorInfo = "无法识别的 action 参数值 '" + action + "'";
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

            ConnectionInfo info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
            if (info == null)
            {
                result.Value = -1;
                result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。登录失败";
                return result;
            }

            info.PropertyList = propertyList;
            info.LibraryUID = libraryUID;
            info.LibraryName = libraryName;
            return result;
        }

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


        // return:
        //      result.Value    -1 出错; 0 没有任何检索目标; 1 成功发起检索
        public MessageResult RequestSearchBiblio(
            string strSearchID,
            string dbNameList,
            string queryWord,
            string fromList,
            string matchStyle,
            string formatList,
            long maxResults)
        {
            MessageResult result = new MessageResult();

            ConnectionInfo connection_info = ServerInfo.ConnectionTable.GetConnection(Context.ConnectionId);
            if (connection_info == null)
            {
                result.Value = -1;
                result.ErrorInfo = "connection ID 为 '" + Context.ConnectionId + "' 的 ConnectionInfo 对象没有找到。请求检索书目失败";
                return result;
            }

            if (Global.Contains(connection_info.PropertyList, "biblio_search") == false)
            {
                result.Value = -1;
                result.ErrorInfo = "当前连接未开通书目检索功能";
                return result;
            }

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
                result.ErrorInfo = "当前没有任何可检索的目标";
                return result;
            }

            SearchInfo search_info = null;

            try
            {
                search_info = ServerInfo.SearchTable.AddSearch(Context.ConnectionId, strSearchID);
            }
            catch (ArgumentException)
            {
                result.Value = -1;
                result.ErrorInfo = "SearchID '" + strSearchID + "' 已经存在了，不允许重复使用";
                return result;
            }

            result.String = search_info.UID;   // 返回检索请求的 UID

            Clients.Clients(connectionIds).searchBiblio(// "searchBiblio",
                search_info.UID,   // 检索请求的 UID
                dbNameList,
                queryWord,
                fromList,
                matchStyle,
                formatList,
                maxResults);

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

        // parameters:
        //      resultCount    命中的总的结果数。如果为 -1，表示检索出错，errorInfo 会给出出错信息
        //      start  records 参数中的第一个元素，在总的命中结果集中的偏移
        //      errorInfo   错误信息
        public MessageResult ResponseSearchBiblio(string searchID,
            long resultCount,
            long start,
            IList<BiblioRecord> records,
            string errorInfo)
        {
            // Thread.Sleep(1000 * 60 * 2);
            MessageResult result = new MessageResult();
            SearchInfo info = ServerInfo.SearchTable.GetSearchInfo(searchID);
            if (info == null)
            {
                result.ErrorInfo = "ID 为 '" + searchID + "' 的检索对象无法找到";
                result.Value = -1;
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
                foreach (BiblioRecord record in records)
                {
                    // record.RecPath += "@UID:" + connection_info.LibraryUID;
                    record.LibraryName = connection_info.LibraryName;
                    record.LibraryUID = connection_info.LibraryUID;
                }
            }

            Clients.Client(info.RequestConnectionID).responseSearchBiblio(
                searchID,
                resultCount,
                start,
                records,
                errorInfo);

            // 判断响应是否为最后一个响应
            bool bRet = IsComplete(resultCount,
                start,
                records);
            if (bRet == true)
            {
                bool bAllComplete = info.CompleteTarget(Context.ConnectionId);
                if (bAllComplete)
                {
                    // 追加一个消息，表示检索响应已经全部完成
                    Clients.Client(info.RequestConnectionID).responseSearchBiblio(
    searchID,
    -1,
    -1,
    null,
    "");
                    // 主动清除已经完成的检索对象
                    ServerInfo.SearchTable.RemoveSearch(searchID);
                }
            }

            return result;
        }

        // 判断响应是否为(顺次发回的)最后一个响应
        static bool IsComplete(long resultCount,
            long start,
            IList<BiblioRecord> records)
        {
            if (resultCount == -1)
                return true;    // 出错，也意味着响应结束

            if (resultCount < 0)
                return false;   // -1 表示结果尺寸不确定

            if (records == null)
            {
                if (start >= resultCount)
                    return true;
                return false;
            }

            if (start + records.Count >= resultCount)
                return true;
            return false;
        }

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
        public List<UserItem> Users { get; set; }
    }

    public class GetRecordResult : MessageResult
    {
        public List<string> Formats { get; set; }
        public List<string> Results { get; set; }

        public string RecPath { get; set; }
        public string Timestamp { get; set; }
    }
}
