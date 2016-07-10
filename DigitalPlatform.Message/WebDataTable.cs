// #define VERIFY_CHUNK

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;

namespace DigitalPlatform.Message
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

            // 将缓冲的数据兑现到 WebData 中
            wrapper.Flush();
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

#if NO
        // 追加数据
        public WebData AddData(string taskID, WebData data)
        {
            WebDataWrapper wrapper = AddWrapper(taskID, data);
            return wrapper.WebData;
        }
#endif
        // 追加数据
        public void AddData(string taskID, WebData data)
        {
            AddWrapper(taskID, data);
        }

        // 追加数据
        WebDataWrapper AddWrapper(string taskID, WebData data)
        {
            lock (_syncRoot)
            {
                WebDataWrapper wrapper = null;
                if (_webData_table.ContainsKey(taskID) == false)
                {
                    wrapper = new WebDataWrapper();
                    _webData_table[taskID] = wrapper;
                }
                else
                    wrapper = (WebDataWrapper)_webData_table[taskID];

                wrapper.Append(data);
                return wrapper;
            }
        }

        // 将距离当前时间 delta 以上的对象清除
        public void Clean(TimeSpan delta)
        {
            lock (_syncRoot)
            {
                DateTime now = DateTime.Now;
                List<string> delete_keys = new List<string>();
                foreach (string key in _webData_table.Keys)
                {
                    WebDataWrapper wrapper = (WebDataWrapper)_webData_table[key];
                    if (now - wrapper.LastTime > delta)
                        delete_keys.Add(key);
                }

                foreach (string key in delete_keys)
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

        // 缓冲用的两个成员
        public StringBuilder Text { get; set; } // 提高字符串拼接的速度
        public List<byte> Content { get; set; } // 提高 byte [] 拼接的速度

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

        // 将 data 的数据追加到本对象
        public void Append(WebData data)
        {
            this.Touch();  // 激活一下最近时间

            if (this.WebData == null)
                this.WebData = new WebData();

            if (data.Headers != null)
            {
                if (this.WebData.Headers == null)
                    this.WebData.Headers = data.Headers;
                else
                    this.WebData.Headers += data.Headers;
            }

#if NO
            if (data.Content != null)
            {
                if (this.WebData.Content == null)
                {
#if VERIFY_CHUNK
                        if (data.Offset != 0)
                            throw new Exception("第一个 chunk 其 Offset 应该为 0");
#endif
                    this.WebData.Content = data.Content;
                }
                else
                {
#if VERIFY_CHUNK
                        if (exist.WebData.Content.Length != data.Offset)
                            throw new Exception("累积 Content 的长度 " + exist.WebData.Content.Length + " 和当前 chunk 的 offset " + data.Offset.ToString() + " 不一致");
#endif
                    this.WebData.Content = ByteArray.Add(this.WebData.Content, data.Content);
                }
            }
#endif
            if (data.Content != null)
            {
                if (this.Content == null)
                    this.Content = new List<byte>(4096);

                this.Content.AddRange(data.Content);
            }

            if (data.Text != null)
            {
#if NO
                    if (exist.WebData.Text == null)
                    {
                        exist.WebData.Text = data.Text;
                    }
                    else
                    {
                        exist.WebData.Text += data.Text;
                    }
#endif
                if (this.Text == null)
                    this.Text = new StringBuilder();

                this.Text.Append(data.Text);
            }

        }

        // 将 Text 中缓冲的数据兑现到 WebData 中
        public void Flush()
        {
            if (this.Text != null && this.Text.Length != 0)
                this.WebData.Text = this.Text.ToString();

            if (this.Content != null && this.Content.Count != 0)
                this.WebData.Content = this.Content.ToArray();
        }
    }
}
