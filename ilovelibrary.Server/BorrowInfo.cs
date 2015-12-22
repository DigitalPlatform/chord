using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ilovelibrary.Server
{
    public class BorrowInfoResult
    {
        public List<BorrowInfo> borrowList { get; set; }
        public ApiResult apiResult { get; set; }
    }

    public class BorrowInfo
    {
        public string barcode { get; set; }
        public string renewNo { get; set; }
        public string borrowDate { get; set; }
        public string period { get; set; }
        public string borrowOperator { get; set; }
        public string renewComment { get; set; }
        public string overdue { get; set; }
        public string returnDate { get; set; }
    }
}
