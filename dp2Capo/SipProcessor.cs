using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Marc;
using DigitalPlatform.Message;
using DigitalPlatform.Net;
using DigitalPlatform.SIP.Server;
using DigitalPlatform.SIP2;
using DigitalPlatform.SIP2.Request;
using DigitalPlatform.SIP2.Response;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2Capo
{
    public static class SipProcessor
    {
        public static void AddEvents(SipServer sip_server, bool bAdd)
        {
            if (bAdd)
            {
                sip_server.ProcessRequest += Sip_server_ProcessRequest;
            }
            else
            {
                sip_server.ProcessRequest -= Sip_server_ProcessRequest;
            }
        }

        public static Tuple<Instance, string> FindSipInstance(string strInstanceName,
            bool bEnglishErrorInfo = true)
        {
            Instance instance = ServerInfo.FindInstance(strInstanceName);
            if (instance == null)
                return new Tuple<Instance, string>(null,
                    bEnglishErrorInfo ?
                    "instance '" + strInstanceName + "' not exist" :
                    "实例 '" + strInstanceName + "' 不存在"
                    );
            if (instance.sip_host == null)
                return new Tuple<Instance, string>(null,
                    bEnglishErrorInfo ?
                    "instance '" + strInstanceName + "' not enable SIP service" :
                    "实例 '" + strInstanceName + "' 没有启用 SIP 服务"
                    );
            return new Tuple<Instance, string>(instance, "");
#if NO
            Instance instance = ServerInfo.FindInstance(strInstanceName);
            if (instance == null)
                return null;
            if (instance.sip_host == null)
                return null;    // 实例虽然存在，但没有启用 SIP 服务
            return instance;
#endif
        }

        public static Instance FindSipInstance(string strInstanceName, out string strError)
        {
            strError = "";
            var ret = FindSipInstance(strInstanceName);
            if (ret.Item1 != null)
                return ret.Item1;
            strError = ret.Item2;
            return null;
        }

        private static void Sip_server_ProcessRequest(object sender, ProcessSipRequestEventArgs e)
        {
            string strResponse = "";
            //string strError = "";
            //int nRet = 0;

            Encoding default_encoding = Encoding.GetEncoding(936);

            SipChannel sip_channel = sender as SipChannel;

            // TODO: TcpClient 可能为 null, 表示通道已经被切断
            string ip = TcpServer.GetClientIP(sip_channel.TcpClient);

            // Login 之前，这里只是默认的编码方式。因为 Login 之前没法确定实例名。如果 Login 的用户名(和实例名)能确保是英文，用默认编码方式倒也不会有问题
            // 如果 Login 请求中的用户名包含汉字，则要求 SIP Client 开发者把字符串按照 UrlEncode 方式进行转义，这样 SIP Server 依然可以识别
            Encoding encoding = sip_channel.Encoding;
            if (encoding == null)
                encoding = default_encoding;
            string strRequest = encoding.GetString(e.Request);

            // 2020/8/18
            strRequest = strRequest.TrimEnd(new char[] { '\r', '\n' });

            string strMessageIdentifiers = strRequest.Substring(0, 2);

            string strChannelName = "ip:" + ip + ",channel:" + sip_channel.GetHashCode();

            if (strMessageIdentifiers != "99")
                LibraryManager.Log?.Info(strChannelName + ",\r\nrequest=" + strRequest);


            //try
            //{
            // 处理消息
            switch (strMessageIdentifiers)
            {
                case "09":
                    {
                        strResponse = Checkin(sip_channel, strRequest);
                        break;
                    }
                case "11":
                    {
                        strResponse = Checkout(sip_channel, strRequest);
                        break;
                    }
                case "17":
                    {

                        strResponse = ItemInfo(sip_channel, strRequest);
                        break;
                    }
                case "19":
                    {

                        strResponse = ItemStatusUpdate(sip_channel, strRequest);
                        break;
                    }
                case "29":
                    {
                        strResponse = Renew(sip_channel, strRequest);
                        break;
                    }
                case "35":
                    {
                        strResponse = EndPatronSession(strRequest);
                        break;
                    }
                case "37":
                    {
                        strResponse = Amerce(sip_channel, strRequest);
                        break;
                    }
                case "85":
                    {
                        /*
                        nRet = GetReaderInfo(strReaderBarcode,
                            strPassword,
                            "readerInfo",
                            out strBackMsg,
                            out strError);
                        if (nRet == 0)
                            this.WriteToLog(strError);
                        */
                        break;
                    }
                case "63":
                    {
                        strResponse = PatronInfo(sip_channel, strRequest);
                        break;
                    }
                case "81":
                    {
                        strResponse = SetReaderInfo(sip_channel, strRequest);
                        //if (nRet == 0)
                        //{
                        //    if (String.IsNullOrEmpty(strError) == false)
                        //        LogManager.Logger.Error(strError);
                        //}
                        break;
                    }
                case "91":
                    {
                        strResponse = CheckDupReaderInfo(sip_channel, strRequest);
                        //if (nRet == 0)
                        //{
                        //    if (String.IsNullOrEmpty(strError) == false)
                        //        LogManager.Logger.Error(strError);
                        //}
                        break;
                    }
                case "93":
                    {
                        strResponse = Login(sip_channel, ip, strRequest);
                        if ("941" == strResponse)
                        {

                        }
                        else
                        {
                            /*
                            sip_channel._dp2username = "";
                            sip_channel._dp2password = "";
                            sip_channel._locationCode = "";
                            */
                        }

                        break;
                    }
                case "96":
                    {
                        strResponse = sip_channel.LastMsg;
                        break;
                    }
                case "99":
                    {
                        strResponse = SCStatus(sip_channel, strRequest);
                        break;
                    }
                default:
                    strResponse = "无法识别的命令'" + strMessageIdentifiers + "'";
                    break;
            }

            sip_channel.LastMsg = strResponse;
            // 加校验码
            strResponse = AddChecksumForMessage(sip_channel, strResponse);

            // ScStatus 请求来得很频繁，不在 Log 中记载 Info
            if (strMessageIdentifiers != "99")
                LibraryManager.Log?.Info(strChannelName + ",\r\nresponse=" + strResponse);

            e.Response = sip_channel.Encoding == null ?
                default_encoding.GetBytes(strResponse)
                : sip_channel.Encoding.GetBytes(strResponse);
            return;
            // }
#if NO
            catch (Exception ex)
            {
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                throw ex;
            }
#endif

            //ERROR1:
            //throw new Exception(strError);
        }

        // 用实例中的自动清理时间参数设置 SipChannel 的 Timeout 值
        static void SetChannelTimeout(SipChannel sip_channel,
            string userName,
            Instance instance)
        {
#if NO
            if (instance == null)
            {
                if (sip_channel.InstanceName == null)
                    return; // 没有 login 的就没法确定 Instance，也就没法为 sip_channel 设置 Timeout

                instance = FindSipInstance(sip_channel.InstanceName, out string strError);
                if (instance == null)
                    return;
            }
#endif

            // sip_channel.Timeout = instance.sip_host.AutoClearSeconds == 0 ? TimeSpan.MinValue : TimeSpan.FromSeconds(instance.sip_host.AutoClearSeconds);

            Debug.Assert(string.IsNullOrEmpty(userName) == false, "");
            int autoclear_seconds = instance.sip_host.GetSipParam(userName).AutoClearSeconds;
            sip_channel.Timeout = autoclear_seconds == 0 ? TimeSpan.MinValue : TimeSpan.FromSeconds(autoclear_seconds);
        }

        // 加校验码
        static string AddChecksumForMessage(
            SipChannel sip_channel,
            string strPackage)
        {
            // 加校验码
            StringBuilder msg = new StringBuilder(strPackage);
            char endChar = strPackage[strPackage.Length - 1];
            if (endChar == '|')
                msg.Append("AY4AZ");
            else
                msg.Append("|AY4AZ");
            msg.Append(GetChecksum(strPackage));

            // 写日志
            //LogManager.Logger.Info("Send:" + msg.ToString());

            // 加消息结束符
            Debug.Assert(sip_channel.Terminator == '\r' || sip_channel.Terminator == '\n', "SIP 通道的结束符应该早就初始化了");
            msg.Append((char)sip_channel.Terminator);
            return msg.ToString();
        }

        /// <summary>
        /// To calculate the checksum add each character as an unsigned binary number,
        /// take the lower 16 bits of the total and perform a 2's complement. 
        /// The checksum field is the result represented by four hex digits.
        /// </summary>
        /// <param name="message">
        /// 内容中不包含 校验和(checksum)
        /// </param>
        /// <returns></returns>
        public static string GetChecksum(string message)
        {
            string checksum = "";

            try
            {
                ushort sum = 0;
                foreach (char c in message)
                {
                    sum += c;
                }

                ushort checksum_inverted_plus1 = (ushort)(~sum + 1);

                checksum = checksum_inverted_plus1.ToString("X4");
            }
            catch (Exception ex)
            {
                string strError = ex.Message;
                checksum = null;
            }
            return checksum;
        }

        static string Login(SipChannel sip_channel,
            string strClientIP,
            string message)
        {
            int nRet = 0;
            string strError = "";

            LoginResponse_94 response = new LoginResponse_94()
            {
                Ok_1 = "0",
            };

            Login_93 request = new Login_93();
            try
            {
                nRet = request.parse(message, out strError);
                if (-1 == nRet)
                {
                    LibraryManager.Log?.Error(strError);
                    return response.ToText();
                }
            }
            catch (Exception ex)
            {
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                return response.ToText();
            }

            // 对用户名解除转义
            string strRequestUserID = request.CN_LoginUserId_r;
            if (string.IsNullOrEmpty(strRequestUserID) == false && strRequestUserID.IndexOf("%") != -1)
                strRequestUserID = Uri.UnescapeDataString(strRequestUserID);

            // 解析出实例名
            List<string> parts = StringUtil.ParseTwoPart(strRequestUserID, "@");
            string strUserName = parts[0];
            string strInstanceName = parts[1];

            string strPassword = request.CO_LoginPassword_r;
            string strLocationCode = request.CP_LocationCode_o;
            if (string.IsNullOrEmpty(strLocationCode) == false && strLocationCode.IndexOf("%") != -1)
                strLocationCode = Uri.UnescapeDataString(strLocationCode);

            var ret = FindSipInstance(strInstanceName, sip_channel.Encoding == null);
            if (ret.Item1 == null)
            {
                if (sip_channel.Encoding == null)
                    strError = "user name '" + strRequestUserID + "' locate instance ，" + ret.Item2;
                else
                    strError = "以用户名 '" + strRequestUserID + "' 定位实例，" + ret.Item2;
                goto ERROR1;
            }
            Instance instance = ret.Item1;

            strInstanceName = instance.Name;

            // 匿名登录情形
            if (string.IsNullOrEmpty(strUserName))
            {
                // 如果定义了允许匿名登录
                if (String.IsNullOrEmpty(instance.zhost.AnonymousUserName) == false)
                {
                    strUserName = instance.sip_host.AnonymousUserName;
                    strPassword = instance.sip_host.AnonymousPassword;
                }
                else
                {
                    if (sip_channel.Encoding == null)
                        strError = "anonymouse login not allowed";
                    else
                        strError = "不允许匿名登录";
                    goto ERROR1;
                }
            }

            try
            {
                SipParam sip_config = instance.sip_host.GetSipParam(strUserName);

                // 检查 IP 白名单
                string ipList = sip_config.IpList;
                if (string.IsNullOrEmpty(ipList) == false && ipList != "*"
                    && StringUtil.MatchIpAddressList(ipList, strClientIP) == false)
                {
                    if (sip_channel.Encoding == null)
                        strError = "client IP address '" + strClientIP + "' not in white list";
                    else
                        strError = "前端 IP 地址 '" + strClientIP + "' 不在白名单允许的范围内";
                    goto ERROR1;
                }
            }
            catch (Exception ex)
            {
                if (sip_channel.Encoding == null)
                    strError = "get SIP configuration error:" + ExceptionUtil.GetAutoText(ex);
                else
                    strError = "获取 SIP 参数时出错:" + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            // 让 channel 从此携带 Instance Name
            sip_channel.InstanceName = strInstanceName;
            sip_channel.UserName = strUserName;
            sip_channel.Password = strPassword;
            // 从此以后，报错信息才可以使用中文了
            sip_channel.Encoding = instance.sip_host.GetSipParam(sip_channel.UserName).Encoding;
            // 注：登录以后 Timeout 才按照实例参数来设定。此前是 sip_channel.Timeout 的默认值
            // sip_channel.Timeout = instance.sip_host.AutoClearSeconds == 0 ? TimeSpan.MinValue : TimeSpan.FromSeconds(instance.sip_host.AutoClearSeconds);
            SetChannelTimeout(sip_channel, sip_channel.UserName, instance);

            LoginInfo login_info = new LoginInfo { UserName = sip_channel.UserName, Password = sip_channel.Password };

            LibraryChannel library_channel = instance.MessageConnection.GetChannel(login_info);

            try
            {
                long lRet = library_channel.Login(strUserName,
                    strPassword,
                    "type=worker,client=dp2SIPServer|0.01,location=#SIP@" + strClientIP + ",clientip=" + strClientIP,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    goto ERROR1;
                }
                else
                {
                    response.Ok_1 = "1";

                    sip_channel.LocationCode = strLocationCode;
                    sip_channel.InstanceName = strInstanceName; // "";  BUG 2018/8/24 排除
                    sip_channel.UserName = strUserName;
                    sip_channel.Password = strPassword;

                    LibraryManager.Log?.Info("终端 " + strLocationCode + " : " + strUserName + " 接入");
                }

                return response.ToText();
            }
            finally
            {
                instance.MessageConnection.ReturnChannel(library_channel);
            }

        ERROR1:
            sip_channel.LocationCode = "";
            sip_channel.InstanceName = "";
            sip_channel.UserName = "";
            sip_channel.Password = "";
            LibraryManager.Log?.Info("Login() error: " + strError);
            return response.ToText();
        }

        class FunctionInfo
        {
            public string InstanceName { get; set; }
            public Instance Instance { get; set; }
            // LoginInfo LoginInfo { get; set; }
            public LibraryChannel LibraryChannel { get; set; }
            public string ErrorInfo { get; set; }
        }

        static FunctionInfo BeginFunction(SipChannel sip_channel)
        {
            bool useSingleInstance = true;  // 是否允许单实例情况，不登录而直接使用唯一实例的匿名账户

            string strError = "";

            bool bSingleInstance = false;
            FunctionInfo info = new FunctionInfo();

            info.InstanceName = sip_channel.InstanceName;
            if (info.InstanceName == null)
            {
                if (useSingleInstance == true && ServerInfo._instances.Count == 1)
                {
                    // 表示此时属于单实例特殊状态
                    bSingleInstance = true;
                }
                else
                {
                    if (sip_channel.Encoding == null)
                        strError = "not login。(SIP channel instance name ('InstanceName') has not initialized)";
                    else
                        strError = "尚未登录。(SIP 通道中 实例名 ('InstanceName') 尚未在属性集合初始化)";
                    goto ERROR1;
                }
            }
            info.Instance = FindSipInstance(info.InstanceName, out strError);
            if (info.Instance == null)
            {
                if (string.IsNullOrEmpty(strError))
                {
                    if (sip_channel.Encoding == null)
                        strError = "instance name '" + info.InstanceName + "' not exists (or SIP Service has not enabled)";
                    else
                        strError = "实例名 '" + info.InstanceName + "' 不存在(或实例没有启用 SIP 服务)";
                }
                goto ERROR1;
            }

            info.InstanceName = info.Instance.Name; // 2018/8/10

            if (info.Instance.Running == false)
            {
                if (sip_channel.Encoding == null)
                    strError = "instance '" + info.Instance.Name + "' is in maintenance";
                else
                    strError = "实例 '" + info.Instance.Name + "' 正在维护中，暂时不能访问";
                goto ERROR1;
            }

            var login_info = new LoginInfo { UserName = sip_channel.UserName, Password = sip_channel.Password };

            // 单实例特殊情况下，使用匿名登录账户
            if (login_info.UserName == null)
            {
                if (bSingleInstance)
                {
                    if (string.IsNullOrEmpty(info.Instance.sip_host.AnonymousUserName))
                    {
                        if (sip_channel.Encoding == null)
                            strError = "please login. (single instance and none anonymouse account, access dp2library denied)";
                        else
                            strError = "虽然是单实例情形，但尚未配置匿名登录账户，因此无法对 dp2library 进行登录和访问";
                        goto ERROR1;
                    }
                    login_info.UserName = info.Instance.sip_host.AnonymousUserName;
                    login_info.Password = info.Instance.sip_host.AnonymousPassword;

                    // Password 为 null 表示需要代理方式登录。要避免出现 null 这种情况 
                    if (login_info.Password == null)
                        login_info.Password = "";

                    SetChannelTimeout(sip_channel, login_info.UserName, info.Instance);
                }
                else
                {
                    if (sip_channel.Encoding == null)
                        strError = "not login, can't locate instance";
                    else
                        strError = "尚未登录，无法定位实例";
                    goto ERROR1;
                }
            }

            info.LibraryChannel = info.Instance.MessageConnection.GetChannel(login_info);

            return info;
        ERROR1:
            info.ErrorInfo = strError;
            return info;
        }

        static void EndFunction(FunctionInfo info)
        {
            info.Instance.MessageConnection.ReturnChannel(info.LibraryChannel);
        }

        /// <summary>
        /// 还书
        /// </summary>
        /// <param name="sip_channel">SIP 通道</param>
        /// <param name="message">SIP 请求消息</param>
        /// <returns></returns>
        static string Checkin(
            SipChannel sip_channel,
            string message)
        {
            string strError = "";
            int nRet = 0;

            CheckinResponse_10 response = new CheckinResponse_10()
            {
                Ok_1 = "0",
                Resensitize_1 = "Y",
                MagneticMedia_1 = "N",
                Alert_1 = "N",
                TransactionDate_18 = SIPUtility.NowDateTime,
                AO_InstitutionId_r = "dp2Library",
                AJ_TitleIdentifier_o = string.Empty,
                AQ_PermanentLocation_r = string.Empty,
                CL_SortBin_o = "sort bin",
            };

#if NO
            string strInstanceName = sip_channel.EnsureProperty().GetKeyValue("i_n");
            if (strInstanceName == null)
            {
                strError = "SIP 通道中 实例名 '" + strInstanceName + "' 尚未初始化";
                LibraryManager.Log?.Error(strError);
                goto ERROR1;
            }
            Instance instance = FindSipInstance(strInstanceName);
            if (instance == null)
            {
                strError = "实例名 '" + strInstanceName + "' 不存在(或实例没有启用 SIP 服务)";
                goto ERROR1;
            }
            if (instance.Running == false)
            {
                strError = "实例 '" + instance.Name + "' 正在维护中，暂时不能访问";
                goto ERROR1;
            }

            LoginInfo login_info = new LoginInfo { UserName = sip_channel._dp2username, Password = sip_channel._dp2password };

            LibraryChannel library_channel = instance.MessageConnection.GetChannel(login_info);
#endif
            FunctionInfo info = BeginFunction(sip_channel);
            if (string.IsNullOrEmpty(info.ErrorInfo) == false)
            {
                strError = info.ErrorInfo;
                goto ERROR1;
            }
            try
            {
                Checkin_09 request = new Checkin_09();
                try
                {
                    nRet = request.parse(message, out strError);
                    if (-1 == nRet)
                    {
                        response.AF_ScreenMessage_o = strError;
                        return response.ToText();
                    }
                }
                catch (Exception ex)
                {
                    response.AF_ScreenMessage_o = ex.Message;
                    LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                    return response.ToText();
                }

                string strItemBarcode = request.AB_ItemIdentifier_r;
                if (!string.IsNullOrEmpty(strItemBarcode))
                {
                    response.AB_ItemIdentifier_r = strItemBarcode;

                    long lRet = info.LibraryChannel.Return(
                        "return",
                        "",    //strReaderBarcode,
                        strItemBarcode,
                        "", // strConfirmItemRecPath
                        false,
                        "item,biblio",
                        "xml",
                        out string[] itemRecords,
                        "xml",
                        out string[] readerRecords,
                        "xml",
                        out string[] biblioRecords,
                        out string[] aDupPath,
                        out string strOutputReaderBarcode,
                        out DigitalPlatform.LibraryClient.localhost.ReturnInfo return_info,
                        out strError);
                    if (-1 == lRet)
                    {
                        if (info.LibraryChannel.ErrorCode == ErrorCode.NotBorrowed)
                            response.Ok_1 = "1";

                        response.AF_ScreenMessage_o = strError;
                    }
                    else
                    {
                        response.Ok_1 = "1";
                        response.AA_PatronIdentifier_o = strOutputReaderBarcode;
                        response.AF_ScreenMessage_o = "成功";
                        response.AG_PrintLine_o = "成功";

                        if (itemRecords != null && itemRecords.Length > 0)
                        {
                            XmlDocument dom = new XmlDocument();
                            try
                            {
                                dom.LoadXml(itemRecords[0]);

                                response.AQ_PermanentLocation_r = DomUtil.GetElementText(dom.DocumentElement, "location");

                                /*
                                 strPrice = DomUtil.GetElementText(item_dom.DocumentElement, "price");
                                 strBookType = DomUtil.GetElementText(item_dom.DocumentElement, "bookType");
                                 string strReturnDate = DomUtil.GetAttr(dom.DocumentElement, "borrowHistory/borrower", "returnDate");
                                 if (String.IsNullOrEmpty(strReturnDate) == false)
                                     strReturnDate = DateTimeUtil.Rfc1123DateTimeStringToLocal(strReturnDate, this.DateFormat);
                                 else
                                    strReturnDate = DateTime.Now.ToString(this.DateFormat);
                                */
                            }
                            catch (Exception ex)
                            {
                                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                            }
                        }
                    }

                    if (biblioRecords != null && biblioRecords.Length > 0)
                    {
                        string strTitle = String.Empty;
                        MarcRecord record = MarcXml2MarcRecord(biblioRecords[0], out string strMarcSyntax, out strError);
                        if (record != null)
                        {
                            if (strMarcSyntax == "unimarc")
                                response.AJ_TitleIdentifier_o = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
                            else if (strMarcSyntax == "usmarc")
                                response.AJ_TitleIdentifier_o = record.select("field[@name='245']/subfield[@name='a']").FirstContent;
                        }
                        else
                        {
                            strError = "书目信息解析错误：" + strError;
                            // LibraryManager.Log?.Error(strError);
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        lRet = info.LibraryChannel.GetBiblioSummary(
                            strItemBarcode,
                            "",
                            "",
                            out string strBiblioRecPath,
                            out string strSummary,
                            out strError);
                        if (-1 != lRet)
                            response.AJ_TitleIdentifier_o = strSummary;
                    }
                }

                return response.ToText();
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                goto ERROR1;
            }
            finally
            {
                // instance.MessageConnection.ReturnChannel(library_channel);
                EndFunction(info);
            }

        ERROR1:
            sip_channel.LocationCode = "";
            sip_channel.UserName = "";
            sip_channel.Password = "";

            LibraryManager.Log?.Info("Checkin() error: " + strError);
            return response.ToText();
        }

        /// <summary>
        /// 借书
        /// </summary>
        /// <param name="sip_channel">SIP 通道</param>
        /// <param name="message">SIP 请求消息</param>
        /// <returns></returns>
        static string Checkout(
            SipChannel sip_channel,
            string message)
        {
            int nRet = 0;
            long lRet = 0;
            string strError = "";

            CheckoutResponse_12 response = new CheckoutResponse_12()
            {
                Ok_1 = "0",
                RenewalOk_1 = "N",
                MagneticMedia_1 = "N",
                Desensitize_1 = "Y",
                TransactionDate_18 = SIPUtility.NowDateTime,
                AO_InstitutionId_r = "dp2Library",
                AJ_TitleIdentifier_r = string.Empty,
                AH_DueDate_r = string.Empty,
            };

            Checkout_11 request = new Checkout_11();
            try
            {
                nRet = request.parse(message, out strError);
                if (-1 == nRet)
                    goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                goto ERROR1;
            }

            string strItemIdentifier = request.AB_ItemIdentifier_r;
            string strPatronIdentifier = request.AA_PatronIdentifier_r;
            if (String.IsNullOrEmpty(strItemIdentifier)
                || String.IsNullOrEmpty(strPatronIdentifier))
            {
                strError = "读者标识和图书标识都不能是空值";
                response.AF_ScreenMessage_o = strError;
                response.AG_PrintLine_o = strError;
                goto ERROR1;
            }
            response.AB_ItemIdentifier_r = strItemIdentifier;
            response.AA_PatronIdentifier_r = strPatronIdentifier;

            FunctionInfo info = BeginFunction(sip_channel);
            if (string.IsNullOrEmpty(info.ErrorInfo) == false)
            {
                strError = info.ErrorInfo;
                goto ERROR1;
            }
            try
            {
                string strCancel = request.BI_Cancel_1_o;
                if (String.IsNullOrEmpty(strCancel) || "N" == strCancel)
                {
                    string strPatronPassword = request.AD_PatronPassword_o;
                    if (!string.IsNullOrEmpty(strPatronPassword))
                    {
                        lRet = info.LibraryChannel.VerifyReaderPassword(
                            strPatronIdentifier,
                            strPatronPassword,
                            out strError);
                        if (-1 == lRet)
                        {
                            response.AF_ScreenMessage_o = "校验密码发生错误：" + strError;
                        }
                        else if (0 == lRet)
                        {
                            response.AF_ScreenMessage_o = "失败：密码错误";
                        }

                        return response.ToText();
                    }

                    string[] aDupPath = null;
                    string[] item_records = null;
                    string[] reader_records = null;
                    string[] biblio_records = null;
                    string strOutputReaderBarcode = "";
                    DigitalPlatform.LibraryClient.localhost.BorrowInfo borrow_info = null;
                    lRet = info.LibraryChannel.Borrow(
                        false,  // 续借为 true
                        strPatronIdentifier,    //读者证条码号
                        strItemIdentifier,     // 册条码号
                        null, //strConfirmItemRecPath,
                        false,
                        null,   // this.OneReaderItemBarcodes,
                        "auto_renew,biblio,item", // strStyle, // auto_renew,biblio,item  //  "reader,item,biblio", // strStyle,
                        "xml:noborrowhistory",  // strItemReturnFormats,
                        out item_records,
                        "summary",    // strReaderFormatList
                        out reader_records,
                        "xml",         //strBiblioReturnFormats,
                        out biblio_records,
                        out aDupPath,
                        out strOutputReaderBarcode,
                        out borrow_info,
                        out strError);
                    if (-1 == lRet)
                    {
                        response.AF_ScreenMessage_o = "失败：" + strError;
                    }
                    else
                    {
                        response.Ok_1 = "1";

                        string strBiblioSummary = String.Empty;
                        string strMarcSyntax = "";
                        MarcRecord record = MarcXml2MarcRecord(biblio_records[0],
                            out strMarcSyntax,
                            out strError);
                        if (record != null)
                        {
                            if (strMarcSyntax == "unimarc")
                            {
                                strBiblioSummary = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
                            }
                            else if (strMarcSyntax == "usmarc")
                            {
                                strBiblioSummary = record.select("field[@name='245']/subfield[@name='a']").FirstContent;
                            }
                        }
                        else
                        {
                            strError = "书目信息解析错误：" + strError;
                            // LogManager.Logger.Error(strError);
                            goto ERROR1;
                        }

                        if (String.IsNullOrEmpty(strBiblioSummary))
                            strBiblioSummary = strItemIdentifier;

                        response.AJ_TitleIdentifier_r = strBiblioSummary;

                        string strLatestReturnTime = DateTimeUtil.Rfc1123DateTimeStringToLocal(borrow_info.LatestReturnTime,
                            info.Instance.sip_host.GetSipParam(sip_channel.UserName).DateFormat);
                        response.AH_DueDate_r = strLatestReturnTime;

                        response.AF_ScreenMessage_o = "成功";
                        response.AG_PrintLine_o = "成功";
                    }
                }
                return response.ToText();
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                goto ERROR1;
            }
            finally
            {
                EndFunction(info);
            }

        ERROR1:
            response.AF_ScreenMessage_o = strError;
            return response.ToText();
        }

        /// <summary>
        /// 图书信息
        /// 图书状态
        ///  1		other -- 其他
        ///  2		on order -- 订购中
        ///  3		available -- 可借
        ///  4		charged -- 在借
        /// 12		lost -- 丢失
        /// 13		missing -- 没有找到
        ///  5		charged; not to be recalled until earliest recall date -- 在借
        ///  6		in process -- 
        ///  7		recalled -- 召回
        ///  8		waiting on hold shelf -- 等待上架
        ///  9		waiting to be re-shelved -- 倒架中
        /// 10		in transit between library locations
        /// 11		claimed returned
        /// </summary>
        /// <param name="sip_channel">ILS 通道</param>
        /// <param name="message">SIP消息</param>
        /// <returns></returns>
        static string ItemInfo(SipChannel sip_channel, string message)
        {
            int nRet = 0;
            string strError = "";

            ItemInformationResponse_18 response = new ItemInformationResponse_18()
            {
                CirculationStatus_2 = "01",
                SecurityMarker_2 = "00",
                FeeType_2 = "01",
                TransactionDate_18 = SIPUtility.NowDateTime,
                CK_MediaType_o = SIPConst.MEDIA_TYPE_BOOK,
            };

            ItemInformation_17 request = new ItemInformation_17();
            try
            {
                nRet = request.parse(message, out strError);
                if (-1 == nRet)
                {
                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                    goto ERROR1;
                }
            }
            catch (Exception ex)
            {
                response.AF_ScreenMessage_o = ex.Message;
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                goto ERROR1;
            }

            FunctionInfo info = BeginFunction(sip_channel);
            if (string.IsNullOrEmpty(info.ErrorInfo) == false)
            {
                strError = info.ErrorInfo;
                goto ERROR1;
            }
            try
            {
                string strItemIdentifier = request.AB_ItemIdentifier_r;
                response.AB_ItemIdentifier_r = strItemIdentifier;
                string strItemXml = "";
                string strBiblio = "";
            REDO:
                long lRet = info.LibraryChannel.GetItemInfo(
                    strItemIdentifier,
                    "xml",
                    out strItemXml,
                    "xml",
                    out strBiblio,
                    out strError);
                if (-1 >= lRet)
                {
                    if (info.LibraryChannel.ErrorCode == ErrorCode.ChannelReleased)
                        goto REDO;

                    response.CirculationStatus_2 = "01";

                    strError = "获得'" + strItemIdentifier + "'发生错误: " + strError;
                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                }
                else if (0 == lRet)
                {
                    response.CirculationStatus_2 = "13";

                    strError = strItemIdentifier + " 记录不存在";
                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                }
                else if (1 < lRet)
                {
                    response.CirculationStatus_2 = "01";
                    strError = strItemIdentifier + " 记录重复，需馆员处理";
                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                }
                else if (1 == lRet)
                {
                    string dateFormat = info.Instance.sip_host.GetSipParam(sip_channel.UserName).DateFormat;

                    if (GetItemInfoResponse(response,
    strItemXml,
    strBiblio,
    dateFormat,
    out strError) == -1)
                        goto ERROR1;
#if REMOVED
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strItemXml);

                        string strItemState = DomUtil.GetElementText(dom.DocumentElement, "state");
                        if (String.IsNullOrEmpty(strItemState))
                        {
                            string strBorrower = DomUtil.GetElementText(dom.DocumentElement, "borrower");
                            response.CirculationStatus_2 = String.IsNullOrEmpty(strBorrower) ? "03" : "04";

                            XmlNodeList reservations = dom.DocumentElement.SelectNodes("reservations/request");
                            if (reservations != null)
                            {
                                response.CF_HoldQueueLength_o = reservations.Count.ToString();

                                if (reservations.Count > 0)
                                    response.CirculationStatus_2 = "08"; // 预约保留架                                
                            }
                        }
                        else
                        {
                            if (StringUtil.IsInList("丢失", strItemState))
                                response.CirculationStatus_2 = "12";
                        }

                        response.AQ_PermanentLocation_o = DomUtil.GetElementText(dom.DocumentElement, "location");

                        // 2018/12/25 根据设备厂家的建议，用CH字段存放索取号
                        response.CH_ItemProperties_o = DomUtil.GetElementText(dom.DocumentElement, "accessNo");

                        string strBorrowDate = DomUtil.GetElementText(dom.DocumentElement, "borrowDate");
                        if (!String.IsNullOrEmpty(strBorrowDate))
                        {
                            strBorrowDate = DateTimeUtil.Rfc1123DateTimeStringToLocal(strBorrowDate, "yyyyMMdd    HHmmss");
                            response.CM_HoldPickupDate_18 = strBorrowDate;
                        }

                        string strReturningDate = DomUtil.GetElementText(dom.DocumentElement, "returningDate");
                        if (!String.IsNullOrEmpty(strReturningDate))
                        {
                            strReturningDate = DateTimeUtil.Rfc1123DateTimeStringToLocal(strReturningDate,
                                info.Instance.sip_host.GetSipParam(sip_channel.UserName).DateFormat);
                            response.AH_DueDate_o = strReturningDate;
                        }

                        string strMarcSyntax = "";
                        MarcRecord record = MarcXml2MarcRecord(strBiblio, out strMarcSyntax, out strError);
                        if (record != null)
                        {
                            if (strMarcSyntax == "unimarc")
                            {
                                // strISBN = record.select("field[@name='010']/subfield[@name='a']").FirstContent;
                                response.AJ_TitleIdentifier_r = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
                                // strAuthor = record.select("field[@name='200']/subfield[@name='f']").FirstContent;
                            }
                            else if (strMarcSyntax == "usmarc")
                            {
                                // strISBN = record.select("field[@name='020']/subfield[@name='a']").FirstContent;
                                response.AJ_TitleIdentifier_r = record.select("field[@name='245']/subfield[@name='a']").FirstContent;
                                // strAuthor = record.select("field[@name='245']/subfield[@name='c']").FirstContent;
                            }
                        }
                        else
                        {
                            strError = "图书信息解析错误:" + strError;
                            LibraryManager.Log?.Error(strError);

                            response.AF_ScreenMessage_o = strError;
                            response.AG_PrintLine_o = strError;
                        }
                    }
                    catch (Exception ex)
                    {
                        strError = strItemIdentifier + ":图书解析错误:" + ExceptionUtil.GetDebugText(ex);
                        LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                        goto ERROR1;
                    }

#endif
                }

                return response.ToText();
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                goto ERROR1;
            }
            finally
            {
                EndFunction(info);
            }

        ERROR1:
            LibraryManager.Log?.Info("ItemInfo() error: " + strError);
            response.AF_ScreenMessage_o = strError;
            return response.ToText();
        }

        // 构造 ItemInfo 响应
        static int GetItemInfoResponse(ItemInformationResponse_18 response,
            string strItemXml,
            string strBiblio,
            string dateFormat,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);

                // 2020/12/8
                // 取记录中真实的册条码号。可能和检索发起的号码不同
                string strItemIdentifier = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                if (string.IsNullOrEmpty(strItemIdentifier))
                {
                    strItemIdentifier = DomUtil.GetElementText(dom.DocumentElement, "refID");
                    if (string.IsNullOrEmpty(strItemIdentifier) == false)
                        strItemIdentifier = "@refID:" + strItemIdentifier;
                }

                string strItemState = DomUtil.GetElementText(dom.DocumentElement, "state");
                if (String.IsNullOrEmpty(strItemState))
                {
                    string strBorrower = DomUtil.GetElementText(dom.DocumentElement, "borrower");
                    response.CirculationStatus_2 = String.IsNullOrEmpty(strBorrower) ? "03" : "04";

                    XmlNodeList reservations = dom.DocumentElement.SelectNodes("reservations/request");
                    if (reservations != null)
                    {
                        response.CF_HoldQueueLength_o = reservations.Count.ToString();

                        if (reservations.Count > 0)
                            response.CirculationStatus_2 = "08"; // 预约保留架 ??                               
                    }
                }
                else
                {
                    if (StringUtil.IsInList("丢失", strItemState))
                        response.CirculationStatus_2 = "12";
                }

                // 永久位置
                // permanent location	AQ	可选	 图书永久馆藏地。
                response.AQ_PermanentLocation_o = DomUtil.GetElementText(dom.DocumentElement, "location");

                // current location	AP	可选	 图书当前馆藏地。
                string currentLocation = DomUtil.GetElementText(dom.DocumentElement, "currentLocation");
                string currentShelfNo = "";
                if (currentLocation != null && currentLocation.Contains(":"))
                {
                    var parts = StringUtil.ParseTwoPart(currentLocation, ":");
                    currentLocation = parts[0];
                    currentShelfNo = parts[1];
                }
                if (string.IsNullOrEmpty(currentLocation) == false)
                    response.AP_CurrentLocation_o = currentLocation;

                // current shelf no	KP	可选	当前架位号，dp2扩展字段。
                if (string.IsNullOrEmpty(currentShelfNo) == false)
                    response.KP_CurrentShelfNo_o = currentShelfNo;

                // permanent shelf no	KQ	可选 	永久架位号，dp2扩展字段。
                response.KQ_PermanentShelfNo_o = DomUtil.GetElementText(dom.DocumentElement, "shelfNo");

                // 2018/12/25 根据设备厂家的建议，用CH字段存放索取号
                // response.CH_ItemProperties_o = DomUtil.GetElementText(dom.DocumentElement, "accessNo");
                // 2020/12/8 改用 KC 返回索取号
                response.KC_CallNo_o = DomUtil.GetElementText(dom.DocumentElement, "accessNo");

                // 2020/12/8 Owner Institution
                response.BG_Owner_o = DomUtil.GetElementText(dom.DocumentElement, "oi");

                string strBorrowDate = DomUtil.GetElementText(dom.DocumentElement, "borrowDate");
                if (!String.IsNullOrEmpty(strBorrowDate))
                {
                    strBorrowDate = DateTimeUtil.Rfc1123DateTimeStringToLocal(strBorrowDate, "yyyyMMdd    HHmmss");
                    response.CM_HoldPickupDate_18 = strBorrowDate;
                }

                string strReturningDate = DomUtil.GetElementText(dom.DocumentElement, "returningDate");
                if (!String.IsNullOrEmpty(strReturningDate))
                {
                    strReturningDate = DateTimeUtil.Rfc1123DateTimeStringToLocal(strReturningDate,
                        dateFormat /*info.Instance.sip_host.GetSipParam(sip_channel.UserName).DateFormat*/);
                    response.AH_DueDate_o = strReturningDate;
                }

                string strMarcSyntax = "";
                MarcRecord record = MarcXml2MarcRecord(strBiblio, out strMarcSyntax, out strError);
                if (record != null)
                {
                    if (strMarcSyntax == "unimarc")
                    {
                        // strISBN = record.select("field[@name='010']/subfield[@name='a']").FirstContent;
                        response.AJ_TitleIdentifier_r = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
                        // strAuthor = record.select("field[@name='200']/subfield[@name='f']").FirstContent;
                    }
                    else if (strMarcSyntax == "usmarc")
                    {
                        // strISBN = record.select("field[@name='020']/subfield[@name='a']").FirstContent;
                        response.AJ_TitleIdentifier_r = record.select("field[@name='245']/subfield[@name='a']").FirstContent;
                        // strAuthor = record.select("field[@name='245']/subfield[@name='c']").FirstContent;
                    }
                }
                else
                {
                    strError = "图书信息解析错误:" + strError;
                    LibraryManager.Log?.Error(strError);

                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = $"册记录 XML 解析出现异常: {ex.Message}";
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                return -1;
            }
        }

        static string ItemStatusUpdate(SipChannel sip_channel, string message)
        {
            int nRet = 0;
            string strError = "";

            ItemStatusUpdateResponse_20 response = new ItemStatusUpdateResponse_20()
            {
                ItemPropertiesOk_1 = "0",
                //AB_ItemIdentifier_r = "",
                //AJ_TitleIdentifier_o = "",
                TransactionDate_18 = SIPUtility.NowDateTime,
            };

            ItemStatusUpdate_19 request = new ItemStatusUpdate_19();
            try
            {
                nRet = request.parse(message, out strError);
                if (-1 == nRet)
                {
                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                    goto ERROR1;
                }
            }
            catch (Exception ex)
            {
                response.AF_ScreenMessage_o = ex.Message;
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                goto ERROR1;
            }

            FunctionInfo info = BeginFunction(sip_channel);
            if (string.IsNullOrEmpty(info.ErrorInfo) == false)
            {
                strError = info.ErrorInfo;
                goto ERROR1;
            }
            try
            {
                string strItemIdentifier = request.AB_ItemIdentifier_r;
                response.AB_ItemIdentifier_r = strItemIdentifier;
                string strItemXml = "";
                string strBiblio = "";

            REDO:
                long lRet = info.LibraryChannel.GetItemInfo(
                    "item",
                    strItemIdentifier,
                    "",
                    "xml",
                    out strItemXml,
                    out string item_recpath,
                    out byte[] item_timestamp,
                    "xml",
                    out strBiblio,
                    out string biblio_recpath,
                    out strError);
                if (-1 >= lRet)
                {
                    if (info.LibraryChannel.ErrorCode == ErrorCode.ChannelReleased)
                        goto REDO;

                    response.ItemPropertiesOk_1 = "0";

                    strError = "获得'" + strItemIdentifier + "'发生错误: " + strError;
                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                }
                else if (0 == lRet)
                {
                    response.ItemPropertiesOk_1 = "0";

                    strError = strItemIdentifier + " 记录不存在";
                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                }
                else if (1 < lRet)
                {
                    response.ItemPropertiesOk_1 = "0";
                    strError = strItemIdentifier + " 记录重复，需馆员处理";
                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                }
                else if (1 == lRet)
                {
                    string dateFormat = info.Instance.sip_host.GetSipParam(sip_channel.UserName).DateFormat;

                    if (GetItemStatusUpdateResponse(
                        info,
                        request,
                        response,
                        strItemXml,
                        strBiblio,
                        dateFormat,
                        out strError) == -1)
                        goto ERROR1;
                }

                return response.ToText();
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                goto ERROR1;
            }
            finally
            {
                EndFunction(info);
            }

        ERROR1:
            LibraryManager.Log?.Info("ItemStatusUpdate() error: " + strError);
            response.ItemPropertiesOk_1 = "0";
            response.AF_ScreenMessage_o = strError;
            return response.ToText();
        }

        static string _libraryServerVersion = "";

        // 构造 ItemStatusUpdate 响应
        static int GetItemStatusUpdateResponse(
            FunctionInfo info,
            ItemStatusUpdate_19 request,
            ItemStatusUpdateResponse_20 response,
            string strItemXml,
            string strBiblio,
            string dateFormat,
            out string strError)
        {
            strError = "";
            // response.ItemPropertiesOk_1 = "1";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);

                // 2020/12/8
                // 取记录中真实的册条码号。可能和检索发起的号码不同
                string strItemIdentifier = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                if (string.IsNullOrEmpty(strItemIdentifier))
                {
                    strItemIdentifier = DomUtil.GetElementText(dom.DocumentElement, "refID");
                    if (string.IsNullOrEmpty(strItemIdentifier) == false)
                        strItemIdentifier = "@refID:" + strItemIdentifier;
                }
                response.AB_ItemIdentifier_r = strItemIdentifier;

                string formats = "";    // "item,biblio";

                // currentLocation 元素内容。格式为 馆藏地:架号
                // 注意馆藏地和架号字符串里面不应包含逗号和冒号
                List<string> commands = new List<string>();

                // currentLocationLong 由 currentLocation 和 currentShelfNo 合成
                string currentLocationLong = null;
                if (request.AP_CurrentLocation_o != null
                    || request.KP_CurrentShelfNo_o != null)
                {
                    if (request.AP_CurrentLocation_o != null)
                        currentLocationLong = request.AP_CurrentLocation_o;
                    else
                        currentLocationLong = "*";
                    if (request.KP_CurrentShelfNo_o != null)
                        currentLocationLong += ":" + request.KP_CurrentShelfNo_o;
                    else
                        currentLocationLong += ":*";
                }

                if (currentLocationLong != null)
                {
                    // 如果 currentLocationLong 包含星号，要检查 dp2library 版本是否为 3.40 以上
                    if (currentLocationLong.Contains("*"))
                    {
                        if (string.IsNullOrEmpty(_libraryServerVersion))
                        {
                            long lRet = info.LibraryChannel.GetVersion(
    out string version,
    out string uid,
    out strError);
                            if (lRet == -1)
                            {
                                strError = $"检查 dp2library 服务器版本号时出错: {strError}";
                                return -1;
                            }
                            _libraryServerVersion = version;
                        }

                        string base_version = "3.40";
                        if (StringUtil.CompareVersion(_libraryServerVersion, base_version) < 0)
                        {
                            strError = $"dp2Capo 和 dp2Library { base_version} 或以上版本配套使用 (而当前 dp2Library 版本号为 { _libraryServerVersion }。请尽快升级 dp2Library 到最新版本";
                            return 0;
                        }
                    }

                    commands.Add($"currentLocation:{StringUtil.EscapeString(currentLocationLong, ":,")}");
                }

                if (request.AQ_PermanentLocation_o != null)
                    commands.Add($"location:{StringUtil.EscapeString(request.AQ_PermanentLocation_o, ":,")}");
                if (request.KQ_PermanentShelfNo_o != null)
                    commands.Add($"shelfNo:{StringUtil.EscapeString(request.KQ_PermanentShelfNo_o, ":,")}");
                /*
                if (string.IsNullOrEmpty(info.BatchNo) == false)
                {
                    commands.Add($"batchNo:{StringUtil.EscapeString(info.BatchNo, ":,")}");
                    // 2020/10/14
                    // 即便册记录没有发生修改，也要产生 transfer 操作日志记录。这样便于进行典藏移交清单统计打印
                    commands.Add("forceLog");
                }
                */
                string style = $"{formats},{StringUtil.MakePathList(commands, ",")}"; // style,

                {
                    // 修改册记录，然后保存回去
                    long lRet = info.LibraryChannel.Return(
                        "transfer",
                        "",
                        strItemIdentifier,
                        "", // strConfirmItemRecPath,
                        false,
                        style,
                        "",
                        out string[] item_records,
                        "",
                        out string[] patron_records,
                        "",
                        out string[] biblio_records,
                        out string[] dup_path,
                        out string output_readerBarcode,
                        out DigitalPlatform.LibraryClient.localhost.ReturnInfo return_info,
                        out strError);
                    if (lRet == -1 
                        && info.LibraryChannel.ErrorCode != ErrorCode.NotChanged)
                        return -1;
                }

                string strMarcSyntax = "";
                MarcRecord record = MarcXml2MarcRecord(strBiblio, out strMarcSyntax, out strError);
                if (record != null)
                {
                    if (strMarcSyntax == "unimarc")
                    {
                        // strISBN = record.select("field[@name='010']/subfield[@name='a']").FirstContent;
                        response.AJ_TitleIdentifier_o = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
                        // strAuthor = record.select("field[@name='200']/subfield[@name='f']").FirstContent;
                    }
                    else if (strMarcSyntax == "usmarc")
                    {
                        // strISBN = record.select("field[@name='020']/subfield[@name='a']").FirstContent;
                        response.AJ_TitleIdentifier_o = record.select("field[@name='245']/subfield[@name='a']").FirstContent;
                        // strAuthor = record.select("field[@name='245']/subfield[@name='c']").FirstContent;
                    }
                }
                else
                {
                    strError = "图书信息解析错误:" + strError;
                    LibraryManager.Log?.Error(strError);

                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = $"册记录 XML 解析出现异常: {ex.Message}";
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                return -1;
            }
        }


        /// <summary>
        /// 续借
        /// </summary>
        /// <param name="sip_channel">SIP 通道</param>
        /// <param name="message">SIP 请求消息</param>
        /// <returns></returns>
        static string Renew(SipChannel sip_channel, string message)
        {
            int nRet = 0;
            long lRet = 0;

            string strError = "";

            RenewResponse_30 response = new RenewResponse_30()
            {
                Ok_1 = "0",
                RenewalOk_1 = "N",
                MagneticMedia_1 = "N",
                Desensitize_1 = "N",
                TransactionDate_18 = SIPUtility.NowDateTime,
                AO_InstitutionId_r = "dp2Library",
                AJ_TitleIdentifier_r = string.Empty,
                AH_DueDate_r = string.Empty,
            };

            Renew_29 request = new Renew_29();
            try
            {
                nRet = request.parse(message, out strError);
                if (-1 == nRet)
                {
                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                    return response.ToText();
                }
            }
            catch (Exception ex)
            {
                response.AF_ScreenMessage_o = ex.Message;
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                return response.ToText();
            }

            FunctionInfo info = BeginFunction(sip_channel);
            if (string.IsNullOrEmpty(info.ErrorInfo) == false)
            {
                strError = info.ErrorInfo;
                goto ERROR1;
            }
            try
            {
                string strItemIdentifier = request.AB_ItemIdentifier_o;
                string strPatronIdentifier = request.AA_PatronIdentifier_r;
                if (String.IsNullOrEmpty(strItemIdentifier)
                    || String.IsNullOrEmpty(strPatronIdentifier))
                {
                    strError = "读者标识和图书标识都不能是空值";
                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;

                    return response.ToText();
                }

                response.AA_PatronIdentifier_r = strPatronIdentifier;
                response.AB_ItemIdentifier_r = strItemIdentifier;

                string strPatronPassword = request.AD_PatronPassword_o;
                if (!string.IsNullOrEmpty(strPatronPassword))
                {
                    lRet = info.LibraryChannel.VerifyReaderPassword(
                        strPatronIdentifier,
                        strPatronPassword,
                        out strError);
                    if (-1 == lRet)
                    {
                        response.AF_ScreenMessage_o = "校验密码发生错误：" + strError;
                    }
                    else if (0 == lRet)
                    {
                        response.AF_ScreenMessage_o = "失败：密码错误";
                    }

                    return response.ToText();
                }


                string[] aDupPath = null;
                string[] item_records = null;
                string[] reader_records = null;
                string[] biblio_records = null;
                string strOutputReaderBarcode = "";
                lRet = info.LibraryChannel.Borrow(
                    true,  // 续借为 true
                    strPatronIdentifier,    //读者证条码号
                    strItemIdentifier,     // 册条码号
                    null, //strConfirmItemRecPath,
                    false,
                    null,   // this.OneReaderItemBarcodes,
                    "auto_renew,biblio,item", // strStyle, // auto_renew,biblio,item                   //  "reader,item,biblio", // strStyle,
                    "xml:noborrowhistory",  // strItemReturnFormats,
                    out item_records,
                    "summary",    // strReaderFormatList
                    out reader_records,
                    "xml",         //strBiblioReturnFormats,
                    out biblio_records,
                    out aDupPath,
                    out strOutputReaderBarcode,
                    out DigitalPlatform.LibraryClient.localhost.BorrowInfo borrow_info,
                    out strError);
                if (-1 == lRet)
                {
                    response.AF_ScreenMessage_o = "失败：" + strError;
                }
                else
                {
                    response.Ok_1 = "1";
                    response.RenewalOk_1 = "Y";

                    string strBiblioSummary = String.Empty;
                    string strMarcSyntax = "";
                    MarcRecord record = MarcXml2MarcRecord(biblio_records[0],
                        out strMarcSyntax,
                        out strError);
                    if (record != null)
                    {
                        if (strMarcSyntax == "unimarc")
                        {
                            strBiblioSummary = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
                        }
                        else if (strMarcSyntax == "usmarc")
                        {
                            strBiblioSummary = record.select("field[@name='245']/subfield[@name='a']").FirstContent;
                        }
                    }
                    else
                    {
                        strError = "书目信息解析错误：" + strError;
                        LibraryManager.Log?.Error(strError);
                    }

                    if (String.IsNullOrEmpty(strBiblioSummary))
                        strBiblioSummary = strItemIdentifier;

                    response.AJ_TitleIdentifier_r = strBiblioSummary;

                    string strLatestReturnTime = DateTimeUtil.Rfc1123DateTimeStringToLocal(borrow_info.LatestReturnTime,
                        info.Instance.sip_host.GetSipParam(sip_channel.UserName).DateFormat);
                    response.AH_DueDate_r = strLatestReturnTime;

                    response.AF_ScreenMessage_o = "成功";
                    response.AG_PrintLine_o = "成功";
                }
                return response.ToText();
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                goto ERROR1;
            }
            finally
            {
                EndFunction(info);
            }

        ERROR1:
            LibraryManager.Log?.Info("Renew() error: " + strError);
            response.AF_ScreenMessage_o = strError;
            return response.ToText();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">SIP消息</param>
        /// <returns></returns>
        static string EndPatronSession(string message)
        {
            int nRet = 0;
            string strError = "";

            EndSessionResponse_36 response = new EndSessionResponse_36()
            {
                EndSession_1 = "N",
                TransactionDate_18 = SIPUtility.NowDateTime,
                AO_InstitutionId_r = "dp2Library",
            };

            EndPatronSession_35 request = new EndPatronSession_35();
            try
            {
                nRet = request.parse(message, out strError);
                if (-1 == nRet)
                {
                    LibraryManager.Log?.Error(strError);
                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                }
                else
                {
                    response.AA_PatronIdentifier_r = request.AA_PatronIdentifier_r;
                    response.AF_ScreenMessage_o = "结束操作成功";
                    response.AG_PrintLine_o = "结束操作成功";
                }
            }
            catch (Exception ex)
            {
                response.AF_ScreenMessage_o = ex.Message;
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
            }

            return response.ToText();
        }

        // 交费
        static string Amerce(SipChannel sip_channel, string message)
        {
            int nRet = 0;
            long lRet = 0;
            string strError = "";

            // 初始化返回的 38命令
            FeePaidResponse_38 response = new FeePaidResponse_38()
            {
                PaymentAccepted_1 = "N",
                TransactionDate_18 = SIPUtility.NowDateTime,

                AO_InstitutionId_r = "dp2Library",
                AA_PatronIdentifier_r = string.Empty,
                BK_TransactionId_o = string.Empty,
                AF_ScreenMessage_o = string.Empty,
                AG_PrintLine_o = string.Empty,
            };

            // 解析37命令
            FeePaid_37 request = new FeePaid_37();
            try
            {
                nRet = request.parse(message, out strError);
                if (-1 == nRet)
                {
                    response.AF_ScreenMessage_o = strError;
                    return response.ToText();
                }
            }
            catch (Exception ex)
            {
                response.AF_ScreenMessage_o = ex.Message;
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                return response.ToText();
            }

            FunctionInfo info = BeginFunction(sip_channel);
            if (string.IsNullOrEmpty(info.ErrorInfo) == false)
            {
                strError = info.ErrorInfo;
                goto ERROR1;
            }
            try
            {
                string strMessage = "";

                string strPatronIdentifier = request.AA_PatronIdentifier_r;

                // 先查到读者记录
                string[] results = null;
                lRet = info.LibraryChannel.GetReaderInfo(
                    strPatronIdentifier, //读者卡号,
                    "advancexml",   // this.RenderFormat, // "html",
                    out results,
                    out strError);
                if (lRet <= -1)
                {
                    strError = "查询读者信息失败：" + strError;
                    goto ERROR1;
                }
                else if (lRet == 0)
                {
                    strError = "查无此证";
                    goto ERROR1;
                }
                else if (lRet > 1)
                {
                    strError = "证号重复";
                    goto ERROR1;
                }

                XmlDocument dom = new XmlDocument();
                string strReaderXml = results[0];
                try
                {
                    dom.LoadXml(strReaderXml);
                }
                catch (Exception ex)
                {
                    LibraryManager.Log?.Error("读者信息解析错误：" + ExceptionUtil.GetDebugText(ex));
                    response.AF_ScreenMessage_o = "读者信息解析错误";
                    response.AG_PrintLine_o = "读者信息解析错误";
                    return response.ToText();
                }

                decimal feeAmount = 0;
                try
                {
                    feeAmount = Convert.ToDecimal(request.BV_FeeAmount_r);
                }
                catch (Exception ex)
                {
                    strError = "传来的金额字符串 '" + request.BV_FeeAmount_r + "' 不合法: " + ex.Message;
                    goto ERROR1;
                }

                // 交费项
                List<AmerceItem> amerce_itemList = new List<AmerceItem>();
                XmlNodeList overdues = dom.DocumentElement.SelectNodes("overdues/overdue");
                if (overdues == null || overdues.Count == 0)
                {
                    strError = "当前读者没有欠款";
                    goto ERROR1;
                }

                List<string> prices = new List<string>();
                foreach (XmlNode node in overdues)
                {
                    string strID = DomUtil.GetAttr(node, "id");
                    string price = DomUtil.GetAttr(node, "price");
                    prices.Add(price);

                    AmerceItem amerceItem = new AmerceItem();
                    amerceItem.ID = strID;
                    amerceItem.NewPrice = price;
                    amerceItem.NewComment = "自助机交费";
                    amerce_itemList.Add(amerceItem);
                }

                // 累计欠款金额
                string totlePrice = PriceUtil.TotalPrice(prices);
                CurrencyItem currItem = null;
                nRet = PriceUtil.ParseSinglePrice(totlePrice, out currItem, out strError);
                if (nRet == -1)
                {
                    strMessage = "计算读者违约金额出错：" + strError;
                    response.AF_ScreenMessage_o = strMessage;
                    response.AG_PrintLine_o = strMessage;
                    return response.ToText();
                }

                if (request.CurrencyType_3 != currItem.Prefix)
                {
                    strMessage = "货币类型不一致";
                    response.AF_ScreenMessage_o = strMessage;
                    response.AG_PrintLine_o = strMessage;
                    return response.ToText();
                }

                // 金额要与欠款总额保持一致
                if (feeAmount != currItem.Value)
                {
                    strMessage = "传来的金额应该与读者欠款总额完全一致";
                    response.AF_ScreenMessage_o = strMessage;
                    response.AG_PrintLine_o = strMessage;
                    return response.ToText();
                }


                // 对所有记录进行交费
                AmerceItem[] amerce_items = new AmerceItem[amerce_itemList.Count];
                amerce_itemList.CopyTo(amerce_items);
                AmerceItem[] failed_items = null;
                string patronXml = "";
                lRet = info.LibraryChannel.Amerce(
                   "amerce",
                   strPatronIdentifier,
                   amerce_items,
                   out failed_items,
                   out patronXml,
                   out strError);
                if (lRet == -1)
                {
                    response.AF_ScreenMessage_o = "失败：" + strError;
                }
                else
                {
                    if (failed_items != null && failed_items.Length > 0)
                    {
                        strMessage = "有" + failed_items.Length.ToString() + "个事项交费未成功。";
                        response.AF_ScreenMessage_o = strMessage;
                        response.AG_PrintLine_o = "交费成功";
                    }
                    else
                    {
                        response.PaymentAccepted_1 = "Y";
                        response.AF_ScreenMessage_o = "交费成功";
                        response.AG_PrintLine_o = "交费成功";
                    }
                }

                return response.ToText();
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                goto ERROR1;
            }
            finally
            {
                EndFunction(info);
            }

        ERROR1:
            LibraryManager.Log?.Info("Amerce() error: " + strError);
            response.AF_ScreenMessage_o = strError;
            response.AG_PrintLine_o = strError;
            return response.ToText();
        }

        /// <summary>
        /// 读者信息
        /// </summary>
        /// <param name="sip_channel">SIP 通道</param>
        /// <param name="message">SIP 请求消息</param>
        /// <returns></returns>
        static string PatronInfo(SipChannel sip_channel, string message)
        {
            string strMessage = "";
            string strError = "";
            int nRet = 0;
            long lRet = 0;

            char[] patronStatus = new char[14];
            for (int i = 0; i < patronStatus.Length; i++)
            {
                patronStatus[i] = (char)0x20; // 空格
            }

            PatronInformationResponse_64 response = new PatronInformationResponse_64()
            {
                PatronStatus_14 = "              ",
                Language_3 = SIPConst.LANGUAGE_CHINESE,
                TransactionDate_18 = SIPUtility.NowDateTime,
                HoldItemsCount_4 = "0000",
                OverdueItemsCount_4 = "0000",
                ChargedItemsCount_4 = "0000",
                FineItemsCount_4 = "0000",
                RecallItemsCount_4 = "0000",
                UnavailableHoldsCount_4 = "0000",
                AO_InstitutionId_r = "dp2Library",
                AA_PatronIdentifier_r = string.Empty,
                AE_PersonalName_r = string.Empty,

                BL_ValidPatron_o = "N",
                CQ_ValidPatronPassword_o = "N",
            };

            PatronInformation_63 request = new PatronInformation_63();
            try
            {
                nRet = request.parse(message, out strError);
                if (-1 == nRet)
                {
                    LibraryManager.Log?.Error(strError);
                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                    return response.ToText();
                }
            }
            catch (Exception ex)
            {
                response.AF_ScreenMessage_o = ex.Message;
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                return response.ToText();
            }

            FunctionInfo info = BeginFunction(sip_channel);
            if (string.IsNullOrEmpty(info.ErrorInfo) == false)
            {
                strError = info.ErrorInfo;
                goto ERROR1;
            }
            try
            {
                string strPassword = request.AD_PatronPassword_o;
                // 用于检索的证条码号或者证号等等。和读者记录中的 barcode 元素不一定相同
                string strQueryBarcode = request.AA_PatronIdentifier_r;
                if (!string.IsNullOrEmpty(strPassword))
                {
                    lRet = info.LibraryChannel.VerifyReaderPassword(
                        strQueryBarcode,
                        strPassword,
                        out strError);
                    if (lRet == -1)
                    {
                        response.AF_ScreenMessage_o = "校验密码发生错误：" + strError;
                        response.AG_PrintLine_o = "校验密码发生错误：" + strError;

                        return response.ToText();
                    }
                    else if (lRet == 0)
                    {
                        response.AF_ScreenMessage_o = "卡号或密码不正确";
                        response.AG_PrintLine_o = "卡号或密码不正确";
                        return response.ToText();
                    }

                    response.CQ_ValidPatronPassword_o = "Y";
                }

                string[] results = null;
                lRet = info.LibraryChannel.GetReaderInfo(
                    strQueryBarcode, //读者卡号,
                    "advancexml",   // this.RenderFormat, // "html",
                    out results,
                    out strError);
                if (lRet <= -1)
                {
                    strError = "查询读者('" + strQueryBarcode + "')信息出错：" + strError;
                    goto ERROR1;
                }
                else if (lRet == 0)
                {
                    strError = "查无此证 '" + strQueryBarcode + "'";
                    goto ERROR1;
                }
                else if (lRet > 1)
                {
                    strError = "证号重复 '" + strQueryBarcode + "'";
                    goto ERROR1;
                }

                XmlDocument dom = new XmlDocument();
                string strReaderXml = results[0];
                try
                {
                    dom.LoadXml(strReaderXml);
                }
                catch (Exception ex)
                {
                    LibraryManager.Log?.Error("读者信息解析错误：" + ExceptionUtil.GetDebugText(ex));
                    response.AF_ScreenMessage_o = "读者信息解析错误";
                    response.AG_PrintLine_o = "读者信息解析错误";
                    return response.ToText();
                }

                // hold items count 4 - char, fixed-length required field -- 预约
                XmlNodeList holdItemNodes = dom.DocumentElement.SelectNodes("reservations/request");
                if (holdItemNodes != null)
                {
                    response.HoldItemsCount_4 = holdItemNodes.Count.ToString().PadLeft(4, '0');

                    List<VariableLengthField> holdItems = new List<VariableLengthField>();
                    foreach (XmlNode node in holdItemNodes)
                    {
                        string strItemBarcode = DomUtil.GetAttr(node, "items");
                        if (string.IsNullOrEmpty(strItemBarcode))
                            continue;

                        if (strItemBarcode.IndexOf(',') != -1)
                        {
                            string[] barcodes = strItemBarcode.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string barcode in barcodes)
                            {
                                holdItems.Add(new VariableLengthField(SIPConst.F_AS_HoldItems, false, barcode));
                            }
                        }
                        else
                        {
                            holdItems.Add(new VariableLengthField(SIPConst.F_AS_HoldItems, false, strItemBarcode));
                        }
                    }

                    if (holdItems.Count > 0)
                        response.AS_HoldItems_o = holdItems;
                }

                // overdue items count 4 - char, fixed-length required field  -- 超期
                // charged items count 4 - char, fixed-length required field -- 在借
                XmlNodeList chargedItemNodes = dom.DocumentElement.SelectNodes("borrows/borrow");
                if (chargedItemNodes != null)
                {
                    response.ChargedItemsCount_4 = chargedItemNodes.Count.ToString().PadLeft(4, '0');

                    List<VariableLengthField> chargedItems = new List<VariableLengthField>();
                    List<VariableLengthField> overdueItems = new List<VariableLengthField>();
                    int nOverdueItemsCount = 0;
                    foreach (XmlNode node in chargedItemNodes)
                    {
                        string strItemBarcode = DomUtil.GetAttr(node, "barcode");
                        if (string.IsNullOrEmpty(strItemBarcode))
                            continue;


                        // 2018/12/25 ryh 如果是@refID:开头，尝试获取对应的索取号
                        if (strItemBarcode.IndexOf("@refID:") != -1)
                        {
                            string strItemXml = "";
                            string strBiblio = "";
                            lRet = info.LibraryChannel.GetItemInfo(
                               strItemBarcode,
                               "xml",
                               out strItemXml,
                               "xml",
                               out strBiblio,
                               out strError);
                            if (1 == lRet)
                            {
                                XmlDocument itemDom = new XmlDocument();
                                itemDom.LoadXml(strItemXml);

                                string registerNo = DomUtil.GetElementText(itemDom.DocumentElement, "registerNo");
                                strItemBarcode = registerNo;
                            }
                        }


                        chargedItems.Add(new VariableLengthField(SIPConst.F_AU_ChargedItems, false, strItemBarcode));

                        string strReturningDate = DomUtil.GetAttr(node, "returningDate");
                        if (string.IsNullOrEmpty(strReturningDate))
                            continue;
                        DateTime returningDate = DateTimeUtil.FromRfc1123DateTimeString(strReturningDate);
                        if (returningDate < DateTime.Now)
                        {
                            nOverdueItemsCount++;
                            overdueItems.Add(new VariableLengthField(SIPConst.F_AT_OverdueItems, false, strItemBarcode));
                        }
                    }

                    if (chargedItems.Count > 0)
                        response.AU_ChargedItems_o = chargedItems;
                    if (overdueItems.Count > 0)
                    {
                        patronStatus[6] = 'Y';
                        response.AT_OverdueItems_o = overdueItems;
                    }
                }

                // 超期交费项
                XmlNodeList overdues = dom.DocumentElement.SelectNodes("overdues/overdue");
                if (overdues != null && overdues.Count > 0)
                {
                    List<string> prices = new List<string>();

                    string strWords = "押金,租金";
                    string strWords2 = "超期,丢失";
                    foreach (XmlNode node in overdues)
                    {
                        string strReason = DomUtil.GetAttr(node, "reason");
                        string strPart = "";
                        if (strReason.Length > 2)
                            strPart = strReason.Substring(0, 2);
                        if (StringUtil.IsInList(strPart, strWords) && patronStatus[11] != 'Y')
                        {
                            patronStatus[11] = 'Y';
                        }
                        else if (StringUtil.IsInList(strPart, strWords2) && patronStatus[10] != 'Y')
                        {
                            patronStatus[10] = 'Y';
                        }

                        // 计算金额
                        string price = DomUtil.GetAttr(node, "price");
                        prices.Add(price);
                    }

                    // 累计欠款金额
                    string totlePrice = PriceUtil.TotalPrice(prices);
                    CurrencyItem currItem = null;
                    nRet = PriceUtil.ParseSinglePrice(totlePrice, out currItem, out strError);
                    if (nRet == -1)
                    {
                        strMessage = "计算读者违约金额时出错：" + strError;
                        response.AF_ScreenMessage_o = strMessage;
                        response.AG_PrintLine_o = strMessage;
                        return response.ToText();
                    }
                    response.BV_feeAmount_o = "-" + currItem.Value.ToString(); //设为负值
                    response.BH_CurrencyType_3 = currItem.Prefix;
                }

                string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");

                response.AA_PatronIdentifier_r = strBarcode;
                response.AE_PersonalName_r = DomUtil.GetElementText(dom.DocumentElement, "name");
                response.BL_ValidPatron_o = "Y";

                string strTotal = DomUtil.GetElementAttr(dom.DocumentElement, "info/item[@name='可借总册数']", "value");
                response.CB_ChargedItemsLimit_o = strTotal;
                string strBorrowsCount = DomUtil.GetElementAttr(dom.DocumentElement, "info/item[@name='当前还可借']", "value");
                response.BZ_HoldItemsLimit_o = strBorrowsCount.PadLeft(4, '0');
                string strState = DomUtil.GetElementText(dom.DocumentElement, "state");
                if (String.IsNullOrEmpty(strState))
                {
                    if (!string.IsNullOrEmpty(strTotal))
                    {
                        if (!string.IsNullOrEmpty(strBorrowsCount) && strBorrowsCount != "0")
                            strMessage = "您在本馆最多可借【" + strTotal + "】册，还可以再借【" + strBorrowsCount + "】册。";
                        else
                            strMessage = "您在本馆借书数已达最多可借数【" + strTotal + "】，不能继续借了!";
                    }
                    if (!string.IsNullOrEmpty(strMessage))
                    {
                        response.AF_ScreenMessage_o = strMessage;
                        response.AG_PrintLine_o = strMessage;
                    }
                }
                else
                {
                    patronStatus[4] = 'Y';

                    string strName = DomUtil.GetElementText(dom.DocumentElement, "name");
                    strMessage = "读者证[" + strBarcode + ":" + strName + "]已被[" + strState + "]";
                    response.AF_ScreenMessage_o = strMessage;
                    response.AG_PrintLine_o = strMessage;
                }
                response.PatronStatus_14 = new string(patronStatus);
                return response.ToText();
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                goto ERROR1;
            }
            finally
            {
                EndFunction(info);
            }

        ERROR1:
            LibraryManager.Log?.Info("PatronInfo() error: " + strError);
            response.AF_ScreenMessage_o = strError;
            response.AG_PrintLine_o = strError;
            return response.ToText();
        }

        static string SetReaderInfo(
            SipChannel sip_channel,
            string strSIP2Package)
        {
            string strBackMsg = "";
            // string strError = "";

            string strBarcode = "";
            string strOperation = "";
            string[] parts = strSIP2Package.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < parts.Length; i++)
            {
                string part = parts[i];
                if (part.Length < 2)
                    continue;

                string strValue = part.Substring(2);
                string str = part.Substring(0, 2);

                if (str == "AA")
                    strBarcode = strValue;
                else if (str == "XK")
                    strOperation = strValue;
                else
                    continue;

                if (String.IsNullOrEmpty(strBarcode) == false
                    && String.IsNullOrEmpty(strOperation) == false)
                    break;
            }

            if (String.IsNullOrEmpty(strOperation))
            {
                // nRet = 0;
                strBackMsg = "82" + SIPUtility.NowDateTime + "AOdp2Library|AA" + strBarcode +
                    "|XK" + strOperation +
                    "|OK0|AF修改读者记录发生错误，命令不对。|AG修改读者记录发生错误，命令不对。";
            }
            else if (strOperation == "01"
                || strOperation == "11"
                || strOperation == "02")
            {
                strBackMsg = DoSetReaderInfo(sip_channel, strSIP2Package);
            }
            else if (strOperation == "14")
            {
                strBackMsg = ChangePassword(sip_channel, strSIP2Package);
            }

            return strBackMsg;
        }

        static string DoSetReaderInfo(
            SipChannel sip_channel,
            string strSIP2Package)
        {
            string strBackMsg = "";
            string strError = "";

            long lRetValue = 0;

            string strMsg = ""; // 返回给SIP2的错误信息

            string strAction = "";
            string strReaderBarcode = "";
            string strIDCardNumber = ""; // 身份证号

            bool bForegift = false; // 是否创建押金
            string strForegiftValue = ""; // 押金金额

            string strPassword = "";

            string strOperation = "";
            StringBuilder sb = new StringBuilder(1024);
            sb.Append("82").Append(SIPUtility.NowDateTime).Append("AOdp2Library");

            FunctionInfo info = BeginFunction(sip_channel);
            if (string.IsNullOrEmpty(info.ErrorInfo) == false)
            {
                strError = info.ErrorInfo;
                goto ERROR1;
            }
            try
            {

                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");

                #region 处理SIP2通讯包，构造读者dom
                string[] parts = strSIP2Package.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 1; i < parts.Length; i++)
                {
                    string part = parts[i];
                    if (part.Length < 2)
                        continue;

                    string strValue = part.Substring(2);
                    switch (part.Substring(0, 2))
                    {
                        case "AA":
                            {
                                if (String.IsNullOrEmpty(strValue))
                                {
                                    strMsg = "办证时读者证号不能为空";
                                    goto ERROR1;
                                }
                                strReaderBarcode = strValue;
                                DomUtil.SetElementText(dom.DocumentElement, "barcode", strValue);
                                break;
                            }
                        case "XO":
                            {
                                if (String.IsNullOrEmpty(strValue))
                                {
                                    strMsg = "办证时身份证号不能为空";
                                    goto ERROR1;
                                }
                                strIDCardNumber = strValue;
                                DomUtil.SetElementText(dom.DocumentElement, "idCardNumber", strValue);
                                break;
                            }
                        case "AD":
                            {
                                strPassword = strValue;
                                break;
                            }
                        case "XF":
                            DomUtil.SetElementText(dom.DocumentElement, "comment", strValue);
                            break;
                        case "XT":
                            DomUtil.SetElementText(dom.DocumentElement, "readerType", strValue);
                            break;
                        case "BV":
                            {
                                strForegiftValue = strValue;
                                break;
                            }
                        case "AM": // 开户馆
                                   // DomUtil.SetElementText(dom.DocumentElement, "readerType", strValue);
                            break;
                        case "BD":
                            DomUtil.SetElementText(dom.DocumentElement, "address", strValue);
                            break;
                        case "XM":
                            {
                                if (strValue == "0")
                                    strValue = "女";
                                else
                                    strValue = "男";
                                DomUtil.SetElementText(dom.DocumentElement, "gender", strValue);
                                break;
                            }
                        case "MP":
                            DomUtil.SetElementText(dom.DocumentElement, "tel", strValue);
                            break;
                        case "XH":
                            {
                                try
                                {
                                    if (!string.IsNullOrEmpty(strValue) && strValue != "00010101")
                                    {
                                        DateTime dt = DateTimeUtil.Long8ToDateTime(strValue);

                                        strValue = DateTimeUtil.Rfc1123DateTimeStringEx(dt);

                                        // dt = DateTimeUtil.FromRfc1123DateTimeString(strValue).ToLocalTime();

                                        DomUtil.SetElementText(dom.DocumentElement, "dateOfBirth", strValue);
                                    }
                                }
                                catch { }
                                break;
                            }
                        case "AE":
                            DomUtil.SetElementText(dom.DocumentElement, "name", strValue);
                            break;
                        case "XN": // 民族
                            DomUtil.SetElementText(dom.DocumentElement, "station", strValue);
                            break;
                        case "XK": // 操作类型，01 办证操作 11办证但不处理押金
                            if (strValue == "01")
                            {
                                strAction = "new";
                                bForegift = true;
                                // DomUtil.SetElementText(dom.DocumentElement, "state", "暂停");
                            }
                            else if (strValue == "11")
                            {
                                strAction = "new";
                                bForegift = false;
                            }
                            else if (strValue == "02")
                            {
                                strAction = "change";
                            }
                            strOperation = strValue;
                            break;
                        default:
                            break;
                    }
                } // end of for
                #endregion

                string strOldXml = "";
                string[] results = null;

                #region 根据卡号检索读者记录是否存在
                long lRet = info.LibraryChannel.SearchReader(
                    "<all>",
                    strReaderBarcode,
                    -1,
                    "Barcode",
                    "exact",
                    "en",
                    "default",
                    "keycount",
                    out strError);
                if (lRet == -1)
                {
                    strMsg = "办证失败！按证号查找读者记录发生错误。";
                    goto ERROR1;
                }
                else if (lRet >= 1)
                {
                    strMsg = "办证失败！卡号为【" + strReaderBarcode + "】的读者已存在。";
                    goto ERROR1;
                }
                #endregion

                #region 根据身份证号获得读者记录
                byte[] baTimestamp = null;
                string strRecPath = "";
                lRet = info.LibraryChannel.GetReaderInfo(
                    strIDCardNumber,
                    "xml",
                    out results,
                    out strRecPath,
                    out baTimestamp,
                    out strError);
                switch (lRet)
                {
                    case -1:
                        strMsg = "办证失败！按身份证号查找读者记录发生错误。";
                        goto ERROR1;
                    case 0:
                        strRecPath = "读者/?";
                        break;
                    case 1:
                        {
                            if (strAction == "change")
                            {
                                strOldXml = results[0];
                            }
                            else // strAction == "new"
                            {
                                XmlDocument result_dom = new XmlDocument();
                                result_dom.LoadXml(results[0]);
                                string strBarcode = DomUtil.GetElementText(result_dom.DocumentElement, "barcode");
                                strMsg = "办证失败！您已经有一张卡号为【" + strBarcode + "】的读者证，不能再办证。如读者证丢失，需补证请到柜台办理。";
                                goto ERROR1;
                            }
                            break;
                        }
                    default: // lRet > 1
                        strMsg = "办证失败！身份证号为【" + strIDCardNumber + "】的读者已存在多条记录。";
                        goto ERROR1;
                }
                #endregion


                string strExistingXml = "";
                string strSavedXml = "";
                string strSavedRecPath = "";
                byte[] baNewTimestamp = null;
                ErrorCodeValue kernel_errorcode = ErrorCodeValue.NoError;
                lRet = info.LibraryChannel.SetReaderInfo(
                    strAction,
                    strRecPath,
                    dom.DocumentElement.OuterXml, // strNewXml
                    strOldXml,
                    baTimestamp,
                    out strExistingXml,
                    out strSavedXml,
                    out strSavedRecPath,
                    out baNewTimestamp,
                    out kernel_errorcode,
                    out strError);
                if (lRet == -1)
                {
                    strMsg = strAction == "new" ? "办证失败！创建读者记录发生错误。" : "修改读者信息发生错误。";
                    goto ERROR1;
                }

                lRetValue = lRet;

                if (bForegift == true
                    && String.IsNullOrEmpty(strForegiftValue) == false)
                {
                    // 创建交费请求
                    string strReaderXml = "";
                    string strOverdueID = "";
                    lRet = info.LibraryChannel.Foregift(
                       "foregift",
                       strReaderBarcode,
                        out strReaderXml,
                        out strOverdueID,
                       out strError);
                    if (lRet == -1)
                    {
                        lRet = DeleteReader(info.LibraryChannel,
                            strSavedRecPath, baNewTimestamp, out strError);
                        if (lRet == -1)
                            strError = "办证过程中交费发生错误（回滚失败）：" + strError;
                        else
                            strError = "办证过程中交费发生错误（回滚成功）";

                        strMsg = "办证交费过程中创建交费请求失败，办证失败，请重新操作。";
                        goto ERROR1;
                    }

                    int nRet = DoAmerce(
                        info.LibraryChannel,
                        strReaderBarcode,
                        strForegiftValue,
                        out strMsg,
                        out strError);
                    if (nRet == 0)
                        goto ERROR1;
                }

                if (String.IsNullOrEmpty(strPassword) == false
                    && strAction == "new")
                {
                    lRet = info.LibraryChannel.ChangeReaderPassword(
                        strReaderBarcode,
                        "", // strOldReaderPassword
                        strPassword,
                        out strError);
                    if (lRet != 1)
                    {
                        strMsg = "设置密码不成功，可用[生日]登录后再修改密码。";
                    }
                }
                sb.Append("|AA").Append(strReaderBarcode).Append("|XD").Append("|OK1");
                if (lRetValue == 0)
                {
                    strMsg = strAction == "new" ? "办理新证成功！" + strMsg : "读者信息修改成功！";
                }
                else // if (lRetValue == 1)
                {
                    strMsg = strAction == "new" ? "办理新证成功！但部分内容被拒绝。" + strMsg : "读者信息修改成功！但部分内容被拒绝。";
                }
                sb.Append("|XK").Append(strOperation);
                sb.Append("|AF").Append(strMsg).Append("|AG").Append(strMsg);
                strBackMsg = sb.ToString();
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                LibraryManager.Log?.Error(ExceptionUtil.GetDebugText(ex));
                goto ERROR1;
            }
            finally
            {
                EndFunction(info);
            }

        ERROR1:
            sb.Append("|XK").Append(strOperation).Append("|OK0").Append("|AF").Append(strMsg).Append("|AG").Append(strMsg);
            strBackMsg = sb.ToString();
            return strBackMsg;
        }

        static string ChangePassword(
            SipChannel sip_channel,
            string strSIP2Package)
        {
            string strError = "";

            int nRet = 0;

            string strBarcode = "";
            string strOldPassword = "";
            string strNewPassword = "";
            string strOperation = "";

            string[] parts = strSIP2Package.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < parts.Length; i++)
            {
                string part = parts[i];
                if (part.Length < 2)
                    continue;

                string strValue = part.Substring(2);
                switch (part.Substring(0, 2))
                {
                    case "AA":
                        strBarcode = strValue;
                        break;
                    case "AD":
                        strOldPassword = strValue;
                        break;
                    case "KD":
                        strNewPassword = strValue;
                        break;
                    case "XK":
                        strOperation = strValue;
                        break;
                }
            }
            StringBuilder sb = new StringBuilder(1024);
            sb.Append("82").Append(SIPUtility.NowDateTime).Append("AOdp2Library");
            sb.Append("|AA").Append(strBarcode);
            sb.Append("|XK").Append(strOperation);

            FunctionInfo info = BeginFunction(sip_channel);
            if (string.IsNullOrEmpty(info.ErrorInfo) == false)
            {
                strError = info.ErrorInfo;
                goto ERROR1;
            }
            try
            {
                string strMsg = "";
                long lRet = info.LibraryChannel.ChangeReaderPassword(
                    strBarcode,
                    strOldPassword,
                    strNewPassword,
                    out strError);
                if (lRet == -1)
                {
                    nRet = 0;
                    strMsg = "修改密码过程中发生错误，请稍后再试。";
                }
                else if (lRet == 0)
                {
                    nRet = 0;
                    strMsg = "旧密码输入错误，请重新输入。";
                }
                else
                {
                    nRet = 1;
                    strMsg = "读者修改密码成功！";
                }
                sb.Append("|OK").Append(nRet.ToString());
                sb.Append("|AF").Append(strMsg).Append("|AG").Append(strMsg);
                return sb.ToString();
            }
            finally
            {
                EndFunction(info);
            }

        ERROR1:
            // TODO: 这里返回错误的做法需要确认一下
            sb.Append("|OK").Append(0.ToString());
            sb.Append("|AF").Append(strError).Append("|AG").Append(strError);
            return sb.ToString();
        }

        static long DeleteReader(
            LibraryChannel library_channel,
            string strRecPath,
    byte[] baTimestamp,
    out string strError)
        {
            string strExistingXml = "";
            string strSavedXml = "";
            string strSavedRecPath = "";
            byte[] baNewTimestamp = null;
            ErrorCodeValue kernel_errorcode = ErrorCodeValue.NoError;
            long lRet = library_channel.SetReaderInfo(
                "forcedelete",
                strRecPath,
                "", // strNewXml, 
                "", // strOldXml, 
                baTimestamp,
                out strExistingXml,
                out strSavedXml,
                out strSavedRecPath,
                out baNewTimestamp,
                out kernel_errorcode,
                out strError);
            return lRet;
        }

        static int DoAmerce(
            LibraryChannel library_channel,
            string strReaderBarcode,
    string strForegiftValue,
    out string strMsg,
    out string strError)
        {
            strMsg = "";
            strError = "";

            int nRet = 0;

            byte[] baTimestamp = null;
            string strRecPath = "";
            string[] results = null;
            long lRet = library_channel.GetReaderInfo(
                strReaderBarcode,
                "xml",
                out results,
                out strRecPath,
                out baTimestamp,
                out strError);
            if (lRet == -1)
            {
                strMsg = "办证交押金时获得读者记录发生错误，办证失败，请重新操作。";
            }
            else if (lRet == 0)
            {
                strMsg = "办证交押金时发现卡号读者竟不存在，办证失败，请重新操作。";
            }
            else if (lRet == 1)
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(results[0]);

                string strId = DomUtil.GetAttr(dom.DocumentElement, "overdues/overdue[@reason='押金。']", "id");
                if (String.IsNullOrEmpty(strId))
                {
                    strMsg = "办证交押金时发现费信息竟不存在，办证失败，请到柜台办理。";
                    goto UNDO;
                }

                string strValue = DomUtil.GetAttr(dom.DocumentElement, "overdues/overdue[@reason='押金。']", "price");
                strValue = PriceUtil.OldGetPurePrice(strValue);
                float value = float.Parse(strValue);

                float foregiftValue = float.Parse(strForegiftValue);
                if (value != foregiftValue)
                {
                    strMsg = "您放入的金额是：" + strForegiftValue + "，而您需要交的押金金额为：" + value.ToString();
                    goto UNDO;
                }


                AmerceItem item = new AmerceItem();
                item.ID = strId;
                AmerceItem[] amerce_items = { item };

                AmerceItem[] failed_items = null;
                string strReaderXml = "";
                lRet = library_channel.Amerce(
                    "amerce",
                    strReaderBarcode,
                    amerce_items,
                    out failed_items,
                    out strReaderXml,
                    out strError);
                if (lRet == -1 && lRet == 1)
                {
                    strMsg = "办证时收押金失败，办证失败，请到柜台办理。";
                    goto UNDO;
                }

                nRet = 1;
            }
            else // lRet > 1
            {
                strMsg = "办证交押金时发现卡号为【" + strReaderBarcode + "】读者存在多条，办证失败，请到柜台办理。";
            }

            return nRet;
        UNDO:
            lRet = DeleteReader(library_channel, strRecPath, baTimestamp, out strError);
            if (lRet == -1)
                strError = "办证过程中交费发生错误（回滚失败）：" + strError;
            else
                strError = "办证过程中交费发生错误（回滚成功）";
            return nRet;
        }

        static string CheckDupReaderInfo(
            SipChannel sip_channel,
            string strSIP2Package)
        {
            string strError = "";
            StringBuilder sb = new StringBuilder(1024);
            sb.Append("9220141021 100511AOdp2Library");

            string strOperation = "";

            FunctionInfo info = BeginFunction(sip_channel);
            if (string.IsNullOrEmpty(info.ErrorInfo) == false)
            {
                strError = info.ErrorInfo;
                goto ERROR1;
            }
            try
            {

                string strBarcode = "";
                string strIDCardNumber = "";
                string[] parts = strSIP2Package.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 1; i < parts.Length; i++)
                {
                    string part = parts[i];
                    if (part.Length < 2)
                        continue;

                    string strValue = part.Substring(2);
                    string str = part.Substring(0, 2);

                    if (str == "AA")
                        strBarcode = strValue;
                    if (str == "XO")
                        strIDCardNumber = strValue;
                    else if (str == "XK")
                        strOperation = strValue;
                    else
                        continue;
                }

                sb.Append("|AA").Append(strBarcode);
                sb.Append("|XO").Append(strIDCardNumber);

                if ((strOperation == "0" && String.IsNullOrEmpty(strBarcode))
                    || (strOperation == "1" && String.IsNullOrEmpty(strIDCardNumber)))
                {
                    goto ERROR1;
                }

                #region 根据借书证号或身份证号获得读者记录
                string[] results = null;
                long lRet = info.LibraryChannel.GetReaderInfo(
                    strOperation == "0" ? strBarcode : strIDCardNumber,
                    "xml",
                    out results,
                    out strError);
                switch (lRet)
                {
                    case -1:
                        goto ERROR1;
                    case 0:
                        sb.Append("|AC0");
                        break;
                    case 1:
                        sb.Append("|AC1");
                        break;
                    default: // lRet > 1
                        sb.Append("|AC1");
                        break;
                }
                #endregion

                return sb.ToString();
            }
            finally
            {
                // instance.MessageConnection.ReturnChannel(library_channel);
                EndFunction(info);
            }

        ERROR1:
            sb.Append("|XK").Append(strOperation).Append("|OK0");
            return sb.ToString();
        }

        // dp2library url --> hangup value string
        static Dictionary<string, string> _hangupStatusTable = new Dictionary<string, string>();
        private static readonly Object _syncRoot_hangupStatusTable = new Object();

        // 清空 hangup 状态缓存表。这样可以迫使后继 ScStatus 请求真正从 dp2library 获取状态
        public static void ClearHangupStatusTable()
        {
            lock (_syncRoot_hangupStatusTable)
            {
                _hangupStatusTable.Clear();
            }
        }

        /// <summary>
        /// 状态查询
        /// </summary>
        /// <param name="sip_channel">SIP 通道</param>
        /// <param name="message">SIP 请求消息</param>
        /// <returns></returns>
        static string SCStatus(SipChannel sip_channel, string message)
        {
            // throw new Exception("test exception");
            // 提前准备好响应对象，以便出错时候可以返回。因为这个消息的 fixed length fields 必须要具备
            ACSStatus_98 response = new ACSStatus_98()
            {
                OnlineStatus_1 = "N",
                CheckinOk_1 = "N",
                CheckoutOk_1 = "N",
                ACSRenewalPolicy_1 = "N",
                StatusUpdateOk_1 = "N",
                OfflineOk_1 = "N",
                TimeoutPeriod_3 = "010",
                RetriesAllowed_3 = "003",
                DatetimeSync_18 = SIPUtility.NowDateTime,
                ProtocolVersion_4 = "2.00",
                AO_InstitutionId_r = "dp2Library",
                AM_LibraryName_o = "dp2Library",
                BX_SupportedMessages_r = "YYYYYYYYYYYYYYYY",
                AF_ScreenMessage_o = "",
            };

            string strError = "";

            FunctionInfo info = BeginFunction(sip_channel);
            if (string.IsNullOrEmpty(info.ErrorInfo) == false)
            {
                strError = info.ErrorInfo;
                goto ERROR1;
            }
            try
            {
                // TODO: 要检查相关 dp2Capo 实例是否在线

                // TODO: 如果某个通道的 ScStatus 请求来得很频繁，要考虑缓冲 GetSystemParameter() API 的结果，直接把这个结果返回给前端
                string strExistingValue = null;

                lock (_syncRoot_hangupStatusTable)
                {
                    if (_hangupStatusTable != null
                        && _hangupStatusTable.ContainsKey(info.LibraryChannel.Url))
                        strExistingValue = _hangupStatusTable[info.LibraryChannel.Url];
                }

                long lRet = -1;
                string strValue = "";
                if (strExistingValue == null)
                {
                    //2018/06/19 
                    lRet = info.LibraryChannel.GetSystemParameter("system",
                        "hangup",
                        out strValue,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "GetSystemParameter('system', 'hangup') error: " + strError;
                        goto ERROR1;
                    }

                    // 缓存起来
                    if (lRet == 1)
                    {
                        lock (_syncRoot_hangupStatusTable)
                        {
                            if (_hangupStatusTable != null && _hangupStatusTable.Count < 100)
                                _hangupStatusTable[info.LibraryChannel.Url] = strValue;
                        }
                    }
                }
                else
                {
                    strValue = strExistingValue;
                    lRet = 1;
                }

                response = new ACSStatus_98()
                {
                    OnlineStatus_1 = "Y",   // see instance info
                    CheckinOk_1 = (lRet == -1 ? "N" : "Y"),
                    CheckoutOk_1 = (lRet == -1 ? "N" : "Y"),
                    ACSRenewalPolicy_1 = (lRet == -1 ? "N" : "Y"),
                    StatusUpdateOk_1 = "N", // "Y",
                    OfflineOk_1 = "N",   // "Y",
                    TimeoutPeriod_3 = "010",
                    RetriesAllowed_3 = "003",
                    DatetimeSync_18 = SIPUtility.NowDateTime,
                    ProtocolVersion_4 = "2.00",
                    AO_InstitutionId_r = "dp2Library",
                    AM_LibraryName_o = "dp2Library",
                    BX_SupportedMessages_r = "YYYYYYYYYYYYYYYY",
                    AF_ScreenMessage_o = (lRet == -1 ? strValue : ""),
                };

                return response.ToText();
            }
            finally
            {
                EndFunction(info);
            }
        ERROR1:
            {
                response.AF_ScreenMessage_o = strError;
                return response.ToText();
            }
        }

        public static MarcRecord MarcXml2MarcRecord(string strMarcXml,
    out string strOutMarcSyntax,
    out string strError)
        {
            MarcRecord record = null;

            strError = "";
            strOutMarcSyntax = "";

            string strMARC = "";
            int nRet = MarcUtil.Xml2Marc(strMarcXml,
                false,
                "",
                out strOutMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == 0)
                record = new MarcRecord(strMARC);
            else
                strError = "MarcXml转换错误:" + strError;

            return record;
        }

    }


}
