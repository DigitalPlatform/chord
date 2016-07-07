// #define VERIFY_CHUNK

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Message;
using DigitalPlatform;

namespace dp2Capo
{
    // 存储和管理 WebData 的数据结构
    public class WebDataTable
    {
        Hashtable _webData_table = new Hashtable(); // taskID --> WebDataWrapper
        private static readonly Object _syncRoot = new Object();

        public WebData GetData(string taskID)
        {
            WebDataWrapper wrapper = GetWrapper(taskID);
            if (wrapper == null)
                return null;
            return wrapper.WebData;
        }

        WebDataWrapper GetWrapper(string taskID)
        {
            lock (_syncRoot)
            {
                return (WebDataWrapper)_webData_table[taskID];
            }
        }

        public void RemoveData(string taskID)
        {
            lock (_syncRoot)
            {
                _webData_table.Remove(taskID);
            }
        }

        // 追加数据
        public WebData AddData(string taskID, WebData data)
        {
            WebDataWrapper wrapper = AddWrapper(taskID, data);
            return wrapper.WebData;
        }

        // 追加数据
        WebDataWrapper AddWrapper(string taskID, WebData data)
        {
            lock (_syncRoot)
            {
                WebDataWrapper wrapper = new WebDataWrapper();
                wrapper.WebData = data;
                if (_webData_table.ContainsKey(taskID) == false)
                {
                    _webData_table[taskID] = wrapper;
                    return wrapper;
                }

                WebDataWrapper exist = (WebDataWrapper)_webData_table[taskID];
                exist.Touch();  // 激活一下最近时间
                if (data.Headers != null)
                {
                    if (exist.WebData.Headers == null)
                        exist.WebData.Headers = data.Headers;
                    else
                        exist.WebData.Headers += data.Headers;
                }

                if (data.Content != null)
                {
                    if (exist.WebData.Content == null)
                    {
#if VERIFY_CHUNK
                        if (data.Offset != 0)
                            throw new Exception("第一个 chunk 其 Offset 应该为 0");
#endif
                        exist.WebData.Content = data.Content;
                    }
                    else
                    {
#if VERIFY_CHUNK
                        if (exist.WebData.Content.Length != data.Offset)
                            throw new Exception("累积 Content 的长度 " + exist.WebData.Content.Length + " 和当前 chunk 的 offset " + data.Offset.ToString() + " 不一致");
#endif
                        exist.WebData.Content = ByteArray.Add(exist.WebData.Content, data.Content);
                    }
                }

                return exist;
            }
        }

        // 将距离当前时间 delta 以上的对象清除
        public void Clean(TimeSpan delta)
        {
            lock (_syncRoot)
            {
                DateTime now = DateTime.Now;
                List<string> delete_keys = new List<string>();
                foreach(string key in _webData_table.Keys)
                {
                    WebDataWrapper wrapper = (WebDataWrapper)_webData_table[key];
                    if (now - wrapper.LastTime > delta)
                        delete_keys.Add(key);
                }

                foreach(string key in delete_keys)
                {
                    _webData_table.Remove(key);
                }
            }
        }
    }

    // WebData 对象的包装容器
    public class WebDataWrapper
    {
        public WebData WebData { get; set; }

        // 最近一次操作的时间
        public DateTime LastTime { get; set; }

        public WebDataWrapper()
        {
            this.LastTime = DateTime.Now;
        }

        public void Touch()
        {
            this.LastTime = DateTime.Now;
        }
    }
}
