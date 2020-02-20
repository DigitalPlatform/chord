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
    public partial class textForm : Form
    {
        public textForm()
        {
            InitializeComponent();
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public string Info
        {
            get
            {
                return this.textBox_info.Text;
            }
            set
            {
                this.textBox_info.Text = value;
            }
        }
    }
}
