using DigitalPlatform.Message;
using DigitalPlatform.MessageClient;
using dp2Message;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace TestMessage
{
    public partial class Form1 : Form
    {
        MessageConnectionCollection _channels = new MessageConnectionCollection();

        BaseMsgHandler msgHandler = null;
        public Form1()
        {
            InitializeComponent();
            _channels.Login += _channels_Login;

            string logDir="c:/msg_data";
            if (Directory.Exists(logDir) == false)
            {
                Directory.CreateDirectory(logDir);
            }

            msgHandler = new BaseMsgHandler();            
            msgHandler.Init(this._channels,
                this.textBox_config_messageServerUrl.Text.Trim(), //"http://localhost:8083/dp2mserver",                
                logDir);
        }

        void _channels_Login(object sender, LoginEventArgs e)
        {
            MessageConnection connection = sender as MessageConnection;

            e.UserName = GetUserName();
            if (string.IsNullOrEmpty(e.UserName) == true)
                throw new Exception("尚未指定用户名，无法进行登录");

            e.Password = GetPassword();
        }

        string GetUserName()
        {
            return this.textBox_config_userName.Text;
        }

        string GetPassword()
        {
            return this.textBox_config_password.Text;
        }


    }
}
