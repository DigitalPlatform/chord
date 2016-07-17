using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    // 栏目api返回结果
    public class SubjectResult : ApiResult
    {
        public List<SubjectItem> list { get; set; }

        public string html { get; set; }

        // 绑定的且有权限的工作人员
        //public string userName { get; set; }
    }

    // 栏目
    public class SubjectItem:IComparable
    {
        public int no { get; set; }
        public string pureName { get; set; }
        public string name { get; set; }

        //public string encodedName { get; set; }
        public int count { get; set; }

        public int CompareTo(object obj)
        {
            string thisName = this.no.ToString() + this.pureName;//this.name;
            string otherName = ((SubjectItem)obj).no.ToString() + ((SubjectItem)obj).pureName;// ((SubjectItem)obj).name;
            int maxLength = thisName.Length>otherName.Length?thisName.Length:otherName.Length;
            if (thisName.Length<maxLength)
                thisName= thisName.PadLeft(maxLength,'0');
            if (otherName.Length<maxLength)
                otherName=otherName.PadLeft(maxLength,'0');

            return String.Compare(thisName, otherName, true);
            //return  ((SubjectItem)obj).no.CompareTo(this.no);
        }
    }

    public class MessageResult : ApiResult
    {
        public List<MessageItem> items { get; set; }

        public string userName { get; set; }
    }

    public class MessageItem
    {
        public string id { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string contentFormat { get; set; }  // 2016-6-20 text/markdown
        public string contentHtml { get; set; }  // 2016-6-20 html形态
        public string publishTime { get; set; } // 2016-6-20 jane 发布时间，服务器消息的时间

        public string creator { get; set; }  //创建消息的工作人员帐户

        public string subject { get; set; } // 栏目
        public int subjectIndex = -1;//当前subject在subject列表中的序号，以便新增的subject在界面上插到合适的位置

        public string remark { get; set; } //注释
    }
}
