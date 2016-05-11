using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.MessageServer
{
    // 管理群名定义字符串
    // 群名字:-n-m
    public class GroupDefinition
    {
        public List<string> GroupNames = new List<string>();
        public string Definition = "";  // -n-m 部分

        public string GroupNameString
        {
            get
            {
                return string.Join(",", this.GroupNames);
            }
        }

        // 构造
        // parameters:
        //      strText gn:name|definition 或者 ui:xxx,ui:xxxx|definition
        //          其中左边部分应该已经被正规化
        public GroupDefinition(string strText)
        {
            List<string> strings = StringUtil.ParseTwoPart(strText, "|");
            this.GroupNames = StringUtil.SplitList(strings[0]);
            for (int i = 0; i < this.GroupNames.Count; i++)
            {
                this.GroupNames[i] = this.GroupNames[i].ToLower();
            }

            this.GroupNames.Sort(); // 排序以后，用逗号连接，可以作为规范的名字使用

            this.Definition = strings[1];
        }

        // 建造一个对象
        public static GroupDefinition Build(string strText)
        {
            GroupDefinition def = new GroupDefinition(strText);
            return def;
        }

        // 获得定义部分
        public static string GetDefinition(string strText)
        {
            var def = GroupDefinition.Build(strText);
            return def.Definition;
        }

        // 获得名字部分
        public static List<string> GetName(string strText)
        {
            var def = GroupDefinition.Build(strText);
            return def.GroupNames;
        }

        // 判断一个组名是否为默认的组名
        // TODO: 函数名似乎修改为 ContainsDefaultGroupName 较好
        public static bool IsDefaultGroupName(string groupName)
        {
            var def = GroupDefinition.Build(groupName);

            return (def.GroupNames.IndexOf("<default>") != -1);
        }

        // 判断一个组名是否为默认的组名
        // TODO: 函数名似乎修改为 ContainsDefaultGroupName 较好
        public static bool IsDefaultGroupName(List<string> names)
        {
            if (names == null || names.Count != 1)
                return false;

            var def = GroupDefinition.Build(names[0]);

            return (def.GroupNames.IndexOf("<default>") != -1);
        }

        // 判断一个组名是否为默认的组名
        // TODO: 函数名似乎修改为 ContainsDefaultGroupName 较好
        public static bool IsDefaultGroupName(string[] names)
        {
            if (names == null || names.Length != 1)
                return false;

            var def = GroupDefinition.Build(names[0]);

            return (def.GroupNames.IndexOf("<default>") != -1);
        }

        // 探测 one 是否包含在 array 列表中。
        // parameters:
        //      one 可能是不纯粹的群组名
        public static bool IncludeGroup(string[] array, string one)
        {
            if (array == null)
            {
                if (IsDefaultGroupName(one) == true)
                    return true;
                return false;
            }

            // return Array.IndexOf(array, one) != -1;

            var def_one = GroupDefinition.Build(one);

            foreach (string s in array)
            {
                var def = GroupDefinition.Build(s);
                if (Equal(def.GroupNames, def_one.GroupNames))
                    return true;
            }
            return false;
        }

        // one -- MessageItem 里面的 groups 成员
        public static bool IncludeGroup(string[] array, string [] one)
        {
            if (array == null)
            {
                if (IsDefaultGroupName(one) == true)
                    return true;
                return false;
            }

            // return Array.IndexOf(array, one) != -1;

            var def_one = GroupDefinition.Build(string.Join(",", one));

            foreach (string s in array)
            {
                var def = GroupDefinition.Build(s);
                if (Equal(def.GroupNames, def_one.GroupNames))
                    return true;
            }
            return false;
        }

        // 观察两个集合的元素是否相等
        static bool Equal(List<string> list1, List<string> list2)
        {
            if (list1.Count != list2.Count)
                return false;
            //list1.Sort();
            //list2.Sort();
            for(int i=0;i<list1.Count;i++)
            {
                if (list1[i] != list2[i])
                    return false;
            }

            return true;
        }

    }
}
