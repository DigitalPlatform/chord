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

            this.textBox_getReaderInfo_remoteUserName.Text = Settings.Default.getReaderInfo_remoteUserName;
            this.textBox_getReaderInfo_queryWord.Text = Settings.Default.getReaderInfo_queryWord;
            this.textBox_getReaderInfo_formatList.Text = Settings.Default.getReaderInfo_formatList;

            this.textBox_searchPatron_remoteUserName.Text = Settings.Default.searchPatron_remoteUserName;
            this.textBox_searchPatron_dbNameList.Text = Settings.Default.searchPatron_dbNameList;
            this.textBox_searchPatron_queryWord.Text = Settings.Default.searchPatron_queryWord;
            this.textBox_searchPatron_use.Text = Settings.Default.searchPatron_use;
            this.textBox_searchPatron_matchStyle.Text = Settings.Default.searchPatron_matchStyle;
            this.textBox_searchPatron_formatList.Text = Settings.Default.searchPatron_formatList;

        }

        void SaveSettings()
        {
            Settings.Default.config_url = this.textBox_config_messageServerUrl.Text;
            Settings.Default.config_userName = this.textBox_config_userName.Text;
            Settings.Default.config_password = this.textBox_config_password.Text;

            Settings.Default.getReaderInfo_remoteUserName = this.textBox_getReaderInfo_remoteUserName.Text;
            Settings.Default.getReaderInfo_queryWord = this.textBox_getReaderInfo_queryWord.Text;
            Settings.Default.getReaderInfo_formatList = this.textBox_getReaderInfo_formatList.Text;

            Settings.Default.searchPatron_remoteUserName = this.textBox_searchPatron_remoteUserName.Text;
            Settings.Default.searchPatron_dbNameList = this.textBox_searchPatron_dbNameList.Text;
            Settings.Default.searchPatron_queryWord = this.textBox_searchPatron_queryWord.Text;
            Settings.Default.searchPatron_use = this.textBox_searchPatron_use.Text;
            Settings.Default.searchPatron_matchStyle = this.textBox_searchPatron_matchStyle.Text;
            Settings.Default.searchPatron_formatList = this.textBox_searchPatron_formatList.Text;

            Settings.Default.Save();
        }

        private void toolStripButton_begin_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == tabPage_getPatronInfo)
            {
                Task.Factory.StartNew(() => DoGetPatronInfo());
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_searchPatron)
            {
                Task.Factory.StartNew(() => DoSearchPatron());
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

        void DoSearchPatron()
        {
            string strError = "";

            EnableControls(false);
            try
            {
                CancellationToken cancel_token = new CancellationToken();

                string id = Guid.NewGuid().ToString();
                SearchRequest request = new SearchRequest(id,
                    "searchPatron",
                    this.textBox_searchPatron_dbNameList.Text,
                    this.textBox_searchPatron_queryWord.Text,
                    this.textBox_searchPatron_use.Text,
                    this.textBox_searchPatron_matchStyle.Text,
                    this.textBox_searchPatron_formatList.Text,
                    1000);
                try
                {
                    Task<MessageConnection> task = this._channels.GetConnectionAsync(
                        this.textBox_config_messageServerUrl.Text,
                        this.textBox_searchPatron_remoteUserName.Text);
                    SearchResult result =
                    task.ContinueWith<SearchResult>((antecendent) =>
                    {
                        if (antecendent.IsFaulted == true)
                        {
                            throw antecendent.Exception;
                        }
                        MessageConnection connection = task.Result;
                        return connection.SearchAsync(
                        this.textBox_searchPatron_remoteUserName.Text,
                        request,
                        new TimeSpan(0, 1, 0),
                        cancel_token).Result;
                    }).Result;

                    this.Invoke(new Action(() =>
                    {
                        if (result.ResultCount == 0)
                            this.textBox_searchPatron_results.Text = "没有找到";
                        else
                            this.textBox_searchPatron_results.Text = ToString(result);
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

        static string ToString(SearchResult result)
        {
            StringBuilder text = new StringBuilder();
            text.Append("ResultCount=" + result.ResultCount + "\r\n");
            text.Append("ErrorInfo=" + result.ErrorInfo + "\r\n");
            if (result.Records != null)
            {
                int i = 0;
                foreach (Record record in result.Records)
                {
                    text.Append((i + 1).ToString() + ") ===");
                    text.Append("RecPath=" + record.RecPath + "\r\n");
                    text.Append("Format=" + record.Format + "\r\n");
                    text.Append("Data=" + record.Data + "\r\n");
                    i++;
                }
            }

            return text.ToString();
        }
    }
}
