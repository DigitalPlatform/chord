using DigitalPlatform.IO;
using dp2weixin.service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace dp2weixinWeb.ApiControllers
{
    public class LibMessageController : ApiController
    {
        public ApiResult GetTemplate(string group,
            string libId,
            string subject)
        {
            ApiResult result = new ApiResult();
            //result.info = "test";


            LibItem lib = LibDatabase.Current.GetLibById(libId);
            if (lib == null)
            {
                result.errorCode = -1;
                result.errorInfo = "未找到id为'"+libId+"'的图书馆";
                return result;
            }


            string file = dp2WeiXinService.Instance.weiXinDataDir 
                + "/lib/" + lib.capoUserName 
                + "/homePage/" 
                + subject+".html";

                // 文件存在，取出文件 的内容
            string text = "";
            string strError = "";
            if (System.IO.File.Exists(file) == true)
            {
                Encoding encoding;
                // 能自动识别文件内容的编码方式的读入文本文件内容模块
                // parameters:
                //      lMaxLength  装入的最大长度。如果超过，则超过的部分不装入。如果为-1，表示不限制装入长度
                // return:
                //      -1  出错 strError中有返回值
                //      0   文件不存在 strError中有返回值
                //      1   文件存在
                //      2   读入的内容不是全部
                int nRet = FileUtil.ReadTextFileContent(file,
                    -1,
                    out text,
                    out encoding,
                    out strError);
                if (nRet == -1 || nRet == 0)
                {
                    goto ERROR1;
                }
                if (nRet == 2)
                {
                    strError="FileUtil.ReadTextFileContent() error";
                    goto ERROR1;
                }

                result.info = text;
            }


            return result;

        ERROR1:
            result.errorInfo = strError;
        result.errorCode = -1;
        return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libId"></param>
        /// <param name="msgId"></param>
        /// <param name="style">browse/full</param>
        /// <returns></returns>
        public SubjectResult GetSubject(string weixinId, 
            string group,
            string libId)
        {
            SubjectResult result = new SubjectResult();

            if (group != dp2WeiXinService.C_GroupName_HomePage
    && group != dp2WeiXinService.C_GroupName_Book)
            {
                result.errorInfo = "不支持的群" + group;
                result.errorCode = -1;
                return result;
            }

            string strError = "";


            // 检查是否绑定工作人员以及权限，前端根据返回的工作人员账号判断是否可以进入编辑界面
            string userName = "";
            if (string.IsNullOrEmpty(weixinId) == false)
            {
                // 查找当前微信用户绑定的工作人员账号
                WxUserItem user = WxUserDatabase.Current.GetWorker(weixinId, libId);
                // todo 后面可以放开对读者的权限
                if (user != null)
                {
                    // 检索是否有权限 _wx_setHomePage
                    string needRight = "";
                    if (group == dp2WeiXinService.C_GroupName_HomePage)
                        needRight = dp2WeiXinService.C_Right_SetHomePage;
                    else if (group == dp2WeiXinService.C_GroupName_Book)
                        needRight = dp2WeiXinService.C_Right_SetBook;

                    LibItem lib = LibDatabase.Current.GetLibById(libId);
                    if (lib == null)
                    {
                        strError = "未找到id为[" + libId + "]的图书馆定义。";
                        goto ERROR1;
                    }

                    int nHasRights = dp2WeiXinService.Instance.CheckRights(lib.capoUserName,
                        user.userName,
                        needRight,
                        out strError);
                    if (nHasRights == -1)
                    {
                        goto ERROR1;
                    }
                    if (nHasRights == 1)
                    {
                        userName = user.userName;
                    }
                    else
                    {
                        userName = "";
                    }
                }
            }
            result.userName = userName;

            // 获取指定图书的栏目
            List<SubjectItem> list = null;
            int nRet = dp2WeiXinService.Instance.GetSubject(libId, group,
                out list, out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }

            // 如果是图书馆主页，需要加一些默认模板
            if (group == dp2WeiXinService.C_GroupName_HomePage)
            {
                LibItem lib = LibDatabase.Current.GetLibById(libId);
                string dir = dp2WeiXinService.Instance.weiXinDataDir + "/lib/" + lib.capoUserName+"/homePage";
                if (Directory.Exists(dir) == true)
                {
                    string[] files = Directory.GetFiles(dir, "*.html");
                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        bool bExist = this.checkContaint(list, fileName);
                        if (bExist == false)
                        {
                            SubjectItem subject = new SubjectItem();
                            subject.name = fileName;
                            subject.count = 0;
                            list.Add(subject);
                        }
                    }
                }
            }


            result.list = list;
            result.errorCode = nRet;
            result.errorInfo = strError;

            return result;

        ERROR1:
            result.errorCode = -1;
            result.errorInfo = strError;
            return result;
        }

        /// <summary>
        /// 检查一个栏目是否已存在
        /// </summary>
        /// <param name="list"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        private bool checkContaint(List<SubjectItem> list, string subject)
        {
            foreach (SubjectItem item in list)
            {
                if (item.name == subject)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="weixinId"></param>
        /// <param name="libId"></param>
        /// <param name="msgId"></param>
        /// <param name="style">browse/full</param>
        /// <returns></returns>
        public MessageResult GetMessage(string weixinId, 
            string group,
            string libId, 
            string msgId,
            string subject,
            string style)
        {
            MessageResult result = new MessageResult();

            if (group != dp2WeiXinService.C_GroupName_Bb
                && group != dp2WeiXinService.C_GroupName_Book
                && group != dp2WeiXinService.C_GroupName_HomePage)
            {
                result.errorInfo = "不支持的群" + group;
                result.errorCode = -1;
                return result;
            }

            string strError="";

            // 检查下有无绑定工作人员账号
            result.userName = "";
            string userName = "";
            if (string.IsNullOrEmpty(weixinId) == false)
            {
                // 查找当前微信用户绑定的工作人员账号
                WxUserItem user = WxUserDatabase.Current.GetWorker(weixinId, libId);
                if (user != null)
                {
                    // 检索是否有权限 _wx_setbbj
                    string needRight = "";
                    if (group == dp2WeiXinService.C_GroupName_Bb)
                        needRight = dp2WeiXinService.C_Right_SetBb;
                    else if (group == dp2WeiXinService.C_GroupName_Book)
                        needRight = dp2WeiXinService.C_Right_SetBook;
                    else if (group == dp2WeiXinService.C_GroupName_HomePage)
                        needRight = dp2WeiXinService.C_Right_SetHomePage;

                    LibItem lib = LibDatabase.Current.GetLibById(libId);
                    if (lib == null)
                    {
                        result.errorInfo = "未找到id为[" + libId + "]的图书馆定义。";
                        result.errorCode = -1;
                        return result;
                    }

                    int nHasRights = dp2WeiXinService.Instance.CheckRights(lib.capoUserName,
                        user.userName,
                        needRight,
                        out strError);
                    if (nHasRights == -1)
                    {
                        result.errorInfo = strError;
                        result.errorCode = -1;
                        return result;
                    }
                    if (nHasRights == 1)
                    {
                        userName = user.userName;
                    }
                    else
                    {
                        userName = "";
                    }
                }
            }
            result.userName = userName;

            List<MessageItem> list = null;
            int nRet = dp2WeiXinService.Instance.GetMessage(group,
                libId,
                msgId, 
                subject,
                style,
                out list,
                out strError);
            if (nRet == -1)
            {
                result.errorCode = -1;
                result.errorInfo = strError;
            }
            result.items = list;
            
            result.errorCode = nRet;
            result.errorInfo = strError;

            return result;
        }

        // POST api/<controller>
        public MessageResult Post(string group, string libId, MessageItem item)
        {
            // 服务器会自动产生id
            //item.id = Guid.NewGuid().ToString();`'
            return dp2WeiXinService.Instance.CoverMessage(group, libId, item, "create");
        }

        // PUT api/<controller>/5
        public MessageResult Put(string group, string libId, MessageItem item)
        {
            return dp2WeiXinService.Instance.CoverMessage(group, libId, item, "change");
     }

        // DELETE api/<controller>/5
        [HttpDelete]
        public MessageResult Delete(string group, string libId, string msgId,string userName)
        {
            MessageItem item = new MessageItem();
            item.id = msgId;
            item.creator = userName;
            //style == delete
            return dp2WeiXinService.Instance.CoverMessage(group, libId, item, "delete");
        }
    }
}
