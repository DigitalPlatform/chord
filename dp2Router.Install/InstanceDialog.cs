using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.Drawing;
using DigitalPlatform.IO;

namespace dp2Router.Install
{
    public partial class InstanceDialog : Form
    {
        // config.xml 配置文件内容
        public XmlDocument CfgDom { get; set; }

        string _cfgFileName = "";

        public bool Changed { get; set; }

        public InstanceDialog()
        {
            InitializeComponent();
        }

        private void InstanceDialog_Load(object sender, EventArgs e)
        {
            LoadCfgXml();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_dataDir.Text))
            {
                strError = "尚未指定数据目录";
                goto ERROR1;
            }

            this.SaveCfgXml();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void button_getDataDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

            dir_dlg.Description = "请指定 dp2Router 数据目录:";
            dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dir_dlg.ShowNewFolderButton = true;
            dir_dlg.SelectedPath = this.textBox_dataDir.Text;

            if (dir_dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_dataDir.Text = dir_dlg.SelectedPath;
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

        public int Port
        {
            get
            {
                return (int)this.numericUpDown_port.Value;
            }
            set
            {
                this.numericUpDown_port.Value = value;
            }
        }

        private void button_edit_dp2mserver_Click(object sender, EventArgs e)
        {
            dp2MServerDialog dlg = new dp2MServerDialog();
            FontUtil.AutoSetDefaultFont(dlg);

            dlg.CfgDom = this.CfgDom;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.Changed = true;
            // 刷新显示
            this.DisplayDp2mserverInfo();

        }

        void LoadCfgXml()
        {
            try
            {
                this.CfgDom = new XmlDocument();
                this.CfgDom.LoadXml("<root />");

                if (string.IsNullOrEmpty(this.DataDir))
                    return;
                if (Directory.Exists(this.DataDir) == false)
                    return;

                string strFileName = Path.Combine(this.DataDir, "config.xml");
                try
                {
                    this.CfgDom.Load(strFileName);
                }
                catch (FileNotFoundException)
                {
                    this.CfgDom.LoadXml("<root />");
                }

                _cfgFileName = strFileName;
            }
            finally
            {
                DisplayPortInfo();
                DisplayDp2mserverInfo();
            }
        }

        void DisplayPortInfo()
        {
            XmlElement httpServer = this.CfgDom.DocumentElement.SelectSingleNode("httpServer") as XmlElement;
            if (httpServer == null)
                return;

            try
            {
                string strPort = httpServer.GetAttribute("port");
                if (string.IsNullOrEmpty(strPort) == true)
                    return;

                this.Port = Convert.ToInt32(strPort);
            }
            catch (Exception ex)
            {
                string strError = "从 httpServer 元素 port 属性中获得端口号时出现异常: " + ExceptionUtil.GetExceptionMessage(ex);
                MessageBox.Show(this, strError);
            }
        }

        void DisplayDp2mserverInfo()
        {
            this.textBox_dp2mserver_def.Text = dp2MServerDialog.GetDisplayText(this.CfgDom);
        }

        void SaveCfgXml()
        {
            PathUtil.CreateDirIfNeed(this.DataDir);

            // 保存 port
            {
                XmlElement httpServer = this.CfgDom.DocumentElement.SelectSingleNode("httpServer") as XmlElement;
                if (httpServer == null)
                {
                    httpServer = this.CfgDom.CreateElement("httpServer");
                    this.CfgDom.DocumentElement.AppendChild(httpServer);
                }

                httpServer.SetAttribute("port", this.Port.ToString());
            }

            string strFileName = Path.Combine(this.DataDir, "config.xml");
            this.CfgDom.Save(strFileName);
            this.Changed = false;
        }

        private void textBox_dataDir_Leave(object sender, EventArgs e)
        {
            // 数据目录修改了，重新装载初始数据
            if (string.IsNullOrEmpty(_cfgFileName) == false
                && Path.GetDirectoryName(_cfgFileName) != this.DataDir)
                LoadCfgXml();
        }

        // 是否允许编辑 DateDir
        public bool EnableDataDir
        {
            get
            {
                return !this.textBox_dataDir.ReadOnly;
            }
            set
            {
                this.textBox_dataDir.ReadOnly = !value;
                this.button_getDataDir.Enabled = value;
            }
        }
    }
}
