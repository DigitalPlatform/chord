using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
//{{first.DATA}}
//图书书名：{{keyword1.DATA}}
//册条码号：{{keyword2.DATA}}
//借阅日期：{{keyword3.DATA}}
//借阅期限：{{keyword4.DATA}}
//应还日期：{{keyword5.DATA}}
//{{remark.DATA}}

//尊敬的XXX，恭喜您借书成功。
//图书书名：C#开发教程
//册条码号：C0000001
//借阅日期：2016-5-27
//借阅期限：31
//应还日期：2016-6-27
//祝您阅读愉快，欢迎再借。
    public class BorrowTemplateData
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem keyword4 { get; set; }
        public TemplateDataItem keyword5 { get; set; }
        public TemplateDataItem remark { get; set; }
    }


//{{first.DATA}}
//书名：{{keyword1.DATA}}
//归还时间：{{keyword2.DATA}}
//借阅人：{{keyword3.DATA}}
//{{remark.DATA}}    
//您好,你借阅的图书已确认归还.
//书名：算法导论
//归还时间：2015-10-10 12:14
//借阅人：李明
//欢迎继续借书!
    public class ReturnTemplateData
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem keyword4 { get; set; }
        public TemplateDataItem keyword5 { get; set; }
        public TemplateDataItem remark { get; set; }
    }



//{{first.DATA}}
//订单号：{{keyword1.DATA}}
//缴费人：{{keyword2.DATA}}
//缴费金额：{{keyword3.DATA}}
//费用类型：{{keyword4.DATA}}
//缴费时间：{{keyword5.DATA}}
//{{remark.DATA}}
//您好，您已缴费成功！
//订单号：书名（册条码号）
//缴费人：张三
//缴费金额：￥100.00
//费用类型：违约
//缴费时间：2015-12-27 13:15
//如有疑问，请联系学校管理员，感谢您的使用！
    public class PayTemplateData
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem keyword4 { get; set; }
        public TemplateDataItem keyword5 { get; set; }
        public TemplateDataItem remark { get; set; }
    }

//{{first.DATA}}
//书刊摘要：{{keyword1.DATA}}
//册条码号：{{keyword2.DATA}}
//交费原因：{{keyword3.DATA}}
//撤消金额：{{keyword4.DATA}}
//撤消时间：{{keyword5.DATA}}
//{{remark.DATA}}
    public class ReturnPayTemplateData
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem keyword4 { get; set; }
        public TemplateDataItem keyword5 { get; set; }
        public TemplateDataItem remark { get; set; }
    }


//{{first.DATA}}
//标题：{{keyword1.DATA}}
//时间：{{keyword2.DATA}}
//内容：{{keyword3.DATA}}
//{{remark.DATA}}
//您好，您有新的消息！
//标题：车辆剩余油量过少
//时间：2015年8月20日
//内容：您的车辆剩余测量过少，请注意加油
//感谢您使用车管家！
    public class MessageTemplateData
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem remark { get; set; }
    }


//{{first.DATA}}
//图书书名：{{keyword1.DATA}}
//应还日期：{{keyword2.DATA}}
//超期天数：{{keyword3.DATA}}
//{{remark.DATA}}
    public class CaoQiTemplateData
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem remark { get; set; }

    }

    //{{first.DATA}}
    //图书书名：{{keyword1.DATA}}
    //到书日期：{{keyword2.DATA}}
    //保留期限：{{keyword3.DATA}}
    //{{remark.DATA}}
    public class ArrivedTemplateData
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem remark { get; set; }

    }


    //{{first.DATA}}
    //绑定帐号：{{keyword1.DATA}}
    //绑定说明：{{keyword2.DATA}}
    //{{remark.DATA}}
    public class BindTemplateData
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem remark { get; set; }

    }



//{{first.DATA}}
//解绑帐号：{{keyword1.DATA}}
//解绑说明：{{keyword2.DATA}}
//{{remark.DATA}}
    public class UnBindTemplateData
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem remark { get; set; }

    }
}
