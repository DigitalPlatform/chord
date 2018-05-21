using DigitalPlatform.LibraryRestClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;

using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using System.Drawing.Printing;
using System.IO;

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
                SearchResponse searchResponse = channel.Search(strQueryXml, "", strOutputStyle);
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

                    lRet = channel.GetSearchResult("",
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
                        // string[] cols = record.Cols;
                        string strPath = record.Path;

                        string strXML = record.RecordBody.Xml;
                        XmlDocument dom = new XmlDocument();
                        dom.LoadXml(strXML);


                        string strState = DomUtil.GetElementText(dom.DocumentElement, "state");
                        if ("arrived" == strState)
                            strState = "图书在馆";
                        else if ("outof" == strState)
                            strState = "超过保留期";

                        string strLocation = DomUtil.GetElementText(dom.DocumentElement, "location");
                        string strAccessNo = DomUtil.GetElementText(dom.DocumentElement, "accessNo");


                        string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement, "itemBarcode");
                        if (string.IsNullOrEmpty(strItemBarcode))
                            continue;
                        GetItemInfoResponse itemInfoResponse = channel.GetItemInfo(strItemBarcode, 
                            "xml", // "xml:noborrowhistory", // resultType (itemType)
                            "xml" // biblioType
                            );
                        lRet = itemInfoResponse.GetItemInfoResult.Value;
                        string strErrorInfo = itemInfoResponse.GetItemInfoResult.ErrorInfo;
                        if (lRet != 1)
                        {
                            MessageBox.Show(strErrorInfo);
                            continue;
                        }

                        string strOutMarcSyntax = "";
                        string strMARC = "";
                        string strMarcXml = itemInfoResponse.strBiblio;
                        int nRet = MarcUtil.Xml2Marc(strMarcXml, 
                            false, 
                            "", // 自动识别 MARC 格式
                            out strOutMarcSyntax, 
                            out strMARC, 
                            out strError);
                        if (nRet == -1)
                            continue;

                        MarcRecord marcRecord = new MarcRecord(strMARC);
                        string strISBN = marcRecord.select("field[@name='010']/subfield[@name='a']").FirstContent;
                        string strTitle = marcRecord.select("field[@name='200']/subfield[@name='a']").FirstContent;
                        string strAuthor = marcRecord.select("field[@name='200']/subfield[@name='f']").FirstContent;


                        string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");
                        GetReaderInfoResponse readerInfoResponse = channel.GetReaderInfo(strReaderBarcode, "xml:noborrowhistory");
                        lRet = readerInfoResponse.GetReaderInfoResult.Value;
                        strErrorInfo = readerInfoResponse.GetReaderInfoResult.ErrorInfo;
                        if (lRet != 1)
                        {
                            MessageBox.Show(strErrorInfo);
                            continue;
                        }

                        string strReaderXml = readerInfoResponse.results[0];
                        dom.LoadXml(strReaderXml);
                        string strName = DomUtil.GetElementText(dom.DocumentElement, "name");
                        string strDept = DomUtil.GetElementText(dom.DocumentElement, "department");


                        // MessageBox.Show(strXML);
                        string[] cols = new string[this.listView_results.Columns.Count];
                        cols[0] = strItemBarcode;
                        cols[1] = strISBN;
                        cols[2] = strTitle;
                        cols[3] = strAuthor;
                        cols[4] = strAccessNo;
                        cols[5] = strLocation;
                        cols[6] = strReaderBarcode;
                        cols[7] = strName;
                        cols[8] = strDept;
                        cols[9] = strState;
                        cols[10] = "未打印";


                        AppendNewLine(this.listView_results, strPath, cols);

                        mainForm.SetMessage((lStart + i + 1).ToString() + " / " + lHitCount);
                        i++;
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;
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

        public static void EnsureColumns(ListView listview,
            int nCount,
            int nInitialWidth = 200)
        {
            if (listview.Columns.Count >= nCount)
                return;

            for (int i = listview.Columns.Count; i < nCount; i++)
            {
                string strText = "";
                // strText = Convert.ToString(i);

                ColumnHeader col = new ColumnHeader();
                col.Text = strText;
                col.Width = nInitialWidth;
                listview.Columns.Add(col);
            }
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
                EnsureColumns(list, others.Length + 1,100);

            ListViewItem item = new ListViewItem(strID, 0);

            list.Items.Add(item);

            if (others != null)
            {
                for (int i = 0; i < others.Length; i++)
                {
                    item.SubItems.Add(others[i]);
                }
            }

            return item;
        }

        private StreamReader streamToPrint;
        private void toolStripMenuItem_print_Click(object sender, EventArgs e)
        {
            using (StreamWriter writer = new StreamWriter(@"print.txt"))
            {
                foreach (ListViewItem item in this.listView_results.SelectedItems)
                {
                    StringBuilder sb = new StringBuilder(256);
                    foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                    {
                        string strText = subItem.Text;
                        if (string.IsNullOrEmpty(strText))
                            continue;

                        writer.WriteLine(strText);
                    }
                    writer.WriteLine("-------------------------------------");
                }
            }

            try
            {
                streamToPrint = new StreamReader(@"print.txt");
                try
                {
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += pd_PrintPage;
                    pd.Print();
                }
                finally
                {
                    streamToPrint.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if(this.listView_results.SelectedItems.Count <= 0)
            {
                this.toolStripMenuItem_print.Enabled = false;
                this.toolStripMenuItem_export.Enabled = false;
            }
        }

        private void toolStripMenuItem_export_Click(object sender, EventArgs e)
        {

        }

        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = 5; // ev.MarginBounds.Left;
            float topMargin = 5; // ev.MarginBounds.Top;
            string line = null;

            Font printFont = new Font("微软雅黑", 9);

            // Calculate the number of lines per page.
            linesPerPage = ev.MarginBounds.Height /
               printFont.GetHeight(ev.Graphics);

            // Print each line of the file.
            while (count < linesPerPage &&
               ((line = streamToPrint.ReadLine()) != null))
            {
                yPos = topMargin + (count *
                   printFont.GetHeight(ev.Graphics));


                ev.Graphics.DrawString(line,
                    printFont,
                    Brushes.Black,
                   leftMargin,
                   yPos,
                   new StringFormat());
                count++;
            }

            // If more lines exist, print another page.
            if (line != null)
                ev.HasMorePages = true;
            else
                ev.HasMorePages = false;
        }
    }
}
