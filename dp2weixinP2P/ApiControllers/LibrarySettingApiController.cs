using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        // 编辑一个图书馆 api/LibrarySettingApi/xxx
        public long Put(string id, LibEntity item)
        {

            //Library lib=dp2WeiXinService.Instance.LibManager.GetLibrary(id);
            //LibEntity entity = lib.Entity;
            ///*
            //        id: id,

            //        libName: libName,
            //        capoUserName: capoUserName,
            //        capoContactPhone: capoContactPhone,
            //        area: area,
            //        wxUserName: wxUserName,

            //        wxPassword: wxPassword,
            //        wxContactPhone: wxContactPhone,
            //        comment: comment,
            //        noShareBiblio: getNoShareBiblio(),
            //        //verifyBarcode:getVerifyBarcode(),

            //        searchDbs: searchDbs,
            //        match: match,
            //        state: state,
            //        biblioFilter: biblioFilter,
            //        ReserveScope:ReserveScope,  //预约范围 2020/3/22

            //        NoReserveLocation: NoReserveLocation,  // 不支持预约馆藏地 2020/3/22
            //        IsSendArrivedNotice: IsSendArrivedNotice,   //是否给读者发预约到书通知 2020/3/22
            //        NoViewLocation:NoViewLocation  //不支持显示册记录的馆藏地 2020/3/25             
            // */
            //entity.libName = item.libName;
            //entity.capoUserName = item.capoUserName;
            //entity.capoContactPhone = item.capoContactPhone;
            //entity.area = item.area;
            //entity.wxUserName = item.wxUserName;

            //entity.wxPassword = item.wxPassword;
            //entity.wxContactPhone = item.wxContactPhone;
            //entity.comment = item.comment;
            //entity.noShareBiblio = item.noShareBiblio;

            //entity.searchDbs = item.searchDbs;
            //entity.match = item.match;
            //entity.state = item.state;
            //entity.biblioFilter = item.biblioFilter;
            //entity.ReserveScope = item.ReserveScope;

            //entity.NoReserveLocation = item.NoReserveLocation;
            //entity.IsSendArrivedNotice = item.IsSendArrivedNotice;
            //entity.NoViewLocation = item.NoViewLocation;

            long ret = LibDatabase.Current.Update(id, item);

            if (ret > 0)
            {
                // 更新内存 2016-9-13 jane
                dp2WeiXinService.Instance.LibManager.UpdateLib(id);
            }

            dp2WeiXinService.Instance._areaMgr.SaveLib(item);

            return ret;
        }

        //删除指定的图书馆 api/LibrarySettingApi/xxx
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