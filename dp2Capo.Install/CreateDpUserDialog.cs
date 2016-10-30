using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.Text;

namespace dp2Capo.Install
{
    public partial class CreateDpUserDialog : Form
    {
        public CreateDpUserDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_userName.Text))
            {
                strError = "尚未指定用户名";
                goto ERROR1;
            }

            if (this.textBox_password.Text != this.textBox_confirmPassword.Text)
            {
                strError = "密码 和 确认密码 不一致。请重新输入";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_ipList.Text))
            {
                strError = "尚未指定 IP 地址白名单";
                goto ERROR1;
            }

            if (this.textBox_ipList.Text.IndexOfAny(new char [] {',',':'}) != -1)
            {
                strError = "IP 地址列表中不应使用逗号和冒号";
                goto ERROR1;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public List<string> IpList
        {
            get
            {
                string[] parts = this.textBox_ipList.Text.Replace("\r\n", "\r").Split(new char[] { '\r' });
                List<string> results = parts.ToList();
                // 去除空元素
                StringUtil.RemoveBlank(ref results);
                return results;
            }
            set
            {
                this.textBox_ipList.Text = StringUtil.MakePathList(value, "\r\n");
            }
        }

        public string UserName
        {
            get
            {
                return this.textBox_userName.Text;
            }
            set
            {
                this.textBox_userName.Text = value;
            }
        }

        public string Password
        {
            get
            {
                return this.textBox_password.Text;
            }
            set
            {
                this.textBox_password.Text = value;
                this.textBox_confirmPassword.Text = value;
            }
        }
    }
}
