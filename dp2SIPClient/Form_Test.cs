
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using log4net;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Common;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Marc;
using DigitalPlatform.SIP2;
using DigitalPlatform.SIP2.Request;
using DigitalPlatform.SIP2.Response;
using DigitalPlatform.Xml;
using System.Threading.Tasks;

namespace dp2SIPClient
{
    public partial class Form_Test : Form
    {

        public Form_Test(string info)
        {
            InitializeComponent();

#if NO
            // Stop初始化
            _stopManager.Initial((object)this.button_stpp,
                (object)this.toolStripStatusLabel1,
                (object)this.toolStripProgressBar1);
            _stop = new DigitalPlatform.Stop();
            _stop.Register(this._stopManager, true);	// 和容器关联
            this.Progress.SetMessage(info);

#endif
        }

        #region 进度条与停止 按钮

#if NO
        /// <summary>
        /// Stop 管理器
        /// </summary>
        public DigitalPlatform.StopManager _stopManager = new DigitalPlatform.StopManager();
        internal DigitalPlatform.Stop _stop = null;
        // 进度条和停止按钮
        public Stop Progress
        {
            get
            {
                return this._stop;
            }
        }

        public void DoStop(object sender, StopEventArgs e)
        {
            foreach (LibraryChannel channel in _channelList)
            {
                if (channel != null)
                    channel.Abort();
            }
        }
#endif

        #endregion

        #region dp2通道

        private LibraryChannelPool _channelPool = new LibraryChannelPool();
        List<LibraryChannel> _channelList = new List<LibraryChannel>();

        // parameters:
        //      style    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public LibraryChannel GetChannel()
        {
            LibraryChannel channel = this._channelPool.GetChannel(this.dp2ServerUrl,
                this.dp2Username);
            // channel.Idle += channel_Idle;
            _channelList.Add(channel);

            // TODO: 检查数组是否溢出
            return channel;
        }

#if NO
        void channel_Idle(object sender, IdleEventArgs e)
        {
            Application.DoEvents();
        }
#endif

        private void ReturnChannel(LibraryChannel channel)
        {
            // channel.Idle -= channel_Idle;

            this._channelPool.ReturnChannel(channel);
            _channelList.Remove(channel);
        }

        void _channelPool_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            e.LibraryServerUrl = this.dp2ServerUrl;
            e.UserName = this.dp2Username;
            e.Password = this.dp2Password;
            e.Parameters = "type=worker,client=dp2SIPClient|0.01";

            e.SavePasswordLong = true;
        }

        private string dp2ServerUrl
        {
            get
            {
                return Properties.Settings.Default.dp2ServerUrl;
            }
        }

        private string dp2Username
        {
            get
            {
                return Properties.Settings.Default.dp2Username;
            }
        }

        private string dp2Password
        {
            get
            {
                return Properties.Settings.Default.dp2Password;
            }
        }

