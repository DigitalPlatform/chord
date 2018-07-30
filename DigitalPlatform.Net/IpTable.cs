using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DigitalPlatform.Net
{
    /// <summary>
    /// 跟踪和限制前端每个 IP 连接数量的工具类
    /// </summary>
    public class IpTable
    {
        ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        // static int m_nLockTimeout = 5000;	// 5000=5秒

        // IpTable 中允许存储的 IpEntry 最大个数
        int _nMaxEntryCount = 10000;

        // 判定攻击行为的两个参数：
        int _maxTotalConnectRequest = 1000; // (同一 IP)一段时间内累计的 Connect 请求个数
        TimeSpan _period = TimeSpan.FromSeconds(10);    // 时间段长度

        /// <summary>
        /// 每个(来自前端的) IP 最多允许访问服务器的前端机器数量
        /// </summary>
        public int MaxClientsPerIp = 1000; // -1 表示不限制

        Dictionary<string, IpEntry> _ipTable = new Dictionary<string, IpEntry>();   // IP -- 信息 对照表

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _ipTable.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // 查找一个 IpEntry
        public IpEntry FindIpEntry(string ip)
        {
            // TODO: localhost 注意做归一化处理
            _lock.EnterReadLock();
            try
            {
                if (_ipTable.TryGetValue(ip, out IpEntry value) == false)
                    return null;
                return value;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool RemoveIpEntry(string ip, bool condition)
        {
            if (condition)
            {
                IpEntry entry = FindIpEntry(ip);
                if (entry == null)
                    return false;

                if (entry.CanRemove() == false)
                    return false;
            }

            // 然后用写锁定尝试
            _lock.EnterWriteLock();
            try
            {
                return _ipTable.Remove(ip);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // 确保获得一个 IpEntry
        // exception:
        //      可能会抛出异常
        public IpEntry GetIpEntry(string ip)
        {
            // TODO: localhost 注意做归一化处理
            // 先用读锁定尝试一次
            _lock.EnterReadLock();
            try
            {
                if (_ipTable.TryGetValue(ip, out IpEntry value) == false)
                    goto NEXT;
                return value;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            NEXT:
            // 然后用写锁定尝试
            _lock.EnterWriteLock();
            try
            {
                if (_ipTable.TryGetValue(ip, out IpEntry value) == false)
                {
                    if (this._ipTable.Count > _nMaxEntryCount)
                        throw new Exception("IP 表超过条目配额 " + _nMaxEntryCount);

                    IpEntry entry = new IpEntry();
                    _ipTable.Add(ip, entry);
                    return entry;
                }
                return value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // 清除所有黑名单事项
        public void ClearBlackList(TimeSpan delta)
        {
            List<IpEntry> list = new List<IpEntry>();
            _lock.EnterReadLock();
            try
            {
                foreach (var pair in _ipTable.ToList())
                {
                    list.Add(pair.Value);
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            foreach (IpEntry entry in list)
            {
                if (entry.IsBackListExpire(delta))
                    entry.SetInBlackList(false);
            }
        }

        // 检查 IP。
        // exception:
        //      可能会抛出异常
        // return:
        //      null    许可 IP 被使用
        //      其他   禁止 IP 使用。字符串内容为禁止理由
        public string CheckIp(string ip)
        {
            // exception:
            //      可能会抛出异常
            IpEntry entry = GetIpEntry(ip);
            if (entry.IsInBlackList() == true)
                return "IP [" + ip + "] 在黑名单之中";

            // return:
            //      false   总量超过 max
            //      true    总量没有超过 max
            if (entry.CheckTotal(_maxTotalConnectRequest, _period) == false)
            {
                // TODO: 何时从黑名单中自动清除?
                entry.SetInBlackList();
                return "IP [" + ip + "] 短时间 (" + _period.TotalSeconds.ToString() + "秒) 内连接请求数超过极限 (" + _maxTotalConnectRequest + ")，已被加入黑名单";
            }

            long value = entry.IncOnline();
            if (MaxClientsPerIp != -1 && value >= MaxClientsPerIp)
            {
                entry.DecOnline();
                return "IP 地址为 '" + ip + "' 的前端数量超过配额 " + MaxClientsPerIp;
            }

            return null;
        }

        // 释放 IpEntry
        public void FinishIp(string ip)
        {
            IpEntry entry = FindIpEntry(ip);
            if (entry == null)
                return;
            long value = entry.DecOnline();
            if (value == 0)
            {
                // 发出清除 0 值条目的请求。不过也许不该清除，因为后面还要判断 Total。可以改为清除 Total 值小于某个阈值的条目
                RemoveIpEntry(ip, true);
            }
        }

#if NO
        // 增量 IP 统计数字
        // 如果 IP 事项总数超过限额，会抛出异常
        // parameters:
        //      strIP   前端机器的 IP 地址。还用于辅助判断是否超过 MaxClients。localhost 是不计算在内的
        long _incIpCount(string strIP, int nDelta)
        {
            // this.MaxClients = 0;    // test
            if (nDelta != 0 && nDelta != 1 && nDelta != -1)
                throw new ArgumentException("nDelta 参数值应为 0 -1 1 之一");

            IpEntry entry = GetIpEntry(strIP);

            long oldOnline = 0;
            if (nDelta == 1)
            {
                oldOnline = entry.IncOnline();
                if (oldOnline >= MaxClientsPerIp)
                {
                    entry.DecOnline();
                    throw new Exception("IP 地址为 '" + strIP + "' 的前端数量超过配额 " + MaxClientsPerIp);
                }

                return oldOnline;
            }
            else if (nDelta == -1)
            {
                oldOnline = entry.DecOnline();
                if (oldOnline == 1)
                {
                    // 发出清除 0 值条目的请求。不过也许不该清除，因为后面还要判断 Total。可以改为清除 Total 值小于某个阈值的条目
                    RemoveIpEntry(strIP);
                }

                return oldOnline;
            }
            else
            {
                Debug.Assert(nDelta == 0, "");
            }

            return entry.OnlineCount;
#if NO
            long v = 0;
            if (this._ipTable.ContainsKey(strIP) == true)
                v = (long)this._ipTable[strIP];
            else
            {
                if (this.Count > _nMaxCount
                    && v + nDelta != 0)
                    throw new OutofSessionException("IP 条目数量超过 " + _nMaxCount.ToString());

                // 判断前端机器台数是否超过限制数额 2014/8/23
                if (this.MaxClients != -1
                    && IsLocalhost(strIP) == false
                    && this.GetClientIpAmount() >= this.MaxClients
                    && v + nDelta != 0)
                    throw new OutofClientsException("前端机器数量已经达到 " + this.GetClientIpAmount().ToString() + " 个 ( 现有IP: " + StringUtil.MakePathList(GetIpList(), ", ") + " 试图申请的IP: " + strIP + ")。请先释放出通道然后重新访问");

            }

            if (v + nDelta == 0)
                this._ipTable.Remove(strIP); // 及时移走计数器为 0 的条目，避免 hashtable 尺寸太大
            else
                this._ipTable[strIP] = v + nDelta;

            return v;   // 返回增量前的数字
#endif
        }

#endif

    }

    /// <summary>
    /// 一个 IP 信息事项
    /// </summary>
    public class IpEntry
    {
        private long _onlineCount;

        // 一段时间内累计的连接请求个数
        private long _totalConnectCount;
        // 最近一次累计的开始时间
        private DateTime _totalStartTime = DateTime.Now;

        private AttackInfo _attackInfo = null;

        // 当前在线的数量
        public long OnlineCount { get => _onlineCount; set => _onlineCount = value; }

        // 发生过 Connect 的总次数
        public long TotalConnectCount { get => _totalConnectCount; set => _totalConnectCount = value; }

        public long IncOnline()
        {
            return Interlocked.Increment(ref _onlineCount);
        }

        public long DecOnline()
        {
            return Interlocked.Decrement(ref _onlineCount);
        }

        public long IncTotal()
        {
            return Interlocked.Increment(ref _totalConnectCount);
        }

        public long DecTotal()
        {
            return Interlocked.Decrement(ref _totalConnectCount);
        }

        // 检查条目是否处于黑名单内
        public bool IsInBlackList()
        {
            if (this._attackInfo != null)
                return true;
            return false;
        }

        // 判断黑名单是否过期
        public bool IsBackListExpire(TimeSpan delta)
        {
            if (this._attackInfo == null)
                return false;
            return this._attackInfo.IsExpired(delta);
        }

        public void SetInBlackList(bool set = true)
        {
            if (set)
            {
                if (this._attackInfo == null)
                    this._attackInfo = new AttackInfo();

                this._attackInfo.Memo();
            }
            else
            {
                if (this._attackInfo != null)
                    this._attackInfo = null;
            }
        }

        // 如果累计开始时间超过指定长度，则清除 total 值
        public void TryClearTotal(TimeSpan delta)
        {
            TimeSpan length = DateTime.Now - _totalStartTime;

            //Debug.WriteLine("length=" + length.ToString() + ", delta=" + delta.ToString());

            if (length > delta)
                ClearTotal();
        }

        public void ClearTotal()
        {
            _totalStartTime = DateTime.Now;
            _totalConnectCount = 0;
            //Debug.WriteLine("Clear() _totalConnectCount");
        }

        // 检查总量是否超出
        // 注意返回前，_totalConnectCount 已经被加 1
        // return:
        //      false   总量超过 max
        //      true    总量没有超过 max。
        public bool CheckTotal(int max, TimeSpan delta)
        {
            TryClearTotal(delta);

            IncTotal();
            //Debug.WriteLine("_totalConnectCount=" + _totalConnectCount);
            if (_totalConnectCount > max)
                return false;
            return true;
        }

        // 是否允许清理？
        public bool CanRemove()
        {
            if (this._totalConnectCount <= 1)
                return true;
            return false;
        }
    }

    // 攻击信息
    public class AttackInfo
    {
        public DateTime AttackTime = DateTime.Now; // 最后一次攻击的时间
        public long AttackCount = 0;    // 一共发生的攻击次数

        // 记载一次攻击
        public void Memo()
        {
            this.AttackTime = DateTime.Now;
            this.AttackCount++;
        }

        // parameters:
        //      delta   从最后一次攻击的时间计算，多长以后清除攻击记忆
        public bool IsExpired(TimeSpan delta)
        {
            if (DateTime.Now > this.AttackTime + delta)
                return true;
            return false;
        }
    }
}
