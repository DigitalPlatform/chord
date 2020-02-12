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

            // 清空listview
            this.listView_results.Items.Clear();
            this.listView_outof.Items.Clear();

            // 父窗口
            MainForm mainForm = null;
            if (this.MdiParent is MainForm)
                mainForm = this.MdiParent as MainForm;
            Debug.Assert(mainForm != null, "MdiParent 父窗口为 null");

            // 设置状态栏参数
            mainForm.SetStatusMessage("");

            // 检索条件
            string strQueryWord = this.textBox_queryWord.Text;
            string strFrom = "读者证条码号";
            string strMatchStyle = "exact";
            // 如果检索词为空，则按__id检索出全部记录
            if (string.IsNullOrEmpty(strQueryWord))
            {
                strFrom = "__id";
                strMatchStyle = "left";
            }

            // 拼装检索语句
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
                for (; ; )
                {
                    Application.DoEvents();

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
                        string strPath = record.Path;

                        // 把一条记录，解析出各列
                        string[] cols = null;
                        int nRet = GetLineCols(channel,
                            record.RecordBody.Xml,
                            out cols,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

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
            MessageBox.Show(strError);
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
            string strRecord,
            out string[] cols,
            out string strError)
        {
            strError = "";
            cols = new string[13]; //13列

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strRecord);
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

            string strISBN = "";
            string strTitle = "";
            string strAuthor = "";

            // 获取册信息以及书目信息
            string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement, "itemBarcode");
            if (!string.IsNullOrEmpty(strItemBarcode))
            {
                GetItemInfoResponse itemRet = channel.GetItemInfo(strItemBarcode,
                    "xml", 
                    "xml");
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
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");
            string strName = "";
            string strDept = "";
            string requestDate = "";// 预约时间
            string arrivedDate = ""; // 到书时间

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
                        requestDate = DateTimeUtil.LocalDate(DomUtil.GetAttr(node, "requestDate"));
                        arrivedDate = DateTimeUtil.LocalDate(DomUtil.GetAttr(node, "arrivedDate"));
                        break;
                    }
                }
            
            }

            cols[0] = strPrintState; //备书状态
            cols[1] = requestDate;
            cols[2] = arrivedDate;
            cols[3] = strItemBarcode;

            cols[4] = strISBN;
            cols[5] = strTitle;
            cols[6] = strAuthor;

            cols[7] = strAccessNo;
            cols[8] = strLocation;

            cols[9] = strReaderBarcode;
            cols[10] = strName;
            cols[11] = strDept;

            // 是否超过保留期
            cols[12] = strState;


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
