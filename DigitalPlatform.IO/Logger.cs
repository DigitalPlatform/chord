using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalPlatform.IO
{
    /// <summary>
    /// 输出文本日志
    /// </summary>
    public class Logger : IDisposable
    {
        internal ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        StreamWriter _sw = null;
        string _fileName = "";

        public void Dispose()
        {
            this.CloseFile();
        }

        public void Write(string strLogDir, string strText)
        {
            if (string.IsNullOrEmpty(strLogDir))
                throw new ArgumentException("strLogDir 不应为空", "strLogDir");

            _lock.EnterWriteLock();
            try
            {
                DateTime now = DateTime.Now;
                // 每天一个日志文件
                string strFileName = Path.Combine(strLogDir, "log_" + DateTimeUtil.DateTimeToString8(now) + ".txt");

                if (strFileName.ToLower() != _fileName
                    || _sw == null)
                    OpenFile(strFileName);

                string strTime = now.ToString();
                _sw.WriteLine(strTime + " " + strText);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        void OpenFile(string strFileName)
        {
            CloseFile();

            Stream stream = File.Open(
strFileName,
FileMode.OpenOrCreate,
FileAccess.ReadWrite,
FileShare.ReadWrite);
            stream.Seek(0, SeekOrigin.End);

            _sw = new StreamWriter(stream);
            _sw.AutoFlush = true;

            _fileName = strFileName.ToLower();
        }

        void CloseFile()
        {
            if (_sw != null)
            {
                _sw.Close();
                _sw.Dispose();
                _sw = null;
            }

            _fileName = "";
        }
    }
}
