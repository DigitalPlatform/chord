using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.MessageServer
{
    // 管理 群名字:-n-m
    public class GroupDefinition
    {
        public string GroupName = "";
        public string Definition = "";  // -n-m 部分

        // 构造
        public GroupDefinition(string strText)
        {
            List<string> strings = StringUtil.ParseTwoPart(strText, ":");
            this.GroupName = strings[0];
            if (this.GroupName != null)
                this.GroupName = this.GroupName.ToLower();

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
        public static string GetName(string strText)
        {
            var def = GroupDefinition.Build(strText);
            return def.GroupName;
        }

        // 判断一个组名是否为默认的组名
        public static bool IsDefaultGroupName(string groupName)
        {
            var def = GroupDefinition.Build(groupName);

            if (def.GroupName == "<default>")
                return true;
            return false;
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
                if (def.GroupName == def_one.GroupName)
                    return true;
            }
            return false;
        }

    }
}
