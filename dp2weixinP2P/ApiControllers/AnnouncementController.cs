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
        public AnnouncementResult Get()
        {
            return dp2WeiXinService.Instance.GetAnnouncements();
        }

        // POST api/<controller>
        public AnnouncementResult Post(AnnouncementItem item)
        {
            //style == add
            return dp2WeiXinService.Instance.CoverAnnouncement(item,"add");
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        public void Delete(string id)
        {
            AnnouncementItem item = new AnnouncementItem();
            item.id = id;
            //style == delete
            dp2WeiXinService.Instance.CoverAnnouncement(item, "delete");

            return;
        }
    }
}
