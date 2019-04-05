﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;

using DigitalPlatform;
using DigitalPlatform.Core;

namespace DigitalPlatform.MessageServer
{
    // TODO: 定期清除已超时的对象
    // 检索请求集合
    // search request UID --> SearchInfo 的查找表。同时也是 SearchInfo 对象的存储机制
    public class SearchTable : Dictionary<string, SearchInfo>, IDisposable
    {
        int _maxSearch = 1000;

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

        // 新增一个 SearchInfo 对象
        // exceptions:
        //      Exception   数量超过配额
        // parameters:
        //      strRequestConnectionID  发起检索一方的 connection id
        public SearchInfo AddSearch(string strRequestConnectionID,
            string strSearchID = "",
            long returnStart = 0,
            long returnCount = -1,
            string serverPushEncoding = "")
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

                if (this.Count >= _maxSearch)
                    throw new Exception("SearchInfo 数量已经超过配额 " + _maxSearch.ToString());

                SearchInfo info = new SearchInfo();
                info.LastTime = DateTime.Now;
                if (string.IsNullOrEmpty(strSearchID) == true)
                    info.UID = Guid.NewGuid().ToString();
                else
                    info.UID = strSearchID;

                info.RequestConnectionID = strRequestConnectionID;
                info.ReturnStart = returnStart;
                info.ReturnCount = returnCount;
                info.ServerPushEncoding = serverPushEncoding;
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
        public SearchInfo GetSearchInfo(string strUID, bool bActivate = true)
        {
            this._lock.EnterReadLock();
            try
            {
                if (this.ContainsKey(strUID) == false)
                    return null;

                return this[strUID].Activate();
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        // 清除已经(超时)失效的对象
        // return:
        //      返回清除操作后集合中剩余的对象总数
        public int Clean()
        {
            // Console.WriteLine("do clean ...");
            int count = 0;

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

                count = this.Count;
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

                count -= keys.Count;
            }

            return count;
        }
    }

    // 一次检索命中的信息
    public class HitInfo
    {
        public long TotalResults = 0;   // 总的命中数量。-1 表示已出错
        public long Recieved = 0;   // 已经收到的数量
    }

    // 一个检索请求的相关信息
    public class SearchInfo
    {
        public string ServerPushEncoding = "";  // dp2mserver 推送消息给前端时候所使用的 encoding。主要是用来避免 .NET 4.0 的前端出现乱码

        public string UID = "";
        public DateTime LastTime; // 请求的时刻
        public string RequestConnectionID = "";    // 请求者的 connection ID

        public long ReturnStart = 0;    // 检索请求中，结果集中要返回部分的开始位置。从 0 开始计数
        public long ReturnCount = -1;   // 检索请求中，结果集中要返回部分的记录个数。-1 表示尽可能多

        private static readonly Object _syncRoot = new Object();
        List<string> _targetIDs = new List<string>(); // 检索目标的 connection id 集合

        Hashtable _targetTable = new Hashtable();   // target id --> HitInfo

        public object Data { get; set; }    // 存储任意数据 2016/11/30

        public void SetTargetIDs(List<string> ids)
        {
            lock (_syncRoot)
            {
                this._targetIDs = ids;
            }
        }

        // 获得 TargetIDs 的复制品。这样就不怕并发修改同时发生
        public List<string> GetSafeTargetIDs()
        {
            List<string> results = new List<string>();
            lock (_syncRoot)
            {
                results.AddRange(this._targetIDs);
            }
            return results;
        }

        public bool IsTimeout()
        {
            if ((DateTime.Now - LastTime) > TimeSpan.FromMinutes(3))    // 3 分钟
                return true;
            return false;
        }

        // 重新设置最近活动时间
        public SearchInfo Activate()
        {
            this.LastTime = DateTime.Now;
            return this;
        }

        // 标记结束一个检索目标
        // return:
        //      false   尚余某些目标没有完成
        //      true    全部目标都已经完成
        bool CompleteTarget(string strConnectionID)
        {
            lock (_syncRoot)
            {
                this._targetIDs.Remove(strConnectionID);
                if (this._targetIDs.Count == 0)
                    return true;
                return false;
            }
        }

        // 标记结束一个检索目标
        // return:
        //      0   尚未结束
        //      1   结束
        //      2   全部结束
        public int CompleteTarget(string strConnectionID, 
            long total_count,
            long this_count)
        {
            if (total_count == -1)
            {
                if (CompleteTarget(strConnectionID) == true)
                    return 2;
                return 0;
            }

            HitInfo info = _targetTable[strConnectionID] as HitInfo;
            if (info == null)
            {
                info = new HitInfo();
                // 2016/6/1 巩固
                info.TotalResults = this.ReturnCount == -1 ? total_count - this.ReturnStart : Math.Min(total_count - this.ReturnStart, this.ReturnCount);
                _targetTable[strConnectionID] = info;
            }
            info.Recieved += this_count;
            if (info.TotalResults <= info.Recieved)
            {
                if (CompleteTarget(strConnectionID) == true)
                    return 2;
                return 1;
            }
            return 0;
        }

        // TODO: 为每个目标单独记载 range、总命中数。这样就可以判断是否全部已经到来
        // ? 中途不清除记载的信息，因为需要用它获得加起来的总命中数

    }

    class CleanThread : ThreadBase
    {
        public SearchTable Container = null;

        public CleanThread()
        {
            this.PerTime = 5 * 60 * 1000;   // 5 分钟
        }

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            if (this.Stopped == true)
                return;

            int count = this.Container.Clean();
            ServerInfo.WriteErrorLog("清除 SearchTable 后剩余的对象数量 " + count.ToString());
        }
    }
}
