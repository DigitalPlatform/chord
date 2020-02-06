using DigitalPlatform;
using DigitalPlatform.Core;
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
            this.PerTime =10* 60 * 1000; //10分钟运行一次
        }

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            dp2WeiXinService.Instance.WriteDebug("===轮循线程begin===\r\n");

            if (this.Stopped == true)
                return;

            // 清理参于检索的数据库
            WeixinService.LibDbs.Clear();  //todo 超过一定时间再清除


            // 检查图书馆是否在线，给工作人员发通知
            // 对于挂起状态的图书馆，通知内容直接为类似"贵馆dp2capo版本太低，公众号无法访问，请及时升级。" 2016/10/17 jane
            WeixinService.WarnOfflineLib();

            dp2WeiXinService.Instance.WriteDebug("===轮循线程end===\r\n");
        }
    }
}
