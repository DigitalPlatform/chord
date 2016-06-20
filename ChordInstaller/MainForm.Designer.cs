namespace ChordInstaller
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
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.MenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_autoUpgrade = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_displayDigitalPlatformEventLog = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_sendDebugInfos = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.MenuItem_dp2Capo = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2capo_install = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2capo_upgrade = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2capo_openDataDir = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2capo_openAppDir = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2capo_instanceManagement = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2capo_tools = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2capo_startService = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2capo_stopService = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2capo_tools_installService = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2capo_tools_uninstallService = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2capo_uninstall = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_help = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openUserFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openDataFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openProgramFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_copyright = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_file,
            this.MenuItem_dp2Capo,
            this.MenuItem_help});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(553, 25);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MenuItem_file
            // 
            this.MenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_autoUpgrade,
            this.toolStripSeparator4,
            this.MenuItem_displayDigitalPlatformEventLog,
            this.MenuItem_sendDebugInfos,
            this.toolStripSeparator8,
            this.MenuItem_exit});
            this.MenuItem_file.Name = "MenuItem_file";
            this.MenuItem_file.Size = new System.Drawing.Size(58, 21);
            this.MenuItem_file.Text = "文件(&F)";
            // 
            // MenuItem_autoUpgrade
            // 
            this.MenuItem_autoUpgrade.Name = "MenuItem_autoUpgrade";
            this.MenuItem_autoUpgrade.Size = new System.Drawing.Size(242, 22);
            this.MenuItem_autoUpgrade.Text = "自动升级全部产品(&A)";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(239, 6);
            // 
            // MenuItem_displayDigitalPlatformEventLog
            // 
            this.MenuItem_displayDigitalPlatformEventLog.Name = "MenuItem_displayDigitalPlatformEventLog";
            this.MenuItem_displayDigitalPlatformEventLog.Size = new System.Drawing.Size(242, 22);
            this.MenuItem_displayDigitalPlatformEventLog.Text = "显示 DigitalPlatform 事件日志";
            // 
            // MenuItem_sendDebugInfos
            // 
            this.MenuItem_sendDebugInfos.Name = "MenuItem_sendDebugInfos";
            this.MenuItem_sendDebugInfos.Size = new System.Drawing.Size(242, 22);
            this.MenuItem_sendDebugInfos.Text = "打包事件日志信息(&S)";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(239, 6);
            // 
            // MenuItem_exit
            // 
            this.MenuItem_exit.Name = "MenuItem_exit";
            this.MenuItem_exit.Size = new System.Drawing.Size(242, 22);
            this.MenuItem_exit.Text = "退出(&X)";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 282);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(553, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 25);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(553, 257);
            this.webBrowser1.TabIndex = 2;
            // 
            // MenuItem_dp2Capo
            // 
            this.MenuItem_dp2Capo.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_dp2capo_install,
            this.MenuItem_dp2capo_upgrade,
            this.toolStripSeparator6,
            this.MenuItem_dp2capo_openDataDir,
            this.MenuItem_dp2capo_openAppDir,
            this.toolStripSeparator9,
            this.MenuItem_dp2capo_instanceManagement,
            this.toolStripSeparator10,
            this.MenuItem_dp2capo_tools});
            this.MenuItem_dp2Capo.Name = "MenuItem_dp2Capo";
            this.MenuItem_dp2Capo.Size = new System.Drawing.Size(74, 21);
            this.MenuItem_dp2Capo.Text = "dp2Capo";
            // 
            // MenuItem_dp2capo_install
            // 
            this.MenuItem_dp2capo_install.Name = "MenuItem_dp2capo_install";
            this.MenuItem_dp2capo_install.Size = new System.Drawing.Size(160, 22);
            this.MenuItem_dp2capo_install.Text = "安装 dp2Capo";
            this.MenuItem_dp2capo_install.Click += new System.EventHandler(this.MenuItem_dp2capo_install_Click);
            // 
            // MenuItem_dp2capo_upgrade
            // 
            this.MenuItem_dp2capo_upgrade.Name = "MenuItem_dp2capo_upgrade";
            this.MenuItem_dp2capo_upgrade.Size = new System.Drawing.Size(160, 22);
            this.MenuItem_dp2capo_upgrade.Text = "升级 dp2Capo";
            this.MenuItem_dp2capo_upgrade.Click += new System.EventHandler(this.MenuItem_dp2capo_upgrade_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(161, 6);
            // 
            // MenuItem_dp2capo_openDataDir
            // 
            this.MenuItem_dp2capo_openDataDir.Name = "MenuItem_dp2capo_openDataDir";
            this.MenuItem_dp2capo_openDataDir.Size = new System.Drawing.Size(164, 22);
            this.MenuItem_dp2capo_openDataDir.Text = "打开数据文件夹";
            // 
            // MenuItem_dp2capo_openAppDir
            // 
            this.MenuItem_dp2capo_openAppDir.Name = "MenuItem_dp2capo_openAppDir";
            this.MenuItem_dp2capo_openAppDir.Size = new System.Drawing.Size(164, 22);
            this.MenuItem_dp2capo_openAppDir.Text = "打开程序文件夹";
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(161, 6);
            // 
            // MenuItem_dp2capo_instanceManagement
            // 
            this.MenuItem_dp2capo_instanceManagement.Name = "MenuItem_dp2capo_instanceManagement";
            this.MenuItem_dp2capo_instanceManagement.Size = new System.Drawing.Size(164, 22);
            this.MenuItem_dp2capo_instanceManagement.Text = "配置实例";
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(161, 6);
            // 
            // MenuItem_dp2capo_tools
            // 
            this.MenuItem_dp2capo_tools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_dp2capo_startService,
            this.MenuItem_dp2capo_stopService,
            this.toolStripSeparator11,
            this.MenuItem_dp2capo_tools_installService,
            this.MenuItem_dp2capo_tools_uninstallService,
            this.toolStripSeparator14,
            this.MenuItem_dp2capo_uninstall});
            this.MenuItem_dp2capo_tools.Name = "MenuItem_dp2capo_tools";
            this.MenuItem_dp2capo_tools.Size = new System.Drawing.Size(164, 22);
            this.MenuItem_dp2capo_tools.Text = "工具";
            // 
            // MenuItem_dp2capo_startService
            // 
            this.MenuItem_dp2capo_startService.Name = "MenuItem_dp2capo_startService";
            this.MenuItem_dp2capo_startService.Size = new System.Drawing.Size(202, 22);
            this.MenuItem_dp2capo_startService.Text = "启动 Windows Service";
            // 
            // MenuItem_dp2capo_stopService
            // 
            this.MenuItem_dp2capo_stopService.Name = "MenuItem_dp2capo_stopService";
            this.MenuItem_dp2capo_stopService.Size = new System.Drawing.Size(202, 22);
            this.MenuItem_dp2capo_stopService.Text = "停止 Windows Service";
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(199, 6);
            // 
            // MenuItem_dp2capo_tools_installService
            // 
            this.MenuItem_dp2capo_tools_installService.Name = "MenuItem_dp2capo_tools_installService";
            this.MenuItem_dp2capo_tools_installService.Size = new System.Drawing.Size(202, 22);
            this.MenuItem_dp2capo_tools_installService.Text = "注册 Windows Service";
            // 
            // MenuItem_dp2capo_tools_uninstallService
            // 
            this.MenuItem_dp2capo_tools_uninstallService.Name = "MenuItem_dp2capo_tools_uninstallService";
            this.MenuItem_dp2capo_tools_uninstallService.Size = new System.Drawing.Size(202, 22);
            this.MenuItem_dp2capo_tools_uninstallService.Text = "注销 Windows Service";
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(199, 6);
            // 
            // MenuItem_dp2capo_uninstall
            // 
            this.MenuItem_dp2capo_uninstall.Name = "MenuItem_dp2capo_uninstall";
            this.MenuItem_dp2capo_uninstall.Size = new System.Drawing.Size(202, 22);
            this.MenuItem_dp2capo_uninstall.Text = "卸载 dp2Kernel";
            // 
            // MenuItem_help
            // 
            this.MenuItem_help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_openUserFolder,
            this.MenuItem_openDataFolder,
            this.MenuItem_openProgramFolder,
            this.toolStripSeparator3,
            this.MenuItem_copyright});
            this.MenuItem_help.Name = "MenuItem_help";
            this.MenuItem_help.Size = new System.Drawing.Size(61, 21);
            this.MenuItem_help.Text = "帮助(&H)";
            // 
            // MenuItem_openUserFolder
            // 
            this.MenuItem_openUserFolder.Name = "MenuItem_openUserFolder";
            this.MenuItem_openUserFolder.Size = new System.Drawing.Size(265, 22);
            this.MenuItem_openUserFolder.Text = "打开 chordInstaller 用户文件夹(&U)";
            this.MenuItem_openUserFolder.Click += new System.EventHandler(this.MenuItem_openUserFolder_Click);
            // 
            // MenuItem_openDataFolder
            // 
            this.MenuItem_openDataFolder.Name = "MenuItem_openDataFolder";
            this.MenuItem_openDataFolder.Size = new System.Drawing.Size(265, 22);
            this.MenuItem_openDataFolder.Text = "打开 chordInstaller 数据文件夹(&D)";
            this.MenuItem_openDataFolder.Click += new System.EventHandler(this.MenuItem_openDataFolder_Click);
            // 
            // MenuItem_openProgramFolder
            // 
            this.MenuItem_openProgramFolder.Name = "MenuItem_openProgramFolder";
            this.MenuItem_openProgramFolder.Size = new System.Drawing.Size(265, 22);
            this.MenuItem_openProgramFolder.Text = "打开 chordInstaller 程序文件夹(&P)";
            this.MenuItem_openProgramFolder.Click += new System.EventHandler(this.MenuItem_openProgramFolder_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(262, 6);
            this.toolStripSeparator3.Visible = false;
            // 
            // MenuItem_copyright
            // 
            this.MenuItem_copyright.Name = "MenuItem_copyright";
            this.MenuItem_copyright.Size = new System.Drawing.Size(265, 22);
            this.MenuItem_copyright.Text = "版权(&C)...";
            this.MenuItem_copyright.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 304);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "Chord 安装工具";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_autoUpgrade;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_displayDigitalPlatformEventLog;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_sendDebugInfos;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_exit;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2Capo;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2capo_install;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2capo_upgrade;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2capo_openDataDir;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2capo_openAppDir;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2capo_instanceManagement;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2capo_tools;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2capo_startService;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2capo_stopService;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2capo_tools_installService;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2capo_tools_uninstallService;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2capo_uninstall;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_help;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openUserFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openDataFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openProgramFolder;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        internal System.Windows.Forms.ToolStripMenuItem MenuItem_copyright;
    }
}

