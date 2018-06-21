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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip_main = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_prep = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_help = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_setting = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip_main = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_message = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_loginName = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip_main = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_prep = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.menuStrip_main.SuspendLayout();
            this.statusStrip_main.SuspendLayout();
            this.toolStrip_main.SuspendLayout();
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
            this.toolStripMenuItem_prep.Size = new System.Drawing.Size(115, 22);
            this.toolStripMenuItem_prep.Text = "备书(&P)";
            this.toolStripMenuItem_prep.Click += new System.EventHandler(this.toolStripMenuItem_prep_Click);
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
            this.toolStripMenuItem_setting.Size = new System.Drawing.Size(115, 22);
            this.toolStripMenuItem_setting.Text = "设置(&S)";
            this.toolStripMenuItem_setting.Click += new System.EventHandler(this.toolStripMenuItem_setting_Click);
            // 
            // statusStrip_main
            // 
            this.statusStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_message,
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel_loginName});
            this.statusStrip_main.Location = new System.Drawing.Point(0, 411);
            this.statusStrip_main.Name = "statusStrip_main";
            this.statusStrip_main.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this.statusStrip_main.Size = new System.Drawing.Size(702, 26);
            this.statusStrip_main.TabIndex = 3;
            this.statusStrip_main.Text = "statusStrip_main";
            // 
            // toolStripStatusLabel_message
            // 
            this.toolStripStatusLabel_message.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusLabel_message.Name = "toolStripStatusLabel_message";
            this.toolStripStatusLabel_message.Size = new System.Drawing.Size(591, 21);
            this.toolStripStatusLabel_message.Spring = true;
            this.toolStripStatusLabel_message.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(96, 21);
            this.toolStripStatusLabel1.Text = "当前登录账户：";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // toolStripStatusLabel_loginName
            // 
            this.toolStripStatusLabel_loginName.Name = "toolStripStatusLabel_loginName";
            this.toolStripStatusLabel_loginName.Size = new System.Drawing.Size(0, 21);
            this.toolStripStatusLabel_loginName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStrip_main
            // 
            this.toolStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_prep,
            this.toolStripButton1});
            this.toolStrip_main.Location = new System.Drawing.Point(0, 25);
            this.toolStrip_main.Name = "toolStrip_main";
            this.toolStrip_main.Size = new System.Drawing.Size(702, 25);
            this.toolStrip_main.TabIndex = 5;
            this.toolStrip_main.Text = "toolStrip1";
            // 
            // toolStripButton_prep
            // 
            this.toolStripButton_prep.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_prep.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_prep.Image")));
            this.toolStripButton_prep.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_prep.Name = "toolStripButton_prep";
            this.toolStripButton_prep.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton_prep.Text = "备书";
            this.toolStripButton_prep.Click += new System.EventHandler(this.toolStripButton_prep_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(702, 437);
            this.Controls.Add(this.toolStrip_main);
            this.Controls.Add(this.statusStrip_main);
            this.Controls.Add(this.menuStrip_main);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip_main;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "馆员备书";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip_main.ResumeLayout(false);
            this.menuStrip_main.PerformLayout();
            this.statusStrip_main.ResumeLayout(false);
            this.statusStrip_main.PerformLayout();
            this.toolStrip_main.ResumeLayout(false);
            this.toolStrip_main.PerformLayout();
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
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_message;
        private System.Windows.Forms.ToolStrip toolStrip_main;
        private System.Windows.Forms.ToolStripButton toolStripButton_prep;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_loginName;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
    }
}

