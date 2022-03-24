using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.ComponentModel;

using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.SIP.Server;
using DigitalPlatform.Forms;
using DigitalPlatform;
using DigitalPlatform.Xml;

namespace dp2Capo.Install
{
    public partial class SipSettingDialog : Form
    {
        // capo.xml
        public XmlDocument CfgDom { get; set; }

        public SipSettingDialog()
        {
            InitializeComponent();

            this.tabControl_main.TabPages.Remove(this.tabPage_sip);
            this.tabPage_sip.Dispose();
        }

        private void SipSettingDialog_Load(object sender, EventArgs e)
        {
            FillInfo();

            SetEnableSipUiState();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 按下 Control 键可越过探测步骤
            bool bControl = Control.ModifierKeys == Keys.Control;

            string strError = "";


            if (SaveToCfgDom() == false)
                return;

            // 警告：如果一个 UserMap 事项也没有，则启用了 SIP Service 也无法真正投入使用
            if (this.checkBox_enableSIP.Checked == true && this.listBox_userNameList.Items.Count == 0)
            {
                MessageBox.Show(this, "警告：虽然启用了 SIP Service，但因为没有配置任何用户名映射参数，所以该实例的 SIP Service 实际上无法访问");
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;

        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public static string GetDisplayText(XmlDocument CfgDom)
        {
            StringBuilder text = new StringBuilder();
            XmlDocument dom = CfgDom;

            XmlElement root = dom.DocumentElement.SelectSingleNode("sipServer") as XmlElement;
            if (root == null)
                return "";

            if (dom.DocumentElement.SelectSingleNode("sipServer/dp2library") is XmlElement node)
            {
                text.Append("anonymousUserName=" + node.GetAttribute("anonymousUserName") + "\r\n");
            }

            if (dom.DocumentElement.SelectSingleNode("sipServer") is XmlElement element)
            {
                XmlNodeList nodes = element.SelectNodes("user");
                text.Append("userCount=" + nodes.Count + "\r\n");

#if NO
                text.Append("encoding=" + element.GetAttribute("encoding") + "\r\n");
                text.Append("dateFormat=" + element.GetAttribute("dateFormat") + "\r\n");
                text.Append("ipList=" + element.GetAttribute("ipList") + "\r\n");
                text.Append("autoClearSeconds=" + element.GetAttribute("autoClearSeconds") + "\r\n");
#endif
            }

            if (text.Length == 0)
                text.Append("*");

            return text.ToString();
        }

        void EnableControls(bool bEnable)
        {
            this.button_OK.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;
            this.button_detectManageUser.Enabled = bEnable;

            this.textBox_anonymousUserName.Enabled = bEnable;
            this.textBox_anonymousPassword.Enabled = bEnable;
            this.button_detectAnonymousUser.Enabled = bEnable;

            this.Update();
        }

        void FillInfo()
        {
            XmlDocument dom = this.CfgDom;

            {
                // dp2library 服务器参数

                // 万一已经存在的文件是不正确的?
                if (!(dom.DocumentElement.SelectSingleNode("dp2library") is XmlElement node))
                {
                    //strError = "配置文件中缺乏 libraryserver 元素";
                    //return -1;
                    this.textBox_manageUserName.Text = "";
                    this.textBox_managePassword.Text = "";

                    this.comboBox_librarywsUrl.Text = "";
                }
                else
                {
                    Debug.Assert(node != null, "");

                    string strUserName = node.GetAttribute("userName");
                    string strPassword = node.GetAttribute("password");
                    strPassword = dp2LibraryDialog.DecryptPasssword(strPassword);

                    string strUrl = node.GetAttribute("url");

                    this.textBox_manageUserName.Text = strUserName;
                    this.textBox_managePassword.Text = strPassword;

                    if (String.IsNullOrEmpty(strUrl) == false)
                        this.comboBox_librarywsUrl.Text = strUrl;
                }
            }

            {
                // sipServer 匿名登录账号 参数
                if (!(dom.DocumentElement.SelectSingleNode("sipServer/dp2library") is XmlElement node))
                {
                    this.textBox_anonymousUserName.Text = "";
                    this.textBox_anonymousPassword.Text = "";
                }
                else
                {
                    Debug.Assert(node != null, "");

                    string strAnonymousUserName = node.GetAttribute("anonymousUserName");
                    string strAnonymousPassword = node.GetAttribute("anonymousPassword");
                    strAnonymousPassword = DecryptPasssword(strAnonymousPassword);

                    this.textBox_anonymousUserName.Text = strAnonymousUserName;
                    this.textBox_anonymousPassword.Text = strAnonymousPassword;
                }
            }

#if NO
            {
                // sipServer 参数
                if (!(dom.DocumentElement.SelectSingleNode("sipServer") is XmlElement node))
                {
                    this.comboBox_dateFormat.Text = "";
                    this.comboBox_encodingName.Text = "";
                    this.textBox_ipList.Text = "";
                    this.textBox_autoClearTime.Text = "";
                }
                else
                {
                    Debug.Assert(node != null, "");

                    this.comboBox_dateFormat.Text = node.GetAttribute("dateFormat");
                    this.comboBox_encodingName.Text = node.GetAttribute("encoding");
                    this.textBox_ipList.Text = node.GetAttribute("ipList");
                    this.textBox_autoClearTime.Text = node.GetAttribute("autoClearSeconds");
                }

                if (string.IsNullOrEmpty(this.comboBox_dateFormat.Text))
                    this.comboBox_dateFormat.Text = SipServer.DEFAULT_DATE_FORMAT;
                if (string.IsNullOrEmpty(this.comboBox_encodingName.Text))
                    this.comboBox_encodingName.Text = SipServer.DEFAULT_ENCODING_NAME;
            }
#endif

            XmlElement root = dom.DocumentElement.SelectSingleNode("sipServer") as XmlElement;

            FillUserMap(root);

            this.checkBox_enableSIP.Checked = (root != null && DomUtil.IsBooleanTrue(root.GetAttribute("enable"), true));

            // SetEnableSipUiState();
        }

        void FillUserMap(XmlElement root)
        {
            this.listBox_userNameList.Items.Clear();

            if (root == null)
                return;

            XmlNodeList nodes = root.SelectNodes("user");
            foreach (XmlElement user in nodes)
            {
                UserItem item = new UserItem();
                item.UserName = user.GetAttribute("userName");
                item.DateFormat = user.GetAttribute("dateFormat");
                item.EncodingName = user.GetAttribute("encoding");
                string style = user.GetAttribute("style");
                item.BookUiiStrict = StringUtil.IsInList("bookUiiStrict", style);
                
                // 2022/3/22
                string maxChannels = user.GetAttribute("maxChannels");
                if (string.IsNullOrEmpty(maxChannels))
                    maxChannels = SipServer.DEFAULT_MAXCHANNELS.ToString();
                if (Int32.TryParse(maxChannels, out int value))
                    item.MaxChannels = value;
                else
                    item.MaxChannels = SipServer.DEFAULT_MAXCHANNELS;
                
                item.IpList = user.GetAttribute("ipList");
                item.AutoClearSeconds = user.GetAttribute("autoClearSeconds");
                item.PropertyChanged += Item_PropertyChanged;

                this.listBox_userNameList.Items.Add(item);
            }

            if (this.listBox_userNameList.Items.Count > 0)
                this.listBox_userNameList.SelectedIndex = 0;
        }

        // 把内存对象兑现到 XML Document
        void RestoreUserMap(XmlElement root)
        {
            if (root == null)
                return;

            // TODO: 检查数据合法性
            if (string.IsNullOrEmpty(this.textBox_anonymousUserName.Text) == false)
            {
                bool bFound = false;
                // 要求至少有一个 * 用户名，或者和匿名账户相等的用户名
                foreach (UserItem item in this.listBox_userNameList.Items)
                {
                    if (item.UserName == "*" || item.UserName == this.textBox_anonymousUserName.Text)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (bFound == false)
                {
                    throw new ArgumentException("匿名账户名 '" + this.textBox_anonymousUserName.Text + "' 在 'SIP' 属性页的用户名列表中尚未定义");
                }
            }

            XmlNodeList nodes = root.SelectNodes("user");
            // 删除以前的 uer 元素
            foreach (XmlElement user in nodes)
            {
                user.ParentNode.RemoveChild(user);
            }

            // 重新创建 user 元素
            foreach (UserItem item in this.listBox_userNameList.Items)
            {
                XmlElement user = root.OwnerDocument.CreateElement("user");
                root.AppendChild(user);

                string style = "";
                if (item.BookUiiStrict)
                    style = "bookUiiStrict";

                user.SetAttribute("userName", item.UserName);
                user.SetAttribute("dateFormat", item.DateFormat);
                user.SetAttribute("style", style);

                // 2022/3/22
                user.SetAttribute("maxChannels", item.MaxChannels.ToString());

                user.SetAttribute("encoding", item.EncodingName);
                user.SetAttribute("ipList", item.IpList);
                user.SetAttribute("autoClearSeconds", item.AutoClearSeconds);
            }
        }

        #region 为 UserMap 服务的类

        [DefaultProperty("UserName")]
        class UserItem : INotifyPropertyChanged
        {
            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            #endregion

            string _userName = "";

            [DisplayName("用户名"), Description("dp2library 用户名")]
            [Category(" 用户名")]
            public string UserName
            {

                get { return _userName; }

                set
                {
                    if (string.IsNullOrEmpty(value))
                        throw new ArgumentException("用户名不应为空");

                    // 检查用户名合法性
                    // return:
                    //      -1  校验过程出错
                    //      0   校验发现不正确
                    //      1   校验正确
                    if (VerifyDp2libraryUserName(value,
                        out string strError) != 1)
                        throw new ArgumentException("用户名 '" + value + "' 不合法：" + strError);

                    _userName = value;
                    OnPropertyChanged("UserName");
                }
            }

            string _encodingName = SipServer.DEFAULT_ENCODING_NAME;

            [DisplayName("编码方式"), Description("SIP 通讯包采用何种编码方式")]
            [TypeConverter(typeof(EncodingNameConverter))]
            [DefaultValue(SipServer.DEFAULT_ENCODING_NAME)]
            [Category("SIP 参数")]
            public string EncodingName
            {
                get { return _encodingName; }
                set
                {
                    if (string.IsNullOrEmpty(value) == false)
                    {
                        try
                        {
                            Encoding encoding = Encoding.GetEncoding(value);
                        }
                        catch (Exception ex)
                        {
                            throw new ArgumentException("编码方式 '" + value + "' 不合法。" + ex.Message);
                        }
                    }

                    _encodingName = value;
                    OnPropertyChanged("EncodingName");
                }
            }

            string _dateFormat = SipServer.DEFAULT_DATE_FORMAT;

            [DisplayName("时间格式"), Description("SIP 通讯包中采用何种时间格式")]
            [TypeConverter(typeof(DateFormatConverter))]
            [DefaultValue(SipServer.DEFAULT_DATE_FORMAT)]
            [Category("SIP 参数")]
            public string DateFormat
            {
                get { return _dateFormat; }
                set
                {
                    // TODO: 验证时间格式合法性
                    _dateFormat = value;
                    OnPropertyChanged("DateFormat");
                }
            }

            [DisplayName("前端 IP 地址白名单"), Description("前端 IP 地址白名单。空表示不启用白名单机制，也就是所有前端的访问都被允许")]
            [Category("SIP 参数")]
            public string IpList { get; set; }

            string _autoClearSeconds = "";
            [DisplayName("通道自动清理秒数"), Description("休眠多少秒以后自动清理通道。空或 0 表示不清理")]
            [Category("SIP 参数")]
            public string AutoClearSeconds
            {
                get { return _autoClearSeconds; }
                set
                {
                    if (string.IsNullOrEmpty(value) == false
                        && Int32.TryParse(value, out int seconds) == false)
                        throw new ArgumentException("自动清理秒数值 '" + value + "' 不合法。应为纯数字");
                    _autoClearSeconds = value;
                }
            }

            bool _bookUiiStrict = SipServer.DEFAULT_BOOKUIISTRICT;

            [DisplayName("图书 UII 严格要求"), Description("是否严格要求请求中的图书号码采用 UII 形态")]
            [DefaultValue(SipServer.DEFAULT_ENCODING_NAME)]
            [Category("SIP 参数")]
            public bool BookUiiStrict
            {
                get { return _bookUiiStrict; }
                set
                {
                    _bookUiiStrict = value;
                    OnPropertyChanged("BookUiiStrict");
                }
            }

            int _maxChannels = SipServer.DEFAULT_MAXCHANNELS;

            [DisplayName("TCP 通道数限额"), Description("允许使用的最多 TCP 通道数。-1 表示不限制")]
            [DefaultValue(SipServer.DEFAULT_MAXCHANNELS)]
            [Category("SIP 参数")]
            public int MaxChannels
            {
                get { return _maxChannels; }
                set
                {
                    _maxChannels = value;
                    OnPropertyChanged("MaxChannels");
                }
            }

            public override string ToString()
            {
                return UserName;
            }
        }

        // 具有列表的编码方式名
        class EncodingNameConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new string[] { "UTF-8", "GB2312", });
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false;
            }
        }

