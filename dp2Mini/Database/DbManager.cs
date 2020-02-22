using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Mini
{
    public class DbManager
    {
        #region 单一实例

        static DbManager _instance;
        private DbManager()
        {
            this.ConnectionDb();
        }
        private static object _lock = new object();
        static public DbManager Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (_lock)  //线程安全的
                    {
                        _instance = new DbManager();
                    }
                }
                return _instance;
            }
        }
        #endregion

        //声明事件
        public event AddNoteDelegate AddNoteHandler;


        // 数据库对象
        NoteDB _dbclient = null;

        // 连接数据库
        public void ConnectionDb()
        {
            this._dbclient = new NoteDB();
            //Create the database file at a path defined in SimpleDataStorage
            this._dbclient.Database.EnsureCreated();
            //Create the database tables defined in SimpleDataStorage
            this._dbclient.Database.Migrate();
        }

        /// <summary>
        /// 增加备书单
        /// </summary>
        /// <param name="note"></param>
        public void AddNote(string patronName,List<ReservationItem> items)
        {
            if (items == null || items.Count == 0)
                throw new Exception("items不应为空");


            // 其实这个paths字段也不是必须，因为现在创建了一个本地预约表，
            // 可以根据备书单id从本地预约表中检索，两者有一对多的关系
            string strPaths = "";
            string patronTel = "";
            foreach (ReservationItem item in items)
            {
                if (strPaths != "")
                    strPaths += ",";
                strPaths += item.RecPath;

                if (patronTel == "")
                    patronTel = item.PatronTel;
            }

            // 增加一个备书单
            Note note = new Note(strPaths, patronName, patronTel);
            this._dbclient.Notes.Add(note);
            this._dbclient.SaveChanges(true);


            // 给本地库保存item
            string noteId = DbManager.NumToString(note.Id); //item表中note用的是字符串格式
            foreach (ReservationItem item in items)
            {
                // 在item里用noteId与备书单建立关联
                item.NoteId = noteId;
                this.AddItem(item);
            }

            // 通知接管了该事件的外面调用者
            if (AddNoteHandler != null)
            {
                AddNoteHandler(note);
            }
        }

        public static string NumToString(int num)
        {
            // 将数字转成7位字符串，左边补0
            return num.ToString().PadLeft(7, '0');
        }

        // 更新一条备书单
        public void UpdateNote(Note note)
        {
            this._dbclient.Notes.Update(note);
            this._dbclient.SaveChanges(true);
        }

        public void RemoveNote(string noteId)
        {
            Note note = this.GetNote(noteId);
            this._dbclient.Notes.Remove(note);
            this._dbclient.SaveChanges();


            string itemPaths = "";
            // 将下级item的noteId置空
            List<ReservationItem> items = DbManager.Instance.GetItemsByNoteId(noteId);
            foreach (ReservationItem item in items)
            {
                if (itemPaths != "")
                    itemPaths += ",";
                itemPaths += item.RecPath;

                item.NoteId = "";
                DbManager.Instance.UpdateItem(item);
            }

        }

        // 获取全部
        public List<Note> GetNotes()
        {
            return this._dbclient.Notes.ToList();
        }

        /// <summary>
        /// 根据id找到一条备书库
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Note GetNote(string strId)
        {
            // 先将0去掉
            int id = Convert.ToInt32(strId);

            Note note = this._dbclient.Notes
                .Single(b => b.Id == id);

            return note;
        }

        /// <summary>
        /// 根据路径查找一项预约记录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ReservationItem GetItem(string path)
        {
            //ReservationItem item = this._dbclient.Items
            //    .Single(p => p.RecPath == path);

            List<ReservationItem> items = this._dbclient.Items
                .Where(b => b.RecPath == path)
                .OrderBy(b => b.RequestTime)
                .ToList();

            if (items.Count > 0)
                return items[0];

            return null;
        }

        public List<ReservationItem> GetItemsByNoteId(string noteId)
        {
            List<ReservationItem> items = this._dbclient.Items
                .Where(b => b.NoteId == noteId)
                .OrderBy(b => b.RequestTime)
                .ToList();

            return items;
        }

        /// <summary>
        /// 给本地预约记录表中新增一行
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(ReservationItem item)
        {
            this._dbclient.Items.Add(item);
            this._dbclient.SaveChanges(true);
        }

        public void UpdateItem(string path, string noteId)
        {
            ReservationItem item = this.GetItem(path);
            if (item == null)
                throw new Exception("在本地item表中未找到路径为" + path + "的记录。");

            // 将备书库的单号更新到item到
            item.NoteId = noteId;

            // 保存到库中
            this._dbclient.Items.Update(item);
            this._dbclient.SaveChanges(true);
        }

        public void UpdateItem(ReservationItem item)
        {
            // 保存到库中
            this._dbclient.Items.Update(item);
            this._dbclient.SaveChanges(true);
        }

        // 删除item
        public void RemoveItem(ReservationItem item)
        {
            this._dbclient.Items.Remove(item);
            this._dbclient.SaveChanges();
        }

    }

    // 增加备书单委托
    public delegate void AddNoteDelegate(Note note);
}
