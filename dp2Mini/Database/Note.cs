using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Mini
{
    public class Note
    {
        public Note()
        { }

        public Note(string items)
        {
            this.Items = items;
            this.CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.Step = "create";

            this.CheckResult = "";
            this.CheckedTime = "";

            this.PrintState = "";
            this.PrintTime = "";

            this.NoticeState = "";
            this.NoticeTime = "";

            this.TakeoffState = "";
            this.TakeoffTime = "";
        }

        // 自增的
        public int Id { get; set; }

        // 包含的预约记录
        public string Items { get; set; }

        // 备书库创建时间
        public string CreateTime { get; set; }



        // 打印小票
        public string PrintState { get; set; }
        public string PrintTime { get; set; }

        // 从书库找书
        public string CheckResult { get; set; }
        public string CheckedTime { get; set; }

        // 通知读者
        public string NoticeState { get; set; }
        public string NoticeTime { get; set; }

        // 读者取书
        public string TakeoffState { get; set; }
        public string TakeoffTime { get; set; }


        // 当前步骤
        public string Step { get; set; }  //create/print/check/notice/takeoff

    }
}
