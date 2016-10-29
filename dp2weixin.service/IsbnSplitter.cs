using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.service
{
    public class IsbnSplitter
    {
        // 盘算是否为 ISBN 字符串
        // 如果用 ISBN 作为前缀，返回的时候 strTextParam 中会去掉前缀部分。这样便于用于对话框检索
        public static bool IsISBN(ref string strTextParam)
        {
            string strText = strTextParam;

            if (string.IsNullOrEmpty(strText) == true)
                return false;
            strText = strText.Replace("-", "").ToUpper();
            if (string.IsNullOrEmpty(strText) == true)
                return false;

            if (StringUtil.HasHead(strText, "ISBN") == true)
            {
                strText = strText.Substring("ISBN".Length).Trim();
                if (string.IsNullOrEmpty(strText) == true)
                    return false;
                strTextParam = strText;
                return true;
            }

            // 2015/5/8
            if (strText.ToUpper().EndsWith("ISBN") == true)
            {
                strText = strText.Substring(0, strText.Length - "ISBN".Length).Trim();
                if (string.IsNullOrEmpty(strText) == true)
                    return false;
                strTextParam = strText;
                return true;
            }

            string strError = "";
            // return:
            //      -1  出错
            //      0   校验正确
            //      1   校验不正确。提示信息在strError中
            int nRet = IsbnSplitter.VerifyISBN(strText,
                out strError);
            if (nRet == 0)
                return true;

            return false;
        }

        // 校验 ISBN 字符串
        // 注：返回 -1 和 返回 1 的区别：-1 表示调用过程出错，暗示对这样的 ISBN 字符串应当预先检查，若不符合基本形式要求则避免调用本函数
        // return:
        //      -1  出错
        //      0   校验正确
        //      1   校验不正确。提示信息在strError中
        public static int VerifyISBN(string strISBNParam,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strISBNParam) == true)
            {
                strError = "ISBN字符串内容为空";
                return -1;
            }

            // 2015/9/7
            string strISBN = strISBNParam.Trim();
            if (string.IsNullOrEmpty(strISBN) == true)
            {
                strError = "ISBN字符串内容为空(1)";
                return -1;
            }

            strISBN = strISBNParam.Replace("-", "").Replace(" ", "");
            if (string.IsNullOrEmpty(strISBN) == true)
            {
                strError = "ISBN字符串内容为空";
                return 1;
            }

            if (strISBN.Length != 10 && strISBN.Length != 13)
            {
                strError = "(除字符'-'和空格外)ISBN字符串的长度既不是10位也不是13位";
                return 1;
            }

            if (strISBN.Length == 10)
            {
                try
                {
                    char c = GetIsbn10VerifyChar(strISBN);
                    if (c != strISBN[9])
                    {
                        strError = "ISBN '" + strISBN + "' 校验不正确";
                        return 1;
                    }
                }
                catch (ArgumentException ex)
                {
                    strError = "ISBN '" + strISBN + "' 校验不正确: " + ex.Message;
                    return 1;
                }
            }

            if (strISBN.Length == 13)
            {
                //
                char c = GetIsbn13VerifyChar(strISBN);
                if (c != strISBN[12])
                {
                    strError = "ISBN '" + strISBN + "' 校验不正确";
                    return 1;
                }
            }

            return 0;
        }

        /// <summary>
        /// 计算出 ISBN-10 校验位
        /// </summary>
        /// <param name="strISBN">ISBN 字符串</param>
        /// <returns>校验位字符</returns>
        public static char GetIsbn10VerifyChar(string strISBN)
        {
            strISBN = strISBN.Trim();
            strISBN = strISBN.Replace("-", "");
            strISBN = strISBN.Replace(" ", "");

            if (strISBN.Length < 9)
                throw new ArgumentException("用于计算校验位的ISBN-10长度至少要在9位数字以上(不包括横杠在内)");

            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += (strISBN[i] - '0') * (i + 1);
            }
            int v = sum % 11;

            if (v == 10)
                return 'X';

            return (char)('0' + v);
        }

        /// <summary>
        /// 计算出 ISBN-13 校验位
        /// </summary>
        /// <param name="strISBN">ISBN 字符串</param>
        /// <returns>校验位字符</returns>
        public static char GetIsbn13VerifyChar(string strISBN)
        {
            strISBN = strISBN.Trim();
            strISBN = strISBN.Replace("-", "");
            strISBN = strISBN.Replace(" ", "");


            if (strISBN.Length < 12)
                throw new Exception("用于计算校验位的ISBN-13长度至少要在12位数字以上(不包括横杠在内)");

            int m = 0;
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                if ((i % 2) == 0)
                    m = 1;
                else
                    m = 3;

                sum += (strISBN[i] - '0') * m;
            }

            // 注：如果步骤5所得余数为0，则校验码为0。
            if ((sum % 10) == 0)
                return '0';

            int v = 10 - (sum % 10);

            return (char)('0' + v);
        }

    }
}
