using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.LibraryRestClient;

namespace dp2Mini
{
    public partial class MainForm : Form
    {
        // 主要的通道池，用于当前服务器
        public LibraryChannelPool _channelPool = new LibraryChannelPool();


        /// <summary>
        /// 当前连接的服务器的图书馆名
        /// </summary>
        public string LibraryName = "";


        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this._channelPool.BeforeLogin += _channelPool_BeforeLogin;

            LoginForm dlg = new LoginForm();
            if (dlg.ShowDialog(this) == DialogResult.Cancel)
            {
                this.Close();
                return;
            }

            this.toolStripStatusLabel_loginName.Text = dlg.Username;

            int nRet = GetLibraryInfo();
            if (nRet == 0)
                this.Text += "[" + this.LibraryName + "]";
            else if (nRet == -1)
                this.Close();

            nRet = InitialArrivedDbProperties();
            if (nRet == -1)
                this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void _channelPool_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == true)
            {
                e.UserName = Properties.Settings.Default.cfg_library_username;

                e.Password = Properties.Settings.Default.cfg_library_password;

                if (!string.IsNullOrEmpty(e.UserName))
                    return;
            }


            if (!string.IsNullOrEmpty(e.ErrorInfo))
            {
                MessageBox.Show(this, e.ErrorInfo);
            }

            IWin32Window owner = null;

            if (sender is IWin32Window)
                owner = sender as IWin32Window;
            else
                owner = this;

            LoginForm dlg = null;
            this.Invoke((Action)(() =>
            {
                dlg = SetDefaultAccount(
                    e.LibraryServerUrl,
                    owner);
            }));
            if (dlg == null)
            {
                e.Cancel = true;
                return;
            }

