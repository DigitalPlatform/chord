using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Mini
{

    /*
 state —— 状态：值为“arrived”表示预约图书已在馆，可以进行备书；“outof”表示预约图书已到馆，但超过了预约保留期。
itemBarcode —— 图书条码号，即图书标识。
itemRefID —— 图书参考ID。
onShelf —— 需要通知的图书是否在架，值为 true 或 false。
libraryCode —— 预约者所属分馆的馆代码。
readerBarcode —— 读者证条码号。
notifyDate —— 通知日期。
refID —— 预约到书队列记录参考ID。
location —— 所预约图书馆藏地点。
accessNo —— 所预约图书索取号。
     */

    public class ReservationItem
    {
        // 以下字段是预约到书记录的字段
        public string RecPath { get; set; }
        public string State { get; set; }  // arrive,outof(超过保留期)
        public string ItemBarcode { get; set; } 
        public string ItemRefID { get; set; } 
        public string ReaderBarcode { get; set; }
        public string LibraryCode { get; set; }
        public string OnShelf { get; set; } //是否在架
        public string NotifyDate { get; set; }
        public string Location { get; set; }
        public string AccessNo { get; set; }


        // 以下字段为图书信息
        public string ISBN { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }


        // 以下字段是读者信息
        public string ReaderName { get; set; }

        public string Department { get; set; }

        public string Tel { get; set; }

        public string RequestDate { get; set; }

        public string ArrivedDate { get; set; }

        // 备书产生的字段
        public string PrintState { get; set; }
        public string CheckState { get; set; }  // 是否找到图书，值为：找到/未找到
    }
}
