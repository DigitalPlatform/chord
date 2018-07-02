using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using static DigitalPlatform.Z3950.ZChannel;
using DigitalPlatform.Text;

namespace DigitalPlatform.Z3950
{
    /// <summary>
    /// Z39.50 前端类。维持通讯通道，提供 Z39.50 请求 API
    /// </summary>
    public class ZClient : IDisposable
    {
        ZChannel _channel = new ZChannel();

        string _currentRefID = "0";

        // 经过字符集协商确定的记录强制使用的编码方式。如果为 null，表示未经过字符集协商
        public Encoding ForcedRecordsEncoding = null;

        public event EventHandler Closed = null;

#if NO
        public string RecordSyntax { get; set; }

        // 推荐的记录语法
        // 确保已经去掉了--部分
        public string PreferredRecordSyntax
        {
            get
            {
                // return "1.2.840.1003.5.109.10";

                if (String.IsNullOrEmpty(this.RecordSyntax) == true)
                    return GetLeftValue(this.TargetInfo.PreferredRecordSyntax);

                return GetLeftValue(this.RecordSyntax);
            }
        }

        // 解析出 '-' 左边的值
        public static string GetLeftValue(string strText)
        {
            int nRet = strText.IndexOf("-");
            if (nRet != -1)
                return strText.Substring(0, nRet).Trim();
            else
                return strText.Trim();
        }
#endif

        public void Dispose()
        {
            if (_channel != null)
                _channel.Dispose();
        }

        // Z39.50 初始化
        // 注：经过字符集协商，targetinfo 里面的某些成员会发生变化。因此需要把它保存下来继续使用
        // return Value:
        //      -1  出错
        //      0   成功
        //      1   调用前已经是初始化过的状态，本次没有进行初始化
        public async Task<InitialResult> TryInitialize(TargetInfo targetinfo)
        {
            {
                // 处理通讯缓冲区中可能残留的 Close Response
                // return value:
                //      -1  error
                //      0   不是Close
                //      1   是Close，已经迫使ZChannel处于尚未初始化状态
                InitialResult result = await CheckServerCloseRequest();
            }

            if (this._channel.Connected == false
                || this._channel.Initialized == false
    || this._channel.HostName != targetinfo.HostName
    || this._channel.Port != targetinfo.Port)
            {
                if (this._channel.Connected == false)
                {
                    Result result = await this._channel.Connect(targetinfo.HostName, targetinfo.Port);
                    if (result.Value == -1)
                        return new InitialResult { Value = -1, ErrorInfo = result.ErrorInfo };
                }

                // this.Stop.SetMessage("正在执行Z39.50初始化 ...");

                {
                    // return Value:
                    //      -1  出错
                    //      0   成功
                    InitialResult result = await Initial(
        targetinfo,
        targetinfo.IgnoreReferenceID,
        this._currentRefID);
                    if (result.Value == -1)
                        return result;

                    return result;
                }
            }

            return new InitialResult { Value = 1 };
        }

        public class InitialResult : Result
        {
            // 说明初始化结果的文字
            public string ResultInfo { get; set; }

            public InitialResult()
            {

            }

            public InitialResult(Result source)
            {
                Result.CopyTo(source, this);
            }

            public override string ToString()
            {
                StringBuilder text = new StringBuilder(base.ToString());
                text.Append("ResultInfo=" + this.ResultInfo + "\r\n");
                return text.ToString();
            }
        }

