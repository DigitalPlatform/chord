using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DigitalPlatform.Forms
{ 
    public static class ListViewUtil
    {
        public static void BeginSelectItem(Control control, ListViewItem item)
        {
            control.BeginInvoke(new Action<ListViewItem>(
                (o) =>
                {
                    o.Selected = true;
                    o.EnsureVisible();
                }), item);
        }

#if NO
        public static int GetColumnHeaderHeight(ListView list)
        {
            API.RECT rc = new API.RECT();
            IntPtr hwnd = API.SendMessage(list.Handle, API.LVM_GETHEADER, 0, 0);
            if (hwnd == null)
                return -1;

            if (API.GetWindowRect(new HandleRef(null, hwnd), out rc))
            {
                return rc.bottom - rc.top;
            }

            return -1;
        }
#endif
        // 获得列标题宽度字符串
        public static string GetColumnWidthListString(ListView list)
        {
            string strResult = "";
            for (int i = 0; i < list.Columns.Count; i++)
            {
                ColumnHeader header = list.Columns[i];
                if (i != 0)
                    strResult += ",";
                strResult += header.Width.ToString();
            }

            return strResult;
        }

        // 设置列标题的宽度
        // parameters:
        //      bExpandColumnCount  是否要扩展列标题到足够数目？
        public static void SetColumnHeaderWidth(ListView list,
            string strWidthList,
            bool bExpandColumnCount)
        {
            string[] parts = strWidthList.Split(new char[] { ',' });

            if (bExpandColumnCount == true)
                EnsureColumns(list, parts.Length, 100);

            for (int i = 0; i < parts.Length; i++)
            {
                if (i >= list.Columns.Count)
                    break;

                string strValue = parts[i].Trim();
                int nWidth = -1;
                try
                {
                    nWidth = Convert.ToInt32(strValue);
                }
                catch
                {
                    break;
                }

                if (nWidth != -1)
                    list.Columns[i].Width = nWidth;
            }
        }

        // 确保列标题数量足够
        public static void EnsureColumns(ListView listview,
            int nCount,
            int nInitialWidth = 200)
        {
            if (listview.Columns.Count >= nCount)
                return;

            for (int i = listview.Columns.Count; i < nCount; i++)
            {
                string strText = "";
                // strText = Convert.ToString(i);

                ColumnHeader col = new ColumnHeader();
                col.Text = strText;
                col.Width = nInitialWidth;
                listview.Columns.Add(col);
            }
        }

        // 获得一个单元的值
        public static string GetItemText(ListViewItem item,
            int col)
        {
            if (col == 0)
                return item.Text;

            if (col >= item.SubItems.Count)
                return "";

            return item.SubItems[col].Text;
        }

        // 修改一个单元的值
        public static void ChangeItemText(ListViewItem item,
            int col,
            string strText)
        {
            // 确保线程安全
            if (item.ListView != null && item.ListView.InvokeRequired)
            {
                item.ListView.BeginInvoke(new Action<ListViewItem, int, string>(ChangeItemText), item, col, strText);
                return;
            }

            if (col == 0)
            {
                item.Text = strText;
                return;
            }

            // 保险
            while (item.SubItems.Count < col + 1)   // 原来为<=, 会造成多加一列的后果 2006/10/9 changed
            {
                item.SubItems.Add("");
            }

            item.SubItems[col].Text = strText;
        }

        public static void DeleteSelectedItems(ListView list)
        {
            int[] indices = new int[list.SelectedItems.Count];
            list.SelectedIndices.CopyTo(indices, 0);

            list.BeginUpdate();

            for (int i = indices.Length - 1; i >= 0; i--)
            {
                list.Items.RemoveAt(indices[i]);
            }

            list.EndUpdate();
        }
    }
}
