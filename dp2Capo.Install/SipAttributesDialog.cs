using DigitalPlatform.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace dp2Capo.Install
{
    public partial class SipAttributesDialog : Form
    {
        /// <summary>
        /// 容器元素 
        /// sipServer 元素
        /// </summary>
        public XmlElement ContainerElement { get; set; }

        public SipAttributesDialog()
        {
            InitializeComponent();
        }

        private void SipAttributesDialog_Load(object sender, EventArgs e)
        {
            Fill();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            Restore();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        void Fill()
        {
            this.listBox1.Items.Clear();

            if (this.ContainerElement == null)
                return;

            XmlNodeList nodes = this.ContainerElement.SelectNodes("user");
            foreach(XmlElement user in nodes)
            {
                UserItem item = new UserItem();
                item.UserName = user.GetAttribute("userName");
                this.listBox1.Items.Add(item);
            }
        }

        // 把内存对象兑现到 XML Document
        void Restore()
        {
            if (this.ContainerElement == null)
                return;

            // TODO: 检查数据合法性

            XmlNodeList nodes = this.ContainerElement.SelectNodes("user");
            // 删除以前的 uer 元素
            foreach(XmlElement user in nodes)
            {
                user.ParentNode.RemoveChild(user);
            }

            // 重新创建 user 元素
            foreach(UserItem item in this.listBox1.Items)
            {
                XmlElement user = this.ContainerElement.OwnerDocument.CreateElement("user");
                this.ContainerElement.AppendChild(user);

                user.SetAttribute("userName", item.UserName);
            }
        }

        class UserItem
        {
            public string UserName { get; set; }

            public string EncodingName { get; set; }

            public override string ToString()
            {
                return UserName;
            }
        }

        private void toolStripButton_newUser_Click(object sender, EventArgs e)
        {
            string strUserName = InputDlg.GetInput(this, 
                "创建新用户事项",
                "用户名",
                "", 
                this.Font);
            // TODO: 对用户名在列表中查重。不允许空用户名
            UserItem item = new UserItem();
            item.UserName = strUserName;
            this.listBox1.Items.Add(item);
        }

        private void toolStripButton_deleteUser_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedItem != null)
                this.listBox1.Items.Remove(this.listBox1.SelectedItem);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedItem != null)
            {
                this.toolStripButton_deleteUser.Enabled = true;

                this.propertyGrid1.SelectedObject = this.listBox1.SelectedItem;
            }
            else
            {
                this.toolStripButton_deleteUser.Enabled = false;

                this.propertyGrid1.SelectedObject = null;
            }
        }
    }
}
