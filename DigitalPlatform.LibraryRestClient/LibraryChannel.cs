using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Xml;
using System.ServiceModel;
using System.Globalization;
using System.ServiceModel.Security;
using System.ServiceModel.Channels;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security.Tokens;
using DigitalPlatform;
using System.Runtime.Serialization.Json;
using System.Web;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryRestClient
{
    /// <summary>
    /// 通讯通道
    /// </summary>
    public class LibraryChannel
    {
        // 检索限制最大命中数常量
        public const int C_Search_MaxCount = 100;

        /// <summary>
        /// dp2Library 服务器的 URL
        /// </summary>
        public string Url = "";
        public string UserName = "";
        public string Password = "";

        /// <summary>
        /// 当前通道所使用的 HTTP Cookies
        /// </summary>
        public CookieContainer Cookies = new System.Net.CookieContainer();

        /// <summary>
        /// 当前通道的登录前事件
        /// </summary>
        public event BeforeLoginEventHandle BeforeLogin;

        // 重登录次数
        int _loginCount = 0;

        /// <summary>
        /// 最近一次调用从 dp2Library 返回的错误码
        /// </summary>
        public ErrorCode ErrorCode = ErrorCode.NoError;


        // return:
        //      -1  error
        //      0   dp2Library的版本号过低。警告信息在strError中
        //      1   dp2Library版本号符合要求
        public long GetVersion(out string version,
            out string strError)
        {
            strError = "";
            version = "";
            try
            {
                CookieAwareWebClient client = new CookieAwareWebClient(Cookies);
                client.Headers["Content-type"] = "application/json; charset=utf-8";

                byte[] data = new byte[0];
                byte[] result = client.UploadData(GetRestfulApiUrl("getversion"),
                        "POST",
                        data);

                string strResult = Encoding.UTF8.GetString(result);
                GetVersionResponse response = Deserialize<GetVersionResponse>(strResult);
                version = response.GetVersionResult.ErrorInfo;
                strError = response.GetVersionResult.ErrorInfo;

                return response.GetVersionResult.Value;
            }
            catch (Exception ex)
            {
                strError = "Exception :" + ex.Message;
                return -1;
            }
        }


        /// <summary>
        /// 专门检索微信用户对应的图书馆账号
        /// </summary>
        /// <param name="strUserOpenId"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public long SearchReaderByWeiXinId(string strWeiXinId,
            out string strError)
        {
            return this.SearchReader("",
                strWeiXinId,
                -1,
                "email",
                "exact",
                "zh",
                "weixin",
                "keyid",
                out strError);            
        }

        /// <summary>
        /// 检索读者记录。
        /// 请参考 dp2Library API SearchReader() 的详细说明
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strReaderDbNames">读者库名。可以为单个库名，也可以是逗号(半角)分割的读者库名列表。还可以为 &lt;全部&gt;/&lt;all&gt; 之一，表示全部读者库。</param>
        /// <param name="strQueryWord">检索词</param>
        /// <param name="nPerMax">一批检索命中的最大记录数。-1表示不限制</param>
        /// <param name="strFrom">检索途径</param>
        /// <param name="strMatchStyle">匹配方式。值为left/right/exact/middle之一</param>
        /// <param name="strLang">界面语言代码。例如 "zh"</param>
        /// <param name="strResultSetName">结果集名。可使用null，等同于 "default"。而指定有区分的结果集名，可让两批以上不同目的的检索结果集可以共存</param>
        /// <param name="strOutputStyle">输出风格。keyid / keycount 之一。缺省为 keyid</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>&gt;=0:  检索命中的记录数</para>
        /// </returns>
        public long SearchReader(string strReaderDbNames,
            string strQueryWord,
            int nPerMax,
            string strFrom,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strOutputStyle,
            out string strError)
        {
            strError = "";

        REDO:

            CookieAwareWebClient client = new CookieAwareWebClient(this.Cookies);
            client.Headers["Content-type"] = "application/json; charset=utf-8";
            
            // 请求实体
            SearchReaderRequest request = new SearchReaderRequest();
            request.strReaderDbNames = strReaderDbNames;// "";           
            request.strQueryWord = strQueryWord;// "";
            request.nPerMax = nPerMax;// -1;

            request.strFrom = strFrom;// "email";
            request.strMatchStyle = strMatchStyle;// "left";
            request.strLang = strLang;// "zh";

            request.strResultSetName = strResultSetName;//"";
            request.strOutputStyle = strOutputStyle;// "id,cols";      、

            byte[] baData = Encoding.UTF8.GetBytes(Serialize(request));
            byte[] result = client.UploadData(this.GetRestfulApiUrl("SearchReader"),
                                                "POST",
                                                baData);
            string strResult = Encoding.UTF8.GetString(result);
            SearchReaderResponse response = Deserialize<SearchReaderResponse>(strResult);
            if (response.SearchReaderResult.Value == -1 && response.SearchReaderResult.ErrorCode == ErrorCode.NotLogin)
            {
                if (DoNotLogin(ref strError) == 1)
                    goto REDO;
                return -1;
            }

            return response.SearchReaderResult.Value;
        }

        /// <summary>
        /// 获取根据微信id检索到的唯一读者记录
        /// </summary>
        /// <param name="strPath">读者记录路径</param>
        /// <param name="strXml">读者xml</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>&gt;=0:  结果集内的记录数。注意，不是本次调用返回的结果数</para>
        /// </returns>
        public long GetSearchResultForWeiXinUser(
            out string strPath,
            out string strXml,
            out string strError)
        {
            strPath = "";
            strXml = "";

            Record[] searchresults = null;
            long lRet = this.GetSearchResult("weixin",
                0,
                -1,
                "id,xml",
                "zh",
                out searchresults,
                out strError);
            if (searchresults.Length != 1)
            {
                throw new Exception("获得的记录数不是1");
            }

            strPath = searchresults[0].Path;
            strXml = searchresults[0].RecordBody.Xml;

            return lRet;
        }

        /// <summary>
        /// 获得检索结果。
        /// 请参考关于 dp2Library API GetSearchResult() 的介绍
        /// </summary>
        /// <param name="strResultSetName">结果集名。如果为空，表示使用当前缺省结果集"default"</param>
        /// <param name="lStart"> 要获取的开始位置。从0开始计数</param>
        /// <param name="lCount">要获取的个数</param>
        /// <param name="strBrowseInfoStyle">返回信息的方式。
        /// id / cols / xml / timestamp / metadata / keycount / keyid 的组合。keycount 和 keyid 二者只能使用一个，缺省为 keyid。
        /// 还可以组合使用 format:???? 这样的子串，表示使用特定的浏览列格式
        /// </param>
        /// <param name="strLang">语言代码,一般为"zh"</param>
        /// <param name="searchresults">返回 Record 对象数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>&gt;=0:  结果集内的记录数。注意，不是本次调用返回的结果数</para>
        /// </returns>
        public long GetSearchResult(
            string strResultSetName,
            long lStart,
            long lCount,
            string strBrowseInfoStyle,
            string strLang,
            out Record[] searchresults,
            out string strError)
        {
            searchresults = null;
            strError = "";            
            REDO:
            try
            {
                CookieAwareWebClient client = new CookieAwareWebClient(this.Cookies);
                client.Headers["Content-type"] = "application/json; charset=utf-8";

                GetSearchResultRequest request = new GetSearchResultRequest();
                request.strResultSetName = strResultSetName;
                request.lStart = lStart;
                request.lCount = lCount;
                request.strBrowseInfoStyle = strBrowseInfoStyle;
                request.strLang = strLang;

                byte[] baData = Encoding.UTF8.GetBytes(Serialize(request));
                byte[] result = client.UploadData(this.GetRestfulApiUrl("getsearchresult"),
                                                    "POST",
                                                    baData);

                string strResult = Encoding.UTF8.GetString(result);
                GetSearchResultResponse response = Deserialize<GetSearchResultResponse>(strResult);

                if (response.GetSearchResultResult.Value == -1 && response.GetSearchResultResult.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = response.GetSearchResultResult.ErrorInfo;

                searchresults = response.searchresults;
                this.ClearRedoCount();

                return response.GetSearchResultResult.Value;
            }
            catch (Exception ex)
            {
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
            
        }

        // 获得读者记录
        /// <summary>
        /// 获得读者记录
        /// 请参考 dp2Library API GetReaderInfo() 的详细说明
        /// </summary>
        /// <param name="stop">Stop 对象</param>
        /// <param name="strBarcode">读者证条码号，或者命令参数</param>
        /// <param name="strResultTypeList">希望获得的返回结果类型的列表。为 xml / html / text / calendar / advancexml / timestamp 的组合</param>
        /// <param name="results">返回结果信息的字符串数组</param>
        /// <param name="strRecPath">返回实际获取的记录的路径</param>
        /// <param name="baTimestamp">返回时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    没有找到读者记录</para>
        /// <para>1:    找到读者记录</para>
        /// <para>&gt;>1:   找到多于一条读者记录，返回值是找到的记录数，这是一种不正常的情况</para>
        /// </returns>
        public GetReaderInfoResponse GetReaderInfo(
            string strBarcode,
            string strResultTypeList)
        {
            string strError = "";

        REDO:
            try
            {
                CookieAwareWebClient client = new CookieAwareWebClient(this.Cookies);
                client.Headers["Content-type"] = "application/json; charset=utf-8";

                GetReaderInfoRequest request = new GetReaderInfoRequest();
                request.strBarcode = strBarcode;
                request.strResultTypeList = strResultTypeList;
                byte[] baData = Encoding.UTF8.GetBytes(Serialize(request));
                byte[] result = client.UploadData(this.GetRestfulApiUrl("getreaderinfo"),
                    "POST",
                    baData);

                string strResult = Encoding.UTF8.GetString(result);

                GetReaderInfoResponse response = Deserialize<GetReaderInfoResponse>(strResult);

                if (response.GetReaderInfoResult.Value == -1 && response.GetReaderInfoResult.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return null;
                }

                return response;
            }
            catch (Exception ex)
            {
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return null;
                goto REDO; ;
            }
        }

        /// <summary>
        /// 写入读者记录,主要用于绑定和解绑微信用户
        /// </summary>
        /// <param name="strRecPath">读者记录路径</param>
        /// <param name="strNewXml">读者xml</param>
        /// <param name="strOldTimestamp">时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// <para>1:    成功，但部分字段被拒绝</para>
        /// </returns>
        public long SetReaderInfoForWeiXin(           
            string strRecPath,
            string strNewXml,
            string strOldTimestamp,
            out string strError)
        {
            string strExistingXml = "";
            string strSavedXml = "";
            string strSavedRecPath = "";
            byte[] baNewTimestamp = null;
            
            return this.SetReaderInfo("change",
                strRecPath,
                strNewXml,
                "",
                strOldTimestamp,
                out strExistingXml,
                out strSavedXml,
                out strSavedRecPath,
                out baNewTimestamp,
                out strError);
        }


        /// <summary>
        /// 写入读者记录。
        /// 请参考 dp2Library API SetReaderInfo() 的详细信息
        /// </summary>
        /// <param name="strAction">动作。为 new / change / delete /changestate / changeforegift 之一</param>
        /// <param name="strRecPath">记录路径</param>
        /// <param name="strNewXml">新记录 XML</param>
        /// <param name="strOldXml">旧记录 XML</param>
        /// <param name="baOldTimestamp">时间戳</param>
        /// <param name="strExistingXml">返回数据库中已经存在的记录的 XML</param>
        /// <param name="strSavedXml">返回实际保存的记录 XML</param>
        /// <param name="strSavedRecPath">返回实际保存记录的路径</param>
        /// <param name="baNewTimestamp">返回最新时间戳</param>
        /// <param name="kernel_errorcode">内核错误码</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    成功</para>
        /// <para>1:    成功，但部分字段被拒绝</para>
        /// </returns>
        public long SetReaderInfo(
            string strAction,
            string strRecPath,
            string strNewXml,
            string strOldXml,
            string strOldTimestamp,
            out string strExistingXml,
            out string strSavedXml,
            out string strSavedRecPath,
            out byte[] baNewTimestamp,
            out string strError)
        {
            strError = "";
            strExistingXml = "";
            strSavedXml = "";
            strSavedRecPath = "";
            baNewTimestamp = null;
        REDO:
            try
            {
                CookieAwareWebClient client = new CookieAwareWebClient(this.Cookies);
                client.Headers["Content-type"] = "application/json; charset=utf-8";

                SetReaderInfoRequest request = new SetReaderInfoRequest();
                request.strAction =strAction;
                request.strRecPath = strRecPath;
                request.strNewXml = strNewXml;
                request.strOldXml = strOldXml;
                request.baOldTimestamp = StringUtil.GetTimeStampByteArray(strOldTimestamp);
                byte[] baData = Encoding.UTF8.GetBytes(Serialize(request));
                byte[] result = client.UploadData(this.GetRestfulApiUrl("setreaderinfo"),
                    "POST",
                    baData);

                string strResult = Encoding.UTF8.GetString(result);

                SetReaderInfoResponse response = Deserialize<SetReaderInfoResponse>(strResult);
                if (response.SetReaderInfoResult != null)
                {
                    if (response.SetReaderInfoResult.Value == -1
                        && response.SetReaderInfoResult.ErrorCode == ErrorCode.NotLogin)
                    {
                        if (DoNotLogin(ref strError) == 1)
                            goto REDO;
                        return -1;
                    }
                    strError = response.SetReaderInfoResult.ErrorInfo;
                    this.ErrorCode = response.SetReaderInfoResult.ErrorCode;
                }
                this.ClearRedoCount();
                return response.SetReaderInfoResult.Value;
            }
            catch (Exception ex)
            {
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
             
        }


        /// <summary>
        /// 借/续借
        /// </summary>
        /// <param name="strReaderBarcode">读者证条码号</param>
        /// <param name="strItemBarcode">册条码</param>
        /// <param name="borrow_info">返回借阅信息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    操作成功</para>/// 
        /// </returns>
        public int Borrow(bool bRenew,
            string strReaderBarcode,
            string strItemBarcode,
            out BorrowInfo borrow_info,
            out string strError)
        {
            borrow_info = null;
            strError = "";

        REDO:
            try
            {
                CookieAwareWebClient client = new CookieAwareWebClient(this.Cookies);
                client.Headers["Content-type"] = "application/json; charset=utf-8";


                /// <param name="bRenew">是否为续借。true 表示xujie；false 表示普通借阅</param>
                /// <param name="strReaderBarcode">读者证条码号，或读者身份证号</param>
                /// <param name="strItemBarcode">要借阅的册条码号</param>
                /// <param name="strConfirmItemRecPath">用于确认册记录的路径</param>
                /// <param name="bForce">此参数目前未使用，设为 false 即可</param>
                /// <param name="saBorrowedItemBarcode">针对同一读者的连续操作中已经借阅的册条码号数组。用于在读者信息 HTML 界面上为这些册的信息行设置特殊背景色</param>
                /// <param name="strStyle">操作风格</param>
                /// <param name="strItemFormatList">指定在 item_records 参数中返回信息的格式列表</param>
                /// <param name="item_records">返回册相关的信息数组</param>
                /// <param name="strReaderFormatList">指定在 reader_records 参数中返回信息的各式列表</param>
                /// <param name="reader_records">返回读者相关的信息数组</param>
                /// <param name="strBiblioFormatList">指定在 biblio_records 参数中返回信息的格式列表</param>
                /// <param name="biblio_records">返回书目相关的信息数组</param>
                /// <param name="aDupPath">如果发生条码号重复，这里返回了相关册记录的路径</param>
                /// <param name="strOutputReaderBarcode">返回实际操作针对的读者证条码号</param>
                /// <param name="borrow_info">返回 BorrowInfo 结构对象，里面是一些关于借阅的详细信息</param>
                /// <param name="strError">返回出错信息</param>
                BorrowRequest request = new BorrowRequest();
                request.bRenew = bRenew;
                request.strReaderBarcode = strReaderBarcode;
                request.strItemBarcode = strItemBarcode;
                request.strConfirmItemRecPath = "";
                request.bForce = false;
                request.saBorrowedItemBarcode = null;//??
                request.strStyle = ""; //??
                request.strItemFormatList = "";//???
                request.strReaderFormatList = "";//??
                request.strBiblioFormatList = "";


                byte[] baData = Encoding.UTF8.GetBytes(Serialize(request));
                byte[] result = client.UploadData(this.GetRestfulApiUrl("Borrow"),
                    "POST",
                    baData);

                string strResult = Encoding.UTF8.GetString(result);

                BorrowResponse response = Deserialize<BorrowResponse>(strResult);
                // 未登录的情况
                if (response.BorrowResult.Value == -1 && response.BorrowResult.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                borrow_info = response.borrow_info;
                strError = response.BorrowResult.ErrorInfo;
                this.ErrorCode = response.BorrowResult.ErrorCode;
                this.ClearRedoCount();
                return (int)response.BorrowResult.Value;
            }
            catch (Exception ex)
            {
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }

        ///还书
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    操作成功</para>
        /// <para>1:    操作成功，并且有值得操作人员留意的情况。提示信息在 strError 中</para>
        /// </returns>
        public int Return(string strItemBarcode,
            out string strOutputReaderBarcode,
            out ReturnInfo return_info,
            out string strError)
        {
            strOutputReaderBarcode = "";
            return_info = null;
            strError = "";

        REDO:
            try
            {
                CookieAwareWebClient client = new CookieAwareWebClient(this.Cookies);
                client.Headers["Content-type"] = "application/json; charset=utf-8";


                // 还书
                // return:
                //      -1  出错
                //      0   正常
                //      1   有超期情况
                /// <summary>
                /// 还书或声明丢失
                /// </summary>
                /// <param name="stop">Stop 对象</param>
                /// <param name="strAction">动作参数。为 return lost 之一</param>
                /// <param name="strReaderBarcode">读者证条码号，或读者身份证号</param>
                /// <param name="strItemBarcode">要还回或声明丢失的册条码号</param>
                /// <param name="strConfirmItemRecPath">用于确认册记录的路径</param>
                
                /// <param name="bForce">此参数目前未使用，设为 false 即可</param>
                /// <param name="strStyle">操作风格</param>
                /// <param name="strItemFormatList">指定在 item_records 参数中返回信息的格式列表</param>
                /// <param name="item_records">返回册相关的信息数组</param>
               
                /// <param name="strReaderFormatList">指定在 reader_records 参数中返回信息的各式列表</param>
                /// <param name="reader_records">返回读者相关的信息数组</param>
                /// <param name="strBiblioFormatList">指定在 biblio_records 参数中返回信息的格式列表</param>
                /// <param name="biblio_records">返回书目相关的信息数组</param>
                /// <param name="aDupPath">如果发生条码号重复，这里返回了相关册记录的路径</param>
                /// <param name="strOutputReaderBarcode">返回实际操作针对的读者证条码号</param>
                /// <param name="return_info">返回 ReturnInfo 结构对象，里面是一些关于还书的详细信息</param>
                /// <param name="strError">返回出错信息</param>
                /// <returns>
                /// <para>-1:   出错</para>
                /// <para>0:    操作成功</para>
                /// <para>1:    操作成功，并且有值得操作人员留意的情况。提示信息在 strError 中</para>
                /// </returns>
                ReturnRequest request = new ReturnRequest();
                request.strAction = "return";
                request.strReaderBarcode = "";
                request.strItemBarcode = strItemBarcode;
                request.strConfirmItemRecPath = "";
                request.bForce = false;
                request.strStyle = ""; 
                request.strItemFormatList = "";
                request.strReaderFormatList = "";
                request.strBiblioFormatList = "";

                byte[] baData = Encoding.UTF8.GetBytes(Serialize(request));
                byte[] result = client.UploadData(this.GetRestfulApiUrl("Return"),
                    "POST",
                    baData);

                string strResult = Encoding.UTF8.GetString(result);
                ReturnResponse response = Deserialize<ReturnResponse>(strResult);
                // 未登录的情况
                if (response.ReturnResult.Value == -1 && response.ReturnResult.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                return_info = response.return_info;
                strError = response.ReturnResult.ErrorInfo;
                strOutputReaderBarcode = response.strOutputReaderBarcode;
                this.ErrorCode = response.ReturnResult.ErrorCode;
                this.ClearRedoCount();
                return (int)response.ReturnResult.Value;
            }
            catch (Exception ex)
            {
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }

        /// <summary>
        /// 检索书目
        /// </summary>
        /// <param name="strWord">检索词</param>
        /// <param name="strError">返回出错信息</param>
        /// <para>-1:   出错</para>
        /// <para>0:    没有命中</para>
        /// <para>&gt;=1:   命中。值为命中的记录条数</para>
        /// <returns></returns>
        public long SearchBiblio(
            string strWord,
            string strFromStyle,
            out string strError)
        {
            strError = "";
        REDO:
            try
            {

                CookieAwareWebClient client = new CookieAwareWebClient(this.Cookies);
                client.Headers["Content-type"] = "application/json; charset=utf-8";

                SearchBiblioRequest request = new SearchBiblioRequest();
                request.strQueryWord = strWord;
                request.strBiblioDbNames = ""; //??
                request.nPerMax = C_Search_MaxCount; //500;// -1;
                request.strLang = "zh";
                request.strSearchStyle = "";// "desc";
                request.strOutputStyle = "id,cols";
                request.strFromStyle = strFromStyle;
                request.strMatchStyle = "middle";
                request.strResultSetName = "weixin-biblio";

                byte[] baData = Encoding.UTF8.GetBytes(Serialize(request));

                string strRequest = Encoding.UTF8.GetString(baData);

                byte[] result = client.UploadData(this.GetRestfulApiUrl("SearchBiblio"),
                                "POST",
                                 baData);

                string strResult = Encoding.UTF8.GetString(result);

                SearchBiblioResponse response = Deserialize<SearchBiblioResponse>(strResult);

                // 未登录的情况
                if (response.SearchBiblioResult.Value == -1 && response.SearchBiblioResult.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }

                strError = response.SearchBiblioResult.ErrorInfo;
                this.ErrorCode = response.SearchBiblioResult.ErrorCode;
                this.ClearRedoCount();
                return response.SearchBiblioResult.Value;
            }
            catch (Exception ex)
            {
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }

        /// <summary>
        /// 获得书目检索结果
        /// </summary>
        /// <param name="lStart">开始序号，从0开始</param>
        /// <param name="lCount">数量</param>
        /// <param name="resultPathList">路径数组，格式为：路径*书名</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    没有命中</para>
        /// <para>&gt;=1:   命中。值为命中的记录条数</para>
        /// </returns>
        public long GetBiblioSearchResult(long lStart,
            long lCount,
            out List<string> resultPathList,
            out string strError)
        {
            resultPathList = new List<string>();
            Record[] searchresults = null;
            long lRet = this.GetSearchResult("weixin-biblio",
                lStart,
                lCount,
                "id,cols",
                "zh",
                out searchresults,
                out strError);
            if (searchresults.Length <= 0)
            {
                strError = "获取目录结果集异常：" + strError;
                return -1;
            }

            for (int i = 0; i < searchresults.Length;i++ )
            {
                resultPathList.Add(searchresults[i].Path + "*" + searchresults[i].Cols[0]);
            }

            return searchresults.Length;
        }


        /// <summary>
        /// 得到一条书目记录的浏览格式
        /// </summary>
        /// <param name="strRecPath">书目记录路径</param>
        /// <param name="strBrowse">返回浏览格式</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    没有找到指定路径的书目记录</para>
        /// <para>1:    成功</para>/// 
        /// </returns>
        public long GetBiblioDetail(string strRecPath,
            out string strBrowse,
            out string strError)
        {
            strBrowse = "";
            strError = "";

        REDO:
            try
            {
                CookieAwareWebClient client = new CookieAwareWebClient(this.Cookies);
                client.Headers["Content-type"] = "application/json; charset=utf-8";                

                GetBrowseRecordsRequest request = new GetBrowseRecordsRequest();
                request.paths = new string[] {strRecPath };
                request.strBrowseInfoStyle = "cols";

                byte[] baData = Encoding.UTF8.GetBytes(Serialize(request));
                string strRequest = Encoding.UTF8.GetString(baData);
                byte[] result = client.UploadData(this.GetRestfulApiUrl("GetBrowseRecords"),
                                "POST",
                                 baData);

                string strResult = Encoding.UTF8.GetString(result);

                GetBrowseRecordsResponse response = Deserialize<GetBrowseRecordsResponse>(strResult);
                if (response.GetBrowseRecordsResult.Value == -1 && response.GetBrowseRecordsResult.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = response.GetBrowseRecordsResult.ErrorInfo;

                Record record = response.searchresults[0];
                strBrowse = "题名:"+record.Cols[0]+"\n"
                    +"责任说明:"+record.Cols[1]+"\n"
                    + "分类号:" + record.Cols[2] + "\n"
                    + "主题词:" + record.Cols[3] + "\n"
                    + "出版者:" + record.Cols[4] + "\n"
                    + "出版时间:" + record.Cols[5] ;

                this.ClearRedoCount();
                return response.GetBrowseRecordsResult.Value;
            }
            catch (Exception ex)
            {
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }

        /// <summary>
        /// 获取书目摘要
        /// </summary>
        /// <param name="strItemBarcode"></param>
        /// <returns></returns>
        public string GetBiblioSummary(string strItemBarcode)
        {
            string strError = "";
        REDO:

            try
            {
                CookieAwareWebClient client = new CookieAwareWebClient(this.Cookies);
                client.Headers["Content-type"] = "application/json; charset=utf-8";

                GetBiblioSummaryRequest request = new GetBiblioSummaryRequest();
                request.strItemBarcode = strItemBarcode; // 册条码号
                request.strConfirmItemRecPath = "";//this.GetBiblioSummary_textBox_strConfirmItemRecPath.Text; // 记录路径
                request.strBiblioRecPathExclude = "";//null; // 希望排除掉的书目记录路径


                byte[] baData = Encoding.UTF8.GetBytes(Serialize(request));
                byte[] result = client.UploadData(this.GetRestfulApiUrl("GetBiblioSummary"),
                                                    "POST",
                                                    baData);

                string strResult = Encoding.UTF8.GetString(result);

                GetBiblioSummaryResponse response = Deserialize<GetBiblioSummaryResponse>(strResult);
                if (response.GetBiblioSummaryResult.Value == -1 && response.GetBiblioSummaryResult.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return strError;
                }

                return response.strSummary;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // 验证读者密码
        /// <summary>
        /// 验证读者帐户的密码
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="strReaderBarcode">读者证条码号</param>
        /// <param name="strReaderPassword">要验证的读者帐户密码</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   验证过程出错</para>
        /// <para>0:    密码不正确</para>
        /// <para>1:    密码正确</para>
        /// </returns>
        public long VerifyReaderPassword(
            string strReaderBarcode,
            string strReaderPassword,
            out string strError)
        {
            strError = "";
        REDO:
            try
            {
                CookieAwareWebClient client = new CookieAwareWebClient(this.Cookies);
                client.Headers["Content-type"] = "application/json; charset=utf-8";

                VerifyReaderPasswordRequest request = new VerifyReaderPasswordRequest();
                request.strReaderBarcode = strReaderBarcode;
                request.strReaderPassword = strReaderPassword;
                byte[] baData = Encoding.UTF8.GetBytes(Serialize(request));
                byte[] result = client.UploadData(this.GetRestfulApiUrl("verifyreaderpassword"),
                    "POST",
                    baData);

                string strResult = Encoding.UTF8.GetString(result);
                VerifyReaderPasswordResponse response = Deserialize<VerifyReaderPasswordResponse>(strResult);

                if (response.VerifyReaderPasswordResult.Value == -1
                    && response.VerifyReaderPasswordResult.ErrorCode == ErrorCode.NotLogin)
                {
                    if (DoNotLogin(ref strError) == 1)
                        goto REDO;
                    return -1;
                }
                strError = response.VerifyReaderPasswordResult.ErrorInfo;
                this.ErrorCode = response.VerifyReaderPasswordResult.ErrorCode;
                this.ClearRedoCount();
                return response.VerifyReaderPasswordResult.Value;
            }
            catch (Exception ex)
            {
                int nRet = ConvertWebError(ex, out strError);
                if (nRet == 0)
                    return -1;
                goto REDO;
            }
        }


        #region 登录相关函数

        /// <summary>
        /// 登录。
        /// 请参考关于 dp2Library API Login() 的详细说明。
        /// </summary>
        /// <param name="strUserName">用户名</param>
        /// <param name="strPassword">密码</param>
        /// <param name="strParameters">登录参数。这是一个逗号间隔的列表字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:   出错</para>
        /// <para>0:    登录未成功</para>
        /// <para>1:    登录成功</para>
        /// </returns>
        public LoginResponse Login(string strUserName,
            string strPassword,
            string strParameters)
        {

            CookieAwareWebClient client = new CookieAwareWebClient(Cookies);
            client.Headers["Content-type"] = "application/json; charset=utf-8";

            LoginRequest request = new LoginRequest();
            request.strUserName = strUserName;
            request.strPassword = strPassword;
            request.strParameters = "";// "location=#web,type=reader"; 
            byte[] baData = Encoding.UTF8.GetBytes(Serialize(request));

            byte[] result = client.UploadData(this.GetRestfulApiUrl("login"),
                "POST",
                baData);

            string strResult = Encoding.UTF8.GetString(result);

            LoginResponse response = Deserialize<LoginResponse>(strResult);
            return response;

        }


        /// <summary>
        /// 清零登录次数
        /// </summary>
        void ClearRedoCount()
        {
            this.m_nRedoCount = 0;
        }
        int m_nRedoCount = 0;   // MessageSecurityException以后重试的次数

        /// <summary>
        /// 处理登录事宜
        /// </summary>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 登录成功</returns>
        public int DoNotLogin(ref string strError)
        {
            this.ClearRedoCount();

            if (this.BeforeLogin != null)
            {
                BeforeLoginEventArgs ea = new BeforeLoginEventArgs();
                ea.LibraryServerUrl = this.Url;
                ea.FirstTry = true;
                ea.ErrorInfo = strError;

            REDOLOGIN:
                this.BeforeLogin(this, ea);

                if (ea.Cancel == true)
                {
                    if (String.IsNullOrEmpty(ea.ErrorInfo) == true)
                        strError = "用户放弃登录";
                    else
                        strError = ea.ErrorInfo;
                    return -1;
                }

                if (ea.Failed == true)
                {
                    strError = ea.ErrorInfo;
                    return -1;
                }

                // 2006/12/30
                if (this.Url != ea.LibraryServerUrl)
                {
                    this.Close();   // 迫使重新构造m_ws 2011/11/22
                    this.Url = ea.LibraryServerUrl;
                }

                string strMessage = "";
                if (ea.FirstTry == true)
                    strMessage = strError;

                if (_loginCount > 100)
                {
                    strError = "重新登录次数太多，超过 100 次，请检查登录 API 是否出现了逻辑问题";
                    _loginCount = 0;    // 重新开始计算
                    return -1;
                }
                _loginCount++;

                LoginResponse lRet = this.Login(ea.UserName,
                    ea.Password,
                    ea.Parameters);
                if (lRet.LoginResult.Value == -1 || lRet.LoginResult.Value == 0)
                {
                    if (String.IsNullOrEmpty(strMessage) == false)
                        ea.ErrorInfo = strMessage + "\r\n\r\n首次自动登录报错: ";
                    else
                        ea.ErrorInfo = "";
                    ea.ErrorInfo += strError;
                    ea.FirstTry = false;
                    ea.LoginFailCondition = LoginFailCondition.PasswordError;
                    goto REDOLOGIN;
                }

                /*
                // this.m_nRedoCount = 0;
                if (this.AfterLogin != null)
                {
                    AfterLoginEventArgs e1 = new AfterLoginEventArgs();
                    this.AfterLogin(this, e1);
                    if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                    {
                        strError = e1.ErrorInfo;
                        return -1;
                    }
                }
                 */
                return 1;   // 登录成功,可以重做API功能了
            }

            return -1;
        }

        #endregion

        public void Close()
        {
            /*
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                if (this.m_ws != null)
                {
                    // TODO: Search()要单独处理
                    try
                    {
                        if (this.m_ws.State != CommunicationState.Faulted)
                            this.m_ws.Close();
                    }
                    catch
                    {
                        if (this.m_ws != null)
                            this.m_ws.Abort();
                    }
                    this.m_ws = null;
                }
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
             */
        }

        /// <summary>
        /// 立即放弃通讯。而 Abort() 要文雅一些
        /// </summary>
        public void AbortIt()
        {
            /*
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                if (this.m_ws != null)
                {
                    this.m_ws.Abort();
                    this.m_ws = null;
                }
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
             */
        }

        #region 公共函数

        /// <summary>
        /// 拼出接口url地址
        /// </summary>
        /// <param name="strMethod"></param>
        /// <returns></returns>
        private string GetRestfulApiUrl(string strMethod)
        {
            if (string.IsNullOrEmpty(this.Url) == true)
                return strMethod;

            if (this.Url[this.Url.Length - 1] == '/')
                return this.Url + strMethod;

            return this.Url + "/" + strMethod;
        }

        /// <summary>
        /// 获得异常信息
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <returns></returns>
        public static string GetExceptionMessage(Exception ex)
        {
            string strResult = ex.GetType().ToString() + ":" + ex.Message;
            while (ex != null)
            {
                if (ex.InnerException != null)
                    strResult += "\r\n" + ex.InnerException.GetType().ToString() + ": " + ex.InnerException.Message;

                ex = ex.InnerException;
            }

            return strResult;
        }

        // return:
        //      0   主流程需返回-1
        //      1   需要重做API
        int ConvertWebError(Exception ex0,
            out string strError)
        {
            strError = "";

            // this.WcfException = ex0;

            // System.TimeoutException
            if (ex0 is System.TimeoutException)
            {
                this.ErrorCode = ErrorCode.RequestTimeOut;
                //this.AbortIt();
                strError = GetExceptionMessage(ex0);
                return 0;
            }

            if (ex0 is System.ServiceModel.Security.MessageSecurityException)
            {
                System.ServiceModel.Security.MessageSecurityException ex = (System.ServiceModel.Security.MessageSecurityException)ex0;
                this.ErrorCode = ErrorCode.RequestError;	// 一般错误
                //this.AbortIt();
                // return ex.Message + (ex.InnerException != null ? " InnerException: " + ex.InnerException.Message : "") ;
                strError = GetExceptionMessage(ex);
                if (this.m_nRedoCount == 0)
                {
                    this.m_nRedoCount++;
                    return 1;   // 重做
                }
                return 0;
            }

            if (ex0 is CommunicationObjectFaultedException)
            {
                CommunicationObjectFaultedException ex = (CommunicationObjectFaultedException)ex0;
                this.ErrorCode = ErrorCode.RequestError;	// 一般错误
                // this.AbortIt();
                strError = GetExceptionMessage(ex);
                // 2011/7/2
                if (this.m_nRedoCount == 0)
                {
                    this.m_nRedoCount++;
                    return 1;   // 重做
                }
                return 0;
            }

            if (ex0 is EndpointNotFoundException)
            {
                EndpointNotFoundException ex = (EndpointNotFoundException)ex0;
                this.ErrorCode = ErrorCode.RequestError;	// 一般错误
                //this.AbortIt();
                strError = "服务器 " + this.Url + " 没有响应";
                return 0;
            }

            if (ex0 is System.ServiceModel.CommunicationException
                && ex0.InnerException is System.ServiceModel.QuotaExceededException)
            {
                /*
                this.ErrorCode = ErrorCode.RequestError;	// 一般错误
                this.MaxReceivedMessageSize *= 2;    // 下次扩大一倍
                this.AbortIt();
                strError = GetExceptionMessage(ex0);
                if (this.m_nRedoCount == 0
                    && this.MaxReceivedMessageSize < 1024 * 1024 * 10)
                {
                    this.m_nRedoCount++;
                    return 1;   // 重做
                }
                 */
                return 0;
            }

            this.ErrorCode = ErrorCode.RequestError;	// 一般错误
            /*
            if (this.m_ws != null)
            {
                this.AbortIt();
                // 一般来说异常都需要重新分配Client()。如果有例外，可以在前面分支
            }
             */
            strError = GetExceptionMessage(ex0);
            return 0;
        }

        #endregion

        #region 2个json序列化为类的方法

        public static T Deserialize<T>(string json)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                obj = (T)serializer.ReadObject(ms);
                return obj;
            }
        }

        public static string Serialize<T>(T obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        #endregion
    }

    /// <summary>
    /// 登录失败的原因
    /// </summary>
    public enum LoginFailCondition
    {
        /// <summary>
        /// 没有出错
        /// </summary>
        None = 0,   // 没有出错
        /// <summary>
        /// 一般错误
        /// </summary>
        NormalError = 1,    // 一般错误
        /// <summary>
        /// 密码不正确
        /// </summary>
        PasswordError = 2,  // 密码不正确
    }

    /// <summary>
    /// 登录前的事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void BeforeLoginEventHandle(object sender,
    BeforeLoginEventArgs e);

    /// <summary>
    /// 登陆前时间的参数
    /// </summary>
    public class BeforeLoginEventArgs : EventArgs
    {
        /// <summary>
        /// [in] 是否为第一次触发
        /// </summary>
        public bool FirstTry = true;    // [in] 是否为第一次触发
        /// <summary>
        /// [in] 图书馆应用服务器 URL
        /// </summary>
        public string LibraryServerUrl = "";    // [in] 图书馆应用服务器URL

        /// <summary>
        /// [out] 用户名
        /// </summary>
        public string UserName = "";    // [out] 用户名
        /// <summary>
        /// [out] 密码
        /// </summary>
        public string Password = "";    // [out] 密码
        /// <summary>
        /// [out] 工作台号
        /// </summary>
        public string Parameters = "";    // [out] 工作台号

        /// <summary>
        /// [out] 事件调用是否失败
        /// </summary>
        public bool Failed = false; // [out] 事件调用是否失败
        /// <summary>
        /// [out] 事件调用是否被放弃
        /// </summary>
        public bool Cancel = false; // [out] 事件调用是否被放弃

        /// <summary>
        /// [in, out] 事件调用错误信息
        /// </summary>
        public string ErrorInfo = "";   // [in, out] 事件调用错误信息

        /// <summary>
        /// [in, out] 前次登录失败的原因，本次登录失败的原因
        /// </summary>
        public LoginFailCondition LoginFailCondition = LoginFailCondition.NormalError;
    }



}
