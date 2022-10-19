using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    //书目检索返回结果
    public class SearchBiblioResult
    {
        // 当检索成功时，errorcode成员返回命中总数。
        public ApiResult apiResult { get; set; }

        // 本次返回记录数量
        public long resultCount = 0;

        // 是否有下次
        public bool isCanNext { get; set; }

        // 返回的结果集名称
        public string resultSetName { get; set; }

        // 本次返回的书目记录集合
        public List<BiblioRecord> records { get; set; }


    }

    // 书目记录
    public class BiblioRecord
    {
        //序号
        public string no = "";

        // 书目路径
        public string recPath = "";

        // 图书名称
        public string name = "";
    }

    public class BiblioDetailResult : ApiResult
    {
        public string biblioPath { get; set; }

        // 书目记录时间戳
        public string Timestamp { get; set; }

        public List<BiblioItem> itemList { get; set; }

        //public string biblioName { get; set; }
    }

    public class BiblioItemResult : ApiResult
    {
        public List<BiblioItem> itemList { get; set; }
    }

    public class BiblioItem
    {

        public string barcode { get; set; }//这个barcode值：如果有册条码号为册条码，没有值为@refID:
        public string pureBarcode { get; set; }//册条码号

        public string state { get; set; }  //状态   
        public string volume { get; set; }  //卷册

        // 馆藏地
        public string location { get; set; }

        // 当前位置
        public string currentLocation { get; set; }

        //shelfNo 法定架号
        public string shelfNo { get; set; }


        public string price { get; set; } //价格

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
        public string returningDate { get; set; } // 2022/8/9 增加一个还书日期

        public string reservationInfo { get; set; }

        public string imgHtml { get; set; }

        public bool disable { get; set; }

        public string refID { get; set; }
        public string parentInfo { get; set; }  //从属于，一般成员册会有该信息

        public string recPath { get; set; }


        public bool isGray = false;
        public bool isNotCareLoc = false;

        // 2020/4/4，注意如果调api的前端传上此参数，则一定要做成属性。
        public string batchNo { get; set; }
        public string bookType { get; set; }
    }

    // 书目字段结构，用于简单编目
    public class BiblioFields
    {
        public string BiblioPath { get; set; } //书目路径

        //Action是针对这一条记录要执行的动作，
        //一般是为"new" "change" "delete" "onlydeletebiblio" "onlydeletesubrecord"之一
        public string Action { get; set; }

        // 字段组合字符串，格式如下：
        /*
            ISBN|010$a|978-7-101-10528-5
            题名|200$a|杜诗详注
            第一作者|200$f|(唐)杜甫撰
            个人主要作者|701$a|杜甫
         */
        public string Fields { get; set; }

        // 时间戳，除new动作，其它动作需要提供时间戳
        public string Timestamp { get; set; }


        public string Dump()
        {
            return this.BiblioPath + "\r\n"
                + this.Action + "\r\n"
                + this.Timestamp + "\r\n"
                + this.Fields;
        }
    }
}
