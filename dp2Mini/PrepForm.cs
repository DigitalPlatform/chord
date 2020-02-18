using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Drawing.Printing;

using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.Forms;
using DigitalPlatform.LibraryRestClient;
using System.Collections.Generic;
using DigitalPlatform.IO;
using DigitalPlatform;
using System.Threading;
using System.Threading.Tasks;

namespace dp2Mini
{
    public partial class PrepForm : Form
    {
        string _fileName = "";
        List<BookNote> _list = new List<BookNote>();

        public PrepForm()
        {
            InitializeComponent();
        }

        private void PrepForm_Load(object sender, EventArgs e)
        {
            this._list = new List<BookNote>();

            this._fileName = Application.StartupPath + "\\records.xml";
            if (File.Exists(this._fileName) == true)
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(this._fileName);
                XmlNodeList nodeList = dom.DocumentElement.SelectNodes("item");
                foreach (XmlNode node in nodeList)
                {
                    string id = DomUtil.GetAttr(node, "id");
                    string records = DomUtil.GetAttr(node, "records");
                    string noticeState = DomUtil.GetAttr(node, "noticeState");
                    string dateTime = DomUtil.GetAttr(node, "dateTime");
                    BookNote order = new BookNote
                    {
                        id = id,
                        records = records,
                        noticeState = noticeState,
                        dateTime = dateTime,
                    };

                    this._list.Add(order);
                }
            }

            foreach (BookNote note in this._list)
            {
                this.AddNoteToListView(note);
            }


        }

        public void AddNoteToListView(BookNote note)
        {
            ListViewItem item = new ListViewItem(note.id, 0);
            this.listView_bsd.Items.Add(item);

            item.SubItems.Add(note.dateTime);
            item.SubItems.Add(note.records);
            item.SubItems.Add(note.noticeState);
            item.SubItems.Add(note.takeBoolState);
        }

