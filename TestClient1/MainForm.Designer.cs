﻿namespace TestClient1
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
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_writeToMSMQ = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_getSummaryAndItems = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_sendMessage = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_getMessage = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_enumSubject = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_enumCreator = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_config = new System.Windows.Forms.TabPage();
            this.textBox_config_libraryPassword = new System.Windows.Forms.TextBox();
            this.label50 = new System.Windows.Forms.Label();
            this.checkBox_config_isPatron = new System.Windows.Forms.CheckBox();
            this.textBox_config_libraryUserName = new System.Windows.Forms.TextBox();
            this.label49 = new System.Windows.Forms.Label();
            this.textBox_config_password = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_config_userName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_config_messageServerUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_getInfo = new System.Windows.Forms.TabPage();
            this.checkBox_getInfo_getSubEntities = new System.Windows.Forms.CheckBox();
            this.comboBox_getInfo_method = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.textBox_getInfo_formatList = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_getInfo_queryWord = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_getInfo_remoteUserName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage_search = new System.Windows.Forms.TabPage();
            this.textBox_search_filter = new System.Windows.Forms.TextBox();
            this.label51 = new System.Windows.Forms.Label();
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
            this.tabPage_bindPatron = new System.Windows.Forms.TabPage();
            this.comboBox_bindPatron_action = new System.Windows.Forms.ComboBox();
            this.label25 = new System.Windows.Forms.Label();
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
            this.textBox_bindPatron_remoteUserName = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tabPage_setInfo = new System.Windows.Forms.TabPage();
            this.button_editEntities = new System.Windows.Forms.Button();
            this.comboBox_setInfo_action = new System.Windows.Forms.ComboBox();
            this.label28 = new System.Windows.Forms.Label();
            this.webBrowser_setInfo_entities = new System.Windows.Forms.WebBrowser();
            this.comboBox_setInfo_method = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.textBox_setInfo_biblioRecPath = new System.Windows.Forms.TextBox();
            this.label26 = new System.Windows.Forms.Label();
            this.textBox_setInfo_remoteUserName = new System.Windows.Forms.TextBox();
            this.label27 = new System.Windows.Forms.Label();
            this.button_testPaste = new System.Windows.Forms.Button();
            this.tabPage_circulation = new System.Windows.Forms.TabPage();
            this.textBox_circulation_biblioFormatList = new System.Windows.Forms.TextBox();
            this.label36 = new System.Windows.Forms.Label();
            this.comboBox_circulation_operation = new System.Windows.Forms.ComboBox();
            this.label29 = new System.Windows.Forms.Label();
            this.textBox_circulation_patron = new System.Windows.Forms.TextBox();
            this.label30 = new System.Windows.Forms.Label();
            this.textBox_circulation_patronFormatList = new System.Windows.Forms.TextBox();
            this.label31 = new System.Windows.Forms.Label();
            this.textBox_circulation_style = new System.Windows.Forms.TextBox();
            this.label32 = new System.Windows.Forms.Label();
            this.textBox_circulation_itemFormatList = new System.Windows.Forms.TextBox();
            this.label33 = new System.Windows.Forms.Label();
            this.textBox_circulation_item = new System.Windows.Forms.TextBox();
            this.label34 = new System.Windows.Forms.Label();
            this.textBox_circulation_remoteUserName = new System.Windows.Forms.TextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.tabPage_message = new System.Windows.Forms.TabPage();
            this.splitContainer_message = new System.Windows.Forms.SplitContainer();
            this.textBox_message_sortCondition = new System.Windows.Forms.TextBox();
            this.label46 = new System.Windows.Forms.Label();
            this.textBox_message_userRange = new System.Windows.Forms.TextBox();
            this.label41 = new System.Windows.Forms.Label();
            this.button_message_delete = new System.Windows.Forms.Button();
            this.button_message_enumGroupName = new System.Windows.Forms.Button();
            this.button_message_getGroupNameQuick = new System.Windows.Forms.Button();
            this.button_message_transGroupName = new System.Windows.Forms.Button();
            this.textBox_message_timeRange = new System.Windows.Forms.TextBox();
            this.label39 = new System.Windows.Forms.Label();
            this.textBox_message_text = new System.Windows.Forms.TextBox();
            this.textBox_message_groupName = new System.Windows.Forms.TextBox();
            this.label37 = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.button_message_send = new System.Windows.Forms.Button();
            this.button_message_load = new System.Windows.Forms.Button();
            this.webBrowser_message = new System.Windows.Forms.WebBrowser();
            this.tabPage_getRes = new System.Windows.Forms.TabPage();
            this.textBox_getRes_outputFile = new System.Windows.Forms.TextBox();
            this.label40 = new System.Windows.Forms.Label();
            this.comboBox_getRes_operation = new System.Windows.Forms.ComboBox();
            this.label42 = new System.Windows.Forms.Label();
            this.textBox_getRes_remoteUserName = new System.Windows.Forms.TextBox();
            this.label48 = new System.Windows.Forms.Label();
            this.textBox_getRes_path = new System.Windows.Forms.TextBox();
            this.label43 = new System.Windows.Forms.Label();
            this.textBox_getRes_style = new System.Windows.Forms.TextBox();
            this.label44 = new System.Windows.Forms.Label();
            this.textBox_getRes_length = new System.Windows.Forms.TextBox();
            this.label45 = new System.Windows.Forms.Label();
            this.textBox_getRes_start = new System.Windows.Forms.TextBox();
            this.label47 = new System.Windows.Forms.Label();
            this.tabPage_markdown = new System.Windows.Forms.TabPage();
            this.textBox_markdown_source = new System.Windows.Forms.TextBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_begin = new System.Windows.Forms.ToolStripButton();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_config.SuspendLayout();
            this.tabPage_getInfo.SuspendLayout();
            this.tabPage_search.SuspendLayout();
            this.tabPage_bindPatron.SuspendLayout();
            this.tabPage_setInfo.SuspendLayout();
            this.tabPage_circulation.SuspendLayout();
            this.tabPage_message.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_message)).BeginInit();
            this.splitContainer_message.Panel1.SuspendLayout();
            this.splitContainer_message.Panel2.SuspendLayout();
            this.splitContainer_message.SuspendLayout();
            this.tabPage_getRes.SuspendLayout();
            this.tabPage_markdown.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(1831, 60);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_writeToMSMQ,
            this.menuItem_getSummaryAndItems,
            this.ToolStripMenuItem_sendMessage,
            this.ToolStripMenuItem_getMessage,
            this.ToolStripMenuItem_enumSubject,
            this.ToolStripMenuItem_enumCreator});
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(94, 58);
            this.testToolStripMenuItem.Text = "test";
            // 
            // MenuItem_writeToMSMQ
            // 
            this.MenuItem_writeToMSMQ.Name = "MenuItem_writeToMSMQ";
            this.MenuItem_writeToMSMQ.Size = new System.Drawing.Size(504, 54);
            this.MenuItem_writeToMSMQ.Text = "Write to MSMQ";
            this.MenuItem_writeToMSMQ.Click += new System.EventHandler(this.MenuItem_writeToMSMQ_Click);
            // 
            // menuItem_getSummaryAndItems
            // 
            this.menuItem_getSummaryAndItems.Name = "menuItem_getSummaryAndItems";
            this.menuItem_getSummaryAndItems.Size = new System.Drawing.Size(504, 54);
            this.menuItem_getSummaryAndItems.Text = "GetSummaryAndItems";
            this.menuItem_getSummaryAndItems.Click += new System.EventHandler(this.menuItem_getSummaryAndItems_Click);
            // 
            // ToolStripMenuItem_sendMessage
            // 
            this.ToolStripMenuItem_sendMessage.Name = "ToolStripMenuItem_sendMessage";
            this.ToolStripMenuItem_sendMessage.Size = new System.Drawing.Size(504, 54);
            this.ToolStripMenuItem_sendMessage.Text = "Send Message ...";
            this.ToolStripMenuItem_sendMessage.Click += new System.EventHandler(this.ToolStripMenuItem_sendMessage_Click);
            // 
            // ToolStripMenuItem_getMessage
            // 
            this.ToolStripMenuItem_getMessage.Name = "ToolStripMenuItem_getMessage";
            this.ToolStripMenuItem_getMessage.Size = new System.Drawing.Size(504, 54);
            this.ToolStripMenuItem_getMessage.Text = "Get Message ...";
            this.ToolStripMenuItem_getMessage.Click += new System.EventHandler(this.ToolStripMenuItem_getMessage_Click);
            // 
            // ToolStripMenuItem_enumSubject
            // 
            this.ToolStripMenuItem_enumSubject.Name = "ToolStripMenuItem_enumSubject";
            this.ToolStripMenuItem_enumSubject.Size = new System.Drawing.Size(504, 54);
            this.ToolStripMenuItem_enumSubject.Text = "Enum Subject ...";
            this.ToolStripMenuItem_enumSubject.Click += new System.EventHandler(this.ToolStripMenuItem_enumSubject_Click);
            // 
            // ToolStripMenuItem_enumCreator
            // 
            this.ToolStripMenuItem_enumCreator.Name = "ToolStripMenuItem_enumCreator";
            this.ToolStripMenuItem_enumCreator.Size = new System.Drawing.Size(504, 54);
            this.ToolStripMenuItem_enumCreator.Text = "Enum Creator ...";
            this.ToolStripMenuItem_enumCreator.Click += new System.EventHandler(this.ToolStripMenuItem_enumCreator_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 948);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 35, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1831, 52);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(250, 36);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(322, 39);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_config);
            this.tabControl_main.Controls.Add(this.tabPage_getInfo);
            this.tabControl_main.Controls.Add(this.tabPage_search);
            this.tabControl_main.Controls.Add(this.tabPage_bindPatron);
            this.tabControl_main.Controls.Add(this.tabPage_setInfo);
            this.tabControl_main.Controls.Add(this.tabPage_circulation);
            this.tabControl_main.Controls.Add(this.tabPage_message);
            this.tabControl_main.Controls.Add(this.tabPage_getRes);
            this.tabControl_main.Controls.Add(this.tabPage_markdown);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 0);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(976, 826);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_config
            // 
            this.tabPage_config.Controls.Add(this.textBox_config_libraryPassword);
            this.tabPage_config.Controls.Add(this.label50);
            this.tabPage_config.Controls.Add(this.checkBox_config_isPatron);
            this.tabPage_config.Controls.Add(this.textBox_config_libraryUserName);
            this.tabPage_config.Controls.Add(this.label49);
            this.tabPage_config.Controls.Add(this.textBox_config_password);
            this.tabPage_config.Controls.Add(this.label3);
            this.tabPage_config.Controls.Add(this.textBox_config_userName);
            this.tabPage_config.Controls.Add(this.label2);
            this.tabPage_config.Controls.Add(this.textBox_config_messageServerUrl);
            this.tabPage_config.Controls.Add(this.label1);
            this.tabPage_config.Location = new System.Drawing.Point(10, 48);
            this.tabPage_config.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.tabPage_config.Name = "tabPage_config";
            this.tabPage_config.Size = new System.Drawing.Size(956, 768);
            this.tabPage_config.TabIndex = 2;
            this.tabPage_config.Text = "Config";
            this.tabPage_config.UseVisualStyleBackColor = true;
            // 
            // textBox_config_libraryPassword
            // 
            this.textBox_config_libraryPassword.Location = new System.Drawing.Point(370, 526);
            this.textBox_config_libraryPassword.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_config_libraryPassword.Name = "textBox_config_libraryPassword";
            this.textBox_config_libraryPassword.PasswordChar = '*';
            this.textBox_config_libraryPassword.Size = new System.Drawing.Size(395, 42);
            this.textBox_config_libraryPassword.TabIndex = 9;
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Location = new System.Drawing.Point(29, 531);
            this.label50.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(388, 30);
            this.label50.TabIndex = 8;
            this.label50.Text = "Password(空表示代理登录):";
            // 
            // checkBox_config_isPatron
            // 
            this.checkBox_config_isPatron.AutoSize = true;
            this.checkBox_config_isPatron.Location = new System.Drawing.Point(370, 591);
            this.checkBox_config_isPatron.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.checkBox_config_isPatron.Name = "checkBox_config_isPatron";
            this.checkBox_config_isPatron.Size = new System.Drawing.Size(141, 34);
            this.checkBox_config_isPatron.TabIndex = 10;
            this.checkBox_config_isPatron.Text = "Patron";
            this.checkBox_config_isPatron.UseVisualStyleBackColor = true;
            // 
            // textBox_config_libraryUserName
            // 
            this.textBox_config_libraryUserName.Location = new System.Drawing.Point(370, 459);
            this.textBox_config_libraryUserName.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_config_libraryUserName.Name = "textBox_config_libraryUserName";
            this.textBox_config_libraryUserName.Size = new System.Drawing.Size(395, 42);
            this.textBox_config_libraryUserName.TabIndex = 7;
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Location = new System.Drawing.Point(29, 466);
            this.label49.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(328, 30);
            this.label49.TabIndex = 6;
            this.label49.Text = "dp2library User Name:";
            // 
            // textBox_config_password
            // 
            this.textBox_config_password.Location = new System.Drawing.Point(370, 291);
            this.textBox_config_password.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_config_password.Name = "textBox_config_password";
            this.textBox_config_password.PasswordChar = '*';
            this.textBox_config_password.Size = new System.Drawing.Size(395, 42);
            this.textBox_config_password.TabIndex = 5;
            this.textBox_config_password.TextChanged += new System.EventHandler(this.textBox_config_messageServerUrl_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 300);
            this.label3.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(148, 30);
            this.label3.TabIndex = 4;
            this.label3.Text = "Password:";
            // 
            // textBox_config_userName
            // 
            this.textBox_config_userName.Location = new System.Drawing.Point(370, 219);
            this.textBox_config_userName.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_config_userName.Name = "textBox_config_userName";
            this.textBox_config_userName.Size = new System.Drawing.Size(395, 42);
            this.textBox_config_userName.TabIndex = 3;
            this.textBox_config_userName.TextChanged += new System.EventHandler(this.textBox_config_messageServerUrl_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 226);
            this.label2.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(163, 30);
            this.label2.TabIndex = 2;
            this.label2.Text = "User Name:";
            // 
            // textBox_config_messageServerUrl
            // 
            this.textBox_config_messageServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_config_messageServerUrl.Location = new System.Drawing.Point(29, 90);
            this.textBox_config_messageServerUrl.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_config_messageServerUrl.Name = "textBox_config_messageServerUrl";
            this.textBox_config_messageServerUrl.Size = new System.Drawing.Size(869, 42);
            this.textBox_config_messageServerUrl.TabIndex = 1;
            this.textBox_config_messageServerUrl.TextChanged += new System.EventHandler(this.textBox_config_messageServerUrl_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 50);
            this.label1.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(238, 30);
            this.label1.TabIndex = 0;
            this.label1.Text = "dp2MServer URL:";
            // 
            // tabPage_getInfo
            // 
            this.tabPage_getInfo.AutoScroll = true;
            this.tabPage_getInfo.Controls.Add(this.checkBox_getInfo_getSubEntities);
            this.tabPage_getInfo.Controls.Add(this.comboBox_getInfo_method);
            this.tabPage_getInfo.Controls.Add(this.label15);
            this.tabPage_getInfo.Controls.Add(this.textBox_getInfo_formatList);
            this.tabPage_getInfo.Controls.Add(this.label6);
            this.tabPage_getInfo.Controls.Add(this.textBox_getInfo_queryWord);
            this.tabPage_getInfo.Controls.Add(this.label5);
            this.tabPage_getInfo.Controls.Add(this.textBox_getInfo_remoteUserName);
            this.tabPage_getInfo.Controls.Add(this.label4);
            this.tabPage_getInfo.Location = new System.Drawing.Point(10, 48);
            this.tabPage_getInfo.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.tabPage_getInfo.Name = "tabPage_getInfo";
            this.tabPage_getInfo.Padding = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.tabPage_getInfo.Size = new System.Drawing.Size(956, 783);
            this.tabPage_getInfo.TabIndex = 0;
            this.tabPage_getInfo.Text = "GetXXXInfo";
            this.tabPage_getInfo.UseVisualStyleBackColor = true;
            // 
            // checkBox_getInfo_getSubEntities
            // 
            this.checkBox_getInfo_getSubEntities.AutoSize = true;
            this.checkBox_getInfo_getSubEntities.Location = new System.Drawing.Point(29, 399);
            this.checkBox_getInfo_getSubEntities.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.checkBox_getInfo_getSubEntities.Name = "checkBox_getInfo_getSubEntities";
            this.checkBox_getInfo_getSubEntities.Size = new System.Drawing.Size(291, 34);
            this.checkBox_getInfo_getSubEntities.TabIndex = 8;
            this.checkBox_getInfo_getSubEntities.Text = "Get Sub Entities";
            this.checkBox_getInfo_getSubEntities.UseVisualStyleBackColor = true;
            // 
            // comboBox_getInfo_method
            // 
            this.comboBox_getInfo_method.FormattingEnabled = true;
            this.comboBox_getInfo_method.Items.AddRange(new object[] {
            "getPatronInfo",
            "getBiblioInfo",
            "getItemInfo",
            "getBrowseRecords",
            "getUserInfo",
            "getSystemParameter"});
            this.comboBox_getInfo_method.Location = new System.Drawing.Point(330, 26);
            this.comboBox_getInfo_method.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.comboBox_getInfo_method.Name = "comboBox_getInfo_method";
            this.comboBox_getInfo_method.Size = new System.Drawing.Size(395, 38);
            this.comboBox_getInfo_method.TabIndex = 1;
            this.comboBox_getInfo_method.SelectedIndexChanged += new System.EventHandler(this.comboBox_getInfo_method_SelectedIndexChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(22, 31);
            this.label15.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(118, 30);
            this.label15.TabIndex = 0;
            this.label15.Text = "Method:";
            // 
            // textBox_getInfo_formatList
            // 
            this.textBox_getInfo_formatList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_getInfo_formatList.Location = new System.Drawing.Point(330, 229);
            this.textBox_getInfo_formatList.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_getInfo_formatList.Name = "textBox_getInfo_formatList";
            this.textBox_getInfo_formatList.Size = new System.Drawing.Size(680, 42);
            this.textBox_getInfo_formatList.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(22, 234);
            this.label6.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(193, 30);
            this.label6.TabIndex = 6;
            this.label6.Text = "Format List:";
            // 
            // textBox_getInfo_queryWord
            // 
            this.textBox_getInfo_queryWord.Location = new System.Drawing.Point(330, 160);
            this.textBox_getInfo_queryWord.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_getInfo_queryWord.Name = "textBox_getInfo_queryWord";
            this.textBox_getInfo_queryWord.Size = new System.Drawing.Size(395, 42);
            this.textBox_getInfo_queryWord.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 169);
            this.label5.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(178, 30);
            this.label5.TabIndex = 4;
            this.label5.Text = "Query Word:";
            // 
            // textBox_getInfo_remoteUserName
            // 
            this.textBox_getInfo_remoteUserName.Location = new System.Drawing.Point(330, 91);
            this.textBox_getInfo_remoteUserName.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_getInfo_remoteUserName.Name = "textBox_getInfo_remoteUserName";
            this.textBox_getInfo_remoteUserName.Size = new System.Drawing.Size(395, 42);
            this.textBox_getInfo_remoteUserName.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 100);
            this.label4.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(268, 30);
            this.label4.TabIndex = 2;
            this.label4.Text = "Remote User Name:";
            // 
            // tabPage_search
            // 
            this.tabPage_search.AutoScroll = true;
            this.tabPage_search.Controls.Add(this.textBox_search_filter);
            this.tabPage_search.Controls.Add(this.label51);
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
            this.tabPage_search.Location = new System.Drawing.Point(10, 48);
            this.tabPage_search.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.tabPage_search.Name = "tabPage_search";
            this.tabPage_search.Padding = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.tabPage_search.Size = new System.Drawing.Size(956, 783);
            this.tabPage_search.TabIndex = 1;
            this.tabPage_search.Text = "SearchXXX";
            this.tabPage_search.UseVisualStyleBackColor = true;
            // 
            // textBox_search_filter
            // 
            this.textBox_search_filter.Location = new System.Drawing.Point(325, 646);
            this.textBox_search_filter.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_search_filter.Name = "textBox_search_filter";
            this.textBox_search_filter.Size = new System.Drawing.Size(454, 42);
            this.textBox_search_filter.TabIndex = 21;
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Location = new System.Drawing.Point(20, 651);
            this.label51.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(118, 30);
            this.label51.TabIndex = 20;
            this.label51.Text = "Filter:";
            // 
            // textBox_search_position
            // 
            this.textBox_search_position.Location = new System.Drawing.Point(325, 579);
            this.textBox_search_position.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_search_position.Name = "textBox_search_position";
            this.textBox_search_position.Size = new System.Drawing.Size(454, 42);
            this.textBox_search_position.TabIndex = 19;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(20, 586);
            this.label18.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(148, 30);
            this.label18.TabIndex = 18;
            this.label18.Text = "Position:";
            // 
            // textBox_search_resultSetName
            // 
            this.textBox_search_resultSetName.Location = new System.Drawing.Point(325, 510);
            this.textBox_search_resultSetName.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_search_resultSetName.Name = "textBox_search_resultSetName";
            this.textBox_search_resultSetName.Size = new System.Drawing.Size(395, 42);
            this.textBox_search_resultSetName.TabIndex = 17;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(18, 519);
            this.label17.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(238, 30);
            this.label17.TabIndex = 16;
            this.label17.Text = "Resultset Name:";
            // 
            // comboBox_search_method
            // 
            this.comboBox_search_method.FormattingEnabled = true;
            this.comboBox_search_method.Items.AddRange(new object[] {
            "searchPatron",
            "searchBiblio",
            "searchItem",
            "searchIssue",
            "searchOrder",
            "searchComment",
            "getBiblioInfo",
            "getBiblioSummary",
            "getItemInfo",
            "getItemInfoEx",
            "getBrowseRecords",
            "getUserInfo",
            "GetConnectionInfo",
            "clearConnection"});
            this.comboBox_search_method.Location = new System.Drawing.Point(325, 14);
            this.comboBox_search_method.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.comboBox_search_method.Name = "comboBox_search_method";
            this.comboBox_search_method.Size = new System.Drawing.Size(395, 38);
            this.comboBox_search_method.TabIndex = 1;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(20, 21);
            this.label16.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(118, 30);
            this.label16.TabIndex = 0;
            this.label16.Text = "Method:";
            // 
            // textBox_search_dbNameList
            // 
            this.textBox_search_dbNameList.Location = new System.Drawing.Point(325, 154);
            this.textBox_search_dbNameList.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_search_dbNameList.Name = "textBox_search_dbNameList";
            this.textBox_search_dbNameList.Size = new System.Drawing.Size(395, 42);
            this.textBox_search_dbNameList.TabIndex = 5;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(18, 161);
            this.label14.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(193, 30);
            this.label14.TabIndex = 4;
            this.label14.Text = "DBName List:";
            // 
            // textBox_search_matchStyle
            // 
            this.textBox_search_matchStyle.Location = new System.Drawing.Point(325, 374);
            this.textBox_search_matchStyle.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_search_matchStyle.Name = "textBox_search_matchStyle";
            this.textBox_search_matchStyle.Size = new System.Drawing.Size(395, 42);
            this.textBox_search_matchStyle.TabIndex = 11;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(18, 381);
            this.label13.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(193, 30);
            this.label13.TabIndex = 10;
            this.label13.Text = "Match Style:";
            // 
            // textBox_search_use
            // 
            this.textBox_search_use.Location = new System.Drawing.Point(325, 309);
            this.textBox_search_use.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_search_use.Name = "textBox_search_use";
            this.textBox_search_use.Size = new System.Drawing.Size(395, 42);
            this.textBox_search_use.TabIndex = 9;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(18, 314);
            this.label12.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(73, 30);
            this.label12.TabIndex = 8;
            this.label12.Text = "Use:";
            // 
            // textBox_search_formatList
            // 
            this.textBox_search_formatList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_search_formatList.Location = new System.Drawing.Point(325, 441);
            this.textBox_search_formatList.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_search_formatList.Name = "textBox_search_formatList";
            this.textBox_search_formatList.Size = new System.Drawing.Size(552, 42);
            this.textBox_search_formatList.TabIndex = 13;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(18, 450);
            this.label9.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(193, 30);
            this.label9.TabIndex = 12;
            this.label9.Text = "Format List:";
            // 
            // textBox_search_queryWord
            // 
            this.textBox_search_queryWord.Location = new System.Drawing.Point(325, 240);
            this.textBox_search_queryWord.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_search_queryWord.Name = "textBox_search_queryWord";
            this.textBox_search_queryWord.Size = new System.Drawing.Size(395, 42);
            this.textBox_search_queryWord.TabIndex = 7;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(18, 249);
            this.label10.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(178, 30);
            this.label10.TabIndex = 6;
            this.label10.Text = "Query Word:";
            // 
            // textBox_search_remoteUserName
            // 
            this.textBox_search_remoteUserName.Location = new System.Drawing.Point(325, 89);
            this.textBox_search_remoteUserName.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_search_remoteUserName.Name = "textBox_search_remoteUserName";
            this.textBox_search_remoteUserName.Size = new System.Drawing.Size(395, 42);
            this.textBox_search_remoteUserName.TabIndex = 3;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(20, 94);
            this.label11.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(268, 30);
            this.label11.TabIndex = 2;
            this.label11.Text = "Remote User Name:";
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
            this.tabPage_bindPatron.Location = new System.Drawing.Point(10, 48);
            this.tabPage_bindPatron.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.tabPage_bindPatron.Name = "tabPage_bindPatron";
            this.tabPage_bindPatron.Size = new System.Drawing.Size(956, 783);
            this.tabPage_bindPatron.TabIndex = 3;
            this.tabPage_bindPatron.Text = "BindPatron";
            this.tabPage_bindPatron.UseVisualStyleBackColor = true;
            // 
            // comboBox_bindPatron_action
            // 
            this.comboBox_bindPatron_action.FormattingEnabled = true;
            this.comboBox_bindPatron_action.Items.AddRange(new object[] {
            "bind",
            "unbind"});
            this.comboBox_bindPatron_action.Location = new System.Drawing.Point(329, 101);
            this.comboBox_bindPatron_action.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.comboBox_bindPatron_action.Name = "comboBox_bindPatron_action";
            this.comboBox_bindPatron_action.Size = new System.Drawing.Size(395, 38);
            this.comboBox_bindPatron_action.TabIndex = 36;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(22, 110);
            this.label25.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(118, 30);
            this.label25.TabIndex = 35;
            this.label25.Text = "Action:";
            // 
            // textBox_bindPatron_queryWord
            // 
            this.textBox_bindPatron_queryWord.Location = new System.Drawing.Point(329, 174);
            this.textBox_bindPatron_queryWord.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_bindPatron_queryWord.Name = "textBox_bindPatron_queryWord";
            this.textBox_bindPatron_queryWord.Size = new System.Drawing.Size(395, 42);
            this.textBox_bindPatron_queryWord.TabIndex = 22;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(20, 181);
            this.label20.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(178, 30);
            this.label20.TabIndex = 21;
            this.label20.Text = "Query Word:";
            // 
            // textBox_bindPatron_style
            // 
            this.textBox_bindPatron_style.Location = new System.Drawing.Point(329, 394);
            this.textBox_bindPatron_style.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_bindPatron_style.Name = "textBox_bindPatron_style";
            this.textBox_bindPatron_style.Size = new System.Drawing.Size(395, 42);
            this.textBox_bindPatron_style.TabIndex = 28;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(20, 401);
            this.label21.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(358, 30);
            this.label21.TabIndex = 27;
            this.label21.Text = "Style(multiple/single):";
            // 
            // textBox_bindPatron_bindingID
            // 
            this.textBox_bindPatron_bindingID.Location = new System.Drawing.Point(329, 329);
            this.textBox_bindPatron_bindingID.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_bindPatron_bindingID.Name = "textBox_bindPatron_bindingID";
            this.textBox_bindPatron_bindingID.Size = new System.Drawing.Size(395, 42);
            this.textBox_bindPatron_bindingID.TabIndex = 26;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(20, 334);
            this.label22.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(178, 30);
            this.label22.TabIndex = 25;
            this.label22.Text = "Binding ID:";
            // 
            // textBox_bindPatron_resultTypeList
            // 
            this.textBox_bindPatron_resultTypeList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_bindPatron_resultTypeList.Location = new System.Drawing.Point(329, 461);
            this.textBox_bindPatron_resultTypeList.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_bindPatron_resultTypeList.Name = "textBox_bindPatron_resultTypeList";
            this.textBox_bindPatron_resultTypeList.Size = new System.Drawing.Size(616, 42);
            this.textBox_bindPatron_resultTypeList.TabIndex = 30;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(20, 470);
            this.label23.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(268, 30);
            this.label23.TabIndex = 29;
            this.label23.Text = "Result Type List:";
            // 
            // textBox_bindPatron_password
            // 
            this.textBox_bindPatron_password.Location = new System.Drawing.Point(329, 260);
            this.textBox_bindPatron_password.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_bindPatron_password.Name = "textBox_bindPatron_password";
            this.textBox_bindPatron_password.Size = new System.Drawing.Size(395, 42);
            this.textBox_bindPatron_password.TabIndex = 24;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(20, 269);
            this.label24.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(148, 30);
            this.label24.TabIndex = 23;
            this.label24.Text = "Password:";
            // 
            // textBox_bindPatron_remoteUserName
            // 
            this.textBox_bindPatron_remoteUserName.Location = new System.Drawing.Point(329, 34);
            this.textBox_bindPatron_remoteUserName.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_bindPatron_remoteUserName.Name = "textBox_bindPatron_remoteUserName";
            this.textBox_bindPatron_remoteUserName.Size = new System.Drawing.Size(395, 42);
            this.textBox_bindPatron_remoteUserName.TabIndex = 5;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(22, 41);
            this.label7.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(268, 30);
            this.label7.TabIndex = 4;
            this.label7.Text = "Remote User Name:";
            // 
            // tabPage_setInfo
            // 
            this.tabPage_setInfo.AutoScroll = true;
            this.tabPage_setInfo.Controls.Add(this.button_editEntities);
            this.tabPage_setInfo.Controls.Add(this.comboBox_setInfo_action);
            this.tabPage_setInfo.Controls.Add(this.label28);
            this.tabPage_setInfo.Controls.Add(this.webBrowser_setInfo_entities);
            this.tabPage_setInfo.Controls.Add(this.comboBox_setInfo_method);
            this.tabPage_setInfo.Controls.Add(this.label8);
            this.tabPage_setInfo.Controls.Add(this.label19);
            this.tabPage_setInfo.Controls.Add(this.textBox_setInfo_biblioRecPath);
            this.tabPage_setInfo.Controls.Add(this.label26);
            this.tabPage_setInfo.Controls.Add(this.textBox_setInfo_remoteUserName);
            this.tabPage_setInfo.Controls.Add(this.label27);
            this.tabPage_setInfo.Controls.Add(this.button_testPaste);
            this.tabPage_setInfo.Location = new System.Drawing.Point(10, 48);
            this.tabPage_setInfo.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.tabPage_setInfo.Name = "tabPage_setInfo";
            this.tabPage_setInfo.Size = new System.Drawing.Size(956, 783);
            this.tabPage_setInfo.TabIndex = 4;
            this.tabPage_setInfo.Text = "SetXXXInfo";
            this.tabPage_setInfo.UseVisualStyleBackColor = true;
            // 
            // button_editEntities
            // 
            this.button_editEntities.Location = new System.Drawing.Point(625, 291);
            this.button_editEntities.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.button_editEntities.Name = "button_editEntities";
            this.button_editEntities.Size = new System.Drawing.Size(325, 59);
            this.button_editEntities.TabIndex = 39;
            this.button_editEntities.Text = "edit entities ...";
            this.button_editEntities.UseVisualStyleBackColor = true;
            this.button_editEntities.Click += new System.EventHandler(this.button_editEntities_Click);
            // 
            // comboBox_setInfo_action
            // 
            this.comboBox_setInfo_action.FormattingEnabled = true;
            this.comboBox_setInfo_action.Items.AddRange(new object[] {
            "new",
            "delete",
            "change"});
            this.comboBox_setInfo_action.Location = new System.Drawing.Point(325, 166);
            this.comboBox_setInfo_action.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.comboBox_setInfo_action.Name = "comboBox_setInfo_action";
            this.comboBox_setInfo_action.Size = new System.Drawing.Size(395, 38);
            this.comboBox_setInfo_action.TabIndex = 38;
            this.comboBox_setInfo_action.Visible = false;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(20, 171);
            this.label28.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(118, 30);
            this.label28.TabIndex = 37;
            this.label28.Text = "Action:";
            this.label28.Visible = false;
            // 
            // webBrowser_setInfo_entities
            // 
            this.webBrowser_setInfo_entities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser_setInfo_entities.Location = new System.Drawing.Point(22, 390);
            this.webBrowser_setInfo_entities.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.webBrowser_setInfo_entities.MinimumSize = new System.Drawing.Size(50, 250);
            this.webBrowser_setInfo_entities.Name = "webBrowser_setInfo_entities";
            this.webBrowser_setInfo_entities.Size = new System.Drawing.Size(1451, 304);
            this.webBrowser_setInfo_entities.TabIndex = 15;
            // 
            // comboBox_setInfo_method
            // 
            this.comboBox_setInfo_method.FormattingEnabled = true;
            this.comboBox_setInfo_method.Items.AddRange(new object[] {
            "setItemInfo",
            "setOrderInfo",
            "setIssueInfo",
            "setCommentInfo",
            "setReaderInfo",
            "setBiblioInfo"});
            this.comboBox_setInfo_method.Location = new System.Drawing.Point(325, 31);
            this.comboBox_setInfo_method.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.comboBox_setInfo_method.Name = "comboBox_setInfo_method";
            this.comboBox_setInfo_method.Size = new System.Drawing.Size(395, 38);
            this.comboBox_setInfo_method.TabIndex = 9;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(18, 40);
            this.label8.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(118, 30);
            this.label8.TabIndex = 8;
            this.label8.Text = "Method:";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(18, 306);
            this.label19.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(148, 30);
            this.label19.TabIndex = 14;
            this.label19.Text = "Entities:";
            // 
            // textBox_setInfo_biblioRecPath
            // 
            this.textBox_setInfo_biblioRecPath.Location = new System.Drawing.Point(325, 230);
            this.textBox_setInfo_biblioRecPath.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_setInfo_biblioRecPath.Name = "textBox_setInfo_biblioRecPath";
            this.textBox_setInfo_biblioRecPath.Size = new System.Drawing.Size(395, 42);
            this.textBox_setInfo_biblioRecPath.TabIndex = 13;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(18, 239);
            this.label26.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(238, 30);
            this.label26.TabIndex = 12;
            this.label26.Text = "Biblio RecPath:";
            // 
            // textBox_setInfo_remoteUserName
            // 
            this.textBox_setInfo_remoteUserName.Location = new System.Drawing.Point(325, 100);
            this.textBox_setInfo_remoteUserName.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_setInfo_remoteUserName.Name = "textBox_setInfo_remoteUserName";
            this.textBox_setInfo_remoteUserName.Size = new System.Drawing.Size(395, 42);
            this.textBox_setInfo_remoteUserName.TabIndex = 11;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(18, 109);
            this.label27.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(268, 30);
            this.label27.TabIndex = 10;
            this.label27.Text = "Remote User Name:";
            // 
            // button_testPaste
            // 
            this.button_testPaste.Location = new System.Drawing.Point(325, 291);
            this.button_testPaste.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.button_testPaste.Name = "button_testPaste";
            this.button_testPaste.Size = new System.Drawing.Size(285, 59);
            this.button_testPaste.TabIndex = 0;
            this.button_testPaste.Text = "paste entities";
            this.button_testPaste.UseVisualStyleBackColor = true;
            this.button_testPaste.Click += new System.EventHandler(this.button_pasteEntities_Click);
            // 
            // tabPage_circulation
            // 
            this.tabPage_circulation.Controls.Add(this.textBox_circulation_biblioFormatList);
            this.tabPage_circulation.Controls.Add(this.label36);
            this.tabPage_circulation.Controls.Add(this.comboBox_circulation_operation);
            this.tabPage_circulation.Controls.Add(this.label29);
            this.tabPage_circulation.Controls.Add(this.textBox_circulation_patron);
            this.tabPage_circulation.Controls.Add(this.label30);
            this.tabPage_circulation.Controls.Add(this.textBox_circulation_patronFormatList);
            this.tabPage_circulation.Controls.Add(this.label31);
            this.tabPage_circulation.Controls.Add(this.textBox_circulation_style);
            this.tabPage_circulation.Controls.Add(this.label32);
            this.tabPage_circulation.Controls.Add(this.textBox_circulation_itemFormatList);
            this.tabPage_circulation.Controls.Add(this.label33);
            this.tabPage_circulation.Controls.Add(this.textBox_circulation_item);
            this.tabPage_circulation.Controls.Add(this.label34);
            this.tabPage_circulation.Controls.Add(this.textBox_circulation_remoteUserName);
            this.tabPage_circulation.Controls.Add(this.label35);
            this.tabPage_circulation.Location = new System.Drawing.Point(10, 48);
            this.tabPage_circulation.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.tabPage_circulation.Name = "tabPage_circulation";
            this.tabPage_circulation.Size = new System.Drawing.Size(956, 783);
            this.tabPage_circulation.TabIndex = 5;
            this.tabPage_circulation.Text = "Circulation";
            this.tabPage_circulation.UseVisualStyleBackColor = true;
            // 
            // textBox_circulation_biblioFormatList
            // 
            this.textBox_circulation_biblioFormatList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_circulation_biblioFormatList.Location = new System.Drawing.Point(325, 529);
            this.textBox_circulation_biblioFormatList.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_circulation_biblioFormatList.Name = "textBox_circulation_biblioFormatList";
            this.textBox_circulation_biblioFormatList.Size = new System.Drawing.Size(584, 42);
            this.textBox_circulation_biblioFormatList.TabIndex = 52;
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(18, 534);
            this.label36.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(298, 30);
            this.label36.TabIndex = 51;
            this.label36.Text = "Biblio Format List:";
            // 
            // comboBox_circulation_operation
            // 
            this.comboBox_circulation_operation.FormattingEnabled = true;
            this.comboBox_circulation_operation.Items.AddRange(new object[] {
            "borrow",
            "renew",
            "return",
            "lost",
            "read",
            "reservation",
            "resetPassword",
            "changePassword",
            "verifyPassword"});
            this.comboBox_circulation_operation.Location = new System.Drawing.Point(325, 100);
            this.comboBox_circulation_operation.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.comboBox_circulation_operation.Name = "comboBox_circulation_operation";
            this.comboBox_circulation_operation.Size = new System.Drawing.Size(395, 38);
            this.comboBox_circulation_operation.TabIndex = 50;
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(20, 109);
            this.label29.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(163, 30);
            this.label29.TabIndex = 49;
            this.label29.Text = "Operation:";
            // 
            // textBox_circulation_patron
            // 
            this.textBox_circulation_patron.Location = new System.Drawing.Point(325, 171);
            this.textBox_circulation_patron.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_circulation_patron.Name = "textBox_circulation_patron";
            this.textBox_circulation_patron.Size = new System.Drawing.Size(395, 42);
            this.textBox_circulation_patron.TabIndex = 40;
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(18, 180);
            this.label30.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(118, 30);
            this.label30.TabIndex = 39;
            this.label30.Text = "Patron:";
            // 
            // textBox_circulation_patronFormatList
            // 
            this.textBox_circulation_patronFormatList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_circulation_patronFormatList.Location = new System.Drawing.Point(325, 391);
            this.textBox_circulation_patronFormatList.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_circulation_patronFormatList.Name = "textBox_circulation_patronFormatList";
            this.textBox_circulation_patronFormatList.Size = new System.Drawing.Size(584, 42);
            this.textBox_circulation_patronFormatList.TabIndex = 46;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(18, 400);
            this.label31.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(298, 30);
            this.label31.TabIndex = 45;
            this.label31.Text = "Patron Format List:";
            // 
            // textBox_circulation_style
            // 
            this.textBox_circulation_style.Location = new System.Drawing.Point(325, 326);
            this.textBox_circulation_style.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_circulation_style.Name = "textBox_circulation_style";
            this.textBox_circulation_style.Size = new System.Drawing.Size(395, 42);
            this.textBox_circulation_style.TabIndex = 44;
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(18, 331);
            this.label32.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(103, 30);
            this.label32.TabIndex = 43;
            this.label32.Text = "Style:";
            // 
            // textBox_circulation_itemFormatList
            // 
            this.textBox_circulation_itemFormatList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_circulation_itemFormatList.Location = new System.Drawing.Point(325, 460);
            this.textBox_circulation_itemFormatList.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_circulation_itemFormatList.Name = "textBox_circulation_itemFormatList";
            this.textBox_circulation_itemFormatList.Size = new System.Drawing.Size(584, 42);
            this.textBox_circulation_itemFormatList.TabIndex = 48;
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(18, 469);
            this.label33.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(268, 30);
            this.label33.TabIndex = 47;
            this.label33.Text = "Item Format List:";
            // 
            // textBox_circulation_item
            // 
            this.textBox_circulation_item.Location = new System.Drawing.Point(325, 259);
            this.textBox_circulation_item.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_circulation_item.Name = "textBox_circulation_item";
            this.textBox_circulation_item.Size = new System.Drawing.Size(395, 42);
            this.textBox_circulation_item.TabIndex = 42;
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(18, 266);
            this.label34.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(88, 30);
            this.label34.TabIndex = 41;
            this.label34.Text = "Item:";
            // 
            // textBox_circulation_remoteUserName
            // 
            this.textBox_circulation_remoteUserName.Location = new System.Drawing.Point(325, 31);
            this.textBox_circulation_remoteUserName.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_circulation_remoteUserName.Name = "textBox_circulation_remoteUserName";
            this.textBox_circulation_remoteUserName.Size = new System.Drawing.Size(395, 42);
            this.textBox_circulation_remoteUserName.TabIndex = 38;
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(20, 40);
            this.label35.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(268, 30);
            this.label35.TabIndex = 37;
            this.label35.Text = "Remote User Name:";
            // 
            // tabPage_message
            // 
            this.tabPage_message.Controls.Add(this.splitContainer_message);
            this.tabPage_message.Location = new System.Drawing.Point(10, 48);
            this.tabPage_message.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.tabPage_message.Name = "tabPage_message";
            this.tabPage_message.Size = new System.Drawing.Size(956, 783);
            this.tabPage_message.TabIndex = 6;
            this.tabPage_message.Text = "Message";
            this.tabPage_message.UseVisualStyleBackColor = true;
            // 
            // splitContainer_message
            // 
            this.splitContainer_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_message.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_message.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.splitContainer_message.Name = "splitContainer_message";
            this.splitContainer_message.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_message.Panel1
            // 
            this.splitContainer_message.Panel1.AutoScroll = true;
            this.splitContainer_message.Panel1.Controls.Add(this.textBox_message_sortCondition);
            this.splitContainer_message.Panel1.Controls.Add(this.label46);
            this.splitContainer_message.Panel1.Controls.Add(this.textBox_message_userRange);
            this.splitContainer_message.Panel1.Controls.Add(this.label41);
            this.splitContainer_message.Panel1.Controls.Add(this.button_message_delete);
            this.splitContainer_message.Panel1.Controls.Add(this.button_message_enumGroupName);
            this.splitContainer_message.Panel1.Controls.Add(this.button_message_getGroupNameQuick);
            this.splitContainer_message.Panel1.Controls.Add(this.button_message_transGroupName);
            this.splitContainer_message.Panel1.Controls.Add(this.textBox_message_timeRange);
            this.splitContainer_message.Panel1.Controls.Add(this.label39);
            this.splitContainer_message.Panel1.Controls.Add(this.textBox_message_text);
            this.splitContainer_message.Panel1.Controls.Add(this.textBox_message_groupName);
            this.splitContainer_message.Panel1.Controls.Add(this.label37);
            this.splitContainer_message.Panel1.Controls.Add(this.label38);
            this.splitContainer_message.Panel1.Controls.Add(this.button_message_send);
            this.splitContainer_message.Panel1.Controls.Add(this.button_message_load);
            // 
            // splitContainer_message.Panel2
            // 
            this.splitContainer_message.Panel2.Controls.Add(this.webBrowser_message);
            this.splitContainer_message.Size = new System.Drawing.Size(956, 783);
            this.splitContainer_message.SplitterDistance = 611;
            this.splitContainer_message.SplitterWidth = 20;
            this.splitContainer_message.TabIndex = 40;
            // 
            // textBox_message_sortCondition
            // 
            this.textBox_message_sortCondition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message_sortCondition.Location = new System.Drawing.Point(192, 229);
            this.textBox_message_sortCondition.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_message_sortCondition.Name = "textBox_message_sortCondition";
            this.textBox_message_sortCondition.Size = new System.Drawing.Size(1877, 42);
            this.textBox_message_sortCondition.TabIndex = 49;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Location = new System.Drawing.Point(0, 234);
            this.label46.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(88, 30);
            this.label46.TabIndex = 48;
            this.label46.Text = "Sort:";
            // 
            // textBox_message_userRange
            // 
            this.textBox_message_userRange.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message_userRange.Location = new System.Drawing.Point(192, 94);
            this.textBox_message_userRange.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_message_userRange.Name = "textBox_message_userRange";
            this.textBox_message_userRange.Size = new System.Drawing.Size(1877, 42);
            this.textBox_message_userRange.TabIndex = 47;
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Location = new System.Drawing.Point(0, 94);
            this.label41.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(178, 30);
            this.label41.TabIndex = 46;
            this.label41.Text = "User Range:";
            // 
            // button_message_delete
            // 
            this.button_message_delete.Location = new System.Drawing.Point(828, 461);
            this.button_message_delete.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.button_message_delete.Name = "button_message_delete";
            this.button_message_delete.Size = new System.Drawing.Size(140, 59);
            this.button_message_delete.TabIndex = 45;
            this.button_message_delete.Text = "delete";
            this.button_message_delete.UseVisualStyleBackColor = true;
            this.button_message_delete.Click += new System.EventHandler(this.button_message_delete_Click);
            // 
            // button_message_enumGroupName
            // 
            this.button_message_enumGroupName.Location = new System.Drawing.Point(708, 461);
            this.button_message_enumGroupName.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.button_message_enumGroupName.Name = "button_message_enumGroupName";
            this.button_message_enumGroupName.Size = new System.Drawing.Size(119, 59);
            this.button_message_enumGroupName.TabIndex = 44;
            this.button_message_enumGroupName.Text = "enum";
            this.button_message_enumGroupName.UseVisualStyleBackColor = true;
            this.button_message_enumGroupName.Click += new System.EventHandler(this.button_message_enumGroupName_Click);
            // 
            // button_message_getGroupNameQuick
            // 
            this.button_message_getGroupNameQuick.Location = new System.Drawing.Point(312, 461);
            this.button_message_getGroupNameQuick.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.button_message_getGroupNameQuick.Name = "button_message_getGroupNameQuick";
            this.button_message_getGroupNameQuick.Size = new System.Drawing.Size(389, 59);
            this.button_message_getGroupNameQuick.TabIndex = 43;
            this.button_message_getGroupNameQuick.Text = "Get Group Name Quick";
            this.button_message_getGroupNameQuick.UseVisualStyleBackColor = true;
            this.button_message_getGroupNameQuick.Click += new System.EventHandler(this.button_message_getGroupNameQuick_Click);
            // 
            // button_message_transGroupName
            // 
            this.button_message_transGroupName.Location = new System.Drawing.Point(0, 461);
            this.button_message_transGroupName.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.button_message_transGroupName.Name = "button_message_transGroupName";
            this.button_message_transGroupName.Size = new System.Drawing.Size(299, 59);
            this.button_message_transGroupName.TabIndex = 42;
            this.button_message_transGroupName.Text = "Get Group Name";
            this.button_message_transGroupName.UseVisualStyleBackColor = true;
            this.button_message_transGroupName.Click += new System.EventHandler(this.button_message_transGroupName_Click);
            // 
            // textBox_message_timeRange
            // 
            this.textBox_message_timeRange.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message_timeRange.Location = new System.Drawing.Point(192, 161);
            this.textBox_message_timeRange.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_message_timeRange.Name = "textBox_message_timeRange";
            this.textBox_message_timeRange.Size = new System.Drawing.Size(1877, 42);
            this.textBox_message_timeRange.TabIndex = 41;
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Location = new System.Drawing.Point(0, 170);
            this.label39.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(178, 30);
            this.label39.TabIndex = 40;
            this.label39.Text = "Time Range:";
            // 
            // textBox_message_text
            // 
            this.textBox_message_text.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message_text.Location = new System.Drawing.Point(135, 294);
            this.textBox_message_text.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_message_text.Multiline = true;
            this.textBox_message_text.Name = "textBox_message_text";
            this.textBox_message_text.Size = new System.Drawing.Size(1934, 150);
            this.textBox_message_text.TabIndex = 3;
            // 
            // textBox_message_groupName
            // 
            this.textBox_message_groupName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message_groupName.Location = new System.Drawing.Point(192, 29);
            this.textBox_message_groupName.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_message_groupName.Name = "textBox_message_groupName";
            this.textBox_message_groupName.Size = new System.Drawing.Size(1877, 42);
            this.textBox_message_groupName.TabIndex = 39;
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(0, 294);
            this.label37.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(88, 30);
            this.label37.TabIndex = 2;
            this.label37.Text = "Text:";
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Location = new System.Drawing.Point(0, 39);
            this.label38.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(178, 30);
            this.label38.TabIndex = 6;
            this.label38.Text = "Group Name:";
            // 
            // button_message_send
            // 
            this.button_message_send.Location = new System.Drawing.Point(0, 390);
            this.button_message_send.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.button_message_send.Name = "button_message_send";
            this.button_message_send.Size = new System.Drawing.Size(119, 59);
            this.button_message_send.TabIndex = 4;
            this.button_message_send.Text = "Send";
            this.button_message_send.UseVisualStyleBackColor = true;
            this.button_message_send.Click += new System.EventHandler(this.button_message_send_Click);
            // 
            // button_message_load
            // 
            this.button_message_load.Location = new System.Drawing.Point(0, 331);
            this.button_message_load.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.button_message_load.Name = "button_message_load";
            this.button_message_load.Size = new System.Drawing.Size(119, 59);
            this.button_message_load.TabIndex = 5;
            this.button_message_load.Text = "Load";
            this.button_message_load.UseVisualStyleBackColor = true;
            this.button_message_load.Click += new System.EventHandler(this.button_message_load_Click);
            // 
            // webBrowser_message
            // 
            this.webBrowser_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_message.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_message.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.webBrowser_message.MinimumSize = new System.Drawing.Size(50, 50);
            this.webBrowser_message.Name = "webBrowser_message";
            this.webBrowser_message.Size = new System.Drawing.Size(956, 152);
            this.webBrowser_message.TabIndex = 1;
            // 
            // tabPage_getRes
            // 
            this.tabPage_getRes.Controls.Add(this.textBox_getRes_outputFile);
            this.tabPage_getRes.Controls.Add(this.label40);
            this.tabPage_getRes.Controls.Add(this.comboBox_getRes_operation);
            this.tabPage_getRes.Controls.Add(this.label42);
            this.tabPage_getRes.Controls.Add(this.textBox_getRes_remoteUserName);
            this.tabPage_getRes.Controls.Add(this.label48);
            this.tabPage_getRes.Controls.Add(this.textBox_getRes_path);
            this.tabPage_getRes.Controls.Add(this.label43);
            this.tabPage_getRes.Controls.Add(this.textBox_getRes_style);
            this.tabPage_getRes.Controls.Add(this.label44);
            this.tabPage_getRes.Controls.Add(this.textBox_getRes_length);
            this.tabPage_getRes.Controls.Add(this.label45);
            this.tabPage_getRes.Controls.Add(this.textBox_getRes_start);
            this.tabPage_getRes.Controls.Add(this.label47);
            this.tabPage_getRes.Location = new System.Drawing.Point(10, 48);
            this.tabPage_getRes.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.tabPage_getRes.Name = "tabPage_getRes";
            this.tabPage_getRes.Size = new System.Drawing.Size(956, 783);
            this.tabPage_getRes.TabIndex = 7;
            this.tabPage_getRes.Text = "GetRes";
            this.tabPage_getRes.UseVisualStyleBackColor = true;
            // 
            // textBox_getRes_outputFile
            // 
            this.textBox_getRes_outputFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_getRes_outputFile.Location = new System.Drawing.Point(325, 486);
            this.textBox_getRes_outputFile.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_getRes_outputFile.Name = "textBox_getRes_outputFile";
            this.textBox_getRes_outputFile.Size = new System.Drawing.Size(584, 42);
            this.textBox_getRes_outputFile.TabIndex = 56;
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Location = new System.Drawing.Point(18, 491);
            this.label40.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(193, 30);
            this.label40.TabIndex = 55;
            this.label40.Text = "Output File:";
            // 
            // comboBox_getRes_operation
            // 
            this.comboBox_getRes_operation.FormattingEnabled = true;
            this.comboBox_getRes_operation.Items.AddRange(new object[] {
            "getRes"});
            this.comboBox_getRes_operation.Location = new System.Drawing.Point(325, 100);
            this.comboBox_getRes_operation.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.comboBox_getRes_operation.Name = "comboBox_getRes_operation";
            this.comboBox_getRes_operation.Size = new System.Drawing.Size(395, 38);
            this.comboBox_getRes_operation.TabIndex = 54;
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Location = new System.Drawing.Point(20, 109);
            this.label42.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(163, 30);
            this.label42.TabIndex = 53;
            this.label42.Text = "Operation:";
            // 
            // textBox_getRes_remoteUserName
            // 
            this.textBox_getRes_remoteUserName.Location = new System.Drawing.Point(325, 31);
            this.textBox_getRes_remoteUserName.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_getRes_remoteUserName.Name = "textBox_getRes_remoteUserName";
            this.textBox_getRes_remoteUserName.Size = new System.Drawing.Size(395, 42);
            this.textBox_getRes_remoteUserName.TabIndex = 52;
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(20, 40);
            this.label48.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(268, 30);
            this.label48.TabIndex = 51;
            this.label48.Text = "Remote User Name:";
            // 
            // textBox_getRes_path
            // 
            this.textBox_getRes_path.Location = new System.Drawing.Point(325, 166);
            this.textBox_getRes_path.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_getRes_path.Name = "textBox_getRes_path";
            this.textBox_getRes_path.Size = new System.Drawing.Size(395, 42);
            this.textBox_getRes_path.TabIndex = 25;
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(18, 171);
            this.label43.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(88, 30);
            this.label43.TabIndex = 24;
            this.label43.Text = "Path:";
            // 
            // textBox_getRes_style
            // 
            this.textBox_getRes_style.Location = new System.Drawing.Point(325, 386);
            this.textBox_getRes_style.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_getRes_style.Name = "textBox_getRes_style";
            this.textBox_getRes_style.Size = new System.Drawing.Size(395, 42);
            this.textBox_getRes_style.TabIndex = 31;
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Location = new System.Drawing.Point(18, 391);
            this.label44.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(103, 30);
            this.label44.TabIndex = 30;
            this.label44.Text = "Style:";
            // 
            // textBox_getRes_length
            // 
            this.textBox_getRes_length.Location = new System.Drawing.Point(325, 319);
            this.textBox_getRes_length.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_getRes_length.Name = "textBox_getRes_length";
            this.textBox_getRes_length.Size = new System.Drawing.Size(395, 42);
            this.textBox_getRes_length.TabIndex = 29;
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Location = new System.Drawing.Point(18, 326);
            this.label45.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(118, 30);
            this.label45.TabIndex = 28;
            this.label45.Text = "Length:";
            // 
            // textBox_getRes_start
            // 
            this.textBox_getRes_start.Location = new System.Drawing.Point(325, 250);
            this.textBox_getRes_start.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_getRes_start.Name = "textBox_getRes_start";
            this.textBox_getRes_start.Size = new System.Drawing.Size(395, 42);
            this.textBox_getRes_start.TabIndex = 27;
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Location = new System.Drawing.Point(18, 259);
            this.label47.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(103, 30);
            this.label47.TabIndex = 26;
            this.label47.Text = "Start:";
            // 
            // tabPage_markdown
            // 
            this.tabPage_markdown.Controls.Add(this.textBox_markdown_source);
            this.tabPage_markdown.Location = new System.Drawing.Point(10, 48);
            this.tabPage_markdown.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.tabPage_markdown.Name = "tabPage_markdown";
            this.tabPage_markdown.Size = new System.Drawing.Size(956, 783);
            this.tabPage_markdown.TabIndex = 8;
            this.tabPage_markdown.Text = "MarkDown";
            this.tabPage_markdown.UseVisualStyleBackColor = true;
            // 
            // textBox_markdown_source
            // 
            this.textBox_markdown_source.AcceptsReturn = true;
            this.textBox_markdown_source.AcceptsTab = true;
            this.textBox_markdown_source.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_markdown_source.Location = new System.Drawing.Point(22, 41);
            this.textBox_markdown_source.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_markdown_source.Multiline = true;
            this.textBox_markdown_source.Name = "textBox_markdown_source";
            this.textBox_markdown_source.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_markdown_source.Size = new System.Drawing.Size(863, 630);
            this.textBox_markdown_source.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_begin});
            this.toolStrip1.Location = new System.Drawing.Point(0, 60);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.toolStrip1.Size = new System.Drawing.Size(1831, 62);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_begin
            // 
            this.toolStripButton_begin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_begin.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_begin.Image")));
            this.toolStripButton_begin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_begin.Name = "toolStripButton_begin";
            this.toolStripButton_begin.Size = new System.Drawing.Size(102, 55);
            this.toolStripButton_begin.Text = "Begin";
            this.toolStripButton_begin.Click += new System.EventHandler(this.toolStripButton_begin_Click);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(0, 122);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tabControl_main);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.webBrowser1);
            this.splitContainer_main.Size = new System.Drawing.Size(1831, 826);
            this.splitContainer_main.SplitterDistance = 976;
            this.splitContainer_main.SplitterWidth = 20;
            this.splitContainer_main.TabIndex = 3;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(50, 50);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(835, 826);
            this.webBrowser1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1831, 1000);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_config.ResumeLayout(false);
            this.tabPage_config.PerformLayout();
            this.tabPage_getInfo.ResumeLayout(false);
            this.tabPage_getInfo.PerformLayout();
            this.tabPage_search.ResumeLayout(false);
            this.tabPage_search.PerformLayout();
            this.tabPage_bindPatron.ResumeLayout(false);
            this.tabPage_bindPatron.PerformLayout();
            this.tabPage_setInfo.ResumeLayout(false);
            this.tabPage_setInfo.PerformLayout();
            this.tabPage_circulation.ResumeLayout(false);
            this.tabPage_circulation.PerformLayout();
            this.tabPage_message.ResumeLayout(false);
            this.splitContainer_message.Panel1.ResumeLayout(false);
            this.splitContainer_message.Panel1.PerformLayout();
            this.splitContainer_message.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_message)).EndInit();
            this.splitContainer_message.ResumeLayout(false);
            this.tabPage_getRes.ResumeLayout(false);
            this.tabPage_getRes.PerformLayout();
            this.tabPage_markdown.ResumeLayout(false);
            this.tabPage_markdown.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
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
        private System.Windows.Forms.TabPage tabPage_setInfo;
        private System.Windows.Forms.Button button_testPaste;
        private System.Windows.Forms.ComboBox comboBox_setInfo_method;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox textBox_setInfo_biblioRecPath;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.TextBox textBox_setInfo_remoteUserName;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.WebBrowser webBrowser_setInfo_entities;
        private System.Windows.Forms.ComboBox comboBox_setInfo_action;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.TabPage tabPage_circulation;
        private System.Windows.Forms.ComboBox comboBox_circulation_operation;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.TextBox textBox_circulation_patron;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.TextBox textBox_circulation_patronFormatList;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.TextBox textBox_circulation_style;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.TextBox textBox_circulation_itemFormatList;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.TextBox textBox_circulation_item;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.TextBox textBox_circulation_remoteUserName;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.TextBox textBox_circulation_biblioFormatList;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.CheckBox checkBox_getInfo_getSubEntities;
        private System.Windows.Forms.TabPage tabPage_message;
        private System.Windows.Forms.WebBrowser webBrowser_message;
        private System.Windows.Forms.TextBox textBox_message_text;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.Button button_message_send;
        private System.Windows.Forms.Button button_message_load;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_writeToMSMQ;
        private System.Windows.Forms.TextBox textBox_message_groupName;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.SplitContainer splitContainer_message;
        private System.Windows.Forms.TextBox textBox_message_timeRange;
        private System.Windows.Forms.Label label39;
        private System.Windows.Forms.Button button_message_transGroupName;
        private System.Windows.Forms.Button button_message_getGroupNameQuick;
        private System.Windows.Forms.Button button_message_enumGroupName;
        private System.Windows.Forms.Button button_message_delete;
        private System.Windows.Forms.ToolStripMenuItem menuItem_getSummaryAndItems;
        private System.Windows.Forms.TabPage tabPage_getRes;
        private System.Windows.Forms.TextBox textBox_getRes_path;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.TextBox textBox_getRes_style;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.TextBox textBox_getRes_length;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.TextBox textBox_getRes_start;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.ComboBox comboBox_getRes_operation;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.TextBox textBox_getRes_remoteUserName;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.TextBox textBox_getRes_outputFile;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TextBox textBox_message_userRange;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.TextBox textBox_message_sortCondition;
        private System.Windows.Forms.Label label46;
        private System.Windows.Forms.TabPage tabPage_markdown;
        private System.Windows.Forms.TextBox textBox_markdown_source;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_sendMessage;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_getMessage;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_enumSubject;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_enumCreator;
        private System.Windows.Forms.TextBox textBox_config_libraryUserName;
        private System.Windows.Forms.Label label49;
        private System.Windows.Forms.CheckBox checkBox_config_isPatron;
        private System.Windows.Forms.TextBox textBox_config_libraryPassword;
        private System.Windows.Forms.Label label50;
        private System.Windows.Forms.Button button_editEntities;
        private System.Windows.Forms.TextBox textBox_search_filter;
        private System.Windows.Forms.Label label51;
    }
}

