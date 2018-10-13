using DigitalPlatform.Net;
using log4net;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DigitalPlatform.Z3950.Server
{
    public class ZServerChannel : TcpChannel
    {
        public ZServerChannelProperty EnsureProperty()
        {
            if (this.Property != null)
                return this.Property as ZServerChannelProperty;
            this.Property = new ZServerChannelProperty();
            return this.Property as ZServerChannelProperty;
        }

        // 获得用于调试输出的，当前通道名称
        public string GetDebugName(TcpClient tcpClient)
        {
            string ip = TcpServer.GetClientIP(tcpClient);

            return string.Format("ip:{0} channel:{1}", ip, this.GetHashCode());
        }
    }
}
