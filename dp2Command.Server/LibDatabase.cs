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
    public sealed class LibDatabase
    {
        // 饿汉模式
        private static readonly LibDatabase repo = new LibDatabase();
        public static LibDatabase Current
        {
            get
            {
                return repo;
            }
        }


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
        public void Open(
            string strMongoDbConnStr,
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
            if (_libCollection == null)
            {
                throw new Exception("访问日志 mongodb 集合尚未初始化");
            }

            // https://docs.mongodb.org/getting-started/csharp/remove/
            var filter = new BsonDocument();
            await _libCollection.DeleteManyAsync(filter);
            //await CreateIndex();
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

        /// <summary>
        /// 根据libCode获得图书馆对象
        /// </summary>
        /// <param name="libCode">*获取全部</param>
        /// <param name="start">0</param>
        /// <param name="count">-1</param>
        /// <returns></returns>
        public async Task<List<LibItem>> GetLibs(string libCode,
            int start,
            int count)
        {
            IMongoCollection<LibItem> collection = this.LibCollection;

            List<LibItem> results = new List<LibItem>();

            var filter = Builders<LibItem>.Filter.Eq("libCode", libCode);
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

        public LibItem Add(LibItem item)
        {
            item.OperTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

            this.LibCollection.InsertOne(item);

            return item;
        }

        // 更新
        public async Task<long> Update(LibItem item)
        {
            IMongoCollection<LibItem> collection = this.LibCollection;

            var filter = Builders<LibItem>.Filter.Eq("id", item.id);
            //var filter = Builders<LibItem>.Filter.Eq("libCode", item.libCode);
            var update = Builders<LibItem>.Update
                .Set("libCode", item.libCode)
                .Set("libName", item.libName)
                .Set("libP2PAccount", item.libUserName)
                .Set("comment", item.comment)
                .Set("OperTime", item.OperTime);

            UpdateResult ret = await collection.UpdateOneAsync(filter, update);
            return ret.ModifiedCount;
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="item"></param>
        public void Delete(String id)
        {
            IMongoCollection<LibItem> collection = this.LibCollection;

            var filter = Builders<LibItem>.Filter.Eq("id", id);
            //var filter = Builders<LibItem>.Filter.Eq("libCode", item.libCode);

            collection.DeleteOne(filter);
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
