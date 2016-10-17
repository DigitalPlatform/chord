using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;

namespace dp2Capo
{
    /// <summary>
    /// 用于进行基本管理和初始化的线程
    /// </summary>
    public class DefaultThread : ThreadBase
    {
        static int ShortTimeout = 60 * 1000;    // 一分钟
        public int LongTimeout = 10 * 60 * 1000;    // 十分钟

        public DefaultThread()
        {
            this.PerTime = ShortTimeout;
        }

#if NO
        public void SetShortTimeout()
        {
            int old = this.PerTime;
            this.PerTime = ShortTimeout;
            if (old == LongTimeout)
                this.Activate();    // 从长到短的转换中，需要立即激活线程一次，避免最近一次等待太长时间
        }

        public void SetLongTimeout()
        {
            this.PerTime = LongTimeout;
        }
#endif

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            if (this.Stopped == true)
                return;

            ServerInfo.BackgroundWork();
        }
    }

}
