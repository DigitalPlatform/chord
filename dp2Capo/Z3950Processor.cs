using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Marc;
using DigitalPlatform.Message;
using DigitalPlatform.Net;
using DigitalPlatform.Text;
using DigitalPlatform.Z3950;
using DigitalPlatform.Z3950.Server;

namespace dp2Capo
{
    public static class Z3950Processor
    {
        public static void AddEvents(ZServer zserver, bool bAdd)
        {
            if (bAdd)
            {
                zserver.SetChannelProperty += Zserver_SetChannelProperty;
                //zserver.GetZConfig += Zserver_GetZConfig;
                zserver.InitializeLogin += Zserver_InitializeLogin;
                zserver.SearchSearch += Zserver_SearchSearch;
                zserver.PresentGetRecords += Zserver_PresentGetRecords;
                zserver.ChannelOpened += Zserver_ChannelOpened;
                //zserver.ChannelClosed += Zserver_ChannelClosed;
            }
            else
            {
                zserver.SetChannelProperty -= Zserver_SetChannelProperty;
                //zserver.GetZConfig -= Zserver_GetZConfig;
                zserver.InitializeLogin -= Zserver_InitializeLogin;
                zserver.SearchSearch -= Zserver_SearchSearch;
                zserver.PresentGetRecords -= Zserver_PresentGetRecords;
                zserver.ChannelOpened += Zserver_ChannelOpened;
                //zserver.ChannelClosed -= Zserver_ChannelClosed;
            }
        }

        private static void Zserver_ChannelOpened(object sender, EventArgs e)
        {
            TcpChannel channel = (TcpChannel)sender;
            channel.Closed += Channel_Closed;
        }

        private static void Channel_Closed(object sender, EventArgs e)
        {
            ZServerChannel channel = (ZServerChannel)sender;
            channel.Closed -= Channel_Closed;   // 避免重入

            // 中断正在进行的检索
            LibraryChannel library_channel = (LibraryChannel)channel.Tag;
            if (library_channel != null)
            {
                library_channel.Abort();
                LibraryManager.Log?.Info(string.Format("ZServerChannel({0}) Channel_Closed() 引发 LibraryChannel.Abort()", channel.GetHashCode()));
            }

            List<string> names = GetResultSetNameList(channel, true);
            if (names.Count > 0)
            {
                FreeGlobalResultSets(channel, names);
            }
        }

#if NO
        private static void Zserver_ChannelClosed(object sender, ChannelClosedEventArgs e)
        {
            List<string> names = GetResultSetNameList(e.Channel);
            if (names.Count > 0)
            {
                FreeGlobalResultSets(e.Channel, names);
            }
        }
#endif

