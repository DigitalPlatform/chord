using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class BbController : ApiController
    {

        // GET api/<controller>
        public BbResult Get(string libId,string weixinId)
        {
            BbResult result = new BbResult();

            string strError = "";
            List<BbItem> list = null;
            string worker = "";
            int nRet = dp2WeiXinService.Instance.GetBbs(libId,weixinId,out list,out worker, out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }

            result.items = list;
            result.worker = worker;
            result.errorCode = nRet;
            result.errorInfo = strError;

            return result;
        }

        // POST api/<controller>
        public BbResult Post(string libId,string type,BbItem item)
        {
            //style == add
            if (type == "covert2html")
            {
                string html = dp2WeiXinService.Convert2Html(item.contentFormat, item.content);
                BbResult result = new BbResult();
                result.html = html;
                return result;
            }

            // 服务器会自动产生id
            //item.id = Guid.NewGuid().ToString();
            return dp2WeiXinService.Instance.CoverBb(libId, item, "create");
        }

        // PUT api/<controller>/5
        public BbResult Put(string libId, BbItem item)
        {
            //return libDb.Update(id, item);

            string test = "";

            return dp2WeiXinService.Instance.CoverBb(libId, item, "change");

            //return null;
     }

        // DELETE api/<controller>/5
        [HttpDelete]
        public void Delete( string id,string libId)
        {
            BbItem item = new BbItem();
            item.id = id;
            //style == delete
            dp2WeiXinService.Instance.CoverBb(libId, item, "delete");

            return;
        }
    }
}
