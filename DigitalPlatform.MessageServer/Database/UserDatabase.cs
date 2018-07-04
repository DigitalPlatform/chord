﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

using DigitalPlatform.Text;

namespace DigitalPlatform.MessageServer
{
    /// <summary>
    /// 用户 数据库
    /// </summary>
    public class UserDatabase : MongoDatabase<UserItem>
    {
#if NO
        string _userDatabaseName = "";

        IMongoCollection<UserItem> _userCollection = null;

        // 初始化
        // parameters:
        public async void Open(
            MongoClient mongoClient,
            // string strMongoDbConnStr,
            string strInstancePrefix)
        {
            //if (string.IsNullOrEmpty(strMongoDbConnStr) == true)
            //    throw new ArgumentException("strMongoDbConnStr 参数值不应为空");

            if (string.IsNullOrEmpty(strInstancePrefix) == false)
                strInstancePrefix = strInstancePrefix + "_";

            _userDatabaseName = strInstancePrefix + "user";


            {
                var db = mongoClient.GetDatabase(this._userDatabaseName);

                _userCollection = db.GetCollection<UserItem>("data");

                bool bExist = false;
                // collection.Indexes.ListAsync().Result.ToListAsync().Result 
                var indexes = _userCollection.Indexes.ListAsync().Result.ToListAsync().Result;
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
                    await CreateIndex();
                }
            }
        }

#endif

        public override async Task CreateIndex()
        {
            if (_collection == null)
                throw new Exception("_collection 尚未初始化");

            // FieldDefinition<UserItem> field = "OperTime";
            await _collection.Indexes.CreateOneAsync(
                Builders<UserItem>.IndexKeys.Ascending("userName"),
                new CreateIndexOptions() { Unique = true });
            await _collection.Indexes.CreateOneAsync(
    Builders<UserItem>.IndexKeys.Ascending("groups"),
    new CreateIndexOptions() { Unique = false });

#if NO
                .CreateIndex(new IndexKeysBuilder().Ascending("OperTime"),
    IndexOptions.SetUnique(false));
#endif
        }

#if NO
        // 清除集合内的全部内容
        public async Task Clear()
        {
            if (_userCollection == null)
            {
                throw new Exception("访问日志 mongodb 集合尚未初始化");
            }

            // https://docs.mongodb.org/getting-started/csharp/remove/
            var filter = new BsonDocument();
            await _userCollection.DeleteManyAsync(filter);
            await CreateIndex();
        }

        public IMongoCollection<UserItem> LogCollection
        {
            get
            {
                return this._userCollection;
            }
        }
#endif

