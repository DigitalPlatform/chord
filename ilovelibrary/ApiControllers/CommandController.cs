using ilovelibrary.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ilovelibrary.ApiControllers
{
    public class CommandController : ApiController
    {
        // GET: api/Command
        public IEnumerable<Command> GetAllCmd()
        {
            if (HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo] == null)
            {
                throw new Exception("尚未登录!");
            }
            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo];

            return sessionInfo.GetAllCmd();
        }

        public Command GetCmd(int id)
        {
            if (HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo] == null)
            {
                throw new Exception("尚未登录!");
            }
            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo];

            return sessionInfo.GetCmd(id);
        }

        [HttpPost]
        public Command CreateCmd(Command item)
        {
            if (HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo] == null)
            {
                throw new Exception("尚未登录!");
            }
            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo];
            
            // 执行命令
            return  sessionInfo.AddCmd(item);
        }


        [HttpDelete]
        public void DeleteCmd(int id)
        {
            if (HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo] == null)
            {
                throw new Exception("尚未登录!");
            }
            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[SessionInfo.C_Session_sessioninfo];
            sessionInfo.RemoveCmd(id);
        }


    }
}