            e.UserName = dlg.Username;
            e.Password = dlg.Password;
            e.Parameters = "type=worker,client=dp2Mini|" + Program.ClientVersion;
        }

        LoginForm SetDefaultAccount(string strServerUrl,
            IWin32Window owner)
        {
            LoginForm loginForm = new LoginForm();

            if (String.IsNullOrEmpty(strServerUrl))
            {
                loginForm.LibraryUrl = Properties.Settings.Default.cfg_library_url;
            }
            else
            {
                loginForm.LibraryUrl = strServerUrl;
            }

            if (owner == null)
                owner = this;

            loginForm.Username = Properties.Settings.Default.cfg_library_username;
            loginForm.IsSavePassword = Properties.Settings.Default.cfg_savePassword;

            if (loginForm.IsSavePassword)
            {
                loginForm.Password = Properties.Settings.Default.cfg_library_password;
            }
            else
            {
                loginForm.Password = "";
            }

            loginForm.ShowDialog(owner);

            if (loginForm.DialogResult == DialogResult.Cancel)
                return null;

            return loginForm;
        }


        List<LibraryChannel> _channelList = new List<LibraryChannel>();

        public LibraryChannel GetChannel(string strServerUrl = ".",
            string strUserName = ".")
        {
            if (strServerUrl == ".")
                strServerUrl = Properties.Settings.Default.cfg_library_url;
            if (strUserName == ".")
                strUserName = Properties.Settings.Default.cfg_library_username;

            LibraryChannel channel = this._channelPool.GetChannel(strServerUrl, strUserName);
            _channelList.Add(channel);
            return channel;
        }

        public void ReturnChannel(LibraryChannel channel)
        {
            this._channelPool.ReturnChannel(channel);
            _channelList.Remove(channel);
        }



        /// <summary>
        /// 获得图书名称
        /// </summary>
        /// <returns>
        /// <para> -1: 出错，不希望继续以后的操作 </para>
        /// <para> 0:  成功</para>
        /// <para> 1:  出错，但希望继续后面的操作</para>
        /// </returns>
        public int GetLibraryInfo()
        {
            REDO:
            string strError = "";
            LibraryChannel channel = this.GetChannel();
            try
            {
                GetSystemParameterResponse response = channel.GetSystemParameter("library", "name");

                long lRet = response.GetSystemParameterResult.Value;
                strError = response.GetSystemParameterResult.ErrorInfo;
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得图书馆名称发生错误：" + strError;
                    goto ERROR1;
                }

                this.LibraryName = response.strValue;
            }
            finally
            {
                this.ReturnChannel(channel);
            }

            return 0;
            ERROR1:
            DialogResult result = MessageBox.Show(this,
                strError + "\r\n\r\n是否重试?",
                "dp2Circulation",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
        }


        public string ArrivedDbName
        {
            get;
            private set;
        }

        // 初始化预约到书库的相关属性
        public int InitialArrivedDbProperties()
        {
            REDO:

            LibraryChannel channel = this.GetChannel();

            string strError = "";

            try
            {
                GetSystemParameterResponse response = channel.GetSystemParameter("arrived", "dbname");

                long lRet = response.GetSystemParameterResult.Value;
                strError = response.GetSystemParameterResult.ErrorInfo;
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得预约到书库名过程发生错误：" + strError;
                    goto ERROR1;
                }

                this.ArrivedDbName = response.strValue;
            }
            finally
            {
                this.ReturnChannel(channel);
            }
            return 0;
            ERROR1:
            DialogResult result = MessageBox.Show(this,
                strError + "\r\n\r\n是否重试?",
                "dp2Mini",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == System.Windows.Forms.DialogResult.Yes)
                goto REDO;
            if (result == DialogResult.No)
                return 1;   // 出错，但希望继续后面的操作

            return -1;  // 出错，不希望继续以后的操作
        }

        private void toolStripMenuItem_prep_Click(object sender, EventArgs e)
        {
            PrepForm prepForm = new PrepForm()
            {
                MdiParent = this,
                Text = "备书"
            };
            prepForm.Show();
        }

        private void toolStripMenuItem_setting_Click(object sender, EventArgs e)
        {
            SettingForm dlg = new SettingForm();
            dlg.ShowDialog(this);
        }


        public void SetMessage(string text)
        {
            this.toolStripStatusLabel_message.Text = text;
        }

        private void toolStripButton_prep_Click(object sender, EventArgs e)
        {
            this.toolStripMenuItem_prep_Click(sender, e);
        }


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string strError = "";

            string line = null;
            StringBuilder sb = new StringBuilder();
            using (StreamReader reader = new StreamReader("print.txt", Encoding.UTF8))
            {
                while ((line = reader.ReadLine())!=null)
                {
                    sb.Append("<p>").Append(line).Append("</p>").AppendLine();
                }
            }

            using (StreamWriter writer = new StreamWriter("print.xml", false, Encoding.UTF8))
            {
                writer.Write(WrapString(sb.ToString()));
            }

            CardPrintForm form = new CardPrintForm();
            form.PrinterInfo = new PrinterInfo();
            form.CardFilename = "print.xml";  // 卡片文件名

            form.WindowState = FormWindowState.Minimized;
            form.Show();
            int nRet = form.PrintFromCardFile(false);
            if (nRet == -1)
            {
                form.WindowState = FormWindowState.Normal;
                strError = strError + "\r\n\r\n以下内容未能成功打印:\r\n" + sb.ToString();
                goto ERROR1;
            }
            form.Close();
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        static string WrapString(string strText)
        {
            string strPrefix = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
                + "<root>\r\n"
                + "<pageSetting width='190'>\r\n"
                + "  <font name=\"微软雅黑\" size=\"8\" style=\"\" />\r\n"
                + "  <p align=\"left\" indent='-60'/>\r\n"
                + "</pageSetting>\\\r\n"
                + "<document padding=\"0,0,0,0\">\r\n"
                + "  <column width=\"auto\" padding='60,0,0,0'>\r\n";

            string strPostfix = "</column></document></root>";

            return strPrefix + strText + strPostfix;
        }
    }
}
