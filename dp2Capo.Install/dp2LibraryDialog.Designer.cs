﻿namespace dp2Capo.Install
{
    partial class dp2LibraryDialog
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_resetManageUserPassword = new System.Windows.Forms.Button();
            this.button_createManageUser = new System.Windows.Forms.Button();
            this.button_detectManageUser = new System.Windows.Forms.Button();
            this.textBox_confirmManagePassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_managePassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_manageUserName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.button_getQueuePath = new System.Windows.Forms.Button();
            this.comboBox_msmqPath = new System.Windows.Forms.ComboBox();
            this.comboBox_url = new System.Windows.Forms.ComboBox();
            this.button_workerRights = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_general = new System.Windows.Forms.TabPage();
            this.tabPage_msmq = new System.Windows.Forms.TabPage();
            this.tabPage_weixin = new System.Windows.Forms.TabPage();
            this.tabPage_router = new System.Windows.Forms.TabPage();
            this.button_createDpAccount = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_webURL = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage_general.SuspendLayout();
            this.tabPage_msmq.SuspendLayout();
            this.tabPage_weixin.SuspendLayout();
            this.tabPage_router.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.AutoSize = true;
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(514, 495);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(84, 34);
            this.button_Cancel.TabIndex = 8;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.AutoSize = true;
            this.button_OK.Location = new System.Drawing.Point(423, 495);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(84, 34);
            this.button_OK.TabIndex = 7;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.button_resetManageUserPassword);
            this.groupBox1.Controls.Add(this.button_createManageUser);
            this.groupBox1.Controls.Add(this.button_detectManageUser);
            this.groupBox1.Controls.Add(this.textBox_confirmManagePassword);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBox_managePassword);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBox_manageUserName);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(10, 98);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(549, 255);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 代理帐户(针对上述dp2Library) ";
            // 
            // button_resetManageUserPassword
            // 
            this.button_resetManageUserPassword.AutoSize = true;
            this.button_resetManageUserPassword.Enabled = false;
            this.button_resetManageUserPassword.Location = new System.Drawing.Point(280, 190);
            this.button_resetManageUserPassword.Name = "button_resetManageUserPassword";
            this.button_resetManageUserPassword.Size = new System.Drawing.Size(176, 42);
            this.button_resetManageUserPassword.TabIndex = 8;
            this.button_resetManageUserPassword.Text = "重设密码(&R)";
            this.button_resetManageUserPassword.UseVisualStyleBackColor = true;
            this.button_resetManageUserPassword.Click += new System.EventHandler(this.button_resetManageUserPassword_Click);
            // 
            // button_createManageUser
            // 
            this.button_createManageUser.AutoSize = true;
            this.button_createManageUser.Enabled = false;
            this.button_createManageUser.Location = new System.Drawing.Point(152, 190);
            this.button_createManageUser.Name = "button_createManageUser";
            this.button_createManageUser.Size = new System.Drawing.Size(122, 42);
            this.button_createManageUser.TabIndex = 7;
            this.button_createManageUser.Text = "创建(&C)";
            this.button_createManageUser.UseVisualStyleBackColor = true;
            this.button_createManageUser.Click += new System.EventHandler(this.button_createManageUser_Click);
            // 
            // button_detectManageUser
            // 
            this.button_detectManageUser.AutoSize = true;
            this.button_detectManageUser.Enabled = false;
            this.button_detectManageUser.Location = new System.Drawing.Point(24, 190);
            this.button_detectManageUser.Name = "button_detectManageUser";
            this.button_detectManageUser.Size = new System.Drawing.Size(122, 42);
            this.button_detectManageUser.TabIndex = 6;
            this.button_detectManageUser.Text = "检测(&D)";
            this.button_detectManageUser.UseVisualStyleBackColor = true;
            this.button_detectManageUser.Click += new System.EventHandler(this.button_detectManageUser_Click);
            // 
            // textBox_confirmManagePassword
            // 
            this.textBox_confirmManagePassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_confirmManagePassword.Location = new System.Drawing.Point(168, 141);
            this.textBox_confirmManagePassword.Name = "textBox_confirmManagePassword";
            this.textBox_confirmManagePassword.PasswordChar = '*';
            this.textBox_confirmManagePassword.Size = new System.Drawing.Size(280, 28);
            this.textBox_confirmManagePassword.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 146);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(125, 18);
            this.label3.TabIndex = 4;
            this.label3.Text = "再次输入密码:";
            // 
            // textBox_managePassword
            // 
            this.textBox_managePassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_managePassword.Location = new System.Drawing.Point(168, 105);
            this.textBox_managePassword.Name = "textBox_managePassword";
            this.textBox_managePassword.PasswordChar = '*';
            this.textBox_managePassword.Size = new System.Drawing.Size(280, 28);
            this.textBox_managePassword.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "密码(&P):";
            // 
            // textBox_manageUserName
            // 
            this.textBox_manageUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_manageUserName.Location = new System.Drawing.Point(168, 50);
            this.textBox_manageUserName.Name = "textBox_manageUserName";
            this.textBox_manageUserName.Size = new System.Drawing.Size(280, 28);
            this.textBox_manageUserName.TabIndex = 1;
            this.textBox_manageUserName.TextChanged += new System.EventHandler(this.textBox_manageUserName_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 52);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 18);
            this.label4.TabIndex = 0;
            this.label4.Text = "用户名(&U):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(242, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "dp2Library 服务器 URL (&U):";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 20);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(152, 18);
            this.label5.TabIndex = 3;
            this.label5.Text = "消息队列路径(&M):";
            // 
            // button_getQueuePath
            // 
            this.button_getQueuePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getQueuePath.AutoSize = true;
            this.button_getQueuePath.Location = new System.Drawing.Point(476, 39);
            this.button_getQueuePath.Name = "button_getQueuePath";
            this.button_getQueuePath.Size = new System.Drawing.Size(86, 42);
            this.button_getQueuePath.TabIndex = 5;
            this.button_getQueuePath.Text = "获得";
            this.button_getQueuePath.UseVisualStyleBackColor = true;
            this.button_getQueuePath.Click += new System.EventHandler(this.button_getQueuePath_Click);
            // 
            // comboBox_msmqPath
            // 
            this.comboBox_msmqPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_msmqPath.FormattingEnabled = true;
            this.comboBox_msmqPath.Items.AddRange(new object[] {
            "!api"});
            this.comboBox_msmqPath.Location = new System.Drawing.Point(12, 42);
            this.comboBox_msmqPath.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBox_msmqPath.Name = "comboBox_msmqPath";
            this.comboBox_msmqPath.Size = new System.Drawing.Size(454, 26);
            this.comboBox_msmqPath.TabIndex = 4;
            // 
            // comboBox_url
            // 
            this.comboBox_url.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_url.FormattingEnabled = true;
            this.comboBox_url.Items.AddRange(new object[] {
            "http://localhost:8001/dp2Library",
            "net.pipe://localhost/dp2library/xe"});
            this.comboBox_url.Location = new System.Drawing.Point(10, 44);
            this.comboBox_url.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBox_url.Name = "comboBox_url";
            this.comboBox_url.Size = new System.Drawing.Size(547, 26);
            this.comboBox_url.TabIndex = 1;
            // 
            // button_workerRights
            // 
            this.button_workerRights.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_workerRights.Location = new System.Drawing.Point(4, 24);
            this.button_workerRights.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_workerRights.Name = "button_workerRights";
            this.button_workerRights.Size = new System.Drawing.Size(560, 34);
            this.button_workerRights.TabIndex = 6;
            this.button_workerRights.Text = "为工作人员添加管理公众号的权限";
            this.button_workerRights.UseVisualStyleBackColor = true;
            this.button_workerRights.Click += new System.EventHandler(this.button_workerRights_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(413, 18);
            this.label6.TabIndex = 9;
            this.label6.Text = "用于 dp2Router 的，dp2Library 服务器 URL (&U):";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_general);
            this.tabControl1.Controls.Add(this.tabPage_msmq);
            this.tabControl1.Controls.Add(this.tabPage_weixin);
            this.tabControl1.Controls.Add(this.tabPage_router);
            this.tabControl1.Location = new System.Drawing.Point(18, 18);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(580, 470);
            this.tabControl1.TabIndex = 11;
            // 
            // tabPage_general
            // 
            this.tabPage_general.Controls.Add(this.label1);
            this.tabPage_general.Controls.Add(this.groupBox1);
            this.tabPage_general.Controls.Add(this.comboBox_url);
            this.tabPage_general.Location = new System.Drawing.Point(4, 28);
            this.tabPage_general.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_general.Name = "tabPage_general";
            this.tabPage_general.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_general.Size = new System.Drawing.Size(572, 438);
            this.tabPage_general.TabIndex = 0;
            this.tabPage_general.Text = "capo 代理账户";
            this.tabPage_general.UseVisualStyleBackColor = true;
            // 
            // tabPage_msmq
            // 
            this.tabPage_msmq.Controls.Add(this.label5);
            this.tabPage_msmq.Controls.Add(this.comboBox_msmqPath);
            this.tabPage_msmq.Controls.Add(this.button_getQueuePath);
            this.tabPage_msmq.Location = new System.Drawing.Point(4, 28);
            this.tabPage_msmq.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_msmq.Name = "tabPage_msmq";
            this.tabPage_msmq.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_msmq.Size = new System.Drawing.Size(572, 438);
            this.tabPage_msmq.TabIndex = 1;
            this.tabPage_msmq.Text = "消息队列";
            this.tabPage_msmq.UseVisualStyleBackColor = true;
            // 
            // tabPage_weixin
            // 
            this.tabPage_weixin.Controls.Add(this.button_workerRights);
            this.tabPage_weixin.Location = new System.Drawing.Point(4, 28);
            this.tabPage_weixin.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_weixin.Name = "tabPage_weixin";
            this.tabPage_weixin.Size = new System.Drawing.Size(572, 438);
            this.tabPage_weixin.TabIndex = 2;
            this.tabPage_weixin.Text = "微信";
            this.tabPage_weixin.UseVisualStyleBackColor = true;
            // 
            // tabPage_router
            // 
            this.tabPage_router.Controls.Add(this.button_createDpAccount);
            this.tabPage_router.Controls.Add(this.label7);
            this.tabPage_router.Controls.Add(this.textBox_webURL);
            this.tabPage_router.Controls.Add(this.label6);
            this.tabPage_router.Location = new System.Drawing.Point(4, 28);
            this.tabPage_router.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_router.Name = "tabPage_router";
            this.tabPage_router.Size = new System.Drawing.Size(572, 438);
            this.tabPage_router.TabIndex = 3;
            this.tabPage_router.Text = "dp2Router";
            this.tabPage_router.UseVisualStyleBackColor = true;
            // 
            // button_createDpAccount
            // 
            this.button_createDpAccount.Location = new System.Drawing.Point(6, 339);
            this.button_createDpAccount.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_createDpAccount.Name = "button_createDpAccount";
            this.button_createDpAccount.Size = new System.Drawing.Size(410, 34);
            this.button_createDpAccount.TabIndex = 13;
            this.button_createDpAccount.Text = "在 dp2library 上创建 dp 账户 ...";
            this.button_createDpAccount.UseVisualStyleBackColor = true;
            this.button_createDpAccount.Click += new System.EventHandler(this.button_createDpAccount_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 302);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(170, 18);
            this.label7.TabIndex = 12;
            this.label7.Text = "注: 每行一个 URL。";
            // 
            // textBox_webURL
            // 
            this.textBox_webURL.AcceptsReturn = true;
            this.textBox_webURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_webURL.Location = new System.Drawing.Point(6, 48);
            this.textBox_webURL.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_webURL.Multiline = true;
            this.textBox_webURL.Name = "textBox_webURL";
            this.textBox_webURL.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_webURL.Size = new System.Drawing.Size(556, 247);
            this.textBox_webURL.TabIndex = 11;
            // 
            // dp2LibraryDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(612, 544);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "dp2LibraryDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "设置针对 dp2Library 的参数";
            this.Load += new System.EventHandler(this.dp2LibraryDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage_general.ResumeLayout(false);
            this.tabPage_general.PerformLayout();
            this.tabPage_msmq.ResumeLayout(false);
            this.tabPage_msmq.PerformLayout();
            this.tabPage_weixin.ResumeLayout(false);
            this.tabPage_router.ResumeLayout(false);
            this.tabPage_router.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_resetManageUserPassword;
        private System.Windows.Forms.Button button_createManageUser;
        private System.Windows.Forms.Button button_detectManageUser;
        private System.Windows.Forms.TextBox textBox_confirmManagePassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_managePassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_manageUserName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button_getQueuePath;
        private System.Windows.Forms.ComboBox comboBox_msmqPath;
        private System.Windows.Forms.ComboBox comboBox_url;
        private System.Windows.Forms.Button button_workerRights;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_general;
        private System.Windows.Forms.TabPage tabPage_msmq;
        private System.Windows.Forms.TabPage tabPage_weixin;
        private System.Windows.Forms.TabPage tabPage_router;
        private System.Windows.Forms.TextBox textBox_webURL;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button_createDpAccount;
    }
}