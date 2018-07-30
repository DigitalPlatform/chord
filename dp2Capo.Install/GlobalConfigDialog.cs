using DigitalPlatform.IO;
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
            catch (DirectoryNotFoundException)
            {
                _cfgDom.LoadXml("<root />");
            }

            {
                if (!(_cfgDom.DocumentElement.SelectSingleNode("zServer") is XmlElement zServer))
                {
                    zServer = _cfgDom.CreateElement("zServer");
                    _cfgDom.DocumentElement.AppendChild(zServer);
                    this.Changed = true;
                }

                this.textBox_z3950ListeningPort.Text = zServer.GetAttribute("port");
                SetZ3950EnabledByPortNumber();
            }

            {
                if (!(_cfgDom.DocumentElement.SelectSingleNode("sipServer") is XmlElement sipServer))
                {
                    sipServer = _cfgDom.CreateElement("sipServer");
                    _cfgDom.DocumentElement.AppendChild(sipServer);
                    this.Changed = true;
                }

                this.textBox_sipListeningPort.Text = sipServer.GetAttribute("port");
                SetSipEnabledByPortNumber();
            }
        }

        // 把界面上的配置值兑现到配置文件
        void Restore()
        {
            if (string.IsNullOrEmpty(this.textBox_z3950ListeningPort.Text) == false
                && this.textBox_z3950ListeningPort.Text == this.textBox_sipListeningPort.Text)
                throw new Exception("Z39.50 端口号 '" + this.textBox_z3950ListeningPort.Text + "' 和 SIP 端口号 '" + this.textBox_sipListeningPort.Text + "' 冲突了");

            Debug.Assert(_cfgDom != null && _cfgDom.DocumentElement != null, "");

            {
                if (!(_cfgDom.DocumentElement.SelectSingleNode("zServer") is XmlElement zServer))
                {
                    zServer = _cfgDom.CreateElement("zServer");
                    _cfgDom.DocumentElement.AppendChild(zServer);
                    this.Changed = true;
                }

                if (string.IsNullOrEmpty(this.textBox_z3950ListeningPort.Text) == false)
                {
                    if (Int32.TryParse(this.textBox_z3950ListeningPort.Text, out int v) == false)
                        throw new Exception("Z39.50 端口号必须是纯数字");
                    if (v < 0)
                        throw new Exception("Z39.50 端口号必须大于等于 0");
                }

                zServer.SetAttribute("port", this.textBox_z3950ListeningPort.Text);
            }

            {
                if (!(_cfgDom.DocumentElement.SelectSingleNode("sipServer") is XmlElement sipServer))
                {
                    sipServer = _cfgDom.CreateElement("sipServer");
                    _cfgDom.DocumentElement.AppendChild(sipServer);
                    this.Changed = true;
                }

                if (string.IsNullOrEmpty(this.textBox_sipListeningPort.Text) == false)
                {
                    if (Int32.TryParse(this.textBox_sipListeningPort.Text, out int v) == false)
                        throw new Exception("SIP 端口号必须是纯数字");
                    if (v < 0)
                        throw new Exception("SIP 端口号必须大于等于 0");
                }

                sipServer.SetAttribute("port", this.textBox_sipListeningPort.Text);
            }

            string filename = this.GetCfgFileName();

            PathUtil.CreateDirIfNeed(Path.GetDirectoryName(filename));

            _cfgDom.Save(filename);
        }

        // 根据 this.textBox_z3950ListeningPort.Text 设置 
        void SetZ3950EnabledByPortNumber()
        {
            if (string.IsNullOrEmpty(this.textBox_z3950ListeningPort.Text))
            {
                this.checkBox_enableZ3950Server.Checked = false;
                this.textBox_z3950ListeningPort.Enabled = false;
            }
            else
            {
                this.checkBox_enableZ3950Server.Checked = true;
                this.textBox_z3950ListeningPort.Enabled = true;
            }
        }

        // 根据 this.textBox_sipListeningPort.Text 设置 
        void SetSipEnabledByPortNumber()
        {
            if (string.IsNullOrEmpty(this.textBox_sipListeningPort.Text))
            {
                this.checkBox_enableSipServer.Checked = false;
                this.textBox_sipListeningPort.Enabled = false;
            }
            else
            {
                this.checkBox_enableSipServer.Checked = true;
                this.textBox_sipListeningPort.Enabled = true;
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
                if (string.IsNullOrEmpty(this.textBox_z3950ListeningPort.Text))
                    this.textBox_z3950ListeningPort.Text = "210";
                this.textBox_z3950ListeningPort.Enabled = true;
            }
            else
            {
                this.textBox_z3950ListeningPort.Text = "";
                this.textBox_z3950ListeningPort.Enabled = false;
            }
        }

        private void checkBox_enableSipServer_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_enableSipServer.Checked == true)
            {
                if (string.IsNullOrEmpty(this.textBox_sipListeningPort.Text))
                    this.textBox_sipListeningPort.Text = "8100";
                this.textBox_sipListeningPort.Enabled = true;
            }
            else
            {
                this.textBox_sipListeningPort.Text = "";
                this.textBox_sipListeningPort.Enabled = false;
            }
        }
    }
}
