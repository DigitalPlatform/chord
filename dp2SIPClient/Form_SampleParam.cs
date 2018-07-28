using SIP2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2SIPClient
{
    public partial class Form_SampleParam : Form
    {
        private Button button_cancel;
        private Button button_ok;
        private Label label2;
        private Label label1;
        private TextBox txtItem;
        private TextBox txtPatron;

        public Form_SampleParam()
        {
            InitializeComponent();
        }



        private void button_ok_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Patron = this.txtPatron.Text.Trim();
            Properties.Settings.Default.Item = this.txtItem.Text.Trim();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void Form_SampleParam_Load(object sender, EventArgs e)
        {
            this.txtPatron.Text = Properties.Settings.Default.Patron;
            this.txtItem.Text= Properties.Settings.Default.Item;
        }
    }



}
