using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Weixin.MP.Helpers;
using System.Xml;
using DigitalPlatform.Xml;
using System.Globalization;
using Senparc.Weixin.Context;
using Senparc.Weixin.MP.Entities.Request;
using DigitalPlatform.IO;
using System.Diagnostics;
using DigitalPlatform;
using DigitalPlatform.Text;
using dp2Command.Service;
using DigitalPlatform.LibraryRestClient;
using dp2Command.Service;

namespace dp2weixin
{
    /// <summary>
    /// 自定义MessageHandler
    /// 把MessageHandler作为基类，重写对应请求的处理方法
    /// </summary>
    public partial class dp2MessageHandler : MessageHandler<dp2MessageContext>
    {
        // 由外面传进来的CommandServer
        private dp2BaseCommandService CmdService = null;
        // 公众号程序目录，用于获取新书推荐与公告配置文件的路径
        private string Dp2WeiXinAppDir = "";
        // 是否显示消息路径
        private bool IsDisplayPath = true;
        // 是否需要选择图书馆
        private bool IsNeedSelectLib = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="maxRecordCount"></param>
        public dp2MessageHandler(dp2BaseCommandService cmdServer,
            Stream inputStream, PostModel postModel, int maxRecordCount = 0)
            : base(inputStream, postModel, maxRecordCount)
        {
            //这里设置仅用于测试，实际开发可以在外部更全局的地方设置，
            //比如MessageHandler<MessageContext>.GlobalWeixinContext.ExpireMinutes = 3。
            WeixinContext.ExpireMinutes = 3;

            this.CmdService = cmdServer;
        }

        // 初始化
        public void Init(string dp2weixinAppDir, bool isDisplayPath, bool isNeedSelectLib)
        {
            this.Dp2WeiXinAppDir = dp2weixinAppDir;
            this.IsDisplayPath = isDisplayPath;
            this.IsNeedSelectLib = isNeedSelectLib;

            // 检索是否选择了图书馆
            this.CheckIsSelectLib();
        }

        /// <summary>
        /// 执行时，用于过滤黑名单
        /// </summary>
        public override void OnExecuting()
        {
            base.OnExecuting();
        }

        /// <summary>
        /// 执行后
        /// </summary>
        public override void OnExecuted()
        {
            base.OnExecuted();
        }

        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            string strText = requestMessage.Content;

            //设当前命令路径，用于在回复时输出
            this.CurrentMessageContext.CurrentCmdPath = strText;

            // 退出命令环境
            if (strText == "exit" || strText == "quit")
            {
                // 置空当前命令
                this.CurrentMessageContext.CurrentCmdName = "";
                return this.CreateTextResponseMessage("成功退出命令环境。");
            }

            // 用空隔号分隔命令与参数，例如：
            // search 空 重新发起检索
            // search n             显示上次命中结果集中下一页
            // search 序号         显示详细
            // binding r0000001/111111
            // renew view
            string strCommand = strText;
            string strParam = "";
            int nIndex = strText.IndexOf(' ');
            if (nIndex > 0)
            {
                strCommand = strText.Substring(0, nIndex);
                strParam = strText.Substring(nIndex + 1);
            }

            // 检查是否是命令，如果不是，则将输入认为是当前命令的参数（二级命令）
            bool bRet = dp2CommandUtility.CheckIsCommand(strCommand);
            if (bRet == false)
            {
                strCommand = "";
                if (String.IsNullOrEmpty(this.CurrentMessageContext.CurrentCmdName) == false)
                {
                    strCommand = this.CurrentMessageContext.CurrentCmdName;
                    strParam = strText;
                }
                else
                {
                    // 没有当前命令
                    return DoUnknownCmd(strText);
                }
            }

            //设当前命令路径，用于在回复时输出
            string strPath = strCommand;
            if (String.IsNullOrEmpty(strParam) == false)
                strPath = strCommand + ">" + strParam;
            this.CurrentMessageContext.CurrentCmdPath = strPath;

            // command可以转成小写
            strCommand = strCommand.ToLower();

            //=========================

            // todo selectlib
            if (this.IsNeedSelectLib == true)
            {
                if (strCommand == dp2CommandUtility.C_Command_SelectLib)
                {
                    return this.DoSelectLib(strParam);
                }
                else
                {
                    // 进行其它命令前，如果尚未选择访问的图书馆，提示请先选择图书馆
                    if (this.CurrentMessageContext.LibCode1 == "")
                    {
                        this.CurrentMessageContext.CurrentCmdName = dp2CommandUtility.C_Command_SelectLib;
                        string text = "您尚未选择图书馆，" + this.getLibList();
                        return this.CreateTextResponseMessage(text);
                    }
                }
            }


