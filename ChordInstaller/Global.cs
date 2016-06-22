using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordInstaller
{
    public static class Global
    {
        // 获得 dpeCapo 的程序存储目录
        // 在 64 位操作系统下，获得 Program files (x86)
        // 在 32 位操作系统下，获得 Program Files
        public static string GetProductDirectory(
            string strProduct,
            string strCompany = "digitalplatform")
        {
            string strProgramDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (string.IsNullOrEmpty(strProgramDir) == true)
                strProgramDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            Debug.Assert(string.IsNullOrEmpty(strProgramDir) == false, "");

            return Path.Combine(strProgramDir, strCompany + "\\" + strProduct);
        }
    }
}
