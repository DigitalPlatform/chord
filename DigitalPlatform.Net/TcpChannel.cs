using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform.Common;

namespace DigitalPlatform.Net
{
    /// <summary>
    /// 存储管理 TCP 服务器的 TCP 通道的集合
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
            _lock.EnterWriteLock();
            try
            {
                foreach (TcpChannel channel in _channels)
                {
                    channel.Close();
                }
                _channels.Clear();
            }
            catch (Exception ex)
            {
                LibraryManager.Log?.Error("TcpChannelCollection Clear() 出现异常: " + ExceptionUtil.GetDebugText(ex));
            }
            finally
            {
                _lock.ExitWriteLock();
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

        // 清理休眠的通道
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
                        current = channel.Timeout;  //  Max(delta, channel.Timeout);
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

        // return:
        //      false   不希望被清理
        //      true    希望被清理
        public delegate bool Delegate_needClean(TcpChannel channel);

        // 按需清理通道
        public int Clean(Delegate_needClean procNeedClear)
        {
            List<TcpChannel> delete_channels = new List<TcpChannel>();
            _lock.EnterReadLock();
            try
            {
                DateTime now = DateTime.Now;
                foreach (TcpChannel channel in _channels)
                {
                    if (procNeedClear(channel) == true)
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
                    channel.Close();
                }
            }

            return delete_channels.Count;
        }

    }

    // 一个 TCP (服务器端)通讯通道
    public class TcpChannel
    {
        public object Tag { get; set; }

        public event EventHandler Closed = null;

        public TcpClient TcpClient { get; set; }
        public DateTime LastTime { get; set; }

        TimeSpan _timeout = TimeSpan.MinValue;
        public TimeSpan Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

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

        // 通讯包是否完整到达?
        // return:
        //      0   通讯包不完整
        //      其他  通讯包完整到达，返回值表示通讯包 byte 数。也就是说后面要从 package 前部开始移走这么多 byte
        public delegate Tuple<int, byte> Delegate_isComplete(byte[] package,
                        int start, int length);

