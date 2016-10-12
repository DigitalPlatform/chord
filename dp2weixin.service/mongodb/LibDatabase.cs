using DigitalPlatform.Text;
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
        IMongoCollection<LibEntity> _libCollection = null;
        public IMongoCollection<LibEntity> LibCollection
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
            _libCollection = this._database.GetCollection<LibEntity>("item");

            //todo
            // 创建索引            
        }

        public LibEntity GetLibById1(string id)
        {
            if (string.IsNullOrEmpty(id) == true)
                return null;

            var filter = Builders<LibEntity>.Filter.Eq("id", id);

            List<LibEntity> list = this.LibCollection.Find(filter).ToList();
            if (list.Count > 0)
            {
                LibEntity item = list[0];
                //解密
                if (String.IsNullOrEmpty(item.wxPassword)== false)
                    item.wxPassword= Cryptography.Decrypt( item.wxPassword, WeiXinConst.EncryptKey);
               
                return item;
            }
            return null;
        }
        public LibEntity GetLibByCapoUserName(string capoUserName)
        {
            var filter = Builders<LibEntity>.Filter.Eq("capoUserName", capoUserName);
            List<LibEntity> list = this.LibCollection.Find(filter).ToList();
            if (list.Count > 0)
            {
                LibEntity item = list[0];
                //解密
                if (String.IsNullOrEmpty(item.wxPassword) == false)
                    item.wxPassword = Cryptography.Decrypt(item.wxPassword, WeiXinConst.EncryptKey);

                return item;
            }
            return null;
        }

        public LibEntity GetLibByName(string libName)
        {
            var filter = Builders<LibEntity>.Filter.Eq("libName", libName);
            List<LibEntity> list = this.LibCollection.Find(filter).ToList();
            if (list.Count > 0)
            {
                LibEntity item = list[0];
                return item;
            }
            return null;
        }

        public List<LibEntity> GetLibs()
        {
            List<LibEntity> list  =this.LibCollection.Find(new BsonDocument()).ToListAsync().Result;
            if (list != null && list.Count > 0)
            {
                foreach (LibEntity item in list)
                {
                    //解密
                    if (String.IsNullOrEmpty(item.wxPassword) == false)
                        item.wxPassword = Cryptography.Decrypt(item.wxPassword, WeiXinConst.EncryptKey);

                    if (item.libName == WeiXinConst.C_Dp2003LibName)
                    {
                        item.no = -1;
                    }
                }
            }
            //list.Sort()
            list.Sort((x, y) =>
            {
                int value = x.no.CompareTo(y.no);
                if (value==0)
                    value=x.libName.CompareTo(y.libName);
                return value;
            });
            return list;
        }


        public LibEntity Add(LibEntity item)
        {
            item.OperTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            item.wxPasswordView = "*".PadRight(item.wxPassword.Length, '*');
            
            string encryptPassword = Cryptography.Encrypt(item.wxPassword, WeiXinConst.EncryptKey);
            item.wxPassword = encryptPassword;

            this.LibCollection.InsertOne(item);
            return item;
        }

        // 更新
        public long Update(string id,LibEntity item)
        {
            if (String.IsNullOrEmpty(item.wxPassword) == false)
            {
                item.wxPasswordView = "*".PadRight(item.wxPassword.Length, '*');

                string encryptPassword = Cryptography.Encrypt(item.wxPassword, WeiXinConst.EncryptKey);
                item.wxPassword = encryptPassword;
            }

            var filter = Builders<LibEntity>.Filter.Eq("id", id);
            var update = Builders<LibEntity>.Update
                //.Set("libCode", item.libCode)
                .Set("libName", item.libName)
                .Set("capoUserName", item.capoUserName)
                .Set("capoContactPhone", item.capoContactPhone)

                .Set("wxUserName", item.wxUserName)
                .Set("wxPassword", item.wxPassword)
                .Set("wxPasswordView", item.wxPasswordView)
                .Set("wxContactPhone", item.wxContactPhone) 

                .Set("comment", item.comment)
                .Set("OperTime", item.OperTime)
                .Set("noShareBiblio", item.noShareBiblio)
                .Set("searchDbs", item.searchDbs)
                .Set("match", item.match)
                ;

            UpdateResult ret = this.LibCollection.UpdateOneAsync(filter, update).Result;
            return ret.ModifiedCount;
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="item"></param>
        public void Delete(String id)
        {
            var filter = Builders<LibEntity>.Filter.Eq("id", id);
            this.LibCollection.DeleteOne(filter);
        }

    }

    public class LibEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; private set; }

        //public string libCode { get; set; }
        public string libName { get; set; }
        public string capoUserName { get; set; }
        public string capoContactPhone { get; set; } // 图书馆联系人电话 jane 2016-6-17

        // 2016-6-17 jane 本方账户的信息
        public string wxUserName { get; set; } //微信端本方用户名
        public string wxPassword { get; set; }    //本方密码
        public string wxContactPhone { get; set; }    //本方联系人手机号，用于将来的找回密码
        public string wxPasswordView{ get; set; }  


        public string comment { get; set; }  // 注释
        public string OperTime { get; set; } // 操作时间

        public int noShareBiblio  { get; set; } // 不对外公开书目;


        public string searchDbs { get; set; }  // 参于检索的书目库
        public string match { get; set; }  // 简单检索匹配方式

        public int no = 0;

    }
}
