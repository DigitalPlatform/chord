namespace TestZClient
{
    partial class Form1
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel_query = new System.Windows.Forms.Panel();
            this.button_nextBatch = new System.Windows.Forms.Button();
            this.button_close = new System.Windows.Forms.Button();
            this.textBox_database = new System.Windows.Forms.TextBox();
            this.button_search = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_use = new System.Windows.Forms.ComboBox();
            this.textBox_serverAddr = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_queryWord = new System.Windows.Forms.TextBox();
            this.textBox_serverPort = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_groupID = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton_authenStyleIdpass = new System.Windows.Forms.RadioButton();
            this.radioButton_authenStyleOpen = new System.Windows.Forms.RadioButton();
            this.button_stop = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel_query.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(851, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(851, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Location = new System.Drawing.Point(0, 584);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(851, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 49);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel_query);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.webBrowser1);
            this.splitContainer1.Size = new System.Drawing.Size(851, 535);
            this.splitContainer1.SplitterDistance = 380;
            this.splitContainer1.SplitterWidth = 8;
            this.splitContainer1.TabIndex = 3;
            // 
            // panel_query
            // 
            this.panel_query.AutoScroll = true;
            this.panel_query.Controls.Add(this.button_stop);
            this.panel_query.Controls.Add(this.textBox_password);
            this.panel_query.Controls.Add(this.label7);
            this.panel_query.Controls.Add(this.textBox_userName);
            this.panel_query.Controls.Add(this.label6);
            this.panel_query.Controls.Add(this.textBox_groupID);
            this.panel_query.Controls.Add(this.label8);
            this.panel_query.Controls.Add(this.groupBox1);
            this.panel_query.Controls.Add(this.button_nextBatch);
            this.panel_query.Controls.Add(this.button_close);
            this.panel_query.Controls.Add(this.textBox_database);
            this.panel_query.Controls.Add(this.button_search);
            this.panel_query.Controls.Add(this.label1);
            this.panel_query.Controls.Add(this.comboBox_use);
            this.panel_query.Controls.Add(this.textBox_serverAddr);
            this.panel_query.Controls.Add(this.label5);
            this.panel_query.Controls.Add(this.label2);
            this.panel_query.Controls.Add(this.textBox_queryWord);
            this.panel_query.Controls.Add(this.textBox_serverPort);
            this.panel_query.Controls.Add(this.label4);
            this.panel_query.Controls.Add(this.label3);
            this.panel_query.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_query.Location = new System.Drawing.Point(0, 0);
            this.panel_query.Name = "panel_query";
            this.panel_query.Size = new System.Drawing.Size(380, 535);
            this.panel_query.TabIndex = 0;
            // 
            // button_nextBatch
            // 
            this.button_nextBatch.Enabled = false;
            this.button_nextBatch.Location = new System.Drawing.Point(112, 216);
            this.button_nextBatch.Name = "button_nextBatch";
            this.button_nextBatch.Size = new System.Drawing.Size(109, 30);
            this.button_nextBatch.TabIndex = 12;
            this.button_nextBatch.Text = ">>";
            this.button_nextBatch.UseVisualStyleBackColor = true;
            this.button_nextBatch.Click += new System.EventHandler(this.button_nextBatch_Click);
            // 
            // button_close
            // 
            this.button_close.Location = new System.Drawing.Point(227, 216);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(109, 30);
            this.button_close.TabIndex = 13;
            this.button_close.Text = "切断通道";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click);
            // 
            // textBox_database
            // 
            this.textBox_database.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_database.Location = new System.Drawing.Point(112, 78);
            this.textBox_database.Name = "textBox_database";
            this.textBox_database.Size = new System.Drawing.Size(265, 28);
            this.textBox_database.TabIndex = 5;
            // 
            // button_search
            // 
            this.button_search.Location = new System.Drawing.Point(112, 180);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(109, 30);
            this.button_search.TabIndex = 10;
            this.button_search.Text = "检索";
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "地址:";
            // 
            // comboBox_use
            // 
            this.comboBox_use.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_use.FormattingEnabled = true;
            this.comboBox_use.Location = new System.Drawing.Point(112, 147);
            this.comboBox_use.Name = "comboBox_use";
            this.comboBox_use.Size = new System.Drawing.Size(265, 26);
            this.comboBox_use.TabIndex = 9;
            // 
            // textBox_serverAddr
            // 
            this.textBox_serverAddr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverAddr.Location = new System.Drawing.Point(112, 10);
            this.textBox_serverAddr.Name = "textBox_serverAddr";
            this.textBox_serverAddr.Size = new System.Drawing.Size(265, 28);
            this.textBox_serverAddr.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 150);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 18);
            this.label5.TabIndex = 8;
            this.label5.Text = "检索途径:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "端口号:";
            // 
            // textBox_queryWord
            // 
            this.textBox_queryWord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_queryWord.Location = new System.Drawing.Point(112, 112);
            this.textBox_queryWord.Name = "textBox_queryWord";
            this.textBox_queryWord.Size = new System.Drawing.Size(265, 28);
            this.textBox_queryWord.TabIndex = 7;
            // 
            // textBox_serverPort
            // 
            this.textBox_serverPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverPort.Location = new System.Drawing.Point(112, 44);
            this.textBox_serverPort.Name = "textBox_serverPort";
            this.textBox_serverPort.Size = new System.Drawing.Size(265, 28);
            this.textBox_serverPort.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 115);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 18);
            this.label4.TabIndex = 6;
            this.label4.Text = "检索词:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 18);
            this.label3.TabIndex = 4;
            this.label3.Text = "数据库名:";
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(463, 535);
            this.webBrowser1.TabIndex = 0;
            // 
            // textBox_password
            // 
            this.textBox_password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_password.Location = new System.Drawing.Point(129, 507);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(222, 28);
            this.textBox_password.TabIndex = 20;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 509);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(80, 18);
            this.label7.TabIndex = 19;
            this.label7.Text = "密码(&P):";
            // 
            // textBox_userName
            // 
            this.textBox_userName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_userName.Location = new System.Drawing.Point(129, 473);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(222, 28);
            this.textBox_userName.TabIndex = 18;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 475);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(98, 18);
            this.label6.TabIndex = 17;
            this.label6.Text = "用户名(&U):";
            // 
            // textBox_groupID
            // 
            this.textBox_groupID.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_groupID.Location = new System.Drawing.Point(129, 440);
            this.textBox_groupID.Name = "textBox_groupID";
            this.textBox_groupID.Size = new System.Drawing.Size(222, 28);
            this.textBox_groupID.TabIndex = 16;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 442);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(89, 18);
            this.label8.TabIndex = 15;
            this.label8.Text = "&Group ID:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton_authenStyleIdpass);
            this.groupBox1.Controls.Add(this.radioButton_authenStyleOpen);
            this.groupBox1.Location = new System.Drawing.Point(14, 313);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(339, 116);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 权限验证方式 ";
            // 
            // radioButton_authenStyleIdpass
            // 
            this.radioButton_authenStyleIdpass.AutoSize = true;
            this.radioButton_authenStyleIdpass.Location = new System.Drawing.Point(21, 62);
            this.radioButton_authenStyleIdpass.Name = "radioButton_authenStyleIdpass";
            this.radioButton_authenStyleIdpass.Size = new System.Drawing.Size(96, 22);
            this.radioButton_authenStyleIdpass.TabIndex = 1;
            this.radioButton_authenStyleIdpass.TabStop = true;
            this.radioButton_authenStyleIdpass.Text = "&ID/Pass";
            this.radioButton_authenStyleIdpass.UseVisualStyleBackColor = true;
            // 
            // radioButton_authenStyleOpen
            // 
            this.radioButton_authenStyleOpen.AutoSize = true;
            this.radioButton_authenStyleOpen.Location = new System.Drawing.Point(21, 32);
            this.radioButton_authenStyleOpen.Name = "radioButton_authenStyleOpen";
            this.radioButton_authenStyleOpen.Size = new System.Drawing.Size(69, 22);
            this.radioButton_authenStyleOpen.TabIndex = 0;
            this.radioButton_authenStyleOpen.TabStop = true;
            this.radioButton_authenStyleOpen.Text = "&Open";
            this.radioButton_authenStyleOpen.UseVisualStyleBackColor = true;
            // 
            // button_stop
            // 
            this.button_stop.Enabled = false;
            this.button_stop.Location = new System.Drawing.Point(227, 180);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(109, 30);
            this.button_stop.TabIndex = 11;
            this.button_stop.Text = "停止";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 606);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel_query.ResumeLayout(false);
            this.panel_query.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox textBox_database;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_serverPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_serverAddr;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_queryWord;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_use;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button_search;
        private System.Windows.Forms.Button button_close;
        private System.Windows.Forms.Panel panel_query;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button button_nextBatch;
        private System.Windows.Forms.TextBox textBox_password;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_userName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_groupID;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton_authenStyleIdpass;
        private System.Windows.Forms.RadioButton radioButton_authenStyleOpen;
        private System.Windows.Forms.Button button_stop;
    }
}

