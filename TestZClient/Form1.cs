using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using TestZClient.Properties;

using static DigitalPlatform.Z3950.ZClient;
using DigitalPlatform.Text;
using DigitalPlatform.Z3950;
using DigitalPlatform.Marc;
using System.Web;
using DigitalPlatform.MarcQuery;
using DigitalPlatform.Net;

namespace TestZClient
{
    public partial class Form1 : Form
    {
        ZClient _zclient = new ZClient();
        public IsbnSplitter _isbnSplitter = null;
        public UseCollection _useList = new UseCollection();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSettings();

            Result result = LoadEnvironment();
            if (result.Value == -1)
                MessageBox.Show(this, result.ErrorInfo);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveSettings();

            if (_zclient != null)
            {
                _zclient.CloseConnection();
                _zclient.Dispose();
            }
        }

        Result LoadEnvironment()
        {
            this.ClearHtml();

            try
            {
                this._isbnSplitter = new IsbnSplitter(Path.Combine(Environment.CurrentDirectory, "rangemessage.xml"));  // "\\isbn.xml"
            }
            catch (FileNotFoundException ex)
            {
                return new Result { Value = -1, ErrorInfo = "装载本地 isbn 规则文件 rangemessage.xml 发生错误 :" + ex.Message };
#if NO
                if (bAutoDownload == true)
                {
                    string strError1 = "";
                    int nRet = this.DownloadDataFile("rangemessage.xml",    // "isbn.xml"
                        out strError1);
                    if (nRet == -1)
                    {
                        strError = strError + "\r\n自动下载文件。\r\n" + strError1;
                        return -1;
                    }
                    goto REDO;
                }
                else
                {
                    return -1;
                }
#endif
            }
            catch (Exception ex)
            {
                return new Result { Value = -1, ErrorInfo = "装载本地 isbn 规则文件发生错误 :" + ex.Message };
            }

            Result result = _useList.Load(Path.Combine(Environment.CurrentDirectory, "bib1use.xml"));
            if (result.Value == -1)
                return result;

            string[] fromlist = this._useList.GetDropDownList();
            this.comboBox_use.Items.AddRange(fromlist);

            return new Result();
        }

        void LoadSettings()
        {
            this.textBox_serverAddr.Text = Settings.Default.serverAddr;
            this.textBox_serverPort.Text = Settings.Default.serverPort;
            this.textBox_database.Text = Settings.Default.databaseNames;
            this.textBox_queryWord.Text = Settings.Default.queryWord;
            this.comboBox_use.Text = Settings.Default.queryUse;

            string strStyle = Settings.Default.authenStyle;
            if (strStyle == "idpass")
                this.radioButton_authenStyleIdpass.Checked = true;
            else
                this.radioButton_authenStyleOpen.Checked = true;

            this.textBox_groupID.Text = Settings.Default.groupID;
            this.textBox_userName.Text = Settings.Default.userName;
            this.textBox_password.Text = Settings.Default.password;

            this.textBox_queryString.Text = Settings.Default.queryString;

            string strQueryStyle = Settings.Default.queryStyle;
            if (strQueryStyle == "easy")
                this.radioButton_query_easy.Checked = true;
            else
                this.radioButton_query_origin.Checked = true;

        }

        void SaveSettings()
        {
            Settings.Default.serverAddr = this.textBox_serverAddr.Text;
            Settings.Default.serverPort = this.textBox_serverPort.Text;
            Settings.Default.databaseNames = this.textBox_database.Text;
            Settings.Default.queryWord = this.textBox_queryWord.Text;
            Settings.Default.queryUse = this.comboBox_use.Text;

            if (this.radioButton_authenStyleIdpass.Checked == true)
                Settings.Default.authenStyle = "idpass";
            else
                Settings.Default.authenStyle = "open";

            Settings.Default.groupID = this.textBox_groupID.Text;
            Settings.Default.userName = this.textBox_userName.Text;
            Settings.Default.password = this.textBox_password.Text;

            Settings.Default.queryString = this.textBox_queryString.Text;

            if (this.radioButton_query_easy.Checked == true)
                Settings.Default.queryStyle = "easy";
            else
                Settings.Default.queryStyle = "origin";

            Settings.Default.Save();
        }

