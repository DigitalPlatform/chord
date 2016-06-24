using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dp2weixinWeb.Models
{
    public class BookEditModel
    {
        public string libId { get; set; }
        public string userName { get; set; }
        public string subject { get; set; }

        public string returnUrl { get; set; }


        public MessageItem msgItem { get; set; }
    }
}