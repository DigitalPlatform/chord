namespace TestClient1
{
    partial class EntityDialog
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
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_refID = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_oldRecord = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_style = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_action = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_newRecord = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button_editOldRecord = new System.Windows.Forms.Button();
            this.button_editNewRecord = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(301, 305);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 61;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_OK.Location = new System.Drawing.Point(220, 305);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 60;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_refID
            // 
            this.textBox_refID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_refID.Location = new System.Drawing.Point(113, 66);
            this.textBox_refID.Name = "textBox_refID";
            this.textBox_refID.Size = new System.Drawing.Size(263, 21);
            this.textBox_refID.TabIndex = 59;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 69);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 58;
            this.label4.Text = "RefID:";
            // 
            // textBox_oldRecord
            // 
            this.textBox_oldRecord.AcceptsReturn = true;
            this.textBox_oldRecord.AcceptsTab = true;
            this.textBox_oldRecord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_oldRecord.Location = new System.Drawing.Point(14, 110);
            this.textBox_oldRecord.Multiline = true;
            this.textBox_oldRecord.Name = "textBox_oldRecord";
            this.textBox_oldRecord.ReadOnly = true;
            this.textBox_oldRecord.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_oldRecord.Size = new System.Drawing.Size(309, 67);
            this.textBox_oldRecord.TabIndex = 57;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 12);
            this.label3.TabIndex = 56;
            this.label3.Text = "Old Record:";
            // 
            // textBox_style
            // 
            this.textBox_style.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_style.Location = new System.Drawing.Point(113, 39);
            this.textBox_style.Name = "textBox_style";
            this.textBox_style.Size = new System.Drawing.Size(263, 21);
            this.textBox_style.TabIndex = 55;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 54;
            this.label1.Text = "Style:";
            // 
            // textBox_action
            // 
            this.textBox_action.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_action.Location = new System.Drawing.Point(113, 12);
            this.textBox_action.Name = "textBox_action";
            this.textBox_action.Size = new System.Drawing.Size(263, 21);
            this.textBox_action.TabIndex = 53;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 52;
            this.label2.Text = "Action:";
            // 
            // textBox_newRecord
            // 
            this.textBox_newRecord.AcceptsReturn = true;
            this.textBox_newRecord.AcceptsTab = true;
            this.textBox_newRecord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_newRecord.Location = new System.Drawing.Point(14, 202);
            this.textBox_newRecord.Multiline = true;
            this.textBox_newRecord.Name = "textBox_newRecord";
            this.textBox_newRecord.ReadOnly = true;
            this.textBox_newRecord.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_newRecord.Size = new System.Drawing.Size(309, 67);
            this.textBox_newRecord.TabIndex = 63;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 187);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 12);
            this.label5.TabIndex = 62;
            this.label5.Text = "New Record:";
            // 
            // button_editOldRecord
            // 
            this.button_editOldRecord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editOldRecord.Location = new System.Drawing.Point(329, 110);
            this.button_editOldRecord.Name = "button_editOldRecord";
            this.button_editOldRecord.Size = new System.Drawing.Size(47, 23);
            this.button_editOldRecord.TabIndex = 64;
            this.button_editOldRecord.Text = "...";
            this.button_editOldRecord.UseVisualStyleBackColor = true;
            this.button_editOldRecord.Click += new System.EventHandler(this.button_editOldRecord_Click);
            // 
            // button_editNewRecord
            // 
            this.button_editNewRecord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editNewRecord.Location = new System.Drawing.Point(329, 202);
            this.button_editNewRecord.Name = "button_editNewRecord";
            this.button_editNewRecord.Size = new System.Drawing.Size(47, 23);
            this.button_editNewRecord.TabIndex = 65;
            this.button_editNewRecord.Text = "...";
            this.button_editNewRecord.UseVisualStyleBackColor = true;
            this.button_editNewRecord.Click += new System.EventHandler(this.button_editNewRecord_Click);
            // 
            // EntityDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(388, 340);
            this.Controls.Add(this.button_editNewRecord);
            this.Controls.Add(this.button_editOldRecord);
            this.Controls.Add(this.textBox_newRecord);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_refID);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_oldRecord);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_style);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_action);
            this.Controls.Add(this.label2);
            this.Name = "EntityDialog";
            this.Text = "EntityDialog";
            this.Load += new System.EventHandler(this.EntityDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TextBox textBox_refID;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_oldRecord;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_style;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_action;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_newRecord;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button_editOldRecord;
        private System.Windows.Forms.Button button_editNewRecord;
    }
}