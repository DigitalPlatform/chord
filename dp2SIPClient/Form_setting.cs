using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2SIPClient
{
    public partial class Form_Setting : Form
    {
        public Form_Setting()
        {
            InitializeComponent();
        }

        public string SIPServerPort
        {
            get
            {
                return this.txtPort.Text;
            }
            set
            {
                this.txtPort.Text = value;
            }
        }

        public string SIPServerUrl
        {
            get
            {
                return this.txtIP.Text;
            }

            set
            {
                this.txtIP.Text = value;
            }
        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.SIPServerUrl = this.SIPServerUrl;

            int v = 0;
            bool bRet = int.TryParse(this.SIPServerPort, out v);
            if (!bRet)
            {
                MessageBox.Show(this, "'端口号'只能是纯数字");
                return;
            }
            Properties.Settings.Default.SIPServerPort = v;
            Properties.Settings.Default.Save();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Form_setting_Load(object sender, EventArgs e)
        {
            this.SIPServerUrl = Properties.Settings.Default.SIPServerUrl;
            this.SIPServerPort = Properties.Settings.Default.SIPServerPort.ToString();

        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel ;
            this.Close();
        }
    }
}
