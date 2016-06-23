using DigitalPlatform.Forms;
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
    public partial class GetMessageDialog : Form
    {
        public GetMessageDialog()
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

        public string GroupCondition
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

        public string UserCondition
        {
            get
            {
                return this.textBox_userCondition.Text;
            }
            set
            {
                this.textBox_userCondition.Text = value;
            }
        }

        public string TimeCondition
        {
            get
            {
                return this.textBox_timeRange.Text;
            }
            set
            {
                this.textBox_timeRange.Text = value;
            }
        }

        public string SortCondition
        {
            get
            {
                return this.textBox_sortCondition.Text;
            }
            set
            {
                this.textBox_sortCondition.Text = value;
            }
        }

        public string IdCondition
        {
            get
            {
                return this.textBox_idCondition.Text;
            }
            set
            {
                this.textBox_idCondition.Text = value;
            }
        }

        public string SubjectCondition
        {
            get
            {
                return this.textBox_subjectCondition.Text;
            }
            set
            {
                this.textBox_subjectCondition.Text = value;
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_groupName);
                controls.Add(this.textBox_userCondition);
                controls.Add(this.textBox_timeRange);
                controls.Add(this.textBox_sortCondition);
                controls.Add(this.textBox_idCondition);
                controls.Add(this.textBox_subjectCondition);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_groupName);
                controls.Add(this.textBox_userCondition);
                controls.Add(this.textBox_timeRange);
                controls.Add(this.textBox_sortCondition);
                controls.Add(this.textBox_idCondition);
                controls.Add(this.textBox_subjectCondition);
                GuiState.SetUiState(controls, value);
            }
        }

    }
}
