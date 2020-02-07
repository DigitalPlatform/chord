using DigitalPlatform;
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
        public WxUserItem ActiveUser = null; // 可能是读者也可能是工作人员

        // 当前图书馆信息
        public Library CurrentLib = null;

        // 当前图书馆配置的读者类型和读者库，用于读者登记
        public string ReaderTypes = "";
        public string ReaderDbnames = "";

        // 当前公众号配置信息
        public GzhCfg  gzh = null;

        // 可选择的图书馆
        public List<string> libIds = new List<string>();

        // 微信传过来的code
        public string oauth2_return_code = "";

        // 命令集合
        public ChargeCommandContainer cmdContainer = null;

        // 传进来的表示公众号名称和图书馆范围的state，格式为 公众号名:capo_1,capo_2
        public string gzhState = "";


        public string Dump()
        {
            string text = "gzhState=[" + gzhState + "],mylibIds.count=[" + libIds.Count + "]"
                + ",weixinId=[" + WeixinId + "]";
            if (this.ActiveUser != null)
            {
                text += "\r\n" + this.ActiveUser.Dump() + "\r\n";
            }
            else
            {
                text += ",Active=[null]";
            }

            if (this.CurrentLib != null)
            {
                text += ",CurrentLib=[" + this.CurrentLib.Entity.libName + "]";
            }
            else
            {
                text += ",CurrentLib=[null]";
            }

            return text;
        }


        // 构造函数
        public SessionInfo()
        {
            // 创建命令管理器
            cmdContainer = new ChargeCommandContainer();
            WeixinId = "";

            this.AddDebugInfo("new sessionInfo");
        }

        /// <summary>
        /// 设置公众号信息
        /// </summary>
        /// <param name="mygzhState">传页url传进来的state</param>
        /// <param name="mygzh"></param>
        /// <param name="mylibIds"></param>
        public int SetGzhInfo(string state,
            out string strError)
        {
            int nRet = 0;
            strError = "";

            // 如果传进来的state为空，则设为：我爱图书馆
            if (string.IsNullOrEmpty(state) == true)
            {
                state = "ilovelibrary";
                dp2WeiXinService.Instance.WriteDebug("公众号state参数为空，设为ilovelibrary。");
            }

            // 根据state参数，获取公众号配置信息和图书馆配置
            nRet = dp2WeiXinService.Instance.GetGzhAndLibs(state,
               out GzhCfg gzh1,
               out List<string> libList1,
               out strError);
            if (nRet == -1)
            {
                strError = "GetGzhAndLibs()出错：" + strError;
                return -1;
            }

            if (gzh1 == null)
            {
                strError = "异常，未得到公众号配置信息";
                return -1;
            }
            // 设到成员变量
            this.gzhState = state;
            this.gzh = gzh1;
            this.libIds = libList1;

            dp2WeiXinService.Instance.WriteDebug("SetGzhInfo(),state=[" + state + "],mylibIds.count=[" + libList1.Count + "]");

            return 0;
        }



        // 根据weixinId得到当前活动帐户的信息
        public int GetActiveUser(string weixinId,
            out string error)
        {
            error = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(weixinId) == true)
            {
                error = "weixinId都不能为空";
                return -1;
            }
          
            this.WeixinId = weixinId;

            // 找到当前激活的帐户，有可能是工作人员，也有可能是读者帐号
            this.ActiveUser = WxUserDatabase.Current.GetActive(this.WeixinId);
            if (this.ActiveUser != null)
            {
                // 当前图书馆
                this.CurrentLib = dp2WeiXinService.Instance.LibManager.GetLibrary(this.ActiveUser.libId);

                // 如果是工作人员，获取地应图书馆的读者类型和读者库，用于读者登记
                if (this.ActiveUser.type == WxUserDatabase.C_Type_Worker 
                    && this.ActiveUser.userName !="public")
                {
                    List<string> dataList = null;
                    nRet = dp2WeiXinService.Instance.GetSystemParameter(
                        this.CurrentLib.Entity,
                        "_valueTable",
                        "readerType",
                        out dataList,
                        out error);
                    if (nRet == -1)
                    {
                        dp2WeiXinService.Instance.WriteErrorLog("!!!" + error);
                        //return -1;
                    }
                    if (dataList != null && dataList.Count > 0)
                    {
                        this.ReaderTypes = dataList[0];
                    }

                    LoginInfo loginInfo = new LoginInfo(this.ActiveUser.userName, false);
                    nRet = dp2WeiXinService.Instance.GetInfo(this.CurrentLib.Entity,
                        loginInfo,
                        "getSystemParameter",
                        "reader",
                        "dbnames",
                        out dataList,
                        out error);
                    if (nRet == -1)
                    {
                        dp2WeiXinService.Instance.WriteErrorLog("!!!" + error);
                        return -1;
                    }
                    if (dataList != null && dataList.Count > 0)
                    {
                        this.ReaderDbnames = dataList[0];
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
