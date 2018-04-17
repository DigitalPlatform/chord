namespace dp2Tools
{
    partial class Form_Class
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.button_stop = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_loadClassTable = new System.Windows.Forms.Button();
            this.button_outputCount = new System.Windows.Forms.Button();
            this.button_search = new System.Windows.Forms.Button();
            this.textBox_inputClass = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.textBox_result = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.button_stop);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel1.Controls.Add(this.button_search);
            this.splitContainer1.Panel1.Controls.Add(this.textBox_inputClass);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainer1.Panel2.Controls.Add(this.textBox_result);
            this.splitContainer1.Size = new System.Drawing.Size(804, 550);
            this.splitContainer1.SplitterDistance = 224;
            this.splitContainer1.TabIndex = 0;
            // 
            // button_stop
            // 
            this.button_stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_stop.Location = new System.Drawing.Point(724, 160);
            this.button_stop.Margin = new System.Windows.Forms.Padding(2);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(69, 38);
            this.button_stop.TabIndex = 5;
            this.button_stop.Text = "中断";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.button_loadClassTable);
            this.groupBox1.Controls.Add(this.button_outputCount);
            this.groupBox1.Location = new System.Drawing.Point(12, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(780, 76);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            // 
            // button_loadClassTable
            // 
            this.button_loadClassTable.Location = new System.Drawing.Point(7, 23);
            this.button_loadClassTable.Name = "button_loadClassTable";
            this.button_loadClassTable.Size = new System.Drawing.Size(151, 38);
            this.button_loadClassTable.TabIndex = 0;
            this.button_loadClassTable.Text = "加载简表到内存";
            this.button_loadClassTable.UseVisualStyleBackColor = true;
            this.button_loadClassTable.Click += new System.EventHandler(this.button_loadClassTable_Click);
            // 
            // button_outputCount
            // 
            this.button_outputCount.Location = new System.Drawing.Point(179, 23);
            this.button_outputCount.Margin = new System.Windows.Forms.Padding(2);
            this.button_outputCount.Name = "button_outputCount";
            this.button_outputCount.Size = new System.Drawing.Size(194, 38);
            this.button_outputCount.TabIndex = 3;
            this.button_outputCount.Text = "输出简表分类号匹配次数";
            this.button_outputCount.UseVisualStyleBackColor = true;
            this.button_outputCount.Click += new System.EventHandler(this.button_outputCount_Click);
            // 
            // button_search
            // 
            this.button_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_search.Location = new System.Drawing.Point(724, 118);
            this.button_search.Margin = new System.Windows.Forms.Padding(2);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(69, 38);
            this.button_search.TabIndex = 2;
            this.button_search.Text = "匹配";
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // textBox_inputClass
            // 
            this.textBox_inputClass.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_inputClass.Location = new System.Drawing.Point(11, 118);
            this.textBox_inputClass.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_inputClass.MaxLength = 999999999;
            this.textBox_inputClass.Multiline = true;
            this.textBox_inputClass.Name = "textBox_inputClass";
            this.textBox_inputClass.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_inputClass.Size = new System.Drawing.Size(709, 95);
            this.textBox_inputClass.TabIndex = 1;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 297);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(804, 25);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(200, 19);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(167, 20);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // textBox_result
            // 
            this.textBox_result.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_result.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_result.Location = new System.Drawing.Point(0, 0);
            this.textBox_result.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_result.MaxLength = 999999999;
            this.textBox_result.Multiline = true;
            this.textBox_result.Name = "textBox_result";
            this.textBox_result.ReadOnly = true;
            this.textBox_result.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_result.Size = new System.Drawing.Size(804, 322);
            this.textBox_result.TabIndex = 4;
            // 
            // Form_Class
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 550);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form_Class";
            this.Text = "分类简表匹配工具";
            this.Load += new System.EventHandler(this.Form_Class_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox textBox_inputClass;
        private System.Windows.Forms.Button button_search;
        private System.Windows.Forms.Button button_outputCount;
        private System.Windows.Forms.TextBox textBox_result;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_loadClassTable;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button button_stop;
    }
}