using DigitalPlatform.Message;
using DigitalPlatform.MessageClient;
using dp2Message;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace TestMessage
{
    public partial class Form1 : Form
    {
        MessageConnectionCollection _channels = new MessageConnectionCollection();

        myMsgHandler msgHandler = null;
        public Form1()
        {
            InitializeComponent();

            _channels.Login += _channels_Login;


            msgHandler = new myMsgHandler();            
            msgHandler.Init(this._channels,
                "http://localhost:8083/dp2mserver",
                "c:/msg_data");
            msgHandler.form = this;
        }

        void _channels_Login(object sender, LoginEventArgs e)
        {
            MessageConnection connection = sender as MessageConnection;

            e.UserName = GetUserName();
            if (string.IsNullOrEmpty(e.UserName) == true)
                throw new Exception("尚未指定用户名，无法进行登录");

            e.Password = GetPassword();
            e.Parameters = "propertyList=biblio_search,libraryUID=xxx";
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

        private void button_message_send_Click(object sender, EventArgs e)
        {
            DoSendMessage(this.textBox_message_groupName.Text,"");
        }



        async void DoSendMessage(string strGroupName, string strText)
        {
            string strError = "";

            //if (string.IsNullOrEmpty(strText) == true)
            //{
            //    strError = "尚未输入文本";
            //    goto ERROR1;
            //}

            SetTextString(this.webBrowser1, "");

            List<MessageRecord> records = new List<MessageRecord>();

            for (int i = 0; i < 10; i++)
            {
                MessageRecord record = new MessageRecord();
                record.groups = strGroupName.Split(new char[] { ',' });
                record.creator = "";    // 服务器会自己填写
                record.data = "["+i.ToString()+"]";
                record.format = "text";
                record.type = "message";
                record.thread = "";
                record.expireTime = new DateTime(0);    // 表示永远不失效
                records.Add(record);
            }

            EnableControls(false);
            try
            {
                // CancellationToken cancel_token = new CancellationToken();

                try
                {
                    MessageConnection connection = await this._channels.GetConnectionAsync(
                        this.textBox_config_messageServerUrl.Text,
                        "");
                    SetMessageRequest param = new SetMessageRequest("create",
                        "",
                        records);

                    SetMessageResult result = await connection.SetMessageAsync(param);

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

        static string ToString(MessageResult result)
        {
            StringBuilder text = new StringBuilder();
            text.Append("ResultValue=" + result.Value + "\r\n");
            text.Append("ErrorInfo=" + result.ErrorInfo + "\r\n");
            text.Append("String=" + result.String + "\r\n");
            return text.ToString();
        }
        public void ShowMsg(MessageRecord record)
        {
            this.textBox1.Text += record.id + "\n";

        }
        static void SetTextString(WebBrowser webBrowser, string strText)
        {
            SetHtmlString(webBrowser, "<pre>" + HttpUtility.HtmlEncode(strText) + "</pre>");
        }

        public static void SetHtmlString(WebBrowser webBrowser,
string strHtml)
        {
            webBrowser.DocumentText = strHtml;
        }

        void EnableControls(bool bEnable)
        {
            //if (this.InvokeRequired)
            //{
            //    this.Invoke(new Action<bool>(EnableControls), bEnable);
            //    return;
            //}
            //this.Enabled = bEnable;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
