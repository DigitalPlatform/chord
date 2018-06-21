namespace TestZClient
{
    partial class EscapeStringDialog
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
            this.textBox_text = new System.Windows.Forms.TextBox();
            this.textBox_converted = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_convert = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "转换前:";
            // 
            // textBox_text
            // 
            this.textBox_text.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_text.Location = new System.Drawing.Point(108, 13);
            this.textBox_text.Name = "textBox_text";
            this.textBox_text.Size = new System.Drawing.Size(506, 28);
            this.textBox_text.TabIndex = 1;
            // 
            // textBox_converted
            // 
            this.textBox_converted.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_converted.Location = new System.Drawing.Point(108, 87);
            this.textBox_converted.Name = "textBox_converted";
            this.textBox_converted.ReadOnly = true;
            this.textBox_converted.Size = new System.Drawing.Size(506, 28);
            this.textBox_converted.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "转换后:";
            // 
            // button_convert
            // 
            this.button_convert.Location = new System.Drawing.Point(108, 48);
            this.button_convert.Name = "button_convert";
            this.button_convert.Size = new System.Drawing.Size(117, 33);
            this.button_convert.TabIndex = 4;
            this.button_convert.Text = "转换";
            this.button_convert.UseVisualStyleBackColor = true;
            this.button_convert.Click += new System.EventHandler(this.button_convert_Click);
            // 
            // EscapeStringDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 180);
            this.Controls.Add(this.button_convert);
            this.Controls.Add(this.textBox_converted);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_text);
            this.Controls.Add(this.label1);
            this.Name = "EscapeStringDialog";
            this.Text = "转义检索词";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_text;
        private System.Windows.Forms.TextBox textBox_converted;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_convert;
    }
}