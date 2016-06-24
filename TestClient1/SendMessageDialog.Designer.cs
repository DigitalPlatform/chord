namespace TestClient1
{
    partial class SendMessageDialog
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
            this.textBox_message_text = new System.Windows.Forms.TextBox();
            this.textBox_groupName = new System.Windows.Forms.TextBox();
            this.label37 = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.textBox_subjects = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_message_text
            // 
            this.textBox_message_text.AcceptsReturn = true;
            this.textBox_message_text.AcceptsTab = true;
            this.textBox_message_text.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message_text.Location = new System.Drawing.Point(83, 39);
            this.textBox_message_text.Multiline = true;
            this.textBox_message_text.Name = "textBox_message_text";
            this.textBox_message_text.Size = new System.Drawing.Size(451, 94);
            this.textBox_message_text.TabIndex = 41;
            // 
            // textBox_groupName
            // 
            this.textBox_groupName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_groupName.Location = new System.Drawing.Point(83, 12);
            this.textBox_groupName.Name = "textBox_groupName";
            this.textBox_groupName.Size = new System.Drawing.Size(451, 21);
            this.textBox_groupName.TabIndex = 43;
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(6, 42);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(35, 12);
            this.label37.TabIndex = 40;
            this.label37.Text = "Text:";
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Location = new System.Drawing.Point(6, 16);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(71, 12);
            this.label38.TabIndex = 42;
            this.label38.Text = "Group Name:";
            // 
            // textBox_subjects
            // 
            this.textBox_subjects.AcceptsReturn = true;
            this.textBox_subjects.AcceptsTab = true;
            this.textBox_subjects.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_subjects.Location = new System.Drawing.Point(83, 139);
            this.textBox_subjects.Multiline = true;
            this.textBox_subjects.Name = "textBox_subjects";
            this.textBox_subjects.Size = new System.Drawing.Size(451, 62);
            this.textBox_subjects.TabIndex = 45;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 142);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 44;
            this.label1.Text = "Subject:";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(378, 345);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 46;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(459, 345);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 47;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // SendMessageDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(546, 380);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_subjects);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_message_text);
            this.Controls.Add(this.textBox_groupName);
            this.Controls.Add(this.label37);
            this.Controls.Add(this.label38);
            this.Name = "SendMessageDialog";
            this.Text = "SendMessageDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_message_text;
        private System.Windows.Forms.TextBox textBox_groupName;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.TextBox textBox_subjects;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}