using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.Xml;
using DigitalPlatform.Drawing;
using DigitalPlatform.Forms;

namespace dp2Capo.Install
{
    /// <summary>
    /// 安装 dp2Capo 的主对话框
    /// </summary>
    public partial class InstallDialog : Form
    {
        public string DebugInfo { get; set; }

        public InstallDialog()
        {
            InitializeComponent();
        }

        private void InstallDialog_Load(object sender, EventArgs e)
        {
            Initial();
        }

        private void InstallDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Finish() == false)
            {
                e.Cancel = true;
                return;
            }
        }

        private void InstallDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {

        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {

        }

        // 获得一个新的实例目录路径
        static string GetNewDirectoryName(string strDataDir)
        {
            // 如果目录不存在
            if (Directory.Exists(strDataDir) == false)
                return Path.Combine(strDataDir, "instance1");

            for(int i=1;;i++)
            {
                string path = Path.Combine(strDataDir, "instance" + i.ToString());
                if (Directory.Exists(path) == false)
                    return path;
            }
        }

        private void button_newInstance_Click(object sender, EventArgs e)
        {
            InstanceDialog dlg = new InstanceDialog();
            FontUtil.AutoSetDefaultFont(dlg);

            // 找到一个没有用过的目录名字
            dlg.DataDir = GetNewDirectoryName(this.DataDir);
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;
        }

        private void button_modifyInstance_Click(object sender, EventArgs e)
        {

        }

        private void button_deleteInstance_Click(object sender, EventArgs e)
        {

        }

        private void button_getDataDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

            dir_dlg.Description = "请指定 dp2Capo 数据目录:";
            dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dir_dlg.ShowNewFolderButton = true;
            dir_dlg.SelectedPath = this.textBox_dataDir.Text;

            if (dir_dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_dataDir.Text = dir_dlg.SelectedPath;
        }

        #region settings.xml

        public string BinDir { get; set; }

        static ConfigSetting _config = null;

        public static ConfigSetting Config
        {
            get
            {
                return _config;
            }
        }

        public void Initial()
        {
            string filename = Path.Combine(this.BinDir, "settings.xml");
            Console.WriteLine(filename);

            _config = ConfigSetting.Open(filename, true);

            this.DataDir = _config.Get("default", "data_dir", "c:\\capo_data");

#if NO
            if (string.IsNullOrEmpty(this.DataDir) == true)
                this.DataDir = "c:\\capo_data";
#endif
            FillInstance(this.DataDir);
        }

        public bool Finish()
        {
            string strError = "";
            if (string.IsNullOrEmpty(this.DataDir))
            {
                strError = "";
                goto ERROR1;
            }

            // Save the configuration file.
            if (_config != null)
            {
                _config.Set("default", "data_dir", this.DataDir);
                _config.Save();
                _config = null;
            }
            return true;
        ERROR1:
            MessageBox.Show(this, strError);
            return false;
        }

        public string DataDir
        {
            get
            {
                return this.textBox_dataDir.Text;
            }
            set
            {
                this.textBox_dataDir.Text = value;
            }
        }

        #endregion

        void FillInstance(string strDataDir)
        {
            this.listView_instance.Items.Clear();

            // 如果目录不存在，则不用填充
            DirectoryInfo root = new DirectoryInfo(strDataDir);
            if (root.Exists == false)
                return;

            var dis = root.GetDirectories();
            int i = 0;
            foreach (DirectoryInfo di in dis)
            {
                string strXmlFileName = Path.Combine(di.FullName, "capo.xml");

                ListViewItem item = new ListViewItem((i + 1).ToString());
                ListViewUtil.ChangeItemText(item, 2, di.FullName);
                i++;
            }
        }
    }
}
