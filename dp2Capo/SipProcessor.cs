using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
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
using static dp2Capo.ZHostInfo;

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

            // 附加的信息
            sip_channel.Tag = e.AccountTable;

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

            // 将请求消息记入日志
            if (strMessageIdentifiers == "99")
            {
                // 99 ScStatus 因为可能太频繁，不予记载
            }
            else if (strMessageIdentifiers == "93")
                LibraryManager.Log?.Info(strChannelName + ",\r\nrequest=" + RemovePassword(strRequest));
            else
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
                // TODO: case "23": // patron status request

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
                        strResponse = Login(sip_channel,
                            ip,
                            strRequest);
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
                case "41":
                    {
                        // 列出通道信息
                        strResponse = ListChannel(sip_channel, strRequest);
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

        // TODO: 可以考虑在原始字符串上 replace() 替换密码部分
        static string RemovePassword(string message)
        {
            Login_93 request = new Login_93();
            try
            {
                int nRet = request.parse(message, out string strError);
                if (-1 == nRet)
                    return message;
                int count = request.CO_LoginPassword_r == null ? 0 : request.CO_LoginPassword_r.Length;
                request.CO_LoginPassword_r = new string('*', count);
                return request.ToText();
            }
            catch
            {
                return message;
            }
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
            // int autoclear_seconds = instance.sip_host.GetSipParam(userName, sip_channel.Encoding == null).AutoClearSeconds;
            var sip_param = instance.sip_host.TryGetSipParam(userName,
                sip_channel.Encoding == null,
                out string error);
            if (sip_param != null)
            {
                int autoclear_seconds = sip_param.AutoClearSeconds;
                sip_channel.Timeout = autoclear_seconds == 0 ? TimeSpan.MinValue : TimeSpan.FromSeconds(autoclear_seconds);
            }
            else
            {
                sip_channel.Timeout = TimeSpan.MinValue;
                // TODO: 报错？写入错误日志？
            }
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

        // 所连接的 dp2library 的最低版本要求
        static string _dp2library_base_version = "3.129";   // "3.49";

        // 用来控制同一个用户名登录时候并发的 记录锁
        public static RecordLockCollection _userNameLocks = new RecordLockCollection();

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
            string strPureUserName = parts[0];  // 纯粹的用户名部分，不带有 @实例名
            string strInstanceName = parts[1];

            string strPassword = request.CO_LoginPassword_r;
            string strLocationCode = request.CP_LocationCode_o;
            if (string.IsNullOrEmpty(strLocationCode) == false && strLocationCode.IndexOf("%") != -1)
                strLocationCode = Uri.UnescapeDataString(strLocationCode);

            var ret = FindSipInstance(strInstanceName, sip_channel.Encoding == null);
            if (ret.Item1 == null)
            {
                if (sip_channel.Encoding == null)
                    strError = "user name '" + strRequestUserID + "' locate instance, " + ret.Item2;
                else
                    strError = "以用户名 '" + strRequestUserID + "' 定位实例，" + ret.Item2;
                goto ERROR1;
            }
            Instance instance = ret.Item1;

            nRet = DoLogin(
            sip_channel,
            instance,
            strClientIP,
            strPureUserName,
            strPassword,
            strLocationCode,
            false,
            out strError);
            if (nRet == -1)
                goto ERROR1;

            LibraryManager.Log?.Info("终端 " + strLocationCode + " : " + strPureUserName + " 接入");
            response.Ok_1 = "1";
            return response.ToText();
        ERROR1:
            sip_channel.LocationCode = "";
            var error = sip_channel.SetUserName("", "", 0, sip_channel.Tag as Hashtable);
            // sip_channel.InstanceName = null;    // InstanceName 清空必须在 SetUserName() 之后!
            sip_channel.Password = "";

            response.Ok_1 = "0";
            response.AF_ScreenMessage_o = strError;
            LibraryManager.Log?.Info("Login() error: " + strError);
            return response.ToText();
        }

        static LoginCache _loginCache = new LoginCache();

        // 清除 LoginCache 中衰老的事项，和 dp2library 版本号缓存
        public static void ClearLoginCache()
        {
            _loginCache.CleanOld(TimeSpan.FromMinutes(5));
            // 2022/9/27
            _libraryServerVersion = "";
        }

        static int DoLogin(
            SipChannel sip_channel,
            Instance instance,
            string strClientIP,
            string strPureUserName,
            string strPassword,
            string strLocationCode,
            bool skipLogin,
            out string strError)
        {
            strError = "";

            // 匿名登录情形
            if (string.IsNullOrEmpty(strPureUserName))
            {
                // 如果定义了允许匿名登录
                if (String.IsNullOrEmpty(instance.sip_host.AnonymousUserName) == false)
                {
                    strPureUserName = instance.sip_host.AnonymousUserName;
                    strPassword = instance.sip_host.AnonymousPassword;

                    // Password 为 null 表示需要代理方式登录。要避免出现 null 这种情况 
                    if (strPassword == null)
                        strPassword = "";
                }
                else
                {
                    sip_channel.Encoding = null;    // 2022/4/3
                    strError = "Anonymouse login not allowed";
                    /*
                    if (encoding == null)
                        strError = "anonymouse login not allowed";
                    else
                        strError = "不允许匿名登录";
                    */
                    goto ERROR1;
                }
            }

            // 从此以后，报错信息才可以使用中文了
            // 此处可能会抛出异常
            var sip_param = instance.sip_host.TryGetSipParam(strPureUserName,
                true,   // sip_channel.Encoding == null,
                out strError);
            if (sip_param == null)
                goto ERROR1;

            var encoding = sip_param.Encoding;
            sip_channel.Encoding = encoding;    // 确保最后打包 respons 时编码方式正确

            var strInstanceName = instance.Name;

            // 检查实例是否正在维护
            if (instance.Running == false)
            {
                if (encoding == null)
                    strError = $"instance '{strInstanceName}' is in maintenance";
                else
                    strError = $"实例 '{strInstanceName}' 正在维护中，暂时不能访问";
                goto ERROR1;
            }

            try
            {
                /*
                SipParam sip_config = instance.sip_host.TryGetSipParam(strPureUserName, sip_channel.Encoding == null, out strError);
                if (sip_config == null)
                {
                    goto ERROR1;
                }
                */

                // 检查 IP 白名单
                string ipList = sip_param.IpList;
                if (string.IsNullOrEmpty(ipList) == false && ipList != "*"
                    && StringUtil.MatchIpAddressList(ipList, strClientIP) == false)
                {
                    if (encoding == null)
                        strError = "client IP address '" + strClientIP + "' not in white list";
                    else
                        strError = "前端 IP 地址 '" + strClientIP + "' 不在白名单允许的范围内";
                    goto ERROR1;
                }
            }
            catch (Exception ex)
            {
                if (encoding == null)
                    strError = "get SIP configuration error:" + ExceptionUtil.GetAutoText(ex);
                else
                    strError = "获取 SIP 参数时出错:" + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            // 让 channel 从此携带 Instance Name
            // sip_channel.InstanceName = strInstanceName;

            // var maxChannels = instance.sip_host.GetSipParam(strPureUserName, true)?.MaxChannels;
            var maxChannels = sip_param.MaxChannels;

            /*
            strError = sip_channel.SetUserName(strPureUserName,
                strInstanceName,
                maxChannels,
                sip_channel.Tag as Hashtable);
            if (strError != null)
                goto ERROR1;
            sip_channel.Password = strPassword;
            // 注：登录以后 Timeout 才按照实例参数来设定。此前是 sip_channel.Timeout 的默认值
            // sip_channel.Timeout = instance.sip_host.AutoClearSeconds == 0 ? TimeSpan.MinValue : TimeSpan.FromSeconds(instance.sip_host.AutoClearSeconds);
            SetChannelTimeout(sip_channel, sip_channel.UserName, instance);
            // 2022/3/4
            sip_channel.Style = instance.sip_host.GetSipParam(sip_channel.UserName, true).Style;
            */

            LoginInfo login_info = new LoginInfo
            {
                UserName = strPureUserName,
                Password = strPassword
            };
            if (string.IsNullOrEmpty(strClientIP) == false)
                login_info.Style = $"clientIP:{strClientIP}";

            // 2022/3/31
            // 按照用户名字符串进行锁定。让相同用户名的并发登录请求变成顺次处理，避免并发情况突然耗费多根 dp2library 通道
            string lock_string = SipChannel.GetUserInstanceName(strPureUserName, strInstanceName);
            try
            {
                _userNameLocks.LockForWrite(lock_string);
            }
            catch (ApplicationException)
            {
                // 超时了
                if (encoding == null)
                    strError = $"Login request concurrent locking fail";
                else
                    strError = $"登录请求并发锁定失败";
                goto ERROR1;
            }

            LibraryChannel library_channel = instance.MessageConnection.GetChannel(login_info);

            try
            {
                // 确保获得 dp2library 版本号
                // 报错要用英文
                if (EnsureGetVersion(library_channel, out strError) == -1)
                    goto ERROR1;

                if (StringUtil.CompareVersion(_libraryServerVersion, _dp2library_base_version) < 0)
                {
                    if (encoding == null)
                        strError = $"dp2Capo requir dp2Library {_dp2library_base_version} or higher version (but currently dp2Library version is {_libraryServerVersion}. please upgrade dp2Library to newest version";
                    else
                        strError = $"dp2Capo 要求和 dp2Library {_dp2library_base_version} 或以上版本配套使用 (而当前 dp2Library 版本号为 {_libraryServerVersion}。请尽快升级 dp2Library 到最新版本";
                    goto ERROR1;
                }

                string style = $"type=worker,client=dp2capo|0.01,clientip={strClientIP},publicError";

                // 2022/2/25
                if (string.IsNullOrEmpty(strLocationCode) == false
                    && strLocationCode.StartsWith("!")
                    && strLocationCode.Length > 1)
                {
                    // string currentLocation = "#SIP@" + strClientIP;
                    var currentLocation = StringUtil.EscapeString(strLocationCode.Substring(1), "=,");
                    style += $",location={currentLocation}";
                }

                if (encoding == null)
                    style += ",lang=en";

                long lRet = 0;

                if (skipLogin)
                    lRet = 1;
                else
                {
                    if (_loginCache.Contains(sip_channel,
                        strPureUserName + "@" + instance.MessageConnection.GetHashCode().ToString(),
                        strPassword,
                        style) == true)
                        lRet = 1;
                    else
                    {
                        lRet = library_channel.Login(strPureUserName,
                            strPassword,
                            style, // $"type=worker,client=dp2SIPServer|0.01,location={currentLocation},clientip=" + strClientIP,
                            out strError);

                        if (lRet == 1)
                            _loginCache.Set(sip_channel,
                                strPureUserName + "@" + instance.MessageConnection.GetHashCode().ToString(),
                                strPassword,
                                style);
                        else
                            _loginCache.Clear(sip_channel);
                    }
                }

                if (skipLogin == false
                    && (lRet == -1 || lRet == 0))
                {
                    goto ERROR1;
                }
                else
                {
                    // 2022/5/23
                    // 检查 dp2library 账户权限是否含有危险权限
                    var danger_rights = MatchDangerousRights(library_channel.Rights);
                    if (danger_rights.Count > 0)
                    {
                        strError = $"dp2library 账户 '{strPureUserName}'({library_channel.Url}) 使用了下列不必要的权限 '{StringUtil.MakePathList(danger_rights)}'，被 dp2capo 拒绝用于登录";
                        library_channel.Logout(out string _);
                        goto ERROR1;
                    }

                    // 2021/3/4
                    // 将登录者的馆代码转换为机构代码
                    string libraryCodeList = library_channel.LibraryCodeList;
                    var codes = StringUtil.SplitList(libraryCodeList);
                    if (codes.Count == 0)
                        codes.Add("");

                    // 注意报错要用英文
                    // TODO: 是否可以为多个馆代码分别得到机构代码，然后用逗号连接返回？
                    int nRet = GetOwnerInstitution(instance,    // library_channel,
        codes[0] + "/",
        out string strInstitution,
        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // sip_channel.InstanceName = strInstanceName; // "";  BUG 2018/8/24 排除。另注: InstanceName 必须在 SetUserName() 前准备好
                    strError = sip_channel.SetUserName(strPureUserName,
                        strInstanceName,
                        maxChannels,
                        sip_channel.Tag as Hashtable);
                    if (strError != null)
                        goto ERROR1;
                    sip_channel.Password = strPassword;
                    sip_channel.Encoding = encoding;

                    sip_channel.LibraryCodeList = libraryCodeList;
                    sip_channel.Institution = strInstitution;
                    sip_channel.LocationCode = strLocationCode;

                    // 注：登录以后 Timeout 才按照实例参数来设定。此前是 sip_channel.Timeout 的默认值
                    // sip_channel.Timeout = instance.sip_host.AutoClearSeconds == 0 ? TimeSpan.MinValue : TimeSpan.FromSeconds(instance.sip_host.AutoClearSeconds);
                    SetChannelTimeout(sip_channel, sip_channel.UserName, instance);
                    // 2022/3/4
                    // sip_channel.Style = instance.sip_host.GetSipParam(sip_channel.UserName, true).Style;
                    sip_channel.Style = sip_param.Style;
                }

                return 0;
            }
            finally
            {
                instance.MessageConnection.ReturnChannel(library_channel);
                _userNameLocks.UnlockForWrite(lock_string);
            }
        ERROR1:
            _loginCache.Clear(sip_channel);
            return -1;
        }

        static string[] _dangerousRights = new string[] {
        "managedatabase",
        "clearalldbs",

        "getuser",
        "changeuser",
        "newuser",
        "deleteuser",

        "setsystemparameter",

        /*
        "setclock",

        "setreaderinfo",
        "movereaderinfo",
        "changereaderbarcode",
        "changereaderpassword",

        "amercemodifyprice",
        "amercemodifycomment",
        "amerceundo",

        "inventory",
        "inventorydelete",

        "search",
        "getrecord",
        "changecalendar",
        "newcalendar",
        "deletecalendar",
        "batchtask",
        "devolvereaderinfo",

        "changeuserpassword",
        "simulatereader",
        "simulateworker",

        "urgentrecover",
        "repairborrowinfo",
        "passgate",
        "getres",
        "writeres",
        "setbiblioinfo",
        "setauthorityinfo",
        "hire",
        "foregift",
        "returnforegift",

        "settlement",
        "deletesettlement",

        "searchissue",
        "getissueinfo",
        "setissueinfo",

        "order",
        "searchorder",
        "getorderinfo",
        "setorderinfo",

        "getcommentinfo",
        "setcommentinfo",
        "searchcomment",

        "writeobject",
        "writerecord",
        "writetemplate",

        "backup",
        "restore",

        "managecache",
        "managecomment",
        "manageopac",

        "settailnumber",
        "setutilinfo",

        "getpatrontempid",
        "getchannelinfo",
        "managechannel",
        "viewreport",
        "upload",
        "download",
        "bindpatron",
        */
        };

        // 检查危险性权限
        static List<string> MatchDangerousRights(string rights)
        {
            List<string> results = new List<string>();
            string[] parts = rights.Split(new char[] { ',' });
            foreach (var part in parts)
            {
                if (Array.IndexOf(_dangerousRights, part) != -1)
                    results.Add(part);
            }

            return results;
        }

#if OLD
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
            string strPureUserName = parts[0];  // 纯粹的用户名部分，不带有 @实例名
            string strInstanceName = parts[1];

            string strPassword = request.CO_LoginPassword_r;
            string strLocationCode = request.CP_LocationCode_o;
            if (string.IsNullOrEmpty(strLocationCode) == false && strLocationCode.IndexOf("%") != -1)
                strLocationCode = Uri.UnescapeDataString(strLocationCode);

            var ret = FindSipInstance(strInstanceName, sip_channel.Encoding == null);
            if (ret.Item1 == null)
            {
                if (sip_channel.Encoding == null)
                    strError = "user name '" + strRequestUserID + "' locate instance, " + ret.Item2;
                else
                    strError = "以用户名 '" + strRequestUserID + "' 定位实例，" + ret.Item2;
                goto ERROR1;
            }
            Instance instance = ret.Item1;

            strInstanceName = instance.Name;

            // 检查实例是否正在维护
            if (instance.Running == false)
            {
                if (sip_channel.Encoding == null)
                    strError = $"instance '{strInstanceName}' is in maintenance";
                else
                    strError = $"实例 '{strInstanceName}' 正在维护中，暂时不能访问";
                goto ERROR1;
            }

            // 匿名登录情形
            if (string.IsNullOrEmpty(strPureUserName))
            {
                // 如果定义了允许匿名登录
                if (String.IsNullOrEmpty(instance.sip_host.AnonymousUserName) == false)
                {
                    strPureUserName = instance.sip_host.AnonymousUserName;
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
                SipParam sip_config = instance.sip_host.GetSipParam(strPureUserName, sip_channel.Encoding == null);

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
            // sip_channel.InstanceName = strInstanceName;

            var maxChannels = instance.sip_host.GetSipParam(strPureUserName, true).MaxChannels;
            // 从此以后，报错信息才可以使用中文了
            // 此处可能会抛出异常
            sip_channel.Encoding = instance.sip_host.GetSipParam(strPureUserName, sip_channel.Encoding == null).Encoding;

            strError = sip_channel.SetUserName(strPureUserName,
                strInstanceName,
                maxChannels,
                sip_channel.Tag as Hashtable);
            if (strError != null)
                goto ERROR1;
            sip_channel.Password = strPassword;
            // 注：登录以后 Timeout 才按照实例参数来设定。此前是 sip_channel.Timeout 的默认值
            // sip_channel.Timeout = instance.sip_host.AutoClearSeconds == 0 ? TimeSpan.MinValue : TimeSpan.FromSeconds(instance.sip_host.AutoClearSeconds);
            SetChannelTimeout(sip_channel, sip_channel.UserName, instance);
            // 2022/3/4
            sip_channel.Style = instance.sip_host.GetSipParam(sip_channel.UserName, true).Style;

            LoginInfo login_info = new LoginInfo { UserName = sip_channel.UserName, Password = sip_channel.Password };

            // 2022/3/31
            // 按照用户名字符串进行锁定。让相同用户名的并发登录请求变成顺次处理，避免并发情况突然耗费多根 dp2library 通道
            string lock_string = sip_channel.GetUserInstanceName();
            try
            {
                _userNameLocks.LockForWrite(lock_string);
            }
            catch (ApplicationException)
            {
                // 超时了
                if (sip_channel.Encoding == null)
                    strError = $"Login request concurrent locking fail";
                else
                    strError = $"登录请求并发锁定失败";
                goto ERROR1;
            }

            LibraryChannel library_channel = instance.MessageConnection.GetChannel(login_info);

            try
            {
                // 确保获得 dp2library 版本号
                // 报错要用英文
                if (EnsureGetVersion(library_channel, out strError) == -1)
                    goto ERROR1;

                if (StringUtil.CompareVersion(_libraryServerVersion, _dp2library_base_version) < 0)
                {
                    if (sip_channel.Encoding == null)
                        strError = $"dp2Capo requir dp2Library { _dp2library_base_version} or higher version (but currently dp2Library version is { _libraryServerVersion }. please upgrade dp2Library to newest version";
                    else
                        strError = $"dp2Capo 要求和 dp2Library { _dp2library_base_version} 或以上版本配套使用 (而当前 dp2Library 版本号为 { _libraryServerVersion }。请尽快升级 dp2Library 到最新版本";
                    goto ERROR1;
                }

                string style = $"type=worker,client=dp2SIPServer|0.01,clientip={strClientIP}";

                // 2022/2/25
                if (string.IsNullOrEmpty(strLocationCode) == false
                    && strLocationCode.StartsWith("!")
                    && strLocationCode.Length > 1)
                {
                    // string currentLocation = "#SIP@" + strClientIP;
                    var currentLocation = StringUtil.EscapeString(strLocationCode.Substring(1), "=,");
                    style += $",location={currentLocation}";
                }

                long lRet = library_channel.Login(strPureUserName,
                    strPassword,
                    style, // $"type=worker,client=dp2SIPServer|0.01,location={currentLocation},clientip=" + strClientIP,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    goto ERROR1;
                }
                else
                {
                    // 2021/3/4
                    // 将登录者的馆代码转换为机构代码
                    string libraryCodeList = library_channel.LibraryCodeList;
                    var codes = StringUtil.SplitList(libraryCodeList);
                    if (codes.Count == 0)
                        codes.Add("");
                    // 注意报错要用英文
                    // TODO: 是否可以为多个馆代码分别得到机构代码，然后用逗号连接返回？
                    nRet = GetOwnerInstitution(library_channel,
    codes[0] + "/",
    out string strInstitution,
    out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    sip_channel.LibraryCodeList = libraryCodeList;
                    sip_channel.Institution = strInstitution;
                    sip_channel.LocationCode = strLocationCode;
                    // sip_channel.InstanceName = strInstanceName; // "";  BUG 2018/8/24 排除。另注: InstanceName 必须在 SetUserName() 前准备好
                    strError = sip_channel.SetUserName(strPureUserName,
                        strInstanceName,
                        maxChannels,
                        sip_channel.Tag as Hashtable);
                    if (strError != null)
                        goto ERROR1;
                    sip_channel.Password = strPassword;

                    LibraryManager.Log?.Info("终端 " + strLocationCode + " : " + strPureUserName + " 接入");
                }

                response.Ok_1 = "1";
                return response.ToText();
            }
            finally
            {
                instance.MessageConnection.ReturnChannel(library_channel);
                _userNameLocks.UnlockForWrite(lock_string);
            }

        ERROR1:
            sip_channel.LocationCode = "";
            var error = sip_channel.SetUserName("", "", 0, sip_channel.Tag as Hashtable);
            // sip_channel.InstanceName = null;    // InstanceName 清空必须在 SetUserName() 之后!
            sip_channel.Password = "";

            response.Ok_1 = "0";
            response.AF_ScreenMessage_o = strError;
            LibraryManager.Log?.Info("Login() error: " + strError);
            return response.ToText();
        }

#endif
        // 获得一个 location 对应的机构代码
        // 注: 利用 capo 账户的 LibraryChannel
        static int GetOwnerInstitution(Instance instance,
                string location,
                out string strInstitution,
                out string strError)
        {
            LibraryChannel library_channel = instance.MessageConnection.GetChannel(null);
            try
            {
                return GetOwnerInstitution(library_channel,
    location,
    out strInstitution,
    out strError);
            }
            finally
            {
                instance.MessageConnection.ReturnChannel(library_channel);
            }
        }

        // 请求 dp2library 获得一个馆藏位置对应的机构代码
        static int GetOwnerInstitution(LibraryChannel library_channel,
                string location,
                out string strInstitution,
                out string strError)
        {
            strError = "";
            strInstitution = "";

            long lRet = library_channel.GetSystemParameter("rfid/getOwnerInstitution",
    location,
    out string strValue,
    out strError);
            if (lRet == -1)
            {
                strError = $"get institution code of '{location}' error: {strError}";
                return -1;
            }

            // 注意返回的是 OI|AOI 形态，需要再分离一下
            var parts = StringUtil.ParseTwoPart(strValue, "|");
            if (string.IsNullOrEmpty(parts[0]) == false)
                strInstitution = parts[0];
            else
                strInstitution = parts[1];
            return 0;
        }

        class FunctionInfo
        {
            public string InstanceName { get; set; }
            public Instance Instance { get; set; }
            // LoginInfo LoginInfo { get; set; }
            public LibraryChannel LibraryChannel { get; set; }
            public string ErrorInfo { get; set; }

            // 2022/4/1
            // 用于锁定的用户名
            public string LockString { get; set; }
        }

        // 注：一定不要忘记最后调用 EndFunction() 以便释放 LibraryChannel
        // parameters:
        //      use_capo_account    是否使用 capo 代理账户?
        static FunctionInfo BeginFunction(
            SipChannel sip_channel,
            bool check_login = true,
            bool locking_userName = false,
            bool use_capo_account = false)
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
                        strError = "not login. (SIP channel instance name ('InstanceName') has not initialized)";
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
                    strError = $"instance '{info.Instance.Name}' is in maintenance";
                else
                    strError = $"实例 '{info.Instance.Name}' 正在维护中，暂时不能访问";
                goto ERROR1;
            }

            var login_info = new LoginInfo
            {
                UserName = sip_channel.UserName,
                Password = sip_channel.Password,
            };

            string ip = TcpServer.GetClientIP(sip_channel.TcpClient);
            if (string.IsNullOrEmpty(ip) == false)
                login_info.Style = $"clientIP:{ip}";

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

                    int nRet = DoLogin(
sip_channel,
info.Instance,
ip,
login_info.UserName,
login_info.Password,
"",
false,
out strError);
                    if (nRet == -1)
                    {
                        if (sip_channel.Encoding == null)
                            strError = $"Anonymouse login fail: {strError}";
                        else
                            strError = $"匿名登录失败: {strError}";
                        goto ERROR1;
                    }
#if OLD
                    SetChannelTimeout(sip_channel, login_info.UserName, info.Instance);

                    // 2022/3/25
                    if (sip_channel.UserName == null)
                    {
                        var maxChannels = info.Instance.sip_host.GetSipParam(login_info.UserName, true).MaxChannels;

                        /*
                        // 2022/4/1
                        if (string.IsNullOrEmpty(sip_channel.InstanceName))
                            sip_channel.InstanceName = info.InstanceName;
                        */

                        strError = sip_channel.SetUserName(login_info.UserName,
                            info.InstanceName,
                            maxChannels,
                            sip_channel.Tag as Hashtable);
                        if (strError != null)
                            goto ERROR1;
                        sip_channel.Password = login_info.Password;
                    }

                    // 2022/3/25
                    // 从此以后，报错信息才可以使用中文了
                    // 此处可能会抛出异常
                    if (sip_channel.Encoding == null
                        && info.Instance != null)
                        sip_channel.Encoding = info.Instance.sip_host.GetSipParam(login_info.UserName, sip_channel.Encoding == null)?.Encoding;
#endif
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
            else
            {
                // 2021/4/8
                if (check_login
                    && string.IsNullOrEmpty(login_info.UserName))
                {
                    if (sip_channel.Encoding == null)
                        strError = "not login";
                    else
                        strError = "尚未登录";
                    goto ERROR1;
                }
            }

            // 2022/3/31
            // 按照用户名字符串进行锁定。让相同用户名的并发登录请求变成顺次处理，避免并发情况突然耗费多根 dp2library 通道
            if (locking_userName)
            {
                string lock_string;
                if (use_capo_account)
                    lock_string = sip_channel.InstanceName;
                else
                    lock_string = sip_channel.GetUserInstanceName();
                try
                {
                    _userNameLocks.LockForWrite(lock_string);
                }
                catch (ApplicationException)
                {
                    // 超时了
                    if (sip_channel.Encoding == null)
                        strError = $"get dp2library channel concurrent locking fail";
                    else
                        strError = $"获得 dp2library 通道时并发锁定失败";
                    goto ERROR1;
                }
                info.LockString = lock_string;
            }
            info.LibraryChannel = info.Instance.MessageConnection.GetChannel(use_capo_account ? null : login_info);
            return info;
        ERROR1:
            info.ErrorInfo = strError;
            return info;
        }

        static void EndFunction(FunctionInfo info)
        {
            info.Instance.MessageConnection.ReturnChannel(info.LibraryChannel);
            if (info.LockString != null)
            {
                _userNameLocks.UnlockForWrite(info.LockString);
                info.LockString = null;
            }
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
                AB_ItemIdentifier_r = "",   // 2022/3/4
                AO_InstitutionId_r = "",    // "dp2Library",
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

                // 2022/2/25
                string strCurrentLocation = request.AP_CurrentLocation_r;

                string strItemIdentifier = request.AB_ItemIdentifier_r;

                // 2022/3/4
                if (StringUtil.IsInList("bookUiiStrict", sip_channel.Style)
                    && strItemIdentifier.Contains(".") == false)
                {
                    strError = $"请求的 AB 字段内容 '{strItemIdentifier}' 不合法。应为 UII 形态";
                    goto ERROR1;
                }

                // 2021/3/3
                string strInstitution = request.AO_InstitutionId_r;

                if (!string.IsNullOrEmpty(strItemIdentifier))
                {
                    response.AB_ItemIdentifier_r = strItemIdentifier;
                    string style = "item,biblio,reader";
                    if (string.IsNullOrEmpty(strCurrentLocation) == false)
                        style += ",currentLocation:" + StringUtil.EscapeString(strCurrentLocation, ":,");
                    long lRet = info.LibraryChannel.Return(
                        "return",
                        "",    //strReaderBarcode,
                        AddOI(strItemIdentifier, strInstitution),
                        "", // strConfirmItemRecPath
                        false,
                        style,
                        "xml",
                        out string[] item_records,
                        "",
                        out string[] reader_records,
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
                        else
                            response.Ok_1 = "0";

                        response.AF_ScreenMessage_o = strError;
                    }
                    else
                    {
                        response.Ok_1 = "1";
                        if (lRet == 1)  // 表示有提示需要提醒工作人员
                            response.Alert_1 = "Y";
                        response.AA_PatronIdentifier_o = strOutputReaderBarcode;
                        if (lRet == 0)
                            strError = "还书成功";
                        response.AF_ScreenMessage_o = strError;
                        response.AG_PrintLine_o = strError;

#if REMOVED
                        // 2021/3/4
                        // 返回读者记录是为了得到确切的机构代码
                        if (reader_records != null && reader_records.Length > 0)
                        {
                            response.AO_InstitutionId_r = BuildAO(reader_records[0]);

                            /*
                            string patron_xml = reader_records[0];
                            XmlDocument dom = new XmlDocument();
                            try
                            {
                                dom.LoadXml(patron_xml);
                            }
                            catch (Exception ex)
                            {
                                LibraryManager.Log?.Error("读者 XML 解析错误：" + ExceptionUtil.GetDebugText(ex));
                                response.AF_ScreenMessage_o = "读者 XML 解析错误";
                                response.AG_PrintLine_o = "读者 XML 解析错误";
                                return response.ToText();
                            }

                            response.AO_InstitutionId_r = DomUtil.GetElementText(dom.DocumentElement, "oi");
                            */
                        }
#endif                        
                        // 2021/3/9
                        // 返回册记录是为了得到确切的机构代码
                        if (item_records != null && item_records.Length > 0)
                        {
                            response.AO_InstitutionId_r = GetInstitution(item_records[0]);
                        }

                        if (item_records != null && item_records.Length > 0)
                        {
                            XmlDocument dom = new XmlDocument();
                            try
                            {
                                dom.LoadXml(item_records[0]);

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
                    else if (response.Ok_1 == "1")  // 注意，如果 API 不成功，不要返回书目摘要
                    {
                        lRet = info.LibraryChannel.GetBiblioSummary(
                            AddOI(strItemIdentifier, strInstitution),   // strItemIdentifier,
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
            // sip_channel.UserName = "";  // TODO: Clear()
            // sip_channel.Password = "";

            LibraryManager.Log?.Info("Checkin() error: " + strError);
            response.Ok_1 = "0";

            // 2022/3/4
            response.AF_ScreenMessage_o = strError;
            response.AG_PrintLine_o = strError;

            return response.ToText();
        }

        // 确保不返回 "" 代替 null
        static string BuildAO(string text)
        {
            if (text == null)
                return "";
            return text;
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
                AA_PatronIdentifier_r = "", // 2022/3/4
                AB_ItemIdentifier_r = "",   // 2022/3/4
                AO_InstitutionId_r = "",    // "dp2Library",
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

            // 2021/3/3
            string strInstitution = request.AO_InstitutionId_r;

            string strItemIdentifier = request.AB_ItemIdentifier_r;

            // 2022/3/4
            if (StringUtil.IsInList("bookUiiStrict", sip_channel.Style)
                && strItemIdentifier.Contains(".") == false)
            {
                strError = $"请求的 AB 字段内容 '{strItemIdentifier}' 不合法。应为 UII 形态";
                goto ERROR1;
            }

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
                        // Result.Value -1出错 0密码不正确 1密码正确
                        lRet = info.LibraryChannel.VerifyReaderPassword(
                            AddOI(strPatronIdentifier, strInstitution),    //读者证条码号
                                                                           // strPatronIdentifier,
                            strPatronPassword,
                            out strError);
                        if (-1 == lRet)
                        {
                            response.AF_ScreenMessage_o = "校验密码发生错误：" + strError;
                            return response.ToText();
                        }
                        else if (0 == lRet)
                        {
                            response.AF_ScreenMessage_o = "失败：密码错误";
                            return response.ToText();
                        }
                    }

                    string[] aDupPath = null;
                    string[] item_records = null;
                    string[] reader_records = null;
                    string[] biblio_records = null;
                    string strOutputReaderBarcode = "";
                    DigitalPlatform.LibraryClient.localhost.BorrowInfo borrow_info = null;
                    lRet = info.LibraryChannel.Borrow(
                        false,  // 续借为 true
                        AddOI(strPatronIdentifier, strInstitution),    //读者证条码号
                        AddOI(strItemIdentifier, strInstitution),
                        // strItemIdentifier,     // 册条码号
                        null, //strConfirmItemRecPath,
                        false,
                        null,   // this.OneReaderItemBarcodes,
                        "auto_renew,biblio,item,reader", // strStyle, // auto_renew,biblio,item  //  "reader,item,biblio", // strStyle,
                        "xml:noborrowhistory",  // strItemReturnFormats,
                        out item_records,
                        "", // "summary",    // strReaderFormatList
                        out reader_records,
                        "xml",         //strBiblioReturnFormats,
                        out biblio_records,
                        out aDupPath,
                        out strOutputReaderBarcode,
                        out borrow_info,
                        out strError);
                    if (-1 == lRet)
                    {
                        response.Ok_1 = "0";
                        response.AF_ScreenMessage_o = "失败：" + strError;
                    }
                    else
                    {
                        response.Ok_1 = "1";

#if REMOVED
                        // 2021/3/4
                        // 返回读者记录是为了得到确切的机构代码
                        if (reader_records != null && reader_records.Length > 0)
                        {
                            response.AO_InstitutionId_r = BuildAO(reader_records[0]);

                            /*
                            string patron_xml = reader_records[0];
                            XmlDocument dom = new XmlDocument();
                            try
                            {
                                dom.LoadXml(patron_xml);
                            }
                            catch (Exception ex)
                            {
                                LibraryManager.Log?.Error("读者 XML 解析错误：" + ExceptionUtil.GetDebugText(ex));
                                response.AF_ScreenMessage_o = "读者 XML 解析错误";
                                response.AG_PrintLine_o = "读者 XML 解析错误";
                                return response.ToText();
                            }

                            response.AO_InstitutionId_r = DomUtil.GetElementText(dom.DocumentElement, "oi");
                            */
                        }
#endif
                        // 2021/3/9
                        // 返回册记录是为了得到确切的机构代码
                        if (item_records != null && item_records.Length > 0)
                        {
                            response.AO_InstitutionId_r = GetInstitution(item_records[0]);
                        }

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

                        // TODO: 这里可能会抛出异常
                        // var date_format = info.Instance.sip_host.GetSipParam(sip_channel.UserName, true).DateFormat;
                        var date_format = GetDateFormat(info.Instance, sip_channel.UserName, sip_channel.Encoding == null);

                        string strLatestReturnTime = DateTimeUtil.Rfc1123DateTimeStringToLocal(borrow_info.LatestReturnTime,
                            date_format);
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
            response.Ok_1 = "0";
            response.AF_ScreenMessage_o = strError;
            response.AG_PrintLine_o = strError;
            return response.ToText();
        }

        static string GetDateFormat(Instance instance,
            string userName,
            bool neutralLanguage)
        {
            var sip_param = instance.sip_host.TryGetSipParam(userName, neutralLanguage, out string error);
            if (sip_param == null)
                return SipServer.DEFAULT_DATE_FORMAT;  // 缺省的
            return sip_param.DateFormat;
        }

        static string GetInstitution(string xml)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch (Exception ex)
            {
                return $"!XML 解析错误：{ex.Message}";
            }

            string oi = DomUtil.GetElementText(dom.DocumentElement, "oi", out XmlNode node);
            if (node != null && ((XmlElement)node).HasAttribute("error"))
            {
                string error = DomUtil.GetAttr(node, "error");
                if (error != null && error.StartsWith("(notfound)"))
                    return "";
                return "!" + error;
            }
            return oi;
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

                // 2021/1/19
                AB_ItemIdentifier_r = "",
                AJ_TitleIdentifier_r = "",
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

                // 2022/3/4
                if (StringUtil.IsInList("bookUiiStrict", sip_channel.Style)
                    && strItemIdentifier.Contains(".") == false)
                {
                    strError = $"请求的 AB 字段内容 '{strItemIdentifier}' 不合法。应为 UII 形态";
                    goto ERROR1;
                }
                response.AB_ItemIdentifier_r = strItemIdentifier;

                // 2021/1/31
                string strInstitution = request.AO_InstitutionId_r;

                string uii = AddOI(strItemIdentifier, strInstitution);
            REDO:
                long lRet = info.LibraryChannel.GetItemInfo(
                    uii,
                    "xml",
                    out string strItemXml,
                    "xml",
                    out string strBiblio,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    if (info.LibraryChannel.ErrorCode == ErrorCode.ChannelReleased)
                        goto REDO;

                    // 2021/1/27
                    response.AB_ItemIdentifier_r = "";

                    response.CirculationStatus_2 = "01";

                    if (lRet == -1)
                        strError = $"获得册记录 {uii} 时发生错误: {strError}";
                    else
                    {
                        // 尽量保留 API 原来的报错信息
                        if (string.IsNullOrEmpty(strError))
                            strError = $"册记录 {uii} 不存在";
                    }

                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                }
#if NO
                else if (0 == lRet)
                {
                    response.CirculationStatus_2 = "13";

                    strError = strItemIdentifier + " 记录不存在";
                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                }
#endif
                else if (1 < lRet)
                {
                    response.CirculationStatus_2 = "01";
                    strError = $"册记录 {uii} 发生重复，需馆员处理";
                    response.AF_ScreenMessage_o = strError;
                    response.AG_PrintLine_o = strError;
                }
                else if (1 == lRet)
                {
                    // string dateFormat = info.Instance.sip_host.GetSipParam(sip_channel.UserName, true).DateFormat;
                    var dateFormat = GetDateFormat(info.Instance, sip_channel.UserName, sip_channel.Encoding == null);

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
            response.AG_PrintLine_o = strError;
            // 2021/4/7
            // 迫使前端感觉到错误
            response.AB_ItemIdentifier_r = "";
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
                            response.CirculationStatus_2 = "08"; // 等待放到预约保留架                               
                    }

                }
                else
                {
                    if (StringUtil.IsInList("丢失", strItemState))
                        response.CirculationStatus_2 = "12";
                    else if (StringUtil.IsInList("注销", strItemState))
                    {
                        response.CirculationStatus_2 = "01";
                        strError = $"本册状态为 '{strItemState}'";
                        // 在 message 里面补充说明
                        response.AF_ScreenMessage_o = strError;
                        response.AG_PrintLine_o = strError;
                    }
                    else if (StringUtil.IsInList("加工中", strItemState))
                        response.CirculationStatus_2 = "06";
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
                AB_ItemIdentifier_r = "",
                // AJ_TitleIdentifier_o = "",
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

                // 2022/3/4
                if (StringUtil.IsInList("bookUiiStrict", sip_channel.Style)
                    && strItemIdentifier.Contains(".") == false)
                {
                    strError = $"请求的 AB 字段内容 '{strItemIdentifier}' 不合法。应为 UII 形态";
                    goto ERROR1;
                }

                // 2021/1/31
                string strInstitution = request.AO_InstitutionId_r;

                response.AB_ItemIdentifier_r = strItemIdentifier;
            // string strItemXml = "";
            // string strBiblio = "";

            REDO:
                long lRet = info.LibraryChannel.GetItemInfo(
                    "item",
                    AddOI(strItemIdentifier, strInstitution),
                    "",
                    "xml",
                    out string strItemXml,
                    out string item_recpath,
                    out byte[] item_timestamp,
                    "xml",
                    out string strBiblio,
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
                    // string dateFormat = info.Instance.sip_host.GetSipParam(sip_channel.UserName, true).DateFormat;
                    var dateFormat = GetDateFormat(info.Instance, sip_channel.UserName, sip_channel.Encoding == null);

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
            response.AG_PrintLine_o = strError;
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

            try
            {
                // 获得图书标题
                string strMarcSyntax = "";
                MarcRecord record = MarcXml2MarcRecord(strBiblio,
                    out strMarcSyntax,
                    out strError);
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
                    return -1;
                }

                XmlDocument item_dom = new XmlDocument();
                item_dom.LoadXml(strItemXml);

                // 2020/12/8
                // 取记录中真实的册条码号。可能和检索发起的号码不同
                string strItemIdentifier = DomUtil.GetElementText(item_dom.DocumentElement, "barcode");
                if (string.IsNullOrEmpty(strItemIdentifier))
                {
                    strItemIdentifier = DomUtil.GetElementText(item_dom.DocumentElement, "refID");
                    if (string.IsNullOrEmpty(strItemIdentifier) == false)
                        strItemIdentifier = "@refID:" + strItemIdentifier;
                }
                response.AB_ItemIdentifier_r = strItemIdentifier;

                // 2021/3/3
                string strInstitution = request.AO_InstitutionId_r;

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
                        // 确保获得 dp2library 版本号
                        if (EnsureGetVersion(info.LibraryChannel, out strError) == -1)
                            return -1;
                        /*
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
                        */

                        string base_version = "3.40";
                        if (StringUtil.CompareVersion(_libraryServerVersion, base_version) < 0)
                        {
                            strError = $"dp2Capo 要求和 dp2Library {base_version} 或以上版本配套使用 (而当前 dp2Library 版本号为 {_libraryServerVersion}。请尽快升级 dp2Library 到最新版本";
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
                        AddOI(strItemIdentifier, strInstitution),
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

                response.ItemPropertiesOk_1 = "1";
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
                AA_PatronIdentifier_r = "", // 2022/3/4
                AB_ItemIdentifier_r = "",   // 2022/3/4
                AO_InstitutionId_r = "",    // "dp2Library",
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

                // 2022/3/4
                if (StringUtil.IsInList("bookUiiStrict", sip_channel.Style)
                    && strItemIdentifier.Contains(".") == false)
                {
                    strError = $"请求的 AB 字段内容 '{strItemIdentifier}' 不合法。应为 UII 形态";
                    goto ERROR1;
                }

                // 2021/3/3
                string strInstitution = request.AO_InstitutionId_r;

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
                        AddOI(strPatronIdentifier, strInstitution),    //读者证条码号
                                                                       // strPatronIdentifier,
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
                    AddOI(strPatronIdentifier, strInstitution),    //读者证条码号
                    AddOI(strItemIdentifier, strInstitution),
                    // strItemIdentifier,     // 册条码号
                    null, //strConfirmItemRecPath,
                    false,
                    null,   // this.OneReaderItemBarcodes,
                    "auto_renew,biblio,item,reader", // strStyle, // auto_renew,biblio,item                   //  "reader,item,biblio", // strStyle,
                    "xml:noborrowhistory",  // strItemReturnFormats,
                    out item_records,
                    "", // "summary",    // strReaderFormatList
                    out reader_records,
                    "xml",         //strBiblioReturnFormats,
                    out biblio_records,
                    out aDupPath,
                    out strOutputReaderBarcode,
                    out DigitalPlatform.LibraryClient.localhost.BorrowInfo borrow_info,
                    out strError);
                if (-1 == lRet)
                {
                    response.Ok_1 = "0";
                    response.AF_ScreenMessage_o = "失败：" + strError;
                }
                else
                {
                    response.Ok_1 = "1";
                    response.RenewalOk_1 = "Y";

#if REMOVED
                    // 2021/3/4
                    // 返回读者记录是为了得到确切的机构代码
                    if (reader_records != null && reader_records.Length > 0)
                    {
                        response.AO_InstitutionId_r = BuildAO(reader_records[0]);

                        /*
                        string patron_xml = reader_records[0];
                        XmlDocument dom = new XmlDocument();
                        try
                        {
                            dom.LoadXml(patron_xml);
                        }
                        catch (Exception ex)
                        {
                            LibraryManager.Log?.Error("读者 XML 解析错误：" + ExceptionUtil.GetDebugText(ex));
                            response.AF_ScreenMessage_o = "读者 XML 解析错误";
                            response.AG_PrintLine_o = "读者 XML 解析错误";
                            return response.ToText();
                        }

                        response.AO_InstitutionId_r = DomUtil.GetElementText(dom.DocumentElement, "oi");
                        */
                    }
#endif
                    // 2021/3/9
                    // 返回册记录是为了得到确切的机构代码
                    if (item_records != null && item_records.Length > 0)
                    {
                        response.AO_InstitutionId_r = GetInstitution(item_records[0]);
                    }

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

                    /*
                    string strLatestReturnTime = DateTimeUtil.Rfc1123DateTimeStringToLocal(borrow_info.LatestReturnTime,
                        info.Instance.sip_host.GetSipParam(sip_channel.UserName, true).DateFormat);
                    */
                    var date_format = GetDateFormat(info.Instance, sip_channel.UserName, sip_channel.Encoding == null);

                    string strLatestReturnTime = DateTimeUtil.Rfc1123DateTimeStringToLocal(borrow_info.LatestReturnTime,
date_format);
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
            response.Ok_1 = "0";
            response.AF_ScreenMessage_o = strError;
            response.AG_PrintLine_o = strError;
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
                AO_InstitutionId_r = "",    // "dp2Library",
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

                AO_InstitutionId_r = "",    // "dp2Library",
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

                // 2021/3/3
                string strInstitution = request.AO_InstitutionId_r;

                // 2022/5/25
                string strFeeIdentifier = request.CG_FeeIdentifier_o;
                List<string> ids = null;
                if (string.IsNullOrEmpty(strFeeIdentifier) == false)
                    ids = StringUtil.SplitList(strFeeIdentifier, ',');

                // 先查到读者记录
                string[] results = null;
                lRet = info.LibraryChannel.GetReaderInfo(
                    AddOI(strPatronIdentifier, strInstitution),
                    // strPatronIdentifier, //读者卡号,
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

                // 2021/3/4
                response.AO_InstitutionId_r = DomUtil.GetElementText(dom.DocumentElement, "oi");

                decimal feeAmount = 0;
                try
                {
                    feeAmount = Convert.ToDecimal(request.BV_FeeAmount_r);
                }
                catch (Exception ex)
                {
                    strError = $"请求的金额字符串 '{request.BV_FeeAmount_r}' 不合法: {ex.Message}";
                    goto ERROR1;
                }

                // 交费项
                List<AmerceItem> amerce_itemList = new List<AmerceItem>();
                XmlNodeList overdues = dom.DocumentElement.SelectNodes("overdues/overdue");
                if (overdues == null || overdues.Count == 0)
                {
                    strError = "交费失败: 当前读者没有任何待交费事项";
                    goto ERROR1;
                }

                List<string> prices = new List<string>();
                foreach (XmlNode node in overdues)
                {
                    string strID = DomUtil.GetAttr(node, "id");

                    // 2022/5/25
                    if (ids != null)
                    {
                        if (ids.IndexOf(strID) == -1)
                            continue;
                        ids.Remove(strID);
                    }

                    string price = DomUtil.GetAttr(node, "price");
                    prices.Add(price);

                    AmerceItem amerceItem = new AmerceItem();
                    amerceItem.ID = strID;
                    // 注: 如果没有变化，NewPrice 和 NewComment 里面不要放东西，因为这样会导致额外需要权限 amercemodifyprice
                    //amerceItem.NewPrice = price;
                    //amerceItem.NewComment = "自助机交费";
                    amerce_itemList.Add(amerceItem);
                }

                if (ids != null && ids.Count > 0)
                {
                    strError = $"交费失败: 请求的下列 ID '{StringUtil.MakePathList(ids)}' 在读者记录中没有找到对应的交费事项";
                    goto ERROR1;
                }

                // 累计交费金额
                string totalPrice = PriceUtil.TotalPrice(prices);
                nRet = PriceUtil.ParseSinglePrice(totalPrice,
                    out CurrencyItem currItem,
                    out strError);
                if (nRet == -1)
                {
                    strMessage = $"解析金额字符串 '{totalPrice}' 时出错：{strError} ";
                    response.AF_ScreenMessage_o = strMessage;
                    response.AG_PrintLine_o = strMessage;
                    return response.ToText();
                }

                if (request.CurrencyType_3 != currItem.Prefix)
                {
                    strMessage = $"货币类型不一致: 事项 {currItem.Value} 的货币类型 '{currItem.Prefix}'，请求的货币类型 '{request.CurrencyType_3}'";
                    response.AF_ScreenMessage_o = strMessage;
                    response.AG_PrintLine_o = strMessage;
                    return response.ToText();
                }

                // 金额要与欠款总额保持一致
                if (feeAmount != currItem.Value)
                {
                    strMessage = $"请求的金额({feeAmount})与读者待交费总额({currItem.Value})不一致，无法交费";
                    response.AF_ScreenMessage_o = strMessage;
                    response.AG_PrintLine_o = strMessage;
                    return response.ToText();
                }

                // 对所有记录进行交费
                //AmerceItem[] amerce_items = new AmerceItem[amerce_itemList.Count];
                //amerce_itemList.CopyTo(amerce_items);
                var amerce_items = amerce_itemList.ToArray();

                AmerceItem[] failed_items = null;
                string patronXml = "";
                lRet = info.LibraryChannel.Amerce(
                   "amerce",
                   AddOI(strPatronIdentifier, strInstitution),
                   // strPatronIdentifier,
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
                AO_InstitutionId_r = "",    // "dp2Library",
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

                // 2021/3/3
                string strInstitution = request.AO_InstitutionId_r;

                if (!string.IsNullOrEmpty(strPassword))
                {
                    lRet = info.LibraryChannel.VerifyReaderPassword(
                        AddOI(strQueryBarcode, strInstitution),    //读者证条码号
                                                                   // strQueryBarcode,
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

                string query = AddOI(strQueryBarcode, strInstitution);

                string[] results = null;
                lRet = info.LibraryChannel.GetReaderInfo(
                    query,
                    // strQueryBarcode, //读者卡号,
                    "advancexml",   // this.RenderFormat, // "html",
                    out results,
                    out strError);
                if (lRet <= -1)
                {
                    strError = "查询读者('" + query + "')信息出错：" + strError;
                    goto ERROR1;
                }
                else if (lRet == 0)
                {
                    strError = "查无此证 '" + query + "'";
                    goto ERROR1;
                }
                else if (lRet > 1)
                {
                    strError = "证号重复 '" + query + "'";
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

                /*
summary
This allows the SC to request partial information only. This field usage is similar to the NISO defined PATRON STATUS field. A Y in any position indicates that detailed as well as summary information about the corresponding category of items can be sent in the response. A blank (code $20) in this position means that only summary information should be sent about the corresponding category of items. Only one category of items should be requested at a time, i.e. it would take 6 of these messages, each with a different position set to Y, to get all the detailed information about a patron’s items. All of the 6 responses, however, would contain the summary information
Position Definition
0 hold items 预约事项
1 overdue items 超期事项
2 charged items 在借事项
3 fine items 罚款事项，一般是超期罚款
4 recall items 召回事项
5 unavailable holds 未到的预约事项
6 fee items 费用事项
                * */
                string summary = request.Summary_10;

                int start = GetValue(request.BP_StartItem_o, 1);
                int end = GetValue(request.BQ_EndItem_o, 9999);

                // 2021/3/4
                string strResponsePatronInstitution = DomUtil.GetElementText(dom.DocumentElement, "oi");
                response.AO_InstitutionId_r = strResponsePatronInstitution;

                // 2021/3/9
                // 注：对于条码号列表，目前可以认为凡是没有带点的，都可以自动加上一个读者的机构代码前缀，因为目前 dp2library 只允许一个读者借阅他所属分馆的图书。
                // 而如果将来允许读者借阅外馆的图书，到时候查到的条码号必然是已经带上前缀的了，也不需要额外处理了

                // hold items count 4 - char, fixed-length required field -- 预约
                XmlNodeList holdItemNodes = dom.DocumentElement.SelectNodes("reservations/request");
                if (holdItemNodes != null)
                {
                    List<BarcodeItem> holdItems = new List<BarcodeItem>();
                    foreach (XmlNode node in holdItemNodes)
                    {
                        /*
                        // 2022/9/26
                        // 敏捷放弃。假如前端已经 Close TCP 连接，则服务器应该尽快停止高耗能操作
                        if (IsConnected(sip_channel) == false)
                        {
                            strError = "前端已经切断 TCP 连接";
                            goto ERROR1;
                        }
                        */

                        string strItemBarcode = DomUtil.GetAttr(node, "items");
                        if (string.IsNullOrEmpty(strItemBarcode))
                            continue;

                        if (strItemBarcode.IndexOf(',') != -1)
                        {
                            string[] barcodes = strItemBarcode.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string barcode in barcodes)
                            {
                                // GetItemUII(info.LibraryChannel, barcode, null, out string uii, out strError);
                                // holdItems.Add(new VariableLengthField(SIPConst.F_AS_HoldItems, false, uii));
                                holdItems.Add(new BarcodeItem { Barcode = barcode });
                            }
                        }
                        else
                        {
                            // GetItemUII(info.LibraryChannel, strItemBarcode, null, out string uii, out strError);
                            // holdItems.Add(new VariableLengthField(SIPConst.F_AS_HoldItems, false, uii));
                            holdItems.Add(new BarcodeItem { Barcode = strItemBarcode });
                        }
                    }

                    response.HoldItemsCount_4 = holdItems.Count.ToString().PadLeft(4, '0');

                    if (IsDetail(summary, 0)
                        && holdItems.Count > 0)
                        response.AS_HoldItems_o = GetRange(holdItems,
                            (o) =>
                            {
                                // 2022/9/26
                                // 敏捷放弃。假如前端已经 Close TCP 连接，则服务器应该尽快停止高耗能操作
                                if (IsConnected(sip_channel) == false)
                                    throw new Exception($"前端已经切断 TCP 连接");

                                GetItemUII(info.LibraryChannel, o.Barcode, o.Location, out string uii, out strError);
                                return new VariableLengthField(SIPConst.F_AS_HoldItems, false, uii);
                            },
                            start, end);
                }

                if (request.AC_TerminalPassword_o == "!testing")
                {
                    // testing
                    while (true)
                    {
                        // 2022/9/26
                        // 敏捷放弃。假如前端已经 Close TCP 连接，则服务器应该尽快停止高耗能操作
                        if (IsConnected(sip_channel) == false)
                        {
                            strError = "前端已经切断 TCP 连接";
                            goto ERROR1;
                        }
                    }
                }

                // overdue items count 4 - char, fixed-length required field  -- 超期
                // charged items count 4 - char, fixed-length required field -- 在借
                XmlNodeList chargedItemNodes = dom.DocumentElement.SelectNodes("borrows/borrow");
                if (chargedItemNodes != null)
                {
                    List<BarcodeItem> chargedItems = new List<BarcodeItem>();
                    List<BarcodeItem> overdueItems = new List<BarcodeItem>();
                    int nOverdueItemsCount = 0;
                    foreach (XmlElement node in chargedItemNodes)
                    {
                        /*
                        // 2022/9/26
                        // 敏捷放弃。假如前端已经 Close TCP 连接，则服务器应该尽快停止高耗能操作
                        if (IsConnected(sip_channel) == false)
                        {
                            strError = "前端已经切断 TCP 连接";
                            goto ERROR1;
                        }
                        */

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

                        string location = node.GetAttribute("location");

                        {
                            // GetItemUII(info.LibraryChannel, strItemBarcode, location, out string uii, out strError);
                            // chargedItems.Add(new VariableLengthField(SIPConst.F_AU_ChargedItems, false, uii));
                            chargedItems.Add(new BarcodeItem { Barcode = strItemBarcode, Location = location });
                        }

                        string strReturningDate = DomUtil.GetAttr(node, "returningDate");
                        if (string.IsNullOrEmpty(strReturningDate))
                            continue;
                        DateTime returningDate = DateTimeUtil.FromRfc1123DateTimeString(strReturningDate);
                        if (returningDate < DateTime.Now)
                        {
                            nOverdueItemsCount++;
                            // GetItemUII(info.LibraryChannel, strItemBarcode, location, out string uii, out strError);
                            // overdueItems.Add(new VariableLengthField(SIPConst.F_AT_OverdueItems, false, uii));
                            overdueItems.Add(new BarcodeItem { Barcode = strItemBarcode, Location = location });
                        }
                    }

                    response.ChargedItemsCount_4 = chargedItems.Count.ToString().PadLeft(4, '0');

                    if (IsDetail(summary, 2)
                        && chargedItems.Count > 0)
                        response.AU_ChargedItems_o = GetRange(chargedItems,
                            (o) =>
                            {
                                // 2022/9/26
                                // 敏捷放弃。假如前端已经 Close TCP 连接，则服务器应该尽快停止高耗能操作
                                if (IsConnected(sip_channel) == false)
                                    throw new Exception($"前端已经切断 TCP 连接");

                                GetItemUII(info.LibraryChannel, o.Barcode, o.Location, out string uii, out strError);
                                return new VariableLengthField(SIPConst.F_AU_ChargedItems, false, uii);
                            },
                            start, end);

                    response.OverdueItemsCount_4 = overdueItems.Count.ToString().PadLeft(4, '0');

                    if (overdueItems.Count > 0)
                        patronStatus[6] = 'Y';  // too many items overdue 只要有一册以上超期未还，就禁止该读者继续借书

                    if (IsDetail(summary, 1)
                        && overdueItems.Count > 0)
                    {
                        // TODO: 添加 oi. 部分是否可以延迟到 GetRange() 之后进行
                        response.AT_OverdueItems_o = GetRange(overdueItems,
                            (o) =>
                            {
                                // 2022/9/26
                                // 敏捷放弃。假如前端已经 Close TCP 连接，则服务器应该尽快停止高耗能操作
                                if (IsConnected(sip_channel) == false)
                                    throw new Exception($"前端已经切断 TCP 连接");

                                GetItemUII(info.LibraryChannel, o.Barcode, o.Location, out string uii, out strError);
                                return new VariableLengthField(SIPConst.F_AT_OverdueItems, false, uii);
                            },
                            start, end);
                    }
                }

                // 超期交费项
                XmlNodeList overdues = dom.DocumentElement.SelectNodes("overdues/overdue");
                if (overdues != null && overdues.Count > 0)
                {
                    List<BarcodeItem> fineItems = new List<BarcodeItem>();

                    List<string> prices = new List<string>();

                    string strWords = "押金,租金";
                    string strWords2 = "超期,丢失";
                    foreach (XmlElement node in overdues)
                    {
                        /*
                        // 2022/9/26
                        // 敏捷放弃。假如前端已经 Close TCP 连接，则服务器应该尽快停止高耗能操作
                        if (IsConnected(sip_channel) == false)
                        {
                            strError = "前端已经切断 TCP 连接";
                            goto ERROR1;
                        }
                        */

                        string id = node.GetAttribute("id");
                        string strReason = DomUtil.GetAttr(node, "reason");
                        string strPart = "";
                        if (strReason.Length > 2)
                            strPart = strReason.Substring(0, 2);
                        if (StringUtil.IsInList(strPart, strWords) /*&& patronStatus[11] != 'Y'*/)
                        {
                            patronStatus[11] = 'Y'; // excessive outstanding fees
                        }
                        else if (StringUtil.IsInList(strPart, strWords2) /*&& patronStatus[10] != 'Y'*/)
                        {
                            patronStatus[10] = 'Y'; // excessive outstanding fines
                        }

                        // 计算金额
                        string price = DomUtil.GetAttr(node, "price");
                        prices.Add(price);

                        // fineItems.Add(new VariableLengthField(SIPConst.F_AV_FineItems, false, $"{id}:{price}"));
                        fineItems.Add(new BarcodeItem { Barcode = $"{id}:{price}" });
                    }

                    // 累计欠款金额
                    string totlePrice = PriceUtil.TotalPrice(prices);
                    nRet = PriceUtil.ParseSinglePrice(totlePrice,
                        out CurrencyItem currItem, out strError);
                    if (nRet == -1)
                    {
                        strMessage = "计算读者违约金额时出错：" + strError;
                        response.AF_ScreenMessage_o = strMessage;
                        response.AG_PrintLine_o = strMessage;
                        return response.ToText();
                    }
                    response.BV_feeAmount_o = "-" + currItem.Value.ToString(); //设为负值
                    response.BH_CurrencyType_3 = currItem.Prefix;

                    response.FineItemsCount_4 = fineItems.Count.ToString().PadLeft(4, '0');

                    // 2022/5/26
                    if (IsDetail(summary, 3)
                        && fineItems.Count > 0)
                        response.AV_FineItems_o = GetRange(fineItems,
                            (o) =>
                            {
                                return new VariableLengthField(SIPConst.F_AV_FineItems, false, o.Barcode);
                            },
                            start, end);
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
                        {
                            patronStatus[5] = 'Y';  // too many items charged
                            strMessage = "您在本馆借书数已达最多可借数【" + strTotal + "】，不能继续借了!";
                        }
                    }
                    if (!string.IsNullOrEmpty(strMessage))
                    {
                        response.AF_ScreenMessage_o = strMessage;
                        response.AG_PrintLine_o = strMessage;
                    }
                }
                else
                {
                    patronStatus[0] = 'Y';  // charge privileges denied
                    patronStatus[1] = 'Y';  // renewal privileges denied
                    patronStatus[3] = 'Y';  // hold privileges denied

                    if (StringUtil.IsInList("挂失,丢失", strState))
                        patronStatus[4] = 'Y';  // card reported lost

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

        // https://social.msdn.microsoft.com/Forums/en-US/c857cad5-2eb6-4b6c-b0b5-7f4ce320c5cd/c-how-to-determine-if-a-tcpclient-has-been-disconnected?forum=netfxnetcom#:~:text=%2F%2F%20Detect%20if%20client%20disconnected%20if%20%28tcp.Client.Poll%20%280%2C,Client%20disconnected%20bClosed%20%3D%20true%20%3B%20%7D%20%7D
        static bool IsConnected(SipChannel channel)
        {
            var tcpClient = channel.TcpClient;
            if (tcpClient.Connected)
            {
                return !tcpClient.Client.Poll(0, SelectMode.SelectError) ? true : false;
            }

            return false;
        }

        // 判断一个位置是否为 'Y'
        static bool IsDetail(string summary, int index)
        {
            if (string.IsNullOrEmpty(summary))
                return false;
            if (index >= summary.Length)
                return false;
            return summary[index] == 'Y';
        }

        static int GetValue(string text, int default_value)
        {
            if (string.IsNullOrEmpty(text))
                return default_value;
            return Convert.ToInt32(text);
        }

        static int _rangeLimit = 100;

        class BarcodeItem
        {
            public string Location { get; set; }
            public string Barcode { get; set; }
        }

        delegate VariableLengthField delegate_createItem(BarcodeItem item);

        static List<VariableLengthField> GetRange(List<BarcodeItem> list,
            delegate_createItem proc,
    int start,
    int end)
        {
            if (start > end)
                throw new ArgumentException($"GetRange() 的 start({start}) 不应大于 end({end})");
            List<VariableLengthField> results = new List<VariableLengthField>();
            int i = 1;
            foreach (var item in list)
            {
                if (i >= start && i <= end)
                {
                    results.Add(proc(item));
                    // 限制集合元素总数
                    if (results.Count >= _rangeLimit)
                        break;
                }
                i++;
                // 优化
                if (i > end)
                    break;
            }

            return results;
        }

#if OLD
        static List<VariableLengthField> GetRange(List<VariableLengthField> list,
            int start,
            int end)
        {
            if (start > end)
                throw new ArgumentException($"GetRange() 的 start({start}) 不应大于 end({end})");
            List<VariableLengthField> results = new List<VariableLengthField>();
            int i = 1;
            foreach (var item in list)
            {
                if (i >= start && i <= end)
                {
                    results.Add(item);
                    // 限制集合元素总数
                    if (results.Count >= _rangeLimit)
                        break;
                }
                i++;
                // 优化
                if (i > end)
                    break;
            }

            return results;
        }
#endif

        // 根据纯净的册条码号获得一个册的 UII。UII 就是 OI.PII
        // parameters:
        //      location    馆藏地。用于加速获得机构代码的运算
        static int GetItemUII(LibraryChannel channel,
            string barcode,
            string location,
            out string uii,
            out string strError)
        {
            strError = "";
            uii = barcode;

            if (uii.Contains("."))
                return 0;

            string strClientXml = "";
            // 前端模拟出一条册记录。这样请求速度快，dp2library 不用再检索册记录了
            if (string.IsNullOrEmpty(location) == false)
            {
                XmlDocument client_dom = new XmlDocument();
                client_dom.LoadXml("<root />");
                DomUtil.SetElementText(client_dom.DocumentElement,
                    "parent",
                    "[none]");
                DomUtil.SetElementText(client_dom.DocumentElement,
                    "barcode", barcode);
                DomUtil.SetElementText(client_dom.DocumentElement,
                    "location", location);
                strClientXml = client_dom.DocumentElement.OuterXml;
            }

            int nRedoCount = 0;
        REDO:
            long lRet = channel.GetItemInfo(
    "item",
    barcode,
    strClientXml,
    "uii",
    out string strItemXml,
    out _,
    out _,
    "",
    out string strBiblio,
    out _,
    out strError);
            /*
            long lRet = channel.GetItemInfo(
                barcode,
                "uii",
                out string strItemXml,
                "",
                out string strBiblio,
                out strError);
            */
            if (lRet == -1)
            {
                if (channel.ErrorCode == ErrorCode.ChannelReleased
                    && nRedoCount < 10)
                {
                    nRedoCount++;
                    goto REDO;
                }
                return -1;
            }

            if (lRet == 0)
                return 0;

            if (string.IsNullOrEmpty(strItemXml))
                uii = barcode;
            else
                uii = strItemXml;
            return 1;
        }

        // 2021/3/9
        // 给条码号加上机构代码前缀
        static string AddOI(string barcode, string oi)
        {
            if (barcode != null && barcode.Contains("."))
                return barcode;
            if (string.IsNullOrEmpty(oi))
                return barcode;
            return oi + "." + barcode;
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

            // TODO: 要从请求中得到 AO
            string strInstitution = "";

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
                        case "AO":
                            // 2021/3/4
                            strInstitution = strValue;
                            break;
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
                    strMsg = $"办证失败！按证号查找读者记录发生错误: {strError}";
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
                // TODO: OI
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
                        strMsg = $"办证失败！按身份证号查找读者记录发生错误: {strError}";
                        goto ERROR1;
                    case 0:
                        strRecPath = "";    // strRecPath = "读者/?";
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
                    strMsg = strAction == "new" ? $"办证失败！创建读者记录发生错误: {strError}" : $"修改读者信息发生错误: {strError}";
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
                            strSavedRecPath,
                            baNewTimestamp,
                            out string strError1);
                        if (lRet == -1)
                            strError = $"办证过程中交费发生错误({strError})。然后回滚失败: " + strError1;
                        else
                            strError = $"办证过程中交费发生错误({strError})。然后回滚成功";

                        strMsg = $"办证交费过程中创建交费请求失败({strError})，办证失败，请重新操作。";
                        goto ERROR1;
                    }


                    int nRet = DoAmerce(
                        info.LibraryChannel,
                        strReaderBarcode,
                        strInstitution,
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
                    strMsg = $"修改密码过程中发生错误({strError})，请稍后再试。";
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
            string strInstitution,
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
                AddOI(strReaderBarcode, strInstitution),
                // strReaderBarcode,
                "xml",
                out results,
                out strRecPath,
                out baTimestamp,
                out strError);
            if (lRet == -1)
            {
                strMsg = $"办证交押金时获得读者记录发生错误({strError})，办证失败，请重新操作。";
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
                    AddOI(strReaderBarcode, strInstitution),
                    // strReaderBarcode,
                    amerce_items,
                    out failed_items,
                    out strReaderXml,
                    out strError);
                if (lRet == -1 && lRet == 1)
                {
                    strMsg = $"办证时收押金失败({strError})，办证失败，请到柜台办理。";
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
            lRet = DeleteReader(library_channel,
                strRecPath,
                baTimestamp,
                out string strError1);
            if (lRet == -1)
                strError = $"办证过程中交费发生错误({strMsg})。然后回滚失败: " + strError1;
            else
                strError = $"办证过程中交费发生错误({strMsg})。然后回滚成功";
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
                AO_InstitutionId_r = "?",    // "dp2Library",
                AM_LibraryName_o = "dp2Library",
                BX_SupportedMessages_r = "YYYYYYYYYYYYYYYY",
                AF_ScreenMessage_o = "",
            };

            string strError = "";

            FunctionInfo info = BeginFunction(sip_channel,
                false,
                true,
                true);  // 使用 capo 账户
            if (string.IsNullOrEmpty(info.ErrorInfo) == false)
            {
                strError = info.ErrorInfo;
                goto ERROR1;
            }

            try
            {
                // TODO: 要检查相关 dp2Capo 实例是否在线

                long lRet = -1;

                // 确保获得 dp2library 版本号
                if (EnsureGetVersion(info.LibraryChannel, out strError) == -1)
                    goto ERROR1;

                if (StringUtil.CompareVersion(_libraryServerVersion, _dp2library_base_version) < 0)
                {
                    strError = $"dp2Capo 要求和 dp2Library {_dp2library_base_version} 或以上版本配套使用 (而当前 dp2Library 版本号为 {_libraryServerVersion}。请尽快升级 dp2Library 到最新版本";
                    goto ERROR1;
                }

                // TODO: 如果某个通道的 ScStatus 请求来得很频繁，要考虑缓冲 GetSystemParameter() API 的结果，直接把这个结果返回给前端
                string strExistingHangupValue = null;

                {
                    lock (_syncRoot_hangupStatusTable)
                    {
                        if (_hangupStatusTable != null
                            && _hangupStatusTable.ContainsKey(info.LibraryChannel.Url))
                            strExistingHangupValue = _hangupStatusTable[info.LibraryChannel.Url];
                    }

                    if (strExistingHangupValue == null)
                    {
                        //2018/06/19 
                        lRet = info.LibraryChannel.GetSystemParameter(
                        "system",
                        "hangup",
                        out strExistingHangupValue,
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
                                if (_hangupStatusTable != null && _hangupStatusTable.Count < 1000)
                                    _hangupStatusTable[info.LibraryChannel.Url] = strExistingHangupValue;
                            }
                        }
                    }
                }

                string strExistingLibraryName = null;
                {
                    lock (_syncRoot_hangupStatusTable)
                    {
                        string key = info.LibraryChannel.Url + "_libraryName";
                        if (_hangupStatusTable != null
                            && _hangupStatusTable.ContainsKey(key))
                            strExistingLibraryName = _hangupStatusTable[key];
                    }

                    if (strExistingLibraryName == null)
                    {
                        // 获得图书馆名字
                        lRet = info.LibraryChannel.GetSystemParameter(
                            "library",
                            "name",
                            out strExistingLibraryName,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "GetSystemParameter('library', 'name') error: " + strError;
                            goto ERROR1;
                        }

                        // 缓存起来
                        if (lRet == 1)
                        {
                            lock (_syncRoot_hangupStatusTable)
                            {
                                string key = info.LibraryChannel.Url + "_libraryName";
                                if (_hangupStatusTable != null && _hangupStatusTable.Count < 1000)
                                    _hangupStatusTable[key] = strExistingLibraryName;
                            }
                        }
                    }
                }

                var is_hangup = string.IsNullOrEmpty(strExistingHangupValue) == false;

                response = new ACSStatus_98()
                {
                    OnlineStatus_1 = "Y",   // see instance info
                    CheckinOk_1 = (is_hangup ? "N" : "Y"), // && string.IsNullOrEmpty(sip_channel.UserName) 
                    CheckoutOk_1 = (is_hangup ? "N" : "Y"),
                    ACSRenewalPolicy_1 = (is_hangup ? "N" : "Y"),
                    StatusUpdateOk_1 = "N", // "Y",
                    OfflineOk_1 = "N",   // "Y",
                    TimeoutPeriod_3 = "010",
                    RetriesAllowed_3 = "003",
                    DatetimeSync_18 = SIPUtility.NowDateTime,
                    ProtocolVersion_4 = "2.00",
                    // TODO: 如果当前已经登录，这里尽量返回登录者所属分馆的机构代码
                    AO_InstitutionId_r = sip_channel.Institution == null ? "" : sip_channel.Institution,    // "dp2Library",
                    AM_LibraryName_o = strExistingLibraryName,  // "dp2Library",
                    BX_SupportedMessages_r = "YYYYYYYYYYYYYYYY",
                    AF_ScreenMessage_o = (is_hangup ? strExistingHangupValue : ""),
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

        /// <summary>
        /// 列出通道信息
        /// </summary>
        /// <param name="sip_channel">SIP 通道</param>
        /// <param name="message">SIP 请求消息</param>
        /// <returns></returns>
        static string ListChannel(SipChannel sip_channel, string message)
        {
            string strError = "";
            int nRet = 0;
            long lRet = 0;

            ChannelInformationResponse_42 response = new ChannelInformationResponse_42()
            {
                Status_1 = "N",
                TransactionDate_18 = SIPUtility.NowDateTime,
                ZT_TotalCount_r = "0",
                ZV_Value_r = "",
                ZR_ReturnCount_r = "0",
            };

            ChannelInformation_41 request = new ChannelInformation_41();
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
                if (StringUtil.IsInList("isManager", sip_channel.Style) == false)
                {
                    if (sip_channel.Encoding == null)
                        strError = $"Current user is not SIP server manager, can't use ListChannel function";
                    else
                        strError = $"当前用户不具备 SIP Server 管理者权限，无法使用 ListChannel 功能";
                    goto ERROR1;
                }

                string query_word = request.ZW_SearchWord_r;
                long offset = 0;
                if (string.IsNullOrEmpty(request.BP_StartItem_r) == false
                    && long.TryParse(request.BP_StartItem_r, out offset) == false)
                {
                    strError = $"StartItem 参数值 '{request.BP_StartItem_r}' 不合法";
                    goto ERROR1;
                }

                long max_count = -1;
                if (string.IsNullOrEmpty(request.ZC_MaxCount_r) == false
    && long.TryParse(request.ZC_MaxCount_r, out max_count) == false)
                {
                    strError = $"MaxCount 参数值 '{request.ZC_MaxCount_r}' 不合法";
                    goto ERROR1;
                }

                string format = request.ZF_format_r;
                if (string.IsNullOrEmpty(format))
                    format = "json";

                string id = sip_channel.GetHashCode().ToString();
                List<SipChannelInfo> infos = null;
                if (offset == 0)
                {

                }
                else
                {
                    infos = _channelResults.GetResult(id)?.Infos;
                }

                if (infos == null)
                {
                    infos = SearchChannel(query_word);
                    _channelResults.PutResult(id,
                        infos);
                }

                var outputs = new List<SipChannelInfo>();
                for (int i = (int)offset; i < infos.Count; i++)
                {
                    if (max_count != -1 && outputs.Count >= max_count)
                        break;
                    // 最多 100 行
                    if (outputs.Count >= 100)
                        break;
                    outputs.Add(infos[i]);
                }

                response.Status_1 = "Y";
                response.ZT_TotalCount_r = infos.Count.ToString();
                response.ZV_Value_r = SipChannelResults.ToString(outputs, format);
                response.ZR_ReturnCount_r = outputs.Count.ToString();
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
            LibraryManager.Log?.Info("ListChannel() error: " + strError);
            response.AF_ScreenMessage_o = strError;
            response.AG_PrintLine_o = strError;
            return response.ToText();
        }

        // 通道检索结果集合
        internal static SipChannelResultsManager _channelResults = new SipChannelResultsManager();

        // 检索获得通道信息集合
        static List<SipChannelInfo> SearchChannel(string query_word)
        {
            List<SipChannel> results = new List<SipChannel>();
            ServerInfo.SipServer?._tcpChannels?.Clean((channel) =>
            {
                SipChannel sip_channel = channel as SipChannel;
                results.Add(sip_channel);
                return false;
            });
            return SipChannelResults.ToInfo(results);
        }

        // 确保获得所连接的 dp2library 服务器的版本号
        static int EnsureGetVersion(LibraryChannel channel,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(_libraryServerVersion) == false)
                return 0;

            long lRet = channel.GetVersion(
out string version,
out string uid,
out strError);
            if (lRet == -1)
            {
                strError = $"get dp2library version error: {strError}";
                return -1;
            }
            _libraryServerVersion = version;
            return 1;
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
