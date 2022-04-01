using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using DigitalPlatform.Net;

namespace DigitalPlatform.SIP.Server
{
    public class SipChannel : TcpChannel
    {
        public string InstanceName { get; set; }
        // 登录dp2系统的帐户
        string _userName = null;
        public string UserName
        {
            get
            {
                return _userName;
            }
        }
        public string Password { get; set; }
        // 3M设备工作台号
        public string LocationCode { get; set; }

        // 登录是否成功
        // public bool LoginSucceed { get; set; }

        // 2021/3/4
        // 登录者的机构代码
        public string Institution { get; set; }

        // 2022/3/28
        // 登录者(管辖的)的馆代码列表
        public string LibraryCodeList { get; set; }

        // 2022/3/28
        // 被请求的累计次数
        public int RequestCount { get; set; }

        // 2022/3/29
        public DateTime CreateTime { get; set; }

        // 消息所用编码方式
        Encoding _encoding = null;  // null 表示根本没有初始化这个参数 // Encoding.UTF8;
        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        // 2022/3/4
        public string Style
        {
            get; set;
        }

        // 2022/3/22
        public int MaxChannels
        {
            get; set;
        }

        // 最近的一条响应消息
        public string LastMsg { get; set; }

        // 前端实际上使用的通讯包结束符。这是从请求包中探测到的
        public byte Terminator { get; set; }
#if NO
        public SipChannelProperty EnsureProperty()
        {
            if (this.Property != null)
                return this.Property as SipChannelProperty;
            this.Property = new SipChannelProperty();
            return this.Property as SipChannelProperty;
        }
#endif

        // 获得 username@instance 形态的字符串
        public string GetUserInstanceName()
        {
            return this.UserName + "@" + this.InstanceName;
        }

        public string SetUserName(string userName,
            Hashtable channel_count_table)
        {
            // 空 --> 有值
            if (string.IsNullOrEmpty(this.UserName) == true
                && string.IsNullOrEmpty(userName) == false)
            {
                string old_value = this._userName;
                this._userName = userName;
                var result = IncCount(channel_count_table, this);
                if (result != null)
                    this._userName = old_value;
                return result;
            }
            // 有值 --> 空
            else if (string.IsNullOrEmpty(this.UserName) == false
                && string.IsNullOrEmpty(userName) == true)
            {
                DecCount(channel_count_table, this);
                this._userName = userName;
                return null;
            }
            // 有值 --> 不同的有值
            else if (string.IsNullOrEmpty(this.UserName) == false
                && string.IsNullOrEmpty(userName) == false
                && this.UserName != userName)
            {
                // 先减量
                DecCount(channel_count_table, this);
                string old_value = this._userName;
                this._userName = userName;
                // 再增量
                var result = IncCount(channel_count_table, this);
                if (result != null) // 2022/4/1
                    this._userName = old_value;
                return result;
            }

            return null;
        }

        static string IncCount(Hashtable channel_count_table,
    SipChannel sip_channel)
        {
            string key = MakeKey(sip_channel);
            return IncCount(channel_count_table, key, sip_channel.MaxChannels);
        }

        static void DecCount(Hashtable channel_count_table,
            SipChannel sip_channel)
        {
            string key = MakeKey(sip_channel);
            DecCount(channel_count_table, key);
        }

        static string MakeKey(SipChannel sip_channel)
        {
            return sip_channel.UserName + "@" + sip_channel.InstanceName;
        }

        // parameters:
        //      maxChannels 最大数。如果为 -1 表示不限制
        static string IncCount(Hashtable channel_count_table,
            string key,
            int maxChannels)
        {
            int count = 0;
            lock (channel_count_table.SyncRoot)
            {
                if (channel_count_table.Contains(key))
                    count = (int)channel_count_table[key];
                if (maxChannels != -1 && count + 1 > maxChannels)
                {
                    return $"Login failed. User '{key}' has already use {count} TCP channels(max value is {maxChannels}). 用户 '{key}' 已经使用了 {count} 根 TCP 通道(极限值为 {maxChannels})，登录被拒绝";
                }
                channel_count_table[key] = count + 1;
                return null;
            }
        }

        static void DecCount(Hashtable channel_count_table,
            string key)
        {
            lock (channel_count_table.SyncRoot)
            {
                if (channel_count_table.Contains(key))
                {
                    int count = (int)channel_count_table[key];
                    channel_count_table[key] = count - 1;
                }
            }
        }

    }

    // 每个(SIP 服务器)通道特有的信息
    public class SipChannelProperty : ChannelProperty
    {
        // 是否成功进行了 Initialize()
        // internal bool _bInitialized { get; set; }

        Dictionary<string, object> _parameters = null;

        // return:
        //      null    value 不存在
        //      其他      value 值
        public string GetKeyValue(string strName)
        {
            if (_parameters == null)
                return null;
            if (_parameters.ContainsKey(strName) == false)
                return null;
            return (string)_parameters[strName];
        }

        public void SetKeyValue(string strName, string strValue)
        {
            if (_parameters == null)
                _parameters = new Dictionary<string, object>();
            _parameters[strName] = strValue;
        }

        public object GetKeyObject(string strName)
        {
            if (_parameters == null)
                return null;
            if (_parameters.ContainsKey(strName) == false)
                return null;
            return _parameters[strName];
        }

        public void SetKeyObject(string strName, object value)
        {
            if (_parameters == null)
                _parameters = new Dictionary<string, object>();
            _parameters[strName] = value;
        }
    }

}
