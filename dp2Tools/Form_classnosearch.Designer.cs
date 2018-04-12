namespace dp2Tools
{
    partial class Form_classnosearch
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txt_chinese = new System.Windows.Forms.TextBox();
            this.btn_fileupload = new System.Windows.Forms.Button();
            this.btn_search = new System.Windows.Forms.Button();
            this.dgv_result = new System.Windows.Forms.DataGridView();
            this.lbl_message = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_result)).BeginInit();
            this.SuspendLayout();
            // 
            // txt_chinese
            // 
            this.txt_chinese.Location = new System.Drawing.Point(56, 22);
            this.txt_chinese.Name = "txt_chinese";
            this.txt_chinese.ReadOnly = true;
            this.txt_chinese.Size = new System.Drawing.Size(409, 35);
            this.txt_chinese.TabIndex = 0;
            // 
            // btn_fileupload
            // 
            this.btn_fileupload.Location = new System.Drawing.Point(527, 12);
            this.btn_fileupload.Name = "btn_fileupload";
            this.btn_fileupload.Size = new System.Drawing.Size(117, 45);
            this.btn_fileupload.TabIndex = 1;
            this.btn_fileupload.Text = "文件";
            this.btn_fileupload.UseVisualStyleBackColor = true;
            this.btn_fileupload.Click += new System.EventHandler(this.btn_fileupload_Click);
            // 
            // btn_search
            // 
            this.btn_search.Location = new System.Drawing.Point(0, 0);
            this.btn_search.Name = "btn_search";
            this.btn_search.Size = new System.Drawing.Size(75, 23);
            this.btn_search.TabIndex = 5;
            // 
            // dgv_result
            // 
            this.dgv_result.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgv_result.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgv_result.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.dgv_result.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_result.Location = new System.Drawing.Point(56, 105);
            this.dgv_result.Name = "dgv_result";
            this.dgv_result.RowTemplate.Height = 37;
            this.dgv_result.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgv_result.Size = new System.Drawing.Size(736, 613);
            this.dgv_result.TabIndex = 3;
            // 
            // lbl_message
            // 
            this.lbl_message.AutoSize = true;
            this.lbl_message.Location = new System.Drawing.Point(62, 78);
            this.lbl_message.Name = "lbl_message";
            this.lbl_message.Size = new System.Drawing.Size(0, 24);
            this.lbl_message.TabIndex = 4;
            // 
            // Form_classnosearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(816, 714);
            this.Controls.Add(this.lbl_message);
            this.Controls.Add(this.btn_search);
            this.Controls.Add(this.btn_fileupload);
            this.Controls.Add(this.txt_chinese);
            this.Controls.Add(this.dgv_result);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_classnosearch";
            this.Text = "Form_classnosearch";
            ((System.ComponentModel.ISupportInitialize)(this.dgv_result)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_chinese;
        private System.Windows.Forms.Button btn_fileupload;
        private System.Windows.Forms.Button btn_search;
        private System.Windows.Forms.DataGridView dgv_result;
        private System.Windows.Forms.Label lbl_message;
    }
}