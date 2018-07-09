using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.Forms;
using DigitalPlatform.Message;

namespace TestClient1
{
    public partial class SetInfoDialog : Form
    {
        public SetInfoRequest SetInfoRequest { get; set; }

        public SetInfoDialog()
        {
            InitializeComponent();
        }

        private void SetInfoDialog_Load(object sender, EventArgs e)
        {
            FillInfo();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            UpdateRequest();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public void Clear()
        {
            this.textBox_biblioRecPath.Clear();
            this.comboBox_operation.Text = "";
            this.listView_entities.Items.Clear();
        }

        void FillInfo()
        {
            this.Clear();

            if (this.SetInfoRequest == null)
                return;

            this.textBox_biblioRecPath.Text = this.SetInfoRequest.BiblioRecPath;
            this.comboBox_operation.Text = this.SetInfoRequest.Operation;

            if (this.SetInfoRequest.Entities != null)
            {
                // ListView 天然就是一个存储机制
                foreach (Entity entity in this.SetInfoRequest.Entities)
                {
                    ListViewItem item = new ListViewItem();
                    SetListViewItem(item, entity.Clone());
                    this.listView_entities.Items.Add(item);
                }
            }


        }

        public const int COLUMN_ACTION = 0;
        public const int COLUMN_OLDRECORD = 1;
        public const int COLUMN_NEWRECORD = 2;
        public const int COLUMN_STYLE = 3;
        public const int COLUMN_ERRORINFO = 4;
        public const int COLUMN_ERRORCODE = 5;
        public const int COLUMN_REFID = 6;

        void SetListViewItem(ListViewItem item, Entity entity)
        {
            ListViewUtil.ChangeItemText(item, COLUMN_ACTION, entity.Action);

            ListViewUtil.ChangeItemText(item, COLUMN_REFID, entity.RefID);

            ListViewUtil.ChangeItemText(item, COLUMN_OLDRECORD, ToString(entity.OldRecord).Replace("\r\n", "; "));
            ListViewUtil.ChangeItemText(item, COLUMN_NEWRECORD, ToString(entity.NewRecord).Replace("\r\n", "; "));
            ListViewUtil.ChangeItemText(item, COLUMN_STYLE, entity.Style);
            ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, entity.ErrorInfo);
            ListViewUtil.ChangeItemText(item, COLUMN_ERRORCODE, entity.ErrorCode);

            item.Tag = entity;
        }

        public static string ToString(Record record)
        {
            if (record == null)
                return "{null}";

            StringBuilder text = new StringBuilder();
            text.Append("RecPath=" + record.RecPath + "\r\n");
            text.Append("Format=" + record.Format + "\r\n");
            text.Append("Data=" + record.Data + "\r\n");
            text.Append("Timestamp=" + record.Timestamp + "\r\n");

            return text.ToString();
        }

        void UpdateRequest()
        {
            if (this.SetInfoRequest == null)
                this.SetInfoRequest = new SetInfoRequest();

            this.SetInfoRequest.BiblioRecPath = this.textBox_biblioRecPath.Text;
            this.SetInfoRequest.Operation = this.comboBox_operation.Text;
            this.SetInfoRequest.Entities = new List<Entity>();
            foreach(ListViewItem item in this.listView_entities.Items)
            {
                this.SetInfoRequest.Entities.Add(item.Tag as Entity);
            }
        }

        private void button_new_Click(object sender, EventArgs e)
        {
            EntityDialog dlg = new EntityDialog();
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            ListViewItem item = new ListViewItem();
            SetListViewItem(item, dlg.Entity);
            this.listView_entities.Items.Add(item);

            this.listView_entities.SelectedIndices.Clear();
            ListViewUtil.BeginSelectItem(this.listView_entities, item);
        }

        private void button_modify_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_entities.SelectedItems.Count == 0)
            {
                strError = "尚未选择要修改的事项";
                goto ERROR1;
            }
            ListViewItem item = this.listView_entities.SelectedItems[0];

            EntityDialog dlg = new EntityDialog();
            dlg.Entity = item.Tag as Entity;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            SetListViewItem(item, dlg.Entity);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_delete_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_entities.SelectedItems.Count == 0)
            {
                strError = "尚未选择要删除的事项";
                goto ERROR1;
            }

            ListViewUtil.DeleteSelectedItems(this.listView_entities);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void listView_entities_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_entities.SelectedIndices.Count > 0)
            {
                this.button_delete.Enabled = true;
                this.button_modify.Enabled = true;
            }
            else
            {
                this.button_delete.Enabled = false;
                this.button_modify.Enabled = false;
            }
        }

        private void listView_entities_DoubleClick(object sender, EventArgs e)
        {
            button_modify_Click(sender, e);
        }
    }

}