        // 执行初始化
        // 同步模式
        // parameters:
        //      strResultInfo   [out]返回说明初始化结果的文字
        // return Value:
        //      -1  出错
        //      0   成功
        async Task<InitialResult> Initial(
            TargetInfo targetinfo,
            bool bIgnoreReferenceID,
            string reference_id)
        {
            string strResultInfo = "";

            BerTree tree = new BerTree();
            INIT_REQUEST struInit_request = new INIT_REQUEST();

            // TargetInfo targetinfo = connection.TargetInfo;

            if (this._channel.Initialized == true)
                return new InitialResult { Value = -1, ErrorInfo = "先前已经初始化过了，不应重复初始化" };  // 不能重复调用

            struInit_request.m_strReferenceId = reference_id;
            struInit_request.m_strOptions = "yynnnnnnnnnnnnnnnn";   // "yyynynnyynynnnyn";

            struInit_request.m_lPreferredMessageSize = 0x100000; ////16384;
            struInit_request.m_lExceptionalRecordSize = 0x100000;

            if (String.IsNullOrEmpty(targetinfo.UserName) == false)
            {
                struInit_request.m_strID = targetinfo.UserName;
                struInit_request.m_strPassword = targetinfo.Password;
                struInit_request.m_strGroupID = targetinfo.GroupID;
                struInit_request.m_nAuthenticationMethod = targetinfo.AuthenticationMethod;
            }
            else
            {
                struInit_request.m_strID = "";
                struInit_request.m_strPassword = "";
                struInit_request.m_strGroupID = "";
                struInit_request.m_nAuthenticationMethod = -1;
            }

            struInit_request.m_strImplementationId = "DigitalPlatform";
            struInit_request.m_strImplementationVersion = "1.2.0";
            struInit_request.m_strImplementationName = "Z3950_library";

            if (targetinfo.CharNegoUTF8 == true)
            {
                struInit_request.m_charNego = new CharsetNeogatiation();
                struInit_request.m_charNego.EncodingLevelOID = CharsetNeogatiation.Utf8OID; //  "1.0.10646.1.0.8";   // utf-8
                struInit_request.m_charNego.RecordsInSelectedCharsets = (targetinfo.CharNegoRecordsUTF8 == true ? 1 : 0);
            }

            int nRet = tree.InitRequest(struInit_request,
                   targetinfo.DefaultQueryTermEncoding,
                   out byte[] baPackage);
            if (nRet == -1)
                return new InitialResult { Value = -1, ErrorInfo = "CBERTree::InitRequest() fail!" };

            if (this._channel.Connected == false)
            {
                this.CloseConnection();
                return new InitialResult { Value = -1, ErrorInfo = "socket尚未连接或者已经被关闭" };
            }



#if DUMPTOFILE
	DeleteFile("initrequest.bin");
	DumpPackage("initrequest.bin",
				(char *)baPackage.GetData(),
				baPackage.GetSize());
	DeleteFile ("initrequest.txt");
	tree.m_RootNode.DumpToFile("initrequest.txt");
#endif


            RecvResult result = await this._channel.SendAndRecv(
                baPackage);
            if (result.Value == -1)
                return new InitialResult(result);

#if DUMPTOFILE
	DeleteFile("initresponse.bin");
	DumpPackage("initresponse.bin",
				(char *)baOutPackage.GetData(),
				baOutPackage.GetSize());
#endif

            ////////////////////////////////////////////////////////////////
            BerTree tree1 = new BerTree();
            tree1.m_RootNode.BuildPartTree(result.Package,
                0,
                result.Package.Length,
                out int nTotalLen);


#if DUMPTOFILE
	DeleteFile("InitResponse.txt"); 
	tree1.m_RootNode.DumpDebugInfoToFile("InitResponse.txt");
#endif

            INIT_RESPONSE init_response = new INIT_RESPONSE();
            nRet = BerTree.GetInfo_InitResponse(tree1.GetAPDuRoot(),
                                 ref init_response,
                                 out string strError);
            if (nRet == -1)
                return new InitialResult { Value = -1, ErrorInfo = strError };


            if (bIgnoreReferenceID == false)
            {
                // 可以帮助发现旧版本dp2zserver的错误
                if (struInit_request.m_strReferenceId != init_response.m_strReferenceId)
                {
                    strError = "请求的 reference id [" + struInit_request.m_strReferenceId + "] 和 响应的 reference id [" + init_response.m_strReferenceId + "] 不一致！";
                    return new InitialResult { Value = -1, ErrorInfo = strError };
                }
            }

            // 2007/11/5检查version和options
            bool bOption_0 = BerTree.GetBit(init_response.m_strOptions,
                0);
            if (bOption_0 == false)
            {
                strError = "服务器响应的 option bit 0 表示不支持 search";
                return new InitialResult { Value = -1, ErrorInfo = strError };
            }

            bool bOption_1 = BerTree.GetBit(init_response.m_strOptions,
                1);
            if (bOption_1 == false)
            {
                strError = "服务器响应的 option bit 1 表示不支持 present";
                return new InitialResult { Value = -1, ErrorInfo = strError };
            }

            if (init_response.m_nResult != 0)
            {
                strError = "Initial OK";
            }
            else
            {
                strError = "Initial被拒绝。\r\n\r\n错误码 ["
                    + init_response.m_lErrorCode.ToString()
                    + "]\r\n错误消息["
                    + init_response.m_strErrorMessage + "]";

                strResultInfo = BuildInitialResultInfo(init_response);
                return new InitialResult
                {
                    Value = -1,
                    ErrorInfo = strError,
                    ResultInfo = strResultInfo
                };
            }

            /*
	this->m_init_strOption = init_response.m_strOptions;
	this->m_init_lPreferredMessageSize = init_response.m_lPreferredMessageSize;
	this->m_init_lExceptionalRecordSize = init_response.m_lExceptionalRecordSize;
	this->m_init_nResult = init_response.m_nResult;
             * */

            this._channel.Initialized = true;

            // 字符集协商
            if (init_response.m_charNego != null
                && BerTree.GetBit(init_response.m_strOptions, 17) == true)
            {
                if (init_response.m_charNego.EncodingLevelOID == CharsetNeogatiation.Utf8OID)
                {
                    // 临时修改检索词的编码方式。
                    // 但是还无法反映到PropertyDialog上。最好能反馈。
                    targetinfo.DefaultQueryTermEncoding = Encoding.UTF8;
                    targetinfo.Changed = true;

                    if (init_response.m_charNego.RecordsInSelectedCharsets == 1)
                        this.ForcedRecordsEncoding = Encoding.UTF8;
                }
            }

            strResultInfo = BuildInitialResultInfo(init_response);
            return new InitialResult
            {
                Value = 0,
                ErrorInfo = strError,
                ResultInfo = strResultInfo
            };
        }

