namespace testSearch
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnGetSummaryAndItem = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(71, 6);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(219, 21);
            this.txtPath.TabIndex = 2;
            this.txtPath.Text = "中文图书/78";
            // 
            // btnGetSummaryAndItem
            // 
            this.btnGetSummaryAndItem.Location = new System.Drawing.Point(71, 33);
            this.btnGetSummaryAndItem.Name = "btnGetSummaryAndItem";
            this.btnGetSummaryAndItem.Size = new System.Drawing.Size(131, 23);
            this.btnGetSummaryAndItem.TabIndex = 3;
            this.btnGetSummaryAndItem.Text = "GetSummaryAndItem";
            this.btnGetSummaryAndItem.UseVisualStyleBackColor = true;
            this.btnGetSummaryAndItem.Click += new System.EventHandler(this.btnGetSummaryAndItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "书目路径";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(357, 318);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnGetSummaryAndItem);
            this.Controls.Add(this.txtPath);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnGetSummaryAndItem;
        private System.Windows.Forms.Label label1;
    }
}

