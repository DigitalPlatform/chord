using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Message;
using DigitalPlatform.MessageClient;
using DigitalPlatform;
using DigitalPlatform.LibraryClient;

namespace dp2Capo
{
    /// <summary>
    /// 连接到 dp2mserver 的通讯通道
    /// </summary>
    public class ServerConnection : MessageConnection
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public HostInfo dp2library { get; set; }
        internal LibraryChannelPool _channelPool = new LibraryChannelPool();

        public ServerConnection()
        {
            this._channelPool.BeforeLogin += new BeforeLoginEventHandle(_channelPool_BeforeLogin);
        }

        void _channelPool_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == true)
            {
                e.LibraryServerUrl = this.dp2library.Url;
                bool bIsReader = false;

                e.UserName = this.dp2library.UserName;

                e.Password = this.dp2library.Password;

                bIsReader = false;

                // e.Parameters = "location=" + strLocation;
                if (bIsReader == true)
                    e.Parameters += ",type=reader";

                // e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

                e.Parameters += ",client=dp2capo|" + "0.01";    // +Program.ClientVersion;

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            // TODO: 可以出现对话框，但要注意跨线程的问题
            // TODO: 当首次登录对话框没有输入密码的时候，这里就必须出现对话框询问密码了
            e.Cancel = true;
        }

        public LibraryChannel GetChannel()
        {
            string strServerUrl = this.dp2library.Url;
            string strUserName = this.dp2library.UserName;

            return this._channelPool.GetChannel(strServerUrl, strUserName);
        }

        #region BindPatron() API

        public override void OnBindPatronRecieved(BindPatronRequest param)
        {
            // 单独给一个线程来执行
            Task.Factory.StartNew(() => BindPatronAndResponse(param));
        }

        void BindPatronAndResponse(BindPatronRequest param)
        {
            string strError = "";
            IList<string> results = new List<string>();

            LibraryChannel channel = GetChannel();
            try
            {
                string[] temp_results = null;
                long lRet = channel.BindPatron(param.Action,
                    param.QueryWord,
                    param.Password,
                    param.BindingID,
                    param.Style,
                    param.ResultTypeList,
                    out temp_results,
                    out strError);
                if (temp_results != null)
                {
                    foreach (string s in temp_results)
                    {
                        results.Add(s);
                    }
                }
                ResponseBindPatron(param.TaskID,
    lRet,
    results,
    strError);
                return;
            }
            catch (Exception ex)
            {
                AddErrorLine("BindPatronAndResponse() 出现异常: " + ex.Message);
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

        ERROR1:
            // 报错
            ResponseBindPatron(
param.TaskID,
-1,
results,
strError);
        }

        #endregion


        #region Search() API

        // 
        // 当 server 发来检索请求的时候被调用。重载的时候要进行检索，并调用 Response 把检索结果发送给 server
        public override void OnSearchRecieved(SearchRequest param)
        {
            // 单独给一个线程来执行
            Task.Factory.StartNew(() => SearchAndResponse(param));
        }

        // TODO: 本函数最好放在一个工作线程内执行
        // Form Close 的时候要及时中断工作线程
        // getPatronInfo getBiblioInfo getBiblioSummary searchBiblio searchPatron
        void SearchAndResponse(SearchRequest searchParam)
        {
            if (searchParam.Operation == "getPatronInfo")
            {
                GetPatronInfo(searchParam);
                return;
            }

            if (searchParam.Operation == "getBiblioInfo")
            {
                GetBiblioInfo(searchParam);
                return;
            }

            if (searchParam.Operation == "getBiblioSummary")
            {
                GetBiblioSummary(searchParam);
                return;
            }

            if (searchParam.Operation == "getItemInfo")
            {
                GetItemInfo(searchParam);
                return;
            }

            string strError = "";
            IList<DigitalPlatform.Message.Record> records = new List<DigitalPlatform.Message.Record>();

            string strResultSetName = searchParam.ResultSetName;
            if (string.IsNullOrEmpty(strResultSetName) == true)
                strResultSetName = "default";
            else
                strResultSetName = "#" + strResultSetName;  // 如果请求方指定了结果集名，则在 dp2library 中处理为全局结果集名

            LibraryChannel channel = GetChannel();
            try
            {
                string strQueryXml = "";
                long lRet = 0;

                if (searchParam.QueryWord == "!getResult")
                {
                    lRet = -1;
                }
                else
                {
                    if (searchParam.Operation == "searchBiblio")
                    {
                        lRet = channel.SearchBiblio(// null,
                             searchParam.DbNameList,
                             searchParam.QueryWord,
                             (int)searchParam.MaxResults,
                             searchParam.UseList,
                             searchParam.MatchStyle,
                             "zh",
                             strResultSetName,
                             "", // strSearchStyle
                             "", // strOutputStyle
                             out strQueryXml,
                             out strError);
                    }
                    else if (searchParam.Operation == "searchPatron")
                    {
                        lRet = channel.SearchReader(// null,
                            searchParam.DbNameList,
                            searchParam.QueryWord,
                            (int)searchParam.MaxResults,
                            searchParam.UseList,
                            searchParam.MatchStyle,
                            "zh",
                            strResultSetName,
                            "",
                            out strError);
                    }
                    else
                    {
                        lRet = -1;
                        strError = "无法识别的 Operation 值 '" + searchParam.Operation + "'";
                    }

                    if (lRet == -1 || lRet == 0)
                    {
                        if (lRet == 0
                            || (lRet == -1 && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound))
                        {
                            // 没有命中
                            ResponseSearch(
    searchParam.TaskID,
    0,
    0,
    records,
    strError);  // 出错信息大概为 not found。
                            return;
                        }
                        goto ERROR1;
                    }
                }


                {
                    long lHitCount = lRet;

                    if (searchParam.Count == 0)
                    {
                        // 返回命中数
                        ResponseSearch(
                            searchParam.TaskID,
                            lHitCount,
0,
records,
"本次没有返回任何记录");
                        return;
                    }

                    long lStart = searchParam.Start;
                    long lPerCount = searchParam.Count; // 本次拟返回的个数

                    if (lHitCount != -1)
                    {
                        if (lPerCount == -1)
                            lPerCount = lHitCount - lStart;
                        else
                            lPerCount = Math.Min(lPerCount, lHitCount - lStart);

                        if (lPerCount <= 0)
                        {
                            strError = "命中结果总数为 " + lHitCount + "，取结果开始位置为 " + lStart + "，它已超出结果集范围";
                            goto ERROR1;
                        }
                    }

                    DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                    // 装入浏览格式
                    for (; ; )
                    {
                        string strBrowseStyle = "id,xml";

                        lRet = channel.GetSearchResult(
                            // null,
            strResultSetName,
            lStart,
            lPerCount,
            strBrowseStyle,
            "zh", // this.Lang,
            out searchresults,
            out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        if (searchresults.Length == 0)
                        {
                            strError = "GetSearchResult() searchResult empty";
                            goto ERROR1;
                        }

                        if (lHitCount == -1)
                            lHitCount = lRet;   // 延迟得到命中总数

                        records.Clear();
                        foreach (DigitalPlatform.LibraryClient.localhost.Record record in searchresults)
                        {
                            DigitalPlatform.Message.Record biblio = new DigitalPlatform.Message.Record();
                            biblio.RecPath = record.Path;
                            biblio.Data = record.RecordBody.Xml;
                            records.Add(biblio);
                        }

                        ResponseSearch(
                            searchParam.TaskID,
                            lHitCount,
                            lStart,
                            records,
                            "");

                        lStart += searchresults.Length;

                        if (lPerCount != -1)
                            lPerCount -= searchresults.Length;

                        if (lStart >= lHitCount || (lPerCount <= 0 && lPerCount != -1))
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                AddErrorLine("SearchAndResponse() 出现异常: " + ex.Message);
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

            this.AddInfoLine("search and response end");
            return;
        ERROR1:
            // 报错
            ResponseSearch(
searchParam.TaskID,
-1,
0,
records,
strError);
        }

        void GetItemInfo(SearchRequest searchParam)
        {
            string strError = "";
            IList<DigitalPlatform.Message.Record> records = new List<DigitalPlatform.Message.Record>();

#if NO
            if (string.IsNullOrEmpty(searchParam.FormatList) == true)
            {
                strError = "FormatList 不应为空";
                goto ERROR1;
            }
#endif

            LibraryChannel channel = GetChannel();
            try
            {
                DigitalPlatform.LibraryClient.localhost.EntityInfo[] entities = null;

                long lRet = 0;
                
                if (searchParam.DbNameList == "entity")
                lRet = channel.GetEntities(
                     searchParam.QueryWord,  // strBiblioRecPath
                     searchParam.Start,
                     searchParam.Count,
                     searchParam.FormatList,
                     "zh",
                     out entities,
                     out strError);
                else if (searchParam.DbNameList == "order")
                    lRet = channel.GetOrders(
                         searchParam.QueryWord,  // strBiblioRecPath
                         searchParam.Start,
                         searchParam.Count,
                         searchParam.FormatList,
                         "zh",
                         out entities,
                         out strError);
                else if (searchParam.DbNameList == "issue")
                    lRet = channel.GetIssues(
                         searchParam.QueryWord,  // strBiblioRecPath
                         searchParam.Start,
                         searchParam.Count,
                         searchParam.FormatList,
                         "zh",
                         out entities,
                         out strError);
                else if (searchParam.DbNameList == "comment")
                    lRet = channel.GetComments(
                         searchParam.QueryWord,  // strBiblioRecPath
                         searchParam.Start,
                         searchParam.Count,
                         searchParam.FormatList,
                         "zh",
                         out entities,
                         out strError);
                else
                {
                    strError = "无法识别的 DbNameList 参数值 '"+searchParam.DbNameList+"'";
                    goto ERROR1;
                }

                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0
                        || (lRet == -1 && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound))
                    {
                        // 没有命中
                        ResponseSearch(
searchParam.TaskID,
0,
0,
records,
strError);  // 出错信息大概为 not found。
                        return;
                    }
                    // TODO: 如何返回 channel.ErrorCode ?
                    // 或者把 ErrorCode.ItemDbNotDef 当作没有命中来返回
                    goto ERROR1;
                }

                if (entities == null)
                    entities = new DigitalPlatform.LibraryClient.localhost.EntityInfo[0];

                records.Clear();
                int i = 0;
                foreach (DigitalPlatform.LibraryClient.localhost.EntityInfo entity in entities)
                {
                    DigitalPlatform.Message.Record biblio = new DigitalPlatform.Message.Record();

                    biblio.RecPath = entity.OldRecPath;
                    biblio.Data = entity.OldRecord;
                    biblio.Timestamp = ByteArray.GetHexTimeStampString(entity.OldTimestamp);
                    biblio.Format = "xml";

                    records.Add(biblio);
                    i++;
                }

                ResponseSearch(
                    searchParam.TaskID,
                    records.Count,  // lHitCount,
                    0, // lStart,
                    records,
                    "");
            }
            catch (Exception ex)
            {
                AddErrorLine("GetItemInfo() 出现异常: " + ex.Message);
                strError = "GetItemInfo() 异常：" + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

            this.AddInfoLine("search and response end");
            return;
        ERROR1:
            // 报错
            ResponseSearch(
searchParam.TaskID,
-1,
0,
records,
strError);
        }

        /*
        public string TaskID { get; set; }    // 本次检索的任务 ID。由于一个 Connection 可以用于同时进行若干检索操作，本参数用于区分不同的检索操作
        public string Operation { get; set; }   // 操作名。
        public string DbNameList { get; set; }  // 数据库名列表。一般为 "<全部>"
        public string QueryWord { get; set; }   // 检索词。若为 !getResult 表示不检索、从已有结果集中获取记录
        public string UseList { get; set; }     // 检索途径列表
        public string MatchStyle { get; set; }  // 匹配方式。为 exact/left/right/middle 之一
        public string ResultSetName { get; set; }   // 检索创建的结果集名。空表示为默认结果集
        public string FormatList { get; set; }  // 返回的数据格式列表
        public long MaxResults { get; set; }    // 本次检索最多命中的记录数。-1 表示不限制
        public long Start { get; set; } // 本次获得结果的开始位置
        public long Count { get; set; } // 本次获得结果的个数。 -1表示尽可能多

         * QueryWord --> strItemBarcode
         * UseList --> strConfirmItemRecPath
         * MatchStyle --> strBiblioRecPathExclude
         * () --> nMaxLength 暂时没有使用

        public int GetBiblioSummary(string strItemBarcode,
            string strConfirmItemRecPath,
            string strBiblioRecPathExclude,
            int nMaxLength,
            out string strBiblioRecPath,
            out string strSummary,
            out string strError) 
         * 
         * */
        void GetBiblioSummary(SearchRequest searchParam)
        {
            string strError = "";
            IList<DigitalPlatform.Message.Record> records = new List<DigitalPlatform.Message.Record>();

            LibraryChannel channel = GetChannel();
            try
            {
                string strBiblioRecPath = "";
                string strSummary = "";

                long lRet = channel.GetBiblioSummary(
                    searchParam.QueryWord,
                    searchParam.UseList,
                    searchParam.MatchStyle,
                    out strBiblioRecPath,
                    out strSummary,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0
                        || (lRet == -1 && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound))
                    {
                        // 没有命中
                        ResponseSearch(
searchParam.TaskID,
0,
0,
records,
strError);  // 出错信息大概为 not found。
                        return;
                    }
                    goto ERROR1;
                }

                records.Clear();
                {
                    DigitalPlatform.Message.Record biblio = new DigitalPlatform.Message.Record();
                    biblio.RecPath = strBiblioRecPath;
                    biblio.Data = strSummary;
                    biblio.Format = "";
                    records.Add(biblio);
                }

                ResponseSearch(
                    searchParam.TaskID,
                    records.Count,  // lHitCount,
                    0, // lStart,
                    records,
                    "");
            }
            catch (Exception ex)
            {
                AddErrorLine("GetBiblioSummary() 出现异常: " + ex.Message);
                strError = "GetBiblioSummary() 异常：" + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

            this.AddInfoLine("search and response end");
            return;
        ERROR1:
            // 报错
            ResponseSearch(
searchParam.TaskID,
-1,
0,
records,
strError);
        }

        // searchParam.UseList 里面提供 strBiblioXml 参数，即，前端提供给服务器，希望服务器加工处理的书目XML内容
        void GetBiblioInfo(SearchRequest searchParam)
        {
            string strError = "";
            IList<DigitalPlatform.Message.Record> records = new List<DigitalPlatform.Message.Record>();

            if (string.IsNullOrEmpty(searchParam.FormatList) == true)
            {
                strError = "FormatList 不应为空";
                goto ERROR1;
            }

            LibraryChannel channel = GetChannel();
            try
            {
                string[] formats = searchParam.FormatList.Split(new char[] { ',' });

                string[] results = null;
                // string strRecPath = "";
                byte[] baTimestamp = null;

                long lRet = channel.GetBiblioInfos(
                    searchParam.QueryWord,
                    searchParam.UseList, // strBiblioXml
                    formats,
                    out results,
                    out baTimestamp,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0
                        || (lRet == -1 && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound))
                    {
                        // 没有命中
                        ResponseSearch(
searchParam.TaskID,
0,
0,
records,
strError);  // 出错信息大概为 not found。
                        return;
                    }
                    goto ERROR1;
                }

                if (results == null)
                    results = new string[0];

                records.Clear();
                int i = 0;
                foreach (string result in results)
                {
                    DigitalPlatform.Message.Record biblio = new DigitalPlatform.Message.Record();

                    // 注：可以在 formatlist 中包含 recpath 要求获得记录路径，这时记录路径会返回在对应元素的 Data 成员中
                    biblio.RecPath = "";
                    biblio.Data = result;

                    // 当 strBiblioRecPath 用 @path-list: 方式调用时，formats 格式个数 X 路径个数 = results 中元素数
                    // 要将 formats 均匀分配到 records 元素中
                    if (formats != null && formats.Length > 0)
                        biblio.Format = formats[i % formats.Length];

                    records.Add(biblio);
                    i++;
                }

                ResponseSearch(
                    searchParam.TaskID,
                    records.Count,  // lHitCount,
                    0, // lStart,
                    records,
                    "");
            }
            catch (Exception ex)
            {
                AddErrorLine("GetBiblioInfo() 出现异常: " + ex.Message);
                strError = "GetBiblioInfo() 异常：" + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

            this.AddInfoLine("search and response end");
            return;
        ERROR1:
            // 报错
            ResponseSearch(
searchParam.TaskID,
-1,
0,
records,
strError);
        }

        void GetPatronInfo(SearchRequest searchParam)
        {
            string strError = "";
            IList<DigitalPlatform.Message.Record> records = new List<DigitalPlatform.Message.Record>();

            if (string.IsNullOrEmpty(searchParam.FormatList) == true)
            {
                strError = "FormatList 不应为空";
                goto ERROR1;
            }

            LibraryChannel channel = GetChannel();
            try
            {
                string[] results = null;
                string strRecPath = "";
                byte[] baTimestamp = null;

                long lRet = channel.GetReaderInfo(// null,
                    searchParam.QueryWord,
                    searchParam.FormatList,
                    out results,
                    out strRecPath,
                    out baTimestamp,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0
                        || (lRet == -1 && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound))
                    {
                        // 没有命中
                        ResponseSearch(
searchParam.TaskID,
0,
0,
records,
strError);  // 出错信息大概为 not found。
                        return;
                    }
                    goto ERROR1;
                }

                if (results == null)
                    results = new string[0];

                records.Clear();
                string[] formats = searchParam.FormatList.Split(new char[] { ',' });
                int i = 0;
                foreach (string result in results)
                {
                    DigitalPlatform.Message.Record biblio = new DigitalPlatform.Message.Record();
                    biblio.RecPath = strRecPath;
                    biblio.Data = result;
                    biblio.Format = formats[i];
                    records.Add(biblio);
                    i++;
                }

                ResponseSearch(
                    searchParam.TaskID,
                    records.Count,  // lHitCount,
                    0, // lStart,
                    records,
                    "");
            }
            catch (Exception ex)
            {
                AddErrorLine("GetPatronInfo() 出现异常: " + ex.Message);
                strError = "GetPatronInfo() 异常：" + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

            this.AddInfoLine("search and response end");
            return;
        ERROR1:
            // 报错
            ResponseSearch(
searchParam.TaskID,
-1,
0,
records,
strError);
        }

        #endregion


        // 连接成功后被调用，执行登录功能。重载时要调用 Login(...) 向 server 发送 login 消息
        public override void TriggerLogin()
        {
            LoginAsync(
            this.UserName,
            this.Password,
            "", // string libraryUID,
            "", // string libraryName,
            "" // string propertyList
            )
            .ContinueWith((antecendent) =>
                    {
                        if (antecendent.IsFaulted == true)
                        {
                            // AddErrorLine(GetExceptionText(antecendent.Exception));
                            // 在日志中写入一条错误信息
                            // Program.WriteWindowsLog();
                            return;
                        }
                    });
        }
    }
}
