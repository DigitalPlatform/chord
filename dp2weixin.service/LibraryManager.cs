using DigitalPlatform.Text;
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

        public const string C_RequestVersion = "2.85";

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

            // 获取版本号
            library.Version = this.GetVersion(entity);

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

            LibEntity entity = LibDatabase.Current.GetLibById1(libId);
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


        internal void RedoGetVersion()
        {
            if (this.Librarys == null || this.Librarys.Count == 0)
                return;

            foreach (Library lib in this.Librarys)
            {
                if (lib.Version == "-1" || lib.Version=="0.0") //2016/9/30
                {
                    lib.Version = this.GetVersion(lib.Entity);
                }
            }
        }

        public string GetVersion(LibEntity lib)
        {
            string version = "";

            // 获取版本号
            string strError = "";
            List<string> dataList = new List<string>();
            int nRet = dp2WeiXinService.Instance.GetSystemParameter(lib,
                "system",
                "version",
                out dataList,
                out strError);
            if (nRet == -1)
            {
                // 设为-1，表示获取时出错，工作线程会自动重新获取
                version = "-1";

                // 记到日志里
                dp2WeiXinService.Instance.WriteErrorLog1("获取 " + lib.libName + " 版本出错：" + strError);
                return version;
            }
            else if (nRet == 0)
            { 
                // 未命中的情况，当空处理
                version = "";
            }
            else
            {
                version = dataList[0];
            }

            if (version == "")
                version = "0.0";//dp2library 本身如果太旧，这个获得版本号的过程会返回空字符串，把空字符串当作 0.0 版看待即可。


            return version;
        }


        public string GetLibVersiongString()
        {
            string versionStr = "";

            if (this.Librarys == null || this.Librarys.Count == 0)
                return "";

            foreach (Library lib in this.Librarys)
            {
                if (versionStr != "")
                    versionStr += ";";

                int ok = 0;
                if (lib.Version != "-1")
                {
                    int nRet = StringUtil.CompareVersion(lib.Version, C_RequestVersion);
                    if (nRet >= 0)
                        ok = 1;
                    else
                        ok = 0;
                }
                else
                {
                    ok = -1;
                }

                versionStr += lib.Entity.id + ":" + ok.ToString();
            }

            return versionStr;
        }


    }

    public class Library//:LibEntity
    {
        //// 对应mongodb的结构
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

        // dp2library版本号
        public string Version { get; set; }

    }
}
