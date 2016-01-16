using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ilovelibrary.Server
{

    public class SearchItemResult
    {
        public ApiResult apiResult { get; set; }

        // 在借册
        public List<BiblioItem> itemList { get; set; }

    }

    public class BiblioItem
    {
        /*
        状态
册条码号
在借情况
书目摘要
卷册
馆藏地
价格
册记录路径
        */
        public string state { get; set; }
        public string barcode { get; set; }
        public string readerSummary { get; set; }
        public string summary { get; set; }


        public string volumn { get; set; }
        public string location { get; set; }
        public string price { get; set; }
        public string oldRecPath { get; set; }


        public bool isGray = false;

        public bool isManagetLoc = true;

        public string readerSummaryStyle  { get; set; }
    }
}
