using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Net
{
    public class NetUtil
    {
        // return:
        //      false   失败
        //      bool    成功
        public static bool Ping(string host, out string strInfomation)
        {
            strInfomation = "";

            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            PingReply reply = pingSender.Send(host, timeout, buffer, options);
            StringBuilder text = new StringBuilder();
            if (reply.Status == IPStatus.Success)
            {
                text.Append(string.Format("Address: {0}", reply.Address.ToString()));
                text.Append(string.Format("RoundTrip time: {0}", reply.RoundtripTime));
                if (reply.Options != null)
                {
                    text.Append(string.Format("Time to live: {0}", reply.Options.Ttl));
                    //text.Append(string.Format("Don't fragment: {0}", reply.Options.DontFragment));
                }
                //text.Append(string.Format("Buffer size: {0}", reply.Buffer.Length));
            }
            else
                text.Append(reply.Status.ToString());

            strInfomation = text.ToString();
            return reply.Status == IPStatus.Success;
        }
    }
}
