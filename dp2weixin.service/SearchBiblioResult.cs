using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    public class SearchBiblioResult
    {
        public ApiResult apiResult { get; set; }

        // 在借册
        public List<BiblioRecord> records { get; set; }

        public long resultCount = 0;

        public bool isCanNext { get; set; }

        public string resultSetName { get; set; }
    }

    public class BiblioRecord
    {
        public string no = "";
        public string recPath = "";
        public string name = "";
        //public string libId = "";
    }

    public class BiblioDetailResult : ApiResult
    {
        public string biblioPath { get; set; }

        public string info { get; set; }
        public List<BiblioItem> itemList { get; set; }
    }

    public class BiblioItemResult : ApiResult
    {
        public List<BiblioItem> itemList { get; set; }
    }

    public class BiblioItem
    {
        /*        
册条码
状态         
卷册
馆藏地
价格

在借情况
册记录路径
        */

        public string barcode { get; set; }//这个barcode值：如果有册条码号为册条码，没有值为@refID:
        public string pureBarcode { get; set; }//册条码号 2016/10/21

        public string state { get; set; }
        public string volume { get; set; }
        public string location { get; set; }
        public string price { get; set; }

        // 索引号
        public string accessNo { get; set; }
        // 出版日期
        public string publishTime { get; set; }
        public string borrowInfo { get; set; }
        // 备注
        public string comment { get; set; }

        //2016-6-17 jane 加借阅信息
        public string borrower { get; set; }
         public string borrowDate { get; set; }
         public string borrowPeriod { get; set; }

         public string reservationInfo { get; set; }

         public string imgHtml { get; set; }

         public bool disable { get; set; }

         public string refID { get; set; }
         public string parentInfo { get; set; }  //从属于，一般成员册会有该信息

         public string recPath { get; set; }
    }
}
