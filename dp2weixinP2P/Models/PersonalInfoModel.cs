using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dp2weixinWeb.Models
{
    public class PersonalInfoModel
    {
        // 证条码号
        public string barcode { get; set; }

        // 显示名
        public string displayName { get; set; }

        //二维码
        public string qrcodeUrl { get; set; }
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

        // email   todo,这里有微信id信息要过滤掉
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


}