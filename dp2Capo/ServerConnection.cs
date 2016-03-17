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


        // 
        // 当 server 发来检索请求的时候被调用。重载的时候要进行检索，并调用 Response 把检索结果发送给 server
        public override void OnSearchBiblioRecieved(SearchRequest param)
        {
            // 单独给一个线程来执行
            Task.Factory.StartNew(() => SearchAndResponse(param));

        }

        // TODO: 本函数最好放在一个工作线程内执行
        // Form Close 的时候要及时中断工作线程
        void SearchAndResponse(SearchRequest searchParam)
        {
            if (searchParam.Operation == "getPatronInfo")
            {
                GetPatronInfo(searchParam);
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