        public static string BuildInitialResultInfo(INIT_RESPONSE info)
        {
            string strText = "";

            strText += "reference-id:\t" + info.m_strReferenceId + "\r\n";
            strText += "options:\t" + info.m_strOptions + "\r\n";
            strText += "preferred-message-size:\t" + info.m_lPreferredMessageSize.ToString() + "\r\n";
            strText += "exceptional-record-size:\t" + info.m_lExceptionalRecordSize.ToString() + "\r\n";

            strText += "\r\n--- Result Code ---\r\n";
            strText += "initial-result:\t" + info.m_nResult.ToString() + "\r\n";

            strText += "\r\n--- Implementation information ---\r\n";
            strText += "implementation-id:\t" + info.m_strImplementationId + "\r\n";
            strText += "implementation-name:\t" + info.m_strImplementationName + "\r\n";
            strText += "implementation-version:\t" + info.m_strImplementationVersion + "\r\n";

            strText += "\r\n--- Error Code and Message ---\r\n";
            strText += "error-code:\t" + info.m_lErrorCode.ToString() + "\r\n";
            strText += "error-messsage:\t" + info.m_strErrorMessage + "\r\n";

            if (info.m_charNego != null)
            {
                strText += "\r\n--- Charset Negotiation Parameters ---\r\n";

                if (BerTree.GetBit(info.m_strOptions, 17) == false)
                    strText += "options bit 17 is false, not allow negotiation\r\n";

                strText += "charnego-encoding-level-oid:\t" + info.m_charNego.EncodingLevelOID + "(note: UTF-8 is: " + CharsetNeogatiation.Utf8OID + ")\r\n";

                strText += "charnego-records-in-selected-charsets:\t" + info.m_charNego.RecordsInSelectedCharsets.ToString() + "\r\n";
            }

            return strText;
        }

        public class SearchResult : Result
        {
            public long ResultCount { get; set; }

            public SearchResult()
            {

            }

            public SearchResult(Result source)
            {
                Result.CopyTo(source, this);
            }

