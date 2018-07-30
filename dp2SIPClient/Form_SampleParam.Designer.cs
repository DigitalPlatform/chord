namespace dp2SIPClient
{
    partial class Form_SampleParam
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
        //private void InitializeComponent()
        //{
        //    this.components = new System.ComponentModel.Container();
        //    this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        //    this.Text = "Form1";
        //}

        private void InitializeComponent()
        {
            this.button_cancel = new System.Windows.Forms.Button();
            this.button_ok = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtItem = new System.Windows.Forms.TextBox();
            this.txtPatron = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.Location = new System.Drawing.Point(287, 245);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(4);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(111, 34);
            this.button_cancel.TabIndex = 28;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // button_ok
            // 
            this.button_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_ok.Location = new System.Drawing.Point(168, 245);
            this.button_ok.Margin = new System.Windows.Forms.Padding(4);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(111, 34);
            this.button_ok.TabIndex = 27;
            this.button_ok.Text = "确定";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 96);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 15);
            this.label2.TabIndex = 26;
            this.label2.Text = "图书馆册条码";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 62);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 15);
            this.label1.TabIndex = 25;
            this.label1.Text = "读者证条码";
            // 
            // txtItem
            // 
            this.txtItem.Location = new System.Drawing.Point(141, 93);
            this.txtItem.Margin = new System.Windows.Forms.Padding(4);
            this.txtItem.Name = "txtItem";
            this.txtItem.Size = new System.Drawing.Size(257, 25);
            this.txtItem.TabIndex = 24;
            this.txtItem.Text = "DPB000001";
            // 
            // txtPatron
            // 
            this.txtPatron.Location = new System.Drawing.Point(141, 56);
            this.txtPatron.Margin = new System.Windows.Forms.Padding(4);
            this.txtPatron.Name = "txtPatron";
            this.txtPatron.Size = new System.Drawing.Size(257, 25);
            this.txtPatron.TabIndex = 23;
            this.txtPatron.Text = "XZXP00001";
            // 
            // Form_SampleParam
            // 
            this.ClientSize = new System.Drawing.Size(429, 334);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtItem);
            this.Controls.Add(this.txtPatron);
            this.Name = "Form_SampleParam";
            this.Load += new System.EventHandler(this.Form_SampleParam_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}