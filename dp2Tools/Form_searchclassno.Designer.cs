namespace dp2Tools
{
    partial class Form_searchclassno
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
            this.txt_chinese = new System.Windows.Forms.TextBox();
            this.btn_search = new System.Windows.Forms.Button();
            this.lbl_msg = new System.Windows.Forms.Label();
            this.txt_result = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txt_chinese
            // 
            this.txt_chinese.Location = new System.Drawing.Point(0, 2);
            this.txt_chinese.Multiline = true;
            this.txt_chinese.Name = "txt_chinese";
            this.txt_chinese.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt_chinese.Size = new System.Drawing.Size(1629, 136);
            this.txt_chinese.TabIndex = 0;
            // 
            // btn_search
            // 
            this.btn_search.Location = new System.Drawing.Point(0, 142);
            this.btn_search.Name = "btn_search";
            this.btn_search.Size = new System.Drawing.Size(150, 61);
            this.btn_search.TabIndex = 1;
            this.btn_search.Text = "查询";
            this.btn_search.UseVisualStyleBackColor = true;
            this.btn_search.Click += new System.EventHandler(this.btn_search_Click);
            // 
            // lbl_msg
            // 
            this.lbl_msg.AutoSize = true;
            this.lbl_msg.Location = new System.Drawing.Point(212, 160);
            this.lbl_msg.Name = "lbl_msg";
            this.lbl_msg.Size = new System.Drawing.Size(0, 24);
            this.lbl_msg.TabIndex = 2;
            // 
            // txt_result
            // 
            this.txt_result.Location = new System.Drawing.Point(0, 209);
            this.txt_result.Multiline = true;
            this.txt_result.Name = "txt_result";
            this.txt_result.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt_result.Size = new System.Drawing.Size(1629, 487);
            this.txt_result.TabIndex = 3;
            // 
            // Form_searchclassno
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1641, 733);
            this.Controls.Add(this.txt_result);
            this.Controls.Add(this.lbl_msg);
            this.Controls.Add(this.btn_search);
            this.Controls.Add(this.txt_chinese);
            this.Name = "Form_searchclassno";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_chinese;
        private System.Windows.Forms.Button btn_search;
        private System.Windows.Forms.Label lbl_msg;
        private System.Windows.Forms.TextBox txt_result;
    }
}