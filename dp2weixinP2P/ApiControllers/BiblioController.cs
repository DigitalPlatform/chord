using dp2Command.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dp2weixinP2P.ApiControllers
{
    public class BiblioController : ApiController
    {
        // GET api/<controller>
        public SearchBiblioResult Get(string libUserName, string from, string word)
        {
            SearchBiblioResult result = dp2CmdService2.Instance.SearchBiblio(libUserName,
                 from,
                 word);

            return result;
        }



    }
}