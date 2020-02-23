using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace dp2Mini
{
    static class Program
    {
        ///// <summary>
        ///// 版本号
        ///// </summary>
        //public static string ClientVersion { get; set; }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            ClientInfo.TypeOfProgram = typeof(Program);

            //if (StringUtil.IsDevelopMode() == false)
            //    ClientInfo.PrepareCatchException();

            //ClientVersion = Assembly.GetAssembly(typeof(Program)).GetName().Version.ToString();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
