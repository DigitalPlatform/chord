using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;

namespace ilovelibrary
{
    public class NotImplExceptionFilter : ExceptionFilterAttribute  
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            /*
                  var resp = new HttpResponseMessage(HttpStatusCode.ExpectationFailed)
                {
                    ReasonPhrase = context.Exception.Message
                };
                throw new HttpResponseException(resp);
            */
            if (context.Exception is NotImplementedException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.NotImplemented);
            }
            else if (context.Exception is Exception)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.ExpectationFailed)
                {
                    ReasonPhrase = context.Exception.Message
                };
                context.Exception= new HttpResponseException(resp);
            }
        } 
    }
}