            public override string ToString()
            {
                StringBuilder text = new StringBuilder(base.ToString());
                text.Append("ResultCount=" + this.ResultCount + "\r\n");
                return text.ToString();
            }
        }

        // 检索
        // 本函数每次调用前，最好调用一次 TryInitialize()
        // parameters:
        //      strQuery    Search() 专用的检索式。注意，不是一个检索词那么简单
        //      dbnames     一般是从 targetInfo.DbNames 里面获得。或者从中选择一个数据库名用在这里
        //      strPreferredRecordSyntax 一般是从 targetInfo.PreferredRecordSyntax 获得即可
        // result.Value:
        //		-1	error
        //		0	fail
        //		1	succeed
        // result.ResultCount:
        //      命中结果集内记录条数 (当 result.Value 为 1 时)
        public async Task<SearchResult> Search(
            string strQuery,
            Encoding queryTermEncoding,
            string[] dbnames,
            string strPreferredRecordSyntax,
            string strResultSetName)
        {
            BerTree tree = new BerTree();
            SEARCH_REQUEST struSearch_request = new SEARCH_REQUEST
            {
                m_dbnames = dbnames
            };

            Debug.Assert(struSearch_request.m_dbnames.Length != 0, "");

            struSearch_request.m_strReferenceId = this._currentRefID;
            struSearch_request.m_lSmallSetUpperBound = 0;
            struSearch_request.m_lLargeSetLowerBound = 1;
            struSearch_request.m_lMediumSetPresentNumber = 0;
            struSearch_request.m_nReplaceIndicator = 1;
            struSearch_request.m_strResultSetName = strResultSetName;   // "default";
            struSearch_request.m_strSmallSetElementSetNames = "";
            struSearch_request.m_strMediumSetElementSetNames = "";
            struSearch_request.m_strPreferredRecordSyntax = strPreferredRecordSyntax; //  ZTargetControl.GetLeftValue(this.TargetInfo.PreferredRecordSyntax);   // BerTree.MARC_SYNTAX;
            struSearch_request.m_strQuery = strQuery;
            struSearch_request.m_nQuery_type = 1;
            struSearch_request.m_queryTermEncoding = queryTermEncoding;


            // m_search_response.m_lErrorCode = 0;
            byte[] baPackage = null;

            try
            {
                // 这里可能抛出异常
                tree.SearchRequest(struSearch_request,
                     out baPackage);
            }
            catch (Exception ex)
            {
                return new SearchResult { Value = -1, ErrorInfo = "CBERTree::SearchRequest() Exception: " + ex.Message };
            }

            if (this._channel.Connected == false)
            {
                this.CloseConnection();
                return new SearchResult { Value = -1, ErrorInfo = "socket尚未连接或者已经被关闭" };
            }

#if DUMPTOFILE
            string strBinFile = this.MainForm.DataDir + "\\searchrequest.bin";
            File.Delete(strBinFile);
            DumpPackage(strBinFile,
                baPackage);
            string strLogFile = this.MainForm.DataDir + "\\searchrequest.txt";
            File.Delete(strLogFile);
            tree.m_RootNode.DumpToFile(strLogFile);
#endif


#if NO
            nRet = CheckConnect(
                out strError);
            if (nRet == -1)
                return -1;
#endif

            BerTree tree1 = new BerTree();

            {
                RecvResult result = await this._channel.SendAndRecv(
        baPackage);
                if (result.Value == -1)
                    return new SearchResult(result);

#if NO
#if DEBUG
            if (nRet == 0)
            {
                Debug.Assert(strError == "", "");
            }
#endif
#endif

#if DUPMTOFILE
	DeleteFile("searchresponse.bin");
	DumpPackage("searchresponse.bin",
				(char *)baOutPackage.GetData(),
				baOutPackage.GetSize());
#endif

                tree1.m_RootNode.BuildPartTree(result.Package,
                    0,
                    result.Package.Length,
                    out int nTotalLen);
            }

            SEARCH_RESPONSE search_response = new SEARCH_RESPONSE();
            int nRet = BerTree.GetInfo_SearchResponse(tree1.GetAPDuRoot(),
                                   ref search_response,
                                   true,
                                   out string strError);
            if (nRet == -1)
                return new SearchResult { Value = -1, ErrorInfo = strError };

#if DUMPTOFILE
	DeleteFile("SearchResponse.txt"); 
	tree1.m_RootNode.DumpDebugInfoToFile("SearchResponse.txt");
#endif
            {
                SearchResult result = new SearchResult();
                result.ResultCount = (int)search_response.m_lResultCount;

                if (search_response.m_nSearchStatus != 0)   // 不一定是1
                {
                    result.Value = 1;
                    return result;
                }

                result.ErrorInfo = "Search Fail: diagRecords:\r\n" + search_response.m_diagRecords.GetMessage();
                result.Value = 0;    // search fail
                return result;
            }
        }

