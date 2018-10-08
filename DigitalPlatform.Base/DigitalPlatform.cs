using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Diagnostics;

using System.Drawing;


namespace DigitalPlatform
{

    // byte[] 数组的实用函数集
    public class ByteArray
    {
        /*
        // 复制一个byte数组
        public static byte[] Dup(byte [] source)
        {
            if (source == null)
                return null;

            byte [] result = null;
            result = EnsureSize(result, source.Length);

            Array.Copy(source, 0, result, 0, source.Length);

            return result;
        }*/

        // 克隆一个字符数组
        public static byte[] GetCopy(byte[] baContent)
        {
            if (baContent == null)
                return null;
            byte[] baResult = new byte[baContent.Length];
            Array.Copy(baContent, 0, baResult, 0, baContent.Length);
            return baResult;
        }

        // 将byte[]转换为字符串，自动探测编码方式
        public static string ToString(byte[] baContent)
        {
            ArrayList encodings = new ArrayList();

            encodings.Add(Encoding.UTF8);
            encodings.Add(Encoding.Unicode);

            for (int i = 0; i < encodings.Count; i++)
            {
                Encoding encoding = (Encoding)encodings[i];

                byte[] Preamble = encoding.GetPreamble();

                if (baContent.Length < Preamble.Length)
                    continue;

                if (ByteArray.Compare(baContent, Preamble, Preamble.Length) == 0)
                    return encoding.GetString(baContent,
                        Preamble.Length,
                        baContent.Length - Preamble.Length);
            }

            // 缺省当作UTF8
            return Encoding.UTF8.GetString(baContent);
        }

        // byte[] 到 字符串
        public static string ToString(byte[] bytes,
            Encoding encoding)
        {
            int nIndex = 0;
            int nCount = bytes.Length;
            byte[] baPreamble = encoding.GetPreamble();
            if (baPreamble != null
                && baPreamble.Length != 0
                && bytes.Length >= baPreamble.Length)
            {
                byte[] temp = new byte[baPreamble.Length];
                Array.Copy(bytes,
                    0,
                    temp,
                    0,
                    temp.Length);

                bool bEqual = true;
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i] != baPreamble[i])
                    {
                        bEqual = false;
                        break;
                    }
                }

