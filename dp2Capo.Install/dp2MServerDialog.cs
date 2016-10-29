using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Threading;

using DigitalPlatform.Text;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Forms;
using DigitalPlatform.Drawing;
using DigitalPlatform.Message;

namespace dp2Capo.Install
{
    public partial class dp2MServerDialog : Form
    {
        // capo.xml
        public XmlDocument CfgDom { get; set; }

        // dp2mserver 超级用户账户名
        string ManagerUserName { get; set; }
        string ManagerPassword { get; set; }

        public dp2MServerDialog()
        {
            InitializeComponent();
        }

        private void dp2MServerDialog_Load(object sender, EventArgs e)
        {
            FillInfo();
        }

        private async void button_OK_Click(object sender, EventArgs e)
        {
            // 按下 Control 键可越过探测步骤
            bool bControl = Control.ModifierKeys == Keys.Control;

            string strError = "";
            if (string.IsNullOrEmpty(this.textBox_url.Text))
            {
                strError = "尚未指定 dp2MServer URL";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_userName.Text))
            {
                strError = "尚未指定用户名";
                goto ERROR1;
            }

            if (bControl == false && await DetectUser() == false)
                return;

            if (SaveToCfgDom() == false)
                return;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            this.Invoke(new Action(() =>
            {
                MessageBox.Show(this, strError);
            }));
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        void EnableControls(bool bEnable)
        {
            this.textBox_url.Enabled = bEnable;
            this.textBox_userName.Enabled = bEnable;
            this.textBox_password.Enabled = bEnable;
            this.textBox_confirmManagePassword.Enabled = bEnable;
            this.button_detect.Enabled = bEnable;
            this.button_createUser.Enabled = bEnable;
            this.button_OK.Enabled = bEnable;
        }

        CancellationTokenSource _cancel = new CancellationTokenSource();

