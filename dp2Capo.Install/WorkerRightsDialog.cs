using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using DigitalPlatform.Forms;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;

namespace dp2Capo.Install
{
    /// <summary>
    /// 对话框：为工作人员添加管理公众号的权限
    /// </summary>
    public partial class WorkerRightsDialog : Form
    {
        public LibraryChannel Channel { get; set; }

        // dp2Capo 代理账户名。要避免给它添加管理公众号的权限
        public string ManagerUserName { get; set; }

        public WorkerRightsDialog()
        {
            InitializeComponent();
        }

        private void WorkerRightsDialog_Load(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(ListUsers));
        }

        void ListUsers()
        {
            this.Enabled = false;
            try
            {
                string strError = "";
                int nRet = ListUsers(out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private void button_add_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.Enabled = false;
            try
            {
                int nRet = ChangeRights("add", out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                this.Enabled = true;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_remove_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.Enabled = false;
            try
            {
                int nRet = ChangeRights("remove", out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                this.Enabled = true;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int ListUsers(out string strError)
        {
            strError = "";
            int nStart = 0;
            UserInfo[] users = null;

            this.listView1.Items.Clear();

            for (; ; )
            {
                long lRet = this.Channel.GetUser("list",
                    "",
                    nStart,
                    -1,
                    out users,
                    out strError);
                if (lRet == -1)
                    return -1;

                long lTotalCount = lRet;

                foreach (UserInfo info in users)
                {
                    // 忽略那些保留的用户名，和代理账户名
                    if (Array.IndexOf(reserve_username, info.UserName) != -1
                        || info.UserName == this.ManagerUserName)
                    {
                        nStart++;
                        continue;
                    }

                    ListViewItem item = new ListViewItem();
                    ListViewUtil.ChangeItemText(item, 0, info.UserName);
                    ListViewUtil.ChangeItemText(item, 1, GetWeixinRights(info.Rights));
                    item.Tag = info;

                    this.listView1.Items.Add(item);

                    nStart++;
                }

                if (nStart >= lTotalCount)
                    break;
            }

            return 0;
        }

        static string _weixinRights = "_wx_setbb,_wx_setbook,_wx_setHomePage";

        // parameters:
        //      strAction   add/remove 之一
        int ChangeRights(string strAction, out string strError)
        {
            strError = "";

            foreach (ListViewItem item in this.listView1.CheckedItems)
            {
                UserInfo user = item.Tag as UserInfo;

                string strNewRights = user.Rights;
                StringUtil.SetInList(ref strNewRights, _weixinRights, strAction == "add");
                if (strNewRights == user.Rights)
                    continue;   // 没有变化

                user.Rights = strNewRights;
                long lRet = this.Channel.SetUser("change", user, out strError);
                if (lRet == -1)
                    return -1;

                ListViewUtil.ChangeItemText(item, 1, GetWeixinRights(user.Rights));
            }

            return 0;
        }

        static string[] reserve_username = new string[] {
            "reader",
            "public",
            "opac",
            "图书馆"
        };

        // 从全部权限值中取出 _wx_ 开头的部分权限
        static string GetWeixinRights(string strRights)
        {
            List<string> results = new List<string>();

            string[] parts = strRights.Split(new char[] { ',' });
            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part))
                    continue;
                if (part.StartsWith("_wx_"))
                    results.Add(part);
            }

            return string.Join(",", results.ToArray());
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (this.listView1.CheckedItems.Count > 0)
            {
                this.button_add.Enabled = true;
                this.button_remove.Enabled = true;
            }
            else
            {
                this.button_add.Enabled = false;
                this.button_remove.Enabled = false;
            }

            if (e.Item.Checked == true)
                e.Item.Font = new Font(this.Font, FontStyle.Bold);
            else
                e.Item.Font = new Font(this.Font, FontStyle.Regular);
        }

    }
}
