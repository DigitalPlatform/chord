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

            /*
            // todo 创建索引            
            bool bExist = false;
            var indexes = _libCollection.Indexes.ListAsync().Result.ToListAsync().Result;
            foreach (BsonDocument doc in indexes)
            {
            }
            // _logCollection.DropAllIndexes();
            if (bExist == false)
            {
                //await CreateIndex();
            }
             */
        }

        // 创建索引
        public async Task CreateIndex()
        {
            /*
            var options = new CreateIndexOptions() { Unique = true };
            await _libCollection.Indexes.CreateOneAsync(
                Builders<LibItem>.IndexKeys.Ascending("libCode"),
                options);
             */
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
            //await CreateIndex();
        }

        public WxUserItem GetLibById(string id)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("id", id);

            var list = this.wxUserCollection.Find(new BsonDocument("id", id)).ToListAsync().Result;
            if (list.Count > 0)
                return list[0];

            return null;
        }

        public List<WxUserItem> GetLibs()
        {
            return this.wxUserCollection.Find(new BsonDocument()).ToListAsync().Result;
        }

        /// <summary>
        /// 根据libCode获得图书馆对象
        /// </summary>
        /// <param name="libCode">*获取全部</param>
        /// <param name="start">0</param>
        /// <param name="count">-1</param>
        /// <returns></returns>
        public async Task<List<WxUserItem>> GetLibs(string libCode,
            int start,
            int count)
        {
            IMongoCollection<WxUserItem> collection = this.wxUserCollection;

            List<WxUserItem> results = new List<WxUserItem>();

            var filter = Builders<WxUserItem>.Filter.Eq("libCode", libCode);
            var index = 0;
            using (var cursor = await collection.FindAsync(
                libCode == "*" ? new BsonDocument() : filter
                ))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        if (count != -1 && index - start >= count)
                            break;
                        if (index >= start)
                            results.Add(document);
                        index++;
                    }
                }
            }
            return results;
        }

        public WxUserItem Add(WxUserItem item)
        {
            item.CreateTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

            this.wxUserCollection.InsertOne(item);

            return item;
        }

        // 更新
        public async Task<long> Update(WxUserItem item)
        {
            IMongoCollection<WxUserItem> collection = this.wxUserCollection;

            var filter = Builders<WxUserItem>.Filter.Eq("id", item.id);
            var update = Builders<WxUserItem>.Update
                .Set("weixinId", item.weixinId)
                .Set("readerBarcode", item.readerBarcode)
                .Set("libCode", item.libCode)
                .Set("CreateTime", item.CreateTime);

            UpdateResult ret = await collection.UpdateOneAsync(filter, update);
            return ret.ModifiedCount;
        }


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

    }
    public class WxUserItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; private set; }

        public string weixinId { get; set; }
        public string readerBarcode { get; set; }
        public string libCode { get; set; }

        public string CreateTime { get; set; } // 操作时间

    }


}
