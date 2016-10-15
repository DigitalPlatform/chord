using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform.Text;
using System.Collections;

namespace DigitalPlatform.MessageServer
{
    /// <summary>
    /// 存储 ConnectionInfo 对象的集合
    /// </summary>
    public class ConnectionTable : Dictionary<string, ConnectionInfo>, IEnumerable
    {
        internal ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

#if NO
        // connection UID --> connection ID 的查找表
        static Dictionary<string, string> UidTable = new Dictionary<string, string>();
#endif

#if NO
        // connection ID  --> ConnectionInfo 的查找表。同时也是 ConnectionInfo 对象的存储机制
        static Dictionary<string, ConnectionInfo> ConnectionTable = new Dictionary<string, ConnectionInfo>();
#endif

#if NO
        // 根据 UID 查找 connection id
        public static string GetConnectionID(string strUID)
        {
            if (UidTable.ContainsKey(strUID) == false)
                return null;

            return UidTable[strUID];
        }

        // 根据 connection ID 查找 UID
        public static string GetUID(string strConnectionID)
        {
            if (ConnectionTable.ContainsKey(strConnectionID) == false)
                return null;

            ConnectionInfo info = ConnectionTable[strConnectionID];
            if (info == null)
                return null;

            return info.UID;
        }
#endif
        // TODO: 限制集合的最大元素数。防止被攻击
        // 新增一个 ConnectionInfo 对象
        public ConnectionInfo AddConnection(string strConnectionID)
        {
            this._lock.EnterWriteLock();
            try
            {
                ConnectionInfo info = null;
                if (this.ContainsKey(strConnectionID) == true)
                {
                    info = this[strConnectionID];
                    if (info != null)
                        return info;    // 已经存在
                }

                info = new ConnectionInfo();
                info.UID = Guid.NewGuid().ToString();
                info.ConnectionID = strConnectionID;
                this[strConnectionID] = info;
                return info;
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        // 移走一个 ConnectionInfo 对象
        public ConnectionInfo RemoveConnection(string strConnectionID)
        {
            this._lock.EnterWriteLock();
            try
            {
                if (this.ContainsKey(strConnectionID) == false)
                    return null;

                ConnectionInfo info = this[strConnectionID];

                this.Remove(strConnectionID);
                // UidTable.Remove(info.UID);

                return info;
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        // 获得一个 ConnectionInfo 对象
        public ConnectionInfo GetConnection(string strConnectionID)
        {
            this._lock.EnterReadLock();
            try
            {
                if (this.ContainsKey(strConnectionID) == false)
                    return null;
                return this[strConnectionID];
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        public new IEnumerator GetEnumerator()
        {  
            foreach (string key in this.Keys)
            {
                ConnectionInfo info = this[key];
                yield return info;
            }
        }

        // 获得书目检索的目标 connection 的 id 集合
        // parameters:
        //      strRequestLibraryUID    发起检索的人所在的图书馆的 UID。本函数要在返回结果中排除这个 UID 的图书馆的连接
        //      bOutputDebugInfo    是否输出调试信息？在生产环境下，连接数量可能很大，要意识到调试信息可能会很大
        // return:
        //      -1  出错
        //      0   成功
        public int GetBiblioSearchTargets(
            string strRequestLibraryUID,
            bool bOutputDebugInfo,
            out List<string> connectionIds,
            out string strError)
        {
            strError = "";

            StringBuilder debugInfo = new StringBuilder();

            connectionIds = new List<string>();
            List<ConnectionInfo> infos = new List<ConnectionInfo>();

            this._lock.EnterReadLock();
            try
            {
                if (bOutputDebugInfo)
                {
                    debugInfo.Append("Connection count=" + this.Keys.Count + "\r\n");
                    debugInfo.Append("strRequestLibraryUID=" + strRequestLibraryUID + "\r\n");
                    debugInfo.Append("第一阶段匹配过程\r\n");
                }

                int i = 0;
                foreach (string key in this.Keys)
                {
                    ConnectionInfo info = this[key];

                    if (bOutputDebugInfo)
                        debugInfo.Append("--- " + (i + 1).ToString() + ") " + info.Dump() + "\r\n");

                    // 不检索来自同一图书馆的连接
                    if (info.LibraryUID == strRequestLibraryUID)
                    {
                        if (bOutputDebugInfo)
                            debugInfo.Append("不检索来自和 '" + strRequestLibraryUID + "' 同一图书馆的目标\r\n");
                        i++;
                        continue;
                    }

                    string strUserName = "";
                    string strDuty = "";
                    if (info.UserItem != null)
                    {
                        strUserName = info.UserItem.userName;
                        strDuty = info.UserItem.duty;
                    }

                    if (StringUtil.IsInList("shareBiblio", strDuty) == false
                        && StringUtil.Contains(info.PropertyList, "biblio_search") == false)
                    {
                        if (bOutputDebugInfo)
                            debugInfo.Append("Duty 里面没有包含 'shareBiblio' 并且 PropertyList 没有包含 'biblio_search'\r\n");
                        i++;
                        continue;
                    }

                    if (bOutputDebugInfo)
                        debugInfo.Append("匹配上了\r\n");
                    infos.Add(info);
                    i++;
                }
            }
            finally
            {
                this._lock.ExitReadLock();
            }

            if (infos.Count == 0)
            {
                if (bOutputDebugInfo)
                    strError = debugInfo.ToString();
                return 0;
            }

            infos.Sort((a, b) =>
            {
                int nRet = string.Compare(a.LibraryUID, b.LibraryUID);
                if (nRet != 0)
                    return nRet;
                return (int)a.SearchCount - (int)b.SearchCount;
            });

            if (bOutputDebugInfo)
            {
                debugInfo.Append("匹配上的数目=" + infos.Count + "\r\n");
                debugInfo.Append("第二阶段对每个图书馆筛选出一个目标\r\n");
            }

            {
                // 对于每个目标图书馆，只选择一个连接。经过排序后，使用次数较小的在前
                string strPrevUID = "~";    // 预设一个不可能的值
                int i = 0;
                foreach (ConnectionInfo info in infos)
                {
                    if (bOutputDebugInfo)
                        debugInfo.Append("--- " + (i + 1).ToString() + ") " + info.Dump() + "\r\n");

                    if (strPrevUID != info.LibraryUID)
                    {
                        connectionIds.Add(info.ConnectionID);
                        info.SearchCount++;
                        if (bOutputDebugInfo)
                            debugInfo.Append("strPrevUID '" + strPrevUID + "' != info.LibraryUID '" + info.LibraryUID + "', 被选中\r\n");
                    }
                    else
                    {
                        if (bOutputDebugInfo)
                            debugInfo.Append("没有被选中\r\n");
                    }

                    strPrevUID = info.LibraryUID;
                    i++;
                }
            }

            if (bOutputDebugInfo)
                strError = debugInfo.ToString();
            return 0;
        }

        // 获得书目检索的目标 connection 的 id 集合
        // parameters:
        //      strTargetUserNameList    被操作一方的用户名列表。本函数要搜索这些用户的连接
        //      strRequestUserName  发起操作一方的用户名。本函数要判断被操作方是否同意发起方进行操作
        //      strStyle            获取 id 集合的方式。first/all/strict_one 之一
        // return:
        //      -1  出错
        //      0   成功
        public int GetOperTargetsByUserName(
            string strTargetUserNameList,
            string strRequestUserName,
            string strOperation,
            string strStyle,
            out List<string> connectionIds,
            out string strError)
        {
            strError = "";

            List<string> matched_usernames = new List<string>();    // 匹配上的用户名。用于精确报错

            connectionIds = new List<string>();
            List<ConnectionInfo> infos = new List<ConnectionInfo>();

            this._lock.EnterReadLock();
            try
            {
                string[] target_usernames = strTargetUserNameList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                // TODO: 后面需要给 username 提供一个 hashtable 索引，提高运行速度
                foreach (string key in this.Keys)
                {
                    ConnectionInfo info = this[key];

                    string strUserName = "";
                    string strDuty = "";
                    if (info.UserItem != null)
                    {
                        strUserName = info.UserItem.userName;
                        strDuty = info.UserItem.duty;
                    }

                    if (strTargetUserNameList != "*"
                        && Array.IndexOf(target_usernames, strUserName) == -1)
                        continue;

                    matched_usernames.Add(strUserName);

                    if (strOperation == "searchBiblio"
                        && StringUtil.IsInList("shareBiblio", strDuty))
                    {

                    }
                    else
                    {
                        // 如何表达允许操作的权限?
                        // getreaderinfo:username1|username2
                        // 如果没有配置，表示不允许
                        string strAllowUserList = StringUtil.GetParameterByPrefixEnvironment(strDuty, strOperation, ":");
                        if (strAllowUserList != "" &&   // "" 表示所有用户名均通配
                            (strAllowUserList == null
                            || StringUtil.Contains(strAllowUserList, strRequestUserName, '|') == false)
                            )
                            continue;
                    }

                    infos.Add(info);
                    // TODO: 这里可以在找到第一个以后就退出循环，以提高速度
                }
            }
            finally
            {
                this._lock.ExitReadLock();
            }

            if (infos.Count == 0)
            {
                if (matched_usernames.Count == 0)
                    strError = "没有匹配上任何目标用户名 '" + strTargetUserNameList + "'";
                else
                    strError = "匹配的用户名 '" + StringUtil.MakePathList(matched_usernames) + "' 中没有找到满足操作 '" + strOperation + "' 的用户";
                return 0;
            }

            if (strStyle == "strict_one")
            {
                if (infos.Count > 1)
                {
                    strError = "匹配的目标通道超过一个，为 " + infos.Count + " 个，这是一个严重错误，请检查目标账户是否被多次用于登录了";
                    return -1;
                }
                // 选择遇到的第一个，也是唯一的一个
                ConnectionInfo info = infos[0];
                connectionIds.Add(info.ConnectionID);
                info.SearchCount++;
                return 0;
            }

            infos.Sort((a, b) =>
            {
                int nRet = string.Compare(a.LibraryUID, b.LibraryUID);
                if (nRet != 0)
                    return nRet;
                return (int)a.SearchCount - (int)b.SearchCount;
            });

            if (strStyle == "first")
            {
                // 选择使用次数较小的一个
                ConnectionInfo info = infos[0];
                connectionIds.Add(info.ConnectionID);
                info.SearchCount++;
                return 0;
            }

            // 对于每个目标图书馆，只选择一个连接。经过排序后，使用次数较小的在前
            string strPrevUID = "~";    // 预设一个不可能的值
            foreach (ConnectionInfo info in infos)
            {
                if (strPrevUID != info.LibraryUID)
                {
                    connectionIds.Add(info.ConnectionID);
                    info.SearchCount++;
                }
                strPrevUID = info.LibraryUID;
            }

            return 0;
        }

        // 根据 user id 列表，获得当前在线的 connection id 列表
        public List<string> GetConnectionIds(string [] groups)
        {
            List<string> results = new List<string>();
            foreach (string name in groups)
            {
                string id = GetConnectionId(name);
                if (id != null)
                    results.Add(id);
            }

            return results;
        }

        // 根据用户的 un ui 获得当前在线的 connection id
        public string GetConnectionId(string name_or_id)
        {
            GroupName name = GroupName.Build(name_or_id, "gn");
            foreach (string key in this.Keys)
            {
                ConnectionInfo info = this[key];
                if (name.Type == "un" && info.UserName == name.Text)
                    return info.ConnectionID;
                else if (name.Type == "ui" && info.UserID == name.Text)
                    return info.ConnectionID;
                else if (name.Type != "un" && name.Type != "ui")
                    throw new ArgumentException("不支持的名字类型 '" + name.Type + "' (应使用 un 或 ui)");
            }

            return null;
        }
    }

    // Connection 查找表的一个事项
    public class ConnectionInfo
    {
        public string Dump(string strDelimiter = "\r\n")
        {
            StringBuilder text = new StringBuilder();
            text.Append("UID=" + this.UID + strDelimiter);
            text.Append("ConnectionID=" + this.ConnectionID + strDelimiter);
            text.Append("UID=" + this.UID + strDelimiter);
            text.Append("UserName=" + this.UserName + strDelimiter);
            text.Append("PropertyList=" + this.PropertyList + strDelimiter);
            text.Append("LibraryUID=" + this.LibraryUID + strDelimiter);
            text.Append("LibraryName=" + this.LibraryName + strDelimiter);
            text.Append("Rights=" + this.Rights + strDelimiter);
            text.Append("Duty=" + this.Duty + strDelimiter);
            text.Append("Notes=" + this.Notes + strDelimiter);
            return text.ToString();
        }

        public string UID = "";
        public string ConnectionID = "";

        // public string UserName = "";    // 用户名
        public string PropertyList = "";    // 属性值列表 biblio_search
        public string ClientIP = "";        // 前端 IP 地址 2016/8/11

        public string LibraryUserName = ""; // 用户在本馆所用的用户名

        public string LibraryUID = "";      // 用户所属图书馆的 UID。用它可以避免给若干同属一个图书馆的连接发送检索请求，因为这样势必会得到完全重复的命中结果
        public string LibraryName = "";     // 图书馆名

        public string Notes = "";       // 前端给出的注释，用于识别不同的通道实例。2016/10/15

        public long SearchCount = 0;    // 响应检索请求的累积次数

        public UserItem UserItem = null;    // 登录后的用户信息

        public string UserName
        {
            get
            {
                if (this.UserItem == null)
                    return "";
                return this.UserItem.userName;
            }
        }

        public string Rights
        {
            get
            {
                if (this.UserItem == null)
                    return "";
                return this.UserItem.rights;
            }
        }

        public string Duty
        {
            get
            {
                if (this.UserItem == null)
                    return "";
                return this.UserItem.duty;
            }
        }

        public string UserID
        {
            get
            {
                if (this.UserItem == null)
                    return "";
                return this.UserItem.id;
            }
        }

        public string Department
        {
            get
            {
                if (this.UserItem == null)
                    return "";
                return this.UserItem.department;
            }
        }

        public string Tel
        {
            get
            {
                if (this.UserItem == null)
                    return "";
                return this.UserItem.tel;
            }
        }

        public string Comment
        {
            get
            {
                if (this.UserItem == null)
                    return "";
                return this.UserItem.comment;
            }
        }

        public string [] Groups
        {
            get
            {
                if (this.UserItem == null)
                    return null;
                return this.UserItem.groups;
            }
        }
    }

}