                if (bEqual == true)
                {
                    nIndex = temp.Length;
                    nCount = bytes.Length - temp.Length;
                }
            }

            return encoding.GetString(bytes,
                nIndex,
                nCount);
        }

        // 比较两个byte[]数组是否相等。
        // parameter:
        //		timestamp1: 第一个byte[]数组
        //		timestamp2: 第二个byte[]数组
        // return:
        //		0   相等
        //		大于或者小于0   不等。先比较长度。长度相等，再逐个字符相减。
        public static int Compare(
            byte[] bytes1,
            byte[] bytes2)
        {
            if (bytes1 == null && bytes2 == null)
                return 0;
            if (bytes1 == null)
                return -1;
            if (bytes2 == null)
                return 1;

            int nDelta = bytes1.Length - bytes2.Length;
            if (nDelta != 0)
                return nDelta;

            for (int i = 0; i < bytes1.Length; i++)
            {
                nDelta = bytes1[i] - bytes2[i];
                if (nDelta != 0)
                    return nDelta;
            }

            return 0;
        }

        // 比较两个byte数组的局部
        public static int Compare(
            byte[] bytes1,
            byte[] bytes2,
            int nLength)
        {
            if (bytes1.Length < nLength || bytes2.Length < nLength)
                return Compare(bytes1, bytes2, Math.Min(bytes1.Length, bytes2.Length));

            for (int i = 0; i < nLength; i++)
            {
                int nDelta = bytes1[i] - bytes2[i];
                if (nDelta != 0)
                    return nDelta;
            }

            return 0;
        }

        public static int IndexOf(byte[] source,
            byte v,
            int nStartPos)
        {
            for (int i = nStartPos; i < source.Length; i++)
            {
                if (source[i] == v)
                    return i;
            }
            return -1;
        }

        // 确保数组尺寸足够
        public static byte[] EnsureSize(byte[] source,
            int nSize)
        {
            if (source == null)
            {
                return new byte[nSize];
            }

            if (source.Length < nSize)
            {
                byte[] temp = new byte[nSize];
                Array.Copy(source,
                    0,
                    temp,
                    0,
                    source.Length);
                return temp;    // 尺寸不够，已经重新分配，并且继承了原有内容
            }

            return source;  // 尺寸足够
        }


        // 在缓冲区尾部追加一个字节
        public static byte[] Add(byte[] source,
            byte v)
        {
            int nIndex = -1;
            if (source != null)
            {
                nIndex = source.Length;
                source = EnsureSize(source, source.Length + 1);
            }
            else
            {
                nIndex = 0;
                source = EnsureSize(source, 1);
            }

            source[nIndex] = v;

            return source;
        }

        // 安全版本
        // 在缓冲区尾部追加若干字节
        public static byte[] SafeAdd(byte[] source,
            byte[] v,
            int nMaxBytes)
        {
            int nIndex = -1;
            if (source != null)
            {
                if (nMaxBytes != -1 && source.Length > nMaxBytes)
                    throw new Exception("source.Length:" + source.Length + " 超过极限尺寸 nMaxBytes:" + nMaxBytes);

                nIndex = source.Length;

                if (nMaxBytes != -1 && source.Length + v.Length > nMaxBytes)
                    throw new Exception("(source.Length:" + source.Length + " + v.Length:" + v.Length + ") 超过极限尺寸 nMaxBytes:" + nMaxBytes);

                source = EnsureSize(source, source.Length + v.Length);
            }
            else
            {
                // 2011/1/22
                if (v == null)
                    return null;

                if (nMaxBytes != -1 && v.Length > nMaxBytes)
                    throw new Exception("v.Length:" + v.Length + " 超过极限尺寸 nMaxBytes:" + nMaxBytes);

                nIndex = 0;
                source = EnsureSize(source, v.Length);
            }

            Array.Copy(v, 0, source, nIndex, v.Length);
            return source;
        }

        // 在缓冲区尾部追加若干字节
        public static byte[] Add(byte[] source,
            byte[] v)
        {
            int nIndex = -1;
            if (source != null)
            {
                nIndex = source.Length;
                source = EnsureSize(source, source.Length + v.Length);
            }
            else
            {
                // 2011/1/22
                if (v == null)
                    return null;
                nIndex = 0;
                source = EnsureSize(source, v.Length);
            }

            Array.Copy(v, 0, source, nIndex, v.Length);
            return source;
        }

        // 2011/9/12
        // 在缓冲区尾部追加若干字节
        public static byte[] Add(byte[] source,
            byte[] v,
            int nLength)
        {
            Debug.Assert(v.Length >= nLength, "");

            int nIndex = -1;
            if (source != null)
            {
                nIndex = source.Length;
                source = EnsureSize(source, source.Length + nLength);
            }
            else
            {
                if (v == null)
                    return null;
                nIndex = 0;
                source = EnsureSize(source, nLength);
            }

            Array.Copy(v, 0, source, nIndex, nLength);

            return source;
        }

        // 从 source 头部移走一段。source 随后被改变
        public static byte[] Remove(ref byte[] source, int length)
        {
            if (length > source.Length)
                length = source.Length;

            byte[] result = new byte[length];
            Array.Copy(source, result, length);
            int rest = source.Length - length;
            byte[] temp = new byte[rest];
            if (rest > 0)
                Array.Copy(source, length, temp, 0, rest);

            source = temp;
            return result;
        }

        // 得到用16进制表示的时间戳字符串
        public static string GetHexTimeStampString(byte[] baTimeStamp)
        {
            if (baTimeStamp == null)
                return "";
            string strText = "";
            for (int i = 0; i < baTimeStamp.Length; i++)
            {
                //string strHex = String.Format("{0,2:X}",baTimeStamp[i]);
                string strHex = Convert.ToString(baTimeStamp[i], 16);
                strText += strHex.PadLeft(2, '0');
            }

            return strText;
        }

        // 得到byte[]类型的时间戳
        public static byte[] GetTimeStampByteArray(string strHexTimeStamp)
        {
            if (string.IsNullOrEmpty(strHexTimeStamp) == true)
                return null;

            byte[] result = new byte[strHexTimeStamp.Length / 2];

            for (int i = 0; i < strHexTimeStamp.Length / 2; i++)
            {
                string strHex = strHexTimeStamp.Substring(i * 2, 2);
                result[i] = Convert.ToByte(strHex, 16);

            }

            return result;
        }
    }


}



