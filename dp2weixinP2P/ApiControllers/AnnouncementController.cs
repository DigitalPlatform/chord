using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class AnnouncementController : ApiController
    {

        // GET api/<controller>
        public AnnouncementResult Get(string libId)
        {
            AnnouncementResult result = new AnnouncementResult();

            string strError = "";
            List<AnnouncementItem> list = null;
            int nRet = dp2WeiXinService.Instance.GetAnnouncements(libId, out list, out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }

            result.items = list;
            result.errorCode = nRet;
            result.errorInfo = strError;

            return result;
        }

        // POST api/<controller>
        public AnnouncementResult Post(string libId,AnnouncementItem item)
        {
            //style == add

            item.id = Guid.NewGuid().ToString();
            return dp2WeiXinService.Instance.CoverAnnouncement(libId, item, "create");
        }

        // PUT api/<controller>/5
        public AnnouncementResult Put(string libId, AnnouncementItem item)
        {
            //return libDb.Update(id, item);

            string test = "";

            return dp2WeiXinService.Instance.CoverAnnouncement(libId, item, "change");

            //return null;
     }

        // DELETE api/<controller>/5
        [HttpDelete]
        public void Delete( string id,string libId)
        {
            AnnouncementItem item = new AnnouncementItem();
            item.id = id;
            //style == delete
            dp2WeiXinService.Instance.CoverAnnouncement(libId, item, "delete");

            return;
        }
    }
}
