using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class LibrarySettingController : ApiController
    {

        // GET api/<controller>
        public IEnumerable<LibEntity> Get()
        {
            List<LibEntity> list = LibDatabase.Current.GetLibsInternal();//"*", 0, -1).Result;
            return list;
        }

        // GET api/<controller>/5
        public LibEntity Get(string id)
        {
            return dp2WeiXinService.Instance.GetLibById(id);
        }

        // POST api/<controller>
        public LibSetResult Post(LibEntity item)
        {
            LibSetResult result = new LibSetResult();
            string strError = "";
            LibEntity outputItem = null;
            int nRet= dp2WeiXinService.Instance.AddLib(item,out outputItem, out strError);// libDb.Add(item);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }
            result.lib = outputItem;

            // 更新内存 2016-9-13 jane
            dp2WeiXinService.Instance.LibManager.AddLib(item);

            dp2WeiXinService.Instance._areaMgr.SaveLib(item);


            return result;

        }

        // PUT api/<controller>/5
        public long Put(string id, LibEntity item)
        {
            long ret = LibDatabase.Current.Update(id, item);

            if (ret > 0)
            {
                // 更新内存 2016-9-13 jane
                dp2WeiXinService.Instance.LibManager.UpdateLib(id);
            }

            dp2WeiXinService.Instance._areaMgr.SaveLib(item);

            return ret;
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        public ApiResult Delete(string id)
        {
            ApiResult result= dp2WeiXinService.Instance.deleteLib(id);
            //if (result.errorCode != -1)
            //{
            //    // 更新内存 2016-9-13 jane
            //    dp2WeiXinService.Instance.LibManager.UpdateLib(id);
            //}

            

            return result;
        }

        [HttpPost]
        public WxUserResult DetectUser(string username, string password)
        {
            WxUserResult result = new WxUserResult();

            string strError = "";
            bool bRet = dp2WeiXinService.Instance.DetectMserverUser(username, password, out strError);
            if (bRet == false)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }

            return result;
        }

        [HttpPost]
        public WxUserResult CreateUser(string username,
            string password,
            string department,
            string mUsername,
            string mPassword)
        {
            WxUserResult result = new WxUserResult();

            string strError = "";
            bool bRet = dp2WeiXinService.Instance.CreateMserverUser(username, password,
                department,
                mUsername,
                mPassword,
                out strError);
            if (bRet == false)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }

            return result;
        }


        [HttpPost]
        public ApiResult GetLibName(string capoUserName)
        {
            WxUserResult result = new WxUserResult();

            string libName = "";
            string strError = "";
            int nRet = dp2WeiXinService.Instance.GetLibName(capoUserName,out libName, out strError);
            if (nRet ==-1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }

            result.info = libName;

            return result;
        }
    }
}