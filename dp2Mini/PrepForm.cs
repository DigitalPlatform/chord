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

namespace dp2Mini
{
    public partial class PrepForm : Form
    {
        public PrepForm()
        {
            InitializeComponent();
        }

        private void PrepForm_Load(object sender, EventArgs e)
        {

        }

        private void PrepForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void PrepForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_search_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.listView_results.Items.Clear();

            MainForm mainForm = null;
            if (this.MdiParent is MainForm)
                mainForm = this.MdiParent as MainForm;

            Debug.Assert(mainForm != null, "MdiParent 父窗口为 null");

            mainForm.SetMessage("");

            string strQueryWord = this.textBox_queryWord.Text;
            string strFrom = "读者证条码号";
            string strMatchStyle = "exact";
            if (string.IsNullOrEmpty(strQueryWord))
            {
                strFrom = "__id";
                strMatchStyle = "left";
            }

            string strQueryXml = "<target list='" + mainForm.ArrivedDbName + ":" + strFrom + "'>" +
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
                SearchResponse searchResponse = channel.Search(strQueryXml, "arrived", strOutputStyle);
                long lRet = searchResponse.SearchResult.Value;
                if (lRet == -1)
                {
                    strError = "检索发生错误：" + strError;
                    goto ERROR1;
                }
                else if (lRet == 0)
                {
                    strError = "读者'" + strQueryWord + "'没有到书信息";
                    goto ERROR1;
                }


                long lHitCount = lRet;
                long lStart = 0;
                long lCount = lHitCount;
                Record[] searchresults = null;
                for (; ; )
                {
                    Application.DoEvents();

                    lRet = channel.GetSearchResult("arrived",
                        lStart,
                        lCount,
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
                        strError = "没有获得到 0 条检索结果";
                        goto ERROR1;
                    }


                    int i = 0;
                    foreach (Record record in searchresults)
                    {
                        string strPath = record.Path;

                        string strRecordXml = record.RecordBody.Xml;
                        XmlDocument dom = new XmlDocument();
                        dom.LoadXml(strRecordXml);

                        //string strState = DomUtil.GetElementText(dom.DocumentElement, "state");
                        //if ("outof" == strState)
                        //{
                        //    //strState = "超过保留期";
                        //    continue;
                        //}

                        string[] cols = FillListViewItem(channel, dom);

                        AppendNewLine(this.listView_results, strPath, cols);

                        mainForm.SetMessage((lStart + i + 1).ToString() + " / " + lHitCount);
                        i++;
                    }

                    lStart += searchresults.Length;
                    // lCount -= searchresults.Length;
                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }

                // this.listView_results.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            finally
            {
                mainForm.ReturnChannel(channel);
            }
            return;

            ERROR1:
            MessageBox.Show(strError);
        }


        string[] FillListViewItem(LibraryChannel channel, XmlDocument dom) //string strRecord)
        {
            string strErrorInfo = "";
            string strError = "";
            string[] cols = new string[11];

            long lRet = 0;

            //// string strXML = record.RecordBody.Xml;
            //XmlDocument dom = new XmlDocument();
            //dom.LoadXml(strRecord);

            string strState = DomUtil.GetElementText(dom.DocumentElement, "state");
            if ("arrived" == strState)
                strState = "图书在馆";
            else if ("outof" == strState)
                strState = "超过保留期";

            string strLocation = DomUtil.GetElementText(dom.DocumentElement, "location");
            string strAccessNo = DomUtil.GetElementText(dom.DocumentElement, "accessNo");
            string strPrintState = DomUtil.GetElementText(dom.DocumentElement, "printState");
            if (string.IsNullOrEmpty(strPrintState))
                strPrintState = "未打印";


            string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement, "itemBarcode");
            if (!string.IsNullOrEmpty(strItemBarcode))
            {
                GetItemInfoResponse itemInfoResponse = channel.GetItemInfo(strItemBarcode,
                    "xml", // "xml:noborrowhistory", // resultType (itemType)
                    "xml");

                lRet = itemInfoResponse.GetItemInfoResult.Value;
                strErrorInfo = itemInfoResponse.GetItemInfoResult.ErrorInfo;
                if (lRet == 1)
                {
                    string strOutMarcSyntax = "";
                    string strMARC = "";
                    string strMarcXml = itemInfoResponse.strBiblio;
                    int nRet = MarcUtil.Xml2Marc(strMarcXml,
                        false,
                        "", // 自动识别 MARC 格式
                        out strOutMarcSyntax,
                        out strMARC,
                        out strError);
                    if (nRet != -1)
                    {
                        MarcRecord marcRecord = new MarcRecord(strMARC);
                        string strISBN = marcRecord.select("field[@name='010']/subfield[@name='a']").FirstContent;
                        string strTitle = marcRecord.select("field[@name='200']/subfield[@name='a']").FirstContent;
                        string strAuthor = marcRecord.select("field[@name='200']/subfield[@name='f']").FirstContent;

                        cols[1] = strISBN;
                        cols[2] = strTitle;
                        cols[3] = strAuthor;
                    }
                }
            }

            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");
            GetReaderInfoResponse readerInfoResponse = channel.GetReaderInfo(strReaderBarcode, "xml:noborrowhistory");
            lRet = readerInfoResponse.GetReaderInfoResult.Value;
            strErrorInfo = readerInfoResponse.GetReaderInfoResult.ErrorInfo;
            if (lRet == 1)
            {
                string strReaderXml = readerInfoResponse.results[0];
                dom.LoadXml(strReaderXml);
                string strName = DomUtil.GetElementText(dom.DocumentElement, "name");
                string strDept = DomUtil.GetElementText(dom.DocumentElement, "department");

                cols[6] = strReaderBarcode;
                cols[7] = strName;
                cols[8] = strDept;
            }

            cols[0] = strItemBarcode;
            cols[4] = strAccessNo;
            cols[5] = strLocation;
            cols[9] = strState;
            cols[10] = strPrintState;

            return cols;
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
                ListViewUtil.EnsureColumns(list, others.Length + 1, 100);

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
            string strPrintState = ListViewUtil.GetItemText(item, item.SubItems.Count - 1);
            if (strPrintState == "已打印")
            {
                item.BackColor = Color.Gray;
            }

            // 如果是超过保留期的，背景显示淡蓝
            string strState = ListViewUtil.GetItemText(item, item.SubItems.Count - 2);
            if (strState == "超过保留期")
            {
                item.BackColor = Color.SkyBlue ;
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
                    for(int i=0;i<item.SubItems.Count-1;i++)
                    {
                        ListViewItem.ListViewSubItem subItem = item.SubItems[i];
                        string strText = subItem.Text;
                        if (string.IsNullOrEmpty(strText))
                            continue;

                        

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
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            string strError = "";

            outputPrintFile();

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

            ListViewItem[] items = new ListViewItem[this.listView_results.SelectedItems.Count];
            this.listView_results.SelectedItems.CopyTo(items, 0);
            changeAcctiveItemPrintState(items);

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
        void changeAcctiveItemPrintState(ListViewItem[] items, string strChangeState = "已打印")
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

                    ListViewUtil.ChangeItemText(item, 11, strChangeState);

                    if (strChangeState == "已打印")
                        item.BackColor = Color.Gray;
                }
            }
            finally
            {
                mainForm.ReturnChannel(channel);
            }
        }
    }
}
