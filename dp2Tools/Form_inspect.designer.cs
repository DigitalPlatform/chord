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
            this.textBox1_result2 = new System.Windows.Forms.TextBox();
            this.button_accessNo2 = new System.Windows.Forms.Button();
            this.button_checkNoZch = new System.Windows.Forms.Button();
            this.button_accessNo1 = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_info = new System.Windows.Forms.ToolStripStatusLabel();
            this.button_compareClassNoAccessNo = new System.Windows.Forms.Button();
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
            this.btnCheck.Location = new System.Drawing.Point(3, 3);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(133, 45);
            this.btnCheck.TabIndex = 0;
            this.btnCheck.Text = "检查帐户权限";
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // txtInput
            // 
            this.txtInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInput.Location = new System.Drawing.Point(0, 0);
            this.txtInput.MaxLength = 99999999;
            this.txtInput.Multiline = true;
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(1238, 181);
            this.txtInput.TabIndex = 1;
            // 
            // txtResult
            // 
            this.txtResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtResult.Location = new System.Drawing.Point(3, 105);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.ReadOnly = true;
            this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResult.Size = new System.Drawing.Size(1231, 98);
            this.txtResult.TabIndex = 2;
            // 
            // btnReaderTypeStatic
            // 
            this.btnReaderTypeStatic.Location = new System.Drawing.Point(142, 3);
            this.btnReaderTypeStatic.Name = "btnReaderTypeStatic";
            this.btnReaderTypeStatic.Size = new System.Drawing.Size(216, 45);
            this.btnReaderTypeStatic.TabIndex = 4;
            this.btnReaderTypeStatic.Text = "读者类型/图书类型 统计";
            this.btnReaderTypeStatic.UseVisualStyleBackColor = true;
            this.btnReaderTypeStatic.Click += new System.EventHandler(this.btnReaderTypeStatic_Click);
            // 
            // btnDepartment
            // 
            this.btnDepartment.Location = new System.Drawing.Point(364, 3);
            this.btnDepartment.Name = "btnDepartment";
            this.btnDepartment.Size = new System.Drawing.Size(126, 45);
            this.btnDepartment.TabIndex = 5;
            this.btnDepartment.Text = "读者单位统计";
            this.btnDepartment.UseVisualStyleBackColor = true;
            this.btnDepartment.Click += new System.EventHandler(this.btnDepartment_Click);
            // 
            // btnPrice
            // 
            this.btnPrice.Location = new System.Drawing.Point(496, 2);
            this.btnPrice.Name = "btnPrice";
            this.btnPrice.Size = new System.Drawing.Size(101, 45);
            this.btnPrice.TabIndex = 6;
            this.btnPrice.Text = "价格校验";
            this.btnPrice.UseVisualStyleBackColor = true;
            this.btnPrice.Click += new System.EventHandler(this.btnPrice_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.校验册条码ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1238, 32);
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
            this.校验册条码ToolStripMenuItem.Size = new System.Drawing.Size(116, 28);
            this.校验册条码ToolStripMenuItem.Text = "校验册条码";
            // 
            // 南开实验学校ToolStripMenuItem
            // 
            this.南开实验学校ToolStripMenuItem.Name = "南开实验学校ToolStripMenuItem";
            this.南开实验学校ToolStripMenuItem.Size = new System.Drawing.Size(329, 34);
            this.南开实验学校ToolStripMenuItem.Text = "南开实验学校";
            this.南开实验学校ToolStripMenuItem.Click += new System.EventHandler(this.南开实验学校ToolStripMenuItem_Click);
            // 
            // 北师大天津附中ToolStripMenuItem
            // 
            this.北师大天津附中ToolStripMenuItem.Name = "北师大天津附中ToolStripMenuItem";
            this.北师大天津附中ToolStripMenuItem.Size = new System.Drawing.Size(329, 34);
            this.北师大天津附中ToolStripMenuItem.Text = "B+7位数字（北附,青干院）";
            this.北师大天津附中ToolStripMenuItem.Click += new System.EventHandler(this.北师大天津附中ToolStripMenuItem_Click);
            // 
            // 瑞景中学册条码分析ToolStripMenuItem
            // 
            this.瑞景中学册条码分析ToolStripMenuItem.Name = "瑞景中学册条码分析ToolStripMenuItem";
            this.瑞景中学册条码分析ToolStripMenuItem.Size = new System.Drawing.Size(329, 34);
            this.瑞景中学册条码分析ToolStripMenuItem.Text = "瑞景中学册条码分析";
            this.瑞景中学册条码分析ToolStripMenuItem.Click += new System.EventHandler(this.瑞景中学册条码分析ToolStripMenuItem_Click);
            // 
            // 中规院ToolStripMenuItem
            // 
            this.中规院ToolStripMenuItem.Name = "中规院ToolStripMenuItem";
            this.中规院ToolStripMenuItem.Size = new System.Drawing.Size(329, 34);
            this.中规院ToolStripMenuItem.Text = "中规院";
            this.中规院ToolStripMenuItem.Click += new System.EventHandler(this.中规院ToolStripMenuItem_Click);
            // 
            // 河西教育中心ToolStripMenuItem
            // 
            this.河西教育中心ToolStripMenuItem.Name = "河西教育中心ToolStripMenuItem";
            this.河西教育中心ToolStripMenuItem.Size = new System.Drawing.Size(329, 34);
            this.河西教育中心ToolStripMenuItem.Text = "河西教育中心";
            this.河西教育中心ToolStripMenuItem.Click += new System.EventHandler(this.河西教育中心ToolStripMenuItem_Click_1);
            // 
            // 河北博物院ToolStripMenuItem
            // 
            this.河北博物院ToolStripMenuItem.Name = "河北博物院ToolStripMenuItem";
            this.河北博物院ToolStripMenuItem.Size = new System.Drawing.Size(329, 34);
            this.河北博物院ToolStripMenuItem.Text = "河北博物院";
            this.河北博物院ToolStripMenuItem.Click += new System.EventHandler(this.河北博物院ToolStripMenuItem_Click);
            // 
            // 河北博物院6位ToolStripMenuItem
            // 
            this.河北博物院6位ToolStripMenuItem.Name = "河北博物院6位ToolStripMenuItem";
            this.河北博物院6位ToolStripMenuItem.Size = new System.Drawing.Size(329, 34);
            this.河北博物院6位ToolStripMenuItem.Text = "河北博物院6位";
            this.河北博物院6位ToolStripMenuItem.Click += new System.EventHandler(this.河北博物院6位ToolStripMenuItem_Click);
            // 
            // 光华学院ToolStripMenuItem
            // 
            this.光华学院ToolStripMenuItem.Name = "光华学院ToolStripMenuItem";
            this.光华学院ToolStripMenuItem.Size = new System.Drawing.Size(329, 34);
            this.光华学院ToolStripMenuItem.Text = "光华学院";
            this.光华学院ToolStripMenuItem.Click += new System.EventHandler(this.光华学院ToolStripMenuItem_Click);
            // 
            // btnAccessNo
            // 
            this.btnAccessNo.Location = new System.Drawing.Point(603, 2);
            this.btnAccessNo.Name = "btnAccessNo";
            this.btnAccessNo.Size = new System.Drawing.Size(107, 45);
            this.btnAccessNo.TabIndex = 9;
            this.btnAccessNo.Text = "索取号校验";
            this.btnAccessNo.UseVisualStyleBackColor = true;
            this.btnAccessNo.Click += new System.EventHandler(this.btnAccessNo_Click);
            // 
            // btnBu0
            // 
            this.btnBu0.Location = new System.Drawing.Point(716, 2);
            this.btnBu0.Name = "btnBu0";
            this.btnBu0.Size = new System.Drawing.Size(118, 45);
            this.btnBu0.TabIndex = 10;
            this.btnBu0.Text = "前补0足10位";
            this.btnBu0.UseVisualStyleBackColor = true;
            this.btnBu0.Click += new System.EventHandler(this.btnBu0_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(840, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(87, 45);
            this.button1.TabIndex = 11;
            this.button1.Text = "去时间";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 32);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtInput);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.button_compareClassNoAccessNo);
            this.splitContainer1.Panel2.Controls.Add(this.textBox1_result2);
            this.splitContainer1.Panel2.Controls.Add(this.button_accessNo2);
            this.splitContainer1.Panel2.Controls.Add(this.button_checkNoZch);
            this.splitContainer1.Panel2.Controls.Add(this.button_accessNo1);
            this.splitContainer1.Panel2.Controls.Add(this.btnCheck);
            this.splitContainer1.Panel2.Controls.Add(this.button1);
            this.splitContainer1.Panel2.Controls.Add(this.txtResult);
            this.splitContainer1.Panel2.Controls.Add(this.btnReaderTypeStatic);
            this.splitContainer1.Panel2.Controls.Add(this.btnBu0);
            this.splitContainer1.Panel2.Controls.Add(this.btnDepartment);
            this.splitContainer1.Panel2.Controls.Add(this.btnAccessNo);
            this.splitContainer1.Panel2.Controls.Add(this.btnPrice);
            this.splitContainer1.Size = new System.Drawing.Size(1238, 547);
            this.splitContainer1.SplitterDistance = 181;
            this.splitContainer1.TabIndex = 12;
            // 
            // textBox1_result2
            // 
            this.textBox1_result2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1_result2.Location = new System.Drawing.Point(4, 209);
            this.textBox1_result2.Multiline = true;
            this.textBox1_result2.Name = "textBox1_result2";
            this.textBox1_result2.ReadOnly = true;
            this.textBox1_result2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1_result2.Size = new System.Drawing.Size(1231, 98);
            this.textBox1_result2.TabIndex = 15;
            // 
            // button_accessNo2
            // 
            this.button_accessNo2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button_accessNo2.Location = new System.Drawing.Point(439, 53);
            this.button_accessNo2.Name = "button_accessNo2";
            this.button_accessNo2.Size = new System.Drawing.Size(201, 45);
            this.button_accessNo2.TabIndex = 14;
            this.button_accessNo2.Text = "书目不同，索取号相同";
            this.button_accessNo2.UseVisualStyleBackColor = true;
            this.button_accessNo2.Click += new System.EventHandler(this.button_accessNo2_Click);
            // 
            // button_checkNoZch
            // 
            this.button_checkNoZch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button_checkNoZch.Location = new System.Drawing.Point(3, 54);
            this.button_checkNoZch.Name = "button_checkNoZch";
            this.button_checkNoZch.Size = new System.Drawing.Size(201, 45);
            this.button_checkNoZch.TabIndex = 13;
            this.button_checkNoZch.Text = "筛选非种次号";
            this.button_checkNoZch.UseVisualStyleBackColor = true;
            this.button_checkNoZch.Click += new System.EventHandler(this.button_checkNoZch_Click);
            // 
            // button_accessNo1
            // 
            this.button_accessNo1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button_accessNo1.Location = new System.Drawing.Point(222, 54);
            this.button_accessNo1.Name = "button_accessNo1";
            this.button_accessNo1.Size = new System.Drawing.Size(201, 45);
            this.button_accessNo1.TabIndex = 12;
            this.button_accessNo1.Text = "书目相同，索取号不同";
            this.button_accessNo1.UseVisualStyleBackColor = true;
            this.button_accessNo1.Click += new System.EventHandler(this.button_accessNo1_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_info});
            this.statusStrip1.Location = new System.Drawing.Point(0, 548);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 15, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1238, 31);
            this.statusStrip1.TabIndex = 13;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_info
            // 
            this.toolStripStatusLabel_info.Name = "toolStripStatusLabel_info";
            this.toolStripStatusLabel_info.Size = new System.Drawing.Size(195, 24);
            this.toolStripStatusLabel_info.Text = "toolStripStatusLabel1";
            // 
            // button_compareClassNoAccessNo
            // 
            this.button_compareClassNoAccessNo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button_compareClassNoAccessNo.Location = new System.Drawing.Point(646, 53);
            this.button_compareClassNoAccessNo.Name = "button_compareClassNoAccessNo";
            this.button_compareClassNoAccessNo.Size = new System.Drawing.Size(201, 45);
            this.button_compareClassNoAccessNo.TabIndex = 16;
            this.button_compareClassNoAccessNo.Text = "比对分类号与索取号";
            this.button_compareClassNoAccessNo.UseVisualStyleBackColor = true;
            this.button_compareClassNoAccessNo.Click += new System.EventHandler(this.button_compareClassNoAccessNo_Click);
            // 
            // Form_inspect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1238, 579);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
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
        private System.Windows.Forms.Button button_accessNo1;
        private System.Windows.Forms.Button button_accessNo2;
        private System.Windows.Forms.Button button_checkNoZch;
        private System.Windows.Forms.TextBox textBox1_result2;
        private System.Windows.Forms.Button button_compareClassNoAccessNo;
    }
}

