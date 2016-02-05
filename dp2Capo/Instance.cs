using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dp2Capo
{
    /// <summary>
    /// 一个服务器实例
    /// </summary>
    public class Instance 
    {
        public HostInfo dp2library { get; set; }
        public HostInfo dp2mserver { get; set; }

        public ServerConnection MessageConnection = new ServerConnection();

        public void Initial(string strXmlFileName)
        {
            XmlDocument dom = new XmlDocument();
            dom.Load(strXmlFileName);

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2library") as XmlElement;
            if (element == null)
                throw new Exception("配置文件 "+strXmlFileName+" 中根元素下尚未定义 dp2library 元素");

            try
            {
                this.dp2library = new HostInfo();
                this.dp2library.Initial(element);

                element = dom.DocumentElement.SelectSingleNode("dp2mserver") as XmlElement;
                if (element == null)
                    throw new Exception("配置文件 " + strXmlFileName + " 中根元素下尚未定义 dp2mserver 元素");

                this.dp2library = new HostInfo();
                this.dp2library.Initial(element);
            }
            catch(Exception ex)
            {
                throw new Exception("配置文件 " + strXmlFileName + " 格式错误: " + ex.Message);
            }
        }

        public void BeginConnnect()
        {
            this.MessageConnection.ServerUrl = this.dp2mserver.Url;
            this.MessageConnection.InitialAsync();
        }

    }

    public class HostInfo
    {
        public string Url { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public void Initial(XmlElement element)
        {
            this.Url = element.GetAttribute("url");
            if (string.IsNullOrEmpty(this.Url) == true)
                throw new Exception("元素 "+element.Name+" 尚未定义 url 属性");

            this.UserName = element.GetAttribute("userName");
            if (string.IsNullOrEmpty(this.UserName) == true)
                throw new Exception("元素 " + element.Name + " 尚未定义 userName 属性");

            this.Password = element.GetAttribute("password");
            // TODO: 解码
        }
    }
}
