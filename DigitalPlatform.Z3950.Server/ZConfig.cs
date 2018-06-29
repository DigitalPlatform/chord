using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalPlatform.Z3950.Server
{
    // Z39.50 服务器参数配置
    public class ZConfig
    {
        // 匿名登录所用到的内层用户名和密码
        public string AnonymousUserName { get; set; }
        public string AnonymousPassword { get; set; }

    }

    // 每个通道特有的信息
    public class ChannelProperty
    {
        // 是否成功进行了 Initialize()
        internal bool _bInitialized { get; set; }

        long _lPreferredMessageSize = 500 * 1024;

        public long PreferredMessageSize
        {
            get
            {
                return this._lPreferredMessageSize;
            }
            set
            {
                this._lPreferredMessageSize = value;
            }
        }

        long _lExceptionalRecordSize = 500 * 1024;

        public long ExceptionalRecordSize
        {
            get
            {
                return this._lExceptionalRecordSize;
            }
            set
            {
                this._lExceptionalRecordSize = value;
            }
        }

        public const long MaxPreferredMessageSize = 1024 * 1024;
        public const long MaxExceptionalRecordSize = 1024 * 1024;

        // 检索词的编码方式
        internal Encoding _searchTermEncoding = Encoding.GetEncoding(936);    // 缺省为GB2312编码方式

        public Encoding SearchTermEncoding
        {
            get
            {
                return _searchTermEncoding;
            }
        }

        // MARC记录的编码方式
        internal Encoding _marcRecordEncoding = Encoding.GetEncoding(936);    // 缺省为GB2312编码方式

        public Encoding MarcRecordEncoding
        {
            get
            {
                return _marcRecordEncoding;
            }
        }

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
