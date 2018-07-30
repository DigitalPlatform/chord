using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2SIPClient
{
    public partial class Form_Checksum : Form
    {
        public Form_Checksum()
        {
            InitializeComponent();
        }

        private void btnCheckSum1_Click(object sender, EventArgs e)
        {
            string text = this.txtMsg.Text;
            string checkSum = this.GetChecksum1(text);
            this.PrinteInfo(checkSum);
        }

        /// <summary>
        /// To calculate the checksum add each character as an unsigned binary number,
        /// take the lower 16 bits of the total and perform a 2's complement. 
        /// The checksum field is the result represented by four hex digits.
        /// </summary>
        /// <param name="message">
        /// 内容中不包含 校验和(checksum)
        /// </param>
        /// <returns></returns>
        private string GetChecksum1(string message)
        {
            string checksum = "";

            try
            {
                ushort sum = 0;
                foreach (char c in message)
                {
                    sum += c;
                }

                ushort checksum_inverted_plus1 = (ushort)(~sum + 1);

                checksum = checksum_inverted_plus1.ToString("X4");
            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                checksum = null;
            }
            return checksum;
        }

        private void btnCheckSum2_Click(object sender, EventArgs e)
        {
            string text = this.txtMsg.Text;
            string checkSum = SIP2.CheckSum.ApplyChecksum(text);
            this.PrinteInfo(checkSum);
        }


        private void PrinteInfo(string text)
        {
            if (this.txtInfo.Text != "")
                this.txtInfo.Text += "\r\n";

            this.txtInfo.Text += text;
            //this.listBox_printer.Items.Add(text);
        }
    }
}
