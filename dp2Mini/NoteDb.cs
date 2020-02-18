using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace dp2Mini
{
    public class NoteDb
    {
        string _fileName = "";
        List<BookNote> _list = new List<BookNote>();


        public void InitDb()
        {
            this._list = new List<BookNote>();

            this._fileName = Application.StartupPath + "\\records.xml";
            if (File.Exists(this._fileName) == true)
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(this._fileName);
                XmlNodeList nodeList = dom.DocumentElement.SelectNodes("item");
                foreach (XmlNode node in nodeList)
                {
                    string id = DomUtil.GetAttr(node, "id");
                    string records = DomUtil.GetAttr(node, "records");
                    string noticeState = DomUtil.GetAttr(node, "noticeState");
                    string dateTime = DomUtil.GetAttr(node, "dateTime");
                    BookNote order = new BookNote
                    {
                        id = id,
                        records = records,
                        noticeState = noticeState,
                        dateTime = dateTime,
                    };

                    this._list.Add(order);
                }
            }

            //foreach (BookNote note in this._list)
            //{
            //    this.AddNoteToListView(note);
            //}
        }

        // 保存备书单
        public void SaveNotes()
        {
            if (this._list.Count == 0)
                return;


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<root>");

            foreach (BookNote order in this._list)
            {
                sb.AppendLine("<item id='" + order.id + "' records='" + order.records + "' noticeState='" + order.noticeState + "' dateTime='" + order.dateTime + "'/>");
            }
            sb.AppendLine("</root>");

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(sb.ToString());
            dom.Save(this._fileName);
        }

        /// <summary>
        /// 给备书单增加一笔记录
        /// </summary>
        /// <param name="note"></param>
        public void AddNoteToListView(BookNote note, ListView listView)
        {
            ListViewItem item = new ListViewItem(note.id, 0);
            listView.Items.Add(item);

            item.SubItems.Add(note.dateTime);
            item.SubItems.Add(note.records);
            item.SubItems.Add(note.noticeState);
            item.SubItems.Add(note.takeBoolState);
        }


        public BookNote getNote(string id)
        {
            foreach (BookNote order in this._list)
            {
                if (order.id == id)
                    return order;
            }
            return null;
        }

        public BookNote getNoteByRecPath(string recPath)
        {
            foreach (BookNote order in this._list)
            {
                if (order.records.IndexOf(recPath) != -1)
                    return order;
            }
            return null;
        }
    }
}
