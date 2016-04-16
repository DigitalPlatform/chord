using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;

using TestClient1.Properties;

using DigitalPlatform.Message;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using System.Web;

namespace TestClient1
{
    public partial class MainForm : Form
    {
        MessageConnectionCollection _channels = new MessageConnectionCollection();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _channels.Login += _channels_Login;
            LoadSettings();
        }

        string GetUserName()
        {
#if NO
            if (this.InvokeRequired)
                return (string)this.Invoke(new Func<string>(GetUserName));
            else
#endif
            return this.textBox_config_userName.Text;
        }


        string GetPassword()
        {
#if NO
            if (this.InvokeRequired)
                return (string)this.Invoke(new Func<string>(GetPassword));
            else
#endif
            return this.textBox_config_password.Text;
        }

        void _channels_Login(object sender, LoginEventArgs e)
        {
            MessageConnection connection = sender as MessageConnection;

            string strUserName = GetUserName();
            string strPassword = GetPassword();
            if (string.IsNullOrEmpty(strUserName) == true)
                throw new Exception("尚未指定用户名，无法进行登录");

            MessageResult result = connection.LoginAsync(
                strUserName,
                strPassword,
                "",
                "",
                "property").Result;
            if (result.Value == -1)
            {
                throw new Exception(result.ErrorInfo);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveSettings();
            _channels.Login -= _channels_Login;
        }

        void LoadSettings()
        {
            this.textBox_config_messageServerUrl.Text = Settings.Default.config_url;
            this.textBox_config_userName.Text = Settings.Default.config_userName;
            this.textBox_config_password.Text = Settings.Default.config_password;

            this.comboBox_getInfo_method.Text = Settings.Default.getInfo_method;
            this.textBox_getInfo_remoteUserName.Text = Settings.Default.getInfo_remoteUserName;
            this.textBox_getInfo_queryWord.Text = Settings.Default.getInfo_queryWord;
            this.textBox_getInfo_formatList.Text = Settings.Default.getInfo_formatList;

            this.comboBox_search_method.Text = Settings.Default.search_method;
            this.textBox_search_remoteUserName.Text = Settings.Default.search_remoteUserName;
            this.textBox_search_dbNameList.Text = Settings.Default.search_dbNameList;
            this.textBox_search_queryWord.Text = Settings.Default.search_queryWord;
            this.textBox_search_use.Text = Settings.Default.search_use;
            this.textBox_search_matchStyle.Text = Settings.Default.search_matchStyle;
            this.textBox_search_formatList.Text = Settings.Default.search_formatList;
            this.textBox_search_resultSetName.Text = Settings.Default.search_resultSetName;
            this.textBox_search_position.Text = Settings.Default.search_position;

            this.textBox_bindPatron_remoteUserName.Text = Settings.Default.bindPatron_remoteUserName;
            this.comboBox_bindPatron_action.Text = Settings.Default.bindPatron_action;
            this.textBox_bindPatron_queryWord.Text = Settings.Default.bindPatron_queryWord;
            this.textBox_bindPatron_password.Text = Settings.Default.bindPatron_password;
            this.textBox_bindPatron_bindingID.Text = Settings.Default.bindPatron_bindingID;
            this.textBox_bindPatron_style.Text = Settings.Default.bindPatron_style;
            this.textBox_bindPatron_resultTypeList.Text = Settings.Default.bindPatron_resultTypeList;

        }

        void SaveSettings()
        {
            Settings.Default.config_url = this.textBox_config_messageServerUrl.Text;
            Settings.Default.config_userName = this.textBox_config_userName.Text;
            Settings.Default.config_password = this.textBox_config_password.Text;

            Settings.Default.getInfo_method = this.comboBox_getInfo_method.Text;
            Settings.Default.getInfo_remoteUserName = this.textBox_getInfo_remoteUserName.Text;
            Settings.Default.getInfo_queryWord = this.textBox_getInfo_queryWord.Text;
            Settings.Default.getInfo_formatList = this.textBox_getInfo_formatList.Text;

            Settings.Default.search_method = this.comboBox_search_method.Text;
            Settings.Default.search_remoteUserName = this.textBox_search_remoteUserName.Text;
            Settings.Default.search_dbNameList = this.textBox_search_dbNameList.Text;
            Settings.Default.search_queryWord = this.textBox_search_queryWord.Text;
            Settings.Default.search_use = this.textBox_search_use.Text;
            Settings.Default.search_matchStyle = this.textBox_search_matchStyle.Text;
            Settings.Default.search_formatList = this.textBox_search_formatList.Text;
            Settings.Default.search_resultSetName = this.textBox_search_resultSetName.Text;
            Settings.Default.search_position = this.textBox_search_position.Text;

            Settings.Default.bindPatron_remoteUserName = this.textBox_bindPatron_remoteUserName.Text;
            Settings.Default.bindPatron_action = this.comboBox_bindPatron_action.Text;
            Settings.Default.bindPatron_queryWord = this.textBox_bindPatron_queryWord.Text;
            Settings.Default.bindPatron_password = this.textBox_bindPatron_password.Text;
            Settings.Default.bindPatron_bindingID = this.textBox_bindPatron_bindingID.Text;
            Settings.Default.bindPatron_style = this.textBox_bindPatron_style.Text;
            Settings.Default.bindPatron_resultTypeList = this.textBox_bindPatron_resultTypeList.Text;

            Settings.Default.Save();
        }

        private void toolStripButton_begin_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == tabPage_getInfo)
            {
                // Task.Factory.StartNew(() => DoGetPatronInfo());
                DoGetInfo();
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_search)
            {
                // Task.Factory.StartNew(() => DoSearchPatron());
                DoSearch();
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_bindPatron)
            {
                DoBindPatron();
            }
        }

        void EnableControls(bool bEnable)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool>(EnableControls), bEnable);
                return;
            }

            this.tabControl_main.Enabled = bEnable;
            this.toolStrip1.Enabled = bEnable;
        }

        async void DoBindPatron()
        {
            string strError = "";

            SetTextString(this.webBrowser1, "");

            if (string.IsNullOrEmpty(this.comboBox_bindPatron_action.Text) == true)
            {
                strError = "尚未指定 Action";
                goto ERROR1;
            }

            EnableControls(false);
            try
            {
                CancellationToken cancel_token = new CancellationToken();

                string id = Guid.NewGuid().ToString();
                BindPatronRequest request = new BindPatronRequest(id,
                    this.comboBox_bindPatron_action.Text,
                    this.textBox_bindPatron_queryWord.Text,
                    this.textBox_bindPatron_password.Text,
                    this.textBox_bindPatron_bindingID.Text,
                    this.textBox_bindPatron_style.Text,
                    this.textBox_bindPatron_resultTypeList.Text);
                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsync(
                        this.textBox_config_messageServerUrl.Text,
                        this.textBox_search_remoteUserName.Text);
                    BindPatronResult result = await connection.BindPatronAsync(
                        this.textBox_search_remoteUserName.Text,
                        request,
                        new TimeSpan(0, 1, 0),
                        cancel_token);

                    this.Invoke(new Action(() =>
                    {
                        if (result.Value == -1)
                            SetTextString(this.webBrowser1, "出错: " + result.ErrorInfo);
                        else
                            SetTextString(this.webBrowser1, ToString(result));
                    }));
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
                return;
            }
            finally
            {
                EnableControls(true);
            }
        ERROR1:
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
        }

        async void DoSearch()
        {
            string strError = "";

            SetTextString(this.webBrowser1, "");

            if (string.IsNullOrEmpty(this.comboBox_search_method.Text) == true)
            {
                strError = "尚未指定方法";
                goto ERROR1;
            }

            long start = 0;
            long count = 0;

            start = StringUtil.GetSubInt64(this.textBox_search_position.Text, ',', 0);
            count = StringUtil.GetSubInt64(this.textBox_search_position.Text, ',', 1);

            EnableControls(false);
            try
            {
                CancellationToken cancel_token = new CancellationToken();

                string id = Guid.NewGuid().ToString();
                SearchRequest request = new SearchRequest(id,
                    this.comboBox_search_method.Text,
                    this.textBox_search_dbNameList.Text,
                    this.textBox_search_queryWord.Text,
                    this.textBox_search_use.Text,
                    this.textBox_search_matchStyle.Text,
                    this.textBox_search_resultSetName.Text,
                    this.textBox_search_formatList.Text,
                    1000,
                    start,
                    count);
                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsync(
                        this.textBox_config_messageServerUrl.Text,
                        this.textBox_search_remoteUserName.Text);
                    SearchResult result = await connection.SearchAsync(
                        this.textBox_search_remoteUserName.Text,
                        request,
                        new TimeSpan(0, 1, 0),
                        cancel_token);

                    this.Invoke(new Action(() =>
                    {
                        if (result.ResultCount == 0)
                            SetTextString(this.webBrowser1, "没有找到");
                        else
                            SetTextString(this.webBrowser1, ToString(result));
                    }));
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
                return;
            }
            finally
            {
                EnableControls(true);
            }
        ERROR1:
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
        }

        async void DoGetInfo()
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.comboBox_getInfo_method.Text) == true)
            {
                strError = "尚未指定方法";
                goto ERROR1;
            }

            EnableControls(false);
            try
            {
                CancellationToken cancel_token = new CancellationToken();

                string id = Guid.NewGuid().ToString();
                SearchRequest request = new SearchRequest(id,
                    this.comboBox_getInfo_method.Text,
                    "",
                    this.textBox_getInfo_queryWord.Text,
                    "",
                    "",
                    "",
                    this.textBox_getInfo_formatList.Text,
                    1,
                    0,
                    -1);

                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsync(
                        this.textBox_config_messageServerUrl.Text,
                        this.textBox_getInfo_remoteUserName.Text);

                    SearchResult result = await connection.SearchAsync(
                        this.textBox_getInfo_remoteUserName.Text,
                        request,
                        new TimeSpan(0, 1, 0),
                        cancel_token);

                    this.Invoke(new Action(() =>
                    {
                        if (result.ResultCount == 0)
                            SetTextString(this.webBrowser1, "没有找到");
                        else
                            SetTextString(this.webBrowser1, ToString(result));
                    }));
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
                return;
            }
            finally
            {
                EnableControls(true);
            }
        ERROR1:
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
        }

        public static void SetHtmlString(WebBrowser webBrowser,
string strHtml)
        {
            webBrowser.DocumentText = strHtml;
        }

        static void SetTextString(WebBrowser webBrowser, string strText)
        {
            SetHtmlString(webBrowser, "<pre>" + HttpUtility.HtmlEncode(strText) + "</pre>");
        }
