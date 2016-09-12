using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    public class LibraryManager
    {
        List<Library> Librarys = null;

        // 宏macro
        public const string M_Lib_PatronCount = "%PatronCount%";
        public const string M_Lib_WorkerCount = "%WorkerCount%";
        public const string M_Lib_BindTotalCount = "%BindTotalCount%";

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int Init(out string strError)
        {
            strError = "";

            Librarys = new List<Library>();

            // 取出所有的图书馆，加载到内存中
            List<LibEntity> libs = LibDatabase.Current.GetLibs();

            foreach (LibEntity entity in libs)
            {
                this.AddLib(entity);
            }

            return 0; 
        }

        /// <summary>
        /// 新增一个图书馆
        /// </summary>
        /// <param name="entity"></param>
        public void AddLib(LibEntity entity)
        {
            Library library = new Library();
            library.Entity = entity;

            // 获取绑定的读者数量
            List<WxUserItem> patrons = WxUserDatabase.Current.GetPatron("", entity.id, "");
            library.PatronCount = patrons.Count;

            // 获取绑定的工作人员数量
            List<WxUserItem> workers = WxUserDatabase.Current.Get("", entity.id, WxUserDatabase.C_Type_Worker);
            library.WorkerCount = workers.Count;

            // 加到内存中
            this.Librarys.Add(library);
        }

        /// <summary>
        /// 删除一个图书馆
        /// </summary>
        /// <param name="libId"></param>
        public void DeleteLib(string libId)
        {
            if (this.Librarys != null && this.Librarys.Count > 0)
            {
                foreach (Library lib in this.Librarys)
                {
                    if (lib.Entity.id == libId)
                    {
                        this.Librarys.Remove(lib);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 更新一个图书馆
        /// </summary>
        /// <param name="libId"></param>
        public void UpdateLib(string libId)
        {
            this.DeleteLib(libId);

            LibEntity entity = LibDatabase.Current.GetLibById(libId);
            this.AddLib(entity);
        }


        /// <summary>
        /// 更新绑定数据，被绑定/解绑的地方调用
        /// </summary>
        /// <param name="libId"></param>
        public void UpdateBindCount(string libId)
        {
            Library lib = this.GetLibrary(libId);
            // 获取绑定的读者数量
            List<WxUserItem> patrons = WxUserDatabase.Current.GetPatron("", lib.Entity.id, "");
            lib.PatronCount = patrons.Count;

            // 获取绑定的工作人员数量
            List<WxUserItem> workers = WxUserDatabase.Current.Get("", lib.Entity.id, WxUserDatabase.C_Type_Worker);
            lib.WorkerCount = workers.Count;
        }

        /// <summary>
        /// 根据id查找图书馆对象
        /// </summary>
        /// <param name="libId"></param>
        /// <returns></returns>
        public Library GetLibrary(string libId)
        {
            if (this.Librarys != null && this.Librarys.Count > 0)
            {
                foreach (Library lib in this.Librarys)
                {
                    if (lib.Entity.id == libId)
                        return lib;
                }
            }
            return null;
        }

    }

    public class Library
    {
        // 对应mongodb的结构
        public LibEntity Entity { get; set; }

        // 绑定的读者数量
        public int PatronCount { get; set; }

        // 绑定的工作人员数量
        public int WorkerCount { get; set; }

        // 绑定总数量
        public int BindTotalCount
        {
            get
            {
                return this.PatronCount + this.WorkerCount;
            }
        }


    }
}
