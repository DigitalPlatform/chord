namespace dp2Tools
{
    partial class Form_inspect
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
            this.btnCheck = new System.Windows.Forms.Button();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.btnReaderTypeStatic = new System.Windows.Forms.Button();
            this.btnDepartment = new System.Windows.Forms.Button();
            this.btnPrice = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.校验册条码ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.南开实验学校ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.北师大天津附中ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.瑞景中学册条码分析ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.中规院ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.河西教育中心ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.河北博物院ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.河北博物院6位ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.光华学院ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAccessNo = new System.Windows.Forms.Button();
            this.btnBu0 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_info = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCheck
            // 
            this.btnCheck.Location = new System.Drawing.Point(3, 2);
            this.btnCheck.Margin = new System.Windows.Forms.Padding(2);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(154, 38);
            this.btnCheck.TabIndex = 0;
            this.btnCheck.Text = "检查帐户权限";
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // txtInput
            // 
            this.txtInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInput.Location = new System.Drawing.Point(0, 0);
            this.txtInput.Margin = new System.Windows.Forms.Padding(2);
            this.txtInput.MaxLength = 99999999;
            this.txtInput.Multiline = true;
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(1100, 152);
            this.txtInput.TabIndex = 1;
            // 
            // txtResult
            // 
            this.txtResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtResult.Location = new System.Drawing.Point(3, 55);
            this.txtResult.Margin = new System.Windows.Forms.Padding(2);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.ReadOnly = true;
            this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResult.Size = new System.Drawing.Size(1095, 243);
            this.txtResult.TabIndex = 2;
            // 
            // btnReaderTypeStatic
            // 
            this.btnReaderTypeStatic.Location = new System.Drawing.Point(177, 2);
            this.btnReaderTypeStatic.Margin = new System.Windows.Forms.Padding(2);
            this.btnReaderTypeStatic.Name = "btnReaderTypeStatic";
            this.btnReaderTypeStatic.Size = new System.Drawing.Size(210, 38);
            this.btnReaderTypeStatic.TabIndex = 4;
            this.btnReaderTypeStatic.Text = "读者类型/图书类型 统计";
            this.btnReaderTypeStatic.UseVisualStyleBackColor = true;
            this.btnReaderTypeStatic.Click += new System.EventHandler(this.btnReaderTypeStatic_Click);
            // 
            // btnDepartment
            // 
            this.btnDepartment.Location = new System.Drawing.Point(408, 2);
            this.btnDepartment.Margin = new System.Windows.Forms.Padding(2);
            this.btnDepartment.Name = "btnDepartment";
            this.btnDepartment.Size = new System.Drawing.Size(144, 38);
            this.btnDepartment.TabIndex = 5;
            this.btnDepartment.Text = "读者单位统计";
            this.btnDepartment.UseVisualStyleBackColor = true;
            this.btnDepartment.Click += new System.EventHandler(this.btnDepartment_Click);
            // 
            // btnPrice
            // 
            this.btnPrice.Location = new System.Drawing.Point(576, 2);
            this.btnPrice.Margin = new System.Windows.Forms.Padding(2);
            this.btnPrice.Name = "btnPrice";
            this.btnPrice.Size = new System.Drawing.Size(144, 38);
            this.btnPrice.TabIndex = 6;
            this.btnPrice.Text = "价格校验";
            this.btnPrice.UseVisualStyleBackColor = true;
            this.btnPrice.Click += new System.EventHandler(this.btnPrice_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.校验册条码ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(1100, 26);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 校验册条码ToolStripMenuItem
            // 
            this.校验册条码ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.南开实验学校ToolStripMenuItem,
            this.北师大天津附中ToolStripMenuItem,
            this.瑞景中学册条码分析ToolStripMenuItem,
            this.中规院ToolStripMenuItem,
            this.河西教育中心ToolStripMenuItem,
            this.河北博物院ToolStripMenuItem,
            this.河北博物院6位ToolStripMenuItem,
            this.光华学院ToolStripMenuItem});
            this.校验册条码ToolStripMenuItem.Name = "校验册条码ToolStripMenuItem";
            this.校验册条码ToolStripMenuItem.Size = new System.Drawing.Size(96, 24);
            this.校验册条码ToolStripMenuItem.Text = "校验册条码";
            // 
            // 南开实验学校ToolStripMenuItem
            // 
            this.南开实验学校ToolStripMenuItem.Name = "南开实验学校ToolStripMenuItem";
            this.南开实验学校ToolStripMenuItem.Size = new System.Drawing.Size(267, 26);
            this.南开实验学校ToolStripMenuItem.Text = "南开实验学校";
            this.南开实验学校ToolStripMenuItem.Click += new System.EventHandler(this.南开实验学校ToolStripMenuItem_Click);
            // 
            // 北师大天津附中ToolStripMenuItem
            // 
            this.北师大天津附中ToolStripMenuItem.Name = "北师大天津附中ToolStripMenuItem";
            this.北师大天津附中ToolStripMenuItem.Size = new System.Drawing.Size(267, 26);
            this.北师大天津附中ToolStripMenuItem.Text = "B+7位数字（北附,青干院）";
            this.北师大天津附中ToolStripMenuItem.Click += new System.EventHandler(this.北师大天津附中ToolStripMenuItem_Click);
            // 
            // 瑞景中学册条码分析ToolStripMenuItem
            // 
            this.瑞景中学册条码分析ToolStripMenuItem.Name = "瑞景中学册条码分析ToolStripMenuItem";
            this.瑞景中学册条码分析ToolStripMenuItem.Size = new System.Drawing.Size(267, 26);
            this.瑞景中学册条码分析ToolStripMenuItem.Text = "瑞景中学册条码分析";
            this.瑞景中学册条码分析ToolStripMenuItem.Click += new System.EventHandler(this.瑞景中学册条码分析ToolStripMenuItem_Click);
            // 
            // 中规院ToolStripMenuItem
            // 
            this.中规院ToolStripMenuItem.Name = "中规院ToolStripMenuItem";
            this.中规院ToolStripMenuItem.Size = new System.Drawing.Size(267, 26);
            this.中规院ToolStripMenuItem.Text = "中规院";
            this.中规院ToolStripMenuItem.Click += new System.EventHandler(this.中规院ToolStripMenuItem_Click);
            // 
            // 河西教育中心ToolStripMenuItem
            // 
            this.河西教育中心ToolStripMenuItem.Name = "河西教育中心ToolStripMenuItem";
            this.河西教育中心ToolStripMenuItem.Size = new System.Drawing.Size(267, 26);
            this.河西教育中心ToolStripMenuItem.Text = "河西教育中心";
            this.河西教育中心ToolStripMenuItem.Click += new System.EventHandler(this.河西教育中心ToolStripMenuItem_Click_1);
            // 
            // 河北博物院ToolStripMenuItem
            // 
            this.河北博物院ToolStripMenuItem.Name = "河北博物院ToolStripMenuItem";
            this.河北博物院ToolStripMenuItem.Size = new System.Drawing.Size(267, 26);
            this.河北博物院ToolStripMenuItem.Text = "河北博物院";
            this.河北博物院ToolStripMenuItem.Click += new System.EventHandler(this.河北博物院ToolStripMenuItem_Click);
            // 
            // 河北博物院6位ToolStripMenuItem
            // 
            this.河北博物院6位ToolStripMenuItem.Name = "河北博物院6位ToolStripMenuItem";
            this.河北博物院6位ToolStripMenuItem.Size = new System.Drawing.Size(267, 26);
            this.河北博物院6位ToolStripMenuItem.Text = "河北博物院6位";
            this.河北博物院6位ToolStripMenuItem.Click += new System.EventHandler(this.河北博物院6位ToolStripMenuItem_Click);
            // 
            // 光华学院ToolStripMenuItem
            // 
            this.光华学院ToolStripMenuItem.Name = "光华学院ToolStripMenuItem";
            this.光华学院ToolStripMenuItem.Size = new System.Drawing.Size(267, 26);
            this.光华学院ToolStripMenuItem.Text = "光华学院";
            this.光华学院ToolStripMenuItem.Click += new System.EventHandler(this.光华学院ToolStripMenuItem_Click);
            // 
            // btnAccessNo
            // 
            this.btnAccessNo.Location = new System.Drawing.Point(738, 2);
            this.btnAccessNo.Margin = new System.Windows.Forms.Padding(2);
            this.btnAccessNo.Name = "btnAccessNo";
            this.btnAccessNo.Size = new System.Drawing.Size(144, 38);
            this.btnAccessNo.TabIndex = 9;
            this.btnAccessNo.Text = "索取号校验";
            this.btnAccessNo.UseVisualStyleBackColor = true;
            this.btnAccessNo.Click += new System.EventHandler(this.btnAccessNo_Click);
            // 
            // btnBu0
            // 
            this.btnBu0.Location = new System.Drawing.Point(886, 2);
            this.btnBu0.Margin = new System.Windows.Forms.Padding(2);
            this.btnBu0.Name = "btnBu0";
            this.btnBu0.Size = new System.Drawing.Size(111, 38);
            this.btnBu0.TabIndex = 10;
            this.btnBu0.Text = "前补0足10位";
            this.btnBu0.UseVisualStyleBackColor = true;
            this.btnBu0.Click += new System.EventHandler(this.btnBu0_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1002, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(78, 38);
            this.button1.TabIndex = 11;
            this.button1.Text = "去时间";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 26);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtInput);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnCheck);
            this.splitContainer1.Panel2.Controls.Add(this.button1);
            this.splitContainer1.Panel2.Controls.Add(this.txtResult);
            this.splitContainer1.Panel2.Controls.Add(this.btnReaderTypeStatic);
            this.splitContainer1.Panel2.Controls.Add(this.btnBu0);
            this.splitContainer1.Panel2.Controls.Add(this.btnDepartment);
            this.splitContainer1.Panel2.Controls.Add(this.btnAccessNo);
            this.splitContainer1.Panel2.Controls.Add(this.btnPrice);
            this.splitContainer1.Size = new System.Drawing.Size(1100, 456);
            this.splitContainer1.SplitterDistance = 152;
            this.splitContainer1.TabIndex = 12;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_info});
            this.statusStrip1.Location = new System.Drawing.Point(0, 457);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1100, 25);
            this.statusStrip1.TabIndex = 13;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_info
            // 
            this.toolStripStatusLabel_info.Name = "toolStripStatusLabel_info";
            this.toolStripStatusLabel_info.Size = new System.Drawing.Size(167, 20);
            this.toolStripStatusLabel_info.Text = "toolStripStatusLabel1";
            // 
            // Form_inspect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 482);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form_inspect";
            this.Text = "巡检工具";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.Button btnReaderTypeStatic;
        private System.Windows.Forms.Button btnDepartment;
        private System.Windows.Forms.Button btnPrice;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 校验册条码ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 南开实验学校ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 北师大天津附中ToolStripMenuItem;
        private System.Windows.Forms.Button btnAccessNo;
        private System.Windows.Forms.Button btnBu0;
        private System.Windows.Forms.ToolStripMenuItem 瑞景中学册条码分析ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 中规院ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 河西教育中心ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 河北博物院ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 河北博物院6位ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 光华学院ToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_info;
    }
}

