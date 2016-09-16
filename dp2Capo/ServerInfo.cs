using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Common;

namespace dp2Capo
{
    public static class ServerInfo
    {
        // 实例集合
        public static List<Instance> _instances = new List<Instance>();

        // 管理线程
        public static DefaultThread _defaultThread = new DefaultThread();

        public static RecordLockCollection _recordLocks = new RecordLockCollection();

        // 从数据目录装载全部实例定义，并连接服务器
        public static void Initial(string strDataDir)
        {
            DirectoryInfo root = new DirectoryInfo(strDataDir);
            var dis = root.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                string strXmlFileName = Path.Combine(di.FullName, "capo.xml");
                if (File.Exists(strXmlFileName) == false)
                    continue;
                Instance instance = new Instance();
                instance.Initial(strXmlFileName);
                _instances.Add(instance);
            }

#if NO
            // 连接服务器，如果暂时连接失败，后面会不断自动重试连接
            foreach (Instance instance in _instances)
            {
                instance.BeginConnnect();
            }
#endif

            _defaultThread.BeginThread();
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

                if (i == index)
                {
                    Instance.ChangeSettings(strXmlFileName);
                    return;
                }
                i++;
            }

            // throw new Exception("下标 "+index.ToString()+" 超过了当前实际存在的实例数");

            // 创建足够多的新实例子目录
            for (; i <= index; i++)
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
            _defaultThread.StopThread(false);
            _defaultThread.Dispose();

            // 保存配置

            // 切断连接
            foreach (Instance instance in _instances)
            {
                instance.Close();
            }
        }

        // 执行一些后台管理任务
        public static void BackgroundWork()
        {
            List<Task> tasks = new List<Task>();

            foreach (Instance instance in _instances)
            {
                string strError = "";
                // 利用 dp2library API 获取一些配置信息
                if (string.IsNullOrEmpty(instance.dp2library.LibraryUID) == true)
                {
                    int nRet = instance.MessageConnection.GetConfigInfo(out strError);
                    if (nRet == -1)
                    {
                        // Program.WriteWindowsLog(strError);
                        instance.WriteErrorLog("获得 dp2library 配置时出错: " + strError);
                    }
                    else
                    {
                        // instance.BeginConnnect();   // 在获得了图书馆 UID 以后再发起 SignalR 连接
                        tasks.Add(instance.BeginConnectTask());
                    }
                }
                else
                {
                    if (instance.MessageConnection.IsConnected == false)
                    {
                        // instance.BeginConnnect();
                        tasks.Add(instance.BeginConnectTask());
                    }
                }
            }

            // 阻塞，直到全部任务完成。避免 BeginConnect() 函数被重叠调用
            if (tasks.Count > 0)
                Task.WaitAll(tasks.ToArray());
        }
    }
}
