using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dp2weixin.service
{
    
    /// <summary>
    /// 用户数据库
    /// </summary>
    public sealed class UserMessageDb
    {
        /// <summary>
        /// 单一静态实例,饿汉模式
        /// </summary>
        private static readonly UserMessageDb _db = new UserMessageDb();
        public static UserMessageDb Current
        {
            get
            {
                return _db;
            }
        }

        // 成员变量
        MongoClient _mongoClient = null;
        IMongoDatabase _database = null;
        string _dbName = "";
        IMongoCollection<UserMessageItem> _collection = null;
        public IMongoCollection<UserMessageItem> Collection
        {
            get
            {
                return this._collection;
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
            _dbName = strInstancePrefix + "message";

            this._mongoClient = new MongoClient(strMongoDbConnStr);
            this._database = this._mongoClient.GetDatabase(this._dbName);

            //collection名称为item
            this._collection = this._database.GetCollection<UserMessageItem>("item");

            // todo 创建索引            
        }

        public List<UserMessageItem> GetAll()
        {
            List<UserMessageItem> list = this.Collection.Find(new BsonDocument()).ToListAsync().Result;
            return list;
        }

        /// <summary>
        /// 获取读者账户,针对一个图书馆可绑定多个读者账户
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libCode"></param>
        /// <param name="readerBarcode"></param>
        /// <returns></returns>
        public List<UserMessageItem> GetByUserId(string userId)
        {
            var filter = Builders<UserMessageItem>.Filter.Eq("userId", userId);

            List<UserMessageItem> list = this.Collection.Find(filter).ToList();
            return list;
        }
        
        
        /// <summary>
        /// 新增微信用户
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void Add(UserMessageItem item)
        {
            this.Collection.InsertOne(item);

        }
        
        

    }
    public class UserMessageItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; private set; }

        public string userId { get; set; } 

        public string msgType { get; set; }

        public string xml { get; set; }
    }

}
