using System;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Message;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Xml;

namespace testSearch
{
    public partial class Form1 : Form
    {

        MessageConnectionCollection _channels = new MessageConnectionCollection();
        string _dp2mserverUrl = "http://dp2003.com:8083/dp2mserver";
        string _remoteUserName = "dp2capo";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _channels.Login += _channels_Login;

        }

        void _channels_Login(object sender, LoginEventArgs e)
        {
            MessageConnection connection = sender as MessageConnection;
            e.UserName = "weixinclient";
            e.Password = "1123455";//错误的密码
            e.Parameters = "propertyList=biblio_search,libraryUID=xxx";
        }

        private void btnGetSummaryAndItem_Click(object sender, EventArgs e)
        {
            string strInfo="";
            string strError="";
            long nRet = this.GetSummaryAndItems(this._dp2mserverUrl,
                this._remoteUserName,
                this.txtPath.Text,
                out strInfo,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, "出错:" + strError);
                return;
            }
            if (nRet ==0)
            {
                MessageBox.Show(this, "未命中:" + strError);
                return;
            }
            MessageBox.Show(this, "成功:" + strInfo);
            return;

        }

        private long GetSummaryAndItems(string dp2mserverUrl,
            string remoteUserName,
            string biblioPath,
            out string info,
            out string strError)
        {
            strError = "";
            info = "";
            int nRet = 0;

            // 取item
            string itemList = "";
            nRet = (int)this.GetItemInfo(dp2mserverUrl,
                remoteUserName,
                biblioPath, out itemList, out strError);
            if (nRet == -1) //0的情况表示没有册，不是错误
            {
                return -1;
            }

            // 取出summary
            string strSummary = "";
            //nRet = this.GetBiblioSummary(dp2mserverUrl,
            //    remoteUserName,
            //    biblioPath,
            //    out strSummary, out strError);
            //if (nRet == -1 || nRet == 0)
            //{
            //    return nRet;
            //}

            //Thread.Sleep(1000);



            info = " summary:[" + strSummary + "]\r\nitems:[" + itemList + "]";

            return 1;
        }
        private int GetBiblioSummary(string dp2mserverUrl,
            string remoteUserName,
            string biblioPath,
            out string summary,
            out string strError)
        {
            summary = "";
            strError = "";

            CancellationToken cancel_token = new CancellationToken();
            string id = Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
                null,   // TODO:
                "getBiblioInfo",
                "<全部>",
                biblioPath,
                "",
                "",
                "",
                "summary",
                1,
                0,
                -1);
            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                   dp2mserverUrl,
                    remoteUserName).Result;

                SearchResult result = connection.SearchTaskAsync(
                    remoteUserName,
                    request,
                    new TimeSpan(0, 1, 0),
                    cancel_token).Result;
                if (result.ResultCount == -1)
                {
                    strError = "检索出错：" + result.ErrorInfo;
                    return -1;
                }
                if (result.ResultCount == 0)
                {
                    strError = "未命中";
                    return 0;
                }

                summary = result.Records[0].Data;


                return 1;
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

        private long GetItemInfo(string dp2mserverUrl,
            string remoteUserName,
            string biblioPath,
            out string itemList,
            out string strError)
        {
            itemList = "";
            strError = "";

            CancellationToken cancel_token = new CancellationToken();
            string id = "2-item";// Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
                null, // TODO
                "getItemInfo",
                "entity",
                biblioPath,
                "",
                "",
                "",
                "opac",
                10,
                0,
                -1);
            try
            {
                MessageConnection connection = this._channels.GetConnectionTaskAsync(
                    dp2mserverUrl,
                    remoteUserName).Result;

                SearchResult result = null;
                try
                {
                    result = connection.SearchTaskAsync(
                       remoteUserName,
                       request,
                       new TimeSpan(0, 1, 0),
                       cancel_token).Result;
                }
                catch (Exception ex)
                {
                    strError = "检索出错：[SearchAsync异常]" + ex.Message;
                    return -1;
                }

                if (result.ResultCount == -1)
                {
                    strError = "检索出错：" + result.ErrorInfo;
                    return -1;
                }
                if (result.ResultCount == 0)
                {
                    strError = "未命中";
                    return 0;
                }

                for (int i = 0; i < result.Records.Count; i++)
                {
                    string xml = result.Records[i].Data;
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);

                    string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");
                    // 册条码号
                    string strViewBarcode = "";
                    if (string.IsNullOrEmpty(strBarcode) == false)
                        strViewBarcode = strBarcode;
                    else
                        strViewBarcode = "refID:" + strRefID;  //"@refID:"
                    itemList += strViewBarcode + "\n";

                }

                return result.Records.Count;
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

    }
}
