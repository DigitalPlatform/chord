namespace TestClient1
{
    partial class SetInfoDialog
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
            this.comboBox_operation = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_biblioRecPath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.listView_entities = new System.Windows.Forms.ListView();
            this.columnHeader_action = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_oldRecord = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_newRecord = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_style = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_errorInfo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_errorCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_refID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_new = new System.Windows.Forms.Button();
            this.button_modify = new System.Windows.Forms.Button();
            this.button_delete = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Operation:";
            // 
            // comboBox_operation
            // 
            this.comboBox_operation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_operation.FormattingEnabled = true;
            this.comboBox_operation.Location = new System.Drawing.Point(153, 12);
            this.comboBox_operation.Name = "comboBox_operation";
            this.comboBox_operation.Size = new System.Drawing.Size(234, 20);
            this.comboBox_operation.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "Biblio RecPath:";
            // 
            // textBox_biblioRecPath
            // 
            this.textBox_biblioRecPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_biblioRecPath.Location = new System.Drawing.Point(153, 39);
            this.textBox_biblioRecPath.Name = "textBox_biblioRecPath";
            this.textBox_biblioRecPath.Size = new System.Drawing.Size(234, 21);
            this.textBox_biblioRecPath.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "Entities:";
            // 
            // listView_entities
            // 
            this.listView_entities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_entities.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_action,
            this.columnHeader_oldRecord,
            this.columnHeader_newRecord,
            this.columnHeader_style,
            this.columnHeader_errorInfo,
            this.columnHeader_errorCode,
            this.columnHeader_refID});
            this.listView_entities.FullRowSelect = true;
            this.listView_entities.HideSelection = false;
            this.listView_entities.Location = new System.Drawing.Point(14, 90);
            this.listView_entities.Name = "listView_entities";
            this.listView_entities.Size = new System.Drawing.Size(373, 149);
            this.listView_entities.TabIndex = 5;
            this.listView_entities.UseCompatibleStateImageBehavior = false;
            this.listView_entities.View = System.Windows.Forms.View.Details;
            this.listView_entities.SelectedIndexChanged += new System.EventHandler(this.listView_entities_SelectedIndexChanged);
            this.listView_entities.DoubleClick += new System.EventHandler(this.listView_entities_DoubleClick);
            // 
            // columnHeader_action
            // 
            this.columnHeader_action.Text = "Action";
            // 
            // columnHeader_oldRecord
            // 
            this.columnHeader_oldRecord.Text = "Old Record";
            this.columnHeader_oldRecord.Width = 100;
            // 
            // columnHeader_newRecord
            // 
            this.columnHeader_newRecord.Text = "New Record";
            this.columnHeader_newRecord.Width = 100;
            // 
            // columnHeader_style
            // 
            this.columnHeader_style.Text = "Style";
            // 
            // columnHeader_errorInfo
            // 
            this.columnHeader_errorInfo.Text = "ErrorInfo";
            // 
            // columnHeader_errorCode
            // 
            this.columnHeader_errorCode.Text = "Error Code";
            // 
            // columnHeader_refID
            // 
            this.columnHeader_refID.Text = "Reference ID";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(312, 274);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 49;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(231, 274);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 48;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_new
            // 
            this.button_new.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_new.Location = new System.Drawing.Point(14, 245);
            this.button_new.Name = "button_new";
            this.button_new.Size = new System.Drawing.Size(75, 23);
            this.button_new.TabIndex = 50;
            this.button_new.Text = "New";
            this.button_new.UseVisualStyleBackColor = true;
            this.button_new.Click += new System.EventHandler(this.button_new_Click);
            // 
            // button_modify
            // 
            this.button_modify.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_modify.Enabled = false;
            this.button_modify.Location = new System.Drawing.Point(95, 245);
            this.button_modify.Name = "button_modify";
            this.button_modify.Size = new System.Drawing.Size(75, 23);
            this.button_modify.TabIndex = 51;
            this.button_modify.Text = "Modify";
            this.button_modify.UseVisualStyleBackColor = true;
            this.button_modify.Click += new System.EventHandler(this.button_modify_Click);
            // 
            // button_delete
            // 
            this.button_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_delete.Enabled = false;
            this.button_delete.Location = new System.Drawing.Point(176, 245);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(75, 23);
            this.button_delete.TabIndex = 52;
            this.button_delete.Text = "Delete";
            this.button_delete.UseVisualStyleBackColor = true;
            this.button_delete.Click += new System.EventHandler(this.button_delete_Click);
            // 
            // SetInfoDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(399, 309);
            this.Controls.Add(this.button_delete);
            this.Controls.Add(this.button_modify);
            this.Controls.Add(this.button_new);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listView_entities);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_biblioRecPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_operation);
            this.Controls.Add(this.label1);
            this.Name = "SetInfoDialog";
            this.Text = "SetInfoDialog";
            this.Load += new System.EventHandler(this.SetInfoDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_operation;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_biblioRecPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListView listView_entities;
        private System.Windows.Forms.ColumnHeader columnHeader_action;
        private System.Windows.Forms.ColumnHeader columnHeader_oldRecord;
        private System.Windows.Forms.ColumnHeader columnHeader_newRecord;
        private System.Windows.Forms.ColumnHeader columnHeader_style;
        private System.Windows.Forms.ColumnHeader columnHeader_errorInfo;
        private System.Windows.Forms.ColumnHeader columnHeader_errorCode;
        private System.Windows.Forms.ColumnHeader columnHeader_refID;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_new;
        private System.Windows.Forms.Button button_modify;
        private System.Windows.Forms.Button button_delete;
    }
}