using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Command.Service
{
    public class SearchCommand:BaseCommand
    {

        /// <summary>
        /// 书目检索结果集，存路径
        /// </summary>
        public List<string> BiblioResultPathList { get; set; }

        /// <summary>
        /// 是否继续输入n翻页
        /// </summary>
        public bool IsCanNextPage = false;

        /// <summary>
        /// 下一步开始序号
        /// </summary>
        public long ResultNextStart = -1;

        /// <summary>
        /// 获取下一页检索结果
        /// </summary>
        /// <param name="strText"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public bool GetNextPage(out string strText,
            out string strError)
        {
            strText = "";
            strError = "";

            if (this.IsCanNextPage == false)
            {
                strError = "已到末页。";
                return false;
            }

            long lTotalCount = this.BiblioResultPathList.Count;
            if (this.ResultNextStart >= lTotalCount)
            {
                strError = "内部错误，下页起始序号>=总记录数了";
                return false;
            }

            // 本页显示的最大序号
            long nMaxIndex = this.ResultNextStart + dp2CommandUtility.C_ViewCount_OnePage;
            if (nMaxIndex > lTotalCount)
            {
                nMaxIndex = lTotalCount;
            }

            string strPreMessage = "";
            if (nMaxIndex < dp2CommandUtility.C_ViewCount_OnePage
                || (this.ResultNextStart == 0 && nMaxIndex == lTotalCount))
            {
                // 没有下页了
                this.IsCanNextPage = false;
                strPreMessage = "命中'" + lTotalCount + "'条书目记录。您可以回复序列查看详细信息。\r\n";
            }
            else if (nMaxIndex < lTotalCount)
            {
                // 有下页
                this.IsCanNextPage = true;
                strPreMessage = "命中'" + lTotalCount + "'条书目记录。本次显示第" + (this.ResultNextStart + 1).ToString() + "-" + nMaxIndex + "条，您可以回复N继续显示下一页，或者回复序列查看详细信息。\r\n";
            }
            else if (nMaxIndex == lTotalCount)
            {
                //无下页
                this.IsCanNextPage = false;
                strPreMessage = "命中'" + lTotalCount + "'条书目记录。本次显示第" + (this.ResultNextStart + 1).ToString() + "-" + nMaxIndex + "条，已到末页。您可以回复序列查看详细信息。\r\n";
            }

            string strBrowse = "";
            for (long i = this.ResultNextStart; i < nMaxIndex; i++)
            {
                if (strBrowse != "")
                    strBrowse += "\n";

                string text = this.BiblioResultPathList[(int)i];
                int index = text.IndexOf("*");
                if (index >= 0)
                    text = text.Substring(index + 1);
                strBrowse += (i + 1).ToString().PadRight(5, ' ') + text;
            }

            // 设置下页索引
            this.ResultNextStart = nMaxIndex;

            //返回结果
            strText = strPreMessage + strBrowse;

            return true;
        }
    }
}
