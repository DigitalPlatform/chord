using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Mini
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void toolStripMenuItem_prep_Click(object sender, EventArgs e)
        {
            PrepForm prepForm = new PrepForm()
            {
                MdiParent = this,
                Text = "备书"
            };
            prepForm.Show();
        }
    }
}
