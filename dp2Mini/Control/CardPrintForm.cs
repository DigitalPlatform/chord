using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2Mini
{
    public partial class CardPrintForm : Form
    {
        PrinterInfo m_printerInfo = null;

        /// <summary>
        /// 打印机信息
        /// </summary>
        public PrinterInfo PrinterInfo
        {
            get
            {
                return this.m_printerInfo;
            }
            set
            {
                this.m_printerInfo = value;
                // SetTitle();
            }
        }

        /// <summary>
        /// 当前卡片文件全路径
        /// </summary>
        public string CardFilename
        {
            get;
            set;
        }

        PrintCardDocument document = null;

        string m_strPrintStyle = "";    // 打印风格


        public CardPrintForm()
        {
            InitializeComponent();
        }

        // 打印预览(根据卡片文件)
        /// <summary>
        /// 根据卡片文件进行打印预览
        /// </summary>
        /// <param name="bDisplayPrinterDialog">是否显示打印机对话框</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int PrintPreviewFromCardFile()
        {
            string strError = "";
            int nRet = this.BeginPrint(
                CardFilename,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            printDialog1.Document = this.document;

            if (this.PrinterInfo != null)
            {
                string strPrinterName = document.PrinterSettings.PrinterName;
                if (string.IsNullOrEmpty(this.PrinterInfo.PrinterName) == false
                    && this.PrinterInfo.PrinterName != strPrinterName)
                {
                    this.document.PrinterSettings.PrinterName = this.PrinterInfo.PrinterName;
                    if (this.document.PrinterSettings.IsValid == false)
                    {
                        MessageBox.Show(this, "打印机 " + this.PrinterInfo.PrinterName + " 当前不可用，请重新选定打印机");
                        this.document.PrinterSettings.PrinterName = strPrinterName;
                        this.PrinterInfo.PrinterName = "";
                    }
                }

                PaperSize old_papersize = document.DefaultPageSettings.PaperSize;
                if (string.IsNullOrEmpty(this.PrinterInfo.PaperName) == false
                    && this.PrinterInfo.PaperName != document.DefaultPageSettings.PaperSize.PaperName)
                {
                    PaperSize found = null;
                    foreach (PaperSize ps in this.document.PrinterSettings.PaperSizes)
                    {
                        if (ps.PaperName.Equals(this.PrinterInfo.PaperName))
                        {
                            found = ps;
                            break;
                        }
                    }

                    if (found != null)
                        this.document.DefaultPageSettings.PaperSize = found;
                    else
                    {
                        MessageBox.Show(this, "打印机 " + this.PrinterInfo.PrinterName + " 的纸张类型 " + this.PrinterInfo.PaperName + " 当前不可用，请重新选定纸张");
                        document.DefaultPageSettings.PaperSize = old_papersize;
                        this.PrinterInfo.PaperName = "";
                    }
                }
            }

            DialogResult result = printDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                // 记忆打印参数
                if (this.PrinterInfo == null)
                    this.PrinterInfo = new PrinterInfo();
                this.PrinterInfo.PrinterName = document.PrinterSettings.PrinterName;
                this.PrinterInfo.PaperName = document.DefaultPageSettings.PaperSize.PaperName;
                this.PrinterInfo.Landscape = document.DefaultPageSettings.Landscape;
            }

            printPreviewDialog1.Document = this.document;
            printPreviewDialog1.ShowDialog(this);

            this.EndPrint();
            return 0;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }



        /// <summary>
        /// 根据卡片文件进行打印
        /// </summary>
        /// <param name="bDisplayPrinterDialog">是否显示打印机对话框</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int PrintFromCardFile(bool bDisplayPrinterDialog = true)
        {
            string strError = "";
            int nRet = this.BeginPrint(
                CardFilename,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            // Allow the user to choose the page range he or she would
            // like to print.
            printDialog1.AllowSomePages = true;

            // Show the help button.
            printDialog1.ShowHelp = true;

            // Set the Document property to the PrintDocument for 
            // which the PrintPage Event has been handled. To display the
            // dialog, either this property or the PrinterSettings property 
            // must be set 
            printDialog1.Document = this.document;

            if (this.PrinterInfo != null)
            {
                string strPrinterName = document.PrinterSettings.PrinterName;
                if (string.IsNullOrEmpty(this.PrinterInfo.PrinterName) == false
                    && this.PrinterInfo.PrinterName != strPrinterName)
                {
                    this.document.PrinterSettings.PrinterName = this.PrinterInfo.PrinterName;
                    if (this.document.PrinterSettings.IsValid == false)
                    {
                        MessageBox.Show(this, "打印机 " + this.PrinterInfo.PrinterName + " 当前不可用，请重新选定打印机");
                        this.document.PrinterSettings.PrinterName = strPrinterName;
                        this.PrinterInfo.PrinterName = "";
                        bDisplayPrinterDialog = true;
                    }
                }

                PaperSize old_papersize = document.DefaultPageSettings.PaperSize;
                if (string.IsNullOrEmpty(this.PrinterInfo.PaperName) == false
                    && this.PrinterInfo.PaperName != document.DefaultPageSettings.PaperSize.PaperName)
                {
                    PaperSize found = null;
                    foreach (PaperSize ps in this.document.PrinterSettings.PaperSizes)
                    {
                        if (ps.PaperName.Equals(this.PrinterInfo.PaperName))
                        {
                            found = ps;
                            break;
                        }
                    }

                    if (found != null)
                        this.document.DefaultPageSettings.PaperSize = found;
                    else
                    {
                        MessageBox.Show(this, "打印机 " + this.PrinterInfo.PrinterName + " 的纸张类型 " + this.PrinterInfo.PaperName + " 当前不可用，请重新选定纸张");
                        document.DefaultPageSettings.PaperSize = old_papersize;
                        this.PrinterInfo.PaperName = "";
                        bDisplayPrinterDialog = true;
                    }
                }

                // 只要有一个打印机事项没有确定，就要出现打印机对话框
                if (string.IsNullOrEmpty(this.PrinterInfo.PrinterName) == true
                    || string.IsNullOrEmpty(this.PrinterInfo.PaperName) == true)
                    bDisplayPrinterDialog = true;
            }
            else
            {
                // 没有首选配置的情况下要出现打印对话框
                bDisplayPrinterDialog = true;
            }

            DialogResult result = DialogResult.OK;
            if (bDisplayPrinterDialog == true)
            {
                result = printDialog1.ShowDialog();
            }

            // If the result is OK then print the document.
            if (result == DialogResult.OK)
            {
                try
                {
                    if (bDisplayPrinterDialog == true)
                    {
                        // 记忆打印参数
                        if (this.PrinterInfo == null)
                            this.PrinterInfo = new PrinterInfo();
                        this.PrinterInfo.PrinterName = document.PrinterSettings.PrinterName;
                        this.PrinterInfo.PaperName = document.DefaultPageSettings.PaperSize.PaperName;
                        this.PrinterInfo.Landscape = document.DefaultPageSettings.Landscape;
                    }

                    this.document.Print();
                }
                catch (Exception ex)
                {
                    strError = "打印过程出错: " + ex.Message;
                    goto ERROR1;
                }
            }

            this.EndPrint();
            return 0;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        int BeginPrint(string strCardFilename,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strCardFilename) == true)
            {
                strError = "尚未指定卡片文件名";
                return -1;
            }

            if (this.document != null)
            {
                this.document.Close();
                this.document = null;
            }

            this.document = new PrintCardDocument();

            int nRet = this.document.Open(strCardFilename,
                out strError);
            if (nRet == -1)
                return -1;

            this.document.PrintPage -= document_PrintPage;
            this.document.PrintPage += document_PrintPage;

            this.m_strPrintStyle = "";

            return 0;
        }


        void document_PrintPage(object sender, PrintPageEventArgs e)
        {
            this.document.DoPrintPage(this,
                this.m_strPrintStyle,
                e);
        }

        void EndPrint()
        {
            if (this.document != null)
            {
                this.document.Close();
                this.document = null;
            }
        }
    }

    // 
    /// <summary>
    /// 打印机首选配置信息
    /// </summary>
    public class PrinterInfo
    {
        /// <summary>
        /// 类型
        /// </summary>
        public string Type = "";

        /// <summary>
        /// 预设缺省打印机名字，记忆选择过的打印机名字
        /// </summary>
        public string PrinterName = "";  // 预设缺省打印机名字，记忆选择过的打印机名字

        /// <summary>
        /// 预设缺省的纸张尺寸名字
        /// </summary>
        public string PaperName = "";   // 预设缺省的纸张尺寸名字

        /// <summary>
        /// 打印纸方向
        /// </summary>
        public bool Landscape = false;  // 打印纸方向

        // 
        /// <summary>
        /// 根据文本表现形式构造
        /// </summary>
        /// <param name="strType">类型</param>
        /// <param name="strText">正文。格式为 printerName=/??;paperName=???</param>
        public PrinterInfo(string strType,
            string strText)
        {
            this.Type = strType;

            Hashtable table = StringUtil.ParseParameters(strText,
                ';',
                '=');
            this.PrinterName = (string)table["printerName"];
            this.PaperName = (string)table["paperName"];
            string strLandscape = (string)table["landscape"];
            if (string.IsNullOrEmpty(strLandscape) == true)
                this.Landscape = false;
            else
                this.Landscape = DomUtil.IsBooleanTrue(strLandscape);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public PrinterInfo()
        {
        }

        // 
        /// <summary>
        /// 获得文本表现形式
        /// </summary>
        /// <returns>返回文本表现形式。格式为 printerName=/??;paperName=???</returns>
        public string GetText()
        {
            return "printerName=" + this.PrinterName
                + ";paperName=" + this.PaperName
                + (this.Landscape == true ? ";landscape=yes" : "");
        }
    }

}
