using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    // 栏目api返回结果
    public class SubjectResult:ApiResult
    {
        public List<SubjectItem> list { get; set; }

        // 绑定的且有权限的工作人员
        public string userName { get; set; }
    }

    // 栏目
    public class SubjectItem
    {
        public string name { get; set; }
        public string count { get; set; }
    }

    // 一个栏目下的msg
    public class SubjectMsgResult : ApiResult
    {
        public List<MessageItem> list { get; set; }

        // 绑定的且有权限的工作人员
        public string userName { get; set; }
    }

}
