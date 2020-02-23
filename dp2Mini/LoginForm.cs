using DigitalPlatform;
using DigitalPlatform.CirculationClient;
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
        MainForm _mainFrom = null;

        public LoginForm(MainForm mainForm)
        {
            this._mainFrom = mainForm;

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
            //LibraryUrl = Properties.Settings.Default.cfg_library_url;
            //Username = Properties.Settings.Default.cfg_library_username;
            //Password = Properties.Settings.Default.cfg_library_password;
            //IsSavePassword = Properties.Settings.Default.cfg_savePassword;

            SettingInfo info = this._mainFrom.GetSettings();
            LibraryUrl = info.Url;
            Username = info.UserName;
            Password = info.Password;
            IsSavePassword = info.IsSavePassword;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    + ",client=dp2mini|" + ClientInfo.ClientVersion; //Program.ClientVersion;
                LoginResponse response = channel.Login(Username,
                    Password, 
                    strParameters);
                if (response.LoginResult.Value == -1 || response.LoginResult.Value == 0)
                {
                    MessageBox.Show(this, response.LoginResult.ErrorInfo);
                    return;
                }

                // 保存配置信息
                SettingInfo info = new SettingInfo();
                info.Url = LibraryUrl;
                info.UserName = Username;
                info.Password = Password;
                info.IsSavePassword = IsSavePassword;
                this._mainFrom.SaveSettings(info,false);

                //Properties.Settings.Default.cfg_library_url = LibraryUrl;
                //Properties.Settings.Default.cfg_library_username = Username;
                //Properties.Settings.Default.cfg_savePassword = IsSavePassword;

                //if (IsSavePassword)
                //    Properties.Settings.Default.cfg_library_password = Password;
                //Properties.Settings.Default.Save();

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

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(ClientInfo.DataDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }
    }
}
