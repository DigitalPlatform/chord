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
        public SearchApiResult  Search(string searchType,
            string loginUserName,
            string loginUserType,
            string weixinId,
            string libId,
            string word,
            string from,
            string match,
            string resultSet)
        {
            SearchApiResult searchRet = new SearchApiResult();
            searchRet.apiResult = new ApiResult();
            searchRet.apiResult.errorCode = 0;
            searchRet.apiResult.errorInfo = "";
            searchRet.records = null;
            searchRet.isCanNext = false;

            string strError = "";
            bool bNext = false;
            long lRet = 0;

            object records = null;//使用一个通用的类型，书目 和 册不同。
            long recordsCount = 0;

            // 2021/8/2 根据前端传的帐户创建LoginInfo
            LoginInfo loginInfo = dp2WeiXinService.GetLoginInfo(loginUserName, loginUserType);

            // 取下一页 或 刷新显示的情况，是从结果集中获取
            if (from == "_N" || from == "_ReView")
            {
                // 当获取下一页 或者 重新获取记录时，如果结果集参数为空，则返回错误。
                if (String.IsNullOrEmpty(resultSet) == true)
                {
                    searchRet.apiResult.errorCode = -1;
                    searchRet.apiResult.errorInfo = "当from参数=" + from + "时，结果集参数不能为空。";
                    return searchRet;
                }

                // 把结果集名称也设到返回结果中
                searchRet.resultSetName = resultSet;

                // 将传入的字符转换成数字
                long lWord = 0;
                try
                {
                    lWord = Convert.ToInt32(word);
                }
                catch
                {
                    searchRet.apiResult.errorCode = -1;
                    searchRet.apiResult.errorInfo = "当from=" + from + "时，传入的word值[" + word + "]格式不正确，必须是数值。";
                    return searchRet;
                }
                if (lWord < 0) //不存在负数的情况
                {
                    searchRet.apiResult.errorCode = -1;
                    searchRet.apiResult.errorInfo = "当from=" + from + "时，传入的word值[" + word + "]格式不正确，必须是>=0。";
                    return searchRet;
                }

                long start = 0;  //开始
                long count = 0;  //长度
                if (from == "_N")
                {
                    start = lWord; //取下一页时，word值表示起始位置
                    count = WeiXinConst.C_OnePage_Count;
                }
                else if (from == "_ReView")
                {
                    if (resultSet.Substring(0, 1) == "#")
                        resultSet = resultSet.Substring(1);

                    //刷新的情况，显示从0至当前最大数字
                    start = 0; //word值表示起始位置
                    count = lWord;//重新显示，此时word代表数量
                }

                //List<BiblioRecord> biblios = null;
                //List<BiblioItem> items = null;
                // 获取结果集
                lRet = dp2WeiXinService.Instance.Search(searchType,
                    weixinId,
                   libId,
                   loginInfo,
                   "!getResult",  //word
                    "",  //strFrom
                    "", //match
                    resultSet,
                    start,
                    count,
                    out records,
                    out recordsCount,
                    out bNext,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    searchRet.apiResult.errorCode = (int)lRet;
                    searchRet.apiResult.errorInfo = strError;
                    return searchRet;
                }

                searchRet.resultCount = recordsCount;//biblios.Count;  //本次返回的记录条数
                searchRet.records = records;
                searchRet.isCanNext = bNext;
                searchRet.apiResult.errorCode = lRet; //把总条数放在errorcode里

                //接口返回
                return searchRet;
            }

            //===上面是从结果集中获取，以下是检索===

            // 传入的结果集参数为空，则new一个guid命令的新的结果集
            if (String.IsNullOrEmpty(resultSet) == true)
            {
                // 设置一个新的结果集名称，后面传dp2检索接口，这个值也会放在接口返回结果中。
                resultSet = "weixin-" + Guid.NewGuid();
                searchRet.resultSetName = resultSet;
            }

            // 未传入检索途径
            if (string.IsNullOrEmpty(from) == true)
            {
                searchRet.apiResult.errorCode = -1;
                searchRet.apiResult.errorInfo = "尚未传入检索途径from参数。";
                return searchRet;
            }

            // 未传入word时，设为空字符串
            if (string.IsNullOrEmpty(word) == true)
            {
                word = "";
            }

            // 调书目检索函数
            lRet = dp2WeiXinService.Instance.Search(searchType,
                weixinId,
               libId,
               loginInfo,
               word,
               from,
               match,
               resultSet,
               0,
               15,// 2018/3/15第一次获取15条，稍微超出平板， WeiXinConst.C_OnePage_Count,
               out records,
               out recordsCount,
               out bNext,
               out strError);
            if (lRet == -1 || lRet == 0)
            {
                searchRet.apiResult.errorCode = (int)lRet;

                string libName = "";
                LibEntity libEntity = dp2WeiXinService.Instance.GetLibById(libId);
                if (libEntity != null)
                {
                    libName = libEntity.libName;
                }
                // 把帐户脱敏一下，防止在提示信息时暴露帐户。
                string maskLoginName = loginInfo.UserName;
                if (loginInfo.UserName != null && loginInfo.UserName.Length >= 1)
                    maskLoginName = loginInfo.UserName.Substring(0, 1).PadRight(loginInfo.UserName.Length, '*');
                searchRet.apiResult.errorInfo = strError + "[图书馆为" + libName + ",帐户为" + maskLoginName + "]";
                return searchRet;
            }

            searchRet.records = records;  //返回的记录集合
            searchRet.resultCount = recordsCount;//records.Count; // 本次返回的记录数 
            searchRet.isCanNext = bNext;  //是否有下页
            searchRet.apiResult.errorCode = lRet;  //-1表示出错，0未命中，其它表示命中总数。
            return searchRet;
        }

        [HttpGet]
        public SearchApiResult SearchBiblio(string loginUserName,
            string loginUserType,
            string weixinId,
            string libId,
            string word,
            string from,
            string match,
            string resultSet)
        {
            return Search("biblio",
                loginUserName,
                loginUserType,
                weixinId,
                libId,
                word,
                from,
                match,
                resultSet);
        }

        [HttpGet]
        public SearchApiResult SearchItem(string loginUserName,
            string loginUserType,
            string weixinId,
            string libId,
            string word,
            string from,
            string match,
            string resultSet)
        {
            return Search("item",
                loginUserName,
                loginUserType,
                weixinId,
                libId,
                word,
                from,
                match,
                resultSet);
        }


        // 这个接口只给我爱图书馆自己使用，因为里面有一些专用的样式。
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


        // 获取书目详情，不包括册记录
        //loginUserName:使用的dp2library帐号，即前端当前绑定的帐户，如果是读者则传读者证条码；如果是馆员帐户，传馆员用户名
        //loginUserType：空表示馆员，如果是读者传"patron"
        //weixinId：前端用户唯一id,作用：当图书馆配置了不支持外部检索时，用weixinId检查该微信用户是否绑定了图书馆账户，如果未绑定，则不能检索。另外还用于获取该微信绑定帐户的分馆代码。
        //libId：图书馆id
        //biblioPath:书目路径
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

        // 获取书目下的册记录
        //loginUserName:使用的dp2library帐号，即前端当前绑定的帐户，如果是读者则传读者证条码；如果是馆员帐户，传馆员用户名
        //loginUserType：空表示馆员，如果是读者传"patron"
        //weixinId：前端用户唯一id
        //libId：图书馆id
        //biblioPath:书目路径
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


        // 编辑书目
        // loginUserName:使用的dp2library帐号,一般为馆员帐户
        // loginUserType：空表示馆员，如果是读者传"patron"
        // weixinId：前端用户唯一id
        // libId：图书馆id
        // biblioFields:书目对象，是一个结构，里面有路径、要编辑的字段。
        public SetBiblioResult SetBiblio(string loginUserName,
            string loginUserType,
            string weixinId,
            string libId,
            BiblioFields biblioFields) //前段简单先传一条书目，一开始想做成集合，但考虑到会带来复杂性，例如其中一条出错是否继续处理其它条，出错信息也要单独设置，返回值也有影响。
        {

            SetBiblioResult result = new SetBiblioResult();

            string strError = "";
            string outputBiblioPath = "";
            string outputTimestamp = "";
            int nRet = dp2WeiXinService.Instance.SetBiblio(loginUserName,
                loginUserType,
                weixinId,
                libId,
                biblioFields,
                out outputBiblioPath,
                out outputTimestamp,
                out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }

            result.errorInfo = strError;

            result.biblioPath = outputBiblioPath;
            result.biblioTimestamp = outputTimestamp;

            return result;
        }


        /*

        // 检索书目
        //loginUserName：检索使用的dp2library帐号，即前端当前绑定的帐户，如果是读者则传读者证条码；如果是馆员帐户，传馆员用户名
        //loginUserType：空表示馆员，如果是读者传"patron"
        //weixinId：前端用户唯一id，暂时没用到，后面可能会用到。
        //libId：图书馆id
        //word：检索词
        //from：检索途径，例如册条码、馆藏地等。检索途径还可以传一个特殊含义的功能：
        // _N表示下一页，此时word传是这次要获取记录的开始序号，结果集参数传前面检索返回的结果集。
        // _ReView表示重新获取数据，此时word传是要获取多少条记录 todo应用场景是什么？
        //match:匹配方式，简单检索时传的left
        //resultSet:前端指定的一个结果集名称，用于分批获取。
        //返回值：
        //searchRet.records = records;  //返回的记录集合
        //searchRet.resultCount = records.Count; // 本次返回的记录数 
        //searchRet.isCanNext = bNext;  //是否有下页
        //searchRet.apiResult.errorCode = lRet;  //-1表示出错，0未命中，其它表示命中总数。
        [HttpGet]
        public SearchItemResult SearchItem(string loginUserName,
            string loginUserType,
            string weixinId,
            string libId,
            string word,
            string from,
            string match,
            string resultSet)
        {
            SearchItemResult searchRet = new SearchItemResult();
            searchRet.apiResult = new ApiResult();
            searchRet.apiResult.errorCode = 0;
            searchRet.apiResult.errorInfo = "";
            searchRet.records = new List<BiblioItem>();
            searchRet.isCanNext = false;

            // 根据前端传的帐户创建LoginInfo
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

                if (from == "_N")  //当获取下一步时，如果结果集参数为空，则返回错误。
                {
                    searchRet.apiResult = new ApiResult();
                    searchRet.apiResult.errorCode = -1;
                    searchRet.apiResult.errorInfo = "当from参数为_N时，结果集名称不能为空。";
                    return searchRet;
                }

                // 设置一个新的结果集名称，后面传dp2检索接口，这个值也会放在接口返回结果中。
                resultSet = "weixin-" + Guid.NewGuid();
                searchRet.resultSetName = resultSet;
            }


            // 取下一页的情况
            if (from == "_N" || from == "_ReView")
            {
                // 将传入的字符转换成数字
                long lWord = 0;
                try
                {
                    lWord = Convert.ToInt32(word);
                    if (lWord < 0)
                    {
                        searchRet.apiResult.errorCode = -1;
                        searchRet.apiResult.errorInfo = "当from="+from+"时，传入的word值[" + word + "]格式不正确，必须是>=0。";
                        goto END1;
                    }
                }
                catch
                {
                    searchRet.apiResult.errorCode = -1;
                    searchRet.apiResult.errorInfo = "当from=" + from + "时，传入的word值[" + word + "]格式不正确，必须是数值。";
                    goto END1;
                }

                long start = 0;  //开始
                long count = 0;  //长度
                if (from == "_N")
                {
                    
                    start = lWord; //word值表示起始位置
                    count = WeiXinConst.C_OnePage_Count;


                }
                else if (from == "_ReView")  //刷新的情况，显示从0至当前最大数字
                {
                    if (resultSet.Substring(0, 1) == "#")
                        resultSet = resultSet.Substring(1);

                    start = 0; //word值表示起始位置
                    count = lWord;//重新显示，此时word代表数量
                }

                searchRet = dp2WeiXinService.Instance.getFromResultSet(loginInfo,
    weixinId,
    libId,
    resultSet,
    num,
    WeiXinConst.C_OnePage_Count);
                goto END1;

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

        */
    }


}