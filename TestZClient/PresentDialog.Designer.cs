namespace TestZClient
{
    partial class PresentDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_resultSetName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_start = new System.Windows.Forms.TextBox();
            this.textBox_count = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.AutoSize = true;
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(377, 299);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(84, 34);
            this.button_Cancel.TabIndex = 17;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.AutoSize = true;
            this.button_OK.Location = new System.Drawing.Point(285, 299);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(84, 34);
            this.button_OK.TabIndex = 16;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 18);
            this.label1.TabIndex = 18;
            this.label1.Text = "结果集名(&R):";
            // 
            // textBox_resultSetName
            // 
            this.textBox_resultSetName.Location = new System.Drawing.Point(134, 13);
            this.textBox_resultSetName.Name = "textBox_resultSetName";
            this.textBox_resultSetName.Size = new System.Drawing.Size(217, 28);
            this.textBox_resultSetName.TabIndex = 19;
            this.textBox_resultSetName.Text = "default";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(116, 18);
            this.label2.TabIndex = 20;
            this.label2.Text = "起始位置(&S):";
            // 
            // textBox_start
            // 
            this.textBox_start.Location = new System.Drawing.Point(134, 52);
            this.textBox_start.Name = "textBox_start";
            this.textBox_start.Size = new System.Drawing.Size(111, 28);
            this.textBox_start.TabIndex = 21;
            // 
            // textBox_count
            // 
            this.textBox_count.Location = new System.Drawing.Point(134, 86);
            this.textBox_count.Name = "textBox_count";
            this.textBox_count.Size = new System.Drawing.Size(111, 28);
            this.textBox_count.TabIndex = 23;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 18);
            this.label3.TabIndex = 22;
            this.label3.Text = "个数(&C):";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(253, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(143, 18);
            this.label4.TabIndex = 24;
            this.label4.Text = "(从 0 开始计数)";
            // 
            // PresentDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(473, 345);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_count);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_start);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_resultSetName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "PresentDialog";
            this.Text = "PresentDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_resultSetName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_start;
        private System.Windows.Forms.TextBox textBox_count;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}