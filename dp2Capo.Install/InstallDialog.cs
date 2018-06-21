using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.Drawing;
using DigitalPlatform.Forms;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace dp2Capo.Install
{
    /// <summary>
    /// 安装 dp2Capo 的主对话框
    /// </summary>
    public partial class InstallDialog : Form
    {
        public string DebugInfo { get; set; }

        public bool UninstallMode { get; set; }

        public InstallDialog()
        {
            InitializeComponent();
        }

        private void InstallDialog_Load(object sender, EventArgs e)
        {
            Initial();

            // 卸载状态
            if (UninstallMode == true)
            {
                this.button_OK.Text = "卸载";
                this.button_newInstance.Visible = false;
                this.button_deleteInstance.Visible = false;
                this.button_modifyInstance.Visible = false;
            }

            listView_instance_SelectedIndexChanged(this, new EventArgs());
        }

        private void InstallDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void InstallDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 全部卸载
            if (this.UninstallMode == true)
            {
                string strError = "";

                DialogResult result = MessageBox.Show(this,
"确实要卸载 dp2Capo? \r\n\r\n(*** 警告：卸载后数据将全部丢失，并无法恢复 ***)",
"卸载 dp2Capo",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return;   // cancelled

                //      -1  出错
                //      0   放弃卸载
                //      1   卸载成功
                int nRet = this.DeleteAllInstanceAndDataDir(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }
                if (nRet == 0)
                {
                    MessageBox.Show(this, strError);
                    return;
                }

                // this.Changed = false;
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
                return;
            }

            if (this.listView_instance.Items.Count == 0)
            {
                MessageBox.Show(this, "尚未创建第一个实例");
                return;
            }

            if (Finish() == false)
                return;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            // TODO: 这里有时会出错
            RestoreDataDir();

            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        // 获得一个新的实例目录路径
        static string GetNewDirectoryName(string strDataDir)
        {
            // 如果目录不存在
            if (Directory.Exists(strDataDir) == false)
                return Path.Combine(strDataDir, "instance1");

            for (int i = 1; ; i++)
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

            dlg.InstanceName = "?";
            // 找到一个没有用过的目录名字
            dlg.DataDir = GetNewDirectoryName(this.DataDir);
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            ListViewItem item = new ListViewItem((this.listView_instance.Items.Count + 1).ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_DATADIR, dlg.DataDir);
            this.listView_instance.Items.Add(item);

        }

        private void button_modifyInstance_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_instance.SelectedItems.Count == 0)
            {
                strError = "尚未选择要修改的实例";
                goto ERROR1;
            }

            ListViewItem item = this.listView_instance.SelectedItems[0];

            InstanceDialog dlg = new InstanceDialog();
            FontUtil.AutoSetDefaultFont(dlg);

            dlg.InstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);
            dlg.DataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            ListViewUtil.ChangeItemText(item, COLUMN_DATADIR, dlg.DataDir);
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_deleteInstance_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_instance.SelectedItems.Count == 0)
            {
                strError = "尚未选择要删除的实例";
                goto ERROR1;
            }

            DialogResult result = MessageBox.Show(this,
"确实要删除选定的 " + this.listView_instance.SelectedItems.Count.ToString() + " 个实例?",
"Install dp2Capo",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            List<ListViewItem> delete_items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_instance.SelectedItems)
            {
                string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                PathUtil.DeleteDirectory(strDataDir);
                delete_items.Add(item);
            }

            foreach (ListViewItem item in delete_items)
            {
                this.listView_instance.Items.Remove(item);
            }

            // 重新设置序号
            RefreshInstanceName();
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        void RefreshInstanceName()
        {
            int i = 0;
            foreach (ListViewItem item in this.listView_instance.Items)
            {
                ListViewUtil.ChangeItemText(item, COLUMN_NAME, (i + 1).ToString());
                i++;
            }
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

            AfterChangeDataDir();
        }

        #region settings.xml

        /// <summary>
        /// 可执行文件目录。在调用本对话框时要给出这个目录
        /// </summary>
        public string BinDir { get; set; }

        static ConfigSetting _config = null;

        public static ConfigSetting Config
        {
            get
            {
                return _config;
            }
        }

        public void AfterChangeDataDir()
        {
            // TODO: 如果以前有修改尚未保存？需要警告是否放弃和删除以前的数据目录

            // 创建备份数据目录
            this.ShadowDataDir = this.DataDir + "_shadow";

            if (Directory.Exists(this.DataDir))
            {
#if NO
                string strError = "";
                int nRet = PathUtil.CopyDirectory(this.DataDir, this.ShadowDataDir, true, out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }
#endif
                if (BackupDataDir(out string strError) == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }
            }
            else
                this.ShadowDataDir = "";

            FillInstance(this.DataDir);
        }

        public void Initial()
        {
            string filename = Path.Combine(this.BinDir, "settings.xml");
            // Console.WriteLine(filename);

            _config = ConfigSetting.Open(filename, true);

            this.DataDir = _config.Get("default", "data_dir", "c:\\capo_data");

            // 创建备份数据目录
            this.ShadowDataDir = this.DataDir + "_shadow";

            if (Directory.Exists(this.DataDir))
            {
#if NO
                string strError = "";
                int nRet = PathUtil.CopyDirectory(this.DataDir, this.ShadowDataDir, true, out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }
#endif
                if (BackupDataDir(out string strError) == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }
            }
            else
                this.ShadowDataDir = "";

            FillInstance(this.DataDir);
        }

        public bool Finish()
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.DataDir))
            {
                strError = "this.DataDir 不应为空";
                goto ERROR1;
            }

            // Save the configuration file.
            if (_config != null)
            {
                _config.Set("default", "data_dir", this.DataDir);
                _config.Save();
                _config = null;
            }

            // 删除备份数据目录
            if (string.IsNullOrEmpty(this.ShadowDataDir) == false)
                PathUtil.DeleteDirectory(this.ShadowDataDir);
            return true;
            ERROR1:
            MessageBox.Show(this, strError);
            return false;
        }

        int BackupDataDir(out string strError)
        {
            strError = "";
            PathUtil.DeleteDirectory(this.ShadowDataDir);
            PathUtil.CreateDirIfNeed(this.ShadowDataDir);

            List<string> data_dirs = GetInstanceDataDir(this.DataDir);
            foreach (string data_dir in data_dirs)
            {
                int nRet = PathUtil.CopyDirectory(data_dir,
                    Path.Combine(this.ShadowDataDir, Path.GetFileName(data_dir)),
true,
out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        // 恢复对话框打开前的数据目录内容
        void RestoreDataDir()
        {
            string strError = "";
            if (string.IsNullOrEmpty(this.ShadowDataDir) == true)
                return; // 对话框打开前数据目录原本不存在。所以这里也不需要恢复

            if (Directory.Exists(this.ShadowDataDir) == false)
            {
                strError = "目录 this.ShadowDataDir [" + this.ShadowDataDir + "] 不存在，无法撤销对话框打开期间所做的修改";
                goto ERROR1;
            }

            REDO:
            // 事先专门删除每个实例目录。实例目录就是名字为 log 以外的名字的目录。this.DataDir 下的普通文件不会被删除
            List<string> data_dirs = GetInstanceDataDir(this.DataDir);
            foreach (string data_dir in data_dirs)
            {
                PathUtil.DeleteDirectory(data_dir);
            }

            int nRet = PathUtil.CopyDirectory(this.ShadowDataDir,
            this.DataDir,
            false,
            out strError);
            if (nRet == -1)
            {
                strError = "恢复数据目录原有内容时出错: " + strError;
                DialogResult result = MessageBox.Show(this,
strError + "。\r\n\r\n是否要重试？",
"InstallDialog",
MessageBoxButtons.RetryCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Retry)
                    goto REDO;
                goto ERROR1;
            }

            PathUtil.DeleteDirectory(this.ShadowDataDir);
            return;
            ERROR1:
            MessageBox.Show(this, strError);
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

        // 用于备份当前对话框修改以前的数据目录
        public string ShadowDataDir { get; set; }

#endregion

        const int COLUMN_NAME = 0;
        const int COLUMN_ERRORINFO = 1;
        const int COLUMN_DATADIR = 2;
        const int COLUMN_DP2LIBRARY_URL = 3;
        const int COLUMN_DP2MSERVER_URL = 4;

        void FillInstance(string strDataDir)
        {
            this.listView_instance.Items.Clear();

#if NO
            // 如果目录不存在，则不用填充
            DirectoryInfo root = new DirectoryInfo(strDataDir);
            if (root.Exists == false)
                return;

            var dis = root.GetDirectories();
            int i = 0;
            foreach (DirectoryInfo di in dis)
            {
                // string strXmlFileName = Path.Combine(di.FullName, "capo.xml");

                ListViewItem item = new ListViewItem((i + 1).ToString());
                ListViewUtil.ChangeItemText(item, COLUMN_DATADIR, di.FullName);
                this.listView_instance.Items.Add(item);

                i++;
            }
#endif
            int nErrorCount = 0;

            List<string> data_dirs = GetInstanceDataDir(strDataDir);
            int i = 0;
            foreach (string data_dir in data_dirs)
            {
                string strFileName = Path.Combine(data_dir, "capo.xml");
                LineInfo info = new LineInfo();
                info.Build(strFileName);

                ListViewItem item = new ListViewItem((i + 1).ToString());
                ListViewUtil.ChangeItemText(item, COLUMN_DATADIR, data_dir);
                ListViewUtil.ChangeItemText(item, COLUMN_DP2LIBRARY_URL, info.dp2Library_url);
                ListViewUtil.ChangeItemText(item, COLUMN_DP2MSERVER_URL, info.dp2MServer_url);
                this.listView_instance.Items.Add(item);

                i++;
            }

            if (nErrorCount > 0)
                this.listView_instance.Columns[COLUMN_ERRORINFO].Width = 200;
            else
                this.listView_instance.Columns[COLUMN_ERRORINFO].Width = 0;
        }

        class LineInfo
        {
            public string dp2Library_url { get; set; }
            public string dp2MServer_url { get; set; }

            public void Build(string strFileName)
            {
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.Load(strFileName);
                }
                catch (FileNotFoundException)
                {
                    dp2Library_url = "";
                    dp2MServer_url = "";
                    return;
                }

                XmlElement element = dom.DocumentElement.SelectSingleNode("dp2library") as XmlElement;
                if (element != null)
                    this.dp2Library_url = element.GetAttribute("url");

                element = dom.DocumentElement.SelectSingleNode("dp2mserver") as XmlElement;
                if (element != null)
                    this.dp2MServer_url = element.GetAttribute("url");
            }
        }

        public static List<string> GetInstanceDataDirByBinDir(string strBinDir)
        {
            string filename = Path.Combine(strBinDir, "settings.xml");

            ConfigSetting config = ConfigSetting.Open(filename, true);
            if (config == null)
                return new List<string>();

            string strDataDir = config.Get("default", "data_dir", "");
            if (string.IsNullOrEmpty(strDataDir))
                return new List<string>();

            return GetInstanceDataDir(strDataDir);
        }

        public static List<string> GetInstanceDataDir(string strDataDir)
        {
            List<string> results = new List<string>();
            // 如果目录不存在，则不用填充
            DirectoryInfo root = new DirectoryInfo(strDataDir);
            if (root.Exists == false)
                return results;

            var dis = root.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                if (di.Name.ToLower() == "log")
                    continue;
                results.Add(di.FullName);
            }

            return results;
        }

        private void listView_instance_DoubleClick(object sender, EventArgs e)
        {
            button_modifyInstance_Click(sender, e);
        }

        // return:
        //      -1  出错
        //      0   放弃卸载
        //      1   卸载成功
        int DeleteAllInstanceAndDataDir(out string strError)
        {
            strError = "";

            List<string> errors = new List<string>();
            foreach (ListViewItem item in this.listView_instance.Items)
            {
                string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);

                if (string.IsNullOrEmpty(strDataDir) == false)
                {
                    REDO_DELETE_DATADIR:
                    // 删除数据目录
                    try
                    {
                        Directory.Delete(strDataDir, true);
                    }
                    catch (Exception ex)
                    {
                        DialogResult temp_result = MessageBox.Show(this, // ForegroundWindow.Instance,
    "删除实例 '" + strInstanceName + "' 的数据目录 '" + strDataDir + "' 出错：" + ex.Message + "\r\n\r\n是否重试?\r\n\r\n(Retry: 重试; Cancel: 不重试，继续后续卸载过程)",
    "卸载 dp2Capo",
    MessageBoxButtons.RetryCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                        if (temp_result == DialogResult.Retry)
                            goto REDO_DELETE_DATADIR;

                        errors.Add("删除实例 '" + strInstanceName + "' 的数据目录 '" + strDataDir + "' 时出错：" + ex.Message);
                    }
                }
            }

            if (string.IsNullOrEmpty(this.DataDir) == false)
            {
                REDO_DELETE_DATADIR:
                // 删除数据目录
                try
                {
                    Directory.Delete(this.DataDir, true);
                }
                catch (Exception ex)
                {
                    DialogResult temp_result = MessageBox.Show(this,
"删除总数据目录 '" + this.DataDir + "' 出错：" + ex.Message + "\r\n\r\n是否重试?\r\n\r\n(Retry: 重试; Cancel: 不重试，继续后续卸载过程)",
"卸载 dp2Capo",
MessageBoxButtons.RetryCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (temp_result == DialogResult.Retry)
                        goto REDO_DELETE_DATADIR;

                    errors.Add("删除总数据目录 '" + this.DataDir + "' 时出错：" + ex.Message);
                }
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "; ");
                return -1;
            }

            return 1;
        }

        private void listView_instance_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_instance.SelectedItems.Count == 0)
            {
                this.button_modifyInstance.Enabled = false;
                this.button_deleteInstance.Enabled = false;
            }
            else
            {
                this.button_modifyInstance.Enabled = true;
                this.button_deleteInstance.Enabled = true;
            }
        }

    }
}