            // 检索命令
            if (strCommand == dp2CommandUtility.C_Command_Search)
            {
                return this.DoSearch(strParam);
            }

            //=========================
            // 绑定读者账号
            if (strCommand == dp2CommandUtility.C_Command_Binding)
            {
                return this.DoBinding(strParam);
            }

            //=========================
            // 解除绑定
            if (strCommand == dp2CommandUtility.C_Command_Unbinding)
            {
                return this.DoUnbinding();
            }

            //=========================
            // 个人信息
            if (strCommand == dp2CommandUtility.C_Command_MyInfo)
            {
                return this.DoMyInfo();
            }


            //=========================
            // 借阅信息
            if (strCommand == dp2CommandUtility.C_Command_BorrowInfo)
            {
                return this.DoBorrowInfo();
            }

            //=========================
            // 续借
            if (strCommand == dp2CommandUtility.C_Command_Renew)
            {
                return this.DoRenew(strParam);
            }

            //=========================
            // 新书推荐
            if (strCommand == dp2CommandUtility.C_Command_BookRecommend)
            {
                return this.DoNewBooks();
            }

            //=========================
            // 近期公告
            if (strCommand == dp2CommandUtility.C_Command_Notice)
            {
                return this.DoNotice();
            }

            // 不认识的命令
            return DoUnknownCmd(strText);

        }

        private IResponseMessageBase DoSelectLib(string strParam)
        {
            // 设置当前命令
            this.CurrentMessageContext.CurrentCmdName = dp2CommandUtility.C_Command_SelectLib;
            long lRet = 0;
            string strError = "";

            if (strParam == "")
            {
                string text = this.getLibList();
                return this.CreateTextResponseMessage(text);
            }

            // 判断输入是序号还是图书馆代码
            string libCode = "";
            string libUserName = "";
            string templibCode = "";
            int nIndex = -1;
            try
            {
                nIndex = Convert.ToInt32(strParam);
            }
            catch
            {
                templibCode = strParam;
            }

            List<LibItem> libs = LibDatabase.Current.GetLibs();
            if (nIndex != -1)
            {
                if (nIndex > 0 && nIndex <= libs.Count)
                {
                    libCode = libs[nIndex - 1].libCode;
                    libUserName = libs[nIndex - 1].libUserName;
                }
                else
                {
                    string text = "您输入的序号不正确，请重新输入。\n" + this.getLibList();
                    return this.CreateTextResponseMessage(text);
                }
            }
            else
            {
                foreach (LibItem item in libs)
                {
                    if (item.libCode == templibCode)
                    {
                        libCode = item.libCode;
                        libUserName = item.libUserName;
                    }
                }

                if (libCode == "")
                {
                    string text = "您输入的馆代码不正确，请重新输入。\n" + this.getLibList();
                    return this.CreateTextResponseMessage(text);
                }
            }

            this.CurrentMessageContext.LibCode1 = libCode;
            this.CurrentMessageContext.LibUserName = libUserName;

            //要保存到微信用户表中，下面绑定用户从对应的图书馆查读者。
            this.CmdService.SelectLib(this.CurrentMessageContext.UserName, libCode,libUserName);
            return this.CreateTextResponseMessage("您成功选择了图书馆[" + libCode + "]");
        }

        public string getLibList()
        {
            List<LibItem> libs = LibDatabase.Current.GetLibs();
            string text = "";
            int i = 1;
            foreach (LibItem item in libs)
            {
                text += i.ToString() + "  " + item.libCode + "  " + item.libName + "\n";
                i++;
            }
            text = "下面是图书馆列表，请回复序号或者馆代码。\n" + text;

            return text;
        }



