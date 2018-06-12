using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebZ.Models;
using System.Web;
using System.Text;

namespace WebZ.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Demo()
        {
            return View();
        }

            public IActionResult Index()
        {
            /*
            Request.Cookies

            Response.Cookies.Append(
            "alert",
            text,
            new Microsoft.AspNetCore.Http.CookieOptions()
            {
                Path = "/"
            }
        );

            string browseId = "";
            if (Request.Cookies["browseId"] != null)
            {
                HttpCookie aCookie = Request.Cookies["browseId"];
                browseId = aCookie.Value;

                sessionInfo.AddDebugInfo("存在cookies=" + browseId);
            }

            if (string.IsNullOrEmpty(browseId) == true)
            {
                sessionInfo.AddDebugInfo("不存在cookies或值为空");

                string guid = Guid.NewGuid().ToString();
                browseId = "~~" + guid;

                sessionInfo.AddDebugInfo("创建一个临时id=" + browseId);

                // 写到cookies
                HttpCookie aCookie = new HttpCookie("browseId");
                aCookie.Value = browseId;
                aCookie.Expires = DateTime.MaxValue;//设为永久不失效， DateTime.Now.AddDays(1); 
                Response.Cookies.Add(aCookie);

                sessionInfo.AddDebugInfo("将临时id写入cookies=" + browseId);
            }
            //ViewData["datadir"] = StarInfoConfig.datadir;
            */
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        // 列出encoding名列表
        // 需要把gb2312 utf-8等常用的提前
        public static List<string> GetEncodingList(bool bHasMarc8)
        {
            List<string> result = new List<string>();

            EncodingInfo[] infos = Encoding.GetEncodings();
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i].GetEncoding().Equals(Encoding.GetEncoding(936)) == true)
                    result.Insert(0, infos[i].Name);
                else if (infos[i].GetEncoding().Equals(Encoding.UTF8) == true)
                    result.Insert(0, infos[i].Name);
                else
                    result.Add(infos[i].Name);
            }

            if (bHasMarc8 == true)
                result.Add("MARC-8");

            return result;
        }
    }
}
