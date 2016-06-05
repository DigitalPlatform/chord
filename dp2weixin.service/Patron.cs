using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{

    public class PatronInfo
    {
        public Patron patron { get; set; }

        // 在借册
        public List<BorrowInfo2> borrowList { get; set; }

        // 违约/交费信息
        public List<OverdueInfo> overdueList { get; set; }

        // 预约请求
        public List<ReservationInfo> reservationList { get; set; }

    }
    public class Patron
    {
        public string barcode { get; set; }
        public string name { get; set; }
        public string department { get; set; }
        public string readerType { get; set; }

        public string state { get; set; }
        public string createDate { get; set; }
        public string expireDate { get; set; }
        public string comment { get; set; }

        public string maxBorrowCount { get; set; }
        public string curBorrowCount { get; set; }

        //WarningText
        public string warningText { get; set; }

        // 是否可借
        public int isWarning = 0;

    }



    //<td nowrap>册条码号</td><td nowrap>到达情况</td><td nowrap>摘要</td><td nowrap>请求日期</td><td nowrap>操作者</td>
    public class ReservationInfo
    {
        public string barcodes { get; set; }
        public string state { get; set; }
        //public string summary { get; set; }

        public string stateText { get; set; }
        public string requestdate { get; set; }
        public string operatorAccount { get; set; }

        public string arrivedBarcode { get; set; }

        public string fullBarcodes { get; set; }
    }

    public class OverdueInfo
    {
        public string barcode { get; set; }
        public string reason { get; set; }
        public string price { get; set; }

        public string pauseInfo { get; set; }
        public string borrowDate { get; set; }
        public string borrowPeriod { get; set; }

        public string returnDate { get; set; }
        public string comment { get; set; }

        public string barcodeUrl { get; set; }
    }


    public class BorrowInfo2
    {
        public string barcode { get; set; }
        public string renewNo { get; set; }
        public string borrowDate { get; set; }
        public string period { get; set; }
        public string borrowOperator { get; set; }
        public string renewComment { get; set; }
        public string overdue { get; set; }
        public string returnDate { get; set; }

        public string barcodeUrl { get; set; }

        public string rowCss { get; set; }
    }

}
