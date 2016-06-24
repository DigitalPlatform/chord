using DigitalPlatform.Forms;
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

namespace TestClient1
{
    public partial class SendMessageDialog : Form
    {
        public SendMessageDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public string GroupName
        {
            get
            {
                return this.textBox_groupName.Text;
            }
            set
            {
                this.textBox_groupName.Text = value;
            }
        }

        public string Data
        {
            get
            {
                return this.textBox_message_text.Text;
            }
            set
            {
                this.textBox_message_text.Text = value;
            }
        }

        public string [] Subjects
        {
            get
            {
                return this.textBox_subjects.Text.Replace("\r\n", "\r").Split(new char[] {'\r'});
            }
            set
            {
                this.textBox_subjects.Text = string.Join("\r\n", value);
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_groupName);
                controls.Add(this.textBox_message_text);
                controls.Add(this.textBox_subjects);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_groupName);
                controls.Add(this.textBox_message_text);
                controls.Add(this.textBox_subjects);
                GuiState.SetUiState(controls, value);
            }
        }
    }
}
