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
        public SettingForm()
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


        private void SettingForm_Load(object sender, EventArgs e)
        {
            LibraryUrl = Properties.Settings.Default.cfg_library_url;
            Username = Properties.Settings.Default.cfg_library_username;
            Password = Properties.Settings.Default.cfg_library_password;
        }

        private void SettingForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void SettingForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.cfg_library_url = LibraryUrl;
            Properties.Settings.Default.cfg_library_username = Username;
            Properties.Settings.Default.cfg_library_password = Password;
            Properties.Settings.Default.Save();
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
