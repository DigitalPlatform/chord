using System;
using System.Collections;

using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.LibraryClient
{
    // 2017/5/6
    /// <summary>
    /// 检索命中结果的枚举器
    /// </summary>
    public class ResultSetLoader : IEnumerable
    {
        /// <summary>
        /// 提示框事件
        /// </summary>
        public event MessagePromptEventHandler Prompt = null;

        public string ResultSetName { get; set; }

        public LibraryChannel Channel { get; set; }

        // public DigitalPlatform.Stop Stop { get; set; }

        public string FormatList
        {
            get;
            set;
        }

        public string Lang { get; set; }

        // 每批获取最多多少个记录
        // TODO: 可否做到自动伸缩每批尺寸，以便刚好获取到希望的总数
        public long BatchSize { get; set; }

        // 从什么偏移开始获取
        public long Start { get; set; }

        // 结果集中总共有多少条记录。至少取得第一个元素后此值可用
        public long TotalCount { get; set; }

        public ResultSetLoader(LibraryChannel channel,
            // DigitalPlatform.Stop stop,
            string resultsetName,
            string formatList,
            string lang = "zh")
        {
            this.Channel = channel;
            // this.Stop = stop;
            this.ResultSetName = resultsetName;
            this.FormatList = formatList;
            this.Lang = lang;
        }

        public IEnumerator GetEnumerator()
        {
            string strError = "";

            long lHitCount = -1;
            long lStart = this.Start;
            long nPerCount = this.BatchSize == 0 ? -1 : this.BatchSize;
            // nPerCount = 1;  // test
            for (; ; )
            {
                Record[] searchresults = null;

                REDO:
                long lRet = this.Channel.GetSearchResult(
                    // this.Stop,
                    this.ResultSetName, // "default",
                    lStart,
                    nPerCount,
                    this.FormatList,  // "id,xml,timestamp,metadata",
                    string.IsNullOrEmpty(this.Lang) ? "zh" : this.Lang,
                    out searchresults,
                    out strError);
                if (lRet == -1)
                {
                    if (this.Prompt != null)
                    {
                        MessagePromptEventArgs e = new MessagePromptEventArgs();
                        e.MessageText = "获得数据库记录时发生错误： " + strError;
                        e.Actions = "yes,no,cancel";
                        this.Prompt(this, e);
                        if (e.ResultAction == "cancel")
                            throw new ChannelException(Channel.ErrorCode, strError);
                        else if (e.ResultAction == "yes")
                            goto REDO;
                        else
                        {
                            // no 也是抛出异常。因为继续下一批代价太大
                            throw new ChannelException(Channel.ErrorCode, strError);
                        }
                    }
                    else
                        throw new ChannelException(Channel.ErrorCode, strError);
                }

                this.TotalCount = lRet;

                if (lRet == 0)
                    yield break;
                if (searchresults == null)
                {
                    strError = "searchresults == null";
                    throw new Exception(strError);
                }
                if (searchresults.Length == 0)
                {
                    strError = "searchresults.Length == 0";
                    throw new Exception(strError);
                }
                lHitCount = lRet;

                foreach (Record record in searchresults)
                {
                    yield return record;
                }

                lStart += searchresults.Length;
                if (lStart >= lHitCount)
                    yield break;
            }
        }

    }

    /// <summary>
    /// 消息提示事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void MessagePromptEventHandler(object sender,
        MessagePromptEventArgs e);

    /// <summary>
    /// 空闲事件的参数
    /// </summary>
    public class MessagePromptEventArgs : EventArgs
    {
        public string MessageText = ""; // [in] 提示文字
        public string Actions = ""; // [in] 可选的动作。例如 "yes,no,cancel"
        public string ResultAction = "";  // [out] 返回希望采取的动作
    }

    /// <summary>
    /// dp2Library 通讯访问异常
    /// </summary>
    public class ChannelException : Exception
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public ErrorCode ErrorCode = ErrorCode.NoError;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="error"></param>
        /// <param name="strText"></param>
        public ChannelException(ErrorCode error,
            string strText)
            : base(strText)
        {
            this.ErrorCode = error;
        }
    }
}