        private void 参数配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_SettingForAuto dlg = new Form_SettingForAuto();
            dlg.ShowDialog(this);
        }

        #endregion

        #region 界面信息
        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public void EnableControls(bool bEnable)
        {
            this.Invoke(new Action(() =>
            {
                this.button_createTestEnv.Enabled = bEnable;
                this.button_deleteTestEnv.Enabled = bEnable;
                this.button_checkin.Enabled = bEnable;
                this.button_checkin_dup.Enabled = bEnable;
                this.button_checkout.Enabled = bEnable;
                this.button_checkout_dup.Enabled = bEnable;
                this.button_checkoutin.Enabled = bEnable;
                this.button_checkoutin_customer.Enabled = bEnable;
                this.button_itemInfo.Enabled = bEnable;
                this.button_login.Enabled = bEnable;
                this.button_patronInfo.Enabled = bEnable;
                this.button_renew.Enabled = bEnable;
                this.button_SCStatus.Enabled = bEnable;
                this.button_stpp.Enabled = bEnable;
            }));
        }


        private void Print(string text)
        {
            this.Invoke(new Action(() =>
            {
                if (this.txtInfo.Text != "")
                    this.txtInfo.Text += "\r\n";
                this.txtInfo.Text += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + text;
            }));
        }

        // 清空消息
        public void ClearInfo()
        {
            this.Invoke(new Action(() =>
            {
                this.txtInfo.Text = "";
            }));
        }

        private void 创建流通权限ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            LibraryChannel channel = this.GetChannel();
            EnableControls(false);
            try
            {
                // 获取流通权限
                string strRightsTableXml = "";
                nRet = GetRightsTableInfo(channel, out strRightsTableXml, out strError);
                if (nRet == -1)
                    goto ERROR1;
                strRightsTableXml = "<rightsTable>" + strRightsTableXml + "</rightsTable>";
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strRightsTableXml);
                }
                catch (Exception ex)
                {
                    strError = "strRightsTableXml装入XMLDOM时发生错误：" + ex.Message;
                    goto ERROR1;
                }

                // 先删除原来测试自动增加权限
                nRet = this.RemoveTestRightsTable(channel, dom, out strError);
                if (nRet == -1)
                    goto ERROR1;

                //增加测试用权限
                nRet = this.AddTestRightsTable(channel, dom, out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                EnableControls(true);
                this.ReturnChannel(channel);
            }
            ERROR1:
            MessageBoxShow(this, strError);
        }

        #endregion


        #region 初始化测试环境

        public string C_ReaderDbName = "_测试读者";
        public string C_BiblioDbName = "_测试用中文图书";
        public string C_Location = "_测试流通库";
        public const string C_CalenderName = "_测试日历";
        public string C_PatronType = "测试-读者类型";
        public string C_BookType = "测试-图书类型";

        // 删除测试环境
        private void button_deleteTestEnv_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string error = "";
                int nRet = this.DeleteTestEnv(out error);
                if (nRet == -1)
                {
                    MessageBoxShow(this, "删除测试环境出错：" + error);
                    return;
                }

                MessageBoxShow(this, "删除测试环境完成");
                return;
            });
        }

        // 删除测试环境
        public int DeleteTestEnv(out string error)
        {
            error = "";
            int nRet = 0;

            // 检查登录信息
            if (string.IsNullOrEmpty(this.dp2ServerUrl) == true
                || string.IsNullOrEmpty(this.dp2Username) == true)
            {
                error = "尚未配置dp2系统登录信息";
                return -1;
            }
            this._channelPool.BeforeLogin -= _channelPool_BeforeLogin;
            this._channelPool.BeforeLogin += _channelPool_BeforeLogin;


            //===
            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

#if NO
            Progress.Style = StopStyle.EnableHalfStop;
            Progress.OnStop -= new StopEventHandler(this.DoStop);
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial(info);
#endif
            string info = "开始删除测试环境 ...";

            LogManager.Logger.Info(info);
            // Progress.BeginLoop();
            EnableControls(false);
            try
            {
                // 删除书目库
                info = "正在删除测试用书目库 ...";
                ProgressSetMessage(info);
                LogManager.Logger.Info(info);
                string strOutputInfo = "";
                long lRet = channel.ManageDatabase(
                    // _stop,
                    "delete",
                    C_BiblioDbName,    // strDatabaseNames,
                    "",
                    out strOutputInfo,
                    out error);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }


                // 删除读者库
                info = "正在删除测试用读者库 ...";
                ProgressSetMessage(info);
                LogManager.Logger.Info(info);
                lRet = channel.ManageDatabase(
                   // _stop,
                   "delete",
                   C_ReaderDbName,    // strDatabaseNames,
                   "",
                   out strOutputInfo,
                   out error);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }



                // *** 删除馆藏地
                info = "正在删除馆藏地 ...";
                ProgressSetMessage(info);
                LogManager.Logger.Info(info);
                List<DigitalPlatform.CirculationClient.ManageHelper.LocationItem> items = new List<DigitalPlatform.CirculationClient.ManageHelper.LocationItem>();
                items.Add(new DigitalPlatform.CirculationClient.ManageHelper.LocationItem("", "_测试阅览室", true, true));
                items.Add(new DigitalPlatform.CirculationClient.ManageHelper.LocationItem("", "_测试流通库", true, true));

                nRet = ManageHelper.AddLocationTypes(
                    channel,
                    // this.Progress,
                    "remove",
                    items,
                    out error);
                if (nRet == -1)
                    goto ERROR1;

                //***删除工作日历
                info = "正在删除工作日历 ...";
                ProgressSetMessage(info);
                LogManager.Logger.Info(info);
                CalenderInfo[] infos1 = null;
                lRet = channel.GetCalendar(
                    // this.Progress,
                    "get",
                    C_CalenderName,
                    0,
                    -1,
                    out infos1,
                    out error);
                if (lRet == -1)
                    goto ERROR1;
                if (lRet > 0)
                {
                    CalenderInfo cInfo = new CalenderInfo();
                    cInfo.Name = C_CalenderName;
                    cInfo.Range = "20170101-20191231";
                    cInfo.Comment = "";
                    cInfo.Content = "";
                    lRet = channel.SetCalendar(
                       // _stop,
                       "delete",
                       cInfo,
                       out error);
                    if (lRet == -1)
                        goto ERROR1;
                }



                // ***删除权限流通权限
                info = "正在删除流通权限 ...";
                ProgressSetMessage(info);
                LogManager.Logger.Info(info);

                nRet = this.RemoveTestRightsTable(channel, null, out error);
                if (nRet == -1)
                    goto ERROR1;

                return 0;
            }
            catch (Exception ex)
            {
                error = "Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
#if NO
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
                Progress.HideProgress();
#endif
                EnableControls(true);

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
            }


            ERROR1:

            LogManager.Logger.Error(error);
            return -1;
        }


        // 初始化测试环境
        private void button_createTestEnv_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string error = "";
                int nRet = 0;
                long lRet = 0;
                string strOutputInfo = "";
                string info = "";


                //先删除测试环境
                nRet = this.DeleteTestEnv(out error);
                if (nRet == -1)
                {
                    error = "删除测试环境出错：" + error;
                    goto ERROR1;
                }

                //===


                LibraryChannel channel = this.GetChannel();
                TimeSpan old_timeout = channel.Timeout;
                channel.Timeout = TimeSpan.FromMinutes(10);

