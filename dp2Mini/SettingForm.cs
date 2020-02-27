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
    public partial class SettingForm : Form
    {
        MainForm _mainFrom = null;
        public SettingForm(MainForm mainForm)
        {
            this._mainFrom = mainForm;
            
            InitializeComponent();
        }



        #region 图书未找到原因

        public string NotFoundReasons
        {
            get
            {
                return this.textBox_reasons.Text.Trim();
            }

            set
            {
                this.textBox_reasons.Text = value;
            }
        }

        #endregion


        private void SettingForm_Load(object sender, EventArgs e)
        {
           

            // 图书未找到原因
            this.NotFoundReasons = this._mainFrom.Setting.NotFoundReasons;


        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            

            this._mainFrom.SaveNotFoundReason(this.NotFoundReasons);


            // 关闭对话框 
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
