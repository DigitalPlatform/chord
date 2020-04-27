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
    public class BiblioApiController : ApiController
    {
        /// <summary>
        /// 检索
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="from"></param>
        /// <param name="word"></param>
        /// <param name="match"></param>
        /// <param name="resultSet"></param>
        /// <returns></returns>
        [HttpGet]
        public SearchBiblioResult Search(string weixinId,
            string libId, 
            string from,
            string word, 
            string match,
            string resultSet)
        {
            SearchBiblioResult searchRet = new SearchBiblioResult();

            // 测试加的日志
            //dp2WeiXinService.Instance.WriteErrorLog1("search-1");

            if (String.IsNullOrEmpty(resultSet) == true)
                resultSet = "weixin-" + Guid.NewGuid();

            // 测试加的日志
           //dp2WeiXinService.Instance.WriteErrorLog1("search-2-"+resultSet);


            // 取下一页的情况
            if (from == "_N" || from == "_ReView")
            {
                // 测试加的日志
                //dp2WeiXinService.Instance.WriteErrorLog1("search-3");

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
                    searchRet = dp2WeiXinService.Instance.getFromResultSet(weixinId,
                        libId, 
                        resultSet,
                        num, 
                        WeiXinConst.C_OnePage_Count);
                    goto END1;
                }
                else if (from == "_ReView")
                {
                    if (resultSet.Substring(0, 1) == "#")
                        resultSet = resultSet.Substring(1);

                    // 重新显示，此时word代表数量
                    searchRet = dp2WeiXinService.Instance.getFromResultSet(weixinId,
                        libId,
                        resultSet, 
                        0, 
                        num);
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
                // 测试加的日志
                //dp2WeiXinService.Instance.WriteErrorLog1("search-4");


                searchRet = dp2WeiXinService.Instance.SearchBiblio(weixinId,
                    libId,
                     from,
                     word,
                     match,
                     resultSet);
                goto END1;
            }

        END1:
            searchRet.resultSetName = resultSet;
            return searchRet;
        }

        /// <summary>
        /// 获取摘要信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="format"></param>
        /// <param name="libId"></param>
        /// <returns></returns>
        [HttpGet]
        public string GetBiblioSummary(string id, [FromUri] string format, string libId)
        {
            string strSummary = "未实现";

            LibEntity lib = dp2WeiXinService.Instance.GetLibById(libId);
            if (lib == null)
            {
                return "未找到id为[" + libId + "]的图书馆定义。";
            }

            if (format == "more-summary")
            {
                strSummary = dp2WeiXinService.Instance.GetBarcodesSummary(lib, id);
                return strSummary;
            }
            else if (format == "summary")
            {
                string strRecPath = "";
                string strError = "";
                int nRet = dp2WeiXinService.Instance.GetBiblioSummary(lib, id,
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
        /// 获取书目详细信息,包括summary与items
        /// </summary>
        /// <param name="libUserName"></param>
        /// <param name="biblioPath"></param>
        /// <returns></returns>
        [HttpGet]
        public BiblioDetailResult GetBiblioDetail(string weixinId,
            string libId, 
            string biblioPath,
            string format,
            string from)
        {
          
            BiblioDetailResult result = dp2WeiXinService.Instance.GetBiblioDetail(weixinId,
                libId,
                biblioPath,
                format,
                from);
            return result;
        }


        // 绑定
        [HttpPost]
        public ApiResult SetBibiloItem(BiblioItem item)
        {
            string error = "";
            // 返回对象
            ApiResult result = new ApiResult();



            //int nRet = dp2WeiXinService.Instance.Bind(item.libId,
            //    item.bindLibraryCode,
            //    item.prefix,
            //    item.word,
            //    item.password,
            //    item.weixinId,
            //    out userItem,
            //    out error);
            //if (nRet == -1)
            //{
            //    result.errorCode = -1;
            //    result.errorInfo = error;
            //    return result;
            //}
            //result.users = new List<WxUserItem>();
            //result.users.Add(userItem);





            return result;
        }

        // PUT api/<controller>/5
        public ApiResult Put(string id, BiblioItem item)
        {
            string error = "";
            // 返回对象
            ApiResult result = new ApiResult();

            return result;
        }

        /*
        /// <summary>
        /// 获取items
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="biblioPath"></param>
        /// <returns></returns>
        [HttpGet]
        public BiblioItemResult GetBiblioItem(string weixinId, 
            string libId, 
            string biblioPath)
        {
            BiblioItemResult result = new BiblioItemResult();

            List<BiblioItem> items = null;
            string strError = "";
            long ret= dp2WeiXinService.Instance.GetItemInfo(weixinId,
                libId,
                biblioPath,
                out items,
                out strError);
            if (ret == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
                return result;
            }

            result.itemList = items;
            return result;
        }
        */
    }
}