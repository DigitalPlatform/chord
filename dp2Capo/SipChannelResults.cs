using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Newtonsoft.Json;

using DigitalPlatform.Net;
using DigitalPlatform.SIP.Server;

namespace dp2Capo
{
    public class SipChannelResultsManager
    {
        // 通道检索结果集合
        // channel_hashcode --> SipChannelResults
        static Hashtable _channelResults = new Hashtable();

        public SipChannelResults GetResult(string id)
        {
            lock (_channelResults.SyncRoot)
            {
                if (_channelResults.ContainsKey(id) == false)
                    return null;
                var result = _channelResults[id] as SipChannelResults;
                result.LastTime = DateTime.Now;
                return result;
            }
        }

        public void PutResult(string id, List<SipChannelInfo> results)
        {
            SipChannelResults item = new SipChannelResults(id, results);
            lock (_channelResults.SyncRoot)
            {
                _channelResults[id] = item;
            }
        }

        // 清除闲置对象
        public void ClearIdle(TimeSpan delta)
        {
            lock (_channelResults.SyncRoot)
            {
                DateTime now = DateTime.Now;
                List<SipChannelResults> delete_items = new List<SipChannelResults>();
                foreach (var key in _channelResults.Keys)
                {
                    var results = _channelResults[key] as SipChannelResults;
                    if (now - results.LastTime > delta)
                        delete_items.Add(results);
                }

                foreach (var item in delete_items)
                {
                    _channelResults.Remove(item);
                }
            }
        }
    }

    public class SipChannelResults
    {
        public string ID { get; set; }
        public List<SipChannelInfo> Infos { get; set; }
        public DateTime CreateTime { get; set; }    // 创建时间
        public DateTime LastTime { get; set; }  // 最后一次访问时间

        public SipChannelResults(string id,
            List<SipChannel> channels)
        {
            this.ID = id;
            this.Infos = ToInfo(channels);
            this.CreateTime = DateTime.Now;
            this.LastTime = DateTime.Now;
        }

        public SipChannelResults(string id,
    List<SipChannelInfo> infos)
        {
            this.ID = id;
            this.Infos = infos;
            this.CreateTime = DateTime.Now;
            this.LastTime = DateTime.Now;
        }

        public static List<SipChannelInfo> ToInfo(List<SipChannel> channels)
        {
            List<SipChannelInfo> results = new List<SipChannelInfo>();
            foreach (var channel in channels)
            {
                results.Add(new SipChannelInfo(channel));
            }

            return results;
        }

        public static string ToString(List<SipChannelInfo> infos,
            string format)
        {
            if (format != null)
                format = format.ToLower();

            if (format == "xml")
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<collection />");
                foreach (var info in infos)
                {
                    var channel = dom.CreateElement("channel");
                    dom.DocumentElement.AppendChild(channel);
                    info.SetXmlAttributes(channel);
                }

                return dom.DocumentElement.OuterXml;
            }
            else if (string.IsNullOrEmpty(format) || format == "json")
            {
                return JsonConvert.SerializeObject(infos);
            }
            else
                throw new ArgumentException($"未知的格式 '{format}'");
        }
    }

    public class SipChannelInfo
    {
        public string ClientIP { get; set; }
        public string UserName { get; set; }
        public string Location { get; set; }
        public string RequestCount { get; set; }
        public string LibraryCode { get; set; }
        public string ID { get; set; }
        public string CreateTime { get; set; }
        public string LastTime { get; set; }
        public string Encoding { get; set; }

        public SipChannelInfo(SipChannel channel)
        {
            this.ID = channel.GetHashCode().ToString();
            this.CreateTime = channel.CreateTime.ToString("u");
            this.LastTime = channel.LastTime.ToString("u");
            this.ClientIP = TcpServer.GetClientIP(channel.TcpClient);
            if (string.IsNullOrEmpty(channel.InstanceName) == true)
                this.UserName = channel.UserName;
            else
                this.UserName = channel.UserName + "@" + channel.InstanceName;
            this.Location = channel.LocationCode;
            this.RequestCount = channel.RequestCount.ToString();
            this.LibraryCode = channel.LibraryCodeList;
            this.Encoding = channel.Encoding?.WebName;
        }

        public void SetXmlAttributes(XmlElement element)
        {
            element.SetAttribute("id", this.ID);
            element.SetAttribute("createTime", this.CreateTime);
            element.SetAttribute("lastTime", this.LastTime);
            element.SetAttribute("encoding", this.Encoding);
            element.SetAttribute("clientIP", this.ClientIP);
            element.SetAttribute("userName", this.UserName);
            element.SetAttribute("location", this.Location);
            element.SetAttribute("requestCount", this.RequestCount);
            element.SetAttribute("libraryCode", this.LibraryCode);
        }
    }

}
