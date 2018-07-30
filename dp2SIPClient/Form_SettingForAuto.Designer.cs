namespace dp2SIPClient
{
    partial class Form_SettingForAuto
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
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.button_verify = new System.Windows.Forms.Button();
            this.textBox_dp2password = new System.Windows.Forms.TextBox();
            this.textBox_dp2username = new System.Windows.Forms.TextBox();
            this.textBox_dp2serverUrl = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_cancel = new System.Windows.Forms.Button();
            this.button_ok = new System.Windows.Forms.Button();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.button_verify);
            this.groupBox4.Controls.Add(this.textBox_dp2password);
            this.groupBox4.Controls.Add(this.textBox_dp2username);
            this.groupBox4.Controls.Add(this.textBox_dp2serverUrl);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Location = new System.Drawing.Point(12, 15);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(6);
            this.groupBox4.Size = new System.Drawing.Size(673, 279);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "图书馆应用服务器配置";
            // 
            // button_verify
            // 
            this.button_verify.Location = new System.Drawing.Point(178, 205);
            this.button_verify.Name = "button_verify";
            this.button_verify.Size = new System.Drawing.Size(135, 46);
            this.button_verify.TabIndex = 6;
            this.button_verify.Text = "检测";
            this.button_verify.UseVisualStyleBackColor = true;
            this.button_verify.Click += new System.EventHandler(this.button_verify_Click);
            // 
            // textBox_dp2password
            // 
            this.textBox_dp2password.Location = new System.Drawing.Point(178, 150);
            this.textBox_dp2password.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_dp2password.Name = "textBox_dp2password";
            this.textBox_dp2password.PasswordChar = '*';
            this.textBox_dp2password.Size = new System.Drawing.Size(298, 35);
            this.textBox_dp2password.TabIndex = 5;
            // 
            // textBox_dp2username
            // 
            this.textBox_dp2username.Location = new System.Drawing.Point(178, 96);
            this.textBox_dp2username.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_dp2username.Name = "textBox_dp2username";
            this.textBox_dp2username.Size = new System.Drawing.Size(298, 35);
            this.textBox_dp2username.TabIndex = 4;
            // 
            // textBox_dp2serverUrl
            // 
            this.textBox_dp2serverUrl.Location = new System.Drawing.Point(178, 42);
            this.textBox_dp2serverUrl.Margin = new System.Windows.Forms.Padding(6);
            this.textBox_dp2serverUrl.Name = "textBox_dp2serverUrl";
            this.textBox_dp2serverUrl.Size = new System.Drawing.Size(456, 35);
            this.textBox_dp2serverUrl.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 156);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 24);
            this.label3.TabIndex = 2;
            this.label3.Text = "密  码：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 102);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 24);
            this.label2.TabIndex = 1;
            this.label2.Text = "用户名：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 48);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(154, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "服务器地址：";
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.Location = new System.Drawing.Point(534, 342);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(6);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(166, 54);
            this.button_cancel.TabIndex = 24;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // button_ok
            // 
            this.button_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_ok.Location = new System.Drawing.Point(356, 342);
            this.button_ok.Margin = new System.Windows.Forms.Padding(6);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(166, 54);
            this.button_ok.TabIndex = 23;
            this.button_ok.Text = "确定";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // Form_SettingForAuto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 411);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.groupBox4);
            this.Name = "Form_SettingForAuto";
            this.Text = "dp2服务器参数配置";
            this.Load += new System.EventHandler(this.Form_CreateTestEnv_Load);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox textBox_dp2password;
        private System.Windows.Forms.TextBox textBox_dp2username;
        private System.Windows.Forms.TextBox textBox_dp2serverUrl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_verify;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Button button_ok;
    }
}