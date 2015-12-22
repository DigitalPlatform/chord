using dp2weixin;
using dp2Command.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DigitalPlatform.LibraryRestClient;

namespace WebWeiXinToGZH
{
    public partial class WeiXinApi : System.Web.UI.Page
    {

        string url = "http://localhost:14153/index.aspx";
        /// <summary>
        /// 通道所使用的 HTTP Cookies
        /// </summary>
        public CookieContainer Cookies = new System.Net.CookieContainer();

        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                CookieAwareWebClient client = new CookieAwareWebClient(this.Cookies);
                client.Headers["Content-type"] = "application/xml; charset=utf-8";
                string xml = WeiXinClientUtil.GetPostXmlToWeiXinGZH(this.txtMessage.Text);
                byte[] baData = Encoding.UTF8.GetBytes(xml);
                byte[] result = client.UploadData(this.url,
                    "POST",
                    baData);
                string strResult = Encoding.UTF8.GetString(result);
                this.txtResult.Text = strResult;

                // 将焦点设回输入框
                this.lblMessage.Text = "您刚才发的消息是[" + this.txtMessage.Text + "]";
                this.txtMessage.Text = "";
                
                this.txtMessage.Focus();
            }
            catch (Exception ex)
            {
                this.txtResult.Text="Exception :" + ex.Message;
            }
        }


    }


}