using dp2Command.Service;
using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dp2weixinP2P.Controllers
{
    public class BaseController : Controller
    {

        public int CheckIsFromWeiXin(string code, string state,out string strError)
        {
            strError = "";

            // 从微信进入的            
            if (string.IsNullOrEmpty(code) == false)
            {
                // 取出session中保存的code
                string sessionCode = "";
                if (Session[WeiXinConst.C_Session_Code] != null)
                {
                    sessionCode = (String)Session[WeiXinConst.C_Session_Code];
                }
                // 如果session中的code与传进入的code相同，则不再获取weixinid
                if (sessionCode == code)
                {
                    dp2CmdService2.Instance.WriteInfoLog("传进来的code["+code+"]与session中保存的code相同，不再获取weixinid了。");
                }
                else
                {
                    dp2CmdService2.Instance.WriteInfoLog("传进来的code[" + code + "]与session中保存的code["+sessionCode+"]不同，重新获取weixinid了。");

                    string weiXinIdTemp = "";
                    int nRet = dp2CmdService2.Instance.GetWeiXinId(code, state, out weiXinIdTemp, out strError);
                    if (nRet == -1)
                    { return -1; }

                    if (String.IsNullOrEmpty(weiXinIdTemp) == false)
                    {
                        // 记下微信id
                        Session[WeiXinConst.C_Session_WeiXinId] = weiXinIdTemp;
                        // 记下code，因为在iphone点返回按钮，要重新传过来同样的code,再用这code取weixinid就会报40029
                        Session[WeiXinConst.C_Session_Code] = code;
                    }
                }
            }

            // 检查session中是否存在weixinid
            if (Session[WeiXinConst.C_Session_WeiXinId] == null
                || (String)Session[WeiXinConst.C_Session_WeiXinId] == "")
            {
                strError = "非正规途径进入或者Session已失效，请重新从微信中\"我爱图书馆\"公众号进入。";
                return -1;
            }


            string weiXinId = (string)Session[WeiXinConst.C_Session_WeiXinId];
            // 检查微信id是否已经绑定的读者
            List<WxUserItem> userList = WxUserDatabase.Current.GetByWeixinId(weiXinId);
            if (userList !=null && userList.Count >0)
                Session[WeiXinConst.C_Session_IsBind] = 1;
            else
                Session[WeiXinConst.C_Session_IsBind] = 0;

            return 0;
        }
    }
}