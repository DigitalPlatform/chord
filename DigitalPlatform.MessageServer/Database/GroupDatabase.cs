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

    }

    public class GroupItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; private set; }  // 组的 id

        public string name { get; set; }   // 组名。表意的名称
        public string creator { get; set; } // 创建组的人。用户名或 id
        public string[] manager { get; set; }   // 管理员
        public string data { get; set; }  // 消息数据体
        public string type { get; set; }    // 组类型。类型是从用途角度来说的

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime createTime { get; set; } // 创建时间

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime expireTime { get; set; } // 组失效时间
    }

}
