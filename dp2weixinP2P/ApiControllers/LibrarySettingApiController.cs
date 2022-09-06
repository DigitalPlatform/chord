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
    
    public class LibrarySettingApiController : ApiController
    {


        // 获取所有的图书馆 api/LibrarySettingApi
        public IEnumerable<LibEntity> Get()
        {
            List<LibEntity> list = LibDatabase.Current.GetLibsInternal();//"*", 0, -1).Result;
            return list;
        }

        // 根据id获取指定图书馆 api/LibrarySettingApi/xxx
        public LibEntity Get(string id)
        {
            return dp2WeiXinService.Instance.GetLibById(id);
        }


        // 新建一个图书馆 api/LibrarySettingApi
        [HttpPost]
        public LibSetResult CreateLib(string userName, string password, LibEntity item)
        {
            LibSetResult result = new LibSetResult();

            // 检查是否已登录  2022/9/6
            bool bLogin = this.CheckIsLogin(userName, password, out string error);
            if (bLogin == false)
            {
                result.errorInfo = error;
                result.errorCode = -1;
                return result;
            }


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

        // 编辑一个图书馆 api/LibrarySettingApi/xxx
        [HttpPost]
        public ApiResult ChangeLib(string userName, string password,string id, LibEntity item)
        {
            ApiResult result = new ApiResult();

            // 检查是否已登录  2022/9/6
            bool bLogin = this.CheckIsLogin(userName, password, out string error);
            if (bLogin == false)
            {
                result.errorInfo = error;
                result.errorCode = -1;
                return result;
            }

            long ret = LibDatabase.Current.Update(id, item);

            if (ret > 0)
            {
                // 更新内存 2016-9-13 jane
                dp2WeiXinService.Instance.LibManager.UpdateLib(id);
            }

            dp2WeiXinService.Instance._areaMgr.SaveLib(item);

            result.errorCode = ret;
            return result;
        }

        //删除指定的图书馆 api/LibrarySettingApi/xxx
        [HttpDelete]
        public ApiResult Delete(string userName, string password, string id)
        {
            ApiResult result = new ApiResult();

            // 检查是否已登录  2022/9/6
            bool bLogin = this.CheckIsLogin(userName, password, out string error);
            if (bLogin == false)
            {
                result.errorInfo = error;
                result.errorCode=-1;
                return result;
            }



            // 实际删除图书馆
             result= dp2WeiXinService.Instance.deleteLib(id);

            

            return result;
        }

        // 检查是否已登录
        public bool CheckIsLogin(string userName, string password,out string error)
        {
            error = "";
            // 如果参数中传了用户名，优先用用户名和密码进行登录。
            // 这种情况是适用于通过api中直接删除图书馆。
            if (string.IsNullOrEmpty(userName) == false)
            {
                string userName1 = "";
                string password1 = "";
                dp2WeiXinService.Instance.GetSupervisorAccount(out userName1, out password1);
                if (userName != userName1 || password != password1)
                {
                    error = "账户或密码不正确。";
                    return false;
                }
            }
            else
            {

                // 检查session中是否有登录标志，主要适用于我爱图书馆后台删除图书馆，没有再输入用户名和密码的地方。
                // 我们是进入管理界面，先要登录，才能进入。
                if (CheckLoginBySession() == false)
                {
                    error = "尚未登录";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 是否是supervisor登录
        /// </summary>
        /// <returns></returns>
        public  bool CheckLoginBySession()
        {
            if (HttpContext.Current.Session[dp2WeiXinService.C_Session_Supervisor] != null
              && (bool)HttpContext.Current.Session[dp2WeiXinService.C_Session_Supervisor] == true)
            {
                return true;
            }
            return false;
        }


        // 检查mserver帐号是否存在
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

        // 新建一个mserver帐号
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

        // 根据capo_xxx帐户得到图书馆名称
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