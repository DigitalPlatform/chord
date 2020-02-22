using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Mini
{
    public partial class SettingForm : Form
    {
        MainForm _mainFrom = null;
        public SettingForm(MainForm mainForm)
        {
            this._mainFrom = mainForm;
            
            InitializeComponent();
        }

        #region 登录帐号参数

        public string LibraryUrl
        {
            get
            {
                return this.textBox_libraryUrl.Text.Trim();
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
                return this.textBox_username.Text.Trim();
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
                return this.textBox_password.Text.Trim();
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

        #endregion

        #region 图书未找到原因

        public string NotFoundReasons
        {
            get
            {
                return this.textBox_reasons.Text.Trim();
            }

            set
            {
                this.textBox_reasons.Text = value;
            }
        }

        #endregion


        private void SettingForm_Load(object sender, EventArgs e)
        {
            //LibraryUrl = Properties.Settings.Default.cfg_library_url;
            //Username = Properties.Settings.Default.cfg_library_username;
            //Password = Properties.Settings.Default.cfg_library_password;

            SettingInfo info = this._mainFrom.GetSettings();
            LibraryUrl = info.Url;
            Username = info.UserName;
            Password = info.Password;
            IsSavePassword = info.IsSavePassword;

            // 图书未找到原因
            this.NotFoundReasons = info.NotFoundReasons;
        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            //Properties.Settings.Default.cfg_library_url = LibraryUrl;
            //Properties.Settings.Default.cfg_library_username = Username;
            //Properties.Settings.Default.cfg_library_password = Password;
            //Properties.Settings.Default.Save();

            // 保存配置信息
            SettingInfo info = new SettingInfo();
            info.Url = LibraryUrl;
            info.UserName = Username;
            info.Password = Password;
            info.IsSavePassword = IsSavePassword;

            // 未找到原因
            info.NotFoundReasons = this.NotFoundReasons;

            //保存
            this._mainFrom.SaveSettings(info,true);

            // 关闭对话框 
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
