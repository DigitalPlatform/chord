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
        //      strInstanceName 实例名。如果为 ".global" 表示全局服务
        public ServiceControlResult StartInstance(string strInstanceName)
        {
            return ServerInfo.StartInstance(strInstanceName);
        }

        // 停止一个 Instance
        //      strInstanceName 实例名。如果为 ".global" 表示全局服务
        public ServiceControlResult StopInstance(string strInstanceName, bool delete)
        {
            return ServerInfo.StopInstance(strInstanceName, delete);
        }

        // 获得一个实例的信息
        // return: result.Value 
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public ServiceControlResult GetInstanceInfo(string strInstanceName,
            out InstanceInfo info)
        {
            info = null;
            ServiceControlResult result = new ServiceControlResult();

            try
            {
                // 查询 dp2Capo 运行状态
                if (strInstanceName == ".")
                {
                    info = new InstanceInfo();
                    info.InstanceName = strInstanceName;
                    info.State = "running";
                    result.Value = 1;   // 表示 dp2capo 正在运行状态
                    return result;
                }

                // 查询全局服务运行状态。全局服务包括 SIP 和 Z39.50 Service
                if (strInstanceName == ".global")
                {
                    info = new InstanceInfo();
                    info.InstanceName = strInstanceName;
                    info.State = ServerInfo.GlobalServiceRunning ? "running" : "stopped";
                    result.Value = 1;
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
