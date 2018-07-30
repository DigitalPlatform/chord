using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;

namespace DigitalPlatform.Common
{
    /// <summary>
    /// chord 项目内所有函数库的共同全局参数
    /// </summary>
    public static class LibraryManager
    {
        public static ILog Log { get; set; }
    }
}
