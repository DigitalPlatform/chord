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

            // 登录地址必须为rest.开头的地址
            string url = this.textBox_libraryUrl.Text;
            if (url.Length < 5
                || (url.Substring(0, 5).ToLower() != "rest."))
            {
                MessageBox.Show(this,"服务器必须仅支持rest.开头的地址，请重新输入服务器地址或咨询管理员");
                return;
            }


            RestChannel channel = mainForm.GetChannel();
            try
            {
                string pureUrl = LibraryUrl.Substring(5);
                channel.Url = pureUrl;
                string strParameters = "type=worker"
                    +",client=dp2Mini|" + Program.ClientVersion;

                //// 以手机短信验证方式登录
                //string phoneNumber = this.textBox_phone.Text.Trim();
                //if (string.IsNullOrEmpty(phoneNumber) == false)
                //    strParameters += ",phoneNumber=" + phoneNumber;

                //// 验证码
                //string tempCode = this.textBox_tempCode.Text.Trim();
                //if (string.IsNullOrEmpty(tempCode) == false)
                //    strParameters += ",tempCode=" + tempCode;

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
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
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

        private void button1_Click(object sender, EventArgs e)
        {
            string text = "aaa";
            test(out text);
            MessageBox.Show(this, text);
        }

        private void test(out string text)
        {
            text = "bbb";
        }
    }
}
