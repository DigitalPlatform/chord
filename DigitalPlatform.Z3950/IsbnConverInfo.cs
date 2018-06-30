using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalPlatform.Z3950
{
    // ISBN 实用转换工具类
    public class IsbnConvertInfo
    {
        public IsbnSplitter IsbnSplitter = null;
        public string ConvertStyle = "";    // force13 force10 addhyphen removehyphen wild

#if NO
        // return:
        //      -1  出错
        //      0   没有必要转换
        //      1   已经转换
        public int ConvertISBN(ref string strISBN,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(this.ConvertStyle) == true)
                return 0;

            bool bForce13 = StringUtil.IsInList("force13", this.ConvertStyle);
            bool bForce10 = StringUtil.IsInList("force10", this.ConvertStyle);
            bool bAddHyphen = StringUtil.IsInList("addhyphen", this.ConvertStyle);
            bool bRemoveHyphen = StringUtil.IsInList("removehyphen", this.ConvertStyle);
            int nRet = 0;

            string strStyle = "remainverifychar";
            if (bAddHyphen == true)
            {
                if (bForce10 == false && bForce13 == false)
                    strStyle += ",auto";
                else if (bForce13 == true)
                    strStyle += ",force13";
                else if (bForce10 == true)
                    strStyle += ",force10";
                else
                {
                    strError = "force10和force13不应同时具备";
                    return -1;
                }

                string strTarget = "";
                nRet = this.IsbnSplitter.IsbnInsertHyphen(
                   strISBN,
                   strStyle,
       out strTarget,
       out strError);
                if (nRet == -1)
                    return -1;
                strISBN = strTarget;
                return 1;
            }

            if (bRemoveHyphen == true)
            {
                if (bForce10 == false && bForce13 == false)
                {
                    strISBN = strISBN.Replace("-", "");
                    return 1;
                }
                else if (bForce13 == true)
                    strStyle += ",force13";
                else if (bForce10 == true)
                    strStyle += ",force10";
                else
                {
                    strError = "force10和force13不应同时具备";
                    return -1;
                }

                string strTarget = "";
                nRet = this.IsbnSplitter.IsbnInsertHyphen(
                   strISBN,
                   strStyle,
       out strTarget,
       out strError);
                if (nRet == -1)
                    return -1;
                strISBN = strTarget.Replace("-", "");
                return 1;
            }

            return 0;
        }
#endif
        // 对 ISBN 执行转换。转换的结果可能会把一个 ISBN 拆散为多个 ISBN
        // result.Value:
        //      -1  出错
        //      0   没有必要转换
        //      1   已经转换
        public Result ConvertISBN(string strISBN,
            out List<string> isbns)
        {
            isbns = new List<string>();

            if (string.IsNullOrEmpty(this.ConvertStyle) == true)
            {
                isbns.Add(strISBN);
                return new Result();
            }

            bool bForce13 = StringUtil.IsInList("force13", this.ConvertStyle);
            bool bForce10 = StringUtil.IsInList("force10", this.ConvertStyle);
            bool bAddHyphen = StringUtil.IsInList("addhyphen", this.ConvertStyle);
            bool bRemoveHyphen = StringUtil.IsInList("removehyphen", this.ConvertStyle);
            bool bWildMatch = StringUtil.IsInList("wild", this.ConvertStyle);

            int nRet = 0;

            if (bWildMatch == true)
            {
                List<string> styles = new List<string>();
                styles.Add("remainverifychar,auto");
                styles.Add("remainverifychar,force13");
                styles.Add("remainverifychar,force10");

                foreach (string style in styles)
                {
                    nRet = this.IsbnSplitter.IsbnInsertHyphen(
                        strISBN,
                        style,
                        out string strTarget,
                        out string strError);
                    if (nRet == -1)
                        continue;
                    isbns.Add(strTarget);
                }
                isbns.Add(strISBN); // 最原始的

                styles = new List<string>();
                styles.Add("remainverifychar,force13");
                styles.Add("remainverifychar,force10");

                foreach (string style in styles)
                {
                    nRet = this.IsbnSplitter.IsbnInsertHyphen(
                        strISBN,
                        style,
                        out string strTarget,
                        out string strError);
                    if (nRet == -1)
                        continue;
                    isbns.Add(strTarget.Replace("-", ""));
                }
                isbns.Add(strISBN.Replace("-", ""));    // 最原始的去掉横线的

                // TODO: 是否要增加10位13位去掉校验位的，然后指明前方一致的?

                StringUtil.RemoveDupNoSort(ref isbns);
                return new Result { Value = 1 };
            }

            string strStyle = "remainverifychar";

            // 如果 bAddHyphen 和 bRemoveHyphen 都没有勾选，那么需要看字符串里面本来是否有横杠，有就保留，没有也不要加入
            if (bAddHyphen == false && bRemoveHyphen == false
                && (bForce13 == true || bForce10 == true))
            {
                if (strISBN.IndexOf("-") == -1)
                    bRemoveHyphen = true;
                else
                    bAddHyphen = true;
            }

            if (bAddHyphen == true)
            {
                if (bForce10 == false && bForce13 == false)
                    strStyle += ",auto";
                else if (bForce13 == true)
                    strStyle += ",force13";
                else if (bForce10 == true)
                    strStyle += ",force10";
                else
                {
                    return new Result { Value = -1, ErrorInfo = "force10和force13不应同时具备" };
                }

                nRet = this.IsbnSplitter.IsbnInsertHyphen(
                   strISBN,
                   strStyle,
       out string strTarget,
       out string strError);
                if (nRet == -1)
                    return new Result { Value = -1, ErrorInfo = strError };
                isbns.Add(strTarget);
                return new Result { Value = 1 };
            }

            if (bRemoveHyphen == true)
            {
                if (bForce10 == false && bForce13 == false)
                {
                    strISBN = strISBN.Replace("-", "");
                    return new Result { Value = 1 };
                }
                else if (bForce13 == true)
                    strStyle += ",force13";
                else if (bForce10 == true)
                    strStyle += ",force10";
                else
                {
                    return new Result { Value = -1, ErrorInfo = "force10和force13不应同时具备" };
                }

                nRet = this.IsbnSplitter.IsbnInsertHyphen(
                   strISBN,
                   strStyle,
       out string strTarget,
       out string strError);
                if (nRet == -1)
                    return new Result { Value = -1, ErrorInfo = strError };
                isbns.Add(strTarget.Replace("-", ""));
                return new Result { Value = 1 };
            }

            return new Result();
        }
    }

}
