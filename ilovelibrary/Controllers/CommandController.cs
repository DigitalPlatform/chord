using ilovelibrary.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ilovelibrary.Controllers
{
    public class CommandController : ApiController
    {
        // GET: api/Command
        public IEnumerable<Command> GetAllCmd()
        {
            return ilovelibraryServer.Instance.GetAllCmd();
        }

        public Command GetCmd(int id)
        {
            return ilovelibraryServer.Instance.GetCmd(id);
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
            string strError="";
            int nRet = ilovelibraryServer.Instance.AddCmd(sessionInfo,item, out strError);
            return item;
        }

        [HttpPut]
        public bool UpdateCmd(Command item)
        {
            return ilovelibraryServer.Instance.UpdateCmd(item);
        }

        [HttpDelete]
        public void DeleteCmd(int id)
        {
            ilovelibraryServer.Instance.RemoveCmd(id);
        }


    }
}
