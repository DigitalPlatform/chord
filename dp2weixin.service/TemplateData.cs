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
        public TemplateDataItem first { get; set; }
        public TemplateDataItem remark { get; set; }
    }

    //您好，您已借书成功。
    //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00
    //册条码号：C0000001
    //借书日期：2016-07-01
    //应还日期：2016-07-31
    //证条码号：R0000001
    //xxx，祝您阅读愉快，欢迎再借。
    public class BorrowTemplateData:BaseTemplateData
    {
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem keyword4 { get; set; }
        public TemplateDataItem keyword5 { get; set; }
    }

    //尊敬的读者，您已成功还书。
    //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00 
    //册条码号：C0000001
    //借书日期：2016-5-27
    //借阅期限：31天
    //还书日期：2016-6-27
    //谢谢您及时归还，欢迎再借。
    public class ReturnTemplateData:BaseTemplateData
    {
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem keyword4 { get; set; }
        public TemplateDataItem keyword5 { get; set; }
    }





    //尊敬的读者，您已成功交费。
    //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00
    //册条码号：C0000001
    //交费金额：CNY 10元
    //交费原因：超期。超1天，违约金因子：CNY1.0/Day
    //交费时间：2015-12-27 13:15
    //如有疑问，请联系系统管理员。
    public class PayTemplateData:BaseTemplateData
    {
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem keyword4 { get; set; }
        public TemplateDataItem keyword5 { get; set; }
    }

    //您好，撤消交费成功。
    //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00
    //册条码号：C0000001
    //交费原因：超期。超1天，违约金因子
    //撤消金额：CNY1
    //撤消时间：2015-12-27 13:15
    //任延华，您已成功撤消交费，如有疑问，请联系系统管理员
    public class CancelPayTemplateData:BaseTemplateData
    {
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem keyword4 { get; set; }
        public TemplateDataItem keyword5 { get; set; }
    }



    //您好，您有新的消息！
    //标题：车辆剩余油量过少
    //时间：2015年8月20日
    //内容：您的车辆剩余测量过少，请注意加油
    //感谢您使用车管家！
    public class MessageTemplateData:BaseTemplateData
    {
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
    }


    //您好，您借出的图书已超期。
    //书刊摘要：中国机读目录格式使用手册 / 北京图书馆《中国机读目录格式使用手册》编委会. -- ISBN 7-80039-990-7 : ￥58.00
    //册条码号：C0000001
    //借书日期：2016-07-01
    //应还日期：2016-07-31
    //超期情况：已超期30天
    //任延华，您借出的图书已超期，请尽快归还。
    public class CaoQiTemplateData1 : BaseTemplateData
    {
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem keyword4 { get; set; }
        public TemplateDataItem keyword5 { get; set; }
    }



    //{{first.DATA}}
    //图书书名：{{keyword1.DATA}}
    //到书日期：{{keyword2.DATA}}
    //保留期限：{{keyword3.DATA}}
    //{{remark.DATA}}
    public class ArrivedTemplateData:BaseTemplateData
    {
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
    }


    //{{first.DATA}}
    //绑定帐号：{{keyword1.DATA}}
    //绑定说明：{{keyword2.DATA}}
    //{{remark.DATA}}
    public class BindTemplateData:BaseTemplateData
    {
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
    }



    //{{first.DATA}}
    //解绑帐号：{{keyword1.DATA}}
    //解绑说明：{{keyword2.DATA}}
    //{{remark.DATA}}
    public class UnBindTemplateData:BaseTemplateData
    {
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
    }
}
