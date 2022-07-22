using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{

    // 2022/7/22 整理接口代码时，将在借、预约、违约这3组集合放在的了patron结构里，所以不需要PatronInfo这个结构了
    /*
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
    */

    // 读者简单结构
    public class SimplePatron
    {
        // 证条码号
        public string barcode { get; set; }

        // 读者类型
        public string readerType { get; set; }

        public string fullReaderType {
            get
            {
                if (String.IsNullOrEmpty(this.libraryCode) == false)
                {
                    return "{" + this.libraryCode + "} " + readerType;
                }
                else
                {
                    return this.readerType;
                }
            }
        }

        // 姓名
        public string name { get; set; }

        // 性别
        public string gender { get; set; }

        // 单位
        public string department { get; set; }

        // 电话
        public string phone { get; set; }

        // 馆代码
        public string libraryCode { get; set; }

        // 状态
        public string state { get; set; }


        //备注信息或不通过原因
        public string comment { get; set; }

    }

    // 读者完整结构，从SimplePatron继承
    public class Patron:SimplePatron
    {
        // 读者记录路径，是dp2library读者的路径
        public string recPath { get; set; }

        // 参考id
        public string refID { get; set; }

        // 显示名
        public string displayName { get; set; }

        // 出生日期
        public string dateOfBirth { get; set; }

        // 证号 2008/11/11
        public string cardNumber { get; set; }

        // 身份证号
        public string idCardNumber { get; set; }

        // 职务
        public string post { get; set; }

        // 地址
        public string address { get; set; }

        // email 
        public string email { get; set; }

        // 证状态
       // public string state { get; set; }

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

        //二维码地址
        public string qrcodeUrl { get; set; }

        //===
        //违约交费数量
        public int OverdueCount { get; set; }
        // 违约记录小点html样子
        public string OverdueCountHtml { get; set; }

        // 违约记录集合,2022/7/22 整理web api接口时增加
        public List<OverdueInfo> overdueList = new List<OverdueInfo>();

        //===
        //在借数量
        public int BorrowCount { get; set; }

        //超期数量
        public int CaoQiCount { get; set; }

        // 在借小点html
        public string BorrowCountHtml { get; set; }

        // 在借册集合,2022/7/22 整理web api接口时增加
        public List<BorrowInfo2> borrowList=new List<BorrowInfo2>();

        // 最大可借数量,2022/7/22 整理web api接口时增加
        public string maxBorrowCount { get; set; }

        // 当前可借数量,2022/7/22 整理web api接口时增加
        public string curBorrowCount { get; set; }

        //===
        //预约数量
        public int ReservationCount { get; set; }

        // 到书数量
        public int ArrivedCount { get; set; }

        // 预约小点html样子
        public string ReservationCountHtml { get; set; }

        // 预约记录集合 2022/7/22 整理web api接口时增加
        public List<ReservationInfo> reservations = new List<ReservationInfo>();
    }

    // 预约信息 结构
     public class ReservationInfo
    {
        //单纯的册条码
        public string pureBarcodes { get; set; }

        // html格式，例如：册条码之一
        public string barcodes { get; set; }

        // 状态 例如：arrived表示到书
        public string state { get; set; }

        // 状态文字表述
        public string stateText { get; set; }

        // 预约时间
        public string requestdate { get; set; }

        // 操作者
        public string operatorAccount { get; set; }

        // 到书册条码
        public string arrivedBarcode { get; set; }

        // 完整的册条码，格式为strBarcodes + "*" + strArrivedItemBarcode
        public string fullBarcodes { get; set; }
    }

    // 违约记录 结构
    public class OverdueInfo
    {
        // 册条码号
        public string barcode { get; set; }

        // 超期原因
        public string reason { get; set; }

        // 费用
        public string price { get; set; }

        // 目前没有用到该字段
        public string pauseInfo { get; set; }

        // 借书日期
        public string borrowDate { get; set; }

        // 借阅期限
        public string borrowPeriod { get; set; }

        // 还书日期
        public string returnDate { get; set; }

        // 备注
        public string comment { get; set; }

        // 目前没有用到，册条码url
        public string barcodeUrl { get; set; }
    }

    // 在借信息 结构
    public class BorrowInfo2
    {
        // 册条码号
        public string barcode { get; set; }

        //续借次
        public string renewNo { get; set; }

        //借阅日期
        public string borrowDate { get; set; }

        //借阅期限
        public string period { get; set; }

        //操作者
        public string borrowOperator { get; set; }

        // 续借备注
        public string renewComment { get; set; }

        // 超期信息
        public string overdue { get; set; }

        //应还日期
        public string returnDate { get; set; }

        // 作废：对应的opac url
        public string barcodeUrl { get; set; }

        // 样式，前端不用管这个字段
        public string rowCss { get; set; }
    }

}