        /// <summary>
        /// 检索
        /// </summary>
        /// <param name="strParam"></param>
        /// <returns></returns>
        private IResponseMessageBase DoSearch(string strParam)
        {
            // 设置当前命令
            this.CurrentMessageContext.CurrentCmdName = dp2CommandUtility.C_Command_Search;

            long lRet = 0;
            string strError = "";
            SearchCommand searchCmd = (SearchCommand)this.CurrentMessageContext.CmdContiner.GetCommand(dp2CommandUtility.C_Command_Search);

            if (strParam == "")
            {
                if (searchCmd.BiblioResultPathList != null && searchCmd.BiblioResultPathList.Count > 0)
                    return this.CreateTextResponseMessage("请输入检索词重新检索，或者输入V查看上次检索结果。");
                else
                    return this.CreateTextResponseMessage("请输入检索词");
            }

            // 如果没有结果集，优先认查询
            if (searchCmd.BiblioResultPathList != null
                && searchCmd.BiblioResultPathList.Count > 0)
            {
                // 查看，从第一页开始
                if (strParam.ToLower() == "v")
                {
                    // 从头显示
                    searchCmd.ResultNextStart = 0;
                    searchCmd.IsCanNextPage = true;
                    string strNextPage = "";
                    bool bRet = searchCmd.GetNextPage(out strNextPage, out strError);
                    if (bRet == true)
                        return this.CreateTextResponseMessage(strNextPage);
                    else
                        return this.CreateTextResponseMessage(strError);
                }

                // 下一页
                if (strParam.ToLower() == "n")
                {
                    string strNextPage = "";
                    bool bRet = searchCmd.GetNextPage(out strNextPage, out strError);
                    if (bRet == true)
                        return this.CreateTextResponseMessage(strNextPage);
                    else
                        return this.CreateTextResponseMessage(strError);
                }

                // 试着转换为书目序号
                int nBiblioIndex = 0;
                try
                {
                    nBiblioIndex = int.Parse(strParam);
                }
                catch
                { }
                // 获取详细信息
                if (nBiblioIndex >= 1)
                {
                    string strBiblioInfo = "";
                    lRet = this.CmdService.GetDetailBiblioInfo(searchCmd, nBiblioIndex,
                        out strBiblioInfo,
                        out strError);
                    if (lRet == -1)
                    {
                        return this.CreateTextResponseMessage(strError);
                    }

                    // 输入详细信息
                    return this.CreateTextResponseMessage(strBiblioInfo);
                }
            }

            // 检索
            string strFirstPage = "";
            lRet = this.CmdService.SearchBiblio(strParam, searchCmd,
                out strFirstPage,
                out strError);
            if (lRet == -1)
            {
                return this.CreateTextResponseMessage("检索出错：" + strError);
            }
            else if (lRet == 0)
            {
                return this.CreateTextResponseMessage("未命中");
            }
            else
            {
                return this.CreateTextResponseMessage(strFirstPage);
            }
        }

        /// <summary>
        /// 绑定
        /// </summary>
        /// <param name="strParam"></param>
        /// <returns></returns>
        private IResponseMessageBase DoBinding(string strParam)
        {
            // 设置当前命令
            this.CurrentMessageContext.CurrentCmdName = dp2CommandUtility.C_Command_Binding;

            if (strParam == "")
            {
                string strMessage = "请输入'读者证条码号'（注:您也可以同时输入'读者证条码号'和'密码'，中间以/分隔，例如:R0000001/123）。";
                return this.CreateTextResponseMessage(strMessage);
            }

            // 得到绑定命令
            BindingCommand bindingCmd = (BindingCommand)this.CurrentMessageContext.CmdContiner.GetCommand(dp2CommandUtility.C_Command_Binding);
            string readerBarcode = strParam;
            int nTempIndex = strParam.IndexOf('/');
            if (nTempIndex > 0) // 同时输入读者证条码与密码
            {
                bindingCmd.ReaderBarcode = strParam.Substring(0, nTempIndex);
                bindingCmd.Password = strParam.Substring(nTempIndex + 1);
            }
            else
            {
                // 看看上一次输入过用户名的没有,如果已存在用户名，那么这次输入的就是密码
                if (bindingCmd.ReaderBarcode == "")
                {
                    bindingCmd.ReaderBarcode = strParam;
                    return this.CreateTextResponseMessage("读输入密码");
                }
                else
                {
                    bindingCmd.Password = strParam;
                }
            }

            string strReaderBarcode = "";
            string strError = "";
            long lRet = this.CmdService.Binding(bindingCmd.ReaderBarcode,
                bindingCmd.Password,
                this.CurrentMessageContext.UserName, //.WeiXinId
                out strReaderBarcode,
                out strError);
            if (lRet == -1 || lRet == 0)
            {
                return CreateTextResponseMessage(strError);
            }

            // 把用户名与密码清掉，以便再绑其它账号
            bindingCmd.ReaderBarcode = "";
            bindingCmd.Password = "";

            // 设到当前读者变量上
            this.CurrentMessageContext.ReaderBarcode = strReaderBarcode;
            return this.CreateTextResponseMessage("绑定成功!");
        }

