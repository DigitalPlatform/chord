using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using MongoDB.Driver;

using DigitalPlatform.Text;

namespace DigitalPlatform.MessageServer
{
    /// <summary>
    /// 解析好的 群组检索式 中间结构。便于快速处理
    /// </summary>
    public class GroupQuery
    {
        public List<GroupSegment> Segments { get; set; }

        public GroupQuery(string strText)
        {
            this.Segments = new List<GroupSegment>();
            // 首先切割为一个一个段落
            string[] segments = strText.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string segment in segments)
            {
                this.Segments.Add(new GroupSegment(segment));
            }
        }

        public static GroupQuery Build(string strText)
        {
            GroupQuery query = new GroupQuery(strText);
            return query;
        }

        // 返回空表示任意匹配
        public FilterDefinition<MessageItem> BuildMongoQuery()
        {
            if (this.Segments.Count == 0)
                return null;

            if (this.Segments.Count == 1)
                return this.Segments[0].BuildMongoQuery();

            // 若干个需要 OR 的单独检索式
            List<FilterDefinition<MessageItem>> querys = new List<FilterDefinition<MessageItem>>();

            foreach (GroupSegment segment in this.Segments)
            {
                querys.Add(segment.BuildMongoQuery());
            }

            return Builders<MessageItem>.Filter.Or(querys);
        }

        // 变换为适于保存到数据库中 MessageItem.groups 的形态
        public bool Canonicalize(Delegate_replaceName proc_replace)
        {
            bool bChanged = false;

            foreach (GroupSegment segment in this.Segments)
            {
                if (segment.Canonicalize(proc_replace))
                    bChanged = true;
            }

            return bChanged;
        }

        // 将 MessageItem.groups 变换为适于显示的形态
        public bool Displaylize(Delegate_replaceName proc_replace)
        {
            bool bChanged = false;

            foreach (GroupSegment segment in this.Segments)
            {
                if (segment.Displaylize(proc_replace))
                    bChanged = true;
            }

            return bChanged;
        }