        // 根据用户 ID 检索用户
        public async Task<List<UserItem>> GetUsersByID(string id,
            int start,
            int count)
        {
            IMongoCollection<UserItem> collection = this._collection;

            List<UserItem> results = new List<UserItem>();

            var filter = Builders<UserItem>.Filter.Eq("id", id);
            var index = 0;
            using (var cursor = await collection.FindAsync(
                id == "*" ? new BsonDocument() : filter
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

        // 根据用户名检索用户
        public async Task<List<UserItem>> GetUsersByName(string userName,
            int start,
            int count)
        {
            IMongoCollection<UserItem> collection = this._collection;

            List<UserItem> results = new List<UserItem>();

            var filter = Builders<UserItem>.Filter.Eq("userName", userName);
            var index = 0;
            using (var cursor = await collection.FindAsync(
                userName == "*" ? new BsonDocument() : filter
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

        // 检索属于指定群组的用户
        // https://docs.mongodb.org/manual/tutorial/query-documents/#arrays
        public async Task<List<UserItem>> GetUsersByGroup(string groupName,
    int start,
    int count)
        {
            IMongoCollection<UserItem> collection = this._collection;

            List<UserItem> results = new List<UserItem>();

            var filter = Builders<UserItem>.Filter.Eq("groups", groupName);
            var index = 0;
            using (var cursor = await collection.FindAsync(
                groupName == "*" ? new BsonDocument() : filter
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

        // parameters:
        //      item    要加入的读者信息事项。注意，item.password 中是明码，在加入数据库时，会自动被变为 Hash 码形态
        public async Task Add(UserItem item)
        {
            // 检查 item
            if (string.IsNullOrEmpty(item.userName) == true)
                throw new Exception("用户名不能为空");

            IMongoCollection<UserItem> collection = this._collection;

            item.password = Cryptography.GetSHA1(item.password);
            await collection.InsertOneAsync(item);
        }

        // 更新 password 以外的全部字段
        public async Task Update(UserItem item)
        {
            // 检查 item
            if (string.IsNullOrEmpty(item.userName) == true)
                throw new Exception("用户名不能为空");

            IMongoCollection<UserItem> collection = this._collection;

            // var filter = Builders<UserItem>.Filter.Eq("id", item.id);
            var filter = Builders<UserItem>.Filter.Eq("userName", item.userName);
            var update = Builders<UserItem>.Update
                .Set("userName", item.userName)
                .Set("rights", item.rights)
                .Set("duty", item.duty)
                .Set("department", item.department)
                .Set("tel", item.tel)
                .Set("comment", item.comment)
                .Set("groups", item.groups)
                .Set("binding", item.binding);

            await collection.UpdateOneAsync(filter, update);
        }

        // 只更新 password
        // parameters:
        //      item    读者信息事项。注意，item.password 中是明码，在用于更新密码时，会自动被变为 Hash 码形态
        public async Task UpdatePassword(UserItem item)
        {
            IMongoCollection<UserItem> collection = this._collection;

            item.password = Cryptography.GetSHA1(item.password);

            // var filter = Builders<UserItem>.Filter.Eq("id", item.id);
            var filter = Builders<UserItem>.Filter.Eq("userName", item.userName);
            var update = Builders<UserItem>.Update
                .Set("password", item.password);

            await collection.UpdateOneAsync(filter, update);
        }

        public async Task Delete(UserItem item)
        {
            IMongoCollection<UserItem> collection = this._collection;

            // var filter = Builders<UserItem>.Filter.Eq("id", item.id);
            var filter = Builders<UserItem>.Filter.Eq("userName", item.userName);

            await collection.DeleteOneAsync(filter);
        }
#if NO
        // parameters:
        //      maxItemCount    最大事项数。如果为 -1 表示不限制
        public bool Add(string operation,
            string path,
            long size,
            string mime,
            string clientAddress,
            long initial_hitcount,
            string operator_param,
            DateTime opertime,
            long maxItemCount)
        {
            IMongoCollection<AccessLogItem> collection = this.LogCollection;
            if (collection == null)
                return false;

            // 限制最大事项数
            {
                string date = GetToday();
                long newValue = _itemCount.GetValue(date, 1);
                if (maxItemCount != -1 && newValue > maxItemCount)
                {
                    _itemCount.GetValue(date, -1);
                    return false;
                }
            }

            var query = new QueryDocument("Path", path);
            query.Add("Operation", operation)
                .Add("Size", size)
                .Add("MIME", mime)
                .Add("ClientAddress", clientAddress)
                .Add("HitCount", initial_hitcount)
                .Add("Operator", operator_param)
                .Add("OperTime", opertime);
#if NO
            var update = Update.Inc("HitCount", 1);
            collection.Update(
    query,
    update,
    UpdateFlags.Upsert);
#endif
            collection.Insert(query);
            return true;
        }

        public IEnumerable<AccessLogItem> Find(string date, int start)
        {
            MongoCollection<AccessLogItem> collection = this.LogCollection;
            if (collection == null)
                return null;

            DateTime start_time = DateTimeUtil.Long8ToDateTime(date);
            DateTime end_time = start_time.AddDays(1);

            var query = Query.And(Query.GTE("OperTime", start_time),
                Query.LT("OperTime", end_time));
            return collection.Find(query).Skip(start);
        }

        // 获得一个日期的事项个数
        // return:
        //      -1  集合不存在
        //      >=0 数量
        public int GetItemCount(string date)
        {
            MongoCollection<AccessLogItem> collection = this.LogCollection;
            if (collection == null)
                return -1;

            DateTime start_time = DateTimeUtil.Long8ToDateTime(date);
            DateTime end_time = start_time.AddDays(1);

            var query = Query.And(Query.GTE("OperTime", start_time),
                Query.LT("OperTime", end_time));

            var keyFunction = (BsonJavaScript)@"{}";

            var document = new BsonDocument("count", 0);
            var result = collection.Group(
                query,
                keyFunction,
                document,
                new BsonJavaScript("function(doc, out){ out.count++; }"),
                null
            ).ToArray();

            foreach(BsonDocument doc in result)
            {
                return doc.GetValue("count", 0).ToInt32();
            }

            return 0;
        }
#endif
    }

    public class UserItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; private set; }

        public string userName { get; set; } // 用户名
        public string password { get; set; }  // 密码
        public string rights { get; set; } // 权限
        public string duty { get; set; }    // 义务
        public string department { get; set; } // 部门名称
        public string tel { get; set; }  // 电话号码
        public string comment { get; set; }  // 注释

        public string [] groups { get; set; }  // 所加入的群组

        public string binding { get; set; } // 绑定信息
#if NO
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime OperTime { get; set; } // 操作时间
#endif
    }

}
