using DigitalPlatform.IO;
using DigitalPlatform.Message;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace dp2weixin.service
{
    public class SessionInfo
    {

        private string _weixinId = "";
        public string oauth2_return_code = "";
        public ChargeCommandContainer cmdContainer = null;
        public GzhCfg  gzh = null;

        // 可选择的图书馆
        public List<string> libIds = new List<string>();

        public SessionInfo()
        {
            cmdContainer = new ChargeCommandContainer();
        }



        public string WeixinId
        {
            get
            {
                return this._weixinId;
            }

        }

        public int SetWeixinId(string weixinId,out string error)
        {
            error = "";

            this._weixinId = weixinId;
            int nRet = this.SetCurInfo(out error);
            if (nRet == -1)
                return -1;
            

            return 0;
        }


        public int SetCurInfo(out string error)
        {
            error = "";
            int nRet = 0;

            // 微信用户设置信息
            //int showPhoto = 0; //显示头像
           // int showCover = 0;//显示封面
            UserSettingItem settingItem = UserSettingDb.Current.GetByWeixinId(this.WeixinId);
            if (settingItem != null)
            {
                if (this.libIds.IndexOf(settingItem.libId) != -1) // 2016-11-22 先要在自己的可访问图书馆
                {
                    this.CurrentLib = dp2WeiXinService.Instance.LibManager.GetLibrary(settingItem.libId);//.GetLibById(settingItem.libId);
                    if (this.CurrentLib == null)
                    {
                        error = "未找到id为'" + settingItem.libId + "'对应的图书馆"; //这里lib为null竟然用了lib.id，一个bug 2016-8-11
                        return -1;
                    }

                    showPhoto = settingItem.showPhoto;
                    showCover = settingItem.showCover;

                }
                else
                {
                    dp2WeiXinService.Instance.WriteErrorLog1("发现weixinid=" + this.WeixinId + "设置的图书馆" + settingItem.libId + "不在访问列表中");
                }
            }

            if (CurrentLib == null) // 找第一个
            {
                // 找可访问的第一个图书馆 2016-11-22
                string firstLibId = this.libIds[0];
                CurrentLib = dp2WeiXinService.Instance.LibManager.GetLibrary(firstLibId);
            }
            if (CurrentLib == null)
            {
                error = "未匹配上图书馆";
                return -1;
            }

            // 找到对应的读者和工作人员
            this.ActivePatron = WxUserDatabase.Current.GetActivePatron(this.WeixinId, CurrentLib.Entity.id);
            this.Worker = WxUserDatabase.Current.GetWorker(this.WeixinId, CurrentLib.Entity.id);


            if (this.Worker != null)
            {
                List<string> dataList = null;
                nRet = dp2WeiXinService.Instance.GetSystemParameter(CurrentLib.Entity,
                    "_valueTable",
                    "readerType",
                    out dataList,
                    out error);
                if (nRet == -1)
                {
                    return -1;
                }
                if (dataList != null && dataList.Count > 0)
                {
                    readerTypes = dataList[0];
                }

                LoginInfo loginInfo = new LoginInfo(this.Worker.userName, false);
                nRet = dp2WeiXinService.Instance.GetInfo(this.CurrentLib.Entity,
                    loginInfo,
                    "getSystemParameter",
                    "reader",
                    "dbnames",
                    out dataList,
                    out error);
                if (nRet == -1)
                    return -1;
                if (dataList != null && dataList.Count > 0)
                {
                    readerDbnames = dataList[0];
                }

            }

            return 0;
        }

        public Library CurrentLib = null;
        public UserSettingItem settingItem = null;
        public int showPhoto = 0; //显示头像
        public int showCover = 0;//显示封面

        // 绑定的当前读者帐户
        public WxUserItem ActivePatron = null;

        // 绑定的工作人员帐户
        public WxUserItem Worker = null;

        public string readerTypes = "";
        public string readerDbnames = "";



    }
}