        /// <summary>
        /// 解除绑定
        /// </summary>
        /// <returns></returns>
        private IResponseMessageBase DoUnbinding()
        {
            // 设置当前命令
            this.CurrentMessageContext.CurrentCmdName = "";

            long lRet = 0;
            string strError = "";

            // 先检查有无绑定读者账号
            lRet = this.CheckIsBinding(out strError);
            if (lRet == -1)
            {
                return this.CreateTextResponseMessage(strError);
            }
            // 尚未绑定读者账号
            if (lRet == 0)
            {
                return this.CreateTextResponseMessage("您尚未绑定读者账号，不需要解除绑定。");
            }

            // 解除绑定
            lRet = this.CmdService.Unbinding(this.CurrentMessageContext.UserName,
                this.CurrentMessageContext.ReaderBarcode, out strError);
            if (lRet == -1 || lRet == 0)
            {
                return this.CreateTextResponseMessage(strError);
            }

            // 置空当前读者变量上
            this.CurrentMessageContext.ReaderBarcode = "";
            return this.CreateTextResponseMessage("解除绑定成功。");
        }

        /// <summary>
        /// 个人信息
        /// </summary>
        /// <returns></returns>
        private IResponseMessageBase DoMyInfo()
        {
            // 置空当前命令，该命令不需要保存状态
            this.CurrentMessageContext.CurrentCmdName = "";

            long lRet = 0;
            string strError = "";

            // 先检查有无绑定读者账号
            lRet = this.CheckIsBinding(out strError);
            if (lRet == -1)
            {
                return this.CreateTextResponseMessage(strError);
            }
            // 尚未绑定读者账号
            if (lRet == 0)
            {
                return this.CreateTextResponseMessage("尚未绑定读者账号，请先调binding命令先绑定");
            }

            // 获取读者信息
            string strMyInfo = "";
            lRet = this.CmdService.GetMyInfo1(this.CurrentMessageContext.ReaderBarcode, out strMyInfo,
                out strError);
            if (lRet == -1 || lRet == 0)
            {
                return this.CreateTextResponseMessage(strError);
            }

            // 显示个人信息
            return this.CreateTextResponseMessage(strMyInfo);
        }


        /// <summary>
        /// 借阅信息
        /// </summary>
        private IResponseMessageBase DoBorrowInfo()
        {
            // 置空当前命令,该命令不需要保存状态
            this.CurrentMessageContext.CurrentCmdName = "";

            long lRet = 0;
            string strError = "";

            // 先检查是否绑定读者账号
            lRet = this.CheckIsBinding(out strError);
            if (lRet == -1)
            {
                return this.CreateTextResponseMessage(strError);
            }
            // 尚未绑定读者账号
            if (lRet == 0)
            {
                return this.CreateTextResponseMessage("尚未绑定读者账号，请先调binding命令先绑定");
            }

            string strBorrowInfo = "";
            lRet = this.CmdService.GetBorrowInfo1(this.CurrentMessageContext.ReaderBarcode, out strBorrowInfo,
                out strError);
            if (lRet == -1)
            {
                return this.CreateTextResponseMessage(strError);
            }

            // 显示借阅信息
            return this.CreateTextResponseMessage(strBorrowInfo);
        }

        /// <summary>
        /// 续借
        /// </summary>
        /// <param name="strParam"></param>
        /// <returns></returns>
        private IResponseMessageBase DoRenew(string strParam)
        {
            // 设置当前命令
            this.CurrentMessageContext.CurrentCmdName = dp2CommandUtility.C_Command_Renew;

            long lRet = 0;
            string strError = "";

            // 先检查是否绑定读者账号
            lRet = this.CheckIsBinding(out strError);
            if (lRet == -1)
            {
                return this.CreateTextResponseMessage(strError);
            }
            // 尚未绑定读者账号
            if (lRet == 0)
            {
                return this.CreateTextResponseMessage("尚未绑定读者账号，请先调binding命令先绑定");
            }

            // 查看已借图书
            if (strParam == "" || strParam == "view")
            {
                string strBorrowInfo = "";
                lRet = this.CmdService.GetBorrowInfo1(this.CurrentMessageContext.ReaderBarcode, out strBorrowInfo,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    return this.CreateTextResponseMessage(strError);
                }

                // 显示个人信息
                string strMessage = strBorrowInfo + "\n"
                    + "请输入续借图书编号或者册条码号。";
                return this.CreateTextResponseMessage(strMessage);
            }


            // 目前只认作册条码，todo支持序号
            BorrowInfo borrowInfo = null;
            lRet = this.CmdService.Renew(this.CurrentMessageContext.ReaderBarcode,
                strParam,
                out borrowInfo,
                out strError);
            if (lRet == -1 || lRet == 0)
            {
                return this.CreateTextResponseMessage(strError);
            }

            // 显示续借成功信息信息
            string returnTime = DateTimeUtil.ToLocalTime(borrowInfo.LatestReturnTime, "yyyy/MM/dd");
            string strText = strParam + "续借成功,还书日期为：" + returnTime + "。";
            return this.CreateTextResponseMessage(strText);
        }


