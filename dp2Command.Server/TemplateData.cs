using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Command.Service
{



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
