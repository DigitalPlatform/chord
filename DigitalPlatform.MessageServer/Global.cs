using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.MessageServer
{
    /// <summary>
    /// 一些实用函数
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// 检测一个列表字符串是否包含一个具体的值
        /// </summary>
        /// <param name="strList">列表字符串。用逗号分隔多个子串</param>
        /// <param name="strOne">要检测的一个具体的值</param>
        /// <returns>false 没有包含; true 包含</returns>
        public static bool Contains(string strList, string strOne)
        {
            if (string.IsNullOrEmpty(strList) == true)
                return false;
            string[] list = strList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string s in list)
            {
                if (strOne == s)
                    return true;
            }

            return false;
        }
    }
}
