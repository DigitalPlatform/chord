using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.SIP2;
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
    public partial class Form_SettingForAuto : Form
    {

        public Form_SettingForAuto()
        {
            InitializeComponent();
        }

        private LibraryChannelPool _channelPool = new LibraryChannelPool();

        List<LibraryChannel> _channelList = new List<LibraryChannel>();

        // parameters:
        //      style    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public LibraryChannel GetChannel()
        {
            LibraryChannel channel = this._channelPool.GetChannel(this.dp2ServerUrl,
                this.dp2Username);
            // channel.Idle += channel_Idle;



            _channelList.Add(channel);
            // TODO: 检查数组是否溢出
            return channel;
        }

#if NO
        void channel_Idle(object sender, IdleEventArgs e)
        {
            Application.DoEvents();
        }
#endif

        public void ReturnChannel(LibraryChannel channel)
        {
            // channel.Idle -= channel_Idle;

            this._channelPool.ReturnChannel(channel);
            _channelList.Remove(channel);
        }

        public string dp2ServerUrl
        {
            get
            {
                return this.textBox_dp2serverUrl.Text;
            }

            set
            {
                this.textBox_dp2serverUrl.Text = value;
            }
        }

        public string dp2Username
        {
            get
            {
                return this.textBox_dp2username.Text;
            }

            set
            {
                this.textBox_dp2username.Text = value;
            }
        }

        public string dp2Password
        {
            get
            {
                return this.textBox_dp2password.Text;
            }

            set
            {
                this.textBox_dp2password.Text = value;
            }
        }

        private void Form_CreateTestEnv_Load(object sender, EventArgs e)
        {
            this.dp2ServerUrl = Properties.Settings.Default.dp2ServerUrl;
            this.dp2Username = Properties.Settings.Default.dp2Username;
            this.dp2Password = Properties.Settings.Default.dp2Password;
        }

        private void button_verify_Click(object sender, EventArgs e)
        {
            LibraryChannel channel = this.GetChannel();
            try
            {
                string strError = "";
                long lRet = channel.Login(this.dp2Username,
                this.dp2Password,
                "type=worker,client=dp2SIPClient|0.01",
                out strError);
                if (lRet == -1)
                {
                    MessageBox.Show(this, "失败：" + strError);
                    return;
                }
                else if (lRet == 0)
                {
                    MessageBox.Show(this, "用户名或者密码不存在");
                    return;
                }
                else
                {
                    MessageBox.Show(this, "帐户已存在");
                    return;
                }
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }
 

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.dp2ServerUrl =this.dp2ServerUrl ;
            Properties.Settings.Default.dp2Username = this.dp2Username;
            Properties.Settings.Default.dp2Password = this.dp2Password;

            Properties.Settings.Default.Save();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }




    }
}
