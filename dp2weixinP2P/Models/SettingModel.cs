using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dp2weixinWeb.Models
{
    public class SettingModel
    {
        public string dp2MserverUrl { get; set; }
        public string userName { get; set; }
        public string password { get; set; }

        public string mongoDbConnection { get; set; }

        //mongoDbPrefix
        public string mongoDbPrefix { get; set; }

    }
}