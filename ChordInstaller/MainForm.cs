using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Deployment.Application;
using System.Web;

using DigitalPlatform.Forms;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform;
using DigitalPlatform.Drawing;
using DigitalPlatform.ServiceProcess;
using Ionic.Zip;
using System.Threading;
using dp2Capo.Install;

namespace ChordInstaller
{
    public partial class MainForm : Form
    {
        FileVersionManager _versionManager = new FileVersionManager();

        FloatingMessageForm _floatingMessage = null;

        ConfigSetting Setting { get; set; }

        /// <summary>
        /// 数据目录
        /// </summary>
        public string DataDir = "";

        /// <summary>
        /// 用户目录
        /// </summary>
        public string UserDir = "";

        public string TempDir = "";

        public string UserLogDir = "";


        public MainForm()
        {
            InitializeComponent();

            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.AutoHide = false;
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);
            }

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            FontUtil.AutoSetDefaultFont(this);
            ClearForPureTextOutputing(this.webBrowser1);

            InitialDir();

            this.Setting = new ConfigSetting(Path.Combine(this.UserDir, "settings.xml"), true);
            this.Setting.LoadFormStates(this,
"mainformstate",
FormWindowState.Normal);

            _versionManager.Load(Path.Combine(this.UserDir, "file_version.xml"));

            DisplayCopyRight();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_versionManager != null)
                _versionManager.AutoSave();

            if (this.Setting != null)
            {
                Setting.SaveFormStates(this,
        "mainformstate");

                Setting.Save();
                Setting = null;	// 避免后面再用这个对象
            }

            if (_floatingMessage != null)
                _floatingMessage.Close();
        }

        void InitialDir()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length >= 2)
            {
#if LOG
                WriteLibraryEventLog("命令行参数=" + string.Join(",", args), EventLogEntryType.Information);
#endif
                // MessageBox.Show(string.Join(",", args));
                for (int i = 1; i < args.Length; i++)
                {
                    string strArg = args[i];
                    if (StringUtil.HasHead(strArg, "datadir=") == true)
                    {
                        this.DataDir = strArg.Substring("datadir=".Length);
#if LOG
                        WriteLibraryEventLog("从命令行参数得到, this.DataDir=" + this.DataDir, EventLogEntryType.Information);
#endif
                    }
                    else if (StringUtil.HasHead(strArg, "userdir=") == true)
                    {
                        this.UserDir = strArg.Substring("userdir=".Length);
#if LOG
                        WriteLibraryEventLog("从命令行参数得到, this.UserDir=" + this.UserDir, EventLogEntryType.Information);
#endif
                    }
                }
            }

            if (string.IsNullOrEmpty(this.DataDir) == true)
            {
                if (ApplicationDeployment.IsNetworkDeployed == true)
                {
#if LOG
                    WriteLibraryEventLog("从网络安装启动", EventLogEntryType.Information);
#endif
                    // MessageBox.Show(this, "network");
                    this.DataDir = Application.LocalUserAppDataPath;
                }
                else
                {
#if LOG
                    WriteLibraryEventLog("绿色安装方式启动", EventLogEntryType.Information);
#endif
                    // MessageBox.Show(this, "no network");
                    this.DataDir = Environment.CurrentDirectory;
                }
#if LOG
                WriteLibraryEventLog("普通方法得到, this.DataDir=" + this.DataDir, EventLogEntryType.Information);
#endif
            }

            if (string.IsNullOrEmpty(this.UserDir) == true)
            {
                this.UserDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "chordinstaller_v1");
#if LOG
                WriteLibraryEventLog("普通方法得到, this.UserDir=" + this.UserDir, EventLogEntryType.Information);
