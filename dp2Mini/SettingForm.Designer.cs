namespace dp2Mini
{
    partial class SettingForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_libraryUrl = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_username = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.checkBox_savePassword = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_loginAccount = new System.Windows.Forms.TabPage();
            this.tabPage_notFoundReason = new System.Windows.Forms.TabPage();
            this.textBox_reasons = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage_loginAccount.SuspendLayout();
            this.tabPage_notFoundReason.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 30);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "服务器地址：";
            // 
            // textBox_libraryUrl
            // 
            this.textBox_libraryUrl.Location = new System.Drawing.Point(148, 25);
            this.textBox_libraryUrl.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_libraryUrl.Name = "textBox_libraryUrl";
            this.textBox_libraryUrl.Size = new System.Drawing.Size(488, 28);
            this.textBox_libraryUrl.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 71);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 18);
            this.label2.TabIndex = 0;
            this.label2.Text = "用户名：";
            // 
            // textBox_username
            // 
            this.textBox_username.Location = new System.Drawing.Point(148, 66);
            this.textBox_username.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_username.Name = "textBox_username";
            this.textBox_username.Size = new System.Drawing.Size(286, 28);
            this.textBox_username.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 111);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 18);
            this.label3.TabIndex = 0;
            this.label3.Text = "密码：";
            // 
            // textBox_password
            // 
            this.textBox_password.Location = new System.Drawing.Point(148, 107);
            this.textBox_password.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(286, 28);
            this.textBox_password.TabIndex = 1;
            // 
            // button_ok
            // 
            this.button_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_ok.Location = new System.Drawing.Point(443, 385);
            this.button_ok.Margin = new System.Windows.Forms.Padding(4);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(112, 34);
            this.button_ok.TabIndex = 2;
            this.button_ok.Text = "确定(&O)";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.Location = new System.Drawing.Point(565, 385);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(4);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(112, 34);
            this.button_cancel.TabIndex = 3;
            this.button_cancel.Text = "取消(&C)";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // checkBox_savePassword
            // 
            this.checkBox_savePassword.AutoSize = true;
            this.checkBox_savePassword.Location = new System.Drawing.Point(148, 143);
            this.checkBox_savePassword.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_savePassword.Name = "checkBox_savePassword";
            this.checkBox_savePassword.Size = new System.Drawing.Size(106, 22);
            this.checkBox_savePassword.TabIndex = 7;
            this.checkBox_savePassword.Text = "记住密码";
            this.checkBox_savePassword.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_loginAccount);
            this.tabControl1.Controls.Add(this.tabPage_notFoundReason);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(669, 366);
            this.tabControl1.TabIndex = 8;
            // 
            // tabPage_loginAccount
            // 
            this.tabPage_loginAccount.Controls.Add(this.textBox_libraryUrl);
            this.tabPage_loginAccount.Controls.Add(this.checkBox_savePassword);
            this.tabPage_loginAccount.Controls.Add(this.label1);
            this.tabPage_loginAccount.Controls.Add(this.label2);
            this.tabPage_loginAccount.Controls.Add(this.textBox_username);
            this.tabPage_loginAccount.Controls.Add(this.textBox_password);
            this.tabPage_loginAccount.Controls.Add(this.label3);
            this.tabPage_loginAccount.Location = new System.Drawing.Point(4, 28);
            this.tabPage_loginAccount.Name = "tabPage_loginAccount";
            this.tabPage_loginAccount.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_loginAccount.Size = new System.Drawing.Size(661, 334);
            this.tabPage_loginAccount.TabIndex = 0;
            this.tabPage_loginAccount.Text = "登录帐号";
            this.tabPage_loginAccount.UseVisualStyleBackColor = true;
            // 
            // tabPage_notFoundReason
            // 
            this.tabPage_notFoundReason.Controls.Add(this.label4);
            this.tabPage_notFoundReason.Controls.Add(this.textBox_reasons);
            this.tabPage_notFoundReason.Location = new System.Drawing.Point(4, 28);
            this.tabPage_notFoundReason.Name = "tabPage_notFoundReason";
            this.tabPage_notFoundReason.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_notFoundReason.Size = new System.Drawing.Size(661, 334);
            this.tabPage_notFoundReason.TabIndex = 1;
            this.tabPage_notFoundReason.Text = "图书未找到原因";
            this.tabPage_notFoundReason.UseVisualStyleBackColor = true;
            // 
            // textBox_reasons
            // 
            this.textBox_reasons.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_reasons.Location = new System.Drawing.Point(19, 44);
            this.textBox_reasons.Multiline = true;
            this.textBox_reasons.Name = "textBox_reasons";
            this.textBox_reasons.Size = new System.Drawing.Size(623, 270);
            this.textBox_reasons.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(314, 18);
            this.label4.TabIndex = 1;
            this.label4.Text = "配置图书未找到原因，每个原因一行。";
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 438);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "SettingForm";
            this.Text = "参数设置";
            this.Load += new System.EventHandler(this.SettingForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_loginAccount.ResumeLayout(false);
            this.tabPage_loginAccount.PerformLayout();
            this.tabPage_notFoundReason.ResumeLayout(false);
            this.tabPage_notFoundReason.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_libraryUrl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_username;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_password;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.CheckBox checkBox_savePassword;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_loginAccount;
        private System.Windows.Forms.TabPage tabPage_notFoundReason;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_reasons;
    }
}