        private static void Zserver_PresentGetRecords(object sender, PresentGetRecordsEventArgs e)
        {
            string strError = "";
            int nCondition = 100;   // (unspecified)error

            ZServerChannel zserver_channel = (ZServerChannel)sender;

            if (zserver_channel == null)
                throw new ArgumentException("zserver_channel 为空");

            string strInstanceName = zserver_channel.EnsureProperty().GetKeyValue("i_n");
            if (strInstanceName == null)
            {
                strError = "通道中 实例名 'i_n' 尚未初始化";   // ?? bug
                LibraryManager.Log?.Error(strError);
                goto ERROR1;
            }
            Instance instance = FindZ3950Instance(strInstanceName, out strError);
            if (instance == null)
            {
                if (string.IsNullOrEmpty(strError))
                    strError = "实例名 '" + strInstanceName + "' 不存在(或实例没有启用 Z39.50 服务)";
                goto ERROR1;
            }
            if (instance.Running == false)
            {
                strError = "实例 '" + instance.Name + "' 正在维护中，暂时不能访问";
                nCondition = 1019;  // Init/AC: System not available due to maintenance
                goto ERROR1;
            }

            string strResultSetName = e.Request.m_strResultSetID;
            if (String.IsNullOrEmpty(strResultSetName) == true)
                strResultSetName = "default";
            long lStart = e.Request.m_lResultSetStartPoint - 1;
            long lNumber = e.Request.m_lNumberOfRecordsRequested;

            int MAX_PRESENT_RECORD = 100;

            // 限制每次 present 的记录数量
            if (lNumber > MAX_PRESENT_RECORD)
                lNumber = MAX_PRESENT_RECORD;

            DiagFormat diag = null;
            List<RetrivalRecord> records = new List<RetrivalRecord>();

            string strUserName = zserver_channel.EnsureProperty().GetKeyValue("i_u");
            string strPassword = zserver_channel.EnsureProperty().GetKeyValue("i_p");

            LoginInfo login_info = BuildLoginInfo(strUserName, strPassword);
            LibraryChannel library_channel = instance.MessageConnection.GetChannel(login_info);
            zserver_channel.Tag = library_channel;
            try
            {
                // 全局结果集名
                string resultset_name = MakeGlobalResultSetName(zserver_channel, strResultSetName);

                // TODO: timestamp 有必要获取么？
                ResultSetLoader loader = new ResultSetLoader(library_channel,
                    resultset_name,
                    "id,xml,timestamp")
                {
                    Start = lStart,
                    BatchSize = Math.Min(10, lNumber)
                };
                int i = 0;
                int nSize = 0;
                foreach (DigitalPlatform.LibraryClient.localhost.Record dp2library_record in loader)
                {
                    if (i >= lNumber)
                        break;

                    // 判断请求边界是否合法。放到循环里面的 i==0 时刻来做，是因为枚举器(loader)需要请求 dp2library 之后才能知道结果集大小
                    if (i == 0)
                    {
                        e.TotalCount = loader.TotalCount;
                        if (lStart >= loader.TotalCount)
                        {
                            DiagFormat diag1 = null;
                            ZProcessor.SetPresentDiagRecord(ref diag1,
    13,  // Present request out-of-range
    "Present 所请求的起始偏移位置 " + lStart + " 超过结果集中记录总数 " + loader.TotalCount);
                            e.Diag = diag1;
                            return;
                        }
                        if (lStart + lNumber > loader.TotalCount)
                        {
                            DiagFormat diag1 = null;
                            ZProcessor.SetPresentDiagRecord(ref diag1,
    13,  // Present request out-of-range
    "Present 所请求的结束偏移位置 " + (lStart + lNumber) + " 超过结果集中记录总数 " + loader.TotalCount);
                            e.Diag = diag1;
                            return;
                        }
                    }

                    {
                        // 解析出数据库名和ID
                        string strDbName = dp2StringUtil.GetDbName(dp2library_record.Path);
                        string strRecID = dp2StringUtil.GetRecordID(dp2library_record.Path);

                        // 如果取得的是xml记录，则根元素可以看出记录的marc syntax，进一步可以获得oid；
                        // 如果取得的是MARC格式记录，则需要根据数据库预定义的marc syntax来看出oid了
                        // string strMarcSyntaxOID = GetMarcSyntaxOID(instance, strDbName);
                        // string strMarcSyntaxOID = GetMarcSyntaxOID(dp2library_record);

                        RetrivalRecord record = new RetrivalRecord
                        {
                            m_strDatabaseName = strDbName
                        };

                        // 根据书目库名获得书目库属性对象
                        // TODO: 这里可以考虑 cache
                        BiblioDbProperty prop = instance.zhost.GetDbProperty(
                            strDbName,
                            false);

                        int nRet = GetIso2709Record(dp2library_record,
                            e.Request.m_elementSetNames,
                            prop != null ? prop.AddField901 : false,
                            prop != null ? prop.RemoveFields : "997",
                            zserver_channel.EnsureProperty().MarcRecordEncoding,
                            out string strMarcSyntaxOID,
                            out byte[] baIso2709,
                            out strError);

                        /*
                                                // 测试记录群中包含诊断记录
                                                if (i == 1)
                                                {
                                                    nRet = -1;
                                                    strError = "测试获取记录错误";
                                                }
                        */
                        if (nRet == -1)
                        {
                            record.m_surrogateDiagnostic = new DiagFormat
                            {
                                m_strDiagSetID = "1.2.840.10003.4.1",
                                m_nDiagCondition = 14,  // system error in presenting records
                                m_strAddInfo = strError
                            };
                        }
                        else if (nRet == 0)
                        {
                            record.m_surrogateDiagnostic = new DiagFormat
                            {
                                m_strDiagSetID = "1.2.840.10003.4.1",
                                m_nDiagCondition = 1028,  // record deleted
                                m_strAddInfo = strError
                            };
                        }
                        else if (String.IsNullOrEmpty(strMarcSyntaxOID) == true)
                        {
                            // 根据数据库名无法获得marc syntax oid。可能是虚拟库检索命中记录所在的物理库没有在 capo.xml 中配置。
                            record.m_surrogateDiagnostic = new DiagFormat
                            {
                                m_strDiagSetID = "1.2.840.10003.4.1",
                                m_nDiagCondition = 227,  // No data available in requested record syntax // 239?
                                // m_strAddInfo = "根据数据库名 '" + strDbName + "' 无法获得 marc syntax oid"
                                m_strAddInfo = "根据书目记录 '" + dp2library_record.Path + "' 的 MARCXML 无法获得 marc syntax oid"
                            };
                        }
                        else
                        {
                            record.m_external = new External
                            {
                                m_strDirectRefenerce = strMarcSyntaxOID,
                                m_octectAligned = baIso2709
                            };
                        }

                        // TODO: 测试观察这里的 size 增长情况
                        nSize += record.GetPackageSize();

                        if (i == 0)
                        {
                            // 连一条记录也放不下
                            if (nSize > zserver_channel.EnsureProperty().ExceptionalRecordSize)
                            {
                                Debug.Assert(diag == null, "");
                                ZProcessor.SetPresentDiagRecord(ref diag,
                                    17, // record exceeds Exceptional_record_size
                                    "记录尺寸 " + nSize.ToString() + " 超过 Exceptional_record_size " + zserver_channel.EnsureProperty().ExceptionalRecordSize.ToString());
                                lNumber = 0;
                                break;
                            }
                        }
                        else
                        {
                            if (nSize >= zserver_channel.EnsureProperty().PreferredMessageSize)
                            {
                                // TODO: 记入日志?
                                // 调整返回的记录数
                                lNumber = i;
                                break;
                            }
                        }

                        records.Add(record);
                    }

                    i++;
                    // 2018/9/28
                    // 防范性编程
                    // TODO: 还可以通过时间长度来控制。超过一定时间，就抛出异常
                    if (i > MAX_PRESENT_RECORD)
                        throw new Exception("Zserver_PresentGetRecords() 中获取记录的循环超过极限数量 " + MAX_PRESENT_RECORD + "(此时 lNumber=" + lNumber + ")");
                }
            }
            catch (ChannelException ex)
            {
                // 指定的结果集没有找到
                if (ex.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                {
                    nCondition = 30;  // Specified result set does not exist
                    strError = ex.Message;
                    goto ERROR1;
                }
                strError = "获取结果集时出现异常(ChannelException): " + ex.Message;
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "获取结果集时出现异常: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                zserver_channel.Tag = null;
                instance.MessageConnection.ReturnChannel(library_channel);
            }

            e.Records = records;
            e.Diag = diag;
            return;
            ERROR1:
            {
                DiagFormat diag1 = null;
                ZProcessor.SetPresentDiagRecord(ref diag1,
                    nCondition,
                    strError);
                e.Diag = diag1;
                return;
            }
        }

#if NO
        static string GetMarcSyntaxOID(Instance instance, string strBiblioDbName)
        {
            string strSyntax = instance.zhost.GetMarcSyntax(strBiblioDbName);
            if (strSyntax == null)
                return null;
            if (strSyntax == "unimarc")
                return "1.2.840.10003.5.1";
            if (strSyntax == "usmarc")
                return "1.2.840.10003.5.10";

            return null;
        }
#endif

#if NO
        // 获得一条记录的 MARC 格式 OID
        static string GetMarcSyntaxOID(DigitalPlatform.LibraryClient.localhost.Record dp2library_record)
        {
            if (dp2library_record.RecordBody == null)
                return null;

            if (string.IsNullOrEmpty(dp2library_record.RecordBody.Xml))
                return null;

            // return:
            //      -1  出错
            //      0   正常
            int nRet = MarcUtil.GetMarcSyntax(dp2library_record.RecordBody.Xml,
        out string strOutMarcSyntax,
        out string strError);
            if (nRet == -1)
                throw new Exception("获得 MARCXML 记录的 MARC Syntax 时出错: " + strError);

            if (strOutMarcSyntax == "unimarc")
                return "1.2.840.10003.5.1";
            if (strOutMarcSyntax == "usmarc")
                return "1.2.840.10003.5.10";

            return null;
        }
#endif

        // TODO: 实现简略格式和详细格式提供。简略格式可以理解为略去一些分类号主题词字段，最好用一段用户定制的脚本来进行过滤
        // 获得MARC记录
        // parameters:
        //      elementSetNames 元素集列表。每个元素集为 'B' 或 'F'，表示简略和详细格式
        //      bAddField901    是否加入901字段？
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        static int GetIso2709Record(
            DigitalPlatform.LibraryClient.localhost.Record dp2library_record,
            List<string> elementSetNames,
            bool bAddField901,
            string strRemoveFields,
            Encoding marcRecordEncoding,
            out string strMarcSyntaxOID,
            out byte[] baIso2709,
            out string strError)
        {
            baIso2709 = null;
            strError = "";
            strMarcSyntaxOID = "";

            // 转换为机内格式
            int nRet = MarcUtil.Xml2Marc(dp2library_record.RecordBody.Xml,
                true,
                "",
                out string strMarcSyntax,
                out string strMarc,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            // 去掉记录中的 997/998
            MarcRecord record = new MarcRecord(strMarc);
            if (string.IsNullOrEmpty(strRemoveFields) == false)
            {
                List<string> field_names = StringUtil.SplitList(strRemoveFields);
                foreach (string field_name in field_names)
                {
                    if (field_name.Length != 3)
                    {
                        strError = "removeFields 定义里面出现了不是 3 字符的字段名('" + strRemoveFields + "')";
                        return -1;
                    }
                    record.select("field[@name='" + field_name + "']").detach();
                }
            }

            if (bAddField901 == true)
            {
                // 901  $p记录路径$t时间戳
                string strContent = "$p" + dp2library_record.Path
                    + "$t" + ByteArray.GetHexTimeStampString(dp2library_record.RecordBody.Timestamp);
                record.setFirstField("901", "  ", strContent.Replace("$", MarcQuery.SUBFLD), "  ");
            }
            strMarc = record.Text;

            // 转换为ISO2709
            nRet = MarcUtil.CvtJineiToISO2709(
                strMarc,
                strMarcSyntax,
                marcRecordEncoding,
                out baIso2709,
                out strError);
            if (nRet == -1)
                return -1;

            if (strMarcSyntax == "unimarc")
                strMarcSyntaxOID = "1.2.840.10003.5.1";
            if (strMarcSyntax == "usmarc")
                strMarcSyntaxOID = "1.2.840.10003.5.10";

            return 1;
        }

        // 慢速检索的时间长度阈值
        static TimeSpan slow_length = TimeSpan.FromSeconds(5);

        private static void Zserver_SearchSearch(object sender, SearchSearchEventArgs e)
        {
            string strError = "";
            int nCondition = 100;

            ZServerChannel zserver_channel = (ZServerChannel)sender;

            if (zserver_channel == null)
                throw new ArgumentException("zserver_channel 为空");

            string strInstanceName = zserver_channel.EnsureProperty().GetKeyValue("i_n");
            if (strInstanceName == null)
            {
                string strErrorText = "通道中 实例名 'i_n' 尚未初始化";    // ?? bug
                LibraryManager.Log?.Error(strErrorText);
                e.Result = new DigitalPlatform.Z3950.ZClient.SearchResult { Value = -1, ErrorInfo = strErrorText };
                return;
            }
            Instance instance = FindZ3950Instance(strInstanceName, out strError);
            if (instance == null)
            {
                if (string.IsNullOrEmpty(strError))
                    strError = "实例名 '" + strInstanceName + "' 不存在(或实例没有启用 Z39.50 服务)";
                e.Result = new DigitalPlatform.Z3950.ZClient.SearchResult { Value = -1, ErrorInfo = strError };
                return;
            }
            if (instance.Running == false)
            {
                strError = "实例 '" + instance.Name + "' 正在维护中，暂时不能访问";
                nCondition = 1019;  // Init/AC: System not available due to maintenance
                goto ERROR1;
            }

            // 检查实例是否有至少一个可用数据库
            if (instance.zhost.GetDbCount() == 0)
            {
#if NO
                string strErrorText = "实例 '" + strInstanceName + "' 没有提供可检索的数据库";
                DiagFormat diag = null;
                ZProcessor.SetPresentDiagRecord(ref diag,
                    1017,  // Init/AC: No databases available for specified userId
                    strErrorText);
                e.Diag = diag;
                e.Result = new ZClient.SearchResult { Value = -1, ErrorInfo = strErrorText };
                return;
#endif
                strError = "实例 '" + instance.Name + "' 没有提供可检索的数据库";
                nCondition = 1017;// Init/AC: No databases available for specified userId
                goto ERROR1;
            }

            // string text = JsonConvert.SerializeObject(e.Request.m_rpnRoot, Formatting.Indented);

            // 根据逆波兰表构造出 dp2 系统检索式
            // return:
            //      -1  出错
            //      0   数据库没有找到
            //      1   成功
            int nRet = Z3950Utility.BuildQueryXml(
                instance.zhost,
                e.Request.m_dbnames,
                e.Request.m_rpnRoot,
                zserver_channel.EnsureProperty().SearchTermEncoding,
                out string strQueryXml,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
#if NO
                DiagFormat diag = null;
                ZProcessor.SetPresentDiagRecord(ref diag,
                    nRet == -1 ? 2 : 235,  // 2:temporary system error; 235:Database does not exist (database name)
                    strError);
                e.Diag = diag;
                e.Result = new ZClient.SearchResult { Value = -1, ErrorInfo = strError };
                return;
#endif
                nCondition = nRet == -1 ? 2 : 235;  // 2:temporary system error; 235:Database does not exist (database name)
                goto ERROR1;
            }

            string strUserName = zserver_channel.EnsureProperty().GetKeyValue("i_u");
            string strPassword = zserver_channel.EnsureProperty().GetKeyValue("i_p");

            LoginInfo login_info = BuildLoginInfo(strUserName, strPassword);
            LibraryChannel library_channel = instance.MessageConnection.GetChannel(login_info);
            try
            {
                // TODO: 附加到 Abort 事件。这样当事件被触发的时候，能执行 library_channel.Abort

                // 全局结果集名
                string resultset_name = MakeGlobalResultSetName(zserver_channel, e.Request.m_strResultSetName);

                DateTime start = DateTime.Now;
                // 进行检索
                long lRet = 0;

                zserver_channel.Tag = library_channel;
                try
                {
                    lRet = library_channel.Search(
            strQueryXml,
            resultset_name,
            "", // strOutputStyle
            out strError);
                }
                finally
                {
                    zserver_channel.Tag = null;
                }

                // testing System.Threading.Thread.Sleep(TimeSpan.FromSeconds(6));
                TimeSpan length = DateTime.Now - start;
                if (length >= slow_length)
                {
                    // TODO: TcpClient 可能为 null, 表示通道已经被切断
                    //string ip = TcpServer.GetClientIP(zserver_channel.TcpClient);
                    //string strChannelName = "ip:" + ip + ",channel:" + zserver_channel.GetHashCode();

                    LibraryManager.Log?.Info("通道 " + zserver_channel.GetDebugName(zserver_channel.TcpClient) + " 检索式 '" + strQueryXml + "' 检索耗时 " + length.ToString() + " (命中记录 " + lRet + ")，超过慢速阈值");
                }

                if (lRet == -1)
                    LibraryManager.Log?.Error("通道 " + zserver_channel.GetDebugName(zserver_channel.TcpClient) + " 检索式 '" + strQueryXml + "' 检索出错: " + strError);


                /*
                // 测试检索失败
                lRet = -1;
                strError = "测试检索失败";
                 * */

                if (lRet == -1)
                {
#if NO
                    DiagFormat diag = null;
                    ZProcessor.SetPresentDiagRecord(ref diag,
                        2,  // temporary system error
                        strError);
                    e.Diag = diag;
                    e.Result = new ZClient.SearchResult { Value = -1, ErrorInfo = strError };
                    return;
#endif
                    goto ERROR1;
                }
                else
                {
                    // 记忆结果集名
                    // return:
                    //      false   正常
                    //      true    结果集数量超过 MAX_RESULTSET_COUNT，返回前已经开始释放所有结果集
                    if (MemoryResultSetName(zserver_channel, resultset_name) == true)
                    {
#if NO
                        DiagFormat diag = null;
                        ZProcessor.SetPresentDiagRecord(ref diag,
                            112,  // Too many result sets created (maximum)
                            strError);  // TODO: 应为 MAX_RESULTSET_COUNT
                        e.Diag = diag;
                        e.Result = new ZClient.SearchResult { Value = -1, ErrorInfo = strError };
                        return;
#endif
                        nCondition = 112;  // Too many result sets created (maximum)
                        goto ERROR1;
                    }

                    e.Result = new ZClient.SearchResult { ResultCount = lRet };
                }
                return;
            }
            finally
            {
                instance.MessageConnection.ReturnChannel(library_channel);
            }
            ERROR1:
            {
                DiagFormat diag = null;
                ZProcessor.SetPresentDiagRecord(ref diag,
                    nCondition,
                    strError);
                e.Diag = diag;
                e.Result = new ZClient.SearchResult { Value = -1, ErrorInfo = strError };
                return;
            }
        }

        // 构造全局结果集名
        static string MakeGlobalResultSetName(TcpChannel zserver_channel, string strResultSetName)
        {
            return "#" + zserver_channel.GetHashCode() + "_" + strResultSetName;
        }

        // 一个 ZServerChannel 中能用到的最多全局结果集数。超过这个数目就会自动开始删除。删除可能会造成前端访问某些结果集时报错
        static readonly int MAX_RESULTSET_COUNT = 100;

        // 记忆全局结果集名
        // return:
        //      false   正常
        //      true    结果集数量超过 MAX_RESULTSET_COUNT，返回前已经开始释放所有结果集
        static bool MemoryResultSetName(ZServerChannel zserver_channel,
            string resultset_name)
        {
            if (zserver_channel == null)
                throw new ArgumentException("zserver_channel 为空");

            if (!(zserver_channel.EnsureProperty().GetKeyObject("r_n") is List<string> names))
            {
                names = new List<string>();
                zserver_channel.EnsureProperty().SetKeyObject("r_n", names);
            }

            if (names.IndexOf(resultset_name) == -1)
                names.Add(resultset_name);

            // 如果结果集名数量太多，就要开始删除
            if (names.Count > MAX_RESULTSET_COUNT)
            {
                FreeGlobalResultSets(zserver_channel, names);
                // 2018/9/28
                names = new List<string>();
                zserver_channel.EnsureProperty().SetKeyObject("r_n", names);
                return true;
            }

            return false;
        }

        // 取出先前记忆的全局结果集名列表
        // parameters:
        //      bRemove 是否在返回前自动删除 key_object 集合中的值
        static List<string> GetResultSetNameList(ZServerChannel zserver_channel,
            bool bRemove = false)
        {
            lock (zserver_channel)
            {
                if (!(zserver_channel.EnsureProperty().GetKeyObject("r_n") is List<string> names))
                    return new List<string>();
                else
                {
                    if (bRemove)
                        zserver_channel.EnsureProperty().SetKeyObject("r_n", null);
                }
                return names;
            }
        }

        static void FreeGlobalResultSets(ZServerChannel zserver_channel,
            List<string> names)
        {
            if (zserver_channel == null)
                throw new ArgumentException("zserver_channel 为空");

            string strInstanceName = zserver_channel.EnsureProperty().GetKeyValue("i_n");
            if (strInstanceName == null)
            {
                LibraryManager.Log?.Error("通道中 实例名 'i_n' 尚未初始化");   // ?? bug
            }
            Instance instance = FindZ3950Instance(strInstanceName, out string strError);
            if (instance == null)
            {
                if (string.IsNullOrEmpty(strError))
                    strError = "实例名 '" + strInstanceName + "' 不存在(或实例没有启用 Z39.50 服务)";
                // 写入错误日志
                LibraryManager.Log?.Error(strError);
                return;
            }

            // 交给 Instance 释放
            instance.AddGlobalResultSets(names);

#if NO
            LibraryChannel library_channel = instance.MessageConnection.GetChannel(null);
            try
            {
                foreach (string name in names)
                {
                    // TODO: 要是能用通配符来删除大量结果集就好了
                    long lRet = library_channel.GetSearchResult("",
                        0,
                        0,
                        "@remove:" + name,
                        "zh",
                        out DigitalPlatform.LibraryClient.localhost.Record[] searchresults,
                        out string strError);
                    if (lRet == -1)
                    {
                        // 写入错误日志
                        return;
                    }
                }
            }
            finally
            {
                instance.MessageConnection.ReturnChannel(library_channel);
            }
#endif
        }

        public static Tuple<Instance, string> FindZ3950Instance(string strInstanceName)
        {
            Instance instance = ServerInfo.FindInstance(strInstanceName);
            if (instance == null)
                return new Tuple<Instance, string>(null, "实例 '" + strInstanceName + "' 不存在");
            if (instance.zhost == null)
                return new Tuple<Instance, string>(null, "实例 '" + strInstanceName + "' 没有启用 Z39.50 服务");
            return new Tuple<Instance, string>(instance, "");
        }

        public static Instance FindZ3950Instance(string strInstanceName, out string strError)
        {
            strError = "";
            var ret = FindZ3950Instance(strInstanceName);
            if (ret.Item1 != null)
                return ret.Item1;
            strError = ret.Item2;
            return null;
        }

#if NO
        private static void Zserver_GetZConfig(object sender, GetZConfigEventArgs e)
        {
            ZServerChannel zserver_channel = (ZServerChannel)sender;

#if NO
            List<string> parts = StringUtil.ParseTwoPart(e.Info.m_strID, "@");
            string strInstanceName = parts[1];
#endif
            string strInstanceName = zserver_channel.SetProperty().GetKeyValue("i_n");
            if (strInstanceName == null)
            {
                string strError = "通道中 实例名 '" + strInstanceName + "' 尚未初始化";
                ZManager.Log.Error(strError);
                e.ZConfig = null;
                e.Result.ErrorInfo = strError;
                return;
            }

            Instance instance = FindInstance(strInstanceName);
            if (instance == null)
            {
                e.ZConfig = null;
                e.Result.ErrorInfo = "以用户名 '" + e.Info.m_strID + "' 中包含的实例名 '" + strInstanceName + "' 没有找到任何实例";
                return;
            }

            // 让 channel 携带 Instance Name
            // zserver_channel.SetProperty().SetKeyValue("i_n", strInstanceName);

            e.ZConfig = new ZConfig
            {
                AnonymousUserName = instance.zhost.AnonymousUserName,
                AnonymousPassword = instance.zhost.AnonymousPassword,
            };
        }
#endif
        private static void Zserver_InitializeLogin(object sender, InitializeLoginEventArgs e)
        {
            ZServerChannel zserver_channel = (ZServerChannel)sender;

            if (zserver_channel == null)
                throw new ArgumentException("zserver_channel 为空");

            string strInstanceName = zserver_channel.EnsureProperty().GetKeyValue("i_n");
            if (strInstanceName == null)
            {
                string strErrorText = "通道中 实例名 'i_n' 尚未初始化";    // ?? bug
                LibraryManager.Log?.Error(strErrorText);
                e.Result = new Result
                {
                    Value = -1,
                    ErrorCode = "2",
                    ErrorInfo = strErrorText
                };
                return;
            }
            Instance instance = FindZ3950Instance(strInstanceName, out string strError);
            if (instance == null)
            {
                if (string.IsNullOrEmpty(strError))
                    strError = "实例名 '" + strInstanceName + "' 不存在(或实例没有启用 Z39.50 服务)";
                e.Result = new Result
                {
                    Value = -1,
                    ErrorCode = "",
                    ErrorInfo = strError
                };
                return;
            }
            if (instance.Running == false)
            {
                e.Result = new Result
                {
                    Value = -1,
                    ErrorCode = "1019", // Init/AC: System not available due to maintenance
                    ErrorInfo = "实例 '" + instance.Name + "' 正在维护中，暂时不能访问"
                };
                return;
            }

            // TODO: TcpClient 可能为 null, 表示通道已经被切断
            string strClientIP = ZServer.GetClientIP(zserver_channel.TcpClient);

            // testing
            // strClientIP = "127.0.0.2";

            string strUserName = zserver_channel.EnsureProperty().GetKeyValue("i_u");
            string strPassword = zserver_channel.EnsureProperty().GetKeyValue("i_p");

            LoginInfo login_info = BuildLoginInfo(strUserName, strPassword);

            LibraryChannel library_channel = instance.MessageConnection.GetChannel(login_info);
            try
            {
                string strParameters = "";
                if (login_info.UserType == "patron")
                    strParameters += ",type=reader";
                strParameters += ",client=dp2capo|" + "0.01" + ",clientip=" + strClientIP;

                // result.Value:
                //      -1  登录出错
                //      0   登录未成功
                //      1   登录成功
                long lRet = library_channel.Login(login_info.UserName,
                    login_info.Password,    // strPassword,
                    strParameters,
                    out strError);
                e.Result.Value = (int)lRet;
                if (lRet != 1)
                    e.Result.ErrorCode = "101";
                e.Result.ErrorInfo = strError;
            }
            finally
            {
                instance.MessageConnection.ReturnChannel(library_channel);
            }
        }

        static LoginInfo BuildLoginInfo(string strUserName, string strPassword)
        {
            if (strUserName.StartsWith("~"))
            {
                string strBarcode = strUserName.Substring(1);
                return new LoginInfo
                {
                    UserName = strBarcode,
                    UserType = "patron",
                    Password = strPassword
                };
            }
            else
                return new LoginInfo { UserName = strUserName, Password = strPassword };
        }

        private static void Zserver_SetChannelProperty(object sender, SetChannelPropertyEventArgs e)
        {
            ZServerChannel zserver_channel = (ZServerChannel)sender;

            List<string> parts = StringUtil.ParseTwoPart(e.Info.m_strID, "@");
            string strUserName = parts[0];
            string strInstanceName = parts[1];

            string strPassword = e.Info.m_strPassword;

            // 匿名登录情形
            if (string.IsNullOrEmpty(strUserName))
            {
                Instance instance = FindZ3950Instance(strInstanceName, out string strError);
                if (instance == null)
                {
                    if (string.IsNullOrEmpty(strError))
                        strError = "以用户名 '" + e.Info.m_strID + "' 中包含的实例名 '" + strInstanceName + "' 没有找到任何实例(或实例没有启用 Z39.50 服务)";
                    else
                        strError = "以用户名 '" + e.Info.m_strID + "' 中包含的实例名 '" + strInstanceName + "' 定位，" + strError;
                    e.Result = new Result
                    {
                        Value = -1,
                        ErrorCode = "",
                        ErrorInfo = strError
                    };
                    return;
                }

                strInstanceName = instance.Name;

                // 如果定义了允许匿名登录
                if (String.IsNullOrEmpty(instance.zhost.AnonymousUserName) == false)
                {
                    strUserName = instance.zhost.AnonymousUserName;
                    strPassword = instance.zhost.AnonymousPassword;
                }
                else
                {
                    e.Result = new Result
                    {
                        Value = -1,
                        ErrorCode = "101",
                        ErrorInfo = "不允许匿名登录"
                    };
                    return;
                }
            }

            // 让 channel 从此携带 Instance Name
            zserver_channel.EnsureProperty().SetKeyValue("i_n", strInstanceName);
            zserver_channel.EnsureProperty().SetKeyValue("i_u", strUserName);
            zserver_channel.EnsureProperty().SetKeyValue("i_p", strPassword);

            Debug.Assert(e.Result != null, "");
        }
    }
}