        // 将 XML 检索式变化为 Search() API 所用的检索式
        // 注： 这是一个辅助性方法，基本 Z39.50 功能可以不包含它。API 所用的检索式可以不必从 XML 检索式转换而来
        // parameters:
        //      strQueryXml XML 形态的检索式
        //      strQueryString [out] Search() 专用的检索式
        // result.Value
        //      -1  出错
        //      0   没有发生转换。例如 strQueryXml 为空的情况
        //      1   成功
        public static Result ConvertQueryString(
            UseCollection use_list,
            string strQueryXml,
            IsbnConvertInfo isbnconvertinfo,
            out string strQueryString)
        {
            strQueryString = "";

            if (String.IsNullOrEmpty(strQueryXml) == true)
                return new Result();

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strQueryXml);
            }
            catch (Exception ex)
            {
                return new Result { Value = -1, ErrorInfo = "strQueryXml装入XMLDOM时出错: " + ex.Message };
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("line");

            foreach (XmlElement node in nodes)
            {
                string strLogic = node.GetAttribute("logic");
                string strWord = node.GetAttribute("word");
                string strFrom = node.GetAttribute("from");

                if (string.IsNullOrEmpty(strWord) == true)
                    continue;   // 检索词为空的行会被跳过。

                strLogic = GetLogicString(strLogic);

                if (strQueryString != "")
                    strQueryString += " " + strLogic + " ";

                int nRet = strFrom.IndexOf("-");
                if (nRet != -1)
                    strFrom = strFrom.Substring(0, nRet).Trim();

                string strValue = use_list.GetValue(strFrom);
                if (strValue == null)
                    return new Result { Value = -1, ErrorInfo = "名称 '" + strFrom + "' 在use表中没有找到对应的编号" };

                // 对ISBN检索词进行预处理
                if (strFrom == "ISBN"
                    && isbnconvertinfo != null)
                {

                    // result.Value:
                    //      -1  出错
                    //      0   没有必要转换
                    //      1   已经转换
                    Result result = isbnconvertinfo.ConvertISBN(strWord,
                        out List<string> isbns);
                    if (result.Value == -1)
                        return new Result { Value = -1, ErrorInfo = "在处理ISBN字符串 '" + strWord + "' 过程中出错: " + result.ErrorInfo };

                    // 如果一个 ISBN 变成了多个 ISBN，要构造为 OR 方式的检索式。但遗憾的是可能有些 Z39.50 服务器并不支持 OR 运算检索
                    int j = 0;
                    foreach (string isbn in isbns)
                    {
                        if (j > 0)
                            strQueryString += " OR ";
                        // string strIsbn = isbn.Replace("\"", "\\\"");    // 字符 " 替换为 \"
                        string strIsbn = StringUtil.EscapeString(isbn, "\"/=");    // eacape 特殊字符
                        strQueryString += "\""
                            + strIsbn + "\"" + "/1="
                            + strValue;
                        j++;
                    }
                    continue;
                }

                // strWord = strWord.Replace("\"", "\\\""); // 字符 " 替换为 \"
                strWord = StringUtil.EscapeString(strWord, "\"/=");    // eacape 特殊字符
                strQueryString += "\""
                    + strWord + "\"" + "/1="
                    + strValue;
            }

            return new Result
            {
                Value = 1
            };
        }

