using DigitalPlatform.Message;
using dp2Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMessage
{
    public class myMsgHandler:BaseMsgHandler
    {
        public Form1 form = null;


        public override int InternalDoMessage(MessageRecord record, out string strError)
        {
            strError = "";
            form.ShowMsg(record);

            return 1;
        }
    }
}
