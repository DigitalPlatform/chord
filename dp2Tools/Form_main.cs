// test 20180415

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2Tools
{
    public partial class Form_main : Form
    {
        public Form_main()
        {
            InitializeComponent(); 
        }

        // 巡检工具
        private void ToolStripMenuItem_inspect_Click(object sender, EventArgs e)
        {
            Form_inspect form = new Form_inspect();
            form.MdiParent = this;
            form.Show();
            
        }

        private void ToolStripMenuItem_Class_Click(object sender, EventArgs e)
        {
            Form_Class form = new Form_Class();
            form.MdiParent = this;
            form.MaximizeBox = true;
            form.Show();
        }
    }
}
