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
            this.splitContainer_prep = new System.Windows.Forms.SplitContainer();
            this.button_search = new System.Windows.Forms.Button();
            this.textBox_queryWord = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.listView_results = new System.Windows.Forms.ListView();
            this.columnHeader_path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_itemBarcode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_isbn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_title = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_author = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_accessNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_location = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_readerBarcode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_readerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_department = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_state = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_isPrint = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip_prep = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_print = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_printPreview = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_move = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_prep)).BeginInit();
            this.splitContainer_prep.Panel1.SuspendLayout();
            this.splitContainer_prep.Panel2.SuspendLayout();
            this.splitContainer_prep.SuspendLayout();
            this.contextMenuStrip_prep.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_prep
            // 
            this.splitContainer_prep.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_prep.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_prep.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainer_prep.Name = "splitContainer_prep";
            this.splitContainer_prep.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_prep.Panel1
            // 
            this.splitContainer_prep.Panel1.Controls.Add(this.button_search);
            this.splitContainer_prep.Panel1.Controls.Add(this.textBox_queryWord);
            this.splitContainer_prep.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer_prep.Panel2
            // 
            this.splitContainer_prep.Panel2.Controls.Add(this.listView_results);
            this.splitContainer_prep.Size = new System.Drawing.Size(1502, 858);
            this.splitContainer_prep.SplitterDistance = 69;
            this.splitContainer_prep.SplitterWidth = 6;
            this.splitContainer_prep.TabIndex = 0;
            // 
            // button_search
            // 
            this.button_search.Location = new System.Drawing.Point(444, 15);
            this.button_search.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(112, 34);
            this.button_search.TabIndex = 2;
            this.button_search.Text = "查询";
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // textBox_queryWord
            // 
            this.textBox_queryWord.Location = new System.Drawing.Point(160, 18);
            this.textBox_queryWord.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_queryWord.Name = "textBox_queryWord";
            this.textBox_queryWord.Size = new System.Drawing.Size(272, 28);
            this.textBox_queryWord.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 22);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "读者证条码号：";
            // 
            // listView_results
            // 
            this.listView_results.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_results.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_itemBarcode,
            this.columnHeader_isbn,
            this.columnHeader_title,
            this.columnHeader_author,
            this.columnHeader_accessNo,
            this.columnHeader_location,
            this.columnHeader_readerBarcode,
            this.columnHeader_readerName,
            this.columnHeader_department,
            this.columnHeader_state,
            this.columnHeader_isPrint});
            this.listView_results.ContextMenuStrip = this.contextMenuStrip_prep;
            this.listView_results.FullRowSelect = true;
            this.listView_results.GridLines = true;
            this.listView_results.HideSelection = false;
            this.listView_results.Location = new System.Drawing.Point(18, 4);
            this.listView_results.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.listView_results.Name = "listView_results";
            this.listView_results.Size = new System.Drawing.Size(1464, 758);
            this.listView_results.TabIndex = 0;
            this.listView_results.UseCompatibleStateImageBehavior = false;
            this.listView_results.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "路径";
            this.columnHeader_path.Width = 150;
            // 
            // columnHeader_itemBarcode
            // 
            this.columnHeader_itemBarcode.Text = "册条码";
            this.columnHeader_itemBarcode.Width = 100;
            // 
            // columnHeader_isbn
            // 
            this.columnHeader_isbn.Text = "ISBN";
            this.columnHeader_isbn.Width = 100;
            // 
            // columnHeader_title
            // 
            this.columnHeader_title.Text = "书名";
            this.columnHeader_title.Width = 150;
            // 
            // columnHeader_author
            // 
            this.columnHeader_author.Text = "作者";
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
            // columnHeader_readerBarcode
            // 
            this.columnHeader_readerBarcode.Text = "预约者条码号";
            this.columnHeader_readerBarcode.Width = 100;
            // 
            // columnHeader_readerName
            // 
            this.columnHeader_readerName.Text = "预约者姓名";
            this.columnHeader_readerName.Width = 100;
            // 
            // columnHeader_department
            // 
            this.columnHeader_department.Text = "预约者部门";
            this.columnHeader_department.Width = 100;
            // 
            // columnHeader_state
            // 
            this.columnHeader_state.Text = "到书状态";
            this.columnHeader_state.Width = 100;
            // 
            // columnHeader_isPrint
            // 
            this.columnHeader_isPrint.Text = "打印状态";
            this.columnHeader_isPrint.Width = 100;
            // 
            // contextMenuStrip_prep
            // 
            this.contextMenuStrip_prep.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip_prep.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_print,
            this.toolStripMenuItem_printPreview,
            this.ToolStripMenuItem_move});
            this.contextMenuStrip_prep.Name = "contextMenuStrip1";
            this.contextMenuStrip_prep.Size = new System.Drawing.Size(177, 94);
            this.contextMenuStrip_prep.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // toolStripMenuItem_print
            // 
            this.toolStripMenuItem_print.Name = "toolStripMenuItem_print";
            this.toolStripMenuItem_print.Size = new System.Drawing.Size(176, 30);
            this.toolStripMenuItem_print.Text = "打印(&P)";
            this.toolStripMenuItem_print.Click += new System.EventHandler(this.toolStripMenuItem_print_Click);
            // 
            // toolStripMenuItem_printPreview
            // 
            this.toolStripMenuItem_printPreview.Name = "toolStripMenuItem_printPreview";
            this.toolStripMenuItem_printPreview.Size = new System.Drawing.Size(176, 30);
            this.toolStripMenuItem_printPreview.Text = "打印预览(&V)";
            this.toolStripMenuItem_printPreview.Click += new System.EventHandler(this.toolStripMenuItem_printPreview_Click);
            // 
            // ToolStripMenuItem_move
            // 
            this.ToolStripMenuItem_move.Name = "ToolStripMenuItem_move";
            this.ToolStripMenuItem_move.Size = new System.Drawing.Size(176, 30);
            this.ToolStripMenuItem_move.Text = "移除(&M)";
            this.ToolStripMenuItem_move.Click += new System.EventHandler(this.toolStripMenuItem_remove_Click);
            // 
            // PrepForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1502, 858);
            this.Controls.Add(this.splitContainer_prep);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "PrepForm";
            this.Text = "PrepForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PrepForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PrepForm_FormClosed);
            this.Load += new System.EventHandler(this.PrepForm_Load);
            this.splitContainer_prep.Panel1.ResumeLayout(false);
            this.splitContainer_prep.Panel1.PerformLayout();
            this.splitContainer_prep.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_prep)).EndInit();
            this.splitContainer_prep.ResumeLayout(false);
            this.contextMenuStrip_prep.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_prep;
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
        private System.Windows.Forms.ColumnHeader columnHeader_isPrint;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_prep;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_print;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_printPreview;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_move;
    }
}