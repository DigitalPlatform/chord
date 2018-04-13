using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2Tools
{
    public partial class Form_classnosearch : Form
    {
        DataTable dt_table = new DataTable();
        DataTable dt_chinese = new DataTable();
        public Form_classnosearch()
        {
            InitializeComponent();
            dt_table.Columns.Add("classno", typeof(string));
            dt_table.Columns.Add("count", typeof(int));
            dt_chinese.Columns.Add("classno", typeof(string));
            //获得文件名包括路径
            string fileName = "..//..//file//basic.txt";
            try
            {
                //定义一个开始时间
                DateTime startTime = DateTime.Now;
                //因为文件比较大，所有使用StreamReader的效率要比使用File.ReadLines高
                using (StreamReader sr = new StreamReader(fileName, Encoding.Default))
                {
                    while (!sr.EndOfStream)
                    {
                        string readStr = sr.ReadLine();//读取一行数据
                        string[] strs = readStr.Split(new char[] { '\t', '"' }, StringSplitOptions.RemoveEmptyEntries);//将读取的字符串按"制表符/t“和””“分割成数组
                        DataRow rows = dt_table.NewRow();
                        rows["classno"] = strs[0];
                        rows["count"] = 0;
                        dt_table.Rows.Add(rows);
                    }
                }

                //结束时间-开始时间=总共花费的时间
               // TimeSpan ts = DateTime.Now - startTime;
                lbl_message.Text = "简表分类号 "   + dt_table.Rows.Count.ToString() + "笔";

            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    

        private void btn_fileupload_Click(object sender, EventArgs e)
        {
            if (dgv_result.DataSource != null)
            {
                dgv_result.DataSource = null;
            }
            else
            {
                dgv_result.Rows.Clear();
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Files|*.txt";
            openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            txt_chinese.Text = openFileDialog.FileName;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                dt_chinese.Rows.Clear();

                try
                {
                    //定义一个开始时间
                    DateTime startTime = DateTime.Now;
                    //因为文件比较大，所有使用StreamReader的效率要比使用File.ReadLines高
                    using (StreamReader sr = new StreamReader(openFileDialog.FileName, Encoding.Default))
                    {

                        while (!sr.EndOfStream)
                        {

                            string readStr = sr.ReadLine();//读取一行数据
                            string[] strs = readStr.Split(new char[] { '\t', '"' }, StringSplitOptions.RemoveEmptyEntries);//将读取的字符串按"制表符/t“和””“分割成数组
                            DataRow rows = dt_chinese.NewRow();
                            rows["classno"] = strs[0];
                            dt_chinese.Rows.Add(rows);
                        }


                    }

                    //结束时间-开始时间=总共花费的时间
                    TimeSpan ts = DateTime.Now - startTime;
                    lbl_message.Text= "导入中图分类号"+ dt_chinese.Rows.Count.ToString()+"笔成功！" ;

                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            
            int count = 0;

            for (int i = 0; i <= dt_chinese.Rows.Count; i++)
            {
                if (count == 0)
                {
                    if (i != 0 && dt_chinese.Rows[i - 1][0].ToString().Length > 0)
                    {
                        i = i - 1;
                        dt_chinese.Rows[i][0] = dt_chinese.Rows[i][0].ToString().Substring(0, dt_chinese.Rows[i][0].ToString().Length - 1);
                    }
                }
                if (dt_chinese.Rows.Count == i)
                {
                    break;
                }
                count = 0;


                for (int j = 0; j < dt_table.Rows.Count; j++)
                {//如果匹配到即中止
                    if (dt_table.Rows[j][0].ToString() == dt_chinese.Rows[i][0].ToString())
                    {
                        dt_table.Rows[j][1] = int.Parse(dt_table.Rows[j][1].ToString()) + 1;
                        count++;
                        break;

                    }

                }
            }
            DataTable table = new DataTable();
            DataColumn column = new DataColumn();

            column.ColumnName = "序号";
            column.AutoIncrement = true;
            column.AutoIncrementSeed = 1;
            column.AutoIncrementStep = 1;

            table.Columns.Add(column);
            table.Merge(dt_table);
            //datagridview1.DataSource = table;
            //datagridview1.Columns["序号"].DisplayIndex = 0;//调整列顺序
            dgv_result.DataSource = table;
            dgv_result.Columns["序号"].DisplayIndex = 0;//调整列顺序
            dgv_result.Columns[1].HeaderCell.Value = "简表分类号";
            dgv_result.Columns[2].HeaderCell.Value = "命中次数";
            dgv_result.RowHeadersVisible = false;
            dgv_result.ColumnHeadersVisible = false;
        }
    }
}
