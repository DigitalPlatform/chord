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
    }
}
