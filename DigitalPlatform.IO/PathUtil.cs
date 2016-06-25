using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.IO
{
    public static class PathUtil
    {
        // 如果目录不存在则创建之
        // return:
        //      false   已经存在
        //      true    刚刚新创建
        public static bool CreateDirIfNeed(string strDir)
        {
            DirectoryInfo di = new DirectoryInfo(strDir);
            if (di.Exists == false)
            {
                di.Create();
                return true;
            }

            return false;
        }

        public static void DeleteDirectory(string strDirPath)
        {
            try
            {
                Directory.Delete(strDirPath, true);
            }
            catch (DirectoryNotFoundException)
            {
                // 不存在就算了
            }
        }

        // 拷贝目录
        // 遇到有同名文件会覆盖
        public static int CopyDirectory(string strSourceDir,
            string strTargetDir,
            bool bDeleteTargetBeforeCopy,
            out string strError)
        {
            strError = "";

            try
            {
                DirectoryInfo di = new DirectoryInfo(strSourceDir);

                if (di.Exists == false)
                {
                    strError = "源目录 '" + strSourceDir + "' 不存在...";
                    return -1;
                }

                if (bDeleteTargetBeforeCopy == true)
                {
                    if (Directory.Exists(strTargetDir) == true)
                        Directory.Delete(strTargetDir, true);
                }

                CreateDirIfNeed(strTargetDir);

                FileSystemInfo[] subs = di.GetFileSystemInfos();

                for (int i = 0; i < subs.Length; i++)
                {
                    // 复制目录
                    if ((subs[i].Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        int nRet = CopyDirectory(subs[i].FullName,
                            Path.Combine(strTargetDir, subs[i].Name),
                            bDeleteTargetBeforeCopy,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        continue;
                    }
                    // 复制文件
                    File.Copy(subs[i].FullName,
                        Path.Combine(strTargetDir, subs[i].Name),
                        true);
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            return 0;
        }
    }
}
