using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using DigitalPlatform.Text;

namespace DigitalPlatform.MessageServer
{
    // 消息 数据库
    public class MessageDatabase : MongoDatabase<MessageItem>
    {
        public override async Task CreateIndex()
        {
            if (_collection == null)
                throw new Exception("_collection 尚未初始化");

            await _collection.Indexes.CreateOneAsync(
                Builders<MessageItem>.IndexKeys.Ascending("publishTime"),
                new CreateIndexOptions() { Unique = false });
            await _collection.Indexes.CreateOneAsync(
    Builders<MessageItem>.IndexKeys.Ascending("creator"),
    new CreateIndexOptions() { Unique = false });
            await _collection.Indexes.CreateOneAsync(
Builders<MessageItem>.IndexKeys.Ascending("group"),
new CreateIndexOptions() { Unique = false });
            await _collection.Indexes.CreateOneAsync(
Builders<MessageItem>.IndexKeys.Ascending("thread"),
new CreateIndexOptions() { Unique = false });
        }

        // return:
        //      true    表示后面要继续处理
        //      false 表示后面要中断处理
        public delegate bool Delegate_outputMessage(long totalCount, MessageItem item);

        FilterDefinition<MessageItem> BuildQuery(string groupName,
            string timeRange)
        {
            string strStart = "";
            string strEnd = "";
            StringUtil.ParseTwoPart(timeRange, "~", out strStart, out strEnd);
            DateTime startTime;
            DateTime endTime;
            try
            {
                startTime = string.IsNullOrEmpty(strStart) ? new DateTime(0) : DateTime.Parse(strStart);
                endTime = string.IsNullOrEmpty(strEnd) ? new DateTime(0) : DateTime.Parse(strEnd);
            }
            catch (Exception)
            {
                throw new ArgumentException("时间范围字符串 '" + timeRange + "' 不合法", "timeRange");
            }

            FilterDefinition<MessageItem> time_filter = null;
            if (startTime == new DateTime(0) && endTime == new DateTime(0))
                time_filter = null;  // Builders<MessageItem>.Filter.Gte("publishTime", startTime);
            else if (startTime == new DateTime(0))
                time_filter = Builders<MessageItem>.Filter.Lt("publishTime", endTime);
            else if (endTime == new DateTime(0))
                time_filter = Builders<MessageItem>.Filter.Gte("publishTime", startTime);
            else
            {
                time_filter = Builders<MessageItem>.Filter.And(
Builders<MessageItem>.Filter.Gte("publishTime", startTime),
Builders<MessageItem>.Filter.Lt("publishTime", endTime));
            }

            var name_filter = Builders<MessageItem>.Filter.Eq("group", groupName);

            if (time_filter == null)
                return name_filter;

            return time_filter = Builders<MessageItem>.Filter.And(time_filter,
                name_filter);
        }

        // parameters:
        //      timeRange   时间范围
        public async Task GetMessages(string groupName,
            string timeRange,
int start,
int count,
            Delegate_outputMessage proc)
        {
            IMongoCollection<MessageItem> collection = this._collection;

            // List<MessageItem> results = new List<MessageItem>();
            FilterDefinition<MessageItem> filter = BuildQuery(groupName, timeRange);
#if NO
            if (string.IsNullOrEmpty(groupName))
            {
                filter = Builders<MessageItem>.Filter.Or(
                    Builders<MessageItem>.Filter.Eq("group", ""),
                    Builders<MessageItem>.Filter.Eq("group", (string)null));
            }
            else
#endif
            // filter = Builders<MessageItem>.Filter.Eq("group", groupName);

            var index = 0;
            using (var cursor = await collection.FindAsync(
                groupName == "*" ? new BsonDocument() : filter
                ))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    long totalCount = batch.Count<MessageItem>();
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

        public async Task<List<MessageItem>> GetMessages(string groupName,
    int start,
    int count)
        {
            IMongoCollection<MessageItem> collection = this._collection;

            List<MessageItem> results = new List<MessageItem>();

            var filter = Builders<MessageItem>.Filter.Eq("group", groupName);
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

        public async Task<List<MessageItem>> GetMessageByID(string id,
    int start = 0,
    int count = -1)
        {
            IMongoCollection<MessageItem> collection = this._collection;

            List<MessageItem> results = new List<MessageItem>();

            var filter = Builders<MessageItem>.Filter.Eq("id", id);
            var index = 0;
            using (var cursor = await collection.FindAsync(filter))
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
        //      item    要加入的消息事项
        public async Task Add(MessageItem item)
        {
            // 检查 item
            if (string.IsNullOrEmpty(item.creator) == true)
                throw new Exception("creator 不能为空");

            // 规范化数据

            // group 的空实际上代表一个群组
            if (item.group == null)
                item.group = "";

            IMongoCollection<MessageItem> collection = this._collection;

            //item.publishTime = DateTime.Now;
            //item.expireTime = new DateTime(0); // 表示永远不失效

            await collection.InsertOneAsync(item);
        }

        // 更新 id 以外的全部字段
        public async Task Update(MessageItem item)
        {
            // 检查 item
            if (string.IsNullOrEmpty(item.id) == true)
                throw new Exception("id 不能为空");

            IMongoCollection<MessageItem> collection = this._collection;

            // var filter = Builders<UserItem>.Filter.Eq("id", item.id);
            var filter = Builders<MessageItem>.Filter.Eq("id", item.id);
            var update = Builders<MessageItem>.Update
                .Set("group", item.group)
                .Set("creator", item.creator)
                .Set("userName", item.userName)
                .Set("data", item.data)
                .Set("format", item.format)
                .Set("type", item.type)
                .Set("thread", item.thread);

            await collection.UpdateOneAsync(filter, update);
        }

        // 根据一个字段的特征删除匹配的事项
        public async Task Delete(string field, string value)
        {
            IMongoCollection<MessageItem> collection = this._collection;

            // var filter = Builders<UserItem>.Filter.Eq("id", item.id);
            var filter = Builders<MessageItem>.Filter.Eq(field, value);

            await collection.DeleteOneAsync(filter);
        }

        public async Task DeleteByID(string id)
        {
            // 检查 id
            if (string.IsNullOrEmpty(id) == true)
                throw new ArgumentException("id 不能为空");

            await Delete("id", id);
        }
    }

    public class MessageItem
    {
        public void SetID(string id)
        {
            this.id = id;
        }

        [BsonId]    // 允许 GUID
        public string id { get; private set; }  // 消息的 id

        public string group { get; set; }   // 组名 或 组id。消息所从属的组
        public string creator { get; set; } // 创建消息的人的id
        public string userName { get; set; } // 创建消息的人的用户名
        public string data { get; set; }  // 消息数据体
        public string format { get; set; } // 消息格式。格式是从存储格式角度来说的
        public string type { get; set; }    // 消息类型。类型是从用途角度来说的
        public string thread { get; set; }    // 消息所从属的话题线索

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime publishTime { get; set; } // 消息发布时间

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime expireTime { get; set; } // 消息失效时间

        // TODO: 消息的历次修改者和时间。也可以不采用这种数据结构，而是在修改后在原时间重新写入一条修改后消息，并注明前后沿革关系
    }

}