        static string GetLogicString(string strText)
        {
            int nRet = strText.IndexOf(" ");
            if (nRet != -1)
                return strText.Substring(0, nRet).Trim();

            return strText.Trim();
        }

        public class PresentResult : Result
        {
            public RecordCollection Records { get; set; }

            public PresentResult()
            {

            }

            public PresentResult(Result source)
            {
                Result.CopyTo(source, this);
            }

            public override string ToString()
            {
                StringBuilder text = new StringBuilder(base.ToString());
                if (this.Records != null)
                    text.Append("Records=" + this.Records + "\r\n");
                return text.ToString();
            }
        }

        // 获得记录
        // 确保一定可以获得nCount个
        // parameters:
        //		nStart	获取记录的开始位置(从0开始计数)
        //      nPreferedEachCount  推荐的每次条数。这涉及到响应的敏捷性。如果为-1或者0，表示最大
        public async Task<PresentResult> Present(
            string strResultSetName,
            int nStart,
            int nCount,
            int nPreferedEachCount,  // 推荐的每次条数。这涉及到响应的敏捷性
            string strElementSetName,
            string strPreferredRecordSyntax)
        {
            if (nCount == 0)
                return new PresentResult { Value = 0, ErrorInfo = "nCount 参数为 0，本次没有真正请求服务器获取记录" };

            RecordCollection records = new RecordCollection();

            int nGeted = 0;
            for (; ; )
            {
                int nPerCount = 0;

                if (nPreferedEachCount == -1 || nPreferedEachCount == 0)
                    nPerCount = nCount - nGeted;
                else
                    nPerCount = Math.Min(nPreferedEachCount, nCount - nGeted);

                // this.Stop.SetMessage("正在获取命中结果 ( " + (nStart + nGeted + 1).ToString() + "-" + (nStart + nGeted + nPerCount).ToString() + " of " + this.ResultCount + " ) ...");

                // RecordCollection temprecords = null;
                PresentResult result = await OncePresent(
                    strResultSetName,
                    nStart + nGeted,
                    nPerCount,
                    strElementSetName,
                    strPreferredRecordSyntax);
                if (result.Value == -1)
                    return result;
                if (result.Records == null)
                    break;


                nGeted += result.Records.Count;
                if (result.Records.Count > 0)
                    records.AddRange(result.Records);

                if (nGeted >= nCount || result.Records.Count == 0)
                    break;
            }

            return new PresentResult { Records = records };
        }

        // 获得记录
        // 本函数每次调用前，最好调用一次 TryInitialize()
        // 不确保一定可以获得nCount个
        // parameters:
        //		nStart	获取记录的开始位置(从0开始计数)
        public async Task<PresentResult> OncePresent(
            string strResultSetName,
            int nStart,
            int nCount,
            string strElementSetName,
            string strPreferredRecordSyntax)
        {
            if (nCount == 0)
                return new PresentResult { Value = 0, ErrorInfo = "nCount 参数为 0，本次没有真正请求服务器获取记录" };

            BerTree tree = new BerTree();
            PRESENT_REQUEST struPresent_request = new PRESENT_REQUEST();
            //byte[] baPackage = null;
            //int nRet;

            struPresent_request.m_strReferenceId = this._currentRefID;
            struPresent_request.m_strResultSetName = strResultSetName; // "default";
            struPresent_request.m_lResultSetStartPoint = nStart + 1;
            struPresent_request.m_lNumberOfRecordsRequested = nCount;
            struPresent_request.m_strElementSetNames = strElementSetName;
            struPresent_request.m_strPreferredRecordSyntax = strPreferredRecordSyntax;

            int nRet = tree.PresentRequest(struPresent_request,
                                     out byte[] baPackage);
            if (nRet == -1)
                return new PresentResult { Value = -1, ErrorInfo = "CBERTree::PresentRequest() fail!" };
            if (this._channel.Connected == false)
            {
                this.CloseConnection();
                return new PresentResult { Value = -1, ErrorInfo = "socket尚未连接或者已经被关闭" };
            }

#if DUMPTOFILE
	DeleteFile("presentrequest.bin");
	DumpPackage("presentrequest.bin",
		(char *)baPackage.GetData(),
		baPackage.GetSize());
	DeleteFile ("presentrequest.txt");
	tree.m_RootNode.DumpToFile("presentrequest.txt");
#endif

            BerTree tree1 = new BerTree();

            {
                RecvResult result = await this._channel.SendAndRecv(
        baPackage);
                if (result.Value == -1)
                    return new PresentResult(result);

#if DUMPTOFILE
	DeleteFile("presendresponse.bin");
	DumpPackage("presentresponse.bin",
				(char *)baPackage.GetData(),
				baPackage.GetSize());
#endif


                tree1.m_RootNode.BuildPartTree(result.Package,
                    0,
                    result.Package.Length,
                    out int nTotalLen);
            }

#if DUMPTOFILE
	DeleteFile("PresentResponse.txt"); 
	tree1.m_RootNode.DumpDebugInfoToFile("PresentResponse.txt");
#endif
            SEARCH_RESPONSE search_response = new SEARCH_RESPONSE();
            nRet = BerTree.GetInfo_PresentResponse(tree1.GetAPDuRoot(),
                                   ref search_response,
                                   out RecordCollection records,
                                   true,
                                   out string strError);
            if (nRet == -1)
                return new PresentResult { Value = -1, ErrorInfo = strError };

            SetElementSetName(records, strElementSetName);

            if (search_response.m_diagRecords.Count != 0)
                return new PresentResult { Value = -1, ErrorInfo = "error diagRecords:\r\n\r\n---\r\n" + search_response.m_diagRecords.GetMessage() };

            return new PresentResult { Records = records };
        }

