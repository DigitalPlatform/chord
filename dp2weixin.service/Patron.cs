﻿using System;
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

        // 违约记录集合
        public List<OverdueInfo> overdueList = new List<OverdueInfo>();

        //===
        //在借数量
        public int BorrowCount { get; set; }

        //超期数量
        public int CaoQiCount { get; set; }

        // 在借小点html
        public string BorrowCountHtml { get; set; }

        // 在借册集合
        public List<BorrowInfo2> borrowList=new List<BorrowInfo2>();

        // 最大可借数量
        public string maxBorrowCount { get; set; }

        // 当前可借数量
        public string curBorrowCount { get; set; }

        //===
        //预约数量
        public int ReservationCount { get; set; }

        // 到书数量
        public int ArrivedCount { get; set; }

        // 预约小点html样子
        public string ReservationCountHtml { get; set; }

        // 预约记录集合
        public List<ReservationInfo> reservations = new List<ReservationInfo>();
    }

    // 预约信息 结构
     public class ReservationInfo
    {
        //pure
        public string pureBarcodes { get; set; }
        public string barcodes { get; set; }
        public string state { get; set; }

        public string stateText { get; set; }
        public string requestdate { get; set; }
        public string operatorAccount { get; set; }

        public string arrivedBarcode { get; set; }

        public string fullBarcodes { get; set; }
    }

    // 违约记录 结构
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

    // 在借信息 结构
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