#endif
            }
            PathUtil.CreateDirIfNeed(this.UserDir);

            this.TempDir = Path.Combine(this.UserDir, "temp");
            PathUtil.CreateDirIfNeed(this.TempDir);

            // 2015/8/8
            this.UserLogDir = Path.Combine(this.UserDir, "log");
            PathUtil.CreateDirIfNeed(this.UserLogDir);
        }

        // 写入日志文件。每天创建一个单独的日志文件
        public void WriteErrorLog(string strText)
        {
            FileUtil.WriteErrorLog(
                this.UserLogDir,
                this.UserLogDir,
                strText,
                "log_",
                ".txt");
        }

        private void MenuItem_openUserFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.UserDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        private void MenuItem_openDataFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.DataDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }

        }

        private void MenuItem_openProgramFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Environment.CurrentDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }

        }

        #region console
        /// <summary>
        /// 将浏览器控件中已有的内容清除，并为后面输出的纯文本显示做好准备
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        public static void ClearForPureTextOutputing(WebBrowser webBrowser)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
            }

            string strHead = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\"><head>"
                // + "<link rel='stylesheet' href='"+strCssFileName+"' type='text/css'>"
    + "<style media='screen' type='text/css'>"
    + "body { font-family:Microsoft YaHei; background-color:#555555; color:#eeeeee; } "
    + "</style>"
    + "</head><body>";

            doc = doc.OpenNew(true);
            doc.Write(strHead + "<pre style=\"font-family:Consolas; \">");  // Calibri
        }

        /// <summary>
        /// 将 HTML 信息输出到控制台，显示出来。
        /// </summary>
        /// <param name="strText">要输出的 HTML 字符串</param>
        public void WriteToConsole(string strText)
        {
            WriteHtml(this.webBrowser1, strText);
        }

        /// <summary>
        /// 将文本信息输出到控制台，显示出来
        /// </summary>
        /// <param name="strText">要输出的文本字符串</param>
        public void WriteTextToConsole(string strText)
        {
            WriteHtml(this.webBrowser1, HttpUtility.HtmlEncode(strText));
        }

        /// <summary>
        /// 向一个浏览器控件中追加写入 HTML 字符串
        /// 不支持异步调用
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        /// <param name="strHtml">HTML 字符串</param>
        public static void WriteHtml(WebBrowser webBrowser,
    string strHtml)
        {

            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
#if NO
                webBrowser.DocumentText = "<h1>hello</h1>";
                doc = webBrowser.Document;
                Debug.Assert(doc != null, "");
#endif
            }

            // doc = doc.OpenNew(true);
            doc.Write(strHtml);
        }


        void AppendSectionTitle(string strText)
        {
            AppendCrLn();
            AppendString("*** " + strText + " ***\r\n");
            AppendCurrentTime();
            AppendCrLn();
        }

        void AppendCurrentTime()
        {
            AppendString("*** " + DateTime.Now.ToString() + " ***\r\n");
        }

        void AppendCrLn()
        {
            AppendString("\r\n");
        }

        // 线程安全
        void AppendString(string strText)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                this.webBrowser1.Invoke(new Action<string>(AppendString), strText);
                return;
            }
            this.WriteTextToConsole(strText);
            ScrollToEnd();
        }

        void ScrollToEnd()
        {
#if NO
            if (this.webBrowser1.Document != null
                && this.webBrowser1.Document.Window != null
                && this.webBrowser1.Document.Body != null)
                this.webBrowser1.Document.Window.ScrollTo(
                    0,
                    this.webBrowser1.Document.Body.ScrollRectangle.Height);
#endif
            this.webBrowser1.ScrollToEnd();
        }

        #endregion

        void DisplayCopyRight()
        {
            AppendString("chordInstaller - dp2 图书馆集成系统 V3 安装实用工具\r\n");
            AppendString("版本: " + Program.ClientVersion + "\r\n");
            AppendString("数字平台(北京)软件有限责任公司\r\n"
                + "(C)2015 年开始以 Apache License Version 2.0 方式开源\r\n"
                + "http://github.com/digitalplatform/chord\r\n");
            AppendString("\r\n");
        }

        private void MenuItem_dp2capo_install_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this._floatingMessage.Text = "正在安装 dp2Capo - V2 V3 桥接模块 ...";

            try
            {
                AppendSectionTitle("安装 dp2Capo 开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = ServiceUtil.GetPathOfService("dp2CapoService");
                if (string.IsNullOrEmpty(strExePath) == false)
                {
                    strError = "dp2Capo 已经安装过了，不能重复安装";
                    goto ERROR1;
                }

                // program files (x86)/digitalplatform/dp2capo
                string strProgramDir = Global.GetProductDirectory("dp2capo");

                PathUtil.CreateDirIfNeed(strProgramDir);

                string strZipFileName = Path.Combine(this.DataDir, "capo_app.zip");

                AppendString("安装可执行文件 ...\r\n");

                // 更新可执行目录
                // return:
                //      -1  出错
                //      0   没有必要刷新
                //      1   已经刷新
                nRet = RefreshBinFiles(
                    false,
                    strZipFileName,
                    strProgramDir,
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 创建实例
                AppendString("创建实例 ...\r\n");

                try
                {
                    dp2Capo.Install.InstallDialog dlg = new dp2Capo.Install.InstallDialog();
                    FontUtil.AutoSetDefaultFont(dlg);

                    dlg.BinDir = strProgramDir;
                    // dlg.DataZipFileName = Path.Combine(this.DataDir, "kernel_data.zip");
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    dlg.ShowDialog(this);

                    // TODO: 是否必须要创建至少一个实例?
                    if (dlg.DialogResult == DialogResult.Cancel)
                    {
                        AppendSectionTitle("放弃创建实例 ...");
                        return;
                    }

                    if (string.IsNullOrEmpty(dlg.DebugInfo) == false)
                        AppendString("创建实例时的调试信息:\r\n" + dlg.DebugInfo + "\r\n");

#if NO
                    if (dlg.Changed == true)
                    {
                        // 兑现修改

                    }
#endif
                }
                finally
                {
                    AppendString("创建实例结束 ...\r\n");
                }

                // 注册为 Windows Service
                strExePath = Path.Combine(strProgramDir, "dp2capo.exe");

                AppendString("注册 Windows Service ...\r\n");

                nRet = ServiceUtil.InstallService(strExePath,
        true,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

#if NO
            AppendString("启动 dp2kernel 服务 ...\r\n");
            nRet = StartService("dp2KernelService",
    out strError);
            if (nRet == -1)
                goto ERROR1;
            AppendString("dp2kernel 服务启动成功\r\n");
#endif

                AppendSectionTitle("安装 dp2Capo 结束");
                Refresh_dp2capo_MenuItems();
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2capo_upgrade_Click(object sender, EventArgs e)
        {

        }

        // 刷新菜单状态
        void Refresh_dp2capo_MenuItems()
        {
            string strExePath = ServiceUtil.GetPathOfService("dp2CapoService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                this.MenuItem_dp2capo_install.Enabled = true;
                this.MenuItem_dp2capo_upgrade.Enabled = false;
            }
            else
            {
                this.MenuItem_dp2capo_install.Enabled = false;
                this.MenuItem_dp2capo_upgrade.Enabled = true;
            }

            this.MenuItem_dp2capo_openDataDir.DropDownItems.Clear();
            AddMenuItem(MenuItem_dp2capo_openDataDir, "dp2Capo");

        }

        void AddMenuItem(ToolStripMenuItem menuItem, string strProductName)
        {
            for (int i = 0; ; i++)
            {
                break;

                string strInstanceName = "";
                string strDataDir = "";
                string strCertificatSN = "";

                string[] existing_urls = null;
#if NO
                bool bRet = InstallHelper.GetInstanceInfo(strProductName,
                    i,
                    out strInstanceName,
                    out strDataDir,
                    out existing_urls,
                    out strCertificatSN);
                if (bRet == false)
                    break;
#endif
                ToolStripMenuItem subItem = new ToolStripMenuItem("'" + strInstanceName + "' - " + strDataDir);
                subItem.Tag = strProductName + "|" + strDataDir;
                subItem.Click += new EventHandler(subItem_Click);
                menuItem.DropDownItems.Add(subItem);
            }

            if (menuItem.DropDownItems.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
        }

        void subItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            string strText = menuItem.Tag as string;

            string strProductName = "";
            string strDataDir = "";
            StringUtil.ParseTwoPart(strText, "|", out strProductName, out strDataDir);

            if (string.IsNullOrEmpty(strDataDir) == false)
            {
                try
                {
                    System.Diagnostics.Process.Start(strDataDir);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                }
            }
        }

        // 更新可执行目录
        // parameters:
        //      excludes    要排除的文件名。纯文件名。必须为小写形态
        // return:
        //      -1  出错
        //      0   没有必要刷新
        //      1   已经刷新
        int RefreshBinFiles(
            bool bAuto,
            string strZipFileName,
            string strTargetDir,
            List<string> excludes,
            out string strError)
        {
            strError = "";

            if (excludes != null)
            {
                foreach (string filename in excludes)
                {
                    if (filename.ToLower() != filename)
                    {
                        strError = "excludes 中的字符串必须为小写形态";
                        return -1;
                    }
                }
            }

            string strOldTimestamp = "";
            int nRet = _versionManager.GetFileVersion(Path.GetFileName(strZipFileName), out strOldTimestamp);
            string strNewTimestamp = File.GetLastWriteTime(strZipFileName).ToString();
            if (bAuto == true && strOldTimestamp == strNewTimestamp)
            {
                strError = "没有更新";
                return 0;
            }

            // 要求在 xxx_app.zip 内准备要安装的可执行程序文件
            try
            {
                using (ZipFile zip = ZipFile.Read(strZipFileName))
                {
                    for (int i = 0; i < zip.Count; i++)
                    {
                        ZipEntry e = zip[i];

                        if (excludes != null && (e.Attributes & FileAttributes.Directory) == 0)
                        {
                            if (excludes.IndexOf(Path.GetFileName(e.FileName).ToLower()) != -1)
                                continue;
                        }

                        string strPart = GetSubPath(e.FileName);
                        string strFullPath = Path.Combine(strTargetDir, strPart);

                        e.FileName = strPart;

                        if ((e.Attributes & FileAttributes.Directory) == 0)
                        {
                            ExtractFile(e, strTargetDir);
                            AppendString("更新文件 " + strFullPath + "\r\n");
                        }
                        else
                            e.Extract(strTargetDir, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

#if NO
            int nRet = PathUtil.CopyDirectory(strTempDataDir,
    this.KernelDataDir,
    true,
    out strError);
            if (nRet == -1)
            {
                strError = "拷贝临时目录 '" + strTempDataDir + "' 到数据目录 '" + this.KernelDataDir + "' 时发生错误：" + strError;
                return -1;
            }
#endif
            _versionManager.SetFileVersion(Path.GetFileName(strZipFileName), strNewTimestamp);
            _versionManager.AutoSave();
            return 1;
        }

        void ExtractFile(ZipEntry e, string strTargetDir)
        {
            string strTempDir = this.TempDir;

            string strTempPath = Path.Combine(strTempDir, Path.GetFileName(e.FileName));
            string strTargetPath = Path.Combine(strTargetDir, e.FileName);

            using (FileStream stream = new FileStream(strTempPath, FileMode.Create))
            {
                e.Extract(stream);
            }

            int nErrorCount = 0;
            for (; ; )
            {
                try
                {
                    // 确保目标目录已经创建
                    PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strTargetPath));

                    File.Copy(strTempPath, strTargetPath, true);
                }
                catch (Exception ex)
                {
                    if (nErrorCount > 10)
                    {
                        DialogResult result = MessageBox.Show(this,
"复制文件 " + strTempPath + " 到 " + strTargetPath + " 的过程中出现错误: " + ex.Message + "。\r\n\r\n是否要重试？",
"dp2Installer",
MessageBoxButtons.RetryCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                        {
                            throw new Exception("复制文件 " + strTargetPath + " 到 " + strTargetPath + " 的过程中出现错误: " + ex.Message);
                        }
                        nErrorCount = 0;
                    }

                    nErrorCount++;
                    Thread.Sleep(1000);
                    continue;
                }
                break;
            }
            File.Delete(strTempPath);
        }

        // 去掉第一级路经
        static string GetSubPath(string strPath)
        {
            int nRet = strPath.IndexOfAny(new char[] { '/', '\\' }, 0);
            if (nRet == -1)
                return "";
            return strPath.Substring(nRet + 1);
        }
    }
}
