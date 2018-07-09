using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace dp2Capo.Install
{
    public partial class GlobalConfigDialog : Form
    {
        // 数据目录
        public string DataDir { get; set; }

        // 配置是否被修改过
        public bool Changed { get; set; }

        XmlDocument _cfgDom = new XmlDocument();

        public GlobalConfigDialog()
        {
            InitializeComponent();
        }

        private void GlobalConfigDialog_Load(object sender, EventArgs e)
        {
            Fill();
        }

        string GetCfgFileName()
        {
            if (string.IsNullOrEmpty(this.DataDir))
                throw new Exception("this.DataDir 为空");
            return Path.Combine(this.DataDir, "config.xml");
        }

        // 从配置文件获取数据，填充到界面
        void Fill()
        {
            string filename = this.GetCfgFileName();

            try
            {
                _cfgDom.Load(filename);
            }
            catch (FileNotFoundException)
            {
                _cfgDom.LoadXml("<root />");
            }

            XmlElement zServer = _cfgDom.DocumentElement.SelectSingleNode("zServer") as XmlElement;
            if (zServer == null)
            {
                zServer = _cfgDom.CreateElement("zServer");
                _cfgDom.DocumentElement.AppendChild(zServer);
                this.Changed = true;
            }

            this.textBox_listeningPort.Text = zServer.GetAttribute("port");
            SetEnabledByPortNumber();
        }

        // 把界面上的配置值兑现到配置文件
        void Restore()
        {
            Debug.Assert(_cfgDom != null && _cfgDom.DocumentElement != null, "");

            XmlElement zServer = _cfgDom.DocumentElement.SelectSingleNode("zServer") as XmlElement;
            if (zServer == null)
            {
                zServer = _cfgDom.CreateElement("zServer");
                _cfgDom.DocumentElement.AppendChild(zServer);
                this.Changed = true;
            }

            if (string.IsNullOrEmpty(this.textBox_listeningPort.Text) == false)
            {
                if (Int32.TryParse(this.textBox_listeningPort.Text, out int v) == false)
                    throw new Exception("端口号必须是纯数字");
                if (v < 0)
                    throw new Exception("端口号必须大于等于 0");
            }

            zServer.SetAttribute("port", this.textBox_listeningPort.Text);

            string filename = this.GetCfgFileName();
            _cfgDom.Save(filename);
        }

        // 根据 this.textBox_listeningPort.Text 设置 
        void SetEnabledByPortNumber()
        {
            if (string.IsNullOrEmpty(this.textBox_listeningPort.Text))
            {
                this.checkBox_enableZ3950Server.Checked = false;
                this.textBox_listeningPort.Enabled = false;
            }
            else
            {
                this.checkBox_enableZ3950Server.Checked = true;
                this.textBox_listeningPort.Enabled = true;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            try
            {
                Restore();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void checkBox_enableZ3950Server_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_enableZ3950Server.Checked == true)
            {
                if (string.IsNullOrEmpty(this.textBox_listeningPort.Text))
                    this.textBox_listeningPort.Text = "210";
                this.textBox_listeningPort.Enabled = true;
            }
            else
            {
                this.textBox_listeningPort.Text = "";
                this.textBox_listeningPort.Enabled = false;
            }
        }
    }
}
