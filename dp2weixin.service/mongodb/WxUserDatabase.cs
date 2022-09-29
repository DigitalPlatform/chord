using DigitalPlatform.IO;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace dp2weixin.service
{
    /// <summary>
    /// 用户绑定数据库
    /// </summary>
    public sealed class WxUserDatabase
    {
        //常量
        public const int C_Type_Patron = 0;
        public const int C_Type_Worker = 1;

        // 状态
        public const int C_State_Available = 1;
        public const int C_State_Temp = 2;
        public const int C_State_disabled = 0;

        // refid前缀，在从mongo库检索时，如果patronBarcode传了@refid前缀，则表示根据refid检索
        // 比如dp2读者信息发生变化，传过来的消息，从本地mongodb库检索时就只能用refid检索，因为读者证条码号可能变化了。
        public const string C_Prefix_RefId = "@refid:";
        public const string C_Prefix_fromWeb = "~~";  // 浏览器来源的前缀
        public const string C_Prefix_fromMina = "**";  //小程序来源的前缀
        public const int C_from_weixin = 1; // 微信公众号
        public const int C_from_web = 2;    // 浏览器入口
        public const int C_from_applet = 3;   // 小程序入口

        // 读者状态
        public const string C_PatronState_TodoReview = "待审核";
        public const string C_PatronState_Pass = "";
        public const string C_PatronState_NoPass = "审核不通过";

        // public
        public const string C_Public = "public";


        /// <summary>
        /// 单一静态实例,饿汉模式
        /// </summary>
        private static readonly WxUserDatabase _db = new WxUserDatabase();
        public static WxUserDatabase Current
        {
            get
            {
                return _db;
            }
        }

        // 成员变量
        MongoClient _mongoClient = null;
        IMongoDatabase _database = null;
        string _wxUserDbName = "";
        IMongoCollection<WxUserItem> _wxUserCollection = null;
        public IMongoCollection<WxUserItem> wxUserCollection
        {
            get
            {
                return this._wxUserCollection;
            }
        }

        // 初始化
        public void Open(string strMongoDbConnStr,
            string strInstancePrefix)
        {
            if (string.IsNullOrEmpty(strMongoDbConnStr) == true)
                throw new ArgumentException("strMongoDbConnStr 参数值不应为空");

            if (string.IsNullOrEmpty(strInstancePrefix) == false)
                strInstancePrefix = strInstancePrefix + "_";
            _wxUserDbName = strInstancePrefix + "user";

            this._mongoClient = new MongoClient(strMongoDbConnStr);
            this._database = this._mongoClient.GetDatabase(this._wxUserDbName);

            //collection名称为item
            _wxUserCollection = this._database.GetCollection<WxUserItem>("item");

            CancellationTokenSource cancel = new CancellationTokenSource();
            bool bExist = false;

            // 已存在索引，不用再创建索引
            var indexes = _wxUserCollection.Indexes.ListAsync(cancel.Token).Result.ToListAsync().Result;
            foreach (BsonDocument doc in indexes)
            {
                string name = doc["name"].AsString;
                if (name.Contains("weixinId") == true)
                {
                    bExist = true;
                    continue;
                }
            }

            // 创建索引
            if (bExist == false)
            {
                this.CreateIndex();
            }
        }

        public void CreateIndex()
        {
            // 为weixinid字段建索引
            _wxUserCollection.Indexes.CreateOne(
                Builders<WxUserItem>.IndexKeys.Ascending("weixinId"),
                new CreateIndexOptions() { Unique = false }
                );
        }

        public List<WxUserItem> GetAll()
        {
            List<WxUserItem> list = this.wxUserCollection.Find(new BsonDocument()).ToListAsync().Result;
            return list;
        }

        // 获取绑定账户
        public List<WxUserItem> Get(string weixinId,
            string libId,
            string libraryCode,
            int type,
            string patronBarcode,
            string userName,
            bool bOnlyAvailable
            )
        {
            var filter = Builders<WxUserItem>.Filter.Empty;

            StringBuilder info = new StringBuilder();

            if (bOnlyAvailable == true) // 只取有效的
            {
                filter = filter & Builders<WxUserItem>.Filter.Eq("state", C_State_Available);
            }

            if (string.IsNullOrEmpty(weixinId) == false)
            {
                filter = filter & Builders<WxUserItem>.Filter.Eq("weixinId", weixinId);

                //info.Append(" weixinId=" + weixinId);
            }


            if (string.IsNullOrEmpty(libId) == false)
            {
                filter = filter & Builders<WxUserItem>.Filter.Eq("libId", libId);

                //info.Append(" libId=" + libId);
            }

            if (string.IsNullOrEmpty(libraryCode) == false)
            {
                if (libraryCode == "空")
                    libraryCode = "";

                filter = filter & Builders<WxUserItem>.Filter.Eq("bindLibraryCode", libraryCode);

                //info.Append(" libId=" + libId);
            }

            if (type != -1)
            {
                filter = filter & Builders<WxUserItem>.Filter.Eq("type", type);

                //info.Append(" type=" + type);
            }

            if (string.IsNullOrEmpty(patronBarcode) == false)
            {
                //2020/3/7 当读者信息更新，外面传的@refid:xxx，这里要根据refID来检索
                // 例如读者先注册，分配的是一个临时号码，馆员审核后，分配了一个正式号码，
                // 这时读者记录发生修改，dp2library会给公众号发送一个读者信息变更的消息。就需要用refid来检索
                string refId = "";
                bool bRefId = WxUserDatabase.CheckIsRefId(patronBarcode, out refId);
                if (bRefId == true)
                {
                    filter = filter & Builders<WxUserItem>.Filter.Eq("refID", refId);
                    //info.Append(" refID=" + refId);
                }
                else
                {
                    filter = filter & Builders<WxUserItem>.Filter.Eq("readerBarcode", patronBarcode);
                    //info.Append(" readerBarcode=" + patronBarcode);
                }
            }

            if (string.IsNullOrEmpty(userName) == false)
            {
                filter = filter & Builders<WxUserItem>.Filter.Eq("userName", userName);

                //info.Append(" userName=" + userName);
            }



            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();

            //dp2WeiXinService.Instance.WriteDebug("检索条件["+info.ToString()+"]，命中数量["+list.Count.ToString()+"]");



            return list;
        }



        /// <summary>
        /// 检查传进来的条码是不是refid
        /// 用于从mongodb库检索读者的函数
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="refiId"></param>
        /// <returns></returns>
        public static bool CheckIsRefId(string barcode, out string refiId)
        {
            refiId = "";

            if (barcode.Length > 7 && barcode.Substring(0, 7) == C_Prefix_RefId)
            {
                refiId = barcode.Substring(7);
                return true;
            }

            return false;
        }

        public static int CheckFrom(string weixinId)
        {
            // 浏览器来源
            if (weixinId.Length >= 2 && weixinId.Substring(0, 2) == C_Prefix_fromWeb)
            {
                return 2;
            }

            // 小程序来源
            if (weixinId.Length >= 2 && weixinId.Substring(0, 2) == C_Prefix_fromMina)
            {
                return 3;
            }

            // 默认是微信公众号来源
            return 1;
        }

        // 获取有效的绑定账户
        public List<WxUserItem> Get(string weixinId, string libId, int type)
        {
            return this.Get(weixinId, libId, null, type, null, null, true);
        }

        // 获取指定的读者账户,针对一个图书馆可绑定多个读者账户
        public List<WxUserItem> GetPatron(string weixinId, string libId, string readerBarcode)
        {
            return this.Get(weixinId, libId, null, C_Type_Patron, readerBarcode, null, true);
        }

        public List<WxUserItem> GetPatron(string weixinId, string libId, string libraryCode, string readerBarcode)
        {
            return this.Get(weixinId, libId, libraryCode, C_Type_Patron, readerBarcode, null, true);
        }

        public List<WxUserItem> GetWorkers(string weixinId, string libId, string userName)
        {
            return this.Get(weixinId, libId, null, C_Type_Worker, null, userName, true);
        }

        public List<WxUserItem> GetWorkers(string weixinId, string libId, string libraryCode, string userName)
        {
            return this.Get(weixinId, libId, libraryCode, C_Type_Worker, null, userName, true);
        }


        //// 获取工作人员账户，目前设计是：针对一个图书馆只绑定一个账户
        //public WxUserItem GetWorker(string weixinId, string libId,string userName)
        //{
        //    List<WxUserItem> list = this.GetWorkers(weixinId, libId, userName);
        //    if (list != null && list.Count >= 1)
        //        return list[0];

        //    return null;
        //}

        public WxUserItem GetPatronByPatronRefID(string weixinId,
            string libId,
            string patronRefID)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("state", C_State_Available)
                & Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("libId", libId)
                & Builders<WxUserItem>.Filter.Eq("refID", patronRefID);

            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();
            if (list != null && list.Count > 0)
                return list[0];

            return null;
        }

        /*
        // 获取当前活动的读者账户
        public WxUserItem GetActivePatron1(string weixinId,string libId)
        {
            WxUserItem activePatron = null;
            int count = 0;
            List<WxUserItem> list = this.Get(weixinId, libId, C_Type_Patron);
            if (list != null && list.Count > 0)
            {
                foreach (WxUserItem item in list)
                {
                    if (item.isActive == 1)
                    {
                        activePatron = item;
                        count++;
                    }
                } 
            }

            if (count>1)
                throw new Exception("程序异常：微信号活动读者数量有" +list.Count+"个");

            return activePatron;
        }
        */

        // 获取当前活动的账户  可能是读者也可能是工作人员，只有一个
        public WxUserItem GetActive(string weixinId)
        {
            WxUserItem activeUser = null;
            List<WxUserItem> activeList = new List<WxUserItem>();

            // 这里不再区分图书馆和类型，读者与工作人员当前只能有一个活动的
            List<WxUserItem> list = this.Get(weixinId, "", -1);
            if (list != null && list.Count > 0)
            {
                foreach (WxUserItem item in list)
                {
                    if (item.isActive == 1)
                    {
                        activeList.Add(item);
                    }
                }
            }

            
            if (activeList.Count > 0)
            {
                // 认第一条为活动帐户
                activeUser = activeList[0];
                if (activeList.Count > 1)  // 活动帐号大于1异常，自动将其它几条设为非活动。
                {
                    string strError = "发现微信号活动读者帐户有" + list.Count + "个,程序自动将除第1个外的其余帐户没为非活动态";
                    dp2WeiXinService.Instance.WriteDebug(strError);
                    for (int i = 1; i < activeList.Count; i++)
                    {
                        WxUserItem item = activeList[i];
                        item.isActive = 0;
                        WxUserDatabase.Current.Update(item);
                        dp2WeiXinService.Instance.WriteDebug(item.userName + "/" + item.readerBarcode);
                    }
                }
            }

            // 有绑号帐号，但没有对应的活动帐号，自动将第一个帐户设为活动帐户
            if (list.Count > 0 && activeList.Count == 0)
            {
                dp2WeiXinService.Instance.WriteDebug("发现微信号有绑定帐户，但当前没有活动帐户，自动将第一条设为活动帐户");
                activeUser = list[0];
                activeUser.isActive = 1;
                WxUserDatabase.Current.Update(activeUser);
                dp2WeiXinService.Instance.WriteDebug(activeUser.userName + "/" + activeUser.readerBarcode);
            }


            return activeUser;
        }

        public WxUserItem CreatePublic(string weixinId, string libId, string bindLibraryCode)
        {
            WxUserItem userItem = new WxUserItem();

            userItem.weixinId = weixinId;
            userItem.libId = libId;
            Library lib = dp2WeiXinService.Instance.LibManager.GetLibrary(libId);
            if (lib == null)
            {
                throw new Exception("libid=" + libId + "对应的对象没有找到。");
            }

            userItem.libName = lib.Entity.libName;
            userItem.bindLibraryCode = bindLibraryCode; //界面上选择的绑定分馆
            if (String.IsNullOrEmpty(userItem.bindLibraryCode) == false)
            {
                userItem.libName = userItem.bindLibraryCode;
            }

            userItem.readerBarcode = "";
            userItem.readerName = "";
            userItem.department = "";
            userItem.phone = "";
            userItem.gender = "";
            userItem.patronState = ""; //2020-3-7加
            userItem.noPassReason = "";
            userItem.isRegister = false;

            userItem.xml = "";
            userItem.refID = "";
            userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
            userItem.updateTime = userItem.createTime;
            userItem.isActive = 1; // 是否为活动帐号

            userItem.libraryCode = ""; //实际分馆
            userItem.type = WxUserDatabase.C_Type_Worker;
            userItem.userName = WxUserDatabase.C_Public;
            userItem.isActiveWorker = 0;//20220720该字段已作废。////是否是激活的工作人员账户，读者时均为0
            userItem.tracing = "off";//默认是关闭监控
            userItem.location = "";
            userItem.selLocation = "";
            userItem.verifyBarcode = 0;
            userItem.audioType = 4;
            userItem.state = WxUserDatabase.C_State_Available; //1;
            userItem.remark = "";
            userItem.rights = "";
            userItem.showCover = 1;
            userItem.showPhoto = 1;

            WxUserDatabase.Current.Add(userItem);

            dp2WeiXinService.Instance.WriteDebug("自动以绑public身份新建一条");

            return userItem;
        }

        // 绑定微信用户绑定的图书馆id列表
        public List<string> GetLibsByWeixinId(string weixinId)
        {
            List<string> libs = new List<string>();
            List<WxUserItem> list = this.Get(weixinId, null, -1);
            if (list != null && list.Count > 0)
            {
                foreach (WxUserItem item in list)
                {
                    if (libs.Contains(item.libId) == false)
                    {
                        libs.Add(item.libId);
                    }
                }
            }
            return libs;
        }

        // 获取一个读者账户
        public WxUserItem GetOnePatron(string weixinId, string libId)
        {
            List<WxUserItem> list = this.Get(weixinId, libId, C_Type_Patron);
            if (list != null && list.Count > 0)
                return list[0];

            return null;
        }

        /// <summary>
        /// 根据id获取对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WxUserItem GetById(String id)
        {
            if (string.IsNullOrEmpty(id) == true || id == "null")
                return null;
            IMongoCollection<WxUserItem> collection = this.wxUserCollection;
            var filter = Builders<WxUserItem>.Filter.Eq("id", id);
            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();
            if (list.Count > 0)
            {
                return list[0];
            }
            return null;
        }


        private static readonly Object _sync_addUser = new Object();
        /// 新增绑定账户
        public WxUserItem Add(WxUserItem item)
        {
            lock (_sync_addUser)
            {
                dp2WeiXinService.Instance.WriteDebug("走进WxUserDatabase.Add(),id=" + item.id + " weixinid=" + item.weixinId);

                List<WxUserItem> itemList = this.Get(item.weixinId, item.libId, null, item.type, item.readerBarcode, item.userName, true);
                if (itemList.Count == 0)
                {
                    this.wxUserCollection.InsertOne(item);
                    dp2WeiXinService.Instance.WriteDebug("InsertOne(),id=" + item.id);
                }
                else
                {
                    dp2WeiXinService.Instance.WriteDebug("发现绑定帐户库中已有'" + item.readerBarcode + "'或'" + item.userName + "'对应的记录。");
                }
            }
            return item;
        }

        /// 删除账户
        /// 如果删除的是读者账户，自动将对应图书馆第一个读者账户设为默认的
        public void Delete1(String id, out WxUserItem newActiveUser)
        {
            newActiveUser = null;

            dp2WeiXinService.Instance.WriteDebug("走进WxUserDatabase.Delete() id=" + id);

            if (string.IsNullOrEmpty(id) == true || id == "null")
                return;

            // 检查一下是否被删除读者是否为默认读者，如果是，后面把自动将默认值设了第一个读者上。
            WxUserItem userItem = this.GetById(id);
            if (userItem == null)
                return;
            dp2WeiXinService.Instance.WriteDebug("走进WxUserDatabase.Delete(),id=" + userItem.id + " weixinid=" + userItem.weixinId);

            // 2022/7/20 增加，删除public帐户时，就不要再创建一个新的public做为action了，否则一直删除不干净
            bool ispublic = false;

            string weixinId = "";
            int type = -1;
            int isActive = 0;
            string libId = "";
            string tracing = "";
            string bindLibraryCode = "";
            if (userItem != null)
            {
                weixinId = userItem.weixinId;
                type = userItem.type;
                isActive = userItem.isActive;
                libId = userItem.libId;
                tracing = userItem.tracing;
                bindLibraryCode = userItem.bindLibraryCode;

                // 2022/7/20 增加，删除public帐户时，就不要再创建一个新的public做为action了，否则一直删除不干净
                if (userItem.userName == "public")
                    ispublic = true;
            }

            // 先删除
            IMongoCollection<WxUserItem> collection = this.wxUserCollection;
            var filter = Builders<WxUserItem>.Filter.Eq("id", id);
            collection.DeleteOne(filter);

            // 如果删除的是读者账户且是激活的，自动将本馆第一个读者账户设为默认的
            if (isActive == 1)
            {
                // 先取本馆的绑定帐户
                List<WxUserItem> list = this.Get(weixinId, libId, -1);
                if (list != null && list.Count > 0)
                {
                    newActiveUser = list[0];
                }

                // 如果本馆帐户没找到，则把该微信号绑定的第一个帐户设为活动的
                if (newActiveUser == null)
                {
                    list = this.Get(weixinId, "", -1);
                    if (list != null && list.Count > 0)
                    {
                        newActiveUser = list[0];
                    }
                }

                // 没有一个绑定帐户时，为刚才的馆创建一个public
                if (newActiveUser == null
                    &&  ispublic==false)  // 2022/7/20 增加，删除public帐户时，就不要再创建一个新的public做为action了，否则一直删除不干净

                {
                    newActiveUser = this.CreatePublic(weixinId, libId, bindLibraryCode);
                }

                // 设为活动状态，让外面调的函数设置，因为刚设数据库不行，也要更新当前session，这样外面一起做
                //this.SetActivePatron1(newActivePatron.weixinId, newActivePatron.id);
            }

            //// 如果是工作人员,且打开监控功能
            //if (type==1 && String.IsNullOrEmpty(tracing)==false && tracing !="off")
            //{
            //    dp2WeiXinService.Instance.UpdateMemoryTracingUser(weixinId, libId, "off");
            //}
        }

        public void SimpleDelete(String id)
        {
            //dp2WeiXinService.Instance.WriteDebug("1.走进WxUserDatabase.SimpleDelete()");

            if (string.IsNullOrEmpty(id) == true)
                return;

            //dp2WeiXinService.Instance.WriteDebug("2.走进WxUserDatabase.SimpleDelete() id=" + id);

            // 从mongodb库删除
            IMongoCollection<WxUserItem> collection = this.wxUserCollection;
            var filter = Builders<WxUserItem>.Filter.Eq("id", id);
            DeleteResult ret = collection.DeleteOne(filter);

            //dp2WeiXinService.Instance.WriteDebug("3.共删除成功" + ret.DeletedCount + "个对象");
        }

        // 根据libId与状态删除记录
        public void Delete(string libId, int state)
        {
            if (String.IsNullOrEmpty(libId) == true)
                return;

            var filter = Builders<WxUserItem>.Filter.Eq("libId", libId);
            if (state != -1)
            {
                filter = filter & Builders<WxUserItem>.Filter.Eq("state", state);
            }
            DeleteResult ret = this.wxUserCollection.DeleteMany(filter);
        }

        public void SetState(string libId, int fromState, int toState)
        {
            if (string.IsNullOrEmpty(libId) == true)
                return;

            // 查找指定图书馆的账户
            var filter = Builders<WxUserItem>.Filter.Eq("libId", libId);
            if (fromState != -1)
            {
                filter = filter & Builders<WxUserItem>.Filter.Eq("state", fromState);
            }


            var update = Builders<WxUserItem>.Update
                .Set("state", toState)
                .Set("updateTime", DateTimeUtil.DateTimeToString(DateTime.Now));
            UpdateResult ret = this.wxUserCollection.UpdateMany(filter, update);
        }

        public List<WxUserItem> GetActivePatrons1()
        {
            var filter = Builders<WxUserItem>.Filter.Eq("isActive", 1);
            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();
            return list;
        }


        /// <summary>
        /// 激活读者账户
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="id"></param>
        public void SetActivePatron1(string weixinId, string id)
        {
            if (string.IsNullOrEmpty(weixinId) == true)
                return;

            if (string.IsNullOrEmpty(id) == true)
                return;

            // 先将该微信用户的所有帐户都设为非活动
            this.SetNoActivePatron(weixinId);

            // 再将参数传入的记录设为活动状态
            var filter = Builders<WxUserItem>.Filter.Eq("id", id);
            var update = Builders<WxUserItem>.Update
                .Set("isActive", 1)
                .Set("updateTime", DateTimeUtil.DateTimeToString(DateTime.Now));
            this.wxUserCollection.UpdateMany(filter, update);
        }

        public void SetNoActivePatron(string weixinId)
        {
            if (string.IsNullOrEmpty(weixinId) == true)
                return;

            // 将该微信用户的所有绑定读者都设为非活动
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId);
            //& Builders<WxUserItem>.Filter.Eq("type", 0); // 2018/3/10不过滤读者，jane 2016-6-16 注意只存读者账户
            var update = Builders<WxUserItem>.Update
                .Set("isActive", 0)
                .Set("updateTime", DateTimeUtil.DateTimeToString(DateTime.Now));
            UpdateResult ret = this.wxUserCollection.UpdateMany(filter, update);
        }

        /// <summary>
        /// 设置监控开关
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tracing"></param>
        /// <returns></returns>
        public long UpdateTracing(string id, string tracing)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("id", id);
            var update = Builders<WxUserItem>.Update
                .Set("tracing", tracing)
                ;
            UpdateResult ret = this.wxUserCollection.UpdateOne(filter, update);
            return ret.ModifiedCount;
        }

        /// <summary>
        /// 更新微信用户
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public long Update(WxUserItem item)
        {
            dp2WeiXinService.Instance.WriteDebug("走进WxUserDatabase.Update(),id=" + item.id + " weixinid=" + item.weixinId);

            var filter = Builders<WxUserItem>.Filter.Eq("id", item.id);
            var update = Builders<WxUserItem>.Update
                .Set("weixinId", item.weixinId)
                .Set("libName", item.libName)
                .Set("libId", item.libId)
                .Set("bindLibraryCode", item.bindLibraryCode)

                .Set("readerBarcode", item.readerBarcode)
                .Set("readerName", item.readerName)
                //.Set("readerBarcodeByReaderGet", item.readerBarcodeByReaderGet)  //2021/7/29为脱敏显示增加
                // .Set("readerNameByReaderGet", item.readerNameByReaderGet)  //2021/7/29为脱敏显示增加

                .Set("department", item.department)
                .Set("phone", item.phone)
                .Set("gender", item.gender) // 2020/6/2 性别
                .Set("patronState", item.patronState)
                 .Set("noPassReason", item.noPassReason)  // 2020-3-8 不通过原因
                .Set("isRegister", item.isRegister)  //2020-3-7 是否读者自助注册的
                .Set("xml", item.xml)
                .Set("recPath", item.recPath)

                .Set("refID", item.refID)
                .Set("createTime", item.createTime)
                .Set("updateTime", item.updateTime)
                .Set("isActive", item.isActive)


                .Set("libraryCode", item.libraryCode)
                .Set("type", item.type)
                .Set("userName", item.userName)
                .Set("isActiveWorker", item.isActiveWorker)  //20220720该字段已作废。

                // 2016-8-26 jane 加
                .Set("state", item.state)
                .Set("remark", item.remark)
                .Set("rights", item.rights)
                .Set("appId", item.appId)
                .Set("tracing", item.tracing)
                .Set("location", item.location)  // 自己可管理的馆藏地
                .Set("selLocation", item.selLocation)  // 关注的馆藏地
               .Set("verifyBarcode", item.verifyBarcode)
               .Set("audioType", item.audioType) //2018/1/2

                /*
         public int showPhoto { get; set; }
         public int showCover { get; set; }
         public string bookSubject { get; set; } // 20170509加
                 */
                .Set("showPhoto", item.showPhoto)
                .Set("showCover", item.showCover)
                .Set("bookSubject", item.bookSubject)
                ;

            UpdateResult ret = this.wxUserCollection.UpdateOne(filter, update);
            return ret.ModifiedCount;
        }


        // 获得打开了监控功能的帐户
        internal List<WxUserItem> GetTracingUsers()
        {
            var filter = Builders<WxUserItem>.Filter.Eq("tracing", "on")
                | Builders<WxUserItem>.Filter.Eq("tracing", "on -mask");
            ;
            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();
            return list;
        }
    }

    // 绑定帐号结构
    public class WxUserItem
    {
        // 绑定帐户记录的id，mongodb库中记录的唯一id`
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; private set; }

        // 用户来源唯一号，格式如下：
        // web浏览器来源的，~~开头
        // 微信公众号来源的，weixinId@公众号appid
        // 小程序来源的：**用户id
        public string weixinId { get; set; }

        // 图书馆id，是我爱图书馆这边存储在mongodb库的libid，不是dp2library端的libId
        public string libId { get; set; }

        // 图书馆名称
        public string libName { get; set; }

        // 20170509
        //绑定时选择的分馆代码，与用户在dp2library端的实际分馆代码 可能不同，
        //比如绑定时选择的星洲小学，但实际馆代码是总馆的，因为对于工作人员来说，
        //可能管理多个分馆，所以总馆帐户或管理多个分馆的帐户，是可以绑定管理的每个分馆的。
        public string bindLibraryCode { get; set; }

        //用户在dp2library系统的分馆代码，读者与工作人员都有该字段
        public string libraryCode { get; set; }

        // 读者证条码号
        public string readerBarcode { get; set; }

        // 读者姓名
        public string readerName { get; set; }

        // 2022/07/20 整理代码时，发现下面两个字段未使用，作废，
        // 但由于mongodb原来的数据有这两个字段，所以结构还不能少这两个字段
        // 20210729由于要求显示已绑定清单脱敏，所以增加了两个用读者身份获取的字段
        public string readerBarcodeByReaderGet { get; set; }
        public string readerNameByReaderGet { get; set; }

        // 20210729用于已绑定列表脱敏显示
        public string displayReaderBarcode
        {
            get
            {
                // 2021/8/20 注释掉，改掉原来绑定后，用读者身份再获取一次信息的做法
                //if (readerBarcodeByReaderGet != null)
                //    return readerBarcodeByReaderGet;
                //else
                //    return readerBarcode;

                // 2021/8/20 采用公众号这边的脱敏配置
                bool send2PatronIsMask = false;
                string maskDef = dp2WeiXinService.Instance.GetMaskDef(libId, bindLibraryCode, out send2PatronIsMask);
                if (send2PatronIsMask == true && string.IsNullOrEmpty(readerBarcode) == false)
                    return dp2WeiXinService.Mask(maskDef, readerBarcode, "barcode");
                else
                    return readerBarcode;

            }
        }

        // 20210729用于已绑定列表脱敏显示
        public string displayReaderName
        {
            get
            {
                // 2021/8/20 注释掉，改掉原来绑定后，用读者身份再获取一次信息的做法
                //if (readerNameByReaderGet != null)
                //    return readerNameByReaderGet;
                //else
                //    return readerName;

                // 2021/8/20 采用公众号这边的脱敏配置
                bool send2PatronIsMask = false;
                string maskDef = dp2WeiXinService.Instance.GetMaskDef(libId, bindLibraryCode, out send2PatronIsMask);
                if (send2PatronIsMask == true && string.IsNullOrEmpty(readerName) == false)
                    return dp2WeiXinService.Mask(maskDef, readerName, "name");
                else
                    return readerBarcode;
            }
        }

        //单位
        public string department { get; set; }  //部门，二维码下方显示 // 2016-6-16 新增

        // 读者手机号 2020-3-1 新增
        public string phone { get; set; }

        //性别 2020/6/2增加 
        public string gender { get; set; }

        // 2020-3-7 读者状态，读者注册提交后是待审核
        public string patronState { get; set; }

        // 2020-3-8 不通过原因
        public string noPassReason { get; set; }

        // 是否是读者自助注册的，1表示是读者自助注册的
        public bool isRegister = false;

        // 读者记录xml
        public string xml { get; set; }

        // 读者记录路径
        public string recPath { get; set; }

        // 读者参考id
        public string refID { get; set; }

        // 绑定帐户的创建时间和更新时间
        public string createTime { get; set; }
        public string updateTime { get; set; }

        // 是否为当前活动状态，
        // 馆员和读者都使用此状态，当前所有绑定的馆员和读者中，只能有一条活动的帐户
        public int isActive = 0;

        //账户类型：0表示读者 1表示工作人员 // 2016-6-16 新增
        public int type = 0;

        //当type=2时，表示工作人员账户名称，其它时候为空// 2016-6-16 新增       
        public string userName { get; set; }

        // 20220720该字段已作废，但由于mongodb中以前的数据有这个字段，所以结构上还不能少，否则会报错。
        // 2022/7/20 整理接口代码时，发现这个字段无意义，馆员和读者均使用isActive字段
        //是否为当前激活工作人员账户，注意该字段对读者账户无意义（均为0），暂时未用到
        public int isActiveWorker= 0; 


        // 2016-8-26 jane 新增
        // //状态:0失效 1有效 2恢复时的临时状态 
        public int state { get; set; }

        //备注，会存一下绑定方式等
        public string remark { get; set; }

        // 权限
        public string rights { get; set; }

        // 公众号id，后来weixinId字段里带了appId
        public string appId { get; set; }

        //是否监控消息 工作人员时有意义，type=1 默认为空或者off
        public string tracing { get; set; }
        // 监控的消息是否做马赛克处理
        public bool IsMask
        {
            get
            {
                if (tracing == null)
                    tracing = "";

                // todo 空认为是马赛克还是不马赛克 2020-3-9

                if (this.tracing.IndexOf("-mask") != -1)  // -mask 指不做马赛克处理
                    return false;
                else
                    return true;
            }
        }

        // 20170213 jane
        // 本用户在dp系统有权限的馆藏地，是xml格式
        /*
        <item canborrow="no" itemBarcodeNullable="yes">保存本库</item>
        <item canborrow="no" itemBarcodeNullable="yes">阅览室</item>
        <item canborrow="yes" itemBarcodeNullable="yes">流通库</item>
        <library code="方洲小学">
        <item canborrow="yes" itemBarcodeNullable="yes">图书总库</item>
        </library>
        <library code="星洲小学">
        <item canborrow="yes" itemBarcodeNullable="yes">阅览室</item>
        </library>
         */
        public string location { get; set; }

        // 20170213 在微信中选择的馆藏地，是以逗号分隔的两级路径，如：/流通库,方洲小学/图书总库
        public string selLocation { get; set; }

        // 借还时是否校验条码
        public int verifyBarcode { get; set; }

        // 借还时语音方案
        public int audioType { get; set; }

        // 是否显示头像
        public int showPhoto { get; set; }

        // 检索时，是否显示图书封面
        public int showCover { get; set; }
        //上次记住的图书推荐栏目
        public string bookSubject { get; set; } // 20170509加

        // 是否是数字平台管理员，只读属性
        public bool IsdpAdmin
        {
            get
            {
                if (this.libName == WeiXinConst.C_Dp2003LibName)
                    return true;
                else
                    return false;
            }
        }

        public string Dump()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("id=[" + this.id + "] ");
            sb.AppendLine("weixinId=[" + this.weixinId + "] ");
            sb.AppendLine("libId=[" + this.libId + "] ");
            sb.AppendLine("libName=[" + this.libName + "] ");
            sb.AppendLine("libraryCode=[" + this.libraryCode + "] ");
            sb.AppendLine("bindLibraryCode=[" + this.bindLibraryCode + "] ");
            sb.AppendLine("readerBarcode=[" + this.readerBarcode + "] ");
            sb.AppendLine("readerName=[" + this.readerName + "] ");
            sb.AppendLine("department=[" + this.department + "] ");
            sb.AppendLine("phone=[" + this.phone + "] ");  //
            sb.AppendLine("gender=[" + this.gender + "] ");  //2020/6/2 性别
            sb.AppendLine("patronState=[" + this.patronState + "] ");  //2020-3-7  //noPassReason
            sb.AppendLine("noPassReason=[" + this.noPassReason + "] ");  //2020-3-8
            sb.AppendLine("isRegister=[" + this.isRegister + "] ");  // 2020-3-7
            sb.AppendLine("refID=[" + this.refID + "] ");
            sb.AppendLine("isActive=[" + this.isActive + "] ");
            sb.AppendLine("userName=[" + this.userName + "] ");
            sb.AppendLine("tracing=[" + this.tracing + "] ");

            sb.AppendLine("location=[" + this.location + "] ");
            sb.AppendLine("selLocation=[" + this.selLocation + "] ");
            sb.AppendLine("verifyBarcode=[" + this.verifyBarcode + "] ");

            sb.AppendLine("audioType=[" + this.audioType + "] ");
            sb.AppendLine("showPhoto=[" + this.showPhoto + "] ");
            sb.AppendLine("showCover=[" + this.showCover + "] ");

            return sb.ToString();
        }
    }


    /// <summary>
    /// 绑定帐户使用的结构
    /// 2016-8-26 为了不混乱使用字段，绑定时参数不再借用WxUserItem，新增bindItem,用于绑定接口
    /// </summary>
    public class BindItem
    {
        // 用户来源唯一号，格式如下：
        // web浏览器来源的，~~开头
        // 微信公众号来源的，weixinId@公众号appid
        // 小程序来源的：**用户id
        public string weixinId { get; set; }

        // 图书馆id
        public string libId { get; set; }

        // 馆代码，传选择的图书馆的馆代码
        public string bindLibraryCode { get; set; }

        // 绑定途径，目前可以传4种，其中前3种都是针对读者，最后一种UN表示工作人员
        // NB:姓名(不是账户名)
        // 空:证条码号
        // TP:电话号码
        // UN:工作人员账户
        public string prefix { get; set; }  //必须设为属性，才能在前端传值。

        // 关键词，根据绑定途径输入的实际信息，例如实际的姓名、证条码号、帐户名
        public string word { get; set; }

        // 密码
        public string password { get; set; }
    }



}
