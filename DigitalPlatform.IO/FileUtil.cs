using System;
using System.Collections.Generic;
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
    }
}
