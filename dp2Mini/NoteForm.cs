using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2Mini
{
    public partial class NoteForm : Form
    {
        public NoteForm()
        {
            InitializeComponent();
        }

        #region 打印功能

        // 打印文件
        private string _printFilename = "print.xml";
        void outputPrintFile()
        {
            using (StreamWriter writer = new StreamWriter(this._printFilename,
                false, Encoding.UTF8))
            {
                StringBuilder sb = new StringBuilder(256);
                foreach (ListViewItem item in this.listView1.SelectedItems)  //todo修改
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

        /// <summary>
        /// 包装打印字符串
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static string WrapString(string strText)
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

        private void Print()
        {
            string strError = "";

            // 鼠标设为等待状态
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;


            // 输入打印文件
            outputPrintFile();

            CardPrintForm form = new CardPrintForm();
            form.PrinterInfo = new PrinterInfo();
            form.CardFilename = this._printFilename;  // 卡片文件名
            form.ShowInTaskbar = false;

            form.WindowState = FormWindowState.Minimized;
            form.ShowDialog(this);//.Show();
            try
            {
                int nRet = form.PrintFromCardFile(false);
                if (nRet == -1)
                {
                    form.WindowState = FormWindowState.Normal;
                    strError = strError + "\r\n\r\n以下内容未能成功打印:\r\n";
                    MessageBox.Show(this, strError);
                    return;
                }
            }
            finally
            {
                form.Close();
            }

            this.Cursor = oldCursor;

            //// 原来的打印功能
            //ListViewItem[] items = new ListViewItem[this.listView_results.SelectedItems.Count];
            //this.listView_results.SelectedItems.CopyTo(items, 0);
            //changeAcctiveItemPrintState(items, "已打印");
        }

        private void PrintPreview()
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            outputPrintFile();
            CardPrintForm dlg = new CardPrintForm();
            dlg.CardFilename = this._printFilename;  // 卡片文件名
            dlg.PrintPreviewFromCardFile();

            this.Cursor = oldCursor;
        }
        #endregion

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
