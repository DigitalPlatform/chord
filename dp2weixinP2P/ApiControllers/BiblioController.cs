using dp2Command.Service;
using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class BiblioController : ApiController
    {
        // GET api/<controller>
        public SearchBiblioResult Get(string libUserName, string from, string word, string resultSet)
        {
            SearchBiblioResult searchRet = new SearchBiblioResult();
            if (String.IsNullOrEmpty(resultSet) == true)
                resultSet = "weixin-" + Guid.NewGuid();

            // 取下一页的情况
            if (from == "_N" || from == "_ReView")
            {
                searchRet.apiResult = new ApiResult();
                long num = 0;
                try
                {
                    num = Convert.ToInt32(word);
                    if (num < 0)
                    {
                        searchRet.apiResult.errorCode = -1;
                        searchRet.apiResult.errorInfo = "传入的word值[" + word + "]格式不正确，必须是>=0。";
                        goto END1;
                    }
                }
                catch
                {
                    searchRet.apiResult.errorCode = -1;
                    searchRet.apiResult.errorInfo = "传入的word值[" + word + "]格式不正确，必须是数值。";
                    goto END1;
                }

                if (from == "_N")
                {
                    //word值表示起始位置
                    searchRet= dp2WeiXinService.Instance.getFromResultSet(libUserName, resultSet, num, WeiXinConst.C_OnePage_Count);
                    goto END1;
                }
                else if (from == "_ReView")
                {
                    if (resultSet.Substring(0, 1) == "#")
                        resultSet = resultSet.Substring(1);

                    // 重新显示，此时word代表数量
                    searchRet= dp2WeiXinService.Instance.getFromResultSet(libUserName, resultSet, 0, num);
                    goto END1;

                }
                else
                {
                    searchRet.apiResult.errorCode = -1;
                    searchRet.apiResult.errorInfo = "不支持的from[" + from + "]";
                    goto END1;
                }
            }
            else
            {
                searchRet= dp2WeiXinService.Instance.SearchBiblio(libUserName,
                     from,
                     word,
                     resultSet);
                goto END1;
            }

        END1:
            searchRet.resultSetName = resultSet;
            return searchRet;
        }

        public string GetBiblio(string id, [FromUri] string format, string libUserName)
        {
            string strSummary = "未实现";    

            if (id == "more")
            {
                strSummary = dp2WeiXinService.Instance.GetBarcodesSummary(libUserName, format);
                return strSummary;
            }

            if (format == "summary") //todo 将summary字符串改为常量
            {
                string strRecPath = "";
                string strError = "";
                int nRet = dp2WeiXinService.Instance.GetBiblioSummary(libUserName,id,
                    "",
                    out strSummary,
                    out strRecPath,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    strSummary = strError;

                return strSummary;
            }
            

            return "未实现的风格:" + format;
        }

        /// <summary>
        /// 获取书目详细信息
        /// </summary>
        /// <param name="libUserName"></param>
        /// <param name="biblioPath"></param>
        /// <returns></returns>
        public BiblioRecordResult Get(string libUserName, string biblioPath)
        {
            dp2WeiXinService.Instance.WriteLog("走进get() libUserName["+libUserName+"],biblioPath["+biblioPath+"]");
            BiblioRecordResult result = dp2WeiXinService.Instance.GetBiblioDetail(libUserName,
                biblioPath);
            return result;
        }



    }
}