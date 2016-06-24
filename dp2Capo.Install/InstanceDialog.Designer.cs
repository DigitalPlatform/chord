namespace dp2Capo.Install
{
    partial class InstanceDialog
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
            this.button_edit_dp2mserver = new System.Windows.Forms.Button();
            this.textBox_dp2mserver_def = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_dataDir = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_instanceName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_edit_dp2library = new System.Windows.Forms.Button();
            this.textBox_dp2library_def = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_edit_dp2mserver
            // 
            this.button_edit_dp2mserver.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_edit_dp2mserver.Location = new System.Drawing.Point(387, 187);
            this.button_edit_dp2mserver.Name = "button_edit_dp2mserver";
            this.button_edit_dp2mserver.Size = new System.Drawing.Size(45, 23);
            this.button_edit_dp2mserver.TabIndex = 26;
            this.button_edit_dp2mserver.Text = "...";
            this.button_edit_dp2mserver.UseVisualStyleBackColor = true;
            this.button_edit_dp2mserver.Click += new System.EventHandler(this.button_edit_dp2mserver_Click);
            // 
            // textBox_dp2mserver_def
            // 
            this.textBox_dp2mserver_def.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dp2mserver_def.Location = new System.Drawing.Point(155, 187);
            this.textBox_dp2mserver_def.Multiline = true;
            this.textBox_dp2mserver_def.Name = "textBox_dp2mserver_def";
            this.textBox_dp2mserver_def.ReadOnly = true;
            this.textBox_dp2mserver_def.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_dp2mserver_def.Size = new System.Drawing.Size(226, 105);
            this.textBox_dp2mserver_def.TabIndex = 25;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 190);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 12);
            this.label4.TabIndex = 24;
            this.label4.Text = "dp2MServer服务器(&M):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(358, 331);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 37;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(277, 331);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 36;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_dataDir
            // 
            this.textBox_dataDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dataDir.Location = new System.Drawing.Point(155, 39);
            this.textBox_dataDir.Name = "textBox_dataDir";
            this.textBox_dataDir.Size = new System.Drawing.Size(226, 21);
            this.textBox_dataDir.TabIndex = 23;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 22;
            this.label2.Text = "实例目录(&D):";
            // 
            // textBox_instanceName
            // 
            this.textBox_instanceName.Location = new System.Drawing.Point(155, 12);
            this.textBox_instanceName.Name = "textBox_instanceName";
            this.textBox_instanceName.Size = new System.Drawing.Size(165, 21);
            this.textBox_instanceName.TabIndex = 21;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 20;
            this.label1.Text = "实例名(&N):";
            // 
            // button_edit_dp2library
            // 
            this.button_edit_dp2library.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_edit_dp2library.Location = new System.Drawing.Point(387, 80);
            this.button_edit_dp2library.Name = "button_edit_dp2library";
            this.button_edit_dp2library.Size = new System.Drawing.Size(45, 23);
            this.button_edit_dp2library.TabIndex = 42;
            this.button_edit_dp2library.Text = "...";
            this.button_edit_dp2library.UseVisualStyleBackColor = true;
            this.button_edit_dp2library.Click += new System.EventHandler(this.button_edit_dp2library_Click);
            // 
            // textBox_dp2library_def
            // 
            this.textBox_dp2library_def.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dp2library_def.Location = new System.Drawing.Point(155, 80);
            this.textBox_dp2library_def.Multiline = true;
            this.textBox_dp2library_def.Name = "textBox_dp2library_def";
            this.textBox_dp2library_def.ReadOnly = true;
            this.textBox_dp2library_def.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_dp2library_def.Size = new System.Drawing.Size(226, 101);
            this.textBox_dp2library_def.TabIndex = 41;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 83);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(125, 12);
            this.label7.TabIndex = 40;
            this.label7.Text = "dp2Library服务器(&L):";
            // 
            // InstanceDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(445, 366);
            this.Controls.Add(this.button_edit_dp2library);
            this.Controls.Add(this.textBox_dp2library_def);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.button_edit_dp2mserver);
            this.Controls.Add(this.textBox_dp2mserver_def);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_dataDir);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_instanceName);
            this.Controls.Add(this.label1);
            this.Name = "InstanceDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "一个实例";
            this.Load += new System.EventHandler(this.InstanceDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_edit_dp2mserver;
        private System.Windows.Forms.TextBox textBox_dp2mserver_def;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TextBox textBox_dataDir;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_instanceName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_edit_dp2library;
        private System.Windows.Forms.TextBox textBox_dp2library_def;
        private System.Windows.Forms.Label label7;
    }
}