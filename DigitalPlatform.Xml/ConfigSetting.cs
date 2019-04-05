using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DigitalPlatform.Xml
{
#if REMOVED
    // TODO: 这里需要删除，因为 DigitalPlatform.Core 里面已经有了
    /// <summary>
    /// 用 XML 文件存储配置信息
    /// </summary>
    public class ConfigSetting
    {
        XmlDocument _dom = new XmlDocument();
        string _filename = "";

        bool _changed = false;
        public bool Changed
        {
            get
            {
                return _changed;
            }
            set
            {
                _changed = value;
            }
        }

        public ConfigSetting(string filename, bool auto_create)
        {
            try
            {
                _dom.Load(filename);
            }
            catch (FileNotFoundException)
            {
                if (auto_create)
                    _dom.LoadXml("<root />");
                else
                    throw;
            }

            _filename = filename;
        }

        public static ConfigSetting Open(string filename, bool auto_create)
        {
            ConfigSetting config = new ConfigSetting(filename, auto_create);
            return config;
        }

        // 写入一个字符串值
        public string Set(string section, string entry, string value)
        {
            XmlElement element = _dom.DocumentElement.SelectSingleNode(section) as XmlElement;
            if (element == null)
            {
                element = _dom.CreateElement(section);
                _dom.DocumentElement.AppendChild(element);
                _changed = true;
            }

            string old_value = element.GetAttribute(entry);
            element.SetAttribute(entry, value);
            _changed = true;
            return old_value;
        }

        // 读取一个字符串值
        public string Get(string section, string entry, string default_value = null)
        {
            XmlElement element = _dom.DocumentElement.SelectSingleNode(section) as XmlElement;
            if (element == null)
                return default_value;

            if (element.GetAttributeNode(entry) == null)
                return default_value;

            return element.GetAttribute(entry);
        }

        // 获得一个整数值
        // return:
        //		所获得的整数值
        public int GetInt(string section,
            string entry,
            int default_value)
        {
            string value = Get(section, entry, default_value.ToString());
            return Convert.ToInt32(value);
        }

        public void SetInt(string section,
    string entry,
    int value)
        {
            Set(section, entry, value.ToString());
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(_filename) == false)
                _dom.Save(_filename);
        }

        public string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.Append("filename=" + _filename + "\r\n");
            XmlNodeList nodes = _dom.DocumentElement.SelectNodes("*");
            foreach (XmlElement section in nodes)
            {
                foreach (XmlAttribute attr in section.Attributes)
                {
                    text.Append("section=" + section.Name + ", name=" + attr.Name + ", value=" + attr.Value + "\r\n");
                }
            }

            return text.ToString();
        }
    }

#endif
}
