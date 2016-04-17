using DigitalPlatform.IO;
using DigitalPlatform.Message;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Xml;
using dp2Command.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace dp2Command.Server
{
    public class dp2CmdService2:dp2BaseCommandService
    {
        MessageConnectionCollection _channels = new MessageConnectionCollection();


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
        public string dp2MServerUrl = "";
        public string userName = "";
        public string password = "";






        public void Init(string dp2MServerUrl,
            string userName,
            string password,
            string weiXinUrl,
            string weiXinLogDir,
            string mongoDbConnStr,
            string instancePrefix)
        {
            this.dp2MServerUrl = dp2MServerUrl;
            this.userName = userName;
            this.password = password;
            this.weiXinUrl = weiXinUrl;
            this.weiXinLogDir = weiXinLogDir;

            _channels.Login += _channels_Login;

            // 使用mongodb存储微信用户与读者绑定关系
            WxUserDatabase.Current.Open(mongoDbConnStr, instancePrefix);
        }

        void _channels_Login(object sender, LoginEventArgs e)
        {
            MessageConnection connection = sender as MessageConnection;

            if (string.IsNullOrEmpty(this.userName) == true)
                throw new Exception("尚未指定用户名，无法进行登录");

            MessageResult result = connection.LoginAsync(
                this.userName,
                this.password,
                "",
                "",
                "property").Result;
            if (result.Value == -1)
            {
                throw new Exception(result.ErrorInfo);
            }
        }



        #region 绑定解绑


        /// <summary>
        /// 
        /// </summary>
        /// <param name="strBarcode"></param>
        /// <param name="strPassword"></param>
        /// <param name="weiXinId"></param>
        /// <returns>
        /// -1 出错
        /// 0 成功
        /// </returns>
        public override int Binding(string strBarcode,
            string strPassword,
            string strWeiXinId,
            out string strReaderBarcode,
            out string strError)
        {
            strError = "";
            strReaderBarcode = "";
            long lRet = -1;


            CancellationToken cancel_token = new CancellationToken();

            string fullWeixinId = dp2CommandUtility.C_WeiXinIdPrefix + strWeiXinId;
            string id = Guid.NewGuid().ToString();
            BindPatronRequest request = new BindPatronRequest(id,
                "bind",
                strBarcode,
                strPassword,
                fullWeixinId,
               "single",
                "xml");

            try
            {
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    this.remoteUserName).Result;


                BindPatronResult result = connection.BindPatronAsync(
                     this.remoteUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;

                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }

                string xml = result.Results[0];
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(xml);

                // 绑定成功，把读者证条码记下来，用于续借 2015/11/7，不要用strbarcode变量，因为可能做的大小写转换
                strReaderBarcode = DomUtil.GetNodeText(dom.DocumentElement.SelectSingleNode("barcode"));

                // 将关系存到mongodb库
                string name = "";
                XmlNode node = dom.DocumentElement.SelectSingleNode("name");
                if (node != null)
                    name = DomUtil.GetNodeText(node);
                string refID = "";
                node = dom.DocumentElement.SelectSingleNode("refID");
                if (node != null)
                    refID = DomUtil.GetNodeText(node);

                // 找到库中对应的记录
                WxUserItem userItem = WxUserDatabase.Current.GetOneOrEmptyPatron(strWeiXinId, this.libCode, strBarcode);
                if (userItem == null)
                {
                    strError = "异常情况：对于大公众号不可能出现未选择图书馆就绑定读者的情况。";
                    return -1;
                }

                userItem.readerBarcode = strBarcode;
                userItem.readerName = name;
                userItem.xml = xml;
                userItem.refID = refID;
                userItem.updateTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                userItem.isActive = 1;
                lRet = WxUserDatabase.Current.Update(userItem);

                // 置为活动状态
                WxUserDatabase.Current.SetActive(userItem);
                return 0;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

        ERROR1:
            return -1;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="weiXinId"></param>
        /// <param name="strError"></param>
        /// <returns>
        /// -1 出错
        /// 0   成功
        /// </returns>
        public override int Unbinding1(string strBarcode,
            string strWeiXinId,
             out string strError)
        {
            strError = "";

            // 从mongodb删除
            long nCount = WxUserDatabase.Current.Delete(strWeiXinId, strBarcode,this.libCode);


            // 调点对点解绑接口
            string fullWeixinId = dp2CommandUtility.C_WeiXinIdPrefix + strWeiXinId;
            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            BindPatronRequest request = new BindPatronRequest(id,
                "unbind",
                strBarcode,
                "",//password
                fullWeixinId,
               "single,null_password",
                "xml");
            try
            {
                // 得到连接
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    this.remoteUserName).Result;

                // 调绑定函数，todo为啥await没反应
                BindPatronResult result = connection.BindPatronAsync(
                     this.remoteUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }

                return 0;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

        ERROR1:
            return -1;
        }

        public override long SearchPatronByWeiXinId(string strWeiXinId,
            out string strBarcode,
            out string strError)
        {
            strError = "";
            strBarcode = "";

            // 从mongodb中检查是否绑定了用户
            WxUserItem userItem = WxUserDatabase.Current.GetOne(strWeiXinId,this.libCode);
            if (userItem == null)
            {
                strError = "异常的情况，未怎么图书馆时不应走到SearchPatronByWeiXinId函数。";
                return -1;
            }

            // mongodb存在
            if ( String.IsNullOrEmpty(userItem.readerBarcode)==false)
            {
                strBarcode = userItem.readerBarcode;
                return 1;
            }

            // 从远程dp2library中查
            string strWord = dp2CommandUtility.C_WeiXinIdPrefix + strWeiXinId;
            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
                "searchPatron",
                "<全部>",
                strWord,
                "email",
                "left",
                "wx-patron",
                "id,cols",
                1000,
                0,
                C_Search_MaxCount);
            try
            {
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    this.remoteUserName).Result;

                SearchResult result = connection.SearchAsync(
                    this.remoteUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                if (result.ResultCount == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }
                if (result.ResultCount == 0)
                    return 0;

                // 找到对应的读者记录
                string fristBarcode = "";
                if (result.ResultCount > 0)
                {
                    for (int i = 0; i < result.ResultCount; i++)
                    {
                        // 可能会检索出多笔记录，先取第一笔 todo
                        string strXml = result.Records[i].Data;
                        XmlDocument dom = new XmlDocument();
                        dom.LoadXml(strXml);
                        string strTempBarcode = DomUtil.GetNodeText(dom.DocumentElement.SelectSingleNode("barcode"));

                        // 更新到mongodb库
                        string name = "";
                        XmlNode node = dom.DocumentElement.SelectSingleNode("name");
                        if (node != null)
                            name = DomUtil.GetNodeText(node);
                        string refID = "";
                        node = dom.DocumentElement.SelectSingleNode("refID");
                        if (node != null)
                            refID = DomUtil.GetNodeText(node);

                        if (i == 0)
                        {
                            userItem.readerBarcode = strTempBarcode;
                            userItem.readerName = name;
                            userItem.xml = strXml;
                            userItem.refID = refID;
                            userItem.updateTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                            WxUserDatabase.Current.Update(userItem);
                            //将第一笔设为活动状态
                            WxUserDatabase.Current.SetActive(userItem);
                            //返回的strBarcode //todo refID
                            strBarcode = strTempBarcode;
                        }
                        else
                        {
                            userItem = new WxUserItem();
                            userItem.weixinId = strWeiXinId;
                            userItem.libCode = this.libCode;
                            userItem.libUserName = this.remoteUserName;
                            userItem.readerBarcode = strTempBarcode;
                            userItem.readerName = name;
                            userItem.xml = strXml;
                            userItem.refID = refID;
                            userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                            userItem.updateTime = userItem.createTime;
                            WxUserDatabase.Current.Add(userItem);
                        }
                    }

                    return 1;
                }

            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
        ERROR1:
            return -1;
        }


        #endregion

        #region 检索书目

        public override long SearchBiblio(string strWord,
            SearchCommand searchCmd,
            out string strFirstPage,
            out string strError)
        {
            strFirstPage = "";
            strError = "";

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
                "searchBiblio",
                "<全部>",
                strWord,
                "title",
                "middle",
                "test",
                "id,cols",
                1000,
                0,
                C_Search_MaxCount);
            try
            {
                MessageConnection connection = this._channels.GetConnectionAsync(
                    this.dp2MServerUrl,
                    this.remoteUserName).Result;

                SearchResult result = connection.SearchAsync(
                    this.remoteUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;

                return result.ResultCount;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
        ERROR1:
            return -1;
        }



        public override int GetDetailBiblioInfo(SearchCommand searchCmd,
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

    }
}
