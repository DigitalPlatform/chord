namespace dp2Capo.Install
{
    partial class SipSettingDialog
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
            this.checkBox_enableSIP = new System.Windows.Forms.CheckBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabPage_sip = new System.Windows.Forms.TabPage();
            this.comboBox_encodingName = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.comboBox_dateFormat = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabPage_dp2library = new System.Windows.Forms.TabPage();
            this.comboBox_librarywsUrl = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button_detectAnonymousUser = new System.Windows.Forms.Button();
            this.textBox_anonymousPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_anonymousUserName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox_managerAccount = new System.Windows.Forms.GroupBox();
            this.button_detectManageUser = new System.Windows.Forms.Button();
            this.textBox_managePassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_manageUserName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_ipList = new System.Windows.Forms.TextBox();
            this.tabPage_sip.SuspendLayout();
            this.tabPage_dp2library.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox_managerAccount.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBox_enableSIP
            // 
            this.checkBox_enableSIP.AutoSize = true;
            this.checkBox_enableSIP.Location = new System.Drawing.Point(15, 568);
            this.checkBox_enableSIP.Name = "checkBox_enableSIP";
            this.checkBox_enableSIP.Size = new System.Drawing.Size(250, 22);
            this.checkBox_enableSIP.TabIndex = 5;
            this.checkBox_enableSIP.Text = "启用本实例的 SIP 服务(&E)";
            this.checkBox_enableSIP.UseVisualStyleBackColor = true;
            this.checkBox_enableSIP.CheckedChanged += new System.EventHandler(this.checkBox_enableSIP_CheckedChanged);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(540, 561);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(84, 34);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(449, 561);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(84, 34);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabPage_sip
            // 
            this.tabPage_sip.Controls.Add(this.textBox_ipList);
            this.tabPage_sip.Controls.Add(this.label7);
            this.tabPage_sip.Controls.Add(this.comboBox_encodingName);
            this.tabPage_sip.Controls.Add(this.label10);
            this.tabPage_sip.Controls.Add(this.comboBox_dateFormat);
            this.tabPage_sip.Controls.Add(this.label6);
            this.tabPage_sip.Location = new System.Drawing.Point(4, 28);
            this.tabPage_sip.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_sip.Name = "tabPage_sip";
            this.tabPage_sip.Size = new System.Drawing.Size(601, 504);
            this.tabPage_sip.TabIndex = 2;
            this.tabPage_sip.Text = "SIP";
            this.tabPage_sip.UseVisualStyleBackColor = true;
            // 
            // comboBox_encodingName
            // 
            this.comboBox_encodingName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_encodingName.FormattingEnabled = true;
            this.comboBox_encodingName.Items.AddRange(new object[] {
            "UTF-8",
            "GB2312"});
            this.comboBox_encodingName.Location = new System.Drawing.Point(153, 25);
            this.comboBox_encodingName.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_encodingName.Name = "comboBox_encodingName";
            this.comboBox_encodingName.Size = new System.Drawing.Size(225, 26);
            this.comboBox_encodingName.TabIndex = 11;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(20, 28);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(125, 18);
            this.label10.TabIndex = 10;
            this.label10.Text = "编码方式(&E)：";
            // 
            // comboBox_dateFormat
            // 
            this.comboBox_dateFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_dateFormat.FormattingEnabled = true;
            this.comboBox_dateFormat.Items.AddRange(new object[] {
            "yyyyMMdd",
            "yyyy-MM-dd"});
            this.comboBox_dateFormat.Location = new System.Drawing.Point(153, 63);
            this.comboBox_dateFormat.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_dateFormat.Name = "comboBox_dateFormat";
            this.comboBox_dateFormat.Size = new System.Drawing.Size(225, 26);
            this.comboBox_dateFormat.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 66);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(125, 18);
            this.label6.TabIndex = 8;
            this.label6.Text = "日期格式(&D)：";
            // 
            // tabPage_dp2library
            // 
            this.tabPage_dp2library.AutoScroll = true;
            this.tabPage_dp2library.Controls.Add(this.comboBox_librarywsUrl);
            this.tabPage_dp2library.Controls.Add(this.label1);
            this.tabPage_dp2library.Controls.Add(this.groupBox2);
            this.tabPage_dp2library.Controls.Add(this.groupBox_managerAccount);
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
            this.groupBox2.Text = "用于匿名登录的 dp2Library 帐户 (如果设置用户名为空，表示不允许 SIP 前端匿名登录)";
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
            // groupBox_managerAccount
            // 
            this.groupBox_managerAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_managerAccount.Controls.Add(this.button_detectManageUser);
            this.groupBox_managerAccount.Controls.Add(this.textBox_managePassword);
            this.groupBox_managerAccount.Controls.Add(this.label2);
            this.groupBox_managerAccount.Controls.Add(this.textBox_manageUserName);
            this.groupBox_managerAccount.Controls.Add(this.label4);
            this.groupBox_managerAccount.Location = new System.Drawing.Point(26, 98);
            this.groupBox_managerAccount.Name = "groupBox_managerAccount";
            this.groupBox_managerAccount.Size = new System.Drawing.Size(549, 177);
            this.groupBox_managerAccount.TabIndex = 2;
            this.groupBox_managerAccount.TabStop = false;
            this.groupBox_managerAccount.Text = "SIP 管理帐户";
            this.groupBox_managerAccount.Visible = false;
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
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_dp2library);
            this.tabControl_main.Controls.Add(this.tabPage_sip);
            this.tabControl_main.Location = new System.Drawing.Point(15, 18);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(609, 536);
            this.tabControl_main.TabIndex = 4;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(20, 126);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(296, 18);
            this.label7.TabIndex = 12;
            this.label7.Text = "前端 IP 地址白名单(逗号间隔)(&B):";
            // 
            // textBox_ipList
            // 
            this.textBox_ipList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_ipList.Location = new System.Drawing.Point(23, 147);
            this.textBox_ipList.Multiline = true;
            this.textBox_ipList.Name = "textBox_ipList";
            this.textBox_ipList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_ipList.Size = new System.Drawing.Size(553, 87);
            this.textBox_ipList.TabIndex = 13;
            // 
            // SipSettingDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(639, 612);
            this.Controls.Add(this.checkBox_enableSIP);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "SipSettingDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "SIP 服务参数";
            this.Load += new System.EventHandler(this.SipSettingDialog_Load);
            this.tabPage_sip.ResumeLayout(false);
            this.tabPage_sip.PerformLayout();
            this.tabPage_dp2library.ResumeLayout(false);
            this.tabPage_dp2library.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox_managerAccount.ResumeLayout(false);
            this.groupBox_managerAccount.PerformLayout();
            this.tabControl_main.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_enableSIP;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabPage tabPage_sip;
        private System.Windows.Forms.TabPage tabPage_dp2library;
        private System.Windows.Forms.ComboBox comboBox_librarywsUrl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button_detectAnonymousUser;
        private System.Windows.Forms.TextBox textBox_anonymousPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_anonymousUserName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.GroupBox groupBox_managerAccount;
        private System.Windows.Forms.Button button_detectManageUser;
        private System.Windows.Forms.TextBox textBox_managePassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_manageUserName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_dateFormat;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBox_encodingName;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBox_ipList;
        private System.Windows.Forms.Label label7;
    }
}