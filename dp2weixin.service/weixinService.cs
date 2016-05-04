using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    public class WeixinService
    {
        public SessionInfo BindPatron(string userName,string passowrd)
        {
            SessionInfo sessionInfo = new SessionInfo();
            sessionInfo.UserName = userName;
            sessionInfo.Password = passowrd;

            return sessionInfo;
        }
    }
}