        // 修改集合中每个元素的 ElementSetName
        static void SetElementSetName(RecordCollection records,
    string strElementSetName)
        {
            if (records == null)
                return;

            foreach (DigitalPlatform.Z3950.Record record in records)
            {
                // 非诊断记录
                if (record.m_nDiagCondition == 0)
                {
                    record.m_strElementSetName = strElementSetName;
                }
            }
        }


        // 处理 Server 端可能发来的 Close
        // return value:
        //      -1  error
        //      0   不是Close
        //      1   是Close，已经迫使ZChannel处于尚未初始化状态
        // return InitialResult:
        //      在 InitialResult::ResultInfo 中返回诊断信息
        async Task<InitialResult> CheckServerCloseRequest()
        {
            if (this._channel.Connected == false || this._channel.DataAvailable == false)
                return new InitialResult(); // 没有发现问题

            // 注意调用返回后如果发现出错，调主要主动 Close 和重新分配 TcpClient
            RecvResult result = await ZChannel.SimpleRecvTcpPackage(this._channel._client);
            if (result.Value == -1)
            {
                this.CloseConnection();
                return new InitialResult { Value = -1, ErrorInfo = result.ErrorInfo };
            }

            BerTree tree1 = new BerTree();
            tree1.m_RootNode.BuildPartTree(result.Package,
                0,
                result.Package.Length,
                out int nTotalLen);

            if (tree1.GetAPDuRoot().m_uTag != BerTree.z3950_close)
            {
                // 不是Close
                return new InitialResult { Value = 0 };
            }

            CLOSE_REQUEST closeStruct = new CLOSE_REQUEST();
            int nRet = BerTree.GetInfo_closeRequest(
                tree1.GetAPDuRoot(),
                ref closeStruct,
                out string strError);
            if (nRet == -1)
            {
                this.CloseConnection();
                return new InitialResult { Value = -1, ErrorInfo = strError };
            }

            this.CloseConnection();
            return new InitialResult { Value = 1, ResultInfo = closeStruct.m_strDiagnosticInformation };
        }

        // 切断连接
        public void CloseConnection()
        {
            this._channel.CloseSocket();
            Debug.Assert(this._channel.Initialized == false, "");  // 迫使重新初始化

            // 触发事件，让外面知晓这里发生了 Close()。这样便于外面自动跟随清除 TargetInfo
            var temp = this.Closed;
            if (temp != null)
                temp(this, new EventArgs());
        }

    }
}
