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
        #region 获取图书馆配置信息

        // 获取所有的图书馆,不区分区域，平级列举 
        public IEnumerable<LibEntity> GetAll()
        {
            List<LibEntity> list = LibDatabase.Current.GetLibsInternal();//"*", 0, -1).Result;
            return list;
        }

        // 根据id获取指定图书馆
        public LibEntity GetByLibId(string libId)
        {
            return dp2WeiXinService.Instance.GetLibById(libId);
        }

        // 获取全部图书馆，按地区分类，用于终端用户选择图书馆
        // 返回Area集合，第一级Area表示地区，第二级Libs/LibModel表示下级的图书馆，是一个集合
        public IEnumerable<Area> GetAreaLib()
        {

            SessionInfo sessionInfo = (SessionInfo)HttpContext.Current.Session[WeiXinConst.C_Session_sessioninfo];
            //if (sessionInfo.ActiveUser == null)
            //{
            //    dp2WeiXinService.Instance.WriteDebug("提交流通API时，发现session失效了。");
            //}



            // 获取可访问的图书馆
            //List<Library> avaiblelibList = dp2WeiXinService.Instance.LibManager.GetLibraryByIds(sessionInfo.libIds);


            // 获取该微信帐户绑定了哪些图书馆帐户
            List<WxUserItem> list = WxUserDatabase.Current.Get(sessionInfo.WeixinId, null, -1);

            // 可显示的区域
            List<Area> areaList = new List<Area>();
            // 从所有区域中查找
            foreach (Area area in dp2WeiXinService.Instance._areaMgr._areas)
            {
                List<LibModel> libList = new List<LibModel>();
                foreach (LibModel lib in area.libs)
                {
                    lib.Checked = "";
                    lib.bindFlag = "";

                    // 如果是到期的图书馆，不显示出来
                    Library thisLib = dp2WeiXinService.Instance.LibManager.GetLibrary(lib.libId);//.GetLibById(lib.libId);
                    if (thisLib != null && thisLib.Entity.state == "到期")
                    {
                        continue;
                    }

                    ////如果不在可访问范围，不显示
                    //if (thisLib != null && avaiblelibList.IndexOf(thisLib) == -1)
                    //{
                    //    continue;
                    //}

                    // 如果从mongodb库没有找到图书馆，不显示
                    // 有可能是mongodb库删除，但配置文件还没有删除
                    if (thisLib == null)
                    {
                        dp2WeiXinService.Instance.WriteDebug("选择图书馆时，根据[" + lib.libId + "]未找到对应的图书馆");
                        continue;
                    }

                    // 加到显示列表
                    libList.Add(lib);

                    // 检查微信用户是否绑定了这个图书馆
                    WxUserItem tempUser = null;
                    //if (this.CheckIsBind(list, lib, out tempUser) == true)  //libs.Contains(lib.libId)
                    //{
                    //    if (tempUser.userName != WxUserDatabase.C_Public)
                    //        lib.bindFlag = " * ";
                    //}

                    // 当前绑定帐户的图书馆显示为勾中状态
                    if (sessionInfo.ActiveUser != null)
                    {
                        if (lib.libId == sessionInfo.ActiveUser.libId
                            && lib.libraryCode == sessionInfo.ActiveUser.bindLibraryCode)
                        {
                            lib.Checked = " checked ";
                        }
                    }
                }

                // 只有当有下级图书馆时，才显示地区
                if (libList.Count > 0)
                {
                    Area newArea = new Area();
                    newArea.name = area.name;
                    newArea.libs = libList;
                    areaList.Add(newArea);
                }

            }

            return areaList;
        }

        #endregion


        #region 增，删，改 图书馆

        // 新建一个图书馆
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

        // 修改一个图书馆
        [HttpPost]
        public ApiResult ChangeLib(string userName, string password,string libId, LibEntity item)
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

            long ret = LibDatabase.Current.Update(libId, item);

            if (ret > 0)
            {
                // 更新内存 2016-9-13 jane
                dp2WeiXinService.Instance.LibManager.UpdateLib(libId);
            }

            dp2WeiXinService.Instance._areaMgr.SaveLib(item);

            result.errorCode = ret;
            return result;
        }

        //删除指定的图书馆 
        [HttpDelete]
        public ApiResult DeleteLib(string userName, string password, string libId)
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
             result= dp2WeiXinService.Instance.deleteLib(libId);

            
            return result;
        }

        // 内部函数，检查是否已登录
        private bool CheckIsLogin(string userName, string password,out string error)
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
                // 我们后台是进入管理界面前，先要登录，才能进入。登录后将登录信息存在了session里，再创建图书馆时，就不需要帐号密码信息了。
                if (HttpContext.Current.Session[dp2WeiXinService.C_Session_Supervisor] == null
                    || (bool)HttpContext.Current.Session[dp2WeiXinService.C_Session_Supervisor] == false)
                {
                    error = "尚未登录";
                    return false;
                }

            }

            return true;
        }

        #endregion


        #region mserver帐号相关

        // 检查mserver帐号是否存在
        [HttpPost]
        public WxUserResult CheckMserverUser(string userName, string password)
        {
            WxUserResult result = new WxUserResult();

            string strError = "";
            bool bRet = dp2WeiXinService.Instance.DetectMserverUser(userName, password, out strError);
            if (bRet == false)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }

            return result;
        }

        // 新建一个mserver帐号
        [HttpPost]
        public WxUserResult CreateMserverUser(string userName,
            string password,
            string department,
            string supervisorUsername,
            string supervisorPassword)
        {
            WxUserResult result = new WxUserResult();

            string strError = "";
            bool bRet = dp2WeiXinService.Instance.CreateMserverUser(userName, password,
                department,
                supervisorUsername,
                supervisorPassword,
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
        public ApiResult GetLibNameByMserverUser(string capoUserName)
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

        #endregion
    }
}