using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.Forms;
using DigitalPlatform.Z3950;
using static DigitalPlatform.Z3950.ZClient;

namespace TestZClient
{
    public partial class MultiChannelForm : Form
    {
        CancellationTokenSource cancel = new CancellationTokenSource();

        public MultiChannelForm()
        {
            InitializeComponent();
        }

        private void MultiChannelForm_Load(object sender, EventArgs e)
        {

        }

        private void MultiChannelForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            cancel.Cancel();

            foreach (ListViewItem item in this.listView_channels.Items)
            {
                ZClient client = (ZClient)item.Tag;
                if (client != null)
                    client.Dispose();
            }
        }

        private async void button_begin_Click(object sender, EventArgs e)
        {
            // TODO: 需要一个对话框选择服务器配置参数
            TargetInfo targetInfo = new TargetInfo
            {
                HostName = "localhost",
                Port = 210,
                DbNames = new string[] { "cbook" },
                //AuthenticationMethod = GetAuthentcationMethod(),
                //GroupID = this.textBox_groupID.Text,
                UserName = "public",
                Password = "",
            };
            this.button_begin.Enabled = false;
            try
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < this.numericUpDown_channelCount.Value; i++)
                    {
                        NewChannel(targetInfo);
                    }
                });
            }
            finally
            {
                this.button_begin.Enabled = true;
            }
        }

        // 创建一个新的通道
        ListViewItem NewChannel(TargetInfo targetinfo)
        {
            ZClient client = new ZClient();

            ListViewItem item = null;

            this.Invoke(
    (Action)(() =>
    {
        item = new ListViewItem();
        ListViewUtil.ChangeItemText(item, 0, this.listView_channels.Items.Count.ToString());
        item.Tag = client;
        this.listView_channels.Items.Add(item);
    }
    ));
            // 启动测试过程
            HandleTesting(client,
    targetinfo,
    item);

            return item;
        }

        // 执行一个通道的测试
        async Task HandleTesting(ZClient client,
            TargetInfo targetInfo,
            ListViewItem item)
        {
            string strError = "";

            Random rnd = new Random();

            int i = 0;
            while (cancel.Token.IsCancellationRequested == false)
            {
                {
                    // return Value:
                    //      -1  出错
                    //      0   成功
                    //      1   调用前已经是初始化过的状态，本次没有进行初始化
                    InitialResult result = await client.TryInitialize(targetInfo, false);
                    if (result.Value == -1)
                    {
                        strError = "Initialize error: " + result.ErrorInfo;
                        this.Invoke(
                            (Action)(() =>
                        ListViewUtil.ChangeItemText(item, 2, strError)
                            ));
                        return;
                    }
                }

                this.Invoke(
                    (Action)(() =>
                ListViewUtil.ChangeItemText(item, 1, ((i++) + 1).ToString())
                    ));

                // 检索
                {
                    string strQuery = "\"中国\"/1=4";
                    SearchResult result = await client.Search(strQuery,
                        targetInfo.DefaultQueryTermEncoding,
                        targetInfo.DbNames,
                        targetInfo.PreferredRecordSyntax,
                        "default");
                    if (result.Value == -1)
                    {
                        strError = "Search error: " + result.ErrorInfo;
                        this.Invoke(
                            (Action)(() =>
                        ListViewUtil.ChangeItemText(item, 2, strError)
                            ));
                        return;
                    }
                }

                // 获取

                // 切断
                if ((i % 10) == 0)
                    client.CloseConnection();

                await Task.Delay(rnd.Next(1, 500));
            }
        }
    }
}
