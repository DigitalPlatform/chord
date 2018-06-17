using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DigitalPlatform.Z3950.Server
{
    /// <summary>
    /// 存储管理 Z39.50 通道的集合
    /// </summary>
    public class ZServerChannelCollection : IDisposable
    {
        static int MAX_CHANNELS = 1000;

        List<ZServerChannel> _channels = new List<ZServerChannel>();

        ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public void Dispose()
        {
            this.Clear();
        }

        public int Count
        {
            get
            {
                return _channels.Count;
            }
        }

        public void Clear()
        {
            try
            {
                foreach (ZServerChannel channel in _channels)
                {
                    channel.Close();
                }
            }
            catch (Exception ex)
            {
                // ServerInfo.WriteErrorLog("HttpChannelColleciont Clear() 出现异常: " + ExceptionUtil.GetExceptionText(ex));
            }
            finally
            {
                _channels.Clear();
            }
        }

        public void Add(ZServerChannel channel)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_channels.Count >= MAX_CHANNELS)
                    throw new Exception("HttpChannelCollection 配额超出。请稍后重试操作");

                _channels.Add(channel);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public ZServerChannel Add(TcpClient tcpClient)
        {
            ZServerChannel channel = new ZServerChannel();
            channel.TcpClient = tcpClient;
            channel.LastTime = DateTime.Now;
            this.Add(channel);
            return channel;
        }

        public void Remove(ZServerChannel channel)
        {
            _lock.EnterWriteLock();
            try
            {
                _channels.Remove(channel);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        static TimeSpan Max(TimeSpan delta1, TimeSpan delta2)
        {
            if (delta1 >= delta2)
                return delta1;
            return delta2;
        }

        // parameters:
        //      delta   休眠多少时间以上的要清除
        public void CleanIdleChannels(TimeSpan delta)
        {
            List<ZServerChannel> delete_channels = new List<ZServerChannel>();
            _lock.EnterReadLock();
            try
            {
                DateTime now = DateTime.Now;
                foreach (ZServerChannel channel in _channels)
                {
                    TimeSpan current = delta;
                    if (channel.Timeout != TimeSpan.MinValue)
                        current = Max(delta, channel.Timeout);
                    if (now - channel.LastTime > current)
                        delete_channels.Add(channel);
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            if (delete_channels.Count > 0)
            {
                _lock.EnterWriteLock();
                try
                {
                    foreach (ZServerChannel channel in delete_channels)
                    {
                        _channels.Remove(channel);
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

                foreach (ZServerChannel channel in delete_channels)
                {
                    channel.Close();    // TODO: 如何让外界挂接动作
                }
            }
        }
    }

    // 一个 Z39.50 通讯通道
    public class ZServerChannel
    {
        public event EventHandler Closed = null;

        public TcpClient TcpClient { get; set; }
        public DateTime LastTime { get; set; }

        // 2017/3/6
        public TimeSpan Timeout { get; set; }

        internal ChannelProperty _property { get; set; }  // Initialize 成功时，才 new 这个成员，以节省空间

        public void Close()
        {
            if (TcpClient != null)
            {
                TcpClient.Close();
                TcpClient = null;
            }
            TriggerClosed();
        }

        void TriggerClosed()
        {
            var func = this.Closed;
            if (func != null)
                func(this, new EventArgs());
        }

        public void Touch()
        {
            this.LastTime = DateTime.Now;
        }

        public void Touch(TimeSpan timeout)
        {
            this.LastTime = DateTime.Now;
            this.Timeout = timeout;
        }

        public ChannelProperty SetProperty()
        {
            if (this._property != null)
                return this._property;
            this._property = new ChannelProperty();
            return this._property;
        }
    }

}
