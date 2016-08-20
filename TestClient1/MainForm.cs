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
using System.Xml;
using System.Messaging;
using System.Net;
using System.IO;

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
            StreamWriter sw = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "trace.txt"));
            sw.AutoFlush = true;
            _channels.TraceWriter = sw;

            ClearForPureTextOutputing(this.webBrowser_message);

            LoadSettings();

            // ServicePointManager.DefaultConnectionLimit = 10;
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
                text.Append("action=" + HttpUtility.HtmlEncode(e.Action) + "\r\n");
                text.Append("id=" + HttpUtility.HtmlEncode(record.id) + "\r\n");
                text.Append("data=" + HttpUtility.HtmlEncode(record.data) + "\r\n");
                if (record.groups != null)
                    text.Append("groups=" + HttpUtility.HtmlEncode(string.Join(",", record.groups)) + "\r\n");
                text.Append("creator=" + HttpUtility.HtmlEncode(record.creator) + "\r\n");
                text.Append("userName=" + HttpUtility.HtmlEncode(record.userName) + "\r\n");

                text.Append("format=" + HttpUtility.HtmlEncode(record.format) + "\r\n");
                text.Append("type=" + HttpUtility.HtmlEncode(record.type) + "\r\n");
                text.Append("thread=" + HttpUtility.HtmlEncode(record.thread) + "\r\n");
                
                if (record.subjects != null)
                    text.Append("subjects=" + HttpUtility.HtmlEncode(string.Join(SUBJECT_DELIM, record.subjects)) + "\r\n");

                text.Append("publishTime=" + HttpUtility.HtmlEncode(record.publishTime) + "\r\n");
                text.Append("expireTime=" + HttpUtility.HtmlEncode(record.expireTime) + "\r\n");
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

            e.UserName = GetUserName();
            if (string.IsNullOrEmpty(e.UserName) == true)
                throw new Exception("尚未指定用户名，无法进行登录");

            e.Password = GetPassword();
            e.Parameters = "propertyList=biblio_search,libraryUID=testclient1";

            // TODO: 登录如果失败，界面会有提示么?
