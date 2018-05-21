using DigitalPlatform.LibraryRestClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Mini
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }


        public string LibraryUrl
        {
            get
            {
                return this.textBox_libraryUrl.Text;
            }

            set
            {
                this.textBox_libraryUrl.Text = value;
            }
        }

        public string Username
        {
            get
            {
                return this.textBox_username.Text;
            }

            set
            {
                this.textBox_username.Text = value;
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
            }
        }

        public bool IsSavePassword
        {
            get
            {
                return this.checkBox_savePassword.Checked;
            }

            set
            {
                this.checkBox_savePassword.Checked = value;
            }
        }


        private void SettingForm_Load(object sender, EventArgs e)
        {
            LibraryUrl = Properties.Settings.Default.cfg_library_url;
            Username = Properties.Settings.Default.cfg_library_username;
            Password = Properties.Settings.Default.cfg_library_password;
            IsSavePassword = Properties.Settings.Default.cfg_savePassword;
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void LoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_login_Click(object sender, EventArgs e)
        {
            MainForm mainForm = null;
            if (this.Owner is MainForm)
                mainForm = this.Owner as MainForm;

            Debug.Assert(mainForm != null, "登录对话框的父窗口为空");

            LibraryChannel channel = mainForm.GetChannel();
            try
            {
                channel.Url = LibraryUrl;
                string strParameters = "type=worker,client=dp2Mini|" + Program.ClientVersion;
                LoginResponse response = channel.Login(Username, Password, strParameters);
                if (response.LoginResult.Value == -1 || response.LoginResult.Value == 0)
                {
                    MessageBox.Show(this, response.LoginResult.ErrorInfo);
                    return;
                }

                Properties.Settings.Default.cfg_library_url = LibraryUrl;
                Properties.Settings.Default.cfg_library_username = Username;
                Properties.Settings.Default.cfg_savePassword = IsSavePassword;

                if (IsSavePassword)
                    Properties.Settings.Default.cfg_library_password = Password;
                Properties.Settings.Default.Save();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            finally
            {
                mainForm.ReturnChannel(channel);
            }
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
