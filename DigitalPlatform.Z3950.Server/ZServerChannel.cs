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
    }
}
