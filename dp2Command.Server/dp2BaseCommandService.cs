using DigitalPlatform.IO;
using DigitalPlatform.LibraryRestClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Command.Service
{
    public class dp2BaseCommandService
    {
        // 检索限制最大命中数常量
        public const int C_Search_MaxCount = 100;

        // 微信web程序url
        public string weiXinUrl = "";
        // 微信目录
        public string weiXinLogDir = "";

        // 访问的目标图书馆
        public string libCode = "";
        public string remoteUserName = "capo2";

        #region 绑定解绑

        public virtual int Binding(string strBarcode,
            string strPassword,
            string strWeiXinId,
            out string strReaderBarcode,
            out string strError)
        {
            strReaderBarcode = "";
            strError = "未实现";
            return -1;

        }

        /// <returns>
        /// -1 出错
        /// 0   成功
        /// </returns>
        public virtual int Unbinding1(string strrBarcode,
            string strWeiXinId,
             out string strError)
        {
            strError = "未实现";

            return -1;
        }

        #endregion

        #region 根据微信id从远程库中查找对应读者

        public virtual long SearchPatronByWeiXinId(string strWeiXinId,
            out string strBarcode,
            out string strError)
        {
            strError = "未实现";
            strBarcode = "";

            return -1;
        }

        #endregion


        #region 检索书目

        /// <summary>
        /// 检索书目
        /// </summary>
        /// <param name="strWord"></param>
        /// <returns></returns>
        public virtual long SearchBiblio(string strWord,
            SearchCommand searchCmd,
            out string strFirstPage,
            out string strError)
        {
            strFirstPage = "";
            strError = "未实现";

            return -1;
        }

        public virtual int GetDetailBiblioInfo(SearchCommand searchCmd,
            int nIndex,
            out string strBiblioInfo,
            out string strError)
        {
            strBiblioInfo = "";
            strError = "未实现";
            Debug.Assert(searchCmd != null);

            return -1;
        }

        #endregion


        /// <returns>
        /// -1  出错
        /// 0   未绑定
        /// 1   成功
        /// </returns>
        public virtual int GetBorrowInfo1(string strReaderBarcode, 
            out string strBorrowInfo, 
            out string strError)
        {
            strBorrowInfo = "";
            strError = "未实现";
            return -1;
        }



        /// <returns>
        /// -1  出错
        /// 0   未绑定
        /// 1   成功
        /// </returns>
        public virtual int GetMyInfo1(string strReaderBarcode,
            out string strMyInfo,
            out string strError)
        {
            strError = "未实现";
            strMyInfo = "";
            Debug.Assert(String.IsNullOrEmpty(strReaderBarcode) == false);
            return -1;
        }

        /// <summary>
        /// 续借
        /// </summary>
        /// <param name="strItemBarcode">册条码号</param>
        /// <returns></returns>
        public virtual int Renew(string strReaderBarcode,
            string strItemBarcode,
            out BorrowInfo borrowInfo,
            out string strError)
        {
            borrowInfo = null;
            strError = "未实现";

            return -1;
        }

        #region 微信用户选择图书馆

        /// <summary>
        /// 检查微信用户是否已经选择了图书馆
        /// </summary>
        /// <param name="strWeiXinId"></param>
        /// <returns></returns>
        public WxUserItem CheckIsSelectLib(string strWeiXinId)
        {
            WxUserItem userItem = WxUserDatabase.Current.GetActive(strWeiXinId);
            if (userItem == null)
                return null;

            //记下来，以便点对点方便访问该图书馆
            this.libCode = userItem.libCode;
            this.remoteUserName = userItem.libUserName;

            return userItem;
        }

        /// <summary>
        /// 选择图书馆
        /// </summary>
        /// <param name="strWeiXinId"></param>
        /// <param name="libCode"></param>
        public void SelectLib(string strWeiXinId, string libCode, string libUserName)
        {
            WxUserItem userItem = WxUserDatabase.Current.GetOne(strWeiXinId,libCode);
            if (userItem == null)
            {
                userItem = new WxUserItem();
                userItem.weixinId = strWeiXinId;
                userItem.libCode = libCode;
                userItem.libUserName = libUserName;
                userItem.readerBarcode = "";
                userItem.readerName = "";
                userItem.xml = "";
                userItem.refID = "";
                userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                userItem.updateTime = userItem.createTime;
                WxUserDatabase.Current.Add(userItem);
            }

            //设为当前活动状态
            WxUserDatabase.Current.SetActive(userItem);

            //记下来，以便点对点方便访问该图书馆
            this.libCode = libCode;
            this.remoteUserName = libUserName;
        }

        #endregion
    }


}
