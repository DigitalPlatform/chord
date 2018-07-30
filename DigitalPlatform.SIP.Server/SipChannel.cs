using System;
using System.Collections.Generic;
using System.Text;

using DigitalPlatform.Net;

namespace DigitalPlatform.SIP.Server
{
    public class SipChannel : TcpChannel
    {
        public string InstanceName { get; set; }
        // 登录dp2系统的帐户
        public string UserName { get; set; }
        public string Password { get; set; }
        // 3M设备工作台号
        public string LocationCode { get; set; }

        // 消息所用编码方式
        Encoding _encoding = Encoding.UTF8;
        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
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