#if NO
            Progress.Style = StopStyle.EnableHalfStop;
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial(info);
#endif
                info = "开始初始化测试环境 ...";
                LogManager.Logger.Info(info);

                // Progress.BeginLoop();
                EnableControls(false);
                try
                {
                    // *** 定义测试所需的馆藏地
                    info = "正在定义测试所需的馆藏地 ...";
                    ProgressSetMessage(info);
                    LogManager.Logger.Info(info);

                    List<DigitalPlatform.CirculationClient.ManageHelper.LocationItem> items = new List<DigitalPlatform.CirculationClient.ManageHelper.LocationItem>();
                    items.Add(new DigitalPlatform.CirculationClient.ManageHelper.LocationItem("", "_测试阅览室", true, true));
                    items.Add(new DigitalPlatform.CirculationClient.ManageHelper.LocationItem("", "_测试流通库", true, true));
                    nRet = ManageHelper.AddLocationTypes(
                        channel,
                        // this.Progress,
                        "add",
                        items,
                        out error);
                    if (nRet == -1)
                        goto ERROR1;

                    //***创建工作日历
                    info = "正在创建工作日历 ...";
                    ProgressSetMessage(info);
                    LogManager.Logger.Info(info);

                    CalenderInfo cInfo = new CalenderInfo();
                    cInfo.Name = C_CalenderName;
                    cInfo.Range = "20170101-20191231";
                    cInfo.Comment = "";
                    cInfo.Content = "";
                    lRet = channel.SetCalendar(
                       // _stop,
                       "new",
                       cInfo,
                       out error);
                    if (lRet == -1)
                        goto ERROR1;

                    // ***创建流通权限
                    info = "正在创建测试所需的流通权限 ...";
                    ProgressSetMessage(info);
                    LogManager.Logger.Info(info);

                    nRet = this.AddTestRightsTable(channel, null, out error);
                    if (nRet == -1)
                        goto ERROR1;

                    // ***创建测试所需的书目库
                    info = "正在创建测试用书目库 ...";
                    ProgressSetMessage(info);
                    LogManager.Logger.Info(info);
                    // 创建一个书目库
                    // parameters:
                    // return:
                    //      -1  出错
                    //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                    //      1   成功创建
                    nRet = ManageHelper.CreateBiblioDatabase(
                        channel,
                        // this.Progress,
                        C_BiblioDbName,
                        "book",
                        "unimarc",
                        out error);
                    if (nRet == -1)
                        goto ERROR1;

                    // 创建书目记录
                    info = "正在创建书目记录和册记录 ...";
                    ProgressSetMessage(info);
                    LogManager.Logger.Info(info);

                    nRet = this.CreateBiblioRecord(channel, C_BiblioDbName, out error);
                    if (nRet == -1)
                        goto ERROR1;

                    // ***创建测试所需的读者库
                    info = "正在创建测试用读者库 ...";
                    ProgressSetMessage(info);
                    LogManager.Logger.Info(info);

                    XmlDocument database_dom = new XmlDocument();
                    database_dom.LoadXml("<root />");
                    // 创建读者库
                    ManageHelper.CreateReaderDatabaseNode(database_dom,
                        C_ReaderDbName,
                        "",
                        true);
                    lRet = channel.ManageDatabase(
                        // this._stop,
                        "create",
                        "",
                        database_dom.OuterXml,
                        out strOutputInfo,
                        out error);
                    if (lRet == -1)
                        goto ERROR1;

                    info = "正在创建测试读者记录 ...";
                    ProgressSetMessage(info);
                    LogManager.Logger.Info(info);
                    lRet = this.CreateReaderRecord(channel, out error);
                    if (lRet == -1)
                        goto ERROR1;

                    info = "初始化测试环境完成";
                    ProgressSetMessage(info);
                    LogManager.Logger.Info(info);
                    MessageBoxShow(this, info);
                    return;
                }
                catch (Exception ex)
                {
                    error = "Exception: " + ExceptionUtil.GetExceptionText(ex);
                    goto ERROR1;
                }
                finally
                {
#if NO
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
                Progress.HideProgress();
#endif
                    EnableControls(true);

                    channel.Timeout = old_timeout;
                    this.ReturnChannel(channel);
                }


                ERROR1:
                info = "初始化测试环境出错：" + error;
                LogManager.Logger.Info(info);
                MessageBoxShow(this, info);
                return;
            });
        }


        // 创建书目记录与册记录
        int CreateBiblioRecord(LibraryChannel channel,
            string strBiblioDbName,
            out string strError)
        {
            strError = "";

            int barcordStart = 1;

            for (int i = 0; i < 10; i++)
            {
                string strTitle = "测试题名-" + i;

                MarcRecord record = new MarcRecord();
                record.add(new MarcField('$', "200  $a" + strTitle));
                record.add(new MarcField('$', "690  $aI247.5"));
                record.add(new MarcField('$', "701  $a测试著者"));
                string strMARC = record.Text;

                string strMarcSyntax = "unimarc";
                string strXml = "";
                int nRet = MarcUtil.Marc2Xml(strMARC,
                    strMarcSyntax,
                    out strXml,
                    out strError);
                if (nRet == -1)
                    return -1;

                string strPath = strBiblioDbName + "/?";
                byte[] baTimestamp = null;
                byte[] baNewTimestamp = null;
                string strOutputPath = "";

                long lRet = channel.SetBiblioInfo(
                    // _stop,
                    "new",
                    strPath,
                    "xml",
                    strXml,
                    baTimestamp,
                    "",
                    "",
                    out strOutputPath,
                    out baNewTimestamp,
                    out strError);
                if (lRet == -1)
                {
                    strError = "保存书目记录 '" + strPath + "' 时出错: " + strError;
                    return -1;
                }

                //// 创建册记录
                //List<string> refids = CreateEntityRecords(entity_form, 10);
                EntityInfo[] entities = null;
                entities = new EntityInfo[5];
                for (int j = 0; j < entities.Length; j++)
                {
                    EntityInfo info = new EntityInfo();
                    info.RefID = Guid.NewGuid().ToString();
                    info.Action = "new";
                    info.Style = "";

                    info.OldRecPath = "";
                    info.OldRecord = "";
                    info.OldTimestamp = null;

                    info.NewRecPath = "";
                    info.NewRecord = "";
                    info.NewTimestamp = null;

                    entities[j] = info;

                    /*
<dprms:item path="中文图书实体/87" timestamp="2a3a427665d5d4080000000000000010" xmlns:dprms="http://dp2003.com/dprms">
  <parent>43</parent> 
  <refID>8e05d74b-650e-42f8-99cc-45442150c115</refID> 
  <barcode>DPB000051</barcode> 
  <location>方洲小学/图书馆</location> 
  <seller>新华书店</seller> 
  <source>本馆经费</source> 
  <price>CNY10.00</price> 
  <batchNo>201707</batchNo> 
  <accessNo>I17(198.4)/Y498</accessNo> 
  <bookType>普通</bookType>
</dprms:item>
                     */
                    XmlDocument itemDom = new XmlDocument();
                    itemDom.LoadXml("<root />");
                    XmlNode root = itemDom.DocumentElement;

                    string strTargetBiblioRecID = GetRecordID(strOutputPath);
                    DomUtil.SetElementText(root, "parent", strTargetBiblioRecID);

                    string barcode = "_B" + barcordStart.ToString().PadLeft(6, '0');// i.ToString().PadLeft(2, '0')+j.ToString().PadLeft(3,'0');
                    DomUtil.SetElementText(root, "barcode", barcode);
                    DomUtil.SetElementText(root, "location", C_Location);
                    DomUtil.SetElementText(root, "batchNo", "test001");
                    DomUtil.SetElementText(root, "bookType", C_BookType);


                    info.NewRecord = itemDom.DocumentElement.OuterXml;

                    barcordStart++;
                }

                EntityInfo[] errorinfos = null;

                lRet = channel.SetEntities(
                     // this._stop,   // this.BiblioStatisForm.stop,
                     strOutputPath,
                     entities,
                     out errorinfos,
                     out strError);

                if (lRet == -1)
                    return -1;


            }


            return 0;

        }

        // 从路径中获取id
        public static string GetRecordID(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return "";

            return strPath.Substring(nRet + 1).Trim();
        }


        // 创建读者记录
        int CreateReaderRecord(LibraryChannel channel,
            out string strError)
        {
            strError = "";

            // 创建10条测试用读者记录
            string strTargetRecPath = C_ReaderDbName + "/?";
            for (int i = 0; i < 10; i++)
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");
                XmlNode root = dom.DocumentElement;

                string barcode = "_P" + i.ToString().PadLeft(3, '0');
                DomUtil.SetElementText(root, "barcode", barcode);

                string name = "SIP2测试" + i.ToString().PadLeft(3, '0'); ;
                DomUtil.SetElementText(root, "name", name);

                DomUtil.SetElementText(root, "readerType", C_PatronType);

                //DomUtil.SetElementText(root,"createDate", this.CreateDate);
                string strExistingXml = "";
                string strSavedXml = "";
                string strSavedPath = "";
                byte[] baNewTimestamp = null;
                ErrorCodeValue kernel_errorcode = ErrorCodeValue.NoError;
                long lRet = channel.SetReaderInfo(
                   // _stop,
                   "new",  // this.m_strSetAction,
                   strTargetRecPath,
                   dom.OuterXml,
                   null,
                   null,

                   out strExistingXml,
                   out strSavedXml,
                   out strSavedPath,
                   out baNewTimestamp,
                   out kernel_errorcode,
                   out strError);
                if (lRet == -1)
                {
                    return -1;
                }
            }

            return 0;

        }




        #region 流通权限


        // 获得流通读者权限相关定义
        int GetRightsTableInfo(LibraryChannel channel,
            out string strRightsTableXml,
            out string strError)
        {
            strError = "";
            strRightsTableXml = "";

#if NO
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在获取读者流通权限定义 ...");
            _stop.BeginLoop();
#endif

            try
            {
                long lRet = channel.GetSystemParameter(
                    // _stop,
                    "circulation",
                    "rightsTable",
                    out strRightsTableXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return (int)lRet;
            }
            finally
            {
#if NO
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
#endif
            }

            ERROR1:
            return -1;
        }

        // 保存流通读者权限相关定义
        // parameters:
        //      strRightsTableXml   流通读者权限定义XML。注意，没有根元素
        int SetRightsTableDef(LibraryChannel channel,
            string strRightsTableXml,
            out string strError)
        {
            strError = "";

#if NO
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在保存读者流通权限定义 ...");
            _stop.BeginLoop();
#endif
            try
            {
                long lRet = channel.SetSystemParameter(
                    //_stop,
                    "circulation",
                    "rightsTable",
                    strRightsTableXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return (int)lRet;
            }
            finally
            {
#if NO
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
#endif
            }

            ERROR1:
            return -1;
        }

        // 删除测试加的流通权限
        public int RemoveTestRightsTable(LibraryChannel channel,
            XmlDocument dom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 获取流通权限
            if (dom == null)
            {
                string strRightsTableXml = "";
                nRet = GetRightsTableInfo(channel, out strRightsTableXml, out strError);
                if (nRet == -1)
                    goto ERROR1;
                strRightsTableXml = "<rightsTable>" + strRightsTableXml + "</rightsTable>";
                dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strRightsTableXml);
                }
                catch (Exception ex)
                {
                    strError = "strRightsTableXml装入XMLDOM时发生错误：" + ex.Message;
                    goto ERROR1;
                }
            }

            XmlNode node = dom.DocumentElement.SelectSingleNode("type[@reader='" + C_PatronType + "']");
            if (node != null)
            {
                dom.DocumentElement.RemoveChild(node);
            }

            XmlNodeList list = dom.DocumentElement.SelectNodes("//type[@book='" + C_BookType + "']");
            foreach (XmlNode n in list)
            {
                n.ParentNode.RemoveChild(n);
            }

            node = dom.DocumentElement.SelectSingleNode("readerTypes/item[text()='" + C_PatronType + "']");
            if (node != null)
            {
                node.ParentNode.RemoveChild(node);
            }
            node = dom.DocumentElement.SelectSingleNode("bookTypes/item[text()='" + C_BookType + "']");
            if (node != null)
            {
                node.ParentNode.RemoveChild(node);
            }

            // 保存到系统
            nRet = this.SetRightsTableDef(channel, dom.DocumentElement.InnerXml, out strError);
            if (nRet == -1)
                goto ERROR1;

            return 0;

            ERROR1:
            return -1;
        }

        // 增加测试用的流通权限
        public int AddTestRightsTable(LibraryChannel channel,
            XmlDocument dom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 获取流通权限
            if (dom == null)
            {
                string strRightsTableXml = "";
                nRet = GetRightsTableInfo(channel, out strRightsTableXml, out strError);
                if (nRet == -1)
                    goto ERROR1;
                strRightsTableXml = "<rightsTable>" + strRightsTableXml + "</rightsTable>";
                dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strRightsTableXml);
                }
                catch (Exception ex)
                {
                    strError = "strRightsTableXml装入XMLDOM时发生错误：" + ex.Message;
                    goto ERROR1;
                }
            }


            XmlNode node = dom.DocumentElement.SelectSingleNode("type[@reader='" + C_PatronType + "']");
            // 增加测试用权限
            if (node == null)
            {
                node = dom.CreateElement("type");
                DomUtil.SetAttr(node, "reader", C_PatronType);
                node.InnerXml = @"<param name='可借总册数' value='10' />
                                                <param name='可预约册数' value='5' />
                                                <param name='以停代金因子' value='' />
                                                <param name='工作日历名' value='" + C_CalenderName + @"' />
                                                <type book='" + C_BookType + @"'>
                                                  <param name='可借册数' value='10' />
                                                  <param name='借期' value='31day,60day' />
                                                  <param name='超期违约金因子' value='' />
                                                  <param name='丢失违约金因子' value='1.5' />
                                                </type>";
                dom.DocumentElement.AppendChild(node);


                XmlNode readerTypesNode = dom.DocumentElement.SelectSingleNode("readerTypes");
                if (readerTypesNode == null)
                {
                    readerTypesNode = dom.CreateElement("readerTypes");
                    dom.DocumentElement.AppendChild(readerTypesNode);
                }
                node = dom.CreateElement("item");
                node.InnerText = C_PatronType;
                readerTypesNode.AppendChild(node);

                //    node.InnerXml = "<item>" + C_PatronType + "</item>";
                //dom.DocumentElement.AppendChild(node);

                XmlNode bookTypesNode = dom.DocumentElement.SelectSingleNode("bookTypes");
                if (bookTypesNode == null)
                {
                    bookTypesNode = dom.CreateElement("bookTypes");
                    dom.DocumentElement.AppendChild(bookTypesNode);
                }
                node = dom.CreateElement("item");
                node.InnerText = C_BookType;
                bookTypesNode.AppendChild(node);

                //node = dom.CreateElement("bookTypes");
                //node.InnerXml = "<item>" + C_BookType + "</item>";
                //dom.DocumentElement.AppendChild(node);
            }
            // 保存
            nRet = this.SetRightsTableDef(channel, dom.DocumentElement.InnerXml, out strError);
            if (nRet == -1)
                goto ERROR1;

            return 0;
            ERROR1:
            return -1;
        }

        #endregion

        #endregion

        #region 登录与SC Status

        //登录
        private void button_login_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                this.ClearInfo();
                string error = "";
                LoginResponse_94 response94 = null;
                string responseText = "";
                int nRet = this.login(out response94,
                    out responseText,
                    out error);
                if (nRet == -1 || nRet == 0)
                {
                    this.Print("登录失败：" + error);
                    this.Print(responseText);
                    return;
                }
                this.Print("登录成功");
                this.Print(responseText);
            });
        }

        /// <returns>
        /// 1 登录成功
        /// 0 登录失败
        /// -1 出错
        /// </returns>
        public int login(out LoginResponse_94 response94,
            out string responseText,
            out string error)
        {
            error = "";
            response94 = null;
            responseText = "";

            string username = this.textBox_93_username.Text;
            string password = this.textBox_93_password.Text;
            if (string.IsNullOrEmpty(username) == true)
            {
                error = "用户名不能为空";
                return -1;
            }

            /// <returns>
            /// 1 登录成功
            /// 0 登录失败
            /// -1 出错
            /// </returns>
            int nRet = SCHelper.Instance.Login(username,
                password,
                out response94,
                out responseText,
                out error);
            return nRet;
        }

        //SC status
        private void button_SCStatus_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                this.ClearInfo();
                string error = "";
                ACSStatus_98 response98 = null;
                string responseText = "";
                int nRet = SCHelper.Instance.SCStatus(out response98,
                    out responseText,
                    out error);
                if (nRet == -1)
                {
                    this.Print("出错：" + error);
                }
                else if (nRet == 0)
                {
                    this.Print("ACS不在线");
                }

                this.Print(responseText);
                return;
            });
        }

        #endregion

        #region 借还

        //借书 10人*5册*1借
        private void button_checkout_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            this.CheckoutAndCheckin(10, 5, 1, 0)
            );
        }

        //还书 50册*1还
        private void button_checkin_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            this.Checkin(50, 1)
            );
        }

        // 借还 10人*2册*1借*1还
        private void button_checkoutin_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            this.CheckoutAndCheckin(10, 2, 1, 1)
);
        }

        // 重复还 10册*2次
        private void button_checkin_dup_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            this.Checkin(10, 3)
);
        }

        void MessageBoxShow(Form form, string strText)
        {
            this.Invoke(new Action(() =>
            {
                MessageBox.Show(form, strText);
            }));
        }

        // 重复借 10人*2册*3借
        private void button_checkout_dup_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            this.CheckoutAndCheckin(10, 2, 3, 0)
);
        }

        // 自定义借还
        private void button_checkoutin_customer_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                int patrontNum = 0;
                if (this.textBox_checkinout_patronNum.Text != "")
                {
                    try
                    {
                        patrontNum = Convert.ToInt32(this.textBox_checkinout_patronNum.Text);
                    }
                    catch
                    {
                        MessageBoxShow(this, "读者人数必须是数字");
                        return;
                    }
                }
                int itemNum = 0;
                if (this.textBox_checkinout_itemNum.Text != "")
                {
                    try
                    {
                        itemNum = Convert.ToInt32(this.textBox_checkinout_itemNum.Text);
                    }
                    catch
                    {
                        MessageBoxShow(this, "图书册数必须是数字");
                        return;
                    }
                }
                int checkoutNum = 0;
                if (this.textBox_checkinout_outNum.Text != "")
                {
                    try
                    {
                        checkoutNum = Convert.ToInt32(this.textBox_checkinout_outNum.Text);
                    }
                    catch
                    {
                        MessageBoxShow(this, "借书次数必须是数字");
                        return;
                    }
                }
                int checkinNum = 0;
                if (this.textBox_checkinout_inNum.Text != "")
                {
                    try
                    {
                        checkinNum = Convert.ToInt32(this.textBox_checkinout_inNum.Text);
                    }
                    catch
                    {
                        MessageBoxShow(this, "还书次数必须是数字");
                        return;
                    }
                }

                // 只还书
                if (checkoutNum == 0 && checkinNum > 0)
                {
                    if (itemNum == 0)
                    {
                        MessageBoxShow(this, "请输入还书的册数");
                        return;
                    }
                    this.Checkin(itemNum, checkinNum);
                    return;
                }

                //借书
                if (checkoutNum > 0)
                {
                    if (patrontNum == 0)
                    {
                        MessageBoxShow(this, "请输入读者数量");
                        return;
                    }
                    if (itemNum == 0)
                    {
                        MessageBoxShow(this, "请输入图书册数量");
                        return;
                    }
                    this.CheckoutAndCheckin(patrontNum,
                        itemNum,
                        checkoutNum,
                        checkinNum);
                    return;
                }

                MessageBoxShow(this, "请输入借书次数，还书次数");
            });
        }

        // 续借
        private void button_renew_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            this.CheckoutAndRenew(10, 2, 1, 2)
);
        }


        //借书，或者借还书
        public void CheckoutAndCheckin(int patrontNum,
            int itemNum,
            int checkoutNum,
            int checkinNum)
        {
            string error = "";
            int nRet = 0;

            // 清空输出信息
            this.ClearInfo();

            REDO:
            // 循环读者
            for (int i = 0; i < patrontNum; i++)
            {
                string patronBarcode = "_P" + i.ToString().PadLeft(3, '0');

                // 设成错误的读者条码
                if (checkBox_wrongPatron.Checked == true)
                {
                    patronBarcode += "_w";
                }

                // 每人 itemNum 册
                for (int j = (i + 1) * itemNum - (itemNum - 1); j <= (i + 1) * itemNum; j++)
                {
                    string itemBarcode = "_B" + j.ToString().PadLeft(6, '0');

                    // 设成错误的册条码
                    if (this.checkBox_wrongItem.Checked == true)
                    {
                        itemBarcode += "_w";
                    }

                    //执行借书
                    for (int a = 0; a < checkoutNum; a++)
                    {
                        CheckoutResponse_12 response12 = null;
                        string responseText = "";
                        nRet = SCHelper.Instance.Checkout(patronBarcode, itemBarcode,
                            out response12,
                            out responseText,
                            out error);
                        if (nRet == -2) //尚未登录的情况
                        {
                            //nRet = this.login(out error);
                            //if (nRet == 1) //登录成功，重新执行
                            //    goto REDO;
                            //MessageBox.Show(this, "登录失败：" + error);
                            //return;

                            this.Print("SC尚未登录图书馆系统");
                            this.Print(responseText);
                            return;
                        }
                        this.Print(patronBarcode + "借" + itemBarcode + "...");
                        if (nRet == -1)
                        {
                            Print("出错:" + error);
                            continue;
                        }
                        if (nRet == 0)
                        {
                            Print("借出失败:" + responseText);
                            continue;
                        }
                        this.Print("借书成功");
                    }

                    //执行还书
                    for (int a = 0; a < checkinNum; a++)
                    {
                        CheckinResponse_10 response10 = null;
                        string responseText = "";
                        nRet = SCHelper.Instance.Checkin(itemBarcode,
                            out response10,
                            out responseText,
                            out error);
                        this.Print("还" + itemBarcode + "...");
                        if (nRet == -1)
                        {
                            Print("出错:" + error);
                            continue;
                        }
                        if (nRet == 0)
                        {
                            Print("还书失败:" + responseText);
                            continue;
                        }
                        this.Print("还书成功");
                    }
                }
            }

            return;
        }

        //还书
        public void Checkin(int itemNum, int checkinNum)
        {
            string error = "";
            int nRet = 0;

            // 清空输出信息
            this.ClearInfo();

            for (int j = 1; j <= itemNum; j++)
            {
                string itemBarcode = "_B" + j.ToString().PadLeft(6, '0');

                // 设成错误的册条码
                if (this.checkBox_wrongItem.Checked == true)
                {
                    itemBarcode += "_w";
                }

                //执行还书
                for (int a = 0; a < checkinNum; a++)
                {
                    CheckinResponse_10 response10 = null;
                    string responseText = "";
                    nRet = SCHelper.Instance.Checkin(itemBarcode,
                        out response10,
                        out responseText,
                        out error);
                    if (nRet == -2) //尚未登录的情况
                    {
                        this.Print("SC尚未登录图书馆系统");
                        this.Print(responseText);
                        return;
                    }

                    this.Print("还" + itemBarcode + "...");
                    if (nRet == -1)
                    {
                        Print("出错:" + error);
                        continue;
                    }
                    if (nRet == 0)
                    {
                        Print("还书失败:" + responseText);
                        continue;
                    }
                    this.Print("还书成功");
                }
            }

        }


        //借书，或者借还书
        public void CheckoutAndRenew(int patrontNum,
            int itemNum,
            int checkoutNum,
            int renewNum)
        {
            string error = "";
            int nRet = 0;

            // 清空输出信息
            this.ClearInfo();

            REDO:
            // 循环读者
            for (int i = 0; i < patrontNum; i++)
            {
                string patronBarcode = "_P" + i.ToString().PadLeft(3, '0');
                // 设成错误的读者条码
                if (checkBox_wrongPatron.Checked == true)
                {
                    patronBarcode += "_w";
                }

                // 每人 itemNum 册
                for (int j = (i + 1) * itemNum - (itemNum - 1); j <= (i + 1) * itemNum; j++)
                {
                    string itemBarcode = "_B" + j.ToString().PadLeft(6, '0');
                    // 设成错误的册条码
                    if (this.checkBox_wrongItem.Checked == true)
                    {
                        itemBarcode += "_w";
                    }

                    //执行借书
                    for (int a = 0; a < checkoutNum; a++)
                    {
                        CheckoutResponse_12 response12 = null;
                        string responseText = "";
                        nRet = SCHelper.Instance.Checkout(patronBarcode, itemBarcode,
                            out response12,
                            out responseText,
                            out error);
                        if (nRet == -2) //尚未登录的情况
                        {
                            //nRet = this.login(out error);
                            //if (nRet == 1) //登录成功，重新执行
                            //    goto REDO;
                            //MessageBox.Show(this, "登录失败：" + error);
                            //return;

                            this.Print("SC尚未登录图书馆系统"); ;
                            this.Print(responseText);
                            return;
                        }
                        this.Print(patronBarcode + "借" + itemBarcode + "...");
                        if (nRet == -1)
                        {
                            Print("出错:" + error);
                            continue;
                        }
                        if (nRet == 0)
                        {
                            Print("借出失败:" + responseText);
                            continue;
                        }
                        this.Print("借书成功");
                    }

                    //执行续借
                    for (int a = 0; a < renewNum; a++)
                    {
                        RenewResponse_30 response30 = null;
                        string responseText = "";
                        nRet = SCHelper.Instance.Renew(patronBarcode, itemBarcode,
                            out response30,
                            out responseText,
                            out error);
                        this.Print(patronBarcode + "续借" + itemBarcode + "...");

                        if (nRet == -1)
                        {
                            Print("出错:" + error);
                            continue;
                        }
                        if (nRet == 0)
                        {
                            Print("续借失败:" + responseText);
                            continue;
                        }
                        this.Print("续借成功");
                    }

                }
            }

            return;
        }

        #endregion

        #region 获取读者信息
        // 获取读者信息 
        private void button_patronInfo_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string numString = this.textBox_63_patronNum.Text.Trim();
                if (numString == "")
                {
                    MessageBoxShow(this, "尚未输入读者数量");
                    return;
                }
                int num = 0;
                try
                {
                    num = Convert.ToInt32(numString);
                }
                catch
                {
                    MessageBoxShow(this, "册数必须为整数");
                    return;
                }
                this.GetPatronInfo(num);
            }
);
        }

        public void GetPatronInfo(int patrontNum)
        {
            string error = "";
            int nRet = 0;

            // 清空输出信息
            this.ClearInfo();
            // 循环读者
            for (int i = 0; i < patrontNum; i++)
            {
                string patronBarcode = "_P" + i.ToString().PadLeft(3, '0');
                // 设成错误的读者条码
                if (checkBox_wrongPatron.Checked == true)
                {
                    patronBarcode += "_w";
                }

                PatronInformationResponse_64 response64 = null;
                string responseText = "";
                nRet = SCHelper.Instance.GetPatronInformation(patronBarcode,
                    out response64,
                    out responseText,
                    out error);
                if (nRet == -2) //尚未登录的情况
                {
                    this.Print("SC尚未登录图书馆系统");
                    this.Print(responseText);
                    return;
                }
                this.Print("获取读者" + patronBarcode + "...");
                if (nRet == -1)
                {
                    Print("出错:" + error);
                    continue;
                }
                this.Print("成功：" + responseText);
            }
        }

        #endregion

        #region 获取册信息

        //获取册信息
        private void button_itemInfo_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string itemNum = this.textBox_17_itemNum.Text.Trim();
                if (itemNum == "")
                {
                    MessageBoxShow(this, "尚未输入册数");
                    return;
                }
                int num = 0;
                try
                {
                    num = Convert.ToInt32(itemNum);
                }
                catch
                {
                    MessageBoxShow(this, "册数必须为整数");
                    return;
                }

                this.GetItemInfo(num);
            });
        }

        public void GetItemInfo(int itemNum)
        {
            string error = "";
            int nRet = 0;

            // 清空输出信息
            this.ClearInfo();
            // 循环读者
            for (int i = 1; i <= itemNum; i++)
            {
                string itemBarcode = "_B" + i.ToString().PadLeft(6, '0');

                // 设成错误的册条码
                if (this.checkBox_wrongItem.Checked == true)
                {
                    itemBarcode += "_w";
                }

                ItemInformationResponse_18 response18 = null;
                string responseText = "";
                nRet = SCHelper.Instance.GetItemInformation(itemBarcode,
                    out response18,
                    out responseText,
                    out error);
                if (nRet == -2) //尚未登录的情况
                {
                    this.Print("SC尚未登录图书馆系统");
                    this.Print(responseText);
                    return;
                }
                this.Print("获取图书" + itemBarcode + "...");
                if (nRet == -1)
                {
                    Print("出错:" + error);
                    continue;
                }
                this.Print("成功：" + responseText);
            }
        }






        #endregion

        private void button_stop_Click(object sender, EventArgs e)
        {

        }

        void ProgressSetMessage(string strText)
        {
            this.Invoke(new Action(() =>
            {
                this.toolStripStatusLabel1.Text = strText;
            }));
        }
    }
}
