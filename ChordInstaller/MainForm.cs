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
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Win32;
using Ionic.Zip;

using dp2Capo.Install;

using DigitalPlatform;
using DigitalPlatform.Forms;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Drawing;
using DigitalPlatform.ServiceProcess;

// TODO: 自动升级 dp2mserver
// TODO: 自动升级 dp2router

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

            Refresh_dp2capo_MenuItems();

            this.BeginInvoke(new Action<object, EventArgs>(MenuItem_autoUpgrade_Click), this, new EventArgs());
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancel.Cancel();
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
            string strError = "";
            int nRet = 0;

            this._floatingMessage.Text = "正在升级 dp2Capo - V2 V3 桥接模块 ...";

            try
            {
                AppendSectionTitle("升级 dp2Capo 开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = ServiceUtil.GetPathOfService("dp2CapoService");
                if (string.IsNullOrEmpty(strExePath) == true)
                {
                    strError = "dp2Capo 未曾安装过";
                    goto ERROR1;
                }
                strExePath = StringUtil.Unquote(strExePath, "\"\"");

                AppendString("正在停止 dp2Capo 服务 ...\r\n");
                nRet = ServiceUtil.StopService("dp2CapoService",
                    TimeSpan.FromMinutes(2),
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                AppendString("dp2Capo 服务已经停止\r\n");

                string strZipFileName = Path.Combine(this.DataDir, "capo_app.zip");

                AppendString("更新可执行文件 ...\r\n");

                // 更新可执行目录
                // return:
                //      -1  出错
                //      0   没有必要刷新
                //      1   已经刷新
                nRet = RefreshBinFiles(
                    false,
                    strZipFileName,
                    Path.GetDirectoryName(strExePath),
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                AppendString("正在重新启动 dp2Capo 服务 ...\r\n");
                nRet = ServiceUtil.StartService("dp2CapoService",
                    TimeSpan.FromMinutes(2),
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                AppendString("dp2Capo 服务启动成功\r\n");

                AppendSectionTitle("升级 dp2Capo 结束");
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
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
            string strExePath = ServiceUtil.GetPathOfService("dp2CapoService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strExePath = StringUtil.Unquote(strExePath, "\"\"");

                List<string> data_dirs = InstallDialog.GetInstanceDataDirByBinDir(Path.GetDirectoryName(strExePath));
                int i = 0;
                foreach (string data_dir in data_dirs)
                {
                    string strInstanceName = (i + 1).ToString();
                    ToolStripMenuItem subItem = new ToolStripMenuItem("'" + strInstanceName + "' - " + data_dir);
                    subItem.Tag = strProductName + "|" + data_dir;
                    subItem.Click += new EventHandler(subItem_Click);
                    menuItem.DropDownItems.Add(subItem);
                    i++;
                }
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

        private void MenuItem_dp2capo_startService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            AppendString("正在获得可执行文件目录 ...\r\n");

            Application.DoEvents();

            string strExePath = ServiceUtil.GetPathOfService("dp2CapoService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                strError = "dp2Capo 未曾安装过";
                goto ERROR1;
            }
            strExePath = StringUtil.Unquote(strExePath, "\"\"");

            AppendString("正在启动 dp2Capo 服务 ...\r\n");
            Application.DoEvents();

            nRet = ServiceUtil.StartService("dp2CapoService",
                TimeSpan.FromMinutes(2),
                out strError);
            if (nRet == -1)
                goto ERROR1;
            AppendString("dp2Capo 服务成功启动\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2capo_stopService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            AppendString("正在获得可执行文件目录 ...\r\n");

            Application.DoEvents();

            string strExePath = ServiceUtil.GetPathOfService("dp2CapoService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                strError = "dp2Capo 未曾安装过";
                goto ERROR1;
            }
            strExePath = StringUtil.Unquote(strExePath, "\"\"");

            AppendString("正在停止 dp2Capo 服务 ...\r\n");
            Application.DoEvents();

            nRet = ServiceUtil.StopService("dp2CapoService",
                TimeSpan.FromMinutes(2),
                out strError);
            if (nRet == -1)
                goto ERROR1;
            AppendString("dp2Capo 服务已经停止\r\n");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2capo_tools_installService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            AppendSectionTitle("注册 Windows Service 开始");

            Application.DoEvents();

            string strExePath = ServiceUtil.GetPathOfService("dp2CapoService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strError = "dp2Capo 已经注册为 Windows Service，无法重复进行注册";
                goto ERROR1;
            }

            // program files (x86)/digitalplatform/dp2capo
            string strProgramDir = Global.GetProductDirectory("dp2capo");

            strExePath = Path.Combine(strProgramDir, "dp2capo.exe");

            if (File.Exists(strExePath) == false)
            {
                strError = "dp2capo.exe 尚未复制到目标位置，无法进行注册";
                goto ERROR1;
            }

            nRet = ServiceUtil.InstallService(strExePath,
                true,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            AppendSectionTitle("注册 Windows Service 结束");

            this.Refresh_dp2capo_MenuItems();
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        private void MenuItem_dp2capo_tools_uninstallService_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            AppendSectionTitle("注销 Windows Service 开始");

            Application.DoEvents();

            string strExePath = ServiceUtil.GetPathOfService("dp2CapoService");
            if (string.IsNullOrEmpty(strExePath) == true)
            {
                strError = "dp2Capo 尚未安装和注册为 Windows Service，无法进行注销";
                goto ERROR1;
            }
            strExePath = StringUtil.Unquote(strExePath, "\"\"");

            // 注销 Windows Service
            nRet = ServiceUtil.InstallService(strExePath,
    false,
    out strError);
            if (nRet == -1)
                goto ERROR1;

            AppendSectionTitle("注销 Windows Service 结束");

            this.Refresh_dp2capo_MenuItems();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2capo_uninstall_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this._floatingMessage.Text = "正在卸载 dp2Capo - V2 V3 桥接模块 ...";

            try
            {
                AppendSectionTitle("卸载 dp2Capo 开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = ServiceUtil.GetPathOfService("dp2CapoService");
                if (string.IsNullOrEmpty(strExePath) == true)
                {
                    strError = "dp2Capo 尚未安装和注册为 Windows Service，无法进行注销";
                    goto ERROR1;
                }
                strExePath = StringUtil.Unquote(strExePath, "\"\"");

                string strProgramDir = Path.GetDirectoryName(strExePath);

                {
                    AppendString("正在停止 dp2Capo 服务 ...\r\n");

                    nRet = ServiceUtil.StopService("dp2CapoService",
                        TimeSpan.FromMinutes(2),
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    AppendString("dp2Capo 服务已经停止\r\n");
                }


                {
                    dp2Capo.Install.InstallDialog dlg = new dp2Capo.Install.InstallDialog();
                    FontUtil.AutoSetDefaultFont(dlg);
                    dlg.Text = "dp2Capo - 彻底卸载所有实例和数据目录";
                    // dlg.Comment = "下列实例将被全部卸载。请仔细确认。一旦卸载，全部数据目录、数据库和实例信息将被删除，并且无法恢复。";
                    dlg.UninstallMode = true;
                    dlg.BinDir = strProgramDir;
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    dlg.ShowDialog(this);

                    if (dlg.DialogResult == DialogResult.Cancel)
                    {
                        MessageBox.Show(this,
                            "已放弃卸载全部实例和数据目录。仅仅卸载了可执行程序。");
                    }
                    else
                    {
                        AppendString("已删除全部数据目录\r\n");
                    }
                }


                // 探测 .exe 是否为新版本。新版本中 Installer.Uninstall 动作不会删除数据目录

                AppendString("注销 Windows Service\r\n");

                // 注销 Windows Service
                nRet = ServiceUtil.InstallService(strExePath,
        false,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                AppendString("删除程序目录\r\n");

            // 删除程序目录
            REDO_DELETE_PROGRAMDIR:
                try
                {
                    PathUtil.DeleteDirectory(Path.GetDirectoryName(strExePath));
                }
                catch (Exception ex)
                {
                    DialogResult temp_result = MessageBox.Show(this,
    "删除程序目录 '" + Path.GetDirectoryName(strExePath) + "' 出错：" + ex.Message + "\r\n\r\n是否重试?\r\n\r\n(Retry: 重试; Cancel: 不重试，继续后续卸载过程)",
    "卸载 dp2Capo",
    MessageBoxButtons.RetryCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    if (temp_result == DialogResult.Retry)
                        goto REDO_DELETE_PROGRAMDIR;
                }

                AppendSectionTitle("卸载 dp2Capo 结束");
                this.Refresh_dp2capo_MenuItems();
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2capo_instanceManagement_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            bool bControl = Control.ModifierKeys == Keys.Control;
            bool bInstalled = true;

            this._floatingMessage.Text = "正在配置 dp2Capo 实例 ...";

            try
            {
                AppendSectionTitle("配置实例开始");

                AppendString("正在获得可执行文件目录 ...\r\n");

                Application.DoEvents();

                string strExePath = ServiceUtil.GetPathOfService("dp2CapoService");
                strExePath = StringUtil.Unquote(strExePath, "\"\"");
                if (string.IsNullOrEmpty(strExePath) == true)
                {
                    if (bControl == false)
                    {
                        strError = "dp2Capo 未曾安装过";
                        goto ERROR1;
                    }
                    bInstalled = false;
                }

                if (bInstalled == true)
                {
                    AppendString("正在停止 dp2Capo 服务 ...\r\n");

                    nRet = ServiceUtil.StopService("dp2CapoService",
                        TimeSpan.FromMinutes(2),
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    AppendString("dp2Capo 服务已经停止\r\n");
                }

                try
                {
                    dp2Capo.Install.InstallDialog dlg = new dp2Capo.Install.InstallDialog();
                    FontUtil.AutoSetDefaultFont(dlg);
                    dlg.Text = "配置 dp2Capo 的实例";
                    dlg.BinDir = Path.GetDirectoryName(strExePath);
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    dlg.ShowDialog(this);

                    // TODO: 是否必须要创建至少一个实例?
                    if (dlg.DialogResult == DialogResult.Cancel)
                    {
                        AppendSectionTitle("放弃配置实例 ...");
                        return;
                    }

                    if (string.IsNullOrEmpty(dlg.DebugInfo) == false)
                        AppendString("配置实例时的调试信息:\r\n" + dlg.DebugInfo + "\r\n");
                }
                finally
                {
                    if (bInstalled == true)
                    {
                        AppendString("正在重新启动 dp2Capo 服务 ...\r\n");
                        nRet = ServiceUtil.StartService("dp2CapoService",
                            TimeSpan.FromMinutes(2),
                            out strError);
                        if (nRet == -1)
                        {
                            AppendString("dp2Capo 服务启动失败: " + strError + "\r\n");
                            MessageBox.Show(this, strError);
                        }
                        else
                        {
                            AppendString("dp2Capo 服务启动成功\r\n");
                        }
                    }

                    AppendSectionTitle("配置实例结束");
                }

                Refresh_dp2capo_MenuItems();
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            AppendString("出错: " + strError + "\r\n");
            MessageBox.Show(this, strError);
        }

        private void MenuItem_dp2capo_openAppDir_Click(object sender, EventArgs e)
        {
            string strExePath = ServiceUtil.GetPathOfService("dp2CapoService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strExePath = StringUtil.Unquote(strExePath, "\"\"");
                try
                {
                    System.Diagnostics.Process.Start(Path.GetDirectoryName(strExePath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                }
            }
            else
            {
                MessageBox.Show(this, "dp2Capo 未曾安装过");
            }

        }

        private void MenuItem_autoUpgrade_Click(object sender, EventArgs e)
        {
            // string strError = "";
            List<string> names = new List<string>();
            string strZipFileName = "";
            string strExePath = "";

            // ---
            strExePath = ServiceUtil.GetPathOfService("dp2CapoService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strZipFileName = Path.Combine(this.DataDir, "capo_app.zip");
                if (DetectChange(strZipFileName) == true)
                    names.Add("dp2Capo");
            }

            if (names.Count > 0)
            {
                DialogResult result = MessageBox.Show(this,
"下列模块有新版本：\r\n" + StringUtil.MakePathList(names, "\r\n") + "\r\n\r\n是否升级？",
"chordInstaller",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.No)
                    return;
                foreach (string name in names)
                {
                    if (name == "dp2Capo")
                        MenuItem_dp2capo_upgrade_Click(this, new EventArgs());
                }
            }
            else
            {
                AppendSectionTitle("目前没有任何新版本需要升级");
            }
        }

        // 探测一个 zip 文件上次用于升级以后是否发生了新变化
        // return:
        //      false   没有发生变化
        //      true    发生了变化
        bool DetectChange(string strZipFileName,
            string strTargetDir = null)
        {
            string strOldTimestamp = "";

            string strEntry = Path.GetFileName(strZipFileName);
            if (strTargetDir != null)
                strEntry += "|" + strTargetDir;

            // 由于数据目录经常变化，所以要使用纯文件名
            int nRet = _versionManager.GetFileVersion(strEntry, out strOldTimestamp);
            string strNewTimestamp = File.GetLastWriteTime(strZipFileName).ToString();
            if (strOldTimestamp == strNewTimestamp)
                return false;
            return true;
        }

        private void MenuItem_displayDigitalPlatformEventLog_Click(object sender, EventArgs e)
        {

        }

        private void MenuItem_sendDebugInfos_Click(object sender, EventArgs e)
        {
            Task.Run(() => ZipDebugInfos());
        }

        void ZipDebugInfos()
        {
            string strError = "";
            int nRet = 0;

            bool bControl = Control.ModifierKeys == Keys.Control;

            this._floatingMessage.Text = "正在打包事件日志信息 ...";
            try
            {
                string strZipFileName = Path.Combine(this.TempDir, "chordinstaller_eventlog.zip");

                List<EventLog> logs = new List<EventLog>();

                logs.Add(new EventLog("DigitalPlatform", ".", "*"));
                logs.Add(new EventLog("Application"));

                // "最近31天" "最近十年" "最近七天"

                nRet = PackageEventLog(logs,
                    strZipFileName,
                    bControl ? "最近十年" : "最近31天",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                try
                {
                    System.Diagnostics.Process.Start(this.TempDir);
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    goto ERROR1;
                }
            }
            finally
            {
                this._floatingMessage.Text = "";
            }
            return;
        ERROR1:
            this.Invoke(new Action(() =>
            {
                MessageBox.Show(this, strError);
            }));
        }

        int PackageEventLog(List<EventLog> logs,
    string strZipFileName,
    string strRangeName,
    out string strError)
        {
            strError = "";
            int nRet = 0;

            this._floatingMessage.Text = "正在打包事件日志 ...";
            // Application.DoEvents();

            //this.toolStripProgressBar_main.Style = ProgressBarStyle.Marquee;
            //this.toolStripProgressBar_main.Visible = true;

            try
            {
                string strTempDir = this.TempDir;

                PathUtil.TryClearDir(strTempDir);

                List<string> filenames = new List<string>();

                foreach (EventLog log in logs)
                {
                    // 创建 eventlog_digitalplatform.txt 文件
                    string strEventLogFilename = Path.Combine(strTempDir, "eventlog_" + log.LogDisplayName + ".txt");

                    //
                    //
                    nRet = MakeWindowsLogFile(log,
                        strEventLogFilename,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    if (nRet > 0)
                        filenames.Add(strEventLogFilename);
                    else
                        File.Delete(strEventLogFilename);
                }

                // 创建一个描述了安装的各个实例和环境情况的文件
                string strDescriptionFilename = Path.Combine(strTempDir, "description.txt");
                try
                {
                    //    stop.SetMessage("正在准备 description.txt 文件 ...");

                    using (StreamWriter sw = new StreamWriter(strDescriptionFilename, false, Encoding.UTF8))
                    {
                        sw.Write(GetEnvironmentDescription());
                    }
                }
                catch (Exception ex)
                {
                    strError = "输出 description.txt 时出现异常: " + ex.Message;
                    return -1;
                }

                filenames.Add(strDescriptionFilename);

                // TODO: 是否复制整个数据目录？ 需要避免复制日志文件和其他尺寸很大的文件

                // 复制错误日志文件和其他重要文件
                List<string> dates = MakeDates(strRangeName); // "最近31天""最近十年""最近七天"

                // *** dp2Capo 各个 instance
                string strExePath = ServiceUtil.GetPathOfService("dp2CapoService");
                strExePath = StringUtil.Unquote(strExePath, "\"\"");

                if (string.IsNullOrEmpty(strExePath) == false)
                {

                    string strLibraryTempDir = Path.Combine(strTempDir, "dp2capo");
                    PathUtil.CreateDirIfNeed(strLibraryTempDir);

                    List<string> data_dirs = InstallDialog.GetInstanceDataDirByBinDir(Path.GetDirectoryName(strExePath));

                    int i = 0;
                    foreach (string data_dir in data_dirs)
                    {
                        string strInstanceDir = strLibraryTempDir;
                        string strInstanceName = (i + 1).ToString();
                        if (string.IsNullOrEmpty(strInstanceName) == false)
                        {
                            strInstanceDir = Path.Combine(strLibraryTempDir, "instance_" + strInstanceName);
                            PathUtil.CreateDirIfNeed(strInstanceDir);
                        }

                        // 复制 capo.xml
                        {
                            string strFilePath = Path.Combine(data_dir, "capo.xml");
                            string strTargetFilePath = Path.Combine(strInstanceDir, "capo.xml");
                            if (File.Exists(strFilePath) == true)
                            {
                                File.Copy(strFilePath,
                                    strTargetFilePath);
                                filenames.Add(strTargetFilePath);
                            }
                        }

                        foreach (string date in dates)
                        {
                            _cancel.Token.ThrowIfCancellationRequested();

                            string strFilePath = Path.Combine(data_dir, "log/log_" + date + ".txt");
                            if (File.Exists(strFilePath) == false)
                                continue;
                            string strTargetFilePath = Path.Combine(strInstanceDir, "log_" + date + ".txt");

                            this._floatingMessage.Text = ("正在复制文件 " + strFilePath);

                            File.Copy(strFilePath, strTargetFilePath);
                            filenames.Add(strTargetFilePath);
                        }
                    }
                }

                if (filenames.Count == 0)
                    return 0;

                if (filenames.Count > 0)
                {
                    // this.toolStripProgressBar_main.Style = ProgressBarStyle.Continuous;

                    bool bRangeSetted = false;
                    using (ZipFile zip = new ZipFile(Encoding.UTF8))
                    {
                        foreach (string filename in filenames)
                        {
                            _cancel.Token.ThrowIfCancellationRequested();

                            string strShortFileName = filename.Substring(strTempDir.Length + 1);

                            this._floatingMessage.Text = ("正在压缩 " + strShortFileName);
                            string directoryPathInArchive = Path.GetDirectoryName(strShortFileName);
                            zip.AddFile(filename, directoryPathInArchive);
                        }


                        this._floatingMessage.Text = ("正在写入压缩文件 ...");

                        zip.SaveProgress += (s, e) =>
                        {
                            Application.DoEvents();
                            if (_cancel.Token.IsCancellationRequested)
                            {
                                e.Cancel = true;
                                return;
                            }

                            if (e.EventType == ZipProgressEventType.Saving_AfterWriteEntry)
                            {
                                if (bRangeSetted == false)
                                {
                                    //stop.SetProgressRange(0, e.EntriesTotal);
                                    bRangeSetted = true;
                                }

                                //stop.SetProgressValue(e.EntriesSaved);
                            }
                        };

                        zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                        zip.Save(strZipFileName);

                        // stop.HideProgress();

                        _cancel.Token.ThrowIfCancellationRequested();
                    }

                    this._floatingMessage.Text = ("正在删除中间文件 ...");

                    // 删除原始文件
                    foreach (string filename in filenames)
                    {
                        File.Delete(filename);
                    }

                    // 删除子目录
                    PathUtil.DeleteDirectory(Path.Combine(strTempDir, "dp2capo"));
                }
            }
            catch (Exception ex)
            {
                strError = "PackageEventLog() 出现异常: " + ExceptionUtil.GetExceptionMessage(ex);
                return -1;
            }
            finally
            {
                // this.toolStripProgressBar_main.Style = ProgressBarStyle.Continuous;
            }

            return 0;
        }

        #region MakeDates()

        List<string> MakeDates(string strName)
        {
            List<string> filenames = new List<string>();

            string strStartDate = "";
            string strEndDate = "";

            if (strName == "本周")
            {
                DateTime now = DateTime.Now;
                int nDelta = (int)now.DayOfWeek; // 0-6 sunday - saturday
                DateTime start = now - new TimeSpan(nDelta, 0, 0, 0);

                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "本月")
            {
                DateTime now = DateTime.Now;
                strEndDate = DateTimeUtil.DateTimeToString8(now);
                strStartDate = strEndDate.Substring(0, 6) + "01";
            }
            else if (strName == "本年")
            {
                DateTime now = DateTime.Now;
                strEndDate = DateTimeUtil.DateTimeToString8(now);
                strStartDate = strEndDate.Substring(0, 4) + "0101";
            }
            else if (strName == "最近七天" || strName == "最近7天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(7 - 1, 0, 0, 0);

                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三十天" || strName == "最近30天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(30 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三十一天" || strName == "最近31天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(31 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三百六十五天" || strName == "最近365天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(365 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近十年" || strName == "最近10年")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(10 * 365 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else
            {
                throw new Exception("无法识别的周期 '" + strName + "'");
            }

            string strWarning = "";
            string strError = "";
            // 根据日期范围，发生日志文件名
            // parameters:
            //      strStartDate    起始日期。8字符
            //      strEndDate  结束日期。8字符
            // return:
            //      -1  错误
            //      0   成功
            int nRet = MakeDates(strStartDate,
                strEndDate,
        out filenames,
        out strWarning,
        out strError);
            if (nRet == -1)
                goto ERROR1;

            return filenames;
        ERROR1:
            throw new Exception(strError);
        }

        static int MakeDates(string strStartDate,
    string strEndDate,
    out List<string> dates,
    out string strWarning,
    out string strError)
        {
            dates = new List<string>();
            strError = "";
            strWarning = "";
            int nRet = 0;

            if (String.Compare(strStartDate, strEndDate) > 0)
            {
                strError = "起始日期 '" + strStartDate + "' 不应大于结束日期 '" + strEndDate + "'。";
                return -1;
            }

            string strDate = strStartDate;

            for (; ; )
            {
                dates.Add(strDate);

                string strNextDate = "";
                // 获得（理论上）下一个日志文件名
                // return:
                //      -1  error
                //      0   正确
                //      1   正确，并且strLogFileName已经是今天的日子了
                nRet = GetNextDate(strDate,
                    out strNextDate,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 1)
                {
                    if (String.Compare(strDate, strEndDate) < 0)
                    {
                        strWarning = "因日期范围的尾部 " + strEndDate + " 超过今天(" + DateTime.Now.ToLongDateString() + ")，部分日期被略去...";
                        break;
                    }
                }

                Debug.Assert(strDate != strNextDate, "");

                strDate = strNextDate;
                if (String.Compare(strDate, strEndDate) > 0)
                    break;
            }

            return 0;
        }

        // 获得（理论上）下一个日志文件名
        // return:
        //      -1  error
        //      0   正确
        //      1   正确，并且 strNextDate 已经是今天的日子了
        static int GetNextDate(string strDate,
            out string strNextDate,
            out string strError)
        {
            strError = "";
            strNextDate = "";
            int nRet = 0;

            DateTime time = DateTimeUtil.Long8ToDateTime(strDate);

            DateTime now = DateTime.Now;

            // 正规化时间
            nRet = RoundTime("day",
                ref now,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = RoundTime("day",
                ref time,
                out strError);
            if (nRet == -1)
                return -1;

            bool bNow = false;
            if (time >= now)
                bNow = true;

            time = time + new TimeSpan(1, 0, 0, 0); // 后面一天

            strNextDate = time.Year.ToString().PadLeft(4, '0')
            + time.Month.ToString().PadLeft(2, '0')
            + time.Day.ToString().PadLeft(2, '0');

            if (bNow == true)
                return 1;

            return 0;
        }

        // 按照时间单位,把时间值零头去除,正规化,便于后面计算差额
        static int RoundTime(string strUnit,
            ref DateTime time,
            out string strError)
        {
            strError = "";

            time = time.ToLocalTime();
            if (strUnit == "day")
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    12, 0, 0, 0);
            }
            else if (strUnit == "hour")
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    time.Hour, 0, 0, 0);
            }
            else
            {
                strError = "未知的时间单位 '" + strUnit + "'";
                return -1;
            }
            time = time.ToUniversalTime();
            return 0;
        }


        #endregion

        public static Version GetIisVersion()
        {
            using (RegistryKey componentsKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\InetStp", false))
            {
                if (componentsKey != null)
                {
                    int majorVersion = (int)componentsKey.GetValue("MajorVersion", -1);
                    int minorVersion = (int)componentsKey.GetValue("MinorVersion", -1);

                    if (majorVersion != -1 && minorVersion != -1)
                    {
                        return new Version(majorVersion, minorVersion);
                    }
                }

                return new Version(0, 0);
            }
        }

        // 获得环境描述字符串
        string GetEnvironmentDescription()
        {
            // string strError = "";

            StringBuilder text = new StringBuilder();
            text.Append("信息创建时间:\t" + DateTime.Now.ToString() + "\r\n");
            text.Append("当前操作系统信息:\t" + Environment.OSVersion.ToString() + "\r\n");
            text.Append("当前操作系统版本号:\t" + Environment.OSVersion.Version.ToString() + "\r\n");
            text.Append("本机 MAC 地址:\t" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress()) + "\r\n");
            text.Append("IIS 版本号:\t" + GetIisVersion().ToString() + "\r\n");

            // *** dp2Capo
            string strExePath = ServiceUtil.GetPathOfService("dp2CapoService");
            if (string.IsNullOrEmpty(strExePath) == false)
            {
                strExePath = StringUtil.Unquote(strExePath, "\"\"");

                text.Append("\r\n*** dp2Capo\r\n");
                text.Append("可执行文件目录:\t" + Path.GetDirectoryName(strExePath) + "\r\n");

                List<string> data_dirs = InstallDialog.GetInstanceDataDirByBinDir(Path.GetDirectoryName(strExePath));
                int i = 0;
                foreach (string data_dir in data_dirs)
                {
                    text.Append("\r\n实例 " + (i + 1) + "\r\n");
                    // text.Append("实例名:\t" + strInstanceName + "\r\n");
                    text.Append("数据目录:\t" + data_dir + "\r\n");
                    i++;
                }
            }
            return text.ToString();
        }

        CancellationTokenSource _cancel = new CancellationTokenSource();

        int MakeWindowsLogFile(EventLog log,
    string strEventLogFilename,
    out string strError)
        {
            strError = "";
            int nLines = 0;
            try
            {
                this._floatingMessage.Text = ("正在准备 Windows 事件日志 " + log.LogDisplayName + "...");

                using (StreamWriter sw = new StreamWriter(strEventLogFilename, false, Encoding.UTF8))
                {
                    foreach (EventLogEntry entry in log.Entries)
                    {
                        _cancel.Token.ThrowIfCancellationRequested();

                        string strText = "*\r\n"
                            + entry.Source + " \t"
                            + entry.EntryType.ToString() + " \t"
                            + entry.TimeGenerated.ToString() + "\r\n"
                            + entry.Message + "\r\n\r\n";

                        sw.Write(strText);
                        nLines++;
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "输出 Windows 日志 " + log.LogDisplayName + "的信息时出现异常: " + ex.Message;
                return -1;
            }

            return nLines;
        }

        private void MenuItem_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
