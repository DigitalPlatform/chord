using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Mini
{
    public class Note
    {
        //create/print/check/notice/takeoff
        public const string C_Step_Create = "create";
        public const string C_Step_Print = "print";
        public const string C_Step_Check = "check";
        public const string C_Step_Notice = "notice";
        public const string C_Step_Takeoff = "takeoff";
        public static string GetStepCaption(string step)
        {
            if (step == C_Step_Create)
                return "等待打印小票";
            else if (step == C_Step_Print)
                return "等待找书";
            else if (step == C_Step_Check)
                return "等待通知读者";
            else if (step == C_Step_Notice)
                return "等待读者取书";
            else if (step == C_Step_Takeoff)
                return "备书结束";

            return step;
        }

        public Note()
        { }

        public Note(string itemPaths,string patronName,string patronTel)
        {
            this.Items = itemPaths;
            this.CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.PatronName = patronName;
            this.PatronTel = patronTel;
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

        // 读者姓名
        public string PatronName { get; set; }

        public string PatronTel { get; set; }

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
        public string NoticeType { get; set; }//通知方式

        // 读者取书
        public string TakeoffState { get; set; }
        public string TakeoffTime { get; set; }


        // 当前步骤
        public string Step { get; set; }  //create/print/check/notice/takeoff


        public string Other { get; set; } //xml格式，预备以后扩展内容


        
             
    }
}
