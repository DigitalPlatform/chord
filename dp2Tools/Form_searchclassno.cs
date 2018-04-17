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
    public partial class Form_searchclassno : Form
    {
        List<BasicNo> list_basic = new List<BasicNo>();
        public Form_searchclassno()
        {
            InitializeComponent();
            string fileName = "..//..//file//basic.txt";
            try
            {
                //使用StreamReader读取文件.txt
                using (StreamReader sr = new StreamReader(fileName, Encoding.Default))
                {
                    while (!sr.EndOfStream)
                    {
                        string readStr = sr.ReadLine();//读取一行数据
                        string[] strs = readStr.Split(new char[] { '\t', '"' }, StringSplitOptions.RemoveEmptyEntries);//将读取的字符串按"制表符/t“和””“分割成数组
                        BasicNo basicno = new BasicNo(strs[0], 0);
                        list_basic.Add(basicno);
                    }
                }

                //结束时间-开始时间=总共花费的时间
                // TimeSpan ts = DateTime.Now - startTime;
                lbl_msg.Text = "简表分类号 " + list_basic.Count.ToString() + "笔";

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

        private void btn_search_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < txt_chinese.Lines.Length; i++)
            {
                txt_result.Text = txt_result.Text + "查询" + txt_chinese.Lines[i].ToString() + "开始\r\n";
                string str_chinese = "";
                for (int j = txt_chinese.Lines[i].Length; j > 0; j--)
                {

                    str_chinese = txt_chinese.Lines[i].Substring(0, j);
                    if (serchBasicNo(str_chinese))
                    {
                        txt_result.Text = txt_result.Text + str_chinese + "命中\r\n";
                        break;
                    }
                    else
                    {
                        txt_result.Text = txt_result.Text + str_chinese + "未命中\r\n";
                    }
                }
                txt_result.Text = txt_result.Text + "查询" + txt_chinese.Lines[i].ToString() + "结束\r\n";
            }
            for (int i = 0; i < list_basic.Count; i++)
            {
                txt_result.Text = txt_result.Text + list_basic[i].getBasicNo().ToString() +"命中"+ list_basic[i].getCount().ToString() + "次\r\n";
            }
            
        }
        public bool serchBasicNo(string inp_classno)
        {
            bool flag = false;
            for (int i = 0; i < list_basic.Count; i++)
            {
                if (list_basic[i].getBasicNo().ToString() == inp_classno)
                {
                    list_basic[i].setBasicCount(list_basic[i].getCount() + 1);
                    flag = true;
                    break;
                }
             }
            return flag;
        }
    }
}
