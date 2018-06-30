using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace DigitalPlatform.Text
{
    /// <summary>
    /// 字符串实用函数
    /// </summary>
    public static class StringUtil
    {

        public static bool IsHttpUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;
            url = url.ToLower();
            if (url.StartsWith("http:") || url.StartsWith("https"))
                return true;
            return false;
        }

        // 获得较短一点的 GUID 字符串
        public static string GetShortGuid()
        {
            Guid guid = Guid.NewGuid();
            byte[] bytes = guid.ToByteArray();
            return Convert.ToBase64String(bytes);
        }

        // 去掉列表中的空字符串，并且去掉每个元素的首尾空白
        public static void RemoveBlank(ref List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string strText = list[i].Trim();
                if (string.IsNullOrEmpty(strText) == true)
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
                if (strText != list[i])
                    list[i] = strText;
            }
        }

        // 把一个字符串数组去重。调用前，不要求已经排序
        public static void RemoveDupNoSort(ref List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string strItem = list[i];
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (strItem == list[j])
                    {
                        list.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        // 把一个字符串数组去重。调用前，应当已经排序
        public static void RemoveDup(ref List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string strItem = list[i];
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (strItem == list[j])
                    {
                        list.RemoveAt(j);
                        j--;
                    }
                    else
                    {
                        i = j - 1;
                        break;
                    }
                }
            }
        }

        public static string EscapeString(string strText, string speical_chars)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            StringBuilder text = new StringBuilder();
            foreach (char ch in strText)
            {
                if (ch != '%' && speical_chars.IndexOf(ch) == -1)
                    text.Append(ch);
                else
                    text.Append(Uri.HexEscape(ch));
            }

            return text.ToString();
        }

        public static string UnescapeString(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            StringBuilder result = new StringBuilder();
            int len = strText.Length;
            int i = 0;
            while (i < len)
            {
                if (Uri.IsHexEncoding(strText, i))
                    result.Append(Uri.HexUnescape(strText, ref i));
                else
                    result.Append(strText[i++]);
            }

            return result.ToString();
        }

        // 去掉外围括住的符号
        // parameters:
        //      pairs   若干对打算去除的符号。例如 "%%" "()" "[](){}"。如果包含多对符号，则从左到右匹配，用上前面的就用它处理然后返回了，后面的若干对就不发生作用了
        public static string Unquote(string strValue, string pairs)
        {
            if (string.IsNullOrEmpty(pairs))
                throw new ArgumentException("pairs 参数值不应为空", "pairs");

            if ((pairs.Length % 2) != 0)
                throw new ArgumentException("pairs 参数值的字符个数应为偶数", "pairs");

            if (string.IsNullOrEmpty(strValue) == true)
                return "";

            for (int i = 0; i < pairs.Length / 2; i++)
            {
                char left = pairs[i * 2];
                if (strValue[0] == left)
                {
                    strValue = strValue.Substring(1);
                    if (strValue.Length == 0)
                        return "";

                    char right = pairs[(i * 2) + 1];
                    if (strValue[strValue.Length - 1] == right)
                        return strValue.Substring(0, strValue.Length - 1);
                }
            }

            return strValue;
        }

        public static string GetMd5(string strText)
        {
#if NO
            MD5 hasher = MD5.Create();
            byte[] buffer = Encoding.UTF8.GetBytes(strText);
            byte[] target = hasher.ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in target)
            {
                sb.Append(b.ToString("x2").ToLower());
            }

            return sb.ToString();
