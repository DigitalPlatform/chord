using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DigitalPlatform.MessageServer
{
    // 组 数据库
    public class GroupDatabase : MongoDatabase<GroupItem>
    {
        public override async Task CreateIndex()
        {
            if (_collection == null)
                throw new Exception("_collection 尚未初始化");

            await _collection.Indexes.CreateOneAsync(
                Builders<GroupItem>.IndexKeys.Ascending("name"),
                new CreateIndexOptions() { Unique = true });
            await _collection.Indexes.CreateOneAsync(
    Builders<GroupItem>.IndexKeys.Ascending("creator"),
    new CreateIndexOptions() { Unique = false });

        }

        // return:
        //      true    表示后面要继续处理
        //      false 表示后面要中断处理
        public delegate bool Delegate_outputGroup(long totalCount, GroupItem item);

        public async Task GetGroups(string groupName,
int start,
int count,
            Delegate_outputGroup proc)
        {
            IMongoCollection<GroupItem> collection = this._collection;

            FilterDefinition<GroupItem> filter = null;
            filter = Builders<GroupItem>.Filter.Eq("name", groupName);

            var index = 0;
            using (var cursor = await collection.FindAsync(
                groupName == "*" ? new BsonDocument() : filter
                ))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    long totalCount = batch.Count<GroupItem>();
                    foreach (var document in batch)
                    {
                        if (count != -1 && index - start >= count)
                            break;
                        if (index >= start)
                        {
                            if (proc(totalCount, document) == false)
                                return;
                        }
                        index++;
                    }
                    proc(totalCount, null); // 表示结束
                }
            }
        }

        public async Task<List<GroupItem>> GetGroupsByName(string groupName,
int start,
int count)
        {
            IMongoCollection<GroupItem> collection = this._collection;

            FilterDefinition<GroupItem> filter = null;

            filter = Builders<GroupItem>.Filter.Eq("name", groupName);

            List<GroupItem> results = new List<GroupItem>();

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

        public async Task<List<GroupItem>> GetGroupsByID(string id,
int start,
int count)
        {
            IMongoCollection<GroupItem> collection = this._collection;

            FilterDefinition<GroupItem> filter = null;

            filter = Builders<GroupItem>.Filter.Eq("id", id);

            List<GroupItem> results = new List<GroupItem>();

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
    }

    public class GroupItem
    {
        public void SetID(string id)
        {
            this.id = id;
        }

        [BsonId]
        // [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; private set; }  // 组的 id

        public string name { get; set; }   // 组名。表意的名称
        public string creator { get; set; } // 创建组的人。用户名或 id
        public string[] manager { get; set; }   // 管理员
        public string comment { get; set; }  // 注释
        public string type { get; set; }    // 组类型。类型是从用途角度来说的

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime createTime { get; set; } // 创建时间

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime expireTime { get; set; } // 组失效时间
    }

}