        class DateFormatConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new string[] { "yyyy-MM-dd", "yyyyMMdd", });
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false;
            }
        }

        #endregion

        void SetEnableSipUiState()
        {
#if NO
            if (this.checkBox_enableSIP.Checked)
                this.tabControl_main.Enabled = true;
            else
                this.tabControl_main.Enabled = false;
#endif
            foreach (TabPage page in this.tabControl_main.TabPages)
            {
                page.Enabled = this.checkBox_enableSIP.Checked;
            }
        }

        // 刚启用 SIP 以后的后继动作
        void AfterEnableSipUiState()
        {
            if (this.listBox_userNameList.Items.Count == 0)
            {
                // 加入一个 * 用户名事项，表示匹配任意用户名
                UserItem item = new UserItem { UserName = "*" };
                item.PropertyChanged += Item_PropertyChanged;
                this.listBox_userNameList.Items.Add(item);
                this.listBox_userNameList.SelectedItem = item;
            }
        }

        UserItem FindItem(string userName, UserItem exclude)
        {
            foreach (UserItem item in this.listBox_userNameList.Items)
            {
                if (item != exclude && item.UserName == userName)
                    return item;
            }

            return null;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "UserName")
                return;

            UserItem item = sender as UserItem;

            // TODO: 对 UserName 进行查重
            UserItem dup = FindItem(item.UserName, item);
            if (dup != null)
            {
                throw new ArgumentException("用户名 '" + item.UserName + "' 已经被其他事项使用了。请修改");
            }

            int index = this.listBox_userNameList.Items.IndexOf(item);
            if (index != -1)
            {
                object selected_item = this.listBox_userNameList.SelectedItem;

                this.listBox_userNameList.Items.Remove(item);
                this.listBox_userNameList.Items.Insert(index, item);

                this.listBox_userNameList.SelectedItem = selected_item;
            }
        }

        // 从控件到 CfgDom
        // return:
        //      false   数据有错。没有保存成功
        //      true    保存成功
        bool SaveToCfgDom()
        {
            XmlDocument dom = this.CfgDom;

            XmlElement root = dom.DocumentElement.SelectSingleNode("sipServer") as XmlElement;

            if (this.checkBox_enableSIP.Checked == false)
            {
                //if (root != null)
                //    root.ParentNode.RemoveChild(root);
                if (root == null)
                    return true;
            }

            if (root == null)
            {
                root = dom.CreateElement("sipServer");
                dom.DocumentElement.AppendChild(root);
            }

            root.SetAttribute("enable", this.checkBox_enableSIP.Checked ? "true" : "false");

            // 检查数据合法性
            if (string.IsNullOrEmpty(this.textBox_autoClearTime.Text) == false
                && Int32.TryParse(this.textBox_autoClearTime.Text, out int seconds) == false)
            {
                MessageBox.Show(this, "自动清理时间秒数 '" + this.textBox_autoClearTime.Text + "' 不合法。应该为纯数字");
                return false;
            }

            {
                if (!(root.SelectSingleNode("dp2library") is XmlElement element))
                {
                    element = dom.CreateElement("dp2library");
                    root.AppendChild(element);
                }

                element.SetAttribute("anonymousUserName", this.textBox_anonymousUserName.Text);
                element.SetAttribute("anonymousPassword", EncryptPassword(this.textBox_anonymousPassword.Text));
            }

#if NO
            {
                root.SetAttribute("dateFormat", this.comboBox_dateFormat.Text);
                root.SetAttribute("encoding", this.comboBox_encodingName.Text);
                root.SetAttribute("ipList", this.textBox_ipList.Text);
                root.SetAttribute("autoClearSeconds", this.textBox_autoClearTime.Text);
            }
#endif

            try
            {
                RestoreUserMap(root);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                return false;
            }

            return true;
        }

        static string EncryptKey = "dp2sipserver_password_key";

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

        private void checkBox_enableSIP_CheckedChanged(object sender, EventArgs e)
        {
#if NO
            // TODO: 只能是勾选了 checkbox 才触发这里
            if (this.checkBox_enableSIP.Checked == true)
            {
                AfterEnableSipUiState();
            }
#endif

            SetEnableSipUiState();
        }

        private void button_detectManageUser_Click(object sender, EventArgs e)
        {
            EnableControls(false);

            try
            {
                if (string.IsNullOrEmpty(this.comboBox_librarywsUrl.Text))
                {
                    MessageBox.Show(this, "尚未输入 dp2Library 服务器的 URL");
                    return;
                }

                if (string.IsNullOrEmpty(this.textBox_manageUserName.Text))
                {
                    MessageBox.Show(this, "尚未指定 dp2Library 管理用户名");
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
                    this.textBox_manageUserName.Text,
                    this.textBox_managePassword.Text,
                    out string strError);
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

        private void button_detectAnonymousUser_Click(object sender, EventArgs e)
        {
            EnableControls(false);

            try
            {
                if (string.IsNullOrEmpty(this.comboBox_librarywsUrl.Text))
                {
                    MessageBox.Show(this, "尚未输入 dp2Library 服务器的 URL");
                    return;
                }

                if (string.IsNullOrEmpty(this.textBox_anonymousUserName.Text))
                {
                    MessageBox.Show(this, "尚未指定 匿名登录用户名");
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
                    out string strError);
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
                    "location=SIP Server,type=worker,client=chordInstaller|3.0",
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
        }

        private void button_userMap_Click(object sender, EventArgs e)
        {
            string strError = "";

            // sipServer 参数
            if (!(this.CfgDom.DocumentElement.SelectSingleNode("sipServer") is XmlElement node))
            {
                strError = "配置文件中根元素下不存在 sipServer 元素";
                goto ERROR1;
            }

            //SipAttributesDialog dlg = new SipAttributesDialog();

            //dlg.ContainerElement = node;
            //dlg.ShowDialog(this);

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBox_userNameList.SelectedItem != null)
            {
                this.toolStripButton_deleteUser.Enabled = true;

                this.propertyGrid_userMapProperty.SelectedObject = this.listBox_userNameList.SelectedItem;
            }
            else
            {
                this.toolStripButton_deleteUser.Enabled = false;

                this.propertyGrid_userMapProperty.SelectedObject = null;
            }
        }

        // return:
        //      -1  校验过程出错
        //      0   校验发现不正确
        //      1   校验正确
        public static int VerifyDp2libraryUserName(string strUserName,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strUserName))
            {
                strError = "用户名不应为空";
                return 0;
            }

            if (strUserName.IndexOf("@") != -1)
            {
                strError = "用户名中不应包含 @";
                return 0;
            }

            return 1;
        }

        private void toolStripButton_newUser_Click(object sender, EventArgs e)
        {
            REDO:
            string strUserName = InputDlg.GetInput(this,
    "创建新用户事项",
    "用户名(* 表示匹配任意用户名)",
    "",
    this.Font);
            if (strUserName == null)
                return;

            // 检查用户名合法性
            // return:
            //      -1  校验过程出错
            //      0   校验发现不正确
            //      1   校验正确
            if (VerifyDp2libraryUserName(strUserName,
                out string strError) != 1)
            {
                MessageBox.Show(this, "用户名 '" + strUserName + "' 不合法：" + strError);
                goto REDO;
            }

            UserItem dup = FindItem(strUserName, null);
            if (dup != null)
            {
                this.listBox_userNameList.SelectedItem = dup;
                MessageBox.Show(this, "用户名 '" + strUserName + "' 已经被其他事项使用了。请重新输入");
                goto REDO;
            }

            // TODO: 对用户名在列表中查重。不允许空用户名
            UserItem item = new UserItem();
            item.UserName = strUserName;
            item.PropertyChanged += Item_PropertyChanged;

            this.listBox_userNameList.Items.Add(item);
            this.listBox_userNameList.SelectedItem = item;
        }

        private void toolStripButton_deleteUser_Click(object sender, EventArgs e)
        {
            if (this.listBox_userNameList.SelectedItem != null)
                this.listBox_userNameList.Items.Remove(this.listBox_userNameList.SelectedItem);

        }


    }
}
