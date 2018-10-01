using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalPlatform.Common
{
    /// <summary>
    /// 紧凑型日志。用于可能泛滥的日志场合
    /// </summary>
    public class CompactLog
    {
        internal ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        Dictionary<string, CompactEntry> _table = new Dictionary<string, CompactEntry>();

        public const int MaxEntryCount = 1000;

        // 添加一条日志
        public async Task<string> Add(string fmt, object[] args)
        {
            return await Task.Run(() => { return AddEntry(fmt, args); }).ConfigureAwait(false);
        }

        string AddEntry(string fmt, object[] args)
        {
            CompactEntry entry = null;
            _lock.EnterWriteLock();
            try
            {
                if (_table.Count >= MaxEntryCount)
                    return "Entry 数超过 " + MaxEntryCount + "，没有记入日志";

                if (_table.ContainsKey(fmt) == false)
                {
                    entry = new CompactEntry { Key = fmt, StartTime = DateTime.Now };
                    _table.Add(fmt, entry);
                }
                else
                    entry = (CompactEntry)_table[fmt];
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            lock (entry)
            {
                entry.AddData(args);
            }

            return null;
        }

        // 把累积的条目一次性写入日志文件
        public void WriteToLog(delegate_writeLog func_writeLog)
        {
            List<string> keys = new List<string>();
            _lock.EnterReadLock();
            try
            {
                foreach (string key in _table.Keys)
                {
                    CompactEntry entry = _table[key];
                    lock (entry)
                    {
                        entry.WriteToLog(func_writeLog);
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            _lock.EnterWriteLock();
            try
            {
                foreach (string key in keys)
                {
                    _table.Remove(key);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    // 条目
    public class CompactEntry
    {
        // 格式名
        public string Key { get; set; }
        // 第一笔数据的时间
        public DateTime StartTime { get; set; }
        // 数据总数
        public long TotalCount { get; set; }

        public List<CompactData> Datas { get; set; }

        public const int MaxDataCount = 10;

        public void AddData(object[] args)
        {
            if (Datas == null)
                Datas = new List<CompactData>();

            if (this.Datas.Count < MaxDataCount)
            {
                CompactData data = new CompactData { Args = args };
                Datas.Add(data);
            }
            TotalCount++;
        }

        public void WriteToLog(delegate_writeLog func_writeLog,
            string style = "display")
        {
            if (this.Datas.Count == 0)
                return;

            if (style == "display")
            {
                // 适合观看的格式
                StringBuilder text = new StringBuilder();
                text.Append("(" + this.TotalCount + " 项) 压缩日志 ");
                text.Append(this.Key + "\r\n");
                int i = 0;
                foreach (CompactData data in this.Datas)
                {
                    text.Append((i + 1).ToString() + ") " + (new DateTime(this.StartTime.Ticks + data.Ticks)).ToString("HH:mm:ss") + ":");
                    text.Append(string.Format(this.Key, data.Args) + "\r\n");
                    i++;
                }
                if (i < this.TotalCount)
                    text.Append("... (余下 " + (this.TotalCount - i) + " 项被略去)");
                func_writeLog(text.ToString());
            }
            else
            {
                // 原始格式
                StringBuilder text = new StringBuilder();
                text.Append("(" + this.TotalCount + " 项)");
                text.Append(this.Key + "\r\n");
                int i = 0;
                foreach (CompactData data in this.Datas)
                {
                    text.Append((i + 1).ToString() + ") " + data.GetString(this.StartTime) + "\r\n");
                    i++;
                }
                if (i < this.TotalCount)
                    text.Append("... (余下 " + (this.TotalCount - i) + " 项被略去)");
                func_writeLog(text.ToString());
            }
            this.Datas.Clear(); // 写入日志后，清除内存
            this.TotalCount = 0;
        }
    }

    // 数据
    public class CompactData
    {
        // 相对于开始时间的 Ticks
        public long Ticks { get; set; }

        // 参数值列表
        public object[] Args { get; set; }

        public string GetString(DateTime start)
        {
            StringBuilder text = new StringBuilder();
            text.Append((new DateTime(start.Ticks + this.Ticks)).ToShortTimeString() + ":");
            int i = 0;
            foreach (object o in Args)
            {
                if (i > 0)
                    text.Append(",");
                text.Append(o.ToString());
                i++;
            }

            return text.ToString();
        }
    }

    public delegate void delegate_writeLog(string text);
}
