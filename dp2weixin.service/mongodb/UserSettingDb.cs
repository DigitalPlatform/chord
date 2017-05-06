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
    public sealed class UserSettingDb
    {
        /// <summary>
        /// 单一静态实例,饿汉模式
        /// </summary>
        private static readonly UserSettingDb _db = new UserSettingDb();
        public static UserSettingDb Current
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
        IMongoCollection<UserSettingItem> _collection = null;
        public IMongoCollection<UserSettingItem> settingCollection
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
            _dbName = strInstancePrefix + "setting";

            this._mongoClient = new MongoClient(strMongoDbConnStr);
            this._database = this._mongoClient.GetDatabase(this._dbName);

            //collection名称为item
            this._collection = this._database.GetCollection<UserSettingItem>("item");

            // todo 创建索引            
        }

        public List<UserSettingItem> GetAll()
        {
            List<UserSettingItem> list = this.settingCollection.Find(new BsonDocument()).ToListAsync().Result;
            return list;
        }

        /// <summary>
        /// 获取读者账户,针对一个图书馆可绑定多个读者账户
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libCode"></param>
        /// <param name="readerBarcode"></param>
        /// <returns></returns>
        public UserSettingItem GetByWeixinId(string weixinId)
        {
            var filter = Builders<UserSettingItem>.Filter.Eq("weixinId", weixinId);

            List<UserSettingItem> list = this.settingCollection.Find(filter).ToList();
            if (list.Count >= 1)
                return list[0];

            return null;
        }

        // 获得了设置了指定图书馆的项
        public List<UserSettingItem> GetByLibId(string libId)
        {
            var filter = Builders<UserSettingItem>.Filter.Eq("libId", libId);

            List<UserSettingItem> list = this.settingCollection.Find(filter).ToList();
            return list;
        }

        public void SetLib(UserSettingItem inputItem)
        {
            UserSettingItem item = this.GetByWeixinId(inputItem.weixinId);
            if (item == null)
            {
                item.libraryCode = "";
                this.Add(inputItem);
            }
            else
            {
                this.Update(inputItem);
            }
        }
        
        /// <summary>
        /// 新增微信用户
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void Add(UserSettingItem item)
        {
            this.settingCollection.InsertOne(item);

        }

        /// <summary>
        /// 更新微信用户
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public long Update(UserSettingItem item)
        {
            var filter = Builders<UserSettingItem>.Filter.Eq("weixinId", item.weixinId);
            var update = Builders<UserSettingItem>.Update
                .Set("libId", item.libId)
                .Set("showPhoto", item.showPhoto)
                .Set("showCover", item.showCover)
                .Set("xml", item.xml)
                .Set("patronRefID", item.patronRefID)
                .Set("libraryCode", "")
                ;

            UpdateResult ret = this.settingCollection.UpdateOne(filter, update);
            return ret.ModifiedCount;
        
        }

        // 更新选择的分馆
        public long UpdateLibId(string weixinId, string libId, string libraryCode)
        {
            var filter = Builders<UserSettingItem>.Filter.Eq("weixinId", weixinId);
            var update = Builders<UserSettingItem>.Update
                .Set("libId", libId)
                .Set("libraryCode", libraryCode)
                ;

            UpdateResult ret = this.settingCollection.UpdateOne(filter, update);
            return ret.ModifiedCount;

        }

        public long UpdateById(UserSettingItem item)
        {
            var filter = Builders<UserSettingItem>.Filter.Eq("id", item.id);
            var update = Builders<UserSettingItem>.Update
                .Set("libId", item.libId)
                .Set("weixinId", item.weixinId)
                .Set("showPhoto", item.showPhoto)
                .Set("showCover", item.showCover)
                .Set("xml", item.xml)
                .Set("patronRefID", item.patronRefID)
                ;

            UpdateResult ret = this.settingCollection.UpdateOne(filter, update);
            return ret.ModifiedCount;

        }

        //// 更新一条记录的tracing值
        //public long UpdateTracing(string id,int tracing)
        //{
        //    var filter = Builders<UserSettingItem>.Filter.Eq("id",id);
        //    var update = Builders<UserSettingItem>.Update
        //        .Set("tracing", tracing)
        //        ;

        //    UpdateResult ret = this.settingCollection.UpdateOne(filter, update);
        //    return ret.ModifiedCount;

        //}

        public static string getBookSubject(string xml)
        {
            string subject = "";
            if (string.IsNullOrEmpty(xml) == false)
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(xml);
                XmlNode root = dom.DocumentElement;
                XmlNode subjectNode = root.SelectSingleNode("subject");
                if (subjectNode != null)
                {
                    subject = DomUtil.GetAttr(subjectNode, "book");
                }
            }
            return subject;
        }

    }
    public class UserSettingItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; private set; }

        public string weixinId { get; set; } 

        public string libId { get; set; }

        //20170507 分馆代码
        public string libraryCode { get; set; } 

        public int showPhoto { get; set; }

        public int showCover { get; set; }

        public string xml { get; set; }

        public string patronRefID { get; set; }


    }


}
