using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Text;

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

            XmlElement element = dom.DocumentElement.SelectSingleNode("sipServer") as XmlElement;
            if (element != null)
            {
                text.Append("anonymousUserName=" + element.GetAttribute("anonymousUserName") + "\r\n");
            }
            else
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
                }
                else
                {
                    Debug.Assert(node != null, "");

                    this.comboBox_dateFormat.Text = node.GetAttribute("dateFormat");
                    this.comboBox_encodingName.Text = node.GetAttribute("encoding");
                }
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
    }
}
