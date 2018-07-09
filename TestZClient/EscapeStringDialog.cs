
using System;
using System.Windows.Forms;

using DigitalPlatform.Text;

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
