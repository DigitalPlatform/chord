using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class PatronReservationController : ApiController
    {
        // GET api/<controller>
        public ReservationResult GetReservations(string libUserName, 
            string patronBarcode)
        {
            ReservationResult result = new ReservationResult();
            result.errorCode = 0;
            result.errorInfo = "";

            // 获取当前账户的信息
            List<ReservationInfo> reservations = null;
            string strError = "";
            int nRet =dp2WeiXinService.Instance.GetPatronReservation(libUserName,
                 patronBarcode,
                 out reservations,
                 out strError);
            result.errorCode = nRet;
            result.errorInfo = strError;
            result.reservations = reservations;

            return result;
        }


        
    }
}
