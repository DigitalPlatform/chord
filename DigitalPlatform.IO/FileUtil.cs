using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.IO
{
    public static class FileUtil
    {
        // 写入文本文件。
        // 如果文件不存在, 会自动创建新文件
        // 如果文件已经存在，则追加在尾部。
        public static void WriteText(string strFileName,
            string strText)
        {
            using (FileStream file = File.Open(
strFileName,
FileMode.Append,	// append
FileAccess.Write,
FileShare.ReadWrite))
            using (StreamWriter sw = new StreamWriter(file,
                System.Text.Encoding.UTF8))
            {
                sw.Write(strText);
            }
        }

        // 能自动识别文件内容的编码方式的读入文本文件内容模块
        // parameters:
        //      lMaxLength  装入的最大长度。如果超过，则超过的部分不装入。如果为-1，表示不限制装入长度
        // return:
        //      -1  出错 strError中有返回值
        //      0   文件不存在 strError中有返回值
        //      1   文件存在
        //      2   读入的内容不是全部
        public static int ReadTextFileContent(string strFilePath,
            long lMaxLength,
            out string strContent,
            out Encoding encoding,
            out string strError)
        {
            strError = "";
            strContent = "";
            encoding = null;

            if (File.Exists(strFilePath) == false)
            {
                strError = "文件 '" + strFilePath + "' 不存在";
                return 0;
            }

            encoding = FileUtil.DetectTextFileEncoding(strFilePath);

            try
            {
                bool bExceed = false;

                using (FileStream file = File.Open(
        strFilePath,
        FileMode.Open,
        FileAccess.Read,
        FileShare.ReadWrite))
                // TODO: 这里的自动探索文件编码方式功能不正确，
                // 需要专门编写一个函数来探测文本文件的编码方式
                // 目前只能用UTF-8编码方式
                using (StreamReader sr = new StreamReader(file, encoding))
                {
                    if (lMaxLength == -1)
                    {
                        strContent = sr.ReadToEnd();
                    }
                    else
                    {
                        long lLoadedLength = 0;
                        StringBuilder temp = new StringBuilder(4096);
                        for (; ; )
                        {
                            string strLine = sr.ReadLine();
                            if (strLine == null)
                                break;
                            if (lLoadedLength + strLine.Length > lMaxLength)
                            {
                                strLine = strLine.Substring(0, (int)(lMaxLength - lLoadedLength));
                                temp.Append(strLine + " ...");
                                bExceed = true;
                                break;
                            }

                            temp.Append(strLine + "\r\n");
                            lLoadedLength += strLine.Length + 2;
                            if (lLoadedLength > lMaxLength)
                            {
                                temp.Append(strLine + " ...");
                                bExceed = true;
                                break;
                            }
                        }
                        strContent = temp.ToString();
                    }
                    /*
                sr.Close();
                sr = null;
                     * */
                }

                if (bExceed == true)
                    return 2;
            }
            catch (Exception ex)
            {
                strError = "打开或读入文件 '" + strFilePath + "' 时出错: " + ex.Message;
                return -1;
            }

            return 1;
        }


        // 如果未能探测出来，则当作 936
        public static Encoding DetectTextFileEncoding(string strFilename)
        {
            Encoding encoding = DetectTextFileEncoding(strFilename, null);
            if (encoding == null)
                return Encoding.GetEncoding(936);    // default

            return encoding;
        }

        // 检测文本文件的encoding
        /*
UTF-8: EF BB BF 
UTF-16 big-endian byte order: FE FF 
UTF-16 little-endian byte order: FF FE 
UTF-32 big-endian byte order: 00 00 FE FF 
UTF-32 little-endian byte order: FF FE 00 00 
         * */
        public static Encoding DetectTextFileEncoding(string strFilename,
            Encoding default_encoding)
        {
            byte[] buffer = new byte[4];

            try
            {
                using (FileStream file = File.Open(
        strFilename,
        FileMode.Open,
        FileAccess.Read,
        FileShare.ReadWrite))
                {
                    if (file.Length >= 2)
                    {
                        file.Read(buffer, 0, 2);    // 1, 2 BUG

                        if (buffer[0] == 0xff && buffer[1] == 0xfe)
                        {
                            return Encoding.Unicode;    // little-endian
                        }

                        if (buffer[0] == 0xfe && buffer[1] == 0xff)
                        {
                            return Encoding.BigEndianUnicode;
                        }
                    }

                    if (file.Length >= 3)
                    {
                        file.Read(buffer, 2, 1);
                        if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                        {
                            return Encoding.UTF8;
                        }

                    }

                    if (file.Length >= 4)
                    {
                        file.Read(buffer, 3, 1);

                        // UTF-32 big-endian byte order: 00 00 FE FF 
                        // UTF-32 little-endian byte order: FF FE 00 00 

                        if (buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0xfe && buffer[3] == 0xff)
                        {
                            return Encoding.UTF32;    // little-endian
                        }

                        if (buffer[0] == 0xff && buffer[1] == 0xfe && buffer[2] == 0x00 && buffer[3] == 0x00)
                        {
                            return Encoding.GetEncoding(65006);    // UTF-32 big-endian
                        }
                    }
                }
            }
            catch
            {
            }

            return default_encoding;    // default
        }



        /// <summary>
        /// 日志锁
        /// </summary>
        static object logSyncRoot = new object();

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="strText"></param>
        public static void WriteLog(string strFilename, string strText,string strEventLogSource)
        {
            try
            {
                //lock (logSyncRoot)
                {
                    string strTime = DateTime.Now.ToString();
                    FileUtil.WriteText(strFilename, strTime + " " + strText + "\r\n");
                }
            }
            catch (Exception ex)
            {
                EventLog Log = new EventLog();
                Log.Source = strEventLogSource;
                Log.WriteEntry("因为原本要写入日志文件的操作发生异常， 所以不得不改为写入 Windows 日志(见后一条)。异常信息如下：'" + ExceptionUtil.GetDebugText(ex) + "'", EventLogEntryType.Error);
                Log.WriteEntry(strText, EventLogEntryType.Error);
            }
        }
    }
}