        // 创建只包含一个检索词的简单 XML 检索式
        // 注：这种 XML 检索式不是 Z39.50 函数库必需的。只是用它来方便构造 API 检索式的过程
        public string BuildQueryXml()
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");
            {
                XmlElement node = dom.CreateElement("line");
                dom.DocumentElement.AppendChild(node);

                string strLogic = "OR";
                string strFrom = this.comboBox_use.Text;

                node.SetAttribute("logic", strLogic);
                node.SetAttribute("word", this.textBox_queryWord.Text);
                node.SetAttribute("from", strFrom);
            }

            return dom.OuterXml;
        }

        // 	0: open 1:idPass
        int GetAuthentcationMethod()
        {
            return (this.radioButton_authenStyleIdpass.Checked ? 1 : 0);
        }

        TargetInfo _targetInfo = new TargetInfo();

        long _resultCount = 0;   // 检索命中条数
        int _fetched = 0;   // 已经 Present 获取的条数

        private async void button_search_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.ClearHtml();
            _resultCount = 0;
            _fetched = 0;

            EnableControls(false);

            try
            {
                // 如果 _targetInfo 涉及到的信息字段对比环境没有变化，就持续使用
                if (_targetInfo.HostName != this.textBox_serverAddr.Text
                    || _targetInfo.Port != Convert.ToInt32(this.textBox_serverPort.Text)
                    || string.Join(",", _targetInfo.DbNames) != this.textBox_database.Text
                    || _targetInfo.AuthenticationMethod != GetAuthentcationMethod()
                    || _targetInfo.UserName != this.textBox_userName.Text
                    || _targetInfo.Password != this.textBox_password.Text)
                {
                    _targetInfo = new TargetInfo
                    {
                        HostName = this.textBox_serverAddr.Text,
                        Port = Convert.ToInt32(this.textBox_serverPort.Text),
                        DbNames = StringUtil.SplitList(this.textBox_database.Text).ToArray(),
                        AuthenticationMethod = GetAuthentcationMethod(),
                        GroupID = this.textBox_groupID.Text,
                        UserName = this.textBox_userName.Text,
                        Password = this.textBox_password.Text,
                    };
                    _zclient.CloseConnection();
                }

                IsbnConvertInfo isbnconvertinfo = new IsbnConvertInfo
                {
                    IsbnSplitter = this._isbnSplitter,
                    ConvertStyle =
                    (_targetInfo.IsbnAddHyphen == true ? "addhyphen," : "")
                    + (_targetInfo.IsbnRemoveHyphen == true ? "removehyphen," : "")
                    + (_targetInfo.IsbnForce10 == true ? "force10," : "")
                    + (_targetInfo.IsbnForce13 == true ? "force13," : "")
                    + (_targetInfo.IsbnWild == true ? "wild," : "")
                };

                string strQueryString = "";

                if (this.radioButton_query_easy.Checked)
                {
                    // 创建只包含一个检索词的简单 XML 检索式
                    // 注：这种 XML 检索式不是 Z39.50 函数库必需的。只是用它来方便构造 API 检索式的过程
                    string strQueryXml = BuildQueryXml();
                    // 将 XML 检索式变化为 API 检索式
                    Result result = ZClient.ConvertQueryString(
                        this._useList,
                        strQueryXml,
                        isbnconvertinfo,
                        out strQueryString);
                    if (result.Value == -1)
                    {
                        strError = result.ErrorInfo;
                        goto ERROR1;
                    }

                    this.textBox_queryString.Text = strQueryString; // 便于调试观察
                }
                else
                    strQueryString = this.textBox_queryString.Text;

                REDO_SEARCH:
                {
                    // return Value:
                    //      -1  出错
                    //      0   成功
                    //      1   调用前已经是初始化过的状态，本次没有进行初始化
                    // InitialResult result = _zclient.TryInitialize(_targetInfo).GetAwaiter().GetResult();
                    // InitialResult result = _zclient.TryInitialize(_targetInfo).Result;
                    InitialResult result = await _zclient.TryInitialize(_targetInfo);
                    if (result.Value == -1)
                    {
                        strError = "Initialize error: " + result.ErrorInfo;
                        goto ERROR1;
                    }
                    /*
                this.Invoke(
                    (Action)(() => MessageBox.Show(this, result.ToString()))
                    );
                    */
                }

                // result.Value:
                //		-1	error
                //		0	fail
                //		1	succeed
                // result.ResultCount:
                //      命中结果集内记录条数 (当 result.Value 为 1 时)
                SearchResult search_result = await _zclient.Search(
        strQueryString,
        _targetInfo.DefaultQueryTermEncoding,
        _targetInfo.DbNames,
        _targetInfo.PreferredRecordSyntax,
        "default");
                if (search_result.Value == -1 || search_result.Value == 0)
                {
                    this.AppendHtml("<div class='debug error' >检索出错 " + search_result.ErrorInfo + "</div>");
                    if (search_result.ErrorCode == "ConnectionAborted")
                    {
                        this.AppendHtml("<div class='debug error' >自动重试检索 ...</div>");
                        goto REDO_SEARCH;
                    }
                }
                else
                    this.AppendHtml("<div class='debug green' >检索共命中记录 " + search_result.ResultCount + "</div>");

#if NO
            this.Invoke(
    (Action)(() => MessageBox.Show(this, search_result.ToString()))
    );
#endif
                _resultCount = search_result.ResultCount;

#if NO
                if (_resultCount - _fetched > 0)
                    this.button_nextBatch.Enabled = true;
                else
                    this.button_nextBatch.Enabled = false;
#endif

                await FetchRecords(_targetInfo);

                return;
            }
            finally
            {
                EnableControls(true);
            }
            ERROR1:
            this.AppendHtml("<div class='debug error' >" + HttpUtility.HtmlEncode(strError) + "</div>");
            MessageBox.Show(this, strError);
        }

        void EnableControls(bool bEnable)
        {
            this.button_search.Enabled = bEnable;
            this.button_stop.Enabled = !bEnable;
            this.button_close.Enabled = bEnable;
            if (_resultCount - _fetched > 0)
                this.button_nextBatch.Enabled = bEnable;
            else
                this.button_nextBatch.Enabled = false;

            if (_resultCount == 0)
                this.button_nextBatch.Text = ">> ";
            else
                this.button_nextBatch.Text = ">> " + _fetched + "/" + _resultCount;

            this.textBox_database.Enabled = bEnable;
            this.textBox_groupID.Enabled = bEnable;
            this.textBox_password.Enabled = bEnable;
            //this.textBox_queryString.Enabled = bEnable;
            //this.textBox_queryWord.Enabled = bEnable;
            this.textBox_serverAddr.Enabled = bEnable;
            this.textBox_serverPort.Enabled = bEnable;
            this.textBox_userName.Enabled = bEnable;

            this.groupBox1.Enabled = bEnable;

            SetQueryEnabled(bEnable);
        }

        async Task FetchRecords(TargetInfo targetinfo)
        {
            EnableControls(false);  // 暂时禁用
            try
            {
                if (_resultCount - _fetched > 0)
                {
                    PresentResult present_result = await _zclient.Present(
                        "default",
                        _fetched,
                        Math.Min((int)_resultCount - _fetched, 10),
                        10,
                        "F",
                        targetinfo.PreferredRecordSyntax);
                    if (present_result.Value == -1)
                    {
                        this.Invoke((Action)(() => MessageBox.Show(this, present_result.ToString())));
                    }
                    else
                    {
                        // 把 MARC 记录显示出来
                        AppendMarcRecords(present_result.Records,
                            _zclient.ForcedRecordsEncoding == null ? targetinfo.DefaultRecordsEncoding : _zclient.ForcedRecordsEncoding,
                            _fetched);
                        _fetched += present_result.Records.Count;
                    }
                }
            }
            finally
            {
                EnableControls(true);
            }

#if NO
            if (_resultCount - _fetched > 0)
                this.button_nextBatch.Enabled = true;
            else
                this.button_nextBatch.Enabled = false;

            this.button_nextBatch.Text = ">> " + _fetched + "/" + _resultCount;
#endif
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            _zclient.CloseConnection();
            EnableControls(true);
            MessageBox.Show(this, "通道已切断");
        }

        void AppendMarcRecords(RecordCollection records,
            Encoding encoding,
            int start_index)
        {
            if (records == null)
                return;

            int i = start_index;
            foreach (Record record in records)
            {
                this.AppendHtml("<div class='debug green' >" + (i + 1) + ") ===</div>");

                if (string.IsNullOrEmpty(record.m_strDiagSetID) == false)
                {
                    // 这是诊断记录

                    this.AppendHtml("<div>" + HttpUtility.HtmlEncode(record.ToString()).Replace("\r\n", "<br/>") + "</div>");
                    i++;
                    continue;
                }

                // 把byte[]类型的MARC记录转换为机内格式
                // return:
                //		-2	MARC格式错
                //		-1	一般错误
                //		0	正常
                int nRet = MarcLoader.ConvertIso2709ToMarcString(record.m_baRecord,
                    encoding == null ? Encoding.GetEncoding(936) : encoding,
                    true,
                    out string strMARC,
                    out string strError);
                if (nRet == -1)
                {
                    this.AppendHtml("<div>" + strError + "</div>");
                    i++;
                    continue;
                }

                // 获得 MARC 记录的 HTML 格式字符串
                string strHtml = MarcUtil.GetHtmlOfMarc(strMARC,
                    null,
                    null,
                    false);

                this.AppendHtml(strHtml);
                i++;
            }
        }

        #region 浏览器控件

        /// <summary>
        /// 清除已有的 HTML 显示
        /// </summary>
        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(Environment.CurrentDirectory, "operloghtml.css");

            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";

            string strJs = "";

            {
                HtmlDocument doc = this.webBrowser1.Document;

                if (doc == null)
                {
                    webBrowser1.Navigate("about:blank");
                    doc = webBrowser1.Document;
                }
                doc = doc.OpenNew(true);
            }

            WriteHtml(this.webBrowser1,
                "<html><head>" + strLink + strJs + "</head><body>");
        }


        delegate void Delegate_AppendHtml(string strText);

        /// <summary>
        /// 向 IE 控件中追加一段 HTML 内容
        /// </summary>
        /// <param name="strText">HTML 内容</param>
        public void AppendHtml(string strText)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                Delegate_AppendHtml d = new Delegate_AppendHtml(AppendHtml);
                this.webBrowser1.BeginInvoke(d, new object[] { strText });
                return;
            }

            WriteHtml(this.webBrowser1,
                strText);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.webBrowser1.Document.Window.ScrollTo(0,
    this.webBrowser1.Document.Body.ScrollRectangle.Height);
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
                Navigate(webBrowser, "about:blank");

                doc = webBrowser.Document;
            }

            doc.Write(strHtml);
        }

        // 能处理异常的 Navigate
        internal static void Navigate(WebBrowser webBrowser, string urlString)
        {
            int nRedoCount = 0;
            REDO:
            try
            {
                webBrowser.Navigate(urlString);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Application.DoEvents();
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }

                throw ex;
            }
        }

        #endregion

        // 获得下一批记录
        private async void button_nextBatch_Click(object sender, EventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) != 0)
                await Present();
            else
                await FetchRecords(_targetInfo);
        }

        // 停止检索等操作
        private void button_stop_Click(object sender, EventArgs e)
        {
            this.EnableControls(false);
            this.button_stop.Enabled = false;
            _zclient.CloseConnection();
            EnableControls(true);
        }

        // 多通道测试
        private void MenuItem_multiChannelTest_Click(object sender, EventArgs e)
        {
            MultiChannelForm dlg = new MultiChannelForm();
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ShowDialog(this);
        }

        private void radioButton_query_origin_CheckedChanged(object sender, EventArgs e)
        {
            SetQueryEnabled(true);
        }

        void SetQueryEnabled(bool bEnable)
        {
            this.radioButton_query_easy.Enabled = bEnable;
            this.radioButton_query_origin.Enabled = bEnable;
            if (bEnable)
            {
                if (this.radioButton_query_easy.Checked == true)
                {
                    this.textBox_queryWord.Enabled = true;
                    this.comboBox_use.Enabled = true;
                    this.textBox_queryString.Enabled = false;
                }
                else
                {
                    this.textBox_queryWord.Enabled = false;
                    this.comboBox_use.Enabled = false;
                    this.textBox_queryString.Enabled = true;
                }
            }
            else
            {
                this.textBox_queryWord.Enabled = false;
                this.comboBox_use.Enabled = false;
                this.textBox_queryString.Enabled = false;
            }
        }

        private void MenuItem_escapeString_Click(object sender, EventArgs e)
        {
            EscapeStringDialog dlg = new EscapeStringDialog();
            dlg.ShowDialog(this);
        }

        private async void MenuItem_iso2709LoaderTest_Click(object sender, EventArgs e)
        {
            this.MenuItem_iso2709LoaderTest.Enabled = false;
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();

                dlg.Title = "请指定要装载的 ISO2709 文件名";
                // dlg.FileName = this.textBox_filename.Text;
                dlg.Filter = "ISO2709文件 (*.iso)|*.iso|All files (*.*)|*.*";
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                MarcLoader loader = new MarcLoader(dlg.FileName,
                    Encoding.GetEncoding(936),
                    "marc",
                    (length, current) =>
                    {
                        this.Invoke(
(Action)(() =>
{
    this.toolStripProgressBar1.Maximum = (int)length;
    this.toolStripProgressBar1.Value = (int)current;
})
);
                    }
                    );

                this.ClearHtml();
                await Task.Run(() =>
                {
                    int i = 0;
                    foreach (string strMARC in loader)
                    {
                        this.AppendHtml("<div class='debug green' >" + (i + 1) + ") ===</div>");

                        // 获得 MARC 记录的 HTML 格式字符串
                        string strHtml = MarcUtil.GetHtmlOfMarc(strMARC,
                               null,
                               null,
                               false);

                        this.AppendHtml(strHtml);
                        i++;
                    }
                });
            }
            finally
            {
                this.MenuItem_iso2709LoaderTest.Enabled = true;
            }
        }

        string _uiState;

        async Task Present()
        {
            PresentDialog dlg = new PresentDialog();
            dlg.UiState = _uiState;
            dlg.ShowDialog(this);
            _uiState = dlg.UiState;
            if (dlg.DialogResult == DialogResult.Cancel)
                return;

            int nStart = Convert.ToInt32(dlg.PresentStart);
            int nCount = Convert.ToInt32(dlg.PresentCount);
            EnableControls(false);  // 暂时禁用
            try
            {
                PresentResult present_result = await _zclient.Present(
                    dlg.ResultSetName,
                    nStart,
                    nCount,
                    10,
                    "F",
                    _targetInfo.PreferredRecordSyntax);
                if (present_result.Value == -1)
                {
                    this.Invoke((Action)(() => MessageBox.Show(this, present_result.ToString())));
                }
                else
                {
                    // 把 MARC 记录显示出来
                    AppendMarcRecords(present_result.Records,
                        _zclient.ForcedRecordsEncoding == null ? _targetInfo.DefaultRecordsEncoding : _zclient.ForcedRecordsEncoding,
                        _fetched);
                    _fetched += present_result.Records.Count;
                }
            }
            finally
            {
                EnableControls(true);
            }
        }

        private async void MenuItem_singlePresent_Click(object sender, EventArgs e)
        {
            await Present();
        }
    }
}
