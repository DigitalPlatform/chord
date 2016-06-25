using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace dp2weixinWeb.Models
{
    public class BookEditModel
    {
        public string _libId { get; set; }
        public string _userName { get; set; }
        public string _subject { get; set; }

        public string _returnUrl { get; set; }



        public string id { get; set; }

         [Required]
        public string title { get; set; }

         [Required]
        public string content { get; set; }

        public string publishTime { get; set; } // 2016-6-20 jane 发布时间，服务器消息的时间

         [Required]
        public string creator { get; set; }  //创建消息的工作人员帐户

         [Required]
        public string subject { get; set; } // 栏目

        public string remark { get; set; } //注释
    }
}