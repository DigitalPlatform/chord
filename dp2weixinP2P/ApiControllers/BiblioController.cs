using dp2Command.Service;
using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace dp2weixinP2P.ApiControllers
{
    public class BiblioController : ApiController
    {
        // GET api/<controller>
        public SearchBiblioResult Get(string libUserName, string from, string word)
        {
            // 取下一页的情况
            if (from == "_N")
            {
                SearchBiblioResult searchRet = new SearchBiblioResult();
                searchRet.apiResult = new ApiResult();
                searchRet.apiResult.errorCode = 0;
                searchRet.apiResult.errorInfo = "";
                searchRet.records = new List<BiblioRecord>();
                searchRet.isCanNext = false;
                if (HttpContext.Current.Session[WeiXinConst.C_Session_SearchResult] == null)
                {
                    searchRet.apiResult.errorCode = -1;
                    searchRet.apiResult.errorInfo = "session中不存在已检索到的数据。";
                    return searchRet;
                }

                int nStart = 0;
                try
                {
                    nStart = Convert.ToInt32(word);
                    if (nStart < 0)
                    {
                        searchRet.apiResult.errorCode = -1;
                        searchRet.apiResult.errorInfo = "传出的起始位置[" + word + "]格式不正确，必须是>=0。";
                        return searchRet;
                    }
                }
                catch
                {
                    searchRet.apiResult.errorCode = -1;
                    searchRet.apiResult.errorInfo = "传出的起始位置[" + word + "]格式不正确，必须是数值。";
                    return searchRet;
                }

                List<BiblioRecord> totalRecords = (List<BiblioRecord>)HttpContext.Current.Session[WeiXinConst.C_Session_SearchResult];
                bool bNext = false;
                List<BiblioRecord> records = WeixinService.Instance.getOnePage(totalRecords, nStart, dp2CmdService2.C_OnePage_Count,
                     out bNext);
                searchRet.resultCount = totalRecords.Count;
                searchRet.records = records;
                searchRet.isCanNext = bNext;
                searchRet.apiResult.errorCode = totalRecords.Count;
                return searchRet;
            }
            else
            {

                List<BiblioRecord> totalRecords = null;
                SearchBiblioResult result = WeixinService.Instance.SearchBiblio(libUserName,
                     from,
                     word,
                     out totalRecords);
                if (result.resultCount > 0)
                {
                    // 存到session里
                    HttpContext.Current.Session[WeiXinConst.C_Session_SearchResult] = totalRecords;
                }
                return result;
            }


        }



    }
}