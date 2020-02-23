using DigitalPlatform.LibraryRestClient;
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

            // 接管增加Note事件
            DbManager.Instance.AddNoteHandler -= new AddNoteDelegate(this.AddNoteToListView);
            DbManager.Instance.AddNoteHandler += new AddNoteDelegate(this.AddNoteToListView);
        }

        // 名字以用途命名即可。TokenSource 这种类型名称可以不出现在名字中
        CancellationTokenSource _cancel = new CancellationTokenSource();

        // mid父窗口
        MainForm _mainForm = null;
        /// <summary>
        /// 窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoteForm_Load(object sender, EventArgs e)
        {
            this._mainForm = this.MdiParent as MainForm;

            // 每次开头都重新 new 一个。这样避免受到上次遗留的 _cancel 对象的状态影响
            this._cancel.Dispose();
            this._cancel = new CancellationTokenSource();
            // 开一个新线程
            Task.Run(() =>
            {
                LoadNotes(this._cancel.Token);
            });
        }

        private void NoteForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 取消Note变化事件
            DbManager.Instance.AddNoteHandler -= new AddNoteDelegate(this.AddNoteToListView);

        }

        /// <summary>
        /// 处理外部增加备书单的事件
        /// </summary>
        /// <param name="note"></param>
        public void AddNoteToListView(Note note)
        {
            string noteId = DbManager.NumToString(note.Id);
            ListViewItem viewItem = new ListViewItem(noteId, 0);

            this.listView_note.Items.Insert(0, viewItem);
            this.LoadOneNote(note, viewItem);
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
                    当前进度
                    读者
                    包含的预约记录
                    创建日期

                    */
            string noteId = DbManager.NumToString(note.Id);
            viewItem.Text = noteId;
            viewItem.SubItems.Add(Note.GetStepCaption( note.Step));
            viewItem.SubItems.Add(note.PatronName);
            viewItem.SubItems.Add(note.PatronTel);
            viewItem.SubItems.Add(note.Items);
            viewItem.SubItems.Add(note.CreateTime);

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
            this.ShowDetail(noteId);
        }

        public void ShowDetail(string noteId)
        {
            this.listView_items.Items.Clear();
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
            viewItem.SubItems.Add(this.GetCheckResultText(resItem.CheckResult));
            viewItem.SubItems.Add(resItem.NotFoundReason);
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

            // 让打印按钮一直可以按
            //this.button_print.Enabled
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

        private void 输出小票信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView_note.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择备书单");
                return;
            }

            ListViewItem viewItem = this.listView_note.SelectedItems[0];
            string noteId = viewItem.SubItems[0].Text;
            Note note = DbManager.Instance.GetNote(noteId);

            string[] reasons = this._mainForm.GetSettings().ReasonArray;
            string reasonText = "";
            foreach (string r in reasons)
            {
                if (reasonText != "")
                    reasonText += "\r\n";

                reasonText += "□ " + r;
            }
            if (reasonText != "")
            {
                reasonText = "未找到原因：\r\n" + reasonText;
            }

            StringBuilder sb = new StringBuilder();
            // 备书单整体信息
            sb.AppendLine("备书单号：" + noteId ); //备书单id
            sb.AppendLine(note.PatronName ); //读者姓名
            sb.AppendLine(note.PatronTel ); //读者电话
            sb.AppendLine( note.PrintTime); //打印时间
            sb.AppendLine("================="); //打印时间

            // 预约记录详细信息
            List<ReservationItem> items = DbManager.Instance.GetItemsByNoteId(noteId);
            foreach (ReservationItem item in items)
            {
                sb.AppendLine( item.ItemBarcode);
                sb.AppendLine(item.Title);
                sb.AppendLine(item.Location);
                sb.AppendLine(item.AccessNo);
                sb.AppendLine( item.ISBN );
                sb.AppendLine(item.Author );
                sb.AppendLine( item.RequestTime);
                sb.AppendLine(reasonText);
                sb.AppendLine("--------------------------");
            }

            textForm dlg = new textForm();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.Info = sb.ToString();
            dlg.ShowDialog(this);
        }

        private void 查看备书结果ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem viewItem = this.listView_note.SelectedItems[0];
            string noteId = viewItem.SubItems[0].Text;
            Note note = DbManager.Instance.GetNote(noteId);

            if (note.Step != Note.C_Step_Check
                && note.Step != Note.C_Step_Notice
                && note.Step != Note.C_Step_Takeoff)
            {
                MessageBox.Show(this, "本单尚未备书完成，备书完成才能查看备书结果信息");
                return;
            }

            string info = this.GetResultInfo(noteId);

            textForm dlg = new textForm();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.Info = info;
            dlg.ShowDialog(this);

        }

        public string GetResultInfo(string noteId)
        {
            Note note = DbManager.Instance.GetNote(noteId);
            StringBuilder sb = new StringBuilder();
            // 备书单整体信息
            sb.AppendLine("备书单号：" + noteId); //备书单id
            sb.AppendLine(note.PatronName); //读者姓名
            sb.AppendLine(note.PatronTel); //读者电话
            sb.AppendLine("备书完成时间：" + note.CheckedTime);
            sb.AppendLine("================="); //打印时间

            List<ReservationItem> items = DbManager.Instance.GetItemsByNoteId(noteId);
            foreach (ReservationItem item in items)
            {
                sb.AppendLine(item.ItemBarcode + " " + item.Title);
                sb.AppendLine(GetCheckResultText(item.CheckResult));
                if (item.CheckResult=="N")
                    sb.AppendLine("原因："+item.NotFoundReason);
                sb.AppendLine("----------------------------"); //打印时间
            }
            return sb.ToString();
        }

        public string GetCheckResultText(string checkResult)
        {
            if (checkResult.ToUpper() == "Y")
                return "满足";
            else if (checkResult.ToUpper() == "N")
                return "不满足";
            return checkResult;
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

                string[] reasons = this._mainForm.GetSettings().ReasonArray;
                string reasonText = "";
                foreach (string r in reasons)
                {
                    if (reasonText != "")
                        reasonText += "<br/>";

                    reasonText += "<font size='10'>□</font> " + r;
                }
                if (reasonText !="")
                {
                    reasonText = "<p>图书未找到原因：<br/>" + reasonText+"</p>";
                }


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
                    sb.AppendLine(reasonText);
                    sb.AppendLine("<p>--------------------------</p>");
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

        private void contextMenuStrip_note_Opening(object sender, CancelEventArgs e)
        {
            if (this.listView_note.SelectedItems.Count <= 0)
            {
                this.打印小票预览ToolStripMenuItem.Enabled = false;
                this.输出小票信息ToolStripMenuItem.Enabled = false;
                this.查看备书结果ToolStripMenuItem.Enabled = false;
                this.撤消备书单ToolStripMenuItem.Enabled = false;
            }
            else
            {
                this.打印小票预览ToolStripMenuItem.Enabled = true;
                this.输出小票信息ToolStripMenuItem.Enabled = true;
                this.查看备书结果ToolStripMenuItem.Enabled = true;
                this.撤消备书单ToolStripMenuItem.Enabled = true;
            }
        }

        /// <summary>
        /// 准备图书完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_check_Click(object sender, EventArgs e)
        {
            if (this.listView_note.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择备书单");
                return;
            }

            ListViewItem viewItem = this.listView_note.SelectedItems[0];
            string noteId = viewItem.SubItems[0].Text;
            checkForm dlg = new checkForm(this._mainForm);
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.NoteId = noteId;
            DialogResult ret = dlg.ShowDialog(this);
            if (ret == DialogResult.Cancel)
            {
                // 用户取消操作，则不做什么事情
                return;
            }

            if (ret == DialogResult.OK)
            {
                // 找到的图书
                string foundItems = dlg.FoundItems;
                if (string.IsNullOrEmpty(foundItems) == false)
                {
                    string[] paths = foundItems.Split(new char[] {','});
                    foreach (string path in paths)
                    {
                        // 更新数据库预约记录的备书结果
                        ReservationItem item = DbManager.Instance.GetItem(path);
                        item.CheckResult = "Y";
                        DbManager.Instance.UpdateItem(item);
                    }
                }

                // 未找到的图书
                string notfoundItems = dlg.NotFoundItems;
                if (string.IsNullOrEmpty(notfoundItems) == false)
                {
                    string[] paths = notfoundItems.Split(new char[] { ',' });
                    foreach (string path in paths)
                    {
                        // 更新数据库预约记录的备书结果
                        ReservationItem item = DbManager.Instance.GetItem(path);
                        item.CheckResult = "N";

                        string strReason = (string)dlg.NotFoundReasonHt[path];
                        item.NotFoundReason = strReason;

                        DbManager.Instance.UpdateItem(item);
                    }
                }

                string checkTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Note note = DbManager.Instance.GetNote(noteId);
                note.CheckedTime = checkTime;
                note.CheckResult = "Y";
                note.Step = Note.C_Step_Check;

                // 更新本地数据库备书库打印状态和时间
                DbManager.Instance.UpdateNote(note);

                // 更新备书行的显示
                this.LoadOneNote(note, viewItem);
                this.SetOperateButton(note.Step);

                // 显示详细信息
                this.ShowDetail(noteId);


                //// 从服务器上取消预约，预约记录的状态会从arrived变为outof
                //// 开一个新线程
                //Task.Run(() =>
                //{
                //    DeleteReservation(noteId);
                //});
            }

        }

        public void DeleteReservation(string noteId)
        {

            RestChannel channel = this._mainForm.GetChannel();
            try
            {
                string strError = "";

                List<ReservationItem> items = DbManager.Instance.GetItemsByNoteId(noteId);
                foreach (ReservationItem item in items)
                {
                    //this.AppendNewLine(this.listView_items, item);

                    ReservationResponse response = channel.Reservation("delete",
                        item.PatronBarcode,
                        item.ItemBarcode);
                    if (response.ReservationResult.ErrorCode != ErrorCode.NoError)
                    {
                        strError += response.ReservationResult.ErrorInfo + "\r\n";
                    }

                    // 更新一下本地库的预约记录状态，与服务器保持一致，
                    // 但界面还不会立即反应出来，需要点一下上方的备书单，再显示出来的详细信息就为outof状态了
                    item.State = "outof";
                    DbManager.Instance.UpdateItem(item);
                }


                if (strError != "")
                {
                    // 用Invoke线程安全的方式来调
                    this.Invoke((Action)(() =>
                    {
                        MessageBox.Show(this, "调服务器取消预约出错:" + strError);
                        return;
                    }
                    ));
                }

            }
            finally
            {

                this._mainForm.ReturnChannel(channel);
            }
            
        }



        private void button_notice_Click(object sender, EventArgs e)
        {
            if (this.listView_note.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择备书单");
                return;
            }

            ListViewItem viewItem = this.listView_note.SelectedItems[0];
            string noteId = viewItem.SubItems[0].Text;

            string info = this.GetResultInfo(noteId);
            noticeForm dlg = new noticeForm();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.Info = info;
            DialogResult ret= dlg.ShowDialog(this);
            if (ret == DialogResult.Cancel)
            {
                // 用户取消操作，则不做什么事情
                return;
            }

            Note note = DbManager.Instance.GetNote(noteId);
            string noticeTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            note.NoticeTime = noticeTime;
            note.NoticeState = "Y";
            note.Step = Note.C_Step_Notice;

            // 更新本地数据库备书库打印状态和时间
            DbManager.Instance.UpdateNote(note);

            // 更新备书行的显示
            this.LoadOneNote(note, viewItem);
            this.SetOperateButton(note.Step);
        }

        private void button_takeoff_Click(object sender, EventArgs e)
        {
            MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
            DialogResult dlg = MessageBox.Show(this,"确认关闭备书单吗?", 
                "dp2mini",
                buttons);
            if (dlg == DialogResult.OK)
            {
                ListViewItem viewItem = this.listView_note.SelectedItems[0];
                string noteId = viewItem.SubItems[0].Text;
                Note note = DbManager.Instance.GetNote(noteId);
                string takeoffTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                note.TakeoffTime = takeoffTime;
                note.TakeoffState = "Y";
                note.Step = Note.C_Step_Takeoff;

                // 更新本地数据库备书库打印状态和时间
                DbManager.Instance.UpdateNote(note);

                // 更新备书行的显示
                this.LoadOneNote(note, viewItem);
                this.SetOperateButton(note.Step);

                // 2020/2/22 改在最后一步从服务器取消预约，因为做了这一步会修改服务上预约到书记录的状态为outof，
                // 从服务器上取消预约，预约记录的状态会从arrived变为outof
                // 开一个新线程
                Task.Run(() =>
                {
                    DeleteReservation(noteId);
                });

                //
                //viewItem.BackColor = Color.LightGray;
            }
        }

        private void 撤消备书单ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView_note.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择备书单");
                return;
            }

            ListViewItem viewItem = this.listView_note.SelectedItems[0];
            string noteId = viewItem.SubItems[0].Text;
            Note note = DbManager.Instance.GetNote(noteId);
            if (note.Step == Note.C_Step_Takeoff)
            {
                MessageBox.Show(this, "此备书单已结束，不能撤消。");
                return;
            }

            MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
            DialogResult dlg = MessageBox.Show(this, "您确定要撤消备书单吗？",
                "dp2mini",
                buttons);
            if (dlg == DialogResult.OK)
            {
                // 将下级item的删除
                List<ReservationItem> items = DbManager.Instance.GetItemsByNoteId(noteId);
                foreach (ReservationItem item in items)
                {
                    DbManager.Instance.RemoveItem(item);
                }

                // 从本地备书表中删除
                DbManager.Instance.RemoveNote(noteId);

                // 从界面删除
                this.listView_note.Items.Remove(viewItem);


            }

            
                
        }
    }
}
