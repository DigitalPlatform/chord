using DigitalPlatform.IO;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    /// <summary>
    /// 用户数据库
    /// </summary>
    public sealed class WxUserDatabase
    {
        //常量
        public const int C_Type_Patron = 0;
        public const int C_Type_Worker = 1;

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

            // todo 创建索引            
            bool bExist = false;
            var indexes = _wxUserCollection.Indexes.ListAsync().Result.ToListAsync().Result;
            foreach (BsonDocument doc in indexes)
            {
            }
            // _logCollection.DropAllIndexes();
            if (bExist == false)
            {
                CreateIndex();
            }
        }

        // 创建索引
        public void CreateIndex()
        {
            var options = new CreateIndexOptions() { Unique = false };  //不唯一，一个微信用户可能对应多个读者
            _wxUserCollection.Indexes.CreateOne(
                Builders<WxUserItem>.IndexKeys.Ascending("weixinId"),
                options);
        }

        /// <summary>
        /// 获取读者账户,针对一个图书馆可绑定多个读者账户
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libCode"></param>
        /// <param name="readerBarcode"></param>
        /// <returns></returns>
        public WxUserItem GetPatronAccount(string weixinId, string libCode,string readerBarcode)
        {
            // 先查到weixinId+libCode+readerBarcode唯一的记录
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("libCode", libCode)
                & Builders<WxUserItem>.Filter.Eq("readerBarcode", readerBarcode)
                & Builders<WxUserItem>.Filter.Eq("type", C_Type_Patron);

            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();
            if (list.Count >= 1)
                return list[0];

            /*
            // 兼容之前微信消息方法，单独选择图书馆的情况
            // 未找到查weixinId+libCode，readerBarcoe为空的记录
            filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("libCode", libCode)
                & Builders<WxUserItem>.Filter.Eq("readerBarcode", "")
                & Builders<WxUserItem>.Filter.Eq("type", C_Type_Patron);
            list = this.wxUserCollection.Find(filter).ToList();
            if (list.Count >= 1)
                return list[0];
            */

            return null;
        }

        /// <summary>
        /// 获取工作人员账户，目前设计是：针对一个图书馆只绑定一个账户
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libCode"></param>
        /// <returns></returns>
        public WxUserItem GetWorkerAccount(string weixinId, string libCode)
        {
            // 先查到weixinId+libCode+readerBarcode唯一的记录
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("libCode", libCode)
                & Builders<WxUserItem>.Filter.Eq("type", C_Type_Worker);
            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();
            if (list.Count >= 1)
                return list[0];

            return null;
        }

        public WxUserItem GetOneWorkerAccount(string weixinId)
        {
            // 先查到weixinId+libCode+readerBarcode唯一的记录
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("type", C_Type_Worker);
            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();
            if (list.Count >= 1)
                return list[0];

            return null;
        }

        /// <summary>
        /// 获取当前激活的读者账户
        /// </summary>
        /// <param name="weixinId"></param>
        /// <returns></returns>
        public WxUserItem GetActivePatron(string weixinId)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("isActive", 1)
                & Builders<WxUserItem>.Filter.Eq("type", C_Type_Patron);

            List<WxUserItem> list= this.wxUserCollection.Find(filter).ToList();//.ToListAsync().Result;
            if (list.Count > 1)
                throw new Exception("程序异常：微信号活动读者数量有" +list.Count+"个");

            if (list.Count == 1)
                return list[0];

            return null;
        }

        /// <summary>
        /// 获取绑定的所有账户，不区分读者与工作人员
        /// </summary>
        /// <param name="weixinId"></param>
        /// <returns></returns>
        public List<WxUserItem> GetAllByWeixinId(string weixinId)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId);
            return this.wxUserCollection.Find(filter).ToList();
        }

        /// <summary>
        /// 获取微信用户绑定的所有读者账户
        /// </summary>
        /// <param name="weixinId"></param>
        /// <returns></returns>
        public List<WxUserItem> GetPatronsByWeixinId(string weixinId)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                 & Builders<WxUserItem>.Filter.Eq("type", C_Type_Patron);  // 2016-6-16 jane 查读者账户

            return this.wxUserCollection.Find(filter).ToList();//.ToListAsync().Result;
        }

        /// <summary>
        /// 获取一个读者账户
        /// </summary>
        /// <param name="weixinId"></param>
        /// <returns></returns>
        public WxUserItem GetOnePatronByWeixinId(string weixinId)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("type",C_Type_Patron);  // 2016-6-16 jane 查读者账户

            List<WxUserItem>list= this.wxUserCollection.Find(filter).ToList();//.ToListAsync().Result;
            if (list.Count > 0)
                return list[0];

            return null;
        }

        /// <summary>
        /// 查找所有用户
        /// </summary>
        /// <returns></returns>
        public List<WxUserItem> GetUsers()
        {
            IFindFluent<WxUserItem, WxUserItem> f = this.wxUserCollection.Find(new BsonDocument());
            if (f != null)
                return f.ToList();

            return null;
        }

        /// <summary>
        /// 新增微信用户
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public WxUserItem Add(WxUserItem item)
        {
            this.wxUserCollection.InsertOne(item);
            return item;
        }

        /// <summary>
        /// 更新微信用户
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public long Update(WxUserItem item)
        {
            IMongoCollection<WxUserItem> collection = this.wxUserCollection;

            /*
                userItem.weixinId = strWeiXinId;
                userItem.libCode = libCode;
                userItem.libUserName = remoteUserName;
                userItem.libName = lib.libName;

                userItem.readerBarcode = readerBarcode;
                userItem.readerName = readerName;
                userItem.department = department;
                userItem.xml = xml;

                userItem.refID = refID;
                userItem.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
                userItem.updateTime = userItem.createTime;
                userItem.isActive = 0; // isActive只针对读者，后面会激活读者，工作人员时均为0

                userItem.prefix = strPrefix;
                userItem.word = strWord;
                userItem.fullWord = strFullWord;
                userItem.password = strPassword;

                userItem.libraryCode = libraryCode;
                userItem.type = type;
                userItem.userName = userName;
                userItem.isActiveWorker = 0;//是否是激活的工作人员账户，读者时均为0             
             */
            var filter = Builders<WxUserItem>.Filter.Eq("id", item.id);
            var update = Builders<WxUserItem>.Update
                .Set("weixinId", item.weixinId)
                .Set("libCode", item.libCode)
                .Set("libUserName", item.libUserName)
                .Set("libName", item.libName)

                .Set("readerBarcode", item.readerBarcode)
                .Set("readerName", item.readerName)
                .Set("department", item.department)
                .Set("xml", item.xml)
                
                .Set("refID", item.refID)
                .Set("createTime", item.createTime)
                .Set("updateTime", item.updateTime)
                .Set("isActive", item.isActive)

                .Set("prefix", item.prefix)
                .Set("word", item.word)
                .Set("fullWord", item.fullWord)
                .Set("password", item.password)

                .Set("libraryCode", item.libraryCode)
                .Set("type", item.type)
                .Set("userName", item.userName)
                .Set("isActiveWorker", item.isActiveWorker)
                ;

            UpdateResult ret = collection.UpdateOne(filter, update);
            return ret.ModifiedCount;
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

        /// <summary>
        /// 删除账户
        /// 如果删除的是读者账户，自动将第一个读者账户设为默认的
        /// </summary>
        /// <param name="id"></param>
        public void Delete(String id)
        {
            if (string.IsNullOrEmpty(id) == true || id=="null")
                return;           

            // 检查一下是否被删除读者是否为默认读者，如果是，后面把自动将默认值设了第一个读者上。
            WxUserItem userItem = this.GetById(id);
            if (userItem==null)
                return;

            string weixinId = "";
            int type = -1;
            int isActive = 0;
            if (userItem != null)
            {
                weixinId = userItem.weixinId;
                type = userItem.type;
                isActive = userItem.isActive;
            }

            // 先删除
            IMongoCollection<WxUserItem> collection = this.wxUserCollection;
            var filter = Builders<WxUserItem>.Filter.Eq("id", id);
            collection.DeleteOne(filter);
            
            // 如果删除的是读者账户且是激活的，自动将第一个读者账户设为默认的
            if (type == 0 && isActive==1)
            {
                WxUserItem newUserItem = this.GetOnePatronByWeixinId(weixinId);
                if (newUserItem != null)
                    this.SetPatronActive(newUserItem.weixinId, newUserItem.id);
            }
        }

        /// <summary>
        /// 激活读者账户
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="id"></param>
        public void SetPatronActive(string weixinId,string id)
        {
            if (string.IsNullOrEmpty(weixinId) == true)
                return;

            if (string.IsNullOrEmpty(id) == true)
                return;

            IMongoCollection<WxUserItem> collection = this.wxUserCollection;

            // 先将该微信用户的所有绑定读者都设为非活动
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("type", 0); // jane 2016-6-16 注意只存读者账户

            var update = Builders<WxUserItem>.Update
                .Set("isActive", 0)
                .Set("updateTime", DateTimeUtil.DateTimeToString(DateTime.Now));
            UpdateResult ret = collection.UpdateMany(filter, update);

            // 再将参数传入的记录设为活动状态
            filter = Builders<WxUserItem>.Filter.Eq("id",id);
            update = Builders<WxUserItem>.Update
                .Set("isActive", 1)
                .Set("updateTime", DateTimeUtil.DateTimeToString(DateTime.Now));
            ret = collection.UpdateMany(filter, update);
        }

    }
    public class WxUserItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; private set; }

        public string weixinId { get; set; } // 绑定必备
        public string libCode { get; set; } // 绑定必备
        public string libUserName { get; set; }// 绑定必备
        public string libName { get; set; }// 绑定必备   

        public string readerBarcode { get; set; }
        public string readerName { get; set; }
        public string department { get; set; } //部门，二维码下方显示 // 2016-6-16 新增
        public string xml { get; set; }

        public string refID { get; set; }
        public string createTime { get; set; } // 创建时间
        public string updateTime { get; set; } // 更校报时间
        //是否就当前激活读者状态，注意该字段对工作人员账户无意义（均为0）
        public int isActive = 0;


        // 绑定必备
        public string prefix { get; set; }  //必须设为属性，才能在前端传值。
        public string word  { get; set; }
        public string fullWord { get; set; } // 服务器用fullWord将strPrefix:strWord存在一起
        public string password { get; set; }

        public string libraryCode { get; set; } //分馆代码，读者与工作人员都有该字段，注意与libCode区分，libCode是微信端定义的绑定的图书馆代码 // 2016-6-16 新增
        public int type = 0;//账户类型：0表示读者 1表示工作人员 // 2016-6-16 新增
        public string userName { get; set; } //当type=2时，表示工作人员账户名称，其它时候为空// 2016-6-16 新增
        //是否就当前激活工作人员账户，注意该字段对读者账户无意义（均为0）
        public int isActiveWorker= 0;

        /*
        在后面编写各种管理功能的时候，需要检查工作人员账号的 rights 字符串，看看权限是不是足够。
        虽然刚才提到，绑定工作人员账号时就可以通过返回的 XML 字符串得到这个账号的 rights 字符串，
        但我觉得不应该在你的 mongodb 数据库中记忆这个字符串。因为dp2library里面随时可以修改这个账户的权限，
        如果没有良好的同步机制，那么公众号模块不如每次需要的时候去临时获取这个字符串。
         */
        //public string right { get; set; }//没有很好的缓存机制，先不加权限字段
    }


}
