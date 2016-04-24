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
using System.Web;

using TestClient1.Properties;

using DigitalPlatform.Message;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform;

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
            _channels.AddMessage += _channels_AddMessage;

            ClearForPureTextOutputing(this.webBrowser_message);

            LoadSettings();
        }

        void _channels_AddMessage(object sender, AddMessageEventArgs e)
        {
            this.BeginInvoke(new Action<AddMessageEventArgs>(DisplayMessage), e);
        }

        void DisplayMessage(AddMessageEventArgs e)
        {
            StringBuilder text = new StringBuilder();
            foreach (MessageRecord record in e.Records)
            {
                text.Append("***\r\n");
                text.Append("action=" + e.Action + "\r\n");
                text.Append("id=" + record.id + "\r\n");
                text.Append("data=" + record.data + "\r\n");
                text.Append("group=" + record.group + "\r\n");
                text.Append("creator=" + record.creator + "\r\n");

                text.Append("format=" + record.format + "\r\n");
                text.Append("type=" + record.type + "\r\n");
                text.Append("thread=" + record.thread + "\r\n");

                text.Append("publishTime=" + record.publishTime + "\r\n");
                text.Append("expireTime=" + record.expireTime + "\r\n");
            }

            AppendHtml(this.webBrowser_message, text.ToString());
        }

        public void AppendHtml(WebBrowser webBrowser, string strText)
        {
            WriteHtml(webBrowser,
                strText);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            webBrowser.Document.Window.ScrollTo(0,
                webBrowser.Document.Body.ScrollRectangle.Height);
        }

        // 不支持异步调用
        public static void WriteHtml(WebBrowser webBrowser,
    string strHtml)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
            }

            // doc = doc.OpenNew(true);
            doc.Write(strHtml);

            // 保持末行可见
            // ScrollToEnd(webBrowser);
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


            LoginRequest param = new LoginRequest();
            param.UserName = GetUserName();
            if (string.IsNullOrEmpty(param.UserName) == true)
                throw new Exception("尚未指定用户名，无法进行登录");

            param.Password = GetPassword();
            MessageResult result = connection.LoginAsync(param).Result;
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

            this.comboBox_setInfo_method.Text = Settings.Default.setInfo_method;
            this.textBox_setInfo_remoteUserName.Text = Settings.Default.setInfo_remoteUserName;
            this.comboBox_setInfo_action.Text = Settings.Default.setInfo_action;
            this.textBox_setInfo_biblioRecPath.Text = Settings.Default.setInfo_biblioRecPath;

            this.textBox_circulation_remoteUserName.Text = Settings.Default.circulation_remoteUserName;
            this.comboBox_circulation_operation.Text = Settings.Default.circulation_operation;
            this.textBox_circulation_patron.Text = Settings.Default.circulation_patron;
            this.textBox_circulation_item.Text = Settings.Default.circulation_item;
            this.textBox_circulation_style.Text = Settings.Default.circulation_style;
            this.textBox_circulation_patronFormatList.Text = Settings.Default.circulation_patronFormatList;
            this.textBox_circulation_itemFormatList.Text = Settings.Default.circulation_itemFormatList;
            this.textBox_circulation_biblioFormatList.Text = Settings.Default.circulation_biblioFormatList;
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

            Settings.Default.setInfo_method = this.comboBox_setInfo_method.Text;
            Settings.Default.setInfo_remoteUserName = this.textBox_setInfo_remoteUserName.Text;
            Settings.Default.setInfo_action = this.comboBox_setInfo_action.Text;
            Settings.Default.setInfo_biblioRecPath = this.textBox_setInfo_biblioRecPath.Text;

            Settings.Default.circulation_remoteUserName = this.textBox_circulation_remoteUserName.Text;
            Settings.Default.circulation_operation = this.comboBox_circulation_operation.Text;
            Settings.Default.circulation_patron = this.textBox_circulation_patron.Text;
            Settings.Default.circulation_item = this.textBox_circulation_item.Text;
            Settings.Default.circulation_style = this.textBox_circulation_style.Text;
            Settings.Default.circulation_patronFormatList = this.textBox_circulation_patronFormatList.Text;
            Settings.Default.circulation_itemFormatList = this.textBox_circulation_itemFormatList.Text;
            Settings.Default.circulation_biblioFormatList = this.textBox_circulation_biblioFormatList.Text;

            Settings.Default.Save();
        }

        private void toolStripButton_begin_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == tabPage_getInfo)
            {
                if (this.comboBox_getInfo_method.Text == "getBiblioInfo"
                    && this.checkBox_getInfo_getSubEntities.Checked)
                    DoGetBiblioAndSub();
                else
                    DoGetInfo();
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_search)
            {
                if (this.comboBox_search_method.Text == "GetConnectionInfo")
                    DoGetConnectionInfo();
                else
                    DoSearch();
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_bindPatron)
            {
                DoBindPatron();
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_setInfo)
            {
                DoSetInfo();
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_circulation)
            {
                DoCirculation();
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

        async void DoCirculation()
        {
            string strError = "";

            SetTextString(this.webBrowser1, "");

            if (string.IsNullOrEmpty(this.comboBox_circulation_operation.Text) == true)
            {
                strError = "尚未指定 Operation";
                goto ERROR1;
            }

            EnableControls(false);
            try
            {
                CancellationToken cancel_token = new CancellationToken();

                string id = Guid.NewGuid().ToString();
                CirculationRequest request = new CirculationRequest(id,
                    this.comboBox_circulation_operation.Text,
                    this.textBox_circulation_patron.Text,
                    this.textBox_circulation_item.Text,
                    this.textBox_circulation_style.Text,
                    this.textBox_circulation_patronFormatList.Text,
                    this.textBox_circulation_itemFormatList.Text,
                    this.textBox_circulation_biblioFormatList.Text);
                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsync(
                        this.textBox_config_messageServerUrl.Text,
                        this.textBox_search_remoteUserName.Text);
                    CirculationResult result = await connection.CirculationAsync(
                        this.textBox_search_remoteUserName.Text,
                        request,
                        new TimeSpan(0, 0, 10), // 10 秒
                        cancel_token);

                    this.Invoke(new Action(() =>
                    {
#if NO
                        if (result.Value == -1)
                            SetTextString(this.webBrowser1, "出错: " + result.ErrorInfo);
                        else
                            SetTextString(this.webBrowser1, ToString(result));
#endif
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

        async void DoSetInfo()
        {
            string strError = "";

            SetTextString(this.webBrowser1, "");

            // TODO: 建立即将发送的对象数组
            // 是否要刷新 refID? 是否要整理 parent 元素内容?
            // action 要设置到每个对象
            List<Entity> entities = null;

            EnableControls(false);
            try
            {
                CancellationToken cancel_token = new CancellationToken();

                string id = Guid.NewGuid().ToString();
                SetInfoRequest request = new SetInfoRequest(id,
                    this.comboBox_setInfo_method.Text,
                    this.textBox_setInfo_biblioRecPath.Text,
                    entities);
                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsync(
                        this.textBox_config_messageServerUrl.Text,
                        this.textBox_search_remoteUserName.Text);
                    SetInfoResult result = await connection.SetInfoAsync(
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

        async void DoGetConnectionInfo()
        {
            string strError = "";

            SetTextString(this.webBrowser1, "");

            long start = 0;
            long count = 0;

            start = StringUtil.GetSubInt64(this.textBox_search_position.Text, ',', 0);
            count = StringUtil.GetSubInt64(this.textBox_search_position.Text, ',', 1);

            EnableControls(false);
            try
            {
                CancellationToken cancel_token = new CancellationToken();

                string id = Guid.NewGuid().ToString();
                GetConnectionInfoRequest request = new GetConnectionInfoRequest(id,
                    this.textBox_search_dbNameList.Text,    // operation
                    this.textBox_search_queryWord.Text,
                    this.textBox_search_formatList.Text,
                    1000,
                    start,
                    count);
                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsync(
                        this.textBox_config_messageServerUrl.Text,
                        this.textBox_search_remoteUserName.Text);
                    GetConnectionInfoResult result = await connection.GetConnectionInfoAsync(
                        request,
                        new TimeSpan(0, 1, 0),
                        cancel_token);

                    this.Invoke(new Action(() =>
                    {
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

        async void DoGetBiblioAndSub()
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
                DateTime start_time = DateTime.Now;

                CancellationToken cancel_token = new CancellationToken();

                // 获取书目记录
                string id1 = Guid.NewGuid().ToString();
                SearchRequest request1 = new SearchRequest(id1,
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
                // 获取下属记录
                string id2 = Guid.NewGuid().ToString();
                SearchRequest request2 = new SearchRequest(id2,
    "getItemInfo",
    "entity",
    this.textBox_getInfo_queryWord.Text,
    "",
    "",
    "",
    "", // "xml",
    1,
    0,
    -1);

                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsync(
                        this.textBox_config_messageServerUrl.Text,
                        this.textBox_getInfo_remoteUserName.Text);

                    Task<SearchResult> task1 = connection.SearchAsync(
                        this.textBox_getInfo_remoteUserName.Text,
                        request1,
                        new TimeSpan(0, 1, 0),
                        cancel_token);
                    Task<SearchResult> task2 = connection.SearchAsync(
                        this.textBox_getInfo_remoteUserName.Text,
                        request2,
                        new TimeSpan(0, 1, 0),
                        cancel_token);

                    Task<SearchResult>[] tasks = new Task<SearchResult>[2];
                    tasks[0] = task1;
                    tasks[1] = task2;
                    Task.WaitAll(tasks);


                    TimeSpan time_length = DateTime.Now - start_time;

                    this.Invoke(new Action(() =>
                    {
                        string strResultText = "time span: " + time_length.TotalSeconds.ToString() + " secs"
                            + "\r\n=== biblio ===\r\n" + ToString(task1.Result)
                            + "\r\n\r\n=== entities ===\r\n" + ToString(task2.Result);
                        SetTextString(this.webBrowser1, strResultText);
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
                DateTime start_time = DateTime.Now;

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

                    TimeSpan time_length = DateTime.Now - start_time;

                    this.Invoke(new Action(() =>
                    {
                        string strResultText = "time span: " + time_length.TotalSeconds.ToString() + " secs"
                            + "\r\n" + ToString(result);
#if NO
                        if (result.ResultCount == 0)
                            SetTextString(this.webBrowser1, "没有找到");
                        else
#endif
                        SetTextString(this.webBrowser1, strResultText);
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
        static string ToString(MessageResult result)
        {
            StringBuilder text = new StringBuilder();
            text.Append("ResultValue=" + result.Value + "\r\n");
            text.Append("ErrorInfo=" + result.ErrorInfo + "\r\n");
            text.Append("String=" + result.String + "\r\n");
            return text.ToString();
        }

        static string ToString(SetMessageResult result)
        {
            StringBuilder text = new StringBuilder();
            text.Append("ResultValue=" + result.Value + "\r\n");
            text.Append("ErrorInfo=" + result.ErrorInfo + "\r\n");
            text.Append("String=" + result.String + "\r\n");
            if (result.Results != null)
            {
                text.Append("Results.Count=" + result.Results.Count + "\r\n");
                int i = 0;
                foreach (MessageRecord record in result.Results)
                {
                    text.Append((i + 1).ToString() + ") ===");
                    text.Append("id=" + record.id + "\r\n");
                    text.Append("creator=" + record.creator + "\r\n");
                    text.Append("publishTime=" + record.publishTime + "\r\n");
                    text.Append("expireTime=" + record.expireTime + "\r\n");
                }
            }
            return text.ToString();
        }

        static string ToString(CirculationResult result)
        {
            StringBuilder text = new StringBuilder();
            text.Append("ResultValue=" + result.Value + "\r\n");
            text.Append("ErrorInfo=" + result.ErrorInfo + "\r\n");
            text.Append("String=" + result.String + "\r\n");
            if (result.PatronResults != null)
            {
                text.Append("PatronResults.Count=" + result.PatronResults.Count + "\r\n");
                int i = 0;
                foreach (string s in result.PatronResults)
                {
                    text.Append((i + 1).ToString() + ") ===");
                    text.Append("Data=" + XmlUtil.TryGetIndentXml(s) + "\r\n");  // TODO: 如果是 XML 可显示为缩进形态
                    i++;
                }
            }
            if (result.ItemResults != null)
            {
                text.Append("ItemResults.Count=" + result.ItemResults.Count + "\r\n");
                int i = 0;
                foreach (string s in result.ItemResults)
                {
                    text.Append((i + 1).ToString() + ") ===");
                    text.Append("Data=" + XmlUtil.TryGetIndentXml(s) + "\r\n");
                    i++;
                }
            }
            if (result.BiblioResults != null)
            {
                text.Append("BiblioResults.Count=" + result.BiblioResults.Count + "\r\n");
                int i = 0;
                foreach (string s in result.BiblioResults)
                {
                    text.Append((i + 1).ToString() + ") ===");
                    text.Append("Data=" + XmlUtil.TryGetIndentXml(s) + "\r\n");
                    i++;
                }
            }
            if (result.BorrowInfo != null)
            {
                text.Append("BorrowInfo.LatestReturnTime=" + result.BorrowInfo.LatestReturnTime + "\r\n");
                text.Append("BorrowInfo.Period=" + result.BorrowInfo.Period + "\r\n");
                text.Append("BorrowInfo.BorrowCount=" + result.BorrowInfo.BorrowCount + "\r\n");
                text.Append("BorrowInfo.BorrowOperator=" + result.BorrowInfo.BorrowOperator + "\r\n");
            }
            if (result.ReturnInfo != null)
            {
                text.Append("ReturnInfo.BorrowTime=" + result.ReturnInfo.BorrowTime + "\r\n");
                text.Append("ReturnInfo.LatestReturnTime=" + result.ReturnInfo.LatestReturnTime + "\r\n");
                text.Append("ReturnInfo.Period=" + result.ReturnInfo.Period + "\r\n");

                text.Append("ReturnInfo.BorrowCount=" + result.ReturnInfo.BorrowCount + "\r\n");
                text.Append("ReturnInfo.OverdueString=" + result.ReturnInfo.OverdueString + "\r\n");
                text.Append("ReturnInfo.BorrowOperator=" + result.ReturnInfo.BorrowOperator + "\r\n");

                text.Append("ReturnInfo.ReturnOperator=" + result.ReturnInfo.ReturnOperator + "\r\n");
                text.Append("ReturnInfo.BookType=" + result.ReturnInfo.BookType + "\r\n");
                text.Append("ReturnInfo.Location=" + result.ReturnInfo.Location + "\r\n");
            }
            return text.ToString();
        }

        static string ToString(SetInfoResult result)
        {
            StringBuilder text = new StringBuilder();
            text.Append("ResultValue=" + result.Value + "\r\n");
            text.Append("ErrorInfo=" + result.ErrorInfo + "\r\n");
            if (result.Entities != null)
            {
                text.Append("Entities.Count=" + result.Entities.Count + "\r\n");
                int i = 0;
                foreach (Entity entity in result.Entities)
                {
                    text.Append((i + 1).ToString() + ") ===");
                    text.Append("NewRecord.RecPath=" + entity.NewRecord.RecPath + "\r\n");
                    text.Append("NewRecord.Data=" + XmlUtil.TryGetIndentXml(entity.NewRecord.Data) + "\r\n");
                    text.Append("NewRecord.Timestamp=" + entity.NewRecord.Timestamp + "\r\n");

                    text.Append("OldRecord.RecPath=" + entity.OldRecord.RecPath + "\r\n");
                    text.Append("OldRecord.Data=" + XmlUtil.TryGetIndentXml(entity.OldRecord.Data) + "\r\n");
                    text.Append("OldRecord.Timestamp=" + entity.OldRecord.Timestamp + "\r\n");

                    text.Append("RefID=" + entity.RefID + "\r\n");
                    text.Append("ErrorInfo=" + entity.ErrorInfo + "\r\n");
                    text.Append("ErrorCode=" + entity.ErrorCode + "\r\n");

                    i++;
                }
            }

            return text.ToString();
        }

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

        static string ToString(GetConnectionInfoResult result)
        {
            StringBuilder text = new StringBuilder();
            text.Append("ResultCount=" + result.ResultCount + "\r\n");
            text.Append("ErrorInfo=" + result.ErrorInfo + "\r\n");
            if (string.IsNullOrEmpty(result.ErrorCode) == false)
                text.Append("ErrorCode=" + result.ErrorCode + "\r\n");
            if (result.Records != null)
            {
                int i = 0;
                foreach (ConnectionRecord record in result.Records)
                {
                    text.Append((i + 1).ToString() + ") ===");
                    if (string.IsNullOrEmpty(record.User.userName) == false)
                        text.Append("User.userName=" + record.User.userName + "\r\n");
                    if (string.IsNullOrEmpty(record.User.rights) == false)
                        text.Append("User.rights=" + record.User.rights + "\r\n");
                    if (string.IsNullOrEmpty(record.User.duty) == false)
                        text.Append("User.duty=" + record.User.duty + "\r\n");

                    if (string.IsNullOrEmpty(record.LibraryUID) == false)
                        text.Append("PropertyList=" + record.LibraryUID + "\r\n");
                    if (string.IsNullOrEmpty(record.LibraryName) == false)
                        text.Append("PropertyList=" + record.LibraryName + "\r\n");

                    if (string.IsNullOrEmpty(record.PropertyList) == false)
                        text.Append("PropertyList=" + record.PropertyList + "\r\n");

                    i++;
                }
            }

            return text.ToString();
        }


        List<string> _entities = new List<string>();

        private void button_pasteEntities_Click(object sender, EventArgs e)
        {
            string strError = "";

            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null
                || iData.GetDataPresent("xml") == false)
            {
                strError = "剪贴板中尚不存在 xml 类型数据";
                goto ERROR1;
            }

            List<string> xmls = (List<string>)iData.GetData("xml");
            if (xmls == null)
            {
                strError = "iData.GetData() return null";
                goto ERROR1;
            }

            _entities = xmls;

            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (string xml in xmls)
            {
                text.Append((i + 1).ToString() + ")\r\n" + DomUtil.GetIndentXml(xml) + "\r\n\r\n");
                i++;
            }
            SetTextString(this.webBrowser_setInfo_entities, text.ToString());
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void comboBox_getInfo_method_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox_getInfo_method.Text == "getBiblioInfo")
                this.checkBox_getInfo_getSubEntities.Enabled = true;
            else
                this.checkBox_getInfo_getSubEntities.Enabled = false;
        }

        private void button_message_send_Click(object sender, EventArgs e)
        {
            DoSendMessage(this.textBox_message_text.Text);
        }

        async void DoSendMessage(string strText)
        {
            string strError = "";

            if (string.IsNullOrEmpty(strText) == true)
            {
                strError = "尚未输入文本";
                goto ERROR1;
            }

            SetTextString(this.webBrowser1, "");

            List<MessageRecord> records = new List<MessageRecord>();
            MessageRecord record = new MessageRecord();
            record.group = "";
            record.creator = "";    // 服务器会自己填写
            record.data = strText;
            record.format = "text";
            record.type = "message";
            record.thread = "";
            record.expireTime = new DateTime(0);    // 表示永远不失效
            records.Add(record);

            EnableControls(false);
            try
            {
                // CancellationToken cancel_token = new CancellationToken();

                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsync(
                        this.textBox_config_messageServerUrl.Text,
                        this.textBox_search_remoteUserName.Text);
                    SetMessageResult result = await connection.SetMessageAsync(
                        "create",
                        records);

                    this.Invoke(new Action(() =>
                    {
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

        // 装载以前的所有消息
        private void button_message_load_Click(object sender, EventArgs e)
        {
            DoLoadMessage("");
        }

        void FillMessage(long totalCount,
            long start,
            IList<MessageRecord> records,
            string errorInfo,
            string errorCode)
        {
            if (this.webBrowser_message.InvokeRequired)
            {
                this.webBrowser_message.Invoke(new Action<long, long, IList<MessageRecord>, string, string >(FillMessage),
                    totalCount, start, records, errorInfo, errorCode);
                return;
            }

            if (records != null)
            {
                foreach (MessageRecord record in records)
                {
                    StringBuilder text = new StringBuilder();
                    text.Append("***\r\n");
                    text.Append("id=" + record.id + "\r\n");
                    text.Append("data=" + record.data + "\r\n");
                    text.Append("group=" + record.group + "\r\n");
                    text.Append("creator=" + record.creator + "\r\n");

                    text.Append("format=" + record.format + "\r\n");
                    text.Append("type=" + record.type + "\r\n");
                    text.Append("thread=" + record.thread + "\r\n");

                    text.Append("publishTime=" + record.publishTime + "\r\n");
                    text.Append("expireTime=" + record.expireTime + "\r\n");
                    AppendHtml(this.webBrowser_message, text.ToString());
                }
            }
        }

        public static void ClearForPureTextOutputing(WebBrowser webBrowser)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                // webBrowser.Navigate("about:blank");
                Navigate(webBrowser, "about:blank");  // 2015/7/28
                doc = webBrowser.Document;
            }

            doc = doc.OpenNew(true);
            doc.Write("<pre>");
        }

        // 2015/7/28 
        // 能处理异常的 Navigate
        internal static void Navigate(WebBrowser webBrowser, string urlString)
        {
            int nRedoCount = 0;
        REDO:
            try
            {
                webBrowser.Navigate(urlString);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Application.DoEvents(); // 2015/8/13
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }

                throw ex;
            }
        }


        async void DoLoadMessage(string strGroupName)
        {
            string strError = "";

            ClearForPureTextOutputing(this.webBrowser_message);
            SetTextString(this.webBrowser1, "");

            EnableControls(false);
            try
            {
                CancellationToken cancel_token = new CancellationToken();

                string id = Guid.NewGuid().ToString();
                GetMessageRequest request = new GetMessageRequest(id,
                    strGroupName, // "" 表示默认群组
                    "",
                    "",
                    0, 
                    -1);
                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsync(
                        this.textBox_config_messageServerUrl.Text,
                        this.textBox_search_remoteUserName.Text);
                    MessageResult result = await connection.GetMessageAsync(
                        request,
                        FillMessage,
                        new TimeSpan(0, 1, 0),
                        cancel_token);
                    this.Invoke(new Action(() =>
                    {
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

    }
}
