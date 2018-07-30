using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.SIP.Server;

namespace dp2Capo.Install
{
    public partial class SipSettingDialog : Form
    {
        // capo.xml
        public XmlDocument CfgDom { get; set; }

        public SipSettingDialog()
        {
            InitializeComponent();
        }

        private void SipSettingDialog_Load(object sender, EventArgs e)
        {
            FillInfo();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 按下 Control 键可越过探测步骤
            bool bControl = Control.ModifierKeys == Keys.Control;

            string strError = "";

            if (SaveToCfgDom() == false)
                return;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;

        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public static string GetDisplayText(XmlDocument CfgDom)
        {
            StringBuilder text = new StringBuilder();
            XmlDocument dom = CfgDom;

            XmlElement root = dom.DocumentElement.SelectSingleNode("sipServer") as XmlElement;
            if (root == null)
                return "";

            if (dom.DocumentElement.SelectSingleNode("sipServer/dp2library") is XmlElement node)
            {
                text.Append("anonymousUserName=" + node.GetAttribute("anonymousUserName") + "\r\n");
            }

            if (dom.DocumentElement.SelectSingleNode("sipServer") is XmlElement element)
            {
                text.Append("encoding=" + element.GetAttribute("encoding") + "\r\n");
                text.Append("dateFormat=" + element.GetAttribute("dateFormat") + "\r\n");
                text.Append("ipList=" + element.GetAttribute("ipList") + "\r\n");
            }

            if (text.Length == 0)
                text.Append("*");

            return text.ToString();
        }

        void EnableControls(bool bEnable)
        {
            this.button_OK.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;
            this.button_detectManageUser.Enabled = bEnable;

            this.textBox_anonymousUserName.Enabled = bEnable;
            this.textBox_anonymousPassword.Enabled = bEnable;
            this.button_detectAnonymousUser.Enabled = bEnable;

            this.Update();
        }

        void FillInfo()
        {
            XmlDocument dom = this.CfgDom;

            {
                // dp2library 服务器参数

                // 万一已经存在的文件是不正确的?
                if (!(dom.DocumentElement.SelectSingleNode("dp2library") is XmlElement node))
                {
                    //strError = "配置文件中缺乏 libraryserver 元素";
                    //return -1;
                    this.textBox_manageUserName.Text = "";
                    this.textBox_managePassword.Text = "";

                    this.comboBox_librarywsUrl.Text = "";
                }
                else
                {
                    Debug.Assert(node != null, "");

                    string strUserName = node.GetAttribute("userName");
                    string strPassword = node.GetAttribute("password");
                    strPassword = dp2LibraryDialog.DecryptPasssword(strPassword);

                    string strUrl = node.GetAttribute("url");

                    this.textBox_manageUserName.Text = strUserName;
                    this.textBox_managePassword.Text = strPassword;

                    if (String.IsNullOrEmpty(strUrl) == false)
                        this.comboBox_librarywsUrl.Text = strUrl;
                }
            }

            {
                // sipServer 服务器参数
                if (!(dom.DocumentElement.SelectSingleNode("sipServer/dp2library") is XmlElement node))
                {
                    this.textBox_anonymousUserName.Text = "";
                    this.textBox_anonymousPassword.Text = "";
                }
                else
                {
                    Debug.Assert(node != null, "");

                    string strAnonymousUserName = node.GetAttribute("anonymousUserName");
                    string strAnonymousPassword = node.GetAttribute("anonymousPassword");
                    strAnonymousPassword = DecryptPasssword(strAnonymousPassword);

                    this.textBox_anonymousUserName.Text = strAnonymousUserName;
                    this.textBox_anonymousPassword.Text = strAnonymousPassword;
                }
            }

            {
                // sipServer 参数

                if (!(dom.DocumentElement.SelectSingleNode("sipServer") is XmlElement node))
                {
                    this.comboBox_dateFormat.Text = "";
                    this.comboBox_encodingName.Text = "";
                    this.textBox_ipList.Text = "";
                }
                else
                {
                    Debug.Assert(node != null, "");

                    this.comboBox_dateFormat.Text = node.GetAttribute("dateFormat");
                    this.comboBox_encodingName.Text = node.GetAttribute("encoding");
                    this.textBox_ipList.Text = node.GetAttribute("ipList");
                }

                if (string.IsNullOrEmpty(this.comboBox_dateFormat.Text))
                    this.comboBox_dateFormat.Text = SipServer.DEFAULT_DATE_FORMAT;
                if (string.IsNullOrEmpty(this.comboBox_encodingName.Text))
                    this.comboBox_encodingName.Text = SipServer.DEFAULT_ENCODING_NAME;
            }

            if (!(dom.DocumentElement.SelectSingleNode("sipServer") is XmlElement root))
                this.checkBox_enableSIP.Checked = false;
            else
                this.checkBox_enableSIP.Checked = true;

            SetEnableSipUiState();
        }

        void SetEnableSipUiState()
        {
            if (this.checkBox_enableSIP.Checked)
                this.tabControl_main.Enabled = true;
            else
                this.tabControl_main.Enabled = false;
        }

        // 从控件到 CfgDom
        bool SaveToCfgDom()
        {
            XmlDocument dom = this.CfgDom;

            XmlElement root = dom.DocumentElement.SelectSingleNode("sipServer") as XmlElement;

            if (this.checkBox_enableSIP.Checked == false)
            {
                if (root != null)
                    root.ParentNode.RemoveChild(root);
                return true;
            }

            if (root == null)
            {
                root = dom.CreateElement("sipServer");
                dom.DocumentElement.AppendChild(root);
            }

            {
                if (!(root.SelectSingleNode("dp2library") is XmlElement element))
                {
                    element = dom.CreateElement("dp2library");
                    root.AppendChild(element);
                }

                element.SetAttribute("anonymousUserName", this.textBox_anonymousUserName.Text);
                element.SetAttribute("anonymousPassword", EncryptPassword(this.textBox_anonymousPassword.Text));
            }

            {
                root.SetAttribute("dateFormat", this.comboBox_dateFormat.Text);
                root.SetAttribute("encoding", this.comboBox_encodingName.Text);
                root.SetAttribute("ipList", this.textBox_ipList.Text);
            }

            return true;
        }

        static string EncryptKey = "dp2sipserver_password_key";

        public string DecryptPasssword(string strEncryptedText)
        {
            if (String.IsNullOrEmpty(strEncryptedText) == false)
            {
                try
                {
                    string strPassword = Cryptography.Decrypt(
        strEncryptedText,
        EncryptKey);
                    return strPassword;
                }
                catch
                {
                    return "errorpassword";
                }
            }

            return "";
        }

        public string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, EncryptKey);
        }

