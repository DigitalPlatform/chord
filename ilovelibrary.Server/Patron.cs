using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ilovelibrary.Server
{
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
    }

    public class PatronResult
    {
        public Patron patron { get; set; }
        public ApiResult apiResult { get; set; }
    }


}
