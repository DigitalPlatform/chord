using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Drawing;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

using DigitalPlatform.Xml;
using DigitalPlatform.Drawing;
using DigitalPlatform.Forms;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Interfaces;

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

            this.BeginInvoke(new Action(RefreshInstanceState));
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
            InstanceDialog new_instance_dlg = new InstanceDialog();
            FontUtil.AutoSetDefaultFont(new_instance_dlg);

            new_instance_dlg.ParentDialog = this;
            new_instance_dlg.Index = this.listView_instance.Items.Count;
            new_instance_dlg.InstanceName = "?";
            // 找到一个没有用过的目录名字
            new_instance_dlg.DataDir = GetNewDirectoryName(this.DataDir);
            new_instance_dlg.StartPosition = FormStartPosition.CenterScreen;
            new_instance_dlg.ShowDialog(this);

            if (new_instance_dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.Enabled = false;
            try
            {
                ListViewItem item = new ListViewItem(new_instance_dlg.InstanceName);

                RefreshItemLine(item, new_instance_dlg.DataDir);
                this.listView_instance.Items.Add(item);

                if (new_instance_dlg.InstanceName.IndexOf("?") != -1)
                    RefreshInstanceName(item);

                if (IsDp2CapoRunning())
                    StartOrStopOneInstance(new_instance_dlg.InstanceName, "start");
            }
            finally
            {
                this.Enabled = true;
            }
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

            string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);
            if (IsLocking(strInstanceName))
            {
                strError = "实例 '" + strInstanceName + "' 当前处于被锁定状态，无法进行修改操作";
                goto ERROR1;
            }

            bool bStopped = false;
            if (item.ImageIndex == IMAGEINDEX_RUNNING)
            {
                // 只对正在 running 状态的实例做停止处理
                StartOrStopOneInstance(strInstanceName,
                "stop");
                bStopped = true;
            }
            try
            {
                InstanceDialog dlg = new InstanceDialog();
                FontUtil.AutoSetDefaultFont(dlg);

                dlg.ParentDialog = this;
                dlg.Index = this.listView_instance.Items.IndexOf(item);
                dlg.InstanceName = strInstanceName;
                dlg.DataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);

                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);

                if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return;

                RefreshItemLine(item, dlg.DataDir);
            }
            finally
            {
                if (bStopped)
                    StartOrStopOneInstance(strInstanceName,
    "start");
            }
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

            // 删除操作中，被停止过的实例的实例名
            List<string> stopped_instance_names = new List<string>();

            this.Enabled = false;
            try
            {
                bool bRunning = IsDp2CapoRunning();

                List<ListViewItem> delete_items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_instance.SelectedItems)
                {
                    string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);

                    if (IsLocking(strInstanceName))
                    {
                        strError = "实例 '" + strInstanceName + "' 当前处于被锁定状态，无法进行删除操作";
                        goto ERROR1;
                    }

                    string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);

                    if (String.IsNullOrEmpty(strDataDir) == true)
                        continue;

                    if (Directory.Exists(strDataDir) == false)
                        continue;

                    // 停止即将被删除的实例
                    if (bRunning)
                    {
                        StartOrStopOneInstance(strInstanceName, "stop");
                        stopped_instance_names.Add(strInstanceName);
                    }

                    PathUtil.DeleteDirectory(strDataDir);
                    delete_items.Add(item);

                    stopped_instance_names.Remove(strInstanceName);
                }

                foreach (ListViewItem item in delete_items)
                {
                    this.listView_instance.Items.Remove(item);
                }

                // 重新设置序号
                RefreshInstanceName();

                // 重新启动那些被放弃删除的实例
                foreach (string strInstanceName in stopped_instance_names)
                {
                    StartOrStopOneInstance(strInstanceName, "start");
                }
                return;
            }
            finally
            {
                this.Enabled = true;
            }
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // parameters:
        //      refrsh_item 如果为空，表示要刷新全部 ListViewItem。否则只刷新这一个 ListViewItem
        void RefreshInstanceName(ListViewItem refresh_item = null)
        {
            int i = 0;
            foreach (ListViewItem item in this.listView_instance.Items)
            {
                if (refresh_item != null && item != refresh_item)
                    continue;
                string data_dir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                string instance_name = Path.GetFileName(data_dir);

                // 从配置文件中得到 instanceName 配置
                string strFileName = Path.Combine(data_dir, "capo.xml");
                string temp = InstanceDialog.GetInstanceName(strFileName);
                if (temp != null)
                    instance_name = temp;

                ListViewUtil.ChangeItemText(item, COLUMN_NAME, instance_name);
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

            if (FillInstance(this.DataDir) == false)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
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
            PathUtil.DeleteDirectory(this.ShadowDataDir);   // 先尝试删除备份目录。因为以前运行到中途被杀死的 dp2Capo.exe 可能残留备份目录，不删除会对本次备份造成影响
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

            // 拷贝 config.xml 文件
            string source_filename = Path.Combine(this.DataDir, "config.xml");
            string target_filename = Path.Combine(this.ShadowDataDir, "config.xml");
            if (File.Exists(source_filename))
                File.Copy(source_filename, target_filename, true);

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

            int nErrorCount = 0;

            // 事先专门删除每个实例目录。实例目录就是名字为 log 以外的名字的目录。this.DataDir 下的普通文件不会被删除
            List<string> data_dirs = GetInstanceDataDir(this.DataDir);
            foreach (string data_dir in data_dirs)
            {
                REDO_DELETE:
                try
                {
                    PathUtil.DeleteDirectory(data_dir);
                }
                catch (Exception ex)
                {
                    strError = "恢复数据目录原有内容时，在删除子目录阶段出错: " + ex.Message;
                    DialogResult result = MessageBox.Show(this,
    strError + "。\r\n\r\n是否要重试？",
    "InstallDialog",
    MessageBoxButtons.RetryCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Retry)
                        goto REDO_DELETE;
                    // 否则就算了，不要报错退出。因为此时退出代价很大
                    nErrorCount++;
                }
            }

            REDO:
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

        bool FillInstance(string strDataDir)
        {
            string strError = "";

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
                if (info.Build(strFileName, out strError) == false)
                    goto ERROR1;

                string instance_name = Path.GetFileName(data_dir);

                ListViewItem item = new ListViewItem(instance_name);
                ListViewUtil.ChangeItemText(item, COLUMN_DATADIR, data_dir);
                ListViewUtil.ChangeItemText(item, COLUMN_DP2LIBRARY_URL, info.dp2Library_url);
                ListViewUtil.ChangeItemText(item, COLUMN_DP2MSERVER_URL, info.dp2MServer_url);
                this.listView_instance.Items.Add(item);

                i++;
            }

            RefreshInstanceName();

            if (nErrorCount > 0)
                this.listView_instance.Columns[COLUMN_ERRORINFO].Width = 200;
            else
                this.listView_instance.Columns[COLUMN_ERRORINFO].Width = 0;

            return true;
            ERROR1:
            MessageBox.Show(this, strError);
            return false;
        }

        void RefreshItemLine(ListViewItem item,
            string data_dir)
        {
            string strFileName = Path.Combine(data_dir, "capo.xml");
            LineInfo info = new LineInfo();
            if (info.Build(strFileName, out string strError) == false)
            {
                MessageBox.Show(this, strError);
                return;
            }

            string instance_name = Path.GetFileName(data_dir);

            ListViewUtil.ChangeItemText(item, COLUMN_NAME, instance_name);
            ListViewUtil.ChangeItemText(item, COLUMN_DATADIR, data_dir);
            ListViewUtil.ChangeItemText(item, COLUMN_DP2LIBRARY_URL, info.dp2Library_url);
            ListViewUtil.ChangeItemText(item, COLUMN_DP2MSERVER_URL, info.dp2MServer_url);

            RefreshInstanceName(item);
        }

        class LineInfo
        {
            public string dp2Library_url { get; set; }
            public string dp2MServer_url { get; set; }

            public bool Build(string strFileName, out string strError)
            {
                strError = "";

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.Load(strFileName);
                }
                catch (FileNotFoundException)
                {
                    dp2Library_url = "";
                    dp2MServer_url = "";
                    return true;
                }
                catch (Exception ex)
                {
                    strError = "从文件 '" + strFileName + "' 装入 XMLDOM 时出错: " + ex.Message;
                    return false;
                }

                XmlElement element = dom.DocumentElement.SelectSingleNode("dp2library") as XmlElement;
                if (element != null)
                    this.dp2Library_url = element.GetAttribute("url");

                element = dom.DocumentElement.SelectSingleNode("dp2mserver") as XmlElement;
                if (element != null)
                    this.dp2MServer_url = element.GetAttribute("url");

                return true;
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

            List<string> results = new List<string>();
            results.Add(strDataDir);    // 根级数据目录也包含到其中，这样便于后面打包其 log 子目录中的错误日志

            results.AddRange(GetInstanceDataDir(strDataDir));

            return results;
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

        // 全局参数配置
        private void button_globalConfig_Click(object sender, EventArgs e)
        {
            bool bStopped = false;

            if (GetGlobalRunningState() == true)
            {
                StartOrStopGlobalInstance("stop");
                bStopped = true;
            }

            try
            {
                GlobalConfigDialog dlg = new GlobalConfigDialog();
                FontUtil.AutoSetDefaultFont(dlg);

                dlg.DataDir = this.textBox_dataDir.Text;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);
            }
            finally
            {
                if (bStopped)
                    StartOrStopGlobalInstance("start");
            }
        }

        private void textBox_dataDir_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBox_dataDir.Text))
                this.button_globalConfig.Enabled = false;
            else
                this.button_globalConfig.Enabled = true;
        }

        // 查找一个实例名。返回在 ListView 中的 index
        public int FindInstanceName(string strInstanceName)
        {
            ListViewItem item = ListViewUtil.FindItem(this.listView_instance, strInstanceName, 0);
            if (item == null)
                return -1;
            return this.listView_instance.Items.IndexOf(item);
        }

        #region 实例运行状态

        void StartOrStopGlobalInstance(string strAction)
        {
            List<string> errors = new List<string>();
            this.EnableControls(false);
            try
            {
                string strError = "";

                {
                    string strInstanceName = ".global";

                    if (IsLocking(strInstanceName))
                    {
                        errors.Add("全局服务当前处于被锁定状态，无法进行 " + strAction + " 操作");
                        goto END1;
                    }

                    int nRet = dp2capo_serviceControl(
        strAction,
        strInstanceName,
        out strError);
                    if (nRet == -1)
                        errors.Add(strError);
                    else
                        SetGlobalRunningState(strAction == "stop" ? false : true);
                }

                return;
            }
            finally
            {
                this.EnableControls(true);
            }

            END1:
            if (errors.Count > 0)
                MessageBox.Show(this, StringUtil.MakePathList(errors, "; "));
        }

        void SetGlobalRunningState(bool bRunning)
        {
            button_globalConfig.Text = (bRunning ? "+" : "-") + " 全局参数 ...";
        }

        bool GetGlobalRunningState()
        {
            if (button_globalConfig.Text.StartsWith("+"))
                return true;
            return false;
        }

        void StartOrStopInstance(string strAction)
        {
            List<string> errors = new List<string>();
            this.EnableControls(false);
            try
            {
                string strError = "";

                foreach (ListViewItem item in this.listView_instance.SelectedItems)
                {
                    string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);

                    if (IsLocking(strInstanceName))
                    {
                        errors.Add("实例 '" + strInstanceName + "' 当前处于被锁定状态，无法进行 " + strAction + " 操作");
                        continue;
                    }

                    int nRet = dp2capo_serviceControl(
        strAction,
        strInstanceName,
        out strError);
                    if (nRet == -1)
                        errors.Add(strError);
                    else
                        item.ImageIndex = strAction == "stop" ? IMAGEINDEX_STOPPED : IMAGEINDEX_RUNNING;
                }

            }
            finally
            {
                this.EnableControls(true);
            }

            if (errors.Count > 0)
                MessageBox.Show(this, StringUtil.MakePathList(errors, "; "));
        }

        void StartOrStopOneInstance(string strInstanceName,
            string strAction)
        {
            ListViewItem item = null;
            if (this.Visible)
            {
                item = ListViewUtil.FindItem(this.listView_instance, strInstanceName, COLUMN_NAME);
                if (item == null)
                {
                    MessageBox.Show(this, "名为 '" + strInstanceName + "' 实例在列表中没有找到");
                    return;
                }
            }
            List<string> errors = new List<string>();
            this.EnableControls(false);
            try
            {
                string strError = "";

                {
                    int nRet = dp2capo_serviceControl(
        strAction,
        strInstanceName,
        out strError);
                    if (nRet == -1)
                        errors.Add(strError);
                    else
                    {
                        if (item != null)
                            item.ImageIndex = strAction == "stop" ? IMAGEINDEX_STOPPED : IMAGEINDEX_RUNNING;
                    }
                }
            }
            finally
            {
                this.EnableControls(true);
            }

            if (errors.Count > 0)
                MessageBox.Show(this, StringUtil.MakePathList(errors, "; "));
        }

        const int IMAGEINDEX_RUNNING = 0;
        const int IMAGEINDEX_STOPPED = 1;

        // 刷新实例状态显示
        void RefreshInstanceState()
        {
            bool bError = false;
            string strError = "";
            foreach (ListViewItem item in this.listView_instance.Items)
            {
                if (bError)
                {
                    item.ImageIndex = IMAGEINDEX_STOPPED;
                    continue;
                }
                string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);
                int nRet = dp2capo_serviceControl(
                    "getState",
                    strInstanceName,
                    out strError);
                if (nRet == -1)
                {
                    // 只要出错一次，后面就不再调用 dp2library_serviceControl()
                    bError = true;
                    item.ImageIndex = IMAGEINDEX_STOPPED;
                    ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, strError);
                    // TODO: 展开显示 errorinfo 列
                }
                else if (nRet == 0 || strError == "stopped")
                {
                    item.ImageIndex = IMAGEINDEX_STOPPED;
                    ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "");
                }
                else
                {
                    // nRet == 1
                    item.ImageIndex = IMAGEINDEX_RUNNING;
                    ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "");
                }
            }

            RefreshGlobalInstanceState();
        }

        void RefreshGlobalInstanceState()
        {
            string strInstanceName = ".global";
            int nRet = dp2capo_serviceControl(
"getState",
strInstanceName,
out string strError);
            if (nRet == -1)
            {
                SetGlobalRunningState(false);
            }
            else if (nRet == 0 || strError == "stopped")
            {
                SetGlobalRunningState(false);
            }
            else
            {
                SetGlobalRunningState(true);
            }
        }

        class IpcInfo
        {
            public IpcClientChannel Channel { get; set; }
            public IServiceControl Server { get; set; }
        }

        static IpcInfo BeginIpc()
        {
            IpcInfo info = new IpcInfo();

            string strUrl = "ipc://dp2capo_ServiceControlChannel/dp2library_ServiceControlServer";
            info.Channel = new IpcClientChannel();

            ChannelServices.RegisterChannel(info.Channel, false);

            info.Server = (IServiceControl)Activator.GetObject(typeof(IServiceControl),
                strUrl);
            if (info.Server == null)
            {
                string strError = "无法连接到 remoting 服务器 " + strUrl;
                throw new Exception(strError);
            }

            return info;
        }

        static void EndIpc(IpcInfo info)
        {
            ChannelServices.UnregisterChannel(info.Channel);
        }

        // 检测 dp2capo.exe 是否在运行状态
        static bool IsDp2CapoRunning()
        {
            try
            {
                IpcInfo ipc = BeginIpc();
                try
                {
                    ServiceControlResult result = null;
                    InstanceInfo info = null;
                    // 获得一个实例的信息
                    result = ipc.Server.GetInstanceInfo(".",
        out info);
                    if (result.Value == -1)
                        return false;
                    if (info != null)
                        return info.State == "running";
                    else
                        return true;
                }
                finally
                {
                    EndIpc(ipc);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // parameters:
        //      strCommand  start/stop/getState
        // return:
        //      -1  出错
        //      0/1 strCommand 为 "getState" 时分别表示实例 不在运行/在运行 状态
        public static int dp2capo_serviceControl(
    string strCommand,
    string strInstanceName,
    out string strError)
        {
            strError = "";

            try
            {
                IpcInfo ipc = BeginIpc();
                try
                {
                    ServiceControlResult result = null;
                    if (strCommand == "start")
                        result = ipc.Server.StartInstance(strInstanceName);
                    else if (strCommand == "stop")
                        result = ipc.Server.StopInstance(strInstanceName);
                    else if (strCommand == "getState")
                    {
                        InstanceInfo info = null;
                        // 获得一个实例的信息
                        result = ipc.Server.GetInstanceInfo(strInstanceName,
            out info);
                        if (result.Value == -1)
                        {
                            strError = result.ErrorInfo;
                            return -1;
                        }
                        else
                            strError = info.State;
                        return result.Value;
                    }
                    else
                    {
                        strError = "未知的命令 '" + strCommand + "'";
                        return -1;
                    }
                    if (result.Value == -1)
                    {
                        strError = result.ErrorInfo;
                        return -1;
                    }
                    strError = result.ErrorInfo;
                    return 0;

                }
                finally
                {
                    EndIpc(ipc);
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }

        #endregion

        private void listView_instance_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            {
                menuItem = new MenuItem("启动实例 [" + this.listView_instance.SelectedItems.Count + "] (&S)");
                menuItem.Click += new System.EventHandler(this.menu_startInstance_Click);
                if (this.listView_instance.SelectedItems.Count == 0)
                    menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);
            }


            {
                menuItem = new MenuItem("停止实例 [" + this.listView_instance.SelectedItems.Count + "] (&T)");
                menuItem.Click += new System.EventHandler(this.menu_stopInstance_Click);
                if (this.listView_instance.SelectedItems.Count == 0)
                    menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);
            }

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            {
                menuItem = new MenuItem("启动全局服务 (&S)");
                menuItem.Click += new System.EventHandler(this.menu_startGlobalInstance_Click);
                contextMenu.MenuItems.Add(menuItem);
            }


            {
                menuItem = new MenuItem("停止全局服务 (&T)");
                menuItem.Click += new System.EventHandler(this.menu_stopGlobalInstance_Click);
                contextMenu.MenuItems.Add(menuItem);
            }

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            {
                menuItem = new MenuItem("刷新状态(&R)");
                menuItem.Click += new System.EventHandler(this.menu_refreshInstanceState_Click);
                if (this.listView_instance.Items.Count == 0)
                    menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);
            }

            contextMenu.Show(this.listView_instance, new Point(e.X, e.Y));
        }

        // 启动所选的实例
        void menu_startInstance_Click(object sender, EventArgs e)
        {
            StartOrStopInstance("start");
        }

        // 停止所选的实例
        void menu_stopInstance_Click(object sender, EventArgs e)
        {
            StartOrStopInstance("stop");
        }

        // 启动全局服务
        void menu_startGlobalInstance_Click(object sender, EventArgs e)
        {
            StartOrStopGlobalInstance("start");
        }

        // 停止全局服务
        void menu_stopGlobalInstance_Click(object sender, EventArgs e)
        {
            StartOrStopGlobalInstance("stop");
        }

        // 刷新全部事项的状态显示
        void menu_refreshInstanceState_Click(object sender, EventArgs e)
        {
            RefreshInstanceState();
        }

        void EnableControls(bool bEnable)
        {
            if (this.Enabled == false)
                return;

            this.listView_instance.Enabled = bEnable;
            this.button_OK.Enabled = bEnable;
            this.button_newInstance.Enabled = bEnable;
            this.button_modifyInstance.Enabled = bEnable;
            this.button_deleteInstance.Enabled = bEnable;

            this.button_getDataDir.Enabled = bEnable;
            this.button_globalConfig.Enabled = bEnable;
            this.textBox_dataDir.Enabled = bEnable;
        }

        bool IsLocking(string strInstanceName)
        {
            if (LockingInstances.IndexOf(strInstanceName) == -1)
                return false;
            return true;
        }

        void LockInstance(string strInstanceName, bool bLock)
        {
            if (bLock)
            {
                if (LockingInstances.IndexOf(strInstanceName) == -1)
                    LockingInstances.Add(strInstanceName);
            }
            else
            {
                LockingInstances.Remove(strInstanceName);
            }
        }

        // 被锁定的实例名数组
        // 正在进行恢复操作的实例名，会进入本数组。以防中途被启动
        // 引用外部值
        public List<string> LockingInstances { get; set; }

    }
}
