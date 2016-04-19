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

        // 运用控制台显示方式，设置一个实例的基本参数
        // parameters:
        //      index   实例子目录下标。从 0 开始计数
        public static void ChangeInstanceSettings(string strDataDir, int index)
        {
            DirectoryInfo root = new DirectoryInfo(strDataDir);
            var dis = root.GetDirectories();
            int i = 0;
            foreach (DirectoryInfo di in dis)
            {
                string strXmlFileName = Path.Combine(di.FullName, "capo.xml");
                
                if ( i == index)
                {
                    Instance.ChangeSettings(strXmlFileName);
                    return;
                }
                i++;
            }

            // throw new Exception("下标 "+index.ToString()+" 超过了当前实际存在的实例数");

            // 创建足够多的新实例子目录
            for (; i<=index;i++ )
            {
                string strName = Guid.NewGuid().ToString();
                string strInstanceDir = Path.Combine(strDataDir, strName);
                Directory.CreateDirectory(strInstanceDir);
                if (i == index)
                {
                    string strXmlFileName = Path.Combine(strInstanceDir, "capo.xml");
                    Instance.ChangeSettings(strXmlFileName);
                    return;
                }
            }

        }
        // 准备退出
        public static void Exit()
        {
            // 保存配置

            // 切断连接
            foreach (Instance instance in _instances)
            {
                instance.CloseConnection();
            }
        }
    }
}
