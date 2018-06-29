namespace dp2Capo.Install
{
    partial class GlobalConfigDialog
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
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_z3950 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_listeningPort = new System.Windows.Forms.TextBox();
            this.checkBox_enableZ3950Server = new System.Windows.Forms.CheckBox();
            this.tabControl_main.SuspendLayout();
            this.tabPage_z3950.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(480, 381);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(112, 34);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(359, 381);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(112, 34);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_z3950);
            this.tabControl_main.Location = new System.Drawing.Point(13, 13);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(579, 361);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_z3950
            // 
            this.tabPage_z3950.Controls.Add(this.checkBox_enableZ3950Server);
            this.tabPage_z3950.Controls.Add(this.textBox_listeningPort);
            this.tabPage_z3950.Controls.Add(this.label1);
            this.tabPage_z3950.Location = new System.Drawing.Point(4, 28);
            this.tabPage_z3950.Name = "tabPage_z3950";
            this.tabPage_z3950.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_z3950.Size = new System.Drawing.Size(571, 329);
            this.tabPage_z3950.TabIndex = 0;
            this.tabPage_z3950.Text = "Z39.50 服务";
            this.tabPage_z3950.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 18);
            this.label1.TabIndex = 1;
            this.label1.Text = "监听端口号(&P):";
            // 
            // textBox_listeningPort
            // 
            this.textBox_listeningPort.Enabled = false;
            this.textBox_listeningPort.Location = new System.Drawing.Point(177, 63);
            this.textBox_listeningPort.Name = "textBox_listeningPort";
            this.textBox_listeningPort.Size = new System.Drawing.Size(104, 28);
            this.textBox_listeningPort.TabIndex = 2;
            this.textBox_listeningPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // checkBox_enableZ3950Server
            // 
            this.checkBox_enableZ3950Server.AutoSize = true;
            this.checkBox_enableZ3950Server.Location = new System.Drawing.Point(11, 26);
            this.checkBox_enableZ3950Server.Name = "checkBox_enableZ3950Server";
            this.checkBox_enableZ3950Server.Size = new System.Drawing.Size(205, 22);
            this.checkBox_enableZ3950Server.TabIndex = 0;
            this.checkBox_enableZ3950Server.Text = "启用 Z39.50 服务(&E)";
            this.checkBox_enableZ3950Server.UseVisualStyleBackColor = true;
            this.checkBox_enableZ3950Server.CheckedChanged += new System.EventHandler(this.checkBox_enableZ3950Server_CheckedChanged);
            // 
            // GlobalConfigDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(605, 428);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "GlobalConfigDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "全局参数";
            this.Load += new System.EventHandler(this.GlobalConfigDialog_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_z3950.ResumeLayout(false);
            this.tabPage_z3950.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_z3950;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_listeningPort;
        private System.Windows.Forms.CheckBox checkBox_enableZ3950Server;
    }
}