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
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.tabPage_notFoundReason = new System.Windows.Forms.TabPage();
            this.textBox_reasons = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_notFoundReason.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
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
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_notFoundReason);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(669, 366);
            this.tabControl1.TabIndex = 8;
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
            this.tabPage_notFoundReason.ResumeLayout(false);
            this.tabPage_notFoundReason.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.TabPage tabPage_notFoundReason;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_reasons;
        private System.Windows.Forms.TabControl tabControl1;
    }
}