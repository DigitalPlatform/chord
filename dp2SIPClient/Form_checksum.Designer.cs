namespace dp2SIPClient
{
    partial class Form_Checksum
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
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.btnCheckSum2 = new System.Windows.Forms.Button();
            this.btnCheckSum1 = new System.Windows.Forms.Button();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtMsg
            // 
            this.txtMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMsg.Location = new System.Drawing.Point(15, 27);
            this.txtMsg.Margin = new System.Windows.Forms.Padding(6);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.Size = new System.Drawing.Size(530, 104);
            this.txtMsg.TabIndex = 19;
            // 
            // btnCheckSum2
            // 
            this.btnCheckSum2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckSum2.Location = new System.Drawing.Point(558, 85);
            this.btnCheckSum2.Margin = new System.Windows.Forms.Padding(6);
            this.btnCheckSum2.Name = "btnCheckSum2";
            this.btnCheckSum2.Size = new System.Drawing.Size(72, 46);
            this.btnCheckSum2.TabIndex = 25;
            this.btnCheckSum2.Text = "校2";
            this.btnCheckSum2.UseVisualStyleBackColor = true;
            this.btnCheckSum2.Click += new System.EventHandler(this.btnCheckSum2_Click);
            // 
            // btnCheckSum1
            // 
            this.btnCheckSum1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckSum1.Location = new System.Drawing.Point(557, 27);
            this.btnCheckSum1.Margin = new System.Windows.Forms.Padding(6);
            this.btnCheckSum1.Name = "btnCheckSum1";
            this.btnCheckSum1.Size = new System.Drawing.Size(73, 46);
            this.btnCheckSum1.TabIndex = 24;
            this.btnCheckSum1.Text = "校1";
            this.btnCheckSum1.UseVisualStyleBackColor = true;
            this.btnCheckSum1.Click += new System.EventHandler(this.btnCheckSum1_Click);
            // 
            // txtInfo
            // 
            this.txtInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInfo.BackColor = System.Drawing.SystemColors.Info;
            this.txtInfo.Location = new System.Drawing.Point(15, 157);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ReadOnly = true;
            this.txtInfo.Size = new System.Drawing.Size(618, 363);
            this.txtInfo.TabIndex = 26;
            // 
            // Form_checksum
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(653, 532);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.btnCheckSum2);
            this.Controls.Add(this.btnCheckSum1);
            this.Controls.Add(this.txtMsg);
            this.Name = "Form_checksum";
            this.Text = "Form_checksum";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtMsg;
        private System.Windows.Forms.Button btnCheckSum2;
        private System.Windows.Forms.Button btnCheckSum1;
        private System.Windows.Forms.TextBox txtInfo;
    }
}