using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace dp2weixinWeb.Models
{
    public class MessageModel
    {
        [Required]
        [Display(Name = "request")]
        public string RequestMsg { get; set; }

        [Display(Name = "Response")]
        public string ResponseMsg = "";
    }
}