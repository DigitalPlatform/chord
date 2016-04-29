using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform.Message;

namespace DigitalPlatform.MessageClient
{
    /// <summary>
    /// MessageConnection 的集合
    /// 对于每一个 strName，集合维持一个连接对象。由于连接对象不怕并发使用，所以集合没有对正在使用的集合提供锁定排斥机制。
    /// </summary>
    public class MessageConnectionCollection : IDisposable
    {
        internal ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        List<MessageConnection> _connections = new List<MessageConnection>();

        public event ConnectionCreatedEventHandler ConnectionCreated = null;
        public event ConnectionClosingEventHandler ConnectionClosing = null;
        public event LoginEventHandler Login = null;
        public event AddMessageEventHandler AddMessage = null;

        // parameters:
        //      strName 连接的名字。如果要针对同一 dp2mserver 使用多根连接，可以用名字区分它们。如果不想区分，可以使用空
        public Task<MessageConnection> GetConnectionAsync(string url,
    string strName,
    bool autoConnect = true)
        {
            MessageConnection connection = null;
            this._lock.EnterUpgradeableReadLock();
            try
            {
                foreach (MessageConnection current_connection in _connections)
                {
                    if (current_connection.ServerUrl == url && current_connection.Name == strName)
                    {
                        connection = current_connection;
                        goto FOUND;
                    }
                }

                connection = new MessageConnection();
                connection.ServerUrl = url;
                connection.Name = strName;
                connection.Container = this;
                this._lock.EnterWriteLock();
                try
                {
                    this._connections.Add(connection);
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }
            finally
            {
                this._lock.ExitUpgradeableReadLock();
            }

            // 触发 Created 事件
            this.TriggerCreated(connection, new ConnectionCreatedEventArgs());

        FOUND:
#if NO
            LoginEventArgs e = new LoginEventArgs();
            e.ServerUrl = url;
            e.Name = strName;
            LoginEventHandler handler = this.Login;
            if (handler != null)
                handler(connection, e); // TODO: 是否在真正连接前再触发?

            if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                throw new Exception(e.ErrorInfo);

            connection.UserName = e.UserName;
            connection.Password = e.Password;
            connection.Parameters = e.Parameters;
#endif

            if (autoConnect && connection.IsConnected == false)
            {
                Task<MessageConnection> task = new Task<MessageConnection>(() =>
                {
                    // TODO: 建议抛出原有 Exception
                    MessageResult result = connection.ConnectAsync().Result;
                    if (result.Value == -1)
                        throw new Exception(result.ErrorInfo);
                    return connection;
                });
                task.Start();
                return task;
            }

            {
                var task = new Task<MessageConnection>(() =>
                {
                    return connection;
                });
                task.Start();
                return task;
            }
        }

#if NO
        // parameters:
        //      strName 连接的名字。如果要针对同一 dp2mserver 使用多根连接，可以用名字区分它们。如果不想区分，可以使用空
        //      autoConnect 是否自动连接
        //      waitConnecting  是否等待连接完成后再返回?
        public MessageConnection GetConnection(string url,
            string remoteName,
            bool autoConnect = true,
            bool waitConnecting = true)
        {
            MessageConnection connection = null;
            foreach (MessageConnection current_connection in _connections)
            {
                if (current_connection.ServerUrl == url && current_connection.Name == remoteName)
                {
                    connection = current_connection;
                    goto FOUND;
                }
            }

            connection = new MessageConnection();
            connection.ServerUrl = url;
            connection.Name = remoteName;
            connection.Container = this;
            this._connections.Add(connection);

        FOUND:
            if (autoConnect && connection.IsConnected == false)
            {
                Task task = connection.ConnectAsync(url);

                if (waitConnecting)
                    task.Wait();
            }

            return connection;
        }
#endif

        public void RemoveConnection(MessageConnection connection)
        {
            // 触发 Closing 事件
            TriggerClosing(connection, new ConnectionClosingEventArgs());
            connection.CloseConnection();

            this._lock.EnterWriteLock();
            try
            {
                this._connections.Remove(connection);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            foreach (MessageConnection connection in this._connections)
            {
                // 触发 Closing 事件
                TriggerClosing(connection, new ConnectionClosingEventArgs());

                connection.CloseConnection();
            }

            this._lock.EnterWriteLock();
            try
            {
                this._connections.Clear();
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            this.Clear();
        }

        // 触发登录事件
        public virtual void TriggerLogin(MessageConnection connection)
        {
            LoginEventHandler handler = this.Login;
            if (handler != null)
            {
                LoginEventArgs e = new LoginEventArgs();
                // 注: 在触发事件以前, MessageConnection 对象的 ServerUrl 和 Name 成员已经准备好了，可以利用
                //e.ServerUrl = connection.ServerUrl;
                //e.Name = connection.Name;
                handler(connection, e);
                if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                    throw new Exception(e.ErrorInfo);

                connection.UserName = e.UserName;
                connection.Password = e.Password;
                connection.Parameters = e.Parameters;
            }
        }

        // 触发消息通知事件
        public virtual void TriggerAddMessage(MessageConnection connection,
            AddMessageEventArgs e)
        {
            AddMessageEventHandler handler = this.AddMessage;
            if (handler != null)
            {
                handler(connection, e);
            }
        }

        public virtual void TriggerCreated(MessageConnection connection,
            ConnectionCreatedEventArgs e)
        {
            ConnectionCreatedEventHandler handler = this.ConnectionCreated;
            if (handler != null)
            {
                handler(connection, e);
            }
        }

        public virtual void TriggerClosing(MessageConnection connection,
    ConnectionClosingEventArgs e)
        {
            ConnectionClosingEventHandler handler = this.ConnectionClosing;
            if (handler != null)
            {
                handler(connection, e);
            }
        }
    }

    /// <summary>
    /// 通道创建成功事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ConnectionCreatedEventHandler(object sender,
    ConnectionCreatedEventArgs e);

    /// <summary>
    /// 通道创建成功事件的参数
    /// </summary>
    public class ConnectionCreatedEventArgs : EventArgs
    {
    }

    /// <summary>
    /// 通道即将关闭事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ConnectionClosingEventHandler(object sender,
    ConnectionClosingEventArgs e);

    /// <summary>
    /// 通道即将关闭事件的参数
    /// </summary>
    public class ConnectionClosingEventArgs : EventArgs
    {
    }

    /// <summary>
    /// 登录事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void LoginEventHandler(object sender,
    LoginEventArgs e);

    /// <summary>
    /// 登录事件的参数
    /// </summary>
    public class LoginEventArgs : EventArgs
    {
        // 注: 在触发事件以前, MessageConnection 对象的 ServerUrl 和 Name 成员已经准备好了，可以利用
        //public string ServerUrl = "";   // [in]
        //public string Name = "";        // [in]

        public string UserName = "";    // [out]
        public string Password = "";    // [out]
        public string Parameters = "";  // [out]

        public string ErrorInfo = "";   // [out] 出错信息。表示无法进行登录
    }

    /// <summary>
    /// 消息通知事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void AddMessageEventHandler(object sender,
        AddMessageEventArgs e);

    /// <summary>
    /// 消息通知事件的参数
    /// </summary>
    public class AddMessageEventArgs : EventArgs
    {
        public string Action = "";
        public List<MessageRecord> Records = null;
    }
}
