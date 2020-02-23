namespace dp2Mini
{
    partial class NoteForm
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
            this.label7 = new System.Windows.Forms.Label();
            this.button_takeoff = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.button_notice = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.button_check = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.button_print = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.listView_note = new System.Windows.Forms.ListView();
            this.columnHeader_id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_step = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_patron = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_items = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_createTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_prite = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_printTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_check = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_checkedTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_notice = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_noticeTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_takeoff = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_takeoffTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip_note = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.打印小票预览ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.输出小票信息ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.查看备书结果ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.撤消备书单ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.button_create = new System.Windows.Forms.Button();
            this.listView_items = new System.Windows.Forms.ListView();
            this.columnHeader_path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_checkResult = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_reason = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
            this.contextMenuStrip_note.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Enabled = false;
            this.label7.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(867, 41);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 31);
            this.label7.TabIndex = 29;
            this.label7.Text = "--->";
            // 
            // button_takeoff
            // 
            this.button_takeoff.Enabled = false;
            this.button_takeoff.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_takeoff.Location = new System.Drawing.Point(928, 15);
            this.button_takeoff.Name = "button_takeoff";
            this.button_takeoff.Size = new System.Drawing.Size(319, 83);
            this.button_takeoff.TabIndex = 28;
            this.button_takeoff.Text = "结束备书单\r\n（读者取走图书或不需要取书）";
            this.button_takeoff.UseVisualStyleBackColor = true;
            this.button_takeoff.Click += new System.EventHandler(this.button_takeoff_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Enabled = false;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(651, 41);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 31);
            this.label6.TabIndex = 27;
            this.label6.Text = "--->";
            // 
            // button_notice
            // 
            this.button_notice.Enabled = false;
            this.button_notice.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_notice.Location = new System.Drawing.Point(711, 30);
            this.button_notice.Name = "button_notice";
            this.button_notice.Size = new System.Drawing.Size(153, 52);
            this.button_notice.TabIndex = 26;
            this.button_notice.Text = "通知读者";
            this.button_notice.UseVisualStyleBackColor = true;
            this.button_notice.Click += new System.EventHandler(this.button_notice_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Enabled = false;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(436, 41);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 31);
            this.label5.TabIndex = 25;
            this.label5.Text = "--->";
            // 
            // button_check
            // 
            this.button_check.Enabled = false;
            this.button_check.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_check.Location = new System.Drawing.Point(499, 30);
            this.button_check.Name = "button_check";
            this.button_check.Size = new System.Drawing.Size(150, 52);
            this.button_check.TabIndex = 24;
            this.button_check.Text = "找书完成";
            this.button_check.UseVisualStyleBackColor = true;
            this.button_check.Click += new System.EventHandler(this.button_check_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Enabled = false;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(185, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 31);
            this.label4.TabIndex = 23;
            this.label4.Text = "--->";
            // 
            // button_print
            // 
            this.button_print.Enabled = false;
            this.button_print.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_print.Location = new System.Drawing.Point(250, 30);
            this.button_print.Name = "button_print";
            this.button_print.Size = new System.Drawing.Size(180, 52);
            this.button_print.TabIndex = 22;
            this.button_print.Text = "打印备书小票";
            this.button_print.UseVisualStyleBackColor = true;
            this.button_print.Click += new System.EventHandler(this.button_print_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(9, 102);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(388, 21);
            this.label2.TabIndex = 20;
            this.label2.Text = "当前选择的备书单包含的预约记录详情：";
            // 
            // listView_note
            // 
            this.listView_note.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_note.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_id,
            this.columnHeader_step,
            this.columnHeader_patron,
            this.columnHeader1,
            this.columnHeader_items,
            this.columnHeader_createTime,
            this.columnHeader_prite,
            this.columnHeader_printTime,
            this.columnHeader_check,
            this.columnHeader_checkedTime,
            this.columnHeader_notice,
            this.columnHeader_noticeTime,
            this.columnHeader_takeoff,
            this.columnHeader_takeoffTime});
            this.listView_note.ContextMenuStrip = this.contextMenuStrip_note;
            this.listView_note.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listView_note.FullRowSelect = true;
            this.listView_note.GridLines = true;
            this.listView_note.HideSelection = false;
            this.listView_note.Location = new System.Drawing.Point(12, 12);
            this.listView_note.MultiSelect = false;
            this.listView_note.Name = "listView_note";
            this.listView_note.Size = new System.Drawing.Size(1233, 260);
            this.listView_note.TabIndex = 18;
            this.listView_note.UseCompatibleStateImageBehavior = false;
            this.listView_note.View = System.Windows.Forms.View.Details;
            this.listView_note.SelectedIndexChanged += new System.EventHandler(this.listView_note_SelectedIndexChanged);
            // 
            // columnHeader_id
            // 
            this.columnHeader_id.Text = "备书单号";
            this.columnHeader_id.Width = 98;
            // 
            // columnHeader_step
            // 
            this.columnHeader_step.Text = "进度";
            this.columnHeader_step.Width = 131;
            // 
            // columnHeader_patron
            // 
            this.columnHeader_patron.Text = "读者姓名";
            this.columnHeader_patron.Width = 116;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "读者电话";
            this.columnHeader1.Width = 101;
            // 
            // columnHeader_items
            // 
            this.columnHeader_items.Text = "包含的预约记录";
            this.columnHeader_items.Width = 181;
            // 
            // columnHeader_createTime
            // 
            this.columnHeader_createTime.Text = "创建日期";
            this.columnHeader_createTime.Width = 100;
            // 
            // columnHeader_prite
            // 
            this.columnHeader_prite.Text = "是否打印小票";
            this.columnHeader_prite.Width = 141;
            // 
            // columnHeader_printTime
            // 
            this.columnHeader_printTime.Text = "打印时间";
            this.columnHeader_printTime.Width = 105;
            // 
            // columnHeader_check
            // 
            this.columnHeader_check.Text = "是否备书完成";
            this.columnHeader_check.Width = 142;
            // 
            // columnHeader_checkedTime
            // 
            this.columnHeader_checkedTime.Text = "备书完成时间";
            this.columnHeader_checkedTime.Width = 122;
            // 
            // columnHeader_notice
            // 
            this.columnHeader_notice.Text = "是否通知用户";
            this.columnHeader_notice.Width = 123;
            // 
            // columnHeader_noticeTime
            // 
            this.columnHeader_noticeTime.Text = "通知时间";
            this.columnHeader_noticeTime.Width = 89;
            // 
            // columnHeader_takeoff
            // 
            this.columnHeader_takeoff.Text = "读者是否取走图书";
            this.columnHeader_takeoff.Width = 163;
            // 
            // columnHeader_takeoffTime
            // 
            this.columnHeader_takeoffTime.Text = "取书时间";
            this.columnHeader_takeoffTime.Width = 68;
            // 
            // contextMenuStrip_note
            // 
            this.contextMenuStrip_note.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip_note.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.打印小票预览ToolStripMenuItem,
            this.输出小票信息ToolStripMenuItem,
            this.查看备书结果ToolStripMenuItem,
            this.撤消备书单ToolStripMenuItem});
            this.contextMenuStrip_note.Name = "contextMenuStrip_note";
            this.contextMenuStrip_note.Size = new System.Drawing.Size(189, 124);
            this.contextMenuStrip_note.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_note_Opening);
            // 
            // 打印小票预览ToolStripMenuItem
            // 
            this.打印小票预览ToolStripMenuItem.Name = "打印小票预览ToolStripMenuItem";
            this.打印小票预览ToolStripMenuItem.Size = new System.Drawing.Size(188, 30);
            this.打印小票预览ToolStripMenuItem.Text = "小票打印预览";
            this.打印小票预览ToolStripMenuItem.Click += new System.EventHandler(this.打印小票预览ToolStripMenuItem_Click);
            // 
            // 输出小票信息ToolStripMenuItem
            // 
            this.输出小票信息ToolStripMenuItem.Name = "输出小票信息ToolStripMenuItem";
            this.输出小票信息ToolStripMenuItem.Size = new System.Drawing.Size(188, 30);
            this.输出小票信息ToolStripMenuItem.Text = "查看小票信息";
            this.输出小票信息ToolStripMenuItem.Click += new System.EventHandler(this.输出小票信息ToolStripMenuItem_Click);
            // 
            // 查看备书结果ToolStripMenuItem
            // 
            this.查看备书结果ToolStripMenuItem.Name = "查看备书结果ToolStripMenuItem";
            this.查看备书结果ToolStripMenuItem.Size = new System.Drawing.Size(188, 30);
            this.查看备书结果ToolStripMenuItem.Text = "查看备书结果";
            this.查看备书结果ToolStripMenuItem.Click += new System.EventHandler(this.查看备书结果ToolStripMenuItem_Click);
            // 
            // 撤消备书单ToolStripMenuItem
            // 
            this.撤消备书单ToolStripMenuItem.Name = "撤消备书单ToolStripMenuItem";
            this.撤消备书单ToolStripMenuItem.Size = new System.Drawing.Size(188, 30);
            this.撤消备书单ToolStripMenuItem.Text = "撤消备书单";
            this.撤消备书单ToolStripMenuItem.Click += new System.EventHandler(this.撤消备书单ToolStripMenuItem_Click);
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
            this.splitContainer1.Panel1.Controls.Add(this.listView_note);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.splitContainer1.Panel2.Controls.Add(this.button_create);
            this.splitContainer1.Panel2.Controls.Add(this.listView_items);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.label7);
            this.splitContainer1.Panel2.Controls.Add(this.button_print);
            this.splitContainer1.Panel2.Controls.Add(this.button_takeoff);
            this.splitContainer1.Panel2.Controls.Add(this.label4);
            this.splitContainer1.Panel2.Controls.Add(this.label6);
            this.splitContainer1.Panel2.Controls.Add(this.button_check);
            this.splitContainer1.Panel2.Controls.Add(this.button_notice);
            this.splitContainer1.Panel2.Controls.Add(this.label5);
            this.splitContainer1.Size = new System.Drawing.Size(1258, 563);
            this.splitContainer1.SplitterDistance = 275;
            this.splitContainer1.TabIndex = 30;
            // 
            // button_create
            // 
            this.button_create.Enabled = false;
            this.button_create.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_create.Location = new System.Drawing.Point(16, 30);
            this.button_create.Name = "button_create";
            this.button_create.Size = new System.Drawing.Size(169, 52);
            this.button_create.TabIndex = 31;
            this.button_create.Text = "创建备书单";
            this.button_create.UseVisualStyleBackColor = true;
            // 
            // listView_items
            // 
            this.listView_items.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_items.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_checkResult,
            this.columnHeader_reason,
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
            this.listView_items.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listView_items.FullRowSelect = true;
            this.listView_items.GridLines = true;
            this.listView_items.HideSelection = false;
            this.listView_items.Location = new System.Drawing.Point(12, 127);
            this.listView_items.Margin = new System.Windows.Forms.Padding(4);
            this.listView_items.Name = "listView_items";
            this.listView_items.Size = new System.Drawing.Size(1232, 144);
            this.listView_items.TabIndex = 30;
            this.listView_items.UseCompatibleStateImageBehavior = false;
            this.listView_items.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "路径";
            this.columnHeader_path.Width = 80;
            // 
            // columnHeader_checkResult
            // 
            this.columnHeader_checkResult.Text = "备书结果";
            this.columnHeader_checkResult.Width = 101;
            // 
            // columnHeader_reason
            // 
            this.columnHeader_reason.Text = "原因";
            // 
            // columnHeader_readerBarcode
            // 
            this.columnHeader_readerBarcode.Text = "读者证条码";
            this.columnHeader_readerBarcode.Width = 118;
            // 
            // columnHeader_readerName
            // 
            this.columnHeader_readerName.Text = "读者姓名";
            this.columnHeader_readerName.Width = 96;
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
            // NoteForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1258, 563);
            this.Controls.Add(this.splitContainer1);
            this.Name = "NoteForm";
            this.Text = "备书单管理";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NoteForm_FormClosing);
            this.Load += new System.EventHandler(this.NoteForm_Load);
            this.contextMenuStrip_note.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button_takeoff;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button_notice;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button_check;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_print;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listView_note;
        private System.Windows.Forms.ColumnHeader columnHeader_id;
        private System.Windows.Forms.ColumnHeader columnHeader_createTime;
        private System.Windows.Forms.ColumnHeader columnHeader_items;
        private System.Windows.Forms.ColumnHeader columnHeader_notice;
        private System.Windows.Forms.ColumnHeader columnHeader_takeoff;
        private System.Windows.Forms.ColumnHeader columnHeader_prite;
        private System.Windows.Forms.ColumnHeader columnHeader_printTime;
        private System.Windows.Forms.ColumnHeader columnHeader_check;
        private System.Windows.Forms.ColumnHeader columnHeader_checkedTime;
        private System.Windows.Forms.ColumnHeader columnHeader_noticeTime;
        private System.Windows.Forms.ColumnHeader columnHeader_takeoffTime;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView listView_items;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.ColumnHeader columnHeader_readerBarcode;
        private System.Windows.Forms.ColumnHeader columnHeader_readerName;
        private System.Windows.Forms.ColumnHeader columnHeader_itemBarcode;
        private System.Windows.Forms.ColumnHeader columnHeader_title;
        private System.Windows.Forms.ColumnHeader columnHeader_accessNo;
        private System.Windows.Forms.ColumnHeader columnHeader_location;
        private System.Windows.Forms.ColumnHeader columnHeader_isbn;
        private System.Windows.Forms.ColumnHeader columnHeader_author;
        private System.Windows.Forms.ColumnHeader columnHeader_tel;
        private System.Windows.Forms.ColumnHeader columnHeader_department;
        private System.Windows.Forms.ColumnHeader columnHeader_requestTime;
        private System.Windows.Forms.ColumnHeader columnHeader_arrivedTime;
        private System.Windows.Forms.ColumnHeader columnHeader_state;
        private System.Windows.Forms.ColumnHeader columnHeader_patron;
        private System.Windows.Forms.ColumnHeader columnHeader_step;
        private System.Windows.Forms.Button button_create;
        private System.Windows.Forms.ColumnHeader columnHeader_checkResult;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_note;
        private System.Windows.Forms.ToolStripMenuItem 输出小票信息ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查看备书结果ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 打印小票预览ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 撤消备书单ToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader columnHeader_reason;
    }
}