using DigitalPlatform.Message;
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
        // 检索书目
        //loginUserName：检索使用的dp2library帐号，即前端当前绑定的帐户，如果是读者则传读者证条码；如果是馆员帐户，传馆员用户名
        //loginUserType：空表示馆员，如果是读者传"patron"
        //weixinId：前端用户唯一id
        //作用：当图书馆配置了不支持外部检索时，用weixinId检查该微信用户是否绑定了图书馆账户，如果未绑定，则不能检索。另外还用于获取该微信绑定帐户的分馆代码。
        //libId：图书馆id
        //from：检索途径，如果前端指定了一个特定的途径，由传指定的途径。
        //如果前端只有一个关键词输入框，是一种简单检索界面，则传以逗号分隔的多个途径，例如：title,ISBN,contributor,subject,clc,_class,publishtime,publisher
        //另外途径还可以传一个特殊含义的功能
        // _N表示下一页，此时word传是这次要获取记录的开始序号，结果集参数传前面检索返回的结果集。
        // _ReView表示重新获取数据，此时word传是要获取多少条记录
        //word：检索词
        //match:匹配方式，简单检索时传的left
        //resultSet:前端指定的一个结果集名称，用于分批获取。
        //返回值：
        //searchRet.records = records;  //返回的记录集合
        //searchRet.resultCount = records.Count; // 本次返回的记录数 
        //searchRet.isCanNext = bNext;  //是否有下页
        //searchRet.apiResult.errorCode = lRet;  //-1表示出错，0未命中，其它表示命中总数。
        [HttpGet]
        public SearchBiblioResult SearchBiblio(string loginUserName,
            string loginUserType,
            string weixinId,
            string libId,
            string from,
            string word,
            string match,
            string resultSet)
        {
            SearchBiblioResult searchRet = new SearchBiblioResult();

            // 测试加的日志
            //dp2WeiXinService.Instance.WriteErrorLog1("search-1");

            // 2021/8/2 根据前端传的帐户创建LoginInfo
            LoginInfo loginInfo = dp2WeiXinService.GetLoginInfo(loginUserName, loginUserType);


            if (String.IsNullOrEmpty(resultSet) == true)
            {
                if (from == "_ReView")  // 重新获取信息时，如果不存在结果集，则返回错误。
                {
                    searchRet.apiResult = new ApiResult();
                    searchRet.apiResult.errorCode = -1;
                    searchRet.apiResult.errorInfo = "当review检索结果时，结果集名称不能为空。";
                    return searchRet;
                }

                if (from == "_N")
                {
                    searchRet.apiResult = new ApiResult();
                    searchRet.apiResult.errorCode = -1;
                    searchRet.apiResult.errorInfo = "当from参数为_N时，结果集名称不能为空。";
                    return searchRet;
                }

                resultSet = "weixin-" + Guid.NewGuid();
            }

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
                    searchRet = dp2WeiXinService.Instance.getFromResultSet(loginInfo,
                        weixinId,
                        libId,
                        resultSet,
                        num,
                        WeiXinConst.C_OnePage_Count);
                    goto END1;
                }
                else if (from == "_ReView")  //刷新的情况，显示从0至当前最大数字
                {
                    if (resultSet.Substring(0, 1) == "#")
                        resultSet = resultSet.Substring(1);

                    // 重新显示，此时word代表数量
                    searchRet = dp2WeiXinService.Instance.getFromResultSet(loginInfo,
                        weixinId,
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


                searchRet = dp2WeiXinService.Instance.SearchBiblio(loginInfo,
                    weixinId,
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



        //获取书目详细信息
        //loginUserName:使用的dp2library帐号，即前端当前绑定的帐户，如果是读者则传读者证条码；如果是馆员帐户，传馆员用户名
        //loginUserType：空表示馆员，如果是读者传"patron"
        //weixinId：前端用户唯一id
        //作用：当图书馆配置了不支持外部检索时，用weixinId检查该微信用户是否绑定了图书馆账户，如果未绑定，则不能检索。另外还用于获取该微信绑定帐户的分馆代码。
        //libId：图书馆id
        //biblioPath:书目路径
        //format：风格，summary表示摘要和封面，table表示表格显示和封面
        //from：此字段作废，获取详情来源，表示是从index过来的，还是detail过来的，主要用于好书推荐的返回，后来没用注释掉了。
        [HttpGet]
        public BiblioDetailResult GetBiblioDetail(string loginUserName,
            string loginUserType,
            string weixinId,
            string libId,
            string biblioPath,
            string format,
            string from)
        {

            // 2021/8/2 根据前端传的帐户创建LoginInfo
            LoginInfo loginInfo = dp2WeiXinService.GetLoginInfo(loginUserName, loginUserType);

            BiblioDetailResult result = dp2WeiXinService.Instance.GetBiblioDetail(loginInfo,
                weixinId,
                libId,
                biblioPath,
                format,
                from);
            return result;
        }

        // 获取书目详情
        [HttpGet]
        public BiblioDetailResult GetBiblio(string loginUserName,
            string loginUserType,
            string weixinId,
            string libId,
            string biblioPath)
        {
            LoginInfo loginInfo = dp2WeiXinService.GetLoginInfo(loginUserName, loginUserType);

            BiblioDetailResult result = dp2WeiXinService.Instance.GetBiblio(loginInfo,
                weixinId,
                libId,
                biblioPath);
            return result;
        }

        // 获取册记录
        [HttpGet]
        public BiblioDetailResult GetItems(string loginUserName,
    string loginUserType,
    string weixinId,
    string libId,
    string biblioPath)
        {
            LoginInfo loginInfo = dp2WeiXinService.GetLoginInfo(loginUserName, loginUserType);

            BiblioDetailResult result = dp2WeiXinService.Instance.GetItems(loginInfo,
                weixinId,
                libId,
                biblioPath);
            return result;
        }


        /// <summary>
        /// 获取摘要信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="format"></param>
        /// <param name="libId"></param>
        /// <returns></returns>
        [HttpGet]
        public string GetBiblioSummary(string loginUserName,
            string loginUserType,
            string id,
            [FromUri] string format,
            string libId)
        {
            string strSummary = "未实现";

            LibEntity lib = dp2WeiXinService.Instance.GetLibById(libId);
            if (lib == null)
            {
                return "未找到id为[" + libId + "]的图书馆定义。";
            }

            // 2021/8/2 根据前端传的帐户创建LoginInfo
            LoginInfo loginInfo = dp2WeiXinService.GetLoginInfo(loginUserName, loginUserType);

            if (format == "more-summary")
            {
                strSummary = dp2WeiXinService.Instance.GetBarcodesSummary(loginInfo,
                    lib,
                    id);
                return strSummary;
            }
            else if (format == "summary")
            {
                string strRecPath = "";
                string strError = "";
                int nRet = dp2WeiXinService.Instance.GetBiblioSummary(loginInfo,
                    lib,
                    id,
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





        // 册登记
        [HttpPost]
        public ApiResult SetItem(string loginUserName,
            string libId,
            string biblioPath,
            string action,
            BiblioItem item)
        {
            // 返回对象
            ApiResult result = new ApiResult();

            string strError = "";
            int nRet = dp2WeiXinService.Instance.SetItem(loginUserName,
                libId,
                biblioPath,
                action,
                item,
                out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }
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