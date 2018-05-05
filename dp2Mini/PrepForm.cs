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
                        string[] cols = new string[9];
                        cols[0] = strItemBarcode;
                        cols[1] = strISBN;
                        cols[2] = strTitle;
                        cols[3] = strAuthor;
                        cols[4] = strLocation;
                        cols[5] = strAccessNo;
                        cols[6] = strReaderBarcode;
                        cols[7] = strName;
                        cols[8] = strDept;


                        AppendNewLine(this.listView_results, strPath, cols);

                        mainForm.SetMessage((lStart + i + 1).ToString() + " / " + lHitCount);
                        i++;
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;
                    if (lStart >= lHitCount || lCount <= 0)
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
    }
}
