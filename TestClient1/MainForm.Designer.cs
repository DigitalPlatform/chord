namespace TestClient1
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_config = new System.Windows.Forms.TabPage();
            this.textBox_config_password = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_config_userName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_config_messageServerUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_getPatronInfo = new System.Windows.Forms.TabPage();
            this.textBox_getReaderInfo_results = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_getReaderInfo_formatList = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_getReaderInfo_queryWord = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_getReaderInfo_remoteUserName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage_searchReader = new System.Windows.Forms.TabPage();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_begin = new System.Windows.Forms.ToolStripButton();
            this.tabControl_main.SuspendLayout();
            this.tabPage_config.SuspendLayout();
            this.tabPage_getPatronInfo.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(487, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 315);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(487, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_config);
            this.tabControl_main.Controls.Add(this.tabPage_getPatronInfo);
            this.tabControl_main.Controls.Add(this.tabPage_searchReader);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 49);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(487, 266);
            this.tabControl_main.TabIndex = 2;
            // 
            // tabPage_config
            // 
            this.tabPage_config.Controls.Add(this.textBox_config_password);
            this.tabPage_config.Controls.Add(this.label3);
            this.tabPage_config.Controls.Add(this.textBox_config_userName);
            this.tabPage_config.Controls.Add(this.label2);
            this.tabPage_config.Controls.Add(this.textBox_config_messageServerUrl);
            this.tabPage_config.Controls.Add(this.label1);
            this.tabPage_config.Location = new System.Drawing.Point(4, 22);
            this.tabPage_config.Name = "tabPage_config";
            this.tabPage_config.Size = new System.Drawing.Size(479, 240);
            this.tabPage_config.TabIndex = 2;
            this.tabPage_config.Text = "Config";
            this.tabPage_config.UseVisualStyleBackColor = true;
            // 
            // textBox_config_password
            // 
            this.textBox_config_password.Location = new System.Drawing.Point(107, 114);
            this.textBox_config_password.Name = "textBox_config_password";
            this.textBox_config_password.PasswordChar = '*';
            this.textBox_config_password.Size = new System.Drawing.Size(161, 21);
            this.textBox_config_password.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "Password:";
            // 
            // textBox_config_userName
            // 
            this.textBox_config_userName.Location = new System.Drawing.Point(107, 87);
            this.textBox_config_userName.Name = "textBox_config_userName";
            this.textBox_config_userName.Size = new System.Drawing.Size(161, 21);
            this.textBox_config_userName.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "User Name:";
            // 
            // textBox_config_messageServerUrl
            // 
            this.textBox_config_messageServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_config_messageServerUrl.Location = new System.Drawing.Point(11, 36);
            this.textBox_config_messageServerUrl.Name = "textBox_config_messageServerUrl";
            this.textBox_config_messageServerUrl.Size = new System.Drawing.Size(460, 21);
            this.textBox_config_messageServerUrl.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "dp2MServer URL:";
            // 
            // tabPage_getPatronInfo
            // 
            this.tabPage_getPatronInfo.Controls.Add(this.textBox_getReaderInfo_results);
            this.tabPage_getPatronInfo.Controls.Add(this.label7);
            this.tabPage_getPatronInfo.Controls.Add(this.textBox_getReaderInfo_formatList);
            this.tabPage_getPatronInfo.Controls.Add(this.label6);
            this.tabPage_getPatronInfo.Controls.Add(this.textBox_getReaderInfo_queryWord);
            this.tabPage_getPatronInfo.Controls.Add(this.label5);
            this.tabPage_getPatronInfo.Controls.Add(this.textBox_getReaderInfo_remoteUserName);
            this.tabPage_getPatronInfo.Controls.Add(this.label4);
            this.tabPage_getPatronInfo.Location = new System.Drawing.Point(4, 22);
            this.tabPage_getPatronInfo.Name = "tabPage_getPatronInfo";
            this.tabPage_getPatronInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_getPatronInfo.Size = new System.Drawing.Size(479, 240);
            this.tabPage_getPatronInfo.TabIndex = 0;
            this.tabPage_getPatronInfo.Text = "GetPatronInfo";
            this.tabPage_getPatronInfo.UseVisualStyleBackColor = true;
            // 
            // textBox_getReaderInfo_results
            // 
            this.textBox_getReaderInfo_results.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_getReaderInfo_results.Location = new System.Drawing.Point(132, 95);
            this.textBox_getReaderInfo_results.Multiline = true;
            this.textBox_getReaderInfo_results.Name = "textBox_getReaderInfo_results";
            this.textBox_getReaderInfo_results.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_getReaderInfo_results.Size = new System.Drawing.Size(339, 133);
            this.textBox_getReaderInfo_results.TabIndex = 11;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 98);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 10;
            this.label7.Text = "Results:";
            // 
            // textBox_getReaderInfo_formatList
            // 
            this.textBox_getReaderInfo_formatList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_getReaderInfo_formatList.Location = new System.Drawing.Point(132, 69);
            this.textBox_getReaderInfo_formatList.Name = "textBox_getReaderInfo_formatList";
            this.textBox_getReaderInfo_formatList.Size = new System.Drawing.Size(339, 21);
            this.textBox_getReaderInfo_formatList.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 72);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 8;
            this.label6.Text = "Format List:";
            // 
            // textBox_getReaderInfo_queryWord
            // 
            this.textBox_getReaderInfo_queryWord.Location = new System.Drawing.Point(132, 42);
            this.textBox_getReaderInfo_queryWord.Name = "textBox_getReaderInfo_queryWord";
            this.textBox_getReaderInfo_queryWord.Size = new System.Drawing.Size(161, 21);
            this.textBox_getReaderInfo_queryWord.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 45);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 12);
            this.label5.TabIndex = 6;
            this.label5.Text = "Query Word:";
            // 
            // textBox_getReaderInfo_remoteUserName
            // 
            this.textBox_getReaderInfo_remoteUserName.Location = new System.Drawing.Point(132, 15);
            this.textBox_getReaderInfo_remoteUserName.Name = "textBox_getReaderInfo_remoteUserName";
            this.textBox_getReaderInfo_remoteUserName.Size = new System.Drawing.Size(161, 21);
            this.textBox_getReaderInfo_remoteUserName.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "Remote User Name:";
            // 
            // tabPage_searchReader
            // 
            this.tabPage_searchReader.Location = new System.Drawing.Point(4, 22);
            this.tabPage_searchReader.Name = "tabPage_searchReader";
            this.tabPage_searchReader.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_searchReader.Size = new System.Drawing.Size(479, 240);
            this.tabPage_searchReader.TabIndex = 1;
            this.tabPage_searchReader.Text = "SearchReader";
            this.tabPage_searchReader.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_begin});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(487, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_begin
            // 
            this.toolStripButton_begin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_begin.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_begin.Image")));
            this.toolStripButton_begin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_begin.Name = "toolStripButton_begin";
            this.toolStripButton_begin.Size = new System.Drawing.Size(45, 22);
            this.toolStripButton_begin.Text = "Begin";
            this.toolStripButton_begin.Click += new System.EventHandler(this.toolStripButton_begin_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(487, 337);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_config.ResumeLayout(false);
            this.tabPage_config.PerformLayout();
            this.tabPage_getPatronInfo.ResumeLayout(false);
            this.tabPage_getPatronInfo.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_config;
        private System.Windows.Forms.TabPage tabPage_getPatronInfo;
        private System.Windows.Forms.TabPage tabPage_searchReader;
        private System.Windows.Forms.TextBox textBox_config_messageServerUrl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_config_password;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_config_userName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_getReaderInfo_remoteUserName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_getReaderInfo_formatList;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_getReaderInfo_queryWord;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_getReaderInfo_results;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_begin;
    }
}