        private void PrepForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this._list.Count == 0)
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<root>");

            foreach (BookNote order in this._list)
            {
                sb.AppendLine("<item id='"+order.id+"' records='"+order.records+"' noticeState='"+order.noticeState + "' dateTime='"+order.dateTime+"'/>");
            }
            sb.AppendLine("</root>");

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(sb.ToString());
            dom.Save(this._fileName);
        }


        private void PrepForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            string text = "";
        }

        // 名字以用途命名即可。TokenSource 这种类型名称可以不出现在名字中
        CancellationTokenSource _cancel = new CancellationTokenSource();


        private void button_search_Click(object sender, EventArgs e)
        {

            // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
            this._cancel.Dispose();
            this._cancel = new CancellationTokenSource();
            //this.textBox_info.Text = "";

            Task.Run(() =>
            {
                doSearch(this._cancel.Token);
            });


        }

        private void doSearch(CancellationToken token)
        {
            Cursor oldCursor = Cursor.Current;
            // 设置按钮状态
            this.Invoke((Action)(() =>
            {
                EnableControls(false);
                // 鼠标设为等待状态
                oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

            }
                )) ;
            try
            {

                string strError = "";

                string strQueryWord = "";
                string dbNams = "";
                MainForm mainForm = null;

                // 界面显示信息
                this.Invoke((Action)(() =>
                {
                // 清空listview
                this.listView_results.Items.Clear();
                    this.listView_outof.Items.Clear();

                //设置父窗口状态栏参数

                if (this.MdiParent is MainForm)
                        mainForm = this.MdiParent as MainForm;
                    Debug.Assert(mainForm != null, "MdiParent 父窗口为 null");

                    mainForm.SetStatusMessage("");

                    strQueryWord = this.textBox_queryWord.Text;
                    dbNams = mainForm.ArrivedDbName;

                }
                 ));

                // 检索条件
                string strFrom = "读者证条码号";
                string strMatchStyle = "exact";
                // 如果检索词为空，则按__id检索出全部记录
                if (string.IsNullOrEmpty(strQueryWord))
                {
                    strFrom = "__id";
                    strMatchStyle = "left";
                }

                // 拼装检索语句
                string strQueryXml = "<target list='" + dbNams + ":" + strFrom + "'>" +
                    "<item>" +
                    "<word>" + strQueryWord + "</word>" +
                    "<match>" + strMatchStyle + "</match>" +
                    "<relation>=</relation>" +
                    "<dataType>string</dataType>" +
                    "</item>" +
                    "<lang>zh</lang>" +
                    "</target>";

                LibraryChannel channel = mainForm.GetChannel();
                try
                {
                    string strOutputStyle = "";
                    SearchResponse searchResponse = channel.Search(strQueryXml,
                        "arrived",
                        strOutputStyle);
                    long lRet = searchResponse.SearchResult.Value;
                    if (lRet == -1)
                    {
                        strError = "检索发生错误：" + strError;
                        goto ERROR1;
                    }
                    if (lRet == 0)
                    {
                        strError = "读者'" + strQueryWord + "'没有到书信息";
                        goto ERROR1;
                    }

                    // 总记录数
                    long lTotalCount = lRet;

                    long lStart = 0;
                    long lOnceCount = lTotalCount;
                    Record[] searchresults = null;
                    while (token.IsCancellationRequested == false)
                    {
                        //Application.DoEvents();


                        lRet = channel.GetSearchResult("arrived",
                            lStart,
                            lOnceCount,
                            "id,xml",// cols,
                            "zh",
                            out searchresults,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "获得检索结果发生错误：" + strError;
                            goto ERROR1;
                        }
                        else if (lRet == 0)
                        {
                            strError = "获得0 条检索结果";
                            goto ERROR1;
                        }


                        int i = 0;
                        foreach (Record record in searchresults)
                        {
                            if (token.IsCancellationRequested == true)
                                break;

                            string strPath = record.Path;
                            // 检查这笔记录是否已经在配书单里了
                            BookNote order = this.getOrderByRecPath(strPath);
                            if (order != null)
                            {
                                //// 增加一行到超过保留期的
                                //AppendNewLine(this.listView1, strPath, cols);
                                continue;
                            }


                            // 把一条记录，解析出各列
                            string[] cols = null;
                            int nRet = GetLineCols(channel,
                                strPath,
                                record.RecordBody.Xml,
                                out cols,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            this.Invoke((Action)(() =>
                            {
                                 if (cols[cols.Length - 1] == "超过保留期")
                                {
                                    // 增加一行到超过保留期的
                                    AppendNewLine(this.listView_outof, strPath, cols);
                                }
                                else
                                {
                                    // 增加一行到预约到书
                                    AppendNewLine(this.listView_results, strPath, cols);
                                }


                                // 设置状态栏信息
                                mainForm.SetStatusMessage((lStart + i + 1).ToString() + " / " + lTotalCount);

                                // 数量加1
                                i++;
                            }
                        ));

                        }

                        lStart += searchresults.Length;
                        if (lStart >= lTotalCount)
                            break;
                    }
                }
                finally
                {
                    mainForm.ReturnChannel(channel);
                }


                return;

            ERROR1:
                this.Invoke((Action)(() =>
                {
                    MessageBox.Show(strError);
                }
                ));

            }
            finally
            {
                // 设置按置状态
                this.Invoke((Action)(() =>
                {
                    EnableControls(true);
                    this.Cursor = oldCursor;
                }
                    ));
            }
        }

        /// <summary>
        /// 分析出各列值
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="strRecord"></param>
        /// <param name="cols"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        int GetLineCols(LibraryChannel channel,
            string strPath,
           string strRecord,
           out string[] cols,
           out string strError)
        {
            strError = "";
            cols = new string[15]; //13列

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strRecord);
            

            string strLocation = DomUtil.GetElementText(dom.DocumentElement, "location");
            string strAccessNo = DomUtil.GetElementText(dom.DocumentElement, "accessNo");
            string strPrintState = DomUtil.GetElementText(dom.DocumentElement, "printState");
            if (string.IsNullOrEmpty(strPrintState))
                strPrintState = "未打印";

            string strISBN = "";
            string strTitle = "";
            string strAuthor = "";

           
            string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement, "itemBarcode");
            
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");
            string strName = "";
            string strDept = "";
            string strTel = "";
            string requestDate = "";// 预约时间
            string arrivedDate = ""; // 到书时间

            // 本地记录
            string operState = "未知";
            //ReservationItem localRecord = this.getItem(strPath);
            //if (localRecord != null)
            //    operState = localRecord.State;

            string strState = DomUtil.GetElementText(dom.DocumentElement, "state");
            if ("arrived" == strState)
            {
                strState = "图书在馆";
            }
            else if ("outof" == strState) 
            {
                strState = "超过保留期";
                goto END;
            }

            // 获取册信息以及书目信息
            if (!string.IsNullOrEmpty(strItemBarcode))
            {
                GetItemInfoResponse itemRet = channel.GetItemInfo(strItemBarcode,
                    "xml",
                    "xml");
                if (itemRet == null)
                {
                    strError = "返回结果为null。";
                    return -1;
                }
                if (itemRet.GetItemInfoResult.Value == -1)
                {
                    strError = itemRet.GetItemInfoResult.ErrorInfo;
                    return -1;
                }
                if (itemRet.GetItemInfoResult.Value == 1)
                {
                    string strOutMarcSyntax = "";
                    string strMARC = "";
                    string strMarcXml = itemRet.strBiblio;
                    int nRet = MarcUtil.Xml2Marc(strMarcXml,
                        false,
                        "", // 自动识别 MARC 格式
                        out strOutMarcSyntax,
                        out strMARC,
                        out strError);
                    if (nRet != -1)
                    {
                        MarcRecord marcRecord = new MarcRecord(strMARC);
                        strISBN = marcRecord.select("field[@name='010']/subfield[@name='a']").FirstContent;
                        strTitle = marcRecord.select("field[@name='200']/subfield[@name='a']").FirstContent;
                        strAuthor = marcRecord.select("field[@name='200']/subfield[@name='f']").FirstContent;


                    }
                }
            }


            // 获取读者信息
            if (string.IsNullOrEmpty(strReaderBarcode) == false)
            {
                GetReaderInfoResponse readerRet = channel.GetReaderInfo(strReaderBarcode,
                    "xml:noborrowhistory");
                if (readerRet.GetReaderInfoResult.Value == -1)
                {
                    strError = readerRet.GetReaderInfoResult.ErrorInfo;
                    return -1;
                }
                if (readerRet.GetReaderInfoResult.Value == 1)
                {
                    string strReaderXml = readerRet.results[0];
                    dom.LoadXml(strReaderXml);

                    XmlNode rootNode = dom.DocumentElement;
                    strName = DomUtil.GetElementText(rootNode, "name");
                    strDept = DomUtil.GetElementText(rootNode, "department");
                    strTel = DomUtil.GetElementText(rootNode, "tel");

                    /*
                    - <root expireDate="">
                     <barcode>XZP10199</barcode> 
                     <readerType>学生</readerType> 
                     <name>李明</name> 
                     <overdues /> 
                   - <reservations>
                     <request items="XZ000101" requestDate="Tue, 11 Feb 2020 00:30:27 +0800" 
                        operator="XZP10199" state="arrived" arrivedDate="Tue, 11 Feb 2020 00:31:45 +0800" 
                        arrivedItemBarcode="XZ000101" notifyID="59abfc23-f44f-4b34-a22c-f8a8aa5e289e" 
                        accessNo="K825.6=76/Z780" location="星洲学校/图书馆,#reservation" /> 
                     </reservations>
                     </root>
                     */

                    XmlNodeList nodeList = rootNode.SelectNodes("reservations/request");
                    foreach (XmlNode node in nodeList)
                    {
                        string arrivedItemBarcode = DomUtil.GetAttr(node, "arrivedItemBarcode");
                        if (arrivedItemBarcode == strItemBarcode)
                        {
                            requestDate = DateTimeUtil.ToLocalTime(DomUtil.GetAttr(node, "requestDate"), "yyyy-MM-dd HH:mm:ss");
                            arrivedDate = DateTimeUtil.ToLocalTime(DomUtil.GetAttr(node, "arrivedDate"), "yyyy-MM-dd HH:mm:ss");
                            break;
                        }
                    }

                }
            }

        END:

            cols[0] = strPrintState; //备书状态
            cols[1] = operState;
            cols[2] = requestDate;
            cols[3] = arrivedDate;
            cols[4] = strItemBarcode;

            cols[5] = strISBN;
            cols[6] = strTitle;
            cols[7] = strAuthor;

            cols[8] = strAccessNo;
            cols[9] = strLocation;

            cols[10] = strReaderBarcode;
            cols[11] = strName;
            cols[12] = strDept;
            cols[13] = strTel;

            // 是否超过保留期
            cols[14] = strState;


            return 0;
        }



        /// <summary>
        /// 在 ListView 最后追加一行
        /// </summary>
        /// <param name="list">ListView 对象</param>
        /// <param name="strID">左边第一列内容</param>
        /// <param name="others">其余列内容</param>
        /// <returns>新创建的 ListViewItem 对象</returns>
        public static ListViewItem AppendNewLine(
            ListView list,
            string strID,
            string[] others)
        {
            if (others != null)
            {
                //确保列标题数量足够
                ListViewUtil.EnsureColumns(list, others.Length + 1, 100);
            }

            ListViewItem item = new ListViewItem(strID, 0);
            list.Items.Add(item);
            if (others != null)
            {
                for (int i = 0; i < others.Length; i++)
                {
                    item.SubItems.Add(others[i]);
                }
            }

            // 如果是已打印过的预约记录，背景显示灰色
            string strPrintState = ListViewUtil.GetItemText(item, 1);
            if (strPrintState == "已打印")
            {
                item.BackColor = Color.Gray;
            }

            // 如果是超过保留期的，背景显示淡蓝
            string strState = ListViewUtil.GetItemText(item, item.SubItems.Count - 1);
            if (strState == "超过保留期")
            {
                item.BackColor = Color.SkyBlue;
            }

            return item;
        }

        private string printFilename = "print.xml";

        void outputPrintFile()
        {
            using (StreamWriter writer = new StreamWriter(printFilename, false, Encoding.UTF8))
            {
                StringBuilder sb = new StringBuilder(256);
                foreach (ListViewItem item in this.listView_results.SelectedItems)
                {
                    /*
                                        string strPrintState = ListViewUtil.GetItemText(item, item.SubItems.Count - 1);
                                        if (strPrintState == "已打印")
                                            continue;
                    */
                    //foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                    for (int i = 0; i < item.SubItems.Count; i++)
                    {
                        ListViewItem.ListViewSubItem subItem = item.SubItems[i];
                        string strText = subItem.Text;
                        //if (string.IsNullOrEmpty(strText))
                        //    continue;

                        if (i == 2)
                        {
                            strText = "预约时间:" + strText;
                        }
                        else if (i == 3)
                        {
                            strText = "到书时间:" + strText;
                        }
                        else if (i == 4 || i == 8 || i == 13)
                        {
                            strText = "<strong><font size='10'>" + strText + "</font></strong>";
                            // strText= "<span style='font-family:verdana;font-size:20px'>" + strText+"</span>";
                        }


                        sb.Append("<p>").Append(strText).Append("</p>").AppendLine();
                    }
                    sb.AppendLine("<p>-----------------------------------</p>");
                }

                writer.Write(WrapString(sb.ToString()));
            }
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

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_print_Click(object sender, EventArgs e)
        {
            // 鼠标设为等待状态
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            // 输入打印文件
            outputPrintFile();

            string strError = "";
            CardPrintForm form = new CardPrintForm();
            form.PrinterInfo = new PrinterInfo();
            form.CardFilename = printFilename;  // 卡片文件名
            form.ShowInTaskbar = false;

            form.WindowState = FormWindowState.Minimized;
            form.Show();
            int nRet = form.PrintFromCardFile(false);
            if (nRet == -1)
            {
                form.WindowState = FormWindowState.Normal;
                strError = strError + "\r\n\r\n以下内容未能成功打印:\r\n";
                goto ERROR1;
            }
            form.Close();

            // 原来的打印功能
            ListViewItem[] items = new ListViewItem[this.listView_results.SelectedItems.Count];
            this.listView_results.SelectedItems.CopyTo(items, 0);
            changeAcctiveItemPrintState(items, "已打印");

            // 生成一个新的备书单
            string paths = "";
            foreach (ListViewItem item in this.listView_results.SelectedItems)
            {
                string onePath = item.SubItems[0].Text;
                if (paths != "")
                {
                    paths += ",";
                }
                paths += onePath;

                // 把预约到书列表中删除
                this.listView_results.Items.Remove(item);
            }

            // 加到备书单
            BookNote note = new BookNote();
            note.id = Guid.NewGuid().ToString();
            note.records = paths;
            note.dateTime= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            this.AddNoteToListView(note);



            this.Cursor = oldCursor;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 打印预览，注意不能用打印预览中的打印按钮打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_printPreview_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            outputPrintFile();
            CardPrintForm dlg = new CardPrintForm();
            dlg.CardFilename = printFilename;  // 卡片文件名
            dlg.PrintPreviewFromCardFile();

            this.Cursor = oldCursor;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (this.listView_results.SelectedItems.Count <= 0)
            {
                this.toolStripMenuItem_print.Enabled = false;
                this.toolStripMenuItem_printPreview.Enabled = false;
                //this.toolStripMenuItem_export.Enabled = false;
                //this.toolStripMenuItem_remove.Enabled = false;
            }
            else
            {
                this.toolStripMenuItem_print.Enabled = true;
                this.toolStripMenuItem_printPreview.Enabled = true;
                //this.toolStripMenuItem_export.Enabled = true;
                //this.toolStripMenuItem_remove.Enabled = true;
            }
        }

        /// <summary>
        /// 导出excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_export_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "尚未实现");
        }

        /// <summary>
        /// 移出行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_remove_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            this.listView_results.BeginUpdate();
            for (int i = this.listView_results.SelectedIndices.Count - 1; i >= 0; i--)
            {
                this.listView_results.Items.RemoveAt(this.listView_results.SelectedIndices[i]);
            }
            this.listView_results.EndUpdate();

            this.Cursor = oldCursor;
        }

        /// <summary>
        /// 将状态修改为未打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_change_Click(object sender, EventArgs e)
        {
            ListViewItem[] listViews = new ListViewItem[this.listView_results.SelectedItems.Count];
            this.listView_results.SelectedItems.CopyTo(listViews, 0);
            changeAcctiveItemPrintState(listViews, "");
        }


        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="items"></param>
        /// <param name="strChangeState"></param>
        void changeAcctiveItemPrintState(ListViewItem[] items, string strChangeState)
        {
            if (items.Length == 0)
                return;

            MainForm mainForm = null;
            if (this.MdiParent is MainForm)
                mainForm = this.MdiParent as MainForm;

            Debug.Assert(mainForm != null, "MdiParent 父窗口为 null");

            LibraryChannel channel = mainForm.GetChannel();
            try
            {
                string strResult = "";
                string strMetaData = "";
                byte[] baTimestamp = null;
                string strOutputResPath = "";
                string strError = "";
                foreach (ListViewItem item in items)
                {
                    Application.DoEvents();

                    string strResPath = item.Text;
                    long lRet = channel.GetRes(strResPath,
                        "content,data,metadata,timestamp,outputpath",
                        out strResult,
                        out strMetaData,
                        out baTimestamp,
                        out strOutputResPath,
                        out strError);
                    if (lRet == -1)
                    {
                        MessageBox.Show(this, strError);
                        return;
                    }

                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(strResult);

                    string strPrintState = DomUtil.GetElementText(dom.DocumentElement, "printState");
                    if (strPrintState == strChangeState)
                        continue;

                    DomUtil.SetElementText(dom.DocumentElement, "printState", strChangeState);

                    byte[] baOutTimestamp = null;
                    lRet = channel.WriteRes(strResPath,
                        dom.DocumentElement.OuterXml,
                        true,
                        "",
                        baTimestamp,
                        out baOutTimestamp,
                        out strOutputResPath,
                        out strError);
                    if (lRet == -1)
                    {
                        MessageBox.Show(this, strError);
                        return;
                    }

                    ListViewUtil.ChangeItemText(item, 1, strChangeState);

                    if (strChangeState == "已打印")
                        item.BackColor = Color.Gray;
                }
            }
            finally
            {
                mainForm.ReturnChannel(channel);
            }
        }

        SortColumns SortColumns1 = new SortColumns();

        // 参与排序的列号数组
        SortColumns SortColumns2 = new SortColumns();


        private void listView_outof_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;

            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 第一列为记录路径，排序风格特殊
            if (nClickColumn == 0)
                sortStyle = ColumnSortStyle.RecPath;

            this.SortColumns2.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView_outof.Columns,
                true);

            // 排序
            this.listView_outof.ListViewItemSorter = new SortColumnsComparer(this.SortColumns2);

            this.listView_outof.ListViewItemSorter = null;
        }

        private void listView_results_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;

            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 第一列为记录路径，排序风格特殊
            if (nClickColumn == 0)
                sortStyle = ColumnSortStyle.RecPath;

            this.SortColumns1.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView_results.Columns,
                true);

            // 排序
            this.listView_results.ListViewItemSorter = new SortColumnsComparer(this.SortColumns1);

            this.listView_results.ListViewItemSorter = null;
        }

        // 设置控件是否可用
        void EnableControls(bool bEnable)
        {
            this.button_search.Enabled = bEnable;
            this.button_stop.Enabled = !(bEnable);
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            // 停止
            this._cancel.Cancel();
        }

        private void 处理结果ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            string state = "找到图书";
            //string state = "未找到图书";

            ListViewItem[] items = new ListViewItem[this.listView_results.SelectedItems.Count];
            this.listView_results.SelectedItems.CopyTo(items, 0);

            foreach (ListViewItem one in items)
            {
                string recPath = one.SubItems[0].Text;

                ReservationItem item = getItem(recPath);
                if (item == null)
                {
                    item = new ReservationItem
                    {
                        RecPath = recPath,
                        State = state,
                    };
                    this._list.Add(item);
                }
                this.listView_results.Items.Remove(one);

                this.listView1.Items.Add(one);
                
            }
            */
        }

        public BookNote getOrder(string id)
        {
            foreach (BookNote order in this._list)
            {
                if (order.id == id)
                    return order;
            }
            return null;
        }

        public BookNote getOrderByRecPath(string recPath)
        {
            foreach (BookNote order in this._list)
            {
                if (order.records.IndexOf(recPath) != -1)
                    return order;
            }
            return null;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }


    public class BookNote
    {
        public string id = "";

        public string records = "";

        public string dateTime = "";

        public string noticeState = "未通知";
        public string takeBoolState = "未取书";
    }
}
