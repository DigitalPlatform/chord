using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2Capo.Install
{
    public partial class Z3950SettingDialog : Form
    {
        // capo.xml
        public XmlDocument CfgDom { get; set; }

        public Z3950SettingDialog()
        {
            InitializeComponent();

            this.tabControl_main.TabPages.Remove(this.tabPage_z3950);
        }

        public static string GetDisplayText(XmlDocument CfgDom)
        {
            StringBuilder text = new StringBuilder();
            XmlDocument dom = CfgDom;

            XmlElement root = dom.DocumentElement.SelectSingleNode("zServer") as XmlElement;
            if (root == null)
                return "";

            // 概括 databases
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("zServer/databases/database");
            text.Append("databaseCount=" + nodes.Count + "\r\n");

            XmlElement element = dom.DocumentElement.SelectSingleNode("zServer") as XmlElement;
            if (element != null)
            {
                // text.Append("url=" + element.GetAttribute("url") + "\r\n");
                text.Append("anonymousUserName=" + element.GetAttribute("anonymousUserName") + "\r\n");
            }
            return text.ToString();
        }

        private void button_detectManageUser_Click(object sender, EventArgs e)
        {
            EnableControls(false);

            try
            {
                string strError = "";

                if (this.LibraryWsUrl == "")
                {
                    MessageBox.Show(this, "尚未输入 dp2Library 服务器的 URL");
                    return;
                }

                if (this.UserName == "")
                {
                    MessageBox.Show(this, "尚未指定 dp2Library 管理用户名。");
                    return;
                }

                /*
                if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
                {
                    strError = "dp2Library 管理用户 密码 和 再次输入密码 不一致。请重新输入。";
                    MessageBox.Show(this, strError);
                    return;
                }*/

                // 检测帐户登录是否成功?


                // 进行登录
                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                int nRet = DoLogin(
                    this.comboBox_librarywsUrl.Text,
                    this.textBox_manageUserName.Text,
                    this.textBox_managePassword.Text,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "检测 dp2library 帐户时发生错误: " + strError);
                    return;
                }
                if (nRet == 0)
                {
                    MessageBox.Show(this, "您指定的 dp2library 帐户 不正确: " + strError);
                    return;
                }

                MessageBox.Show(this, "您指定的 dp2library 帐户 正确");
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 按住 Control 键可以越过检测 dp2library server 的部分
        private void button_OK_Click(object sender, EventArgs e)
        {
            // 按下 Control 键可越过探测步骤
            bool bControl = Control.ModifierKeys == Keys.Control;

            string strError = "";

            if (SaveToCfgDom() == false)
                return;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
#if NO
            ERROR1:
            this.Invoke(new Action(() =>
            {
                MessageBox.Show(this, strError);
            }));
#endif
        }

        // 进行登录
        // return:
        //      -1  error
        //      0   登录未成功
        //      1   登录成功
        static int DoLogin(
            string strLibraryWsUrl,
            string strUserName,
            string strPassword,
            out string strError)
        {
            strError = "";

            using (LibraryChannel Channel = new LibraryChannel())
            {

                Channel.Url = strLibraryWsUrl;

                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                long lRet = Channel.Login(strUserName,
                    strPassword,
                    "location=z39.50 server,type=worker,client=dp2ZServer|0.01",
                    /*
                    "z39.50 server",    // string strLocation,
                    false,  // bReader,
                     * */
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
        }

        void EnableControls(bool bEnable)
        {
            this.button_OK.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;
            this.button_detectManageUser.Enabled = bEnable;

            this.textBox_anonymousUserName.Enabled = bEnable;
            this.textBox_anonymousPassword.Enabled = bEnable;
            this.button_detectAnonymousUser.Enabled = bEnable;

            this.textBox_databaseDef.Enabled = bEnable;
            this.button_import_databaseDef.Enabled = bEnable;

            this.numericUpDown_z3950_port.Enabled = bEnable;
            this.textBox_z3950_maxResultCount.Enabled = bEnable;
            this.textBox_z3950_maxSessions.Enabled = bEnable;

            this.Update();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string LibraryWsUrl
        {
            get
            {
                return this.comboBox_librarywsUrl.Text;
            }
            set
            {
                this.comboBox_librarywsUrl.Text = value;
            }
        }

        public string UserName
        {
            get
            {
                return this.textBox_manageUserName.Text;
            }
            set
            {
                this.textBox_manageUserName.Text = value;
            }
        }

        public string Password
        {
            get
            {
                return this.textBox_managePassword.Text;
            }
            set
            {
                this.textBox_managePassword.Text = value;
                // this.textBox_confirmManagePassword.Text = value;
            }
        }

        public string AnonymousUserName
        {
            get
            {
                return this.textBox_anonymousUserName.Text;
            }
            set
            {
                this.textBox_anonymousUserName.Text = value;
            }
        }

        public string AnonymousPassword
        {
            get
            {
                return this.textBox_anonymousPassword.Text;
            }
            set
            {
                this.textBox_anonymousPassword.Text = value;
            }
        }

        private void button_detectAnonymousUser_Click(object sender, EventArgs e)
        {
            EnableControls(false);

            try
            {
                string strError = "";

                if (this.LibraryWsUrl == "")
                {
                    MessageBox.Show(this, "尚未输入 dp2Library 服务器的 URL");
                    return;
                }

                if (this.AnonymousUserName == "")
                {
                    MessageBox.Show(this, "尚未指定 匿名登录用户名。");
                    return;
                }


                // 检测帐户登录是否成功?


                // 进行登录
                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                int nRet = DoLogin(
                    this.comboBox_librarywsUrl.Text,
                    this.textBox_anonymousUserName.Text,
                    this.textBox_anonymousPassword.Text,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "检测 匿名登录 用户时发生错误: " + strError);
                    return;
                }
                if (nRet == 0)
                {
                    MessageBox.Show(this, "您指定的 匿名登录 用户 不正确: " + strError);
                    return;
                }

                MessageBox.Show(this, "您指定的 匿名登录 用户 正确");
            }
            finally
            {
                EnableControls(true);
            }
        }

        private void button_import_databaseDef_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strXml = "";

            this.EnableControls(false);
            try
            {
                if (string.IsNullOrEmpty(this.comboBox_librarywsUrl.Text))
                {
                    strError = "尚未输入 dp2Library 服务器的 URL";
                    goto ERROR1;
                }

                if (string.IsNullOrEmpty(this.textBox_manageUserName.Text))
                {
                    strError = "尚未指定 dp2Library 管理用户名";
                    goto ERROR1;
                }

                // this.textBox_databaseDef.Text = "";

                int nRet = GetDatabaseDef(
        this.comboBox_librarywsUrl.Text,
        this.textBox_manageUserName.Text,
        this.textBox_managePassword.Text,
        out strXml,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strOutputXml = "";
                nRet = BuildZDatabaseDef(strXml,
                    out strOutputXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                {
                    this.DatabasesXml = "";
                    MessageBox.Show(this, "dp2library 中尚未定义 OPAC 检索数据库，或没有为任何数据库定义别名。请先利用内务系统管理窗“OPAC”属性页进行配置，再使用本功能");
                }
                else
                    this.DatabasesXml = strOutputXml;
                return;
            }
            finally
            {
                this.EnableControls(true);
            }
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得 dp2library <virtualDatabases> 数据库定义
        // return:
        //      -1  error
        //      0   成功
        static int GetDatabaseDef(
            string strLibraryWsUrl,
            string strUserName,
            string strPassword,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            using (LibraryChannel Channel = new LibraryChannel())
            {
                Channel.Url = strLibraryWsUrl;

                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                long lRet = Channel.Login(strUserName,
                    strPassword,
                    "location=z39.50 server,type=worker,client=dp2ZServer|0.01",
                    out strError);
                if (lRet == -1)
                    return -1;

                if (lRet == 0)
                {
                    strError = "登录未成功:" + strError;
                    return -1;
                }

                lRet = Channel.GetSystemParameter(
    "opac",
    "databases",
    out strXml,
    out strError);
                if (lRet == -1)
                    return -1;

                return 0;
            }
        }

        /*
  <databases>
    <database name="中文图书" alias="cbook">
      <use value="4" from="题名" />
      <use value="7" from="ISBN" />
      <use value="8" from="ISSN" />
      <use value="21" from="主题词" />
      <use value="1003" from="责任者" />
    </database>
    <database name="英文图书" alias="ebook">
      <use value="4" from="题名" />
      <use value="7" from="ISBN" />
      <use value="8" from="ISSN" />
      <use value="21" from="主题词" />
      <use value="1003" from="责任者" />
    </database>
  </databases>
         * */
        // 根据 library.xml 中的 <virtualDatabases> 元素构造 dp2zserver.xml 中的 <databases>
        // return:
        //      -1  出错
        //      0   strXml 中没有发现有意义的信息
        //      1   构造成功
        int BuildZDatabaseDef(string strXml,
            out string strOutputXml,
            out string strError)
        {
            strError = "";
            strOutputXml = "";

            XmlDocument source_dom = new XmlDocument();
            source_dom.LoadXml("<root />");
            try
            {
                source_dom.DocumentElement.InnerXml = strXml;
            }
            catch (Exception ex)
            {
                strError = "输入的 XML 字符串格式错误: " + ex.Message;
                return -1;
            }

            XmlDocument target_dom = new XmlDocument();
            target_dom.LoadXml("<databases />");

            int createCount = 0;
            XmlNodeList databases = source_dom.DocumentElement.SelectNodes("database");
            foreach (XmlElement database in databases)
            {
                string name = database.GetAttribute("name");
                string alias = database.GetAttribute("alias");

                // 没有别名的数据库不会用在 Z39.50 检索中
                if (string.IsNullOrEmpty(alias))
                    continue;

                XmlElement new_database = target_dom.CreateElement("database");
                target_dom.DocumentElement.AppendChild(new_database);
                new_database.SetAttribute("name", name);
                new_database.SetAttribute("alias", alias);

                createCount++;

                // 翻译 use
                string from_name = FindFromByStyle(database, "title");
                if (string.IsNullOrEmpty(from_name) == false)
                    CreateUseElement(new_database, "4", from_name);

                from_name = FindFromByStyle(database, "isbn");
                if (string.IsNullOrEmpty(from_name) == false)
                    CreateUseElement(new_database, "7", from_name);

                from_name = FindFromByStyle(database, "issn");
                if (string.IsNullOrEmpty(from_name) == false)
                    CreateUseElement(new_database, "8", from_name);

                from_name = FindFromByStyle(database, "subject");
                if (string.IsNullOrEmpty(from_name) == false)
                    CreateUseElement(new_database, "21", from_name);

                from_name = FindFromByStyle(database, "contributor");
                if (string.IsNullOrEmpty(from_name) == false)
                    CreateUseElement(new_database, "1003", from_name);
            }

            if (createCount == 0)
                return 0;

            strOutputXml = target_dom.DocumentElement.OuterXml;
            return 1;
        }

        /*
      <use value="4" from="题名" />
         * */
        static void CreateUseElement(XmlElement database, string number, string from)
        {
            XmlElement element = database.OwnerDocument.CreateElement("use");
            database.AppendChild(element);
            element.SetAttribute("value", number);
            element.SetAttribute("from", from);
        }

        /*
        <database name="中文图书" alias="cbook">
            <caption lang="zh">中文图书</caption>
            <from name="ISBN" style="isbn">
                <caption lang="zh-CN">ISBN</caption>
                <caption lang="en">ISBN</caption>
            </from>
            <from name="ISSN" style="issn">
                <caption lang="zh-CN">ISSN</caption>
                <caption lang="en">ISSN</caption>
            </from>
            <from name="题名" style="title">
                <caption lang="zh-CN">题名</caption>
                <caption lang="en">Title</caption>
            </from>
            <from name="题名拼音" style="pinyin_title">
                <caption lang="zh-CN">题名拼音</caption>
                <caption lang="en">Title pinyin</caption>
            </from>
            <from name="主题词" style="subject">
                <caption lang="zh-CN">主题词</caption>
                <caption lang="en">Thesaurus</caption>
            </from>
            <from name="中图法分类号" style="clc,__class">
                <caption lang="zh-CN">中图法分类号</caption>
                <caption lang="en">CLC Class number</caption>
            </from>
            <from name="责任者" style="contributor">
                <caption lang="zh-CN">责任者</caption>
                <caption lang="en">Contributor</caption>
            </from>
            <from name="责任者拼音" style="pinyin_contributor">
                <caption lang="zh-CN">责任者拼音</caption>
                <caption lang="en">Contributor pinyin</caption>
            </from>
            <from name="出版发行者" style="publisher">
                <caption lang="zh-CN">出版发行者</caption>
                <caption lang="en">Publisher</caption>
            </from>
            <from name="出版时间" style="publishtime,_time,_freetime">
                <caption lang="zh-CN">出版时间</caption>
                <caption lang="en">Publish Time</caption>
            </from>
            <from name="批次号" style="batchno">
                <caption lang="zh-CN">批次号</caption>
                <caption lang="en">Batch number</caption>
            </from>
            <from name="目标记录路径" style="targetrecpath">
                <caption lang="zh-CN">目标记录路径</caption>
                <caption lang="en">Target Record Path</caption>
            </from>
            <from name="状态" style="state">
                <caption lang="zh-CN">状态</caption>
                <caption lang="en">State</caption>
            </from>
            <from name="操作时间" style="opertime,_time,_utime">
                <caption lang="zh-CN">操作时间</caption>
                <caption lang="en">OperTime</caption>
            </from>
            <from name="其它标识号" style="identifier">
                <caption lang="zh-CN">其它标识号</caption>
                <caption lang="en">Identifier</caption>
            </from>
            <from name="__id" style="recid" />
        </database>
         * */
        static string FindFromByStyle(XmlElement database, string strStyle)
        {
            XmlNodeList froms = database.SelectNodes("from");
            foreach (XmlElement from in froms)
            {
                string name = from.GetAttribute("name");
                string style = from.GetAttribute("style");

                if (string.IsNullOrEmpty(style))
                    continue;

                if (StringUtil.IsInList(strStyle, style) == true)
                    return name;
            }

            return null;    // not found
        }

        // <databases> 元素 OuterXml
        public string DatabasesXml
        {
            get
            {
                return this.textBox_databaseDef.Text;
            }
            set
            {
                this.textBox_databaseDef.Text = DomUtil.GetIndentXml(value);
            }
        }

        public int Port
        {
            get
            {
                return Convert.ToInt32(this.numericUpDown_z3950_port.Value);
            }
            set
            {
                this.numericUpDown_z3950_port.Value = value;
            }
        }

        public string MaxSessions
        {
            get
            {
                return this.textBox_z3950_maxSessions.Text;
            }
            set
            {
                this.textBox_z3950_maxSessions.Text = value;
            }
        }

        public string MaxResultCount
        {
            get
            {
                return this.textBox_z3950_maxResultCount.Text;
            }
            set
            {
                this.textBox_z3950_maxResultCount.Text = value;
            }
        }

        void FillInfo()
        {
            XmlDocument dom = this.CfgDom;

            {
                XmlElement databases = dom.DocumentElement.SelectSingleNode("zServer/databases") as XmlElement;
                if (databases == null)
                    DatabasesXml = "";
                else
                    DatabasesXml = databases.OuterXml;
            }

            {
                // dp2library 服务器参数
                XmlElement node = dom.DocumentElement.SelectSingleNode("dp2library") as XmlElement;

                // 万一已经存在的文件是不正确的?
                if (node == null)
                {
                    //strError = "配置文件中缺乏 libraryserver 元素";
                    //return -1;
                    this.UserName = "";
                    this.Password = "";

                    this.LibraryWsUrl = "";
                }
                else
                {
                    Debug.Assert(node != null, "");

                    string strUserName = node.GetAttribute("userName");
                    string strPassword = node.GetAttribute("password");
                    strPassword = dp2LibraryDialog.DecryptPasssword(strPassword);

                    string strUrl = node.GetAttribute("url");

                    this.UserName = strUserName;
                    this.Password = strPassword;

                    if (String.IsNullOrEmpty(strUrl) == false)
                        this.LibraryWsUrl = strUrl;

#if NO
                XmlElement databases = dom.DocumentElement.SelectSingleNode("databases") as XmlElement;
                if (databases != null)
                {
                    dlg.DatabasesXml = databases.OuterXml;
                    dlg.MaxResultCount = databases.GetAttribute("maxResultCount");
                    if (string.IsNullOrEmpty(dlg.MaxResultCount))
                        dlg.MaxResultCount = "-1";
                }

                XmlElement network = dom.DocumentElement.SelectSingleNode("network") as XmlElement;
                if (network != null)
                {
                    string strPort = network.GetAttribute("port");
                    int port = 210;
                    if (string.IsNullOrEmpty(strPort) == false)
                        Int32.TryParse(strPort, out port);
                    dlg.Port = port;

                    dlg.MaxSessions = network.GetAttribute("maxSessions");
                    if (string.IsNullOrEmpty(dlg.MaxSessions))
                        dlg.MaxSessions = "-1";
                }
#endif
                }
            }

            {
                // zServer 服务器参数
                XmlElement node = dom.DocumentElement.SelectSingleNode("zServer/dp2library") as XmlElement;
                if (node == null)
                {
                    this.AnonymousUserName = "";
                    this.AnonymousPassword = "";
                }
                else
                {
                    Debug.Assert(node != null, "");

                    string strAnonymousUserName = node.GetAttribute("anonymousUserName");
                    string strAnonymousPassword = node.GetAttribute("anonymousPassword");
                    strAnonymousPassword = DecryptPasssword(strAnonymousPassword);

                    this.AnonymousUserName = strAnonymousUserName;
                    this.AnonymousPassword = strAnonymousPassword;
                }
            }

            XmlElement root = dom.DocumentElement.SelectSingleNode("zServer") as XmlElement;
            if (root == null)
                this.checkBox_enableZ3950.Checked = false;
            else
                this.checkBox_enableZ3950.Checked = true;

            SetEnableZ3950UiState();
        }

        void SetEnableZ3950UiState()
        {
            if (this.checkBox_enableZ3950.Checked)
                this.tabControl_main.Enabled = true;
            else
                this.tabControl_main.Enabled = false;
        }

        // 从控件到 CfgDom
        bool SaveToCfgDom()
        {
            XmlDocument dom = this.CfgDom;

            XmlElement root = dom.DocumentElement.SelectSingleNode("zServer") as XmlElement;

            if (this.checkBox_enableZ3950.Checked == false)
            {
                if (root != null)
                    root.ParentNode.RemoveChild(root);
                return true;
            }

            if (root == null)
            {
                root = dom.CreateElement("zServer");
                dom.DocumentElement.AppendChild(root);
            }

            {
                XmlElement element = root.SelectSingleNode("dp2library") as XmlElement;
                if (element == null)
                {
                    element = dom.CreateElement("dp2library");
                    root.AppendChild(element);
                }

                element.SetAttribute("anonymousUserName", this.AnonymousUserName);
                element.SetAttribute("anonymousPassword", EncryptPassword(this.AnonymousPassword));
            }

            {
                XmlElement element = root.SelectSingleNode("databases") as XmlElement;
                if (element == null)
                {
                    element = dom.CreateElement("databases");
                    root.AppendChild(element);
                }

                DomUtil.SetElementOuterXml(element, this.DatabasesXml);
            }

            return true;
        }

        static string EncryptKey = "dp2zserver_password_key";

        public string DecryptPasssword(string strEncryptedText)
        {
            if (String.IsNullOrEmpty(strEncryptedText) == false)
            {
                try
                {
                    string strPassword = Cryptography.Decrypt(
        strEncryptedText,
        EncryptKey);
                    return strPassword;
                }
                catch
                {
                    return "errorpassword";
                }

            }

            return "";
        }

        public string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, EncryptKey);
        }


        private void InstallZServerDlg_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void InstallZServerDlg_Load(object sender, EventArgs e)
        {
            FillInfo();
        }

        private void checkBox_enableZ3950_CheckedChanged(object sender, EventArgs e)
        {
            SetEnableZ3950UiState();
        }
    }
}