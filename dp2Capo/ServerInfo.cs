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
            _defaultThread.StopThread(true);    // 强制退出
            _defaultThread.Dispose();

            // 保存配置

            // 切断连接
            foreach (Instance instance in _instances)
            {
                instance.Close();
            }
        }

        static void Process(Instance instance, List<Task> tasks)
        {
            if (instance.MessageConnection.IsConnected == false)
            {
                // instance.BeginConnnect();
                tasks.Add(instance.BeginConnectTask());
            }
            else
            {
                // 验证一次请求
                string text = Guid.NewGuid().ToString();
                instance.WriteErrorLog("Begin echo: " + text);
                string result = instance.MessageConnection.echo(text).Result;
                instance.WriteErrorLog("End   echo: " + result);
            }
        }

        // 执行一些后台管理任务
        public static void BackgroundWork()
        {
            List<Task> tasks = new List<Task>();

            Instance first_instance = null;
            if (_instances.Count > 0)
                first_instance = _instances[0];
            foreach (Instance instance in _instances)
            {
                instance.WriteErrorLog("<<< BackgroundWork 开始一轮处理\r\n状态:\r\n" + instance.GetDebugState());

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
#if NO
                        if (instance.MessageConnection.IsConnected == false)
                        {
                            tasks.Add(instance.BeginConnectTask()); // 2016/10/13 以前没有 if 语句，那样就容易导致重复 BeginConnect()
                        }
#endif
                        Process(instance, tasks);
                    }
                }
                else
                {
#if NO
                    if (instance.MessageConnection.IsConnected == false)
                    {
                        // instance.BeginConnnect();
                        tasks.Add(instance.BeginConnectTask());
                    }
                    else
                    {
                        // TODO: 验证一次请求
                    }
#endif
                    Process(instance, tasks);

                }
            }

            // 阻塞，直到全部任务完成。避免 BeginConnect() 函数被重叠调用
            if (tasks.Count > 0)
            {
                if (first_instance != null)
                    first_instance.WriteErrorLog("-- BackgroundWork - 等待 " + tasks.Count + " 个 Connect 任务完成");

                Task.WaitAll(tasks.ToArray());

                if (first_instance != null)
                    first_instance.WriteErrorLog("-- BackgroundWork - " + tasks.Count + " 个 Connect 任务已经完成");
            }

            if (first_instance != null)
                first_instance.WriteErrorLog(">>> BackgroundWork 结束一轮处理\r\n");

        }
    }
}
