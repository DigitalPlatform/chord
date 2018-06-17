using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;

namespace DigitalPlatform.ServiceProcess
{
    public static class ServiceUtil
    {
        // http://stackoverflow.com/questions/1633429/install-windows-service-with-recovery-action-to-restart
        public static void SetRecoveryOptions(string serviceName)
        {
            int exitCode;
            using (var process = new Process())
            {
                var startInfo = process.StartInfo;
                startInfo.FileName = "sc";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                // tell Windows that the service should restart if it fails
                startInfo.Arguments = string.Format("failure \"{0}\" reset= 0 actions= restart/60000", serviceName);

                process.Start();
                process.WaitForExit();

                exitCode = process.ExitCode;
            }

            if (exitCode != 0)
                throw new InvalidOperationException();
        }

        public static string GetPathOfService(string serviceName)
        {
            using (RegistryKey service = Registry.LocalMachine.CreateSubKey("System\\CurrentControlSet\\Services"))
            {
                // 2015/11/23 增加 using 部分
                using (RegistryKey path = service.OpenSubKey(serviceName))
                {
                    if (path == null)
                        return null;   // not found
                    return (string)path.GetValue("ImagePath");
                }
            }
        }

        public static int InstallService(string fullFileName,
bool bInstall,
out string strError)
        {
            // http://stackoverflow.com/questions/20938531/managedinstallerclass-installhelper-is-locking-winservice-exe-file
            // ManagedInstallerClass.InstallHelper is locking WinService exe file
            Isolated<InstallServiceWork> isolated = new Isolated<InstallServiceWork>();
            try
            {
                strError = isolated.Value.InstallService(new Parameters { ExePath = fullFileName, Install = bInstall });
                if (string.IsNullOrEmpty(strError) == true)
                    return 0;
                return -1;
            }
            finally
            {
                isolated.Dispose();
            }
        }

        public static void StopService(string strServiceName,
            TimeSpan timeout)
        {
            ServiceController service = new ServiceController(strServiceName);
            //     = TimeSpan.FromMinutes(2);
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
        }

        public static int StartService(string strServiceName,
            TimeSpan timeout,
            out string strError)
        {
            strError = "";

            ServiceController service = new ServiceController(strServiceName);
            try
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception ex)
            {
                if (GetNativeErrorCode(ex) == 1060)
                {
                    strError = "服务不存在";
                    return -1;
                }

                else if (GetNativeErrorCode(ex) == 1056)
                {
                    strError = "调用前已经启动了";
                    return 0;
                }
                else
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }
            }

            return 1;
        }

        public static int StopService(string strServiceName,
            TimeSpan timeout,
            out string strError)
        {
            strError = "";

            ServiceController service = new ServiceController(strServiceName);
            try
            {
                service.Stop();

                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch (Exception ex)
            {
                if (GetNativeErrorCode(ex) == 1060)
                {
                    strError = "服务不存在";
                    return -1;
                }

                else if (GetNativeErrorCode(ex) == 1062)
                {
                    strError = "调用前已经停止了";
                    return 0;
                }
                else
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }
            }

            return 1;
        }

        // 1056 调用前已经启动了
        // 1060 service 尚未安装
        // 1062 调用前已经停止了
        static int GetNativeErrorCode(Exception ex0)
        {
            if (ex0 is InvalidOperationException)
            {
                InvalidOperationException ex = ex0 as InvalidOperationException;

                if (ex0.InnerException != null && ex0.InnerException is Win32Exception)
                {
                    Win32Exception ex1 = ex0.InnerException as Win32Exception;
                    return ex1.NativeErrorCode;
                }
            }

            return 0;
        }
    }

    /// <summary>
    /// 实做注册/注销 Windows Service 的类
    /// </summary>
    public class InstallServiceWork : MarshalByRefObject
    {
        public string InstallService(Parameters parameters)
        {
            // 准备 rootdir 参数，以便兼容以前的 Installer 模块功能
            // string strRootDir = Path.GetDirectoryName(parameters.ExePath);
            // string strRootDirParam = "/rootdir='" + strRootDir + "'";
            try
            {
                if (parameters.Install == true)
                    ManagedInstallerClass.InstallHelper(new[] { 
                        // strRootDirParam, 
                        parameters.ExePath });
                else
                {
                    ManagedInstallerClass.InstallHelper(new[] { "/u", 
                        // strRootDirParam, 
                        parameters.ExePath });
                }
            }
            catch (Exception ex)
            {
                // TODO: 要能够看到 InnerException
                if (parameters.Install == true)
                    return "注册 Windows Service 的过程发生错误: " + ex.Message + (ex.InnerException == null ? "" : " " + ex.InnerException.Message);
                else
                    return "注销 Windows Service 的过程发生错误: " + ex.Message + (ex.InnerException == null ? "" : " " + ex.InnerException.Message);
            }

            return null;
        }
    }

}
