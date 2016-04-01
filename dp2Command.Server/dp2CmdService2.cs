using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using dp2Command.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dp2Command.Server
{
    public class dp2CmdService2
    {

        //=================
        // 设为单一实例
        static dp2CmdService2 _instance;
        private dp2CmdService2()
        {
            //Thread.Sleep(100); //假设多线程的时候因某种原因阻塞100毫秒
        }
        static object myObject = new object();
        static public dp2CmdService2 Instance
        {
            get
            {
                lock (myObject)
                {
                    if (null == _instance)
                    {
                        _instance = new dp2CmdService2();
                    }
                    return _instance;
                }
            }
        }
        //===========


        // dp2服务器地址与代理账号
        public string dp2mServerUrl = "";
        public string userName = "";
        public string password = "";

        // 微信web程序url
        public string weiXinUrl = "";
        // 微信目录
        public string weiXinLogDir = "";

        public void Init(string dp2mServerUrl,
            string userName,
            string password,
            string weiXinUrl,
            string weiXinLogDir,
            string mongoDbConnStr,
            string instancePrefix)
        {
            this.dp2mServerUrl = dp2mServerUrl;
            this.userName = userName;
            this.password = password;
            this.weiXinUrl = weiXinUrl;
            this.weiXinLogDir = weiXinLogDir;

            // 使用mongodb存储微信用户与读者绑定关系
            WxUserDatabase.Current.Open(mongoDbConnStr, instancePrefix);
        }

        #region 微信用户选择图书馆

        /// <summary>
        /// 检查微信用户是否已经选择了图书馆
        /// </summary>
        /// <param name="strWeiXinId"></param>
        /// <returns></returns>
        public string CheckIsSelectLib(string strWeiXinId)
        {
            WxUserItem userItem = WxUserDatabase.Current.GetOneByWeixinId(strWeiXinId);
            if (userItem == null)
                return "";

            if (userItem.libCode == "")
                return "";

            return userItem.libCode;
        }

        /// <summary>
        /// 选择图书馆
        /// </summary>
        /// <param name="strWeiXinId"></param>
        /// <param name="libCode"></param>
        public void SelectLib(string strWeiXinId, string libCode, string libUserName)
        {
            WxUserItem userItem = WxUserDatabase.Current.GetOneByWeixinId(strWeiXinId);
            if (userItem == null)
            {
                userItem = new WxUserItem();
                userItem.weixinId = strWeiXinId;
                userItem.libCode = libCode;
                userItem.libUserName = libUserName;
                userItem.readerBarcode = "";
                userItem.readerName = "";
                userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                WxUserDatabase.Current.Add(userItem);
            }
            else
            {
                userItem.libCode = libCode;
                userItem.libUserName = libUserName;
                userItem.readerBarcode = "";
                userItem.readerName = "";
                userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                WxUserDatabase.Current.Update(userItem);
            }
        }

        #endregion

        #region 绑定解绑



        /// <summary>
        /// 
        /// </summary>
        /// <param name="strBarcode"></param>
        /// <param name="strPassword"></param>
        /// <param name="weiXinId"></param>
        /// <returns>
        /// -1 出错
        /// 0 读者证条码号或密码不正确
        /// 1 成功
        /// </returns>
        public int Binding(string strBarcode,
            string strPassword,
            string strWeiXinId,
            out string strReaderBarcode,
            out string strError)
        {
            strError = "";
            strReaderBarcode = "";
            long lRet = -1;

            // 可以先调一下VerifyReaderPassword直接校验一下读者条码与密码。

            // getPatronInfo,获取读者记录
            // 检查密码是否正确


            // 根据barcode检索出来,得到原记录与时间戳
            //string strRecPath = response.strRecPath;
            //string strTimestamp = StringUtil.GetHexTimeStampString(response.baTimestamp);
            //string strXml = response.results[0];

            /*
                    // 改读者的email字段
                   XmlDocument readerDom = new XmlDocument();
                   readerDom.LoadXml(strXml);
                    XmlNode emailNode = readerDom.SelectSingleNode("//email");
                    if (emailNode == null)
                    {
                        emailNode = readerDom.CreateElement("email");
                        readerDom.DocumentElement.AppendChild(emailNode);
                    }
                    emailNode.InnerText = JoinEmail(emailNode.InnerText, strWeiXinId);
                    string strNewXml = ConvertXmlToString(readerDom);
            */
            // 更新到读者库 setPatronInfo

            // 绑定成功，把读者证条码记下来，用于续借 2015/11/7，不要用strbarcode变量，因为可能做的大小写转换
            //strReaderBarcode = DomUtil.GetNodeText(readerDom.DocumentElement.SelectSingleNode("barcode"));

            // 将关系存到mongodb库
            //name
            string name = "";
            /*
            XmlNode node = readerDom.DocumentElement.SelectSingleNode("name");
            if (node != null)
                name = DomUtil.GetNodeText(node);
            */
            WxUserItem userItem = WxUserDatabase.Current.GetOneByWeixinId(strWeiXinId);
            if (userItem == null)
            {
                // 大微信号管理多个图书馆不可能出现不存在的情况，必然先选择了图书馆
                userItem = new WxUserItem();
                userItem.weixinId = strWeiXinId;
                userItem.libCode = "";
                userItem.libUserName = "";
                userItem.readerBarcode = "";
                userItem.readerName = "";
                userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                WxUserDatabase.Current.Add(userItem);
            }
            else
            {
                userItem.readerBarcode = strBarcode;
                userItem.readerName = name;
                userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                lRet = WxUserDatabase.Current.Update(userItem);
            }

            return 1;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="weiXinId"></param>
        /// <param name="strError"></param>
        /// <returns>
        /// -1 出错
        /// 0   本来就未绑定，不需解绑
        /// 1   解除绑定成功
        /// </returns>
        public int Unbinding(string weixinId,
            string strReaderBarcode, out string strError)
        {
            strError = "";

            // 从mongodb删除
            long nCount = WxUserDatabase.Current.Delete(weixinId, strReaderBarcode);

            //getPatronInfo setPatronInfo

            /*
                // 得到原读者记录与时间戳
                GetReaderInfoResponse response = channel.GetReaderInfo(strReaderBarcode, "xml");
                if (response.GetReaderInfoResult.Value != 1)
                {
                    strError = "根据路径得到读者记录异常：" + response.GetReaderInfoResult.ErrorInfo;
                    return -1;
                }
                string strRecPath = response.strRecPath;
                string strTimestamp = StringUtil.GetHexTimeStampString(response.baTimestamp);
                string strXml = response.results[0];

                // 修改xml中的email字段，去掉weixin:***
                // 改为读者的email字段
                XmlDocument readerDom = new XmlDocument();
                readerDom.LoadXml(strXml);
                XmlNode emailNode = readerDom.SelectSingleNode("//email");
                string email = emailNode.InnerText.Trim();
                string strEmailLeft = email;
                string strEmailLRight = "";
                int nIndex = email.IndexOf(dp2CommandUtility.C_WeiXinIdPrefix);//"weixinid:");
                if (nIndex >= 0)
                {
                    strEmailLeft = email.Substring(0, nIndex);
                    string strOldWeixinId = email.Substring(nIndex);
                    nIndex = strOldWeixinId.IndexOf(',');
                    if (nIndex > 0)
                    {
                        strEmailLRight = strOldWeixinId.Substring(nIndex);
                        strOldWeixinId = strOldWeixinId.Substring(0, nIndex);
                    }
                    strEmailLeft = TrimComma(strEmailLeft);
                    strEmailLRight = TrimComma(strEmailLRight);
                }
                email = strEmailLeft;
                if (strEmailLRight != "")
                {
                    if (email != "")
                        email += ",";
                    email += strEmailLRight;
                }
                emailNode.InnerText = email;
                string strNewXml = ConvertXmlToString(readerDom);

                // 更新到读者库
                long lRet = channel.SetReaderInfoForWeiXin(strRecPath,
                    strNewXml,
                    strTimestamp,
                    out strError);
                if (lRet == -1)
                {
                    strError = "解除绑定出错：" + strError;
                    return -1;
                }

            */


            return 1;



        }

        #endregion
    }
}
