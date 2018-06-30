using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using static DigitalPlatform.Z3950.ZChannel;
using static DigitalPlatform.Z3950.ZClient;

namespace DigitalPlatform.Z3950.Server
{
    public static class ZProcessor
    {
        // 注意调用返回后如果发现返回 null 或者抛出了异常，调主要主动 Close 和重新分配 TcpClient
        public static async Task<BerTree> GetIncomingRequest(TcpClient client)
        {
            RecvResult result = await ZChannel.SimpleRecvTcpPackage(client);
            if (result.Value == -1)
            {
                if (result.ErrorCode == "Closed")
                    return null;    // 表示通道被对方切断
                throw new Exception(result.ErrorInfo);
            }

#if DEBUG
            if (result.Package != null)
            {
                Debug.Assert(result.Package.Length == result.Length, "");
            }
            else
            {
                Debug.Assert(result.Length == 0, "");
            }
#endif

            // 分析请求包
            BerTree tree1 = new BerTree();
            tree1.m_RootNode.BuildPartTree(result.Package,
                0,
                result.Package.Length,
                out int nTotallen);

            return tree1;
        }

        // 注意调用返回 result.Value == -1 情况下，要及时 Close TcpClient
        public static async Task<Result> SendResponse(byte[] response, TcpClient client)
        {
            Result result = await SimpleSendTcpPackage(client,
                response,
                response.Length);
            if (result.Value == -1 || result.Value == 1)
                return new Result { Value = -1, ErrorInfo = result.ErrorInfo };

            return new Result();
        }


#if NO
        public static async Task<HttpRequest> GetIncomingRequest(Stream inputStream,
    List<byte> cache,
    Delegate_verifyHeaders proc_verify,
    CancellationToken token)
        {
            // Read Request Line
            string request = await ReadLineAsync(inputStream, cache, token);
            if (string.IsNullOrEmpty(request))
                return null;    // 表示前端已经切断通讯
#if NO
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
#endif
            List<string> tokens = MessageUtility.SplitFirstLine(request);
            if (tokens == null)
                throw new Exception("1 invalid http request line '" + request + "'");

            string method = tokens[0].ToUpper();
            string url = tokens[1];
            string protocolVersion = tokens[2];

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string line;
            while ((line = await ReadLineAsync(inputStream, cache, token)) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("1 invalid http header line: " + line);
                }
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                headers.Add(name, value);
                if (headers.Count > MAX_HEADER_LINES)
                    throw new Exception("headers 行数超过配额");
            }

            if (proc_verify != null)
            {
                if (proc_verify(headers) == false)
                    return null;
            }

            byte[] raw_content = null;
            if (headers.ContainsKey("Content-Length"))
            {
                int totalBytes = Convert.ToInt32(headers["Content-Length"]);

                if (totalBytes >= MAX_ENTITY_BYTES)
                    throw new Exception("Content-Length " + totalBytes + " 超过配额");

                int bytesLeft = totalBytes;
                byte[] bytes = new byte[totalBytes];

                while (bytesLeft > 0)
                {
                    byte[] buffer = new byte[bytesLeft > 1024 ? 1024 : bytesLeft];

                    int nRet = 0;
                    if (cache.Count > 0)
                    {
                        nRet = Math.Min(buffer.Length, cache.Count);
                        Array.Copy(cache.ToArray(), buffer, nRet);
                        cache.RemoveRange(0, nRet);
                    }

                    int n = 0;
                    if (nRet < buffer.Length)
                    {
                        // int n = inputStream.Read(buffer, 0, buffer.Length);
                        n = await inputStream.ReadAsync(buffer, nRet, buffer.Length - nRet, token);
                    }

                    buffer.CopyTo(bytes, totalBytes - bytesLeft);

                    bytesLeft -= nRet + n;
                }

                raw_content = bytes;
            }

            return new HttpRequest()
            {
                Method = method,
                Url = url,
                Version = protocolVersion,
                Headers = headers,
                Content = raw_content
            };
        }

#endif

        // 解码Initial请求包
        public static int Decode_InitRequest(
            BerNode root,
            out InitRequestInfo info,
            out string strDebugInfo,
            out string strError)
        {
            strError = "";
            strDebugInfo = "";
            info = new InitRequestInfo();

            Debug.Assert(root != null, "");

            if (root.m_uTag != BerTree.z3950_initRequest)
            {
                strError = "root tag is not z3950_initRequest";
                return -1;
            }

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                switch (node.m_uTag)
                {
                    case BerTree.z3950_ReferenceId:
                        info.m_strReferenceId = node.GetCharNodeData();
                        strDebugInfo += "ReferenceID='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_ProtocolVersion:
                        info.m_strProtocolVersion = node.GetBitstringNodeData();
                        strDebugInfo += "ProtocolVersion='" + node.GetBitstringNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_Options:
                        info.m_strOptions = node.GetBitstringNodeData();
                        strDebugInfo += "Options='" + node.GetBitstringNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_PreferredMessageSize:
                        info.m_lPreferredMessageSize = node.GetIntegerNodeData();
                        strDebugInfo += "PreferredMessageSize='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_ExceptionalRecordSize:
                        info.m_lExceptionalRecordSize = node.GetIntegerNodeData();
                        strDebugInfo += "ExceptionalRecordSize='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_idAuthentication:
                        {
                            string strGroupId = "";
                            string strUserId = "";
                            string strPassword = "";
                            int nAuthentType = 0;

                            int nRet = DecodeAuthentication(
                                node,
                                out strGroupId,
                                out strUserId,
                                out strPassword,
                                out nAuthentType,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            info.m_nAuthenticationMethod = nAuthentType;	// 0: open 1:idPass
                            info.m_strGroupID = strGroupId;
                            info.m_strID = strUserId;
                            info.m_strPassword = strPassword;

                            strDebugInfo += "idAuthentication struct occur\r\n";
                        }
                        break;
                    case BerTree.z3950_ImplementationId:
                        info.m_strImplementationId = node.GetCharNodeData();
                        strDebugInfo += "ImplementationId='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_ImplementationName:
                        info.m_strImplementationName = node.GetCharNodeData();
                        strDebugInfo += "ImplementationName='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_ImplementationVersion:
                        info.m_strImplementationVersion = node.GetCharNodeData();
                        strDebugInfo += "ImplementationVersion='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_OtherInformationField:
                        info.m_charNego = new CharsetNeogatiation();
                        info.m_charNego.DecodeProposal(node);
                        break;
                    default:
                        strDebugInfo += "Undefined tag = [" + node.m_uTag.ToString() + "]\r\n";
                        break;
                }
            }

            return 0;
        }

