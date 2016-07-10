using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Message
{
    /// <summary>
    /// 将 WebData 拆分为若干个分块的拆分器
    /// </summary>
    public class WebDataSplitter : IEnumerable
    {
        public int ChunkSize { get; set; }

        public string TransferEncoding { get; set; }

        // 待拆分的 WebData
        public WebData WebData { get; set; }

        // 当前是否处于第一个分块
        bool _firstOne = true;
        public bool FirstOne
        {
            get
            {
                return _firstOne;
            }
        }

        // 当前是否已经处于最后一个分块
        bool _lastOne = false;
        public bool LastOne
        {
            get
            {
                return _lastOne;
            }
        }

        public static string RemoveFrom(StringBuilder text, int length)
        {
            if (length > text.Length)
                length = text.Length;
            string result = text.ToString(0, length);
            text.Remove(0, length);
            return result;
        }

        public IEnumerator GetEnumerator()
        {
            if (this.ChunkSize == 0)
                throw new Exception("尚未设置 ChunkSize");
            if (this.WebData == null)
                throw new Exception("尚未设置 WebData");

            if (TransferEncoding == "text" || TransferEncoding == "base64")
            {
                StringBuilder content = new StringBuilder(this.WebData.Text);
                for (int i = 0; ; i++)
                {
                    WebData current = new WebData();
                    if (i == 0)
                    {
                        current.Headers = this.WebData.Headers;
                        if (this.WebData.Headers.Length < this.ChunkSize)
                            current.Text = RemoveFrom(content, this.ChunkSize - this.WebData.Headers.Length);
                    }
                    else
                    {
                        current.Headers = null;
                        current.Text = RemoveFrom(content, this.ChunkSize);
                    }

                    _lastOne = content.Length == 0;

                    yield return current;
                    if (content.Length == 0)
                        yield break;

                    _firstOne = false;
                }
            }
            else if (TransferEncoding == "content")
            {
                byte[] content = this.WebData.Content;
#if VERIFY_CHUNK
                    int content_send = 0;
#endif
                for (int i = 0; ; i++)
                {
                    WebData current = new WebData();
                    if (i == 0)
                    {
                        current.Headers = this.WebData.Headers;
                        if (this.WebData.Headers.Length < this.ChunkSize)
                            current.Content = ByteArray.Remove(ref content, this.ChunkSize - this.WebData.Headers.Length);
                    }
                    else
                    {
                        current.Headers = null;
                        current.Content = ByteArray.Remove(ref content, this.ChunkSize);
                    }

#if VERIFY_CHUNK
                        current.Offset = content_send;

                        //
                        content_send += current.Content.Length;
#endif
                    _lastOne = content.Length == 0;

                    yield return current;

                    if (content.Length == 0)
                        yield break;

                    _firstOne = false;
                }
            }
            else
                throw new Exception("无法识别的 TransferEncoding '"+this.TransferEncoding+"'");
        }
    }
}
