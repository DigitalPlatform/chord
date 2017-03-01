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
    public partial class RecordDialog : Form
    {
        public Record Record { get; set; }

        public RecordDialog()
        {
            InitializeComponent();
        }

        private void EntityDialog_Load(object sender, EventArgs e)
        {
            FillInfo();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            UpdateRecord();

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
            this.textBox_data.Text = "";
            this.textBox_format.Text = "";
            this.textBox_recPath.Text = "";
            this.textBox_timestamp.Text = "";
        }

        void FillInfo()
        {
            this.Clear();

            if (this.Record == null)
                return;

            this.textBox_data.Text = this.Record.Data;
            this.textBox_format.Text = this.Record.Format;
            this.textBox_recPath.Text = this.Record.RecPath;
            this.textBox_timestamp.Text = this.Record.Timestamp;
        }

        void UpdateRecord()
        {
            if (this.Record == null)
                this.Record = new Record();

            this.Record.Data = this.textBox_data.Text;
            this.Record.Format = this.textBox_format.Text;
            this.Record.RecPath = this.textBox_recPath.Text;
            this.Record.Timestamp = this.textBox_timestamp.Text;
        }
    }
}
