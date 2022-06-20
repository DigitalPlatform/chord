using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.SIP.Server;

namespace dp2Capo
{
    /// <summary>
    /// 登录信息缓存机制。避免高密度的登录操作过度耗费 CPU 资源
    /// </summary>
    public class LoginCache
    {
        Hashtable _table = new Hashtable();

        // 储存信息
        public void Set(SipChannel channel,
            string userName,
            string password,
            string style)
        {
            lock (_table.SyncRoot)
            {
                // 限制 hashtable 最大元素数
                if (_table.Count > 10000)
                    _table.Clear();

                _table[channel.GetHashCode()] = new LoginCacheItem
                {
                    ChannelCode = channel.GetHashCode(),
                    UserName = userName,
                    Password = password,
                    Style = style,
                };
            }
        }

        // 清除
        public void Clear(SipChannel channel)
        {
            lock (_table.SyncRoot)
            {
                _table.Remove(channel.GetHashCode());
            }
        }

        // 是否包含?
        public bool Contains(SipChannel channel,
            string userName,
            string password,
            string style)
        {
            var code = channel.GetHashCode();
            lock (_table.SyncRoot)
            {
                var item = _table[code] as LoginCacheItem;
                if (item == null)
                    return false;
                if (item.UserName != userName
                    || item.Password != password
                    || item.Style != style)
                {
                    _table.Remove(code);
                    return false;
                }

                item.Touch();
                return true;
            }
        }

        // 清除衰老的事项
        public void CleanOld(TimeSpan length)
        {
            DateTime now = DateTime.Now;
            lock (_table.SyncRoot)
            {
                List<int> delete_keys = new List<int>();
                foreach(int key in _table.Keys)
                {
                    var item = _table[key] as LoginCacheItem;
                    if (now - item.CreatedTime > length)
                        delete_keys.Add(key);
                }

                foreach(int key in delete_keys)
                {
                    _table.Remove(key);
                }
            }
        }
    }

    class LoginCacheItem
    {
        public int ChannelCode { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Style { get; set; }

        public DateTime CreatedTime { get; set; }
        public DateTime LastTime { get; set; }

        public LoginCacheItem()
        {
            CreatedTime = DateTime.Now;
            LastTime = CreatedTime;
        }

        public void Touch()
        {
            LastTime = DateTime.Now;
        }
    }
}
