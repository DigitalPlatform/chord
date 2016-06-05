using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Interfaces
{
    public class ScriptManager
    {
        // 获得从指定类或者接口派生的类
        public static Type GetDerivedClassType(Assembly assembly,
            string strBaseTypeFullName)
        {
            if (assembly == null)
                return null;

            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.IsClass == false)
                    continue;

                // 2015/5/28
                Type[] interfaces = type.GetInterfaces();
                foreach (Type inter in interfaces)
                {
                    if (inter.FullName == strBaseTypeFullName)
                        return type;
                }

                if (IsDerivedFrom(type,
                    strBaseTypeFullName) == true)
                    return type;
            }

            return null;
        }

        // 观察type的基类中是否有类名为strBaseTypeFullName的类。
        public static bool IsDerivedFrom(Type type,
            string strBaseTypeFullName)
        {
            Type curType = type;
            for (; ; )
            {
                if (curType == null
                    || curType.FullName == "System.Object")
                    return false;

                if (curType.FullName == strBaseTypeFullName)
                    return true;

                curType = curType.BaseType;
            }

        }
    }
}