        public string ToStringUnQuote()
        {
            StringBuilder text = new StringBuilder();
            foreach (GroupSegment segment in this.Segments)
            {
                if (text.Length > 0)
                    text.Append(";");
                text.Append(segment.ToStringUnQuote());
            }
            return text.ToString();
        }
    }

    // 表示一个分段。若干分段之间用 OR 关系连接起来
    public class GroupSegment
    {
        public string Type { get; set; }    // [ 或者 (
        public List<GroupName> Names { get; set; }
        public string Definition { get; set; }  // 附加的定义部分

        string _defaultNameType = "gn";
        public string DefaultNameType
        {
            get
            {
                return _defaultNameType;
            }
            set
            {
                _defaultNameType = value;
            }
        }

        // parameters:
        //      strText 例如 [id:xxxx,id:xxxx]|definition
        public GroupSegment(string strText, string strDefaultNameType = "gn")
        {
            if (strText == null)
                throw new ArgumentException("strText 参数值不应为空", "strText");

            strText = strText.Trim();
            if (strText == "")
                throw new ArgumentException("strText 参数值不应为空", "strText");

            this.DefaultNameType = strDefaultNameType;

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
                this.Type = "[";    // 默认

            this.Names = new List<GroupName>();

            // 首先切割为一个一个段落
            string[] names = strText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string name in names)
            {
                this.Names.Add(GroupName.Build(name, DefaultNameType));
            }

            this.SortNames();
            this.Definition = strDefinition;
        }

        public GroupSegment(string[] names, string definition)
        {
            this.Names = new List<GroupName>();
            foreach (string name in names)
            {
                this.Names.Add(GroupName.Build(name, DefaultNameType));
            }

            this.Definition = definition;
        }

        public string[] ToStringArray()
        {
            List<string> results = new List<string>();
            foreach (GroupName name in this.Names)
            {
                results.Add(name.ToString());
            }
            return results.ToArray();
        }


        public List<string> ToStringList()
        {
            List<string> results = new List<string>();
            foreach (GroupName name in this.Names)
            {
                results.Add(name.ToString());
            }
            return results;
        }

        // 检查正规化以后的组名是否符合要求
        void CheckGroupNameUnion()
        {
            if (this.Names.Count == 1)
                return;

            // 多个组合的情况，要求每个部分必须都是 ui type (这是多人讨论组的形态)。这样就杜绝了多个群名组合的情况。
            foreach (GroupName name in this.Names)
            {
                if (name.Type != "ui")
                    throw new ArgumentException("群名不合法。出现了不是 ui 类型的部分: " + this.ToString());
            }
        }

        // 将 MessageItem.groups 变换为适于显示的形态
        public bool Displaylize(Delegate_replaceName proc_replace)
        {
            bool bChanged = false;
            // 把 gn 替换为 gi
            // 把 un 替换为 ui
            foreach (GroupName name in this.Names)
            {
                if (name.Type == "gi" || name.Type == "ui")
                {
                    GroupName result_name = proc_replace(name);
                    name.Text = result_name.Text;
                    name.Type = result_name.Type;
                    bChanged = true;
                }
            }

            return bChanged;
        }

        // 变换为适于保存到数据库中 MessageItem.groups 的形态
        public bool Canonicalize(Delegate_replaceName proc_replace)
        {
            bool bChanged = false;
            // 把 gn 替换为 gi
            // 把 un 替换为 ui
            foreach (GroupName name in this.Names)
            {
                if ((name.Type == "gn" && (name.Text[0] != '<' && name.Text[0] != '_'))
                    || name.Type == "un")
                {
                    GroupName result_name = proc_replace(name);
                    name.Text = result_name.Text;
                    name.Type = result_name.Type;
                    bChanged = true;
                }
            }

            if (bChanged)
            {
                this.SortNames();
                CheckGroupNameUnion();
                return true;
            }

            CheckGroupNameUnion();
            return false;
        }

        // 变换为适于保存到数据库中 MessageItem.groups 的形态
        public static string[] Canonicalize(string[] names,
            Delegate_replaceName proc_replace,
            bool bCheckUnion = true)
        {
            GroupSegment segment = new GroupSegment(names, "");

            // 把 gn 替换为 gi
            // 把 un 替换为 ui
            foreach (GroupName name in segment.Names)
            {
                if ((name.Type == "gn" && (name.Text[0] != '<' && name.Text[0] != '_'))
                    || name.Type == "un")
                {
                    GroupName result_name = proc_replace(name);
                    name.Text = result_name.Text;
                    name.Type = result_name.Type;
                }
            }

            if (bCheckUnion)
            {
                segment.SortNames();
                segment.CheckGroupNameUnion();
            }

            return segment.ToStringArray();
        }

        // 变换为适于保存到数据库中 MessageItem.groups 的形态
        public static List<string> Canonicalize(List<string> names,
            Delegate_replaceName proc_replace,
            bool bCheckUnion = true)
        {
            GroupSegment segment = new GroupSegment(names.ToArray(), "");

            // 把 gn 替换为 gi
            // 把 un 替换为 ui
            foreach (GroupName name in segment.Names)
            {
                if ((name.Type == "gn" && (name.Text[0] != '<' && name.Text[0] != '_'))
                    || name.Type == "un")
                {
                    GroupName result_name = proc_replace(name);
                    name.Text = result_name.Text;
                    name.Type = result_name.Type;
                }
            }

            if (bCheckUnion)
            {
                segment.SortNames();
                segment.CheckGroupNameUnion();
            }

            return segment.ToStringList();
        }

        // 把下级元素按照名字排序
        public void SortNames()
        {
            this.Names.Sort((x, y) =>
            {
                return string.Compare(x.ToString(), y.ToString());
            });
        }

        public FilterDefinition<MessageItem> BuildMongoQuery()
        {
            // 精确包含
            if (this.Type == "[")
            {
                List<string> names = new List<string>();
                foreach (GroupName name in this.Names)
                {
                    names.Add(name.ToString());
                }

                return Builders<MessageItem>.Filter.Eq("groups", names);
            }

            // 至少包含

            if (this.Names.Count == 1)
                return Builders<MessageItem>.Filter.Eq("groups", this.Names[0].ToString());

            // 包含
            // 构造一个 AND 运算的检索式
            List<FilterDefinition<MessageItem>> subs = new List<FilterDefinition<MessageItem>>();
            foreach (GroupName name in this.Names)
            {
                subs.Add(Builders<MessageItem>.Filter.Eq("groups", name.ToString()));
            }
            return Builders<MessageItem>.Filter.And(subs);
        }

        // 获得纯粹的名字。没有括号部分。例如 id:xxxxx,id:xxxxx|definiton
        public string ToStringUnQuote()
        {
            StringBuilder text = new StringBuilder();
            foreach (GroupName name in this.Names)
            {
                if (text.Length > 0)
                    text.Append(",");
                text.Append(name.ToString());
            }

            if (string.IsNullOrEmpty(this.Definition) == false)
                text.Append("|" + this.Definition);

            return text.ToString();
        }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            foreach (GroupName name in this.Names)
            {
                if (text.Length > 0)
                    text.Append(",");
                text.Append(name.ToString());
            }
            text.Insert(0, this.Type);
            if (this.Type == "[")
                text.Append("]");
            else
            {
                Debug.Assert(this.Type == "(", "");
                text.Append(")");
            }

            if (string.IsNullOrEmpty(this.Definition) == false)
                text.Append("|" + this.Definition);

            return text.ToString();
        }
    }

    // 表示一个名字
    public class GroupName
    {
        public string Type { get; set; }    // gn/gi/un/ui
        public string Text { get; set; }

        public GroupName(string strType, string strText)
        {
            this.Set(strType, strText);
        }

        // parameters:
        //      strType 类型
        //      strText 名字部分
        public void Set(string strType, string strText)
        {
            this.Type = strType;
            this.Text = strText;

            // 检查合法性
            CheckType();
            CheckText();
        }

        void CheckText()
        {
            if (this.Text.IndexOfAny(new char[] { ',', ':', ';' }) != -1)
                throw new ArgumentException("strText '" + this.Text + "' 中出现了非法符号");
        }

        void CheckType()
        {
            if (this.Type == "gn" || this.Type == "gi"
    || this.Type == "un" || this.Type == "ui")
            {

            }
            else
                throw new ArgumentException("未知的名称类型 '" + this.Type + "'");
        }

        // parameters:
        //      strType 类型、名字
        public static GroupName Build(string strText, string strDefaultType = "gn")
        {
            if (strText == null)
                throw new ArgumentException("strText 参数值不应为 null", "strText");

            strText = strText.Trim();
            if (strText == "")
                throw new ArgumentException("strText 参数值不应为空", "strText");

            string strType = "";
            string strName = "";
            List<string> array = StringUtil.ParseTwoPart(strText, ":");
            if (string.IsNullOrEmpty(array[1]))
            {
                strType = strDefaultType;   //  "gn";
                strName = array[0];
            }
            else
            {
                strType = array[0];
                strName = array[1];
            }

            GroupName result = new GroupName(strType, strName);
            return result;
            // this.Set(strType, strName);
        }

        public override string ToString()
        {
            return this.Type + ":" + this.Text;
        }
    }

    public delegate GroupName Delegate_replaceName(GroupName name);

}
