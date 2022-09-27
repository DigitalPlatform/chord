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

        // 读者预约图书
        // weixinid:前端id
        // libId:图书馆id
        // patronBarcode:读者证条码号
        // itemBarcodes:册条码号,可以是多个册条码，中间以逗号分隔。
        // style的值：
        // “new”表示创建新的预约事项。
        // “delete”表示删除已经存在的预约事项。
        // “merge”表示合并已经存在的预约事项。
        // “split”表示分割已经存在的预约事项。
        [HttpPost]
        public ItemReservationResult Reserve(string weixinId,
            string libId,
            string patronBarcode,
            string itemBarcodes,
            string style)
        {
            ItemReservationResult result = new ItemReservationResult();
            string reserRowHtml = "";
            string strError = "";
            int nRet = dp2WeiXinService.Instance.Reservation(weixinId,
                libId,
                patronBarcode,
                itemBarcodes,
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
