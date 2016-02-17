using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.IO;
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

                this.dp2mserver = new HostInfo();
                this.dp2mserver.Initial(element);
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

        // 运用控制台显示方式，设置一个实例的基本参数
        public static void ChangeSettings(string strXmlFileName)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strXmlFileName);
            }
            catch(FileNotFoundException)
            {
                dom.LoadXml("<root />");
            }

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2library") as XmlElement;
            if (element == null)
            {
                element = dom.CreateElement("dp2library");
                dom.DocumentElement.AppendChild(element);
            }

            Console.WriteLine("请输入 dp2library 服务器 URL: (当前值为 '" + element.GetAttribute("url") + "' )");
            string strNewValue = Console.ReadLine();
            if (string.IsNullOrEmpty(strNewValue) == false)
                element.SetAttribute("url", strNewValue);

            Console.WriteLine("请输入 dp2library 服务器 用户名: (当前值为 '" + element.GetAttribute("userName") + "' )");
            strNewValue = Console.ReadLine();
            if (string.IsNullOrEmpty(strNewValue) == false)
                element.SetAttribute("userName", strNewValue);

            string strPassword = Cryptography.Decrypt(element.GetAttribute("password"), HostInfo.EncryptKey);
            Console.WriteLine("请输入 dp2library 服务器 密码: (当前值为 '" + new string('*', strPassword.Length) + "' )");

            Console.BackgroundColor = Console.ForegroundColor;
            strNewValue = Console.ReadLine();
            Console.ResetColor();

            if (string.IsNullOrEmpty(strNewValue) == false)
                element.SetAttribute("password", Cryptography.Encrypt(strNewValue, HostInfo.EncryptKey));

            element = dom.DocumentElement.SelectSingleNode("dp2mserver") as XmlElement;
            if (element == null)
            {
                element = dom.CreateElement("dp2mserver");
                dom.DocumentElement.AppendChild(element);
            }

            Console.WriteLine("请输入 dp2mserver 服务器 URL: (当前值为 '" + element.GetAttribute("url") + "' )");
            strNewValue = Console.ReadLine();
            if (string.IsNullOrEmpty(strNewValue) == false)
                element.SetAttribute("url", strNewValue);

            Console.WriteLine("请输入 dp2mserver 服务器 用户名: (当前值为 '" + element.GetAttribute("userName") + "' )");
            strNewValue = Console.ReadLine();
            if (string.IsNullOrEmpty(strNewValue) == false)
                element.SetAttribute("userName", strNewValue);

            strPassword = Cryptography.Decrypt(element.GetAttribute("password"), HostInfo.EncryptKey);
            Console.WriteLine("请输入 dp2mserver 服务器 密码: (当前值为 '" + new string('*', strPassword.Length) + "' )");
            
            Console.BackgroundColor = Console.ForegroundColor;
            strNewValue = Console.ReadLine();
            Console.ResetColor();
            
            if (string.IsNullOrEmpty(strNewValue) == false)
                element.SetAttribute("password", Cryptography.Encrypt(strNewValue, HostInfo.EncryptKey));

            dom.Save(strXmlFileName);
        }

    }

    public class HostInfo
    {
        public static string EncryptKey = "dp2capopassword";

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

            this.Password = Cryptography.Decrypt(element.GetAttribute("password"), EncryptKey);
        }
    }
}
