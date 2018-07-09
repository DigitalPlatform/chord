using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using MongoDB.Driver;

using DigitalPlatform.Text;

namespace DigitalPlatform.MessageServer
{
    /// <summary>
    /// 通用的集合查询辅助类
    /// </summary>
    public class CollectionQuery
    {
        public string Type { get; set; }    // [ 或者 (
        public List<string> Names { get; set; }
        public string Definition { get; set; }  // 附加的定义部分。and/or。默认 or

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
            string default_type = "(")
        {
            CollectionQuery query = new CollectionQuery(text, default_type);
            return query.BuildMongoQuery(field);
        }

        // parameters:
        //      strText 例如 [xxxx,xxxx] 或 (xxxx,xxxx)
        //              xxxx 部分内容，需要利用 StringUtil.EscapeString() 函数进行转义，以保护里面的特殊字符。用法如下：
        //              EscapeString(strText, "[](),|")
        public CollectionQuery(string strText, string default_type)
        {
            if (strText == null)
                throw new ArgumentException("strText 参数值不应为空", "strText");

            strText = strText.Trim();
            if (strText == "")
                throw new ArgumentException("strText 参数值不应为空", "strText");

            if (default_type == "[" || default_type == "(")
            {

            }
            else
                throw new ArgumentException("default_type 参数值应为 '[' '(' 之一", "default_type");

            this.DefaultType = default_type;

            string strDefinition = "";
            List<string> array1 = StringUtil.ParseTwoPart(strText, "|");
            if (string.IsNullOrEmpty(array1[1]))
            {

            }
            else
            {
                strText = array1[0];
                strDefinition = array1[1];
            }

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
                this.Names.Add(StringUtil.UnescapeString(name));
            }

            this.Definition = strDefinition;
        }

        // parameters:
        //      names   这个数组里面的每个字符串元素，不需要预先转义。就是说里面允许出现逗号等特殊符号
        public CollectionQuery(string[] names, string definition)
        {
            this.Names = new List<string>();
            foreach (string name in names)
            {
                this.Names.Add(name);
            }

            this.Definition = definition;
        }

        public string[] ToStringArray()
        {
            return this.Names.ToArray();
        }

        public FilterDefinition<MessageItem> BuildMongoQuery(string field)
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
            if (this.Definition.ToLower() == "and")
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
