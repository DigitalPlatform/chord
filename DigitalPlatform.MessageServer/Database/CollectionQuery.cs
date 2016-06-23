using DigitalPlatform.Text;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.MessageServer
{
    /// <summary>
    /// 通用的集合查询辅助类
    /// </summary>
    public class CollectionQuery
    {
        public string Type { get; set; }    // [ 或者 (
        public List<string> Names { get; set; }

        string _defaultType = "(";
        public string DefaultType
        {
            get
            {
                return _defaultType;
            }
            set
            {
                _defaultType = value;
            }
        }

        public static FilterDefinition<MessageItem> BuildMongoQuery(string text,
            string field,
            string style,
            string default_type = "(")
        {
            CollectionQuery query = new CollectionQuery(text, default_type);
            return query.BuildMongoQuery(field, style);
        }

        // parameters:
        //      strText 例如 [xxxx,xxxx] 或 (xxxx,xxxx)
        public CollectionQuery(string strText, string default_type)
        {
            if (strText == null)
                throw new ArgumentException("strText 参数值不应为空", "strText");

            strText = strText.Trim();
            if (strText == "")
                throw new ArgumentException("strText 参数值不应为空", "strText");

            this.DefaultType = default_type;

            // 去掉外围的括号
            if (strText[0] == '[' || strText[0] == '(')
            {
                this.Type = strText.Substring(0, 1);
                strText = StringUtil.Unquote(strText, "[]()");
            }
            else
                this.Type = this.DefaultType;    // 默认

            this.Names = new List<string>();

            // 首先切割为一个一个段落
            string[] names = strText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string name in names)
            {
                this.Names.Add(name);
            }
        }

        public CollectionQuery(string[] names)
        {
            this.Names = new List<string>();
            foreach (string name in names)
            {
                this.Names.Add(name);
            }
        }

        public string[] ToStringArray()
        {
            return this.Names.ToArray();
        }

        public FilterDefinition<MessageItem> BuildMongoQuery(string field,
            string style)
        {
            // 精确包含
            if (this.Type == "[")
            {
                if (this.Names.Count == 0)
                {
                    return Builders<MessageItem>.Filter.Or(
                        Builders<MessageItem>.Filter.Eq(field, this.Names),
                        Builders<MessageItem>.Filter.Eq(field, (string[])null)
                        );
                }
                return Builders<MessageItem>.Filter.Eq(field, this.Names);
            }

            // 至少包含
            if (this.Names.Count == 1)
                return Builders<MessageItem>.Filter.Eq(field, this.Names[0]);

            // 包含
            // 构造一个 AND 运算的检索式
            List<FilterDefinition<MessageItem>> subs = new List<FilterDefinition<MessageItem>>();
            foreach (string name in this.Names)
            {
                subs.Add(Builders<MessageItem>.Filter.Eq(field, name));
            }
            if (style.ToLower() == "and")
                return Builders<MessageItem>.Filter.And(subs);
            return Builders<MessageItem>.Filter.Or(subs);
        }

        // 获得纯粹的名字。没有括号部分。例如 id:xxxxx,id:xxxxx|definiton
        public string ToStringUnQuote()
        {
            StringBuilder text = new StringBuilder();
            foreach (string name in this.Names)
            {
                if (text.Length > 0)
                    text.Append(",");
                text.Append(name);
            }

            return text.ToString();
        }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            foreach (string name in this.Names)
            {
                if (text.Length > 0)
                    text.Append(",");
                text.Append(name);
            }
            text.Insert(0, this.Type);
            if (this.Type == "[")
                text.Append("]");
            else
            {
                Debug.Assert(this.Type == "(", "");
                text.Append(")");
            }

            return text.ToString();
        }
    }
}
