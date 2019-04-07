﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.Core;

namespace dp2Capo
{
    /// <summary>
    /// 用于搜集 dp2library 通知消息并发送给 dp2mserver 的线程
    /// </summary>
    public class NotifyThread : ThreadBase
    {
        public Instance Container = null;

        public NotifyThread()
        {
            this.PerTime = 60 * 1000;
        }

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            if (this.Stopped == true)
                return;

            Container.Notify();
        }
    }

}