        private async void button_detect_Click(object sender, EventArgs e)
        {
            if (this.textBox_password.Text != this.textBox_confirmManagePassword.Text)
            {
                MessageBox.Show(this, "密码 和 确认密码 不一致。请重新输入");
                return;
            }

            if (await DetectUser() == true)
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show(this, "账户存在");
                }));
            }
        }

        async Task<bool> DetectUser()
        {
            string strError = "";
            EnableControls(false);
            try
            {
                using (MessageConnectionCollection _channels = new MessageConnectionCollection())
                {
                    _channels.Login += _channels_Login;

                    MessageConnection connection = await _channels.GetConnectionAsyncLite(
            this.textBox_url.Text,
            "");
                    CancellationToken cancel_token = _cancel.Token;

                    string id = Guid.NewGuid().ToString();
                    GetMessageRequest request = new GetMessageRequest(id,
                        "",
                        "gn:<default>", // "" 表示默认群组
                        "",
                        "",
                        "",
                        "",
                        "",
                        0,
                        1);
                    GetMessageResult result = await connection.GetMessageAsyncLite(
            request,
            new TimeSpan(0, 1, 0),
            cancel_token);

                    if (result.Value == -1)
                    {
                        strError = "检测用户时出错: " + result.ErrorInfo;
                        goto ERROR1;
                    }

                    return true;
                }
            }
            catch (MessageException ex)
            {
                if (ex.ErrorCode == "Unauthorized")
                {
                    strError = "以用户名 '" + ex.UserName + "' 登录时, 用户名或密码不正确";
                    goto ERROR1;
                }
                if (ex.ErrorCode == "HttpRequestException")
                {
                    strError = "dp2MServer URL 不正确，或 dp2MServer 尚未启动";
                    goto ERROR1;
                }
                strError = ex.Message;
                goto ERROR1;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                EnableControls(true);
            }
        ERROR1:
            this.Invoke(new Action(() =>
            {
                MessageBox.Show(this, strError);
            }));
            return false;
        }

        string GetUserName()
        {
            return this.textBox_userName.Text;
        }

        string GetPassword()
        {
            return this.textBox_password.Text;
        }

        // 用面板上的 capo 用户名进行登录
        void _channels_Login(object sender, LoginEventArgs e)
        {
            MessageConnection connection = sender as MessageConnection;

            e.UserName = GetUserName();
            if (string.IsNullOrEmpty(e.UserName) == true)
                throw new Exception("尚未指定用户名，无法进行登录");

            e.Password = GetPassword();
            e.Parameters = "propertyList=biblio_search,libraryUID=install";
        }

        public static string EncryptKey = "dp2capopassword";

        void FillDefaultValue()
        {
            this.textBox_url.Text = "http://dp2003.com:8083/dp2mserver";
            this.textBox_userName.Text = "";
            this.textBox_password.Text = "";
        }

        // 从 CfgDom 中填充信息到控件
        void FillInfo()
        {
            XmlDocument dom = this.CfgDom;

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2mserver") as XmlElement;
            if (element == null)
            {
                element = dom.CreateElement("dp2mserver");
                dom.DocumentElement.AppendChild(element);

                FillDefaultValue();
                return;
            }

            this.textBox_url.Text = element.GetAttribute("url");

            this.textBox_userName.Text = element.GetAttribute("userName");

            string strPassword = Cryptography.Decrypt(element.GetAttribute("password"), EncryptKey);
            this.textBox_password.Text = strPassword;
        }

        public static string GetDisplayText(XmlDocument CfgDom)
        {
            StringBuilder text = new StringBuilder();
            XmlDocument dom = CfgDom;

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2mserver") as XmlElement;
            if (element == null)
                return "";

            text.Append("url=" + element.GetAttribute("url") + "\r\n");
            text.Append("userName=" + element.GetAttribute("userName") + "\r\n");
            return text.ToString();
        }

        // 从控件到 CfgDom
        bool SaveToCfgDom()
        {
            XmlDocument dom = this.CfgDom;

            XmlElement element = dom.DocumentElement.SelectSingleNode("dp2mserver") as XmlElement;
            if (element == null)
            {
                element = dom.CreateElement("dp2mserver");
                dom.DocumentElement.AppendChild(element);
            }

            element.SetAttribute("url", this.textBox_url.Text);

            element.SetAttribute("userName", this.textBox_userName.Text);

            string strPassword = Cryptography.Encrypt(this.textBox_password.Text, EncryptKey);
            element.SetAttribute("password", strPassword);
            return true;
        }

        private void textBox_userName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBox_userName.Text) == true)
            {
                this.button_detect.Enabled = false;
            }
            else
            {
                this.button_detect.Enabled = true;
            }
        }

        private void dp2MServerDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancel.Cancel();
        }

        private void dp2MServerDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private async void button_createUser_Click(object sender, EventArgs e)
        {
            if (this.textBox_confirmManagePassword.Text != this.textBox_password.Text)
            {
                MessageBox.Show(this, "密码 和 确认密码 不一致，请重新输入");
                return;
            }
            await CreateCapoUser();
        }

        // 根据 capo_xxx 用户名构造出对应的 weixin_xxx 用户名
        static string MakeWeixinUserName(string strCapoUserName)
        {
            string strName = "";
            List<string> parts = StringUtil.ParseTwoPart(strCapoUserName, "_");
            if (string.IsNullOrEmpty(parts[1]) == false)
                strName = parts[1];
            else
                strName = strCapoUserName;

            return "weixin_" + strName;
        }

        /*
微信公众号新图书馆dp2mserver账号
命名：weixin_图书馆英文或中文简称（如weixin_cctb,weixin_tjsyzx）
权限：getPatronInfo,searchBiblio,searchPatron,bindPatron,getBiblioInfo,getBiblioSummary,getItemInfo,circulation,getUserInfo,getRes
义务：空
单位：图书馆名称
群组：gn:_lib_bb
gn:_lib_book
gn:_lib_homePage

===
新图书馆安装dp2capo时创建的dp2mserver账号
命名：capo_图书馆英文或中文简称（如capo_cctb,capo_tjsyzx）
权限：空
义务：getPatronInfo,searchBiblio,searchPatron,bindPatron,getBiblioInfo,getBiblioSummary,getItemInfo,circulation,getUserInfo,getRes
单位：图书馆名称
群组：gn:_patronNotify
         * */

        async Task<bool> CreateCapoUser()
        {
            string strError = "";
            EnableControls(false);
            try
            {
                using (MessageConnectionCollection _channels = new MessageConnectionCollection())
                {
                    _channels.Login += _channels_LoginSupervisor;

                    MessageConnection connection = await _channels.GetConnectionAsyncLite(
            this.textBox_url.Text,
            "supervisor");
                    // 记忆用过的超级用户名和密码
                    this.ManagerUserName = connection.UserName;
                    this.ManagerPassword = connection.Password;

                    CancellationToken cancel_token = _cancel.Token;

                    string id = Guid.NewGuid().ToString();

                    string strDepartment = InputDlg.GetInput(
    this,
    "图书馆名",
    "请指定图书馆名: ",
    "",
    this.Font);
                    if (strDepartment == null)
                        return false;

                    bool bEanbleWebCall = false;
                    this.Invoke(new Action(() =>
                    {
                        DialogResult temp_result = MessageBox.Show(this,
    "是否允许 webCall (通过 dp2Router 访问 dp2library)?",
    "安装 dp2Capo",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                        if (temp_result == System.Windows.Forms.DialogResult.Yes)
                            bEanbleWebCall = true;
                    }));

                    List<User> users = new List<User>();

                    User user = new User();
                    user.userName = this.textBox_userName.Text;
                    user.password = this.textBox_password.Text;
                    user.rights = "";
                    // TODO: 看看除了 weixin_xxx 以外是否还有其他请求者需要许可
                    user.duty = ":weixinclient|" + MakeWeixinUserName(this.textBox_userName.Text) + ",getPatronInfo,searchBiblio,searchPatron,bindPatron,getBiblioInfo,getBiblioSummary,getItemInfo,circulation,getUserInfo,getRes,getSystemParameter";
                    if (bEanbleWebCall)
                        user.duty += ",webCall:router";
                    user.groups = new string[] { "gn:_patronNotify" };
                    user.department = strDepartment;
                    user.binding = "ip:[current]";
                    user.comment = "dp2Capo 专用账号";

                    users.Add(user);

                    MessageResult result = await connection.SetUsersAsyncLite("create",
                        users,
                        new TimeSpan(0, 1, 0),
                        cancel_token);

                    if (result.Value == -1)
                    {
                        strError = "创建用户 '" + this.textBox_userName.Text + "' 时出错: " + result.ErrorInfo;
                        goto ERROR1;
                    }

                    return true;
                }
            }
            catch (MessageException ex)
            {
                if (ex.ErrorCode == "Unauthorized")
                {
                    strError = "以用户名 '" + ex.UserName + "' 登录时, 用户名或密码不正确";
                    this.ManagerUserName = "";
                    this.ManagerPassword = "";
                    goto ERROR1;
                }
                if (ex.ErrorCode == "HttpRequestException")
                {
                    strError = "dp2MServer URL 不正确，或 dp2MServer 尚未启动";
                    goto ERROR1;
                }
                strError = ex.Message;
                goto ERROR1;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                EnableControls(true);
            }
        ERROR1:
            this.Invoke(new Action(() =>
            {
                MessageBox.Show(this, strError);
            }));
            return false;
        }

        // 用 supervisor 用户名进行登录
        void _channels_LoginSupervisor(object sender, LoginEventArgs e)
        {
            MessageConnection connection = sender as MessageConnection;

            if (string.IsNullOrEmpty(this.ManagerUserName) == false)
            {
                e.UserName = this.ManagerUserName;
                e.Password = this.ManagerPassword;
                e.Parameters = "propertyList=biblio_search,libraryUID=install";
                return;
            }

            ConfirmSupervisorDialog dlg = new ConfirmSupervisorDialog();
            FontUtil.AutoSetDefaultFont(dlg);
            // dlg.Text = "";
            dlg.ServerUrl = this.textBox_url.Text;
            dlg.Comment = "为在 dp2mserver 服务器上创建图书馆账户，请使用超级用户登录";
            dlg.UserName = e.UserName;
            dlg.Password = e.Password;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            dlg.ShowDialog(this);

            if (dlg.DialogResult == DialogResult.Cancel)
            {
                e.ErrorInfo = "放弃登录";
                return;
            }

            e.UserName = dlg.UserName;
            if (string.IsNullOrEmpty(e.UserName) == true)
                throw new Exception("尚未指定用户名，无法进行登录");

            e.Password = dlg.Password;
            e.Parameters = "propertyList=biblio_search,libraryUID=install";
        }

    }
}
