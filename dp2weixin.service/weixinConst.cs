using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    public class WeiXinConst
    {
        public const string C_Session_WeiXinId = "weixinId";
        public const string C_Session_Code = "code";

        public const string C_Session_IsBind = "isBind";


        public static string EncryptKey = "dp2weixinPassword";

        public const String C_WeiXinIdPrefix = "weixinid:";

        // 检索限制最大命中数常量
        public const int C_Search_MaxCount = 200;
        public const int C_OnePage_Count = 10;

        #region 模板消息

        //微信绑定通知
        public const string C_Template_Bind = "hFmNH7on2FqSOAiYPZVJN-FcXBv4xpVLBvHsfpLLQKU";
        // 微信解绑通知 overdues
        public const string C_Template_UnBind = "1riAKkt2W0AOtkx5rx-Lwa0RKRydDTHaMjSoUBGuHog";
        //预约到书通知 
        public const string C_Template_Arrived = "Wm-7-0HJay4yloWEgGG9HXq9eOF5cL8Qm2aAUy-isoM";
        //图书超期提醒 
        public const string C_Template_CaoQi = "QcS3LoLHk37Jh0rgKJId2o93IZjulr5XxgshzlW5VkY";
        //图书到期提醒
        public const string C_Template_DaoQi = "Q6O3UFPxPnq0rSz82r9P9be41tqEPaJVPD3U0PU8XOU";

        //借阅成功通知
        public const string C_Template_Borrow = "_F9kVyDWhunqM5ijvcwm6HwzVCnwbkeZl6GV6awB_fc";
        //图书归还通知 
        public const string C_Template_Return = "zzlLzStt_qZlzMFhcDgRm8Zoi-tsxjWdsI2b3FeoRMs";
        //缴费成功通知
        public const string C_Template_Pay = "xFg1P44Hbk_Lpjc7Ds4gU8aZUqAlzoKpoeixtK1ykBI";//"4HNhEfLcroEMdX0Pr6aFo_n7_aHuvAzD8_6lzABHkiM";
         
        //退款通知
        public const string C_Template_CancelPay = "-XsD34ux9R2EgAdMhH0lpOSjcozf4Jli_eC86AXwM3Q";// "sIzSJJ-VRbFUFrDHszxCqwiIYjr9IyyqEqLr95iJVTs";
        //个人消息通知 
        public const string C_Template_Message = "rtAx0BoUAwZ3npbNIO8Y9eIbdWO-weLGE2iOacGqN_s";

        #endregion
    }
}
