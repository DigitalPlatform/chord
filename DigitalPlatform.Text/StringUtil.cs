using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.Text
{
    /// <summary>
    /// 字符串实用函数
    /// </summary>
    public static class StringUtil
    {
        public static long TryGetSubInt64(string strText,
    char seperator,
    int index,
    long default_value = 0)
        {
            try
            {
                return GetSubInt64(strText, seperator, index, default_value);
            }
            catch
            {
                return default_value;
            }
        }

        // exception:
        //      抛出 Int64.Parse() 要抛出的那些异常
        public static long GetSubInt64(string strText, 
            char seperator, 
            int index, 
            long default_value = 0)
        {
            string str_value = GetSubString(strText, seperator, index);
            if (string.IsNullOrEmpty(str_value) == true)
                return default_value;

            return Int64.Parse(str_value);
        }

        public static string GetSubString(string strText, char seperator, int index)
        {
            string[] parts = strText.Split(new char[] { seperator });
            if (index >= parts.Length)
                return null;
            return parts[index];
        }

        public static List<string> GetStringList(string strText,
            char delimeter)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return new List<string>();

            string[] parts = strText.Split(new char[] { delimeter });
            List<string> results = new List<string>();
            results.AddRange(parts);
            return results;
        }

        /// <summary>
        /// 检测一个列表字符串是否包含一个具体的值
        /// </summary>
        /// <param name="strList">列表字符串。用逗号分隔多个子串</param>
        /// <param name="strOne">要检测的一个具体的值</param>
        /// <returns>false 没有包含; true 包含</returns>
        public static bool Contains(string strList, string strOne, char delimeter = ',')
        {
            if (string.IsNullOrEmpty(strList) == true)
                return false;
            string[] list = strList.Split(new char[] { delimeter }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in list)
            {
                if (strOne == s)
                    return true;
            }

            return false;
        }

        // parameters:
        //      strPrefix 前缀。例如 "getreaderinfo:"
        // return:
        //      null    没有找到前缀
        //      ""      找到了前缀，并且值部分为空
        //      其他     返回值部分
        public static string GetParameterByPrefix(string strList, string strPrefix)
        {
            if (string.IsNullOrEmpty(strList) == true)
                return "";
            string[] list = strList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in list)
            {
                if (s.StartsWith(strPrefix) == true)
                    return s.Substring(strPrefix.Length);
            }

            return null;
        }
    }
}
