using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    public class BaseTemplateData
    {
        public const string C_TColor_Content = "#000000";
        public const string C_TColor_Remark = "#CCCCCC";

        public TemplateDataItem first { get; set; }
        public TemplateDataItem remark { get; set; }
    }

    // 2个数据项
    public class Template2Data : BaseTemplateData
    {
        public Template2Data(string first, string first_color,
            string k1, string k2, 
            string remark)
        {
            this.first = new TemplateDataItem(first, first_color);
            this.keyword1 = new TemplateDataItem(k1, C_TColor_Content);
            this.keyword2 = new TemplateDataItem(k2, C_TColor_Content);
            this.remark = new TemplateDataItem(remark, C_TColor_Remark);
        }

        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
    }

    // 3个数据项
    public class Template3Data : BaseTemplateData
    {
        public Template3Data(string first, string first_color,
            string k1, string k2, string k3, 
            string remark)
        {
            this.first = new TemplateDataItem(first, first_color);
            this.keyword1 = new TemplateDataItem(k1, C_TColor_Content);
            this.keyword2 = new TemplateDataItem(k2, C_TColor_Content);
            this.keyword3 = new TemplateDataItem(k3, C_TColor_Content);
            this.remark = new TemplateDataItem(remark, C_TColor_Remark);
        }

        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
    }

    // 5个数据项
    public class Template5Data : BaseTemplateData
    {
        public Template5Data(string first, string first_color,
            string k1, string k2, string k3, string k4, string k5,
            string remark)
        {
            this.first = new TemplateDataItem(first, first_color);
            this.keyword1 = new TemplateDataItem(k1, C_TColor_Content);
            this.keyword2 = new TemplateDataItem(k2, C_TColor_Content);
            this.keyword3 = new TemplateDataItem(k3, C_TColor_Content);
            this.keyword4 = new TemplateDataItem(k4, C_TColor_Content);
            this.keyword5 = new TemplateDataItem(k5, C_TColor_Content);
            this.remark = new TemplateDataItem(remark, C_TColor_Remark);
        }

        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem keyword4 { get; set; }
        public TemplateDataItem keyword5 { get; set; }
    }

    //您好，您已借书成功。
    //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00
    //册条码号：C0000001
    //借书日期：2016-07-01
    //应还日期：2016-07-31
    //证条码号：R0000001
    //xxx，祝您阅读愉快，欢迎再借。
    public class BorrowTemplateData:Template5Data
    {
        public BorrowTemplateData(string first, string first_color,
            string k1, string k2, string k3, string k4, string k5,
            string remark)
            : base(first, first_color, k1, k2, k3, k4, k5, remark)
        { }
    }

    //尊敬的读者，您已成功还书。
    //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00 
    //册条码号：C0000001
    //借书日期：2016-5-27
    //借阅期限：31天
    //还书日期：2016-6-27
    //谢谢您及时归还，欢迎再借。
    public class ReturnTemplateData : Template5Data
    {
        public ReturnTemplateData(string first, string first_color,
            string k1, string k2, string k3, string k4, string k5,
            string remark)
            : base(first, first_color, k1, k2, k3, k4, k5, remark)
        { }
    }

    //尊敬的读者，您已成功交费。
    //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00
    //册条码号：C0000001
    //交费金额：CNY 10元
    //交费原因：超期。超1天，违约金因子：CNY1.0/Day
    //交费时间：2015-12-27 13:15
    //如有疑问，请联系系统管理员。
    public class PayTemplateData : Template5Data
    {
        public PayTemplateData(string first, string first_color,
            string k1, string k2, string k3, string k4, string k5,
            string remark)
            : base(first, first_color, k1, k2, k3, k4, k5, remark)
        { }
    }

    //您好，撤消交费成功。
    //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00
    //册条码号：C0000001
    //交费原因：超期。超1天，违约金因子
    //撤消金额：CNY1
    //撤消时间：2015-12-27 13:15
    //任延华，您已成功撤消交费，如有疑问，请联系系统管理员
    public class CancelPayTemplateData : Template5Data
    {
        public CancelPayTemplateData(string first, string first_color,
            string k1, string k2, string k3, string k4, string k5,
            string remark)
            : base(first, first_color, k1, k2, k3, k4, k5, remark)
        { }
    }




    //您好，您借出的图书已超期。
    //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00
    //册条码号：C0000001
    //借书日期：2016-07-01
    //应还日期：2016-07-31
    //超期情况：已超期30天
    //任延华，您借出的图书已超期，请尽快归还。
    public class CaoQiTemplateData : Template5Data
    {
        public CaoQiTemplateData(string first, string first_color,
            string k1, string k2, string k3, string k4, string k5,
            string remark)
            : base(first, first_color, k1, k2, k3, k4, k5, remark)
        { }
    }



    //您好，您预约的图书已经到书。
    //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00
    //册条码号：C00001
    //预约日期：2016-08-15
    //到书日期：2016-09-05
    //保留期限：2016-09-07（保留2天）
    //XXX，您预约的图书到了，请尽快来图书馆办理借书手续，请尽快来图书馆办理借书手续。如果您未能在保留期限内来馆办理借阅手续，图书馆将把优先借阅权转给后面排队等待的预约者，或做归架处理。
    public class ArrivedTemplateData : Template5Data
    {
        public ArrivedTemplateData(string first, string first_color,
            string k1, string k2, string k3, string k4, string k5,
            string remark)
            : base(first, first_color, k1, k2, k3, k4, k5, remark)
        { }
    }

//取消图书预约成功。
//书刊摘要：中国机读目录格式使用手册
//册条码号：B0000001
//预约日期：2017-10-01
//取消日期：2017-10-03
//证条码号：P000005
//张三，您取消图书预约成功，该书将不再为您保留。
    public class CancelReserveTemplateData : Template5Data
    {
        public CancelReserveTemplateData(string first, string first_color,
            string k1, string k2, string k3, string k4, string k5,
            string remark)
            : base(first, first_color, k1, k2, k3, k4, k5, remark)
        { }
    }



    //{{first.DATA}}
    //绑定帐号：{{keyword1.DATA}}
    //绑定说明：{{keyword2.DATA}}
    //{{remark.DATA}}
    public class BindTemplateData:Template2Data
    {
        public BindTemplateData(string first, string first_color,
            string k1, string k2,
            string remark)
            : base(first, first_color, k1, k2, remark)
        { }
    }



    //{{first.DATA}}
    //解绑帐号：{{keyword1.DATA}}
    //解绑说明：{{keyword2.DATA}}
    //{{remark.DATA}}
    public class UnBindTemplateData:Template2Data
    {
        public UnBindTemplateData(string first, string first_color,
            string k1, string k2,
            string remark)
            : base(first, first_color, k1, k2, remark)
        { }
    }

    //您好，您有新的消息！
    //标题：车辆剩余油量过少
    //时间：2015年8月20日
    //内容：您的车辆剩余测量过少，请注意加油
    //感谢您使用车管家！
    public class MessageTemplateData : Template3Data
    {
        public MessageTemplateData(string first, string first_color,
            string k1, string k2,string k3,
            string remark)
            : base(first, first_color, k1, k2,k3, remark)
        { }
    }
}
