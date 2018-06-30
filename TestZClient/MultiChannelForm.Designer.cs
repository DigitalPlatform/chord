namespace TestZClient
{
    partial class MultiChannelForm
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
            this.listView_channels = new System.Windows.Forms.ListView();
            this.columnHeader_index = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_requestCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_begin = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown_channelCount = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_channelCount)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "通道列表:";
            // 
            // listView_channels
            // 
            this.listView_channels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_channels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_index,
            this.columnHeader_requestCount});
            this.listView_channels.FullRowSelect = true;
            this.listView_channels.Location = new System.Drawing.Point(16, 35);
            this.listView_channels.Name = "listView_channels";
            this.listView_channels.Size = new System.Drawing.Size(557, 345);
            this.listView_channels.TabIndex = 1;
            this.listView_channels.UseCompatibleStateImageBehavior = false;
            this.listView_channels.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_index
            // 
            this.columnHeader_index.Text = "序号";
            this.columnHeader_index.Width = 91;
            // 
            // columnHeader_requestCount
            // 
            this.columnHeader_requestCount.Text = "请求数";
            this.columnHeader_requestCount.Width = 126;
            // 
            // button_begin
            // 
            this.button_begin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_begin.Location = new System.Drawing.Point(462, 403);
            this.button_begin.Name = "button_begin";
            this.button_begin.Size = new System.Drawing.Size(111, 35);
            this.button_begin.TabIndex = 2;
            this.button_begin.Text = "开始";
            this.button_begin.UseVisualStyleBackColor = true;
            this.button_begin.Click += new System.EventHandler(this.button_begin_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 411);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 18);
            this.label2.TabIndex = 3;
            this.label2.Text = "通道数:";
            // 
            // numericUpDown_channelCount
            // 
            this.numericUpDown_channelCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.numericUpDown_channelCount.Location = new System.Drawing.Point(89, 409);
            this.numericUpDown_channelCount.Name = "numericUpDown_channelCount";
            this.numericUpDown_channelCount.Size = new System.Drawing.Size(120, 28);
            this.numericUpDown_channelCount.TabIndex = 4;
            this.numericUpDown_channelCount.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // MultiChannelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 450);
            this.Controls.Add(this.numericUpDown_channelCount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_begin);
            this.Controls.Add(this.listView_channels);
            this.Controls.Add(this.label1);
            this.Name = "MultiChannelForm";
            this.Text = "MultiChannelForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MultiChannelForm_FormClosed);
            this.Load += new System.EventHandler(this.MultiChannelForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_channelCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listView_channels;
        private System.Windows.Forms.ColumnHeader columnHeader_index;
        private System.Windows.Forms.ColumnHeader columnHeader_requestCount;
        private System.Windows.Forms.Button button_begin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown_channelCount;
    }
}