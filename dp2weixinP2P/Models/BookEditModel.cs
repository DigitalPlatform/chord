using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dp2weixinWeb.Models
{
    public class BookEditModel:MessageItem
    {
        public string _libId { get; set; }
        public string _userName { get; set; }
        public string _subject { get; set; }

        public string _returnUrl { get; set; }
    }
}