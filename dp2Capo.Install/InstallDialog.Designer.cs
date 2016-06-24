namespace dp2Capo.Install
{
    partial class InstallDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_deleteInstance = new System.Windows.Forms.Button();
            this.button_modifyInstance = new System.Windows.Forms.Button();
            this.button_newInstance = new System.Windows.Forms.Button();
            this.listView_instance = new System.Windows.Forms.ListView();
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_errorInfo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_dp2library_url = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_dp2MServer_url = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_dataDir = new System.Windows.Forms.TextBox();
            this.button_getDataDir = new System.Windows.Forms.Button();
            this.columnHeader_dataDir = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(419, 281);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 20;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(338, 281);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 19;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_deleteInstance
            // 
            this.button_deleteInstance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_deleteInstance.Location = new System.Drawing.Point(205, 252);
            this.button_deleteInstance.Name = "button_deleteInstance";
            this.button_deleteInstance.Size = new System.Drawing.Size(75, 23);
            this.button_deleteInstance.TabIndex = 18;
            this.button_deleteInstance.Text = "删除(&D)";
            this.button_deleteInstance.UseVisualStyleBackColor = true;
            this.button_deleteInstance.Click += new System.EventHandler(this.button_deleteInstance_Click);
            // 
            // button_modifyInstance
            // 
            this.button_modifyInstance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_modifyInstance.Location = new System.Drawing.Point(109, 252);
            this.button_modifyInstance.Name = "button_modifyInstance";
            this.button_modifyInstance.Size = new System.Drawing.Size(90, 23);
            this.button_modifyInstance.TabIndex = 17;
            this.button_modifyInstance.Text = "修改(&M)...";
            this.button_modifyInstance.UseVisualStyleBackColor = true;
            this.button_modifyInstance.Click += new System.EventHandler(this.button_modifyInstance_Click);
            // 
            // button_newInstance
            // 
            this.button_newInstance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_newInstance.Location = new System.Drawing.Point(12, 252);
            this.button_newInstance.Name = "button_newInstance";
            this.button_newInstance.Size = new System.Drawing.Size(91, 23);
            this.button_newInstance.TabIndex = 16;
            this.button_newInstance.Text = "新增(&N)...";
            this.button_newInstance.UseVisualStyleBackColor = true;
            this.button_newInstance.Click += new System.EventHandler(this.button_newInstance_Click);
            // 
            // listView_instance
            // 
            this.listView_instance.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_instance.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name,
            this.columnHeader_errorInfo,
            this.columnHeader_dataDir,
            this.columnHeader_dp2library_url,
            this.columnHeader_dp2MServer_url});
            this.listView_instance.FullRowSelect = true;
            this.listView_instance.HideSelection = false;
            this.listView_instance.Location = new System.Drawing.Point(12, 62);
            this.listView_instance.MultiSelect = false;
            this.listView_instance.Name = "listView_instance";
            this.listView_instance.Size = new System.Drawing.Size(482, 184);
            this.listView_instance.TabIndex = 15;
            this.listView_instance.UseCompatibleStateImageBehavior = false;
            this.listView_instance.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "实例名";
            this.columnHeader_name.Width = 103;
            // 
            // columnHeader_errorInfo
            // 
            this.columnHeader_errorInfo.Text = "出错信息";
            // 
            // columnHeader_dp2library_url
            // 
            this.columnHeader_dp2library_url.Text = "dp2Library 服务器";
            this.columnHeader_dp2library_url.Width = 250;
            // 
            // columnHeader_dp2MServer_url
            // 
            this.columnHeader_dp2MServer_url.Text = "dp2MServer 服务器";
            this.columnHeader_dp2MServer_url.Width = 250;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 21;
            this.label1.Text = "数据目录:";
            // 
            // textBox_dataDir
            // 
            this.textBox_dataDir.Location = new System.Drawing.Point(12, 28);
            this.textBox_dataDir.Name = "textBox_dataDir";
            this.textBox_dataDir.Size = new System.Drawing.Size(340, 21);
            this.textBox_dataDir.TabIndex = 22;
            // 
            // button_getDataDir
            // 
            this.button_getDataDir.Location = new System.Drawing.Point(358, 26);
            this.button_getDataDir.Name = "button_getDataDir";
            this.button_getDataDir.Size = new System.Drawing.Size(35, 23);
            this.button_getDataDir.TabIndex = 23;
            this.button_getDataDir.Text = "...";
            this.button_getDataDir.UseVisualStyleBackColor = true;
            this.button_getDataDir.Click += new System.EventHandler(this.button_getDataDir_Click);
            // 
            // columnHeader_dataDir
            // 
            this.columnHeader_dataDir.Text = "数据目录";
            this.columnHeader_dataDir.Width = 150;
            // 
            // InstallDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(506, 316);
            this.Controls.Add(this.button_getDataDir);
            this.Controls.Add(this.textBox_dataDir);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_deleteInstance);
            this.Controls.Add(this.button_modifyInstance);
            this.Controls.Add(this.button_newInstance);
            this.Controls.Add(this.listView_instance);
            this.Name = "InstallDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "安装 dp2Capo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InstallDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.InstallDialog_FormClosed);
            this.Load += new System.EventHandler(this.InstallDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_deleteInstance;
        private System.Windows.Forms.Button button_modifyInstance;
        private System.Windows.Forms.Button button_newInstance;
        private System.Windows.Forms.ListView listView_instance;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.ColumnHeader columnHeader_errorInfo;
        private System.Windows.Forms.ColumnHeader columnHeader_dp2library_url;
        private System.Windows.Forms.ColumnHeader columnHeader_dp2MServer_url;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_dataDir;
        private System.Windows.Forms.Button button_getDataDir;
        private System.Windows.Forms.ColumnHeader columnHeader_dataDir;
    }
}