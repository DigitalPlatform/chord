namespace dp2Mini
{
    partial class MainForm
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip_main = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_prep = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip_main = new System.Windows.Forms.StatusStrip();
            this.toolStripMenuItem_help = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_setting = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip_main
            // 
            this.menuStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_file,
            this.toolStripMenuItem_help});
            this.menuStrip_main.Location = new System.Drawing.Point(0, 0);
            this.menuStrip_main.Name = "menuStrip_main";
            this.menuStrip_main.Size = new System.Drawing.Size(702, 25);
            this.menuStrip_main.TabIndex = 1;
            this.menuStrip_main.Text = "menuStrip_main";
            // 
            // toolStripMenuItem_file
            // 
            this.toolStripMenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_prep});
            this.toolStripMenuItem_file.Name = "toolStripMenuItem_file";
            this.toolStripMenuItem_file.Size = new System.Drawing.Size(58, 21);
            this.toolStripMenuItem_file.Text = "文件(&F)";
            // 
            // toolStripMenuItem_prep
            // 
            this.toolStripMenuItem_prep.Name = "toolStripMenuItem_prep";
            this.toolStripMenuItem_prep.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_prep.Text = "备书(&P)";
            this.toolStripMenuItem_prep.Click += new System.EventHandler(this.toolStripMenuItem_prep_Click);
            // 
            // statusStrip_main
            // 
            this.statusStrip_main.Location = new System.Drawing.Point(0, 577);
            this.statusStrip_main.Name = "statusStrip_main";
            this.statusStrip_main.Size = new System.Drawing.Size(702, 22);
            this.statusStrip_main.TabIndex = 3;
            this.statusStrip_main.Text = "statusStrip_main";
            // 
            // toolStripMenuItem_help
            // 
            this.toolStripMenuItem_help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_setting});
            this.toolStripMenuItem_help.Name = "toolStripMenuItem_help";
            this.toolStripMenuItem_help.Size = new System.Drawing.Size(61, 21);
            this.toolStripMenuItem_help.Text = "帮助(&H)";
            // 
            // toolStripMenuItem_setting
            // 
            this.toolStripMenuItem_setting.Name = "toolStripMenuItem_setting";
            this.toolStripMenuItem_setting.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_setting.Text = "设置(&S)";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(702, 599);
            this.Controls.Add(this.statusStrip_main);
            this.Controls.Add(this.menuStrip_main);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip_main;
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip_main.ResumeLayout(false);
            this.menuStrip_main.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip_main;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_prep;
        private System.Windows.Forms.StatusStrip statusStrip_main;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_help;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_setting;
    }
}