        // 接收通讯包
        // 本函数支持 Pipeline 方式。
        // parameters:
        //      cache   用来支持 Pipeline 方式，把多于一个通讯包的 bytes 部分，存储起来，下次先处理这部分内容
        //              如果为 null，表示不支持 Pipeline 方式
        //      nMaxLength  读入等待处理的 bytes 极限数字。超过了这个，还没有找到结束符，就会抛出异常。意在防范攻击。-1 表示不限制
        public static async Task<RecvResult> SimpleRecvTcpPackage(TcpClient client,
            List<byte> cache,
            Delegate_isComplete procIsComplete,
            int nMaxLength = 4096)
        {
            // string strError = "";
            RecvResult result = new RecvResult();

            int recieved = 0;   // 累计读取的 byte 数
            int current = 0;    // 本次读取的 byte 数
            // bool bInitialLen = false;

            Debug.Assert(client != null, "client为空");

            int CHUNK_SIZE = 4096;

            result.Package = new byte[CHUNK_SIZE];
            recieved = 0;
            result.Length = CHUNK_SIZE;

            // 优先从 cache 中复制数据过来进行处理
            if (cache != null && cache.Count > 0)
            {
                result.Package = EnlargeBuffer(cache.ToArray(), CHUNK_SIZE);
                result.Length = result.Package.Length;
                recieved = result.Length;
                cache.Clear();
            }

            while (recieved < result.Length)
            {
                if (client == null)
                {
                    return new RecvResult
                    {
                        Value = -1,
                        ErrorInfo = "通讯中断",
                        ErrorCode = "abort"
                    };
                }

                try
                {
                    current = await client.GetStream().ReadAsync(result.Package,
                        recieved,
                        result.Package.Length - recieved).ConfigureAwait(false);
                }
                catch (SocketException ex)
                {

                    if (ex.ErrorCode == 10035)
                    {
                        System.Threading.Thread.Sleep(100);
                        continue;
                    }
                    return new RecvResult
                    {
                        Exception = ex,
                        Value = -1,
                        ErrorInfo = "recv出错1: " + ex.Message,
                        // "ConnectionAborted"
                        ErrorCode = ((SocketException)ex).SocketErrorCode.ToString()
                    };
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    if (ex is IOException && ex.InnerException is SocketException)
                    {
                        // "ConnectionAborted"
                        result.ErrorCode = ((SocketException)ex.InnerException).SocketErrorCode.ToString();
                    }
                    result.ErrorInfo = "recv出错2: " + ex.Message;
                    result.Value = -1;
                    return result;
                }

                if (current == 0)
                {
                    return new RecvResult
                    {
                        Value = -1,
                        ErrorCode = "Closed",
                        ErrorInfo = "Closed by remote peer"
                    };
                }

                // 得到包的长度

                if (current >= 1 || recieved >= 1)
                {
                    var ret = procIsComplete(result.Package,
                        0,
                        recieved + current);
                    if (ret.Item1 > 0)
                    {
                        result.Length = ret.Item1;
                        result.Terminator = ret.Item2;
                        // 将结束符后面多出来的部分复制到 cache 中，以便下一次调用处理
                        if (result.Length > recieved + current) // ?? bug
                        {
                            if (cache == null)
                                throw new Exception("当前不支持 Pipeline 方式的请求。发现前端一次性发送了多于一个通讯包");
                            for (int i = result.Length; i < recieved + current; i++)
                            {
                                cache.Add(result.Package[i]);
                            }
                        }
                        break;
                    }
                }

                recieved += current;
                if (recieved >= result.Package.Length)
                {
#if NO
                    // 扩大缓冲区
                    byte[] temp = new byte[result.Package.Length + 4096];
                    Array.Copy(result.Package, 0, temp, 0, nInLen);
                    result.Package = temp;
                    result.Length = result.Package.Length;
#endif
                    if (nMaxLength != -1 && result.Package.Length >= nMaxLength)
                        throw new Exception("接收超过 " + nMaxLength + " bytes 也没有找到通讯包结束符");
                    // 扩大缓冲区
                    result.Package = EnlargeBuffer(result.Package, CHUNK_SIZE);
                    result.Length = result.Package.Length;
                }
            }

            // 最后规整缓冲区尺寸，如果必要的话
            if (result.Package.Length > result.Length)
            {
                byte[] temp = new byte[result.Length];
                Array.Copy(result.Package, 0, temp, 0, result.Length);
                result.Package = temp;
            }

            return result;
        }

        static byte[] EnlargeBuffer(byte[] package, int delta)
        {
            // 扩大缓冲区
            byte[] temp = new byte[package.Length + delta];
            Array.Copy(package, 0, temp, 0, package.Length);
            return temp;
        }

        // 发出请求包
        // return:
        //      -1  出错
        //      0   正确发出
        //      1   发出前，发现流中有未读入的数据
        public static async Task<Result> SimpleSendTcpPackage(
            TcpClient client,
            byte[] baPackage,
            int nLen)
        {
            Result result = new Result();

            if (client == null)
            {
                result.Value = -1;
                result.ErrorInfo = "client尚未初始化。请重新连接";
                return result;
            }

            if (client == null)
            {
                result.Value = -1;
                result.ErrorInfo = "用户中断";
                return result;
            }

            // TODO: 是否要关闭 NetworkStream !!!
            NetworkStream stream = client.GetStream();

            if (stream.DataAvailable == true)
            {
                result.Value = 1;
                result.ErrorInfo = "发送前发现流中有未读的数据";
                return result;
            }

            try
            {
                await stream.WriteAsync(baPackage, 0, nLen).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is IOException && ex.InnerException is SocketException)
                {
                    // "ConnectionAborted"
                    result.ErrorCode = ((SocketException)ex.InnerException).SocketErrorCode.ToString();
                }

                result.Value = -1;
                result.ErrorInfo = "send出错: " + ex.Message;
                // this.CloseSocket();
                return result;
            }

            return result;
        }

    }

    public class ChannelProperty
    {

    }
}