#endif
            return GetMd5(Encoding.UTF8.GetBytes(strText));
        }

        public static string GetMd5(byte [] buffer)
        {
            MD5 hasher = MD5.Create();
            byte[] target = hasher.ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in target)
            {
                sb.Append(b.ToString("x2").ToLower());
            }

            return sb.ToString();
        }

        // 将权限列表字符串切割为单个元素构成的数组
        public static List<string> SplitRights(string strList)
        {
            List<string> results = new List<string>();
            if (string.IsNullOrEmpty(strList))
                return results;

            StringBuilder one = new StringBuilder();
            foreach (char ch in strList)
            {
                if (ch == '+' || ch == '-')
                {
                    if (one.Length > 0)
                    {
                        results.Add(one.ToString());
                        one.Clear();
                    }
                }

                one.Append(ch);
            }

            if (one.Length > 0)
            {
                results.Add(one.ToString());
                one.Clear();
            }

            return results;
        }

        // 检测权限 strRight 是否包含在列表字符串 strList 中
        // parameters:
        //      strRight  要检测的单个权限。单个权限形态为 'a'
        //      strList 列表字符串。形态为 +a-b-b
        // return:
        //      -1  包含了 -
        //      0   + - 都没有出现过
        //      1   包含了 +
        public static int ContainsRight(string strList, string strRight)
        {
            List<string> rights = SplitRights(strList);
            int on = 0;
            foreach (string one in rights)
            {
                char ch = one[0];
                if (one.Substring(1) == strRight)
                {
                    if (ch == '+')
                        on = 1;
                    else
                    {
                        if (ch != '-')
                            throw new ArgumentException("单个权限值 '" + one + "' 在列表中 '" + strList + "' 不合法");
                        on = -1;
                    }
                }
            }

            return on;
        }

        public static List<string> ParseTwoPart(string strText, string strSep)
        {
            string strLeft = "";
            string strRight = "";
            ParseTwoPart(strText, strSep, out strLeft, out strRight);
            List<string> results = new List<string>();
            results.Add(strLeft);
            results.Add(strRight);
            return results;
        }

        /// <summary>
        /// 通用的，切割两个部分的函数
        /// </summary>
        /// <param name="strText">要处理的字符串</param>
        /// <param name="strSep">分隔符号</param>
        /// <param name="strPart1">返回第一部分</param>
        /// <param name="strPart2">返回第二部分</param>
        public static void ParseTwoPart(string strText,
            string strSep,
            out string strPart1,
            out string strPart2)
        {
            strPart1 = "";
            strPart2 = "";

            if (string.IsNullOrEmpty(strText) == true)
                return;

            int nRet = strText.IndexOf(strSep);
            if (nRet == -1)
            {
                strPart1 = strText;
                return;
            }

            strPart1 = strText.Substring(0, nRet).Trim();
            strPart2 = strText.Substring(nRet + strSep.Length).Trim();
        }

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
        //      strPrefix 前缀。例如 "getreaderinfo"
        //      strDelimiter    前缀和后面参数的分隔符号。例如 ":"
        // return:
        //      null    没有找到前缀
        //      ""      找到了前缀，并且值部分为空
        //      其他     返回值部分
        public static string GetParameterByPrefix(string strList,
            string strPrefix,
            string strDelimiter = ":")
        {
            if (string.IsNullOrEmpty(strList) == true)
                return null;
            string[] list = strList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in list)
            {
                if (s.StartsWith(strPrefix + strDelimiter) == true)
                    return s.Substring(strPrefix.Length + strDelimiter.Length);
                if (s == strPrefix)
                    return "";
            }

            return null;
        }

        // 查找符合特定前缀的参数。增强版本，能处理 :username, operation:username, 这样的形态
        // parameters:
        //      strPrefix 前缀。例如 "getreaderinfo"
        //      strDelimiter    前缀和后面参数的分隔符号。例如 ":"
        // return:
        //      null    没有找到前缀
        //      ""      找到了前缀，并且值部分为空
        //      其他     返回值部分
        public static string GetParameterByPrefixEnvironment(string strList,
            string strPrefix,
            string strDelimiter = ":")
        {
            if (string.IsNullOrEmpty(strList) == true)
                return null;
            string environment = "";    // 当前环境值
            string[] list = strList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in list)
            {
                if (s.StartsWith(strDelimiter) == true)
                {
                    environment = s.Substring(strDelimiter.Length);
                    continue;
                }
                if (s.StartsWith(strPrefix + strDelimiter) == true)
                {
                    string value = s.Substring(strPrefix.Length + strDelimiter.Length);
                    if (string.IsNullOrEmpty(value) == false)
                        return value;
                    return environment;
                }
                if (s == strPrefix)
                    return environment;
            }

            return null;
        }

        // 从 binding 字符串中寻找特定名字的 binding
        public static string GetOneBinding(string strText, string strName)
        {
            // return:
            //      null    没有找到前缀
            //      ""      找到了前缀，并且值部分为空
            //      其他     返回值部分
            return GetParameterByPrefix(strText,
                strName,
                ":");
        }

        // 匹配 ip 地址列表
        // parameters:
        //      strList IP 地址列表。例如 localhost|192.168.1.1|192.168.*.*
        //      strIP   要检测的一个 IP 地址
        public static bool MatchIpAddressList(string strList, string strIP)
        {
            string[] list = strList.Split(new char[] {'|'});
            foreach(string pattern in list)
            {
                if (MatchIpAddress(pattern, strIP) == true)
                    return true;
            }

            return false;
        }

        public static bool MatchIpAddress(string pattern, string ip)
        {
            ip = CanonicalizeIP(ip);
            pattern = CanonicalizeIP(pattern);

            if (pattern == ip)
                return true;
            return false;
        }

        // 正规化 IP 地址
        public static string CanonicalizeIP(string ip)
        {
            if (ip == "::1" || ip == "127.0.0.1")
                return "localhost";
            return ip;
        }

        //===================
        // 任延华 2015-12-22 加

        public static string MakePathList(List<string> aPath,
            string strSep)
        {
            // 2012/9/7
            if (aPath.Count == 0)
                return "";

            string[] pathlist = new string[aPath.Count];
            aPath.CopyTo(pathlist);

            return String.Join(strSep, pathlist);
        }

        // 得到用16进制表示的时间戳字符串
        public static string GetHexTimeStampString(byte[] baTimeStamp)
        {
            if (baTimeStamp == null)
                return "";
            string strText = "";
            for (int i = 0; i < baTimeStamp.Length; i++)
            {
                string strHex = Convert.ToString(baTimeStamp[i], 16);
                strText += strHex.PadLeft(2, '0');
            }

            return strText;
        }

        // 得到byte[]类型的时间戳
        public static byte[] GetTimeStampByteArray(string strHexTimeStamp)
        {
            if (strHexTimeStamp == "")
                return null;

            byte[] result = new byte[strHexTimeStamp.Length / 2];

            for (int i = 0; i < strHexTimeStamp.Length / 2; i++)
            {
                string strHex = strHexTimeStamp.Substring(i * 2, 2);
                result[i] = Convert.ToByte(strHex, 16);

            }

            return result;
        }

        // 将 "string1,string2" 按照逗号切割为 List<string>
        public static List<string> SplitList(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return new List<string>();

            string[] parts = strText.Split(new char[] { ',' });
            return parts.ToList();
        }

        // 检测一个字符串的头部
        public static bool HasHead(string strText,
            string strHead,
            bool bIgnoreCase = false)
        {
            // 2013/9/11
            if (strText == null)
                strText = "";
            if (strHead == null)
                strHead = "";

            if (strText.Length < strHead.Length)
                return false;

            string strPart = strText.Substring(0, strHead.Length);  // BUG!!! strText.Substring(strHead.Length);

            // 2015/4/3
            if (bIgnoreCase == true)
            {
                if (string.Compare(strPart, strHead, true) == 0)
                    return true;
                return false;
            }

            if (strPart == strHead)
                return true;

            return false;
        }

        // 构造路径列表字符串，逗号分隔
        public static string MakePathList(List<string> aPath)
        {
            // 2012/9/7
            if (aPath.Count == 0)
                return "";

            string[] pathlist = new string[aPath.Count];
            aPath.CopyTo(pathlist);

            return String.Join(",", pathlist);
        }

        // 修改字符串某一个位字符
        public static string SetAt(string strText, int index, char c)
        {
            strText = strText.Remove(index, 1);
            strText = strText.Insert(index, new string(c, 1));

            return strText;
        }

        // 获取引导的{...}内容。注意返回值不包括花括号
        public static string GetLeadingCommand(string strLine)
        {
            if (string.IsNullOrEmpty(strLine) == true)
                return null;

            // 关注"{...}"
            if (strLine[0] == '{')
            {
                int nRet = strLine.IndexOf("}");
                if (nRet != -1)
                    return strLine.Substring(1, nRet - 1).Trim();
            }

            return null;
        }

        // 检测字符串是否为纯数字(前面可以包含一个'-'号)
        public static bool IsNumber(string s)
        {
            if (string.IsNullOrEmpty(s) == true)
                return false;

            bool bFoundNumber = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '-' && bFoundNumber == false)
                {
                    continue;
                }
                if (s[i] > '9' || s[i] < '0')
                    return false;
                bFoundNumber = true;
            }
            return true;
        }

        //比较字符串是否符合正则表达式
        public static bool RegexCompare(string strPattern,
            RegexOptions regOptions,
            string strInstance)
        {
            Regex r = new Regex(strPattern, regOptions);
            System.Text.RegularExpressions.Match m = r.Match(strInstance);

            if (m.Success)
                return true;
            else
                return false;
        }

        //===================

        // 2012/5/13
        public static string BuildParameterString(Hashtable table,
            char chSegChar = ',',
            char chEqualChar = '=',
            string strEncodeStyle = "")
        {
            StringBuilder result = new StringBuilder(4096);
            foreach (string key in table.Keys)
            {
                if (result.Length > 0)
                    result.Append(chSegChar);
                string strValue = (string)table[key];

                if (strEncodeStyle == "url")
                    result.Append(key + new string(chEqualChar, 1) + HttpUtility.UrlEncode(strValue));
                else
                    result.Append(key + new string(chEqualChar, 1) + strValue);
            }

            return result.ToString();
        }

        // 2014/8/23
        // 按照指定的 key 名字集合顺序和个数输出
        public static string BuildParameterString(Hashtable table,
            List<string> keys,
            char chSegChar = ',',
            char chEqualChar = '=',
            string strEncodeStyle = "")
        {
            StringBuilder result = new StringBuilder(4096);
            foreach (string key in keys)
            {
                if (result.Length > 0)
                    result.Append(chSegChar);
                string strValue = (string)table[key];

                if (strEncodeStyle == "url")
                    result.Append(key + new string(chEqualChar, 1) + HttpUtility.UrlEncode(strValue));
                else
                    result.Append(key + new string(chEqualChar, 1) + strValue);
            }

            return result.ToString();
        }

        // 2014/8/23
        // 将参数字符串内的参数排序
        public static string SortParams(string strParams,
            char chSegChar = ',',
            char chEqualChar = '=',
            string strEncodeStyle = "")
        {
            Hashtable table = StringUtil.ParseParameters(strParams, chSegChar, chEqualChar, strEncodeStyle);
            List<string> keys = new List<string>();
            foreach (string key in table.Keys)
            {
                keys.Add(key);
            }

            keys.Sort();
            return StringUtil.BuildParameterString(table, keys, chSegChar, chEqualChar, strEncodeStyle);
        }

        // 合并两个参数表
        // 如果有同名的参数，table2的会覆盖table1
        public static Hashtable MergeParametersTable(Hashtable table1, Hashtable table2)
        {
            Hashtable new_table = new Hashtable();
            foreach (string key in table1.Keys)
            {
                new_table[key] = table1[key];
            }
            foreach (string key in table2.Keys)
            {
                new_table[key] = table2[key];
            }
            return new_table;
        }

        // 将逗号间隔的参数表解析到Hashtable中
        // parameters:
        //      strText 字符串。形态如 "名1=值1,名2=值2"
        public static Hashtable ParseParameters(string strText)
        {
            return ParseParameters(strText, ',', '=');
        }

        // 将逗号间隔的参数表解析到Hashtable中
        // parameters:
        //      strText 字符串。形态如 "名1=值1,名2=值2"
        public static Hashtable ParseParameters(string strText,
            char chSegChar,
            char chEqualChar,
            string strDecodeStyle = "")
        {
            Hashtable results = new Hashtable();

            if (string.IsNullOrEmpty(strText) == true)
                return results;

            string[] parts = strText.Split(new char[] { chSegChar });   // ','
            for (int i = 0; i < parts.Length; i++)
            {
                string strPart = parts[i].Trim();
                if (String.IsNullOrEmpty(strPart) == true)
                    continue;
                string strName = "";
                string strValue = "";
                int nRet = strPart.IndexOf(chEqualChar);    // '='
                if (nRet == -1)
                {
                    strName = strPart;
                    strValue = "";
                }
                else
                {
                    strName = strPart.Substring(0, nRet).Trim();
                    strValue = strPart.Substring(nRet + 1).Trim();
                }

                if (String.IsNullOrEmpty(strName) == true
                    && String.IsNullOrEmpty(strValue) == true)
                    continue;

                if (strDecodeStyle == "url")
                    strValue = HttpUtility.UrlDecode(strValue);

                results[strName] = strValue;
            }

            return results;
        }

        /// <summary>
        /// 忽略大小写
        /// 查找一个小字符串是否包含在大字符串，
        /// 内部调isInAList函数
        /// </summary>
        /// <param name="strSub">小字符串</param>
        /// <param name="strList">大字符串</param>
        /// <returns>
        /// 1:包含
        /// 0:不包含
        /// </returns>
        public static bool IsInList(string strSub,
            string strList)
        {
            /*
            string[] aTemp;
            aTemp = strList.Split(new char[]{','});

            int nRet = strList.IndexOfAny(new char[]{' ','\t'});
            if (nRet != -1) 
            {
                for(int i=0;i<aTemp.Length;i++) {
                    aTemp[i] = aTemp[i].Trim();	// 去除左右空白
                }
            }
 
            return IsInAlist(strSub,aTemp);
            */
            return IsInList(strSub,
                strList,
                true);
        }

        // parameters:
        //		bIgnoreCase	是否忽略大小写
        public static bool IsInList(string strSub,
            string strList,
            bool bIgnoreCase)
        {
            if (String.IsNullOrEmpty(strList) == true)
                return false;	// 优化

            string[] aTemp;
            aTemp = strList.Split(new char[] { ',' });

            int nRet = strList.IndexOfAny(new char[] { ' ', '\t' });
            if (nRet != -1)
            {
                for (int i = 0; i < aTemp.Length; i++)
                {
                    aTemp[i] = aTemp[i].Trim();	// 去除左右空白
                }
            }

            return IsInAlist(strSub,
                aTemp,
                bIgnoreCase);
        }

        // TODO: 似乎可以用 IndexOf() 代替
        public static bool IsInList(int v, int[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (v == a[i])
                    return true;
            }

            return false;
        }

        // 合并两list，去重
        // parameters:
        //		bIgnoreCase	是否忽略大小写
        public static string MergeList(string strList1,
            string strList2,
            bool bIgnoreCase)
        {
            string[] items1 = strList1.Split(new char[] { ',' });

            // 去掉左右空白
            int nRet = strList1.IndexOfAny(new char[] { ' ', '\t' });
            if (nRet != -1)
            {
                for (int i = 0; i < items1.Length; i++)
                {
                    items1[i] = items1[i].Trim();	// 去除左右空白
                }
            }

            string[] items2 = strList2.Split(new char[] { ',' });

            // 去掉左右空白
            nRet = strList2.IndexOfAny(new char[] { ' ', '\t' });
            if (nRet != -1)
            {
                for (int i = 0; i < items2.Length; i++)
                {
                    items2[i] = items2[i].Trim();	// 去除左右空白
                }
            }

            // TODO: 改造为使用StringBuilder
            string strResult = "";
            for (int i = 0; i < items1.Length; i++)
            {
                for (int j = 0; j < items2.Length; j++)
                {
                    if (String.Compare(items1[i], items2[j], bIgnoreCase) == 0)
                        goto FOUND;
                }

                if (strResult != "")
                    strResult += ",";
                strResult += items1[i];
                continue;
            FOUND:
                continue;
            }

            for (int i = 0; i < items2.Length; i++)
            {
                if (strResult != "")
                    strResult += ",";
                strResult += items2[i];
            }

            return strResult;
        }

        /// <summary>
        /// 查找一个小字符串是否包含在一个字符串数组中
        /// </summary>
        /// <param name="strSub">小字符串</param>
        /// <param name="aList">字符串数组</param>
        /// <returns>
        /// 1:包含
        /// 0:不包含
        /// </returns>
        public static bool IsInAlist(string strSub,
            string[] aList)
        {
            /*
            for(int i=0;i<aList.Length;i++)
            {
                if (String.Compare(strSub,aList[i],true) == 0) 
                {
                    return true;
                }
            }
            return false;
            */
            return IsInAlist(strSub,
                aList,
                true);
        }

        // parameters:
        //      strSub      要比较的单个值。可以包含多个单独的值，用逗号连接。注：如果是多个值，则只要有一个匹配上，就返回true
        //		bIgnoreCase	是否忽略大小写
        public static bool IsInAlist(string strSub,
            string[] aList,
            bool bIgnoreCase)
        {
            // 2015/5/27
            if (string.IsNullOrEmpty(strSub) == true)
                return false;

            string[] sub_parts = strSub.Split(new char[] { ',' });

            // 2012/2/2 增加了处理strSub中包含多个值的能力
            foreach (string sub in sub_parts)
            {
                if (sub == null)
                    continue;

                string strOne = sub.Trim();
                if (string.IsNullOrEmpty(strOne) == true)
                    continue;

                for (int i = 0; i < aList.Length; i++)
                {
                    if (String.Compare(strOne, aList[i], bIgnoreCase) == 0)
                        return true;
                }
            }
            return false;
        }

        // 从逗号间隔的list中去除一个特定的style值。大小写不敏感
        // parameters:
        //      strSub  要去除的值列表。字符串中可以包含多个值。
        //      bRemoveMultiple	是否具有去除多个相同strSub值的能力。==false，只去除发现的第一个
        public static bool RemoveFromInList(string strSub,
            bool bRemoveMultiple,
            ref string strList)
        {
            string[] sub_parts = strSub.Split(new char[] { ',' });

            string[] list_parts = strList.Split(new char[] { ',' });

            bool bChanged = false;
            foreach (string temp in sub_parts)
            {
                string sub = temp.Trim();
                if (string.IsNullOrEmpty(sub) == true)
                    continue;

                for (int j = 0; j < list_parts.Length; j++)
                {
                    string list = list_parts[j];
                    if (list == null)
                        continue;

                    list = list.Trim();
                    if (string.IsNullOrEmpty(list) == true)
                        continue;

                    if (String.Compare(sub, list, true) == 0)
                    {
                        bChanged = true;
                        list_parts[j] = null;
                        if (bRemoveMultiple == false)
                            break;
                    }
                }
            }

            StringBuilder result = new StringBuilder(4096);
            foreach (string list in list_parts)
            {
                if (string.IsNullOrEmpty(list) == false)
                {
                    if (result.Length > 0)
                        result.Append(",");
                    result.Append(list);
                }
            }

            strList = result.ToString();

            return bChanged;
        }

        // 在列举值中增加或清除一个值
        // parameters:
        //      strSub  里面可以包含多个值
        public static void SetInList(ref string strList,
            string strSub,
            bool bOn)
        {
            if (bOn == false)
            {
                RemoveFromInList(strSub,
                    true,
                    ref strList);
            }
            else
            {
                // 单个值的情况
                if (strSub.IndexOf(',') == -1)
                {
                    if (IsInList(strSub, strList) == true)
                        return;	// 已经有了

                    // 在尾部新增加
                    if (string.IsNullOrEmpty(strList) == false)
                        strList += ",";

                    strList += strSub;
                    return;
                }

                // 2012/2/2
                // 多个值的情况
                string[] sub_parts = strSub.Split(new char[] { ',' });
                foreach (string sub in sub_parts)
                {
                    if (sub == null)
                        continue;

                    string strOne = sub.Trim();
                    if (string.IsNullOrEmpty(strOne) == true)
                        continue;

                    if (IsInList(strOne, strList) == true)
                        continue;	// 已经有了

                    // 在尾部新增加
                    if (string.IsNullOrEmpty(strList) == false)
                        strList += ",";

                    strList += strOne;
                }
            }
        }

        public static int CompareVersion(string strVersion1, string strVersion2)
        {
            if (string.IsNullOrEmpty(strVersion1) == true)
                strVersion1 = "0.0";
            if (string.IsNullOrEmpty(strVersion2) == true)
                strVersion2 = "0.0";

            try
            {
                Version version1 = new Version(strVersion1);
                Version version2 = new Version(strVersion2);

                return version1.CompareTo(version2);
            }
            catch (Exception ex)
            {
                throw new Exception("比较版本号字符串 '" + strVersion1 + "' 和 '" + strVersion2 + "' 过程出现异常: " + ex.Message, ex);
            }
        }


        // 检测一个号码字符串是否在指定的范围内
        public static bool Between(string strNumber,
            string strStart,
            string strEnd)
        {
            if (strStart.Length != strEnd.Length)
                throw new ArgumentException("strStart 和 strEnd 应当字符数相同");
            if (strNumber == null)
                throw new ArgumentException("strNumber 参数值不能为 null");

            if (strNumber.Length != strStart.Length)
                return false;
            if (string.Compare(strNumber, strStart) < 0)
                return false;
            if (string.Compare(strNumber, strEnd) > 0)
                return false;
            return true;
        }
    }
}
