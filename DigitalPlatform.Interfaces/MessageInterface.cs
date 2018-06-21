using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Interfaces
{
    // 一个扩展消息接口
    public class MessageInterface
    {
        public string Type = "";
        public Assembly Assembly = null;
        public ExternalMessageHost HostObj = null;
    }
}
