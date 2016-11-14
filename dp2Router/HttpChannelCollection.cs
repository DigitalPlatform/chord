using DigitalPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dp2Router
{
    /// <summary>
    /// 存储 HTTP 通道的集合
    /// </summary>
    public class HttpChannelCollection : IDisposable
    {
        static int MAX_CHANNELS = 1000;

        List<HttpChannel> _channels = new List<HttpChannel>();

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
                foreach (HttpChannel channel in _channels)
                {
                    channel.Close();
                }
            }
            catch(Exception ex)
            {
                ServerInfo.WriteErrorLog("HttpChannelColleciont Clear() 出现异常: " + ExceptionUtil.GetExceptionText(ex));
            }
            finally
            {
                _channels.Clear();
            }
        }

        public void Add(HttpChannel channel)
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

        public HttpChannel Add(TcpClient tcpClient)
        {
            HttpChannel channel = new HttpChannel();
            channel.TcpClient = tcpClient;
            channel.LastTime = DateTime.Now;
            this.Add(channel);
            return channel;
        }

        public void Remove(HttpChannel channel)
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

        // parameters:
        //      delta   休眠多少时间以上的要清除
        public void CleanIdleChannels(TimeSpan delta)
        {
            List<HttpChannel> delete_channels = new List<HttpChannel>();
            _lock.EnterReadLock();
            try
            {
                DateTime now = DateTime.Now;
                foreach(HttpChannel channel in _channels)
                {
                    if (now - channel.LastTime > delta)
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
                    foreach (HttpChannel channel in delete_channels)
                    {
                        _channels.Remove(channel);
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

                foreach(HttpChannel channel in delete_channels)
                {
                    channel.Close();
                }
            }
        }
    }

    // 一个 HTTP 通讯通道
    public class HttpChannel
    {
        public TcpClient TcpClient { get; set; }
        public DateTime LastTime { get; set; }

        public void Close()
        {
            if (TcpClient != null)
            {
                TcpClient.Close();
                TcpClient = null;
            }
        }

        public void Touch()
        {
            LastTime = DateTime.Now;
        }
    }
}
