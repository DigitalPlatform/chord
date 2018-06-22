//testhu 6 22
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Tools
{
    public class ClassItem
    {
        // 分类号
        public string Class { get; set; }

        // 名称
        public string Name { get; set; }

        // 级别
        public int Level { get; set; }

        // 序号
        public int No { get; set; }

        // 匹配次数
        public int Count { get; set; }

        // 输出信息
        public string Dump()
        {
            return this.Count + "\t"
                + this.Class + "\t"
                + this.Name + "\t"
                + this.Level;
        }
    }
}