#if NO
        void DoGetPatronInfo()
        {
            string strError = "";

            EnableControls(false);
            try
            {
                CancellationToken cancel_token = new CancellationToken();

                string id = Guid.NewGuid().ToString();
                SearchRequest request = new SearchRequest(id,
                    "getPatronInfo",
                    "",
                    this.textBox_getReaderInfo_queryWord.Text,
                    "",
                    "",
                    this.textBox_getReaderInfo_formatList.Text, 1);

                try
                {
                    Task<MessageConnection> task = this._channels.GetConnectionAsync(
                        this.textBox_config_messageServerUrl.Text,
                        this.textBox_getReaderInfo_remoteUserName.Text);
                    SearchResult result =
                    task.ContinueWith<SearchResult>((antecendent) =>
                    {
                        if (antecendent.IsFaulted == true)
                        {
                            throw antecendent.Exception;
                        }
                        MessageConnection connection = task.Result;
                        return connection.SearchAsync(
                        this.textBox_getReaderInfo_remoteUserName.Text,
                        request,
                        new TimeSpan(0, 1, 0),
                        cancel_token).Result;
                    }).Result;

                    this.Invoke(new Action(() =>
                    {
                        if (result.ResultCount == 0)
                            this.textBox_getReaderInfo_results.Text = "没有找到";
                        else
                            this.textBox_getReaderInfo_results.Text = ToString(result);
                    }));
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
                return;
            }
            finally
            {
                EnableControls(true);
            }
        ERROR1:
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
        }
#endif
        static string ToString(BindPatronResult result)
        {
            StringBuilder text = new StringBuilder();
            text.Append("ResultValue=" + result.Value + "\r\n");
            text.Append("ErrorInfo=" + result.ErrorInfo + "\r\n");
            if (result.Results != null)
            {
                text.Append("Results.Count=" + result.Results.Count + "\r\n");
                int i = 0;
                foreach (string s in result.Results)
                {
                    text.Append((i + 1).ToString() + ") ===");
                    text.Append("Data=" + XmlUtil.TryGetIndentXml(s) + "\r\n");
                    i++;
                }
            }

            return text.ToString();
        }

        static string ToString(SearchResult result)
        {
            StringBuilder text = new StringBuilder();
            text.Append("ResultCount=" + result.ResultCount + "\r\n");
            text.Append("ErrorInfo=" + result.ErrorInfo + "\r\n");
            if (string.IsNullOrEmpty(result.ErrorCode) == false)
                text.Append("ErrorCode=" + result.ErrorCode + "\r\n");
            if (result.Records != null)
            {
                int i = 0;
                foreach (Record record in result.Records)
                {
                    text.Append((i + 1).ToString() + ") ===");
                    if (string.IsNullOrEmpty(record.RecPath) == false)
                        text.Append("RecPath=" + record.RecPath + "\r\n");
                    if (string.IsNullOrEmpty(record.Format) == false)
                        text.Append("Format=" + record.Format + "\r\n");
                    text.Append("Data=" + XmlUtil.TryGetIndentXml(record.Data) + "\r\n");

                    if (string.IsNullOrEmpty(record.Timestamp) == false)
                        text.Append("Timestamp=" + record.Timestamp + "\r\n");

                    i++;
                }
            }

            return text.ToString();
        }
    }
}
