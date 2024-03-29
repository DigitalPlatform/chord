﻿using DigitalPlatform.Message;
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
    public class PatronApiController : ApiController  //BaseApiController//
    {
        // 参数值常量
        public const string C_format_summary = "summary";
        public const string C_format_verifyBarcode = "verifyBarcode";


        // 获取读者信息
        // libId：图书馆id
        // userName：馆员帐户名，
        // 如果传了馆员帐号，则用馆员身份获取读者信息。
        // 如果未传馆员帐户，则用读者自己身份获取信息。
        // patronBarcode：读者证条码号
        public SetReaderInfoResult GetPatron(string libId,
           string userName,
           string patronBarcode)
        {
            SetReaderInfoResult result = new SetReaderInfoResult();

            string strError = "";
            string recPath = "";
            string timestamp = "";

            // 2022/07/21 整理接口时改进，
            // 如果参数中传了馆员帐号，则用馆员帐户。
            // 如果参数中没有传递馆员帐户，则用读者帐号。
            LoginInfo loginInfo = null;
            if(string.IsNullOrEmpty(userName)==false)
                loginInfo = new LoginInfo(userName,false);
            else
                loginInfo = new LoginInfo(patronBarcode,true);

            string strXml = "";
            int nRet = dp2WeiXinService.Instance.GetPatronXml(libId,
                loginInfo,
                patronBarcode,
                "advancexml,timestamp",
                out recPath,
                out timestamp,
                out strXml,
                out strError);
            if (nRet == -1 || nRet == 0)  // 出错或不存在的情况
            {
                result.errorCode = -1;
                result.errorInfo = strError;
                return result;  // 2022/8/9 原来少了这条代码，导致后面报未将对象引用到对象实例上。
            }

            result.recPath = recPath;
            result.timestamp = timestamp;

            int showPhoto = 0;
            Patron patron = dp2WeiXinService.Instance.ParsePatronXml(libId,
                strXml,
                recPath,
                showPhoto,
                true);

            result.obj = patron;

            return result;
        }

        /// <summary>
        /// 设置读者信息
        /// </summary>
        /// <param name="libId">图书馆id，找本地mongodb的id</param>
        /// <param name="userName">工作人员帐号名</param>
        /// <param name="opeType">操作类型：
        /// 1 register:读者自助注册，转换为new
        /// 2 reRegister:读者重新提交注册，转换为change,当状态是"待审核"，读者类型为空时才能保存成功
        /// 3 reviewPass:馆员审核通过，转换为change
        /// 4 reviewNopass:馆员审核不通过,转换为changestate
        /// 5 reviewNopassDel:馆员审核不通过+删除，转换为delete
        /// 6 changeByPatron:读者自己修改信息，转换为change,注意只允许读者修改几个字段，其它字段传过来为null，表示不修改
        /// 7 deleteByPatron:读者自己删除信息，转换为delete,只有当状态为"待审核"或者"审核不通过"时，读者才能自己删除记录
        /// 8 newByWorker:馆员登记读者，转换为new
        /// 9 changeByWorker:馆员修改读者信息，转换为change
        /// </param>
        /// <param name="recPath">读者记录路径</param>
        /// <param name="timestamp">读者记录时间戳</param>
        /// <param name="weixinId">读者weixinId，当opeType=register/reRegister时有值</param>
        /// <param name="patron">传递的读者信息对象</param>
        /// <returns></returns>
        [HttpPost]
        public SetReaderInfoResult SetPatron(string libId,
            string userName,
            string opeType,
            string recPath,
            string timestamp,
            string weixinId,
            SimplePatron patron)
        {
            //dp2WeiXinService.Instance.WriteErrorLog("***0***");

            SetReaderInfoResult result = new SetReaderInfoResult();
            string strError="";
            int nRet = 0;

            // 读者自助注册时使用代理账号capo 2020/1/21
            LoginInfo loginInfo = new LoginInfo("", false);
            if (string.IsNullOrEmpty(userName) == false)
            {
                loginInfo = new LoginInfo(userName, false);
            }

            // 当审核通过时，检查一下是否存在相同的姓名
            if (opeType == "reviewPass")
            {
                dp2WeiXinService.Instance.WriteErrorLog("***1***");
                List<Patron> patronList = new List<Patron>();
                nRet = dp2WeiXinService.Instance.GetPatronsByName(loginInfo,
                    libId,
                    patron.libraryCode,
                    patron.name,
                   out patronList,
                   out strError);
                if (nRet == -1)
                {
                    result.errorCode = -1;
                    result.errorInfo = strError;
                    return result;
                }
                if (patronList.Count > 0)
                {
                    dp2WeiXinService.Instance.WriteErrorLog("***3***"+patronList.Count);
                    string names = "";
                    foreach (Patron temp in patronList)
                    {
                        if (names != "")
                            names += ",";
                        names += temp.barcode;
                    }
                    result.errorCode = -2;
                    result.info = patronList.Count.ToString();
                    result.errorInfo = names;
                    return result;
                }

            }
            else
            {
                //dp2WeiXinService.Instance.WriteErrorLog("***2***");
            }

            // 读者确实有姓名重复，再次提交
            if (opeType == "reviewPass-noCheckName")
                opeType = "reviewPass";

            // 2021/8/5 将读者注册，修改注册信息，删除这一类改为使用一个独立帐户wx_registerByPatron。
            // 2021/8/5 将读者修改手机号，改为使用一个独立帐户wx_changeTelByPatron
            if (opeType == dp2WeiXinService.C_OpeType_register
                || opeType == dp2WeiXinService.C_OpeType_reRegister
                || opeType == dp2WeiXinService.C_OpeType_deleteByPatron)
            {
                // 使用约定的wx_registerByPatron
                loginInfo = new LoginInfo("wx_registerByPatron", false);

                // 新增读者时，要传入weixinid
                if (string.IsNullOrEmpty(weixinId) == true)
                {
                    result.errorCode = -1;
                    result.errorInfo = "缺少前端id参数（weixinId)";
                    return result;
                }
            }
            else if (opeType == dp2WeiXinService.C_OpeType_changeByPatron)
            {
                // 使用约定的wx_changeTelByPatron
                loginInfo = new LoginInfo("wx_changeTelByPatron", false);
            }

            string outputRecPath = "";
            string outputTimestamp = "";

            WxUserItem userItem = null;
            nRet = dp2WeiXinService.Instance.SetReaderInfo(loginInfo,
                libId,
                userName,
                opeType,
                recPath,
                timestamp,
                weixinId,
                patron,
                out outputRecPath,
                out outputTimestamp,
                out userItem,
                out  strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
                return result;
            }
            
            // 返回读者路径和时间戳
            result.recPath = outputRecPath;
            result.timestamp = outputTimestamp;

            // 如果是读者自助注册过来的，需要把注册的这个帐户设置为当前帐户
            // reregister不需要设置活动帐户，因为当前帐户就是他本身。
            if (opeType== "register")
            {
                result.info = userItem.readerBarcode;

                nRet = ApiHelper.ActiveUser(userItem, out strError);
                if (nRet == -1)
                {
                    result.errorInfo = strError;
                    return result;
                }
            }
            return result;
        }

        /// <summary>
        /// 注册读者时，发送手机验证码
        /// </summary>
        /// <param name="libId">图书馆id</param>
        /// <param name="tel">手机号</param>
        /// <returns></returns>
        public GetVerifyCodeResult GetVerifyCode(string libId, 
            string phone)
        {
            GetVerifyCodeResult result = new GetVerifyCodeResult();
            //result.verifyCode = "7";

            string error = "";
            int nRet = dp2WeiXinService.Instance.GetVerifyCode(libId,
                phone,
                out string verifyCode,
                out error);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = error;
            }

            result.verifyCode = verifyCode;


            return result;
        }


        /// <summary>
        /// 登记读者时，自动生成一个增量的证条码号
        /// </summary>
        /// <param name="libId"></param>
        /// <param name="libraryCode"></param>
        /// <returns></returns>
        [HttpGet]
        public ApiResult IncrementPatronBarcode(string libId,
            string libraryCode)
        {
            ApiResult result = new ApiResult();
            LibModel libCfg = dp2WeiXinService.Instance._areaMgr.GetLibCfg(
                libId,
                libraryCode);
            if (libCfg != null)
            {
                string patronBarcodeTail = libCfg.patronBarcodeTail;

                if (string.IsNullOrEmpty(patronBarcodeTail) == true)
                {
                    result.errorCode = -1;
                    result.errorInfo = "尚未配置证条码尾号";
                }
                else
                {

                    dp2WeiXinService.IncrementBarcode(ref patronBarcodeTail);
                    libCfg.patronBarcodeTail = patronBarcodeTail;

                    // 保存到xml
                    dp2WeiXinService.Instance._areaMgr.Save2Xml();

                    result.info = patronBarcodeTail;
                }
            }
            else
            {
                result.errorCode = -1;
                result.errorInfo = "未找到lib=["+libId+"] libraryCode=["+libraryCode+"]对应的配置。";
            }

            return result;
        }

    }


}