        // 解析出init请求中的 鉴别信息
        // parameters:
        //      nAuthentType 0: open(simple) 1:idPass(group)
        static int DecodeAuthentication(
            BerNode root,
            out string strGroupId,
            out string strUserId,
            out string strPassword,
            out int nAuthentType,
            out string strError)
        {
            strGroupId = "";
            strUserId = "";
            strPassword = "";
            nAuthentType = 0;
            strError = "";

            if (root == null)
            {
                strError = "root == null";
                return -1;
            }

            string strOpen = ""; // open mode authentication


            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                switch (node.m_uTag)
                {
                    case BerNode.ASN1_SEQUENCE:

                        nAuthentType = 1;   //  "GROUP";
                        for (int k = 0; k < node.ChildrenCollection.Count; k++)
                        {
                            BerNode nodek = node.ChildrenCollection[k];
                            switch (nodek.m_uTag)
                            {
                                case 0: // groupId
                                    strGroupId = nodek.GetCharNodeData();
                                    break;
                                case 1: // userId
                                    strUserId = nodek.GetCharNodeData();
                                    break;
                                case 2: // password
                                    strPassword = nodek.GetCharNodeData();
                                    break;
                            }
                        }

                        break;
                    case BerNode.ASN1_VISIBLESTRING:
                    case BerNode.ASN1_GENERALSTRING:
                        nAuthentType = 0; //  "SIMPLE";
                        strOpen = node.GetCharNodeData();
                        break;
                }
            }

            if (nAuthentType == 0)
            {
                int nRet = strOpen.IndexOf("/");
                if (nRet != -1)
                {
                    strUserId = strOpen.Substring(0, nRet);
                    strPassword = strOpen.Substring(nRet + 1);
                }
                else
                {
                    strUserId = strOpen;
                }
            }

