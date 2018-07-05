using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Interfaces;

namespace dp2Capo
{
    public class ServiceControlServer : MarshalByRefObject, IServiceControl, IDisposable
    {
        // 启动一个 Instance
        public ServiceControlResult StartInstance(string strInstanceName)
        {
            return ServerInfo.StartInstance(strInstanceName);
        }

        // 停止一个 Instance
        public ServiceControlResult StopInstance(string strInstanceName)
        {
            return ServerInfo.StopInstance(strInstanceName);
        }

        // 获得一个实例的信息
        public ServiceControlResult GetInstanceInfo(string strInstanceName,
            out InstanceInfo info)
        {
            info = null;
            ServiceControlResult result = new ServiceControlResult();

            try
            {
                if (strInstanceName == ".")
                {
                    info = new InstanceInfo();
                    info.InstanceName = strInstanceName;
                    info.State = "running";
                    result.Value = 1;   // 表示 dp2capo 正在运行状态
                    return result;
                }

                Instance instance = ServerInfo.FindInstance(strInstanceName);
                if (instance == null)
                {
                    result.Value = 0;
                    return result;
                }

                info = new InstanceInfo();
                info.InstanceName = instance.Name;
                info.State = instance.Running ? "running" : "stopped";
                result.Value = 1;
                return result;
            }
            catch (Exception ex)
            {
                result.Value = -1;
                result.ErrorInfo = "GetInstanceInfo() 出现异常: " + ex.Message;
                return result;
            }
        }

        public void Dispose()
        {

        }
    }
}
