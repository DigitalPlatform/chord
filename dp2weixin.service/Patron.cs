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

    /// <summary>
    /// 读者对象
    /// </summary>
    public class Patron
    {
        // 证条码号
        public string barcode { get; set; }

        // 显示名
        public string displayName { get; set; }

        // 姓名
        public string name { get; set; }

        // 性别
        public string gender { get; set; }

        // 出生日期
        public string dateOfBirth { get; set; }

        // 证号 2008/11/11
        public string cardNumber { get; set; }

        // 身份证号
        public string idCardNumber { get; set; }

        // 单位
        public string department { get; set; }

        // 职务
        public string post { get; set; }

        // 地址
        public string address { get; set; }

        // 电话
        public string tel { get; set; }

        // email 
        public string email { get; set; }

        // 读者类型
        public string readerType { get; set; }

        // 证状态
        public string state { get; set; }

        // 发证日期
        public string createDate { get; set; }
        // 证失效期
        public string expireDate { get; set; }

        // 租金 2008/11/11
        public string hire { get; set; }

        // 押金 2008/11/11
        public string foregift { get; set; }

        //头像地址
        public string imageUrl { get; set; }

        //二维码
        public string qrcodeUrl { get; set; }


        //违约交费数量
        public int OverdueCount { get; set; }
        public string OverdueCountHtml { get; set; }


        //在借 超期数量
        public int BorrowCount { get; set; }
        public int CaoQiCount { get; set; }
        public string BorrowCountHtml { get; set; }

        //在借 超期数量
        public int ReservationCount { get; set; }
        public int DaoQiCount { get; set; }
        public string ReservationCountHtml { get; set; }
    }




    //<td nowrap>册条码号</td><td nowrap>到达情况</td><td nowrap>摘要</td><td nowrap>请求日期</td><td nowrap>操作者</td>
    public class ReservationInfo
    {
        //pure
        public string pureBarcodes { get; set; }
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