        private void checkBox_enableSIP_CheckedChanged(object sender, EventArgs e)
        {
            SetEnableSipUiState();
        }

        private void button_detectManageUser_Click(object sender, EventArgs e)
        {
            EnableControls(false);

            try
            {
                if (string.IsNullOrEmpty(this.comboBox_librarywsUrl.Text))
                {
                    MessageBox.Show(this, "尚未输入 dp2Library 服务器的 URL");
                    return;
                }

                if (string.IsNullOrEmpty(this.textBox_manageUserName.Text))
                {
                    MessageBox.Show(this, "尚未指定 dp2Library 管理用户名");
                    return;
                }

                // 检测帐户登录是否成功?
                // 进行登录
                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                int nRet = DoLogin(
                    this.comboBox_librarywsUrl.Text,
                    this.textBox_manageUserName.Text,
                    this.textBox_managePassword.Text,
                    out string strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "检测 dp2library 帐户时发生错误: " + strError);
                    return;
                }
                if (nRet == 0)
                {
                    MessageBox.Show(this, "您指定的 dp2library 帐户 不正确: " + strError);
                    return;
                }

                MessageBox.Show(this, "您指定的 dp2library 帐户 正确");
            }
            finally
            {
                EnableControls(true);
            }
        }

        private void button_detectAnonymousUser_Click(object sender, EventArgs e)
        {
            EnableControls(false);

            try
            {
                if (string.IsNullOrEmpty(this.comboBox_librarywsUrl.Text))
                {
                    MessageBox.Show(this, "尚未输入 dp2Library 服务器的 URL");
                    return;
                }

                if (string.IsNullOrEmpty(this.textBox_anonymousUserName.Text))
                {
                    MessageBox.Show(this, "尚未指定 匿名登录用户名");
                    return;
                }

                // 检测帐户登录是否成功?

                // 进行登录
                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                int nRet = DoLogin(
                    this.comboBox_librarywsUrl.Text,
                    this.textBox_anonymousUserName.Text,
                    this.textBox_anonymousPassword.Text,
                    out string strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "检测 匿名登录 用户时发生错误: " + strError);
                    return;
                }
                if (nRet == 0)
                {
                    MessageBox.Show(this, "您指定的 匿名登录 用户 不正确: " + strError);
                    return;
                }

                MessageBox.Show(this, "您指定的 匿名登录 用户 正确");
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 进行登录
        // return:
        //      -1  error
        //      0   登录未成功
        //      1   登录成功
        static int DoLogin(
            string strLibraryWsUrl,
            string strUserName,
            string strPassword,
            out string strError)
        {
            strError = "";

            using (LibraryChannel Channel = new LibraryChannel())
            {
                Channel.Url = strLibraryWsUrl;

                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                long lRet = Channel.Login(strUserName,
                    strPassword,
                    "location=SIP Server,type=worker,client=chordInstaller|3.0",
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
        }
    }
}
