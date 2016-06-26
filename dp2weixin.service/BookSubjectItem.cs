using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    public class BookSubjectResult:ApiResult
    {
        public List<BookSubjectItem> list { get; set; }

        // 绑定的且有权限的工作人员
        public string userName { get; set; }
    }


    public class BookSubjectItem
    {
        public string name { get; set; }
        public string count { get; set; }
    }

    public class BookMsgResult : ApiResult
    {
        public List<MessageItem> list { get; set; }

        // 绑定的且有权限的工作人员
        public string userName { get; set; }
    }

}