        /// <summary>
        /// 处理未知的命令
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        private IResponseMessageBase DoUnknownCmd(string strText)
        {
            string strMessage = "您好，不认识的命令，您可以回复：\n"
                   + "search:检索" + "\n"
                   + "binding:绑定读者账号" + "\n"
                   + "unbinding:解除绑定" + "\n"
                   + "myinfo:个人信息" + "\n"
                   + "borrowinfo:借阅信息" + "\n"
                   + "renew:续借" + "\n"
                   + "bookrecommend:新书推荐" + "\n"
                   + "notice:最新公告";
            return this.CreateTextResponseMessage(strMessage);
        }

        private IResponseMessageBase CreateTextResponseMessage(string strText, bool bHasPath)
        {
            if (bHasPath == true && this.IsDisplayPath == true)
            {
                strText = "命令路径:[" + this.CurrentMessageContext.CurrentCmdPath + "]\n"
                    + "------------\n"
                    + strText;
            }

            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = strText;
            return responseMessage;
        }

        /// <summary>
        /// 创建文本回复消息
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        private IResponseMessageBase CreateTextResponseMessage(string strText)
        {
            return this.CreateTextResponseMessage(strText, true);
        }

        /// <summary>
        /// 检索是否选择了图书馆
        /// </summary>
        /// <param name="strError"></param>
        /// <returns></returns>
        private bool CheckIsSelectLib()
        {

            if (String.IsNullOrEmpty(this.CurrentMessageContext.LibCode1) == true)
            {
                // 从mongodb中查
                WxUserItem userItem= this.CmdService.CheckIsSelectLib(this.WeixinOpenId);
                if (userItem== null)
                    return false;


                this.CurrentMessageContext.LibCode1 = userItem.libCode;
                this.CurrentMessageContext.LibUserName = userItem.libUserName;
            }

            return true;
        }