#if NO
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
#endif
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        CancellationTokenSource _cancel = new CancellationTokenSource();

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancel.Cancel();

            SaveSettings();
            _channels.AddMessage -= _channels_AddMessage;
            _channels.Login -= _channels_Login;
            _channels.TraceWriter.Close();
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

            this.textBox_message_groupName.Text = Settings.Default.message_groupName;
            this.textBox_message_timeRange.Text = Settings.Default.message_timeRange;
            this.textBox_message_userRange.Text = Settings.Default.message_userRange;
            this.textBox_message_sortCondition.Text = Settings.Default.message_sortCondition;

            this.textBox_getRes_remoteUserName.Text = Settings.Default.getRes_remoteUserName;
            this.comboBox_getRes_operation.Text = Settings.Default.getRes_operation;
            this.textBox_getRes_path.Text = Settings.Default.getRes_path;
            this.textBox_getRes_start.Text = Settings.Default.getRes_start;
            this.textBox_getRes_length.Text = Settings.Default.getRes_length;
            this.textBox_getRes_style.Text = Settings.Default.getRes_style;
            this.textBox_getRes_outputFile.Text = Settings.Default.getRes_outputFile;

            this.textBox_markdown_source.Text = Settings.Default.markdown_source;
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

            Settings.Default.message_groupName = this.textBox_message_groupName.Text;
            Settings.Default.message_timeRange = this.textBox_message_timeRange.Text;
            Settings.Default.message_userRange = this.textBox_message_userRange.Text;
            Settings.Default.message_sortCondition = this.textBox_message_sortCondition.Text;

            Settings.Default.getRes_remoteUserName = this.textBox_getRes_remoteUserName.Text;
            Settings.Default.getRes_operation = this.comboBox_getRes_operation.Text;
            Settings.Default.getRes_path = this.textBox_getRes_path.Text;
            Settings.Default.getRes_start = this.textBox_getRes_start.Text;
            Settings.Default.getRes_length = this.textBox_getRes_length.Text;
            Settings.Default.getRes_style = this.textBox_getRes_style.Text;
            Settings.Default.getRes_outputFile = this.textBox_getRes_outputFile.Text;

            Settings.Default.markdown_source = this.textBox_markdown_source.Text;

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

            if (this.tabControl_main.SelectedTab == this.tabPage_getRes)
            {
                DoGetRes2();
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_markdown)
            {
                DisplayMarkDown();
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
                    MessageConnection connection = await this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "");
                    CirculationResult result = await connection.CirculationAsyncLite(
                        this.textBox_circulation_remoteUserName.Text,
                        request,
                        new TimeSpan(0, 1, 10), // 10 秒
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
                    MessageConnection connection = await this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "");
                    SetInfoResult result = await connection.SetInfoAsyncLite(
                        this.textBox_setInfo_remoteUserName.Text,
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
                    MessageConnection connection = await this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "");
                    BindPatronResult result = await connection.BindPatronAsyncLite(
                        this.textBox_bindPatron_remoteUserName.Text,
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
                    MessageConnection connection = await this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "");
                    GetConnectionInfoResult result = await connection.GetConnectionInfoAsyncLite(
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
                    MessageConnection connection = await this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "");
                    SearchResult result = await connection.SearchAsyncLite(
                        this.textBox_search_remoteUserName.Text,
                        request,
                        new TimeSpan(0, 1, 0),
                        cancel_token);

                    Thread.Sleep(1000);
                    this.Invoke(new Action(() =>
                    {
                        if (result.ResultCount == 0)
                            SetTextString(this.webBrowser1, "没有找到\r\n" + ToString(result));
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
                    MessageConnection connection = await this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "");

                    Task<SearchResult> task1 = connection.SearchTaskAsync(
                        this.textBox_getInfo_remoteUserName.Text,
                        request1,
                        new TimeSpan(0, 1, 0),
                        cancel_token);
                    Task<SearchResult> task2 = connection.SearchTaskAsync(
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
                    MessageConnection connection = await this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "");

                    SearchResult result = await connection.SearchAsyncLite(
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
                        "");
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

        static string ToString(GetMessageResult result)
        {
            StringBuilder text = new StringBuilder();

            text.Append(ToString(result as MessageResult));

            if (result.Results != null)
            {
                int i = 0;
                foreach (MessageRecord record in result.Results)
                {
                    text.Append("*** " + (i + 1) + "\r\n");
                    text.Append("id=" + record.id + "\r\n");
                    text.Append("data=" + record.data + "\r\n");
                    if (record.groups != null)
                        text.Append("groups=" + string.Join(",", record.groups) + "\r\n");
                    text.Append("creator=" + record.creator + "\r\n");
                    text.Append("userName=" + record.userName + "\r\n");

                    text.Append("format=" + record.format + "\r\n");
                    text.Append("type=" + record.type + "\r\n");
                    text.Append("thread=" + record.thread + "\r\n");

                    if (record.subjects != null)
                        text.Append("subjects=" + (string.Join(SUBJECT_DELIM, record.subjects)) + "\r\n");

                    text.Append("publishTime=" + record.publishTime.ToString("G") + "\r\n");
                    text.Append("expireTime=" + record.expireTime + "\r\n");

                    i++;
                }
            }

            return text.ToString();
        }

        const string SUBJECT_DELIM = "|";

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
                    if (record.groups != null)
                        text.Append("groups=" + string.Join(",", record.groups) + "\r\n");
                    text.Append("creator=" + record.creator + "\r\n");
                    text.Append("userName=" + record.userName + "\r\n");
                    text.Append("publishTime=" + record.publishTime + "\r\n");
                    text.Append("expireTime=" + record.expireTime + "\r\n");

                    if (record.subjects != null)
                        text.Append("subjects=" + string.Join(SUBJECT_DELIM, record.subjects) + "\r\n");

                    i++;
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
            text.Append("PatronBarcode=" + result.PatronBarcode + "\r\n");
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
                        text.Append("LibraryUID=" + record.LibraryUID + "\r\n");
                    if (string.IsNullOrEmpty(record.LibraryName) == false)
                        text.Append("LibraryName=" + record.LibraryName + "\r\n");
                    if (string.IsNullOrEmpty(record.LibraryUserName) == false)
                        text.Append("LibraryUserName=" + record.LibraryUserName + "\r\n");

                    if (string.IsNullOrEmpty(record.PropertyList) == false)
                        text.Append("PropertyList=" + record.PropertyList + "\r\n");
                    if (string.IsNullOrEmpty(record.ClientIP) == false)
                        text.Append("ClientIP=" + record.ClientIP + "\r\n");

                    i++;
                }
            }

            return text.ToString();
        }

        static string ToString(GetResResponse result)
        {
            StringBuilder text = new StringBuilder();

            text.Append("TotalLength=" + result.TotalLength + "\r\n");
            text.Append("Start=" + result.Start + "\r\n");
            text.Append("Path=" + result.Path + "\r\n");
            text.Append("Data.Length=" + (result.Data == null ? 0 : result.Data.Length) + "\r\n");
            text.Append("Metadata=" + result.Metadata + "\r\n");
            text.Append("Timestamp=" + result.Timestamp + "\r\n");

            text.Append("ErrorInfo=" + result.ErrorInfo + "\r\n");
            text.Append("ErrorCode=" + result.ErrorCode + "\r\n");
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
            DoSendMessage(this.textBox_message_groupName.Text,
                this.textBox_message_text.Text,
                null);
        }

        async void DoSendMessage(string strGroupName,
            string strText,
            string[] subjects)
        {
            string strError = "";

            if (string.IsNullOrEmpty(strText) == true)
            {
                strError = "尚未输入文本";
                goto ERROR1;
            }

            SetTextString(this.webBrowser1, "");

            List<MessageRecord> records = new List<MessageRecord>();
            if (strText == "*")
            {
                for (int i = 0; i < 400; i++)
                {
                    MessageRecord record = new MessageRecord();
                    record.groups = strGroupName.Split(new char[] { ',' });
                    record.creator = "";    // 服务器会自己填写
                    record.data = i.ToString();
                    record.format = "text";
                    record.type = "message";
                    record.thread = "";
                    record.subjects = subjects;
                    record.expireTime = new DateTime(0);    // 表示永远不失效
                    records.Add(record);
                }
            }
            else
            {
                MessageRecord record = new MessageRecord();
                record.groups = strGroupName.Split(new char[] { ',' });
                record.creator = "";    // 服务器会自己填写
                record.data = strText;
                record.format = "text";
                record.type = "message";
                record.thread = "";
                record.subjects = subjects;
                record.expireTime = new DateTime(0);    // 表示永远不失效
                records.Add(record);
            }

            EnableControls(false);
            try
            {
                // CancellationToken cancel_token = new CancellationToken();

                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "");
                    SetMessageRequest param = new SetMessageRequest("create",
                        "",
                        records);

                    SetMessageResult result = await connection.SetMessageAsyncLite(param,
                        new TimeSpan(0, 1, 0),
                        this._cancel.Token);

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
            bool bControl = Control.ModifierKeys == Keys.Control;
            if (bControl)
                DoLoadMessage2(this.textBox_message_groupName.Text,
                    this.textBox_message_userRange.Text,
                this.textBox_message_timeRange.Text,
                this.textBox_message_sortCondition.Text,
                this.textBox_message_text.Text,
                "");
            else
                DoLoadMessage(
                    "",
                    this.textBox_message_groupName.Text,
                    this.textBox_message_userRange.Text,
                    this.textBox_message_timeRange.Text,
                this.textBox_message_sortCondition.Text,
                this.textBox_message_text.Text,
                "");
        }

        void FillMessage(long totalCount,
            long start,
            IList<MessageRecord> records,
            string errorInfo,
            string errorCode)
        {
            if (this.webBrowser_message.InvokeRequired)
            {
                this.webBrowser_message.Invoke(new Action<long, long, IList<MessageRecord>, string, string>(FillMessage),
                    totalCount, start, records, errorInfo, errorCode);
                return;
            }

            if (totalCount == -1)
            {
                StringBuilder text = new StringBuilder();
                text.Append("***\r\n");
                text.Append("totalCount=" + totalCount + "\r\n");
                text.Append("errorInfo=" + errorInfo + "\r\n");
                text.Append("errorCode=" + errorCode + "\r\n");

                AppendHtml(this.webBrowser_message, text.ToString());
            }

            if (records != null)
            {
                foreach (MessageRecord record in records)
                {
                    StringBuilder text = new StringBuilder();
                    text.Append("***\r\n");
                    text.Append("id=" + HttpUtility.HtmlEncode(record.id) + "\r\n");
                    text.Append("data=" + HttpUtility.HtmlEncode(record.data) + "\r\n");
                    if (record.groups != null)
                        text.Append("groups=" + HttpUtility.HtmlEncode(string.Join(",", record.groups)) + "\r\n");
                    text.Append("creator=" + HttpUtility.HtmlEncode(record.creator) + "\r\n");
                    text.Append("userName=" + HttpUtility.HtmlEncode(record.userName) + "\r\n");

                    text.Append("format=" + HttpUtility.HtmlEncode(record.format) + "\r\n");
                    text.Append("type=" + HttpUtility.HtmlEncode(record.type) + "\r\n");
                    text.Append("thread=" + HttpUtility.HtmlEncode(record.thread) + "\r\n");

                    if (record.subjects != null)
                        text.Append("subjects=" + HttpUtility.HtmlEncode(string.Join(SUBJECT_DELIM, record.subjects)) + "\r\n");

                    text.Append("publishTime=" + HttpUtility.HtmlEncode(record.publishTime.ToString("G")) + "\r\n");
                    text.Append("expireTime=" + HttpUtility.HtmlEncode(record.expireTime) + "\r\n");
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

        async void DoLoadMessage(
            string strAction,
            string strGroupCondition,
            string strUserCondition,
            string strTimeRange,
            string strSortCondition,
            string strIdContidion,
            string strSubjectCondition)
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
                    strAction,
                    strGroupCondition, // "" 表示默认群组
                    strUserCondition,
                    strTimeRange,
                    strSortCondition,
                    strIdContidion,
                    strSubjectCondition,
                    0,
                    -1);
                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "");
                    MessageResult result = await connection.GetMessageAsyncLite(
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

        // 同步阻塞版本
        void DoLoadMessage1(string strGroupCondition,
            string strTimeRange,
            string strSortCondition,
            string strIdCondition,
            string strSubjectCondition)
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
                    "",
                    strGroupCondition, // "" 表示默认群组
                    "",
                    strTimeRange,
                    strSortCondition,
                    strIdCondition,
                    strSubjectCondition,
                    0,
                    -1);
                try
                {
                    MessageConnection connection = this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "").Result;
                    GetMessageResult result = connection.GetMessage(
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

        async void DoLoadMessage2(string strGroupCondition,
            string strUserCondition,
            string strTimeRange,
            string strSortCondition,
            string strIdCondition,
            string strSubjectCondition)
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
                    "",
                    strGroupCondition, // "" 表示默认群组
                    strUserCondition,
                    strTimeRange,
                    strSortCondition,
                    strIdCondition,
                    strSubjectCondition,
                    0,
                    -1);
                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "");
                    GetMessageResult result = await connection.GetMessageAsyncLite(
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

        // 将当前正在使用的通道切断。迫使后面重新连接
        private void textBox_config_messageServerUrl_TextChanged(object sender, EventArgs e)
        {
            this._channels.Clear();
#if NO
            try
            {
                MessageConnection connection = this._channels.GetConnectionAsync(
        this.textBox_config_messageServerUrl.Text,
        "",
        false).Result;
                if (connection != null)
                    connection.CloseConnection();
            }
            catch
            {

            }
#endif
        }

        private void MenuItem_writeToMSMQ_Click(object sender, EventArgs e)
        {
            string strError = "";

            string strQueuePath = ".\\private$\\myQueue";
            string strRecipient = Guid.NewGuid().ToString();
            string strBody = "adfasdfasdfasdfasdfasdfasdf";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");
            DomUtil.SetElementText(dom.DocumentElement, "type", "patronNotify");
            DomUtil.SetElementText(dom.DocumentElement, "recipient", strRecipient);
            DomUtil.SetElementText(dom.DocumentElement, "body", strBody);
            DomUtil.SetElementText(dom.DocumentElement, "mime", "text");

            try
            {
                MessageQueue myQueue = new MessageQueue(strQueuePath);

                System.Messaging.Message myMessage = new System.Messaging.Message();
                myMessage.Body = dom.DocumentElement.OuterXml;
                myMessage.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                myQueue.Send(myMessage);
            }
            catch (Exception ex)
            {
                strError = "发送消息到 MQ 失败: " + ex.Message;
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_message_transGroupName_Click(object sender, EventArgs e)
        {
            DoGetGroupName(this.textBox_message_groupName.Text,
                "transGroupName");
        }

        // parameters:
        //      strAction   getGroupName/getGroupNameQuick
        async void DoGetGroupName(string strGroupCondition,
            string strAction)
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
                    strAction,
                    strGroupCondition, //
                    "",
                    "",
                    "",
                    "",
                    "",
                    0,
                    -1);
                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "");
                    MessageResult result = await connection.GetMessageAsyncLite(
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

        private void button_message_getGroupNameQuick_Click(object sender, EventArgs e)
        {
            DoGetGroupName(this.textBox_message_groupName.Text,
    "transGroupNameQuick");
        }

        private void button_message_enumGroupName_Click(object sender, EventArgs e)
        {
            DoGetGroupName(this.textBox_message_groupName.Text,
"enumGroupName");
        }

        private void button_message_delete_Click(object sender, EventArgs e)
        {
            bool bControl = Control.ModifierKeys == Keys.Control;

            DoDeleteMessage(bControl ? "expire" : "delete",
                this.textBox_message_groupName.Text,
                this.textBox_config_userName.Text,
                this.textBox_message_text.Text);
        }

        // parameters:
        //      strUserName 用于过滤的用户名。如果为空则表示不过滤
        async Task<List<string>> GetAllMessageID(string strGroupCondition,
            string strUserCondition)
        {
            string strError = "";

            List<string> results = new List<string>();

            EnableControls(false);
            try
            {
                CancellationToken cancel_token = this._cancel.Token;  //  new CancellationToken();

                string id = Guid.NewGuid().ToString();
                GetMessageRequest request = new GetMessageRequest(id,
                    "",
                    strGroupCondition,
                    strUserCondition,
                    "", // strTimeRange,
                    "",
                    "",
                    "",
                    0,
                    -1);
                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "");
                    MessageResult result = await connection.GetMessageAsyncLite(
                        request,
                        (totalCount,
            start,
            records,
            errorInfo,
            errorCode) =>
                        {
                            foreach (MessageRecord record in records)
                            {
                                results.Add(record.id);
                            }
                        },
                        new TimeSpan(0, 1, 0),
                        cancel_token);
                    this.Invoke(new Action(() =>
                    {
                        SetTextString(this.webBrowser1, ToString(result));
                    }));
                    return results;
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
            }
            finally
            {
                EnableControls(true);
            }
        ERROR1:
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
            return results;
        }

        // parameters:
        //      strUserName 用于过滤的用户名(仅删除此用户创建的消息)。如果为空则表示不过滤，即全部删除
        async void DoDeleteMessage(
            string strAction,
            string strGroupName,
            string strUserName,
            string strMessageIDList)
        {
            string strError = "";

            if (string.IsNullOrEmpty(strMessageIDList) == true)
            {
                strError = "尚未输入要删除的消息 ID";
                goto ERROR1;
            }

            string[] ids = null;
            if (strMessageIDList == "*")
            {
                // 表示希望全部删除
                ids = (await GetAllMessageID(strGroupName, strUserName)).ToArray();
            }
            else
            {
                SetTextString(this.webBrowser1, "");

                ids = strMessageIDList.Replace("\r\n", ",").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }

            List<MessageRecord> records = new List<MessageRecord>();
            foreach (string id in ids)
            {
                MessageRecord record = new MessageRecord();
                record.groups = strGroupName.Split(new char[] { ',' });
                record.id = id;
                records.Add(record);
            }

            EnableControls(false);
            try
            {
                // CancellationToken cancel_token = new CancellationToken();

                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "");
                    SetMessageRequest param = new SetMessageRequest(strAction,  // "delete",
                        "",
                        records);

                    SetMessageResult result = await connection.SetMessageAsyncLite(param,
                        new TimeSpan(0, 1, 0),
                        this._cancel.Token);

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

        #region 任延华测试代码

        private long GetSummaryAndItems(string dp2mserverUrl,
    string remoteUserName,
    string biblioPath,
    out string info,
    out string strError)
        {
            strError = "";
            info = "";

            // 取出summary
            string strSummary = "";
            int nRet = this.GetBiblioSummary(this.textBox_config_messageServerUrl.Text,
                this.textBox_getInfo_remoteUserName.Text,
                this.textBox_getInfo_queryWord.Text,
                out strSummary, out strError);
            if (nRet == -1 || nRet == 0)
            {
                return nRet;
            }

            // 取item
            string itemList = "";
            nRet = (int)this.GetItemInfo(this.textBox_config_messageServerUrl.Text,
                this.textBox_getInfo_remoteUserName.Text,
                this.textBox_getInfo_queryWord.Text, out itemList, out strError);
            if (nRet == -1) //0的情况表示没有册，不是错误
            {
                return -1;
            }

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
            string id = Guid.NewGuid().ToString();
            SearchRequest request = new SearchRequest(id,
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
                MessageConnection connection = this._channels.GetConnectionAsyncLite(
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

        #endregion

        private void menuItem_getSummaryAndItems_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strList = "";

            this.tabControl_main.SelectedTab = this.tabPage_getInfo;

            for (int i = 0; i < 1; i++)
            {
                long nRet = GetSummaryAndItems(this.textBox_config_messageServerUrl.Text,
                    this.textBox_getInfo_remoteUserName.Text,
                    this.textBox_getInfo_queryWord.Text,    // string biblioPath,
        out strList,
        out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            MessageBox.Show(this, "list:" + strList);
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        // 缓冲区版本
        async void DoGetRes1()
        {
            string strError = "";

            SetTextString(this.webBrowser1, "");

            if (string.IsNullOrEmpty(this.comboBox_getRes_operation.Text) == true)
            {
                strError = "尚未指定方法";
                goto ERROR1;
            }

            long start = 0;
            long length = 0;

            start = Convert.ToInt64(this.textBox_getRes_start.Text);
            length = Convert.ToInt64(this.textBox_getRes_length.Text);

            EnableControls(false);
            try
            {
                CancellationToken cancel_token = new CancellationToken();

                string id = Guid.NewGuid().ToString();
                GetResRequest request = new GetResRequest(id,
                    this.comboBox_getRes_operation.Text,
                    this.textBox_getRes_path.Text,
                    start,
                    length,
                    this.textBox_getRes_style.Text);
                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "");
                    GetResResponse result = await connection.GetResAsyncLite(
                        this.textBox_search_remoteUserName.Text,
                        request,
                        new TimeSpan(0, 1, 0),
                        cancel_token);

                    if (result.Data != null
                        && string.IsNullOrEmpty(this.textBox_getRes_outputFile.Text) == false)
                    {
                        using (Stream output = File.Create(this.textBox_getRes_outputFile.Text))
                        {
                            output.Write(result.Data, 0, result.Data.Length);
                        }
                    }

                    // Thread.Sleep(1000);
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

        // 流版本
        async void DoGetRes2()
        {
            string strError = "";

            SetTextString(this.webBrowser1, "");

            if (string.IsNullOrEmpty(this.comboBox_getRes_operation.Text) == true)
            {
                strError = "尚未指定方法";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_getRes_outputFile.Text))
            {
                strError = "尚未指定输出文件名";
                goto ERROR1;
            }

            long start = 0;
            long length = 0;

            start = Convert.ToInt64(this.textBox_getRes_start.Text);
            length = Convert.ToInt64(this.textBox_getRes_length.Text);

            EnableControls(false);
            try
            {
                CancellationToken cancel_token = _cancel.Token; // new CancellationToken();

                string id = Guid.NewGuid().ToString();
                GetResRequest request = new GetResRequest(id,
                    this.comboBox_getRes_operation.Text,
                    this.textBox_getRes_path.Text,
                    start,
                    length,
                    this.textBox_getRes_style.Text);
                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsyncLite(
                        this.textBox_config_messageServerUrl.Text,
                        "");

                    using (Stream output = File.Create(this.textBox_getRes_outputFile.Text))
                    {
                        GetResResponse result = await connection.GetResAsyncLite(
                            this.textBox_search_remoteUserName.Text,
                            request,
                            output,
                            setProgress,
                            new TimeSpan(0, 1, 0),
                            cancel_token);

                        // Thread.Sleep(1000);
                        this.Invoke(new Action(() =>
                        {
                            SetTextString(this.webBrowser1, ToString(result));
                        }));
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
                return;
            }
            finally
            {
                EnableControls(true);
            }
        ERROR1:
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
        }

        void setProgress(long totalLength, long current)
        {

            if (this.InvokeRequired)
            {
                this.Invoke(new Action<long, long>(setProgress), totalLength, current);
            }

            int width = 1000;

            double ratio = (double)width / (double)totalLength;

            if (this.toolStripProgressBar1.Minimum != width)
            {
                this.toolStripProgressBar1.Maximum = width;
                this.toolStripProgressBar1.Minimum = 0;
            }
            this.toolStripProgressBar1.Value = (int)(current * (double)ratio);

            this.toolStripStatusLabel1.Text = current.ToString() + " / " + totalLength;
        }

        void DisplayMarkDown()
        {
            var result = CommonMark.CommonMarkConverter.Convert(this.textBox_markdown_source.Text);

            ClearForHtmlOutputing(this.webBrowser1);
            AppendHtml(this.webBrowser1, result);
        }

        public static void ClearForHtmlOutputing(WebBrowser webBrowser)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                Navigate(webBrowser, "about:blank");  // 2015/7/28
                doc = webBrowser.Document;
            }

            doc = doc.OpenNew(true);
            doc.Write("<html><body>");
        }

        private void ToolStripMenuItem_sendMessage_Click(object sender, EventArgs e)
        {
            SendMessageDialog dlg = new SendMessageDialog();

            dlg.UiState = Settings.Default.sendMessageDialog_ui;
            dlg.ShowDialog(this);
            Settings.Default.sendMessageDialog_ui = dlg.UiState;
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.tabControl_main.SelectedTab = this.tabPage_message;
            DoSendMessage(dlg.GroupName,
            dlg.Data,
            dlg.Subjects);
        }

        private void ToolStripMenuItem_getMessage_Click(object sender, EventArgs e)
        {
            GetMessageDialog dlg = new GetMessageDialog();

            dlg.UiState = Settings.Default.getMessageDialog_ui;
            dlg.ShowDialog(this);
            Settings.Default.getMessageDialog_ui = dlg.UiState;
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.tabControl_main.SelectedTab = this.tabPage_message;

            DoLoadMessage(
                "",
                dlg.GroupCondition,
                dlg.UserCondition,
                dlg.TimeCondition,
                dlg.SortCondition,
                dlg.IdCondition,
                dlg.SubjectCondition);
        }

        private void ToolStripMenuItem_enumSubject_Click(object sender, EventArgs e)
        {
            GetMessageDialog dlg = new GetMessageDialog();

            dlg.UiState = Settings.Default.getMessageDialog_ui;
            dlg.ShowDialog(this);
            Settings.Default.getMessageDialog_ui = dlg.UiState;
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.tabControl_main.SelectedTab = this.tabPage_message;

            DoLoadMessage(
                "enumSubject",
                dlg.GroupCondition,
                dlg.UserCondition,
                dlg.TimeCondition,
                dlg.SortCondition,
                dlg.IdCondition,
                dlg.SubjectCondition);
        }

        private void ToolStripMenuItem_enumCreator_Click(object sender, EventArgs e)
        {
            GetMessageDialog dlg = new GetMessageDialog();

            dlg.UiState = Settings.Default.getMessageDialog_ui;
            dlg.ShowDialog(this);
            Settings.Default.getMessageDialog_ui = dlg.UiState;
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.tabControl_main.SelectedTab = this.tabPage_message;

            DoLoadMessage(
                "enumCreator",
                dlg.GroupCondition,
                dlg.UserCondition,
                dlg.TimeCondition,
                dlg.SortCondition,
                dlg.IdCondition,
                dlg.SubjectCondition);
        }
    }
}
