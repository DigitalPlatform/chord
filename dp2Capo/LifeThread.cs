using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using DigitalPlatform;

namespace dp2Capo
{
    public class LifeThread : ThreadBase
    {
        public LifeThread()
        {
            this.PerTime = 3 * 60 * 1000;   // 3 分钟
        }

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            if (this.Stopped == true)
                return;

            if (ServerInfo.DetectDeadLock())
            {
                string strText = "*** dp2Capo 因为检测到死锁而主动退出";
                Program.WriteWindowsLog(strText, EventLogEntryType.Error);
                ServerInfo.WriteFirstInstanceErrorLog(strText);

                // 结束进程
                // http://stackoverflow.com/questions/220382/how-can-a-windows-service-programmatically-restart-itself
                Environment.Exit(1);
            }
        }
    }
}
