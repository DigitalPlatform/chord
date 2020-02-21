using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2Mini
{
    public partial class checkForm : Form
    {
        public checkForm()
        {
            InitializeComponent();
        }

        public string NoteId { get; set; }
        public string FoundItems = "";
        public string NotFoundItems = "";
        public Hashtable NotFoundReasonHt = new Hashtable();

        private void button_ok_Click(object sender, EventArgs e)
        {
            this.FoundItems = "";
            this.NotFoundItems = "";

            // todo是否需要一个确认步骤

            string strError = "";

            foreach (Control cl in this.groupBox1.Controls)//循环整个form上的控件
            {
                if (cl is CheckBox)//看看是不是checkbox
                {
                    CheckBox cb = cl as CheckBox;//将找到的control转化成checkbox
                    string path = (string)cb.Tag;
                    if (cb.Checked)//判断是否选中
                    {
                        if (FoundItems != "")
                            FoundItems += ",";

                        FoundItems += path;
                    }
                    else
                    {
                        if (NotFoundItems != "")
                            NotFoundItems += ",";

                        NotFoundItems += path;

                        TextBox tb = (TextBox)this.ht[cb];
                        if (tb != null)
                        {
                            if (tb.Text.Trim() == "")
                            {
                                strError+=cb.Text + "，尚未输入未找到原因。\r\n";
                                continue;
                            }

                            this.NotFoundReasonHt[path] = tb.Text.Trim();
                        }
                    }
                }
            }

            // 检查有错的，不能点确定
            if (strError != "")
            {
                MessageBox.Show(this, strError);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        Hashtable ht = new Hashtable();

        private void Form_checkResult_Load(object sender, EventArgs e)
        {

            int x = 10, y = 20;

            List<ReservationItem> items = DbManager.Instance.GetItemsByNoteId(this.NoteId);
            foreach (ReservationItem item in items)
            {
                CheckBox cb = new CheckBox();
                cb.Text = "["+item.ItemBarcode+"]"+item.Title;
                cb.Tag = item.RecPath;
                cb.AutoSize = true;
                cb.Location = new Point(x, y);
                cb.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);

                this.groupBox1.Controls.Add(cb);

                y += 30;

                TextBox tb = new TextBox();
                tb.Size = new Size(400, 30);
                tb.Location = new Point(x, y);
                this.groupBox1.Controls.Add(tb);

                // 建立关联
                this.ht[cb] = tb;

                y += 50;
            }
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb =(CheckBox)sender;

            TextBox tb = (TextBox)this.ht[cb];
            if (tb != null)
            {
                if (cb.Checked == true)
                    tb.Visible = false;
                else
                    tb.Visible = true;
            }
        }
    }
}
