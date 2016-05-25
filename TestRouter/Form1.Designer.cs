namespace TestRouter
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_sender = new System.Windows.Forms.TabPage();
            this.button_sender_begin = new System.Windows.Forms.Button();
            this.textBox_sender_password = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_sender_userName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage_reciever = new System.Windows.Forms.TabPage();
            this.button_reciever_stop = new System.Windows.Forms.Button();
            this.button_reviever_begin = new System.Windows.Forms.Button();
            this.textBox_reciever_password = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_reciever_userName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_messageServerUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_groupName = new System.Windows.Forms.TextBox();
            this.label38 = new System.Windows.Forms.Label();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.button_sender_stop = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_clearDisplay = new System.Windows.Forms.ToolStripButton();
            this.tabControl1.SuspendLayout();
            this.tabPage_sender.SuspendLayout();
            this.tabPage_reciever.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_sender);
            this.tabControl1.Controls.Add(this.tabPage_reciever);
            this.tabControl1.Location = new System.Drawing.Point(13, 94);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(440, 127);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_sender
            // 
            this.tabPage_sender.Controls.Add(this.button_sender_stop);
            this.tabPage_sender.Controls.Add(this.button_sender_begin);
            this.tabPage_sender.Controls.Add(this.textBox_sender_password);
            this.tabPage_sender.Controls.Add(this.label3);
            this.tabPage_sender.Controls.Add(this.textBox_sender_userName);
            this.tabPage_sender.Controls.Add(this.label2);
            this.tabPage_sender.Location = new System.Drawing.Point(4, 22);
            this.tabPage_sender.Name = "tabPage_sender";
            this.tabPage_sender.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_sender.Size = new System.Drawing.Size(432, 101);
            this.tabPage_sender.TabIndex = 0;
            this.tabPage_sender.Text = "发送者";
            this.tabPage_sender.UseVisualStyleBackColor = true;
            // 
            // button_sender_begin
            // 
            this.button_sender_begin.Location = new System.Drawing.Point(101, 60);
            this.button_sender_begin.Name = "button_sender_begin";
            this.button_sender_begin.Size = new System.Drawing.Size(75, 23);
            this.button_sender_begin.TabIndex = 10;
            this.button_sender_begin.Text = "开始发送";
            this.button_sender_begin.UseVisualStyleBackColor = true;
            this.button_sender_begin.Click += new System.EventHandler(this.button_sender_begin_Click);
            // 
            // textBox_sender_password
            // 
            this.textBox_sender_password.Location = new System.Drawing.Point(101, 33);
            this.textBox_sender_password.Name = "textBox_sender_password";
            this.textBox_sender_password.PasswordChar = '*';
            this.textBox_sender_password.Size = new System.Drawing.Size(161, 21);
            this.textBox_sender_password.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "Password:";
            // 
            // textBox_sender_userName
            // 
            this.textBox_sender_userName.Location = new System.Drawing.Point(101, 6);
            this.textBox_sender_userName.Name = "textBox_sender_userName";
            this.textBox_sender_userName.Size = new System.Drawing.Size(161, 21);
            this.textBox_sender_userName.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "User Name:";
            // 
            // tabPage_reciever
            // 
            this.tabPage_reciever.Controls.Add(this.button_reciever_stop);
            this.tabPage_reciever.Controls.Add(this.button_reviever_begin);
            this.tabPage_reciever.Controls.Add(this.textBox_reciever_password);
            this.tabPage_reciever.Controls.Add(this.label4);
            this.tabPage_reciever.Controls.Add(this.textBox_reciever_userName);
            this.tabPage_reciever.Controls.Add(this.label5);
            this.tabPage_reciever.Location = new System.Drawing.Point(4, 22);
            this.tabPage_reciever.Name = "tabPage_reciever";
            this.tabPage_reciever.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_reciever.Size = new System.Drawing.Size(432, 101);
            this.tabPage_reciever.TabIndex = 1;
            this.tabPage_reciever.Text = "接收者";
            this.tabPage_reciever.UseVisualStyleBackColor = true;
            // 
            // button_reciever_stop
            // 
            this.button_reciever_stop.Location = new System.Drawing.Point(180, 60);
            this.button_reciever_stop.Name = "button_reciever_stop";
            this.button_reciever_stop.Size = new System.Drawing.Size(75, 23);
            this.button_reciever_stop.TabIndex = 11;
            this.button_reciever_stop.Text = "停止";
            this.button_reciever_stop.UseVisualStyleBackColor = true;
            this.button_reciever_stop.Click += new System.EventHandler(this.button_reciever_stop_Click);
            // 
            // button_reviever_begin
            // 
            this.button_reviever_begin.Location = new System.Drawing.Point(99, 60);
            this.button_reviever_begin.Name = "button_reviever_begin";
            this.button_reviever_begin.Size = new System.Drawing.Size(75, 23);
            this.button_reviever_begin.TabIndex = 10;
            this.button_reviever_begin.Text = "开始接收";
            this.button_reviever_begin.UseVisualStyleBackColor = true;
            this.button_reviever_begin.Click += new System.EventHandler(this.button_reviever_begin_Click);
            // 
            // textBox_reciever_password
            // 
            this.textBox_reciever_password.Location = new System.Drawing.Point(104, 33);
            this.textBox_reciever_password.Name = "textBox_reciever_password";
            this.textBox_reciever_password.PasswordChar = '*';
            this.textBox_reciever_password.Size = new System.Drawing.Size(161, 21);
            this.textBox_reciever_password.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 36);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "Password:";
            // 
            // textBox_reciever_userName
            // 
            this.textBox_reciever_userName.Location = new System.Drawing.Point(104, 6);
            this.textBox_reciever_userName.Name = "textBox_reciever_userName";
            this.textBox_reciever_userName.Size = new System.Drawing.Size(161, 21);
            this.textBox_reciever_userName.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 6;
            this.label5.Text = "User Name:";
            // 
            // textBox_messageServerUrl
            // 
            this.textBox_messageServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_messageServerUrl.Location = new System.Drawing.Point(13, 40);
            this.textBox_messageServerUrl.Name = "textBox_messageServerUrl";
            this.textBox_messageServerUrl.Size = new System.Drawing.Size(432, 21);
            this.textBox_messageServerUrl.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "dp2MServer URL:";
            // 
            // textBox_groupName
            // 
            this.textBox_groupName.Location = new System.Drawing.Point(89, 67);
            this.textBox_groupName.Name = "textBox_groupName";
            this.textBox_groupName.Size = new System.Drawing.Size(190, 21);
            this.textBox_groupName.TabIndex = 41;
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Location = new System.Drawing.Point(12, 70);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(71, 12);
            this.label38.TabIndex = 40;
            this.label38.Text = "Group Name:";
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(17, 227);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(432, 161);
            this.webBrowser1.TabIndex = 42;
            // 
            // button_sender_stop
            // 
            this.button_sender_stop.Location = new System.Drawing.Point(182, 60);
            this.button_sender_stop.Name = "button_sender_stop";
            this.button_sender_stop.Size = new System.Drawing.Size(75, 23);
            this.button_sender_stop.TabIndex = 12;
            this.button_sender_stop.Text = "停止";
            this.button_sender_stop.UseVisualStyleBackColor = true;
            this.button_sender_stop.Click += new System.EventHandler(this.button_sender_stop_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_clearDisplay});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(465, 25);
            this.toolStrip1.TabIndex = 43;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_clearDisplay
            // 
            this.toolStripButton_clearDisplay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_clearDisplay.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_clearDisplay.Image")));
            this.toolStripButton_clearDisplay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_clearDisplay.Name = "toolStripButton_clearDisplay";
            this.toolStripButton_clearDisplay.Size = new System.Drawing.Size(60, 22);
            this.toolStripButton_clearDisplay.Text = "清除显示";
            this.toolStripButton_clearDisplay.Click += new System.EventHandler(this.toolStripButton_clearDisplay_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(465, 400);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.textBox_groupName);
            this.Controls.Add(this.label38);
            this.Controls.Add(this.textBox_messageServerUrl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_sender.ResumeLayout(false);
            this.tabPage_sender.PerformLayout();
            this.tabPage_reciever.ResumeLayout(false);
            this.tabPage_reciever.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_sender;
        private System.Windows.Forms.TabPage tabPage_reciever;
        private System.Windows.Forms.TextBox textBox_messageServerUrl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_sender_password;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_sender_userName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_reciever_password;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_reciever_userName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_groupName;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.Button button_sender_begin;
        private System.Windows.Forms.Button button_reviever_begin;
        private System.Windows.Forms.Button button_reciever_stop;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button button_sender_stop;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_clearDisplay;
    }
}

