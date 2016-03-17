using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2ConsoleToWeiXin
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Instance instance = new Instance())
            {
                while (true)
                {
                    string line = Console.ReadLine();
                    if (instance.ProcessCommand(line) == true)
                        return;
                }
            }
        }
    }
}
