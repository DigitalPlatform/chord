using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Marc;
using DigitalPlatform.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using TestZ3950.Properties;

namespace TestZ3950
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        void LoadSettings()
        {
            this.textBox_serverAddr.Text = Settings.Default.serverAddr;
            this.textBox_serverPort.Text = Settings.Default.serverPort;
            this.textBox_database.Text = Settings.Default.databaseNames;
#if NO
            this.textBox_queryWord.Text = Settings.Default.queryWord;
            this.comboBox_use.Text = Settings.Default.queryUse;

            string strStyle = Settings.Default.authenStyle;
            if (strStyle == "idpass")
                this.radioButton_authenStyleIdpass.Checked = true;
            else
                this.radioButton_authenStyleOpen.Checked = true;

            this.textBox_groupID.Text = Settings.Default.groupID;
#endif

            this.textBox_userName.Text = Settings.Default.userName;
            this.textBox_password.Text = Settings.Default.password;

#if NO
            this.textBox_queryString.Text = Settings.Default.queryString;

            string strQueryStyle = Settings.Default.queryStyle;
            if (strQueryStyle == "easy")
                this.radioButton_query_easy.Checked = true;
            else
                this.radioButton_query_origin.Checked = true;
#endif
        }

        void SaveSettings()
        {
            Settings.Default.serverAddr = this.textBox_serverAddr.Text;
            Settings.Default.serverPort = this.textBox_serverPort.Text;
            Settings.Default.databaseNames = this.textBox_database.Text;
#if NO
            Settings.Default.queryWord = this.textBox_queryWord.Text;
            Settings.Default.queryUse = this.comboBox_use.Text;

            if (this.radioButton_authenStyleIdpass.Checked == true)
                Settings.Default.authenStyle = "idpass";
            else
                Settings.Default.authenStyle = "open";

            Settings.Default.groupID = this.textBox_groupID.Text;
#endif
            Settings.Default.userName = this.textBox_userName.Text;
            Settings.Default.password = this.textBox_password.Text;

#if NO
            Settings.Default.queryString = this.textBox_queryString.Text;

            if (this.radioButton_query_easy.Checked == true)
                Settings.Default.queryStyle = "easy";
            else
                Settings.Default.queryStyle = "origin";
#endif
            Settings.Default.Save();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveSettings();

            _channelPool.BeforeLogin += _channelPool_BeforeLogin;
        }

        private void _channelPool_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void button_begin_test1_Click(object sender, EventArgs e)
        {

        }

        LibraryChannelPool _channelPool = new LibraryChannelPool();

        // 从 dp2library 服务器获得全部 isbn 和 title 字符串
        IsbnResult GetIsbnStrings()
        {
            LibraryChannel channel = _channelPool.GetChannel(this.textBox_dp2libraryUrl.Text, "public");
            try
            {
                // TODO: <全部UNIMARC>
                long lRet = channel.SearchBiblio("<全部>", "", -1, "isbn",
                    "left", "zh", "default", "", "", "",
                    out string strQueryXml, out string strError);
                if (lRet == -1)
                    return new IsbnResult { Value = -1, ErrorInfo = strError };
                ResultSetLoader loader = new ResultSetLoader(channel, "default",
                    "id,xml");
                IsbnResult result = new IsbnResult();
                result.IsbnList = new List<string>();
                foreach (Record record in loader)
                {
                    // XML 转换为 MARC
                    int nRet = MarcUtil.Xml2Marc(record.RecordBody.Xml,
                        false,
                        "",
                        out string strMarcSyntax,
                        out string strMARC,
                        out strError);
                    if (nRet != 0)
                        continue;

                    // 从 MARC 中取 010$a
                    var marc_record = new MarcRecord(strMARC);
                    string xpath = "field[@name='010']/subfield[@name='a']";
                    if (strMarcSyntax == "usmarc")
                        xpath = "field[@name='020']/subfield[@name='a']";
                    var nodes = marc_record.select(xpath);

                    foreach (MarcSubfield subfield in nodes)
                    {
                        result.IsbnList.Add(subfield.Content);
                    }
                }

                return result;
            }
            finally
            {
                _channelPool.ReturnChannel(channel);
            }
        }
    }

    public class IsbnResult : DigitalPlatform.Net.Result
    {
        public List<string> IsbnList { get; set; }
    }
}
