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
            WeixinService.LibDbs.Clear();

            // todo 检查有没有不在线的图书馆，给工作人员发通知
        }
    }
}
