using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.ServiceProcess
{
    public static class ServiceUtil
    {
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
