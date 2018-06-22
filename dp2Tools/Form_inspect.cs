
using DigitalPlatform.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2Tools
{
    public partial class Form_inspect : Form
    {
        //22
        public static string[] danger_rights = new string[] { 
        "batchtask",
        "clearalldbs",
        "devolvereaderinfo",
        "changeuser",
        "newuser",
        "deleteuser",
        "changeuserpassword",
        "simulatereader",
        "simulateworker",
        "setsystemparameter",
        "urgentrecover",
        "repairborrowinfo",
        "settlement",
        "undosettlement",
        "deletesettlement",
        "writerecord",
        "managedatabase",
        "restore",
        "managecache",
        "managechannel",
        "upload",
        "bindpatron",
        };

        public Form_inspect()
        {
            InitializeComponent();
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            string result = "";
            string[] lines = this.GetLines();
            foreach (string acc in lines)
            {
                if (acc=="")
                continue;

                int nIndex = acc.IndexOf("~");
                if (nIndex == -1)
                    continue;

                string userName = acc.Substring(0, nIndex);
                string account = acc.Substring(nIndex + 1);


                string[] rightList = account.Split(',');

                string hasRights = "";
                int count = 0;

                foreach (string danger in danger_rights)
                {
                    if (rightList.Contains(danger) == true)
                    {
                        if (hasRights != "")
                        {
                            hasRights += ",";
                        }

                        hasRights += danger;
                        count++;
                    }
                }

                result += userName + "包含下列" + count + "个危险权限\n" + hasRights + "\r\n";

            }

            this.txtResult.Text = result;

            MessageBox.Show("ok");
        }






        private void btnReaderTypeStatic_Click(object sender, EventArgs e)
        {
            Hashtable keyvalues = new Hashtable();

            string[] lines = this.GetLines();
            foreach (string line1 in lines)
            {
                string line = line1;
                if (line == "")
                    line = "[空白]";

                //int nIndex = line.IndexOf('\t');
                //string key = line.Substring(0, nIndex);
                //string value = line.Substring(nIndex + 1);


                int value = 0;
                if (keyvalues.ContainsKey(line) == true)
                {
                    value = (int)keyvalues[line];
                }

                value++;

                keyvalues[line] = value;
 
            }

            string result = "";
            foreach (string key in keyvalues.Keys)
            {
                if (result != "")
                    result += "\r\n";

                result += key +"\t"+ keyvalues[key];
            }
            this.txtResult.Text = result;

            MessageBox.Show("ok");

        }

        private void btnDepartment_Click(object sender, EventArgs e)
        {
            Hashtable keyvalues = new Hashtable();
            Hashtable pathList = new Hashtable();

            string[] lines = this.GetLines();
            foreach (string line in lines)
            {
                if (line == "")
                    continue;

                string path = line;
                string key = "";

                int nIndex = line.IndexOf('\t');
                if (nIndex > 0)
                {
                    path = line.Substring(0, nIndex);
                    key = line.Substring(nIndex + 1);
                }

                int value = 0;
                if (keyvalues.ContainsKey(key) == true)
                {
                    value = (int)keyvalues[key];
                }

                value++;

                keyvalues[key] = value;
                pathList[key] = path;

            }

            string result = "";
            foreach (string key in keyvalues.Keys)
            {
                if (result != "")
                    result += "\r\n";

                result += key + "\t" + keyvalues[key] +"\t"+pathList[key];
            }
            this.txtResult.Text = result;

            MessageBox.Show("ok");

        }

        public string[] GetLines()
        {
            string text = this.txtInput.Text.Trim();
            text = text.Replace("\r\n", "\n");
            text = text.Replace("\r", "\n");
            string[] lines = text.Split(new char[] { '\n' });
            return lines;
        }
        private void btnPrice_Click(object sender, EventArgs e)
        {
            this.toolStripStatusLabel_info.Text = "开始";

            string[] lines = this.GetLines();
            string result = "";

            string match = "^[0-9]*([.][0-9]*)$";// this.txtMatch.Text.Trim();

            //空价格的
            List<string> emptyList= new List<string>();
            //高于500价格
            List<string> largeList = new List<string>();
            //括号
            List<string> bracketList = new List<string>();
            //其它
            List<string> otherList = new List<string>();

            foreach (string line in lines)
            {
                if (line == "")
                    continue;

                string path = line;
                string price = "";

                int nIndex = line.IndexOf('\t');
                if (nIndex > 0)
                {
                    path = line.Substring(0, nIndex);
                    price = line.Substring(nIndex + 1);
                }

                this.toolStripStatusLabel_info.Text = "处理 " +line;
                Application.DoEvents();


                string retLine = path + "\t" + price;

                if (price == "")
                {
                    emptyList.Add(retLine);
                    continue;
                }

                if (price.Length > 3 &&
                    (price.Substring(0, 3) == "CNY"
                    || price.Substring(0, 3) == "USD"
                    || price.Substring(0, 3) == "KWR"
                    || price.Substring(0, 3) == "TWD"
                    || price.Substring(0, 3) == "HKD"
                    || price.Substring(0, 3) == "JPY"
                    || price.Substring(0, 3) == "EUR")
                    ) 
                {
                    string right = price.Substring(3);

                    // 未定义
                    if (right == "")
                    {
                        emptyList.Add(retLine);
                        continue;
                    }

                    //大于500
                    try
                    {
                        double dPrice = Convert.ToDouble(right);
                        if (dPrice > 100)
                        {
                            largeList.Add(retLine);
                        }
                        continue;
                    }
                    catch {

                        int nTemp = right.IndexOf('/');
                        if (nTemp > 0)
                        {
                            string r1 = right.Substring(0, nTemp);
                            string r2 = right.Substring(nTemp + 1);
                            try
                            {
                                double dR1 = Convert.ToDouble(r1);
                                double dR2 = Convert.ToDouble(r2);
                                continue;
                            }
                            catch
                            {
                                
                            }

                        }
                    
                    }
                    
                    // 正常的
                    bool bRet = Regex.IsMatch(right,match);// "^[0-9]*(.[0-9])$");
                    if (bRet == true)
                        continue;

                    //带括号的
                    if (right.IndexOf("(")!=-1 || right.IndexOf("（")!=-1)
                    {
                        bracketList.Add(retLine);
                        continue;

                    }

                }

                otherList.Add(retLine);
            }



            //输出
            result += "==价格为空" + emptyList.Count + "条==";
            foreach (string li in emptyList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }
            //
            result += "\r\n\r\n==价格超过500的" + largeList.Count + "条==";
            foreach (string li in largeList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }
            //
            result += "\r\n\r\n==价格带括号的" + bracketList.Count + "条==";
            foreach (string li in bracketList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }
            //
            result += "\r\n\r\n==其它不合规则的" + otherList.Count + "条==";
            foreach (string li in otherList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }

            this.txtResult.Text = result;

            MessageBox.Show("ok");
            this.toolStripStatusLabel_info.Text = "结束";

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000; toolTip1.InitialDelay = 500; toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.btnCheck, "格式：一个帐户一行，帐户名与权限用~分隔");

            toolTip1.SetToolTip(this.btnReaderTypeStatic, "格式：读者类型 或者 图书类型(每行只包括1列 )。");

            toolTip1.SetToolTip(this.btnDepartment, "格式：路径\t单位(每行包括2列 )");

            toolTip1.SetToolTip(this.btnPrice, "格式：路径\t价格(每行包括2列 )");

            toolTip1.SetToolTip(this.btnAccessNo, "格式：路径\t索取号(每行包括2列 )");
        }

        private void 南开实验学校ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string result = "";
            string[] lines = this.GetLines();
            foreach (string line in lines)
            {
                if (line == "")
                    continue;

                string[] items = line.Split(new char[] { '\t' });

                string path = items[0];
                string barcode = items[1];

                // 册条码
                if (StringUtil.Between(barcode, "000001", "999999")
                    || StringUtil.Between(barcode, "X000001", "X999999"))
                {
                    continue;
                }

                if (result != "")
                    result += "\r\n";

                result += path + "\t" + barcode;
            }

            this.txtResult.Text = result;
            MessageBox.Show("ok");
        }

        private void 北师大天津附中ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string result = "";
            string[] lines = this.GetLines();
            foreach (string line in lines)
            {
                if (line == "")
                    continue;

                string path = line;

                string barcode = "";

                string[] items = line.Split(new char[] { '\t' });

                if (items.Length >=1)
                    path = items[0];
                 
                if (items.Length >=2)
                    barcode = items[1];

                // 册条码
                if (barcode.Length == 8 && barcode[0] == 'B')
                {
                    continue;
                }


                if (result != "")
                    result += "\r\n";

                result += path + "\t" + barcode;
            }

            this.txtResult.Text = result;
            MessageBox.Show("ok");


            /*
            string strFilePath = "D:/北附册条码.txt";
            if (File.Exists(strFilePath) == false)
            {
                MessageBox.Show( "文件 '" + strFilePath + "' 不存在");
                return;
            }

            Encoding encoding = FileUtil.DetectTextFileEncoding(strFilePath);

            try
            {
                using (FileStream file = File.Open(
                    strFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))

                // TODO: 这里的自动探索文件编码方式功能不正确，
                // 需要专门编写一个函数来探测文本文件的编码方式
                // 目前只能用UTF-8编码方式
                using (StreamReader sr = new StreamReader(file, encoding))
                {

                    string result = "";

                    for (; ; )
                    {
                        string strLine = sr.ReadLine();
                        if (strLine == null)
                            break;

                        if (strLine == "")
                            continue;

                        string[] items = strLine.Split(new char[] { '\t' });

                        string path = items[0];
                        string barcode = items[1];

                        string msg = "path=[" + path + "] barcode=[" + barcode + "]";

                        if (barcode.Length == 8 && barcode[0] == 'B')
                        {
                        }
                        else
                        {
                            if (result != "")
                                result += "\r\n";

                            result += path + "\t" + barcode;
                        }

                        //MessageBox.Show(msg);
                        //return 0;

                    }

                    this.txtResult.Text = result;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("打开或读入文件 '" + strFilePath + "' 时出错: " + ex.Message);
                return;
            }

            MessageBox.Show("ok");
             */
        }

        private void btnAccessNo_Click(object sender, EventArgs e)
        {
            this.toolStripStatusLabel_info.Text = "开始";

            string[] lines = this.GetLines();
            string result = "";

            //string match = "^[0-9]*([.][0-9]*)$";// this.txtMatch.Text.Trim();

            //空索取号的
            List<string> emptyList = new List<string>();

            //没有斜撇
            List<string> noXpList = new List<string>();

            //有斜撇，但左或右没有值
            List<string> hasXpNoValueList = new List<string>();

            //左右有空格的
            List<string> hasKgList = new List<string>();




            List<string> leftWrongList = new List<string>();
            List<string> rightWrongList = new List<string>();

            // 其它
            List<string> otherList = new List<string>();

            foreach (string line in lines)
            {
                if (line == "")
                    continue;

                string path = line;
                string accessNo = "";

                int nIndex = line.IndexOf('\t');
                if (nIndex > 0)
                {
                    path = line.Substring(0, nIndex);
                    accessNo = line.Substring(nIndex + 1);
                }

                this.toolStripStatusLabel_info.Text = "处理 " + line;
                Application.DoEvents();


                string retLine = path + "\t" + accessNo;

                // 无索取号的
                if (accessNo == "")
                {
                    emptyList.Add(retLine);
                    continue;
                }

                int nTemp = accessNo.IndexOf('/');
                if (nTemp == -1)
                {
                    //索取号不包括/
                    noXpList.Add(retLine);
                    continue;
                }

                string left = accessNo.Substring(0, nTemp);
                string right = accessNo.Substring(nTemp + 1);
                if (left == "" || right == "")
                {
                    //有斜撇，但左或右没有值
                    hasXpNoValueList.Add(retLine);
                    continue;
                }

                string left1 = left.Trim();
                string right1 = right.Trim();
                if (left1 != left || right1 != right)
                {
                    //左右有空格的
                    hasKgList.Add(retLine);
                    continue;
                }

                // 左侧不是字母的
                string firstLeft = left.Substring(0, 1);
                if (StringUtil.Between(firstLeft, "A", "Z") == false)
                {
                    leftWrongList.Add(retLine);
                    continue;
                }

                // 右边不合适
                try
                {
                    double d = Convert.ToDouble(right);
                    continue;
                }
                catch
                {
                    string firstRight = right.Substring(0, 1);
                    if (StringUtil.Between(firstRight, "A", "Z") == false)
                    {
                        nTemp = right.IndexOf(":");
                        if (nTemp == -1)
                            nTemp = right.IndexOf("：");
                        if (nTemp == -1)
                            nTemp = right.IndexOf("=");
                        if (nTemp == -1)
                            nTemp = right.IndexOf(";");
                        if (nTemp == -1)
                            nTemp = right.IndexOf("；");


                        if (nTemp > 0)
                        {
                            string strFirst = right.Substring(0, nTemp);
                            //string strEnd = right.Substring(nTemp + 1);
                            try
                            {
                                int n = Convert.ToInt32(strFirst);
                                continue;
                            }
                            catch
                            { }
                        }

                        rightWrongList.Add(retLine);
                        continue;
                    }
                    else
                    {

                        continue;
                    }

                }

                //
                otherList.Add(retLine);

            }



            //空索取号的
            result += "##索取号为空的" + emptyList.Count + "条";
            foreach (string li in emptyList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }
            //没有斜
            result += "\r\n##没有斜的" + noXpList.Count + "条";
            foreach (string li in noXpList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }
            //有斜，但左或右没值
            result += "\r\n##有斜，但左或右没值的" + hasXpNoValueList.Count + "条";
            foreach (string li in hasXpNoValueList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }

            //左边错误
            result += "\r\n##左边错误的" + leftWrongList.Count + "条";
            foreach (string li in leftWrongList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }


            //右边错误
            result += "\r\n##右边错误" + rightWrongList.Count + "条";
            foreach (string li in rightWrongList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }

            //其它
            result += "\r\n##其它" + otherList.Count + "条";
            foreach (string li in otherList)
            {
                if (result != "")
                    result += "\r\n";
                result += li;
            }

            this.txtResult.Text = result;

            MessageBox.Show("ok");
            this.toolStripStatusLabel_info.Text = "结束";

        }

        private void btnBu0_Click(object sender, EventArgs e)
        {
            string[] lines = this.GetLines();
            string result = "";
            foreach(string line1 in lines)
            {
                string line = "'"+line1.PadLeft(10, '0')+"'";

                if (result != "")
                    result += ",";

                result += line;

            }

            this.txtResult.Text = result;
        }

        private void 瑞景中学册条码分析ToolStripMenuItem_Click(object sender, EventArgs e)
        {
                        string[] lines = this.GetLines();
            string result = "";

            int count5 = 0;
            int count6 = 0;
            int count7 = 0;
            int count8 = 0;

            int RJDZTS_count = 0;
            int RJRDZL_count = 0;
            int RJTS_count = 0;
            int RJTS6_count = 0;
            int rjts_count = 0;
            int rjtsx_count = 0;
            int RJQK_count = 0;

            int other_count = 0;
            string others = "";

            foreach (string line1 in lines)
            {
                string line =line1;
                if (line == "")
                    continue;

                if (StringUtil.Between(line, "00000", "99999"))
                {
                    count5++;
                    continue;
                }

                if (StringUtil.Between(line, "000000", "999999"))
                {
                    count6++;
                    continue;
                }

                if (StringUtil.Between(line, "0000000", "9999999"))
                {
                    count7++;
                    continue;
                }

                if (StringUtil.Between(line, "00000000", "99999999"))
                {
                    count8++;
                    continue;
                }


                /*
                 * RJDZTS+5位数字 如RJDZTS00001
RJRDZL+5位数字 如RJRDZL00055
RJTS+5位数字 如RJTS00429
rjts+5位数字 如rjts12744
rjtsx+5位数字 如rjtsx12773
                 * RJQK+5位数字 如RJQK00001
                 */
                if (StringUtil.Between(line, "RJDZTS00000", "RJDZTS99999"))
                {
                    RJDZTS_count++;
                    continue;
                }

                if (StringUtil.Between(line, "RJRDZL00000", "RJRDZL99999"))
                {
                    RJRDZL_count++;
                    continue;
                }

                if (StringUtil.Between(line, "RJTS00000", "RJTS99999"))
                {
                    if (line.Substring(0, 4) == "rjts")
                    {
                        rjts_count++;
                        continue;
                    }

                    RJTS_count++;
                    continue;
                }

                if (StringUtil.Between(line, "RJTS000000", "RJTS999999"))
                {
                    RJTS6_count++;
                    continue;
                }

                //rjtsx
                if (StringUtil.Between(line, "rjtsx00000", "rjtsx99999"))
                {
                    rjtsx_count++;
                    continue;
                }

                //rjtsx
                if (StringUtil.Between(line, "RJQK00000", "RJQK99999"))
                {
                    RJQK_count++;
                    continue;
                }

                other_count++;
                if (others != "")
                {
                    others += "\r\n";
                }
                others += line;

            }

            //输出
            /*
            int count5 = 0;
            int count6 = 0;
            int count7 = 0;
            int count8 = 0;

            int RJDZTS_count = 0;
            int RJRDZL_count = 0;
            int RJTS_count = 0;
            int rjts_count = 0;
            int rjtsx_count = 0;
            int RJQK_count = 0;

            int other_count = 0;
            string others = "";
             */

            result += "5位纯数字\t" + count5 + "\r\n";
            result += "6位纯数字\t" + count6 + "\r\n";
            result += "7位纯数字\t" + count7 + "\r\n";
            result += "8位纯数字\t" + count8 + "\r\n";


            /*
             *                  * RJDZTS+5位数字 如RJDZTS00001
RJRDZL+5位数字 如RJRDZL00055
RJTS+5位数字 如RJTS00429
rjts+5位数字 如rjts12744
rjtsx+5位数字 如rjtsx12773
                 * RJQK+5位数字 如RJQK00001
             */
            result += "RJDZTS+5位数字\t" + RJDZTS_count + "\r\n";
            result += "RJRDZL+5位数字\t" + RJRDZL_count + "\r\n";
            result += "RJTS+5位数字\t" + RJTS_count + "\r\n";
            result += "rjts+5位数字\t" + rjts_count + "\r\n";
            result += "RJTS+6位数字\t" + RJTS6_count + "\r\n";
            result += "rjtsx+5位数字\t" + rjtsx_count + "\r\n";
            result += "RJQK+5位数字\t" + RJQK_count + "\r\n";

            result += "其它\t" + other_count + "\r\n";

            result += others;

            this.txtResult.Text = result;
            MessageBox.Show("ok");


        }

        private void 中规院ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string result = "";
            string[] lines = this.GetLines();
            foreach (string line in lines)
            {
                if (line == "")
                    continue;

                string[] items = line.Split(new char[] { '\t' });

                string path = items[0];
                string barcode = items[1];

                // 册条码
                if (StringUtil.Between(barcode, "ZGS000001", "ZGS999999")
                    || StringUtil.Between(barcode, "ZGY000001","ZGY999999"))
                {
                    continue;
                }

                if (result != "")
                    result += "\r\n";

                result += path + "\t" + barcode;
            }

            this.txtResult.Text = result;
            MessageBox.Show("ok");
        }


        private void 河西教育中心ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            string result = "";
            string[] lines = this.GetLines();
            foreach (string line in lines)
            {
                if (line == "")
                    continue;

                string[] items = line.Split(new char[] { '\t' });

                string path = items[0];
                string barcode = items[1];

                // 册条码
                if (StringUtil.Between(barcode, "HXE0000001", "HXE9999999")  //图书
        || StringUtil.Between(barcode, "HXEX000001", "HXEX999999")  //现刊
        || StringUtil.Between(barcode, "HXEG000001", "HXEG999999")  //过刊
        )
                {
                    continue;
                }

                if (result != "")
                    result += "\r\n";

                result += path + "\t" + barcode;
            }

            this.txtResult.Text = result;
            MessageBox.Show("ok");
        }


        private void 河北博物院ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string result = "";
            string[] lines = this.GetLines();
            foreach (string line in lines)
            {
                if (line == "")
                    continue;

                string[] items = line.Split(new char[] { '\t' });

                string path = items[0];
                string barcode = items[1];

                // 册条码
                if (StringUtil.Between(barcode, "0000001", "9999999")
                    || StringUtil.Between(barcode, "000001", "999999")
                    || StringUtil.Between(barcode, "201700001", "201799999")  
                    || StringUtil.Between(barcode, "qk000000001", "qk999999999")  //现刊
        )
                {
                    continue;
                }


                if (result != "")
                    result += "\r\n";

                result += path + "\t" + barcode;
            }

            this.txtResult.Text = result;
            MessageBox.Show("ok");
        }

        private void 河北博物院6位ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string result = "";
            string[] lines = this.GetLines();
            foreach (string line in lines)
            {
                if (line == "")
                    continue;

                string[] items = line.Split(new char[] { '\t' });

                string path = items[0];
                string barcode = items[1];

                // 册条码
                if (StringUtil.Between(barcode, "000001", "999999")==false)
                {
                    continue;
                }


                if (result != "")
                    result += "\r\n";

                result += path + "\t" + barcode;
            }

            this.txtResult.Text = result;
            MessageBox.Show("ok");
        }

        private void 光华学院ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string result = "";
            string[] lines = this.GetLines();
            foreach (string line in lines)
            {
                if (line == "")
                    continue;

                string[] items = line.Split(new char[] { '\t' });

                string path = items[0];
                string barcode = items[1];

                /*
                 中文图书：7位数字
报纸：B+6位数字
中文现刊:X+7位数字
中文合订刊：HS+6位数字 或者 HZ+6位数字
西文图书：W+7位数字
西文现刊：WX+6位数字
西文合订刊：WH+6位数字
                 */
                // 册条码
                if (StringUtil.Between(barcode, "0000001", "9999999")
                    || StringUtil.Between(barcode, "B000001", "B999999")
                    || StringUtil.Between(barcode, "X0000001", "X9999999")
                    || StringUtil.Between(barcode, "HS000001", "HS999999")
                    || StringUtil.Between(barcode, "HZ000001", "HZ999999")
                    || StringUtil.Between(barcode, "W0000001", "W9999999")
                    || StringUtil.Between(barcode, "WX000001", "WX999999")
                    || StringUtil.Between(barcode, "WH000001", "WH999999")  
        )
                {
                    continue;
                }


                if (result != "")
                    result += "\r\n";

                result += path + "\t" + barcode;
            }

            this.txtResult.Text = result;
            MessageBox.Show("ok");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string result = "";
            string[] lines = this.GetLines();
            foreach (string line in lines)
            {
                if (line == "")
                    continue;

                string date = line.Substring(0, 10);


                if (result != "")
                    result += "\r\n";

                result +=date;
            }

            this.txtResult.Text = result;
            MessageBox.Show("ok");
        }
    }


}
