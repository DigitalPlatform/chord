using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.Forms;

namespace TestZClient
{
    public partial class PresentDialog : Form
    {
        public PresentDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string ResultSetName
        {
            get
            {
                return this.textBox_resultSetName.Text;
            }
            set
            {
                this.textBox_resultSetName.Text = value;
            }
        }

        public string PresentStart
        {
            get
            {
                return this.textBox_start.Text;
            }
            set
            {
                this.textBox_start.Text = value;
            }
        }

        public string PresentCount
        {
            get
            {
                return this.textBox_count.Text;
            }
            set
            {
                this.textBox_count.Text = value;
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_resultSetName);
                controls.Add(this.textBox_start);
                controls.Add(this.textBox_count);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_resultSetName);
                controls.Add(this.textBox_start);
                controls.Add(this.textBox_count);
                GuiState.SetUiState(controls, value);
            }
        }

    }
}