        /// <summary>
        /// 检查微信用户是否绑定读者账号
        /// </summary>
        /// <param name="strWeiXinId"></param>
        /// <param name="strXml"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        private int CheckIsBinding(out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(this.CurrentMessageContext.ReaderBarcode) == true)
            {
                // 根据openid检索绑定的读者
                string strRecPath = "";
                string strXml = "";
                long lRet = this.CmdService.SearchReaderByWeiXinId(this.CurrentMessageContext.UserName,
                    out strRecPath,
                    out strXml,
                    out strError);
                if (lRet == -1)
                {
                    return -1;
                }
                // 未绑定
                if (lRet == 0)
                {
                    return 0;
                }

                XmlDocument dom = new XmlDocument();
                dom.LoadXml(strXml);
                this.CurrentMessageContext.ReaderBarcode = DomUtil.GetNodeText(dom.DocumentElement.SelectSingleNode("barcode"));
            }
            return 1;
        }





        #region 新书推荐

        /// <summary>
        /// 图书推荐
        /// </summary>
        /// <returns></returns>
        private IResponseMessageBase DoNewBooks()
        {
            // 置空当前命令,该命令不需要保存状态
            this.CurrentMessageContext.CurrentCmdName = "";

            // 加载新书配置文件
            string fileName = this.Dp2WeiXinAppDir + "/newbooks.xml";
            if (File.Exists(fileName) == false)
            {
                return this.CreateTextResponseMessage("暂无推荐图书。");
            }

            // 拼成图文消息
            var responseMessage = CreateResponseMessage<ResponseMessageNews>();
            XmlDocument dom = new XmlDocument();
            dom.Load(fileName);
            XmlNodeList itemList = dom.DocumentElement.SelectNodes("item");
            foreach (XmlNode node in itemList)
            {
                responseMessage.Articles.Add(new Article()
                {
                    Title = DomUtil.GetNodeText(node.SelectSingleNode("Title")),
                    Description = DomUtil.GetNodeText(node.SelectSingleNode("Description")),
                    PicUrl = this.CmdService.weiXinUrl + DomUtil.GetNodeText(node.SelectSingleNode("PicUrl")),
                    Url = DomUtil.GetNodeText(node.SelectSingleNode("Url"))
                });
            }
            return responseMessage;
        }

        /// <summary>
        /// 近期通告
        /// </summary>
        /// <returns></returns>
        private IResponseMessageBase DoNotice()
        {
            // 置空当前命令,该命令不需要保存状态
            this.CurrentMessageContext.CurrentCmdName = "";

            // 加载公告配置文件
            string fileName = this.Dp2WeiXinAppDir + "/notice.xml";
            if (File.Exists(fileName) == false)
            {
                return this.CreateTextResponseMessage("暂无公告");
                var textResponseMessage = CreateResponseMessage<ResponseMessageText>();
                textResponseMessage.Content = "";
                return textResponseMessage;
            }

            //拼成图文消息
            var responseMessage = CreateResponseMessage<ResponseMessageNews>();
            XmlDocument dom = new XmlDocument();
            dom.Load(fileName);
            XmlNodeList itemList = dom.DocumentElement.SelectNodes("item");
            foreach (XmlNode node in itemList)
            {
                Article article = new Article();
                article.Title = DomUtil.GetNodeText(node.SelectSingleNode("Title"));
                article.Description = DomUtil.GetNodeText(node.SelectSingleNode("Description"));
                string picUrl = DomUtil.GetNodeText(node.SelectSingleNode("PicUrl"));
                if (String.IsNullOrEmpty(picUrl) == false)
                    article.PicUrl = this.CmdService.weiXinUrl + picUrl;
                article.Url = DomUtil.GetNodeText(node.SelectSingleNode("Url"));
                responseMessage.Articles.Add(article);
            }
            return responseMessage;
        }

        #endregion


        #region 其它类型消息

        /// <summary>
        /// 处理位置请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnLocationRequest(RequestMessageLocation requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = string.Format("您刚才发送了地理位置信息。Location_X：{0}，Location_Y：{1}，Scale：{2}，标签：{3}",
                              requestMessage.Location_X, requestMessage.Location_Y,
                              requestMessage.Scale, requestMessage.Label);
            return responseMessage;
        }
        /// <summary>
        /// 处理图片请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnImageRequest(RequestMessageImage requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageNews>();
            responseMessage.Articles.Add(new Article()
            {
                Title = "您刚才发送了图片信息",
                Description = "您发送的图片将会显示在边上",
                PicUrl = requestMessage.PicUrl,
                Url = "http://www.qxuninfo.com"
            });
            responseMessage.Articles.Add(new Article()
            {
                Title = "第二条",
                Description = "第二条带连接的内容",
                PicUrl = requestMessage.PicUrl,
                Url = "http://www.qxuninfo.com"
            });
            return responseMessage;
        }
        /// <summary>
        /// 处理语音请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnVoiceRequest(RequestMessageVoice requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageMusic>();
            responseMessage.Music.MusicUrl = "http://www.qxuninfo.com/music.mp3";
            responseMessage.Music.Title = "这里是一条音乐消息";
            responseMessage.Music.Description = "时间都去哪儿了";
            return responseMessage;
        }
        /// <summary>
        /// 处理视频请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnVideoRequest(RequestMessageVideo requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您发送了一条视频信息，ID：" + requestMessage.MediaId;
            return responseMessage;
        }
        /// <summary>
        /// 处理链接消息请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnLinkRequest(RequestMessageLink requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = string.Format(@"您发送了一条连接信息：
Title：{0}
Description:{1}
Url:{2}", requestMessage.Title, requestMessage.Description, requestMessage.Url);
            return responseMessage;
        }
        /// <summary>
        /// 处理事件请求（这个方法一般不用重写，这里仅作为示例出现。除非需要在判断具体Event类型以外对Event信息进行统一操作
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEventRequest(IRequestMessageEventBase requestMessage)
        {
            var eventResponseMessage = base.OnEventRequest(requestMessage);//对于Event下属分类的重写方法，见：CustomerMessageHandler_Events.cs
            return eventResponseMessage;
        }

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            //所有没有被处理的消息会默认返回这里的结果
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "这条消息来自DefaultResponseMessage。";
            return responseMessage;
        }
        #endregion
    }
}
