using DigitalPlatform.Text;
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

namespace dp2Capo.Install
{
    /// <summary>
    /// 配置一个 dp2Capo 实例针对 dp2library 的参数
    /// </summary>
    public partial class dp2LibraryDialog : Form
    {
        // capo.xml
        public XmlDocument CfgDom { get; set; }

        public dp2LibraryDialog()
        {
            InitializeComponent();
        }

        private void dp2LibraryDialog_Load(object sender, EventArgs e)
        {
            FillInfo();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (SaveToCfgDom() == false)
                return;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void button_detectManageUser_Click(object sender, EventArgs e)
        {

        }

        private void button_createManageUser_Click(object sender, EventArgs e)
        {

        }

        private void button_resetManageUserPassword_Click(object sender, EventArgs e)
        {

        }

        public static string EncryptKey = "dp2capopassword";

        void FillDefaultValue()
        {
            this.textBox_dp2LibraryUrl.Text = "http://localhost:8001/dp2library";

            this.textBox_manageUserName.Text = "capo";

            this.textBox_managePassword.Text = "";
            this.textBox_confirmManagePassword.Text = "";

            this.textBox_msmqPath.Text = @".\private$\myQueue";
        }

        // 从 CfgDom 中填充信息到控件
        void FillInfo()
        {
            XmlDocument dom = this.CfgDom;

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2library") as XmlElement;
            if (element == null)
            {
                element = dom.CreateElement("dp2library");
                dom.DocumentElement.AppendChild(element);

                FillDefaultValue();
                return;
            }

            this.textBox_dp2LibraryUrl.Text = element.GetAttribute("url");

            this.textBox_manageUserName.Text = element.GetAttribute("userName");

            string strPassword = Cryptography.Decrypt(element.GetAttribute("password"), EncryptKey);
            this.textBox_managePassword.Text = strPassword;
            this.textBox_confirmManagePassword.Text = strPassword;

            this.textBox_msmqPath.Text = element.GetAttribute("defaultQueue");
        }

        public static string GetDisplayText(XmlDocument CfgDom)
        {
            StringBuilder text = new StringBuilder();
            XmlDocument dom = CfgDom;

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2library") as XmlElement;
            if (element == null)
                return "";

            text.Append("url=" + element.GetAttribute("url") + "\r\n");
            text.Append("userName=" + element.GetAttribute("userName") + "\r\n");
            text.Append("defaultQueue=" + element.GetAttribute("defaultQueue") + "\r\n");
            return text.ToString();
        }

        // 从控件到 CfgDom
        bool SaveToCfgDom()
        {
            if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
            {
                MessageBox.Show(this, "密码和确认密码不一致，请重新输入");
                return false;
            }

            XmlDocument dom = this.CfgDom;

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2library") as XmlElement;
            if (element == null)
            {
                element = dom.CreateElement("dp2library");
                dom.DocumentElement.AppendChild(element);
            }

            element.SetAttribute("url", this.textBox_dp2LibraryUrl.Text);

            element.SetAttribute("userName", this.textBox_manageUserName.Text);

            string strPassword = Cryptography.Encrypt(this.textBox_managePassword.Text, EncryptKey);
            element.SetAttribute("password", strPassword);

            element.SetAttribute("defaultQueue", this.textBox_msmqPath.Text);

            return true;
        }

    }
}
