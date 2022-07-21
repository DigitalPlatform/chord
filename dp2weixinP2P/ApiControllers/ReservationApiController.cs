using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class ReservationApiController : ApiController
    {

        // POST api/<controller>
        [HttpPost]
        public ItemReservationResult Post(string weixinId,
            string libId,
            string patron,
            string items,
            string style)
        {
            ItemReservationResult result = new ItemReservationResult();
            string reserRowHtml = "";
            string strError = "";
            int nRet = dp2WeiXinService.Instance.Reservation(weixinId,
                libId,
                patron,
                items,
                style,
                out reserRowHtml,
                out strError);
            result.errorCode = nRet;
            result.errorInfo = strError;

            result.reserRowHtml = reserRowHtml;

            return result;
        }
        
    }
}
