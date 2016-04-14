namespace TestClient1
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_config = new System.Windows.Forms.TabPage();
            this.textBox_config_password = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_config_userName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_config_messageServerUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_getInfo = new System.Windows.Forms.TabPage();
            this.comboBox_getInfo_method = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.textBox_getInfo_formatList = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_getInfo_queryWord = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_getInfo_remoteUserName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage_search = new System.Windows.Forms.TabPage();
            this.textBox_search_position = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.textBox_search_resultSetName = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.comboBox_search_method = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.textBox_search_dbNameList = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.textBox_search_matchStyle = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.textBox_search_use = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.textBox_search_formatList = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox_search_queryWord = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox_search_remoteUserName = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_begin = new System.Windows.Forms.ToolStripButton();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.tabPage_bindPatron = new System.Windows.Forms.TabPage();
            this.textBox_bindPatron_remoteUserName = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_bindPatron_queryWord = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.textBox_bindPatron_style = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.textBox_bindPatron_bindingID = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.textBox_bindPatron_resultTypeList = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.textBox_bindPatron_password = new System.Windows.Forms.TextBox();
            this.label24 = new System.Windows.Forms.Label();
            this.comboBox_bindPatron_action = new System.Windows.Forms.ComboBox();
            this.label25 = new System.Windows.Forms.Label();
            this.tabControl_main.SuspendLayout();
            this.tabPage_config.SuspendLayout();
            this.tabPage_getInfo.SuspendLayout();
            this.tabPage_search.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tabPage_bindPatron.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(728, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 347);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(728, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_config);
            this.tabControl_main.Controls.Add(this.tabPage_getInfo);
            this.tabControl_main.Controls.Add(this.tabPage_search);
            this.tabControl_main.Controls.Add(this.tabPage_bindPatron);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 0);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(341, 298);
            this.tabControl_main.TabIndex = 2;
            // 
            // tabPage_config
            // 
            this.tabPage_config.Controls.Add(this.textBox_config_password);
            this.tabPage_config.Controls.Add(this.label3);
            this.tabPage_config.Controls.Add(this.textBox_config_userName);
            this.tabPage_config.Controls.Add(this.label2);
            this.tabPage_config.Controls.Add(this.textBox_config_messageServerUrl);
            this.tabPage_config.Controls.Add(this.label1);
            this.tabPage_config.Location = new System.Drawing.Point(4, 22);
            this.tabPage_config.Name = "tabPage_config";
            this.tabPage_config.Size = new System.Drawing.Size(246, 272);
            this.tabPage_config.TabIndex = 2;
            this.tabPage_config.Text = "Config";
            this.tabPage_config.UseVisualStyleBackColor = true;
            // 
            // textBox_config_password
            // 
            this.textBox_config_password.Location = new System.Drawing.Point(107, 114);
            this.textBox_config_password.Name = "textBox_config_password";
            this.textBox_config_password.PasswordChar = '*';
            this.textBox_config_password.Size = new System.Drawing.Size(161, 21);
            this.textBox_config_password.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "Password:";
            // 
            // textBox_config_userName
            // 
            this.textBox_config_userName.Location = new System.Drawing.Point(107, 87);
            this.textBox_config_userName.Name = "textBox_config_userName";
            this.textBox_config_userName.Size = new System.Drawing.Size(161, 21);
            this.textBox_config_userName.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "User Name:";
            // 
            // textBox_config_messageServerUrl
            // 
            this.textBox_config_messageServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_config_messageServerUrl.Location = new System.Drawing.Point(11, 36);
            this.textBox_config_messageServerUrl.Name = "textBox_config_messageServerUrl";
            this.textBox_config_messageServerUrl.Size = new System.Drawing.Size(227, 21);
            this.textBox_config_messageServerUrl.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "dp2MServer URL:";
            // 
            // tabPage_getInfo
            // 
            this.tabPage_getInfo.AutoScroll = true;
            this.tabPage_getInfo.Controls.Add(this.comboBox_getInfo_method);
            this.tabPage_getInfo.Controls.Add(this.label15);
            this.tabPage_getInfo.Controls.Add(this.textBox_getInfo_formatList);
            this.tabPage_getInfo.Controls.Add(this.label6);
            this.tabPage_getInfo.Controls.Add(this.textBox_getInfo_queryWord);
            this.tabPage_getInfo.Controls.Add(this.label5);
            this.tabPage_getInfo.Controls.Add(this.textBox_getInfo_remoteUserName);
            this.tabPage_getInfo.Controls.Add(this.label4);
            this.tabPage_getInfo.Location = new System.Drawing.Point(4, 22);
            this.tabPage_getInfo.Name = "tabPage_getInfo";
            this.tabPage_getInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_getInfo.Size = new System.Drawing.Size(246, 272);
            this.tabPage_getInfo.TabIndex = 0;
            this.tabPage_getInfo.Text = "GetXXXInfo";
            this.tabPage_getInfo.UseVisualStyleBackColor = true;
            // 
            // comboBox_getInfo_method
            // 
            this.comboBox_getInfo_method.FormattingEnabled = true;
            this.comboBox_getInfo_method.Items.AddRange(new object[] {
            "getPatronInfo",
            "getBiblioInfo",
            "getItemInfo"});
            this.comboBox_getInfo_method.Location = new System.Drawing.Point(132, 10);
            this.comboBox_getInfo_method.Name = "comboBox_getInfo_method";
            this.comboBox_getInfo_method.Size = new System.Drawing.Size(161, 20);
            this.comboBox_getInfo_method.TabIndex = 1;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(9, 13);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(47, 12);
            this.label15.TabIndex = 0;
            this.label15.Text = "Method:";
            // 
            // textBox_getInfo_formatList
            // 
            this.textBox_getInfo_formatList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_getInfo_formatList.Location = new System.Drawing.Point(132, 91);
            this.textBox_getInfo_formatList.Name = "textBox_getInfo_formatList";
            this.textBox_getInfo_formatList.Size = new System.Drawing.Size(152, 21);
            this.textBox_getInfo_formatList.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 94);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 6;
            this.label6.Text = "Format List:";
            // 
            // textBox_getInfo_queryWord
            // 
            this.textBox_getInfo_queryWord.Location = new System.Drawing.Point(132, 64);
            this.textBox_getInfo_queryWord.Name = "textBox_getInfo_queryWord";
            this.textBox_getInfo_queryWord.Size = new System.Drawing.Size(161, 21);
            this.textBox_getInfo_queryWord.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 67);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "Query Word:";
            // 
            // textBox_getInfo_remoteUserName
            // 
            this.textBox_getInfo_remoteUserName.Location = new System.Drawing.Point(132, 37);
            this.textBox_getInfo_remoteUserName.Name = "textBox_getInfo_remoteUserName";
            this.textBox_getInfo_remoteUserName.Size = new System.Drawing.Size(161, 21);
            this.textBox_getInfo_remoteUserName.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "Remote User Name:";
            // 
            // tabPage_search
            // 
            this.tabPage_search.AutoScroll = true;
            this.tabPage_search.Controls.Add(this.textBox_search_position);
            this.tabPage_search.Controls.Add(this.label18);
            this.tabPage_search.Controls.Add(this.textBox_search_resultSetName);
            this.tabPage_search.Controls.Add(this.label17);
            this.tabPage_search.Controls.Add(this.comboBox_search_method);
            this.tabPage_search.Controls.Add(this.label16);
            this.tabPage_search.Controls.Add(this.textBox_search_dbNameList);
            this.tabPage_search.Controls.Add(this.label14);
            this.tabPage_search.Controls.Add(this.textBox_search_matchStyle);
            this.tabPage_search.Controls.Add(this.label13);
            this.tabPage_search.Controls.Add(this.textBox_search_use);
            this.tabPage_search.Controls.Add(this.label12);
            this.tabPage_search.Controls.Add(this.textBox_search_formatList);
            this.tabPage_search.Controls.Add(this.label9);
            this.tabPage_search.Controls.Add(this.textBox_search_queryWord);
            this.tabPage_search.Controls.Add(this.label10);
            this.tabPage_search.Controls.Add(this.textBox_search_remoteUserName);
            this.tabPage_search.Controls.Add(this.label11);
            this.tabPage_search.Location = new System.Drawing.Point(4, 22);
            this.tabPage_search.Name = "tabPage_search";
            this.tabPage_search.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_search.Size = new System.Drawing.Size(333, 272);
            this.tabPage_search.TabIndex = 1;
            this.tabPage_search.Text = "SearchXXX";
            this.tabPage_search.UseVisualStyleBackColor = true;
            // 
            // textBox_search_position
            // 
            this.textBox_search_position.Location = new System.Drawing.Point(130, 231);
            this.textBox_search_position.Name = "textBox_search_position";
            this.textBox_search_position.Size = new System.Drawing.Size(184, 21);
            this.textBox_search_position.TabIndex = 19;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(8, 234);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(59, 12);
            this.label18.TabIndex = 18;
            this.label18.Text = "Position:";
            // 
            // textBox_search_resultSetName
            // 
            this.textBox_search_resultSetName.Location = new System.Drawing.Point(130, 204);
            this.textBox_search_resultSetName.Name = "textBox_search_resultSetName";
            this.textBox_search_resultSetName.Size = new System.Drawing.Size(161, 21);
            this.textBox_search_resultSetName.TabIndex = 17;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(7, 207);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(95, 12);
            this.label17.TabIndex = 16;
            this.label17.Text = "Resultset Name:";
            // 
            // comboBox_search_method
            // 
            this.comboBox_search_method.FormattingEnabled = true;
            this.comboBox_search_method.Items.AddRange(new object[] {
            "searchPatron",
            "searchBiblio",
            "searchItem"});
            this.comboBox_search_method.Location = new System.Drawing.Point(130, 6);
            this.comboBox_search_method.Name = "comboBox_search_method";
            this.comboBox_search_method.Size = new System.Drawing.Size(161, 20);
            this.comboBox_search_method.TabIndex = 1;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(8, 9);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(47, 12);
            this.label16.TabIndex = 0;
            this.label16.Text = "Method:";
            // 
            // textBox_search_dbNameList
            // 
            this.textBox_search_dbNameList.Location = new System.Drawing.Point(130, 62);
            this.textBox_search_dbNameList.Name = "textBox_search_dbNameList";
            this.textBox_search_dbNameList.Size = new System.Drawing.Size(161, 21);
            this.textBox_search_dbNameList.TabIndex = 5;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(7, 65);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(77, 12);
            this.label14.TabIndex = 4;
            this.label14.Text = "DBName List:";
            // 
            // textBox_search_matchStyle
            // 
            this.textBox_search_matchStyle.Location = new System.Drawing.Point(130, 150);
            this.textBox_search_matchStyle.Name = "textBox_search_matchStyle";
            this.textBox_search_matchStyle.Size = new System.Drawing.Size(161, 21);
            this.textBox_search_matchStyle.TabIndex = 11;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(7, 153);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(77, 12);
            this.label13.TabIndex = 10;
            this.label13.Text = "Match Style:";
            // 
            // textBox_search_use
            // 
            this.textBox_search_use.Location = new System.Drawing.Point(130, 123);
            this.textBox_search_use.Name = "textBox_search_use";
            this.textBox_search_use.Size = new System.Drawing.Size(161, 21);
            this.textBox_search_use.TabIndex = 9;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 126);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(29, 12);
            this.label12.TabIndex = 8;
            this.label12.Text = "Use:";
            // 
            // textBox_search_formatList
            // 
            this.textBox_search_formatList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_search_formatList.Location = new System.Drawing.Point(130, 177);
            this.textBox_search_formatList.Name = "textBox_search_formatList";
            this.textBox_search_formatList.Size = new System.Drawing.Size(213, 21);
            this.textBox_search_formatList.TabIndex = 13;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 180);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(77, 12);
            this.label9.TabIndex = 12;
            this.label9.Text = "Format List:";
            // 
            // textBox_search_queryWord
            // 
            this.textBox_search_queryWord.Location = new System.Drawing.Point(130, 96);
            this.textBox_search_queryWord.Name = "textBox_search_queryWord";
            this.textBox_search_queryWord.Size = new System.Drawing.Size(161, 21);
            this.textBox_search_queryWord.TabIndex = 7;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 99);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(71, 12);
            this.label10.TabIndex = 6;
            this.label10.Text = "Query Word:";
            // 
            // textBox_search_remoteUserName
            // 
            this.textBox_search_remoteUserName.Location = new System.Drawing.Point(130, 35);
            this.textBox_search_remoteUserName.Name = "textBox_search_remoteUserName";
            this.textBox_search_remoteUserName.Size = new System.Drawing.Size(161, 21);
            this.textBox_search_remoteUserName.TabIndex = 3;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(8, 38);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(107, 12);
            this.label11.TabIndex = 2;
            this.label11.Text = "Remote User Name:";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_begin});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(728, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_begin
            // 
            this.toolStripButton_begin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_begin.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_begin.Image")));
            this.toolStripButton_begin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_begin.Name = "toolStripButton_begin";
            this.toolStripButton_begin.Size = new System.Drawing.Size(45, 22);
            this.toolStripButton_begin.Text = "Begin";
            this.toolStripButton_begin.Click += new System.EventHandler(this.toolStripButton_begin_Click);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(0, 49);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tabControl_main);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.webBrowser1);
            this.splitContainer_main.Size = new System.Drawing.Size(728, 298);
            this.splitContainer_main.SplitterDistance = 341;
            this.splitContainer_main.SplitterWidth = 8;
            this.splitContainer_main.TabIndex = 3;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(379, 298);
            this.webBrowser1.TabIndex = 0;
            // 
            // tabPage_bindPatron
            // 
            this.tabPage_bindPatron.Controls.Add(this.comboBox_bindPatron_action);
            this.tabPage_bindPatron.Controls.Add(this.label25);
            this.tabPage_bindPatron.Controls.Add(this.textBox_bindPatron_queryWord);
            this.tabPage_bindPatron.Controls.Add(this.label20);
            this.tabPage_bindPatron.Controls.Add(this.textBox_bindPatron_style);
            this.tabPage_bindPatron.Controls.Add(this.label21);
            this.tabPage_bindPatron.Controls.Add(this.textBox_bindPatron_bindingID);
            this.tabPage_bindPatron.Controls.Add(this.label22);
            this.tabPage_bindPatron.Controls.Add(this.textBox_bindPatron_resultTypeList);
            this.tabPage_bindPatron.Controls.Add(this.label23);
            this.tabPage_bindPatron.Controls.Add(this.textBox_bindPatron_password);
            this.tabPage_bindPatron.Controls.Add(this.label24);
            this.tabPage_bindPatron.Controls.Add(this.textBox_bindPatron_remoteUserName);
            this.tabPage_bindPatron.Controls.Add(this.label7);
            this.tabPage_bindPatron.Location = new System.Drawing.Point(4, 22);
            this.tabPage_bindPatron.Name = "tabPage_bindPatron";
            this.tabPage_bindPatron.Size = new System.Drawing.Size(333, 272);
            this.tabPage_bindPatron.TabIndex = 3;
            this.tabPage_bindPatron.Text = "BindPatron";
            this.tabPage_bindPatron.UseVisualStyleBackColor = true;
            // 
            // textBox_bindPatron_remoteUserName
            // 
            this.textBox_bindPatron_remoteUserName.Location = new System.Drawing.Point(131, 14);
            this.textBox_bindPatron_remoteUserName.Name = "textBox_bindPatron_remoteUserName";
            this.textBox_bindPatron_remoteUserName.Size = new System.Drawing.Size(161, 21);
            this.textBox_bindPatron_remoteUserName.TabIndex = 5;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 17);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(107, 12);
            this.label7.TabIndex = 4;
            this.label7.Text = "Remote User Name:";
            // 
            // textBox_bindPatron_queryWord
            // 
            this.textBox_bindPatron_queryWord.Location = new System.Drawing.Point(131, 70);
            this.textBox_bindPatron_queryWord.Name = "textBox_bindPatron_queryWord";
            this.textBox_bindPatron_queryWord.Size = new System.Drawing.Size(161, 21);
            this.textBox_bindPatron_queryWord.TabIndex = 22;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(8, 73);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(71, 12);
            this.label20.TabIndex = 21;
            this.label20.Text = "Query Word:";
            // 
            // textBox_bindPatron_style
            // 
            this.textBox_bindPatron_style.Location = new System.Drawing.Point(131, 158);
            this.textBox_bindPatron_style.Name = "textBox_bindPatron_style";
            this.textBox_bindPatron_style.Size = new System.Drawing.Size(161, 21);
            this.textBox_bindPatron_style.TabIndex = 28;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(8, 161);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(143, 12);
            this.label21.TabIndex = 27;
            this.label21.Text = "Style(multiple/single):";
            // 
            // textBox_bindPatron_bindingID
            // 
            this.textBox_bindPatron_bindingID.Location = new System.Drawing.Point(131, 131);
            this.textBox_bindPatron_bindingID.Name = "textBox_bindPatron_bindingID";
            this.textBox_bindPatron_bindingID.Size = new System.Drawing.Size(161, 21);
            this.textBox_bindPatron_bindingID.TabIndex = 26;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(8, 134);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(71, 12);
            this.label22.TabIndex = 25;
            this.label22.Text = "Binding ID:";
            // 
            // textBox_bindPatron_resultTypeList
            // 
            this.textBox_bindPatron_resultTypeList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_bindPatron_resultTypeList.Location = new System.Drawing.Point(131, 185);
            this.textBox_bindPatron_resultTypeList.Name = "textBox_bindPatron_resultTypeList";
            this.textBox_bindPatron_resultTypeList.Size = new System.Drawing.Size(213, 21);
            this.textBox_bindPatron_resultTypeList.TabIndex = 30;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(8, 188);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(107, 12);
            this.label23.TabIndex = 29;
            this.label23.Text = "Result Type List:";
            // 
            // textBox_bindPatron_password
            // 
            this.textBox_bindPatron_password.Location = new System.Drawing.Point(131, 104);
            this.textBox_bindPatron_password.Name = "textBox_bindPatron_password";
            this.textBox_bindPatron_password.Size = new System.Drawing.Size(161, 21);
            this.textBox_bindPatron_password.TabIndex = 24;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(8, 107);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(59, 12);
            this.label24.TabIndex = 23;
            this.label24.Text = "Password:";
            // 
            // comboBox_bindPatron_action
            // 
            this.comboBox_bindPatron_action.FormattingEnabled = true;
            this.comboBox_bindPatron_action.Items.AddRange(new object[] {
            "bind",
            "unbind"});
            this.comboBox_bindPatron_action.Location = new System.Drawing.Point(131, 41);
            this.comboBox_bindPatron_action.Name = "comboBox_bindPatron_action";
            this.comboBox_bindPatron_action.Size = new System.Drawing.Size(161, 20);
            this.comboBox_bindPatron_action.TabIndex = 36;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(9, 44);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(47, 12);
            this.label25.TabIndex = 35;
            this.label25.Text = "Action:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(728, 369);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_config.ResumeLayout(false);
            this.tabPage_config.PerformLayout();
            this.tabPage_getInfo.ResumeLayout(false);
            this.tabPage_getInfo.PerformLayout();
            this.tabPage_search.ResumeLayout(false);
            this.tabPage_search.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tabPage_bindPatron.ResumeLayout(false);
            this.tabPage_bindPatron.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_config;
        private System.Windows.Forms.TabPage tabPage_getInfo;
        private System.Windows.Forms.TabPage tabPage_search;
        private System.Windows.Forms.TextBox textBox_config_messageServerUrl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_config_password;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_config_userName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_getInfo_remoteUserName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_getInfo_formatList;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_getInfo_queryWord;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_begin;
        private System.Windows.Forms.TextBox textBox_search_matchStyle;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox textBox_search_use;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox textBox_search_formatList;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox_search_queryWord;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBox_search_remoteUserName;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textBox_search_dbNameList;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox comboBox_getInfo_method;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ComboBox comboBox_search_method;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox textBox_search_resultSetName;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox textBox_search_position;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.TabPage tabPage_bindPatron;
        private System.Windows.Forms.TextBox textBox_bindPatron_remoteUserName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_bindPatron_queryWord;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox textBox_bindPatron_style;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox textBox_bindPatron_bindingID;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox textBox_bindPatron_resultTypeList;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TextBox textBox_bindPatron_password;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.ComboBox comboBox_bindPatron_action;
        private System.Windows.Forms.Label label25;
    }
}

