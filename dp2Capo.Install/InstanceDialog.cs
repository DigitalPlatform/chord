﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Drawing;
using DigitalPlatform.IO;

namespace dp2Capo.Install
{
    public partial class InstanceDialog : Form
    {
        // capo.xml 配置文件内容
        public XmlDocument CfgDom { get; set; }

        public bool Changed { get; set; }

        public InstanceDialog()
        {
            InitializeComponent();
        }

        public string InstanceName
        {
            get
            {
                return this.textBox_instanceName.Text;
            }
            set
            {
                this.textBox_instanceName.Text = value;
            }
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

        private void InstanceDialog_Load(object sender, EventArgs e)
        {
            LoadCfgXml();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 检查参数是否输入全了
            if (string.IsNullOrEmpty(this.textBox_dataDir.Text))
            {
                strError = "尚未指定数据目录";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_dp2library_def.Text))
            {
                strError = "尚未配置 dp2library 服务器参数";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_dp2mserver_def.Text))
            {
                // dp2mserver 参数或者 Z39.50 服务器参数，至少要配置了其中一组，否则就报错
#if NO
                strError = "尚未配置 dp2MServer 服务器参数";
                goto ERROR1;
#endif
            }

            if (this.Changed)
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

        void LoadCfgXml()
        {
            this.CfgDom = new XmlDocument();
            this.CfgDom.LoadXml("<root />");

            if (string.IsNullOrEmpty(this.DataDir))
                return;
            if (Directory.Exists(this.DataDir) == false)
                return;

            string strFileName = Path.Combine(this.DataDir, "capo.xml");
            try
            {
                this.CfgDom.Load(strFileName);
            }
            catch (FileNotFoundException)
            {
                this.CfgDom.LoadXml("<root />");
            }

            DisplayDp2libraryInfo();
            DisplayDp2mserverInfo();
            DisplayDp2zserverInfo();
        }

        void DisplayDp2libraryInfo()
        {
            this.textBox_dp2library_def.Text = dp2LibraryDialog.GetDisplayText(this.CfgDom);
        }

        void DisplayDp2mserverInfo()
        {
            this.textBox_dp2mserver_def.Text = dp2MServerDialog.GetDisplayText(this.CfgDom);
        }

        void DisplayDp2zserverInfo()
        {
            this.textBox_z3950_def.Text = InstallZServerDlg.GetDisplayText(this.CfgDom);
        }

        void SaveCfgXml()
        {
            PathUtil.CreateDirIfNeed(this.DataDir);

            string strFileName = Path.Combine(this.DataDir, "capo.xml");
            this.CfgDom.Save(strFileName);
            this.Changed = false;
        }

        private void button_edit_dp2library_Click(object sender, EventArgs e)
        {
            dp2LibraryDialog dlg = new dp2LibraryDialog();
            FontUtil.AutoSetDefaultFont(dlg);

            dlg.CfgDom = this.CfgDom;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.Changed = true;
            // 刷新显示
            this.DisplayDp2libraryInfo();
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

        private void button_edit_z3950_Click(object sender, EventArgs e)
        {
            InstallZServerDlg dlg = new InstallZServerDlg();
            FontUtil.AutoSetDefaultFont(dlg);

            dlg.CfgDom = this.CfgDom;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.Changed = true;
            // 刷新显示
            this.DisplayDp2zserverInfo();
        }
    }
}
