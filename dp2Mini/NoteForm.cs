using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2Mini
{
    public partial class NoteForm : Form
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public NoteForm()
        {
            InitializeComponent();
        }

        // 名字以用途命名即可。TokenSource 这种类型名称可以不出现在名字中
        CancellationTokenSource _cancel = new CancellationTokenSource();

        /// <summary>
        /// 窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoteForm_Load(object sender, EventArgs e)
        {
            // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
            this._cancel.Dispose();
            this._cancel = new CancellationTokenSource();
            // 开一个新线程
            Task.Run(() =>
            {
                LoadNotes(this._cancel.Token);
            });
        }

        /// <summary>
        /// 装载备书单
        /// </summary>
        /// <param name="token"></param>
        public void LoadNotes(CancellationToken token)
        {
            //把本地库的备书单显示在列表中
            List<Note> notes = DbManager.Instance.GetNotes();
            foreach (Note note in notes)
            {
                if (token.IsCancellationRequested == true)
                    break;

                string noteId = DbManager.NumToString(note.Id);
                ListViewItem viewItem = new ListViewItem(noteId, 0);
                this.Invoke((Action)(() =>
                {
                    this.listView_note.Items.Add(viewItem);
                    this.LoadOneNote(note, viewItem);
                }
                ));
            }
        }

        public void LoadOneNote(Note note,ListViewItem viewItem)
        {
            viewItem.SubItems.Clear();
            /*
                    单号
                    读者
                    包含的预约记录
                    创建日期
                    当前进度
                    */
            string noteId = DbManager.NumToString(note.Id);
            viewItem.Text = noteId;

            viewItem.SubItems.Add(note.PatronName);
            viewItem.SubItems.Add(note.PatronTel);
            viewItem.SubItems.Add(note.Items);
            viewItem.SubItems.Add(note.CreateTime);
            viewItem.SubItems.Add(note.Step);
            /*
            打印
            备书
            通知
            取书
             */
            viewItem.SubItems.Add(GetStepStateText(note.PrintState)); //note.PrintState);
            viewItem.SubItems.Add(note.PrintTime);
            viewItem.SubItems.Add(GetStepStateText(note.CheckResult));// note.CheckResult);
            viewItem.SubItems.Add(note.CheckedTime);
            viewItem.SubItems.Add(GetStepStateText(note.NoticeState));//note.NoticeState);
            viewItem.SubItems.Add(note.NoticeTime);
            viewItem.SubItems.Add(GetStepStateText(note.TakeoffState));//note.TakeoffState);
            viewItem.SubItems.Add(note.TakeoffTime);
        }




        private void listView_note_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 先清空下方界面上次备书单的信息
            this.SetOperateButton("");
            this.listView_items.Items.Clear();

            if (this.listView_note.SelectedItems.Count == 0)
                return;


            // 选择一行，下方进度按钮变化，并且显示详细信息
            ListViewItem viewItem = this.listView_note.SelectedItems[0];
            string noteId = viewItem.SubItems[0].Text;
            Note note = DbManager.Instance.GetNote(noteId);

            // 设置这个备书单的操作按钮
            SetOperateButton(note.Step);

            // 显示详细信息
            List<ReservationItem> items = DbManager.Instance.GetItemsByNoteId(noteId);
            foreach (ReservationItem item in items)
            {
                this.AppendNewLine(this.listView_items, item);
            }
        }

        /// <summary>
        /// 在 ListView 最后追加一行
        /// </summary>
        /// <param name="list">ListView 对象</param>
        /// <param name="strID">左边第一列内容</param>
        /// <param name="others">其余列内容</param>
        /// <returns>新创建的 ListViewItem 对象</returns>
        public  ListViewItem AppendNewLine(ListView list,
            ReservationItem resItem)
        {
            ListViewItem viewItem = new ListViewItem(resItem.RecPath, 0);
            list.Items.Add(viewItem);

            /*
            路径
            备书结果
            读者证条码
            读者姓名
            */
            viewItem.SubItems.Add(resItem.CheckResult);
            viewItem.SubItems.Add(resItem.PatronBarcode);
            viewItem.SubItems.Add(resItem.PatronName);
            /*
            册条码
            书名
            索取号
            馆藏地点
            ISBN
            作者
            */
            viewItem.SubItems.Add(resItem.ItemBarcode);
            viewItem.SubItems.Add(resItem.Title);
            viewItem.SubItems.Add(resItem.AccessNo);
            viewItem.SubItems.Add(resItem.Location);
            viewItem.SubItems.Add(resItem.ISBN);
            viewItem.SubItems.Add(resItem.Author);
            /*
            读者电话
            读者部门
            预约时间
            到书时间
            预约状态
             */
            viewItem.SubItems.Add(resItem.PatronTel);
            viewItem.SubItems.Add(resItem.Department);
            viewItem.SubItems.Add(resItem.RequestTime);
            viewItem.SubItems.Add(resItem.ArrivedTime);
            viewItem.SubItems.Add(resItem.State);



            return viewItem;
        }

        #region 操作按钮状态

        private void FinishButton(Button btn)
        {
            btn.Enabled = false;
            btn.BackColor = Color.Green;
        }

        private void TodoButton(Button btn)
        {
            btn.Enabled = true;
            btn.BackColor = Color.Yellow;
        }

        private void DisableButton(Button btn)
        {
            btn.Enabled = false;
            btn.BackColor = Button.DefaultBackColor;
        }

        public void SetOperateButton(string step)
        {
            //create/print/check/notice/takeoff
            this.DisableButton(this.button_create);
            this.DisableButton(this.button_print);
            this.DisableButton(this.button_check);
            this.DisableButton(this.button_notice);
            this.DisableButton(this.button_takeoff);

            if (step == Note.C_Step_Create)
            {
                this.FinishButton(this.button_create);
                this.TodoButton(this.button_print);
            }
            else if (step == Note.C_Step_Print)
            {
                this.FinishButton(this.button_create);
                this.FinishButton(this.button_print);
                this.TodoButton(this.button_check);
            }
            else if (step == Note.C_Step_Check)
            {
                this.FinishButton(this.button_create);
                this.FinishButton(this.button_print);
                this.FinishButton(this.button_check);
                this.TodoButton(this.button_notice);
            }
            else if (step == Note.C_Step_Notice)
            {
                this.FinishButton(this.button_create);
                this.FinishButton(this.button_print);
                this.FinishButton(this.button_check);
                this.FinishButton(this.button_notice);
                this.TodoButton(this.button_takeoff);
            }
            else if (step == Note.C_Step_Takeoff)
            {
                this.FinishButton(this.button_create);
                this.FinishButton(this.button_print);
                this.FinishButton(this.button_check);
                this.FinishButton(this.button_notice);
                this.FinishButton(this.button_takeoff);
            }
        }

        #endregion

        private void button_print_Click(object sender, EventArgs e)
        {
            if (this.listView_note.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择备书单");
                return;
            }

            ListViewItem viewItem = this.listView_note.SelectedItems[0];
            string noteId = viewItem.SubItems[0].Text;
            Note note = DbManager.Instance.GetNote(noteId);
            if (note == null)
            { 
                MessageBox.Show(this,"未找到单号为'" + noteId + "'的记录。");
                return;
            }

            string printTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            note.PrintTime = printTime;
            note.PrintState = "Y";
            note.Step = Note.C_Step_Print;

            // 实际打印
            this.Print(note);

            // 更新本地数据库备书库打印状态和时间
            DbManager.Instance.UpdateNote(note);

            // 更新备书行的显示
            this.LoadOneNote(note, viewItem);
            this.SetOperateButton(note.Step);
            //viewItem.Selected = true;

            //viewItem.SubItems[5].Text=""
            //viewItem.SubItems[6].Text = GetStepStateText(note.PrintState);
            //viewItem.SubItems[7].Text = note.PrintTime;
            //this.listView_note.Update();
        }



        private string GetStepStateText(string stepState)
        {
            if (stepState == "Y")
                return "完成";

            return stepState;
        }

        private void 打印小票预览ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView_note.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择备书单");
                return;
            }

            ListViewItem viewItem = this.listView_note.SelectedItems[0];
            string noteId = viewItem.SubItems[0].Text;
            Note note = DbManager.Instance.GetNote(noteId);
            if (note == null)
            {
                MessageBox.Show(this, "未找到单号为'" + noteId + "'的记录。");
                return;
            }

            string printTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            note.PrintTime = printTime;
            PrintPreview(note);
        }

        #region 打印功能

        // 打印文件
        private string _printFilename = "print.xml";

        /// <summary>
        /// 输出打印文件
        /// </summary>
        /// <param name="noteId"></param>
        void OutputPrintFile(Note note)
        {
            using (StreamWriter writer = new StreamWriter(this._printFilename,
                false, Encoding.UTF8))
            {
                StringBuilder sb = new StringBuilder(1024);// 256);


                // 备书单整体信息
                string noteId = DbManager.NumToString(note.Id);
                sb.AppendLine("<p>备书单号："+noteId+"</p>"); //备书单id
                sb.AppendLine("<p><b><font size='10'>"+note.PatronName+"</font></b></p>"); //读者姓名
                sb.AppendLine("<p><font size='10'>"+note.PatronTel+"</font></p>"); //读者电话
                sb.AppendLine("<p>"+note.PrintTime+"</p>"); //打印时间
                sb.AppendLine("<p>=================</p>"); //打印时间

                // 预约记录详细信息
                List<ReservationItem> items = DbManager.Instance.GetItemsByNoteId(noteId);
                foreach (ReservationItem item in items)
                {
                    sb.AppendLine("<p><font size='10'>"+item.ItemBarcode+"</font></p>");
                    sb.AppendLine("<p>"+item.Title+"</p>");
                    sb.AppendLine("<p><font size='10'>"+item.Location+"</font></p>");
                    sb.AppendLine("<p><font size='10'>"+item.AccessNo+"</font></p>");
                    sb.AppendLine("<p>"+item.ISBN+"</p>");
                    sb.AppendLine("<p>"+item.Author+"</p>");
                    sb.AppendLine("<p>预约时间：" + item.RequestTime + "</p>");
                    sb.AppendLine("<p>-----------------------------</p>");
                }

                // 加打印内容加上格式
                string wrapText = NoteForm.WrapString(sb.ToString());

                // 写到打印文件
                writer.Write(wrapText);
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

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="noteId"></param>
        private void Print(Note note)
        {
            string strError = "";

            // 鼠标设为等待状态
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            // 输出打印文件
            this.OutputPrintFile(note);

            CardPrintForm form = new CardPrintForm();
            form.PrinterInfo = new PrinterInfo();
            form.CardFilename = this._printFilename;  // 卡片文件名
            form.ShowInTaskbar = false;
            form.WindowState = FormWindowState.Minimized;
            form.Show();  // 必须这样写 2020/2/21 增加备注
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
                this.Cursor = oldCursor;
            }

            
            // 更新note的打印状态和时间

            //// 原来的打印功能
            //ListViewItem[] items = new ListViewItem[this.listView_results.SelectedItems.Count];
            //this.listView_results.SelectedItems.CopyTo(items, 0);
            //changeAcctiveItemPrintState(items, "已打印");
        }

        /// <summary>
        /// 打印预约
        /// </summary>
        /// <param name="noteId"></param>
        private void PrintPreview(Note note)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            string printTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.OutputPrintFile(note);
            CardPrintForm dlg = new CardPrintForm();
            dlg.CardFilename = this._printFilename;  // 卡片文件名
            dlg.PrintPreviewFromCardFile();

            this.Cursor = oldCursor;
        }


        #endregion


    }
}
