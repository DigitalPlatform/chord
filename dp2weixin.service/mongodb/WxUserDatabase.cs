using DigitalPlatform.IO;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace dp2weixin.service
{
    /// <summary>
    /// 用户绑定数据库
    /// </summary>
    public sealed class WxUserDatabase
    {
        //常量
        public const int C_Type_Patron = 0;
        public const int C_Type_Worker = 1;

        // 状态
        public const int C_State_Available = 1;
        public const int C_State_Temp = 2;
        public const int C_State_disabled = 0;

        /// <summary>
        /// 单一静态实例,饿汉模式
        /// </summary>
        private static readonly WxUserDatabase _db = new WxUserDatabase();
        public static WxUserDatabase Current
        {
            get
            {
                return _db;
            }
        }

        // 成员变量
        MongoClient _mongoClient = null;
        IMongoDatabase _database = null;
        string _wxUserDbName = "";
        IMongoCollection<WxUserItem> _wxUserCollection = null;
        public IMongoCollection<WxUserItem> wxUserCollection
        {
            get
            {
                return this._wxUserCollection;
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
            _wxUserDbName = strInstancePrefix + "user";

            this._mongoClient = new MongoClient(strMongoDbConnStr);
            this._database = this._mongoClient.GetDatabase(this._wxUserDbName);

            //collection名称为item
            _wxUserCollection = this._database.GetCollection<WxUserItem>("item");

            CancellationTokenSource cancel= new CancellationTokenSource();
            bool bExist = false;

            // 已存在索引，不用再创建索引
            var indexes = _wxUserCollection.Indexes.ListAsync(cancel.Token).Result.ToListAsync().Result;
            foreach (BsonDocument doc in indexes)
            {
                string name= doc["name"].AsString;
                if (name.Contains("weixinId")==true)
                {
                    bExist = true;
                    continue;
                }
            }

            // 创建索引
            if (bExist == false)
            {
                this.CreateIndex();
            }
        }

        public void CreateIndex()
        {
            // 为weixinid字段建索引
            _wxUserCollection.Indexes.CreateOne(
                Builders<WxUserItem>.IndexKeys.Ascending("weixinId"),
                new CreateIndexOptions() { Unique = false }
                );
        }

        public List<WxUserItem> GetAll()
        {
            List<WxUserItem> list = this.wxUserCollection.Find(new BsonDocument()).ToListAsync().Result;
            return list;
        }

        // 获取绑定账户
        public List<WxUserItem> Get(string weixinId, 
            string libId,
            int type,
            string patronBarcode,
            string userName,
            bool bOnlyAvailable)
        {
            var filter = Builders<WxUserItem>.Filter.Empty;

            if (bOnlyAvailable == true) // 只取有效的
            {
                filter = filter & Builders<WxUserItem>.Filter.Eq("state", C_State_Available);
            }

            if (string.IsNullOrEmpty(weixinId) == false)
            {
                filter = filter & Builders<WxUserItem>.Filter.Eq("weixinId", weixinId);
            }

            if (string.IsNullOrEmpty(libId) == false)
            {
                filter = filter & Builders<WxUserItem>.Filter.Eq("libId", libId);
            }

            if (type != -1)
            {
                filter = filter & Builders<WxUserItem>.Filter.Eq("type", type);
            }

            if (string.IsNullOrEmpty(patronBarcode) == false)
            {
                filter = filter & Builders<WxUserItem>.Filter.Eq("readerBarcode", patronBarcode);
            }

            if (string.IsNullOrEmpty(userName) == false)
            {
                filter = filter & Builders<WxUserItem>.Filter.Eq("userName", userName);
            }



            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();
            return list;
        }

        // 获取有效的绑定账户
        public List<WxUserItem> Get(string weixinId, string libId, int type)
        {
            return this.Get(weixinId, libId,type,null,null, true);
        }

        // 获取指定的读者账户,针对一个图书馆可绑定多个读者账户
        public List<WxUserItem> GetPatron(string weixinId, string libId, string readerBarcode)
        {
            return this.Get(weixinId, libId, C_Type_Patron, readerBarcode,null, true);
        }

        public List<WxUserItem> GetWorkers(string weixinId, string libId, string userName)
        {
            return this.Get(weixinId, libId, C_Type_Worker,  null,userName, true);
        }

        // 获取工作人员账户，目前设计是：针对一个图书馆只绑定一个账户
        public WxUserItem GetWorker(string weixinId, string libId)
        {
            List<WxUserItem> list = this.Get(weixinId, libId, C_Type_Worker);// this.wxUserCollection.Find(filter).ToList();
            if (list != null && list.Count >= 1)
                return list[0];

            return null;
        }

        public WxUserItem GetPatronByPatronRefID(string weixinId,
            string libId,
            string patronRefID)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("state", C_State_Available)
                & Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("libId", libId)
                & Builders<WxUserItem>.Filter.Eq("refID", patronRefID);

            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();
            if (list != null && list.Count > 0)
                return list[0];

            return null;
        }

        // 获取当前活动的读者账户
        public WxUserItem GetActivePatron(string weixinId,string libId)
        {
            WxUserItem activePatron = null;
            int count = 0;
            List<WxUserItem> list = this.Get(weixinId, libId, C_Type_Patron);//this.wxUserCollection.Find(filter).ToList();//.ToListAsync().Result;
            if (list != null && list.Count > 0)
            {
                foreach (WxUserItem item in list)
                {
                    if (item.isActive == 1)
                    {
                        activePatron = item;
                        count++;
                    }
                } 
            }

            if (count>1)
                throw new Exception("程序异常：微信号活动读者数量有" +list.Count+"个");

            return activePatron;
        }

        // 绑定微信用户绑定的图书馆id列表
        public List<string> GetLibsByWeixinId(string weixinId)
        {
            List<string> libs = new List<string>();
            List<WxUserItem> list = this.Get(weixinId, null,-1);
            if (list != null && list.Count > 0)
            {
                foreach (WxUserItem item in list)
                {
                    if (libs.Contains(item.libId) == false)
                    {
                        libs.Add(item.libId);
                    }
                }
            }
            return libs;
        }

        // 获取一个读者账户
        public WxUserItem GetOnePatron(string weixinId,string libId)
        {
            List<WxUserItem> list = this.Get(weixinId, libId, C_Type_Patron);
            if (list !=null && list.Count > 0)
                return list[0];

            return null;
        }

        /// <summary>
        /// 根据id获取对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WxUserItem GetById(String id)
        {
            if (string.IsNullOrEmpty(id) == true || id == "null")
                return null;
            IMongoCollection<WxUserItem> collection = this.wxUserCollection;
            var filter = Builders<WxUserItem>.Filter.Eq("id", id);
            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();
            if (list.Count > 0)
            {
                return list[0];
            }
            return null;
        }


        private static readonly Object _sync_addUser = new Object();
        /// 新增绑定账户
        public WxUserItem Add(WxUserItem item)
        {
            lock (_sync_addUser)
            {
                List<WxUserItem> itemList = this.Get(item.weixinId, item.libId, item.type, item.readerBarcode, item.userName, true);
                if (itemList.Count == 0)
                {
                    this.wxUserCollection.InsertOne(item);
                }
                else
                {
                    dp2WeiXinService.Instance.WriteLog1("发现绑定帐户库中已有'" + item.readerBarcode + "'或'" + item.userName + "'对应的记录。");
                }
            }
            return item;
        }

        /// 删除账户
        /// 如果删除的是读者账户，自动将对应图书馆第一个读者账户设为默认的
        public void Delete(String id,out WxUserItem newActivePatron)
        {
            newActivePatron = null;

            if (string.IsNullOrEmpty(id) == true || id == "null")
                return;

            // 检查一下是否被删除读者是否为默认读者，如果是，后面把自动将默认值设了第一个读者上。
            WxUserItem userItem = this.GetById(id);
            if (userItem == null)
                return;

            string weixinId = "";
            int type = -1;
            int isActive = 0;
            string libId = "";
            string tracing = "";
            if (userItem != null)
            {
                weixinId = userItem.weixinId;
                type = userItem.type;
                isActive = userItem.isActive;
                libId = userItem.libId;
                tracing = userItem.tracing;
            }

            // 先删除
            IMongoCollection<WxUserItem> collection = this.wxUserCollection;
            var filter = Builders<WxUserItem>.Filter.Eq("id", id);
            collection.DeleteOne(filter);

            // 如果删除的是读者账户且是激活的，自动将第一个读者账户设为默认的
            if (type == 0 && isActive == 1)
            {
                WxUserItem newUserItem = this.GetOnePatron(weixinId, libId);
                if (newUserItem != null)
                {
                    this.SetActivePatron(newUserItem.weixinId, newUserItem.id);
                    newActivePatron = newUserItem;
                }
            }

            // 如果是工作人员,且打开监控功能
            if (type==1 && String.IsNullOrEmpty(tracing)==false && tracing !="off")
            {
                dp2WeiXinService.Instance.UpdateMemoryTracingUser(weixinId, libId, "off");
            }
        }


        // 根据libId与状态删除记录
        public void Delete(string libId,int state)
        {
            if (String.IsNullOrEmpty(libId) == true)
                return;
            
            var filter = Builders<WxUserItem>.Filter.Eq("libId", libId);
            if (state != -1)
            {
                filter = filter & Builders<WxUserItem>.Filter.Eq("state", state);
            }
            DeleteResult ret = this.wxUserCollection.DeleteMany(filter);
        }

        public void SetState(string libId,int fromState,int toState)
        {
            if (string.IsNullOrEmpty(libId) == true)
                return;

            // 查找指定图书馆的账户
            var filter = Builders<WxUserItem>.Filter.Eq("libId", libId);
            if (fromState != -1) 
            {
                filter = filter & Builders<WxUserItem>.Filter.Eq("state", fromState); 
            }
                
            var update = Builders<WxUserItem>.Update
                .Set("state", toState)
                .Set("updateTime", DateTimeUtil.DateTimeToString(DateTime.Now));
            UpdateResult ret = this.wxUserCollection.UpdateMany(filter, update);
        }

        public List<WxUserItem> GetActivePatrons()
        {
            var filter = Builders<WxUserItem>.Filter.Eq("isActive", 1);
            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();
            return list;
        }


        /// <summary>
        /// 激活读者账户
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="id"></param>
        public void SetActivePatron(string weixinId, string id)
        {
            if (string.IsNullOrEmpty(weixinId) == true)
                return;

            if (string.IsNullOrEmpty(id) == true)
                return;

            // 先将该微信用户的所有绑定读者都设为非活动
            this.SetNoActivePatron(weixinId);

            // 再将参数传入的记录设为活动状态
            var filter = Builders<WxUserItem>.Filter.Eq("id",id);
            var update = Builders<WxUserItem>.Update
                .Set("isActive", 1)
                .Set("updateTime", DateTimeUtil.DateTimeToString(DateTime.Now));
            this.wxUserCollection.UpdateMany(filter, update);
        }

        public void SetNoActivePatron(string weixinId)
        {
            if (string.IsNullOrEmpty(weixinId) == true)
                return;
            
            // 将该微信用户的所有绑定读者都设为非活动
            var filter = Builders<WxUserItem>.Filter.Eq("weixinId", weixinId)
                & Builders<WxUserItem>.Filter.Eq("type", 0); // jane 2016-6-16 注意只存读者账户
            var update = Builders<WxUserItem>.Update
                .Set("isActive", 0)
                .Set("updateTime", DateTimeUtil.DateTimeToString(DateTime.Now));
            UpdateResult ret = this.wxUserCollection.UpdateMany(filter, update);
        }

        /// <summary>
        /// 设置监控开关
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tracing"></param>
        /// <returns></returns>
        public long UpdateTracing(string id,string tracing)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("id",id);
            var update = Builders<WxUserItem>.Update
                .Set("tracing", tracing)
                ;
            UpdateResult ret = this.wxUserCollection.UpdateOne(filter, update);
            return ret.ModifiedCount;
        }

        /// <summary>
        /// 更新微信用户
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public long Update(WxUserItem item)
        {
            var filter = Builders<WxUserItem>.Filter.Eq("id", item.id);
            var update = Builders<WxUserItem>.Update
                .Set("weixinId", item.weixinId)
                .Set("libName", item.libName)
                .Set("libId", item.libId)

                .Set("readerBarcode", item.readerBarcode)
                .Set("readerName", item.readerName)
                .Set("department", item.department)
                .Set("xml", item.xml)
                .Set("recPath", item.recPath)

                .Set("refID", item.refID)
                .Set("createTime", item.createTime)
                .Set("updateTime", item.updateTime)
                .Set("isActive", item.isActive)


                .Set("libraryCode", item.libraryCode)
                .Set("type", item.type)
                .Set("userName", item.userName)
                .Set("isActiveWorker", item.isActiveWorker)

                // 2016-8-26 jane 加
                .Set("state", item.state)
                .Set("remark", item.remark)
                .Set("rights", item.rights)
                .Set("appId", item.appId)
                .Set("tracing", item.tracing)
                .Set("location", item.location)
                .Set("selLocation", item.selLocation)
                ;

            UpdateResult ret = this.wxUserCollection.UpdateOne(filter, update);
            return ret.ModifiedCount;
        }


        internal List<WxUserItem> GetTracingUsers()
        {
            var filter = Builders<WxUserItem>.Filter.Eq("tracing","on")
                | Builders<WxUserItem>.Filter.Eq("tracing", "on -mask");
                ;
            List<WxUserItem> list = this.wxUserCollection.Find(filter).ToList();
            return list;
        }
    }
    public class WxUserItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; private set; }

        // 微信id
        public string weixinId { get; set; }    

        // 图书馆名称
        public string libName { get; set; }    

        // 图书馆代码
        public string libId { get; set; }     

        // 读者证条码号
        public string readerBarcode { get; set; }  

        // 读者姓名
        public string readerName { get; set; }

        //单位
        public string department { get; set; }  //部门，二维码下方显示 // 2016-6-16 新增

        // 读者记录xml
        public string xml { get; set; }        

        // 读者记录路径
        public string recPath { get; set; } 
        
        // 读者参考id
        public string refID { get; set; }
        public string createTime { get; set; }
        public string updateTime { get; set; }

        // 是否活动状态
        public int isActive = 0;

        //分馆代码，读者与工作人员都有该字段
        public string libraryCode { get; set; } 

        //账户类型：0表示读者 1表示工作人员 // 2016-6-16 新增
        public int type = 0;

        //当type=2时，表示工作人员账户名称，其它时候为空// 2016-6-16 新增       
        public string userName { get; set; } 
        public int isActiveWorker= 0; //是否为当前激活工作人员账户，注意该字段对读者账户无意义（均为0），暂时未用到


        // 2016-8-26 jane 新增
        public int state { get; set; } //状态:0失效 1有效 2恢复时的临时状态 
        public string remark { get; set; } // 会存一下绑定方式等

        // 权限
        public string rights { get; set; }

        // 公众号id 2016-11-14
        public string appId { get; set; }

        //tracing 2016-11-21 type=1工作人员时有意义 默认为空或者off
        public string tracing { get; set; }

        // 20170213 jane
        // 本用户在dp系统有权限的馆藏地，是xml格式
            /*
<item canborrow="no" itemBarcodeNullable="yes">保存本库</item>
<item canborrow="no" itemBarcodeNullable="yes">阅览室</item>
<item canborrow="yes" itemBarcodeNullable="yes">流通库</item>
<library code="方洲小学">
  <item canborrow="yes" itemBarcodeNullable="yes">图书总库</item>
</library>
<library code="星洲小学">
  <item canborrow="yes" itemBarcodeNullable="yes">阅览室</item>
</library>
             */       
        public string location { get; set; }

        // 20170213 在微信中选择的馆藏地，是以逗号分隔的两级路径，如：/流通库,方洲小学/图书总库
        public string selLocation { get; set; }
    }


    /// <summary>
    /// 2016-8-26 为了不混乱使用字段，绑定时参数不再借用WxUserItem，新增bindItem,用于绑定接口
    /// </summary>
    public class BindItem
    {
        // 2016-11-16 appId统一放在weixinId里
        //公众号 appid 2016-11-14 
        //public string appId { get; set; }   

        public string weixinId { get; set; }   
        public string libId { get; set; }     

        public string prefix { get; set; }  //必须设为属性，才能在前端传值。
        public string word { get; set; }
        public string password { get; set; }

    }



}
