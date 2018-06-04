using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebZ.Server.database
{
    public class ZServerDatabase : MongoDatabase<ZServerItem>
    {
        // 创建索引
        public override async Task CreateIndex()
        {
            if (_collection == null)
                throw new Exception("_collection 尚未初始化");

            // 服务器地址建索引
            await _collection.Indexes.CreateOneAsync(
                Builders<ZServerItem>.IndexKeys.Ascending("hostName"),
                new CreateIndexOptions() { Unique = false });
        }


        // parameters:
        //      item    要加入的站点信息
        public async Task Add(ZServerItem item)
        {
            // 检查 item
            if (string.IsNullOrEmpty(item.userName) == true)
                throw new Exception("用户名不能为空");

            IMongoCollection<ZServerItem> collection = this._collection;
            await collection.InsertOneAsync(item);
        }

        // 更新 password 以外的全部字段
        public async Task Update(ZServerItem item)
        {

            IMongoCollection<ZServerItem> collection = this._collection;

            var filter = Builders<ZServerItem>.Filter.Eq("id", item.id);
            var update = Builders<ZServerItem>.Update
                .Set("hostName", item.hostName)
                .Set("port", item.port)
                .Set("dbNames", item.dbNames)
                .Set("authenticationMethod", item.authenticationMethod)
                .Set("groupID", item.groupID)
                .Set("userName", item.userName)
                .Set("password", item.password)

                .Set("creatorPhone", item.creatorPhone)
                .Set("creatorId", item.creatorId)
                .Set("createTime", item.createTime)
                .Set("state", item.state)
                .Set("verifier", item.verifier)
                .Set("verifyTime", item.verifyTime)
                ;

            await collection.UpdateOneAsync(filter, update);
        }


        // 根据一个字段的特征删除匹配的事项
        public async Task Delete(string id)
        {
            IMongoCollection<ZServerItem> collection = this._collection;

             var filter = Builders<ZServerItem>.Filter.Eq("id", id);
            //var filter = Builders<ZServerItem>.Filter.Eq(field, value);

            await collection.DeleteOneAsync(filter);
        }

        // 取记录
        public async Task<List<ZServerItem>> Get(int start,
                int count)
        {
            List<ZServerItem> results = new List<ZServerItem>();
            var index = 0;
            using (var cursor = await this._collection.FindAsync(new BsonDocument()))
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

        // 根据用户 ID 检索用户
        // 返回单个对象 不知async怎么写???
        public ZServerItem GetById(string id)
        {
            List<ZServerItem> results = new List<ZServerItem>();

            var filter = Builders<ZServerItem>.Filter.Eq("id", id);
            results=  this._collection.Find(filter).ToList();

            if (results.Count > 0)
                return results[0];

            return null;
        }

    }
}