            return 0;
        }

        public static void SetInitResponseUserInfo(InitResponseInfo response_info,
    string strOID,
    long lErrorCode,
    string strErrorMessage)
        {
            if (response_info.UserInfoField == null)
                response_info.UserInfoField = new External();

            response_info.UserInfoField.m_strDirectRefenerce = strOID;
            if (lErrorCode != 0)
            {
                response_info.UserInfoField.m_lIndirectReference = lErrorCode;
                response_info.UserInfoField.m_bHasIndirectReference = true; // 2018/6/29
            }
            if (String.IsNullOrEmpty(strErrorMessage) == false)
            {
                response_info.UserInfoField.m_octectAligned = Encoding.UTF8.GetBytes(strErrorMessage);
            }
        }

        //	 build a z39.50 Init response
        public static void Encode_InitialResponse(InitResponseInfo info,
            out byte[] baPackage)
        {
            baPackage = null;

            BerNode root = null;

            BerTree tree = new BerTree();

            root = tree.m_RootNode.NewChildConstructedNode(BerTree.z3950_initResponse,
                BerNode.ASN1_CONTEXT);

            if (String.IsNullOrEmpty(info.m_strReferenceId) == false)
            {
                root.NewChildCharNode(BerTree.z3950_ReferenceId,
                    BerNode.ASN1_CONTEXT,
                    Encoding.UTF8.GetBytes(info.m_strReferenceId));
            }

            root.NewChildBitstringNode(BerTree.z3950_ProtocolVersion,   // 3
                BerNode.ASN1_CONTEXT,
                "yy");

            /* option
         search                 (0), 
         present                (1), 
         delSet                 (2),
         resourceReport         (3),
         triggerResourceCtrl    (4),
         resourceCtrl           (5), 
         accessCtrl             (6),
         scan                   (7),
         sort                   (8), 
         --                     (9) (reserved)
         extendedServices       (10),
         level-1Segmentation    (11),
         level-2Segmentation    (12),
         concurrentOperations   (13),
         namedResultSets        (14)
            15 Encapsulation  Z39.50-1995 Amendment 3: Z39.50 Encapsulation 
            16 resultCount parameter in Sort Response  See Note 8 Z39.50-1995 Amendment 1: Add resultCount parameter to Sort Response  
            17 Negotiation Model  See Note 9 Model for Z39.50 Negotiation During Initialization  
            18 Duplicate Detection See Note 1  Z39.50 Duplicate Detection Service  
            19 Query type 104 
*/
            root.NewChildBitstringNode(BerTree.z3950_Options,   // 4
                BerNode.ASN1_CONTEXT,
                info.m_strOptions);    // "110000000000001"

            root.NewChildIntegerNode(BerTree.z3950_PreferredMessageSize,    // 5
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes((long)info.m_lPreferredMessageSize));

            root.NewChildIntegerNode(BerTree.z3950_ExceptionalRecordSize,   // 6
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes((long)info.m_lExceptionalRecordSize));

            // 2007/11/7 原来这个事项曾经位置不对，现在调整到这里
            // bool
            root.NewChildIntegerNode(BerTree.z3950_result,  // 12
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes((long)info.m_nResult));


            root.NewChildCharNode(BerTree.z3950_ImplementationId,   // 110
                BerNode.ASN1_CONTEXT,
                Encoding.UTF8.GetBytes(info.m_strImplementationId));

            root.NewChildCharNode(BerTree.z3950_ImplementationName, // 111
                BerNode.ASN1_CONTEXT,
                Encoding.UTF8.GetBytes(info.m_strImplementationName));

            root.NewChildCharNode(BerTree.z3950_ImplementationVersion,  // 112
                BerNode.ASN1_CONTEXT,
                Encoding.UTF8.GetBytes(info.m_strImplementationVersion));  // "3"


            // userInformationField
            if (info.UserInfoField != null)
            {
                BerNode nodeUserInfoRoot = root.NewChildConstructedNode(BerTree.z3950_UserInformationField,    // 11
                    BerNode.ASN1_CONTEXT);
                info.UserInfoField.Build(nodeUserInfoRoot);
            }

            if (info.m_charNego != null)
            {
                info.m_charNego.EncodeResponse(root);
            }

            baPackage = null;
            root.EncodeBERPackage(ref baPackage);
        }

        // 解码Search请求包
        public static int Decode_SearchRequest(
            BerNode root,
            out SearchRequestInfo info,
            out string strError)
        {
            strError = "";

            int nRet = 0;

            info = new SearchRequestInfo();

            Debug.Assert(root != null, "");

            if (root.m_uTag != BerTree.z3950_searchRequest)
            {
                strError = "root tag is not z3950_searchRequest";
                return -1;
            }

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];

                switch (node.m_uTag)
                {
                    case BerTree.z3950_ReferenceId: // 2
                        info.m_strReferenceId = node.GetCharNodeData();
                        break;
                    case BerTree.z3950_smallSetUpperBound: // 13 smallSetUpperBound (Integer)
                        info.m_lSmallSetUpperBound = node.GetIntegerNodeData();
                        break;
                    case BerTree.z3950_largeSetLowerBound: // 14 largeSetLowerBound  (Integer)         
                        info.m_lLargeSetLowerBound = node.GetIntegerNodeData();
                        break;
                    case BerTree.z3950_mediumSetPresentNumber: // 15 mediumSetPresentNumber (Integer)      
                        info.m_lMediumSetPresentNumber = node.GetIntegerNodeData();
                        break;
                    case BerTree.z3950_replaceIndicator: // 16 replaceIndicator, (boolean)
                        info.m_lReplaceIndicator = node.GetIntegerNodeData();
                        break;
                    case BerTree.z3950_resultSetName: // 17 resultSetName (string)
                        info.m_strResultSetName = node.GetCharNodeData();
                        break;
                    case BerTree.z3950_databaseNames: // 18 dbNames (sequence)
                        /*
                        // sequence is constructed, // have child with case = 105, (string)
                        m_saDBName.RemoveAll();
                        DecodeDBName(pNode, m_saDBName, m_bIsCharSetUTF8);
                         * */
                        {
                            List<string> dbnames = null;
                            nRet = DecodeDbnames(node,
                                out dbnames,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            info.m_dbnames = dbnames;
                        }
                        break;
                    case BerTree.z3950_query: // 21 query (query)
                        //			DecodeSearchQuery(pNode, m_strSQLWhere, pRPNStructureRoot);
                        {
                            BerNode rpn_root = GetRPNStructureRoot(node,
                                out strError);
                            if (rpn_root == null)
                                return -1;

                            info.m_rpnRoot = rpn_root;
                        }
                        break;
                    default:
                        break;
                }

            }

            return 0;
        }

        // 获得search请求中的RPN根节点
        static BerNode GetRPNStructureRoot(BerNode root,
            out string strError)
        {
            strError = "";

            if (root == null)
            {
                strError = "query root is null";
                return null;
            }

            if (root.ChildrenCollection.Count < 1)
            {
                strError = "no query item";
                return null;
            }

            BerNode RPNRoot = root.ChildrenCollection[0];
            if (1 != RPNRoot.m_uTag) // type-1 query
            {
                strError = "not type-1 query. unsupported query type";
                return null;
            }

            string strAttributeSetId = ""; //attributeSetId OBJECT IDENTIFIER
            // string strQuery = "";


            for (int i = 0; i < RPNRoot.ChildrenCollection.Count; i++)
            {
                BerNode node = RPNRoot.ChildrenCollection[i];
                switch (node.m_uTag)
                {
                    case 6: // attributeSetId (OBJECT IDENTIFIER)
                        strAttributeSetId = node.GetOIDsNodeData();
                        if (strAttributeSetId != "1.2.840.10003.3.1") // bib-1
                        {
                            strError = "support bib-1 only";
                            return null;
                        }
                        break;
                    // RPNStructure (CHOICE 0, 1)
                    case 0:
                    case 1:
                        return node; // this is RPN Stucture root
                }
            }

            strError = "not found";
            return null;
        }

        // 解析出search请求中的 数据库名列表
        static int DecodeDbnames(BerNode root,
            out List<string> dbnames,
            out string strError)
        {
            dbnames = new List<string>();
            strError = "";

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                if (node.m_uTag == 105)
                {
                    dbnames.Add(node.GetCharNodeData());
                }
            }

            return 0;
        }


        // 编码(构造) Search响应包
        public static int Encode_SearchResponse(SearchRequestInfo info,
            SearchResult result,
            DiagFormat diag,
            out byte[] baPackage,
            out string strError)
        {
            baPackage = null;
            // int nRet = 0;
            // long lRet = 0;
            strError = "";

            BerTree tree = new BerTree();
            BerNode root = null;

            long lSearchStatus = 0; // 0 失败；1成功
            // long lHitCount = 0;

#if NO
            string strQueryXml = "";
            // 根据逆波兰表进行检索

            // return:
            //      -1  error
            //      0   succeed
            nRet = BuildQueryXml(
                info.m_dbnames,
                info.m_rpnRoot,
                out strQueryXml,
                out strError);
            if (nRet == -1)
            {
                SetPresentDiagRecord(ref diag,
                    2,  // temporary system error
                    strError);
            }
#endif

            string strResultSetName = info.m_strResultSetName;
            if (String.IsNullOrEmpty(strResultSetName) == true)
                strResultSetName = "default";

            if (diag == null)
            {
#if NO
                lRet = _channel.Search(null,
                    strQueryXml,
                    strResultSetName,
                    "", // strOutputStyle
                    out strError);

                /*
                // 测试检索失败
                lRet = -1;
                strError = "测试检索失败";
                 * */

                if (lRet == -1)
                {
                    lSearchStatus = 0;  // failed

                    SetPresentDiagRecord(ref diag,
                        2,  // temporary system error
                        strError);
                }
                else
                {
                    lHitCount = lRet;
                    lSearchStatus = 1;  // succeed
                }
#endif


            }

            if (result.Value == -1)
            {
                lSearchStatus = 0;  // failed

                // TODO: nCondition 查一下 Z39.50 协议文本，看看是否还有更贴切的错误码可用
                SetPresentDiagRecord(ref diag,
                    2,  // temporary system error
                    result.ErrorInfo);
            }
            else
            {
                // lHitCount = result.ResultCount;
                lSearchStatus = 1;  // succeed
            }

            root = tree.m_RootNode.NewChildConstructedNode(
                BerTree.z3950_searchResponse,
                BerNode.ASN1_CONTEXT);

            // reference id
            if (String.IsNullOrEmpty(info.m_strReferenceId) == false)
            {
                root.NewChildCharNode(BerTree.z3950_ReferenceId,
                    BerNode.ASN1_CONTEXT,
                    Encoding.UTF8.GetBytes(info.m_strReferenceId));
            }


            // resultCount
            root.NewChildIntegerNode(BerTree.z3950_resultCount, // 23
                BerNode.ASN1_CONTEXT,   // ASNI_PRIMITIVE BUG!!!!
                BitConverter.GetBytes((long)result.ResultCount));

            // numberOfRecordsReturned
            root.NewChildIntegerNode(BerTree.z3950_NumberOfRecordsReturned, // 24
                BerNode.ASN1_CONTEXT,   // ASNI_PRIMITIVE BUG!!!!
                BitConverter.GetBytes((long)0/*info.m_lNumberOfRecordReturned*/));    // 0

            // nextResultSetPosition
            root.NewChildIntegerNode(BerTree.z3950_NextResultSetPosition, // 25
                BerNode.ASN1_CONTEXT,   // ASNI_PRIMITIVE BUG!!!!
                BitConverter.GetBytes((long)1/*info.m_lNextResultSetPosition*/));

            // 2007/11/7 原来本项位置不对，现在移动到这里
            // bool
            // searchStatus
            root.NewChildIntegerNode(BerTree.z3950_searchStatus, // 22
                BerNode.ASN1_CONTEXT,   // ASNI_PRIMITIVE BUG!!!!
                BitConverter.GetBytes((long)lSearchStatus));

            // resultSetStatus OPTIONAL

            // 2007/11/7
            // presentStatus
            root.NewChildIntegerNode(BerTree.z3950_presentStatus, // 27
                BerNode.ASN1_CONTEXT,   // ASNI_PRIMITIVE BUG!!!!
                BitConverter.GetBytes((long)0));


            // 诊断记录
            if (diag != null)
            {
                BerNode nodeDiagRoot = root.NewChildConstructedNode(BerTree.z3950_nonSurrogateDiagnostic,    // 130
                    BerNode.ASN1_CONTEXT);

                diag.BuildBer(nodeDiagRoot);
                // TODO: Z39.50 前端要能显示 nodeDiagRoot
            }

            baPackage = null;
            root.EncodeBERPackage(ref baPackage);

            return 0;
        }

        // 设置present response中的诊断记录
        public static void SetPresentDiagRecord(ref DiagFormat diag,
            int nCondition,
            string strAddInfo)
        {
            if (diag == null)
            {
                diag = new DiagFormat();
                diag.m_strDiagSetID = "1.2.840.10003.4.1";
            }

            diag.m_nDiagCondition = nCondition;
            diag.m_strAddInfo = strAddInfo;
        }

        // 解码Present请求包
        public static int Decode_PresentRequest(
            BerNode root,
            out PresentRequestInfo info,
            out string strError)
        {
            strError = "";

            int nRet = 0;

            info = new PresentRequestInfo();

            Debug.Assert(root != null, "");

            if (root.m_uTag != BerTree.z3950_presentRequest)
            {
                strError = "root tag is not z3950_presentRequest";
                return -1;
            }

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];

                switch (node.m_uTag)
                {
                    case BerTree.z3950_ReferenceId: // 2
                        info.m_strReferenceId = node.GetCharNodeData();
                        break;
                    case BerTree.z3950_ResultSetId: // 31 resultSetId (IntenationalString)
                        info.m_strResultSetID = node.GetCharNodeData();
                        break;
                    case BerTree.z3950_resultSetStartPoint: // 30 resultSetStartPoint  (Integer)         
                        info.m_lResultSetStartPoint = node.GetIntegerNodeData();
                        break;
                    case BerTree.z3950_numberOfRecordsRequested: // 29 numberOfRecordsRequested (Integer)      
                        info.m_lNumberOfRecordsRequested = node.GetIntegerNodeData();
                        break;
                    case BerTree.z3950_ElementSetNames: // 19 ElementSetNames (complicates)
                        {
                            List<string> elementset_names = null;
                            nRet = DecodeElementSetNames(node,
                                out elementset_names,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            info.m_elementSetNames = elementset_names;
                        }
                        break;
                    default:
                        break;
                }
            }

            return 0;
        }

        // 解析出search请求中的 数据库名列表
        static int DecodeElementSetNames(BerNode root,
            out List<string> elementset_names,
            out string strError)
        {
            elementset_names = new List<string>();
            strError = "";

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                /*
                if (node.m_uTag == 105)
                {
                    dbnames.Add(node.GetCharNodeData());
                }
                 * */
                // TODO: 这里需要看一下PDU定义，看看是否需要判断m_uTag
                elementset_names.Add(node.GetCharNodeData());
            }

            return 0;
        }


        // 编码(构造) Present响应包
        public static int Encode_PresentResponse(PresentRequestInfo info,
            List<RetrivalRecord> records,
            DiagFormat diag,
            long lHitCount,
            out byte[] baPackage)
        {
            baPackage = null;
            //int nRet = 0;
            //string strError = "";

            // DiagFormat diag = null;

            BerTree tree = new BerTree();
            BerNode root = null;

            string strResultSetName = info.m_strResultSetID;
            if (String.IsNullOrEmpty(strResultSetName) == true)
                strResultSetName = "default";
            long lStart = info.m_lResultSetStartPoint - 1;
            // long lNumber = info.m_lNumberOfRecordsRequested;

            // long lPerCount = lNumber;

            // long lHitCount = 0;

            // List<string> paths = new List<string>();

            int nPresentStatus = 5; // failed

#if NO
            // 获取结果集中需要部分的记录path
            long lOffset = lStart;
            int nCount = 0;
            for (; ; )
            {
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                long lRet = this._channel.GetSearchResult(
                    null,   // stop,
                    strResultSetName,   // strResultSetName
                    lOffset,
                    lPerCount,
                    "id",
                    "zh",   // this.Lang,
                    out searchresults,
                    out strError);
                /*
                // 测试获取结果集失败的情况，返回非代理诊断记录
                lRet = -1;
                strError = "测试检索错误信息！";
                 * */

                if (lRet == -1)
                {
                    SetPresentDiagRecord(ref diag,
                        2,  // temporary system error
                        strError);
                    break;
                }
                if (lRet == 0)
                {
                    // goto ERROR1 ?
                }

                lHitCount = lRet;   // 顺便得到命中记录总条数

                // 转储
                for (int i = 0; i < searchresults.Length; i++)
                {
                    paths.Add(searchresults[i].Path);
                }

                lOffset += searchresults.Length;
                lPerCount -= searchresults.Length;
                nCount += searchresults.Length;

                if (lOffset >= lHitCount
                    || lPerCount <= 0
                    || nCount >= lNumber)
                {
                    // 
                    break;
                }
            }

            // TODO: 需要注意多个错误是否形成多个diag记录？V2不允许这样，V3允许这样
            if (lHitCount < info.m_lResultSetStartPoint
                && diag == null)
            {
                strError = "start参数值 "
                    + info.m_lResultSetStartPoint
                    + " 超过结果集中记录总数 "
                    + lHitCount;
                // return -1;  // 如果表示错误状态？
                SetPresentDiagRecord(ref diag,
                    13,  // Present request out-of-range
                    strError);
            }

#endif

#if NO
            int MAX_PRESENT_RECORD = 100;

            // 限制每次 present 的记录数量
            if (lNumber > MAX_PRESENT_RECORD)
                lNumber = MAX_PRESENT_RECORD;
#endif
            long lNumber = records == null ? 0 : records.Count;

            long nNextResultSetPosition = 0;

            // 
            if (lHitCount < (lStart - 1) + lNumber)
            {
                // 是 present 错误，但还可以调整 lNumber
                lNumber = lHitCount - (lStart - 1);
                nNextResultSetPosition = 0;
            }
            else
            {
                //
                nNextResultSetPosition = lStart + lNumber + 1;
            }

            root = tree.m_RootNode.NewChildConstructedNode(
                BerTree.z3950_presentResponse,
                BerNode.ASN1_CONTEXT);

            // reference id
            if (String.IsNullOrEmpty(info.m_strReferenceId) == false)
            {
                root.NewChildCharNode(BerTree.z3950_ReferenceId,
                    BerNode.ASN1_CONTEXT,
                    Encoding.UTF8.GetBytes(info.m_strReferenceId));
            }

#if NO
            List<RetrivalRecord> records = new List<RetrivalRecord>();

            // 获取要返回的MARC记录
            if (diag == null)
            {

                // 记录编码格式为 GRS-1 (generic-record-syntax-1) :
                //		EXTERNAL 
                //			--- OID (Object Identifier)
                //			--- MARC (OCTET STRING)
                //	m_strOID = _T("1.2.840.10003.5.1");  // OID of UNIMARC
                //	m_strOID = _T("1.2.840.10003.5.10"); // OID of USMARC //
                // 需要建立一个数据库名和oid的对照表，方面快速取得数据库MARC syntax OID

                // TODO: 编码过程中，可能会发现记录太多，总尺寸超过Initial中规定的prefered message size。
                // 这样需要减少返回的记录数量。这样，就需要先做这里的循环，后构造另外几个参数
                int nSize = 0;
                for (int i = 0; i < (int)lNumber; i++)
                {
                    // 编码 N 条 MARC 记录
                    //
                    // if (m_bStop) return false;

                    // 取出数据库指针
                    // lStart 不是 0 起点的
                    string strPath = paths[i];

                    // 解析出数据库名和ID
                    string strDbName = Global.GetDbName(strPath);
                    string strRecID = Global.GetRecordID(strPath);

                    // 如果取得的是xml记录，则根元素可以看出记录的marc syntax，进一步可以获得oid；
                    // 如果取得的是MARC格式记录，则需要根据数据库预定义的marc syntax来看出oid了
                    string strMarcSyntaxOID = GetMarcSyntaxOID(strDbName);

                    byte[] baMARC = null;

                    RetrivalRecord record = new RetrivalRecord();
                    record.m_strDatabaseName = strDbName;

                    // 根据书目库名获得书目库属性对象
                    BiblioDbProperty prop = this._service.GetDbProperty(
                        strDbName,
                        false);

                    nRet = GetMARC(strPath,
                        info.m_elementSetNames,
                        prop != null ? prop.AddField901 : false,
                        prop != null ? prop.RemoveFields : "997",
                        out baMARC,
                        out strError);

                    /*
                    // 测试记录群中包含诊断记录
                    if (i == 1)
                    {
                        nRet = -1;
                        strError = "测试获取记录错误";
                    }*/
                    if (nRet == -1)
                    {
                        record.m_surrogateDiagnostic = new DiagFormat();
                        record.m_surrogateDiagnostic.m_strDiagSetID = "1.2.840.10003.4.1";
                        record.m_surrogateDiagnostic.m_nDiagCondition = 14;  // system error in presenting records
                        record.m_surrogateDiagnostic.m_strAddInfo = strError;
                    }
                    else if (nRet == 0)
                    {
                        record.m_surrogateDiagnostic = new DiagFormat();
                        record.m_surrogateDiagnostic.m_strDiagSetID = "1.2.840.10003.4.1";
                        record.m_surrogateDiagnostic.m_nDiagCondition = 1028;  // record deleted
                        record.m_surrogateDiagnostic.m_strAddInfo = strError;
                    }
                    else if (String.IsNullOrEmpty(strMarcSyntaxOID) == true)
                    {
                        // 根据数据库名无法获得marc syntax oid。可能是虚拟库检索命中记录所在的物理库没有在dp2zserver.xml中配置。
                        record.m_surrogateDiagnostic = new DiagFormat();
                        record.m_surrogateDiagnostic.m_strDiagSetID = "1.2.840.10003.4.1";
                        record.m_surrogateDiagnostic.m_nDiagCondition = 109;  // database unavailable // 似乎235:database dos not exist也可以
                        record.m_surrogateDiagnostic.m_strAddInfo = "根据数据库名 '" + strDbName + "' 无法获得marc syntax oid";
                    }
                    else
                    {
                        record.m_external = new External();
                        record.m_external.m_strDirectRefenerce = strMarcSyntaxOID;
                        record.m_external.m_octectAligned = baMARC;
                    }

                    nSize += record.GetPackageSize();

                    if (i == 0)
                    {
                        // 连一条记录也放不下
                        if (nSize > this._lExceptionalRecordSize)
                        {
                            Debug.Assert(diag == null, "");
                            SetPresentDiagRecord(ref diag,
                                17, // record exceeds Exceptional_record_size
                                "记录尺寸 " + nSize.ToString() + " 超过 Exceptional_record_size " + this._lExceptionalRecordSize.ToString());
                            lNumber = 0;
                            break;
                        }
                    }
                    else
                    {
                        if (nSize >= this._lPreferredMessageSize)
                        {
                            // 调整返回的记录数
                            lNumber = i;
                            break;
                        }
                    }

                    records.Add(record);
                }
            }
#endif

            // numberOfRecordsReturned
            root.NewChildIntegerNode(BerTree.z3950_NumberOfRecordsReturned, // 24
                BerNode.ASN1_CONTEXT,   // ASN1_PRIMITIVE BUG!!!
                BitConverter.GetBytes((long)lNumber));

            if (diag != null)
                nPresentStatus = 5;
            else
                nPresentStatus = 0;

            // nextResultSetPosition
            // if 0, that's end of the result set
            // else M+1, M is 最后一次 present response 的最后一条记录在 result set 中的 position
            root.NewChildIntegerNode(BerTree.z3950_NextResultSetPosition, // 25
                BerNode.ASN1_CONTEXT,   // ASN1_PRIMITIVE BUG!!!
                BitConverter.GetBytes((long)nNextResultSetPosition));

            // presentStatus
            // success      (0),
            // partial-1    (1),
            // partial-2    (2),
            // partial-3    (3),
            // partial-4    (4),
            // failure      (5).
            root.NewChildIntegerNode(BerTree.z3950_presentStatus, // 27
                BerNode.ASN1_CONTEXT,   // ASN1_PRIMITIVE BUG!!!
               BitConverter.GetBytes((long)nPresentStatus));

            // 诊断记录
            if (diag != null)
            {
                BerNode nodeDiagRoot = root.NewChildConstructedNode(BerTree.z3950_nonSurrogateDiagnostic,    // 130
                    BerNode.ASN1_CONTEXT);

                diag.BuildBer(nodeDiagRoot);

                /*
                nodeDiagRoot.NewChildOIDsNode(6,
                    BerNode.ASN1_UNIVERSAL,
                    diag.m_strDiagSetID);   // "1.2.840.10003.4.1"

                nodeDiagRoot.NewChildIntegerNode(2,
                    BerNode.ASN1_UNIVERSAL,
                    BitConverter.GetBytes((long)diag.m_nDiagCondition));

                if (String.IsNullOrEmpty(diag.m_strAddInfo) == false)
                {
                    nodeDiagRoot.NewChildCharNode(26,
                        BerNode.ASN1_UNIVERSAL,
                        Encoding.UTF8.GetBytes(diag.m_strAddInfo));
                }
                 * */
            }


            // 如果 present 是非法的，到这里打包完成，可以返回了
            if (0 != nPresentStatus)
                goto END1;

            // 编码记录BER树

            // 以下为 present 成功时，打包返回记录。
            // present success
            // presRoot records child, constructed (choice of ... ... optional)
            // if present fail, then may be no records 'node'
            // Records ::= CHOICE {
            //		responseRecords              [28]   IMPLICIT SEQUENCE OF NamePlusRecord,
            //		nonSurrogateDiagnostic       [130]  IMPLICIT DefaultDiagFormat,
            //		multipleNonSurDiagnostics    [205]  IMPLICIT SEQUENCE OF DiagRec} 

            // 当 present 成功时，response 选择了 NamePlusRecord (数据库名 +　记录)
            BerNode node = root.NewChildConstructedNode(BerTree.z3950_dataBaseOrSurDiagnostics,    // 28
                            BerNode.ASN1_CONTEXT);

            if (records != null)
            {
                foreach (RetrivalRecord record in records)
                {
                    record.BuildNamePlusRecord(node);
                }
            }

            END1:

            baPackage = null;
            root.EncodeBERPackage(ref baPackage);

            return 0;
        }

    }

    // Init请求信息结构
    public class InitRequestInfo
    {
        public string m_strReferenceId = "";
        public string m_strProtocolVersion = "";

        public string m_strOptions = "";

        public long m_lPreferredMessageSize = 0;
        public long m_lExceptionalRecordSize = 0;

        public int m_nAuthenticationMethod = 0;	// 0: open 1:idPass
        public string m_strGroupID = "";
        public string m_strID = "";
        public string m_strPassword = "";

        public string m_strImplementationId = "";
        public string m_strImplementationName = "";
        public string m_strImplementationVersion = "";

        public CharsetNeogatiation m_charNego = null;
    }

    public class InitResponseInfo
    {
        public string m_strReferenceId = "";
        public string m_strOptions = "";
        public long m_lPreferredMessageSize = 0;
        public long m_lExceptionalRecordSize = 0;
        public long m_nResult = 0;

        public string m_strImplementationId = "";
        public string m_strImplementationName = "";
        public string m_strImplementationVersion = "";

        // public long m_lErrorCode = 0;

        // public string m_strErrorMessage = "";
        public External UserInfoField = null;

        public CharsetNeogatiation m_charNego = null;
    }

    // Search请求信息结构
    public class SearchRequestInfo
    {
        public string m_strReferenceId = "";

        public long m_lSmallSetUpperBound = 0;
        public long m_lLargeSetLowerBound = 0;
        public long m_lMediumSetPresentNumber = 0;

        // bool
        public long m_lReplaceIndicator = 0;

        public string m_strResultSetName = "default";
        public List<string> m_dbnames = null;

        public BerNode m_rpnRoot = null;
    }

    // Search响应信息结构
    public class SearchResponseInfo
    {
        public string m_strReferenceId = "";

        public long m_lResultCount = 0;
        public long m_lNumberOfRecordReturned = 0;
        public long m_lNextResultSetPosition = 0;

        // bool
        public long m_lSearchStatus = 0;
    }

    // Present请求信息结构
    public class PresentRequestInfo
    {
        public string m_strReferenceId = "";

        public string m_strResultSetID = "";
        public long m_lResultSetStartPoint = 0;
        public long m_lNumberOfRecordsRequested = 0;
        public List<string> m_elementSetNames = null;
    }

    /*
External is defined in the ASN.1 standard.

EXTERNAL ::= [UNIVERSAL 8] IMPLICIT SEQUENCE
{direct-reference      OBJECT IDENTIFIER OPTIONAL,
 indirect-reference    INTEGER           OPTIONAL,
 data-value-descriptor ObjectDescriptor  OPTIONAL,
 encoding              CHOICE
    {single-ASN1-type  [0] ANY,
     octet-aligned     [1] IMPLICIT OCTET STRING,
     arbitrary         [2] IMPLICIT BIT STRING}}

In Z39.50, we use the direct-reference option and omit the
indirect-reference and data-value-descriptor.  For the encoding, we use
single-asn1-type if the record has been defined with ASN.1.  Examples would
be GRS-1 and SUTRS records.  We use octet-aligned for non-ASN.1 records.
The most common example of this would be a MARC record.

Hope this helps!

Ralph
 * */
    // 检索命中的记录
    public class RetrivalRecord
    {
        public string m_strDatabaseName = "";    //
        public External m_external = null;
        public DiagFormat m_surrogateDiagnostic = null;

        // 估算数据所占的包尺寸
        public int GetPackageSize()
        {
            int nSize = 0;

            if (String.IsNullOrEmpty(this.m_strDatabaseName) == false)
            {
                nSize += Encoding.UTF8.GetByteCount(this.m_strDatabaseName);
            }

            if (this.m_external != null)
                nSize += this.m_external.GetPackageSize();

            if (this.m_surrogateDiagnostic != null)
                nSize += this.m_surrogateDiagnostic.GetPackageSize();

            return nSize;
        }

        // 构造NamePlusRecord子树
        // parameters:
        //      node    NamePlusRecord的容器节点。也就是Present Response的根节点
        public void BuildNamePlusRecord(BerNode node)
        {
            if (this.m_external == null
                && this.m_surrogateDiagnostic == null)
                throw new Exception("m_external 和 m_surrogateDiagnostic 不能同时为空");

            if (this.m_external != null
                && this.m_surrogateDiagnostic != null)
                throw new Exception("m_external 和 m_surrogateDiagnostic 不能同时为非空。只能有一个为空");


            BerNode pSequence = node.NewChildConstructedNode(
                BerNode.ASN1_SEQUENCE,    // 16
                BerNode.ASN1_UNIVERSAL);

            // 数据库名
            pSequence.NewChildCharNode(0,
                BerNode.ASN1_CONTEXT,   // ASN1_PRIMITIVE, BUG!!!
                Encoding.UTF8.GetBytes(this.m_strDatabaseName));

            // record(一条记录)
            BerNode nodeRecord = pSequence.NewChildConstructedNode(
                1,
                BerNode.ASN1_CONTEXT);


            if (this.m_external != null)
            {
                // extenal
                BerNode nodeRetrievalRecord = nodeRecord.NewChildConstructedNode(
                    1,
                    BerNode.ASN1_CONTEXT);

                // real extenal!
                BerNode nodeExternal = nodeRetrievalRecord.NewChildConstructedNode(
                    8,  // UNI_EXTERNAL
                    BerNode.ASN1_UNIVERSAL);

                // TODO: 和前一条重复的库名和marc syntax oid可以省略？

                Debug.Assert(String.IsNullOrEmpty(this.m_external.m_strDirectRefenerce) == false, "");

                nodeExternal.NewChildOIDsNode(6,   // UNI_OBJECTIDENTIFIER,
                    BerNode.ASN1_UNIVERSAL,
                    this.m_external.m_strDirectRefenerce);

                // 1 条 MARC 记录
                nodeExternal.NewChildCharNode(1,
                    BerNode.ASN1_CONTEXT,
                    this.m_external.m_octectAligned);
            }

            // 如果获得MARC记录出错，则这里要创建SurrogateDiagnostic record
            if (this.m_surrogateDiagnostic != null)
            {
                BerNode nodeSurrogateDiag = nodeRecord.NewChildConstructedNode(
                    2,
                    BerNode.ASN1_CONTEXT);

                BerNode nodeDiagRoot = nodeSurrogateDiag.NewChildConstructedNode(
                    BerNode.ASN1_SEQUENCE, // sequence
                    BerNode.ASN1_UNIVERSAL);

                this.m_surrogateDiagnostic.BuildBer(nodeDiagRoot);

                /*
                nodeDiagRoot.NewChildOIDsNode(6,
                    BerNode.ASN1_UNIVERSAL,
                    this.m_surrogateDiagnostic.m_strDiagSetID);   // "1.2.840.10003.4.1"

                nodeDiagRoot.NewChildIntegerNode(2,
                    BerNode.ASN1_UNIVERSAL,
                    BitConverter.GetBytes((long)this.m_surrogateDiagnostic.m_nDiagCondition));

                if (String.IsNullOrEmpty(this.m_surrogateDiagnostic.m_strAddInfo) == false)
                {
                    nodeDiagRoot.NewChildCharNode(26,
                        BerNode.ASN1_UNIVERSAL,
                        Encoding.UTF8.GetBytes(this.m_surrogateDiagnostic.m_strAddInfo));
                }
                 * */


            }

        }

    }

}
