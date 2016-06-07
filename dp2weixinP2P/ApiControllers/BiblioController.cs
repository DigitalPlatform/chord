using dp2Command.Service;
using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class BiblioController : ApiController
    {
        // GET api/<controller>
        public SearchBiblioResult Get(string libUserName, string from, string word)
        {
            // 取下一页的情况
            if (from == "_N")
            {
                return dp2WeiXinService.Instance.getOnePage(libUserName, word);
            }
            else
            {
                return dp2WeiXinService.Instance.SearchBiblio(libUserName,
                     from,
                     word);
            }
        }

        /// <summary>
        /// 获取书目详细信息
        /// </summary>
        /// <param name="libUserName"></param>
        /// <param name="biblioPath"></param>
        /// <returns></returns>
        public BiblioRecordResult Get(string libUserName, string biblioPath)
        {
            dp2WeiXinService.Instance.WriteLog("走进get() libUserName["+libUserName+"],biblioPath["+biblioPath+"]");
            BiblioRecordResult result = dp2WeiXinService.Instance.GetBiblioDetail(libUserName,
                biblioPath);
            return result;
        }



    }
}