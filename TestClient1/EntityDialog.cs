using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.Message;

namespace TestClient1
{
    public partial class EntityDialog : Form
    {
        public Entity Entity { get; set; }

        // 临时存储对象
        Record _oldRecord = null;
        Record _newRecord = null;

        public EntityDialog()
        {
            InitializeComponent();
        }

        private void EntityDialog_Load(object sender, EventArgs e)
        {
            FillInfo();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            UpdateEntity();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        void Clear()
        {
            this.textBox_action.Text = "";
            this.textBox_newRecord.Text = "";
            this.textBox_oldRecord.Text = "";
            this.textBox_refID.Text = "";
            this.textBox_style.Text = "";
        }

        void FillInfo()
        {
            this.Clear();

            if (this.Entity == null)
                return;

            this.textBox_action.Text = this.Entity.Action;
            this.textBox_newRecord.Text = SetInfoDialog.ToString(this.Entity.NewRecord);
            this.textBox_oldRecord.Text = SetInfoDialog.ToString(this.Entity.OldRecord);
            this.textBox_refID.Text = this.Entity.RefID;
            this.textBox_style.Text = this.Entity.Style;

            if (this.Entity.OldRecord != null)
                this._oldRecord = this.Entity.OldRecord.Clone();
            if (this.Entity.NewRecord != null)
                this._newRecord = this.Entity.NewRecord.Clone();
        }

        private void button_editOldRecord_Click(object sender, EventArgs e)
        {
            if (this._oldRecord == null)
                this._oldRecord = new Record();

            RecordDialog dlg = new RecordDialog();
            dlg.Record = this._oldRecord;
            dlg.ShowDialog(this);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;
            this._oldRecord = dlg.Record;
            this.textBox_oldRecord.Text = SetInfoDialog.ToString(this._oldRecord);
        }

        private void button_editNewRecord_Click(object sender, EventArgs e)
        {
            if (this._newRecord == null)
                this._newRecord = new Record();

            RecordDialog dlg = new RecordDialog();
            dlg.Record = this._newRecord;
            dlg.ShowDialog(this);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;
            this._newRecord = dlg.Record;
            this.textBox_newRecord.Text = SetInfoDialog.ToString(this._newRecord);
        }

        void UpdateEntity()
        {
            if (this.Entity == null)
                this.Entity = new DigitalPlatform.Message.Entity();

            this.Entity.Action = this.textBox_action.Text;
            this.Entity.RefID = this.textBox_refID.Text;
            this.Entity.Style = this.textBox_style.Text;

            this.Entity.OldRecord = this._oldRecord;
            this.Entity.NewRecord = this._newRecord;
        }
    }
}
