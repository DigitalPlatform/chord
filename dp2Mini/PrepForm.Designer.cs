namespace dp2Mini
{
    partial class PrepForm
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
            this.components = new System.ComponentModel.Container();
            this.button_search = new System.Windows.Forms.Button();
            this.textBox_queryWord = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.listView_results = new System.Windows.Forms.ListView();
            this.columnHeader_path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_readerBarcode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_readerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_itemBarcode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_title = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_accessNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_location = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_isbn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_author = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_tel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_department = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_requestTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_arrivedTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_state = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip_prep = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_createNote = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_arrived = new System.Windows.Forms.TabPage();
            this.tabPage_outof = new System.Windows.Forms.TabPage();
            this.listView_outof = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_stop = new System.Windows.Forms.Button();
            this.全选AToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip_prep.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage_arrived.SuspendLayout();
            this.tabPage_outof.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_search
            // 
            this.button_search.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_search.Location = new System.Drawing.Point(440, 13);
            this.button_search.Margin = new System.Windows.Forms.Padding(4);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(112, 34);
            this.button_search.TabIndex = 2;
            this.button_search.Text = "查询";
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // textBox_queryWord
            // 
            this.textBox_queryWord.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_queryWord.Location = new System.Drawing.Point(156, 16);
            this.textBox_queryWord.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_queryWord.Name = "textBox_queryWord";
            this.textBox_queryWord.Size = new System.Drawing.Size(272, 31);
            this.textBox_queryWord.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(14, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(157, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "读者证条码号：";
            // 
            // listView_results
            // 
            this.listView_results.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_readerBarcode,
            this.columnHeader_readerName,
            this.columnHeader_itemBarcode,
            this.columnHeader_title,
            this.columnHeader_accessNo,
            this.columnHeader_location,
            this.columnHeader_isbn,
            this.columnHeader_author,
            this.columnHeader_tel,
            this.columnHeader_department,
            this.columnHeader_requestTime,
            this.columnHeader_arrivedTime,
            this.columnHeader_state});
            this.listView_results.ContextMenuStrip = this.contextMenuStrip_prep;
            this.listView_results.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_results.FullRowSelect = true;
            this.listView_results.GridLines = true;
            this.listView_results.HideSelection = false;
            this.listView_results.Location = new System.Drawing.Point(3, 3);
            this.listView_results.Margin = new System.Windows.Forms.Padding(4);
            this.listView_results.Name = "listView_results";
            this.listView_results.Size = new System.Drawing.Size(1188, 434);
            this.listView_results.TabIndex = 0;
            this.listView_results.UseCompatibleStateImageBehavior = false;
            this.listView_results.View = System.Windows.Forms.View.Details;
            this.listView_results.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_results_ColumnClick);
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "路径";
            this.columnHeader_path.Width = 97;
            // 
            // columnHeader_readerBarcode
            // 
            this.columnHeader_readerBarcode.Text = "读者证条码";
            this.columnHeader_readerBarcode.Width = 108;
            // 
            // columnHeader_readerName
            // 
            this.columnHeader_readerName.Text = "读者姓名";
            this.columnHeader_readerName.Width = 100;
            // 
            // columnHeader_itemBarcode
            // 
            this.columnHeader_itemBarcode.Text = "册条码";
            this.columnHeader_itemBarcode.Width = 100;
            // 
            // columnHeader_title
            // 
            this.columnHeader_title.Text = "书名";
            this.columnHeader_title.Width = 113;
            // 
            // columnHeader_accessNo
            // 
            this.columnHeader_accessNo.Text = "索取号";
            this.columnHeader_accessNo.Width = 100;
            // 
            // columnHeader_location
            // 
            this.columnHeader_location.Text = "馆藏地点";
            this.columnHeader_location.Width = 100;
            // 
            // columnHeader_isbn
            // 
            this.columnHeader_isbn.Text = "ISBN";
            this.columnHeader_isbn.Width = 77;
            // 
            // columnHeader_author
            // 
            this.columnHeader_author.Text = "作者";
            // 
            // columnHeader_tel
            // 
            this.columnHeader_tel.Text = "读者电话";
            this.columnHeader_tel.Width = 94;
            // 
            // columnHeader_department
            // 
            this.columnHeader_department.Text = "读者部门";
            this.columnHeader_department.Width = 87;
            // 
            // columnHeader_requestTime
            // 
            this.columnHeader_requestTime.Text = "预约时间";
            this.columnHeader_requestTime.Width = 97;
            // 
            // columnHeader_arrivedTime
            // 
            this.columnHeader_arrivedTime.Text = "到书时间";
            this.columnHeader_arrivedTime.Width = 113;
            // 
            // columnHeader_state
            // 
            this.columnHeader_state.Text = "预约状态";
            this.columnHeader_state.Width = 100;
            // 
            // contextMenuStrip_prep
            // 
            this.contextMenuStrip_prep.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip_prep.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.全选AToolStripMenuItem,
            this.toolStripMenuItem_createNote});
            this.contextMenuStrip_prep.Name = "contextMenuStrip1";
            this.contextMenuStrip_prep.Size = new System.Drawing.Size(241, 97);
            this.contextMenuStrip_prep.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // toolStripMenuItem_createNote
            // 
            this.toolStripMenuItem_createNote.Name = "toolStripMenuItem_createNote";
            this.toolStripMenuItem_createNote.Size = new System.Drawing.Size(240, 30);
            this.toolStripMenuItem_createNote.Text = "创建备书单(&P)";
            this.toolStripMenuItem_createNote.Click += new System.EventHandler(this.toolStripMenuItem_bs_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_arrived);
            this.tabControl1.Controls.Add(this.tabPage_outof);
            this.tabControl1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl1.Location = new System.Drawing.Point(12, 54);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1202, 475);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage_arrived
            // 
            this.tabPage_arrived.Controls.Add(this.listView_results);
            this.tabPage_arrived.Location = new System.Drawing.Point(4, 31);
            this.tabPage_arrived.Name = "tabPage_arrived";
            this.tabPage_arrived.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_arrived.Size = new System.Drawing.Size(1194, 440);
            this.tabPage_arrived.TabIndex = 0;
            this.tabPage_arrived.Text = "预约到书";
            this.tabPage_arrived.UseVisualStyleBackColor = true;
            // 
            // tabPage_outof
            // 
            this.tabPage_outof.Controls.Add(this.listView_outof);
            this.tabPage_outof.Location = new System.Drawing.Point(4, 31);
            this.tabPage_outof.Name = "tabPage_outof";
            this.tabPage_outof.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_outof.Size = new System.Drawing.Size(1194, 440);
            this.tabPage_outof.TabIndex = 1;
            this.tabPage_outof.Text = "超过保留期";
            this.tabPage_outof.UseVisualStyleBackColor = true;
            // 
            // listView_outof
            // 
            this.listView_outof.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader13,
            this.columnHeader14});
            this.listView_outof.ContextMenuStrip = this.contextMenuStrip_prep;
            this.listView_outof.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_outof.FullRowSelect = true;
            this.listView_outof.GridLines = true;
            this.listView_outof.HideSelection = false;
            this.listView_outof.Location = new System.Drawing.Point(3, 3);
            this.listView_outof.Margin = new System.Windows.Forms.Padding(4);
            this.listView_outof.Name = "listView_outof";
            this.listView_outof.Size = new System.Drawing.Size(1188, 434);
            this.listView_outof.TabIndex = 1;
            this.listView_outof.UseCompatibleStateImageBehavior = false;
            this.listView_outof.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "路径";
            this.columnHeader1.Width = 97;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "读者证条码";
            this.columnHeader2.Width = 108;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "读者姓名";
            this.columnHeader3.Width = 100;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "册条码";
            this.columnHeader4.Width = 100;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "书名";
            this.columnHeader5.Width = 113;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "索取号";
            this.columnHeader6.Width = 100;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "馆藏地点";
            this.columnHeader7.Width = 100;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "ISBN";
            this.columnHeader8.Width = 77;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "作者";
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "读者电话";
            this.columnHeader10.Width = 94;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "读者部门";
            this.columnHeader11.Width = 87;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "预约时间";
            this.columnHeader12.Width = 97;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "到书时间";
            this.columnHeader13.Width = 113;
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "预约状态";
            this.columnHeader14.Width = 100;
            // 
            // button_stop
            // 
            this.button_stop.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_stop.Location = new System.Drawing.Point(560, 13);
            this.button_stop.Margin = new System.Windows.Forms.Padding(4);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(112, 34);
            this.button_stop.TabIndex = 3;
            this.button_stop.Text = "停止";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // 全选AToolStripMenuItem
            // 
            this.全选AToolStripMenuItem.Name = "全选AToolStripMenuItem";
            this.全选AToolStripMenuItem.Size = new System.Drawing.Size(240, 30);
            this.全选AToolStripMenuItem.Text = "全选(&A)";
            this.全选AToolStripMenuItem.Click += new System.EventHandler(this.全选AToolStripMenuItem_Click);
            // 
            // PrepForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1221, 540);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button_search);
            this.Controls.Add(this.textBox_queryWord);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "PrepForm";
            this.Text = "预约到书查询";
            this.Load += new System.EventHandler(this.PrepForm_Load);
            this.contextMenuStrip_prep.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_arrived.ResumeLayout(false);
            this.tabPage_outof.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button_search;
        private System.Windows.Forms.TextBox textBox_queryWord;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listView_results;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.ColumnHeader columnHeader_itemBarcode;
        private System.Windows.Forms.ColumnHeader columnHeader_isbn;
        private System.Windows.Forms.ColumnHeader columnHeader_title;
        private System.Windows.Forms.ColumnHeader columnHeader_author;
        private System.Windows.Forms.ColumnHeader columnHeader_accessNo;
        private System.Windows.Forms.ColumnHeader columnHeader_location;
        private System.Windows.Forms.ColumnHeader columnHeader_readerBarcode;
        private System.Windows.Forms.ColumnHeader columnHeader_readerName;
        private System.Windows.Forms.ColumnHeader columnHeader_department;
        private System.Windows.Forms.ColumnHeader columnHeader_state;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_prep;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_createNote;
        private System.Windows.Forms.ColumnHeader columnHeader_requestTime;
        private System.Windows.Forms.ColumnHeader columnHeader_arrivedTime;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_arrived;
        private System.Windows.Forms.TabPage tabPage_outof;
        private System.Windows.Forms.ColumnHeader columnHeader_tel;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.ListView listView_outof;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.ColumnHeader columnHeader14;
        private System.Windows.Forms.ToolStripMenuItem 全选AToolStripMenuItem;
    }
}