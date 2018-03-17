using DigitalPlatform.Message;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace dp2weixin.service
{
    public class LibraryManager
    {
        public List<Library> Librarys = null;

        // 宏macro
        public const string M_Lib_PatronCount = "%PatronCount%";
        public const string M_Lib_WorkerCount = "%WorkerCount%";
        public const string M_Lib_BindTotalCount = "%BindTotalCount%";

        public const string C_RequestCapoVersion = "1.26";
        public const string C_State_Hangup = "hang-up";

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
            List<LibEntity> libs = LibDatabase.Current.GetLibsInternal();

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
            // 到期的图书馆不加入到内存集合中
            if (entity!=null && entity.state == "到期")
                return;

            Library library = new Library();
            library.Entity = entity;

            //// 获取绑定的读者数量
            //List<WxUserItem> patrons = WxUserDatabase.Current.GetPatron("", entity.id, "");
            //library.PatronCount = patrons.Count;

            //// 获取绑定的工作人员数量
            //List<WxUserItem> workers = WxUserDatabase.Current.Get("", entity.id, WxUserDatabase.C_Type_Worker);
            //library.WorkerCount = workers.Count;

            // 获取版本号
            string version = this.GetVersion(entity);
            library.SetVersion(entity, version);


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
            LibEntity entity = LibDatabase.Current.GetLibById1(libId);


            this.DeleteLib(libId);

            //LibEntity entity = LibDatabase.Current.GetLibById1(libId);
            this.AddLib(entity);
        }


        /// <summary>
        /// 更新绑定数据，被绑定/解绑的地方调用
        /// </summary>
        /// <param name="libId"></param>
        //public void UpdateBindCount(string libId)
        //{
        //    Library lib = this.GetLibrary(libId);
        //    // 获取绑定的读者数量
        //    List<WxUserItem> patrons = WxUserDatabase.Current.GetPatron("", lib.Entity.id, "");
        //    lib.PatronCount = patrons.Count;

        //    // 获取绑定的工作人员数量
        //    List<WxUserItem> workers = WxUserDatabase.Current.Get("", lib.Entity.id, WxUserDatabase.C_Type_Worker);
        //    lib.WorkerCount = workers.Count;
        //}

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

        //根据ids得到对应的图书馆数组
        public List<Library> GetLibraryByIds(List<string> ids)
        {
            List<Library> libs = new List<Library>();
            if (this.Librarys != null && this.Librarys.Count > 0)
            {
                foreach (Library lib in this.Librarys)
                {
                    if (ids.IndexOf(lib.Entity.id) != -1)
                        libs.Add(lib);
                }
            }
            return libs;
        }

        //根据capo_***找到图书馆配置信息
        public Library GetLibraryByCapoName(string capoName)
        {
            if (this.Librarys != null && this.Librarys.Count > 0)
            {
                foreach (Library lib in this.Librarys)
                {
                    if (lib.Entity.capoUserName == capoName)
                        return lib;
                }
            }
            return null;
        }

        // 得到图书馆挂起警告
        public static string GetLibHungWarn(Library lib)
        {
            string warnText = "";
            // 如果图书馆是挂起状态，作为警告
            if (lib.State == LibraryManager.C_State_Hangup)
            {
                // 立即重新检查一下
                dp2WeiXinService.Instance.LibManager.RedoGetVersion(lib);
                if (lib.Version == "-1")
                {
                    //的桥接服务器dp2capo已失去连接，请尽快修复。
                    warnText = lib.Entity.libName + " 的桥接服务器dp2capo失去连接，公众号功能已被挂起，请尽快修复。";
                }
                else
                {
                    //warnText = lib.Entity.libName + " 的桥接服务器dp2capo版本不够新，公众号功能已被挂起，请尽快升级。";

                    warnText = lib.Entity.libName + " 的桥接服务器dp2capo版本不够新（当前版本是" + lib.Version + "，要求版本为" + LibraryManager.C_RequestCapoVersion + "或以上），公众号功能已被挂起，请尽快升级。";

                }
            }

            return warnText;
        }

        public void RedoGetVersion(Library library)
        {
            if (this.Librarys == null || this.Librarys.Count == 0)
                return;

            // 如果传入lib，则只处理当前一个图书馆
            if (library != null)
            {
                this.RedoGetVersionOneLib(library);
                return;
            }

            //如果未传入图书馆，则检查所有图书馆
            foreach (Library lib in this.Librarys)
            {
                this.RedoGetVersionOneLib(lib);
            }
        }

        public void RedoGetVersionOneLib(Library library)
        {
            if (library != null)
            {
                if (library.State == LibraryManager.C_State_Hangup) // 20161017 统一改为用挂起状态判断 //.Version == "-1" || lib.Version=="0.0") //2016/9/30
                {
                    string version = this.GetVersion(library.Entity);
                    library.SetVersion(library.Entity,version);
                }
            }
        }

        private string GetVersion(LibEntity lib)
        {
            string version = "";

            // 获取版本号
            string strError = "";
            List<string> dataList = new List<string>();
            //int nRet = dp2WeiXinService.Instance.GetSystemParameter(lib,
            //    "system",
            //    "version",
            //    out dataList,
            //    out strError);

            //(较早的dp2Capo在上述功能被调用时会返回ErrorInfo=未知的 category '_clock' 和 name '',ErrorCode=NotFound)
            int nRet = dp2WeiXinService.Instance.GetSystemParameter(lib,
                "_capoVersion",
                "",
                out dataList,
                out strError);

            Thread.Sleep(500);//2016/11/9 延尽半秒

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
                if (lib.State == "")//正常状态
                    ok = 1;

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
        //public int PatronCount { get; set; }

        // 绑定的工作人员数量
        //public int WorkerCount { get; set; }

        public List<DbCfg> DbList = new List<DbCfg>();

        // 是否校验二维码
        public bool VerifyBarcode = true;

        public DbCfg GetDb(string dbName)
        {
            foreach (DbCfg db in this.DbList)
            {
                if(db.BiblioDbName == dbName)
                    return db;
            }
            return null;
        }

        //// 绑定总数量
        //public int BindTotalCount
        //{
        //    get
        //    {
        //        return this.PatronCount + this.WorkerCount;
        //    }
        //}

        // dp2library版本号
        private string _version = "";
        public string Version
        {
            get
            {
                return this._version;
            }
        }

        public void SetVersion(LibEntity libEntity, string version)
        {
            string strError = "";

            // 设置版本
            this._version = version;

            // 更新状态
            this.UpdateState();


            // 获取数据库信息
            if (this.State != LibraryManager.C_State_Hangup
                || (this.State == LibraryManager.C_State_Hangup && this.Version != "-1"))
            {
                // 得到期库
                //用点对点的 getSystemParameter 功能。category 为 "system", name 为 "biblioDbGroup"，
                //可以获得一段 XML 字符串，格式和 library.xml 中的 itemdbgroup 元素相仿，
                //但每个 database 元素的 name 属性名字变为 itemDbName。
                // item.IssueDbName = DomUtil.GetAttr(node, "issueDbName");
                List<string> dataList = new List<string>();
                //(较早的dp2Capo在上述功能被调用时会返回ErrorInfo=未知的 category '_clock' 和 name '',ErrorCode=NotFound)
                int nRet = dp2WeiXinService.Instance.GetSystemParameter(libEntity,
                    "system",
                    "biblioDbGroup",
                    out dataList,
                    out strError);
                if (nRet == -1 || nRet == 0)
                {
                    goto ERROR1;
                }

                // 取出数据库配置xml
                this.BiblioDbGroup = "<root>"+dataList[0]+"</root>";

                XmlDocument dom = new XmlDocument();
                dom.LoadXml(this.BiblioDbGroup);
                XmlNodeList databaseList = dom.DocumentElement.SelectNodes("database");
                foreach (XmlNode node in databaseList)
                {
                    DbCfg db = new DbCfg();

                    db.DbName = DomUtil.GetAttr(node, "itemDbName");
                    db.BiblioDbName = DomUtil.GetAttr(node, "biblioDbName");
                    db.BiblioDbSyntax = DomUtil.GetAttr(node, "syntax");
                    db.IssueDbName = DomUtil.GetAttr(node, "issueDbName");

                    db.OrderDbName = DomUtil.GetAttr(node, "orderDbName");
                    db.CommentDbName = DomUtil.GetAttr(node, "commentDbName");
                    db.UnionCatalogStyle = DomUtil.GetAttr(node, "unionCatalogStyle");

                    // 2008/6/4
                    bool bValue = true;
                    nRet = DomUtil.GetBooleanParam(node,
                        "inCirculation",
                        true,
                        out bValue,
                        out strError);
                    if (nRet == -1)
                    {
                        throw new Exception("元素<//biblioDbGroup/database>属性inCirculation读入时发生错误: " + strError);
                    }
                    db.InCirculation = bValue;

                    db.Role = DomUtil.GetAttr(node, "role");

                    this.DbList.Add(db);
                }
            }

            return;

            ERROR1:
            dp2WeiXinService.Instance.WriteErrorLog1("获取库信息出错:" + strError);

        
        }

        private void UpdateState()
        {
                if (Version != "-1")
                {
                    int nRet = StringUtil.CompareVersion(Version, LibraryManager.C_RequestCapoVersion);
                    if (nRet > 0)
                        this.State = "";
                    else
                        this.State = LibraryManager.C_State_Hangup;
                }
                else
                {
                    this.State = LibraryManager.C_State_Hangup; 
                }            
        }

        // 图书馆，目前主要有用值为 hang-up
        public string State { get; private set; }


        public string BiblioDbGroup { get; set; }

    }

    public class DbCfg
    {
        public string DbName = "";  // 实体库名
        public string BiblioDbName = "";    // 书目库名
        public string BiblioDbSyntax = "";  // 书目库MARC语法

        public string IssueDbName = ""; // 期库
        public string OrderDbName = ""; // 订购库 
        public string CommentDbName = "";   // 评注库

        public string UnionCatalogStyle = "";   // 联合编目特性 905 

        public bool InCirculation = true;   

        public string Role = "";    // 角色 biblioSource/orderWork 
    }
}
