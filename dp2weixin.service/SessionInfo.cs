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
        // 当前微信id
        public string WeixinId { get; set; }

        // 当前活动帐户
        public WxUserItem Active = null; // 可能是读者也可能是工作人员

        // 当前图书馆信息
        public Library CurrentLib = null;

        // 当前图书馆配置的读者类型和读者库，用于读者登记
        public string readerTypes = "";
        public string readerDbnames = "";

        // 当前公众号配置信息
        public GzhCfg  gzh = null;

        // 可选择的图书馆
        public List<string> libIds = new List<string>();

        // 微信传过来的code
        public string oauth2_return_code = "";

        // 命令集合
        public ChargeCommandContainer cmdContainer = null;

        public string gzhState = "";

        // 构造函数
        public SessionInfo()
        {
            // 创建命令管理器
            cmdContainer = new ChargeCommandContainer();
            WeixinId = "";
        }

        public void SetInfo(string mygzhState,GzhCfg mygzh, List<string> mylibIds)
        {
            this.gzhState = mygzhState;
            this.gzh = mygzh;
            this.libIds = mylibIds;
        }

        // 初始化
        public int Init1(string weixinId,
            out string error)
        {
            error = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(weixinId) == true)
            {
                error = "weixinId都不能为空";
                return -1;
            }

            this.AddDebugInfo("初始化session，weixinId="+weixinId);
            
            this.WeixinId = weixinId;

            // 找到对应的读者和工作人员
            this.Active = WxUserDatabase.Current.GetActive(this.WeixinId);
            if (this.Active != null)
            {
                // 当前图书馆
                this.CurrentLib = dp2WeiXinService.Instance.LibManager.GetLibrary(this.Active.libId);

                // 如果是工作人员，获取地应图书馆的读者类型和读者库，用于读者登记
                if (this.Active.type == WxUserDatabase.C_Type_Worker && this.Active.userName !="public")
                {
                    List<string> dataList = null;
                    nRet = dp2WeiXinService.Instance.GetSystemParameter(this.CurrentLib.Entity,
                        "_valueTable",
                        "readerType",
                        out dataList,
                        out error);
                    if (nRet == -1)
                    {
                        dp2WeiXinService.Instance.WriteErrorLog1("!!!" + error);
                        //return -1;
                    }
                    if (dataList != null && dataList.Count > 0)
                    {
                        readerTypes = dataList[0];
                    }

                    LoginInfo loginInfo = new LoginInfo(this.Active.userName, false);
                    nRet = dp2WeiXinService.Instance.GetInfo(this.CurrentLib.Entity,
                        loginInfo,
                        "getSystemParameter",
                        "reader",
                        "dbnames",
                        out dataList,
                        out error);
                    if (nRet == -1)
                    {
                        dp2WeiXinService.Instance.WriteErrorLog1("!!!" + error);
                        return -1;
                    }
                    if (dataList != null && dataList.Count > 0)
                    {
                        readerDbnames = dataList[0];
                    }

                }
            }

            return 0;
        }

        private string _debugInfo = "";
        public string DebugInfo
        {
            get
            {
                return _debugInfo;
            }
        }

        public void ClearDebugInfo()
        {
            this._debugInfo = "";
        }

        public string AddDebugInfo(string text)
        {
            if (String.IsNullOrEmpty(this._debugInfo) == false)
            {
                this._debugInfo += "<br/>";
            }

            this._debugInfo +=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +" "+ text;

            return this._debugInfo;
        }


    }
}
