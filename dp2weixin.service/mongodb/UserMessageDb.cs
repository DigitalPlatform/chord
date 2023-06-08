using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dp2weixin.service
{
    
    /// <summary>
    /// 用户数据库
    /// </summary>
    public sealed class UserMessageDb
    {
        /// <summary>
        /// 单一静态实例,饿汉模式
        /// </summary>
        private static readonly UserMessageDb _db = new UserMessageDb();
        public static UserMessageDb Current
        {
            get
            {
                return _db;
            }
        }

        // 成员变量
        MongoClient _mongoClient = null;
        IMongoDatabase _database = null;
        string _dbName = "";
        IMongoCollection<UserMessageItem> _collection = null;
        public IMongoCollection<UserMessageItem> Collection
        {
            get
            {
                return this._collection;
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
            _dbName = strInstancePrefix + "message";

            this._mongoClient = new MongoClient(strMongoDbConnStr);
            this._database = this._mongoClient.GetDatabase(this._dbName);

            //collection名称为item
            this._collection = this._database.GetCollection<UserMessageItem>("item");

            // todo 创建索引            
        }

        public List<UserMessageItem> GetAll()
        {
            List<UserMessageItem> list = this.Collection.Find(new BsonDocument()).ToListAsync().Result;
            return list;
        }

        /// <summary>
        /// 获取读者账户,针对一个图书馆可绑定多个读者账户
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libCode"></param>
        /// <param name="readerBarcode"></param>
        /// <returns></returns>
        public List<UserMessageItem> GetByUserId(string userId)
        {
            var filter = Builders<UserMessageItem>.Filter.Eq("userId", userId);

            //Sort
            SortDefinition<UserMessageItem> sort =  Builders<UserMessageItem>.Sort.Descending("createTime");

            List<UserMessageItem> list = this.Collection.Find(filter).Sort(sort).ToList();
            return list;
        }
        
        
        /// <summary>
        /// 新增微信用户
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void Add(UserMessageItem item)
        {
            this.Collection.InsertOne(item);

        }

        // 根据libId与状态删除记录
        public void Delete(string userId,string id)
        {

            if (string.IsNullOrEmpty(userId) == true && string.IsNullOrEmpty(id)==true)
                return;

            var filter = Builders<UserMessageItem>.Filter.Empty;

            if (string.IsNullOrEmpty(userId) == false)
            {
                filter = filter & Builders<UserMessageItem>.Filter.Eq("userId", userId);
            }

            if (string.IsNullOrEmpty(id) == false)
            {
                filter = filter & Builders<UserMessageItem>.Filter.Eq("id", id);
            }

            DeleteResult ret = this.Collection.DeleteMany(filter);

            //dp2WeiXinService.Instance.WriteDebug("3.共删除成功" + ret.DeletedCount + "个对象");
        }


    }
    public class UserMessageItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; private set; }

        // 用于定位元素，id是objectid类型，是自动产生的，消息设置linkurl时还不知道id值，所以新增一个统一的refid
        public string refid { get; set; }

        public string userId { get; set; }  // 对应的微信id,也可能是~~

        public string msgType { get; set; }

        public string xml { get; set; }

        // 原始xml 2023/6/7增加
        public string originalXml { get; set; }


        public string createTime { get; set; }



    }

    public class UserMessageMode
    {

        public UserMessageMode(UserMessageItem item)
        {
            this.id=item.id;
            this.refid=item.refid;
            this.userId=item.userId;
            this.msgType=item.msgType;
            this.xml=item.xml;
            this.originalXml=item.originalXml;
            this.createTime=item.createTime;

            this.ParseXml(item.xml);
        }

        public string id { get; private set; }

        // 用于定位元素
        public string refid { get; set; }

        public string userId { get; set; }  // 对应的微信id,也可能是~~

        public string msgType { get; set; }

        // 消息message
        public string xml { get; set; }

        // 原始xml，暂未使用
        public string originalXml { get; set; }

        public string createTime { get; set; }

        public string first { get; set; }
        public string remark { get; set; }

        public string title { get; set; }
        public List<LabelValue> valueList { get; set; }
        public string keyword1 { get; set; }
        public string keyword2 { get; set; }
        public string keyword3 { get; set; }
        public string keyword4 { get; set; }
        public string keyword5 { get; set; }
        /*
<root>
<first>▉▊▋▍▎▉▊▋▍▎▉▊▋▍▎</first>
<keyword1>文心雕龙义证 [专著]  </keyword1>
<keyword2>B001(本地图书馆/流通库)</keyword2>
<keyword3>2023/05/24</keyword3>
<keyword4>31天</keyword4>
<keyword5>2023/05/24 test P001(本地图书馆)</keyword5>
<remark>test P001(本地图书馆)，感谢还书。</remark>
</root>
 */
        public void ParseXml(string xml)
        {
            if (string.IsNullOrEmpty(xml) == true)
                return;

            XmlDocument dom = new XmlDocument ();
            dom.LoadXml(xml);
            XmlNode root=dom.DocumentElement;

            this.first=DomUtil.GetElementText(root, "first");
            this.remark = DomUtil.GetElementText(root, "remark");

            this.keyword1 = DomUtil.GetElementText(root, "keyword1");
            this.keyword2 = DomUtil.GetElementText(root, "keyword2");
            this.keyword3 = DomUtil.GetElementText(root, "keyword3");
            this.keyword4 = DomUtil.GetElementText(root, "keyword4");
            this.keyword5 = DomUtil.GetElementText(root, "keyword5");

            this.valueList = new List<LabelValue>();


            /* weixin.xml配置
<templates>
        <template name='Bind' id="hFmNH7on2FqSOAiYPZVJN-FcXBv4xpVLBvHsfpLLQKU"/>
        <template name="UnBind" id="1riAKkt2W0AOtkx5rx-Lwa0RKRydDTHaMjSoUBGuHog"/>

        <template name="Borrow" id="2AVbpcn0y1NtqIQ7X7KY1Ebcyyhx6mUXTpAxuOcxSE0"/>
        <template name="Return" id="zzlLzStt_qZlzMFhcDgRm8Zoi-tsxjWdsI2b3FeoRMs"/>

        <template name="Arrived" id="U79IrJOgNJWZnqeKy2467ZoN-aM9vrEGQf2JJtvdBPM"/>
        <template name="CancelReserve" id="8V9JKEs7s01spSOANhcG-LfED7EBYOhtanmOzYsaG-s" remark="预约取消通知"/>

        <template name="CaoQi" id="2sOCuATcFdSNbJM24zrHnFv89R3D-cZFIpk4ec_Irn4"/>
        <template name="KuaiCaoQi" id="L0RqYhRelT7AJ5Z2_eeImlK0cq4sn4mBHmZYv_Lbkw0" remark="图书即将超期通知"/>

        <template name="Pay" id="xFg1P44Hbk_Lpjc7Ds4gU8aZUqAlzoKpoeixtK1ykBI"/>
        <template name="CancelPay" id="-XsD34ux9R2EgAdMhH0lpOSjcozf4Jli_eC86AXwM3Q"/>


        <template name="ReviewPatron" id="5c6svlDm4NyhpHJEE9bp8Wi5hMrCBGmIg0iQfuB-a-0" remark="读者注册审核"/>
        <template name="ReviewResult" id="GQrTTzTfHzq-zHWtcdsKsOcJGrwb4Ciycdyjx7GoRDw" remark="审核结果通知"/>
        <template name="PatronInfoChanged" id="1wbaAc0iMzjS3oCkLhh8I6YInReW5pTUCB343tSzeIE" remark="用户资料变更通知"/>
        
        <template name="Message" id="rtAx0BoUAwZ3npbNIO8Y9eIbdWO-weLGE2iOacGqN_s" remark="个人消息"/>

            <
	<template name="YiTingDaiJinDaoQi" id="YmKMCqLne46296tXgvJB5DX8GUjpa7Yue41-EWunITs" remark="停借期满通知-未使用"/>
	<template name="Recall" id="YmKMCqLne46296tXgvJB5DX8GUjpa7Yue41-EWunITs" remark="图书召回通知-未使用"/>

      

</templates>
             */


            /*
        // 微信通知类型，到时根据此类型转为对应的模板消息id
        public const string C_Template_Bind = "Bind";
        public const string C_Template_UnBind = "UnBind";

        public const string C_Template_Borrow = "Borrow";
        public const string C_Template_Return = "Return";



                    public const string C_Template_Arrived = "Arrived";
        public const string C_Template_CancelReserve = "CancelReserve";

                    public const string C_Template_CaoQi = "CaoQi";
        public const string C_Template_KuaiCaoQi = "KuaiCaoQi";  //即将超期

        public const string C_Template_Pay = "Pay";
        public const string C_Template_CancelPay = "CancelPay";

                    public const string C_Template_Message = "Message";

                    // 2020-3-1增加 审核读者
        public const string C_Template_ReviewPatron = "ReviewPatron";
        // 2020-3-7加 审核结果
        public const string C_Template_ReviewResult = "ReviewResult";
        // 2020-3-17 加 读者信息变更
        public const string C_Template_PatronInfoChanged = "PatronInfoChanged";









             */
            if (this.msgType == "Bind")
            {
                /*
模板ID：hFmNH7on2FqSOAiYPZVJN-FcXBv4xpVLBvHsfpLLQKU
标题：微信绑定通知
详细内容：
{{first.DATA}}
绑定帐号：{{keyword1.DATA}}
绑定说明：{{keyword2.DATA}}
{{remark.DATA}}
                 */
                this.title = "微信绑定通知";
                this.valueList.Add(new LabelValue("绑定帐号", keyword1));
                this.valueList.Add(new LabelValue("绑定说明", keyword2));
            }
            else if (this.msgType == "UnBind")
            {
                /*
模版ID：1riAKkt2W0AOtkx5rx-Lwa0RKRydDTHaMjSoUBGuHog
标题：微信解绑通知
详细内容：
{{first.DATA}}
解绑帐号：{{keyword1.DATA}}
解绑说明：{{keyword2.DATA}}
{{remark.DATA}}
                 */
                this.title = "微信解绑通知";
                this.valueList.Add(new LabelValue("解绑帐号", keyword1));
                this.valueList.Add(new LabelValue("解绑说明", keyword2));
            }
            else if (this.msgType == "Borrow")
            {
                /*
    模版ID：2AVbpcn0y1NtqIQ7X7KY1Ebcyyhx6mUXTpAxuOcxSE0
    标题：借书成功通知
    详细内容：
    {{first.DATA}}
    书刊摘要：{{keyword1.DATA}}
    册条码号：{{keyword2.DATA}}
    借书日期：{{keyword3.DATA}}
    应还日期：{{keyword4.DATA}}
    证条码号：{{keyword5.DATA}}
    {{remark.DATA}}
                 */
                this.title = "借书成功通知";
                this.valueList.Add(new LabelValue("书刊摘要", keyword1));
                this.valueList.Add(new LabelValue("册条码号", keyword2));
                this.valueList.Add(new LabelValue("借书日期", keyword3));
                this.valueList.Add(new LabelValue("应还日期", keyword4));
                this.valueList.Add(new LabelValue("证条码号", keyword5));
            }
            else if (this.msgType== "Return")
            {
                /*
模版ID：2AVbpcn0y1NtqIQ7X7KY1Ebcyyhx6mUXTpAxuOcxSE0
标题：还书成功通知
详细内容：
{{first.DATA}}
书刊摘要：{{keyword1.DATA}}
册条码号：{{keyword2.DATA}}
借书日期：{{keyword3.DATA}}
借阅期限：{{keyword4.DATA}}
还书日期：{{keyword5.DATA}}
{{remark.DATA}}
                 */
                this.title = "还书成功通知";
                this.valueList.Add(new LabelValue("书刊摘要", keyword1));
                this.valueList.Add(new LabelValue("册条码号", keyword2));
                this.valueList.Add(new LabelValue("借书日期", keyword3));
                this.valueList.Add(new LabelValue("借阅期限", keyword4));
                this.valueList.Add(new LabelValue("还书日期", keyword5));
            }
            else if (this.msgType == "Arrived")
            {
                /*
模版ID：U79IrJOgNJWZnqeKy2467ZoN-aM9vrEGQf2JJtvdBPM
标题：预约到书通知
详细内容：
{{first.DATA}}
书刊摘要：{{keyword1.DATA}}
册条码号：{{keyword2.DATA}}
预约日期：{{keyword3.DATA}}
到书日期：{{keyword4.DATA}}
保留期限：{{keyword5.DATA}}
{{remark.DATA}}


                 */
                this.title = "预约到书通知";
                this.valueList.Add(new LabelValue("书刊摘要", keyword1));
                this.valueList.Add(new LabelValue("册条码号", keyword2));
                this.valueList.Add(new LabelValue("预约日期", keyword3));
                this.valueList.Add(new LabelValue("到书日期", keyword4));
                this.valueList.Add(new LabelValue("保留期限", keyword5));
            }
            else if (this.msgType == "CancelReserve")
            {
                /*
模版ID：8V9JKEs7s01spSOANhcG-LfED7EBYOhtanmOzYsaG-s
标题：预约取消通知
详细内容：
{{first.DATA}}
书刊摘要：{{keyword1.DATA}}
册条码号：{{keyword2.DATA}}
预约日期：{{keyword3.DATA}}
取消日期：{{keyword4.DATA}}
证条码号：{{keyword5.DATA}}
{{remark.DATA}}


                 */
                this.title = "预约取消通知";
                this.valueList.Add(new LabelValue("书刊摘要", keyword1));
                this.valueList.Add(new LabelValue("册条码号", keyword2));
                this.valueList.Add(new LabelValue("预约日期", keyword3));
                this.valueList.Add(new LabelValue("取消日期", keyword4));
                this.valueList.Add(new LabelValue("证条码号", keyword5));
            }
            else if (this.msgType == "CaoQi")
            {
                /*
模版ID：todo
标题：图书超期通知
{{first.DATA}}
书刊摘要：{{keyword1.DATA}}
册条码号：{{keyword2.DATA}}
借书日期：{{keyword3.DATA}}
应还日期：{{keyword4.DATA}}
超期情况：{{keyword5.DATA}}
{{remark.DATA}}
                 */
                this.title = "图书超期通知";
                this.valueList.Add(new LabelValue("书刊摘要", keyword1));
                this.valueList.Add(new LabelValue("册条码号", keyword2));
                this.valueList.Add(new LabelValue("借书日期", keyword3));
                this.valueList.Add(new LabelValue("应还日期", keyword4));
                this.valueList.Add(new LabelValue("超期情况", keyword5));
            }
            else if (this.msgType == "KuaiCaoQi")
            {
                /*
模板ID
L0RqYhRelT7AJ5Z2_eeImlK0cq4sn4mBHmZYv_Lbkw0
开发者调用模板消息接口时需提供模板ID
标题
图书即将超期通知
行业
政府与公共事业 - 博物馆
详细内容
{{first.DATA}}
书刊摘要：{{keyword1.DATA}}
册条码号：{{keyword2.DATA}}
借书日期：{{keyword3.DATA}}
应还日期：{{keyword4.DATA}}
超期说明：{{keyword5.DATA}}
{{remark.DATA}}
                 */
                this.title = "图书即将超期通知";
                this.valueList.Add(new LabelValue("书刊摘要", keyword1));
                this.valueList.Add(new LabelValue("册条码号", keyword2));
                this.valueList.Add(new LabelValue("借书日期", keyword3));
                this.valueList.Add(new LabelValue("应还日期", keyword4));
                this.valueList.Add(new LabelValue("超期说明", keyword5));
            }
            else if (this.msgType == "Pay")
            {
                /*
模版ID：xFg1P44Hbk_Lpjc7Ds4gU8aZUqAlzoKpoeixtK1ykBI
标题：交费成功通知
详细内容：
{{first.DATA}}
书刊摘要：{{keyword1.DATA}}
册条码号：{{keyword2.DATA}}
交费金额：{{keyword3.DATA}}
交费原因：{{keyword4.DATA}}
交费时间：{{keyword5.DATA}}
{{remark.DATA}}
                 */
                this.title = "交费成功通知";
                this.valueList.Add(new LabelValue("书刊摘要", keyword1));
                this.valueList.Add(new LabelValue("册条码号", keyword2));
                this.valueList.Add(new LabelValue("交费金额", keyword3));
                this.valueList.Add(new LabelValue("交费原因", keyword4));
                this.valueList.Add(new LabelValue("交费时间", keyword5));
            }
            else if (this.msgType == "CancelPay")
            {
                /*
模版ID：-XsD34ux9R2EgAdMhH0lpOSjcozf4Jli_eC86AXwM3Q
标题：交费撤消通知
详细内容：
{{first.DATA}}
书刊摘要：{{keyword1.DATA}}
册条码号：{{keyword2.DATA}}
交费原因：{{keyword3.DATA}}
撤消金额：{{keyword4.DATA}}
撤消时间：{{keyword5.DATA}}
{{remark.DATA}}

                 */
                this.title = "交费成功通知";
                this.valueList.Add(new LabelValue("书刊摘要", keyword1));
                this.valueList.Add(new LabelValue("册条码号", keyword2));
                this.valueList.Add(new LabelValue("交费原因", keyword3));
                this.valueList.Add(new LabelValue("撤消金额", keyword4));
                this.valueList.Add(new LabelValue("撤消时间", keyword5));
            }
            else if (this.msgType == "Message")
            {
                /*
模板ID
rtAx0BoUAwZ3npbNIO8Y9eIbdWO-weLGE2iOacGqN_s
开发者调用模板消息接口时需提供模板ID
标题
个人消息通知
行业
IT科技 - IT软件与服务
详细内容
{{first.DATA}}
标题：{{keyword1.DATA}}
时间：{{keyword2.DATA}}
内容：{{keyword3.DATA}}
{{remark.DATA}}
                 */
                this.title = "个人消息通知";
                this.valueList.Add(new LabelValue("标题", keyword1));
                this.valueList.Add(new LabelValue("时间", keyword2));
                this.valueList.Add(new LabelValue("内容", keyword3));

                // 注：以停代金，图书召回，使用的此模板
            }
            else if (this.msgType == "ReviewPatron")
            {
                /*
5c6svlDm4NyhpHJEE9bp8Wi5hMrCBGmIg0iQfuB-a-0
开发者调用模板消息接口时需提供模板ID
标题
待审核通知
行业
IT科技 - IT软件与服务
详细内容
{{first.DATA}}
申请人：{{keyword1.DATA}}
手机号码：{{keyword2.DATA}}
申请进度：{{keyword3.DATA}}
申请时间：{{keyword4.DATA}}
{{remark.DATA}}

                 */
                this.title = "待审核通知";
                this.valueList.Add(new LabelValue("申请人", keyword1));
                this.valueList.Add(new LabelValue("手机号码", keyword2));
                this.valueList.Add(new LabelValue("申请进度", keyword3));
                this.valueList.Add(new LabelValue("申请时间", keyword4));
            }
            else if (this.msgType == "ReviewResult")
            {
                /*
模板ID
GQrTTzTfHzq-zHWtcdsKsOcJGrwb4Ciycdyjx7GoRDw
开发者调用模板消息接口时需提供模板ID
标题
审核结果通知
行业
IT科技 - IT软件与服务
详细内容
{{first.DATA}}
申请人：{{keyword1.DATA}}
手机号码：{{keyword2.DATA}}
审核结果：{{keyword3.DATA}}
{{remark.DATA}}


                 */
                this.title = "审核结果通知";
                this.valueList.Add(new LabelValue("申请人", keyword1));
                this.valueList.Add(new LabelValue("手机号码", keyword2));
                this.valueList.Add(new LabelValue("审核结果", keyword3));
            }
            else if (this.msgType == "PatronInfoChanged")
            {
                /*
模板ID
1wbaAc0iMzjS3oCkLhh8I6YInReW5pTUCB343tSzeIE
开发者调用模板消息接口时需提供模板ID
标题
用户资料变更通知
行业
IT科技 - IT软件与服务
详细内容
{{first.DATA}}
用户名：{{keyword1.DATA}}
联系方式：{{keyword2.DATA}}
变更类型：{{keyword3.DATA}}
变更时间：{{keyword4.DATA}}
{{remark.DATA}}
                 */
                this.title = "用户资料变更通知";
                this.valueList.Add(new LabelValue("用户名", keyword1));
                this.valueList.Add(new LabelValue("联系方式", keyword2));
                this.valueList.Add(new LabelValue("变更类型", keyword3));
                this.valueList.Add(new LabelValue("变更时间", keyword4));
            }
            /*
                                 // 2020-3-1增加 审核读者
        public const string C_Template_ReviewPatron = "ReviewPatron";
        // 2020-3-7加 审核结果
        public const string C_Template_ReviewResult = "ReviewResult";
        // 2020-3-17 加 读者信息变更
        public const string C_Template_PatronInfoChanged = "PatronInfoChanged";
             */



        }



    }

    public class LabelValue
    {
        public LabelValue(string strLable,string strValue) {
            this.lable = strLable;
            this.value= strValue; 
        }
        public string lable { get; set; }
        public string value { get; set;}
    }

}
