using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebZ.Models;

namespace WebZ.Controllers
{
    public class HomeController : Controller
    {
        //定义配置信息对象
        //public ApplicationConfiguration StarInfoConfig;
        //public HomeController(IOptions<ApplicationConfiguration> setting)
        //{
        //    StarInfoConfig = setting.Value;
        //}

        public IActionResult Index()
        {
            //ViewData["datadir"] = StarInfoConfig.datadir;
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
    }
}
