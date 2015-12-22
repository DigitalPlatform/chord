using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform.Text;

namespace DigitalPlatform.MessageServer
{
    /// <summary>
    /// 存储 ConnectionInfo 对象的集合
    /// </summary>
    public class ConnectionTable : Dictionary<string, ConnectionInfo>
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

        // 获得书目检索的目标 connection 的 id 集合
        // parameters:
        //      strRequestLibraryUID    发起检索的人所在的图书馆的 UID。本函数要在返回结果中排除这个 UID 的图书馆的连接
        // return:
        //      -1  出错
        //      0   成功
        public int GetBiblioSearchTargets(
            string strRequestLibraryUID,
            out List<string> connectionIds,
            out string strError)
        {
            strError = "";

            connectionIds = new List<string>();
            List<ConnectionInfo> infos = new List<ConnectionInfo>();

            this._lock.EnterReadLock();
            try
            {
                foreach (string key in this.Keys)
                {
                    ConnectionInfo info = this[key];

                    // 不检索来自同一图书馆的连接
                    if (info.LibraryUID == strRequestLibraryUID)
                        continue;

                    if (StringUtil.Contains(info.PropertyList, "biblio_search") == false)
                        continue;

                    infos.Add(info);
                }
            }
            finally
            {
                this._lock.ExitReadLock();
            }

            if (infos.Count == 0)
                return 0;

            infos.Sort((a, b) =>
            {
                int nRet = string.Compare(a.LibraryUID, b.LibraryUID);
                if (nRet != 0)
                    return nRet;
                return (int)a.SearchCount - (int)b.SearchCount;
            });

            // 对于每个目标图书馆，只选择一个连接。经过排序后，使用次数较小的在前
            string strPrevUID = "";
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

        // 获得书目检索的目标 connection 的 id 集合
        // parameters:
        //      strTargetUserNameList    被操作一方的用户名列表。本函数要搜索这些用户的连接
        //      strRequestUserName  发起操作一方的用户名。本函数要判断被操作方是否同意发起方进行操作
        //      strStyle            获取 id 集合的方式。first/all 之一
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

                    if (Array.IndexOf(target_usernames, strUserName) == -1)
                        continue;

                    // 如何表达允许操作的权限?
                    // getreaderinfo:username1|username2
                    // 如果没有配置，表示不允许
                    string strAllowUserList = StringUtil.GetParameterByPrefix(strDuty, strOperation + ":");
                    if (strAllowUserList == null
                        || StringUtil.Contains(strAllowUserList, strRequestUserName) == false)
                        continue;

                    infos.Add(info);
                    // TODO: 这里可以在找到第一个以后就退出循环，以提高速度
                }
            }
            finally
            {
                this._lock.ExitReadLock();
            }

            if (infos.Count == 0)
                return 0;

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
            string strPrevUID = "";
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

    }

    // Connection 查找表的一个事项
    public class ConnectionInfo
    {
        public string UID = "";
        public string ConnectionID = "";

        // public string UserName = "";    // 用户名
        public string PropertyList = "";    // 属性值列表 biblio_search
        public string LibraryUID = "";      // 用户所属图书馆的 UID。用它可以避免给若干同属一个图书馆的连接发送检索请求，因为这样势必会得到完全重复的命中结果
        public string LibraryName = "";     // 图书馆名

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
    }

}
