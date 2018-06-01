//test 1 -jane
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Tools
{
    class BasicNo
    {
        public string basicno;//简表分类号
        public int count;//被命中次数
        public   BasicNo(string basicno, int basiccount)
        {
            this.count = basiccount;
            this.basicno = basicno;
        }
        public void setBasicCount(int basiccount)
        {
            this.count = basiccount;
        }
        public void setBasicno(string  basicno)
        {
            this.basicno = basicno;
        }
        public string getBasicNo()
        {
            return this.basicno;
        }
        public int getCount()
        {
            return this.count;
        }
    }
}
