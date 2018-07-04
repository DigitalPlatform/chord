using System;
using System.Windows.Forms;

using DigitalPlatform.Forms;
using DigitalPlatform.Z3950;

namespace TestZClient
{
    public partial class MultiChannelForm : Form
    {
        public MultiChannelForm()
        {
            InitializeComponent();
        }

        private void MultiChannelForm_Load(object sender, EventArgs e)
        {

        }

        private void MultiChannelForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private async void button_begin_Click(object sender, EventArgs e)
        {
#if NO
            // TODO: 需要一个对话框选择服务器配置参数
            TargetInfo targetInfo = new TargetInfo
            {
                HostName = this.textBox_serverAddr.Text,
                Port = Convert.ToInt32(this.textBox_serverPort.Text),
                DbNames = StringUtil.SplitList(this.textBox_database.Text).ToArray(),
                AuthenticationMethod = GetAuthentcationMethod(),
                GroupID = this.textBox_groupID.Text,
                UserName = this.textBox_userName.Text,
                Password = this.textBox_password.Text,
            };
            this.button_begin.Enabled = false;
            try
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < this.numericUpDown_channelCount.Value; i++)
                    {
                        NewChannel();
                    }
                });
            }
            finally
            {
                this.button_begin.Enabled = true;
            }
#endif
        }

        // 创建一个新的通道
        ListViewItem NewChannel(TargetInfo targetinfo)
        {
            ZClient client = new ZClient();

            ListViewItem item = new ListViewItem();
            ListViewUtil.ChangeItemText(item, 0, this.listView_channels.Items.Count.ToString());
            item.Tag = client;
            this.listView_channels.Items.Add(item);

            return item;
        }
    }
}
