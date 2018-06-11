using DigitalPlatform.IO;
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
        public const int C_State_WaitForVerfity = 0;
        public const int C_State_Pass = 1;
        public const int C_State_NoPass = 2;

        // 创建索引
        public override async Task CreateIndex()
        {
            if (_collection == null)
                throw new Exception("_collection 尚未初始化");

            /*
            // 服务器地址建索引
            await _collection.Indexes.CreateOneAsync(
                Builders<ZServerItem>.IndexKeys.Ascending("name"),
                new CreateIndexOptions() { Unique = false });

            // 服务器地址建索引
            await _collection.Indexes.CreateOneAsync(
                Builders<ZServerItem>.IndexKeys.Ascending("addr"),
                new CreateIndexOptions() { Unique = false });
            */
        }


        // parameters:
        //      item    要加入的站点信息
        public async Task<ZServerItem> Add(ZServerItem item)
        {
            // 检查 item
            if (string.IsNullOrEmpty(item.name) == true)
                throw new Exception("服务器名称不能为空");

            if (string.IsNullOrEmpty(item.addr) == true)
                throw new Exception("服务器地址不能为空");

            // 创建时间
            item.createTime = DateTimeUtil.DateTimeToString(DateTime.Now);
            item.state = C_State_WaitForVerfity; // 未审核
            item.verifier = "";
            item.lastModifyTime = item.createTime;//新增时，两个时间一致，状态没改变时，会修改

            IMongoCollection<ZServerItem> collection = this._collection;
            await collection.InsertOneAsync(item);

            return item;
        }

        // 更新 
        public async Task<ZServerItem> Update(ZServerItem item)
        {
            // 最后修改时间
            item.lastModifyTime = DateTimeUtil.DateTimeToString(DateTime.Now);


            var filter = Builders<ZServerItem>.Filter.Eq("id", item.id);
            var update = Builders<ZServerItem>.Update
                 //主要字段
                 .Set("name", item.name)
                .Set("addr", item.addr)
                .Set("port", item.port)
                .Set("homepage", item.homepage)
                .Set("dbnames", item.dbnames)
                .Set("authmethod", item.authmethod)
                .Set("groupid", item.groupid)
                .Set("username", item.username)
                .Set("password", item.password)

                //其它字段
                .Set("recsperbatch", item.recsperbatch)
                .Set("defaultMarcSyntaxOID", item.defaultMarcSyntaxOID)
                .Set("defaultElementSetName", item.defaultElementSetName)
                .Set("firstfull", item.firstfull)
                .Set("detectmarcsyntax", item.detectmarcsyntax)
                .Set("ignorereferenceid", item.ignorereferenceid)

                .Set("isbn_force13", item.isbn_force13)
                .Set("isbn_force10", item.isbn_force10)
                .Set("isbn_addhyphen", item.isbn_addhyphen)
                .Set("isbn_removehyphen", item.isbn_removehyphen)
                .Set("isbn_wild", item.isbn_wild)

                .Set("queryTermEncoding", item.queryTermEncoding)
                .Set("defaultEncoding", item.defaultEncoding)
                .Set("recordSyntaxAndEncodingBinding", item.recordSyntaxAndEncodingBinding)
                .Set("charNegoUtf8", item.charNegoUtf8)
                .Set("charNego_recordsInSeletedCharsets", item.charNego_recordsInSeletedCharsets)


                // 辅助信息
                //.Set("creatorPhone", item.creatorPhone)
                //.Set("creatorIP", item.creatorIP)
                //.Set("createTime", item.createTime)
                .Set("state", item.state)
                .Set("verifier", item.verifier)
                .Set("lastModifyTime", item.lastModifyTime)
                .Set("remark", item.remark)
                //
                ;

            await this._collection.UpdateOneAsync(filter, update);

            return item;
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
        public async Task<ZServerItem> GetById(string id)
        {
            List<ZServerItem> results = new List<ZServerItem>();

            var filter = Builders<ZServerItem>.Filter.Eq("id", id);
            results= await this._collection.Find(filter).ToListAsync();

            if (results.Count > 0)
                return results[0];

            return null;
        }

    }
}
