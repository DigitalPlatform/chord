using DigitalPlatform.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace dp2weixin.service
{
    public class SessionInfo
    {

        public string weixinId = "";
        public string oauth2_return_code = "";
        public ChargeCommandContainer cmdContainer = null;
        public GzhCfg  gzh = null;

        // 可选择的图书馆
        public List<string> libIds = new List<string>();

        public SessionInfo()
        {
            cmdContainer = new ChargeCommandContainer();
        }



    }
}
