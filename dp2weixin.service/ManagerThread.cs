using DigitalPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{

    class ManagerThread : ThreadBase
    {
        public dp2WeiXinService WeixinService = null;

        public ManagerThread()
        {
            this.PerTime =10* 60 * 1000; //10分钟
        }

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            if (this.Stopped == true)
                return;

            // 清理图书馆参于检索数据库
            WeixinService.LibDbs.Clear();  //todo 超过一定时间再清除

            // 检查不在线的图书馆，给工作人员发通知
            WeixinService.WarnOfflineLib();

            // 对上次未成功获取版本号的图书馆重新获取版本号。
            WeixinService.LibManager.RedoGetVersion();
        }
    }
}
