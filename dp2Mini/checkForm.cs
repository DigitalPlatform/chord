using System;
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

        private void button_ok_Click(object sender, EventArgs e)
        {
            this.FoundItems = "";
            this.NotFoundItems = "";

            foreach (Control cl in this.groupBox1.Controls)//循环整个form上的控件
            {
                if (cl is CheckBox)//看看是不是checkbox
                {
                    CheckBox cb = cl as CheckBox;//将找到的control转化成checkbox
                    if (cb.Checked)//判断是否选中
                    {
                        if (FoundItems != "")
                            FoundItems += ",";

                        FoundItems += (string)cb.Tag;
                    }
                    else
                    {
                        if (NotFoundItems != "")
                            NotFoundItems += ",";

                        NotFoundItems += (string)cb.Tag;
                    }
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Form_checkResult_Load(object sender, EventArgs e)
        {

            int x = 10, y = 20;

            List<ReservationItem> items = DbManager.Instance.GetItemsByNoteId(this.NoteId);
            foreach (ReservationItem item in items)
            {
                //this.AppendNewLine(this.listView_items, item);
                CheckBox cb = new CheckBox();
                cb.Text = item.ItemBarcode+"-"+item.Title;
                cb.Tag = item.RecPath;
                cb.AutoSize = true;
                cb.Location = new Point(x, y);
                this.groupBox1.Controls.Add(cb);

                y += 20;
            }
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
