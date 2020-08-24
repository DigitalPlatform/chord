using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Interfaces
{
    // 通知转发接口
    public class NoticeInterface
    {
        /// <summary>
        /// 发送通知
        /// </summary>
        /// <param name="noticeXml">通知xml</param>
        /// <param name="strError">出错信息</param>
        /// <returns>
        /// 0 成功
        /// -1 出错
        /// </returns>
        public virtual int SendNotice(string noticeXml,
            out string strError)
        {
            strError = "";
            return 0;
        }
    }
}
