using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Common
{
    public static class PropertyExtension
    {
        public static void SetPropertyValue(this object obj, string propName, object value)
        {
            // obj.GetType().GetProperty(propName).SetValue(obj, value, null);
            obj.GetType().GetProperty(propName).SetValue(obj, value);
        }

        public static Type GetPropertyType(this object obj, string propName)
        {
            return obj.GetType().GetProperty(propName).PropertyType;
        }
    }
}
