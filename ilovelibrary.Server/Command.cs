using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ilovelibrary.Server
{
    public class Command
    {
        public int id { get; set; }
        public string type { get; set; }
        public string readerBarcode { get; set; }
        public string itemBarcode { get; set; }

        public string description { get; set; }
        public string resultInfo { get; set; }
        public int state { get; set; }

        public string operTime { get; set; }
    }
}
