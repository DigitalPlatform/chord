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
        public void AddNote(Note note)
        {
            this._dbclient.Add(note);
            this._dbclient.SaveChanges(true);
        }

        // 更新一条备书单
        public void UpdateNote(Note note)
        {
            this._dbclient.Notes.Update(note);
            this._dbclient.SaveChanges(true);
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
        public Note GetNote(int id)
        {
            Note note = this._dbclient.Notes
                .Single(b => b.Id == id);

            return note;
        }



    }
}
