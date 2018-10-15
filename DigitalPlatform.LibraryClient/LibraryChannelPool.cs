using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DigitalPlatform.LibraryClient
{
    // 
    /// <summary>
    /// 通道池
    /// </summary>
    public class LibraryChannelPool
    {
        List<LibraryChannelWrapper> _usedList = new List<LibraryChannelWrapper>();
        List<LibraryChannelWrapper> _freeList = new List<LibraryChannelWrapper>();

        /// <summary>
        /// 登录前事件
        /// </summary>
        public event BeforeLoginEventHandle BeforeLogin;

        /// <summary>
        /// 登录后事件
        /// </summary>
        public event AfterLoginEventHandle AfterLogin;

        /// <summary>
        /// 最多通道数
        /// </summary>
        public int MaxCount = 50;

        internal ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        internal static int m_nLockTimeout = 5000;	// 5000=5秒

        public LibraryChannelPool()
        {

        }

        public LibraryChannelPool(int max_count)
        {
            this.MaxCount = max_count;
        }

        // 
        /// <summary>
        /// 征用一个通道
        /// </summary>
        /// <param name="strUrl">服务器 URL</param>
        /// <param name="strUserName">用户名</param>
        /// <param name="strLang">语言代码。如果为空，表示不在意通道的语言代码</param>
        /// <returns>返回通道对象</returns>
        public LibraryChannel GetChannel(string strUrl,
            string strUserName,
            string strLang = "")
        {
            LibraryChannelWrapper wrapper = null;

            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new LockException("锁定尝试中超时");
            try
            {
                wrapper = this._findFreeChannel(strUrl, strUserName, strLang);

                if (wrapper != null)
                {
                    LibraryChannel result = wrapper.Channel;
                    return result;
                }

                if (this._usedList.Count + this._freeList.Count >= MaxCount)
                {
                    // 清理不用的通道
                    int nDeleteCount = _cleanFreeChannel(false);
                    if (nDeleteCount == 0)
                    {
                        // 全部都在使用
                        throw new Exception("通道池已满，请稍后重试获取通道");
                    }
                }

                // 如果没有找到
                LibraryChannel inner_channel = new LibraryChannel();
                inner_channel.Url = strUrl;
                inner_channel.UserName = strUserName;
                inner_channel.Lang = strLang;
                inner_channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                inner_channel.BeforeLogin += new BeforeLoginEventHandle(channel_BeforeLogin);

                inner_channel.AfterLogin -= inner_channel_AfterLogin;
                inner_channel.AfterLogin += inner_channel_AfterLogin;

                wrapper = new LibraryChannelWrapper();
                wrapper.Channel = inner_channel;
                // wrapper.InUsing = true;

                this._usedList.Add(wrapper);
                return inner_channel;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        void inner_channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            if (this.AfterLogin != null)
                this.AfterLogin(sender, e);
        }

        void channel_BeforeLogin(object sender,
    BeforeLoginEventArgs e)
        {
            if (this.BeforeLogin != null)
                this.BeforeLogin(sender, e);
        }

        // 获得正在使用中的通道数量
        // exception:
        //      可能会抛出异常
        public int GetUsingCount()
        {
            return this._usedList.Count;
#if NO
            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new LockException("锁定尝试中超时");
            try
            {
                int count = 0;
                foreach (LibraryChannelWrapper wrapper in this._usedList)
                {
                    if (wrapper.InUsing == true)
                        count++;
                }

                return count;
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }
#endif
        }

        // 查找指定 URL 的 闲置状态的 LibraryChannel 对象
        LibraryChannelWrapper _findFreeChannel(string strUrl,
            string strUserName,
            string strLang
            // bool bAutoSetUsing
            )
        {
            foreach (LibraryChannelWrapper wrapper in this._freeList)
            {
                if (wrapper.Channel.Url == strUrl
                    && (string.IsNullOrEmpty(wrapper.Channel.UserName) == true
                    || wrapper.Channel.UserName == strUserName)
                    && (string.IsNullOrEmpty(strLang) == true
                    || wrapper.Channel.Lang == strLang)
                    )
                {
                    //if (bAutoSetUsing == true)
                    //    wrapper.InUsing = true;
                    return MoveToUsedList(wrapper);
                }
            }

            return null;
        }

        LibraryChannelWrapper MoveToUsedList(LibraryChannelWrapper wrapper)
        {
            this._freeList.Remove(wrapper);
            this._usedList.Add(wrapper);
            return wrapper;
        }

        LibraryChannelWrapper MoveToFreeList(LibraryChannelWrapper wrapper)
        {
            this._usedList.Remove(wrapper);
            this._freeList.Add(wrapper);
            return wrapper;
        }

        // 查找指定URL的LibraryChannel对象
        LibraryChannelWrapper _findUsedChannel(LibraryChannel inner_channel)
        {
            foreach (LibraryChannelWrapper channel in this._usedList)
            {
                if (channel.Channel == inner_channel)
                {
                    return channel;
                }
            }

            return null;
        }

        // 
        /// <summary>
        /// 归还一个通道
        /// </summary>
        /// <param name="channel">通道对象</param>
        public void ReturnChannel(LibraryChannel channel)
        {
            LibraryChannelWrapper wrapper = null;
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new LockException("锁定尝试中超时");
            try
            {
                wrapper = _findUsedChannel(channel);
                if (wrapper != null)
                {
                    // wrapper.InUsing = false;
                    MoveToFreeList(wrapper);
                }
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }

            // testing
            // CleanChannel();
        }

        public int CleanChannel(string strUserName = "")
        {
            return _cleanFreeChannel(true, strUserName);
        }

        // 清理处在未使用状态的通道
        // parameters:
        //      strUserName 希望清除用户名为此值的全部通道。如果本参数值为空，则表示清除全部通道
        // return:
        //      清理掉的通道数目
        int _cleanFreeChannel(bool bLock, string strUserName = "")
        {
            List<LibraryChannelWrapper> deletes = new List<LibraryChannelWrapper>();

            if (bLock == true)
            {
                if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                    throw new LockException("锁定尝试中超时");
            }
            try
            {
                for (int i = 0; i < this._freeList.Count; i++)
                {
                    LibraryChannelWrapper wrapper = this._freeList[i];
                    if (string.IsNullOrEmpty(strUserName) == true || wrapper.Channel.UserName == strUserName)
                    {
                        this._freeList.RemoveAt(i);
                        i--;
                        deletes.Add(wrapper);
                    }
                }
            }
            finally
            {
                if (bLock == true)
                    this.m_lock.ExitWriteLock();
            }

            foreach (LibraryChannelWrapper wrapper in deletes)
            {
                wrapper.Channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                wrapper.Channel.AfterLogin -= inner_channel_AfterLogin;
                wrapper.Channel.Close();
            }

            return deletes.Count;
        }

        /// <summary>
        /// 关闭所有通道，清除集合
        /// </summary>
        public void Clear()
        {
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new LockException("锁定尝试中超时");
            try
            {
                foreach (LibraryChannelWrapper wrapper in this._freeList)
                {
                    wrapper.Channel.Close();
                }
                this._freeList.Clear();

                foreach (LibraryChannelWrapper wrapper in this._usedList)
                {
                    wrapper.Channel.Close();
                }
                this._usedList.Clear();
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        public void Close()
        {
            this.Clear();
        }
    }

    /// <summary>
    /// 通道包装对象
    /// </summary>
    public class LibraryChannelWrapper
    {
        // 
        /// <summary>
        /// 通道是否正在使用中
        /// </summary>
        // public bool InUsing = false;

        /// <summary>
        /// 通道对象
        /// </summary>
        public LibraryChannel Channel = null;
    }

    /// <summary>
    /// 获取通道的事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetChannelEventHandler(object sender,
    GetChannelEventArgs e);

    /// <summary>
    /// 获取通道事件的参数
    /// </summary>
    public class GetChannelEventArgs : EventArgs
    {
        public bool BeginLoop = false;  // [in]
        public LibraryChannel Channel = null;   // [out]
        public string ErrorInfo = "";   // [out]
    }

    /// <summary>
    /// 归还通道的事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ReturnChannelEventHandler(object sender,
    ReturnChannelEventArgs e);

    /// <summary>
    /// 归还通道事件的参数
    /// </summary>
    public class ReturnChannelEventArgs : EventArgs
    {
        public LibraryChannel Channel = null;   // [in]
        public bool EndLoop = false;    // [in]
    }

    public class LockException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="error"></param>
        /// <param name="strText"></param>
        public LockException(string strText)
            : base(strText)
        {
        }
    }
}
