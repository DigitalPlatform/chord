using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Threading;

using DigitalPlatform.Text;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Forms;
using DigitalPlatform.Drawing;
using DigitalPlatform.Message;

namespace dp2Capo.Install
{
    public partial class dp2MServerDialog : Form
    {
        // capo.xml
        public XmlDocument CfgDom { get; set; }

        public dp2MServerDialog()
        {
            InitializeComponent();
        }

        private void dp2MServerDialog_Load(object sender, EventArgs e)
        {
            FillInfo();
        }

        private async void button_OK_Click(object sender, EventArgs e)
        {
            // 按下 Control 键可越过探测步骤
            bool bControl = Control.ModifierKeys == Keys.Control;

            string strError = "";
            if (string.IsNullOrEmpty(this.textBox_url.Text))
            {
                strError = "尚未指定 dp2MServer URL";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_userName.Text))
            {
                strError = "尚未指定用户名";
                goto ERROR1;
            }

            if (bControl == false && await DetectUser() == false)
                return;

            if (SaveToCfgDom() == false)
                return;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            this.Invoke(new Action(() =>
            {
                MessageBox.Show(this, strError);
            }));
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        void EnableControls(bool bEnable)
        {
            this.textBox_url.Enabled = bEnable;
            this.textBox_userName.Enabled = bEnable;
            this.textBox_password.Enabled = bEnable;
            this.button_detect.Enabled = bEnable;
            this.button_OK.Enabled = bEnable;
        }

        CancellationTokenSource _cancel = new CancellationTokenSource();

        private async void button_detect_Click(object sender, EventArgs e)
        {
            if (await DetectUser() == true)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show(this, "账户存在");
                }));
            }
        }

        async Task<bool>DetectUser()
        {
            string strError = "";
            EnableControls(false);
            try
            {
                MessageConnectionCollection _channels = new MessageConnectionCollection();
                _channels.Login += _channels_Login;

                MessageConnection connection = await _channels.GetConnectionAsyncLite(
        this.textBox_url.Text,
        "");
                CancellationToken cancel_token = _cancel.Token;

                string id = Guid.NewGuid().ToString();
                GetMessageRequest request = new GetMessageRequest(id,
                    "",
                    "gn:<default>", // "" 表示默认群组
                    "",
                    "",
                    "",
                    "",
                    "",
                    0,
                    1);
                GetMessageResult result = await connection.GetMessageAsyncLite(
        request,
        new TimeSpan(0, 1, 0),
        cancel_token);

                if (result.Value == -1)
                {
                    strError = "检测用户时出错: " + result.ErrorInfo;
                    goto ERROR1;
                }

                return true;
            }
            catch (MessageException ex)
            {
                if (ex.ErrorCode == "Unauthorized")
                {
                    strError = "用户名或密码不正确";
                    goto ERROR1;
                }
                if (ex.ErrorCode == "HttpRequestException")
                {
                    strError = "dp2MServer URL 不正确，或 dp2MServer 尚未启动";
                    goto ERROR1;
                }
                strError = ex.Message;
                goto ERROR1;
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
            finally
            {
                EnableControls(true);
            }
        ERROR1:
            this.Invoke(new Action(() =>
            {
                MessageBox.Show(this, strError);
            }));
            return false;
        }

        string GetUserName()
        {
            return this.textBox_userName.Text;
        }

        string GetPassword()
        {
            return this.textBox_password.Text;
        }

        void _channels_Login(object sender, LoginEventArgs e)
        {
            MessageConnection connection = sender as MessageConnection;

            e.UserName = GetUserName();
            if (string.IsNullOrEmpty(e.UserName) == true)
                throw new Exception("尚未指定用户名，无法进行登录");

            e.Password = GetPassword();
            e.Parameters = "propertyList=biblio_search,libraryUID=install";
        }

        public static string EncryptKey = "dp2capopassword";

        void FillDefaultValue()
        {
            this.textBox_url.Text = "http://dp2003.com:8083/dp2mserver";
            this.textBox_userName.Text = "";
            this.textBox_password.Text = "";
        }

        // 从 CfgDom 中填充信息到控件
        void FillInfo()
        {
            XmlDocument dom = this.CfgDom;

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2mserver") as XmlElement;
            if (element == null)
            {
                element = dom.CreateElement("dp2mserver");
                dom.DocumentElement.AppendChild(element);

                FillDefaultValue();
                return;
            }

            this.textBox_url.Text = element.GetAttribute("url");

            this.textBox_userName.Text = element.GetAttribute("userName");

            string strPassword = Cryptography.Decrypt(element.GetAttribute("password"), EncryptKey);
            this.textBox_password.Text = strPassword;
        }

        public static string GetDisplayText(XmlDocument CfgDom)
        {
            StringBuilder text = new StringBuilder();
            XmlDocument dom = CfgDom;

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2mserver") as XmlElement;
            if (element == null)
                return "";

            text.Append("url=" + element.GetAttribute("url") + "\r\n");
            text.Append("userName=" + element.GetAttribute("userName") + "\r\n");
            return text.ToString();
        }

        // 从控件到 CfgDom
        bool SaveToCfgDom()
        {
            XmlDocument dom = this.CfgDom;

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2mserver") as XmlElement;
            if (element == null)
            {
                element = dom.CreateElement("dp2mserver");
                dom.DocumentElement.AppendChild(element);
            }

            element.SetAttribute("url", this.textBox_url.Text);

            element.SetAttribute("userName", this.textBox_userName.Text);

            string strPassword = Cryptography.Encrypt(this.textBox_password.Text, EncryptKey);
            element.SetAttribute("password", strPassword);
            return true;
        }

        private void textBox_userName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBox_userName.Text) == true)
            {
                this.button_detect.Enabled = false;
            }
            else
            {
                this.button_detect.Enabled = true;
            }
        }

        private void dp2MServerDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancel.Cancel();
        }

        private void dp2MServerDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

    }
}
