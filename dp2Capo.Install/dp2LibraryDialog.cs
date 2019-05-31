using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using DigitalPlatform.Core;
using DigitalPlatform.Drawing;
using DigitalPlatform.Forms;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;

namespace dp2Capo.Install
{
    /// <summary>
    /// 配置一个 dp2Capo 实例针对 dp2library 的参数
    /// </summary>
    public partial class dp2LibraryDialog : Form
    {
        // capo.xml
        public XmlDocument CfgDom { get; set; }

        public dp2LibraryDialog()
        {
            InitializeComponent();
        }

        private void dp2LibraryDialog_Load(object sender, EventArgs e)
        {
            FillInfo();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (string.IsNullOrEmpty(this.comboBox_url.Text))
            {
                strError = "尚未指定 dp2library 服务器 URL";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_manageUserName.Text))
            {
                strError = "尚未指定用户名";
                goto ERROR1;
            }

#if NO
            if (string.IsNullOrEmpty(this.comboBox_msmqPath.Text))
            {
                strError = "尚未指定消息队列路径";
                goto ERROR1;
            }
#endif
            // TODO: 检测 capo 用户名和密码是否正确
            int nRet = DetectManagerUser(out strError);
            if (nRet == -1)
                goto ERROR1;

            if (SaveToCfgDom() == false)
                return;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        int DetectManagerUser(out string strError)
        {
            strError = "";
            if (string.IsNullOrEmpty(this.textBox_manageUserName.Text) == false)
            {
                EnableControls(false);
                try
                {
                    // return:
                    //       -1  出错
                    //      0   不存在
                    //      1   存在, 且密码一致
                    //      2   存在, 但密码不一致
                    int nRet = DetectManageUser(out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    else if (nRet == 0)
                    {
                        strError = "代理帐户 '" + this.textBox_manageUserName.Text + "' 目前尚不存在。";
                        goto ERROR1;
                    }
                    else if (nRet == 2)
                    {
                        strError = "代理帐户 '" + this.textBox_manageUserName.Text + "' 经检验存在，但其密码和当前面板上输入的密码不一致。";
                        goto ERROR1;
                    }
                    else
                    {
                        Debug.Assert(nRet == 1, "");
                        return 0;
                    }
                }
                finally
                {
                    EnableControls(true);
                }
            }
            ERROR1:
            return -1;
        }

        private void button_detectManageUser_Click(object sender, EventArgs e)
        {
            int nRet = DetectManagerUser(out string strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                MessageBox.Show(this, "代理帐户 '" + this.textBox_manageUserName.Text + "' 经检验存在。");
#if NO
            EnableControls(false);
            try
            {
                if (string.IsNullOrEmpty(this.textBox_manageUserName.Text) == true)
                {
                    MessageBox.Show(this, "请先在面板上指定要检测的代理帐户名");
                    return;
                }
                string strError = "";
                // return:
                //       -1  出错
                //      0   不存在
                //      1   存在, 且密码一致
                //      2   存在, 但密码不一致
                int nRet = DetectManageUser(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
                else if (nRet == 0)
                {
                    MessageBox.Show(this, "代理帐户 '" + this.textBox_manageUserName.Text + "' 目前尚不存在。");
                }
                else if (nRet == 2)
                {
                    MessageBox.Show(this, "代理帐户 '" + this.textBox_manageUserName.Text + "' 经检验存在，但其密码和当前面板上输入的密码不一致。");
                }
                else
                {
                    Debug.Assert(nRet == 1, "");
                    MessageBox.Show(this, "代理帐户 '" + this.textBox_manageUserName.Text + "' 经检验存在。");
                }
            }
            finally
            {
                EnableControls(true);
            }
#endif
        }

        private void button_createManageUser_Click(object sender, EventArgs e)
        {
            EnableControls(false);

            try
            {
                string strError = "";
                int nRet = CreateManageUser(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
                else
                {
                    MessageBox.Show(this, "代理帐户创建成功。");
                }
            }
            finally
            {
                EnableControls(true);
            }
        }

        private void button_resetManageUserPassword_Click(object sender, EventArgs e)
        {
            // 重设置代理帐户密码
            EnableControls(false);
            try
            {
                string strError = "";
                int nRet = ResetManageUserPassword(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
                else
                {
                    MessageBox.Show(this, "重设代理帐户密码成功。");
                }
            }
            finally
            {
                EnableControls(true);
            }
        }

        public static string EncryptKey = "dp2capopassword";

        void FillDefaultValue()
        {
            this.comboBox_url.Text = "http://localhost:8001/dp2library";

            this.textBox_manageUserName.Text = "capo";

            this.textBox_managePassword.Text = "";
            this.textBox_confirmManagePassword.Text = "";

            this.comboBox_msmqPath.Text = "";   // @".\private$\myQueue";

            this.textBox_webURL.Text = "";
        }

        public static string DecryptPasssword(string strText)
        {
            return Cryptography.Decrypt(strText, EncryptKey);
        }

        public static string EncryptPassword(string strText)
        {
            return Cryptography.Encrypt(strText, EncryptKey);
        }

        // 从 CfgDom 中填充信息到控件
        void FillInfo()
        {
            XmlDocument dom = this.CfgDom;

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2library") as XmlElement;
            if (element == null)
            {
                element = dom.CreateElement("dp2library");
                dom.DocumentElement.AppendChild(element);

                FillDefaultValue();
                return;
            }

            this.comboBox_url.Text = element.GetAttribute("url");

            this.textBox_manageUserName.Text = element.GetAttribute("userName");

            // string strPassword = Cryptography.Decrypt(element.GetAttribute("password"), EncryptKey);
            string strPassword = DecryptPasssword(element.GetAttribute("password"));
            this.textBox_managePassword.Text = strPassword;
            this.textBox_confirmManagePassword.Text = strPassword;

            this.comboBox_msmqPath.Text = element.GetAttribute("defaultQueue");

            this.textBox_webURL.Text = element.GetAttribute("webURL").Replace(";", "\r\n");
        }

        public static string GetMessageQueue(XmlDocument CfgDom)
        {
            StringBuilder text = new StringBuilder();
            XmlDocument dom = CfgDom;

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2library") as XmlElement;
            if (element == null)
                return "";

            return element.GetAttribute("defaultQueue");
        }

        public static string GetDisplayText(XmlDocument CfgDom)
        {
            StringBuilder text = new StringBuilder();
            XmlDocument dom = CfgDom;

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2library") as XmlElement;
            if (element == null)
                return "";

            text.Append("url=" + element.GetAttribute("url") + "\r\n");
            text.Append("userName=" + element.GetAttribute("userName") + "\r\n");
            text.Append("defaultQueue=" + element.GetAttribute("defaultQueue") + "\r\n");
            text.Append("webURL=" + element.GetAttribute("webURL") + "\r\n");
            return text.ToString();
        }

        // 从控件到 CfgDom
        bool SaveToCfgDom()
        {
            if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
            {
                MessageBox.Show(this, "密码和确认密码不一致，请重新输入");
                return false;
            }

            XmlDocument dom = this.CfgDom;

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2library") as XmlElement;
            if (element == null)
            {
                element = dom.CreateElement("dp2library");
                dom.DocumentElement.AppendChild(element);
            }

            element.SetAttribute("url", this.comboBox_url.Text);

            element.SetAttribute("userName", this.textBox_manageUserName.Text);

            // string strPassword = Cryptography.Encrypt(this.textBox_managePassword.Text, EncryptKey);
            string strPassword = EncryptPassword(this.textBox_managePassword.Text);
            element.SetAttribute("password", strPassword);

            element.SetAttribute("defaultQueue", this.comboBox_msmqPath.Text);

            element.SetAttribute("webURL", this.textBox_webURL.Text.Replace("\r\n", ";"));
            return true;
        }

        void EnableControls(bool bEnable)
        {
            this.textBox_confirmManagePassword.Enabled = bEnable;
            this.textBox_managePassword.Enabled = bEnable;
            this.textBox_manageUserName.Enabled = bEnable;
            this.comboBox_url.Enabled = bEnable;
            this.textBox_webURL.Enabled = bEnable;

            this.button_OK.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;
            this.button_createManageUser.Enabled = bEnable;
            this.button_detectManageUser.Enabled = bEnable;
            this.button_resetManageUserPassword.Enabled = bEnable;

            this.comboBox_msmqPath.Enabled = bEnable;
        }

#region dp2library 协议有关操作

        string ManageAccountRights { get; set; }

        // 检测管理用户是否已经存在?
        // return:
        //       -1  出错
        //      0   不存在
        //      1   存在, 且密码一致
        //      2   存在, 但密码不一致
        int DetectManageUser(out string strError)
        {
            strError = "";
            if (this.comboBox_url.Text == "")
            {
                strError = "尚未指定 dp2Library 服务器 URL";
                return -1;
            }

            if (this.textBox_manageUserName.Text == "")
            {
                strError = "尚未指定代理帐户的用户名";
                return -1;
            }

            if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
            {
                strError = "代理帐户 密码 和 再次输入密码 不一致。请重新输入。";
                return -1;
            }

            using (LibraryChannel channel = new LibraryChannel())
            {
                channel.Url = this.comboBox_url.Text;

                // Debug.Assert(false, "");
                string strParameters = "location=#setup,type=worker,client=dp2CapoInstall|0.01";
                long nRet = channel.Login(this.textBox_manageUserName.Text,
                    this.textBox_managePassword.Text,
                    strParameters,
                    out strError);
                if (nRet == -1)
                {
                    strError = "以用户名 '" + this.textBox_manageUserName.Text + "' 和密码登录失败: " + strError;
                    return -1;
                }

                if (nRet == 1)
                    this.ManageAccountRights = channel.Rights;

                channel.Logout(out strError);

                if (nRet == 0)
                {
                    channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                    channel.BeforeLogin += new BeforeLoginEventHandle(channel_BeforeLogin);

                    strError = "为确认代理帐户是否存在, 请用超级用户身份登录。";
                    nRet = channel.DoNotLogin(ref strError);
                    if (nRet == -1 || nRet == 0)
                    {
                        strError = "以超级用户身份登录失败: " + strError + "\r\n\r\n因此无法确定代理帐户是否存在";
                        return -1;
                    }

                    UserInfo[] users = null;
                    nRet = channel.GetUser(
                        "list",
                        this.textBox_manageUserName.Text,
                        0,
                        -1,
                        out users,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "获取用户 '" + this.textBox_manageUserName.Text + "' 信息时发生错误: " + strError + "\r\n\r\n因此无法确定代理帐户是否存在。";
                        return -1;
                    }
                    if (nRet == 1)
                    {
                        Debug.Assert(users != null, "");
                        strError = "代理帐户 '" + this.textBox_manageUserName.Text + "' 已经存在, 但其密码和当前面板拟设置的密码不一致。";
                        return 2;
                    }
                    if (nRet >= 1)
                    {
                        Debug.Assert(users != null, "");
                        strError = "以 '" + this.textBox_manageUserName.Text + "' 为用户名 的用户记录存在多条，这是一个严重错误，请系统管理员启用dp2circulation尽快修正此错误。";
                        return -1;
                    }

                    return 0;
                }

                return 1;
            }
        }

        string SupervisorUserName { get; set; }
        string SupervisorPassword { get; set; }

        void channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == true
                && string.IsNullOrEmpty(this.SupervisorUserName) == false)
            {
                e.UserName = this.SupervisorUserName;
                e.Password = this.SupervisorPassword;

                e.Parameters = "location=#setup,type=worker,client=dp2CapoInstall|0.01";   // 2016/5/6 加上 0.01 部分

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            // 
            IWin32Window owner = null;

            if (sender is IWin32Window)
                owner = (IWin32Window)sender;
            else
                owner = this;

            ConfirmSupervisorDialog dlg = new ConfirmSupervisorDialog();
            FontUtil.AutoSetDefaultFont(dlg);
            // dlg.Text = "";
            dlg.ServerUrl = this.comboBox_url.Text;
            dlg.Comment = e.ErrorInfo;
            dlg.UserName = e.UserName;
            dlg.Password = e.Password;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            dlg.ShowDialog(owner);

            if (dlg.DialogResult == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            e.UserName = dlg.UserName;
            e.Password = dlg.Password;
            e.SavePasswordShort = true;
            e.Parameters = "location=#setup,type=worker,client=dp2CapoInstall|0.01";

            e.SavePasswordLong = true;
            e.LibraryServerUrl = dlg.ServerUrl;

            this.SupervisorUserName = e.UserName;
            this.SupervisorPassword = e.Password;
        }

        // 创建代理帐户
        // 安装成功后，dp2Capo 运行中，点对点 API 实际上是使用这个账户对 dp2library 进行操作的
        int CreateManageUser(out string strError)
        {
            strError = "";
            if (this.comboBox_url.Text == "")
            {
                strError = "尚未指定 dp2Library 服务器 URL";
                return -1;
            }

            if (this.textBox_manageUserName.Text == "")
            {
                strError = "尚未指定代理帐户的用户名";
                return -1;
            }

            if (this.textBox_manageUserName.Text == "reader"
                || this.textBox_manageUserName.Text == "public"
                || this.textBox_manageUserName.Text == "opac"
                || this.textBox_manageUserName.Text == "图书馆")
            {
                strError = "代理帐户的用户名不能为 'reader' 'public' 'opac' '图书馆' 之一，因为这些都是 dp2Library 系统内具有特定用途的保留帐户名";
                return -1;
            }

            if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
            {
                strError = "代理帐 密码 和 再次输入密码 不一致。请重新输入。";
                return -1;
            }

            using (LibraryChannel channel = new LibraryChannel())
            {
                channel.Url = this.comboBox_url.Text;

                channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                channel.BeforeLogin += new BeforeLoginEventHandle(channel_BeforeLogin);

                strError = "请用超级用户身份登录，以便创建代理帐户。";
                int nRet = channel.DoNotLogin(ref strError);
                if (nRet == -1 || nRet == 0)
                {
                    strError = "以超级用户身份登录失败: " + strError;
                    return -1;
                }

                // 2.81
                // 检查 dp2library 版本
                // return:
                //      -1  出错
                //      0   dp2library 版本太低
                //      1   成功
                nRet = CheckVersion(channel,
                    ProductUtil.dp2library_base_version,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    return -1;

                UserInfo user = new UserInfo();
                user.UserName = this.textBox_manageUserName.Text;
                user.Password = this.textBox_managePassword.Text;
                user.SetPassword = true;
                user.Binding = "ip:[current]";  // 自动绑定当前请求者的 IP
                // default_capo_rights
                user.Rights = "getsystemparameter,getres,search,getbiblioinfo,setbiblioinfo,getreaderinfo,writeobject,getbibliosummary,listdbfroms,simulatereader,simulateworker"
                    + ",getiteminfo,getorderinfo,getissueinfo,getcommentinfo"
                    + ",borrow,return,getmsmqmessage"
                    + ",bindpatron,searchbiblio,getpatrontempid,resetpasswordreturnmessage,getuser,changereaderpassword,renew,reservation";

                long lRet = channel.SetUser(
        "new",
        user,
        out strError);
                if (lRet == -1)
                {
                    strError = "创建代理帐户时发生错误: " + strError;
                    return -1;
                }

                channel.Logout(out strError);
                return 0;
            }
        }

        // 检查 dp2library 版本
        // return:
        //      -1  出错
        //      0   dp2library 版本太低
        //      1   成功
        static int CheckVersion(LibraryChannel channel,
            string base_version,
            out string strError)
        {
            strError = "";
            string strVersion = "";
            string strUID = "";
            long lRet = channel.GetVersion(
out strVersion,
out strUID,
out strError);
            if (lRet == -1)
            {
                strError = "针对服务器 " + channel.Url + " 获得版本号的过程发生错误：" + strError;
                return -1;
            }

            if (string.IsNullOrEmpty(strVersion) == true)
                strVersion = "2.0";

            if (StringUtil.CompareVersion(strVersion, base_version) < 0)   // 2.12
            {
                strError = "dp2library 版本必须升级为 " + base_version + " 以上时才能使用 (当前 dp2library 版本为 " + strVersion + ")\r\n\r\n请立即升级 dp2Library 到最新版本";
                return 0;
            }

            return 1;
        }

        // 重设置代理帐户密码
        int ResetManageUserPassword(out string strError)
        {
            strError = "";
            if (this.comboBox_url.Text == "")
            {
                strError = "尚未指定 dp2Library 服务器 URL";
                return -1;
            }

            if (this.textBox_manageUserName.Text == "")
            {
                strError = "尚未指定代理帐户的用户名";
                return -1;
            }

            if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
            {
                strError = "代理帐户 密码 和 再次输入密码 不一致。请重新输入。";
                return -1;
            }

            using (LibraryChannel channel = new LibraryChannel())
            {
                channel.Url = this.comboBox_url.Text;

                channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                channel.BeforeLogin += new BeforeLoginEventHandle(channel_BeforeLogin);

                strError = "请用超级用户身份登录，以便重设代理帐户密码。";
                int nRet = channel.DoNotLogin(ref strError);
                if (nRet == -1 || nRet == 0)
                {
                    strError = "以超级用户身份登录失败: " + strError;
                    return -1;
                }

                if (StringUtil.IsInList("changeuserpassword", channel.Rights) == false)
                {
                    strError = "您所使用的超级用户 '" + this.SupervisorUserName + "' 不具备 changeuserpassword 权限，无法进行(为代理帐户 '" + this.textBox_manageUserName.Text + "' )重设密码的操作";
                    return -1;
                }

                UserInfo user = new UserInfo();
                user.UserName = this.textBox_manageUserName.Text;
                user.Password = this.textBox_managePassword.Text;

                long lRet = channel.SetUser(
                    "resetpassword",
                    user,
                    out strError);
                if (lRet == -1)
                {
                    strError = "重设密码时发生错误: " + strError;
                    return -1;
                }

                channel.Logout(out strError);
                return 0;
            }
        }

        // 从 dp2library 服务器获得 outgoingQueue 字符串
        int GetQueuePath(out string strError)
        {
            strError = "";
            if (this.comboBox_url.Text == "")
            {
                strError = "尚未指定 dp2Library 服务器 URL";
                return -1;
            }

            using (LibraryChannel channel = new LibraryChannel())
            {
                channel.Url = this.comboBox_url.Text;

                channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                channel.BeforeLogin += new BeforeLoginEventHandle(channel_BeforeLogin);

                strError = "请用超级用户身份登录，以便获得队列路径配置信息。";
                int nRet = channel.DoNotLogin(ref strError);
                if (nRet == -1 || nRet == 0)
                {
                    strError = "以超级用户身份登录失败: " + strError;
                    return -1;
                }

                // 2.81
                // 检查 dp2library 版本
                // return:
                //      -1  出错
                //      0   dp2library 版本太低
                //      1   成功
                nRet = CheckVersion(channel,
                    ProductUtil.dp2library_base_version,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    return -1;

                string strValue = "";
                long lRet = channel.GetSystemParameter(
        "system",
        "outgoingQueue",
        out strValue,
        out strError);
                if (lRet != 1)
                {
                    strError = "从 dp2library 服务器获得配置信息时发生错误: " + strError;
                    return -1;
                }

                this.comboBox_msmqPath.Text = strValue;
                channel.Logout(out strError);
                return 0;
            }
        }

#endregion

        private void textBox_manageUserName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBox_manageUserName.Text) == true)
            {
                this.button_detectManageUser.Enabled = false;
                this.button_createManageUser.Enabled = false;
                this.button_resetManageUserPassword.Enabled = false;
            }
            else
            {
                this.button_detectManageUser.Enabled = true;
                this.button_createManageUser.Enabled = true;
                this.button_resetManageUserPassword.Enabled = true;
            }
        }

        private void button_getQueuePath_Click(object sender, EventArgs e)
        {
            string strError = "";
            EnableControls(false);
            try
            {
                // 从 dp2library 服务器获得 outgoingQueue 字符串
                int nRet = GetQueuePath(out strError);
                if (nRet == -1)
                    goto ERROR1;
                return;
            }
            finally
            {
                EnableControls(true);
            }
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 为工作人员添加管理公众号的权限
        private void button_workerRights_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.EnableControls(false);
            try
            {
                using (LibraryChannel channel = new LibraryChannel())
                {
                    channel.Url = this.comboBox_url.Text;

                    channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                    channel.BeforeLogin += new BeforeLoginEventHandle(channel_BeforeLogin);

                    strError = "请用超级用户身份登录，以便为工作人员添加管理公众号的权限。";
                    int nRet = channel.DoNotLogin(ref strError);
                    if (nRet == -1 || nRet == 0)
                    {
                        strError = "以超级用户身份登录失败: " + strError;
                        goto ERROR1;
                    }

                    WorkerRightsDialog dlg = new WorkerRightsDialog();
                    FontUtil.AutoSetDefaultFont(dlg);
                    dlg.Channel = channel;
                    dlg.ManagerUserName = this.textBox_manageUserName.Text;
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    dlg.ShowDialog(this);

                    channel.Logout(out strError);
                }
            }
            finally
            {
                this.EnableControls(true);
            }
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_createDpAccount_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = CreateDpUser(out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        int CreateDpUser(out string strError)
        {
            strError = "";
            if (this.comboBox_url.Text == "")
            {
                strError = "尚未指定 dp2Library 服务器 URL";
                return -1;
            }

            using (LibraryChannel channel = new LibraryChannel())
            {
                channel.Url = this.comboBox_url.Text;

                channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                channel.BeforeLogin += new BeforeLoginEventHandle(channel_BeforeLogin);

                strError = "请用超级用户身份登录，以便创建 dp 帐户。";
                int nRet = channel.DoNotLogin(ref strError);
                if (nRet == -1 || nRet == 0)
                {
                    strError = "以超级用户身份登录失败: " + strError;
                    return -1;
                }

                // 2.81
                // 检查 dp2library 版本
                // return:
                //      -1  出错
                //      0   dp2library 版本太低
                //      1   成功
                nRet = CheckVersion(channel,
                    ProductUtil.dp2library_base_version,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    return -1;

                CreateDpUserDialog dlg = new CreateDpUserDialog();
                dlg.UserName = "dp";
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);
                if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    strError = "放弃创建 dp 用户";
                    return -1;
                }

                UserInfo user = new UserInfo();
                user.UserName = dlg.UserName;
                user.Password = dlg.Password;
                user.SetPassword = true;
                user.Binding = "ip:[current]"   // 自动绑定当前请求者的 IP
                    + ",router_ip:" + StringUtil.MakePathList(dlg.IpList, "|");
                user.Rights = "changeuser,getcalendar,getchannelinfo,getoperlog,getreaderinfo,getres,getsystemparameter,getuser,listbibliodbfroms,listdbfroms,managedatabase,order,searchreader,setsystemparameter,writeres,managechannel";
                user.Comment = "数字平台远程维护用账号";

                long lRet = channel.SetUser(
        "new",
        user,
        out strError);
                if (lRet == -1)
                {
                    strError = "创建代理帐户时发生错误: " + strError;
                    return -1;
                }

                channel.Logout(out strError);
                return 0;
            }
        }
    }
}
