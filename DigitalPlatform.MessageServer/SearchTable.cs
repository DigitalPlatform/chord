using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform;

namespace DigitalPlatform.MessageServer
{
    // TODO: 定期清除已超时的对象
    // 检索请求集合
    // search request UID --> SearchInfo 的查找表。同时也是 SearchInfo 对象的存储机制
    public class SearchTable : Dictionary<string, SearchInfo>, IDisposable
    {
        internal ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        CleanThread CleanThread = new CleanThread();

        public virtual void Dispose()
        {
            _lock.Dispose();
            CleanThread.Dispose();
        }

        public SearchTable(bool bEnable)
        {
            this.CleanThread.Container = this;
            this.CleanThread.BeginThread();
        }

        // 准备退出
        public void Exit()
        {
            this.CleanThread.StopThread(true);
        }

        // TODO: 限制集合的最大元素数。防止被攻击
        // 新增一个 SearchInfo 对象
        public SearchInfo AddSearch(string strConnectionID,
            string strSearchID = "",
            long returnStart = 0,
            long returnCount = -1)
        {
            this._lock.EnterWriteLock();
            try
            {
                // 查重
                if (string.IsNullOrEmpty(strSearchID) == false)
                {
                    if (this.ContainsKey(strSearchID) == true)
                        throw new ArgumentException("strSearchID", "strSearchID '" + strSearchID + "' 已经存在了");
                }

                SearchInfo info = new SearchInfo();
                info.CreateTime = DateTime.Now;
                if (string.IsNullOrEmpty(strSearchID) == true)
                    info.UID = Guid.NewGuid().ToString();
                else
                    info.UID = strSearchID;

                info.RequestConnectionID = strConnectionID;
                info.ReturnStart = returnStart;
                info.ReturnCount = returnCount;
                this[info.UID] = info;
                return info;
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        // 移走一个 Search 对象
        public SearchInfo RemoveSearch(string strSearchUID)
        {
            this._lock.EnterWriteLock();
            try
            {
                if (this.ContainsKey(strSearchUID) == false)
                    return null;
                SearchInfo info = this[strSearchUID];

                this.Remove(strSearchUID);
                return info;
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        // 根据 search request UID 查找 SearchInfo 对象
        public SearchInfo GetSearchInfo(string strUID)
        {
            this._lock.EnterReadLock();
            try
            {
                if (this.ContainsKey(strUID) == false)
                    return null;

                return this[strUID];
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        // 清除已经(超时)失效的对象
        public void Clean()
        {
            // Console.WriteLine("do clean ...");

            // 首先找到那些已经失效的对象
            List<string> keys = new List<string>();
            this._lock.EnterReadLock();
            try
            {
                foreach (string key in this.Keys)
                {
                    SearchInfo info = this[key];
                    if (info.IsTimeout() == true)
                        keys.Add(key);
                }
            }
            finally
            {
                this._lock.ExitReadLock();
            }

            // 然后从集合中移走这些对象
            if (keys.Count > 0)
            {
                this._lock.EnterWriteLock();
                try
                {
                    foreach (string key in keys)
                    {
                        if (this.ContainsKey(key) == true)
                        {
                            this.Remove(key);
                        }
                    }
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }
        }
    }

    // 一个检索请求的相关信息
    public class SearchInfo
    {
        public string UID = "";
        public DateTime CreateTime; // 请求的时刻
        public string RequestConnectionID = "";    // 请求者的 connection ID

        public long ReturnStart = 0;    // 结果集中要返回部分的开始位置。从 0 开始计数
        public long ReturnCount = -1;   // 结果集中要返回部分的记录个数。-1 表示尽可能多

        public List<string> TargetIDs = new List<string>(); // 检索目标的 connection id 集合

        public bool IsTimeout()
        {
            if ((DateTime.Now - CreateTime) > new TimeSpan(0, 30, 0))
                return true;
            return false;
        }

        // 标记结束一个检索目标
        // return:
        //      false   尚余某些目标没有完成
        //      true    全部目标都已经完成
        public bool CompleteTarget(string strConnectionID)
        {
            this.TargetIDs.Remove(strConnectionID);
            if (this.TargetIDs.Count == 0)
                return true;
            return false;
        }
    }

    class CleanThread : ThreadBase
    {
        public SearchTable Container = null;

        public CleanThread()
        {
            this.PerTime = 60 * 1000;
        }

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            if (this.Stopped == true)
                return;

            this.Container.Clean();
        }
    }
}
