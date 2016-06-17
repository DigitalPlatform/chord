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
    public sealed class LibDatabase
    {
        // 饿汉模式
        private static readonly LibDatabase _db = new LibDatabase();
        public static LibDatabase Current
        {
            get
            {
                return _db;
            }
        }

        // 成员变量
        MongoClient _mongoClient = null;
        IMongoDatabase _database = null;
        string _libDbName = "";
        IMongoCollection<LibItem> _libCollection = null;
        public IMongoCollection<LibItem> LibCollection
        {
            get
            {
                return this._libCollection;
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
            _libDbName = strInstancePrefix + "lib";

            this._mongoClient = new MongoClient(strMongoDbConnStr);
            this._database = this._mongoClient.GetDatabase(this._libDbName);

            //图书馆点对点账号
            _libCollection = this._database.GetCollection<LibItem>("item");

            //todo
            //CreateIndex();
            
        }

        // 创建索引
        public  void CreateIndex()
        {
            var options = new CreateIndexOptions() { Unique = true };
            _libCollection.Indexes.CreateOneAsync(
                Builders<LibItem>.IndexKeys.Ascending("libCode"),
                options);
        }

        public LibItem GetLibById(string id)
        {
            var filter = Builders<LibItem>.Filter.Eq("id", id);

            var list = this.LibCollection.Find(new BsonDocument("id", id)).ToListAsync().Result;
            if (list.Count > 0)
                return list[0];

            return null;
        }
        public LibItem GetLibByLibCode(string libCode)
        {
            var filter = Builders<LibItem>.Filter.Eq("libCode", libCode);
            List<LibItem> list = this.LibCollection.Find(filter).ToList();
            if (list.Count > 0)
                return list[0];

            return null;
        }

        public List<LibItem> GetLibs()
        {
            return this.LibCollection.Find(new BsonDocument()).ToListAsync().Result;
        }

        public LibItem Add(LibItem item)
        {
            item.OperTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

            this.LibCollection.InsertOne(item);
            return item;
        }

        // 更新
        public long Update(string id,LibItem item)
        {
            var filter = Builders<LibItem>.Filter.Eq("id", id);
            var update = Builders<LibItem>.Update
                .Set("libCode", item.libCode)
                .Set("libName", item.libName)
                .Set("libUserName", item.libUserName) 
                .Set("libContactPhone", item.libContactPhone)

                .Set("wxUserName", item.wxUserName)
                .Set("wxPassword", item.wxPassword)
                .Set("wxContactPhone", item.wxContactPhone) 

                .Set("comment", item.comment)
                .Set("OperTime", item.OperTime);

            UpdateResult ret = this.LibCollection.UpdateOneAsync(filter, update).Result;
            return ret.ModifiedCount;
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="item"></param>
        public void Delete(String id)
        {
            var filter = Builders<LibItem>.Filter.Eq("id", id);
            this.LibCollection.DeleteOne(filter);
        }

    }

    public class LibItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; private set; }

        public string libCode { get; set; }
        public string libName { get; set; }
        public string libUserName { get; set; }
        public string libContactPhone { get; set; } // 图书馆联系人电话 jane 2016-6-17

        // 2016-6-17 jane 本方账户的信息
        public string wxUserName { get; set; } //微信端本方用户名
        public string wxPassword { get; set; }    //本方密码
        public string wxContactPhone { get; set; }    //本方联系人手机号，用于将来的找回密码
        public string wxPasswordView
        {
            get
            {
                return "*".PadRight(this.wxPassword.Length, '*');
            }
        }

        public string comment { get; set; }  // 注释
        public string OperTime { get; set; } // 操作时间


    }
    /*
    public class LibraryRespository
    {
        private static LibraryRespository repo = new LibraryRespository();

        public static LibraryRespository Current
        {
            get
            {
                return repo;
            }
        }

        private List<LibItem> data = new List<LibItem> {
            new LibItem {
               id="001", libCode = "lib1", libName = "图书馆1", libP2PAccount = "a1"},
            new LibItem {
                id="002",libCode = "lib2", libName = "图书馆2", libP2PAccount = "a2"},
            new LibItem {
                id="003",libCode = "lib3", libName = "图书馆3", libP2PAccount = "a3"},
        };

        public IEnumerable<LibItem> GetAll()
        {
            return data;
        }

        public LibItem Get(string libId)
        {
            return data.Where(r => r.id == libId).FirstOrDefault();
        }

        public LibItem Add(LibItem item)
        {
            // id取guid todo
            item.id = Guid.NewGuid().ToString();

            data.Add(item);
            return item;
        }

        public void Remove(string libId)
        {
            LibItem item = Get(libId);
            if (item != null)
            {
                data.Remove(item);
            }
        }

        public bool Update(LibItem item)
        {
            LibItem storedItem = Get(item.libCode);
            if (storedItem != null)
            {
                storedItem.libName = item.libName;
                storedItem.libP2PAccount = item.libP2PAccount;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    */
}
