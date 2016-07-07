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
        Hashtable _webData_table = new Hashtable(); // taskID --> WebData
        private static readonly Object _syncRoot = new Object();

        public WebData GetData(string taskID)
        {
            lock (_syncRoot)
            {
                return (WebData)_webData_table[taskID];
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
            lock (_syncRoot)
            {
                if (_webData_table.ContainsKey(taskID) == false)
                {
                    _webData_table[taskID] = data;
                    return data;
                }

                WebData exist = (WebData)_webData_table[taskID];
                if (data.Headers != null)
                {
                    if (exist.Headers == null)
                        exist.Headers = data.Headers;
                    else
                        exist.Headers += data.Headers;
                }

                if (data.Content != null)
                {
                    if (exist.Content == null)
                        exist.Content = data.Content;
                    else
                        exist.Content = ByteArray.Add(exist.Content, data.Content);
                }

                return exist;
            }
        }
    }
}
