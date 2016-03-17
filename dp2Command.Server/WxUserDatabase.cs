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

        //只返回一个
        public WxUserItem GetOneByWeixinId(string weixinId)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId);

            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();//.ToListAsync().Result;
            if (list.Count > 0)
                return list[0];

            return null;
        }

        public List<WxUserItem> GetByWeixinId(string weixinId)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId);

            return this.wxUserCollection.Find(filter).ToList();//.ToListAsync().Result;
        }

        /// <summary>
        /// 查找所有用户
        /// </summary>
        /// <returns></returns>
        public List<WxUserItem> GetUsers()
        {
            IFindFluent<WxUserItem,WxUserItem> f= this.wxUserCollection.Find(new BsonDocument());
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

            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", item.weixinId);
            var update = Builders<WxUserItem>.Update
                .Set("weixinId", item.weixinId)
                .Set("readerBarcode", item.readerBarcode)
                .Set("readerName", item.readerName)
                .Set("libCode", item.libCode)
                .Set("createTime", item.createTime);

            UpdateResult ret = collection.UpdateOne(filter, update);
            return ret.ModifiedCount;
        }

        /*
        public async Task<long> UpdateLibCode(WxUserItem item)
        {
            item.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);

            IMongoCollection<WxUserItem> collection = this.wxUserCollection;

            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", item.weixinId);
            var update = Builders<WxUserItem>.Update
                //.Set("weixinId", item.weixinId)
                //.Set("readerBarcode", item.readerBarcode)
                //.Set("readerBarcode", item.readerName)
                .Set("libCode", item.libCode)
                .Set("createTime", item.createTime);

            UpdateResult ret = await collection.UpdateOneAsync(filter, update);
            return ret.ModifiedCount;
        }
        */

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="item"></param>
        public void Delete(String id)
        {
            IMongoCollection<WxUserItem> collection = this.wxUserCollection;

            var filter = Builders<WxUserItem>.Filter.Eq("id", id);

            collection.DeleteOne(filter);
        }

        /// <summary>
        /// 删除绑定
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="readerBarcode"></param>
        public long Delete(String weixinId, string readerBarcode)
        {
            IMongoCollection<WxUserItem> collection = this.wxUserCollection;
          
            var builder = Builders<WxUserItem>.Filter;
            var filter = builder.Eq("weixinId", weixinId) & builder.Eq("readerBarcode", readerBarcode);

            DeleteResult ret = collection.DeleteOne(filter);

            return ret.DeletedCount;
        }



    }
    public class WxUserItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; private set; }

        public string weixinId { get; set; }
        public string readerBarcode { get; set; }

        public string readerName { get; set; }

        public string libCode { get; set; }

        public string createTime { get; set; } // 操作时间

    }


}
