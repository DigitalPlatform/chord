using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading;

namespace DigitalPlatform.MessageServer
{
    /// <summary>
    /// Mongo 数据库的基础类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MongoDatabase<T>
    {
        internal string _pureDatabaseName = ""; // 数据库名。实际上要加上 prefix 部分才构成真正使用的数据库名
        internal IMongoCollection<T> _collection = null;
        internal string _collectionName = "collection";

        // 数据库是否已经启用
        public bool Enabled
        {
            get
            {
                return this._collection != null;
            }
        }

        public string CollectionName
        {
            get
            {
                return _collectionName;
            }
            set
            {
                _collectionName = value;
            }
        }

        // 初始化
        // 默认的初始化函数，只初始化一个 collection
        // parameters:
        //      strDatabaseName 数据库名。实际上要加上 prefix 部分才构成真正使用的数据库名
        public virtual void Open(MongoClient mongoClient,
            string instancePrefix,
            string pureDatabaseName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(instancePrefix) == false)
                instancePrefix = instancePrefix + "_";

            if (string.IsNullOrEmpty(pureDatabaseName) == true)
                throw new ArgumentException("strDatabaseName 参数不应为空", "strDatabaseName");

            // _userDatabaseName = strInstancePrefix + "user";
            this._pureDatabaseName = pureDatabaseName;

            string databaseName = instancePrefix + this._pureDatabaseName;

            {
                var db = mongoClient.GetDatabase(databaseName);

                _collection = db.GetCollection<T>("data");

                bool bExist = false;
                // collection.Indexes.ListAsync().Result.ToListAsync().Result 
                var indexes = _collection.Indexes.ListAsync(cancellationToken).Result.ToListAsync().Result;
                foreach (BsonDocument doc in indexes)
                {

                }

                // _logCollection.DropAllIndexes();
                if (bExist == false)
                {
#if NO
                    _logCollection.CreateIndex(new IndexKeysBuilder().Ascending("OperTime"),
                        IndexOptions.SetUnique(false));
#endif
                    CreateIndex().Wait(cancellationToken);
                }
            }
        }

        public virtual async Task CreateIndex()
        {
            await Task.Run(() => { }).ConfigureAwait(false);
        }

        // 清除集合内的全部内容
        public virtual async Task Clear()
        {
            if (_collection == null)
                throw new Exception("_collection 尚未初始化");

            // https://docs.mongodb.org/getting-started/csharp/remove/
            var filter = new BsonDocument();
            await _collection.DeleteManyAsync(filter).ConfigureAwait(false);
            await CreateIndex().ConfigureAwait(false);
        }

    }
}
