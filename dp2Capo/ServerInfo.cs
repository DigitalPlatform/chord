using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Capo
{
    public static class ServerInfo
    {
        public static List<Instance> _instances = new List<Instance>();

        // 从数据目录装载全部实例定义，并连接服务器
        public static void Initial(string strDataDir)
        {
            DirectoryInfo root = new DirectoryInfo(strDataDir);
            var dis = root.GetDirectories();
            foreach(DirectoryInfo di in dis)
            {
                string strXmlFileName = Path.Combine(di.FullName, "capo.xml");
                if (File.Exists(strXmlFileName) == false)
                    continue;
                Instance instance = new Instance();
                instance.Initial(strXmlFileName);
                _instances.Add(instance);
            }

            // 连接服务器，如果暂时连接失败，后面会不断自动重试连接
            foreach(Instance instance in _instances)
            {
                instance.BeginConnnect();
            }
        }

        // 准备退出
        public static void Exit()
        {
            // 保存配置
        }
    }
}
