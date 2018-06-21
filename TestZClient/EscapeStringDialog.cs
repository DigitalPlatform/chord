using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestZClient
{
    public partial class EscapeStringDialog : Form
    {
        public EscapeStringDialog()
        {
            InitializeComponent();
        }

        private void button_convert_Click(object sender, EventArgs e)
        {
            this.textBox_converted.Text = StringUtil.EscapeString(this.textBox_text.Text, "\"/=");
        }
    }
}
