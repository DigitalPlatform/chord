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
        public const string M_Lib_WxPatronCount = "%PatronCount%";
        public const string M_Lib_WxWorkerCount = "%WorkerCount%";
        public const string M_Lib_WxTotalCount = "%WxTotalCount%";

        public const string M_Lib_WebPatronCount = "%WebPatronCount%";
        public const string M_Lib_WebWorkerCount = "%WebWorkerCount%";
        public const string M_Lib_WebTotalCount = "%WebTotalCount%";

        public const string M_Lib_BindTotalCount = "%BindTotalCount%";

        public const string C_RequestCapoVersion = "1.32";
        public const string C_State_Hangup1 = "hang-up";

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

        public const string C_HangupReason_VersionApiError = "VersionApiError";
        public const string C_HangupReason_Low = "Low";
        public const string C_HangupReason_libraryStop = "libraryStop";

        public void CheckIsHangup(Library library)
        {
            int nRet = 0;
            string strError = "";

            library.IsHangup = false; //默认值设为不挂起，因为有一些程序通讯的错误，不应该通知管理员


            // 获取版本号
            string capoVersion = "";
            nRet = this.GetCapoVersion(library,
                out capoVersion,
                out strError);
            if (nRet == -1)
            {
                library.IsHangup = true;
                library.HangupReason = C_HangupReason_VersionApiError;
                dp2WeiXinService.Instance.WriteErrorLog("获取"+library.Entity.libName+"版本出错："+strError);
                return;
            }

            // 设置版本
            library.Version = capoVersion;
            // 比较capo版本是否满足需求
            nRet = StringUtil.CompareVersion(library.Version,
               LibraryManager.C_RequestCapoVersion);
            if (nRet < 0)
            {
                library.IsHangup = true;
                library.HangupReason = C_HangupReason_Low;
                return;
            }

            // 检查dp2library是否在线
            bool isOnline = dp2WeiXinService.Instance.CheckIsOnline(library.Entity,
                 out string clock,
                 out strError);
            if (isOnline == false)
            {
                library.IsHangup = true;
                library.HangupReason = C_HangupReason_libraryStop;
                return;
            }

            library.IsHangup = false;//此时才设为不挂起
        }


        /// <summary>
        /// 新增一个图书馆
        /// </summary>
        /// <param name="entity"></param>
        public void AddLib(LibEntity entity)
        {
            int nRet = 0;
            string strError = "";

            // 到期的图书馆不加入到内存集合中
            if (entity != null
                && entity.state == dp2WeiXinService.C_State_Expire)
                return;

            Library library = new Library();
            library.Entity = entity;

            // 加到内存中
            this.Librarys.Add(library);

            // 检查版本号和是否连接成功
            this.CheckIsHangup(library);

            // 获取数据库
            if (library.IsHangup==false)
                this.GetDbs(library);
            


        }


        public void GetDbs(Library library)
        {
            int nRet = 0;
            string strError = "";
            // 获取数据库
            //用点对点的 getSystemParameter 功能。category 为 "system", name 为 "biblioDbGroup"，
            //可以获得一段 XML 字符串，格式和 library.xml 中的 itemdbgroup 元素相仿，
            //但每个 database 元素的 name 属性名字变为 itemDbName。
            // item.IssueDbName = DomUtil.GetAttr(node, "issueDbName");
            List<string> dataList = new List<string>();
            //(较早的dp2Capo在上述功能被调用时会返回ErrorInfo=未知的 category '_clock' 和 name '',ErrorCode=NotFound)
            nRet = dp2WeiXinService.Instance.GetSystemParameter(library.Entity,
                "system",
                "biblioDbGroup",
                out dataList,
                out strError);
            // -1 记录错误日志，不影响其它馆使用
            if (nRet == -1 || nRet == 0)  //0是什么情况？
            {
                dp2WeiXinService.Instance.WriteErrorLog("获取[" + library.Entity.libName + "]的数据库信息出错:" + strError);
                return;
            }

            // 取出数据库配置xml
            library.BiblioDbGroup = "<root>" + dataList[0] + "</root>";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(library.BiblioDbGroup);
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

                library.DbList.Add(db);
            }
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
        public static string GetLibHungWarn(Library lib,bool bCheckHangup)
        {
            string warnText = "";
            // 如果图书馆是挂起状态，需要发出警告
            if (lib.IsHangup == true)
            {
                if (bCheckHangup == true)
                {
                    // 立即重新检查一下 todo，要不要这样做，如果不立即检查，要等10分钟后才能生效
                    dp2WeiXinService.Instance.LibManager.RedoCheckHangup(lib);
                }

                if (lib.HangupReason == C_HangupReason_VersionApiError)
                {
                    warnText = lib.Entity.libName + " 的桥接服务器dp2capo获取版本号访问不通，公众号功能已被挂起，请尽快修复。";
                }
                else if (lib.HangupReason == C_HangupReason_Low)
                {
                    warnText = lib.Entity.libName + " 的桥接服务器dp2capo版本不够新（当前版本是" + lib.Version + "，要求版本为" + LibraryManager.C_RequestCapoVersion + "或以上），公众号功能已被挂起，请尽快升级。";
                }
                else if (lib.HangupReason == C_HangupReason_libraryStop)
                {
                    warnText = lib.Entity.libName + " 的桥接服务器dp2capo连接dp2library不成功，公众号功能已被挂起，请尽快修复。";
                }
                else
                {
                    warnText = lib.Entity.libName + " 的桥接服务器dp2capo连接不成功(不明原因)，公众号功能已被挂起，请尽快修复。";
                }
            }

            return warnText;
        }


        // 重新获取图书馆版本号
        // 如果参数中传入的图书馆，那么只获取这个图书馆的版本
        // 如果未传，则获取所有图书馆挂起状态的图书馆版本
        public void RedoCheckHangup(Library library1)
        {
            if (this.Librarys == null || this.Librarys.Count == 0)
                return;

            List<Library> libList = new List<Library>();
            // 如果传入lib，则只处理当前一个图书馆
            if (library1 != null)
            {
                libList.Add(library1);
            }
            else
            {
                //如果未传入图书馆，则检查所有图书馆
                foreach (Library lib in this.Librarys)
                {
                    libList.Add(lib);
                }
            }

            // 为这些图书馆检查获取版本号
            foreach (Library lib in libList)
            {
                this.CheckIsHangup(lib);

                // 获取数据库
                if (lib.IsHangup == false)
                    this.GetDbs(lib);
            }
        }



        /// <summary>
        /// 获取一个图书馆的capo版本号
        /// </summary>
        /// <param name="lib">图书馆对象</param>
        /// <param name="capoVersion">返回capo版本号</param>
        /// <param name="strError">出错信息</param>
        /// <returns>
        /// -1 出错
        /// 0 成功
        /// </returns>
        private int GetCapoVersion(Library lib,
            out string capoVersion,
            out string strError)
        {
            capoVersion = "";

            // 获取版本号
            List<string> dataList = new List<string>();

            //(较早的dp2Capo在上述功能被调用时会返回ErrorInfo=未知的 category '_clock' 和 name '',ErrorCode=NotFound)
            int nRet = dp2WeiXinService.Instance.GetSystemParameter(lib.Entity,
                "_capoVersion",
                "",
                out dataList,
                out strError);

            Thread.Sleep(500);//2016/11/9 延尽半秒

            if (nRet == -1)
            {
                // 记到日志里,由使用者记日志
                //dp2WeiXinService.Instance.WriteErrorLog("获取 " + lib.Entity.libName + " 版本出错：" + strError + "。此时将版本设为-1");
                return -1;
            }
            
            if (nRet == 0)
            {
                // 未命中的情况，当空处理
                capoVersion = "";
            }
            else
            {
                capoVersion = dataList[0];
            }

            if (capoVersion == "")
                capoVersion = "0.0";//dp2library 本身如果太旧，这个获得版本号的过程会返回空字符串，把空字符串当作 0.0 版看待即可。


            return 0;
        }

        /*
        /// <summary>
        /// 根据版本号，设置状态是否挂起，并且获取数据库定义
        /// </summary>
        /// <param name="library">图书馆对象</param>
        /// <param name="version">版本</param>
        public void SetStateByVersion(Library library, string version)
        {
            string strError = "";

            // 设置版本
            library.Version = version;

            // 更新状态
            if (library.Version != "-1")
            {
                // 比较capo版本是否满足需求
                int nRet = StringUtil.CompareVersion(library.Version,
                    LibraryManager.C_RequestCapoVersion);
                if (nRet > 0)
                {
                    library.IsHangup = false;
                }
                else
                    library.State = LibraryManager.C_State_Hangup1;
            }
            else
            {
                library.State = LibraryManager.C_State_Hangup1;
            }
        }
        */
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
                if (lib.IsHangup == false)//正常状态
                    ok = 1;

                versionStr += lib.Entity.id + ":" + ok.ToString();
            }

            return versionStr;
        }
    }

    public class Library
    {
        //// 对应mongodb的结构
        public LibEntity Entity { get; set; }

        public List<DbCfg> DbList = new List<DbCfg>();

        public DbCfg GetDb(string dbName)
        {
            foreach (DbCfg db in this.DbList)
            {
                if(db.BiblioDbName == dbName)
                    return db;
            }
            return null;
        }

        // 版本号
        public string Version { get; set; }

        // 是否挂起
        public bool IsHangup { get;  set; }

        // 挂起原因
        public string HangupReason { get; set; }


        public string BiblioDbGroup { get; set; }


        // 是否校验二维码
        public bool VerifyBarcode = true;
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
