﻿namespace dp2Capo.Install
{
    partial class InstallZServerDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallZServerDlg));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_detectManageUser = new System.Windows.Forms.Button();
            this.textBox_managePassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_manageUserName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button_detectAnonymousUser = new System.Windows.Forms.Button();
            this.textBox_anonymousPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_anonymousUserName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_dp2library = new System.Windows.Forms.TabPage();
            this.comboBox_librarywsUrl = new System.Windows.Forms.ComboBox();
            this.tabPage_database = new System.Windows.Forms.TabPage();
            this.textBox_databaseDef = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button_import_databaseDef = new System.Windows.Forms.Button();
            this.tabPage_z3950 = new System.Windows.Forms.TabPage();
            this.textBox_z3950_maxResultCount = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox_z3950_maxSessions = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.numericUpDown_z3950_port = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.checkBox_enableZ3950 = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_dp2library.SuspendLayout();
            this.tabPage_database.SuspendLayout();
            this.tabPage_z3950.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_z3950_port)).BeginInit();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(543, 561);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(84, 34);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(452, 561);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(84, 34);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.button_detectManageUser);
            this.groupBox1.Controls.Add(this.textBox_managePassword);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBox_manageUserName);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(26, 98);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(549, 177);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Z39.50 服务器管理帐户";
            // 
            // button_detectManageUser
            // 
            this.button_detectManageUser.Location = new System.Drawing.Point(168, 120);
            this.button_detectManageUser.Name = "button_detectManageUser";
            this.button_detectManageUser.Size = new System.Drawing.Size(122, 34);
            this.button_detectManageUser.TabIndex = 4;
            this.button_detectManageUser.Text = "检测(&D)";
            this.button_detectManageUser.UseVisualStyleBackColor = true;
            this.button_detectManageUser.Click += new System.EventHandler(this.button_detectManageUser_Click);
            // 
            // textBox_managePassword
            // 
            this.textBox_managePassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_managePassword.Enabled = false;
            this.textBox_managePassword.Location = new System.Drawing.Point(168, 82);
            this.textBox_managePassword.Name = "textBox_managePassword";
            this.textBox_managePassword.PasswordChar = '*';
            this.textBox_managePassword.Size = new System.Drawing.Size(278, 28);
            this.textBox_managePassword.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "密码(&P):";
            // 
            // textBox_manageUserName
            // 
            this.textBox_manageUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_manageUserName.Enabled = false;
            this.textBox_manageUserName.Location = new System.Drawing.Point(168, 45);
            this.textBox_manageUserName.Name = "textBox_manageUserName";
            this.textBox_manageUserName.Size = new System.Drawing.Size(278, 28);
            this.textBox_manageUserName.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 18);
            this.label4.TabIndex = 0;
            this.label4.Text = "用户名(&U):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Enabled = false;
            this.label1.Location = new System.Drawing.Point(21, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(314, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "所访问的 dp2Library 服务器 URL(&U):";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.button_detectAnonymousUser);
            this.groupBox2.Controls.Add(this.textBox_anonymousPassword);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.textBox_anonymousUserName);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(26, 294);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(549, 177);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "用于匿名登录的 dp2Library 帐户 (如果设置用户名为空，表示不允许Z39.50前端匿名登录)";
            // 
            // button_detectAnonymousUser
            // 
            this.button_detectAnonymousUser.Location = new System.Drawing.Point(168, 120);
            this.button_detectAnonymousUser.Name = "button_detectAnonymousUser";
            this.button_detectAnonymousUser.Size = new System.Drawing.Size(122, 34);
            this.button_detectAnonymousUser.TabIndex = 4;
            this.button_detectAnonymousUser.Text = "检测(&T)";
            this.button_detectAnonymousUser.UseVisualStyleBackColor = true;
            this.button_detectAnonymousUser.Click += new System.EventHandler(this.button_detectAnonymousUser_Click);
            // 
            // textBox_anonymousPassword
            // 
            this.textBox_anonymousPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_anonymousPassword.Location = new System.Drawing.Point(168, 82);
            this.textBox_anonymousPassword.Name = "textBox_anonymousPassword";
            this.textBox_anonymousPassword.PasswordChar = '*';
            this.textBox_anonymousPassword.Size = new System.Drawing.Size(278, 28);
            this.textBox_anonymousPassword.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 18);
            this.label3.TabIndex = 2;
            this.label3.Text = "密码(&P):";
            // 
            // textBox_anonymousUserName
            // 
            this.textBox_anonymousUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_anonymousUserName.Location = new System.Drawing.Point(168, 45);
            this.textBox_anonymousUserName.Name = "textBox_anonymousUserName";
            this.textBox_anonymousUserName.Size = new System.Drawing.Size(278, 28);
            this.textBox_anonymousUserName.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(21, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(98, 18);
            this.label5.TabIndex = 0;
            this.label5.Text = "用户名(&U):";
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_dp2library);
            this.tabControl_main.Controls.Add(this.tabPage_database);
            this.tabControl_main.Controls.Add(this.tabPage_z3950);
            this.tabControl_main.Location = new System.Drawing.Point(18, 18);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(609, 536);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_dp2library
            // 
            this.tabPage_dp2library.AutoScroll = true;
            this.tabPage_dp2library.Controls.Add(this.comboBox_librarywsUrl);
            this.tabPage_dp2library.Controls.Add(this.label1);
            this.tabPage_dp2library.Controls.Add(this.groupBox2);
            this.tabPage_dp2library.Controls.Add(this.groupBox1);
            this.tabPage_dp2library.Location = new System.Drawing.Point(4, 28);
            this.tabPage_dp2library.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_dp2library.Name = "tabPage_dp2library";
            this.tabPage_dp2library.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_dp2library.Size = new System.Drawing.Size(601, 504);
            this.tabPage_dp2library.TabIndex = 0;
            this.tabPage_dp2library.Text = "dp2Library 服务器";
            this.tabPage_dp2library.UseVisualStyleBackColor = true;
            // 
            // comboBox_librarywsUrl
            // 
            this.comboBox_librarywsUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_librarywsUrl.Enabled = false;
            this.comboBox_librarywsUrl.FormattingEnabled = true;
            this.comboBox_librarywsUrl.Items.AddRange(new object[] {
            "net.pipe://localhost/dp2library",
            "http://localhost:8001/dp2library",
            "net.pipe://locahost/dp2library/xe",
            "http://localhost:8001/dp2library/xe"});
            this.comboBox_librarywsUrl.Location = new System.Drawing.Point(24, 45);
            this.comboBox_librarywsUrl.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_librarywsUrl.Name = "comboBox_librarywsUrl";
            this.comboBox_librarywsUrl.Size = new System.Drawing.Size(548, 26);
            this.comboBox_librarywsUrl.TabIndex = 1;
            this.comboBox_librarywsUrl.Text = "http://localhost:8001/dp2library";
            // 
            // tabPage_database
            // 
            this.tabPage_database.Controls.Add(this.textBox_databaseDef);
            this.tabPage_database.Controls.Add(this.label6);
            this.tabPage_database.Controls.Add(this.button_import_databaseDef);
            this.tabPage_database.Location = new System.Drawing.Point(4, 28);
            this.tabPage_database.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_database.Name = "tabPage_database";
            this.tabPage_database.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_database.Size = new System.Drawing.Size(601, 504);
            this.tabPage_database.TabIndex = 1;
            this.tabPage_database.Text = "数据库";
            this.tabPage_database.UseVisualStyleBackColor = true;
            // 
            // textBox_databaseDef
            // 
            this.textBox_databaseDef.AcceptsReturn = true;
            this.textBox_databaseDef.AcceptsTab = true;
            this.textBox_databaseDef.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_databaseDef.Location = new System.Drawing.Point(14, 80);
            this.textBox_databaseDef.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_databaseDef.Multiline = true;
            this.textBox_databaseDef.Name = "textBox_databaseDef";
            this.textBox_databaseDef.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_databaseDef.Size = new System.Drawing.Size(572, 406);
            this.textBox_databaseDef.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 56);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(107, 18);
            this.label6.TabIndex = 1;
            this.label6.Text = "数据库定义:";
            // 
            // button_import_databaseDef
            // 
            this.button_import_databaseDef.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_import_databaseDef.Location = new System.Drawing.Point(10, 10);
            this.button_import_databaseDef.Margin = new System.Windows.Forms.Padding(4);
            this.button_import_databaseDef.Name = "button_import_databaseDef";
            this.button_import_databaseDef.Size = new System.Drawing.Size(578, 34);
            this.button_import_databaseDef.TabIndex = 0;
            this.button_import_databaseDef.Text = "从 dp2library 服务器导入数据库定义";
            this.button_import_databaseDef.UseVisualStyleBackColor = true;
            this.button_import_databaseDef.Click += new System.EventHandler(this.button_import_databaseDef_Click);
            // 
            // tabPage_z3950
            // 
            this.tabPage_z3950.Controls.Add(this.textBox_z3950_maxResultCount);
            this.tabPage_z3950.Controls.Add(this.label9);
            this.tabPage_z3950.Controls.Add(this.textBox_z3950_maxSessions);
            this.tabPage_z3950.Controls.Add(this.label8);
            this.tabPage_z3950.Controls.Add(this.numericUpDown_z3950_port);
            this.tabPage_z3950.Controls.Add(this.label7);
            this.tabPage_z3950.Location = new System.Drawing.Point(4, 28);
            this.tabPage_z3950.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_z3950.Name = "tabPage_z3950";
            this.tabPage_z3950.Size = new System.Drawing.Size(601, 504);
            this.tabPage_z3950.TabIndex = 2;
            this.tabPage_z3950.Text = "Z39.50";
            this.tabPage_z3950.UseVisualStyleBackColor = true;
            // 
            // textBox_z3950_maxResultCount
            // 
            this.textBox_z3950_maxResultCount.Location = new System.Drawing.Point(198, 108);
            this.textBox_z3950_maxResultCount.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_z3950_maxResultCount.Name = "textBox_z3950_maxResultCount";
            this.textBox_z3950_maxResultCount.Size = new System.Drawing.Size(193, 28);
            this.textBox_z3950_maxResultCount.TabIndex = 5;
            this.textBox_z3950_maxResultCount.Text = "-1";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(20, 112);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(170, 18);
            this.label9.TabIndex = 4;
            this.label9.Text = "最大检索命中数(&S):";
            // 
            // textBox_z3950_maxSessions
            // 
            this.textBox_z3950_maxSessions.Location = new System.Drawing.Point(198, 68);
            this.textBox_z3950_maxSessions.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_z3950_maxSessions.Name = "textBox_z3950_maxSessions";
            this.textBox_z3950_maxSessions.Size = new System.Drawing.Size(193, 28);
            this.textBox_z3950_maxSessions.TabIndex = 3;
            this.textBox_z3950_maxSessions.Text = "-1";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(20, 72);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(134, 18);
            this.label8.TabIndex = 2;
            this.label8.Text = "最大会话数(&S):";
            // 
            // numericUpDown_z3950_port
            // 
            this.numericUpDown_z3950_port.Location = new System.Drawing.Point(198, 27);
            this.numericUpDown_z3950_port.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDown_z3950_port.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numericUpDown_z3950_port.Name = "numericUpDown_z3950_port";
            this.numericUpDown_z3950_port.Size = new System.Drawing.Size(195, 28);
            this.numericUpDown_z3950_port.TabIndex = 1;
            this.numericUpDown_z3950_port.Value = new decimal(new int[] {
            210,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(20, 30);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(116, 18);
            this.label7.TabIndex = 0;
            this.label7.Text = "监听端口(&P):";
            // 
            // checkBox_enableZ3950
            // 
            this.checkBox_enableZ3950.AutoSize = true;
            this.checkBox_enableZ3950.Location = new System.Drawing.Point(18, 568);
            this.checkBox_enableZ3950.Name = "checkBox_enableZ3950";
            this.checkBox_enableZ3950.Size = new System.Drawing.Size(277, 22);
            this.checkBox_enableZ3950.TabIndex = 1;
            this.checkBox_enableZ3950.Text = "启用本实例的 Z39.50 服务(&E)";
            this.checkBox_enableZ3950.UseVisualStyleBackColor = true;
            this.checkBox_enableZ3950.CheckedChanged += new System.EventHandler(this.checkBox_enableZ3950_CheckedChanged);
            // 
            // InstallZServerDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 612);
            this.Controls.Add(this.checkBox_enableZ3950);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "InstallZServerDlg";
            this.Text = "请指定 Z39.50 服务安装参数";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.InstallZServerDlg_FormClosed);
            this.Load += new System.EventHandler(this.InstallZServerDlg_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_dp2library.ResumeLayout(false);
            this.tabPage_dp2library.PerformLayout();
            this.tabPage_database.ResumeLayout(false);
            this.tabPage_database.PerformLayout();
            this.tabPage_z3950.ResumeLayout(false);
            this.tabPage_z3950.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_z3950_port)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_detectManageUser;
        private System.Windows.Forms.TextBox textBox_managePassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_manageUserName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button_detectAnonymousUser;
        private System.Windows.Forms.TextBox textBox_anonymousPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_anonymousUserName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_dp2library;
        private System.Windows.Forms.TabPage tabPage_database;
        private System.Windows.Forms.Button button_import_databaseDef;
        private System.Windows.Forms.TextBox textBox_databaseDef;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBox_librarywsUrl;
        private System.Windows.Forms.TabPage tabPage_z3950;
        private System.Windows.Forms.NumericUpDown numericUpDown_z3950_port;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox_z3950_maxSessions;
        private System.Windows.Forms.TextBox textBox_z3950_maxResultCount;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox checkBox_enableZ3950;
    }
}