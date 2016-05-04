using dp2Command.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    public class SessionInfo
    {
        public const string C_Session_sessioninfo = "sessioninfo";

        public string UserName = "";
        public string Password = "";
        public string Parameters = "";
        public string Rights = "";
        public string LibraryCode = "";
        public string PersonalLibrary = "";

        // 在登录时选定的馆藏
        public string SelPerLib = "";

        public bool isReader = false;

        WxUserItem userItem = null;
    }
}
