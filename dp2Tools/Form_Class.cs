using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace dp2Tools
{
    public partial class Form_Class : Form
    {
        // 简表分类号数组
        List<ClassItem> ClassList = new List<ClassItem>();

        //是否中断
        bool bStop = false;

        // 构造函数
        public Form_Class()
        {
            InitializeComponent();
        }

        // 窗体加载
        private void Form_Class_Load(object sender, EventArgs e)
        {
            // 改为用按钮专门加载
            /*
            // 窗体加载时，将简表txt文件加载到内存

            // 简表文件路径
            string fileName = Application.StartupPath + "/file//basic.txt";
            string error = "";
            bool bRet = this.LoadClassTable(fileName, out error);
            if (bRet == false)
            {
                this.textBox_result.Text = error;
                MessageBox.Show(this, "加载简表有错，详情请查看输出信息");
                return;
            }
            this.textBox_result.Text = "共加载简表中分类号" + this.ClassList.Count+"条";

            */
        }

        // 加载简表到内存
        private bool LoadClassTable(string fileName, out string error)
        {
            error = "";

            if (File.Exists(fileName) == false)
            {
                error = "简表文件[" + fileName + "]不存在";
                return false;
            }

            StringBuilder sb = new StringBuilder();
            // 将简表文件读到内存，要求txt文件排过序
            int i = 0;
            using (StreamReader sr = new StreamReader(fileName, Encoding.Default))
            {
                while (!sr.EndOfStream)
                {
                    //读取一行数据
                    string line = sr.ReadLine().Trim();
                    i++;
                    if (line == "")
                    {
                        sb.AppendLine("第" + i.ToString() + "行为空");
                        continue;
                    }

                    // 按tab键拆分
                    string[] infos = line.Split(new char[] { '\t' });
                    if (infos.Length < 4)
                    {
                        sb.AppendLine("第" + i.ToString() + "行[" + line + "]，不足4个字段\r\n");
                        continue;
                    }
                    ClassItem item = new ClassItem();
                    item.Class = infos[0].Trim();
                    if (item.Class == "")
                    {
                        sb.AppendLine("第" + i.ToString() + "行的分类号为空");
                        continue;
                    }

                    item.Name = infos[1].Trim();
                    if (item.Class == "")
                    {
                        sb.AppendLine("第" + i.ToString() + "行的名称为空");
                    }

                    item.Level = -1;
                    item.Level = Convert.ToInt32(infos[2]);
                    if (item.Level ==-1)
                    {
                        sb.AppendLine("第" + i.ToString() + "行的级别为空");
                    }

                    item.No = Convert.ToInt32(infos[3]);
                    item.Count = 0;

                    // 加到集合里
                    this.ClassList.Add(item);
                }
            }

            // 错误信息
            error = sb.ToString();
            if (error != "")
                return false;

            return true;
        }


        // 匹配简表分类号
        private void button_search_Click(object sender, EventArgs e)
        {
            this.textBox_result.Text = "";
            string[] lines = this.GetLines(this.textBox_inputClass.Text.Trim());

            int totalCount = lines.Length;
            this.toolStripProgressBar1.Maximum = totalCount;
            this.toolStripProgressBar1.Minimum = 0;
            int count = 0;
            foreach (string line in lines)
            {
                if (bStop == true)
                {
                    MessageBox.Show(this, "用户中断，当前处理到第"+count+"个");
                    break;
                }

                count ++;
                this.toolStripProgressBar1.Value = count;
                this.toolStripStatusLabel1.Text = count.ToString() + "/" + totalCount;
                ClassItem classItem = null;
                string outputInfo = this.SearchClass(line, out classItem);
                this.textBox_result.Text += outputInfo;

                // 出让控制权
                Application.DoEvents();
            }
        }

        // 输入一个分类号，匹配简表对应分类号
        private string SearchClass(string inputClass,
            out ClassItem classItem)
        {
            classItem = null;

            Debug.Assert(String.IsNullOrEmpty(inputClass)==false, "输入的分类号不能为空");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("~查询[" + inputClass + "]开始~");
            string thisClass = inputClass;
            while (thisClass != "")
            {
                classItem = this.SearchOneClass(thisClass);
                if (classItem != null)
                {
                    sb.AppendLine("["+thisClass+"]找到了");
                    break;
                }
                sb.AppendLine("["+thisClass+"]未找到");
                thisClass = thisClass.Substring(0, thisClass.Length - 1);
            }
            sb.AppendLine("结束");
            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// 输入一个分类号，从简表中检索对应的短分类号
        /// </summary>
        /// <param name="inputClass">输入的分类号</param>
        /// <param name="classItem">命中的简表分类号对象</param>
        /// <param name="outputInfo">匹配过程信息</param>
        /// <returns>
        /// true 匹配上
        /// false 未命中
        /// </returns>
        private ClassItem SearchOneClass(string inputClass)
        {
            Debug.Assert(String.IsNullOrEmpty(inputClass)==false, "输入的分类号不能为空");

            // 大写处理
            inputClass = inputClass.ToUpper();
            // 第一个字母
            string firstChar = inputClass.Substring(0, 1);

            // 是否进入比较区段
            bool bStart = false;

            // 循环简表，超过大类区段则不再继续比较
            foreach (ClassItem item in this.ClassList)
            {
                string thisFirst = item.Class.Substring(0, 1);
                if (thisFirst == firstChar)
                {
                    // 进入比较区段
                    if (bStart == false)
                        bStart = true;

                    if (inputClass == item.Class)
                    {
                        // 次数加1
                        item.Count++;
                        return item;
                    }
                }
                else
                {
                    // 大类字母超过区段，不再比较后面
                    if (bStart == true)
                        break;
                }
            }


            return null;
        }

        // 将多行文本转成数组
        public string[] GetLines(string inputText)
        {
            string text = inputText.Trim();
            text = text.Replace("\r\n", "\n");
            text = text.Replace("\r", "\n");
            string[] lines = text.Split(new char[] { '\n' });
            return lines;
        }

        private void button_outputCount_Click(object sender, EventArgs e)
        {
            this.textBox_result.Text = "";

            StringBuilder sb = new StringBuilder();
            foreach (ClassItem item in this.ClassList)
            {
                sb.AppendLine(item.Dump());
            }

            this.textBox_result.Text = sb.ToString();
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            this.bStop = true;
        }

        private void button_loadClassTable_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要加载的简表文件名";
            // dlg.FileName = this.textBox_filename.Text;

            dlg.Filter = "简表文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            // 简表文件路径
            string fileName = dlg.FileName;

            string error = "";
            bool bRet = this.LoadClassTable(fileName, out error);
            if (bRet == false)
            {
                this.textBox_result.Text = error;
                MessageBox.Show(this, "加载简表有错，详情请查看输出信息");
                return;
            }
            this.textBox_result.Text = "共加载简表中分类号" + this.ClassList.Count + "条";
        }
    }


}
