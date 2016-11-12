using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.AspNet.SignalR.Client;

using DigitalPlatform.Common;
using DigitalPlatform.Net;
using DigitalPlatform;
using DigitalPlatform.MessageClient;

namespace dp2Capo
{
    public static class ServerInfo
    {
        // 实例集合
        public static List<Instance> _instances = new List<Instance>();

        // 管理线程
        public static DefaultThread _defaultThread = new DefaultThread();

        public static LifeThread _lifeThread = new LifeThread();

        public static RecordLockCollection _recordLocks = new RecordLockCollection();

        // 从数据目录装载全部实例定义，并连接服务器
        public static void Initial(string strDataDir)
        {
            _exited = false;

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

            _lifeThread.BeginThread();
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

        static bool _exited = false;
        // 准备退出
        public static void Exit()
        {
            if (_exited == false)
            {
                _lifeThread.StopThread(true);
                _lifeThread.Dispose();

                _defaultThread.StopThread(true);    // 强制退出
                _defaultThread.Dispose();

                // 保存配置

                // 切断连接
                foreach (Instance instance in _instances)
                {
                    instance.Close();
                }
                _exited = true;
            }
        }

        static TimeSpan LongTime = TimeSpan.FromMinutes(5);    // 10

        static bool NeedCheck(Instance instance)
        {
            DateTime now = DateTime.Now;

            ConnectionState state = instance.MessageConnection.ConnectState;

            if (state == ConnectionState.Disconnected)
            {
                instance.LastCheckTime = now;
                return true;
            }
            else
            {
                if (now - instance.LastCheckTime >= LongTime)
                {
                    instance.LastCheckTime = now;
                    return true;
                }
            }

            return false;
        }

        static void Echo(Instance instance, bool writeLog)
        {
            if (instance.MessageConnection.ConnectState != ConnectionState.Connected)
                return;

            // 验证一次请求
            // string text = Guid.NewGuid().ToString();
            string text = "!verify";

            if (writeLog)
                instance.WriteErrorLog("Begin echo: " + text);

            try
            {
                string result = instance.MessageConnection.EchoTaskAsync(text, TimeSpan.FromSeconds(5), instance._cancel.Token).Result;

                // 此用法在 dp2mserver 不存在 echo() API 的时候会挂起当前线程
                // string result = instance.MessageConnection.echo(text).Result;

                if (result == null)
                    result = "(timeout)";

                if (writeLog)
                    instance.WriteErrorLog("End   echo: " + result);
            }
            catch (Exception ex)
            {
                instance.WriteErrorLog("echo 出现异常: " + ExceptionUtil.GetExceptionText(ex));
                {
                    string strErrorCode = "";
                    if (MessageConnection.IsHttpClientException(ex, out strErrorCode))
                    {
                        // echo 的时候有小概率可能会返回用户认证异常？重置连接
                        Task.Run(() => instance.TryResetConnection());
                    }
                }
            }

        }

        /*
*
.NET Runtime 	Error 	2016/11/4 13:19:54
Application: dp2capo.exe
Framework Version: v4.0.30319
Description: The process was terminated due to an unhandled exception.
Exception Info: System.Net.Sockets.SocketException
   at System.Net.Dns.GetAddrInfo(System.String)
   at System.Net.Dns.InternalGetHostByName(System.String, Boolean)
   at System.Net.Dns.GetHostAddresses(System.String)
   at System.Net.NetworkInformation.Ping.Send(System.String, Int32, Byte[], System.Net.NetworkInformation.PingOptions)

Exception Info: System.Net.NetworkInformation.PingException
   at System.Net.NetworkInformation.Ping.Send(System.String, Int32, Byte[], System.Net.NetworkInformation.PingOptions)
   at DigitalPlatform.Net.NetUtil.Ping(System.String, System.String ByRef)
   at dp2Capo.ServerInfo.Check(dp2Capo.Instance, System.Collections.Generic.List`1<System.Threading.Tasks.Task>)
   at dp2Capo.ServerInfo.BackgroundWork()
   at dp2Capo.DefaultThread.Worker()
   at DigitalPlatform.ThreadBase.ThreadMain()
   at System.Threading.ExecutionContext.RunInternal(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object, Boolean)
   at System.Threading.ExecutionContext.Run(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object, Boolean)
   at System.Threading.ExecutionContext.Run(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object)
   at System.Threading.ThreadHelper.ThreadStart()
         * */
        static void Check(Instance instance, List<Task> tasks)
        {
            if (instance.MessageConnection.ConnectState == ConnectionState.Disconnected)    // 注：如果在 Connecting 和 Reconnecting 状态则不尝试 ConnectAsync
            {
                // instance.BeginConnnect();
                tasks.Add(instance.BeginConnectTask());

                Uri uri = new Uri(instance.dp2mserver.Url);
                try
                {
                    string strInformation = "";
                    if (NetUtil.Ping(uri.DnsSafeHost, out strInformation) == true)
                    {
                        instance.WriteErrorLog("ping '" + uri.DnsSafeHost + "' success");
                    }
                    else
                    {
                        instance.WriteErrorLog("ping '" + uri.DnsSafeHost + "' fail: " + strInformation);
                    }
                }
                catch (Exception ex)
                {
                    instance.WriteErrorLog("ping '" + uri.DnsSafeHost + "' 出现异常: " + ExceptionUtil.GetExceptionText(ex));
                }

            }
            else
            {
                Echo(instance, true);
            }
        }

        // 检测是否发生了死锁
        // return:
        //      true    发生了死锁
        //      false   没有发生死锁
        public static bool DetectDeadLock()
        {
            if (_instances == null || _instances.Count == 0)
                return false;
            DateTime now = DateTime.Now;
            TimeSpan delta = TimeSpan.FromMinutes(10);  // 10 分钟没有动作就表明死锁了
            foreach (Instance instance in _instances)
            {
                if (now - instance.LastCheckTime > delta)
                    return true;
            }

            return false;
        }

        // 向第一个实例的日志文件中写入
        public static void WriteFirstInstanceErrorLog(string strText)
        {
            if (_instances == null || _instances.Count == 0)
                return;
            else
            {
                try
                {
                    _instances[0].WriteErrorLog(strText);
                }
                catch
                {
                    Program.WriteWindowsLog(strText);
                }
            }
        }

        static DateTime _lastCleanTime = DateTime.Now;

        // 执行一些后台管理任务
        public static void BackgroundWork()
        {
            List<Task> tasks = new List<Task>();

            bool bOutputBegin = false;

            Instance first_instance = null;
            if (_instances.Count > 0)
                first_instance = _instances[0];
            foreach (Instance instance in _instances)
            {
                // 向 dp2mserver 发送心跳消息
                // instance.SendHeartBeat();

                bool bNeedCheck = NeedCheck(instance);

                if (bNeedCheck)
                {
                    instance.WriteErrorLog("<<< BackgroundWork 开始一轮处理\r\n状态:\r\n" + instance.GetDebugState());
                    bOutputBegin = true;
                }

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
                        if (bNeedCheck)
                            Check(instance, tasks);
                        else
                            Echo(instance, false);
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

                    if (bNeedCheck)
                        Check(instance, tasks);
                    else
                        Echo(instance, false);

                    // 每隔二十分钟清理一次闲置的 dp2library 通道
                    if (DateTime.Now - _lastCleanTime >= TimeSpan.FromMinutes(20))
                    {
                        try
                        {
                            instance.MessageConnection.CleanLibraryChannel();
                        }
                        catch (Exception ex)
                        {
                            instance.WriteErrorLog("CleanLibraryChannel() 异常: " + ExceptionUtil.GetExceptionText(ex));
                        }
                        _lastCleanTime = DateTime.Now;
                    }
                }
            }

            // 阻塞，直到全部任务完成。避免 BeginConnect() 函数被重叠调用
            if (tasks.Count > 0)
            {
                // test 这一句可以用来制造死锁测试场景
                // Thread.Sleep(60 * 60 * 1000);

                if (first_instance != null)
                    first_instance.WriteErrorLog("-- BackgroundWork - 等待 " + tasks.Count + " 个 Connect 任务完成");

                Task.WaitAll(tasks.ToArray());

                if (first_instance != null)
                    first_instance.WriteErrorLog("-- BackgroundWork - " + tasks.Count + " 个 Connect 任务已经完成");
            }

            if (bOutputBegin == true && first_instance != null)
                first_instance.WriteErrorLog(">>> BackgroundWork 结束一轮处理\r\n");

        }

        public static string Version
        {
            get
            {
                return Assembly.GetAssembly(typeof(Instance)).GetName().Version.ToString();
            }
        }
    }
}
