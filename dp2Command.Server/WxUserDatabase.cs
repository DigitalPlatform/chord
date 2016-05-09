using DigitalPlatform.IO;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Command.Service
{
    /// <summary>
    /// 用户数据库
    /// </summary>
    public class WxUserDatabase
    {
        private static WxUserDatabase _db = new WxUserDatabase();
        public static WxUserDatabase Current
        {
            get
            {
                return _db;
            }
        }

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
        public void Open(
            string strMongoDbConnStr,
            string strInstancePrefix)
        {
            if (string.IsNullOrEmpty(strMongoDbConnStr) == true)
                throw new ArgumentException("strMongoDbConnStr 参数值不应为空");

            if (string.IsNullOrEmpty(strInstancePrefix) == false)
                strInstancePrefix = strInstancePrefix + "_";
            _wxUserDbName = strInstancePrefix + "user";

            this._mongoClient = new MongoClient(strMongoDbConnStr);
            this._database = this._mongoClient.GetDatabase(this._wxUserDbName);

            //图书馆点对点账号
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

        // 清除集合内的全部内容
        public async Task Clear()
        {
            if (_wxUserCollection == null)
            {
                throw new Exception("访问日志 mongodb 集合尚未初始化");
            }

            // https://docs.mongodb.org/getting-started/csharp/remove/
            var filter = new BsonDocument();
            await _wxUserCollection.DeleteManyAsync(filter);
            CreateIndex();
        }

        /*
        /// <summary>
        /// 根据微信号与图书馆代码查
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libCode"></param>
        /// <returns></returns>
        public WxUserItem GetActive(string weixinId,string libCode)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("libCode", libCode)
                & Builders<WxUserItem>.Filter.Eq("isActive", 1);

            List<WxUserItem> list = this.wxUserCollection .Find(filter).ToList();
            if (list.Count > 0)
                return list[0];

            return null;
        }
        */
        public WxUserItem GetActiveOrFirst(string weixinId, string libCode)
        {
            // 先查active的
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("libCode", libCode)
                & Builders<WxUserItem>.Filter.Eq("isActive", 1);

            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();
            if (list.Count > 0)
                return list[0];

            // 没有查first
            filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                     & Builders<WxUserItem>.Filter.Eq("libCode", libCode);

            list = this.wxUserCollection.Find(filter).ToList();
            if (list.Count > 0)
                return list[0];

            return null;
        }

        public WxUserItem GetOneOrEmptyPatron(string weixinId, string libCode,string readerBarcode)
        {
            // 先查到weixinId+libCode+readerBarcode唯一的记录
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("libCode", libCode)
                & Builders<WxUserItem>.Filter.Eq("readerBarcode", readerBarcode);
            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();
            if (list.Count >= 1)
                return list[0];

            // 未找到查weixinId+libCode，readerBarcoe为空的记录
            filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("libCode", libCode)
                & Builders<WxUserItem>.Filter.Eq("readerBarcode", "");
            list = this.wxUserCollection.Find(filter).ToList();
            if (list.Count >= 1)
                return list[0];


            return null;
        }

        public WxUserItem GetActive(string weixinId)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("isActive", 1);

            List<WxUserItem> list= this.wxUserCollection.Find(filter).ToList();//.ToListAsync().Result;
            if (list.Count > 1)
                throw new Exception("程序异常：微信号活动读者数量有" +list.Count+"个");

            if (list.Count == 1)
                return list[0];

            return null;
        }

        public List<WxUserItem> GetByWeixinId(string weixinId)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId);

            return this.wxUserCollection.Find(filter).ToList();//.ToListAsync().Result;
        }

        public WxUserItem GetOneByWeixinId(string weixinId)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId);

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



        public WxUserItem Add(WxUserItem item)
        {
            //item.CreateTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

            this.wxUserCollection.InsertOne(item);

            return item;
        }

        // 更新
        public long Update(WxUserItem item)
        {
            IMongoCollection<WxUserItem> collection = this.wxUserCollection;

            var filter = Builders<WxUserItem>.Filter.Eq("id", item.id);
            var update = Builders<WxUserItem>.Update
                .Set("weixinId", item.weixinId)
                .Set("readerBarcode", item.readerBarcode)
                .Set("readerName", item.readerName)
                .Set("libCode", item.libCode)
                .Set("libUserName", item.libUserName)
                .Set("createTime", item.createTime)
                .Set("updateTime", item.updateTime)
                .Set("xml", item.xml)
                .Set("refID", item.refID)
                .Set("isActive", item.isActive);

            UpdateResult ret = collection.UpdateOne(filter, update);
            return ret.ModifiedCount;
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="item"></param>
        public void Delete(String id)
        {
            if (string.IsNullOrEmpty(id) == true || id=="null")
                return;

            

            IMongoCollection<WxUserItem> collection = this.wxUserCollection;
            var filter = Builders<WxUserItem>.Filter.Eq("id", id);

            // 检查一下是否被删除读者是否为默认读者，如果是，把自动将默认值设了第一个读者上。
            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();
            string weixinId = "";
            if (list.Count > 0)
            {
                weixinId = list[0].weixinId;
            }

            // 先删除
            collection.DeleteOne(filter);

            // 自动将第一个设为默认的
            WxUserItem newUserItem = this.GetOneByWeixinId(weixinId);
            if (newUserItem != null)
                this.SetActive(newUserItem);
        }

        public void SetActive(WxUserItem item)
        {
            this.SetActive(item.weixinId, item.id);
        }

        public void SetActive(string weixinId,string id)
        {
            if (string.IsNullOrEmpty(weixinId) == true || weixinId=="null")
                return;

            if (string.IsNullOrEmpty(weixinId) == true || id=="null")
                return;

            IMongoCollection<WxUserItem> collection = this.wxUserCollection;

            // 先将该微信用户的所有绑定读者都设为非活动
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId);
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

        /// <summary>
        /// 删除绑定
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="readerBarcode"></param>
        public long Delete(String weixinId, string readerBarcode,string libCode)
        {
            IMongoCollection<WxUserItem> collection = this.wxUserCollection;
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("readerBarcode", readerBarcode)
                & Builders<WxUserItem>.Filter.Eq("libCode", libCode);
            DeleteResult ret = collection.DeleteOne(filter);

            return ret.DeletedCount;
        }



    }
    public class WxUserItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; private set; }

        public string weixinId { get; set; } // 绑定必备
        public string readerBarcode { get; set; }
        public string readerName { get; set; }

        public string libCode { get; set; } // 绑定必备
        public string libUserName { get; set; }// 绑定必备
        

        public string createTime { get; set; } // 创建时间
        public string updateTime { get; set; } // 更校报时间


        public string xml { get; set; }
        public string refID { get; set; }

        public int isActive = 0;


        // 绑定必备
        public string prefix { get; set; }  //必须设为属性，才能在前端传值。
        public string word  { get; set; }
        public string password  { get; set; }

        public string fullWord { get; set; } // 服务器用fullWord将strPrefix:strWord存在一起
    }


}
