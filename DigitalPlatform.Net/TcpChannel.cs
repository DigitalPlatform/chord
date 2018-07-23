using DigitalPlatform.Common;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DigitalPlatform.Net
{
    /// <summary>
    /// 存储管理 TCP 通道的集合
    /// </summary>
    public class TcpChannelCollection : IDisposable
    {
        static int MAX_CHANNELS = 10000;

        List<TcpChannel> _channels = new List<TcpChannel>();

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
                foreach (TcpChannel channel in _channels)
                {
                    channel.Close();
                }
            }
            catch (Exception ex)
            {
                LibraryManager.Log?.Error("ZServerChannelColleciont Clear() 出现异常: " + ExceptionUtil.GetExceptionText(ex));
            }
            finally
            {
                _channels.Clear();
            }
        }

        public void Add(TcpChannel channel)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_channels.Count >= MAX_CHANNELS)
                    throw new Exception("ZServerChannelCollection 配额超出。请稍后重试操作");

                _channels.Add(channel);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public delegate TcpChannel Delegate_newTcpChannel();

        public TcpChannel Add(TcpClient tcpClient,
            Delegate_newTcpChannel procNewTcpChannel)
        {
            TcpChannel channel = null;

            if (procNewTcpChannel == null)
                channel = new TcpChannel();
            else
                channel = procNewTcpChannel();

            channel.TcpClient = tcpClient;
            channel.LastTime = DateTime.Now;
            this.Add(channel);
            return channel;
        }

        public void Remove(TcpChannel channel)
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
            List<TcpChannel> delete_channels = new List<TcpChannel>();
            _lock.EnterReadLock();
            try
            {
                DateTime now = DateTime.Now;
                foreach (TcpChannel channel in _channels)
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
                    foreach (TcpChannel channel in delete_channels)
                    {
                        _channels.Remove(channel);
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

                foreach (TcpChannel channel in delete_channels)
                {
                    channel.Close();    // TODO: 如何让外界挂接动作
                }
            }
        }
    }

    // 一个 TCP 通讯通道
    public class TcpChannel
    {
        public event EventHandler Closed = null;

        public TcpClient TcpClient { get; set; }
        public DateTime LastTime { get; set; }

        // 2017/3/6
        public TimeSpan Timeout { get; set; }

        private ChannelProperty _property = null;
        public ChannelProperty Property // Initialize 成功时，才 new 这个成员，以节省空间
        {
            get
            {
                return _property;
            }
            set
            {
                _property = value;
            }
        }

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

        // 确保获得一个 ChannelProperty 对象
        public virtual ChannelProperty GetProperty()
        {
            if (this._property != null)
                return this._property;
            this._property = new ChannelProperty();
            return this._property;
        }
    }

    public class ChannelProperty
    {

    }
}
