using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Threading.Tasks;

using TestRouter.Properties;

using DigitalPlatform.Message;
using DigitalPlatform.MessageClient;

namespace TestRouter
{
    public partial class Form1 : Form
    {
        CancellationTokenSource _cancel = new CancellationTokenSource();

        MessageConnectionCollection _channels = new MessageConnectionCollection();

        Router _router = new Router();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ClearForPureTextOutputing(this.webBrowser1);

            LoadSettings();
        }


        // 本来是要把消息转发给外部，但这里只能用显示到浏览器控件来模拟了
        void _router_SendMessageEvent(object sender, SendMessageEventArgs e)
        {
            FillMessage(e.Message);
        }

        int _index = 0;

        void FillMessage(MessageRecord record)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                this.webBrowser1.Invoke(new Action<MessageRecord>(FillMessage), record);
                return;
            }

            {
                StringBuilder text = new StringBuilder();
                text.Append("*** " + (_index + 1) + "\r\n");
                _index++;
                text.Append("id=" + record.id + "\r\n");
                text.Append("data=" + record.data + "\r\n");
                if (record.groups != null)
                    text.Append("groups=" + HttpUtility.HtmlEncode(string.Join(",", record.groups)) + "\r\n");
                text.Append("creator=" + record.creator + "\r\n");
                text.Append("userName=" + record.userName + "\r\n");

                text.Append("format=" + record.format + "\r\n");
                text.Append("type=" + record.type + "\r\n");
                text.Append("thread=" + record.thread + "\r\n");

                text.Append("publishTime=" + record.publishTime.ToString("G") + "\r\n");
                text.Append("expireTime=" + record.expireTime + "\r\n");
                AppendHtml(this.webBrowser1, text.ToString());
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this._router.Stop();

            this._cancel.Cancel();  // 中断发送循环

            SaveSettings();
        }

        void LoadSettings()
        {
            this.textBox_messageServerUrl.Text = Settings.Default.messageServerUrl;
            this.textBox_sender_userName.Text = Settings.Default.sender_userName;
            this.textBox_sender_password.Text = Settings.Default.sender_password;

            this.textBox_reciever_userName.Text = Settings.Default.reciever_userName;
            this.textBox_reciever_password.Text = Settings.Default.reciever_password;

            this.textBox_groupName.Text = Settings.Default.groupName;
        }

        void SaveSettings()
        {
            Settings.Default.messageServerUrl = this.textBox_messageServerUrl.Text;
            Settings.Default.sender_userName = this.textBox_sender_userName.Text;
            Settings.Default.sender_password = this.textBox_sender_password.Text;

            Settings.Default.reciever_userName = this.textBox_reciever_userName.Text;
            Settings.Default.reciever_password = this.textBox_reciever_password.Text;

            Settings.Default.groupName = this.textBox_groupName.Text;

            Settings.Default.Save();
        }

        void EnableControls(bool bEnable)
        {
            this.textBox_reciever_userName.Enabled = bEnable;
            this.textBox_reciever_password.Enabled = bEnable;
            if (this.tabControl1.SelectedTab == this.tabPage_reciever)
            {
                this.button_reviever_begin.Enabled = bEnable;
                this.button_reciever_stop.Enabled = !bEnable;
            }
            else
            {
                this.button_reviever_begin.Enabled = bEnable;
                this.button_reciever_stop.Enabled = bEnable;
            }

            this.textBox_sender_userName.Enabled = bEnable;
            this.textBox_sender_password.Enabled = bEnable;
            if (this.tabControl1.SelectedTab == this.tabPage_sender)
            {
                this.button_sender_begin.Enabled = bEnable;
                this.button_sender_stop.Enabled = !bEnable;
            }
            else
            {
                this.button_sender_begin.Enabled = bEnable;
                this.button_sender_stop.Enabled = bEnable;
            }

        }

        private void button_reviever_begin_Click(object sender, EventArgs e)
        {
            _mode = "reciever";
            // 注意事件是当时挂接的。用后也及时解挂
            this._router.SendMessageEvent += _router_SendMessageEvent;
            EnableControls(false);
            this._router.Start(
                this._channels,
                this.textBox_messageServerUrl.Text,
                this.textBox_groupName.Text,
                this.textBox_reciever_userName.Text,
                this.textBox_reciever_password.Text);
        }

        private void button_reciever_stop_Click(object sender, EventArgs e)
        {
            this._router.Stop();
            EnableControls(true);
            this._router.SendMessageEvent -= _router_SendMessageEvent;
        }

        #region IE 控件相关代码

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

        #endregion


        // 模拟持续不断发出消息
        void SendingMessage()
        {
            // 给 sender 使用
            this._channels.AddMessage += _channels_AddMessage;
            this._channels.Login += _channels_Login;
            this.Invoke((Action)(() =>
    EnableControls(false)
    ));

            try
            {
                // 注意这个写法。因为当前函数是在非界面线程中，所以从界面控件取字符串要特别小心
                string strUrl = (string)this.Invoke(new Func<string>(() =>
                {
                    return this.textBox_messageServerUrl.Text;
                }));
                string strGroupName = (string)this.Invoke(new Func<string>(() =>
                {
                    return this.textBox_groupName.Text;
                }));

                MessageConnection connection = this._channels.GetConnectionTaskAsync(
        strUrl,
        "").Result;

                for (int i = 0; ; i++)
                {
                    _cancel.Token.ThrowIfCancellationRequested();

                    Thread.Sleep(1000);

                    bool bRet = SendMessage(connection,
                        strGroupName,
                        new string[] { (i + 1).ToString() });
                    if (bRet == false)
                        break;
                }

            }
            finally
            {
                this.Invoke((Action)(() =>
                    EnableControls(true)
                    ));
                this._channels.AddMessage -= _channels_AddMessage;
                this._channels.Login -= _channels_Login;
            }
        }

        // return:
        //      false   出错
        //      true    成功
        bool SendMessage(
            MessageConnection connection,
            string strGroupName,
            string[] texts)
        {
            string strError = "";

            List<MessageRecord> records = new List<MessageRecord>();

            foreach (string text in texts)
            {
                MessageRecord record = new MessageRecord();
                record.groups = strGroupName.Split(new char[] { ',' });
                record.creator = "";    // 服务器会自己填写
                record.data = text;
                record.format = "text";
                record.type = "message";
                record.thread = "";
                record.expireTime = new DateTime(0);    // 表示永远不失效
                records.Add(record);
            }

            try
            {
                SetMessageRequest param = new SetMessageRequest("create",
                    "",
                    records);
                SetMessageResult result = connection.SetMessageTaskAsync(param, new CancellationToken()).Result;

                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }

                return true;
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
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
            return false;
        }

        string _mode = "";

        private void button_sender_begin_Click(object sender, EventArgs e)
        {
            _mode = "sender";
            this._cancel = new CancellationTokenSource();
            Task.Run(new Action(SendingMessage), this._cancel.Token);
        }

        private void button_sender_stop_Click(object sender, EventArgs e)
        {
            this._cancel.Cancel();  // 中断发送循环
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
                if (record.groups != null)
                    text.Append("groups=" + HttpUtility.HtmlEncode(string.Join(",", record.groups)) + "\r\n");
                text.Append("creator=" + record.creator + "\r\n");
                text.Append("userName=" + record.userName + "\r\n");

                text.Append("format=" + record.format + "\r\n");
                text.Append("type=" + record.type + "\r\n");
                text.Append("thread=" + record.thread + "\r\n");

                text.Append("publishTime=" + record.publishTime + "\r\n");
                text.Append("expireTime=" + record.expireTime + "\r\n");
            }

            AppendHtml(this.webBrowser1, text.ToString());
        }

        // 这里挂接的事件，只用于发送模式时显示。当前为接收模式时，这个事件被忽略
        void _channels_AddMessage(object sender, AddMessageEventArgs e)
        {
            if (_mode == "sender")
            {
                this.BeginInvoke(new Action<AddMessageEventArgs>(DisplayMessage), e);
            }
        }

        void _channels_Login(object sender, LoginEventArgs e)
        {
            MessageConnection connection = sender as MessageConnection;

            e.UserName = (string)this.Invoke(new Func<string>(() =>
            {
                return this.textBox_sender_userName.Text;
            }));
            e.Password = (string)this.Invoke(new Func<string>(() =>
            {
                return this.textBox_sender_password.Text;
            }));
            e.Parameters = "propertyList=biblio_search,libraryUID=xxx";
        }

        private void toolStripButton_clearDisplay_Click(object sender, EventArgs e)
        {
            ClearForPureTextOutputing(this.webBrowser1);
        }
    }
}
